using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

/* ============================================================
 *  VasoComPratoBuilder.cs  (Editor)
 *  ---------------------------------------------------------------------
 *  Gera o asset "Vaso de Flor" (vaso de barro + pratinho embaixo) direto
 *  no Unity, sem precisar de modelo externo.
 *
 *  Menu:  Dengue > Criar Vaso de Flor com Pratinho
 *
 *  - As peças são malhas de REVOLUÇÃO (torno): um perfil 2D girado em
 *    volta do eixo Y. Por isso o vaso tem forma de verdade (afunila,
 *    tem borda/lábio) e o prato tem aba levantada.
 *  - Cria em Assets/Meus/Assets Gerados/Vaso de Flor/:
 *      Malha Vaso/Prato/Terra/Agua/Areia.asset
 *      Materiais .mat (URP Lit)
 *      Vaso de Flor.prefab          (com VasoInteractable configurado)
 *  - Pode rodar de novo quantas vezes quiser: sobrescreve os arquivos
 *    mantendo os GUIDs (referências na cena continuam funcionando).
 *
 *  Unidades: metros. Vaso ~44 cm de altura, prato ~54 cm de diâmetro.
 *  Para mudar o tamanho, basta ajustar a Scale da raiz do prefab.
 * ============================================================ */

public static class VasoComPratoBuilder
{
    private const string Folder = "Assets/Meus/Assets Gerados/Vaso de Flor";
    private const string PrefabPath = Folder + "/Vaso de Flor.prefab";
    private const string FlowerPrefabPath = "Assets/SimpleNaturePack/Prefabs/Flowers_01.prefab";
    private const string ExistingWaterMatPath = "Assets/Meus/agua do pneu.mat";

    private const int Segments = 48;

    /* ---- Perfis (raio, altura) de fora para dentro ---- */

    // Vaso: fundo -> parede que afunila -> degrau -> lábio -> parede interna -> nível da terra
    private static readonly Vector2[] PotProfile =
    {
        new Vector2(0.000f, 0.030f), new Vector2(0.140f, 0.030f), new Vector2(0.150f, 0.040f),
        new Vector2(0.195f, 0.345f), new Vector2(0.200f, 0.352f), new Vector2(0.222f, 0.356f),
        new Vector2(0.228f, 0.365f), new Vector2(0.228f, 0.432f), new Vector2(0.222f, 0.440f),
        new Vector2(0.200f, 0.440f), new Vector2(0.194f, 0.432f), new Vector2(0.186f, 0.395f),
        new Vector2(0.000f, 0.395f),
    };

    // Pratinho: base -> aba externa inclinada -> borda -> aba interna -> fundo (piso do prato em y=0.022)
    private static readonly Vector2[] SaucerProfile =
    {
        new Vector2(0.000f, 0.000f), new Vector2(0.225f, 0.000f), new Vector2(0.235f, 0.006f),
        new Vector2(0.268f, 0.068f), new Vector2(0.268f, 0.076f), new Vector2(0.258f, 0.076f),
        new Vector2(0.252f, 0.068f), new Vector2(0.222f, 0.022f), new Vector2(0.000f, 0.022f),
    };

    // Terra dentro do vaso (levemente abaulada)
    private static readonly Vector2[] SoilProfile =
    {
        new Vector2(0.187f, 0.398f), new Vector2(0.120f, 0.405f),
        new Vector2(0.050f, 0.412f), new Vector2(0.000f, 0.413f),
    };

    // Lâmina de água parada no prato
    private static readonly Vector2[] WaterProfile =
    {
        new Vector2(0.250f, 0.048f), new Vector2(0.000f, 0.048f),
    };

    // Areia (pivô no piso do prato; o VasoInteractable escala em Y para "encher")
    private static readonly Vector2[] SandProfile =
    {
        new Vector2(0.215f, 0.000f), new Vector2(0.247f, 0.050f), new Vector2(0.200f, 0.056f),
        new Vector2(0.150f, 0.058f), new Vector2(0.000f, 0.060f),
    };

    private const float SaucerFloorY = 0.022f;

    [MenuItem("Dengue/Criar Vaso de Flor com Pratinho")]
    public static void Build()
    {
        EnsureFolder(Folder);

        /* ---------- Malhas ---------- */
        Mesh potMesh = Lathe("Vaso", PotProfile);
        Mesh saucerMesh = Lathe("Prato", SaucerProfile);
        Mesh soilMesh = Lathe("Terra", SoilProfile);
        Mesh waterMesh = Lathe("Agua", WaterProfile);
        Mesh sandMesh = Lathe("Areia", SandProfile);

        potMesh = SaveMesh(potMesh);
        saucerMesh = SaveMesh(saucerMesh);
        soilMesh = SaveMesh(soilMesh);
        waterMesh = SaveMesh(waterMesh);
        sandMesh = SaveMesh(sandMesh);
        AssetDatabase.SaveAssets();

        /* ---------- Materiais ---------- */
        Material clay = GetOrCreateMaterial("barro do vaso", new Color(0.78f, 0.40f, 0.23f), 0.15f);
        Material clayDark = GetOrCreateMaterial("barro do prato", new Color(0.66f, 0.31f, 0.16f), 0.2f);
        Material soil = GetOrCreateMaterial("terra do vaso", new Color(0.23f, 0.14f, 0.08f), 0.05f);
        Material sand = GetOrCreateMaterial("areia do prato", new Color(0.86f, 0.76f, 0.55f), 0.05f);
        Material water = AssetDatabase.LoadAssetAtPath<Material>(ExistingWaterMatPath);
        if (water == null)
            water = GetOrCreateMaterial("agua do prato", new Color(0.29f, 0.56f, 0.79f), 0.9f);

        /* ---------- Hierarquia ---------- */
        GameObject root = new GameObject("Vaso de Flor");
        try
        {
            AddPart(root, "Prato", saucerMesh, clayDark, Vector3.zero);
            AddPart(root, "Vaso", potMesh, clay, Vector3.zero);
            AddPart(root, "Terra", soilMesh, soil, Vector3.zero);
            GameObject waterGo = AddPart(root, "agua do prato", waterMesh, water, Vector3.zero);
            GameObject sandGo = AddPart(root, "areia do prato", sandMesh, sand, new Vector3(0f, SaucerFloorY, 0f));
            sandGo.SetActive(false);

            AddPlant(root);

            // Um único collider na raiz para o raycast do Interactor.
            BoxCollider box = root.AddComponent<BoxCollider>();
            box.center = new Vector3(0f, 0.22f, 0f);
            box.size = new Vector3(0.54f, 0.44f, 0.54f);

            VasoInteractable vaso = root.AddComponent<VasoInteractable>();
            SerializedObject so = new SerializedObject(vaso);
            so.FindProperty("requiredClicks").intValue = 3;
            so.FindProperty("water").objectReferenceValue = waterGo;
            so.FindProperty("sand").objectReferenceValue = sandGo.transform;
            so.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath, out bool ok);
            if (ok)
            {
                AssetDatabase.SaveAssets();
                Object prefab = AssetDatabase.LoadAssetAtPath<Object>(PrefabPath);
                EditorGUIUtility.PingObject(prefab);
                Selection.activeObject = prefab;
                Debug.Log("[Dengue] Prefab criado: " + PrefabPath + " — arraste para a cena.");
            }
            else
            {
                Debug.LogError("[Dengue] Falha ao salvar o prefab em " + PrefabPath);
            }
        }
        finally
        {
            Object.DestroyImmediate(root);
        }
    }

    /* ================= Planta ================= */

    private static void AddPlant(GameObject root)
    {
        GameObject flowerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(FlowerPrefabPath);
        if (flowerPrefab == null)
        {
            Debug.LogWarning("[Dengue] Flowers_01.prefab não encontrado — vaso criado sem planta.");
            return;
        }

        GameObject plant = (GameObject)PrefabUtility.InstantiatePrefab(flowerPrefab);
        // Desempacota: vira uma cópia comum (usa a mesma malha/material do pacote),
        // assim dá para remover componentes sem mexer no prefab original.
        PrefabUtility.UnpackPrefabInstance(plant, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        plant.name = "Planta";
        plant.transform.SetParent(root.transform, false);

        // Remove colliders da planta (o collider da raiz já cobre o vaso).
        foreach (Collider c in plant.GetComponentsInChildren<Collider>(true))
            Object.DestroyImmediate(c);

        // Ajusta a escala para a planta caber na boca do vaso (~35 cm de altura).
        Bounds b = RendererBounds(plant);
        if (b.size.y > 0.0001f)
        {
            float targetHeight = 0.35f;
            float byHeight = targetHeight / b.size.y;
            float byWidth = 0.40f / Mathf.Max(b.size.x, b.size.z);
            plant.transform.localScale = Vector3.one * Mathf.Min(byHeight, byWidth);
            b = RendererBounds(plant);
        }

        // Base da planta apoiada na terra, centralizada.
        Vector3 p = plant.transform.localPosition;
        p.x -= b.center.x;
        p.z -= b.center.z;
        p.y += 0.395f - b.min.y;
        plant.transform.localPosition = p;
    }

    private static Bounds RendererBounds(GameObject go)
    {
        Renderer[] rs = go.GetComponentsInChildren<Renderer>();
        if (rs.Length == 0)
            return new Bounds(go.transform.position, Vector3.zero);
        Bounds b = rs[0].bounds;
        for (int i = 1; i < rs.Length; i++)
            b.Encapsulate(rs[i].bounds);
        return b;
    }

    /* ================= Geração de malha ================= */

    /* Gira o perfil em torno de Y. Cada trecho do perfil tem seus próprios
     * vértices (cantos definidos no perfil, liso ao redor do eixo). */
    private static Mesh Lathe(string name, Vector2[] profile)
    {
        List<Vector3> verts = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> tris = new List<int>();

        float totalLen = 0f;
        for (int i = 0; i < profile.Length - 1; i++)
            totalLen += Vector2.Distance(profile[i], profile[i + 1]);
        float accLen = 0f;

        for (int i = 0; i < profile.Length - 1; i++)
        {
            Vector2 p0 = profile[i];
            Vector2 p1 = profile[i + 1];
            Vector2 d = p1 - p0;
            float len = d.magnitude;
            if (len < 1e-6f)
                continue;

            float nr = d.y / len;
            float ny = -d.x / len;
            float v0 = accLen / totalLen;
            accLen += len;
            float v1 = accLen / totalLen;

            int start = verts.Count;
            for (int s = 0; s <= Segments; s++)
            {
                float u = (float)s / Segments;
                float a = u * Mathf.PI * 2f;
                float cos = Mathf.Cos(a);
                float sin = Mathf.Sin(a);

                verts.Add(new Vector3(p0.x * cos, p0.y, p0.x * sin));
                normals.Add(new Vector3(nr * cos, ny, nr * sin));
                uvs.Add(new Vector2(u, v0));

                verts.Add(new Vector3(p1.x * cos, p1.y, p1.x * sin));
                normals.Add(new Vector3(nr * cos, ny, nr * sin));
                uvs.Add(new Vector2(u, v1));
            }

            for (int s = 0; s < Segments; s++)
            {
                int a0 = start + 2 * s;
                int a1 = a0 + 1;
                int b0 = a0 + 2;
                int b1 = a0 + 3;
                tris.Add(a0); tris.Add(a1); tris.Add(b0);
                tris.Add(a1); tris.Add(b1); tris.Add(b0);
            }
        }

        Mesh mesh = new Mesh { name = name };
        mesh.SetVertices(verts);
        mesh.SetNormals(normals);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();
        return mesh;
    }

    /* ================= Assets ================= */

    /* Cada malha vira um .asset próprio. Se já existir, reaproveita o arquivo
     * (CopySerialized) para manter o GUID e as referências do prefab/cena. */
    private static Mesh SaveMesh(Mesh mesh)
    {
        string path = Folder + "/Malha " + mesh.name + ".asset";
        Mesh old = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (old == null)
        {
            AssetDatabase.CreateAsset(mesh, path);
            return mesh;
        }

        string keepName = old.name;
        EditorUtility.CopySerialized(mesh, old);
        old.name = keepName;
        EditorUtility.SetDirty(old);
        Object.DestroyImmediate(mesh);
        return old;
    }

    private static Material GetOrCreateMaterial(string name, Color color, float smoothness)
    {
        string path = Folder + "/" + name + ".mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(FindLitShader());
            AssetDatabase.CreateAsset(mat, path);
        }

        mat.name = name;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
        if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
        if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", smoothness);
        EditorUtility.SetDirty(mat);
        return mat;
    }

    private static Shader FindLitShader()
    {
        RenderPipelineAsset rp = GraphicsSettings.currentRenderPipeline;
        string rpType = rp != null ? rp.GetType().Name : "";

        Shader s = null;
        if (rpType.Contains("HDRenderPipeline")) s = Shader.Find("HDRP/Lit");
        if (s == null) s = Shader.Find("Universal Render Pipeline/Lit");
        if (s == null) s = Shader.Find("HDRP/Lit");
        if (s == null) s = Shader.Find("Standard");
        return s;
    }

    private static GameObject AddPart(GameObject root, string name, Mesh mesh, Material mat, Vector3 localPos)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(root.transform, false);
        go.transform.localPosition = localPos;
        go.AddComponent<MeshFilter>().sharedMesh = mesh;
        go.AddComponent<MeshRenderer>().sharedMaterial = mat;
        return go;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
            return;
        string parent = Path.GetDirectoryName(path).Replace('\\', '/');
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
    }
}
