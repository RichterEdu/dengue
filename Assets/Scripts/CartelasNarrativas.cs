using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

/* ============================================================
 *  CartelasNarrativas.cs
 *  ---------------------------------------------------------------------
 *  Tela preta com uma frase por vez — as "cartelas" da abertura e do
 *  epílogo (quem decide o QUE mostrar e QUANDO é o HistoriaDoJogo.cs).
 *
 *  - Cada frase entra com fade, fica o tempo proporcional ao tamanho do
 *    texto e sai com fade.
 *  - Clique / Espaço / Enter avança; Esc pula o resto.
 *  - Roda em tempo REAL (unscaledDeltaTime): funciona com o jogo pausado
 *    (Time.timeScale = 0), que é como a abertura segura o resto do jogo.
 *
 *  A hierarquia é gerada pelo menu  Dengue > Configurar Historia
 *  (Assets/Editor/HistoriaBuilder.cs), que já liga as referências.
 * ============================================================ */

public class CartelasNarrativas : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("CanvasGroup da tela inteira (fundo preto + texto).")]
    [SerializeField] private CanvasGroup grupo;
    [SerializeField] private CanvasGroup textoGrupo;
    [SerializeField] private TextMeshProUGUI texto;
    [SerializeField] private CanvasGroup dicaGrupo;

    [Header("Ritmo (segundos)")]
    [SerializeField] private float fadeFundo = 0.9f;
    [SerializeField] private float fadeTexto = 0.55f;
    [Tooltip("Tempo mínimo que cada frase fica na tela.")]
    [SerializeField] private float tempoBase = 1.8f;
    [Tooltip("Tempo extra por letra (frase longa fica mais).")]
    [SerializeField] private float tempoPorLetra = 0.045f;
    [SerializeField] private float tempoMaximo = 7f;
    [Tooltip("Alpha da dica \"clique para continuar\".")]
    [SerializeField] private float alphaDica = 0.5f;

    private static readonly Regex Tags = new Regex("<.*?>");

    private bool pedirProxima;
    private bool pedirPularTudo;
    private Coroutine rotina;

    public bool Tocando { get; private set; }

    /* Tela preta já no primeiro frame (antes do Start), para a abertura
     * não mostrar o hospital por um instante antes de escurecer. */
    public void MostrarFundoImediato()
    {
        if (grupo != null) grupo.alpha = 1f;
        if (textoGrupo != null) textoGrupo.alpha = 0f;
        if (dicaGrupo != null) dicaGrupo.alpha = 0f;
    }

    public void EsconderImediato()
    {
        if (grupo != null) grupo.alpha = 0f;
        if (textoGrupo != null) textoGrupo.alpha = 0f;
        if (dicaGrupo != null) dicaGrupo.alpha = 0f;
    }

    /* Toca as frases em sequência. antesDeClarear é chamado quando a última
     * frase some, logo antes da tela preta começar a clarear (o epílogo usa
     * isso para a tela de estatísticas já vir surgindo por baixo);
     * aoTerminar, quando tudo acabou. */
    public void Tocar(IList<string> linhas, Action aoTerminar, Action antesDeClarear = null)
    {
        if (rotina != null)
            StopCoroutine(rotina);
        rotina = StartCoroutine(Rotina(linhas, aoTerminar, antesDeClarear));
    }

    void Update()
    {
        if (!Tocando)
            return;

        Keyboard teclado = Keyboard.current;
        Mouse mouse = Mouse.current;

        if ((mouse != null && mouse.leftButton.wasPressedThisFrame) ||
            (teclado != null && (teclado.spaceKey.wasPressedThisFrame ||
                                 teclado.enterKey.wasPressedThisFrame ||
                                 teclado.numpadEnterKey.wasPressedThisFrame)))
            pedirProxima = true;

        if (teclado != null && teclado.escapeKey.wasPressedThisFrame)
            pedirPularTudo = true;
    }

    private IEnumerator Rotina(IList<string> linhas, Action aoTerminar, Action antesDeClarear)
    {
        Tocando = true;
        pedirProxima = false;
        pedirPularTudo = false;
        if (textoGrupo != null) textoGrupo.alpha = 0f;

        yield return Fade(grupo, 1f, fadeFundo, false);
        StartCoroutine(Fade(dicaGrupo, alphaDica, fadeTexto, false));

        if (linhas != null)
        {
            foreach (string linha in linhas)
            {
                if (pedirPularTudo)
                    break;
                if (string.IsNullOrWhiteSpace(linha))
                    continue;

                if (texto != null) texto.text = linha;
                pedirProxima = false;

                yield return Fade(textoGrupo, 1f, fadeTexto, true);
                pedirProxima = false; // clique durante a entrada só completa o fade; o próximo avança

                float segurar = Mathf.Min(tempoMaximo, tempoBase + Tags.Replace(linha, "").Length * tempoPorLetra);
                for (float t = 0f; t < segurar && !pedirProxima && !pedirPularTudo; t += Time.unscaledDeltaTime)
                    yield return null;

                pedirProxima = false;
                yield return Fade(textoGrupo, 0f, fadeTexto * 0.7f, true);
            }
        }

        if (textoGrupo != null) textoGrupo.alpha = 0f;
        StartCoroutine(Fade(dicaGrupo, 0f, fadeTexto * 0.5f, false));
        antesDeClarear?.Invoke();

        yield return Fade(grupo, 0f, fadeFundo, false);

        Tocando = false;
        rotina = null;
        aoTerminar?.Invoke();
    }

    /* Fade em tempo real. interrompivel = um clique completa na hora. */
    private IEnumerator Fade(CanvasGroup alvo, float destino, float duracao, bool interrompivel)
    {
        if (alvo == null)
            yield break;

        float inicio = alvo.alpha;
        if (duracao <= 0f || Mathf.Approximately(inicio, destino))
        {
            alvo.alpha = destino;
            yield break;
        }

        for (float t = 0f; t < duracao; t += Time.unscaledDeltaTime)
        {
            if (interrompivel && (pedirProxima || pedirPularTudo))
                break;
            alvo.alpha = Mathf.Lerp(inicio, destino, t / duracao);
            yield return null;
        }
        alvo.alpha = destino;
    }
}
