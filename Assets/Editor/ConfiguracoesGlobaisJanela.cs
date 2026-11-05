using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/* ============================================================
 *  ConfiguracoesGlobaisJanela.cs  (Editor)
 *  ---------------------------------------------------------------------
 *  Menu:  Dengue > Configurações Globais
 *
 *  Muda de uma vez os valores de todos os focos de um tipo (aba Focos) e
 *  os valores da partida (aba Partida), na cena do FluxoDeJogo. A lógica
 *  fica em ConfiguracoesGlobais.cs; aqui é só a interface.
 *
 *  Focos: cada tipo tem o próprio Aplicar, e só os campos que você mudou
 *  são gravados (valores diferentes entre focos aparecem como "—" e ficam
 *  como estão). Partida: grava na hora, como o Inspector.
 *
 *  Não salva a cena: fica marcada como modificada, para o Ctrl+S ser seu.
 *  Quem marca é o ApplyModifiedProperties, e o Ctrl+Z desmarca junto (um
 *  MarkSceneDirty aqui deixaria a cena suja depois de desfazer).
 *  Fechada em Play Mode: baixar os passos de um foco já começado o trava.
 * ============================================================ */

public class ConfiguracoesGlobaisJanela : EditorWindow
{
    private static readonly string[] Abas = { "Focos", "Partida" };
    private const float LarguraDoRotuloDaPartida = 210f;

    // O que foi mudado e ainda não aplicado, por tipo.
    private readonly Dictionary<ConfiguracoesGlobais.TipoDeFoco, ConfiguracoesGlobais.Valores> pendentes =
        new Dictionary<ConfiguracoesGlobais.TipoDeFoco, ConfiguracoesGlobais.Valores>();

    private readonly List<SerializedObject> partida = new List<SerializedObject>();
    private List<Interactable> focos = new List<Interactable>();
    private Scene cena;
    private bool temCena;
    private int aba;
    private Vector2 rolagem;

    [MenuItem("Dengue/Configurações Globais")]
    public static void Abrir()
    {
        ConfiguracoesGlobaisJanela janela = GetWindow<ConfiguracoesGlobaisJanela>(false, "Configurações Globais", true);
        janela.minSize = new Vector2(440f, 520f);
        janela.Show();
    }

    private void OnEnable()
    {
        Undo.undoRedoPerformed += AoMudarAlgo;
        EditorApplication.playModeStateChanged += AoMudarPlayMode;
        EditorSceneManager.sceneOpened += AoAbrirCena;
        EditorSceneManager.sceneClosed += AoFecharCena;
        Reler();
    }

    private void OnDisable()
    {
        Undo.undoRedoPerformed -= AoMudarAlgo;
        EditorApplication.playModeStateChanged -= AoMudarPlayMode;
        EditorSceneManager.sceneOpened -= AoAbrirCena;
        EditorSceneManager.sceneClosed -= AoFecharCena;
    }

    private void OnFocus() { Reler(); }
    private void OnHierarchyChange() { AoMudarAlgo(); }
    private void AoMudarPlayMode(PlayModeStateChange _) { AoMudarAlgo(); }
    private void AoAbrirCena(Scene _, OpenSceneMode __) { AoMudarAlgo(); }
    private void AoFecharCena(Scene _) { AoMudarAlgo(); }

    private void AoMudarAlgo()
    {
        Reler();
        Repaint();
    }

    private void Reler()
    {
        partida.Clear();
        focos.Clear();
        temCena = ConfiguracoesGlobais.AcharCenaDoJogo(out cena);
        if (!temCena)
            return;

        focos = ConfiguracoesGlobais.FocosDaCena(cena);
        foreach (ConfiguracoesGlobais.GrupoDaPartida grupo in ConfiguracoesGlobais.Partida)
        {
            Component alvo = ConfiguracoesGlobais.ComponenteDaCena(cena, grupo.Tipo);
            partida.Add(alvo != null ? new SerializedObject(alvo) : null);
        }
    }

    private void OnGUI()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorGUILayout.HelpBox("Saia do Play Mode para mudar as configurações.", MessageType.Info);
            return;
        }
        if (!temCena)
        {
            EditorGUILayout.HelpBox("Abra a SampleScene (a cena do jogo, com FluxoDeJogo).", MessageType.Warning);
            return;
        }

        aba = GUILayout.Toolbar(aba, Abas);
        if (cena.isDirty)
            EditorGUILayout.HelpBox("A cena " + cena.name + " tem mudanças sem salvar (Ctrl+S).", MessageType.None);

        rolagem = EditorGUILayout.BeginScrollView(rolagem);
        if (aba == 0)
            DesenharFocos();
        else
            DesenharPartida();
        EditorGUILayout.EndScrollView();
    }

    /* ================= Aba Focos ================= */

    private void DesenharFocos()
    {
        foreach (ConfiguracoesGlobais.TipoDeFoco tipo in ConfiguracoesGlobais.Tipos)
            DesenharTipo(tipo);
    }

    private void DesenharTipo(ConfiguracoesGlobais.TipoDeFoco tipo)
    {
        ConfiguracoesGlobais.Leitura leitura = ConfiguracoesGlobais.Ler(tipo, focos);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField($"{tipo.Rotulo} ({leitura.Quantidade})", EditorStyles.boldLabel);
        EditorGUILayout.LabelField(ConfiguracoesGlobais.Resumo(tipo, focos), EditorStyles.wordWrappedMiniLabel);

        if (leitura.Quantidade > 0)
        {
            if (tipo.Nota != null)
                EditorGUILayout.LabelField(tipo.Nota, EditorStyles.wordWrappedMiniLabel);

            if (!pendentes.TryGetValue(tipo, out ConfiguracoesGlobais.Valores valores))
                valores = new ConfiguracoesGlobais.Valores();

            if (tipo.CampoPassos != null)
                valores.Passos = CampoInteiro(tipo.RotuloPassos, leitura.Passos, valores.Passos);
            valores.Tempo = CampoDecimal(tipo.RotuloTempo, leitura.Tempo, valores.Tempo);
            if (tipo.TemEsvaziar)
            {
                valores.PassosEsvaziar = CampoInteiro("Passos para esvaziar", leitura.PassosEsvaziar, valores.PassosEsvaziar);
                valores.SegundosEsvaziar = CampoDecimal("Tempo para esvaziar (s)", leitura.SegundosEsvaziar, valores.SegundosEsvaziar);
            }

            int passos = valores.Passos ?? Mathf.RoundToInt(leitura.Passos.Max);
            string aviso = ConfiguracoesGlobais.Aviso(tipo, focos, passos);
            if (aviso != null)
                EditorGUILayout.HelpBox(aviso, MessageType.Warning);

            bool temMudanca = valores.Passos.HasValue || valores.Tempo.HasValue ||
                              valores.PassosEsvaziar.HasValue || valores.SegundosEsvaziar.HasValue;
            if (temMudanca)
                pendentes[tipo] = valores;

            using (new EditorGUI.DisabledScope(!temMudanca))
            {
                if (GUILayout.Button($"Aplicar em {leitura.Quantidade}"))
                    Aplicar(tipo, valores);
            }
        }
        EditorGUILayout.EndVertical();
    }

    private void Aplicar(ConfiguracoesGlobais.TipoDeFoco tipo, ConfiguracoesGlobais.Valores valores)
    {
        int mudados = ConfiguracoesGlobais.Aplicar(tipo, focos, valores);
        pendentes.Remove(tipo);
        GUI.FocusControl(null);
        Debug.Log($"[Config] {tipo.Rotulo}: {mudados} focos alterados (cena {cena.name}, sem salvar).");
        Reler();
    }

    private static int? CampoInteiro(string rotulo, ConfiguracoesGlobais.Faixa atual, int? pendente)
    {
        EditorGUI.showMixedValue = !pendente.HasValue && atual.Misto;
        EditorGUI.BeginChangeCheck();
        int valor = EditorGUILayout.IntField(rotulo, pendente ?? Mathf.RoundToInt(atual.Min));
        bool mudou = EditorGUI.EndChangeCheck();
        EditorGUI.showMixedValue = false;
        return mudou ? valor : pendente;
    }

    private static float? CampoDecimal(string rotulo, ConfiguracoesGlobais.Faixa atual, float? pendente)
    {
        EditorGUI.showMixedValue = !pendente.HasValue && atual.Misto;
        EditorGUI.BeginChangeCheck();
        float valor = EditorGUILayout.FloatField(rotulo, pendente ?? atual.Min);
        bool mudou = EditorGUI.EndChangeCheck();
        EditorGUI.showMixedValue = false;
        return mudou ? valor : pendente;
    }

    /* ================= Aba Partida ================= */

    private void DesenharPartida()
    {
        float larguraDoRotulo = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = LarguraDoRotuloDaPartida;
        for (int i = 0; i < ConfiguracoesGlobais.Partida.Count; i++)
        {
            ConfiguracoesGlobais.GrupoDaPartida grupo = ConfiguracoesGlobais.Partida[i];
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(grupo.Titulo, EditorStyles.boldLabel);

            SerializedObject so = i < partida.Count ? partida[i] : null;
            if (so == null || so.targetObject == null)
            {
                EditorGUILayout.LabelField(grupo.Tipo.Name + " não está na cena.", EditorStyles.wordWrappedMiniLabel);
            }
            else
            {
                so.Update();
                foreach (string campo in grupo.Campos)
                    EditorGUILayout.PropertyField(so.FindProperty(campo));
                so.ApplyModifiedProperties();
            }
            EditorGUILayout.EndVertical();
        }
        EditorGUIUtility.labelWidth = larguraDoRotulo;
    }
}
