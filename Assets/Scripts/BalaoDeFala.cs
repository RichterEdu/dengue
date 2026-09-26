using TMPro;
using UnityEngine;
using UnityEngine.UI;

/* ============================================================
 *  BalaoDeFala.cs
 *  ---------------------------------------------------------------------
 *  Balão de fala 3D flutuando acima de um personagem (ou de uma "voz"
 *  sem corpo, como os pacientes da recepção) — dá contexto/narrativa
 *  sem áudio.
 *
 *  2ª versão (2026-09-26), escolhida pelo usuário num preview renderizado
 *  no Unity ("variante B"): painel escuro sóbrio igual à HUD, nome de
 *  quem fala em verde-água, texto branco grande e um "rabinho" apontando
 *  pra pessoa. A 1ª versão era um TextMeshPro 3D com fundo <mark>: texto
 *  pequeno, fundo cinza lavado contra a parede branca e — bug — girado
 *  com a face de trás pra câmera (texto espelhado).
 *
 *  - O visual é MONTADO EM RUNTIME (Start) como um Canvas world-space
 *    filho, a partir dos campos abaixo — não precisa de prefab nem de
 *    builder de UI. Se o objeto ainda tiver o TextMeshPro 3D da versão
 *    antiga, ele é desligado no Awake (cena antiga continua funcionando).
 *  - Sempre de frente total pra câmera (rotation = cam.rotation). Um
 *    Canvas/TextMeshPro é legível do lado -Z, então o forward dele tem
 *    que apontar pra LONGE de quem olha — é exatamente o forward da câmera.
 *  - De longe cresce um pouco (até escalaMaximaDeLonge) pra continuar
 *    legível; de perto fica no tamanho base (400 px = 1 m).
 *  - deslocamentoFrente empurra o balão pra frente do personagem: a
 *    enfermeira fica encostada na parede e o balão (~1,3 m de largura)
 *    entrava nela quando visto de lado.
 * ============================================================ */

public class BalaoDeFala : MonoBehaviour
{
    [Header("Fala")]
    [Tooltip("O que o personagem está \"dizendo\".")]
    [TextArea(1, 3)]
    public string texto = "";

    [Tooltip("Quem fala (aparece em cima, em maiúsculas). Vazio = sem nome.")]
    public string quemFala = "";

    [Header("Posição (no espaço do personagem)")]
    [Tooltip("Altura do balão acima do pivô do personagem (m).")]
    public float alturaAcimaDaCabeca = 1.9f;

    [Tooltip("Empurra o balão pra frente do personagem (m) — tira o balão de dentro da parede quando ele está encostado nela.")]
    public float deslocamentoFrente = 0f;

    [Header("Visual (valores do preview aprovado)")]
    [Tooltip("Sprite arredondado fatiado do fundo (UI/Skin/UISprite). O builder preenche; vazio = retângulo reto.")]
    public Sprite spriteFundo;
    public Color corFundoPainel = new Color(0.07f, 0.09f, 0.11f, 1f);
    public Color corTextoFala = new Color(0.95f, 0.96f, 0.97f, 1f);
    public Color corNome = new Color(0.25f, 1f, 0.8f, 1f);
    public float tamanhoTextoPx = 36f;
    public float tamanhoNomePx = 24f;
    [Tooltip("Largura do texto antes de quebrar linha (px do balão).")]
    public float larguraTextoPx = 400f;

    [Tooltip("Metros por pixel do balão (0.0025 = 400 px por metro).")]
    public float metrosPorPixel = 0.0025f;

    [Tooltip("Até esta distância (m) o balão fica no tamanho base; mais longe, cresce.")]
    public float distanciaSemAumento = 3.5f;

    [Tooltip("Quanto o balão pode crescer de longe (1.5 = 50% maior).")]
    public float escalaMaximaDeLonge = 1.5f;

    private Transform cam;
    private RectTransform visual;
    private TextMeshProUGUI nome;
    private TextMeshProUGUI corpo;

    void Awake()
    {
        // Cena antiga: o balão era um TextMeshPro 3D neste mesmo objeto.
        TextMeshPro antigo = GetComponent<TextMeshPro>();
        if (antigo != null)
            antigo.enabled = false;
    }

    void Start()
    {
        transform.localPosition = new Vector3(0f, alturaAcimaDaCabeca, deslocamentoFrente);
        if (Camera.main != null)
            cam = Camera.main.transform;

        Montar();
        AtualizarTexto();
    }

    /* Troca a fala em runtime (ex.: um evento da história). */
    public void DefinirFala(string quem, string fala)
    {
        quemFala = quem;
        texto = fala;
        AtualizarTexto();
    }

    void LateUpdate()
    {
        if (cam == null)
        {
            if (Camera.main != null)
                cam = Camera.main.transform;
            return;
        }

        transform.rotation = cam.rotation;

        if (visual != null)
        {
            float distancia = Vector3.Distance(cam.position, transform.position);
            float fator = Mathf.Clamp(distancia / Mathf.Max(0.1f, distanciaSemAumento), 1f, escalaMaximaDeLonge);
            visual.localScale = Vector3.one * (metrosPorPixel * fator);
        }
    }

    /* ---------------- Montagem do visual ---------------- */

    private void AtualizarTexto()
    {
        if (visual == null)
            return;

        bool temFala = !string.IsNullOrWhiteSpace(texto);
        visual.gameObject.SetActive(temFala);
        if (corpo != null)
            corpo.text = texto;
        if (nome != null)
        {
            nome.text = string.IsNullOrWhiteSpace(quemFala) ? "" : quemFala.ToUpperInvariant();
            nome.gameObject.SetActive(!string.IsNullOrWhiteSpace(quemFala));
        }
    }

    /* Canvas world-space com pivô embaixo no centro: o rabinho do balão
     * fica exatamente na posição do balão (acima da pessoa). */
    private void Montar()
    {
        GameObject raiz = new GameObject("Visual", typeof(RectTransform), typeof(Canvas));
        raiz.layer = gameObject.layer;
        visual = (RectTransform)raiz.transform;
        visual.SetParent(transform, false);
        visual.sizeDelta = new Vector2(10f, 10f);
        visual.pivot = new Vector2(0.5f, 0f);
        visual.localScale = Vector3.one * metrosPorPixel;

        Canvas canvas = raiz.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        if (Camera.main != null)
            canvas.worldCamera = Camera.main;

        // Painel: cresce com o texto (VerticalLayoutGroup + ContentSizeFitter).
        RectTransform painel = NovoRect("Painel", visual);
        painel.anchorMin = painel.anchorMax = new Vector2(0.5f, 0f);
        painel.pivot = new Vector2(0.5f, 0f);
        painel.anchoredPosition = new Vector2(0f, 18f);

        Image fundo = painel.gameObject.AddComponent<Image>();
        fundo.sprite = spriteFundo;
        if (spriteFundo != null)
        {
            fundo.type = Image.Type.Sliced;
            fundo.pixelsPerUnitMultiplier = 0.3f;   // cantos bem arredondados
        }
        fundo.color = corFundoPainel;
        fundo.raycastTarget = false;

        // Sombra e contorno no PRÓPRIO Image (mesmo acabamento da HUD).
        Shadow sombra = painel.gameObject.AddComponent<Shadow>();
        sombra.effectColor = new Color(0f, 0f, 0f, 0.35f);
        sombra.effectDistance = new Vector2(3f, -5f);
        Outline contorno = painel.gameObject.AddComponent<Outline>();
        contorno.effectColor = new Color(1f, 1f, 1f, 0.14f);
        contorno.effectDistance = new Vector2(2f, -2f);
        contorno.useGraphicAlpha = false;

        VerticalLayoutGroup layout = painel.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(26, 26, 16, 20);
        layout.spacing = 2f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        ContentSizeFitter ajuste = painel.gameObject.AddComponent<ContentSizeFitter>();
        ajuste.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        ajuste.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Rabinho: quadrado girado 45°, metade escondida atrás do painel.
        RectTransform rabo = NovoRect("Rabo", painel);
        rabo.SetAsFirstSibling();
        rabo.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
        rabo.anchorMin = rabo.anchorMax = new Vector2(0.5f, 0f);
        rabo.pivot = new Vector2(0.5f, 0.5f);
        rabo.sizeDelta = new Vector2(30f, 30f);
        rabo.anchoredPosition = new Vector2(0f, 2f);
        rabo.localRotation = Quaternion.Euler(0f, 0f, 45f);
        Image raboImg = rabo.gameObject.AddComponent<Image>();
        raboImg.color = corFundoPainel;
        raboImg.raycastTarget = false;

        nome = NovoTexto("Nome", painel, tamanhoNomePx, corNome);
        nome.fontStyle = FontStyles.Bold;
        nome.characterSpacing = 4f;

        corpo = NovoTexto("Texto", painel, tamanhoTextoPx, corTextoFala);
        corpo.gameObject.AddComponent<LayoutElement>().preferredWidth = larguraTextoPx;
    }

    private RectTransform NovoRect(string nomeObjeto, Transform pai)
    {
        GameObject go = new GameObject(nomeObjeto, typeof(RectTransform));
        go.layer = gameObject.layer;
        RectTransform rt = (RectTransform)go.transform;
        rt.SetParent(pai, false);
        return rt;
    }

    private TextMeshProUGUI NovoTexto(string nomeObjeto, Transform pai, float tamanho, Color cor)
    {
        TextMeshProUGUI tmp = NovoRect(nomeObjeto, pai).gameObject.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = tamanho;
        tmp.color = cor;
        tmp.alignment = TextAlignmentOptions.Left;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        tmp.raycastTarget = false;
        return tmp;
    }
}
