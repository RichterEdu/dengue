using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

/* ============================================================
 *  ConfiguracoesGlobaisTestes.cs
 *  ---------------------------------------------------------------------
 *  Testes EditMode da lógica da janela Dengue > Configurações Globais:
 *  o que cada tipo de foco lê e grava, os limites, os prefabs de origem
 *  e a existência dos campos das duas seções.
 *
 *  A maioria usa objetos do TesteUtil (escondidos, fora de cena). Os de
 *  prefab e de cena usam a cena temporária que o Test Runner abre no lugar
 *  da sua (sem título, some no fim da execução) e um prefab temporário,
 *  apagado no [TearDown]. UsarCenaDoRunner falha se a cena ativa tiver
 *  caminho em disco: nada aqui escreve numa cena de verdade.
 *
 *  Rodar:  Window > General > Test Runner > EditMode (categoria Config)
 *  ou      unity command run_tests --mode EditMode --filter_type category --filter Config
 * ============================================================ */

[Category("Config")]
public class ConfiguracoesGlobaisTestes
{
    private const string PastaTemporaria = "Assets/_TesteConfigGlobais";
    private const string CaminhoPrefab = PastaTemporaria + "/Foco.prefab";

    private Scene cenaDeTeste;
    private readonly List<GameObject> naCena = new List<GameObject>();

    [TearDown]
    public void Limpar()
    {
        TesteUtil.LimparTudo();
        foreach (GameObject go in naCena)
            if (go != null)
                Object.DestroyImmediate(go);
        naCena.Clear();
        if (AssetDatabase.IsValidFolder(PastaTemporaria))
            AssetDatabase.DeleteAsset(PastaTemporaria);
    }

    private static ConfiguracoesGlobais.TipoDeFoco Tipo<T>() where T : Interactable
    {
        ConfiguracoesGlobais.TipoDeFoco tipo = ConfiguracoesGlobais.Tipos.FirstOrDefault(t => t.Tipo == typeof(T));
        Assert.IsNotNull(tipo, typeof(T).Name + " sem linha na tabela Tipos");
        return tipo;
    }

    private static T Foco<T>(int passos = 3) where T : Interactable
    {
        T foco = TesteUtil.Com<T>(typeof(T).Name + " (teste)");
        TesteUtil.Setar(foco, "requiredClicks", passos);
        return foco;
    }

    private static float Lido(Object alvo, string campo)
    {
        return new SerializedObject(alvo).FindProperty(campo).floatValue;
    }

    private void UsarCenaDoRunner()
    {
        cenaDeTeste = SceneManager.GetActiveScene();
        Assert.IsEmpty(cenaDeTeste.path, "este teste só pode rodar na cena temporária do Test Runner");
    }

    /* Prefab temporário com um foco T e uma instância dele na cena de teste. */
    private T InstanciaDePrefab<T>(int passosNoPrefab) where T : Interactable
    {
        UsarCenaDoRunner();
        if (!AssetDatabase.IsValidFolder(PastaTemporaria))
            AssetDatabase.CreateFolder("Assets", Path.GetFileName(PastaTemporaria));

        T modelo = Foco<T>(passosNoPrefab);
        modelo.gameObject.hideFlags = HideFlags.None;
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(modelo.gameObject, CaminhoPrefab);

        GameObject instancia = (GameObject)PrefabUtility.InstantiatePrefab(prefab, cenaDeTeste);
        naCena.Add(instancia);
        return instancia.GetComponent<T>();
    }

    private static T DoPrefab<T>() where T : Interactable
    {
        return AssetDatabase.LoadAssetAtPath<GameObject>(CaminhoPrefab).GetComponent<T>();
    }

    /* ================= Tabela de tipos ================= */

    [Test]
    public void Tipos_CobreTodoFocoDoJogo()
    {
        var doJogo = TypeCache.GetTypesDerivedFrom<Interactable>()
            .Where(t => t.Assembly.GetName().Name == "Assembly-CSharp" && !t.IsAbstract);

        foreach (System.Type tipo in doJogo)
            Assert.IsTrue(ConfiguracoesGlobais.Tipos.Any(t => t.Tipo == tipo), tipo.Name + " sem linha na tabela Tipos");
    }

    [Test]
    public void Tipos_TodoCampoExisteNoFoco()
    {
        foreach (ConfiguracoesGlobais.TipoDeFoco tipo in ConfiguracoesGlobais.Tipos)
        {
            Interactable foco = (Interactable)TesteUtil.Novo(tipo.Rotulo).AddComponent(tipo.Tipo);
            SerializedObject so = new SerializedObject(foco);

            Assert.IsNotNull(so.FindProperty(tipo.CampoTempo), tipo.Rotulo + ": " + tipo.CampoTempo);
            if (tipo.CampoPassos != null)
                Assert.IsNotNull(so.FindProperty(tipo.CampoPassos), tipo.Rotulo + ": " + tipo.CampoPassos);
        }
    }

    [Test]
    public void Partida_TodoCampoExisteNoComponente()
    {
        Assert.IsNotEmpty(ConfiguracoesGlobais.Partida);
        foreach (ConfiguracoesGlobais.GrupoDaPartida grupo in ConfiguracoesGlobais.Partida)
        {
            Component c = TesteUtil.Novo(grupo.Titulo).AddComponent(grupo.Tipo);
            SerializedObject so = new SerializedObject(c);
            foreach (string campo in grupo.Campos)
                Assert.IsNotNull(so.FindProperty(campo), grupo.Titulo + ": " + campo);
        }
    }

    /* Os menus de montagem não podem desfazer o que a janela grava: a cena é
     * a única fonte destes valores. */
    [TestCase("Assets/Editor/KitDoAgenteBuilder.cs")]
    [TestCase("Assets/Editor/FocosVivosBuilder.cs")]
    public void Builders_NaoGravamOsCamposDaJanela(string caminho)
    {
        string codigo = File.ReadAllText(caminho);
        IEnumerable<string> campos = ConfiguracoesGlobais.Partida.SelectMany(g => g.Campos)
            .Concat(ConfiguracoesGlobais.Tipos.SelectMany(t => new[] { t.CampoPassos, t.CampoTempo }))
            .Concat(new[] { ConfiguracoesGlobais.CampoPassosEsvaziar, ConfiguracoesGlobais.CampoSegundosEsvaziar })
            .Where(c => c != null)
            .Distinct();

        foreach (string campo in campos)
            StringAssert.DoesNotContain("\"" + campo + "\"", codigo, caminho + " regrava " + campo);
    }

    /* ================= Aplicar ================= */

    [Test]
    public void Aplicar_MudaTodosDoTipo_ENenhumDeOutroTipo()
    {
        VasoInteractable a = Foco<VasoInteractable>(3);
        VasoInteractable b = Foco<VasoInteractable>(10);
        PneuInteractable pneu = Foco<PneuInteractable>(5);

        int mudados = ConfiguracoesGlobais.Aplicar(Tipo<VasoInteractable>(), new Interactable[] { a, b, pneu },
                                                   new ConfiguracoesGlobais.Valores { Passos = 7 });

        Assert.AreEqual(2, mudados);
        Assert.AreEqual(7, a.RequiredClicks);
        Assert.AreEqual(7, b.RequiredClicks);
        Assert.AreEqual(5, pneu.RequiredClicks, "outro tipo não muda");
    }

    [Test]
    public void Aplicar_SoMudaOCampoPedido()
    {
        VasoInteractable vaso = Foco<VasoInteractable>(3);
        TesteUtil.Setar(vaso, "segundosParaConcluir", 2f);

        ConfiguracoesGlobais.Aplicar(Tipo<VasoInteractable>(), new Interactable[] { vaso },
                                     new ConfiguracoesGlobais.Valores { Passos = 7 });

        Assert.AreEqual(2f, Lido(vaso, "segundosParaConcluir"), 1e-5f, "tempo não pedido fica");
    }

    [Test]
    public void Aplicar_PassosZero_ViraUm()
    {
        VasoInteractable vaso = Foco<VasoInteractable>(3);

        ConfiguracoesGlobais.Aplicar(Tipo<VasoInteractable>(), new Interactable[] { vaso },
                                     new ConfiguracoesGlobais.Valores { Passos = 0 });

        Assert.AreEqual(1, vaso.RequiredClicks, "com 0 o foco nunca seria eliminado");
    }

    [Test]
    public void Aplicar_TempoNegativo_ViraZero()
    {
        VasoInteractable vaso = Foco<VasoInteractable>(3);

        ConfiguracoesGlobais.Aplicar(Tipo<VasoInteractable>(), new Interactable[] { vaso },
                                     new ConfiguracoesGlobais.Valores { Tempo = -1f });

        Assert.AreEqual(0f, Lido(vaso, "segundosParaConcluir"), 1e-5f);
    }

    [Test]
    public void Aplicar_Garrafas_NaoGravaPassos_GravaSegundosPorGarrafa()
    {
        GarrafaInteractable garrafa = Foco<GarrafaInteractable>(5);

        ConfiguracoesGlobais.Aplicar(Tipo<GarrafaInteractable>(), new Interactable[] { garrafa },
                                     new ConfiguracoesGlobais.Valores { Passos = 2, Tempo = 0.7f });

        Assert.AreEqual(5, garrafa.RequiredClicks, "garrafas: um passo por garrafa, sempre");
        Assert.AreEqual(0.7f, Lido(garrafa, "segundosPorGarrafa"), 1e-5f);
    }

    [Test]
    public void Aplicar_Ralo_GravaSegundosPorVolta_ENaoSegundosParaConcluir()
    {
        RaloInteractable ralo = Foco<RaloInteractable>(3);
        TesteUtil.Setar(ralo, "segundosParaConcluir", 2f);

        ConfiguracoesGlobais.Aplicar(Tipo<RaloInteractable>(), new Interactable[] { ralo },
                                     new ConfiguracoesGlobais.Valores { Passos = 4, Tempo = 0.9f });

        Assert.AreEqual(4, ralo.RequiredClicks);
        Assert.AreEqual(0.9f, ralo.SegundosPorVoltaDoCheck, 1e-5f);
        Assert.AreEqual(2f, Lido(ralo, "segundosParaConcluir"), 1e-5f);
    }

    [Test]
    public void Aplicar_Ralo_VoltaMinima()
    {
        RaloInteractable ralo = Foco<RaloInteractable>(3);

        ConfiguracoesGlobais.Aplicar(Tipo<RaloInteractable>(), new Interactable[] { ralo },
                                     new ConfiguracoesGlobais.Valores { Tempo = 0f });

        Assert.AreEqual(0.1f, ralo.SegundosPorVoltaDoCheck, 1e-5f, "volta de 0 s tiraria o tempo de reação");
    }

    [Test]
    public void Aplicar_Esvaziar_SoNoTipoQueTemOAtalho()
    {
        VasoInteractable vaso = Foco<VasoInteractable>(3);
        PneuInteractable pneu = Foco<PneuInteractable>(5);
        ConfiguracoesGlobais.Valores valores = new ConfiguracoesGlobais.Valores { PassosEsvaziar = 6, SegundosEsvaziar = 2f };

        ConfiguracoesGlobais.Aplicar(Tipo<VasoInteractable>(), new Interactable[] { vaso }, valores);
        ConfiguracoesGlobais.Aplicar(Tipo<PneuInteractable>(), new Interactable[] { pneu }, valores);

        Assert.AreEqual(6, vaso.PassosParaEsvaziar);
        Assert.AreEqual(2f, Lido(vaso, "segundosParaEsvaziar"), 1e-5f);
        Assert.AreEqual(3, pneu.PassosParaEsvaziar, "no pneu o atalho não existe");
    }

    [Test]
    public void Aplicar_SemMudanca_NaoContaFoco()
    {
        VasoInteractable vaso = Foco<VasoInteractable>(7);

        int mudados = ConfiguracoesGlobais.Aplicar(Tipo<VasoInteractable>(), new Interactable[] { vaso },
                                                   new ConfiguracoesGlobais.Valores { Passos = 7 });

        Assert.AreEqual(0, mudados);
    }

    /* ================= Prefab de origem ================= */

    [Test]
    public void Aplicar_InstanciaComOverride_RecebeOValor_EOPrefabTambem()
    {
        VasoInteractable instancia = InstanciaDePrefab<VasoInteractable>(3);
        TesteUtil.Setar(instancia, "requiredClicks", 10);

        ConfiguracoesGlobais.Aplicar(Tipo<VasoInteractable>(), new Interactable[] { instancia },
                                     new ConfiguracoesGlobais.Valores { Passos = 6 });

        Assert.AreEqual(6, instancia.RequiredClicks, "o override da cena vence o prefab: tem de ser gravado");
        Assert.AreEqual(6, DoPrefab<VasoInteractable>().RequiredClicks, "instância nova nasce certa");
    }

    [Test]
    public void Aplicar_InstanciaSemOverride_ContinuaSemOverride()
    {
        VasoInteractable instancia = InstanciaDePrefab<VasoInteractable>(3);

        ConfiguracoesGlobais.Aplicar(Tipo<VasoInteractable>(), new Interactable[] { instancia },
                                     new ConfiguracoesGlobais.Valores { Passos = 6 });

        Assert.AreEqual(6, instancia.RequiredClicks);
        SerializedProperty passos = new SerializedObject(instancia).FindProperty("requiredClicks");
        Assert.IsFalse(passos.prefabOverride, "herda do prefab, como antes");
    }

    /* ================= Leitura ================= */

    [Test]
    public void Ler_ValoresDiferentes_Misto()
    {
        VasoInteractable a = Foco<VasoInteractable>(3);
        VasoInteractable b = Foco<VasoInteractable>(5);
        TesteUtil.Setar(a, "segundosParaConcluir", 2f);
        TesteUtil.Setar(b, "segundosParaConcluir", 2f);

        ConfiguracoesGlobais.Leitura leitura = ConfiguracoesGlobais.Ler(Tipo<VasoInteractable>(), new Interactable[] { a, b });

        Assert.AreEqual(2, leitura.Quantidade);
        Assert.IsTrue(leitura.Passos.Misto);
        Assert.AreEqual(3f, leitura.Passos.Min);
        Assert.AreEqual(5f, leitura.Passos.Max);
        Assert.IsFalse(leitura.Tempo.Misto);
    }

    [Test]
    public void Ler_SoContaOTipoPedido()
    {
        VasoInteractable vaso = Foco<VasoInteractable>(3);
        PneuInteractable pneu = Foco<PneuInteractable>(5);

        ConfiguracoesGlobais.Leitura leitura = ConfiguracoesGlobais.Ler(Tipo<VasoInteractable>(), new Interactable[] { vaso, pneu });

        Assert.AreEqual(1, leitura.Quantidade);
    }

    [Test]
    public void Ler_Garrafas_PassosVemDasGarrafas_NaoDoCampo()
    {
        GarrafaInteractable grupo = Foco<GarrafaInteractable>(99);
        for (int i = 0; i < 4; i++)
        {
            GameObject filho = GameObject.CreatePrimitive(PrimitiveType.Cube);
            filho.hideFlags = HideFlags.HideAndDontSave;
            filho.transform.SetParent(grupo.transform);
        }

        ConfiguracoesGlobais.Leitura leitura = ConfiguracoesGlobais.Ler(Tipo<GarrafaInteractable>(), new Interactable[] { grupo });

        Assert.AreEqual(4f, leitura.Passos.Min, "o OnStart troca o requiredClicks pelo número de garrafas");
    }

    [Test]
    public void Resumo_FocoDeSegurar_MostraAConta()
    {
        VasoInteractable vaso = Foco<VasoInteractable>(10);
        TesteUtil.Setar(vaso, "segundosParaConcluir", 2f);

        string resumo = ConfiguracoesGlobais.Resumo(Tipo<VasoInteractable>(), new Interactable[] { vaso });

        Assert.AreEqual("10 passos · 2 s no total · 0,20 s por passo", resumo);
    }

    [Test]
    public void Resumo_Garrafas_ListaOsGrupos()
    {
        GarrafaInteractable a = Foco<GarrafaInteractable>(1);
        GarrafaInteractable b = Foco<GarrafaInteractable>(1);
        for (int i = 0; i < 2; i++)
        {
            GameObject filho = GameObject.CreatePrimitive(PrimitiveType.Cube);
            filho.hideFlags = HideFlags.HideAndDontSave;
            filho.transform.SetParent(i == 0 ? a.transform : b.transform);
        }
        GameObject extra = GameObject.CreatePrimitive(PrimitiveType.Cube);
        extra.hideFlags = HideFlags.HideAndDontSave;
        extra.transform.SetParent(b.transform);

        string resumo = ConfiguracoesGlobais.Resumo(Tipo<GarrafaInteractable>(), new Interactable[] { a, b });

        Assert.AreEqual("garrafas por grupo: 1, 2 · 0,40 s por garrafa", resumo);
    }

    [Test]
    public void Aviso_BromeliaComMaisPassosQuePocas()
    {
        BromeliaInteractable bromelia = Foco<BromeliaInteractable>(5);
        SerializedObject so = new SerializedObject(bromelia);
        so.FindProperty("pocas").arraySize = 5;
        so.ApplyModifiedPropertiesWithoutUndo();

        Assert.IsNull(ConfiguracoesGlobais.Aviso(Tipo<BromeliaInteractable>(), new Interactable[] { bromelia }, 5));
        StringAssert.Contains("5 poças", ConfiguracoesGlobais.Aviso(Tipo<BromeliaInteractable>(), new Interactable[] { bromelia }, 6));
    }

    /* ================= Cena ================= */

    [Test]
    public void FocosDaCena_IncluiInativo_EIgnoraOutrasCenas()
    {
        UsarCenaDoRunner();
        GameObject inativo = new GameObject("Inativo", typeof(VasoInteractable));
        naCena.Add(inativo);
        inativo.SetActive(false);
        PneuInteractable foraDaCena = Foco<PneuInteractable>();

        List<Interactable> focos = ConfiguracoesGlobais.FocosDaCena(cenaDeTeste);

        CollectionAssert.Contains(focos, inativo.GetComponent<Interactable>());
        CollectionAssert.DoesNotContain(focos, foraDaCena);
    }
}
