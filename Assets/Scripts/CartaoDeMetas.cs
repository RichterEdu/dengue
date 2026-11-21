using TMPro;
using UnityEngine;
using UnityEngine.UI;

/* ============================================================
 *  CartaoDeMetas.cs
 *  ---------------------------------------------------------------------
 *  O cartão "METAS DA PARTIDA" da pausa. Atualizar() lê o placar quando a
 *  pausa abre (o jogo fica parado enquanto ela está aberta).
 *
 *  - Durante a partida: meta viva = estrela cheia e o andamento
 *    (DetalheAoVivo); meta perdida = estrela vazia e o detalhe em
 *    corPerdida.
 *  - Com a partida encerrada, usa o que o placar fechou (Resultado e
 *    DetalheFinal).
 *  - Abaixo: RECORDE, pontos e estrelas. Sem recorde, a linha não
 *    aparece.
 *  - Referências nulas e arrays curtos são ignorados.
 * ============================================================ */

public class CartaoDeMetas : MonoBehaviour
{
    [Tooltip("Vazio = o PlacarDaPartida da cena.")]
    [SerializeField] private PlacarDaPartida placar;

    [Header("Metas (3, na ordem de AvaliacaoDaPartida.Metas)")]
    [SerializeField] private Image[] estrelasDasMetas;
    [SerializeField] private TextMeshProUGUI[] nomesDasMetas;
    [SerializeField] private TextMeshProUGUI[] detalhesDasMetas;

    [Header("Estrelas")]
    [SerializeField] private Sprite estrelaCheia;
    [SerializeField] private Sprite estrelaVazia;

    [Header("Cores")]
    [SerializeField] private Color corCheia = new Color(0.25f, 1f, 0.8f, 1f);
    [SerializeField] private Color corPerdida = new Color(1f, 0.4f, 0.4f, 1f);
    [SerializeField] private Color corTextoFraco = new Color(1f, 1f, 1f, 0.55f);
    [Tooltip("Estrelas vazias da linha de recorde (e das metas de uma partida perdida, que não têm meta perdida marcada).")]
    [SerializeField] private Color corRecordeVazia = new Color(1f, 1f, 1f, 0.28f);

    [Header("Recorde")]
    [SerializeField] private GameObject linhaDoRecorde;
    [SerializeField] private TextMeshProUGUI textoRecorde;
    [SerializeField] private Image[] estrelasDoRecorde;

    public void Atualizar()
    {
        if (placar == null) placar = FindAnyObjectByType<PlacarDaPartida>();
        if (placar == null)
            return;

        AtualizarMetas();
        AtualizarRecorde();
    }

    private void AtualizarMetas()
    {
        DadosDaPartida d = placar.Dados;
        for (int i = 0; i < AvaliacaoDaPartida.Metas.Length; i++)
        {
            MetaDaPartida meta = AvaliacaoDaPartida.Metas[i];
            bool viva = placar.Fechado ? placar.Resultado.Cumpriu(meta) : placar.MetaViva(meta);
            bool perdida = placar.Fechado ? d.Venceu && !viva : !viva;

            TextMeshProUGUI nome = Item(nomesDasMetas, i);
            if (nome != null)
                nome.text = AvaliacaoDaPartida.Nome(meta);

            TextMeshProUGUI detalhe = Item(detalhesDasMetas, i);
            if (detalhe != null)
            {
                detalhe.text = placar.Fechado
                    ? AvaliacaoDaPartida.DetalheFinal(meta, d, placar.Regras)
                    : AvaliacaoDaPartida.DetalheAoVivo(meta, d, placar.Regras);
                detalhe.color = perdida ? corPerdida : corTextoFraco;
            }

            Image estrela = Item(estrelasDasMetas, i);
            if (estrela != null)
            {
                estrela.sprite = viva ? estrelaCheia : estrelaVazia;
                estrela.color = viva ? corCheia : perdida ? corPerdida : corRecordeVazia;
            }
        }
    }

    private void AtualizarRecorde()
    {
        RecordeDoJogo recorde = placar.Recorde;
        bool tem = recorde.Pontos > 0;

        if (linhaDoRecorde != null)
            linhaDoRecorde.SetActive(tem);
        if (!tem)
            return;

        if (textoRecorde != null)
            textoRecorde.text = AvaliacaoDaPartida.Formatar(recorde.Pontos);
        TelaDaNota.PintarEstrelas(estrelasDoRecorde, recorde.Estrelas, estrelaCheia, estrelaVazia, corCheia, corRecordeVazia);
    }

    private static T Item<T>(T[] itens, int i) where T : class =>
        itens != null && i >= 0 && i < itens.Length ? itens[i] : null;
}
