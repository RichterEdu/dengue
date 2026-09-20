using System.Collections.Generic;
using TMPro;
using UnityEngine;

/* ============================================================
 *  DengueHUD.cs
 *  ---------------------------------------------------------------------
 *  HUD em primeira pessoa do jogo (uGUI + TextMeshPro).
 *
 *  - MIRA: ponto no centro. Cresce e muda de cor ao mirar num foco.
 *  - PAINEL DO FOCO: texto ao mirar e barra de cliques (3/5).
 *  - AVISO: ao eliminar um foco, mostra a mensagem de conclusão e, numa
 *    segunda linha, a DICA educativa do foco (fica mais tempo na tela).
 *  - CONTADOR: canto superior esquerdo, "Focos eliminados 2/8".
 *  - MENSAGEM FINAL quando todos os focos são eliminados.
 *
 *  TEXTOS POR FOCO: adicione o componente FocoHUDInfo no foco.
 *  Tudo que não for preenchido lá usa os "Textos padrão" daqui.
 *
 *  A hierarquia é gerada pelo menu  Dengue > Criar HUD
 *  (Assets/Editor/DengueHUDBuilder.cs), que já liga as referências.
 * ============================================================ */

public class DengueHUD : MonoBehaviour
{
    [Header("Textos padrão")]
    [Tooltip("Usado quando o foco não tem FocoHUDInfo. Marcadores: {nome} {cliques} {total} {faltam}")]
    [TextArea(1, 3)]
    [SerializeField] private string textoAoMirar = "Clique para limpar {nome}";

    [Tooltip("Aviso quando um foco é eliminado. Marcadores: {nome} {eliminados} {total} {restantes}")]
    [TextArea(1, 3)]
    [SerializeField] private string textoAoEliminar = "<b>Foco eliminado!</b>";

    [Tooltip("Título do contador.")]
    [SerializeField] private string tituloContador = "FOCOS ELIMINADOS";

    [Tooltip("Formato do contador. Marcadores: {eliminados} {total} {restantes}")]
    [TextArea(1, 3)]
    [SerializeField] private string formatoContador = "{eliminados}<size=60%><color=#FFFFFF99>/{total}</color></size>";

    [Tooltip("Mensagem quando todos os focos são eliminados.")]
    [TextArea(1, 3)]
    [SerializeField] private string mensagemFinal = "Todos os focos foram eliminados!";

    [Header("Dica educativa")]
    [Tooltip("Mostra a dica do foco no aviso, DEPOIS que ele é eliminado.")]
    [SerializeField] private bool mostrarDicaAoEliminar = true;

    [Tooltip("Mostra a dica no painel enquanto o jogador está só mirando (antes de limpar).")]
    [SerializeField] private bool mostrarDicaAoMirar = false;

    [Tooltip("Duração do aviso quando ele vem com dica (segundos). O aviso sem dica usa a duração normal.")]
    [SerializeField] private float duracaoAvisoComDica = 3f;

    [Tooltip("Como a dica aparece dentro do aviso. {dica} é o texto do foco.")]
    [TextArea(1, 3)]
    [SerializeField] private string formatoDicaNoAviso = "<size=70%><color=#FFFFFFB4><i>{dica}</i></color></size>";

    [Tooltip("Dica usada quando o foco não tem FocoHUDInfo (ou está com a dica vazia). Deixe vazio para não mostrar nada.")]
    [TextArea(2, 4)]
    [SerializeField] private string dicaPadrao = "Água parada de uma semana já vira mosquito. Vistorie o quintal todo fim de semana.";

    [Header("Controle automático")]
    [Tooltip("Desmarque para o script NÃO mexer no tamanho/cor da mira (você controla pelo Image e pelo RectTransform).")]
    [SerializeField] private bool animarMira = true;

    [Tooltip("Desmarque para manter a cor que você escolheu no Image do ProgressFill.")]
    [SerializeField] private bool controlarCorDaBarra = true;

    [Tooltip("Desmarque para usar o texto digitado no CounterLabel e no CompleteText, em vez dos campos acima.")]
    [SerializeField] private bool aplicarTextosFixos = true;

    [Header("Referências do jogo")]
    [Tooltip("Interactor do player. Se vazio, procura na cena.")]
    [SerializeField] private Interactor interactor;

    [Header("Mira")]
    [SerializeField] private RectTransform crosshair;
    [SerializeField] private UnityEngine.UI.Image crosshairImage;
    [SerializeField] private Color crosshairNormal = new Color(1f, 1f, 1f, 0.85f);
    [SerializeField] private Color crosshairTarget = new Color(1f, 0.85f, 0.2f, 1f);
    [SerializeField] private float crosshairNormalSize = 8f;
    [SerializeField] private float crosshairTargetSize = 18f;

    [Header("Painel do foco mirado")]
    [SerializeField] private CanvasGroup targetGroup;
    [SerializeField] private TextMeshProUGUI promptText;
    [Tooltip("Linha com a barra + 3/5 (é escondida quando o foco pede).")]
    [SerializeField] private GameObject progressRow;
    [Tooltip("Preenchimento da barra (o anchorMax.x vai de 0 a 1).")]
    [SerializeField] private RectTransform progressFill;
    [SerializeField] private UnityEngine.UI.Image progressFillImage;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private TextMeshProUGUI hintText;
    [SerializeField] private Color corPadraoDaBarra = new Color(0.25f, 1f, 0.8f, 1f);

    [Header("Aviso de foco eliminado")]
    [SerializeField] private CanvasGroup toastGroup;
    [SerializeField] private TextMeshProUGUI toastText;
    [Tooltip("Quanto tempo o aviso fica na tela (segundos).")]
    [SerializeField] private float duracaoAviso = 1f;

    [Header("Contador de focos")]
    [SerializeField] private TextMeshProUGUI counterLabel;
    [SerializeField] private TextMeshProUGUI counterText;
    [SerializeField] private RectTransform counterPanel;

    [Header("Mensagem final")]
    [SerializeField] private CanvasGroup completeGroup;
    [SerializeField] private TextMeshProUGUI completeText;

    [Header("Animação")]
    [Tooltip("Velocidade das transições (mira, fade, barra).")]
    [SerializeField] private float smoothSpeed = 14f;

    private readonly List<Interactable> focos = new List<Interactable>();
    private int eliminated;
    private float displayedFill;
    private Interactable lastTarget;
    private float counterPunch;   // 0..1, decai com o tempo
    private float toastTimer;
    private float toastDuracao;
    private Vector2 toastBasePos;

    void Start()
    {
        if (interactor == null)
            interactor = FindFirstObjectByType<Interactor>();

        // Registra todos os focos da cena (inclusive desativados).
        foreach (Interactable foco in FindObjectsByType<Interactable>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            focos.Add(foco);
            if (foco.IsComplete)
                eliminated++;
            else
                foco.OnCompleted += HandleCompleted;
        }

        if (targetGroup != null) targetGroup.alpha = 0f;
        if (completeGroup != null) completeGroup.alpha = 0f;
        if (toastGroup != null)
        {
            toastGroup.alpha = 0f;
            toastBasePos = ((RectTransform)toastGroup.transform).anchoredPosition;
        }

        // O aviso agora pode ter duas linhas (título + dica): sem isso o
        // texto sairia numa linha só, atravessando a tela.
        if (toastText != null)
            toastText.textWrappingMode = TextWrappingModes.Normal;

        toastDuracao = duracaoAviso;

        ApplyStaticTexts();
        UpdateCounter();
        AvisarReferenciasFaltando();
    }

    /* Referência vazia aqui é falha silenciosa: o aviso (e a dica junto
     * com ele) simplesmente não aparece, sem nenhum erro no Console.
     * Melhor gritar no Play do que o jogador achar que é bug do jogo. */
    private void AvisarReferenciasFaltando()
    {
        System.Text.StringBuilder faltando = new System.Text.StringBuilder();

        if (toastGroup == null || toastText == null)
            faltando.Append("\n  • ToastPanel/ToastText — o aviso de foco eliminado E a dica educativa não aparecem");
        if (promptText == null)
            faltando.Append("\n  • PromptText — o texto ao mirar não aparece");
        if (counterText == null)
            faltando.Append("\n  • CounterText — o contador de focos não atualiza");
        if (completeText == null)
            faltando.Append("\n  • CompleteText — a mensagem final não aparece");
        if (hintText == null && mostrarDicaAoMirar)
            faltando.Append("\n  • HintText — a dica ao mirar não aparece");

        if (faltando.Length > 0)
            Debug.LogWarning("[HUD] Faltam referências na DengueHUD:" + faltando +
                             "\nRode  Dengue > Consertar HUD (criar o que falta)  para criar o que sumiu e religar tudo.", this);
    }

    /* Atualiza os textos fixos no Editor assim que você muda no Inspector. */
    void OnValidate()
    {
        ApplyStaticTexts();
    }

    void OnDestroy()
    {
        foreach (Interactable foco in focos)
            if (foco != null)
                foco.OnCompleted -= HandleCompleted;
    }

    void Update()
    {
        float k = 1f - Mathf.Exp(-smoothSpeed * Time.deltaTime);
        Interactable target = interactor != null ? interactor.CurrentTarget : null;

        UpdateCrosshair(target != null, k);
        UpdateTargetPanel(target, k);
        UpdateToast();
        UpdateCounterPunch();

        if (completeGroup != null)
        {
            float goal = focos.Count > 0 && eliminated >= focos.Count ? 1f : 0f;
            completeGroup.alpha = Mathf.Lerp(completeGroup.alpha, goal, k * 0.5f);
        }
    }

    /* ---------------- Textos fixos ---------------- */

    private void ApplyStaticTexts()
    {
        if (!aplicarTextosFixos) return;
        if (counterLabel != null) counterLabel.text = tituloContador;
        if (completeText != null) completeText.text = mensagemFinal;
    }

    /* ---------------- Mira ---------------- */

    private void UpdateCrosshair(bool hasTarget, float k)
    {
        if (!animarMira)
            return;

        if (crosshair != null)
        {
            float goal = hasTarget ? crosshairTargetSize : crosshairNormalSize;
            float size = Mathf.Lerp(crosshair.sizeDelta.x, goal, k);
            crosshair.sizeDelta = new Vector2(size, size);
        }
        if (crosshairImage != null)
            crosshairImage.color = Color.Lerp(crosshairImage.color, hasTarget ? crosshairTarget : crosshairNormal, k);
    }

    /* ---------------- Painel do foco ---------------- */

    private void UpdateTargetPanel(Interactable target, float k)
    {
        if (targetGroup != null)
            targetGroup.alpha = Mathf.Lerp(targetGroup.alpha, target != null ? 1f : 0f, k);

        // Sem alvo: mantém o último conteúdo enquanto some (fade-out suave).
        if (target == null)
            return;

        FocoHUDInfo info = target.GetComponent<FocoHUDInfo>();

        if (target != lastTarget)
        {
            lastTarget = target;
            displayedFill = Progress(target);

            bool hideProgress = info != null && info.esconderProgresso;
            if (progressRow != null)
                progressRow.SetActive(!hideProgress);

            if (progressFillImage != null && (controlarCorDaBarra || (info != null && info.usarCorPropria)))
                progressFillImage.color = info != null && info.usarCorPropria ? info.corDaBarra : corPadraoDaBarra;

            if (hintText != null)
            {
                // Por padrão a dica NÃO aparece aqui: ela é mostrada depois,
                // no aviso de foco eliminado, onde o jogador tem tempo de ler.
                string dica = mostrarDicaAoMirar ? DicaDoFoco(target, info) : "";
                hintText.gameObject.SetActive(!string.IsNullOrWhiteSpace(dica));
                hintText.text = FormatFoco(dica, target, info);
            }
        }

        // Texto de mirar é refeito a cada frame porque pode usar {cliques}/{faltam}.
        if (promptText != null)
        {
            string template = info != null && !string.IsNullOrWhiteSpace(info.textoAoMirar) ? info.textoAoMirar : textoAoMirar;
            promptText.text = FormatFoco(template, target, info);
        }

        displayedFill = Mathf.Lerp(displayedFill, Progress(target), k);
        if (progressFill != null)
            progressFill.anchorMax = new Vector2(displayedFill, 1f);
        if (progressText != null)
            progressText.text = $"{target.CurrentClicks}/{target.RequiredClicks}";
    }

    private static float Progress(Interactable foco)
    {
        return foco.RequiredClicks > 0 ? Mathf.Clamp01((float)foco.CurrentClicks / foco.RequiredClicks) : 1f;
    }

    /* Nome do foco: FocoHUDInfo.nome > padrão por tipo > nome do objeto. */
    private static string NomeDoFoco(Interactable foco, FocoHUDInfo info)
    {
        if (info != null && !string.IsNullOrWhiteSpace(info.nome)) return info.nome;
        if (foco is PneuInteractable) return "o pneu";
        if (foco is VasoInteractable) return "o pratinho do vaso";
        return foco.gameObject.name;
    }

    private static string FormatFoco(string template, Interactable foco, FocoHUDInfo info)
    {
        if (string.IsNullOrEmpty(template))
            return "";
        return template
            .Replace("{nome}", NomeDoFoco(foco, info))
            .Replace("{cliques}", foco.CurrentClicks.ToString())
            .Replace("{total}", foco.RequiredClicks.ToString())
            .Replace("{faltam}", Mathf.Max(0, foco.RequiredClicks - foco.CurrentClicks).ToString());
    }

    /* Dica do foco: FocoHUDInfo.dica > padrão por tipo > dicaPadrao.
     * O pneu e o vaso são anteriores ao FocoHUDInfo e não têm o
     * componente, por isso o padrão por tipo — assim todo foco do jogo
     * ensina alguma coisa ao ser eliminado, sem mexer nos prefabs. */
    private string DicaDoFoco(Interactable foco, FocoHUDInfo info)
    {
        if (info != null && !string.IsNullOrWhiteSpace(info.dica))
            return info.dica;
        if (foco is PneuInteractable)
            return "Pneu guarda água da chuva por meses. Fure, cubra ou guarde em lugar coberto.";
        if (foco is VasoInteractable)
            return "Pratinho de vaso: encha de areia até a borda. Sem água livre, não nasce larva.";
        return dicaPadrao;
    }

    private string FormatContagem(string template)
    {
        return template
            .Replace("{eliminados}", eliminated.ToString())
            .Replace("{total}", focos.Count.ToString())
            .Replace("{restantes}", Mathf.Max(0, focos.Count - eliminated).ToString());
    }

    /* ---------------- Aviso (toast) ---------------- */

    private void ShowToast(string message, bool comDica)
    {
        if (toastGroup == null || toastText == null || string.IsNullOrWhiteSpace(message))
            return;
        toastText.text = message;
        toastDuracao = comDica ? Mathf.Max(duracaoAviso, duracaoAvisoComDica) : duracaoAviso;
        toastTimer = toastDuracao;
    }

    private void UpdateToast()
    {
        if (toastGroup == null)
            return;

        toastTimer = Mathf.Max(0f, toastTimer - Time.deltaTime);
        float elapsed = toastDuracao - toastTimer;

        // Entra rápido, fica, e sai subindo — saída rápida (0.2s) para não
        // demorar na tela depois que o jogador já leu.
        float alpha = toastTimer <= 0f ? 0f : Mathf.Min(Mathf.Clamp01(elapsed / 0.15f), Mathf.Clamp01(toastTimer / 0.2f));
        toastGroup.alpha = alpha;

        float rise = toastTimer < 0.2f ? (1f - toastTimer / 0.2f) * 20f : 0f;
        ((RectTransform)toastGroup.transform).anchoredPosition = toastBasePos + new Vector2(0f, rise);
    }

    /* ---------------- Contador ---------------- */

    private void HandleCompleted(Interactable foco)
    {
        foco.OnCompleted -= HandleCompleted;
        eliminated++;
        counterPunch = 1f;
        UpdateCounter();

        FocoHUDInfo info = foco.GetComponent<FocoHUDInfo>();
        string template = info != null && !string.IsNullOrWhiteSpace(info.textoAoEliminar) ? info.textoAoEliminar : textoAoEliminar;
        string mensagem = FormatFoco(FormatContagem(template), foco, info);

        // A dica educativa entra aqui, numa segunda linha: é o momento em
        // que o jogador acabou de agir e está olhando para o resultado.
        string dica = mostrarDicaAoEliminar ? DicaDoFoco(foco, info) : "";
        bool comDica = !string.IsNullOrWhiteSpace(dica);
        if (comDica)
            mensagem += "\n" + formatoDicaNoAviso.Replace("{dica}", FormatFoco(FormatContagem(dica), foco, info));

        ShowToast(mensagem, comDica);
    }

    private void UpdateCounter()
    {
        if (counterText != null)
            counterText.text = FormatContagem(formatoContador);
    }

    /* "Pulinho" do painel quando um foco é eliminado. */
    private void UpdateCounterPunch()
    {
        if (counterPanel == null)
            return;
        counterPunch = Mathf.MoveTowards(counterPunch, 0f, Time.deltaTime * 3f);
        float s = 1f + Mathf.Sin(counterPunch * Mathf.PI) * 0.12f;
        counterPanel.localScale = new Vector3(s, s, 1f);
    }
}
