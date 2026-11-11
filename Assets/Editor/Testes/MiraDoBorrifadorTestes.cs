using NUnit.Framework;
using UnityEngine;

/* ============================================================
 *  MiraDoBorrifadorTestes.cs
 *  ---------------------------------------------------------------------
 *  Testes EditMode da lógica pura da mira do borrifador: mira presa ao
 *  cone, só seca segurando e dentro do raio, a mais próxima primeiro,
 *  umidade guardada ao soltar, passos nas contagens certas (um por
 *  Atualizar), FracaoDoPasso em 0..1 e entradas ruins (dt, olhar,
 *  arrays) sem estragar a sessão.
 *
 *  Rodar:  unity test "<projeto>" --mode EditMode --filter MiraDoBorrifadorTestes
 * ============================================================ */

[Category("Minigames")]
public class MiraDoBorrifadorTestes
{
    private const float Quadro = 1f / 60f;

    private static MiraDoBorrifador Padrao()
    {
        return new MiraDoBorrifador(
            new[] { new Vector2(0f, 0f), new Vector2(10f, 0f), new Vector2(0f, 10f) },
            new[] { 0.5f, 0.5f, 0.5f }, 3);
    }

    private static EstadoDoMinigame Frame(MiraDoBorrifador m, Vector2 olhar, bool segurando, float dt = Quadro)
    {
        EntradaDoMinigame entrada = new EntradaDoMinigame();
        entrada.Olhar = olhar;
        entrada.Segurando = segurando;
        return m.Atualizar(dt, entrada);
    }

    /* Leva a mira até (x, y) em um quadro, sem segurar (Mira.y -= Olhar.y). */
    private static void Ir(MiraDoBorrifador m, float x, float y)
    {
        Frame(m, new Vector2(x - m.Mira.x, -(y - m.Mira.y)), false);
    }

    /* Segura parado por 'segundos' (quadros de 1/60 s); devolve quantos Passo saíram. */
    private static int Segurar(MiraDoBorrifador m, float segundos)
    {
        int passos = 0;
        int quadros = Mathf.RoundToInt(segundos / Quadro);
        for (int i = 0; i < quadros; i++)
            if (Frame(m, Vector2.zero, true) == EstadoDoMinigame.Passo) passos++;
        return passos;
    }

    /* Segura parado até uma pocinha secar; devolve o estado da chamada em que isso ocorreu. */
    private static EstadoDoMinigame AteSecar(MiraDoBorrifador m)
    {
        int antes = m.Secas;
        for (int i = 0; i < 300; i++)
        {
            EstadoDoMinigame e = Frame(m, Vector2.zero, true);
            if (m.Secas > antes) return e;
        }
        Assert.Fail("nenhuma pocinha secou");
        return EstadoDoMinigame.EmAndamento;
    }

    [Test]
    public void MiraPresaAoCone()
    {
        MiraDoBorrifador m = Padrao();
        Frame(m, new Vector2(100f, 0f), false);
        Assert.LessOrEqual(m.Mira.magnitude, 30f + 1e-4f);
        Frame(m, new Vector2(-100f, 100f), false);
        Assert.LessOrEqual(m.Mira.magnitude, 30f + 1e-4f);
    }

    [Test]
    public void ConeCrescePraAlcancarTodasAsPocinhas()
    {
        MiraDoBorrifador m = new MiraDoBorrifador(new[] { new Vector2(50f, 0f) }, new[] { 0.5f }, 1);
        Frame(m, new Vector2(1000f, 0f), false);
        Assert.AreEqual(56f, m.Mira.magnitude, 1e-3f, "cone efetivo = 50 + raio 4 + 2");
        Ir(m, 50f, 0f);
        Segurar(m, 1f);
        Assert.AreEqual(1, m.Secas);
    }

    [Test]
    public void ConvencaoDaMira_YInvertido()
    {
        MiraDoBorrifador m = Padrao();
        Frame(m, new Vector2(5f, 3f), false);
        Assert.AreEqual(5f, m.Mira.x, 1e-4f);
        Assert.AreEqual(-3f, m.Mira.y, 1e-4f);
    }

    [Test]
    public void SegurarForaNaoSeca()
    {
        MiraDoBorrifador m = Padrao();
        Ir(m, 20f, 20f);
        Segurar(m, 2f);
        Assert.AreEqual(0, m.Secas);
        Assert.AreEqual(-1, m.PocaSecandoAgora);
    }

    [Test]
    public void SemSegurarNaoSeca()
    {
        MiraDoBorrifador m = Padrao();
        for (int i = 0; i < 120; i++) Frame(m, Vector2.zero, false);
        Assert.AreEqual(1f, m.Umidade(0));
        Assert.AreEqual(-1, m.PocaSecandoAgora);
    }

    [Test]
    public void SecaAMaisProxima()
    {
        MiraDoBorrifador m = Padrao();
        Ir(m, 1f, 0f);
        int passos = Segurar(m, 0.6f);
        Assert.AreEqual(0f, m.Umidade(0));
        Assert.AreEqual(1f, m.Umidade(1));
        Assert.AreEqual(1, m.Secas);
        Assert.AreEqual(1, passos);
    }

    [Test]
    public void EscolheAMaisProximaDentroDoRaio()
    {
        MiraDoBorrifador m = new MiraDoBorrifador(
            new[] { new Vector2(0f, 0f), new Vector2(3f, 0f) }, new[] { 0.5f, 0.5f }, 2);
        Ir(m, 2f, 0f);
        Frame(m, Vector2.zero, true);
        Assert.AreEqual(1, m.PocaSecandoAgora);
        Assert.Less(m.Umidade(1), 1f);
        Assert.AreEqual(1f, m.Umidade(0));
    }

    [Test]
    public void SoltarNoMeioGuardaAUmidade()
    {
        MiraDoBorrifador m = Padrao();
        Ir(m, 0f, 0f);
        Segurar(m, 0.25f);
        Assert.That(m.Umidade(0), Is.InRange(0.4f, 0.6f));
        float guardada = m.Umidade(0);

        for (int i = 0; i < 60; i++) Frame(m, Vector2.zero, false);
        Assert.AreEqual(guardada, m.Umidade(0));
        Assert.AreEqual(-1, m.PocaSecandoAgora);

        Segurar(m, 0.3f);
        Assert.AreEqual(0f, m.Umidade(0));
        Assert.AreEqual(1, m.Secas);
    }

    [Test]
    public void PassosNasContagensCertas()
    {
        Vector2[] pos = { new Vector2(0f, 0f), new Vector2(10f, 0f), new Vector2(20f, 0f),
                          new Vector2(0f, 10f), new Vector2(0f, 20f) };
        float[] seg = { 0.5f, 0.5f, 0.5f, 0.5f, 0.5f };
        MiraDoBorrifador m = new MiraDoBorrifador(pos, seg, 2);

        bool[] saiuNaSeca = new bool[5];
        for (int i = 0; i < 5; i++)
        {
            Ir(m, pos[i].x, pos[i].y);
            saiuNaSeca[i] = AteSecar(m) == EstadoDoMinigame.Passo;
        }
        Assert.AreEqual(new[] { false, false, true, false, true }, saiuNaSeca, "Passo ao secar a 3ª e a 5ª");
    }

    [Test]
    public void MaisPassosQuePocinhas()
    {
        MiraDoBorrifador m = new MiraDoBorrifador(
            new[] { new Vector2(0f, 0f), new Vector2(10f, 0f) }, new[] { 0.5f, 0.5f }, 5);
        int total = 0;

        /* 1ª seca: limiares 1,1,2,2,2 -> 2 passos devidos, um por chamada. */
        Ir(m, 0f, 0f);
        Assert.AreEqual(EstadoDoMinigame.Passo, AteSecar(m)); total++;
        Assert.AreEqual(EstadoDoMinigame.Passo, Frame(m, Vector2.zero, false)); total++;
        Assert.AreEqual(EstadoDoMinigame.EmAndamento, Frame(m, Vector2.zero, false));

        /* 2ª seca: 3 passos devidos, um por chamada (o 1º na própria chamada que seca). */
        Ir(m, 10f, 0f);
        Assert.AreEqual(EstadoDoMinigame.Passo, AteSecar(m)); total++;
        Assert.AreEqual(EstadoDoMinigame.Passo, Frame(m, Vector2.zero, false)); total++;
        Assert.AreEqual(EstadoDoMinigame.Passo, Frame(m, Vector2.zero, true)); total++;
        Assert.AreEqual(EstadoDoMinigame.EmAndamento, Frame(m, Vector2.zero, false));

        Assert.AreEqual(5, total);
        for (int i = 0; i < 10; i++)
            Assert.AreEqual(EstadoDoMinigame.EmAndamento, Frame(m, Vector2.zero, true), "nada além dos 5 passos");
    }

    [Test]
    public void SemPocinhasTodosOsPassosSaem()
    {
        MiraDoBorrifador m = new MiraDoBorrifador(new Vector2[0], new float[0], 3);
        Assert.AreEqual(0, m.Total);
        Assert.AreEqual(EstadoDoMinigame.Passo, Frame(m, Vector2.zero, false));
        Assert.AreEqual(EstadoDoMinigame.Passo, Frame(m, Vector2.zero, true));
        Assert.AreEqual(EstadoDoMinigame.Passo, Frame(m, Vector2.zero, false));
        for (int i = 0; i < 5; i++)
            Assert.AreEqual(EstadoDoMinigame.EmAndamento, Frame(m, Vector2.zero, true));
    }

    [Test]
    public void FracaoDoPasso_FicaEmZeroAUm()
    {
        MiraDoBorrifador m = Padrao();
        float maior = 0f;
        Assert.AreEqual(0f, m.FracaoDoPasso);
        Ir(m, 0f, 0f);
        for (int i = 0; i < 40; i++)
        {
            EstadoDoMinigame e = Frame(m, Vector2.zero, true);
            Assert.That(m.FracaoDoPasso, Is.InRange(0f, 1f));
            maior = Mathf.Max(maior, m.FracaoDoPasso);
            if (e == EstadoDoMinigame.Passo)
                Assert.AreEqual(0f, m.FracaoDoPasso, 1e-5f, "logo depois de um Passo a fração volta a 0");
        }
        Assert.Greater(maior, 0.5f, "a fração sobe enquanto a pocinha seca");
    }

    [Test]
    public void DtInvalidoNaoMudaNada()
    {
        MiraDoBorrifador m = Padrao();
        Ir(m, 1f, 0f);
        Segurar(m, 0.2f);
        float umidade = m.Umidade(0);
        Vector2 mira = m.Mira;

        foreach (float dt in new[] { 0f, -1f, float.NaN })
        {
            EstadoDoMinigame e = Frame(m, new Vector2(5f, 5f), true, dt);
            Assert.AreEqual(EstadoDoMinigame.EmAndamento, e);
            Assert.AreEqual(umidade, m.Umidade(0));
            Assert.AreEqual(mira, m.Mira);
        }
    }

    [Test]
    public void DtEnorme_SecaNoMaximoUmaPocinha()
    {
        MiraDoBorrifador m = Padrao();
        Ir(m, 0f, 0f);
        Frame(m, Vector2.zero, true, 1e6f);
        Assert.AreEqual(1, m.Secas);
        Frame(m, Vector2.zero, true, float.PositiveInfinity);
        Assert.AreEqual(1, m.Secas, "sem pocinha molhada no raio, nada seca");
        Assert.IsTrue(float.IsFinite(m.Umidade(1)));
    }

    [Test]
    public void OlharNaoFinitoContaComoParado()
    {
        MiraDoBorrifador m = Padrao();
        Frame(m, new Vector2(float.NaN, 0f), false);
        Frame(m, new Vector2(float.PositiveInfinity, float.NegativeInfinity), false);
        Assert.IsTrue(float.IsFinite(m.Mira.x) && float.IsFinite(m.Mira.y));
        Assert.AreEqual(Vector2.zero, m.Mira);

        Ir(m, 0f, 0f);
        Segurar(m, 1f);
        Assert.Greater(m.Secas, 0, "depois da entrada ruim a sessão ainda funciona");
    }

    [Test]
    public void ArraysDeTamanhosDiferentes()
    {
        MiraDoBorrifador m = new MiraDoBorrifador(
            new[] { new Vector2(0f, 0f), new Vector2(10f, 0f), new Vector2(0f, 10f) },
            new[] { 0.5f, 0.5f }, 2);
        Assert.AreEqual(2, m.Total);
    }

    [Test]
    public void ArgumentosRuins_ViramPadrao()
    {
        Assert.AreEqual(0, new MiraDoBorrifador(null, null, 1).Total);
        Assert.AreEqual(0f, new MiraDoBorrifador(null, null, 1).Umidade(0));

        /* tempo 0 vira 0,01 s; passos 0 vira 1; índice fora devolve 0 */
        MiraDoBorrifador m = new MiraDoBorrifador(
            new[] { new Vector2(0f, 0f), new Vector2(10f, 0f) }, new[] { 0f, float.NaN }, 0);
        Assert.AreEqual(0f, m.Umidade(-1));
        Assert.AreEqual(0f, m.Umidade(2));
        Ir(m, 0f, 0f);
        Segurar(m, 0.05f);
        Assert.AreEqual(1, m.Secas);
        Ir(m, 10f, 0f);
        int passos = Segurar(m, 0.05f);
        Assert.AreEqual(2, m.Secas);
        Assert.AreEqual(1, passos, "com passos = 1 só sai um Passo, ao secar a última");
    }

    [Test]
    public void AgindoSoSegurando()
    {
        MiraDoBorrifador m = Padrao();
        Assert.IsFalse(m.Agindo, "antes do primeiro quadro");

        Ir(m, 20f, 20f);
        Frame(m, Vector2.zero, true);
        Assert.IsTrue(m.Agindo, "segurando, mesmo fora das pocinhas");

        Frame(m, Vector2.zero, false);
        Assert.IsFalse(m.Agindo, "soltou");

        Frame(m, Vector2.zero, true);
        Frame(m, Vector2.zero, true, float.NaN);
        Assert.IsFalse(m.Agindo, "dt inválido");
    }

    [Test]
    public void ArraysSaoCopiados()
    {
        Vector2[] pos = { new Vector2(0f, 0f) };
        MiraDoBorrifador m = new MiraDoBorrifador(pos, new[] { 0.5f }, 1);
        pos[0] = new Vector2(25f, 25f);
        Segurar(m, 1f);
        Assert.AreEqual(1, m.Secas, "mexer no array depois não muda a pocinha");
    }
}
