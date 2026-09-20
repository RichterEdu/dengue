using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/* ============================================================
 *  GarrafaFocoBuilder.cs  (Editor)
 *  ---------------------------------------------------------------------
 *  Transforma um grupo de garrafas JÁ MONTADO NA CENA num foco de
 *  criadouro onde as garrafas somem UMA POR CLIQUE.
 *
 *  Menu:  Dengue > Transformar Selecao em Foco (Garrafas)
 *
 *  Selecione na Hierarchy o objeto raiz (a garrafa "mãe", com as outras
 *  penduradas nela) e rode o menu. Ele:
 *   1. põe um BoxCollider em cada garrafa que ainda não tem collider —
 *      o Unity já ajusta a caixa à malha sozinho. É o que faz o raycast
 *      do Interactor enxergar o foco (o Interactor procura o
 *      Interactable nos PAIS, então collider nas filhas funciona);
 *   2. põe o GarrafaInteractable na RAIZ (um foco só para o grupo
 *      inteiro, não um por garrafa) com a lista das garrafas preenchida;
 *   3. deixa os cliques necessários iguais ao número de garrafas, para
 *      cada clique tirar uma e a última fechar o foco;
 *   4. zera o punch da base (quem anima agora é cada garrafa, não o
 *      grupo inteiro);
 *   5. põe um FocoHUDInfo com os textos da HUD já preenchidos.
 *
 *  Dá para rodar em vários grupos de uma vez (seleção múltipla) e de
 *  novo no mesmo grupo: a lista de garrafas é refeita, o que já existe
 *  é respeitado e nada é duplicado. Tudo passa por Undo — Ctrl+Z desfaz.
 * ============================================================ */

public static class GarrafaFocoBuilder
{
    [MenuItem("Dengue/Transformar Selecao em Foco (Garrafas)")]
    public static void Converter()
    {
        GameObject[] alvos = Selection.gameObjects;
        if (alvos == null || alvos.Length == 0)
        {
            EditorUtility.DisplayDialog("Foco de Garrafas",
                "Selecione na Hierarchy o objeto raiz do grupo de garrafas (a garrafa que tem as outras penduradas nela) e rode o menu de novo.",
                "OK");
            return;
        }

        StringBuilder resumo = new StringBuilder();
        int convertidos = 0;

        foreach (GameObject raiz in alvos)
        {
            if (raiz == null || !raiz.scene.IsValid())
            {
                Debug.LogWarning("[Dengue] \"" + (raiz != null ? raiz.name : "?") +
                                 "\" não está na cena (é um asset do Project). Arraste para a cena antes de converter.");
                continue;
            }

            // Não mexe em algo que já é foco de outro tipo (pneu, vaso, caixa...).
            Interactable existente = raiz.GetComponent<Interactable>();
            if (existente != null && !(existente is GarrafaInteractable))
            {
                Debug.LogWarning("[Dengue] \"" + raiz.name + "\" já tem um " + existente.GetType().Name +
                                 " — deixei como está.");
                continue;
            }

            Undo.RegisterFullObjectHierarchyUndo(raiz, "Transformar em Foco de Garrafas");

            int colliders = GarantirColliders(raiz);
            List<Renderer> garrafas = BuscarGarrafas(raiz);

            GarrafaInteractable foco = existente as GarrafaInteractable;
            if (foco == null)
                foco = Undo.AddComponent<GarrafaInteractable>(raiz);

            SerializedObject so = new SerializedObject(foco);
            LigarGarrafas(so, garrafas);
            so.FindProperty("requiredClicks").intValue = Mathf.Max(1, garrafas.Count);
            so.FindProperty("punchScale").floatValue = 0f;   // quem anima é cada garrafa
            so.ApplyModifiedProperties();

            if (raiz.GetComponent<FocoHUDInfo>() == null)
            {
                FocoHUDInfo info = Undo.AddComponent<FocoHUDInfo>(raiz);
                info.nome = garrafas.Count > 1 ? "as garrafas" : "a garrafa";
                info.textoAoMirar = "Clique para recolher {nome}";
                info.dica = "Garrafa jogada no mato junta água de chuva. Recolha, ou deixe de boca para baixo.";
                // textoAoEliminar fica vazio de propósito: usa o padrão "Foco eliminado!"
                // do DengueHUD, igual ao pneu — só a dica acima muda por tipo de foco.
                EditorUtility.SetDirty(info);
            }

            EditorSceneManager.MarkSceneDirty(raiz.scene);
            convertidos++;
            resumo.Append("\n  • ").Append(raiz.name)
                  .Append(": ").Append(garrafas.Count).Append(garrafas.Count == 1 ? " garrafa" : " garrafas")
                  .Append(" (= ").Append(Mathf.Max(1, garrafas.Count)).Append(" cliques), ")
                  .Append(colliders).Append(" collider(s) novo(s)");
        }

        if (convertidos > 0)
            Debug.Log("[Dengue] " + convertidos + " foco(s) de garrafa criado(s):" + resumo +
                      "\nA cada clique some uma garrafa (pulinho + giro + encolher); a última fecha o foco. " +
                      "A ordem padrão é a mais perto do jogador primeiro — dá para trocar no Inspector.");
    }

    /* Todo objeto com malha visível precisa de collider para o raycast
     * do Interactor acertar. O BoxCollider recém-adicionado já nasce do
     * tamanho da malha. */
    private static int GarantirColliders(GameObject raiz)
    {
        int novos = 0;
        foreach (MeshRenderer r in raiz.GetComponentsInChildren<MeshRenderer>(true))
        {
            if (EhAgua(r.gameObject.name))
                continue;
            if (r.GetComponent<Collider>() != null)
                continue;

            Undo.AddComponent<BoxCollider>(r.gameObject);
            novos++;
        }
        return novos;
    }

    /* Cada MeshRenderer do grupo é uma garrafa. Objetos de água são
     * ignorados: eles são filhos da garrafa e somem junto com ela. */
    private static List<Renderer> BuscarGarrafas(GameObject raiz)
    {
        List<Renderer> garrafas = new List<Renderer>();
        foreach (MeshRenderer r in raiz.GetComponentsInChildren<MeshRenderer>(true))
            if (!EhAgua(r.gameObject.name))
                garrafas.Add(r);
        return garrafas;
    }

    private static void LigarGarrafas(SerializedObject so, List<Renderer> garrafas)
    {
        SerializedProperty lista = so.FindProperty("garrafas");
        lista.arraySize = garrafas.Count;
        for (int i = 0; i < garrafas.Count; i++)
            lista.GetArrayElementAtIndex(i).objectReferenceValue = garrafas[i];
    }

    private static bool EhAgua(string nome)
    {
        if (string.IsNullOrEmpty(nome))
            return false;
        string n = nome.TrimStart().ToLowerInvariant();
        return n.StartsWith("agua") || n.StartsWith("água");
    }
}
