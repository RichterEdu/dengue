using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Liga os sons gerados (Ferramentas/Audio/gerar_sons.py) no jogo.
///
/// Menu: Dengue > Configurar Sons   (pode rodar de novo: cria só o que falta)
///
///   SampleScene
///     --- SISTEMAS ---/Sons   SomDosFocos, AmbienteSonoro, TrilhaSonora, SomDeInterface
///     Player                  SomDoJogador
///     CelularHUD              clipes do CelularController + CanalDeAudio (Efeitos)
///   MenuInicial (aberta por trás, salva e fechada)
///     Sons                    MusicaDeFundo (segue o fade do MenuInicial), SomDeInterface
///
/// Os clipes são religados toda vez (a pasta gerada é a fonte da verdade); volumes e
/// ajustes do Inspector ficam como estão. Na tabela de cliques dos focos, só entra
/// o tipo que ainda não tem linha.
///
/// LigarKit(SomDoKit) liga os sons do kit do agente (Parte A); fica fora do
/// Configurar e é chamado por quem monta o kit no Player. Do mesmo jeito,
/// LigarSonsDosFocosVivos(Scene) liga os sons da Parte B em quem existir na
/// cena, e é chamado pelo builder da cena.
/// </summary>
public static class SomBuilder
{
    const string Pasta = ImportadorDeSons.Pasta;
    const string CenaMenu = "Assets/Scenes/MenuInicial.unity";
    const string NomeSons = "Sons";

    [MenuItem("Dengue/Configurar Sons")]
    public static void Configurar()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogError("[Sons] Saia do Play Mode antes de configurar os sons.");
            return;
        }
        if (Clip("mosquito_zumbido_loop") == null)
        {
            Debug.LogError("[Sons] Nenhum som em " + Pasta + ". Gere com:  python -X utf8 Ferramentas/Audio/gerar_sons.py \"" + Pasta.TrimEnd('/') + "\"");
            return;
        }

        StringBuilder log = new StringBuilder();
        Scene cena = SceneManager.GetActiveScene();

        if (Object.FindAnyObjectByType<FluxoDeJogo>(FindObjectsInactive.Include) == null)
        {
            if (cena.path == CenaMenu)
            {
                MontarSomDoMenu(cena);
                Debug.Log("[Sons] Só o menu inicial foi configurado (é a cena aberta). Salve a cena. " +
                          "Para os sons da partida, abra a SampleScene e rode de novo.");
            }
            else
            {
                Debug.LogError("[Sons] Abra a SampleScene (a cena do jogo, com FluxoDeJogo) antes de configurar os sons.");
            }
            return;
        }

        /* ---------- Sons da partida ---------- */
        GameObject sistemas = GameObject.Find(HierarquiaOrganizador.Sistemas);
        Transform existente = sistemas != null ? sistemas.transform.Find(NomeSons) : null;
        GameObject sons = existente != null ? existente.gameObject : GameObject.Find(NomeSons);
        if (sons == null)
        {
            sons = new GameObject(NomeSons);
            Undo.RegisterCreatedObjectUndo(sons, "Configurar Sons");
            if (sistemas != null)
                Undo.SetTransformParent(sons.transform, sistemas.transform, false, "Configurar Sons");
            log.Append("\n  • Objeto ").Append(NomeSons).Append(sistemas != null ? " em " + HierarquiaOrganizador.Sistemas : " na raiz");
        }

        LigarSomDosFocos(Garantir<SomDosFocos>(sons, log));
        LigarAmbiente(Garantir<AmbienteSonoro>(sons, log));
        LigarTrilha(Garantir<TrilhaSonora>(sons, log));
        LigarInterface(Garantir<SomDeInterface>(sons, log));

        ControladorJogador jogador = Object.FindAnyObjectByType<ControladorJogador>(FindObjectsInactive.Include);
        if (jogador != null)
            LigarJogador(Garantir<SomDoJogador>(jogador.gameObject, log));
        else
            Debug.LogWarning("[Sons] Player (ControladorJogador) não encontrado: passos ficaram de fora.");

        CelularController celular = Object.FindAnyObjectByType<CelularController>(FindObjectsInactive.Include);
        if (celular != null)
            LigarSonsDoCelular(celular);
        else
            Debug.LogWarning("[Sons] CelularController não encontrado.");

        EditorSceneManager.MarkSceneDirty(cena);

        /* ---------- Menu inicial ---------- */
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(CenaMenu) != null)
        {
            Scene menu = SceneManager.GetSceneByPath(CenaMenu);
            bool abriu = !menu.isLoaded;
            if (abriu)
                menu = EditorSceneManager.OpenScene(CenaMenu, OpenSceneMode.Additive);
            MontarSomDoMenu(menu);
            EditorSceneManager.SaveScene(menu);
            if (abriu)
                EditorSceneManager.CloseScene(menu, true);
            log.Append("\n  • ").Append(CenaMenu).Append(": música e clique (salva)");
        }

        Selection.activeGameObject = sons;
        Debug.Log("[Sons] Concluído. Salve a cena." + log);
    }

    /* ================= Partida ================= */

    static void LigarSomDosFocos(SomDosFocos c)
    {
        SerializedObject so = new SerializedObject(c);
        Ref(so, "zumbido", Clip("mosquito_zumbido_loop"));
        Ref(so, "eliminado", Clip("foco_eliminado"));
        Ref(so, "ocupacaoDesce", Clip("ocupacao_desce"));

        AudioClip[] agua = Clips("foco_agua", 3);
        CompletarTipo(so, TipoFoco.Pneu, agua, null, 1f, 1f);
        CompletarTipo(so, TipoFoco.Vaso, Clips("foco_areia", 2), agua[0], 1f, 1f);
        CompletarTipo(so, TipoFoco.CaixaDagua, Clips("foco_tampa", 2), null, 1f, 1f);
        CompletarTipo(so, TipoFoco.Piscina, agua, null, 0.85f, 1f);
        CompletarTipo(so, TipoFoco.Garrafas, Clips("foco_garrafa", 3), null, 1f, 0.9f);
        CompletarTipo(so, TipoFoco.Ralo, Clips("foco_metal", 2), null, 1f, 0.85f);
        CompletarTipo(so, TipoFoco.Bromelia, agua, null, 1.25f, 0.7f);
        so.ApplyModifiedProperties();
    }

    /* Acha a linha do tipo (ou cria com pitch/volume padrão) e religa os clipes. */
    static void CompletarTipo(SerializedObject so, TipoFoco tipo, AudioClip[] cliques, AudioClip primeiro, float pitch, float volume)
    {
        SerializedProperty lista = so.FindProperty("sonsPorTipo");
        SerializedProperty linha = null;
        for (int i = 0; i < lista.arraySize; i++)
        {
            SerializedProperty el = lista.GetArrayElementAtIndex(i);
            if (el.FindPropertyRelative("tipo").intValue == (int)tipo)
            {
                linha = el;
                break;
            }
        }
        if (linha == null)
        {
            lista.arraySize++;
            linha = lista.GetArrayElementAtIndex(lista.arraySize - 1);
            linha.FindPropertyRelative("tipo").intValue = (int)tipo;
            linha.FindPropertyRelative("pitch").floatValue = pitch;
            linha.FindPropertyRelative("volume").floatValue = volume;
        }
        Lista(linha.FindPropertyRelative("cliques"), cliques);
        linha.FindPropertyRelative("primeiroClique").objectReferenceValue = primeiro;
    }

    static void LigarAmbiente(AmbienteSonoro c)
    {
        SerializedObject so = new SerializedObject(c);
        Ref(so, "loopHospital", Clip("ambiente_hospital_loop"));
        Ref(so, "loopBairro", Clip("ambiente_bairro_loop"));
        Lista(so.FindProperty("passaros.clips"), Clips("passaro", 3));
        Lista(so.FindProperty("latidos.clips"), Clips("latido", 2));
        so.ApplyModifiedProperties();
    }

    static void LigarTrilha(TrilhaSonora c)
    {
        SerializedObject so = new SerializedObject(c);
        Ref(so, "camadaBase", Clip("trilha_base"));
        Ref(so, "camadaPulso", Clip("trilha_pulso"));
        Ref(so, "camadaTensao", Clip("trilha_tensao"));
        Ref(so, "camadaEsperanca", Clip("trilha_esperanca"));
        Ref(so, "vitoria", Clip("vitoria"));
        Ref(so, "derrota", Clip("derrota"));
        Ref(so, "alerta", Clip("ocupacao_alerta"));
        so.ApplyModifiedProperties();
    }

    static void LigarInterface(SomDeInterface c)
    {
        SerializedObject so = new SerializedObject(c);
        Ref(so, "clique", Clip("ui_clique"));
        so.ApplyModifiedProperties();
    }

    static void LigarJogador(SomDoJogador c)
    {
        SerializedObject so = new SerializedObject(c);
        Lista(so.FindProperty("passosRua"), Clips("passo_rua", 4));
        Lista(so.FindProperty("passosPiso"), Clips("passo_piso", 4));
        Ref(so, "pulo", Clip("pulo"));
        Ref(so, "aterrissagem", Clip("aterrissagem"));
        so.ApplyModifiedProperties();
    }

    /* Sons do kit do agente (Parte A), religados pelo nome do arquivo. */
    public static void LigarKit(SomDoKit c)
    {
        SerializedObject so = new SerializedObject(c);
        Ref(so, "trocar", Clip("kit_trocar"));
        Ref(so, "erro", Clip("kit_erro"));
        Ref(so, "reabastecer", Clip("kit_reabastecer"));
        Ref(so, "despejar", Clip("cacamba_despejar"));
        Ref(so, "pneuPegar", Clip("pneu_pegar"));
        Ref(so, "pneuLargar", Clip("pneu_largar"));
        Ref(so, "segurarLoop", Clip("kit_segurar_loop"));
        so.ApplyModifiedProperties();
    }

    /* Sons dos focos vivos e da chuva (Parte B), religados pelo nome do
     * arquivo em quem existir na cena (quem falta é ignorado). */
    public static void LigarSonsDosFocosVivos(Scene cena)
    {
        LigarSonsDosFocosVivos(cena.GetRootGameObjects());
        EditorSceneManager.MarkSceneDirty(cena);
    }

    internal static void LigarSonsDosFocosVivos(GameObject[] raizes)
    {
        foreach (GameObject raiz in raizes)
        {
            LigarEmTodos<ChuvaVisual>(raiz, so =>
            {
                Ref(so, "trovao", Clip("trovao"));
                Ref(so, "chuvaLoop", Clip("chuva_loop"));
            });
            LigarEmTodos<SomDosFocos>(raiz, so => Ref(so, "esvaziar", Clip("esvaziar")));
            LigarEmTodos<Cacamba>(raiz, so =>
            {
                Ref(so, "somPneu", Clip("pneu_picape"));
                Ref(so, "somSuspensao", Clip("suspensao"));
            });
            LigarEmTodos<Lixeira>(raiz, so => Ref(so, "somTampa", Clip("lixeira_tampa")));
            LigarEmTodos<FeedbackDeCarga>(raiz, so =>
            {
                Lista(so.FindProperty("esforco"), Clips("esforco", 3));
                Ref(so, "sacoTilintar", Clip("saco_tilintar"));
            });
            LigarEmTodos<SomDoJogador>(raiz, so => Lista(so.FindProperty("passosPesados"), Clips("passo_pesado", 3)));
        }
    }

    /* Aplica 'ligar' em cada T da raiz e dos filhos, inativos também. */
    static void LigarEmTodos<T>(GameObject raiz, System.Action<SerializedObject> ligar) where T : Component
    {
        foreach (T c in raiz.GetComponentsInChildren<T>(true))
        {
            SerializedObject so = new SerializedObject(c);
            ligar(so);
            so.ApplyModifiedProperties();
        }
    }

    /* Também chamado pelo CelularBuilder: recriar o celular não perde os sons. */
    public static void LigarSonsDoCelular(CelularController celular)
    {
        SerializedObject so = new SerializedObject(celular);
        AudioSource fonte = so.FindProperty("audioSource").objectReferenceValue as AudioSource;
        if (fonte == null)
        {
            fonte = celular.GetComponent<AudioSource>();
            if (fonte == null)
                fonte = Undo.AddComponent<AudioSource>(celular.gameObject);
            so.FindProperty("audioSource").objectReferenceValue = fonte;
        }
        Ref(so, "somNotificacao", Clip("celular_notificacao"));
        Ref(so, "somAbrir", Clip("celular_abrir"));
        Ref(so, "somFechar", Clip("celular_fechar"));
        so.ApplyModifiedProperties();

        Undo.RecordObject(fonte, "Configurar Sons");
        fonte.playOnAwake = false;
        fonte.spatialBlend = 0f;

        CanalDeAudio canal = fonte.GetComponent<CanalDeAudio>();
        if (canal == null)
            canal = Undo.AddComponent<CanalDeAudio>(fonte.gameObject);
        SerializedObject soCanal = new SerializedObject(canal);
        soCanal.FindProperty("canal").intValue = (int)CanalSom.Efeitos;
        soCanal.ApplyModifiedProperties();
    }

    /* ================= Menu inicial ================= */

    /* Também chamado pelo MenuInicialBuilder, que refaz a cena do zero. */
    public static void MontarSomDoMenu(Scene cena)
    {
        GameObject sons = null;
        MenuInicial menu = null;
        foreach (GameObject raiz in cena.GetRootGameObjects())
        {
            if (raiz.name == NomeSons) sons = raiz;
            if (menu == null) menu = raiz.GetComponentInChildren<MenuInicial>(true);
        }
        if (sons == null)
        {
            sons = new GameObject(NomeSons);
            SceneManager.MoveGameObjectToScene(sons, cena);
            Undo.RegisterCreatedObjectUndo(sons, "Configurar Sons");
        }

        MusicaDeFundo musica = Garantir<MusicaDeFundo>(sons, null);
        SerializedObject so = new SerializedObject(musica);
        Ref(so, "musica", Clip("trilha_menu"));
        if (menu != null)
            Ref(so, "acompanharFade", new SerializedObject(menu).FindProperty("fade").objectReferenceValue);
        so.ApplyModifiedProperties();

        LigarInterface(Garantir<SomDeInterface>(sons, null));
        EditorSceneManager.MarkSceneDirty(cena);
    }

    /* ================= Utilidades ================= */

    static T Garantir<T>(GameObject go, StringBuilder log) where T : Component
    {
        T c = go.GetComponent<T>();
        if (c == null)
        {
            c = Undo.AddComponent<T>(go);
            log?.Append("\n  • ").Append(typeof(T).Name).Append(" em ").Append(go.name);
        }
        return c;
    }

    static AudioClip Clip(string nome)
    {
        return AssetDatabase.LoadAssetAtPath<AudioClip>(Pasta + nome + ".wav");
    }

    /* prefixo_1 .. prefixo_n */
    static AudioClip[] Clips(string prefixo, int quantidade)
    {
        List<AudioClip> clips = new List<AudioClip>();
        for (int i = 1; i <= quantidade; i++)
        {
            AudioClip c = Clip(prefixo + "_" + i);
            if (c != null) clips.Add(c);
            else Debug.LogWarning("[Sons] Falta " + Pasta + prefixo + "_" + i + ".wav");
        }
        return clips.ToArray();
    }

    static void Ref(SerializedObject so, string campo, Object valor)
    {
        SerializedProperty p = so.FindProperty(campo);
        if (p == null)
            Debug.LogError("[Sons] Campo '" + campo + "' não existe em " + so.targetObject.GetType().Name);
        else
            p.objectReferenceValue = valor;
    }

    static void Lista(SerializedProperty p, AudioClip[] clips)
    {
        if (p == null)
        {
            Debug.LogError("[Sons] Lista de clipes não encontrada.");
            return;
        }
        p.arraySize = clips.Length;
        for (int i = 0; i < clips.Length; i++)
            p.GetArrayElementAtIndex(i).objectReferenceValue = clips[i];
    }
}
