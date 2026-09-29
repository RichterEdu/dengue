using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Impede que a MenuInicial e a SampleScene rodem JUNTAS no Play do Editor.
///
/// Com as duas abertas na Hierarchy, o Play roda as duas ao mesmo tempo: dois
/// EventSystems e dois AudioListeners, o MenuInicial.Awake força timeScale = 1 por
/// cima da abertura e ainda pré-carrega outra SampleScene por trás. Aconteceu de
/// verdade (2026-09-28): o jogo parecia "iniciar direto".
///
/// Ao apertar Play com as duas abertas, fica só a cena ATIVA:
///   - a outra, sem mudanças não salvas, é fechada e o Console explica;
///   - a outra, com mudanças não salvas, o Play é cancelado (nada é descartado).
/// Sem diálogo, para não travar o CLI.
/// </summary>
[InitializeOnLoad]
public static class GuardaDeCenas
{
    static GuardaDeCenas()
    {
        EditorApplication.playModeStateChanged += AoMudarPlayMode;
    }

    private static void AoMudarPlayMode(PlayModeStateChange estado)
    {
        if (estado != PlayModeStateChange.ExitingEditMode)
            return;

        Scene menu = SceneManager.GetSceneByName(MenuInicial.NomeCena);
        Scene jogo = SceneManager.GetSceneByName(MenuInicial.NomeCenaJogo);
        if (!menu.isLoaded || !jogo.isLoaded)
            return;

        Scene ativa = SceneManager.GetActiveScene();
        Scene fica = ativa == jogo ? jogo : menu;
        Scene extra = fica == menu ? jogo : menu;
        string nomeFica = fica.name;
        string nomeExtra = extra.name;   // o handle da cena fica inválido depois do CloseScene

        if (extra.isDirty)
        {
            EditorApplication.isPlaying = false;
            Debug.LogWarning($"[Cenas] Play cancelado: {nomeFica} e {nomeExtra} estão abertas juntas e rodariam ao mesmo tempo. " +
                             $"Salve (ou descarte) as mudanças de {nomeExtra} e feche-a na Hierarchy (botão direito > Remove Scene), " +
                             "ou deixe aberta só a cena que quer testar.");
            return;
        }

        if (!EditorSceneManager.CloseScene(extra, true))
        {
            EditorApplication.isPlaying = false;
            Debug.LogWarning($"[Cenas] Play cancelado: {nomeFica} e {nomeExtra} estão abertas juntas e não consegui fechar {nomeExtra}. " +
                             "Feche uma delas na Hierarchy (botão direito > Remove Scene) e aperte Play de novo.");
            return;
        }

        Debug.LogWarning($"[Cenas] {nomeFica} e {nomeExtra} estavam abertas juntas; fechei {nomeExtra} para o Play rodar só {nomeFica}.");
    }
}
