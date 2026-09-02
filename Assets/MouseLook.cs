using UnityEngine;
using UnityEngine.InputSystem;

/* ============================================================
 *  MouseLook.cs
 *  ---------------------------------------------------------------------
 *  Responsável pela VISÃO EM PRIMEIRA PESSOA do personagem (cubo).
 *
 *  - O MOUSE controla a direção do olhar:
 *        mouse para a direita  -> gira o CUBO para a direita (eixo Y / yaw)
 *        mouse para cima/baixo -> gira a CÂMERA para cima/baixo (eixo X / pitch)
 *  - A câmera NÃO precisa ser filha do cubo: este script SINCRONIZA a
 *    posição e a rotação dela com o cubo a cada frame ("altura dos olhos").
 *  - Trava e esconde o cursor do mouse.
 *
 *  Depende do Input System NOVO (Mouse.current).
 *  A trava de rotação do Rigidbody (FreezeRotation) fica no Mover.cs
 *  e não conflita, pois aqui giramos o Transform diretamente.
 * ============================================================ */

public class MouseLook : MonoBehaviour
{
    [Header("Visão")]
    [Tooltip("Altura da câmera em relação ao centro do cubo (simula os olhos).")]
    [SerializeField] private float eyeHeight = 0.7f;

    [Tooltip("Sensibilidade do mouse. Quanto maior, mais rápido gira a visão.")]
    [SerializeField] private float mouseSensitivity = 2f;

    [Tooltip("Limite de inclinação vertical (pitch) em graus.")]
    [SerializeField] private float maxPitch = 90f;

    private float yaw;        // Rotação horizontal acumulada (cubo)
    private float pitch;      // Rotação vertical acumulada (câmera)
    private Transform cam;    // Câmera principal

    void Start()
    {
        // Trava e esconde o cursor para capturar o movimento do mouse.
        Cursor.lockState = CursorLockMode.Locked;

        // Guarda a referência da câmera principal.
        if (Camera.main != null)
            cam = Camera.main.transform;
    }

    void Update()
    {
        // Se não há teclado/mouse conectados, não faz nada.
        Mouse mouse = Mouse.current;
        if (mouse == null || cam == null)
            return;

        // Lê o deslocamento do mouse neste frame.
        Vector2 delta = mouse.delta.ReadValue();

        // Acumula a rotação. "up" (yaw) gira o cubo; "right" (pitch) a câmera.
        yaw += delta.x * mouseSensitivity;
        pitch -= delta.y * mouseSensitivity;   // "-": mover o mouse para cima sobe a visão

        // Limita o pitch para não virar a cabeça de ponta-cabeça.
        pitch = Mathf.Clamp(pitch, -maxPitch, maxPitch);

        // Gira o CUBO apenas no eixo vertical (para onde aponta a visão).
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
    }

    // LateUpdate roda DEPOIS do movimento/física, então a câmera acompanha
    // a posição final do cubo neste frame.
    void LateUpdate()
    {
        if (cam == null)
            return;

        // Posiciona a câmera na "cabeça" do cubo (centro + altura dos olhos).
        cam.position = transform.position + transform.up * eyeHeight;

        // Aplica a rotação completa: vertical (pitch) + horizontal (yaw).
        cam.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }
}