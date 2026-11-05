using NUnit.Framework;

/* ============================================================
 *  CicloDeVidaTestes.cs
 *  ---------------------------------------------------------------------
 *  Testes EditMode (Parte B) do CicloDeVida: ovo → larva → mosquito.
 *  O ciclo é C# puro, então aqui nem entra o TesteUtil.
 *
 *  Todos usam 75 s por estágio (o padrão do jogo).
 *
 *  Rodar:  Window > General > Test Runner > EditMode (categoria ParteB)
 *  ou      unity command run_tests --mode EditMode --filter_type category --filter ParteB
 * ============================================================ */

[Category("ParteB")]
public class CicloDeVidaTestes
{
    private CicloDeVida ciclo;

    [SetUp]
    public void Preparar()
    {
        ciclo = new CicloDeVida(75f);
    }

    [Test]
    public void Comeca_OvoZero()
    {
        Assert.AreEqual(EstagioDoFoco.Ovo, ciclo.Estagio);
        Assert.That(ciclo.TempoNoEstagio, Is.EqualTo(0f).Within(1e-4f));
    }

    [Test]
    public void Avancar_74s_ContinuaOvo_DevolveFalse()
    {
        Assert.IsFalse(ciclo.Avancar(74f));
        Assert.AreEqual(EstagioDoFoco.Ovo, ciclo.Estagio);
        Assert.That(ciclo.TempoNoEstagio, Is.EqualTo(74f).Within(1e-4f));
    }

    [Test]
    public void Avancar_75s_ViraLarva_DevolveTrue()
    {
        Assert.IsTrue(ciclo.Avancar(75f));
        Assert.AreEqual(EstagioDoFoco.Larva, ciclo.Estagio);
        Assert.That(ciclo.TempoNoEstagio, Is.EqualTo(0f).Within(1e-4f), "a larva começa do zero");
    }

    [Test]
    public void Avancar_160s_DeUmaVez_ViraMosquito()
    {
        // 75 s de ovo + 75 s de larva cabem num dt só.
        Assert.IsTrue(ciclo.Avancar(160f));
        Assert.AreEqual(EstagioDoFoco.Mosquito, ciclo.Estagio);
    }

    [Test]
    public void Mosquito_EhOFim()
    {
        ciclo.Definir(EstagioDoFoco.Mosquito, 0f);

        Assert.IsFalse(ciclo.Avancar(1000f));
        Assert.AreEqual(EstagioDoFoco.Mosquito, ciclo.Estagio);
    }

    [Test]
    public void Zerar_VoltaOvo()
    {
        ciclo.Definir(EstagioDoFoco.Mosquito, 20f);

        ciclo.Zerar();

        Assert.AreEqual(EstagioDoFoco.Ovo, ciclo.Estagio);
        Assert.That(ciclo.TempoNoEstagio, Is.EqualTo(0f).Within(1e-4f));
    }

    [Test]
    public void Reencher_VoltaLarva()
    {
        ciclo.Avancar(40f);   // ovo com 40 s

        ciclo.Reencher();

        Assert.AreEqual(EstagioDoFoco.Larva, ciclo.Estagio);
        Assert.That(ciclo.TempoNoEstagio, Is.EqualTo(0f).Within(1e-4f));
    }

    [Test]
    public void Definir_GuardaTempo()
    {
        ciclo.Definir(EstagioDoFoco.Larva, 30f);
        Assert.AreEqual(EstagioDoFoco.Larva, ciclo.Estagio);
        Assert.That(ciclo.TempoNoEstagio, Is.EqualTo(30f).Within(1e-4f));

        // Os 30 s já passados + 45 s fecham os 75 s da larva.
        Assert.IsTrue(ciclo.Avancar(45f));
        Assert.AreEqual(EstagioDoFoco.Mosquito, ciclo.Estagio);
    }

    [Test]
    public void Avancar_80s_SobraFicaNaLarva()
    {
        Assert.IsTrue(ciclo.Avancar(80f));
        Assert.AreEqual(EstagioDoFoco.Larva, ciclo.Estagio);
        Assert.That(ciclo.TempoNoEstagio, Is.EqualTo(5f).Within(1e-4f), "os 5 s que passaram dos 75");
    }

    [Test]
    public void Avancar_DtZeroOuNegativo_NaoMuda()
    {
        // No limite dos 75 s: qualquer dt positivo já viraria larva.
        ciclo.Definir(EstagioDoFoco.Ovo, 75f);

        Assert.IsFalse(ciclo.Avancar(0f), "dt zero");
        Assert.IsFalse(ciclo.Avancar(-10f), "dt negativo");
        Assert.AreEqual(EstagioDoFoco.Ovo, ciclo.Estagio);
        Assert.That(ciclo.TempoNoEstagio, Is.EqualTo(75f).Within(1e-4f), "o tempo não anda nem volta");
    }
}
