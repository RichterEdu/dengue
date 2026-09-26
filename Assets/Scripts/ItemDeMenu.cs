using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/* ============================================================
 *  ItemDeMenu.cs
 *  ---------------------------------------------------------------------
 *  Visual de um item de menu (pausa ou menu inicial): botão ou linha de
 *  configuração.
 *
 *  - Passar o mouse por cima SELECIONA o alvo: mouse e controle usam o
 *    mesmo destaque, e só um item fica destacado por vez.
 *  - Destacado = cor de fundo mais clara + barrinha de acento à esquerda
 *    (+ cor do texto, se houver um texto ligado — botões do menu inicial).
 *  - Se o alvo for um Toggle, também desenha a chave liga/desliga
 *    (bolinha de um lado pro outro, cor da pista e "Ligado/Desligado").
 *
 *  Não depende de Time (funciona com o jogo pausado). Criado pelos menus
 *  Dengue > Criar Menu de Pausa / Criar Menu Inicial.
 * ============================================================ */

public class ItemDeMenu : MonoBehaviour, IPointerEnterHandler
{
    [Tooltip("O que é selecionado/acionado. Vazio = o Selectable deste objeto.")]
    [SerializeField] private Selectable alvo;

    [Header("Destaque")]
    [SerializeField] private Graphic fundo;
    [SerializeField] private Color corNormal = new Color(0.93f, 0.94f, 0.95f, 1f);
    [SerializeField] private Color corDestaque = new Color(0.86f, 0.88f, 0.90f, 1f);
    [SerializeField] private GameObject barraFoco;
    [Tooltip("Opcional: texto que muda de cor com o destaque.")]
    [SerializeField] private Graphic texto;
    [SerializeField] private Color corTextoNormal = Color.white;
    [SerializeField] private Color corTextoDestaque = Color.white;

    [Header("Chave liga/desliga (só se o alvo for Toggle)")]
    [SerializeField] private Graphic pista;
    [SerializeField] private RectTransform bola;
    [SerializeField] private Graphic bolaGrafico;
    [SerializeField] private TextMeshProUGUI estado;
    [SerializeField] private Color pistaLigada = new Color(0.14f, 0.16f, 0.19f, 1f);
    [SerializeField] private Color pistaDesligada = new Color(0f, 0f, 0f, 0.15f);
    [SerializeField] private Color bolaLigada = Color.white;
    [SerializeField] private Color bolaDesligada = Color.white;
    [Tooltip("Distância da bolinha até a borda da pista (px).")]
    [SerializeField] private float margemBola = 4f;

    public Selectable Alvo => alvo;

    void Awake()
    {
        if (alvo == null)
            alvo = GetComponent<Selectable>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (alvo != null && alvo.IsInteractable() && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(alvo.gameObject);
    }

    void LateUpdate()
    {
        EventSystem es = EventSystem.current;
        bool selecionado = alvo != null && es != null && es.currentSelectedGameObject == alvo.gameObject;

        if (fundo != null)
            fundo.color = selecionado ? corDestaque : corNormal;
        if (barraFoco != null && barraFoco.activeSelf != selecionado)
            barraFoco.SetActive(selecionado);
        if (texto != null)
            texto.color = selecionado ? corTextoDestaque : corTextoNormal;

        if (alvo is Toggle toggle)
            DesenharChave(toggle.isOn);
    }

    private void DesenharChave(bool ligado)
    {
        if (pista != null)
            pista.color = ligado ? pistaLigada : pistaDesligada;
        if (bolaGrafico != null)
            bolaGrafico.color = ligado ? bolaLigada : bolaDesligada;
        if (bola != null)
        {
            float lado = ligado ? 1f : 0f;
            bola.anchorMin = new Vector2(lado, 0.5f);
            bola.anchorMax = new Vector2(lado, 0.5f);
            bola.pivot = new Vector2(lado, 0.5f);
            bola.anchoredPosition = new Vector2(ligado ? -margemBola : margemBola, 0f);
        }
        if (estado != null)
            estado.text = ligado ? "Ligado" : "Desligado";
    }
}
