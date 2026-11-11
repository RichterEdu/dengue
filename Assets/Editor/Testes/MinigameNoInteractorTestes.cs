using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

/* ============================================================
 *  MinigameNoInteractorTestes.cs
 *  ---------------------------------------------------------------------
 *  Testes EditMode da base comum de minigame (Minigame.cs) conduzida
 *  pelo Interactor: Passo vira AvancarCom, o último passo encerra, sair
 *  guarda os cliques (só com PodeSair), o erro chama o gancho do foco sem
 *  gastar carga, alvo destruído ou Bloqueado encerram, e a mira da sessão
 *  aponta o olhar do jogador.
 *
 *  O foco de teste (FocoDeTeste) devolve um MinigameFalso, que responde
 *  uma fila de estados, e conta as chamadas dos ganchos. Ele pede Areia
 *  (o kit tem uma carga), para o teste ver se alguma carga foi gasta.
 *
 *  O Start não roda no EditMode e as corrotinas não andam: o foco tem
 *  punchScale 0 e shrinkDuration 0 (concluir só desativa o objeto), e o
 *  Interactor acha kit e ControladorJogador no primeiro uso. O kit e o
 *  Interactor ficam no objeto do ControladorJogador, como no Player.
 *
 *  Rodar:  unity test --mode EditMode --filter MinigameNoInteractorTestes
 * ============================================================ */

[Category("Minigames")]
public class MinigameNoInteractorTestes
{
    /* Sessão falsa: devolve os estados da fila (vazia = EmAndamento). */
    private class MinigameFalso : IMinigame
    {
        public readonly Queue<EstadoDoMinigame> Fila = new Queue<EstadoDoMinigame>();
        public bool Sai = true;

        public EstadoDoMinigame Atualizar(float dt, EntradaDoMinigame entrada)
        {
            return Fila.Count > 0 ? Fila.Dequeue() : EstadoDoMinigame.EmAndamento;
        }

        public float FracaoDoPasso => 0f;
        public bool PodeSair => Sai;
    }

    /* Sessão falsa que mira sempre no mesmo ponto. */
    private class MinigameComMiraFalso : MinigameFalso, IMinigameComMira
    {
        public Vector2 Mira => new Vector2(10f, -5f);
    }

    private ControladorJogador jogador;
    private KitDoAgente kit;
    private Interactor interactor;
    private FocoDeTeste foco;

    [SetUp]
    public void Preparar()
    {
        jogador = TesteUtil.Com<ControladorJogador>("Jogador (teste)");
        kit = jogador.gameObject.AddComponent<KitDoAgente>();
        kit.Selecionar(Ferramenta.Areia);
        interactor = jogador.gameObject.AddComponent<Interactor>();

        foco = TesteUtil.Com<FocoDeTeste>("Foco (teste)");
        TesteUtil.Setar(foco, "ferramentaNecessaria", Ferramenta.Areia);
        TesteUtil.Setar(foco, "requiredClicks", 3);
        TesteUtil.Setar(foco, "punchScale", 0f);
        TesteUtil.Setar(foco, "shrinkDuration", 0f);
    }

    [TearDown]
    public void Limpar()
    {
        TesteUtil.LimparTudo();
    }

    /* Um quadro do minigame, sem sair. */
    private void Quadro(bool sair = false)
    {
        interactor.AvancarMinigame(0.1f, new EntradaDoMinigame(), sair);
    }

    [Test]
    public void PassoViraAvancarCom()
    {
        foco.Sessao.Fila.Enqueue(EstadoDoMinigame.Passo);
        Assert.IsTrue(interactor.IniciarMinigame(foco));

        Quadro();

        Assert.AreEqual(1, foco.CurrentClicks);
        Assert.IsNotNull(interactor.MinigameAtivo);
    }

    [Test]
    public void UltimoPassoEncerra()
    {
        TesteUtil.Setar(foco, "requiredClicks", 1);
        foco.Sessao.Fila.Enqueue(EstadoDoMinigame.Passo);
        interactor.IniciarMinigame(foco);

        Quadro();

        Assert.IsTrue(foco.IsComplete);
        Assert.IsNull(interactor.MinigameAtivo);
    }

    [Test]
    public void SairMantemOsCliques()
    {
        foco.Sessao.Fila.Enqueue(EstadoDoMinigame.Passo);
        interactor.IniciarMinigame(foco);

        Quadro();
        Quadro(sair: true);

        Assert.AreEqual(1, foco.CurrentClicks);
        Assert.AreEqual(1, foco.Saidas);
        Assert.IsNull(interactor.MinigameAtivo);
    }

    [Test]
    public void SairIgnoradoSemPodeSair()
    {
        foco.Sessao.Sai = false;
        interactor.IniciarMinigame(foco);

        Quadro(sair: true);

        Assert.IsNotNull(interactor.MinigameAtivo);
    }

    [Test]
    public void ErroChamaOGanchoENaoGastaCarga()
    {
        int areiaAntes = kit.Estoque.Quantidade(Ferramenta.Areia);
        foco.Sessao.Fila.Enqueue(EstadoDoMinigame.Errou);
        interactor.IniciarMinigame(foco);

        Quadro();

        Assert.AreEqual(1, foco.Erros);
        Assert.AreEqual(areiaAntes, kit.Estoque.Quantidade(Ferramenta.Areia));
    }

    [Test]
    public void AlvoDestruidoEncerraSemExcecao()
    {
        interactor.IniciarMinigame(foco);
        Object.DestroyImmediate(foco.gameObject);

        Assert.DoesNotThrow(() => Quadro());
        Assert.IsNull(interactor.MinigameAtivo);
    }

    [Test]
    public void FocoSemMinigameNaoInicia()
    {
        Interactable puro = TesteUtil.Com<Interactable>("Foco puro (teste)");

        Assert.IsFalse(interactor.IniciarMinigame(puro));
        Assert.IsNull(interactor.MinigameAtivo);
    }

    [Test]
    public void BloqueioEncerraSemGastarCarga()
    {
        int areiaAntes = kit.Estoque.Quantidade(Ferramenta.Areia);
        foco.Sessao.Fila.Enqueue(EstadoDoMinigame.Passo);
        interactor.IniciarMinigame(foco);
        Quadro();

        interactor.Bloqueado = true;
        interactor.AoBloquear();

        Assert.IsNull(interactor.MinigameAtivo);
        Assert.AreEqual(1, foco.CurrentClicks);
        Assert.AreEqual(areiaAntes, kit.Estoque.Quantidade(Ferramenta.Areia));
        Assert.AreEqual(1, foco.Saidas);
    }

    [Test]
    public void MiraAplicadaNoOlhar()
    {
        foco.Sessao = new MinigameComMiraFalso();
        float yaw = jogador.Yaw;
        float pitch = jogador.Pitch;
        interactor.IniciarMinigame(foco);

        Quadro();

        Assert.AreEqual(yaw + 10f, jogador.Yaw, 0.001f);
        Assert.AreEqual(pitch - 5f, jogador.Pitch, 0.001f);
    }

    /* Foco que devolve a sessão falsa e conta os ganchos. */
    private class FocoDeTeste : Interactable
    {
        public MinigameFalso Sessao = new MinigameFalso();
        public int Erros;
        public int Saidas;

        public override IMinigame CriarMinigame(KitDoAgente kit, int semente) => Sessao;
        public override void AoErrarMinigame(KitDoAgente kit) => Erros++;
        public override void AoSairDoMinigame() => Saidas++;
    }
}
