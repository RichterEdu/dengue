using System.Reflection;
using NUnit.Framework;
using UnityEngine;

/* ============================================================
 *  BromeliaBorrifarTestes.cs
 *  ---------------------------------------------------------------------
 *  Testes EditMode da bromélia no minigame do borrifador (MiraDoBorrifador):
 *  a sessão tem as pocinhas ativas, nos ângulos certos vistos do olho; a
 *  pocinha atingida é a que seca (não a da ordem fixa); o último passo
 *  seca o que sobrou; sair guarda as secas e devolve a meio caminho à
 *  escala original; retomar cobre só o que falta; o caminho antigo
 *  (Interact sem kit) mantém a ordem fixa; a dose de água sanitária só sai
 *  no fim; textos da HUD, tempo estimado e ganchos fora de ordem.
 *
 *  Câmera: os testes que passam pelo Interactor precisam do Camera.main
 *  (o CriarMinigame lê o olho dele). A cena aberta no modo batch já tem a
 *  sua "Main Camera"; sem nenhuma, o teste cria uma com a tag MainCamera
 *  pelo TesteUtil. O olho é a posição dessa câmera, e as pocinhas ficam em
 *  volta dele (a visão vem do yaw/pitch do ControladorJogador, não da
 *  rotação da câmera). Os testes só de geometria chamam CriarMira direto.
 *
 *  A bromélia de teste tem 3 pocinhas (a última é o copo central) 1,5 m à
 *  frente do olho e 0,3 m abaixo dele, com 3 cliques: cada pocinha seca é
 *  um passo. O jogador começa olhando reto (yaw e pitch 0).
 *
 *  Rodar:  unity test "<projeto>" --mode EditMode --filter BromeliaBorrifarTestes
 * ============================================================ */

[Category("Minigames")]
public class BromeliaBorrifarTestes
{
    private const float Quadro = 1f / 60f;
    private static readonly Vector3 EscalaDaPoca = new Vector3(0.2f, 0.05f, 0.2f);

    /* Deslocamentos a partir do olho. */
    private static readonly Vector3[] TresPocinhas =
    {
        new Vector3(-0.4f, -0.3f, 1.5f),
        new Vector3(0.4f, -0.3f, 1.5f),
        new Vector3(0f, -0.3f, 1.5f)       // copo central
    };

    private ControladorJogador jogador;
    private KitDoAgente kit;
    private Interactor interactor;
    private Camera cameraDoTeste;    // null = a cena já tinha a sua
    private Vector3 olho;
    private BromeliaInteractable bromelia;
    private GameObject[] pocas;

    // O olhar do começo da sessão: a Mira conta a partir dele.
    private Quaternion visaoDoComeco;

    [SetUp]
    public void Preparar()
    {
        jogador = TesteUtil.Com<ControladorJogador>("Jogador (teste)");
        kit = jogador.gameObject.AddComponent<KitDoAgente>();
        kit.Selecionar(Ferramenta.AguaSanitaria);
        interactor = jogador.gameObject.AddComponent<Interactor>();

        Camera principal = Camera.main;
        cameraDoTeste = null;
        if (principal == null)
        {
            cameraDoTeste = TesteUtil.Com<Camera>("Câmera (teste)");
            cameraDoTeste.tag = "MainCamera";
            cameraDoTeste.transform.position = new Vector3(0f, 1.5f, 0f);
            principal = cameraDoTeste;
        }
        olho = principal.transform.position;

        Montar(olho, TresPocinhas, 3);
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

    /* (yaw, pitch) em graus de uma posição vista do olho com esta visão. */
    private static Vector2 Angulos(Vector3 posicao, Vector3 olho, Quaternion visao)
    {
        Vector3 local = Quaternion.Inverse(visao) * (posicao - olho);
        float yaw = Mathf.Atan2(local.x, local.z) * Mathf.Rad2Deg;
        float pitch = Mathf.Atan2(-local.y, Mathf.Sqrt(local.x * local.x + local.z * local.z)) * Mathf.Rad2Deg;
        return new Vector2(yaw, pitch);
    }

    private static void Frame(MiraDoBorrifador m, Vector2 olhar, bool segurando, float dt = Quadro)
    {
        m.Atualizar(dt, new EntradaDoMinigame { Olhar = olhar, Segurando = segurando });
    }

    /* Começa a sessão pelo Interactor, guardando o olhar do começo. */
    private MiraDoBorrifador Iniciar()
    {
        visaoDoComeco = Quaternion.Euler(jogador.Pitch, jogador.Yaw, 0f);
        Assert.IsTrue(interactor.IniciarMinigame(bromelia), "a bromélia devia ter minigame");
        return (MiraDoBorrifador)interactor.MinigameAtivo;
    }

    /* Um quadro do Interactor que leva a mira até a pocinha, sem segurar. */
    private void Mirar(MiraDoBorrifador m, GameObject poca)
    {
        Vector2 alvo = Angulos(poca.transform.position, olho, visaoDoComeco);
        Vector2 olhar = new Vector2(alvo.x - m.Mira.x, -(alvo.y - m.Mira.y));
        interactor.AvancarMinigame(Quadro, new EntradaDoMinigame { Olhar = olhar }, false);
    }

    /* Segura parado até a pocinha sumir (ou a sessão acabar). */
    private void SegurarAteSecar(GameObject poca)
    {
        for (int i = 0; i < 300 && poca.activeSelf && interactor.MinigameAtivo != null; i++)
            Segurar(1);
        Assert.IsFalse(poca.activeSelf, poca.name + " devia ter secado");
    }

    private void Segurar(int quadros, bool sair = false)
    {
        for (int i = 0; i < quadros; i++)
            interactor.AvancarMinigame(Quadro, new EntradaDoMinigame { Segurando = true }, sair && i == quadros - 1);
    }

    private void Secar(MiraDoBorrifador m, GameObject poca)
    {
        Mirar(m, poca);
        SegurarAteSecar(poca);
    }

    [Test]
    public void CriaAMiraComAsPocinhasAtivas()
    {
        Assert.AreEqual(3, bromelia.CriarMira(olho, Quaternion.identity).Total);
        Assert.IsInstanceOf<MiraDoBorrifador>(bromelia.CriarMinigame(kit, 0));

        pocas[0].SetActive(false);
        Assert.AreEqual(2, bromelia.CriarMira(olho, Quaternion.identity).Total, "só as ativas");
    }

    [Test]
    public void AngulosDaPocinha()
    {
        Montar(Vector3.zero, new[] { new Vector3(0.1f, -0.2f, 1.5f) }, 1);

        MiraDoBorrifador certa = bromelia.CriarMira(Vector3.zero, Quaternion.identity);
        Frame(certa, new Vector2(3.814f, -7.578f), false);
        for (int i = 0; i < 90; i++) Frame(certa, Vector2.zero, true);
        Assert.AreEqual(1, certa.Secas, "mirada na pocinha, seca");
        Assert.AreEqual(0f, certa.Umidade(0));

        MiraDoBorrifador longe = bromelia.CriarMira(Vector3.zero, Quaternion.identity);
        Frame(longe, new Vector2(3.814f + 6f, -7.578f), false);
        for (int i = 0; i < 90; i++) Frame(longe, Vector2.zero, true);
        Assert.AreEqual(0, longe.Secas, "6° fora, não seca");
        Assert.AreEqual(1f, longe.Umidade(0));
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
        Assert.AreEqual(3, bromelia.CriarMira(olho, Quaternion.identity).Total);
    }

    [Test]
    public void PocinhaAtingidaEAQueSeca()
    {
        MiraDoBorrifador m = Iniciar();

        Mirar(m, pocas[1]);
        for (int i = 0; i < 300 && bromelia.CurrentClicks < 1; i++)
            Segurar(1);

        Assert.AreEqual(1, bromelia.CurrentClicks, "um passo fechado");
        Assert.IsTrue(pocas[0].activeSelf, "a 1ª da ordem fixa continua com água");
        Assert.IsFalse(pocas[1].activeSelf, "a atingida secou");
        Assert.IsTrue(pocas[2].activeSelf);
        Assert.IsNotNull(interactor.MinigameAtivo);
    }

    [Test]
    public void AvancandoSoSegurando()
    {
        MiraDoBorrifador m = Iniciar();

        Mirar(m, pocas[0]);
        Assert.IsFalse(interactor.Avancando, "sem segurar, sem névoa");
        Segurar(1);
        Assert.IsTrue(interactor.Avancando, "segurando, a névoa sai");
    }

    [Test]
    public void VisualSegueAUmidade()
    {
        MiraDoBorrifador m = Iniciar();

        Mirar(m, pocas[1]);
        Segurar(15);

        float u = m.Umidade(1);
        Assert.That(u, Is.InRange(0.1f, 0.9f), "a meio caminho");
        Vector3 escala = pocas[1].transform.localScale;
        Assert.AreEqual(EscalaDaPoca.x * u, escala.x, 1e-4f);
        Assert.AreEqual(EscalaDaPoca.y, escala.y, 1e-6f, "a altura fica");
        Assert.AreEqual(EscalaDaPoca.z * u, escala.z, 1e-4f);
    }

    [Test]
    public void UltimoPassoSecaOQueSobrou()
    {
        bromelia.AoIniciarMinigame(bromelia.CriarMira(olho, Quaternion.identity));

        Assert.AreEqual(ResultadoDoPasso.Avancou, bromelia.AvancarCom(kit));
        Assert.AreEqual(ResultadoDoPasso.Avancou, bromelia.AvancarCom(kit));
        foreach (GameObject p in pocas)
            Assert.IsTrue(p.activeSelf, "com sessão, o passo não seca na ordem fixa");

        Assert.AreEqual(ResultadoDoPasso.Concluiu, bromelia.AvancarCom(kit));
        foreach (GameObject p in pocas)
            Assert.IsFalse(p.activeSelf, "o último passo seca o que sobrou");
        Assert.IsTrue(bromelia.IsComplete);
    }

    [Test]
    public void SairGuardaAsSecasEDevolveAMeia()
    {
        MiraDoBorrifador m = Iniciar();
        Secar(m, pocas[0]);

        Mirar(m, pocas[1]);
        Segurar(15);
        Assert.Less(pocas[1].transform.localScale.x, EscalaDaPoca.x, "a 2ª encolheu");
        Segurar(1, sair: true);

        Assert.IsNull(interactor.MinigameAtivo);
        Assert.IsFalse(pocas[0].activeSelf, "a seca continua seca");
        Assert.IsTrue(pocas[1].activeSelf, "a meio caminho continua com água");
        Assert.AreEqual(0f, Vector3.Distance(EscalaDaPoca, pocas[1].transform.localScale), 1e-6f, "escala original");
        Assert.AreEqual(0f, Vector3.Distance(EscalaDaPoca, pocas[0].transform.localScale), 1e-6f,
                        "a seca guarda a escala original (para a chuva)");
        Assert.AreEqual(1, bromelia.CurrentClicks);
    }

    [Test]
    public void RetomarUsaSoOQueFalta()
    {
        Montar(olho, new[]
        {
            new Vector3(-0.5f, -0.3f, 1.5f), new Vector3(-0.25f, -0.3f, 1.5f), new Vector3(0.25f, -0.3f, 1.5f),
            new Vector3(0.5f, -0.3f, 1.5f), new Vector3(0f, -0.3f, 1.5f)
        }, 5);

        MiraDoBorrifador m = Iniciar();
        Secar(m, pocas[3]);
        Secar(m, pocas[1]);
        Assert.AreEqual(2, bromelia.CurrentClicks);
        Segurar(1, sair: true);
        Assert.IsNull(interactor.MinigameAtivo);

        MiraDoBorrifador retomada = Iniciar();
        Assert.AreEqual(3, retomada.Total, "só as que faltam");

        int passos = 0;
        foreach (int i in new[] { 0, 2, 4 })
        {
            int antes = bromelia.CurrentClicks;
            Secar(retomada, pocas[i]);
            passos += bromelia.CurrentClicks - antes;
            if (i != 4)
                Assert.IsFalse(bromelia.IsComplete, "ainda não");
        }
        Assert.AreEqual(3, passos, "mais três passos");
        Assert.IsTrue(bromelia.IsComplete);
        Assert.IsNull(interactor.MinigameAtivo);
    }

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
        // Secas fora de ordem (uma sessão que saiu no meio, com as de índice
        // 1 e 3 secas): o passo sem sessão seca a próxima ATIVA do array, e a
        // conta (ceil(5·cliques/5) secas) fecha com o que está mesmo seco.
        Montar(olho, new[]
        {
            new Vector3(-0.5f, -0.3f, 1.5f), new Vector3(-0.25f, -0.3f, 1.5f), new Vector3(0.25f, -0.3f, 1.5f),
            new Vector3(0.5f, -0.3f, 1.5f), new Vector3(0f, -0.3f, 1.5f)
        }, 5);
        MiraDoBorrifador m = Iniciar();
        Secar(m, pocas[3]);
        Secar(m, pocas[1]);
        Segurar(1, sair: true);
        Assert.AreEqual(2, bromelia.CurrentClicks);

        Assert.AreEqual(ResultadoDoPasso.Avancou, bromelia.AvancarCom(kit));
        Assert.IsFalse(pocas[0].activeSelf, "a 1ª ativa do array");
        Assert.IsTrue(pocas[2].activeSelf, "só uma a mais: 3 de 5 secas no 3º passo");
        Assert.IsTrue(pocas[4].activeSelf);

        Assert.AreEqual(ResultadoDoPasso.Avancou, bromelia.AvancarCom(kit));
        Assert.IsFalse(pocas[2].activeSelf);
        Assert.IsTrue(pocas[4].activeSelf);

        Assert.AreEqual(ResultadoDoPasso.Concluiu, bromelia.AvancarCom(kit));
        Assert.IsFalse(pocas[4].activeSelf);
    }

    [Test]
    public void DoseSoNoFim()
    {
        int antes = kit.Estoque.Quantidade(Ferramenta.AguaSanitaria);
        MiraDoBorrifador m = Iniciar();

        Secar(m, pocas[0]);
        Assert.AreEqual(antes, kit.Estoque.Quantidade(Ferramenta.AguaSanitaria), "1º passo");
        Secar(m, pocas[1]);
        Assert.AreEqual(antes, kit.Estoque.Quantidade(Ferramenta.AguaSanitaria), "2º passo");
        Secar(m, pocas[2]);

        Assert.IsTrue(bromelia.IsComplete);
        Assert.AreEqual(antes - 1, kit.Estoque.Quantidade(Ferramenta.AguaSanitaria), "a dose sai no fim");
    }

    [Test]
    public void ConviteEDica()
    {
        Assert.AreEqual("Clique para borrifar as pocinhas", bromelia.ConviteDoMinigame(kit));
        Assert.AreEqual("Segure para borrifar as pocinhas   |   Botão direito: sair", bromelia.DicaDoMinigame);

        kit.Selecionar(Ferramenta.Areia);
        Assert.IsNull(bromelia.ConviteDoMinigame(kit), "outra ferramenta");
        Assert.IsNull(bromelia.ConviteDoMinigame(null), "sem kit");
    }

    [Test]
    public void SegundosEstimadosSomaOsTempos()
    {
        // 2 × 0,5 + 1,2 (copo central) + 0,4 × 3
        Assert.AreEqual(3.4f, bromelia.SegundosEstimados, 1e-4f);

        DefinirPocas(new GameObject[0]);
        Assert.AreEqual(bromelia.SegundosPorPasso * bromelia.RequiredClicks, bromelia.SegundosEstimados, 1e-4f,
                        "sem pocinhas, o da base");
    }

    [Test]
    public void GanchosForaDeOrdemNaoLancam()
    {
        MiraDoBorrifador solta = bromelia.CriarMira(olho, Quaternion.identity);
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
    }
}
