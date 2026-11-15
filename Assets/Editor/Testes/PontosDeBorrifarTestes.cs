using NUnit.Framework;
using UnityEngine;

/* ============================================================
 *  PontosDeBorrifarTestes.cs
 *  ---------------------------------------------------------------------
 *  Testes EditMode da lógica pura do minigame da bromélia (clicar nas
 *  bolinhas): mira presa ao campo e movida pelo Ponteiro, acerto só com
 *  aperto dentro do raio da bolinha da vez, espera inicial, anel que fecha
 *  e devolve a bolinha depois da pausa, passos nas contagens certas (um
 *  por Atualizar), fecho segurando o passo do último acerto, FracaoDoPasso,
 *  Agindo, entradas ruins sem estragar a sessão e PoseDePerto.NoCampo.
 *
 *  Rodar:  unity test "<projeto>" --mode EditMode --filter PontosDeBorrifarTestes
 * ============================================================ */

[Category("Minigames")]
public class PontosDeBorrifarTestes
{
    private const float Q = 1f / 60f;
    private const float Mpg = 0.01f;   // mira por grau nos testes: Ponteiro (10, 0) anda 0,1 do campo
    private const float Tol = 1e-4f;
    private static readonly Vector2[] Tres = { new Vector2(0.4f, 0f), new Vector2(-0.4f, 0.2f), Vector2.zero };
    private static readonly Vector2[] Dois = { new Vector2(0.4f, 0f), new Vector2(-0.4f, 0.2f) };
    private static readonly Vector2[] Cinco =
    {
        new Vector2(0f, 0f), new Vector2(0.5f, 0f), new Vector2(-0.5f, 0f),
        new Vector2(0f, 0.5f), new Vector2(0f, -0.5f)
    };

    private static PontosDeBorrifar Nova(Vector2[] pontos, int passos, float espera = 0f, float fecho = 0f)
        => new PontosDeBorrifar(pontos, passos, 0.14f, 1.5f, 0.35f, espera, fecho, 1.5f, Mpg);

    private static EstadoDoMinigame Quadro(PontosDeBorrifar s, Vector2 ponteiro = default, bool apertou = false, float dt = Q)
        => s.Atualizar(dt, new EntradaDoMinigame { Ponteiro = ponteiro, Apertou = apertou });

    /* Um quadro que leva a mira até 'alvo' (e aperta, se pedido). */
    private static EstadoDoMinigame Ir(PontosDeBorrifar s, Vector2 alvo, bool apertou = false)
        => Quadro(s, (alvo - s.Mira) / Mpg, apertou);

    /* Acerta a bolinha da vez. */
    private static EstadoDoMinigame Acertar(PontosDeBorrifar s)
        => Ir(s, s.Ponto(s.DaVez), true);

    /* Quadros parados (de Q em Q) por 'segundos'; devolve quantos deram Passo. Falha se algum der Errou. */
    private static int Esperar(PontosDeBorrifar s, float segundos)
    {
        int passos = 0;
        int quadros = Mathf.RoundToInt(segundos / Q);
        for (int i = 0; i < quadros; i++)
        {
            EstadoDoMinigame e = Quadro(s);
            Assert.AreNotEqual(EstadoDoMinigame.Errou, e, "a sessão nunca devolve Errou");
            if (e == EstadoDoMinigame.Passo) passos++;
        }
        return passos;
    }

    private static void IgualV(Vector2 esperado, Vector2 obtido, string msg = null)
    {
        Assert.AreEqual(esperado.x, obtido.x, Tol, msg);
        Assert.AreEqual(esperado.y, obtido.y, Tol, msg);
    }

    [Test]
    public void MiraAndaComOPonteiro()
    {
        PontosDeBorrifar s = Nova(Tres, 3);
        Quadro(s, new Vector2(10f, -5f));
        IgualV(new Vector2(0.1f, -0.05f), s.Mira);
    }

    [Test]
    public void MiraIgnoraOOlhar()
    {
        PontosDeBorrifar s = Nova(Tres, 3);
        s.Atualizar(Q, new EntradaDoMinigame { Olhar = new Vector2(10f, 10f) });
        IgualV(Vector2.zero, s.Mira);
    }

    [Test]
    public void MiraPresaAoCampo()
    {
        PontosDeBorrifar s = Nova(Tres, 3);
        Quadro(s, new Vector2(1000f, 1000f));
        IgualV(new Vector2(1.5f, 1f), s.Mira);
        Quadro(s, new Vector2(-5000f, -5000f));
        IgualV(new Vector2(-1.5f, -1f), s.Mira);
    }

    [Test]
    public void MovimentoNaoFinitoContaComoParado()
    {
        PontosDeBorrifar s = Nova(Tres, 3);
        Quadro(s, new Vector2(float.NaN, float.PositiveInfinity));
        IgualV(Vector2.zero, s.Mira);
        Quadro(s, new Vector2(10f, 0f));
        IgualV(new Vector2(0.1f, 0f), s.Mira);
    }

    [Test]
    public void CliqueDentroAcertaEPassaParaAProxima()
    {
        PontosDeBorrifar s = Nova(Tres, 3);
        Assert.AreEqual(EstadoDoMinigame.Passo, Ir(s, Tres[0], true));
        Assert.AreEqual(1, s.Acertos);
        Assert.AreEqual(1, s.DaVez);
        Assert.AreEqual(2, s.Proxima);
        Assert.AreEqual(1f, s.Anel, Tol);
        Assert.IsTrue(s.UltimaAcertou);
        Assert.AreEqual(1, s.Borrifadas);
        IgualV(Tres[0], s.UltimaBorrifada);
    }

    [Test]
    public void CliqueNoRaioAcerta()
    {
        PontosDeBorrifar s = Nova(Tres, 3);
        Ir(s, Tres[0] + new Vector2(0.139f, 0f), true);
        Assert.AreEqual(1, s.Acertos, "dentro do raio");

        PontosDeBorrifar fora = Nova(Tres, 3);
        Ir(fora, Tres[0] + new Vector2(0.15f, 0f), true);
        Assert.AreEqual(0, fora.Acertos, "fora do raio");
    }

    [Test]
    public void CliqueForaNaoConta()
    {
        PontosDeBorrifar s = Nova(Tres, 3);
        Assert.AreEqual(EstadoDoMinigame.EmAndamento, Ir(s, new Vector2(0.9f, 0.9f), true));
        Assert.AreEqual(0, s.Acertos);
        Assert.AreEqual(1, s.Borrifadas);
        Assert.IsFalse(s.UltimaAcertou);
        Assert.AreEqual(0, s.DaVez);
    }

    [Test]
    public void CliqueNaApagadaNaoConta()
    {
        PontosDeBorrifar s = Nova(Tres, 3);
        Ir(s, Tres[1], true);
        Assert.AreEqual(0, s.Acertos);
    }

    [Test]
    public void SegurarNaoAcerta()
    {
        PontosDeBorrifar s = Nova(Tres, 3);
        Ir(s, Tres[0]);
        for (int i = 0; i < 60; i++)
            s.Atualizar(Q, new EntradaDoMinigame { Segurando = true, Apertou = false });
        Assert.AreEqual(0, s.Acertos);
        Assert.AreEqual(0, s.Borrifadas);
    }

    [Test]
    public void NadaAcertaNaEsperaInicial()
    {
        PontosDeBorrifar s = Nova(Tres, 3, espera: 0.3f);
        Ir(s, Tres[0], true);
        Assert.AreEqual(0, s.Acertos);
        Assert.AreEqual(-1, s.DaVez);
        Assert.AreEqual(-1, s.Proxima);
        Assert.IsFalse(s.BolinhaNaTela);
        Assert.AreEqual(1, s.Borrifadas);

        Esperar(s, 0.3f);
        Assert.AreEqual(0, s.DaVez);
        Assert.IsTrue(s.BolinhaNaTela);

        Quadro(s, apertou: true);
        Assert.AreEqual(1, s.Acertos);
    }

    [Test]
    public void AnelFechaEABolinhaVolta()
    {
        PontosDeBorrifar s = Nova(Tres, 3);
        Ir(s, Tres[0]);
        Esperar(s, 0.75f);
        Assert.AreEqual(0.5f, s.Anel, 0.04f);

        Esperar(s, 0.8f);
        Assert.IsFalse(s.BolinhaNaTela);
        Assert.AreEqual(0, s.DaVez);
        Quadro(s, apertou: true);
        Assert.AreEqual(0, s.Acertos, "sem bolinha na tela, o aperto não acerta");

        Esperar(s, 0.4f);
        Assert.IsTrue(s.BolinhaNaTela);
        Assert.Greater(s.Anel, 0.8f);
        Quadro(s, apertou: true);
        Assert.AreEqual(1, s.Acertos);
    }

    [Test]
    public void PassosNasContagensCertas()
    {
        PontosDeBorrifar s = Nova(Cinco, 3);
        bool[] deuPasso = new bool[5];
        for (int i = 0; i < 5; i++)
            deuPasso[i] = Acertar(s) == EstadoDoMinigame.Passo;
        Assert.AreEqual(5, s.Acertos);
        Assert.AreEqual(new[] { false, true, false, true, true }, deuPasso, "Passo no 2º, no 4º e no 5º acerto");
    }

    [Test]
    public void MaisPassosQuePontos()
    {
        PontosDeBorrifar s = Nova(Dois, 5);
        Assert.AreEqual(EstadoDoMinigame.Passo, Acertar(s));
        Assert.AreEqual(1f, s.FracaoDoPasso, Tol, "o passo seguinte já é devido");
        Assert.AreEqual(EstadoDoMinigame.Passo, Quadro(s));
        Assert.AreEqual(EstadoDoMinigame.EmAndamento, Quadro(s));

        Assert.AreEqual(EstadoDoMinigame.Passo, Acertar(s));
        Assert.AreEqual(EstadoDoMinigame.Passo, Quadro(s));
        Assert.AreEqual(EstadoDoMinigame.Passo, Quadro(s));
        Assert.AreEqual(EstadoDoMinigame.EmAndamento, Quadro(s));
    }

    [Test]
    public void SemPontosTodosOsPassosSaem()
    {
        foreach (Vector2[] pontos in new[] { new Vector2[0], null })
        {
            PontosDeBorrifar s = Nova(pontos, 3);
            Assert.AreEqual(0, s.Total);
            Assert.AreEqual(-1, s.DaVez);
            Assert.AreEqual(EstadoDoMinigame.Passo, Quadro(s));
            Assert.AreEqual(EstadoDoMinigame.Passo, Quadro(s));
            Assert.AreEqual(EstadoDoMinigame.Passo, Quadro(s));
            Assert.AreEqual(EstadoDoMinigame.EmAndamento, Quadro(s));
        }
    }

    [Test]
    public void PassoDoUltimoAcertoSoDepoisDoFecho()
    {
        PontosDeBorrifar s = Nova(Tres, 3, fecho: 0.45f);
        Assert.AreEqual(EstadoDoMinigame.Passo, Acertar(s));
        Assert.AreEqual(EstadoDoMinigame.Passo, Acertar(s));
        Assert.AreEqual(EstadoDoMinigame.EmAndamento, Acertar(s));
        Assert.IsTrue(s.NoFecho);
        Assert.AreEqual(-1, s.DaVez);
        Assert.AreEqual(-1, s.Proxima);
        Assert.AreEqual(1f, s.FracaoDoPasso, Tol);

        Assert.AreEqual(0, Esperar(s, 0.4f));
        Assert.AreEqual(1, Esperar(s, 0.1f));
        Assert.IsFalse(s.NoFecho);
    }

    [Test]
    public void FechoSeguraTodosOsPassosDoUltimoAcerto()
    {
        PontosDeBorrifar s = Nova(Dois, 5, fecho: 0.45f);
        Acertar(s);
        Esperar(s, 0.1f);
        Acertar(s);
        Assert.AreEqual(2, s.Acertos);
        Assert.AreEqual(0, Esperar(s, 0.4f));
        Assert.AreEqual(3, Esperar(s, 0.2f));
    }

    [Test]
    public void DtInvalidoNaoMudaNada()
    {
        PontosDeBorrifar s = Nova(Tres, 3);
        foreach (float dt in new[] { 0f, -1f, float.NaN })
        {
            Quadro(s, new Vector2(10f, 0f), true, dt);
            IgualV(Vector2.zero, s.Mira, "dt " + dt);
            Assert.AreEqual(0, s.Borrifadas, "dt " + dt);
            Assert.AreEqual(1f, s.Anel, Tol, "dt " + dt);
            Assert.IsFalse(s.Agindo, "dt " + dt);
        }

        PontosDeBorrifar d = Nova(Dois, 5);
        Assert.AreEqual(EstadoDoMinigame.Passo, Acertar(d));
        Assert.AreEqual(EstadoDoMinigame.Passo, Quadro(d, dt: 0f), "o Passo devido ainda sai");
    }

    [Test]
    public void DtInvalidoNaoAndaOFecho()
    {
        PontosDeBorrifar s = Nova(Tres, 3, fecho: 0.45f);
        Acertar(s);
        Acertar(s);
        Acertar(s);
        Assert.IsTrue(s.NoFecho);
        for (int i = 0; i < 100; i++)
            Assert.AreEqual(EstadoDoMinigame.EmAndamento, Quadro(s, dt: 0f));
    }

    [Test]
    public void AgindoSoLogoDepoisDeUmAperto()
    {
        PontosDeBorrifar s = Nova(Tres, 3);
        Assert.IsFalse(s.Agindo, "antes");
        Ir(s, new Vector2(0.9f, 0.9f), true);
        Assert.IsTrue(s.Agindo);
        Esperar(s, 0.1f);
        Assert.IsTrue(s.Agindo);
        Esperar(s, 0.15f);
        Assert.IsFalse(s.Agindo);
    }

    [Test]
    public void FracaoDoPassoContaOsAcertos()
    {
        PontosDeBorrifar s = Nova(Cinco, 2);
        Assert.AreEqual(0f, s.FracaoDoPasso, Tol);
        Acertar(s);
        Assert.AreEqual(1f / 3f, s.FracaoDoPasso, Tol);
        Acertar(s);
        Assert.AreEqual(2f / 3f, s.FracaoDoPasso, Tol);
        Assert.AreEqual(EstadoDoMinigame.Passo, Acertar(s));
        Assert.AreEqual(0f, s.FracaoDoPasso, Tol);
        Acertar(s);
        Assert.AreEqual(0.5f, s.FracaoDoPasso, Tol);
    }

    [Test]
    public void PodeSairSempre()
    {
        PontosDeBorrifar s = Nova(Tres, 3, espera: 0.3f, fecho: 0.45f);
        Quadro(s);
        Assert.IsFalse(s.BolinhaNaTela);
        Assert.IsTrue(s.PodeSair, "espera inicial");

        Esperar(s, 0.35f);
        Assert.IsTrue(s.BolinhaNaTela);
        Assert.IsTrue(s.PodeSair, "bolinha na tela");

        Acertar(s);
        Acertar(s);
        Acertar(s);
        Assert.IsTrue(s.NoFecho);
        Assert.IsTrue(s.PodeSair, "fecho");
    }

    [Test]
    public void ArgumentosRuinsViramPadrao()
    {
        PontosDeBorrifar s = new PontosDeBorrifar(new[] { new Vector2(float.NaN, 0.2f) }, 0,
            -1f, float.NaN, -1f, float.NaN, -1f, 0f, float.PositiveInfinity);
        Assert.AreEqual(0.14f, s.Raio, Tol);
        IgualV(new Vector2(0f, 0.2f), s.Ponto(0));

        int passos = 0;
        if (Quadro(s, new Vector2(100f, 0f)) == EstadoDoMinigame.Passo) passos++;
        Assert.AreEqual(-1, s.DaVez, "a espera virou 0,3 s");
        Assert.AreEqual(0.93f, s.Mira.x, Tol, "mira por grau virou 0,0093");

        if (Quadro(s, new Vector2(1e6f, 0f)) == EstadoDoMinigame.Passo) passos++;
        Assert.AreEqual(16f / 9f, s.Mira.x, Tol, "meia largura virou 16/9");

        passos += Esperar(s, 0.35f);
        Assert.IsTrue(s.BolinhaNaTela);
        if (Quadro(s, (s.Ponto(0) - s.Mira) / PontosDeBorrifar.MiraPorGrauPadrao, true) == EstadoDoMinigame.Passo) passos++;
        Assert.AreEqual(1, s.Acertos);
        passos += Esperar(s, 1f);
        Assert.AreEqual(1, passos, "passos 0 virou 1");
    }

    [Test]
    public void ForaDoIntervaloPontoEZero()
    {
        PontosDeBorrifar s = Nova(Tres, 3);
        IgualV(Vector2.zero, s.Ponto(-1));
        IgualV(Vector2.zero, s.Ponto(3));
    }

    [Test]
    public void PoseDePerto_NoCampo()
    {
        PoseDePerto pose = new PoseDePerto
        {
            Posicao = new Vector3(0f, 1f, 0f),
            Giro = Quaternion.LookRotation(Vector3.down, Vector3.forward),
            Fov = 90f
        };
        IgualV(Vector2.zero, pose.NoCampo(new Vector3(0f, 0f, 0f)));
        IgualV(new Vector2(0f, 1f), pose.NoCampo(new Vector3(0f, 0f, 1f)));
        IgualV(new Vector2(1f, 0f), pose.NoCampo(new Vector3(1f, 0f, 0f)));
        IgualV(Vector2.zero, pose.NoCampo(new Vector3(0f, 2f, 0f)), "atrás da câmera");
    }
}
