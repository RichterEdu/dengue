using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/* ============================================================
 *  Interactor.cs
 *  ---------------------------------------------------------------------
 *  Fica no PLAYER e gerencia a interação em primeira pessoa.
 *
 *  - Dispara um raycast do CENTRO da tela (como a visão do jogador) e
 *    mira o primeiro alvo válido: um foco (Interactable ou derivado) que
 *    ainda aceita passos, ou um ponto de uso (PontoDeUso: picape, monte
 *    de areia, caçamba), achados no próprio objeto ou nos pais. Foco já
 *    concluído (ou que não pode ser alvo agora) não tapa a mira: o raio
 *    passa por ele (o pneu entregue fica na vaga da carroceria). Parede,
 *    chão e o resto tapam.
 *  - SEGURAR para agir (Parte A): o botão de interagir (clique ESQUERDO;
 *    no controle, a ação "Attack" do EntradaJogador.cs, botão Oeste)
 *    mantido apertado acumula tempo no alvo (SessaoDeSegurar.cs). Cada
 *    SegundosPorPasso segurado vira um AvancarCom(kit) do foco (= um
 *    clique antigo); no ponto, segurar SegundosParaUsarPara(kit) chama
 *    Usar(kit), um uso por aperto.
 *  - No aperto (ou ao trocar de alvo segurando), o alvo é conferido uma
 *    vez (Verificar / PodeUsar). Recusado, OnRecusado / OnPontoRecusado
 *    avisa a HUD e nada acumula até soltar o botão ou trocar de alvo.
 *    Recusa de ponto com motivo vazio passa em silêncio.
 *  - Kit do agente: as teclas 1–5, a roda do mouse e LB/RB trocam a
 *    ferramenta; o botão direito (G; no controle, botão Norte) larga o
 *    pneu. Com o pneu nas mãos, só ponto de uso é alvo (a caçamba). Sem
 *    KitDoAgente no player, segurar avança o foco sem ferramenta (como o
 *    clique antigo) e os pontos de uso recusam sem aviso.
 *  - Atalho de esvaziar (Parte B): com as Mãos num vaso ou grupo de
 *    garrafas ainda não começado, o passo vale SegundosPorPassoCom(kit)
 *    e o resultado Esvaziou encerra o aperto (como o uso de ponto:
 *    segurar mais não emenda num JaVazio). JaVazio é recusa, como a
 *    ferramenta errada.
 *  - Minigame (Minigame.cs; foco cujo CriarMinigame devolve uma sessão,
 *    como o skill check do ralo): só um aperto NOVO, conferido, começa a
 *    sessão no lugar do segurar (ConferirFoco); a mira chegando ao foco com
 *    o botão já apertado não começa nada nem avisa, e o segurar fica preso
 *    até soltar. O jogador fica parado até o fim, sem trocar de
 *    ferramenta nem de alvo. A cada quadro, AvancarMinigame: cada Passo é
 *    um AvancarCom(kit) (resultado diferente de Avancou encerra); Errou
 *    chama AoErrarMinigame(kit) e OnErrouSkillCheck; o botão de largar
 *    sai (AoSairDoMinigame) se a sessão PodeSair; sessão com mira
 *    (IMinigameComMira) aponta o olhar. Bloqueado (AoBloquear) sai sem
 *    culpa. Encerrado, segurar só volta a valer depois de soltar o botão.
 *  - ProgressoContinuo (0..1) e Avancando alimentam a barra da HUD e o
 *    som de ação.
 *  - Destaque visual: quando o jogador mira num foco ou ponto de uso, os
 *    materiais (Renderer) do alvo ganham um brilho de emissão pulsante
 *    (no ponto de uso, sem os focos que estão nele: os pneus empilhados
 *    na carroceria da picape). Ao deselecionar, cada material é restaurado
 *    ao estado original.
 *  - Usa o Input System NOVO, pelo EntradaJogador.cs (Mouse.current só
 *    quando o player não tem EntradaJogador).
 * ============================================================ */

public class Interactor : MonoBehaviour
{
    [Header("Interação")]
    [Tooltip("Distância máxima de interação, em unidades.")]
    [SerializeField] private float interactionRange = 3f;

    [Header("Kit do agente")]
    [Tooltip("Kit do agente (ferramenta, cargas, pneu). Vazio = o deste mesmo objeto. Sem kit, segurar avança o foco sem ferramenta, como o clique antigo.")]
    [SerializeField] private KitDoAgente kit;

    [Header("Destaque ao mirar")]
    [Tooltip("Cor do brilho de emissão aplicada ao objeto mirado.")]
    [SerializeField] private Color highlightColor = new Color(1f, 0.85f, 0.2f);
    [Tooltip("Intensidade base do brilho.")]
    [SerializeField] private float highlightIntensity = 1.6f;
    [Tooltip("Velocidade do pulso do brilho (0 = brilho fixo).")]
    [SerializeField] private float highlightPulseSpeed = 4.5f;

    private Camera cam;
    private EntradaJogador entrada;
    private Interactable currentTarget;
    private PontoDeUso pontoAlvo;

    // O agente de saúde (corpo do próprio jogador, ver PlayerBuilder.cs) não tem
    // Collider hoje, então o raycast abaixo não tem como acertar o próprio
    // corpo — mas se algum dia ganhar física (roupa, ragdoll etc.), esta
    // máscara evita que o jogador "mire em si mesmo" sem querer.
    // NÃO pode ser "static readonly" com inicializador: LayerMask.NameToLayer
    // é proibido em inicializador de campo/construtor estático de
    // MonoBehaviour (Unity lança exceção toda vez que o tipo é usado) —
    // por isso é lido em Start(), como qualquer outra referência da cena.
    private int camadaCorpoJogador = -1;

    // O raio atravessa focos fora de jogo, então são vários acertos por
    // quadro. 16 sobra para 3 m de alcance (sem alocar a cada quadro).
    private const int MaxAcertos = 16;
    private readonly RaycastHit[] acertos = new RaycastHit[MaxAcertos];
    private static readonly IComparer<RaycastHit> PorDistancia =
        Comparer<RaycastHit>.Create((a, b) => a.distance.CompareTo(b.distance));

    // Segurar para agir: tempo segurado no alvo → passos.
    private readonly SessaoDeSegurar sessao = new SessaoDeSegurar();

    // Conferência do aperto: o alvo já conferido enquanto o botão está
    // apertado, e se ele liberou o segurar (false depois de uma recusa ou
    // de um uso de ponto, até soltar o botão ou trocar de alvo).
    private Component alvoConferido;
    private bool liberado;

    // Minigame em andamento (null = nenhum) e o olhar do começo dele (a
    // mira de IMinigameComMira é contada a partir daqui).
    private IMinigame minigame;
    private float yawInicial;
    private float pitchInicial;

    private ControladorJogador jogador;

    private readonly List<HighlightEntry> highlighted = new List<HighlightEntry>();

    /* Recusa num foco (ferramenta errada, sem carga, saco cheio, mãos
     * ocupadas), para a HUD explicar. Sai uma vez por aperto. */
    public event Action<Interactable, ResultadoDoPasso> OnRecusado;

    /* Recusa num ponto de uso, com o motivo (nunca vazio). Sai uma vez por aperto. */
    public event Action<PontoDeUso, string> OnPontoRecusado;

    /* O jogador errou um skill check neste foco (a tela já foi gasta). */
    public event Action<Interactable> OnErrouSkillCheck;

    /* Minigame em andamento (null = nenhum). */
    public IMinigame MinigameAtivo => minigame;

    /* Skill check em andamento (null = nenhum, ou o minigame é outro).
     * Lido pela SkillCheckHUD, pelo ItensNaMao e pela HUD. */
    public SessaoDeSkillCheck SkillCheckAtivo => (minigame as MinigameDeSkillCheck)?.Sessao;

    /* Foco mirado (lido pela HUD, pelo brilho de proximidade e por scripts futuros). */
    public Interactable CurrentTarget => currentTarget;

    /* Ponto de uso mirado (picape, monte de areia, caçamba). Nunca junto com um foco. */
    public PontoDeUso PontoAlvo => pontoAlvo;

    /* Kit do agente usado nos passos e nos pontos (null = sem kit). A HUD
     * pede a ele o texto da mira do ponto (PontoDeUso.TextoPara). Vazio, o
     * deste objeto, achado no primeiro uso (no EditMode o Start não roda). */
    public KitDoAgente Kit
    {
        get
        {
            if (kit == null)
                kit = GetComponent<KitDoAgente>();
            return kit;
        }
    }

    /* O ControladorJogador deste objeto, achado no primeiro uso. */
    private ControladorJogador Jogador
    {
        get
        {
            if (jogador == null)
                jogador = GetComponent<ControladorJogador>();
            return jogador;
        }
    }

    /* Barra da HUD (0..1): no foco, cliques dados + o passo em andamento;
     * no ponto, quanto do uso já foi segurado. */
    public float ProgressoContinuo { get; private set; }

    /* Segurando num alvo liberado (passo válido) neste quadro, ou num
     * minigame cuja sessão está Agindo: som de ação e item na mão. */
    public bool Avancando { get; private set; }

    /* Quando true, o jogador não mira nem interage com nada (usado, por
     * exemplo, enquanto o celular está levantado). Ao bloquear, o destaque
     * do alvo atual é desfeito e o segurar é zerado. */
    public bool Bloqueado { get; set; }

    /* Guarda o estado ORIGINAL de um material antes de aplicar o destaque. */
    private class HighlightEntry
    {
        public Renderer renderer;
        public Material material;
        public Color originalEmissionColor;
        public bool emissionEnabled;
    }

    void Start()
    {
        cam = Camera.main;
        entrada = GetComponent<EntradaJogador>();
        if (kit == null)
            kit = GetComponent<KitDoAgente>();
        camadaCorpoJogador = LayerMask.NameToLayer("CorpoJogador");
        jogador = GetComponent<ControladorJogador>();
        if (GetComponent<SkillCheckHUD>() == null)
            gameObject.AddComponent<SkillCheckHUD>();
    }

    void Update()
    {
        // 1) Bloqueado (celular levantado, menu aberto...): larga o alvo,
        //    zera o segurar e sai.
        if (Bloqueado)
        {
            AoBloquear();
            return;
        }

        // Minigame: nada de trocar ferramenta nem de alvo até acabar.
        // Pausado, a sessão para e o aperto não conta.
        if (minigame != null)
        {
            if (Time.deltaTime > 0f)
                AvancarMinigame(Time.deltaTime, LerEntradaDoMinigame(), entrada != null && entrada.LargarPressionado);
            else
                Avancando = false;
            UpdateHighlightPulse();
            AtualizarProgresso();
            return;
        }

        // 2) Troca de ferramenta, só com as mãos livres.
        if (kit != null && kit.PneuCarregado == null && entrada != null)
        {
            int pedida = entrada.FerramentaPedida;
            if (pedida >= 0 && pedida < Ferramentas.Todas.Length)
                kit.Selecionar(Ferramentas.Todas[pedida]);

            int troca = entrada.TrocaDeFerramenta;
            if (troca > 0)
                kit.Proxima();
            else if (troca < 0)
                kit.Anterior();
        }

        // 3) Largar o pneu no chão, na frente do agente.
        if (kit != null && kit.PneuCarregado != null && entrada != null && entrada.LargarPressionado)
            kit.LargarPneu(transform.position, transform.forward);

        // 4) Alvo da mira: se mudou, desfaz o destaque no antigo e aplica no novo.
        bool comPneu = kit != null && kit.PneuCarregado != null;
        RaycastTarget(comPneu, out Interactable foco, out PontoDeUso ponto);
        TrocarAlvo(foco, ponto);

        UpdateHighlightPulse();

        // 5) e 6) Conferir no aperto e avançar segurando.
        AtualizarSegurar();

        // 7) Barra da HUD.
        AtualizarProgresso();
    }

    /* Botão de interagir mantido apertado (clique esquerdo ou "Attack"). */
    private bool LerSegurando()
    {
        if (entrada != null)
            return entrada.InteragirSegurado;

        // Sem EntradaJogador no player: só o mouse, como antes.
        Mouse mouse = Mouse.current;
        return mouse != null && mouse.leftButton.isPressed;
    }

    /* Botão de interagir apertado neste quadro. */
    private bool LerApertou()
    {
        if (entrada != null)
            return entrada.InteragirPressionado;

        Mouse mouse = Mouse.current;
        return mouse != null && mouse.leftButton.wasPressedThisFrame;
    }

    /* O que o jogador fez neste quadro, para o minigame. */
    private EntradaDoMinigame LerEntradaDoMinigame()
    {
        return new EntradaDoMinigame
        {
            Olhar = entrada != null ? entrada.Olhar : Vector2.zero,
            Segurando = LerSegurando(),
            Apertou = LerApertou()
        };
    }

    /* Bloqueado: sai do minigame sem culpa, larga o alvo e zera o segurar. */
    internal void AoBloquear()
    {
        if (minigame != null)
        {
            if (currentTarget != null)
                currentTarget.AoSairDoMinigame();
            EncerrarMinigame();
        }
        TrocarAlvo(null, null);
        sessao.Zerar();
        alvoConferido = null;
        liberado = false;
        ProgressoContinuo = 0f;
        Avancando = false;
    }

    /* Começa o minigame do foco (que vira o alvo atual). false = o foco não
     * tem minigame agora (CriarMinigame devolveu null): segue o segurar. */
    internal bool IniciarMinigame(Interactable foco)
    {
        IMinigame criado = foco.CriarMinigame(Kit, UnityEngine.Random.Range(int.MinValue, int.MaxValue));
        if (criado == null)
            return false;

        TrocarAlvo(foco, null);
        minigame = criado;
        sessao.Zerar();
        if (Jogador != null)
        {
            Jogador.Imobilizado = true;
            yawInicial = Jogador.Yaw;
            pitchInicial = Jogador.Pitch;
        }
        foco.AoIniciarMinigame(criado);
        return true;
    }

    /* Um quadro do minigame: sair, atualizar a sessão e aplicar o
     * resultado no foco. Alvo destruído ou desativado encerra. */
    internal void AvancarMinigame(float dt, EntradaDoMinigame entradaDoMinigame, bool sair)
    {
        if (minigame == null)
            return;

        Interactable foco = currentTarget;
        if (foco == null || !foco.gameObject.activeInHierarchy)
        {
            EncerrarMinigame();
            return;
        }

        if (sair && minigame.PodeSair)
        {
            foco.AoSairDoMinigame();
            EncerrarMinigame();
            return;
        }

        IMinigame ativo = minigame;
        EstadoDoMinigame estado = ativo.Atualizar(dt, entradaDoMinigame);
        foco.AoAtualizarMinigame(ativo);
        if (ativo is IMinigameComMira comMira && Jogador != null)
            Jogador.DefinirOlhar(yawInicial + comMira.Mira.x, pitchInicial + comMira.Mira.y);

        if (estado == EstadoDoMinigame.Passo)
        {
            if (foco.AvancarCom(Kit) != ResultadoDoPasso.Avancou)
                EncerrarMinigame();
        }
        else if (estado == EstadoDoMinigame.Errou)
        {
            foco.AoErrarMinigame(Kit);
            EncerrarMinigame();
            OnErrouSkillCheck?.Invoke(foco);
        }

        Avancando = minigame != null && minigame.Agindo;
    }

    /* Fim do minigame: o jogador volta a andar. Segurar só vale de novo
     * depois de soltar o botão (o ralo não avança segurando). */
    private void EncerrarMinigame()
    {
        minigame = null;
        sessao.Zerar();
        liberado = false;
        Avancando = false;
        if (Jogador != null)
            Jogador.Imobilizado = false;
    }

    /* Confere o alvo no aperto (ou ao trocar de alvo segurando) e converte o
     * tempo segurado em passos: AvancarCom no foco, Usar no ponto. */
    private void AtualizarSegurar()
    {
        // Alvo como referência C# de verdade: um objeto destruído vira null
        // aqui (o "null falso" do Unity não pode chegar à SessaoDeSegurar,
        // que compara por referência).
        Component alvo = null;
        if (currentTarget != null)
            alvo = currentTarget;
        else if (pontoAlvo != null)
            alvo = pontoAlvo;

        bool segurando = LerSegurando();
        if (!segurando)
        {
            alvoConferido = null;
            liberado = false;
        }

        // Pausado (timeScale 0): nenhum passo, nem o instantâneo do pneu
        // esvaziado, e nenhuma conferência (o aviso sairia com o jogo parado).
        if (Time.deltaTime <= 0f)
        {
            sessao.Zerar();
            Avancando = false;
            return;
        }

        if (segurando && alvo != null && !ReferenceEquals(alvo, alvoConferido))
        {
            // Aperto novo, ou outro alvo com o botão ainda apertado: confere uma vez.
            alvoConferido = alvo;
            liberado = Conferir(LerApertou());

            // Foco com minigame: o aperto conferido começou a sessão, no lugar do segurar.
            if (minigame != null)
            {
                Avancando = false;
                return;
            }
        }

        float segundosPorPasso = currentTarget != null ? currentTarget.SegundosPorPassoCom(kit)
                               : pontoAlvo != null ? pontoAlvo.SegundosParaUsarPara(kit)
                               : 0f;
        int passos = sessao.Atualizar(alvo, segurando && liberado, Time.deltaTime, segundosPorPasso);

        for (int i = 0; i < passos; i++)
        {
            if (!DarPasso())
                break;
        }

        Avancando = segurando && liberado && alvo != null;
    }

    /* Conferência do aperto (ou da troca de alvo segurando), sem avançar nem
     * gastar nada. true = o segurar fica liberado. Recusado, avisa. */
    private bool Conferir(bool apertoNovo)
    {
        if (currentTarget != null)
            return ConferirFoco(currentTarget, apertoNovo);

        if (pontoAlvo != null)
        {
            if (pontoAlvo.PodeUsar(kit, out string motivo))
                return true;

            AvisarRecusa(pontoAlvo, motivo);
            return false;
        }

        return false;
    }

    /* Conferência de um foco. Sem minigame, como sempre: aprovado, o segurar
     * fica liberado (true). Com minigame para este kit, só um aperto novo
     * conta: conferido, começa a sessão (e o segurar não é liberado). A mira
     * chegando com o botão já apertado não começa nada, não confere (sem
     * aviso, como o ralo antigo) e prende o segurar até soltar: no ralo, o
     * ponteiro andaria sem o jogador querer e o erro gastaria a tela. */
    internal bool ConferirFoco(Interactable foco, bool apertoNovo)
    {
        // Só a pergunta: CriarMinigame é uma fábrica pura e a sessão é descartada.
        bool temMinigame = foco.CriarMinigame(Kit, 0) != null;
        if (temMinigame && !apertoNovo)
            return false;

        ResultadoDoPasso resultado = foco.Verificar(Kit);
        if (resultado != ResultadoDoPasso.Avancou)
        {
            AvisarRecusa(foco, resultado);
            return false;
        }

        if (temMinigame)
        {
            IniciarMinigame(foco);
            return false;
        }
        return true;
    }

    /* Um passo no alvo atual. False = parar por aqui neste aperto ou alvo
     * (o foco concluiu ou foi pego, o ponto foi usado, ou houve recusa). */
    private bool DarPasso()
    {
        if (currentTarget != null)
        {
            ResultadoDoPasso resultado = currentTarget.AvancarCom(kit);
            switch (resultado)
            {
                case ResultadoDoPasso.Avancou:
                    return true;

                case ResultadoDoPasso.Concluiu:
                case ResultadoDoPasso.Pegou:
                    // O foco saiu da mira (concluído ou nos braços).
                    sessao.Zerar();
                    return false;

                case ResultadoDoPasso.Esvaziou:
                    // O foco continua na mira (a ferramenta certa ainda
                    // resolve): só um esvaziar por aperto, para segurar mais
                    // não emendar num aviso de JaVazio.
                    sessao.Zerar();
                    liberado = false;
                    return false;

                default:
                    // Recusa no meio do segurar (ex.: o saco encheu).
                    sessao.Zerar();
                    liberado = false;
                    AvisarRecusa(currentTarget, resultado);
                    return false;
            }
        }

        if (pontoAlvo != null)
        {
            // Confere de novo: o kit pode ter mudado desde o aperto.
            if (pontoAlvo.PodeUsar(kit, out string motivo))
                pontoAlvo.Usar(kit);
            else
                AvisarRecusa(pontoAlvo, motivo);

            // Um uso por aperto: segurar mais não repete o uso (nem mostra
            // "kit já está cheio" logo depois de reabastecer).
            sessao.Zerar();
            liberado = false;
            return false;
        }

        return false;
    }

    /* Só as recusas que a HUD explica; o Bloqueado passa em silêncio. */
    private void AvisarRecusa(Interactable foco, ResultadoDoPasso resultado)
    {
        if (Ferramentas.EhRecusa(resultado))
            OnRecusado?.Invoke(foco, resultado);
    }

    /* Motivo vazio = recusa sem aviso (ex.: sem kit). */
    private void AvisarRecusa(PontoDeUso ponto, string motivo)
    {
        if (!string.IsNullOrEmpty(motivo))
            OnPontoRecusado?.Invoke(ponto, motivo);
    }

    /* Barra da HUD: no foco, a conta dele (Interactable.ProgressoCom) com
     * a fração do passo em andamento (a do minigame, se houver um ativo). */
    private void AtualizarProgresso()
    {
        if (currentTarget != null)
        {
            float fracao = minigame != null ? minigame.FracaoDoPasso : sessao.Fracao;
            ProgressoContinuo = currentTarget.ProgressoCom(kit, fracao);
        }
        else if (pontoAlvo != null)
            ProgressoContinuo = sessao.Fracao;
        else
            ProgressoContinuo = 0f;
    }

    /* Troca o alvo mirado; mudou, desfaz o destaque no antigo e aplica no novo. */
    private void TrocarAlvo(Interactable foco, PontoDeUso ponto)
    {
        // Por referência: um alvo destruído também conta como mudança.
        if (ReferenceEquals(foco, currentTarget) && ReferenceEquals(ponto, pontoAlvo))
            return;

        RestoreHighlight();
        currentTarget = foco;
        pontoAlvo = ponto;

        if (foco != null)
            ApplyHighlight(foco);
        else if (ponto != null)
            ApplyHighlight(ponto);
    }

    /* Dispara um raio do centro da tela e acha o primeiro alvo válido, do
     * mais perto para o mais longe: um foco que ainda aceita passos (só com
     * as mãos livres) ou um ponto de uso. Foco concluído ou que não pode ser
     * alvo agora (ex.: vaso já com areia, pneu entregue encolhendo, qualquer
     * foco com o pneu nas mãos) é atravessado; qualquer outro collider
     * (parede, chão, muro) tapa a mira. */
    private void RaycastTarget(bool comPneu, out Interactable foco, out PontoDeUso ponto)
    {
        foco = null;
        ponto = null;

        if (cam == null)
            return;

        Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f));

        int mascara = camadaCorpoJogador >= 0 ? ~(1 << camadaCorpoJogador) : ~0;
        int quantos = Physics.RaycastNonAlloc(ray, acertos, interactionRange, mascara, QueryTriggerInteraction.Ignore);
        Array.Sort(acertos, 0, quantos, PorDistancia);

        for (int i = 0; i < quantos; i++)
        {
            Collider atingido = acertos[i].collider;

            Interactable achado = atingido.GetComponentInParent<Interactable>();
            if (achado != null)
            {
                if (!comPneu && achado.CanInteract)
                {
                    foco = achado;
                    return;
                }
                continue;   // foco fora de jogo: o raio passa por ele
            }

            PontoDeUso pontoAchado = atingido.GetComponentInParent<PontoDeUso>();
            if (pontoAchado != null)
                ponto = pontoAchado;
            return;   // ponto de uso, ou algo que tapa a mira
        }
    }

    /* Este Renderer brilha com este alvo mirado? O de um foco dentro de um
     * ponto de uso, não: o pneu empilhado na carroceria não pisca quando a
     * picape é mirada. */
    internal static bool EntraNoDestaque(Component alvo, Renderer rend)
    {
        if (!EmissionHighlightUtil.Destacavel(rend))
            return false;
        return !(alvo is PontoDeUso) || rend.GetComponentInParent<Interactable>() == null;
    }

    /* Aplica emissão pulsante em todos os Renderer do alvo (e filhos),
     * menos as partículas (o enxame de mosquitos: sem _EmissionColor, e o
     * material dele é compartilhado; ver EmissionHighlightUtil.Destacavel). */
    private void ApplyHighlight(Component target)
    {
        foreach (Renderer rend in target.GetComponentsInChildren<Renderer>())
        {
            if (!EntraNoDestaque(target, rend))
                continue;

            // Instancia o material para não afetar outros usuários do mesmo asset.
            Material mat = rend.material;
            highlighted.Add(new HighlightEntry
            {
                renderer = rend,
                material = mat,
                originalEmissionColor = mat.GetColor("_EmissionColor"),
                emissionEnabled = mat.IsKeywordEnabled("_EMISSION")
            });

            mat.EnableKeyword("_EMISSION");
        }
    }

    /* Pulsa a cor de emissão de cada material destacado a cada frame. */
    private void UpdateHighlightPulse()
    {
        if (highlighted.Count == 0)
            return;

        float intensity = highlightIntensity * (0.75f + 0.25f * Mathf.Sin(Time.time * highlightPulseSpeed));
        Color emission = highlightColor * intensity;

        for (int i = highlighted.Count - 1; i >= 0; i--)
        {
            if (highlighted[i].renderer == null)
            {
                highlighted.RemoveAt(i);
                continue;
            }
            highlighted[i].material.SetColor("_EmissionColor", emission);
        }
    }

    /* Devolve cada material ao estado original (emissão/cor/ativado). */
    private void RestoreHighlight()
    {
        foreach (HighlightEntry entry in highlighted)
        {
            if (entry.renderer == null || entry.material == null)
                continue;

            if (entry.emissionEnabled)
                entry.material.SetColor("_EmissionColor", entry.originalEmissionColor);
            else
                entry.material.DisableKeyword("_EMISSION");
        }

        highlighted.Clear();
    }
}
