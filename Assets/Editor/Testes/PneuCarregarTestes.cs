using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

/* ============================================================
 *  PneuCarregarTestes.cs
 *  ---------------------------------------------------------------------
 *  Testes EditMode (Parte A) do pneu carregado até a caçamba: com as
 *  Mãos, o último passo esvazia o pneu no lugar (Parte B) e o passo
 *  seguinte o põe nos braços (sem concluir), o agente
 *  fica mais lento e sem trocar de ferramenta, largar devolve o foco (do
 *  lado de cá de um muro, ao alcance da mira, fora do corpo do agente) e
 *  pegar de novo é instantâneo, e só a entrega conclui (pela mira ou
 *  largando o pneu em cima da caçamba). Na Parte B o agente carregando
 *  anda a 0,85 da velocidade, e o pneu entregue vai para uma vaga da
 *  caçamba da picape (fora do Play, na hora).
 *
 *  O Start não roda no EditMode e as corrotinas não andam. Por isso o
 *  pneu de teste tem 2 cliques, sem punch (punchScale 0) e sem animação
 *  de conclusão (shrinkDuration 0: o OnComplete só desativa o objeto).
 *  O kit fica num objeto com ControladorJogador, e o PontoDeCarga é um
 *  Transform de teste.
 *
 *  O Editor costuma estar com a SampleScene aberta, e os raycasts do
 *  pneu enxergam o chão dela. Por isso o pneu e os pés do agente ficam
 *  em SemChao, bem abaixo do mapa, onde não há collider nenhum; o chão,
 *  o muro e a caçamba de cada teste são colliders de teste ali.
 *
 *  Os testes da revisão final usam o pneu como na cena: 5 cliques com
 *  encolher por clique, ~2 m de diâmetro, em pé, com o eixo da roda no X
 *  local, e o corpo do agente com as medidas do Player.
 *
 *  Os objetos nascem pelo TesteUtil (escondidos, fora da cena) e são
 *  destruídos no [TearDown].
 *
 *  Rodar:  Window > General > Test Runner > EditMode (categoria ParteA)
 *  ou      unity command run_tests --mode EditMode --filter_type category --filter ParteA
 * ============================================================ */

[Category("ParteA")]
public class PneuCarregarTestes
{
    // Bem abaixo de qualquer cena do jogo: aqui o raycast não acha chão.
    private static readonly Vector3 SemChao = new Vector3(0f, -10000f, 0f);

    // O corpo do Player na cena (CharacterController do PlayerBuilder).
    private const float RaioDoCorpo = 0.3f;
    private const float PeleDoCorpo = 0.03f;
    private const float AlturaDoCorpo = 1.75f;

    private ControladorJogador jogador;
    private KitDoAgente kit;
    private Transform ponto;
    private PneuInteractable pneu;

    [SetUp]
    public void Preparar()
    {
        jogador = TesteUtil.Com<ControladorJogador>("Jogador (teste)");
        kit = jogador.gameObject.AddComponent<KitDoAgente>();
        ponto = TesteUtil.Novo("Ponto de carga (teste)").transform;
        kit.PontoDeCarga = ponto;

        pneu = TesteUtil.Com<PneuInteractable>("Pneu (teste)");
        TesteUtil.Setar(pneu, "requiredClicks", 2);
        TesteUtil.Setar(pneu, "punchScale", 0f);
        TesteUtil.Setar(pneu, "shrinkDuration", 0f);
        pneu.transform.position = SemChao;
    }

    [TearDown]
    public void Limpar()
    {
        TesteUtil.LimparTudo();
    }

    /* Os dois passos com as Mãos esvaziam o pneu no lugar; o terceiro o
     * põe nos braços. */
    private void PegarNasMaos()
    {
        pneu.AvancarCom(kit);
        pneu.AvancarCom(kit);
        pneu.AvancarCom(kit);
    }

    /* Chão de teste de 20 × 20 m, com o topo na altura dos pés. */
    private static void CriarChao(Vector3 pes)
    {
        BoxCollider chao = TesteUtil.Com<BoxCollider>("Chão (teste)");
        chao.size = new Vector3(20f, 0.2f, 20f);
        chao.transform.position = pes + Vector3.down * 0.1f;
    }

    /* O Start não roda no EditMode; chamado por reflexão, o foco guarda a
     * escala inicial como no Play (sem ele, ela ficaria zero). */
    private static void RodarStart(Interactable foco)
    {
        MethodInfo start = typeof(Interactable).GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(start, "Interactable.Start não encontrado");
        start.Invoke(foco, null);
    }

    /* O corpo do agente de teste com as medidas do Player da cena (o padrão
     * do CharacterController é maior: raio 0,5 m e 2 m de altura). */
    private void CorpoComoNaCena()
    {
        CharacterController corpo = jogador.GetComponent<CharacterController>();
        corpo.radius = RaioDoCorpo;
        corpo.skinWidth = PeleDoCorpo;
        corpo.height = AlturaDoCorpo;
        corpo.center = new Vector3(0f, AlturaDoCorpo / 2f, 0f);
    }

    /* Dá ao pneu de teste a forma do pneu da cena (rubber tries.fbx): banda
     * de 0,6 m no X local e disco de 2 m no plano YZ, com o pivô no centro.
     * Malha (para o pneu medir o próprio tamanho) e collider na raiz (como
     * o MeshCollider da cena), do mesmo tamanho. Devolve o collider. */
    private BoxCollider DarFormaDePneu()
    {
        Vector3 tamanho = new Vector3(0.6f, 2f, 2f);

        GameObject banda = TesteUtil.Novo("Banda do pneu (teste)");
        banda.transform.SetParent(pneu.transform, false);
        banda.transform.localScale = tamanho;
        banda.AddComponent<MeshFilter>().sharedMesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
        banda.AddComponent<MeshRenderer>();

        BoxCollider forma = pneu.gameObject.AddComponent<BoxCollider>();
        forma.size = tamanho;
        return forma;
    }

    /* Distância no plano do chão entre um ponto e a caixa (0 = por cima/dentro). */
    private static float DistanciaNoChao(Bounds limites, Vector3 ponto)
    {
        float dx = Mathf.Max(limites.min.x - ponto.x, 0f, ponto.x - limites.max.x);
        float dz = Mathf.Max(limites.min.z - ponto.z, 0f, ponto.z - limites.max.z);
        return Mathf.Sqrt(dx * dx + dz * dz);
    }

    private void AssertEscala(Vector3 esperada, string onde)
    {
        Vector3 escala = pneu.transform.localScale;
        Assert.That(Vector3.Distance(esperada, escala), Is.LessThan(1e-4f),
                    $"{onde}: escala {escala.ToString("F2")}, esperada {esperada.ToString("F2")}");
    }

    [Test]
    public void TerminarAcao_PegaENaoConclui()
    {
        int pegos = 0;
        kit.OnPneuPego += p => pegos++;

        Assert.AreEqual(ResultadoDoPasso.Avancou, pneu.AvancarCom(kit), "1º passo");
        Assert.AreEqual(ResultadoDoPasso.Esvaziou, pneu.AvancarCom(kit), "2º passo: esvazia no lugar");
        Assert.AreEqual(ResultadoDoPasso.Pegou, pneu.AvancarCom(kit), "o passo seguinte: vai para os braços");

        Assert.AreSame(pneu, kit.PneuCarregado);
        Assert.IsFalse(pneu.IsComplete, "só conclui na caçamba");
        Assert.IsFalse(pneu.CanInteract, "nos braços não é alvo");
        Assert.AreSame(ponto, pneu.transform.parent, "fica no PontoDeCarga");
        Assert.AreEqual(0.85f, jogador.MultiplicadorVelocidade, 1e-5f);   // Parte B: era 0,7
        Assert.IsFalse(jogador.PodePular);
        Assert.AreEqual(1, pegos, "OnPneuPego");
    }

    [Test]
    public void Largar_SemChao_CaiNoPe()
    {
        int largados = 0;
        kit.OnPneuLargado += p => largados++;
        PegarNasMaos();

        Vector3 pes = SemChao + new Vector3(5f, 0f, 5f);
        Assert.IsTrue(kit.LargarPneu(pes, Vector3.forward));

        Assert.That(Vector3.Distance(pes, pneu.transform.position), Is.LessThan(1e-3f), "cai no pé, não no vazio");
        Assert.IsNull(pneu.transform.parent);
        Assert.IsTrue(pneu.CanInteract, "largado, continua sendo foco");
        Assert.IsNull(kit.PneuCarregado);
        Assert.AreEqual(1f, jogador.MultiplicadorVelocidade, "movimento restaurado");
        Assert.IsTrue(jogador.PodePular, "movimento restaurado");
        Assert.AreEqual(1, largados, "OnPneuLargado");

        Assert.IsFalse(kit.LargarPneu(pes, Vector3.forward), "sem pneu nos braços");
    }

    /* Encostado num muro (a 0,35 m), largar não pode jogar o pneu do outro
     * lado: lá a mira não alcança e a partida não fecha. Chão e muro são
     * colliders de teste em SemChao, longe da SampleScene. */
    [Test]
    public void Largar_ComMuroNaFrente_NaoAtravessa()
    {
        Vector3 pes = SemChao;
        Vector3 frente = Vector3.forward;
        CriarChao(pes);

        BoxCollider muro = TesteUtil.Com<BoxCollider>("Muro (teste)");
        muro.size = new Vector3(4f, 3f, 0.3f);
        muro.transform.position = pes + frente * 0.5f + Vector3.up * 1.5f;   // face de cá a 0,35 m

        // O pivô do pneu 0,7 m acima do chão, como nos pneus da cena.
        pneu.transform.position = pes + Vector3.up * 0.7f;
        Physics.SyncTransforms();

        PegarNasMaos();
        Assert.IsTrue(kit.LargarPneu(pes, frente));

        Vector3 desloc = pneu.transform.position - pes;
        Assert.That(Vector3.Dot(desloc, frente), Is.LessThan(0.5f), "não passou do muro");
        Assert.AreEqual(0.7f, desloc.y, 1e-3f, "assentado no chão do lado de cá");
        Assert.IsTrue(pneu.CanInteract, "largado, continua sendo foco");
    }

    /* Regressão da verificação final da Parte B. Encostado num muro, o pneu
     * recua para perto do agente, e o raio do chão desce por dentro do
     * corpo dele, que tem collider fora da camada CorpoJogador: o raio
     * parava no corpo, e o pneu ficava flutuando acima da cabeça. O corpo
     * do agente nunca é chão. */
    [Test]
    public void Largar_ColadoNoMuro_OCorpoDoAgenteNaoEChao()
    {
        Vector3 pes = SemChao;
        Vector3 frente = Vector3.forward;
        CriarChao(pes);
        CorpoComoNaCena();
        BoxCollider corpo = jogador.gameObject.AddComponent<BoxCollider>();
        corpo.size = new Vector3(0.6f, 1.4f, 0.6f);
        corpo.center = new Vector3(0f, 0.7f, 0f);
        jogador.transform.position = pes;

        BoxCollider muro = TesteUtil.Com<BoxCollider>("Muro (teste)");
        muro.size = new Vector3(4f, 3f, 0.3f);
        muro.transform.position = pes + frente * 0.5f + Vector3.up * 1.5f;   // face de cá a 0,35 m

        pneu.transform.position = pes + Vector3.back * 3f + Vector3.up * 0.7f;
        Physics.SyncTransforms();

        PegarNasMaos();
        Assert.IsTrue(kit.LargarPneu(pes, frente));

        Assert.AreEqual(0.7f, pneu.transform.position.y - pes.y, 1e-3f, "assentado no chão, não no corpo do agente");
    }

    [Test]
    public void PegarDeNovo_EInstantaneo()
    {
        PegarNasMaos();
        kit.LargarPneu(SemChao, Vector3.forward);

        Assert.AreEqual(0f, pneu.SegundosPorPasso, "já esvaziado: pegar não leva tempo");
        Assert.AreEqual(ResultadoDoPasso.Pegou, pneu.AvancarCom(kit));
        Assert.AreSame(pneu, kit.PneuCarregado);
    }

    [Test]
    public void Entregar_Conclui()
    {
        int concluidos = 0;
        int entregues = 0;
        pneu.OnCompleted += f => concluidos++;
        kit.OnPneuEntregue += p => entregues++;
        Cacamba cacamba = TesteUtil.Com<Cacamba>("Caçamba (teste)");
        PegarNasMaos();

        Assert.IsTrue(kit.EntregarPneu(cacamba));

        Assert.IsTrue(pneu.IsComplete);
        Assert.AreEqual(1, concluidos, "OnCompleted");
        Assert.IsNull(kit.PneuCarregado);
        Assert.AreEqual(1f, jogador.MultiplicadorVelocidade, "movimento restaurado");
        Assert.IsTrue(jogador.PodePular, "movimento restaurado");
        Assert.AreEqual(1, entregues, "OnPneuEntregue");

        Assert.IsFalse(kit.EntregarPneu(cacamba), "sem pneu nos braços");
        Assert.AreEqual(1, concluidos, "não conclui de novo");
    }

    [Test]
    public void ComPneu_OutroFoco_MaosOcupadas()
    {
        Interactable outro = TesteUtil.Com<Interactable>("Foco (teste)");
        TesteUtil.Setar(outro, "requiredClicks", 2);
        TesteUtil.Setar(outro, "punchScale", 0f);
        TesteUtil.Setar(outro, "shrinkDuration", 0f);
        TesteUtil.Setar(outro, "ferramentaNecessaria", Ferramenta.Maos);
        PegarNasMaos();

        Assert.AreEqual(ResultadoDoPasso.MaosOcupadas, outro.AvancarCom(kit));
        Assert.AreEqual(0, outro.CurrentClicks);
    }

    [Test]
    public void ComPneu_NaoTrocaFerramenta()
    {
        PegarNasMaos();

        Assert.IsFalse(kit.Selecionar(Ferramenta.Areia));
        Assert.AreEqual(Ferramenta.Maos, kit.Selecionada);
    }

    [Test]
    public void Esvaziado_NaoZumbe()
    {
        Assert.IsTrue(pneu.Zumbe, "com água, zumbe");

        PegarNasMaos();

        Assert.IsTrue(pneu.Esvaziado);
        Assert.IsFalse(pneu.Zumbe);
    }

    /* Regressão da revisão final (B). Os 6 pneus da cena têm 5 cliques e
     * scaleReductionPerClick 0,2: cada passo encolhia o pneu, que chegava
     * aos braços com 20% do tamanho e assim ficava ao ser largado. O foco
     * que vai ser carregado não encolhe mais por clique. Com chão: largado,
     * assenta com o pivô na altura medida ao pegar. */
    [Test]
    public void PegarELargar_ComEncolherPorClique_MantemOTamanho()
    {
        Vector3 pes = SemChao;
        CriarChao(pes);
        Vector3 escalaInicial = Vector3.one * 1.3f;
        pneu.transform.localScale = escalaInicial;
        pneu.transform.position = pes + Vector3.back * 2f + Vector3.up * 0.7f;
        TesteUtil.Setar(pneu, "requiredClicks", 5);
        TesteUtil.Setar(pneu, "scaleReductionPerClick", 0.2f);
        RodarStart(pneu);
        Physics.SyncTransforms();

        for (int i = 1; i <= 4; i++)
            Assert.AreEqual(ResultadoDoPasso.Avancou, pneu.AvancarCom(kit), $"{i}º passo");
        Assert.AreEqual(ResultadoDoPasso.Esvaziou, pneu.AvancarCom(kit), "5º passo: esvazia no lugar");
        Assert.AreEqual(ResultadoDoPasso.Pegou, pneu.AvancarCom(kit), "o passo seguinte: vai para os braços");
        AssertEscala(escalaInicial, "nos braços");

        Assert.IsTrue(kit.LargarPneu(pes, Vector3.forward));
        AssertEscala(escalaInicial, "largado");
        Assert.AreEqual(0.7f, pneu.transform.position.y - pes.y, 1e-3f, "assentado no chão, com o pivô a 0,7 m");
    }

    /* Regressão da revisão final (C). O pneu da cena tem ~2 m de diâmetro,
     * em pé, com o eixo da roda no X local. Largado a 0,8 m dos pés com o
     * giro de antes (de lado para o agente), o disco atravessava a cápsula
     * do CharacterController (raio 0,3 m), que levava um tranco ao religar
     * o collider. Agora ele cai com o eixo apontado para a frente e
     * afastado pelo próprio tamanho, de pé e assentado no chão. */
    [Test]
    public void Largar_PneuGrande_NaoNasceDentroDoAgente()
    {
        Vector3 pes = SemChao;
        CriarChao(pes);
        CorpoComoNaCena();
        BoxCollider forma = DarFormaDePneu();
        pneu.transform.position = pes + Vector3.back * 3f + Vector3.up * 1f;   // de pé: pivô no centro, a 1 m do chão
        Physics.SyncTransforms();
        PegarNasMaos();

        Assert.IsTrue(kit.LargarPneu(pes, Vector3.forward));
        Physics.SyncTransforms();

        Collider[] tocados = Physics.OverlapCapsule(pes + Vector3.up * RaioDoCorpo,
                                                    pes + Vector3.up * (AlturaDoCorpo - RaioDoCorpo),
                                                    RaioDoCorpo, ~0, QueryTriggerInteraction.Ignore);
        Assert.AreEqual(-1, System.Array.IndexOf(tocados, forma), "o pneu largado não pode cruzar a cápsula do agente");
        Assert.That(DistanciaNoChao(forma.bounds, pes), Is.GreaterThanOrEqualTo(RaioDoCorpo),
                    "a borda de cá do pneu fica fora do raio do corpo");
        Assert.That(Vector3.Dot(pneu.transform.position - pes, Vector3.forward), Is.GreaterThan(0f), "na frente do agente");
        Assert.AreEqual(1f, pneu.transform.position.y - pes.y, 1e-3f, "de pé, assentado no chão");
    }

    /* Revisão final (M). O jogador encosta na caçamba e larga o pneu (botão
     * direito) achando que joga dentro. É entrega, e o foco conclui, em vez
     * de o pneu ficar em cima dela sem contar. Caçamba de 0,8 m: o raio do
     * peito passa por cima e o do chão acerta o topo (o caso do achado);
     * de 1,2 m: o raio do peito acerta a parede dela. */
    [TestCase(0.8f)]
    [TestCase(1.2f)]
    public void Largar_NaCacamba_Entrega(float alturaDaCacamba)
    {
        Vector3 pes = SemChao;
        CriarChao(pes);
        BoxCollider caixa = TesteUtil.Com<BoxCollider>("Caçamba (teste)");
        caixa.center = new Vector3(0f, alturaDaCacamba / 2f, 0f);
        caixa.size = new Vector3(2.4f, alturaDaCacamba, 1.4f);
        caixa.transform.position = pes + Vector3.forward * 0.9f;   // de 0,2 m a 1,6 m na frente dos pés
        Cacamba cacamba = caixa.gameObject.AddComponent<Cacamba>();

        // Uma vaga na carroceria (Parte B), 1,2 m acima do pivô.
        Transform vaga = TesteUtil.Novo("Vaga (teste)").transform;
        vaga.SetParent(caixa.transform, false);
        vaga.localPosition = Vector3.up * 1.2f;
        SerializedObject so = new SerializedObject(cacamba);
        SerializedProperty vagas = so.FindProperty("vagas");
        vagas.arraySize = 1;
        vagas.GetArrayElementAtIndex(0).objectReferenceValue = vaga;
        so.ApplyModifiedPropertiesWithoutUndo();

        pneu.transform.position = pes + Vector3.back * 2f + Vector3.up * 0.7f;
        Physics.SyncTransforms();

        int entregues = 0;
        int largados = 0;
        kit.OnPneuEntregue += p => entregues++;
        kit.OnPneuLargado += p => largados++;
        PegarNasMaos();

        Assert.IsTrue(kit.LargarPneu(pes, Vector3.forward));

        Assert.IsTrue(pneu.IsComplete, "largado na caçamba, conta como entregue");
        Assert.IsNull(kit.PneuCarregado, "mãos livres");
        Assert.AreEqual(1, entregues, "OnPneuEntregue");
        Assert.AreEqual(0, largados, "OnPneuLargado");
        Assert.AreSame(vaga, pneu.VagaNaCacamba, "na vaga da caçamba, como na entrega pela mira");
        Assert.That(Vector3.Distance(caixa.transform.position + Vector3.up * 1.2f, pneu.transform.position),
                    Is.LessThan(1e-4f), "na vaga, 1,2 m acima do pivô");
    }
}
