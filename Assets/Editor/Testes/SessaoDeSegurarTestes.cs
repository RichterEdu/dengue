using NUnit.Framework;

/* ============================================================
 *  SessaoDeSegurarTestes.cs
 *  ---------------------------------------------------------------------
 *  Testes EditMode (Parte A) da SessaoDeSegurar: como o tempo segurado
 *  vira passos. A sessão é C# pura e o alvo é só um object, então os
 *  alvos aqui são new object() (nada de GameObject nem de TesteUtil).
 *
 *  Quase todos usam 0,5 s por passo; o último usa 0 s (um passo por
 *  aperto).
 *
 *  Rodar:  Window > General > Test Runner > EditMode (categoria ParteA)
 *  ou      unity command run_tests --mode EditMode --filter_type category --filter ParteA
 * ============================================================ */

[Category("ParteA")]
public class SessaoDeSegurarTestes
{
    private SessaoDeSegurar sessao;
    private object a;
    private object b;

    [SetUp]
    public void Preparar()
    {
        sessao = new SessaoDeSegurar();
        a = new object();
        b = new object();
    }

    [Test]
    public void MeioPasso_NaoAvanca()
    {
        Assert.AreEqual(0, sessao.Atualizar(a, true, 0.25f, 0.5f));
        Assert.That(sessao.Fracao, Is.EqualTo(0.5f).Within(1e-4f));
    }

    [Test]
    public void PassoInteiro_AvancaUm()
    {
        int passos = sessao.Atualizar(a, true, 0.25f, 0.5f);
        passos += sessao.Atualizar(a, true, 0.25f, 0.5f);

        Assert.AreEqual(1, passos);
    }

    [Test]
    public void DtGrande_AvancaVarios()
    {
        Assert.AreEqual(2, sessao.Atualizar(a, true, 1.1f, 0.5f));
        Assert.That(sessao.Fracao, Is.EqualTo(0.2f).Within(1e-4f), "resto de 0,1 s guardado");
    }

    [Test]
    public void TrocarDeAlvo_ZeraTempo()
    {
        sessao.Atualizar(a, true, 0.4f, 0.5f);

        // Se os 0,4 s de 'a' passassem para 'b', 0,6 s dariam um passo.
        Assert.AreEqual(0, sessao.Atualizar(b, true, 0.2f, 0.5f));
        Assert.That(sessao.Fracao, Is.EqualTo(0.4f).Within(1e-4f));
        Assert.AreSame(b, sessao.Alvo);
    }

    [Test]
    public void Soltar_ZeraFracao()
    {
        sessao.Atualizar(a, true, 0.4f, 0.5f);

        Assert.AreEqual(0, sessao.Atualizar(a, false, 0.1f, 0.5f), "sem segurar não avança");
        Assert.That(sessao.Fracao, Is.EqualTo(0f).Within(1e-4f));

        // Voltou a segurar o mesmo alvo: começa do zero (0,2 s, não 0,6 s).
        Assert.AreEqual(0, sessao.Atualizar(a, true, 0.2f, 0.5f));
    }

    [Test]
    public void DtZero_NaoAvanca()
    {
        Assert.AreEqual(0, sessao.Atualizar(a, true, 0f, 0.5f));
    }

    [Test]
    public void SegundosZero_UmPassoPorAperto()
    {
        Assert.AreEqual(1, sessao.Atualizar(a, true, 0.016f, 0f), "primeiro quadro do aperto");
        Assert.AreEqual(0, sessao.Atualizar(a, true, 0.016f, 0f), "continua segurando");

        sessao.Atualizar(a, false, 0.016f, 0f);
        Assert.AreEqual(1, sessao.Atualizar(a, true, 0.016f, 0f), "soltou e apertou de novo");
    }
}
