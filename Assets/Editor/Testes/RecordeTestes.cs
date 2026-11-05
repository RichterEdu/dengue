using NUnit.Framework;
using UnityEngine;

/* ============================================================
 *  RecordeTestes.cs
 *  ---------------------------------------------------------------------
 *  O recorde guardado em PlayerPrefs: só sobe com nota maior, estrelas
 *  independentes dos pontos, valores inválidos presos na faixa e o
 *  prefixo de teste sem tocar nas chaves reais.
 * ============================================================ */

[Category("ParteC")]
public class RecordeTestes
{
    private RecordeDoJogo recorde;

    [SetUp]
    public void Preparar()
    {
        recorde = new RecordeDoJogo("teste.recorde.");
        recorde.Apagar();
    }

    [TearDown]
    public void Limpar()
    {
        recorde.Apagar();
        TesteUtil.LimparTudo();
    }

    [Test]
    public void Registrar_SoSobeComNotaMaior()
    {
        Assert.IsTrue(recorde.Registrar(1000, 1));
        Assert.IsFalse(recorde.Registrar(1000, 1));
        Assert.IsFalse(recorde.Registrar(900, 0));
        Assert.AreEqual(1000, recorde.Pontos);
    }

    [Test]
    public void Estrelas_IndependemDosPontos()
    {
        recorde.Registrar(6000, 1);
        recorde.Registrar(5000, 3);
        Assert.AreEqual(6000, recorde.Pontos);
        Assert.AreEqual(3, recorde.Estrelas);
    }

    [Test]
    public void NotaZero_NaoERecorde() => Assert.IsFalse(recorde.Registrar(0, 0));

    [Test]
    public void ValoresInvalidos_FicamNaFaixa()
    {
        PlayerPrefs.SetInt("teste.recorde.pontos", -50);
        PlayerPrefs.SetInt("teste.recorde.estrelas", 9);
        Assert.AreEqual(0, recorde.Pontos);
        Assert.AreEqual(3, recorde.Estrelas);
    }

    [Test]
    public void PrefixoDeTeste_NaoTocaNasChavesReais()
    {
        int antes = new RecordeDoJogo().Pontos;
        recorde.Registrar(antes + 1000, 3);
        Assert.AreEqual(antes, new RecordeDoJogo().Pontos);
    }
}
