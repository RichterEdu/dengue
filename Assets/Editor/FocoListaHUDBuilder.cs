using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/* ============================================================
 *  FocoListaHUDBuilder.cs  (Editor)
 *  ---------------------------------------------------------------------
 *  Cria/conserta o painel "Focos restantes" (tecla Tab) dentro do
 *  Canvas DengueHUD já existente na cena.
 *
 *  Menu:  Dengue > Criar Lista de Focos (Tab)
 *
 *  Hierarquia criada dentro de DengueHUD:
 *    FocoListaPanelSombra          (sombra suave, atrás)
 *    FocoListaPanel                (canto superior direito)
 *    └── FocoListaTexto            ("Focos faltantes: Pneu (4), Vaso de flor (2)...")
 *
 *  (Sessões antigas criavam uma "Faixa" lateral verde brilhante e um
 *  "FocoListaTitulo" separado — os dois são desativados se sobrarem: a
 *  faixa neon foi trocada por sombra+contorno sutil, o mesmo acabamento
 *  do resto da HUD; o texto já vem com o rótulo "Focos faltantes:"
 *  embutido.)
 *
 *  Idempotente: acha pelo nome o que já existe e cria só o que falta,
 *  religando as referências vazias do componente FocoListaHUD. Não
 *  mexe em nada do DengueHUDBuilder/DengueHUDReparo — arquivo isolado
 *  de propósito, para não arriscar a HUD já existente.
 * ============================================================ */

public static class FocoListaHUDBuilder
{
    private static readonly Color CorFundo = new Color(0.07f, 0.09f, 0.11f, 0.96f);
    private static readonly Color CorContorno = new Color(1f, 1f, 1f, 0.10f);
    private static readonly Color CorSombra = new Color(0f, 0f, 0f, 0.35f);

    [MenuItem("Dengue/Criar Lista de Focos (Tab)")]
    public static void Criar()
    {
        GameObject hudRoot = GameObject.Find("DengueHUD");
        if (hudRoot == null)
        {
            EditorUtility.DisplayDialog("Lista de Focos",
                "Não encontrei o Canvas \"DengueHUD\" na cena.\n\nRode primeiro  Dengue > Criar HUD.",
                "OK");
            return;
        }

        RectTransform hudRt = hudRoot.GetComponent<RectTransform>();
        if (hudRt == null)
        {
            Debug.LogError("[Lista de Focos] \"DengueHUD\" não tem RectTransform — não parece ser o Canvas da HUD.");
            return;
        }

        Undo.RegisterFullObjectHierarchyUndo(hudRoot, "Criar Lista de Focos");
        StringBuilder criados = new StringBuilder();
        Sprite uiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        /* ---------- Painel (canto superior direito) ---------- */
        RectTransform painel = Achar<RectTransform>(hudRoot, "FocoListaPanel");
        if (painel == null)
        {
            painel = NovoRect("FocoListaPanel", hudRt, new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-32f, -32f), new Vector2(380f, 96f));
            criados.Append("\n  • FocoListaPanel");
        }

        UnityEngine.UI.Image fundo = painel.GetComponent<UnityEngine.UI.Image>();
        if (fundo == null)
        {
            fundo = Undo.AddComponent<UnityEngine.UI.Image>(painel.gameObject);
            fundo.sprite = uiSprite;
            fundo.type = UnityEngine.UI.Image.Type.Sliced;
            fundo.color = CorFundo;
            fundo.raycastTarget = false;
            criados.Append("\n  • Fundo do painel");
        }

        // Sombra como componente Shadow no PRÓPRIO Image do painel (não uma
        // Image separada): herda o CanvasGroup.alpha do painel automaticamente
        // — uma Image "Sombra" irmã, fora do CanvasGroup, ficava sempre
        // visível mesmo com a lista fechada (bug real desta sessão).
        if (painel.GetComponent<UnityEngine.UI.Shadow>() == null)
        {
            var sombra = Undo.AddComponent<UnityEngine.UI.Shadow>(painel.gameObject);
            sombra.effectColor = CorSombra;
            sombra.effectDistance = new Vector2(2f, -4f);
            criados.Append("\n  • Sombra");
        }

        if (painel.GetComponent<UnityEngine.UI.Outline>() == null)
        {
            var contorno = Undo.AddComponent<UnityEngine.UI.Outline>(painel.gameObject);
            contorno.effectColor = CorContorno;
            contorno.effectDistance = new Vector2(1f, -1f);
            contorno.useGraphicAlpha = false;
            criados.Append("\n  • Contorno sutil");
        }

        CanvasGroup grupo = painel.GetComponent<CanvasGroup>();
        if (grupo == null)
        {
            grupo = Undo.AddComponent<CanvasGroup>(painel.gameObject);
            grupo.blocksRaycasts = false;
            grupo.interactable = false;
            criados.Append("\n  • CanvasGroup do painel");
        }

        UnityEngine.UI.VerticalLayoutGroup vlg = painel.GetComponent<UnityEngine.UI.VerticalLayoutGroup>();
        if (vlg == null)
        {
            vlg = Undo.AddComponent<UnityEngine.UI.VerticalLayoutGroup>(painel.gameObject);
            criados.Append("\n  • VerticalLayoutGroup");
        }
        vlg.padding = new RectOffset(20, 20, 14, 14);
        vlg.spacing = 6f;
        vlg.childAlignment = TextAnchor.UpperLeft;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        UnityEngine.UI.ContentSizeFitter fit = painel.GetComponent<UnityEngine.UI.ContentSizeFitter>();
        if (fit == null)
        {
            fit = Undo.AddComponent<UnityEngine.UI.ContentSizeFitter>(painel.gameObject);
            criados.Append("\n  • ContentSizeFitter");
        }
        fit.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;

        // De sessões antigas: a faixa lateral neon não é mais usada (trocada
        // por sombra+contorno sutil, ver topo do arquivo) — só desativa se sobrou.
        RectTransform faixaAntiga = Achar<RectTransform>(painel.gameObject, "Faixa");
        if (faixaAntiga != null && faixaAntiga.gameObject.activeSelf)
        {
            Undo.RecordObject(faixaAntiga.gameObject, "Criar Lista de Focos");
            faixaAntiga.gameObject.SetActive(false);
            criados.Append("\n  • Desativei Faixa (trocada por sombra+contorno)");
        }

        // De sessões antigas: o título separado não é mais usado (o texto
        // já vem com "Focos faltantes:" embutido) — só desativa se sobrou.
        TextMeshProUGUI titulo = Achar<TextMeshProUGUI>(painel.gameObject, "FocoListaTitulo");
        if (titulo != null && titulo.gameObject.activeSelf)
        {
            Undo.RecordObject(titulo.gameObject, "Criar Lista de Focos");
            titulo.gameObject.SetActive(false);
            criados.Append("\n  • Desativei FocoListaTitulo (texto já vem com o rótulo embutido)");
        }

        TextMeshProUGUI texto = Achar<TextMeshProUGUI>(painel.gameObject, "FocoListaTexto");
        if (texto == null)
        {
            texto = NovoTexto("FocoListaTexto", painel, "Focos faltantes: Pneu (4)", 22,
                TextAlignmentOptions.TopLeft, new Color(1f, 1f, 1f, 0.9f));
            texto.textWrappingMode = TextWrappingModes.Normal;
            criados.Append("\n  • FocoListaTexto");
        }

        /* ---------- Dica permanente sobre a tecla (fora do CanvasGroup do
         * painel, para continuar visível mesmo com a lista fechada) ---------- */
        if (Achar<TextMeshProUGUI>(hudRoot, "FocoListaDica") == null)
        {
            TextMeshProUGUI dica = NovoTexto("FocoListaDica", hudRt, "TAB — focos restantes", 16,
                TextAlignmentOptions.BottomLeft, new Color(1f, 1f, 1f, 0.55f));
            RectTransform dicaRt = dica.rectTransform;
            dicaRt.anchorMin = new Vector2(0f, 0f);
            dicaRt.anchorMax = new Vector2(0f, 0f);
            dicaRt.pivot = new Vector2(0f, 0f);
            dicaRt.anchoredPosition = new Vector2(32f, 32f);
            dicaRt.sizeDelta = new Vector2(360f, 24f);
            criados.Append("\n  • FocoListaDica");
        }

        /* ---------- Componente + referências ---------- */
        FocoListaHUD hud = painel.GetComponent<FocoListaHUD>();
        if (hud == null)
        {
            hud = Undo.AddComponent<FocoListaHUD>(painel.gameObject);
            criados.Append("\n  • Componente FocoListaHUD");
        }

        StringBuilder ligados = new StringBuilder();
        SerializedObject so = new SerializedObject(hud);
        Ligar(so, "painelGrupo", grupo, ligados);
        Ligar(so, "textoLista", texto, ligados);
        so.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(hudRoot.scene);
        Selection.activeGameObject = painel.gameObject;

        StringBuilder msg = new StringBuilder("[Lista de Focos] Concluído.");
        msg.Append(criados.Length > 0 ? "\nCriei:" + criados : "\nNada faltava na hierarquia.");
        msg.Append(ligados.Length > 0 ? "\nLiguei:" + ligados : "\nNenhuma referência nova para ligar.");
        Debug.Log(msg.ToString(), painel.gameObject);
    }

    /* ================= Helpers ================= */

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

    private static RectTransform NovoRect(string nome, RectTransform pai, Vector2 ancora, Vector2 pivo,
        Vector2 pos, Vector2 tamanho)
    {
        GameObject go = new GameObject(nome, typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(go, "Criar Lista de Focos");
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
        Undo.RegisterCreatedObjectUndo(go, "Criar Lista de Focos");
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
}
