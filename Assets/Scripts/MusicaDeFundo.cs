using UnityEngine;

/* ============================================================
 *  MusicaDeFundo.cs
 *  ---------------------------------------------------------------------
 *  Uma música em loop no canal Música (menu inicial).
 *
 *  Com `acompanharFade` ligado a um CanvasGroup de tela preta, o volume
 *  segue a imagem: volume × (1 − alpha). A música entra junto com o menu
 *  clareando e some quando ele escurece no "Jogar" — sem o MenuInicial
 *  precisar saber que existe música. Sem fade, entra em duracaoEntrada.
 *
 *  Tempo real (continua com o jogo pausado). Montado por
 *  Dengue > Configurar Sons (e pelo Dengue > Criar Menu Inicial).
 * ============================================================ */

public class MusicaDeFundo : MonoBehaviour
{
    [SerializeField] private AudioClip musica;
    [Range(0f, 1f)]
    [SerializeField] private float volume = 0.7f;
    [Tooltip("Opcional: tela preta de transição. O volume segue 1 − alpha.")]
    [SerializeField] private CanvasGroup acompanharFade;
    [Tooltip("Segundos do fade de entrada quando não há tela de fade.")]
    [SerializeField] private float duracaoEntrada = 2f;

    private CanalDeAudio fonte;
    private float entrada;

    void Awake()
    {
        fonte = CanalDeAudio.Criar(transform, "Fonte Musica", CanalSom.Musica, false);
        fonte.Fonte.clip = musica;
        fonte.Fonte.loop = true;
        fonte.Fonte.ignoreListenerPause = true;
        fonte.Fonte.priority = 0;
        fonte.Volume = 0f;
    }

    void Start()
    {
        if (musica != null)
            fonte.Fonte.Play();
    }

    void Update()
    {
        float claro;
        if (acompanharFade != null)
        {
            claro = 1f - acompanharFade.alpha;
        }
        else
        {
            float dt = Mathf.Min(Time.unscaledDeltaTime, 0.1f);   // o 1º frame traz o tempo de carregamento
            entrada = Mathf.MoveTowards(entrada, 1f, dt / Mathf.Max(0.01f, duracaoEntrada));
            claro = entrada;
        }
        fonte.Volume = volume * claro;
    }
}
