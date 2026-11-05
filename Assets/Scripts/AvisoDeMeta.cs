using TMPro;
using UnityEngine;

/* ============================================================
 *  AvisoDeMeta.cs
 *  ---------------------------------------------------------------------
 *  Faixa "META PERDIDA" logo abaixo da barra de ocupação. Ouve o
 *  OnMetaPerdida do PlacarDaPartida, entra com fade, fica cerca de 3 s e
 *  sai. Sem fila: se outra meta cair, o conteúdo é trocado na hora (como
 *  no NotificacaoCelular). Não aparece depois da partida encerrada.
 *
 *  No EditMode o Start e o Update não rodam: os testes chamam Ligar,
 *  Mostrar e Avancar direto. Funciona com o grupo nulo.
 * ============================================================ */

public class AvisoDeMeta : MonoBehaviour
{
    [Tooltip("Vazio = o PlacarDaPartida da cena.")]
    [SerializeField] private PlacarDaPartida placar;
    [SerializeField] private CanvasGroup grupo;
    [Tooltip("Mostra o nome da meta perdida.")]
    [SerializeField] private TextMeshProUGUI textoMeta;
    [Tooltip("Quanto tempo o aviso fica na tela (segundos).")]
    [SerializeField] private float segundosVisivel = 3f;
    [Tooltip("Velocidade do fade (alpha por segundo).")]
    [SerializeField] private float velocidadeFade = 4f;

    internal float Restante { get; private set; }

    /* Verdadeiro enquanto a faixa está no ar (o toast do HUD desce para não ficar embaixo dela). */
    internal bool Visivel => Restante > 0f;

    void Start()
    {
        if (grupo != null)
            grupo.alpha = 0f;
        Ligar();
    }

    void Update()
    {
        Avancar(Time.deltaTime);
    }

    void OnDestroy()
    {
        if (placar != null)
            placar.OnMetaPerdida -= Mostrar;
    }

    /* Acha o placar (só se o campo estiver vazio) e assina o aviso. */
    internal void Ligar()
    {
        if (placar == null) placar = FindAnyObjectByType<PlacarDaPartida>();
        if (placar == null)
            return;

        placar.OnMetaPerdida -= Mostrar;
        placar.OnMetaPerdida += Mostrar;
    }

    internal void Mostrar(MetaDaPartida meta)
    {
        if (placar != null && placar.Fechado)
            return;

        if (textoMeta != null)
            textoMeta.text = AvaliacaoDaPartida.Nome(meta);
        Restante = segundosVisivel;
    }

    internal void Avancar(float dt)
    {
        if (placar != null && placar.Fechado)
            Restante = 0f;
        else
            Restante = Mathf.Max(0f, Restante - dt);

        if (grupo != null)
            grupo.alpha = Mathf.MoveTowards(grupo.alpha, Restante > 0f ? 1f : 0f, dt * velocidadeFade);
    }
}
