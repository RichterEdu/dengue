using UnityEngine;

/* ============================================================
 *  PontoDeUso.cs
 *  ---------------------------------------------------------------------
 *  Base abstrata das coisas que o agente USA no bairro (Parte A), mas
 *  que não são criadouro: a picape da UPA e o monte de areia
 *  (PontoDeReabastecimento.cs), a caçamba da picape (Cacamba.cs) e as
 *  lixeiras (Lixeira.cs, Parte B).
 *
 *  NÃO herda Interactable de propósito: FluxoDeJogo, DengueHUD,
 *  FocoListaHUD e RedeSocial varrem Interactable como foco, e um ponto
 *  de uso entraria na contagem, na lista e nos posts como criadouro.
 *
 *  O Interactor mira o ponto do mesmo jeito que um foco:
 *    - a mira mostra o TextoPara(kit): por padrão o TextoAoMirar, mas o
 *      ponto pode mudar o texto pelo que o agente tem nas mãos (Parte B:
 *      a picape com o pneu nos braços);
 *    - no aperto, PodeUsar(kit, out motivo) confere sem mexer em nada;
 *      recusado, o motivo vai para a HUD;
 *    - segurando SegundosParaUsarPara(kit), chama Usar(kit).
 * ============================================================ */

public abstract class PontoDeUso : MonoBehaviour
{
    [Header("Ponto de uso")]
    [Tooltip("Texto mostrado na mira quando o agente olha para este ponto.")]
    [TextArea]
    [SerializeField] protected string textoAoMirar;

    [Tooltip("Segundos segurando o botão para usar este ponto.")]
    [SerializeField] protected float segundosParaUsar = 1f;

    public string TextoAoMirar => textoAoMirar;
    public float SegundosParaUsar => segundosParaUsar;

    /* Texto da mira para este kit (a HUD mostra). Por padrão, o
     * TextoAoMirar; o ponto que muda de papel com o pneu nas mãos (a
     * caçamba, a picape) sobrescreve. */
    public virtual string TextoPara(KitDoAgente kit) => textoAoMirar;

    /* Tempo de segurar para este kit. Por padrão, o SegundosParaUsar; o
     * ponto que delega a outro (a picape com o pneu nas mãos) usa o dele. */
    public virtual float SegundosParaUsarPara(KitDoAgente kit) => segundosParaUsar;

    /* Confere se dá para usar agora, sem mexer em nada. Recusado, 'motivo'
     * diz o porquê (texto da HUD). Nunca é null: vazio quando não há o que
     * avisar (liberado, ou sem kit, que é recusa sem aviso). */
    public abstract bool PodeUsar(KitDoAgente kit, out string motivo);

    /* Usa o ponto (chamado depois de segurar SegundosParaUsarPara). Sem kit, nada. */
    public abstract void Usar(KitDoAgente kit);
}
