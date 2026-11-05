using NUnit.Framework;
using UnityEngine;

/* ============================================================
 *  StaminaDeCargaTestes.cs
 *  ---------------------------------------------------------------------
 *  A stamina de correr com o pneu, sozinha e ligada ao kit e ao
 *  controlador, as vagas pelo número de pneus e a viagem do simulador.
 * ============================================================ */

[Category("ParteB")]
public class StaminaDeCargaTestes
{
    private const float Quadro = 0.05f;

    [TearDown]
    public void Limpar()
    {
        TesteUtil.LimparTudo();
    }

    private static void Passar(StaminaDeCarga s, float segundos, bool correndo)
    {
        for (float t = 0f; t < segundos - 1e-4f; t += Quadro)
            s.Avancar(Quadro, correndo);
    }

    [Test]
    public void Esgota_EmSeisSegundos_AvisaUmaVez()
    {
        StaminaDeCarga s = new StaminaDeCarga(6f, 5f, 0.3f);
        Passar(s, 5.9f, true);
        Assert.IsTrue(s.PodeCorrer);
        Assert.Greater(s.Fracao, 0f);

        int avisos = 0;
        for (int i = 0; i < 10; i++)
            if (s.Avancar(Quadro, true))
                avisos++;

        Assert.AreEqual(1, avisos);
        Assert.IsTrue(s.Esgotada);
        Assert.IsFalse(s.PodeCorrer);
    }

    [Test]
    public void Esgotada_SoVoltaEmTrintaPorCento()
    {
        StaminaDeCarga s = new StaminaDeCarga(6f, 5f, 0.3f);
        Passar(s, 6.1f, true);

        Passar(s, 1.4f, false);
        Assert.IsFalse(s.PodeCorrer, "abaixo de 30%");
        Passar(s, 0.15f, false);
        Assert.IsTrue(s.PodeCorrer, "30% em 1,5 s");
    }

    [Test]
    public void Recupera_EmCincoSegundos()
    {
        StaminaDeCarga s = new StaminaDeCarga(6f, 5f, 0.3f);
        Passar(s, 3f, true);
        Assert.AreEqual(0.5f, s.Fracao, 0.01f);
        Passar(s, 2.5f, false);
        Assert.AreEqual(1f, s.Fracao, 0.01f);
    }

    private static KitDoAgente NovoKit(out ControladorJogador jogador, out PneuInteractable pneu)
    {
        jogador = TesteUtil.Com<ControladorJogador>("Jogador (teste)");
        jogador.transform.position = new Vector3(0f, -500f, 0f);
        KitDoAgente kit = jogador.gameObject.AddComponent<KitDoAgente>();
        pneu = TesteUtil.Com<PneuInteractable>("Pneu (teste)");
        pneu.transform.position = new Vector3(0f, -500f, 2f);
        return kit;
    }

    [Test]
    public void ComPneu_CorreMasNaoPula_EZeradaNaoCorre()
    {
        KitDoAgente kit = NovoKit(out ControladorJogador jogador, out PneuInteractable pneu);
        Assert.IsTrue(kit.Pegar(pneu));
        Assert.IsTrue(jogador.PodeCorrer);
        Assert.IsFalse(jogador.PodePular);

        int esgotou = 0;
        kit.OnStaminaEsgotada += () => esgotou++;
        for (int i = 0; i < 130; i++)
            kit.AvancarStamina(Quadro, true);

        Assert.AreEqual(1, esgotou);
        Assert.IsFalse(jogador.PodeCorrer);
        Assert.IsFalse(jogador.PodePular);
    }

    [Test]
    public void LargarZerado_DevolveAMovimentacaoNormal()
    {
        KitDoAgente kit = NovoKit(out ControladorJogador jogador, out PneuInteractable pneu);
        Assert.IsTrue(kit.Pegar(pneu));
        for (int i = 0; i < 130; i++)
            kit.AvancarStamina(Quadro, true);

        Assert.IsTrue(kit.LargarPneu(new Vector3(0f, -500f, 0f), Vector3.forward));
        kit.AvancarStamina(Quadro, false);

        Assert.IsTrue(jogador.PodeCorrer, "de mãos livres a corrida não depende da stamina");
        Assert.IsTrue(jogador.PodePular);
        Assert.AreEqual(1f, jogador.MultiplicadorVelocidade);
    }

    [Test]
    public void MaosLivres_CorrerNaoGasta()
    {
        KitDoAgente kit = NovoKit(out _, out _);
        for (int i = 0; i < 130; i++)
            kit.AvancarStamina(Quadro, true);
        Assert.AreEqual(1f, kit.Stamina.Fracao, 1e-4f);
    }

    [TestCase(0, 6)]
    [TestCase(6, 6)]
    [TestCase(7, 8)]
    [TestCase(11, 12)]
    [TestCase(12, 12)]
    public void Vagas_PeloNumeroDePneus(int pneus, int vagas)
    {
        Assert.AreEqual(vagas, FocosVivosBuilder.TotalDeVagas(pneus));
    }

    [Test]
    public void Simulador_ViagemCurta_CorreTudo_ELongaAlterna()
    {
        Assert.AreEqual(2f, BalanceamentoSimulador.SegundosCarregando(10f, 5f, 2f, new StaminaDeCarga(6f, 5f, 0.3f)), 0.11f);

        float longa = BalanceamentoSimulador.SegundosCarregando(100f, 5f, 2f, new StaminaDeCarga(6f, 5f, 0.3f));
        Assert.Greater(longa, 20f, "mais lento que correr tudo");
        Assert.Less(longa, 50f, "mais rápido que andar tudo");
    }
}
