using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* ============================================================
 *  SomDosFocos.cs
 *  ---------------------------------------------------------------------
 *  Todo o som dos focos de criadouro (no objeto "Sons" da cena):
 *
 *  - ZUMBIDO: um loop 3D de mosquito no centro de cada foco ainda ativo
 *    (é assim que o jogador acha os focos pelo ouvido, GameDoc). Só os
 *    maxZumbidosJuntos focos MAIS PERTO (padrão: 1) zumbem — pedido do
 *    usuário, "menos sons de mosquitos". Trocar de foco é um fade (peso),
 *    não um corte. Curva de distância própria: cheio até distanciaMinima e
 *    ZERO em distanciaMaxima. Some em duracaoSumir ao eliminar. O volume
 *    sobe com o estágio do foco no GerenciadorDeFocos (volumePorEstagio:
 *    ovo mais baixo, mosquito cheio), em rampa: trocar de estágio não dá
 *    degrau no volume. O foco pode mudar de lugar (o pneu largado longe e
 *    reenchido pela chuva): a fonte vai junto (OnFocoReenchido).
 *    Limpar um foco (esvaziado ou eliminado) cala TODOS os zumbidos por
 *    silencioAposLimpar segundos: há focos a menos de 1 m um do outro, e o
 *    vizinho entrando na hora parecia o foco limpo ainda zumbindo.
 *  - CLIQUE: som do tipo do foco (tabela sonsPorTipo), 3D, no foco.
 *  - ESVAZIAR: o splash da água saindo (OnFocoEsvaziado), 3D, no foco. O
 *    clique e o splash saem de onde o foco está naquela hora, não de onde
 *    ele nasceu: as fontes do pool só alcançam 30 m.
 *  - ELIMINADO: whoosh + brilho (2D) e, logo depois, a "confirmação" de que
 *    a ocupação do hospital caiu (não toca no último foco: é a vitória).
 *
 *  Os focos, os cliques, os esvaziados e as eliminações vêm do
 *  GerenciadorDeFocos (do campo ou, vazio, GerenciadorDeFocos.Obter()).
 *
 *  Os objetos de áudio nascem em runtime como filhos DESTE objeto (não dos
 *  focos: nada muda na hierarquia deles). Montado por Dengue > Configurar Sons.
 * ============================================================ */

public class SomDosFocos : MonoBehaviour
{
    [System.Serializable]
    public class SomPorTipo
    {
        public TipoFoco tipo;
        [Tooltip("Um é sorteado a cada clique (sem repetir o anterior).")]
        public AudioClip[] cliques;
        [Tooltip("Opcional: toca no 1º clique no lugar dos de cima (vaso: tira a água, depois põe areia).")]
        public AudioClip primeiroClique;
        [Range(0.5f, 2f)] public float pitch = 1f;
        [Range(0f, 1f)] public float volume = 1f;

        [System.NonSerialized] public int ultimo = -1;
    }

    [Header("Zumbido (loop 3D em cada foco)")]
    [SerializeField] private AudioClip zumbido;
    [Range(0f, 1f)]
    [SerializeField] private float volumeZumbido = 0.35f;
    [Tooltip("Até esta distância (m) o zumbido fica no volume cheio.")]
    [SerializeField] private float distanciaMinima = 0.8f;
    [Tooltip("Distância (m) em que o zumbido chega a zero. Mais longe, a fonte nem toca.")]
    [SerializeField] private float distanciaMaxima = 7f;
    [Tooltip("Quantos focos (os mais perto) podem zumbir ao mesmo tempo.")]
    [Min(0)]
    [SerializeField] private int maxZumbidosJuntos = 1;
    [Tooltip("Segundos para um zumbido entrar ou sair quando o foco mais perto muda.")]
    [SerializeField] private float duracaoTroca = 0.5f;
    [Tooltip("Tempo (s) para o zumbido sumir quando o foco é eliminado.")]
    [SerializeField] private float duracaoSumir = 0.4f;
    [Tooltip("Segundos sem nenhum zumbido depois de limpar um foco (o vizinho não entra na hora).")]
    [Min(0f)]
    [SerializeField] private float silencioAposLimpar = 3f;
    [Tooltip("Multiplicador do volume do zumbido por estágio do foco: x = ovo, y = larva, z = mosquito (quanto mais perto de virar mosquito, mais alto).")]
    [SerializeField] private Vector3 volumePorEstagio = new Vector3(0.45f, 0.7f, 1.0f);

    [Header("Cliques")]
    [SerializeField] private List<SomPorTipo> sonsPorTipo = new List<SomPorTipo>();
    [Tooltip("Usado quando o tipo do foco não está na tabela.")]
    [SerializeField] private TipoFoco tipoPadrao = TipoFoco.Pneu;
    [Range(0f, 1f)]
    [SerializeField] private float volumeCliques = 0.8f;
    [Tooltip("Variação aleatória do pitch a cada clique (0.06 = ±6%).")]
    [SerializeField] private float variacaoPitch = 0.06f;

    [Header("Foco esvaziado")]
    [Tooltip("Splash da água saindo, no foco, quando ele fica sem água. Vazio = sem som.")]
    [SerializeField] private AudioClip esvaziar;
    [Tooltip("Volume do splash ao esvaziar.")]
    [Range(0f, 1f)]
    [SerializeField] private float volumeEsvaziar = 0.8f;

    [Header("Foco eliminado")]
    [SerializeField] private AudioClip eliminado;
    [Range(0f, 1f)]
    [SerializeField] private float volumeEliminado = 0.7f;
    [Tooltip("Confirmação de que a ocupação do hospital caiu.")]
    [SerializeField] private AudioClip ocupacaoDesce;
    [Range(0f, 1f)]
    [SerializeField] private float volumeOcupacaoDesce = 0.45f;
    [SerializeField] private float atrasoOcupacaoDesce = 0.45f;

    [Tooltip("Vazio = procura na cena.")]
    [SerializeField] private FluxoDeJogo fluxo;

    [Tooltip("Focos da partida (lista, cliques, eliminados e estágio de cada um). Vazio = GerenciadorDeFocos.Obter().")]
    [SerializeField] private GerenciadorDeFocos gerenciador;

    private class Zumbido
    {
        public Interactable foco;
        public CanalDeAudio canal;
        public bool sumindo;
        public float peso;        // 0 a 1: entra/sai suave
        public float distancia;   // ao ouvinte, neste frame
        public float fator;       // do estágio, em rampa (sem degrau no volume)
    }

    private const int TamanhoPool = 4;

    private readonly List<Zumbido> zumbidos = new List<Zumbido>();
    private readonly List<Zumbido> perto = new List<Zumbido>();
    private CanalDeAudio[] pool;
    private int proximoPool;
    private CanalDeAudio fonte2D;
    private Transform ouvinte;
    private float silencioAte;   // Time.time até quando nenhum foco zumbe

    /* Um foco acabou de ser limpo: nenhum zumbido entra agora. */
    internal bool ZumbidosEmSilencio => Time.time < silencioAte;

    void Start()
    {
        Ligar();
    }

    /* O corpo do Start (chamável no EditMode, onde ele não roda): acha as
     * referências, cria as fontes e assina o gerenciador. Uma vez só. */
    internal void Ligar()
    {
        if (fluxo == null) fluxo = FindAnyObjectByType<FluxoDeJogo>();
        if (gerenciador == null) gerenciador = GerenciadorDeFocos.Obter();
        AudioListener listener = FindAnyObjectByType<AudioListener>();
        ouvinte = listener != null ? listener.transform : (Camera.main != null ? Camera.main.transform : null);

        pool = new CanalDeAudio[TamanhoPool];
        for (int i = 0; i < TamanhoPool; i++)
        {
            pool[i] = CanalDeAudio.Criar(transform, "Clique " + (i + 1), CanalSom.Efeitos, true);
            pool[i].Fonte.minDistance = 1.5f;
            pool[i].Fonte.maxDistance = 30f;
            pool[i].Fonte.priority = 100;
        }
        fonte2D = CanalDeAudio.Criar(transform, "Eliminado", CanalSom.Efeitos, false);
        fonte2D.Fonte.priority = 64;

        Transform raiz = new GameObject("Zumbidos").transform;
        raiz.SetParent(transform, false);
        AnimationCurve curva = CurvaZumbido();

        // Os focos vêm do gerenciador (sem ele, nenhum foco: nada zumbe).
        if (gerenciador == null)
            return;

        gerenciador.Garantir();
        foreach (Interactable foco in gerenciador.Focos)
        {
            if (foco == null || foco.IsComplete)
                continue;
            if (zumbido != null)
                zumbidos.Add(CriarZumbido(foco, raiz, curva));
        }
        gerenciador.OnFocoClicado += Clicado;
        gerenciador.OnFocoEsvaziado += Esvaziado;
        gerenciador.OnFocoReenchido += Reenchido;
        gerenciador.OnFocoEliminado += Concluido;
    }

    /* Só pelo campo: procurar ou criar um gerenciador aqui (fim do Play,
     * troca de cena) deixaria objeto novo para trás. */
    void OnDestroy()
    {
        if (gerenciador == null)
            return;
        gerenciador.OnFocoClicado -= Clicado;
        gerenciador.OnFocoEsvaziado -= Esvaziado;
        gerenciador.OnFocoReenchido -= Reenchido;
        gerenciador.OnFocoEliminado -= Concluido;
    }

    void Update()
    {
        if (ouvinte == null)
            return;

        // Os focos dentro do alcance, do mais perto para o mais longe.
        Vector3 pos = ouvinte.position;
        perto.Clear();
        foreach (Zumbido z in zumbidos)
        {
            if (z.sumindo || z.canal == null || z.foco == null || !z.foco.gameObject.activeInHierarchy || !z.foco.Zumbe)
                continue;
            z.distancia = Vector3.Distance(z.canal.transform.position, pos);
            if (z.distancia < distanciaMaxima)
                perto.Add(z);
        }
        perto.Sort((a, b) => a.distancia.CompareTo(b.distancia));

        float passo = Time.deltaTime / Mathf.Max(0.01f, duracaoTroca);
        foreach (Zumbido z in zumbidos)
        {
            if (z.sumindo || z.canal == null)
                continue;
            int lugar = perto.IndexOf(z);
            float alvo = !ZumbidosEmSilencio && lugar >= 0 && lugar < maxZumbidosJuntos ? 1f : 0f;
            z.peso = Mathf.MoveTowards(z.peso, alvo, passo);
            z.fator = Mathf.MoveTowards(z.fator, FatorDe(z.foco), passo);
            z.canal.Volume = volumeZumbido * z.peso * z.fator;

            AudioSource f = z.canal.Fonte;
            if (z.peso > 0f && !f.isPlaying)
            {
                f.Play();
                f.time = Random.Range(0f, f.clip.length * 0.9f);   // cada foco num ponto do loop
            }
            else if (z.peso <= 0f && f.isPlaying)
            {
                f.Stop();
            }
        }
    }

    /* ---------------- zumbido ---------------- */

    private Zumbido CriarZumbido(Interactable foco, Transform raiz, AnimationCurve curva)
    {
        CanalDeAudio c = CanalDeAudio.Criar(raiz, "Zumbido " + foco.name, CanalSom.Efeitos, true);
        c.transform.position = CentroDe(foco);
        AudioSource f = c.Fonte;
        f.clip = zumbido;
        f.loop = true;
        f.priority = 200;   // o primeiro a perder a voz se faltar
        f.pitch = Random.Range(0.93f, 1.08f);
        f.rolloffMode = AudioRolloffMode.Custom;
        f.maxDistance = distanciaMaxima;
        f.SetCustomCurve(AudioSourceCurveType.CustomRolloff, curva);
        c.Volume = 0f;   // o Update sobe o peso do mais perto
        return new Zumbido { foco = foco, canal = c, fator = FatorDe(foco) };
    }

    /* Volume × distância/maxDistance: cheio até distanciaMinima, cai como 1/d
     * (o ouvido acha a fonte) e chega a zero na borda — sem corte seco. */
    private AnimationCurve CurvaZumbido()
    {
        float inicio = Mathf.Clamp01(distanciaMinima / Mathf.Max(distanciaMaxima, 0.01f));
        AnimationCurve curva = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(inicio, 1f));
        const int passos = 10;
        for (int i = 1; i <= passos; i++)
        {
            float x = Mathf.Lerp(inicio, 1f, i / (float)passos);
            float v = Mathf.Min(1f, inicio / x) * (1f - x * x);
            curva.AddKey(x, v);
        }
        for (int i = 0; i < curva.length; i++)
            curva.SmoothTangents(i, 0f);
        return curva;
    }

    private IEnumerator Sumir(Zumbido z)
    {
        z.sumindo = true;
        if (z.canal == null)
            yield break;
        float inicial = z.canal.Volume;
        for (float t = 0f; t < duracaoSumir; t += Time.deltaTime)
        {
            if (z.canal == null) yield break;
            z.canal.Volume = inicial * (1f - t / duracaoSumir);
            yield return null;
        }
        if (z.canal != null)
        {
            z.canal.Fonte.Stop();
            z.canal.Volume = 0f;
        }
        z.peso = 0f;
    }

    /* Multiplicador do volume do zumbido pelo estágio do foco: ovo = x,
     * larva = y, mosquito = z da tabela (volumePorEstagio). */
    internal static float FatorDoEstagio(EstagioDoFoco e, Vector3 tabela)
    {
        switch (e)
        {
            case EstagioDoFoco.Larva: return tabela.y;
            case EstagioDoFoco.Mosquito: return tabela.z;
            default: return tabela.x;
        }
    }

    /* Fator do estágio atual do foco (sem gerenciador, 1: volume cheio). */
    private float FatorDe(Interactable foco)
    {
        return gerenciador != null ? FatorDoEstagio(gerenciador.EstagioDe(foco), volumePorEstagio) : 1f;
    }

    /* ---------------- cliques e eliminação ---------------- */

    private void Clicado(Interactable foco)
    {
        SomPorTipo som = SomDe(HistoriaRedeSocial.TipoDoFoco(foco));
        if (som == null)
            return;

        AudioClip clip = TocaPrimeiroClique(foco) && som.primeiroClique != null ? som.primeiroClique : Sortear(som);
        if (clip == null)
            return;

        TocarNoFoco(foco, clip, som.pitch, som.volume * volumeCliques);
    }

    /* O primeiroClique é "tira a água, depois põe a ação": só no 1º passo de
     * um foco que ainda tem água. No esvaziado, a água já saiu. */
    internal static bool TocaPrimeiroClique(Interactable foco)
    {
        return foco.CurrentClicks == 1 && !foco.Esvaziado;
    }

    /* OnFocoEsvaziado do gerenciador: a água saiu. */
    private void Esvaziado(Interactable foco)
    {
        Silenciar();
        if (esvaziar != null)
            TocarNoFoco(foco, esvaziar, 1f, volumeEsvaziar);
    }

    private void Silenciar()
    {
        silencioAte = Time.time + silencioAposLimpar;
    }

    /* OnFocoReenchido do gerenciador: a água voltou, e o foco volta a zumbir
     * onde ele está agora. */
    private void Reenchido(Interactable foco)
    {
        CentroAtual(foco);
    }

    /* Um som 3D no centro do foco, na próxima fonte do pool. */
    private void TocarNoFoco(Interactable foco, AudioClip clip, float pitch, float volume)
    {
        CanalDeAudio c = pool[proximoPool];
        proximoPool = (proximoPool + 1) % pool.Length;
        c.transform.position = CentroAtual(foco);
        c.Fonte.pitch = pitch * (1f + Random.Range(-variacaoPitch, variacaoPitch));
        c.Fonte.PlayOneShot(clip, volume);
    }

    /* OnFocoEliminado do gerenciador (uma vez por foco). */
    private void Concluido(Interactable foco)
    {
        Silenciar();
        Zumbido z = zumbidos.Find(x => x.foco == foco);
        if (z != null && !z.sumindo)
            StartCoroutine(Sumir(z));

        if (eliminado != null)
            fonte2D.Fonte.PlayOneShot(eliminado, volumeEliminado);
        if (ocupacaoDesce != null)
            StartCoroutine(TocarOcupacaoDesce());
    }

    private IEnumerator TocarOcupacaoDesce()
    {
        yield return new WaitForSeconds(atrasoOcupacaoDesce);
        if (fluxo != null && fluxo.PartidaEncerrada)
            yield break;   // foi o último: quem toca agora é a vitória
        fonte2D.Fonte.PlayOneShot(ocupacaoDesce, volumeOcupacaoDesce);
    }

    private SomPorTipo SomDe(TipoFoco tipo)
    {
        SomPorTipo padrao = null;
        foreach (SomPorTipo s in sonsPorTipo)
        {
            if (s.tipo == tipo) return s;
            if (s.tipo == tipoPadrao) padrao = s;
        }
        return padrao;
    }

    private static AudioClip Sortear(SomPorTipo som)
    {
        if (som.cliques == null || som.cliques.Length == 0)
            return null;
        int i = Random.Range(0, som.cliques.Length);
        if (som.cliques.Length > 1 && i == som.ultimo)
            i = (i + 1) % som.cliques.Length;
        som.ultimo = i;
        return som.cliques[i];
    }

    /* O centro do foco agora (ele pode ter mudado de lugar). A fonte do
     * zumbido dele, parada no mundo, vai junto. */
    private Vector3 CentroAtual(Interactable foco)
    {
        Vector3 centro = CentroDe(foco);
        Zumbido z = zumbidos.Find(x => x.foco == foco);
        if (z != null && z.canal != null)
            z.canal.transform.position = centro;
        return centro;
    }

    /* Centro visual do foco (o pivô costuma estar no chão). */
    private static Vector3 CentroDe(Interactable foco)
    {
        bool tem = false;
        Bounds b = default;
        foreach (Renderer r in foco.GetComponentsInChildren<Renderer>())
        {
            if (r is ParticleSystemRenderer)
                continue;
            if (!tem)
            {
                b = r.bounds;
                tem = true;
            }
            else
            {
                b.Encapsulate(r.bounds);
            }
        }
        return tem ? b.center : foco.transform.position;
    }
}
