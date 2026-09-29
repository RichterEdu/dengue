using System;
using System.Collections.Generic;
using UnityEngine;

/* ============================================================
 *  FluxoDeJogo.cs
 *  ---------------------------------------------------------------------
 *  Estado geral da partida: ocupação hospitalar, cronômetro e detecção
 *  de vitória/derrota. Não desenha nada — quem mostra isso na tela é o
 *  FluxoDeJogoHUD.cs, que assina OnVitoria/OnDerrota.
 *
 *  - A ocupação SÓ sobe depois que o jogador sai do quarto do hospital
 *    (SaidaDoHospital.cs chama IniciarPartida()) — antes disso, o
 *    relógio fica parado.
 *  - Como no GameDoc, cada foco eliminado DESACELERA a subida: a taxa é
 *    proporcional aos focos ainda ativos (taxaComTodosOsFocos com todos,
 *    menos recuperacaoSemFocos). Quando sobram poucos focos a taxa fica
 *    negativa e a ocupação DESCE — o "clímax" de sucesso. Cada foco ainda
 *    dá um alívio pequeno na hora (alivioPorFoco), e a ocupação nunca fica
 *    abaixo de ocupacaoMinima: não dá pra "guardar" vantagem zerando a
 *    barra no começo.
 *    Vitória: todos os focos eliminados antes de bater 100%.
 *    Derrota: ocupação bate 100% antes de todos os focos sumirem.
 *  - O ritmo NÃO é constante (pedido do usuário, "pra dar tempo do player
 *    pensar e ler o que tá acontecendo"): a taxa começa multiplicada por
 *    fatorInicioCalmo e, depois de duracaoInicioCalmo, sobe em rampa até
 *    o valor cheio. E enquanto o celular está aberto a SUBIDA cai
 *    (multiplicadorComCelular) — sem nenhum aviso na tela, o jogador não
 *    precisa saber.
 *  - Para reajustar os números: Dengue > Simular Balanceamento
 *    (Assets/Editor/BalanceamentoSimulador.cs), que usa o mesmo TaxaPara.
 *
 *  Mesma varredura de Interactable usada por DengueHUD/RedeSocial/
 *  FocoListaHUD — não existe gerenciador central de focos no projeto,
 *  cada sistema faz a própria.
 * ============================================================ */

public class FluxoDeJogo : MonoBehaviour
{
    [Header("Ocupação hospitalar (0 a 1)")]
    [Tooltip("Ocupação ao nascer o jogo (0.7 = 70%).")]
    [SerializeField] private float ocupacaoInicial = 0.7f;

    [Tooltip("A ocupação nunca fica abaixo disto (0.3 = 30%) — o hospital não esvazia, e limpar focos no começo não 'guarda' vantagem.")]
    [Range(0f, 1f)]
    [SerializeField] private float ocupacaoMinima = 0.3f;

    [Header("Taxa (cada foco eliminado desacelera a subida)")]
    [Tooltip("Quanto a ocupação sobe por segundo com TODOS os focos ativos (0.003 = 0.3%/s). A subida cai na proporção dos focos eliminados.")]
    [SerializeField] private float taxaComTodosOsFocos = 0.003f;

    [Tooltip("Recuperação do hospital por segundo, descontada da subida (0.001 = 0.1%/s). Quando sobram poucos focos a taxa fica negativa e a ocupação desce.")]
    [SerializeField] private float recuperacaoSemFocos = 0.001f;

    [Tooltip("Quanto a ocupação desce na hora, a cada foco eliminado (0.025 = 2,5%).")]
    [SerializeField] private float alivioPorFoco = 0.025f;

    [Header("Ritmo (começo calmo, depois pressão)")]
    [Tooltip("Multiplicador da taxa logo que o jogador sai do hospital (0.2 = 5x mais devagar) — tempo pra ler o celular e se situar.")]
    [Range(0f, 1f)]
    [SerializeField] private float fatorInicioCalmo = 0.2f;

    [Tooltip("Segundos (desde a saída do hospital) em que a taxa fica multiplicada por fatorInicioCalmo.")]
    [SerializeField] private float duracaoInicioCalmo = 45f;

    [Tooltip("Segundos da rampa que leva de fatorInicioCalmo até a taxa cheia.")]
    [SerializeField] private float duracaoRampa = 60f;

    [Tooltip("Com o celular aberto, a SUBIDA é multiplicada por isto (0.25 = sobe 4x mais devagar). A descida do fim não muda. Não aparece na tela.")]
    [Range(0f, 1f)]
    [SerializeField] private float multiplicadorComCelular = 0.25f;

    [Tooltip("Vazio = procura o CelularController na cena.")]
    [SerializeField] private CelularController celular;

    public event Action OnPartidaIniciada;
    public event Action OnVitoria;
    public event Action OnDerrota;

    public float Ocupacao { get; private set; }
    public bool PartidaIniciada { get; private set; }
    public bool PartidaEncerrada { get; private set; }
    public float TempoDecorrido { get; private set; }
    public int FocosEliminados { get; private set; }
    public int FocosTotal { get; private set; }

    /* Lidos pelo simulador de balanceamento (Editor). */
    public float OcupacaoInicial => Mathf.Clamp(ocupacaoInicial, ocupacaoMinima, 1f);
    public float OcupacaoMinima => ocupacaoMinima;
    public float AlivioPorFoco => alivioPorFoco;

    private readonly List<Interactable> focos = new List<Interactable>();

    void Start()
    {
        Ocupacao = OcupacaoInicial;
        if (celular == null) celular = FindAnyObjectByType<CelularController>();

        foreach (Interactable foco in FindObjectsByType<Interactable>(FindObjectsInactive.Include))
        {
            focos.Add(foco);
            if (foco.IsComplete)
                FocosEliminados++;
            else
                foco.OnCompleted += HandleFocoCompletado;
        }
        FocosTotal = focos.Count;
    }

    void OnDestroy()
    {
        foreach (Interactable foco in focos)
            if (foco != null)
                foco.OnCompleted -= HandleFocoCompletado;
    }

    /* Chamado pelo SaidaDoHospital.cs quando o jogador cruza a porta. */
    public void IniciarPartida()
    {
        if (PartidaIniciada)
            return;

        PartidaIniciada = true;
        OnPartidaIniciada?.Invoke();
    }

    void Update()
    {
        if (!PartidaIniciada || PartidaEncerrada)
            return;

        TempoDecorrido += Time.deltaTime;
        float taxa = TaxaPara(TempoDecorrido, FocosTotal - FocosEliminados, celular != null && celular.Aberto);
        Ocupacao = Mathf.Clamp(Ocupacao + taxa * Time.deltaTime, ocupacaoMinima, 1f);

        if (Ocupacao >= 1f)
        {
            PartidaEncerrada = true;
            OnDerrota?.Invoke();
        }
    }

    /* Quanto a ocupação varia por segundo (negativo = desce) com `focosAtivos`
     * ainda de pé, `tempo` segundos depois da saída do hospital: proporcional
     * aos focos ativos, calmo no começo e com a subida mais lenta com o
     * celular aberto. `focosTotal` < 0 usa o FocosTotal da partida (o
     * simulador do Editor passa o total, porque fora do Play o Start não rodou). */
    public float TaxaPara(float tempo, int focosAtivos, bool celularAberto, int focosTotal = -1)
    {
        if (focosTotal < 0)
            focosTotal = FocosTotal;
        float pressao = focosTotal > 0 ? Mathf.Clamp01((float)focosAtivos / focosTotal) : 0f;

        float progressoRampa = duracaoRampa > 0f
            ? Mathf.Clamp01((tempo - duracaoInicioCalmo) / duracaoRampa)
            : (tempo >= duracaoInicioCalmo ? 1f : 0f);
        float rampa = Mathf.Lerp(fatorInicioCalmo, 1f, progressoRampa);

        float taxa = rampa * (taxaComTodosOsFocos * pressao - recuperacaoSemFocos);
        if (taxa > 0f && celularAberto)
            taxa *= multiplicadorComCelular;
        return taxa;
    }

    private void HandleFocoCompletado(Interactable foco)
    {
        foco.OnCompleted -= HandleFocoCompletado;
        FocosEliminados++;
        Ocupacao = Mathf.Max(ocupacaoMinima, Ocupacao - alivioPorFoco);

        if (!PartidaEncerrada && FocosEliminados >= FocosTotal)
        {
            PartidaEncerrada = true;
            OnVitoria?.Invoke();
        }
    }
}
