using System;
using UnityEngine;
using UnityEngine.InputSystem;

/* ============================================================
 *  CameraMenu.cs
 *  ---------------------------------------------------------------------
 *  Câmera do MENU INICIAL: percorre as tomadas em ciclo (rua do
 *  hospital, praça, sobrevoo...). Cada tomada é um travelling lento de
 *  (posInicio, alvoInicio) até (posFim, alvoFim); na troca, o fundo
 *  escurece e clareia (fadeTomada — fica ATRÁS do texto do menu, então
 *  só o cenário pisca).
 *
 *  Parallax leve: a câmera gira um pouco seguindo o mouse.
 *  Tudo em tempo real (unscaledDeltaTime).
 *
 *  Criado pelo menu  Dengue > Criar Menu Inicial  (MenuInicialBuilder.cs),
 *  com as tomadas aprovadas no preview. Selecione a câmera para ver os
 *  caminhos na Scene view.
 * ============================================================ */

public class CameraMenu : MonoBehaviour
{
    [Serializable]
    public class Tomada
    {
        public string nome;
        public Vector3 posInicio;
        public Vector3 alvoInicio;
        public Vector3 posFim;
        public Vector3 alvoFim;
        [Min(1f)] public float duracao = 20f;
    }

    [SerializeField] private Tomada[] tomadas = new Tomada[0];

    [Header("Troca de tomada")]
    [Tooltip("Tela preta atrás do texto do menu (escurece só o cenário).")]
    [SerializeField] private CanvasGroup fadeTomada;
    [SerializeField] private float duracaoFade = 0.6f;

    [Header("Parallax pelo mouse")]
    [Tooltip("Quanto a câmera gira seguindo o mouse (graus).")]
    [SerializeField] private float parallax = 1.5f;
    [SerializeField] private float suavidadeParallax = 3f;

    private int indice;
    private float tempo;
    private Vector2 parallaxAtual;

    void Start()
    {
        Posicionar();
    }

    void LateUpdate()
    {
        if (tomadas == null || tomadas.Length == 0)
            return;

        tempo += Time.unscaledDeltaTime;
        if (tempo >= tomadas[indice].duracao)
        {
            tempo = 0f;
            indice = (indice + 1) % tomadas.Length;
        }

        AtualizarParallax();
        Posicionar();
    }

    private void Posicionar()
    {
        if (tomadas == null || tomadas.Length == 0)
            return;

        Tomada t = tomadas[indice];
        float u = Mathf.Clamp01(tempo / t.duracao);
        u = Mathf.Lerp(u, Mathf.SmoothStep(0f, 1f, u), 0.5f);   // começa e termina um pouco mais devagar

        Vector3 pos = Vector3.Lerp(t.posInicio, t.posFim, u);
        Vector3 alvo = Vector3.Lerp(t.alvoInicio, t.alvoFim, u);
        Vector3 direcao = alvo - pos;
        if (direcao.sqrMagnitude < 0.0001f)
            direcao = Vector3.forward;

        transform.SetPositionAndRotation(pos,
            Quaternion.LookRotation(direcao) * Quaternion.Euler(-parallaxAtual.y * parallax, parallaxAtual.x * parallax, 0f));

        if (fadeTomada != null)
        {
            // Clareia no começo da tomada e escurece no fim.
            float entrada = duracaoFade > 0f ? 1f - Mathf.Clamp01(tempo / duracaoFade) : 0f;
            float saida = duracaoFade > 0f ? Mathf.Clamp01((tempo - (t.duracao - duracaoFade)) / duracaoFade) : 0f;
            fadeTomada.alpha = Mathf.Max(entrada, saida);
        }
    }

    private void AtualizarParallax()
    {
        Vector2 alvo = Vector2.zero;
        Mouse mouse = Mouse.current;
        if (mouse != null && Screen.width > 0 && Screen.height > 0)
        {
            Vector2 p = mouse.position.ReadValue();
            alvo = new Vector2(p.x / Screen.width * 2f - 1f, p.y / Screen.height * 2f - 1f);
            alvo = Vector2.ClampMagnitude(alvo, 1.5f);
        }
        parallaxAtual = Vector2.Lerp(parallaxAtual, alvo, 1f - Mathf.Exp(-suavidadeParallax * Time.unscaledDeltaTime));
    }

    void OnDrawGizmosSelected()
    {
        if (tomadas == null)
            return;
        foreach (Tomada t in tomadas)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(t.posInicio, t.posFim);
            Gizmos.DrawWireSphere(t.posInicio, 0.5f);
            Gizmos.color = new Color(1f, 0.5f, 0f);
            Gizmos.DrawLine(t.posInicio, t.alvoInicio);
            Gizmos.DrawLine(t.posFim, t.alvoFim);
        }
    }
}
