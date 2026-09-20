using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Pinta o mato do terreno baldio usando o sistema de Details do Terrain
/// (grama em manchas, com ruido Perlin) em vez de milhares de GameObjects.
///
/// Usa os prefabs de vegetacao que ja existem no projeto (TerrainSampleAssets e,
/// como reserva, SimpleNaturePack). Guarda no EditorPrefs quais prototypes foram
/// adicionados, para conseguir desfazer depois sem apagar os seus.
/// </summary>
public static class TerrenoBaldioMato
{
    class Camada
    {
        public string[] caminhos;
        public float peso;        // quanto ocupa em relacao ao maximo
        public float escalaRuido; // em metros: quanto maior, manchas maiores
        public float corte;       // acima disso o mato aparece
        public float alturaMin, alturaMax, larguraMin, larguraMax;
    }

    const string TSA = "Assets/TerrainSampleAssets/Prefabs/";
    const string SNP = "Assets/SimpleNaturePack/Prefabs/";

    static string Meu(string nome) { return TerrenoBaldioAssets.Prefabs + "/" + nome + ".prefab"; }

    /// <summary>
    /// Primeiro os tufos que o proprio builder gera (material URP nosso, cor garantida);
    /// os prefabs dos pacotes ficam so como reserva, caso os nossos nao existam.
    /// </summary>
    static Camada[] Camadas()
    {
        return new Camada[]
        {
            new Camada { caminhos = new string[] { Meu("Tufo Seco 1"), TSA + "GrassDry_A.prefab",
                                                   SNP + "Grass_02.prefab" },
                         peso = 0.85f, escalaRuido = 9f, corte = 0.36f,
                         alturaMin = 0.8f, alturaMax = 1.4f, larguraMin = 0.8f, larguraMax = 1.4f },

            new Camada { caminhos = new string[] { Meu("Tufo de Mato 1"), TSA + "Grass_A.prefab",
                                                   SNP + "Grass_01.prefab" },
                         peso = 0.70f, escalaRuido = 5.5f, corte = 0.45f,
                         alturaMin = 0.8f, alturaMax = 1.3f, larguraMin = 0.8f, larguraMax = 1.3f },

            new Camada { caminhos = new string[] { Meu("Tufo Alto"), TSA + "Plant_B.prefab",
                                                   TSA + "Fern_A.prefab" },
                         peso = 0.35f, escalaRuido = 3.5f, corte = 0.58f,
                         alturaMin = 0.7f, alturaMax = 1.1f, larguraMin = 0.7f, larguraMax = 1.2f },
        };
    }

    static string ChaveInicio(TerrainData td) { return "TerrenoBaldio.mato.inicio." + td.name; }
    static string ChaveQtd(TerrainData td) { return "TerrenoBaldio.mato.qtd." + td.name; }

    // ------------------------------------------------------------------ pintar
    public static string Pintar(Terrain terreno, int semente, float densidade, float margem)
    {
        TerrainData td = terreno.terrainData;

        Apagar(terreno);   // tira o que este builder tinha pintado antes

        if (td.detailResolution < 16)
        {
            td.SetDetailResolution(512, 16);
        }

        List<Camada> usadas = new List<Camada>();
        List<DetailPrototype> protos = new List<DetailPrototype>(td.detailPrototypes);
        int inicio = protos.Count;

        Camada[] todas = Camadas();
        for (int i = 0; i < todas.Length; i++)
        {
            GameObject prefab = Primeiro(todas[i].caminhos);
            if (prefab == null) continue;

            DetailPrototype dp = new DetailPrototype();
            dp.prototype = prefab;
            dp.usePrototypeMesh = true;
            dp.renderMode = DetailRenderMode.VertexLit;
            dp.useInstancing = true;
            dp.minWidth = todas[i].larguraMin;
            dp.maxWidth = todas[i].larguraMax;
            dp.minHeight = todas[i].alturaMin;
            dp.maxHeight = todas[i].alturaMax;
            dp.noiseSpread = 0.6f;
            dp.healthyColor = Color.white;
            dp.dryColor = new Color(0.92f, 0.88f, 0.72f);

            string erro;
            if (!dp.Validate(out erro))
            {
                Debug.LogWarning("[Terreno Baldio] Mato \"" + prefab.name + "\" recusado: " + erro);
                continue;
            }

            protos.Add(dp);
            usadas.Add(todas[i]);
        }

        if (usadas.Count == 0)
            return "Mato do terreno: nao achei prefabs de vegetacao para usar como detail (pulei).";

        td.detailPrototypes = protos.ToArray();
        EditorPrefs.SetInt(ChaveInicio(td), inicio);
        EditorPrefs.SetInt(ChaveQtd(td), usadas.Count);

        int res = td.detailResolution;
        int max = Mathf.Max(1, td.maxDetailScatterPerRes);
        Vector3 tamanho = td.size;
        float menorLado = Mathf.Min(tamanho.x, tamanho.z);
        int borda = Mathf.CeilToInt(margem / Mathf.Max(0.01f, menorLado) * res);
        int total = 0;

        for (int li = 0; li < usadas.Count; li++)
        {
            Camada c = usadas[li];
            float ox = (semente * 37 + li * 113) % 1000 + 0.5f;
            float oz = (semente * 71 + li * 191) % 1000 + 0.5f;
            float f1 = tamanho.x / Mathf.Max(0.01f, c.escalaRuido) / res;
            float f2 = f1 * 2.7f;

            int[,] mapa = new int[res, res];
            for (int a = 0; a < res; a++)
            {
                for (int b = 0; b < res; b++)
                {
                    if (a < borda || a >= res - borda || b < borda || b >= res - borda) continue;

                    float n = 0.65f * Mathf.PerlinNoise(b * f1 + ox, a * f1 + oz)
                            + 0.35f * Mathf.PerlinNoise(b * f2 + ox * 2f, a * f2 + oz * 2f);
                    float cob = Mathf.InverseLerp(c.corte, c.corte + 0.30f, n);
                    if (cob <= 0.02f) continue;

                    int valor = Mathf.RoundToInt(cob * c.peso * densidade * max);
                    valor = Mathf.Clamp(valor, 0, max);
                    mapa[a, b] = valor;
                    total += valor;
                }
            }
            td.SetDetailLayer(0, 0, inicio + li, mapa);
        }

        EditorUtility.SetDirty(td);
        AssetDatabase.SaveAssets();
        return "Mato pintado no terreno: " + usadas.Count + " camadas de detail (resolucao " + res + ").";
    }

    // ------------------------------------------------------------------ apagar
    public static string Apagar(Terrain terreno)
    {
        if (terreno == null) return "";
        TerrainData td = terreno.terrainData;

        if (!EditorPrefs.HasKey(ChaveInicio(td))) return "";
        int inicio = EditorPrefs.GetInt(ChaveInicio(td));
        int qtd = EditorPrefs.GetInt(ChaveQtd(td), 0);

        DetailPrototype[] protos = td.detailPrototypes;
        if (qtd <= 0 || inicio < 0 || inicio + qtd > protos.Length)
        {
            EditorPrefs.DeleteKey(ChaveInicio(td));
            EditorPrefs.DeleteKey(ChaveQtd(td));
            return "";
        }

        int res = td.detailResolution;
        int[,] zero = new int[res, res];
        for (int i = 0; i < qtd; i++) td.SetDetailLayer(0, 0, inicio + i, zero);

        // so tira os prototypes se ninguem adicionou outros depois dos nossos
        if (inicio + qtd == protos.Length)
        {
            List<DetailPrototype> restantes = new List<DetailPrototype>(protos);
            restantes.RemoveRange(inicio, qtd);
            td.detailPrototypes = restantes.ToArray();
        }

        EditorPrefs.DeleteKey(ChaveInicio(td));
        EditorPrefs.DeleteKey(ChaveQtd(td));
        EditorUtility.SetDirty(td);
        AssetDatabase.SaveAssets();
        return "Mato do terreno apagado.";
    }

    static GameObject Primeiro(string[] caminhos)
    {
        for (int i = 0; i < caminhos.Length; i++)
        {
            GameObject g = AssetDatabase.LoadAssetAtPath<GameObject>(caminhos[i]);
            if (g != null) return g;
        }
        return null;
    }
}
