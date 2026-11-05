using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

/* ============================================================
 *  FeedbackDeCargaTestes.cs
 *  ---------------------------------------------------------------------
 *  Testes EditMode (Parte B) do peso de carregar (FeedbackDeCarga): com
 *  um pneu nos braços, a câmera entra no ModoPesado (balanço mais forte
 *  e passo mais longo) e os passos ficam pesados (PassoPesado); largar
 *  devolve os dois ao normal. O saco de lixo na mão aparece com alguma
 *  garrafa e cresce com a quantidade (escala entre escalaSaco.x e .y,
 *  pela fração do saco cheio). Com o pneu nos braços ele some (as duas
 *  mãos estão no pneu) e volta quando o pneu sai. O braço direito sobe
 *  para o saco pela camada "CarregandoSaco", só com garrafas e sem pneu.
 *
 *  O Animator do corpo pode ainda não estar pronto quando o Registrar
 *  roda (ordem de carga da cena): o feedback lê o parâmetro e a camada
 *  depois, leva o Animator ao estado do kit e só avisa, uma vez, quando
 *  o controller realmente não tem o que a pose precisa. Um Animator em
 *  objeto inativo faz esse papel aqui (isInitialized fica false até o
 *  objeto ser ativado). O controller do projeto também é conferido: as
 *  duas camadas de carga nascem com peso 0 no asset, para uma cena sem o
 *  FeedbackDeCarga não ficar com os braços congelados.
 *
 *  O Awake/OnEnable não rodam no EditMode: as referências do feedback
 *  são ligadas pelo TesteUtil.Setar (o animator fica vazio, menos nos
 *  testes do Animator) e o Registrar() assina o OnMudou do kit, como o
 *  OnEnable faz no jogo.
 *
 *  O kit, o SomDoJogador e o feedback ficam no mesmo objeto (como no
 *  Player), e a câmera num objeto à parte. O pneu fica em SemChao, bem
 *  abaixo do mapa, onde os raycasts de pegar e largar não acham nada da
 *  cena aberta no Editor.
 *
 *  Os objetos nascem pelo TesteUtil (escondidos, fora da cena) e são
 *  destruídos no [TearDown], com os controllers em memória (nada vira
 *  asset).
 *
 *  Rodar:  Window > General > Test Runner > EditMode (categoria ParteB)
 *  ou      unity command run_tests --mode EditMode --filter_type category --filter ParteB
 * ============================================================ */

[Category("ParteB")]
public class FeedbackDeCargaTestes
{
    // Bem abaixo de qualquer cena do jogo: aqui o raycast não acha chão.
    private static readonly Vector3 SemChao = new Vector3(0f, -10000f, 0f);

    private KitDoAgente kit;
    private CameraJogador cameraJogador;
    private SomDoJogador somDoJogador;
    private FeedbackDeCarga feedback;
    private Transform saco;
    private PneuInteractable pneu;
    private readonly List<AnimatorController> controllers = new List<AnimatorController>();
    private readonly List<string> avisos = new List<string>();

    [SetUp]
    public void Preparar()
    {
        Application.logMessageReceived += CapturarAviso;

        ControladorJogador jogador = TesteUtil.Com<ControladorJogador>("Jogador (teste)");
        jogador.transform.position = SemChao;
        kit = jogador.gameObject.AddComponent<KitDoAgente>();
        somDoJogador = jogador.gameObject.AddComponent<SomDoJogador>();
        feedback = jogador.gameObject.AddComponent<FeedbackDeCarga>();

        cameraJogador = TesteUtil.Com<CameraJogador>("Câmera (teste)");
        saco = TesteUtil.Novo("Saco na mão (teste)").transform;

        TesteUtil.Setar(feedback, "kit", kit);
        TesteUtil.Setar(feedback, "cameraJogador", cameraJogador);
        TesteUtil.Setar(feedback, "somDoJogador", somDoJogador);
        TesteUtil.Setar(feedback, "sacoNaMao", saco);
        kit.Selecionar(Ferramenta.SacoDeLixo);
        feedback.Registrar();

        pneu = TesteUtil.Com<PneuInteractable>("Pneu (teste)");
        pneu.transform.position = SemChao + Vector3.forward * 2f;
    }

    [TearDown]
    public void Limpar()
    {
        Application.logMessageReceived -= CapturarAviso;
        avisos.Clear();

        TesteUtil.LimparTudo();
        foreach (AnimatorController controller in controllers)
            Object.DestroyImmediate(controller);
        controllers.Clear();
    }

    /* Guarda os avisos sobre "Carregando", tanto os do feedback quanto os do
     * próprio Animator (SetBool de parâmetro que não existe). */
    private void CapturarAviso(string mensagem, string pilha, LogType tipo)
    {
        if (tipo == LogType.Warning && mensagem.Contains("Carregando"))
            avisos.Add(mensagem);
    }

    /* Animator com um controller em memória, ligado ao feedback e ainda sem
     * Registrar. Inativo (ativo = false), ele fica sem inicializar, como o
     * do corpo durante a carga da cena. As camadas nascem com pesoNoAsset (0,
     * como no Agente_AnimController). comCamada traz as duas camadas de carga;
     * semCamadaDoSaco tira só a do saco. */
    private Animator NovoAnimator(bool comParametro, bool comCamada, bool ativo,
                                  float pesoNoAsset = 0f, bool semCamadaDoSaco = false)
    {
        AnimatorController controller = new AnimatorController { hideFlags = HideFlags.HideAndDontSave };
        controllers.Add(controller);
        controller.AddLayer("Base Layer");
        if (comParametro)
            controller.AddParameter("Carregando", AnimatorControllerParameterType.Bool);
        if (comCamada)
        {
            controller.AddLayer("Carregando");
            if (!semCamadaDoSaco)
                controller.AddLayer("CarregandoSaco");
            AnimatorControllerLayer[] camadas = controller.layers;
            for (int i = 1; i < camadas.Length; i++)
                camadas[i].defaultWeight = pesoNoAsset;
            controller.layers = camadas;
        }

        GameObject corpo = TesteUtil.Novo("Corpo (teste)");
        corpo.SetActive(ativo);
        Animator animator = corpo.AddComponent<Animator>();
        animator.runtimeAnimatorController = controller;
        TesteUtil.Setar(feedback, "animator", animator);
        return animator;
    }

    private static float PesoDaCamada(Animator animator, string camada = "Carregando")
    {
        return animator.GetLayerWeight(animator.GetLayerIndex(camada));
    }

    [Test]
    public void Pegar_LigaModoPesado()
    {
        Assert.IsFalse(cameraJogador.ModoPesado, "sem pneu a câmera começa normal");
        Assert.IsFalse(somDoJogador.PassoPesado, "sem pneu os passos começam normais");

        Assert.IsTrue(kit.Pegar(pneu));

        Assert.IsTrue(cameraJogador.ModoPesado, "com o pneu nos braços a câmera fica pesada");
        Assert.IsTrue(somDoJogador.PassoPesado, "com o pneu nos braços os passos ficam pesados");
    }

    [Test]
    public void Largar_DesligaModoPesado()
    {
        Assert.IsTrue(kit.Pegar(pneu));
        Assert.IsTrue(kit.LargarPneu(SemChao, Vector3.forward));

        Assert.IsFalse(cameraJogador.ModoPesado, "largado o pneu, a câmera volta ao normal");
        Assert.IsFalse(somDoJogador.PassoPesado, "largado o pneu, os passos voltam ao normal");
    }

    [Test]
    public void ModoPesado_EntraESaiSuave()
    {
        Assert.AreEqual(0f, cameraJogador.PesoPesado, 1e-4f);

        Assert.IsTrue(kit.Pegar(pneu));
        Assert.AreEqual(0f, cameraJogador.PesoPesado, 1e-4f, "o peso não pula junto com o pneu");
        cameraJogador.AtualizarPesoPesado(0.125f);
        Assert.AreEqual(0.5f, cameraJogador.PesoPesado, 1e-4f, "em 1/8 s: meio caminho");
        cameraJogador.AtualizarPesoPesado(1f);
        Assert.AreEqual(1f, cameraJogador.PesoPesado, 1e-4f);

        Assert.IsTrue(kit.LargarPneu(SemChao, Vector3.forward));
        cameraJogador.AtualizarPesoPesado(0.125f);
        Assert.AreEqual(0.5f, cameraJogador.PesoPesado, 1e-4f, "a saída também é suave");
        cameraJogador.AtualizarPesoPesado(1f);
        Assert.AreEqual(0f, cameraJogador.PesoPesado, 1e-4f);
    }

    [Test]
    public void Saco_EscalaPelasGarrafas()
    {
        Assert.AreEqual(6, kit.Estoque.CapacidadeDoSaco, "capacidade padrão do saco");
        Assert.IsFalse(saco.gameObject.activeSelf, "saco vazio: nada na mão");

        for (int i = 0; i < 3; i++)
            Assert.IsTrue(kit.GuardarGarrafa());

        Assert.IsTrue(saco.gameObject.activeSelf, "com garrafas o saco aparece na mão");
        Assert.AreEqual(0.75f, saco.localScale.x, 1e-4f, "3 de 6: meio caminho entre 0,5 e 1,0");
        Assert.AreEqual(0.75f, saco.localScale.y, 1e-4f);
        Assert.AreEqual(0.75f, saco.localScale.z, 1e-4f);

        kit.EsvaziarSaco();

        Assert.IsFalse(saco.gameObject.activeSelf, "esvaziado na lixeira, o saco some da mão");
    }

    [Test]
    public void TrocarDeFerramenta_GuardaOSaco_EVoltarMostra()
    {
        Assert.IsTrue(kit.GuardarGarrafa());
        Assert.IsTrue(saco.gameObject.activeSelf, "saco selecionado e com garrafa: à vista");

        Assert.IsTrue(kit.Selecionar(Ferramenta.Areia));
        Assert.IsFalse(saco.gameObject.activeSelf, "outra ferramenta: o saco é guardado");
        Assert.AreEqual(1, kit.Estoque.GarrafasNoSaco, "a garrafa continua no saco");

        Assert.IsTrue(kit.Selecionar(Ferramenta.SacoDeLixo));
        Assert.IsTrue(saco.gameObject.activeSelf, "selecionado de novo: volta à mão");
    }

    [Test]
    public void Pneu_EscondeOSaco()
    {
        for (int i = 0; i < 3; i++)
            Assert.IsTrue(kit.GuardarGarrafa());
        Assert.IsTrue(saco.gameObject.activeSelf);

        Assert.IsTrue(kit.Pegar(pneu));
        Assert.IsFalse(saco.gameObject.activeSelf, "as duas mãos estão no pneu: o saco não aparece");

        Assert.IsTrue(kit.LargarPneu(SemChao, Vector3.forward));
        Assert.IsTrue(saco.gameObject.activeSelf, "largado o pneu, o saco volta à mão");
        Assert.AreEqual(0.75f, saco.localScale.x, 1e-4f, "com o tamanho das garrafas que tinha");
    }

    [Test]
    public void Pneu_ComGarrafasGuardadasNoCaminho_SacoSegueEscondido()
    {
        Assert.IsTrue(kit.Pegar(pneu));
        Assert.IsTrue(kit.GuardarGarrafa());

        Assert.IsFalse(saco.gameObject.activeSelf, "o OnMudou da garrafa não traz o saco de volta");
    }

    [Test]
    public void EntregarPneu_DevolveOSaco()
    {
        Cacamba cacamba = TesteUtil.Com<Cacamba>("Caçamba (teste)");
        Assert.IsTrue(kit.GuardarGarrafa());
        Assert.IsTrue(kit.Pegar(pneu));
        Assert.IsFalse(saco.gameObject.activeSelf);

        Assert.IsTrue(kit.EntregarPneu(cacamba));

        Assert.IsTrue(saco.gameObject.activeSelf, "entregue o pneu, o saco volta à mão");
    }

    [Test]
    public void BracoDoSaco_SoComGarrafasESemPneu()
    {
        Animator animator = NovoAnimator(comParametro: true, comCamada: true, ativo: true);
        feedback.Registrar();
        Assert.AreEqual(0f, PesoDaCamada(animator, "CarregandoSaco"), 1e-4f, "sem garrafas o braço fica na locomoção");

        Assert.IsTrue(kit.GuardarGarrafa());
        feedback.AtualizarPesoDosBracos(0.075f);
        Assert.AreEqual(0.5f, PesoDaCamada(animator, "CarregandoSaco"), 1e-4f, "metade dos 0,15 s: meio caminho");
        feedback.AtualizarPesoDosBracos(1f);
        Assert.AreEqual(1f, PesoDaCamada(animator, "CarregandoSaco"), 1e-4f, "com garrafas o braço sobe");

        Assert.IsTrue(kit.Pegar(pneu));
        feedback.AtualizarPesoDosBracos(1f);
        Assert.AreEqual(0f, PesoDaCamada(animator, "CarregandoSaco"), 1e-4f, "com o pneu, a pose é a dos dois braços");
        Assert.AreEqual(1f, PesoDaCamada(animator), 1e-4f);

        Assert.IsTrue(kit.LargarPneu(SemChao, Vector3.forward));
        feedback.AtualizarPesoDosBracos(1f);
        Assert.AreEqual(1f, PesoDaCamada(animator, "CarregandoSaco"), 1e-4f, "largado o pneu, o braço volta ao saco");
        Assert.AreEqual(0f, PesoDaCamada(animator), 1e-4f);

        kit.EsvaziarSaco();
        feedback.AtualizarPesoDosBracos(1f);
        Assert.AreEqual(0f, PesoDaCamada(animator, "CarregandoSaco"), 1e-4f, "saco vazio: o braço desce");
        Assert.IsEmpty(avisos);
    }

    [Test]
    public void AnimatorAindaNaoPronto_SacoChegaQuandoEleFicarPronto()
    {
        Animator animator = NovoAnimator(comParametro: true, comCamada: true, ativo: false);
        feedback.Registrar();
        Assert.IsTrue(kit.GuardarGarrafa());

        animator.gameObject.SetActive(true);
        feedback.AtualizarPesoDosBracos(0f);

        Assert.AreEqual(1f, PesoDaCamada(animator, "CarregandoSaco"), 1e-4f, "as garrafas de antes chegam ao Animator");
        Assert.IsEmpty(avisos);
    }

    [Test]
    public void ControllerSemCamadaDoSaco_AvisaUmaVezSo()
    {
        NovoAnimator(comParametro: true, comCamada: true, ativo: true, semCamadaDoSaco: true);

        feedback.Registrar();
        Assert.IsTrue(kit.GuardarGarrafa());
        feedback.AtualizarPesoDosBracos(0.016f);

        Assert.AreEqual(1, avisos.Count, "um aviso só: " + string.Join(" | ", avisos));
        StringAssert.Contains("CarregandoSaco", avisos[0]);
    }

    [Test]
    public void ControllerDoProjeto_CamadasDeCargaNascemComPesoZero()
    {
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(PoseDeCarga.CaminhoController);
        Assert.IsNotNull(controller);

        AnimatorControllerLayer pneuNosBracos = controller.layers.Single(l => l.name == PoseDeCarga.NomeCamada);
        AnimatorControllerLayer sacoNaMao = controller.layers.Single(l => l.name == PoseDeCarga.NomeCamadaSaco);

        Assert.AreEqual(0f, pneuNosBracos.defaultWeight, "sem o FeedbackDeCarga, os braços não congelam");
        Assert.AreEqual(0f, sacoNaMao.defaultWeight, "sem o FeedbackDeCarga, o braço não fica erguido");
        Assert.AreEqual(PoseDeCarga.NomeClipeSaco, sacoNaMao.stateMachine.defaultState.motion.name);
    }

    [Test]
    public void MascaraDoSaco_SoOBracoDireito()
    {
        var mascara = AssetDatabase.LoadAssetAtPath<AvatarMask>(PoseDeCarga.CaminhoMascaraSaco);
        Assert.IsNotNull(mascara);

        var ativos = new List<string>();
        for (int i = 0; i < mascara.transformCount; i++)
            if (mascara.GetTransformActive(i))
            {
                string caminho = mascara.GetTransformPath(i);
                ativos.Add(caminho.Substring(caminho.LastIndexOf('/') + 1));
            }

        CollectionAssert.AreEquivalent(new[] { "RightUpperArm", "RightLowerArm", "RightHand" }, ativos);
    }

    [Test]
    public void AnimatorAindaNaoPronto_PneuPegoChegaQuandoEleFicarPronto()
    {
        Animator animator = NovoAnimator(comParametro: true, comCamada: true, ativo: false);
        feedback.Registrar();
        Assert.IsFalse(animator.isInitialized, "objeto inativo: o Animator ainda não inicializou");

        Assert.IsTrue(kit.Pegar(pneu));
        feedback.AtualizarPesoDosBracos(0.016f);   // um frame com o Animator ainda fora do ar

        animator.gameObject.SetActive(true);
        Assert.IsTrue(animator.isInitialized, "objeto ativo: o Animator ficou pronto");
        feedback.AtualizarPesoDosBracos(0f);

        Assert.IsTrue(animator.GetBool("Carregando"), "o pneu pego antes do Animator chega a ele");
        Assert.AreEqual(1f, PesoDaCamada(animator), 1e-4f, "os braços vão direto ao pneu");
        Assert.IsEmpty(avisos, "Animator atrasado não é controller incompleto");
    }

    [Test]
    public void AnimatorAindaNaoPronto_SemPneuTiraACamadaDoCaminho()
    {
        // Um controller com a camada em peso 1 (o asset do projeto nasce em 0).
        Animator animator = NovoAnimator(comParametro: true, comCamada: true, ativo: false, pesoNoAsset: 1f);
        feedback.Registrar();

        animator.gameObject.SetActive(true);
        Assert.AreEqual(1f, PesoDaCamada(animator), 1e-4f, "a camada veio com peso 1 do controller");
        feedback.AtualizarPesoDosBracos(0f);

        Assert.AreEqual(0f, PesoDaCamada(animator), 1e-4f, "sem pneu, a locomoção passa inteira");
        Assert.IsEmpty(avisos);
    }

    [Test]
    public void AnimatorAindaNaoPronto_DetectaAoMudarOKit()
    {
        Animator animator = NovoAnimator(comParametro: true, comCamada: true, ativo: false, pesoNoAsset: 1f);
        feedback.Registrar();
        Assert.IsTrue(kit.Pegar(pneu));

        animator.gameObject.SetActive(true);
        Assert.IsTrue(kit.LargarPneu(SemChao, Vector3.forward), "o OnMudou chega antes do primeiro Update");

        Assert.IsFalse(animator.GetBool("Carregando"));
        Assert.AreEqual(0f, PesoDaCamada(animator), 1e-4f, "o kit mudou: já leva o Animator ao estado dele");
    }

    [Test]
    public void AnimatorPronto_PesoSegueOPneu()
    {
        Animator animator = NovoAnimator(comParametro: true, comCamada: true, ativo: true);
        feedback.Registrar();
        Assert.AreEqual(0f, PesoDaCamada(animator), 1e-4f, "ao assinar, vai direto ao estado do kit");

        Assert.IsTrue(kit.Pegar(pneu));
        Assert.IsTrue(animator.GetBool("Carregando"));
        feedback.AtualizarPesoDosBracos(0.075f);
        Assert.AreEqual(0.5f, PesoDaCamada(animator), 1e-4f, "metade dos 0,15 s: meio caminho");
        feedback.AtualizarPesoDosBracos(1f);
        Assert.AreEqual(1f, PesoDaCamada(animator), 1e-4f);

        Assert.IsTrue(kit.LargarPneu(SemChao, Vector3.forward));
        Assert.IsFalse(animator.GetBool("Carregando"));
        feedback.AtualizarPesoDosBracos(1f);
        Assert.AreEqual(0f, PesoDaCamada(animator), 1e-4f);
        Assert.IsEmpty(avisos);
    }

    [Test]
    public void ControllerSemParametroNemCamada_AvisaUmaVezSo()
    {
        NovoAnimator(comParametro: false, comCamada: false, ativo: true);

        feedback.Registrar();
        Assert.IsTrue(kit.Pegar(pneu));
        feedback.AtualizarPesoDosBracos(0.016f);
        feedback.AtualizarPesoDosBracos(0.016f);
        Assert.IsTrue(kit.LargarPneu(SemChao, Vector3.forward));
        feedback.Registrar();                      // como um OnEnable de novo

        Assert.AreEqual(1, avisos.Count, "um aviso só: " + string.Join(" | ", avisos));
        StringAssert.Contains("FeedbackDeCarga", avisos[0]);
    }

    [Test]
    public void ControllerSemCamada_AvisaEOBoolContinua()
    {
        Animator animator = NovoAnimator(comParametro: true, comCamada: false, ativo: true);

        feedback.Registrar();
        Assert.IsTrue(kit.Pegar(pneu));

        Assert.IsTrue(animator.GetBool("Carregando"), "o parâmetro existe e continua ligado");
        Assert.AreEqual(1, avisos.Count, "um aviso só: " + string.Join(" | ", avisos));
    }

    [Test]
    public void AnimatorSemController_AvisaUmaVezSo()
    {
        Animator animator = TesteUtil.Com<Animator>("Corpo sem controller (teste)");
        TesteUtil.Setar(feedback, "animator", animator);

        feedback.Registrar();
        Assert.IsTrue(kit.Pegar(pneu));
        feedback.AtualizarPesoDosBracos(0.016f);

        Assert.AreEqual(1, avisos.Count, "um aviso só: " + string.Join(" | ", avisos));
    }
}
