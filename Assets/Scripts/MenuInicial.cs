using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/* ============================================================
 *  MenuInicial.cs
 *  ---------------------------------------------------------------------
 *  Tela de título do FOCO ZERO (cena MenuInicial, índice 0 do build).
 *
 *  - Telas: Título (Jogar / Como jogar / Configurações / Sair), Como
 *    jogar e Configurações (PainelConfiguracoes.cs — a mesma do pause).
 *    Esc / B no controle = voltar uma tela.
 *  - Enquanto o jogador está no menu, a SampleScene (pesada) já vai sendo
 *    carregada por trás (allowSceneActivation = false). "Jogar" escurece
 *    a tela e só então ativa a cena — ela nasce com a tela preta das
 *    cartelas, então a troca é de preto para preto.
 *  - Mouse e controle: o item sob o mouse é o item selecionado
 *    (ItemDeMenu.cs), então os dois usam o mesmo destaque.
 *
 *  A cena inteira é gerada pelo menu  Dengue > Criar Menu Inicial
 *  (Assets/Editor/MenuInicialBuilder.cs), que já liga as referências.
 * ============================================================ */

public class MenuInicial : MonoBehaviour
{
    public const string NomeCena = "MenuInicial";
    public const string NomeCenaJogo = "SampleScene";

    [Header("Telas")]
    [SerializeField] private RectTransform telaTitulo;
    [SerializeField] private RectTransform telaComoJogar;
    [SerializeField] private RectTransform telaConfiguracoes;
    [SerializeField] private PainelConfiguracoes configuracoes;
    [Tooltip("Os cartões (caixas) das telas — encolhem se a janela for baixa demais.")]
    [SerializeField] private RectTransform[] cartoes;

    [Header("Título")]
    [SerializeField] private Button botaoJogar;
    [SerializeField] private Button botaoComoJogar;
    [SerializeField] private Button botaoConfiguracoes;
    [SerializeField] private Button botaoSair;

    [Header("Como jogar")]
    [SerializeField] private Button botaoVoltarComoJogar;

    [Header("Transição")]
    [Tooltip("Tela preta por cima de tudo: clareia ao abrir o menu e escurece ao Jogar.")]
    [SerializeField] private CanvasGroup fade;
    [SerializeField] private float duracaoEntrada = 1.2f;
    [SerializeField] private float duracaoSaida = 0.5f;
    [SerializeField] private TextMeshProUGUI textoCarregando;

    private enum Tela { Titulo, ComoJogar, Configuracoes }

    private Tela tela = Tela.Titulo;
    private RectTransform raizTelaAtual;
    private GameObject focoPadrao;
    private GameObject ultimoSelecionado;
    private AsyncOperation carregamento;
    private bool saindo;

    void Awake()
    {
        // Vindo do jogo (pause > Menu inicial), o tempo e o cursor podem estar travados.
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (configuracoes != null) configuracoes.Preparar();
        LigarEventos();

        if (fade != null)
        {
            fade.alpha = 1f;
            fade.blocksRaycasts = false;
        }
        if (textoCarregando != null)
            textoCarregando.gameObject.SetActive(false);
    }

    void Start()
    {
        MostrarTela(Tela.Titulo, botaoJogar);
        StartCoroutine(PreCarregarJogo());
    }

    void Update()
    {
        if (saindo)
            return;

        if (fade != null)
            fade.alpha = Mathf.MoveTowards(fade.alpha, 0f, Time.unscaledDeltaTime / Mathf.Max(0.01f, duracaoEntrada));

        Keyboard teclado = Keyboard.current;
        Gamepad controle = Gamepad.current;
        bool esc = teclado != null && teclado.escapeKey.wasPressedThisFrame;
        bool b = controle != null && controle.buttonEast.wasPressedThisFrame;
        if (esc || b)
            Voltar();

        GarantirSelecao();
    }

    /* ---------------- Carregamento do jogo ---------------- */

    private IEnumerator PreCarregarJogo()
    {
        yield return null;   // deixa o primeiro frame do menu aparecer antes
        IniciarCarregamento();
    }

    private void IniciarCarregamento()
    {
        if (carregamento != null)
            return;
        if (!Application.CanStreamedLevelBeLoaded(NomeCenaJogo))
        {
            Debug.LogError("[Menu Inicial] A cena '" + NomeCenaJogo + "' não está no Build Settings.");
            return;
        }
        carregamento = SceneManager.LoadSceneAsync(NomeCenaJogo);
        carregamento.allowSceneActivation = false;
    }

    private void Jogar()
    {
        if (saindo)
            return;
        saindo = true;
        ConfiguracoesJogo.Salvar();
        StartCoroutine(EntrarNoJogo());
    }

    private IEnumerator EntrarNoJogo()
    {
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
        if (fade != null)
        {
            fade.blocksRaycasts = true;   // nenhum clique passa durante a saída
            while (fade.alpha < 1f)
            {
                fade.alpha = Mathf.MoveTowards(fade.alpha, 1f, Time.unscaledDeltaTime / Mathf.Max(0.01f, duracaoSaida));
                yield return null;
            }
        }

        IniciarCarregamento();
        if (carregamento == null)
            yield break;

        // Com allowSceneActivation = false o progresso para em 0,9 (= pronto).
        while (carregamento.progress < 0.9f)
        {
            if (textoCarregando != null)
            {
                textoCarregando.gameObject.SetActive(true);
                textoCarregando.text = "Carregando…  " + Mathf.RoundToInt(carregamento.progress / 0.9f * 100f) + "%";
            }
            yield return null;
        }
        carregamento.allowSceneActivation = true;
    }

    private void SairDoJogo()
    {
        ConfiguracoesJogo.Salvar();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /* ---------------- Telas ---------------- */

    private void LigarEventos()
    {
        Ligar(botaoJogar, Jogar);
        Ligar(botaoComoJogar, () => MostrarTela(Tela.ComoJogar, botaoVoltarComoJogar));
        Ligar(botaoConfiguracoes, () => MostrarTela(Tela.Configuracoes, configuracoes != null ? configuracoes.PrimeiroItem : null));
        Ligar(botaoSair, SairDoJogo);
        Ligar(botaoVoltarComoJogar, Voltar);
        if (configuracoes != null)
            Ligar(configuracoes.BotaoVoltar, Voltar);
    }

    private static void Ligar(Button botao, UnityEngine.Events.UnityAction acao)
    {
        if (botao != null)
            botao.onClick.AddListener(acao);
    }

    /* Esc / B: das telas internas volta ao título (no título, nada). */
    private void Voltar()
    {
        switch (tela)
        {
            case Tela.Configuracoes:
                ConfiguracoesJogo.Salvar();
                MostrarTela(Tela.Titulo, botaoConfiguracoes);
                break;
            case Tela.ComoJogar:
                MostrarTela(Tela.Titulo, botaoComoJogar);
                break;
        }
    }

    private void MostrarTela(Tela nova, Selectable foco)
    {
        tela = nova;

        if (telaTitulo != null) telaTitulo.gameObject.SetActive(nova == Tela.Titulo);
        if (telaComoJogar != null) telaComoJogar.gameObject.SetActive(nova == Tela.ComoJogar);
        if (telaConfiguracoes != null) telaConfiguracoes.gameObject.SetActive(nova == Tela.Configuracoes);

        raizTelaAtual = nova == Tela.ComoJogar ? telaComoJogar
                      : nova == Tela.Configuracoes ? telaConfiguracoes
                      : telaTitulo;

        if (nova == Tela.Configuracoes && configuracoes != null)
            configuracoes.Mostrar();

        AjustarAoTamanhoDaTela();

        focoPadrao = foco != null ? foco.gameObject : null;
        ultimoSelecionado = null;
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(focoPadrao);
    }

    /* Mantém sempre um item da tela atual selecionado: clicar no vazio
     * desmarcaria tudo e o controle ficaria sem ter onde navegar. */
    private void GarantirSelecao()
    {
        EventSystem es = EventSystem.current;
        if (es == null || raizTelaAtual == null)
            return;

        GameObject atual = es.currentSelectedGameObject;
        if (atual != null && atual.activeInHierarchy && atual.transform.IsChildOf(raizTelaAtual))
        {
            ultimoSelecionado = atual;
            return;
        }

        GameObject alvo = ultimoSelecionado != null && ultimoSelecionado.activeInHierarchy ? ultimoSelecionado : focoPadrao;
        if (alvo != null && alvo != atual)
            es.SetSelectedGameObject(alvo);
    }

    /* Janela baixa demais: encolhe os cartões pra caberem. */
    private void AjustarAoTamanhoDaTela()
    {
        RectTransform canvasRt = transform as RectTransform;
        if (canvasRt == null || cartoes == null)
            return;

        float alturaDisponivel = canvasRt.rect.height - 48f;
        foreach (RectTransform cartao in cartoes)
        {
            if (cartao == null)
                continue;
            float escala = Mathf.Min(1f, alturaDisponivel / Mathf.Max(1f, cartao.rect.height));
            cartao.localScale = new Vector3(escala, escala, 1f);
        }
    }
}
