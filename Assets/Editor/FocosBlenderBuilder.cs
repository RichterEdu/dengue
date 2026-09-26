using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/* ============================================================
 *  FocosBlenderBuilder.cs  (Editor)
 *  ---------------------------------------------------------------------
 *  Monta os prefabs dos focos modelados no Blender
 *  (Ferramentas/Blender/gerar_focos.py -> Ralo.fbx, Bromelia.fbx).
 *
 *  Menus:  Dengue > Criar Ralo
 *          Dengue > Criar Bromélia
 *
 *  - Configura o import do FBX (eixo corrigido, sem animação) e troca os
 *    materiais do Blender por materiais URP Lit com as mesmas cores
 *    (Assets/Meus/Assets Gerados/Focos/Materiais/Foco_*.mat). A água usa
 *    o mesmo "agua do pneu.mat" dos outros focos.
 *  - Gera Ralo.prefab / Bromelia.prefab na mesma pasta, com o
 *    Interactable do tipo, FocoHUDInfo e um BoxCollider na raiz.
 *  - Pode rodar de novo: sobrescreve o prefab mantendo o GUID.
 *
 *  Pivô no chão (centro da base): é só largar na calçada/quintal.
 * ============================================================ */

public static class FocosBlenderBuilder
{
    private const string Pasta = "Assets/Meus/Assets Gerados/Focos";
    private const string PastaMateriais = Pasta + "/Materiais";
    private const string AguaMatPath = "Assets/Meus/agua do pneu.mat";

    // Mesmas cores do gerar_focos.py (sRGB) + suavidade/metálico.
    private static readonly (string nome, string hex, float suavidade, float metalico)[] Paleta =
    {
        ("Concreto", "#9A968E", 0.15f, 0f), ("ConcretoEscuro", "#6E6A63", 0.15f, 0f), ("Fundo", "#2E2B27", 0.2f, 0f),
        ("Grelha", "#4A4D52", 0.45f, 0.6f), ("Ferrugem", "#7A4A2A", 0.2f, 0.3f), ("Tela", "#2F6B3A", 0.25f, 0f),
        ("Folha", "#3F8A3A", 0.3f, 0f), ("FolhaClara", "#78B84A", 0.3f, 0f), ("Ponta", "#C2412F", 0.3f, 0f),
        ("Flor", "#E0435A", 0.35f, 0f), ("Terracota", "#B25228", 0.15f, 0f), ("Terra", "#4A3222", 0.05f, 0f),
    };

    // Altura da boca do ralo (ALT do gerar_focos.py) + folga para a grelha não brigar com a moldura.
    private const float AlturaGrelhaFechada = 0.034f;

    [MenuItem("Dengue/Criar Ralo")]
    public static void CriarRalo()
    {
        GameObject root = Instanciar("Ralo");
        if (root == null)
            return;
        try
        {
            Transform tampa = Achar(root.transform, "Tampa");
            GameObject agua = Achar(root.transform, "Agua")?.gameObject;
            GameObject tela = Achar(root.transform, "Tela")?.gameObject;
            if (tampa == null || agua == null || tela == null)
            {
                Debug.LogError("[Dengue] Ralo.fbx sem Tampa/Agua/Tela. Gere de novo pelo gerar_focos.py.");
                return;
            }
            tela.SetActive(false);

            AdicionarCollider(root, 0.10f);

            RaloInteractable ralo = root.AddComponent<RaloInteractable>();
            SerializedObject so = new SerializedObject(ralo);
            so.FindProperty("requiredClicks").intValue = 3;
            so.FindProperty("punchScale").floatValue = 0.04f;
            so.FindProperty("tampa").objectReferenceValue = tampa;
            so.FindProperty("agua").objectReferenceValue = agua;
            so.FindProperty("tela").objectReferenceValue = tela;
            // A pose ABERTA é a que veio do Blender; a FECHADA é centrada na boca.
            so.FindProperty("posAberta").vector3Value = tampa.localPosition;
            so.FindProperty("rotAberta").vector3Value = tampa.localEulerAngles;
            so.FindProperty("posFechada").vector3Value = new Vector3(0f, AlturaGrelhaFechada, 0f);
            so.FindProperty("rotFechada").vector3Value = Vector3.zero;
            so.ApplyModifiedPropertiesWithoutUndo();

            FocoHUDInfo info = root.AddComponent<FocoHUDInfo>();
            info.nome = "o ralo";
            info.textoAoMirar = "Clique para tampar {nome}";
            info.dica = "Ralo aberto junta água parada. Mantenha fechado ou com tela e limpe toda semana.";

            SalvarPrefab(root, "Ralo", "3 cliques recolocam a grelha e põem a tela.");
        }
        finally
        {
            Object.DestroyImmediate(root);
        }
    }

    [MenuItem("Dengue/Criar Bromélia")]
    public static void CriarBromelia()
    {
        GameObject root = Instanciar("Bromelia");
        if (root == null)
            return;
        try
        {
            // Ordem de secar: as pocinhas das folhas primeiro, o copo central por último.
            List<GameObject> pocas = new List<GameObject>();
            for (int i = 1; i <= 4; i++)
            {
                Transform p = Achar(root.transform, "Agua_" + i);
                if (p != null) pocas.Add(p.gameObject);
            }
            Transform centro = Achar(root.transform, "Agua_Centro");
            if (centro != null) pocas.Add(centro.gameObject);
            if (pocas.Count == 0)
            {
                Debug.LogError("[Dengue] Bromelia.fbx sem as poças Agua_*. Gere de novo pelo gerar_focos.py.");
                return;
            }

            AdicionarCollider(root, 0f);

            BromeliaInteractable brom = root.AddComponent<BromeliaInteractable>();
            SerializedObject so = new SerializedObject(brom);
            so.FindProperty("requiredClicks").intValue = pocas.Count;
            so.FindProperty("punchScale").floatValue = 0.06f;
            SerializedProperty lista = so.FindProperty("pocas");
            lista.arraySize = pocas.Count;
            for (int i = 0; i < pocas.Count; i++)
                lista.GetArrayElementAtIndex(i).objectReferenceValue = pocas[i];
            so.ApplyModifiedPropertiesWithoutUndo();

            FocoHUDInfo info = root.AddComponent<FocoHUDInfo>();
            info.nome = "a bromélia";
            info.textoAoMirar = "Clique para esvaziar {nome}";
            info.dica = "Bromélia guarda água entre as folhas. Esvazie com um jato d'água toda semana.";

            SalvarPrefab(root, "Bromelia", pocas.Count + " cliques secam a água das folhas.");
        }
        finally
        {
            Object.DestroyImmediate(root);
        }
    }

    /* ================= Montagem ================= */

    /* Configura o import e devolve uma cópia desempacotada do modelo. */
    private static GameObject Instanciar(string nome)
    {
        string fbxPath = Pasta + "/" + nome + ".fbx";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath) == null)
        {
            EditorUtility.DisplayDialog("Focos do Blender", "Não encontrei " + fbxPath + ".", "OK");
            return null;
        }

        ConfigurarImportacao(fbxPath, CriarMateriais());

        GameObject modelo = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
        GameObject root = (GameObject)PrefabUtility.InstantiatePrefab(modelo);
        PrefabUtility.UnpackPrefabInstance(root, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        root.name = nome;
        root.transform.position = Vector3.zero;
        root.transform.rotation = Quaternion.identity;
        return root;
    }

    private static Dictionary<string, Material> CriarMateriais()
    {
        if (!AssetDatabase.IsValidFolder(Pasta))
            AssetDatabase.CreateFolder("Assets/Meus/Assets Gerados", "Focos");
        if (!AssetDatabase.IsValidFolder(PastaMateriais))
            AssetDatabase.CreateFolder(Pasta, "Materiais");

        Shader lit = Shader.Find("Universal Render Pipeline/Lit");
        var materiais = new Dictionary<string, Material>();
        foreach (var (nome, hex, suavidade, metalico) in Paleta)
        {
            string caminho = PastaMateriais + "/Foco_" + nome + ".mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(caminho);
            if (mat == null)
            {
                mat = new Material(lit);
                AssetDatabase.CreateAsset(mat, caminho);
            }
            ColorUtility.TryParseHtmlString(hex, out Color cor);
            mat.shader = lit;
            mat.SetColor("_BaseColor", cor);
            mat.SetColor("_Color", cor);
            mat.SetFloat("_Smoothness", suavidade);
            mat.SetFloat("_Metallic", metalico);
            mat.enableInstancing = true;
            EditorUtility.SetDirty(mat);
            materiais[nome] = mat;
        }

        Material agua = AssetDatabase.LoadAssetAtPath<Material>(AguaMatPath);
        if (agua != null)
            materiais["Agua"] = agua;

        AssetDatabase.SaveAssets();
        return materiais;
    }

    private static void ConfigurarImportacao(string fbxPath, Dictionary<string, Material> materiais)
    {
        var imp = (ModelImporter)AssetImporter.GetAtPath(fbxPath);
        imp.globalScale = 1f;
        imp.useFileScale = true;
        imp.bakeAxisConversion = true; // sem isso o Blender entra com -90° em X
        imp.importCameras = false;
        imp.importLights = false;
        imp.importBlendShapes = false;
        imp.importAnimation = false;
        imp.animationType = ModelImporterAnimationType.None;
        imp.addCollider = false;
        imp.preserveHierarchy = true; // sem isso o Unity funde o nó raiz e perde a correção de eixo
        imp.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
        foreach (var par in materiais)
            imp.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), par.Key), par.Value);
        imp.SaveAndReimport();
    }

    /* Um BoxCollider na raiz, cobrindo tudo o que é visível (inclusive a
     * grelha tirada ao lado), para o raycast do Interactor pegar fácil. */
    private static void AdicionarCollider(GameObject root, float alturaMinima)
    {
        Renderer[] rs = root.GetComponentsInChildren<Renderer>(true);
        if (rs.Length == 0)
            return;
        Bounds b = rs[0].bounds;
        foreach (Renderer r in rs)
            b.Encapsulate(r.bounds);

        BoxCollider box = root.AddComponent<BoxCollider>();
        Vector3 size = b.size;
        Vector3 center = b.center - root.transform.position;
        if (size.y < alturaMinima)
        {
            center.y += (alturaMinima - size.y) * 0.5f;
            size.y = alturaMinima;
        }
        box.center = center;
        box.size = size;
    }

    private static Transform Achar(Transform t, string nome)
    {
        if (t.name == nome)
            return t;
        foreach (Transform filho in t)
        {
            Transform r = Achar(filho, nome);
            if (r != null)
                return r;
        }
        return null;
    }

    private static void SalvarPrefab(GameObject root, string nome, string detalhe)
    {
        string path = Pasta + "/" + nome + ".prefab";
        PrefabUtility.SaveAsPrefabAsset(root, path, out bool ok);
        if (!ok)
        {
            Debug.LogError("[Dengue] Falha ao salvar o prefab em " + path);
            return;
        }
        AssetDatabase.SaveAssets();
        Object prefab = AssetDatabase.LoadAssetAtPath<Object>(path);
        EditorGUIUtility.PingObject(prefab);
        Selection.activeObject = prefab;
        Debug.Log("[Dengue] Prefab criado: " + path + " — arraste para a cena. " + detalhe);
    }
}
