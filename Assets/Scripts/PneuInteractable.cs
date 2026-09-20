using UnityEngine;

/* ============================================================
 *  PneuInteractable.cs
 *  ---------------------------------------------------------------------
 *  Foco do tipo PNEU — herda a base Interactable e adiciona o
 *  comportamento específico: depois de alguns cliques, a ÁGUA é
 *  retirada (o filho "agua do pneu" é desativado), e em seguida
 *  o pneu segue o fluxo padrão de encolher até sumir.
 * ============================================================ */

public class PneuInteractable : Interactable
{
    [Header("Pneu")]
    [Tooltip("Objeto filho que representa a água parada dentro do pneu.")]
    [SerializeField] private GameObject water;

    [Tooltip("Após quantos cliques a água é removida.")]
    [SerializeField] private int clicksToDrainWater = 3;

    protected override void OnInteract()
    {
        // Ao atingir o número de cliques configurado, remove a água.
        if (water != null && CurrentClicks >= clicksToDrainWater && water.activeSelf)
            water.SetActive(false);

        base.OnInteract();
    }
}