using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

/* ============================================================
 *  DestinosTestes.cs
 *  ---------------------------------------------------------------------
 *  Testes EditMode (Parte B) dos destinos do que o agente recolhe:
 *
 *    - a caçamba da picape da UPA (Cacamba) só recebe pneus: cada pneu
 *      entregue conclui e ocupa a próxima vaga da carroceria (pulando as
 *      vagas vazias da lista); o OnPneuRecebido conta os pneus recebidos
 *      (1, 2, 3...), com ou sem vaga. Sem vaga livre, conclui e encolhe
 *      1,2 m acima do pivô da caçamba (Review Focus 5). Sem pneu, delega à
 *      picape (reabastecer) e, sem picape, recusa;
 *    - a cabine da picape (PontoDeReabastecimento com uma Cacamba filha),
 *      mirada com o pneu nas mãos, entrega na caçamba;
 *    - a lixeira (Lixeira) esvazia o saco e recusa o pneu e o saco vazio;
 *      sem tampa, a raiz balança ±6° e volta ao giro original (o ângulo e
 *      o giro são chamados direto, porque as corrotinas não andam aqui);
 *    - os textos da mira na caçamba e na lixeira não repetem o "segure"
 *      (a HUD acrescenta a linha "segure o clique" embaixo);
 *    - largar o pneu em cima da caçamba é entrega, e contra a cabine da
 *      picape (o collider do PontoDeReabastecimento) também; contra o
 *      monte de areia, sem Cacamba filha, é largar;
 *    - o tempo de segurar na cabine com o pneu nas mãos é o da caçamba
 *      (SegundosParaUsarPara);
 *    - carregando, o agente anda a 0,85 da velocidade;
 *    - a água do pneu só some quando ele fica Esvaziado (no último passo
 *      com as Mãos), e não no meio dos passos.
 *
 *  Fora do Play o arremesso é instantâneo (Application.isPlaying é
 *  false): o pneu vai direto para a vaga e o OnPneuRecebido sai na hora.
 *  As corrotinas não andam no EditMode, por isso os focos de teste não
 *  têm punch (punchScale 0) nem animação de conclusão (shrinkDuration 0:
 *  o OnComplete só desativa o objeto).
 *
 *  O kit fica num objeto com ControladorJogador e um PontoDeCarga de
 *  teste. A lista 'vagas' é trocada pelo SerializedObject (o
 *  TesteUtil.Setar não aceita listas). O teste de largar fica em SemChao,
 *  bem abaixo do mapa, longe dos colliders da cena aberta.
 *
 *  Os objetos nascem pelo TesteUtil (escondidos, fora da cena) e são
 *  destruídos no [TearDown].
 *
 *  Rodar:  Window > General > Test Runner > EditMode (categoria ParteB)
 *  ou      unity command run_tests --mode EditMode --filter_type category --filter ParteB
 * ============================================================ */

[Category("ParteB")]
public class DestinosTestes
{
    // Bem abaixo de qualquer cena do jogo: aqui o raycast não acha chão.
    private static readonly Vector3 SemChao = new Vector3(0f, -10000f, 0f);

    private const string TextoJogarPneu = "Jogar o pneu na <b>caçamba</b>";

    private ControladorJogador jogador;
    private KitDoAgente kit;

    [SetUp]
    public void Preparar()
    {
        jogador = TesteUtil.Com<ControladorJogador>("Jogador (teste)");
        kit = jogador.gameObject.AddComponent<KitDoAgente>();
        kit.PontoDeCarga = TesteUtil.Novo("Ponto de carga (teste)").transform;
    }

    [TearDown]
    public void Limpar()
    {
        TesteUtil.LimparTudo();
    }

    /* Pneu de teste sem punch nem animação de conclusão, em SemChao. */
    private static PneuInteractable NovoPneu(string nome = "Pneu (teste)")
    {
        PneuInteractable pneu = TesteUtil.Com<PneuInteractable>(nome);
        TesteUtil.Setar(pneu, "requiredClicks", 2);
        TesteUtil.Setar(pneu, "punchScale", 0f);
        TesteUtil.Setar(pneu, "shrinkDuration", 0f);
        pneu.transform.position = SemChao;
        return pneu;
    }

    /* Pneu de teste já nos braços do agente. */
    private PneuInteractable PneuNasMaos(string nome = "Pneu (teste)")
    {
        PneuInteractable pneu = NovoPneu(nome);
        Assert.IsTrue(kit.Pegar(pneu), "pneu nos braços");
        return pneu;
    }

    /* Caçamba de teste com 'quantas' vagas (filhas dela), devolvidas em 'vagas'. */
    private static Cacamba NovaCacamba(int quantas, out List<Transform> vagas)
    {
        Cacamba cacamba = TesteUtil.Com<Cacamba>("Caçamba da picape (teste)");
        vagas = new List<Transform>();
        for (int i = 0; i < quantas; i++)
        {
            Transform vaga = TesteUtil.Novo($"Vaga {i + 1} (teste)").transform;
            vaga.SetParent(cacamba.transform, false);
            vaga.localPosition = new Vector3(0f, 0.3f * i, 0f);
            vagas.Add(vaga);
        }
        DefinirVagas(cacamba, vagas);
        return cacamba;
    }

    /* Troca a lista 'vagas' pelo SerializedObject, como o Inspector. */
    private static void DefinirVagas(Cacamba cacamba, List<Transform> vagas)
    {
        SerializedObject so = new SerializedObject(cacamba);
        SerializedProperty lista = so.FindProperty("vagas");
        Assert.IsNotNull(lista, "o campo serializado 'vagas' existe");

        lista.arraySize = vagas.Count;
        for (int i = 0; i < vagas.Count; i++)
            lista.GetArrayElementAtIndex(i).objectReferenceValue = vagas[i];
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    /* ---------------- Caçamba da picape ---------------- */

    [Test]
    public void Cacamba_EntregaPneuNaPrimeiraVaga()
    {
        Cacamba cacamba = NovaCacamba(2, out List<Transform> vagas);
        List<int> numerosNoAviso = new List<int>();
        cacamba.OnPneuRecebido += (p, numero) => numerosNoAviso.Add(numero);
        PneuInteractable pneu = PneuNasMaos();

        Assert.IsTrue(cacamba.PodeUsar(kit, out string motivo), $"pneu nas mãos (motivo: {motivo})");
        cacamba.Usar(kit);

        Assert.IsTrue(pneu.IsComplete, "entregue, o pneu conclui");
        Assert.IsNull(kit.PneuCarregado, "mãos livres");
        Assert.AreSame(vagas[0], pneu.VagaNaCacamba);
        Assert.AreEqual(1, cacamba.Ocupadas);
        Assert.AreEqual(2, cacamba.Capacidade);

        // Fora do Play o arremesso é instantâneo: já está na vaga, sem encolher.
        Assert.AreSame(vagas[0], pneu.transform.parent, "filho da vaga");
        Assert.That(pneu.transform.localPosition.magnitude, Is.LessThan(1e-4f), "pose local zero");
        Assert.IsTrue(pneu.gameObject.activeSelf, "na vaga, o pneu não some");
        CollectionAssert.AreEqual(new[] { 1 }, numerosNoAviso, "OnPneuRecebido 1×, o 1º pneu recebido");
    }

    [Test]
    public void Cacamba_SegundoPneu_SegundaVaga()
    {
        Cacamba cacamba = NovaCacamba(2, out List<Transform> vagas);
        List<int> numerosNoAviso = new List<int>();
        cacamba.OnPneuRecebido += (p, numero) => numerosNoAviso.Add(numero);

        PneuInteractable primeiro = PneuNasMaos("Pneu 1 (teste)");
        cacamba.Usar(kit);
        PneuInteractable segundo = PneuNasMaos("Pneu 2 (teste)");
        cacamba.Usar(kit);

        Assert.AreSame(vagas[0], primeiro.VagaNaCacamba);
        Assert.AreSame(vagas[1], segundo.VagaNaCacamba);
        Assert.IsTrue(segundo.IsComplete);
        Assert.AreEqual(2, cacamba.Ocupadas);
        CollectionAssert.AreEqual(new[] { 1, 2 }, numerosNoAviso);
    }

    /* Review Focus 5: sem vaga livre (0 vagas), o pneu ainda conclui e
     * encolhe como antes, num ponto visível (1,2 m acima do pivô da
     * caçamba), e o OnPneuRecebido sai na hora. */
    [Test]
    public void Cacamba_SemVaga_ConcluiEEncolhe()
    {
        Cacamba cacamba = NovaCacamba(0, out _);
        int recebidos = 0;
        PneuInteractable recebido = null;
        cacamba.OnPneuRecebido += (p, numero) => { recebidos++; recebido = p; };
        PneuInteractable pneu = PneuNasMaos();

        Assert.DoesNotThrow(() => cacamba.Usar(kit));

        Assert.IsTrue(pneu.IsComplete);
        Assert.IsNull(pneu.VagaNaCacamba);
        Assert.AreEqual(0, cacamba.Ocupadas);
        Assert.AreEqual(1, recebidos, "OnPneuRecebido");
        Assert.AreSame(pneu, recebido);
        Assert.IsFalse(pneu.gameObject.activeSelf, "sem vaga, encolhe até sumir (shrinkDuration 0: some na hora)");
        Vector3 pontoVisivel = cacamba.transform.position + Vector3.up * 1.2f;
        Assert.That(Vector3.Distance(pontoVisivel, pneu.transform.position), Is.LessThan(1e-4f),
                    "encolhe 1,2 m acima do pivô, e não dentro dele");
    }

    /* Revisão da Task 8: a contagem do aviso é a dos pneus recebidos, e não
     * a das vagas ocupadas. Com 1 vaga, o 2º pneu conclui sem vaga e o
     * aviso diz 2. */
    [Test]
    public void Cacamba_Cheia_SegundoPneuConcluiSemVaga()
    {
        Cacamba cacamba = NovaCacamba(1, out List<Transform> vagas);
        List<int> numeros = new List<int>();
        cacamba.OnPneuRecebido += (p, numero) => numeros.Add(numero);

        PneuInteractable primeiro = PneuNasMaos("Pneu 1 (teste)");
        cacamba.Usar(kit);
        PneuInteractable segundo = PneuNasMaos("Pneu 2 (teste)");
        cacamba.Usar(kit);

        Assert.AreSame(vagas[0], primeiro.VagaNaCacamba);
        Assert.IsTrue(segundo.IsComplete, "cheia, o 2º ainda conclui");
        Assert.IsNull(segundo.VagaNaCacamba, "sem vaga");
        Assert.AreEqual(1, cacamba.Ocupadas);
        CollectionAssert.AreEqual(new[] { 1, 2 }, numeros, "o aviso conta os pneus recebidos");
    }

    /* Revisão da Task 8: uma vaga vazia (null) na lista é pulada, e não
     * fecha a carroceria. */
    [Test]
    public void Cacamba_VagaNula_EPulada()
    {
        Cacamba cacamba = NovaCacamba(2, out List<Transform> vagas);
        DefinirVagas(cacamba, new List<Transform> { null, vagas[1] });
        PneuInteractable pneu = PneuNasMaos();

        cacamba.Usar(kit);

        Assert.AreSame(vagas[1], pneu.VagaNaCacamba, "a vaga null é pulada");
        Assert.AreEqual(1, cacamba.Ocupadas);
    }

    [Test]
    public void Cacamba_SemPneu_DelegaAPicape()
    {
        Cacamba cacamba = NovaCacamba(2, out _);
        PontoDeReabastecimento picape = TesteUtil.Com<PontoDeReabastecimento>("Picape (teste)");
        TesteUtil.Setar(cacamba, "picape", picape);
        Assert.IsTrue(kit.Consumir(Ferramenta.Areia));
        Assert.AreEqual(0, kit.Estoque.Quantidade(Ferramenta.Areia));

        Assert.IsTrue(cacamba.PodeUsar(kit, out string motivo), $"falta areia (motivo: {motivo})");
        cacamba.Usar(kit);

        Assert.AreEqual(1, kit.Estoque.Quantidade(Ferramenta.Areia), "a picape reabasteceu");
    }

    [Test]
    public void Cacamba_SemPneuSemPicape_Recusa()
    {
        Cacamba cacamba = NovaCacamba(2, out _);
        Assert.IsTrue(kit.GuardarGarrafa(), "nem com garrafas no saco");

        Assert.IsFalse(cacamba.PodeUsar(kit, out string motivo));
        Assert.AreEqual("Só pneu vai na caçamba da picape.", motivo);

        cacamba.Usar(kit);
        Assert.AreEqual(1, kit.Estoque.GarrafasNoSaco, "o saco não esvazia na caçamba");
    }

    [Test]
    public void Cacamba_TextoPara()
    {
        Cacamba cacamba = NovaCacamba(2, out _);
        PontoDeReabastecimento picape = TesteUtil.Com<PontoDeReabastecimento>("Picape (teste)");
        TesteUtil.Setar(picape, "textoAoMirar", "Picape da UPA");
        TesteUtil.Setar(cacamba, "picape", picape);

        Assert.AreEqual("Picape da UPA", cacamba.TextoPara(kit), "sem pneu: o texto da picape");

        PneuNasMaos();
        Assert.AreEqual(TextoJogarPneu, cacamba.TextoPara(kit), "com pneu");
    }

    /* Decisão do controlador: a cabine da picape (o PontoDeReabastecimento)
     * com uma Cacamba filha, mirada com o pneu nas mãos, entrega nela. Sem
     * Cacamba filha (o monte de areia), continua recusando. */
    [Test]
    public void Picape_ComPneu_DelegaACacambaFilha()
    {
        PontoDeReabastecimento picape = TesteUtil.Com<PontoDeReabastecimento>("Picape (teste)");
        Cacamba cacamba = NovaCacamba(1, out List<Transform> vagas);
        cacamba.transform.SetParent(picape.transform, false);
        TesteUtil.Setar(cacamba, "picape", picape);
        PontoDeReabastecimento monte = TesteUtil.Com<PontoDeReabastecimento>("Monte de areia (teste)");
        PneuInteractable pneu = PneuNasMaos();

        Assert.IsFalse(monte.PodeUsar(kit, out string motivoMonte), "sem caçamba filha");
        Assert.AreEqual("Largue o pneu antes.", motivoMonte);

        Assert.AreEqual(TextoJogarPneu, picape.TextoPara(kit));
        Assert.IsTrue(picape.PodeUsar(kit, out string motivo), $"a cabine entrega na caçamba (motivo: {motivo})");
        picape.Usar(kit);

        Assert.IsTrue(pneu.IsComplete);
        Assert.AreSame(vagas[0], pneu.VagaNaCacamba);
        Assert.IsNull(kit.PneuCarregado);
    }

    /* A cabine com o pneu nas mãos entrega na caçamba: segurar leva o tempo
     * dela, não o do reabastecimento. Sem pneu, o tempo da própria picape. */
    [Test]
    public void Picape_ComPneu_SegundosDaCacamba()
    {
        PontoDeReabastecimento picape = TesteUtil.Com<PontoDeReabastecimento>("Picape (teste)");
        TesteUtil.Setar(picape, "segundosParaUsar", 1.5f);
        Cacamba cacamba = NovaCacamba(1, out _);
        cacamba.transform.SetParent(picape.transform, false);
        TesteUtil.Setar(cacamba, "segundosParaUsar", 0.4f);

        Assert.AreEqual(1.5f, picape.SegundosParaUsarPara(kit), 1e-5f, "sem pneu, reabastece");
        Assert.AreEqual(0.4f, cacamba.SegundosParaUsarPara(kit), 1e-5f, "a caçamba, o dela");

        PneuNasMaos();
        Assert.AreEqual(0.4f, picape.SegundosParaUsarPara(kit), 1e-5f, "com pneu, é a caçamba que atende");
        Assert.AreEqual(1.5f, picape.SegundosParaUsar, 1e-5f, "o campo da picape não muda");
    }

    /* Mirando a carroceria sem pneu, a caçamba é a própria picape: segurar
     * leva o tempo dela, o mesmo de mirar na cabine. */
    [Test]
    public void Cacamba_SemPneu_SegundosDaPicape()
    {
        PontoDeReabastecimento picape = TesteUtil.Com<PontoDeReabastecimento>("Picape (teste)");
        TesteUtil.Setar(picape, "segundosParaUsar", 1.5f);
        Cacamba cacamba = NovaCacamba(1, out _);
        TesteUtil.Setar(cacamba, "segundosParaUsar", 0.4f);
        TesteUtil.Setar(cacamba, "picape", picape);

        Assert.AreEqual(1.5f, cacamba.SegundosParaUsarPara(kit), 1e-5f, "sem pneu, o tempo de reabastecer");

        PneuNasMaos();
        Assert.AreEqual(0.4f, cacamba.SegundosParaUsarPara(kit), 1e-5f, "com pneu, o da caçamba");
    }

    /* ---------------- Lixeira ---------------- */

    [Test]
    public void Lixeira_EsvaziaSaco()
    {
        Lixeira lixeira = TesteUtil.Com<Lixeira>("Lixeira (teste)");
        int esvaziados = 0;
        kit.OnSacoEsvaziado += () => esvaziados++;
        for (int i = 0; i < 3; i++)
            Assert.IsTrue(kit.GuardarGarrafa(), $"garrafa {i + 1}");

        Assert.IsTrue(lixeira.PodeUsar(kit, out string motivo), $"saco com garrafas (motivo: {motivo})");
        lixeira.Usar(kit);

        Assert.AreEqual(0, kit.Estoque.GarrafasNoSaco);
        Assert.AreEqual(1, esvaziados, "OnSacoEsvaziado");
    }

    [Test]
    public void Lixeira_ComPneu_Recusa()
    {
        Lixeira lixeira = TesteUtil.Com<Lixeira>("Lixeira (teste)");
        Assert.IsTrue(kit.GuardarGarrafa(), "mesmo com garrafas, o pneu vem antes");
        PneuNasMaos();

        Assert.IsFalse(lixeira.PodeUsar(kit, out string motivo));
        Assert.AreEqual("Pneu vai na caçamba da <b>picape da UPA</b>.", motivo);
    }

    /* A HUD acrescenta a linha "segure o clique": o texto da lixeira não
     * repete o "segure". */
    [Test]
    public void Lixeira_Balanco_UmaOscilacaoDeSeisGraus()
    {
        Assert.AreEqual(6f, Lixeira.AnguloDoBalanco, 1e-4f);
        Assert.AreEqual(0f, Lixeira.AnguloDoBalancoEm(0f), 1e-4f, "começa parada");
        Assert.AreEqual(6f, Lixeira.AnguloDoBalancoEm(0.25f), 1e-4f, "um lado");
        Assert.AreEqual(-6f, Lixeira.AnguloDoBalancoEm(0.75f), 1e-4f, "o outro lado");
        Assert.AreEqual(0f, Lixeira.AnguloDoBalancoEm(1f), 1e-4f, "termina parada");
    }

    [Test]
    public void Lixeira_Balanco_InclinaEVoltaAoGiroOriginal()
    {
        Lixeira lixeira = TesteUtil.Com<Lixeira>("Lixeira (teste)");
        // Giro de malha importada: o eixo Z local é o vertical do mundo.
        Quaternion original = Quaternion.Euler(-90f, 30f, 0f);
        lixeira.transform.localRotation = original;

        lixeira.AplicarBalanco(0.25f);
        Assert.AreEqual(6f, Quaternion.Angle(original, lixeira.transform.localRotation), 1e-2f, "6° do giro original");
        Assert.AreEqual(6f, Vector3.Angle(original * Vector3.forward, lixeira.transform.forward), 1e-2f,
            "inclina de verdade: o eixo vertical da malha sai do prumo");

        lixeira.AplicarBalanco(1f);
        Assert.AreEqual(0f, Quaternion.Angle(original, lixeira.transform.localRotation), 1e-2f, "volta ao giro original");
    }

    [Test]
    public void Lixeira_DesligadaNoMeioDoBalanco_Endireita()
    {
        Lixeira lixeira = TesteUtil.Com<Lixeira>("Lixeira (teste)");
        Quaternion original = Quaternion.Euler(0f, 40f, 0f);
        lixeira.transform.localRotation = original;
        lixeira.AplicarBalanco(0.25f);

        // O OnDisable não é chamado no EditMode; por reflexão, como no Play.
        typeof(Lixeira).GetMethod("OnDisable", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(lixeira, null);

        Assert.AreEqual(0f, Quaternion.Angle(original, lixeira.transform.localRotation), 1e-2f);
    }

    [Test]
    public void Lixeira_TextoPara()
    {
        Lixeira lixeira = TesteUtil.Com<Lixeira>("Lixeira (teste)");
        TesteUtil.Setar(lixeira, "textoAoMirar", "Lixeira");

        Assert.AreEqual("Esvaziar o saco na <b>lixeira</b>", lixeira.TextoPara(kit));
    }

    [Test]
    public void Lixeira_SacoVazio_Recusa()
    {
        Lixeira lixeira = TesteUtil.Com<Lixeira>("Lixeira (teste)");

        Assert.IsFalse(lixeira.PodeUsar(null, out string semKit), "sem kit");
        Assert.AreEqual(string.Empty, semKit, "sem kit, recusa sem aviso");

        Assert.IsFalse(lixeira.PodeUsar(kit, out string motivo));
        Assert.AreEqual("O saco está vazio.", motivo);
    }

    /* ---------------- Largar e carregar ---------------- */

    /* A picape de teste com o collider na raiz (a cabine) e a Cacamba num
     * filho sem collider: 'altura' 1,4 m é pega pelo raio do peito, 0,8 m
     * pelo raio do chão. Devolve o pneu já nos braços, de frente para ela. */
    private PneuInteractable PneuDeFrenteParaACabine(float altura, bool comCacamba, Vector3 pes)
    {
        BoxCollider chao = TesteUtil.Com<BoxCollider>("Chão (teste)");
        chao.size = new Vector3(20f, 0.2f, 20f);
        chao.transform.position = pes + Vector3.down * 0.1f;

        BoxCollider cabine = TesteUtil.Com<BoxCollider>("Cabine da picape (teste)");
        cabine.center = new Vector3(0f, altura / 2f, 0f);
        cabine.size = new Vector3(2.4f, altura, 1.4f);
        cabine.transform.position = pes + Vector3.forward * 0.9f;
        cabine.gameObject.AddComponent<PontoDeReabastecimento>();

        if (comCacamba)
        {
            Cacamba cacamba = NovaCacamba(1, out _);
            cacamba.transform.SetParent(cabine.transform, false);
            cacamba.transform.localPosition = Vector3.back * 3f;
        }

        PneuInteractable pneu = NovoPneu();
        pneu.transform.position = pes + Vector3.back * 2f + Vector3.up * 0.7f;
        Physics.SyncTransforms();
        Assert.IsTrue(kit.Pegar(pneu), "pneu nos braços");
        return pneu;
    }

    /* Revisão da Task 9: largar contra a CABINE da picape (e não contra a
     * carroceria) também entrega, como a mira na cabine com o pneu nas mãos. */
    [TestCase(1.4f)]
    [TestCase(0.8f)]
    public void Largar_ContraACabine_Entrega(float altura)
    {
        Vector3 pes = SemChao;
        PneuInteractable pneu = PneuDeFrenteParaACabine(altura, true, pes);

        Assert.IsTrue(kit.LargarPneu(pes, Vector3.forward));

        Assert.IsTrue(pneu.IsComplete, "largado na cabine, conta como entregue");
        Assert.IsNull(kit.PneuCarregado, "mãos livres");
        Assert.IsNotNull(pneu.VagaNaCacamba, "foi para a vaga da caçamba filha");
    }

    /* O monte de areia também é PontoDeReabastecimento, mas sem Cacamba
     * filha: o pneu só é largado ali. */
    [TestCase(1.4f)]
    [TestCase(0.8f)]
    public void Largar_ContraOMonteDeAreia_NaoEntrega(float altura)
    {
        Vector3 pes = SemChao;
        PneuInteractable pneu = PneuDeFrenteParaACabine(altura, false, pes);

        Assert.IsTrue(kit.LargarPneu(pes, Vector3.forward));

        Assert.IsFalse(pneu.IsComplete, "só largou");
        Assert.IsNull(kit.PneuCarregado, "mãos livres");
        Assert.IsNull(pneu.VagaNaCacamba);
    }

    /* Como o teste M da Parte A: o agente encosta na caçamba e larga o pneu
     * (botão direito). É entrega, e o foco conclui. */
    [Test]
    public void Largar_SobreCacamba_Entrega()
    {
        Vector3 pes = SemChao;
        BoxCollider chao = TesteUtil.Com<BoxCollider>("Chão (teste)");
        chao.size = new Vector3(20f, 0.2f, 20f);
        chao.transform.position = pes + Vector3.down * 0.1f;

        BoxCollider caixa = TesteUtil.Com<BoxCollider>("Caçamba (teste)");
        caixa.center = new Vector3(0f, 0.6f, 0f);
        caixa.size = new Vector3(2.4f, 1.2f, 1.4f);
        caixa.transform.position = pes + Vector3.forward * 0.9f;
        caixa.gameObject.AddComponent<Cacamba>();

        PneuInteractable pneu = NovoPneu();
        pneu.transform.position = pes + Vector3.back * 2f + Vector3.up * 0.7f;
        Physics.SyncTransforms();
        Assert.IsTrue(kit.Pegar(pneu), "pneu nos braços");

        Assert.IsTrue(kit.LargarPneu(pes, Vector3.forward));

        Assert.IsTrue(pneu.IsComplete, "largado na caçamba, conta como entregue");
        Assert.IsNull(kit.PneuCarregado, "mãos livres");
    }

    [Test]
    public void Kit_MultiplicadorCarregando_085()
    {
        PneuNasMaos();

        Assert.AreEqual(0.85f, jogador.MultiplicadorVelocidade, 1e-5f);
        Assert.IsFalse(jogador.PodePular);
    }

    /* Decisão do controlador: a água do pneu sumia no 3º de 5 passos, e o
     * pneu parecia vazio mas contava como cheio (zumbia, a chuva não o
     * reenchia). Agora ela só some quando o pneu fica Esvaziado. */
    [Test]
    public void Pneu_AguaSoSomeAoEsvaziar()
    {
        PneuInteractable pneu = NovoPneu();
        TesteUtil.Setar(pneu, "requiredClicks", 5);
        GameObject agua = TesteUtil.Novo("agua do pneu (teste)");
        agua.transform.SetParent(pneu.transform, false);
        TesteUtil.Setar(pneu, "water", agua);

        for (int i = 1; i <= 4; i++)
        {
            Assert.AreEqual(ResultadoDoPasso.Avancou, pneu.AvancarCom(kit), $"{i}º passo");
            Assert.IsTrue(agua.activeSelf, $"{i}º passo: ainda com água");
            Assert.IsFalse(pneu.Esvaziado);
        }

        Assert.AreEqual(ResultadoDoPasso.Esvaziou, pneu.AvancarCom(kit), "5º passo");
        Assert.IsFalse(agua.activeSelf, "esvaziado, sem água");
    }
}
