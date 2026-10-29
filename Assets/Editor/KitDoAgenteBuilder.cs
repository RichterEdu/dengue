using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/* ============================================================
 *  KitDoAgenteBuilder.cs  (Editor)
 *  ---------------------------------------------------------------------
 *  Monta na cena do jogo (SampleScene) tudo o que o kit do agente
 *  (Parte A) precisa para funcionar.
 *
 *  Menu:  Dengue > Configurar Kit do Agente
 *
 *  Pode rodar de novo: acha pelo nome o que já criou e só atualiza
 *  (pose, campos e textos). No fim salva a cena e os assets e loga um
 *  resumo [Kit] no Console.
 *
 *  A cena configurada é a do FluxoDeJogo achado: é nela que tudo é
 *  procurado, criado e salvo, mesmo com outra cena aberta junto e ativa
 *  (ex.: a MenuInicial ativa com a SampleScene aditiva).
 *
 *    1. Player (o objeto com ControladorJogador): KitDoAgente e SomDoKit
 *       (clipes pelo SomBuilder.LigarKit; o som de erro mais baixo, porque
 *       o kit_erro é bem mais "cheio" que os outros). PontoDeCarga, filho
 *       da Main Camera: onde o pneu fica nos braços, no canto inferior
 *       direito da tela, sem cobrir a mira.
 *    2. HUD: o objeto KitHUD, irmão da DengueHUD. Fica VAZIO (só o
 *       KitHUD), fora de qualquer Canvas: o KitHUD monta o próprio Canvas
 *       no Awake. A DengueHUD passa a mirar só "{nome}", porque a linha
 *       "segure o clique" já explica o que fazer. A barra de ferramentas
 *       do KitHUD fica no centro de baixo da tela: a barra de Ocupação
 *       Hospitalar (com a sombra dela) sobe para o centro de cima e o
 *       aviso (toast) desce para logo abaixo dela.
 *    3. Focos: a tabela Focos (ferramenta, segundos, passos e textos por
 *       tipo C#) vai para os prefabs de foco E para cada instância da
 *       cena, que pode ter override ou nem ser instância (pneus e garrafas
 *       não têm prefab). Cria o FocoHUDInfo onde falta. Garrafas: sempre
 *       um passo por garrafa (cliquesIgualAoNumeroDeGarrafas), para a
 *       última garrafa recolhida fechar o grupo.
 *    4. Picape da UPA (pacote Palmov), perto da saída do hospital no
 *       bairro: PontoDeReabastecimento de areia, tela e água sanitária,
 *       marcado como a picape (ehPicapeDaUpa: o post de reabastecer).
 *    5. Caçambas (5 cubos cada, pivô no chão), uma por grupo de pneus,
 *       nos lugares da tabela Cacambas: Cacamba. O pneu entregue é solto
 *       1,2 m acima do pivô, logo acima da borda (1,0 m). Cada lugar é
 *       conferido com OverlapBox (a caixa mais a folga em volta e acima);
 *       o resumo traz o pneu mais longe da caçamba mais perto.
 *    6. O "Monte de Areia 1" do terreno baldio mais perto dos vasos:
 *       PontoDeReabastecimento só de areia.
 *    7. O balão da Enf. Rita ganha a fala nova, que cita o kit e a picape
 *       (PersonagensHospitalBuilder.FalaEnfermeira).
 *    8. A 1ª fala da Lúcia ao eliminar uma bromélia (personagem salvo na
 *       HistoriaRedeSocial da cena) fala da água sanitária
 *       (HistoriaRedeSocial.FalaLuciaPrimeiraBromelia).
 *
 *  Os pontos de uso precisam de collider SÓLIDO (o Interactor ignora
 *  triggers). O destaque de mira usa _EmissionColor, que o URP Lit tem
 *  (picape, monte de areia e o material da caçamba).
 * ============================================================ */

public static class KitDoAgenteBuilder
{
    private const string NomeUndo = "Configurar Kit do Agente";

    private const string NomePontoDeCarga = "PontoDeCarga";
    private const string NomeKitHUD = "KitHUD";
    private const string NomePicape = "Picape da UPA";
    private const string NomeMonteDeAreia = "Monte de Areia 1";   // prefab do terreno baldio
    private const string QuemFalaRita = "Enf. Rita";
    private const string GrupoObjetosDeRua = "Objetos de Rua";     // dentro do --- BAIRRO ---

    private const string CaminhoPicape =
        "Assets/Palmov Island/Low Poly Atmospheric Locations Pack/Prefabs/Vehicles/car SUVs pickups lights red.prefab";
    private const string PastaCacamba = MeusOrganizador.Destino + "/Cacamba";
    private const string CaminhoMaterialCacamba = PastaCacamba + "/Cacamba.mat";

    // Pose do pneu nos braços, local na Main Camera: canto inferior direito
    // da tela, sem cobrir a mira (conferida no preview em Play Mode). O pneu
    // da cena tem ~2 m de diâmetro, com o eixo no X local: fica de pé, quase
    // de perfil (giro de 10° em Y) e com o topo um pouco para fora (−10° em
    // Z), e só o arco de baixo aparece no canto. Deitado na frente da câmera
    // (a pose inicial do plano) ele tapava a metade de baixo da tela.
    private static readonly Vector3 PosicaoDaCarga = new Vector3(0.80f, -1.00f, 1.10f);
    private static readonly Vector3 RotacaoDaCarga = new Vector3(0f, 10f, -10f);

    // Onde a picape fica (conferido de cima: nada atravessa casa nem muro):
    // sai da posição da saída do hospital no bairro, estacionada na rua,
    // rente à calçada do lado norte, de frente para o oeste (mão brasileira).
    private static readonly Vector3 OffsetPicape = new Vector3(4.0f, 0f, 0.85f);
    private const float GiroPicape = -90f;

    // As caçambas, uma por grupo de pneus: nome, lugar no chão (x, z; a
    // altura sai do raio até o chão) e onde fica, para o resumo. Os 6 pneus
    // estão espalhados pelas duas quadras (dois deles a ~91 m um do outro):
    // com uma caçamba só, a viagem carregando pneu (0,7× a velocidade, sem
    // correr) chegava a ~61 m. Com estas três, nenhum pneu fica a mais de
    // ~20 m da caçamba mais perto. Os lugares foram conferidos de cima e com
    // OverlapBox (caixa + FolgaCacamba): nada atravessa casa, muro, cerca
    // nem enfeite do terreno baldio, e sobra passagem em volta.
    //   - Cacamba: logo dentro da entrada do terreno baldio, ao sul do monte
    //     de areia (pneu, pneu (1) e pneu (2); vê-se da rua);
    //   - Cacamba (2): na grama a oeste da delegacia, entre a cerca do limite
    //     do mapa (FimDoMundo) e o prédio (pneu (3) e pneu (4));
    //   - Cacamba (3): no fundo do beco entre a oficina e o prédio laranja,
    //     logo depois do pneu (5).
    private static readonly (string nome, Vector3 lugar, string onde)[] Cacambas =
    {
        ("Cacamba",     new Vector3(-23.9f, 0f, 56.4f), "entrada do terreno baldio"),
        ("Cacamba (2)", new Vector3(-45.3f, 0f, -2.7f), "grama a oeste da delegacia"),
        ("Cacamba (3)", new Vector3(43.9f, 0f, 28.1f),  "fundo do beco do pneu (5)"),
    };

    // Layout da HUD com o kit (mesmos valores do HUDLayoutAjustador, menu
    // Dengue > Ajustar Layout da HUD, sem as janelas de diálogo dele, que
    // travariam este menu quando rodado pela linha de comando). Âncora e
    // pivô no centro de cima; a posição é a distância a partir do topo.
    private static readonly Vector2 PosicaoOcupacao = new Vector2(0f, -32f);
    private static readonly Vector2 TamanhoOcupacao = new Vector2(560f, 84f);
    private static readonly Vector2 PosicaoToast = new Vector2(0f, -126f);

    // Os raios que assentam no chão saem desta altura acima da referência.
    private const float AlturaDaBusca = 3f;
    private const float AlcanceDaBusca = 20f;

    // Caçamba: caixa aberta de 2,4 × 1,0 × 1,4 m, com o pivô no chão.
    private static readonly Vector3 TamanhoCacamba = new Vector3(2.4f, 1.0f, 1.4f);
    private const float EspessuraFundo = 0.1f;
    private const float EspessuraParede = 0.08f;
    private static readonly Color CorCacamba = new Color32(0xE8, 0xA2, 0x1F, 0xFF);

    // Ao conferir o lugar: folga em volta e acima da caixa, e o quanto ela
    // sobe do chão (um piso vizinho na mesma altura, como a rua, não é
    // obstáculo; um meio-fio já conta).
    private const float FolgaCacamba = 0.6f;
    private const float RespiroDoChao = 0.05f;

    private const float VolumeErro = 0.25f;

    // Prefabs dos focos que têm prefab (pneus e garrafas não têm). A
    // piscina tem duas cópias: a gerada e uma antiga na raiz de Assets.
    private static readonly string[] PrefabsDeFoco =
    {
        "Assets/Meus/Vaso de Flor/Vaso de Flor.prefab",
        "Assets/Meus/Caixa d'Agua.prefab",
        MeusOrganizador.Destino + "/Focos/Ralo.prefab",
        MeusOrganizador.Destino + "/Focos/Bromelia.prefab",
        MeusOrganizador.Destino + "/Piscina de Plastico/Piscina de Plastico.prefab",
        "Assets/Piscina de Plastico.prefab",
    };

    // Uma linha por tipo C# de foco. A mira mostra o problema; o texto de
    // ferramenta errada ensina a certa. Garrafas: 'segundos' é por garrafa.
    // 'passos' > 0 grava o requiredClicks (0 = cada foco mantém o seu). O
    // ralo tem 8 passos de 0,25 s: ao soltar, só a fração do passo em
    // andamento se perde, e com 3 passos de 0,67 s a barra voltava um
    // terço. 'dica' != null grava a dica educativa do FocoHUDInfo (null =
    // cada foco mantém a sua); a da bromélia ensina a água sanitária, como
    // o aviso de ferramenta errada.
    private static readonly (System.Type tipo, string rotulo, Ferramenta ferramenta, float segundos, int passos,
                             string textoAoMirar, string textoFerramentaErrada, string dica)[] Focos =
    {
        (typeof(PneuInteractable), "pneus", Ferramenta.Maos, 1.5f, 0,
            "Pneu velho com água parada",
            "Pneu não se resolve com isso. Esvazie e leve para a caçamba: use as <b>mãos</b>.",
            null),
        (typeof(VasoInteractable), "vasos", Ferramenta.Areia, 2.0f, 0,
            "Pratinho do vaso com água parada",
            "Isso não deixa o pratinho seco. Encha de <b>areia</b> até a borda.",
            null),
        (typeof(CaixaDaguaInteractable), "caixas", Ferramenta.Maos, 3.0f, 0,
            "Caixa d'água destampada",
            "Caixa d'água se protege com a tampa. Use as <b>mãos</b> para encaixar.",
            null),
        (typeof(PiscinaInteractable), "piscinas", Ferramenta.Maos, 2.5f, 0,
            "Piscina de plástico com água da chuva",
            "Esvazie e vire de boca para baixo. Use as <b>mãos</b>.",
            null),
        (typeof(GarrafaInteractable), "garrafas", Ferramenta.SacoDeLixo, 0.4f, 0,
            "Garrafas largadas juntando água",
            "Garrafa vai para o <b>saco de lixo</b>, e o saco para a caçamba.",
            null),
        (typeof(RaloInteractable), "ralos", Ferramenta.Tela, 2.0f, 8,
            "Ralo destampado com água parada",
            "Isso não resolve o ralo. Ralo se protege com <b>tela</b> e grelha fechada.",
            null),
        (typeof(BromeliaInteractable), "bromélias", Ferramenta.AguaSanitaria, 2.0f, 0,
            "Bromélia com água acumulada nas folhas",
            "Não precisa arrancar nem cobrir. Bromélia se trata com <b>água sanitária</b> diluída.",
            "Bromélia guarda água entre as folhas. Regue o copinho com água sanitária diluída " +
            "(1 colher de sopa por litro) duas vezes por semana."),
    };

    // A cena do jogo sendo configurada (a do FluxoDeJogo achado), só durante
    // o Configurar: tudo o que o builder procura, cria e salva fica nela.
    private static Scene cenaDoJogo;

    [MenuItem("Dengue/Configurar Kit do Agente")]
    public static void Configurar()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogError("[Kit] Saia do Play Mode antes de configurar o kit.");
            return;
        }
        FluxoDeJogo fluxo = Object.FindAnyObjectByType<FluxoDeJogo>(FindObjectsInactive.Include);
        if (fluxo == null)
        {
            Debug.LogError("[Kit] Abra a SampleScene (a cena do jogo, com FluxoDeJogo) antes de configurar o kit.");
            return;
        }

        // A cena do FluxoDeJogo, e não a ativa: com a MenuInicial ativa e a
        // SampleScene aditiva, a picape, as caçambas e a KitHUD iriam (e
        // seriam salvas) na MenuInicial.
        cenaDoJogo = fluxo.gameObject.scene;
        try
        {
            ControladorJogador jogador = PrimeiroDaCena<ControladorJogador>();
            if (jogador == null)
            {
                Debug.LogError("[Kit] Player (ControladorJogador) não encontrado na cena " + cenaDoJogo.name +
                               ". Rode antes Dengue > Configurar Player.");
                return;
            }

            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName(NomeUndo);
            int grupoUndo = Undo.GetCurrentGroup();

            StringBuilder log = new StringBuilder();
            ConfigurarPlayer(jogador, log);
            ConfigurarHUD(log);
            ConfigurarFocos(log);
            MontarPicape(log);
            MontarCacambas(log);
            MontarMonteDeAreia(log);
            ConfigurarRita(log);
            ConfigurarLucia(log);

            Undo.CollapseUndoOperations(grupoUndo);

            EditorSceneManager.MarkSceneDirty(cenaDoJogo);
            EditorSceneManager.SaveScene(cenaDoJogo);
            AssetDatabase.SaveAssets();

            Debug.Log("[Kit] Concluído (cena " + cenaDoJogo.name + " salva)." + log);
        }
        finally
        {
            cenaDoJogo = default;
        }
    }

    /* ================= 1. Player ================= */

    private static void ConfigurarPlayer(ControladorJogador jogador, StringBuilder log)
    {
        GameObject player = jogador.gameObject;
        KitDoAgente kit = Garantir<KitDoAgente>(player);

        SomDoKit som = Garantir<SomDoKit>(player);
        SomBuilder.LigarKit(som);
        SerializedObject soSom = new SerializedObject(som);
        soSom.FindProperty("volumeErro").floatValue = VolumeErro;
        soSom.ApplyModifiedProperties();

        // A câmera é livre (não é filha do player): a CameraJogador a acha.
        // A Camera.main só vale se for da cena do jogo (a MenuInicial tem a dela).
        CameraJogador cameraJogador = PrimeiroDaCena<CameraJogador>();
        Camera principal = Camera.main;
        Transform camera = cameraJogador != null ? cameraJogador.transform
                         : principal != null && principal.gameObject.scene == cenaDoJogo ? principal.transform : null;
        if (camera == null)
        {
            Debug.LogWarning("[Kit] Main Camera não encontrada: o pneu carregado vai ficar preso ao player.");
            log.Append("\n  • Player: KitDoAgente e SomDoKit (sem PontoDeCarga: falta a Main Camera)");
            return;
        }

        Transform ponto = camera.Find(NomePontoDeCarga);
        if (ponto == null)
        {
            ponto = NovoNaCena(NomePontoDeCarga).transform;
            Undo.SetTransformParent(ponto, camera, false, NomeUndo);
        }
        Undo.RecordObject(ponto, NomeUndo);
        ponto.localPosition = PosicaoDaCarga;
        ponto.localEulerAngles = RotacaoDaCarga;
        ponto.localScale = Vector3.one;

        SerializedObject soKit = new SerializedObject(kit);
        soKit.FindProperty("pontoDeCarga").objectReferenceValue = ponto;
        soKit.ApplyModifiedProperties();

        log.Append("\n  • Player: KitDoAgente, SomDoKit (volumeErro ").Append(VolumeErro.ToString("0.00"))
           .Append(") e ").Append(camera.name).Append("/").Append(NomePontoDeCarga)
           .Append(" em ").Append(PosicaoDaCarga.ToString("F2")).Append(", giro ").Append(RotacaoDaCarga.ToString("F0"));
    }

    /* ================= 2. HUD ================= */

    private static void ConfigurarHUD(StringBuilder log)
    {
        DengueHUD hud = PrimeiroDaCena<DengueHUD>();

        // Irmão da DengueHUD, desde que o pai não seja (nem esteja dentro de)
        // um Canvas: o KitHUD.Awake adiciona o próprio Canvas e quebraria.
        Transform pai = hud != null ? hud.transform.parent : null;
        if (pai != null && (pai is RectTransform || pai.GetComponentInParent<Canvas>(true) != null))
            pai = null;

        GameObject kitHud = AcharNaCena(NomeKitHUD);
        if (kitHud == null)
        {
            kitHud = NovoNaCena(NomeKitHUD);
            if (pai != null)
            {
                Undo.SetTransformParent(kitHud.transform, pai, false, NomeUndo);
                kitHud.transform.SetSiblingIndex(hud.transform.GetSiblingIndex() + 1);
            }
        }
        Garantir<KitHUD>(kitHud);

        string layout = "";
        if (hud != null)
        {
            SerializedObject so = new SerializedObject(hud);
            so.FindProperty("textoAoMirar").stringValue = "{nome}";
            so.ApplyModifiedProperties();
            layout = AjustarLayoutDaHUD(hud.transform);
        }
        else
        {
            Debug.LogWarning("[Kit] DengueHUD não encontrada: o KitHUD ficou na raiz e o texto de mira não mudou.");
        }

        log.Append("\n  • HUD: ").Append(NomeKitHUD).Append(" em ")
           .Append(kitHud.transform.parent != null ? kitHud.transform.parent.name : "(raiz)")
           .Append("; DengueHUD.textoAoMirar = \"{nome}\"").Append(layout);
    }

    /* A barra de ferramentas do KitHUD (e a linha "Carregando pneu" acima
     * dela) ocupa o centro de baixo da tela, onde a DengueHUD antiga punha
     * a barra de Ocupação Hospitalar; o Canvas do KitHUD desenha por cima.
     * A ocupação sobe para o centro de cima, levando junto a sombra irmã
     * dela (Image de builders antigos, a caixa do painel com uma margem em
     * volta), e o toast desce para logo abaixo. Devolve o trecho do resumo. */
    private static string AjustarLayoutDaHUD(Transform raizHud)
    {
        RectTransform ocupacao = raizHud.Find("OcupacaoPanel") as RectTransform;
        RectTransform sombra = raizHud.Find("OcupacaoPanelSombra") as RectTransform;
        RectTransform toast = raizHud.Find("ToastPanel") as RectTransform;

        StringBuilder log = new StringBuilder();
        if (ocupacao != null)
        {
            // A margem sai do tamanho atual, antes de mexer no painel.
            Vector2 margem = sombra != null ? (sombra.sizeDelta - ocupacao.sizeDelta) / 2f : Vector2.zero;
            NoCentroDeCima(ocupacao, PosicaoOcupacao, TamanhoOcupacao);
            if (sombra != null)
                NoCentroDeCima(sombra, PosicaoOcupacao + new Vector2(0f, margem.y), TamanhoOcupacao + 2f * margem);
            log.Append("; Ocupação").Append(sombra != null ? " (e a sombra)" : "")
               .Append(" no centro de cima, y ").Append(PosicaoOcupacao.y.ToString("0"));
        }
        else
        {
            Debug.LogWarning("[Kit] OcupacaoPanel não encontrado na DengueHUD: confira se a barra de ferramentas não cobre nada.");
        }
        if (toast != null)
        {
            NoCentroDeCima(toast, PosicaoToast, toast.sizeDelta);   // a altura vem do ContentSizeFitter
            log.Append("; toast em y ").Append(PosicaoToast.y.ToString("0"));
        }
        return log.ToString();
    }

    /* Âncoras e pivô no centro de cima; posição a partir do topo. */
    private static void NoCentroDeCima(RectTransform rt, Vector2 posicao, Vector2 tamanho)
    {
        Undo.RecordObject(rt, NomeUndo);
        Vector2 centroDeCima = new Vector2(0.5f, 1f);
        rt.anchorMin = centroDeCima;
        rt.anchorMax = centroDeCima;
        rt.pivot = centroDeCima;
        rt.anchoredPosition = posicao;
        rt.sizeDelta = tamanho;
    }

    /* ================= 3. Focos ================= */

    private static void ConfigurarFocos(StringBuilder log)
    {
        // Prefabs primeiro: as instâncias da cena já recebem o FocoHUDInfo
        // novo do prefab, e o passo da cena não cria um segundo.
        int prefabs = 0;
        foreach (string caminho in PrefabsDeFoco)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(caminho) == null)
            {
                Debug.LogWarning("[Kit] Prefab de foco não encontrado: " + caminho);
                continue;
            }

            GameObject raiz = PrefabUtility.LoadPrefabContents(caminho);
            try
            {
                bool mudou = false;
                foreach (Interactable foco in raiz.GetComponentsInChildren<Interactable>(true))
                    mudou |= AplicarFoco(foco, false);
                if (mudou)
                    PrefabUtility.SaveAsPrefabAsset(raiz, caminho);
                prefabs++;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(raiz);
            }
        }

        // Cada instância da cena, por tipo C#.
        int[] porTipo = new int[Focos.Length];
        int total = 0;
        foreach (Interactable foco in DaCena<Interactable>())
        {
            int linha = LinhaDo(foco);
            if (linha < 0)
            {
                Debug.LogWarning("[Kit] Foco sem linha na tabela (" + foco.GetType().Name + "): " + foco.name);
                continue;
            }
            AplicarFoco(foco, true);
            porTipo[linha]++;
            total++;
        }

        log.Append("\n  • Focos: ").Append(total).Append(" configurados na cena (");
        for (int i = 0; i < Focos.Length; i++)
            log.Append(i > 0 ? ", " : "").Append(porTipo[i]).Append(' ').Append(Focos[i].rotulo);
        log.Append(") e ").Append(prefabs).Append(" prefabs; garrafas com um passo por garrafa");
        foreach (var dados in Focos)
        {
            if (dados.passos > 0)
                log.Append("; ").Append(dados.rotulo).Append(" com ").Append(dados.passos).Append(" passos");
            if (dados.dica != null)
                log.Append("; dica nova nas ").Append(dados.rotulo);
        }
    }

    /* Índice da linha da tabela para o tipo exato do foco (-1 = sem linha). */
    private static int LinhaDo(Interactable foco)
    {
        for (int i = 0; i < Focos.Length; i++)
            if (foco.GetType() == Focos[i].tipo)
                return i;
        return -1;
    }

    /* Grava a linha da tabela no foco e no FocoHUDInfo dele (cria se faltar).
     * comUndo: true na cena; false no conteúdo de prefab carregado.
     * Devolve true se algo mudou. */
    private static bool AplicarFoco(Interactable foco, bool comUndo)
    {
        int linha = LinhaDo(foco);
        if (linha < 0)
            return false;
        var dados = Focos[linha];

        SerializedObject so = new SerializedObject(foco);
        so.FindProperty("ferramentaNecessaria").intValue = (int)dados.ferramenta;
        // Garrafas: o tempo é por garrafa (o total depende de quantas há).
        string campoTempo = foco is GarrafaInteractable ? "segundosPorGarrafa" : "segundosParaConcluir";
        so.FindProperty(campoTempo).floatValue = dados.segundos;
        if (dados.passos > 0)
            so.FindProperty("requiredClicks").intValue = dados.passos;
        // Garrafas: um passo por garrafa, sempre. Com o ajuste desligado, a
        // 'Lata 001' concluía em 11 passos e deixava 1 das 12 garrafas na
        // cena, fora do alcance da mira (o raio atravessa foco concluído).
        if (foco is GarrafaInteractable)
            so.FindProperty("cliquesIgualAoNumeroDeGarrafas").boolValue = true;
        bool mudou = Aplicar(so, comUndo);

        FocoHUDInfo info = foco.GetComponent<FocoHUDInfo>();
        if (info == null)
        {
            info = comUndo ? Undo.AddComponent<FocoHUDInfo>(foco.gameObject)
                           : foco.gameObject.AddComponent<FocoHUDInfo>();
            mudou = true;
        }
        SerializedObject soInfo = new SerializedObject(info);
        soInfo.FindProperty("textoAoMirar").stringValue = dados.textoAoMirar;
        soInfo.FindProperty("textoFerramentaErrada").stringValue = dados.textoFerramentaErrada;
        if (dados.dica != null)
            soInfo.FindProperty("dica").stringValue = dados.dica;
        mudou |= Aplicar(soInfo, comUndo);
        return mudou;
    }

    private static bool Aplicar(SerializedObject so, bool comUndo)
    {
        return comUndo ? so.ApplyModifiedProperties() : so.ApplyModifiedPropertiesWithoutUndo();
    }

    /* ================= 4. Picape da UPA ================= */

    private static void MontarPicape(StringBuilder log)
    {
        SaidaDoHospital saida = PrimeiroDaCena<SaidaDoHospital>();
        if (saida == null)
        {
            Debug.LogWarning("[Kit] SaidaDoHospital não encontrada: a picape da UPA ficou de fora.");
            log.Append("\n  • ").Append(NomePicape).Append(": FALTOU (sem SaidaDoHospital)");
            return;
        }
        Vector3 saidaNoBairro = new SerializedObject(saida).FindProperty("posicaoNoBairro").vector3Value;

        GameObject picape = AcharNaCena(NomePicape);
        if (picape == null)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CaminhoPicape);
            if (prefab == null)
            {
                Debug.LogWarning("[Kit] Prefab da picape não encontrado: " + CaminhoPicape);
                log.Append("\n  • ").Append(NomePicape).Append(": FALTOU (prefab do pacote Palmov)");
                return;
            }
            picape = (GameObject)PrefabUtility.InstantiatePrefab(prefab, cenaDoJogo);
            picape.name = NomePicape;
            Undo.RegisterCreatedObjectUndo(picape, NomeUndo);
            PorNoGrupoDeRua(picape);
        }

        Undo.RecordObject(picape.transform, NomeUndo);
        picape.transform.rotation = Quaternion.Euler(0f, GiroPicape, 0f);
        string chao = AssentarNoChao(picape, saidaNoBairro + OffsetPicape);

        if (!TemColliderSolido(picape))
        {
            Bounds limites = LimitesLocais(picape);
            BoxCollider box = Undo.AddComponent<BoxCollider>(picape);
            box.center = limites.center;
            box.size = limites.size;
        }

        PontoDeReabastecimento ponto = Garantir<PontoDeReabastecimento>(picape);
        ConfigurarReabastecimento(ponto, "Picape da UPA: material para o kit", 1f, true,
                                  Ferramenta.Areia, Ferramenta.Tela, Ferramenta.AguaSanitaria);

        log.Append("\n  • ").Append(NomePicape).Append(": OK em ").Append(picape.transform.position.ToString("F2"))
           .Append(" (chão: ").Append(chao).Append("), reabastece areia, tela e água sanitária; ehPicapeDaUpa (post de reabastecer)");
    }

    /* ================= 5. Caçambas ================= */

    private static void MontarCacambas(StringBuilder log)
    {
        Material material = MaterialDaCacamba();
        List<GameObject> montadas = new List<GameObject>();
        foreach (var dados in Cacambas)
        {
            GameObject cacamba = MontarCacamba(dados.nome, dados.lugar, material, out string chao, out string espaco);
            montadas.Add(cacamba);
            log.Append("\n  • ").Append(dados.nome).Append(" (").Append(dados.onde).Append("): OK em ")
               .Append(cacamba.transform.position.ToString("F2")).Append(" (chão: ").Append(chao)
               .Append("; ").Append(espaco).Append(')');
        }

        // A viagem carregando pneu (0,7× a velocidade, sem correr): de cada
        // pneu até a caçamba mais perto; o resumo traz a mais longa.
        List<PneuInteractable> pneus = DaCena<PneuInteractable>();
        if (pneus.Count == 0)
        {
            Debug.LogWarning("[Kit] Nenhum PneuInteractable na cena: confira se as caçambas ainda fazem sentido.");
            log.Append("\n  • Pneus: nenhum na cena");
            return;
        }
        float maisLonge = 0f;
        string qual = "";
        foreach (PneuInteractable pneu in pneus)
        {
            float distancia = float.MaxValue;
            string cacambaMaisPerto = "";
            foreach (GameObject cacamba in montadas)
            {
                float d = DistanciaNoChao(pneu.transform.position, cacamba.transform.position);
                if (d < distancia)
                {
                    distancia = d;
                    cacambaMaisPerto = cacamba.name;
                }
            }
            if (distancia > maisLonge)
            {
                maisLonge = distancia;
                qual = pneu.name + " → " + cacambaMaisPerto;
            }
        }
        log.Append("\n  • Pneus: ").Append(pneus.Count).Append(", o mais longe da caçamba mais perto a ")
           .Append(maisLonge.ToString("0.0")).Append(" m (").Append(qual).Append(')');
    }

    /* Uma caçamba (acha pelo nome ou cria) no lugar dado, com o pivô no chão.
     * Devolve o nome do chão achado e o trecho do resumo sobre o espaço. */
    private static GameObject MontarCacamba(string nome, Vector3 lugar, Material material,
                                            out string chao, out string espaco)
    {
        GameObject cacamba = AcharNaCena(nome);
        if (cacamba == null)
        {
            cacamba = NovoNaCena(nome);
            PorNoGrupoDeRua(cacamba);
        }

        // Fundo e quatro paredes; as laterais cabem entre a frente e o fundo.
        float largura = TamanhoCacamba.x, altura = TamanhoCacamba.y, profundidade = TamanhoCacamba.z;
        float e = EspessuraParede;
        Peca(cacamba, "Fundo", new Vector3(0f, EspessuraFundo / 2f, 0f),
             new Vector3(largura, EspessuraFundo, profundidade), material);
        Peca(cacamba, "Parede Frente", new Vector3(0f, altura / 2f, profundidade / 2f - e / 2f),
             new Vector3(largura, altura, e), material);
        Peca(cacamba, "Parede Tras", new Vector3(0f, altura / 2f, -(profundidade / 2f - e / 2f)),
             new Vector3(largura, altura, e), material);
        Peca(cacamba, "Parede Esquerda", new Vector3(-(largura / 2f - e / 2f), altura / 2f, 0f),
             new Vector3(e, altura, profundidade - 2f * e), material);
        Peca(cacamba, "Parede Direita", new Vector3(largura / 2f - e / 2f, altura / 2f, 0f),
             new Vector3(e, altura, profundidade - 2f * e), material);

        // Um collider só, na raiz (os cubos nascem sem): é o que a mira acha.
        BoxCollider box = Garantir<BoxCollider>(cacamba);
        Undo.RecordObject(box, NomeUndo);
        box.isTrigger = false;
        box.center = new Vector3(0f, altura / 2f, 0f);
        box.size = TamanhoCacamba;

        // Sem giro: a caixa conferida no OverlapBox fica alinhada aos eixos.
        Undo.RecordObject(cacamba.transform, NomeUndo);
        cacamba.transform.rotation = Quaternion.identity;
        chao = AssentarNoChao(cacamba, lugar, out Collider colliderDoChao);
        espaco = ConferirEspaco(cacamba, colliderDoChao);

        Cacamba ponto = Garantir<Cacamba>(cacamba);
        SerializedObject so = new SerializedObject(ponto);
        so.FindProperty("textoAoMirar").stringValue = "Caçamba de entulho";
        so.FindProperty("segundosParaUsar").floatValue = 1f;
        so.ApplyModifiedProperties();
        return cacamba;
    }

    /* Confere que nada atravessa a caçamba nem encosta nela: OverlapBox da
     * caixa com FolgaCacamba em volta e acima, sem triggers, sem a própria
     * caçamba e sem o chão em que ela assentou. Devolve o trecho do resumo
     * (e avisa no Console se bateu em algo). */
    private static string ConferirEspaco(GameObject cacamba, Collider chao)
    {
        Vector3 meiaCaixa = new Vector3(TamanhoCacamba.x / 2f + FolgaCacamba,
                                        (TamanhoCacamba.y + FolgaCacamba) / 2f,
                                        TamanhoCacamba.z / 2f + FolgaCacamba);
        Vector3 centro = cacamba.transform.position + Vector3.up * (RespiroDoChao + meiaCaixa.y);

        List<string> batidas = new List<string>();
        foreach (Collider c in Physics.OverlapBox(centro, meiaCaixa, cacamba.transform.rotation, ~0,
                                                  QueryTriggerInteraction.Ignore))
        {
            if (c != chao && !c.transform.IsChildOf(cacamba.transform))
                batidas.Add(c.name);
        }

        if (batidas.Count == 0)
            return "livre, com " + FolgaCacamba.ToString("0.0") + " m de folga";

        string lista = string.Join(", ", batidas);
        Debug.LogWarning("[Kit] " + cacamba.name + " encosta em: " + lista + ". Escolha outro lugar na tabela Cacambas.");
        return "ENCOSTA em " + lista;
    }

    /* Um cubo da caçamba (acha pelo nome ou cria, sem collider próprio). */
    private static void Peca(GameObject raiz, string nome, Vector3 centro, Vector3 tamanho, Material material)
    {
        Transform peca = raiz.transform.Find(nome);
        if (peca == null)
        {
            GameObject cubo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cubo.name = nome;
            Object.DestroyImmediate(cubo.GetComponent<Collider>());
            Undo.RegisterCreatedObjectUndo(cubo, NomeUndo);
            Undo.SetTransformParent(cubo.transform, raiz.transform, false, NomeUndo);
            peca = cubo.transform;
        }
        Undo.RecordObject(peca, NomeUndo);
        peca.localPosition = centro;
        peca.localRotation = Quaternion.identity;
        peca.localScale = tamanho;

        MeshRenderer rend = peca.GetComponent<MeshRenderer>();
        Undo.RecordObject(rend, NomeUndo);
        rend.sharedMaterial = material;
    }

    /* Cacamba.mat (URP Lit, amarelo de caçamba), criado ou atualizado. */
    private static Material MaterialDaCacamba()
    {
        MeusOrganizador.GarantirPasta(PastaCacamba);
        Shader lit = Shader.Find("Universal Render Pipeline/Lit");
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(CaminhoMaterialCacamba);
        if (mat == null)
        {
            mat = new Material(lit);
            AssetDatabase.CreateAsset(mat, CaminhoMaterialCacamba);
        }
        mat.shader = lit;
        mat.SetColor("_BaseColor", CorCacamba);
        mat.SetColor("_Color", CorCacamba);
        mat.SetFloat("_Smoothness", 0.3f);
        mat.SetFloat("_Metallic", 0f);
        EditorUtility.SetDirty(mat);
        return mat;
    }

    /* ================= 6. Monte de areia ================= */

    private static void MontarMonteDeAreia(StringBuilder log)
    {
        List<VasoInteractable> vasos = DaCena<VasoInteractable>();
        Vector3 centroide = Vector3.zero;
        foreach (VasoInteractable vaso in vasos)
            centroide += vaso.transform.position;
        if (vasos.Count > 0)
            centroide /= vasos.Count;

        // As instâncias do prefab "Monte de Areia 1" (na cena viram
        // "Monte de Areia 001"...): vale a mais perto dos vasos.
        GameObject monte = null;
        float melhor = float.MaxValue;
        foreach (Transform t in DaCena<Transform>())
        {
            GameObject origem = PrefabUtility.IsAnyPrefabInstanceRoot(t.gameObject)
                ? PrefabUtility.GetCorrespondingObjectFromSource(t.gameObject)
                : null;
            bool ehMonte = t.name == NomeMonteDeAreia || (origem != null && origem.name == NomeMonteDeAreia);
            if (!ehMonte)
                continue;

            float distancia = DistanciaNoChao(t.position, centroide);
            if (distancia < melhor)
            {
                melhor = distancia;
                monte = t.gameObject;
            }
        }
        if (monte == null)
        {
            Debug.LogWarning("[Kit] Nenhum \"" + NomeMonteDeAreia + "\" na cena: a areia só reabastece na picape.");
            log.Append("\n  • Monte de areia: FALTOU (sem \"").Append(NomeMonteDeAreia).Append("\" na cena)");
            return;
        }

        if (!TemColliderSolido(monte))
            Undo.AddComponent<MeshCollider>(monte);

        PontoDeReabastecimento ponto = Garantir<PontoDeReabastecimento>(monte);
        ConfigurarReabastecimento(ponto, "Monte de areia: encher o balde de areia", 1.5f, false, Ferramenta.Areia);

        log.Append("\n  • Monte de areia: OK em ").Append(monte.name).Append(' ')
           .Append(monte.transform.position.ToString("F2")).Append(", a ").Append(melhor.ToString("0.0"))
           .Append(" m do centro dos vasos; reabastece só a areia");
    }

    /* ================= 7. Rita ================= */

    private static void ConfigurarRita(StringBuilder log)
    {
        int baloes = 0;
        foreach (BalaoDeFala balao in DaCena<BalaoDeFala>())
        {
            if (balao.quemFala != QuemFalaRita)
                continue;
            SerializedObject so = new SerializedObject(balao);
            so.FindProperty("texto").stringValue = PersonagensHospitalBuilder.FalaEnfermeira;
            so.ApplyModifiedProperties();
            baloes++;
        }
        if (baloes == 0)
            Debug.LogWarning("[Kit] Balão da " + QuemFalaRita + " não encontrado: a fala do kit ficou de fora.");
        log.Append("\n  • ").Append(QuemFalaRita).Append(": fala nova em ").Append(baloes).Append(" balão(ões)");
    }

    /* ================= 8. Lúcia (bromélias) ================= */

    /* A bromélia é tratada com água sanitária diluída: a 1ª fala da Lúcia
     * ao eliminar uma bromélia diz isso. Os personagens estão salvos na
     * HistoriaRedeSocial da cena, e o padrão do código só vale numa lista
     * vazia: a fala é gravada no primeiro personagem do tipo Bromelia (o
     * mesmo que posta). */
    private static void ConfigurarLucia(StringBuilder log)
    {
        HistoriaRedeSocial historia = PrimeiroDaCena<HistoriaRedeSocial>();
        if (historia == null)
        {
            Debug.LogWarning("[Kit] HistoriaRedeSocial não encontrada: a fala da bromélia ficou de fora.");
            log.Append("\n  • Lúcia: FALTOU (sem HistoriaRedeSocial)");
            return;
        }

        SerializedObject so = new SerializedObject(historia);
        SerializedProperty personagens = so.FindProperty("personagens");
        for (int i = 0; i < personagens.arraySize; i++)
        {
            SerializedProperty personagem = personagens.GetArrayElementAtIndex(i);
            if (personagem.FindPropertyRelative("tipo").intValue != (int)TipoFoco.Bromelia)
                continue;

            SerializedProperty falas = personagem.FindPropertyRelative("aoEliminar");
            if (falas.arraySize == 0)
                falas.arraySize = 1;
            falas.GetArrayElementAtIndex(0).stringValue = HistoriaRedeSocial.FalaLuciaPrimeiraBromelia;
            so.ApplyModifiedProperties();

            log.Append("\n  • ").Append(personagem.FindPropertyRelative("autor").stringValue)
               .Append(": 1ª fala ao eliminar bromélia com água sanitária");
            return;
        }

        // Lista vazia ou sem o tipo: o Awake da HistoriaRedeSocial usa o padrão do código.
        log.Append("\n  • Lúcia: nenhum personagem de bromélia salvo na cena (vale o padrão do código)");
    }

    /* ================= Utilidades ================= */

    private static T Garantir<T>(GameObject go) where T : Component
    {
        T c = go.GetComponent<T>();
        return c != null ? c : Undo.AddComponent<T>(go);
    }

    /* Componentes T da cena do jogo (inclusive inativos). Os de outras cenas
     * abertas ficam de fora. */
    private static List<T> DaCena<T>() where T : Component
    {
        List<T> achados = new List<T>();
        foreach (T c in Object.FindObjectsByType<T>(FindObjectsInactive.Include))
            if (c.gameObject.scene == cenaDoJogo)
                achados.Add(c);
        return achados;
    }

    /* O primeiro componente T da cena do jogo (null se não houver). */
    private static T PrimeiroDaCena<T>() where T : Component
    {
        List<T> achados = DaCena<T>();
        return achados.Count > 0 ? achados[0] : null;
    }

    /* Primeiro objeto da cena do jogo com este nome (inclusive inativo). */
    private static GameObject AcharNaCena(string nome)
    {
        foreach (GameObject raiz in cenaDoJogo.GetRootGameObjects())
            foreach (Transform t in raiz.GetComponentsInChildren<Transform>(true))
                if (t.name == nome)
                    return t.gameObject;
        return null;
    }

    /* GameObject novo na raiz da cena do jogo (e não na cena ativa, se for
     * outra), com Undo. */
    private static GameObject NovoNaCena(string nome)
    {
        GameObject go = new GameObject(nome);
        if (go.scene != cenaDoJogo)
            SceneManager.MoveGameObjectToScene(go, cenaDoJogo);
        Undo.RegisterCreatedObjectUndo(go, NomeUndo);
        return go;
    }

    /* Objeto novo vai para --- BAIRRO ---/Objetos de Rua (se existir). */
    private static void PorNoGrupoDeRua(GameObject go)
    {
        GameObject bairro = AcharNaCena(HierarquiaOrganizador.Bairro);
        Transform grupo = bairro != null ? bairro.transform.Find(GrupoObjetosDeRua) : null;
        if (grupo != null)
            Undo.SetTransformParent(go.transform, grupo, true, NomeUndo);
    }

    /* Põe o objeto em 'alvo' (x, z) com a base dos Renderers no primeiro
     * chão abaixo (raio de cima para baixo, sem triggers, sem o próprio
     * objeto). Devolve o nome do collider achado, para o resumo. */
    private static string AssentarNoChao(GameObject go, Vector3 alvo)
    {
        return AssentarNoChao(go, alvo, out _);
    }

    /* O mesmo, devolvendo também o collider do chão (null se nenhum). */
    private static string AssentarNoChao(GameObject go, Vector3 alvo, out Collider colliderDoChao)
    {
        colliderDoChao = null;
        Undo.RecordObject(go.transform, NomeUndo);
        go.transform.position = alvo;
        Physics.SyncTransforms();

        float yDoChao = alvo.y;
        string chao = "nenhum: ficou na altura de referência";
        RaycastHit[] acertos = Physics.RaycastAll(alvo + Vector3.up * AlturaDaBusca, Vector3.down,
                                                  AlcanceDaBusca, ~0, QueryTriggerInteraction.Ignore);
        System.Array.Sort(acertos, (a, b) => a.distance.CompareTo(b.distance));
        foreach (RaycastHit acerto in acertos)
        {
            if (acerto.collider.transform.IsChildOf(go.transform))
                continue;
            yDoChao = acerto.point.y;
            chao = acerto.collider.name;
            colliderDoChao = acerto.collider;
            break;
        }

        float baseAtual = LimitesNoMundo(go).min.y;
        go.transform.position += Vector3.up * (yDoChao - baseAtual);
        Physics.SyncTransforms();
        return chao;
    }

    private static bool TemColliderSolido(GameObject go)
    {
        foreach (Collider c in go.GetComponentsInChildren<Collider>(true))
            if (!c.isTrigger)
                return true;
        return false;
    }

    /* Caixa dos Renderers no mundo (vazia: só a posição do objeto). */
    private static Bounds LimitesNoMundo(GameObject go)
    {
        Renderer[] rends = go.GetComponentsInChildren<Renderer>();
        if (rends.Length == 0)
            return new Bounds(go.transform.position, Vector3.zero);
        Bounds b = rends[0].bounds;
        for (int i = 1; i < rends.Length; i++)
            b.Encapsulate(rends[i].bounds);
        return b;
    }

    /* Caixa dos Renderers no espaço local do objeto (para um BoxCollider na raiz). */
    private static Bounds LimitesLocais(GameObject go)
    {
        bool primeiro = true;
        Bounds b = new Bounds(Vector3.zero, Vector3.zero);
        foreach (Renderer rend in go.GetComponentsInChildren<Renderer>())
        {
            Bounds local = rend.localBounds;
            for (int i = 0; i < 8; i++)
            {
                Vector3 canto = local.center + Vector3.Scale(local.extents,
                    new Vector3((i & 1) == 0 ? -1f : 1f, (i & 2) == 0 ? -1f : 1f, (i & 4) == 0 ? -1f : 1f));
                Vector3 p = go.transform.InverseTransformPoint(rend.transform.TransformPoint(canto));
                if (primeiro)
                {
                    b = new Bounds(p, Vector3.zero);
                    primeiro = false;
                }
                else
                {
                    b.Encapsulate(p);
                }
            }
        }
        return b;
    }

    /* Grava a lista do que o ponto recarrega, o texto da mira, o tempo e se
     * é a picape da UPA (só ela dá o post de reabastecer). */
    private static void ConfigurarReabastecimento(PontoDeReabastecimento ponto, string textoAoMirar,
                                                  float segundos, bool ehPicapeDaUpa, params Ferramenta[] recarrega)
    {
        SerializedObject so = new SerializedObject(ponto);
        so.FindProperty("textoAoMirar").stringValue = textoAoMirar;
        so.FindProperty("segundosParaUsar").floatValue = segundos;
        so.FindProperty("ehPicapeDaUpa").boolValue = ehPicapeDaUpa;
        SerializedProperty lista = so.FindProperty("recarrega");
        lista.arraySize = recarrega.Length;
        for (int i = 0; i < recarrega.Length; i++)
            lista.GetArrayElementAtIndex(i).intValue = (int)recarrega[i];
        so.ApplyModifiedProperties();
    }

    private static float DistanciaNoChao(Vector3 a, Vector3 b)
    {
        return Vector2.Distance(new Vector2(a.x, a.z), new Vector2(b.x, b.z));
    }
}
