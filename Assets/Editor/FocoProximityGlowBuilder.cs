using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/* ============================================================
 *  FocoProximityGlowBuilder.cs  (Editor)
 *  ---------------------------------------------------------------------
 *  Garante o componente FocoProximityGlow no player (mesmo objeto que
 *  já tem o Interactor, achado por CharacterController — igual ao
 *  PlayerBuilder.cs). Idempotente: se já existir, não mexe.
 *
 *  Menu:  Dengue > Configurar Brilho de Proximidade dos Focos
 * ============================================================ */

public static class FocoProximityGlowBuilder
{
    [MenuItem("Dengue/Configurar Brilho de Proximidade dos Focos")]
    public static void Configurar()
    {
        CharacterController controller = Object.FindFirstObjectByType<CharacterController>();
        if (controller == null)
        {
            EditorUtility.DisplayDialog("Brilho de Proximidade",
                "Não encontrei nenhum CharacterController (o player) na cena.", "OK");
            return;
        }

        GameObject player = controller.gameObject;

        if (player.GetComponent<FocoProximityGlow>() != null)
        {
            EditorUtility.DisplayDialog("Brilho de Proximidade",
                "O player já tem o FocoProximityGlow configurado.", "OK");
            return;
        }

        Undo.AddComponent<FocoProximityGlow>(player);
        EditorSceneManager.MarkSceneDirty(player.scene);
        Selection.activeGameObject = player;

        Debug.Log("[Dengue] FocoProximityGlow adicionado ao player. Os focos agora brilham quando o jogador se aproxima.", player);
    }
}
