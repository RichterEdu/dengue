using NUnit.Framework;
using UnityEngine;

/* ============================================================
 *  MinigamesLonaPoteTestes.cs
 *  ---------------------------------------------------------------------
 *  Testes EditMode dos minigames do pote (EsfregarCirculo) e da lona
 *  (InclinarLona): a lógica pura, com dt e entradas montadas à mão, e a
 *  fábrica de cada foco (sem sessão quando não cabe).
 *
 *  Rodar:  unity command run_tests --mode EditMode --filter_type category --filter ParteB
 * ============================================================ */

[Category("ParteB")]
public class MinigamesLonaPoteTestes
{
    private const float Dt = 1f / 60f;

    [TearDown]
    public void Limpar()
    {
        TesteUtil.LimparTudo();
    }

    private static EntradaDoMinigame Mexer(Vector2 ponteiro, bool segurando)
        => new EntradaDoMinigame { Ponteiro = ponteiro, Segurando = segurando };

    /* Graus de ponteiro que levam a esponja de 'de' até 'para' num quadro. */
    private static Vector2 Graus(Vector2 de, Vector2 para) => (para - de) / EsfregarCirculo.MetrosPorGrauPadrao;

    /* Dá uma volta no círculo em 'quadros' quadros, segurando, e conta os passos. */
    private static int DarVolta(EsfregarCirculo s, int quadros, float voltas = 1f, bool segurando = true)
    {
        int passos = 0;
        float a0 = Mathf.Atan2(s.Esponja.y, s.Esponja.x);
        for (int q = 1; q <= quadros; q++)
        {
            float a = a0 + 2f * Mathf.PI * voltas * q / quadros;
            Vector2 alvo = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * s.Raio;
            if (s.Atualizar(Dt, Mexer(Graus(s.Esponja, alvo), segurando)) == EstadoDoMinigame.Passo)
                passos++;
        }
        return passos;
    }

    /* ---------------- Esfregar (pote) ---------------- */

    [Test]
    public void Esfregar_ComecaNoCirculo_EmbaixoESemNadaLimpo()
    {
        EsfregarCirculo s = new EsfregarCirculo(0.1f, 4, 0.03f);
        Assert.AreEqual(new Vector2(0f, -0.1f), s.Esponja);
        Assert.IsTrue(s.NoTraco);
        Assert.AreEqual(0, s.Limpos);
        Assert.IsTrue(s.PodeSair);
    }

    [Test]
    public void Esfregar_UmaVoltaSegurando_LimpaTudo_EDaTodosOsPassos()
    {
        EsfregarCirculo s = new EsfregarCirculo(0.1f, 4, 0.03f);

        int passos = DarVolta(s, 120, 1.02f);
        // Um passo por Atualizar: os que sobrarem saem nos quadros seguintes.
        for (int i = 0; i < 4 && passos < 4; i++)
            if (s.Atualizar(Dt, Mexer(Vector2.zero, false)) == EstadoDoMinigame.Passo) passos++;

        Assert.AreEqual(s.Segmentos, s.Limpos);
        Assert.AreEqual(1f, s.FracaoLimpa);
        Assert.AreEqual(4, passos);
    }

    [Test]
    public void Esfregar_SemSegurar_NaoLimpa()
    {
        EsfregarCirculo s = new EsfregarCirculo(0.1f, 4, 0.03f);
        Assert.AreEqual(0, DarVolta(s, 120, 1f, segurando: false));
        Assert.AreEqual(0, s.Limpos);
        Assert.IsFalse(s.Agindo);
    }

    [Test]
    public void Esfregar_ForaDoTraco_NaoLimpa()
    {
        EsfregarCirculo s = new EsfregarCirculo(0.1f, 4, 0.02f);
        // Vai para o centro do pote, segurando.
        s.Atualizar(Dt, Mexer(Graus(s.Esponja, Vector2.zero), true));
        Assert.IsFalse(s.NoTraco);
        Assert.IsFalse(s.Esfregando);
        int antes = s.Limpos;
        s.Atualizar(Dt, Mexer(new Vector2(5f, 0f), true));
        Assert.AreEqual(antes, s.Limpos);
    }

    [Test]
    public void Esfregar_MeiaVolta_DaMetadeDosPassos_EAFracaoAcompanha()
    {
        EsfregarCirculo s = new EsfregarCirculo(0.1f, 4, 0.03f);
        int passos = DarVolta(s, 60, 0.5f);
        for (int i = 0; i < 3; i++)
            if (s.Atualizar(Dt, Mexer(Vector2.zero, false)) == EstadoDoMinigame.Passo) passos++;

        Assert.That(s.FracaoLimpa, Is.InRange(0.48f, 0.56f));
        Assert.AreEqual(2, passos);
        Assert.Less(s.FracaoDoPasso, 1f);
    }

    [Test]
    public void Esfregar_SaltoRapido_NaoPulaTrecho()
    {
        EsfregarCirculo s = new EsfregarCirculo(0.1f, 1, 0.03f);
        // Um quarto de volta em só dois quadros.
        DarVolta(s, 2, 0.25f);
        Assert.That(s.FracaoLimpa, Is.GreaterThanOrEqualTo(0.24f));
    }

    [Test]
    public void Esfregar_DtInvalido_NaoMexe()
    {
        EsfregarCirculo s = new EsfregarCirculo(0.1f, 4, 0.03f);
        Vector2 antes = s.Esponja;
        s.Atualizar(0f, Mexer(new Vector2(30f, 30f), true));
        s.Atualizar(float.NaN, Mexer(new Vector2(30f, 30f), true));
        Assert.AreEqual(antes, s.Esponja);
    }

    /* ---------------- Inclinar (lona) ---------------- */

    private static InclinarLona NovaLona(params Vector2[] pocas)
        => new InclinarLona(pocas, new Vector2(1f, 0.7f), Mathf.Max(1, pocas.Length));

    [Test]
    public void Inclinar_ParaASaida_AsPocasEscorrem_EDaoOsPassos()
    {
        InclinarLona s = NovaLona(new Vector2(0.4f, 0.2f), new Vector2(-0.3f, -0.3f));
        int passos = 0;
        // Inclina toda para a esquerda (−x).
        s.Atualizar(Dt, Mexer(new Vector2(-1000f, 0f), false));
        Assert.AreEqual(-1f, s.Inclinacao.x, 1e-4f);
        // Segura a lona erguida (sem mexer, ela volta a ficar reta).
        for (int q = 0; q < 60 * 10 && passos < 2; q++)
            if (s.Atualizar(Dt, Mexer(new Vector2(-30f, 0f), false)) == EstadoDoMinigame.Passo) passos++;

        Assert.AreEqual(2, s.Saidas);
        Assert.IsTrue(s.Saiu(0) && s.Saiu(1));
        Assert.AreEqual(2, passos);
    }

    [Test]
    public void Inclinar_ParaOutroLado_NenhumaPocaSai()
    {
        InclinarLona s = NovaLona(new Vector2(0f, 0f));
        for (int q = 0; q < 60 * 6; q++)
            Assert.AreNotEqual(EstadoDoMinigame.Passo, s.Atualizar(Dt, Mexer(new Vector2(30f, 0f), false)));

        Assert.AreEqual(0, s.Saidas);
        Assert.AreEqual(0.9f, s.Posicao(0).x, 1e-4f, "para encostada na borda da direita, com a margem");
    }

    [Test]
    public void Inclinar_BordasDeCimaEBaixo_SeguramAPoca()
    {
        InclinarLona s = NovaLona(new Vector2(0f, 0f));
        for (int q = 0; q < 60 * 6; q++)
            s.Atualizar(Dt, Mexer(new Vector2(0f, 30f), false));
        Assert.AreEqual(0.6f, s.Posicao(0).y, 1e-4f);
        Assert.AreEqual(0, s.Saidas);
    }

    [Test]
    public void Inclinar_FicaNoDiscoDeRaio1()
    {
        InclinarLona s = NovaLona(new Vector2(0f, 0f));
        s.Atualizar(Dt, Mexer(new Vector2(1000f, 1000f), false));
        Assert.AreEqual(1f, s.Inclinacao.magnitude, 1e-4f);
    }

    [Test]
    public void Inclinar_SemMexer_ALonaVoltaAFicarReta()
    {
        InclinarLona s = NovaLona(new Vector2(0f, 0f));
        s.Atualizar(Dt, Mexer(new Vector2(-1000f, 0f), false));
        for (int q = 0; q < 60 * 5; q++)
            s.Atualizar(Dt, Mexer(Vector2.zero, false));
        Assert.Less(s.Inclinacao.magnitude, 0.01f);
    }

    [Test]
    public void Inclinar_SemPoca_OPassoJaEDevido()
    {
        InclinarLona s = new InclinarLona(new Vector2[0], new Vector2(1f, 0.7f), 1);
        Assert.AreEqual(EstadoDoMinigame.Passo, s.Atualizar(Dt, Mexer(Vector2.zero, false)));
        Assert.AreEqual(EstadoDoMinigame.EmAndamento, s.Atualizar(Dt, Mexer(Vector2.zero, false)));
    }

    /* ---------------- Fábricas dos focos ---------------- */

    private KitDoAgente NovoKit()
    {
        ControladorJogador jogador = TesteUtil.Com<ControladorJogador>("Jogador (teste)");
        return jogador.gameObject.AddComponent<KitDoAgente>();
    }

    [Test]
    public void Pote_ComAsMaos_TemMinigame_ComOsPassosQueFaltam_EOutraFerramentaNao()
    {
        KitDoAgente kit = NovoKit();
        PoteDeBichoInteractable pote = TesteUtil.Com<PoteDeBichoInteractable>("Pote (teste)");
        TesteUtil.Setar(pote, "requiredClicks", 4);
        TesteUtil.Setar(pote, "punchScale", 0f);
        pote.transform.position = new Vector3(0f, 0f, 2f);
        kit.transform.position = Vector3.zero;

        SessaoDoPote s = pote.CriarMinigame(kit, 0) as SessaoDoPote;
        Assert.IsNotNull(s);
        Assert.IsTrue(s.Pose.Posicao.y > 0.3f, "câmera em cima do pote");

        pote.AvancarCom(kit);
        SessaoDoPote depois = pote.CriarSessao(Vector3.zero, Vector3.forward);
        int passos = DarVolta(depois, 120, 1.02f);
        for (int i = 0; i < 5; i++)
            if (depois.Atualizar(Dt, Mexer(Vector2.zero, false)) == EstadoDoMinigame.Passo) passos++;
        Assert.AreEqual(3, passos, "só os 3 que faltam");

        kit.Selecionar(Ferramenta.Areia);
        Assert.IsNull(pote.CriarMinigame(kit, 0));
    }

    [Test]
    public void Lona_SemPoca_NaoTemMinigame()
    {
        KitDoAgente kit = NovoKit();
        LonaInteractable lona = TesteUtil.Com<LonaInteractable>("Lona (teste)");
        Assert.IsNull(lona.CriarMinigame(kit, 0), "lona sem poça nenhuma");
    }
}
