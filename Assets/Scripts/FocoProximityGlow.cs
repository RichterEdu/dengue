using System.Collections.Generic;
using UnityEngine;

/* ============================================================
 *  FocoProximityGlow.cs
 *  ---------------------------------------------------------------------
 *  Fica no PLAYER, ao lado do Interactor. Dá um brilho de emissão suave
 *  em qualquer foco de dengue (Interactable) ainda ativo quando o
 *  jogador entra num raio de distância dele — uma pista visual de
 *  proximidade, complementar ao highlight de MIRA que o Interactor já
 *  aplica (Interactor.cs).
 *
 *  Prioridade: um foco que esteja mirado (Interactor.CurrentTarget) é
 *  ignorado aqui enquanto durar a mira, para não disputar a mesma
 *  instância de material com o highlight de mira.
 *
 *  Reaproveita a técnica de emissão (_EMISSION/_EmissionColor) via
 *  EmissionHighlightUtil.cs, a mesma já usada em Interactor/Interactable.
 *
 *  Os focos vêm do GerenciadorDeFocos (do campo ou, vazio,
 *  GerenciadorDeFocos.Obter()); sem ele, nada brilha.
 * ============================================================ */

public class FocoProximityGlow : MonoBehaviour
{
    [Header("Proximidade")]
    [Tooltip("Distância (em unidades) a partir da qual o foco começa a brilhar.")]
    [SerializeField] private float raioProximidade = 5.5f;

    [Header("Brilho")]
    [Tooltip("Cor do brilho de emissão aplicado aos focos próximos.")]
    [SerializeField] private Color corBrilho = new Color(0.25f, 1f, 0.8f);

    [Tooltip("Intensidade do brilho (baixa de propósito: vários focos podem ficar próximos ao mesmo tempo e somar no Bloom da cena).")]
    [SerializeField] private float intensidadeBrilho = 0.5f;

    [Header("Referências")]
    [Tooltip("Focos da partida. Vazio = GerenciadorDeFocos.Obter().")]
    [SerializeField] private GerenciadorDeFocos gerenciador;

    private Camera cam;
    private Interactor interactor;
    private readonly List<Interactable> focos = new List<Interactable>();
    private readonly Dictionary<Interactable, List<EmissionHighlightUtil.HighlightEntry>> brilhando =
        new Dictionary<Interactable, List<EmissionHighlightUtil.HighlightEntry>>();

    void Start()
    {
        cam = Camera.main;
        interactor = GetComponent<Interactor>();
        if (gerenciador == null) gerenciador = GerenciadorDeFocos.Obter();
        if (gerenciador != null)
        {
            gerenciador.Garantir();
            focos.AddRange(gerenciador.Focos);
        }
    }

    void Update()
    {
        if (cam == null)
            return;

        Vector3 posJogador = cam.transform.position;
        float raioAoQuadrado = raioProximidade * raioProximidade;

        for (int i = focos.Count - 1; i >= 0; i--)
        {
            Interactable foco = focos[i];

            // Foco destruído (destroyOnComplete = true): descarta a entrada
            // sem tentar restaurar material (já não existe mais).
            if (foco == null)
            {
                focos.RemoveAt(i);
                brilhando.Remove(foco);
                continue;
            }

            bool elegivel = foco.gameObject.activeInHierarchy
                && foco.CanInteract
                && (interactor == null || interactor.CurrentTarget != foco);

            bool dentroDoRaio = elegivel &&
                (posJogador - foco.transform.position).sqrMagnitude <= raioAoQuadrado;

            bool jaBrilhando = brilhando.TryGetValue(foco, out List<EmissionHighlightUtil.HighlightEntry> entradas);

            if (dentroDoRaio && !jaBrilhando)
            {
                List<EmissionHighlightUtil.HighlightEntry> novasEntradas =
                    EmissionHighlightUtil.Capture(foco.GetComponentsInChildren<Renderer>());
                EmissionHighlightUtil.ApplyColor(novasEntradas, corBrilho * intensidadeBrilho);
                brilhando[foco] = novasEntradas;
            }
            else if (!dentroDoRaio && jaBrilhando)
            {
                EmissionHighlightUtil.Restore(entradas);
                brilhando.Remove(foco);
            }
        }
    }
}
