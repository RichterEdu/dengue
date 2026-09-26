using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/* ============================================================
 *  DengueHUDBuilder.cs  (Editor)
 *  ---------------------------------------------------------------------
 *  Gera a HUD (Canvas uGUI) na cena aberta.
 *
 *  Menus:
 *    Dengue > Criar HUD        gera a HUD padrão (pergunta antes de substituir)
 *    Dengue > Reconectar HUD   religa as referências depois de você editar
 *                              a hierarquia na mão (renomear/mover/apagar)
 *
 *  Hierarquia criada:
 *    DengueHUD (Canvas + CanvasScaler 1920x1080 + DengueHUD.cs)
 *    ├── Crosshair                      (centro)
 *    ├── TargetPanel                    (abaixo da mira, Vertical Layout)
 *    │   ├── PromptText
 *    │   ├── ProgressRow
 *    │   │   ├── ProgressBar
 *    │   │   │   └── ProgressFill
 *    │   │   └── ProgressText
 *    │   └── HintText
 *    ├── ToastPanel                     (acima da mira, "Foco eliminado!")
 *    │   └── ToastText
 *    ├── CounterPanel                   (canto superior esquerdo)
 *    │   ├── CounterLabel
 *    │   └── CounterText
 *    └── CompletePanel                  (topo, aparece no final)
 *        └── CompleteText
 *
 *  - Se já existir um objeto "DengueHUD" na cena, ele é substituído.
 *  - Depois de gerar, dá pra mover/recolorir tudo à vontade no Inspector.
 *  - Os TEXTOS ficam no componente DengueHUD (padrão) e no FocoHUDInfo
 *    de cada foco (específico).
 * ============================================================ */

public static class DengueHUDBuilder
{
    private const string RootName = "DengueHUD";

    private static readonly Color PanelColor = new Color(0.05f, 0.08f, 0.10f, 0.72f);
    private static readonly Color AccentColor = new Color(0.25f, 1f, 0.8f, 1f);   // mesmo verde do "pop"
    private static readonly Color BarBgColor = new Color(1f, 1f, 1f, 0.18f);

    [MenuItem("Dengue/Criar HUD")]
    public static void CreateHUD()
    {
        // TextMeshPro precisa dos "Essential Resources" (fonte padrão etc).
        if (TMP_Settings.instance == null)
        {
            TMP_PackageResourceImporter.ImportResources(true, false, false);
            EditorUtility.DisplayDialog("Criar HUD",
                "Importei os recursos essenciais do TextMeshPro.\n\nEspere o Unity terminar de importar e rode  Dengue > Criar HUD  de novo.",
                "OK");
            return;
        }

        GameObject old = GameObject.Find(RootName);
        Transform oldParent = null;
        int oldIndex = 0;
        if (old != null)
        {
            bool replace = EditorUtility.DisplayDialog("Criar HUD",
                "Já existe uma HUD na cena.\n\nRECRIAR apaga a HUD atual e cria uma nova padrão — você perde os ajustes feitos à mão.\n\nSe você só renomeou/moveu/apagou elementos, use  Dengue > Reconectar HUD  em vez disso.",
                "Recriar do zero", "Cancelar");
            if (!replace)
                return;
            oldParent = old.transform.parent;
            oldIndex = old.transform.GetSiblingIndex();
            Undo.DestroyObjectImmediate(old);
        }

        Sprite uiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        Sprite knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

        /* ---------- Canvas ---------- */
        GameObject root = new GameObject(RootName, typeof(RectTransform), typeof(Canvas),
            typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
        Undo.RegisterCreatedObjectUndo(root, "Criar HUD");
        HierarquiaOrganizador.RecolocarNoLugar(root, oldParent, oldIndex);

        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        var scaler = root.GetComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        RectTransform rootRt = (RectTransform)root.transform;

        /* ---------- Mira ---------- */
        RectTransform crosshair = CreateRect("Crosshair", rootRt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(8, 8));
        var crosshairImg = AddImage(crosshair, knob, new Color(1f, 1f, 1f, 0.85f));

        /* ---------- Painel do foco (cresce para baixo conforme o conteúdo) ---------- */
        RectTransform target = CreateRect("TargetPanel", rootRt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 1f),
            new Vector2(0, -40), new Vector2(460, 90));
        CanvasGroup targetGroup = target.gameObject.AddComponent<CanvasGroup>();
        targetGroup.blocksRaycasts = false;
        targetGroup.interactable = false;
        var targetBg = AddImage(target, uiSprite, PanelColor);
        targetBg.type = UnityEngine.UI.Image.Type.Sliced;

        var vlg = target.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
        vlg.padding = new RectOffset(20, 20, 10, 12);
        vlg.spacing = 6;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        var fitter = target.gameObject.AddComponent<UnityEngine.UI.ContentSizeFitter>();
        fitter.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;

        TextMeshProUGUI prompt = CreateText("PromptText", target, "Clique para limpar o pneu", 28,
            TextAlignmentOptions.Center, Color.white);
        prompt.textWrappingMode = TextWrappingModes.Normal;

        // Linha: [ barra ] 3/5
        RectTransform row = CreateRect("ProgressRow", target, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(0, 24));
        var rowLe = row.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();
        rowLe.minHeight = 24;
        rowLe.preferredHeight = 24;

        RectTransform bar = CreateRect("ProgressBar", row, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(-30, 0), new Vector2(240, 12));
        var barImg = AddImage(bar, uiSprite, BarBgColor);
        barImg.type = UnityEngine.UI.Image.Type.Sliced;
        barImg.pixelsPerUnitMultiplier = 3f;

        // A barra "enche" mexendo no anchorMax.x (0..1) do ProgressFill.
        RectTransform fill = CreateRect("ProgressFill", bar, Vector2.zero, new Vector2(0f, 0.5f), Vector2.zero, Vector2.zero);
        fill.anchorMax = new Vector2(0.6f, 1f);
        fill.offsetMin = Vector2.zero;
        fill.offsetMax = Vector2.zero;
        var fillImg = AddImage(fill, uiSprite, AccentColor);
        fillImg.type = UnityEngine.UI.Image.Type.Sliced;
        fillImg.pixelsPerUnitMultiplier = 3f;

        TextMeshProUGUI progress = CreateText("ProgressText", row, "3/5", 20,
            TextAlignmentOptions.Left, new Color(1f, 1f, 1f, 0.8f));
        SetRect(progress.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 0.5f),
            new Vector2(100, 0), new Vector2(80, 24));

        TextMeshProUGUI hint = CreateText("HintText", target,
            "Pneus acumulam água da chuva: é onde o mosquito coloca os ovos.", 19,
            TextAlignmentOptions.Center, new Color(1f, 1f, 1f, 0.65f));
        hint.textWrappingMode = TextWrappingModes.Normal;
        hint.fontStyle = FontStyles.Italic;

        /* ---------- Aviso de foco eliminado (topo da tela) ---------- */
        RectTransform toast = CreateRect("ToastPanel", rootRt, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0, -60), new Vector2(700, 50));
        CanvasGroup toastGroup = toast.gameObject.AddComponent<CanvasGroup>();
        toastGroup.blocksRaycasts = false;
        toastGroup.interactable = false;
        TextMeshProUGUI toastText = CreateText("ToastText", toast, "<b>Foco eliminado!</b>", 32,
            TextAlignmentOptions.Center, AccentColor);
        SetRect(toastText.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        toastText.textWrappingMode = TextWrappingModes.Normal;

        // Fundo do aviso (painel escuro + faixa lateral) e altura que
        // acompanha o texto — mesmo código do Dengue > Consertar HUD,
        // para o aviso nascer igual pelos dois caminhos.
        DengueHUDReparo.GarantirFundo(toast, toastText);

        /* ---------- Contador (canto superior esquerdo) ---------- */
        RectTransform counter = CreateRect("CounterPanel", rootRt, new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(32, -32), new Vector2(260, 96));
        var counterBg = AddImage(counter, uiSprite, PanelColor);
        counterBg.type = UnityEngine.UI.Image.Type.Sliced;

        RectTransform accent = CreateRect("Accent", counter, new Vector2(0, 0), new Vector2(0, 0.5f), Vector2.zero, Vector2.zero);
        accent.anchorMax = new Vector2(0, 1);
        accent.offsetMin = new Vector2(0, 10);
        accent.offsetMax = new Vector2(6, -10);
        AddImage(accent, null, AccentColor);

        TextMeshProUGUI counterLabel = CreateText("CounterLabel", counter, "FOCOS ELIMINADOS", 18,
            TextAlignmentOptions.TopLeft, new Color(1f, 1f, 1f, 0.7f));
        counterLabel.characterSpacing = 4;
        SetRect(counterLabel.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1),
            new Vector2(22, -12), new Vector2(-30, 26));

        TextMeshProUGUI counterText = CreateText("CounterText", counter, "0<size=60%><color=#FFFFFF99>/8</color></size>", 46,
            TextAlignmentOptions.BottomLeft, Color.white);
        counterText.fontStyle = FontStyles.Bold;
        SetRect(counterText.rectTransform, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 0),
            new Vector2(22, 8), new Vector2(-30, 58));

        /* ---------- Mensagem final ---------- */
        RectTransform complete = CreateRect("CompletePanel", rootRt, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0, -140), new Vector2(720, 90));
        var completeBg = AddImage(complete, uiSprite, PanelColor);
        completeBg.type = UnityEngine.UI.Image.Type.Sliced;
        CanvasGroup completeGroup = complete.gameObject.AddComponent<CanvasGroup>();
        completeGroup.blocksRaycasts = false;
        completeGroup.interactable = false;

        TextMeshProUGUI completeText = CreateText("CompleteText", complete, "Todos os focos foram eliminados!", 38,
            TextAlignmentOptions.Center, AccentColor);
        completeText.fontStyle = FontStyles.Bold;
        SetRect(completeText.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

        /* ---------- Script + referências ---------- */
        DengueHUD hud = root.AddComponent<DengueHUD>();
        SerializedObject so = new SerializedObject(hud);
        so.FindProperty("interactor").objectReferenceValue = Object.FindFirstObjectByType<Interactor>();
        so.FindProperty("crosshair").objectReferenceValue = crosshair;
        so.FindProperty("crosshairImage").objectReferenceValue = crosshairImg;
        so.FindProperty("targetGroup").objectReferenceValue = targetGroup;
        so.FindProperty("promptText").objectReferenceValue = prompt;
        so.FindProperty("progressRow").objectReferenceValue = row.gameObject;
        so.FindProperty("progressFill").objectReferenceValue = fill;
        so.FindProperty("progressFillImage").objectReferenceValue = fillImg;
        so.FindProperty("progressText").objectReferenceValue = progress;
        so.FindProperty("hintText").objectReferenceValue = hint;
        so.FindProperty("toastGroup").objectReferenceValue = toastGroup;
        so.FindProperty("toastText").objectReferenceValue = toastText;
        so.FindProperty("counterLabel").objectReferenceValue = counterLabel;
        so.FindProperty("counterText").objectReferenceValue = counterText;
        so.FindProperty("counterPanel").objectReferenceValue = counter;
        so.FindProperty("completeGroup").objectReferenceValue = completeGroup;
        so.FindProperty("completeText").objectReferenceValue = completeText;
        so.ApplyModifiedPropertiesWithoutUndo();

        if (so.FindProperty("interactor").objectReferenceValue == null)
            Debug.LogWarning("[HUD] Nenhum Interactor encontrado na cena. A HUD vai procurar de novo ao dar Play.");

        // Acabamento sóbrio (sombra + contorno sutil) — mesmo caminho que
        // "Consertar HUD" usa, pra nascer igual nos dois builders.
        StringBuilder acabamento = new StringBuilder();
        DengueHUDReparo.GarantirAcabamentoSobrio(root, "CounterPanel", "Accent", acabamento);
        DengueHUDReparo.GarantirAcabamentoSobrio(root, "ToastPanel", "Faixa", acabamento);
        DengueHUDReparo.GarantirAcabamentoSobrio(root, "TargetPanel", null, acabamento);
        DengueHUDReparo.GarantirAcabamentoSobrio(root, "CompletePanel", null, acabamento);

        EditorSceneManager.MarkSceneDirty(root.scene);
        Selection.activeGameObject = root;
        Debug.Log("[HUD] DengueHUD criada. Os textos de exemplo são trocados pelos valores reais ao dar Play.");
    }


    /* ============================================================
     *  Reconectar: religa as referências do componente DengueHUD aos
     *  objetos da HUD pelo NOME, sem recriar nada. Use depois de
     *  mover, renomear, apagar ou substituir elementos na mão.
     * ============================================================ */
    [MenuItem("Dengue/Reconectar HUD")]
    public static void ReconnectHUD()
    {
        DengueHUD hud = Object.FindFirstObjectByType<DengueHUD>();
        if (hud == null)
        {
            EditorUtility.DisplayDialog("Reconectar HUD",
                "Não encontrei nenhum objeto com o componente DengueHUD na cena aberta.", "OK");
            return;
        }

        Transform root = hud.transform;
        SerializedObject so = new SerializedObject(hud);
        System.Text.StringBuilder faltando = new System.Text.StringBuilder();

        Assign(so, "interactor", Object.FindFirstObjectByType<Interactor>(), faltando);
        Assign(so, "crosshair", Find<RectTransform>(root, "Crosshair"), faltando);
        Assign(so, "crosshairImage", Find<UnityEngine.UI.Image>(root, "Crosshair"), faltando);
        Assign(so, "targetGroup", Group(root, "TargetPanel"), faltando);
        Assign(so, "promptText", Find<TextMeshProUGUI>(root, "PromptText"), faltando);
        Transform row = FindTransform(root, "ProgressRow");
        Assign(so, "progressRow", row != null ? row.gameObject : null, faltando);
        Assign(so, "progressFill", Find<RectTransform>(root, "ProgressFill"), faltando);
        Assign(so, "progressFillImage", Find<UnityEngine.UI.Image>(root, "ProgressFill"), faltando);
        Assign(so, "progressText", Find<TextMeshProUGUI>(root, "ProgressText"), faltando);
        Assign(so, "hintText", Find<TextMeshProUGUI>(root, "HintText"), faltando);
        Assign(so, "toastGroup", Group(root, "ToastPanel"), faltando);
        Assign(so, "toastText", Find<TextMeshProUGUI>(root, "ToastText"), faltando);
        Assign(so, "counterLabel", Find<TextMeshProUGUI>(root, "CounterLabel"), faltando);
        Assign(so, "counterText", Find<TextMeshProUGUI>(root, "CounterText"), faltando);
        Assign(so, "counterPanel", Find<RectTransform>(root, "CounterPanel"), faltando);
        Assign(so, "completeGroup", Group(root, "CompletePanel"), faltando);
        Assign(so, "completeText", Find<TextMeshProUGUI>(root, "CompleteText"), faltando);
        so.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(hud.gameObject.scene);
        Selection.activeGameObject = hud.gameObject;

        if (faltando.Length > 0)
            Debug.LogWarning("[HUD] Reconectado. Não encontrei (esses campos ficaram vazios e são simplesmente ignorados no jogo): " + faltando);
        else
            Debug.Log("[HUD] Referências reconectadas.");
    }

    private static void Assign(SerializedObject so, string field, Object value, System.Text.StringBuilder faltando)
    {
        SerializedProperty prop = so.FindProperty(field);
        if (prop == null)
            return;
        prop.objectReferenceValue = value;
        if (value == null)
        {
            if (faltando.Length > 0) faltando.Append(", ");
            faltando.Append(field);
        }
    }

    /* Procura um filho pelo nome, em qualquer nível (inclusive desativados). */
    private static Transform FindTransform(Transform root, string name)
    {
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            if (t.name == name)
                return t;
        return null;
    }

    private static T Find<T>(Transform root, string name) where T : Component
    {
        Transform t = FindTransform(root, name);
        return t != null ? t.GetComponent<T>() : null;
    }

    /* CanvasGroup do painel; cria se você tiver apagado sem querer. */
    private static CanvasGroup Group(Transform root, string name)
    {
        Transform t = FindTransform(root, name);
        if (t == null)
            return null;
        CanvasGroup cg = t.GetComponent<CanvasGroup>();
        if (cg == null)
        {
            cg = Undo.AddComponent<CanvasGroup>(t.gameObject);
            cg.blocksRaycasts = false;
            cg.interactable = false;
        }
        return cg;
    }

    /* ================= Helpers ================= */

    private static RectTransform CreateRect(string name, RectTransform parent, Vector2 anchor, Vector2 pivot,
        Vector2 position, Vector2 size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.layer = LayerMask.NameToLayer("UI");
        RectTransform rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        SetRect(rt, anchor, anchor, pivot, position, size);
        return rt;
    }

    private static void SetRect(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 position, Vector2 size)
    {
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = position;
        rt.sizeDelta = size;
    }

    private static UnityEngine.UI.Image AddImage(RectTransform rt, Sprite sprite, Color color)
    {
        var img = rt.gameObject.AddComponent<UnityEngine.UI.Image>();
        img.sprite = sprite;
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    private static TextMeshProUGUI CreateText(string name, RectTransform parent, string text, float size,
        TextAlignmentOptions align, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.layer = LayerMask.NameToLayer("UI");
        go.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.alignment = align;
        tmp.color = color;
        tmp.raycastTarget = false;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        return tmp;
    }
}
