using UnityEngine;
using UnityEngine.InputSystem;

/* ============================================================
 *  EntradaJogador.cs
 *  ---------------------------------------------------------------------
 *  ÚNICO lugar que lê a entrada do jogador (andar, olhar, pular, correr,
 *  interagir). Usa as ações do asset Assets/InputSystem_Actions.inputactions
 *  (mapa "Player"), então teclado+mouse e controle (gamepad) funcionam
 *  sem nenhum código extra:
 *
 *    Move    WASD / setas       | analógico esquerdo
 *    Look    mouse              | analógico direito
 *    Jump    Espaço             | botão Sul (A / X)
 *    Sprint  Shift esquerdo     | apertar o analógico esquerdo
 *    Attack  clique esquerdo    | botão Oeste (X / Quadrado)
 *
 *  O ControladorJogador.cs e o Interactor.cs só consomem os valores
 *  prontos daqui. Roda antes deles (DefaultExecutionOrder) para que todos
 *  vejam a mesma entrada no mesmo frame.
 * ============================================================ */

[DefaultExecutionOrder(-100)]
public class EntradaJogador : MonoBehaviour
{
    [Tooltip("Asset de ações do Input System (mapa \"Player\"). Vazio = usa as ações do projeto (InputSystem.actions).")]
    [SerializeField] private InputActionAsset acoes;

    [Header("Sensibilidade")]
    [Tooltip("Mouse: graus girados por pixel movido.")]
    [SerializeField] private float sensibilidadeMouse = 0.15f;

    [Tooltip("Controle: graus por segundo com o analógico direito no máximo.")]
    [SerializeField] private float sensibilidadeControle = 170f;

    [Tooltip("Inverte o eixo vertical do olhar.")]
    [SerializeField] private bool inverterY;

    [Header("Pulo")]
    [Tooltip("Por quanto tempo um aperto de pulo \"fica valendo\" antes de tocar o chão (segundos).")]
    [SerializeField] private float bufferPulo = 0.12f;

    private InputAction acaoMover;
    private InputAction acaoOlhar;
    private InputAction acaoPular;
    private InputAction acaoCorrer;
    private InputAction acaoInteragir;
    private float ultimoApertoPulo = float.NegativeInfinity;

    /* Desligado = o jogador não anda, não olha e não interage (fim de jogo, menus). */
    public bool Habilitado { get; set; } = true;

    /* Direção pedida (x = lado, y = frente), magnitude até 1. */
    public Vector2 Movimento => Habilitado && acaoMover != null
        ? Vector2.ClampMagnitude(acaoMover.ReadValue<Vector2>(), 1f)
        : Vector2.zero;

    /* Quanto girar NESTE frame, em graus (x = yaw, y = pitch; + = pra cima). */
    public Vector2 Olhar { get; private set; }

    public bool CorrerSegurado => Habilitado && acaoCorrer != null && acaoCorrer.IsPressed();

    public bool InteragirPressionado => Habilitado && acaoInteragir != null && acaoInteragir.WasPressedThisFrame();

    /* Pulo apertado há pouco e ainda não usado (ver bufferPulo). */
    public bool PuloNoBuffer => Habilitado && Time.time - ultimoApertoPulo <= bufferPulo;

    public void ConsumirPulo() => ultimoApertoPulo = float.NegativeInfinity;

    public float SensibilidadeMouse
    {
        get => sensibilidadeMouse;
        set => sensibilidadeMouse = Mathf.Max(0f, value);
    }

    /* Escala a sensibilidade do mouse E do controle (Configurações do menu de pausa). */
    public float MultiplicadorSensibilidade { get; set; } = 1f;

    public bool InverterY
    {
        get => inverterY;
        set => inverterY = value;
    }

    void OnEnable()
    {
        InputActionAsset asset = acoes != null ? acoes : InputSystem.actions;
        if (asset == null)
        {
            Debug.LogWarning("[EntradaJogador] Nenhum InputActionAsset — o jogador não vai responder à entrada.", this);
            return;
        }

        acaoMover = asset.FindAction("Player/Move");
        acaoOlhar = asset.FindAction("Player/Look");
        acaoPular = asset.FindAction("Player/Jump");
        acaoCorrer = asset.FindAction("Player/Sprint");
        acaoInteragir = asset.FindAction("Player/Attack");

        InputActionMap mapa = asset.FindActionMap("Player");
        mapa?.Enable();
    }

    void Update()
    {
        if (acaoPular != null && acaoPular.WasPressedThisFrame() && Habilitado)
            ultimoApertoPulo = Time.time;

        Olhar = Habilitado ? LerOlhar() : Vector2.zero;
    }

    private Vector2 LerOlhar()
    {
        if (acaoOlhar == null)
            return Vector2.zero;

        Vector2 bruto = acaoOlhar.ReadValue<Vector2>();
        bool doControle = acaoOlhar.activeControl != null && acaoOlhar.activeControl.device is Gamepad;

        // Mouse já vem como deslocamento do frame (pixels); analógico vem
        // como posição (-1..1) e precisa virar velocidade × tempo.
        Vector2 graus = doControle
            ? bruto * (sensibilidadeControle * Time.deltaTime)
            : bruto * sensibilidadeMouse;
        graus *= MultiplicadorSensibilidade;

        if (inverterY)
            graus.y = -graus.y;
        return graus;
    }
}
