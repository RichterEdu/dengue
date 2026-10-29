using UnityEngine;

/* ============================================================
 *  Cacamba.cs
 *  ---------------------------------------------------------------------
 *  A caçamba de entulho do terreno baldio (Parte A): ponto de uso onde
 *  o agente se livra do que recolheu.
 *
 *    - com um pneu nas mãos, entrega o pneu (KitDoAgente.EntregarPneu):
 *      ele é solto alturaDescarte acima da caçamba (PontoDeDescarte) e só
 *      aí o foco conta como eliminado;
 *    - sem pneu, esvazia o saco de lixo (KitDoAgente.EsvaziarSaco).
 *
 *  O pneu vem primeiro: carregando, o agente não tem como mexer no saco.
 *  Sem pneu e com o saco vazio, recusa com "Nada para jogar na caçamba.".
 *  Largar o pneu (botão direito) em cima da caçamba também é entrega
 *  (KitDoAgente.LargarPneu).
 * ============================================================ */

public class Cacamba : PontoDeUso
{
    [Header("Caçamba")]
    [Tooltip("Altura (em unidades) acima do pivô da caçamba onde o pneu entregue é solto.")]
    [SerializeField] private float alturaDescarte = 1.2f;

    /* Onde o pneu entregue é solto: alturaDescarte acima do pivô (no chão). */
    public Vector3 PontoDeDescarte => transform.position + Vector3.up * alturaDescarte;

    public override bool PodeUsar(KitDoAgente kit, out string motivo)
    {
        motivo = string.Empty;

        // Sem kit não há o que jogar: recusa sem aviso.
        if (kit == null)
            return false;

        if (kit.PneuCarregado != null || kit.Estoque.GarrafasNoSaco > 0)
            return true;

        motivo = "Nada para jogar na caçamba.";
        return false;
    }

    public override void Usar(KitDoAgente kit)
    {
        if (kit == null)
            return;

        if (kit.PneuCarregado != null)
            kit.EntregarPneu(PontoDeDescarte);
        else
            kit.EsvaziarSaco();
    }
}
