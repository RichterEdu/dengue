using NUnit.Framework;

/* ============================================================
 *  EstoqueDoKitTestes.cs
 *  ---------------------------------------------------------------------
 *  Testes EditMode (Parte A) do EstoqueDoKit e dos dados das ferramentas
 *  (Ferramentas.Nome/Consumivel/Todas). O estoque é C# puro: não precisa
 *  de GameObject nem de cena, então aqui nem entra o TesteUtil.
 *
 *  Todos usam o estoque padrão do kit: areia 1, tela 1, água sanitária 2
 *  doses e saco de 6 garrafas.
 *
 *  Rodar:  Window > General > Test Runner > EditMode (categoria ParteA)
 *  ou      unity command run_tests --mode EditMode --filter_type category --filter ParteA
 * ============================================================ */

[Category("ParteA")]
public class EstoqueDoKitTestes
{
    private EstoqueDoKit estoque;

    [SetUp]
    public void Preparar()
    {
        estoque = new EstoqueDoKit(1, 1, 2, 6);
    }

    [Test]
    public void NovoEstoque_ComecaCheio()
    {
        Assert.AreEqual(1, estoque.Quantidade(Ferramenta.Areia));
        Assert.AreEqual(1, estoque.Quantidade(Ferramenta.Tela));
        Assert.AreEqual(2, estoque.Quantidade(Ferramenta.AguaSanitaria));
        Assert.AreEqual(0, estoque.GarrafasNoSaco);
        Assert.AreEqual(6, estoque.CapacidadeDoSaco);
    }

    [Test]
    public void Consumir_GastaUma_E_FalhaQuandoVazio()
    {
        Assert.IsTrue(estoque.Consumir(Ferramenta.Areia));
        Assert.AreEqual(0, estoque.Quantidade(Ferramenta.Areia));
        Assert.IsFalse(estoque.TemCarga(Ferramenta.Areia));
        Assert.IsFalse(estoque.Consumir(Ferramenta.Areia));
    }

    [Test]
    public void NaoConsumivel_SempreTemCarga()
    {
        foreach (Ferramenta f in new[] { Ferramenta.Maos, Ferramenta.SacoDeLixo })
        {
            Assert.IsTrue(estoque.TemCarga(f), $"TemCarga({f})");
            Assert.IsTrue(estoque.Consumir(f), $"Consumir({f})");
            Assert.AreEqual(0, estoque.Quantidade(f), $"Quantidade({f})");
        }
    }

    [Test]
    public void Reabastecer_SoOQueEstaNaLista()
    {
        estoque.Consumir(Ferramenta.Areia);
        estoque.Consumir(Ferramenta.Tela);

        Assert.IsTrue(estoque.Reabastecer(new[] { Ferramenta.Areia }));
        Assert.AreEqual(1, estoque.Quantidade(Ferramenta.Areia));
        Assert.AreEqual(0, estoque.Quantidade(Ferramenta.Tela));
    }

    [Test]
    public void Reabastecer_JaCheio_DevolveFalse()
    {
        Assert.IsFalse(estoque.Reabastecer(new[] { Ferramenta.Areia, Ferramenta.Tela, Ferramenta.AguaSanitaria }));
        Assert.AreEqual(1, estoque.Quantidade(Ferramenta.Areia));
        Assert.AreEqual(1, estoque.Quantidade(Ferramenta.Tela));
        Assert.AreEqual(2, estoque.Quantidade(Ferramenta.AguaSanitaria));
    }

    [Test]
    public void Cheio_FicaFalsoDepoisDeConsumir()
    {
        Ferramenta[] conjunto = { Ferramenta.Areia, Ferramenta.Tela };

        Assert.IsTrue(estoque.Cheio(conjunto));
        estoque.Consumir(Ferramenta.Areia);
        Assert.IsFalse(estoque.Cheio(conjunto));
    }

    [Test]
    public void Saco_EncheEsvazia()
    {
        for (int i = 1; i <= 6; i++)
            Assert.IsTrue(estoque.GuardarGarrafa(), $"garrafa {i}");

        Assert.IsFalse(estoque.GuardarGarrafa(), "7ª garrafa com o saco cheio");
        Assert.IsTrue(estoque.SacoCheio);

        Assert.AreEqual(6, estoque.EsvaziarSaco());
        Assert.AreEqual(0, estoque.GarrafasNoSaco);
    }

    [Test]
    public void Ferramentas_NomesEConsumiveis()
    {
        Assert.AreEqual("Água sanitária", Ferramentas.Nome(Ferramenta.AguaSanitaria));
        Assert.IsTrue(Ferramentas.Consumivel(Ferramenta.Tela));
        Assert.IsFalse(Ferramentas.Consumivel(Ferramenta.SacoDeLixo));
        Assert.AreEqual(5, Ferramentas.Todas.Length);
    }
}
