using NUnit.Framework;
using UnityEngine;

/* ============================================================
 *  PneuEsvaziarTestes.cs
 *  ---------------------------------------------------------------------
 *  Testes EditMode (Parte B) do pneu esvaziado no lugar: com as Mãos, o
 *  último passo tira a água (Esvaziou) e o pneu fica onde estava; o
 *  passo seguinte é que o põe nos braços (Pegou). Reenchido (a chuva), a
 *  água volta, os cliques zeram e as Mãos esvaziam de novo. Nos braços,
 *  a chuva não reenche. No caminho antigo (Interact(), sem kit), o
 *  último clique conclui como antes.
 *
 *  O Start não roda no EditMode e as corrotinas não andam. Por isso o
 *  pneu de teste tem 2 cliques, sem punch (punchScale 0) e sem animação
 *  de conclusão (shrinkDuration 0: o OnComplete só desativa o objeto),
 *  e a água é um filho de teste ligado no campo 'water'. Ele fica num
 *  grupo de teste (o pai dos focos na cena) e em SemChao, bem abaixo do
 *  mapa, longe dos colliders da cena aberta.
 *
 *  O kit fica num objeto com ControladorJogador e começa com as Mãos
 *  selecionadas; o PontoDeCarga é um Transform de teste.
 *
 *  Os objetos nascem pelo TesteUtil (escondidos, fora da cena) e são
 *  destruídos no [TearDown].
 *
 *  Rodar:  Window > General > Test Runner > EditMode (categoria ParteB)
 *  ou      unity command run_tests --mode EditMode --filter_type category --filter ParteB
 * ============================================================ */

[Category("ParteB")]
public class PneuEsvaziarTestes
{
    // Bem abaixo de qualquer cena do jogo: aqui o raycast não acha chão.
    private static readonly Vector3 SemChao = new Vector3(0f, -10000f, 0f);

    private KitDoAgente kit;
    private Transform grupo;
    private PneuInteractable pneu;
    private GameObject agua;

    [SetUp]
    public void Preparar()
    {
        ControladorJogador jogador = TesteUtil.Com<ControladorJogador>("Jogador (teste)");
        kit = jogador.gameObject.AddComponent<KitDoAgente>();
        kit.PontoDeCarga = TesteUtil.Novo("Ponto de carga (teste)").transform;

        grupo = TesteUtil.Novo("Focos (teste)").transform;
        pneu = TesteUtil.Com<PneuInteractable>("Pneu (teste)");
        TesteUtil.Setar(pneu, "requiredClicks", 2);
        TesteUtil.Setar(pneu, "punchScale", 0f);
        TesteUtil.Setar(pneu, "shrinkDuration", 0f);
        pneu.transform.SetParent(grupo, false);
        pneu.transform.position = SemChao;

        agua = TesteUtil.Novo("agua do pneu (teste)");
        agua.transform.SetParent(pneu.transform, false);
        TesteUtil.Setar(pneu, "water", agua);
    }

    [TearDown]
    public void Limpar()
    {
        TesteUtil.LimparTudo();
    }

    /* Os dois passos com as Mãos: o pneu fica esvaziado no lugar. */
    private void EsvaziarNoLugar()
    {
        pneu.AvancarCom(kit);
        pneu.AvancarCom(kit);
    }

    [Test]
    public void UltimoPasso_EsvaziaEFicaNoLugar()
    {
        int esvaziados = 0, concluidos = 0;
        bool aguaLigadaNoAviso = true;
        pneu.OnEsvaziado += f =>
        {
            esvaziados++;
            aguaLigadaNoAviso = agua.activeSelf;
        };
        pneu.OnCompleted += f => concluidos++;

        Assert.AreEqual(ResultadoDoPasso.Avancou, pneu.AvancarCom(kit), "1º passo");
        Assert.AreEqual(ResultadoDoPasso.Esvaziou, pneu.AvancarCom(kit), "2º passo: a água sai, o pneu fica");

        Assert.IsNull(kit.PneuCarregado, "não vai para os braços");
        Assert.AreSame(grupo, pneu.transform.parent, "continua no grupo dos focos");
        Assert.IsTrue(pneu.Esvaziado);
        Assert.AreEqual(1, esvaziados, "OnEsvaziado 1 vez");
        Assert.IsFalse(aguaLigadaNoAviso, "quem ouve o OnEsvaziado já vê o pneu sem água");
        Assert.IsFalse(agua.activeSelf);
        Assert.IsFalse(pneu.IsComplete, "esvaziar não conclui");
        Assert.AreEqual(0, concluidos, "OnCompleted");
        Assert.IsTrue(pneu.CanInteract, "continua alvo: pegar e levar à caçamba");
    }

    [Test]
    public void Esvaziado_ProximoPasso_Pega()
    {
        EsvaziarNoLugar();

        Assert.AreEqual(ResultadoDoPasso.Pegou, pneu.AvancarCom(kit));
        Assert.AreSame(pneu, kit.PneuCarregado);
        Assert.IsTrue(pneu.Carregado);
    }

    [Test]
    public void Reencher_NoChao_AguaVoltaECliquesZeram()
    {
        EsvaziarNoLugar();
        int reenchidos = 0;
        pneu.OnReenchido += f => reenchidos++;

        Assert.IsTrue(pneu.PodeReencher, "esvaziado no chão, a chuva enche");
        pneu.Reencher();

        Assert.IsFalse(pneu.Esvaziado);
        Assert.IsTrue(agua.activeSelf, "a água volta");
        Assert.AreEqual(0, pneu.CurrentClicks, "os cliques zeram");
        Assert.IsTrue(pneu.Zumbe, "com água, volta a zumbir");
        Assert.AreEqual(1, reenchidos, "OnReenchido 1 vez");
    }

    [Test]
    public void Reencher_DepoisPodeEsvaziarDeNovo()
    {
        int esvaziados = 0;
        pneu.OnEsvaziado += f => esvaziados++;
        EsvaziarNoLugar();
        pneu.Reencher();

        Assert.IsTrue(pneu.CanInteract, "reenchido, volta a ser alvo das Mãos");
        Assert.AreEqual(ResultadoDoPasso.Avancou, pneu.AvancarCom(kit), "1º passo");
        Assert.AreEqual(ResultadoDoPasso.Esvaziou, pneu.AvancarCom(kit), "2º passo: esvazia de novo");

        Assert.IsTrue(pneu.Esvaziado);
        Assert.IsFalse(agua.activeSelf);
        Assert.AreEqual(2, esvaziados, "OnEsvaziado nas duas vezes");
        Assert.IsNull(kit.PneuCarregado);
    }

    [Test]
    public void Carregado_NaoReenche()
    {
        EsvaziarNoLugar();
        Assert.AreEqual(ResultadoDoPasso.Pegou, pneu.AvancarCom(kit));
        int reenchidos = 0;
        pneu.OnReenchido += f => reenchidos++;

        Assert.IsFalse(pneu.PodeReencher, "nos braços, a chuva não enche");
        pneu.Reencher();

        Assert.IsTrue(pneu.Esvaziado);
        Assert.IsFalse(agua.activeSelf);
        Assert.AreEqual(0, reenchidos, "OnReenchido");
    }

    /* Caminho antigo (clique sem kit): o último clique conclui, como antes do kit. */
    [Test]
    public void Interact_SemKit_ConcluiNoUltimoClique()
    {
        int concluidos = 0;
        pneu.OnCompleted += f => concluidos++;

        pneu.Interact();
        pneu.Interact();

        Assert.IsTrue(pneu.IsComplete);
        Assert.AreEqual(1, concluidos, "OnCompleted");
        Assert.IsFalse(agua.activeSelf);
    }
}
