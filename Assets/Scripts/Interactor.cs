using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/* ============================================================
 *  Interactor.cs
 *  ---------------------------------------------------------------------
 *  Fica no PLAYER e gerencia a interação em primeira pessoa.
 *
 *  - Dispara um raycast do CENTRO da tela (como a visão do jogador).
 *  - Clique ESQUERDO do mouse = interagir com o objeto mirado. No
 *    controle, a ação "Attack" do EntradaJogador.cs (botão Oeste).
 *  - Funciona com qualquer objeto que tenha um Interactable
 *    (ou derivado) no próprio objeto ou nos pais.
 *  - Destaque visual: quando o jogador mira num foco, os materiais
 *    (Renderer) do alvo ganham um brilho de emissão pulsante. Ao
 *    deselecionar, cada material é restaurado ao estado original.
 *  - Usa o Input System NOVO (Mouse.current).
 * ============================================================ */

public class Interactor : MonoBehaviour
{
    [Header("Interação")]
    [Tooltip("Distância máxima de interação, em unidades.")]
    [SerializeField] private float interactionRange = 3f;

    [Header("Destaque ao mirar")]
    [Tooltip("Cor do brilho de emissão aplicada ao objeto mirado.")]
    [SerializeField] private Color highlightColor = new Color(1f, 0.85f, 0.2f);
    [Tooltip("Intensidade base do brilho.")]
    [SerializeField] private float highlightIntensity = 1.6f;
    [Tooltip("Velocidade do pulso do brilho (0 = brilho fixo).")]
    [SerializeField] private float highlightPulseSpeed = 4.5f;

    private Camera cam;
    private EntradaJogador entrada;
    private Interactable currentTarget;

    // O agente de saúde (corpo do próprio jogador, ver PlayerBuilder.cs) não tem
    // Collider hoje, então o raycast abaixo não tem como acertar o próprio
    // corpo — mas se algum dia ganhar física (roupa, ragdoll etc.), esta
    // máscara evita que o jogador "mire em si mesmo" sem querer.
    // NÃO pode ser "static readonly" com inicializador: LayerMask.NameToLayer
    // é proibido em inicializador de campo/construtor estático de
    // MonoBehaviour (Unity lança exceção toda vez que o tipo é usado) —
    // por isso é lido em Start(), como qualquer outra referência da cena.
    private int camadaCorpoJogador = -1;

    private readonly List<HighlightEntry> highlighted = new List<HighlightEntry>();

    /* Alvo atual (usado por scripts/EA futuros para leitura do foco mirado). */
    public Interactable CurrentTarget => currentTarget;

    /* Quando true, o jogador não mira nem interage com nada (usado, por
     * exemplo, enquanto o celular está levantado). Ao bloquear, o destaque
     * do alvo atual é desfeito. */
    public bool Bloqueado { get; set; }

    /* Guarda o estado ORIGINAL de um material antes de aplicar o destaque. */
    private class HighlightEntry
    {
        public Renderer renderer;
        public Material material;
        public Color originalEmissionColor;
        public bool emissionEnabled;
    }

    void Start()
    {
        cam = Camera.main;
        entrada = GetComponent<EntradaJogador>();
        camadaCorpoJogador = LayerMask.NameToLayer("CorpoJogador");
    }

    void Update()
    {
        // Bloqueado (celular levantado, menu aberto...): larga o alvo e sai.
        if (Bloqueado)
        {
            if (currentTarget != null)
            {
                RestoreHighlight();
                currentTarget = null;
            }
            return;
        }

        // Atualiza o alvo a cada frame: se mudou, desfaz o destaque no antigo
        // e aplica no novo.
        Interactable target = RaycastTarget();
        if (target != currentTarget)
        {
            RestoreHighlight();
            currentTarget = target;
            if (currentTarget != null)
                ApplyHighlight(currentTarget);
        }

        UpdateHighlightPulse();

        // Clique esquerdo do mouse (ou "Attack" no controle) interage com o objeto mirado.
        Mouse mouse = Mouse.current;
        bool interagir = (mouse != null && mouse.leftButton.wasPressedThisFrame) ||
                         (entrada != null && entrada.InteragirPressionado);
        if (interagir && currentTarget != null)
            currentTarget.Interact();
    }

    /* Dispara um raio do centro da tela e devolve o Interactable mais próximo. */
    private Interactable RaycastTarget()
    {
        if (cam == null)
            return null;

        Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f));

        int mascara = camadaCorpoJogador >= 0 ? ~(1 << camadaCorpoJogador) : ~0;
        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange, mascara))
        {
            // Focos já concluídos que ficam na cena (ex.: vaso com areia) não são mais alvo.
            Interactable found = hit.collider.GetComponentInParent<Interactable>();
            return found != null && found.CanInteract ? found : null;
        }

        return null;
    }

    /* Aplica emissão pulsante em todos os Renderer do alvo (e filhos). */
    private void ApplyHighlight(Interactable target)
    {
        foreach (Renderer rend in target.GetComponentsInChildren<Renderer>())
        {
            if (rend == null || rend.sharedMaterial == null)
                continue;

            // Instancia o material para não afetar outros usuários do mesmo asset.
            Material mat = rend.material;
            highlighted.Add(new HighlightEntry
            {
                renderer = rend,
                material = mat,
                originalEmissionColor = mat.GetColor("_EmissionColor"),
                emissionEnabled = mat.IsKeywordEnabled("_EMISSION")
            });

            mat.EnableKeyword("_EMISSION");
        }
    }

    /* Pulsa a cor de emissão de cada material destacado a cada frame. */
    private void UpdateHighlightPulse()
    {
        if (highlighted.Count == 0)
            return;

        float intensity = highlightIntensity * (0.75f + 0.25f * Mathf.Sin(Time.time * highlightPulseSpeed));
        Color emission = highlightColor * intensity;

        for (int i = highlighted.Count - 1; i >= 0; i--)
        {
            if (highlighted[i].renderer == null)
            {
                highlighted.RemoveAt(i);
                continue;
            }
            highlighted[i].material.SetColor("_EmissionColor", emission);
        }
    }

    /* Devolve cada material ao estado original (emissão/cor/ativado). */
    private void RestoreHighlight()
    {
        foreach (HighlightEntry entry in highlighted)
        {
            if (entry.renderer == null || entry.material == null)
                continue;

            if (entry.emissionEnabled)
                entry.material.SetColor("_EmissionColor", entry.originalEmissionColor);
            else
                entry.material.DisableKeyword("_EMISSION");
        }

        highlighted.Clear();
    }
}