using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

/* ============================================================
 *  BalanceamentoSimuladorTestes.cs
 *  ---------------------------------------------------------------------
 *  Testes EditMode (Parte B) do simulador de balanceamento: uma partida
 *  simulada (SimularSemente) tem de dar o mesmo que as classes reais do
 *  jogo, GerenciadorDeFocos e FluxoDeJogo.TaxaPara, jogadas passo a passo
 *  com a mesma semente; e a linha da tabela (Resumir) sai da maioria das
 *  sementes, mostrando a semente que muda o resultado.
 *
 *  As rotas (RotaMinima, RotaComKit) leem a cena aberta e não entram
 *  aqui: a conferência delas é o Dengue > Simular Balanceamento.
 *
 *  Os objetos nascem pelo TesteUtil (escondidos, fora da cena) e são
 *  destruídos no [TearDown].
 *
 *  Rodar:  Window > General > Test Runner > EditMode (categoria ParteB)
 *  ou      unity command run_tests --mode EditMode --filter_type category --filter ParteB
 * ============================================================ */

[Category("ParteB")]
public class BalanceamentoSimuladorTestes
{
    private const int Total = 6;
    private const int Semente = 3;

    private GerenciadorDeFocos gerenciador;
    private FluxoDeJogo fluxo;

    [SetUp]
    public void Preparar()
    {
        gerenciador = TesteUtil.Com<GerenciadorDeFocos>("Gerenciador (teste)");
        fluxo = TesteUtil.Com<FluxoDeJogo>("Fluxo (teste)");
        TesteUtil.Setar(gerenciador, "focosComMosquitoNoInicio", 2);
        TesteUtil.Setar(gerenciador, "segundosPorEstagio", 20f);
    }

    [TearDown]
    public void Limpar()
    {
        TesteUtil.LimparTudo();
    }

    private static List<Interactable> NovosFocos()
    {
        List<Interactable> focos = new List<Interactable>();
        for (int i = 0; i < Total; i++)
        {
            Interactable foco = TesteUtil.Com<Interactable>("Foco " + i + " (teste)");
            TesteUtil.Setar(foco, "requiredClicks", 1);
            TesteUtil.Setar(foco, "punchScale", 0f);
            TesteUtil.Setar(foco, "shrinkDuration", 0f);
            focos.Add(foco);
        }
        return focos;
    }

    /* A mesma partida com os focos e o gerenciador de verdade: o ciclo anda
     * pelo Avancar, a pressão vem do Pressao e cada foco cai pelo Interact. */
    private BalanceamentoSimulador.Partida JogarDeVerdade(float ritmo)
    {
        List<Interactable> focos = NovosFocos();
        gerenciador.Registrar(focos);
        gerenciador.SortearInicio(new System.Random(Semente));

        const float passo = BalanceamentoSimulador.Passo;
        float ocupacao = fluxo.OcupacaoInicial;
        float pico = ocupacao;
        float t = 0f;
        float noFoco = 0f;

        while (gerenciador.Restantes > 0)
        {
            gerenciador.Avancar(passo);
            ocupacao = Mathf.Clamp(ocupacao + fluxo.TaxaPara(t, gerenciador.Pressao(), false) * passo, fluxo.OcupacaoMinima, 1f);
            t += passo;
            noFoco += passo;
            pico = Mathf.Max(pico, ocupacao);

            if (ocupacao >= 1f)
                break;

            float tempoDesteFoco = ritmo * (1f + BalanceamentoSimulador.FatorUltimos * (1f - (float)gerenciador.Restantes / Total));
            if (noFoco >= tempoDesteFoco)
            {
                noFoco = 0f;
                focos[gerenciador.Eliminados].Interact();
                ocupacao = Mathf.Max(fluxo.OcupacaoMinima, ocupacao - fluxo.AlivioPorFoco);
            }
        }

        return new BalanceamentoSimulador.Partida
        {
            Semente = Semente,
            Venceu = gerenciador.Restantes == 0,
            Duracao = t,
            Pico = pico,
            Final = ocupacao,
            Eliminados = gerenciador.Eliminados,
        };
    }

    private static BalanceamentoSimulador.Partida Vitoria(int semente, float pico)
    {
        return new BalanceamentoSimulador.Partida
        {
            Semente = semente, Venceu = true, Terminou = true,
            Duracao = 600f, Pico = pico, Final = 0.4f, Eliminados = Total,
        };
    }

    private static BalanceamentoSimulador.Partida Derrota(int semente, float duracao)
    {
        return new BalanceamentoSimulador.Partida
        {
            Semente = semente, Terminou = true,
            Duracao = duracao, Pico = 1f, Final = 1f, Eliminados = 2,
        };
    }

    /* Vence (5 s/foco), perde com focos eliminados (60 s/foco) e parado. */
    [TestCase(5f, true)]
    [TestCase(60f, false)]
    [TestCase(float.PositiveInfinity, false)]
    public void SimularSemente_BateComAsClassesReais(float ritmo, bool vence)
    {
        BalanceamentoSimulador.Partida simulada = BalanceamentoSimulador.SimularSemente(fluxo, gerenciador, Total, ritmo, Semente);
        BalanceamentoSimulador.Partida real = JogarDeVerdade(ritmo);

        Assert.AreEqual(vence, real.Venceu, "o caso de teste não é o que diz o nome");
        Assert.AreEqual(real.Venceu, simulada.Venceu, "Venceu");
        Assert.AreEqual(real.Eliminados, simulada.Eliminados, "Eliminados");
        Assert.AreEqual(real.Duracao, simulada.Duracao, 1e-3f, "Duracao");
        Assert.AreEqual(real.Pico, simulada.Pico, 1e-4f, "Pico");
        Assert.AreEqual(real.Final, simulada.Final, 1e-4f, "Final");
    }

    /* Sem chance de vencer nem de perder: nem ritmo, nem subida. */
    [Test]
    public void SimularSemente_SemSubida_NaoTerminaEmUmaHora()
    {
        TesteUtil.Setar(fluxo, "taxaComTodosOsFocos", 0f);

        BalanceamentoSimulador.Partida partida = BalanceamentoSimulador.SimularSemente(fluxo, gerenciador, Total, float.PositiveInfinity, Semente);

        Assert.IsFalse(partida.Terminou);
        Assert.IsFalse(partida.Venceu);
    }

    [Test]
    public void Resumir_TodasIguais_MostraAMediaSemSementeQueMuda()
    {
        string linha = BalanceamentoSimulador.Resumir(new[] { Vitoria(1, 0.8f), Vitoria(2, 0.9f) }, Total);

        StringAssert.StartsWith("vence em 10:00", linha);
        StringAssert.Contains("pico 85% (80%–90%)", linha);
        StringAssert.Contains("2/2 sementes vencem", linha);
        StringAssert.DoesNotContain("muda", linha);
    }

    [Test]
    public void Resumir_MaioriaVence_MostraAQuePerdeMaisCedo()
    {
        string linha = BalanceamentoSimulador.Resumir(new[]
        {
            Vitoria(1, 0.9f), Vitoria(2, 0.9f), Vitoria(3, 0.9f), Derrota(4, 500f), Derrota(5, 300f),
        }, Total);

        StringAssert.StartsWith("vence em 10:00", linha);
        StringAssert.Contains("3/5 sementes vencem", linha);
        StringAssert.Contains("semente 5 muda: PERDE aos 5:00", linha);
    }

    [Test]
    public void Resumir_MaioriaPerde_MostraAQueVenceComMenorPico()
    {
        string linha = BalanceamentoSimulador.Resumir(new[]
        {
            Derrota(1, 300f), Derrota(2, 360f), Derrota(3, 420f), Vitoria(4, 0.97f), Vitoria(5, 0.95f),
        }, Total);

        StringAssert.StartsWith("PERDE aos 6:00 (5:00–7:00)", linha);
        StringAssert.Contains("2/5 sementes vencem", linha);
        StringAssert.Contains("semente 5 muda: vence em 10:00", linha);
    }

    /* Empate não é vitória: o critério "vence" pede mais da metade. */
    [Test]
    public void Resumir_Empate_ContaComoDerrota()
    {
        string linha = BalanceamentoSimulador.Resumir(new[] { Vitoria(1, 0.9f), Derrota(2, 300f) }, Total);

        StringAssert.StartsWith("PERDE", linha);
    }

    [Test]
    public void Resumir_NinguemTermina_Avisa()
    {
        BalanceamentoSimulador.Partida parada = new BalanceamentoSimulador.Partida { Semente = 1, Duracao = 3600f, Pico = 0.8f, Final = 0.8f };

        StringAssert.StartsWith("não termina em 1 h", BalanceamentoSimulador.Resumir(new[] { parada }, Total));
    }
}
