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
 *    - o jogador começa a limpar um foco e vai embora deixando pela metade.
 *
 *  Além disso, outros momentos da partida viram post: sair do hospital,
 *  demorar sem eliminar foco, ficar parado, limpar dois focos seguidos,
 *  pular à toa, a ocupação do hospital passar de certos limites e a
 *  derrota. No fim, LinhasEpilogo() monta o "uma semana depois" com a
 *  fala de cada personagem, que muda conforme o que o jogador limpou.
 *
 *  Quem publica de fato é a RedeSocial.cs (fila, não lidos). Ao nascer,
 *  este componente liga RedeSocial.NarradorExterno: a RedeSocial passa a
 *  soltar sozinha só a abertura, e o resto sai daqui.
 *
 *  Marcadores que dá pra usar nos textos daqui (trocados na hora de
 *  enfileirar): {restantesTipo} {eliminadosTipo} {totalTipo} {ocupacao}.
 *  Os marcadores do feed ({eliminados} {total} {restantes}) continuam
 *  valendo e são trocados na hora da publicação, como sempre.
 *
 *  Mesma varredura de Interactable usada por DengueHUD/RedeSocial/
 *  FocoListaHUD/FluxoDeJogo — não existe gerenciador central de focos.
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

public class HistoriaRedeSocial : MonoBehaviour
{
    [Header("Referências (vazias = procura na cena)")]
    [SerializeField] private RedeSocial rede;
    [SerializeField] private FluxoDeJogo fluxo;
    [SerializeField] private CelularController celular;
    [SerializeField] private ControladorJogador jogador;

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

    [Tooltip("Posts de comportamento (parado, pulos, combo...) só entram se a fila tiver no máximo isto — evita spam.")]
    [SerializeField] private int maxFilaParaExtras = 2;

    private readonly List<Interactable> focos = new List<Interactable>();
    private readonly Dictionary<Interactable, TipoFoco> tipoDe = new Dictionary<Interactable, TipoFoco>();
    private readonly Dictionary<TipoFoco, int> totalPorTipo = new Dictionary<TipoFoco, int>();
    private readonly Dictionary<TipoFoco, int> eliminadosPorTipo = new Dictionary<TipoFoco, int>();
    private readonly Dictionary<TipoFoco, int> proximaFala = new Dictionary<TipoFoco, int>();
    private readonly Dictionary<TipoFoco, int> ultimaFalaSorteada = new Dictionary<TipoFoco, int>();
    private readonly HashSet<TipoFoco> jaChegouPerto = new HashSet<TipoFoco>();
    private readonly HashSet<Interactable> largadoAvisado = new HashSet<Interactable>();
    private readonly Queue<float> pulos = new Queue<float>();

    private Transform player;
    private Vector3 ultimaPosicaoPlayer;
    private int eliminadosTotal;
    private int idxDemora, idxParado, idxCombo, idxPulos, idxSemDono;
    private bool[] marcoArmado;
    private bool[] marcoDisparado;
    private float timerSemFoco;
    private float timerParado;
    private float ultimoFocoTempo = float.NegativeInfinity;
    private float proximaVarredura;

    private bool FilaLivre => rede == null || rede.NaFila <= maxFilaParaExtras;
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

        foreach (Interactable foco in FindObjectsByType<Interactable>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            TipoFoco tipo = TipoDoFoco(foco);
            focos.Add(foco);
            tipoDe[foco] = tipo;
            totalPorTipo[tipo] = Contar(totalPorTipo, tipo) + 1;

            if (foco.IsComplete)
            {
                eliminadosTotal++;
                eliminadosPorTipo[tipo] = Contar(eliminadosPorTipo, tipo) + 1;
            }
            else
            {
                foco.OnCompleted += HandleFocoEliminado;
            }
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
    }

    void OnDestroy()
    {
        foreach (Interactable foco in focos)
            if (foco != null)
                foco.OnCompleted -= HandleFocoEliminado;

        if (fluxo != null)
        {
            fluxo.OnPartidaIniciada -= HandlePartidaIniciada;
            fluxo.OnDerrota -= HandleDerrota;
        }
        if (jogador != null)
            jogador.OnPulou -= HandlePulou;
    }

    void Update()
    {
        float dt = Time.deltaTime;
        float velocidade = VelocidadeDoPlayer(dt);

        if (!EmJogo || dt <= 0f)
            return;

        VerificarOcupacao();

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
        if (timerSemFoco >= tempoSemFoco && PostarDaLista(aoDemorarSemFoco, ref idxDemora, extra: true))
            timerSemFoco = 0f;

        // Parado: alguém estranha.
        if (velocidade < 0.15f && !lendoCelular)
            timerParado += dt;
        else
            timerParado = 0f;

        if (timerParado >= tempoParado && PostarDaLista(aoFicarParado, ref idxParado, extra: true))
            timerParado = 0f;
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
        foreach (PostRedeSocial p in aoSairDoHospital)
            Enfileirar(p.autor, p.usuario, p.texto, p.humor, TipoFoco.Nenhum, p.curtidas);
    }

    private void HandleDerrota()
    {
        foreach (PostRedeSocial p in postsDerrota)
            Enfileirar(p.autor, p.usuario, p.texto, p.humor, TipoFoco.Nenhum, p.curtidas);
    }

    private void HandleFocoEliminado(Interactable foco)
    {
        foco.OnCompleted -= HandleFocoEliminado;

        TipoFoco tipo = tipoDe.TryGetValue(foco, out TipoFoco t) ? t : TipoFoco.Nenhum;
        eliminadosTotal++;
        eliminadosPorTipo[tipo] = Contar(eliminadosPorTipo, tipo) + 1;
        jaChegouPerto.Add(tipo);   // não faz sentido "estou vendo o agente chegando" depois de limpo
        timerSemFoco = 0f;

        // 1. O dono do tipo comenta ESTE foco.
        PersonagemDoBairro dono = Personagem(tipo);
        string fala = dono != null ? FalaAoEliminar(dono, tipo) : null;
        if (!string.IsNullOrWhiteSpace(fala))
            Enfileirar(dono.autor, dono.usuario, fala, HumorPost.Positivo, tipo, -1);
        else
            PostarDaLista(aoEliminarFocoSemDono, ref idxSemDono, extra: false, emCiclo: true);

        bool acabouOMapa = eliminadosTotal >= focos.Count;

        // 2. Sequência rápida (não no último — ali quem fala são os finais).
        if (!acabouOMapa && Time.time - ultimoFocoTempo <= janelaCombo)
            PostarDaLista(aoFazerCombo, ref idxCombo, extra: true);
        ultimoFocoTempo = Time.time;

        // 3. Bairro limpo: publicações finais da RedeSocial.
        if (acabouOMapa && rede != null)
            foreach (PostRedeSocial p in rede.PostsFinais)
                rede.Enfileirar(p);
    }

    private void HandlePulou()
    {
        if (!EmJogo)
            return;

        pulos.Enqueue(Time.time);
        while (pulos.Count > 0 && Time.time - pulos.Peek() > janelaPulos)
            pulos.Dequeue();

        if (pulos.Count >= pulosParaPost && PostarDaLista(aoPularMuito, ref idxPulos, extra: true))
            pulos.Clear();
    }

    /* ---------------- Verificações periódicas ---------------- */

    private void VerificarChegadas()
    {
        if (player == null || !FilaLivre)
            return;

        foreach (Interactable foco in focos)
        {
            if (foco == null || !foco.CanInteract || !foco.gameObject.activeInHierarchy)
                continue;

            TipoFoco tipo = tipoDe[foco];
            if (tipo == TipoFoco.Nenhum || jaChegouPerto.Contains(tipo))
                continue;

            if (Vector3.Distance(player.position, foco.transform.position) > raioChegada)
                continue;

            jaChegouPerto.Add(tipo);
            PersonagemDoBairro dono = Personagem(tipo);
            if (dono != null && !string.IsNullOrWhiteSpace(dono.aoChegarPerto))
            {
                Enfileirar(dono.autor, dono.usuario, dono.aoChegarPerto, HumorPost.Neutro, tipo, -1);
                return; // um por varredura
            }
        }
    }

    private void VerificarLargados()
    {
        if (player == null || !FilaLivre)
            return;

        foreach (Interactable foco in focos)
        {
            if (foco == null || foco.CurrentClicks == 0 || foco.IsComplete || largadoAvisado.Contains(foco))
                continue;

            if (Vector3.Distance(player.position, foco.transform.position) < raioLargado)
                continue;

            largadoAvisado.Add(foco);
            TipoFoco tipo = tipoDe[foco];
            PersonagemDoBairro dono = Personagem(tipo);
            if (dono != null && !string.IsNullOrWhiteSpace(dono.aoLargarPelaMetade))
            {
                Enfileirar(dono.autor, dono.usuario, dono.aoLargarPelaMetade, HumorPost.Negativo, tipo, -1);
                return;
            }
        }
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
        marcoDisparado[i] = true;
        Enfileirar(m.post.autor, m.post.usuario, m.post.texto, m.post.humor, TipoFoco.Nenhum, m.post.curtidas);
    }

    /* ---------------- Utilidades ---------------- */

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

    /* Publica o próximo post de uma lista. extra = post de comportamento,
     * que só entra se a fila estiver folgada. emCiclo = volta ao começo
     * quando a lista acaba (senão, acabou, não posta mais). */
    private bool PostarDaLista(List<PostRedeSocial> lista, ref int indice, bool extra, bool emCiclo = false)
    {
        if (lista == null || lista.Count == 0)
            return false;
        if (indice >= lista.Count)
        {
            if (!emCiclo)
                return false;
            indice = 0;
        }
        if (extra && !FilaLivre)
            return false;

        PostRedeSocial p = lista[indice++];
        return p != null && Enfileirar(p.autor, p.usuario, p.texto, p.humor, TipoFoco.Nenhum, p.curtidas);
    }

    /* Cria uma cópia do post com os marcadores daqui já trocados — o
     * original (do Inspector) nunca é alterado. curtidas < 0 = sorteia,
     * crescendo com o progresso (o bairro engaja mais conforme melhora). */
    private bool Enfileirar(string autor, string usuario, string texto, HumorPost humor, TipoFoco tipo, int curtidas)
    {
        if (rede == null || string.IsNullOrWhiteSpace(texto))
            return false;

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

        rede.Enfileirar(new PostRedeSocial
        {
            autor = autor,
            usuario = usuario,
            texto = texto,
            humor = humor,
            curtidas = curtidas
        });
        return true;
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
    private static TipoFoco TipoDoFoco(Interactable foco)
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
    }

    /* Acrescenta o personagem padrão de cada tipo de foco que ainda não tem
     * dono na lista (tipos novos numa cena já configurada). Não mexe nos
     * personagens existentes. Devolve quantos foram acrescentados. */
    public int CompletarPersonagens()
    {
        if (personagens == null)
            personagens = new List<PersonagemDoBairro>();

        int novos = 0;
        foreach (PersonagemDoBairro padrao in PersonagensPadrao())
        {
            if (personagens.Exists(p => p != null && p.tipo == padrao.tipo))
                continue;
            personagens.Add(padrao);
            novos++;
        }
        return novos;
    }

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
                    "O agente esvaziou o primeiro pneu e saiu água parada pra todo lado. Faltam {restantesTipo}.",
                    "Mais um pneu sem água. Ninguém tinha feito isso em um ano.",
                    "Tá indo pneu por pneu. Faltam {restantesTipo}, agente!",
                },
                aoEliminarUltimo = "ACABOU! Nenhum pneu com água no bairro. Eu reclamei por meses e o agente resolveu numa tarde.",
                aoLargarPelaMetade = "Ué, o agente mexeu num pneu e foi embora com ele ainda cheio d'água? Volta aqui, moço!",
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
                epilogoLimpo = "Coloquei tela em todos os ralos do bar e agora ensino os clientes. Ralo aberto aqui não passa mais.",
                epilogoPendente = "Os ralos continuam abertos, cheios d'água. À noite o bar vira festa de mosquito.",
            },
            new PersonagemDoBairro
            {
                autor = "Lúcia Andrade", usuario = "@lu.plantas", tipo = TipoFoco.Bromelia,
                aoChegarPerto = "O agente parou na frente das minhas bromélias. Não vai arrancar não, né?",
                aoEliminar = new List<string>
                {
                    "Ufa! O agente só esvaziou a água das folhas, a bromélia ficou inteirinha.",
                    "Mais uma bromélia sem água parada. Dá pra ter planta bonita sem criar mosquito.",
                },
                aoEliminarUltimo = "Todas as bromélias da praça secas e vivas. Agora é jato de mangueira toda semana.",
                aoLargarPelaMetade = "O agente começou a esvaziar a bromélia e parou. Ainda tem água no meio das folhas!",
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
            new MarcoDeOcupacao { limite = 0.40f, subindo = false, post = Novo(Upa, UpaUsuario,
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
