using System.Reflection;
using NUnit.Framework;
using UnityEngine;

/* ============================================================
 *  EsvaziarTestes.cs
 *  ---------------------------------------------------------------------
 *  Testes EditMode (Parte B) do atalho de esvaziar com as Mãos: no vaso
 *  e nas garrafas (podeEsvaziar), segurar com as Mãos tira a água em
 *  passosParaEsvaziar passos, sem concluir o foco (nada de OnCompleted
 *  nem de clique da ação definitiva). O pneu não usa o atalho: esvazia
 *  pelo caminho normal (PneuEsvaziarTestes). Esvaziado, o foco não
 *  zumbe e as Mãos dão JaVazio; a ferramenta certa ainda resolve de vez,
 *  e o Reencher (a chuva) devolve a água. Com a ação definitiva começada
 *  (currentClicks > 0) o atalho não vale. O primeiroClique do som ("tira
 *  a água...") não toca num foco já esvaziado, e a mira das garrafas
 *  esvaziadas tem texto próprio, sem o "juntando água".
 *
 *  O Start não roda no EditMode e as corrotinas não andam. Por isso os
 *  focos de teste não têm punch (punchScale 0) nem animação de conclusão
 *  (shrinkDuration 0: o OnComplete só desativa o objeto). O vaso de
 *  teste pede Areia, tem podeEsvaziar e 3 cliques. As garrafas de teste
 *  têm garrafas de verdade (filhos com MeshRenderer, um deles com água)
 *  e o Start é chamado por reflexão para o grupo juntá-las.
 *
 *  O kit fica num objeto com ControladorJogador, usa o estoque padrão
 *  (areia 1) e começa com as Mãos selecionadas.
 *
 *  Os objetos nascem pelo TesteUtil (escondidos, fora da cena) e são
 *  destruídos no [TearDown].
 *
 *  Rodar:  Window > General > Test Runner > EditMode (categoria ParteB)
 *  ou      unity command run_tests --mode EditMode --filter_type category --filter ParteB
 * ============================================================ */

[Category("ParteB")]
public class EsvaziarTestes
{
    private KitDoAgente kit;

    [SetUp]
    public void Preparar()
    {
        ControladorJogador jogador = TesteUtil.Com<ControladorJogador>("Jogador (teste)");
        kit = jogador.gameObject.AddComponent<KitDoAgente>();
    }

    [TearDown]
    public void Limpar()
    {
        TesteUtil.LimparTudo();
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

    /* Os 3 passos de esvaziar com as Mãos (o kit começa nelas). */
    private void Esvaziar(Interactable foco)
    {
        for (int i = 0; i < 3; i++)
            foco.AvancarCom(kit);
    }

    /* O Start não roda no EditMode; chamado por reflexão, o grupo de
     * garrafas junta as garrafas (OnStart) como no Play. */
    private static void RodarStart(Interactable foco)
    {
        MethodInfo start = typeof(Interactable).GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(start, "Interactable.Start não encontrado");
        start.Invoke(foco, null);
    }

    [Test]
    public void Maos_EsvaziaEmTresPassos()
    {
        VasoInteractable vaso = NovoVaso();
        int esvaziados = 0, concluidos = 0, clicados = 0;
        vaso.OnEsvaziado += f => esvaziados++;
        vaso.OnCompleted += f => concluidos++;
        vaso.OnClicado += f => clicados++;

        Assert.AreEqual(ResultadoDoPasso.Avancou, vaso.AvancarCom(kit), "1º passo");
        Assert.AreEqual(1, vaso.PassosEsvaziando);
        Assert.AreEqual(ResultadoDoPasso.Avancou, vaso.AvancarCom(kit), "2º passo");
        Assert.AreEqual(2, vaso.PassosEsvaziando);
        Assert.AreEqual(ResultadoDoPasso.Esvaziou, vaso.AvancarCom(kit), "3º passo");

        Assert.IsTrue(vaso.Esvaziado);
        Assert.AreEqual(0, vaso.PassosEsvaziando, "esvaziado, os passos zeram");
        Assert.AreEqual(1, esvaziados, "OnEsvaziado 1 vez");
        Assert.AreEqual(0, concluidos, "esvaziar não conclui");
        Assert.AreEqual(0, clicados, "esvaziar não é passo da ação definitiva (som do tipo)");
        Assert.IsFalse(vaso.IsComplete);
        Assert.AreEqual(0, vaso.CurrentClicks, "os cliques da areia não andam");
        Assert.IsTrue(vaso.CanInteract, "continua alvo: a areia ainda resolve");
    }

    [Test]
    public void Esvaziado_Maos_JaVazio()
    {
        VasoInteractable vaso = NovoVaso();
        Esvaziar(vaso);
        int esvaziados = 0;
        vaso.OnEsvaziado += f => esvaziados++;

        Assert.AreEqual(ResultadoDoPasso.JaVazio, vaso.Verificar(kit));
        Assert.AreEqual(ResultadoDoPasso.JaVazio, vaso.AvancarCom(kit));
        Assert.AreEqual(0, vaso.PassosEsvaziando, "recusado, nada anda");
        Assert.AreEqual(0, esvaziados, "não esvazia de novo");

        Assert.IsTrue(Ferramentas.EhRecusa(ResultadoDoPasso.JaVazio), "JaVazio é recusa que a HUD explica");
        Assert.IsFalse(Ferramentas.EhRecusa(ResultadoDoPasso.Esvaziou), "Esvaziou não é recusa");
    }

    [Test]
    public void Esvaziado_NaoZumbe()
    {
        VasoInteractable vaso = NovoVaso();
        Assert.IsTrue(vaso.Zumbe, "com água, zumbe");

        Esvaziar(vaso);

        Assert.IsFalse(vaso.Zumbe);
    }

    [Test]
    public void Reenchido_VoltaAZumbir()
    {
        VasoInteractable vaso = NovoVaso();
        Esvaziar(vaso);
        int reenchidos = 0;
        vaso.OnReenchido += f => reenchidos++;

        vaso.Reencher();

        Assert.IsFalse(vaso.Esvaziado);
        Assert.AreEqual(1, reenchidos, "OnReenchido 1 vez");
        Assert.IsTrue(vaso.Zumbe);
        Assert.AreEqual(ResultadoDoPasso.Avancou, vaso.Verificar(kit), "reenchido, as Mãos esvaziam de novo");
    }

    [Test]
    public void Esvaziado_AreiaResolveDeVez()
    {
        VasoInteractable vaso = NovoVaso();
        Esvaziar(vaso);
        kit.Selecionar(Ferramenta.Areia);
        int concluidos = 0;
        vaso.OnCompleted += f => concluidos++;

        Assert.AreEqual(ResultadoDoPasso.Avancou, vaso.AvancarCom(kit), "1º passo de areia");
        Assert.AreEqual(ResultadoDoPasso.Avancou, vaso.AvancarCom(kit), "2º passo de areia");
        Assert.AreEqual(ResultadoDoPasso.Concluiu, vaso.AvancarCom(kit), "3º passo de areia");

        Assert.IsTrue(vaso.IsComplete);
        Assert.AreEqual(1, concluidos);
        Assert.AreEqual(0, kit.Estoque.Quantidade(Ferramenta.Areia), "a areia foi gasta");
    }

    [Test]
    public void AcaoDefinitivaComecada_SemAtalho()
    {
        VasoInteractable vaso = NovoVaso();
        kit.Selecionar(Ferramenta.Areia);
        Assert.AreEqual(ResultadoDoPasso.Avancou, vaso.AvancarCom(kit), "1 passo de areia");

        kit.Selecionar(Ferramenta.Maos);

        Assert.AreEqual(ResultadoDoPasso.FerramentaErrada, vaso.AvancarCom(kit), "areia pela metade: as Mãos não esvaziam");
        Assert.IsFalse(vaso.Esvaziado);
        Assert.AreEqual(0, vaso.PassosEsvaziando);
        Assert.AreEqual(1, vaso.CurrentClicks);
    }

    /* Esvaziado e com a areia já começada, a chuva não reenche: a água
     * voltaria por cima da areia pela metade. */
    [Test]
    public void Vaso_AreiaComecada_NaoReenche()
    {
        VasoInteractable vaso = NovoVaso();
        Esvaziar(vaso);
        kit.Selecionar(Ferramenta.Areia);
        Assert.AreEqual(ResultadoDoPasso.Avancou, vaso.AvancarCom(kit), "1 passo de areia");
        int reenchidos = 0;
        vaso.OnReenchido += f => reenchidos++;

        Assert.IsFalse(vaso.PodeReencher);
        vaso.Reencher();

        Assert.IsTrue(vaso.Esvaziado);
        Assert.AreEqual(0, reenchidos, "OnReenchido");
    }

    [Test]
    public void SemPodeEsvaziar_MaosDaFerramentaErrada()
    {
        Interactable ralo = TesteUtil.Com<Interactable>("Ralo (teste)");
        TesteUtil.Setar(ralo, "ferramentaNecessaria", Ferramenta.Tela);
        TesteUtil.Setar(ralo, "podeEsvaziar", false);
        TesteUtil.Setar(ralo, "punchScale", 0f);
        TesteUtil.Setar(ralo, "shrinkDuration", 0f);

        Assert.AreEqual(ResultadoDoPasso.FerramentaErrada, ralo.AvancarCom(kit));
        Assert.IsFalse(ralo.Esvaziado);
        Assert.AreEqual(0, ralo.PassosEsvaziando);
    }

    [Test]
    public void Reencher_SoQuandoEsvaziado()
    {
        VasoInteractable vaso = NovoVaso();
        int reenchidos = 0;
        vaso.OnReenchido += f => reenchidos++;

        Assert.IsFalse(vaso.PodeReencher);
        vaso.Reencher();

        Assert.AreEqual(0, reenchidos, "com água, não há o que reencher");
        Assert.IsFalse(vaso.Esvaziado);
    }

    [Test]
    public void SegundosPorPassoCom_Esvaziar()
    {
        VasoInteractable vaso = NovoVaso();

        Assert.AreEqual(0.4f, vaso.SegundosPorPassoCom(kit), 1e-5f, "Mãos: 1,2 s em 3 passos");

        kit.Selecionar(Ferramenta.Areia);
        Assert.AreEqual(vaso.SegundosPorPasso, vaso.SegundosPorPassoCom(kit), 1e-5f, "Areia: o passo normal");
        Assert.AreEqual(vaso.SegundosPorPasso, vaso.SegundosPorPassoCom(null), 1e-5f, "sem kit: o passo normal");
    }

    [Test]
    public void Vaso_Esvaziado_AguaDesligada_Reenchido_AguaLigada()
    {
        VasoInteractable vaso = NovoVaso();
        GameObject agua = TesteUtil.Novo("agua (teste)");
        agua.transform.SetParent(vaso.transform, false);
        agua.transform.localScale = new Vector3(1f, 0.6f, 1f);
        TesteUtil.Setar(vaso, "water", agua);

        vaso.AvancarCom(kit);
        Assert.AreEqual(0.4f, agua.transform.localScale.y, 1e-5f, "1º passo: a água baixa para 2/3");
        Assert.IsTrue(agua.activeSelf);

        vaso.AvancarCom(kit);
        Assert.AreEqual(0.2f, agua.transform.localScale.y, 1e-5f, "2º passo: 1/3");

        vaso.AvancarCom(kit);
        Assert.IsFalse(agua.activeSelf, "esvaziado: a água some");

        vaso.Reencher();
        Assert.IsTrue(agua.activeSelf, "reenchido: a água volta");
        Assert.AreEqual(new Vector3(1f, 0.6f, 1f), agua.transform.localScale, "com a escala original");
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

    [Test]
    public void Garrafas_EsvaziarNaoTiraGarrafa()
    {
        GarrafaInteractable grupo = NovasGarrafas(out GameObject agua);
        Assert.AreEqual(3, grupo.GarrafasRestantes, "o grupo juntou as 3 (a água não conta)");

        Esvaziar(grupo);

        Assert.IsTrue(grupo.Esvaziado);
        Assert.AreEqual(3, grupo.GarrafasRestantes, "esvaziar não recolhe garrafa");
        Assert.AreEqual(0, kit.Estoque.GarrafasNoSaco, "nada foi para o saco");
        Assert.IsFalse(agua.activeSelf, "a água das garrafas some");

        grupo.Reencher();
        Assert.IsTrue(agua.activeSelf, "reenchido: a água volta");
    }

    [Test]
    public void Garrafas_SacoCheio_AindaEsvazia()
    {
        TesteUtil.Setar(kit, "capacidadeSaco", 1);
        kit.GuardarGarrafa();
        Assert.IsTrue(kit.Estoque.SacoCheio);
        GarrafaInteractable grupo = NovasGarrafas(out _);

        Assert.AreEqual(ResultadoDoPasso.Avancou, grupo.Verificar(kit), "o esvaziar não passa pela regra do saco");
        Esvaziar(grupo);

        Assert.IsTrue(grupo.Esvaziado);
    }

    /* Esvaziadas e com o recolhimento já começado, a chuva não reenche. */
    [Test]
    public void Garrafas_RecolhimentoComecado_NaoReenche()
    {
        GarrafaInteractable grupo = NovasGarrafas(out _);
        Esvaziar(grupo);
        kit.Selecionar(Ferramenta.SacoDeLixo);
        Assert.AreEqual(ResultadoDoPasso.Avancou, grupo.AvancarCom(kit), "1 garrafa para o saco");
        int reenchidos = 0;
        grupo.OnReenchido += f => reenchidos++;

        Assert.IsFalse(grupo.PodeReencher);
        grupo.Reencher();

        Assert.IsTrue(grupo.Esvaziado);
        Assert.AreEqual(0, reenchidos, "OnReenchido");
    }

    /* O "tira a água" só no 1º passo de um foco que ainda tem água. */
    [Test]
    public void PrimeiroClique_SoComAgua()
    {
        kit.Selecionar(Ferramenta.Areia);
        VasoInteractable cheio = NovoVaso();
        Assert.IsFalse(SomDosFocos.TocaPrimeiroClique(cheio), "sem passo nenhum");
        cheio.AvancarCom(kit);
        Assert.AreEqual(1, cheio.CurrentClicks);
        Assert.IsTrue(SomDosFocos.TocaPrimeiroClique(cheio), "1º passo, com água");

        kit.Selecionar(Ferramenta.Maos);
        VasoInteractable seco = NovoVaso();
        Esvaziar(seco);
        kit.Selecionar(Ferramenta.Areia);
        seco.AvancarCom(kit);
        Assert.AreEqual(1, seco.CurrentClicks);
        Assert.IsFalse(SomDosFocos.TocaPrimeiroClique(seco), "esvaziado: a água já saiu");
    }

    /* A mira mostra só o nome do foco, cheio ou esvaziado. */
    [Test]
    public void Mira_SoONomeDoFoco()
    {
        GarrafaInteractable grupo = NovasGarrafas(out _);
        FocoHUDInfo infoGrupo = grupo.gameObject.AddComponent<FocoHUDInfo>();
        infoGrupo.textoAoMirar = "Garrafas largadas juntando água";
        Assert.AreEqual("Garrafas", DengueHUD.TextoDeMirar(grupo, null, infoGrupo, kit));
        Esvaziar(grupo);
        Assert.AreEqual("Garrafas", DengueHUD.TextoDeMirar(grupo, null, infoGrupo, kit));

        VasoInteractable vaso = NovoVaso();
        FocoHUDInfo infoVaso = vaso.gameObject.AddComponent<FocoHUDInfo>();
        infoVaso.textoAoMirar = "Pratinho com água";
        Esvaziar(vaso);
        Assert.AreEqual("Vaso de flor", DengueHUD.TextoDeMirar(vaso, null, infoVaso, kit));
    }
}
