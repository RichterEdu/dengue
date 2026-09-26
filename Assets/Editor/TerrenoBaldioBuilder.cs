using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Popula o Terrain "terreno baldio" com cenario de lote abandonado:
/// entulho, tijolos, sacos de lixo, caixotes, areia, tabuas, pilar quebrado,
/// colchao, pneu velho, garrafas e latas (tudo gerado por codigo, no mesmo
/// acabamento low poly do vaso / caixa d'agua / escada), mais mato, arbustos,
/// pedras, galhos e arvores raquiticas reaproveitados dos pacotes do projeto.
///
/// Menu: Dengue > Popular Terreno Baldio  (abre a janela com as opcoes)
///
/// Os assets ficam em Assets/Meus/Terreno Baldio/ (malhas, materiais, prefabs) e
/// tudo e instanciado na cena dentro de "Cenario Terreno Baldio". NENHUM objeto
/// recebe script de interacao -- e so cenario; os focos sao colocados a mao.
///
/// Pode ser rodado de novo: reaproveita os assets (mantem os GUIDs) e refaz o
/// espalhamento do zero. Nenhum .meta e escrito a mao -- o Unity gera todos.
/// </summary>
public class TerrenoBaldioBuilder : EditorWindow
{
    public const string RaizCenario = "Cenario Terreno Baldio";

    // ----------------------------------------------------------- preferencias
    string _nomeTerreno = "terreno baldio";
    int _semente = 7;
    float _densidade = 1.0f;
    float _margem = 2.0f;
    float _inclinacaoMax = 32f;
    float _raioPlayer = 3.0f;
    float _raioFocos = 1.5f;
    bool _usarGerados = true;
    bool _usarPacotes = true;
    bool _pintarMato = true;
    bool _respeitarCena = true;
    bool _consertarMateriais = true;
    Vector2 _scroll;
    string _relatorio = "";

    [MenuItem("Dengue/Popular Terreno Baldio")]
    public static void Abrir()
    {
        TerrenoBaldioBuilder j = GetWindow<TerrenoBaldioBuilder>(true, "Terreno Baldio", true);
        j.minSize = new Vector2(440f, 520f);
        j.Show();
    }

    void OnGUI()
    {
        _scroll = EditorGUILayout.BeginScrollView(_scroll);

        EditorGUILayout.LabelField("Terreno", EditorStyles.boldLabel);
        _nomeTerreno = EditorGUILayout.TextField(
            new GUIContent("Nome do Terrain", "Acha o Terrain pelo nome (ignora acento e maiuscula). " +
                                              "Se nao achar, usa o que estiver selecionado na Hierarchy."),
            _nomeTerreno);

        Terrain t = Achar(_nomeTerreno);
        if (t == null)
        {
            EditorGUILayout.HelpBox("Nao achei nenhum Terrain com esse nome na cena aberta.\n" +
                                    "Selecione o terreno na Hierarchy ou corrija o nome acima.",
                                    MessageType.Warning);
        }
        else
        {
            Vector3 s = t.terrainData.size;
            float area = Mathf.Max(0f, (s.x - 2f * _margem) * (s.z - 2f * _margem));
            EditorGUILayout.HelpBox(string.Format(
                "Terrain: \"{0}\"\nTamanho: {1:0.0} x {2:0.0} m   |   area util (ja com a margem): {3:0} m²",
                t.name, s.x, s.z, area), MessageType.Info);
        }

        EditorGUILayout.Space(6f);
        EditorGUILayout.LabelField("Espalhamento", EditorStyles.boldLabel);
        _semente = EditorGUILayout.IntField(new GUIContent("Semente",
            "Mesma semente = mesmo layout. Troque o numero para sortear outro."), _semente);
        _densidade = EditorGUILayout.Slider(new GUIContent("Densidade",
            "1 = medio (abandonado, mas caminhavel)."), _densidade, 0.3f, 2.5f);
        _margem = EditorGUILayout.Slider(new GUIContent("Margem das bordas (m)",
            "Faixa livre na borda do terreno, para nao encostar na cerca."), _margem, 0f, 10f);
        _inclinacaoMax = EditorGUILayout.Slider(new GUIContent("Inclinacao maxima (graus)",
            "Nao coloca nada em barranco mais ingreme que isso."), _inclinacaoMax, 5f, 60f);

        EditorGUILayout.Space(6f);
        EditorGUILayout.LabelField("Areas livres", EditorStyles.boldLabel);
        _raioPlayer = EditorGUILayout.Slider(new GUIContent("Raio livre no player (m)"), _raioPlayer, 0f, 10f);
        _raioFocos = EditorGUILayout.Slider(new GUIContent("Raio livre nos focos (m)",
            "Nao enterra os criadouros que voce ja colocou."), _raioFocos, 0f, 6f);
        _respeitarCena = EditorGUILayout.Toggle(new GUIContent("Desviar do que ja existe",
            "Usa os colisores da cena (cerca, casas, focos) para nao colocar nada por cima."),
            _respeitarCena);

        EditorGUILayout.Space(6f);
        EditorGUILayout.LabelField("O que colocar", EditorStyles.boldLabel);
        _usarGerados = EditorGUILayout.Toggle(new GUIContent("Pecas geradas por codigo",
            "Entulho, lixo, sucata, mato e arbustos -- com material URP proprio."), _usarGerados);
        _usarPacotes = EditorGUILayout.Toggle(new GUIContent("Extras dos pacotes",
            "Pedras, galhos, toco, arvores e o monte de lixo da cidade."), _usarPacotes);
        _pintarMato = EditorGUILayout.Toggle(new GUIContent("Pintar mato no terreno",
            "Usa o sistema de Details do Terrain: mato denso em manchas, sem custo de GameObject."),
            _pintarMato);
        _consertarMateriais = EditorGUILayout.Toggle(new GUIContent("Consertar materiais (Built-in -> URP)",
            "Os pacotes da Asset Store vem com shader do Built-in: no URP ficam rosa ou brancos. " +
            "Marcado, converte antes de popular."), _consertarMateriais);

        EditorGUILayout.Space(10f);
        using (new EditorGUI.DisabledScope(t == null))
        {
            if (GUILayout.Button("Popular terreno baldio", GUILayout.Height(34f))) Popular(t);
        }
        EditorGUILayout.Space(2f);
        if (GUILayout.Button("Limpar (tirar tudo que este builder colocou)", GUILayout.Height(24f))) Limpar(t);
        EditorGUILayout.Space(2f);
        if (GUILayout.Button("So (re)gerar os assets, sem mexer na cena", GUILayout.Height(20f)))
        {
            int n = TerrenoBaldioAssets.GerarTudo();
            _relatorio = n + " prefabs em " + TerrenoBaldioAssets.Pasta;
        }

        if (!string.IsNullOrEmpty(_relatorio))
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Resultado", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(_relatorio, MessageType.None);
        }

        EditorGUILayout.EndScrollView();
    }

    // ------------------------------------------------------------ achar terrain
    static string Simples(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        string baixo = s.ToLowerInvariant();
        System.Text.StringBuilder sb = new System.Text.StringBuilder(baixo.Length);
        for (int i = 0; i < baixo.Length; i++)
        {
            char d = baixo[i];
            if ("áàâãä".IndexOf(d) >= 0) d = 'a';
            else if ("éèêë".IndexOf(d) >= 0) d = 'e';
            else if ("íìîï".IndexOf(d) >= 0) d = 'i';
            else if ("óòôõö".IndexOf(d) >= 0) d = 'o';
            else if ("úùûü".IndexOf(d) >= 0) d = 'u';
            else if (d == 'ç') d = 'c';
            if (d == ' ' || d == '_' || d == '-') continue;
            sb.Append(d);
        }
        return sb.ToString();
    }

    static Terrain Achar(string nome)
    {
        Terrain[] todos = Object.FindObjectsByType<Terrain>(FindObjectsSortMode.None);
        if (todos.Length == 0) return null;

        string alvo = Simples(nome);
        if (alvo.Length > 0)
        {
            for (int i = 0; i < todos.Length; i++)
                if (Simples(todos[i].name) == alvo) return todos[i];
            for (int i = 0; i < todos.Length; i++)
                if (Simples(todos[i].name).Contains(alvo)) return todos[i];
        }

        if (Selection.activeGameObject != null)
        {
            Terrain sel = Selection.activeGameObject.GetComponent<Terrain>();
            if (sel != null) return sel;
        }
        return todos.Length == 1 ? todos[0] : null;
    }

    // ---------------------------------------------------------------- popular
    void Popular(Terrain terreno)
    {
        if (terreno == null) return;

        int materiais = _consertarMateriais ? TerrenoBaldioMateriais.Converter(null, true) : 0;
        TerrenoBaldioAssets.GerarTudo();

        UnityEngine.SceneManagement.Scene cena = terreno.gameObject.scene;

        // raiz: refaz do zero, no mesmo grupo da Hierarchy em que estava
        List<GameObject> antigos = AcharCenarios(cena);
        Transform paiAntigo = antigos.Count > 0 ? antigos[0].transform.parent : null;
        int indiceAntigo = antigos.Count > 0 ? antigos[0].transform.GetSiblingIndex() : 0;
        for (int i = 0; i < antigos.Count; i++)
            Undo.DestroyObjectImmediate(antigos[i]);

        GameObject raiz = new GameObject(RaizCenario);
        raiz.transform.position = terreno.transform.position;
        Undo.RegisterCreatedObjectUndo(raiz, "Popular Terreno Baldio");
        HierarquiaOrganizador.RecolocarNoLugar(raiz, paiAntigo, indiceAntigo);

        Vector3 tamanho = terreno.terrainData.size;
        float margem = Mathf.Min(_margem, Mathf.Min(tamanho.x, tamanho.z) * 0.4f);
        float area = Mathf.Max(1f, (tamanho.x - 2f * margem) * (tamanho.z - 2f * margem));

        // zonas livres (em coordenadas locais do terreno): x, z, raio
        List<Vector3> livres = new List<Vector3>();
        CharacterController player = Object.FindFirstObjectByType<CharacterController>();
        if (player != null && _raioPlayer > 0f)
        {
            Vector3 p = player.transform.position - terreno.transform.position;
            livres.Add(new Vector3(p.x, p.z, _raioPlayer));
        }
        if (_raioFocos > 0f)
        {
            Interactable[] focos = Object.FindObjectsByType<Interactable>(FindObjectsSortMode.None);
            for (int i = 0; i < focos.Length; i++)
            {
                Vector3 p = focos[i].transform.position - terreno.transform.position;
                livres.Add(new Vector3(p.x, p.z, _raioFocos));
            }
        }

        if (_respeitarCena) Physics.SyncTransforms();

        System.Random rng = new System.Random(_semente);
        Dictionary<long, List<Vector4>> grade = new Dictionary<long, List<Vector4>>();
        Dictionary<string, Transform> grupos = new Dictionary<string, Transform>();
        List<string> resumo = new List<string>();
        int total = 0;
        float cosMax = Mathf.Cos(_inclinacaoMax * Mathf.Deg2Rad);

        Categoria[] catalogo = Categoria.Catalogo();
        for (int ci = 0; ci < catalogo.Length; ci++)
        {
            Categoria c = catalogo[ci];
            if (c.gerado && !_usarGerados) continue;
            if (!c.gerado && !_usarPacotes) continue;

            GameObject[] opcoes = c.Carregar();
            if (opcoes.Length == 0) continue;

            int alvo = Mathf.RoundToInt(c.por100 * _densidade * area / 100f);
            if (alvo <= 0) continue;

            int postos = 0;
            int tentativas = alvo * 24 + 40;
            for (int k = 0; k < tentativas && postos < alvo && total < 2500; k++)
            {
                float lx = margem + (float)rng.NextDouble() * (tamanho.x - 2f * margem);
                float lz = margem + (float)rng.NextDouble() * (tamanho.z - 2f * margem);

                if (!Cabe(grade, lx, lz, c.espaco, ci)) continue;

                bool naZonaLivre = false;
                for (int i = 0; i < livres.Count; i++)
                {
                    Vector3 l = livres[i];
                    if (new Vector2(l.x - lx, l.y - lz).sqrMagnitude < l.z * l.z) { naZonaLivre = true; break; }
                }
                if (naZonaLivre) continue;

                Vector3 mundo = terreno.transform.position + new Vector3(lx, 0f, lz);
                mundo.y = terreno.SampleHeight(mundo) + terreno.transform.position.y;

                Vector3 normal = terreno.terrainData.GetInterpolatedNormal(lx / tamanho.x, lz / tamanho.z);
                if (normal.y < cosMax) continue;

                float escala = Mathf.Lerp(c.escalaMin, c.escalaMax, (float)rng.NextDouble());
                if (_respeitarCena && Bloqueado(mundo, Mathf.Max(0.35f, c.espaco * 0.45f) * escala)) continue;

                GameObject prefab = opcoes[rng.Next(opcoes.Length)];
                GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                if (go == null) continue;

                // respeita a pose original do prefab (alguns vem com rotacao/escala de import)
                Quaternion giro = Quaternion.Euler(0f, (float)rng.NextDouble() * 360f, 0f) *
                                  prefab.transform.rotation;
                if (c.alinhaNormal > 0f)
                    giro = Quaternion.Slerp(Quaternion.identity,
                                            Quaternion.FromToRotation(Vector3.up, normal),
                                            c.alinhaNormal) * giro;

                go.transform.SetPositionAndRotation(mundo - Vector3.up * (0.02f * escala), giro);
                go.transform.localScale = prefab.transform.localScale * escala;

                AjustarColisores(go, c);

                go.transform.SetParent(Grupo(grupos, raiz.transform, c.grupo), true);
                go.name = c.nome + " " + (postos + 1).ToString("000");
                GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.BatchingStatic |
                                                           StaticEditorFlags.OccludeeStatic);

                Anotar(grade, lx, lz, c.espaco, ci);
                postos++;
                total++;
            }

            if (postos > 0) resumo.Add("  " + c.nome + ": " + postos);
        }

        string mato = _pintarMato ? TerrenoBaldioMato.Pintar(terreno, _semente, _densidade, margem) : "";

        EditorSceneManager.MarkSceneDirty(cena);
        Selection.activeGameObject = raiz;

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine(total + " objetos em \"" + RaizCenario + "\" (semente " + _semente +
                      ", densidade " + _densidade.ToString("0.00") + ").");
        for (int i = 0; i < resumo.Count; i++) sb.AppendLine(resumo[i]);
        if (mato.Length > 0) sb.AppendLine(mato);
        if (materiais > 0) sb.AppendLine(materiais + " materiais convertidos de Built-in para URP.");
        sb.Append("Salve a cena (Ctrl+S).");
        _relatorio = sb.ToString();
        Debug.Log("[Terreno Baldio] " + _relatorio);
    }

    /// <summary>Plantas de pacote costumam vir com colisor da caixa inteira -- atrapalha andar e mirar.</summary>
    static void AjustarColisores(GameObject go, Categoria c)
    {
        if (!c.tirarColisor) return;

        // desliga em vez de apagar: mexer em componente de prefab instanciado e
        // mais arriscado, e colisor desligado nao atrapalha nem o andar nem a mira
        Collider[] cols = go.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < cols.Length; i++) cols[i].enabled = false;

        if (c.raioTronco <= 0f) return;

        Renderer[] rs = go.GetComponentsInChildren<Renderer>(true);
        float altura = 1f;
        for (int i = 0; i < rs.Length; i++) altura = Mathf.Max(altura, rs[i].bounds.size.y);
        altura = Mathf.Min(altura, c.alturaTronco > 0f ? c.alturaTronco : altura);

        CapsuleCollider cap = go.AddComponent<CapsuleCollider>();
        cap.radius = c.raioTronco;
        cap.height = Mathf.Max(altura, c.raioTronco * 2f);
        cap.center = new Vector3(0f, cap.height * 0.5f, 0f);
    }

    static Transform Grupo(Dictionary<string, Transform> cache, Transform raiz, string nome)
    {
        Transform t;
        if (cache.TryGetValue(nome, out t) && t != null) return t;
        GameObject go = new GameObject(nome);
        go.transform.SetParent(raiz, false);
        cache[nome] = go.transform;
        return go.transform;
    }

    static bool Bloqueado(Vector3 mundo, float raio)
    {
        Collider[] achados = Physics.OverlapSphere(mundo + Vector3.up * 0.25f, raio);
        for (int i = 0; i < achados.Length; i++)
        {
            if (achados[i] is TerrainCollider) continue;
            return true;
        }
        return false;
    }

    // --------------------------------------------------- grade de espacamento
    const float Celula = 1.0f;

    static long Chave(int i, int j) { return ((long)i << 32) ^ (uint)j; }

    static bool Cabe(Dictionary<long, List<Vector4>> grade, float x, float z, float raio, int cat)
    {
        int alcance = Mathf.CeilToInt((raio + 1f) / Celula);
        int i0 = Mathf.FloorToInt(x / Celula);
        int j0 = Mathf.FloorToInt(z / Celula);
        for (int i = i0 - alcance; i <= i0 + alcance; i++)
        {
            for (int j = j0 - alcance; j <= j0 + alcance; j++)
            {
                List<Vector4> lista;
                if (!grade.TryGetValue(Chave(i, j), out lista)) continue;
                for (int k = 0; k < lista.Count; k++)
                {
                    Vector4 p = lista[k];
                    float d = new Vector2(p.x - x, p.y - z).magnitude;
                    float minimo = ((int)p.w == cat)
                        ? Mathf.Max(raio, p.z)
                        : Mathf.Max(0.30f, Mathf.Min(raio, p.z) * 0.45f);
                    if (d < minimo) return false;
                }
            }
        }
        return true;
    }

    static void Anotar(Dictionary<long, List<Vector4>> grade, float x, float z, float raio, int cat)
    {
        long c = Chave(Mathf.FloorToInt(x / Celula), Mathf.FloorToInt(z / Celula));
        List<Vector4> lista;
        if (!grade.TryGetValue(c, out lista)) { lista = new List<Vector4>(); grade[c] = lista; }
        lista.Add(new Vector4(x, z, raio, cat));
    }

    // ----------------------------------------------------------------- limpar
    void Limpar(Terrain terreno)
    {
        UnityEngine.SceneManagement.Scene cena = terreno != null
            ? terreno.gameObject.scene
            : UnityEngine.SceneManagement.SceneManager.GetActiveScene();

        int n = 0;
        List<GameObject> cenarios = AcharCenarios(cena);
        for (int i = 0; i < cenarios.Count; i++)
        {
            n += cenarios[i].GetComponentsInChildren<Transform>(true).Length;
            Undo.DestroyObjectImmediate(cenarios[i]);
        }

        string mato = terreno != null ? TerrenoBaldioMato.Apagar(terreno) : "";
        EditorSceneManager.MarkSceneDirty(cena);
        _relatorio = "Cenario removido (" + n + " transforms). " + mato;
        Debug.Log("[Terreno Baldio] " + _relatorio);
    }

    // acha "Cenario Terreno Baldio" em qualquer nivel da Hierarchy (pode estar dentro
    // de um grupo do Dengue > Organizar Hierarquia), nao so na raiz
    static List<GameObject> AcharCenarios(UnityEngine.SceneManagement.Scene cena)
    {
        List<GameObject> achados = new List<GameObject>();
        GameObject[] raizes = cena.GetRootGameObjects();
        for (int i = 0; i < raizes.Length; i++)
        {
            Transform[] todos = raizes[i].GetComponentsInChildren<Transform>(true);
            for (int j = 0; j < todos.Length; j++)
                if (todos[j].name == RaizCenario) achados.Add(todos[j].gameObject);
        }
        return achados;
    }

    // -------------------------------------------------------------- catalogo
    class Categoria
    {
        public string nome;
        public string grupo;
        public string[] caminhos;
        public bool gerado;
        public float por100;
        public float espaco;
        public float escalaMin = 0.9f;
        public float escalaMax = 1.1f;
        public float alinhaNormal = 0.65f;   // 0 = sempre em pe, 1 = deita junto com o barranco
        public bool tirarColisor;            // plantas de pacote vem com colisor de caixa
        public float raioTronco;             // > 0: poe capsula no tronco no lugar do colisor
        public float alturaTronco;

        public GameObject[] Carregar()
        {
            List<GameObject> achados = new List<GameObject>();
            for (int i = 0; i < caminhos.Length; i++)
            {
                GameObject g = AssetDatabase.LoadAssetAtPath<GameObject>(caminhos[i]);
                if (g != null) achados.Add(g);
            }
            return achados.ToArray();
        }

        static string[] Ger(params string[] nomes)
        {
            string[] r = new string[nomes.Length];
            for (int i = 0; i < nomes.Length; i++)
                r[i] = TerrenoBaldioAssets.Prefabs + "/" + nomes[i] + ".prefab";
            return r;
        }

        static string[] Mais(string[] a, params string[] b)
        {
            List<string> r = new List<string>(a);
            r.AddRange(b);
            return r.ToArray();
        }

        const string SNP = "Assets/SimpleNaturePack/Prefabs/";
        const string CIDADE = "Assets/CartoonLowPolyCityLite/Prefabs/";

        /// <summary>Grandes primeiro (definem o layout), miudos depois, mato por ultimo.</summary>
        public static Categoria[] Catalogo()
        {
            return new Categoria[]
            {
                new Categoria { nome = "Monte de Entulho", grupo = "Entulho", gerado = true,
                    caminhos = Ger("Monte de Entulho 1", "Monte de Entulho 2", "Monte de Entulho 3"),
                    por100 = 0.80f, espaco = 3.5f, escalaMin = 0.80f, escalaMax = 1.35f, alinhaNormal = 0.85f },

                new Categoria { nome = "Monte de Areia", grupo = "Entulho", gerado = true,
                    caminhos = Ger("Monte de Areia 1", "Monte de Areia 2"),
                    por100 = 0.35f, espaco = 4.0f, escalaMin = 0.80f, escalaMax = 1.30f, alinhaNormal = 0.85f },

                new Categoria { nome = "Pilar Quebrado", grupo = "Entulho", gerado = true,
                    caminhos = Ger("Pilar Quebrado 1", "Pilar Quebrado 2"),
                    por100 = 0.30f, espaco = 4.0f, escalaMin = 0.90f, escalaMax = 1.20f, alinhaNormal = 0.25f },

                new Categoria { nome = "Colchao Velho", grupo = "Sucata", gerado = true,
                    caminhos = Ger("Colchao Velho"),
                    por100 = 0.20f, espaco = 5.0f, escalaMin = 0.95f, escalaMax = 1.05f, alinhaNormal = 0.90f },

                new Categoria { nome = "Arvore Rala", grupo = "Arvores", gerado = false,
                    caminhos = new string[] { SNP + "Tree_01.prefab", SNP + "Tree_03.prefab", SNP + "Tree_05.prefab" },
                    por100 = 0.35f, espaco = 5.0f, escalaMin = 0.80f, escalaMax = 1.25f, alinhaNormal = 0.15f,
                    tirarColisor = true, raioTronco = 0.22f, alturaTronco = 2.5f },

                new Categoria { nome = "Pilha de Tabuas", grupo = "Sucata", gerado = true,
                    caminhos = Ger("Pilha de Tabuas 1", "Pilha de Tabuas 2"),
                    por100 = 0.40f, espaco = 3.5f, escalaMin = 0.90f, escalaMax = 1.15f, alinhaNormal = 0.80f },

                new Categoria { nome = "Pilha de Tijolos", grupo = "Entulho", gerado = true,
                    caminhos = Ger("Pilha de Tijolos 1", "Pilha de Tijolos 2"),
                    por100 = 0.50f, espaco = 3.0f, escalaMin = 0.90f, escalaMax = 1.15f, alinhaNormal = 0.80f },

                new Categoria { nome = "Toco", grupo = "Arvores", gerado = false,
                    caminhos = new string[] { SNP + "Stump_01.prefab" },
                    por100 = 0.50f, espaco = 3.0f, escalaMin = 0.90f, escalaMax = 1.20f, alinhaNormal = 0.50f,
                    tirarColisor = true, raioTronco = 0.28f, alturaTronco = 0.45f },

                new Categoria { nome = "Caixote", grupo = "Lixo", gerado = true,
                    caminhos = Ger("Caixote 1", "Caixote 2"),
                    por100 = 0.80f, espaco = 2.0f, escalaMin = 0.80f, escalaMax = 1.20f, alinhaNormal = 0.70f },

                new Categoria { nome = "Laje Partida", grupo = "Entulho", gerado = true,
                    caminhos = Ger("Laje Partida 1", "Laje Partida 2"),
                    por100 = 1.20f, espaco = 2.0f, escalaMin = 0.80f, escalaMax = 1.30f, alinhaNormal = 0.90f },

                new Categoria { nome = "Pneu Velho", grupo = "Sucata", gerado = true,
                    caminhos = Ger("Pneu Velho"),
                    por100 = 0.60f, espaco = 2.0f, escalaMin = 0.90f, escalaMax = 1.15f, alinhaNormal = 0.90f },

                new Categoria { nome = "Monte de Lixo", grupo = "Lixo", gerado = false,
                    caminhos = new string[] { CIDADE + "Trash_01.prefab" },
                    por100 = 0.60f, espaco = 2.5f, escalaMin = 0.80f, escalaMax = 1.20f, alinhaNormal = 0.70f },

                new Categoria { nome = "Arbusto", grupo = "Mato e Pedras", gerado = true,
                    caminhos = Mais(Ger("Arbusto 1", "Arbusto 2", "Arbusto 3"),
                                    SNP + "Bush_01.prefab", SNP + "Bush_02.prefab", SNP + "Bush_03.prefab"),
                    por100 = 3.50f, espaco = 1.8f, escalaMin = 0.80f, escalaMax = 1.45f, alinhaNormal = 0.30f,
                    tirarColisor = true },

                new Categoria { nome = "Saco de Lixo", grupo = "Lixo", gerado = true,
                    caminhos = Ger("Saco de Lixo 1", "Saco de Lixo 2"),
                    por100 = 1.80f, espaco = 1.2f, escalaMin = 0.85f, escalaMax = 1.20f, alinhaNormal = 0.80f },

                new Categoria { nome = "Pedra", grupo = "Mato e Pedras", gerado = false,
                    caminhos = new string[] { SNP + "Rock_01.prefab", SNP + "Rock_02.prefab",
                                              SNP + "Rock_03.prefab", SNP + "Rock_04.prefab",
                                              SNP + "Rock_05.prefab" },
                    por100 = 3.00f, espaco = 1.2f, escalaMin = 0.60f, escalaMax = 1.40f, alinhaNormal = 0.80f,
                    tirarColisor = true },

                new Categoria { nome = "Tabua Solta", grupo = "Sucata", gerado = true,
                    caminhos = Ger("Tabua Solta 1", "Tabua Solta 2"),
                    por100 = 1.50f, espaco = 1.2f, escalaMin = 0.85f, escalaMax = 1.20f, alinhaNormal = 0.90f },

                new Categoria { nome = "Galho", grupo = "Mato e Pedras", gerado = false,
                    caminhos = new string[] { SNP + "Branch_01.prefab" },
                    por100 = 2.50f, espaco = 0.9f, escalaMin = 0.80f, escalaMax = 1.30f, alinhaNormal = 0.90f,
                    tirarColisor = true },

                new Categoria { nome = "Pedaco de Concreto", grupo = "Entulho", gerado = true,
                    caminhos = Ger("Pedaco de Concreto 1", "Pedaco de Concreto 2", "Pedaco de Concreto 3"),
                    por100 = 3.50f, espaco = 0.8f, escalaMin = 0.70f, escalaMax = 1.50f, alinhaNormal = 0.90f },

                new Categoria { nome = "Tijolo", grupo = "Entulho", gerado = true,
                    caminhos = Ger("Tijolo"),
                    por100 = 4.00f, espaco = 0.5f, escalaMin = 0.90f, escalaMax = 1.10f, alinhaNormal = 0.95f },

                new Categoria { nome = "Garrafa", grupo = "Lixo", gerado = true,
                    caminhos = Ger("Garrafa"),
                    por100 = 3.00f, espaco = 0.5f, escalaMin = 0.90f, escalaMax = 1.10f, alinhaNormal = 0.90f },

                new Categoria { nome = "Lata", grupo = "Lixo", gerado = true,
                    caminhos = Ger("Lata"),
                    por100 = 3.00f, espaco = 0.5f, escalaMin = 0.90f, escalaMax = 1.10f, alinhaNormal = 0.90f },

                new Categoria { nome = "Tufo Seco", grupo = "Mato e Pedras", gerado = true,
                    caminhos = Mais(Ger("Tufo Seco 1", "Tufo Seco 2"), SNP + "Grass_02.prefab"),
                    por100 = 6.00f, espaco = 1.1f, escalaMin = 0.80f, escalaMax = 1.30f, alinhaNormal = 0.20f,
                    tirarColisor = true },

                new Categoria { nome = "Tufo de Mato", grupo = "Mato e Pedras", gerado = true,
                    caminhos = Mais(Ger("Tufo de Mato 1", "Tufo de Mato 2", "Tufo Alto"),
                                    SNP + "Grass_01.prefab", SNP + "Flowers_01.prefab"),
                    por100 = 10.00f, espaco = 1.0f, escalaMin = 0.80f, escalaMax = 1.35f, alinhaNormal = 0.20f,
                    tirarColisor = true },
            };
        }
    }
}
