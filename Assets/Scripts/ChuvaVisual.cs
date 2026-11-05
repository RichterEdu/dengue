using UnityEngine;
using UnityEngine.SceneManagement;

/* ============================================================
 *  ChuvaVisual.cs
 *  ---------------------------------------------------------------------
 *  O que se vê e se ouve da chuva (Parte B). Ouve a Chuva e o FluxoDeJogo:
 *
 *    - OnPrevisao: em rampa, até a hora da chuva (45 s), a luz do sol cai
 *      para fatorDeLuzNaChuva da original e a névoa chega mais perto, até
 *      começar em inicioDaNevoaNaChuva e fechar em fimDaNevoaNaChuva (se a
 *      cena não tinha névoa, ela é ligada);
 *    - OnComecou: a luz e a névoa ficam no máximo, as gotas (partículas
 *      presas à câmera) começam a cair, o trovão toca e o loop de chuva
 *      entra em segundosFadeEntrada (2 s);
 *    - OnTerminou: as gotas param, a luz e a névoa voltam ao normal em
 *      segundosDeVolta (10 s) e o loop sai em segundosFadeSaida (4 s);
 *    - OnVitoria/OnDerrota: o mesmo do OnTerminou.
 *
 *  Um valor só, 'nivel' (0 = céu normal, 1 = chuva), anda em rampa no
 *  Update (Time.deltaTime: com o jogo pausado, a rampa para) e decide a
 *  luz e a névoa. A intensidade do sol e a névoa originais (ligada, modo,
 *  densidade, início, fim e cor) são guardadas no Start e devolvidas quando
 *  o nível volta a 0 e no OnDestroy (só se o visual mudou alguma coisa e a
 *  cena dele é a ativa: ao voltar ao menu pela pausa, o RenderSettings já
 *  é o da tela de título e não pode receber a névoa da SampleScene).
 *
 *  Por que a névoa da chuva é Linear: a SampleScene está com a névoa
 *  desligada, e uma névoa ligada só em runtime depende de a build levar
 *  as variantes de shader daquele modo. Em Automatic, o Fog Modes das
 *  Graphics Settings só levaria os modos que alguma cena da build usa
 *  ligados. Por isso o FocosVivosBuilder deixa o Fog Modes em Custom com
 *  Linear marcado. No Editor qualquer modo funcionaria: a falta só apareceria
 *  no jogo exportado.
 *
 *  As gotas: o ParticleSystem do campo 'gotas' (filho da Main Camera),
 *  criado pelo FocosVivosBuilder. Vazio, a chuva fica só na luz e na névoa.
 *
 *  O som é 2D (a chuva cai em volta do agente), no canal de Efeitos, em
 *  fontes criadas no Start só para os clipes ligados (sem clipe, nada
 *  toca). O peso do loop anda em rampa no Update, como o 'nivel'; o Play
 *  e o Stop seguem um flag próprio, porque o isPlaying fica false com o
 *  AudioListener pausado. Os clipes são ligados por
 *  SomBuilder.LigarSonsDosFocosVivos.
 * ============================================================ */

public class ChuvaVisual : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("Relógio da chuva que este visual acompanha. Vazio = procura a Chuva na cena.")]
    [SerializeField] private Chuva chuva;

    [Tooltip("Fluxo da partida (na vitória ou na derrota, o céu volta ao normal). Vazio = procura o FluxoDeJogo na cena.")]
    [SerializeField] private FluxoDeJogo fluxo;

    [Tooltip("Luz direcional do sol, que escurece na chuva. Vazio = o Sun das Lighting Settings ou a primeira luz direcional da cena.")]
    [SerializeField] private Light sol;

    [Tooltip("Partículas das gotas, presas à câmera (filho da Main Camera). Vazio = chuva sem gotas, só luz e névoa.")]
    [SerializeField] private ParticleSystem gotas;

    [Header("Céu de chuva")]
    [Tooltip("Intensidade do sol durante a chuva, como fração da original (0.45 = 45%).")]
    [Range(0f, 1f)]
    [SerializeField] private float fatorDeLuzNaChuva = 0.45f;

    [Tooltip("Distância (m) da câmera em que a névoa Linear da chuva começa.")]
    [Min(0f)]
    [SerializeField] private float inicioDaNevoaNaChuva = 15f;

    [Tooltip("Distância (m) da câmera em que a névoa Linear da chuva cobre tudo (120 m deixa o fundo da rua acinzentado).")]
    [Min(0f)]
    [SerializeField] private float fimDaNevoaNaChuva = 120f;

    [Tooltip("Segundos para a luz e a névoa voltarem ao normal depois da chuva (ou no fim da partida).")]
    [Min(0f)]
    [SerializeField] private float segundosDeVolta = 10f;

    [Header("Som")]
    [Tooltip("Trovão, uma vez, quando a chuva começa. Vazio = sem trovão.")]
    [SerializeField] private AudioClip trovao;

    [Tooltip("Volume do trovão.")]
    [Range(0f, 1f)]
    [SerializeField] private float volumeTrovao = 0.8f;

    [Tooltip("Loop 2D da chuva. Vazio = chuva muda.")]
    [SerializeField] private AudioClip chuvaLoop;

    [Tooltip("Volume do loop de chuva.")]
    [Range(0f, 1f)]
    [SerializeField] private float volumeChuva = 0.55f;

    [Tooltip("Segundos para o loop de chuva entrar.")]
    [Min(0.01f)]
    [SerializeField] private float segundosFadeEntrada = 2f;

    [Tooltip("Segundos para o loop de chuva sair (fim da chuva ou da partida).")]
    [Min(0.01f)]
    [SerializeField] private float segundosFadeSaida = 4f;

    // De onde a névoa Linear parte quando a cena não tinha névoa Linear
    // ligada: longe o bastante para não aparecer no bairro.
    private const float InicioSemNevoa = 400f;
    private const float FimSemNevoa = 1000f;

    // 0 = céu normal, 1 = chuva; anda até 'alvo' a 'velocidade' por segundo.
    private float nivel;
    private float alvo;
    private float velocidade;

    // Originais, guardados no Start.
    private bool guardou;
    private float intensidadeOriginal;
    private bool nevoaOriginal;
    private FogMode modoOriginal;
    private float densidadeOriginal;
    private float inicioOriginal;
    private float fimOriginal;
    private Color corDaNevoaOriginal;

    // O céu está fora do original: há o que devolver no OnDestroy.
    private bool alterou;

    // Som: 0 = mudo, 1 = chuva cheia; anda até 'alvoDoPeso' a 'velocidadeDoPeso' por segundo.
    private CanalDeAudio fonteTrovao;
    private CanalDeAudio fonteChuva;
    private float alvoDoPeso;
    private float velocidadeDoPeso;
    private bool loopLigado;

    // Assinados no Start (para desassinar no OnDestroy).
    private Chuva chuvaAssinada;
    private FluxoDeJogo fluxoAssinado;

    /* Quanto do loop de chuva está valendo, de 0 a 1. */
    internal float PesoDaChuva { get; private set; }

    void Start()
    {
        if (chuva == null) chuva = FindAnyObjectByType<Chuva>();
        if (fluxo == null) fluxo = FindAnyObjectByType<FluxoDeJogo>();
        if (sol == null) sol = AcharSol();

        intensidadeOriginal = sol != null ? sol.intensity : 0f;
        nevoaOriginal = RenderSettings.fog;
        modoOriginal = RenderSettings.fogMode;
        densidadeOriginal = RenderSettings.fogDensity;
        inicioOriginal = RenderSettings.fogStartDistance;
        fimOriginal = RenderSettings.fogEndDistance;
        corDaNevoaOriginal = RenderSettings.fogColor;
        guardou = true;

        if (gotas != null)
            gotas.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        CriarFontes();

        if (chuva != null)
        {
            chuva.OnPrevisao += AoPrever;
            chuva.OnComecou += AoComecar;
            chuva.OnTerminou += AoTerminar;
            chuvaAssinada = chuva;
        }

        if (fluxo != null)
        {
            fluxo.OnVitoria += AoEncerrarPartida;
            fluxo.OnDerrota += AoEncerrarPartida;
            fluxoAssinado = fluxo;
        }
    }

    void Update()
    {
        AtualizarSom(Time.deltaTime);

        if (Mathf.Approximately(nivel, alvo))
            return;

        nivel = velocidade > 0f ? Mathf.MoveTowards(nivel, alvo, velocidade * Time.deltaTime) : alvo;
        Aplicar();
    }

    /* Desfaz as assinaturas e devolve a luz e a névoa originais (o
     * RenderSettings não volta sozinho ao sair do Play), se for o caso
     * (DeveRestaurar). */
    void OnDestroy()
    {
        if (chuvaAssinada != null)
        {
            chuvaAssinada.OnPrevisao -= AoPrever;
            chuvaAssinada.OnComecou -= AoComecar;
            chuvaAssinada.OnTerminou -= AoTerminar;
        }
        chuvaAssinada = null;

        if (fluxoAssinado != null)
        {
            fluxoAssinado.OnVitoria -= AoEncerrarPartida;
            fluxoAssinado.OnDerrota -= AoEncerrarPartida;
        }
        fluxoAssinado = null;

        if (guardou && DeveRestaurar(alterou, gameObject.scene, SceneManager.GetActiveScene()))
        {
            nivel = 0f;
            Aplicar();
        }
    }

    /* Devolver o RenderSettings só faz sentido se o visual mudou alguma
     * coisa e se a cena dele ainda é a ativa: com outra cena ativa, o
     * RenderSettings já é o dela. */
    internal static bool DeveRestaurar(bool alterou, Scene cenaDoVisual, Scene cenaAtiva)
    {
        return alterou && cenaDoVisual == cenaAtiva;
    }

    /* O Sun das Lighting Settings ou, sem ele, a primeira luz direcional. */
    private static Light AcharSol()
    {
        if (RenderSettings.sun != null)
            return RenderSettings.sun;

        foreach (Light luz in FindObjectsByType<Light>(FindObjectsInactive.Exclude))
        {
            if (luz.type == LightType.Directional)
                return luz;
        }
        return null;
    }

    /* Previsão: rampa até 1 no tempo que falta para a chuva (45 s). */
    private void AoPrever()
    {
        float segundos = chuva != null ? chuva.ProximoInicio - chuva.Tempo : 0f;
        IrPara(1f, segundos);
    }

    /* Começou: céu de chuva na hora (se a rampa não chegou) e as gotas. */
    private void AoComecar()
    {
        IrPara(1f, 0f);
        if (gotas != null)
            gotas.Play(true);
        IniciarSomDaChuva();
    }

    private void AoTerminar()
    {
        Voltar();
    }

    private void AoEncerrarPartida()
    {
        Voltar();
    }

    /* As gotas param (as que já caem terminam a queda), o céu volta ao
     * normal em segundosDeVolta e o loop de chuva sai. */
    private void Voltar()
    {
        if (gotas != null)
            gotas.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        IrPara(0f, segundosDeVolta);
        PararSomDaChuva();
    }

    /* Leva o nível até 'destino' em 'segundos' (0 = na hora). */
    private void IrPara(float destino, float segundos)
    {
        alvo = destino;
        float distancia = Mathf.Abs(alvo - nivel);
        velocidade = segundos > 0f ? distancia / segundos : 0f;

        if (velocidade <= 0f)
        {
            nivel = alvo;
            Aplicar();
        }
    }

    /* Uma fonte 2D para cada clipe ligado. */
    private void CriarFontes()
    {
        if (trovao != null)
            fonteTrovao = CanalDeAudio.Criar(transform, "Som Trovão", CanalSom.Efeitos, false);

        if (chuvaLoop != null)
        {
            fonteChuva = CanalDeAudio.Criar(transform, "Som Chuva", CanalSom.Efeitos, false);
            fonteChuva.Fonte.clip = chuvaLoop;
            fonteChuva.Fonte.loop = true;
            fonteChuva.Volume = 0f;   // o AtualizarSom sobe o peso
        }
    }

    /* A chuva começou: o trovão, e o loop entra em segundosFadeEntrada. */
    internal void IniciarSomDaChuva()
    {
        if (fonteTrovao != null)
            fonteTrovao.Fonte.PlayOneShot(trovao, volumeTrovao);

        alvoDoPeso = 1f;
        velocidadeDoPeso = 1f / Mathf.Max(0.01f, segundosFadeEntrada);
    }

    /* A chuva parou (ou a partida acabou): o loop sai em segundosFadeSaida. */
    internal void PararSomDaChuva()
    {
        alvoDoPeso = 0f;
        velocidadeDoPeso = 1f / Mathf.Max(0.01f, segundosFadeSaida);
    }

    /* Anda o fade do loop e dá o Play ou o Stop nas pontas dele. */
    internal void AtualizarSom(float dt)
    {
        PesoDaChuva = Mathf.MoveTowards(PesoDaChuva, alvoDoPeso, velocidadeDoPeso * dt);

        if (fonteChuva == null)
            return;

        fonteChuva.Volume = volumeChuva * PesoDaChuva;
        if (PesoDaChuva > 0f && !loopLigado)
        {
            fonteChuva.Fonte.Play();
            loopLigado = true;
        }
        else if (PesoDaChuva <= 0f && loopLigado)
        {
            fonteChuva.Fonte.Stop();
            loopLigado = false;
        }
    }

    /* Luz e névoa para o nível atual; em 0, exatamente as originais. */
    private void Aplicar()
    {
        if (!guardou)
            return;

        if (sol != null)
            sol.intensity = Mathf.Lerp(intensidadeOriginal, intensidadeOriginal * fatorDeLuzNaChuva, nivel);

        if (nivel <= 0f)
        {
            RenderSettings.fog = nevoaOriginal;
            RenderSettings.fogMode = modoOriginal;
            RenderSettings.fogDensity = densidadeOriginal;
            RenderSettings.fogStartDistance = inicioOriginal;
            RenderSettings.fogEndDistance = fimOriginal;
            RenderSettings.fogColor = corDaNevoaOriginal;
            alterou = false;
            return;
        }

        alterou = true;

        // Sempre Linear (ver o cabeçalho: é o modo que a build mantém). Com
        // névoa Linear na cena, parte dela; sem, nasce de longe. Uma névoa
        // Exponencial ligada na cena viraria Linear na hora (nenhuma cena
        // do jogo tem isso hoje).
        bool partiuDaLinear = nevoaOriginal && modoOriginal == FogMode.Linear;
        float inicioDePartida = partiuDaLinear ? inicioOriginal : InicioSemNevoa;
        float fimDePartida = partiuDaLinear ? fimOriginal : FimSemNevoa;
        float fimNaChuva = Mathf.Max(fimDaNevoaNaChuva, inicioDaNevoaNaChuva + 1f);

        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = Mathf.Lerp(inicioDePartida, inicioDaNevoaNaChuva, nivel);
        RenderSettings.fogEndDistance = Mathf.Lerp(fimDePartida, fimNaChuva, nivel);
    }
}
