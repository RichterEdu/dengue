using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Gera as pecas de cenario do terreno baldio (malhas, materiais e prefabs).
/// Chamado pelo TerrenoBaldioBuilder -- nao precisa rodar na mao.
///
/// Tudo vai para Assets/Meus/Assets Gerados/Terreno Baldio/:
///   Malhas/*.asset      uma malha por peca (com submeshes, um por material)
///   Materiais/*.mat     URP Lit (com fallback HDRP/Standard)
///   Prefabs/*.prefab    27 prefabs de cenario, SEM script de interacao
///
/// Mesma tecnica dos builders anteriores: prismas de secao chanfrada e malhas de
/// revolucao (torno), faces planas (nenhum vertice compartilhado), acabamento low
/// poly. Roda de novo sem problema: reaproveita os assets e mantem os GUIDs.
/// </summary>
public static class TerrenoBaldioAssets
{
    public const string PastaMeus = "Assets/Meus/Assets Gerados";
    public const string Pasta = "Assets/Meus/Assets Gerados/Terreno Baldio";
    public const string Malhas = Pasta + "/Malhas";
    public const string Materiais = Pasta + "/Materiais";
    public const string Prefabs = Pasta + "/Prefabs";

    // ------------------------------------------------------------- materiais
    static readonly Dictionary<string, Color> Cores = new Dictionary<string, Color>
    {
        { "tijolo",   new Color(0.600f, 0.270f, 0.190f) },
        { "concreto", new Color(0.630f, 0.610f, 0.570f) },
        { "terra",    new Color(0.340f, 0.260f, 0.180f) },
        { "areia",    new Color(0.760f, 0.660f, 0.460f) },
        { "plastico", new Color(0.085f, 0.085f, 0.095f) },
        { "papelao",  new Color(0.620f, 0.450f, 0.280f) },
        { "madeira",  new Color(0.440f, 0.350f, 0.250f) },
        { "ferrugem", new Color(0.450f, 0.260f, 0.150f) },
        { "tecido",   new Color(0.710f, 0.690f, 0.610f) },
        { "vidro",    new Color(0.420f, 0.550f, 0.400f) },
        { "metal",    new Color(0.600f, 0.620f, 0.640f) },
        { "borracha", new Color(0.100f, 0.100f, 0.110f) },
        { "folha",    new Color(0.285f, 0.400f, 0.175f) },
        { "folha seca", new Color(0.560f, 0.495f, 0.275f) },
    };

    static readonly Dictionary<string, float> Brilho = new Dictionary<string, float>
    {
        { "tijolo", 0.06f }, { "concreto", 0.08f }, { "terra", 0.04f }, { "areia", 0.05f },
        { "plastico", 0.38f }, { "papelao", 0.05f }, { "madeira", 0.08f }, { "ferrugem", 0.10f },
        { "tecido", 0.06f }, { "vidro", 0.62f }, { "metal", 0.48f }, { "borracha", 0.14f },
        { "folha", 0.10f }, { "folha seca", 0.07f },
    };

    // ------------------------------------------------------------------ medidas
    const float TijC = 0.19f;   // comprimento do tijolo
    const float TijL = 0.09f;   // largura
    const float TijA = 0.055f;  // altura

    // ------------------------------------------------------------------ gerar
    public static int GerarTudo()
    {
        GarantirPastas();
        foreach (KeyValuePair<string, Color> par in Cores) MaterialDe(par.Key);

        int n = 0;
        n += Criar("Monte de Entulho 1", 11, true, (m, r) => MonteEntulho(m, r, 0.72f, 0.34f));
        n += Criar("Monte de Entulho 2", 12, true, (m, r) => MonteEntulho(m, r, 0.86f, 0.40f));
        n += Criar("Monte de Entulho 3", 13, true, (m, r) => MonteEntulho(m, r, 0.62f, 0.28f));

        n += Criar("Monte de Areia 1", 21, true, (m, r) => MonteAreia(m, r, 0.80f, 0.42f));
        n += Criar("Monte de Areia 2", 22, true, (m, r) => MonteAreia(m, r, 1.05f, 0.52f));

        n += Criar("Pilar Quebrado 1", 31, true, (m, r) => PilarQuebrado(m, r, 0.72f));
        n += Criar("Pilar Quebrado 2", 32, true, (m, r) => PilarQuebrado(m, r, 0.55f));

        n += Criar("Colchao Velho", 41, true, Colchao);

        n += Criar("Pilha de Tabuas 1", 51, true, PilhaTabuas);
        n += Criar("Pilha de Tabuas 2", 52, true, PilhaTabuas);

        n += Criar("Pilha de Tijolos 1", 61, true, PilhaTijolos);
        n += Criar("Pilha de Tijolos 2", 62, true, PilhaTijolos);

        n += Criar("Caixote 1", 71, true, Caixote);
        n += Criar("Caixote 2", 72, true, Caixote);

        n += Criar("Laje Partida 1", 81, false, (m, r) => LajePartida(m, r, 0.55f));
        n += Criar("Laje Partida 2", 82, false, (m, r) => LajePartida(m, r, 0.38f));

        n += Criar("Pneu Velho", 91, false, Pneu);

        n += Criar("Saco de Lixo 1", 101, true, (m, r) => SacoLixo(m, r, 1.0f));
        n += Criar("Saco de Lixo 2", 102, true, (m, r) => SacoLixo(m, r, 0.78f));

        n += Criar("Tabua Solta 1", 111, false, TabuaSolta);
        n += Criar("Tabua Solta 2", 112, false, TabuaSolta);

        n += Criar("Pedaco de Concreto 1", 121, false, (m, r) => PedacoConcreto(m, r, 0.22f));
        n += Criar("Pedaco de Concreto 2", 122, false, (m, r) => PedacoConcreto(m, r, 0.15f));
        n += Criar("Pedaco de Concreto 3", 123, false, (m, r) => PedacoConcreto(m, r, 0.30f));

        n += Criar("Tijolo", 131, false, (m, r) => Tijolo(m, Matrix4x4.TRS(
            new Vector3(0f, TijA * 0.5f, 0f), Quaternion.identity, Vector3.one)));

        n += Criar("Garrafa", 141, false, Garrafa);
        n += Criar("Lata", 151, false, Lata);

        // vegetacao propria: material URP nosso, para nao depender dos pacotes
        n += Criar("Tufo de Mato 1", 201, false, (m, r) => TufoMato(m, r, "folha", 0.60f, 14));
        n += Criar("Tufo de Mato 2", 202, false, (m, r) => TufoMato(m, r, "folha", 0.72f, 16));
        n += Criar("Tufo Alto", 203, false, (m, r) => TufoMato(m, r, "folha", 0.95f, 16));
        n += Criar("Tufo Seco 1", 211, false, (m, r) => TufoMato(m, r, "folha seca", 0.42f, 11));
        n += Criar("Tufo Seco 2", 212, false, (m, r) => TufoMato(m, r, "folha seca", 0.55f, 12));
        n += Criar("Arbusto 1", 221, false, (m, r) => Arbusto(m, r, 0.38f, 0.70f));
        n += Criar("Arbusto 2", 222, false, (m, r) => Arbusto(m, r, 0.52f, 0.95f));
        n += Criar("Arbusto 3", 223, false, (m, r) => Arbusto(m, r, 0.30f, 0.55f));

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return n;
    }

    static int Criar(string nome, int semente, bool colisor, System.Action<Malha, System.Random> montar)
    {
        Malha malha = new Malha();
        montar(malha, new System.Random(semente));
        if (malha.Vazia) return 0;

        Mesh mesh = SalvarMalha(malha.Criar(nome), Malhas + "/" + nome + ".asset");

        string[] chaves = malha.Chaves;
        Material[] mats = new Material[chaves.Length];
        for (int i = 0; i < chaves.Length; i++) mats[i] = MaterialDe(chaves[i]);

        GameObject raiz = new GameObject(nome);
        raiz.AddComponent<MeshFilter>().sharedMesh = mesh;
        MeshRenderer mr = raiz.AddComponent<MeshRenderer>();
        mr.sharedMaterials = mats;
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;

        if (colisor)
        {
            Bounds b = mesh.bounds;
            BoxCollider bc = raiz.AddComponent<BoxCollider>();
            bc.center = b.center;
            bc.size = new Vector3(Mathf.Max(0.05f, b.size.x * 0.85f),
                                  Mathf.Max(0.05f, b.size.y),
                                  Mathf.Max(0.05f, b.size.z * 0.85f));
        }

        PrefabUtility.SaveAsPrefabAsset(raiz, Prefabs + "/" + nome + ".prefab");
        Object.DestroyImmediate(raiz);
        return 1;
    }

    // ================================================================== pecas
    static void Tijolo(Malha m, Matrix4x4 M)
    {
        m.Prisma("tijolo", Ret(TijL, TijA, 0.006f), TijC, M * Rot(0f, 90f, 0f));
    }

    static void PilhaTijolos(Malha m, System.Random r)
    {
        const float passoX = TijC + 0.012f;
        const float passoZ = TijL + 0.010f;
        for (int c = 0; c < 4; c++)
        {
            float y = TijA * (c + 0.5f) + 0.003f * c;
            float meio = (c % 2) * passoX * 0.5f;
            int n = c < 3 ? 3 : 2;
            for (int i = 0; i < n; i++)
            {
                for (int s = 0; s < 2; s++)
                {
                    float z = (s == 0 ? -0.5f : 0.5f);
                    if (c == 3 && s == 1 && i == 1) continue;   // um vao no topo
                    Vector3 pos = new Vector3((i - (n - 1) * 0.5f) * passoX + meio + R(r, -0.008f, 0.008f),
                                              y, z * passoZ + R(r, -0.006f, 0.006f));
                    Tijolo(m, TRS(pos, new Vector3(0f, R(r, -2.5f, 2.5f), R(r, -1.2f, 1.2f))));
                }
            }
        }
        for (int i = 0; i < 3; i++)   // alguns caidos em volta
        {
            float a = R(r, 0f, Mathf.PI * 2f);
            float d = R(r, 0.35f, 0.6f);
            Tijolo(m, TRS(new Vector3(d * Mathf.Cos(a), TijA * 0.5f, d * Mathf.Sin(a)),
                          new Vector3(R(r, -6f, 6f), R(r, 0f, 360f), R(r, -6f, 6f))));
        }
    }

    static void PedacoConcreto(Malha m, System.Random r, float raio)
    {
        float alt = raio * R(r, 0.35f, 0.6f);
        m.Pedaco("concreto", r, raio, alt,
                 TRS(new Vector3(0f, alt * 0.5f, 0f),
                     new Vector3(R(r, -14f, 14f), R(r, 0f, 360f), R(r, -14f, 14f))));
    }

    static void MonteEntulho(Malha m, System.Random r, float raio, float altura)
    {
        float[] jit = new float[14];
        for (int i = 0; i < jit.Length; i++) jit[i] = R(r, 0.82f, 1.18f);

        Vector2[] perfil = new Vector2[]
        {
            new Vector2(raio, 0f),
            new Vector2(raio * 0.78f, altura * 0.35f),
            new Vector2(raio * 0.45f, altura * 0.72f),
            new Vector2(raio * 0.18f, altura * 0.93f),
            new Vector2(0f, altura),
        };
        m.Lathe("terra", perfil, 14, Matrix4x4.identity, jit);

        for (int i = 0; i < 14; i++)
        {
            float a = R(r, 0f, Mathf.PI * 2f);
            float d = raio * Mathf.Sqrt(R(r, 0.05f, 1f)) * 0.8f;
            float h = Mathf.Max(0.02f, altura * (1f - Mathf.Pow(d / raio, 1.6f)) * R(r, 0.5f, 1f));
            Vector3 pos = new Vector3(d * Mathf.Cos(a), h, d * Mathf.Sin(a));
            Vector3 rot = new Vector3(R(r, -35f, 35f), R(r, 0f, 360f), R(r, -35f, 35f));
            if (i % 3 == 0) Tijolo(m, TRS(pos, rot));
            else
            {
                float rr = R(r, 0.09f, 0.19f);
                m.Pedaco("concreto", r, rr, rr * R(r, 0.4f, 0.7f), TRS(pos, rot));
            }
        }
    }

    static void LajePartida(Malha m, System.Random r, float raio)
    {
        Vector2[] sec = PoligonoIrregular(r, 6, raio, 0.75f);
        float tomb = R(r, 6f, 18f);
        m.Prisma("concreto", sec, 0.075f,
                 TRS(new Vector3(0f, 0.05f, 0f), new Vector3(-90f + tomb, R(r, 0f, 360f), 0f)));
    }

    static void SacoLixo(Malha m, System.Random r, float escala)
    {
        Vector2[] perfil = new Vector2[]
        {
            new Vector2(0f, 0f), new Vector2(0.150f, 0.015f), new Vector2(0.225f, 0.110f),
            new Vector2(0.235f, 0.220f), new Vector2(0.175f, 0.330f), new Vector2(0.085f, 0.395f),
            new Vector2(0.055f, 0.425f), new Vector2(0.100f, 0.465f), new Vector2(0.055f, 0.500f),
            new Vector2(0f, 0.505f),
        };
        for (int i = 0; i < perfil.Length; i++) perfil[i] *= escala;

        float[] jit = new float[11];
        for (int i = 0; i < jit.Length; i++) jit[i] = R(r, 0.88f, 1.13f);
        m.Lathe("plastico", perfil, 11, Matrix4x4.identity, jit);
    }

    static void Caixote(Malha m, System.Random r)
    {
        const float L = 0.46f, P = 0.34f, A = 0.30f, Comp = 0.15f;
        m.Prisma("papelao", Ret(L, P, 0.012f), A, EixoY(new Vector3(0f, A * 0.5f, 0f)));

        Aba(m, new Vector3(L * 0.5f, A, 0f), P * 0.94f, 0f, R(r, 35f, 70f), Comp);
        Aba(m, new Vector3(-L * 0.5f, A, 0f), P * 0.94f, 180f, R(r, 25f, 60f), Comp);
        Aba(m, new Vector3(0f, A, P * 0.5f), L * 0.94f, -90f, R(r, 40f, 75f), Comp);
        Aba(m, new Vector3(0f, A, -P * 0.5f), L * 0.94f, 90f, R(r, 15f, 50f), Comp);
    }

    /// <summary>Aba da caixa girando na dobradica do topo. ang 0 = deitada, 90 = em pe.</summary>
    static void Aba(Malha m, Vector3 dobradica, float largura, float giroY, float ang, float comp)
    {
        float a = ang * Mathf.Deg2Rad;
        Vector3 local = new Vector3(comp * 0.5f * Mathf.Cos(a), comp * 0.5f * Mathf.Sin(a), 0f);
        Quaternion q = Quaternion.Euler(0f, giroY, 0f);
        Vector3 centro = dobradica + q * local;
        Matrix4x4 M = Matrix4x4.TRS(centro, q, Vector3.one) * Rot(0f, 0f, ang);
        m.Prisma("papelao", Ret(comp, 0.008f, 0.003f), largura, M);
    }

    static void MonteAreia(Malha m, System.Random r, float raio, float altura)
    {
        float[] jit = new float[12];
        for (int i = 0; i < jit.Length; i++) jit[i] = R(r, 0.86f, 1.14f);
        Vector2[] perfil = new Vector2[]
        {
            new Vector2(raio, 0f),
            new Vector2(raio * 0.72f, altura * 0.42f),
            new Vector2(raio * 0.38f, altura * 0.76f),
            new Vector2(0f, altura),
        };
        m.Lathe("areia", perfil, 12, Matrix4x4.identity, jit);
    }

    static void Tabua(Malha m, float comp, Matrix4x4 M)
    {
        m.Prisma("madeira", Ret(0.125f, 0.026f, 0.005f), comp, M * Rot(0f, 90f, 0f));
    }

    static void PilhaTabuas(Malha m, System.Random r)
    {
        for (int i = 0; i < 5; i++)
            Tabua(m, R(r, 1.15f, 1.5f),
                  TRS(new Vector3(R(r, -0.05f, 0.05f), 0.013f + i * 0.028f, R(r, -0.03f, 0.03f)),
                      new Vector3(0f, R(r, -6f, 6f), R(r, -1.5f, 1.5f))));
        for (int i = 0; i < 2; i++)
            Tabua(m, R(r, 1.0f, 1.35f),
                  TRS(new Vector3(R(r, -0.1f, 0.1f), 0.16f + i * 0.03f, R(r, -0.1f, 0.1f)),
                      new Vector3(R(r, -4f, 4f), R(r, 60f, 120f), R(r, -6f, 6f))));
    }

    static void TabuaSolta(Malha m, System.Random r)
    {
        Tabua(m, R(r, 0.9f, 1.5f),
              TRS(new Vector3(0f, 0.014f, 0f), new Vector3(0f, R(r, 0f, 360f), R(r, -3f, 3f))));
    }

    static void PilarQuebrado(Malha m, System.Random r, float h)
    {
        const float lado = 0.20f;
        m.Prisma("concreto", Ret(lado, lado, 0.012f), h * 0.82f, EixoY(new Vector3(0f, h * 0.41f, 0f)));

        for (int i = 0; i < 3; i++)   // topo quebrado
        {
            float rr = R(r, 0.05f, 0.085f);
            m.Pedaco("concreto", r, rr, rr * 1.2f,
                     TRS(new Vector3(R(r, -0.06f, 0.06f), h * 0.82f + rr * 0.5f, R(r, -0.06f, 0.06f)),
                         new Vector3(R(r, -20f, 20f), R(r, 0f, 360f), R(r, -20f, 20f))));
        }

        for (int i = 0; i < 3; i++)   // vergalhoes tortos
        {
            float bx = R(r, -0.05f, 0.05f);
            float bz = R(r, -0.05f, 0.05f);
            float y = h * 0.8f;
            float angX = 0f, angZ = 0f;
            for (int s = 0; s < 3; s++)
            {
                const float seg = 0.11f;
                angX += R(r, -22f, 22f);
                angZ += R(r, -22f, 22f);
                m.Prisma("ferrugem", Ret(0.012f, 0.012f, 0.004f), seg,
                         TRS(new Vector3(bx, y + seg * 0.5f, bz), new Vector3(angX, 0f, angZ)) *
                         Rot(-90f, 0f, 0f));
                y += seg * 0.92f;
                bx += Mathf.Sin(angZ * Mathf.Deg2Rad) * seg * 0.5f;
                bz -= Mathf.Sin(angX * Mathf.Deg2Rad) * seg * 0.5f;
            }
        }
    }

    static void Colchao(Malha m, System.Random r)
    {
        const float L = 1.85f, P = 0.88f, A = 0.18f;
        Matrix4x4 giro = Rot(0f, R(r, 0f, 360f), R(r, -2.5f, 2.5f));
        m.Prisma("tecido", Ret(P, A, 0.065f), L, giro * EixoX(new Vector3(0f, A * 0.5f, 0f)));
    }

    static void Garrafa(Malha m, System.Random r)
    {
        Vector2[] perfil = new Vector2[]
        {
            new Vector2(0f, 0f), new Vector2(0.036f, 0.004f), new Vector2(0.039f, 0.100f),
            new Vector2(0.036f, 0.145f), new Vector2(0.016f, 0.185f), new Vector2(0.014f, 0.225f),
            new Vector2(0.018f, 0.232f), new Vector2(0f, 0.234f),
        };
        bool deitada = r.Next(3) == 0;
        Matrix4x4 M = deitada
            ? TRS(new Vector3(0f, 0.039f, 0f), new Vector3(88f, R(r, 0f, 360f), 0f))
            : TRS(Vector3.zero, new Vector3(0f, R(r, 0f, 360f), 0f));
        m.Lathe("vidro", perfil, 9, M, null);
    }

    static void Lata(Malha m, System.Random r)
    {
        Vector2[] perfil = new Vector2[]
        {
            new Vector2(0f, 0f), new Vector2(0.030f, 0.002f), new Vector2(0.033f, 0.012f),
            new Vector2(0.033f, 0.100f), new Vector2(0.030f, 0.113f), new Vector2(0f, 0.116f),
        };
        float[] jit = new float[12];
        for (int i = 0; i < jit.Length; i++) jit[i] = R(r, 0.93f, 1.05f);
        m.Lathe("metal", perfil, 12, TRS(new Vector3(0f, 0.031f, 0f),
                                         new Vector3(90f, R(r, 0f, 360f), 0f)), jit);
    }

    static void Pneu(Malha m, System.Random r)
    {
        const float Raio = 0.30f, Tubo = 0.105f;
        const int nu = 14, nv = 8;
        Matrix4x4 M = TRS(new Vector3(0f, Tubo, 0f), new Vector3(0f, R(r, 0f, 360f), R(r, -4f, 4f)));

        for (int i = 0; i < nu; i++)
        {
            for (int j = 0; j < nv; j++)
            {
                Vector3 a = Toro(Raio, Tubo, i, j, nu, nv);
                Vector3 b = Toro(Raio, Tubo, i + 1, j, nu, nv);
                Vector3 c = Toro(Raio, Tubo, i + 1, j + 1, nu, nv);
                Vector3 d = Toro(Raio, Tubo, i, j + 1, nu, nv);
                m.Quad("borracha", M.MultiplyPoint3x4(a), M.MultiplyPoint3x4(d),
                                   M.MultiplyPoint3x4(c), M.MultiplyPoint3x4(b));
            }
        }
    }

    static Vector3 Toro(float R0, float r0, int i, int j, int nu, int nv)
    {
        float u = 2f * Mathf.PI * i / nu;
        float v = 2f * Mathf.PI * j / nv;
        float rr = r0 * (Mathf.Cos(v) > 0.7f ? 0.82f : 1f);   // banda de rodagem achatada
        return new Vector3((R0 + rr * Mathf.Cos(v)) * Mathf.Cos(u),
                           rr * Mathf.Sin(v),
                           (R0 + rr * Mathf.Cos(v)) * Mathf.Sin(u));
    }

    // ============================================================ vegetacao
    /// <summary>Uma lamina de capim: fita que afina para a ponta, dobrando, com os dois lados.</summary>
    static void Lamina(Malha m, string mat, Vector2 baseXZ, Vector2 dir,
                       float alt, float larg, float curva, int segs)
    {
        Vector3[] p = new Vector3[segs + 1];
        float[] w = new float[segs + 1];
        for (int s = 0; s <= segs; s++)
        {
            float t = (float)s / segs;
            float desl = curva * t * t;
            p[s] = new Vector3(baseXZ.x + dir.x * desl, alt * t, baseXZ.y + dir.y * desl);
            w[s] = larg * (1f - 0.92f * t);
        }

        Vector2 perp = new Vector2(-dir.y, dir.x);
        for (int s = 0; s < segs; s++)
        {
            Vector3 a0 = p[s] + new Vector3(-perp.x * w[s], 0f, -perp.y * w[s]);
            Vector3 a1 = p[s] + new Vector3(perp.x * w[s], 0f, perp.y * w[s]);
            Vector3 b0 = p[s + 1] + new Vector3(-perp.x * w[s + 1], 0f, -perp.y * w[s + 1]);
            Vector3 b1 = p[s + 1] + new Vector3(perp.x * w[s + 1], 0f, perp.y * w[s + 1]);
            m.Quad(mat, a0, a1, b1, b0);
            m.Quad(mat, a0, b0, b1, a1);   // verso, para a folha aparecer dos dois lados
        }
    }

    static void TufoMato(Malha m, System.Random r, string mat, float alt, int laminas)
    {
        for (int i = 0; i < laminas; i++)
        {
            float a = R(r, 0f, Mathf.PI * 2f);
            float d = R(r, 0f, 0.11f);
            float h = alt * R(r, 0.45f, 1f);
            Vector2 dir = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
            Lamina(m, mat, new Vector2(d * dir.x, d * dir.y), dir,
                   h, R(r, 0.016f, 0.026f), R(r, 0.12f, 0.40f) * h / alt, 2);
        }
    }

    static void Arbusto(Malha m, System.Random r, float raio, float alt)
    {
        int lobos = r.Next(3, 5);
        for (int i = 0; i < lobos; i++)
        {
            float rr = raio * R(r, 0.62f, 0.92f);
            float hh = alt * R(r, 0.62f, 0.95f);
            float cx = R(r, -raio * 0.28f, raio * 0.28f);
            float cz = R(r, -raio * 0.28f, raio * 0.28f);
            float cy = alt * R(r, 0.38f, 0.58f);

            Vector2[] perfil = new Vector2[]
            {
                new Vector2(0f, -hh * 0.5f),
                new Vector2(rr * 0.62f, -hh * 0.28f),
                new Vector2(rr, 0f),
                new Vector2(rr * 0.66f, hh * 0.3f),
                new Vector2(0f, hh * 0.5f),
            };
            float[] jit = new float[8];
            for (int k = 0; k < jit.Length; k++) jit[k] = R(r, 0.78f, 1.2f);

            m.Lathe("folha", perfil, 8, TRS(new Vector3(cx, cy, cz), Vector3.zero), jit);
        }

        for (int i = 0; i < 3; i++)   // caules aparecendo por baixo
        {
            float a = R(r, 0f, Mathf.PI * 2f);
            m.Prisma("madeira", Ret(0.030f, 0.030f, 0.008f), alt * 0.45f,
                     TRS(new Vector3(0f, alt * 0.2f, 0f),
                         new Vector3(R(r, 10f, 22f) * Mathf.Cos(a), a * Mathf.Rad2Deg,
                                     R(r, 10f, 22f) * Mathf.Sin(a))) * Rot(-90f, 0f, 0f));
        }
    }

    // ============================================================== utilidades
    static float R(System.Random r, float a, float b) { return a + (float)r.NextDouble() * (b - a); }

    static Matrix4x4 TRS(Vector3 pos, Vector3 euler)
    {
        return Matrix4x4.TRS(pos, Quaternion.Euler(euler), Vector3.one);
    }

    static Matrix4x4 Rot(float x, float y, float z)
    {
        return Matrix4x4.Rotate(Quaternion.Euler(x, y, z));
    }

    /// <summary>Extrusao ao longo de Y do mundo (secao.x -> X, secao.y -> Z).</summary>
    static Matrix4x4 EixoY(Vector3 centro) { return TRS(centro, new Vector3(-90f, 0f, 0f)); }

    /// <summary>Extrusao ao longo de X do mundo (secao.x -> Z, secao.y -> Y).</summary>
    static Matrix4x4 EixoX(Vector3 centro) { return TRS(centro, new Vector3(0f, 90f, 0f)); }

    /// <summary>Retangulo com as 4 quinas cortadas (8 pontos), anti-horario.</summary>
    static Vector2[] Ret(float largura, float altura, float chanfro)
    {
        float x = largura * 0.5f;
        float y = altura * 0.5f;
        float c = Mathf.Min(chanfro, Mathf.Min(x, y) * 0.9f);
        return new Vector2[]
        {
            new Vector2( x - c, -y), new Vector2( x, -y + c), new Vector2( x,  y - c),
            new Vector2( x - c,  y), new Vector2(-x + c,  y), new Vector2(-x,  y - c),
            new Vector2(-x, -y + c), new Vector2(-x + c, -y),
        };
    }

    static Vector2[] PoligonoIrregular(System.Random r, int lados, float raio, float achatamento)
    {
        Vector2[] p = new Vector2[lados];
        for (int i = 0; i < lados; i++)
        {
            float a = 2f * Mathf.PI * (i + R(r, -0.12f, 0.12f)) / lados;
            float rr = raio * R(r, 0.72f, 1f);
            p[i] = new Vector2(rr * Mathf.Cos(a), rr * Mathf.Sin(a) * achatamento);
        }
        return p;
    }

    // ------------------------------------------------------------ salvar asset
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
        existente.indexFormat = nova.indexFormat;
        existente.vertices = nova.vertices;
        existente.uv = nova.uv;
        existente.subMeshCount = nova.subMeshCount;
        for (int i = 0; i < nova.subMeshCount; i++) existente.SetTriangles(nova.GetTriangles(i), i);
        existente.RecalculateNormals();
        existente.RecalculateBounds();
        EditorUtility.SetDirty(existente);
        Object.DestroyImmediate(nova);
        return existente;
    }

    static Material MaterialDe(string chave)
    {
        string caminho = Materiais + "/" + chave + ".mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(caminho);
        if (mat == null)
        {
            mat = new Material(EncontrarShader());
            AssetDatabase.CreateAsset(mat, caminho);
        }

        Color cor = Cores.ContainsKey(chave) ? Cores[chave] : Color.gray;
        float brilho = Brilho.ContainsKey(chave) ? Brilho[chave] : 0.1f;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", cor);
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", cor);
        if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", brilho);
        if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", brilho);
        if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", chave == "metal" ? 0.6f : 0f);
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

    static void GarantirPastas()
    {
        MeusOrganizador.GarantirPasta(Malhas);
        MeusOrganizador.GarantirPasta(Materiais);
        MeusOrganizador.GarantirPasta(Prefabs);
    }

    // ============================================================ construtor
    /// <summary>
    /// Acumula geometria em varios materiais (um submesh por material).
    /// Faces planas: nenhum vertice e compartilhado entre triangulos.
    /// </summary>
    class Malha
    {
        class Parte
        {
            public readonly List<Vector3> v = new List<Vector3>();
            public readonly List<Vector2> uv = new List<Vector2>();
        }

        readonly List<string> _ordem = new List<string>();
        readonly Dictionary<string, Parte> _partes = new Dictionary<string, Parte>();

        public bool Vazia { get { return _ordem.Count == 0; } }
        public string[] Chaves { get { return _ordem.ToArray(); } }

        Parte De(string mat)
        {
            Parte p;
            if (_partes.TryGetValue(mat, out p)) return p;
            p = new Parte();
            _partes[mat] = p;
            _ordem.Add(mat);
            return p;
        }

        public void Tri(string mat, Vector3 a, Vector3 b, Vector3 c)
        {
            Parte p = De(mat);
            p.v.Add(a); p.v.Add(b); p.v.Add(c);
            p.uv.Add(new Vector2(a.x + a.z, a.y));
            p.uv.Add(new Vector2(b.x + b.z, b.y));
            p.uv.Add(new Vector2(c.x + c.z, c.y));
        }

        public void Quad(string mat, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            Tri(mat, a, b, c);
            Tri(mat, a, c, d);
        }

        /// <summary>Secao 2D (anti-horaria) extrudada ao longo do +Z local, centrada.</summary>
        public void Prisma(string mat, Vector2[] sec, float comprimento, Matrix4x4 M)
        {
            int n = sec.Length;
            float z0 = -comprimento * 0.5f;
            float z1 = comprimento * 0.5f;

            for (int i = 0; i < n; i++)
            {
                Vector2 a = sec[i];
                Vector2 b = sec[(i + 1) % n];
                Quad(mat,
                     M.MultiplyPoint3x4(new Vector3(a.x, a.y, z0)),
                     M.MultiplyPoint3x4(new Vector3(b.x, b.y, z0)),
                     M.MultiplyPoint3x4(new Vector3(b.x, b.y, z1)),
                     M.MultiplyPoint3x4(new Vector3(a.x, a.y, z1)));
            }
            Tampa(mat, sec, z1, M, false);
            Tampa(mat, sec, z0, M, true);
        }

        void Tampa(string mat, Vector2[] sec, float z, Matrix4x4 M, bool inverter)
        {
            int n = sec.Length;
            for (int i = 1; i < n - 1; i++)
            {
                Vector3 p0 = M.MultiplyPoint3x4(new Vector3(sec[0].x, sec[0].y, z));
                Vector3 p1 = M.MultiplyPoint3x4(new Vector3(sec[i].x, sec[i].y, z));
                Vector3 p2 = M.MultiplyPoint3x4(new Vector3(sec[i + 1].x, sec[i + 1].y, z));
                if (inverter) Tri(mat, p0, p2, p1);
                else Tri(mat, p0, p1, p2);
            }
        }

        /// <summary>Torno: perfil (raio, altura) girado em torno de Y. jitter bagunca o raio.</summary>
        public void Lathe(string mat, Vector2[] perfil, int segmentos, Matrix4x4 M, float[] jitter)
        {
            float[] fat = new float[segmentos + 1];
            for (int i = 0; i < segmentos; i++) fat[i] = jitter == null ? 1f : jitter[i % jitter.Length];
            fat[segmentos] = fat[0];

            for (int i = 0; i < segmentos; i++)
            {
                for (int k = 0; k < perfil.Length - 1; k++)
                {
                    float r0 = perfil[k].x;
                    float r1 = perfil[k + 1].x;
                    if (r0 < 1e-5f && r1 < 1e-5f) continue;

                    Vector3 a0 = Ponto(perfil, fat, segmentos, i, k, M);
                    Vector3 a1 = Ponto(perfil, fat, segmentos, i + 1, k, M);
                    Vector3 b0 = Ponto(perfil, fat, segmentos, i, k + 1, M);
                    Vector3 b1 = Ponto(perfil, fat, segmentos, i + 1, k + 1, M);

                    if (r0 < 1e-5f) Tri(mat, a0, b0, b1);
                    else if (r1 < 1e-5f) Tri(mat, a0, b0, a1);
                    else Quad(mat, a0, b0, b1, a1);
                }
            }
        }

        static Vector3 Ponto(Vector2[] perfil, float[] fat, int segmentos, int i, int k, Matrix4x4 M)
        {
            float ang = 2f * Mathf.PI * i / segmentos;
            float r = perfil[k].x * fat[i % (segmentos + 1)];
            return M.MultiplyPoint3x4(new Vector3(r * Mathf.Cos(ang), perfil[k].y, r * Mathf.Sin(ang)));
        }

        /// <summary>Pedaco quebrado: base irregular + topo menor, girado e deslocado.</summary>
        public void Pedaco(string mat, System.Random rng, float raio, float altura, Matrix4x4 M)
        {
            int n = rng.Next(5, 8);
            Vector2[] baseP = PoligonoIrregular(rng, n, raio, 1f);
            System.Array.Reverse(baseP);   // no plano XZ o sentido se inverte

            float dx = R(rng, -0.2f, 0.2f) * raio;
            float dz = R(rng, -0.2f, 0.2f) * raio;
            Vector2[] topo = new Vector2[n];
            for (int i = 0; i < n; i++)
                topo[i] = new Vector2(baseP[i].x * R(rng, 0.45f, 0.8f) + dx,
                                      baseP[i].y * R(rng, 0.45f, 0.8f) + dz);

            float y0 = -altura * 0.5f, y1 = altura * 0.5f;
            for (int i = 0; i < n; i++)
            {
                Vector2 a = baseP[i], b = baseP[(i + 1) % n];
                Vector2 c = topo[(i + 1) % n], d = topo[i];
                Quad(mat, P(M, a, y0), P(M, b, y0), P(M, c, y1), P(M, d, y1));
            }
            for (int i = 1; i < n - 1; i++)
            {
                Tri(mat, P(M, baseP[0], y0), P(M, baseP[i + 1], y0), P(M, baseP[i], y0));
                Tri(mat, P(M, topo[0], y1), P(M, topo[i], y1), P(M, topo[i + 1], y1));
            }
        }

        static Vector3 P(Matrix4x4 M, Vector2 p, float y)
        {
            return M.MultiplyPoint3x4(new Vector3(p.x, y, p.y));
        }

        public Mesh Criar(string nome)
        {
            List<Vector3> verts = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int[]> subs = new List<int[]>();

            for (int i = 0; i < _ordem.Count; i++)
            {
                Parte p = _partes[_ordem[i]];
                int b = verts.Count;
                verts.AddRange(p.v);
                uvs.AddRange(p.uv);
                int[] idx = new int[p.v.Count];
                for (int k = 0; k < idx.Length; k++) idx[k] = b + k;
                subs.Add(idx);
            }

            Mesh mesh = new Mesh();
            mesh.name = nome;
            if (verts.Count > 65000) mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.SetVertices(verts);
            mesh.SetUVs(0, uvs);
            mesh.subMeshCount = subs.Count;
            for (int i = 0; i < subs.Count; i++) mesh.SetTriangles(subs[i], i);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
