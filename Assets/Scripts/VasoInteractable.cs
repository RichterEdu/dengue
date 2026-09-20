using System.Collections;
using UnityEngine;

/* ============================================================
 *  VasoInteractable.cs
 *  ---------------------------------------------------------------------
 *  Foco do tipo VASO DE FLOR COM PRATINHO — herda Interactable.
 *
 *  Comportamento (GameDoc: "prato sem areia -> interagir -> prato com areia"):
 *  - 1º clique: a água parada do pratinho some e a areia começa a aparecer.
 *  - A cada clique a areia "enche" um pouco mais o pratinho.
 *  - No último clique (requiredClicks) o pratinho fica cheio de areia e o
 *    foco é considerado eliminado (evento OnCompleted é disparado).
 *  - O vaso CONTINUA na cena (não encolhe/some), a menos que
 *    removeOnComplete seja marcado.
 *
 *  O prefab é gerado pelo menu  Dengue > Criar Vaso de Flor com Pratinho
 *  (Assets/Editor/VasoComPratoBuilder.cs).
 * ============================================================ */

public class VasoInteractable : Interactable
{
    [Header("Vaso de Flor com Pratinho")]
    [Tooltip("Objeto filho com a água parada no pratinho (desativado no 1º clique).")]
    [SerializeField] private GameObject water;

    [Tooltip("Objeto filho com a areia do pratinho (pivô no fundo do prato).")]
    [SerializeField] private Transform sand;

    [Tooltip("Altura inicial da areia (fração) logo após o 1º clique.")]
    [Range(0.05f, 1f)]
    [SerializeField] private float minSandFill = 0.4f;

    [Tooltip("Duração da animação da areia subindo a cada clique.")]
    [SerializeField] private float sandFillDuration = 0.25f;

    [Tooltip("Se marcado, após encher de areia o vaso segue o fluxo padrão (encolhe e some).")]
    [SerializeField] private bool removeOnComplete = false;

    private Vector3 sandFullScale = Vector3.one;
    private Coroutine fillRoutine;

    /* 0 = vazio, 1 = pratinho cheio de areia. */
    public float SandFill { get; private set; }

    protected override void OnStart()
    {
        if (sand != null)
        {
            sandFullScale = sand.localScale;
            sand.gameObject.SetActive(false);
        }
        if (water != null)
            water.SetActive(true);
        SandFill = 0f;
    }

    protected override void OnInteract()
    {
        if (water != null && water.activeSelf)
            water.SetActive(false);

        float target = RequiredClicks <= 1
            ? 1f
            : Mathf.Lerp(minSandFill, 1f, (CurrentClicks - 1f) / (RequiredClicks - 1f));

        SetSandFill(target);
        base.OnInteract();
    }

    protected override void OnComplete()
    {
        if (removeOnComplete)
            base.OnComplete();
        // Caso contrário, o vaso fica na cena com o pratinho cheio de areia.
    }

    private void SetSandFill(float target)
    {
        SandFill = Mathf.Clamp01(target);
        if (sand == null)
            return;

        if (!sand.gameObject.activeSelf)
        {
            sand.gameObject.SetActive(true);
            // Começa já um pouco acima do fundo (evita a borda da areia atravessar a aba do prato).
            sand.localScale = new Vector3(sandFullScale.x, sandFullScale.y * minSandFill * 0.6f, sandFullScale.z);
        }

        if (fillRoutine != null)
            StopCoroutine(fillRoutine);
        fillRoutine = StartCoroutine(FillRoutine(SandFill));
    }

    private IEnumerator FillRoutine(float target)
    {
        float startY = sand.localScale.y;
        float endY = sandFullScale.y * target;

        for (float t = 0f; t < sandFillDuration; t += Time.deltaTime)
        {
            float k = 1f - Mathf.Pow(1f - t / sandFillDuration, 3f); // ease-out
            sand.localScale = new Vector3(sandFullScale.x, Mathf.Lerp(startY, endY, k), sandFullScale.z);
            yield return null;
        }

        sand.localScale = new Vector3(sandFullScale.x, endY, sandFullScale.z);
        fillRoutine = null;
    }
}
