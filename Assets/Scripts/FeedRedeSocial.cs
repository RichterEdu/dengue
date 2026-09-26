using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/* ============================================================
 *  FeedRedeSocial.cs
 *  ---------------------------------------------------------------------
 *  A TELA do app dentro do celular (Canvas em World Space, preso na
 *  frente do aparelho).
 *
 *  - Escuta a RedeSocial e cria um card novo no TOPO do feed a cada
 *    publicação, com animação de entrada.
 *  - Avatar: círculo com a inicial do autor, cor fixa por pessoa.
 *  - Mostra "agora", "2 min", "1 h" conforme o post envelhece.
 *  - Relógio da barra de status = hora real do computador.
 *  - Rolagem suave (o CelularController repassa a rodinha do mouse).
 *
 *  A hierarquia inteira é gerada pelo menu  Dengue > Criar Celular
 *  (Assets/Editor/CelularBuilder.cs), que já liga as referências.
 * ============================================================ */

public class FeedRedeSocial : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private RedeSocial rede;
    [SerializeField] private RectTransform viewport;
    [SerializeField] private RectTransform conteudo;
    [Tooltip("Card modelo (fica desativado). É clonado a cada post.")]
    [SerializeField] private PostView modelo;
    [SerializeField] private TextMeshProUGUI relogio;
    [SerializeField] private TextMeshProUGUI subtitulo;
    [SerializeField] private CanvasGroup vazio;

    [Header("Cabeçalho")]
    [Tooltip("Marcadores: {eliminados} {total} {restantes}")]
    [SerializeField] private string formatoSubtitulo = "Vila Nova · {eliminados}/{total} focos eliminados";

    [Header("Cores")]
    [SerializeField] private Color corNegativo = new Color(0.97f, 0.32f, 0.29f);
    [SerializeField] private Color corNeutro = new Color(0.82f, 0.60f, 0.13f);
    [SerializeField] private Color corPositivo = new Color(0.25f, 0.73f, 0.31f);
    [Tooltip("Paleta dos avatares. A cor é escolhida pelo nome do autor.")]
    [SerializeField] private Color[] coresAvatar =
    {
        new Color(0.30f, 0.55f, 0.90f),
        new Color(0.85f, 0.45f, 0.35f),
        new Color(0.45f, 0.70f, 0.45f),
        new Color(0.70f, 0.45f, 0.80f),
        new Color(0.90f, 0.65f, 0.25f),
        new Color(0.30f, 0.72f, 0.72f),
    };

    [Header("Rolagem")]
    [SerializeField] private float suavidadeScroll = 12f;
    [Tooltip("Quanto a rodinha do mouse anda por passo (em pixels do canvas).")]
    [SerializeField] private float passoScroll = 140f;

    [Header("Animação de entrada")]
    [SerializeField] private float duracaoEntrada = 0.35f;

    private readonly List<PostView> entradas = new List<PostView>();
    private float alvoScroll;
    private float scrollAtual;
    private float proximaAtualizacaoDeTempo;

    void Awake()
    {
        if (rede == null) rede = GetComponentInParent<RedeSocial>();
        if (modelo != null) modelo.gameObject.SetActive(false);
    }

    void OnEnable()
    {
        if (rede != null)
            rede.OnPostPublicado += Adicionar;
    }

    void OnDisable()
    {
        if (rede != null)
            rede.OnPostPublicado -= Adicionar;
    }

    void Start()
    {
        // Se o feed nasceu depois de alguma publicação, recupera o que já saiu.
        if (rede != null)
            foreach (PostRedeSocial p in rede.Publicados)
                Adicionar(p);

        AtualizarSubtitulo();
    }

    void Update()
    {
        // Relógio da barra de status.
        if (relogio != null)
            relogio.text = System.DateTime.Now.ToString("HH:mm");

        // "agora" -> "3 min" -> "1 h": não precisa ser a cada frame.
        if (Time.time >= proximaAtualizacaoDeTempo)
        {
            proximaAtualizacaoDeTempo = Time.time + 5f;
            foreach (PostView pv in entradas)
                if (pv != null && pv.usuario != null)
                    pv.usuario.text = pv.usuarioBase + "  ·  " + TempoRelativo(Time.time - pv.instante);
        }

        AnimarEntradas();
        AtualizarScroll();

        if (vazio != null)
        {
            float alvo = entradas.Count == 0 ? 1f : 0f;
            vazio.alpha = Mathf.MoveTowards(vazio.alpha, alvo, Time.deltaTime * 3f);
        }
    }

    /* ---------------- API usada pelo CelularController ---------------- */

    /* Rodinha do mouse. valor > 0 = rolar para cima (posts mais novos). */
    public void Rolar(float valor)
    {
        alvoScroll -= valor * passoScroll;
        alvoScroll = Mathf.Clamp(alvoScroll, 0f, MaximoScroll());
    }

    public void IrParaTopo()
    {
        alvoScroll = 0f;
    }

    /* ---------------- Cards ---------------- */

    private void Adicionar(PostRedeSocial post)
    {
        if (modelo == null || conteudo == null || post == null)
            return;

        PostView pv = Instantiate(modelo, conteudo);
        pv.gameObject.SetActive(true);
        pv.transform.SetAsFirstSibling();       // post novo entra no topo
        pv.name = "Post - " + post.autor;

        if (pv.rect == null) pv.rect = (RectTransform)pv.transform;
        if (pv.grupo == null) pv.grupo = pv.GetComponent<CanvasGroup>();

        if (pv.nome != null) pv.nome.text = post.autor;
        if (pv.texto != null) pv.texto.text = Marcadores(post.texto);
        if (pv.curtidas != null) pv.curtidas.text = post.curtidas.ToString();

        pv.usuarioBase = post.usuario;
        pv.instante = Time.time;
        pv.anim = 0f;
        if (pv.usuario != null)
            pv.usuario.text = pv.usuarioBase + "  ·  agora";

        Color corDoAutor = CorDoAutor(post.autor);
        if (pv.avatar != null) pv.avatar.color = corDoAutor;
        if (pv.inicial != null) pv.inicial.text = Inicial(post.autor);
        if (pv.faixa != null) pv.faixa.color = CorDoHumor(post.humor);
        if (pv.coracao != null) pv.coracao.color = new Color(0.94f, 0.36f, 0.45f, 0.9f);

        if (pv.grupo != null) pv.grupo.alpha = 0f;
        pv.rect.localScale = new Vector3(0.94f, 0.94f, 1f);

        entradas.Add(pv);

        LayoutRebuilder.ForceRebuildLayoutImmediate(conteudo);
        IrParaTopo();
        AtualizarSubtitulo();
    }

    private void AnimarEntradas()
    {
        for (int i = 0; i < entradas.Count; i++)
        {
            PostView pv = entradas[i];
            if (pv == null || pv.anim >= 1f)
                continue;

            pv.anim = Mathf.Min(1f, pv.anim + Time.deltaTime / Mathf.Max(0.05f, duracaoEntrada));
            float e = 1f - Mathf.Pow(1f - pv.anim, 3f);          // ease out cubic
            if (pv.grupo != null) pv.grupo.alpha = e;
            float s = Mathf.LerpUnclamped(0.94f, 1f, e);
            pv.rect.localScale = new Vector3(s, s, 1f);
        }
    }

    private void AtualizarScroll()
    {
        if (conteudo == null)
            return;

        alvoScroll = Mathf.Clamp(alvoScroll, 0f, MaximoScroll());
        scrollAtual = Mathf.Lerp(scrollAtual, alvoScroll, 1f - Mathf.Exp(-suavidadeScroll * Time.deltaTime));
        Vector2 p = conteudo.anchoredPosition;
        conteudo.anchoredPosition = new Vector2(p.x, scrollAtual);
    }

    private float MaximoScroll()
    {
        if (conteudo == null || viewport == null)
            return 0f;
        return Mathf.Max(0f, conteudo.rect.height - viewport.rect.height);
    }

    private void AtualizarSubtitulo()
    {
        if (subtitulo == null || rede == null)
            return;

        subtitulo.text = Marcadores(formatoSubtitulo);
    }

    /* Troca {eliminados} {total} {restantes} pelos números do momento.
     * Usado no texto do post (congelado na hora em que ele é publicado)
     * e no subtítulo do cabeçalho (atualizado a cada post). Público: a
     * NotificacaoCelular mostra o mesmo texto do card. */
    public string Marcadores(string texto)
    {
        if (string.IsNullOrEmpty(texto) || rede == null)
            return texto;

        return texto
            .Replace("{eliminados}", rede.Eliminados.ToString())
            .Replace("{total}", rede.TotalFocos.ToString())
            .Replace("{restantes}", Mathf.Max(0, rede.TotalFocos - rede.Eliminados).ToString());
    }

    /* ---------------- Utilidades ---------------- */

    private Color CorDoHumor(HumorPost humor)
    {
        switch (humor)
        {
            case HumorPost.Positivo: return corPositivo;
            case HumorPost.Neutro: return corNeutro;
            default: return corNegativo;
        }
    }

    /* Mesma pessoa = sempre a mesma cor de avatar (no feed e na notificação). */
    public Color CorDoAutor(string autor)
    {
        if (coresAvatar == null || coresAvatar.Length == 0)
            return new Color(0.35f, 0.5f, 0.8f);

        int soma = 0;
        if (!string.IsNullOrEmpty(autor))
            foreach (char c in autor)
                soma += c;
        return coresAvatar[Mathf.Abs(soma) % coresAvatar.Length];
    }

    public static string Inicial(string autor)
    {
        if (string.IsNullOrWhiteSpace(autor))
            return "?";
        return char.ToUpper(autor.Trim()[0]).ToString();
    }

    private static string TempoRelativo(float segundos)
    {
        if (segundos < 45f) return "agora";
        if (segundos < 3600f) return Mathf.Max(1, Mathf.FloorToInt(segundos / 60f)) + " min";
        return Mathf.FloorToInt(segundos / 3600f) + " h";
    }
}
