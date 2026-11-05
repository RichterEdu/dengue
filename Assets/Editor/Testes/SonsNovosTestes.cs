using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

/* ============================================================
 *  SonsNovosTestes.cs
 *  ---------------------------------------------------------------------
 *  Testes EditMode (Parte B) dos sons novos e dos acabamentos que vieram
 *  com eles:
 *
 *    - importação dos WAVs novos pelo prefixo (ImportadorDeSons): os sons
 *      de mundo em mono, o chuva_loop comprimido e os curtos em PCM;
 *    - SomBuilder.LigarSonsDosFocosVivos liga os clipes em quem existir
 *      (idempotente; quem falta é ignorado);
 *    - a rampa do loop de chuva (2 s para entrar, 4 s para sair);
 *    - o esforço a cada intervaloEsforco com o pneu e o tilintar a cada
 *      passo com o saco a mais da metade (FeedbackDeCarga);
 *    - os passos pesados no lugar dos normais (SomDoJogador);
 *    - os dois param com a partida encerrada ou sem controle do agente
 *      (o FeedbackDeCarga sem fluxo nem jogador não para: é o caso dos
 *      outros testes);
 *    - o zumbido e o splash do foco que mudou de lugar saem de onde ele
 *      está agora (SomDosFocos);
 *    - o SomDoKit não toca o despejar na entrega do pneu (ela já tem
 *      pneu_picape + suspensao, da Cacamba);
 *    - o ChuvaVisual só devolve o RenderSettings se mexeu nele e se a
 *      cena dele é a ativa;
 *    - o destaque de um ponto de uso pula os renderers de focos (os pneus
 *      empilhados na carroceria);
 *    - o material do estouro de partículas é um só.
 *
 *  Em EditMode o Awake/OnEnable/Update não rodam e as fontes de áudio
 *  não existem: o que decide QUANDO tocar são métodos internos, testados
 *  aqui. O toque em si (fontes, tampa, carroceria e o RenderSettings ao
 *  trocar de cena) é conferido em Play.
 *
 *  Os objetos nascem pelo TesteUtil (escondidos, fora da cena) e são
 *  destruídos no [TearDown].
 *
 *  Rodar:  Window > General > Test Runner > EditMode (categoria ParteB)
 *  ou      unity command run_tests --mode EditMode --filter_type category --filter ParteB
 * ============================================================ */

[Category("ParteB")]
public class SonsNovosTestes
{
    private static readonly Vector3 SemChao = new Vector3(0f, -10000f, 0f);

    private readonly List<Object> temporarios = new List<Object>();

    [TearDown]
    public void Limpar()
    {
        TesteUtil.LimparTudo();
        foreach (Object o in temporarios)
            Object.DestroyImmediate(o);
        temporarios.Clear();
    }

    private static AudioClip Asset(string nome)
    {
        AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(ImportadorDeSons.Pasta + nome + ".wav");
        Assert.IsNotNull(clip, "falta o som " + nome);
        return clip;
    }

    private static AudioImporter Importador(string nome)
    {
        AudioImporter imp = AssetImporter.GetAtPath(ImportadorDeSons.Pasta + nome + ".wav") as AudioImporter;
        Assert.IsNotNull(imp, "falta o som " + nome);
        return imp;
    }

    /* A referência serializada do 'campo' (falha se o campo não existe). */
    private static Object Ref(Object alvo, string campo)
    {
        SerializedProperty p = new SerializedObject(alvo).FindProperty(campo);
        Assert.IsNotNull(p, "campo " + campo + " em " + alvo.GetType().Name);
        return p.objectReferenceValue;
    }

    private static void AssertLista(Object alvo, string campo, params string[] nomes)
    {
        SerializedProperty p = new SerializedObject(alvo).FindProperty(campo);
        Assert.IsNotNull(p, "campo " + campo + " em " + alvo.GetType().Name);
        Assert.AreEqual(nomes.Length, p.arraySize, campo);
        for (int i = 0; i < nomes.Length; i++)
            Assert.AreSame(Asset(nomes[i]), p.GetArrayElementAtIndex(i).objectReferenceValue, campo + "[" + i + "]");
    }

    private FeedbackDeCarga NovoFeedback(out KitDoAgente kit)
    {
        ControladorJogador jogador = TesteUtil.Com<ControladorJogador>("Jogador (teste)");
        jogador.transform.position = SemChao;
        kit = jogador.gameObject.AddComponent<KitDoAgente>();
        kit.PontoDeCarga = TesteUtil.Novo("Ponto de carga (teste)").transform;
        FeedbackDeCarga feedback = jogador.gameObject.AddComponent<FeedbackDeCarga>();
        TesteUtil.Setar(feedback, "kit", kit);
        return feedback;
    }

    private static PneuInteractable PneuNosBracos(KitDoAgente kit)
    {
        PneuInteractable pneu = TesteUtil.Com<PneuInteractable>("Pneu (teste)");
        pneu.transform.position = SemChao;
        Assert.IsTrue(kit.Pegar(pneu), "pneu nos braços");
        return pneu;
    }

    private static void IntervaloEsforco(FeedbackDeCarga feedback, float segundos)
    {
        SerializedObject so = new SerializedObject(feedback);
        so.FindProperty("intervaloEsforco").vector2Value = new Vector2(segundos, segundos);
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    /* ---------------- importação ---------------- */

    [Test]
    public void Importador_SonsDeMundo_EmMono()
    {
        string[] mundo = { "esvaziar", "pneu_picape", "suspensao", "lixeira_tampa", "passo_pesado_1", "passo_pesado_2", "passo_pesado_3" };
        foreach (string nome in mundo)
            Assert.IsTrue(Importador(nome).forceToMono, nome + " é tocado em 3D");
    }

    [Test]
    public void Importador_ChuvaELongaEOTrovaoEEstereo()
    {
        AudioImporter chuva = Importador("chuva_loop");
        Assert.IsFalse(chuva.forceToMono, "o loop de chuva é 2D, estéreo");
        Assert.AreEqual(AudioClipLoadType.CompressedInMemory, chuva.defaultSampleSettings.loadType);
        Assert.AreEqual(AudioCompressionFormat.Vorbis, chuva.defaultSampleSettings.compressionFormat);

        AudioImporter trovao = Importador("trovao");
        Assert.IsFalse(trovao.forceToMono, "o trovão é 2D, estéreo");
        Assert.AreEqual(AudioClipLoadType.DecompressOnLoad, trovao.defaultSampleSettings.loadType);
        Assert.AreEqual(AudioCompressionFormat.PCM, trovao.defaultSampleSettings.compressionFormat);
    }

    /* ---------------- SomBuilder ---------------- */

    [Test]
    public void LigarSons_LigaEmQuemExiste()
    {
        GameObject raiz = TesteUtil.Novo("Raiz (teste)");
        ChuvaVisual visual = raiz.AddComponent<ChuvaVisual>();
        SomDosFocos focos = raiz.AddComponent<SomDosFocos>();

        Cacamba cacamba = TesteUtil.Com<Cacamba>("Caçamba (teste)");
        cacamba.transform.SetParent(raiz.transform, false);

        // As inativas também: o builder liga antes da partida.
        Lixeira lixeiraInativa = TesteUtil.Com<Lixeira>("Lixeira (teste)");
        lixeiraInativa.transform.SetParent(raiz.transform, false);
        lixeiraInativa.gameObject.SetActive(false);
        Lixeira outraLixeira = TesteUtil.Com<Lixeira>("Lixeira 2 (teste)");

        ControladorJogador jogador = TesteUtil.Com<ControladorJogador>("Jogador (teste)");
        FeedbackDeCarga feedback = jogador.gameObject.AddComponent<FeedbackDeCarga>();
        SomDoJogador somDoJogador = jogador.gameObject.AddComponent<SomDoJogador>();

        GameObject[] raizes = { raiz, outraLixeira.gameObject, jogador.gameObject };
        SomBuilder.LigarSonsDosFocosVivos(raizes);
        SomBuilder.LigarSonsDosFocosVivos(raizes);   // idempotente

        Assert.AreSame(Asset("trovao"), Ref(visual, "trovao"));
        Assert.AreSame(Asset("chuva_loop"), Ref(visual, "chuvaLoop"));
        Assert.AreSame(Asset("esvaziar"), Ref(focos, "esvaziar"));
        Assert.AreSame(Asset("pneu_picape"), Ref(cacamba, "somPneu"));
        Assert.AreSame(Asset("suspensao"), Ref(cacamba, "somSuspensao"));
        Assert.AreSame(Asset("lixeira_tampa"), Ref(lixeiraInativa, "somTampa"));
        Assert.AreSame(Asset("lixeira_tampa"), Ref(outraLixeira, "somTampa"));
        Assert.AreSame(Asset("saco_tilintar"), Ref(feedback, "sacoTilintar"));
        AssertLista(feedback, "esforco", "esforco_1", "esforco_2", "esforco_3");
        AssertLista(somDoJogador, "passosPesados", "passo_pesado_1", "passo_pesado_2", "passo_pesado_3");
    }

    [Test]
    public void LigarSons_CenaSemNada_NaoQuebra()
    {
        Assert.DoesNotThrow(() => SomBuilder.LigarSonsDosFocosVivos(new[] { TesteUtil.Novo("Vazio (teste)") }));
    }

    /* ---------------- chuva ---------------- */

    [Test]
    public void Chuva_ComecaEmDoisSegundos()
    {
        ChuvaVisual visual = TesteUtil.Com<ChuvaVisual>("Visual (teste)");
        Assert.AreEqual(0f, visual.PesoDaChuva, "antes da chuva, mudo");

        visual.IniciarSomDaChuva();
        visual.AtualizarSom(1f);
        Assert.AreEqual(0.5f, visual.PesoDaChuva, 1e-4f, "metade do fade de 2 s");
        visual.AtualizarSom(1f);
        Assert.AreEqual(1f, visual.PesoDaChuva, 1e-4f);
    }

    [Test]
    public void Chuva_TerminaEmQuatroSegundos()
    {
        ChuvaVisual visual = TesteUtil.Com<ChuvaVisual>("Visual (teste)");
        visual.IniciarSomDaChuva();
        visual.AtualizarSom(2f);

        visual.PararSomDaChuva();
        visual.AtualizarSom(2f);
        Assert.AreEqual(0.5f, visual.PesoDaChuva, 1e-4f, "metade do fade de 4 s");
        visual.AtualizarSom(2f);
        Assert.AreEqual(0f, visual.PesoDaChuva, 1e-4f);
    }

    [Test]
    public void ChuvaVisual_SoRestauraSeAlterouNaCenaAtiva()
    {
        Scene ativa = SceneManager.GetActiveScene();

        Assert.IsTrue(ChuvaVisual.DeveRestaurar(true, ativa, ativa));
        Assert.IsFalse(ChuvaVisual.DeveRestaurar(false, ativa, ativa), "não mexeu em nada");
        Assert.IsFalse(ChuvaVisual.DeveRestaurar(true, default, ativa), "outra cena ativa (voltou ao menu)");
    }

    /* ---------------- carga ---------------- */

    [Test]
    public void Esforco_SoComPneu_ACadaIntervalo()
    {
        FeedbackDeCarga feedback = NovoFeedback(out KitDoAgente kit);
        IntervaloEsforco(feedback, 2f);

        Assert.IsFalse(feedback.AvancarEsforco(5f), "sem pneu, nada");

        PneuNosBracos(kit);
        Assert.IsFalse(feedback.AvancarEsforco(0.01f), "acabou de pegar: o relógio começa aqui");
        Assert.IsFalse(feedback.AvancarEsforco(1.9f));
        Assert.IsTrue(feedback.AvancarEsforco(0.2f), "passou o intervalo");
        Assert.IsFalse(feedback.AvancarEsforco(0.1f), "o relógio recomeçou");
        Assert.IsFalse(feedback.AvancarEsforco(1.8f));
        Assert.IsTrue(feedback.AvancarEsforco(0.2f), "de novo");
    }

    [Test]
    public void Esforco_SemPneu_RelogioZera()
    {
        FeedbackDeCarga feedback = NovoFeedback(out KitDoAgente kit);
        IntervaloEsforco(feedback, 2f);

        PneuInteractable pneu = PneuNosBracos(kit);
        feedback.AvancarEsforco(0.01f);
        feedback.AvancarEsforco(1.9f);

        Assert.IsTrue(kit.LargarPneu(SemChao, Vector3.forward));
        Assert.IsFalse(feedback.AvancarEsforco(5f), "largou: nada");

        Assert.IsTrue(kit.Pegar(pneu), "pegou de novo");
        Assert.IsFalse(feedback.AvancarEsforco(0.01f), "o relógio recomeça do zero");
        Assert.IsFalse(feedback.AvancarEsforco(1.9f), "a espera de antes não conta");
    }

    [Test]
    public void Tilintar_ACadaPasso_ComSacoAcimaDaMetade()
    {
        FeedbackDeCarga feedback = NovoFeedback(out KitDoAgente kit);
        kit.Selecionar(Ferramenta.SacoDeLixo);
        for (int i = 0; i * 2 <= kit.Estoque.CapacidadeDoSaco; i++)
            Assert.IsTrue(kit.GuardarGarrafa());

        // O pé bate no chão em π/2 + kπ: 1,57 e 4,71 aqui.
        Assert.IsFalse(feedback.PassoDoSaco(1f, true), "ainda não pisou");
        Assert.IsTrue(feedback.PassoDoSaco(2f, true), "pisou");
        Assert.IsFalse(feedback.PassoDoSaco(3f, true), "entre um passo e outro");
        Assert.IsFalse(feedback.PassoDoSaco(5f, false), "pisou, mas parado ou no ar");
        Assert.IsFalse(feedback.PassoDoSaco(7f, true), "entre um passo e outro");
        Assert.IsTrue(feedback.PassoDoSaco(8f, true), "pisou de novo");
    }

    [Test]
    public void Tilintar_SacoAteAMetade_NaoToca()
    {
        FeedbackDeCarga feedback = NovoFeedback(out KitDoAgente kit);
        for (int i = 0; (i + 1) * 2 <= kit.Estoque.CapacidadeDoSaco; i++)
            Assert.IsTrue(kit.GuardarGarrafa());

        feedback.PassoDoSaco(1f, true);
        Assert.IsFalse(feedback.PassoDoSaco(2f, true), "metade do saco ainda não tilinta");
    }

    [Test]
    public void Tilintar_ComPneu_SacoEscondidoNaoToca()
    {
        FeedbackDeCarga feedback = NovoFeedback(out KitDoAgente kit);
        while (!kit.Estoque.SacoCheio)
            Assert.IsTrue(kit.GuardarGarrafa());
        PneuNosBracos(kit);

        feedback.PassoDoSaco(1f, true);
        Assert.IsFalse(feedback.PassoDoSaco(2f, true), "as duas mãos estão no pneu");
    }

    /* Partida encerrada ou sem controle (cartelas, pausa, epílogo): o corpo
     * do agente fica quieto, mesmo com o pneu nos braços ou o saco cheio. */
    [Test]
    public void Esforco_PartidaEncerrada_NaoToca()
    {
        FeedbackDeCarga feedback = NovoFeedback(out KitDoAgente kit);
        FluxoDeJogo fluxo = TesteUtil.Com<FluxoDeJogo>("Fluxo (teste)");
        TesteUtil.Setar(feedback, "fluxo", fluxo);
        IntervaloEsforco(feedback, 2f);
        PneuNosBracos(kit);

        fluxo.DefinirEstadoParaTeste(true, false);
        feedback.AvancarEsforco(0.01f);
        Assert.IsTrue(feedback.AvancarEsforco(2.1f), "com a partida rolando, o esforço toca");

        fluxo.DefinirEstadoParaTeste(true, true);
        Assert.IsFalse(feedback.AvancarEsforco(5f), "no epílogo, não");
    }

    [Test]
    public void Esforco_SemControle_NaoToca_EORelogioRecomeca()
    {
        FeedbackDeCarga feedback = NovoFeedback(out KitDoAgente kit);
        ControladorJogador jogador = TesteUtil.Com<ControladorJogador>("Jogador (teste)");
        TesteUtil.Setar(feedback, "jogador", jogador);
        IntervaloEsforco(feedback, 2f);
        PneuNosBracos(kit);

        feedback.AvancarEsforco(0.01f);
        feedback.AvancarEsforco(1.9f);

        jogador.ControleAtivo = false;
        Assert.IsFalse(feedback.AvancarEsforco(5f), "sem controle, nada");

        jogador.ControleAtivo = true;
        Assert.IsFalse(feedback.AvancarEsforco(0.01f), "o relógio recomeça do zero");
        Assert.IsFalse(feedback.AvancarEsforco(1.9f), "a espera de antes não conta");
        Assert.IsTrue(feedback.AvancarEsforco(0.2f));
    }

    [Test]
    public void Tilintar_PartidaEncerrada_NaoToca()
    {
        FeedbackDeCarga feedback = NovoFeedback(out KitDoAgente kit);
        kit.Selecionar(Ferramenta.SacoDeLixo);
        FluxoDeJogo fluxo = TesteUtil.Com<FluxoDeJogo>("Fluxo (teste)");
        TesteUtil.Setar(feedback, "fluxo", fluxo);
        while (!kit.Estoque.SacoCheio)
            Assert.IsTrue(kit.GuardarGarrafa());

        fluxo.DefinirEstadoParaTeste(true, false);
        feedback.PassoDoSaco(1f, true);
        Assert.IsTrue(feedback.PassoDoSaco(2f, true), "com a partida rolando, tilinta");

        fluxo.DefinirEstadoParaTeste(true, true);
        Assert.IsFalse(feedback.PassoDoSaco(5f, true), "no epílogo, não");
    }

    [Test]
    public void Tilintar_SemControle_NaoToca_ENaoAcumulaOPasso()
    {
        FeedbackDeCarga feedback = NovoFeedback(out KitDoAgente kit);
        ControladorJogador jogador = TesteUtil.Com<ControladorJogador>("Jogador (teste)");
        TesteUtil.Setar(feedback, "jogador", jogador);
        while (!kit.Estoque.SacoCheio)
            Assert.IsTrue(kit.GuardarGarrafa());

        feedback.PassoDoSaco(1f, true);
        jogador.ControleAtivo = false;
        Assert.IsFalse(feedback.PassoDoSaco(2f, true), "sem controle, nada");

        jogador.ControleAtivo = true;
        Assert.IsFalse(feedback.PassoDoSaco(3f, true), "o passo sem controle não sai depois");
    }

    /* ---------------- o foco que muda de lugar (SomDosFocos) ---------------- */

    /* O som dos focos ligado a um gerenciador com este foco só. */
    private static SomDosFocos NovoSomDosFocos(Interactable foco, out GerenciadorDeFocos gerenciador)
    {
        gerenciador = TesteUtil.Com<GerenciadorDeFocos>("Gerenciador (teste)");
        gerenciador.Registrar(new[] { foco });

        SomDosFocos som = TesteUtil.Com<SomDosFocos>("Sons (teste)");
        TesteUtil.Setar(som, "gerenciador", gerenciador);
        TesteUtil.Setar(som, "zumbido", Asset("mosquito_zumbido_loop"));
        TesteUtil.Setar(som, "esvaziar", Asset("esvaziar"));
        return som;
    }

    /* Os passos das Mãos até a água sair (o kit começa nelas). */
    private static void EsvaziarComAsMaos(Interactable foco, KitDoAgente kit)
    {
        for (int i = 0; i < 20 && !foco.Esvaziado; i++)
            foco.AvancarCom(kit);
        Assert.IsTrue(foco.Esvaziado, "o foco tinha que ficar esvaziado");
    }

    /* Pneu largado longe de onde estava e reenchido pela chuva: o zumbido
     * vai para o lugar novo (a fonte fica parada no mundo). */
    [Test]
    public void Zumbido_PneuReenchidoEmOutroLugar_ZumbeNoLugarNovo()
    {
        KitDoAgente kit = TesteUtil.Com<KitDoAgente>("Kit (teste)");
        PneuInteractable pneu = TesteUtil.Com<PneuInteractable>("Pneu (teste)");
        pneu.transform.position = SemChao;
        SomDosFocos som = NovoSomDosFocos(pneu, out GerenciadorDeFocos gerenciador);
        som.Ligar();
        Transform canal = som.transform.Find("Zumbidos/Zumbido " + pneu.name);
        Assert.IsNotNull(canal, "o canal do zumbido do pneu");
        Assert.AreEqual(SemChao, canal.position, "nasce no foco");

        EsvaziarComAsMaos(pneu, kit);
        Vector3 lugarNovo = SemChao + Vector3.right * 60f;
        pneu.transform.position = lugarNovo;
        Assert.AreEqual(1, gerenciador.ReencherEsvaziados());

        Assert.AreEqual(lugarNovo, canal.position, "zumbe onde o pneu está agora");
    }

    /* Limpar um foco cala o zumbido de todos por um tempo: num monte de
     * pneus, o vizinho entraria na hora e pareceria o foco limpo zumbindo. */
    [Test]
    public void Zumbido_FocoEsvaziado_SilenciaOsZumbidos()
    {
        KitDoAgente kit = TesteUtil.Com<KitDoAgente>("Kit (teste)");
        PneuInteractable pneu = TesteUtil.Com<PneuInteractable>("Pneu (teste)");
        pneu.transform.position = SemChao;
        SomDosFocos som = NovoSomDosFocos(pneu, out _);
        som.Ligar();
        Assert.IsFalse(som.ZumbidosEmSilencio, "antes de limpar, pode zumbir");

        EsvaziarComAsMaos(pneu, kit);

        Assert.IsTrue(som.ZumbidosEmSilencio, "logo depois de limpar, silêncio");
    }

    /* O splash do esvaziar sai de onde o foco está agora: o pool só alcança
     * 30 m, e o pneu pode ter sido levado para longe. */
    [Test]
    public void Esvaziar_TocaNoLugarAtualDoFoco()
    {
        KitDoAgente kit = TesteUtil.Com<KitDoAgente>("Kit (teste)");
        PneuInteractable pneu = TesteUtil.Com<PneuInteractable>("Pneu (teste)");
        pneu.transform.position = SemChao;
        SomDosFocos som = NovoSomDosFocos(pneu, out _);
        som.Ligar();

        Vector3 lugarNovo = SemChao + Vector3.right * 60f;
        pneu.transform.position = lugarNovo;
        EsvaziarComAsMaos(pneu, kit);

        Assert.AreEqual(lugarNovo, som.transform.Find("Clique 1").position, "o som sai de onde o pneu está agora");
    }

    [Test]
    public void PassoPesado_TrocaALista()
    {
        ControladorJogador jogador = TesteUtil.Com<ControladorJogador>("Jogador (teste)");
        SomDoJogador som = jogador.gameObject.AddComponent<SomDoJogador>();
        SomBuilder.LigarSonsDosFocosVivos(new[] { jogador.gameObject });
        AudioClip rua = Asset("passo_rua_1");
        SerializedObject so = new SerializedObject(som);
        SerializedProperty passosRua = so.FindProperty("passosRua");
        passosRua.arraySize = 1;
        passosRua.GetArrayElementAtIndex(0).objectReferenceValue = rua;
        so.ApplyModifiedPropertiesWithoutUndo();

        Assert.AreSame(rua, som.ListaDePassos()[0], "sem peso, o passo normal");

        som.PassoPesado = true;
        Assert.AreSame(Asset("passo_pesado_1"), som.ListaDePassos()[0], "com o pneu, o pesado");
    }

    [Test]
    public void PassoPesado_SemClip_FicaNoNormal()
    {
        ControladorJogador jogador = TesteUtil.Com<ControladorJogador>("Jogador (teste)");
        SomDoJogador som = jogador.gameObject.AddComponent<SomDoJogador>();
        AudioClip rua = Asset("passo_rua_1");
        SerializedObject so = new SerializedObject(som);
        SerializedProperty passosRua = so.FindProperty("passosRua");
        passosRua.arraySize = 1;
        passosRua.GetArrayElementAtIndex(0).objectReferenceValue = rua;
        so.ApplyModifiedPropertiesWithoutUndo();

        som.PassoPesado = true;
        Assert.AreSame(rua, som.ListaDePassos()[0]);
    }

    /* ---------------- entrega sem som duplicado ---------------- */

    [Test]
    public void SomDoKit_NaoOuveAEntregaDoPneu()
    {
        KitDoAgente kit = TesteUtil.Com<KitDoAgente>("Kit (teste)");
        SomDoKit som = kit.gameObject.AddComponent<SomDoKit>();
        BindingFlags privado = BindingFlags.Instance | BindingFlags.NonPublic;

        typeof(SomDoKit).GetMethod("OnEnable", privado).Invoke(som, null);
        Delegate saco = (Delegate)typeof(KitDoAgente).GetField("OnSacoEsvaziado", privado).GetValue(kit);
        Delegate entregue = (Delegate)typeof(KitDoAgente).GetField("OnPneuEntregue", privado).GetValue(kit);
        typeof(SomDoKit).GetMethod("OnDisable", privado).Invoke(som, null);

        Assert.IsNotNull(saco, "o saco continua com o despejar");
        Assert.IsNull(entregue, "a entrega do pneu toca pneu_picape + suspensao, na Cacamba");
    }

    /* ---------------- destaque e partículas ---------------- */

    private MeshRenderer NovoRenderer(string nome, Transform pai)
    {
        MeshRenderer r = TesteUtil.Com<MeshRenderer>(nome);
        r.transform.SetParent(pai, false);
        Material mat = new Material(Shader.Find("Sprites/Default"));
        temporarios.Add(mat);
        r.sharedMaterial = mat;
        return r;
    }

    [Test]
    public void Destaque_DaPicape_PulaOsPneusEmpilhados()
    {
        PontoDeReabastecimento picape = TesteUtil.Com<PontoDeReabastecimento>("Picape (teste)");
        MeshRenderer lataria = NovoRenderer("Lataria (teste)", picape.transform);
        PneuInteractable pneu = TesteUtil.Com<PneuInteractable>("Pneu empilhado (teste)");
        pneu.transform.SetParent(picape.transform, false);
        MeshRenderer borracha = NovoRenderer("Borracha (teste)", pneu.transform);

        Assert.IsTrue(Interactor.EntraNoDestaque(picape, lataria), "a picape brilha");
        Assert.IsFalse(Interactor.EntraNoDestaque(picape, borracha), "o pneu na carroceria, não");
        Assert.IsTrue(Interactor.EntraNoDestaque(pneu, borracha), "mirado, o foco brilha");
    }

    [Test]
    public void Estouro_MaterialCompartilhadoComTextura()
    {
        Material material = Interactable.MaterialDoEstouro();

        Assert.IsNotNull(material);
        Assert.AreSame(material, Interactable.MaterialDoEstouro(), "um material só para todos os estouros");
        Assert.IsNotNull(material.mainTexture, "textura gerada em código");
    }
}
