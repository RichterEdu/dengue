using NUnit.Framework;

/* ============================================================
 *  RegraDoItemNaMaoTestes.cs
 *  ---------------------------------------------------------------------
 *  Testes EditMode da regra pura dos itens na mão: qual item aparece
 *  para cada ferramenta e carga, quando o gesto toca (nunca na vista de
 *  perto), que estado do Animator cada item usa e quanto o ombro gira
 *  com o olhar.
 *
 *  Rodar:  Window > General > Test Runner > EditMode (categoria ItensNaMao)
 *  ou      unity command run_tests --mode EditMode --filter_type category --filter ItensNaMao
 * ============================================================ */

[Category("ItensNaMao")]
public class RegraDoItemNaMaoTestes
{
    [TestCase(Ferramenta.Areia, 1, false, ItemNaMao.BaldeCheio)]
    [TestCase(Ferramenta.Areia, 0, false, ItemNaMao.BaldeVazio)]
    [TestCase(Ferramenta.Tela, 1, false, ItemNaMao.Tela)]
    [TestCase(Ferramenta.Tela, 0, false, ItemNaMao.Nenhum)]
    [TestCase(Ferramenta.AguaSanitaria, 2, false, ItemNaMao.Borrifador)]
    [TestCase(Ferramenta.AguaSanitaria, 0, false, ItemNaMao.Borrifador)]
    [TestCase(Ferramenta.Maos, 0, false, ItemNaMao.Nenhum)]
    [TestCase(Ferramenta.SacoDeLixo, 3, false, ItemNaMao.Nenhum)]
    [TestCase(Ferramenta.Areia, 1, true, ItemNaMao.Nenhum)]
    [TestCase(Ferramenta.AguaSanitaria, 2, true, ItemNaMao.Nenhum)]
    public void Qual_SegueATabela(Ferramenta ferramenta, int carga, bool comPneu, ItemNaMao esperado)
    {
        Assert.AreEqual(esperado, RegraDoItemNaMao.Qual(ferramenta, carga, comPneu));
    }

    [TestCase(ItemNaMao.BaldeCheio, Ferramenta.Areia, false, true, Ferramenta.Areia, true)]      // foco certo
    [TestCase(ItemNaMao.Borrifador, Ferramenta.AguaSanitaria, false, true, null, false)]         // ponto de uso
    [TestCase(ItemNaMao.BaldeCheio, Ferramenta.Areia, false, true, Ferramenta.Tela, false)]      // outro foco
    [TestCase(ItemNaMao.BaldeCheio, Ferramenta.Areia, false, false, Ferramenta.Areia, false)]    // sem segurar
    [TestCase(ItemNaMao.BaldeVazio, Ferramenta.Areia, false, true, Ferramenta.Areia, false)]     // nada a despejar
    [TestCase(ItemNaMao.Tela, Ferramenta.Tela, true, false, Ferramenta.Tela, true)]              // skill check
    [TestCase(ItemNaMao.Nenhum, Ferramenta.Tela, true, false, Ferramenta.Tela, false)]           // errou e zerou
    public void Gesticula_SegueATabela(ItemNaMao item, Ferramenta selecionada, bool skillCheck, bool avancando,
                                       Ferramenta? necessaria, bool esperado)
    {
        Assert.AreEqual(esperado, RegraDoItemNaMao.Gesticula(item, selecionada, skillCheck, avancando, necessaria));
    }

    [Test]
    public void DePertoNaoGesticula()
    {
        Assert.IsFalse(RegraDoItemNaMao.Gesticula(ItemNaMao.Borrifador, Ferramenta.AguaSanitaria, false, true,
                                                  Ferramenta.AguaSanitaria, dePerto: true));
        Assert.IsTrue(RegraDoItemNaMao.Gesticula(ItemNaMao.Borrifador, Ferramenta.AguaSanitaria, false, true,
                                                 Ferramenta.AguaSanitaria, dePerto: false));
        Assert.IsFalse(RegraDoItemNaMao.Gesticula(ItemNaMao.Borrifador, Ferramenta.AguaSanitaria, true, true,
                                                  Ferramenta.AguaSanitaria, dePerto: true));
    }

    [TestCase(ItemNaMao.BaldeCheio, false, "CarregandoBalde")]
    [TestCase(ItemNaMao.BaldeVazio, false, "CarregandoBalde")]
    [TestCase(ItemNaMao.BaldeCheio, true, "UsandoBalde")]
    [TestCase(ItemNaMao.Tela, false, "CarregandoTela")]
    [TestCase(ItemNaMao.Tela, true, "UsandoTela")]
    [TestCase(ItemNaMao.Borrifador, false, "CarregandoBorrifador")]
    [TestCase(ItemNaMao.Borrifador, true, "UsandoBorrifador")]
    [TestCase(ItemNaMao.Nenhum, false, null)]
    public void EstadoDoAnimator_DoItem(ItemNaMao item, bool gesticulando, string esperado)
    {
        Assert.AreEqual(esperado, RegraDoItemNaMao.EstadoDoAnimator(item, gesticulando));
    }

    [Test]
    public void GiroDoOmbro_MultiplicaPitchFatorEPeso()
    {
        Assert.AreEqual(15f, RegraDoItemNaMao.GiroDoOmbro(40f, 0.75f, 0.5f), 0.001f);
    }
}
