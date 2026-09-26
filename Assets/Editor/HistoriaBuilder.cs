using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/* ============================================================
 *  HistoriaBuilder.cs  (Editor)
 *  ---------------------------------------------------------------------
 *  Monta na cena a camada de HISTÓRIA do jogo (o celular conta a
 *  história — ver HistoriaRedeSocial.cs):
 *
 *    - CartelasCanvas (novo, sortingOrder 30 — acima de TUDO, inclusive
 *      das telas de fim de jogo, que são 20): tela preta + texto das
 *      cartelas de abertura/epílogo, com o CartelasNarrativas.
 *    - Notificacao, DENTRO do Canvas CelularHUD já existente: banner push
 *      acima do ícone do celular, com o NotificacaoCelular.
 *    - "Historia" (objeto solto na raiz): HistoriaRedeSocial (o narrador)
 *      + HistoriaDoJogo (abertura/epílogo), com as referências ligadas.
 *    - Troca o texto do briefing antigo ("A equipe médica avisa...") por
 *      um que aponta pro celular — só se ainda estiver com o texto antigo.
 *
 *  Menu:  Dengue > Configurar Historia
 *
 *  Pré-requisitos: Dengue > Criar Sala do Hospital (FluxoDeJogo),
 *  Dengue > Criar Celular (CelularHUD) e Dengue > Criar HUD de Fim de Jogo.
 *
 *  Idempotente: acha pelo nome o que já existe, cria só o que falta e só
 *  liga referências vazias. Se o CelularHUD for recriado (Criar Celular
 *  apaga e refaz), é só rodar este menu de novo para o banner voltar.
 *  Acabamento sóbrio igual ao FluxoDeJogoHUDBuilder: fundo quase opaco,
 *  Shadow/Outline no PRÓPRIO Image (herdam o alpha do CanvasGroup).
 * ============================================================ */

public static class HistoriaBuilder
{
    private const string NomeCartelas = "CartelasCanvas";
    private const string NomeHistoria = "Historia";
    private const string NomeNotificacao = "Notificacao";

    private const string BriefingAntigo =
        "A equipe médica avisa: erradique os criadouros no bairro. Sem isso, o tratamento não adianta.";

    private static readonly Color CorPreto = new Color(0.012f, 0.016f, 0.02f, 1f);
    private static readonly Color CorFundo = new Color(0.07f, 0.09f, 0.11f, 0.96f);
    private static readonly Color CorContorno = new Color(1f, 1f, 1f, 0.10f);
    private static readonly Color CorSombra = new Color(0f, 0f, 0f, 0.35f);
    private static readonly Color CorTexto = new Color(0.95f, 0.96f, 0.97f, 1f);
    private static readonly Color CorTextoFraco = new Color(1f, 1f, 1f, 0.55f);

    private static Sprite uiSprite;
    private static Sprite knobSprite;

    [MenuItem("Dengue/Configurar Historia")]
    public static void Configurar()
    {
        FluxoDeJogo fluxo = Object.FindFirstObjectByType<FluxoDeJogo>();
        GameObject celularHud = GameObject.Find("CelularHUD");
        RedeSocial rede = celularHud != null ? celularHud.GetComponent<RedeSocial>() : null;

        if (fluxo == null || celularHud == null || rede == null)
        {
            EditorUtility.DisplayDialog("Configurar História",
                "Faltam peças na cena:\n" +
                (fluxo == null ? "\n  • FluxoDeJogo  →  Dengue > Criar Sala do Hospital" : "") +
                (celularHud == null || rede == null ? "\n  • CelularHUD com RedeSocial  →  Dengue > Criar Celular" : ""),
                "OK");
            return;
        }

        uiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        knobSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

        StringBuilder criados = new StringBuilder();

        CartelasNarrativas cartelas = GarantirCartelas(criados);
        NotificacaoCelular notificacao = GarantirNotificacao(celularHud, criados);

        /* ---------- Objeto "Historia" com o narrador e a abertura/epílogo ---------- */
        GameObject historiaGo = GameObject.Find(NomeHistoria);
        if (historiaGo == null)
        {
            historiaGo = new GameObject(NomeHistoria);
            Undo.RegisterCreatedObjectUndo(historiaGo, "Configurar Historia");
            criados.Append("\n  • ").Append(NomeHistoria);
        }

        HistoriaRedeSocial narrador = historiaGo.GetComponent<HistoriaRedeSocial>();
        if (narrador == null)
        {
            narrador = Undo.AddComponent<HistoriaRedeSocial>(historiaGo);
            criados.Append("\n  • Componente HistoriaRedeSocial");
        }

        HistoriaDoJogo historia = historiaGo.GetComponent<HistoriaDoJogo>();
        if (historia == null)
        {
            historia = Undo.AddComponent<HistoriaDoJogo>(historiaGo);
            criados.Append("\n  • Componente HistoriaDoJogo");
        }

        // Tipos de foco novos (ex.: Ralo, Bromélia) ganham o personagem padrão
        // sem mexer nos que já foram editados no Inspector.
        Undo.RecordObject(narrador, "Configurar Historia");
        int novosPersonagens = narrador.CompletarPersonagens();
        if (novosPersonagens > 0)
        {
            EditorUtility.SetDirty(narrador);
            criados.Append("\n  • ").Append(novosPersonagens).Append(" personagem(ns) da rede social para tipos de foco novos");
        }

        /* ---------- Referências ---------- */
        CelularController celular = celularHud.GetComponent<CelularController>();
        FeedRedeSocial feed = celularHud.GetComponentInChildren<FeedRedeSocial>(true);
        ControladorJogador jogador = Object.FindFirstObjectByType<ControladorJogador>();
        StringBuilder ligados = new StringBuilder();

        SerializedObject soNarrador = new SerializedObject(narrador);
        Ligar(soNarrador, "rede", rede, ligados);
        Ligar(soNarrador, "fluxo", fluxo, ligados);
        Ligar(soNarrador, "celular", celular, ligados);
        Ligar(soNarrador, "jogador", jogador, ligados);
        soNarrador.ApplyModifiedProperties();

        SerializedObject soHistoria = new SerializedObject(historia);
        Ligar(soHistoria, "cartelas", cartelas, ligados);
        Ligar(soHistoria, "narrador", narrador, ligados);
        Ligar(soHistoria, "fluxo", fluxo, ligados);
        Ligar(soHistoria, "fluxoHUD", Object.FindFirstObjectByType<FluxoDeJogoHUD>(), ligados);
        Ligar(soHistoria, "interactor", Object.FindFirstObjectByType<Interactor>(), ligados);
        Ligar(soHistoria, "celular", celular, ligados);
        Ligar(soHistoria, "jogador", jogador, ligados);
        soHistoria.ApplyModifiedProperties();

        SerializedObject soNotificacao = new SerializedObject(notificacao);
        Ligar(soNotificacao, "rede", rede, ligados);
        Ligar(soNotificacao, "feed", feed, ligados);
        Ligar(soNotificacao, "celular", celular, ligados);
        soNotificacao.ApplyModifiedProperties();

        /* ---------- Briefing aponta pro celular ---------- */
        AtualizarBriefing(criados);

        EditorSceneManager.MarkSceneDirty(historiaGo.scene);
        Selection.activeGameObject = historiaGo;

        StringBuilder msg = new StringBuilder("[História] Concluído.");
        msg.Append(criados.Length > 0 ? "\nCriei/alterei:" + criados : "\nNada faltava na hierarquia.");
        msg.Append(ligados.Length > 0 ? "\nLiguei:" + ligados : "\nNenhuma referência nova para ligar.");
        if (jogador == null)
            msg.Append("\nAviso: não achei ControladorJogador na cena — sem ele, a abertura não trava o olhar e os posts de pulo não saem.");
        Debug.Log(msg.ToString(), historiaGo);
    }

    /* ================= Cartelas ================= */

    private static CartelasNarrativas GarantirCartelas(StringBuilder criados)
    {
        GameObject root = GameObject.Find(NomeCartelas);
        if (root == null)
        {
            root = new GameObject(NomeCartelas, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(CanvasGroup));
            Undo.RegisterCreatedObjectUndo(root, "Configurar Historia");
            root.layer = LayerMask.NameToLayer("UI");

            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 30;   // acima do FluxoDeJogoCanvas (20)

            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            // Invisível no modo Edição; o HistoriaDoJogo escurece no Awake.
            CanvasGroup grupo = root.GetComponent<CanvasGroup>();
            grupo.alpha = 0f;
            grupo.blocksRaycasts = false;
            grupo.interactable = false;

            criados.Append("\n  • ").Append(NomeCartelas);
        }
        RectTransform rootRt = (RectTransform)root.transform;

        if (Achar<RectTransform>(root, "Fundo") == null)
        {
            RectTransform fundo = NovoRect("Fundo", rootRt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            Esticar(fundo);
            NovaImagem(fundo, null, CorPreto);
        }

        if (Achar<TextMeshProUGUI>(root, "CartelaTexto") == null)
        {
            TextMeshProUGUI texto = NovoTexto("CartelaTexto", rootRt, "", 46, TextAlignmentOptions.Center, CorTexto);
            texto.textWrappingMode = TextWrappingModes.Normal;
            texto.lineSpacing = 12f;
            SetRect(texto.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 20f), new Vector2(1280f, 460f));
            CanvasGroup g = texto.gameObject.AddComponent<CanvasGroup>();
            g.alpha = 0f;
            g.blocksRaycasts = false;
            g.interactable = false;
        }

        if (Achar<TextMeshProUGUI>(root, "CartelaDica") == null)
        {
            TextMeshProUGUI dica = NovoTexto("CartelaDica", rootRt, "clique para continuar   ·   Esc pula", 18,
                TextAlignmentOptions.Center, CorTextoFraco);
            dica.characterSpacing = 2f;
            SetRect(dica.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 54f), new Vector2(900f, 30f));
            CanvasGroup g = dica.gameObject.AddComponent<CanvasGroup>();
            g.alpha = 0f;
            g.blocksRaycasts = false;
            g.interactable = false;
        }

        CartelasNarrativas cartelas = root.GetComponent<CartelasNarrativas>();
        if (cartelas == null)
        {
            cartelas = Undo.AddComponent<CartelasNarrativas>(root);
            criados.Append("\n  • Componente CartelasNarrativas");
        }

        StringBuilder ligados = new StringBuilder();
        SerializedObject so = new SerializedObject(cartelas);
        Ligar(so, "grupo", root.GetComponent<CanvasGroup>(), ligados);
        Ligar(so, "texto", Achar<TextMeshProUGUI>(root, "CartelaTexto"), ligados);
        Ligar(so, "textoGrupo", Achar<CanvasGroup>(root, "CartelaTexto"), ligados);
        Ligar(so, "dicaGrupo", Achar<CanvasGroup>(root, "CartelaDica"), ligados);
        so.ApplyModifiedProperties();

        return cartelas;
    }

    /* ================= Notificação push (dentro do CelularHUD) ================= */

    private static NotificacaoCelular GarantirNotificacao(GameObject celularHud, StringBuilder criados)
    {
        RectTransform hudRt = (RectTransform)celularHud.transform;
        RectTransform painel = Achar<RectTransform>(celularHud, NomeNotificacao);

        if (painel == null)
        {
            // Canto inferior direito, logo ACIMA do ícone do celular
            // (ícone: 54×94 em x -40 / y 40 — ver CelularBuilder).
            painel = NovoRect(NomeNotificacao, hudRt, new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(-40f, 150f), new Vector2(440f, 104f));
            painel.SetAsFirstSibling();   // o painel do celular e o ícone desenham por cima

            Image fundo = NovaImagem(painel, uiSprite, CorFundo);
            fundo.type = Image.Type.Sliced;
            fundo.pixelsPerUnitMultiplier = 0.6f;

            Shadow sombra = painel.gameObject.AddComponent<Shadow>();
            sombra.effectColor = CorSombra;
            sombra.effectDistance = new Vector2(2f, -4f);

            Outline contorno = painel.gameObject.AddComponent<Outline>();
            contorno.effectColor = CorContorno;
            contorno.effectDistance = new Vector2(1f, -1f);
            contorno.useGraphicAlpha = false;

            CanvasGroup grupo = painel.gameObject.AddComponent<CanvasGroup>();
            grupo.alpha = 0f;
            grupo.blocksRaycasts = false;
            grupo.interactable = false;

            RectTransform avatar = NovoRect("Avatar", painel, new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(16f, -16f), new Vector2(44f, 44f));
            NovaImagem(avatar, knobSprite, new Color(0.30f, 0.55f, 0.90f));

            TextMeshProUGUI inicial = NovoTexto("Inicial", avatar, "?", 22, TextAlignmentOptions.Center, Color.white);
            inicial.fontStyle = FontStyles.Bold;
            SetRect(inicial.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            TextMeshProUGUI cabecalho = NovoTexto("Cabecalho", painel, "", 17, TextAlignmentOptions.Left, CorTexto);
            cabecalho.overflowMode = TextOverflowModes.Ellipsis;
            SetRect(cabecalho.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f),
                new Vector2(74f, -14f), new Vector2(-90f, 24f));

            TextMeshProUGUI texto = NovoTexto("Texto", painel, "", 16, TextAlignmentOptions.TopLeft,
                new Color(CorTexto.r, CorTexto.g, CorTexto.b, 0.85f));
            texto.textWrappingMode = TextWrappingModes.Normal;
            texto.overflowMode = TextOverflowModes.Ellipsis;
            RectTransform textoRt = texto.rectTransform;
            textoRt.anchorMin = Vector2.zero;
            textoRt.anchorMax = Vector2.one;
            textoRt.pivot = new Vector2(0.5f, 0.5f);
            textoRt.offsetMin = new Vector2(74f, 12f);
            textoRt.offsetMax = new Vector2(-18f, -42f);

            criados.Append("\n  • CelularHUD/").Append(NomeNotificacao);
        }

        NotificacaoCelular notificacao = painel.GetComponent<NotificacaoCelular>();
        if (notificacao == null)
        {
            notificacao = Undo.AddComponent<NotificacaoCelular>(painel.gameObject);
            criados.Append("\n  • Componente NotificacaoCelular");
        }

        StringBuilder ligados = new StringBuilder();
        SerializedObject so = new SerializedObject(notificacao);
        Ligar(so, "painel", painel, ligados);
        Ligar(so, "grupo", painel.GetComponent<CanvasGroup>(), ligados);
        Ligar(so, "avatar", Achar<Image>(painel.gameObject, "Avatar"), ligados);
        Ligar(so, "inicial", Achar<TextMeshProUGUI>(painel.gameObject, "Inicial"), ligados);
        Ligar(so, "cabecalho", Achar<TextMeshProUGUI>(painel.gameObject, "Cabecalho"), ligados);
        Ligar(so, "texto", Achar<TextMeshProUGUI>(painel.gameObject, "Texto"), ligados);
        so.ApplyModifiedProperties();

        return notificacao;
    }

    /* ================= Briefing ================= */

    private static void AtualizarBriefing(StringBuilder criados)
    {
        GameObject fim = GameObject.Find("FluxoDeJogoCanvas");
        if (fim == null)
            return;

        TextMeshProUGUI briefing = Achar<TextMeshProUGUI>(fim, "BriefingTexto");
        if (briefing == null || briefing.text != BriefingAntigo)
            return;

        Undo.RecordObject(briefing, "Configurar Historia");
        briefing.text = FluxoDeJogoHUDBuilder.BriefingPadrao;
        EditorUtility.SetDirty(briefing);
        criados.Append("\n  • Texto do BriefingTexto (agora aponta pro celular)");
    }

    /* ================= Helpers (mesmo padrão dos outros builders) ================= */

    private static void Ligar(SerializedObject so, string campo, Object valor, StringBuilder ligados)
    {
        SerializedProperty prop = so.FindProperty(campo);
        if (prop == null || valor == null)
            return;
        if (prop.objectReferenceValue != null)
            return;

        prop.objectReferenceValue = valor;
        ligados.Append("\n  • ").Append(so.targetObject.GetType().Name).Append('.').Append(campo);
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
        Undo.RegisterCreatedObjectUndo(go, "Configurar Historia");
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

    private static void Esticar(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static Image NovaImagem(RectTransform rt, Sprite sprite, Color cor)
    {
        Image img = rt.gameObject.AddComponent<Image>();
        img.sprite = sprite;
        img.color = cor;
        img.raycastTarget = false;
        return img;
    }

    private static TextMeshProUGUI NovoTexto(string nome, RectTransform pai, string texto, float tamanho,
        TextAlignmentOptions alinhamento, Color cor)
    {
        GameObject go = new GameObject(nome, typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(go, "Configurar Historia");
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
