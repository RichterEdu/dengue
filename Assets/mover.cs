using UnityEngine;
using UnityEngine.InputSystem;

/* ============================================================
 *  Mover.cs
 *  ---------------------------------------------------------------------
 *  Responsável APENAS por MOVER o personagem na horizontal (no chão).
 *
 *  - Leitura das teclas W/A/S/D usando o Input System NOVO
 *    (Keyboard.current) — necessário pois o projeto desativou o
 *    Input antigo (Input.GetKey*).
 *  - O movimento é RELATIVO À CÂMERA:
 *        W = frente da câmera      A = esquerda da câmera
 *        S = trás da câmera        D = direita da câmera
 *  - Trabalha junto com a física (Rigidbody): aplica velocidade
 *    horizontal mantendo a vertical (gravidade) intacta.
 *
 *  O pulo fica em OUTRO script: Pular.cs
 * ============================================================ */

[RequireComponent(typeof(Rigidbody))]
public class Mover : MonoBehaviour
{
    [Header("Movimento")]
    [Tooltip("Velocidade máxima do personagem, em unidades por segundo.")]
    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody rb;              // Rigidbody do personagem (física)
    private Transform cameraTransform; // Câmera principal (direção do movimento)

    void Start()
    {
        // Obtém o Rigidbody que está no MESMO GameObject deste script.
        rb = GetComponent<Rigidbody>();

        // Congela a rotação em todos os eixos para o objeto não "tombar"
        // enquanto se move (comum quando se usa um Box Collider).
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        // Guarda a referência da câmera principal para calcular a direção.
        if (Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    // FixedUpdate roda em intervalos fixos → lugar correto para física.
    void FixedUpdate()
    {
        Vector2 input = GetMoveInput();

        // Converte o input 2D (X = lateral, Y = frente/trás) em uma
        // direção no mundo, relativa à orientação da câmera.
        Vector3 direction = GetCameraRelativeDirection(input);

        // Mantém a velocidade vertical (gravidade/pulo) e só altera o plano
        // horizontal. Atribuir direto à velocidade dá um movimento preciso.
        Vector3 velocity = rb.linearVelocity;
        velocity.x = direction.x * moveSpeed;
        velocity.z = direction.z * moveSpeed;
        rb.linearVelocity = velocity;
    }

    /* Lê as teclas W/A/S/D e devolve um vetor 2D:
     *   x = +1 (D) / -1 (A)  -> lateral
     *   y = +1 (W) / -1 (S)  -> frente/trás */
    private Vector2 GetMoveInput()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null)
            return Vector2.zero;          // Nenhum teclado conectado

        Vector2 input = Vector2.zero;
        if (kb.wKey.isPressed) input.y += 1f;
        if (kb.sKey.isPressed) input.y -= 1f;
        if (kb.aKey.isPressed) input.x -= 1f;
        if (kb.dKey.isPressed) input.x += 1f;
        return input;
    }

    /* Transforma o input em uma direção no mundo usando a orientação da
     * câmera. Ex.: se a câmera estiver encarando o norte, W move para o norte. */
    private Vector3 GetCameraRelativeDirection(Vector2 input)
    {
        // Sem câmera na cena: usa a direção do próprio mundo.
        if (cameraTransform == null)
            return new Vector3(input.x, 0f, input.y).normalized;

        // Pega a frente e a direita da câmera, zerando a inclinação
        // vertical para o personagem andar sempre na horizontal.
        Vector3 forward = cameraTransform.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 right = cameraTransform.right;
        right.y = 0f;
        right.Normalize();

        // Combina: frente * (W/S) + direita * (A/D).
        Vector3 direction = forward * input.y + right * input.x;

        // Normaliza para a diagonal não ficar mais rápida que a reta.
        return direction.normalized;
    }
}