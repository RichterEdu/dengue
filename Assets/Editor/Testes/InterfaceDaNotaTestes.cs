using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/* ============================================================
 *  InterfaceDaNotaTestes.cs
 *  ---------------------------------------------------------------------
 *  Testes EditMode (Parte C) das telas da nota: TelaDaNota (estrelas e
 *  textos da tela final), AvisoDeMeta (faixa de meta perdida),
 *  CartaoDeMetas (cartão da pausa), RecordeNoMenu (linha do menu
 *  inicial) e a escolha entre a tela nova e o texto antigo no
 *  FluxoDeJogoHUD (PreencherFim).
 *
 *  O Start e o Update não rodam no EditMode: o placar é ligado com
 *  Ligar, os componentes recebem as referências por Setar (e por
 *  SetarArray, que cobre os arrays que o Setar não aceita) e os métodos
 *  internal são chamados direto. O recorde de teste usa o prefixo
 *  "teste.interface.", apagado no [SetUp] e no [TearDown].
 *
 *  Rodar:  Window > General > Test Runner > EditMode (categoria ParteC)
 *  ou      unity command run_tests --mode EditMode --filter_type category --filter ParteC
 * ============================================================ */

[Category("ParteC")]
public class InterfaceDaNotaTestes
{
    private const string Prefixo = "teste.interface.";

    private FluxoDeJogo fluxo;
    private GerenciadorDeFocos gerenciador;
    private PlacarDaPartida placar;
    private KitDoAgente kit;
    private Sprite cheia;
    private Sprite vazia;
    private readonly List<Object> temporarios = new List<Object>();

    [SetUp]
    public void Preparar()
    {
        new RecordeDoJogo(Prefixo).Apagar();

        fluxo = TesteUtil.Com<FluxoDeJogo>("Fluxo (teste)");
        gerenciador = TesteUtil.Com<GerenciadorDeFocos>("Gerenciador (teste)");
        TesteUtil.Setar(gerenciador, "fluxo", fluxo);
        TesteUtil.Setar(fluxo, "gerenciador", gerenciador);
        fluxo.Ligar();

        placar = TesteUtil.Com<PlacarDaPartida>("Placar (teste)");
        TesteUtil.Setar(placar, "fluxo", fluxo);
        TesteUtil.Setar(placar, "gerenciador", gerenciador);
        placar.UsarRecorde(new RecordeDoJogo(Prefixo));
        placar.Ligar();

        fluxo.DefinirEstadoParaTeste(true, false);

        ControladorJogador jogador = TesteUtil.Com<ControladorJogador>("Jogador (teste)");
        kit = jogador.gameObject.AddComponent<KitDoAgente>();

        cheia = NovoSprite();
        vazia = NovoSprite();
    }

    [TearDown]
    public void Limpar()
    {
        new RecordeDoJogo(Prefixo).Apagar();
        TesteUtil.LimparTudo();
        foreach (Object o in temporarios)
            if (o != null)
                Object.DestroyImmediate(o);
        temporarios.Clear();
    }

    /* ---------------- Ajudantes ---------------- */

    private Sprite NovoSprite()
    {
        Texture2D textura = new Texture2D(2, 2);
        Sprite sprite = Sprite.Create(textura, new Rect(0, 0, 2, 2), Vector2.zero);
        temporarios.Add(textura);
        temporarios.Add(sprite);
        return sprite;
    }

    private static Image Imagem() => TesteUtil.Com<Image>("Imagem (teste)");

    private static TextMeshProUGUI Texto() => TesteUtil.Com<TextMeshProUGUI>("Texto (teste)");

    private static Image[] Imagens(int n)
    {
        Image[] itens = new Image[n];
        for (int i = 0; i < n; i++)
            itens[i] = Imagem();
        return itens;
    }

    private static TextMeshProUGUI[] Textos(int n)
    {
        TextMeshProUGUI[] itens = new TextMeshProUGUI[n];
        for (int i = 0; i < n; i++)
            itens[i] = Texto();
        return itens;
    }

    /* O TesteUtil.Setar não aceita arrays: preenche pelo SerializedObject. */
    private static void SetarArray(Object alvo, string campo, Object[] itens)
    {
        SerializedObject so = new SerializedObject(alvo);
        SerializedProperty prop = so.FindProperty(campo);
        Assert.IsNotNull(prop, $"campo {campo} não existe em {alvo.GetType().Name}");
        prop.arraySize = itens.Length;
        for (int i = 0; i < itens.Length; i++)
            prop.GetArrayElementAtIndex(i).objectReferenceValue = itens[i];
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    /* Vaso esvaziado (3 passos com as Mãos) e registrado no gerenciador. */
    private VasoInteractable NovoVasoEsvaziado()
    {
        VasoInteractable vaso = TesteUtil.Com<VasoInteractable>("Vaso (teste)");
        TesteUtil.Setar(vaso, "ferramentaNecessaria", Ferramenta.Areia);
        TesteUtil.Setar(vaso, "podeEsvaziar", true);
        TesteUtil.Setar(vaso, "requiredClicks", 3);
        TesteUtil.Setar(vaso, "punchScale", 0f);
        TesteUtil.Setar(vaso, "shrinkDuration", 0f);
        gerenciador.Registrar(new Interactable[] { vaso });

        for (int i = 0; i < 3; i++)
            vaso.AvancarCom(kit);
        Assert.IsTrue(vaso.Esvaziado, "o vaso de teste começa esvaziado");
        return vaso;
    }

    private TelaDaNota NovaTela(out TextMeshProUGUI textoNota, out TextMeshProUGUI textoRecorde)
    {
        TelaDaNota tela = TesteUtil.Com<TelaDaNota>("Tela (teste)");
        textoNota = Texto();
        textoRecorde = Texto();
        TesteUtil.Setar(tela, "textoNota", textoNota);
        TesteUtil.Setar(tela, "textoRecorde", textoRecorde);
        TesteUtil.Setar(tela, "estrelaCheia", cheia);
        TesteUtil.Setar(tela, "estrelaVazia", vazia);
        return tela;
    }

    private FluxoDeJogoHUD NovoHud(out TextMeshProUGUI vitoriaTexto)
    {
        FluxoDeJogoHUD hud = TesteUtil.Com<FluxoDeJogoHUD>("HUD (teste)");
        vitoriaTexto = Texto();
        TesteUtil.Setar(hud, "fluxo", fluxo);
        TesteUtil.Setar(hud, "vitoriaTexto", vitoriaTexto);
        return hud;
    }

    private AvisoDeMeta NovoAviso(out TextMeshProUGUI textoMeta)
    {
        AvisoDeMeta aviso = TesteUtil.Com<AvisoDeMeta>("Aviso (teste)");
        textoMeta = Texto();
        TesteUtil.Setar(aviso, "placar", placar);
        TesteUtil.Setar(aviso, "textoMeta", textoMeta);
        return aviso;
    }

    private CartaoDeMetas NovoCartao(out Image[] estrelasMetas, out TextMeshProUGUI[] detalhes)
    {
        CartaoDeMetas cartao = TesteUtil.Com<CartaoDeMetas>("Cartão (teste)");
        estrelasMetas = Imagens(3);
        detalhes = Textos(3);
        TesteUtil.Setar(cartao, "placar", placar);
        TesteUtil.Setar(cartao, "estrelaCheia", cheia);
        TesteUtil.Setar(cartao, "estrelaVazia", vazia);
        SetarArray(cartao, "estrelasDasMetas", estrelasMetas);
        SetarArray(cartao, "nomesDasMetas", Textos(3));
        SetarArray(cartao, "detalhesDasMetas", detalhes);
        return cartao;
    }

    /* ---------------- TelaDaNota ---------------- */

    [Test]
    public void PintarEstrelas_EsquerdaParaDireita()
    {
        Image[] i = { Imagem(), Imagem(), Imagem() };
        TelaDaNota.PintarEstrelas(i, 2, cheia, vazia, Color.green, Color.gray);
        Assert.AreSame(cheia, i[0].sprite);
        Assert.AreSame(cheia, i[1].sprite);
        Assert.AreSame(vazia, i[2].sprite);
        Assert.AreEqual(Color.green, i[1].color);
        Assert.AreEqual(Color.gray, i[2].color);
    }

    [Test]
    public void PintarEstrelas_IgnoraNulos()
    {
        Image[] i = { null, Imagem(), null };
        Assert.DoesNotThrow(() => TelaDaNota.PintarEstrelas(i, 3, cheia, vazia, Color.green, Color.gray));
        Assert.AreSame(cheia, i[1].sprite);
        Assert.DoesNotThrow(() => TelaDaNota.PintarEstrelas(null, 3, cheia, vazia, Color.green, Color.gray));
    }

    [Test]
    public void Tela_Mostrar_PreencheNotaMetasEPontos()
    {
        TelaDaNota tela = NovaTela(out TextMeshProUGUI textoNota, out TextMeshProUGUI textoRecorde);
        Image[] estrelas = Imagens(3);
        Image[] estrelasMetas = Imagens(3);
        TextMeshProUGUI[] nomes = Textos(3);
        TextMeshProUGUI[] detalhes = Textos(3);
        TextMeshProUGUI[] rotulos = Textos(6);
        TextMeshProUGUI[] valores = Textos(6);
        SetarArray(tela, "estrelas", estrelas);
        SetarArray(tela, "estrelasDasMetas", estrelasMetas);
        SetarArray(tela, "nomesDasMetas", nomes);
        SetarArray(tela, "detalhesDasMetas", detalhes);
        SetarArray(tela, "rotulosDePontos", rotulos);
        SetarArray(tela, "valoresDePontos", valores);

        RegrasDaNota regras = placar.Regras;
        DadosDaPartida d = new DadosDaPartida
        {
            Venceu = true, Segundos = 1100f, Pico = 0.9f, Eliminados = 30, Total = 30, Reenchidos = 0
        };
        ResultadoDaNota n = AvaliacaoDaPartida.Calcular(d, regras);   // rápida e sem retrabalho; pico 90% perde

        tela.Mostrar(n, d, regras, true, 5000);

        Assert.AreSame(cheia, estrelas[1].sprite);
        Assert.AreSame(vazia, estrelas[2].sprite);
        Assert.AreEqual(AvaliacaoDaPartida.Formatar(n.Pontos), textoNota.text);
        Assert.AreEqual("NOVO RECORDE", textoRecorde.text);
        Assert.AreEqual("Resposta rápida", nomes[0].text);
        Assert.AreEqual("Hospital sob controle", nomes[2].text);
        Assert.AreSame(cheia, estrelasMetas[0].sprite);
        Assert.AreSame(vazia, estrelasMetas[2].sprite);
        Assert.AreEqual("pico 90% de 85%", detalhes[2].text);
        Assert.AreNotEqual(detalhes[2].color, detalhes[0].color, "meta perdida em vermelho");
        StringAssert.StartsWith("Focos", rotulos[0].text);
        Assert.AreEqual(AvaliacaoDaPartida.Formatar(n.DeFocos), valores[0].text);
        Assert.AreEqual("", rotulos[5].text, "as sobras ficam limpas");
    }

    [Test]
    public void Tela_Mostrar_Derrota_ExigeVencer()
    {
        TelaDaNota tela = NovaTela(out _, out TextMeshProUGUI textoRecorde);
        TextMeshProUGUI[] detalhes = Textos(3);
        TextMeshProUGUI[] rotulos = Textos(6);
        SetarArray(tela, "detalhesDasMetas", detalhes);
        SetarArray(tela, "rotulosDePontos", rotulos);

        DadosDaPartida d = new DadosDaPartida { Venceu = false, Segundos = 600f, Pico = 1f, Eliminados = 14, Total = 30 };
        ResultadoDaNota n = AvaliacaoDaPartida.Calcular(d, placar.Regras);
        tela.Mostrar(n, d, placar.Regras, false, 6280);

        Assert.AreEqual("exige vencer", detalhes[0].text);
        Assert.AreEqual("Recorde: 6.280", textoRecorde.text);
        Assert.AreEqual("Faltaram 16 focos", rotulos[1].text);
    }

    [Test]
    public void Tela_Mostrar_SemReferencias_NaoQuebra()
    {
        TelaDaNota tela = TesteUtil.Com<TelaDaNota>("Tela vazia (teste)");
        DadosDaPartida d = new DadosDaPartida { Venceu = true, Eliminados = 30, Total = 30 };
        ResultadoDaNota n = AvaliacaoDaPartida.Calcular(d, placar.Regras);
        SetarArray(tela, "estrelas", new Object[] { null, null, null });
        SetarArray(tela, "detalhesDasMetas", new Object[] { null });
        Assert.DoesNotThrow(() => tela.Mostrar(n, d, placar.Regras, false, 0));
    }

    /* ---------------- FluxoDeJogoHUD ---------------- */

    [Test]
    public void HUD_SemPlacar_MantemEstatisticas()
    {
        FluxoDeJogoHUD hud = NovoHud(out TextMeshProUGUI vitoriaTexto);
        hud.PreencherFim(true);
        StringAssert.Contains("Focos eliminados", vitoriaTexto.text);
    }

    [Test]
    public void HUD_ComPlacar_UsaATela_ENaoOTexto()
    {
        FluxoDeJogoHUD hud = NovoHud(out TextMeshProUGUI vitoriaTexto);
        TelaDaNota tela = NovaTela(out TextMeshProUGUI textoNota, out _);
        TesteUtil.Setar(hud, "placar", placar);
        TesteUtil.Setar(hud, "telaVitoria", tela);

        hud.PreencherFim(true);

        Assert.AreEqual("", vitoriaTexto.text ?? "");
        Assert.AreEqual(AvaliacaoDaPartida.Formatar(placar.Resultado.Pontos), textoNota.text);
        Assert.IsTrue(placar.Fechado);
        Assert.IsTrue(placar.Dados.Venceu);
    }

    [Test]
    public void Tela_Mostrar_RecordeNovo_FicaEmNegrito_ENaoVazaParaOProximo()
    {
        TelaDaNota tela = NovaTela(out _, out TextMeshProUGUI textoRecorde);
        DadosDaPartida d = new DadosDaPartida { Venceu = true, Eliminados = 30, Total = 30 };
        ResultadoDaNota n = AvaliacaoDaPartida.Calcular(d, placar.Regras);

        tela.Mostrar(n, d, placar.Regras, true, 5000);
        Assert.IsTrue((textoRecorde.fontStyle & FontStyles.Bold) != 0, "NOVO RECORDE em negrito");

        tela.Mostrar(n, d, placar.Regras, false, 6280);
        Assert.AreEqual("Recorde: 6.280", textoRecorde.text);
        Assert.IsTrue((textoRecorde.fontStyle & FontStyles.Bold) == 0, "o negrito anterior não fica");
    }

    [Test]
    public void Tela_Mostrar_SemRecordeNovo_FicaNormal()
    {
        TelaDaNota tela = NovaTela(out _, out TextMeshProUGUI textoRecorde);
        DadosDaPartida d = new DadosDaPartida { Venceu = false, Eliminados = 10, Total = 30 };
        ResultadoDaNota n = AvaliacaoDaPartida.Calcular(d, placar.Regras);
        tela.Mostrar(n, d, placar.Regras, false, 6280);
        Assert.IsTrue((textoRecorde.fontStyle & FontStyles.Bold) == 0);
    }

    /* ---------------- Tela final x faixa antiga ---------------- */

    private static void Disparar(FluxoDeJogoHUD hud, string handler) =>
        typeof(FluxoDeJogoHUD).GetMethod(handler, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(hud, null);

    [Test]
    public void HUD_TelaFinalVisivel_FalsoSemFimDePartida()
    {
        FluxoDeJogoHUD hud = NovoHud(out _);
        Assert.IsFalse(hud.TelaFinalVisivel);
    }

    [Test]
    public void HUD_TelaFinalVisivel_FalsoEnquantoSegura_VerdadeiroDepois()
    {
        FluxoDeJogoHUD hud = NovoHud(out _);
        hud.SegurarTelaFinal = true;
        Disparar(hud, "HandleVitoria");
        Assert.IsFalse(hud.TelaFinalVisivel, "o epílogo ainda toca");
        hud.SegurarTelaFinal = false;
        Assert.IsTrue(hud.TelaFinalVisivel);
    }

    [Test]
    public void HUD_TelaFinalVisivel_ValeTambemNaDerrota()
    {
        FluxoDeJogoHUD hud = NovoHud(out _);
        Disparar(hud, "HandleDerrota");
        Assert.IsTrue(hud.TelaFinalVisivel);
    }

    [Test]
    public void FaixaDosFocos_AlvoSomeComATelaFinal()
    {
        Assert.AreEqual(1f, DengueHUD.AlvoDaFaixaFinal(30, 30, false));
        Assert.AreEqual(0f, DengueHUD.AlvoDaFaixaFinal(30, 30, true), "tela final visível apaga a faixa");
        Assert.AreEqual(0f, DengueHUD.AlvoDaFaixaFinal(29, 30, false));
        Assert.AreEqual(0f, DengueHUD.AlvoDaFaixaFinal(0, 0, false), "sem focos não acende");
    }

    [Test]
    public void Toast_DesceEnquantoOAvisoDeMetaEstaVisivel()
    {
        Assert.AreEqual(-54f, DengueHUD.DeslocamentoDoToast(true));
        Assert.AreEqual(0f, DengueHUD.DeslocamentoDoToast(false));
    }

    /* ---------------- AvisoDeMeta ---------------- */

    [Test]
    public void Aviso_Visivel_AntesEDepoisDoTempo()
    {
        AvisoDeMeta aviso = NovoAviso(out _);
        Assert.IsFalse(aviso.Visivel);
        aviso.Mostrar(MetaDaPartida.SemRetrabalho);
        Assert.IsTrue(aviso.Visivel);
        aviso.Avancar(3.1f);
        Assert.IsFalse(aviso.Visivel);
    }

    [Test]
    public void Aviso_MostraETroca_SemFila()
    {
        AvisoDeMeta aviso = NovoAviso(out TextMeshProUGUI textoMeta);
        aviso.Mostrar(MetaDaPartida.SemRetrabalho);
        Assert.AreEqual("Sem retrabalho", textoMeta.text);
        Assert.AreEqual(3f, aviso.Restante, 1e-4f);
        aviso.Avancar(2f);
        aviso.Mostrar(MetaDaPartida.RespostaRapida);
        Assert.AreEqual("Resposta rápida", textoMeta.text);
        Assert.AreEqual(3f, aviso.Restante, 1e-4f);
    }

    [Test]
    public void Aviso_SomeDepoisDoTempo()
    {
        AvisoDeMeta aviso = NovoAviso(out _);
        aviso.Mostrar(MetaDaPartida.SemRetrabalho);
        aviso.Avancar(3.1f);
        Assert.AreEqual(0f, aviso.Restante, 1e-4f);
    }

    [Test]
    public void Aviso_NaoApareceDepoisDoFim()
    {
        AvisoDeMeta aviso = NovoAviso(out _);
        placar.Fechar(true);
        aviso.Mostrar(MetaDaPartida.SemRetrabalho);
        Assert.AreEqual(0f, aviso.Restante, 1e-4f);
    }

    [Test]
    public void Aviso_FadeAndaParaUmEDepoisParaZero()
    {
        AvisoDeMeta aviso = NovoAviso(out _);
        CanvasGroup grupo = aviso.gameObject.AddComponent<CanvasGroup>();
        TesteUtil.Setar(aviso, "grupo", grupo);
        grupo.alpha = 0f;

        aviso.Mostrar(MetaDaPartida.SemRetrabalho);
        aviso.Avancar(0.1f);
        Assert.Greater(grupo.alpha, 0f);
        aviso.Avancar(0.5f);
        Assert.AreEqual(1f, grupo.alpha, 1e-4f);
        aviso.Avancar(3f);
        aviso.Avancar(1f);
        Assert.AreEqual(0f, grupo.alpha, 1e-4f);
    }

    [Test]
    public void Aviso_RecebeOEventoDoPlacar()
    {
        AvisoDeMeta aviso = NovoAviso(out TextMeshProUGUI textoMeta);
        aviso.Ligar();
        fluxo.AvancarOcupacao(1201f);
        placar.Conferir();
        Assert.AreEqual("Resposta rápida", textoMeta.text);
        Assert.AreEqual(3f, aviso.Restante, 1e-4f);
    }

    /* ---------------- CartaoDeMetas ---------------- */

    [Test]
    public void Cartao_MostraMetaEPerdida()
    {
        CartaoDeMetas cartao = NovoCartao(out Image[] estrelasMetas, out TextMeshProUGUI[] detalhes);
        VasoInteractable vaso = NovoVasoEsvaziado();
        vaso.Reencher();
        Assert.AreEqual(1, placar.Reenchidos);

        cartao.Atualizar();

        Assert.AreEqual("perdida: 1 reenchido", detalhes[1].text);
        Assert.AreSame(vazia, estrelasMetas[1].sprite);
        Assert.AreSame(cheia, estrelasMetas[0].sprite);

        Color corCheia = new Color(0.25f, 1f, 0.8f, 1f);
        Color corPerdida = new Color(1f, 0.4f, 0.4f, 1f);
        Assert.AreEqual(corPerdida, estrelasMetas[1].color, "estrela perdida em vermelho");
        Assert.AreEqual(corCheia, estrelasMetas[0].color, "estrela viva no acento");
    }

    [Test]
    public void Cartao_LinhaDoRecorde_SoComRecorde()
    {
        CartaoDeMetas cartao = NovoCartao(out _, out _);
        GameObject linha = TesteUtil.Novo("Linha (teste)");
        TextMeshProUGUI textoRecorde = Texto();
        Image[] estrelasRecorde = Imagens(3);
        TesteUtil.Setar(cartao, "linhaDoRecorde", linha);
        TesteUtil.Setar(cartao, "textoRecorde", textoRecorde);
        SetarArray(cartao, "estrelasDoRecorde", estrelasRecorde);

        cartao.Atualizar();
        Assert.IsFalse(linha.activeSelf);

        placar.Recorde.Registrar(6280, 2);
        cartao.Atualizar();
        Assert.IsTrue(linha.activeSelf);
        Assert.AreEqual("6.280", textoRecorde.text);
        Assert.AreSame(cheia, estrelasRecorde[1].sprite);
        Assert.AreSame(vazia, estrelasRecorde[2].sprite);
    }

    [Test]
    public void Cartao_SemPlacarENulos_NaoQuebra()
    {
        // Tira o placar do SetUp: o FindAnyObjectByType do cartão não acha nada.
        Object.DestroyImmediate(placar);
        placar = null;
        Assert.IsNull(Object.FindAnyObjectByType<PlacarDaPartida>(), "sem placar na cena");

        CartaoDeMetas cartao = TesteUtil.Com<CartaoDeMetas>("Cartão vazio (teste)");
        Assert.DoesNotThrow(cartao.Atualizar);

        placar = TesteUtil.Com<PlacarDaPartida>("Placar novo (teste)");
        placar.UsarRecorde(new RecordeDoJogo(Prefixo));
        TesteUtil.Setar(cartao, "placar", placar);
        SetarArray(cartao, "estrelasDasMetas", new Object[] { null, null });
        SetarArray(cartao, "detalhesDasMetas", new Object[] { null });
        Assert.DoesNotThrow(cartao.Atualizar);
    }

    private static Color CorDoCartao(CartaoDeMetas cartao, string campo) =>
        new SerializedObject(cartao).FindProperty(campo).colorValue;

    [Test]
    public void Cartao_PartidaVencidaComMetaPerdida_UsaOResultadoFechado()
    {
        NovoVasoEsvaziado().Reencher();     // 1 reenchido: Sem retrabalho cai
        placar.Fechar(true);
        CartaoDeMetas cartao = NovoCartao(out Image[] estrelas, out TextMeshProUGUI[] detalhes);
        cartao.Atualizar();

        Color cheiaCor = CorDoCartao(cartao, "corCheia");
        Color perdida = CorDoCartao(cartao, "corPerdida");
        Color fraco = CorDoCartao(cartao, "corTextoFraco");

        Assert.AreSame(cheia, estrelas[0].sprite);
        Assert.AreEqual(cheiaCor, estrelas[0].color);
        Assert.AreEqual(fraco, detalhes[0].color);

        Assert.AreSame(vazia, estrelas[1].sprite, "Sem retrabalho perdida");
        Assert.AreEqual(perdida, estrelas[1].color);
        Assert.AreEqual(perdida, detalhes[1].color);
        Assert.AreEqual(AvaliacaoDaPartida.DetalheFinal(MetaDaPartida.SemRetrabalho, placar.Dados, placar.Regras), detalhes[1].text);

        Assert.AreSame(cheia, estrelas[2].sprite);
        Assert.AreEqual(cheiaCor, estrelas[2].color);
    }

    [Test]
    public void Cartao_PartidaPerdida_EstrelasVaziasEDetalheFraco()
    {
        placar.Fechar(false);
        CartaoDeMetas cartao = NovoCartao(out Image[] estrelas, out TextMeshProUGUI[] detalhes);
        cartao.Atualizar();

        Color vazias = CorDoCartao(cartao, "corRecordeVazia");
        Color fraco = CorDoCartao(cartao, "corTextoFraco");
        for (int i = 0; i < 3; i++)
        {
            MetaDaPartida meta = AvaliacaoDaPartida.Metas[i];
            Assert.AreSame(vazia, estrelas[i].sprite, $"estrela {i}");
            Assert.AreEqual(vazias, estrelas[i].color, $"cor da estrela {i}");
            Assert.AreEqual(fraco, detalhes[i].color, $"cor do detalhe {i}");
            Assert.AreEqual(AvaliacaoDaPartida.DetalheFinal(meta, placar.Dados, placar.Regras), detalhes[i].text, $"texto {i}");
        }
    }

    /* ---------------- RecordeNoMenu ---------------- */

    private RecordeNoMenu NovoMenu(out GameObject linha, out TextMeshProUGUI textoPontos, out Image[] estrelas)
    {
        RecordeNoMenu menu = TesteUtil.Com<RecordeNoMenu>("Menu (teste)");
        linha = TesteUtil.Novo("Linha (teste)");
        textoPontos = Texto();
        estrelas = Imagens(3);
        TesteUtil.Setar(menu, "linha", linha);
        TesteUtil.Setar(menu, "textoPontos", textoPontos);
        TesteUtil.Setar(menu, "estrelaCheia", cheia);
        TesteUtil.Setar(menu, "estrelaVazia", vazia);
        SetarArray(menu, "estrelas", estrelas);
        return menu;
    }

    [Test]
    public void RecordeNoMenu_SemRecorde_Esconde()
    {
        RecordeNoMenu menu = NovoMenu(out GameObject linha, out _, out _);
        menu.Atualizar(new RecordeDoJogo(Prefixo));
        Assert.IsFalse(linha.activeSelf);
    }

    [Test]
    public void RecordeNoMenu_ComRecorde_MostraPontosEEstrelas()
    {
        RecordeNoMenu menu = NovoMenu(out GameObject linha, out TextMeshProUGUI textoPontos, out Image[] estrelas);
        RecordeDoJogo r = new RecordeDoJogo(Prefixo);
        r.Registrar(6280, 2);
        menu.Atualizar(r);
        Assert.IsTrue(linha.activeSelf);
        Assert.AreEqual("6.280", textoPontos.text);
        Assert.AreSame(cheia, estrelas[1].sprite);
        Assert.AreSame(vazia, estrelas[2].sprite);
    }

    [Test]
    public void RecordeNoMenu_SemReferencias_NaoQuebra()
    {
        RecordeNoMenu menu = TesteUtil.Com<RecordeNoMenu>("Menu vazio (teste)");
        Assert.DoesNotThrow(() => menu.Atualizar(new RecordeDoJogo(Prefixo)));
    }
}
