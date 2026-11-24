using UnityEngine;

/* ============================================================
 *  EsfregarCirculo.cs
 *  ---------------------------------------------------------------------
 *  Lógica pura do minigame do pote de bicho (sem MonoBehaviour e sem
 *  UnityEngine.Time: o tempo entra pelo dt, testável no EditMode).
 *
 *  A câmera olha o pote de cima e a parede de dentro ganha um círculo
 *  tracejado. O jogador leva a esponja com o mouse e, segurando o clique,
 *  passa por cima do círculo: cada trecho por onde a esponja passa fica
 *  limpo. Vale qualquer sentido e em quantas passadas quiser.
 *
 *  - Plano: em metros, no plano do círculo, com o centro do pote em
 *    (0, 0); x = direita da tela, y = cima da tela (a pose da câmera
 *    deixa os dois assim).
 *  - Esponja += Ponteiro × metrosPorGrau (cada eixo não finito vale 0),
 *    presa a um disco de raio 'limite'. Começa no círculo, embaixo (o
 *    lado do agente).
 *  - NoTraco: a esponja a até 'tolerancia' do círculo.
 *  - Esfregando = Segurando e NoTraco. Esfregando, o trecho da esponja
 *    fica limpo, e também todos os trechos entre ela e onde ela estava no
 *    quadro anterior (se lá também estava no traço e o salto foi menor
 *    que meia volta): mexer rápido não pula trecho.
 *  - Agindo: esfregando e mexendo, e por SegundosAgindo depois (o som e
 *    o gesto).
 *  - Passos: o k-ésimo fica devido com Limpos >= ceil(Segmentos·k/passos)
 *    (o último pede o círculo inteiro). No máximo um Passo por Atualizar.
 *    Nunca devolve Errou: sair da linha só não limpa.
 *  - dt inválido (<= 0 ou não finito): nada muda, só sai um Passo devido.
 *
 *  Quem conduz é o Interactor.cs; o pote (PoteDeBichoInteractable)
 *  deriva desta classe para pôr a pose da câmera.
 * ============================================================ */
public class EsfregarCirculo : IMinigame
{
    public const int SegmentosPadrao = 48;
    public const float MetrosPorGrauPadrao = 0.0016f;

    /* Quanto tempo o Agindo dura depois do último movimento esfregando. */
    public const float SegundosAgindo = 0.15f;

    private readonly bool[] limpos;
    private readonly int passos;
    private readonly float metrosPorGrau;
    private readonly float limite;

    private int passosDados;
    private float agindoRestante;
    private bool noTracoAntes;
    private float anguloAntes;

    public float Raio { get; }
    public float Tolerancia { get; }
    public int Segmentos => limpos.Length;
    public int Limpos { get; private set; }
    public Vector2 Esponja { get; private set; }
    public bool Esfregando { get; private set; }
    public bool PodeSair => true;
    public bool Agindo => agindoRestante > 0f;

    /* 0..1 do círculo já limpo. */
    public float FracaoLimpa => Segmentos == 0 ? 1f : (float)Limpos / Segmentos;

    public bool NoTraco => Mathf.Abs(Esponja.magnitude - Raio) <= Tolerancia;

    public float FracaoDoPasso
    {
        get
        {
            if (passosDados >= passos) return 0f;
            int de = LimposParaOPasso(passosDados);
            int preciso = LimposParaOPasso(passosDados + 1) - de;
            if (preciso <= 0) return 1f;
            return Mathf.Clamp01((float)(Limpos - de) / preciso);
        }
    }

    public EsfregarCirculo(float raio, int passos, float tolerancia, int segmentos = SegmentosPadrao,
                           float metrosPorGrau = MetrosPorGrauPadrao, float limite = 0f)
    {
        Raio = raio > 0f && float.IsFinite(raio) ? raio : 0.1f;
        Tolerancia = tolerancia > 0f && float.IsFinite(tolerancia) ? tolerancia : Raio * 0.3f;
        limpos = new bool[Mathf.Max(4, segmentos)];
        this.passos = Mathf.Max(1, passos);
        this.metrosPorGrau = metrosPorGrau > 0f && float.IsFinite(metrosPorGrau) ? metrosPorGrau : MetrosPorGrauPadrao;
        this.limite = limite > Raio + Tolerancia && float.IsFinite(limite) ? limite : (Raio + Tolerancia) * 1.4f;
        Esponja = new Vector2(0f, -Raio);
    }

    /* O trecho i já está limpo (fora do intervalo: false). */
    public bool Limpo(int i) => i >= 0 && i < limpos.Length && limpos[i];

    /* Ângulo do meio do trecho i, em radianos (0 = direita, anti-horário). */
    public float AnguloDoSegmento(int i) => 2f * Mathf.PI * (i + 0.5f) / limpos.Length;

    /* O trecho de um ângulo (radianos, qualquer volta). */
    public int SegmentoDe(float angulo)
    {
        float volta = Mathf.Repeat(angulo, 2f * Mathf.PI);
        return Mathf.Clamp((int)(volta / (2f * Mathf.PI) * limpos.Length), 0, limpos.Length - 1);
    }

    private int LimposParaOPasso(int k)
    {
        if (k <= 0) return 0;
        return (int)(((long)limpos.Length * k + passos - 1) / passos);
    }

    public EstadoDoMinigame Atualizar(float dt, EntradaDoMinigame entrada)
    {
        if (dt > 0f && float.IsFinite(dt))
        {
            Vector2 antes = Esponja;
            noTracoAntes = NoTraco;
            anguloAntes = Mathf.Atan2(antes.y, antes.x);
            Mover(entrada.Ponteiro);
            agindoRestante = Mathf.Max(0f, agindoRestante - dt);
            Esfregar(entrada.Segurando, (Esponja - antes).sqrMagnitude > 1e-12f);
        }
        return DarPasso();
    }

    private void Mover(Vector2 ponteiro)
    {
        float x = float.IsFinite(ponteiro.x) ? ponteiro.x * metrosPorGrau : 0f;
        float y = float.IsFinite(ponteiro.y) ? ponteiro.y * metrosPorGrau : 0f;
        Esponja = Vector2.ClampMagnitude(Esponja + new Vector2(x, y), limite);
    }

    private void Esfregar(bool segurando, bool mexeu)
    {
        Esfregando = segurando && NoTraco;
        if (!Esfregando)
            return;

        float angulo = Mathf.Atan2(Esponja.y, Esponja.x);
        Limpar(SegmentoDe(angulo));

        if (noTracoAntes)
        {
            // O arco entre o ângulo de antes e o de agora, pelo lado mais curto.
            float delta = Mathf.DeltaAngle(anguloAntes * Mathf.Rad2Deg, angulo * Mathf.Rad2Deg) * Mathf.Deg2Rad;
            if (Mathf.Abs(delta) < Mathf.PI)
            {
                float passo = 2f * Mathf.PI / limpos.Length * 0.5f;
                int n = Mathf.CeilToInt(Mathf.Abs(delta) / passo);
                for (int i = 1; i < n; i++)
                    Limpar(SegmentoDe(anguloAntes + delta * i / n));
            }
        }

        if (mexeu)
            agindoRestante = SegundosAgindo;
    }

    private void Limpar(int i)
    {
        if (limpos[i]) return;
        limpos[i] = true;
        Limpos++;
    }

    private EstadoDoMinigame DarPasso()
    {
        if (passosDados >= passos) return EstadoDoMinigame.EmAndamento;
        if (Limpos < LimposParaOPasso(passosDados + 1)) return EstadoDoMinigame.EmAndamento;

        passosDados++;
        return EstadoDoMinigame.Passo;
    }
}
