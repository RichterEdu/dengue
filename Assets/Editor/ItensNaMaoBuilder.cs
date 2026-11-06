using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

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
 *
 *  AplicarNoPlayer(player) — idempotente: BaldeNaMao, TelaNaMao e
 *  BorrifadorNaMao filhos do osso RightHand, inativos, com as partículas
 *  do gesto, e o componente ItensNaMao ligado.
 *
 *  AdicionarTelaAoRalo() — idempotente: a mesma tela no Ralo.prefab, sob a
 *  grelha fechada, em escala local 1. Por ser filha do prefab, acompanha a
 *  escala de cada ralo da cena.
 *
 *  Menu  Dengue > Configurar Itens na Mão  roda tudo e salva a cena.
 *  Rodar de novo depois do Dengue > Configurar Player, que destrói o
 *  "Modelo" e recria o controller.
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

    // Itens no referencial do osso RightHand, com a mão nas poses Carregando*.
    private const string NomeBalde = "BaldeNaMao";
    private const string NomeTela = "TelaNaMao";
    private const string NomeBorrifador = "BorrifadorNaMao";
    private const string NomeAreiaCaindo = "AreiaCaindo";
    private const string NomeNevoa = "Nevoa";
    private const string NomeOssoOmbro = "RightUpperArm";

    private const float EscalaDaTelaNaMao = 0.67f;

    private static readonly Vector3 PosicaoDoBalde = new Vector3(0f, 0.070f, 0f);
    private static readonly Vector3 GiroDoBalde = new Vector3(302.3f, 170.8f, 190.7f);
    private static readonly Vector3 PosicaoDaTela = new Vector3(-0.066f, 0.093f, -0.090f);
    private static readonly Vector3 GiroDaTela = new Vector3(4.2f, 172.9f, 208.1f);
    private static readonly Vector3 PosicaoDoBorrifador = new Vector3(-0.010f, 0.067f, -0.050f);
    private static readonly Vector3 GiroDoBorrifador = new Vector3(274.7f, 106.7f, 257.9f);

    // Tela no ralo: filha do prefab, ao lado da Tampa, para herdar a escala de cada
    // ralo. O centro fica logo abaixo da grelha fechada (3,4 cm, no FocosBlenderBuilder).
    private const string CaminhoRalo = "Assets/Meus/Assets Gerados/Focos/Ralo.prefab";
    private const string NomeTelaInstalada = "TelaInstalada";
    private const float AlturaDaTelaNoRalo = 0.029f;

    // Partículas no referencial de cada item.
    private static readonly Vector3 PosicaoDaAreiaCaindo = new Vector3(0f, -0.16f, 0.10f);
    private static readonly Vector3 PosicaoDaNevoa = new Vector3(0f, 0.034f, 0.088f);

    [MenuItem("Dengue/Configurar Itens na Mão")]
    public static void Configurar()
    {
        ControladorJogador jogador = Object.FindFirstObjectByType<ControladorJogador>();
        if (jogador == null)
        {
            Debug.LogError("[Itens] Abra a cena do jogo: não achei o Player.");
            return;
        }

        string importacao = GarantirImportacaoDosItens();
        string camada = GarantirCamadaDosItens();
        if (importacao.Contains("ERRO") || camada.Contains("ERRO"))
        {
            Debug.LogError(importacao + "\n" + camada);
            return;
        }

        AplicarNoPlayer(jogador.gameObject);
        AdicionarTelaAoRalo();
        EditorSceneManager.SaveScene(jogador.gameObject.scene);
        Debug.Log(importacao + "\n" + camada + "\n[Itens] Player montado.");
    }

    /* Os itens na mão direita, as partículas e o ItensNaMao (idempotente).
     * Não salva a cena: quem chama decide. */
    public static void AplicarNoPlayer(GameObject player)
    {
        if (player == null)
        {
            Debug.LogError("[Itens] AplicarNoPlayer: player nulo.");
            return;
        }

        Transform corpo = player.transform.Find("Modelo");
        if (corpo == null)
            corpo = player.transform;
        Transform mao = PoseDeCarga.AcharOsso(corpo, PoseDeCarga.NomeOssoMao);
        Transform ombro = PoseDeCarga.AcharOsso(corpo, NomeOssoOmbro);
        KitDoAgente kit = player.GetComponent<KitDoAgente>();
        if (mao == null || ombro == null || kit == null)
        {
            Debug.LogError("[Itens] " + player.name + " sem mão, ombro ou KitDoAgente: rode Dengue > Configurar Player e o kit.");
            return;
        }

        Transform balde = GarantirItem(mao, NomeBalde, "Balde", PosicaoDoBalde, GiroDoBalde, 1f);
        Transform tela = GarantirItem(mao, NomeTela, "Tela", PosicaoDaTela, GiroDaTela, EscalaDaTelaNaMao);
        Transform borrifador = GarantirItem(mao, NomeBorrifador, "Borrifador",
                                            PosicaoDoBorrifador, GiroDoBorrifador, 1f);
        if (balde == null || tela == null || borrifador == null)
            return;

        ParticleSystem areiaCaindo = GarantirAreiaCaindo(balde);
        ParticleSystem nevoa = GarantirNevoa(borrifador);
        foreach (Transform item in new[] { balde, tela, borrifador })
            foreach (Transform t in item.GetComponentsInChildren<Transform>(true))
                t.gameObject.layer = mao.gameObject.layer;   // CorpoJogador: a mira não pega

        ItensNaMao itens = player.GetComponent<ItensNaMao>();
        if (itens == null)
            itens = player.AddComponent<ItensNaMao>();

        SerializedObject so = new SerializedObject(itens);
        so.FindProperty("kit").objectReferenceValue = kit;
        so.FindProperty("interactor").objectReferenceValue = Object.FindFirstObjectByType<Interactor>();
        so.FindProperty("jogador").objectReferenceValue = player.GetComponent<ControladorJogador>();
        so.FindProperty("animator").objectReferenceValue = player.GetComponentInChildren<Animator>(true);
        so.FindProperty("baldeNaMao").objectReferenceValue = balde;
        so.FindProperty("areiaDoBalde").objectReferenceValue = balde.Find("Areia").gameObject;
        so.FindProperty("telaNaMao").objectReferenceValue = tela;
        so.FindProperty("borrifadorNaMao").objectReferenceValue = borrifador;
        so.FindProperty("areiaCaindo").objectReferenceValue = areiaCaindo;
        so.FindProperty("nevoa").objectReferenceValue = nevoa;
        so.FindProperty("ombroDireito").objectReferenceValue = ombro;
        so.ApplyModifiedPropertiesWithoutUndo();

        if (!Application.isPlaying && player.scene.IsValid())
            EditorSceneManager.MarkSceneDirty(player.scene);
    }

    /* A tela com moldura no Ralo.prefab, sob a grelha fechada e inativa: o
     * RaloInteractable a mostra ao concluir. Edita o prefab sem recriá-lo,
     * para não perder os valores gravados pelo kit. O objeto "Tela" do
     * Ralo.fbx fica no prefab, inativo e sem referência (idempotente). */
    public static void AdicionarTelaAoRalo()
    {
        string caminhoTela = Pasta + "/Tela.fbx";
        GameObject modelo = AssetDatabase.LoadAssetAtPath<GameObject>(caminhoTela);
        if (modelo == null)
        {
            Debug.LogError("[Itens] Não achei " + caminhoTela + ": rode o gerar_itens.py.");
            return;
        }

        GameObject raiz = PrefabUtility.LoadPrefabContents(CaminhoRalo);
        try
        {
            SerializedObject ralo = new SerializedObject(raiz.GetComponent<RaloInteractable>());
            Transform tampa = (Transform)ralo.FindProperty("tampa").objectReferenceValue;
            Transform pai = tampa.parent;

            Transform tela = pai.Find(NomeTelaInstalada);
            if (tela == null)
            {
                GameObject instancia = (GameObject)PrefabUtility.InstantiatePrefab(modelo, pai);
                instancia.name = NomeTelaInstalada;
                tela = instancia.transform;
            }

            foreach (Transform t in tela.GetComponentsInChildren<Transform>(true))
                t.gameObject.layer = tampa.gameObject.layer;
            tela.localPosition = new Vector3(0f, AlturaDaTelaNoRalo, 0f);
            tela.localRotation = Quaternion.identity;
            tela.localScale = Vector3.one;
            tela.gameObject.SetActive(false);

            ralo.FindProperty("tela").objectReferenceValue = tela.gameObject;
            ralo.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(raiz, CaminhoRalo);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(raiz);
        }
    }

    /* Instância do FBX filha da mão, inativa (o ItensNaMao mostra). */
    private static Transform GarantirItem(Transform mao, string nome, string fbx,
                                          Vector3 posicao, Vector3 giro, float escala)
    {
        Transform item = mao.Find(nome);
        if (item == null)
        {
            string caminho = Pasta + "/" + fbx + ".fbx";
            GameObject modelo = AssetDatabase.LoadAssetAtPath<GameObject>(caminho);
            if (modelo == null)
            {
                Debug.LogError("[Itens] Não achei " + caminho + ": rode o gerar_itens.py.");
                return null;
            }

            GameObject instancia = (GameObject)PrefabUtility.InstantiatePrefab(modelo, mao);
            instancia.name = nome;
            item = instancia.transform;
        }

        item.localPosition = posicao;
        item.localEulerAngles = giro;
        item.localScale = Vector3.one * escala;
        item.gameObject.SetActive(false);
        return item;
    }

    private static ParticleSystem GarantirAreiaCaindo(Transform balde)
    {
        ParticleSystem particulas = GarantirParticulas(balde, NomeAreiaCaindo, PosicaoDaAreiaCaindo,
                                                       Hexa("#D6B87A"), 60, 40f, 0.5f, 0.2f, 1f);
        ParticleSystem.MainModule principal = particulas.main;
        principal.startSize = new ParticleSystem.MinMaxCurve(0.012f, 0.02f);

        ParticleSystem.ShapeModule forma = particulas.shape;
        forma.shapeType = ParticleSystemShapeType.Circle;
        forma.radius = 0.04f;
        return particulas;
    }

    private static ParticleSystem GarantirNevoa(Transform borrifador)
    {
        Color cor = Color.white;
        cor.a = 0.6f;
        ParticleSystem particulas = GarantirParticulas(borrifador, NomeNevoa, PosicaoDaNevoa,
                                                       cor, 40, 50f, 0.35f, 1.2f, 0f);
        ParticleSystem.MainModule principal = particulas.main;
        principal.startSize = 0.05f;

        ParticleSystem.ShapeModule forma = particulas.shape;
        forma.shapeType = ParticleSystemShapeType.Cone;
        forma.angle = 14f;
        forma.radius = 0.002f;

        ParticleSystem.SizeOverLifetimeModule tamanho = particulas.sizeOverLifetime;
        tamanho.enabled = true;
        tamanho.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 2.4f));

        ParticleSystem.ColorOverLifetimeModule desbotar = particulas.colorOverLifetime;
        desbotar.enabled = true;
        Gradient gradiente = new Gradient();
        gradiente.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                          new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
        desbotar.color = gradiente;
        return particulas;
    }

    /* Partículas em espaço local do item: o sistema lê o emissor antes do
     * LateUpdate, onde o ombro gira com o olhar. Em espaço de mundo, elas
     * nasceriam longe do bico ao olhar para baixo. Sem sombra e com a emissão
     * desligada: o ItensNaMao liga durante o gesto. */
    private static ParticleSystem GarantirParticulas(Transform item, string nome, Vector3 posicao, Color cor,
                                                     int maximo, float porSegundo, float vida,
                                                     float velocidade, float gravidade)
    {
        Transform existente = item.Find(nome);
        GameObject objeto = existente != null ? existente.gameObject : new GameObject(nome);
        objeto.transform.SetParent(item, false);
        objeto.transform.localPosition = posicao;
        objeto.transform.localRotation = Quaternion.identity;

        ParticleSystem particulas = objeto.GetComponent<ParticleSystem>();
        if (particulas == null)
            particulas = objeto.AddComponent<ParticleSystem>();

        ParticleSystem.MainModule principal = particulas.main;
        principal.loop = true;
        principal.playOnAwake = true;
        principal.startLifetime = vida;
        principal.startSpeed = velocidade;
        principal.startColor = cor;
        principal.gravityModifier = gravidade;
        principal.maxParticles = maximo;
        principal.simulationSpace = ParticleSystemSimulationSpace.Local;

        ParticleSystem.EmissionModule emissao = particulas.emission;
        emissao.enabled = false;
        emissao.rateOverTime = porSegundo;

        ParticleSystemRenderer render = objeto.GetComponent<ParticleSystemRenderer>();
        render.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(CaminhoMaterialParticula);
        render.shadowCastingMode = ShadowCastingMode.Off;
        render.receiveShadows = false;
        return particulas;
    }

    private static Color Hexa(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out Color cor);
        return cor;
    }

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
