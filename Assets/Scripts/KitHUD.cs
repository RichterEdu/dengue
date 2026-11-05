using TMPro;
using UnityEngine;
using UnityEngine.UI;

/* ============================================================
 *  KitHUD.cs
 *  ---------------------------------------------------------------------
 *  Barra de ferramentas do kit do agente (Parte A), embaixo no centro
 *  da tela, no estilo da HUD atual (painel escuro, contorno claro).
 *
 *  - 5 espaços (96×96, 10 px entre eles, a 40 px da borda de baixo), na
 *    ordem das teclas 1–5 (Ferramentas.Todas). Cada um tem o número no
 *    canto, o nome da ferramenta e a quantidade (TextoQuantidade):
 *    consumível "1/1", saco de lixo "garrafas/capacidade", mãos nada.
 *  - Selecionada: contorno verde-água e escala 1,1. Consumível zerado:
 *    quantidade em vermelho (hora de ir à picape da UPA).
 *  - Carregando pneu: uma linha acima da barra (TextoLinhaPneu) lembra a
 *    picape da UPA e o botão direito, que larga.
 *  - Dica "Teclas 1–5 trocam a ferramenta", com o visual do DicaCorrer
 *    (na lateral esquerda, logo abaixo do lugar da dica de correr, para
 *    as duas não se taparem): aparece com a partida e some com fade na
 *    primeira troca de ferramenta feita com a barra à mostra, para
 *    sempre. Trocar ainda no hospital (a Rita fala do [1-5]) não conta.
 *  - Só aparece depois que o jogador sai do hospital
 *    (FluxoDeJogo.PartidaIniciada), e some com o jogo pausado ou com o
 *    controle desligado (menu de pausa, cartelas, tela final), como o
 *    DicaCorrer: o Canvas fica no sortingOrder 40, acima desses.
 *
 *  Monta o próprio Canvas (ScreenSpaceOverlay) no Awake, neste mesmo
 *  objeto: não precisa de prefab nem de builder de UI. O KitDoAgente é
 *  achado na cena; sem kit, a barra fica escondida (sem erro).
 *  Redesenha a cada OnMudou do kit (ferramenta, cargas, saco, pneu).
 * ============================================================ */

public class KitHUD : MonoBehaviour
{
    [Tooltip("Duração do fade de entrada/saída da barra e da dica (segundos).")]
    [SerializeField] private float duracaoFade = 0.4f;

    // Medidas da barra (px na resolução de referência 1920×1080).
    private const float TamanhoEspaco = 96f;
    private const float DistanciaEntreEspacos = 10f;
    private const float DistanciaDaBorda = 40f;
    private const float EscalaSelecionado = 1.1f;
    private const float VelocidadeEscala = 14f;   // mesma suavidade da DengueHUD

    // Cores (#RRGGBBAA).
    private static readonly Color CorFundo = new Color32(0x12, 0x17, 0x1C, 0xF5);
    private static readonly Color CorContorno = new Color32(0xFF, 0xFF, 0xFF, 0x1A);
    private static readonly Color CorSelecionado = new Color32(0x40, 0xFF, 0xCC, 0xFF);
    private static readonly Color CorZerado = new Color32(0xE0, 0x48, 0x48, 0xFF);
    private static readonly Color CorQuantidade = new Color(1f, 1f, 1f, 0.9f);
    private static readonly Color CorNumero = new Color(1f, 1f, 1f, 0.55f);
    private static readonly Color CorNome = new Color(1f, 1f, 1f, 0.85f);

    internal const string TextoLinhaPneu = "Carregando pneu. Leve à <b>picape da UPA</b>. [Shift] corre. [Botão direito] larga.";
    private const string TextoDica = "Teclas <b>1–5</b> trocam a ferramenta";

    /* Peças de um espaço da barra, guardadas para redesenhar. */
    private const float AlturaStamina = 5f;
    private const float FracaoAlertaStamina = 0.3f;
    private static readonly Color CorTrilhoStamina = new Color(1f, 1f, 1f, 0.15f);

    private class Espaco
    {
        public RectTransform rect;
        public Outline contorno;
        public TextMeshProUGUI quantidade;
    }

    private Espaco[] espacos;
    private CanvasGroup grupo;        // a barra toda (some fora da partida)
    private GameObject linhaPneu;
    private RectTransform barraStamina;
    private Image preenchimentoStamina;
    private CanvasGroup grupoDica;

    private KitDoAgente kit;
    private FluxoDeJogo fluxo;
    private ControladorJogador jogador;
    private bool dicaVista;           // já trocou de ferramenta uma vez

    void Awake()
    {
        Montar();
    }

    void Start()
    {
        // FindAnyObjectByType: o FindFirstObjectByType está obsoleto nesta
        // versão do Unity (aviso CS0618) e só existe um kit e um fluxo.
        kit = FindAnyObjectByType<KitDoAgente>();
        fluxo = FindAnyObjectByType<FluxoDeJogo>();

        if (kit == null)
            return;   // sem kit: fica escondida

        jogador = kit.GetComponent<ControladorJogador>();
        kit.OnMudou += Redesenhar;
        kit.OnFerramentaTrocada += HandleFerramentaTrocada;
        Redesenhar();
    }

    void OnDestroy()
    {
        if (kit != null)
        {
            kit.OnMudou -= Redesenhar;
            kit.OnFerramentaTrocada -= HandleFerramentaTrocada;
        }
    }

    /* A barra aparece só com a partida em andamento (fora do hospital), sem
     * pausa e com o controle ligado. */
    private bool BarraAMostra => kit != null
        && fluxo != null && fluxo.PartidaIniciada
        && Time.timeScale > 0f
        && (jogador == null || jogador.ControleAtivo);

    void Update()
    {
        bool mostrar = BarraAMostra;

        float passo = Time.unscaledDeltaTime / Mathf.Max(0.01f, duracaoFade);
        grupo.alpha = Mathf.MoveTowards(grupo.alpha, mostrar ? 1f : 0f, passo);
        grupoDica.alpha = Mathf.MoveTowards(grupoDica.alpha, dicaVista ? 0f : 1f, passo);

        if (kit == null)
            return;

        if (kit.PneuCarregado != null)
        {
            StaminaDeCarga stamina = kit.Stamina;
            barraStamina.anchorMax = new Vector2(stamina.Fracao, 1f);
            preenchimentoStamina.color = CorDaStamina(stamina);
        }

        // A selecionada cresce (e a anterior volta) suavemente.
        float k = 1f - Mathf.Exp(-VelocidadeEscala * Time.unscaledDeltaTime);
        for (int i = 0; i < espacos.Length; i++)
        {
            float alvo = Ferramentas.Todas[i] == kit.Selecionada ? EscalaSelecionado : 1f;
            float escala = Mathf.Lerp(espacos[i].rect.localScale.x, alvo, k);
            espacos[i].rect.localScale = new Vector3(escala, escala, 1f);
        }
    }

    /* Texto de quantidade de um espaço: consumível "{q}/{cap}", saco de lixo
     * "{garrafas}/{capacidade do saco}", mãos "" (não gastam nada). */
    public static string TextoQuantidade(EstoqueDoKit e, Ferramenta f)
    {
        if (Ferramentas.Consumivel(f))
            return $"{e.Quantidade(f)}/{e.Capacidade(f)}";
        if (f == Ferramenta.SacoDeLixo)
            return $"{e.GarrafasNoSaco}/{e.CapacidadeDoSaco}";
        return "";
    }

    /* ---------------- Eventos do kit ---------------- */

    private void Redesenhar()
    {
        EstoqueDoKit estoque = kit.Estoque;
        for (int i = 0; i < espacos.Length; i++)
        {
            Ferramenta f = Ferramentas.Todas[i];
            espacos[i].contorno.effectColor = f == kit.Selecionada ? CorSelecionado : CorContorno;
            espacos[i].quantidade.text = TextoQuantidade(estoque, f);

            bool zerado = Ferramentas.Consumivel(f) && estoque.Quantidade(f) <= 0;
            espacos[i].quantidade.color = zerado ? CorZerado : CorQuantidade;
        }

        linhaPneu.SetActive(kit.PneuCarregado != null);
    }

    /* A dica 1–5 cumpriu o papel: some (com fade) e não volta mais. Só vale
     * a troca com a barra à mostra: no hospital (barra escondida), quem
     * obedece à Rita e aperta 1–5 ainda vai ver a dica no bairro. */
    private void HandleFerramentaTrocada(Ferramenta ferramenta)
    {
        if (BarraAMostra)
            dicaVista = true;
    }

    /* ---------------- Montagem do visual ---------------- */

    private void Montar()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 40;
        CanvasScaler escala = gameObject.AddComponent<CanvasScaler>();
        escala.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        escala.referenceResolution = new Vector2(1920, 1080);
        escala.matchWidthOrHeight = 0.5f;   // como os outros Canvas do jogo (fora de 16:9 escala igual ao resto da HUD)
        grupo = gameObject.AddComponent<CanvasGroup>();
        grupo.alpha = 0f;
        grupo.interactable = false;
        grupo.blocksRaycasts = false;

        // Barra: os espaços centralizados, a 40 px da borda de baixo.
        int n = Ferramentas.Todas.Length;
        RectTransform barra = NovoRect("Barra", transform);
        barra.anchorMin = barra.anchorMax = new Vector2(0.5f, 0f);
        barra.pivot = new Vector2(0.5f, 0f);
        barra.anchoredPosition = new Vector2(0f, DistanciaDaBorda);
        barra.sizeDelta = new Vector2(n * TamanhoEspaco + (n - 1) * DistanciaEntreEspacos, TamanhoEspaco);

        espacos = new Espaco[n];
        for (int i = 0; i < n; i++)
            espacos[i] = MontarEspaco(barra, i, n);

        MontarLinhaPneu();
        MontarDica();
    }

    /* Um espaço: fundo com contorno, número no canto, nome e quantidade. */
    private Espaco MontarEspaco(RectTransform barra, int i, int n)
    {
        Ferramenta f = Ferramentas.Todas[i];

        // Pivô no centro: a escala 1,1 da selecionada cresce por igual.
        RectTransform rect = NovoRect("Espaco " + (i + 1), barra);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2((i - (n - 1) * 0.5f) * (TamanhoEspaco + DistanciaEntreEspacos), 0f);
        rect.sizeDelta = new Vector2(TamanhoEspaco, TamanhoEspaco);

        Image fundo = rect.gameObject.AddComponent<Image>();
        fundo.color = CorFundo;
        fundo.raycastTarget = false;
        Outline contorno = rect.gameObject.AddComponent<Outline>();
        contorno.effectColor = CorContorno;
        contorno.effectDistance = new Vector2(2f, -2f);
        contorno.useGraphicAlpha = false;

        TextMeshProUGUI numero = NovoTexto("Numero", rect, 18f, CorNumero, TextAlignmentOptions.TopLeft);
        numero.text = (i + 1).ToString();
        numero.fontStyle = FontStyles.Bold;
        RectTransform rn = numero.rectTransform;
        rn.anchorMin = rn.anchorMax = new Vector2(0f, 1f);
        rn.pivot = new Vector2(0f, 1f);
        rn.anchoredPosition = new Vector2(7f, -4f);
        rn.sizeDelta = new Vector2(30f, 22f);

        // Nome no meio, em até duas linhas ("Água / sanitária").
        TextMeshProUGUI nome = NovoTexto("Nome", rect, 16f, CorNome, TextAlignmentOptions.Center);
        nome.text = Ferramentas.Nome(f);
        nome.textWrappingMode = TextWrappingModes.Normal;
        RectTransform rnome = nome.rectTransform;
        rnome.anchorMin = Vector2.zero;
        rnome.anchorMax = Vector2.one;
        rnome.offsetMin = new Vector2(4f, 28f);
        rnome.offsetMax = new Vector2(-4f, -20f);

        TextMeshProUGUI quantidade = NovoTexto("Quantidade", rect, 20f, CorQuantidade, TextAlignmentOptions.Center);
        quantidade.fontStyle = FontStyles.Bold;
        RectTransform rq = quantidade.rectTransform;
        rq.anchorMin = new Vector2(0f, 0f);
        rq.anchorMax = new Vector2(1f, 0f);
        rq.pivot = new Vector2(0.5f, 0f);
        rq.anchoredPosition = new Vector2(0f, 6f);
        rq.sizeDelta = new Vector2(-8f, 24f);

        return new Espaco { rect = rect, contorno = contorno, quantidade = quantidade };
    }

    /* Linha do pneu: painel do mesmo estilo, logo acima da barra, do
     * tamanho do texto. */
    private void MontarLinhaPneu()
    {
        RectTransform painel = NovoRect("LinhaPneu", transform);
        painel.anchorMin = painel.anchorMax = new Vector2(0.5f, 0f);
        painel.pivot = new Vector2(0.5f, 0f);
        painel.anchoredPosition = new Vector2(0f, DistanciaDaBorda + TamanhoEspaco + 18f);

        Image fundo = painel.gameObject.AddComponent<Image>();
        fundo.color = CorFundo;
        fundo.raycastTarget = false;
        Outline contorno = painel.gameObject.AddComponent<Outline>();
        contorno.effectColor = CorContorno;
        contorno.effectDistance = new Vector2(2f, -2f);
        contorno.useGraphicAlpha = false;

        HorizontalLayoutGroup layout = painel.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(18, 18, 8, 8);
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        ContentSizeFitter ajuste = painel.gameObject.AddComponent<ContentSizeFitter>();
        ajuste.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        ajuste.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        TextMeshProUGUI texto = NovoTexto("Texto", painel, 24f, Color.white, TextAlignmentOptions.Center);
        texto.text = TextoLinhaPneu;

        // A stamina: faixa fina na base do painel, fora do layout do texto.
        RectTransform faixa = NovoRect("Stamina", painel);
        faixa.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
        faixa.anchorMin = Vector2.zero;
        faixa.anchorMax = new Vector2(1f, 0f);
        faixa.pivot = new Vector2(0.5f, 0f);
        faixa.sizeDelta = new Vector2(0f, AlturaStamina);
        faixa.anchoredPosition = Vector2.zero;
        Image trilho = faixa.gameObject.AddComponent<Image>();
        trilho.color = CorTrilhoStamina;
        trilho.raycastTarget = false;

        barraStamina = NovoRect("Fill", faixa);
        barraStamina.anchorMin = Vector2.zero;
        barraStamina.anchorMax = Vector2.one;
        barraStamina.offsetMin = barraStamina.offsetMax = Vector2.zero;
        preenchimentoStamina = barraStamina.gameObject.AddComponent<Image>();
        preenchimentoStamina.raycastTarget = false;

        linhaPneu = painel.gameObject;
        linhaPneu.SetActive(false);
    }

    /* Cor da stamina: esgotada ou abaixo de 30%, a do alerta. */
    internal static Color CorDaStamina(StaminaDeCarga stamina)
    {
        return stamina.Esgotada || stamina.Fracao < FracaoAlertaStamina ? CorZerado : CorSelecionado;
    }

    /* Dica 1–5 com o visual do DicaCorrer: fundo preto translúcido, barrinha
     * branca à esquerda e texto de 28 px. Fica 90 px abaixo do lugar da
     * dica de correr e cresce com o texto. */
    private void MontarDica()
    {
        RectTransform fundo = NovoRect("Dica", transform);
        fundo.anchorMin = fundo.anchorMax = new Vector2(0f, 0.5f);
        fundo.pivot = new Vector2(0f, 0.5f);
        fundo.anchoredPosition = new Vector2(30f, -90f);
        fundo.sizeDelta = new Vector2(380f, 70f);
        grupoDica = fundo.gameObject.AddComponent<CanvasGroup>();
        grupoDica.interactable = false;
        grupoDica.blocksRaycasts = false;

        Image imagem = fundo.gameObject.AddComponent<Image>();
        imagem.color = new Color(0f, 0f, 0f, 0.6f);
        imagem.raycastTarget = false;

        HorizontalLayoutGroup layout = fundo.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(26, 20, 0, 0);
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = true;
        ContentSizeFitter ajuste = fundo.gameObject.AddComponent<ContentSizeFitter>();
        ajuste.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

        RectTransform barrinha = NovoRect("Barra", fundo);
        barrinha.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
        barrinha.anchorMin = Vector2.zero;
        barrinha.anchorMax = new Vector2(0f, 1f);
        barrinha.pivot = new Vector2(0f, 0.5f);
        barrinha.anchoredPosition = Vector2.zero;
        barrinha.sizeDelta = new Vector2(6f, 0f);
        Image branca = barrinha.gameObject.AddComponent<Image>();
        branca.color = Color.white;
        branca.raycastTarget = false;

        TextMeshProUGUI texto = NovoTexto("Texto", fundo, 28f, Color.white, TextAlignmentOptions.Center);
        texto.text = TextoDica;
    }

    private static RectTransform NovoRect(string nome, Transform pai)
    {
        RectTransform rt = (RectTransform)new GameObject(nome, typeof(RectTransform)).transform;
        rt.SetParent(pai, false);
        return rt;
    }

    private static TextMeshProUGUI NovoTexto(string nome, Transform pai, float tamanho, Color cor, TextAlignmentOptions alinhamento)
    {
        TextMeshProUGUI tmp = NovoRect(nome, pai).gameObject.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = tamanho;
        tmp.color = cor;
        tmp.alignment = alinhamento;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.raycastTarget = false;
        return tmp;
    }
}
