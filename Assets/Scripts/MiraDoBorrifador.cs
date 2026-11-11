using UnityEngine;

/* ============================================================
 *  MiraDoBorrifador.cs
 *  ---------------------------------------------------------------------
 *  Lógica pura do minigame do borrifador (sem MonoBehaviour e sem
 *  UnityEngine.Time: o tempo entra pelo dt, testável no EditMode).
 *
 *  A bromélia tem pocinhas d'água espalhadas. O jogador mexe a mira dentro
 *  de um cone e segura o botão para secar uma pocinha por vez:
 *
 *  - Mira (yaw, pitch) em graus a partir do olhar do começo, mesma convenção
 *    do ControladorJogador: Mira.x += Olhar.x, Mira.y -= Olhar.y. Fica presa
 *    a um cone de raio max(cone, maior |pocinha| + raio + 2), para toda
 *    pocinha ser alcançável.
 *  - Segurando, seca a pocinha molhada mais próxima (distância euclidiana
 *    em graus) que estiver a até 'raio' da mira. A Umidade (1 cheia .. 0 seca)
 *    fica guardada ao soltar. Sem segurar ou fora do raio, nada seca.
 *  - Os 'passos' são distribuídos pelas pocinhas: o k-ésimo Passo sai ao
 *    secar a ceil(Total·k/passos)-ésima. No máximo um Passo por Atualizar; os
 *    que ficarem devidos saem um por chamada nas seguintes. Sem pocinhas, todos
 *    os passos já são devidos.
 *  - Ir mal só custa tempo: nada aqui gasta carga nem reinicia o foco.
 *
 *  Quem conduz é o Interactor.cs; o BromeliaInteractable cria esta sessão.
 * ============================================================ */
public class MiraDoBorrifador : IMinigame, IMinigameComMira
{
    /* Tempo para secar quando o valor recebido não serve (<= 0 ou não finito). */
    private const float SegundosMinimosParaSecar = 0.01f;
    /* Limite do olhar de um quadro: mantém a conta da mira finita. */
    private const float OlharMaximoPorQuadro = 1e6f;

    private readonly Vector2[] pocas;
    private readonly float[] segundosParaSecar;
    private readonly float[] umidade;
    private readonly int passos;
    private readonly float raio;
    private readonly float coneEfetivo;

    private int passosDados;
    private int ultimaSecada = -1;

    public Vector2 Mira { get; private set; }
    public int Total => pocas.Length;
    public int Secas { get; private set; }
    public int PocaSecandoAgora { get; private set; } = -1;
    public bool PodeSair => true;

    public float FracaoDoPasso
    {
        get
        {
            if (passosDados >= passos) return 0f;
            int de = SecasParaOPasso(passosDados);
            int preciso = SecasParaOPasso(passosDados + 1) - de;
            if (preciso <= 0) return 0f;

            float parcial = 0f;
            if (ultimaSecada >= 0 && umidade[ultimaSecada] > 0f)
                parcial = 1f - umidade[ultimaSecada];
            return Mathf.Clamp01((Secas - de + parcial) / preciso);
        }
    }

    public MiraDoBorrifador(Vector2[] pocas, float[] segundosParaSecar, int passos, float cone = 30f, float raio = 4f)
    {
        int n = pocas == null || segundosParaSecar == null ? 0 : Mathf.Min(pocas.Length, segundosParaSecar.Length);
        this.pocas = new Vector2[n];
        this.segundosParaSecar = new float[n];
        umidade = new float[n];
        this.passos = Mathf.Max(1, passos);
        this.raio = raio;

        float maior = 0f;
        for (int i = 0; i < n; i++)
        {
            Vector2 p = pocas[i];
            if (!float.IsFinite(p.x)) p.x = 0f;
            if (!float.IsFinite(p.y)) p.y = 0f;
            this.pocas[i] = p;
            maior = Mathf.Max(maior, p.magnitude);

            float s = segundosParaSecar[i];
            this.segundosParaSecar[i] = s > 0f && float.IsFinite(s) ? s : SegundosMinimosParaSecar;
            umidade[i] = 1f;
        }
        coneEfetivo = Mathf.Max(cone, maior + raio + 2f);
    }

    /* Pocinhas que precisam estar secas para o k-ésimo Passo: ceil(Total·k/passos). */
    private int SecasParaOPasso(int k)
    {
        if (k <= 0) return 0;
        return (int)(((long)pocas.Length * k + passos - 1) / passos);
    }

    public float Umidade(int i)
    {
        return i >= 0 && i < umidade.Length ? umidade[i] : 0f;
    }

    public EstadoDoMinigame Atualizar(float dt, EntradaDoMinigame entrada)
    {
        /* !(dt > 0f) também pega NaN: nada muda, só sai um Passo que já seja devido. */
        if (dt > 0f)
        {
            MoverAMira(entrada.Olhar);
            Secar(dt, entrada.Segurando);
        }

        if (passosDados < passos && Secas >= SecasParaOPasso(passosDados + 1))
        {
            passosDados++;
            return EstadoDoMinigame.Passo;
        }
        return EstadoDoMinigame.EmAndamento;
    }

    private void MoverAMira(Vector2 olhar)
    {
        float x = float.IsFinite(olhar.x) ? Mathf.Clamp(olhar.x, -OlharMaximoPorQuadro, OlharMaximoPorQuadro) : 0f;
        float y = float.IsFinite(olhar.y) ? Mathf.Clamp(olhar.y, -OlharMaximoPorQuadro, OlharMaximoPorQuadro) : 0f;
        Mira = Vector2.ClampMagnitude(new Vector2(Mira.x + x, Mira.y - y), coneEfetivo);
    }

    private void Secar(float dt, bool segurando)
    {
        PocaSecandoAgora = -1;
        if (!segurando) return;

        float melhor = raio;
        for (int i = 0; i < pocas.Length; i++)
        {
            if (umidade[i] <= 0f) continue;
            float d = Vector2.Distance(Mira, pocas[i]);
            if (d <= melhor)
            {
                melhor = d;
                PocaSecandoAgora = i;
            }
        }
        if (PocaSecandoAgora < 0) return;

        int alvo = PocaSecandoAgora;
        ultimaSecada = alvo;
        /* dt enorme seca só esta pocinha: a conta termina em 0. */
        umidade[alvo] = Mathf.Max(0f, umidade[alvo] - dt / segundosParaSecar[alvo]);
        if (umidade[alvo] <= 0f) Secas++;
    }
}
