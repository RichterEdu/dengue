using UnityEngine;

/* ============================================================
 *  TrilhaSonora.cs
 *  ---------------------------------------------------------------------
 *  Música adaptativa da partida + sinais do hospital (no objeto "Sons").
 *
 *  TRILHA: 4 camadas do mesmo tamanho tocando JUNTAS em loop
 *  (PlayScheduled no mesmo dspTime), e o volume de cada uma segue o jogo.
 *  A MAIOR PARTE DO JOGO É CALMA (pedido do usuário): só base + marimba.
 *  Pulso e tensão entram apenas quando o hospital está perto do colapso.
 *    - base       sempre (pad grave)
 *    - esperança  marimba suave desde o começo, cresce com os focos limpos
 *    - pulso      batimento + baixo, só com ocupação alta (faixaPulso)
 *    - tensão     ostinato + tique-taque, só com ocupação muito alta (faixaTensao)
 *  Vitória: pulso e tensão somem e toca o sting; derrota: tudo some e
 *  toca o sting.
 *
 *  ALERTA: bipe quando a ocupação cruza um marco SUBINDO (90/95%);
 *  o marco se rearma quando ela cai `rearme` abaixo dele.
 *
 *  PAUSA DO ÁUDIO: AudioListener.pause segue Time.timeScale == 0 — no menu
 *  de pausa e nas cartelas da abertura o mundo (zumbido, ambiente, passos)
 *  congela junto com o jogo; música e interface continuam
 *  (ignoreListenerPause). Com o menu de pausa aberto a música abaixa.
 *
 *  Música no canal Música; alerta no canal Efeitos.
 * ============================================================ */

public class TrilhaSonora : MonoBehaviour
{
    private enum Fim { Nenhum, Vitoria, Derrota }

    [Header("Camadas (mesmo tamanho, tocam juntas em loop)")]
    [SerializeField] private AudioClip camadaBase;
    [SerializeField] private AudioClip camadaPulso;
    [SerializeField] private AudioClip camadaTensao;
    [SerializeField] private AudioClip camadaEsperanca;
    [Tooltip("Volume de uma camada no máximo.")]
    [Range(0f, 1f)]
    [SerializeField] private float volumeCamada = 0.4f;
    [Tooltip("Quanto o peso de uma camada muda por segundo (0.15 = ~7 s de fade).")]
    [SerializeField] private float velocidadeMistura = 0.15f;
    [Range(0f, 1f)]
    [SerializeField] private float pesoBase = 0.8f;
    [Tooltip("Peso da marimba (esperança) sem nenhum foco limpo; chega a 1 no fim.")]
    [Range(0f, 1f)]
    [SerializeField] private float esperancaInicial = 0.45f;
    [Tooltip("Ocupação em que o pulso começa (x) e em que fica cheio (y).")]
    [SerializeField] private Vector2 faixaPulso = new Vector2(0.82f, 0.97f);
    [Tooltip("Ocupação em que a tensão começa (x) e em que fica cheia (y).")]
    [SerializeField] private Vector2 faixaTensao = new Vector2(0.88f, 0.98f);
    [Tooltip("Multiplicador da música com o menu de pausa aberto.")]
    [Range(0f, 1f)]
    [SerializeField] private float volumeNaPausa = 0.55f;

    [Header("Fim de jogo")]
    [SerializeField] private AudioClip vitoria;
    [SerializeField] private AudioClip derrota;
    [Range(0f, 1f)]
    [SerializeField] private float volumeFim = 0.8f;
    [Tooltip("Segundos para a trilha sumir na derrota.")]
    [SerializeField] private float duracaoSumirDerrota = 1.5f;

    [Header("Alerta de ocupação")]
    [SerializeField] private AudioClip alerta;
    [Range(0f, 1f)]
    [SerializeField] private float volumeAlerta = 0.45f;
    [SerializeField] private float[] marcos = { 0.9f, 0.95f };
    [Tooltip("Quanto a ocupação precisa cair abaixo do marco para ele valer de novo.")]
    [SerializeField] private float rearme = 0.03f;

    [Header("Referências (vazio = procura na cena)")]
    [SerializeField] private FluxoDeJogo fluxo;
    [SerializeField] private MenuPausa menuPausa;

    private CanalDeAudio[] camadas;
    private readonly float[] pesos = new float[4];
    private float multiplicadorPausa = 1f;
    private CanalDeAudio fonteSting;
    private CanalDeAudio fonteAlerta;
    private bool[] armado;
    private Fim fim;

    void Start()
    {
        if (fluxo == null) fluxo = FindAnyObjectByType<FluxoDeJogo>();
        if (menuPausa == null) menuPausa = FindAnyObjectByType<MenuPausa>();

        AudioClip[] clips = { camadaBase, camadaPulso, camadaTensao, camadaEsperanca };
        string[] nomes = { "Trilha Base", "Trilha Pulso", "Trilha Tensao", "Trilha Esperanca" };
        camadas = new CanalDeAudio[clips.Length];
        double inicio = AudioSettings.dspTime + 0.2;   // folga pra todas começarem juntas
        for (int i = 0; i < clips.Length; i++)
        {
            if (clips[i] == null)
                continue;
            CanalDeAudio c = CanalDeAudio.Criar(transform, nomes[i], CanalSom.Musica, false);
            c.Fonte.clip = clips[i];
            c.Fonte.loop = true;
            c.Fonte.ignoreListenerPause = true;
            c.Fonte.priority = i;
            c.Volume = 0f;
            c.Fonte.PlayScheduled(inicio);
            camadas[i] = c;
        }

        fonteSting = CanalDeAudio.Criar(transform, "Trilha Fim", CanalSom.Musica, false);
        fonteSting.Fonte.ignoreListenerPause = true;
        fonteSting.Fonte.priority = 4;
        fonteAlerta = CanalDeAudio.Criar(transform, "Alerta Ocupacao", CanalSom.Efeitos, false);
        fonteAlerta.Fonte.priority = 32;

        armado = new bool[marcos.Length];
        float ocupacao = fluxo != null ? fluxo.Ocupacao : 0f;
        for (int i = 0; i < marcos.Length; i++)
            armado[i] = ocupacao < marcos[i];

        if (fluxo != null)
        {
            fluxo.OnVitoria += Venceu;
            fluxo.OnDerrota += Perdeu;
        }
    }

    void OnDestroy()
    {
        if (fluxo != null)
        {
            fluxo.OnVitoria -= Venceu;
            fluxo.OnDerrota -= Perdeu;
        }
        AudioListener.pause = false;   // é global: não deixar a próxima cena muda
    }

    void Update()
    {
        bool pausado = Time.timeScale == 0f;
        if (AudioListener.pause != pausado)
            AudioListener.pause = pausado;

        Misturar();
        VerificarOcupacao();
    }

    /* ---------------- trilha ---------------- */

    private void Misturar()
    {
        if (camadas == null)
            return;

        float ocupacao = fluxo != null ? fluxo.Ocupacao : 0f;
        float progresso = fluxo != null && fluxo.FocosTotal > 0 ? (float)fluxo.FocosEliminados / fluxo.FocosTotal : 0f;

        float[] alvos =
        {
            pesoBase,
            Mathf.InverseLerp(faixaPulso.x, faixaPulso.y, ocupacao),
            Mathf.InverseLerp(faixaTensao.x, faixaTensao.y, ocupacao),
            Mathf.Lerp(esperancaInicial, 1f, Mathf.Pow(progresso, 0.8f)),
        };
        float velocidade = velocidadeMistura;
        if (fim == Fim.Vitoria)
        {
            alvos[1] = 0f;
            alvos[2] = 0f;
        }
        else if (fim == Fim.Derrota)
        {
            for (int i = 0; i < alvos.Length; i++) alvos[i] = 0f;
            velocidade = 1f / Mathf.Max(0.01f, duracaoSumirDerrota);
        }

        // A música continua com o jogo pausado: tempo real. Limitado porque o
        // primeiro frame traz o tempo de carregamento da cena (pularia o fade).
        float dt = Mathf.Min(Time.unscaledDeltaTime, 0.1f);
        bool menuAberto = menuPausa != null && menuPausa.Aberto;
        multiplicadorPausa = Mathf.MoveTowards(multiplicadorPausa, menuAberto ? volumeNaPausa : 1f, dt * 2f);

        for (int i = 0; i < camadas.Length; i++)
        {
            if (camadas[i] == null) continue;
            pesos[i] = Mathf.MoveTowards(pesos[i], alvos[i], velocidade * dt);
            camadas[i].Volume = pesos[i] * volumeCamada * multiplicadorPausa;
        }
    }

    private void Venceu()
    {
        fim = Fim.Vitoria;
        TocarSting(vitoria);
    }

    private void Perdeu()
    {
        fim = Fim.Derrota;
        TocarSting(derrota);
    }

    private void TocarSting(AudioClip clip)
    {
        if (clip != null && fonteSting != null)
            fonteSting.Fonte.PlayOneShot(clip, volumeFim);
    }

    /* ---------------- alerta de ocupação ---------------- */

    private void VerificarOcupacao()
    {
        if (fluxo == null || armado == null || !fluxo.PartidaIniciada || fluxo.PartidaEncerrada)
            return;

        float ocupacao = fluxo.Ocupacao;
        for (int i = 0; i < marcos.Length; i++)
        {
            if (armado[i] && ocupacao >= marcos[i])
            {
                armado[i] = false;
                if (alerta != null)
                    fonteAlerta.Fonte.PlayOneShot(alerta, volumeAlerta);
            }
            else if (!armado[i] && ocupacao < marcos[i] - rearme)
            {
                armado[i] = true;
            }
        }
    }
}
