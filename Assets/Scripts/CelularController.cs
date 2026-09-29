using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/* ============================================================
 *  CelularController.cs
 *  ---------------------------------------------------------------------
 *  O celular do agente, feito 100% na HUD (2D). Sem nada em 3D.
 *
 *  - Tecla T (configurável) abre / fecha o painel.
 *  - O painel sobe do canto inferior direito, onde fica o ícone:
 *    desliza + aparece + dá uma leve "esticada" no fim (overshoot).
 *  - Com o painel fechado, cada AVISO da RedeSocial (no máximo um a cada
 *    tantos segundos — ver RedeSocial.OnAviso) faz o ícone sacudir, pulsar
 *    e tocar o som. O badge vermelho conta todos os não lidos.
 *  - Com o painel aberto: bloqueia a interação com os focos, esconde a
 *    HUD do jogo e a rodinha do mouse rola o feed.
 *
 *  A hierarquia é gerada pelo menu  Dengue > Criar Celular
 *  (Assets/Editor/CelularBuilder.cs), que já liga as referências.
 * ============================================================ */

public class CelularController : MonoBehaviour
{
    [Header("Entrada")]
    [Tooltip("Tecla que abre e fecha o celular.")]
    [SerializeField] private Key teclaAbrir = Key.T;

    [Header("Painel")]
    [SerializeField] private RectTransform painel;
    [SerializeField] private CanvasGroup painelGrupo;
    [Tooltip("Posição do painel quando aberto (anchoredPosition).")]
    [SerializeField] private Vector2 posAberto = new Vector2(-40f, 40f);
    [Tooltip("Posição do painel quando fechado — fora da tela, embaixo.")]
    [SerializeField] private Vector2 posFechado = new Vector2(-40f, -820f);
    [SerializeField] private float tempoAbrir = 0.32f;
    [SerializeField] private float tempoFechar = 0.20f;

    [Header("Referências")]
    [SerializeField] private RedeSocial rede;
    [SerializeField] private FeedRedeSocial feed;
    [Tooltip("Interactor do player. Fica bloqueado enquanto o celular está aberto.")]
    [SerializeField] private Interactor interactor;
    [Tooltip("Canvas da DengueHUD. É desligado enquanto o celular está aberto.")]
    [SerializeField] private Canvas hudDoJogo;

    [Header("Ícone (canto inferior direito)")]
    [SerializeField] private RectTransform icone;
    [SerializeField] private CanvasGroup iconeGrupo;
    [SerializeField] private Image iconeTela;
    [SerializeField] private GameObject badge;
    [SerializeField] private TextMeshProUGUI badgeTexto;
    [SerializeField] private Color iconeTelaNormal = new Color(0.25f, 1f, 0.8f, 0.30f);
    [SerializeField] private Color iconeTelaAviso = new Color(0.25f, 1f, 0.8f, 1f);

    [Header("Som (opcional)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip somNotificacao;
    [SerializeField] private AudioClip somAbrir;
    [SerializeField] private AudioClip somFechar;

    private bool aberto;
    private float t;             // 0 = fechado, 1 = aberto
    private float pulso;         // decai de 1 a 0 (notificação)
    private Vector2 iconeBase;

    public bool Aberto => aberto;
    public Key TeclaAbrir => teclaAbrir;

    void Start()
    {
        if (rede == null) rede = GetComponentInChildren<RedeSocial>();
        if (feed == null) feed = GetComponentInChildren<FeedRedeSocial>();
        if (interactor == null) interactor = FindFirstObjectByType<Interactor>();

        if (icone != null)
            iconeBase = icone.anchoredPosition;

        if (rede != null)
        {
            rede.OnAviso += HandleAviso;
            rede.OnNaoLidosMudou += AtualizarBadge;
        }

        if (painelGrupo != null)
        {
            painelGrupo.alpha = 0f;
            painelGrupo.blocksRaycasts = false;
            painelGrupo.interactable = false;
        }
        if (painel != null)
            painel.anchoredPosition = posFechado;

        AtualizarBadge();
    }

    void OnDestroy()
    {
        if (rede != null)
        {
            rede.OnAviso -= HandleAviso;
            rede.OnNaoLidosMudou -= AtualizarBadge;
        }
    }

    void Update()
    {
        LerEntrada();

        float duracao = aberto ? tempoAbrir : tempoFechar;
        t = Mathf.MoveTowards(t, aberto ? 1f : 0f, Time.deltaTime / Mathf.Max(0.05f, duracao));

        AtualizarPainel();
        AtualizarIcone();
    }

    /* ---------------- Entrada ---------------- */

    private void LerEntrada()
    {
        Keyboard teclado = Keyboard.current;
        if (teclado != null && teclado[teclaAbrir].wasPressedThisFrame)
            Alternar();

        if (aberto && feed != null && Mouse.current != null)
        {
            float roda = Mouse.current.scroll.ReadValue().y;
            if (Mathf.Abs(roda) > 0.01f)
                feed.Rolar(Mathf.Sign(roda));
        }
    }

    public void Alternar()
    {
        if (aberto) Fechar();
        else Abrir();
    }

    public void Abrir()
    {
        aberto = true;
        pulso = 0f;

        if (rede != null) rede.MarcarComoLido();
        if (feed != null) feed.IrParaTopo();
        if (interactor != null) interactor.Bloqueado = true;
        if (hudDoJogo != null) hudDoJogo.enabled = false;
        Tocar(somAbrir);
    }

    public void Fechar()
    {
        aberto = false;
        if (interactor != null) interactor.Bloqueado = false;
        if (hudDoJogo != null) hudDoJogo.enabled = true;
        Tocar(somFechar);
    }

    /* ---------------- Animação ---------------- */

    private void AtualizarPainel()
    {
        if (painel == null)
            return;

        float e = aberto ? EaseOutBack(t) : EaseInCubic(t);
        painel.anchoredPosition = Vector2.LerpUnclamped(posFechado, posAberto, e);

        float s = Mathf.LerpUnclamped(0.96f, 1f, e);
        painel.localScale = new Vector3(s, s, 1f);

        if (painelGrupo != null)
            painelGrupo.alpha = Mathf.Clamp01(t * 1.8f);
    }

    private static float EaseOutBack(float x)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        float u = x - 1f;
        return 1f + c3 * u * u * u + c1 * u * u;
    }

    private static float EaseInCubic(float x)
    {
        return x * x * x;
    }

    /* ---------------- Ícone ---------------- */

    private void HandleAviso(PostRedeSocial post)
    {
        if (aberto)
            return;

        pulso = 1f;
        Tocar(somNotificacao);
    }

    private void AtualizarBadge()
    {
        int n = rede != null ? rede.NaoLidos : 0;
        if (badge != null) badge.SetActive(n > 0);
        if (badgeTexto != null) badgeTexto.text = n > 9 ? "9+" : n.ToString();
    }

    private void AtualizarIcone()
    {
        if (iconeGrupo != null)
        {
            float alvo = aberto ? 0f : 1f;
            iconeGrupo.alpha = Mathf.MoveTowards(iconeGrupo.alpha, alvo, Time.deltaTime * 6f);
        }

        if (pulso > 0f)
            pulso = Mathf.Max(0f, pulso - Time.deltaTime * 1.3f);

        if (icone != null)
        {
            // Pulsa e sacode de leve quando chega publicação.
            float s = 1f + Mathf.Sin(pulso * Mathf.PI * 3f) * 0.16f * pulso;
            icone.localScale = new Vector3(s, s, 1f);
            float tremor = Mathf.Sin(Time.time * 42f) * 5f * pulso * pulso;
            icone.anchoredPosition = iconeBase + new Vector2(tremor, 0f);
        }

        if (iconeTela != null)
            iconeTela.color = Color.Lerp(iconeTelaNormal, iconeTelaAviso, pulso);
    }

    private void Tocar(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }
}
