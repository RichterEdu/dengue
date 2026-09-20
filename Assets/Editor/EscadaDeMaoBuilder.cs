using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Gera o asset "Escada de Mao" (escada encostavel, para subir na casa / telhado).
///
/// Menus:
///   Dengue > Criar Escada de Mao        -> gera/atualiza os assets (malhas, materiais, prefab)
///   Dengue > Adicionar Escada na Cena   -> poe a escada na cena, na frente do player, ja inclinada
///
/// Cria em Assets/Meus/Assets Gerados/Escada de Mao/:
///   - Malha Madeira.asset   (montantes + degraus, uma malha so)
///   - Malha Sapatas.asset   (pes de borracha embaixo e capas no topo)
///   - madeira da escada.mat / sapata da escada.mat
///   - Escada de Mao.prefab  (raiz com BoxCollider, sem script de interacao)
///
/// Pode ser rodado de novo: reaproveita os arquivos existentes (mantem os GUIDs).
/// Nenhum .meta e escrito a mao -- o Unity gera todos.
///
/// Estilo low poly: todas as pecas sao prismas de secao chanfrada (8 lados),
/// entao as quinas pegam a luz em vez de ficarem lisas como um cubo.
/// </summary>
public static class EscadaDeMaoBuilder
{
    // ---------------------------------------------------------------- medidas
    // Tudo em metros. Pivo no centro da base: e so largar no chao e girar no X
    // (uns -15 a -20 graus) para encostar na parede.

    const float AlturaMontante = 3.00f;   // comprimento das laterais
    const float LarguraExterna = 0.44f;   // largura total da escada
    const float MontanteEsp = 0.030f;     // espessura do montante (eixo X)
    const float MontanteProf = 0.090f;    // "altura" do montante (eixo Z) -- e o lado largo
    const float MontanteChanfro = 0.007f;

    const int Degraus = 10;
    const float PrimeiroDegrau = 0.30f;   // altura do degrau mais baixo
    const float EspacoDegraus = 0.28f;    // distancia entre degraus
    const float DegrauProf = 0.055f;      // profundidade do degrau (eixo Z) -- onde o pe pisa
    const float DegrauAlt = 0.028f;       // espessura do degrau (eixo Y)
    const float DegrauChanfro = 0.010f;   // bem chanfrado: fica quase roliço
    const float DegrauVao = 0.40f;        // comprimento do degrau (entra 1 cm em cada montante)

    const float SapataAlt = 0.025f;       // pe de borracha embaixo
    const float SapataEsp = 0.038f;
    const float SapataProf = 0.100f;
    const float CapaAlt = 0.020f;         // capa de borracha no topo do montante

    static readonly Color CorMadeira = new Color(0.520f, 0.352f, 0.192f);
    static readonly Color CorBorracha = new Color(0.115f, 0.118f, 0.125f);

    // ---------------------------------------------------------------- caminhos
    const string PastaMeus = "Assets/Meus/Assets Gerados";
    const string Pasta = "Assets/Meus/Assets Gerados/Escada de Mao";
    const string CaminhoMalhaMadeira = Pasta + "/Malha Madeira.asset";
    const string CaminhoMalhaSapatas = Pasta + "/Malha Sapatas.asset";
    const string CaminhoMatMadeira = Pasta + "/madeira da escada.mat";
    const string CaminhoMatSapata = Pasta + "/sapata da escada.mat";
    const string CaminhoPrefab = Pasta + "/Escada de Mao.prefab";

    [MenuItem("Dengue/Criar Escada de Mao")]
    public static void Criar()
    {
        GarantirPasta();

        // --- malhas -------------------------------------------------------
        Construtor madeira = new Construtor();
        Construtor borracha = new Construtor();

        float baseMadeira = SapataAlt;                       // a madeira comeca em cima da sapata
        float topoMadeira = baseMadeira + AlturaMontante;
        float xMontante = (LarguraExterna - MontanteEsp) * 0.5f;

        Vector2[] secaoMontante = RetanguloChanfrado(MontanteEsp, MontanteProf, MontanteChanfro);
        Vector2[] secaoDegrau = RetanguloChanfrado(DegrauProf, DegrauAlt, DegrauChanfro);
        Vector2[] secaoSapata = RetanguloChanfrado(SapataEsp, SapataProf, 0.006f);

        // montantes (extrusao ao longo de Y)
        for (int lado = -1; lado <= 1; lado += 2)
        {
            Vector3 centro = new Vector3(lado * xMontante, baseMadeira + AlturaMontante * 0.5f, 0f);
            madeira.Prisma(secaoMontante, AlturaMontante, EixoY(centro));

            // sapata de borracha embaixo
            Vector3 pe = new Vector3(lado * xMontante, SapataAlt * 0.5f, 0f);
            borracha.Prisma(secaoSapata, SapataAlt, EixoY(pe));

            // capa de borracha no topo
            Vector3 capa = new Vector3(lado * xMontante, topoMadeira + CapaAlt * 0.5f, 0f);
            borracha.Prisma(secaoSapata, CapaAlt, EixoY(capa));
        }

        // degraus (extrusao ao longo de X)
        for (int i = 0; i < Degraus; i++)
        {
            float y = PrimeiroDegrau + i * EspacoDegraus;
            madeira.Prisma(secaoDegrau, DegrauVao, EixoX(new Vector3(0f, y, 0f)));
        }

        Mesh malhaMadeira = SalvarMalha(madeira.Construir("Malha Madeira"), CaminhoMalhaMadeira);
        Mesh malhaSapatas = SalvarMalha(borracha.Construir("Malha Sapatas"), CaminhoMalhaSapatas);

        // --- materiais ----------------------------------------------------
        Material matMadeira = MaterialOpaco(CaminhoMatMadeira, CorMadeira, 0.12f, 0f);
        Material matSapata = MaterialOpaco(CaminhoMatSapata, CorBorracha, 0.25f, 0f);

        AssetDatabase.SaveAssets();

        // --- prefab -------------------------------------------------------
        GameObject raiz = new GameObject("Escada de Mao");

        float alturaTotal = topoMadeira + CapaAlt;
        BoxCollider col = raiz.AddComponent<BoxCollider>();
        col.center = new Vector3(0f, alturaTotal * 0.5f, 0f);
        col.size = new Vector3(LarguraExterna + 0.02f, alturaTotal, MontanteProf + 0.02f);

        PecaVisual(raiz, "Madeira", malhaMadeira, matMadeira);
        PecaVisual(raiz, "Sapatas", malhaSapatas, matSapata);

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(raiz, CaminhoPrefab);
        Object.DestroyImmediate(raiz);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = prefab;
        EditorGUIUtility.PingObject(prefab);

        Debug.Log(string.Format(
            "[Escada de Mao] Pronto: {0}  |  {1:0.00} m de altura, {2:0.00} m de largura, {3} degraus. " +
            "Arraste para a cena e gire uns -15 graus no X para encostar na parede.",
            CaminhoPrefab, alturaTotal, LarguraExterna, Degraus));
    }

    // ------------------------------------------------------- por na cena
    const float InclinacaoPadrao = 16f;   // graus de inclinacao ao encostar
    const float DistanciaDoPlayer = 2.2f; // onde a escada aparece

    /// <summary>true enquanto quem esta chamando e a instalacao automatica.</summary>
    internal static bool _autoSetup;

    [MenuItem("Dengue/Adicionar Escada na Cena")]
    public static void AdicionarNaCena()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CaminhoPrefab);
        if (prefab == null)
        {
            Criar(); // ainda nao existe: gera o asset primeiro
            prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CaminhoPrefab);
        }
        if (prefab == null)
        {
            Debug.LogError("[Escada de Mao] Nao consegui criar o prefab em " + CaminhoPrefab);
            return;
        }

        // ponto de referencia: o player, senao a camera da Scene View
        Vector3 origem;
        Vector3 frente;
        CharacterController player = Object.FindFirstObjectByType<CharacterController>();
        if (player != null)
        {
            origem = player.transform.position;
            frente = player.transform.forward;
        }
        else if (SceneView.lastActiveSceneView != null)
        {
            Transform cam = SceneView.lastActiveSceneView.camera.transform;
            origem = cam.position;
            frente = cam.forward;
        }
        else
        {
            origem = Vector3.zero;
            frente = Vector3.forward;
        }

        frente.y = 0f;
        if (frente.sqrMagnitude < 0.0001f) frente = Vector3.forward;
        frente.Normalize();

        Vector3 pos = origem + frente * DistanciaDoPlayer;

        // procura o chao logo abaixo (usa os colisores da cena)
        RaycastHit hit;
        if (Physics.Raycast(pos + Vector3.up * 8f, Vector3.down, out hit, 40f))
            pos.y = hit.point.y;
        else
            pos.y = origem.y - 1f;

        // o topo da escada tomba para o -Z local, entao o yaw aponta esse lado
        // para a frente (parede/casa) e a inclinacao fica no X local.
        Quaternion rot = Quaternion.LookRotation(-frente, Vector3.up)
                         * Quaternion.Euler(-InclinacaoPadrao, 0f, 0f);

        if (_autoSetup && GameObject.Find("Escada de Mao") != null)
        {
            Debug.Log("[Escada de Mao] Ja existe uma escada na cena -- nao coloquei outra.");
            return;
        }

        GameObject instancia = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        instancia.transform.SetPositionAndRotation(pos, rot);
        instancia.name = "Escada de Mao";

        Undo.RegisterCreatedObjectUndo(instancia, "Adicionar Escada de Mao");
        Selection.activeGameObject = instancia;
        EditorSceneManager.MarkSceneDirty(instancia.scene);
        if (SceneView.lastActiveSceneView != null) SceneView.lastActiveSceneView.FrameSelected();

        Debug.Log(string.Format(
            "[Escada de Mao] Escada colocada na cena em {0} (inclinada {1} graus). " +
            "Ela ja esta selecionada: arraste ate encostar na parede da casa e salve a cena (Ctrl+S).",
            pos, InclinacaoPadrao));
    }

    // ------------------------------------------------------------- hierarquia
    static void PecaVisual(GameObject pai, string nome, Mesh malha, Material material)
    {
        GameObject go = new GameObject(nome);
        go.transform.SetParent(pai.transform, false);
        go.AddComponent<MeshFilter>().sharedMesh = malha;
        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.sharedMaterial = material;
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
    }

    // ----------------------------------------------------------- matrizes
    // A secao e desenhada no plano XY local e extrudada em +Z local.

    /// <summary>Extrusao ao longo do eixo Y do mundo (secao.x -> X, secao.y -> Z).</summary>
    static Matrix4x4 EixoY(Vector3 centro)
    {
        return Matrix4x4.TRS(centro, Quaternion.Euler(-90f, 0f, 0f), Vector3.one);
    }

    /// <summary>Extrusao ao longo do eixo X do mundo (secao.x -> Z, secao.y -> Y).</summary>
    static Matrix4x4 EixoX(Vector3 centro)
    {
        return Matrix4x4.TRS(centro, Quaternion.Euler(0f, 90f, 0f), Vector3.one);
    }

    // ------------------------------------------------------------- secoes 2D
    /// <summary>Retangulo com as 4 quinas cortadas (8 pontos), em sentido anti-horario.</summary>
    static Vector2[] RetanguloChanfrado(float largura, float altura, float chanfro)
    {
        float x = largura * 0.5f;
        float y = altura * 0.5f;
        float c = Mathf.Min(chanfro, Mathf.Min(x, y) * 0.9f);

        return new[]
        {
            new Vector2( x - c, -y),
            new Vector2( x,     -y + c),
            new Vector2( x,      y - c),
            new Vector2( x - c,  y),
            new Vector2(-x + c,  y),
            new Vector2(-x,      y - c),
            new Vector2(-x,     -y + c),
            new Vector2(-x + c, -y),
        };
    }

    // --------------------------------------------------------- malha/material
    static Mesh SalvarMalha(Mesh nova, string caminho)
    {
        Mesh existente = AssetDatabase.LoadAssetAtPath<Mesh>(caminho);
        if (existente == null)
        {
            AssetDatabase.CreateAsset(nova, caminho);
            return nova;
        }

        // reaproveita o asset (mantem o GUID e as referencias do prefab)
        existente.Clear();
        existente.name = nova.name;
        existente.vertices = nova.vertices;
        existente.uv = nova.uv;
        existente.triangles = nova.triangles;
        existente.RecalculateNormals();
        existente.RecalculateBounds();
        EditorUtility.SetDirty(existente);
        Object.DestroyImmediate(nova);
        return existente;
    }

    static Material MaterialOpaco(string caminho, Color cor, float smoothness, float metallic)
    {
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(caminho);
        if (mat == null)
        {
            mat = new Material(EncontrarShader());
            AssetDatabase.CreateAsset(mat, caminho);
        }

        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", cor);
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", cor);
        if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
        if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", smoothness);
        if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
        EditorUtility.SetDirty(mat);
        return mat;
    }

    static Shader EncontrarShader()
    {
        Shader s = Shader.Find("Universal Render Pipeline/Lit");
        if (s == null) s = Shader.Find("HDRP/Lit");
        if (s == null) s = Shader.Find("Standard");
        return s;
    }

    static void GarantirPasta()
    {
        MeusOrganizador.GarantirPasta(Pasta);
    }

    // ------------------------------------------------------------- construtor
    /// <summary>Acumula prismas (secao 2D extrudada) numa malha so, com faces planas.</summary>
    class Construtor
    {
        readonly List<Vector3> _v = new List<Vector3>();
        readonly List<Vector2> _uv = new List<Vector2>();
        readonly List<int> _t = new List<int>();

        public void Prisma(Vector2[] secao, float comprimento, Matrix4x4 m)
        {
            int n = secao.Length;
            float z0 = -comprimento * 0.5f;
            float z1 = comprimento * 0.5f;

            // laterais
            float u = 0f;
            for (int i = 0; i < n; i++)
            {
                Vector2 a = secao[i];
                Vector2 b = secao[(i + 1) % n];
                float d = Vector2.Distance(a, b);

                Quad(
                    m.MultiplyPoint3x4(new Vector3(a.x, a.y, z0)),
                    m.MultiplyPoint3x4(new Vector3(b.x, b.y, z0)),
                    m.MultiplyPoint3x4(new Vector3(b.x, b.y, z1)),
                    m.MultiplyPoint3x4(new Vector3(a.x, a.y, z1)),
                    new Vector2(u, 0f), new Vector2(u + d, 0f),
                    new Vector2(u + d, comprimento), new Vector2(u, comprimento));

                u += d;
            }

            Tampa(secao, z1, m, false); // frente (+Z local)
            Tampa(secao, z0, m, true);  // tras  (-Z local)
        }

        void Tampa(Vector2[] secao, float z, Matrix4x4 m, bool inverter)
        {
            int n = secao.Length;
            for (int i = 1; i < n - 1; i++)
            {
                Vector3 p0 = m.MultiplyPoint3x4(new Vector3(secao[0].x, secao[0].y, z));
                Vector3 p1 = m.MultiplyPoint3x4(new Vector3(secao[i].x, secao[i].y, z));
                Vector3 p2 = m.MultiplyPoint3x4(new Vector3(secao[i + 1].x, secao[i + 1].y, z));

                if (inverter) Tri(p0, p2, p1, secao[0], secao[i + 1], secao[i]);
                else Tri(p0, p1, p2, secao[0], secao[i], secao[i + 1]);
            }
        }

        void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d,
                  Vector2 ua, Vector2 ub, Vector2 uc, Vector2 ud)
        {
            Tri(a, b, c, ua, ub, uc);
            Tri(a, c, d, ua, uc, ud);
        }

        void Tri(Vector3 a, Vector3 b, Vector3 c, Vector2 ua, Vector2 ub, Vector2 uc)
        {
            int i = _v.Count;
            _v.Add(a); _v.Add(b); _v.Add(c);
            _uv.Add(ua); _uv.Add(ub); _uv.Add(uc);
            _t.Add(i); _t.Add(i + 1); _t.Add(i + 2);
        }

        public Mesh Construir(string nome)
        {
            Mesh malha = new Mesh { name = nome };
            malha.SetVertices(_v);
            malha.SetUVs(0, _uv);
            malha.SetTriangles(_t, 0);
            malha.RecalculateNormals(); // faces planas: nenhum vertice e compartilhado
            malha.RecalculateBounds();
            return malha;
        }
    }
}
