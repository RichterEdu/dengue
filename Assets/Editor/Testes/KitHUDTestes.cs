using NUnit.Framework;

/* ============================================================
 *  KitHUDTestes.cs
 *  ---------------------------------------------------------------------
 *  Testes EditMode (Parte A) da barra de ferramentas (KitHUD): o texto
 *  de quantidade de cada espaço e a linha de carga acima da barra.
 *
 *    - consumível (areia, tela, água sanitária): "quantidade/capacidade";
 *    - saco de lixo: "garrafas no saco/capacidade do saco";
 *    - mãos: vazio (não gastam nada);
 *    - linha de carga (TextoDaLinha, com a stamina): a do pneu nos
 *      braços, a da tampa da caixa d'água nos braços, e nenhuma com as
 *      mãos livres.
 *
 *  TextoQuantidade é estático e só lê o EstoqueDoKit (C# puro): não
 *  precisa de GameObject nem de Canvas. O estoque é o padrão do kit:
 *  areia 1, tela 1, água sanitária 2 doses e saco de 6 garrafas. A linha
 *  de carga lê o KitDoAgente: o kit fica num objeto com
 *  ControladorJogador, criado pelo TesteUtil.
 *
 *  Rodar:  Window > General > Test Runner > EditMode (categoria ParteA)
 *  ou      unity command run_tests --mode EditMode --filter_type category --filter ParteA
 * ============================================================ */

[Category("ParteA")]
public class KitHUDTestes
{
    [TearDown]
    public void Limpar()
    {
        TesteUtil.LimparTudo();
    }

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

    private static KitDoAgente NovoKit()
    {
        KitDoAgente kit = TesteUtil.Com<ControladorJogador>("Jogador (teste)").gameObject.AddComponent<KitDoAgente>();
        kit.PontoDeCarga = TesteUtil.Novo("Ponto de carga (teste)").transform;
        return kit;
    }

    [Test]
    public void LinhaDeCarga_MaosLivresNaoTem()
    {
        Assert.IsNull(KitHUD.TextoDaLinha(NovoKit()));
        Assert.IsNull(KitHUD.TextoDaLinha(null), "sem kit");
    }

    [Test]
    public void LinhaDeCarga_ComOPneu()
    {
        KitDoAgente kit = NovoKit();
        PneuInteractable pneu = TesteUtil.Com<PneuInteractable>("Pneu (teste)");
        pneu.transform.position = new UnityEngine.Vector3(0f, -10000f, 0f);
        TesteUtil.EsvaziarPneu(pneu, kit, false);   // esvaziado nos braços

        Assert.AreEqual(KitHUD.TextoLinhaPneu, KitHUD.TextoDaLinha(kit));
    }

    [Test]
    public void LinhaDeCarga_PneuComAgua()
    {
        KitDoAgente kit = NovoKit();
        PneuInteractable pneu = TesteUtil.Com<PneuInteractable>("Pneu (teste)");
        pneu.transform.position = new UnityEngine.Vector3(0f, -10000f, 0f);
        Assert.IsTrue(kit.Pegar(pneu));

        Assert.AreEqual(KitHUD.TextoLinhaPneuComAgua(100), KitHUD.TextoDaLinha(kit));
        Assert.AreEqual("Pneu com água (100%): balance a câmera para esvaziar. [Botão direito] larga.", KitHUD.TextoDaLinha(kit));
    }

    [Test]
    public void LinhaDeCarga_ComATampa()
    {
        KitDoAgente kit = NovoKit();
        CaixaDaguaInteractable caixa = TesteUtil.Com<CaixaDaguaInteractable>("Caixa d'água (teste)");
        Assert.IsTrue(kit.PegarTampa(caixa));

        Assert.AreEqual("Carregando a tampa. Tampe a caixa d'água. [Botão direito] devolve.", KitHUD.TextoDaLinha(kit));
    }
}
