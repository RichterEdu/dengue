using UnityEngine;

/* ============================================================
 *  CanalDeAudio.cs
 *  ---------------------------------------------------------------------
 *  Põe uma AudioSource no canal "Música" ou "Efeitos" das Configurações
 *  (menu de pausa). O volume da fonte vira:
 *
 *      Volume  ×  volume do canal
 *
 *  (o "Volume geral" já vale pra tudo pelo AudioListener, com ou sem
 *  este componente). Coloque em toda AudioSource nova — trilha sonora no
 *  canal Música; zumbido, notificação e sons de foco em Efeitos.
 *
 *  - Volume começa com o volume do Inspector. Script que faz fade mexe em
 *    Volume, NUNCA no AudioSource.volume (seria desfeito no próximo Mudou).
 *  - CanalDeAudio.Criar(...) monta um filho com AudioSource + canal — é o
 *    que os componentes de som (SomDosFocos, TrilhaSonora...) usam.
 * ============================================================ */

[RequireComponent(typeof(AudioSource))]
public class CanalDeAudio : MonoBehaviour
{
    [SerializeField] private CanalSom canal = CanalSom.Efeitos;

    private AudioSource fonte;
    private float volumeBase;
    private bool iniciado;

    public CanalSom Canal
    {
        get => canal;
        set
        {
            canal = value;
            Aplicar();
        }
    }

    /* Volume antes do canal (0 a 1). */
    public float Volume
    {
        get
        {
            Iniciar();
            return volumeBase;
        }
        set
        {
            Iniciar();
            volumeBase = Mathf.Max(0f, value);
            Aplicar();
        }
    }

    public AudioSource Fonte
    {
        get
        {
            Iniciar();
            return fonte;
        }
    }

    void Awake()
    {
        Iniciar();
    }

    void OnEnable()
    {
        ConfiguracoesJogo.Mudou += Aplicar;
        Aplicar();
    }

    void OnDisable()
    {
        ConfiguracoesJogo.Mudou -= Aplicar;
    }

    private void Iniciar()
    {
        if (iniciado)
            return;
        iniciado = true;
        fonte = GetComponent<AudioSource>();
        volumeBase = fonte.volume;
    }

    private void Aplicar()
    {
        Iniciar();
        if (fonte != null)
            fonte.volume = volumeBase * ConfiguracoesJogo.Volume(canal);
    }

    /* Cria um filho de `pai` com AudioSource (sem tocar ao nascer) e este
     * componente. espacial = som 3D, com rolloff logarítmico e sem Doppler. */
    public static CanalDeAudio Criar(Transform pai, string nome, CanalSom canal, bool espacial)
    {
        GameObject go = new GameObject(nome);
        go.transform.SetParent(pai, false);

        AudioSource fonte = go.AddComponent<AudioSource>();
        fonte.playOnAwake = false;
        fonte.volume = 1f;
        fonte.spatialBlend = espacial ? 1f : 0f;
        if (espacial)
        {
            fonte.rolloffMode = AudioRolloffMode.Logarithmic;
            fonte.dopplerLevel = 0f;
        }

        CanalDeAudio c = go.AddComponent<CanalDeAudio>();
        c.Canal = canal;
        return c;
    }
}
