using UnityEngine;
using UnityEngine.InputSystem;

/* ============================================================
 *  Pular.cs
 *  ---------------------------------------------------------------------
 *  Responsável APENAS por PULAR o personagem.
 *
 *  - Usa a tecla ESPAÇO (Space) lida pelo Input System NOVO
 *    (Keyboard.current).
 *  - Só pula se o personagem estiver no chão (checagem com Raycast).
 *  - Aplica o pulo na vertical do Rigidbody, convivendo bem com o
 *    script Mover.cs (que cuida apenas do movimento horizontal).
 * ============================================================ */

[RequireComponent(typeof(Rigidbody))]
public class Pular : MonoBehaviour
{
    [Header("Configurações de pulo")]
    [Tooltip("Força do pulo. Quanto maior, mais alto o personagem sobe.")]
    [SerializeField] private float jumpForce = 6f;

    [Tooltip("Margem extra (em unidades) usada para detectar o chão. Se o pulo ficar travando, aumente este valor.")]
    [SerializeField] private float groundCheckOffset = 0.1f;

    private Rigidbody rb;

    void Start()
    {
        // Obtém o Rigidbody que está no MESMO GameObject deste script.
        rb = GetComponent<Rigidbody>();
    }

        // Update é bom para capturar o "toque" da tecla (ocorre apenas 1 frame).
    void Update()
    {
        // Condições para pular:
        // 1) Existe um teclado conectado.
        // 2) A tecla Espaço foi pressionada NESTE frame (não basta estar segurada).
        // 3) O personagem está no chão.
        if (Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame &&
            IsGrounded())
        {
            // Substitui a velocidade vertical atual (gravidade) pela força do
            // pulo. Isso garante um pulo consistente, do mesmo tamanho sempre.
            Vector3 velocity = rb.linearVelocity;
            velocity.y = jumpForce;
            rb.linearVelocity = velocity;
        }
    }

    /* Verifica se existe chão logo abaixo do personagem usando um Raycast
     * (um "raio" de detecção disparado para baixo). */
    private bool IsGrounded()
    {
        // Metade da altura do objeto (o Raycast parte do centro dele).
        float halfHeight = transform.localScale.y / 2f;

        // Distância total percorrida pelo raio = metade da altura + a margem.
        float distance = halfHeight + groundCheckOffset;

        // Se o raio atingir qualquer coisa antes da distância, está no chão.
        return Physics.Raycast(transform.position, Vector3.down, distance);
    }
}