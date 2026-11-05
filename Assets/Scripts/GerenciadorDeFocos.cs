using System;
using System.Collections.Generic;
using UnityEngine;

/* ============================================================
 *  GerenciadorDeFocos.cs
 *  ---------------------------------------------------------------------
 *  Os focos vivos (Parte B): guarda um CicloDeVida por foco (ovo →
 *  larva → mosquito), conta quantos já foram eliminados e diz quanto os
 *  focos de pé pesam na barra de ocupação (Pressao).
 *
 *    - Registrar assina os eventos de cada foco e os repassa num lugar
 *      só (OnFocoEliminado, OnFocoClicado, OnFocoEsvaziado,
 *      OnFocoReenchido); o mesmo foco nunca entra duas vezes, e os já
 *      concluídos contam direto em Eliminados. O Awake chama Garantir,
 *      que registra todos os Interactable da cena, inclusive inativos;
 *    - Avancar(dt) anda o relógio de cada foco ainda de pé e com água e
 *      avisa OnEstagioMudou quando o estágio muda. O Update só chama com
 *      CicloAtivo (partida iniciada e não encerrada no FluxoDeJogo);
 *    - esvaziado (o atalho das Mãos), o foco volta para ovo e pesa 0;
 *      reenchido (a chuva, ReencherEsvaziados), volta como larva;
 *    - PesoDe: 0 se concluído ou esvaziado; senão, o peso do estágio
 *      (pesoOvo, pesoLarva, pesoMosquito). Pressao é a média dos pesos
 *      sobre o Total, inclusive os eliminados (que pesam 0);
 *    - SortearInicio, no OnPartidaIniciada do FluxoDeJogo: alguns focos
 *      já começam com mosquito, uma fração com larva e o resto com ovo,
 *      cada um com um tempo sorteado dentro do estágio (as eclosões não
 *      acontecem todas juntas). A regra em si é o SortearEstagios, sem
 *      foco nenhum: o simulador de balanceamento usa a mesma.
 *
 *  No EditMode o Awake/Start/Update não rodam: os testes chamam
 *  Registrar, Avancar e SortearInicio direto, e o FluxoDeJogo do campo
 *  vazio é procurado no acesso até ser achado (de novo se o achado for
 *  destruído). O OnPartidaIniciada dele é assinado uma vez só: já no
 *  Awake (o gerenciador criado em runtime pelo Obter() não perde a
 *  partida que começa antes do Start dele), de novo no Start (se o fluxo
 *  só apareceu depois) e quando a busca o acha; o getter CicloAtivo não
 *  assina nada.
 *
 *  Quem precisa do gerenciador usa o do campo ou GerenciadorDeFocos.Obter()
 *  (o da cena, inclusive em objeto inativo, já com os focos registrados;
 *  em Play, sem nenhum, cria um, menos quando o jogo está fechando).
 * ============================================================ */

public class GerenciadorDeFocos : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("Fluxo da partida (o ciclo só anda com a partida iniciada e não encerrada). Vazio = procura o FluxoDeJogo na cena.")]
    [SerializeField] private FluxoDeJogo fluxo;

    [Header("Ciclo de vida")]
    [Tooltip("Segundos de cada estágio do foco (ovo → larva → mosquito).")]
    [Min(1f)]
    [SerializeField] private float segundosPorEstagio = 75f;

    [Header("Peso na barra de ocupação")]
    [Tooltip("Peso de um foco com ovos na pressão sobre a ocupação.")]
    [SerializeField] private float pesoOvo = 1f;

    [Tooltip("Peso de um foco com larvas na pressão sobre a ocupação.")]
    [SerializeField] private float pesoLarva = 1.5f;

    [Tooltip("Peso de um foco com mosquitos na pressão sobre a ocupação.")]
    [SerializeField] private float pesoMosquito = 2.5f;

    [Header("Início da partida (sorteado)")]
    [Tooltip("Quantos focos já começam com mosquitos (no máximo, os focos restantes).")]
    [SerializeField] private int focosComMosquitoNoInicio = 3;

    [Tooltip("Dos outros focos, a fração que já começa com larvas (arredondada); o resto começa com ovos.")]
    [Range(0f, 1f)]
    [SerializeField] private float fracaoLarvaNoInicio = 1f / 3f;

    /* Um foco eliminado (conta uma vez por foco). */
    public event Action<Interactable> OnFocoEliminado;

    /* Repasses do OnClicado, OnEsvaziado e OnReenchido de cada foco. */
    public event Action<Interactable> OnFocoClicado;
    public event Action<Interactable> OnFocoEsvaziado;
    public event Action<Interactable> OnFocoReenchido;

    /* O estágio de um foco mudou (avanço do relógio, esvaziar, reencher ou sorteio). */
    public event Action<Interactable, EstagioDoFoco> OnEstagioMudou;

    private readonly List<Interactable> focos = new List<Interactable>();
    private readonly Dictionary<Interactable, CicloDeVida> ciclos = new Dictionary<Interactable, CicloDeVida>();
    private readonly HashSet<Interactable> eliminados = new HashSet<Interactable>();

    // Garantir já registrou os focos da cena.
    private bool registrou;

    // Fluxo cujo OnPartidaIniciada está assinado (para desassinar no OnDestroy).
    private FluxoDeJogo fluxoAssinado;

    // A busca já achou um FluxoDeJogo (só repete se ele for destruído).
    private bool procurouFluxo;

    // O jogo está fechando: Obter() não cria gerenciador novo (sobraria na cena).
    private static bool saindo;

    public IReadOnlyList<Interactable> Focos => focos;
    public int Total => focos.Count;
    public int Eliminados => eliminados.Count;
    public int Restantes => Total - Eliminados;
    public float SegundosPorEstagio => segundosPorEstagio;

    /* O ciclo só anda com a partida rolando. */
    public bool CicloAtivo
    {
        get
        {
            FluxoDeJogo f = AcharFluxo();
            return f != null && f.PartidaIniciada && !f.PartidaEncerrada;
        }
    }

    /* O FluxoDeJogo do campo ou, vazio, o da cena. A busca repete até
     * achar um e, depois, só se o fluxo assinado foi destruído (a
     * referência ainda existe, mas o Unity a vê como null). Achado, o
     * OnPartidaIniciada dele é assinado (sorteio). */
    private FluxoDeJogo AcharFluxo()
    {
        bool assinadoDestruido = (object)fluxoAssinado != null && fluxoAssinado == null;
        if (fluxo == null && (!procurouFluxo || assinadoDestruido))
        {
            if (assinadoDestruido)
                fluxoAssinado = null;

            fluxo = FindAnyObjectByType<FluxoDeJogo>();
            procurouFluxo = fluxo != null;
            AssinarFluxo();
        }
        return fluxo;
    }

    /* Assina o OnPartidaIniciada do fluxo atual uma vez (e solta o antigo). */
    private void AssinarFluxo()
    {
        if (fluxo == null || fluxo == fluxoAssinado)
            return;

        if (fluxoAssinado != null)
            fluxoAssinado.OnPartidaIniciada -= AoIniciarPartida;
        fluxo.OnPartidaIniciada += AoIniciarPartida;
        fluxoAssinado = fluxo;
    }

    /* Ao entrar em Play (inclusive sem recarregar o domínio): zera o
     * "saindo" e assina o Application.quitting uma vez só. */
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void AoEntrarEmPlay()
    {
        saindo = false;
        Application.quitting -= AoSair;
        Application.quitting += AoSair;
    }

    private static void AoSair()
    {
        saindo = true;
    }

    /* O gerenciador da cena (inclusive num objeto inativo, que não teve
     * Awake). Em Play, o achado já sai com os focos registrados (Garantir:
     * quem acorda antes do Awake dele recebe a lista pronta) e, se não
     * houver nenhum, cria um (o Awake dele registra os focos), menos com o
     * jogo fechando. Fora do Play só devolve o que achar. */
    public static GerenciadorDeFocos Obter()
    {
        GerenciadorDeFocos achado = FindAnyObjectByType<GerenciadorDeFocos>(FindObjectsInactive.Include);
        if (!Application.isPlaying)
            return achado;

        if (achado != null)
            achado.Garantir();
        else if (!saindo)
            achado = new GameObject("GerenciadorDeFocos").AddComponent<GerenciadorDeFocos>();
        return achado;
    }

    /* Assina o fluxo já aqui: o gerenciador criado em runtime pelo Obter()
     * só tem o Start no frame seguinte, e uma partida que começasse antes
     * disso passaria sem o sorteio (OnPartidaIniciada). O AssinarFluxo não
     * assina duas vezes o mesmo fluxo. */
    void Awake()
    {
        Garantir();
        AcharFluxo();
        AssinarFluxo();
    }

    void Start()
    {
        // De novo no Start: o fluxo pode ter aparecido depois do Awake (o
        // do campo é assinado direto; vazio, o achado pela busca).
        AcharFluxo();
        AssinarFluxo();
    }

    void Update()
    {
        if (CicloAtivo)
            Avancar(Time.deltaTime);
    }

    void OnDestroy()
    {
        foreach (Interactable foco in focos)
        {
            if (foco == null)
                continue;
            foco.OnCompleted -= AoConcluir;
            foco.OnClicado -= AoClicar;
            foco.OnEsvaziado -= AoEsvaziar;
            foco.OnReenchido -= AoReencher;
        }

        if (fluxoAssinado != null)
            fluxoAssinado.OnPartidaIniciada -= AoIniciarPartida;
        fluxoAssinado = null;
    }

    /* Registra todos os focos da cena (inclusive inativos), uma vez só. */
    public void Garantir()
    {
        if (registrou)
            return;

        registrou = true;
        // Sem FindObjectsSortMode: a sobrecarga com ele está obsoleta no Unity 6.5.
        Registrar(FindObjectsByType<Interactable>(FindObjectsInactive.Include));
    }

    /* Cria o ciclo de cada foco novo, assina os eventos dele e conta os já
     * concluídos. Foco null ou já registrado é ignorado. */
    public void Registrar(IEnumerable<Interactable> novos)
    {
        if (novos == null)
            return;

        foreach (Interactable foco in novos)
        {
            if (foco == null || ciclos.ContainsKey(foco))
                continue;

            focos.Add(foco);
            ciclos.Add(foco, new CicloDeVida(segundosPorEstagio));
            if (foco.IsComplete)
                eliminados.Add(foco);

            foco.OnCompleted += AoConcluir;
            foco.OnClicado += AoClicar;
            foco.OnEsvaziado += AoEsvaziar;
            foco.OnReenchido += AoReencher;
        }
    }

    /* Estágio do ciclo do foco (Ovo se não está registrado). */
    public EstagioDoFoco EstagioDe(Interactable foco)
    {
        if (foco != null && ciclos.TryGetValue(foco, out CicloDeVida ciclo))
            return ciclo.Estagio;
        return EstagioDoFoco.Ovo;
    }

    /* Peso do foco na pressão: 0 se concluído, esvaziado ou não registrado;
     * senão, o peso do estágio. */
    public float PesoDe(Interactable foco)
    {
        if (foco == null || foco.IsComplete || foco.Esvaziado || !ciclos.ContainsKey(foco))
            return 0f;

        return PesoDoEstagio(EstagioDe(foco));
    }

    /* Peso do estágio na pressão (valores do Inspector). */
    public float PesoDoEstagio(EstagioDoFoco estagio)
    {
        switch (estagio)
        {
            case EstagioDoFoco.Larva: return pesoLarva;
            case EstagioDoFoco.Mosquito: return pesoMosquito;
            default: return pesoOvo;
        }
    }

    /* Média dos pesos sobre o Total (os eliminados entram pesando 0). */
    public float Pressao()
    {
        if (Total == 0)
            return 0f;

        float soma = 0f;
        foreach (Interactable foco in focos)
            soma += PesoDe(foco);
        return soma / Total;
    }

    /* Anda o relógio dos focos de pé e com água; avisa quem mudou de estágio.
     * Por índice: quem ouve o OnEstagioMudou pode registrar focos no meio
     * (os novos entram no fim e já andam nesta mesma chamada). */
    public void Avancar(float dt)
    {
        for (int i = 0; i < focos.Count; i++)
        {
            Interactable foco = focos[i];
            if (foco == null || foco.IsComplete || foco.Esvaziado)
                continue;

            CicloDeVida ciclo = ciclos[foco];
            if (ciclo.Avancar(dt))
                OnEstagioMudou?.Invoke(foco, ciclo.Estagio);
        }
    }

    /* A chuva: reenche todo foco que pode (PodeReencher) e devolve quantos. */
    public int ReencherEsvaziados()
    {
        // Cópia: quem ouve o OnReenchido pode registrar focos no meio.
        int reenchidos = 0;
        foreach (Interactable foco in focos.ToArray())
        {
            if (foco == null || !foco.PodeReencher)
                continue;

            foco.Reencher();
            reenchidos++;
        }
        return reenchidos;
    }

    /* Começo da partida: aplica o SortearEstagios nos focos de pé e com água. */
    public void SortearInicio(System.Random rng)
    {
        List<Interactable> candidatos = new List<Interactable>();
        foreach (Interactable foco in focos)
        {
            if (foco != null && !foco.IsComplete && !foco.Esvaziado)
                candidatos.Add(foco);
        }

        foreach (var (indice, estagio, tempo) in SortearEstagios(candidatos.Count, rng))
        {
            CicloDeVida ciclo = ciclos[candidatos[indice]];
            EstagioDoFoco antes = ciclo.Estagio;
            ciclo.Definir(estagio, tempo);
            if (estagio != EstagioDoFoco.Ovo || estagio != antes)
                OnEstagioMudou?.Invoke(candidatos[indice], estagio);
        }
    }

    /* A regra do sorteio, sem tocar em foco nenhum: de 'quantos' focos,
     * min(focosComMosquitoNoInicio, quantos) com mosquito,
     * round(fracaoLarvaNoInicio × outros) com larva e o resto com ovo, cada
     * um com o tempo sorteado em [0, segundosPorEstagio). 'indice' é a
     * posição do foco (0 a quantos − 1). O simulador de balanceamento usa
     * este mesmo método. */
    public (int indice, EstagioDoFoco estagio, float tempo)[] SortearEstagios(int quantos, System.Random rng)
    {
        if (rng == null)
            rng = new System.Random();

        // Embaralha (Fisher-Yates): quem fica com mosquito e larva é sorteado.
        int[] ordem = new int[quantos];
        for (int i = 0; i < quantos; i++)
            ordem[i] = i;
        for (int i = quantos - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (ordem[i], ordem[j]) = (ordem[j], ordem[i]);
        }

        int mosquitos = Mathf.Clamp(focosComMosquitoNoInicio, 0, quantos);
        int larvas = Mathf.Clamp(Mathf.RoundToInt(fracaoLarvaNoInicio * (quantos - mosquitos)), 0, quantos - mosquitos);

        var sorteio = new (int indice, EstagioDoFoco estagio, float tempo)[quantos];
        for (int i = 0; i < quantos; i++)
        {
            EstagioDoFoco estagio = i < mosquitos ? EstagioDoFoco.Mosquito
                : i < mosquitos + larvas ? EstagioDoFoco.Larva
                : EstagioDoFoco.Ovo;

            // O arredondamento para float pode dar o próprio fim do estágio.
            float tempo = (float)(rng.NextDouble() * segundosPorEstagio);
            if (tempo >= segundosPorEstagio)
                tempo = 0f;

            sorteio[i] = (ordem[i], estagio, tempo);
        }
        return sorteio;
    }

    private void AoIniciarPartida()
    {
        SortearInicio(new System.Random());
    }

    /* Concluído (inclusive o pneu do caminho antigo, logo depois do
     * OnEsvaziado): conta uma vez por foco. */
    private void AoConcluir(Interactable foco)
    {
        if (!eliminados.Add(foco))
            return;

        OnFocoEliminado?.Invoke(foco);
    }

    private void AoClicar(Interactable foco)
    {
        OnFocoClicado?.Invoke(foco);
    }

    /* Esvaziado: os ovos ficam, o relógio volta a zero. */
    private void AoEsvaziar(Interactable foco)
    {
        if (ciclos.TryGetValue(foco, out CicloDeVida ciclo))
        {
            ciclo.Zerar();
            OnEstagioMudou?.Invoke(foco, EstagioDoFoco.Ovo);
        }
        OnFocoEsvaziado?.Invoke(foco);
    }

    /* Reenchido pela chuva: os ovos grudados na parede eclodem, volta larva. */
    private void AoReencher(Interactable foco)
    {
        if (ciclos.TryGetValue(foco, out CicloDeVida ciclo))
        {
            ciclo.Reencher();
            OnEstagioMudou?.Invoke(foco, EstagioDoFoco.Larva);
        }
        OnFocoReenchido?.Invoke(foco);
    }
}
