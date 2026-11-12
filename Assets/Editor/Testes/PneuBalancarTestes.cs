using NUnit.Framework;
using UnityEngine;

/* ============================================================
 *  PneuBalancarTestes.cs
 *  ---------------------------------------------------------------------
 *  Testes EditMode do pneu no minigame (OndaDoPneu): só as Mãos num pneu
 *  cheio criam a onda; o último passo esvazia e encerra a sessão (pneu
 *  no lugar, na pose de antes); sair e o bloqueio devolvem a pose; o
 *  pneu reenchido recomeça do zero; o convite da HUD só aparece quando um
 *  aperto novo começaria a sessão; os ganchos não lançam fora de ordem.
 *  O apoio ignora as partículas do enxame; a taxa de derrame do Inspector
 *  chega à onda; desativar o pneu no meio (OnDisable) e começar uma sessão
 *  com a anterior ainda marcada devolvem a pose.
 *
 *  A sessão é conduzida pelo Interactor (IniciarMinigame/AvancarMinigame),
 *  empurrando o pneu em fase com a velocidade da onda. O pneu de teste tem
 *  2 cliques, escala e posição da água diferentes do padrão (para provar
 *  que voltam), e fica em SemChao, longe dos colliders da cena aberta.
 *
 *  Rodar:  unity test "<projeto>" --mode EditMode --filter PneuBalancarTestes
 * ============================================================ */

[Category("Minigames")]
public class PneuBalancarTestes
{
    private static readonly Vector3 SemChao = new Vector3(0f, -10000f, 0f);
    private const float Quadro = 1f / 60f;

    private ControladorJogador jogador;
    private KitDoAgente kit;
    private Interactor interactor;
    private PneuInteractable pneu;
    private GameObject agua;

    private Quaternion rotacaoInicial;
    private Vector3 escalaDaAgua;
    private Vector3 posicaoDaAgua;

    [SetUp]
    public void Preparar()
    {
        jogador = TesteUtil.Com<ControladorJogador>("Jogador (teste)");
        kit = jogador.gameObject.AddComponent<KitDoAgente>();
        kit.PontoDeCarga = TesteUtil.Novo("Ponto de carga (teste)").transform;
        interactor = jogador.gameObject.AddComponent<Interactor>();

        Transform grupo = TesteUtil.Novo("Focos (teste)").transform;
        pneu = TesteUtil.Com<PneuInteractable>("Pneu (teste)");
        TesteUtil.Setar(pneu, "requiredClicks", 2);
        TesteUtil.Setar(pneu, "punchScale", 0f);
        TesteUtil.Setar(pneu, "shrinkDuration", 0f);
        pneu.transform.SetParent(grupo, false);
        pneu.transform.position = SemChao;
        pneu.transform.rotation = Quaternion.Euler(0f, 30f, 0f);
        rotacaoInicial = pneu.transform.rotation;

        agua = TesteUtil.Novo("agua do pneu (teste)");
        agua.transform.SetParent(pneu.transform, false);
        agua.transform.localScale = new Vector3(1f, 2f, 1f);
        agua.transform.localPosition = new Vector3(0f, 0.1f, 0f);
        escalaDaAgua = agua.transform.localScale;
        posicaoDaAgua = agua.transform.localPosition;
        TesteUtil.Setar(pneu, "water", agua);
    }

    [TearDown]
    public void Limpar()
    {
        TesteUtil.LimparTudo();
    }

    /* Um quadro empurrando em fase com a velocidade da onda. */
    private void QuadroEmFase(bool sair = false)
    {
        float u = 1f;
        if (interactor.MinigameAtivo is OndaDoPneu onda && onda.Velocidade < 0f)
            u = -1f;
        EntradaDoMinigame entrada = new EntradaDoMinigame { Olhar = new Vector2(u * 180f * Quadro, 0f) };
        interactor.AvancarMinigame(Quadro, entrada, sair);
    }

    private void QuadrosEmFase(int quadros)
    {
        for (int i = 0; i < quadros && interactor.MinigameAtivo != null; i++)
            QuadroEmFase();
    }

    private void EsvaziarPeloMinigame()
    {
        Assert.IsTrue(interactor.IniciarMinigame(pneu));
        for (int i = 0; i < 2400 && interactor.MinigameAtivo != null; i++)
            QuadroEmFase();
        Assert.IsNull(interactor.MinigameAtivo, "a sessão devia ter acabado");
    }

    private void AssertPoseOriginal(Vector3 posicao)
    {
        Assert.AreEqual(0f, Vector3.Distance(posicao, pneu.transform.position), 1e-3f, "posição");
        Assert.Less(Quaternion.Angle(rotacaoInicial, pneu.transform.rotation), 0.01f, "rotação");
    }

    [Test]
    public void MaosNoPneuCheioCriaAOnda()
    {
        Assert.IsTrue(pneu.CriarMinigame(kit, 0) is OndaDoPneu);
    }

    [Test]
    public void PneuEsvaziadoNaoTemMinigame()
    {
        pneu.AvancarCom(kit);
        pneu.AvancarCom(kit);
        Assert.IsTrue(pneu.Esvaziado);

        Assert.IsNull(pneu.CriarMinigame(kit, 0), "pegar continua instantâneo");
    }

    [Test]
    public void OutraFerramentaNaoTemMinigame()
    {
        kit.Selecionar(Ferramenta.Areia);
        Assert.IsNull(pneu.CriarMinigame(kit, 0));
    }

    [Test]
    public void ConviteSoComMaosEPneuCheio()
    {
        Assert.AreEqual("Clique para balançar o pneu", pneu.ConviteDoMinigame(kit), "Mãos, pneu cheio");

        kit.Selecionar(Ferramenta.Areia);
        Assert.IsNull(pneu.ConviteDoMinigame(kit), "outra ferramenta");

        kit.Selecionar(Ferramenta.Maos);
        pneu.AvancarCom(kit);
        pneu.AvancarCom(kit);
        Assert.IsTrue(pneu.Esvaziado);
        Assert.IsNull(pneu.ConviteDoMinigame(kit), "pneu esvaziado");
    }

    [Test]
    public void UltimoPassoEsvaziaEEncerra()
    {
        Vector3 posicao = pneu.transform.position;

        EsvaziarPeloMinigame();

        Assert.IsTrue(pneu.Esvaziado);
        Assert.IsFalse(agua.activeSelf, "água desativada");
        Assert.IsFalse(pneu.Carregado, "o pneu fica no lugar");
        AssertPoseOriginal(posicao);
        Assert.AreEqual(0f, Vector3.Distance(escalaDaAgua, agua.transform.localScale), 1e-5f, "escala da água");
        Assert.AreEqual(0f, Vector3.Distance(posicaoDaAgua, agua.transform.localPosition), 1e-5f, "posição da água");
    }

    [Test]
    public void SairDevolveAPose()
    {
        Vector3 posicao = pneu.transform.position;
        interactor.IniciarMinigame(pneu);

        QuadrosEmFase(30);
        Assert.Greater(Quaternion.Angle(rotacaoInicial, pneu.transform.rotation), 1f, "o pneu inclinou");
        QuadroEmFase(sair: true);

        Assert.IsNull(interactor.MinigameAtivo);
        AssertPoseOriginal(posicao);
    }

    [Test]
    public void BloqueioNoMeioDevolveAPose()
    {
        Vector3 posicao = pneu.transform.position;
        interactor.IniciarMinigame(pneu);
        for (int i = 0; i < 1200 && pneu.CurrentClicks < 1; i++)
            QuadroEmFase();
        Assert.AreEqual(1, pneu.CurrentClicks, "um passo fechado");
        Assert.Greater(Quaternion.Angle(rotacaoInicial, pneu.transform.rotation), 1f, "o pneu está inclinado");

        interactor.Bloqueado = true;
        interactor.AoBloquear();

        Assert.IsNull(interactor.MinigameAtivo);
        AssertPoseOriginal(posicao);
        Assert.AreEqual(1, pneu.CurrentClicks);
        Assert.IsFalse(pneu.Esvaziado);
    }

    [Test]
    public void AguaAcompanhaONivel()
    {
        interactor.IniciarMinigame(pneu);
        OndaDoPneu onda = (OndaDoPneu)interactor.MinigameAtivo;
        QuadrosEmFase(150);

        Assert.IsNotNull(interactor.MinigameAtivo, "a sessão ainda está de pé");
        Assert.Less(onda.Nivel, 1f, "já derramou");
        Assert.AreEqual(escalaDaAgua.y * onda.Nivel, agua.transform.localScale.y, 1e-4f);
    }

    [Test]
    public void RetomarContinuaDoNivelGuardado()
    {
        TesteUtil.Setar(pneu, "requiredClicks", 4);
        interactor.IniciarMinigame(pneu);
        for (int i = 0; i < 1200 && pneu.CurrentClicks < 1; i++)
            QuadroEmFase();
        Assert.AreEqual(1, pneu.CurrentClicks, "um passo fechado");
        QuadroEmFase(sair: true);
        Assert.IsNull(interactor.MinigameAtivo);

        Assert.IsTrue(interactor.IniciarMinigame(pneu));
        OndaDoPneu onda = (OndaDoPneu)interactor.MinigameAtivo;
        Assert.AreEqual(0.75f, onda.Nivel, 1e-4f);

        QuadroEmFase();
        Assert.AreEqual(escalaDaAgua.y * 0.75f, agua.transform.localScale.y, 0.05f * escalaDaAgua.y, "a água não volta a cheia");

        int passos = 0;
        for (int i = 0; i < 2400 && interactor.MinigameAtivo != null; i++)
        {
            int antes = pneu.CurrentClicks;
            QuadroEmFase();
            if (pneu.CurrentClicks > antes) passos++;
        }
        Assert.AreEqual(3, passos, "mais três passos");
        Assert.IsTrue(pneu.Esvaziado);
        Assert.AreEqual(0f, onda.Nivel, "esvaziou junto com o Nivel da sessão");
    }

    [Test]
    public void ReenchidoRecomecaDoZero()
    {
        EsvaziarPeloMinigame();
        pneu.Reencher();

        Assert.IsFalse(pneu.Esvaziado);
        Assert.IsTrue(agua.activeSelf, "a água volta");
        Assert.AreEqual(0f, Vector3.Distance(escalaDaAgua, agua.transform.localScale), 1e-5f, "a água volta cheia");
        Assert.IsTrue(pneu.CriarMinigame(kit, 0) is OndaDoPneu);

        Assert.IsTrue(interactor.IniciarMinigame(pneu));
        OndaDoPneu onda = (OndaDoPneu)interactor.MinigameAtivo;
        Assert.AreEqual(1f, onda.Nivel);
        Assert.AreEqual(0, pneu.CurrentClicks);
    }

    [Test]
    public void GanchosForaDeOrdemNaoLancam()
    {
        Assert.DoesNotThrow(() => pneu.AoSairDoMinigame(), "sair sem ter iniciado");
        Assert.DoesNotThrow(() => pneu.AoAtualizarMinigame(new OndaDoPneu(2)), "atualizar sem ter iniciado");
        Assert.DoesNotThrow(() => pneu.AoAtualizarMinigame(null), "sessão nula");

        pneu.AoIniciarMinigame(new OndaDoPneu(2));
        pneu.AoSairDoMinigame();
        Assert.DoesNotThrow(() => pneu.AoSairDoMinigame(), "sair duas vezes");
    }

    [Test]
    public void ApoioIgnoraAsParticulasDoEnxame()
    {
        // Fora do eixo da origem: a caixa vazia de um ParticleSystem ainda
        // não simulado fica em (0, 0, 0) e também mexeria no apoio.
        pneu.transform.position = SemChao + new Vector3(5f, 0f, 5f);
        GameObject borracha = TesteUtil.Primitivo(PrimitiveType.Cube, "Borracha (teste)");
        Object.DestroyImmediate(borracha.GetComponent<Collider>());
        borracha.transform.SetParent(pneu.transform, false);
        borracha.transform.localScale = new Vector3(0.2f, 0.7f, 0.7f);

        pneu.AoIniciarMinigame(new OndaDoPneu(2));
        Vector3 apoioSemEnxame = pneu.Apoio;
        float raioSemEnxame = pneu.RaioDoPneu;
        pneu.AoSairDoMinigame();

        // Como o EnxameDeMosquitos: um filho com partículas, longe da borracha.
        GameObject enxame = TesteUtil.Novo("Enxame (teste)");
        enxame.transform.SetParent(pneu.transform, false);
        enxame.transform.localPosition = new Vector3(3f, 5f, 2f);
        enxame.AddComponent<ParticleSystem>().Simulate(0.5f, true, true);   // partículas de verdade, com caixa

        pneu.AoIniciarMinigame(new OndaDoPneu(2));
        Assert.Less(Vector3.Distance(apoioSemEnxame, pneu.Apoio), 1e-4f, "o enxame não mexe no apoio");
        Assert.AreEqual(raioSemEnxame, pneu.RaioDoPneu, 1e-4f, "nem no raio do respingo");
        pneu.AoSairDoMinigame();
    }

    [Test]
    public void TaxaDeDerrameChegaASessao()
    {
        OndaDoPneu padrao = (OndaDoPneu)pneu.CriarMinigame(kit, 0);
        TesteUtil.Setar(pneu, "taxaDeDerrame", 2.8f);
        OndaDoPneu dobro = (OndaDoPneu)pneu.CriarMinigame(kit, 0);

        // A onda não depende do Nivel: com a mesma entrada, o dobro da taxa
        // derrama o dobro (esvazia na metade do tempo de derrame).
        for (int i = 0; i < 2400 && padrao.Nivel > 0.95f; i++)
        {
            float u = padrao.Velocidade < 0f ? -1f : 1f;
            EntradaDoMinigame entrada = new EntradaDoMinigame { Olhar = new Vector2(u * 180f * Quadro, 0f) };
            padrao.Atualizar(Quadro, entrada);
            dobro.Atualizar(Quadro, entrada);
        }

        Assert.LessOrEqual(padrao.Nivel, 0.95f, "o padrão derramou");
        Assert.AreEqual(2f * (1f - padrao.Nivel), 1f - dobro.Nivel, 1e-3f, "o dobro da taxa, o dobro derramado");
    }

    [Test]
    public void DesativadoNoMeioDevolveAPose()
    {
        Vector3 posicao = pneu.transform.position;
        interactor.IniciarMinigame(pneu);
        QuadrosEmFase(30);
        Assert.Greater(Quaternion.Angle(rotacaoInicial, pneu.transform.rotation), 1f, "o pneu inclinou");

        pneu.gameObject.SetActive(false);
        pneu.OnDisable();   // no EditMode o Unity não manda o OnDisable a este script
        pneu.gameObject.SetActive(true);

        AssertPoseOriginal(posicao);
    }

    [Test]
    public void NovaSessaoComAAnteriorDePeVoltaAPoseAntes()
    {
        Vector3 posicao = pneu.transform.position;
        interactor.IniciarMinigame(pneu);
        QuadrosEmFase(30);
        Assert.Greater(Quaternion.Angle(rotacaoInicial, pneu.transform.rotation), 1f, "o pneu inclinou");

        // A sessão anterior acabou sem gancho nenhum: a nova guarda a pose de antes.
        pneu.AoIniciarMinigame(new OndaDoPneu(2));
        pneu.AoSairDoMinigame();

        AssertPoseOriginal(posicao);
    }
}
