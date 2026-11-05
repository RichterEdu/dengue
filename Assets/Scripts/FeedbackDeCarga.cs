using UnityEngine;

/* ============================================================
 *  FeedbackDeCarga.cs
 *  ---------------------------------------------------------------------
 *  O peso de carregar (Parte B), sentido no corpo do agente de saúde.
 *  Fica no Player, junto do KitDoAgente, e escuta o OnMudou do kit:
 *
 *    - com um pneu nos braços: o Animator liga o bool "Carregando" (a
 *      camada "Carregando" do Agente_AnimController põe os braços em
 *      volta do pneu) e o peso dessa camada vai a 1 em duracaoBracos;
 *      sem pneu, volta a 0 (no asset ela nasce em 0; o estado Vazio da
 *      camada não tem clipe e, com peso acima de 0, prenderia os braços
 *      na última pose: o peso 0 deixa a locomoção passar inteira). A
 *      câmera entra no ModoPesado (balanço mais forte e passo mais
 *      longo, CameraJogador) e os passos ficam pesados (PassoPesado,
 *      SomDoJogador). Sem pneu, tudo volta ao normal;
 *    - o saco de lixo na mão direita (sacoNaMao, filho do osso
 *      RightHand, montado pelo PoseDeCarga do Editor) aparece com
 *      alguma garrafa e o saco selecionado na barra (trocar de ferramenta
 *      o guarda, com as garrafas dentro) e cresce com elas: escala entre escalaSaco.x
 *      (quase vazio) e escalaSaco.y (cheio), pela fração do saco. A
 *      camada "CarregandoSaco" do Animator ergue o braço direito à
 *      frente, para o saco entrar no canto da tela: o peso dela vai a 1
 *      em duracaoBracos (e a 0 sem saco);
 *    - com o pneu nos braços, o saco fica escondido, mesmo com garrafas:
 *      as duas mãos estão no pneu. Largado ou entregue o pneu, ele volta.
 *
 *  Sons (2D, no canal de efeitos, com o corpo colado no AudioListener):
 *    - com o pneu nos braços, um esforço sorteado a cada intervaloEsforco
 *      (AvancarEsforco), parado ou andando, e um na hora em que a stamina
 *      zera (KitDoAgente.OnStaminaEsgotada);
 *    - com o saco a mais da metade à mão (selecionado, sem o pneu) e o agente andando
 *      no chão, o saco tilinta a cada passo: quando a fase do CameraJogador
 *      passa por π/2 + kπ, como os passos do SomDoJogador (PassoDoSaco).
 *  Sem clipe ligado, nada toca. Os dois param com a partida encerrada
 *  (fluxo) ou sem o controle do agente (jogador: cartelas, pausa, epílogo).
 *  Os clipes são ligados por SomBuilder.LigarSonsDosFocosVivos.
 *
 *  O Animator do corpo é um filho e pode não estar pronto no OnEnable
 *  (ordem de carga da cena). Por isso o parâmetro e as camadas são lidos
 *  só quando ele fica pronto (ConferirAnimator, a cada Aplicar e a cada
 *  frame até dar certo), e aí ele já recebe o estado do kit. Se o
 *  controller não tiver o que a pose precisa, sai um aviso, uma vez só.
 *
 *  No EditMode o OnEnable não roda: o Registrar() assina o kit e aplica
 *  o estado atual (os testes chamam direto). Referências vazias são
 *  procuradas no Awake (o kit, o jogador e o som neste objeto, o Animator
 *  no corpo, a câmera e o fluxo na cena). A decisão de tocar fica em
 *  AvancarEsforco e PassoDoSaco, chamáveis nos testes (a fonte só existe
 *  em Play).
 * ============================================================ */

public class FeedbackDeCarga : MonoBehaviour
{
    private static readonly int ParametroCarregando = Animator.StringToHash("Carregando");
    private const string NomeCamadaCarregando = "Carregando";
    private const string NomeCamadaSaco = "CarregandoSaco";

    [Header("Referências (vazio = procura)")]
    [Tooltip("Kit do agente que avisa o pneu nos braços e as garrafas no saco. Vazio = o deste objeto.")]
    [SerializeField] private KitDoAgente kit;

    [Tooltip("Animator do corpo do agente (bool \"Carregando\" da camada de carga). Vazio = o do corpo (filho).")]
    [SerializeField] private Animator animator;

    [Tooltip("Câmera em primeira pessoa que balança mais com o pneu (ModoPesado). Vazio = a da cena.")]
    [SerializeField] private CameraJogador cameraJogador;

    [Tooltip("Sons do corpo que trocam para o passo pesado com o pneu (PassoPesado). Vazio = o deste objeto.")]
    [SerializeField] private SomDoJogador somDoJogador;

    [Tooltip("Agente que este corpo acompanha: sem controle (cartelas, pausa, epílogo), os sons de carga param. Vazio = o deste objeto.")]
    [SerializeField] private ControladorJogador jogador;

    [Tooltip("Fluxo da partida: encerrada, os sons de carga param. Vazio = procura o FluxoDeJogo na cena.")]
    [SerializeField] private FluxoDeJogo fluxo;

    [Tooltip("Segundos para os braços irem ao pneu ou ao saco e voltarem (peso das camadas de carga do Animator).")]
    [SerializeField] private float duracaoBracos = 0.15f;

    [Header("Esforço")]
    [Tooltip("Gemidos de esforço do agente carregando o pneu (um por vez, sorteado).")]
    [SerializeField] private AudioClip[] esforco;

    [Tooltip("Segundos entre um esforço e o próximo enquanto carrega o pneu (mínimo, máximo).")]
    [SerializeField] private Vector2 intervaloEsforco = new Vector2(3f, 6f);

    [Tooltip("Volume dos sons de esforço.")]
    [Range(0f, 1f)]
    [SerializeField] private float volumeEsforco = 0.4f;

    [Header("Tilintar do saco")]
    [Tooltip("Garrafas batendo no saco, a cada passo, com o saco a mais da metade.")]
    [SerializeField] private AudioClip sacoTilintar;

    [Tooltip("Volume do tilintar do saco.")]
    [Range(0f, 1f)]
    [SerializeField] private float volumeTilintar = 0.3f;

    [Tooltip("Abaixo desta velocidade (m/s) o agente está parado e o saco não tilinta.")]
    [SerializeField] private float velocidadeMinimaTilintar = 0.3f;

    [Header("Saco na mão")]
    [Tooltip("Saco de lixo preso à mão direita (filho do osso RightHand). Aparece com garrafas no saco, o saco selecionado e sem pneu nos braços.")]
    [SerializeField] private Transform sacoNaMao;

    [Tooltip("Escala do saco com uma garrafa quase nada (x) e com o saco cheio (y); no meio, pela fração de garrafas.")]
    [SerializeField] private Vector2 escalaSaco = new Vector2(0.5f, 1.0f);

    private KitDoAgente kitAssinado;
    private bool animatorConferido;      // já leu o parâmetro e as camadas do Animator pronto
    private bool avisouAnimator;         // o aviso de controller incompleto sai uma vez só
    private bool temParametroCarregando;
    private int camadaCarregando = -1;   // índice da camada no Animator (-1 = não tem)
    private int camadaSaco = -1;         // idem, a do braço do saco
    private float pesoAlvo;              // camada "Carregando": 1 com pneu, 0 sem
    private float pesoAlvoSaco;          // camada "CarregandoSaco": 1 com o saco à vista, 0 sem
    private CanalDeAudio fonte;
    private float esperaEsforco = -1f;   // segundos até o próximo esforço (negativo = relógio parado)
    private int ultimoEsforco = -1;
    private float ultimaFase;

    void Awake()
    {
        if (kit == null) kit = GetComponent<KitDoAgente>();
        if (somDoJogador == null) somDoJogador = GetComponent<SomDoJogador>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (cameraJogador == null) cameraJogador = FindAnyObjectByType<CameraJogador>();

        if (jogador == null) jogador = GetComponent<ControladorJogador>();
        if (fluxo == null) fluxo = FindAnyObjectByType<FluxoDeJogo>();

        fonte = CanalDeAudio.Criar(transform, "Som Carga", CanalSom.Efeitos, false);
        fonte.Fonte.priority = 60;
    }

    void OnEnable()
    {
        Registrar();
    }

    void Update()
    {
        AtualizarPesoDosBracos(Time.deltaTime);

        if (AvancarEsforco(Time.deltaTime))
            TocarEsforco();

        if (cameraJogador != null && PassoDoSaco(cameraJogador.FasePasso, Andando()))
            Tocar(sacoTilintar, volumeTilintar);
    }

    void OnDisable()
    {
        if (kitAssinado != null)
        {
            kitAssinado.OnMudou -= Aplicar;
            kitAssinado.OnStaminaEsgotada -= Cansou;
        }
        kitAssinado = null;
    }

    /* Assina o OnMudou do kit (uma vez só) e aplica o estado atual. */
    public void Registrar()
    {
        animatorConferido = false;
        temParametroCarregando = false;
        camadaCarregando = -1;
        camadaSaco = -1;

        if (kit != kitAssinado)
        {
            if (kitAssinado != null)
            {
                kitAssinado.OnMudou -= Aplicar;
                kitAssinado.OnStaminaEsgotada -= Cansou;
            }
            kitAssinado = kit;
            if (kitAssinado != null)
            {
                kitAssinado.OnMudou += Aplicar;
                kitAssinado.OnStaminaEsgotada += Cansou;
            }
        }
        Aplicar();
    }

    private void Cansou()
    {
        if (SonsLigados)
            TocarEsforco();
    }

    /* Os sons de carga só valem com a partida rolando e o agente no controle
     * (o esforço não pode tocar no epílogo). Sem jogador ou sem fluxo ligado
     * (como nos testes EditMode), não há o que os pare. */
    private bool SonsLigados => (fluxo == null || !fluxo.PartidaEncerrada)
                                && (jogador == null || jogador.ControleAtivo);

    /* Anda o relógio do esforço; true = hora de um esforço. Sem pneu ou sem
     * sons (SonsLigados), ele para; ao pegar o pneu, sorteia a primeira espera. */
    internal bool AvancarEsforco(float dt)
    {
        if (kit == null || kit.PneuCarregado == null || !SonsLigados)
        {
            esperaEsforco = -1f;
            return false;
        }

        if (esperaEsforco < 0f)
        {
            esperaEsforco = SortearEspera();
            return false;
        }

        esperaEsforco -= dt;
        if (esperaEsforco > 0f)
            return false;

        esperaEsforco = SortearEspera();
        return true;
    }

    private float SortearEspera()
    {
        return Mathf.Max(0.01f, Random.Range(intervaloEsforco.x, intervaloEsforco.y));
    }

    /* Recebe a fase do passo a cada quadro; true = o pé bateu no chão (a
     * fase passou por π/2 + kπ) com o agente andando, o saco pesado e os
     * sons ligados (SonsLigados). */
    internal bool PassoDoSaco(float fase, bool andando)
    {
        int antes = Mathf.FloorToInt((ultimaFase - Mathf.PI * 0.5f) / Mathf.PI);
        int agora = Mathf.FloorToInt((fase - Mathf.PI * 0.5f) / Mathf.PI);
        ultimaFase = fase;
        return agora > antes && andando && SacoPesado() && SonsLigados;
    }

    /* O saco fica à mão só selecionado e sem pneu (as duas mãos estão nele). */
    private bool SacoEmMaos()
    {
        return kit != null && kit.PneuCarregado == null && kit.Selecionada == Ferramenta.SacoDeLixo;
    }

    /* Mais da metade do saco, e à mão. */
    private bool SacoPesado()
    {
        if (!SacoEmMaos())
            return false;

        EstoqueDoKit estoque = kit.Estoque;
        return estoque.GarrafasNoSaco * 2 > estoque.CapacidadeDoSaco;
    }

    private bool Andando()
    {
        return jogador != null && jogador.NoChao && jogador.VelocidadeHorizontal > velocidadeMinimaTilintar;
    }

    /* Um esforço sorteado, sem repetir o anterior. */
    private void TocarEsforco()
    {
        if (esforco == null || esforco.Length == 0)
            return;

        int i = Random.Range(0, esforco.Length);
        if (esforco.Length > 1 && i == ultimoEsforco)
            i = (i + 1) % esforco.Length;
        ultimoEsforco = i;
        Tocar(esforco[i], volumeEsforco);
    }

    private void Tocar(AudioClip clip, float volume)
    {
        if (clip != null && fonte != null)
            fonte.Fonte.PlayOneShot(clip, volume);
    }

    /* Lê o parâmetro e as camadas de carga assim que o Animator fica pronto e
     * o leva direto ao estado do kit (o que veio antes disso não chegou nele).
     * Com o Animator ainda sem inicializar, não faz nada: tenta de novo depois.
     * Sem o parâmetro ou uma camada (controller antigo), avisa uma vez e a pose
     * que falta fica desligada: chamar o SetBool assim só encheria o Console. */
    private void ConferirAnimator()
    {
        if (animatorConferido || animator == null || !animator.isInitialized)
            return;

        animatorConferido = true;
        if (animator.runtimeAnimatorController != null)
        {
            foreach (AnimatorControllerParameter p in animator.parameters)
                if (p.nameHash == ParametroCarregando)
                    temParametroCarregando = true;
            camadaCarregando = animator.GetLayerIndex(NomeCamadaCarregando);
            camadaSaco = animator.GetLayerIndex(NomeCamadaSaco);
        }

        if (!avisouAnimator && (!temParametroCarregando || camadaCarregando < 0 || camadaSaco < 0))
        {
            avisouAnimator = true;
            Debug.LogWarning("FeedbackDeCarga: o Animator de \"" + animator.name + "\" não tem tudo o que a pose precisa "
                + "(parâmetro \"Carregando\": " + temParametroCarregando + ", camada \"Carregando\": " + (camadaCarregando >= 0)
                + ", camada \"" + NomeCamadaSaco + "\": " + (camadaSaco >= 0)
                + "). A pose que falta fica desligada: rode o PoseDeCarga.GarantirAssets().", this);
        }

        if (temParametroCarregando)
            animator.SetBool(ParametroCarregando, pesoAlvo > 0f);
        if (camadaCarregando >= 0)
            animator.SetLayerWeight(camadaCarregando, pesoAlvo);
        if (camadaSaco >= 0)
            animator.SetLayerWeight(camadaSaco, pesoAlvoSaco);
    }

    /* Leva o peso das camadas de carga aos alvos (pneu e saco), em duracaoBracos. */
    internal void AtualizarPesoDosBracos(float dt)
    {
        ConferirAnimator();
        if (animator == null)
            return;

        float passo = dt / Mathf.Max(0.01f, duracaoBracos);
        LevarPeso(camadaCarregando, pesoAlvo, passo);
        LevarPeso(camadaSaco, pesoAlvoSaco, passo);
    }

    private void LevarPeso(int camada, float alvo, float passo)
    {
        if (camada >= 0)
            animator.SetLayerWeight(camada, Mathf.MoveTowards(animator.GetLayerWeight(camada), alvo, passo));
    }

    /* Põe o corpo, a câmera, os passos e o saco no estado do kit. */
    internal void Aplicar()
    {
        if (kit == null)
            return;

        bool carregando = kit.PneuCarregado != null;
        EstoqueDoKit estoque = kit.Estoque;
        int garrafas = estoque.GarrafasNoSaco;
        bool sacoAVista = sacoNaMao != null && garrafas > 0 && SacoEmMaos();

        pesoAlvo = carregando ? 1f : 0f;
        pesoAlvoSaco = sacoAVista ? 1f : 0f;
        ConferirAnimator();
        if (animator != null && temParametroCarregando)
            animator.SetBool(ParametroCarregando, carregando);
        if (cameraJogador != null)
            cameraJogador.ModoPesado = carregando;
        if (somDoJogador != null)
            somDoJogador.PassoPesado = carregando;

        if (sacoNaMao == null)
            return;

        sacoNaMao.gameObject.SetActive(sacoAVista);
        if (sacoAVista)
        {
            float fracao = (float)garrafas / Mathf.Max(1, estoque.CapacidadeDoSaco);
            sacoNaMao.localScale = Vector3.one * Mathf.Lerp(escalaSaco.x, escalaSaco.y, fracao);
        }
    }
}
