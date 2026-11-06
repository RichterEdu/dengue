using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor.Animations;
using UnityEngine;

/* ============================================================
 *  ItensNaMaoTestes.cs
 *  ---------------------------------------------------------------------
 *  Testes EditMode do componente ItensNaMao: o objeto certo na mão para
 *  cada ferramenta e carga, as partículas do gesto, o peso da camada do
 *  Animator, o estado pedido a ela e o giro do ombro com o olhar.
 *
 *  O Awake/OnEnable não rodam no EditMode: as referências são ligadas
 *  pelo TesteUtil.Setar e o Registrar() assina o OnMudou do kit, como o
 *  OnEnable faz no jogo. O gesto entra direto no AtualizarGesto (quando
 *  ele toca é a regra pura, nos RegraDoItemNaMaoTestes).
 *
 *  Rodar:  Window > General > Test Runner > EditMode (categoria ItensNaMao)
 *  ou      unity command run_tests --mode EditMode --filter_type category --filter ItensNaMao
 * ============================================================ */

[Category("ItensNaMao")]
public class ItensNaMaoTestes
{
    private const float Duracao = 0.15f;

    private ControladorJogador jogador;
    private KitDoAgente kit;
    private ItensNaMao itens;
    private GameObject balde;
    private GameObject areia;
    private GameObject tela;
    private GameObject borrifador;
    private ParticleSystem areiaCaindo;
    private ParticleSystem nevoa;
    private Transform ombro;
    private readonly List<AnimatorController> controllers = new List<AnimatorController>();
    private readonly List<string> avisos = new List<string>();

    [SetUp]
    public void Preparar()
    {
        Application.logMessageReceived += CapturarAviso;

        jogador = TesteUtil.Com<ControladorJogador>("Jogador (teste)");
        kit = jogador.gameObject.AddComponent<KitDoAgente>();
        itens = jogador.gameObject.AddComponent<ItensNaMao>();

        balde = TesteUtil.Novo("Balde (teste)");
        areia = TesteUtil.Novo("Areia (teste)");
        areia.transform.SetParent(balde.transform, false);
        tela = TesteUtil.Novo("Tela (teste)");
        borrifador = TesteUtil.Novo("Borrifador (teste)");
        areiaCaindo = TesteUtil.Com<ParticleSystem>("Areia caindo (teste)");
        nevoa = TesteUtil.Com<ParticleSystem>("Névoa (teste)");
        ombro = TesteUtil.Novo("Ombro (teste)").transform;

        TesteUtil.Setar(itens, "kit", kit);
        TesteUtil.Setar(itens, "jogador", jogador);
        TesteUtil.Setar(itens, "baldeNaMao", balde.transform);
        TesteUtil.Setar(itens, "areiaDoBalde", areia);
        TesteUtil.Setar(itens, "telaNaMao", tela.transform);
        TesteUtil.Setar(itens, "borrifadorNaMao", borrifador.transform);
        TesteUtil.Setar(itens, "areiaCaindo", areiaCaindo);
        TesteUtil.Setar(itens, "nevoa", nevoa);
        TesteUtil.Setar(itens, "ombroDireito", ombro);
        itens.Registrar();
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

    private void CapturarAviso(string mensagem, string pilha, LogType tipo)
    {
        if (tipo == LogType.Warning && mensagem.Contains(ItensNaMao.NomeCamada))
            avisos.Add(mensagem);
    }

    /* Animator com controller em memória. Inativo, ele fica sem inicializar,
     * como o do corpo durante a carga da cena. */
    private Animator NovoAnimator(bool comCamada, bool ativo)
    {
        AnimatorController controller = new AnimatorController { hideFlags = HideFlags.HideAndDontSave };
        controllers.Add(controller);
        controller.AddLayer("Base Layer");
        if (comCamada)
        {
            controller.AddLayer(ItensNaMao.NomeCamada);
            AnimatorStateMachine maquina = controller.layers[1].stateMachine;
            foreach (string estado in new[] { "CarregandoBalde", "UsandoBalde" })
                maquina.AddState(estado);
        }

        GameObject corpo = TesteUtil.Novo("Corpo (teste)");
        corpo.SetActive(ativo);
        Animator animator = corpo.AddComponent<Animator>();
        animator.runtimeAnimatorController = controller;
        TesteUtil.Setar(itens, "animator", animator);
        itens.Registrar();
        return animator;
    }

    /* ---- O que aparece na mão ---- */

    [Test]
    public void Areia_MostraBaldeComAreia()
    {
        kit.Selecionar(Ferramenta.Areia);

        Assert.AreEqual(ItemNaMao.BaldeCheio, itens.ItemAtual);
        Assert.IsTrue(balde.activeSelf);
        Assert.IsTrue(areia.activeSelf);
        Assert.IsFalse(tela.activeSelf);
        Assert.IsFalse(borrifador.activeSelf);
    }

    [Test]
    public void AreiaZerada_MostraBaldeVazio()
    {
        kit.Selecionar(Ferramenta.Areia);
        kit.Consumir(Ferramenta.Areia);

        Assert.IsTrue(balde.activeSelf);
        Assert.IsFalse(areia.activeSelf);
    }

    [Test]
    public void TelaZerada_NaoMostraNada()
    {
        kit.Selecionar(Ferramenta.Tela);
        kit.Consumir(Ferramenta.Tela);

        Assert.AreEqual(ItemNaMao.Nenhum, itens.ItemAtual);
        Assert.IsFalse(balde.activeSelf);
        Assert.IsFalse(tela.activeSelf);
        Assert.IsFalse(borrifador.activeSelf);
    }

    [Test]
    public void AguaZerada_MantemBorrifador()
    {
        kit.Selecionar(Ferramenta.AguaSanitaria);
        while (kit.Estoque.TemCarga(Ferramenta.AguaSanitaria))
            kit.Consumir(Ferramenta.AguaSanitaria);

        Assert.AreEqual(ItemNaMao.Borrifador, itens.ItemAtual);
        Assert.IsTrue(borrifador.activeSelf);
    }

    [Test]
    public void TrocarDeFerramenta_TrocaOItem()
    {
        kit.Selecionar(Ferramenta.Tela);
        kit.Selecionar(Ferramenta.AguaSanitaria);

        Assert.IsFalse(tela.activeSelf);
        Assert.IsTrue(borrifador.activeSelf);
    }

    [Test]
    public void Maos_NaoMostraNada()
    {
        kit.Selecionar(Ferramenta.Areia);
        kit.Selecionar(Ferramenta.Maos);

        Assert.AreEqual(ItemNaMao.Nenhum, itens.ItemAtual);
        Assert.IsFalse(balde.activeSelf);
    }

    [Test]
    public void Reabastecer_DevolveAreiaAoBalde()
    {
        kit.Selecionar(Ferramenta.Areia);
        kit.Consumir(Ferramenta.Areia);

        kit.Reabastecer(new[] { Ferramenta.Areia });

        Assert.AreEqual(ItemNaMao.BaldeCheio, itens.ItemAtual);
        Assert.IsTrue(areia.activeSelf);
    }

    /* ---- Gesto e partículas ---- */

    [Test]
    public void Gesto_LigaSoAParticulaDoItem()
    {
        kit.Selecionar(Ferramenta.Areia);

        itens.AtualizarGesto(0.1f, true);
        Assert.IsTrue(itens.Gesticulando);
        Assert.IsTrue(areiaCaindo.emission.enabled);
        Assert.IsFalse(nevoa.emission.enabled);

        itens.AtualizarGesto(0.1f, false);
        Assert.IsFalse(itens.Gesticulando);
        Assert.IsFalse(areiaCaindo.emission.enabled);
        Assert.IsFalse(nevoa.emission.enabled);
    }

    [Test]
    public void GestoDoBorrifador_LigaANevoa()
    {
        kit.Selecionar(Ferramenta.AguaSanitaria);

        itens.AtualizarGesto(0.1f, true);

        Assert.IsTrue(nevoa.emission.enabled);
        Assert.IsFalse(areiaCaindo.emission.enabled);
    }

    [Test]
    public void UltimaCargaNoGesto_EscondeEParaParticulas()
    {
        kit.Selecionar(Ferramenta.Areia);
        itens.AtualizarGesto(0.1f, true);

        kit.Consumir(Ferramenta.Areia);
        itens.AtualizarGesto(0.016f, false);

        Assert.IsFalse(areia.activeSelf);
        Assert.IsFalse(areiaCaindo.emission.enabled);
        Assert.IsFalse(itens.Gesticulando);
    }

    [Test]
    public void UltimaTelaNoGesto_SomeDaMao()
    {
        kit.Selecionar(Ferramenta.Tela);
        itens.AtualizarGesto(0.1f, true);

        kit.Consumir(Ferramenta.Tela);

        Assert.IsFalse(tela.activeSelf);
    }

    [Test]
    public void SemItem_NaoGesticulaMesmoPedindo()
    {
        itens.AtualizarGesto(0.1f, true);

        Assert.IsFalse(itens.Gesticulando);
        Assert.IsFalse(areiaCaindo.emission.enabled);
    }

    /* ---- Peso da camada ---- */

    [Test]
    public void Peso_SobeEmDuracaoBraco()
    {
        kit.Selecionar(Ferramenta.Tela);

        itens.AtualizarGesto(Duracao / 2f, false);
        Assert.AreEqual(0.5f, itens.Peso, 0.001f);

        itens.AtualizarGesto(Duracao / 2f, false);
        Assert.AreEqual(1f, itens.Peso, 0.001f);
    }

    [Test]
    public void Peso_DesceSemItem()
    {
        kit.Selecionar(Ferramenta.Tela);
        itens.AtualizarGesto(Duracao, false);

        kit.Selecionar(Ferramenta.Maos);
        itens.AtualizarGesto(Duracao, false);

        Assert.AreEqual(0f, itens.Peso, 0.001f);
    }

    [Test]
    public void Pausa_NaoAndaOPeso()
    {
        kit.Selecionar(Ferramenta.Areia);
        itens.AtualizarGesto(Duracao / 2f, true);

        itens.AtualizarGesto(0f, true);
        Assert.AreEqual(0.5f, itens.Peso, 0.001f);

        itens.AtualizarGesto(0f, false);
        Assert.IsFalse(itens.Gesticulando);
    }

    /* ---- Ombro ---- */

    [Test]
    public void Ombro_GiraPeloOlhar()
    {
        kit.Selecionar(Ferramenta.Tela);
        itens.AtualizarGesto(Duracao / 2f, false);   // peso 0,5
        jogador.DefinirOlhar(0f, 40f);
        Quaternion antes = ombro.rotation;

        itens.GirarOmbro();

        Assert.AreEqual(20f, Quaternion.Angle(antes, ombro.rotation), 0.01f);
        Vector3 frente = ombro.rotation * Quaternion.Inverse(antes) * jogador.transform.forward;
        Assert.Less(frente.y, 0f, "pitch positivo olha para baixo: o braço desce junto");
    }

    [Test]
    public void Ombro_ParadoSemPeso()
    {
        jogador.DefinirOlhar(0f, 40f);
        Quaternion antes = ombro.rotation;

        itens.GirarOmbro();

        Assert.AreEqual(0f, Quaternion.Angle(antes, ombro.rotation), 0.001f);
    }

    /* ---- Animator ---- */

    [Test]
    public void AnimatorNaoPronto_EntraQuandoFicarPronto()
    {
        Animator animator = NovoAnimator(comCamada: true, ativo: false);
        kit.Selecionar(Ferramenta.Areia);

        itens.AtualizarGesto(Duracao, false);
        Assert.IsEmpty(avisos, "Animator atrasado não é controller incompleto");

        animator.gameObject.SetActive(true);
        itens.AtualizarGesto(0.016f, false);

        Assert.AreEqual(1f, animator.GetLayerWeight(1), 0.001f);
        Assert.AreEqual("CarregandoBalde", itens.EstadoNaCamada);
        Assert.IsEmpty(avisos);
    }

    [Test]
    public void Gesto_TrocaOEstadoDaCamada()
    {
        NovoAnimator(comCamada: true, ativo: true);
        kit.Selecionar(Ferramenta.Areia);

        itens.AtualizarGesto(0.016f, true);
        Assert.AreEqual("UsandoBalde", itens.EstadoNaCamada);

        itens.AtualizarGesto(0.016f, false);
        Assert.AreEqual("CarregandoBalde", itens.EstadoNaCamada);
    }

    [Test]
    public void CamadaAusente_AvisaUmaVez()
    {
        NovoAnimator(comCamada: false, ativo: true);
        kit.Selecionar(Ferramenta.Areia);

        itens.AtualizarGesto(0.016f, false);
        itens.AtualizarGesto(0.016f, false);

        Assert.AreEqual(1, avisos.Count, "um aviso só: " + string.Join(" | ", avisos));
        Assert.IsTrue(balde.activeSelf, "sem a pose, o item continua na mão");
    }
}
