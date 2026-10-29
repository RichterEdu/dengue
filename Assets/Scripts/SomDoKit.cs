using UnityEngine;

/* ============================================================
 *  SomDoKit.cs
 *  ---------------------------------------------------------------------
 *  Sons do kit do agente de saúde (Parte A). Fica no Player, junto do
 *  KitDoAgente e do Interactor, e só escuta os eventos deles:
 *
 *  - TROCAR: clique de plástico ao trocar de ferramenta (OnFerramentaTrocada).
 *  - ERRO: dois bipes graves quando o Interactor recusa um foco ou um
 *    ponto de uso (OnRecusado / OnPontoRecusado), junto do aviso da HUD.
 *  - REABASTECER: zíper + batidas de plástico ao encher o kit (OnReabasteceu).
 *  - DESPEJAR: metal oco + baque da caçamba, ao esvaziar o saco de
 *    garrafas (OnSacoEsvaziado) e ao entregar o pneu (OnPneuEntregue).
 *  - PNEU: pegar (OnPneuPego) e largar no chão (OnPneuLargado).
 *  - SEGURAR: farfalhar em loop enquanto o jogador segura com um passo
 *    válido (Interactor.Avancando). Entra e sai com fade (duracaoFade),
 *    contado em tempo real: na pausa (timeScale 0) o Avancando cai e o
 *    loop termina o fade e para mesmo com o jogo parado, sem voltar
 *    tocando quando a pausa acaba.
 *
 *  Som 2D (as mãos estão coladas no AudioListener), em duas fontes: uma
 *  para os curtos (PlayOneShot) e outra só para o loop. Kit e Interactor
 *  vazios = os deste objeto ou, sem eles, os da cena; o que faltar fica
 *  mudo, sem erro. Os clipes são ligados por SomBuilder.LigarKit.
 * ============================================================ */

public class SomDoKit : MonoBehaviour
{
    [Header("Sons curtos")]
    [Tooltip("Clique de plástico ao trocar de ferramenta.")]
    [SerializeField] private AudioClip trocar;
    [Tooltip("Volume do clique de troca de ferramenta.")]
    [Range(0f, 1f)]
    [SerializeField] private float volumeTrocar = 0.5f;

    [Tooltip("Dois bipes graves quando um foco ou ponto de uso recusa a ação.")]
    [SerializeField] private AudioClip erro;
    [Tooltip("Volume do som de recusa.")]
    [Range(0f, 1f)]
    [SerializeField] private float volumeErro = 0.5f;

    [Tooltip("Zíper + batidas de plástico ao reabastecer o kit.")]
    [SerializeField] private AudioClip reabastecer;
    [Tooltip("Volume do som de reabastecer.")]
    [Range(0f, 1f)]
    [SerializeField] private float volumeReabastecer = 0.5f;

    [Tooltip("Metal oco + baque ao jogar o saco de garrafas ou o pneu na caçamba.")]
    [SerializeField] private AudioClip despejar;
    [Tooltip("Volume do som da caçamba.")]
    [Range(0f, 1f)]
    [SerializeField] private float volumeDespejar = 0.5f;

    [Tooltip("Borracha abafada ao pegar o pneu.")]
    [SerializeField] private AudioClip pneuPegar;
    [Tooltip("Volume do som de pegar o pneu.")]
    [Range(0f, 1f)]
    [SerializeField] private float volumePneuPegar = 0.5f;

    [Tooltip("Baque de borracha + quique ao largar o pneu no chão.")]
    [SerializeField] private AudioClip pneuLargar;
    [Tooltip("Volume do som de largar o pneu.")]
    [Range(0f, 1f)]
    [SerializeField] private float volumePneuLargar = 0.5f;

    [Header("Segurar (loop)")]
    [Tooltip("Farfalhar em loop enquanto o jogador segura com um passo válido.")]
    [SerializeField] private AudioClip segurarLoop;
    [Tooltip("Volume do loop de segurar.")]
    [Range(0f, 1f)]
    [SerializeField] private float volumeSegurar = 0.25f;
    [Tooltip("Segundos para o loop entrar ou sair (tempo real, vale também na pausa).")]
    [SerializeField] private float duracaoFade = 0.15f;

    [Header("Referências (vazio = este objeto ou a cena)")]
    [Tooltip("Kit do agente cujos eventos tocam os sons. Vazio = o deste objeto ou, sem ele, o da cena.")]
    [SerializeField] private KitDoAgente kit;
    [Tooltip("Interactor que avisa as recusas e se o jogador está segurando. Vazio = o deste objeto ou, sem ele, o da cena.")]
    [SerializeField] private Interactor interactor;

    private CanalDeAudio fonteCurta;
    private CanalDeAudio fonteLoop;
    private float peso;          // 0 a 1: o fade do loop
    private bool loopLigado;     // Play já dado (o isPlaying fica false com o AudioListener pausado)

    void Awake()
    {
        fonteCurta = CanalDeAudio.Criar(transform, "Som Kit", CanalSom.Efeitos, false);
        fonteLoop = CanalDeAudio.Criar(transform, "Som Kit Segurar", CanalSom.Efeitos, false);
        fonteCurta.Fonte.priority = 60;
        fonteLoop.Fonte.priority = 60;
        fonteLoop.Fonte.loop = true;
        fonteLoop.Volume = 0f;   // o Update sobe o peso enquanto segura
    }

    /* Assina no OnEnable e desassina no OnDisable, nos mesmos objetos
     * (o OnDisable também roda antes do OnDestroy). */
    void OnEnable()
    {
        AcharReferencias();

        if (kit != null)
        {
            kit.OnFerramentaTrocada += Trocou;
            kit.OnReabasteceu += Reabasteceu;
            kit.OnSacoEsvaziado += Despejou;
            kit.OnPneuPego += PegouPneu;
            kit.OnPneuLargado += LargouPneu;
            kit.OnPneuEntregue += EntregouPneu;
        }
        if (interactor != null)
        {
            interactor.OnRecusado += RecusouFoco;
            interactor.OnPontoRecusado += RecusouPonto;
        }
    }

    void OnDisable()
    {
        if (kit != null)
        {
            kit.OnFerramentaTrocada -= Trocou;
            kit.OnReabasteceu -= Reabasteceu;
            kit.OnSacoEsvaziado -= Despejou;
            kit.OnPneuPego -= PegouPneu;
            kit.OnPneuLargado -= LargouPneu;
            kit.OnPneuEntregue -= EntregouPneu;
        }
        if (interactor != null)
        {
            interactor.OnRecusado -= RecusouFoco;
            interactor.OnPontoRecusado -= RecusouPonto;
        }

        // Desligado no meio do segurar: o loop não pode ficar tocando.
        peso = 0f;
        loopLigado = false;
        if (fonteLoop != null)
        {
            fonteLoop.Volume = 0f;
            fonteLoop.Fonte.Stop();
        }
    }

    void Update()
    {
        bool segurando = interactor != null && interactor.Avancando && segurarLoop != null;
        float passo = Time.unscaledDeltaTime / Mathf.Max(0.01f, duracaoFade);
        peso = Mathf.MoveTowards(peso, segurando ? 1f : 0f, passo);
        fonteLoop.Volume = volumeSegurar * peso;

        // Pelo próprio flag, não pelo isPlaying: com o AudioListener pausado
        // (TrilhaSonora, timeScale 0) o isPlaying fica false e o Play seria
        // repetido a cada quadro, e o Stop do fim do fade nunca sairia.
        if (peso > 0f && !loopLigado)
        {
            fonteLoop.Fonte.clip = segurarLoop;
            fonteLoop.Fonte.Play();
            loopLigado = true;
        }
        else if (peso <= 0f && loopLigado)
        {
            fonteLoop.Fonte.Stop();
            loopLigado = false;
        }
    }

    /* Campo vazio: procura no próprio objeto e, sem ele, na cena. */
    private void AcharReferencias()
    {
        if (kit == null) kit = GetComponent<KitDoAgente>();
        if (kit == null) kit = FindAnyObjectByType<KitDoAgente>();
        if (interactor == null) interactor = GetComponent<Interactor>();
        if (interactor == null) interactor = FindAnyObjectByType<Interactor>();
    }

    private void Tocar(AudioClip clip, float volume)
    {
        if (clip != null && fonteCurta != null)
            fonteCurta.Fonte.PlayOneShot(clip, volume);
    }

    /* ---------------- eventos ---------------- */

    private void Trocou(Ferramenta ferramenta) => Tocar(trocar, volumeTrocar);
    private void Reabasteceu(PontoDeReabastecimento origem) => Tocar(reabastecer, volumeReabastecer);
    private void Despejou() => Tocar(despejar, volumeDespejar);
    private void PegouPneu(PneuInteractable pneu) => Tocar(pneuPegar, volumePneuPegar);
    private void LargouPneu(PneuInteractable pneu) => Tocar(pneuLargar, volumePneuLargar);
    private void EntregouPneu(PneuInteractable pneu) => Tocar(despejar, volumeDespejar);
    private void RecusouFoco(Interactable foco, ResultadoDoPasso resultado) => Tocar(erro, volumeErro);
    private void RecusouPonto(PontoDeUso ponto, string motivo) => Tocar(erro, volumeErro);
}
