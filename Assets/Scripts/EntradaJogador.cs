using UnityEngine;
using UnityEngine.InputSystem;

/* ============================================================
 *  EntradaJogador.cs
 *  ---------------------------------------------------------------------
 *  ÚNICO lugar que lê a entrada do jogador (andar, olhar, pular, correr,
 *  interagir, trocar de ferramenta, largar). Usa as ações do asset
 *  Assets/InputSystem_Actions.inputactions (mapa "Player"), então
 *  teclado+mouse e controle (gamepad) funcionam sem nenhum código extra:
 *
 *    Move      WASD / setas       | analógico esquerdo
 *    Look      mouse              | analógico direito
 *    Jump      Espaço             | botão Sul (A / X)
 *    Sprint    Shift esquerdo     | apertar o analógico esquerdo
 *    Attack    clique esquerdo    | botão Oeste (X / Quadrado)
 *    Next      roda para baixo    | RB / direcional para a direita
 *    Previous  roda para cima     | LB / direcional para a esquerda
 *    Largar    clique direito / G | botão Norte (Y / Triângulo)
 *
 *  Kit do agente (Parte A): as teclas 1–5 (ou 1–5 do teclado numérico)
 *  escolhem a ferramenta direto, e a roda do mouse anda pela barra. As
 *  duas são lidas aqui, direto do Keyboard/Mouse, porque não têm ação no
 *  asset (as teclas 1 e 2 saíram de Previous/Next para isso).
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
    private InputAction acaoProxima;
    private InputAction acaoAnterior;
    private InputAction acaoLargar;
    private float ultimoApertoPulo = float.NegativeInfinity;

    // Teclas que escolhem a ferramenta: índice 0..4 = espaço 1..5 da barra.
    private static readonly Key[] teclasFerramenta = { Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4, Key.Digit5 };
    private static readonly Key[] teclasFerramentaNumpad = { Key.Numpad1, Key.Numpad2, Key.Numpad3, Key.Numpad4, Key.Numpad5 };

    // Abaixo disso, o valor da roda é ruído (mesmo limiar do CelularController).
    private const float LimiarRoda = 0.01f;

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

    /* Botão de interagir mantido apertado (o "segurar para agir" do kit). */
    public bool InteragirSegurado => Habilitado && acaoInteragir != null && acaoInteragir.IsPressed();

    /* Largar o pneu, apertado neste frame. */
    public bool LargarPressionado => Habilitado && acaoLargar != null && acaoLargar.WasPressedThisFrame();

    /* Ferramenta escolhida neste frame pelas teclas 1–5 (ou numpad 1–5):
     * 0..4 = espaço da barra; −1 = nenhuma. */
    public int FerramentaPedida
    {
        get
        {
            Keyboard teclado = Keyboard.current;
            if (!Habilitado || teclado == null)
                return -1;

            for (int i = 0; i < teclasFerramenta.Length; i++)
            {
                if (teclado[teclasFerramenta[i]].wasPressedThisFrame ||
                    teclado[teclasFerramentaNumpad[i]].wasPressedThisFrame)
                    return i;
            }
            return -1;
        }
    }

    /* Andar na barra neste frame: +1 = próxima (Next ou roda para baixo),
     * −1 = anterior (Previous ou roda para cima), 0 = nada. */
    public int TrocaDeFerramenta
    {
        get
        {
            if (!Habilitado)
                return 0;

            Mouse mouse = Mouse.current;
            float roda = mouse != null ? mouse.scroll.ReadValue().y : 0f;

            if ((acaoProxima != null && acaoProxima.WasPressedThisFrame()) || roda < -LimiarRoda)
                return +1;
            if ((acaoAnterior != null && acaoAnterior.WasPressedThisFrame()) || roda > LimiarRoda)
                return -1;
            return 0;
        }
    }

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

        acaoMover = AcharAcao(asset, "Player/Move");
        acaoOlhar = AcharAcao(asset, "Player/Look");
        acaoPular = AcharAcao(asset, "Player/Jump");
        acaoCorrer = AcharAcao(asset, "Player/Sprint");
        acaoInteragir = AcharAcao(asset, "Player/Attack");
        acaoProxima = AcharAcao(asset, "Player/Next");
        acaoAnterior = AcharAcao(asset, "Player/Previous");
        acaoLargar = AcharAcao(asset, "Player/Largar");

        InputActionMap mapa = asset.FindActionMap("Player");
        mapa?.Enable();
    }

    /* FindAction com aviso: uma ação faltando no asset (ex.: asset antigo,
     * sem "Largar") só desliga aquele comando, mas não pode passar calada. */
    private InputAction AcharAcao(InputActionAsset asset, string nome)
    {
        InputAction acao = asset.FindAction(nome);
        if (acao == null)
            Debug.LogWarning($"[EntradaJogador] Ação \"{nome}\" não encontrada no asset {asset.name} — esse comando não vai responder.", this);
        return acao;
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
