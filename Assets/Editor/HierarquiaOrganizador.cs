using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

/// <summary>
/// Arruma a Hierarchy da cena ativa em grupos (pais vazios na origem), sem mudar
/// a posição de nada no mundo.
///
/// Menu: Dengue > Organizar Hierarquia
///
/// Resultado:
///   --- SISTEMAS ---             FluxoDeJogo, Historia, Sons
///   --- JOGADOR ---              Player, Main Camera (a câmera continua irmã do Player)
///   --- ILUMINACAO ---           luz direcional, Global Volume
///   --- UI ---                   EventSystem e todos os Canvas
///   --- FOCOS ---                tudo que tem Interactable
///   --- BAIRRO ---/<subgrupo>    terrenos, ruas, construções, vegetação, objetos, cercas, limites
///   --- HOSPITAL (INTERIOR) ---  SalaDoHospital
///   --- TESTES ---               sobras de teste
///
/// Só olha os objetos soltos na RAIZ: o que já está dentro de um grupo não é mexido.
/// Primeiro classifica por componente (foco ou canvas novo cai sozinho no grupo certo),
/// depois pela tabela de prefixos de nome abaixo. O que não se encaixar fica na raiz
/// e aparece no Console. Pode rodar de novo; tudo vira um passo só de Undo.
/// </summary>
public static class HierarquiaOrganizador
{
    public const string Sistemas = "--- SISTEMAS ---";
    public const string Jogador = "--- JOGADOR ---";
    public const string Iluminacao = "--- ILUMINACAO ---";
    public const string UI = "--- UI ---";
    public const string Focos = "--- FOCOS ---";
    public const string Bairro = "--- BAIRRO ---";
    public const string Hospital = "--- HOSPITAL (INTERIOR) ---";
    public const string Testes = "--- TESTES ---";

    const string PrefixoGrupo = "--- ";

    // ordem dos grupos na raiz e dos subgrupos dentro do BAIRRO
    static readonly string[] OrdemGrupos = { Sistemas, Jogador, Iluminacao, UI, Focos, Bairro, Hospital, Testes };
    static readonly string[] OrdemBairro = { "Terrenos", "Ruas e Praca", "Construcoes", "Vegetacao", "Objetos de Rua", "Cercas", "Limites do Mapa" };

    // cenário do bairro: vale o PRIMEIRO prefixo que bater (por isso "NaturePark Hovels" vem antes de "NaturePark")
    static readonly string[,] Prefixos =
    {
        { "Cenario Terreno Baldio", Bairro + "/Terrenos" },
        { "Quadra",                 Bairro + "/Ruas e Praca" },
        { "Praca",                  Bairro + "/Ruas e Praca" },
        { "NaturePark Hovels",      Bairro + "/Construcoes" },
        { "Hospital",               Bairro + "/Construcoes" },
        { "Coffe house",            Bairro + "/Construcoes" },
        { "House_",                 Bairro + "/Construcoes" },
        { "Building",               Bairro + "/Construcoes" },
        { "building",               Bairro + "/Construcoes" },
        { "Market",                 Bairro + "/Construcoes" },
        { "School",                 Bairro + "/Construcoes" },
        { "Police_Station",         Bairro + "/Construcoes" },
        { "Workshop",               Bairro + "/Construcoes" },
        { "Fire_Station",           Bairro + "/Construcoes" },
        { "LPW_Tree",               Bairro + "/Vegetacao" },
        { "NaturePark",             Bairro + "/Vegetacao" },
        { "Car_",                   Bairro + "/Objetos de Rua" },
        { "Trash_",                 Bairro + "/Objetos de Rua" },
        { "Escada de Mao",          Bairro + "/Objetos de Rua" },
        { "cerca",                  Bairro + "/Cercas" },
        { "FimDoMundo",             Bairro + "/Limites do Mapa" },
        { "Teste",                  Testes },
    };

    [MenuItem("Dengue/Organizar Hierarquia")]
    public static void Organizar()
    {
        Scene cena = SceneManager.GetActiveScene();
        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Organizar Hierarquia");
        int grupoUndo = Undo.GetCurrentGroup();

        List<string> movidos = new List<string>();
        List<string> soltos = new List<string>();

        GameObject[] raizes = cena.GetRootGameObjects();
        for (int i = 0; i < raizes.Length; i++)
        {
            GameObject go = raizes[i];
            if (go.name.StartsWith(PrefixoGrupo)) continue;

            string grupo = GrupoDe(go);
            if (grupo == null)
            {
                soltos.Add(go.name);
                continue;
            }

            Undo.SetTransformParent(go.transform, AcharOuCriarGrupo(cena, grupo), true, "Organizar Hierarquia");
            movidos.Add(go.name + " -> " + grupo);
        }

        OrdenarGrupos(cena);

        Undo.CollapseUndoOperations(grupoUndo);
        if (movidos.Count > 0) EditorSceneManager.MarkSceneDirty(cena);

        StringBuilder sb = new StringBuilder();
        sb.AppendLine(movidos.Count + " objetos movidos para grupos:");
        for (int i = 0; i < movidos.Count; i++) sb.AppendLine("  " + movidos[i]);
        if (soltos.Count > 0)
        {
            sb.AppendLine("Ficaram na raiz (não sei onde pôr; adicione um prefixo em HierarquiaOrganizador.Prefixos):");
            for (int i = 0; i < soltos.Count; i++) sb.AppendLine("  " + soltos[i]);
        }
        Debug.Log("[Organizar Hierarquia] " + sb.ToString());
    }

    /// <summary>
    /// Para os builders que apagam e recriam um objeto: põe o novo no mesmo pai e
    /// na mesma posição entre os irmãos do antigo. Com pai nulo (antigo na raiz ou
    /// inexistente), não faz nada.
    /// </summary>
    public static void RecolocarNoLugar(GameObject novo, Transform pai, int indice)
    {
        if (novo == null || pai == null) return;
        novo.transform.SetParent(pai, true);
        novo.transform.SetSiblingIndex(Mathf.Min(indice, pai.childCount - 1));
    }

    // ---------------------------------------------------------- classificação
    static string GrupoDe(GameObject go)
    {
        if (go.GetComponent<Interactable>() != null) return Focos;
        if (go.GetComponent<Canvas>() != null || go.GetComponent<EventSystem>() != null) return UI;
        if (go.GetComponent<ControladorJogador>() != null || go.GetComponent<CameraJogador>() != null) return Jogador;
        if (go.GetComponent<Light>() != null || go.GetComponent<Volume>() != null) return Iluminacao;
        if (go.GetComponent<FluxoDeJogo>() != null || go.GetComponent<HistoriaDoJogo>() != null || go.GetComponent<TrilhaSonora>() != null) return Sistemas;
        if (go.GetComponentInChildren<SaidaDoHospital>(true) != null) return Hospital;
        if (go.GetComponent<Terrain>() != null) return Bairro + "/Terrenos";

        for (int i = 0; i < Prefixos.GetLength(0); i++)
            if (go.name.StartsWith(Prefixos[i, 0])) return Prefixos[i, 1];

        return null;
    }

    // ----------------------------------------------------------------- grupos
    static Transform AcharOuCriarGrupo(Scene cena, string caminho)
    {
        string[] partes = caminho.Split('/');

        Transform atual = null;
        GameObject[] raizes = cena.GetRootGameObjects();
        for (int i = 0; i < raizes.Length; i++)
            if (raizes[i].name == partes[0]) { atual = raizes[i].transform; break; }
        if (atual == null) atual = CriarGrupo(partes[0], null, cena);

        for (int p = 1; p < partes.Length; p++)
        {
            Transform filho = atual.Find(partes[p]);
            atual = filho != null ? filho : CriarGrupo(partes[p], atual, cena);
        }
        return atual;
    }

    static Transform CriarGrupo(string nome, Transform pai, Scene cena)
    {
        GameObject go = new GameObject(nome);
        if (pai == null) SceneManager.MoveGameObjectToScene(go, cena);
        else go.transform.SetParent(pai, false);
        Undo.RegisterCreatedObjectUndo(go, "Organizar Hierarquia");
        return go.transform;
    }

    static void OrdenarGrupos(Scene cena)
    {
        GameObject[] raizes = cena.GetRootGameObjects();
        int indice = 0;
        for (int g = 0; g < OrdemGrupos.Length; g++)
        {
            for (int i = 0; i < raizes.Length; i++)
            {
                if (raizes[i].name != OrdemGrupos[g]) continue;
                raizes[i].transform.SetSiblingIndex(indice++);

                if (OrdemGrupos[g] != Bairro) break;
                int sub = 0;
                for (int s = 0; s < OrdemBairro.Length; s++)
                {
                    Transform t = raizes[i].transform.Find(OrdemBairro[s]);
                    if (t != null) t.SetSiblingIndex(sub++);
                }
                break;
            }
        }
    }
}
