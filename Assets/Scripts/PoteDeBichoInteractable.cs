using UnityEngine;

/* ============================================================
 *  PoteDeBichoInteractable.cs
 *  ---------------------------------------------------------------------
 *  Foco do tipo POTE DE ÁGUA DE BICHO — herda Interactable.
 *
 *  Comportamento ("pote com água velha -> interagir -> pote lavado com
 *  água limpa"):
 *  - A vasilha do cachorro tem água velha, esverdeada, com sujeira
 *    boiando. Os ovos do mosquito ficam grudados na parede do pote.
 *  - Com as Mãos, o primeiro passo joga fora a água velha; os passos
 *    seguintes esfregam a parede; o último enche de água limpa.
 *  - O pote CONTINUA na cena, com água limpa: o certo não é tirar a água
 *    do bicho, é lavar o pote e trocar a água toda semana.
 *
 *  Minigame da esponja (vista de perto): com as Mãos, o clique começa uma
 *  SessaoDoPote (EsfregarCirculo) no lugar do segurar. A água velha vai
 *  fora na hora, a câmera para em cima do pote (alturaDaCamera acima,
 *  recuada recuoDaCamera para o lado do agente) e a parede de dentro
 *  ganha um círculo tracejado na alturaDoCirculo. Uma esponja segue o
 *  mouse; segurando o clique em cima do círculo, o trecho fica verde.
 *  Os passos que faltam se dividem pelo círculo: fechada a volta, o pote
 *  está lavado e ganha água limpa. Sair no meio guarda os passos dados;
 *  sem passo nenhum, a água velha volta (ninguém lavou nada).
 *
 *  Nascimento seco e chuva: nasce sem a água velha (ainda é foco: os ovos
 *  continuam na parede) e a chuva devolve a água velha, se ninguém
 *  começou a lavar.
 *
 *  O modelo vem do Blender (Ferramentas/Blender/gerar_lona_pote.py) e o
 *  prefab é gerado pelo menu  Dengue > Criar Pote de Bicho
 *  (FocosBlenderBuilder.cs).
 * ============================================================ */

public class PoteDeBichoInteractable : Interactable
{
    [Header("Pote de bicho")]
    [Tooltip("A água velha, esverdeada (some no primeiro passo).")]
    [SerializeField] protected GameObject aguaVelha;

    [Tooltip("A sujeira boiando na água velha (some junto com ela).")]
    [SerializeField] protected GameObject sujeira;

    [Tooltip("A água limpa (aparece ao concluir).")]
    [SerializeField] protected GameObject aguaLimpa;

    [Tooltip("Se marcado, após lavar segue o fluxo padrão (encolhe e some).")]
    [SerializeField] private bool removeOnComplete = false;

    [Header("Vista de perto (minigame da esponja)")]
    [Tooltip("Metros da câmera acima do pote.")]
    [SerializeField] private float alturaDaCamera = 0.5f;

    [Tooltip("Metros que a câmera recua do pote para o lado do agente.")]
    [SerializeField] private float recuoDaCamera = 0.2f;

    [Tooltip("Campo de visão vertical da câmera de perto, em graus.")]
    [SerializeField] private float fovDePerto = 40f;

    [Tooltip("Raio do círculo a esfregar, em metros (na parede de dentro).")]
    [SerializeField] private float raioDoCirculo = 0.092f;

    [Tooltip("Altura do círculo acima da base do pote, em metros.")]
    [SerializeField] private float alturaDoCirculo = 0.05f;

    [Tooltip("Quanto a esponja pode sair do círculo e ainda esfregar, em metros.")]
    [SerializeField] private float tolerancia = 0.03f;

    [Tooltip("Quanto a esponja anda, em metros, por grau de movimento do mouse/analógico.")]
    [SerializeField] private float metrosPorGrau = EsfregarCirculo.MetrosPorGrauPadrao;

    private const string DicaDeEsfregar = "Segure o clique e passe a esponja em volta da parede do pote   |   Botão direito: sair";
    private const string ConviteDeEsfregar = "Clique para lavar o pote com a esponja";
    private const string NomeDoVisual = "__EsfregarPote";

    // Tempo de uma volta inteira com a esponja (SegundosEstimados).
    private const float SegundosPorVolta = 6f;

    private static readonly Color CorDoTraco = new Color(1f, 1f, 1f, 0.95f);
    private static readonly Color CorDoLimpo = new Color(0.35f, 0.95f, 0.45f, 1f);
    private static readonly Color CorDaEsponja = new Color(1f, 0.83f, 0.18f);
    private static readonly Color CorDaFibra = new Color(0.18f, 0.55f, 0.25f);

    // Sessão em andamento (null = nenhuma) e o visual dela.
    private SessaoDoPote sessaoAtual;
    private GameObject visual;
    private Transform[] tracos;
    private Renderer[] rendsDosTracos;
    private Transform esponja;
    private Material matTraco, matLimpo;

    /* Sem começar a lavar, a chuva enche de novo. */
    public override bool PodeReencher => base.PodeReencher && CurrentClicks == 0;

    /* A água velha ainda está no pote. */
    public bool TemAguaVelha => aguaVelha != null && aguaVelha.activeSelf;

    /* Sorteado como já limpo, fica no mapa com água limpa (o OnComplete). */
    public override bool AparecerJaLimpo => true;

    protected override void OnStart()
    {
        MostrarAguaVelha(true);
        if (aguaLimpa != null) aguaLimpa.SetActive(false);
    }

    void OnDisable()
    {
        if (sessaoAtual != null)
            LargarSessao(true);
    }

    /* ---------------- Minigame ---------------- */

    public override IMinigame CriarMinigame(KitDoAgente kit, int semente)
    {
        if (!TemMinigame(kit))
            return null;
        return CriarSessao(kit.transform.position, kit.transform.forward);
    }

    private bool TemMinigame(KitDoAgente kit)
    {
        return kit != null && AceitaFerramenta(kit.Selecionada) && CanInteract && !kit.MaosOcupadas;
    }

    /* A sessão com os passos que faltam e a pose de cima do pote. Fábrica pura. */
    internal SessaoDoPote CriarSessao(Vector3 agente, Vector3 frenteReserva)
    {
        PoseDePerto pose = PoseDaVistaDePerto(agente, frenteReserva);
        int passos = Mathf.Max(1, RequiredClicks - CurrentClicks);
        return new SessaoDoPote(pose, raioDoCirculo, passos, tolerancia, metrosPorGrau);
    }

    /* Acima do pote, recuada para o lado do agente, olhando para o centro,
     * com o topo da tela do lado oposto ao agente (a esponja começa
     * embaixo, do lado dele). */
    internal PoseDePerto PoseDaVistaDePerto(Vector3 agente, Vector3 frenteReserva)
    {
        Vector3 centro = CentroDoCirculo();
        Vector3 frente = SemAltura(centro - agente);
        if (frente.sqrMagnitude < 1e-6f) frente = SemAltura(frenteReserva);
        if (frente.sqrMagnitude < 1e-6f) frente = Vector3.forward;
        frente.Normalize();

        Vector3 posicao = centro + Vector3.up * alturaDaCamera - frente * recuoDaCamera;
        Vector3 olhar = centro - posicao;
        return new PoseDePerto
        {
            Posicao = posicao,
            Giro = Quaternion.LookRotation(olhar.sqrMagnitude > 1e-8f ? olhar : Vector3.down, frente),
            Fov = fovDePerto
        };
    }

    private Vector3 CentroDoCirculo() => transform.position + Vector3.up * alturaDoCirculo * transform.lossyScale.y;

    private static Vector3 SemAltura(Vector3 v) => new Vector3(v.x, 0f, v.z);

    public override string ConviteDoMinigame(KitDoAgente kit) => TemMinigame(kit) ? ConviteDeEsfregar : null;

    public override string DicaDoMinigame => DicaDeEsfregar;

    public override float SegundosEstimados => SegundosPorVolta;

    /* A água velha vai fora e o círculo aparece. */
    public override void AoIniciarMinigame(IMinigame sessao)
    {
        LargarSessao(false);
        if (!(sessao is SessaoDoPote nova))
            return;

        sessaoAtual = nova;
        MostrarAguaVelha(false);
        MontarVisual();
        AtualizarVisual();
    }

    public override void AoAtualizarMinigame(IMinigame sessao)
    {
        if (sessaoAtual == null || sessao != sessaoAtual)
            return;
        AtualizarVisual();
    }

    public override void AoSairDoMinigame()
    {
        LargarSessao(true);
    }

    /* Recusado no meio, o Interactor encerra sem o AoSairDoMinigame. */
    public override ResultadoDoPasso AvancarCom(KitDoAgente kit)
    {
        ResultadoDoPasso resultado = base.AvancarCom(kit);
        if (resultado != ResultadoDoPasso.Avancou && sessaoAtual != null)
            LargarSessao(!IsComplete);
        return resultado;
    }

    /* Zera a sessão e desmonta o visual. devolverAgua: sem passo dado, a
     * água velha volta (ninguém lavou o pote). */
    private void LargarSessao(bool devolverAgua)
    {
        sessaoAtual = null;
        if (visual != null)
            Destroy(visual);
        visual = null;
        tracos = null;
        rendsDosTracos = null;
        esponja = null;

        if (devolverAgua && CurrentClicks == 0 && !Esvaziado && !IsComplete)
            MostrarAguaVelha(true);
    }

    /* Os tracinhos do círculo e a esponja, filhos de um objeto só (sem colliders). */
    private void MontarVisual()
    {
        visual = new GameObject(NomeDoVisual);
        visual.transform.SetParent(transform, false);

        Shader semLuz = Shader.Find("Sprites/Default");
        matTraco = new Material(semLuz) { color = CorDoTraco };
        matLimpo = new Material(semLuz) { color = CorDoLimpo };

        int n = sessaoAtual.Segmentos;
        tracos = new Transform[n];
        rendsDosTracos = new Renderer[n];
        for (int i = 0; i < n; i++)
        {
            GameObject t = Primitiva(PrimitiveType.Cube, "Traco" + i, visual.transform);
            tracos[i] = t.transform;
            rendsDosTracos[i] = t.GetComponent<Renderer>();
        }

        Material baseLit = null;
        Renderer qualquer = GetComponentInChildren<Renderer>();
        if (qualquer != null) baseLit = qualquer.sharedMaterial;

        esponja = new GameObject("Esponja").transform;
        esponja.SetParent(visual.transform, false);
        GameObject corpo = Primitiva(PrimitiveType.Cube, "Corpo", esponja);
        corpo.transform.localScale = new Vector3(0.046f, 0.02f, 0.028f);
        Pintar(corpo, baseLit, CorDaEsponja);
        GameObject fibra = Primitiva(PrimitiveType.Cube, "Fibra", esponja);
        fibra.transform.localPosition = new Vector3(0f, -0.014f, 0f);
        fibra.transform.localScale = new Vector3(0.046f, 0.008f, 0.028f);
        Pintar(fibra, baseLit, CorDaFibra);
    }

    private static GameObject Primitiva(PrimitiveType tipo, string nome, Transform pai)
    {
        GameObject go = GameObject.CreatePrimitive(tipo);
        go.name = nome;
        Collider c = go.GetComponent<Collider>();
        if (c != null) Destroy(c);
        go.transform.SetParent(pai, false);
        Renderer r = go.GetComponent<Renderer>();
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        return go;
    }

    private static void Pintar(GameObject go, Material baseLit, Color cor)
    {
        Renderer r = go.GetComponent<Renderer>();
        Material m = baseLit != null ? new Material(baseLit) : new Material(Shader.Find("Sprites/Default"));
        m.color = cor;
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", cor);
        r.sharedMaterial = m;
    }

    /* Ponto do plano da sessão (metros; x = direita da tela, y = cima) no mundo. */
    private Vector3 NoMundo(Vector2 p, float alturaExtra)
    {
        PoseDePerto pose = sessaoAtual.Pose;
        Vector3 direita = SemAltura(pose.Giro * Vector3.right).normalized;
        Vector3 cima = SemAltura(pose.Giro * Vector3.up).normalized;
        return CentroDoCirculo() + direita * p.x + cima * p.y + Vector3.up * alturaExtra;
    }

    private void AtualizarVisual()
    {
        if (sessaoAtual == null || visual == null)
            return;

        SessaoDoPote s = sessaoAtual;
        Vector3 direita = SemAltura(s.Pose.Giro * Vector3.right).normalized;
        for (int i = 0; i < tracos.Length; i++)
        {
            float a = s.AnguloDoSegmento(i);
            Vector2 p = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * s.Raio;
            Vector3 mundo = NoMundo(p, 0f);
            Vector3 tangente = Vector3.Cross(Vector3.up, mundo - CentroDoCirculo()).normalized;
            if (tangente.sqrMagnitude < 1e-6f) tangente = direita;

            bool limpo = s.Limpo(i);
            tracos[i].position = mundo;
            tracos[i].rotation = Quaternion.LookRotation(tangente, Vector3.up);
            tracos[i].localScale = limpo ? new Vector3(0.008f, 0.006f, 0.014f) : new Vector3(0.007f, 0.005f, 0.008f);
            rendsDosTracos[i].sharedMaterial = limpo ? matLimpo : matTraco;
            // Tracejado: no trecho sujo, um sim, um não.
            rendsDosTracos[i].enabled = limpo || i % 2 == 0;
        }

        // A esponja: em cima da parede quando está no traço, um pouco acima fora dele.
        Vector2 e = s.Esponja;
        float sobe = s.Esfregando ? 0.012f + Mathf.Sin(Time.time * 40f) * 0.002f : 0.03f;
        Vector2 dentro = e.sqrMagnitude > 1e-8f ? e - e.normalized * 0.012f : e;
        esponja.position = NoMundo(dentro, sobe);
        float giro = Mathf.Atan2(e.y, e.x) * Mathf.Rad2Deg;
        esponja.rotation = Quaternion.LookRotation(SemAltura(s.Pose.Giro * Vector3.up), Vector3.up)
                         * Quaternion.Euler(0f, -giro + 90f, s.Esfregando ? 14f : 0f);
    }

    /* ---------------- Passos e água ---------------- */

    protected override void OnInteract()
    {
        // Primeiro passo: a água velha vai fora (o resto é esfregar).
        MostrarAguaVelha(false);
        base.OnInteract();
    }

    protected override void OnComplete()
    {
        if (sessaoAtual != null)
            LargarSessao(false);
        MostrarAguaVelha(false);
        if (aguaLimpa != null) aguaLimpa.SetActive(true);

        if (removeOnComplete)
            base.OnComplete();
    }

    protected override void AoEsvaziar()
    {
        MostrarAguaVelha(false);
    }

    protected override void AoNascerVazio()
    {
        AoEsvaziar();
    }

    protected override void AoReencher()
    {
        MostrarAguaVelha(true);
    }

    protected void MostrarAguaVelha(bool sim)
    {
        if (aguaVelha != null) aguaVelha.SetActive(sim);
        if (sujeira != null) sujeira.SetActive(sim);
    }
}

/* A sessão do pote: o esfregar e a pose da câmera. */
internal sealed class SessaoDoPote : EsfregarCirculo, IMinigameDePerto
{
    public PoseDePerto Pose { get; }

    internal SessaoDoPote(PoseDePerto pose, float raio, int passos, float tolerancia, float metrosPorGrau)
        : base(raio, passos, tolerancia, SegmentosPadrao, metrosPorGrau)
    {
        Pose = pose;
    }
}
