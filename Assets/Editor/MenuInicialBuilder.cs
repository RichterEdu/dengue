using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static MenuPausaBuilder;

/* ============================================================
 *  MenuInicialBuilder.cs  (Editor)
 *  ---------------------------------------------------------------------
 *  Monta a cena do MENU INICIAL (Assets/Scenes/MenuInicial.unity):
 *
 *    --- CENARIO ---     cópia VISUAL do bairro da SampleScene (sem
 *                        scripts, colisores nem áudio). As malhas que
 *                        estavam embutidas na cena (cercas) são combinadas
 *                        por material em CenarioMalhas.asset.
 *    --- ILUMINACAO ---  sol de fim de tarde + neblina (só no menu)
 *    --- CAMERA ---      Main Camera com CameraMenu.cs (3 tomadas)
 *    --- UI ---          MenuInicialCanvas (MenuInicial.cs) + EventSystem
 *
 *  Também põe o MenuInicial como cena 0 do Build Settings (SampleScene 1).
 *
 *  Menu:  Dengue > Criar Menu Inicial
 *
 *  REFAZ a cena do zero a cada execução — nada nela é editado à mão. A
 *  SampleScene só é LIDA (se não estiver aberta, abre por trás e fecha
 *  sem salvar). Mudou o bairro? Rode de novo.
 *
 *  Layout, cores, tomadas e luz vêm do preview aprovado pelo usuário.
 *  As peças da UI (cartão, botões, chips, tela de Configurações) são as
 *  do MenuPausaBuilder.cs.
 * ============================================================ */

public static class MenuInicialBuilder
{
    private const string CaminhoCena = "Assets/Scenes/MenuInicial.unity";
    private const string CaminhoJogo = "Assets/Scenes/SampleScene.unity";
    private const string PastaPai = "Assets/Meus/Assets Gerados";
    private const string Pasta = PastaPai + "/Menu Inicial";
    private const string CaminhoMalhas = Pasta + "/CenarioMalhas.asset";
    private const string CaminhoDegrade = Pasta + "/Degrade.png";
    private const string CaminhoDegradeBaixo = Pasta + "/DegradeBaixo.png";

    private static readonly string[] GruposCopiados = { "--- BAIRRO ---", "--- FOCOS ---" };

    /* ---------- Título (tela cinematográfica) ---------- */
    private const float Margem = 120f;
    private const string Titulo = "FOCO <color=#E04848>ZERO</color>";
    private static readonly Color Escuro = new Color(0.03f, 0.04f, 0.05f, 1f);
    private static readonly Color CorVeuMenu = new Color(0.02f, 0.03f, 0.04f, 0.55f);
    private static readonly Color BrancoRotulo = new Color(1f, 1f, 1f, 0.78f);
    private static readonly Color BrancoSubtitulo = new Color(1f, 1f, 1f, 0.82f);
    private static readonly Color BrancoBotao = new Color(1f, 1f, 1f, 0.74f);
    private static readonly Color BrancoDestaque = new Color(1f, 1f, 1f, 0.12f);
    private static readonly Color BrancoLinha = new Color(1f, 1f, 1f, 0.16f);
    private static readonly Color BrancoRodape = new Color(1f, 1f, 1f, 0.55f);
    private static readonly Color BrancoDica = new Color(1f, 1f, 1f, 0.62f);
    private static readonly Color BrancoChip = new Color(1f, 1f, 1f, 0.90f);
    private static readonly Color CorCaixaTecla = new Color(0f, 0f, 0f, 0.06f);

    private const string TextoObjetivo =
        "Elimine os criadouros do <b>Aedes aegypti</b> espalhados pela Vila Nova antes que a ocupação da UPA " +
        "chegue a <b>100%</b>. Cada foco eliminado alivia o hospital — e o bairro comenta tudo pelo celular.";

    private static readonly string[,] Controles =
    {
        { "W A S D", "Andar" }, { "Mouse", "Olhar" }, { "Shift", "Correr" }, { "Espaço", "Pular" },
        { "Clique", "Eliminar foco" }, { "T", "Abrir o celular" }, { "Tab", "Lista de focos" }, { "Esc", "Pausar" },
    };

    /* ---------- Luz de fim de tarde ---------- */
    private static readonly Vector3 RotacaoSol = new Vector3(16f, 285f, 0f);
    private static readonly Color CorSol = new Color(1f, 0.74f, 0.50f);
    private const float IntensidadeSol = 1.9f;
    private static readonly Color CorAmbiente = new Color(0.42f, 0.36f, 0.40f);
    private static readonly Color CorNeblina = new Color(0.86f, 0.66f, 0.52f);
    private const float NeblinaInicio = 40f;
    private const float NeblinaFim = 170f;

    /* ---------- Tomadas da câmera (enquadramentos do preview) ---------- */
    private static CameraMenu.Tomada[] Tomadas()
    {
        return new[]
        {
            new CameraMenu.Tomada
            {
                nome = "Rua do hospital", duracao = 20f,
                posInicio = new Vector3(-30f, 6f, -20f), alvoInicio = new Vector3(9f, 4f, -29f),
                posFim = new Vector3(-24f, 5.5f, -20f), alvoFim = new Vector3(15f, 4f, -29f),
            },
            new CameraMenu.Tomada
            {
                nome = "Praça", duracao = 20f,
                posInicio = new Vector3(30f, 9f, -30f), alvoInicio = new Vector3(-5.3f, 1f, -11.2f),
                posFim = new Vector3(30f, 9f, -22f), alvoFim = new Vector3(-5.3f, 1f, -3.2f),
            },
            new CameraMenu.Tomada
            {
                nome = "Sobrevoo", duracao = 20f,
                posInicio = new Vector3(30f, 30f, 40f), alvoInicio = new Vector3(12.1f, 0f, -6.7f),
                posFim = new Vector3(40f, 29f, 30f), alvoFim = new Vector3(12.1f, 0f, -6.7f),
            },
        };
    }

    [MenuItem("Dengue/Criar Menu Inicial")]
    public static void Criar()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogError("[Menu Inicial] Saia do Play Mode antes de criar o menu.");
            return;
        }

        PrepararRecursos();
        StringBuilder log = new StringBuilder();
        GarantirPasta();

        /* ---------- Cenas: a SampleScene aberta (só leitura) e a do menu refeita ---------- */
        bool menuEstavaAberta = SceneManager.GetSceneByPath(CaminhoCena).isLoaded;
        Scene jogo = SceneManager.GetSceneByPath(CaminhoJogo);
        bool abriuJogo = false;
        if (!jogo.isLoaded)
        {
            jogo = EditorSceneManager.OpenScene(CaminhoJogo, OpenSceneMode.Additive);
            abriuJogo = true;
        }
        Scene antiga = SceneManager.GetSceneByPath(CaminhoCena);
        if (antiga.isLoaded)
            EditorSceneManager.CloseScene(antiga, true);

        SceneManager.SetActiveScene(jogo);
        Material skybox = RenderSettings.skybox;
        ConferirTomadas(log);   // antes das cópias: só os colisores do jogo

        Scene menu = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        try
        {
            SceneManager.SetActiveScene(menu);

            MontarCenario(jogo, log);
            MontarLuz(skybox);
            CanvasGroup fadeTomada = MontarUI(log);
            MontarCamera(fadeTomada);
            SomBuilder.MontarSomDoMenu(menu);   // música + clique dos botões

            if (!EditorSceneManager.SaveScene(menu, CaminhoCena))
                Debug.LogError("[Menu Inicial] Não foi possível salvar " + CaminhoCena);
            log.Append("\n  • Cena salva em ").Append(CaminhoCena);
        }
        finally
        {
            // Deixa as cenas abertas como estavam.
            if (menuEstavaAberta)
            {
                if (abriuJogo)
                    EditorSceneManager.CloseScene(jogo, true);
                SceneManager.SetActiveScene(menu);
            }
            else
            {
                EditorSceneManager.CloseScene(menu, true);
                if (abriuJogo)
                    EditorSceneManager.CloseScene(jogo, true);
                else
                    SceneManager.SetActiveScene(jogo);
            }
        }

        AtualizarBuildSettings(log);
        Debug.Log("[Menu Inicial] Concluído." + log);
    }

    /* ================= Cenário ================= */

    private static void MontarCenario(Scene jogo, StringBuilder log)
    {
        GameObject raiz = new GameObject("--- CENARIO ---");

        List<GameObject> origens = new List<GameObject>();
        foreach (string nome in GruposCopiados)
        {
            GameObject origem = jogo.GetRootGameObjects().FirstOrDefault(g => g.name == nome);
            if (origem == null)
                log.Append("\n  ! Grupo não achado na SampleScene: ").Append(nome);
            else
                origens.Add(origem);
        }
        int malhasNulasAntes = ContarMalhasNulas(origens);

        int copiados = 0;
        foreach (GameObject origem in origens)
        {
            GameObject copia = Object.Instantiate(origem, raiz.transform, true);
            copia.name = origem.name.Trim('-', ' ');
            copiados += copia.GetComponentsInChildren<Transform>(true).Length;
        }

        // A cópia usa AS MESMAS malhas da SampleScene. O ProBuilder apaga a
        // malha do MeshFilter quando o componente é destruído — sem isto,
        // as cercas da SampleScene aberta perdiam a malha junto.
        PreservarMalhasProBuilder(raiz);

        // Combina ANTES de limpar: o ProBuilderMesh exige MeshFilter/Renderer.
        int combinadas = CombinarMalhasEmbutidas(raiz, out List<Component> substituidos, log);
        int componentes = LimparComponentes(raiz);
        foreach (Component c in substituidos)
            if (c != null)
                Object.DestroyImmediate(c);

        if (ContarMalhasNulas(origens) > malhasNulasAntes)
            Debug.LogError("[Menu Inicial] A SampleScene aberta perdeu malhas durante a cópia. NÃO salve a SampleScene: reabra-a do disco.");

        int vazios = 0;
        for (int i = raiz.transform.childCount - 1; i >= 0; i--)
            vazios += RemoverSemVisual(raiz.transform.GetChild(i));

        log.Append("\n  • Cenário: ").Append(copiados).Append(" objetos copiados, ")
           .Append(componentes).Append(" componentes não visuais removidos, ")
           .Append(combinadas).Append(" malhas embutidas combinadas, ")
           .Append(vazios).Append(" objetos vazios/inativos removidos");
    }

    private static int ContarMalhasNulas(List<GameObject> raizes)
    {
        int n = 0;
        foreach (GameObject r in raizes)
            foreach (MeshFilter mf in r.GetComponentsInChildren<MeshFilter>(true))
                if (mf.sharedMesh == null)
                    n++;
        return n;
    }

    /* ProBuilderMesh.preserveMeshAssetOnDestroy = true em toda a cópia (por
     * reflexão: o builder não depende do pacote ProBuilder). */
    private static void PreservarMalhasProBuilder(GameObject raiz)
    {
        foreach (MonoBehaviour mb in raiz.GetComponentsInChildren<MonoBehaviour>(true))
        {
            if (mb == null || mb.GetType().Name != "ProBuilderMesh")
                continue;
            PropertyInfo preservar = mb.GetType().GetProperty("preserveMeshAssetOnDestroy", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (preservar != null && preservar.CanWrite)
                preservar.SetValue(mb, true);
            else
                Debug.LogWarning("[Menu Inicial] ProBuilderMesh sem preserveMeshAssetOnDestroy — confira as malhas da SampleScene.", mb);
        }
    }

    /* Só fica o que desenha. Scripts primeiro (alguns exigem colisor). */
    private static bool Visual(Component c)
    {
        return c is Transform || c is MeshFilter || c is LODGroup || c is Terrain ||
               (c is Renderer && !(c is ParticleSystemRenderer));
    }

    private static int LimparComponentes(GameObject raiz)
    {
        int removidos = 0;
        foreach (Transform t in raiz.GetComponentsInChildren<Transform>(true))
            removidos += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);

        for (int passada = 0; passada < 4; passada++)
        {
            List<Component> sobras = raiz.GetComponentsInChildren<Component>(true)
                .Where(c => c != null && !Visual(c))
                .OrderBy(c => c is MonoBehaviour ? 0 : c is ParticleSystemRenderer ? 1 : 2)
                .ToList();
            if (sobras.Count == 0)
                break;
            foreach (Component c in sobras)
            {
                if (c == null)
                    continue;
                Object.DestroyImmediate(c);
                removidos++;
            }
        }
        return removidos;
    }

    /* Malhas que vivem DENTRO da SampleScene (não são asset — as cercas
     * de ProBuilder) seriam duplicadas no arquivo da cena do menu. Viram
     * poucas malhas grandes, uma por material, em CenarioMalhas.asset.
     * Devolve em "substituidos" os MeshRenderer/MeshFilter que saem. */
    private static int CombinarMalhasEmbutidas(GameObject raiz, out List<Component> substituidos, StringBuilder log)
    {
        Dictionary<Material, List<CombineInstance>> grupos = new Dictionary<Material, List<CombineInstance>>();
        Dictionary<Material, MeshRenderer> modelo = new Dictionary<Material, MeshRenderer>();
        List<Component> apagar = new List<Component>();
        substituidos = apagar;
        Matrix4x4 paraRaiz = raiz.transform.worldToLocalMatrix;
        int malhas = 0;

        foreach (MeshFilter mf in raiz.GetComponentsInChildren<MeshFilter>(true))
        {
            Mesh m = mf.sharedMesh;
            if (m == null || AssetDatabase.Contains(m))
                continue;
            MeshRenderer mr = mf.GetComponent<MeshRenderer>();
            if (mr == null || !mr.enabled || !mf.gameObject.activeInHierarchy || mf.GetComponentInParent<LODGroup>(true) != null)
                continue;

            Material[] mats = mr.sharedMaterials;
            for (int s = 0; s < m.subMeshCount; s++)
            {
                Material mat = mats.Length > 0 ? mats[Mathf.Min(s, mats.Length - 1)] : null;
                if (mat == null)
                    continue;
                if (!grupos.TryGetValue(mat, out List<CombineInstance> lista))
                {
                    grupos[mat] = lista = new List<CombineInstance>();
                    modelo[mat] = mr;
                }
                lista.Add(new CombineInstance { mesh = m, subMeshIndex = s, transform = paraRaiz * mf.transform.localToWorldMatrix });
            }
            apagar.Add(mr);
            apagar.Add(mf);
            malhas++;
        }

        AssetDatabase.DeleteAsset(CaminhoMalhas);
        if (grupos.Count == 0)
            return 0;

        GameObject pai = new GameObject("Malhas combinadas");
        pai.transform.SetParent(raiz.transform, false);

        bool primeira = true;
        foreach (KeyValuePair<Material, List<CombineInstance>> par in grupos)
        {
            string nomeMalha = "Cenario_" + par.Key.name;
            Mesh combinada = new Mesh { name = nomeMalha, indexFormat = IndexFormat.UInt32 };
            combinada.CombineMeshes(par.Value.ToArray(), true, true);
            Enxugar(combinada, par.Key);
            combinada.RecalculateBounds();
            if (primeira)
                AssetDatabase.CreateAsset(combinada, CaminhoMalhas);   // (o principal ganha o nome do arquivo)
            else
                AssetDatabase.AddObjectToAsset(combinada, CaminhoMalhas);
            primeira = false;

            GameObject go = new GameObject(nomeMalha);
            go.transform.SetParent(pai.transform, false);
            go.AddComponent<MeshFilter>().sharedMesh = combinada;
            MeshRenderer r = go.AddComponent<MeshRenderer>();
            MeshRenderer m0 = modelo[par.Key];
            r.sharedMaterial = par.Key;
            r.shadowCastingMode = m0.shadowCastingMode;
            r.receiveShadows = m0.receiveShadows;
            GameObjectUtility.SetStaticEditorFlags(go, GameObjectUtility.GetStaticEditorFlags(m0.gameObject));
            log.Append("\n      ").Append(nomeMalha).Append(": ").Append(par.Value.Count).Append(" partes, ")
               .Append(combinada.vertexCount).Append(" vértices");
        }
        AssetDatabase.SaveAssets();
        return malhas;   // os MeshRenderer/MeshFilter substituídos são apagados por quem chamou
    }

    /* Menos bytes por vértice (o asset é texto): sem tangente quando o
     * material não tem normal map, e normal em meia precisão. */
    private static void Enxugar(Mesh m, Material mat)
    {
        bool precisaTangente = mat.HasProperty("_BumpMap") && mat.GetTexture("_BumpMap") != null;
        List<VertexAttributeDescriptor> layout = new List<VertexAttributeDescriptor>();
        foreach (VertexAttributeDescriptor a in m.GetVertexAttributes())
        {
            if (a.attribute == VertexAttribute.Tangent && !precisaTangente)
                continue;
            if (a.attribute == VertexAttribute.Normal)
                layout.Add(new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float16, 4));
            else
                layout.Add(new VertexAttributeDescriptor(a.attribute, a.format, a.dimension));
        }
        m.SetVertexBufferParams(m.vertexCount, layout.ToArray());   // converte os dados existentes
    }

    /* Apaga (de baixo pra cima) objetos inativos e objetos sem nada visível. */
    private static int RemoverSemVisual(Transform t)
    {
        int removidos = 0;
        for (int i = t.childCount - 1; i >= 0; i--)
            removidos += RemoverSemVisual(t.GetChild(i));

        bool temVisual = t.GetComponent<Renderer>() != null || t.GetComponent<Terrain>() != null || t.GetComponent<LODGroup>() != null;
        if (!t.gameObject.activeSelf || (!temVisual && t.childCount == 0))
        {
            Object.DestroyImmediate(t.gameObject);
            removidos++;
        }
        return removidos;
    }

    /* ================= Luz ================= */

    private static void MontarLuz(Material skybox)
    {
        GameObject grupo = new GameObject("--- ILUMINACAO ---");

        GameObject solGo = new GameObject("Sol (fim de tarde)");
        solGo.transform.SetParent(grupo.transform, false);
        solGo.transform.rotation = Quaternion.Euler(RotacaoSol);
        Light sol = solGo.AddComponent<Light>();
        sol.type = LightType.Directional;
        sol.color = CorSol;
        sol.intensity = IntensidadeSol;
        sol.shadows = LightShadows.Soft;
        if (solGo.GetComponent<UniversalAdditionalLightData>() == null)
            solGo.AddComponent<UniversalAdditionalLightData>();

        RenderSettings.sun = sol;
        RenderSettings.skybox = skybox;
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = CorAmbiente;
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = CorNeblina;
        RenderSettings.fogStartDistance = NeblinaInicio;
        RenderSettings.fogEndDistance = NeblinaFim;
    }

    /* ================= Câmera ================= */

    private static void MontarCamera(CanvasGroup fadeTomada)
    {
        GameObject grupo = new GameObject("--- CAMERA ---");

        GameObject camGo = new GameObject("Main Camera");
        camGo.transform.SetParent(grupo.transform, false);
        camGo.tag = "MainCamera";
        Camera cam = camGo.AddComponent<Camera>();
        cam.fieldOfView = 55f;
        cam.nearClipPlane = 0.3f;
        cam.farClipPlane = 800f;
        cam.clearFlags = CameraClearFlags.Skybox;
        camGo.AddComponent<AudioListener>();
        UniversalAdditionalCameraData dados = camGo.GetComponent<UniversalAdditionalCameraData>();
        if (dados == null)
            dados = camGo.AddComponent<UniversalAdditionalCameraData>();
        dados.renderPostProcessing = false;   // como no preview aprovado

        CameraMenu.Tomada[] tomadas = Tomadas();
        camGo.transform.SetPositionAndRotation(tomadas[0].posInicio,
            Quaternion.LookRotation(tomadas[0].alvoInicio - tomadas[0].posInicio));

        CameraMenu cm = camGo.AddComponent<CameraMenu>();
        SerializedObject so = new SerializedObject(cm);
        SerializedProperty lista = so.FindProperty("tomadas");
        lista.arraySize = tomadas.Length;
        for (int i = 0; i < tomadas.Length; i++)
        {
            SerializedProperty e = lista.GetArrayElementAtIndex(i);
            e.FindPropertyRelative("nome").stringValue = tomadas[i].nome;
            e.FindPropertyRelative("posInicio").vector3Value = tomadas[i].posInicio;
            e.FindPropertyRelative("alvoInicio").vector3Value = tomadas[i].alvoInicio;
            e.FindPropertyRelative("posFim").vector3Value = tomadas[i].posFim;
            e.FindPropertyRelative("alvoFim").vector3Value = tomadas[i].alvoFim;
            e.FindPropertyRelative("duracao").floatValue = tomadas[i].duracao;
        }
        Ref(so, "fadeTomada", fadeTomada);
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    /* A câmera não pode atravessar prédio/poste no caminho (checado com
     * os colisores da SampleScene). */
    private static void ConferirTomadas(StringBuilder log)
    {
        Physics.SyncTransforms();
        foreach (CameraMenu.Tomada t in Tomadas())
        {
            for (int i = 0; i <= 20; i++)
            {
                Vector3 p = Vector3.Lerp(t.posInicio, t.posFim, i / 20f);
                if (Physics.CheckSphere(p, 0.8f, ~0, QueryTriggerInteraction.Ignore))
                {
                    string aviso = "Tomada \"" + t.nome + "\" encosta em algo perto de " + p.ToString("F1");
                    Debug.LogWarning("[Menu Inicial] " + aviso);
                    log.Append("\n  ! ").Append(aviso);
                    break;
                }
            }
        }
    }

    /* ================= UI ================= */

    private static CanvasGroup MontarUI(StringBuilder log)
    {
        GameObject grupo = new GameObject("--- UI ---");

        GameObject es = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        es.transform.SetParent(grupo.transform, false);
        es.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();

        GameObject root = new GameObject("MenuInicialCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        root.transform.SetParent(grupo.transform, false);
        root.layer = LayerMask.NameToLayer("UI");

        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        MenuInicial menu = root.AddComponent<MenuInicial>();
        SerializedObject so = new SerializedObject(menu);

        // Atrás de tudo: escurece só o cenário na troca de tomada.
        RectTransform fadeTomadaRt = Tela("FadeTomada", root.transform);
        Img(fadeTomadaRt, Color.black);
        CanvasGroup fadeTomada = fadeTomadaRt.gameObject.AddComponent<CanvasGroup>();
        fadeTomada.alpha = 1f;
        fadeTomada.blocksRaycasts = false;
        fadeTomada.interactable = false;

        MontarTitulo(root.transform, so);
        RectTransform cartaoComoJogar = MontarComoJogar(root.transform, so);

        RectTransform cartaoConfig = MontarConfiguracoes(root.transform, "FOCO ZERO", out RectTransform telaConfig, out PainelConfiguracoes painel);
        RectTransform veu = Tela("Veu", telaConfig);
        Img(veu, CorVeuMenu, null, 1f, true);
        veu.SetAsFirstSibling();
        Ref(so, "telaConfiguracoes", telaConfig);
        Ref(so, "configuracoes", painel);

        // Por cima de tudo: entrada do menu e saída para o jogo.
        RectTransform fadeRt = Tela("Fade", root.transform);
        Img(fadeRt, Color.black, null, 1f, true);
        CanvasGroup fade = fadeRt.gameObject.AddComponent<CanvasGroup>();
        fade.alpha = 1f;
        fade.blocksRaycasts = false;
        fade.interactable = false;
        TextMeshProUGUI carregando = Txt(Pos(Novo("Carregando", fadeRt), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-Margem, 44f), new Vector2(400f, 30f)),
            "Carregando…", 18, BrancoDica, TextAlignmentOptions.MidlineRight);
        carregando.gameObject.SetActive(false);
        Ref(so, "fade", fade);
        Ref(so, "textoCarregando", carregando);

        SerializedProperty cartoes = so.FindProperty("cartoes");
        cartoes.arraySize = 2;
        cartoes.GetArrayElementAtIndex(0).objectReferenceValue = cartaoComoJogar;
        cartoes.GetArrayElementAtIndex(1).objectReferenceValue = cartaoConfig;
        so.ApplyModifiedPropertiesWithoutUndo();

        log.Append("\n  • MenuInicialCanvas (título, como jogar, configurações) + EventSystem");
        return fadeTomada;
    }

    private static void MontarTitulo(Transform pai, SerializedObject so)
    {
        RectTransform tela = Tela("TelaTitulo", pai);
        Ref(so, "telaTitulo", tela);

        CriarDegrades(out Sprite degrade, out Sprite degradeBaixo);

        RectTransform lado = Novo("Degrade", tela);
        lado.anchorMin = Vector2.zero;
        lado.anchorMax = new Vector2(0f, 1f);
        lado.pivot = new Vector2(0f, 0.5f);
        lado.sizeDelta = new Vector2(1350f, 0f);
        lado.anchoredPosition = Vector2.zero;
        Img(lado, Color.white, degrade);

        RectTransform baixo = Novo("DegradeBaixo", tela);
        baixo.anchorMin = Vector2.zero;
        baixo.anchorMax = new Vector2(1f, 0f);
        baixo.pivot = new Vector2(0.5f, 0f);
        baixo.sizeDelta = new Vector2(0f, 220f);
        baixo.anchoredPosition = Vector2.zero;
        Img(baixo, Color.white, degradeBaixo);

        Vector2 esq = new Vector2(0f, 0.5f);

        // Cruz vermelha + rótulo
        RectTransform icone = Pos(Novo("Icone", tela), esq, esq, new Vector2(Margem, 250f), new Vector2(34f, 34f));
        Img(icone, Perigo, knobSprite);
        Img(Pos(Novo("H", icone), Centro, Centro, Vector2.zero, new Vector2(19f, 6f)), TextoClaro);
        Img(Pos(Novo("V", icone), Centro, Centro, Vector2.zero, new Vector2(6f, 19f)), TextoClaro);
        Txt(Pos(Novo("Rotulo", tela), esq, esq, new Vector2(Margem + 50f, 250f), new Vector2(600f, 30f)),
            "VILA NOVA  ·  FEVEREIRO", 17, BrancoRotulo, TextAlignmentOptions.Left, true, 4f);

        // Título + subtítulo
        TextMeshProUGUI titulo = Txt(Pos(Novo("Titulo", tela), esq, esq, new Vector2(Margem - 6f, 158f), new Vector2(1000f, 150f)),
            Titulo, 136, Color.white, TextAlignmentOptions.Left, true, -1f);
        Shadow sombra = titulo.gameObject.AddComponent<Shadow>();
        sombra.effectColor = new Color(0f, 0f, 0f, 0.35f);
        sombra.effectDistance = new Vector2(0f, -4f);
        Txt(Pos(Novo("Subtitulo", tela), esq, esq, new Vector2(Margem, 72f), new Vector2(900f, 40f)),
            "Um agente de saúde contra a dengue.", 28, BrancoSubtitulo, TextAlignmentOptions.Left);

        // Botões
        Button jogar = BotaoTitulo(tela, -30f, "Jogar", true);
        Button comoJogar = BotaoTitulo(tela, -96f, "Como jogar", false);
        Button configuracoes = BotaoTitulo(tela, -162f, "Configurações", false);
        Img(Pos(Novo("Divisoria", tela), esq, esq, new Vector2(Margem, -206f), new Vector2(380f, 1f)), BrancoLinha);
        Button sair = BotaoTitulo(tela, -246f, "Sair", false);
        Vertical(jogar, comoJogar, configuracoes, sair);
        Ref(so, "botaoJogar", jogar);
        Ref(so, "botaoComoJogar", comoJogar);
        Ref(so, "botaoConfiguracoes", configuracoes);
        Ref(so, "botaoSair", sair);

        // Rodapé
        Vector2 baseEsq = Vector2.zero;
        Txt(Pos(Novo("Assinatura", tela), baseEsq, baseEsq, new Vector2(Margem, 46f), new Vector2(700f, 26f)),
            "v0.1  ·  Eduardo Augusto Richter", 16, BrancoRodape, TextAlignmentOptions.Left, false, 0.5f);

        // [ENTER] Selecionar   [ESC] Voltar — montado da direita pra esquerda.
        Vector2 baseDir = new Vector2(1f, 0f);
        Txt(Pos(Novo("Dica Voltar", tela), baseDir, baseDir, new Vector2(-Margem, 44f), new Vector2(60f, 28f)),
            "Voltar", 16, BrancoDica, TextAlignmentOptions.MidlineLeft);
        Tecla(tela, baseDir, new Vector2(-Margem - 70f, 44f), "ESC", BrancoChip, BrancoLinha);
        Txt(Pos(Novo("Dica Selecionar", tela), baseDir, baseDir, new Vector2(-Margem - 151f, 44f), new Vector2(95f, 28f)),
            "Selecionar", 16, BrancoDica, TextAlignmentOptions.MidlineLeft);
        Tecla(tela, baseDir, new Vector2(-Margem - 256f, 44f), "ENTER", BrancoChip, BrancoLinha);
    }

    /* Botão só de texto: destaque = fundo branco suave + barrinha + seta,
     * e o texto acende (ItemDeMenu). */
    private static Button BotaoTitulo(RectTransform pai, float y, string texto, bool principal)
    {
        Vector2 esq = new Vector2(0f, 0.5f);
        RectTransform rt = Pos(Novo("Botao " + texto, pai), esq, esq, new Vector2(Margem - 24f, y), new Vector2(430f, 60f));
        Image img = Img(rt, Transparente, uiSprite, 0.5f, true);

        RectTransform t = Tela("Texto", rt);
        t.offsetMin = new Vector2(24f, 0f);
        TextMeshProUGUI rotulo = Txt(t, texto, principal ? 32 : 29, BrancoBotao, TextAlignmentOptions.MidlineLeft, principal, 0.5f);

        RectTransform foco = Tela("Foco", rt);
        Img(Pos(Novo("Barra", foco), esq, esq, Vector2.zero, new Vector2(4f, 30f)), Color.white, uiSprite, 2f);
        Txt(Pos(Novo("Seta", foco), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-20f, 0f), new Vector2(40f, 40f)),
            "›", 36, Color.white, TextAlignmentOptions.Center, true);
        foco.gameObject.SetActive(false);

        Button b = rt.gameObject.AddComponent<Button>();
        b.targetGraphic = img;
        b.transition = Selectable.Transition.None;   // o destaque é do ItemDeMenu

        ItemDeMenu item = Item(rt.gameObject, b, img, Transparente, BrancoDestaque, foco.gameObject);
        SerializedObject so = new SerializedObject(item);
        Ref(so, "texto", rotulo);
        so.FindProperty("corTextoNormal").colorValue = BrancoBotao;
        so.FindProperty("corTextoDestaque").colorValue = Color.white;
        so.ApplyModifiedPropertiesWithoutUndo();
        return b;
    }

    /* Cartão branco no tema do pause: objetivo + controles. */
    private static RectTransform MontarComoJogar(Transform pai, SerializedObject so)
    {
        RectTransform tela = Tela("TelaComoJogar", pai);
        tela.gameObject.SetActive(false);
        Img(Tela("Veu", tela), CorVeuMenu, null, 1f, true);
        Ref(so, "telaComoJogar", tela);

        const float largura = 820f;
        RectTransform cartao = Cartao("Cartao", tela, new Vector2(largura, 700f));

        Cruz(cartao, new Vector2(PaddingConteudo, -40f), 30f);
        Txt(Pos(Novo("Rotulo", cartao), TopoEsq, TopoEsq, new Vector2(PaddingConteudo + 44f, -40f), new Vector2(500f, 30f)),
            "FOCO ZERO", 14, CorRotulo, TextAlignmentOptions.Left, true, 3f);
        Txt(Pos(Novo("Titulo", cartao), TopoEsq, TopoEsq, new Vector2(PaddingConteudo - 2f, -80f), new Vector2(500f, 52f)),
            "Como jogar", 40, CorTexto, TextAlignmentOptions.Left, true);
        Divisoria(cartao, -146f, largura - PaddingBotoes * 2f);

        float y = -168f;
        Secao(cartao, ref y, "OBJETIVO");
        TextMeshProUGUI objetivo = Txt(Pos(Novo("Objetivo", cartao), TopoEsq, TopoEsq, new Vector2(PaddingConteudo, y), new Vector2(largura - PaddingConteudo * 2f, 90f)),
            TextoObjetivo, 20, CorTextoFraco, TextAlignmentOptions.TopLeft);
        objetivo.textWrappingMode = TextWrappingModes.Normal;
        objetivo.lineSpacing = 8f;
        y -= 104f;

        Secao(cartao, ref y, "CONTROLES");
        y -= 8f;
        float coluna = (largura - PaddingConteudo * 2f) / 2f;
        for (int i = 0; i < Controles.GetLength(0); i++)
        {
            float x = PaddingConteudo + (i / 4) * coluna;
            float yy = y - (i % 4) * 50f;
            float w = ChipGrande(cartao, new Vector2(x, yy), Controles[i, 0]);
            Txt(Pos(Novo("Acao " + Controles[i, 0], cartao), TopoEsq, TopoEsq, new Vector2(x + w + 16f, yy), new Vector2(260f, 36f)),
                Controles[i, 1], 20, CorTexto, TextAlignmentOptions.MidlineLeft);
        }
        y -= 4 * 50f;

        Txt(Pos(Novo("Dica controle", cartao), TopoEsq, TopoEsq, new Vector2(PaddingConteudo, y - 4f), new Vector2(largura - PaddingConteudo * 2f, 28f)),
            "Com controle: analógicos para andar e olhar, X para eliminar focos, Start para pausar.", 16, CorTextoDica, TextAlignmentOptions.Left);
        y -= 40f;

        // Rodapé: cresce junto com o conteúdo.
        cartao.sizeDelta = new Vector2(largura, -y + 28f + 56f + 28f);
        Img(Pos(Novo("Divisoria rodape", cartao), BaseCentro, BaseCentro, new Vector2(0f, 28f + 56f + 24f), new Vector2(largura - PaddingBotoes * 2f, 1f)), CorLinha);

        Vector2 baseDir = new Vector2(1f, 0f);
        Button voltar = BotaoCentral(cartao, baseDir, new Vector2(-PaddingBotoes, PaddingBotoes), new Vector2(220f, 56f),
            "Voltar", 21, Primaria, PrimariaHover, TextoClaro, null);
        Navegar(voltar, null, null, null, null);
        Tecla(cartao, baseDir, new Vector2(-PaddingBotoes - 240f, PaddingBotoes + 14f), "ESC", CorTextoFraco, CorCaixa);
        Txt(Pos(Novo("Dica", cartao), baseDir, baseDir, new Vector2(-PaddingBotoes - 301f, PaddingBotoes + 14f), new Vector2(200f, 28f)),
            "volta ao menu", 15, CorTextoDica, TextAlignmentOptions.MidlineRight);

        Ref(so, "botaoVoltarComoJogar", voltar);
        return cartao;
    }

    /* Chip de tecla maior (cartão "Como jogar"); devolve a largura. */
    private static float ChipGrande(RectTransform pai, Vector2 pos, string texto)
    {
        float w = 26f + texto.Length * 11.5f;
        RectTransform k = Pos(Novo("Tecla " + texto, pai), TopoEsq, TopoEsq, pos, new Vector2(w, 36f));
        Contorno(Img(k, CorCaixaTecla, uiSprite, 0.8f), CorLinha);
        Txt(Tela("Texto", k), texto, 15, CorTexto, TextAlignmentOptions.Center, true, 1f);
        return w;
    }

    /* ================= Assets ================= */

    private static void GarantirPasta()
    {
        if (!AssetDatabase.IsValidFolder(Pasta))
            AssetDatabase.CreateFolder(PastaPai, "Menu Inicial");
    }

    /* Degradês do título: lateral (escuro até ~22% da largura e some
     * suave) e de rodapé. */
    private static void CriarDegrades(out Sprite lateral, out Sprite rodape)
    {
        Texture2D h = new Texture2D(512, 4, TextureFormat.RGBA32, false);
        for (int x = 0; x < h.width; x++)
        {
            float t = x / (h.width - 1f);
            float a = 0.94f * (1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.22f, 1f, t)));
            for (int y = 0; y < h.height; y++)
                h.SetPixel(x, y, new Color(Escuro.r, Escuro.g, Escuro.b, a));
        }
        lateral = SalvarSprite(h, CaminhoDegrade);

        Texture2D v = new Texture2D(4, 128, TextureFormat.RGBA32, false);
        for (int y = 0; y < v.height; y++)
        {
            float t = y / (v.height - 1f);
            float a = 0.55f * Mathf.Pow(1f - t, 2f);
            for (int x = 0; x < v.width; x++)
                v.SetPixel(x, y, new Color(Escuro.r, Escuro.g, Escuro.b, a));
        }
        rodape = SalvarSprite(v, CaminhoDegradeBaixo);
    }

    private static Sprite SalvarSprite(Texture2D tex, string caminho)
    {
        File.WriteAllBytes(caminho, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(caminho, ImportAssetOptions.ForceUpdate);

        TextureImporter imp = (TextureImporter)AssetImporter.GetAtPath(caminho);
        imp.textureType = TextureImporterType.Sprite;
        imp.spriteImportMode = SpriteImportMode.Single;
        imp.alphaIsTransparency = true;
        imp.mipmapEnabled = false;
        imp.wrapMode = TextureWrapMode.Clamp;
        imp.filterMode = FilterMode.Bilinear;
        imp.textureCompression = TextureImporterCompression.Uncompressed;
        imp.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(caminho);
    }

    /* ================= Build ================= */

    private static void AtualizarBuildSettings(StringBuilder log)
    {
        List<EditorBuildSettingsScene> cenas = new List<EditorBuildSettingsScene>
        {
            new EditorBuildSettingsScene(CaminhoCena, true),
            new EditorBuildSettingsScene(CaminhoJogo, true),
        };
        foreach (EditorBuildSettingsScene c in EditorBuildSettings.scenes)
            if (c.path != CaminhoCena && c.path != CaminhoJogo)
                cenas.Add(c);
        EditorBuildSettings.scenes = cenas.ToArray();
        log.Append("\n  • Build Settings: 0 = MenuInicial, 1 = SampleScene");
    }
}
