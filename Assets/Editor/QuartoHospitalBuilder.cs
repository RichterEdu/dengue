using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/* ============================================================
 *  QuartoHospitalBuilder.cs  (Editor)
 *  ---------------------------------------------------------------------
 *  Cria uma salinha de posto de saúde — escondida bem abaixo do mapa,
 *  longe de qualquer outra geometria — montada com peças de verdade do
 *  pack "Office" (paredes, moldura de porta, piso em ladrilho, balcão,
 *  cadeira, armário, dispensador de álcool em gel, tapete, quadro), em
 *  vez de cubos esticados. É a abertura jogável: o jogo começa aqui, o
 *  jogador anda até a porta e é liberado pro bairro.
 *
 *  Menu:  Dengue > Criar Sala do Hospital
 *
 *  IMPORTANTE sobre os prefabs do pack Office: o pivô de cada peça
 *  modular (parede, moldura de porta, piso) fica NA QUINA da malha, não
 *  no centro — confirmado medindo `MeshFilter.sharedMesh.bounds` de
 *  cada um (ex.: wall_standard tem bounds locais min=(-2,0,-0.1)
 *  max=(0,3,0.1): a malha inteira fica do lado NEGATIVO de X, o pivô é
 *  a quina). A matemática de posicionamento abaixo foi deduzida a
 *  partir disso (não é um chute): pra cada rotação (0/90/180/270), o
 *  eixo local X da peça aponta pra uma direção diferente no mundo
 *  (verificado com Quaternion.Euler(0,ang,0) * Vector3.right), então a
 *  fórmula do "próximo módulo" muda de sinal conforme a parede.
 *
 *  Hierarquia criada:
 *    SalaDoHospital           (bem abaixo do mapa, Y = -30)
 *    ├── Piso                 (3x3 floor_tile, 6x6 m)
 *    ├── Teto
 *    ├── ParedeNorte / ParedeLeste / ParedeOeste   (3x wall_standard cada)
 *    ├── ParedeSul             (2x wall_standard + 1x wall_standard_door_frame no meio = porta)
 *    ├── LuzA / LuzB           (duas luzes fracas em cantos opostos — sem hotspot)
 *    ├── Mobilia               (Balcao, Dispensador, Cadeira, Armario, Relogio, Tapete, Quadro)
 *    └── PortaTrigger          (BoxCollider isTrigger + SaidaDoHospital.cs, na abertura da porta)
 *    FluxoDeJogo               (objeto solto na raiz da cena — estado da partida; só cria se ainda não existir)
 *
 *  RECONSTRUÍVEL: se "SalaDoHospital" já existir, esta ferramenta apaga
 *  e reconstrói do zero, reaproveitando a posição no bairro que já
 *  tinha sido gravada na porta antiga (não lê a posição atual do
 *  player, que a essa altura já está dentro da sala) — assim dá pra
 *  rodar de novo quantas vezes quiser pra ajustar o layout. Os
 *  materiais tingidos gerados também são apagados e recriados a cada
 *  rodada (ver `LimparMateriaisAntigos`), pra nunca acumular lixo nem
 *  ficar com cor desatualizada por causa do cache.
 * ============================================================ */

public static class QuartoHospitalBuilder
{
    private const string RootName = "SalaDoHospital";
    private static readonly Vector3 CentroDaSala = new Vector3(0f, -30f, 0f);

    // Dimensões reais medidas nos prefabs do pack Office (bounds das malhas).
    private const float ModuloParede = 2f;     // wall_standard / floor_tile: 2m de módulo
    private const float AlturaParede = 3f;     // wall_standard: 3m de altura
    private const int ModulosPorLado = 3;      // sala de 3x3 módulos = 6x6 m

    // Paredes claras, piso mais escuro — contraste de ambiente real em vez de
    // tudo pintado no mesmo tom pastel (o que ficava "achatado"/artificial).
    private static readonly Color CorParede = new Color(0.87f, 0.88f, 0.86f);
    private static readonly Color CorTeto = new Color(0.93f, 0.94f, 0.92f);
    private static readonly Color CorPiso = new Color(0.30f, 0.36f, 0.42f);

    private const string CaminhoParede = "Assets/Office/Prefabs/Building/wall_standard.prefab";
    private const string CaminhoParedeComPorta = "Assets/Office/Prefabs/Building/wall_standard_door_frame.prefab";
    private const string CaminhoPiso = "Assets/Office/Prefabs/Building/floor_tile.prefab";
    private const string CaminhoBalcao = "Assets/Office/Prefabs/Furniture/counter_a_1.prefab";
    private const string CaminhoCadeira = "Assets/Office/Prefabs/Office/standard_office_chair_1.prefab";
    private const string CaminhoArmario = "Assets/Office/Prefabs/Furniture/file_cabinet_medium.prefab";
    private const string CaminhoDispensador = "Assets/Office/Prefabs/Deco/alcohol_dispenser_a.prefab";
    private const string CaminhoRelogio = "Assets/Office/Prefabs/Office/clock.prefab";
    private const string CaminhoTapete = "Assets/Office/Prefabs/Deco/rug_a.prefab";
    private const string CaminhoQuadro = "Assets/Office/Prefabs/Deco/big_painting.prefab";

    [MenuItem("Dengue/Criar Sala do Hospital")]
    public static void Criar()
    {
        Vector3 posicaoOriginalNoBairro;

        GameObject salaExistente = GameObject.Find(RootName);
        if (salaExistente != null)
        {
            SaidaDoHospital saidaAntiga = salaExistente.GetComponentInChildren<SaidaDoHospital>(true);
            if (saidaAntiga == null)
            {
                EditorUtility.DisplayDialog("Sala do Hospital",
                    "Já existe uma \"" + RootName + "\" na cena, mas não achei a porta antiga pra recuperar a posição no bairro.\n\nApague \"" + RootName + "\" manualmente na Hierarchy e rode de novo com o player já no bairro.",
                    "OK");
                return;
            }
            posicaoOriginalNoBairro = new SerializedObject(saidaAntiga).FindProperty("posicaoNoBairro").vector3Value;
            Undo.DestroyObjectImmediate(salaExistente);
        }
        else
        {
            CharacterController player = Object.FindFirstObjectByType<CharacterController>();
            if (player == null)
            {
                EditorUtility.DisplayDialog("Sala do Hospital",
                    "Não encontrei nenhum CharacterController (o player) na cena.",
                    "OK");
                return;
            }
            posicaoOriginalNoBairro = player.transform.position;
        }

        GameObject parede = AssetDatabase.LoadAssetAtPath<GameObject>(CaminhoParede);
        GameObject paredeComPorta = AssetDatabase.LoadAssetAtPath<GameObject>(CaminhoParedeComPorta);
        GameObject piso = AssetDatabase.LoadAssetAtPath<GameObject>(CaminhoPiso);
        if (parede == null || paredeComPorta == null || piso == null)
        {
            EditorUtility.DisplayDialog("Sala do Hospital",
                "Não encontrei os prefabs do pack \"Office\" (paredes/piso). Confirme se a pasta Assets/Office ainda existe no projeto.",
                "OK");
            return;
        }

        LimparMateriaisAntigos();

        float metade = ModulosPorLado * ModuloParede * 0.5f; // 3 m (meia-sala)

        /* ---------- Sala ---------- */
        GameObject root = new GameObject(RootName);
        Undo.RegisterCreatedObjectUndo(root, "Criar Sala do Hospital");
        root.transform.position = CentroDaSala;

        /* ----- Piso: grade 3x3, mesma regra de eixo X da parede Norte (rot. 0°)
         * e de eixo Z da parede Sul (rot. 180°) — ver comentário de topo. ----- */
        GameObject pisoRoot = new GameObject("Piso");
        Undo.RegisterCreatedObjectUndo(pisoRoot, "Criar Sala do Hospital");
        pisoRoot.transform.SetParent(root.transform, false);
        for (int i = 0; i < ModulosPorLado; i++)
        {
            float x = -metade + ModuloParede * (i + 1);
            for (int j = 0; j < ModulosPorLado; j++)
            {
                float z = -metade + ModuloParede * j;
                Instanciar(piso, pisoRoot.transform, new Vector3(x, 0f, z), 0f, CorPiso);
            }
        }

        // Teto: fecha a sala visualmente. Cubo simples (não floor_tile — a face
        // de baixo de um floor_tile não é confiável vista de baixo).
        GameObject teto = GameObject.CreatePrimitive(PrimitiveType.Cube);
        teto.name = "Teto";
        Undo.RegisterCreatedObjectUndo(teto, "Criar Sala do Hospital");
        teto.transform.SetParent(root.transform, false);
        teto.transform.localPosition = new Vector3(0f, AlturaParede + 0.1f, 0f);
        teto.transform.localScale = new Vector3(metade * 2f, 0.2f, metade * 2f);
        teto.GetComponent<Renderer>().sharedMaterial = MaterialTingido(teto.GetComponent<Renderer>().sharedMaterial, CorTeto);

        /* ----- Parede Norte (rot. 0°, Z fixo = -metade): eixo local X da peça
         * aponta pra +X no mundo sem rotação, e a malha se estende do pivô
         * PRA TRÁS em X (pivô é a quina direita). Pra cobrir [-metade,+metade]
         * com N módulos consecutivos, os pivôs ficam em
         * -metade + Modulo*(i+1), i=0..N-1. ----- */
        GameObject paredeNorte = new GameObject("ParedeNorte");
        Undo.RegisterCreatedObjectUndo(paredeNorte, "Criar Sala do Hospital");
        paredeNorte.transform.SetParent(root.transform, false);
        for (int i = 0; i < ModulosPorLado; i++)
        {
            float x = -metade + ModuloParede * (i + 1);
            Instanciar(parede, paredeNorte.transform, new Vector3(x, 0f, -metade), 0f, CorParede);
        }

        /* ----- Parede Sul (rot. 180°, Z fixo = +metade): a rotação de 180°
         * inverte o eixo local X pra -X no mundo, então a malha passa a se
         * estender do pivô PRA FRENTE em X — pivôs em -metade + Modulo*i. ----- */
        GameObject paredeSul = new GameObject("ParedeSul");
        Undo.RegisterCreatedObjectUndo(paredeSul, "Criar Sala do Hospital");
        paredeSul.transform.SetParent(root.transform, false);
        for (int i = 0; i < ModulosPorLado; i++)
        {
            float x = -metade + ModuloParede * i;
            bool ehPorta = i == ModulosPorLado / 2;
            Instanciar(ehPorta ? paredeComPorta : parede, paredeSul.transform, new Vector3(x, 0f, metade), 180f, CorParede);
        }

        /* ----- Parede Leste (rot. 90°, X fixo = +metade): a 90° o eixo local
         * X aponta pra -Z, então a malha se estende do pivô PRA FRENTE em Z
         * (mesmo sinal da parede Sul) — pivôs em -metade + Modulo*i. ----- */
        GameObject paredeLeste = new GameObject("ParedeLeste");
        Undo.RegisterCreatedObjectUndo(paredeLeste, "Criar Sala do Hospital");
        paredeLeste.transform.SetParent(root.transform, false);
        for (int i = 0; i < ModulosPorLado; i++)
        {
            float z = -metade + ModuloParede * i;
            Instanciar(parede, paredeLeste.transform, new Vector3(metade, 0f, z), 90f, CorParede);
        }

        /* ----- Parede Oeste (rot. 270°, X fixo = -metade): a 270° o eixo
         * local X aponta pra +Z, mesmo sinal da parede Norte — pivôs em
         * -metade + Modulo*(i+1). ATENÇÃO: a versão anterior usava 90° nas
         * DUAS paredes (Leste e Oeste) por engano — era a causa dos buracos
         * mostrando o bairro "flutuando" ao fundo. ----- */
        GameObject paredeOeste = new GameObject("ParedeOeste");
        Undo.RegisterCreatedObjectUndo(paredeOeste, "Criar Sala do Hospital");
        paredeOeste.transform.SetParent(root.transform, false);
        for (int i = 0; i < ModulosPorLado; i++)
        {
            float z = -metade + ModuloParede * (i + 1);
            Instanciar(parede, paredeOeste.transform, new Vector3(-metade, 0f, z), 270f, CorParede);
        }

        /* ----- Luzes: duas, fracas, em cantos opostos e na ALTURA MÉDIA da
         * sala — não coladas no teto. Perto de uma superfície, mesmo uma luz
         * "fraca" estoura (lei do inverso do quadrado); duas fracas e
         * afastadas de tudo iluminam mais parelho, sem hotspot. ----- */
        CriarLuz("LuzA", root.transform, new Vector3(-metade + 1.2f, AlturaParede * 0.55f, -metade + 1.2f));
        CriarLuz("LuzB", root.transform, new Vector3(metade - 1.2f, AlturaParede * 0.55f, metade - 1.2f));

        /* ---------- Mobília ---------- */
        MobiliarSala(root.transform, metade);

        /* ---------- Porta (trigger de saída) ---------- */
        GameObject porta = new GameObject("PortaTrigger", typeof(BoxCollider));
        Undo.RegisterCreatedObjectUndo(porta, "Criar Sala do Hospital");
        porta.transform.SetParent(root.transform, false);
        porta.transform.localPosition = new Vector3(0f, AlturaParede * 0.5f, metade + 0.6f);
        BoxCollider trigger = porta.GetComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = new Vector3(1.4f, AlturaParede, 1f);

        SaidaDoHospital saida = Undo.AddComponent<SaidaDoHospital>(porta);
        SerializedObject soSaida = new SerializedObject(saida);
        soSaida.FindProperty("posicaoNoBairro").vector3Value = posicaoOriginalNoBairro;
        soSaida.ApplyModifiedPropertiesWithoutUndo();

        /* ---------- Move o jogador pra dentro da sala ---------- */
        CharacterController playerAtual = Object.FindFirstObjectByType<CharacterController>();
        if (playerAtual != null)
        {
            Undo.RecordObject(playerAtual.transform, "Criar Sala do Hospital");
            playerAtual.transform.position = CentroDaSala + new Vector3(0f, 1.2f, -metade + 1.6f);
        }

        /* ---------- Estado da partida (só cria se ainda não existir) ---------- */
        if (Object.FindFirstObjectByType<FluxoDeJogo>() == null)
        {
            GameObject fluxoGo = new GameObject("FluxoDeJogo", typeof(FluxoDeJogo));
            Undo.RegisterCreatedObjectUndo(fluxoGo, "Criar Sala do Hospital");
        }

        EditorSceneManager.MarkSceneDirty(root.scene);
        Selection.activeGameObject = root;

        Debug.Log("[Sala do Hospital] Criada com peças do pack Office. Jogador movido pra dentro; posição original no bairro (" +
                   posicaoOriginalNoBairro + ") gravada na porta de saída. Rode  Dengue > Criar HUD de Fim de Jogo  se ainda não rodou.", root);
    }

    private static void CriarLuz(string nome, Transform pai, Vector3 posicaoLocal)
    {
        GameObject luzGo = new GameObject(nome, typeof(Light));
        Undo.RegisterCreatedObjectUndo(luzGo, "Criar Sala do Hospital");
        luzGo.transform.SetParent(pai, false);
        luzGo.transform.localPosition = posicaoLocal;
        Light luz = luzGo.GetComponent<Light>();
        luz.type = LightType.Point;
        luz.range = 8f;
        luz.intensity = 3.5f;
        luz.color = new Color(0.95f, 0.98f, 1f);
    }

    private static void MobiliarSala(Transform pai, float metade)
    {
        GameObject balcaoPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CaminhoBalcao);
        GameObject cadeiraPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CaminhoCadeira);
        GameObject armarioPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CaminhoArmario);
        GameObject dispensadorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CaminhoDispensador);
        GameObject relogioPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CaminhoRelogio);
        GameObject tapetePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CaminhoTapete);
        GameObject quadroPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CaminhoQuadro);

        GameObject mobilia = new GameObject("Mobilia");
        Undo.RegisterCreatedObjectUndo(mobilia, "Criar Sala do Hospital");
        mobilia.transform.SetParent(pai, false);

        // Balcão de recepção encostado na parede Norte.
        if (balcaoPrefab != null)
            Instanciar(balcaoPrefab, mobilia.transform, new Vector3(-1.5f, 0f, -metade + 0.4f), 0f, Color.white);

        // Dispensador de álcool em gel em cima do balcão (0,95 = altura do
        // balcão; a malha do dispensador fica centrada no próprio pivô, por
        // isso soma metade da altura dele — 0,20 — pra base encostar no tampo).
        if (dispensadorPrefab != null)
            Instanciar(dispensadorPrefab, mobilia.transform, new Vector3(-1.7f, 0.95f + 0.20f, -metade + 0.35f), -20f, Color.white);

        // Cadeira de frente pro balcão.
        if (cadeiraPrefab != null)
            Instanciar(cadeiraPrefab, mobilia.transform, new Vector3(-1.5f, 0f, -metade + 1.6f), 180f, Color.white);

        // Tapete na frente do balcão/cadeira — quebra o piso liso com uma
        // textura de verdade (material próprio, não cor lisa).
        if (tapetePrefab != null)
            Instanciar(tapetePrefab, mobilia.transform, new Vector3(-1.5f, 0.001f, -metade + 1.5f), 0f, Color.white);

        // Armário de remédios/prontuários no canto.
        if (armarioPrefab != null)
            Instanciar(armarioPrefab, mobilia.transform, new Vector3(metade - 0.5f, 0f, -metade + 0.4f), -90f, Color.white);

        // Relógio na parede Norte.
        if (relogioPrefab != null)
            Instanciar(relogioPrefab, mobilia.transform, new Vector3(1.8f, 2.3f, -metade + 0.08f), 0f, Color.white);

        // Quadro na parede Oeste (a única sem mobília) — quebra a parede lisa.
        if (quadroPrefab != null)
            Instanciar(quadroPrefab, mobilia.transform, new Vector3(-metade + 0.06f, 1.8f, 0f), 90f, Color.white);
    }

    private static GameObject Instanciar(GameObject prefab, Transform pai, Vector3 posicaoLocal, float rotacaoY, Color tingir)
    {
        GameObject instancia = (GameObject)PrefabUtility.InstantiatePrefab(prefab, pai);
        Undo.RegisterCreatedObjectUndo(instancia, "Criar Sala do Hospital");
        instancia.transform.localPosition = posicaoLocal;
        instancia.transform.localRotation = Quaternion.Euler(0f, rotacaoY, 0f);

        if (tingir != Color.white)
        {
            foreach (Renderer r in instancia.GetComponentsInChildren<Renderer>())
                r.sharedMaterial = MaterialTingido(r.sharedMaterial, tingir);
        }

        return instancia;
    }

    private const string PastaMateriais = "Assets/Meus/Assets Gerados/Sala do Hospital/Materiais";
    private static readonly System.Collections.Generic.Dictionary<Material, Material> CacheTingido =
        new System.Collections.Generic.Dictionary<Material, Material>();

    /* Apaga os materiais tingidos de uma rodada anterior antes de recriar —
     * sem isso, o CacheTingido (que vive até o próximo recompile) continuaria
     * devolvendo os materiais ANTIGOS mesmo depois de mudar as cores no
     * código, e cada reconstrução acumularia arquivos .mat órfãos na pasta. */
    private static void LimparMateriaisAntigos()
    {
        CacheTingido.Clear();
        if (AssetDatabase.IsValidFolder(PastaMateriais))
            AssetDatabase.DeleteAsset(PastaMateriais);
    }

    /* Deriva uma versão tingida de um material do pack Office e salva como
     * ASSET de verdade (não um Material solto em memória) — usar
     * renderer.material em modo Editor vaza material sem dono na cena;
     * um .mat salvo com AssetDatabase.CreateAsset persiste igual a
     * qualquer outro asset gerado pelos builders do projeto. */
    private static Material MaterialTingido(Material original, Color tingir)
    {
        if (original == null)
            return null;

        if (CacheTingido.TryGetValue(original, out Material existente) && existente != null)
            return existente;

        MeusOrganizador.GarantirPasta(PastaMateriais);
        string caminho = AssetDatabase.GenerateUniqueAssetPath(PastaMateriais + "/" + original.name + " (clinica).mat");

        Material tingido = new Material(original) { color = tingir };
        // Smoothness padrão (0.5) do URP/Lit cria um brilho especular forte
        // que "estoura" com qualquer luz próxima — acabamento fosco fica
        // mais parecido com parede/piso pintado, sem hotspot branco.
        if (tingido.HasProperty("_Smoothness"))
            tingido.SetFloat("_Smoothness", 0.05f);
        AssetDatabase.CreateAsset(tingido, caminho);

        CacheTingido[original] = tingido;
        return tingido;
    }
}
