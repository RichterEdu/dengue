using System.Collections;
using UnityEngine;

/* ============================================================
 *  PiscinaInteractable.cs
 *  ---------------------------------------------------------------------
 *  Foco do tipo PISCINA DE PLÁSTICO / BACIA — herda Interactable.
 *
 *  Comportamento (a lição real do agente de saúde: não basta esvaziar,
 *  tem que guardar de boca para baixo):
 *  - A piscina começa cheia de água parada esverdeada.
 *  - A cada clique ela TOMBA um pouco mais, girando em volta da própria
 *    borda que está apoiada no chão — como a gente vira uma bacia de
 *    verdade, e não em volta do centro (senão ela afundaria no chão).
 *  - A água escorre nos primeiros graus do tombo: encolhe e some,
 *    mantendo-se sempre HORIZONTAL enquanto existe (água não inclina
 *    junto com o balde).
 *  - No último clique a piscina fecha o giro e fica emborcada, de boca
 *    para baixo, ao lado de onde estava. O foco é eliminado aí.
 *  - A piscina CONTINUA na cena (não encolhe/some), a menos que
 *    removeOnComplete seja marcado.
 *
 *  O prefab é gerado pelo menu  Dengue > Criar Piscina de Plastico
 *  (Assets/Editor/PiscinaBuilder.cs), que já preenche as referências.
 * ============================================================ */

public class PiscinaInteractable : Interactable
{
    [Header("Piscina")]
    [Tooltip("Objeto filho que tomba (a malha da piscina). A água é filha dele.")]
    [SerializeField] private Transform corpo;

    [Tooltip("Objeto filho com a lâmina de água parada.")]
    [SerializeField] private GameObject agua;

    [Header("Tombo")]
    [Tooltip("Quanto ela gira do início ao fim. 180 = emborcada de boca para baixo.")]
    [SerializeField] private float anguloFinal = 180f;

    [Tooltip("Distância do centro até a borda que toca o chão (o eixo do tombo).")]
    [SerializeField] private float raioDaBorda = 0.41f;

    [Tooltip("Encosta a piscina no chão a cada frame do giro. Sem isso ela afunda no chão no meio do tombo.")]
    [SerializeField] private bool apoiarNoChao = true;

    [Tooltip("Duração da animação a cada clique.")]
    [SerializeField] private float duracaoDoMovimento = 0.38f;

    [Tooltip("Com quanto do tombo a água já secou (0.35 = escorreu toda nos primeiros 35%).")]
    [Range(0.05f, 1f)]
    [SerializeField] private float fracaoParaEsvaziar = 0.35f;

    [Tooltip("Se marcado, após virar a piscina segue o fluxo padrão (encolhe e some).")]
    [SerializeField] private bool removeOnComplete = false;

    private Vector3 posInicial;
    private Quaternion rotInicial;
    private Vector3 escalaDaAgua = Vector3.one;
    private Renderer rendCorpo;
    private Coroutine moveRoutine;

    /* 0 = em pé e cheia, 1 = emborcada. */
    public float Tombo { get; private set; }

    protected override void OnStart()
    {
        if (corpo == null)
            corpo = transform;

        posInicial = corpo.localPosition;
        rotInicial = corpo.localRotation;
        rendCorpo = corpo.GetComponent<Renderer>();

        if (agua != null)
        {
            escalaDaAgua = agua.transform.localScale;
            agua.SetActive(true);
        }

        Tombo = 0f;
        AplicarPose(0f);
    }

    protected override void OnInteract()
    {
        float alvo = RequiredClicks <= 0 ? 1f : (float)CurrentClicks / RequiredClicks;
        Tombar(alvo);
        base.OnInteract();
    }

    protected override void OnComplete()
    {
        // Garante o tombo completo mesmo se a animação for interrompida.
        Tombar(1f);

        if (removeOnComplete)
            base.OnComplete();
        // Caso contrário, a piscina fica na cena, emborcada.
    }

    private void Tombar(float alvo)
    {
        Tombo = Mathf.Clamp01(alvo);
        if (corpo == null)
            return;

        if (moveRoutine != null)
            StopCoroutine(moveRoutine);

        if (duracaoDoMovimento <= 0.01f)
        {
            AplicarPose(Tombo);
            return;
        }

        moveRoutine = StartCoroutine(MoveRoutine(Tombo));
    }

    private IEnumerator MoveRoutine(float alvo)
    {
        float inicio = ProgressoAtual();

        for (float t = 0f; t < duracaoDoMovimento; t += Time.deltaTime)
        {
            float k = 1f - Mathf.Pow(1f - t / duracaoDoMovimento, 3f);   // ease-out
            AplicarPose(Mathf.Lerp(inicio, alvo, k));
            yield return null;
        }

        AplicarPose(alvo);
        moveRoutine = null;
    }

    /* Gira o corpo em volta da borda apoiada no chão, em vez do centro:
     * é isso que faz a piscina "rolar" sobre a borda como um objeto de
     * verdade, sem afundar no chão nem flutuar. */
    private void AplicarPose(float p)
    {
        if (corpo == null)
            return;

        float angulo = Mathf.Lerp(0f, anguloFinal, p);
        Quaternion giro = Quaternion.AngleAxis(-angulo, Vector3.forward);
        Vector3 eixo = new Vector3(raioDaBorda, 0f, 0f);

        corpo.localPosition = eixo + giro * (posInicial - eixo);
        corpo.localRotation = giro * rotInicial;

        if (apoiarNoChao)
            EncostarNoChao();

        AtualizarAgua(p);
    }

    /* Girar em volta de um ponto fixo enterraria a piscina no chão na
     * metade do caminho (e a faria flutuar na outra metade). Como um
     * objeto de verdade rola sobre a borda, a regra certa é simples:
     * o ponto mais baixo encosta no chão o tempo todo. */
    private void EncostarNoChao()
    {
        if (rendCorpo == null)
            return;

        float chao = transform.position.y;          // a raiz fica no nível do chão
        float maisBaixo = rendCorpo.bounds.min.y;   // AABB do corpo já girado
        corpo.position += Vector3.up * (chao - maisBaixo);
    }

    private void AtualizarAgua(float p)
    {
        if (agua == null)
            return;

        float k = Mathf.Clamp01(p / Mathf.Max(0.01f, fracaoParaEsvaziar));

        if (k >= 1f)
        {
            if (agua.activeSelf)
                agua.SetActive(false);
            return;
        }

        if (!agua.activeSelf)
            agua.SetActive(true);

        // Encolhe uniforme (sem distorcer) e continua na horizontal: a
        // água não inclina junto com a piscina.
        agua.transform.localScale = escalaDaAgua * (1f - k);
        agua.transform.rotation = Quaternion.identity;
    }

    /* Onde o corpo está entre em pé e emborcado — para a animação
     * continuar de onde parou se o jogador clicar rápido. */
    private float ProgressoAtual()
    {
        if (corpo == null || Mathf.Abs(anguloFinal) < 0.01f)
            return Tombo;

        float angulo = Quaternion.Angle(rotInicial, corpo.localRotation);
        return Mathf.Clamp01(angulo / Mathf.Abs(anguloFinal));
    }
}
