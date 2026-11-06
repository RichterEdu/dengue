using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

/* ============================================================
 *  ItensNaMaoAssetsTestes.cs
 *  ---------------------------------------------------------------------
 *  Testes EditMode dos assets dos itens na mão: medidas e importação dos
 *  FBX, malha da tela com furos e material de partícula.
 *
 *  Depois de reexportar os FBX: rodar  Dengue > Configurar Itens na Mão.
 *
 *  Rodar:  Window > General > Test Runner > EditMode (categoria ItensNaMao)
 *  ou      unity command run_tests --mode EditMode --filter_type category --filter ItensNaMao
 * ============================================================ */

[Category("ItensNaMao")]
public class ItensNaMaoAssetsTestes
{
    private const string Pasta = "Assets/Meus/Assets Gerados/Itens";

    private static GameObject Carregar(string nome)
    {
        GameObject modelo = AssetDatabase.LoadAssetAtPath<GameObject>(Pasta + "/" + nome + ".fbx");
        Assert.IsNotNull(modelo, nome + ".fbx não existe");
        return modelo;
    }

    /* Limites do modelo no próprio espaço, com a importação aplicada. */
    private static Bounds Limites(string nome)
    {
        GameObject copia = Object.Instantiate(Carregar(nome));
        try
        {
            Renderer[] renderers = copia.GetComponentsInChildren<Renderer>();
            Bounds limites = renderers[0].bounds;
            foreach (Renderer r in renderers)
                limites.Encapsulate(r.bounds);
            return limites;
        }
        finally
        {
            Object.DestroyImmediate(copia);
        }
    }

    private static Renderer RendererDe(string nome, string filho)
    {
        return Carregar(nome).GetComponentsInChildren<Renderer>(true).First(r => r.name == filho);
    }

    [TestCase("Balde", 0.21f, 0.24f)]      // Ø 22 cm, com a borda
    [TestCase("Tela", 0.285f, 0.295f)]
    public void Fbx_TemALarguraDaSpec(string nome, float minimo, float maximo)
    {
        Vector3 tamanho = Limites(nome).size;
        float largura = Mathf.Max(tamanho.x, tamanho.z);

        Assert.GreaterOrEqual(largura, minimo);
        Assert.LessOrEqual(largura, maximo);
    }

    [Test]
    public void Borrifador_TemAAlturaDaSpec()
    {
        float altura = Limites("Borrifador").size.y;

        Assert.GreaterOrEqual(altura, 0.19f);
        Assert.LessOrEqual(altura, 0.21f);
    }

    [Test]
    public void Balde_TemAreiaSeparada()
    {
        Assert.IsNotNull(RendererDe("Balde", "Areia"));
    }

    [Test]
    public void Tela_MalhaEhAlphaClip()
    {
        Material material = RendererDe("Tela", "Malha").sharedMaterial;

        Assert.AreEqual(1f, material.GetFloat("_AlphaClip"));
        Assert.IsTrue(material.IsKeywordEnabled("_ALPHATEST_ON"));
        Assert.AreEqual(0f, material.GetFloat("_Cull"), "as duas faces visíveis");
        Assert.AreEqual("Tela_Malha", material.GetTexture("_BaseMap").name);
    }

    [Test]
    public void Tela_TexturaPreservaCobertura()
    {
        var importador = (TextureImporter)AssetImporter.GetAtPath(Pasta + "/Tela_Malha.png");

        Assert.AreEqual(TextureWrapMode.Repeat, importador.wrapMode);
        Assert.IsTrue(importador.mipmapEnabled);
        Assert.IsTrue(importador.mipMapsPreserveCoverage);
    }

    [Test]
    public void Tela_EhPlanaEDeitada()
    {
        Assert.Less(Limites("Tela").size.y, 0.01f);
    }

    [TestCase("Balde", "Balde", "Zinco")]
    [TestCase("Balde", "Areia", "Areia")]
    [TestCase("Tela", "Moldura", "TelaMoldura")]
    [TestCase("Borrifador", "Borrifador", "Frasco")]
    public void Fbx_UsaOMaterialURP(string nome, string filho, string material)
    {
        Material usado = RendererDe(nome, filho).sharedMaterials
            .First(m => m != null && m.name.Contains(material));

        Assert.AreEqual("Universal Render Pipeline/Lit", usado.shader.name);
    }

    [Test]
    public void Particula_TemMaterialProprio()
    {
        Material material = AssetDatabase.LoadAssetAtPath<Material>(Pasta + "/Itens_Particula.mat");

        Assert.IsNotNull(material);
        Assert.AreEqual("Sprites/Default", material.shader.name);
        Assert.AreEqual("Particula", material.mainTexture.name);
    }

    /* ---- Poses, gestos e camada do Animator ---- */

    private static readonly string[] EstadosDaCamada =
    {
        "CarregandoBalde", "CarregandoTela", "CarregandoBorrifador",
        "UsandoBalde", "UsandoTela", "UsandoBorrifador",
    };

    private static AnimationClip[] ClipesDoAgente()
    {
        return AssetDatabase.LoadAllAssetsAtPath(PoseDeCarga.CaminhoFbxAgente).OfType<AnimationClip>()
            .Where(c => !c.name.StartsWith("__preview__"))
            .ToArray();
    }

    private static AnimatorControllerLayer CamadaDosItens(AnimatorController controller)
    {
        AnimatorControllerLayer camada = controller.layers.FirstOrDefault(l => l.name == ItensNaMao.NomeCamada);
        Assert.IsNotNull(camada, "o controller não tem a camada " + ItensNaMao.NomeCamada);
        return camada;
    }

    [Test]
    public void Controller_TemACamadaComOs6Estados()
    {
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(PoseDeCarga.CaminhoController);
        AnimatorControllerLayer camada = CamadaDosItens(controller);

        Assert.AreEqual(0f, camada.defaultWeight, "sem o componente, o braço não pode congelar");
        Assert.AreEqual(PoseDeCarga.CaminhoMascaraSaco, AssetDatabase.GetAssetPath(camada.avatarMask));
        Assert.AreEqual(AnimatorLayerBlendingMode.Override, camada.blendingMode);

        ChildAnimatorState[] estados = camada.stateMachine.states;
        CollectionAssert.AreEquivalent(EstadosDaCamada, estados.Select(e => e.state.name));
        foreach (ChildAnimatorState filho in estados)
        {
            Assert.IsNotNull(filho.state.motion, filho.state.name + " sem clipe");
            Assert.AreEqual(filho.state.name, filho.state.motion.name);
            Assert.IsFalse(filho.state.writeDefaultValues, filho.state.name);
        }
    }

    [TestCase("UsandoBalde", 16, 24)]
    [TestCase("UsandoTela", 16, 24)]
    [TestCase("UsandoBorrifador", 16, 24)]
    public void Gesto_DuraDe16a24Quadros(string nome, int minimo, int maximo)
    {
        AnimationClip clipe = ClipesDoAgente().FirstOrDefault(c => c.name == nome);
        Assert.IsNotNull(clipe, nome + " não está no FBX");

        int quadros = Mathf.RoundToInt(clipe.length * clipe.frameRate);

        Assert.GreaterOrEqual(quadros, minimo);
        Assert.LessOrEqual(quadros, maximo);
    }

    [Test]
    public void Clipes_MantemOsAntigos()
    {
        string[] nomes = ClipesDoAgente().Select(c => c.name).ToArray();

        foreach (string antigo in new[] { "Idle", "Walk", "Run", "Air", "Carregando", "CarregandoSaco" })
            CollectionAssert.Contains(nomes, antigo);
    }
}
