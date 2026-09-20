using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Converte materiais do Built-in Render Pipeline (shader Standard/Legacy/Mobile,
/// ou shader faltando) para URP -- que e o pipeline deste projeto.
///
/// Menu: Dengue > Converter Materiais para URP
///
/// Por que: os pacotes da Asset Store (SimpleNaturePack, CartoonLowPolyCityLite,
/// Park, MiniWorld...) vem com materiais do Built-in. No URP eles aparecem ROSA/ROXO
/// (shader invalido) ou BRANCO (shader faltando, sem textura). A conversao troca o
/// shader para "Universal Render Pipeline/Lit" levando junto textura, cor, normal,
/// emissao, metallic/smoothness e o modo de transparencia.
///
/// As propriedades sao lidas com SerializedObject (m_SavedProperties), entao
/// funciona mesmo quando o shader antigo nao existe mais no projeto -- que e
/// justamente o caso em que o material aparece branco.
/// </summary>
public static class TerrenoBaldioMateriais
{
    [MenuItem("Dengue/Converter Materiais para URP")]
    public static void ConverterProjeto()
    {
        int n = Converter(null, true);
        EditorUtility.DisplayDialog("Materiais para URP",
            n == 0 ? "Nenhum material precisava de conversao." : n + " materiais convertidos para URP.",
            "Ok");
    }

    /// <summary>Converte os materiais das pastas indicadas (null = projeto inteiro).</summary>
    public static int Converter(string[] pastas, bool log)
    {
        Shader lit = Shader.Find("Universal Render Pipeline/Lit");
        Shader unlit = Shader.Find("Universal Render Pipeline/Unlit");
        if (lit == null)
        {
            if (log) Debug.LogWarning("[Materiais] Nao achei o shader do URP -- o projeto esta em URP mesmo?");
            return 0;
        }

        string[] guids = pastas == null
            ? AssetDatabase.FindAssets("t:Material")
            : AssetDatabase.FindAssets("t:Material", pastas);

        List<string> convertidos = new List<string>();
        for (int i = 0; i < guids.Length; i++)
        {
            string caminho = AssetDatabase.GUIDToAssetPath(guids[i]);
            if (string.IsNullOrEmpty(caminho) || !caminho.StartsWith("Assets/")) continue;

            Material m = AssetDatabase.LoadAssetAtPath<Material>(caminho);
            if (m == null || !PrecisaConverter(m)) continue;

            if (Converter(m, lit, unlit)) convertidos.Add(caminho);
        }

        if (convertidos.Count > 0)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            if (log)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.AppendLine("[Materiais] " + convertidos.Count + " materiais convertidos para URP:");
                for (int i = 0; i < convertidos.Count && i < 40; i++) sb.AppendLine("  " + convertidos[i]);
                if (convertidos.Count > 40) sb.AppendLine("  (...)");
                Debug.Log(sb.ToString());
            }
        }
        return convertidos.Count;
    }

    static bool PrecisaConverter(Material m)
    {
        Shader s = m.shader;
        if (s == null) return true;

        string n = s.name;
        if (n.StartsWith("Universal Render Pipeline/")) return false;
        if (n.StartsWith("Shader Graphs/")) return false;
        if (n.StartsWith("Hidden/")) return n == "Hidden/InternalErrorShader";

        // fora da lista de proposito: Skybox, Sprites, UI e Particles (tem shader
        // proprio no URP -- converter para Lit estragaria em vez de arrumar)
        return n == "Standard"
            || n == "Standard (Specular setup)"
            || n == "Autodesk Interactive"
            || n.StartsWith("Legacy Shaders/")
            || n.StartsWith("Mobile/")
            || n.StartsWith("Nature/")
            || n.StartsWith("Unlit/")
            || n.StartsWith("HDRP/");
    }

    // --------------------------------------------------------------- conversao
    static bool Converter(Material m, Shader lit, Shader unlit)
    {
        Lido lido = Lido.De(m);
        bool eraUnlit = m.shader != null && m.shader.name.StartsWith("Unlit/");

        m.shader = eraUnlit && unlit != null ? unlit : lit;
        m.shaderKeywords = new string[0];   // comeca limpo

        // cor e textura base
        Color cor = lido.Cor("_Color", lido.Cor("_BaseColor", Color.white));
        Aplicar(m, "_BaseColor", cor);
        Aplicar(m, "_Color", cor);

        Lido.Tex baseTex = lido.Textura("_MainTex");
        if (baseTex.tex == null) baseTex = lido.Textura("_BaseMap");
        if (baseTex.tex != null)
        {
            AplicarTex(m, "_BaseMap", baseTex);
            AplicarTex(m, "_MainTex", baseTex);
        }

        // normal
        Lido.Tex normal = lido.Textura("_BumpMap");
        if (normal.tex != null && m.HasProperty("_BumpMap"))
        {
            AplicarTex(m, "_BumpMap", normal);
            if (m.HasProperty("_BumpScale")) m.SetFloat("_BumpScale", lido.Float("_BumpScale", 1f));
            m.EnableKeyword("_NORMALMAP");
        }

        // metallic / smoothness
        float metallic = lido.Float("_Metallic", 0f);
        float smooth = lido.Float("_Glossiness", lido.Float("_Smoothness", 0.15f));
        if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", metallic);
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smooth);
        if (m.HasProperty("_WorkflowMode")) m.SetFloat("_WorkflowMode", 1f);   // metallic

        Lido.Tex mg = lido.Textura("_MetallicGlossMap");
        if (mg.tex != null && m.HasProperty("_MetallicGlossMap"))
        {
            AplicarTex(m, "_MetallicGlossMap", mg);
            m.EnableKeyword("_METALLICSPECGLOSSMAP");
        }

        Lido.Tex oc = lido.Textura("_OcclusionMap");
        if (oc.tex != null && m.HasProperty("_OcclusionMap"))
        {
            AplicarTex(m, "_OcclusionMap", oc);
            m.EnableKeyword("_OCCLUSIONMAP");
        }

        // emissao
        Color emissao = lido.Cor("_EmissionColor", Color.black);
        Lido.Tex emap = lido.Textura("_EmissionMap");
        bool temEmissao = emap.tex != null ||
                          (emissao.r + emissao.g + emissao.b) > 0.01f;
        if (temEmissao && m.HasProperty("_EmissionColor"))
        {
            m.SetColor("_EmissionColor", emissao);
            if (emap.tex != null) AplicarTex(m, "_EmissionMap", emap);
            m.EnableKeyword("_EMISSION");
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }
        else
        {
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
        }

        // opaco / cutout / transparente (o _Mode do Standard)
        int modo = Mathf.RoundToInt(lido.Float("_Mode", 0f));
        float cutoff = lido.Float("_Cutoff", 0.5f);
        if (m.HasProperty("_Cutoff")) m.SetFloat("_Cutoff", cutoff);

        if (modo == 1)   // cutout
        {
            DefinirFloat(m, "_Surface", 0f);
            DefinirFloat(m, "_AlphaClip", 1f);
            m.EnableKeyword("_ALPHATEST_ON");
            m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
        }
        else if (modo >= 2)   // fade / transparent
        {
            DefinirFloat(m, "_Surface", 1f);
            DefinirFloat(m, "_Blend", 0f);
            DefinirFloat(m, "_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            DefinirFloat(m, "_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            DefinirFloat(m, "_ZWrite", 0f);
            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }
        else
        {
            DefinirFloat(m, "_Surface", 0f);
            DefinirFloat(m, "_AlphaClip", 0f);
            DefinirFloat(m, "_SrcBlend", (float)UnityEngine.Rendering.BlendMode.One);
            DefinirFloat(m, "_DstBlend", (float)UnityEngine.Rendering.BlendMode.Zero);
            DefinirFloat(m, "_ZWrite", 1f);
            m.renderQueue = -1;
        }

        m.SetShaderPassEnabled("ShadowCaster", true);
        EditorUtility.SetDirty(m);
        return true;
    }

    static void DefinirFloat(Material m, string nome, float v)
    {
        if (m.HasProperty(nome)) m.SetFloat(nome, v);
    }

    static void Aplicar(Material m, string nome, Color c)
    {
        if (m.HasProperty(nome)) m.SetColor(nome, c);
    }

    static void AplicarTex(Material m, string nome, Lido.Tex t)
    {
        if (!m.HasProperty(nome)) return;
        m.SetTexture(nome, t.tex);
        m.SetTextureScale(nome, t.escala);
        m.SetTextureOffset(nome, t.offset);
    }

    // ---------------------------------------------------------------- leitura
    /// <summary>
    /// Le as propriedades salvas do material direto do serializado. Assim funciona
    /// mesmo quando o shader original nao existe mais (material branco/quebrado).
    /// </summary>
    class Lido
    {
        public struct Tex
        {
            public Texture tex;
            public Vector2 escala;
            public Vector2 offset;
        }

        readonly Dictionary<string, Tex> _tex = new Dictionary<string, Tex>();
        readonly Dictionary<string, Color> _cores = new Dictionary<string, Color>();
        readonly Dictionary<string, float> _floats = new Dictionary<string, float>();

        public static Lido De(Material m)
        {
            Lido l = new Lido();
            SerializedObject so = new SerializedObject(m);

            SerializedProperty texs = so.FindProperty("m_SavedProperties.m_TexEnvs");
            if (texs != null)
            {
                for (int i = 0; i < texs.arraySize; i++)
                {
                    SerializedProperty e = texs.GetArrayElementAtIndex(i);
                    SerializedProperty nome = e.FindPropertyRelative("first");
                    SerializedProperty val = e.FindPropertyRelative("second");
                    if (nome == null || val == null) continue;

                    Tex t = new Tex();
                    SerializedProperty pt = val.FindPropertyRelative("m_Texture");
                    SerializedProperty ps = val.FindPropertyRelative("m_Scale");
                    SerializedProperty po = val.FindPropertyRelative("m_Offset");
                    t.tex = pt != null ? pt.objectReferenceValue as Texture : null;
                    t.escala = ps != null ? ps.vector2Value : Vector2.one;
                    t.offset = po != null ? po.vector2Value : Vector2.zero;
                    l._tex[nome.stringValue] = t;
                }
            }

            SerializedProperty cores = so.FindProperty("m_SavedProperties.m_Colors");
            if (cores != null)
            {
                for (int i = 0; i < cores.arraySize; i++)
                {
                    SerializedProperty e = cores.GetArrayElementAtIndex(i);
                    SerializedProperty nome = e.FindPropertyRelative("first");
                    SerializedProperty val = e.FindPropertyRelative("second");
                    if (nome == null || val == null) continue;
                    l._cores[nome.stringValue] = val.colorValue;
                }
            }

            SerializedProperty nums = so.FindProperty("m_SavedProperties.m_Floats");
            if (nums != null)
            {
                for (int i = 0; i < nums.arraySize; i++)
                {
                    SerializedProperty e = nums.GetArrayElementAtIndex(i);
                    SerializedProperty nome = e.FindPropertyRelative("first");
                    SerializedProperty val = e.FindPropertyRelative("second");
                    if (nome == null || val == null) continue;
                    l._floats[nome.stringValue] = val.floatValue;
                }
            }

            // reserva: se o serializado nao entregou nada (formato diferente), le pela
            // API normal do material -- funciona desde que o shader antigo ainda exista
            if (l._tex.Count == 0 && l._cores.Count == 0) l.PelaApi(m);
            return l;
        }

        void PelaApi(Material m)
        {
            string[] texturas = { "_MainTex", "_BaseMap", "_BumpMap", "_EmissionMap",
                                  "_MetallicGlossMap", "_OcclusionMap" };
            for (int i = 0; i < texturas.Length; i++)
            {
                if (!m.HasProperty(texturas[i])) continue;
                Tex t = new Tex();
                t.tex = m.GetTexture(texturas[i]);
                t.escala = m.GetTextureScale(texturas[i]);
                t.offset = m.GetTextureOffset(texturas[i]);
                _tex[texturas[i]] = t;
            }

            string[] cores = { "_Color", "_BaseColor", "_EmissionColor" };
            for (int i = 0; i < cores.Length; i++)
                if (m.HasProperty(cores[i])) _cores[cores[i]] = m.GetColor(cores[i]);

            string[] nums = { "_Metallic", "_Glossiness", "_Smoothness", "_Cutoff", "_Mode", "_BumpScale" };
            for (int i = 0; i < nums.Length; i++)
                if (m.HasProperty(nums[i])) _floats[nums[i]] = m.GetFloat(nums[i]);
        }

        public Tex Textura(string nome)
        {
            Tex t;
            if (_tex.TryGetValue(nome, out t)) return t;
            t = new Tex();
            t.escala = Vector2.one;
            t.offset = Vector2.zero;
            return t;
        }

        public Color Cor(string nome, Color padrao)
        {
            Color c;
            return _cores.TryGetValue(nome, out c) ? c : padrao;
        }

        public float Float(string nome, float padrao)
        {
            float f;
            return _floats.TryGetValue(nome, out f) ? f : padrao;
        }
    }
}
