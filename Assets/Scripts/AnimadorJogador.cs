using UnityEngine;

/* ============================================================
 *  AnimadorJogador.cs
 *  ---------------------------------------------------------------------
 *  Liga o movimento real do ControladorJogador.cs ao Animator do corpo do
 *  agente de saúde (filho "Modelo", ver PlayerBuilder.cs), que usa o
 *  controller gerado Agente_AnimController.controller:
 *
 *    Speed  (Float)  Blend Tree Idle (0) / Walk (0.5) / Run (1)
 *    NoChao (Bool)   false = estado "NoAr" (pose de pulo/queda)
 *
 *  O Speed é mapeado pela velocidade REAL: andando a velocidadeAndar dá
 *  exatamente 0.5 (Walk) e correndo a velocidadeCorrer dá 1 (Run). As
 *  passadas das animações foram feitas pra essas velocidades, então os pés
 *  não "patinam" no chão.
 *
 *  Não precisa mais da correção de "pés grudados no chão" por frame que o
 *  corpo antigo (médico) exigia: as animações do agente são in-place e
 *  com os pés no chão desde a origem (modelo gerado em
 *  Ferramentas/Blender/gerar_agente.py). Quem move o player é sempre o
 *  CharacterController — Animator.applyRootMotion fica false.
 * ============================================================ */

[RequireComponent(typeof(ControladorJogador))]
public class AnimadorJogador : MonoBehaviour
{
    private static readonly int ParametroSpeed = Animator.StringToHash("Speed");
    private static readonly int ParametroNoChao = Animator.StringToHash("NoChao");

    [Tooltip("Suaviza as mudanças do parâmetro Speed (maior = responde mais rápido).")]
    [SerializeField] private float suavizacao = 10f;

    [Tooltip("Tempo no ar antes de trocar pra pose de pulo/queda (evita piscar ao descer degraus).")]
    [SerializeField] private float tempoAntesDeCair = 0.15f;

    private ControladorJogador jogador;
    private Animator animator;
    private bool temNoChao;
    private float speedAtual;

    void Start()
    {
        jogador = GetComponent<ControladorJogador>();
        animator = GetComponentInChildren<Animator>();
        if (animator == null)
            return;

        foreach (AnimatorControllerParameter p in animator.parameters)
            if (p.nameHash == ParametroNoChao)
                temNoChao = true;
    }

    void Update()
    {
        if (animator == null || jogador == null)
            return;

        float v = jogador.VelocidadeHorizontal;
        float andar = Mathf.Max(jogador.VelocidadeAndar, 0.01f);
        float alvo = v <= andar
            ? 0.5f * v / andar
            : 0.5f + 0.5f * Mathf.InverseLerp(andar, jogador.VelocidadeCorrer, v);

        speedAtual = Mathf.Lerp(speedAtual, alvo, 1f - Mathf.Exp(-suavizacao * Time.deltaTime));
        animator.SetFloat(ParametroSpeed, speedAtual);

        // Pulo (subindo) troca na hora; só a queda sem pulo espera um pouco.
        if (temNoChao)
            animator.SetBool(ParametroNoChao, jogador.NoChao ||
                                              (jogador.TempoNoAr < tempoAntesDeCair && jogador.VelocidadeVertical <= 0f));
    }
}
