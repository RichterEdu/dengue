using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* ============================================================
 *  Interactable.cs
 *  ---------------------------------------------------------------------
 *  Classe BASE genérica para objetos interativos do jogo (focos de
 *  criadouro do Aedes aegypti: pneus, caixas d'água, vasos, etc).
 *
 *  - O player mira via Interactor.cs (raycast do centro da tela).
 *  - Cada clique (passo) dispara OnInteract() (virtual, reutilizável) e
 *    um "punch" de escala (squash & stretch) como feedback imediato.
 *  - Ao atingir requiredClicks, conclui: dispara OnCompleted/OnComplete().
 *  - Conclusão padrão: POP (expande), FLASH branco de emissão,
 *    estouro de partículas, e encolhe com easing + giro + pulinho.
 *
 *  Kit do agente (Parte A): no "segurar para agir", cada passo vale
 *  SegundosPorPasso (segundosParaConcluir dividido entre os cliques) e é
 *  um AvancarCom(kit):
 *  - Verificar(kit) confere SEM avançar nem gastar nada, nesta ordem:
 *    foco que não aceita mais passos (Bloqueado), pneu nas mãos
 *    (MaosOcupadas), ferramenta errada, sem carga e a regra extra de
 *    cada foco (VerificarExtra). Ferramenta errada vem antes de sem
 *    carga porque é a mensagem que ensina.
 *  - Se deu Avancou, o passo é igual a um clique antigo; a carga da
 *    ferramentaNecessaria só é gasta quando o foco conclui.
 *  - IsComplete é uma FLAG, setada só na conclusão real (Concluir), e
 *    não mais "cliques >= total". Com ConcluiSozinho = false o último
 *    passo só termina a ação (AoTerminarAcao) e a própria classe conclui
 *    depois: no pneu, o último passo esvazia (ele fica no lugar) e pegar
 *    é o passo seguinte; ele só conta como eliminado quando é entregue
 *    na caçamba. Esse foco não encolhe por clique
 *    (scaleReductionPerClick): é carregado do tamanho dele.
 *  - Interact() continua sendo o caminho antigo: um passo sem kit.
 *
 *  Atalho de esvaziar (Parte B): em focos com podeEsvaziar que pedem
 *  outra ferramenta (vaso, garrafas), segurar com as Mãos antes de
 *  começar a ação definitiva (currentClicks == 0) tira a água em
 *  passosParaEsvaziar passos (segundosParaEsvaziar no total, ver
 *  SegundosPorPassoCom).
 *  - No Verificar o desvio vem depois do MaosOcupadas e antes da
 *    ferramenta errada: esvaziado, dá JaVazio; senão, Avancou. Não passa
 *    pelo VerificarExtra (garrafas com o saco cheio ainda esvaziam).
 *  - Cada passo de esvaziar chama AoAvancarEsvaziando e dá o punch (menos
 *    o último), sem mexer em currentClicks e sem OnClicado (o som do
 *    OnClicado é o da ação definitiva). O último chama Esvaziar() e dá
 *    Esvaziou: o foco fica no lugar, sem água (Esvaziado), não zumbe e
 *    NÃO conclui (nada de OnCompleted). A ferramenta certa ainda resolve
 *    de vez, e Reencher() (a chuva) devolve a água.
 *  - Esvaziado não é serializado: todo foco nasce cheio.
 *
 *  Para focos com comportamento próprio, herde esta classe e sobrescreva
 *  OnInteract()/OnComplete() (ver PneuInteractable.cs como exemplo) e,
 *  com o kit, AceitaFerramenta/VerificarExtra/AoAvancar.
 * ============================================================ */

public class Interactable : MonoBehaviour
{
    /* Disparado quando o foco é concluído (referência do próprio objeto). */
    public event System.Action<Interactable> OnCompleted;

    /* Disparado a cada clique aceito, inclusive o último (sons dos focos). */
    public event System.Action<Interactable> OnClicado;

    /* Disparado quando o atalho de esvaziar termina (a água saiu, o foco não concluiu). */
    public event System.Action<Interactable> OnEsvaziado;

    /* Disparado quando um foco esvaziado volta a ter água (Reencher, a chuva). */
    public event System.Action<Interactable> OnReenchido;

    [Header("Interação")]
    [Tooltip("Quantos cliques o jogador precisa dar para eliminar este foco.")]
    [SerializeField] protected int requiredClicks = 5;

    [Tooltip("Distância máxima (em unidades) para o jogador interagir.")]
    [SerializeField] protected float interactionRange = 3f;

    [Header("Kit do agente")]
    [Tooltip("Ferramenta do kit que elimina este foco. Com outra selecionada, o passo é recusado.")]
    [SerializeField] protected Ferramenta ferramentaNecessaria = Ferramenta.Maos;

    [Tooltip("Segundos segurando o botão para eliminar o foco, divididos igualmente entre os cliques.")]
    [SerializeField] protected float segundosParaConcluir = 2f;

    [Header("Atalho de esvaziar")]
    [Tooltip("Segurar com as Mãos tira a água deste foco (vaso, garrafas) sem eliminá-lo. Só vale em foco que pede outra ferramenta e antes de começar a ação definitiva.")]
    [SerializeField] protected bool podeEsvaziar = false;

    [Tooltip("Quantos passos (segurando com as Mãos) para esvaziar o foco.")]
    [SerializeField] protected int passosParaEsvaziar = 3;

    [Tooltip("Segundos segurando com as Mãos para esvaziar, divididos igualmente entre os passos.")]
    [SerializeField] protected float segundosParaEsvaziar = 1.2f;

    [Header("Feedback por clique")]
    [Tooltip("Fração da escala usada no 'punch' a cada clique (0 = sem punch).")]
    [SerializeField] protected float punchScale = 0.1f;

    [Tooltip("Duração do 'punch' (expande e volta), em segundos.")]
    [SerializeField] protected float punchDuration = 0.14f;

    [Header("Feedback de conclusão")]
    [Tooltip("Fração da escala inicial removida POR CLIQUE (0 = não encolhe a cada clique). Não vale para foco carregado até a caçamba (pneu).")]
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

    // Um material para todos os estouros (criado no primeiro uso).
    private static Material materialDoEstouro;

    // Setada só pelo Concluir(). Não serializada: todo foco nasce não concluído.
    private bool concluido;

    // Passos de esvaziar já dados (zera ao esvaziar ou ao começar a ação definitiva).
    private int passosEsvaziando;

    private Coroutine punchRoutine;

    /* --- Propriedades públicas (para feedback externo) --- */
    public int RequiredClicks => requiredClicks;
    public int CurrentClicks => currentClicks;
    public Ferramenta FerramentaNecessaria => ferramentaNecessaria;

    /* O foco tem o atalho de esvaziar com as Mãos. */
    public bool PodeEsvaziar => podeEsvaziar;
    public int PassosParaEsvaziar => passosParaEsvaziar;

    /* Passos de esvaziar dados até agora (barra da HUD). */
    public int PassosEsvaziando => passosEsvaziando;

    /* A água saiu pelo atalho (ou, no pneu, no último passo). Não é
     * serializado: todo foco nasce cheio. */
    public bool Esvaziado { get; private set; }

    /* Tempo segurando que vale um passo (= um clique antigo). */
    public virtual float SegundosPorPasso => segundosParaConcluir / Mathf.Max(1, requiredClicks);

    /* Tempo de um passo com este kit: no caminho de esvaziar, o do atalho;
     * senão, o SegundosPorPasso de sempre. */
    public virtual float SegundosPorPassoCom(KitDoAgente kit)
    {
        if (NoCaminhoDeEsvaziar(kit))
            return segundosParaEsvaziar / Mathf.Max(1, passosParaEsvaziar);
        return SegundosPorPasso;
    }

    /* Um passo com este kit seria (ou é) do atalho de esvaziar: Mãos num foco
     * com o atalho, que pede outra ferramenta e cuja ação definitiva ainda
     * não começou. Usado também pelo Interactor (barra) e pela HUD. */
    public bool NoCaminhoDeEsvaziar(KitDoAgente kit)
    {
        return kit != null
            && kit.Selecionada == Ferramenta.Maos
            && podeEsvaziar
            && ferramentaNecessaria != Ferramenta.Maos
            && currentClicks == 0;
    }

    /* Skill check (ralo): este foco troca o "segurar" por checks, um por
     * passo. O Interactor conduz; o erro e o cancelamento ficam no foco. */
    public virtual bool UsaSkillCheck => false;
    public virtual float SegundosPorVoltaDoCheck => 1.2f;
    public virtual void ErrarSkillCheck(KitDoAgente kit) { }
    public virtual void CancelarSkillCheck() { }

    /* Esvaziado e ainda não eliminado: a chuva enche de novo. */
    public virtual bool PodeReencher => Esvaziado && !IsComplete;

    /* Ainda aceita passos (o Interactor só mira focos assim). */
    public virtual bool CanInteract => !concluido && currentClicks < requiredClicks;

    /* Eliminado de verdade: é a flag do Concluir(), não a conta de cliques
     * (o pneu termina a ação e só conclui ao ser entregue na caçamba). */
    public bool IsComplete => concluido;

    /* Se o foco faz o zumbido do mosquito (SomDosFocos). Esvaziado, não. */
    public virtual bool Zumbe => CanInteract && !Esvaziado;

    void Start()
    {
        currentClicks = 0;
        initialScale = transform.localScale;
        OnStart();
    }

    /* Caminho antigo (clique sem kit): um passo, sem conferir ferramenta nem carga. */
    public void Interact()
    {
        if (!CanInteract)
            return;

        DarPasso(null);
    }

    /* A ferramenta serve para este foco? Por padrão, só a ferramentaNecessaria. */
    public virtual bool AceitaFerramenta(Ferramenta f)
    {
        return f == ferramentaNecessaria;
    }

    /* Confere se um passo com este kit contaria, sem avançar nem gastar nada.
     * Sem kit, só diz se o foco ainda aceita passos (Avancou ou Bloqueado). */
    public virtual ResultadoDoPasso Verificar(KitDoAgente kit)
    {
        if (!CanInteract)
            return ResultadoDoPasso.Bloqueado;
        if (kit == null)
            return ResultadoDoPasso.Avancou;

        if (kit.PneuCarregado != null)
            return ResultadoDoPasso.MaosOcupadas;
        // Atalho de esvaziar: as Mãos, que aqui seriam a ferramenta errada.
        // Não passa pelo VerificarExtra (o saco cheio não impede esvaziar).
        if (NoCaminhoDeEsvaziar(kit))
            return Esvaziado ? ResultadoDoPasso.JaVazio : ResultadoDoPasso.Avancou;
        // Antes do sem carga: com a ferramenta errada E vazia, o aviso de
        // ferramenta errada é o que ensina.
        if (!AceitaFerramenta(kit.Selecionada))
            return ResultadoDoPasso.FerramentaErrada;
        if (!kit.Estoque.TemCarga(kit.Selecionada))
            return ResultadoDoPasso.SemCarga;

        return VerificarExtra(kit);
    }

    /* Regra a mais de cada foco (ex.: garrafas com o saco cheio). Só é
     * chamada com kit, depois de ferramenta e carga aprovadas. */
    protected virtual ResultadoDoPasso VerificarExtra(KitDoAgente kit)
    {
        return ResultadoDoPasso.Avancou;
    }

    /* Um passo de "segurar para agir" com o kit. Recusado, nada muda. A
     * carga só é gasta quando o foco conclui (no meio da ação, nada). */
    public virtual ResultadoDoPasso AvancarCom(KitDoAgente kit)
    {
        ResultadoDoPasso resultado = Verificar(kit);
        if (resultado != ResultadoDoPasso.Avancou)
            return resultado;

        if (NoCaminhoDeEsvaziar(kit))
            return DarPassoDeEsvaziar();

        DarPasso(kit);

        if (!IsComplete)
            return ResultadoDoPasso.Avancou;

        if (kit != null)
            kit.Consumir(FerramentaNecessaria);
        return ResultadoDoPasso.Concluiu;
    }

    /* Um passo (= um clique antigo). kit é null no caminho Interact(). */
    private void DarPasso(KitDoAgente kit)
    {
        currentClicks++;

        // A ação definitiva começou: um esvaziar pela metade fica para trás.
        passosEsvaziando = 0;

        // Último passo pela conta de cliques (o IsComplete agora é a flag
        // do Concluir, que ainda não foi setada aqui).
        bool ultimo = currentClicks >= requiredClicks;

        // Redução de escala a cada clique (optativo). Foco que não conclui
        // sozinho vai ser carregado (o pneu) e não encolhe: chega aos braços,
        // ao chão e à caçamba com o tamanho dele.
        if (scaleReductionPerClick > 0f && !ultimo && ConcluiSozinho)
        {
            float factor = Mathf.Max(0f, 1f - scaleReductionPerClick * currentClicks);
            transform.localScale = initialScale * factor;
        }

        OnInteract();
        AoAvancar(kit);
        OnClicado?.Invoke(this);

        if (!ultimo)
            PlayPunch();
        else if (ConcluiSozinho)
            Concluir();
        else
        {
            // Para o punch antes: o fim dele reescreveria a escala do foco,
            // que logo depois pode ir para outro pai (no pneu, o último
            // passo esvazia; pegar, para os braços, é o passo seguinte).
            PararPunch();
            AoTerminarAcao(kit);
        }
    }

    /* Um passo do atalho de esvaziar (o Verificar já aprovou). Não mexe em
     * currentClicks nem dispara OnClicado: o som do OnClicado é o da ação
     * definitiva (areia no vaso, garrafa recolhida). O feedback é o punch,
     * menos no último passo, que esvazia. */
    private ResultadoDoPasso DarPassoDeEsvaziar()
    {
        passosEsvaziando++;
        AoAvancarEsvaziando(passosEsvaziando);

        if (passosEsvaziando < passosParaEsvaziar)
        {
            PlayPunch();
            return ResultadoDoPasso.Avancou;
        }

        Esvaziar();
        return ResultadoDoPasso.Esvaziou;
    }

    /* A água sai: o foco fica no lugar, Esvaziado, sem concluir. */
    protected void Esvaziar()
    {
        Esvaziado = true;
        passosEsvaziando = 0;
        AoEsvaziar();
        OnEsvaziado?.Invoke(this);
    }

    /* A água volta (a chuva). Só num foco esvaziado e não eliminado. */
    public void Reencher()
    {
        if (!PodeReencher)
            return;

        Esvaziado = false;
        AoReencher();
        OnReenchido?.Invoke(this);
    }

    /* Elimina o foco de verdade. Só age uma vez: seta a flag, para o punch e
     * dispara OnCompleted (HUD, contagem, rede social) e depois OnComplete(). */
    protected void Concluir()
    {
        if (concluido)
            return;

        concluido = true;

        PararPunch();
        OnCompleted?.Invoke(this);
        OnComplete();
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

    /* Interrompe o punch em andamento, se houver. */
    private void PararPunch()
    {
        if (punchRoutine == null)
            return;

        StopCoroutine(punchRoutine);
        punchRoutine = null;
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

    /* Hook de cada passo, logo depois do OnInteract(), com o kit (null no Interact()). */
    protected virtual void AoAvancar(KitDoAgente kit) { }

    /* false = o último passo só termina a ação (AoTerminarAcao) e a própria
     * classe chama Concluir() depois (ex.: pneu entregue na caçamba). */
    protected virtual bool ConcluiSozinho => true;

    /* Chamado no último passo quando ConcluiSozinho é false (kit pode ser null). */
    protected virtual void AoTerminarAcao(KitDoAgente kit) { }

    /* Hook de cada passo de esvaziar (1..passosParaEsvaziar), inclusive o último. */
    protected virtual void AoAvancarEsvaziando(int passo) { }

    /* Hook do Esvaziar(), antes do OnEsvaziado: esconder a água. */
    protected virtual void AoEsvaziar() { }

    /* Hook do Reencher(), antes do OnReenchido: devolver a água. */
    protected virtual void AoReencher() { }

    /* Hook chamado na conclusão (Concluir), logo depois do OnCompleted. */
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
        EstourarParticulas(transform.position + Vector3.up * 0.3f);
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

        // Sem as partículas (enxame, estouro): ver EmissionHighlightUtil.Destacavel.
        foreach (Renderer rend in GetComponentsInChildren<Renderer>())
        {
            if (!EmissionHighlightUtil.Destacavel(rend))
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

    /* Cria um estouro de partículas em runtime (sem assets externos). Usado
     * na conclusão e pelas classes filhas (o pneu, ao cair na vaga). */
    protected void EstourarParticulas(Vector3 pos)
    {
        GameObject go = new GameObject("PopFeedback");
        go.transform.position = pos;

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
        psr.sharedMaterial = MaterialDoEstouro();

        Destroy(go, 1.5f);
    }

    /* O material de todos os estouros: Sprites/Default (transparente e
     * sempre incluído na build), com a bolinha gerada em código do enxame
     * (a textura embutida "Default-Particle.psd" não é achada nesta versão
     * do Unity). A cor vem das partículas. */
    internal static Material MaterialDoEstouro()
    {
        if (materialDoEstouro != null)
            return materialDoEstouro;

        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null)
            shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");

        materialDoEstouro = new Material(shader)
        {
            name = "Estouro (runtime)",
            hideFlags = HideFlags.DontSave,
            mainTexture = EnxameDeMosquitos.Bolinha()
        };
        return materialDoEstouro;
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