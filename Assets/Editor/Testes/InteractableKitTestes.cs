using NUnit.Framework;

/* ============================================================
 *  InteractableKitTestes.cs
 *  ---------------------------------------------------------------------
 *  Testes EditMode (Parte A) do Interactable avançando com o kit:
 *  ferramenta certa avança e conclui, a errada e a sem carga não mexem
 *  no foco, a carga só é gasta na conclusão e o caminho antigo
 *  Interact() (sem kit) continua funcionando.
 *
 *  O Start não roda no EditMode e as corrotinas não andam. Por isso o
 *  Novo() zera o punchScale (sem punch por passo) e o shrinkDuration
 *  (o OnComplete padrão cai direto no FinishRemoval e só desativa o
 *  objeto). Cada foco de teste tem 2 cliques e 1 s para concluir.
 *
 *  O kit usa o estoque padrão (areia 1, tela 1, água sanitária 2) e
 *  começa com as Mãos selecionadas.
 *
 *  Os objetos nascem pelo TesteUtil (escondidos, fora da cena) e são
 *  destruídos no [TearDown].
 *
 *  Rodar:  Window > General > Test Runner > EditMode (categoria ParteA)
 *  ou      unity command run_tests --mode EditMode --filter_type category --filter ParteA
 * ============================================================ */

[Category("ParteA")]
public class InteractableKitTestes
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

    /* Foco de 2 cliques e 1 s, sem punch nem animação de conclusão. */
    private static Interactable Novo(Ferramenta ferramenta)
    {
        Interactable foco = TesteUtil.Com<Interactable>("Foco (teste)");
        TesteUtil.Setar(foco, "requiredClicks", 2);
        TesteUtil.Setar(foco, "segundosParaConcluir", 1f);
        TesteUtil.Setar(foco, "punchScale", 0f);
        TesteUtil.Setar(foco, "shrinkDuration", 0f);
        TesteUtil.Setar(foco, "ferramentaNecessaria", ferramenta);
        return foco;
    }

    [Test]
    public void FerramentaCerta_AvancaEConclui()
    {
        Interactable foco = Novo(Ferramenta.Areia);
        kit.Selecionar(Ferramenta.Areia);

        Assert.AreEqual(ResultadoDoPasso.Avancou, foco.AvancarCom(kit), "1º passo");
        Assert.AreEqual(1, foco.CurrentClicks);
        Assert.IsFalse(foco.IsComplete, "conclui só no último passo");

        Assert.AreEqual(ResultadoDoPasso.Concluiu, foco.AvancarCom(kit), "2º passo");
        Assert.IsTrue(foco.IsComplete);
        Assert.AreEqual(0, kit.Estoque.Quantidade(Ferramenta.Areia), "a areia foi gasta");
    }

    [Test]
    public void CargaSoEGastaAoConcluir()
    {
        Interactable foco = Novo(Ferramenta.Areia);
        kit.Selecionar(Ferramenta.Areia);

        foco.AvancarCom(kit);

        Assert.AreEqual(1, kit.Estoque.Quantidade(Ferramenta.Areia), "no meio da ação a carga continua lá");
    }

    [Test]
    public void FerramentaErrada_NaoAvanca()
    {
        Interactable foco = Novo(Ferramenta.Areia);   // o kit está nas Mãos

        Assert.AreEqual(ResultadoDoPasso.FerramentaErrada, foco.AvancarCom(kit));
        Assert.AreEqual(0, foco.CurrentClicks);
    }

    [Test]
    public void SemCarga_NaoAvanca()
    {
        Interactable foco = Novo(Ferramenta.Areia);
        kit.Selecionar(Ferramenta.Areia);
        kit.Consumir(Ferramenta.Areia);

        Assert.AreEqual(ResultadoDoPasso.SemCarga, foco.AvancarCom(kit));
        Assert.AreEqual(0, foco.CurrentClicks);
    }

    [Test]
    public void FerramentaErrada_TemPrioridadeSobreSemCarga()
    {
        // Errada E vazia: o aviso de ferramenta errada é o que ensina.
        Interactable foco = Novo(Ferramenta.Tela);
        kit.Selecionar(Ferramenta.Areia);
        kit.Consumir(Ferramenta.Areia);

        Assert.AreEqual(ResultadoDoPasso.FerramentaErrada, foco.AvancarCom(kit));
    }

    [Test]
    public void Verificar_NaoMexeNoFoco()
    {
        Interactable foco = Novo(Ferramenta.Areia);
        kit.Selecionar(Ferramenta.Areia);

        Assert.AreEqual(ResultadoDoPasso.Avancou, foco.Verificar(kit));
        Assert.AreEqual(0, foco.CurrentClicks, "Verificar só confere");
        Assert.AreEqual(1, kit.Estoque.Quantidade(Ferramenta.Areia), "Verificar não gasta carga");
    }

    [Test]
    public void OnCompleted_DisparaUmaVez()
    {
        Interactable foco = Novo(Ferramenta.Maos);
        int concluidos = 0;
        foco.OnCompleted += f => concluidos++;

        foco.AvancarCom(kit);
        foco.AvancarCom(kit);
        Assert.AreEqual(1, concluidos);

        Assert.AreEqual(ResultadoDoPasso.Bloqueado, foco.AvancarCom(kit), "foco já concluído");
        Assert.AreEqual(1, concluidos, "não conclui de novo");
    }

    [Test]
    public void Interact_SemKit_ContinuaFuncionando()
    {
        Interactable foco = Novo(Ferramenta.Maos);

        foco.Interact();
        foco.Interact();

        Assert.IsTrue(foco.IsComplete);
    }

    [Test]
    public void SegundosPorPasso_DivideEntreOsCliques()
    {
        Interactable foco = Novo(Ferramenta.Maos);
        TesteUtil.Setar(foco, "segundosParaConcluir", 2f);
        TesteUtil.Setar(foco, "requiredClicks", 4);

        Assert.AreEqual(0.5f, foco.SegundosPorPasso, 1e-5f);
    }
}
