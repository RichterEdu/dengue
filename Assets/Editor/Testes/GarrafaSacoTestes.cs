using NUnit.Framework;

/* ============================================================
 *  GarrafaSacoTestes.cs
 *  ---------------------------------------------------------------------
 *  Testes EditMode (Parte A) das garrafas recolhidas no saco de lixo:
 *  cada passo guarda uma garrafa no saco, com o saco cheio o passo é
 *  recusado (SacoCheio) sem mexer no foco, e depois de esvaziar na
 *  caçamba o grupo volta a avançar até concluir. O tempo de cada passo
 *  é o de uma garrafa, seja qual for o tamanho do grupo.
 *
 *  O Start não roda no EditMode e as corrotinas não andam. Por isso o
 *  grupo de teste não tem garrafas de verdade (o OnInteract não acha
 *  nenhuma para sumir) e os 3 cliques vêm do requiredClicks; o Nova()
 *  zera o punchScale e o shrinkDuration, como nos outros testes.
 *
 *  O saco do kit de teste tem 2 lugares (capacidadeSaco ajustado antes
 *  do primeiro acesso ao Estoque, que é criado com esse número).
 *
 *  Os objetos nascem pelo TesteUtil (escondidos, fora da cena) e são
 *  destruídos no [TearDown].
 *
 *  Rodar:  Window > General > Test Runner > EditMode (categoria ParteA)
 *  ou      unity command run_tests --mode EditMode --filter_type category --filter ParteA
 * ============================================================ */

[Category("ParteA")]
public class GarrafaSacoTestes
{
    private KitDoAgente kit;

    [SetUp]
    public void Preparar()
    {
        kit = TesteUtil.Com<KitDoAgente>("Kit (teste)");
        TesteUtil.Setar(kit, "capacidadeSaco", 2);
    }

    [TearDown]
    public void Limpar()
    {
        TesteUtil.LimparTudo();
    }

    /* Grupo de 3 cliques que pede o saco de lixo, sem punch nem animação de conclusão. */
    private static GarrafaInteractable Nova()
    {
        GarrafaInteractable garrafa = TesteUtil.Com<GarrafaInteractable>("Garrafas (teste)");
        TesteUtil.Setar(garrafa, "requiredClicks", 3);
        TesteUtil.Setar(garrafa, "ferramentaNecessaria", Ferramenta.SacoDeLixo);
        TesteUtil.Setar(garrafa, "punchScale", 0f);
        TesteUtil.Setar(garrafa, "shrinkDuration", 0f);
        return garrafa;
    }

    [Test]
    public void SacoCheio_Bloqueia_E_CacambaLibera()
    {
        GarrafaInteractable garrafa = Nova();
        kit.Selecionar(Ferramenta.SacoDeLixo);

        Assert.AreEqual(ResultadoDoPasso.Avancou, garrafa.AvancarCom(kit), "1ª garrafa");
        Assert.AreEqual(ResultadoDoPasso.Avancou, garrafa.AvancarCom(kit), "2ª garrafa");

        Assert.AreEqual(ResultadoDoPasso.SacoCheio, garrafa.AvancarCom(kit), "3ª garrafa: o saco está cheio");
        Assert.AreEqual(2, kit.Estoque.GarrafasNoSaco);
        Assert.AreEqual(2, garrafa.CurrentClicks, "recusado, o foco não avança");
        Assert.IsFalse(garrafa.IsComplete);

        Assert.AreEqual(2, kit.EsvaziarSaco(), "a caçamba recebe as 2");

        Assert.AreEqual(ResultadoDoPasso.Concluiu, garrafa.AvancarCom(kit), "com o saco vazio, a última fecha o foco");
        Assert.IsTrue(garrafa.IsComplete);
        Assert.AreEqual(1, kit.Estoque.GarrafasNoSaco, "a última também vai para o saco");
    }

    [Test]
    public void SegundosPorPasso_EhPorGarrafa()
    {
        GarrafaInteractable garrafa = Nova();

        Assert.AreEqual(0.4f, garrafa.SegundosPorPasso, 1e-5f);

        // Não divide um tempo total entre os cliques: cada garrafa leva o mesmo.
        TesteUtil.Setar(garrafa, "requiredClicks", 6);
        Assert.AreEqual(0.4f, garrafa.SegundosPorPasso, 1e-5f, "grupo maior, mesmo tempo por garrafa");
    }

    [Test]
    public void SemSaco_FerramentaErrada()
    {
        GarrafaInteractable garrafa = Nova();   // o kit está nas Mãos

        Assert.AreEqual(ResultadoDoPasso.FerramentaErrada, garrafa.AvancarCom(kit));
        Assert.AreEqual(0, garrafa.CurrentClicks);
        Assert.AreEqual(0, kit.Estoque.GarrafasNoSaco, "nada foi para o saco");
    }
}
