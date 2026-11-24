using NUnit.Framework;

/* ============================================================
 *  SkillCheckTestes.cs
 *  ---------------------------------------------------------------------
 *  Testes EditMode do skill check do bueiro: acerto, erro e tempo esgotado
 *  na lógica pura; o adaptador MinigameDeSkillCheck (acerto = Passo, erro
 *  = Errou); progresso e tela no BueiroInteractable.
 *
 *  Rodar:  unity command run_tests --mode EditMode --filter_type category --filter SkillCheck
 * ============================================================ */

[Category("SkillCheck")]
public class SkillCheckTestes
{
    private KitDoAgente kit;
    private BueiroInteractable bueiro;

    [SetUp]
    public void Montar()
    {
        kit = TesteUtil.Com<KitDoAgente>("Kit (teste)");
        kit.Selecionar(Ferramenta.Tela);

        bueiro = TesteUtil.Com<BueiroInteractable>("Bueiro (teste)");
        TesteUtil.Setar(bueiro, "ferramentaNecessaria", Ferramenta.Tela);
        TesteUtil.Setar(bueiro, "requiredClicks", 3);
        TesteUtil.Setar(bueiro, "punchScale", 0f);
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
    public void Bueiro_Errar_ZeraProgressoEGastaATela()
    {
        bueiro.AvancarCom(kit);
        bueiro.AoErrarMinigame(kit);

        Assert.AreEqual(0, bueiro.CurrentClicks);
        Assert.AreEqual(0f, bueiro.Fechamento);
        Assert.AreEqual(ResultadoDoPasso.SemCarga, bueiro.Verificar(kit));
    }

    [Test]
    public void Bueiro_Cancelar_ZeraProgressoMasGuardaATela()
    {
        bueiro.AvancarCom(kit);
        bueiro.AoSairDoMinigame();

        Assert.AreEqual(0, bueiro.CurrentClicks);
        Assert.IsTrue(kit.Estoque.TemCarga(Ferramenta.Tela));
    }

    [Test]
    public void Bueiro_TresAcertos_ConcluiEGastaUmaTela()
    {
        bueiro.AvancarCom(kit);
        bueiro.AvancarCom(kit);
        Assert.AreEqual(ResultadoDoPasso.Concluiu, bueiro.AvancarCom(kit));
        Assert.IsFalse(kit.Estoque.TemCarga(Ferramenta.Tela));
    }

    [Test]
    public void FocoComum_NaoTemMinigame_BueiroTemOSkillCheck()
    {
        Assert.IsNull(TesteUtil.Com<Interactable>("Foco (teste)").CriarMinigame(kit, 0));
        Assert.IsInstanceOf<MinigameDeSkillCheck>(bueiro.CriarMinigame(kit, 0));
    }

    [Test]
    public void AdaptadorAcertoViraPasso()
    {
        MinigameDeSkillCheck minigame = new MinigameDeSkillCheck(new SessaoDeSkillCheck(3, 1f, 42));
        SkillCheck check = minigame.Sessao.Atual;

        Assert.AreEqual(EstadoDoMinigame.EmAndamento,
                        minigame.Atualizar(check.InicioDaZona + check.TamanhoDaZona * 0.5f, new EntradaDoMinigame()));
        Assert.AreEqual(EstadoDoMinigame.Passo, minigame.Atualizar(0f, new EntradaDoMinigame { Apertou = true }));
        Assert.AreEqual(1, minigame.Sessao.Acertos);
        Assert.IsFalse(minigame.PodeSair);
        Assert.AreEqual(0f, minigame.FracaoDoPasso);
        Assert.IsFalse(minigame.Agindo, "o skill check não tem som de ação");
    }

    [Test]
    public void AdaptadorErroViraErrou()
    {
        // O ponteiro ainda no topo: a zona nunca começa antes de InicioMinimoDaZona.
        MinigameDeSkillCheck minigame = new MinigameDeSkillCheck(new SessaoDeSkillCheck(3, 1f, 42));

        Assert.AreEqual(EstadoDoMinigame.Errou, minigame.Atualizar(0f, new EntradaDoMinigame { Apertou = true }));
    }
}
