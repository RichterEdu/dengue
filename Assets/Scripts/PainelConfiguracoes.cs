using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/* ============================================================
 *  PainelConfiguracoes.cs
 *  ---------------------------------------------------------------------
 *  Tela de Configurações (áudio, controles, vídeo) — a MESMA no menu de
 *  pausa (MenuPausa.cs) e no menu inicial (MenuInicial.cs). Liga cada
 *  slider/chave a ConfiguracoesJogo.cs, que aplica e grava em PlayerPrefs.
 *
 *  Quem abre a tela chama Mostrar() (recarrega os valores) e cuida do
 *  botão Voltar (BotaoVoltar) — o painel só cuida das próprias linhas e do
 *  "Restaurar padrão".
 *
 *  A hierarquia é gerada por MenuPausaBuilder.MontarConfiguracoes.
 * ============================================================ */

public class PainelConfiguracoes : MonoBehaviour
{
    [SerializeField] private Slider sliderVolumeGeral;
    [SerializeField] private TextMeshProUGUI valorVolumeGeral;
    [SerializeField] private Slider sliderMusica;
    [SerializeField] private TextMeshProUGUI valorMusica;
    [SerializeField] private Slider sliderEfeitos;
    [SerializeField] private TextMeshProUGUI valorEfeitos;
    [SerializeField] private Slider sliderSensibilidade;
    [SerializeField] private TextMeshProUGUI valorSensibilidade;
    [SerializeField] private Toggle chaveInverterY;
    [SerializeField] private Slider sliderFov;
    [SerializeField] private TextMeshProUGUI valorFov;
    [SerializeField] private Toggle chaveBalanco;
    [Tooltip("Slider sem trilho: só guarda o índice e responde a esquerda/direita no controle.")]
    [SerializeField] private Slider seletorQualidade;
    [SerializeField] private TextMeshProUGUI valorQualidade;
    [SerializeField] private Button qualidadeAnterior;
    [SerializeField] private Button qualidadeProxima;
    [SerializeField] private Toggle chaveTelaCheia;
    [SerializeField] private Button botaoRestaurar;
    [SerializeField] private Button botaoVoltar;

    /* Passos dos sliders (o controle anda 1 passo por toque). */
    private const float PassoVolume = 0.05f;        // 5%
    private const float PassoSensibilidade = 0.1f;  // 0,1×
    private const float PassoFov = 5f;              // 5°

    private bool atualizandoTela;
    private bool preparado;

    /* Item selecionado ao abrir a tela. */
    public Selectable PrimeiroItem => sliderVolumeGeral;
    public Button BotaoVoltar => botaoVoltar;

    void Awake()
    {
        Preparar();
    }

    /* Pode ser chamado antes do Awake (o painel começa desativado). */
    public void Preparar()
    {
        if (preparado)
            return;
        preparado = true;

        ConfigurarSliders();
        OcultarQualidadeSeUnica();
        LigarEventos();
    }

    public void Mostrar()
    {
        Preparar();
        CarregarValores();
    }

    private void LigarEventos()
    {
        Ligar(botaoRestaurar, () =>
        {
            ConfiguracoesJogo.RestaurarPadrao();
            CarregarValores();
        });
        Ligar(qualidadeAnterior, () => MudarQualidade(-1));
        Ligar(qualidadeProxima, () => MudarQualidade(+1));

        LigarSlider(sliderVolumeGeral, v => ConfiguracoesJogo.VolumeGeral = v * PassoVolume);
        LigarSlider(sliderMusica, v => ConfiguracoesJogo.VolumeMusica = v * PassoVolume);
        LigarSlider(sliderEfeitos, v => ConfiguracoesJogo.VolumeEfeitos = v * PassoVolume);
        LigarSlider(sliderSensibilidade, v => ConfiguracoesJogo.Sensibilidade = v * PassoSensibilidade);
        LigarSlider(sliderFov, v => ConfiguracoesJogo.Fov = v * PassoFov);
        LigarSlider(seletorQualidade, v => ConfiguracoesJogo.Qualidade = Mathf.RoundToInt(v));

        LigarChave(chaveInverterY, v => ConfiguracoesJogo.InverterY = v);
        LigarChave(chaveBalanco, v => ConfiguracoesJogo.BalancoCamera = v);
        LigarChave(chaveTelaCheia, v => ConfiguracoesJogo.TelaCheia = v);
    }

    private static void Ligar(Button botao, UnityEngine.Events.UnityAction acao)
    {
        if (botao != null)
            botao.onClick.AddListener(acao);
    }

    private void LigarSlider(Slider slider, System.Action<float> aoMudar)
    {
        if (slider == null)
            return;
        slider.onValueChanged.AddListener(v =>
        {
            if (atualizandoTela)
                return;
            aoMudar(v);
            AtualizarTextos();
        });
    }

    private void LigarChave(Toggle chave, System.Action<bool> aoMudar)
    {
        if (chave == null)
            return;
        chave.onValueChanged.AddListener(v =>
        {
            if (!atualizandoTela)
                aoMudar(v);
        });
    }

    private void ConfigurarSliders()
    {
        Faixa(sliderVolumeGeral, 0f, 1f / PassoVolume);
        Faixa(sliderMusica, 0f, 1f / PassoVolume);
        Faixa(sliderEfeitos, 0f, 1f / PassoVolume);
        Faixa(sliderSensibilidade, ConfiguracoesJogo.SensibilidadeMin / PassoSensibilidade, ConfiguracoesJogo.SensibilidadeMax / PassoSensibilidade);
        Faixa(sliderFov, ConfiguracoesJogo.FovMin / PassoFov, ConfiguracoesJogo.FovMax / PassoFov);
        Faixa(seletorQualidade, 0f, Mathf.Max(0, QualitySettings.count - 1));
    }

    /* No build de PC o projeto hoje só tem o nível "PC" (o "Mobile" é
     * excluído do Standalone) — no Editor fora do Play aparecem os dois,
     * por isso a decisão é aqui e não no builder. Com um nível só, a linha
     * some, as de baixo sobem e o cartão encolhe. */
    private void OcultarQualidadeSeUnica()
    {
        if (seletorQualidade == null || QualitySettings.count > 1)
            return;

        RectTransform linha = (RectTransform)seletorQualidade.transform;
        RectTransform cartao = linha.parent as RectTransform;
        Navigation nav = seletorQualidade.navigation;
        Selectable cima = nav.selectOnUp;
        Selectable baixo = nav.selectOnDown;

        // Altura a fechar = distância até a próxima linha (inclui o espaçamento).
        float yLinha = linha.anchoredPosition.y;
        float altura = baixo != null && baixo.transform.parent == cartao
            ? yLinha - ((RectTransform)baixo.transform).anchoredPosition.y
            : linha.rect.height;

        linha.gameObject.SetActive(false);
        if (cartao != null)
        {
            foreach (RectTransform irmao in cartao)
                if (irmao != linha && irmao.anchorMin.y > 0.99f && irmao.anchoredPosition.y < yLinha)
                    irmao.anchoredPosition += new Vector2(0f, altura);
            cartao.sizeDelta -= new Vector2(0f, altura);
        }

        if (cima != null)
        {
            Navigation n = cima.navigation;
            n.selectOnDown = baixo;
            cima.navigation = n;
        }
        if (baixo != null)
        {
            Navigation n = baixo.navigation;
            n.selectOnUp = cima;
            baixo.navigation = n;
        }
        seletorQualidade = null;
    }

    private static void Faixa(Slider slider, float min, float max)
    {
        if (slider == null)
            return;
        slider.wholeNumbers = true;
        slider.minValue = Mathf.Round(min);
        slider.maxValue = Mathf.Round(max);
    }

    private void CarregarValores()
    {
        atualizandoTela = true;
        Definir(sliderVolumeGeral, ConfiguracoesJogo.VolumeGeral / PassoVolume);
        Definir(sliderMusica, ConfiguracoesJogo.VolumeMusica / PassoVolume);
        Definir(sliderEfeitos, ConfiguracoesJogo.VolumeEfeitos / PassoVolume);
        Definir(sliderSensibilidade, ConfiguracoesJogo.Sensibilidade / PassoSensibilidade);
        Definir(sliderFov, ConfiguracoesJogo.Fov / PassoFov);
        Definir(seletorQualidade, ConfiguracoesJogo.Qualidade);
        if (chaveInverterY != null) chaveInverterY.SetIsOnWithoutNotify(ConfiguracoesJogo.InverterY);
        if (chaveBalanco != null) chaveBalanco.SetIsOnWithoutNotify(ConfiguracoesJogo.BalancoCamera);
        if (chaveTelaCheia != null) chaveTelaCheia.SetIsOnWithoutNotify(ConfiguracoesJogo.TelaCheia);
        atualizandoTela = false;
        AtualizarTextos();
    }

    private static void Definir(Slider slider, float valor)
    {
        if (slider != null)
            slider.SetValueWithoutNotify(Mathf.Round(valor));
    }

    private void MudarQualidade(int passo)
    {
        if (seletorQualidade == null)
            return;
        seletorQualidade.value = Mathf.Clamp(seletorQualidade.value + passo, seletorQualidade.minValue, seletorQualidade.maxValue);
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(seletorQualidade.gameObject);
    }

    private void AtualizarTextos()
    {
        Texto(valorVolumeGeral, Porcentagem(ConfiguracoesJogo.VolumeGeral));
        Texto(valorMusica, Porcentagem(ConfiguracoesJogo.VolumeMusica));
        Texto(valorEfeitos, Porcentagem(ConfiguracoesJogo.VolumeEfeitos));
        Texto(valorSensibilidade, ConfiguracoesJogo.Sensibilidade.ToString("0.0", CultureInfo.GetCultureInfo("pt-BR")) + "×");
        Texto(valorFov, Mathf.RoundToInt(ConfiguracoesJogo.Fov) + "°");
        Texto(valorQualidade, NomeQualidade(ConfiguracoesJogo.Qualidade));
    }

    private static string Porcentagem(float v) => Mathf.RoundToInt(v * 100f) + "%";

    private static void Texto(TextMeshProUGUI alvo, string texto)
    {
        if (alvo != null)
            alvo.text = texto;
    }

    /* Nomes dos níveis do projeto em português (Mobile/PC viram Baixa/Alta). */
    private static string NomeQualidade(int indice)
    {
        string[] nomes = QualitySettings.names;
        if (indice < 0 || indice >= nomes.Length)
            return "-";

        switch (nomes[indice].ToLowerInvariant())
        {
            case "very low": return "Muito baixa";
            case "low":
            case "mobile": return "Baixa";
            case "medium": return "Média";
            case "high":
            case "pc": return "Alta";
            case "very high": return "Muito alta";
            default: return nomes[indice];
        }
    }
}
