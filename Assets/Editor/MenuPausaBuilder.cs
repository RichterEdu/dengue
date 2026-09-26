using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

/* ============================================================
 *  MenuPausaBuilder.cs  (Editor)
 *  ---------------------------------------------------------------------
 *  Monta o MENU DE PAUSA (Esc) na cena:
 *
 *    - MenuPausaCanvas (Screen Space Overlay, sortingOrder 40 — acima de
 *      tudo, inclusive das cartelas, que são 30) com o MenuPausa.cs:
 *        Veu            escurece o jogo atrás
 *        TelaMenu       Continuar / Configurações / Reiniciar do zero /
 *                       Menu inicial / Sair
 *        TelaConfig     Áudio, Controles e Vídeo (PainelConfiguracoes.cs)
 *        TelaConfirmar  "Reiniciar do zero?" / "Sair do jogo?"
 *    - EventSystem com InputSystemUIInputModule, se a cena não tiver
 *      (o projeto usa só o Input System novo).
 *    - CanalDeAudio (Efeitos) na AudioSource do CelularHUD.
 *
 *  Menu:  Dengue > Criar Menu de Pausa
 *
 *  RECONSTRÓI do zero a cada execução (apaga o MenuPausaCanvas antigo):
 *  o menu não guarda nada editado à mão — valores ficam em PlayerPrefs.
 *  Tema branco: cartão branco opaco, texto e botão principal grafite,
 *  Shadow e Outline no PRÓPRIO Image; vermelho só no que é perigoso
 *  (Sair / confirmar reinício).
 *
 *  As peças (cartão, botões, linhas, chaves...) e a tela de Configurações
 *  são internal: o MenuInicialBuilder.cs monta o menu inicial com elas.
 *
 *  ESPAÇAMENTO: o texto dos cartões fica a PaddingConteudo da borda; os
 *  botões e as linhas de configuração, a PaddingBotoes (um pouco menor —
 *  os botões ficam mais largos que a coluna de texto, com o texto
 *  deles alinhado ao título).
 * ============================================================ */

public static class MenuPausaBuilder
{
    private const string NomeCanvas = "MenuPausaCanvas";

    internal const float PaddingConteudo = 44f;
    internal const float PaddingBotoes = 28f;
    internal const float AlturaBotao = 60f;
    internal const float AlturaLinha = 50f;

    // Tema branco: cartão branco opaco, texto grafite, primária grafite; vermelho só no que é perigoso.
    internal static readonly Color CorVeu = new Color(0.02f, 0.03f, 0.04f, 0.45f);
    internal static readonly Color CorVeuConfirmar = new Color(0.02f, 0.03f, 0.04f, 0.40f);
    internal static readonly Color CorFundo = Color.white;
    internal static readonly Color CorContorno = new Color(0f, 0f, 0f, 0.07f);
    internal static readonly Color CorSombra = new Color(0f, 0f, 0f, 0.18f);
    internal static readonly Color CorLinha = new Color(0f, 0f, 0f, 0.10f);
    internal static readonly Color CorTexto = new Color(0.11f, 0.13f, 0.15f, 1f);
    internal static readonly Color CorTextoFraco = new Color(0.38f, 0.41f, 0.45f, 1f);
    internal static readonly Color CorTextoDica = new Color(0.55f, 0.58f, 0.62f, 1f);
    internal static readonly Color CorRotulo = new Color(0.20f, 0.23f, 0.27f, 1f);
    internal static readonly Color TextoClaro = Color.white;
    internal static readonly Color Primaria = new Color(0.14f, 0.16f, 0.19f, 1f);
    internal static readonly Color PrimariaHover = new Color(0.24f, 0.27f, 0.31f, 1f);
    internal static readonly Color Perigo = new Color(0.82f, 0.22f, 0.22f, 1f);
    internal static readonly Color PerigoHover = new Color(0.90f, 0.30f, 0.30f, 1f);
    internal static readonly Color PerigoFundo = new Color(0.82f, 0.22f, 0.22f, 0.08f);
    internal static readonly Color PerigoFundoHover = new Color(0.82f, 0.22f, 0.22f, 0.14f);
    internal static readonly Color Neutro = new Color(0.93f, 0.94f, 0.95f, 1f);
    internal static readonly Color NeutroDestaque = new Color(0.86f, 0.88f, 0.90f, 1f);
    internal static readonly Color LinhaNormal = new Color(0f, 0f, 0f, 0f);
    internal static readonly Color LinhaDestaque = new Color(0f, 0f, 0f, 0.05f);
    internal static readonly Color CorTrilho = new Color(0f, 0f, 0f, 0.12f);
    internal static readonly Color CorChaveDesligada = new Color(0f, 0f, 0f, 0.15f);
    internal static readonly Color CorCaixa = new Color(0f, 0f, 0f, 0.05f);
    internal static readonly Color Transparente = new Color(1f, 1f, 1f, 0f);

    internal static Sprite uiSprite;
    internal static Sprite knobSprite;
    private static int camadaUI;

    /* Sprites embutidos da UI e a layer "UI" — chamar antes de usar as peças. */
    internal static void PrepararRecursos()
    {
        uiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        knobSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        camadaUI = LayerMask.NameToLayer("UI");
    }

    [MenuItem("Dengue/Criar Menu de Pausa")]
    public static void Criar()
    {
        PrepararRecursos();

        StringBuilder log = new StringBuilder();
        GarantirEventSystem(log);

        GameObject antigo = GameObject.Find(NomeCanvas);
        Transform paiAntigo = null;
        int indiceAntigo = 0;
        if (antigo != null)
        {
            paiAntigo = antigo.transform.parent;
            indiceAntigo = antigo.transform.GetSiblingIndex();
            Undo.DestroyObjectImmediate(antigo);
            log.Append("\n  • MenuPausaCanvas antigo apagado (reconstruído do zero)");
        }

        /* ---------- Canvas ---------- */
        GameObject root = new GameObject(NomeCanvas, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler),
            typeof(GraphicRaycaster), typeof(CanvasGroup));
        HierarquiaOrganizador.RecolocarNoLugar(root, paiAntigo, indiceAntigo);
        root.layer = camadaUI;

        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 40;

        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        CanvasGroup grupo = root.GetComponent<CanvasGroup>();
        grupo.alpha = 0f;
        grupo.blocksRaycasts = false;
        grupo.interactable = false;

        MenuPausa menu = root.AddComponent<MenuPausa>();
        SerializedObject so = new SerializedObject(menu);
        Ref(so, "grupo", grupo);

        Img(Tela("Veu", root.transform), CorVeu, null, 1f, true);

        RectTransform cartaoMenu = MontarMenu(root.transform, so);
        RectTransform cartaoConfig = MontarConfiguracoes(root.transform, "JOGO PAUSADO", out RectTransform telaConfig, out PainelConfiguracoes painel);
        Ref(so, "telaConfiguracoes", telaConfig);
        Ref(so, "configuracoes", painel);
        RectTransform cartaoConfirmar = MontarConfirmacao(root.transform, so);

        SerializedProperty cartoes = so.FindProperty("cartoes");
        cartoes.arraySize = 3;
        cartoes.GetArrayElementAtIndex(0).objectReferenceValue = cartaoMenu;
        cartoes.GetArrayElementAtIndex(1).objectReferenceValue = cartaoConfig;
        cartoes.GetArrayElementAtIndex(2).objectReferenceValue = cartaoConfirmar;

        Ref(so, "fluxo", Object.FindAnyObjectByType<FluxoDeJogo>());
        Ref(so, "jogador", Object.FindAnyObjectByType<ControladorJogador>());
        Ref(so, "interactor", Object.FindAnyObjectByType<Interactor>());
        Ref(so, "celular", Object.FindAnyObjectByType<CelularController>());
        Ref(so, "cartelas", Object.FindAnyObjectByType<CartelasNarrativas>());
        so.FindProperty("corOcupacaoBaixa").colorValue = Primaria;
        so.FindProperty("corOcupacaoAlta").colorValue = Perigo;
        so.FindProperty("corFocosEliminados").colorValue = CorTexto;
        so.ApplyModifiedPropertiesWithoutUndo();

        Undo.RegisterCreatedObjectUndo(root, "Criar Menu de Pausa");
        log.Append("\n  • MenuPausaCanvas (menu, configurações e confirmação)");

        GarantirCanalNoCelular(log);

        EditorSceneManager.MarkSceneDirty(root.scene);
        Selection.activeGameObject = root;
        Debug.Log("[Menu de Pausa] Concluído." + log, root);
    }

    /* ================= Tela: menu ================= */

    private static RectTransform MontarMenu(Transform pai, SerializedObject so)
    {
        RectTransform tela = Tela("TelaMenu", pai);
        Ref(so, "telaMenu", tela);

        const float largura = 520f;
        float larguraBotao = largura - PaddingBotoes * 2f;
        RectTransform cartao = Cartao("Cartao", tela, new Vector2(largura, 712f));

        Cruz(cartao, new Vector2(PaddingConteudo, -44f), 30f);
        Ref(so, "rotuloMenu", Txt(Pos(Novo("Rotulo", cartao), TopoEsq, TopoEsq, new Vector2(PaddingConteudo + 44f, -44f), new Vector2(400f, 30f)),
            "VILA NOVA  ·  MISSÃO EM ANDAMENTO", 14, CorRotulo, TextAlignmentOptions.Left, true, 3f));
        Txt(Pos(Novo("Titulo", cartao), TopoEsq, TopoEsq, new Vector2(PaddingConteudo - 2f, -88f), new Vector2(440f, 56f)),
            "Jogo pausado", 44, CorTexto, TextAlignmentOptions.Left, true);
        Ref(so, "statusMenu", Txt(Pos(Novo("Status", cartao), TopoEsq, TopoEsq, new Vector2(PaddingConteudo, -148f), new Vector2(440f, 28f)),
            "0/0 focos eliminados    ·    Hospital 0%", 18, CorTextoFraco, TextAlignmentOptions.Left));

        Divisoria(cartao, -196f, larguraBotao);
        Button continuar = Botao(cartao, -224f, larguraBotao, "Continuar", Primaria, PrimariaHover, TextoClaro, null, "ESC");
        Button config = Botao(cartao, -296f, larguraBotao, "Configurações", Neutro, NeutroDestaque, CorTexto, Primaria);
        Button reiniciar = Botao(cartao, -368f, larguraBotao, "Reiniciar do zero", Neutro, NeutroDestaque, CorTexto, Primaria);
        Button menuInicial = Botao(cartao, -440f, larguraBotao, "Menu inicial", Neutro, NeutroDestaque, CorTexto, Primaria);
        Divisoria(cartao, -524f, larguraBotao);
        Button sair = Botao(cartao, -552f, larguraBotao, "Sair do jogo", PerigoFundo, PerigoFundoHover, Perigo, Perigo);

        Txt(Pos(Novo("Rodape", cartao), BaseCentro, BaseCentro, new Vector2(0f, 30f), new Vector2(larguraBotao, 28f)),
            "O tempo do hospital fica parado enquanto o menu está aberto.", 15, CorTextoDica, TextAlignmentOptions.Center);

        Vertical(continuar, config, reiniciar, menuInicial, sair);
        Ref(so, "botaoContinuar", continuar);
        Ref(so, "botaoConfiguracoes", config);
        Ref(so, "botaoReiniciar", reiniciar);
        Ref(so, "botaoMenuInicial", menuInicial);
        Ref(so, "botaoSair", sair);
        return cartao;
    }

    /* ================= Tela: configurações ================= */

    /* Tela de Configurações (a mesma no pause e no menu inicial). Devolve o
     * cartão; a tela começa desativada e leva o PainelConfiguracoes. */
    internal static RectTransform MontarConfiguracoes(Transform pai, string rotulo, out RectTransform tela, out PainelConfiguracoes painel)
    {
        tela = Tela("TelaConfig", pai);
        tela.gameObject.SetActive(false);
        painel = tela.gameObject.AddComponent<PainelConfiguracoes>();
        SerializedObject so = new SerializedObject(painel);

        const float largura = 820f;
        float larguraLinha = largura - PaddingBotoes * 2f;
        RectTransform cartao = Cartao("Cartao", tela, new Vector2(largura, 880f));

        Cruz(cartao, new Vector2(PaddingConteudo, -40f), 30f);
        Txt(Pos(Novo("Rotulo", cartao), TopoEsq, TopoEsq, new Vector2(PaddingConteudo + 44f, -40f), new Vector2(500f, 30f)),
            rotulo, 14, CorRotulo, TextAlignmentOptions.Left, true, 3f);
        Txt(Pos(Novo("Titulo", cartao), TopoEsq, TopoEsq, new Vector2(PaddingConteudo - 2f, -80f), new Vector2(500f, 52f)),
            "Configurações", 40, CorTexto, TextAlignmentOptions.Left, true);
        Divisoria(cartao, -146f, larguraLinha);

        float y = -168f;
        Secao(cartao, ref y, "ÁUDIO");
        Slider volume = LinhaSlider(cartao, ref y, larguraLinha, "Volume geral", so, "valorVolumeGeral");
        Slider musica = LinhaSlider(cartao, ref y, larguraLinha, "Música", so, "valorMusica");
        Slider efeitos = LinhaSlider(cartao, ref y, larguraLinha, "Efeitos e zumbido", so, "valorEfeitos");
        y -= 6f;
        Secao(cartao, ref y, "CONTROLES");
        Slider sensibilidade = LinhaSlider(cartao, ref y, larguraLinha, "Sensibilidade", so, "valorSensibilidade");
        Toggle inverterY = LinhaChave(cartao, ref y, larguraLinha, "Inverter eixo Y");
        y -= 6f;
        Secao(cartao, ref y, "VÍDEO");
        Slider fov = LinhaSlider(cartao, ref y, larguraLinha, "Campo de visão (FOV)", so, "valorFov");
        Toggle balanco = LinhaChave(cartao, ref y, larguraLinha, "Balanço da câmera");
        // Sempre criada: com um nível só no build, o MenuPausa esconde a linha em runtime.
        Slider qualidade = LinhaSeletor(cartao, ref y, larguraLinha, "Qualidade gráfica", so);
        Toggle telaCheia = LinhaChave(cartao, ref y, larguraLinha, "Tela cheia");

        // O cartão cresce com as linhas: rodapé (32 + 60) + folga.
        cartao.sizeDelta = new Vector2(largura, -y + 26f + 60f + 32f);

        /* ---------- Rodapé ---------- */
        RectTransform rodape = Pos(Novo("Rodape", cartao), BaseCentro, BaseCentro, new Vector2(0f, 32f), new Vector2(larguraLinha, 60f));
        Button restaurar = BotaoCentral(rodape, new Vector2(0f, 0.5f), Vector2.zero, new Vector2(240f, 56f),
            "Restaurar padrão", 19, Neutro, NeutroDestaque, CorTexto, Primaria);
        Button voltar = BotaoCentral(rodape, new Vector2(1f, 0.5f), Vector2.zero, new Vector2(220f, 56f),
            "Voltar", 21, Primaria, PrimariaHover, TextoClaro, null);
        Tecla(rodape, new Vector2(1f, 0.5f), new Vector2(-240f, 0f), "ESC", CorTextoFraco, CorCaixa);
        Txt(Pos(Novo("Dica", rodape), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-298f, 0f), new Vector2(160f, 30f)),
            "salva e volta", 15, CorTextoDica, TextAlignmentOptions.Right);

        Vertical(volume, musica, efeitos, sensibilidade, inverterY, fov, balanco, qualidade, telaCheia, voltar);
        Navegar(restaurar, telaCheia, null, null, voltar);
        Navegar(voltar, telaCheia, null, restaurar, null);

        Ref(so, "sliderVolumeGeral", volume);
        Ref(so, "sliderMusica", musica);
        Ref(so, "sliderEfeitos", efeitos);
        Ref(so, "sliderSensibilidade", sensibilidade);
        Ref(so, "chaveInverterY", inverterY);
        Ref(so, "sliderFov", fov);
        Ref(so, "chaveBalanco", balanco);
        Ref(so, "seletorQualidade", qualidade);
        Ref(so, "chaveTelaCheia", telaCheia);
        Ref(so, "botaoRestaurar", restaurar);
        Ref(so, "botaoVoltar", voltar);
        so.ApplyModifiedPropertiesWithoutUndo();
        return cartao;
    }

    internal static void Secao(RectTransform cartao, ref float y, string nome)
    {
        Txt(Pos(Novo("Secao " + nome, cartao), TopoEsq, TopoEsq, new Vector2(PaddingConteudo, y), new Vector2(400f, 26f)),
            nome, 14, CorRotulo, TextAlignmentOptions.Left, true, 3f);
        y -= 32f;
    }

    /* Linha clicável: fundo (destaca quando selecionada) + barrinha + rótulo.
     * O rótulo fica alinhado com a coluna de texto do cartão. */
    internal static RectTransform Linha(RectTransform cartao, ref float y, float largura, string rotulo)
    {
        RectTransform linha = Pos(Novo("Linha " + rotulo, cartao), TopoCentro, TopoCentro, new Vector2(0f, y), new Vector2(largura, AlturaLinha));
        Img(linha, LinhaNormal, uiSprite, 0.5f, true);
        Txt(Pos(Novo("Rotulo", linha), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(PaddingConteudo - PaddingBotoes, 0f), new Vector2(320f, 40f)),
            rotulo, 20, CorTexto, TextAlignmentOptions.Left);
        y -= AlturaLinha + 4f;
        return linha;
    }

    internal static Slider LinhaSlider(RectTransform cartao, ref float y, float largura, string rotulo, SerializedObject so, string campoValor)
    {
        RectTransform linha = Linha(cartao, ref y, largura, rotulo);

        RectTransform sRt = Pos(Novo("Slider", linha), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-110f, 0f), new Vector2(300f, 28f));
        Img(sRt, Transparente, null, 1f, true);   // área de clique
        Slider slider = sRt.gameObject.AddComponent<Slider>();

        RectTransform trilho = Novo("Trilho", sRt);
        Faixa(trilho, 6f);
        Img(trilho, CorTrilho, uiSprite, 3f);

        RectTransform areaFill = Novo("AreaFill", sRt);
        Faixa(areaFill, 6f);
        RectTransform fill = Novo("Fill", areaFill);
        fill.sizeDelta = Vector2.zero;
        Img(fill, Primaria, uiSprite, 3f);

        RectTransform areaHandle = Novo("AreaHandle", sRt);
        Esticar(areaHandle);
        RectTransform handle = Novo("Handle", areaHandle);
        handle.sizeDelta = new Vector2(22f, -6f);
        Image handleImg = Img(handle, Color.white, knobSprite, 1f, true);

        slider.fillRect = fill;
        slider.handleRect = handle;
        slider.targetGraphic = handleImg;
        slider.direction = Slider.Direction.LeftToRight;
        slider.transition = Selectable.Transition.ColorTint;
        ColorBlock cores = slider.colors;
        cores.normalColor = Primaria;
        cores.highlightedColor = PrimariaHover;
        cores.selectedColor = PrimariaHover;
        cores.pressedColor = CorTexto;
        cores.disabledColor = CorChaveDesligada;
        cores.fadeDuration = 0.08f;
        slider.colors = cores;
        slider.wholeNumbers = true;
        slider.minValue = 0f;
        slider.maxValue = 20f;
        slider.value = 16f;

        TextMeshProUGUI valor = Txt(Pos(Novo("Valor", linha), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-(PaddingConteudo - PaddingBotoes), 0f), new Vector2(76f, 40f)),
            "80%", 20, CorTexto, TextAlignmentOptions.Right, true);
        Ref(so, campoValor, valor);

        Item(linha.gameObject, slider, linha.GetComponent<Image>(), LinhaNormal, LinhaDestaque, BarraFoco(linha, Primaria));
        return slider;
    }

    internal static Toggle LinhaChave(RectTransform cartao, ref float y, float largura, string rotulo)
    {
        RectTransform linha = Linha(cartao, ref y, largura, rotulo);

        // O Toggle é a própria linha: clicar em qualquer lugar dela liga/desliga.
        Toggle toggle = linha.gameObject.AddComponent<Toggle>();
        toggle.transition = Selectable.Transition.None;
        toggle.isOn = false;

        RectTransform pista = Pos(Novo("Chave", linha), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-(PaddingConteudo - PaddingBotoes), 0f), new Vector2(56f, 30f));
        Image pistaImg = Img(pista, CorChaveDesligada, uiSprite, 0.25f);
        RectTransform bola = Pos(Novo("Bola", pista), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(4f, 0f), new Vector2(22f, 22f));
        Image bolaImg = Img(bola, TextoClaro, knobSprite);
        TextMeshProUGUI estado = Txt(Pos(Novo("Estado", linha), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-(PaddingConteudo - PaddingBotoes) - 70f, 0f), new Vector2(120f, 40f)),
            "Desligado", 17, CorTextoFraco, TextAlignmentOptions.Right);

        ItemDeMenu item = Item(linha.gameObject, toggle, linha.GetComponent<Image>(), LinhaNormal, LinhaDestaque, BarraFoco(linha, Primaria));
        SerializedObject so = new SerializedObject(item);
        Ref(so, "pista", pistaImg);
        Ref(so, "bola", bola);
        Ref(so, "bolaGrafico", bolaImg);
        Ref(so, "estado", estado);
        so.FindProperty("pistaLigada").colorValue = Primaria;
        so.FindProperty("pistaDesligada").colorValue = CorChaveDesligada;
        so.FindProperty("bolaLigada").colorValue = TextoClaro;
        so.FindProperty("bolaDesligada").colorValue = TextoClaro;
        so.ApplyModifiedPropertiesWithoutUndo();
        return toggle;
    }

    /* "< Alta >": Slider SEM trilho na própria linha (só guarda o índice e
     * responde a esquerda/direita no controle) + setas clicáveis. */
    internal static Slider LinhaSeletor(RectTransform cartao, ref float y, float largura, string rotulo, SerializedObject so)
    {
        RectTransform linha = Linha(cartao, ref y, largura, rotulo);

        Slider slider = linha.gameObject.AddComponent<Slider>();
        slider.transition = Selectable.Transition.None;
        slider.direction = Slider.Direction.LeftToRight;
        slider.wholeNumbers = true;
        slider.minValue = 0f;
        slider.maxValue = 1f;

        RectTransform caixa = Pos(Novo("Seletor", linha), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-(PaddingConteudo - PaddingBotoes), 0f), new Vector2(300f, 36f));
        Img(caixa, CorCaixa, uiSprite, 0.5f);
        Ref(so, "valorQualidade", Txt(Tela("Valor", caixa), "Alta", 19, CorTexto, TextAlignmentOptions.Center, true));
        Ref(so, "qualidadeAnterior", Seta(caixa, 0f, "<"));
        Ref(so, "qualidadeProxima", Seta(caixa, 1f, ">"));

        Item(linha.gameObject, slider, linha.GetComponent<Image>(), LinhaNormal, LinhaDestaque, BarraFoco(linha, Primaria));
        return slider;
    }

    internal static Button Seta(RectTransform caixa, float lado, string texto)
    {
        RectTransform rt = Pos(Novo(lado < 0.5f ? "Anterior" : "Proxima", caixa), new Vector2(lado, 0.5f), new Vector2(lado, 0.5f),
            new Vector2(lado < 0.5f ? 4f : -4f, 0f), new Vector2(40f, 36f));
        Img(rt, Transparente, null, 1f, true);
        TextMeshProUGUI t = Txt(Tela("Texto", rt), texto, 22, Color.white, TextAlignmentOptions.Center, true);

        Button b = rt.gameObject.AddComponent<Button>();
        b.targetGraphic = t;
        b.transition = Selectable.Transition.ColorTint;
        ColorBlock cores = b.colors;
        cores.normalColor = Primaria;
        cores.highlightedColor = PrimariaHover;
        cores.selectedColor = Primaria;
        cores.pressedColor = CorTexto;
        cores.fadeDuration = 0.06f;
        b.colors = cores;
        b.navigation = new Navigation { mode = Navigation.Mode.None };   // o controle usa esquerda/direita na linha
        return b;
    }

    /* ================= Tela: confirmação ================= */

    private static RectTransform MontarConfirmacao(Transform pai, SerializedObject so)
    {
        RectTransform tela = Tela("TelaConfirmar", pai);
        tela.gameObject.SetActive(false);
        Img(tela, CorVeuConfirmar, null, 1f, true);   // escurece o menu atrás
        Ref(so, "telaConfirmacao", tela);

        const float largura = 560f;
        RectTransform cartao = Cartao("Cartao", tela, new Vector2(largura, 300f));

        RectTransform aviso = Pos(Novo("Aviso", cartao), TopoEsq, TopoEsq, new Vector2(PaddingConteudo, -40f), new Vector2(34f, 34f));
        Img(aviso, Perigo, knobSprite);
        Txt(Tela("!", aviso), "!", 24, TextoClaro, TextAlignmentOptions.Center, true);
        Ref(so, "tituloConfirmacao", Txt(Pos(Novo("Titulo", cartao), TopoEsq, TopoEsq, new Vector2(PaddingConteudo + 50f, -38f), new Vector2(420f, 40f)),
            "Reiniciar do zero?", 32, CorTexto, TextAlignmentOptions.Left, true));
        TextMeshProUGUI corpo = Txt(Pos(Novo("Corpo", cartao), TopoEsq, TopoEsq, new Vector2(PaddingConteudo + 2f, -98f), new Vector2(largura - PaddingConteudo * 2f, 90f)),
            "A partida volta para a abertura no hospital.", 19, CorTextoFraco, TextAlignmentOptions.TopLeft);
        corpo.textWrappingMode = TextWrappingModes.Normal;
        Ref(so, "corpoConfirmacao", corpo);

        float larguraBotao = (largura - PaddingBotoes * 2f - 16f) / 2f;
        Button cancelar = BotaoCentral(cartao, Vector2.zero, new Vector2(PaddingBotoes, PaddingBotoes), new Vector2(larguraBotao, 56f),
            "Cancelar", 20, Neutro, NeutroDestaque, CorTexto, Primaria);
        Button confirmar = BotaoCentral(cartao, new Vector2(1f, 0f), new Vector2(-PaddingBotoes, PaddingBotoes), new Vector2(larguraBotao, 56f),
            "Reiniciar", 20, Perigo, PerigoHover, TextoClaro, null);
        Ref(so, "textoConfirmar", confirmar.GetComponentInChildren<TextMeshProUGUI>());

        Navegar(cancelar, null, null, null, confirmar);
        Navegar(confirmar, null, null, cancelar, null);
        Ref(so, "botaoCancelar", cancelar);
        Ref(so, "botaoConfirmar", confirmar);
        return cartao;
    }

    /* ================= Peças ================= */

    /* Botão de lista (menu principal): texto à esquerda, alinhado com o título. */
    internal static Button Botao(RectTransform cartao, float y, float largura, string texto,
        Color normal, Color destaque, Color corTexto, Color? corBarra, string tecla = null)
    {
        RectTransform rt = Pos(Novo("Botao " + texto, cartao), TopoCentro, TopoCentro, new Vector2(0f, y), new Vector2(largura, AlturaBotao));
        Image img = Img(rt, normal, uiSprite, 0.5f, true);
        Contorno(img, CorContorno);

        RectTransform t = Tela("Texto", rt);
        t.offsetMin = new Vector2(PaddingConteudo - PaddingBotoes, 0f);
        Txt(t, texto, 22, corTexto, TextAlignmentOptions.Left, true, 0.5f);

        if (tecla != null)
            Tecla(rt, new Vector2(1f, 0.5f), new Vector2(-(PaddingConteudo - PaddingBotoes), 0f), tecla,
                new Color(corTexto.r, corTexto.g, corTexto.b, 0.85f), new Color(corTexto.r, corTexto.g, corTexto.b, 0.14f));

        return FinalizarBotao(rt, img, normal, destaque, corBarra);
    }

    /* Botão com texto centralizado (rodapé das configurações, confirmação). */
    internal static Button BotaoCentral(RectTransform pai, Vector2 ancora, Vector2 pos, Vector2 tamanho, string texto, float tamanhoTexto,
        Color normal, Color destaque, Color corTexto, Color? corBarra)
    {
        RectTransform rt = Pos(Novo("Botao " + texto, pai), ancora, ancora, pos, tamanho);
        Image img = Img(rt, normal, uiSprite, 0.5f, true);
        Contorno(img, CorContorno);
        Txt(Tela("Texto", rt), texto, tamanhoTexto, corTexto, TextAlignmentOptions.Center, true);
        return FinalizarBotao(rt, img, normal, destaque, corBarra);
    }

    internal static Button FinalizarBotao(RectTransform rt, Image img, Color normal, Color destaque, Color? corBarra)
    {
        Button b = rt.gameObject.AddComponent<Button>();
        b.targetGraphic = img;
        b.transition = Selectable.Transition.None;   // o destaque é do ItemDeMenu
        GameObject barra = corBarra.HasValue ? BarraFoco(rt, corBarra.Value) : null;
        Item(rt.gameObject, b, img, normal, destaque, barra);
        return b;
    }

    internal static GameObject BarraFoco(RectTransform pai, Color cor)
    {
        RectTransform barra = Pos(Novo("Foco", pai), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), Vector2.zero, new Vector2(4f, 30f));
        Img(barra, cor, uiSprite, 2f);
        barra.gameObject.SetActive(false);
        return barra.gameObject;
    }

    internal static ItemDeMenu Item(GameObject go, Selectable alvo, Graphic fundo, Color normal, Color destaque, GameObject barra)
    {
        ItemDeMenu item = go.AddComponent<ItemDeMenu>();
        SerializedObject so = new SerializedObject(item);
        Ref(so, "alvo", alvo);
        Ref(so, "fundo", fundo);
        so.FindProperty("corNormal").colorValue = normal;
        so.FindProperty("corDestaque").colorValue = destaque;
        Ref(so, "barraFoco", barra);
        so.ApplyModifiedPropertiesWithoutUndo();
        return item;
    }

    internal static void Tecla(RectTransform pai, Vector2 ancora, Vector2 pos, string texto, Color corTexto, Color corFundo)
    {
        RectTransform k = Pos(Novo("Tecla", pai), ancora, ancora, pos, new Vector2(18f + texto.Length * 11f, 28f));
        Img(k, corFundo, uiSprite, 0.8f);
        Txt(Tela("Texto", k), texto, 13, corTexto, TextAlignmentOptions.Center, true, 1f);
    }

    internal static RectTransform Cartao(string nome, RectTransform tela, Vector2 tamanho)
    {
        RectTransform rt = Pos(Novo(nome, tela), Centro, Centro, Vector2.zero, tamanho);
        Image img = Img(rt, CorFundo, uiSprite, 0.35f, true);
        Shadow sombra = rt.gameObject.AddComponent<Shadow>();
        sombra.effectColor = CorSombra;
        sombra.effectDistance = new Vector2(0f, -10f);
        Contorno(img, CorContorno);
        return rt;
    }

    internal static void Contorno(Image img, Color cor)
    {
        Outline contorno = img.gameObject.AddComponent<Outline>();
        contorno.effectColor = cor;
        contorno.effectDistance = new Vector2(1f, -1f);
    }

    internal static void Divisoria(RectTransform cartao, float y, float largura)
    {
        Img(Pos(Novo("Divisoria", cartao), TopoCentro, TopoCentro, new Vector2(0f, y), new Vector2(largura, 1f)), CorLinha);
    }

    /* Círculo de acento com uma cruz de saúde (mesmo ícone dos outros painéis). */
    internal static void Cruz(RectTransform pai, Vector2 pos, float d)
    {
        RectTransform c = Pos(Novo("Icone", pai), TopoEsq, TopoEsq, pos, new Vector2(d, d));
        Img(c, CorRotulo, knobSprite);
        Img(Pos(Novo("H", c), Centro, Centro, Vector2.zero, new Vector2(d * 0.56f, d * 0.18f)), TextoClaro);
        Img(Pos(Novo("V", c), Centro, Centro, Vector2.zero, new Vector2(d * 0.18f, d * 0.56f)), TextoClaro);
    }

    /* ================= Navegação (controle / setas) ================= */

    internal static void Vertical(params Selectable[] itens)
    {
        Selectable[] ordem = System.Array.FindAll(itens, s => s != null);
        for (int i = 0; i < ordem.Length; i++)
            Navegar(ordem[i], i > 0 ? ordem[i - 1] : null, i < ordem.Length - 1 ? ordem[i + 1] : null, null, null);
    }

    internal static void Navegar(Selectable s, Selectable cima, Selectable baixo, Selectable esquerda, Selectable direita)
    {
        s.navigation = new Navigation
        {
            mode = Navigation.Mode.Explicit,
            selectOnUp = cima,
            selectOnDown = baixo,
            selectOnLeft = esquerda,
            selectOnRight = direita,
        };
    }

    /* ================= Cena ================= */

    internal static void GarantirEventSystem(StringBuilder log)
    {
        EventSystem es = Object.FindAnyObjectByType<EventSystem>();
        if (es == null)
        {
            GameObject go = new GameObject("EventSystem", typeof(EventSystem));
            Undo.RegisterCreatedObjectUndo(go, "Criar Menu de Pausa");
            es = go.GetComponent<EventSystem>();
            log.Append("\n  • EventSystem");
        }

        StandaloneInputModule antigo = es.GetComponent<StandaloneInputModule>();
        if (antigo != null)
        {
            Undo.DestroyObjectImmediate(antigo);   // não funciona com só o Input System novo
            log.Append("\n  • StandaloneInputModule trocado pelo InputSystemUIInputModule");
        }

        if (es.GetComponent<InputSystemUIInputModule>() == null)
        {
            InputSystemUIInputModule modulo = Undo.AddComponent<InputSystemUIInputModule>(es.gameObject);
            modulo.AssignDefaultActions();
            log.Append("\n  • InputSystemUIInputModule");
        }
    }

    private static void GarantirCanalNoCelular(StringBuilder log)
    {
        CelularController celular = Object.FindAnyObjectByType<CelularController>();
        AudioSource fonte = celular != null ? celular.GetComponent<AudioSource>() : null;
        if (fonte != null && fonte.GetComponent<CanalDeAudio>() == null)
        {
            Undo.AddComponent<CanalDeAudio>(fonte.gameObject);
            log.Append("\n  • CanalDeAudio (Efeitos) no ").Append(fonte.gameObject.name);
        }
    }

    /* ================= Utilitários ================= */

    internal static readonly Vector2 Centro = new Vector2(0.5f, 0.5f);
    internal static readonly Vector2 TopoEsq = new Vector2(0f, 1f);
    internal static readonly Vector2 TopoCentro = new Vector2(0.5f, 1f);
    internal static readonly Vector2 BaseCentro = new Vector2(0.5f, 0f);

    internal static void Ref(SerializedObject so, string campo, Object valor)
    {
        SerializedProperty p = so.FindProperty(campo);
        if (p == null)
        {
            Debug.LogWarning("[Menu de Pausa] Campo não encontrado: " + campo);
            return;
        }
        p.objectReferenceValue = valor;
    }

    internal static RectTransform Novo(string nome, Transform pai)
    {
        GameObject go = new GameObject(nome, typeof(RectTransform));
        go.layer = camadaUI;
        go.transform.SetParent(pai, false);
        return (RectTransform)go.transform;
    }

    internal static RectTransform Tela(string nome, Transform pai)
    {
        RectTransform rt = Novo(nome, pai);
        Esticar(rt);
        return rt;
    }

    internal static void Esticar(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    /* Faixa horizontal fina centrada na vertical (trilho do slider). */
    internal static void Faixa(RectTransform rt, float altura)
    {
        rt.anchorMin = new Vector2(0f, 0.5f);
        rt.anchorMax = new Vector2(1f, 0.5f);
        rt.sizeDelta = new Vector2(0f, altura);
    }

    internal static RectTransform Pos(RectTransform rt, Vector2 ancora, Vector2 pivo, Vector2 pos, Vector2 tamanho)
    {
        rt.anchorMin = ancora;
        rt.anchorMax = ancora;
        rt.pivot = pivo;
        rt.anchoredPosition = pos;
        rt.sizeDelta = tamanho;
        return rt;
    }

    internal static Image Img(RectTransform rt, Color cor, Sprite sprite = null, float ppu = 1f, bool raycast = false)
    {
        Image img = rt.gameObject.AddComponent<Image>();
        img.sprite = sprite;
        img.color = cor;
        img.raycastTarget = raycast;
        if (sprite == uiSprite && sprite != null)
        {
            img.type = Image.Type.Sliced;
            img.pixelsPerUnitMultiplier = ppu;
        }
        return img;
    }

    internal static TextMeshProUGUI Txt(RectTransform rt, string texto, float tamanho, Color cor, TextAlignmentOptions alinhamento,
        bool negrito = false, float espacamento = 0f)
    {
        TextMeshProUGUI tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
        tmp.text = texto;
        tmp.fontSize = tamanho;
        tmp.color = cor;
        tmp.alignment = alinhamento;
        tmp.characterSpacing = espacamento;
        tmp.fontStyle = negrito ? FontStyles.Bold : FontStyles.Normal;
        tmp.raycastTarget = false;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        return tmp;
    }
}
