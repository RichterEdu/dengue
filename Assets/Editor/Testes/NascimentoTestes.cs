using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

/* ============================================================
 *  NascimentoTestes.cs
 *  ---------------------------------------------------------------------
 *  Testes EditMode (Parte B) do nascimento seco: em Play, o sorteio do
 *  começo (GerenciadorDeFocos.SortearNascimento) define uma contagem
 *  FIXA de focos que nascem com água; o resto nasce seco (NascerVazio),
 *  como se já tivesse sido esvaziado antes do agente chegar — sem o
 *  OnEsvaziado, sem zumbido, pesando 0, as Mãos dão JaVazio, a
 *  ferramenta certa resolve de vez e a chuva (ReencherEsvaziados)
 *  devolve a água de cada tipo: pneu, vaso, garrafas, bromélia, caixa
 *  d'água e piscina. O ralo fica de fora: nasce sempre com água.
 *
 *  O Start não roda no EditMode e, fora do Play, ele não sorteia o
 *  nascimento (todo foco nasce cheio, como antes): os testes ficam
 *  determinísticos. O sorteio é chamado direto, com System.Random de
 *  semente fixa; o Start por reflexão só onde o foco precisa dele
 *  (garrafas: juntar o grupo; piscina: guardar a escala da água).
 *
 *  Os focos de teste não têm punch (punchScale 0) nem animação de
 *  conclusão (shrinkDuration 0). O kit fica num objeto com
 *  ControladorJogador e começa com as Mãos selecionadas.
 *
 *  Os objetos nascem pelo TesteUtil (escondidos, fora da cena) e são
 *  destruídos no [TearDown].
 *
 *  Rodar:  Window > General > Test Runner > EditMode (categoria ParteB)
 *  ou      unity command run_tests --mode EditMode --filter_type category --filter ParteB
 * ============================================================ */

[Category("ParteB")]
public class NascimentoTestes
{
    // Bem abaixo de qualquer cena do jogo: aqui o raycast não acha chão.
    private static readonly Vector3 SemChao = new Vector3(0f, -10000f, 0f);

    private GerenciadorDeFocos gerenciador;
    private KitDoAgente kit;

    [SetUp]
    public void Preparar()
    {
        gerenciador = TesteUtil.Com<GerenciadorDeFocos>("Gerenciador (teste)");

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

    /* Vaso que pede Areia, com o atalho de esvaziar e 3 cliques. */
    private static VasoInteractable NovoVaso()
    {
        VasoInteractable vaso = TesteUtil.Com<VasoInteractable>("Vaso (teste)");
        TesteUtil.Setar(vaso, "ferramentaNecessaria", Ferramenta.Areia);
        TesteUtil.Setar(vaso, "podeEsvaziar", true);
        TesteUtil.Setar(vaso, "requiredClicks", 3);
        TesteUtil.Setar(vaso, "punchScale", 0f);
        TesteUtil.Setar(vaso, "shrinkDuration", 0f);
        return vaso;
    }

    /* O Start não roda no EditMode; chamado por reflexão quando o foco
     * precisa dele (garrafas: juntar o grupo; piscina: guardar a escala
     * da água). */
    private static void RodarStart(Interactable foco)
    {
        MethodInfo start = typeof(Interactable).GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(start, "Interactable.Start não encontrado");
        start.Invoke(foco, null);
    }

    /* Grupo de 3 garrafas de verdade (filhas da raiz); a primeira tem água. */
    private static GarrafaInteractable NovasGarrafas(out GameObject agua)
    {
        GarrafaInteractable grupo = TesteUtil.Com<GarrafaInteractable>("Garrafas (teste)");
        TesteUtil.Setar(grupo, "ferramentaNecessaria", Ferramenta.SacoDeLixo);
        TesteUtil.Setar(grupo, "podeEsvaziar", true);
        TesteUtil.Setar(grupo, "punchScale", 0f);
        TesteUtil.Setar(grupo, "shrinkDuration", 0f);

        agua = null;
        for (int i = 0; i < 3; i++)
        {
            GameObject garrafa = TesteUtil.Novo("Garrafa " + i + " (teste)");
            garrafa.AddComponent<MeshRenderer>();
            garrafa.transform.SetParent(grupo.transform, false);

            if (i == 0)
            {
                agua = TesteUtil.Novo("agua da garrafa (teste)");
                agua.AddComponent<MeshRenderer>();
                agua.transform.SetParent(garrafa.transform, false);
            }
        }

        RodarStart(grupo);
        return grupo;
    }

    private int ContarComAgua(List<Interactable> focos)
    {
        int n = 0;
        foreach (Interactable foco in focos)
            if (gerenciador.NasceComAgua(foco))
                n++;
        return n;
    }

    /* ---------------- A regra da contagem ---------------- */

    [Test]
    public void ComAguaNoNascimento_ContagemFixa()
    {
        Assert.AreEqual(2, gerenciador.ComAguaNoNascimento(18), "round(0,1 × 18)");
        Assert.AreEqual(1, gerenciador.ComAguaNoNascimento(10), "no mínimo 1");
        Assert.AreEqual(1, gerenciador.ComAguaNoNascimento(1));
        Assert.AreEqual(0, gerenciador.ComAguaNoNascimento(0));

        TesteUtil.Setar(gerenciador, "fracaoNascendoComAgua", 0f);
        Assert.AreEqual(0, gerenciador.ComAguaNoNascimento(18), "fração 0: ninguém nasce com água");

        TesteUtil.Setar(gerenciador, "fracaoNascendoComAgua", 0.5f);
        Assert.AreEqual(9, gerenciador.ComAguaNoNascimento(18));

        TesteUtil.Setar(gerenciador, "fracaoNascendoComAgua", 1f);
        Assert.AreEqual(18, gerenciador.ComAguaNoNascimento(18), "no máximo o total");
    }

    [Test]
    public void SortearNascimento_ContagemFixa_SorteiaQuem()
    {
        TesteUtil.Setar(gerenciador, "fracaoNascendoComAgua", 0.2f);
        List<Interactable> focos = NovosFocos(10);
        gerenciador.Registrar(focos);

        gerenciador.SortearNascimento(new System.Random(7));
        Assert.AreEqual(2, ContarComAgua(focos), "2 de 10 com água, quem for sorteado");

        bool[] respostas = new bool[10];
        for (int i = 0; i < 10; i++)
            respostas[i] = gerenciador.NasceComAgua(focos[i]);

        gerenciador.SortearNascimento(new System.Random(7));
        for (int i = 0; i < 10; i++)
            Assert.AreEqual(respostas[i], gerenciador.NasceComAgua(focos[i]), "mesma semente, mesmos secos");

        gerenciador.SortearNascimento(new System.Random(8));
        Assert.AreEqual(2, ContarComAgua(focos), "outra semente: a contagem é a mesma");
    }

    [Test]
    public void SortearNascimento_FracaoZero_TodosSecos()
    {
        TesteUtil.Setar(gerenciador, "fracaoNascendoComAgua", 0f);
        List<Interactable> focos = NovosFocos(3);
        gerenciador.Registrar(focos);

        gerenciador.SortearNascimento(new System.Random(1));

        Assert.AreEqual(0, ContarComAgua(focos));
    }

    /* Foco registrado depois do sorteio (só o da cena entra nele). */
    [Test]
    public void FocoForaDoSorteio_NasceComAgua()
    {
        TesteUtil.Setar(gerenciador, "fracaoNascendoComAgua", 0f);
        gerenciador.Registrar(NovosFocos(3));
        gerenciador.SortearNascimento(new System.Random(1));

        Interactable novo = NovoFoco("Foco novo (teste)");

        Assert.IsTrue(gerenciador.NasceComAgua(novo), "nasce com água, como sempre nasceu");
    }

    /* ---------------- O foco nascido seco ---------------- */

    [Test]
    public void NascerVazio_FicaEsvaziado_SemEvento_NaoZumbe()
    {
        VasoInteractable vaso = NovoVaso();
        gerenciador.Registrar(new[] { vaso });
        int esvaziados = 0;
        vaso.OnEsvaziado += f => esvaziados++;

        vaso.NascerVazio();

        Assert.IsTrue(vaso.Esvaziado);
        Assert.AreEqual(0, esvaziados, "nascer seco não é esvaziar: sem OnEsvaziado");
        Assert.IsFalse(vaso.Zumbe, "sem água, sem zumbido");
        Assert.IsTrue(vaso.CanInteract, "continua alvo");
        Assert.AreEqual(ResultadoDoPasso.JaVazio, vaso.Verificar(kit), "as Mãos: já está seco");
        Assert.AreEqual(0f, gerenciador.PesoDe(vaso), 1e-5f, "nasce parado, pesando 0");
    }

    [Test]
    public void NascerVazio_AguaSome_E_AChuvaDevolve()
    {
        VasoInteractable vaso = NovoVaso();
        GameObject agua = TesteUtil.Novo("agua (teste)");
        agua.transform.SetParent(vaso.transform, false);
        agua.transform.localScale = new Vector3(1f, 0.6f, 1f);
        TesteUtil.Setar(vaso, "water", agua);
        gerenciador.Registrar(new[] { vaso });

        vaso.NascerVazio();
        Assert.IsFalse(agua.activeSelf, "nasceu seco");
        Assert.AreEqual(EstagioDoFoco.Ovo, gerenciador.EstagioDe(vaso));

        Assert.AreEqual(1, gerenciador.ReencherEsvaziados());

        Assert.IsFalse(vaso.Esvaziado);
        Assert.IsTrue(agua.activeSelf, "a chuva devolve a água");
        Assert.AreEqual(new Vector3(1f, 0.6f, 1f), agua.transform.localScale, "na altura original");
        Assert.AreEqual(EstagioDoFoco.Larva, gerenciador.EstagioDe(vaso), "como todo reenchido");
    }

    [Test]
    public void NascerVazio_FerramentaCertaResolveDeVez()
    {
        VasoInteractable vaso = NovoVaso();
        vaso.NascerVazio();
        kit.Selecionar(Ferramenta.Areia);
        int concluidos = 0;
        vaso.OnCompleted += f => concluidos++;

        Assert.AreEqual(ResultadoDoPasso.Avancou, vaso.AvancarCom(kit), "1º passo de areia");
        Assert.AreEqual(ResultadoDoPasso.Avancou, vaso.AvancarCom(kit), "2º passo de areia");
        Assert.AreEqual(ResultadoDoPasso.Concluiu, vaso.AvancarCom(kit), "3º passo de areia");

        Assert.IsTrue(vaso.IsComplete);
        Assert.AreEqual(1, concluidos);
    }

    [Test]
    public void Pneu_NasceSeco_PegaNaHoraENosBracosNaoReenche()
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

        pneu.NascerVazio();

        Assert.IsFalse(agua.activeSelf, "nasceu seco");
        Assert.IsTrue(pneu.CanInteract, "é alvo");
        Assert.IsFalse(pneu.Zumbe);
        Assert.AreEqual(0f, pneu.SegundosPorPasso, 1e-5f, "pegar é na hora");

        Assert.AreEqual(ResultadoDoPasso.Pegou, pneu.AvancarCom(kit));
        Assert.IsTrue(pneu.Carregado);

        Assert.AreEqual(0, gerenciador.ReencherEsvaziados(), "nos braços, a chuva não reenche");
        Assert.IsFalse(agua.activeSelf);
    }

    /* Fora do Play o Start não sorteia: os testes EditMode ficam
     * determinísticos (todo foco nasce cheio). */
    [Test]
    public void StartForaDoPlay_NaoSorteia()
    {
        VasoInteractable vaso = NovoVaso();
        gerenciador.Registrar(new[] { vaso });
        TesteUtil.Setar(gerenciador, "fracaoNascendoComAgua", 0f);
        gerenciador.SortearNascimento(new System.Random(1));

        RodarStart(vaso);

        Assert.IsFalse(vaso.Esvaziado, "fora do Play todo foco nasce cheio");
    }

    /* ---------------- Cada tipo, o visual de nascer seco ---------------- */

    [Test]
    public void Garrafas_NascemSecas_E_AChuvaDevolve()
    {
        GarrafaInteractable grupo = NovasGarrafas(out GameObject agua);
        gerenciador.Registrar(new[] { grupo });

        grupo.NascerVazio();

        Assert.IsTrue(grupo.Esvaziado);
        Assert.AreEqual(3, grupo.GarrafasRestantes, "as garrafas ficam");
        Assert.IsFalse(agua.activeSelf, "nasceu sem água");

        Assert.AreEqual(1, gerenciador.ReencherEsvaziados());
        Assert.IsTrue(agua.activeSelf, "a chuva devolve a água das garrafas");
    }

    [Test]
    public void Bromelia_NasceSeca_E_AChuvaDevolveAsPocas()
    {
        BromeliaInteractable bromelia = TesteUtil.Com<BromeliaInteractable>("Bromélia (teste)");
        TesteUtil.Setar(bromelia, "ferramentaNecessaria", Ferramenta.Areia);
        TesteUtil.Setar(bromelia, "requiredClicks", 2);
        TesteUtil.Setar(bromelia, "punchScale", 0f);
        TesteUtil.Setar(bromelia, "shrinkDuration", 0f);
        GameObject[] pocas = new GameObject[3];
        for (int i = 0; i < 3; i++)
            pocas[i] = TesteUtil.Novo("Poça " + i + " (teste)");
        typeof(BromeliaInteractable).GetField("pocas", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(bromelia, pocas);
        gerenciador.Registrar(new[] { bromelia });

        bromelia.NascerVazio();

        Assert.IsTrue(bromelia.Esvaziado);
        foreach (GameObject p in pocas)
            Assert.IsFalse(p.activeSelf, "nasceu sem poça");

        Assert.AreEqual(1, gerenciador.ReencherEsvaziados());
        foreach (GameObject p in pocas)
            Assert.IsTrue(p.activeSelf, "a chuva devolve as poças");
    }

    /* Secar pela metade: a chuva não enche por cima (o passo da areia num
     * born-dry não toca nas poças: o contador já está no fim). */
    [Test]
    public void Bromelia_SecarComecado_NaoReenche()
    {
        BromeliaInteractable bromelia = TesteUtil.Com<BromeliaInteractable>("Bromélia (teste)");
        TesteUtil.Setar(bromelia, "ferramentaNecessaria", Ferramenta.Areia);
        TesteUtil.Setar(bromelia, "requiredClicks", 2);
        TesteUtil.Setar(bromelia, "punchScale", 0f);
        TesteUtil.Setar(bromelia, "shrinkDuration", 0f);
        GameObject[] pocas = new GameObject[3];
        for (int i = 0; i < 3; i++)
            pocas[i] = TesteUtil.Novo("Poça " + i + " (teste)");
        typeof(BromeliaInteractable).GetField("pocas", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(bromelia, pocas);
        gerenciador.Registrar(new[] { bromelia });

        bromelia.NascerVazio();
        kit.Selecionar(Ferramenta.Areia);
        Assert.AreEqual(ResultadoDoPasso.Avancou, bromelia.AvancarCom(kit), "1º passo da areia");
        Assert.AreEqual(1, bromelia.CurrentClicks);

        Assert.IsFalse(bromelia.PodeReencher, "areia pela metade, a chuva não reenche");
        Assert.AreEqual(0, gerenciador.ReencherEsvaziados());
        foreach (GameObject p in pocas)
            Assert.IsFalse(p.activeSelf, "as poças continuam escondidas");
    }

    [Test]
    public void CaixaDagua_NasceSeca_E_AChuvaDevolve()
    {
        CaixaDaguaInteractable caixa = TesteUtil.Com<CaixaDaguaInteractable>("Caixa d'água (teste)");
        TesteUtil.Setar(caixa, "ferramentaNecessaria", Ferramenta.Areia);
        TesteUtil.Setar(caixa, "requiredClicks", 2);
        TesteUtil.Setar(caixa, "punchScale", 0f);
        TesteUtil.Setar(caixa, "shrinkDuration", 0f);
        GameObject agua = TesteUtil.Novo("agua (teste)");
        agua.transform.SetParent(caixa.transform, false);
        TesteUtil.Setar(caixa, "agua", agua);
        gerenciador.Registrar(new[] { caixa });

        caixa.NascerVazio();
        Assert.IsFalse(agua.activeSelf, "nasceu sem água");

        Assert.AreEqual(1, gerenciador.ReencherEsvaziados());
        Assert.IsTrue(agua.activeSelf, "a chuva devolve a água da caixa aberta");

        // Fechamento pela metade: a chuva não enche (a tampa nem existe no
        // foco de teste: só o CurrentClicks importa).
        kit.Selecionar(Ferramenta.Areia);
        Assert.AreEqual(ResultadoDoPasso.Avancou, caixa.AvancarCom(kit), "1º passo da areia");

        Assert.IsFalse(caixa.PodeReencher);
        Assert.AreEqual(0, gerenciador.ReencherEsvaziados());
    }

    [Test]
    public void Piscina_NasceSeca_E_AChuvaDevolve()
    {
        PiscinaInteractable piscina = TesteUtil.Com<PiscinaInteractable>("Piscina (teste)");
        TesteUtil.Setar(piscina, "ferramentaNecessaria", Ferramenta.Areia);
        TesteUtil.Setar(piscina, "requiredClicks", 2);
        TesteUtil.Setar(piscina, "punchScale", 0f);
        TesteUtil.Setar(piscina, "shrinkDuration", 0f);
        TesteUtil.Setar(piscina, "duracaoDoMovimento", 0f);
        GameObject agua = TesteUtil.Novo("agua (teste)");
        agua.transform.SetParent(piscina.transform, false);
        agua.transform.localScale = new Vector3(1f, 0.3f, 1f);
        TesteUtil.Setar(piscina, "agua", agua);
        RodarStart(piscina);    // guarda a escala da água (OnStart)
        gerenciador.Registrar(new[] { piscina });

        piscina.NascerVazio();
        Assert.IsFalse(agua.activeSelf, "nasceu sem água");

        Assert.AreEqual(1, gerenciador.ReencherEsvaziados());
        Assert.IsTrue(agua.activeSelf, "a chuva devolve a água");
        Assert.AreEqual(new Vector3(1f, 0.3f, 1f), agua.transform.localScale, "na escala original");
    }

    /* O ralo nasce sempre com água: fica fora do sorteio do começo. */
    [Test]
    public void Ralo_NasceSempreComAgua_ForaDoSorteio()
    {
        List<Interactable> focos = NovosFocos(3);
        RaloInteractable ralo = TesteUtil.Com<RaloInteractable>("Ralo (teste)");
        gerenciador.Registrar(new List<Interactable>(focos) { ralo });

        // Fração 0: todo foco comum nasce seco, menos o ralo.
        TesteUtil.Setar(gerenciador, "fracaoNascendoComAgua", 0f);
        gerenciador.SortearNascimento(new System.Random(1));
        Assert.AreEqual(0, ContarComAgua(focos), "os comuns nascem secos");
        Assert.IsTrue(ralo.SempreNasceComAgua);
        Assert.IsTrue(gerenciador.NasceComAgua(ralo), "o ralo fica sempre com água");

        // Fração 0,4: só os 3 comuns entram na conta, round(0,4 × 3) = 1.
        // Se o ralo entrasse no pool, seriam round(0,4 × 4) = 2.
        TesteUtil.Setar(gerenciador, "fracaoNascendoComAgua", 0.4f);
        gerenciador.SortearNascimento(new System.Random(2));
        Assert.AreEqual(1, ContarComAgua(focos), "o sorteio conta só quem pode nascer seco");
    }
}
