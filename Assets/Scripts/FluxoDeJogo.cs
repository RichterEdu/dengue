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
        PartidaIniciada = true;
    }

    void Update()
    {
        if (!PartidaIniciada || PartidaEncerrada)
            return;

        TempoDecorrido += Time.deltaTime;
        Ocupacao = Mathf.Clamp01(Ocupacao + aumentoPorSegundo * Time.deltaTime);

        if (Ocupacao >= 1f)
        {
            PartidaEncerrada = true;
            OnDerrota?.Invoke();
        }
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
