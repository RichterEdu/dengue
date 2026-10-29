using System.Collections.Generic;
using UnityEngine;

/* ============================================================
 *  PontoDeReabastecimento.cs
 *  ---------------------------------------------------------------------
 *  Ponto de uso que enche de novo as cargas do kit (Parte A). A lista
 *  'recarrega' diz o que este ponto oferece:
 *
 *    - picape da UPA: areia, tela e água sanitária (o padrão);
 *    - monte de areia do terreno baldio: só a areia.
 *
 *  Recusa, nesta ordem: com o pneu nas mãos ("Largue o pneu antes.") e
 *  com tudo o que ele oferece já cheio ("Seu kit já está cheio."). Usar
 *  passa pelo KitDoAgente.Reabastecer, que só dispara o OnReabasteceu
 *  (som, aviso e post) quando alguma carga encheu de verdade, levando
 *  este ponto como origem.
 *
 *  ehPicapeDaUpa marca a picape: o post "passou na picape da UPA" da
 *  HistoriaRedeSocial só sai no primeiro reabastecimento nela.
 *
 *  Os nomes 'recarrega' e 'ehPicapeDaUpa' são lidos/gravados por
 *  SerializedObject pelo builder da cena ('recarrega' também pelo
 *  simulador de balanceamento): não renomear.
 * ============================================================ */

public class PontoDeReabastecimento : PontoDeUso
{
    [Header("Reabastecimento")]
    [Tooltip("Ferramentas que este ponto enche até o máximo do kit (as não consumíveis são ignoradas).")]
    [SerializeField] private List<Ferramenta> recarrega = new List<Ferramenta>
    {
        Ferramenta.Areia,
        Ferramenta.Tela,
        Ferramenta.AguaSanitaria
    };

    [Tooltip("Marcado só na picape da UPA: o primeiro reabastecimento nela vira post na rede social (o monte de areia fica desmarcado).")]
    [SerializeField] private bool ehPicapeDaUpa = false;

    /* É a picape da UPA (o post do primeiro reabastecimento só sai nela). */
    public bool EhPicapeDaUpa => ehPicapeDaUpa;

    public override bool PodeUsar(KitDoAgente kit, out string motivo)
    {
        motivo = string.Empty;

        // Sem kit não há o que encher: recusa sem aviso.
        if (kit == null)
            return false;

        if (kit.PneuCarregado != null)
        {
            motivo = "Largue o pneu antes.";
            return false;
        }
        if (kit.Estoque.Cheio(recarrega))
        {
            motivo = "Seu kit já está cheio.";
            return false;
        }
        return true;
    }

    public override void Usar(KitDoAgente kit)
    {
        if (kit == null)
            return;

        kit.Reabastecer(recarrega, this);
    }
}
