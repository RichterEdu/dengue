using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

/* ============================================================
 *  PiscinaBuilder.cs  (Editor)
 *  ---------------------------------------------------------------------
 *  Gera o asset "Piscina de Plástico" — aquela piscininha azul de
 *  criança esquecida no quintal, com água parada esverdeada.
 *
 *  Menu:  Dengue > Criar Piscina de Plastico
 *
 *  - Malhas de REVOLUÇÃO (torno), mesma técnica do Vaso e da Caixa
 *    d'Água: parede que abre para cima, rolo na borda e parede interna
 *    descendo até o fundo, que é onde a lâmina d'água aparece.
 *  - O foco NÃO é esvaziar: é VIRAR. O PiscinaInteractable tomba a
 *    piscina em volta da borda apoiada no chão, um pedaço por clique,
 *    até ela ficar emborcada de boca para baixo.
 *  - Cria em Assets/Meus/Assets Gerados/Piscina de Plastico/:
 *      Malha Piscina/Agua.asset
 *      piscina azul.mat / agua parada.mat  (URP Lit)
 *      Piscina de Plastico.prefab  (PiscinaInteractable + FocoHUDInfo)
 *  - Pode rodar de novo: reaproveita os arquivos e mantém os GUIDs.
 *
 *  Unidades: metros. Piscina de ~1,06 m de diâmetro por 29 cm de
 *  altura. O pivô fica no CENTRO DA BASE: é só largar no chão.
 * ============================================================ */

public static class PiscinaBuilder
{
    private static readonly string Pasta = MeusOrganizador.Destino + "/Piscina de Plastico";
    private static string PrefabPath => Pasta + "/Piscina de Plastico.prefab";

    private const int Segmentos = 30;

    /* Raio onde a borda toca o chão: é em volta deste eixo que a piscina
     * tomba. O PiscinaInteractable ainda encosta o ponto mais baixo no
     * chão a cada frame, então ela rola sobre a borda em vez de afundar. */
    private const float RaioDaBorda = 0.41f;

    /* ---- Perfis (raio, altura) ----
     * Regra do torno: percorra o contorno deixando o "material" à
     * esquerda; a normal sai à direita do sentido de caminhada. */

    // Fundo -> parede externa abrindo -> rolo da borda -> parede interna
    // descendo -> fundo interno (onde a água fica).
    private static readonly Vector2[] PiscinaProfile =
    {
        new Vector2(0.000f, 0.000f), new Vector2(0.410f, 0.000f), new Vector2(0.448f, 0.026f),
        new Vector2(0.470f, 0.080f), new Vector2(0.496f, 0.190f), new Vector2(0.512f, 0.246f),
        new Vector2(0.528f, 0.268f), new Vector2(0.524f, 0.286f), new Vector2(0.502f, 0.290f),
        new Vector2(0.488f, 0.274f), new Vector2(0.480f, 0.240f), new Vector2(0.452f, 0.110f),
        new Vector2(0.420f, 0.030f), new Vector2(0.000f, 0.018f),
    };

    // Lâmina de água parada, mais ou menos pela metade da piscina.
    private static readonly Vector2[] AguaProfile =
    {
        new Vector2(0.455f, 0.150f), new Vector2(0.000f, 0.150f),
    };

    [MenuItem("Dengue/Criar Piscina de Plastico")]
    public static void Build()
    {
        MeusOrganizador.GarantirPasta(Pasta);

        /* ---------- Malhas ---------- */
        Mesh piscina = SalvarMalha(Torno("Piscina", PiscinaProfile));
        Mesh agua = SalvarMalha(Torno("Agua", AguaProfile));
        AssetDatabase.SaveAssets();

        /* ---------- Materiais ---------- */
        Material azul = Mat("piscina azul", new Color(0.20f, 0.55f, 0.82f), 0.45f);
        Material agualenta = Mat("agua parada", new Color(0.29f, 0.47f, 0.31f), 0.88f);

        /* ---------- Hierarquia ---------- */
        GameObject raiz = new GameObject("Piscina de Plastico");
        try
        {
            // O CORPO é quem tomba — por isso o collider vai nele, e não
            // na raiz: assim o jogador continua acertando a piscina
            // enquanto ela vira.
            GameObject corpo = Peca(raiz, "Corpo", piscina, azul);
            BoxCollider box = corpo.AddComponent<BoxCollider>();
            box.center = new Vector3(0f, 0.145f, 0f);
            box.size = new Vector3(1.056f, 0.290f, 1.056f);

            GameObject aguaGo = Peca(corpo, "agua da piscina", agua, agualenta);

            PiscinaInteractable foco = raiz.AddComponent<PiscinaInteractable>();
            SerializedObject so = new SerializedObject(foco);
            so.FindProperty("requiredClicks").intValue = 4;
            so.FindProperty("punchScale").floatValue = 0.04f;
            so.FindProperty("corpo").objectReferenceValue = corpo.transform;
            so.FindProperty("agua").objectReferenceValue = aguaGo;
            so.FindProperty("raioDaBorda").floatValue = RaioDaBorda;
            so.ApplyModifiedPropertiesWithoutUndo();

            FocoHUDInfo info = raiz.AddComponent<FocoHUDInfo>();
            info.nome = "a piscina de plástico";
            info.textoAoMirar = "Clique para virar {nome}";
            info.dica = "Não basta esvaziar: guarde de boca para baixo, senão a próxima chuva enche de novo.";
            // textoAoEliminar fica vazio de propósito: usa o padrão "Foco eliminado!"
            // do DengueHUD, igual ao pneu — só a dica acima muda por tipo de foco.

            PrefabUtility.SaveAsPrefabAsset(raiz, PrefabPath, out bool ok);
            if (ok)
            {
                AssetDatabase.SaveAssets();
                Object prefab = AssetDatabase.LoadAssetAtPath<Object>(PrefabPath);
                EditorGUIUtility.PingObject(prefab);
                Selection.activeObject = prefab;
                Debug.Log("[Dengue] Prefab criado: " + PrefabPath + " — arraste para a cena. " +
                          "São 4 cliques: a água escorre no começo do tombo e a piscina termina emborcada.");
            }
            else
            {
                Debug.LogError("[Dengue] Falha ao salvar o prefab em " + PrefabPath);
            }
        }
        finally
        {
            Object.DestroyImmediate(raiz);
        }
    }

    /* ================= Geração de malha ================= */

    private static Mesh Torno(string nome, Vector2[] perfil)
    {
        List<Vector3> verts = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> tris = new List<int>();

        float total = 0f;
        for (int i = 0; i < perfil.Length - 1; i++)
            total += Vector2.Distance(perfil[i], perfil[i + 1]);
        float acc = 0f;

        for (int i = 0; i < perfil.Length - 1; i++)
        {
            Vector2 p0 = perfil[i];
            Vector2 p1 = perfil[i + 1];
            Vector2 d = p1 - p0;
            float len = d.magnitude;
            if (len < 1e-6f)
                continue;

            float nr = d.y / len;
            float ny = -d.x / len;
            float v0 = acc / total;
            acc += len;
            float v1 = acc / total;

            int start = verts.Count;
            for (int s = 0; s <= Segmentos; s++)
            {
                float u = (float)s / Segmentos;
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

            for (int s = 0; s < Segmentos; s++)
            {
                int a0 = start + 2 * s;
                int a1 = a0 + 1;
                int b0 = a0 + 2;
                int b1 = a0 + 3;
                tris.Add(a0); tris.Add(a1); tris.Add(b0);
                tris.Add(a1); tris.Add(b1); tris.Add(b0);
            }
        }

        Mesh malha = new Mesh { name = nome };
        malha.SetVertices(verts);
        malha.SetNormals(normals);
        malha.SetUVs(0, uvs);
        malha.SetTriangles(tris, 0);
        malha.RecalculateBounds();
        malha.RecalculateTangents();
        return malha;
    }

    /* ================= Assets ================= */

    private static Mesh SalvarMalha(Mesh malha)
    {
        string path = Pasta + "/Malha " + malha.name + ".asset";
        Mesh antiga = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (antiga == null)
        {
            AssetDatabase.CreateAsset(malha, path);
            return malha;
        }

        string nome = antiga.name;
        EditorUtility.CopySerialized(malha, antiga);
        antiga.name = nome;
        EditorUtility.SetDirty(antiga);
        Object.DestroyImmediate(malha);
        return antiga;
    }

    private static Material Mat(string nome, Color cor, float smoothness)
    {
        string path = Pasta + "/" + nome + ".mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(ShaderLit());
            AssetDatabase.CreateAsset(mat, path);
        }

        mat.name = nome;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", cor);
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", cor);
        if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
        if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", smoothness);
        EditorUtility.SetDirty(mat);
        return mat;
    }

    private static Shader ShaderLit()
    {
        RenderPipelineAsset rp = GraphicsSettings.currentRenderPipeline;
        string tipo = rp != null ? rp.GetType().Name : "";

        Shader s = null;
        if (tipo.Contains("HDRenderPipeline")) s = Shader.Find("HDRP/Lit");
        if (s == null) s = Shader.Find("Universal Render Pipeline/Lit");
        if (s == null) s = Shader.Find("HDRP/Lit");
        if (s == null) s = Shader.Find("Standard");
        return s;
    }

    private static GameObject Peca(GameObject pai, string nome, Mesh malha, Material mat)
    {
        GameObject go = new GameObject(nome);
        go.transform.SetParent(pai.transform, false);
        go.AddComponent<MeshFilter>().sharedMesh = malha;
        go.AddComponent<MeshRenderer>().sharedMaterial = mat;
        return go;
    }
}
