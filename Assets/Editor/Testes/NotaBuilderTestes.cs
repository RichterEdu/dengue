using NUnit.Framework;
using UnityEngine;

/* ============================================================
 *  NotaBuilderTestes.cs
 *  ---------------------------------------------------------------------
 *  Testes EditMode (Parte C) da estrela que o NotaBuilder desenha: a
 *  cheia tem o miolo preenchido, a vazia é só o contorno, e o canto fica
 *  transparente nas duas. A montagem da cena é conferida rodando o menu
 *  Dengue > Configurar Nota e Recorde.
 *
 *  Rodar:  Window > General > Test Runner > EditMode (categoria ParteC)
 *  ou      unity command run_tests --mode EditMode --filter_type category --filter ParteC
 * ============================================================ */

[Category("ParteC")]
public class NotaBuilderTestes
{
    [Test]
    public void GerarEstrela_CheiaTemCentro_VaziaNao()
    {
        Assert.Greater(Alfa(true, 96, 48, 48), 0.9f);
        Assert.Less(Alfa(false, 96, 48, 48), 0.1f);
    }

    [Test]
    public void GerarEstrela_CantoTransparente()
    {
        Assert.Less(Alfa(true, 96, 0, 0), 0.01f);
    }

    private static float Alfa(bool cheia, int tamanho, int x, int y)
    {
        Texture2D estrela = NotaBuilder.GerarEstrela(cheia, tamanho);
        float alfa = estrela.GetPixel(x, y).a;
        Object.DestroyImmediate(estrela);
        return alfa;
    }
}
