using UnityEngine;
using UnityEngine.UI;

/* ============================================================
 *  DicaCorrer.cs
 *  ---------------------------------------------------------------------
 *  Se o jogador passa um tempo sem correr, mostra na lateral esquerda
 *  da tela a dica "Pressione [Shift] para correr". Ao correr, a dica
 *  some e o contador zera. Enquanto o agente não pode correr
 *  (ControladorJogador.PodePular falso: carregando pneu), a dica
 *  não aparece e o tempo não conta.
 *
 *  Não precisa ser colocado na cena: se instala sozinho no objeto que
 *  tem o ControladorJogador (RuntimeInitializeOnLoadMethod) e cria o
 *  próprio Canvas em tempo de execução.
 * ============================================================ */

public class DicaCorrer : MonoBehaviour
{
    [Tooltip("Segundos sem correr até a dica aparecer.")]
    [SerializeField] private float tempoSemCorrer = 20f;

    [Tooltip("Duração do fade de entrada/saída (segundos).")]
    [SerializeField] private float duracaoFade = 0.4f;

    private EntradaJogador entrada;
    private ControladorJogador controlador;
    private CanvasGroup grupo;
    private float tempoParado;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Instalar()
    {
        var jogador = FindAnyObjectByType<ControladorJogador>();
        if (jogador != null && jogador.GetComponent<DicaCorrer>() == null)
            jogador.gameObject.AddComponent<DicaCorrer>();
    }

    void Awake()
    {
        entrada = GetComponent<EntradaJogador>();
        controlador = GetComponent<ControladorJogador>();
        CriarUI();
    }

    void Update()
    {
        // Carregando pneu o Shift não corre: mandar correr seria mentira.
        bool podeMostrar = Time.timeScale > 0f
            && (controlador == null || (controlador.ControleAtivo && controlador.PodePular))
            && (entrada == null || entrada.Habilitado);

        bool correndo = entrada != null && entrada.CorrerSegurado;
        if (correndo)
            tempoParado = 0f;
        else if (podeMostrar)
            tempoParado += Time.deltaTime;

        float alvo = podeMostrar && !correndo && tempoParado >= tempoSemCorrer ? 1f : 0f;
        float passo = Time.unscaledDeltaTime / Mathf.Max(0.01f, duracaoFade);
        grupo.alpha = Mathf.MoveTowards(grupo.alpha, alvo, passo);
    }

    void OnDestroy()
    {
        if (grupo != null)
            Destroy(grupo.gameObject);
    }

    private void CriarUI()
    {
        var raiz = new GameObject("DicaCorrer_Canvas");
        var canvas = raiz.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        var escala = raiz.AddComponent<CanvasScaler>();
        escala.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        escala.referenceResolution = new Vector2(1920, 1080);
        grupo = raiz.AddComponent<CanvasGroup>();
        grupo.alpha = 0f;
        grupo.interactable = false;
        grupo.blocksRaycasts = false;

        var fundo = new GameObject("Fundo").AddComponent<Image>();
        fundo.transform.SetParent(raiz.transform, false);
        fundo.color = new Color(0f, 0f, 0f, 0.6f);
        fundo.raycastTarget = false;
        var rf = fundo.rectTransform;
        rf.anchorMin = rf.anchorMax = new Vector2(0f, 0.5f);
        rf.pivot = new Vector2(0f, 0.5f);
        rf.anchoredPosition = new Vector2(30f, 0f);
        rf.sizeDelta = new Vector2(380f, 70f);

        var barra = new GameObject("Barra").AddComponent<Image>();
        barra.transform.SetParent(fundo.transform, false);
        barra.color = Color.white;
        barra.raycastTarget = false;
        var rb = barra.rectTransform;
        rb.anchorMin = Vector2.zero;
        rb.anchorMax = new Vector2(0f, 1f);
        rb.pivot = new Vector2(0f, 0.5f);
        rb.anchoredPosition = Vector2.zero;
        rb.sizeDelta = new Vector2(6f, 0f);

        var texto = new GameObject("Texto").AddComponent<Text>();
        texto.transform.SetParent(fundo.transform, false);
        texto.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        texto.fontSize = 28;
        texto.color = Color.white;
        texto.alignment = TextAnchor.MiddleCenter;
        texto.supportRichText = true;
        texto.raycastTarget = false;
        texto.text = "Pressione <b>[Shift]</b> para correr";
        var rt = texto.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(14f, 0f);
        rt.offsetMax = Vector2.zero;
    }
}
