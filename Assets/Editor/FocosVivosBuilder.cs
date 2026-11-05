using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/* ============================================================
 *  FocosVivosBuilder.cs  (Editor)
 *  ---------------------------------------------------------------------
 *  Monta na cena do jogo (SampleScene) tudo o que os focos vivos, a
 *  chuva e os novos destinos da carga (Parte B) precisam para funcionar.
 *
 *  Menu:  Dengue > Configurar Focos Vivos e Chuva
 *
 *  Rodar depois do Dengue > Configurar Kit do Agente. Pode rodar de novo:
 *  acha pelo nome o que já criou e só atualiza. No fim salva a cena e os
 *  assets e loga um resumo [FocosVivos] no Console.
 *
 *  A cena configurada é a do FluxoDeJogo achado, como no
 *  KitDoAgenteBuilder: é nela que tudo é procurado, criado e salvo.
 *
 *    1. Gerenciador, chuva e enxame: o objeto GerenciadorDeFocos no grupo
 *       dos focos (com o EnxameDeMosquitos), o "Clima" nos sistemas (Chuva
 *       e ChuvaVisual: sol = a luz direcional, gotas = o ParticleSystem
 *       "Gotas de Chuva", filho da Main Camera) e as referências ligadas
 *       em quem usa o gerenciador e a chuva.
 *    2. Atalho: podeEsvaziar nos vasos, pneus e garrafas da cena, e no
 *       prefab deles quando o foco vem do prefab. Ralo, bromélia e caixa
 *       d'água ficam de fora.
 *    3. Pneus em tamanho real: escala uniforme que leva a maior dimensão
 *       do pneu a 0,7 m, assentado no chão. A medida é a do próprio pneu
 *       (no espaço local), e não a caixa alinhada ao mundo: esta cresce com
 *       a inclinação, e pneus iguais sairiam de tamanhos diferentes. Um
 *       pneu já com ~0,7 m não é tocado.
 *    4. Caçamba da picape: o filho "Cacamba da Picape" da Picape da UPA,
 *       com collider na metade traseira, o Cacamba e as vagas "Vaga 1..N"
 *       (2 colunas × 3 alturas, pneus deitados). O collider fica 2 cm para
 *       dentro da caixa da carroceria (BoxCollider do prefab): assim a
 *       mira acerta a picape, que atende pela Cacamba com o pneu nas mãos
 *       (PontoDeReabastecimento) e acende o destaque do carro inteiro.
 *    5. Tira as 3 caçambas de entulho da Parte A.
 *    6. Lixeiras: Lixeira nas 3 que o bairro já tem e em 2 novas, cópias
 *       da "Trassh Bin (1)" (a de ~1 m de altura), no lugar livre mais
 *       perto do pedido (OverlapBox com folga). O resumo traz a lixeira
 *       mais perto de cada grupo de garrafas.
 *    7. Player: PoseDeCarga (assets e pose) e FeedbackDeCarga com as
 *       referências.
 *    8. Textos: os da Parte B na HistoriaRedeSocial da cena (os campos
 *       novos e os revistos) e, nos FocoHUDInfo de pneus e garrafas, o
 *       textoFerramentaErrada que cita a picape e a lixeira.
 *    9. Sons: SomBuilder.LigarSonsDosFocosVivos, depois de tudo criado.
 *   10. Build: Fog Modes em Custom com Linear, para a névoa da chuva
 *       sobreviver à build sem depender da névoa da MenuInicial.
 *
 *  O balanceamento (taxa, alívio, multiplicadorCarregando) não é daqui: o
 *  valor que vale é o da cena (Dengue > Configurações Globais). Os padrões
 *  dos campos no código continuam os antigos.
 * ============================================================ */

public static class FocosVivosBuilder
{
    private const string NomeUndo = "Configurar Focos Vivos e Chuva";

    private const string NomeGerenciador = "GerenciadorDeFocos";
    private const string NomeClima = "Clima";
    private const string NomeGotas = "Gotas de Chuva";
    private const string NomePicape = "Picape da UPA";
    private const string NomeCacamba = "Cacamba da Picape";
    private const string GrupoObjetosDeRua = "Objetos de Rua";     // dentro do --- BAIRRO ---

    private const string PastaChuva = MeusOrganizador.Destino + "/Chuva";
    private const string CaminhoMaterialGota = PastaChuva + "/Gota de Chuva.mat";
    private const string CaminhoGraphicsSettings = "ProjectSettings/GraphicsSettings.asset";
    private const int FogStrippingCustom = 1;

    // Quem tem o campo 'gerenciador' (e 'chuva', se tiver).
    private static readonly System.Type[] UsamGerenciador =
    {
        typeof(FluxoDeJogo), typeof(DengueHUD), typeof(FocoListaHUD), typeof(RedeSocial),
        typeof(HistoriaRedeSocial), typeof(SomDosFocos), typeof(FocoProximityGlow),
        typeof(Chuva), typeof(EnxameDeMosquitos),
    };

    /* ---- Pneus ---- */
    private const float DiametroDoPneu = 0.7f;
    private const float ToleranciaDoPneu = 0.05f;   // dentro dela, o pneu não é tocado

    /* ---- Caçamba da picape ----
     * Medidas no espaço da raiz da picape (a frente é +Z), do mapa de alturas
     * da malha: piso da carroceria a 0,82 m do pivô, de z −0,45 (cabine) a
     * −1,70 (tampa traseira) e ~1 m de largura. O pivô da Cacamba fica no
     * centro do piso: sem vaga, o pneu some 1,2 m acima dele. */
    private static readonly Vector3 CentroDoPiso = new Vector3(0f, 0.82f, -1.07f);
    private const float MargemDoCollider = 0.02f;   // para dentro da caixa da carroceria

    // As colunas ficam no comprimento: 2 pneus de 0,7 m não cabem na largura.
    // A primeira altura deixa o pneu (~0,2 m de banda) deitado no piso, e a
    // borda dele entra só na espessura da cabine e da tampa.
    private const int ColunasDeVagas = 2;
    private const int VagasMinimas = 6;
    private const float MeiaDistanciaDasColunas = 0.35f;
    private const float AlturaDaPrimeiraVaga = 0.1f;
    private const float DistanciaEntreAlturas = 0.22f;

    // O eixo da roda do pneu é o X local: girado 90° em Z, ele fica na vertical.
    private static readonly Vector3 GiroDaVaga = new Vector3(0f, 0f, 90f);

    /* ---- Lixeiras ---- */
    private static readonly string[] LixeirasDoBairro = { "Trassh Bin", "Trassh Bin (1)", "Trash_01" };
    private const string ModeloDaLixeiraNova = "Trassh Bin (1)";

    // Perto dos grupos de garrafas a mais de 20 m de uma lixeira (x, z).
    private static readonly (string nome, Vector2 lugar)[] LixeirasNovas =
    {
        ("Lixeira Terreno Norte", new Vector2(-22f, 52f)),
        ("Lixeira Oeste", new Vector2(-30f, -14f)),
    };

    private const float FolgaDaLixeira = 0.6f;
    private const float RespiroDoChao = 0.05f;
    private const float PassoDaBusca = 0.5f;
    private const float RaioMaximoDaBusca = 15f;
    private const int DirecoesDaBusca = 16;
    private const float AlturaDeReferencia = 0.3f;   // o raio até o chão sai 3 m acima disto

    /* ---- Gotas (partículas presas à câmera) ----
     * Nascem numa esfera em volta da câmera, e não num plano acima dela: a
     * câmera gira, e a esfera é a única forma que não gira junto. Caem por
     * velocidade em espaço de mundo, retas para qualquer lado que o agente
     * olhe, e somem depressa (entram e saem em fade). */
    private const float RaioDaChuva = 16f;
    private const float VidaDaGota = 0.7f;
    private const float VelocidadeDaQueda = 18f;
    private const float Vento = 1.2f;
    private const float GotasPorSegundo = 2800f;
    private const int MaximoDeGotas = 3000;
    private const float EspessuraDaGota = 0.02f;
    private const float EsticadaDaGota = 0.05f;       // comprimento (m) por m/s
    private static readonly Color CorDaGota = new Color(0.78f, 0.84f, 0.92f, 0.45f);

    // A cena do jogo sendo configurada, só durante o Configurar.
    private static Scene cenaDoJogo;

    [MenuItem("Dengue/Configurar Focos Vivos e Chuva")]
    public static void Configurar()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogError("[FocosVivos] Saia do Play Mode antes de configurar.");
            return;
        }
        FluxoDeJogo fluxo = Object.FindAnyObjectByType<FluxoDeJogo>(FindObjectsInactive.Include);
        if (fluxo == null)
        {
            Debug.LogError("[FocosVivos] Abra a SampleScene (a cena do jogo, com FluxoDeJogo) antes de configurar.");
            return;
        }

        cenaDoJogo = fluxo.gameObject.scene;
        try
        {
            ControladorJogador jogador = PrimeiroDaCena<ControladorJogador>();
            if (jogador == null || jogador.GetComponent<KitDoAgente>() == null)
            {
                Debug.LogError("[FocosVivos] Player com KitDoAgente não encontrado na cena " + cenaDoJogo.name +
                               ". Rode antes Dengue > Configurar Kit do Agente.");
                return;
            }

            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName(NomeUndo);
            int grupoUndo = Undo.GetCurrentGroup();

            StringBuilder log = new StringBuilder();
            ConfigurarGerenciadorEChuva(fluxo, log);
            ConfigurarAtalho(log);
            PorPneusEmTamanhoReal(log);
            MontarCacambaDaPicape(log);
            RemoverCacambasDeEntulho(log);
            MontarLixeiras(log);
            ConfigurarPlayer(jogador, fluxo, log);
            GravarTextos(log);

            SomBuilder.LigarSonsDosFocosVivos(cenaDoJogo);
            log.Append("\n  • Sons: ligados (chuva, esvaziar, picape, lixeiras, esforço, passos pesados)");

            ManterNevoaLinearNaBuild(log);

            Undo.CollapseUndoOperations(grupoUndo);

            EditorSceneManager.MarkSceneDirty(cenaDoJogo);
            EditorSceneManager.SaveScene(cenaDoJogo);
            AssetDatabase.SaveAssets();

            Debug.Log("[FocosVivos] Concluído (cena " + cenaDoJogo.name + " salva)." + log);
        }
        finally
        {
            cenaDoJogo = default;
        }
    }

    /* ================= 1. Gerenciador, chuva e enxame ================= */

    private static void ConfigurarGerenciadorEChuva(FluxoDeJogo fluxo, StringBuilder log)
    {
        GerenciadorDeFocos gerenciador = PrimeiroDaCena<GerenciadorDeFocos>();
        if (gerenciador == null)
        {
            GameObject go = NovoEm(NomeGerenciador, RaizDaCena(HierarquiaOrganizador.Focos));
            gerenciador = Undo.AddComponent<GerenciadorDeFocos>(go);
        }
        Garantir<EnxameDeMosquitos>(gerenciador.gameObject);
        Ligar(gerenciador, "fluxo", fluxo);

        Chuva chuva = PrimeiroDaCena<Chuva>();
        GameObject clima = chuva != null ? chuva.gameObject : AcharNaCena(NomeClima);
        if (clima == null)
            clima = NovoEm(NomeClima, RaizDaCena(HierarquiaOrganizador.Sistemas));
        chuva = Garantir<Chuva>(clima);
        ChuvaVisual visual = Garantir<ChuvaVisual>(clima);
        Ligar(chuva, "fluxo", fluxo);

        Light sol = AcharSol();
        if (sol == null)
            Debug.LogWarning("[FocosVivos] Luz direcional não encontrada: a chuva não vai escurecer o sol.");
        ParticleSystem gotas = MontarGotas();

        Ligar(visual, "chuva", chuva);
        Ligar(visual, "fluxo", fluxo);
        Ligar(visual, "sol", sol);
        Ligar(visual, "gotas", gotas);

        // As referências: o campo vazio funcionaria, mas por busca em runtime.
        int ligados = 0;
        foreach (System.Type tipo in UsamGerenciador)
        {
            foreach (Object c in Object.FindObjectsByType(tipo, FindObjectsInactive.Include))
            {
                if (((Component)c).gameObject.scene != cenaDoJogo)
                    continue;
                Ligar(c, "gerenciador", gerenciador);
                if (new SerializedObject(c).FindProperty("chuva") != null)
                    Ligar(c, "chuva", chuva);
                ligados++;
            }
        }

        log.Append("\n  • Gerenciador: ").Append(Caminho(gerenciador.transform)).Append(" com EnxameDeMosquitos, ")
           .Append(DaCena<Interactable>().Count).Append(" focos na cena; ligado em ").Append(ligados).Append(" componentes");
        log.Append("\n  • Clima: ").Append(Caminho(clima.transform)).Append(" (Chuva e ChuvaVisual); sol = ")
           .Append(sol != null ? sol.name : "FALTOU").Append("; gotas = ")
           .Append(gotas != null ? Caminho(gotas.transform) : "FALTOU (sem Main Camera)");
    }

    private static Light AcharSol()
    {
        foreach (Light luz in DaCena<Light>())
            if (luz.type == LightType.Directional)
                return luz;
        return null;
    }

    /* As gotas: filho da Main Camera (a chuva vai com o agente), desligado
     * até a primeira chuva. */
    private static ParticleSystem MontarGotas()
    {
        CameraJogador camera = PrimeiroDaCena<CameraJogador>();
        if (camera == null)
        {
            Debug.LogWarning("[FocosVivos] Main Camera (CameraJogador) não encontrada: a chuva fica sem gotas.");
            return null;
        }

        Transform existente = camera.transform.Find(NomeGotas);
        GameObject go = existente != null ? existente.gameObject : NovoEm(NomeGotas, camera.transform);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;

        ParticleSystem ps = Garantir<ParticleSystem>(go);
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        ParticleSystem.MainModule main = ps.main;
        main.loop = true;
        main.playOnAwake = false;
        main.startLifetime = VidaDaGota;
        main.startSpeed = 0f;                 // quem cai é a velocidade em espaço de mundo, abaixo
        main.startSize = EspessuraDaGota;
        main.startColor = CorDaGota;
        main.maxParticles = MaximoDeGotas;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = 0f;

        ParticleSystem.EmissionModule emission = ps.emission;
        emission.rateOverTime = GotasPorSegundo;

        ParticleSystem.ShapeModule shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = RaioDaChuva;
        shape.radiusThickness = 1f;

        ParticleSystem.VelocityOverLifetimeModule queda = ps.velocityOverLifetime;
        queda.enabled = true;
        queda.space = ParticleSystemSimulationSpace.World;
        queda.x = new ParticleSystem.MinMaxCurve(Vento);
        queda.y = new ParticleSystem.MinMaxCurve(-VelocidadeDaQueda);
        queda.z = new ParticleSystem.MinMaxCurve(0f);

        Gradient fade = new Gradient();
        fade.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                     new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.15f),
                             new GradientAlphaKey(1f, 0.8f), new GradientAlphaKey(0f, 1f) });
        ParticleSystem.ColorOverLifetimeModule cor = ps.colorOverLifetime;
        cor.enabled = true;
        cor.color = new ParticleSystem.MinMaxGradient(fade);

        ParticleSystemRenderer rend = go.GetComponent<ParticleSystemRenderer>();
        rend.renderMode = ParticleSystemRenderMode.Stretch;
        rend.lengthScale = 1f;
        rend.velocityScale = EsticadaDaGota;
        rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        rend.receiveShadows = false;
        rend.sharedMaterial = MaterialDaGota();

        return ps;
    }

    /* Sprites/Default: transparente, com a cor vindo das partículas (o mesmo
     * dos enxames). Como asset, o shader vai para a build pela referência. */
    private static Material MaterialDaGota()
    {
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(CaminhoMaterialGota);
        if (mat == null)
        {
            MeusOrganizador.GarantirPasta(PastaChuva);
            mat = new Material(Shader.Find("Sprites/Default"));
            AssetDatabase.CreateAsset(mat, CaminhoMaterialGota);
        }
        return mat;
    }

    /* ================= 2. Atalho ================= */

    private static void ConfigurarAtalho(StringBuilder log)
    {
        int cena = 0;
        HashSet<string> prefabs = new HashSet<string>();
        foreach (Interactable foco in DaCena<Interactable>())
        {
            if (!(foco is VasoInteractable || foco is PneuInteractable || foco is GarrafaInteractable))
                continue;

            // Pneus e garrafas da cena não têm o componente no prefab.
            Interactable origem = PrefabUtility.GetCorrespondingObjectFromSource(foco);
            if (origem != null)
                prefabs.Add(AssetDatabase.GetAssetPath(origem));

            Ligar(foco, "podeEsvaziar", true);
            cena++;
        }

        foreach (string caminho in prefabs)
        {
            GameObject raiz = PrefabUtility.LoadPrefabContents(caminho);
            try
            {
                bool mudou = false;
                foreach (Interactable foco in raiz.GetComponentsInChildren<Interactable>(true))
                {
                    SerializedObject so = new SerializedObject(foco);
                    so.FindProperty("podeEsvaziar").boolValue = true;
                    mudou |= so.ApplyModifiedPropertiesWithoutUndo();
                }
                if (mudou)
                    PrefabUtility.SaveAsPrefabAsset(raiz, caminho);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(raiz);
            }
        }

        log.Append("\n  • Atalho: podeEsvaziar em ").Append(cena).Append(" focos da cena e nos prefabs: ")
           .Append(prefabs.Count > 0 ? string.Join(", ", prefabs) : "nenhum");
    }

    /* ================= 3. Pneus em tamanho real ================= */

    private static void PorPneusEmTamanhoReal(StringBuilder log)
    {
        List<PneuInteractable> pneus = DaCena<PneuInteractable>();
        log.Append("\n  • Pneus: ").Append(pneus.Count);
        foreach (PneuInteractable pneu in pneus)
        {
            float antes = MaiorDimensao(pneu);
            log.Append("\n      ").Append(pneu.name).Append(": ");
            if (Mathf.Abs(antes - DiametroDoPneu) <= ToleranciaDoPneu)
            {
                log.Append("já com ").Append(antes.ToString("0.00")).Append(" m (escala ")
                   .Append(pneu.transform.localScale.x.ToString("0.0")).Append("), sem mudança");
                continue;
            }

            float escalaAntes = pneu.transform.localScale.x;
            Undo.RecordObject(pneu.transform, NomeUndo);
            pneu.transform.localScale *= DiametroDoPneu / antes;
            string chao = KitDoAgenteBuilder.AssentarNoChao(pneu.gameObject, pneu.transform.position, out _);
            DescontarInclinacao(pneu);

            log.Append("escala ").Append(escalaAntes.ToString("0.0")).Append(" → ")
               .Append(pneu.transform.localScale.x.ToString("0.0")).Append(", ")
               .Append(antes.ToString("0.00")).Append(" m → ").Append(MaiorDimensao(pneu).ToString("0.00"))
               .Append(" m, em ").Append(pneu.transform.position.ToString("F2")).Append(" (chão: ").Append(chao).Append(')');
        }
    }

    /* O AssentarNoChao usa a caixa dos Renderers, e a de um pneu inclinado é
     * maior que ele: o pneu ficaria flutuando. Aqui a altura do pivô vem do
     * ponto mais baixo do cilindro (o eixo da roda é o X local). */
    private static void DescontarInclinacao(PneuInteractable pneu)
    {
        Vector3 tamanho = Vector3.Scale(KitDoAgenteBuilder.LimitesLocais(pneu.gameObject).size, pneu.transform.lossyScale);
        float eixoNaVertical = Mathf.Abs(pneu.transform.right.y);
        float maisBaixo = tamanho.y / 2f * Mathf.Sqrt(1f - eixoNaVertical * eixoNaVertical)
                          + tamanho.x / 2f * eixoNaVertical;

        Vector3 p = pneu.transform.position;
        p.y = KitDoAgenteBuilder.LimitesNoMundo(pneu.gameObject).min.y + maisBaixo;
        pneu.transform.position = p;
    }

    /* A maior dimensão do pneu em metros, no espaço dele (não muda com a
     * inclinação). */
    private static float MaiorDimensao(PneuInteractable pneu)
    {
        Vector3 tamanho = Vector3.Scale(KitDoAgenteBuilder.LimitesLocais(pneu.gameObject).size, pneu.transform.lossyScale);
        return Mathf.Max(tamanho.x, tamanho.y, tamanho.z);
    }

    /* ================= 4. Caçamba da picape ================= */

    private static void MontarCacambaDaPicape(StringBuilder log)
    {
        GameObject picape = AcharNaCena(NomePicape);
        PontoDeReabastecimento ponto = picape != null ? picape.GetComponent<PontoDeReabastecimento>() : null;
        if (ponto == null)
        {
            Debug.LogWarning("[FocosVivos] " + NomePicape + " (PontoDeReabastecimento) não encontrada: rode Dengue > Configurar Kit do Agente.");
            log.Append("\n  • Caçamba: FALTOU (sem ").Append(NomePicape).Append(')');
            return;
        }

        // A carroceria balança: static batching ignoraria o giro.
        int semStatic = 0;
        foreach (Transform t in picape.GetComponentsInChildren<Transform>(true))
        {
            StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(t.gameObject);
            if ((flags & StaticEditorFlags.BatchingStatic) == 0)
                continue;
            GameObjectUtility.SetStaticEditorFlags(t.gameObject, flags & ~StaticEditorFlags.BatchingStatic);
            semStatic++;
        }

        Bounds limites = KitDoAgenteBuilder.LimitesLocais(picape);

        Transform cacambaT = picape.transform.Find(NomeCacamba);
        GameObject go = cacambaT != null ? cacambaT.gameObject : NovoEm(NomeCacamba, picape.transform);
        Undo.RecordObject(go.transform, NomeUndo);
        go.transform.localPosition = CentroDoPiso;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;

        // A metade traseira, do piso para cima e 2 cm para dentro da carroceria.
        Vector3 minimo = new Vector3(limites.min.x + MargemDoCollider, CentroDoPiso.y, limites.min.z + MargemDoCollider);
        Vector3 maximo = new Vector3(limites.max.x - MargemDoCollider, limites.max.y - MargemDoCollider, limites.center.z);
        BoxCollider box = Garantir<BoxCollider>(go);
        Undo.RecordObject(box, NomeUndo);
        box.isTrigger = false;
        box.center = (minimo + maximo) / 2f - CentroDoPiso;
        box.size = maximo - minimo;

        List<Transform> vagas = MontarVagas(go.transform, PneusDaCena());

        Cacamba cacamba = Garantir<Cacamba>(go);
        SerializedObject so = new SerializedObject(cacamba);
        so.FindProperty("textoAoMirar").stringValue = "Caçamba da picape da UPA";
        so.FindProperty("segundosParaUsar").floatValue = 1f;
        so.FindProperty("picape").objectReferenceValue = ponto;
        so.FindProperty("carroceria").objectReferenceValue = picape.transform;
        SerializedProperty lista = so.FindProperty("vagas");
        lista.arraySize = vagas.Count;
        for (int i = 0; i < vagas.Count; i++)
            lista.GetArrayElementAtIndex(i).objectReferenceValue = vagas[i];
        so.ApplyModifiedProperties();

        log.Append("\n  • Caçamba: ").Append(NomeCacamba).Append(" em ").Append(go.transform.position.ToString("F2"))
           .Append(", collider ").Append(box.size.ToString("F2")).Append(", ").Append(vagas.Count)
           .Append(" vagas (escala de mundo ").Append(vagas[0].lossyScale.ToString("F2")).Append("), carroceria = ")
           .Append(picape.name).Append(", escala da picape ").Append(picape.transform.lossyScale.ToString("F2"))
           .Append(", ").Append(semStatic).Append(" objetos saíram do Batching Static");
    }

    private static int PneusDaCena()
    {
        int pneus = 0;
        foreach (GameObject raiz in cenaDoJogo.GetRootGameObjects())
            pneus += raiz.GetComponentsInChildren<PneuInteractable>(true).Length;
        return pneus;
    }

    /* Uma vaga por pneu da cena, em pares (as colunas), com o mínimo de
     * VagasMinimas. Pôs mais pneus no mapa: rode o menu de novo. */
    internal static int TotalDeVagas(int pneus)
    {
        int alturas = Mathf.CeilToInt(Mathf.Max(pneus, VagasMinimas) / (float)ColunasDeVagas);
        return alturas * ColunasDeVagas;
    }

    /* Vaga 1..N: de baixo para cima, alternando as colunas. Escala (1, 1, 1):
     * o pneu assentado vira filho da vaga e não pode esticar. */
    private static List<Transform> MontarVagas(Transform cacamba, int pneus)
    {
        List<Transform> vagas = new List<Transform>();
        int total = TotalDeVagas(pneus);
        for (int i = 0; i < total; i++)
        {
            string nome = "Vaga " + (i + 1);
            Transform vaga = cacamba.Find(nome);
            if (vaga == null)
                vaga = NovoEm(nome, cacamba).transform;

            int altura = i / ColunasDeVagas;
            float coluna = i % ColunasDeVagas == 0 ? MeiaDistanciaDasColunas : -MeiaDistanciaDasColunas;
            Undo.RecordObject(vaga, NomeUndo);
            vaga.localPosition = new Vector3(0f, AlturaDaPrimeiraVaga + altura * DistanciaEntreAlturas, coluna);
            vaga.localEulerAngles = GiroDaVaga;
            vaga.localScale = Vector3.one;
            vagas.Add(vaga);
        }
        return vagas;
    }

    /* ================= 5. Caçambas de entulho ================= */

    private static void RemoverCacambasDeEntulho(StringBuilder log)
    {
        int removidas = 0;
        foreach (string nome in new[] { "Cacamba", "Cacamba (2)", "Cacamba (3)" })
        {
            GameObject antiga = AcharNaCena(nome);
            if (antiga == null)
                continue;
            Undo.DestroyObjectImmediate(antiga);
            removidas++;
        }
        Physics.SyncTransforms();
        log.Append("\n  • Caçambas de entulho: ").Append(removidas).Append(" removidas");
    }

    /* ================= 6. Lixeiras ================= */

    private static void MontarLixeiras(StringBuilder log)
    {
        foreach (string nome in LixeirasDoBairro)
        {
            GameObject lixeira = AcharNaCena(nome);
            if (lixeira == null)
            {
                Debug.LogWarning("[FocosVivos] Lixeira não encontrada: " + nome);
                log.Append("\n  • Lixeira ").Append(nome).Append(": FALTOU");
                continue;
            }
            ConfigurarLixeira(lixeira);
            log.Append("\n  • Lixeira ").Append(nome).Append(": ").Append(lixeira.transform.position.ToString("F2"));
        }

        foreach (var nova in LixeirasNovas)
            MontarLixeiraNova(nova.nome, nova.lugar, log);

        // A viagem com o saco: de cada grupo de garrafas até a lixeira mais perto.
        List<Lixeira> lixeiras = DaCena<Lixeira>();
        log.Append("\n  • Lixeiras na cena: ").Append(lixeiras.Count);
        foreach (GarrafaInteractable garrafas in DaCena<GarrafaInteractable>())
        {
            float distancia = float.MaxValue;
            string qual = "nenhuma";
            foreach (Lixeira lixeira in lixeiras)
            {
                float d = KitDoAgenteBuilder.DistanciaNoChao(garrafas.transform.position, lixeira.transform.position);
                if (d >= distancia)
                    continue;
                distancia = d;
                qual = lixeira.name;
            }
            log.Append("\n      ").Append(garrafas.name.Trim()).Append(" ").Append(garrafas.transform.position.ToString("F1"))
               .Append(": ").Append(distancia.ToString("0.0")).Append(" m até ").Append(qual);
        }
    }

    /* Cópia da lixeira-modelo no lugar livre mais perto do pedido. Já
     * existindo, fica onde está. */
    private static void MontarLixeiraNova(string nome, Vector2 pedido, StringBuilder log)
    {
        log.Append("\n  • ").Append(nome).Append(": ");
        GameObject lixeira = AcharNaCena(nome);
        if (lixeira == null)
        {
            GameObject modelo = AcharNaCena(ModeloDaLixeiraNova);
            if (modelo == null)
            {
                Debug.LogWarning("[FocosVivos] Modelo " + ModeloDaLixeiraNova + " não encontrado: " + nome + " ficou de fora.");
                log.Append("FALTOU (sem ").Append(ModeloDaLixeiraNova).Append(')');
                return;
            }

            GameObject bairro = RaizDaCena(HierarquiaOrganizador.Bairro).gameObject;
            Transform grupo = bairro.transform.Find(GrupoObjetosDeRua);
            lixeira = Object.Instantiate(modelo, grupo != null ? grupo : bairro.transform);
            lixeira.name = nome;
            lixeira.transform.SetPositionAndRotation(modelo.transform.position, modelo.transform.rotation);
            Undo.RegisterCreatedObjectUndo(lixeira, NomeUndo);

            if (!AcharLugarLivre(lixeira, pedido, out string detalhe))
                log.Append("SEM LUGAR LIVRE (").Append(detalhe).Append("); ");
        }

        ConfigurarLixeira(lixeira);
        Vector3 p = lixeira.transform.position;
        log.Append("em ").Append(p.ToString("F2")).Append(", a ")
           .Append(Vector2.Distance(new Vector2(p.x, p.z), pedido).ToString("0.0")).Append(" m do pedido ")
           .Append(pedido.ToString("F0")).Append("; ")
           .Append(ConferirEspaco(lixeira, ChaoEmbaixo(lixeira), out string batidas) ? "livre, com " + FolgaDaLixeira.ToString("0.0") + " m de folga" : "ENCOSTA em " + batidas);
    }

    /* Anéis em volta do pedido, do mais perto para o mais longe: a lixeira
     * fica no primeiro lugar livre. Sem nenhum, volta para o pedido. */
    private static bool AcharLugarLivre(GameObject lixeira, Vector2 pedido, out string detalhe)
    {
        detalhe = "";
        for (float raio = 0f; raio <= RaioMaximoDaBusca; raio += PassoDaBusca)
        {
            int direcoes = raio <= 0f ? 1 : DirecoesDaBusca;
            for (int i = 0; i < direcoes; i++)
            {
                float angulo = i * Mathf.PI * 2f / direcoes;
                Vector3 alvo = new Vector3(pedido.x + raio * Mathf.Cos(angulo), AlturaDeReferencia, pedido.y + raio * Mathf.Sin(angulo));
                KitDoAgenteBuilder.AssentarNoChao(lixeira, alvo, out Collider chao);
                if (chao == null)
                    continue;
                if (ConferirEspaco(lixeira, chao, out string batidas))
                    return true;
                if (raio <= 0f)
                    detalhe = "pedido ocupado por " + batidas;
            }
        }

        Debug.LogWarning("[FocosVivos] " + lixeira.name + ": nenhum lugar livre a até " + RaioMaximoDaBusca + " m do pedido.");
        KitDoAgenteBuilder.AssentarNoChao(lixeira, new Vector3(pedido.x, AlturaDeReferencia, pedido.y), out _);
        return false;
    }

    /* Nada atravessa nem encosta na lixeira: OverlapBox da caixa dela com a
     * folga em volta e acima, sem triggers, sem ela e sem o chão dela. */
    private static bool ConferirEspaco(GameObject lixeira, Collider chao, out string batidas)
    {
        Bounds b = KitDoAgenteBuilder.LimitesNoMundo(lixeira);
        Vector3 meiaCaixa = new Vector3(b.extents.x + FolgaDaLixeira, (b.size.y + FolgaDaLixeira) / 2f, b.extents.z + FolgaDaLixeira);
        Vector3 centro = new Vector3(b.center.x, b.min.y + RespiroDoChao + meiaCaixa.y, b.center.z);

        List<string> nomes = new List<string>();
        foreach (Collider c in Physics.OverlapBox(centro, meiaCaixa, Quaternion.identity, ~0, QueryTriggerInteraction.Ignore))
            if (c != chao && !c.transform.IsChildOf(lixeira.transform))
                nomes.Add(c.name);

        batidas = string.Join(", ", nomes);
        return nomes.Count == 0;
    }

    /* O collider logo abaixo da lixeira (null se não houver). */
    private static Collider ChaoEmbaixo(GameObject lixeira)
    {
        Bounds b = KitDoAgenteBuilder.LimitesNoMundo(lixeira);
        foreach (RaycastHit acerto in Physics.RaycastAll(b.center, Vector3.down, b.size.y + 1f, ~0, QueryTriggerInteraction.Ignore))
            if (!acerto.collider.transform.IsChildOf(lixeira.transform))
                return acerto.collider;
        return null;
    }

    /* Lixeira, texto da mira, collider sólido (o Interactor ignora trigger) e
     * a tampa, se o modelo tiver uma em separado. */
    private static void ConfigurarLixeira(GameObject go)
    {
        if (!KitDoAgenteBuilder.TemColliderSolido(go))
        {
            Bounds limites = KitDoAgenteBuilder.LimitesLocais(go);
            BoxCollider box = Undo.AddComponent<BoxCollider>(go);
            box.center = limites.center;
            box.size = limites.size;
        }

        Lixeira lixeira = Garantir<Lixeira>(go);
        SerializedObject so = new SerializedObject(lixeira);
        so.FindProperty("textoAoMirar").stringValue = "Lixeira";
        so.FindProperty("tampa").objectReferenceValue = AcharTampa(go.transform);
        so.ApplyModifiedProperties();
    }

    private static Transform AcharTampa(Transform lixeira)
    {
        foreach (Transform t in lixeira.GetComponentsInChildren<Transform>(true))
        {
            if (t == lixeira)
                continue;
            string nome = t.name.ToLowerInvariant();
            if (nome.Contains("lid") || nome.Contains("tampa"))
                return t;
        }
        return null;
    }

    /* ================= 7. Player ================= */

    /* O FeedbackDeCarga antes do Aplicar, e os assets antes dos dois: o
     * Aplicar precisa do kit, do corpo e do Saco.fbx prontos. */
    private static void ConfigurarPlayer(ControladorJogador jogador, FluxoDeJogo fluxo, StringBuilder log)
    {
        string assets = PoseDeCarga.GarantirAssets();
        if (assets.Contains("ERRO"))
            Debug.LogError(assets);

        GameObject player = jogador.gameObject;
        FeedbackDeCarga feedback = Garantir<FeedbackDeCarga>(player);
        PoseDeCarga.Aplicar(player);

        Ligar(feedback, "jogador", jogador);
        Ligar(feedback, "fluxo", fluxo);
        Ligar(feedback, "animator", player.GetComponentInChildren<Animator>(true));
        Ligar(feedback, "cameraJogador", PrimeiroDaCena<CameraJogador>());
        Ligar(feedback, "somDoJogador", player.GetComponent<SomDoJogador>());

        KitDoAgente kit = player.GetComponent<KitDoAgente>();

        log.Append("\n  • Player: FeedbackDeCarga ligado; PontoDeCarga em ")
           .Append(kit.PontoDeCarga != null ? Caminho(kit.PontoDeCarga) : "FALTOU")
           .Append("\n      ").Append(assets.Replace("\n", "\n      "));
    }

    /* ================= 8. Textos ================= */

    private static void GravarTextos(StringBuilder log)
    {
        HistoriaRedeSocial historia = PrimeiroDaCena<HistoriaRedeSocial>();
        if (historia != null)
        {
            Undo.RecordObject(historia, NomeUndo);
            historia.RegravarTextosDaParteB();
            EditorUtility.SetDirty(historia);
        }
        else
        {
            Debug.LogWarning("[FocosVivos] HistoriaRedeSocial não encontrada: os posts da Parte B ficaram de fora.");
        }

        int avisos = 0;
        foreach (FocoHUDInfo info in DaCena<FocoHUDInfo>())
        {
            Interactable foco = info.GetComponent<Interactable>();
            string texto = foco is PneuInteractable ? KitDoAgenteBuilder.TextoPneuFerramentaErrada
                         : foco is GarrafaInteractable ? KitDoAgenteBuilder.TextoGarrafaFerramentaErrada
                         : null;
            if (texto == null)
                continue;
            Ligar(info, "textoFerramentaErrada", texto);
            avisos++;
        }

        log.Append("\n  • Textos: ").Append(historia != null ? "HistoriaRedeSocial regravada" : "sem HistoriaRedeSocial")
           .Append("; picape e lixeira em ").Append(avisos).Append(" FocoHUDInfo");
    }

    /* ================= 10. Build ================= */

    /* A chuva liga a névoa Linear só em runtime: com o Fog Modes em Automatic,
     * ela dependeria de alguma cena da build ter névoa Linear ligada. */
    private static void ManterNevoaLinearNaBuild(StringBuilder log)
    {
        Object[] ativos = AssetDatabase.LoadAllAssetsAtPath(CaminhoGraphicsSettings);
        if (ativos.Length == 0)
        {
            Debug.LogWarning("[FocosVivos] Não achei " + CaminhoGraphicsSettings + ": confira o Fog Modes na mão.");
            return;
        }

        SerializedObject so = new SerializedObject(ativos[0]);
        so.FindProperty("m_FogStripping").intValue = FogStrippingCustom;
        so.FindProperty("m_FogKeepLinear").boolValue = true;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(ativos[0]);
        log.Append("\n  • Build: Fog Modes em Custom, Linear mantido");
    }

    /* ================= Utilidades ================= */

    private static T Garantir<T>(GameObject go) where T : Component
    {
        T c = go.GetComponent<T>();
        return c != null ? c : Undo.AddComponent<T>(go);
    }

    /* Grava um campo serializado (referência, bool, float ou texto), com Undo. */
    private static void Ligar(Object alvo, string campo, object valor)
    {
        SerializedObject so = new SerializedObject(alvo);
        SerializedProperty p = so.FindProperty(campo);
        if (p == null)
        {
            Debug.LogError("[FocosVivos] Campo '" + campo + "' não existe em " + alvo.GetType().Name);
            return;
        }

        switch (valor)
        {
            case bool b: p.boolValue = b; break;
            case float f: p.floatValue = f; break;
            case string s: p.stringValue = s; break;
            default: p.objectReferenceValue = valor as Object; break;
        }
        so.ApplyModifiedProperties();
    }

    /* Componentes T da cena do jogo (inclusive inativos). */
    private static List<T> DaCena<T>() where T : Component
    {
        List<T> achados = new List<T>();
        foreach (T c in Object.FindObjectsByType<T>(FindObjectsInactive.Include))
            if (c.gameObject.scene == cenaDoJogo)
                achados.Add(c);
        return achados;
    }

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

    /* O grupo da hierarquia, na raiz da cena (criado se faltar). */
    private static Transform RaizDaCena(string nome)
    {
        foreach (GameObject raiz in cenaDoJogo.GetRootGameObjects())
            if (raiz.name == nome)
                return raiz.transform;

        GameObject grupo = new GameObject(nome);
        if (grupo.scene != cenaDoJogo)
            SceneManager.MoveGameObjectToScene(grupo, cenaDoJogo);
        Undo.RegisterCreatedObjectUndo(grupo, NomeUndo);
        return grupo.transform;
    }

    /* GameObject novo filho de 'pai', com pose local zerada e Undo. */
    private static GameObject NovoEm(string nome, Transform pai)
    {
        GameObject go = new GameObject(nome);
        if (go.scene != cenaDoJogo)
            SceneManager.MoveGameObjectToScene(go, cenaDoJogo);
        go.transform.SetParent(pai, false);
        Undo.RegisterCreatedObjectUndo(go, NomeUndo);
        return go;
    }

    private static string Caminho(Transform t)
    {
        return t.parent == null ? t.name : Caminho(t.parent) + "/" + t.name;
    }
}
