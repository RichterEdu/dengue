using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/* ============================================================
 *  HUDLayoutAjustador.cs  (Editor)
 *  ---------------------------------------------------------------------
 *  Utilitário de migração/ajuste da HUD:
 *  Move a barra de Ocupação Hospitalar para o centro superior e
 *  desce o ToastPanel para evitar sobreposições na tela.
 *  Preserva todos os componentes, bindings e dados serializados.
 *
 *  Menu: Dengue > Ajustar Layout da HUD (Ocupação no Topo)
 * ============================================================ */

public static class HUDLayoutAjustador
{
    [MenuItem("Dengue/Ajustar Layout da HUD (Ocupação no Topo)")]
    public static void AjustarLayout()
    {
        GameObject hudRoot = GameObject.Find("DengueHUD");
        if (hudRoot == null)
        {
            EditorUtility.DisplayDialog("Ajustar Layout da HUD",
                "Canvas 'DengueHUD' não foi encontrado na cena aberta.", "OK");
            return;
        }

        Undo.RegisterFullObjectHierarchyUndo(hudRoot, "Ajustar Layout da HUD");

        bool alterou = false;

        // 1. Reposicionar OcupacaoPanel para o topo central
        Transform ocupacaoT = hudRoot.transform.Find("OcupacaoPanel");
        if (ocupacaoT != null && ocupacaoT is RectTransform ocupacaoRt)
        {
            ocupacaoRt.anchorMin = new Vector2(0.5f, 1f);
            ocupacaoRt.anchorMax = new Vector2(0.5f, 1f);
            ocupacaoRt.pivot = new Vector2(0.5f, 1f);
            ocupacaoRt.anchoredPosition = new Vector2(0f, -32f);
            ocupacaoRt.sizeDelta = new Vector2(560f, 84f);
            EditorUtility.SetDirty(ocupacaoRt);
            alterou = true;
            Debug.Log("[HUDLayoutAjustador] OcupacaoPanel movido para o centro superior (y = -32).");
        }

        // 2. Reposicionar ToastPanel logo abaixo da ocupação
        Transform toastT = hudRoot.transform.Find("ToastPanel");
        if (toastT != null && toastT is RectTransform toastRt)
        {
            toastRt.anchorMin = new Vector2(0.5f, 1f);
            toastRt.anchorMax = new Vector2(0.5f, 1f);
            toastRt.pivot = new Vector2(0.5f, 1f);
            toastRt.anchoredPosition = new Vector2(0f, -126f);
            EditorUtility.SetDirty(toastRt);
            alterou = true;
            Debug.Log("[HUDLayoutAjustador] ToastPanel reposicionado para y = -126.");
        }

        if (alterou)
        {
            EditorSceneManager.MarkSceneDirty(hudRoot.scene);
            EditorUtility.DisplayDialog("Ajustar Layout da HUD",
                "Layout atualizado com sucesso!\n\n• Ocupação Hospitalar: Centro Superior (y = -32)\n• Avisos / Toast: Abaixo da barra de ocupação (y = -126)\n• Hotbar: Área inferior livre",
                "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Ajustar Layout da HUD",
                "Nenhum painel correspondente (OcupacaoPanel / ToastPanel) precisou ser alterado.",
                "OK");
        }
    }
}
