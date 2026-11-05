using UnityEngine;

/* ============================================================
 *  CameraJogador.cs
 *  ---------------------------------------------------------------------
 *  Fica na Main Camera. Câmera em primeira pessoa do agente de saúde,
 *  substituindo a parte de câmera do antigo MouseLook.cs.
 *
 *  - A câmera gira em torno do PESCOÇO (pivô), não do meio da cabeça:
 *    ao olhar pra baixo os olhos vão pra frente e pra baixo, como uma
 *    cabeça de verdade — assim dá pra ver o próprio colete e os pés sem
 *    a câmera entrar no peito.
 *  - Balanço ao andar/correr (head bob) sincronizado com a DISTÂNCIA
 *    percorrida, com o mesmo comprimento de passo das animações Walk/Run
 *    do modelo — o balanço e os passos do corpo batem. Exceção: no
 *    ModoPesado o passo × 1,25 é de propósito (passadas mais lentas e
 *    arrastadas), e o balanço e o som dos passos, que seguem a FasePasso,
 *    deixam de bater com as pernas enquanto carrega.
 *  - "Afundada" amortecida ao aterrissar de um pulo/queda.
 *  - FOV abre um pouco na corrida.
 *  - ModoPesado (carregando pneu, FeedbackDeCarga): balanço mais forte,
 *    passo mais longo e mais inclinação lateral — o corpo sente o peso.
 *    Entra e sai suave (PesoPesado, de 0 a 1), como o balanço.
 *
 *  Roda depois de tudo (LateUpdate + DefaultExecutionOrder alto): o
 *  ControladorJogador já moveu o corpo neste frame.
 * ============================================================ */

[DefaultExecutionOrder(100)]
[RequireComponent(typeof(Camera))]
public class CameraJogador : MonoBehaviour
{
    [Tooltip("Jogador que a câmera acompanha. Vazio = procura o ControladorJogador da cena.")]
    [SerializeField] private ControladorJogador jogador;

    [Header("Cabeça")]
    [Tooltip("Altura do pivô do pescoço, a partir dos pés (m). Medida no modelo pelo PlayerBuilder.")]
    [SerializeField] private float alturaPivo = 1.5f;

    [Tooltip("Posição dos olhos em relação ao pivô, olhando reto: x = pra frente, y = pra cima (m).")]
    [SerializeField] private Vector2 olhosNoPivo = new Vector2(0.10f, 0.13f);

    [Header("Balanço ao andar")]
    [SerializeField] private float amplitudeAndar = 0.022f;
    [SerializeField] private float amplitudeCorrer = 0.045f;
    [Tooltip("Comprimento de um passo andando/correndo (m) — o mesmo das animações do modelo.")]
    [SerializeField] private Vector2 comprimentoPasso = new Vector2(0.75f, 1.45f);
    [Tooltip("Inclinação lateral máxima no balanço (graus).")]
    [SerializeField] private float rolagemBalanco = 0.6f;

    [Header("Modo pesado (carregando pneu)")]
    [Tooltip("Multiplica a amplitude do balanço no ModoPesado.")]
    [SerializeField] private float multiplicadorAmplitudePesado = 1.6f;
    [Tooltip("Multiplica o comprimento do passo no ModoPesado (passos mais lentos e arrastados).")]
    [SerializeField] private float multiplicadorPassoPesado = 1.25f;
    [Tooltip("Graus somados à inclinação lateral máxima no ModoPesado.")]
    [SerializeField] private float rolagemExtraPesado = 1.5f;

    [Header("Aterrissagem")]
    [SerializeField] private float rigidezMola = 110f;
    [SerializeField] private float amortecimentoMola = 13f;
    [Tooltip("Quanto de velocidade de queda vira afundada (m/s de câmera por m/s de queda).")]
    [SerializeField] private float forcaAterrissagem = 0.07f;

    [Header("FOV")]
    [SerializeField] private float fovExtraCorrendo = 6f;
    [SerializeField] private float velocidadeFov = 6f;

    [Header("Recorte")]
    [SerializeField] private float nearClip = 0.05f;

    private Camera cam;
    private float fovBase;

    /* FOV parado/andando (a corrida soma fovExtraCorrendo). Muda na hora,
     * inclusive com o jogo pausado — é o slider do menu de pausa. */
    public float FovBase
    {
        get => fovBase;
        set
        {
            fovBase = Mathf.Clamp(value, 1f, 179f);
            if (cam != null)
                cam.fieldOfView = fovBase;
        }
    }

    /* Desligado = sem head bob nem inclinação ao andar (Configurações). */
    public bool BalancoAtivo { get; set; } = true;

    /* Carregando pneu (FeedbackDeCarga): balanço × multiplicadorAmplitudePesado,
     * passo × multiplicadorPassoPesado e inclinação + rolagemExtraPesado. */
    public bool ModoPesado { get; set; }

    /* Quanto do ModoPesado já está valendo, de 0 a 1 (AtualizarPesoPesado). */
    public float PesoPesado => pesoPesado;

    /* Fase do ciclo de passos (rad): cada π é um passo, e o pé bate no chão
     * em π/2 + kπ (câmera no ponto mais baixo). Avança mesmo sem balanço. */
    public float FasePasso => fasePasso;
    private float fasePasso;
    private float pesoBalanco;
    private float pesoPesado;
    private float afundada;
    private float velocidadeAfundada;

    void Awake()
    {
        cam = GetComponent<Camera>();
        fovBase = cam.fieldOfView;
        cam.nearClipPlane = nearClip;
        if (jogador == null)
            jogador = FindFirstObjectByType<ControladorJogador>();
    }

    void OnEnable()
    {
        if (jogador != null)
            jogador.OnAterrissou += Aterrissou;
    }

    void OnDisable()
    {
        if (jogador != null)
            jogador.OnAterrissou -= Aterrissou;
    }

    private void Aterrissou(float velocidadeQueda)
    {
        velocidadeAfundada -= Mathf.Min(velocidadeQueda * forcaAterrissagem, 0.9f);
    }

    /* Leva o PesoPesado até o ModoPesado (1 ligado, 0 desligado), em 1/4 s, como o pesoBalanco. */
    public void AtualizarPesoPesado(float dt)
    {
        pesoPesado = Mathf.MoveTowards(pesoPesado, ModoPesado ? 1f : 0f, dt * 4f);
    }

    void LateUpdate()
    {
        if (jogador == null)
            return;

        float dt = Time.deltaTime;
        float vel = jogador.VelocidadeHorizontal;
        float tCorrida = Mathf.InverseLerp(jogador.VelocidadeAndar, jogador.VelocidadeCorrer, vel);
        AtualizarPesoPesado(dt);

        // --- balanço: fase avança com a distância andada ---
        float passo = Mathf.Lerp(comprimentoPasso.x, comprimentoPasso.y, tCorrida)
                      * Mathf.Lerp(1f, multiplicadorPassoPesado, pesoPesado);
        fasePasso += vel * dt / Mathf.Max(passo, 0.01f) * Mathf.PI;
        float pesoAlvo = jogador.NoChao && BalancoAtivo ? Mathf.Clamp01(vel / Mathf.Max(jogador.VelocidadeAndar, 0.01f)) : 0f;
        pesoBalanco = Mathf.MoveTowards(pesoBalanco, pesoAlvo, dt * 4f);
        float amplitude = Mathf.Lerp(amplitudeAndar, amplitudeCorrer, tCorrida) * pesoBalanco
                          * Mathf.Lerp(1f, multiplicadorAmplitudePesado, pesoPesado);
        float rolagem = rolagemBalanco + rolagemExtraPesado * pesoPesado;

        float bobY = -Mathf.Abs(Mathf.Sin(fasePasso)) * amplitude;           // desce a cada passo
        float bobX = Mathf.Sin(fasePasso) * amplitude * 0.5f;                // balança de lado a cada 2 passos
        float roll = Mathf.Sin(fasePasso) * rolagem * pesoBalanco;

        // --- afundada ao aterrissar (mola amortecida) ---
        float acel = -rigidezMola * afundada - amortecimentoMola * velocidadeAfundada;
        velocidadeAfundada += acel * dt;
        afundada += velocidadeAfundada * dt;

        // --- pose da cabeça: pivô no pescoço + olhos à frente/acima ---
        Quaternion rot = Quaternion.Euler(jogador.Pitch, jogador.Yaw, 0f);
        Vector3 pivo = jogador.transform.position + Vector3.up * alturaPivo;
        Vector3 olhos = pivo + rot * new Vector3(0f, olhosNoPivo.y, olhosNoPivo.x);

        transform.position = olhos
                             + (rot * Vector3.right) * bobX
                             + Vector3.up * (bobY + afundada);
        transform.rotation = rot * Quaternion.Euler(0f, 0f, roll);

        // --- FOV da corrida ---
        float fovAlvo = fovBase + (jogador.Correndo ? fovExtraCorrendo * Mathf.Max(tCorrida, 0.3f) : 0f);
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, fovAlvo, 1f - Mathf.Exp(-velocidadeFov * dt));
    }
}
