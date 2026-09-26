using System.Collections;
using UnityEngine;

/* ============================================================
 *  RaloInteractable.cs
 *  ---------------------------------------------------------------------
 *  Foco do tipo RALO DESTAMPADO — herda Interactable.
 *
 *  Comportamento ("ralo aberto -> interagir -> ralo tampado com tela"):
 *  - O ralo começa ABERTO: a grelha está tirada, encostada ao lado, e a
 *    água parada aparece lá dentro.
 *  - A cada clique a grelha vai voltando para a boca do ralo (posição e
 *    rotação interpoladas, com um arco para não atravessar a moldura).
 *  - No último clique a grelha assenta, a água some e a TELA de proteção
 *    aparece embaixo dela. O ralo continua na cena.
 *
 *  Não herda CaixaDaguaInteractable de propósito: FocoListaHUD e
 *  HistoriaRedeSocial classificam o foco pelo tipo C# ("is"), e o ralo
 *  seria contado como caixa d'água.
 *
 *  O modelo vem do Blender (Ferramentas/Blender/gerar_focos.py) e o
 *  prefab é gerado pelo menu  Dengue > Criar Ralo  (FocosBlenderBuilder.cs).
 * ============================================================ */

public class RaloInteractable : Interactable
{
    [Header("Ralo")]
    [Tooltip("Objeto filho com a grelha do ralo.")]
    [SerializeField] private Transform tampa;

    [Tooltip("Posição local da grelha com o ralo ABERTO (tirada, ao lado).")]
    [SerializeField] private Vector3 posAberta;

    [Tooltip("Rotação local (euler) da grelha com o ralo ABERTO.")]
    [SerializeField] private Vector3 rotAberta;

    [Tooltip("Posição local da grelha com o ralo FECHADO (assentada na boca).")]
    [SerializeField] private Vector3 posFechada;

    [Tooltip("Rotação local (euler) da grelha com o ralo FECHADO.")]
    [SerializeField] private Vector3 rotFechada;

    [Tooltip("Duração da animação da grelha a cada clique.")]
    [SerializeField] private float duracaoDoMovimento = 0.25f;

    [Tooltip("Altura do arco que a grelha descreve no caminho.")]
    [SerializeField] private float alturaDoArco = 0.08f;

    [Tooltip("Objeto filho com a água parada. Some quando o ralo fecha.")]
    [SerializeField] private GameObject agua;

    [Tooltip("Tela de proteção. Aparece quando o ralo fecha.")]
    [SerializeField] private GameObject tela;

    [Tooltip("Se marcado, após fechar o ralo segue o fluxo padrão (encolhe e some).")]
    [SerializeField] private bool removeOnComplete = false;

    private Coroutine moveRoutine;

    /* 0 = grelha tirada, 1 = ralo fechado. */
    public float Fechamento { get; private set; }

    protected override void OnStart()
    {
        Fechamento = 0f;
        if (agua != null) agua.SetActive(true);
        if (tela != null) tela.SetActive(false);
        AplicarPose(0f, 0f);
    }

    protected override void OnInteract()
    {
        float alvo = RequiredClicks <= 0 ? 1f : (float)CurrentClicks / RequiredClicks;
        MoverTampa(alvo);
        base.OnInteract();
    }

    protected override void OnComplete()
    {
        MoverTampa(1f);
        if (agua != null) agua.SetActive(false);
        if (tela != null) tela.SetActive(true);

        if (removeOnComplete)
            base.OnComplete();
    }

    private void MoverTampa(float alvo)
    {
        float inicio = Fechamento;
        Fechamento = Mathf.Clamp01(alvo);
        if (tampa == null)
            return;

        if (moveRoutine != null)
            StopCoroutine(moveRoutine);

        if (duracaoDoMovimento <= 0.01f)
            AplicarPose(Fechamento, 0f);
        else
            moveRoutine = StartCoroutine(MoveRoutine(inicio, Fechamento));
    }

    private IEnumerator MoveRoutine(float de, float para)
    {
        for (float t = 0f; t < duracaoDoMovimento; t += Time.deltaTime)
        {
            float k = 1f - Mathf.Pow(1f - t / duracaoDoMovimento, 3f);   // ease-out
            AplicarPose(Mathf.Lerp(de, para, k), Mathf.Sin(k * Mathf.PI));
            yield return null;
        }

        AplicarPose(para, 0f);
        moveRoutine = null;
    }

    /* p = progresso aberta->fechada; arco = 0..1 do "levanta e assenta". */
    private void AplicarPose(float p, float arco)
    {
        if (tampa == null)
            return;
        tampa.localPosition = Vector3.Lerp(posAberta, posFechada, p) + Vector3.up * (alturaDoArco * arco);
        tampa.localRotation = Quaternion.Slerp(Quaternion.Euler(rotAberta), Quaternion.Euler(rotFechada), p);
    }
}
