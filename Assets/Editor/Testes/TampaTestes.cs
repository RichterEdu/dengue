using NUnit.Framework;
using UnityEngine;

/* ============================================================
 *  TampaTestes.cs
 *  ---------------------------------------------------------------------
 *  Testes EditMode da caixa d'água com a tampa carregável: com as Mãos,
 *  um passo põe a tampa nos braços (Pegou, filha do PontoDeCarga, com os
 *  colliders dela desligados e o agente limitado como com o pneu); o
 *  passo seguinte tampa e conclui (a tampa volta a ser filha da caixa,
 *  fechada, com os colliders religados). Devolver (o botão de largar)
 *  leva a tampa de volta à lateral. Com a tampa nos braços, os outros
 *  focos e as outras caixas recusam (MaosOcupadas), o pneu não entra nos
 *  braços, a ferramenta não troca e a chuva não reenche a caixa. Bloquear
 *  o Interactor (pausa, cutscene) deixa a tampa nos braços. O caminho
 *  antigo Interact() (sem kit) continua deslizando a tampa clique a
 *  clique, e sem kit o passo segurado dura o da base (não é na hora).
 *
 *  "Soltar antes de tampar" (o Interactor exige soltar o botão depois do
 *  Pegou) depende da entrada do jogador e não é observável aqui.
 *
 *  O Start não roda no EditMode e as corrotinas não andam: fora do Play
 *  a tampa vai direto à pose fechada. Os focos de teste não têm punch
 *  (punchScale 0) nem animação de conclusão (shrinkDuration 0). O kit
 *  fica num objeto com ControladorJogador, com um PontoDeCarga de teste.
 *
 *  Rodar:  unity test --mode EditMode --filter TampaTestes
 * ============================================================ */

[Category("Minigames")]
public class TampaTestes
{
    // As poses padrão do CaixaDaguaInteractable (o TesteUtil não grava Vector3).
    private static readonly Vector3 PosAberta = new Vector3(0.44f, 0.45f, 0f);
    private static readonly Vector3 RotAberta = new Vector3(0f, 0f, 62f);
    private static readonly Vector3 PosFechada = new Vector3(0f, 0.762f, 0f);

    private ControladorJogador jogador;
    private KitDoAgente kit;
    private Transform ponto;
    private CaixaDaguaInteractable caixa;
    private Transform tampa;
    private BoxCollider colliderDaTampa;

    [SetUp]
    public void Preparar()
    {
        jogador = TesteUtil.Com<ControladorJogador>("Jogador (teste)");
        kit = jogador.gameObject.AddComponent<KitDoAgente>();
        ponto = TesteUtil.Novo("Ponto de carga (teste)").transform;
        ponto.SetParent(jogador.transform, false);
        kit.PontoDeCarga = ponto;

        caixa = NovaCaixa("Caixa d'água (teste)", out tampa);
        colliderDaTampa = tampa.gameObject.AddComponent<BoxCollider>();
    }

    [TearDown]
    public void Limpar()
    {
        TesteUtil.LimparTudo();
    }

    /* Caixa de 5 cliques com uma tampa filha, encostada na lateral. */
    private static CaixaDaguaInteractable NovaCaixa(string nome, out Transform tampaDaCaixa)
    {
        CaixaDaguaInteractable nova = TesteUtil.Com<CaixaDaguaInteractable>(nome);
        TesteUtil.Setar(nova, "requiredClicks", 5);
        TesteUtil.Setar(nova, "punchScale", 0f);
        TesteUtil.Setar(nova, "shrinkDuration", 0f);

        tampaDaCaixa = TesteUtil.Novo("Tampa (teste)").transform;
        tampaDaCaixa.SetParent(nova.transform, false);
        tampaDaCaixa.localPosition = PosAberta;
        tampaDaCaixa.localRotation = Quaternion.Euler(RotAberta);
        TesteUtil.Setar(nova, "tampa", tampaDaCaixa);
        return nova;
    }

    private static void AssertPerto(Vector3 esperado, Vector3 obtido, string onde)
    {
        Assert.That(Vector3.Distance(esperado, obtido), Is.LessThan(1e-3f),
                    $"{onde}: {obtido.ToString("F3")}, esperado {esperado.ToString("F3")}");
    }

    private static void AssertGiro(Quaternion esperado, Quaternion obtido, string onde)
    {
        Assert.That(Quaternion.Angle(esperado, obtido), Is.LessThan(0.1f), onde);
    }

    [Test]
    public void SemKitOPassoDuraODaBase()
    {
        // segundosParaConcluir (2 s, o padrão) dividido pelos 5 cliques.
        Assert.AreEqual(2f / 5f, caixa.SegundosPorPassoCom(null), 1e-5f, "sem kit, clique a clique como antes");
        Assert.AreEqual(0f, caixa.SegundosPorPassoCom(kit), 1e-5f, "com kit, pegar a tampa é na hora");

        Assert.AreEqual(ResultadoDoPasso.Pegou, caixa.AvancarCom(kit));
        Assert.AreEqual(0.8f, caixa.SegundosPorPassoCom(kit), 1e-5f, "nos braços, segundosParaTampar");
    }

    [Test]
    public void PegarPoeATampaNosBracos()
    {
        int pegas = 0;
        kit.OnTampaPega += c => pegas++;

        Assert.AreEqual(ResultadoDoPasso.Pegou, caixa.AvancarCom(kit));

        Assert.AreSame(caixa, kit.TampaCarregada);
        Assert.IsTrue(kit.MaosOcupadas);
        Assert.IsTrue(caixa.TampaNosBracos);
        Assert.AreSame(ponto, tampa.parent, "a tampa fica no PontoDeCarga");
        Assert.IsFalse(jogador.PodePular);
        Assert.Less(jogador.MultiplicadorVelocidade, 1f, "mais lento, como com o pneu");
        Assert.IsFalse(caixa.IsComplete);
        Assert.IsTrue(caixa.CanInteract, "a caixa continua alvo, para tampar");
        Assert.IsFalse(colliderDaTampa.enabled, "nos braços, os colliders da tampa desligam");
        Assert.AreEqual(1, pegas, "OnTampaPega");
    }

    [Test]
    public void TamparConclui()
    {
        int colocadas = 0;
        int concluidas = 0;
        kit.OnTampaColocada += c => colocadas++;
        caixa.OnCompleted += f => concluidas++;

        caixa.AvancarCom(kit);
        Assert.AreEqual(0.8f, caixa.SegundosPorPasso, 1e-5f, "segurar para tampar");
        Assert.AreEqual(ResultadoDoPasso.Concluiu, caixa.AvancarCom(kit), "um passo só tampa, apesar dos 5 cliques");

        Assert.IsTrue(caixa.IsComplete);
        Assert.IsNull(kit.TampaCarregada);
        Assert.IsFalse(kit.MaosOcupadas);
        Assert.IsFalse(caixa.TampaNosBracos);
        Assert.AreSame(caixa.transform, tampa.parent, "a tampa volta a ser filha da caixa");
        AssertPerto(PosFechada, tampa.localPosition, "tampa assentada na boca");
        AssertGiro(Quaternion.identity, tampa.localRotation, "giro da tampa fechada");
        Assert.AreEqual(1f, caixa.Fechamento, 1e-5f);
        Assert.IsTrue(jogador.PodePular, "movimento restaurado");
        Assert.AreEqual(1f, jogador.MultiplicadorVelocidade, 1e-5f, "movimento restaurado");
        Assert.AreEqual(1, colocadas, "OnTampaColocada");
        Assert.AreEqual(1, concluidas, "OnCompleted");
    }

    [Test]
    public void DevolverVoltaALateral()
    {
        int devolvidas = 0;
        kit.OnTampaDevolvida += c => devolvidas++;
        Vector3 escala = tampa.localScale;

        caixa.AvancarCom(kit);
        Assert.IsTrue(kit.DevolverTampa());

        Assert.IsNull(kit.TampaCarregada);
        Assert.IsFalse(caixa.TampaNosBracos);
        Assert.AreSame(caixa.transform, tampa.parent);
        AssertPerto(PosAberta, tampa.localPosition, "tampa encostada na lateral");
        AssertGiro(Quaternion.Euler(RotAberta), tampa.localRotation, "giro da tampa aberta");
        AssertPerto(escala, tampa.localScale, "escala local restaurada");
        Assert.AreEqual(0f, caixa.Fechamento);
        Assert.IsFalse(caixa.IsComplete);
        Assert.IsTrue(caixa.CanInteract);
        Assert.IsTrue(jogador.PodePular, "movimento restaurado");
        Assert.AreEqual(1f, jogador.MultiplicadorVelocidade, 1e-5f, "movimento restaurado");
        Assert.AreEqual(1, devolvidas, "OnTampaDevolvida");

        Assert.IsFalse(kit.DevolverTampa(), "sem tampa nos braços");
    }

    [Test]
    public void OutroFocoRecusaComATampa()
    {
        VasoInteractable vaso = TesteUtil.Com<VasoInteractable>("Vaso (teste)");
        TesteUtil.Setar(vaso, "punchScale", 0f);
        TesteUtil.Setar(vaso, "shrinkDuration", 0f);

        caixa.AvancarCom(kit);

        Assert.AreEqual(ResultadoDoPasso.MaosOcupadas, vaso.Verificar(kit));
    }

    [Test]
    public void OutraCaixaRecusaComATampa()
    {
        CaixaDaguaInteractable outra = NovaCaixa("Outra caixa (teste)", out Transform outraTampa);

        caixa.AvancarCom(kit);

        Assert.AreEqual(ResultadoDoPasso.MaosOcupadas, outra.Verificar(kit));
        Assert.AreEqual(ResultadoDoPasso.MaosOcupadas, outra.AvancarCom(kit));
        Assert.AreSame(outra.transform, outraTampa.parent, "a tampa da outra caixa fica onde está");
        Assert.AreSame(caixa, kit.TampaCarregada);
    }

    [Test]
    public void PneuETampaNaoCoexistem()
    {
        PneuInteractable pneu = TesteUtil.Com<PneuInteractable>("Pneu (teste)");
        TesteUtil.Setar(pneu, "punchScale", 0f);
        TesteUtil.Setar(pneu, "shrinkDuration", 0f);

        caixa.AvancarCom(kit);
        Assert.IsFalse(kit.Pegar(pneu), "com a tampa, o pneu não entra nos braços");
        Assert.IsNull(kit.PneuCarregado);

        kit.DevolverTampa();
        Assert.IsTrue(kit.Pegar(pneu));
        Assert.IsFalse(kit.PegarTampa(caixa), "com o pneu, a tampa não entra nos braços");
        Assert.IsNull(kit.TampaCarregada);
        Assert.AreEqual(ResultadoDoPasso.MaosOcupadas, caixa.AvancarCom(kit));
        Assert.AreSame(caixa.transform, tampa.parent);
    }

    [Test]
    public void NaoTrocaDeFerramentaComATampa()
    {
        caixa.AvancarCom(kit);

        Assert.IsFalse(kit.Selecionar(Ferramenta.Areia));
        Assert.AreEqual(Ferramenta.Maos, kit.Selecionada);
    }

    [Test]
    public void FerramentaErradaNaoPega()
    {
        kit.Selecionar(Ferramenta.Areia);

        Assert.AreEqual(ResultadoDoPasso.FerramentaErrada, caixa.AvancarCom(kit));
        Assert.IsNull(kit.TampaCarregada);
        Assert.AreSame(caixa.transform, tampa.parent);
    }

    [Test]
    public void ChuvaNaoReencheComATampaNosBracos()
    {
        caixa.NascerVazio();
        Assert.IsTrue(caixa.PodeReencher, "nascida seca e aberta, a chuva reencheria");

        caixa.AvancarCom(kit);

        Assert.IsFalse(caixa.PodeReencher);
    }

    [Test]
    public void BloqueioMantemATampaNosBracos()
    {
        Interactor interactor = jogador.gameObject.AddComponent<Interactor>();
        caixa.AvancarCom(kit);

        interactor.Bloqueado = true;
        Assert.DoesNotThrow(() => interactor.AoBloquear());

        Assert.AreSame(caixa, kit.TampaCarregada);
        Assert.AreSame(ponto, tampa.parent, "a tampa continua nos braços");
    }

    [Test]
    public void TampaColocadaReligaOsColliders()
    {
        BoxCollider desligado = tampa.gameObject.AddComponent<BoxCollider>();
        desligado.enabled = false;

        caixa.AvancarCom(kit);
        Assert.IsFalse(colliderDaTampa.enabled);

        caixa.AvancarCom(kit);
        Assert.IsTrue(colliderDaTampa.enabled, "o collider que estava ligado volta");
        Assert.IsFalse(desligado.enabled, "o que já estava desligado continua desligado");
    }

    [Test]
    public void TampaDevolvidaReligaOsColliders()
    {
        BoxCollider desligado = tampa.gameObject.AddComponent<BoxCollider>();
        desligado.enabled = false;

        caixa.AvancarCom(kit);
        Assert.IsFalse(colliderDaTampa.enabled);

        kit.DevolverTampa();
        Assert.IsTrue(colliderDaTampa.enabled, "o collider que estava ligado volta");
        Assert.IsFalse(desligado.enabled, "o que já estava desligado continua desligado");
    }

    [Test]
    public void CaminhoAntigoSemKitContinuaCliqueAClique()
    {
        for (int i = 1; i <= 5; i++)
        {
            caixa.Interact();
            Assert.AreSame(caixa.transform, tampa.parent, $"clique {i}: a tampa não sai da caixa");
            Assert.AreEqual(i / 5f, caixa.Fechamento, 1e-5f, $"clique {i}");
            Assert.AreEqual(i == 5, caixa.IsComplete, $"clique {i}");
        }

        AssertPerto(PosFechada, tampa.localPosition, "tampa assentada na boca");
        Assert.IsNull(kit.TampaCarregada);
    }
}
