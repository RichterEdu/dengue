using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* ============================================================
 *  GarrafaInteractable.cs
 *  ---------------------------------------------------------------------
 *  Foco do tipo GARRAFA / RECIPIENTE — herda a base Interactable.
 *
 *  Comportamento (GameDoc: "garrafa com água -> interagir -> recipiente
 *  retirado"):
 *  - O foco é o GRUPO de garrafas (a raiz e as filhas penduradas nela).
 *  - A CADA CLIQUE uma garrafa some: dá um pulinho, gira e encolhe até
 *    desaparecer, como se o agente tivesse recolhido aquela.
 *  - Por padrão o número de cliques é ajustado ao número de garrafas
 *    do grupo, então o último clique tira a última garrafa e o foco é
 *    eliminado nesse exato momento.
 *  - Como não sobra nada para encolher, a conclusão NÃO roda o encolher
 *    padrão do Interactable (o grupo já sumiu garrafa por garrafa).
 *
 *  A ordem padrão é "a mais perto do jogador primeiro": você recolhe o
 *  que está olhando, e não uma garrafa qualquer do outro lado do monte.
 *
 *  Se alguma garrafa tiver água como objeto FILHO dela, a água some
 *  junto com a garrafa, sem precisar amarrar nada.
 *
 *  Kit do agente (Parte A): as garrafas vão para o saco de lixo.
 *  - Cada passo leva segundosPorGarrafa segurando (seja qual for o
 *    tamanho do grupo) e guarda a garrafa recolhida no saco
 *    (KitDoAgente.GuardarGarrafa).
 *  - Com o saco cheio o passo é recusado (SacoCheio) e nada muda: é
 *    preciso esvaziar o saco numa lixeira para continuar o grupo.
 *  - ContarGarrafas() diz quantas garrafas (= passos) o grupo tem,
 *    também fora do Play (simulador de balanceamento).
 *
 *  Atalho de esvaziar (Parte B, podeEsvaziar no Interactable): com as
 *  Mãos, a água das garrafas que restam (os filhos "agua"/"água" de cada
 *  garrafa) some, e as garrafas ficam no lugar; reenchido (a chuva), essa
 *  água volta (mas não com o recolhimento já começado). Garrafa sem
 *  filho de água não muda nada visualmente.
 *
 *  Para pendurar este componente num grupo já montado na cena, use o
 *  menu  Dengue > Transformar Selecao em Foco (Garrafas)
 *  (Assets/Editor/GarrafaFocoBuilder.cs), que também põe os colliders.
 * ============================================================ */

public class GarrafaInteractable : Interactable
{
    public enum OrdemDeSumico
    {
        MaisPertoPrimeiro,   // a mais perto da câmera
        Hierarquia,          // na ordem em que aparecem na lista
        Aleatoria
    }

    [Header("Garrafas")]
    [Tooltip("As garrafas do grupo. Se ficar vazio, pega sozinho todos os MeshRenderer filhos (menos os que começam com \"agua\").")]
    [SerializeField] private List<Renderer> garrafas = new List<Renderer>();

    [Tooltip("Ajusta os cliques necessários ao número de garrafas: uma some por clique (passo) e a última fecha o foco.")]
    [SerializeField] private bool cliquesIgualAoNumeroDeGarrafas = true;

    [Tooltip("Segundos segurando para recolher UMA garrafa para o saco. Substitui o segundosParaConcluir: o grupo leva isto vezes o número de garrafas.")]
    [SerializeField] private float segundosPorGarrafa = 0.4f;

    [Tooltip("Qual garrafa some primeiro.")]
    [SerializeField] private OrdemDeSumico ordem = OrdemDeSumico.MaisPertoPrimeiro;

    [Header("Animação de sumiço")]
    [Tooltip("Duração do pulinho + giro + encolher de cada garrafa (segundos).")]
    [SerializeField] private float duracaoSumico = 0.32f;

    [Tooltip("Altura do pulinho que a garrafa dá ao ser recolhida (unidades).")]
    [SerializeField] private float alturaDoPulinho = 0.22f;

    [Tooltip("Quanto a garrafa gira enquanto some (graus).")]
    [SerializeField] private float giroAoSumir = 240f;

    private readonly List<Renderer> restantes = new List<Renderer>();

    // A água que o esvaziar escondeu (só ela volta no reencher).
    private readonly List<GameObject> aguasEscondidas = new List<GameObject>();

    /* Quantas garrafas ainda estão na cena. */
    public int GarrafasRestantes => restantes.Count;

    /* Um passo = uma garrafa, então o tempo é por garrafa e não um total
     * dividido entre os cliques. */
    public override float SegundosPorPasso => segundosPorGarrafa;

    /* Com o recolhimento já começado, a chuva não reenche: as garrafas que
     * sobraram vão para o saco de qualquer jeito. */
    public override bool PodeReencher => base.PodeReencher && CurrentClicks == 0;

    /* Quantas garrafas o grupo tem: as mesmas que o OnStart junta (a lista
     * ou, vazia, as achadas nos filhos). Funciona fora do Play, sem o
     * OnStart: o simulador de balanceamento conta assim os passos. */
    public int ContarGarrafas()
    {
        return GarrafasDoGrupo().Count;
    }

    protected override void OnStart()
    {
        restantes.Clear();
        restantes.AddRange(GarrafasDoGrupo());

        // Uma garrafa por clique: o último clique tira a última e conclui o foco.
        if (cliquesIgualAoNumeroDeGarrafas && restantes.Count > 0)
            requiredClicks = restantes.Count;
    }

    protected override void OnInteract()
    {
        Renderer garrafa = Proxima();
        if (garrafa != null)
        {
            restantes.Remove(garrafa);
            StartCoroutine(SumirGarrafa(garrafa));
        }

        base.OnInteract();
    }

    /* Saco cheio: a garrafa não tem para onde ir até esvaziar na lixeira. */
    protected override ResultadoDoPasso VerificarExtra(KitDoAgente kit)
    {
        if (kit != null && kit.Estoque.SacoCheio)
            return ResultadoDoPasso.SacoCheio;

        return base.VerificarExtra(kit);
    }

    /* Cada passo com o kit põe a garrafa recolhida no saco. O VerificarExtra
     * já garantiu o espaço. Sem kit (Interact() antigo), não há saco. */
    protected override void AoAvancar(KitDoAgente kit)
    {
        if (kit != null)
            kit.GuardarGarrafa();
    }

    /* Esvaziado: a água das garrafas que restam some; elas ficam no lugar. */
    protected override void AoEsvaziar()
    {
        foreach (Renderer garrafa in restantes)
        {
            if (garrafa == null)
                continue;

            foreach (Transform filho in garrafa.transform)
            {
                if (!EhAgua(filho.gameObject) || !filho.gameObject.activeSelf)
                    continue;
                filho.gameObject.SetActive(false);
                aguasEscondidas.Add(filho.gameObject);
            }
        }
    }

    /* Reenchido (a chuva): volta a água das garrafas que ainda estão aí. */
    protected override void AoReencher()
    {
        foreach (GameObject agua in aguasEscondidas)
        {
            if (agua == null)
                continue;

            Renderer garrafa = agua.transform.parent != null ? agua.transform.parent.GetComponent<Renderer>() : null;
            if (garrafa != null && restantes.Contains(garrafa))
                agua.SetActive(true);
        }
        aguasEscondidas.Clear();
    }

    protected override void OnComplete()
    {
        // Não chama base: as garrafas já sumiram uma a uma, não sobrou
        // nada para encolher. O evento OnCompleted já foi disparado pela
        // base antes daqui (HUD e rede social são avisadas normalmente).
    }

    /* ---------------- Interno ---------------- */

    /* As garrafas da lista ou, com ela vazia, as achadas nos filhos; sem as nulas. */
    private List<Renderer> GarrafasDoGrupo()
    {
        IEnumerable<Renderer> fonte = garrafas != null && garrafas.Count > 0
            ? garrafas
            : (IEnumerable<Renderer>)BuscarGarrafas();

        List<Renderer> doGrupo = new List<Renderer>();
        foreach (Renderer r in fonte)
            if (r != null)
                doGrupo.Add(r);
        return doGrupo;
    }

    private Renderer[] BuscarGarrafas()
    {
        List<Renderer> achadas = new List<Renderer>();
        foreach (MeshRenderer r in GetComponentsInChildren<MeshRenderer>(true))
        {
            if (EhAgua(r.gameObject))
                continue;   // água é filha da garrafa: some junto com ela
            achadas.Add(r);
        }
        return achadas.ToArray();
    }

    /* A água de uma garrafa: objeto cujo nome começa com "agua"/"água". */
    private static bool EhAgua(GameObject go)
    {
        string nome = go.name.TrimStart().ToLowerInvariant();
        return nome.StartsWith("agua") || nome.StartsWith("água");
    }

    private Renderer Proxima()
    {
        for (int i = restantes.Count - 1; i >= 0; i--)
            if (restantes[i] == null)
                restantes.RemoveAt(i);

        if (restantes.Count == 0)
            return null;

        // A garrafa da RAIZ fica sempre por último: animar a raiz mexe
        // junto com todas as filhas (elas são penduradas nela), então ela
        // só pode sumir quando não sobrar mais nenhuma.
        List<Renderer> candidatas = new List<Renderer>();
        foreach (Renderer r in restantes)
            if (r.transform != transform)
                candidatas.Add(r);

        if (candidatas.Count == 0)
            candidatas = restantes;

        switch (ordem)
        {
            case OrdemDeSumico.Hierarquia:
                return candidatas[0];

            case OrdemDeSumico.Aleatoria:
                return candidatas[Random.Range(0, candidatas.Count)];

            default:
                Camera cam = Camera.main;
                if (cam == null)
                    return candidatas[0];

                Renderer melhor = candidatas[0];
                float menor = float.MaxValue;
                foreach (Renderer r in candidatas)
                {
                    float d = (r.bounds.center - cam.transform.position).sqrMagnitude;
                    if (d < menor)
                    {
                        menor = d;
                        melhor = r;
                    }
                }
                return melhor;
        }
    }

    /* Pulinho + giro + encolher, e a garrafa some. */
    private IEnumerator SumirGarrafa(Renderer garrafa)
    {
        Transform t = garrafa.transform;
        bool ehARaiz = t == transform;

        // Some do caminho do raycast na hora, para não dar para clicar nela de novo.
        Collider col = garrafa.GetComponent<Collider>();
        if (col != null)
            col.enabled = false;

        Vector3 escalaInicial = t.localScale;
        Vector3 posInicial = t.localPosition;
        Quaternion rotInicial = t.localRotation;
        Vector3 eixo = Vector3.up;

        for (float tempo = 0f; tempo < duracaoSumico; tempo += Time.deltaTime)
        {
            float k = Mathf.Clamp01(tempo / duracaoSumico);
            float suave = k * k;                                   // encolhe devagar e acelera

            t.localScale = escalaInicial * (1f - suave);
            t.localPosition = posInicial + Vector3.up * (Mathf.Sin(k * Mathf.PI) * alturaDoPulinho);
            t.localRotation = rotInicial * Quaternion.AngleAxis(giroAoSumir * k, eixo);
            yield return null;
        }

        // Devolve a pose (para ficar limpo se o foco for reaproveitado) e esconde.
        t.localScale = escalaInicial;
        t.localPosition = posInicial;
        t.localRotation = rotInicial;

        if (ehARaiz)
            garrafa.enabled = false;            // a raiz não pode ser desativada: leva o script junto
        else
            garrafa.gameObject.SetActive(false);
    }
}
