using UnityEngine;
using UnityEngine.InputSystem;

/* ============================================================
 *  Mover.cs
 *  ---------------------------------------------------------------------
 *  Responsável por MOVER o personagem na horizontal e aplicar gravidade.
 *
 *  Usa CharacterController (não Rigidbody), o que resolve o problema
 *  de travar em rampas e pequenas elevações do terreno.
 *
 *  - Leitura das teclas W/A/S/D usando o Input System NOVO.
 *  - Movimento relativo à direção em que o player olha (yaw do MouseLook).
 *  - Aceleração/frenagem suaves (sem arranque/parada "robótica").
 *  - Roda no Update (mesmo ritmo da câmera) para evitar tremedeira.
 *  - Gravidade e pulo aplicados num único CharacterController.Move().
 *
 *  O pulo é disparado pelo Pular.cs via TriggerJump().
 * ============================================================ */

[RequireComponent(typeof(CharacterController))]
public class Mover : MonoBehaviour
{
    [Header("Movimento")]
    [Tooltip("Velocidade máxima do personagem, em unidades por segundo.")]
    [SerializeField] private float moveSpeed = 5f;

    [Tooltip("Quão rápido atinge a velocidade máxima no chão (unid/s²).")]
    [SerializeField] private float acceleration = 40f;

    [Tooltip("Quão rápido para ao soltar as teclas no chão (unid/s²).")]
    [SerializeField] private float deceleration = 50f;

    [Tooltip("Controle no ar (0 = nenhum, 1 = igual ao chão).")]
    [Range(0f, 1f)]
    [SerializeField] private float airControl = 0.5f;

    [Header("Gravidade")]
    [Tooltip("Gravidade aplicada ao personagem (valor negativo).")]
    [SerializeField] private float gravity = -20f;

    [Header("Pulo")]
    [Tooltip("Velocidade vertical inicial do pulo.")]
    [SerializeField] private float jumpForce = 3f;

    private CharacterController controller;
    private Vector3 horizontalVelocity;
    private float verticalVelocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        float dt = Time.deltaTime;
        bool grounded = controller.isGrounded;

        // --- Horizontal: acelera em direção à velocidade desejada ---
        Vector3 targetVelocity = GetMoveDirection(GetMoveInput()) * moveSpeed;
        float rate = targetVelocity.sqrMagnitude > 0.001f ? acceleration : deceleration;
        if (!grounded)
            rate *= airControl;
        horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, targetVelocity, rate * dt);

        // --- Vertical: gravidade ---
        if (grounded && verticalVelocity < 0f)
            verticalVelocity = -2f; // Pequena força para manter colado no chão/rampas

        verticalVelocity += gravity * dt;

        // --- Move tudo de uma vez ---
        Vector3 move = horizontalVelocity + Vector3.up * verticalVelocity;
        CollisionFlags flags = controller.Move(move * dt);

        // Bateu a cabeça no teto: corta a subida.
        if ((flags & CollisionFlags.Above) != 0 && verticalVelocity > 0f)
            verticalVelocity = 0f;

        // Bateu numa parede: não "acumula" velocidade contra ela.
        if ((flags & CollisionFlags.Sides) != 0)
        {
            Vector3 actual = controller.velocity;
            actual.y = 0f;
            if (actual.sqrMagnitude < horizontalVelocity.sqrMagnitude)
                horizontalVelocity = actual;
        }
    }

    /* Chamado pelo Pular.cs para aplicar o pulo */
    public void TriggerJump()
    {
        verticalVelocity = jumpForce;
    }

    /* Lê as teclas W/A/S/D e devolve um vetor 2D */
    private Vector2 GetMoveInput()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null)
            return Vector2.zero;

        Vector2 input = Vector2.zero;
        if (kb.wKey.isPressed) input.y += 1f;
        if (kb.sKey.isPressed) input.y -= 1f;
        if (kb.aKey.isPressed) input.x -= 1f;
        if (kb.dKey.isPressed) input.x += 1f;
        return input;
    }

    /* Direção no mundo relativa ao corpo do player (o MouseLook gira o
       transform no eixo Y), sem influência do olhar para cima/baixo. */
    private Vector3 GetMoveDirection(Vector2 input)
    {
        Vector3 forward = transform.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 right = transform.right;
        right.y = 0f;
        right.Normalize();

        Vector3 direction = forward * input.y + right * input.x;
        return Vector3.ClampMagnitude(direction, 1f);
    }
}
