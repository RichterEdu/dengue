using UnityEngine;

/* ============================================================
 *  InclinarLona.cs
 *  ---------------------------------------------------------------------
 *  Lógica pura do minigame da lona (sem MonoBehaviour e sem
 *  UnityEngine.Time: o tempo entra pelo dt, testável no EditMode).
 *
 *  A câmera olha a lona de cima. O mouse inclina a lona como uma bandeja
 *  e as poças escorrem para o lado mais baixo. A borda da esquerda (−x)
 *  é a saída: poça que passa dela escorre para fora da lona.
 *
 *  - Plano: em metros, com o centro da lona em (0, 0); x = direita da
 *    tela, y = cima da tela. A lona vai de −MeiaExtensao a +MeiaExtensao.
 *  - Inclinacao += Ponteiro × inclinacaoPorGrau (cada eixo não finito
 *    vale 0), presa ao disco de raio 1 (1 = inclinação máxima). A
 *    inclinação aponta para o lado que desce. Sem mexer, ela volta aos
 *    poucos a zero (Inclinacao /= 1 + retorno × dt): o agente segura a
 *    lona erguida, e soltar o mouse deixa ela cair de volta.
 *  - Cada poça que ainda está na lona: v += Inclinacao × aceleracao × dt;
 *    v /= 1 + atrito × dt; p += v × dt. Passou de x < −MeiaExtensao.x:
 *    Saiu (para de mexer). Nas outras três bordas a poça para encostada
 *    a 'margem' delas, e a velocidade naquele eixo zera.
 *  - Agindo: alguma poça andando acima de VelocidadeAgindo (o som da
 *    água).
 *  - Passos: o k-ésimo fica devido com Saidas >= ceil(Total·k/passos).
 *    Sem poça, todos já são devidos. No máximo um Passo por Atualizar.
 *    Nunca devolve Errou.
 *  - dt inválido (<= 0 ou não finito): nada muda, só sai um Passo devido.
 *
 *  Quem conduz é o Interactor.cs; a lona (LonaInteractable) deriva desta
 *  classe para pôr a pose da câmera.
 * ============================================================ */
public class InclinarLona : IMinigame
{
    public const float AceleracaoPadrao = 0.8f, AtritoPadrao = 1.4f, InclinacaoPorGrauPadrao = 0.012f,
                       RetornoPadrao = 1.2f;
    public const float VelocidadeAgindo = 0.05f;

    private readonly Vector2[] posicoes;
    private readonly Vector2[] velocidades;
    private readonly bool[] sairam;
    private readonly int passos;
    private readonly float aceleracao;
    private readonly float atrito;
    private readonly float inclinacaoPorGrau;
    private readonly float margem;
    private readonly float retorno;

    private int passosDados;
    private bool agindo;

    public Vector2 MeiaExtensao { get; }
    public Vector2 Inclinacao { get; private set; }
    public int Total => posicoes.Length;
    public int Saidas { get; private set; }
    public bool PodeSair => true;
    public bool Agindo => agindo;

    public float FracaoDoPasso
    {
        get
        {
            if (passosDados >= passos) return 0f;
            int de = SaidasParaOPasso(passosDados);
            int preciso = SaidasParaOPasso(passosDados + 1) - de;
            if (preciso <= 0) return 1f;
            return Mathf.Clamp01((float)(Saidas - de) / preciso);
        }
    }

    public InclinarLona(Vector2[] pocas, Vector2 meiaExtensao, int passos, float margem = 0.1f,
                        float aceleracao = AceleracaoPadrao, float atrito = AtritoPadrao,
                        float inclinacaoPorGrau = InclinacaoPorGrauPadrao, float retorno = RetornoPadrao)
    {
        MeiaExtensao = new Vector2(Positivo(meiaExtensao.x, 1f), Positivo(meiaExtensao.y, 0.7f));
        this.margem = Mathf.Clamp(float.IsFinite(margem) ? margem : 0f, 0f, Mathf.Min(MeiaExtensao.x, MeiaExtensao.y) * 0.9f);

        int n = pocas == null ? 0 : pocas.Length;
        posicoes = new Vector2[n];
        velocidades = new Vector2[n];
        sairam = new bool[n];
        for (int i = 0; i < n; i++)
            posicoes[i] = PresaNasBordas(new Vector2(float.IsFinite(pocas[i].x) ? pocas[i].x : 0f,
                                                     float.IsFinite(pocas[i].y) ? pocas[i].y : 0f), ref velocidades[i]);

        this.passos = Mathf.Max(1, passos);
        this.aceleracao = Positivo(aceleracao, AceleracaoPadrao);
        this.atrito = float.IsFinite(atrito) && atrito >= 0f ? atrito : AtritoPadrao;
        this.inclinacaoPorGrau = Positivo(inclinacaoPorGrau, InclinacaoPorGrauPadrao);
        this.retorno = float.IsFinite(retorno) && retorno >= 0f ? retorno : RetornoPadrao;
    }

    private static float Positivo(float valor, float padrao)
        => valor > 0f && float.IsFinite(valor) ? valor : padrao;

    /* Fora do intervalo: Vector2.zero. */
    public Vector2 Posicao(int i) => i >= 0 && i < posicoes.Length ? posicoes[i] : Vector2.zero;

    /* A poça i já escorreu para fora (fora do intervalo: false). */
    public bool Saiu(int i) => i >= 0 && i < sairam.Length && sairam[i];

    private int SaidasParaOPasso(int k)
    {
        if (k <= 0) return 0;
        return (int)(((long)posicoes.Length * k + passos - 1) / passos);
    }

    public EstadoDoMinigame Atualizar(float dt, EntradaDoMinigame entrada)
    {
        if (dt > 0f && float.IsFinite(dt))
        {
            Inclinar(entrada.Ponteiro, dt);
            Escorrer(dt);
        }
        return DarPasso();
    }

    private void Inclinar(Vector2 ponteiro, float dt)
    {
        Inclinacao /= 1f + retorno * dt;
        float x = float.IsFinite(ponteiro.x) ? ponteiro.x * inclinacaoPorGrau : 0f;
        float y = float.IsFinite(ponteiro.y) ? ponteiro.y * inclinacaoPorGrau : 0f;
        Inclinacao = Vector2.ClampMagnitude(Inclinacao + new Vector2(x, y), 1f);
    }

    private void Escorrer(float dt)
    {
        agindo = false;
        for (int i = 0; i < posicoes.Length; i++)
        {
            if (sairam[i])
                continue;

            Vector2 v = velocidades[i] + Inclinacao * aceleracao * dt;
            v /= 1f + atrito * dt;
            Vector2 p = posicoes[i] + v * dt;

            if (p.x < -MeiaExtensao.x)
            {
                sairam[i] = true;
                Saidas++;
                posicoes[i] = p;
                velocidades[i] = Vector2.zero;
                continue;
            }

            posicoes[i] = PresaNasBordas(p, ref v);
            velocidades[i] = v;
            if (v.magnitude > VelocidadeAgindo)
                agindo = true;
        }
    }

    /* A poça para encostada nas bordas que não são a saída (a da esquerda
     * fica livre: é por ela que a água sai). */
    private Vector2 PresaNasBordas(Vector2 p, ref Vector2 v)
    {
        float maxX = MeiaExtensao.x - margem;
        float maxY = MeiaExtensao.y - margem;
        if (p.x > maxX) { p.x = maxX; if (v.x > 0f) v.x = 0f; }
        if (p.y > maxY) { p.y = maxY; if (v.y > 0f) v.y = 0f; }
        if (p.y < -maxY) { p.y = -maxY; if (v.y < 0f) v.y = 0f; }
        return p;
    }

    private EstadoDoMinigame DarPasso()
    {
        if (passosDados >= passos) return EstadoDoMinigame.EmAndamento;
        if (Saidas < SaidasParaOPasso(passosDados + 1)) return EstadoDoMinigame.EmAndamento;

        passosDados++;
        return EstadoDoMinigame.Passo;
    }
}
