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

    [Tooltip("Ajusta os cliques necessários ao número de garrafas: uma some por clique e a última fecha o foco.")]
    [SerializeField] private bool cliquesIgualAoNumeroDeGarrafas = true;

    [Tooltip("Qual garrafa some primeiro.")]
    [SerializeField] private OrdemDeSumico ordem = OrdemDeSumico.MaisPertoPrimeiro;

    [Header("Animação de sumiço")]
    [SerializeField] private float duracaoSumico = 0.32f;

    [Tooltip("Altura do pulinho que a garrafa dá ao ser recolhida (unidades).")]
    [SerializeField] private float alturaDoPulinho = 0.22f;

    [Tooltip("Quanto a garrafa gira enquanto some (graus).")]
    [SerializeField] private float giroAoSumir = 240f;

    private readonly List<Renderer> restantes = new List<Renderer>();

    /* Quantas garrafas ainda estão na cena. */
    public int GarrafasRestantes => restantes.Count;

    protected override void OnStart()
    {
        restantes.Clear();

        if (garrafas == null || garrafas.Count == 0)
            garrafas = new List<Renderer>(BuscarGarrafas());

        foreach (Renderer r in garrafas)
            if (r != null)
                restantes.Add(r);

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

    protected override void OnComplete()
    {
        // Não chama base: as garrafas já sumiram uma a uma, não sobrou
        // nada para encolher. O evento OnCompleted já foi disparado pela
        // base antes daqui (HUD e rede social são avisadas normalmente).
    }

    /* ---------------- Interno ---------------- */

    private Renderer[] BuscarGarrafas()
    {
        List<Renderer> achadas = new List<Renderer>();
        foreach (MeshRenderer r in GetComponentsInChildren<MeshRenderer>(true))
        {
            string nome = r.gameObject.name.TrimStart().ToLowerInvariant();
            if (nome.StartsWith("agua") || nome.StartsWith("água"))
                continue;   // água é filha da garrafa: some junto com ela
            achadas.Add(r);
        }
        return achadas.ToArray();
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
