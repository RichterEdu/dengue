using UnityEngine;

/* ============================================================
 *  ItensNaMao.cs
 *  ---------------------------------------------------------------------
 *  Os itens do kit na mão direita do agente: balde de areia, tela com
 *  moldura e borrifador. Fica no Player, junto do KitDoAgente. O saco de
 *  lixo e o pneu continuam no FeedbackDeCarga.
 *
 *    - Qual item aparece e quando há gesto é a RegraDoItemNaMao. Aqui só
 *      se aplica o resultado: a cada OnMudou do kit, ativa o objeto certo
 *      (a areia do balde só com carga).
 *    - A camada "ItemNaMao" do Animator (só o braço direito) tem um estado
 *      de segurar e um de usar por item. O peso vai a 1 em duracaoBraco
 *      com item na mão e a 0 sem item. O estado troca por CrossFade, sem
 *      parâmetros nem transições no controller.
 *    - Com gesto, liga a emissão da partícula do item. Desligar só a
 *      emissão deixa os grãos que estão no ar terminarem de cair.
 *    - Na vista de perto (Interactor.DePerto) a mão fica quieta, sem
 *      gesto nem névoa: quem borrifa ali é a cópia do borrifador presa à
 *      câmera (BorrifadorDePerto, que lê o BorrifadorNaMao).
 *    - O ombro gira com o olhar (LateUpdate, sobre a pose do Animator): o
 *      bueiro e o vaso ficam no chão e, com a pose presa ao corpo, olhar
 *      para baixo subiria o item para cima do foco.
 *
 *  O Animator do corpo pode ainda não estar pronto ao carregar a cena:
 *  a camada é procurada depois, e o aviso de camada ausente sai uma vez
 *  só (rodar Dengue > Configurar Itens na Mão). Sem camada, o item
 *  aparece sem a pose.
 *
 *  Montado pelo menu  Dengue > Configurar Itens na Mão
 *  (Assets/Editor/ItensNaMaoBuilder.cs).
 * ============================================================ */

public class ItensNaMao : MonoBehaviour
{
    public const string NomeCamada = "ItemNaMao";

    private const float SegundosDaTrocaDeEstado = 0.12f;

    [Header("Referências")]
    [Tooltip("Kit do agente. Vazio = o deste objeto.")]
    [SerializeField] private KitDoAgente kit;

    [Tooltip("Quem diz se o agente está agindo num foco. Vazio = o da cena.")]
    [SerializeField] private Interactor interactor;

    [Tooltip("Controlador do jogador, de onde vem o olhar. Vazio = o deste objeto.")]
    [SerializeField] private ControladorJogador jogador;

    [Tooltip("Animator do corpo. Vazio = o primeiro entre os filhos.")]
    [SerializeField] private Animator animator;

    [Header("Itens (filhos do osso RightHand)")]
    [SerializeField] private Transform baldeNaMao;

    [Tooltip("A areia dentro do balde: só aparece com carga.")]
    [SerializeField] private GameObject areiaDoBalde;

    [SerializeField] private Transform telaNaMao;
    [SerializeField] private Transform borrifadorNaMao;

    [Header("Partículas do gesto")]
    [SerializeField] private ParticleSystem areiaCaindo;
    [SerializeField] private ParticleSystem nevoa;

    [Header("Braço")]
    [Tooltip("Osso RightUpperArm: gira para o braço acompanhar o olhar.")]
    [SerializeField] private Transform ombroDireito;

    [Tooltip("Segundos para o braço ir ao item e voltar (peso da camada do Animator).")]
    [SerializeField] private float duracaoBraco = 0.15f;

    [Tooltip("Quanto do olhar o braço acompanha (0 = nada, 1 = tudo).")]
    [Range(0f, 1f)]
    [SerializeField] private float fatorDoOlhar = 1f;

    private KitDoAgente kitAssinado;
    private bool animatorConferido;
    private bool avisouCamada;
    private int camada = -1;

    /* O borrifador da mão: o original que o BorrifadorDePerto copia. */
    internal Transform BorrifadorNaMao => borrifadorNaMao;

    internal ItemNaMao ItemAtual { get; private set; }
    internal float Peso { get; private set; }
    internal bool Gesticulando { get; private set; }

    /* Último estado pedido à camada (null = nenhum). */
    internal string EstadoNaCamada { get; private set; }

    void Awake()
    {
        if (kit == null) kit = GetComponent<KitDoAgente>();
        if (jogador == null) jogador = GetComponent<ControladorJogador>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (interactor == null) interactor = FindAnyObjectByType<Interactor>();
    }

    void OnEnable()
    {
        Registrar();
    }

    void OnDisable()
    {
        if (kitAssinado != null)
            kitAssinado.OnMudou -= Aplicar;
        kitAssinado = null;
    }

    void Update()
    {
        AtualizarGesto(Time.deltaTime, QuerGesticular());
    }

    void LateUpdate()
    {
        GirarOmbro();
    }

    /* Assina o OnMudou do kit (uma vez só) e aplica o estado atual. */
    public void Registrar()
    {
        animatorConferido = false;
        camada = -1;
        EstadoNaCamada = null;

        if (kit != kitAssinado)
        {
            if (kitAssinado != null)
                kitAssinado.OnMudou -= Aplicar;
            kitAssinado = kit;
            if (kitAssinado != null)
                kitAssinado.OnMudou += Aplicar;
        }

        Aplicar();
    }

    /* Põe na mão o item da ferramenta selecionada. */
    internal void Aplicar()
    {
        if (kit == null)
            return;

        Ferramenta selecionada = kit.Selecionada;
        ItemAtual = RegraDoItemNaMao.Qual(selecionada, kit.Estoque.Quantidade(selecionada), kit.MaosOcupadas);

        bool balde = ItemAtual == ItemNaMao.BaldeCheio || ItemAtual == ItemNaMao.BaldeVazio;
        Ativar(baldeNaMao, balde);
        Ativar(telaNaMao, ItemAtual == ItemNaMao.Tela);
        Ativar(borrifadorNaMao, ItemAtual == ItemNaMao.Borrifador);
        if (areiaDoBalde != null)
            areiaDoBalde.SetActive(ItemAtual == ItemNaMao.BaldeCheio);
    }

    /* Peso da camada, partículas e estado do Animator, a cada quadro. */
    internal void AtualizarGesto(float dt, bool gesticula)
    {
        Gesticulando = gesticula && ItemAtual != ItemNaMao.Nenhum;

        float alvo = ItemAtual != ItemNaMao.Nenhum ? 1f : 0f;
        Peso = Mathf.MoveTowards(Peso, alvo, dt / Mathf.Max(0.01f, duracaoBraco));

        bool comBalde = ItemAtual == ItemNaMao.BaldeCheio || ItemAtual == ItemNaMao.BaldeVazio;
        LigarEmissao(areiaCaindo, Gesticulando && comBalde);
        LigarEmissao(nevoa, Gesticulando && ItemAtual == ItemNaMao.Borrifador);

        ConferirAnimator();
        if (camada < 0)
            return;

        animator.SetLayerWeight(camada, Peso);

        string estado = RegraDoItemNaMao.EstadoDoAnimator(ItemAtual, Gesticulando);
        if (estado != null && estado != EstadoNaCamada)
        {
            animator.CrossFadeInFixedTime(estado, SegundosDaTrocaDeEstado, camada);
            EstadoNaCamada = estado;
        }
    }

    /* O braço acompanha o olhar. Roda depois do Animator, sobre a pose do quadro. */
    internal void GirarOmbro()
    {
        if (ombroDireito == null || jogador == null || Peso <= 0f)
            return;

        float giro = RegraDoItemNaMao.GiroDoOmbro(jogador.Pitch, fatorDoOlhar, Peso);
        ombroDireito.RotateAround(ombroDireito.position, transform.right, giro);
    }

    /* Se o item da mão gesticula neste quadro (a regra, com o estado do Interactor). */
    internal bool QuerGesticular()
    {
        if (kit == null || interactor == null)
            return false;

        Interactable alvo = interactor.CurrentTarget;
        Ferramenta? necessaria = alvo != null ? alvo.FerramentaNecessaria : (Ferramenta?)null;
        return RegraDoItemNaMao.Gesticula(ItemAtual, kit.Selecionada, interactor.SkillCheckAtivo != null,
                                          interactor.Avancando, necessaria, interactor.DePerto);
    }

    /* Procura a camada quando o Animator fica pronto, e avisa uma vez se ela não existir. */
    private void ConferirAnimator()
    {
        if (animatorConferido || animator == null || !animator.isInitialized)
            return;

        animatorConferido = true;
        if (animator.runtimeAnimatorController != null)
            camada = animator.GetLayerIndex(NomeCamada);

        if (camada < 0 && !avisouCamada)
        {
            avisouCamada = true;
            Debug.LogWarning("ItensNaMao: o Animator de \"" + animator.name + "\" não tem a camada \"" + NomeCamada
                + "\". Os itens ficam sem a pose: rode Dengue > Configurar Itens na Mão.", this);
        }
    }

    private static void Ativar(Transform item, bool ativo)
    {
        if (item != null)
            item.gameObject.SetActive(ativo);
    }

    private static void LigarEmissao(ParticleSystem particulas, bool ligada)
    {
        if (particulas == null)
            return;

        ParticleSystem.EmissionModule emissao = particulas.emission;
        emissao.enabled = ligada;
    }
}
