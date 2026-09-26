using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/* ============================================================
 *  ConfiguracoesJogo.cs
 *  ---------------------------------------------------------------------
 *  As opções do jogador (tela Configurações do menu de pausa), salvas em
 *  PlayerPrefs — valem de uma partida pra outra e depois de fechar o jogo.
 *
 *    Volume geral            AudioListener.volume (tudo que toca)
 *    Música / Efeitos        AudioSources com o componente CanalDeAudio
 *    Sensibilidade           EntradaJogador.MultiplicadorSensibilidade
 *    Inverter eixo Y         EntradaJogador.InverterY
 *    Campo de visão (FOV)    CameraJogador.FovBase
 *    Balanço da câmera       CameraJogador.BalancoAtivo
 *    Qualidade gráfica       QualitySettings (níveis do projeto)
 *    Tela cheia              Screen.fullScreenMode (o Unity já guarda)
 *
 *  Carrega sozinho antes da primeira cena e reaplica no player/câmera a
 *  cada cena carregada (inclusive ao reiniciar). Mudar um valor aplica na
 *  hora; Salvar() grava no disco (o MenuPausa chama ao sair da tela).
 * ============================================================ */

public enum CanalSom
{
    Musica,
    Efeitos,
}

public static class ConfiguracoesJogo
{
    public const float VolumeGeralPadrao = 1f;
    public const float VolumeMusicaPadrao = 0.8f;
    public const float VolumeEfeitosPadrao = 1f;
    public const float SensibilidadePadrao = 1f;
    public const float SensibilidadeMin = 0.1f;
    public const float SensibilidadeMax = 3f;
    public const float FovPadrao = 60f;
    public const float FovMin = 60f;
    public const float FovMax = 100f;

    private const string Prefixo = "config.";

    private static float volumeGeral = VolumeGeralPadrao;
    private static float volumeMusica = VolumeMusicaPadrao;
    private static float volumeEfeitos = VolumeEfeitosPadrao;
    private static float sensibilidade = SensibilidadePadrao;
    private static bool inverterY;
    private static float fov = FovPadrao;
    private static bool balancoCamera = true;
    private static int qualidade = -1;   // -1 = não mexe (usa o nível padrão do projeto)
    private static int qualidadeDoProjeto;

    /* Disparado a cada mudança (depois de aplicar). */
    public static event Action Mudou;

    public static float VolumeGeral
    {
        get => volumeGeral;
        set => Definir(ref volumeGeral, Mathf.Clamp01(value));
    }

    public static float VolumeMusica
    {
        get => volumeMusica;
        set => Definir(ref volumeMusica, Mathf.Clamp01(value));
    }

    public static float VolumeEfeitos
    {
        get => volumeEfeitos;
        set => Definir(ref volumeEfeitos, Mathf.Clamp01(value));
    }

    /* Multiplicador sobre a sensibilidade do EntradaJogador (1 = a do Inspector). */
    public static float Sensibilidade
    {
        get => sensibilidade;
        set => Definir(ref sensibilidade, Mathf.Clamp(value, SensibilidadeMin, SensibilidadeMax));
    }

    public static bool InverterY
    {
        get => inverterY;
        set => Definir(ref inverterY, value);
    }

    public static float Fov
    {
        get => fov;
        set => Definir(ref fov, Mathf.Clamp(value, FovMin, FovMax));
    }

    public static bool BalancoCamera
    {
        get => balancoCamera;
        set => Definir(ref balancoCamera, value);
    }

    /* Índice em QualitySettings.names. */
    public static int Qualidade
    {
        get => qualidade >= 0 ? qualidade : QualitySettings.GetQualityLevel();
        set => Definir(ref qualidade, Mathf.Clamp(value, 0, QualitySettings.count - 1));
    }

    public static bool TelaCheia
    {
        get => Screen.fullScreen;
        set
        {
            Screen.fullScreenMode = value ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
            Mudou?.Invoke();
        }
    }

    public static float Volume(CanalSom canal)
    {
        return canal == CanalSom.Musica ? volumeMusica : volumeEfeitos;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Iniciar()
    {
        qualidadeDoProjeto = QualitySettings.GetQualityLevel();
        Carregar();
        AplicarGlobais();

        // -= antes do += : sem domain reload no Play do Editor, o estático sobrevive.
        SceneManager.sceneLoaded -= AoCarregarCena;
        SceneManager.sceneLoaded += AoCarregarCena;
    }

    /* Roda depois do Awake dos objetos da cena e antes do Start. */
    private static void AoCarregarCena(Scene cena, LoadSceneMode modo)
    {
        AplicarNaCena();
    }

    public static void Salvar()
    {
        PlayerPrefs.SetFloat(Prefixo + "volumeGeral", volumeGeral);
        PlayerPrefs.SetFloat(Prefixo + "volumeMusica", volumeMusica);
        PlayerPrefs.SetFloat(Prefixo + "volumeEfeitos", volumeEfeitos);
        PlayerPrefs.SetFloat(Prefixo + "sensibilidade", sensibilidade);
        PlayerPrefs.SetInt(Prefixo + "inverterY", inverterY ? 1 : 0);
        PlayerPrefs.SetFloat(Prefixo + "fov", fov);
        PlayerPrefs.SetInt(Prefixo + "balancoCamera", balancoCamera ? 1 : 0);
        PlayerPrefs.SetInt(Prefixo + "qualidade", qualidade);
        PlayerPrefs.Save();
    }

    public static void RestaurarPadrao()
    {
        volumeGeral = VolumeGeralPadrao;
        volumeMusica = VolumeMusicaPadrao;
        volumeEfeitos = VolumeEfeitosPadrao;
        sensibilidade = SensibilidadePadrao;
        inverterY = false;
        fov = FovPadrao;
        balancoCamera = true;
        qualidade = -1;
        if (QualitySettings.GetQualityLevel() != qualidadeDoProjeto)
            QualitySettings.SetQualityLevel(qualidadeDoProjeto, true);
        Aplicar();
    }

    private static void Carregar()
    {
        volumeGeral = Mathf.Clamp01(PlayerPrefs.GetFloat(Prefixo + "volumeGeral", VolumeGeralPadrao));
        volumeMusica = Mathf.Clamp01(PlayerPrefs.GetFloat(Prefixo + "volumeMusica", VolumeMusicaPadrao));
        volumeEfeitos = Mathf.Clamp01(PlayerPrefs.GetFloat(Prefixo + "volumeEfeitos", VolumeEfeitosPadrao));
        sensibilidade = Mathf.Clamp(PlayerPrefs.GetFloat(Prefixo + "sensibilidade", SensibilidadePadrao), SensibilidadeMin, SensibilidadeMax);
        inverterY = PlayerPrefs.GetInt(Prefixo + "inverterY", 0) == 1;
        fov = Mathf.Clamp(PlayerPrefs.GetFloat(Prefixo + "fov", FovPadrao), FovMin, FovMax);
        balancoCamera = PlayerPrefs.GetInt(Prefixo + "balancoCamera", 1) == 1;
        qualidade = PlayerPrefs.GetInt(Prefixo + "qualidade", -1);
        if (qualidade >= QualitySettings.count)
            qualidade = -1;
    }

    private static void Definir<T>(ref T campo, T valor)
    {
        if (Equals(campo, valor))
            return;
        campo = valor;
        Aplicar();
    }

    private static void Aplicar()
    {
        AplicarGlobais();
        AplicarNaCena();
        Mudou?.Invoke();
    }

    private static void AplicarGlobais()
    {
        AudioListener.volume = volumeGeral;
        if (qualidade >= 0 && qualidade != QualitySettings.GetQualityLevel())
            QualitySettings.SetQualityLevel(qualidade, true);
    }

    private static void AplicarNaCena()
    {
        foreach (EntradaJogador entrada in UnityEngine.Object.FindObjectsByType<EntradaJogador>())
        {
            entrada.MultiplicadorSensibilidade = sensibilidade;
            entrada.InverterY = inverterY;
        }

        foreach (CameraJogador camera in UnityEngine.Object.FindObjectsByType<CameraJogador>())
        {
            camera.FovBase = fov;
            camera.BalancoAtivo = balancoCamera;
        }
    }
}
