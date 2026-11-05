using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

/* ============================================================
 *  GerenciadorDeFocosTestes.cs
 *  ---------------------------------------------------------------------
 *  Testes EditMode (Parte B) do GerenciadorDeFocos: a contagem dos focos
 *  (Total, Eliminados, Restantes), o repasse dos eventos, o ciclo de
 *  vida de cada foco (ovo → larva → mosquito) com o peso de cada
 *  estágio na Pressao, o esvaziar (volta a ovo, pesa 0), o reencher da
 *  chuva (volta como larva, mas nunca no pneu nos braços nem em foco já
 *  eliminado) e o sorteio do começo da partida.
 *
 *  O Awake/Start não rodam no EditMode: os focos de teste entram pelo
 *  Registrar, e o FluxoDeJogo de teste é ligado no campo 'fluxo' (o
 *  FindAnyObjectByType não acha objetos escondidos do TesteUtil). As
 *  corrotinas não andam: os focos de teste não têm punch (punchScale 0)
 *  nem animação de conclusão (shrinkDuration 0: o OnComplete só desativa
 *  o objeto).
 *
 *  O foco de teste padrão pede Areia, tem o atalho de esvaziar
 *  (podeEsvaziar, 3 passos com as Mãos) e 1 clique: o Interact() o
 *  conclui de uma vez. O kit fica num objeto com ControladorJogador, usa
 *  o estoque padrão (areia 1) e começa com as Mãos selecionadas.
 *
 *  Os objetos nascem pelo TesteUtil (escondidos, fora da cena) e são
 *  destruídos no [TearDown].
 *
 *  Rodar:  Window > General > Test Runner > EditMode (categoria ParteB)
 *  ou      unity command run_tests --mode EditMode --filter_type category --filter ParteB
 * ============================================================ */

[Category("ParteB")]
public class GerenciadorDeFocosTestes
{
    // Bem abaixo de qualquer cena do jogo: aqui o raycast não acha chão.
    private static readonly Vector3 SemChao = new Vector3(0f, -10000f, 0f);

    private GerenciadorDeFocos gerenciador;
    private FluxoDeJogo fluxo;
    private KitDoAgente kit;

    [SetUp]
    public void Preparar()
    {
        gerenciador = TesteUtil.Com<GerenciadorDeFocos>("Gerenciador (teste)");
        fluxo = TesteUtil.Com<FluxoDeJogo>("Fluxo (teste)");
        TesteUtil.Setar(gerenciador, "fluxo", fluxo);

        ControladorJogador jogador = TesteUtil.Com<ControladorJogador>("Jogador (teste)");
        kit = jogador.gameObject.AddComponent<KitDoAgente>();
    }

    [TearDown]
    public void Limpar()
    {
        TesteUtil.LimparTudo();
    }

    /* Foco que pede Areia, com o atalho de esvaziar e 1 clique. */
    private static Interactable NovoFoco(string nome = "Foco (teste)")
    {
        Interactable foco = TesteUtil.Com<Interactable>(nome);
        TesteUtil.Setar(foco, "ferramentaNecessaria", Ferramenta.Areia);
        TesteUtil.Setar(foco, "podeEsvaziar", true);
        TesteUtil.Setar(foco, "requiredClicks", 1);
        TesteUtil.Setar(foco, "punchScale", 0f);
        TesteUtil.Setar(foco, "shrinkDuration", 0f);
        return foco;
    }

    private static List<Interactable> NovosFocos(int quantos)
    {
        List<Interactable> focos = new List<Interactable>();
        for (int i = 0; i < quantos; i++)
            focos.Add(NovoFoco("Foco " + i + " (teste)"));
        return focos;
    }

    /* Os 3 passos de esvaziar com as Mãos (o kit começa nelas). */
    private void Esvaziar(Interactable foco)
    {
        for (int i = 0; i < 3; i++)
            foco.AvancarCom(kit);
        Assert.IsTrue(foco.Esvaziado, "o foco de teste esvaziou");
    }

    private int Contar(EstagioDoFoco estagio)
    {
        int n = 0;
        foreach (Interactable foco in gerenciador.Focos)
            if (gerenciador.EstagioDe(foco) == estagio)
                n++;
        return n;
    }

    [Test]
    public void Registrar_ContaTotalEliminados()
    {
        List<Interactable> focos = NovosFocos(3);
        focos[0].Interact();
        Assert.IsTrue(focos[0].IsComplete, "o foco de teste concluiu antes de registrar");

        gerenciador.Registrar(focos);

        Assert.AreEqual(3, gerenciador.Total);
        Assert.AreEqual(1, gerenciador.Eliminados);
        Assert.AreEqual(2, gerenciador.Restantes);
    }

    [Test]
    public void RepassaEliminado()
    {
        List<Interactable> focos = NovosFocos(2);
        gerenciador.Registrar(focos);
        int eliminados = 0;
        Interactable ultimo = null;
        gerenciador.OnFocoEliminado += f =>
        {
            eliminados++;
            ultimo = f;
        };

        focos[1].Interact();

        Assert.AreEqual(1, eliminados, "OnFocoEliminado 1 vez");
        Assert.AreSame(focos[1], ultimo);
        Assert.AreEqual(1, gerenciador.Eliminados);
        Assert.AreEqual(1, gerenciador.Restantes);
    }

    [Test]
    public void Pressao_Pesos()
    {
        // O Avancar anda com todos os registrados: cada foco entra depois,
        // para parar num estágio diferente.
        Interactable mosquito = NovoFoco("Mosquito (teste)");
        gerenciador.Registrar(new[] { mosquito });
        gerenciador.Avancar(150f);

        Interactable larva = NovoFoco("Larva (teste)");
        gerenciador.Registrar(new[] { larva });
        gerenciador.Avancar(75f);

        Interactable ovo = NovoFoco("Ovo (teste)");
        Interactable esvaziado = NovoFoco("Esvaziado (teste)");
        gerenciador.Registrar(new[] { ovo, esvaziado });
        Esvaziar(esvaziado);

        Assert.AreEqual(EstagioDoFoco.Mosquito, gerenciador.EstagioDe(mosquito));
        Assert.AreEqual(EstagioDoFoco.Larva, gerenciador.EstagioDe(larva));
        Assert.AreEqual(EstagioDoFoco.Ovo, gerenciador.EstagioDe(ovo));
        Assert.AreEqual(2.5f, gerenciador.PesoDe(mosquito), 1e-5f);
        Assert.AreEqual(1.5f, gerenciador.PesoDe(larva), 1e-5f);
        Assert.AreEqual(1f, gerenciador.PesoDe(ovo), 1e-5f);
        Assert.AreEqual(0f, gerenciador.PesoDe(esvaziado), 1e-5f);
        Assert.AreEqual(1.25f, gerenciador.Pressao(), 1e-5f, "(1 + 1,5 + 2,5 + 0) / 4");
    }

    [Test]
    public void Pressao_ConcluidoPesaZero()
    {
        List<Interactable> focos = NovosFocos(2);
        gerenciador.Registrar(focos);
        gerenciador.Avancar(150f);
        Assert.AreEqual(2.5f, gerenciador.Pressao(), 1e-5f, "dois mosquitos");

        focos[0].Interact();

        Assert.AreEqual(0f, gerenciador.PesoDe(focos[0]), 1e-5f);
        Assert.AreEqual(1.25f, gerenciador.Pressao(), 1e-5f, "(2,5 + 0) / 2: o concluído ainda conta no Total");
    }

    [Test]
    public void Pressao_SemFocos_Zero()
    {
        Assert.AreEqual(0, gerenciador.Total);
        Assert.AreEqual(0f, gerenciador.Pressao());
    }

    [Test]
    public void Avancar_MudaEstagioEDispara()
    {
        Interactable foco = NovoFoco();
        gerenciador.Registrar(new[] { foco });
        List<EstagioDoFoco> mudancas = new List<EstagioDoFoco>();
        Interactable avisado = null;
        gerenciador.OnEstagioMudou += (f, e) =>
        {
            avisado = f;
            mudancas.Add(e);
        };

        gerenciador.Avancar(74f);
        Assert.AreEqual(0, mudancas.Count, "74 s: ainda ovo");

        gerenciador.Avancar(1f);

        Assert.AreEqual(new[] { EstagioDoFoco.Larva }, mudancas.ToArray());
        Assert.AreSame(foco, avisado);
        Assert.AreEqual(EstagioDoFoco.Larva, gerenciador.EstagioDe(foco));
    }

    [Test]
    public void Avancar_NaoMexeEmEsvaziado()
    {
        Interactable foco = NovoFoco();
        gerenciador.Registrar(new[] { foco });
        Esvaziar(foco);
        int mudancas = 0;
        gerenciador.OnEstagioMudou += (f, e) => mudancas++;

        gerenciador.Avancar(200f);

        Assert.AreEqual(EstagioDoFoco.Ovo, gerenciador.EstagioDe(foco));
        Assert.AreEqual(0, mudancas, "OnEstagioMudou");
    }

    [Test]
    public void Avancar_NaoMexeEmConcluido()
    {
        Interactable foco = NovoFoco();
        gerenciador.Registrar(new[] { foco });
        foco.Interact();
        int mudancas = 0;
        gerenciador.OnEstagioMudou += (f, e) => mudancas++;

        gerenciador.Avancar(200f);

        Assert.AreEqual(EstagioDoFoco.Ovo, gerenciador.EstagioDe(foco));
        Assert.AreEqual(0, mudancas, "OnEstagioMudou");
    }

    [Test]
    public void CicloInativo_AntesDaPartida()
    {
        Assert.IsFalse(fluxo.PartidaIniciada, "o fluxo de teste nasce antes da partida");
        Assert.IsFalse(gerenciador.CicloAtivo);
    }

    [Test]
    public void Esvaziar_ZeraCiclo()
    {
        Interactable foco = NovoFoco();
        gerenciador.Registrar(new[] { foco });
        gerenciador.Avancar(150f);
        Assert.AreEqual(EstagioDoFoco.Mosquito, gerenciador.EstagioDe(foco));
        List<EstagioDoFoco> mudancas = new List<EstagioDoFoco>();
        gerenciador.OnEstagioMudou += (f, e) => mudancas.Add(e);
        int esvaziados = 0;
        gerenciador.OnFocoEsvaziado += f => esvaziados++;

        Esvaziar(foco);

        Assert.AreEqual(EstagioDoFoco.Ovo, gerenciador.EstagioDe(foco));
        Assert.AreEqual(0f, gerenciador.PesoDe(foco), 1e-5f);
        Assert.AreEqual(new[] { EstagioDoFoco.Ovo }, mudancas.ToArray(), "OnEstagioMudou(foco, Ovo)");
        Assert.AreEqual(1, esvaziados, "OnFocoEsvaziado 1 vez");
        Assert.AreEqual(0, gerenciador.Eliminados, "esvaziar não elimina");
    }

    [Test]
    public void Reencher_VoltaLarva()
    {
        Interactable foco = NovoFoco();
        gerenciador.Registrar(new[] { foco });
        Esvaziar(foco);
        List<EstagioDoFoco> mudancas = new List<EstagioDoFoco>();
        gerenciador.OnEstagioMudou += (f, e) => mudancas.Add(e);
        int reenchidos = 0;
        gerenciador.OnFocoReenchido += f => reenchidos++;

        Assert.AreEqual(1, gerenciador.ReencherEsvaziados());

        Assert.IsFalse(foco.Esvaziado);
        Assert.AreEqual(EstagioDoFoco.Larva, gerenciador.EstagioDe(foco));
        Assert.AreEqual(1.5f, gerenciador.PesoDe(foco), 1e-5f);
        Assert.AreEqual(new[] { EstagioDoFoco.Larva }, mudancas.ToArray(), "OnEstagioMudou(foco, Larva)");
        Assert.AreEqual(1, reenchidos, "OnFocoReenchido 1 vez");
        Assert.AreEqual(0, gerenciador.ReencherEsvaziados(), "já com água: nada a reencher");
    }

    /* Review Focus 1: o pneu esvaziado nos braços do agente não reenche. */
    [Test]
    public void Reencher_PneuCarregado_NaoReenche()
    {
        PneuInteractable pneu = TesteUtil.Com<PneuInteractable>("Pneu (teste)");
        TesteUtil.Setar(pneu, "requiredClicks", 2);
        TesteUtil.Setar(pneu, "punchScale", 0f);
        TesteUtil.Setar(pneu, "shrinkDuration", 0f);
        pneu.transform.position = SemChao;
        GameObject agua = TesteUtil.Novo("agua do pneu (teste)");
        agua.transform.SetParent(pneu.transform, false);
        TesteUtil.Setar(pneu, "water", agua);
        kit.PontoDeCarga = TesteUtil.Novo("Ponto de carga (teste)").transform;
        gerenciador.Registrar(new Interactable[] { pneu });

        pneu.AvancarCom(kit);
        pneu.AvancarCom(kit);
        Assert.IsTrue(pneu.Esvaziado, "os 2 passos com as Mãos esvaziam o pneu");
        Assert.IsTrue(kit.Pegar(pneu), "o pneu vai para os braços");
        int reenchidos = 0;
        gerenciador.OnFocoReenchido += f => reenchidos++;

        Assert.AreEqual(0, gerenciador.ReencherEsvaziados());

        Assert.IsTrue(pneu.Esvaziado);
        Assert.IsFalse(agua.activeSelf, "a água não volta nos braços");
        Assert.AreEqual(0, reenchidos, "OnFocoReenchido");
        Assert.AreEqual(0f, gerenciador.PesoDe(pneu), 1e-5f);
    }

    /* Review Focus 2: esvaziado e depois eliminado com a Areia, o foco não volta. */
    [Test]
    public void Reencher_IgnoraConcluidos()
    {
        Interactable foco = NovoFoco();
        gerenciador.Registrar(new[] { foco });
        Esvaziar(foco);
        kit.Selecionar(Ferramenta.Areia);
        Assert.AreEqual(ResultadoDoPasso.Concluiu, foco.AvancarCom(kit), "a Areia elimina o foco esvaziado");
        int reenchidos = 0;
        gerenciador.OnFocoReenchido += f => reenchidos++;

        Assert.AreEqual(0, gerenciador.ReencherEsvaziados());

        Assert.IsTrue(foco.IsComplete);
        Assert.AreEqual(0, reenchidos, "OnFocoReenchido");
        Assert.AreEqual(0f, gerenciador.PesoDe(foco), 1e-5f);
        Assert.AreEqual(1, gerenciador.Eliminados);
    }

    [Test]
    public void Sortear_RespeitaMosquitos()
    {
        gerenciador.Registrar(NovosFocos(10));
        int avisos = 0;
        gerenciador.OnEstagioMudou += (f, e) => avisos++;

        gerenciador.SortearInicio(new System.Random(1));

        Assert.AreEqual(3, Contar(EstagioDoFoco.Mosquito), "focosComMosquitoNoInicio");
        Assert.AreEqual(2, Contar(EstagioDoFoco.Larva), "round(7 / 3)");
        Assert.AreEqual(5, Contar(EstagioDoFoco.Ovo), "o resto");
        Assert.AreEqual(5, avisos, "OnEstagioMudou para cada um que não é ovo");

        // O tempo sorteado fica em [0, 75): um instante a mais não muda
        // ninguém de estágio, e 75 s levam todo ovo e toda larva adiante.
        gerenciador.Avancar(0.001f);
        Assert.AreEqual(5, Contar(EstagioDoFoco.Ovo), "nenhum tempo sorteado chegou a 75 s");
        Assert.AreEqual(2, Contar(EstagioDoFoco.Larva));

        gerenciador.Avancar(75f);
        Assert.AreEqual(0, Contar(EstagioDoFoco.Ovo), "todo tempo sorteado é >= 0");
    }

    [Test]
    public void Sortear_PoucosFocos_NaoPassaDoTotal()
    {
        gerenciador.Registrar(NovosFocos(2));

        Assert.DoesNotThrow(() => gerenciador.SortearInicio(new System.Random(1)));

        Assert.AreEqual(2, Contar(EstagioDoFoco.Mosquito));
        Assert.AreEqual(0, Contar(EstagioDoFoco.Larva));
        Assert.AreEqual(0, Contar(EstagioDoFoco.Ovo));
    }

    /* A regra do sorteio sem tocar em foco, para o simulador usar a mesma:
     * com a mesma semente, sai o que o SortearInicio aplica. */
    [Test]
    public void SortearEstagios_EhARegraDoSortearInicio()
    {
        List<Interactable> focos = NovosFocos(10);
        gerenciador.Registrar(focos);

        var sorteio = gerenciador.SortearEstagios(10, new System.Random(7));
        gerenciador.SortearInicio(new System.Random(7));

        Assert.AreEqual(10, sorteio.Length);
        foreach (var (indice, estagio, tempo) in sorteio)
        {
            Assert.AreEqual(gerenciador.EstagioDe(focos[indice]), estagio, "foco " + indice);
            Assert.That(tempo, Is.InRange(0f, gerenciador.SegundosPorEstagio));
        }
    }

    [Test]
    public void PesoDoEstagio_EhOPesoDoInspector()
    {
        TesteUtil.Setar(gerenciador, "pesoOvo", 2f);
        TesteUtil.Setar(gerenciador, "pesoLarva", 3f);
        TesteUtil.Setar(gerenciador, "pesoMosquito", 5f);

        Assert.AreEqual(2f, gerenciador.PesoDoEstagio(EstagioDoFoco.Ovo), 1e-5f);
        Assert.AreEqual(3f, gerenciador.PesoDoEstagio(EstagioDoFoco.Larva), 1e-5f);
        Assert.AreEqual(5f, gerenciador.PesoDoEstagio(EstagioDoFoco.Mosquito), 1e-5f);
    }

    [Test]
    public void Registrar_MesmoFocoDuasVezes_NaoDuplica()
    {
        Interactable foco = NovoFoco();
        gerenciador.Registrar(new[] { foco, null });
        gerenciador.Registrar(new[] { foco });
        int eliminados = 0;
        gerenciador.OnFocoEliminado += f => eliminados++;

        foco.Interact();

        Assert.AreEqual(1, gerenciador.Total, "o null é ignorado e o repetido não entra de novo");
        Assert.AreEqual(1, eliminados, "assinado uma vez só");
        Assert.AreEqual(1, gerenciador.Eliminados);
    }
}
