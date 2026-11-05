using NUnit.Framework;

/* ============================================================
 *  FluxoPressaoTestes.cs
 *  ---------------------------------------------------------------------
 *  Testes EditMode (Parte B) do FluxoDeJogo ligado ao GerenciadorDeFocos:
 *  a taxa da ocupação pela pressão dos focos (a média dos pesos, que
 *  pode passar de 1) e a vitória pelo OnFocoEliminado do gerenciador.
 *
 *  O fluxo de teste não tem começo calmo nem rampa (duracaoInicioCalmo 0
 *  e duracaoRampa 0): a taxa é taxaComTodosOsFocos × pressao −
 *  recuperacaoSemFocos, com 0,003 e 0,001.
 *
 *  O Start não roda no EditMode: o gerenciador de teste entra no campo
 *  'gerenciador' (o FindAnyObjectByType não acha objetos escondidos do
 *  TesteUtil) e o fluxo é ligado com Ligar(). Os focos de teste não têm
 *  punch nem animação de conclusão (o OnComplete só desativa o objeto).
 *
 *  Os objetos nascem pelo TesteUtil (escondidos, fora da cena) e são
 *  destruídos no [TearDown].
 *
 *  Rodar:  Window > General > Test Runner > EditMode (categoria ParteB)
 *  ou      unity command run_tests --mode EditMode --filter_type category --filter ParteB
 * ============================================================ */

[Category("ParteB")]
public class FluxoPressaoTestes
{
    private FluxoDeJogo fluxo;

    [SetUp]
    public void Preparar()
    {
        fluxo = TesteUtil.Com<FluxoDeJogo>("Fluxo (teste)");
        TesteUtil.Setar(fluxo, "taxaComTodosOsFocos", 0.003f);
        TesteUtil.Setar(fluxo, "recuperacaoSemFocos", 0.001f);
        TesteUtil.Setar(fluxo, "duracaoInicioCalmo", 0f);
        TesteUtil.Setar(fluxo, "duracaoRampa", 0f);
    }

    [TearDown]
    public void Limpar()
    {
        TesteUtil.LimparTudo();
    }

    /* Foco de 1 clique: o Interact() o conclui de uma vez. */
    private static Interactable NovoFoco(string nome)
    {
        Interactable foco = TesteUtil.Com<Interactable>(nome);
        TesteUtil.Setar(foco, "requiredClicks", 1);
        TesteUtil.Setar(foco, "punchScale", 0f);
        TesteUtil.Setar(foco, "shrinkDuration", 0f);
        return foco;
    }

    [Test]
    public void TaxaPara_PressaoUm()
    {
        Assert.AreEqual(0.002f, fluxo.TaxaPara(100f, 1f, false), 1e-6f, "0,003 × 1 − 0,001");
    }

    [Test]
    public void TaxaPara_PressaoDoisEMeio()
    {
        Assert.AreEqual(0.0065f, fluxo.TaxaPara(100f, 2.5f, false), 1e-6f, "0,003 × 2,5 − 0,001: a pressão passa de 1");
    }

    [Test]
    public void TaxaPara_PressaoZero_Desce()
    {
        Assert.AreEqual(-0.001f, fluxo.TaxaPara(100f, 0f, false), 1e-6f, "só a recuperação");
    }

    [Test]
    public void Vitoria_QuandoGerenciadorEliminaTodos()
    {
        GerenciadorDeFocos ger = TesteUtil.Com<GerenciadorDeFocos>("Gerenciador (teste)");
        TesteUtil.Setar(fluxo, "gerenciador", ger);
        Interactable a = NovoFoco("Foco A (teste)");
        Interactable b = NovoFoco("Foco B (teste)");
        ger.Registrar(new[] { a, b });
        fluxo.Ligar();
        int vitorias = 0;
        fluxo.OnVitoria += () => vitorias++;

        a.Interact();
        Assert.AreEqual(0, vitorias, "um foco de pé: ainda não venceu");
        Assert.AreEqual(1, fluxo.FocosEliminados);
        Assert.AreEqual(2, fluxo.FocosTotal);

        b.Interact();

        Assert.AreEqual(1, vitorias, "OnVitoria 1 vez");
        Assert.IsTrue(fluxo.PartidaEncerrada);
        Assert.AreEqual(2, fluxo.FocosEliminados);
    }
}
