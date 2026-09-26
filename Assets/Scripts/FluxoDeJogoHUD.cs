using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/* ============================================================
 *  FluxoDeJogoHUD.cs
 *  ---------------------------------------------------------------------
 *  Apresentação do FluxoDeJogo.cs: texto de briefing no início (fade in
 *  e depois fade out sozinho), barra de ocupação hospitalar (some da
 *  cor de acento pro vermelho conforme sobe) e telas de vitória/derrota
 *  com estatísticas. Aperte R depois do fim de jogo pra recarregar a
 *  cena e jogar de novo.
 *
 *  A hierarquia é gerada pelo menu  Dengue > Criar HUD de Fim de Jogo
 *  (Assets/Editor/FluxoDeJogoHUDBuilder.cs), que já liga as referências.
 * ============================================================ */

public class FluxoDeJogoHUD : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private FluxoDeJogo fluxo;
    [SerializeField] private Interactor interactor;
    [SerializeField] private ControladorJogador jogador;

    [Header("Briefing inicial")]
    [SerializeField] private CanvasGroup briefingGrupo;
    [SerializeField] private TextMeshProUGUI briefingTexto;
    [Tooltip("Quanto tempo o briefing fica visível antes de sumir sozinho (segundos).")]
    [SerializeField] private float duracaoBriefing = 6f;
    [SerializeField] private float velocidadeFadeBriefing = 3f;

    [Header("Barra de ocupação")]
    [SerializeField] private RectTransform ocupacaoFill;
    [SerializeField] private UnityEngine.UI.Image ocupacaoFillImage;
    [SerializeField] private TextMeshProUGUI ocupacaoTexto;
    [SerializeField] private Color corOcupacaoBaixa = new Color(0.25f, 1f, 0.8f, 1f);
    [SerializeField] private Color corOcupacaoAlta = new Color(1f, 0.25f, 0.25f, 1f);

    [Header("Vitória")]
    [SerializeField] private CanvasGroup vitoriaGrupo;
    [SerializeField] private TextMeshProUGUI vitoriaTexto;
    [SerializeField] private string tituloVitoria = "MISSÃO CUMPRIDA!";

    [Header("Derrota")]
    [SerializeField] private CanvasGroup derrotaGrupo;
    [SerializeField] private TextMeshProUGUI derrotaTexto;
    [SerializeField] private string tituloDerrota = "HOSPITAL EM COLAPSO";

    private float displayedOcupacao;
    private float briefingTimer;
    private bool vitoriaAtiva;
    private bool derrotaAtiva;

    /* Enquanto true, a tela de vitória/derrota espera (fica em alpha 0)
     * mesmo com a partida encerrada — o HistoriaDoJogo.cs liga isso para
     * tocar o epílogo antes das estatísticas. */
    public bool SegurarTelaFinal { get; set; }

    void Start()
    {
        if (fluxo == null) fluxo = FindFirstObjectByType<FluxoDeJogo>();
        if (interactor == null) interactor = FindFirstObjectByType<Interactor>();
        if (jogador == null) jogador = FindFirstObjectByType<ControladorJogador>();

        if (fluxo != null)
        {
            fluxo.OnVitoria += HandleVitoria;
            fluxo.OnDerrota += HandleDerrota;
            displayedOcupacao = fluxo.Ocupacao;
        }

        if (briefingGrupo != null)
        {
            briefingGrupo.alpha = 0f;
            briefingGrupo.blocksRaycasts = false;
            briefingGrupo.interactable = false;
        }
        briefingTimer = duracaoBriefing;

        if (vitoriaGrupo != null)
        {
            vitoriaGrupo.alpha = 0f;
            vitoriaGrupo.blocksRaycasts = false;
            vitoriaGrupo.interactable = false;
        }
        if (derrotaGrupo != null)
        {
            derrotaGrupo.alpha = 0f;
            derrotaGrupo.blocksRaycasts = false;
            derrotaGrupo.interactable = false;
        }

        AtualizarBarra();
    }

    void OnDestroy()
    {
        if (fluxo != null)
        {
            fluxo.OnVitoria -= HandleVitoria;
            fluxo.OnDerrota -= HandleDerrota;
        }
    }

    void Update()
    {
        AtualizarBriefing();
        AtualizarBarra();
        AtualizarFimDeJogo();

        if (fluxo != null && fluxo.PartidaEncerrada)
        {
            Keyboard teclado = Keyboard.current;
            if (teclado != null && teclado.rKey.wasPressedThisFrame)
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    /* ---------------- Briefing ---------------- */

    private void AtualizarBriefing()
    {
        if (briefingGrupo == null)
            return;

        briefingTimer -= Time.deltaTime;
        float alvo = briefingTimer > 0f ? 1f : 0f;
        briefingGrupo.alpha = Mathf.MoveTowards(briefingGrupo.alpha, alvo, Time.deltaTime * velocidadeFadeBriefing);
    }

    /* ---------------- Barra de ocupação ---------------- */

    private void AtualizarBarra()
    {
        if (fluxo == null)
            return;

        displayedOcupacao = Mathf.Lerp(displayedOcupacao, fluxo.Ocupacao, Time.deltaTime * 4f);

        if (ocupacaoFill != null)
            ocupacaoFill.anchorMax = new Vector2(displayedOcupacao, 1f);
        if (ocupacaoFillImage != null)
            ocupacaoFillImage.color = Color.Lerp(corOcupacaoBaixa, corOcupacaoAlta, displayedOcupacao);
        if (ocupacaoTexto != null)
            ocupacaoTexto.text = Mathf.RoundToInt(displayedOcupacao * 100f) + "%";
    }

    /* ---------------- Vitória / derrota ---------------- */

    private void AtualizarFimDeJogo()
    {
        bool liberado = !SegurarTelaFinal;
        if (vitoriaGrupo != null)
            vitoriaGrupo.alpha = Mathf.MoveTowards(vitoriaGrupo.alpha, vitoriaAtiva && liberado ? 1f : 0f, Time.deltaTime * 2f);
        if (derrotaGrupo != null)
            derrotaGrupo.alpha = Mathf.MoveTowards(derrotaGrupo.alpha, derrotaAtiva && liberado ? 1f : 0f, Time.deltaTime * 2f);
    }

    private void HandleVitoria()
    {
        if (interactor != null) interactor.Bloqueado = true;
        if (jogador != null) jogador.ControleAtivo = false; // fica parado na tela final
        vitoriaAtiva = true;
        if (vitoriaTexto != null)
            vitoriaTexto.text = MontarEstatisticas(tituloVitoria);
    }

    private void HandleDerrota()
    {
        if (interactor != null) interactor.Bloqueado = true;
        if (jogador != null) jogador.ControleAtivo = false; // fica parado na tela final
        derrotaAtiva = true;
        if (derrotaTexto != null)
            derrotaTexto.text = MontarEstatisticas(tituloDerrota);
    }

    private string MontarEstatisticas(string titulo)
    {
        int minutos = Mathf.FloorToInt(fluxo.TempoDecorrido / 60f);
        int segundos = Mathf.FloorToInt(fluxo.TempoDecorrido % 60f);
        return $"<b>{titulo}</b>\n\n" +
               $"Tempo: {minutos:00}:{segundos:00}\n" +
               $"Focos eliminados: {fluxo.FocosEliminados}/{fluxo.FocosTotal}\n" +
               $"Ocupação final: {Mathf.RoundToInt(fluxo.Ocupacao * 100f)}%\n\n" +
               "<size=70%><color=#FFFFFFB4>Aperte R para jogar de novo</color></size>";
    }
}
