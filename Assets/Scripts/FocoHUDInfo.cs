using UnityEngine;

/* ============================================================
 *  FocoHUDInfo.cs
 *  ---------------------------------------------------------------------
 *  Textos da HUD específicos de UM foco. Opcional: coloque no mesmo
 *  objeto do Interactable (pneu, vaso, caixa d'água...) e preencha.
 *
 *  Campos vazios usam o padrão configurado no DengueHUD.
 *
 *  Marcadores que podem ser usados nos textos:
 *    {nome}     -> o campo "Nome" abaixo
 *    {cliques}  -> cliques já dados
 *    {total}    -> cliques necessários
 *    {faltam}   -> cliques que faltam
 *  Aceita tags do TextMeshPro, ex.: <b>negrito</b>, <color=#FFD700>cor</color>
 * ============================================================ */

[DisallowMultipleComponent]
public class FocoHUDInfo : MonoBehaviour
{
    [Tooltip("Nome do foco usado nos textos. Ex.: \"o pneu\", \"a caixa d'água\".")]
    public string nome = "";

    [Tooltip("Texto ao mirar. Vazio = padrão do DengueHUD. Ex.: \"Clique para esvaziar {nome}\"")]
    [TextArea(1, 3)]
    public string textoAoMirar = "";

    [Tooltip("Dica educativa (linha menor abaixo da barra). Vazio = sem dica.")]
    [TextArea(2, 4)]
    public string dica = "";

    [Tooltip("Mensagem rápida quando o foco é eliminado. Vazio = padrão do DengueHUD.")]
    [TextArea(1, 3)]
    public string textoAoEliminar = "";

    [Header("Visual (opcional)")]
    [Tooltip("Troca a cor da barra de progresso para este foco.")]
    public bool usarCorPropria = false;
    public Color corDaBarra = new Color(0.25f, 1f, 0.8f, 1f);

    [Tooltip("Esconde a barra e o 3/5 deste foco (ex.: interação de 1 clique).")]
    public bool esconderProgresso = false;
}
