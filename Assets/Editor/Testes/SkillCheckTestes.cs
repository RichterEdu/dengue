using NUnit.Framework;

/* ============================================================
 *  SkillCheckTestes.cs
 *  ---------------------------------------------------------------------
 *  Testes EditMode do skill check do ralo: acerto, erro e tempo esgotado
 *  na lógica pura; progresso e tela no RaloInteractable.
 *
 *  Rodar:  unity command run_tests --mode EditMode --filter_type category --filter SkillCheck
 * ============================================================ */

[Category("SkillCheck")]
public class SkillCheckTestes
{
    private KitDoAgente kit;
    private RaloInteractable ralo;

    [SetUp]
    public void Montar()
    {
        kit = TesteUtil.Com<KitDoAgente>("Kit (teste)");
        kit.Selecionar(Ferramenta.Tela);

        ralo = TesteUtil.Com<RaloInteractable>("Ralo (teste)");
        TesteUtil.Setar(ralo, "ferramentaNecessaria", Ferramenta.Tela);
        TesteUtil.Setar(ralo, "requiredClicks", 3);
        TesteUtil.Setar(ralo, "punchScale", 0f);
    }

    [TearDown]
    public void Limpar()
    {
        TesteUtil.LimparTudo();
    }

    [Test]
    public void Check_ApertarNaZona_Acerta()
    {
        SkillCheck check = new SkillCheck(0.5f, 0.2f, 1f);
        check.Atualizar(0.6f);
        Assert.AreEqual(ResultadoDoCheck.Acertou, check.Apertar());
    }

    [TestCase(0.4f)]
    [TestCase(0.8f)]
    public void Check_ApertarForaDaZona_Erra(float tempo)
    {
        SkillCheck check = new SkillCheck(0.5f, 0.2f, 1f);
        check.Atualizar(tempo);
        Assert.AreEqual(ResultadoDoCheck.Errou, check.Apertar());
    }

    [Test]
    public void Check_VoltaSemAperto_Erra()
    {
        SkillCheck check = new SkillCheck(0.5f, 0.2f, 1f);
        Assert.AreEqual(ResultadoDoCheck.EmAndamento, check.Atualizar(0.9f));
        Assert.AreEqual(ResultadoDoCheck.Errou, check.Atualizar(0.2f));
    }

    [Test]
    public void Sessao_AcertosTerminam_ComZonaMenorECabendoNoAnel()
    {
        SessaoDeSkillCheck sessao = new SessaoDeSkillCheck(3, 1f, 42);
        float anterior = float.MaxValue;

        for (int i = 0; i < 3; i++)
        {
            SkillCheck check = sessao.Atual;
            Assert.Less(check.TamanhoDaZona, anterior);
            Assert.GreaterOrEqual(check.InicioDaZona, SessaoDeSkillCheck.InicioMinimoDaZona);
            Assert.LessOrEqual(check.InicioDaZona + check.TamanhoDaZona, SessaoDeSkillCheck.FimMaximoDaZona + 0.0001f);
            anterior = check.TamanhoDaZona;

            sessao.Atualizar(check.InicioDaZona + check.TamanhoDaZona * 0.5f);
            Assert.AreEqual(ResultadoDoCheck.Acertou, sessao.Apertar());
        }

        Assert.IsTrue(sessao.Terminou);
    }

    [Test]
    public void Ralo_Errar_ZeraProgressoEGastaATela()
    {
        ralo.AvancarCom(kit);
        ralo.ErrarSkillCheck(kit);

        Assert.AreEqual(0, ralo.CurrentClicks);
        Assert.AreEqual(0f, ralo.Fechamento);
        Assert.AreEqual(ResultadoDoPasso.SemCarga, ralo.Verificar(kit));
    }

    [Test]
    public void Ralo_Cancelar_ZeraProgressoMasGuardaATela()
    {
        ralo.AvancarCom(kit);
        ralo.CancelarSkillCheck();

        Assert.AreEqual(0, ralo.CurrentClicks);
        Assert.IsTrue(kit.Estoque.TemCarga(Ferramenta.Tela));
    }

    [Test]
    public void Ralo_TresAcertos_ConcluiEGastaUmaTela()
    {
        ralo.AvancarCom(kit);
        ralo.AvancarCom(kit);
        Assert.AreEqual(ResultadoDoPasso.Concluiu, ralo.AvancarCom(kit));
        Assert.IsFalse(kit.Estoque.TemCarga(Ferramenta.Tela));
    }

    [Test]
    public void FocoComum_NaoUsaSkillCheck()
    {
        Assert.IsFalse(TesteUtil.Com<Interactable>("Foco (teste)").UsaSkillCheck);
        Assert.IsTrue(ralo.UsaSkillCheck);
    }
}
