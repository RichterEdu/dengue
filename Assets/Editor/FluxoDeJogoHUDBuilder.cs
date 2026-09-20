using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/* ============================================================
 *  FluxoDeJogoHUDBuilder.cs  (Editor)
 *  ---------------------------------------------------------------------
 *  Cria a apresentação do FluxoDeJogo.cs. Acabamento SÓBRIO de propósito
 *  (2ª versão — a 1ª usava uma borda inteira na cor de acento saturada,
 *  que junto com o Bloom da cena criava um halo/vazamento verde-água
 *  por cima dos painéis, parecendo "gerado por IA"): sombra suave atrás
 *  do painel + contorno de 1px bem discreto (branco baixo alpha, não a
 *  cor de acento) + fundo quase opaco. A cor de acento fica reservada
 *  pra detalhes pequenos (ícone, barra, título), não pra molduras
 *  inteiras.
 *
 *    - OcupacaoPanel   dentro do Canvas DengueHUD já existente (canto
 *      inferior central) — a barra de ocupação hospitalar.
 *    - FluxoDeJogoCanvas (novo, sortingOrder 20 — acima de tudo)
 *      com BriefingPanel, VitoriaPanel e DerrotaPanel.
 *
 *  Menu:  Dengue > Criar HUD de Fim de Jogo
 *
 *  Pré-requisito: rodar antes  Dengue > Criar Sala do Hospital  (cria o
 *  componente FluxoDeJogo que esta HUD liga).
 *
 *  Idempotente: acha pelo nome o que já existe e cria só o que falta.
 * ============================================================ */

public static class FluxoDeJogoHUDBuilder
{
    // Paleta nomeada — acento usado com moderação, não como moldura.
    private static readonly Color CorFundo = new Color(0.07f, 0.09f, 0.11f, 0.96f);
    private static readonly Color CorContorno = new Color(1f, 1f, 1f, 0.10f);
    private static readonly Color CorSombra = new Color(0f, 0f, 0f, 0.35f);
    private static readonly Color CorLinha = new Color(1f, 1f, 1f, 0.08f);
    private static readonly Color CorTexto = new Color(0.95f, 0.96f, 0.97f, 1f);
    private static readonly Color CorTextoFraco = new Color(1f, 1f, 1f, 0.55f);
    private static readonly Color AccentColor = new Color(0.25f, 1f, 0.8f, 1f);
    private static readonly Color AccentDerrota = new Color(1f, 0.4f, 0.4f, 1f);
    private static readonly Color BarBgColor = new Color(1f, 1f, 1f, 0.14f);

    private const string BriefingPadrao =
        "A equipe médica avisa: erradique os criadouros no bairro. Sem isso, o tratamento não adianta.";

    private static Sprite uiSprite;
    private static Sprite knobSprite;

    [MenuItem("Dengue/Criar HUD de Fim de Jogo")]
    public static void Criar()
    {
        FluxoDeJogo fluxo = Object.FindFirstObjectByType<FluxoDeJogo>();
        if (fluxo == null)
        {
            EditorUtility.DisplayDialog("HUD de Fim de Jogo",
                "Não encontrei o componente FluxoDeJogo na cena.\n\nRode primeiro  Dengue > Criar Sala do Hospital.",
                "OK");
            return;
        }

        GameObject hudRoot = GameObject.Find("DengueHUD");
        if (hudRoot == null)
        {
            EditorUtility.DisplayDialog("HUD de Fim de Jogo",
                "Não encontrei o Canvas \"DengueHUD\" na cena.\n\nRode primeiro  Dengue > Criar HUD.",
                "OK");
            return;
        }
        RectTransform hudRt = hudRoot.GetComponent<RectTransform>();

        uiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        knobSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

        StringBuilder criados = new StringBuilder();

        /* ================= Barra de ocupação (dentro do DengueHUD) ================= */

        RectTransform ocupacaoPanel = Achar<RectTransform>(hudRoot, "OcupacaoPanel");
        if (ocupacaoPanel == null)
        {
            ocupacaoPanel = PainelSobrio("OcupacaoPanel", hudRt, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 86f), new Vector2(560f, 84f));

            IconeSaude(ocupacaoPanel, new Vector2(28f, -42f), 30f, AccentColor);

            TextMeshProUGUI titulo = NovoTexto("OcupacaoTitulo", ocupacaoPanel, "OCUPAÇÃO HOSPITALAR", 15,
                TextAlignmentOptions.Left, CorTextoFraco);
            titulo.characterSpacing = 2;
            SetRect(titulo.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f),
                new Vector2(54f, -10f), new Vector2(-84f, 20f));

            TextMeshProUGUI texto = NovoTexto("OcupacaoTexto", ocupacaoPanel, "70%", 22,
                TextAlignmentOptions.Right, CorTexto);
            texto.fontStyle = FontStyles.Bold;
            SetRect(texto.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-16f, -7f), new Vector2(76f, 24f));

            LinhaDivisoria(ocupacaoPanel, -32f);

            RectTransform barraFundo = NovoRect("OcupacaoBarra", ocupacaoPanel, new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(20f, 16f), new Vector2(520f, 12f));
            var barraImg = NovaImagem(barraFundo, uiSprite, BarBgColor);
            barraImg.type = UnityEngine.UI.Image.Type.Sliced;
            barraImg.pixelsPerUnitMultiplier = 3f;

            RectTransform fill = NovoRect("OcupacaoFill", barraFundo, Vector2.zero, new Vector2(0f, 0.5f), Vector2.zero, Vector2.zero);
            fill.anchorMax = new Vector2(0.7f, 1f);
            fill.offsetMin = Vector2.zero;
            fill.offsetMax = Vector2.zero;
            var fillImg = NovaImagem(fill, uiSprite, AccentColor);
            fillImg.type = UnityEngine.UI.Image.Type.Sliced;
            fillImg.pixelsPerUnitMultiplier = 3f;

            criados.Append("\n  • OcupacaoPanel (dentro do DengueHUD)");
        }

        /* ================= Canvas de fim de jogo ================= */

        GameObject fimRoot = GameObject.Find("FluxoDeJogoCanvas");
        RectTransform fimRt;
        if (fimRoot == null)
        {
            fimRoot = new GameObject("FluxoDeJogoCanvas", typeof(RectTransform), typeof(Canvas),
                typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
            Undo.RegisterCreatedObjectUndo(fimRoot, "Criar HUD de Fim de Jogo");

            Canvas canvas = fimRoot.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20;

            var scaler = fimRoot.GetComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            criados.Append("\n  • FluxoDeJogoCanvas");
        }
        fimRt = (RectTransform)fimRoot.transform;

        /* ---------- Briefing ---------- */
        RectTransform briefing = Achar<RectTransform>(fimRoot, "BriefingPanel");
        if (briefing == null)
        {
            briefing = PainelSobrio("BriefingPanel", fimRt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(920f, 190f));
            briefing.gameObject.AddComponent<CanvasGroup>();

            IconeSaude(briefing, new Vector2(0f, 34f), 38f, AccentColor);

            TextMeshProUGUI briefingTexto = NovoTexto("BriefingTexto", briefing, BriefingPadrao, 27,
                TextAlignmentOptions.Center, CorTexto);
            briefingTexto.textWrappingMode = TextWrappingModes.Normal;
            SetRect(briefingTexto.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
                new Vector2(0f, -18f), new Vector2(-90f, -90f));

            criados.Append("\n  • BriefingPanel");
        }

        /* ---------- Vitória ---------- */
        RectTransform vitoria = Achar<RectTransform>(fimRoot, "VitoriaPanel");
        if (vitoria == null)
        {
            vitoria = NovoRect("VitoriaPanel", fimRt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            Esticar(vitoria, 0f);
            NovaImagem(vitoria, null, new Color(0.02f, 0.03f, 0.03f, 0.80f));
            vitoria.gameObject.AddComponent<CanvasGroup>();

            RectTransform vitoriaCaixa = PainelSobrio("VitoriaCaixa", vitoria, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(720f, 400f));
            IconeSaude(vitoriaCaixa, new Vector2(0f, 106f), 46f, AccentColor);

            TextMeshProUGUI vitoriaTitulo = NovoTexto("VitoriaTitulo", vitoriaCaixa, "MISSÃO CUMPRIDA!", 32,
                TextAlignmentOptions.Center, AccentColor);
            vitoriaTitulo.fontStyle = FontStyles.Bold;
            SetRect(vitoriaTitulo.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -62f), new Vector2(-40f, 42f));

            LinhaDivisoria(vitoriaCaixa, -98f);

            TextMeshProUGUI vitoriaTexto = NovoTexto("VitoriaTexto", vitoriaCaixa, "", 21,
                TextAlignmentOptions.Center, CorTexto);
            vitoriaTexto.textWrappingMode = TextWrappingModes.Normal;
            SetRect(vitoriaTexto.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -66f), new Vector2(-60f, -132f));

            criados.Append("\n  • VitoriaPanel");
        }

        /* ---------- Derrota ---------- */
        RectTransform derrota = Achar<RectTransform>(fimRoot, "DerrotaPanel");
        if (derrota == null)
        {
            derrota = NovoRect("DerrotaPanel", fimRt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            Esticar(derrota, 0f);
            NovaImagem(derrota, null, new Color(0.04f, 0.02f, 0.02f, 0.82f));
            derrota.gameObject.AddComponent<CanvasGroup>();

            RectTransform derrotaCaixa = PainelSobrio("DerrotaCaixa", derrota, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(720f, 400f));
            IconeSaude(derrotaCaixa, new Vector2(0f, 106f), 46f, AccentDerrota);

            TextMeshProUGUI derrotaTitulo = NovoTexto("DerrotaTitulo", derrotaCaixa, "HOSPITAL EM COLAPSO", 32,
                TextAlignmentOptions.Center, AccentDerrota);
            derrotaTitulo.fontStyle = FontStyles.Bold;
            SetRect(derrotaTitulo.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -62f), new Vector2(-40f, 42f));

            LinhaDivisoria(derrotaCaixa, -98f);

            TextMeshProUGUI derrotaTexto = NovoTexto("DerrotaTexto", derrotaCaixa, "", 21,
                TextAlignmentOptions.Center, CorTexto);
            derrotaTexto.textWrappingMode = TextWrappingModes.Normal;
            SetRect(derrotaTexto.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -66f), new Vector2(-60f, -132f));

            criados.Append("\n  • DerrotaPanel");
        }

        /* ================= Componente + referências ================= */

        FluxoDeJogoHUD hud = fimRoot.GetComponent<FluxoDeJogoHUD>();
        if (hud == null)
        {
            hud = Undo.AddComponent<FluxoDeJogoHUD>(fimRoot);
            criados.Append("\n  • Componente FluxoDeJogoHUD");
        }

        StringBuilder ligados = new StringBuilder();
        SerializedObject so = new SerializedObject(hud);
        Ligar(so, "fluxo", fluxo, ligados);
        Ligar(so, "interactor", Object.FindFirstObjectByType<Interactor>(), ligados);
        Ligar(so, "briefingGrupo", Achar<RectTransform>(fimRoot, "BriefingPanel")?.GetComponent<CanvasGroup>(), ligados);
        Ligar(so, "briefingTexto", Achar<TextMeshProUGUI>(fimRoot, "BriefingTexto"), ligados);
        Ligar(so, "ocupacaoFill", Achar<RectTransform>(ocupacaoPanel.gameObject, "OcupacaoFill"), ligados);
        Ligar(so, "ocupacaoFillImage", Achar<UnityEngine.UI.Image>(ocupacaoPanel.gameObject, "OcupacaoFill"), ligados);
        Ligar(so, "ocupacaoTexto", Achar<TextMeshProUGUI>(ocupacaoPanel.gameObject, "OcupacaoTexto"), ligados);
        Ligar(so, "vitoriaGrupo", Achar<RectTransform>(fimRoot, "VitoriaPanel")?.GetComponent<CanvasGroup>(), ligados);
        Ligar(so, "vitoriaTexto", Achar<TextMeshProUGUI>(fimRoot, "VitoriaTexto"), ligados);
        Ligar(so, "derrotaGrupo", Achar<RectTransform>(fimRoot, "DerrotaPanel")?.GetComponent<CanvasGroup>(), ligados);
        Ligar(so, "derrotaTexto", Achar<TextMeshProUGUI>(fimRoot, "DerrotaTexto"), ligados);
        so.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(fimRoot.scene);
        Selection.activeGameObject = fimRoot;

        StringBuilder msg = new StringBuilder("[HUD de Fim de Jogo] Concluído.");
        msg.Append(criados.Length > 0 ? "\nCriei:" + criados : "\nNada faltava na hierarquia.");
        msg.Append(ligados.Length > 0 ? "\nLiguei:" + ligados : "\nNenhuma referência nova para ligar.");
        Debug.Log(msg.ToString(), fimRoot);
    }

    /* ================= Composição visual ================= */

    /* Painel sóbrio: fundo quase opaco + sombra suave e contorno de 1px
     * sutil aplicados como MODIFICADORES DO PRÓPRIO Image (componentes
     * Shadow/Outline do uGUI, que desenham uma cópia deslocada da mesma
     * malha) em vez de uma segunda Image separada. Isso importa por dois
     * motivos: (1) Shadow/Outline são processados pelo mesmo
     * CanvasRenderer do painel, então herdam o CanvasGroup.alpha dele
     * automaticamente — uma Image "Sombra" irmã, fora do CanvasGroup,
     * ficava sempre visível mesmo com o painel em alpha 0 (bug real desta
     * sessão); (2) acompanham o tamanho do painel se ele crescer via
     * ContentSizeFitter, sem precisar recalcular nada. Devolve o próprio
     * painel (o conteúdo entra nele direto). */
    private static RectTransform PainelSobrio(string nome, RectTransform pai, Vector2 ancora, Vector2 pivo,
        Vector2 pos, Vector2 tamanho)
    {
        RectTransform painel = NovoRect(nome, pai, ancora, pivo, pos, tamanho);
        var img = NovaImagem(painel, uiSprite, CorFundo);
        img.type = UnityEngine.UI.Image.Type.Sliced;

        UnityEngine.UI.Shadow sombra = painel.gameObject.AddComponent<UnityEngine.UI.Shadow>();
        sombra.effectColor = CorSombra;
        sombra.effectDistance = new Vector2(2f, -4f);

        UnityEngine.UI.Outline contorno = painel.gameObject.AddComponent<UnityEngine.UI.Outline>();
        contorno.effectColor = CorContorno;
        contorno.effectDistance = new Vector2(1f, -1f);
        contorno.useGraphicAlpha = false;

        return painel;
    }

    /* Linha divisória fina — separa título de conteúdo dentro de um painel. */
    private static void LinhaDivisoria(RectTransform pai, float y)
    {
        RectTransform linha = NovoRect("Divisor", pai, new Vector2(0f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, y), new Vector2(0f, 1f));
        linha.anchorMax = new Vector2(1f, 1f);
        linha.offsetMin = new Vector2(18f, y);
        linha.offsetMax = new Vector2(-18f, y + 1f);
        NovaImagem(linha, null, CorLinha);
    }

    /* Ícone de "saúde" simulado com formas de UI: círculo discreto (fundo
     * semi-transparente na cor passada, não 100% opaco — evita alimentar
     * o Bloom com um disco saturado) + duas barras formando um "+". */
    private static void IconeSaude(RectTransform pai, Vector2 pos, float tamanho, Color corCirculo)
    {
        RectTransform circulo = NovoRect("IconeCirculo", pai, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            pos, new Vector2(tamanho, tamanho));
        Color corSuave = corCirculo;
        corSuave.a = 0.22f;
        NovaImagem(circulo, knobSprite, corSuave);

        float barra = tamanho * 0.46f;
        float espessura = tamanho * 0.13f;

        RectTransform h = NovoRect("IconeBarraH", circulo, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(barra, espessura));
        NovaImagem(h, null, corCirculo);

        RectTransform v = NovoRect("IconeBarraV", circulo, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(espessura, barra));
        NovaImagem(v, null, corCirculo);
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
        Undo.RegisterCreatedObjectUndo(go, "Criar HUD de Fim de Jogo");
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

    private static void SetRect(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 position, Vector2 size)
    {
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = position;
        rt.sizeDelta = size;
    }

    /* Estica pra preencher o pai inteiro, com uma margem opcional em todos os lados. */
    private static void Esticar(RectTransform rt, float margem)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = new Vector2(margem, margem);
        rt.offsetMax = new Vector2(-margem, -margem);
    }

    private static UnityEngine.UI.Image NovaImagem(RectTransform rt, Sprite sprite, Color cor)
    {
        var img = rt.gameObject.AddComponent<UnityEngine.UI.Image>();
        img.sprite = sprite;
        img.color = cor;
        img.raycastTarget = false;
        return img;
    }

    private static TextMeshProUGUI NovoTexto(string nome, RectTransform pai, string texto, float tamanho,
        TextAlignmentOptions alinhamento, Color cor)
    {
        GameObject go = new GameObject(nome, typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(go, "Criar HUD de Fim de Jogo");
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
