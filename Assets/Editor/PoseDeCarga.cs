using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

/* ============================================================
 *  PoseDeCarga.cs  (Editor)
 *  ---------------------------------------------------------------------
 *  O corpo do agente carregando (Parte B): onde o pneu fica nos braços,
 *  onde o saco de lixo fica na mão e a camada do Animator que põe os
 *  braços em volta do pneu e a do braço direito erguido com o saco. Não
 *  tem menu: o builder da cena (Task 13) chama o Aplicar, e os assets
 *  saem do GarantirAssets.
 *
 *  Aplicar(player) — no Player da cena (idempotente):
 *    - acha os ossos "Chest" e "RightHand" no corpo (filho "Modelo");
 *    - "PontoDeCarga" filho do Chest, na PosicaoDaCarga/GiroDaCarga
 *      (o pneu de ~0,7 m nas mãos da pose "Carregando", de pé, com o topo
 *      um pouco para a frente). O PontoDeCarga antigo (filho da câmera,
 *      Parte A), vazio, é apagado;
 *    - "SacoNaMao" (instância do Saco.fbx, inativo) filho da RightHand,
 *      na PosicaoDoSaco/GiroDoSaco: pende da mão, com a boca amarrada na
 *      palma, na pose "CarregandoSaco" (braço erguido à frente);
 *    - liga o KitDoAgente.pontoDeCarga e o FeedbackDeCarga.sacoNaMao
 *      (o FeedbackDeCarga é criado se faltar), com a EscalaDoSaco.
 *  Não salva a cena: quem chama decide.
 *
 *  GarantirAssets() — idempotente, rodar depois de reexportar os FBX:
 *    - clipes "Carregando" e "CarregandoSaco" no import do AgenteSaude.fbx
 *      (mesmos ajustes dos clipes do PlayerBuilder: sem root motion);
 *    - import do Saco.fbx (eixo corrigido, sem animação) com o material
 *      URP Saco_Plastico.mat;
 *    - Agente_Carregando.mask (Spine, Chest, braços e mãos) e, no
 *      Agente_AnimController, o bool "Carregando" e a camada "Carregando"
 *      (a máscara, estados Vazio <-> SegurandoPneu, 0,15 s);
 *    - Agente_CarregandoSaco.mask (só o braço direito) e a camada
 *      "CarregandoSaco" (a máscara, um estado só, SegurandoSaco).
 *      As duas camadas são recriadas a cada vez (nada duplica) e nascem
 *      com peso 0, para uma cena sem o FeedbackDeCarga não congelar os
 *      braços: é ele quem leva o peso de cada uma a 1 (e de volta a 0)
 *      em 0,15 s. Os estados ficam sem Write Defaults. O Vazio não tem
 *      clipe e, com peso acima de 0, guardaria a última pose dos braços;
 *      por isso, sem pneu, a camada precisa ficar em peso 0.
 *
 *  Atenção: o PlayerBuilder (Dengue > Configurar Player) recria o
 *  controller do zero e destrói o "Modelo" (com o PontoDeCarga e o
 *  SacoNaMao, que são filhos dos ossos). Depois dele, rodar o
 *  GarantirAssets E o Aplicar.
 *
 *  Poses e saco: gerados por Ferramentas/Blender/gerar_agente.py (ações
 *  Carregando e CarregandoSaco) e gerar_saco.py.
 * ============================================================ */

public static class PoseDeCarga
{
    private const string PastaAgente = "Assets/Meus/Assets Gerados/Agente de Saude";
    public const string CaminhoFbxAgente = PastaAgente + "/AgenteSaude.fbx";
    public const string CaminhoController = PastaAgente + "/Agente_AnimController.controller";
    public const string CaminhoMascara = PastaAgente + "/Agente_Carregando.mask";
    public const string CaminhoMascaraSaco = PastaAgente + "/Agente_CarregandoSaco.mask";

    private const string PastaSaco = "Assets/Meus/Assets Gerados/Saco";
    public const string CaminhoFbxSaco = PastaSaco + "/Saco.fbx";
    public const string CaminhoMaterialSaco = PastaSaco + "/Saco_Plastico.mat";
    private const string NomeMaterialSacoNoFbx = "SacoPlastico";
    private const string CorSaco = "#161618";
    private const float SuavidadeSaco = 0.68f;   // plástico liso (rugosidade 0,32 no Blender)

    public const string NomeParametro = "Carregando";
    public const string NomeCamada = "Carregando";
    public const string NomeClipe = "Carregando";
    public const string NomeEstadoVazio = "Vazio";
    public const string NomeEstadoSegurando = "SegurandoPneu";
    public const string NomeCamadaSaco = "CarregandoSaco";
    public const string NomeClipeSaco = "CarregandoSaco";
    public const string NomeEstadoSaco = "SegurandoSaco";
    public const float DuracaoTransicao = 0.15f;

    private const string NomeModelo = "Modelo";
    public const string NomeOssoPeito = "Chest";
    public const string NomeOssoMao = "RightHand";
    public const string NomePontoDeCarga = "PontoDeCarga";
    public const string NomeSacoNaMao = "SacoNaMao";

    // Ossos que a camada "Carregando" controla (o resto vem da locomoção).
    public static readonly string[] OssosDaMascara =
    {
        "Spine", "Chest",
        "LeftUpperArm", "LeftLowerArm", "LeftHand",
        "RightUpperArm", "RightLowerArm", "RightHand",
    };

    // Ossos que a camada "CarregandoSaco" controla: só o braço direito.
    public static readonly string[] OssosDaMascaraSaco = { "RightUpperArm", "RightLowerArm", "RightHand" };

    /* ---- Pose (ajustada no preview da Task 11) ----
     * PontoDeCarga no referencial do osso Chest (pneu com o eixo da roda
     * no X local, como o rubber tries da cena): o centro do pneu 0,46 m à
     * frente e ~1,15 m do chão, com as palmas da pose Carregando na banda
     * dele, 45° acima da linha do centro; de pé, com o eixo da roda para a
     * frente e o topo 8° para a frente. Assim as mãos entram nos cantos
     * inferiores da tela e a mira fica livre até ~12° para baixo. */
    public static readonly Vector3 PosicaoDaCarga = new Vector3(0.003f, -0.041f, -0.466f);
    public static readonly Vector3 GiroDaCarga = new Vector3(359.3f, 90.1f, 347f);

    /* SacoNaMao no referencial do osso RightHand: a boca 8,5 cm ao longo
     * da mão (dentro do punho fechado) e o saco pendurado na vertical,
     * de frente para a frente do agente, com a mão na pose CarregandoSaco. */
    public static readonly Vector3 PosicaoDoSaco = new Vector3(0f, 0.085f, 0f);
    public static readonly Vector3 GiroDoSaco = new Vector3(301.9f, 170.6f, 191.1f);

    /* Escala do saco com uma garrafa quase nada (x) e cheio (y). */
    public static readonly Vector2 EscalaDoSaco = new Vector2(0.5f, 1.0f);

    /* ================= Player ================= */

    /* Monta o PontoDeCarga no peito e o saco na mão do Player e liga o
     * kit e o FeedbackDeCarga. Não salva a cena. */
    public static void Aplicar(GameObject player)
    {
        if (player == null)
        {
            Debug.LogError("[Carga] PoseDeCarga.Aplicar: player nulo.");
            return;
        }

        Transform corpo = player.transform.Find(NomeModelo);
        if (corpo == null)
            corpo = player.transform;
        Transform peito = AcharOsso(corpo, NomeOssoPeito);
        Transform mao = AcharOsso(corpo, NomeOssoMao);
        if (peito == null || mao == null)
        {
            Debug.LogError("[Carga] Não achei os ossos \"" + NomeOssoPeito + "\" e \"" + NomeOssoMao
                           + "\" no corpo de " + player.name + ": rode Dengue > Configurar Player.");
            return;
        }

        KitDoAgente kit = player.GetComponent<KitDoAgente>();
        if (kit == null)
        {
            Debug.LogError("[Carga] " + player.name + " não tem KitDoAgente: rode Dengue > Configurar Kit do Agente.");
            return;
        }

        // ---- PontoDeCarga no peito ----
        Transform ponto = peito.Find(NomePontoDeCarga);
        if (ponto == null)
        {
            ponto = new GameObject(NomePontoDeCarga).transform;
            ponto.SetParent(peito, false);
        }
        ponto.gameObject.layer = peito.gameObject.layer;
        ponto.localPosition = PosicaoDaCarga;
        ponto.localEulerAngles = GiroDaCarga;
        ponto.localScale = Vector3.one;

        // O ponto antigo (filho da câmera, Parte A) sai, se estiver vazio.
        Transform antigo = kit.PontoDeCarga;
        if (antigo != null && antigo != ponto && antigo.name == NomePontoDeCarga && antigo.childCount == 0)
            Object.DestroyImmediate(antigo.gameObject);

        SerializedObject soKit = new SerializedObject(kit);
        soKit.FindProperty("pontoDeCarga").objectReferenceValue = ponto;
        soKit.ApplyModifiedProperties();

        // ---- Saco na mão direita ----
        Transform saco = mao.Find(NomeSacoNaMao);
        if (saco == null)
        {
            GameObject modeloSaco = AssetDatabase.LoadAssetAtPath<GameObject>(CaminhoFbxSaco);
            if (modeloSaco == null)
            {
                Debug.LogError("[Carga] Não achei " + CaminhoFbxSaco + ": rode o gerar_saco.py e o GarantirAssets.");
                return;
            }
            GameObject instancia = (GameObject)PrefabUtility.InstantiatePrefab(modeloSaco, player.scene);
            instancia.name = NomeSacoNaMao;
            saco = instancia.transform;
            saco.SetParent(mao, false);
        }
        foreach (Transform t in saco.GetComponentsInChildren<Transform>(true))
            t.gameObject.layer = mao.gameObject.layer;   // CorpoJogador: a mira não pega
        saco.localPosition = PosicaoDoSaco;
        saco.localEulerAngles = GiroDoSaco;
        saco.localScale = Vector3.one * EscalaDoSaco.y;
        saco.gameObject.SetActive(false);                // o FeedbackDeCarga mostra com garrafas

        FeedbackDeCarga feedback = player.GetComponent<FeedbackDeCarga>();
        if (feedback == null)
            feedback = player.AddComponent<FeedbackDeCarga>();
        SerializedObject soFeedback = new SerializedObject(feedback);
        soFeedback.FindProperty("kit").objectReferenceValue = kit;
        soFeedback.FindProperty("sacoNaMao").objectReferenceValue = saco;
        soFeedback.FindProperty("escalaSaco").vector2Value = EscalaDoSaco;
        soFeedback.ApplyModifiedProperties();

        if (!Application.isPlaying)
            EditorSceneManager.MarkSceneDirty(player.scene);
    }

    /* Osso pelo nome, em qualquer profundidade (o primeiro achado). */
    private static Transform AcharOsso(Transform raiz, string nome)
    {
        return raiz.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == nome);
    }

    /* ================= Assets ================= */

    /* Clipe, saco, máscara e camada do Animator (idempotente). Devolve um resumo. */
    public static string GarantirAssets()
    {
        string clipe = GarantirClipesDeCarga();
        string saco = GarantirImportacaoDoSaco();
        string camada = GarantirCamadaDeCarga();
        return clipe + "\n" + saco + "\n" + camada;
    }

    /* Os takes "Carregando" e "CarregandoSaco" do AgenteSaude.fbx viram clipes. */
    public static string GarantirClipesDeCarga()
    {
        var imp = AssetImporter.GetAtPath(CaminhoFbxAgente) as ModelImporter;
        if (imp == null)
            return "[Carga] ERRO: não achei " + CaminhoFbxAgente;

        List<ModelImporterClipAnimation> clipes = imp.clipAnimations.ToList();
        var adicionados = new List<string>();
        foreach (string nome in new[] { NomeClipe, NomeClipeSaco })
        {
            if (clipes.Any(c => c.name == nome))
                continue;

            ModelImporterClipAnimation novo = imp.defaultClipAnimations
                .FirstOrDefault(c => c.takeName == nome || c.takeName.EndsWith("|" + nome));
            if (novo == null)
                return "[Carga] ERRO: o FBX não tem o take \"" + nome + "\" (rode o gerar_agente.py e copie o FBX)";

            // Os mesmos ajustes dos clipes do PlayerBuilder.
            novo.name = nome;
            novo.loopTime = true;
            novo.loopPose = false;
            novo.lockRootRotation = true;
            novo.lockRootHeightY = true;
            novo.lockRootPositionXZ = true;
            novo.keepOriginalOrientation = true;
            novo.keepOriginalPositionY = true;
            novo.keepOriginalPositionXZ = true;
            clipes.Add(novo);
            adicionados.Add(nome);
        }

        if (adicionados.Count == 0)
            return "[Carga] clipes de carga já importados";

        imp.clipAnimations = clipes.ToArray();
        imp.SaveAndReimport();
        return "[Carga] clipes adicionados: " + string.Join(", ", adicionados) + " (" + clipes.Count + " clipes)";
    }

    /* Import do Saco.fbx (como os focos do Blender) e o material dele. */
    public static string GarantirImportacaoDoSaco()
    {
        var imp = AssetImporter.GetAtPath(CaminhoFbxSaco) as ModelImporter;
        if (imp == null)
            return "[Carga] ERRO: não achei " + CaminhoFbxSaco;

        Material mat = AssetDatabase.LoadAssetAtPath<Material>(CaminhoMaterialSaco);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(mat, CaminhoMaterialSaco);
        }
        ColorUtility.TryParseHtmlString(CorSaco, out Color cor);
        mat.SetColor("_BaseColor", cor);
        mat.SetColor("_Color", cor);
        mat.SetFloat("_Smoothness", SuavidadeSaco);
        mat.SetFloat("_Metallic", 0f);
        mat.enableInstancing = true;
        EditorUtility.SetDirty(mat);

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
        imp.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), NomeMaterialSacoNoFbx), mat);
        imp.SaveAndReimport();
        AssetDatabase.SaveAssets();
        return "[Carga] Saco.fbx importado com " + CaminhoMaterialSaco;
    }

    /* Máscaras, parâmetro e as camadas "Carregando" e "CarregandoSaco" no
     * controller do agente. */
    public static string GarantirCamadaDeCarga()
    {
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(CaminhoController);
        AnimationClip[] clipesDoFbx = AssetDatabase.LoadAllAssetsAtPath(CaminhoFbxAgente)
            .OfType<AnimationClip>()
            .ToArray();
        AnimationClip clipe = clipesDoFbx.FirstOrDefault(c => c.name == NomeClipe);
        AnimationClip clipeSaco = clipesDoFbx.FirstOrDefault(c => c.name == NomeClipeSaco);
        if (controller == null || clipe == null || clipeSaco == null)
            return "[Carga] ERRO: falta o controller ou um clipe de carga (GarantirClipesDeCarga)";

        AvatarMask mascara = GarantirMascara(CaminhoMascara, OssosDaMascara);
        AvatarMask mascaraSaco = GarantirMascara(CaminhoMascaraSaco, OssosDaMascaraSaco);
        if (mascara == null || mascaraSaco == null)
            return "[Carga] ERRO: não achei o modelo " + CaminhoFbxAgente + " para as máscaras";

        if (!controller.parameters.Any(p => p.name == NomeParametro))
            controller.AddParameter(NomeParametro, AnimatorControllerParameterType.Bool);

        AnimatorStateMachine maquina = RecriarCamada(controller, NomeCamada, mascara);
        AnimatorState vazio = maquina.AddState(NomeEstadoVazio, new Vector3(300f, 0f, 0f));
        vazio.writeDefaultValues = false;   // sem clipe: guarda a última pose até o peso ir a 0 (FeedbackDeCarga)
        AnimatorState segurando = maquina.AddState(NomeEstadoSegurando, new Vector3(300f, 120f, 0f));
        segurando.motion = clipe;
        segurando.writeDefaultValues = false;
        maquina.defaultState = vazio;

        AnimatorStateTransition pegar = vazio.AddTransition(segurando);
        pegar.hasExitTime = false;
        pegar.duration = DuracaoTransicao;
        pegar.AddCondition(AnimatorConditionMode.If, 0f, NomeParametro);

        AnimatorStateTransition largar = segurando.AddTransition(vazio);
        largar.hasExitTime = false;
        largar.duration = DuracaoTransicao;
        largar.AddCondition(AnimatorConditionMode.IfNot, 0f, NomeParametro);

        // O saco não precisa de bool nem de estado vazio: é um estado só, e o
        // peso da camada (FeedbackDeCarga) liga e desliga o braço.
        AnimatorStateMachine maquinaSaco = RecriarCamada(controller, NomeCamadaSaco, mascaraSaco);
        AnimatorState segurandoSaco = maquinaSaco.AddState(NomeEstadoSaco, new Vector3(300f, 0f, 0f));
        segurandoSaco.motion = clipeSaco;
        segurandoSaco.writeDefaultValues = false;
        maquinaSaco.defaultState = segurandoSaco;

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        return "[Carga] camadas \"" + NomeCamada + "\" e \"" + NomeCamadaSaco + "\" (peso 0) e bool \""
               + NomeParametro + "\" no controller (" + controller.layers.Length + " camadas, "
               + controller.parameters.Length + " parâmetros)";
    }

    /* Camada recriada a cada vez, com peso 0 e a máscara: o RemoveLayer apaga
     * a máquina de estados antiga (estados e transições) do asset. */
    private static AnimatorStateMachine RecriarCamada(AnimatorController controller, string nome, AvatarMask mascara)
    {
        int existente = Array.FindIndex(controller.layers, l => l.name == nome);
        if (existente >= 0)
            controller.RemoveLayer(existente);
        controller.AddLayer(nome);

        AnimatorControllerLayer[] camadas = controller.layers;
        int indice = camadas.Length - 1;
        camadas[indice].defaultWeight = 0f;
        camadas[indice].avatarMask = mascara;
        camadas[indice].blendingMode = AnimatorLayerBlendingMode.Override;
        controller.layers = camadas;
        return camadas[indice].stateMachine;
    }

    /* Máscara de transforms do rig Generic: só os ossos pedidos ativos. */
    private static AvatarMask GarantirMascara(string caminhoMascara, string[] ossos)
    {
        GameObject modelo = AssetDatabase.LoadAssetAtPath<GameObject>(CaminhoFbxAgente);
        if (modelo == null)
            return null;

        AvatarMask mascara = AssetDatabase.LoadAssetAtPath<AvatarMask>(caminhoMascara);
        if (mascara == null)
        {
            mascara = new AvatarMask();
            AssetDatabase.CreateAsset(mascara, caminhoMascara);
        }

        mascara.transformCount = 0;
        mascara.AddTransformPath(modelo.transform, true);
        for (int i = 0; i < mascara.transformCount; i++)
        {
            string caminho = mascara.GetTransformPath(i);
            string nome = caminho.Substring(caminho.LastIndexOf('/') + 1);
            mascara.SetTransformActive(i, ossos.Contains(nome));
        }
        EditorUtility.SetDirty(mascara);
        return mascara;
    }
}
