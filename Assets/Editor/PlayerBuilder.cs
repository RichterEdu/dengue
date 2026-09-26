using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

/* ============================================================
 *  PlayerBuilder.cs  (Editor)
 *  ---------------------------------------------------------------------
 *  Monta o jogador como AGENTE DE SAÚDE, com a movimentação nova.
 *
 *  Menu:  Dengue > Configurar Player (Agente de Saúde)
 *
 *  O modelo (Assets/Meus/Assets Gerados/Agente de Saude/AgenteSaude.fbx)
 *  foi modelado, rigado e animado do zero no Blender por
 *  Ferramentas/Blender/gerar_agente.py (low poly, esqueleto Generic,
 *  animações in-place Idle/Walk/Run/Air). Pra mudar o visual, edite o
 *  script do Blender, gere o FBX de novo e rode este menu.
 *
 *  Idempotente — pode rodar quantas vezes quiser:
 *    1. Materiais URP/Lit (paleta do uniforme) em .../Agente de Saude/Materiais,
 *       remapeados no import do FBX pelo nome do material.
 *    2. Import do FBX: eixo corrigido (bakeAxisConversion), Generic, sem
 *       root motion, clipes renomeados (Idle/Walk/Run/Air) e em loop.
 *    3. Agente_AnimController.controller (recriado): Blend Tree "Speed"
 *       Idle 0 / Walk 0.5 / Run 1 + estado "NoAr" pelo parâmetro "NoChao".
 *    4. Prefab "Agente de Saude.prefab" (variante do FBX com o controller).
 *    5. No player da cena (achado pelo CharacterController, não pelo nome):
 *       - troca Mover/Pular/MouseLook por EntradaJogador + ControladorJogador
 *         (mantendo a sensibilidade de mouse que já estava ajustada);
 *       - CharacterController com medidas de gente (1,75 m) e skin fino;
 *       - troca o corpo antigo (filho "Modelo" ou o "Cube" provisório) pelo
 *         agente, com a malha da cabeça em "Shadows Only" (some da câmera
 *         em primeira pessoa mas continua fazendo sombra);
 *       - layer "CorpoJogador" no corpo (o Interactor ignora essa layer);
 *       - CameraJogador na Main Camera, com o pivô do pescoço medido no
 *         osso "Head" do modelo.
 * ============================================================ */

public static class PlayerBuilder
{
    private const string Pasta = "Assets/Meus/Assets Gerados/Agente de Saude";
    private const string CaminhoFbx = Pasta + "/AgenteSaude.fbx";
    private const string PastaMateriais = Pasta + "/Materiais";
    private const string CaminhoController = Pasta + "/Agente_AnimController.controller";
    private const string CaminhoPrefab = Pasta + "/Agente de Saude.prefab";
    private const string CaminhoAcoes = "Assets/InputSystem_Actions.inputactions";

    private const string NomeModelo = "Modelo";
    private const string NomeLayerCorpo = "CorpoJogador";
    private const string NomeMalhaCabeca = "AgenteSaude_Cabeca";

    private const float AlturaJogador = 1.75f;
    private const float RaioJogador = 0.3f;
    private const float SkinWidth = 0.03f;

    // Mesmas cores do gerador do Blender (sRGB), com a suavidade de cada tecido.
    private static readonly (string nome, string hex, float suavidade, float metalico)[] Paleta =
    {
        ("Pele", "#B07A55", 0.30f, 0f), ("Boca", "#7A4434", 0.30f, 0f), ("Olhos", "#1A1A1A", 0.65f, 0f),
        ("Cabelo", "#2B1D14", 0.20f, 0f), ("Camisa", "#F2F2EE", 0.15f, 0f), ("Colete", "#1F5FBF", 0.18f, 0f),
        ("ColeteEscuro", "#174A96", 0.18f, 0f), ("Refletivo", "#D8E04A", 0.55f, 0f), ("Calca", "#2E3F5C", 0.12f, 0f),
        ("Tenis", "#34343A", 0.25f, 0f), ("Sola", "#EDEDED", 0.20f, 0f), ("Bone", "#1F5FBF", 0.18f, 0f),
        ("Logo", "#FFFFFF", 0.20f, 0f), ("Bolsa", "#9C7B55", 0.25f, 0f), ("BolsaEscuro", "#6E5238", 0.25f, 0f),
        ("Alca", "#35353A", 0.20f, 0f), ("Metal", "#B8BCC4", 0.70f, 0.6f), ("Cracha", "#FFFFFF", 0.35f, 0f),
        ("Texto", "#FFFFFF", 0.20f, 0f),
    };

    private static readonly string[] ScriptsAntigos = { "Pular", "Mover", "MouseLook" }; // Pular antes: depende do Mover

    [MenuItem("Dengue/Configurar Player (Agente de Saúde)")]
    public static void Configurar()
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(CaminhoFbx) == null)
        {
            EditorUtility.DisplayDialog("Configurar Player", "Não encontrei " + CaminhoFbx + ".", "OK");
            return;
        }

        Dictionary<string, Material> materiais = CriarMateriais();
        ConfigurarImportacao(materiais);
        AnimatorController controller = CriarAnimatorController();
        if (controller == null)
            return;
        GameObject prefab = CriarPrefab(controller, ClipeDoFbx("Idle"));
        ConfigurarPlayer(prefab);
    }

    private static AnimationClip ClipeDoFbx(string nome) =>
        AssetDatabase.LoadAllAssetsAtPath(CaminhoFbx).OfType<AnimationClip>().FirstOrDefault(c => c.name == nome);

    /* ---------------- 1. Materiais ---------------- */

    private static Dictionary<string, Material> CriarMateriais()
    {
        if (!AssetDatabase.IsValidFolder(PastaMateriais))
            AssetDatabase.CreateFolder(Pasta, "Materiais");

        Shader lit = Shader.Find("Universal Render Pipeline/Lit");
        var materiais = new Dictionary<string, Material>();
        foreach (var (nome, hex, suavidade, metalico) in Paleta)
        {
            string caminho = PastaMateriais + "/Agente_" + nome + ".mat";
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
            mat.DisableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", Color.black);
            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
            mat.enableInstancing = true;
            EditorUtility.SetDirty(mat);
            materiais[nome] = mat;
        }
        AssetDatabase.SaveAssets();
        return materiais;
    }

    /* ---------------- 2. Import do FBX ---------------- */

    private static void ConfigurarImportacao(Dictionary<string, Material> materiais)
    {
        var imp = (ModelImporter)AssetImporter.GetAtPath(CaminhoFbx);

        imp.globalScale = 1f;
        imp.useFileScale = true;
        imp.bakeAxisConversion = true; // sem isso o Blender entra com -90° em X
        imp.importCameras = false;
        imp.importLights = false;
        imp.importBlendShapes = false;
        imp.isReadable = false;
        imp.animationType = ModelImporterAnimationType.Generic;
        imp.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
        imp.motionNodeName = ""; // sem root motion: quem move é o CharacterController
        imp.importAnimation = true;
        imp.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;

        foreach (var par in materiais)
            imp.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), par.Key), par.Value);

        ModelImporterClipAnimation[] clipes = imp.defaultClipAnimations;
        foreach (ModelImporterClipAnimation c in clipes)
        {
            int barra = c.takeName.LastIndexOf('|');
            c.name = barra >= 0 ? c.takeName.Substring(barra + 1) : c.takeName; // "AgenteSaude|Walk" -> "Walk"
            c.loopTime = true;
            c.loopPose = false; // primeiro e último frame já são iguais
            c.lockRootRotation = true;
            c.lockRootHeightY = true;
            c.lockRootPositionXZ = true;
            c.keepOriginalOrientation = true;
            c.keepOriginalPositionY = true;
            c.keepOriginalPositionXZ = true;
        }
        imp.clipAnimations = clipes;

        imp.SaveAndReimport();
    }

    /* ---------------- 3. Animator Controller ---------------- */

    private static AnimatorController CriarAnimatorController()
    {
        Dictionary<string, AnimationClip> clipes = AssetDatabase.LoadAllAssetsAtPath(CaminhoFbx)
            .OfType<AnimationClip>()
            .Where(c => !c.name.StartsWith("__preview__"))
            .ToDictionary(c => c.name);

        foreach (string nome in new[] { "Idle", "Walk", "Run", "Air" })
        {
            if (!clipes.ContainsKey(nome))
            {
                EditorUtility.DisplayDialog("Configurar Player", "O FBX não tem o clipe \"" + nome + "\".", "OK");
                return null;
            }
        }

        if (AssetDatabase.LoadAssetAtPath<AnimatorController>(CaminhoController) != null)
            AssetDatabase.DeleteAsset(CaminhoController);

        AnimatorController ctrl = AnimatorController.CreateAnimatorControllerAtPath(CaminhoController);
        ctrl.AddParameter("Speed", AnimatorControllerParameterType.Float);
        ctrl.AddParameter(new AnimatorControllerParameter
        {
            name = "NoChao",
            type = AnimatorControllerParameterType.Bool,
            defaultBool = true
        });

        AnimatorStateMachine sm = ctrl.layers[0].stateMachine;
        AnimatorState locomocao = ctrl.CreateBlendTreeInController("Locomocao", out BlendTree arvore, 0);
        arvore.blendType = BlendTreeType.Simple1D;
        arvore.blendParameter = "Speed";
        arvore.useAutomaticThresholds = false;
        arvore.AddChild(clipes["Idle"], 0f);
        arvore.AddChild(clipes["Walk"], 0.5f);
        arvore.AddChild(clipes["Run"], 1f);

        AnimatorState noAr = sm.AddState("NoAr", new Vector3(300f, 120f, 0f));
        noAr.motion = clipes["Air"];
        sm.defaultState = locomocao;

        AnimatorStateTransition sair = locomocao.AddTransition(noAr);
        sair.hasExitTime = false;
        sair.duration = 0.12f;
        sair.AddCondition(AnimatorConditionMode.IfNot, 0f, "NoChao");

        AnimatorStateTransition voltar = noAr.AddTransition(locomocao);
        voltar.hasExitTime = false;
        voltar.duration = 0.1f;
        voltar.AddCondition(AnimatorConditionMode.If, 0f, "NoChao");

        EditorUtility.SetDirty(ctrl);
        AssetDatabase.SaveAssets();
        return ctrl;
    }

    /* ---------------- 4. Prefab ---------------- */

    private static GameObject CriarPrefab(AnimatorController controller, AnimationClip idle)
    {
        GameObject modelo = AssetDatabase.LoadAssetAtPath<GameObject>(CaminhoFbx);
        GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(modelo);
        inst.name = "Agente de Saude";

        // O FBX sai do Blender numa pose qualquer; no Editor (fora do Play) o
        // agente fica em pé na pose Idle, e é nela que o pivô da câmera é medido.
        if (idle != null)
            idle.SampleAnimation(inst, 0f);

        Animator anim = inst.GetComponent<Animator>();
        if (anim == null)
            anim = inst.AddComponent<Animator>();
        anim.runtimeAnimatorController = controller;
        anim.applyRootMotion = false;
        anim.cullingMode = AnimatorCullingMode.AlwaysAnimate;

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(inst, CaminhoPrefab);
        Object.DestroyImmediate(inst);
        return prefab;
    }

    /* ---------------- 5. Player da cena ---------------- */

    private static void ConfigurarPlayer(GameObject prefab)
    {
        CharacterController cc = Object.FindFirstObjectByType<CharacterController>();
        if (cc == null)
        {
            EditorUtility.DisplayDialog("Configurar Player",
                "Não encontrei nenhum CharacterController (o player) na cena. " +
                "Materiais, controller e prefab foram criados mesmo assim.", "OK");
            return;
        }
        GameObject player = cc.gameObject;
        var relatorio = new List<string>();

        // Sensibilidade que o usuário já tinha ajustado no MouseLook antigo.
        float? sensibilidadeAntiga = null;
        foreach (MonoBehaviour mb in player.GetComponents<MonoBehaviour>())
        {
            if (mb != null && mb.GetType().Name == "MouseLook")
            {
                SerializedProperty p = new SerializedObject(mb).FindProperty("mouseSensitivity");
                if (p != null)
                    sensibilidadeAntiga = p.floatValue;
            }
        }

        // Componentes antigos (por nome: os scripts podem já ter sido apagados).
        foreach (string nome in ScriptsAntigos)
        {
            foreach (MonoBehaviour mb in player.GetComponents<MonoBehaviour>())
            {
                if (mb != null && mb.GetType().Name == nome)
                {
                    Undo.DestroyObjectImmediate(mb);
                    relatorio.Add("removido " + nome);
                }
            }
        }
        int faltando = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(player);
        if (faltando > 0)
            relatorio.Add(faltando + " script(s) ausente(s) removido(s)");

        // Entrada + controlador + animador.
        EntradaJogador entrada = player.GetComponent<EntradaJogador>();
        if (entrada == null)
            entrada = Undo.AddComponent<EntradaJogador>(player);
        var soEntrada = new SerializedObject(entrada);
        soEntrada.FindProperty("acoes").objectReferenceValue = AssetDatabase.LoadAssetAtPath<InputActionAsset>(CaminhoAcoes);
        if (sensibilidadeAntiga.HasValue)
            soEntrada.FindProperty("sensibilidadeMouse").floatValue = sensibilidadeAntiga.Value;
        soEntrada.ApplyModifiedProperties();

        ControladorJogador controlador = player.GetComponent<ControladorJogador>();
        if (controlador == null)
            controlador = Undo.AddComponent<ControladorJogador>(player);
        if (player.GetComponent<AnimadorJogador>() == null)
            Undo.AddComponent<AnimadorJogador>(player);

        // CharacterController com medidas de gente; skin fino (o antigo, 0,08,
        // deixava o corpo flutuando 8 cm) e o modelo desce o mesmo tanto.
        Undo.RecordObject(cc, "Configurar Player");
        cc.height = AlturaJogador;
        cc.radius = RaioJogador;
        cc.center = new Vector3(0f, AlturaJogador * 0.5f, 0f);
        cc.slopeLimit = 45f;
        cc.stepOffset = 0.35f;
        cc.skinWidth = SkinWidth;
        cc.minMoveDistance = 0f;

        // Corpo antigo: "Modelo" (médico) ou o "Cube" provisório.
        foreach (Transform filho in player.transform.Cast<Transform>().ToList())
        {
            bool corpoAntigo = filho.name == NomeModelo ||
                               (filho.name == "Cube" && filho.GetComponent<MeshFilter>() != null);
            if (corpoAntigo)
            {
                relatorio.Add("corpo antigo \"" + filho.name + "\" removido");
                Undo.DestroyObjectImmediate(filho.gameObject);
            }
        }

        GameObject corpo = (GameObject)PrefabUtility.InstantiatePrefab(prefab, player.transform);
        Undo.RegisterCreatedObjectUndo(corpo, "Configurar Player");
        corpo.name = NomeModelo;
        corpo.transform.localPosition = new Vector3(0f, -SkinWidth, 0f);
        corpo.transform.localRotation = Quaternion.identity;
        corpo.transform.localScale = Vector3.one;

        foreach (SkinnedMeshRenderer smr in corpo.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            smr.shadowCastingMode = smr.name == NomeMalhaCabeca ? ShadowCastingMode.ShadowsOnly : ShadowCastingMode.On;
            smr.updateWhenOffscreen = true; // câmera fica colada no corpo: sem sumir por culling
        }

        int layer = GarantirLayer(NomeLayerCorpo);
        if (layer >= 0)
            AplicarLayerRecursivo(corpo, layer);

        // Olhando pra +Z, a mão esquerda fica em -X local. Se não, o FBX foi
        // exportado com o eixo errado (ver axis_forward no gerar_agente.py).
        Transform maoEsquerda = AcharOsso(corpo.transform, "LeftHand");
        if (maoEsquerda != null && corpo.transform.InverseTransformPoint(maoEsquerda.position).x > 0f)
            relatorio.Add("AVISO: o modelo está olhando pra trás (-Z)");

        // Câmera: pivô do pescoço medido no osso Head (em pose Idle).
        Camera cam = Camera.main;
        if (cam != null)
        {
            CameraJogador camJogador = cam.GetComponent<CameraJogador>();
            if (camJogador == null)
                camJogador = Undo.AddComponent<CameraJogador>(cam.gameObject);

            var soCam = new SerializedObject(camJogador);
            soCam.FindProperty("jogador").objectReferenceValue = controlador;
            Transform cabeca = AcharOsso(corpo.transform, "Head");
            if (cabeca != null)
                soCam.FindProperty("alturaPivo").floatValue = cabeca.position.y - player.transform.position.y;
            soCam.ApplyModifiedProperties();

            if (cam.transform.parent != null)
                relatorio.Add("aviso: a Main Camera é filha de \"" + cam.transform.parent.name + "\"");
        }
        else
        {
            relatorio.Add("aviso: sem Main Camera na cena");
        }

        EditorSceneManager.MarkSceneDirty(player.scene);
        Selection.activeGameObject = player;

        Debug.Log("[Player] Agente de saúde configurado em \"" + player.name + "\" (" +
                  string.Join("; ", relatorio.DefaultIfEmpty("nada antigo pra remover")) + ").", player);
    }

    private static Transform AcharOsso(Transform raiz, string nome)
    {
        if (raiz.name == nome)
            return raiz;
        foreach (Transform filho in raiz)
        {
            Transform achado = AcharOsso(filho, nome);
            if (achado != null)
                return achado;
        }
        return null;
    }

    /* Cria a User Layer se ainda não existir (primeiro slot vazio do
     * TagManager) e devolve o índice dela. -1 se não sobrar nenhum slot. */
    private static int GarantirLayer(string nome)
    {
        int existente = LayerMask.NameToLayer(nome);
        if (existente >= 0)
            return existente;

        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layers = tagManager.FindProperty("layers");

        for (int i = 8; i < layers.arraySize; i++)
        {
            SerializedProperty slot = layers.GetArrayElementAtIndex(i);
            if (string.IsNullOrEmpty(slot.stringValue))
            {
                slot.stringValue = nome;
                tagManager.ApplyModifiedProperties();
                return i;
            }
        }

        Debug.LogWarning("[Player] Não sobrou nenhuma User Layer livre pra criar \"" + nome + "\".");
        return -1;
    }

    private static void AplicarLayerRecursivo(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform filho in go.transform)
            AplicarLayerRecursivo(filho.gameObject, layer);
    }
}
