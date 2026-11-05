using System;
using UnityEngine;

/* ============================================================
 *  Chuva.cs
 *  ---------------------------------------------------------------------
 *  O relógio da chuva (Parte B): a chuva é o preço do atalho de esvaziar
 *  com as Mãos. Todo foco só esvaziado (e não tratado) volta a ter água
 *  quando ela começa, e retorna como larva.
 *
 *    - Iniciar(rng), no OnPartidaIniciada do FluxoDeJogo: relógio em 0,
 *      Seco e o primeiro início sorteado em intervaloEntreInicios;
 *    - Avancar(dt) anda o relógio e faz, em ordem, todas as transições
 *      que o dt atravessa:
 *        Seco → Previsao   em ProximoInicio − segundosDePrevisao (OnPrevisao);
 *        Previsao → Chovendo em ProximoInicio: Chuvas++, o gerenciador
 *                          reenche os esvaziados, OnComecou e o próximo
 *                          início é sorteado (ProximoInicio += intervalo);
 *        Chovendo → Seco   segundosChovendo depois do começo (OnTerminou);
 *    - sem o Iniciar, ou com a partida encerrada, o relógio fica parado.
 *
 *  Quem mostra a chuva é o ChuvaVisual (luz, névoa e gotas), que ouve
 *  OnPrevisao, OnComecou e OnTerminou.
 *
 *  No EditMode o Start/Update não rodam: os testes ligam o gerenciador e
 *  o fluxo nos campos e chamam Iniciar e Avancar direto. Em Play, os
 *  campos vazios viram o GerenciadorDeFocos.Obter() e o FluxoDeJogo da
 *  cena.
 * ============================================================ */

public enum EstadoDaChuva { Seco, Previsao, Chovendo }

public class Chuva : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("Focos da partida (a chuva reenche os esvaziados). Vazio = o GerenciadorDeFocos da cena.")]
    [SerializeField] private GerenciadorDeFocos gerenciador;

    [Tooltip("Fluxo da partida (a chuva começa a contar no início da partida e para quando ela acaba). Vazio = procura o FluxoDeJogo na cena.")]
    [SerializeField] private FluxoDeJogo fluxo;

    [Header("Tempos (segundos)")]
    [Tooltip("Intervalo entre o início de uma chuva e o da seguinte, sorteado entre x e y. A primeira conta a partir do início da partida.")]
    [SerializeField] private Vector2 intervaloEntreInicios = new Vector2(180f, 240f);

    [Tooltip("Quantos segundos antes de cada chuva sai a previsão (o céu começa a fechar).")]
    [Min(0f)]
    [SerializeField] private float segundosDePrevisao = 45f;

    [Tooltip("Quanto tempo cada chuva dura.")]
    [Min(0f)]
    [SerializeField] private float segundosChovendo = 20f;

    // Menor intervalo aceito no sorteio: com 0, o laço do Avancar não sairia.
    private const float IntervaloMinimo = 1f;

    /* A previsão saiu (o céu começa a fechar). */
    public event Action OnPrevisao;

    /* A chuva começou (os esvaziados já foram reenchidos). */
    public event Action OnComecou;

    /* A chuva parou. */
    public event Action OnTerminou;

    public EstadoDaChuva Estado { get; private set; } = EstadoDaChuva.Seco;

    /* Segundos desde o Iniciar. */
    public float Tempo { get; private set; }

    /* Tempo (no relógio da chuva) em que a próxima chuva começa. */
    public float ProximoInicio { get; private set; }

    /* Quantas chuvas já começaram nesta partida. */
    public int Chuvas { get; private set; }

    private System.Random rng;
    private bool iniciada;
    private float inicioDaChuvaAtual;

    // Fluxo cujo OnPartidaIniciada está assinado (para desassinar no OnDestroy).
    private FluxoDeJogo fluxoAssinado;

    void Start()
    {
        if (gerenciador == null) gerenciador = GerenciadorDeFocos.Obter();
        if (fluxo == null) fluxo = FindAnyObjectByType<FluxoDeJogo>();

        if (fluxo != null)
        {
            fluxo.OnPartidaIniciada += AoIniciarPartida;
            fluxoAssinado = fluxo;

            // A partida pode ter começado antes deste Start.
            if (fluxo.PartidaIniciada && !iniciada)
                Iniciar(new System.Random());
        }
    }

    void Update()
    {
        Avancar(Time.deltaTime);
    }

    /* Só pelo campo em cache: procurar ou criar objetos aqui (fim do Play,
     * troca de cena) deixaria objeto novo para trás. */
    void OnDestroy()
    {
        if (fluxoAssinado != null)
            fluxoAssinado.OnPartidaIniciada -= AoIniciarPartida;
        fluxoAssinado = null;
    }

    private void AoIniciarPartida()
    {
        Iniciar(new System.Random());
    }

    /* Começo da partida: relógio em 0, Seco, nenhuma chuva ainda e o
     * primeiro início sorteado. rng null = um System.Random novo. */
    public void Iniciar(System.Random rng)
    {
        this.rng = rng ?? new System.Random();
        iniciada = true;
        Tempo = 0f;
        Estado = EstadoDaChuva.Seco;
        Chuvas = 0;
        inicioDaChuvaAtual = 0f;
        ProximoInicio = SortearIntervalo();
    }

    /* Anda o relógio dt segundos, disparando em ordem cada transição que o
     * dt atravessa (um dt grande pode passar por previsão, chuva e fim). */
    public void Avancar(float dt)
    {
        if (!iniciada || dt <= 0f)
            return;
        if (fluxo != null && fluxo.PartidaEncerrada)
            return;

        float fim = Tempo + dt;
        while (true)
        {
            float limite = ProximoLimite();
            if (limite > fim)
                break;

            // O limite pode já ter passado (previsão maior que o intervalo):
            // a transição sai na hora, sem o relógio voltar.
            Tempo = Mathf.Max(Tempo, limite);
            Transicionar();
        }
        Tempo = fim;
    }

    /* Quando acontece a próxima transição do estado atual. */
    private float ProximoLimite()
    {
        switch (Estado)
        {
            case EstadoDaChuva.Previsao: return ProximoInicio;
            case EstadoDaChuva.Chovendo: return inicioDaChuvaAtual + segundosChovendo;
            default: return ProximoInicio - segundosDePrevisao;
        }
    }

    private void Transicionar()
    {
        switch (Estado)
        {
            case EstadoDaChuva.Seco:
                Estado = EstadoDaChuva.Previsao;
                OnPrevisao?.Invoke();
                break;

            case EstadoDaChuva.Previsao:
                Estado = EstadoDaChuva.Chovendo;
                inicioDaChuvaAtual = ProximoInicio;
                Chuvas++;
                if (gerenciador != null)
                    gerenciador.ReencherEsvaziados();
                OnComecou?.Invoke();
                ProximoInicio += SortearIntervalo();
                break;

            case EstadoDaChuva.Chovendo:
                Estado = EstadoDaChuva.Seco;
                OnTerminou?.Invoke();
                break;
        }
    }

    /* Uniforme em [x, y] de intervaloEntreInicios (nunca abaixo de
     * IntervaloMinimo). */
    private float SortearIntervalo()
    {
        float x = intervaloEntreInicios.x;
        float y = intervaloEntreInicios.y;
        float sorteado = x + (y - x) * (float)rng.NextDouble();
        return Mathf.Max(IntervaloMinimo, sorteado);
    }
}
