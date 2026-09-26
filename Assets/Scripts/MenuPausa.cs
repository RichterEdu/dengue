using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/* ============================================================
 *  MenuPausa.cs
 *  ---------------------------------------------------------------------
 *  Menu de pausa: Esc (ou Start no controle) abre e fecha.
 *
 *  - PAUSA DE VERDADE: Time.timeScale = 0 (a ocupação do hospital para),
 *    cursor solto, jogador sem andar/olhar, sem mirar em foco e sem abrir
 *    o celular. Ao fechar, tudo volta como estava.
 *  - Não abre durante as cartelas (lá o Esc pula a história) e fecha
 *    sozinho se o epílogo começar. Com o celular levantado, o Esc
 *    primeiro guarda o celular.
 *  - Telas: Menu (Continuar / Configurações / Reiniciar do zero /
 *    Menu inicial / Sair), Configurações (PainelConfiguracoes.cs, a mesma
 *    do menu inicial) e Confirmação (antes de reiniciar, voltar ao menu
 *    inicial ou sair). Esc / B no controle = voltar uma tela.
 *  - Mouse e controle: o item sob o mouse é o item selecionado
 *    (ItemDeMenu.cs), então os dois usam o mesmo destaque.
 *
 *  A hierarquia é gerada pelo menu  Dengue > Criar Menu de Pausa
 *  (Assets/Editor/MenuPausaBuilder.cs), que já liga as referências.
 * ============================================================ */

public class MenuPausa : MonoBehaviour
{
    [Header("Jogo (vazio = procura na cena)")]
    [SerializeField] private FluxoDeJogo fluxo;
    [SerializeField] private ControladorJogador jogador;
    [SerializeField] private Interactor interactor;
    [SerializeField] private CelularController celular;
    [SerializeField] private CartelasNarrativas cartelas;

    [Header("Telas")]
    [SerializeField] private CanvasGroup grupo;
    [SerializeField] private RectTransform telaMenu;
    [SerializeField] private RectTransform telaConfiguracoes;
    [SerializeField] private RectTransform telaConfirmacao;
    [Tooltip("Os cartões (caixas) das telas — encolhem se a janela for baixa demais.")]
    [SerializeField] private RectTransform[] cartoes;
    [SerializeField] private float velocidadeFade = 10f;

    [Header("Menu")]
    [SerializeField] private TextMeshProUGUI rotuloMenu;
    [SerializeField] private TextMeshProUGUI statusMenu;
    [SerializeField] private Button botaoContinuar;
    [SerializeField] private Button botaoConfiguracoes;
    [SerializeField] private Button botaoReiniciar;
    [SerializeField] private Button botaoMenuInicial;
    [SerializeField] private Button botaoSair;

    [Header("Confirmação")]
    [SerializeField] private TextMeshProUGUI tituloConfirmacao;
    [SerializeField] private TextMeshProUGUI corpoConfirmacao;
    [SerializeField] private TextMeshProUGUI textoConfirmar;
    [SerializeField] private Button botaoCancelar;
    [SerializeField] private Button botaoConfirmar;

    [Header("Configurações")]
    [SerializeField] private PainelConfiguracoes configuracoes;

    [Header("Cores do status")]
    [SerializeField] private Color corOcupacaoBaixa = new Color(0.14f, 0.16f, 0.19f, 1f);
    [SerializeField] private Color corOcupacaoAlta = new Color(0.82f, 0.22f, 0.22f, 1f);
    [SerializeField] private Color corFocosEliminados = new Color(0.11f, 0.13f, 0.15f, 1f);

    private enum Tela { Nenhuma, Menu, Configuracoes, Confirmacao }

    private Tela tela = Tela.Nenhuma;
    private RectTransform raizTelaAtual;
    private GameObject focoPadrao;
    private GameObject ultimoSelecionado;
    private Action acaoConfirmada;
    private Selectable voltarPara;

    // Estado do jogo antes de abrir (restaurado ao fechar).
    private float escalaTempoAntes = 1f;
    private bool controleAntes = true;
    private bool interacaoBloqueadaAntes;
    private bool celularLigadoAntes = true;
    private bool restaurarNoFimDoFrame;

    public bool Aberto { get; private set; }

    void Awake()
    {
        if (fluxo == null) fluxo = FindAnyObjectByType<FluxoDeJogo>();
        if (jogador == null) jogador = FindAnyObjectByType<ControladorJogador>();
        if (interactor == null) interactor = FindAnyObjectByType<Interactor>();
        if (celular == null) celular = FindAnyObjectByType<CelularController>();
        if (cartelas == null) cartelas = FindAnyObjectByType<CartelasNarrativas>();
        if (grupo == null) grupo = GetComponent<CanvasGroup>();

        if (configuracoes != null) configuracoes.Preparar();
        OcultarMenuInicialSeIndisponivel();
        LigarEventos();
        EsconderImediato();
    }

    /* ---------------- Entrada ---------------- */

    void Update()
    {
        Keyboard teclado = Keyboard.current;
        Gamepad controle = Gamepad.current;
        bool esc = teclado != null && teclado.escapeKey.wasPressedThisFrame;
        bool start = controle != null && controle.startButton.wasPressedThisFrame;
        bool b = controle != null && controle.buttonEast.wasPressedThisFrame;

        if (!Aberto)
        {
            if (esc || start)
            {
                if (celular != null && celular.enabled && celular.Aberto)
                    celular.Fechar();
                else if (PodeAbrir())
                    Abrir();
            }
        }
        else if (cartelas != null && cartelas.Tocando)
        {
            Fechar();   // o epílogo começou por baixo do menu
        }
        else if (start)
        {
            Fechar();
        }
        else if (esc || b)
        {
            Voltar();
        }

        if (Aberto)
            GarantirSelecao();

        if (grupo != null)
            grupo.alpha = Mathf.MoveTowards(grupo.alpha, Aberto ? 1f : 0f, Time.unscaledDeltaTime * velocidadeFade);
    }

    /* Religa o jogo só no fim do frame: o clique/Espaço que fechou o menu
     * não vira clique num foco nem pulo neste mesmo frame. */
    void LateUpdate()
    {
        if (!restaurarNoFimDoFrame)
            return;
        restaurarNoFimDoFrame = false;

        if (jogador != null) jogador.ControleAtivo = controleAntes;
        if (interactor != null) interactor.Bloqueado = interacaoBloqueadaAntes;
        if (celular != null) celular.enabled = celularLigadoAntes;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnDestroy()
    {
        // timeScale é global: não deixar o jogo congelado se a cena sair com o menu aberto.
        if (Aberto)
            Time.timeScale = escalaTempoAntes;
    }

    private bool PodeAbrir()
    {
        return !(cartelas != null && cartelas.Tocando);
    }

    /* ---------------- Abrir / fechar ---------------- */

    public void Abrir()
    {
        if (Aberto)
            return;
        Aberto = true;
        restaurarNoFimDoFrame = false;

        escalaTempoAntes = Time.timeScale;
        Time.timeScale = 0f;

        if (jogador != null)
        {
            controleAntes = jogador.ControleAtivo;
            jogador.ControleAtivo = false;
        }
        if (interactor != null)
        {
            interacaoBloqueadaAntes = interactor.Bloqueado;
            interactor.Bloqueado = true;
        }
        if (celular != null)
        {
            celularLigadoAntes = celular.enabled;
            celular.enabled = false;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (grupo != null)
        {
            grupo.blocksRaycasts = true;
            grupo.interactable = true;
        }

        AtualizarStatus();
        MostrarTela(Tela.Menu, botaoContinuar);
    }

    public void Fechar()
    {
        if (!Aberto)
            return;
        Aberto = false;

        ConfiguracoesJogo.Salvar();
        Time.timeScale = escalaTempoAntes;
        restaurarNoFimDoFrame = true;

        if (grupo != null)
        {
            grupo.blocksRaycasts = false;
            grupo.interactable = false;
        }
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
        tela = Tela.Nenhuma;
    }

    private void EsconderImediato()
    {
        if (grupo != null)
        {
            grupo.alpha = 0f;
            grupo.blocksRaycasts = false;
            grupo.interactable = false;
        }
        if (telaConfiguracoes != null) telaConfiguracoes.gameObject.SetActive(false);
        if (telaConfirmacao != null) telaConfirmacao.gameObject.SetActive(false);
    }

    /* Esc / B: sobe uma tela; no menu, volta pro jogo. */
    private void Voltar()
    {
        switch (tela)
        {
            case Tela.Confirmacao:
                Cancelar();
                break;
            case Tela.Configuracoes:
                ConfiguracoesJogo.Salvar();
                MostrarTela(Tela.Menu, botaoConfiguracoes);
                break;
            default:
                Fechar();
                break;
        }
    }

    /* ---------------- Telas ---------------- */

    private void MostrarTela(Tela nova, Selectable foco)
    {
        tela = nova;

        // A confirmação abre POR CIMA do menu (o menu fica escurecido atrás).
        if (telaMenu != null) telaMenu.gameObject.SetActive(nova == Tela.Menu || nova == Tela.Confirmacao);
        if (telaConfiguracoes != null) telaConfiguracoes.gameObject.SetActive(nova == Tela.Configuracoes);
        if (telaConfirmacao != null) telaConfirmacao.gameObject.SetActive(nova == Tela.Confirmacao);

        raizTelaAtual = nova == Tela.Configuracoes ? telaConfiguracoes
                      : nova == Tela.Confirmacao ? telaConfirmacao
                      : telaMenu;

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

    /* Janela baixa demais (ex.: Game view larga): encolhe os cartões pra caberem. */
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

    private void AtualizarStatus()
    {
        if (fluxo == null)
            return;

        if (rotuloMenu != null)
        {
            rotuloMenu.text = !fluxo.PartidaIniciada ? "UPA VILA NOVA  ·  RECEPÇÃO"
                            : fluxo.PartidaEncerrada ? "VILA NOVA  ·  MISSÃO ENCERRADA"
                            : "VILA NOVA  ·  MISSÃO EM ANDAMENTO";
        }

        if (statusMenu != null)
        {
            int pct = Mathf.RoundToInt(fluxo.Ocupacao * 100f);
            string cor = ColorUtility.ToHtmlStringRGB(Color.Lerp(corOcupacaoBaixa, corOcupacaoAlta, fluxo.Ocupacao));
            string corFocos = ColorUtility.ToHtmlStringRGBA(corFocosEliminados);
            statusMenu.text = $"<color=#{corFocos}><b>{fluxo.FocosEliminados}</b></color>/{fluxo.FocosTotal} focos eliminados" +
                              $"    ·    Hospital <color=#{cor}><b>{pct}%</b></color>";
        }
    }

    /* ---------------- Botões ---------------- */

    private void LigarEventos()
    {
        Ligar(botaoContinuar, Fechar);
        Ligar(botaoConfiguracoes, () => MostrarTela(Tela.Configuracoes, configuracoes != null ? configuracoes.PrimeiroItem : null));
        Ligar(botaoReiniciar, () => PedirConfirmacao(
            "Reiniciar do zero?",
            "A partida volta para a abertura no hospital. Os focos eliminados e a ocupação atual serão perdidos.",
            "Reiniciar", botaoReiniciar, ReiniciarDoZero));
        Ligar(botaoMenuInicial, () => PedirConfirmacao(
            "Voltar ao menu inicial?",
            "A partida atual termina aqui. Os focos eliminados e a ocupação atual serão perdidos.",
            "Voltar ao menu", botaoMenuInicial, VoltarAoMenuInicial));
        Ligar(botaoSair, () => PedirConfirmacao(
            "Sair do jogo?",
            "O progresso desta partida não é salvo. Suas configurações continuam guardadas.",
            "Sair", botaoSair, SairDoJogo));

        Ligar(botaoCancelar, Cancelar);
        Ligar(botaoConfirmar, () => acaoConfirmada?.Invoke());

        if (configuracoes != null)
            Ligar(configuracoes.BotaoVoltar, Voltar);
    }

    private void Ligar(Button botao, UnityEngine.Events.UnityAction acao)
    {
        if (botao != null)
            botao.onClick.AddListener(acao);
    }

    private void PedirConfirmacao(string titulo, string corpo, string confirmar, Selectable origem, Action acao)
    {
        if (tituloConfirmacao != null) tituloConfirmacao.text = titulo;
        if (corpoConfirmacao != null) corpoConfirmacao.text = corpo;
        if (textoConfirmar != null) textoConfirmar.text = confirmar;
        acaoConfirmada = acao;
        voltarPara = origem;
        MostrarTela(Tela.Confirmacao, botaoCancelar);   // o padrão é o botão seguro
    }

    private void Cancelar()
    {
        acaoConfirmada = null;
        MostrarTela(Tela.Menu, voltarPara != null ? voltarPara : botaoContinuar);
    }

    private void ReiniciarDoZero()
    {
        ConfiguracoesJogo.Salvar();
        HistoriaDoJogo.EsquecerAbertura();
        Aberto = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void VoltarAoMenuInicial()
    {
        ConfiguracoesJogo.Salvar();
        HistoriaDoJogo.EsquecerAbertura();   // "Jogar" no menu toca a abertura de novo
        Aberto = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(MenuInicial.NomeCena);
    }

    /* Sem a cena do menu no build (ex.: Build Settings mexidas), o botão
     * some, os de baixo sobem e o cartão encolhe. */
    private void OcultarMenuInicialSeIndisponivel()
    {
        if (botaoMenuInicial == null || Application.CanStreamedLevelBeLoaded(MenuInicial.NomeCena))
            return;

        RectTransform botao = (RectTransform)botaoMenuInicial.transform;
        RectTransform cartao = botao.parent as RectTransform;
        Navigation nav = botaoMenuInicial.navigation;
        Selectable cima = nav.selectOnUp;
        Selectable baixo = nav.selectOnDown;

        float yBotao = botao.anchoredPosition.y;
        float altura = baixo != null && baixo.transform.parent == cartao
            ? yBotao - ((RectTransform)baixo.transform).anchoredPosition.y
            : botao.rect.height;

        botao.gameObject.SetActive(false);
        if (cartao != null)
        {
            foreach (RectTransform irmao in cartao)
                if (irmao != botao && irmao.anchorMin.y > 0.99f && irmao.anchoredPosition.y < yBotao)
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
        botaoMenuInicial = null;
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
}
