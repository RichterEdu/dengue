using NUnit.Framework;
using UnityEngine;

/* ============================================================
 *  ChuvaTestes.cs
 *  ---------------------------------------------------------------------
 *  Testes EditMode (Parte B) do relógio da Chuva: Seco → Previsão →
 *  Chovendo → Seco nos tempos certos, o sorteio do próximo início dentro
 *  do intervalo, o reencher dos focos esvaziados no começo da chuva e o
 *  relógio parado com a partida encerrada ou sem o Iniciar.
 *
 *  A Chuva de teste usa intervaloEntreInicios (200, 200), para o sorteio
 *  ser determinístico: previsão aos 155 s (200 − 45), chuva aos 200,
 *  fim aos 220 e o próximo início aos 400. O Start não roda no EditMode:
 *  o gerenciador e o fluxo de teste são ligados nos campos e o Iniciar é
 *  chamado direto, com System.Random de semente fixa.
 *
 *  O vaso de teste pede Areia, tem o atalho de esvaziar (podeEsvaziar,
 *  3 passos com as Mãos) e não tem punch nem animação de conclusão. O
 *  kit fica num objeto com ControladorJogador e começa com as Mãos.
 *
 *  Os objetos nascem pelo TesteUtil (escondidos, fora da cena) e são
 *  destruídos no [TearDown].
 *
 *  Rodar:  Window > General > Test Runner > EditMode (categoria ParteB)
 *  ou      unity command run_tests --mode EditMode --filter_type category --filter ParteB
 * ============================================================ */

[Category("ParteB")]
public class ChuvaTestes
{
    private Chuva chuva;
    private GerenciadorDeFocos gerenciador;
    private FluxoDeJogo fluxo;
    private KitDoAgente kit;

    private int previsoes, comecos, terminos;

    [SetUp]
    public void Preparar()
    {
        gerenciador = TesteUtil.Com<GerenciadorDeFocos>("Gerenciador (teste)");
        fluxo = TesteUtil.Com<FluxoDeJogo>("Fluxo (teste)");
        TesteUtil.Setar(gerenciador, "fluxo", fluxo);
        fluxo.DefinirEstadoParaTeste(true, false);

        chuva = NovaChuva(200f, 200f);

        ControladorJogador jogador = TesteUtil.Com<ControladorJogador>("Jogador (teste)");
        kit = jogador.gameObject.AddComponent<KitDoAgente>();
    }

    [TearDown]
    public void Limpar()
    {
        TesteUtil.LimparTudo();
    }

    /* Chuva ligada ao gerenciador e ao fluxo de teste, com o intervalo
     * pedido e os eventos contados. */
    private Chuva NovaChuva(float minimo, float maximo)
    {
        Chuva nova = TesteUtil.Com<Chuva>("Chuva (teste)");
        TesteUtil.Setar(nova, "gerenciador", gerenciador);
        TesteUtil.Setar(nova, "fluxo", fluxo);
        TesteUtil.Setar(nova, "intervaloEntreInicios.x", minimo);
        TesteUtil.Setar(nova, "intervaloEntreInicios.y", maximo);

        previsoes = comecos = terminos = 0;
        nova.OnPrevisao += () => previsoes++;
        nova.OnComecou += () => comecos++;
        nova.OnTerminou += () => terminos++;
        return nova;
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

    [Test]
    public void Iniciar_Seco_ProximoInicio200()
    {
        chuva.Iniciar(new System.Random(1));

        Assert.AreEqual(EstadoDaChuva.Seco, chuva.Estado);
        Assert.AreEqual(0f, chuva.Tempo, 1e-4f);
        Assert.AreEqual(200f, chuva.ProximoInicio, 1e-4f);
        Assert.AreEqual(0, chuva.Chuvas);

        chuva.Avancar(100f);
        chuva.Iniciar(new System.Random(2));
        Assert.AreEqual(0f, chuva.Tempo, 1e-4f, "o Iniciar zera o relógio");
        Assert.AreEqual(200f, chuva.ProximoInicio, 1e-4f, "e sorteia de novo a partir do zero");
    }

    [Test]
    public void Previsao_Aos155()
    {
        chuva.Iniciar(new System.Random(1));

        chuva.Avancar(154f);
        Assert.AreEqual(EstadoDaChuva.Seco, chuva.Estado, "aos 154 s ainda está seco");
        Assert.AreEqual(0, previsoes);

        chuva.Avancar(1f);
        Assert.AreEqual(EstadoDaChuva.Previsao, chuva.Estado);
        Assert.AreEqual(1, previsoes, "OnPrevisao 1 vez");
        Assert.AreEqual(0, comecos);
    }

    [Test]
    public void Chove_Aos200_Reenche()
    {
        VasoInteractable vaso = NovoVasoEsvaziado();
        chuva.Iniciar(new System.Random(1));

        chuva.Avancar(200f);

        Assert.AreEqual(EstadoDaChuva.Chovendo, chuva.Estado);
        Assert.AreEqual(1, previsoes, "um Avancar só atravessa a previsão: OnPrevisao 1 vez");
        Assert.AreEqual(1, comecos, "OnComecou 1 vez");
        Assert.AreEqual(1, chuva.Chuvas);
        Assert.IsFalse(vaso.Esvaziado, "a chuva reencheu o vaso");
        Assert.AreEqual(EstagioDoFoco.Larva, gerenciador.EstagioDe(vaso), "reenchido volta como larva");
    }

    [Test]
    public void Termina_Aos220()
    {
        chuva.Iniciar(new System.Random(1));

        chuva.Avancar(219f);
        Assert.AreEqual(EstadoDaChuva.Chovendo, chuva.Estado, "aos 219 s ainda chove");
        Assert.AreEqual(0, terminos);

        chuva.Avancar(1f);
        Assert.AreEqual(EstadoDaChuva.Seco, chuva.Estado);
        Assert.AreEqual(1, terminos, "OnTerminou 1 vez");
    }

    [Test]
    public void ProximaChuva_Aos400()
    {
        chuva.Iniciar(new System.Random(1));

        chuva.Avancar(200f);
        Assert.AreEqual(400f, chuva.ProximoInicio, 1e-4f, "o próximo início é sorteado no começo da chuva");

        chuva.Avancar(154f);
        Assert.AreEqual(EstadoDaChuva.Seco, chuva.Estado, "aos 354 s está seco");
        Assert.AreEqual(1, previsoes);

        chuva.Avancar(1f);
        Assert.AreEqual(EstadoDaChuva.Previsao, chuva.Estado, "a segunda previsão sai aos 355 s");
        Assert.AreEqual(2, previsoes);

        chuva.Avancar(45f);
        Assert.AreEqual(EstadoDaChuva.Chovendo, chuva.Estado, "a segunda chuva começa aos 400 s");
        Assert.AreEqual(2, chuva.Chuvas);
        Assert.AreEqual(600f, chuva.ProximoInicio, 1e-4f);
    }

    [Test]
    public void Intervalo_DentroDoSorteio()
    {
        Chuva sorteada = NovaChuva(180f, 240f);

        for (int semente = 0; semente < 20; semente++)
        {
            sorteada.Iniciar(new System.Random(semente));
            float primeiro = sorteada.ProximoInicio;
            Assert.That(primeiro, Is.InRange(180f, 240f), "1º início, semente " + semente);

            sorteada.Avancar(primeiro);
            Assert.AreEqual(EstadoDaChuva.Chovendo, sorteada.Estado, "semente " + semente);
            float intervalo = sorteada.ProximoInicio - primeiro;
            Assert.That(intervalo, Is.InRange(180f - 1e-3f, 240f + 1e-3f), "2º intervalo, semente " + semente);
        }
    }

    /* Review Focus 3: a partida encerrada para o relógio da chuva. */
    [Test]
    public void PartidaEncerrada_NaoAvanca()
    {
        VasoInteractable vaso = NovoVasoEsvaziado();
        chuva.Iniciar(new System.Random(1));
        fluxo.DefinirEstadoParaTeste(true, true);

        chuva.Avancar(500f);

        Assert.AreEqual(EstadoDaChuva.Seco, chuva.Estado);
        Assert.AreEqual(0f, chuva.Tempo, 1e-4f, "o relógio não anda");
        Assert.AreEqual(0, previsoes + comecos + terminos, "nenhum evento");
        Assert.AreEqual(0, chuva.Chuvas);
        Assert.IsTrue(vaso.Esvaziado, "0 reenchidos");
    }

    [Test]
    public void NaoIniciada_NaoAvanca()
    {
        VasoInteractable vaso = NovoVasoEsvaziado();

        chuva.Avancar(500f);

        Assert.AreEqual(EstadoDaChuva.Seco, chuva.Estado);
        Assert.AreEqual(0f, chuva.Tempo, 1e-4f, "o relógio não anda");
        Assert.AreEqual(0, previsoes + comecos + terminos, "nenhum evento");
        Assert.AreEqual(0, chuva.Chuvas);
        Assert.IsTrue(vaso.Esvaziado, "0 reenchidos");
    }
}
