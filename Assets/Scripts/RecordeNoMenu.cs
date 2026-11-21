using TMPro;
using UnityEngine;
using UnityEngine.UI;

/* ============================================================
 *  RecordeNoMenu.cs
 *  ---------------------------------------------------------------------
 *  A linha "RECORDE" da tela de título (acima da assinatura): pontos e
 *  estrelas, em tamanho discreto. Sem recorde salvo, a linha não
 *  aparece. O Start lê o recorde real; Atualizar recebe o recorde por
 *  parâmetro para os testes usarem outro prefixo.
 * ============================================================ */

public class RecordeNoMenu : MonoBehaviour
{
    [Tooltip("A linha inteira: escondida quando não há recorde.")]
    [SerializeField] private GameObject linha;
    [SerializeField] private TextMeshProUGUI textoPontos;
    [SerializeField] private Image[] estrelas;
    [SerializeField] private Sprite estrelaCheia;
    [SerializeField] private Sprite estrelaVazia;
    [SerializeField] private Color corCheia = new Color(0.25f, 1f, 0.8f, 1f);
    [SerializeField] private Color corVazia = new Color(1f, 1f, 1f, 0.28f);

    void Start()
    {
        Atualizar(new RecordeDoJogo());
    }

    internal void Atualizar(RecordeDoJogo recorde)
    {
        bool tem = recorde != null && recorde.Pontos > 0;

        if (linha != null)
            linha.SetActive(tem);
        if (!tem)
            return;

        if (textoPontos != null)
            textoPontos.text = AvaliacaoDaPartida.Formatar(recorde.Pontos);
        TelaDaNota.PintarEstrelas(estrelas, recorde.Estrelas, estrelaCheia, estrelaVazia, corCheia, corVazia);
    }
}
