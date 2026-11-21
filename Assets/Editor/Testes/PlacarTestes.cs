using NUnit.Framework;
using UnityEngine;

/* ============================================================
 *  PlacarTestes.cs
 *  ---------------------------------------------------------------------
 *  Testes EditMode (Parte C) do PlacarDaPartida: o pico da ocupação, as
 *  metas que caem (uma vez só), o minigame errado que derruba Sem erros,
 *  o fechamento da nota (uma vez, com recorde) e as guardas de partida
 *  não iniciada, encerrada e sem fluxo.
 *
 *  O Start e o Update não rodam no EditMode: o fluxo e o gerenciador são
 *  ligados nos campos, e Ligar, Conferir e AvancarOcupacao são chamados
 *  direto. O recorde de teste usa o prefixo "teste.placar.", apagado no
 *  [SetUp] e no [TearDown].
 *
 *  Rodar:  Window > General > Test Runner > EditMode (categoria ParteC)
 *  ou      unity command run_tests --mode EditMode --filter_type category --filter ParteC
 * ============================================================ */

[Category("ParteC")]
public class PlacarTestes
{
    private const string Prefixo = "teste.placar.";

    private FluxoDeJogo fluxo;
    private GerenciadorDeFocos gerenciador;
    private PlacarDaPartida placar;
    private KitDoAgente kit;

    [SetUp]
    public void Preparar()
    {
        new RecordeDoJogo(Prefixo).Apagar();

        fluxo = TesteUtil.Com<FluxoDeJogo>("Fluxo (teste)");
        gerenciador = TesteUtil.Com<GerenciadorDeFocos>("Gerenciador (teste)");
        TesteUtil.Setar(gerenciador, "fluxo", fluxo);
        TesteUtil.Setar(fluxo, "gerenciador", gerenciador);
        fluxo.Ligar();

        placar = TesteUtil.Com<PlacarDaPartida>("Placar (teste)");
        TesteUtil.Setar(placar, "fluxo", fluxo);
        TesteUtil.Setar(placar, "gerenciador", gerenciador);
        placar.UsarRecorde(new RecordeDoJogo(Prefixo));
        placar.Ligar();

        fluxo.DefinirEstadoParaTeste(true, false);

        ControladorJogador jogador = TesteUtil.Com<ControladorJogador>("Jogador (teste)");
        kit = jogador.gameObject.AddComponent<KitDoAgente>();
    }

    [TearDown]
    public void Limpar()
    {
        new RecordeDoJogo(Prefixo).Apagar();
        TesteUtil.LimparTudo();
    }

    /* Vaso esvaziado (3 passos com as Mãos) e registrado no gerenciador. */
    private VasoInteractable NovoVasoEsvaziado()
    {
        VasoInteractable vaso = TesteUtil.Com<VasoInteractable>("Vaso (teste)");
        TesteUtil.Setar(vaso, "ferramentaNecessaria", Ferramenta.Areia);
        TesteUtil.Setar(vaso, "podeEsvaziar", true);
        TesteUtil.Setar(vaso, "requiredClicks", 3);
        TesteUtil.Setar(vaso, "punchScale", 0f);
        TesteUtil.Setar(vaso, "shrinkDuration", 0f);
        gerenciador.Registrar(new Interactable[] { vaso });

        for (int i = 0; i < 3; i++)
            vaso.AvancarCom(kit);
        Assert.IsTrue(vaso.Esvaziado, "o vaso de teste começa esvaziado");
        return vaso;
    }

    [Test]
    public void Pico_SobeENaoDesce()
    {
        TesteUtil.Setar(fluxo, "ocupacaoInicial", 0.9f);
        fluxo.Ligar();
        Assert.AreEqual(0.9f, fluxo.OcupacaoPico, 1e-4f);
        fluxo.AvancarOcupacao(100f);          // sem focos, a ocupação desce
        Assert.Less(fluxo.Ocupacao, 0.9f);
        Assert.AreEqual(0.9f, fluxo.OcupacaoPico, 1e-4f);
    }

    [Test]
    public void Pico_SobeComPressao()
    {
        NovoFoco("Foco A (teste)");
        TesteUtil.Setar(fluxo, "ocupacaoInicial", 0.3f);
        fluxo.Ligar();
        fluxo.AvancarOcupacao(60f);
        Assert.Greater(fluxo.Ocupacao, 0.3f, "com foco de pé a ocupação sobe");
        Assert.Greater(fluxo.OcupacaoPico, 0.3f);
        Assert.AreEqual(fluxo.Ocupacao, fluxo.OcupacaoPico, 1e-4f);
    }

    [Test]
    public void Tempo_CaiUmaVez()
    {
        int avisos = 0;
        placar.OnMetaPerdida += m => avisos++;
        fluxo.AvancarOcupacao(1201f);
        placar.Conferir();
        placar.Conferir();
        Assert.AreEqual(1, avisos);
        Assert.IsFalse(placar.MetaViva(MetaDaPartida.RespostaRapida));
    }

    [Test]
    public void Hospital_CaiAoPassarDoLimite()
    {
        TesteUtil.Setar(fluxo, "ocupacaoInicial", 0.9f);
        fluxo.Ligar();
        placar.Conferir();
        Assert.IsFalse(placar.MetaViva(MetaDaPartida.HospitalSobControle));
    }

    [Test]
    public void Reencher_SoConta_ErroDerruba()
    {
        VasoInteractable vaso = NovoVasoEsvaziado();
        placar.Conferir();
        Assert.IsTrue(placar.MetaViva(MetaDaPartida.SemErros));
        vaso.Reencher();
        Assert.AreEqual(1, placar.Reenchidos);
        Assert.IsTrue(placar.MetaViva(MetaDaPartida.SemErros), "reencher não derruba mais");
        placar.HandleErrouMinigame(vaso);
        Assert.AreEqual(1, placar.Erros);
        Assert.IsFalse(placar.MetaViva(MetaDaPartida.SemErros));
    }

    [Test]
    public void Fechar_DuasVezes_GravaUmaEMantemOPrimeiro()
    {
        ResultadoDaNota a = placar.Fechar(true);
        ResultadoDaNota b = placar.Fechar(false);
        Assert.AreEqual(a.Pontos, b.Pontos);
        Assert.IsTrue(placar.Dados.Venceu);
    }

    [Test]
    public void Fechar_NotaMaior_MarcaRecordeNovo()
    {
        placar.Fechar(true);
        Assert.IsTrue(placar.RecordeNovo);
        Assert.Greater(placar.Recorde.Pontos, 0);
    }

    [Test]
    public void ForaDaPartida_NaoConta()
    {
        fluxo.DefinirEstadoParaTeste(false, false);
        fluxo.AvancarOcupacao(1201f);
        placar.Conferir();
        Assert.IsTrue(placar.MetaViva(MetaDaPartida.RespostaRapida));
        fluxo.DefinirEstadoParaTeste(true, true);
        placar.Conferir();
        Assert.IsTrue(placar.MetaViva(MetaDaPartida.RespostaRapida));
    }

    [Test]
    public void AntesDaPorta_MetasVivas()
    {
        fluxo.DefinirEstadoParaTeste(false, false);
        placar.Conferir();
        foreach (MetaDaPartida meta in AvaliacaoDaPartida.Metas)
            Assert.IsTrue(placar.MetaViva(meta));
        StringAssert.StartsWith("00:00 de", AvaliacaoDaPartida.DetalheAoVivo(MetaDaPartida.RespostaRapida, placar.Dados, placar.Regras));
        Assert.AreEqual("pico 70% de 85%", AvaliacaoDaPartida.DetalheAoVivo(MetaDaPartida.HospitalSobControle, placar.Dados, placar.Regras));
    }

    /* Foco de 1 clique, registrado no gerenciador. */
    private Interactable NovoFoco(string nome)
    {
        Interactable foco = TesteUtil.Com<Interactable>(nome);
        TesteUtil.Setar(foco, "requiredClicks", 1);
        TesteUtil.Setar(foco, "punchScale", 0f);
        TesteUtil.Setar(foco, "shrinkDuration", 0f);
        gerenciador.Registrar(new[] { foco });
        return foco;
    }

    /* O OnDestroy não roda de forma confiável no EditMode: chamado direto. */
    private static void ChamarOnDestroy(PlacarDaPartida alvo)
    {
        typeof(PlacarDaPartida).GetMethod("OnDestroy",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
            .Invoke(alvo, null);
    }

    [Test]
    public void Vitoria_Real_FechaSozinhoEGrava()
    {
        Interactable a = NovoFoco("Foco A (teste)");
        Interactable b = NovoFoco("Foco B (teste)");
        a.Interact();
        Assert.IsFalse(placar.Fechado, "um foco de pé: ainda não fechou");
        b.Interact();

        Assert.IsTrue(fluxo.PartidaEncerrada);
        Assert.IsTrue(placar.Fechado);
        Assert.IsTrue(placar.Dados.Venceu);
        Assert.AreEqual(2, placar.Dados.Eliminados);
        Assert.Greater(placar.Resultado.Pontos, 0);
        Assert.AreEqual(placar.Resultado.Pontos, new RecordeDoJogo(Prefixo).Pontos, "gravou no recorde");
    }

    [Test]
    public void Derrota_Real_FechaSozinhoSemVencer()
    {
        NovoFoco("Foco A (teste)");
        TesteUtil.Setar(fluxo, "ocupacaoInicial", 0.99f);
        fluxo.Ligar();

        fluxo.AvancarOcupacao(10000f);

        Assert.IsTrue(fluxo.PartidaEncerrada);
        Assert.IsTrue(placar.Fechado);
        Assert.IsFalse(placar.Dados.Venceu);
        Assert.AreEqual(0, placar.Resultado.Estrelas);
    }

    [Test]
    public void Destruir_SemFechar_NaoGrava()
    {
        NovoFoco("Foco A (teste)");
        TesteUtil.Setar(fluxo, "ocupacaoInicial", 0.99f);
        fluxo.Ligar();
        VasoInteractable vaso = NovoVasoEsvaziado();
        int antes = placar.Recorde.Pontos;

        PlacarDaPartida morto = placar;
        Object.DestroyImmediate(placar);
        ChamarOnDestroy(morto);

        vaso.Reencher();
        fluxo.AvancarOcupacao(10000f);   // derrota real: o placar solto não pode reagir

        Assert.IsTrue(fluxo.PartidaEncerrada);
        Assert.AreEqual(0, morto.Reenchidos, "o reenchido não é mais contado");
        Assert.IsFalse(morto.Fechado, "a derrota não fecha mais");
        Assert.AreEqual(antes, new RecordeDoJogo(Prefixo).Pontos, "nada gravado");
    }

    [Test]
    public void Reenchido_ForaDaPartida_NaoConta()
    {
        VasoInteractable naoIniciada = NovoVasoEsvaziado();
        VasoInteractable encerrada = NovoVasoEsvaziado();

        fluxo.DefinirEstadoParaTeste(false, false);
        naoIniciada.Reencher();
        placar.HandleErrouMinigame(naoIniciada);
        Assert.AreEqual(0, placar.Reenchidos, "partida não iniciada");
        Assert.AreEqual(0, placar.Erros, "partida não iniciada");
        Assert.IsTrue(placar.MetaViva(MetaDaPartida.SemErros));

        fluxo.DefinirEstadoParaTeste(true, true);
        encerrada.Reencher();
        placar.HandleErrouMinigame(encerrada);
        Assert.AreEqual(0, placar.Reenchidos, "partida encerrada");
        Assert.AreEqual(0, placar.Erros, "partida encerrada");
        Assert.IsTrue(placar.MetaViva(MetaDaPartida.SemErros));
    }

    [Test]
    public void Fechar_SemFluxo_NaoQuebra()
    {
        // Tira o fluxo e o gerenciador do SetUp: o Ligar do placar solto não acha nada.
        Object.DestroyImmediate(placar);
        Object.DestroyImmediate(fluxo);
        Object.DestroyImmediate(gerenciador);

        PlacarDaPartida solto = TesteUtil.Com<PlacarDaPartida>("Solto (teste)");
        solto.UsarRecorde(new RecordeDoJogo(Prefixo));
        Assert.DoesNotThrow(() => { solto.Ligar(); solto.Conferir(); solto.Fechar(false); });
        Assert.AreEqual(0f, solto.Dados.Segundos);
        Assert.AreEqual(0f, solto.Dados.Pico);
        Assert.IsTrue(solto.Fechado);
    }
}
