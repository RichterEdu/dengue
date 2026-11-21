using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static MenuPausaBuilder;

/* ============================================================
 *  NotaBuilder.cs  (Editor)
 *  ---------------------------------------------------------------------
 *  Monta a interface da Parte C (nota, estrelas e recorde) nas duas cenas:
 *
 *    SampleScene
 *      - PlacarDaPartida no objeto do FluxoDeJogo.
 *      - FocoListaHUD começa fechado; FocoProximityGlow com raio de 3,5 m.
 *      - TelaDaNota nas caixas VitoriaCaixa e DerrotaCaixa.
 *      - AvisoDeMeta no FluxoDeJogoCanvas.
 *      - CartaoMetas filho do Cartao da TelaMenu (que vai para a esquerda):
 *        o MenuPausa escala o Cartao em janela baixa e o filho vai junto,
 *        por isso ele fica fora da lista de cartões do MenuPausa.
 *      - Referências ligadas no FluxoDeJogoHUD.
 *    MenuInicial
 *      - LinhaRecorde e RecordeNoMenu na TelaTitulo.
 *
 *  Menu:  Dengue > Configurar Nota e Recorde
 *
 *  As estrelas são sprites gerados aqui (a fonte do jogo não tem as
 *  estrelas), em Assets/Meus/Assets Gerados/Interface/.
 *
 *  Sem diálogos: em Play, ou com alguma cena suja, só avisa no Console e
 *  sai. O menu abre e salva as duas cenas sozinho e termina só com a
 *  MenuInicial aberta.
 *
 *  Idempotente: cada peça é procurada pelo nome antes de criar, e rodar de
 *  novo só repõe posições, textos e ligações.
 *
 *  ATENÇÃO: rodar Dengue > Criar Menu de Pausa ou Dengue > Criar Menu
 *  Inicial reconstrói a tela do zero e apaga o que este menu pôs nela.
 *  Depois de qualquer um dos dois, rode este menu de novo.
 *
 *  Os valores das metas e dos pontos não são daqui: ficam no
 *  PlacarDaPartida da cena.
 * ============================================================ */

public static class NotaBuilder
{
    private const string CaminhoJogo = "Assets/Scenes/SampleScene.unity";
    private const string CaminhoMenu = "Assets/Scenes/MenuInicial.unity";

    private const string PastaInterface = MeusOrganizador.Destino + "/Interface";
    private const string CaminhoEstrelaCheia = PastaInterface + "/Estrela Cheia.png";
    private const string CaminhoEstrelaVazia = PastaInterface + "/Estrela Vazia.png";

    /* ---- Forma da estrela ---- */
    private const int TamanhoDoSprite = 96;
    private const int PontasDoPoligono = 10;
    private const float RaioExterno = 0.98f;
    private const float RaioInterno = 0.45f;
    private const float EscalaDoMiolo = 0.68f;      // a vazia tira esta estrela menor do meio
    private const float DescidaDoMiolo = 0.02f;
    private const int Amostras = 3;                 // supersampling por lado

    /* ---- Tela final ---- */
    private const int LinhasDePontos = 6;
    private const float PassoDeMeta = 62f;
    private const float PassoDePonto = 36f;
    private const float TopoDasColunas = -364f;
    private const float ColunaPontos = 530f;
    private const float FimDosValores = 880f;

    private static readonly Color CorCheiaTela = new Color(0.25f, 1f, 0.8f, 1f);
    private static readonly Color CorVaziaTela = new Color(1f, 1f, 1f, 0.28f);
    private static readonly Color CorPerdidaTela = new Color(1f, 0.4f, 0.4f, 1f);
    private static readonly Color CorFracaTela = new Color(1f, 1f, 1f, 0.55f);
    private static readonly Color CorRecordeNovo = new Color(1f, 0.82f, 0.3f, 1f);
    private static readonly Color CorTextoTela = new Color(0.95f, 0.96f, 0.97f, 1f);
    private static readonly Color CorLinhaTela = new Color(1f, 1f, 1f, 0.08f);
    private static readonly Color CorFundoAviso = new Color(0.07f, 0.09f, 0.11f, 0.92f);

    /* ---- Cartão da pausa (tema branco) ---- */
    private const float PassoDeMetaCartao = 76f;
    private static readonly Color CorCheiaCartao = new Color(0.12f, 0.62f, 0.5f, 1f);
    private static readonly Color CorPerdidaCartao = new Color(0.8f, 0.25f, 0.25f, 1f);
    private static readonly Color CorRecordeVaziaCartao = new Color(0f, 0f, 0f, 0.3f);

    private static readonly Vector2 MeioEsq = new Vector2(0f, 0.5f);
    private static readonly Vector2 TopoDir = new Vector2(1f, 1f);

    [MenuItem("Dengue/Configurar Nota e Recorde")]
    public static void Configurar()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogWarning("[Nota] Saia do Play Mode antes de configurar.");
            return;
        }
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene aberta = SceneManager.GetSceneAt(i);
            if (aberta.isDirty)
            {
                Debug.LogWarning("[Nota] A cena " + aberta.name + " tem mudanças sem salvar: salve antes de configurar. Nada foi alterado.");
                return;
            }
        }

        PrepararRecursos();
        Sprite cheia = SalvarEstrela(true, CaminhoEstrelaCheia);
        Sprite vazia = SalvarEstrela(false, CaminhoEstrelaVazia);

        StringBuilder log = new StringBuilder();
        Scene jogo = EditorSceneManager.OpenScene(CaminhoJogo, OpenSceneMode.Single);
        bool montouJogo = MontarJogo(cheia, vazia, log);
        if (montouJogo)
        {
            EditorSceneManager.MarkSceneDirty(jogo);
            EditorSceneManager.SaveScene(jogo);
        }

        Scene menu = EditorSceneManager.OpenScene(CaminhoMenu, OpenSceneMode.Single);
        bool montouMenu = MontarMenuInicial(cheia, vazia, log);
        if (montouMenu)
        {
            EditorSceneManager.MarkSceneDirty(menu);
            EditorSceneManager.SaveScene(menu);
        }

        AssetDatabase.SaveAssets();
        if (montouJogo && montouMenu)
            Debug.Log("[Nota] Concluído (SampleScene e MenuInicial salvas)." + log);
    }

    /* ================= Estrelas ================= */

    /* Estrela branca (a cor vem do Image): polígono de 10 pontas com teste
     * ponto no polígono e supersampling. A vazia tira do meio a mesma estrela
     * menor e um pouco mais baixa, sobrando o contorno. */
    internal static Texture2D GerarEstrela(bool cheia, int tamanho)
    {
        Vector2[] poligono = PoligonoDaEstrela();
        Color32[] pixels = new Color32[tamanho * tamanho];
        Vector2 descida = new Vector2(0f, DescidaDoMiolo);

        for (int y = 0; y < tamanho; y++)
        {
            for (int x = 0; x < tamanho; x++)
            {
                int dentro = 0;
                for (int ay = 0; ay < Amostras; ay++)
                {
                    for (int ax = 0; ax < Amostras; ax++)
                    {
                        Vector2 p = new Vector2((x + (ax + 0.5f) / Amostras) / tamanho, (y + (ay + 0.5f) / Amostras) / tamanho) * 2f - Vector2.one;
                        if (DentroDoPoligono(p, poligono) && (cheia || !DentroDoPoligono((p + descida) / EscalaDoMiolo, poligono)))
                            dentro++;
                    }
                }
                pixels[y * tamanho + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(255f * dentro / (Amostras * Amostras)));
            }
        }

        Texture2D textura = new Texture2D(tamanho, tamanho, TextureFormat.RGBA32, false);
        textura.SetPixels32(pixels);
        textura.Apply();
        return textura;
    }

    private static Vector2[] PoligonoDaEstrela()
    {
        Vector2[] pontos = new Vector2[PontasDoPoligono];
        for (int i = 0; i < PontasDoPoligono; i++)
        {
            float raio = i % 2 == 0 ? RaioExterno : RaioInterno;
            float angulo = Mathf.PI / 2f - i * Mathf.PI * 2f / PontasDoPoligono;
            pontos[i] = new Vector2(Mathf.Cos(angulo), Mathf.Sin(angulo)) * raio;
        }
        return pontos;
    }

    /* Raio horizontal: dentro se cruza um número ímpar de lados. */
    private static bool DentroDoPoligono(Vector2 p, Vector2[] poligono)
    {
        bool dentro = false;
        for (int i = 0, j = poligono.Length - 1; i < poligono.Length; j = i++)
        {
            Vector2 a = poligono[i];
            Vector2 b = poligono[j];
            if ((a.y > p.y) != (b.y > p.y) && p.x < (b.x - a.x) * (p.y - a.y) / (b.y - a.y) + a.x)
                dentro = !dentro;
        }
        return dentro;
    }

    private static Sprite SalvarEstrela(bool cheia, string caminho)
    {
        MeusOrganizador.GarantirPasta(PastaInterface);

        Texture2D textura = GerarEstrela(cheia, TamanhoDoSprite);
        File.WriteAllBytes(caminho, textura.EncodeToPNG());
        Object.DestroyImmediate(textura);
        AssetDatabase.ImportAsset(caminho, ImportAssetOptions.ForceUpdate);

        TextureImporter importador = (TextureImporter)AssetImporter.GetAtPath(caminho);
        importador.textureType = TextureImporterType.Sprite;
        importador.spriteImportMode = SpriteImportMode.Single;
        importador.mipmapEnabled = false;
        importador.alphaIsTransparency = true;
        importador.wrapMode = TextureWrapMode.Clamp;
        importador.filterMode = FilterMode.Bilinear;
        importador.textureCompression = TextureImporterCompression.Uncompressed;
        importador.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(caminho);
    }

    /* ================= SampleScene ================= */

    /* Confere tudo antes de mexer: faltando uma peça, a cena fica intacta. */
    private static bool MontarJogo(Sprite cheia, Sprite vazia, StringBuilder log)
    {
        FluxoDeJogo fluxo = Unico<FluxoDeJogo>();
        FluxoDeJogoHUD hud = Unico<FluxoDeJogoHUD>();
        MenuPausa pausa = Unico<MenuPausa>();
        FocoListaHUD lista = Unico<FocoListaHUD>();
        FocoProximityGlow brilho = Unico<FocoProximityGlow>();
        RectTransform caixaVitoria = Achar("VitoriaCaixa");
        RectTransform caixaDerrota = Achar("DerrotaCaixa");
        RectTransform telaMenu = pausa != null ? new SerializedObject(pausa).FindProperty("telaMenu").objectReferenceValue as RectTransform : null;
        RectTransform cartaoDoMenu = telaMenu != null ? telaMenu.Find("Cartao") as RectTransform : null;

        if (fluxo == null || hud == null || pausa == null || lista == null || brilho == null
            || caixaVitoria == null || caixaDerrota == null || cartaoDoMenu == null)
        {
            Debug.LogError("[Nota] Faltam peças na SampleScene (FluxoDeJogo, FluxoDeJogoHUD, MenuPausa, FocoListaHUD, " +
                           "FocoProximityGlow, VitoriaCaixa, DerrotaCaixa ou o Cartao da TelaMenu). A cena não foi alterada.");
            return false;
        }

        PlacarDaPartida placar = Garantir<PlacarDaPartida>(fluxo.gameObject);
        Gravar(lista, "comecarAberto", false);
        Gravar(brilho, "raioProximidade", 3.5f);

        TelaDaNota telaVitoria = MontarTelaDaNota(caixaVitoria, "Vitoria", cheia, vazia);
        TelaDaNota telaDerrota = MontarTelaDaNota(caixaDerrota, "Derrota", cheia, vazia);
        MontarAviso((RectTransform)hud.transform, caixaVitoria.parent, placar, vazia);
        MontarCartaoDeMetas(pausa, telaMenu, cartaoDoMenu, placar, cheia, vazia);

        SerializedObject so = new SerializedObject(hud);
        Ref(so, "placar", placar);
        Ref(so, "telaVitoria", telaVitoria);
        Ref(so, "telaDerrota", telaDerrota);
        so.ApplyModifiedPropertiesWithoutUndo();

        log.Append("\n  • SampleScene: PlacarDaPartida em ").Append(fluxo.name)
           .Append("; TelaDaNota nas 2 caixas; AvisoDeMeta; CartaoMetas (").Append(pausa.name).Append(")");
        return true;
    }

    /* ---------- Tela final ---------- */

    private static TelaDaNota MontarTelaDaNota(RectTransform caixa, string prefixo, Sprite cheia, Sprite vazia)
    {
        Pos(caixa, Centro, Centro, Vector2.zero, new Vector2(920f, 630f));

        RectTransform titulo = caixa.Find(prefixo + "Titulo") as RectTransform;
        if (titulo != null)
            titulo.anchoredPosition = new Vector2(0f, -30f);

        foreach (string nome in new[] { "IconeCirculo", "Divisor", prefixo + "Texto" })
        {
            Transform sai = caixa.Find(nome);
            if (sai != null)
                sai.gameObject.SetActive(false);
        }

        Image[] estrelas = new Image[3];
        for (int i = 0; i < estrelas.Length; i++)
            estrelas[i] = Imagem(Peca(caixa, "Estrela " + (i + 1), TopoCentro, Centro, new Vector2(-92f + 92f * i, -132f), new Vector2(76f, 76f)), CorVaziaTela, vazia);

        TextMeshProUGUI textoNota = Texto(Peca(caixa, "TextoNota", TopoCentro, Centro, new Vector2(0f, -222f), new Vector2(400f, 80f)),
            "0", 68, CorTextoTela, TextAlignmentOptions.Center, true);
        TextMeshProUGUI textoRecorde = Texto(Peca(caixa, "TextoRecorde", TopoCentro, Centro, new Vector2(0f, -272f), new Vector2(400f, 30f)),
            "", 20, CorFracaTela, TextAlignmentOptions.Center);
        Imagem(Peca(caixa, "Linha", TopoCentro, Centro, new Vector2(0f, -306f), new Vector2(840f, 2f)), CorLinhaTela);

        Texto(Peca(caixa, "RotuloMetas", TopoEsq, TopoEsq, new Vector2(40f, -326f), new Vector2(300f, 24f)),
            "METAS", 16, CorFracaTela, TextAlignmentOptions.Left, true);
        Texto(Peca(caixa, "RotuloPontos", TopoEsq, TopoEsq, new Vector2(ColunaPontos, -326f), new Vector2(300f, 24f)),
            "PONTOS", 16, CorFracaTela, TextAlignmentOptions.Left, true);

        int metas = AvaliacaoDaPartida.Metas.Length;
        Image[] estrelasDasMetas = new Image[metas];
        TextMeshProUGUI[] nomes = new TextMeshProUGUI[metas];
        TextMeshProUGUI[] detalhes = new TextMeshProUGUI[metas];
        for (int i = 0; i < metas; i++)
        {
            float y = TopoDasColunas - PassoDeMeta * i;
            estrelasDasMetas[i] = Imagem(Peca(caixa, "Meta " + (i + 1) + " Estrela", TopoEsq, TopoEsq, new Vector2(40f, y - 6f), new Vector2(30f, 30f)), CorVaziaTela, vazia);
            nomes[i] = Texto(Peca(caixa, "Meta " + (i + 1) + " Nome", TopoEsq, TopoEsq, new Vector2(84f, y), new Vector2(380f, 28f)),
                AvaliacaoDaPartida.Nome(AvaliacaoDaPartida.Metas[i]), 22, CorTextoTela, TextAlignmentOptions.Left);
            detalhes[i] = Texto(Peca(caixa, "Meta " + (i + 1) + " Detalhe", TopoEsq, TopoEsq, new Vector2(84f, y - 28f), new Vector2(380f, 28f)),
                "", 17, CorFracaTela, TextAlignmentOptions.Left);
        }

        TextMeshProUGUI[] rotulos = new TextMeshProUGUI[LinhasDePontos];
        TextMeshProUGUI[] valores = new TextMeshProUGUI[LinhasDePontos];
        for (int i = 0; i < LinhasDePontos; i++)
        {
            float y = TopoDasColunas - PassoDePonto * i;
            rotulos[i] = Texto(Peca(caixa, "Ponto " + (i + 1) + " Rotulo", TopoEsq, TopoEsq, new Vector2(ColunaPontos, y), new Vector2(260f, 28f)),
                "", 21, CorTextoTela, TextAlignmentOptions.Left);
            valores[i] = Texto(Peca(caixa, "Ponto " + (i + 1) + " Valor", TopoEsq, TopoDir, new Vector2(FimDosValores, y), new Vector2(120f, 28f)),
                "", 21, CorTextoTela, TextAlignmentOptions.Right, true);
        }

        Texto(Peca(caixa, "Rodape", BaseCentro, BaseCentro, new Vector2(0f, 20f), new Vector2(500f, 28f)),
            "Aperte R para jogar de novo", 17, CorFracaTela, TextAlignmentOptions.Center);

        TelaDaNota tela = Garantir<TelaDaNota>(caixa.gameObject);
        SerializedObject so = new SerializedObject(tela);
        Lista(so, "estrelas", estrelas);
        Ref(so, "estrelaCheia", cheia);
        Ref(so, "estrelaVazia", vazia);
        so.FindProperty("corCheia").colorValue = CorCheiaTela;
        so.FindProperty("corVazia").colorValue = CorVaziaTela;
        so.FindProperty("corPerdida").colorValue = CorPerdidaTela;
        so.FindProperty("corTextoFraco").colorValue = CorFracaTela;
        so.FindProperty("corRecordeNovo").colorValue = CorRecordeNovo;
        Ref(so, "textoNota", textoNota);
        Ref(so, "textoRecorde", textoRecorde);
        Lista(so, "estrelasDasMetas", estrelasDasMetas);
        Lista(so, "nomesDasMetas", nomes);
        Lista(so, "detalhesDasMetas", detalhes);
        Lista(so, "rotulosDePontos", rotulos);
        Lista(so, "valoresDePontos", valores);
        so.ApplyModifiedPropertiesWithoutUndo();
        return tela;
    }

    /* ---------- Aviso de meta perdida ---------- */

    private static void MontarAviso(RectTransform canvas, Transform painelDeVitoria, PlacarDaPartida placar, Sprite vazia)
    {
        bool nova = canvas.Find("AvisoDeMeta") == null;
        RectTransform faixa = Peca(canvas, "AvisoDeMeta", TopoCentro, TopoCentro, new Vector2(0f, -126f), new Vector2(420f, 46f));
        if (nova)
            faixa.SetSiblingIndex(painelDeVitoria.GetSiblingIndex());   // atrás das telas finais

        Imagem(faixa, CorFundoAviso);

        CanvasGroup grupo = Garantir<CanvasGroup>(faixa.gameObject);
        grupo.alpha = 0f;
        grupo.interactable = false;
        grupo.blocksRaycasts = false;

        Imagem(Peca(faixa, "Estrela", MeioEsq, MeioEsq, new Vector2(18f, 0f), new Vector2(24f, 24f)), CorPerdidaTela, vazia);
        Texto(Peca(faixa, "Rotulo", MeioEsq, MeioEsq, new Vector2(54f, 0f), new Vector2(136f, 30f)),
            "META PERDIDA", 15, CorPerdidaTela, TextAlignmentOptions.Left, true);
        TextMeshProUGUI textoMeta = Texto(Peca(faixa, "TextoMeta", MeioEsq, MeioEsq, new Vector2(194f, 0f), new Vector2(216f, 30f)),
            "", 21, CorTextoTela, TextAlignmentOptions.Left);

        SerializedObject so = new SerializedObject(Garantir<AvisoDeMeta>(faixa.gameObject));
        Ref(so, "placar", placar);
        Ref(so, "grupo", grupo);
        Ref(so, "textoMeta", textoMeta);
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    /* ---------- Cartão da pausa ---------- */

    private static void MontarCartaoDeMetas(MenuPausa pausa, RectTransform telaMenu, RectTransform cartaoDoMenu,
        PlacarDaPartida placar, Sprite cheia, Sprite vazia)
    {
        cartaoDoMenu.anchoredPosition = new Vector2(-220f, 0f);

        Vector2 tamanho = new Vector2(400f, 420f);
        RectTransform cartao = cartaoDoMenu.Find("CartaoMetas") as RectTransform;
        if (cartao == null)
            cartao = telaMenu.Find("CartaoMetas") as RectTransform;   // cena montada quando ele era filho da TelaMenu
        if (cartao == null)
            cartao = Cartao("CartaoMetas", cartaoDoMenu, tamanho);
        cartao.SetParent(cartaoDoMenu, false);

        // A âncora é o centro do cartão do menu, que está em −220: fica em 60, 356 na tela.
        Pos(cartao, Centro, TopoEsq, new Vector2(280f, 356f), tamanho);

        Texto(Peca(cartao, "Titulo", TopoEsq, TopoEsq, new Vector2(32f, -44f), new Vector2(336f, 24f)),
            "METAS DA PARTIDA", 15, CorTextoFraco, TextAlignmentOptions.Left, true);

        int metas = AvaliacaoDaPartida.Metas.Length;
        Image[] estrelasDasMetas = new Image[metas];
        TextMeshProUGUI[] nomes = new TextMeshProUGUI[metas];
        TextMeshProUGUI[] detalhes = new TextMeshProUGUI[metas];
        for (int i = 0; i < metas; i++)
        {
            float y = -92f - PassoDeMetaCartao * i;
            estrelasDasMetas[i] = Imagem(Peca(cartao, "Meta " + (i + 1) + " Estrela", TopoEsq, TopoEsq, new Vector2(32f, y), new Vector2(30f, 30f)), CorCheiaCartao, cheia);
            nomes[i] = Texto(Peca(cartao, "Meta " + (i + 1) + " Nome", TopoEsq, TopoEsq, new Vector2(76f, y), new Vector2(292f, 28f)),
                AvaliacaoDaPartida.Nome(AvaliacaoDaPartida.Metas[i]), 22, CorTexto, TextAlignmentOptions.Left);
            detalhes[i] = Texto(Peca(cartao, "Meta " + (i + 1) + " Detalhe", TopoEsq, TopoEsq, new Vector2(76f, y - 28f), new Vector2(292f, 28f)),
                "", 17, CorTextoFraco, TextAlignmentOptions.Left);
        }

        Imagem(Peca(cartao, "Linha", TopoEsq, TopoEsq, new Vector2(32f, -326f), new Vector2(336f, 2f)), CorLinha);

        // Filha do cartão, e não o cartão: é ela que some sem recorde.
        RectTransform linha = Peca(cartao, "RecordeDoCartao", TopoEsq, TopoEsq, new Vector2(0f, -344f), new Vector2(400f, 32f));
        Texto(Peca(linha, "Rotulo", MeioEsq, MeioEsq, new Vector2(32f, 0f), new Vector2(92f, 24f)),
            "RECORDE", 15, CorTextoFraco, TextAlignmentOptions.Left);
        TextMeshProUGUI pontos = Texto(Peca(linha, "Pontos", MeioEsq, MeioEsq, new Vector2(128f, 0f), new Vector2(94f, 32f)),
            "0", 26, CorTexto, TextAlignmentOptions.Left, true);
        Image[] estrelas = Fileira(linha, 226f, CorRecordeVaziaCartao, vazia);
        linha.gameObject.SetActive(false);

        CartaoDeMetas componente = Garantir<CartaoDeMetas>(cartao.gameObject);
        SerializedObject so = new SerializedObject(componente);
        Ref(so, "placar", placar);
        Lista(so, "estrelasDasMetas", estrelasDasMetas);
        Lista(so, "nomesDasMetas", nomes);
        Lista(so, "detalhesDasMetas", detalhes);
        Ref(so, "estrelaCheia", cheia);
        Ref(so, "estrelaVazia", vazia);
        so.FindProperty("corCheia").colorValue = CorCheiaCartao;
        so.FindProperty("corPerdida").colorValue = CorPerdidaCartao;
        so.FindProperty("corTextoFraco").colorValue = CorTextoFraco;
        so.FindProperty("corRecordeVazia").colorValue = CorRecordeVaziaCartao;
        Ref(so, "linhaDoRecorde", linha.gameObject);
        Ref(so, "textoRecorde", pontos);
        Lista(so, "estrelasDoRecorde", estrelas);
        so.ApplyModifiedPropertiesWithoutUndo();

        SerializedObject soPausa = new SerializedObject(pausa);
        RetirarDaLista(soPausa.FindProperty("cartoes"), cartao);
        Ref(soPausa, "cartaoDeMetas", componente);
        soPausa.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void RetirarDaLista(SerializedProperty lista, Object item)
    {
        int ficam = 0;
        for (int i = 0; i < lista.arraySize; i++)
        {
            Object atual = lista.GetArrayElementAtIndex(i).objectReferenceValue;
            if (atual != item)
                lista.GetArrayElementAtIndex(ficam++).objectReferenceValue = atual;
        }
        lista.arraySize = ficam;
    }

    /* ================= MenuInicial ================= */

    private static bool MontarMenuInicial(Sprite cheia, Sprite vazia, StringBuilder log)
    {
        RectTransform tela = Achar("TelaTitulo");
        RectTransform assinatura = tela != null ? tela.Find("Assinatura") as RectTransform : null;
        if (assinatura == null)
        {
            Debug.LogError("[Nota] TelaTitulo com Assinatura não encontrada na MenuInicial. A cena não foi alterada.");
            return false;
        }

        RectTransform linha = Peca(tela, "LinhaRecorde", Vector2.zero, Vector2.zero, new Vector2(assinatura.anchoredPosition.x, 96f), new Vector2(420f, 52f));
        Texto(Peca(linha, "Rotulo", MeioEsq, MeioEsq, Vector2.zero, new Vector2(90f, 30f)),
            "RECORDE", 15, CorFracaTela, TextAlignmentOptions.Left);
        TextMeshProUGUI pontos = Texto(Peca(linha, "Pontos", MeioEsq, MeioEsq, new Vector2(96f, 0f), new Vector2(90f, 36f)),
            "0", 26, Color.white, TextAlignmentOptions.Left, true);
        Image[] estrelas = Fileira(linha, 188f, CorVaziaTela, vazia);
        linha.gameObject.SetActive(false);

        // Na TelaTitulo, e não na linha: o Start roda com a linha inativa.
        SerializedObject so = new SerializedObject(Garantir<RecordeNoMenu>(tela.gameObject));
        Ref(so, "linha", linha.gameObject);
        Ref(so, "textoPontos", pontos);
        Lista(so, "estrelas", estrelas);
        Ref(so, "estrelaCheia", cheia);
        Ref(so, "estrelaVazia", vazia);
        so.FindProperty("corCheia").colorValue = CorCheiaTela;
        so.FindProperty("corVazia").colorValue = CorVaziaTela;
        so.ApplyModifiedPropertiesWithoutUndo();

        log.Append("\n  • MenuInicial: LinhaRecorde e RecordeNoMenu em ").Append(tela.name);
        return true;
    }

    /* ================= Peças ================= */

    /* As 3 estrelas pequenas da linha de recorde, a partir de x, de 28 em 28. */
    private static Image[] Fileira(RectTransform linha, float x, Color cor, Sprite vazia)
    {
        Image[] estrelas = new Image[3];
        for (int i = 0; i < estrelas.Length; i++)
            estrelas[i] = Imagem(Peca(linha, "Estrela " + (i + 1), MeioEsq, MeioEsq, new Vector2(x + 28f * i, 0f), new Vector2(24f, 24f)), cor, vazia);
        return estrelas;
    }

    /* Filho 'nome' de 'pai' (criado se faltar), na posição pedida. */
    private static RectTransform Peca(RectTransform pai, string nome, Vector2 ancora, Vector2 pivo, Vector2 pos, Vector2 tamanho)
    {
        RectTransform rt = pai.Find(nome) as RectTransform;
        if (rt == null)
            rt = Novo(nome, pai);
        return Pos(rt, ancora, pivo, pos, tamanho);
    }

    private static Image Imagem(RectTransform rt, Color cor, Sprite sprite = null)
    {
        Image img = rt.GetComponent<Image>();
        if (img == null)
            img = Img(rt, cor, sprite);
        img.sprite = sprite;
        img.color = cor;
        img.preserveAspect = sprite != null;
        img.raycastTarget = false;
        return img;
    }

    private static TextMeshProUGUI Texto(RectTransform rt, string texto, float corpo, Color cor, TextAlignmentOptions alinhamento, bool negrito = false)
    {
        TextMeshProUGUI tmp = rt.GetComponent<TextMeshProUGUI>();
        if (tmp == null)
            tmp = Txt(rt, texto, corpo, cor, alinhamento, negrito);

        tmp.text = texto;
        tmp.fontSize = corpo;
        tmp.color = cor;
        tmp.alignment = alinhamento;
        tmp.fontStyle = negrito ? FontStyles.Bold : FontStyles.Normal;
        if (tmp.font == null)
            tmp.font = TMP_Settings.defaultFontAsset;   // numa peça inativa o Awake do TMP não pôs a fonte
        return tmp;
    }

    /* ================= Utilidades ================= */

    private static T Garantir<T>(GameObject go) where T : Component
    {
        T c = go.GetComponent<T>();
        return c != null ? c : go.AddComponent<T>();
    }

    private static void Gravar(Object alvo, string campo, object valor)
    {
        SerializedObject so = new SerializedObject(alvo);
        SerializedProperty p = so.FindProperty(campo);
        if (valor is bool b) p.boolValue = b;
        else p.floatValue = (float)valor;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void Lista(SerializedObject so, string campo, IList<Object> itens)
    {
        SerializedProperty p = so.FindProperty(campo);
        p.arraySize = itens.Count;
        for (int i = 0; i < itens.Count; i++)
            p.GetArrayElementAtIndex(i).objectReferenceValue = itens[i];
    }

    /* O único T da cena aberta (null se faltar ou houver mais de um). */
    private static T Unico<T>() where T : Component
    {
        T[] achados = Object.FindObjectsByType<T>(FindObjectsInactive.Include);
        return achados.Length == 1 ? achados[0] : null;
    }

    /* Primeiro RectTransform da cena ativa com este nome (inclusive inativo). */
    private static RectTransform Achar(string nome)
    {
        foreach (GameObject raiz in SceneManager.GetActiveScene().GetRootGameObjects())
            foreach (RectTransform rt in raiz.GetComponentsInChildren<RectTransform>(true))
                if (rt.name == nome)
                    return rt;
        return null;
    }
}
