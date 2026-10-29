using NUnit.Framework;

/* ============================================================
 *  KitDoAgenteTestes.cs
 *  ---------------------------------------------------------------------
 *  Testes EditMode (Parte A) do KitDoAgente (estoque, ferramenta
 *  selecionada e os avisos) e dos limites novos do ControladorJogador.
 *
 *  O Awake não roda no EditMode: o kit cria o estoque no primeiro
 *  acesso, com os campos do Inspector (padrão: areia 1, tela 1, água
 *  sanitária 2 doses, saco de 6 garrafas). Por isso o Setar vem ANTES
 *  de qualquer acesso ao Estoque.
 *
 *  Os objetos nascem pelo TesteUtil (escondidos, fora da cena) e são
 *  destruídos no [TearDown].
 *
 *  Rodar:  Window > General > Test Runner > EditMode (categoria ParteA)
 *  ou      unity command run_tests --mode EditMode --filter_type category --filter ParteA
 * ============================================================ */

[Category("ParteA")]
public class KitDoAgenteTestes
{
    private KitDoAgente kit;

    [SetUp]
    public void Preparar()
    {
        kit = TesteUtil.Com<KitDoAgente>("Kit (teste)");
    }

    [TearDown]
    public void Limpar()
    {
        TesteUtil.LimparTudo();
    }

    [Test]
    public void Estoque_SemAwake_UsaCargasPadrao()
    {
        Assert.AreEqual(1, kit.Estoque.Quantidade(Ferramenta.Areia));
        Assert.AreEqual(2, kit.Estoque.Quantidade(Ferramenta.AguaSanitaria));
        Assert.AreEqual(6, kit.Estoque.CapacidadeDoSaco);
    }

    [Test]
    public void Estoque_UsaCampoDoInspector()
    {
        TesteUtil.Setar(kit, "cargaTela", 3);

        Assert.AreEqual(3, kit.Estoque.Capacidade(Ferramenta.Tela));
    }

    [Test]
    public void Selecionar_TrocaEAvisa()
    {
        int trocas = 0;
        int mudancas = 0;
        Ferramenta recebida = Ferramenta.Maos;
        kit.OnFerramentaTrocada += f => { trocas++; recebida = f; };
        kit.OnMudou += () => mudancas++;

        Assert.IsTrue(kit.Selecionar(Ferramenta.Areia));
        Assert.AreEqual(Ferramenta.Areia, kit.Selecionada);
        Assert.AreEqual(1, trocas, "OnFerramentaTrocada");
        Assert.AreEqual(Ferramenta.Areia, recebida);
        Assert.GreaterOrEqual(mudancas, 1, "OnMudou");

        Assert.IsFalse(kit.Selecionar(Ferramenta.Areia), "já era a selecionada");
        Assert.AreEqual(1, trocas, "selecionar a mesma não avisa de novo");
    }

    [Test]
    public void Proxima_DaVolta()
    {
        kit.Selecionar(Ferramenta.SacoDeLixo);
        kit.Proxima();
        Assert.AreEqual(Ferramenta.Maos, kit.Selecionada, "Próxima a partir do Saco de lixo");

        kit.Anterior();
        Assert.AreEqual(Ferramenta.SacoDeLixo, kit.Selecionada, "Anterior a partir das Mãos");
    }

    [Test]
    public void Reabastecer_SoAvisaQuandoMuda()
    {
        int avisos = 0;
        kit.OnReabasteceu += origem => avisos++;

        Assert.IsFalse(kit.Reabastecer(new[] { Ferramenta.Areia }), "kit cheio");
        Assert.AreEqual(0, avisos, "kit cheio não avisa");

        Assert.IsTrue(kit.Consumir(Ferramenta.Areia));
        Assert.IsTrue(kit.Reabastecer(new[] { Ferramenta.Areia }), "depois de gastar a areia");
        Assert.AreEqual(1, avisos);
        Assert.AreEqual(1, kit.Estoque.Quantidade(Ferramenta.Areia));
    }

    [Test]
    public void EsvaziarSaco_SoAvisaComGarrafa()
    {
        int avisos = 0;
        kit.OnSacoEsvaziado += () => avisos++;

        Assert.AreEqual(0, kit.EsvaziarSaco(), "saco vazio");
        Assert.AreEqual(0, avisos, "saco vazio não avisa");

        Assert.IsTrue(kit.GuardarGarrafa());
        Assert.IsTrue(kit.GuardarGarrafa());
        Assert.AreEqual(2, kit.EsvaziarSaco());
        Assert.AreEqual(1, avisos);
        Assert.AreEqual(0, kit.Estoque.GarrafasNoSaco);
    }

    [Test]
    public void Controlador_PadraoSemLimite()
    {
        ControladorJogador jogador = TesteUtil.Com<ControladorJogador>("Jogador (teste)");

        Assert.AreEqual(1f, jogador.MultiplicadorVelocidade);
        Assert.IsTrue(jogador.PodeCorrerEPular);
    }
}
