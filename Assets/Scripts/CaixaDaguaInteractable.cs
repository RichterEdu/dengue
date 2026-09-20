using System.Collections;
using UnityEngine;

/* ============================================================
 *  CaixaDaguaInteractable.cs
 *  ---------------------------------------------------------------------
 *  Foco do tipo CAIXA D'ÁGUA DESTAMPADA — herda Interactable.
 *
 *  Comportamento (GameDoc: "caixa d'água destampada -> interagir ->
 *  caixa tampada"):
 *  - A caixa começa ABERTA, com a tampa encostada na lateral e a água
 *    parada à mostra lá dentro.
 *  - A cada clique a tampa desliza um pouco em direção à boca da caixa
 *    (posição e rotação interpoladas), como se o agente estivesse
 *    empurrando ela para o lugar.
 *  - No último clique a tampa assenta, a caixa fica fechada e o foco é
 *    eliminado (evento OnCompleted é disparado).
 *  - A caixa CONTINUA na cena (não encolhe/some), a menos que
 *    removeOnComplete seja marcado.
 *
 *  O prefab é gerado pelo menu  Dengue > Criar Caixa d'Agua
 *  (Assets/Editor/CaixaDaguaBuilder.cs), que já preenche as poses.
 * ============================================================ */

public class CaixaDaguaInteractable : Interactable
{
    [Header("Caixa d'Água")]
    [Tooltip("Objeto filho com a tampa da caixa.")]
    [SerializeField] private Transform tampa;

    [Tooltip("Posição local da tampa com a caixa ABERTA (encostada na lateral).")]
    [SerializeField] private Vector3 posAberta = new Vector3(0.44f, 0.45f, 0f);

    [Tooltip("Rotação local (euler) da tampa com a caixa ABERTA.")]
    [SerializeField] private Vector3 rotAberta = new Vector3(0f, 0f, 62f);

    [Tooltip("Posição local da tampa com a caixa FECHADA (assentada na boca).")]
    [SerializeField] private Vector3 posFechada = new Vector3(0f, 0.762f, 0f);

    [Tooltip("Rotação local (euler) da tampa com a caixa FECHADA.")]
    [SerializeField] private Vector3 rotFechada = Vector3.zero;

    [Tooltip("Duração da animação da tampa a cada clique.")]
    [SerializeField] private float duracaoDoMovimento = 0.3f;

    [Tooltip("Altura extra do arco que a tampa descreve no caminho (0 = desliza reto).")]
    [SerializeField] private float alturaDoArco = 0.20f;

    [Tooltip("Objeto filho com a água parada (opcional). Some quando a caixa fecha.")]
    [SerializeField] private GameObject agua;

    [Tooltip("Se marcado, após fechar a caixa segue o fluxo padrão (encolhe e some).")]
    [SerializeField] private bool removeOnComplete = false;

    private Coroutine moveRoutine;

    /* 0 = tampa encostada na lateral, 1 = caixa fechada. */
    public float Fechamento { get; private set; }

    protected override void OnStart()
    {
        Fechamento = 0f;
        if (agua != null)
            agua.SetActive(true);
        AplicarPose(0f);
    }

    protected override void OnInteract()
    {
        float alvo = RequiredClicks <= 0 ? 1f : (float)CurrentClicks / RequiredClicks;
        MoverTampa(alvo);
        base.OnInteract();
    }

    protected override void OnComplete()
    {
        // Garante o assentamento exato mesmo se a animação for interrompida.
        MoverTampa(1f);

        if (removeOnComplete)
            base.OnComplete();
        // Caso contrário, a caixa fica na cena, fechada.
    }

    private void MoverTampa(float alvo)
    {
        Fechamento = Mathf.Clamp01(alvo);
        if (tampa == null)
            return;

        if (moveRoutine != null)
            StopCoroutine(moveRoutine);

        if (duracaoDoMovimento <= 0.01f)
        {
            AplicarPose(Fechamento);
            return;
        }

        moveRoutine = StartCoroutine(MoveRoutine(Fechamento));
    }

    private IEnumerator MoveRoutine(float alvo)
    {
        Vector3 posInicial = tampa.localPosition;
        Quaternion rotInicial = tampa.localRotation;

        // De onde a pose atual "está" no caminho, para o arco continuar coerente.
        float inicio = EstimarProgresso(posInicial);

        for (float t = 0f; t < duracaoDoMovimento; t += Time.deltaTime)
        {
            float k = 1f - Mathf.Pow(1f - t / duracaoDoMovimento, 3f);   // ease-out
            float p = Mathf.Lerp(inicio, alvo, k);
            tampa.localPosition = Vector3.Lerp(posInicial, PosicaoEm(alvo), k) + Vector3.up * Arco(p);
            tampa.localRotation = Quaternion.Slerp(rotInicial, RotacaoEm(alvo), k);
            yield return null;
        }

        AplicarPose(alvo);
        moveRoutine = null;
    }

    private void AplicarPose(float p)
    {
        if (tampa == null)
            return;
        tampa.localPosition = PosicaoEm(p) + Vector3.up * Arco(p);
        tampa.localRotation = RotacaoEm(p);

        // Com a caixa fechada a água some (já ficaria escondida, mas evita
        // z-fighting da lâmina d'água com a tampa e economiza um draw).
        if (agua != null)
            agua.SetActive(p < 0.999f);
    }

    private Vector3 PosicaoEm(float p)
    {
        return Vector3.Lerp(posAberta, posFechada, p);
    }

    private Quaternion RotacaoEm(float p)
    {
        return Quaternion.Slerp(Quaternion.Euler(rotAberta), Quaternion.Euler(rotFechada), p);
    }

    /* Levanta a tampa no meio do caminho: ela sobe e assenta, em vez de
     * atravessar a parede da caixa. */
    private float Arco(float p)
    {
        return alturaDoArco * Mathf.Sin(Mathf.Clamp01(p) * Mathf.PI);
    }

    /* Onde a pose atual está entre aberta e fechada (usado para continuar
     * o arco de onde a tampa parou). */
    private float EstimarProgresso(Vector3 posAtual)
    {
        Vector3 caminho = posFechada - posAberta;
        float mag = caminho.sqrMagnitude;
        if (mag < 1e-6f)
            return Fechamento;
        return Mathf.Clamp01(Vector3.Dot(posAtual - posAberta, caminho) / mag);
    }
}
