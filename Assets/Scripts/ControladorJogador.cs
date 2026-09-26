using System;
using UnityEngine;

/* ============================================================
 *  ControladorJogador.cs
 *  ---------------------------------------------------------------------
 *  Movimentação do agente de saúde em primeira pessoa. Substitui os
 *  antigos Mover.cs + Pular.cs + a parte de giro do MouseLook.cs.
 *
 *  Tudo acontece num ÚNICO Update, sempre na mesma ordem:
 *    1. olhar  (gira o corpo no yaw; o pitch só é guardado — quem inclina
 *               a câmera é o CameraJogador.cs)
 *    2. andar/correr  (aceleração e frenagem suaves, relativas ao yaw
 *               deste mesmo frame — sem o atraso de 1 frame de antes)
 *    3. pulo   (por ALTURA em metros, com coyote time + buffer de pulo)
 *    4. gravidade e um único CharacterController.Move()
 *
 *  Andar = WASD (animação Walk); correr = segurar Shift andando pra
 *  frente (animação Run). A entrada vem do EntradaJogador.cs.
 *
 *  O yaw inicial é lido da rotação que o player já tem na cena (antes
 *  o MouseLook começava sempre em 0 e "estalava" no primeiro frame).
 * ============================================================ */

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(EntradaJogador))]
public class ControladorJogador : MonoBehaviour
{
    [Header("Velocidade")]
    [Tooltip("Velocidade andando (m/s). Casada com a passada da animação Walk do agente.")]
    [SerializeField] private float velocidadeAndar = 2.8f;

    [Tooltip("Velocidade correndo com Shift (m/s). Casada com a passada da animação Run.")]
    [SerializeField] private float velocidadeCorrer = 5.8f;

    [Tooltip("Quão rápido chega na velocidade desejada no chão (m/s²).")]
    [SerializeField] private float aceleracao = 22f;

    [Tooltip("Quão rápido para ao soltar as teclas no chão (m/s²).")]
    [SerializeField] private float desaceleracao = 28f;

    [Tooltip("Controle no ar (0 = nenhum, 1 = igual ao chão).")]
    [Range(0f, 1f)]
    [SerializeField] private float controleNoAr = 0.35f;

    [Header("Pulo e gravidade")]
    [Tooltip("Altura do pulo em metros.")]
    [SerializeField] private float alturaPulo = 1.1f;

    [Tooltip("Gravidade (valor negativo).")]
    [SerializeField] private float gravidade = -20f;

    [Tooltip("Ainda dá pra pular por este tempo depois de sair de uma beirada (segundos).")]
    [SerializeField] private float coyoteTime = 0.12f;

    [Header("Olhar")]
    [SerializeField] private float pitchMinimo = -80f;
    [SerializeField] private float pitchMaximo = 80f;

    private CharacterController controller;
    private EntradaJogador entrada;
    private Vector3 velocidadeHorizontal;
    private float velocidadeVertical;
    private float ultimoTempoNoChao = float.NegativeInfinity;
    private bool estavaNoChao;
    private bool controleAtivo = true;

    /* Disparados pra câmera/sons: pulo e aterrissagem (com a velocidade de queda em m/s). */
    public event Action OnPulou;
    public event Action<float> OnAterrissou;

    public float Yaw { get; private set; }
    public float Pitch { get; private set; }
    public float VelocidadeHorizontal => velocidadeHorizontal.magnitude;
    public float VelocidadeVertical => velocidadeVertical;
    public float VelocidadeAndar => velocidadeAndar;
    public float VelocidadeCorrer => velocidadeCorrer;
    public bool NoChao { get; private set; }
    public bool Correndo { get; private set; }
    public float TempoNoAr => NoChao ? 0f : Time.time - ultimoTempoNoChao;

    /* Desligado = sem andar/olhar/pular (a gravidade continua valendo). Usado no fim de jogo. */
    public bool ControleAtivo
    {
        get => controleAtivo;
        set
        {
            controleAtivo = value;
            if (entrada != null)
                entrada.Habilitado = value;
        }
    }

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        entrada = GetComponent<EntradaJogador>();
        Yaw = transform.eulerAngles.y;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        float dt = Time.deltaTime;

        // --- 1. Olhar ---
        Vector2 olhar = controleAtivo ? entrada.Olhar : Vector2.zero;
        Yaw = Mathf.Repeat(Yaw + olhar.x, 360f);
        Pitch = Mathf.Clamp(Pitch - olhar.y, pitchMinimo, pitchMaximo);
        transform.rotation = Quaternion.Euler(0f, Yaw, 0f);

        // --- 2. Andar / correr ---
        NoChao = controller.isGrounded;
        if (NoChao)
            ultimoTempoNoChao = Time.time;

        Vector2 pedido = controleAtivo ? entrada.Movimento : Vector2.zero;
        Correndo = entrada.CorrerSegurado && pedido.y > 0.1f && controleAtivo;
        float velMax = Correndo ? velocidadeCorrer : velocidadeAndar;

        Vector3 frente = transform.forward;
        Vector3 direita = transform.right;
        Vector3 alvo = (frente * pedido.y + direita * pedido.x) * velMax;

        float taxa = alvo.sqrMagnitude > 0.0001f ? aceleracao : desaceleracao;
        if (!NoChao)
            taxa *= controleNoAr;
        velocidadeHorizontal = Vector3.MoveTowards(velocidadeHorizontal, alvo, taxa * dt);
        if (velocidadeHorizontal.sqrMagnitude < 0.01f * 0.01f)
            Correndo = false;

        // --- 3. Pulo (coyote time + buffer) ---
        bool podePular = NoChao || (Time.time - ultimoTempoNoChao <= coyoteTime && velocidadeVertical <= 0f);
        if (controleAtivo && podePular && entrada.PuloNoBuffer)
        {
            entrada.ConsumirPulo();
            velocidadeVertical = Mathf.Sqrt(2f * alturaPulo * -gravidade);
            ultimoTempoNoChao = float.NegativeInfinity; // não pula 2x no mesmo coyote
            OnPulou?.Invoke();
        }
        else if (NoChao && velocidadeVertical < 0f)
        {
            velocidadeVertical = -2f; // mantém colado no chão em rampas e degraus
        }

        // --- 4. Gravidade + um único Move ---
        // Anda com a velocidade MÉDIA do frame (não a do fim): a altura do pulo
        // sai exata em qualquer FPS, mesmo num frame mais lento.
        float verticalAntes = velocidadeVertical;
        velocidadeVertical += gravidade * dt;
        float verticalMedia = 0.5f * (verticalAntes + velocidadeVertical);
        float quedaAntes = -velocidadeVertical;

        CollisionFlags batidas = controller.Move((velocidadeHorizontal + Vector3.up * verticalMedia) * dt);

        if ((batidas & CollisionFlags.Above) != 0 && velocidadeVertical > 0f)
            velocidadeVertical = 0f;

        if ((batidas & CollisionFlags.Sides) != 0)
        {
            // Bateu na parede: não acumula velocidade contra ela.
            Vector3 real = controller.velocity;
            real.y = 0f;
            if (real.sqrMagnitude < velocidadeHorizontal.sqrMagnitude)
                velocidadeHorizontal = real;
        }

        bool agoraNoChao = controller.isGrounded;
        if (agoraNoChao && !estavaNoChao && quedaAntes > 3f)
            OnAterrissou?.Invoke(quedaAntes);
        estavaNoChao = agoraNoChao;
    }

    /* Move o jogador sem brigar com o CharacterController (desliga, move, religa). */
    public void Teleportar(Vector3 posicao)
    {
        controller.enabled = false;
        transform.position = posicao;
        controller.enabled = true;
        velocidadeHorizontal = Vector3.zero;
        velocidadeVertical = 0f;
        Physics.SyncTransforms();
    }

    /* Aponta a visão pra uma direção (graus). Útil pra cutscenes/teleportes. */
    public void DefinirOlhar(float yaw, float pitch)
    {
        Yaw = yaw;
        Pitch = Mathf.Clamp(pitch, pitchMinimo, pitchMaximo);
        transform.rotation = Quaternion.Euler(0f, Yaw, 0f);
    }
}
