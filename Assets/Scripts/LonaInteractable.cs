using System.Collections.Generic;
using UnityEngine;

/* ============================================================
 *  LonaInteractable.cs
 *  ---------------------------------------------------------------------
 *  Foco do tipo LONA COM POÇAS — herda Interactable.
 *
 *  Comportamento ("lona largada com água -> interagir -> lona dobrada"):
 *  - A lona está esticada no chão, presa por tijolos, com água da chuva
 *    parada nas bacias que o entulho por baixo forma (as 'pocas').
 *  - Com as Mãos, cada passo tira a água de uma poça (na ordem do array,
 *    pulando as que já estão secas); no último passo não sobra nenhuma.
 *  - Concluída, a lona esticada some e aparece a dobrada, ao lado dos
 *    tijolos: o certo é esvaziar e guardar a lona, não jogá-la fora. Os
 *    tijolos ficam.
 *
 *  Minigame de inclinar (vista de perto): com as Mãos numa lona com
 *  poça, o clique começa uma SessaoDaLona (InclinarLona) no lugar do
 *  segurar. A câmera para em cima da lona, com o −x da lona à esquerda da
 *  tela, e essa borda ganha uma faixa verde com setas: é a saída. O mouse
 *  inclina a lona (a esticada e as poças giram juntas num pivô, erguido
 *  para a borda baixa ficar rente ao chão) e as poças escorrem; cada uma
 *  que passa da borda verde some e dá um passo. As medidas saem da malha
 *  da esticada, e a altura de cada poça segue a superfície da lona
 *  (grade de alturas tirada dos vértices). Sair no meio guarda as poças
 *  que já saíram; as outras voltam para o lugar delas.
 *
 *  Nascimento seco e chuva: nasce sem poças (como se alguém já tivesse
 *  tirado a água) e a chuva devolve todas, se ninguém começou a dobrar.
 *  Sem poça, não há minigame: o segurar dobra a lona.
 *
 *  O modelo vem do Blender (Ferramentas/Blender/gerar_lona_pote.py) e o
 *  prefab é gerado pelo menu  Dengue > Criar Lona  (FocosBlenderBuilder.cs).
 * ============================================================ */

public class LonaInteractable : Interactable
{
    [Header("Lona")]
    [Tooltip("Poças d'água sobre a lona, na ordem em que saem.")]
    [SerializeField] protected GameObject[] pocas = new GameObject[0];

    [Tooltip("A lona esticada no chão (some ao concluir).")]
    [SerializeField] protected GameObject esticada;

    [Tooltip("A lona dobrada (aparece ao concluir).")]
    [SerializeField] protected GameObject dobrada;

    [Tooltip("Se marcado, após dobrar segue o fluxo padrão (encolhe e some).")]
    [SerializeField] private bool removeOnComplete = false;

    [Header("Vista de perto (minigame de inclinar)")]
    [Tooltip("Metros da câmera acima da lona.")]
    [SerializeField] private float alturaDaCamera = 2.3f;

    [Tooltip("Metros que a câmera recua para baixo da tela (para ver a lona um pouco de lado).")]
    [SerializeField] private float recuoDaCamera = 0.8f;

    [Tooltip("Campo de visão vertical da câmera de perto, em graus.")]
    [SerializeField] private float fovDePerto = 50f;

    [Tooltip("Inclinação máxima da lona, em graus.")]
    [SerializeField] private float inclinacaoMaxima = 8f;

    [Tooltip("Aceleração das poças com a lona toda inclinada, em m/s².")]
    [SerializeField] private float aceleracao = InclinarLona.AceleracaoPadrao;

    [Tooltip("Quanto a água freia sozinha (1/s).")]
    [SerializeField] private float atrito = InclinarLona.AtritoPadrao;

    [Tooltip("Quanto a lona inclina (0..1) por grau de movimento do mouse/analógico.")]
    [SerializeField] private float inclinacaoPorGrau = InclinarLona.InclinacaoPorGrauPadrao;

    [Tooltip("Quanto a lona volta sozinha a ficar reta sem o mouse (1/s).")]
    [SerializeField] private float retorno = InclinarLona.RetornoPadrao;

    private const string DicaDeInclinar = "Mova o mouse para inclinar a lona e leve as poças até a borda verde   |   Botão direito: sair";
    private const string ConviteDeInclinar = "Clique para escorrer a água da lona";
    private const string NomeDoPivo = "__InclinarLona";
    private const float FolgaDaPoca = 0.012f;
    private const int CelulasX = 15, CelulasZ = 10;

    // Tempo de levar cada poça até a borda (SegundosEstimados).
    private const float SegundosPorPoca = 2.5f;

    private static readonly Color CorDaSaida = new Color(0.35f, 0.95f, 0.45f, 1f);

    // Sessão em andamento (null = nenhuma), o pivô que inclina e o que foi mexido.
    private SessaoDaLona sessaoAtual;
    private Transform pivo;
    private int[] pocaDoIndice;                 // índice em 'pocas' de cada poça da sessão
    private readonly Dictionary<Transform, Vector3> lugarDasPocas = new Dictionary<Transform, Vector3>();
    private Vector3 lugarDaEsticada;
    private Quaternion giroDaEsticada;
    private Transform paiOriginal;   // o nó do FBX onde a esticada e as poças moram
    private Medidas medidas;

    protected GameObject[] Pocas => pocas ?? new GameObject[0];

    /* Sem começar a tirar a água, a chuva enche de novo. */
    public override bool PodeReencher => base.PodeReencher && CurrentClicks == 0;

    /* Poças ainda com água. */
    public int PocasComAgua
    {
        get
        {
            int n = 0;
            foreach (GameObject p in Pocas)
                if (p != null && p.activeSelf) n++;
            return n;
        }
    }

    /* Sorteado como já limpo, fica no mapa dobrada (o OnComplete). */
    public override bool AparecerJaLimpo => true;

    protected override void OnStart()
    {
        MostrarDobrada(false);
        foreach (GameObject p in Pocas)
            if (p != null) p.SetActive(true);
    }

    void OnDisable()
    {
        if (sessaoAtual != null)
            LargarSessao();
    }

    /* ---------------- Medidas da lona ---------------- */

    /* Retângulo da lona esticada no espaço da raiz (x, z) e a grade de
     * alturas da superfície, tirados da malha uma vez só. */
    private sealed class Medidas
    {
        public Vector2 Centro;
        public Vector2 MeiaExtensao;
        public float[,] Altura;   // [CelulasX, CelulasZ], y no espaço da raiz

        /* A maior altura da célula do ponto e das 8 em volta: a poça tem
         * quase o tamanho de uma célula e não pode ser cortada pela dobra
         * da vizinha. */
        public float AlturaEm(Vector2 p)
        {
            int nx = Altura.GetLength(0), nz = Altura.GetLength(1);
            float u = Mathf.InverseLerp(-MeiaExtensao.x, MeiaExtensao.x, p.x - Centro.x);
            float v = Mathf.InverseLerp(-MeiaExtensao.y, MeiaExtensao.y, p.y - Centro.y);
            int ci = Mathf.Clamp((int)(u * nx), 0, nx - 1), cj = Mathf.Clamp((int)(v * nz), 0, nz - 1);
            float maior = float.MinValue;
            for (int i = Mathf.Max(0, ci - 1); i <= Mathf.Min(nx - 1, ci + 1); i++)
                for (int j = Mathf.Max(0, cj - 1); j <= Mathf.Min(nz - 1, cj + 1); j++)
                    maior = Mathf.Max(maior, Altura[i, j]);
            return maior;
        }
    }

    private Medidas Medir()
    {
        if (medidas != null)
            return medidas;

        MeshFilter mf = esticada != null ? esticada.GetComponent<MeshFilter>() : null;
        Mesh malha = mf != null ? mf.sharedMesh : null;
        Vector3[] vs = malha != null && malha.isReadable ? malha.vertices : null;
        if (vs == null || vs.Length == 0)
        {
            medidas = new Medidas { Centro = Vector2.zero, MeiaExtensao = new Vector2(1f, 0.7f), Altura = new float[1, 1] };
            return medidas;
        }

        Vector3[] naRaiz = new Vector3[vs.Length];
        Vector2 min = new Vector2(float.MaxValue, float.MaxValue), max = new Vector2(float.MinValue, float.MinValue);
        for (int k = 0; k < vs.Length; k++)
        {
            Vector3 r = transform.InverseTransformPoint(esticada.transform.TransformPoint(vs[k]));
            naRaiz[k] = r;
            min = Vector2.Min(min, new Vector2(r.x, r.z));
            max = Vector2.Max(max, new Vector2(r.x, r.z));
        }

        Medidas m = new Medidas
        {
            Centro = (min + max) * 0.5f,
            MeiaExtensao = (max - min) * 0.5f,
            Altura = new float[CelulasX, CelulasZ]
        };
        for (int i = 0; i < CelulasX; i++)
            for (int j = 0; j < CelulasZ; j++)
                m.Altura[i, j] = float.MinValue;
        foreach (Vector3 r in naRaiz)
        {
            float u = Mathf.InverseLerp(min.x, max.x, r.x), v = Mathf.InverseLerp(min.y, max.y, r.z);
            int i = Mathf.Clamp((int)(u * CelulasX), 0, CelulasX - 1), j = Mathf.Clamp((int)(v * CelulasZ), 0, CelulasZ - 1);
            m.Altura[i, j] = Mathf.Max(m.Altura[i, j], r.y);
        }
        for (int i = 0; i < CelulasX; i++)
            for (int j = 0; j < CelulasZ; j++)
                if (m.Altura[i, j] == float.MinValue) m.Altura[i, j] = 0f;

        medidas = m;
        return medidas;
    }

    /* ---------------- Minigame ---------------- */

    public override IMinigame CriarMinigame(KitDoAgente kit, int semente)
    {
        if (!TemMinigame(kit))
            return null;
        return CriarSessao();
    }

    private bool TemMinigame(KitDoAgente kit)
    {
        return kit != null && AceitaFerramenta(kit.Selecionada) && CanInteract && !kit.MaosOcupadas && PocasComAgua > 0;
    }

    /* A sessão com as poças com água, no plano da lona (centro em 0). Fábrica pura. */
    internal SessaoDaLona CriarSessao()
    {
        Medidas m = Medir();
        List<Vector2> posicoes = new List<Vector2>();
        List<int> indices = new List<int>();
        GameObject[] lista = Pocas;
        for (int i = 0; i < lista.Length; i++)
        {
            if (lista[i] == null || !lista[i].activeSelf)
                continue;
            Vector3 r = transform.InverseTransformPoint(lista[i].transform.position);
            posicoes.Add(new Vector2(r.x, r.z) - m.Centro);
            indices.Add(i);
        }

        int passos = Mathf.Max(1, RequiredClicks - CurrentClicks);
        return new SessaoDaLona(PoseDaVistaDePerto(), indices.ToArray(), posicoes.ToArray(), m.MeiaExtensao, passos,
                                aceleracao, atrito, inclinacaoPorGrau, retorno);
    }

    /* Acima do centro da lona, recuada para o −z dela, com o +x da lona à
     * direita da tela e o +z para cima (a saída, o −x, fica à esquerda). */
    internal PoseDePerto PoseDaVistaDePerto()
    {
        Medidas m = Medir();
        Vector3 centro = transform.TransformPoint(new Vector3(m.Centro.x, 0f, m.Centro.y));
        Vector3 frente = SemAltura(transform.forward);
        if (frente.sqrMagnitude < 1e-6f) frente = Vector3.forward;
        frente.Normalize();

        Vector3 posicao = centro + Vector3.up * alturaDaCamera - frente * recuoDaCamera;
        return new PoseDePerto
        {
            Posicao = posicao,
            Giro = Quaternion.LookRotation(centro - posicao, frente),
            Fov = fovDePerto
        };
    }

    private static Vector3 SemAltura(Vector3 v) => new Vector3(v.x, 0f, v.z);

    public override string ConviteDoMinigame(KitDoAgente kit) => TemMinigame(kit) ? ConviteDeInclinar : null;

    public override string DicaDoMinigame => DicaDeInclinar;

    public override float SegundosEstimados => SegundosPorPoca * Mathf.Max(1, Pocas.Length);

    public override void AoIniciarMinigame(IMinigame sessao)
    {
        LargarSessao();
        if (!(sessao is SessaoDaLona nova))
            return;

        sessaoAtual = nova;
        pocaDoIndice = nova.Pocas;
        MontarPivo();
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
        LargarSessao();
    }

    /* Recusado no meio, o Interactor encerra sem o AoSairDoMinigame. */
    public override ResultadoDoPasso AvancarCom(KitDoAgente kit)
    {
        ResultadoDoPasso resultado = base.AvancarCom(kit);
        if (resultado != ResultadoDoPasso.Avancou && sessaoAtual != null)
            LargarSessao();
        return resultado;
    }

    /* A esticada e as poças passam para um pivô no centro da lona, que
     * inclina; a faixa verde da saída e as setas vão junto. */
    private void MontarPivo()
    {
        Medidas m = Medir();
        pivo = new GameObject(NomeDoPivo).transform;
        pivo.SetParent(transform, false);
        pivo.localPosition = new Vector3(m.Centro.x, 0f, m.Centro.y);

        lugarDasPocas.Clear();
        paiOriginal = esticada != null ? esticada.transform.parent : transform;
        if (esticada != null)
        {
            lugarDaEsticada = esticada.transform.localPosition;
            giroDaEsticada = esticada.transform.localRotation;
            esticada.transform.SetParent(pivo, true);
        }
        foreach (GameObject p in Pocas)
        {
            if (p == null) continue;
            lugarDasPocas[p.transform] = p.transform.localPosition;
            p.transform.SetParent(pivo, true);
        }

        Material verde = new Material(Shader.Find("Sprites/Default")) { color = CorDaSaida };
        float x = -m.MeiaExtensao.x;
        Marca("Saida", new Vector3(x, 0.03f, 0f), new Vector3(0.05f, 0.02f, m.MeiaExtensao.y * 2f), 0f, verde);
        for (int k = -1; k <= 1; k++)
            Marca("Seta" + (k + 1), new Vector3(x - 0.15f, 0.02f, k * m.MeiaExtensao.y * 0.6f),
                  new Vector3(0.09f, 0.01f, 0.09f), 45f, verde);
    }

    private void Marca(string nome, Vector3 local, Vector3 escala, float giro, Material mat)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = nome;
        Collider c = go.GetComponent<Collider>();
        if (c != null) Destroy(c);
        go.transform.SetParent(pivo, false);
        go.transform.localPosition = local;
        go.transform.localRotation = Quaternion.Euler(0f, giro, 0f);
        go.transform.localScale = escala;
        Renderer r = go.GetComponent<Renderer>();
        r.sharedMaterial = mat;
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }

    private void AtualizarVisual()
    {
        if (sessaoAtual == null || pivo == null)
            return;

        SessaoDaLona s = sessaoAtual;
        Medidas m = Medir();

        // Inclinação para o lado que desce; o pivô sobe o que afundaria.
        float az = -s.Inclinacao.x * inclinacaoMaxima;   // −x desce com Inclinacao.x < 0
        float ax = s.Inclinacao.y * inclinacaoMaxima;    // +z desce com Inclinacao.y > 0
        pivo.localRotation = Quaternion.Euler(ax, 0f, az);
        float sobe = m.MeiaExtensao.x * Mathf.Abs(Mathf.Sin(az * Mathf.Deg2Rad))
                   + m.MeiaExtensao.y * Mathf.Abs(Mathf.Sin(ax * Mathf.Deg2Rad));
        pivo.localPosition = new Vector3(m.Centro.x, sobe, m.Centro.y);

        GameObject[] lista = Pocas;
        for (int k = 0; k < pocaDoIndice.Length; k++)
        {
            GameObject poca = lista[pocaDoIndice[k]];
            if (poca == null) continue;
            if (s.Saiu(k))
            {
                if (poca.activeSelf) poca.SetActive(false);
                continue;
            }
            Vector2 p = s.Posicao(k);
            float y = m.AlturaEm(p + m.Centro) + FolgaDaPoca;
            poca.transform.localPosition = new Vector3(p.x, y, p.y);
        }
    }

    /* Tira o pivô: a esticada e as poças voltam para o pai de antes (o nó
     * do FBX), as que não saíram no lugar delas. */
    private void LargarSessao()
    {
        sessaoAtual = null;
        pocaDoIndice = null;
        if (pivo == null)
            return;

        if (esticada != null)
        {
            esticada.transform.SetParent(paiOriginal, false);
            esticada.transform.localPosition = lugarDaEsticada;
            esticada.transform.localRotation = giroDaEsticada;
        }
        foreach (KeyValuePair<Transform, Vector3> par in lugarDasPocas)
        {
            if (par.Key == null) continue;
            par.Key.SetParent(paiOriginal, false);
            par.Key.localRotation = Quaternion.identity;
            par.Key.localPosition = par.Value;
        }
        lugarDasPocas.Clear();
        Destroy(pivo.gameObject);
        pivo = null;
    }

    /* ---------------- Passos e água ---------------- */

    protected override void OnInteract()
    {
        // Com a sessão, quem tira as poças é o escorrer (AtualizarVisual).
        if (sessaoAtual == null)
        {
            // Distribui as poças pelos passos; no último, tira o que sobrou.
            GameObject[] lista = Pocas;
            int alvo = RequiredClicks <= 0
                ? lista.Length
                : Mathf.Min(lista.Length, Mathf.CeilToInt((float)lista.Length * CurrentClicks / RequiredClicks));

            int secas = lista.Length - PocasComAgua;
            for (int i = 0; i < lista.Length && secas < alvo; i++)
            {
                if (lista[i] == null || !lista[i].activeSelf)
                    continue;
                lista[i].SetActive(false);
                secas++;
            }
        }

        base.OnInteract();
    }

    protected override void OnComplete()
    {
        LargarSessao();
        foreach (GameObject p in Pocas)
            if (p != null) p.SetActive(false);
        MostrarDobrada(true);

        if (removeOnComplete)
            base.OnComplete();
    }

    protected override void AoEsvaziar()
    {
        foreach (GameObject p in Pocas)
            if (p != null) p.SetActive(false);
    }

    protected override void AoNascerVazio()
    {
        AoEsvaziar();
    }

    protected override void AoReencher()
    {
        foreach (GameObject p in Pocas)
            if (p != null) p.SetActive(true);
    }

    private void MostrarDobrada(bool sim)
    {
        if (esticada != null) esticada.SetActive(!sim);
        if (dobrada != null) dobrada.SetActive(sim);
    }
}

/* A sessão da lona: o inclinar, a pose da câmera e qual poça é cada uma. */
internal sealed class SessaoDaLona : InclinarLona, IMinigameDePerto
{
    public PoseDePerto Pose { get; }
    internal int[] Pocas { get; }   // Pocas[k] = índice em LonaInteractable.pocas da poça k

    internal SessaoDaLona(PoseDePerto pose, int[] pocas, Vector2[] posicoes, Vector2 meiaExtensao, int passos,
                          float aceleracao, float atrito, float inclinacaoPorGrau, float retorno)
        : base(posicoes, meiaExtensao, passos, 0.1f, aceleracao, atrito, inclinacaoPorGrau, retorno)
    {
        Pose = pose;
        Pocas = pocas ?? new int[0];
    }
}
