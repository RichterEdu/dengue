using TMPro;
using UnityEngine;
using UnityEngine.UI;

/* ============================================================
 *  TelaDaNota.cs
 *  ---------------------------------------------------------------------
 *  A parte da nota nas caixas de vitória e derrota: 3 estrelas, a nota,
 *  a linha do recorde, a coluna METAS e a coluna PONTOS. Não faz conta:
 *  todos os textos vêm de AvaliacaoDaPartida.
 *
 *  - As estrelas (sprites, porque a fonte do jogo não tem as estrelas)
 *    sempre preenchem da esquerda para a direita.
 *  - Cada meta tem estrela cheia só se foi cumprida; o detalhe fica em
 *    corPerdida se venceu e não cumpriu, senão em corTextoFraco.
 *  - Referências nulas ou arrays curtos são ignorados (a cena é ligada
 *    pelo NotaBuilder).
 * ============================================================ */

public class TelaDaNota : MonoBehaviour
{
    [Header("Estrelas")]
    [Tooltip("As 3 estrelas grandes, da esquerda para a direita.")]
    [SerializeField] private Image[] estrelas;
    [SerializeField] private Sprite estrelaCheia;
    [SerializeField] private Sprite estrelaVazia;

    [Header("Cores")]
    [SerializeField] private Color corCheia = new Color(0.25f, 1f, 0.8f, 1f);
    [SerializeField] private Color corVazia = new Color(1f, 1f, 1f, 0.28f);
    [SerializeField] private Color corPerdida = new Color(1f, 0.4f, 0.4f, 1f);
    [SerializeField] private Color corTextoFraco = new Color(1f, 1f, 1f, 0.55f);
    [SerializeField] private Color corRecordeNovo = new Color(1f, 0.82f, 0.3f, 1f);

    [Header("Nota")]
    [SerializeField] private TextMeshProUGUI textoNota;
    [SerializeField] private TextMeshProUGUI textoRecorde;

    [Header("Metas (3, na ordem de AvaliacaoDaPartida.Metas)")]
    [SerializeField] private Image[] estrelasDasMetas;
    [SerializeField] private TextMeshProUGUI[] nomesDasMetas;
    [SerializeField] private TextMeshProUGUI[] detalhesDasMetas;

    [Header("Pontos (6 linhas)")]
    [SerializeField] private TextMeshProUGUI[] rotulosDePontos;
    [SerializeField] private TextMeshProUGUI[] valoresDePontos;

    public void Mostrar(ResultadoDaNota n, DadosDaPartida d, RegrasDaNota r, bool recordeNovo, int guardado)
    {
        PintarEstrelas(estrelas, n.Estrelas, estrelaCheia, estrelaVazia, corCheia, corVazia);

        if (textoNota != null)
            textoNota.text = AvaliacaoDaPartida.Formatar(n.Pontos);

        if (textoRecorde != null)
        {
            textoRecorde.text = AvaliacaoDaPartida.TextoDoRecorde(recordeNovo, guardado);
            textoRecorde.color = recordeNovo ? corRecordeNovo : corTextoFraco;
            textoRecorde.fontStyle = recordeNovo ? FontStyles.Bold : FontStyles.Normal;
        }

        MostrarMetas(n, d, r);
        MostrarPontos(n, d, r);
    }

    private void MostrarMetas(ResultadoDaNota n, DadosDaPartida d, RegrasDaNota r)
    {
        for (int i = 0; i < AvaliacaoDaPartida.Metas.Length; i++)
        {
            MetaDaPartida meta = AvaliacaoDaPartida.Metas[i];
            bool cumpriu = n.Cumpriu(meta);

            TextMeshProUGUI nome = Item(nomesDasMetas, i);
            if (nome != null)
                nome.text = AvaliacaoDaPartida.Nome(meta);

            TextMeshProUGUI detalhe = Item(detalhesDasMetas, i);
            if (detalhe != null)
            {
                detalhe.text = AvaliacaoDaPartida.DetalheFinal(meta, d, r);
                detalhe.color = d.Venceu && !cumpriu ? corPerdida : corTextoFraco;
            }

            Image estrela = Item(estrelasDasMetas, i);
            if (estrela != null)
            {
                estrela.sprite = cumpriu ? estrelaCheia : estrelaVazia;
                estrela.color = cumpriu ? corCheia : corVazia;
            }
        }
    }

    private void MostrarPontos(ResultadoDaNota n, DadosDaPartida d, RegrasDaNota r)
    {
        var linhas = AvaliacaoDaPartida.LinhasDePontos(n, d, r);
        int total = Mathf.Max(rotulosDePontos != null ? rotulosDePontos.Length : 0,
                              valoresDePontos != null ? valoresDePontos.Length : 0);

        for (int i = 0; i < total; i++)
        {
            bool tem = i < linhas.Count;
            TextMeshProUGUI rotulo = Item(rotulosDePontos, i);
            TextMeshProUGUI valor = Item(valoresDePontos, i);
            if (rotulo != null) rotulo.text = tem ? linhas[i].rotulo : "";
            if (valor != null) valor.text = tem ? linhas[i].valor : "";
        }
    }

    private static T Item<T>(T[] itens, int i) where T : class =>
        itens != null && i >= 0 && i < itens.Length ? itens[i] : null;

    /* Preenche 'quantas' estrelas da esquerda para a direita. Ignora itens nulos. */
    public static void PintarEstrelas(Image[] estrelas, int quantas, Sprite cheia, Sprite vazia, Color corCheia, Color corVazia)
    {
        if (estrelas == null)
            return;

        for (int i = 0; i < estrelas.Length; i++)
        {
            if (estrelas[i] == null)
                continue;
            bool preenchida = i < quantas;
            estrelas[i].sprite = preenchida ? cheia : vazia;
            estrelas[i].color = preenchida ? corCheia : corVazia;
        }
    }
}
