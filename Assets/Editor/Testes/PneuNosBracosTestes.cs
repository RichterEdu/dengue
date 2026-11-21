using NUnit.Framework;
using UnityEngine;

/* ============================================================
 *  PneuNosBracosTestes.cs
 *  ---------------------------------------------------------------------
 *  Testes EditMode do pneu que se pega cheio e se esvazia NOS BRAÇOS:
 *  as Mãos pegam o pneu na hora (cheio ou vazio), sem minigame no chão;
 *  a água some nos braços (o pneu de pé a taparia) e volta ao largar;
 *  mexer a câmera, para qualquer lado e sem ritmo, derrama (NivelDeAgua cai
 *  na proporção dos graus mexidos), cada passo de nível vira um clique, e
 *  o último esvazia; largar no meio guarda o nível e assenta o pneu de pé,
 *  sem a inclinação do balanço.
 *
 *  O pneu de teste tem 2 cliques, sem punch e sem encolher, e fica em
 *  SemChao, longe dos colliders da cena aberta. O balanço é conduzido
 *  direto pelo BalancarNosBracos (o Interactor só repassa o olhar).
 *
 *  Rodar:  unity command run_tests --mode EditMode --filter_type category --filter Minigames
 * ============================================================ */

[Category("Minigames")]
public class PneuNosBracosTestes
{
    private static readonly Vector3 SemChao = new Vector3(0f, -10000f, 0f);
    private const float Quadro = 1f / 60f;

    private ControladorJogador jogador;
    private KitDoAgente kit;
    private Transform ponto;
    private PneuInteractable pneu;
    private GameObject agua;

    [SetUp]
    public void Preparar()
    {
        jogador = TesteUtil.Com<ControladorJogador>("Jogador (teste)");
        kit = jogador.gameObject.AddComponent<KitDoAgente>();
        ponto = TesteUtil.Novo("Ponto de carga (teste)").transform;
        ponto.position = SemChao;
        kit.PontoDeCarga = ponto;

        Transform grupo = TesteUtil.Novo("Focos (teste)").transform;
        pneu = TesteUtil.Com<PneuInteractable>("Pneu (teste)");
        TesteUtil.Setar(pneu, "requiredClicks", 2);
        TesteUtil.Setar(pneu, "punchScale", 0f);
        TesteUtil.Setar(pneu, "shrinkDuration", 0f);
        pneu.transform.SetParent(grupo, false);
        pneu.transform.position = SemChao + new Vector3(3f, 0f, 0f);
        pneu.transform.rotation = Quaternion.Euler(0f, 30f, 0f);

        agua = TesteUtil.Novo("agua do pneu (teste)");
        agua.transform.SetParent(pneu.transform, false);
        TesteUtil.Setar(pneu, "water", agua);
    }

    [TearDown]
    public void Limpar()
    {
        TesteUtil.LimparTudo();
    }

    private void Pegar()
    {
        Assert.AreEqual(ResultadoDoPasso.Pegou, pneu.AvancarCom(kit));
    }

    /* Mexe a câmera para os lados (até 5° por quadro, um movimento normal de
     * mouse) até acabar o tempo ou esvaziar; devolve os segundos usados. */
    private float MexerACamera(float segundos)
    {
        float t = 0f;
        while (t < segundos && !pneu.Esvaziado)
        {
            float olharX = 5f * Mathf.Sin(2f * Mathf.PI * t / 0.9f);
            pneu.BalancarNosBracos(Quadro, olharX, Vector3.forward);
            t += Quadro;
        }
        return t;
    }

    [Test]
    public void MaosNoPneuCheioPegaNaHora()
    {
        Assert.AreEqual(ResultadoDoPasso.Pegou, pneu.AvancarCom(kit));
        Assert.AreSame(pneu, kit.PneuCarregado);
        Assert.IsFalse(pneu.Esvaziado, "pega cheio");
        Assert.IsTrue(pneu.Carregado);
    }

    [Test]
    public void PneuNoChaoNaoTemMinigame()
    {
        Assert.IsNull(pneu.CriarMinigame(kit, 0), "cheio");
        Assert.IsNull(pneu.ConviteDoMinigame(kit), "cheio");

        pneu.NascerVazio();
        Assert.IsNull(pneu.CriarMinigame(kit, 0), "vazio");
        Assert.IsNull(pneu.ConviteDoMinigame(kit), "vazio");
    }

    [Test]
    public void OutraFerramentaNaoPega()
    {
        kit.Selecionar(Ferramenta.Areia);

        Assert.AreEqual(ResultadoDoPasso.FerramentaErrada, pneu.AvancarCom(kit));
        Assert.IsNull(kit.PneuCarregado);
    }

    [Test]
    public void AguaSomeNosBracosEVoltaAoLargar()
    {
        Assert.IsTrue(agua.activeSelf);
        Pegar();
        Assert.IsFalse(agua.activeSelf, "o pneu de pé taparia o furo com a água deitada");

        Assert.IsTrue(kit.LargarPneu(SemChao, Vector3.forward));
        Assert.IsTrue(agua.activeSelf, "largado cheio, a água volta");
    }

    [Test]
    public void MexerACameraEsvaziaRapido()
    {
        Pegar();

        float gasto = MexerACamera(12f);

        Assert.Less(gasto, 1.5f, "um pouco de câmera já esvazia");
        Assert.IsTrue(pneu.Esvaziado);
        Assert.IsFalse(agua.activeSelf);
        Assert.AreEqual(0f, pneu.NivelDeAgua);
    }

    [Test]
    public void CameraParadaNaoDerrama()
    {
        Pegar();

        for (float t = 0f; t < 5f; t += Quadro)
            pneu.BalancarNosBracos(Quadro, 0f, Vector3.forward);

        Assert.AreEqual(1f, pneu.NivelDeAgua);
        Assert.IsFalse(pneu.Esvaziado);
    }

    [Test]
    public void PoucoMovimentoSemRitmoJaDerrama()
    {
        Pegar();

        for (int i = 0; i < 10; i++)
            pneu.BalancarNosBracos(Quadro, 1f, Vector3.forward);   // 10° no mesmo sentido

        Assert.Less(pneu.NivelDeAgua, 1f);
        Assert.Greater(pneu.NivelDeAgua, 0.5f, "10° derramam só um pouco");
    }

    [Test]
    public void PassosSobemComONivel()
    {
        TesteUtil.Setar(pneu, "requiredClicks", 4);
        Pegar();

        float t = 0f;
        while (t < 12f && pneu.CurrentClicks < 1)
        {
            pneu.BalancarNosBracos(Quadro, 3f * Mathf.Sin(2f * Mathf.PI * t / 0.9f), Vector3.forward);
            t += Quadro;
        }

        Assert.GreaterOrEqual(pneu.CurrentClicks, 1, "o nível fechou um passo");
        int fechados = Mathf.FloorToInt((1f - pneu.NivelDeAgua) * pneu.RequiredClicks + 1e-4f);
        Assert.AreEqual(fechados, pneu.CurrentClicks);
    }

    [Test]
    public void LargarNoMeioDoBalancoAssentaDePe()
    {
        Pegar();
        for (int i = 0; i < 20; i++)
            pneu.BalancarNosBracos(Quadro, 3f * Mathf.Sin(2f * Mathf.PI * i * Quadro / 0.9f), Vector3.forward);
        Assert.Greater(Quaternion.Angle(ponto.rotation, pneu.transform.rotation), 0.5f, "o pneu inclinou nos braços");

        pneu.OndeCai(SemChao, Vector3.forward, 0.3f, out _, out Quaternion giro, out _);
        Assert.IsTrue(kit.LargarPneu(SemChao, Vector3.forward));

        Assert.Less(Quaternion.Angle(giro, pneu.transform.rotation), 0.01f);
    }

    [Test]
    public void UltimoPassoEsvazia()
    {
        Pegar();

        for (int i = 1; i < pneu.RequiredClicks; i++)
            Assert.AreEqual(ResultadoDoPasso.Avancou, pneu.DarPassoNosBracos(), $"passo {i}");

        Assert.AreEqual(ResultadoDoPasso.Esvaziou, pneu.DarPassoNosBracos());
        Assert.IsTrue(pneu.Esvaziado);
        Assert.IsFalse(agua.activeSelf);
        Assert.AreEqual(Vector3.zero, pneu.transform.localPosition);
        Assert.AreEqual(Quaternion.identity, pneu.transform.localRotation);
    }

    [Test]
    public void LargarNoMeioGuardaONivel()
    {
        TesteUtil.Setar(pneu, "requiredClicks", 4);
        Pegar();
        pneu.DarPassoNosBracos();
        pneu.DarPassoNosBracos();

        Assert.IsTrue(kit.LargarPneu(SemChao, Vector3.forward));
        Pegar();

        Assert.AreEqual(1f - 2f / 4f, pneu.NivelDeAgua, 1e-4f);
    }

    [Test]
    public void LargarNoMeioGuardaONivelExato()
    {
        TesteUtil.Setar(pneu, "requiredClicks", 4);
        Pegar();
        float t = 0f;
        while (t < 12f && pneu.CurrentClicks < 1)
        {
            pneu.BalancarNosBracos(Quadro, 5f * Mathf.Sin(2f * Mathf.PI * t / 0.9f), Vector3.forward);
            t += Quadro;
        }
        for (int i = 0; i < 5; i++)
            pneu.BalancarNosBracos(Quadro, 2f, Vector3.forward);
        float nivel = pneu.NivelDeAgua;
        Assert.AreEqual(1, pneu.CurrentClicks, "um passo fechado");
        Assert.Less(nivel, 0.74f, "entre dois limites de passo");
        Assert.Greater(nivel, 0.5f + 1e-3f);

        Assert.IsTrue(kit.LargarPneu(SemChao, Vector3.forward));
        Assert.AreEqual(nivel, pneu.NivelDeAgua, 1e-4f, "largado");
        Pegar();
        Assert.AreEqual(nivel, pneu.NivelDeAgua, 1e-4f, "pego de novo");
        pneu.BalancarNosBracos(Quadro, 0f, Vector3.forward);
        Assert.AreEqual(nivel, pneu.NivelDeAgua, 0.02f, "continua desse nível");
    }

    [Test]
    public void LargarComEnxameIgnoraAsParticulas()
    {
        Vector3 pes = SemChao;
        BoxCollider chao = TesteUtil.Com<BoxCollider>("Chão (teste)");
        chao.size = new Vector3(20f, 0.2f, 20f);
        chao.transform.position = pes + Vector3.down * 0.1f;

        GameObject banda = TesteUtil.Primitivo(PrimitiveType.Cube, "Banda (teste)");
        Object.DestroyImmediate(banda.GetComponent<Collider>());
        banda.transform.SetParent(pneu.transform, false);
        banda.transform.localScale = new Vector3(0.6f, 2f, 2f);
        pneu.transform.position = pes + new Vector3(5f, 1f, 5f);
        Physics.SyncTransforms();
        Pegar();

        pneu.OndeCai(pes, Vector3.forward, 0.3f, out Vector3 semEnxame, out Quaternion giroSem, out Cacamba cacambaSem);

        GameObject enxame = TesteUtil.Novo("Enxame (teste)");
        enxame.transform.SetParent(pneu.transform, false);
        enxame.transform.localPosition = new Vector3(3f, 5f, 2f);
        enxame.AddComponent<ParticleSystem>().Simulate(0.5f, true, true);

        pneu.OndeCai(pes, Vector3.forward, 0.3f, out Vector3 comEnxame, out Quaternion giroCom, out Cacamba cacambaCom);

        Assert.IsNull(cacambaSem);
        Assert.IsNull(cacambaCom, "nenhuma caçamba falsa");
        Assert.AreEqual(0f, Vector3.Distance(semEnxame, comEnxame), 1e-4f, "mesma posição");
        Assert.Less(Quaternion.Angle(giroSem, giroCom), 0.01f);
        Assert.Greater(Vector3.Distance(comEnxame, pes), 0.3f, "à frente, fora do corpo");
    }

    [Test]
    public void ChuvaNaoReencheNosBracos()
    {
        Pegar();
        Assert.IsFalse(pneu.PodeReencher);
    }

    [Test]
    public void NivelDeAguaVazioEhZero()
    {
        pneu.NascerVazio();
        Assert.AreEqual(0f, pneu.NivelDeAgua);
    }

    [Test]
    public void PesoSegueONivel()
    {
        Assert.AreEqual(0.5f, kit.MultiplicadorComPneu(1f), 1e-5f);
        Assert.AreEqual(0.85f, kit.MultiplicadorComPneu(0f), 1e-5f);
        Assert.AreEqual(0.675f, kit.MultiplicadorComPneu(0.5f), 1e-5f);
    }

    [Test]
    public void PegarCheioDeixaLento()
    {
        Pegar();
        Assert.AreEqual(0.5f, jogador.MultiplicadorVelocidade, 1e-5f);
    }

    [Test]
    public void EsvaziandoFicaMaisLeve()
    {
        TesteUtil.Setar(pneu, "requiredClicks", 4);
        Pegar();
        pneu.DarPassoNosBracos();
        pneu.DarPassoNosBracos();

        kit.AvancarStamina(0.016f, false);

        Assert.AreEqual(Mathf.Lerp(0.85f, 0.5f, 1f - 2f / 4f), jogador.MultiplicadorVelocidade, 1e-4f);
    }

    /* Chão e caçamba como em DestinosTestes.Largar_SobreCacamba_Entrega. */
    private Cacamba CacambaNaFrente()
    {
        BoxCollider chao = TesteUtil.Com<BoxCollider>("Chão (teste)");
        chao.size = new Vector3(20f, 0.2f, 20f);
        chao.transform.position = SemChao + Vector3.down * 0.1f;

        BoxCollider caixa = TesteUtil.Com<BoxCollider>("Caçamba (teste)");
        caixa.center = new Vector3(0f, 0.6f, 0f);
        caixa.size = new Vector3(2.4f, 1.2f, 1.4f);
        caixa.transform.position = SemChao + Vector3.forward * 0.9f;
        Cacamba cacamba = caixa.gameObject.AddComponent<Cacamba>();
        Physics.SyncTransforms();
        return cacamba;
    }

    [Test]
    public void CacambaRecusaPneuComAgua()
    {
        Cacamba cacamba = CacambaNaFrente();
        Pegar();

        Assert.IsFalse(cacamba.PodeUsar(kit, out string motivo));
        Assert.AreEqual(Cacamba.RecusaPneuComAgua, motivo);
        Assert.AreEqual("Esvazie o pneu antes: balance a câmera.", motivo, "texto exigido, sem tags");

        TesteUtil.EsvaziarPneu(pneu, kit, false);
        Assert.IsTrue(cacamba.PodeUsar(kit, out _), "esvaziado, a caçamba aceita");
    }

    [Test]
    public void LargarCheioSobreACacambaNaoEntrega()
    {
        CacambaNaFrente();
        Pegar();

        Assert.IsFalse(kit.LargarPneu(SemChao, Vector3.forward));

        Assert.AreSame(pneu, kit.PneuCarregado, "continua nos braços");
        Assert.IsFalse(pneu.IsComplete);
    }

    [Test]
    public void InteractorRepassaOOlhar()
    {
        Interactor interactor = jogador.gameObject.AddComponent<Interactor>();
        Pegar();

        interactor.Bloqueado = true;
        for (float t = 0f; t < 2f; t += Quadro)
            interactor.BalancarPneuNosBracos(Quadro, new Vector2(5f * Mathf.Sin(2f * Mathf.PI * t / 0.9f), 0f));
        Assert.AreEqual(1f, pneu.NivelDeAgua, "bloqueado, não balança");

        interactor.Bloqueado = false;
        for (float t = 0f; t < 3f; t += Quadro)
            interactor.BalancarPneuNosBracos(Quadro, new Vector2(5f * Mathf.Sin(2f * Mathf.PI * t / 0.9f), 0f));
        Assert.Less(pneu.NivelDeAgua, 1f, "mexer a câmera derrama");
    }

    [Test]
    public void InteractorContaOOlharVertical()
    {
        Interactor interactor = jogador.gameObject.AddComponent<Interactor>();
        Pegar();

        for (int i = 0; i < 10; i++)
            interactor.BalancarPneuNosBracos(Quadro, new Vector2(0f, 2f));

        Assert.Less(pneu.NivelDeAgua, 1f, "mexer a câmera para cima e para baixo também derrama");
    }

    [Test]
    public void ForaDosBracosBalancarNaoFazNada()
    {
        Vector3 posicao = pneu.transform.position;
        Quaternion giro = pneu.transform.rotation;

        MexerACamera(1f);

        Assert.AreEqual(1f, pneu.NivelDeAgua);
        Assert.AreEqual(0f, Vector3.Distance(posicao, pneu.transform.position), 1e-5f);
        Assert.Less(Quaternion.Angle(giro, pneu.transform.rotation), 0.001f);
    }

    [Test]
    public void AcaoNoPneuCheioEhPegar()
    {
        Assert.AreEqual(Ferramenta.Maos, kit.Selecionada);
        Assert.AreEqual("Clique para pegar o pneu", DengueHUD.AcaoDoFoco(pneu, false, kit));
    }
}
