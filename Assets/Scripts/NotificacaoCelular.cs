using TMPro;
using UnityEngine;
using UnityEngine.UI;

/* ============================================================
 *  NotificacaoCelular.cs
 *  ---------------------------------------------------------------------
 *  Notificação "push" da rede social: com o celular FECHADO, cada AVISO
 *  da RedeSocial (OnAviso: um de cada vez, com prioridade — os outros
 *  posts entram só no feed) aparece num banner acima do ícone do celular (avatar, autor, @ e
 *  o começo do texto), entra deslizando, fica uns segundos e sai. É o que
 *  deixa a história correr na tela sem o jogador precisar apertar T o
 *  tempo todo — quem quiser ler tudo abre o celular.
 *
 *  - Sem fila (pedido do usuário: "as mensagens estão com delay"): o aviso
 *    aparece NA HORA. Se já houver um banner (só acontece quando chega algo
 *    de prioridade maior logo depois), o conteúdo é trocado e ele reentra
 *    do meio da animação.
 *  - Celular aberto = banner some (o jogador já está vendo tudo no feed).
 *  - O texto passa pelos mesmos marcadores do feed ({eliminados}...),
 *    então banner e card mostram exatamente a mesma coisa.
 *  - Post com foto (a foto de um foco, ver HistoriaRedeSocial): a
 *    miniatura da foto liga à direita do banner, o cabeçalho diz "enviou
 *    uma foto" e o texto termina antes dela. Post sem foto: miniatura
 *    desligada, cabeçalho com o @ e o texto até a borda, como antes.
 *
 *  Fica dentro do Canvas CelularHUD. Criado pelo menu
 *  Dengue > Configurar Historia  (Assets/Editor/HistoriaBuilder.cs).
 * ============================================================ */

public class NotificacaoCelular : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private RedeSocial rede;
    [SerializeField] private FeedRedeSocial feed;
    [SerializeField] private CelularController celular;
    [SerializeField] private RectTransform painel;
    [SerializeField] private CanvasGroup grupo;
    [SerializeField] private Image avatar;
    [SerializeField] private TextMeshProUGUI inicial;
    [SerializeField] private TextMeshProUGUI cabecalho;
    [SerializeField] private TextMeshProUGUI texto;

    [Header("Foto do foco")]
    [Tooltip("Moldura da miniatura (com Mask), à direita do banner. Ligada só nos posts com foto.")]
    [SerializeField] private GameObject miniatura;
    [SerializeField] private RawImage miniaturaImagem;

    [Header("Tempo (segundos)")]
    [Tooltip("Quanto um banner fica na tela.")]
    [SerializeField] private float duracao = 4.5f;
    [SerializeField] private float tempoEntrada = 0.28f;
    [Tooltip("De quantos pixels à direita o banner entra deslizando.")]
    [SerializeField] private float deslocamento = 80f;

    // Onde terminam o texto (offsetMax.x) e o cabeçalho (sizeDelta.x), sem e
    // com a miniatura de 96 px (margem 14) à direita.
    private const float FimDoTexto = -18f;
    private const float FimDoTextoComFoto = -118f;
    private const float FimDoCabecalho = -90f;
    private const float FimDoCabecalhoComFoto = -190f;

    private struct Aviso
    {
        public string autor;
        public string usuario;
        public string texto;
        public Texture foto;
    }

    private bool mostrando;
    private bool saindo;
    private float tempoNaTela;
    private float anim;         // 0 = escondido, 1 = na tela
    private Vector2 posBase;

    void Awake()
    {
        if (rede == null) rede = GetComponentInParent<RedeSocial>();
        if (celular == null) celular = GetComponentInParent<CelularController>();
        if (feed == null)
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null) feed = canvas.GetComponentInChildren<FeedRedeSocial>(true);
        }
        if (painel == null) painel = (RectTransform)transform;
        if (grupo == null) grupo = GetComponent<CanvasGroup>();

        posBase = painel.anchoredPosition;
        Aplicar();
    }

    void OnEnable()
    {
        if (rede != null)
            rede.OnAviso += HandlePost;
    }

    void OnDisable()
    {
        if (rede != null)
            rede.OnAviso -= HandlePost;
    }

    internal void HandlePost(PostRedeSocial post)
    {
        if (post == null || CelularAberto)
            return;

        // Marcadores trocados AGORA, com os números do momento da publicação
        // (igual ao card do feed, que também é criado neste instante).
        string corpo = feed != null ? feed.Marcadores(post.texto) : post.texto;
        bool trocando = mostrando;
        Mostrar(new Aviso { autor = post.autor, usuario = post.usuario, texto = corpo, foto = post.foto });
        if (trocando)
            anim = Mathf.Min(anim, 0.5f);   // reentra do meio: dá pra ver que mudou
    }

    private bool CelularAberto => celular != null && celular.Aberto;

    void Update()
    {
        float dt = Time.unscaledDeltaTime;

        if (CelularAberto && mostrando)
            saindo = true;

        if (mostrando && !saindo)
        {
            tempoNaTela += dt;
            if (tempoNaTela >= duracao)
                saindo = true;
        }

        float alvo = mostrando && !saindo ? 1f : 0f;
        anim = Mathf.MoveTowards(anim, alvo, dt / Mathf.Max(0.05f, tempoEntrada));

        if (saindo && anim <= 0f)
        {
            mostrando = false;
            saindo = false;
        }

        Aplicar();
    }

    private void Mostrar(Aviso aviso)
    {
        mostrando = true;
        saindo = false;
        tempoNaTela = 0f;

        // Sem a miniatura na cena (celular ainda não preparado para fotos),
        // o post com foto aparece como os outros.
        bool comFoto = aviso.foto != null && miniatura != null && miniaturaImagem != null;
        if (miniaturaImagem != null)
            miniaturaImagem.texture = comFoto ? aviso.foto : null;
        if (miniatura != null)
            miniatura.SetActive(comFoto);

        if (cabecalho != null)
        {
            cabecalho.text = comFoto
                ? $"<b>{aviso.autor}</b>  <color=#FFFFFF80>enviou uma foto</color>"
                : $"<b>{aviso.autor}</b>  <color=#FFFFFF80>{aviso.usuario}</color>";
            RectTransform cabRt = cabecalho.rectTransform;
            cabRt.sizeDelta = new Vector2(comFoto ? FimDoCabecalhoComFoto : FimDoCabecalho, cabRt.sizeDelta.y);
        }
        if (texto != null)
        {
            texto.text = aviso.texto;
            RectTransform textoRt = texto.rectTransform;
            textoRt.offsetMax = new Vector2(comFoto ? FimDoTextoComFoto : FimDoTexto, textoRt.offsetMax.y);
        }
        if (avatar != null && feed != null)
            avatar.color = feed.CorDoAutor(aviso.autor);
        if (inicial != null)
            inicial.text = FeedRedeSocial.Inicial(aviso.autor);
    }

    private void Aplicar()
    {
        if (grupo != null)
            grupo.alpha = anim;
        if (painel != null)
        {
            float e = 1f - Mathf.Pow(1f - anim, 3f);   // ease out cubic
            painel.anchoredPosition = posBase + new Vector2((1f - e) * deslocamento, 0f);
        }
    }
}
