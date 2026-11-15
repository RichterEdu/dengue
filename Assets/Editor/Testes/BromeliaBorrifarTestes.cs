using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

/* ============================================================
 *  BromeliaBorrifarTestes.cs
 *  ---------------------------------------------------------------------
 *  Testes EditMode da bromélia no minigame dos pontos de borrifar
 *  (SessaoDaBromelia, derivada de PontosDeBorrifar): a sessão tem as
 *  pocinhas com água, embaralhadas pela semente e com o copo por último,
 *  no centro do campo; os pontos são a projeção das pocinhas na pose da
 *  vista de perto (acima do copo, com a base da tela do lado do agente);
 *  a proporção da tela chega à sessão; o acerto seca a pocinha do ponto;
 *  o último acerto só conclui depois do fecho; sair guarda as secas;
 *  retomar cobre só o que falta; sair ou bloquear no fecho devolve a água
 *  do copo; desativada no meio, larga a sessão; o enxame some ao começar
 *  e volta com a câmera; a dose de água sanitária só sai no fim; o
 *  caminho antigo (Interact sem kit) mantém a ordem fixa; textos da HUD,
 *  tempo estimado e ganchos fora de ordem. No prefab de verdade: as
 *  pocinhas de fora mais altas e maiores (à vista de cima), as bolinhas
 *  sem se sobrepor de qualquer lado, e o AjustarPocinhas idempotente.
 *
 *  Câmera: o CriarMinigame pede o Camera.main (a proporção da tela). A
 *  cena aberta no modo batch já tem a sua "Main Camera"; sem nenhuma, o
 *  teste cria uma com a tag MainCamera pelo TesteUtil. A câmera da vista
 *  de perto é uma CameraJogador de teste ligada ao Interactor
 *  (CameraDePerto) e ao jogador; "a câmera volta" = Posicionar(Q, 1) e
 *  AtualizarVolta.
 *
 *  A bromélia de teste fica em Origem, com 3 pocinhas (a última é o copo)
 *  e 3 cliques: cada pocinha seca é um passo. O jogador fica 1,5 m atrás
 *  dela (−z). Fora do Play a pocinha some na hora (as corrotinas não
 *  andam), e o OnDisable não roda: os testes chamam o AoDesativar.
 *
 *  Rodar:  unity command run_tests --mode EditMode --filter_type category --filter Minigames
 * ============================================================ */

[Category("Minigames")]
public class BromeliaBorrifarTestes
{
    private const float Q = 1f / 60f;
    private static readonly Vector3 Origem = new Vector3(20f, 0f, 20f);
    private static readonly Vector3 EscalaDaPoca = new Vector3(0.2f, 0.05f, 0.2f);

    /* Deslocamentos a partir da bromélia; a última é o copo. */
    private static readonly Vector3[] TresPocinhas =
        { new Vector3(-0.06f, 0.3f, 0.05f), new Vector3(0.06f, 0.3f, 0.05f), new Vector3(0f, 0.3f, 0f) };
    private static readonly Vector3[] CincoPocinhas =
        { new Vector3(-0.08f, 0.3f, 0.03f), new Vector3(0.08f, 0.3f, 0.03f), new Vector3(-0.05f, 0.3f, -0.07f),
          new Vector3(0.05f, 0.3f, -0.07f), new Vector3(0f, 0.3f, 0f) };

    private ControladorJogador jogador;
    private KitDoAgente kit;
    private Interactor interactor;
    private Camera cameraDoTeste;    // null = a cena já tinha a sua
    private CameraJogador cameraDePerto;
    private BromeliaInteractable bromelia;
    private GameObject[] pocas;

    [SetUp]
    public void Preparar()
    {
        jogador = TesteUtil.Com<ControladorJogador>("Jogador (teste)");
        jogador.transform.position = Origem + new Vector3(0f, 0f, -1.5f);
        kit = jogador.gameObject.AddComponent<KitDoAgente>();
        kit.Selecionar(Ferramenta.AguaSanitaria);
        interactor = jogador.gameObject.AddComponent<Interactor>();

        cameraDoTeste = null;
        if (Camera.main == null)
        {
            cameraDoTeste = TesteUtil.Com<Camera>("Câmera (teste)");
            cameraDoTeste.tag = "MainCamera";
            cameraDoTeste.transform.position = new Vector3(0f, 1.5f, 0f);
        }

        cameraDePerto = TesteUtil.Com<CameraJogador>("Câmera de perto (teste)");
        TesteUtil.Setar(cameraDePerto, "jogador", jogador);
        interactor.CameraDePerto = cameraDePerto;

        Montar(Origem, TresPocinhas, 3);
    }

    [TearDown]
    public void Limpar()
    {
        TesteUtil.LimparTudo();
    }

    /* Bromélia nova (na origem dada) com uma pocinha em cada deslocamento. */
    private void Montar(Vector3 origem, Vector3[] deslocamentos, int cliques)
    {
        bromelia = TesteUtil.Com<BromeliaInteractable>("Bromélia (teste)");
        TesteUtil.Setar(bromelia, "ferramentaNecessaria", Ferramenta.AguaSanitaria);
        TesteUtil.Setar(bromelia, "requiredClicks", cliques);
        TesteUtil.Setar(bromelia, "punchScale", 0f);
        TesteUtil.Setar(bromelia, "shrinkDuration", 0f);

        bromelia.transform.position = origem;
        pocas = new GameObject[deslocamentos.Length];
        for (int i = 0; i < deslocamentos.Length; i++)
        {
            pocas[i] = TesteUtil.Novo("Pocinha " + i + " (teste)");
            pocas[i].transform.SetParent(bromelia.transform, false);
            pocas[i].transform.position = origem + deslocamentos[i];
            pocas[i].transform.localScale = EscalaDaPoca;
        }
        DefinirPocas(pocas);
    }

    private void DefinirPocas(GameObject[] lista)
    {
        typeof(BromeliaInteractable).GetField("pocas", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(bromelia, lista);
    }

    /* O enxame de mosquitos da bromélia (filho com o nome do EnxameDeMosquitos). */
    private GameObject CriarEnxame()
    {
        GameObject enxame = TesteUtil.Novo(EnxameDeMosquitos.NomeDoFilho);
        enxame.transform.SetParent(bromelia.transform, false);
        return enxame;
    }

    private Vector3 Agente => kit.transform.position;
    private Vector3 Copo => pocas[pocas.Length - 1].transform.position;

    private SessaoDaBromelia Sessao(int semente = 0)
        => bromelia.CriarSessao(Agente, Vector3.forward, 16f / 9f, semente);

    /* Começa pelo Interactor e passa da espera inicial. */
    private SessaoDaBromelia Iniciar()
    {
        Assert.IsTrue(interactor.IniciarMinigame(bromelia), "a bromélia devia ter minigame");
        Esperar(0.35f);
        return (SessaoDaBromelia)interactor.MinigameAtivo;
    }

    private void Esperar(float segundos)
    {
        for (float t = 0f; t < segundos && interactor.MinigameAtivo != null; t += Q)
            interactor.AvancarMinigame(Q, new EntradaDoMinigame(), false);
    }

    /* Leva a mira à bolinha da vez e aperta, num quadro só. */
    private void Acertar(SessaoDaBromelia s)
    {
        Vector2 ponteiro = (s.Ponto(s.DaVez) - s.Mira) / PontosDeBorrifar.MiraPorGrauPadrao;
        interactor.AvancarMinigame(Q, new EntradaDoMinigame { Ponteiro = ponteiro, Apertou = true }, false);
    }

    private void Sair() => interactor.AvancarMinigame(Q, new EntradaDoMinigame(), true);

    private void CameraVolta()
    {
        cameraDePerto.Posicionar(Q, 1f);
        interactor.AtualizarVolta();
    }

    private static void AssertPerto(Vector2 esperado, Vector2 obtido, float tolerancia, string mensagem)
    {
        Assert.AreEqual(esperado.x, obtido.x, tolerancia, mensagem + " (x)");
        Assert.AreEqual(esperado.y, obtido.y, tolerancia, mensagem + " (y)");
    }

    private static void AssertPerto(Vector3 esperado, Vector3 obtido, float tolerancia, string mensagem)
    {
        Assert.AreEqual(0f, Vector3.Distance(esperado, obtido), tolerancia, mensagem);
    }

    /* ---------------- A sessão ---------------- */

    [Test]
    public void CriaASessaoComAsPocinhasComAgua()
    {
        IMinigame criado = bromelia.CriarMinigame(kit, 1);
        Assert.IsInstanceOf<SessaoDaBromelia>(criado);
        Assert.AreEqual(3, ((SessaoDaBromelia)criado).Total);

        pocas[0].SetActive(false);
        Assert.AreEqual(2, ((SessaoDaBromelia)bromelia.CriarMinigame(kit, 1)).Total, "só as com água");
    }

    [Test]
    public void CopoPorUltimoENoCentroDoCampo()
    {
        for (int semente = 0; semente < 10; semente++)
        {
            SessaoDaBromelia s = Sessao(semente);
            Assert.AreEqual(2, s.Pocas[s.Total - 1], "semente " + semente + ": o copo por último");
            AssertPerto(Vector2.zero, s.Ponto(s.Total - 1), 1e-4f, "semente " + semente + ": o copo no centro");
        }
    }

    [Test]
    public void MesmaSementeMesmaOrdem()
    {
        Montar(Origem, CincoPocinhas, 5);

        CollectionAssert.AreEqual(Sessao(7).Pocas, Sessao(7).Pocas, "mesma semente, mesma ordem");

        HashSet<string> ordens = new HashSet<string>();
        for (int semente = 0; semente < 20; semente++)
            ordens.Add(string.Join(",", Sessao(semente).Pocas));
        Assert.GreaterOrEqual(ordens.Count, 2, "as sementes embaralham");
    }

    [Test]
    public void PontosSaoAProjecaoDasPocinhas()
    {
        Montar(Origem, CincoPocinhas, 5);
        SessaoDaBromelia s = Sessao(3);

        Assert.AreEqual(5, s.Total);
        for (int k = 0; k < s.Total; k++)
            AssertPerto(s.Pose.NoCampo(pocas[s.Pocas[k]].transform.position), s.Ponto(k), 1e-5f, "ponto " + k);
    }

    [Test]
    public void PoseAcimaDoCopoComABaseDaTelaDoLadoDoAgente()
    {
        PoseDePerto pose = bromelia.PoseDaVistaDePerto(Origem + new Vector3(0f, 0f, -1.5f), Vector3.forward);

        AssertPerto(Copo + new Vector3(0f, 0.58f, -0.17f), pose.Posicao, 1e-4f, "acima do copo, recuada para o agente");
        Assert.AreEqual(40f, pose.Fov, 1e-5f);
        AssertPerto(Vector2.zero, pose.NoCampo(Copo), 1e-4f, "o copo no centro");
        Assert.Less(pose.NoCampo(Copo + new Vector3(0f, 0f, -0.05f)).y, 0f, "o lado do agente fica na base da tela");
        Assert.Greater(pose.NoCampo(Copo + new Vector3(0.05f, 0f, 0f)).x, 0f, "+x do mundo à direita");
    }

    [Test]
    public void AgenteEmCimaDaPlantaUsaAFrenteReserva()
    {
        Vector3 agente = new Vector3(Copo.x, 0f, Copo.z);
        PoseDePerto pose = bromelia.PoseDaVistaDePerto(agente, Vector3.right);

        AssertPerto(Copo + new Vector3(-0.17f, 0.58f, 0f), pose.Posicao, 1e-4f, "recuada contra a frente reserva");
        Quaternion g = pose.Giro;
        Assert.IsTrue(float.IsFinite(g.x) && float.IsFinite(g.y) && float.IsFinite(g.z) && float.IsFinite(g.w),
                      "giro finito");
    }

    [Test]
    public void ProporcaoDaTelaChegaASessao()
    {
        SessaoDaBromelia s = bromelia.CriarSessao(Agente, Vector3.forward, 4f / 3f, 0);

        s.Atualizar(Q, new EntradaDoMinigame { Ponteiro = new Vector2(1e6f, 0f) });

        Assert.AreEqual(4f / 3f, s.Mira.x, 1e-5f, "a mira para na borda do campo");
    }

    /* ---------------- Pelo Interactor ---------------- */

    [Test]
    public void AcertoSecaAPocinhaCerta()
    {
        SessaoDaBromelia s = Iniciar();
        int i = bromelia.PocaDoPonto(0);

        Acertar(s);

        Assert.IsFalse(pocas[i].activeSelf, "a pocinha do ponto secou");
        for (int j = 0; j < pocas.Length; j++)
            if (j != i) Assert.IsTrue(pocas[j].activeSelf, "a pocinha " + j + " continua com água");
        Assert.AreEqual(1, bromelia.CurrentClicks);
        Assert.IsNotNull(interactor.MinigameAtivo);
    }

    [Test]
    public void CliqueForaNaoSecaNada()
    {
        SessaoDaBromelia s = Iniciar();

        Vector2 ponteiro = (new Vector2(1.2f, 0.9f) - s.Mira) / PontosDeBorrifar.MiraPorGrauPadrao;
        interactor.AvancarMinigame(Q, new EntradaDoMinigame { Ponteiro = ponteiro, Apertou = true }, false);

        foreach (GameObject p in pocas)
            Assert.IsTrue(p.activeSelf);
        Assert.AreEqual(0, bromelia.CurrentClicks);
    }

    [Test]
    public void UltimoAcertoSoConcluiDepoisDoFecho()
    {
        SessaoDaBromelia s = Iniciar();
        for (int k = 0; k < 3; k++) Acertar(s);

        foreach (GameObject p in pocas)
            Assert.IsFalse(p.activeSelf, "as três secas");
        Assert.IsFalse(bromelia.IsComplete, "ainda no fecho");
        Assert.IsNotNull(interactor.MinigameAtivo);

        Esperar(0.5f);
        Assert.IsTrue(bromelia.IsComplete);
        Assert.IsNull(interactor.MinigameAtivo);
    }

    [Test]
    public void UltimoPassoSecaOQueSobrou()
    {
        bromelia.AoIniciarMinigame(Sessao());

        Assert.AreEqual(ResultadoDoPasso.Avancou, bromelia.AvancarCom(kit));
        Assert.AreEqual(ResultadoDoPasso.Avancou, bromelia.AvancarCom(kit));
        foreach (GameObject p in pocas)
            Assert.IsTrue(p.activeSelf, "com sessão, o passo não seca na ordem fixa");

        Assert.AreEqual(ResultadoDoPasso.Concluiu, bromelia.AvancarCom(kit));
        foreach (GameObject p in pocas)
            Assert.IsFalse(p.activeSelf, "o último passo seca o que sobrou");
    }

    [Test]
    public void SairGuardaAsSecas()
    {
        SessaoDaBromelia s = Iniciar();
        int i = bromelia.PocaDoPonto(0);
        Acertar(s);

        Sair();

        Assert.IsNull(interactor.MinigameAtivo);
        Assert.IsFalse(pocas[i].activeSelf, "a seca continua seca");
        AssertPerto(EscalaDaPoca, pocas[i].transform.localScale, 1e-6f, "a seca guarda a escala original (para a chuva)");
        for (int j = 0; j < pocas.Length; j++)
            if (j != i) Assert.IsTrue(pocas[j].activeSelf, "a pocinha " + j + " continua com água");
        Assert.AreEqual(1, bromelia.CurrentClicks);
    }

    [Test]
    public void RetomarUsaSoOQueFalta()
    {
        Montar(Origem, CincoPocinhas, 5);
        SessaoDaBromelia s = Iniciar();
        Acertar(s);
        Acertar(s);
        Assert.AreEqual(2, bromelia.CurrentClicks);
        Sair();
        Assert.IsNull(interactor.MinigameAtivo);

        SessaoDaBromelia retomada = Iniciar();
        Assert.AreEqual(3, retomada.Total, "só as que faltam");
        Assert.AreEqual(4, retomada.Pocas[retomada.Total - 1], "o copo por último");

        for (int k = 0; k < 3; k++) Acertar(retomada);
        Esperar(0.5f);

        Assert.IsTrue(bromelia.IsComplete);
        Assert.AreEqual(5, bromelia.CurrentClicks);
    }

    /* Três acertos (o último entra no fecho) e então a interrupção. */
    private void InterromperNoFecho(System.Action interromper)
    {
        SessaoDaBromelia s = Iniciar();
        for (int k = 0; k < 3; k++) Acertar(s);
        Assert.AreEqual(2, bromelia.CurrentClicks, "o terceiro passo espera o fecho");

        interromper();

        Assert.IsFalse(bromelia.IsComplete);
        Assert.AreEqual(2, bromelia.CurrentClicks);
        Assert.IsTrue(pocas[2].activeSelf, "o copo volta a ter água");
        AssertPerto(EscalaDaPoca, pocas[2].transform.localScale, 1e-6f, "com a escala original");
        Assert.IsFalse(pocas[0].activeSelf);
        Assert.IsFalse(pocas[1].activeSelf);
        Assert.AreEqual(1, ((SessaoDaBromelia)bromelia.CriarMinigame(kit, 0)).Total, "a próxima sessão é só o copo");
    }

    [Test]
    public void SairNoFechoDevolveAAguaDoCopo()
    {
        InterromperNoFecho(Sair);
    }

    [Test]
    public void BloqueioNoFechoDevolveAAguaDoCopo()
    {
        InterromperNoFecho(() => interactor.AoBloquear());
    }

    [Test]
    public void DesativadaNoMeioLargaASessao()
    {
        GameObject enxame = CriarEnxame();
        Iniciar();
        Assert.IsFalse(enxame.activeSelf);

        bromelia.AoDesativar();

        Assert.IsTrue(enxame.activeSelf, "o enxame volta na hora");
        Assert.AreEqual(-1, bromelia.PocaDoPonto(0), "sem sessão");

        bool[] antes = new bool[pocas.Length];
        for (int i = 0; i < pocas.Length; i++) antes[i] = pocas[i].activeSelf;
        bromelia.AoDesativar();
        for (int i = 0; i < pocas.Length; i++)
            Assert.AreEqual(antes[i], pocas[i].activeSelf, "sem sessão, nenhuma pocinha muda");
    }

    /* Com o enxame: some ao começar, continua escondido enquanto a câmera
     * volta e reaparece quando ela chega aos olhos. */
    private void EnxameVoltaComACamera(string caso, System.Action<SessaoDaBromelia> encerrar)
    {
        Montar(Origem, TresPocinhas, 3);
        GameObject enxame = CriarEnxame();

        SessaoDaBromelia s = Iniciar();
        Assert.IsFalse(enxame.activeSelf, caso + ": escondido na vista de perto");

        encerrar(s);
        Assert.IsNull(interactor.MinigameAtivo, caso + ": a sessão acabou");
        Assert.IsTrue(interactor.Voltando, caso + ": a câmera está voltando");
        Assert.IsFalse(enxame.activeSelf, caso + ": ainda escondido na volta");

        CameraVolta();
        Assert.IsFalse(interactor.Voltando, caso + ": a câmera chegou");
        Assert.IsTrue(enxame.activeSelf, caso + ": o enxame volta com a câmera");
    }

    [Test]
    public void EnxameSomeAoComecarEVoltaComACamera()
    {
        EnxameVoltaComACamera("sair", s => Sair());
        EnxameVoltaComACamera("bloqueio", s => interactor.AoBloquear());
        EnxameVoltaComACamera("concluir", s =>
        {
            for (int k = 0; k < 3; k++) Acertar(s);
            Esperar(0.5f);
            Assert.IsTrue(bromelia.IsComplete);
        });
        EnxameVoltaComACamera("recusa no meio", s =>
        {
            kit.Selecionar(Ferramenta.Areia);
            Acertar(s);
        });
    }

    [Test]
    public void EnxameQueNasceNoMeioTambemSome()
    {
        Iniciar();
        GameObject enxame = CriarEnxame();

        Esperar(Q);

        Assert.IsFalse(enxame.activeSelf);
    }

    [Test]
    public void DoseSoNoFim()
    {
        int antes = kit.Estoque.Quantidade(Ferramenta.AguaSanitaria);
        SessaoDaBromelia s = Iniciar();

        Acertar(s);
        Assert.AreEqual(antes, kit.Estoque.Quantidade(Ferramenta.AguaSanitaria), "1º acerto");
        Acertar(s);
        Assert.AreEqual(antes, kit.Estoque.Quantidade(Ferramenta.AguaSanitaria), "2º acerto");
        Acertar(s);
        Assert.AreEqual(antes, kit.Estoque.Quantidade(Ferramenta.AguaSanitaria), "3º acerto, no fecho");

        Esperar(0.5f);
        Assert.IsTrue(bromelia.IsComplete);
        Assert.AreEqual(antes - 1, kit.Estoque.Quantidade(Ferramenta.AguaSanitaria), "a dose sai no fim");
    }

    /* ---------------- Textos, tempo e casos sem minigame ---------------- */

    [Test]
    public void ConviteEDica()
    {
        Assert.AreEqual("Clique para borrifar as pocinhas", bromelia.ConviteDoMinigame(kit));
        Assert.AreEqual("Clique nas bolinhas para borrifar   |   Botão direito: sair", bromelia.DicaDoMinigame);

        kit.Selecionar(Ferramenta.Areia);
        Assert.IsNull(bromelia.ConviteDoMinigame(kit), "outra ferramenta");
        Assert.IsNull(bromelia.ConviteDoMinigame(null), "sem kit");
    }

    [Test]
    public void SegundosEstimadosPelasPocinhas()
    {
        // 0,3 (espera) + 0,45 (fecho) + 0,87 × 3
        Assert.AreEqual(3.36f, bromelia.SegundosEstimados, 1e-4f);

        Montar(Origem, CincoPocinhas, 5);
        Assert.AreEqual(5.1f, bromelia.SegundosEstimados, 1e-4f);

        DefinirPocas(new GameObject[0]);
        Assert.AreEqual(bromelia.SegundosPorPasso * bromelia.RequiredClicks, bromelia.SegundosEstimados, 1e-4f,
                        "sem pocinhas, o da base");
    }

    [Test]
    public void SemPocinhasNaoTemMinigame()
    {
        DefinirPocas(new GameObject[0]);
        Assert.DoesNotThrow(() => Assert.IsNull(bromelia.CriarMinigame(kit, 0)), "pocas vazio");
        Assert.IsNull(bromelia.ConviteDoMinigame(kit));

        DefinirPocas(new GameObject[] { null, null });
        Assert.DoesNotThrow(() => Assert.IsNull(bromelia.CriarMinigame(kit, 0)), "só nulos");
        Assert.IsNull(bromelia.ConviteDoMinigame(kit));
    }

    [Test]
    public void SemFerramentaOuSemCameraNaoTemMinigame()
    {
        kit.Selecionar(Ferramenta.Areia);
        Assert.IsNull(bromelia.CriarMinigame(kit, 0), "outra ferramenta");
        Assert.IsNull(bromelia.CriarMinigame(null, 0), "sem kit");

        // Sem câmera só dá para conferir quando ela é a do teste: a da cena
        // aberta não é desligada daqui.
        kit.Selecionar(Ferramenta.AguaSanitaria);
        if (cameraDoTeste != null)
        {
            Object.DestroyImmediate(cameraDoTeste.gameObject);
            Assert.IsNull(bromelia.CriarMinigame(kit, 0), "sem câmera");
        }
        Assert.AreEqual("Clique para borrifar as pocinhas", bromelia.ConviteDoMinigame(kit),
                        "o convite não depende da câmera");
    }

    [Test]
    public void NascidaSecaNaoTemMinigame()
    {
        bromelia.NascerVazio();
        Assert.IsNull(bromelia.CriarMinigame(kit, 0));

        bromelia.Reencher();
        Assert.AreEqual(3, ((SessaoDaBromelia)bromelia.CriarMinigame(kit, 0)).Total);
    }

    /* ---------------- Caminho antigo ---------------- */

    [Test]
    public void CaminhoAntigoMantemAOrdemFixa()
    {
        bromelia.Interact();
        Assert.IsFalse(pocas[0].activeSelf, "1º clique: a 1ª do array");
        Assert.IsTrue(pocas[1].activeSelf);
        Assert.IsTrue(pocas[2].activeSelf);

        bromelia.Interact();
        Assert.IsFalse(pocas[1].activeSelf, "2º clique: a 2ª");
        Assert.IsTrue(pocas[2].activeSelf);

        bromelia.Interact();
        Assert.IsFalse(pocas[2].activeSelf);
        Assert.IsTrue(bromelia.IsComplete);
    }

    [Test]
    public void CaminhoAntigoPulaAsJaSecas()
    {
        // Secas fora de ordem (as duas primeiras da sessão, que saiu no
        // meio): o passo sem sessão seca a próxima ATIVA do array, e a conta
        // (ceil(5·cliques/5) secas) fecha com o que está mesmo seco.
        Montar(Origem, CincoPocinhas, 5);
        SessaoDaBromelia s = Iniciar();
        Acertar(s);
        Acertar(s);
        Sair();
        Assert.AreEqual(2, bromelia.CurrentClicks);

        List<int> comAgua = new List<int>();
        for (int i = 0; i < pocas.Length; i++)
            if (pocas[i].activeSelf) comAgua.Add(i);
        Assert.AreEqual(3, comAgua.Count);
        Assert.AreEqual(4, comAgua[2], "o copo nunca sai entre os primeiros");

        Assert.AreEqual(ResultadoDoPasso.Avancou, bromelia.AvancarCom(kit));
        Assert.IsFalse(pocas[comAgua[0]].activeSelf, "a 1ª ativa do array");
        Assert.IsTrue(pocas[comAgua[1]].activeSelf, "só uma a mais: 3 de 5 secas no 3º passo");
        Assert.IsTrue(pocas[4].activeSelf);

        Assert.AreEqual(ResultadoDoPasso.Avancou, bromelia.AvancarCom(kit));
        Assert.IsFalse(pocas[comAgua[1]].activeSelf);
        Assert.IsTrue(pocas[4].activeSelf);

        Assert.AreEqual(ResultadoDoPasso.Concluiu, bromelia.AvancarCom(kit));
        Assert.IsFalse(pocas[4].activeSelf);
    }

    [Test]
    public void GanchosForaDeOrdemNaoLancam()
    {
        SessaoDaBromelia solta = Sessao();
        Assert.DoesNotThrow(() => bromelia.AoSairDoMinigame(), "sair sem ter iniciado");
        Assert.DoesNotThrow(() => bromelia.AoAtualizarMinigame(solta), "atualizar sem ter iniciado");
        Assert.DoesNotThrow(() => bromelia.AoAtualizarMinigame(null), "sessão nula");
        Assert.DoesNotThrow(() => bromelia.AoIniciarMinigame(null), "iniciar com nula");

        bromelia.AoIniciarMinigame(solta);
        bromelia.AoSairDoMinigame();
        Assert.DoesNotThrow(() => bromelia.AoSairDoMinigame(), "sair duas vezes");
        Assert.DoesNotThrow(() => bromelia.AoAtualizarMinigame(solta), "atualizar depois de sair");
        foreach (GameObject p in pocas)
            Assert.IsTrue(p.activeSelf);

        // A sessão de outra bromélia, com um acerto, não seca nada nesta.
        BromeliaInteractable esta = bromelia;
        GameObject[] pocasDesta = pocas;
        esta.AoIniciarMinigame(esta.CriarSessao(Agente, Vector3.forward, 16f / 9f, 0));

        Montar(Origem + new Vector3(3f, 0f, 0f), TresPocinhas, 3);
        SessaoDaBromelia outra = bromelia.CriarSessao(Agente, Vector3.forward, 16f / 9f, 0);
        for (float t = 0f; t < 0.35f; t += Q) outra.Atualizar(Q, new EntradaDoMinigame());
        Vector2 ponteiro = (outra.Ponto(outra.DaVez) - outra.Mira) / PontosDeBorrifar.MiraPorGrauPadrao;
        outra.Atualizar(Q, new EntradaDoMinigame { Ponteiro = ponteiro, Apertou = true });
        Assert.AreEqual(1, outra.Acertos);

        Assert.DoesNotThrow(() => esta.AoAtualizarMinigame(outra));
        foreach (GameObject p in pocasDesta)
            Assert.IsTrue(p.activeSelf, "a sessão de outra bromélia não seca esta");
    }

    /* ---------------- O prefab de verdade ---------------- */

    private const string CaminhoDoPrefab = "Assets/Meus/Assets Gerados/Focos/Bromelia.prefab";

    private static GameObject CarregarPrefab()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CaminhoDoPrefab);
        Assert.IsNotNull(prefab, "o Bromelia.prefab devia existir");
        return prefab;
    }

    private static Transform AcharFilho(Transform t, string nome)
    {
        if (t.name == nome) return t;
        foreach (Transform filho in t)
        {
            Transform r = AcharFilho(filho, nome);
            if (r != null) return r;
        }
        return null;
    }

    [Test]
    public void PrefabReal_PocinhasAVista()
    {
        Transform raiz = CarregarPrefab().transform;

        for (int i = 1; i <= 4; i++)
        {
            Transform p = AcharFilho(raiz, "Agua_" + i);
            Assert.IsNotNull(p, "Agua_" + i);
            Assert.AreEqual(0.22f, raiz.InverseTransformPoint(p.position).y, 1e-3f, "Agua_" + i + ": altura");
            AssertPerto(Vector3.one * 1.5f, p.localScale, 1e-5f, "Agua_" + i + ": escala");
        }

        Transform copo = AcharFilho(raiz, "Agua_Centro");
        Assert.IsNotNull(copo, "Agua_Centro");
        Assert.AreEqual(0.215f, raiz.InverseTransformPoint(copo.position).y, 1e-3f, "o copo não sobe");
        AssertPerto(Vector3.one, copo.localScale, 1e-5f, "o copo não aumenta");
    }

    [Test]
    public void PrefabReal_BolinhasNaoSeSobrepoem()
    {
        GameObject instancia = Object.Instantiate(CarregarPrefab());
        instancia.hideFlags = HideFlags.HideAndDontSave;
        try
        {
            instancia.transform.position = Origem;
            instancia.transform.localScale = Vector3.one * 1.51f;
            BromeliaInteractable real = instancia.GetComponent<BromeliaInteractable>();
            Assert.IsNotNull(real, "o prefab devia ter a BromeliaInteractable");

            for (int d = 0; d < 8; d++)
            {
                Vector3 frente = Quaternion.Euler(0f, 45f * d, 0f) * Vector3.forward;
                Vector3 agente = Origem - frente * 1.5f;
                SessaoDaBromelia s = real.CriarSessao(agente, frente, 16f / 9f, 0);
                string caso = "direção " + (45 * d) + "°";

                Assert.AreEqual(5, s.Total, caso + ": cinco pocinhas");
                AssertPerto(Vector2.zero, s.Ponto(s.Total - 1), 1e-3f, caso + ": o copo no centro");
                for (int k = 0; k < s.Total - 1; k++)
                {
                    float r = s.Ponto(k).magnitude;
                    Assert.That(r, Is.InRange(0.30f, 0.42f), caso + ": ponto " + k + " a " + r + " do centro");
                }
                for (int a = 0; a < s.Total; a++)
                    for (int b = a + 1; b < s.Total; b++)
                    {
                        float dist = Vector2.Distance(s.Ponto(a), s.Ponto(b));
                        Assert.Greater(dist, 0.30f, caso + ": pontos " + a + " e " + b);
                    }
            }
        }
        finally
        {
            Object.DestroyImmediate(instancia);
        }
    }

    [Test]
    public void AjustarPocinhas_Idempotente()
    {
        Transform raiz = TesteUtil.Novo("Raiz das pocinhas (teste)").transform;
        raiz.position = Origem;
        Transform modelo = TesteUtil.Novo("Modelo (teste)").transform;
        modelo.SetParent(raiz, false);
        modelo.localPosition = new Vector3(0f, 0.01f, 0f);

        string[] nomes = { "Agua_1", "Agua_2", "Agua_3", "Agua_4", "Agua_Centro" };
        Transform[] filhos = new Transform[nomes.Length];
        for (int i = 0; i < nomes.Length; i++)
        {
            filhos[i] = TesteUtil.Novo(nomes[i]).transform;
            filhos[i].SetParent(modelo, false);
            filhos[i].localPosition = new Vector3(0.05f * (i - 2), 0.19f, 0.03f * i);
        }
        filhos[4].localPosition = new Vector3(0f, 0.205f, 0f);

        Assert.AreEqual(4, FocosBlenderBuilder.AjustarPocinhas(raiz));

        Vector3[] posicoes = new Vector3[nomes.Length];
        Vector3[] escalas = new Vector3[nomes.Length];
        for (int i = 0; i < 4; i++)
        {
            Vector3 noEspacoDaRaiz = raiz.InverseTransformPoint(filhos[i].position);
            Assert.AreEqual(FocosBlenderBuilder.AlturaDasPocinhas, noEspacoDaRaiz.y, 1e-5f, nomes[i] + ": altura");
            Assert.AreEqual(0.05f * (i - 2), noEspacoDaRaiz.x, 1e-5f, nomes[i] + ": o x fica");
            Assert.AreEqual(0.03f * i, noEspacoDaRaiz.z, 1e-5f, nomes[i] + ": o z fica");
            AssertPerto(Vector3.one * FocosBlenderBuilder.EscalaDasPocinhas, filhos[i].localScale, 1e-6f,
                        nomes[i] + ": escala");
        }
        for (int i = 0; i < nomes.Length; i++)
        {
            posicoes[i] = filhos[i].position;
            escalas[i] = filhos[i].localScale;
        }

        Assert.AreEqual(4, FocosBlenderBuilder.AjustarPocinhas(raiz));
        for (int i = 0; i < nomes.Length; i++)
        {
            AssertPerto(posicoes[i], filhos[i].position, 1e-6f, nomes[i] + ": a segunda vez não muda a posição");
            AssertPerto(escalas[i], filhos[i].localScale, 1e-6f, nomes[i] + ": a segunda vez não muda a escala");
        }

        AssertPerto(new Vector3(0f, 0.205f, 0f), filhos[4].localPosition, 1e-6f, "o copo não muda de lugar");
        AssertPerto(Vector3.one, filhos[4].localScale, 1e-6f, "o copo não muda de escala");
    }
}
