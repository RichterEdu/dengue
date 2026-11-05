using UnityEngine;

/* ============================================================
 *  SomDoJogador.cs
 *  ---------------------------------------------------------------------
 *  Passos, pulo e aterrissagem do agente de saúde (no Player).
 *
 *  - PASSOS no mesmo ritmo do balanço da câmera: toca quando a fase do
 *    CameraJogador passa por π/2 + kπ (o pé no chão, câmera no ponto mais
 *    baixo). Só no chão e andando. Mais alto correndo.
 *  - Piso dentro do hospital; rua depois que a partida começa (o jogador
 *    sai do hospital uma vez só, pelo SaidaDoHospital).
 *  - Pulo: OnPulou. Aterrissagem: OnAterrissou (só dispara acima de 3 m/s);
 *    quedas menores (degrau, pulinho) viram um passo ao tocar o chão.
 *
 *  - PassoPesado (carregando pneu, FeedbackDeCarga): os passosPesados no
 *    lugar dos normais, mais altos. Sem esses clipes, o passo normal mais
 *    alto e mais grave.
 *
 *  Som 2D: o corpo está colado no AudioListener (câmera).
 * ============================================================ */

[RequireComponent(typeof(ControladorJogador))]
public class SomDoJogador : MonoBehaviour
{
    [Header("Passos")]
    [SerializeField] private AudioClip[] passosRua;
    [SerializeField] private AudioClip[] passosPiso;
    [Range(0f, 1f)]
    [SerializeField] private float volumeAndando = 0.35f;
    [Range(0f, 1f)]
    [SerializeField] private float volumeCorrendo = 0.6f;
    [Tooltip("Variação aleatória do pitch de cada passo (0.08 = ±8%).")]
    [SerializeField] private float variacaoPitch = 0.08f;
    [Tooltip("Abaixo desta velocidade (m/s) não toca passo.")]
    [SerializeField] private float velocidadeMinima = 0.3f;

    [Header("Passo pesado (carregando pneu)")]
    [Tooltip("Passos com o pneu nos braços, no lugar dos normais. Vazio = os normais, mais graves (multiplicadorPitchPesado).")]
    [SerializeField] private AudioClip[] passosPesados;
    [Tooltip("Multiplica o volume de cada passo com o PassoPesado.")]
    [SerializeField] private float multiplicadorVolumePesado = 1.3f;
    [Tooltip("Multiplica o pitch do passo normal com o PassoPesado, sem clipes pesados (abaixo de 1 = mais grave).")]
    [SerializeField] private float multiplicadorPitchPesado = 0.85f;

    [Header("Pulo e aterrissagem")]
    [SerializeField] private AudioClip pulo;
    [Range(0f, 1f)]
    [SerializeField] private float volumePulo = 0.35f;
    [SerializeField] private AudioClip aterrissagem;
    [Tooltip("Volume da aterrissagem com queda de 3 m/s (x) e de 12 m/s ou mais (y).")]
    [SerializeField] private Vector2 volumeAterrissagem = new Vector2(0.4f, 0.85f);
    [Tooltip("Tempo no ar (s) a partir do qual uma queda pequena toca um passo ao tocar o chão.")]
    [SerializeField] private float tempoNoArMinimo = 0.2f;

    [Header("Referências (vazio = procura na cena)")]
    [SerializeField] private CameraJogador cameraJogador;
    [SerializeField] private FluxoDeJogo fluxo;

    private ControladorJogador jogador;
    private CanalDeAudio fontePassos;
    private CanalDeAudio fonteCorpo;
    private float ultimaFase;
    private int ultimoPasso = -1;
    private bool estavaNoChao = true;
    private float tempoNoArAntes;
    private bool aterrissouForte;

    /* Carregando pneu (FeedbackDeCarga): os passosPesados, com volume ×
     * multiplicadorVolumePesado (sem eles, o passo normal com pitch ×
     * multiplicadorPitchPesado). */
    public bool PassoPesado { get; set; }

    void Awake()
    {
        jogador = GetComponent<ControladorJogador>();
        fontePassos = CanalDeAudio.Criar(transform, "Som Passos", CanalSom.Efeitos, false);
        fonteCorpo = CanalDeAudio.Criar(transform, "Som Corpo", CanalSom.Efeitos, false);
        fontePassos.Fonte.priority = 60;
        fonteCorpo.Fonte.priority = 60;
    }

    void Start()
    {
        if (cameraJogador == null) cameraJogador = FindAnyObjectByType<CameraJogador>();
        if (fluxo == null) fluxo = FindAnyObjectByType<FluxoDeJogo>();
        if (cameraJogador != null) ultimaFase = cameraJogador.FasePasso;
    }

    void OnEnable()
    {
        jogador.OnPulou += Pulou;
        jogador.OnAterrissou += Aterrissou;
    }

    void OnDisable()
    {
        jogador.OnPulou -= Pulou;
        jogador.OnAterrissou -= Aterrissou;
    }

    void LateUpdate()
    {
        bool noChao = jogador.NoChao;
        if (noChao && !estavaNoChao && !aterrissouForte && tempoNoArAntes >= tempoNoArMinimo)
            TocarPasso(volumeAndando);
        if (!noChao)
            tempoNoArAntes = jogador.TempoNoAr;
        estavaNoChao = noChao;
        aterrissouForte = false;

        if (cameraJogador == null)
            return;

        // Quantas vezes a fase passou por π/2 + kπ desde o último frame.
        float fase = cameraJogador.FasePasso;
        int antes = Mathf.FloorToInt((ultimaFase - Mathf.PI * 0.5f) / Mathf.PI);
        int agora = Mathf.FloorToInt((fase - Mathf.PI * 0.5f) / Mathf.PI);
        ultimaFase = fase;

        float vel = jogador.VelocidadeHorizontal;
        if (agora > antes && noChao && vel > velocidadeMinima)
        {
            float corrida = Mathf.InverseLerp(jogador.VelocidadeAndar, jogador.VelocidadeCorrer, vel);
            TocarPasso(Mathf.Lerp(volumeAndando, volumeCorrendo, corrida));
        }
    }

    /* Os passos de agora: pesados, de rua ou de piso. */
    internal AudioClip[] ListaDePassos()
    {
        if (PassoPesado && passosPesados != null && passosPesados.Length > 0)
            return passosPesados;

        bool naRua = fluxo == null || fluxo.PartidaIniciada;
        return naRua || passosPiso == null || passosPiso.Length == 0 ? passosRua : passosPiso;
    }

    private void TocarPasso(float volume)
    {
        AudioClip[] lista = ListaDePassos();
        if (lista == null || lista.Length == 0)
            return;

        int i = Random.Range(0, lista.Length);
        if (lista.Length > 1 && i == ultimoPasso)
            i = (i + 1) % lista.Length;
        ultimoPasso = i;

        fontePassos.Fonte.pitch = (1f + Random.Range(-variacaoPitch, variacaoPitch))
                                  * (PassoPesado && lista != passosPesados ? multiplicadorPitchPesado : 1f);
        fontePassos.Fonte.PlayOneShot(lista[i], volume * (PassoPesado ? multiplicadorVolumePesado : 1f));
    }

    private void Pulou()
    {
        if (pulo != null)
            fonteCorpo.Fonte.PlayOneShot(pulo, volumePulo);
    }

    private void Aterrissou(float velocidadeQueda)
    {
        aterrissouForte = true;
        if (aterrissagem == null)
            return;
        float forca = Mathf.InverseLerp(3f, 12f, velocidadeQueda);
        fonteCorpo.Fonte.PlayOneShot(aterrissagem, Mathf.Lerp(volumeAterrissagem.x, volumeAterrissagem.y, forca));
    }
}
