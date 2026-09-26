using System.Collections;
using UnityEngine;

/* ============================================================
 *  BromeliaInteractable.cs
 *  ---------------------------------------------------------------------
 *  Foco do tipo BROMÉLIA — herda Interactable.
 *
 *  Comportamento ("bromélia com água -> interagir -> bromélia seca"):
 *  - A planta começa com água parada no copo central e em pocinhas
 *    entre as folhas.
 *  - A cada clique algumas pocinhas secam (encolhem e somem), das de
 *    fora para a do centro.
 *  - No último clique não sobra água nenhuma. A planta CONTINUA na cena:
 *    o certo é esvaziar a bromélia, não arrancá-la.
 *
 *  O modelo vem do Blender (Ferramentas/Blender/gerar_focos.py) e o
 *  prefab é gerado pelo menu  Dengue > Criar Bromélia  (FocosBlenderBuilder.cs).
 * ============================================================ */

public class BromeliaInteractable : Interactable
{
    [Header("Bromélia")]
    [Tooltip("Poças d'água, na ordem em que secam (a última deve ser a do centro).")]
    [SerializeField] private GameObject[] pocas = new GameObject[0];

    [Tooltip("Duração da animação de cada poça secando.")]
    [SerializeField] private float duracaoSecar = 0.3f;

    [Tooltip("Se marcado, após secar segue o fluxo padrão (encolhe e some).")]
    [SerializeField] private bool removeOnComplete = false;

    private int secas;

    protected override void OnStart()
    {
        secas = 0;
        foreach (GameObject p in pocas)
            if (p != null) p.SetActive(true);
    }

    protected override void OnInteract()
    {
        // Distribui as poças pelos cliques; no último clique seca o que sobrou.
        int alvo = RequiredClicks <= 0
            ? pocas.Length
            : Mathf.CeilToInt((float)pocas.Length * CurrentClicks / RequiredClicks);

        for (; secas < Mathf.Min(alvo, pocas.Length); secas++)
            if (pocas[secas] != null && pocas[secas].activeSelf)
                StartCoroutine(Secar(pocas[secas]));

        base.OnInteract();
    }

    protected override void OnComplete()
    {
        if (removeOnComplete)
            base.OnComplete();
        // Caso contrário, a bromélia fica na cena, seca.
    }

    private IEnumerator Secar(GameObject poca)
    {
        Transform t = poca.transform;
        Vector3 escala = t.localScale;
        for (float e = 0f; e < duracaoSecar; e += Time.deltaTime)
        {
            float k = e / duracaoSecar;
            t.localScale = Vector3.Lerp(escala, new Vector3(0f, escala.y, 0f), k * k);
            yield return null;
        }
        poca.SetActive(false);
        t.localScale = escala;
    }
}
