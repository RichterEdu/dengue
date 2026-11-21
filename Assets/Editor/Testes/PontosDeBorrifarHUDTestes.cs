using NUnit.Framework;
using UnityEngine;

/* ============================================================
 *  PontosDeBorrifarHUDTestes.cs
 *  ---------------------------------------------------------------------
 *  Testes EditMode da interface das bolinhas da bromélia: a conversão do
 *  campo para o Canvas (NaTela), o tamanho do anel (EscalaDoAnel), as
 *  alturas da dica e da contagem (acima da barra do kit, sem encostar uma
 *  na outra nem na bolinha mais baixa) e a DengueHUD escondendo a mira e o
 *  painel no skill check e na vista de perto.
 *
 *  A montagem (Start/LateUpdate) não roda no EditMode: é conferida em Play.
 *
 *  Rodar:  unity test "<projeto>" --mode EditMode --filter PontosDeBorrifarHUDTestes
 * ============================================================ */

[Category("Minigames")]
public class PontosDeBorrifarHUDTestes
{
    private const float Tol = 1e-4f;

    private static void Igual(Vector2 esperado, Vector2 obtido)
    {
        Assert.AreEqual(esperado.x, obtido.x, Tol, "x");
        Assert.AreEqual(esperado.y, obtido.y, Tol, "y");
    }

    [Test]
    public void NaTela_BordasDoCampo()
    {
        Igual(new Vector2(540f, -540f), PontosDeBorrifarHUD.NaTela(new Vector2(1f, -1f), 1080f));
        Igual(Vector2.zero, PontosDeBorrifarHUD.NaTela(Vector2.zero, 1080f));
        Igual(new Vector2(180f, 0f), PontosDeBorrifarHUD.NaTela(new Vector2(0.5f, 0f), 720f));
    }

    [Test]
    public void EscalaDoAnel_DeCheioAFechado()
    {
        Assert.AreEqual(2.2f, PontosDeBorrifarHUD.EscalaDoAnel(1f), Tol);
        Assert.AreEqual(1f, PontosDeBorrifarHUD.EscalaDoAnel(0f), Tol);
        Assert.AreEqual(1.6f, PontosDeBorrifarHUD.EscalaDoAnel(0.5f), Tol);
        Assert.AreEqual(2.2f, PontosDeBorrifarHUD.EscalaDoAnel(2f), Tol);
        Assert.AreEqual(1f, PontosDeBorrifarHUD.EscalaDoAnel(-1f), Tol);
    }

    [Test]
    public void DicaAcimaDaBarraDoKit()
    {
        // A barra do kit: 40 px da borda de baixo + o espaço de 96 px com a escala 1,1 da selecionada.
        Assert.Greater(PontosDeBorrifarHUD.AlturaDaDica - PontosDeBorrifarHUD.AlturaDaFaixa / 2f, 40f + 96f * 1.1f);
    }

    [Test]
    public void ContagemAcimaDaDica()
    {
        Assert.Greater(PontosDeBorrifarHUD.AlturaDaContagem - PontosDeBorrifarHUD.TamanhoDoPonto / 2f,
                       PontosDeBorrifarHUD.AlturaDaDica + PontosDeBorrifarHUD.AlturaDaFaixa / 2f);
    }

    [Test]
    public void BolinhaMaisBaixaNaoEncostaNaContagem()
    {
        // 0,42 é o teto de quão baixo uma bolinha pode ficar (Task 5).
        Assert.Greater(540f - (0.42f + PontosDeBorrifar.RaioPadrao) * 540f,
                       PontosDeBorrifarHUD.AlturaDaContagem + PontosDeBorrifarHUD.TamanhoDoPonto / 2f);
    }

    [Test]
    public void EscondeMiraEPainel()
    {
        Assert.IsFalse(DengueHUD.EscondeMiraEPainel(false, false));
        Assert.IsTrue(DengueHUD.EscondeMiraEPainel(true, false));
        Assert.IsTrue(DengueHUD.EscondeMiraEPainel(false, true));
        Assert.IsTrue(DengueHUD.EscondeMiraEPainel(true, true));
    }
}
