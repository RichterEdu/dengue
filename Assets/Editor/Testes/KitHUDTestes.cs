using NUnit.Framework;

/* ============================================================
 *  KitHUDTestes.cs
 *  ---------------------------------------------------------------------
 *  Testes EditMode (Parte A) da barra de ferramentas (KitHUD): o texto
 *  de quantidade de cada espaço.
 *
 *    - consumível (areia, tela, água sanitária): "quantidade/capacidade";
 *    - saco de lixo: "garrafas no saco/capacidade do saco";
 *    - mãos: vazio (não gastam nada).
 *
 *  TextoQuantidade é estático e só lê o EstoqueDoKit (C# puro): não
 *  precisa de GameObject nem de Canvas, então aqui nem entra o TesteUtil.
 *  O estoque é o padrão do kit: areia 1, tela 1, água sanitária 2 doses
 *  e saco de 6 garrafas.
 *
 *  Rodar:  Window > General > Test Runner > EditMode (categoria ParteA)
 *  ou      unity command run_tests --mode EditMode --filter_type category --filter ParteA
 * ============================================================ */

[Category("ParteA")]
public class KitHUDTestes
{
    [Test]
    public void TextoQuantidade_PorFerramenta()
    {
        EstoqueDoKit estoque = new EstoqueDoKit(1, 1, 2, 6);
        Assert.IsTrue(estoque.GuardarGarrafa());

        Assert.AreEqual("1/1", KitHUD.TextoQuantidade(estoque, Ferramenta.Areia));
        Assert.AreEqual("2/2", KitHUD.TextoQuantidade(estoque, Ferramenta.AguaSanitaria));
        Assert.AreEqual("1/6", KitHUD.TextoQuantidade(estoque, Ferramenta.SacoDeLixo));
        Assert.AreEqual("", KitHUD.TextoQuantidade(estoque, Ferramenta.Maos));
    }
}
