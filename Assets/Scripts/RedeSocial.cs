using System.Collections.Generic;
using UnityEngine;

/* ============================================================
 *  RedeSocial.cs
 *  ---------------------------------------------------------------------
 *  O "cérebro" do app do celular: decide QUANDO cada publicação
 *  da comunidade aparece no feed.
 *
 *  São três listas, cada uma com seu momento:
 *
 *   1. ABERTURA  — saem logo que a fase começa (o bairro reclamando).
 *   2. INCENTIVO — saem A CADA 1, 2 OU 3 FOCOS LIMPOS (sorteado dentro
 *      do intervalo configurado), elogiando o jogador e dando dicas.
 *      A primeira sai sempre no PRIMEIRO foco, para o jogador entender
 *      na hora que limpar foco mexe com o bairro.
 *   3. FINAL     — saem quando todos os focos do mapa acabam.
 *
 *  Se as publicações de incentivo acabarem antes dos focos, ele sorteia
 *  entre as que já existem (sem repetir a última), então nunca fica mudo,
 *  mesmo com os 20 focos do GameDoc.
 *
 *  Os posts saem em FILA, com um intervalo entre eles, para não
 *  despejarem todos de uma vez na tela.
 *
 *  Marcadores que dá pra usar dentro do texto (o feed troca na hora
 *  em que o post é publicado):
 *      {eliminados}  {total}  {restantes}
 *
 *  Quem desenha os posts é o FeedRedeSocial; quem abre e fecha o
 *  celular é o CelularController.
 * ============================================================ */

public enum HumorPost
{
    Negativo,   // bairro reclamando
    Neutro,     // notou o trabalho / dica educativa
    Positivo    // resultado aparecendo
}

[System.Serializable]
public class PostRedeSocial
{
    [Tooltip("Nome que aparece em negrito no post.")]
    public string autor = "Vizinho";

    [Tooltip("Arroba do perfil, com @.")]
    public string usuario = "@vizinho";

    [TextArea(2, 4)]
    [Tooltip("Texto da publicação. Marcadores: {eliminados} {total} {restantes}")]
    public string texto = "";

    [Tooltip("Número de curtidas mostrado no rodapé do post.")]
    public int curtidas = 12;

    [Tooltip("Muda a cor da faixa lateral do post.")]
    public HumorPost humor = HumorPost.Negativo;
}

public class RedeSocial : MonoBehaviour
{
    [Header("A cada quantos focos sai um incentivo")]
    [Tooltip("Mínimo de focos limpos entre uma publicação de incentivo e a próxima.")]
    [SerializeField] private int minFocosPorPost = 1;

    [Tooltip("Máximo de focos limpos entre uma publicação de incentivo e a próxima.")]
    [SerializeField] private int maxFocosPorPost = 3;

    [Tooltip("A primeira publicação de incentivo sai já no primeiro foco limpo.")]
    [SerializeField] private bool incentivoNoPrimeiroFoco = true;

    [Header("Publicações — abertura (começo da fase)")]
    [SerializeField] private List<PostRedeSocial> postsIniciais = new List<PostRedeSocial>();

    [Header("Publicações — incentivo (a cada N focos, em ordem)")]
    [SerializeField] private List<PostRedeSocial> postsDeIncentivo = new List<PostRedeSocial>();

    [Header("Publicações — final (todos os focos limpos)")]
    [SerializeField] private List<PostRedeSocial> postsFinais = new List<PostRedeSocial>();

    [Header("Ritmo")]
    [Tooltip("Espera antes da primeira publicação (segundos).")]
    [SerializeField] private float atrasoInicial = 1.2f;

    [Tooltip("Intervalo entre publicações que saem em sequência (segundos).")]
    [SerializeField] private float intervaloEntrePosts = 1.4f;

    /* Disparado a cada publicação nova (o feed escuta para criar o card). */
    public event System.Action<PostRedeSocial> OnPostPublicado;

    /* Disparado quando o número de não lidos muda (o badge escuta). */
    public event System.Action OnNaoLidosMudou;

    private readonly List<Interactable> focos = new List<Interactable>();
    private readonly Queue<PostRedeSocial> fila = new Queue<PostRedeSocial>();
    private readonly List<PostRedeSocial> publicados = new List<PostRedeSocial>();

    private int eliminados;
    private int proximoIncentivo;     // índice do próximo incentivo da lista
    private int ultimoSorteado = -1;  // evita repetir o mesmo post duas vezes seguidas
    private int focosAteProximoPost;
    private bool finaisEnfileirados;
    private float timer;
    private int naoLidos;

    public int NaoLidos => naoLidos;
    public int Eliminados => eliminados;
    public int TotalFocos => focos.Count;
    public IReadOnlyList<PostRedeSocial> Publicados => publicados;
    public IReadOnlyList<PostRedeSocial> PostsFinais => postsFinais;

    /* Quantas publicações ainda esperam a vez na fila. */
    public int NaFila => fila.Count;

    /* Quando true, a RedeSocial só publica a ABERTURA sozinha: incentivos
     * e finais passam a ser decididos por um narrador externo
     * (HistoriaRedeSocial.cs), que posta sobre cada ação do jogador. */
    public bool NarradorExterno { get; set; }

    /* Fração de focos eliminados (0 a 1). Serve de entrada para a futura
     * barra de ocupação hospitalar. */
    public float Progresso => focos.Count > 0 ? (float)eliminados / focos.Count : 0f;

    void Reset()
    {
        postsIniciais = AberturaPadrao();
        postsDeIncentivo = IncentivosPadrao();
        postsFinais = FinaisPadrao();
    }

    void Awake()
    {
        if (postsIniciais == null || postsIniciais.Count == 0) postsIniciais = AberturaPadrao();
        if (postsDeIncentivo == null || postsDeIncentivo.Count == 0) postsDeIncentivo = IncentivosPadrao();
        if (postsFinais == null || postsFinais.Count == 0) postsFinais = FinaisPadrao();
    }

    void Start()
    {
        // Mesmo padrão do DengueHUD: pega todos os focos da cena, inclusive desativados.
        foreach (Interactable foco in FindObjectsByType<Interactable>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            focos.Add(foco);
            if (foco.IsComplete)
                eliminados++;
            else
                foco.OnCompleted += HandleFocoEliminado;
        }

        timer = atrasoInicial;
        focosAteProximoPost = incentivoNoPrimeiroFoco ? 1 : SortearIntervalo();

        foreach (PostRedeSocial p in postsIniciais)
            Enfileirar(p);
    }

    void OnDestroy()
    {
        foreach (Interactable foco in focos)
            if (foco != null)
                foco.OnCompleted -= HandleFocoEliminado;
    }

    void Update()
    {
        if (fila.Count == 0)
            return;

        timer -= Time.deltaTime;
        if (timer > 0f)
            return;

        Publicar(fila.Dequeue());
        timer = intervaloEntrePosts;
    }

    /* Marca tudo como lido (chamado quando o jogador abre o celular). */
    public void MarcarComoLido()
    {
        if (naoLidos == 0)
            return;
        naoLidos = 0;
        OnNaoLidosMudou?.Invoke();
    }

    /* ---------------- Interno ---------------- */

    private void HandleFocoEliminado(Interactable foco)
    {
        foco.OnCompleted -= HandleFocoEliminado;
        eliminados++;

        if (NarradorExterno)
            return;

        // Acabou o mapa: entram as publicações finais no lugar do incentivo.
        if (!finaisEnfileirados && focos.Count > 0 && eliminados >= focos.Count)
        {
            finaisEnfileirados = true;
            foreach (PostRedeSocial p in postsFinais)
                Enfileirar(p);
            return;
        }

        focosAteProximoPost--;
        if (focosAteProximoPost > 0)
            return;

        Enfileirar(ProximoIncentivo());
        focosAteProximoPost = SortearIntervalo();
    }

    /* Sorteia de quantos em quantos focos sai o próximo incentivo. */
    private int SortearIntervalo()
    {
        int min = Mathf.Max(1, minFocosPorPost);
        int max = Mathf.Max(min, maxFocosPorPost);
        return Random.Range(min, max + 1);
    }

    /* Pega o próximo incentivo da lista. Quando a lista acaba, sorteia
     * entre os existentes sem repetir o último usado. */
    private PostRedeSocial ProximoIncentivo()
    {
        if (postsDeIncentivo == null || postsDeIncentivo.Count == 0)
            return null;

        if (proximoIncentivo < postsDeIncentivo.Count)
        {
            ultimoSorteado = proximoIncentivo;
            return postsDeIncentivo[proximoIncentivo++];
        }

        if (postsDeIncentivo.Count == 1)
            return postsDeIncentivo[0];

        int i;
        do { i = Random.Range(0, postsDeIncentivo.Count); }
        while (i == ultimoSorteado);

        ultimoSorteado = i;
        return postsDeIncentivo[i];
    }

    /* Coloca uma publicação na fila (sai no ritmo de intervaloEntrePosts). */
    public void Enfileirar(PostRedeSocial post)
    {
        if (post != null && !string.IsNullOrWhiteSpace(post.texto))
            fila.Enqueue(post);
    }

    private void Publicar(PostRedeSocial post)
    {
        publicados.Add(post);
        naoLidos++;
        OnPostPublicado?.Invoke(post);
        OnNaoLidosMudou?.Invoke();
    }

    /* ---------------- Conteúdo padrão ----------------
     * Tudo editável pelo Inspector, sem tocar em código. */

    private static List<PostRedeSocial> AberturaPadrao()
    {
        return new List<PostRedeSocial>
        {
            Novo("Ana Paula", "@anapaula.rs",
                "Quarta casa da rua com dengue este mês. Alguém no bairro vai fazer alguma coisa?",
                42, HumorPost.Negativo),

            Novo("João Silva", "@joaosilva",
                "Meu filho está internado com dengue. Cheguei no hospital e não tinha mais leito. É de chorar.",
                87, HumorPost.Negativo),

            Novo("Carlos Mendes", "@carlinhos.m",
                "O terreno baldio da esquina virou depósito de pneu velho cheio de água. Um absurdo isso.",
                35, HumorPost.Negativo),

            Novo("Dona Neusa", "@neusa.flores",
                "Chove todo santo dia e ninguém limpa nada. Assim não tem hospital que dê conta.",
                28, HumorPost.Negativo),
        };
    }

    private static List<PostRedeSocial> IncentivosPadrao()
    {
        return new List<PostRedeSocial>
        {
            Novo("Ana Paula", "@anapaula.rs",
                "Vi o agente esvaziando os pneus do terreno agora. Alguém finalmente botou a mão na massa!",
                46, HumorPost.Neutro),

            Novo("Carlos Mendes", "@carlinhos.m",
                "Mais um foco a menos na rua. Já são {eliminados}. Continua, moço, tá fazendo diferença.",
                58, HumorPost.Neutro),

            Novo("Bruna Lima", "@bru.lima",
                "Dica de quem já viu vizinho internado: água parada de uma semana já vira mosquito. Cada foco conta.",
                63, HumorPost.Neutro),

            Novo("Dona Neusa", "@neusa.flores",
                "Olhei da janela e o rapaz estava limpando o quintal do 42. Que trabalho bonito.",
                51, HumorPost.Positivo),

            Novo("João Silva", "@joaosilva",
                "{eliminados} focos eliminados e meu filho já está reagindo melhor. Acho que não é coincidência.",
                112, HumorPost.Positivo),

            Novo("UPA Vila Nova", "@upa.vilanova",
                "Cada criadouro eliminado evita centenas de mosquitos. Faltam {restantes} no bairro — seguimos!",
                134, HumorPost.Neutro),

            Novo("Ana Paula", "@anapaula.rs",
                "Gente, se o agente consegue, a gente também consegue. Olhem o quintal de vocês hoje.",
                77, HumorPost.Positivo),

            Novo("Carlos Mendes", "@carlinhos.m",
                "Passei no terreno baldio e estava limpo. Está ficando outra coisa isso aqui.",
                69, HumorPost.Positivo),

            Novo("Bruna Lima", "@bru.lima",
                "Vaso de planta: joga areia no pratinho. Leva dois minutos e acaba com o criadouro de vez.",
                84, HumorPost.Neutro),

            Novo("Dona Neusa", "@neusa.flores",
                "Tampei minha caixa d'água depois de ver o serviço do agente. Se cada um fizer a parte dele, acaba.",
                74, HumorPost.Positivo),

            Novo("UPA Vila Nova", "@upa.vilanova",
                "Registramos queda nos casos suspeitos esta semana. Continuem: está funcionando.",
                156, HumorPost.Positivo),

            Novo("João Silva", "@joaosilva",
                "Faltam {restantes} focos para o bairro ficar limpo. Estamos junto, agente!",
                98, HumorPost.Positivo),

            Novo("Ana Paula", "@anapaula.rs",
                "Meu filho perguntou por que o moço mexe nos pneus. Expliquei. Agora ele quer ajudar também.",
                91, HumorPost.Positivo),

            Novo("Carlos Mendes", "@carlinhos.m",
                "Choveu forte ontem e hoje não tem um pneu cheio na rua. Isso é serviço bem feito.",
                88, HumorPost.Positivo),

            Novo("Bruna Lima", "@bru.lima",
                "Lembrete: calha entupida também vira criadouro. Vale dar uma olhada na sua hoje.",
                72, HumorPost.Neutro),

            Novo("UPA Vila Nova", "@upa.vilanova",
                "Já são {eliminados} de {total} focos eliminados. O bairro está virando o jogo.",
                143, HumorPost.Positivo),
        };
    }

    private static List<PostRedeSocial> FinaisPadrao()
    {
        return new List<PostRedeSocial>
        {
            Novo("Ana Paula", "@anapaula.rs",
                "Bairro limpo! Obrigada, agente. No fim das contas isso aqui foi a comunidade inteira.",
                231, HumorPost.Positivo),

            Novo("UPA Vila Nova", "@upa.vilanova",
                "Ocupação hospitalar estabilizada. Prevenção funciona — e ela começa dentro de casa.",
                189, HumorPost.Positivo),

            Novo("João Silva", "@joaosilva",
                "Meu filho teve alta hoje. Os {total} focos foram embora e ele voltou pra casa. Obrigado mesmo.",
                204, HumorPost.Positivo),
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
