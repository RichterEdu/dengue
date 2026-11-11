using UnityEngine;

/* ============================================================
 *  OndaDoPneu.cs
 *  ---------------------------------------------------------------------
 *  Lógica pura do minigame do pneu (sem MonoBehaviour e sem
 *  UnityEngine.Time: o tempo entra pelo dt, testável no EditMode).
 *
 *  A água dentro do pneu é um oscilador amortecido:
 *      v' = -ω²·x - 2·amortecimento·ω·v + ganho·u
 *  O jogador balança o pneu para os lados (u = olhar horizontal normalizado,
 *  -1..1). Empurrar no ritmo da onda (em fase com a velocidade) faz a
 *  amplitude crescer; fora do ritmo ela se anula. Quando |x| passa de
 *  limiarDaBorda a água transborda e o Nivel (1 cheio .. 0 vazio) cai.
 *
 *  - Inclinacao: x da onda; ±1 = água na borda.
 *  - Cada 1/passos de Nivel esvaziado é um Passo (no máximo um por Atualizar;
 *    o Nivel não passa do limite do passo, o resto fica para o quadro seguinte).
 *  - Ir mal só custa tempo: nada aqui gasta carga nem reinicia o foco.
 *
 *  Quem conduz é o Interactor.cs; o PneuInteractable cria esta sessão.
 * ============================================================ */
public class OndaDoPneu : IMinigame
{
    private const float PassoMaximoDeSimulacao = 1f / 120f;
    /* Quadro maior que isto (pausa, engasgo) é simulado só até aqui: o estado
     * fica finito e o custo por quadro limitado. */
    private const float DtMaximo = 0.25f;

    private readonly int passos;
    private readonly float omega;
    private readonly float amortecimento;
    private readonly float ganho;
    private readonly float limiarDaBorda;
    private readonly float taxa;
    private readonly float grausPorSegundoCheio;

    private int passosFeitos;

    public float Inclinacao { get; private set; }
    public float Velocidade { get; private set; }
    public float Nivel { get; private set; } = 1f;
    public bool Derramando { get; private set; }
    public bool PodeSair => true;
    /* Balançando o pneu o tempo todo: som de ação e gesto das Mãos. */
    public bool Agindo => true;

    public float FracaoDoPasso
    {
        get
        {
            float feito = (1f - (float)passosFeitos / passos) - Nivel;
            return Mathf.Clamp01(feito * passos);
        }
    }

    public OndaDoPneu(int passos, float periodo = 0.9f, float amortecimento = 0.12f, float ganho = 15f,
                      float limiarDaBorda = 1f, float taxa = 1.4f, float grausPorSegundoCheio = 180f,
                      float nivelInicial = 1f)
    {
        this.passos = Mathf.Max(1, passos);
        omega = 2f * Mathf.PI / periodo;
        this.amortecimento = amortecimento;
        this.ganho = ganho;
        this.limiarDaBorda = limiarDaBorda;
        this.taxa = taxa;
        this.grausPorSegundoCheio = grausPorSegundoCheio;

        /* Retomada: começa do nível já esvaziado; os passos que ele implica
         * contam como dados (a tolerância cobre o erro de ponto flutuante de
         * um nível exatamente sobre o limite do passo). */
        Nivel = float.IsFinite(nivelInicial) ? Mathf.Clamp01(nivelInicial) : 1f;
        passosFeitos = Mathf.Clamp(Mathf.FloorToInt((1f - Nivel) * this.passos + 1e-4f), 0, this.passos);
    }

    public EstadoDoMinigame Atualizar(float dt, EntradaDoMinigame entrada)
    {
        /* !(dt > 0f) também pega NaN. */
        if (!(dt > 0f) || passosFeitos >= passos)
            return EstadoDoMinigame.EmAndamento;

        float u = Mathf.Clamp(entrada.Olhar.x / dt / grausPorSegundoCheio, -1f, 1f);
        if (!float.IsFinite(u)) u = 0f;
        dt = Mathf.Min(dt, DtMaximo);
        float limite = passosFeitos + 1 >= passos ? 0f : 1f - (float)(passosFeitos + 1) / passos;

        int subpassos = Mathf.Max(1, Mathf.CeilToInt(dt / PassoMaximoDeSimulacao));
        float h = dt / subpassos;
        Derramando = false;

        for (int i = 0; i < subpassos; i++)
        {
            Velocidade += (-omega * omega * Inclinacao - 2f * amortecimento * omega * Velocidade + ganho * u) * h;
            Inclinacao += Velocidade * h;

            float excesso = Mathf.Abs(Inclinacao) - limiarDaBorda;
            if (excesso > 0f)
            {
                Derramando = true;
                Nivel = Mathf.Max(limite, Nivel - taxa * excesso * h);
            }
        }

        if (Nivel <= limite)
        {
            passosFeitos++;
            return EstadoDoMinigame.Passo;
        }
        return EstadoDoMinigame.EmAndamento;
    }
}
