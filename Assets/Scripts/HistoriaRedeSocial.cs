using System.Collections.Generic;
using UnityEngine;

/* ============================================================
 *  HistoriaRedeSocial.cs
 *  ---------------------------------------------------------------------
 *  O NARRADOR do jogo. A história é contada pelo celular: tudo o que o
 *  jogador faz vira uma publicação de um personagem do bairro sobre
 *  AQUELA ação — não mais um incentivo genérico a cada N focos.
 *
 *  Cada tipo de foco tem um "dono" (PersonagemDoBairro) que posta quando:
 *    - o jogador chega perto de um foco desse tipo pela primeira vez;
 *    - um foco desse tipo é eliminado (fala do primeiro, das seguintes e
 *      uma comemoração quando é o último do tipo);
 *    - o jogador começa a limpar um foco e vai embora deixando pela metade
 *      (o pneu já esvaziado, largado para levar à picape da UPA depois,
 *      e as garrafas com o saco cheio, a caminho de esvaziar numa
 *      lixeira, não contam);
 *    - o jogador tenta um foco desse tipo com a ferramenta errada (uma
 *      vez por tipo — Interactor.OnRecusado).
 *
 *  Além disso, outros momentos da partida viram post: sair do hospital,
 *  demorar sem eliminar foco, ficar parado, limpar dois focos seguidos,
 *  pular à toa, a ocupação do hospital passar de certos limites e a
 *  derrota. No fim, LinhasEpilogo() monta o "uma semana depois" com a
 *  fala de cada personagem, que muda conforme o que o jogador limpou.
 *
 *  Kit do agente (Parte A): o primeiro reabastecimento na picape da UPA
 *  (o monte de areia não conta), o agente carregando pneu e o primeiro
 *  saco de garrafas na lixeira também viram post (KitDoAgente.
 *  OnReabasteceu / OnPneuPego / OnSacoEsvaziado). Sem KitDoAgente ou sem
 *  Interactor na cena, esses posts só não saem.
 *
 *  Chuva e atalho (Parte B):
 *    - cada previsão de chuva (Chuva.OnPrevisao) dá um post, sorteado
 *      sem repetir o da previsão anterior — é o único aviso da chuva, por
 *      isso sai com prioridade Alta e, se a janela de conflito recusar,
 *      fica pendente e tenta de novo até começar a chover;
 *    - o primeiro foco esvaziado pelo atalho das Mãos (GerenciadorDeFocos.
 *      OnFocoEsvaziado; vaso e garrafas — o pneu esvaziado é o caminho
 *      normal dele, não o atalho, e não conta) lembra que os ovos ficam e
 *      a chuva traz de volta;
 *    - o primeiro foco reenchido pela chuva (OnFocoReenchido) dá o post
 *      do tipo dele (aoReencherPrimeiraVez, um por tipo; um só na
 *      partida);
 *    - o primeiro pneu entregue na caçamba da picape da UPA também vira
 *      post: sai no próprio HandleFocoEliminado (a entrega é o que elimina
 *      o pneu), no lugar da fala do dono.
 *  Sem Chuva na cena, a previsão só não sai.
 *
 *  Quem publica de fato é a RedeSocial.cs (avisos, não lidos). Ao nascer,
 *  este componente liga RedeSocial.NarradorExterno: a RedeSocial passa a
 *  soltar sozinha só a abertura, e o resto sai daqui.
 *
 *  Cada mensagem sai NA HORA do gatilho ou é recusada (outro aviso tão ou
 *  mais importante acabou de sair — RedeSocial.Aceitaria). Recusada, ela
 *  não é gasta e só volta quando o gatilho dela acontecer DE NOVO. Um foco
 *  eliminado gera UMA mensagem: finais > primeiro pneu na picape > último
 *  do tipo > sequência rápida > fala do dono.
 *
 *  Marcadores que dá pra usar nos textos daqui (trocados na hora de
 *  postar): {restantesTipo} {eliminadosTipo} {totalTipo} {ocupacao}.
 *  Os marcadores do feed ({eliminados} {total} {restantes}) continuam
 *  valendo e são trocados na hora da publicação, como sempre.
 *
 *  Os focos vêm do GerenciadorDeFocos (do campo ou, vazio,
 *  GerenciadorDeFocos.Obter()), como no DengueHUD/RedeSocial/FocoListaHUD/
 *  FluxoDeJogo; cada eliminação, do OnFocoEliminado dele.
 *
 *  Tudo é editável pelo Inspector (objeto "Historia"), sem tocar em código.
 *  Criado pelo menu  Dengue > Configurar Historia  (HistoriaBuilder.cs).
 * ============================================================ */

public enum TipoFoco
{
    Nenhum,
    Pneu,
    Vaso,
    CaixaDagua,
    Piscina,
    Garrafas,
    // Novos tipos sempre no FIM: o enum é salvo como número na cena.
    Ralo,
    Bromelia
}

[System.Serializable]
public class PersonagemDoBairro
{
    public string autor = "Vizinho";
    public string usuario = "@vizinho";

    [Tooltip("Tipo de foco que este personagem comenta.")]
    public TipoFoco tipo = TipoFoco.Nenhum;

    [Tooltip("Primeira vez que o jogador chega perto de um foco deste tipo.")]
    [TextArea(2, 4)] public string aoChegarPerto = "";

    [Tooltip("Um foco deste tipo eliminado. O 1º texto é do primeiro foco; depois segue a ordem e, quando acabar, sorteia.")]
    [TextArea(2, 4)] public List<string> aoEliminar = new List<string>();

    [Tooltip("O ÚLTIMO foco deste tipo eliminado (só quando o tipo tem mais de um foco).")]
    [TextArea(2, 4)] public string aoEliminarUltimo = "";

    [Tooltip("O jogador começou a limpar um foco deste tipo e foi embora deixando pela metade.")]
    [TextArea(2, 4)] public string aoLargarPelaMetade = "";

    [Tooltip("Primeira vez que o jogador tenta um foco deste tipo com a ferramenta errada. Vazio = o texto padrão do tipo.")]
    [TextArea(2, 4)] public string aoUsarFerramentaErrada = "";

    [Tooltip("Epílogo (\"uma semana depois\") se TODOS os focos deste tipo foram eliminados.")]
    [TextArea(2, 4)] public string epilogoLimpo = "";

    [Tooltip("Epílogo se sobrou algum foco deste tipo.")]
    [TextArea(2, 4)] public string epilogoPendente = "";
}

[System.Serializable]
public class MarcoDeOcupacao
{
    [Range(0f, 1f)] public float limite = 0.85f;

    [Tooltip("true = posta quando a ocupação SOBE e passa do limite; false = quando DESCE abaixo dele.")]
    public bool subindo = true;

    public PostRedeSocial post = new PostRedeSocial();
}

/* Um post ligado a um tipo de foco (o primeiro reenchido escolhe pelo tipo). */
[System.Serializable]
public class PostPorTipo
{
    [Tooltip("Tipo de foco a que este post responde.")]
    public TipoFoco tipo = TipoFoco.Nenhum;

    [Tooltip("O post que sai quando o gatilho é de um foco deste tipo.")]
    public PostRedeSocial post = new PostRedeSocial();
}

public class HistoriaRedeSocial : MonoBehaviour
{
    [Header("Referências (vazias = procura na cena)")]
    [SerializeField] private RedeSocial rede;
    [SerializeField] private FluxoDeJogo fluxo;
    [SerializeField] private CelularController celular;
    [SerializeField] private ControladorJogador jogador;

    [Tooltip("Interactor do jogador: avisa quando um foco recusa a ferramenta selecionada.")]
    [SerializeField] private Interactor interactor;

    [Tooltip("Kit do agente: avisa o reabastecimento, o pneu pego, o pneu entregue na picape e o saco esvaziado na lixeira.")]
    [SerializeField] private KitDoAgente kit;

    [Tooltip("Relógio da chuva: avisa a previsão. Vazio = procura a Chuva na cena.")]
    [SerializeField] private Chuva chuva;

    [Tooltip("Focos da partida (tipos, eliminados). Vazio = GerenciadorDeFocos.Obter().")]
    [SerializeField] private GerenciadorDeFocos gerenciador;

    [Header("Personagens (um por tipo de foco)")]
    [SerializeField] private List<PersonagemDoBairro> personagens = new List<PersonagemDoBairro>();

    [Header("Momentos da história")]
    [Tooltip("Saem quando o jogador cruza a porta do hospital (em ordem).")]
    [SerializeField] private List<PostRedeSocial> aoSairDoHospital = new List<PostRedeSocial>();

    [Tooltip("Um por vez, quando passa muito tempo sem eliminar foco.")]
    [SerializeField] private List<PostRedeSocial> aoDemorarSemFoco = new List<PostRedeSocial>();

    [Tooltip("Um por vez, quando o jogador fica parado muito tempo (com o celular fechado).")]
    [SerializeField] private List<PostRedeSocial> aoFicarParado = new List<PostRedeSocial>();

    [Tooltip("Um por vez, quando dois focos são eliminados em sequência rápida.")]
    [SerializeField] private List<PostRedeSocial> aoFazerCombo = new List<PostRedeSocial>();

    [Tooltip("Um por vez, quando o jogador pula várias vezes seguidas.")]
    [SerializeField] private List<PostRedeSocial> aoPularMuito = new List<PostRedeSocial>();

    [Tooltip("Foco eliminado de um tipo sem personagem (fallback, em ciclo).")]
    [SerializeField] private List<PostRedeSocial> aoEliminarFocoSemDono = new List<PostRedeSocial>();

    [Tooltip("A ocupação do hospital passando de certos limites (cada marco sai uma vez).")]
    [SerializeField] private List<MarcoDeOcupacao> marcosDeOcupacao = new List<MarcoDeOcupacao>();

    [Tooltip("Saem na derrota (ocupação em 100%).")]
    [SerializeField] private List<PostRedeSocial> postsDerrota = new List<PostRedeSocial>();

    [Tooltip("Sai no primeiro reabastecimento do kit na picape da UPA (o monte de areia não conta). Com mais de um post: um por reabastecimento na picape, em ordem, até acabar.")]
    [SerializeField] private List<PostRedeSocial> aoReabastecerPrimeiraVez = new List<PostRedeSocial>();

    [Tooltip("Sai quando o agente pega um pneu pela primeira vez. Com mais de um post: um por pneu pego, em ordem, até acabar.")]
    [SerializeField] private List<PostRedeSocial> aoCarregarPneu = new List<PostRedeSocial>();

    [Tooltip("Sai quando o primeiro saco de garrafas é esvaziado na lixeira. Com mais de um post: um por saco, em ordem, até acabar.")]
    [SerializeField] private List<PostRedeSocial> aoEsvaziarSacoPrimeiraVez = new List<PostRedeSocial>();

    [Tooltip("Sai quando o primeiro pneu é entregue na caçamba da picape da UPA. Com mais de um post: um por pneu, em ordem, até acabar.")]
    [SerializeField] private List<PostRedeSocial> aoEntregarPneuPrimeiraVez = EntregarPneuPadrao();

    [Header("Chuva e atalho de esvaziar (Parte B)")]
    [Tooltip("Um por previsão de chuva, sorteado sem repetir o da previsão anterior. É o único aviso da chuva: sai com prioridade Alta e, recusado, tenta de novo até começar a chover.")]
    [SerializeField] private List<PostRedeSocial> aoPreverChuva = PrevisaoPadrao();

    [Tooltip("Sai no primeiro foco esvaziado pelo atalho das Mãos (vaso e garrafas; o pneu esvaziado é o caminho normal dele e não conta). Com mais de um post: um por esvaziado, em ordem, até acabar.")]
    [SerializeField] private List<PostRedeSocial> aoEsvaziarPrimeiraVez = EsvaziarPadrao();

    [Tooltip("O primeiro foco reenchido pela chuva: sai o post do tipo dele (um post só na partida). Tipo sem post aqui = não posta e espera o próximo reenchido.")]
    [SerializeField] private List<PostPorTipo> aoReencherPrimeiraVez = ReencherPadrao();

    [Header("Gatilhos")]
    [Tooltip("Distância (m) que conta como \"chegou perto\" de um foco.")]
    [SerializeField] private float raioChegada = 8f;

    [Tooltip("Distância (m) que conta como \"foi embora\" de um foco começado.")]
    [SerializeField] private float raioLargado = 12f;

    [Tooltip("Segundos sem eliminar foco até alguém cobrar.")]
    [SerializeField] private float tempoSemFoco = 60f;

    [Tooltip("Segundos parado (com o celular fechado) até alguém estranhar.")]
    [SerializeField] private float tempoParado = 35f;

    [Tooltip("Dois focos eliminados dentro desta janela (s) contam como sequência rápida.")]
    [SerializeField] private float janelaCombo = 20f;

    [Tooltip("Quantos pulos dentro da janela abaixo viram post.")]
    [SerializeField] private int pulosParaPost = 4;
    [SerializeField] private float janelaPulos = 6f;

    private readonly List<Interactable> focos = new List<Interactable>();
    private readonly Dictionary<Interactable, TipoFoco> tipoDe = new Dictionary<Interactable, TipoFoco>();
    private readonly Dictionary<TipoFoco, int> totalPorTipo = new Dictionary<TipoFoco, int>();
    private readonly Dictionary<TipoFoco, int> eliminadosPorTipo = new Dictionary<TipoFoco, int>();
    private readonly Dictionary<TipoFoco, int> proximaFala = new Dictionary<TipoFoco, int>();
    private readonly Dictionary<TipoFoco, int> ultimaFalaSorteada = new Dictionary<TipoFoco, int>();
    private readonly HashSet<TipoFoco> jaChegouPerto = new HashSet<TipoFoco>();
    private readonly HashSet<TipoFoco> ferramentaErradaAvisada = new HashSet<TipoFoco>();
    private readonly HashSet<Interactable> largadoAvisado = new HashSet<Interactable>();
    private readonly HashSet<Interactable> dentroDoRaio = new HashSet<Interactable>();     // p/ "chegar perto" (gatilho = ENTRAR)
    private readonly HashSet<Interactable> pertoDoComecado = new HashSet<Interactable>();  // p/ "largar" (gatilho = SAIR)
    private readonly Queue<float> pulos = new Queue<float>();

    private Transform player;
    private Vector3 ultimaPosicaoPlayer;
    private int eliminadosTotal;
    private int idxDemora, idxParado, idxCombo, idxPulos, idxSemDono;
    private int idxReabastecer, idxCarregarPneu, idxEsvaziarSaco;
    private int idxEntregarPneu, idxEsvaziar;
    private int ultimaPrevisao = -1;   // índice do último post de previsão que saiu
    private int previsaoPendente = -1; // índice da previsão recusada que ainda vai sair (-1 = nenhuma)
    private float proximaTentativaPrevisao;
    private bool reenchidoPostado;
    private readonly System.Random sorteioPrevisao = new System.Random();
    private bool[] marcoArmado;
    private bool[] marcoDisparado;
    private float timerSemFoco;
    private float timerParado;
    private float ultimoFocoTempo = float.NegativeInfinity;
    private float proximaVarredura;

    // Regra das mensagens (pedido do usuário): sai NA HORA ou é recusada pela
    // RedeSocial (outro aviso tão ou mais importante acabou de sair). Recusada
    // = a mensagem não é gasta e só volta quando o gatilho dela acontecer DE
    // NOVO — nunca sai sozinha depois. Exceção: a previsão de chuva, que é o
    // único aviso da chuva (OnPrevisao sai uma vez por chuva) — recusada, ela
    // fica pendente e tenta de novo enquanto a chuva estiver em previsão.
    private bool EmJogo => fluxo != null && fluxo.PartidaIniciada && !fluxo.PartidaEncerrada;

    void Reset()
    {
        PreencherPadroes(true);
    }

    void Awake()
    {
        PreencherPadroes(false);

        if (rede == null) rede = FindFirstObjectByType<RedeSocial>();
        if (fluxo == null) fluxo = FindFirstObjectByType<FluxoDeJogo>();
        if (celular == null) celular = FindFirstObjectByType<CelularController>();
        if (jogador == null) jogador = FindFirstObjectByType<ControladorJogador>();

        // FindAnyObjectByType: o FindFirstObjectByType está obsoleto nesta
        // versão do Unity (aviso CS0618) e só existe um de cada. Sem eles na
        // cena (kit ainda não montado), os posts do kit só não saem.
        if (interactor == null) interactor = FindAnyObjectByType<Interactor>();
        if (kit == null) kit = FindAnyObjectByType<KitDoAgente>();
        if (chuva == null) chuva = FindAnyObjectByType<Chuva>();

        // Awake roda antes de qualquer Start: a RedeSocial já nasce sabendo
        // que incentivos e finais são decididos aqui.
        if (rede != null)
            rede.NarradorExterno = true;
    }

    void Start()
    {
        player = jogador != null ? jogador.transform : FindFirstObjectByType<CharacterController>()?.transform;
        if (player != null)
            ultimaPosicaoPlayer = player.position;

        // Os focos da partida (inclusive desativados), vindos do gerenciador.
        if (gerenciador == null) gerenciador = GerenciadorDeFocos.Obter();
        if (gerenciador != null)
        {
            gerenciador.Garantir();
            foreach (Interactable foco in gerenciador.Focos)
            {
                if (foco == null)
                    continue;

                TipoFoco tipo = TipoDoFoco(foco);
                focos.Add(foco);
                tipoDe[foco] = tipo;
                totalPorTipo[tipo] = Contar(totalPorTipo, tipo) + 1;

                if (foco.IsComplete)
                {
                    eliminadosTotal++;
                    eliminadosPorTipo[tipo] = Contar(eliminadosPorTipo, tipo) + 1;
                }
            }
            gerenciador.OnFocoEliminado += HandleFocoEliminado;
            gerenciador.OnFocoEsvaziado += HandleFocoEsvaziado;
            gerenciador.OnFocoReenchido += HandleFocoReenchido;
        }

        marcoArmado = new bool[marcosDeOcupacao.Count];
        marcoDisparado = new bool[marcosDeOcupacao.Count];

        if (fluxo != null)
        {
            fluxo.OnPartidaIniciada += HandlePartidaIniciada;
            fluxo.OnDerrota += HandleDerrota;
        }
        if (jogador != null)
            jogador.OnPulou += HandlePulou;
        if (interactor != null)
            interactor.OnRecusado += HandleRecusado;
        if (kit != null)
        {
            kit.OnReabasteceu += HandleReabasteceu;
            kit.OnPneuPego += HandlePneuPego;
            kit.OnSacoEsvaziado += HandleSacoEsvaziado;
        }
        if (chuva != null)
        {
            chuva.OnPrevisao += HandlePrevisaoChuva;
            chuva.OnComecou += HandleChuvaComecou;
        }
    }

    void OnDestroy()
    {
        // Só pelo campo: procurar ou criar um gerenciador aqui (fim do Play,
        // troca de cena) deixaria objeto novo para trás.
        if (gerenciador != null)
        {
            gerenciador.OnFocoEliminado -= HandleFocoEliminado;
            gerenciador.OnFocoEsvaziado -= HandleFocoEsvaziado;
            gerenciador.OnFocoReenchido -= HandleFocoReenchido;
        }

        if (fluxo != null)
        {
            fluxo.OnPartidaIniciada -= HandlePartidaIniciada;
            fluxo.OnDerrota -= HandleDerrota;
        }
        if (jogador != null)
            jogador.OnPulou -= HandlePulou;
        if (interactor != null)
            interactor.OnRecusado -= HandleRecusado;
        if (kit != null)
        {
            kit.OnReabasteceu -= HandleReabasteceu;
            kit.OnPneuPego -= HandlePneuPego;
            kit.OnSacoEsvaziado -= HandleSacoEsvaziado;
        }
        if (chuva != null)
        {
            chuva.OnPrevisao -= HandlePrevisaoChuva;
            chuva.OnComecou -= HandleChuvaComecou;
        }
    }

    void Update()
    {
        float dt = Time.deltaTime;
        float velocidade = VelocidadeDoPlayer(dt);

        if (!EmJogo || dt <= 0f)
            return;

        VerificarOcupacao();

        // Previsão de chuva recusada pela janela de conflito: tenta de novo
        // a cada 1 s enquanto a chuva ainda está em previsão.
        if (previsaoPendente >= 0 && Time.time >= proximaTentativaPrevisao)
        {
            proximaTentativaPrevisao = Time.time + 1f;
            TentarPrevisaoPendente();
        }

        if (Time.time >= proximaVarredura)
        {
            proximaVarredura = Time.time + 0.25f;
            VerificarChegadas();
            VerificarLargados();
        }

        // Ler o celular não conta como "enrolar": os dois timers abaixo
        // param enquanto ele está aberto.
        bool lendoCelular = celular != null && celular.Aberto;

        // Muito tempo sem eliminar foco: alguém do bairro cobra.
        if (!lendoCelular)
            timerSemFoco += dt;
        if (timerSemFoco >= tempoSemFoco)
        {
            PostarDaLista(aoDemorarSemFoco, ref idxDemora, PrioridadePost.Baixa);
            timerSemFoco = 0f;   // saiu ou foi recusada: o gatilho foi gasto, só volta daqui a tempoSemFoco
        }

        // Parado: alguém estranha.
        if (velocidade < 0.15f && !lendoCelular)
            timerParado += dt;
        else
            timerParado = 0f;

        if (timerParado >= tempoParado)
        {
            PostarDaLista(aoFicarParado, ref idxParado, PrioridadePost.Baixa);
            timerParado = 0f;
        }
    }

    /* ---------------- API ---------------- */

    /* "Uma semana depois": uma fala por personagem que tem foco na cena.
     * Vitória = todos com epilogoLimpo; derrota = só os tipos que ficaram
     * pendentes (é o que o jogador deixou para trás que marca o fim). */
    public List<string> LinhasEpilogo(bool vitoria)
    {
        List<string> linhas = new List<string>();
        foreach (PersonagemDoBairro p in personagens)
        {
            if (p == null || p.tipo == TipoFoco.Nenhum)
                continue;

            int total = Contar(totalPorTipo, p.tipo);
            if (total == 0)
                continue;

            bool limpo = Contar(eliminadosPorTipo, p.tipo) >= total;
            if (!vitoria && limpo)
                continue;

            string fala = limpo ? p.epilogoLimpo : p.epilogoPendente;
            if (!string.IsNullOrWhiteSpace(fala))
                linhas.Add(FormatarComoPost(p.autor, p.usuario, fala));
        }
        return linhas;
    }

    /* Texto de cartela com cara de post: autor e @ pequenos em cima. */
    public static string FormatarComoPost(string autor, string usuario, string texto)
    {
        return $"<size=55%><color=#FFFFFF99><b>{autor}</b>  {usuario}</color></size>\n\"{texto}\"";
    }

    /* ---------------- Eventos ---------------- */

    private void HandlePartidaIniciada()
    {
        timerSemFoco = 0f;
        if (rede != null)
            rede.PostarGrupo(Formatados(aoSairDoHospital), PrioridadePost.Alta);
    }

    private void HandleDerrota()
    {
        if (rede != null)
            rede.PostarGrupo(Formatados(postsDerrota), PrioridadePost.Maxima);
    }

    /* UMA mensagem por foco eliminado — a mais importante. As outras não são
     * gastas: ficam para a próxima vez que o gatilho delas acontecer. */
    private void HandleFocoEliminado(Interactable foco)
    {
        TipoFoco tipo = tipoDe.TryGetValue(foco, out TipoFoco t) ? t : TipoFoco.Nenhum;
        eliminadosTotal++;
        eliminadosPorTipo[tipo] = Contar(eliminadosPorTipo, tipo) + 1;
        jaChegouPerto.Add(tipo);   // não faz sentido "estou vendo o agente chegando" depois de limpo
        timerSemFoco = 0f;

        bool sequencia = Time.time - ultimoFocoTempo <= janelaCombo;
        ultimoFocoTempo = Time.time;

        // 1. Bairro limpo: publicações finais da RedeSocial.
        if (eliminadosTotal >= focos.Count)
        {
            if (rede != null)
                rede.PostarGrupo(rede.PostsFinais, PrioridadePost.Maxima);
            return;
        }

        // Pneu só é eliminado na entrega: o primeiro dá o post da picape, e
        // a fala do dono fica para o próximo. Recusado, não é gasto.
        if (foco is PneuInteractable && PostarDaLista(aoEntregarPneuPrimeiraVez, ref idxEntregarPneu, PrioridadePost.Alta))
            return;

        PersonagemDoBairro dono = Personagem(tipo);
        int total = Contar(totalPorTipo, tipo);
        bool ultimoDoTipo = Contar(eliminadosPorTipo, tipo) >= total;

        // 2. Último foco daquele tipo: o dono fecha a história dele.
        if (dono != null && ultimoDoTipo && total > 1 && !string.IsNullOrWhiteSpace(dono.aoEliminarUltimo))
        {
            Postar(dono.autor, dono.usuario, dono.aoEliminarUltimo, HumorPost.Positivo, tipo, -1, PrioridadePost.Alta);
            return;
        }

        // 3. Sequência rápida (a fala do dono fica guardada para o próximo foco do tipo).
        if (sequencia && PostarDaLista(aoFazerCombo, ref idxCombo, PrioridadePost.Alta))
            return;

        // 4. O dono do tipo comenta ESTE foco — a fala só é gasta se sair.
        if (dono != null && TemFalaAoEliminar(dono))
        {
            if (rede != null && rede.Aceitaria(PrioridadePost.Alta))
                Postar(dono.autor, dono.usuario, FalaAoEliminar(dono, tipo), HumorPost.Positivo, tipo, -1, PrioridadePost.Alta);
            return;
        }

        PostarDaLista(aoEliminarFocoSemDono, ref idxSemDono, PrioridadePost.Normal, emCiclo: true);
    }

    private void HandlePulou()
    {
        if (!EmJogo)
            return;

        pulos.Enqueue(Time.time);
        while (pulos.Count > 0 && Time.time - pulos.Peek() > janelaPulos)
            pulos.Dequeue();

        if (pulos.Count >= pulosParaPost)
        {
            PostarDaLista(aoPularMuito, ref idxPulos, PrioridadePost.Baixa);
            pulos.Clear();   // saiu ou foi recusada: precisa de outra série de pulos
        }
    }

    /* Ferramenta errada num foco: o dono do tipo comenta, uma vez por tipo.
     * Recusada pela RedeSocial, o tipo não é marcado e volta na próxima
     * tentativa errada. As outras recusas (sem carga, saco cheio, mãos
     * ocupadas) a HUD já explica e não viram post. */
    private void HandleRecusado(Interactable foco, ResultadoDoPasso resultado)
    {
        if (!EmJogo || foco == null || resultado != ResultadoDoPasso.FerramentaErrada)
            return;

        TipoFoco tipo = TipoDoFoco(foco);
        if (ferramentaErradaAvisada.Contains(tipo))
            return;

        PersonagemDoBairro dono = Personagem(tipo);   // tipo sem dono (Nenhum) = não posta
        if (dono != null && !string.IsNullOrWhiteSpace(dono.aoUsarFerramentaErrada)
            && Postar(dono.autor, dono.usuario, dono.aoUsarFerramentaErrada, HumorPost.Neutro, tipo, -1, PrioridadePost.Normal))
            ferramentaErradaAvisada.Add(tipo);
    }

    /* Os três abaixo saem um por gatilho, da lista do Inspector: o índice
     * só anda se o post sair (recusado = volta no próximo gatilho). Com o
     * padrão de um post por lista, cada um sai uma vez na partida. O de
     * reabastecer fala da picape da UPA: só a picape é gatilho (o monte de
     * areia do terreno baldio não). */
    private void HandleReabasteceu(PontoDeReabastecimento origem)
    {
        if (EmJogo && origem != null && origem.EhPicapeDaUpa)
            PostarDaLista(aoReabastecerPrimeiraVez, ref idxReabastecer, PrioridadePost.Normal);
    }

    private void HandlePneuPego(PneuInteractable pneu)
    {
        if (EmJogo)
            PostarDaLista(aoCarregarPneu, ref idxCarregarPneu, PrioridadePost.Normal);
    }

    private void HandleSacoEsvaziado()
    {
        if (EmJogo)
            PostarDaLista(aoEsvaziarSacoPrimeiraVez, ref idxEsvaziarSaco, PrioridadePost.Normal);
    }

    /* Parte B. Os métodos abaixo são internal para os testes EditMode
     * chamarem direto (o Start, que assina os eventos, e o Update, que
     * tenta de novo a previsão pendente, não rodam lá). */

    /* Previsão de chuva: um post por previsão, sorteado entre os da lista
     * sem repetir o da previsão anterior (com um post só, ele repete).
     * É o único aviso da chuva: recusado pela janela de conflito, o post
     * sorteado fica pendente (o "anterior" ainda não muda) e o Update
     * tenta de novo a cada 1 s (TentarPrevisaoPendente) enquanto a chuva
     * estiver em previsão. Uma previsão nova substitui a pendente. */
    internal void HandlePrevisaoChuva()
    {
        previsaoPendente = -1;
        if (!EmJogo || aoPreverChuva == null || aoPreverChuva.Count == 0)
            return;

        int n = aoPreverChuva.Count;
        int i;
        if (n == 1 || ultimaPrevisao < 0 || ultimaPrevisao >= n)
            i = sorteioPrevisao.Next(n);
        else
        {
            // Sorteia entre os n - 1 que não são o anterior: pula o índice dele.
            i = sorteioPrevisao.Next(n - 1);
            if (i >= ultimaPrevisao)
                i++;
        }

        PostRedeSocial p = aoPreverChuva[i];
        if (p == null)
            return;
        if (Postar(p.autor, p.usuario, p.texto, p.humor, TipoFoco.Nenhum, p.curtidas, PrioridadePost.Alta))
            ultimaPrevisao = i;
        else
        {
            previsaoPendente = i;
            proximaTentativaPrevisao = Time.time + 1f;
        }
    }

    /* Nova tentativa da previsão recusada. Só vale enquanto a chuva está
     * em previsão: choveu (ou acabou a partida, ou não há Chuva para
     * conferir), a pendente é descartada. */
    internal void TentarPrevisaoPendente()
    {
        if (previsaoPendente < 0)
            return;

        if (!EmJogo || chuva == null || chuva.Estado != EstadoDaChuva.Previsao
            || aoPreverChuva == null || previsaoPendente >= aoPreverChuva.Count
            || aoPreverChuva[previsaoPendente] == null)
        {
            previsaoPendente = -1;
            return;
        }

        PostRedeSocial p = aoPreverChuva[previsaoPendente];
        if (Postar(p.autor, p.usuario, p.texto, p.humor, TipoFoco.Nenhum, p.curtidas, PrioridadePost.Alta))
        {
            ultimaPrevisao = previsaoPendente;
            previsaoPendente = -1;
        }
    }

    /* Começou a chover: a previsão que não saiu perdeu a hora. */
    internal void HandleChuvaComecou()
    {
        previsaoPendente = -1;
    }

    /* Foco esvaziado pelo atalho das Mãos (vaso e garrafas): avisa, da
     * lista, que os ovos ficam e a chuva traz de volta. O pneu esvaziado
     * NÃO conta: tirar a água do pneu é o caminho normal da ação
     * definitiva (esvaziar e levar pra picape), não o atalho — o DengueHUD
     * faz a mesma distinção. Ele é ignorado sem gastar o gatilho. */
    internal void HandleFocoEsvaziado(Interactable foco)
    {
        if (!EmJogo || foco is PneuInteractable)
            return;
        PostarDaLista(aoEsvaziarPrimeiraVez, ref idxEsvaziar, PrioridadePost.Normal);
    }

    /* Foco reenchido pela chuva: o primeiro da partida dá o post do tipo
     * dele. Só fica "gasto" se o post sair (tipo sem post na lista ou
     * recusado = espera o próximo reenchido). */
    internal void HandleFocoReenchido(Interactable foco)
    {
        if (!EmJogo || foco == null || reenchidoPostado || aoReencherPrimeiraVez == null)
            return;

        TipoFoco tipo = TipoDoFoco(foco);
        PostPorTipo doTipo = aoReencherPrimeiraVez.Find(x => x != null && x.tipo == tipo && x.post != null);
        if (doTipo == null)
            return;

        PostRedeSocial p = doTipo.post;
        if (Postar(p.autor, p.usuario, p.texto, p.humor, tipo, p.curtidas, PrioridadePost.Normal))
            reenchidoPostado = true;
    }

    /* ---------------- Verificações periódicas ---------------- */

    /* Gatilho = ENTRAR no raio de um foco (não "estar perto"). Recusada, só
     * tenta de novo quando o jogador sair e entrar outra vez. */
    private void VerificarChegadas()
    {
        if (player == null)
            return;

        foreach (Interactable foco in focos)
        {
            if (foco == null)
                continue;

            bool dentro = foco.CanInteract && foco.gameObject.activeInHierarchy
                          && Vector3.Distance(player.position, foco.transform.position) <= raioChegada;
            if (!dentro)
            {
                dentroDoRaio.Remove(foco);
                continue;
            }
            if (!dentroDoRaio.Add(foco))
                continue;   // já estava dentro: não é um gatilho novo

            TipoFoco tipo = tipoDe[foco];
            if (tipo == TipoFoco.Nenhum || jaChegouPerto.Contains(tipo))
                continue;

            PersonagemDoBairro dono = Personagem(tipo);
            if (dono != null && !string.IsNullOrWhiteSpace(dono.aoChegarPerto)
                && Postar(dono.autor, dono.usuario, dono.aoChegarPerto, HumorPost.Neutro, tipo, -1, PrioridadePost.Normal))
                jaChegouPerto.Add(tipo);
        }
    }

    /* Gatilho = SAIR de perto de um foco começado. Recusada, só tenta de
     * novo quando o jogador voltar para perto e sair outra vez. Sair por
     * ordem do próprio jogo não conta (SaidaObrigatoria). */
    private void VerificarLargados()
    {
        if (player == null)
            return;

        foreach (Interactable foco in focos)
        {
            if (foco == null || foco.CurrentClicks == 0 || foco.IsComplete || largadoAvisado.Contains(foco)
                || SaidaObrigatoria(foco))
            {
                if (foco != null) pertoDoComecado.Remove(foco);
                continue;
            }

            if (Vector3.Distance(player.position, foco.transform.position) < raioLargado)
            {
                pertoDoComecado.Add(foco);
                continue;
            }
            if (!pertoDoComecado.Remove(foco))
                continue;   // já estava longe: não é uma saída nova

            TipoFoco tipo = tipoDe[foco];
            PersonagemDoBairro dono = Personagem(tipo);
            if (dono != null && !string.IsNullOrWhiteSpace(dono.aoLargarPelaMetade)
                && Postar(dono.autor, dono.usuario, dono.aoLargarPelaMetade, HumorPost.Negativo, tipo, -1, PrioridadePost.Normal))
                largadoAvisado.Add(foco);
        }
    }

    /* Sair de perto deste foco começado agora é o que o jogo manda, e não
     * abandono:
     *   - o pneu já esvaziado: largado (ou nos braços) a caminho da picape
     *     da UPA não está "pela metade" — a água já saiu;
     *   - as garrafas com o saco cheio: o aviso manda esvaziar numa lixeira.
     *     Enquanto o saco está cheio o grupo sai do pertoDoComecado; ao
     *     voltar com espaço e sair de novo deixando garrafas, aí é
     *     abandono, e o post sai. */
    private bool SaidaObrigatoria(Interactable foco)
    {
        if (foco is PneuInteractable pneu)
            return pneu.Esvaziado;
        if (foco is GarrafaInteractable)
            return kit != null && kit.Estoque.SacoCheio;
        return false;
    }

    /* Um marco só dispara depois de "armado": a ocupação precisa ter estado
     * do outro lado do limite antes. Assim um marco de 70% não sai sozinho
     * se a partida já começa em 70%, e o de "caiu abaixo de 40%" não sai se
     * a ocupação nunca esteve acima disso. */
    private void VerificarOcupacao()
    {
        float ocupacao = fluxo.Ocupacao;
        for (int i = 0; i < marcosDeOcupacao.Count; i++)
        {
            if (marcoDisparado[i])
                continue;

            MarcoDeOcupacao m = marcosDeOcupacao[i];
            if (m == null || m.post == null)
                continue;

            if (m.subindo)
            {
                if (ocupacao < m.limite - 0.01f) marcoArmado[i] = true;
                else if (marcoArmado[i] && ocupacao >= m.limite) Disparar(i, m);
            }
            else
            {
                if (ocupacao > m.limite + 0.01f) marcoArmado[i] = true;
                else if (marcoArmado[i] && ocupacao <= m.limite) Disparar(i, m);
            }
        }
    }

    private void Disparar(int i, MarcoDeOcupacao m)
    {
        if (Postar(m.post.autor, m.post.usuario, m.post.texto, m.post.humor, TipoFoco.Nenhum, m.post.curtidas, PrioridadePost.Alta))
            marcoDisparado[i] = true;
        else
            marcoArmado[i] = false;   // recusado: precisa voltar pro outro lado do limite e cruzar de novo
    }

    /* ---------------- Utilidades ---------------- */

    private static bool TemFalaAoEliminar(PersonagemDoBairro p)
    {
        return (p.aoEliminar != null && p.aoEliminar.Count > 0) || !string.IsNullOrWhiteSpace(p.aoEliminarUltimo);
    }

    /* ATENÇÃO: gasta a fala (avança o índice). Só chame quando ela vai sair. */
    private string FalaAoEliminar(PersonagemDoBairro p, TipoFoco tipo)
    {
        int total = Contar(totalPorTipo, tipo);
        bool ultimo = Contar(eliminadosPorTipo, tipo) >= total;

        if (ultimo && total > 1 && !string.IsNullOrWhiteSpace(p.aoEliminarUltimo))
            return p.aoEliminarUltimo;

        if (p.aoEliminar == null || p.aoEliminar.Count == 0)
            return p.aoEliminarUltimo;

        int i = Contar(proximaFala, tipo);
        proximaFala[tipo] = i + 1;
        if (i < p.aoEliminar.Count)
        {
            ultimaFalaSorteada[tipo] = i;
            return p.aoEliminar[i];
        }

        // Acabaram as falas em ordem: sorteia sem repetir a última.
        if (p.aoEliminar.Count == 1)
            return p.aoEliminar[0];

        int anterior = ultimaFalaSorteada.TryGetValue(tipo, out int u) ? u : -1;
        int sorteio;
        do { sorteio = Random.Range(0, p.aoEliminar.Count); }
        while (sorteio == anterior);
        ultimaFalaSorteada[tipo] = sorteio;
        return p.aoEliminar[sorteio];
    }

    /* Posta o próximo post da lista SE ele sair — só então o índice anda
     * (recusado = fica guardado para o próximo gatilho). emCiclo = volta ao
     * começo quando a lista acaba (senão, acabou, não posta mais). */
    private bool PostarDaLista(List<PostRedeSocial> lista, ref int indice, PrioridadePost prioridade, bool emCiclo = false)
    {
        if (lista == null || lista.Count == 0)
            return false;
        if (indice >= lista.Count)
        {
            if (!emCiclo)
                return false;
            indice = 0;
        }

        PostRedeSocial p = lista[indice];
        if (p == null)
        {
            indice++;
            return false;
        }
        if (!Postar(p.autor, p.usuario, p.texto, p.humor, TipoFoco.Nenhum, p.curtidas, prioridade))
            return false;
        indice++;
        return true;
    }

    /* Publica NA HORA (se a RedeSocial aceitar). Devolve se saiu. */
    private bool Postar(string autor, string usuario, string texto, HumorPost humor, TipoFoco tipo, int curtidas, PrioridadePost prioridade)
    {
        PostRedeSocial post = Formatar(autor, usuario, texto, humor, tipo, curtidas);
        return post != null && rede != null && rede.Postar(post, prioridade);
    }

    private List<PostRedeSocial> Formatados(List<PostRedeSocial> lista)
    {
        List<PostRedeSocial> saida = new List<PostRedeSocial>();
        if (lista != null)
            foreach (PostRedeSocial p in lista)
            {
                PostRedeSocial f = p != null ? Formatar(p.autor, p.usuario, p.texto, p.humor, TipoFoco.Nenhum, p.curtidas) : null;
                if (f != null)
                    saida.Add(f);
            }
        return saida;
    }

    /* Cria uma cópia do post com os marcadores daqui já trocados — o
     * original (do Inspector) nunca é alterado. curtidas < 0 = sorteia,
     * crescendo com o progresso (o bairro engaja mais conforme melhora). */
    private PostRedeSocial Formatar(string autor, string usuario, string texto, HumorPost humor, TipoFoco tipo, int curtidas)
    {
        if (string.IsNullOrWhiteSpace(texto))
            return null;

        int total = Contar(totalPorTipo, tipo);
        int eliminados = Contar(eliminadosPorTipo, tipo);
        string ocupacao = fluxo != null ? Mathf.RoundToInt(fluxo.Ocupacao * 100f) + "%" : "";

        texto = texto
            .Replace("{restantesTipo}", Mathf.Max(0, total - eliminados).ToString())
            .Replace("{eliminadosTipo}", eliminados.ToString())
            .Replace("{totalTipo}", total.ToString())
            .Replace("{ocupacao}", ocupacao);

        if (curtidas < 0)
        {
            float progresso = focos.Count > 0 ? (float)eliminadosTotal / focos.Count : 0f;
            curtidas = Random.Range(18, 60) + Mathf.RoundToInt(progresso * 150f);
        }

        return new PostRedeSocial
        {
            autor = autor,
            usuario = usuario,
            texto = texto,
            humor = humor,
            curtidas = curtidas
        };
    }

    /* Velocidade horizontal medida pelo deslocamento real do player —
     * funciona com qualquer controlador (e ignora teleportes longos). */
    private float VelocidadeDoPlayer(float dt)
    {
        if (player == null || dt <= 0f)
            return 0f;

        Vector3 delta = player.position - ultimaPosicaoPlayer;
        ultimaPosicaoPlayer = player.position;
        delta.y = 0f;
        float v = delta.magnitude / dt;
        return v > 50f ? 0f : v;
    }

    private PersonagemDoBairro Personagem(TipoFoco tipo)
    {
        if (tipo == TipoFoco.Nenhum)
            return null;
        foreach (PersonagemDoBairro p in personagens)
            if (p != null && p.tipo == tipo)
                return p;
        return null;
    }

    /* Mesma classificação por tipo C# do FocoListaHUD.Categoria(). */
    internal static TipoFoco TipoDoFoco(Interactable foco)
    {
        if (foco is PneuInteractable) return TipoFoco.Pneu;
        if (foco is VasoInteractable) return TipoFoco.Vaso;
        if (foco is CaixaDaguaInteractable) return TipoFoco.CaixaDagua;
        if (foco is PiscinaInteractable) return TipoFoco.Piscina;
        if (foco is GarrafaInteractable) return TipoFoco.Garrafas;
        if (foco is RaloInteractable) return TipoFoco.Ralo;
        if (foco is BromeliaInteractable) return TipoFoco.Bromelia;
        return TipoFoco.Nenhum;
    }

    private static int Contar(Dictionary<TipoFoco, int> tabela, TipoFoco tipo)
    {
        return tabela.TryGetValue(tipo, out int n) ? n : 0;
    }

    /* ---------------- Conteúdo padrão ----------------
     * Preenche só as listas vazias (todas, no Reset). Tudo editável pelo
     * Inspector depois. Sem emoji: a fonte padrão do TextMeshPro não tem. */

    private void PreencherPadroes(bool tudo)
    {
        if (tudo || personagens == null || personagens.Count == 0) personagens = PersonagensPadrao();
        else CompletarPersonagens();
        if (tudo || aoSairDoHospital == null || aoSairDoHospital.Count == 0) aoSairDoHospital = SaidaPadrao();
        if (tudo || aoDemorarSemFoco == null || aoDemorarSemFoco.Count == 0) aoDemorarSemFoco = DemoraPadrao();
        if (tudo || aoFicarParado == null || aoFicarParado.Count == 0) aoFicarParado = ParadoPadrao();
        if (tudo || aoFazerCombo == null || aoFazerCombo.Count == 0) aoFazerCombo = ComboPadrao();
        if (tudo || aoPularMuito == null || aoPularMuito.Count == 0) aoPularMuito = PulosPadrao();
        if (tudo || aoEliminarFocoSemDono == null || aoEliminarFocoSemDono.Count == 0) aoEliminarFocoSemDono = SemDonoPadrao();
        if (tudo || marcosDeOcupacao == null || marcosDeOcupacao.Count == 0) marcosDeOcupacao = MarcosPadrao();
        if (tudo || postsDerrota == null || postsDerrota.Count == 0) postsDerrota = DerrotaPadrao();
        if (tudo || aoReabastecerPrimeiraVez == null || aoReabastecerPrimeiraVez.Count == 0) aoReabastecerPrimeiraVez = ReabastecerPadrao();
        if (tudo || aoCarregarPneu == null || aoCarregarPneu.Count == 0) aoCarregarPneu = CarregarPneuPadrao();
        if (tudo || aoEsvaziarSacoPrimeiraVez == null || aoEsvaziarSacoPrimeiraVez.Count == 0) aoEsvaziarSacoPrimeiraVez = EsvaziarSacoPadrao();
        if (tudo || aoEntregarPneuPrimeiraVez == null || aoEntregarPneuPrimeiraVez.Count == 0) aoEntregarPneuPrimeiraVez = EntregarPneuPadrao();
        if (tudo || aoPreverChuva == null || aoPreverChuva.Count == 0) aoPreverChuva = PrevisaoPadrao();
        if (tudo || aoEsvaziarPrimeiraVez == null || aoEsvaziarPrimeiraVez.Count == 0) aoEsvaziarPrimeiraVez = EsvaziarPadrao();
        if (tudo || aoReencherPrimeiraVez == null || aoReencherPrimeiraVez.Count == 0) aoReencherPrimeiraVez = ReencherPadrao();
    }

    /* Acrescenta o personagem padrão de cada tipo de foco que ainda não tem
     * dono na lista (tipos novos numa cena já configurada). Dos personagens
     * existentes, só preenche o aoUsarFerramentaErrada vazio com o texto
     * padrão do mesmo tipo (campo novo da Parte A: os personagens já salvos
     * na cena chegam sem ele); o resto não é mexido. Devolve quantos
     * personagens foram acrescentados. */
    public int CompletarPersonagens()
    {
        if (personagens == null)
            personagens = new List<PersonagemDoBairro>();

        int novos = 0;
        foreach (PersonagemDoBairro padrao in PersonagensPadrao())
        {
            // O primeiro do tipo, o mesmo que o Personagem(tipo) usa para postar.
            PersonagemDoBairro existente = personagens.Find(p => p != null && p.tipo == padrao.tipo);
            if (existente == null)
            {
                personagens.Add(padrao);
                novos++;
                continue;
            }

            if (string.IsNullOrWhiteSpace(existente.aoUsarFerramentaErrada))
                existente.aoUsarFerramentaErrada = padrao.aoUsarFerramentaErrada;
        }
        return novos;
    }

    /* Regrava nesta instância os textos da Parte B: os campos novos e os
     * revistos (a carga agora vai para a picape e para a lixeira). O builder
     * da cena chama: o PreencherPadroes só preenche lista vazia, e os
     * textos antigos salvos na cena ficariam. */
    internal void RegravarTextosDaParteB()
    {
        aoCarregarPneu = CarregarPneuPadrao();
        aoEsvaziarSacoPrimeiraVez = EsvaziarSacoPadrao();
        aoEntregarPneuPrimeiraVez = EntregarPneuPadrao();
        aoPreverChuva = PrevisaoPadrao();
        aoEsvaziarPrimeiraVez = EsvaziarPadrao();
        aoReencherPrimeiraVez = ReencherPadrao();

        foreach (PersonagemDoBairro padrao in PersonagensPadrao())
        {
            if (padrao.tipo != TipoFoco.Pneu && padrao.tipo != TipoFoco.Garrafas)
                continue;

            // O primeiro do tipo, o mesmo que o Personagem(tipo) usa para postar.
            PersonagemDoBairro existente = personagens.Find(p => p != null && p.tipo == padrao.tipo);
            if (existente == null)
                continue;

            existente.aoUsarFerramentaErrada = padrao.aoUsarFerramentaErrada;

            // A 1ª fala do Carlos só sai a partir do 2º pneu (o 1º é o post
            // da picape); as outras ficam como estão.
            if (padrao.tipo == TipoFoco.Pneu && existente.aoEliminar != null && existente.aoEliminar.Count > 0)
                existente.aoEliminar[0] = FalaCarlosDepoisDoPrimeiroPneu;
        }
    }

    /* 1ª fala do Carlos ao eliminar um pneu. O primeiro pneu entregue dá o
     * post da picape (aoEntregarPneuPrimeiraVez), então esta sai no 2º e
     * não pode dizer "primeiro". */
    internal const string FalaCarlosDepoisDoPrimeiroPneu =
        "Lá se vai mais um na picape da UPA. Faltam {restantesTipo}, e eu tô contando daqui da janela.";

    /* 1ª fala da Lúcia ao eliminar uma bromélia: a bromélia é tratada com
     * água sanitária diluída (Parte A). Pública porque o KitDoAgenteBuilder
     * grava a mesma fala no personagem já salvo na cena (o padrão daqui só
     * vale numa lista vazia). */
    public const string FalaLuciaPrimeiraBromelia =
        "Ufa! O agente não arrancou nada: só pôs água sanitária diluída no copinho, e a bromélia ficou inteirinha.";

    private const string EnfRita = "Enf. Rita";
    private const string EnfRitaUsuario = "@enf.rita.upa";
    private const string Upa = "UPA Vila Nova";
    private const string UpaUsuario = "@upa.vilanova";

    private static List<PersonagemDoBairro> PersonagensPadrao()
    {
        return new List<PersonagemDoBairro>
        {
            new PersonagemDoBairro
            {
                autor = "Carlos Mendes", usuario = "@carlinhos.m", tipo = TipoFoco.Pneu,
                aoChegarPerto = "Tô vendo daqui da janela: o agente chegou perto dos pneus. Meses reclamando disso... vamos ver.",
                aoEliminar = new List<string>
                {
                    FalaCarlosDepoisDoPrimeiroPneu,
                    "Mais um pneu sem água. Ninguém tinha feito isso em um ano.",
                    "Tá indo pneu por pneu. Faltam {restantesTipo}, agente!",
                },
                aoEliminarUltimo = "ACABOU! Nenhum pneu com água no bairro. Eu reclamei por meses e o agente resolveu numa tarde.",
                aoLargarPelaMetade = "Ué, o agente mexeu num pneu e foi embora com ele ainda cheio d'água? Volta aqui, moço!",
                aoUsarFerramentaErrada = "Pneu não adianta só esvaziar: tem que levar embora. Pega com as mãos e leva pra picape da UPA!",
                epilogoLimpo = "Juntei a vizinhança e o terreno baldio virou horta comunitária. Pneu agora só furado, de vaso de couve.",
                epilogoPendente = "Os pneus continuam lá, cheios d'água. Todo fim de tarde é aquela nuvem de mosquito.",
            },
            new PersonagemDoBairro
            {
                autor = "Dona Neusa", usuario = "@neusa.flores", tipo = TipoFoco.Vaso,
                aoChegarPerto = "O agente tá passando na frente dos vasos aqui da rua. Será que é com as minhas plantas?",
                aoEliminar = new List<string>
                {
                    "O agente botou areia no pratinho do vaso. Eu nem sabia que aquela aguinha virava criadouro!",
                    "Outro pratinho com areia. Tô aprendendo: planta gosta de água, mosquito também.",
                },
                aoEliminarUltimo = "Todos os pratinhos com areia! Minhas samambaias continuam lindas e sem mosquito.",
                aoLargarPelaMetade = "O agente começou a mexer no meu vaso e parou no meio. O pratinho ainda tá com água!",
                aoUsarFerramentaErrada = "Pratinho de vaso é com areia, viu? Até a borda. O agente tá aprendendo!",
                epilogoLimpo = "Agora eu mesma passo de casa em casa conferindo os pratinhos. Virei fiscal de vaso, com orgulho.",
                epilogoPendente = "Ainda tem pratinho com água parada aqui na rua. Eu vejo as larvas quando rego as plantas.",
            },
            new PersonagemDoBairro
            {
                autor = "João Silva", usuario = "@joaosilva", tipo = TipoFoco.CaixaDagua,
                aoChegarPerto = "O agente tá perto das caixas d'água. Meu filho continua internado. Por favor, não esquece de nenhuma.",
                aoEliminar = new List<string>
                {
                    "O agente tampou uma caixa d'água que estava aberta. Uma caixa assim cria milhares de mosquitos.",
                    "Mais uma caixa d'água tampada. Cada tampa dessas é uma criança a menos no hospital.",
                },
                aoEliminarUltimo = "Todas as caixas d'água tampadas. Liguei pro hospital: meu filho já está sem febre.",
                aoLargarPelaMetade = "Viram? O agente começou a tampar a caixa d'água e largou aberta. Meu filho tá no soro, gente.",
                aoUsarFerramentaErrada = "Caixa d'água se resolve com a tampa, e a tampa tá ali do lado! É só encaixar.",
                epilogoLimpo = "Meu filho teve alta na sexta-feira. Chegou em casa e a primeira coisa que fez foi conferir a tampa da caixa d'água.",
                epilogoPendente = "Meu filho continua internado. E a caixa d'água aberta continua do mesmo jeito.",
            },
            new PersonagemDoBairro
            {
                autor = "Ana Paula", usuario = "@anapaula.rs", tipo = TipoFoco.Piscina,
                aoChegarPerto = "O agente tá indo na direção da piscininha do meu filho. Ih... acho que esqueci de esvaziar.",
                aoEliminar = new List<string>
                {
                    "O agente virou a piscina de plástico de boca pra baixo. Meu filho perguntou por quê. Expliquei, e ele entendeu.",
                },
                aoEliminarUltimo = "Piscina virada, seca e guardada. Meu filho agora diz que quer ser agente de saúde.",
                aoLargarPelaMetade = "O agente começou a esvaziar a piscininha e deixou pela metade. Ainda tem água lá dentro!",
                aoUsarFerramentaErrada = "A piscininha é só esvaziar e virar de boca pra baixo. Com as mãos mesmo!",
                epilogoLimpo = "Meu filho montou a \"patrulha do mosquito\" com os amigos da rua. Eles conferem os quintais todo sábado.",
                epilogoPendente = "A piscininha continua cheia no quintal. Hoje meu filho acordou com febre.",
            },
            new PersonagemDoBairro
            {
                autor = "Bruna Lima", usuario = "@bru.lima", tipo = TipoFoco.Garrafas,
                aoChegarPerto = "Olha o agente chegando perto do lixo acumulado. Garrafa jogada é criadouro garantido.",
                aoEliminar = new List<string>
                {
                    "O agente recolheu as garrafas jogadas. Dica: garrafa se guarda de boca pra baixo, sempre.",
                    "Mais um monte de garrafa e lata recolhido. Tampinha com água também vira criadouro, viu?",
                    "O agente não deixa passar nem lata. Faltam {restantesTipo} montes de lixo.",
                },
                aoEliminarUltimo = "Nenhuma garrafa largada no bairro! Agora é com a gente manter assim.",
                aoLargarPelaMetade = "O agente começou a recolher as garrafas e deixou metade pra trás. Cada uma que sobra é um criadouro.",
                aoUsarFerramentaErrada = "Garrafa vai pro saco de lixo e depois pra lixeira. Nada de deixar pelo chão!",
                epilogoLimpo = "A rua ganhou um ponto de coleta de recicláveis. Garrafa agora vai pro lugar certo, não pro quintal.",
                epilogoPendente = "O lixo acumulado continua juntando água a cada chuva. Já tem gente doente no fim da rua.",
            },
            new PersonagemDoBairro
            {
                autor = "Seu Osvaldo", usuario = "@osvaldo.bar", tipo = TipoFoco.Ralo,
                aoChegarPerto = "O agente tá olhando os ralos da calçada aqui do bar. Eu sempre deixo a grelha de lado pra lavar o chão...",
                aoEliminar = new List<string>
                {
                    "O agente botou a grelha de volta no ralo e ainda pôs tela. Nunca pensei que ralo virava criadouro.",
                    "Mais um ralo fechado com tela. Faltam {restantesTipo}.",
                },
                aoEliminarUltimo = "Todos os ralos da rua fechados e com tela. O balcão do bar nunca teve tão pouco mosquito.",
                aoLargarPelaMetade = "O agente mexeu no ralo e largou a grelha fora do lugar. A água continua lá embaixo!",
                aoUsarFerramentaErrada = "Vi o agente querendo resolver o ralo com outra coisa. Ralo é com tela, moço!",
                epilogoLimpo = "Coloquei tela em todos os ralos do bar e agora ensino os clientes. Ralo aberto aqui não passa mais.",
                epilogoPendente = "Os ralos continuam abertos, cheios d'água. À noite o bar vira festa de mosquito.",
            },
            new PersonagemDoBairro
            {
                autor = "Lúcia Andrade", usuario = "@lu.plantas", tipo = TipoFoco.Bromelia,
                aoChegarPerto = "O agente parou na frente das minhas bromélias. Não vai arrancar não, né?",
                aoEliminar = new List<string>
                {
                    FalaLuciaPrimeiraBromelia,
                    "Mais uma bromélia sem água parada. Dá pra ter planta bonita sem criar mosquito.",
                },
                aoEliminarUltimo = "Todas as bromélias da praça secas e vivas. Agora é jato de mangueira toda semana.",
                aoLargarPelaMetade = "O agente começou a esvaziar a bromélia e parou. Ainda tem água no meio das folhas!",
                aoUsarFerramentaErrada = "Calma, agente! Bromélia não precisa arrancar nem tampar: água sanitária diluída no copinho e pronto.",
                epilogoLimpo = "As bromélias continuam na praça. Toda semana a criançada me ajuda a esvaziar as folhas com a mangueira.",
                epilogoPendente = "Os vizinhos querem arrancar minhas bromélias por causa do mosquito. Bastava esvaziar a água.",
            },
        };
    }

    private static List<PostRedeSocial> SaidaPadrao()
    {
        return new List<PostRedeSocial>
        {
            Novo(EnfRita, EnfRitaUsuario,
                "Nosso agente de saúde acabou de sair da UPA. Vizinhos: tudo o que ele fizer, contem aqui. É assim que a gente acompanha.",
                64, HumorPost.Neutro),
            Novo(Upa, UpaUsuario,
                "Ocupação da UPA em {ocupacao}. Cada criadouro eliminado no bairro é um paciente a menos amanhã.",
                97, HumorPost.Neutro),
        };
    }

    private static List<PostRedeSocial> DemoraPadrao()
    {
        return new List<PostRedeSocial>
        {
            Novo("João Silva", "@joaosilva",
                "Faz tempo que ninguém vê o agente limpar foco nenhum. Meu filho piorou de noite. Cadê ele?",
                73, HumorPost.Negativo),
            Novo(EnfRita, EnfRitaUsuario,
                "Chegaram mais três pacientes com febre na última hora. Agente, precisamos de você lá fora.",
                88, HumorPost.Negativo),
            Novo("Ana Paula", "@anapaula.rs",
                "O bairro tá quieto demais. Agente, ainda faltam {restantes} focos por aí!",
                41, HumorPost.Negativo),
        };
    }

    private static List<PostRedeSocial> ParadoPadrao()
    {
        return new List<PostRedeSocial>
        {
            Novo("Ana Paula", "@anapaula.rs",
                "O agente tá parado no meio da rua faz um tempão. Tá tudo bem, moço?",
                29, HumorPost.Neutro),
            Novo("Carlos Mendes", "@carlinhos.m",
                "Agente parado de novo. Enquanto isso, o mosquito não para.",
                35, HumorPost.Negativo),
        };
    }

    private static List<PostRedeSocial> ComboPadrao()
    {
        return new List<PostRedeSocial>
        {
            Novo("Bruna Lima", "@bru.lima",
                "Dois focos em menos de um minuto! O agente tá voando hoje.",
                66, HumorPost.Positivo),
            Novo("Carlos Mendes", "@carlinhos.m",
                "Nem deu tempo de postar o último e o agente já limpou outro. Esse é bom!",
                58, HumorPost.Positivo),
            Novo(Upa, UpaUsuario,
                "Ritmo excelente, agente! Já são {eliminados} focos eliminados.",
                121, HumorPost.Positivo),
        };
    }

    private static List<PostRedeSocial> PulosPadrao()
    {
        return new List<PostRedeSocial>
        {
            Novo("Ana Paula", "@anapaula.rs",
                "Alguém sabe por que o agente tá pulando no meio da rua? kkkkk",
                47, HumorPost.Neutro),
            Novo("Bruna Lima", "@bru.lima",
                "Agente pulando de novo... deve estar procurando caixa d'água em cima do muro.",
                52, HumorPost.Neutro),
        };
    }

    private static List<PostRedeSocial> SemDonoPadrao()
    {
        return new List<PostRedeSocial>
        {
            Novo(Upa, UpaUsuario,
                "Mais um foco eliminado no bairro. Já são {eliminados} de {total}.",
                90, HumorPost.Positivo),
            Novo("Bruna Lima", "@bru.lima",
                "Vi o agente acabando com mais um criadouro. Faltam {restantes}!",
                54, HumorPost.Positivo),
        };
    }

    private static List<MarcoDeOcupacao> MarcosPadrao()
    {
        return new List<MarcoDeOcupacao>
        {
            new MarcoDeOcupacao { limite = 0.70f, subindo = true, post = Novo(EnfRita, EnfRitaUsuario,
                "Ocupação em {ocupacao}. Já estamos atendendo gente sentada no corredor.", 76, HumorPost.Negativo) },
            new MarcoDeOcupacao { limite = 0.85f, subindo = true, post = Novo(Upa, UpaUsuario,
                "ALERTA: ocupação em {ocupacao}. Suspendemos as consultas de rotina para atender os casos de dengue.", 143, HumorPost.Negativo) },
            new MarcoDeOcupacao { limite = 0.95f, subindo = true, post = Novo(EnfRita, EnfRitaUsuario,
                "Não tem mais maca. Agente, é agora ou nunca.", 188, HumorPost.Negativo) },
            new MarcoDeOcupacao { limite = 0.55f, subindo = false, post = Novo(Upa, UpaUsuario,
                "A ocupação caiu para {ocupacao}! Primeira vez no mês que sobra leito. O trabalho no bairro está funcionando.", 167, HumorPost.Positivo) },
        };
    }

    private static List<PostRedeSocial> DerrotaPadrao()
    {
        return new List<PostRedeSocial>
        {
            Novo(Upa, UpaUsuario,
                "A UPA Vila Nova chegou a 100% da capacidade e está transferindo pacientes. Ainda restam {restantes} focos no bairro.",
                211, HumorPost.Negativo),
            Novo("João Silva", "@joaosilva",
                "Não deu tempo. Os focos venceram dessa vez.",
                98, HumorPost.Negativo),
        };
    }

    /* Kit do agente (Parte A). Curtidas -1 = sorteia, crescendo com o progresso. */
    private static List<PostRedeSocial> ReabastecerPadrao()
    {
        return new List<PostRedeSocial>
        {
            Novo(Upa, UpaUsuario,
                "Nosso agente passou na picape da UPA pra pegar material. Areia, tela e água sanitária a caminho dos quintais!",
                -1, HumorPost.Positivo),
        };
    }

    private static List<PostRedeSocial> CarregarPneuPadrao()
    {
        return new List<PostRedeSocial>
        {
            Novo("Carlos Mendes", "@carlinhos.m",
                "Olha o agente carregando pneu pela rua até a picape da UPA! Finalmente alguém tirando esse entulho do terreno.",
                -1, HumorPost.Positivo),
        };
    }

    private static List<PostRedeSocial> EsvaziarSacoPadrao()
    {
        return new List<PostRedeSocial>
        {
            Novo("Bruna Lima", "@bru.lima",
                "Primeiro saco de garrafas na lixeira! Lixo no lugar certo não junta água.",
                -1, HumorPost.Positivo),
        };
    }

    /* Chuva, atalho e destinos (Parte B). Curtidas -1 = sorteia. */
    private static List<PostRedeSocial> EntregarPneuPadrao()
    {
        return new List<PostRedeSocial>
        {
            Novo("Carlos Mendes", "@carlinhos.m",
                "Primeiro pneu na caçamba da picape da UPA! Um criadouro a menos no bairro.",
                -1, HumorPost.Positivo),
        };
    }

    private static List<PostRedeSocial> PrevisaoPadrao()
    {
        return new List<PostRedeSocial>
        {
            Novo("Ana Paula", "@anapaula.rs",
                "Previsão de chuva forte daqui a pouco aqui no bairro! Tudo que estiver só esvaziado vai encher de novo.",
                -1, HumorPost.Neutro),
            Novo("João Silva", "@joaosilva",
                "O céu fechou do lado da praça. Vai cair água já já.",
                -1, HumorPost.Neutro),
            Novo("Dona Neusa", "@neusa.flores",
                "Recolhi a roupa do varal, que vem chuva aí. Quem esvaziou vaso, bota areia antes que encha.",
                -1, HumorPost.Neutro),
            Novo("Seu Osvaldo", "@osvaldo.bar",
                "Trovoada chegando. Daqui a pouco chove forte no bairro.",
                -1, HumorPost.Neutro),
        };
    }

    private static List<PostRedeSocial> EsvaziarPadrao()
    {
        return new List<PostRedeSocial>
        {
            Novo("Lúcia Andrade", "@lu.plantas",
                "Esvaziar ajuda, mas os ovos ficam grudados na parede. Se chover, eles voltam.",
                -1, HumorPost.Neutro),
        };
    }

    private static List<PostPorTipo> ReencherPadrao()
    {
        return new List<PostPorTipo>
        {
            new PostPorTipo { tipo = TipoFoco.Vaso, post = Novo("Dona Neusa", "@neusa.flores",
                "Choveu e aquele pratinho já está cheio de larva de novo. Tem que pôr areia.",
                -1, HumorPost.Negativo) },
            new PostPorTipo { tipo = TipoFoco.Pneu, post = Novo("Carlos Mendes", "@carlinhos.m",
                "Choveu e o pneu que só esvaziaram encheu de novo. Pneu tem que ir embora na picape.",
                -1, HumorPost.Negativo) },
            new PostPorTipo { tipo = TipoFoco.Garrafas, post = Novo("Bruna Lima", "@bru.lima",
                "Choveu e as garrafas largadas encheram de água de novo. Garrafa vai pro saco e pra lixeira.",
                -1, HumorPost.Negativo) },
        };
    }

    private static PostRedeSocial Novo(string autor, string usuario, string texto, int curtidas, HumorPost humor)
    {
        return new PostRedeSocial
        {
            autor = autor,
            usuario = usuario,
            texto = texto,
            curtidas = curtidas,
            humor = humor
        };
    }
}
