using System;
using NUnit.Framework;
using UnityEngine;

/* ============================================================
 *  OndaDoPneuTestes.cs
 *  ---------------------------------------------------------------------
 *  Testes EditMode da lógica pura do minigame do pneu: parado não derrama,
 *  empurrar em fase esvazia (em 3 a 7 s), contra-fase e abandono não passam
 *  da borda, quadro longo não explode, FracaoDoPasso fica em 0..1.
 *
 *  Rodar:  unity test "<projeto>" --mode EditMode --filter OndaDoPneuTestes
 * ============================================================ */

[Category("Minigames")]
public class OndaDoPneuTestes
{
    private const float Quadro = 1f / 60f;
    private const float GrausPorSegundoCheio = 180f;

    /* Anda em quadros de 1/60 s; u (-1..1) vira o olhar em graus por quadro.
     * Devolve quantos Passo saíram; aoQuadro roda depois de cada Atualizar. */
    private static int Simular(OndaDoPneu onda, float segundos, Func<OndaDoPneu, float> u,
                               Action<OndaDoPneu, EstadoDoMinigame> aoQuadro = null)
    {
        int passos = 0;
        int quadros = Mathf.RoundToInt(segundos / Quadro);
        for (int i = 0; i < quadros; i++)
        {
            EntradaDoMinigame entrada = new EntradaDoMinigame();
            entrada.Olhar = new Vector2(u(onda) * GrausPorSegundoCheio / 60f, 0f);
            EstadoDoMinigame estado = onda.Atualizar(Quadro, entrada);
            if (estado == EstadoDoMinigame.Passo) passos++;
            aoQuadro?.Invoke(onda, estado);
        }
        return passos;
    }

    private static float EmFase(OndaDoPneu o) => o.Velocidade >= 0f ? 1f : -1f;
    private static float ContraFase(OndaDoPneu o) => o.Velocidade >= 0f ? -1f : 1f;

    [Test]
    public void Parada_NaoDerrama()
    {
        OndaDoPneu onda = new OndaDoPneu(3);
        int passos = Simular(onda, 10f, o => 0f);
        Assert.AreEqual(0, passos);
        Assert.AreEqual(1f, onda.Nivel);
    }

    [Test]
    public void EmFase_Esvazia()
    {
        OndaDoPneu onda = new OndaDoPneu(3);
        float tempo = 0f;
        float tempoAteVazio = -1f;
        int passos = Simular(onda, 10f, EmFase, (o, estado) =>
        {
            tempo += Quadro;
            if (tempoAteVazio < 0f && o.Nivel <= 0f) tempoAteVazio = tempo;
        });

        Assert.AreEqual(3, passos);
        Assert.AreEqual(0f, onda.Nivel);
        Assert.That(tempoAteVazio, Is.InRange(3f, 7f), "tempo simulado até esvaziar");
    }

    [Test]
    public void ContraFase_NaoPassaDaBorda()
    {
        OndaDoPneu onda = new OndaDoPneu(3);
        float maior = 0f;
        int passos = Simular(onda, 10f, ContraFase, (o, estado) => maior = Mathf.Max(maior, Mathf.Abs(o.Inclinacao)));
        Assert.Less(maior, 1f);
        Assert.AreEqual(0, passos);
    }

    [Test]
    public void OndaMorreSozinha()
    {
        OndaDoPneu onda = new OndaDoPneu(3);
        Simular(onda, 2f, EmFase);
        Simular(onda, 3f, o => 0f);
        Assert.Less(Mathf.Abs(onda.Inclinacao), 0.3f);
    }

    [Test]
    public void QuadroLongo_NaoExplode()
    {
        OndaDoPneu onda = new OndaDoPneu(3);
        Simular(onda, 2f, EmFase);
        float nivelAntes = onda.Nivel;

        EntradaDoMinigame entrada = new EntradaDoMinigame();
        entrada.Olhar = new Vector2(GrausPorSegundoCheio * 0.5f, 0f); // u = 1 em dt = 0,5 s
        EstadoDoMinigame estado = onda.Atualizar(0.5f, entrada);

        Assert.Less(Mathf.Abs(onda.Inclinacao), 3f);
        Assert.That(estado, Is.EqualTo(EstadoDoMinigame.EmAndamento).Or.EqualTo(EstadoDoMinigame.Passo));
        Assert.LessOrEqual(nivelAntes - onda.Nivel, 1f / 3f + 1e-5f);
    }

    [Test]
    public void FracaoDoPasso_SobeDentroDoTerco()
    {
        OndaDoPneu onda = new OndaDoPneu(3);
        float maior = 0f;
        Simular(onda, 10f, EmFase, (o, estado) =>
        {
            Assert.That(o.FracaoDoPasso, Is.InRange(0f, 1f));
            maior = Mathf.Max(maior, o.FracaoDoPasso);
            if (estado == EstadoDoMinigame.Passo)
                Assert.Less(o.FracaoDoPasso, 0.05f, "logo depois de um Passo a fração volta perto de 0");
        });
        Assert.Greater(maior, 0.5f, "a fração chegou a subir antes de cada Passo");
    }

    private static bool Finito(OndaDoPneu o) =>
        float.IsFinite(o.Inclinacao) && float.IsFinite(o.Velocidade) && float.IsFinite(o.Nivel);

    [Test]
    public void DtNaoFinito_NaoMudaNada()
    {
        OndaDoPneu onda = new OndaDoPneu(3);
        Simular(onda, 1f, EmFase);
        float x = onda.Inclinacao, v = onda.Velocidade, n = onda.Nivel;

        EntradaDoMinigame entrada = new EntradaDoMinigame();
        entrada.Olhar = new Vector2(3f, 0f);
        EstadoDoMinigame estado = onda.Atualizar(float.NaN, entrada);
        Assert.AreEqual(EstadoDoMinigame.EmAndamento, estado);
        Assert.AreEqual(x, onda.Inclinacao);
        Assert.AreEqual(v, onda.Velocidade);
        Assert.AreEqual(n, onda.Nivel);

        onda.Atualizar(float.PositiveInfinity, entrada);
        Assert.IsTrue(Finito(onda));
    }

    [Test]
    public void OlharNaoFinito_ContaComoParado()
    {
        OndaDoPneu onda = new OndaDoPneu(3);
        EntradaDoMinigame entrada = new EntradaDoMinigame();
        entrada.Olhar = new Vector2(float.NaN, 0f);
        onda.Atualizar(Quadro, entrada);
        entrada.Olhar = new Vector2(float.PositiveInfinity, 0f);
        onda.Atualizar(Quadro, entrada);
        Assert.IsTrue(Finito(onda));

        int passos = Simular(onda, 10f, EmFase);
        Assert.IsTrue(Finito(onda));
        Assert.Greater(passos, 0, "depois da entrada ruim a sessão ainda funciona");
    }

    [Test]
    public void DtEnorme_EhLimitado()
    {
        OndaDoPneu onda = new OndaDoPneu(3);
        EntradaDoMinigame entrada = new EntradaDoMinigame();
        entrada.Olhar = new Vector2(GrausPorSegundoCheio * 1e6f, 0f); // u = 1
        EstadoDoMinigame estado = onda.Atualizar(1e6f, entrada);

        Assert.IsTrue(Finito(onda));
        Assert.LessOrEqual(1f - onda.Nivel, 1f / 3f + 1e-5f);
        Assert.That(estado, Is.EqualTo(EstadoDoMinigame.EmAndamento).Or.EqualTo(EstadoDoMinigame.Passo));
    }
}
