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
 *  - Sobe sozinha com o tempo; cada foco eliminado desafoga uma fração
 *    fixa. Vitória: todos os focos eliminados antes de bater 100%.
 *    Derrota: ocupação bate 100% antes de todos os focos sumirem.
 *  - O ritmo NÃO é constante (pedido do usuário, "pra dar tempo do player
 *    pensar e ler o que tá acontecendo"): começa calmo (aumentoNoInicio),
 *    depois de duracaoInicioCalmo sobe em rampa até aumentoPorSegundo.
 *    E enquanto o celular está aberto a taxa cai (multiplicadorComCelular)
 *    — sem nenhum aviso na tela, o jogador não precisa saber.
 *
 *  Mesma varredura de Interactable usada por DengueHUD/RedeSocial/
 *  FocoListaHUD — não existe gerenciador central de focos no projeto,
 *  cada sistema faz a própria.
 * ============================================================ */

public class FluxoDeJogo : MonoBehaviour
{
    [Header("Ocupação hospitalar (0 a 1)")]
    [Tooltip("Ocupação ao nascer o jogo (0.5 = 50%).")]
    [SerializeField] private float ocupacaoInicial = 0.5f;

    [Tooltip("Quanto a ocupação sobe por segundo enquanto o tempo passa (0.003 = 0.3%/s — sem eliminar nenhum foco, dá uns 2min50 até perder, tempo o bastante pra sair da sala e se situar).")]
    [SerializeField] private float aumentoPorSegundo = 0.003f;

    [Tooltip("Quanto a ocupação desce a cada foco eliminado (0.04 = 4%).")]
    [SerializeField] private float reducaoPorFoco = 0.04f;

    [Header("Ritmo (começo calmo, depois pressão)")]
    [Tooltip("Taxa por segundo logo que o jogador sai do hospital (0.002 = 0.2%/s) — tempo pra ler o celular e se situar.")]
    [SerializeField] private float aumentoNoInicio = 0.002f;

    [Tooltip("Segundos (desde a saída do hospital) em que a taxa fica em aumentoNoInicio.")]
    [SerializeField] private float duracaoInicioCalmo = 45f;

    [Tooltip("Segundos da rampa que leva de aumentoNoInicio até aumentoPorSegundo.")]
    [SerializeField] private float duracaoRampa = 60f;

    [Tooltip("Com o celular aberto, a taxa é multiplicada por isto (0.25 = sobe 4x mais devagar). Não aparece na tela.")]
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

    private readonly List<Interactable> focos = new List<Interactable>();

    void Start()
    {
        Ocupacao = Mathf.Clamp01(ocupacaoInicial);
        if (celular == null) celular = FindFirstObjectByType<CelularController>();

        foreach (Interactable foco in FindObjectsByType<Interactable>(FindObjectsInactive.Include, FindObjectsSortMode.None))
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
        Ocupacao = Mathf.Clamp01(Ocupacao + TaxaAtual() * Time.deltaTime);

        if (Ocupacao >= 1f)
        {
            PartidaEncerrada = true;
            OnDerrota?.Invoke();
        }
    }

    /* Quanto a ocupação sobe por segundo AGORA: calmo no começo, rampa até
     * aumentoPorSegundo, e mais devagar com o celular aberto. */
    private float TaxaAtual()
    {
        float rampa = duracaoRampa > 0f
            ? Mathf.Clamp01((TempoDecorrido - duracaoInicioCalmo) / duracaoRampa)
            : (TempoDecorrido >= duracaoInicioCalmo ? 1f : 0f);
        float taxa = Mathf.Lerp(aumentoNoInicio, aumentoPorSegundo, rampa);

        if (celular != null && celular.Aberto)
            taxa *= multiplicadorComCelular;
        return taxa;
    }

    private void HandleFocoCompletado(Interactable foco)
    {
        foco.OnCompleted -= HandleFocoCompletado;
        FocosEliminados++;
        Ocupacao = Mathf.Clamp01(Ocupacao - reducaoPorFoco);

        if (!PartidaEncerrada && FocosEliminados >= FocosTotal)
        {
            PartidaEncerrada = true;
            OnVitoria?.Invoke();
        }
    }
}
