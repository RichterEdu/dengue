using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/* ============================================================
 *  CelularBuilder.cs  (Editor)
 *  ---------------------------------------------------------------------
 *  Gera o CELULAR do agente — tudo na HUD, em 2D. Nada em 3D.
 *
 *  Menu:  Dengue > Criar Celular
 *
 *  Hierarquia criada (um Canvas só, Screen Space Overlay):
 *    CelularHUD            (RedeSocial + CelularController + AudioSource)
 *    ├── Painel            (o celular; sobe do canto inferior direito)
 *    │   ├── StatusBar     (hora, sinal, bateria)
 *    │   ├── Header        (VizinhApp + contador de focos + "ao vivo")
 *    │   ├── Viewport      (RectMask2D)
 *    │   │   └── Conteudo  (lista) > ModeloPost   (card modelo, PostView)
 *    │   ├── Vazio         ("nenhuma publicação ainda")
 *    │   └── Rodape        ("[T] fechar")
 *    └── Icone             (ícone + badge de não lidos + dica [T])
 *
 *  Também LIMPA a versão antiga em 3D, se existir: apaga o objeto
 *  "Celular" que ficava dentro da Main Camera, devolve o Near Clip
 *  Plane para 0.3 e apaga a pasta Assets/Meus/Celular (malhas e
 *  materiais do aparelho).
 *
 *  Pode rodar de novo quantas vezes quiser: substitui o que existir.
 * ============================================================ */

public static class CelularBuilder
{
    private const string NomeHUD = "CelularHUD";
    private const string PastaAntiga3D = "Assets/Meus/Celular";

    /* ---- Medidas (referência 1920x1080) ---- */
    private const float PainelW = 420f;
    private const float PainelH = 780f;
    private const float Margem = 40f;
    private const float AlturaStatus = 44f;
    private const float AlturaHeader = 84f;
    private const float AlturaRodape = 46f;

    /* ---- Cores ---- */
    private static readonly Color CorFundoApp = new Color(0.055f, 0.067f, 0.086f, 0.98f);
    private static readonly Color CorCard = new Color(0.086f, 0.106f, 0.133f, 1f);
    private static readonly Color CorTexto = new Color(0.902f, 0.929f, 0.953f, 1f);
    private static readonly Color CorTextoFraco = new Color(0.545f, 0.580f, 0.620f, 1f);
    private static readonly Color CorAccent = new Color(0.25f, 1f, 0.8f, 1f);
    private static readonly Color CorLinha = new Color(1f, 1f, 1f, 0.09f);
    private static readonly Color CorBorda = new Color(1f, 1f, 1f, 0.14f);

    [MenuItem("Dengue/Criar Celular")]
    public static void Criar()
    {
        // TextMeshPro precisa dos "Essential Resources".
        if (TMP_Settings.instance == null)
        {
            TMP_PackageResourceImporter.ImportResources(true, false, false);
            EditorUtility.DisplayDialog("Criar Celular",
                "Importei os recursos essenciais do TextMeshPro.\n\nEspere o Unity terminar e rode  Dengue > Criar Celular  de novo.",
                "OK");
            return;
        }

        LimparVersao3D();

        GameObject antigo = GameObject.Find(NomeHUD);
        if (antigo != null)
            Undo.DestroyObjectImmediate(antigo);

        Sprite ui = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        Sprite knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

        /* ---------- Canvas ---------- */
        GameObject root = new GameObject(NomeHUD, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        Undo.RegisterCreatedObjectUndo(root, "Criar Celular");
        root.layer = LayerMask.NameToLayer("UI");

        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 11;              // acima da DengueHUD (10)

        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        RectTransform rootRt = (RectTransform)root.transform;

        /* ---------- Painel do celular ---------- */
        RectTransform painel = NovoRect("Painel", rootRt);
        painel.anchorMin = new Vector2(1f, 0f);
        painel.anchorMax = new Vector2(1f, 0f);
        painel.pivot = new Vector2(1f, 0f);
        painel.anchoredPosition = new Vector2(-Margem, Margem);
        painel.sizeDelta = new Vector2(PainelW, PainelH);
        CanvasGroup painelGrupo = painel.gameObject.AddComponent<CanvasGroup>();
        painelGrupo.blocksRaycasts = false;
        painelGrupo.interactable = false;
        painelGrupo.alpha = 0f;

        Img(painel, ui, CorFundoApp, 0.22f);

        RectTransform borda = NovoRect("Borda", painel);
        Esticar(borda, 0, 0, 0, 0);
        Img(borda, ui, CorBorda, 0.22f);
        RectTransform dentro = NovoRect("Dentro", borda);
        Esticar(dentro, 2, 2, 2, 2);
        Img(dentro, ui, CorFundoApp, 0.22f);

        /* ---------- Barra de status ---------- */
        RectTransform status = NovoRect("StatusBar", painel);
        status.anchorMin = new Vector2(0f, 1f);
        status.anchorMax = new Vector2(1f, 1f);
        status.pivot = new Vector2(0.5f, 1f);
        status.anchoredPosition = Vector2.zero;
        status.sizeDelta = new Vector2(0f, AlturaStatus);

        TextMeshProUGUI relogio = Txt("Hora", status, "18:42", 18, TextAlignmentOptions.Left, CorTexto);
        relogio.fontStyle = FontStyles.Bold;
        Ancorar(relogio.rectTransform, new Vector2(0f, 0.5f), new Vector2(18f, 0f), new Vector2(140f, 26f));
        relogio.rectTransform.pivot = new Vector2(0f, 0.5f);

        for (int i = 0; i < 3; i++)
        {
            RectTransform barra = NovoRect("Sinal " + (i + 1), status);
            Ancorar(barra, new Vector2(1f, 0.5f), new Vector2(-70f + i * 7f, -2f + i * 2f), new Vector2(5f, 6f + i * 4f));
            Img(barra, ui, new Color(1f, 1f, 1f, 0.7f), 1.5f);
        }

        RectTransform bat = NovoRect("Bateria", status);
        Ancorar(bat, new Vector2(1f, 0.5f), new Vector2(-30f, 0f), new Vector2(28f, 14f));
        Img(bat, ui, new Color(1f, 1f, 1f, 0.32f), 2f);
        RectTransform batFill = NovoRect("BateriaFill", bat);
        batFill.anchorMin = Vector2.zero;
        batFill.anchorMax = new Vector2(0.72f, 1f);
        batFill.offsetMin = new Vector2(3f, 3f);
        batFill.offsetMax = new Vector2(-3f, -3f);
        Img(batFill, ui, new Color(1f, 1f, 1f, 0.8f), 2.5f);

        /* ---------- Cabeçalho ---------- */
        RectTransform header = NovoRect("Header", painel);
        header.anchorMin = new Vector2(0f, 1f);
        header.anchorMax = new Vector2(1f, 1f);
        header.pivot = new Vector2(0.5f, 1f);
        header.anchoredPosition = new Vector2(0f, -AlturaStatus);
        header.sizeDelta = new Vector2(0f, AlturaHeader);

        TextMeshProUGUI titulo = Txt("Titulo", header, "VizinhApp", 28, TextAlignmentOptions.Left, CorTexto);
        titulo.fontStyle = FontStyles.Bold;
        Ancorar(titulo.rectTransform, new Vector2(0f, 1f), new Vector2(18f, -10f), new Vector2(240f, 34f));
        titulo.rectTransform.pivot = new Vector2(0f, 1f);

        TextMeshProUGUI subtitulo = Txt("Subtitulo", header, "Vila Nova · 0/0 focos eliminados", 16,
            TextAlignmentOptions.Left, CorTextoFraco);
        Ancorar(subtitulo.rectTransform, new Vector2(0f, 1f), new Vector2(18f, -44f), new Vector2(330f, 22f));
        subtitulo.rectTransform.pivot = new Vector2(0f, 1f);

        RectTransform ponto = NovoRect("PontoAoVivo", header);
        Ancorar(ponto, new Vector2(1f, 1f), new Vector2(-76f, -24f), new Vector2(9f, 9f));
        Img(ponto, knob, CorAccent, 1f);

        TextMeshProUGUI aoVivo = Txt("AoVivo", header, "ao vivo", 15, TextAlignmentOptions.Left, CorAccent);
        Ancorar(aoVivo.rectTransform, new Vector2(1f, 1f), new Vector2(-64f, -24f), new Vector2(64f, 20f));
        aoVivo.rectTransform.pivot = new Vector2(0f, 0.5f);

        RectTransform divisor = NovoRect("Divisor", header);
        divisor.anchorMin = new Vector2(0f, 0f);
        divisor.anchorMax = new Vector2(1f, 0f);
        divisor.pivot = new Vector2(0.5f, 0f);
        divisor.anchoredPosition = Vector2.zero;
        divisor.sizeDelta = new Vector2(-28f, 1.5f);
        Img(divisor, null, CorLinha, 1f);

        /* ---------- Feed ---------- */
        RectTransform viewport = NovoRect("Viewport", painel);
        Esticar(viewport, 2f, 2f, AlturaStatus + AlturaHeader, AlturaRodape);
        viewport.gameObject.AddComponent<RectMask2D>();

        RectTransform conteudo = NovoRect("Conteudo", viewport);
        conteudo.anchorMin = new Vector2(0f, 1f);
        conteudo.anchorMax = new Vector2(1f, 1f);
        conteudo.pivot = new Vector2(0.5f, 1f);
        conteudo.anchoredPosition = Vector2.zero;
        conteudo.sizeDelta = new Vector2(0f, 100f);

        VerticalLayoutGroup vlg = conteudo.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(14, 14, 12, 20);
        vlg.spacing = 12f;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        ContentSizeFitter fit = conteudo.gameObject.AddComponent<ContentSizeFitter>();
        fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        PostView modelo = MontarCard(conteudo, ui, knob);

        TextMeshProUGUI vazioTxt = Txt("Vazio", painel, "Nenhuma publicação ainda.\nO bairro está quieto… por enquanto.",
            17, TextAlignmentOptions.Center, CorTextoFraco);
        vazioTxt.textWrappingMode = TextWrappingModes.Normal;
        Ancorar(vazioTxt.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, -20f), new Vector2(340f, 90f));
        CanvasGroup vazio = vazioTxt.gameObject.AddComponent<CanvasGroup>();

        /* ---------- Rodapé ---------- */
        RectTransform rodape = NovoRect("Rodape", painel);
        rodape.anchorMin = new Vector2(0f, 0f);
        rodape.anchorMax = new Vector2(1f, 0f);
        rodape.pivot = new Vector2(0.5f, 0f);
        rodape.anchoredPosition = Vector2.zero;
        rodape.sizeDelta = new Vector2(0f, AlturaRodape);

        RectTransform linhaRodape = NovoRect("Linha", rodape);
        linhaRodape.anchorMin = new Vector2(0f, 1f);
        linhaRodape.anchorMax = new Vector2(1f, 1f);
        linhaRodape.pivot = new Vector2(0.5f, 1f);
        linhaRodape.anchoredPosition = Vector2.zero;
        linhaRodape.sizeDelta = new Vector2(-28f, 1.5f);
        Img(linhaRodape, null, CorLinha, 1f);

        TextMeshProUGUI dicaFechar = Txt("DicaFechar", rodape, "[T] fechar  ·  rodinha do mouse rola o feed", 15,
            TextAlignmentOptions.Center, new Color(1f, 1f, 1f, 0.45f));
        Ancorar(dicaFechar.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, -4f), new Vector2(400f, 22f));

        /* ---------- Ícone ---------- */
        RectTransform icone = NovoRect("Icone", rootRt);
        icone.anchorMin = new Vector2(1f, 0f);
        icone.anchorMax = new Vector2(1f, 0f);
        icone.pivot = new Vector2(1f, 0f);
        icone.anchoredPosition = new Vector2(-Margem, Margem);
        icone.sizeDelta = new Vector2(54f, 94f);
        CanvasGroup iconeGrupo = icone.gameObject.AddComponent<CanvasGroup>();
        iconeGrupo.blocksRaycasts = false;
        iconeGrupo.interactable = false;

        Img(icone, ui, new Color(0.06f, 0.09f, 0.11f, 0.88f), 0.45f);

        RectTransform iconeBorda = NovoRect("Borda", icone);
        Esticar(iconeBorda, 2, 2, 2, 2);
        Img(iconeBorda, ui, CorBorda, 0.5f);

        RectTransform iconeTelaRt = NovoRect("Tela", icone);
        Esticar(iconeTelaRt, 7, 7, 10, 13);
        Image iconeTela = Img(iconeTelaRt, ui, new Color(0.25f, 1f, 0.8f, 0.30f), 0.7f);

        RectTransform slot = NovoRect("Alto-falante", icone);
        Ancorar(slot, new Vector2(0.5f, 1f), new Vector2(0f, -6f), new Vector2(16f, 2.5f));
        Img(slot, ui, new Color(1f, 1f, 1f, 0.35f), 2f);

        RectTransform home = NovoRect("Home", icone);
        Ancorar(home, new Vector2(0.5f, 0f), new Vector2(0f, 7f), new Vector2(18f, 3f));
        Img(home, ui, new Color(1f, 1f, 1f, 0.35f), 2f);

        RectTransform badge = NovoRect("Badge", icone);
        badge.anchorMin = new Vector2(1f, 1f);
        badge.anchorMax = new Vector2(1f, 1f);
        badge.pivot = new Vector2(0.5f, 0.5f);
        badge.anchoredPosition = new Vector2(2f, 2f);
        badge.sizeDelta = new Vector2(28f, 28f);
        Img(badge, knob, new Color(0.97f, 0.32f, 0.29f, 1f), 1f);

        TextMeshProUGUI badgeTexto = Txt("BadgeTexto", badge, "3", 16, TextAlignmentOptions.Center, Color.white);
        badgeTexto.fontStyle = FontStyles.Bold;
        Esticar(badgeTexto.rectTransform, 0, 0, 0, 0);
        badge.gameObject.SetActive(false);

        TextMeshProUGUI dica = Txt("Dica", icone, "[T]", 15, TextAlignmentOptions.Center, new Color(1f, 1f, 1f, 0.55f));
        Ancorar(dica.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, -18f), new Vector2(80f, 20f));

        /* ---------- Componentes ---------- */
        RedeSocial rede = root.AddComponent<RedeSocial>();
        AudioSource audio = root.AddComponent<AudioSource>();
        audio.playOnAwake = false;
        audio.spatialBlend = 0f;

        FeedRedeSocial feed = painel.gameObject.AddComponent<FeedRedeSocial>();
        SerializedObject soFeed = new SerializedObject(feed);
        soFeed.FindProperty("rede").objectReferenceValue = rede;
        soFeed.FindProperty("viewport").objectReferenceValue = viewport;
        soFeed.FindProperty("conteudo").objectReferenceValue = conteudo;
        soFeed.FindProperty("modelo").objectReferenceValue = modelo;
        soFeed.FindProperty("relogio").objectReferenceValue = relogio;
        soFeed.FindProperty("subtitulo").objectReferenceValue = subtitulo;
        soFeed.FindProperty("vazio").objectReferenceValue = vazio;
        soFeed.FindProperty("passoScroll").floatValue = 110f;
        soFeed.ApplyModifiedPropertiesWithoutUndo();

        CelularController controller = root.AddComponent<CelularController>();
        SerializedObject so = new SerializedObject(controller);
        so.FindProperty("painel").objectReferenceValue = painel;
        so.FindProperty("painelGrupo").objectReferenceValue = painelGrupo;
        so.FindProperty("posAberto").vector2Value = new Vector2(-Margem, Margem);
        so.FindProperty("posFechado").vector2Value = new Vector2(-Margem, -(PainelH + 60f));
        so.FindProperty("rede").objectReferenceValue = rede;
        so.FindProperty("feed").objectReferenceValue = feed;
        so.FindProperty("interactor").objectReferenceValue = Object.FindFirstObjectByType<Interactor>();
        so.FindProperty("icone").objectReferenceValue = icone;
        so.FindProperty("iconeGrupo").objectReferenceValue = iconeGrupo;
        so.FindProperty("iconeTela").objectReferenceValue = iconeTela;
        so.FindProperty("badge").objectReferenceValue = badge.gameObject;
        so.FindProperty("badgeTexto").objectReferenceValue = badgeTexto;
        so.FindProperty("audioSource").objectReferenceValue = audio;

        GameObject hudJogo = GameObject.Find("DengueHUD");
        if (hudJogo != null)
            so.FindProperty("hudDoJogo").objectReferenceValue = hudJogo.GetComponent<Canvas>();
        so.ApplyModifiedPropertiesWithoutUndo();

        painel.anchoredPosition = new Vector2(-Margem, -(PainelH + 60f));

        EditorSceneManager.MarkSceneDirty(root.scene);
        Selection.activeGameObject = root;
        Debug.Log("[Celular] HUD criada. Dê Play e aperte T. " +
                  "As publicações estão no componente RedeSocial (objeto CelularHUD).");
    }

    /* ================= Limpeza da versão 3D ================= */

    private static void LimparVersao3D()
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            Transform velho = cam.transform.Find("Celular");
            if (velho != null)
            {
                Undo.DestroyObjectImmediate(velho.gameObject);
                Debug.Log("[Celular] Removi o aparelho 3D que ficava dentro da Main Camera.");
            }

            // O builder antigo tinha baixado o near clip por causa do aparelho 3D.
            if (Mathf.Abs(cam.nearClipPlane - 0.03f) < 0.0001f)
            {
                Undo.RecordObject(cam, "Restaurar near clip");
                cam.nearClipPlane = 0.3f;
                Debug.Log("[Celular] Near Clip Plane da câmera devolvido para 0.3.");
            }
        }

        if (AssetDatabase.IsValidFolder(PastaAntiga3D))
        {
            AssetDatabase.DeleteAsset(PastaAntiga3D);
            AssetDatabase.Refresh();
            Debug.Log("[Celular] Apaguei " + PastaAntiga3D + " (malhas e materiais do aparelho 3D).");
        }
    }

    /* ================= Card de publicação ================= */

    private static PostView MontarCard(RectTransform pai, Sprite ui, Sprite knob)
    {
        RectTransform card = NovoRect("ModeloPost", pai);
        card.sizeDelta = new Vector2(0f, 140f);
        Img(card, ui, CorCard, 0.5f);
        CanvasGroup grupo = card.gameObject.AddComponent<CanvasGroup>();

        VerticalLayoutGroup vlg = card.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(18, 14, 12, 12);
        vlg.spacing = 7f;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        ContentSizeFitter fit = card.gameObject.AddComponent<ContentSizeFitter>();
        fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Faixa lateral colorida pelo humor do post (fora do layout).
        RectTransform faixa = NovoRect("Faixa", card);
        faixa.anchorMin = new Vector2(0f, 0f);
        faixa.anchorMax = new Vector2(0f, 1f);
        faixa.pivot = new Vector2(0f, 0.5f);
        faixa.offsetMin = new Vector2(0f, 7f);
        faixa.offsetMax = new Vector2(5f, -7f);
        Image faixaImg = Img(faixa, ui, CorAccent, 2.5f);
        faixa.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;

        // Linha do autor.
        RectTransform cab = NovoRect("Cabecalho", card);
        LayoutElement cabLe = cab.gameObject.AddComponent<LayoutElement>();
        cabLe.minHeight = 42f;
        cabLe.preferredHeight = 42f;
        HorizontalLayoutGroup hlg = cab.gameObject.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 10f;
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = true;

        RectTransform avatarRt = NovoRect("Avatar", cab);
        Image avatar = Img(avatarRt, knob, new Color(0.30f, 0.55f, 0.90f), 1f);
        LayoutElement avLe = avatarRt.gameObject.AddComponent<LayoutElement>();
        avLe.preferredWidth = 40f;
        avLe.preferredHeight = 40f;
        avLe.flexibleWidth = 0f;

        TextMeshProUGUI inicial = Txt("Inicial", avatarRt, "A", 20, TextAlignmentOptions.Center, Color.white);
        inicial.fontStyle = FontStyles.Bold;
        Esticar(inicial.rectTransform, 0, 0, 0, 0);

        RectTransform coluna = NovoRect("Coluna", cab);
        LayoutElement colLe = coluna.gameObject.AddComponent<LayoutElement>();
        colLe.flexibleWidth = 1f;
        VerticalLayoutGroup colVlg = coluna.gameObject.AddComponent<VerticalLayoutGroup>();
        colVlg.spacing = 0f;
        colVlg.childAlignment = TextAnchor.MiddleLeft;
        colVlg.childControlWidth = true;
        colVlg.childControlHeight = true;
        colVlg.childForceExpandWidth = true;
        colVlg.childForceExpandHeight = false;

        TextMeshProUGUI nome = Txt("Nome", coluna, "Ana Paula", 20, TextAlignmentOptions.Left, CorTexto);
        nome.fontStyle = FontStyles.Bold;
        nome.gameObject.AddComponent<LayoutElement>().preferredHeight = 23f;

        TextMeshProUGUI usuario = Txt("Usuario", coluna, "@anapaula.rs  ·  agora", 15, TextAlignmentOptions.Left, CorTextoFraco);
        usuario.gameObject.AddComponent<LayoutElement>().preferredHeight = 19f;

        // Texto da publicação.
        TextMeshProUGUI texto = Txt("Texto", card, "Texto da publicação.", 18, TextAlignmentOptions.TopLeft, CorTexto);
        texto.textWrappingMode = TextWrappingModes.Normal;
        texto.lineSpacing = 4f;

        // Rodapé: curtidas.
        RectTransform acoes = NovoRect("Acoes", card);
        LayoutElement acLe = acoes.gameObject.AddComponent<LayoutElement>();
        acLe.minHeight = 22f;
        acLe.preferredHeight = 22f;
        HorizontalLayoutGroup acHlg = acoes.gameObject.AddComponent<HorizontalLayoutGroup>();
        acHlg.spacing = 7f;
        acHlg.childAlignment = TextAnchor.MiddleLeft;
        acHlg.childControlWidth = true;
        acHlg.childControlHeight = true;
        acHlg.childForceExpandWidth = false;
        acHlg.childForceExpandHeight = false;

        RectTransform coracaoRt = NovoRect("Coracao", acoes);
        Image coracao = Img(coracaoRt, knob, new Color(0.94f, 0.36f, 0.45f, 0.9f), 1f);
        LayoutElement coLe = coracaoRt.gameObject.AddComponent<LayoutElement>();
        coLe.preferredWidth = 14f;
        coLe.preferredHeight = 14f;

        TextMeshProUGUI curtidas = Txt("Curtidas", acoes, "42", 15, TextAlignmentOptions.Left, CorTextoFraco);
        LayoutElement cuLe = curtidas.gameObject.AddComponent<LayoutElement>();
        cuLe.preferredWidth = 80f;
        cuLe.preferredHeight = 20f;

        PostView pv = card.gameObject.AddComponent<PostView>();
        pv.rect = card;
        pv.grupo = grupo;
        pv.faixa = faixaImg;
        pv.avatar = avatar;
        pv.inicial = inicial;
        pv.nome = nome;
        pv.usuario = usuario;
        pv.texto = texto;
        pv.curtidas = curtidas;
        pv.coracao = coracao;

        card.gameObject.SetActive(false);
        return pv;
    }

    /* ================= Helpers de UI ================= */

    private static RectTransform NovoRect(string nome, RectTransform pai)
    {
        GameObject go = new GameObject(nome, typeof(RectTransform));
        go.layer = LayerMask.NameToLayer("UI");
        RectTransform rt = (RectTransform)go.transform;
        rt.SetParent(pai, false);
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(100f, 100f);
        return rt;
    }

    private static void Ancorar(RectTransform rt, Vector2 ancora, Vector2 pos, Vector2 tamanho)
    {
        rt.anchorMin = ancora;
        rt.anchorMax = ancora;
        rt.anchoredPosition = pos;
        rt.sizeDelta = tamanho;
    }

    private static void Esticar(RectTransform rt, float esq, float dir, float topo, float baixo)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = new Vector2(esq, baixo);
        rt.offsetMax = new Vector2(-dir, -topo);
    }

    private static Image Img(RectTransform rt, Sprite sprite, Color cor, float ppu)
    {
        Image img = rt.gameObject.AddComponent<Image>();
        img.sprite = sprite;
        img.color = cor;
        img.raycastTarget = false;
        if (sprite != null)
        {
            img.type = Image.Type.Sliced;
            img.pixelsPerUnitMultiplier = ppu;
        }
        return img;
    }

    private static TextMeshProUGUI Txt(string nome, RectTransform pai, string texto, float tamanho,
        TextAlignmentOptions alinhamento, Color cor)
    {
        GameObject go = new GameObject(nome, typeof(RectTransform));
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
