using UnityEngine;

/* ============================================================
 *  SaidaDoHospital.cs
 *  ---------------------------------------------------------------------
 *  Fica na "porta" do quarto do hospital (Assets/Editor/QuartoHospitalBuilder.cs).
 *  Quando o jogador (CharacterController) cruza o trigger, teleporta de
 *  volta pra posição original no bairro e avisa o FluxoDeJogo que a
 *  partida começou (a ocupação só sobe a partir daqui).
 *
 *  Só a POSIÇÃO é restaurada — a rotação não, de propósito: o jogador
 *  simplesmente continua olhando pra onde já estava olhando ao cruzar a
 *  porta — natural, como atravessar um vão. (Se um dia precisar forçar a
 *  direção, use ControladorJogador.DefinirOlhar: ele guarda o yaw/pitch
 *  internamente e sobrescreveria uma rotação posta direto no Transform.)
 * ============================================================ */

public class SaidaDoHospital : MonoBehaviour
{
    [Tooltip("Posição no bairro para onde o jogador é teleportado (gravada pelo QuartoHospitalBuilder).")]
    [SerializeField] private Vector3 posicaoNoBairro;

    private bool acionado;

    void OnTriggerEnter(Collider other)
    {
        if (acionado)
            return;

        CharacterController controller = other.GetComponent<CharacterController>();
        if (controller == null)
            return;

        acionado = true;

        ControladorJogador jogador = controller.GetComponent<ControladorJogador>();
        if (jogador != null)
        {
            jogador.Teleportar(posicaoNoBairro);
        }
        else
        {
            // CharacterController "briga" com transform.position direto se
            // estiver ligado durante o set — desliga, move, religa.
            controller.enabled = false;
            controller.transform.position = posicaoNoBairro;
            controller.enabled = true;
        }

        FluxoDeJogo fluxo = FindFirstObjectByType<FluxoDeJogo>();
        if (fluxo != null)
            fluxo.IniciarPartida();
    }
}
