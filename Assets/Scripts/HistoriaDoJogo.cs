using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* ============================================================
 *  HistoriaDoJogo.cs
 *  ---------------------------------------------------------------------
 *  Abre e fecha a história do jogo com cartelas (CartelasNarrativas.cs):
 *
 *  ABERTURA — antes de aparecer o hospital: onde e quando estamos, o que
 *  está acontecendo, quem é o jogador. Enquanto as cartelas tocam o jogo
 *  fica PAUSADO (Time.timeScale = 0): o jogador não anda nem olha, a fila
 *  da rede social e o briefing esperam. Ao reiniciar com R a abertura é
 *  pulada (o jogador já viu).
 *
 *  EPÍLOGO — na vitória ou na derrota, antes da tela de estatísticas:
 *  "Uma semana depois..." + a fala de cada personagem do bairro, que muda
 *  conforme o que o jogador limpou (HistoriaRedeSocial.LinhasEpilogo), e
 *  um fechamento. A tela de estatísticas do FluxoDeJogoHUD espera
 *  (SegurarTelaFinal) até o epílogo acabar.
 *
 *  Criado pelo menu  Dengue > Configurar Historia  (HistoriaBuilder.cs).
 * ============================================================ */

public class HistoriaDoJogo : MonoBehaviour
{
    [Header("Referências (vazias = procura na cena)")]
    [SerializeField] private CartelasNarrativas cartelas;
    [SerializeField] private HistoriaRedeSocial narrador;
    [SerializeField] private FluxoDeJogo fluxo;
    [SerializeField] private FluxoDeJogoHUD fluxoHUD;
    [SerializeField] private Interactor interactor;
    [SerializeField] private CelularController celular;
    [SerializeField] private ControladorJogador jogador;

    [Header("Abertura")]
    [SerializeField] private bool tocarAbertura = true;
    [Tooltip("Ao reiniciar a partida (tecla R), pula a abertura.")]
    [SerializeField] private bool pularAberturaAoReiniciar = true;
    [TextArea(2, 4)]
    [SerializeField] private List<string> abertura = new List<string>();

    [Header("Epílogo")]
    [Tooltip("Espera (s) entre o fim da partida e o epílogo — dá tempo de ver o último foco sumir.")]
    [SerializeField] private float atrasoEpilogo = 1.6f;
    [SerializeField] private string tituloEpilogo = "Uma semana depois...";
    [TextArea(2, 4)]
    [SerializeField] private List<string> fechamentoVitoria = new List<string>();
    [TextArea(2, 4)]
    [SerializeField] private List<string> fechamentoDerrota = new List<string>();

    // Sobrevive ao recarregar a cena (tecla R), mas não a fechar o jogo.
    private static bool aberturaJaTocada;

    // Zera a cada Play no Editor, mesmo com "Enter Play Mode" sem domain
    // reload (aí os campos estáticos sobreviveriam entre um Play e outro e
    // a abertura só apareceria no primeiro). Não roda no LoadScene do R.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ZerarEstado()
    {
        aberturaJaTocada = false;
    }

    /* "Reiniciar do zero" do menu de pausa: a próxima carga da cena toca a
     * abertura de novo (o R do fim de jogo continua pulando). */
    public static void EsquecerAbertura()
    {
        aberturaJaTocada = false;
    }

    private bool vaiTocarAbertura;
    private bool pausouJogo;

    void Reset()
    {
        abertura = AberturaPadrao();
        fechamentoVitoria = FechamentoVitoriaPadrao();
        fechamentoDerrota = FechamentoDerrotaPadrao();
    }

    void Awake()
    {
        if (abertura == null || abertura.Count == 0) abertura = AberturaPadrao();
        if (fechamentoVitoria == null || fechamentoVitoria.Count == 0) fechamentoVitoria = FechamentoVitoriaPadrao();
        if (fechamentoDerrota == null || fechamentoDerrota.Count == 0) fechamentoDerrota = FechamentoDerrotaPadrao();

        if (cartelas == null) cartelas = FindFirstObjectByType<CartelasNarrativas>();
        if (narrador == null) narrador = FindFirstObjectByType<HistoriaRedeSocial>();
        if (fluxo == null) fluxo = FindFirstObjectByType<FluxoDeJogo>();
        if (fluxoHUD == null) fluxoHUD = FindFirstObjectByType<FluxoDeJogoHUD>();
        if (interactor == null) interactor = FindFirstObjectByType<Interactor>();
        if (celular == null) celular = FindFirstObjectByType<CelularController>();
        if (jogador == null) jogador = FindFirstObjectByType<ControladorJogador>();

        vaiTocarAbertura = tocarAbertura && cartelas != null && abertura.Count > 0 &&
                           !(pularAberturaAoReiniciar && aberturaJaTocada);

        if (cartelas != null)
        {
            if (vaiTocarAbertura) cartelas.MostrarFundoImediato();
            else cartelas.EsconderImediato();
        }
    }

    void Start()
    {
        if (fluxo != null)
        {
            fluxo.OnVitoria += HandleVitoria;
            fluxo.OnDerrota += HandleDerrota;
        }

        if (vaiTocarAbertura)
            IniciarAbertura();
    }

    void OnDestroy()
    {
        if (fluxo != null)
        {
            fluxo.OnVitoria -= HandleVitoria;
            fluxo.OnDerrota -= HandleDerrota;
        }

        // timeScale é global e sobrevive à troca de cena: nunca deixar o
        // jogo congelado se a cena for descarregada no meio da abertura.
        if (pausouJogo)
            Time.timeScale = 1f;
    }

    /* ---------------- Abertura ---------------- */

    private void IniciarAbertura()
    {
        aberturaJaTocada = true;
        pausouJogo = true;
        Time.timeScale = 0f;
        Travar(true);
        cartelas.Tocar(abertura, FimAbertura);
    }

    private void FimAbertura()
    {
        Time.timeScale = 1f;
        pausouJogo = false;
        Travar(false);
    }

    /* Durante as cartelas: sem olhar/andar/pular, sem mirar em foco e sem
     * abrir o celular por trás da tela preta. */
    private void Travar(bool travar)
    {
        if (interactor != null) interactor.Bloqueado = travar;
        if (jogador != null) jogador.ControleAtivo = !travar;
        if (celular != null) celular.enabled = !travar;
    }

    /* ---------------- Epílogo ---------------- */

    private void HandleVitoria() => IniciarEpilogo(true);
    private void HandleDerrota() => IniciarEpilogo(false);

    private void IniciarEpilogo(bool vitoria)
    {
        if (cartelas == null)
            return;

        if (fluxoHUD != null)
            fluxoHUD.SegurarTelaFinal = true;
        StartCoroutine(Epilogo(vitoria));
    }

    private IEnumerator Epilogo(bool vitoria)
    {
        yield return new WaitForSecondsRealtime(atrasoEpilogo);

        if (celular != null && celular.Aberto)
            celular.Fechar();

        List<string> linhas = new List<string> { tituloEpilogo };
        if (narrador != null)
            linhas.AddRange(narrador.LinhasEpilogo(vitoria));
        linhas.AddRange(vitoria ? fechamentoVitoria : fechamentoDerrota);

        cartelas.Tocar(linhas, null, LiberarTelaFinal);
    }

    private void LiberarTelaFinal()
    {
        if (fluxoHUD != null)
            fluxoHUD.SegurarTelaFinal = false;
    }

    /* ---------------- Conteúdo padrão (editável no Inspector) ---------------- */

    private static List<string> AberturaPadrao()
    {
        return new List<string>
        {
            "Vila Nova, fevereiro.",
            "Três semanas de chuva sem parar.\nEm cada quintal, água parada.",
            "A UPA do bairro já não tem onde colocar os doentes de dengue.",
            "Você é agente comunitário de saúde.\nE o bairro inteiro está de olho em você.",
            "Tudo o que você fizer, alguém vai comentar.\n<size=70%><color=#40FFCC>Acompanhe pelo celular [T].</color></size>",
        };
    }

    private static List<string> FechamentoVitoriaPadrao()
    {
        return new List<string>
        {
            "A ocupação da UPA voltou ao normal.",
            "Prevenção não é trabalho de uma pessoa só.\nMas alguém precisa começar.",
        };
    }

    private static List<string> FechamentoDerrotaPadrao()
    {
        return new List<string>
        {
            "A UPA Vila Nova fechou as portas para novos pacientes.",
            "Um único foco esquecido basta para tudo recomeçar.\nTente de novo.",
        };
    }
}
