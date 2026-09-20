using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/* ============================================================
 *  DengueHUDReparo.cs  (Editor)
 *  ---------------------------------------------------------------------
 *  Conserta a HUD da cena SEM refazer ela do zero.
 *
 *  Menu:  Dengue > Consertar HUD (criar o que falta)
 *
 *  O "Dengue > Criar HUD" APAGA a HUD existente e monta tudo de novo —
 *  o que leva junto qualquer ajuste manual de posição, cor e tamanho.
 *  Esta ferramenta é o contrário: olha o que está faltando, cria só
 *  isso e religa todas as referências pelo nome dos objetos.
 *
 *  O que ela recria, se tiver sumido:
 *   - ToastPanel + ToastText  (o aviso "Foco eliminado!" acima da mira,
 *     que é onde a DICA educativa do foco aparece hoje);
 *   - HintText  (a dica no painel de mira, usada quando o campo
 *     "mostrarDicaAoMirar" da DengueHUD está marcado).
 *
 *  Depois religa: interactor, crosshair, targetGroup, promptText,
 *  progressFill(+Image), progressText, hintText, toastGroup, toastText,
 *  counterLabel, counterText, counterPanel, completeGroup e completeText.
 *
 *  Pode rodar quantas vezes quiser: o que já existe e já está ligado
 *  não é tocado. Tudo passa por Undo.
 * ============================================================ */

public static class DengueHUDReparo
{
    private static readonly Color CorAccent = new Color(0.25f, 1f, 0.8f, 1f);
    private static readonly Color CorPainel = new Color(0.07f, 0.09f, 0.11f, 0.96f);
    private static readonly Color CorContorno = new Color(1f, 1f, 1f, 0.10f);
    private static readonly Color CorSombra = new Color(0f, 0f, 0f, 0.35f);

    [MenuItem("Dengue/Consertar HUD (criar o que falta)")]
    public static void Consertar()
    {
        DengueHUD hud = Object.FindFirstObjectByType<DengueHUD>();
        if (hud == null)
        {
            EditorUtility.DisplayDialog("Consertar HUD",
                "Não encontrei nenhuma DengueHUD na cena aberta.\n\nSe a HUD ainda não existe, rode  Dengue > Criar HUD.",
                "OK");
            return;
        }

        GameObject root = hud.gameObject;
        RectTransform rootRt = root.GetComponent<RectTransform>();
        if (rootRt == null)
        {
            Debug.LogError("[HUD] O objeto da DengueHUD não tem RectTransform — não parece ser o Canvas da HUD.");
            return;
        }

        Undo.RegisterFullObjectHierarchyUndo(root, "Consertar HUD");

        StringBuilder criados = new StringBuilder();

        /* ---------- Aviso de foco eliminado (é onde a dica aparece) ---------- */
        RectTransform toast = Achar<RectTransform>(root, "ToastPanel");
        if (toast == null)
        {
            toast = NovoRect("ToastPanel", rootRt, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -60f), new Vector2(700f, 72f));
            criados.Append("\n  • ToastPanel");
        }

        if (toast.GetComponent<CanvasGroup>() == null)
        {
            CanvasGroup g = Undo.AddComponent<CanvasGroup>(toast.gameObject);
            g.blocksRaycasts = false;
            g.interactable = false;
        }

        TextMeshProUGUI toastText = Achar<TextMeshProUGUI>(root, "ToastText");
        if (toastText == null)
        {
            toastText = NovoTexto("ToastText", toast, "<b>Foco eliminado!</b>", 32,
                TextAlignmentOptions.Center, CorAccent);
            Esticar(toastText.rectTransform);
            // O aviso tem duas linhas (título + dica): sem quebra o texto
            // sairia numa linha só, atravessando a tela.
            toastText.textWrappingMode = TextWrappingModes.Normal;
            criados.Append("\n  • ToastText");
        }

        // Fundo do aviso: sem ele o texto fica em cima do cenário e some
        // no claro. O painel cresce junto com a dica (2 linhas ou mais).
        if (GarantirFundo(toast, toastText))
            criados.Append("\n  • Fundo do aviso (painel + faixa lateral)");

        /* ---------- Dica no painel de mira (opcional) ---------- */
        RectTransform target = Achar<RectTransform>(root, "TargetPanel");
        if (Achar<TextMeshProUGUI>(root, "HintText") == null && target != null)
        {
            TextMeshProUGUI hint = NovoTexto("HintText", target,
                "Dica educativa do foco.", 19, TextAlignmentOptions.Center, new Color(1f, 1f, 1f, 0.65f));
            hint.textWrappingMode = TextWrappingModes.Normal;
            hint.fontStyle = FontStyles.Italic;
            criados.Append("\n  • HintText (dentro do TargetPanel)");
        }

        /* ---------- Acabamento sóbrio (sombra + contorno sutil) em todos os
         * painéis originais — trocando a antiga faixa/accent neon, que
         * combinada com o Bloom da cena criava um halo esverdeado por cima
         * de tudo. Cor de acento passa a aparecer só em texto/barra, não
         * como moldura de painel inteiro. ---------- */
        GarantirAcabamentoSobrio(root, "CounterPanel", "Accent", criados);
        GarantirAcabamentoSobrio(root, "ToastPanel", "Faixa", criados);
        GarantirAcabamentoSobrio(root, "TargetPanel", null, criados);
        GarantirAcabamentoSobrio(root, "CompletePanel", null, criados);

        /* ---------- Religa tudo pelo nome ---------- */
        SerializedObject so = new SerializedObject(hud);
        StringBuilder ligados = new StringBuilder();

        Ligar(so, "interactor", Object.FindFirstObjectByType<Interactor>(), ligados);
        Ligar(so, "crosshair", Achar<RectTransform>(root, "Crosshair"), ligados);
        Ligar(so, "crosshairImage", Achar<UnityEngine.UI.Image>(root, "Crosshair"), ligados);
        Ligar(so, "targetGroup", Grupo(root, "TargetPanel"), ligados);
        Ligar(so, "promptText", Achar<TextMeshProUGUI>(root, "PromptText"), ligados);
        RectTransform row = Achar<RectTransform>(root, "ProgressRow");
        Ligar(so, "progressRow", row != null ? row.gameObject : null, ligados);
        Ligar(so, "progressFill", Achar<RectTransform>(root, "ProgressFill"), ligados);
        Ligar(so, "progressFillImage", Achar<UnityEngine.UI.Image>(root, "ProgressFill"), ligados);
        Ligar(so, "progressText", Achar<TextMeshProUGUI>(root, "ProgressText"), ligados);
        Ligar(so, "hintText", Achar<TextMeshProUGUI>(root, "HintText"), ligados);
        Ligar(so, "toastGroup", Grupo(root, "ToastPanel"), ligados);
        Ligar(so, "toastText", Achar<TextMeshProUGUI>(root, "ToastText"), ligados);
        Ligar(so, "counterLabel", Achar<TextMeshProUGUI>(root, "CounterLabel"), ligados);
        Ligar(so, "counterText", Achar<TextMeshProUGUI>(root, "CounterText"), ligados);
        Ligar(so, "counterPanel", Achar<RectTransform>(root, "CounterPanel"), ligados);
        Ligar(so, "completeGroup", Grupo(root, "CompletePanel"), ligados);
        Ligar(so, "completeText", Achar<TextMeshProUGUI>(root, "CompleteText"), ligados);

        so.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(root.scene);
        Selection.activeGameObject = root;

        StringBuilder msg = new StringBuilder("[HUD] Conserto concluído.");
        msg.Append(criados.Length > 0 ? "\nCriei:" + criados : "\nNada faltava na hierarquia.");
        msg.Append(ligados.Length > 0 ? "\nLiguei:" + ligados : "\nNenhuma referência nova para ligar.");
        Debug.Log(msg.ToString(), root);
    }

    /* Põe fundo no aviso e faz o painel acompanhar a altura do texto.
     * Público porque o DengueHUDBuilder usa o mesmo código ao criar a
     * HUD do zero — assim o aviso nasce igual nos dois caminhos.
     * Devolve true se criou alguma coisa. */
    public static bool GarantirFundo(RectTransform toast, TextMeshProUGUI texto)
    {
        bool criou = false;
        Sprite ui = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        UnityEngine.UI.Image fundo = toast.GetComponent<UnityEngine.UI.Image>();
        if (fundo == null)
        {
            fundo = Undo.AddComponent<UnityEngine.UI.Image>(toast.gameObject);
            fundo.sprite = ui;
            fundo.type = UnityEngine.UI.Image.Type.Sliced;
            fundo.pixelsPerUnitMultiplier = 0.35f;   // cantos bem arredondados
            fundo.color = CorPainel;
            fundo.raycastTarget = false;
            criou = true;
        }

        // (A faixa lateral verde que existia aqui foi trocada pelo acabamento
        // sóbrio — sombra + contorno sutil, aplicado por GarantirAcabamentoSobrio
        // logo depois de GarantirFundo rodar. Ver DengueHUDReparo.Consertar().)

        // O painel passa a ter altura do conteúdo: título curto = painel
        // baixo; com a dica, ele cresce para baixo (o pivô do ToastPanel
        // é (0.5, 1), ancorado no topo da tela).
        UnityEngine.UI.VerticalLayoutGroup vlg = toast.GetComponent<UnityEngine.UI.VerticalLayoutGroup>();
        if (vlg == null)
        {
            vlg = Undo.AddComponent<UnityEngine.UI.VerticalLayoutGroup>(toast.gameObject);
            criou = true;
        }
        vlg.padding = new RectOffset(26, 22, 14, 14);
        vlg.spacing = 4f;
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        UnityEngine.UI.ContentSizeFitter fit = toast.GetComponent<UnityEngine.UI.ContentSizeFitter>();
        if (fit == null)
        {
            fit = Undo.AddComponent<UnityEngine.UI.ContentSizeFitter>(toast.gameObject);
            criou = true;
        }
        fit.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;

        if (texto != null)
        {
            texto.textWrappingMode = TextWrappingModes.Normal;
            texto.alignment = TextAlignmentOptions.Center;
        }

        return criou;
    }

    /* Aplica o acabamento sóbrio (sombra suave atrás + contorno de 1px
     * discreto + fundo na cor unificada CorPainel) num painel que já
     * existe. Desativa a faixa/accent antiga (nome em `nomeFaixaAntiga`,
     * ou null se o painel não tiver uma) se ainda estiver ativa — não
     * apaga (Undo-friendly), só some da tela. Idempotente: pode rodar
     * quantas vezes quiser. Não faz nada se o painel não existir (outro
     * código é responsável por criá-lo). */
    /* Público porque o DengueHUDBuilder também chama isso ao criar a HUD
     * do zero — assim o acabamento nasce igual pelos dois caminhos. */
    public static void GarantirAcabamentoSobrio(GameObject root, string nomePainel, string nomeFaixaAntiga, StringBuilder criados)
    {
        RectTransform painel = Achar<RectTransform>(root, nomePainel);
        if (painel == null)
            return;

        if (nomeFaixaAntiga != null)
        {
            RectTransform faixa = Achar<RectTransform>(painel.gameObject, nomeFaixaAntiga);
            if (faixa != null && faixa.gameObject.activeSelf)
            {
                Undo.RecordObject(faixa.gameObject, "Consertar HUD");
                faixa.gameObject.SetActive(false);
                criados.Append("\n  • Desativei ").Append(nomeFaixaAntiga).Append(" de ").Append(nomePainel);
            }
        }

        UnityEngine.UI.Image fundo = painel.GetComponent<UnityEngine.UI.Image>();
        if (fundo != null && fundo.color != CorPainel)
        {
            Undo.RecordObject(fundo, "Consertar HUD");
            fundo.color = CorPainel;
        }

        if (painel.GetComponent<UnityEngine.UI.Outline>() == null)
        {
            var contorno = Undo.AddComponent<UnityEngine.UI.Outline>(painel.gameObject);
            contorno.effectColor = CorContorno;
            contorno.effectDistance = new Vector2(1f, -1f);
            contorno.useGraphicAlpha = false;
            criados.Append("\n  • Contorno sutil em ").Append(nomePainel);
        }

        // Sombra como componente Shadow no PRÓPRIO Image do painel (não uma
        // Image separada): assim ela é processada pelo mesmo CanvasRenderer
        // e herda o CanvasGroup.alpha do painel automaticamente — uma Image
        // "Sombra" irmã, fora do CanvasGroup, ficava sempre visível mesmo
        // com o painel em alpha 0 (bug real desta sessão, corrigido aqui).
        if (painel.GetComponent<UnityEngine.UI.Shadow>() == null)
        {
            var sombra = Undo.AddComponent<UnityEngine.UI.Shadow>(painel.gameObject);
            sombra.effectColor = CorSombra;
            sombra.effectDistance = new Vector2(2f, -4f);
            criados.Append("\n  • Sombra em ").Append(nomePainel);
        }
    }

    /* ================= Helpers ================= */

    /* Liga só o que está vazio: o que já aponta para alguma coisa fica como está. */
    private static void Ligar(SerializedObject so, string campo, Object valor, StringBuilder ligados)
    {
        SerializedProperty prop = so.FindProperty(campo);
        if (prop == null || valor == null)
            return;
        if (prop.objectReferenceValue != null)
            return;

        prop.objectReferenceValue = valor;
        ligados.Append("\n  • ").Append(campo);
    }

    private static T Achar<T>(GameObject root, string nome) where T : Component
    {
        foreach (T c in root.GetComponentsInChildren<T>(true))
            if (c.gameObject.name == nome)
                return c;
        return null;
    }

    private static CanvasGroup Grupo(GameObject root, string nome)
    {
        RectTransform rt = Achar<RectTransform>(root, nome);
        if (rt == null)
            return null;

        CanvasGroup g = rt.GetComponent<CanvasGroup>();
        if (g == null)
        {
            g = Undo.AddComponent<CanvasGroup>(rt.gameObject);
            g.blocksRaycasts = false;
            g.interactable = false;
        }
        return g;
    }

    private static RectTransform NovoRect(string nome, RectTransform pai, Vector2 ancora, Vector2 pivo,
        Vector2 pos, Vector2 tamanho)
    {
        GameObject go = new GameObject(nome, typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(go, "Consertar HUD");
        go.layer = LayerMask.NameToLayer("UI");
        RectTransform rt = (RectTransform)go.transform;
        rt.SetParent(pai, false);
        rt.anchorMin = ancora;
        rt.anchorMax = ancora;
        rt.pivot = pivo;
        rt.anchoredPosition = pos;
        rt.sizeDelta = tamanho;
        return rt;
    }

    private static TextMeshProUGUI NovoTexto(string nome, RectTransform pai, string texto, float tamanho,
        TextAlignmentOptions alinhamento, Color cor)
    {
        GameObject go = new GameObject(nome, typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(go, "Consertar HUD");
        go.layer = LayerMask.NameToLayer("UI");
        go.transform.SetParent(pai, false);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = texto;
        tmp.fontSize = tamanho;
        tmp.alignment = alinhamento;
        tmp.color = cor;
        tmp.raycastTarget = false;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        return tmp;
    }

    private static void Esticar(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
