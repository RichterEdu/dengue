using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* ============================================================
 *  Interactable.cs
 *  ---------------------------------------------------------------------
 *  Classe BASE genérica para objetos interativos do jogo (focos de
 *  criadouro do Aedes aegypti: pneus, caixas d'água, vasos, etc).
 *
 *  - O player clica via Interactor.cs (raycast do centro da tela).
 *  - Cada clique dispara OnInteract() (virtual, reutilizável) e um
 *    "punch" de escala (squash & stretch) como feedback imediato.
 *  - Ao atingir requiredClicks, dispara OnCompleted/OnComplete().
 *  - Conclusão padrão: POP (expande), FLASH branco de emissão,
 *    estouro de partículas, e encolhe com easing + giro + pulinho.
 *
 *  Para focos com comportamento próprio, herde esta classe e sobrescreva
 *  OnInteract()/OnComplete() (ver PneuInteractable.cs como exemplo).
 * ============================================================ */

public class Interactable : MonoBehaviour
{
    /* Disparado quando o foco é concluído (referência do próprio objeto). */
    public event System.Action<Interactable> OnCompleted;

    [Header("Interação")]
    [Tooltip("Quantos cliques o jogador precisa dar para eliminar este foco.")]
    [SerializeField] protected int requiredClicks = 5;

    [Tooltip("Distância máxima (em unidades) para o jogador interagir.")]
    [SerializeField] protected float interactionRange = 3f;

    [Header("Feedback por clique")]
    [Tooltip("Fração da escala usada no 'punch' a cada clique (0 = sem punch).")]
    [SerializeField] protected float punchScale = 0.1f;

    [Tooltip("Duração do 'punch' (expande e volta), em segundos.")]
    [SerializeField] protected float punchDuration = 0.14f;

    [Header("Feedback de conclusão")]
    [Tooltip("Fração da escala inicial removida POR CLIQUE (0 = não encolhe a cada clique).")]
    [SerializeField] protected float scaleReductionPerClick = 0f;

    [Tooltip("Duração da animação de encolher até sumir (em segundos).")]
    [SerializeField] protected float shrinkDuration = 0.8f;

    [Tooltip("Altura do 'pulinho' ao concluir (em unidades).")]
    [SerializeField] protected float hopHeight = 0.35f;

    [Tooltip("Inclinação/giro (graus) ao encolher no final.")]
    [SerializeField] protected float tiltDegrees = 55f;

    [Tooltip("Cor do flash de emissão ao concluir.")]
    [SerializeField] protected Color flashColor = Color.white;

    [Tooltip("Cor das partículas do estouro.")]
    [SerializeField] protected Color popColor = new Color(0.25f, 1f, 0.8f);

    [Tooltip("Quantidade de partículas do estouro.")]
    [SerializeField] protected int particleCount = 16;

    [Tooltip("Ao concluir: true = Destroi o objeto, false = apenas desativa (reutilizável).")]
    [SerializeField] protected bool destroyOnComplete = false;

    private Vector3 initialScale;
    protected int currentClicks;

    private Coroutine punchRoutine;

    /* --- Propriedades públicas (para feedback externo) --- */
    public int RequiredClicks => requiredClicks;
    public int CurrentClicks => currentClicks;
    public bool CanInteract => currentClicks < requiredClicks;
    public bool IsComplete => currentClicks >= requiredClicks;

    void Start()
    {
        currentClicks = 0;
        initialScale = transform.localScale;
        OnStart();
    }

    /* Chamado pelo Interactor quando o jogador clica neste objeto. */
    public void Interact()
    {
        if (!CanInteract)
            return;

        currentClicks++;

        // Redução de escala a cada clique (optativo)
        if (scaleReductionPerClick > 0f && !IsComplete)
        {
            float factor = Mathf.Max(0f, 1f - scaleReductionPerClick * currentClicks);
            transform.localScale = initialScale * factor;
        }

        OnInteract();

        if (IsComplete)
        {
            if (punchRoutine != null)
            {
                StopCoroutine(punchRoutine);
                punchRoutine = null;
            }
            OnCompleted?.Invoke(this);
            OnComplete();
        }
        else
        {
            PlayPunch();
        }
    }

    /* Feedback rápido por clique: expande e volta com leve overshoot. */
    private void PlayPunch()
    {
        if (punchScale <= 0f)
            return;

        if (punchRoutine != null)
            StopCoroutine(punchRoutine);
        punchRoutine = StartCoroutine(PunchRoutine());
    }

    private IEnumerator PunchRoutine()
    {
        Vector3 baseScale = transform.localScale;
        Vector3 peakScale = baseScale * (1f + punchScale);
        float half = punchDuration * 0.5f;

        // Fase 1: expande rapidamente.
        for (float t = 0f; t < half; t += Time.deltaTime)
        {
            transform.localScale = Vector3.Lerp(baseScale, peakScale, t / half);
            yield return null;
        }
        transform.localScale = peakScale;

        // Fase 2: volta ao normal com um pequeno overshoot (efeito "mole").
        for (float t = 0f; t < half; t += Time.deltaTime)
        {
            float k = t / half;
            float overshoot = 1f + Mathf.Sin(k * Mathf.PI) * 0.04f;
            transform.localScale = Vector3.Lerp(peakScale, baseScale, k) * overshoot;
            yield return null;
        }

        transform.localScale = baseScale;
        punchRoutine = null;
    }

    /* Hook chamado após cada clique. Sobrescreva em classes derivadas. */
    protected virtual void OnInteract() { }

    /* Hook chamado quando requiredClicks é atingido. */
    protected virtual void OnComplete()
    {
        if (shrinkDuration <= 0f)
        {
            FinishRemoval();
            return;
        }
        StartCoroutine(CompleteAnimation());
    }

    /* Sequência de conclusão com "sinal": pop, flash, partículas e encolhe vivo. */
    private IEnumerator CompleteAnimation()
    {
        Vector3 startScale = transform.localScale;
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.localRotation;

        // 1) POP: expande rapidamente.
        float pop = 0.13f;
        Vector3 poppedScale = startScale * 1.18f;
        for (float t = 0f; t < pop; t += Time.deltaTime)
        {
            transform.localScale = Vector3.Lerp(startScale, poppedScale, t / pop);
            yield return null;
        }
        transform.localScale = poppedScale;

        // 2) FLASH de emissão + estouro de partículas (em paralelo).
        Coroutine flash = StartCoroutine(FlashRoutine());
        SpawnPop(transform.position + Vector3.up * 0.3f);
        yield return flash;

        // 3) Encolhe com easing + giro + pulinho.
        float elapsed = 0f;
        while (elapsed < shrinkDuration)
        {
            elapsed += Time.deltaTime;
            float k = Mathf.Clamp01(elapsed / shrinkDuration);
            float e = 1f - Mathf.Pow(1f - k, 3f); // ease-out cúbico

            transform.localScale = Vector3.Lerp(poppedScale, Vector3.zero, e);
            transform.position = startPos + Vector3.up * (hopHeight * Mathf.Sin(k * Mathf.PI));
            transform.localRotation = startRot * Quaternion.Euler(tiltDegrees * k, 120f * k, 0f);
            yield return null;
        }

        transform.localScale = Vector3.zero;
        transform.position = startPos;
        transform.localRotation = startRot;
        FinishRemoval();
    }

    /* Eleva a emissão ao branco e retorna ao estado original do material. */
    private IEnumerator FlashRoutine()
    {
        List<Renderer> renderers = new List<Renderer>();
        List<Color> originals = new List<Color>();
        List<bool> enabled = new List<bool>();

        foreach (Renderer rend in GetComponentsInChildren<Renderer>())
        {
            if (rend == null || rend.sharedMaterial == null)
                continue;
            Material mat = rend.material;
            renderers.Add(rend);
            originals.Add(mat.GetColor("_EmissionColor"));
            enabled.Add(mat.IsKeywordEnabled("_EMISSION"));
        }

        float elapsed = 0f;
        float duration = 0.13f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float fade = 1f - elapsed / duration;
            Color c = flashColor * (fade * 3f);
            for (int i = 0; i < renderers.Count; i++)
            {
                if (renderers[i] == null)
                    continue;
                Material mat = renderers[i].material;
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", c);
            }
            yield return null;
        }

        for (int i = 0; i < renderers.Count; i++)
        {
            if (renderers[i] == null)
                continue;
            Material mat = renderers[i].material;
            if (enabled[i])
                mat.SetColor("_EmissionColor", originals[i]);
            else
                mat.DisableKeyword("_EMISSION");
        }
    }

    /* Cria um estouro de partículas em runtime (sem assets externos). */
    private void SpawnPop(Vector3 worldPos)
    {
        GameObject go = new GameObject("PopFeedback");
        go.transform.position = worldPos;

        ParticleSystem ps = go.AddComponent<ParticleSystem>();

        ParticleSystem.MainModule main = ps.main;
        main.startColor = popColor;
        main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.25f);
        main.startLifetime = 0.7f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 3f);
        main.maxParticles = Mathf.Max(particleCount, 8);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = 0.25f;

        ParticleSystem.EmissionModule emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0f, (short)Mathf.Max(particleCount, 1))
        });

        ParticleSystem.ShapeModule shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.2f;

        ParticleSystemRenderer psr = go.GetComponent<ParticleSystemRenderer>();
        psr.renderMode = ParticleSystemRenderMode.Billboard;
        psr.material = CreateParticleMaterial();

        Destroy(go, 1.5f);
    }

    /* Material aditivo para o estouro (URP com fallback para shaders legados). */
    private Material CreateParticleMaterial()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader == null)
            shader = Shader.Find("Legacy Shaders/Particles/Additive");
        if (shader == null)
            shader = Shader.Find("Sprites/Default");

        Material mat = new Material(shader != null ? shader : Shader.Find("Unlit/Color"));
        Texture2D tex = Resources.GetBuiltinResource<Texture2D>("Default-Particle.psd");
        if (tex != null)
            mat.mainTexture = tex;
        return mat;
    }

    private void FinishRemoval()
    {
        if (destroyOnComplete)
            Destroy(gameObject);
        else
            gameObject.SetActive(false);
    }

    /* Hook opcional chamado no Start. */
    protected virtual void OnStart() { }
}