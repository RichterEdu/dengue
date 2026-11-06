using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

/* ============================================================
 *  ItensNaMaoBuilder.cs  (Editor)
 *  ---------------------------------------------------------------------
 *  Monta os itens do kit na mão do agente: balde de areia, tela com
 *  moldura e borrifador. Modelos e texturas saem de
 *  Ferramentas/Blender/gerar_itens.py, copiados para a Pasta.
 *
 *  GarantirImportacaoDosItens() — idempotente, rodar depois de reexportar:
 *    - importação dos 3 FBX (como a do Saco.fbx), com os materiais URP;
 *    - malha da tela com furos: Alpha Clip, duas faces, mip maps
 *      preservando a cobertura do alfa, para não sumir de longe;
 *    - material de partícula com Sprites/Default e uma bolinha própria
 *      (a textura embutida "Default-Particle.psd" não é achada nesta
 *      versão do Unity).
 *
 *  GarantirCamadaDosItens() — idempotente, rodar depois de reexportar o
 *  agente: clipes dos itens no AgenteSaude.fbx e a camada "ItemNaMao"
 *  (máscara do braço direito, a mesma do saco).
 * ============================================================ */

public static class ItensNaMaoBuilder
{
    public const string Pasta = "Assets/Meus/Assets Gerados/Itens";

    private const string CaminhoTexturaMalha = Pasta + "/Tela_Malha.png";
    private const string CaminhoTexturaParticula = Pasta + "/Particula.png";
    private const string CaminhoMaterialMalha = Pasta + "/Tela_Malha.mat";
    public const string CaminhoMaterialParticula = Pasta + "/Itens_Particula.mat";

    private const string NomeMaterialMalhaNoFbx = "TelaMalha";

    // Estados da camada "ItemNaMao": os mesmos nomes dos clipes do gerar_agente.py.
    private static readonly string[] EstadosDaCamada =
    {
        "CarregandoBalde", "CarregandoTela", "CarregandoBorrifador",
        "UsandoBalde", "UsandoTela", "UsandoBorrifador",
    };

    // Mesmas cores do gerar_itens.py (sRGB) + suavidade.
    private static readonly (string fbx, (string nome, string hex, float suavidade)[] materiais)[] Modelos =
    {
        ("Balde", new[] { ("Zinco", "#9EA8B3", 0.55f), ("Alca", "#4D4F54", 0.5f), ("Areia", "#D6B87A", 0.1f) }),
        ("Tela", new[] { ("TelaMoldura", "#17805F", 0.45f) }),
        ("Borrifador", new[] { ("Frasco", "#EDF0EB", 0.55f), ("Rotulo", "#1C54AD", 0.3f), ("Faixa", "#F5CC33", 0.3f),
                               ("Cabeca", "#17805F", 0.45f), ("Gatilho", "#0D4D3D", 0.35f) }),
    };

    /* FBX, texturas e materiais dos itens. */
    public static string GarantirImportacaoDosItens()
    {
        ConfigurarTextura(CaminhoTexturaMalha, TextureWrapMode.Repeat, true);
        ConfigurarTextura(CaminhoTexturaParticula, TextureWrapMode.Clamp, false);
        Material malha = GarantirMaterialDaMalha();
        GarantirMaterialDeParticula();

        foreach (var (fbx, materiais) in Modelos)
        {
            string caminho = Pasta + "/" + fbx + ".fbx";
            var imp = AssetImporter.GetAtPath(caminho) as ModelImporter;
            if (imp == null)
                return "[Itens] ERRO: não achei " + caminho + " (rode o gerar_itens.py e copie o FBX)";

            imp.globalScale = 1f;
            imp.useFileScale = true;
            imp.bakeAxisConversion = true;      // sem isso o Blender entra com -90° em X
            imp.importCameras = false;
            imp.importLights = false;
            imp.importBlendShapes = false;
            imp.importAnimation = false;
            imp.animationType = ModelImporterAnimationType.None;
            imp.addCollider = false;
            imp.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;

            foreach (var (nome, hex, suavidade) in materiais)
                imp.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), nome),
                             GarantirMaterial(nome, hex, suavidade));
            if (fbx == "Tela")
                imp.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), NomeMaterialMalhaNoFbx), malha);

            imp.SaveAndReimport();
        }

        AssetDatabase.SaveAssets();
        return "[Itens] 3 FBX importados com os materiais URP";
    }

    /* Clipes dos itens no AgenteSaude.fbx e a camada "ItemNaMao" no controller.
     * A camada é recriada a cada vez (nada duplica) e nasce com peso 0, para
     * uma cena sem o ItensNaMao não congelar o braço. Sem transições: o
     * componente troca de estado por CrossFade. */
    public static string GarantirCamadaDosItens()
    {
        string clipes = PoseDeCarga.GarantirClipes(EstadosDaCamada);
        if (clipes.Contains("ERRO"))
            return clipes;

        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(PoseDeCarga.CaminhoController);
        AnimationClip[] clipesDoFbx = AssetDatabase.LoadAllAssetsAtPath(PoseDeCarga.CaminhoFbxAgente)
            .OfType<AnimationClip>()
            .ToArray();
        AvatarMask mascara = PoseDeCarga.GarantirMascara(PoseDeCarga.CaminhoMascaraSaco, PoseDeCarga.OssosDaMascaraSaco);
        if (controller == null || mascara == null)
            return "[Itens] ERRO: falta o controller ou a máscara do braço (PoseDeCarga.GarantirAssets)";

        AnimatorStateMachine maquina = PoseDeCarga.RecriarCamada(controller, ItensNaMao.NomeCamada, mascara);
        for (int i = 0; i < EstadosDaCamada.Length; i++)
        {
            string nome = EstadosDaCamada[i];
            AnimationClip clipe = clipesDoFbx.FirstOrDefault(c => c.name == nome);
            if (clipe == null)
                return "[Itens] ERRO: falta o clipe " + nome;

            AnimatorState estado = maquina.AddState(nome, new Vector3(300f, 60f * i, 0f));
            estado.motion = clipe;
            estado.writeDefaultValues = false;
        }
        maquina.defaultState = maquina.states[0].state;

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        return clipes + "\n[Itens] camada \"" + ItensNaMao.NomeCamada + "\" (peso 0) com "
               + EstadosDaCamada.Length + " estados";
    }

    private static void ConfigurarTextura(string caminho, TextureWrapMode repeticao, bool cobertura)
    {
        var imp = AssetImporter.GetAtPath(caminho) as TextureImporter;
        if (imp == null)
        {
            Debug.LogError("[Itens] Não achei " + caminho + ": rode o gerar_itens.py e copie o PNG.");
            return;
        }

        imp.textureType = TextureImporterType.Default;
        imp.wrapMode = repeticao;
        imp.alphaIsTransparency = true;
        imp.mipmapEnabled = true;
        imp.mipMapsPreserveCoverage = cobertura;
        imp.alphaTestReferenceValue = 0.5f;
        imp.SaveAndReimport();
    }

    /* Material URP Lit opaco, em Pasta/Item_<nome>.mat. */
    private static Material GarantirMaterial(string nome, string hex, float suavidade)
    {
        Material mat = CarregarOuCriar(Pasta + "/Item_" + nome + ".mat", "Universal Render Pipeline/Lit");
        ColorUtility.TryParseHtmlString(hex, out Color cor);
        mat.SetColor("_BaseColor", cor);
        mat.SetColor("_Color", cor);
        mat.SetFloat("_Smoothness", suavidade);
        mat.SetFloat("_Metallic", 0f);
        mat.enableInstancing = true;
        EditorUtility.SetDirty(mat);
        return mat;
    }

    private static Material GarantirMaterialDaMalha()
    {
        Material mat = CarregarOuCriar(CaminhoMaterialMalha, "Universal Render Pipeline/Lit");
        mat.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(CaminhoTexturaMalha));
        mat.SetColor("_BaseColor", Color.white);
        mat.SetFloat("_Smoothness", 0.2f);
        mat.SetFloat("_Metallic", 0f);
        mat.SetFloat("_AlphaClip", 1f);
        mat.SetFloat("_Cutoff", 0.5f);
        mat.SetFloat("_Cull", 0f);
        mat.EnableKeyword("_ALPHATEST_ON");
        mat.renderQueue = 2450;
        mat.enableInstancing = true;
        EditorUtility.SetDirty(mat);
        return mat;
    }

    private static void GarantirMaterialDeParticula()
    {
        Material mat = CarregarOuCriar(CaminhoMaterialParticula, "Sprites/Default");
        mat.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(CaminhoTexturaParticula);
        EditorUtility.SetDirty(mat);
    }

    private static Material CarregarOuCriar(string caminho, string shader)
    {
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(caminho);
        if (mat == null)
        {
            mat = new Material(Shader.Find(shader));
            AssetDatabase.CreateAsset(mat, caminho);
        }
        return mat;
    }
}
