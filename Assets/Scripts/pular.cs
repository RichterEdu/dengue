using UnityEngine;
using UnityEngine.InputSystem;

/* ============================================================
 *  Pular.cs
 *  ---------------------------------------------------------------------
 *  Responsável por disparar o PULO do personagem.
 *
 *  - Usa a tecla ESPAÇO (Space) lida pelo Input System NOVO.
 *  - Só pula se o personagem estiver no chão (CharacterController.isGrounded).
 *  - Chama o Mover.TriggerJump() para aplicar a velocidade vertical,
 *    pois gravidade e movimento ficam unificados no CharacterController.
 * ============================================================ */

[RequireComponent(typeof(Mover))]
public class Pular : MonoBehaviour
{
    private CharacterController controller;
    private Mover mover;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        mover = GetComponent<Mover>();
    }

    void Update()
    {
        // Só pula se houver teclado, tecla foi pressionada NESTE frame
        // e o personagem está no chão.
        if (Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame &&
            controller.isGrounded)
        {
            mover.TriggerJump();
        }
    }
}