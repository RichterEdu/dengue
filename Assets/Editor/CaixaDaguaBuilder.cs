using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

/* ============================================================
 *  CaixaDaguaBuilder.cs  (Editor)
 *  ---------------------------------------------------------------------
 *  Gera o asset "Caixa d'Água" (caixa de polietileno redonda, daquelas
 *  azuis de telhado) direto no Unity, sem precisar de modelo externo.
 *
 *  Menu:  Dengue > Criar Caixa d'Agua
 *
 *  - As peças são malhas de REVOLUÇÃO (torno): um perfil 2D girado em
 *    volta do eixo Y — mesma técnica do Vaso de Flor. Por isso a caixa
 *    tem parede cônica, nervura de reforço, lábio na boca e a tampa
 *    tem domo com saia que encaixa por fora.
 *  - A caixa nasce ABERTA: a tampa fica encostada na lateral e a lâmina
 *    d'água aparece lá dentro. O jogador clica e a tampa vai assentando.
 *  - Cria em Assets/Meus/Assets Gerados/Caixa Dagua/:
 *      Malha Corpo/Tampa/Puxador/Agua.asset
 *      Materiais .mat (URP Lit)
 *      Caixa d'Agua.prefab  (com CaixaDaguaInteractable + FocoHUDInfo)
 *  - Pode rodar de novo quantas vezes quiser: sobrescreve os arquivos
 *    mantendo os GUIDs (referências na cena continuam funcionando).
 *
 *  Unidades: metros. Caixa de ~94 cm de diâmetro por 77 cm de altura
 *  (uma 500 litros). O pivô fica no CENTRO DA BASE: é só largar no chão.
 * ============================================================ */

public static class CaixaDaguaBuilder
{
    private const string Folder = "Assets/Meus/Assets Gerados/Caixa Dagua";
    private const string PrefabPath = Folder + "/Caixa d'Agua.prefab";
    private const string ExistingWaterMatPath = "Assets/Meus/agua do pneu.mat";

    private const int Segments = 28;

    /* ---- Perfis (raio, altura) ----
     * Regra do torno: percorra o contorno deixando o "material" à
     * esquerda; a normal sai à direita do sentido de caminhada. */

    // Corpo: fundo -> parede cônica -> nervura -> lábio da boca -> parede
    // interna descendo -> fundo interno (onde a água aparece).
    private static readonly Vector2[] CorpoProfile =
    {
        new Vector2(0.000f, 0.000f), new Vector2(0.360f, 0.000f), new Vector2(0.395f, 0.030f),
        new Vector2(0.420f, 0.090f), new Vector2(0.440f, 0.300f), new Vector2(0.452f, 0.560f),
        new Vector2(0.452f, 0.595f), new Vector2(0.472f, 0.625f), new Vector2(0.472f, 0.660f),
        new Vector2(0.450f, 0.690f), new Vector2(0.452f, 0.735f), new Vector2(0.465f, 0.752f),
        new Vector2(0.462f, 0.765f), new Vector2(0.430f, 0.762f), new Vector2(0.418f, 0.720f),
        new Vector2(0.405f, 0.480f), new Vector2(0.000f, 0.470f),
    };

    // Tampa: pivô no plano de assentamento (y = 0). Percorre a face de
    // baixo, desce a saia por dentro, contorna e sobe o domo.
    private static readonly Vector2[] TampaProfile =
    {
        new Vector2(0.000f, 0.064f), new Vector2(0.110f, 0.060f), new Vector2(0.240f, 0.042f),
        new Vector2(0.370f, 0.016f), new Vector2(0.440f, 0.004f), new Vector2(0.470f, -0.004f),
        new Vector2(0.470f, -0.048f), new Vector2(0.482f, -0.048f), new Vector2(0.482f, -0.006f),
        new Vector2(0.450f, 0.006f), new Vector2(0.380f, 0.024f), new Vector2(0.260f, 0.052f),
        new Vector2(0.120f, 0.070f), new Vector2(0.000f, 0.075f),
    };

    // Puxador no topo da tampa (coordenadas locais da tampa).
    private static readonly Vector2[] PuxadorProfile =
    {
        new Vector2(0.000f, 0.074f), new Vector2(0.038f, 0.074f), new Vector2(0.042f, 0.088f),
        new Vector2(0.034f, 0.102f), new Vector2(0.000f, 0.106f),
    };

    // Lâmina de água parada dentro da caixa.
    private static readonly Vector2[] AguaProfile =
    {
        new Vector2(0.402f, 0.478f), new Vector2(0.000f, 0.478f),
    };

    /* ---- Poses da tampa ---- */
    private static readonly Vector3 PosAberta = new Vector3(0.44f, 0.45f, 0f);
    private static readonly Vector3 RotAberta = new Vector3(0f, 0f, 62f);
    private static readonly Vector3 PosFechada = new Vector3(0f, 0.762f, 0f);
    private static readonly Vector3 RotFechada = Vector3.zero;

    [MenuItem("Dengue/Criar Caixa d'Agua")]
    public static void Build()
    {
        EnsureFolder(Folder);

        /* ---------- Malhas ---------- */
        Mesh corpo = SaveMesh(Lathe("Corpo", CorpoProfile));
        Mesh tampa = SaveMesh(Lathe("Tampa", TampaProfile));
        Mesh puxador = SaveMesh(Lathe("Puxador", PuxadorProfile));
        Mesh agua = SaveMesh(Lathe("Agua", AguaProfile));
        AssetDatabase.SaveAssets();

        /* ---------- Materiais ---------- */
        Material azul = GetOrCreateMaterial("caixa dagua", new Color(0.16f, 0.40f, 0.68f), 0.32f);
        Material azulEscuro = GetOrCreateMaterial("tampa da caixa", new Color(0.12f, 0.31f, 0.54f), 0.38f);
        Material water = AssetDatabase.LoadAssetAtPath<Material>(ExistingWaterMatPath);
        if (water == null)
            water = GetOrCreateMaterial("agua da caixa", new Color(0.29f, 0.56f, 0.79f), 0.9f);

        /* ---------- Hierarquia ---------- */
        GameObject root = new GameObject("Caixa d'Agua");
        try
        {
            AddPart(root, "Corpo", corpo, azul, Vector3.zero);
            GameObject aguaGo = AddPart(root, "agua da caixa", agua, water, Vector3.zero);

            GameObject tampaGo = AddPart(root, "Tampa", tampa, azulEscuro, PosAberta);
            tampaGo.transform.localRotation = Quaternion.Euler(RotAberta);
            AddPart(tampaGo, "Puxador", puxador, azulEscuro, Vector3.zero);

            // Um collider só na raiz, para o raycast do Interactor.
            CapsuleCollider capsule = root.AddComponent<CapsuleCollider>();
            capsule.radius = 0.47f;
            capsule.height = 0.80f;
            capsule.center = new Vector3(0f, 0.40f, 0f);

            CaixaDaguaInteractable caixa = root.AddComponent<CaixaDaguaInteractable>();
            SerializedObject so = new SerializedObject(caixa);
            so.FindProperty("requiredClicks").intValue = 4;
            so.FindProperty("punchScale").floatValue = 0.03f;   // caixa grande: punch discreto
            so.FindProperty("tampa").objectReferenceValue = tampaGo.transform;
            so.FindProperty("agua").objectReferenceValue = aguaGo;
            so.FindProperty("posAberta").vector3Value = PosAberta;
            so.FindProperty("rotAberta").vector3Value = RotAberta;
            so.FindProperty("posFechada").vector3Value = PosFechada;
            so.FindProperty("rotFechada").vector3Value = RotFechada;
            so.ApplyModifiedPropertiesWithoutUndo();

            FocoHUDInfo info = root.AddComponent<FocoHUDInfo>();
            info.nome = "a caixa d'água";
            info.textoAoMirar = "Clique para tampar {nome}";
            info.dica = "Caixa d'água destampada é criadouro certo. Tampe e vede bem as bordas.";
            // textoAoEliminar fica vazio de propósito: usa o padrão "Foco eliminado!"
            // do DengueHUD, igual ao pneu — só a dica acima muda por tipo de foco.

            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath, out bool ok);
            if (ok)
            {
                AssetDatabase.SaveAssets();
                Object prefab = AssetDatabase.LoadAssetAtPath<Object>(PrefabPath);
                EditorGUIUtility.PingObject(prefab);
                Selection.activeObject = prefab;
                Debug.Log("[Dengue] Prefab criado: " + PrefabPath + " — arraste para a cena. " +
                          "A caixa nasce aberta; 4 cliques assentam a tampa.");
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
