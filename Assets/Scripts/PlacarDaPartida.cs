using System;
using System.Collections.Generic;
using UnityEngine;

/* ============================================================
 *  PlacarDaPartida.cs
 *  ---------------------------------------------------------------------
 *  Acompanha a partida para a nota. Não desenha nada: a janela, o HUD e
 *  a tela final leem daqui.
 *
 *  - Metas: Resposta rápida e Hospital sob controle caem quando o limite
 *    passa (Conferir, todo frame); Sem erros cai no primeiro minigame
 *    errado, como a tela rasgada do ralo (Interactor.OnErrouSkillCheck).
 *    O reenchido (OnFocoReenchido) só é contado, para o desconto. Cada meta cai uma vez só (OnMetaPerdida)
 *    e só durante a partida (iniciada e não encerrada). MetaViva lê o
 *    conjunto das perdidas; não recalcula dos números.
 *  - Dados: antes de Fechar, o retrato ao vivo (Venceu = false); depois,
 *    o retrato fechado. Sem fluxo, tudo zero.
 *  - Fechar(venceu) calcula a nota, registra no recorde e fica com o
 *    primeiro resultado (chamar de novo devolve o mesmo).
 *  - Fecha sozinho: OnVitoria do fluxo chama Fechar(true) e OnDerrota
 *    chama Fechar(false), sem depender do HUD.
 *  - No EditMode o Start e o Update não rodam: os testes chamam Ligar e
 *    Conferir direto e trocam o recorde por UsarRecorde.
 * ============================================================ */

public class PlacarDaPartida : MonoBehaviour
{
    [Tooltip("Vazio = o FluxoDeJogo da cena.")]
    [SerializeField] private FluxoDeJogo fluxo;

    [Tooltip("Vazio = o GerenciadorDeFocos da cena.")]
    [SerializeField] private GerenciadorDeFocos gerenciador;

    [Tooltip("Vazio = o Interactor da cena (avisa os minigames errados).")]
    [SerializeField] private Interactor interactor;

    [Tooltip("Limites das metas e valores da nota.")]
    [SerializeField] private RegrasDaNota regras = new RegrasDaNota();

    public event Action<MetaDaPartida> OnMetaPerdida;

    private readonly HashSet<MetaDaPartida> perdidas = new HashSet<MetaDaPartida>();
    private RecordeDoJogo recorde;
    private DadosDaPartida fechados;

    public int Reenchidos { get; private set; }
    public int Erros { get; private set; }
    public RegrasDaNota Regras => regras;
    public bool Fechado { get; private set; }
    public ResultadoDaNota Resultado { get; private set; }
    public bool RecordeNovo { get; private set; }
    public RecordeDoJogo Recorde => recorde ?? (recorde = new RecordeDoJogo());

    public DadosDaPartida Dados => Fechado ? fechados : Retrato(false);

    private bool EmPartida => fluxo != null && fluxo.PartidaIniciada && !fluxo.PartidaEncerrada;

    void Start()
    {
        Ligar();
    }

    void Update()
    {
        Conferir();
    }

    /* Acha o gerenciador e o fluxo (só se o campo estiver vazio) e assina
     * o OnFocoReenchido; chamar de novo não assina duas vezes. */
    internal void Ligar()
    {
        if (gerenciador == null) gerenciador = GerenciadorDeFocos.Obter();
        if (fluxo == null) fluxo = FindAnyObjectByType<FluxoDeJogo>();
        if (interactor == null) interactor = FindAnyObjectByType<Interactor>();

        if (gerenciador != null)
        {
            gerenciador.OnFocoReenchido -= HandleFocoReenchido;
            gerenciador.OnFocoReenchido += HandleFocoReenchido;
        }

        if (interactor != null)
        {
            interactor.OnErrouSkillCheck -= HandleErrouMinigame;
            interactor.OnErrouSkillCheck += HandleErrouMinigame;
        }

        if (fluxo != null)
        {
            fluxo.OnVitoria -= HandleVitoria;
            fluxo.OnVitoria += HandleVitoria;
            fluxo.OnDerrota -= HandleDerrota;
            fluxo.OnDerrota += HandleDerrota;
        }
    }

    void OnDestroy()
    {
        if (fluxo != null)
        {
            fluxo.OnVitoria -= HandleVitoria;
            fluxo.OnDerrota -= HandleDerrota;
        }

        if (gerenciador != null)
            gerenciador.OnFocoReenchido -= HandleFocoReenchido;

        if (interactor != null)
            interactor.OnErrouSkillCheck -= HandleErrouMinigame;
    }

    internal void UsarRecorde(RecordeDoJogo novo)
    {
        recorde = novo;
    }

    public bool MetaViva(MetaDaPartida meta) => !perdidas.Contains(meta);

    /* Derruba Resposta rápida e Hospital sob controle ao passar do limite. */
    internal void Conferir()
    {
        if (!EmPartida)
            return;

        DadosDaPartida d = Retrato(false);
        if (!AvaliacaoDaPartida.DentroDoLimite(MetaDaPartida.RespostaRapida, d, regras))
            Perder(MetaDaPartida.RespostaRapida);
        if (!AvaliacaoDaPartida.DentroDoLimite(MetaDaPartida.HospitalSobControle, d, regras))
            Perder(MetaDaPartida.HospitalSobControle);
    }

    /* Calcula e grava a nota. Só a primeira chamada vale. */
    public ResultadoDaNota Fechar(bool venceu)
    {
        if (Fechado)
            return Resultado;

        fechados = Retrato(venceu);
        Resultado = AvaliacaoDaPartida.Calcular(fechados, regras);
        RecordeNovo = Recorde.Registrar(Resultado.Pontos, Resultado.Estrelas);
        Fechado = true;
        return Resultado;
    }

    private DadosDaPartida Retrato(bool venceu)
    {
        DadosDaPartida d = new DadosDaPartida();
        d.Venceu = venceu;
        d.Reenchidos = Reenchidos;
        d.Erros = Erros;
        if (fluxo != null)
        {
            d.Segundos = fluxo.TempoDecorrido;
            d.Pico = fluxo.OcupacaoPico;
            d.Eliminados = fluxo.FocosEliminados;
            d.Total = fluxo.FocosTotal;
        }
        return d;
    }

    private void Perder(MetaDaPartida meta)
    {
        if (perdidas.Add(meta))
            OnMetaPerdida?.Invoke(meta);
    }

    private void HandleVitoria() { Fechar(true); }

    private void HandleDerrota() { Fechar(false); }

    private void HandleFocoReenchido(Interactable foco)
    {
        if (!EmPartida)
            return;

        Reenchidos++;
    }

    /* Um minigame errado (a tela do ralo rasgou): Sem erros cai. */
    internal void HandleErrouMinigame(Interactable foco)
    {
        if (!EmPartida)
            return;

        Erros++;
        Perder(MetaDaPartida.SemErros);
    }
}
