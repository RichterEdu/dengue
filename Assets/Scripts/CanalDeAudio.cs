using UnityEngine;

/* ============================================================
 *  CanalDeAudio.cs
 *  ---------------------------------------------------------------------
 *  Põe uma AudioSource no canal "Música" ou "Efeitos" das Configurações
 *  (menu de pausa). O volume da fonte vira:
 *
 *      volume do Inspector  ×  volume do canal
 *
 *  (o "Volume geral" já vale pra tudo pelo AudioListener, com ou sem
 *  este componente). Coloque em toda AudioSource nova — trilha sonora no
 *  canal Música; zumbido, notificação e sons de foco em Efeitos.
 * ============================================================ */

[RequireComponent(typeof(AudioSource))]
public class CanalDeAudio : MonoBehaviour
{
    [SerializeField] private CanalSom canal = CanalSom.Efeitos;

    private AudioSource fonte;
    private float volumeBase;

    public CanalSom Canal
    {
        get => canal;
        set
        {
            canal = value;
            Aplicar();
        }
    }

    void Awake()
    {
        fonte = GetComponent<AudioSource>();
        volumeBase = fonte.volume;
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

    private void Aplicar()
    {
        if (fonte != null)
            fonte.volume = volumeBase * ConfiguracoesJogo.Volume(canal);
    }
}
