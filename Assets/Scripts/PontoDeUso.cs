using UnityEngine;

/* ============================================================
 *  PontoDeUso.cs
 *  ---------------------------------------------------------------------
 *  Base abstrata das coisas que o agente USA no bairro (Parte A), mas
 *  que não são criadouro: a picape da UPA e o monte de areia
 *  (PontoDeReabastecimento.cs) e a caçamba (Cacamba.cs).
 *
 *  NÃO herda Interactable de propósito: FluxoDeJogo, DengueHUD,
 *  FocoListaHUD e RedeSocial varrem Interactable como foco, e um ponto
 *  de uso entraria na contagem, na lista e nos posts como criadouro.
 *
 *  O Interactor mira o ponto do mesmo jeito que um foco:
 *    - a mira mostra o TextoAoMirar;
 *    - no aperto, PodeUsar(kit, out motivo) confere sem mexer em nada;
 *      recusado, o motivo vai para a HUD;
 *    - segurando SegundosParaUsar, chama Usar(kit).
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

    /* Confere se dá para usar agora, sem mexer em nada. Recusado, 'motivo'
     * diz o porquê (texto da HUD). Nunca é null: vazio quando não há o que
     * avisar (liberado, ou sem kit, que é recusa sem aviso). */
    public abstract bool PodeUsar(KitDoAgente kit, out string motivo);

    /* Usa o ponto (chamado depois de segurar SegundosParaUsar). Sem kit, nada. */
    public abstract void Usar(KitDoAgente kit);
}
