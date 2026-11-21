using UnityEngine;

/* ============================================================
 *  PontosDeBorrifar.cs
 *  ---------------------------------------------------------------------
 *  Lógica pura do minigame da bromélia (sem MonoBehaviour e sem
 *  UnityEngine.Time: o tempo entra pelo dt, testável no EditMode).
 *
 *  A câmera chega perto do copo da bromélia e as pocinhas viram bolinhas
 *  na tela, uma por vez, na ordem dada. O jogador leva a mira até a
 *  bolinha da vez e clica para borrifar:
 *
 *  - Campo: y de −1 (base da tela) a +1 (topo), x na mesma unidade, de
 *    −meiaLargura a +meiaLargura (a proporção da tela); centro em (0, 0).
 *  - Mira += Ponteiro × miraPorGrau (cada eixo não finito vale 0), presa
 *    ao campo. O Olhar é ignorado.
 *  - Aperto (Apertou): conta uma borrifada, guarda onde a mira estava e
 *    deixa Agindo por SegundosAgindo. Acerta se a bolinha da vez estiver
 *    na tela e a mira a até 'raio' dela: a seguinte entra com o anel cheio.
 *    Segurar não borrifa; só o aperto conta.
 *  - Anel: 1 cheio .. 0 fechado em 'segundosDoAnel'. Fechado, a bolinha
 *    some pela 'pausa' e volta com o anel cheio. Errar só custa tempo.
 *  - Espera inicial: nenhuma bolinha na tela até ela acabar.
 *  - Passos: o k-ésimo fica devido com Acertos >= ceil(Total·k/passos).
 *    Os que precisam de todos os acertos só saem depois do 'fecho' (o
 *    tempo do último borrifo aparecer). Sem pontos, todos já são devidos.
 *    No máximo um Passo por Atualizar. Nunca devolve Errou.
 *  - dt inválido (<= 0 ou não finito): nada muda, só sai um Passo devido.
 *
 *  Quem conduz é o Interactor.cs; a bromélia deriva desta classe.
 * ============================================================ */
public class PontosDeBorrifar : IMinigame
{
    public const float RaioPadrao = 0.14f, AnelPadrao = 1.5f, PausaPadrao = 0.35f, EsperaPadrao = 0.3f,
                       FechoPadrao = 0.45f, MeiaLarguraPadrao = 16f / 9f, MiraPorGrauPadrao = 0.0093f;

    /* Quanto tempo o Agindo dura depois de um aperto. */
    public const float SegundosAgindo = 0.2f;

    private readonly Vector2[] pontos;
    private readonly int passos;
    private readonly float segundosDoAnel;
    private readonly float pausa;
    private readonly float fecho;
    private readonly float meiaLargura;
    private readonly float miraPorGrau;

    private int passosDados;
    private float esperaRestante;
    private float pausaRestante;
    private float fechoRestante;
    private float agindoRestante;
    private float anel = 1f;

    public Vector2 Mira { get; private set; }
    public int Total => pontos.Length;
    public int Acertos { get; private set; }
    public float Raio { get; }
    public int Borrifadas { get; private set; }
    public Vector2 UltimaBorrifada { get; private set; }
    public bool UltimaAcertou { get; private set; }
    public bool PodeSair => true;
    public bool Agindo => agindoRestante > 0f;

    /* Índice da bolinha da vez; −1 na espera inicial e depois do último acerto. */
    public int DaVez => esperaRestante > 0f || Acertos >= pontos.Length ? -1 : Acertos;

    /* A bolinha depois da da vez; −1 se não houver. */
    public int Proxima => DaVez < 0 || DaVez + 1 >= pontos.Length ? -1 : DaVez + 1;

    /* False na espera inicial, na pausa e no fecho. */
    public bool BolinhaNaTela => DaVez >= 0 && pausaRestante <= 0f;

    /* 1 = cheio, 0 = fechado; 1 sem bolinha na tela. */
    public float Anel => BolinhaNaTela ? anel : 1f;

    /* Tudo acertado e ainda há passo a dar. */
    public bool NoFecho => pontos.Length > 0 && Acertos >= pontos.Length && passosDados < passos;

    public float FracaoDoPasso
    {
        get
        {
            if (passosDados >= passos) return 0f;
            int de = AcertosParaOPasso(passosDados);
            int preciso = AcertosParaOPasso(passosDados + 1) - de;
            if (preciso <= 0) return 1f;
            return Mathf.Clamp01((float)(Acertos - de) / preciso);
        }
    }

    public PontosDeBorrifar(Vector2[] pontos, int passos,
        float raio = RaioPadrao, float segundosDoAnel = AnelPadrao, float pausa = PausaPadrao,
        float esperaInicial = EsperaPadrao, float fecho = FechoPadrao,
        float meiaLargura = MeiaLarguraPadrao, float miraPorGrau = MiraPorGrauPadrao)
    {
        int n = pontos == null ? 0 : pontos.Length;
        this.pontos = new Vector2[n];
        for (int i = 0; i < n; i++)
        {
            Vector2 p = pontos[i];
            if (!float.IsFinite(p.x)) p.x = 0f;
            if (!float.IsFinite(p.y)) p.y = 0f;
            this.pontos[i] = p;
        }

        this.passos = Mathf.Max(1, passos);
        Raio = Positivo(raio, RaioPadrao);
        this.segundosDoAnel = Positivo(segundosDoAnel, AnelPadrao);
        this.meiaLargura = Positivo(meiaLargura, MeiaLarguraPadrao);
        this.miraPorGrau = Positivo(miraPorGrau, MiraPorGrauPadrao);
        this.pausa = NaoNegativo(pausa, PausaPadrao);
        esperaRestante = NaoNegativo(esperaInicial, EsperaPadrao);
        this.fecho = NaoNegativo(fecho, FechoPadrao);
    }

    private static float Positivo(float valor, float padrao)
        => valor > 0f && float.IsFinite(valor) ? valor : padrao;

    private static float NaoNegativo(float valor, float padrao)
        => valor >= 0f && float.IsFinite(valor) ? valor : padrao;

    /* Fora do intervalo: Vector2.zero. */
    public Vector2 Ponto(int k)
        => k >= 0 && k < pontos.Length ? pontos[k] : Vector2.zero;

    /* Acertos necessários para o k-ésimo Passo: ceil(Total·k/passos). */
    private int AcertosParaOPasso(int k)
    {
        if (k <= 0) return 0;
        return (int)(((long)pontos.Length * k + passos - 1) / passos);
    }

    public EstadoDoMinigame Atualizar(float dt, EntradaDoMinigame entrada)
    {
        /* !(dt > 0f) também pega NaN: nada muda, só sai um Passo que já seja devido. */
        if (dt > 0f && float.IsFinite(dt))
        {
            bool bolinhaNoComeco = BolinhaNaTela;
            MoverAMira(entrada.Ponteiro);
            AndarOsTempos(dt);
            Borrifar(dt, entrada.Apertou, bolinhaNoComeco);
        }
        return DarPasso();
    }

    private void MoverAMira(Vector2 ponteiro)
    {
        float x = float.IsFinite(ponteiro.x) ? ponteiro.x * miraPorGrau : 0f;
        float y = float.IsFinite(ponteiro.y) ? ponteiro.y * miraPorGrau : 0f;
        Mira = new Vector2(Mathf.Clamp(Mira.x + x, -meiaLargura, meiaLargura),
                           Mathf.Clamp(Mira.y + y, -1f, 1f));
    }

    private void AndarOsTempos(float dt)
    {
        agindoRestante = Mathf.Max(0f, agindoRestante - dt);

        if (esperaRestante > 0f)
        {
            esperaRestante = Mathf.Max(0f, esperaRestante - dt);
            if (esperaRestante <= 0f) anel = 1f;   // a primeira bolinha entra com o anel cheio
        }

        if (pausaRestante > 0f)
        {
            pausaRestante = Mathf.Max(0f, pausaRestante - dt);
            if (pausaRestante <= 0f) anel = 1f;    // a bolinha volta com o anel cheio
        }

        if (fechoRestante > 0f)
            fechoRestante = Mathf.Max(0f, fechoRestante - dt);
    }

    private void Borrifar(float dt, bool apertou, bool bolinhaNoComeco)
    {
        bool acertou = false;
        if (apertou)
        {
            Borrifadas++;
            UltimaBorrifada = Mira;
            agindoRestante = SegundosAgindo;

            acertou = BolinhaNaTela && Vector2.Distance(Mira, pontos[DaVez]) <= Raio;
            UltimaAcertou = acertou;
            if (acertou)
            {
                Acertos++;
                anel = 1f;
                if (Acertos >= pontos.Length) fechoRestante = fecho;
            }
        }

        if (!acertou && bolinhaNoComeco && BolinhaNaTela)
        {
            anel = Mathf.Max(0f, anel - dt / segundosDoAnel);
            if (anel <= 0f)
            {
                /* Sem pausa, a bolinha volta na hora com o anel cheio. */
                if (pausa > 0f) pausaRestante = pausa;
                else anel = 1f;
            }
        }
    }

    private EstadoDoMinigame DarPasso()
    {
        if (passosDados >= passos) return EstadoDoMinigame.EmAndamento;

        int preciso = AcertosParaOPasso(passosDados + 1);
        if (Acertos < preciso) return EstadoDoMinigame.EmAndamento;
        /* O passo que precisa de todos os acertos espera o fecho. */
        if (pontos.Length > 0 && preciso >= pontos.Length && fechoRestante > 0f)
            return EstadoDoMinigame.EmAndamento;

        passosDados++;
        return EstadoDoMinigame.Passo;
    }
}
