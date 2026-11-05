using System.Collections.Generic;
using TMPro;
using UnityEngine;

/* ============================================================
 *  DengueHUD.cs
 *  ---------------------------------------------------------------------
 *  HUD em primeira pessoa do jogo (uGUI + TextMeshPro).
 *
 *  - MIRA: ponto no centro. Cresce e muda de cor ao mirar num foco (ou
 *    num ponto de uso: picape, monte de areia, caçamba).
 *  - PAINEL DO FOCO: texto ao mirar, a linha de ação e a barra contínua
 *    do segurar (Interactor.ProgressoContinuo) com os cliques (3/5). No
 *    ponto de uso: o TextoPara(kit) dele e a barra, sem o 3/5. Pneu
 *    esvaziado no chão: "Pneu vazio: leve à picape da UPA", sem barra
 *    (pegar de novo é na hora).
 *    - No foco, o texto é só o nome, o mesmo da lista de focos; o estado
 *      e o que fazer ficam na linha de ação.
 *    - A linha de ação, embaixo, é por padrão o "segure o clique". Num
 *      foco esvaziado, vira "Segure com a <ferramenta> para resolver de
 *      vez" (o artigo concorda: "com o saco de lixo"); com as Mãos no atalho de esvaziar, "Segure para esvaziar (a
 *      chuva enche de novo)", e o 3/5 conta os passos de esvaziar. Uma
 *      linha só: o "segure" nunca aparece duas vezes.
 *  - AVISO: ao eliminar um foco, mostra a mensagem de conclusão e, numa
 *    segunda linha, a DICA educativa do foco (fica mais tempo na tela).
 *    Também explica as recusas do kit (Interactor.OnRecusado: ferramenta
 *    errada, sem carga, saco cheio, mãos ocupadas, já vazio) e dá o
 *    motivo da recusa de um ponto de uso (OnPontoRecusado). Ao esvaziar
 *    um foco (OnFocoEsvaziado do gerenciador): "Esvaziado, por enquanto.
 *    A chuva enche de novo." (no pneu, "Pneu vazio: leve à picape da
 *    UPA", o próximo passo). Ao chegar um pneu na caçamba da picape
 *    (Cacamba.OnPneuRecebido, de todas as caçambas da cena): "Pneu
 *    recolhido — N de T" (T = os pneus da partida), com a dica do pneu
 *    embaixo. Pneu com vaga na carroceria não mostra o aviso de eliminado
 *    antes: o texto trocaria no meio da leitura, e ele conta na entrega.
 *  - CONTADOR: canto superior esquerdo, "Focos eliminados 2/8".
 *    Os focos vêm do GerenciadorDeFocos (do campo ou, vazio,
 *    GerenciadorDeFocos.Obter()) e cada eliminação, do OnFocoEliminado
 *    dele. Sem gerenciador, o contador fica em 0/0.
 *  - MENSAGEM FINAL quando todos os focos são eliminados.
 *
 *  TEXTOS POR FOCO: adicione o componente FocoHUDInfo no foco.
 *  Tudo que não for preenchido lá usa os "Textos padrão" daqui.
 *
 *  A hierarquia é gerada pelo menu  Dengue > Criar HUD
 *  (Assets/Editor/DengueHUDBuilder.cs), que já liga as referências.
 * ============================================================ */

public class DengueHUD : MonoBehaviour
{
    [Header("Textos padrão")]
    [Tooltip("Usado quando o foco não tem FocoHUDInfo (mostra o problema, não a solução). Marcadores: {nome} {cliques} {total} {faltam}")]
    [TextArea(1, 3)]
    [SerializeField] private string textoAoMirar = "{nome}";

    [Tooltip("Linha menor acrescentada embaixo do texto ao mirar (foco ou ponto de uso): como agir. Num foco esvaziado ou no atalho de esvaziar, dá lugar à ação, no mesmo estilo. Vazio = sem a linha.")]
    [TextArea(1, 3)]
    [SerializeField] private string linhaSegurar = "<size=70%><color=#FFFFFFB4>segure o clique</color></size>";

    [Tooltip("Aviso quando um foco é eliminado. Marcadores: {nome} {eliminados} {total} {restantes}")]
    [TextArea(1, 3)]
    [SerializeField] private string textoAoEliminar = "<b>Foco eliminado!</b>";

    [Tooltip("Título do contador.")]
    [SerializeField] private string tituloContador = "FOCOS ELIMINADOS";

    [Tooltip("Formato do contador. Marcadores: {eliminados} {total} {restantes}")]
    [TextArea(1, 3)]
    [SerializeField] private string formatoContador = "{eliminados}<size=60%><color=#FFFFFF99>/{total}</color></size>";

    [Tooltip("Mensagem quando todos os focos são eliminados.")]
    [TextArea(1, 3)]
    [SerializeField] private string mensagemFinal = "Todos os focos foram eliminados!";

    [Header("Dica educativa")]
    [Tooltip("Mostra a dica do foco no aviso, DEPOIS que ele é eliminado.")]
    [SerializeField] private bool mostrarDicaAoEliminar = true;

    [Tooltip("Mostra a dica no painel enquanto o jogador está só mirando (antes de limpar).")]
    [SerializeField] private bool mostrarDicaAoMirar = false;

    [Tooltip("Duração do aviso quando ele vem com dica (segundos). O aviso sem dica usa a duração normal.")]
    [SerializeField] private float duracaoAvisoComDica = 3f;

    [Tooltip("Como a dica aparece dentro do aviso. {dica} é o texto do foco.")]
    [TextArea(1, 3)]
    [SerializeField] private string formatoDicaNoAviso = "<size=70%><color=#FFFFFFB4><i>{dica}</i></color></size>";

    [Tooltip("Dica usada quando o foco não tem FocoHUDInfo (ou está com a dica vazia). Deixe vazio para não mostrar nada.")]
    [TextArea(2, 4)]
    [SerializeField] private string dicaPadrao = "Água parada de uma semana já vira mosquito. Vistorie o quintal todo fim de semana.";

    [Header("Controle automático")]
    [Tooltip("Desmarque para o script NÃO mexer no tamanho/cor da mira (você controla pelo Image e pelo RectTransform).")]
    [SerializeField] private bool animarMira = true;

    [Tooltip("Desmarque para manter a cor que você escolheu no Image do ProgressFill.")]
    [SerializeField] private bool controlarCorDaBarra = true;

    [Tooltip("Desmarque para usar o texto digitado no CounterLabel e no CompleteText, em vez dos campos acima.")]
    [SerializeField] private bool aplicarTextosFixos = true;

    [Header("Referências do jogo")]
    [Tooltip("Interactor do player. Se vazio, procura na cena.")]
    [SerializeField] private Interactor interactor;

    [Tooltip("Focos da partida (contador e aviso de eliminado). Vazio = GerenciadorDeFocos.Obter().")]
    [SerializeField] private GerenciadorDeFocos gerenciador;

    [Header("Mira")]
    [SerializeField] private RectTransform crosshair;
    [SerializeField] private UnityEngine.UI.Image crosshairImage;
    [SerializeField] private Color crosshairNormal = new Color(1f, 1f, 1f, 0.85f);
    [SerializeField] private Color crosshairTarget = new Color(1f, 0.85f, 0.2f, 1f);
    [SerializeField] private float crosshairNormalSize = 8f;
    [SerializeField] private float crosshairTargetSize = 18f;

    [Header("Painel do foco mirado")]
    [SerializeField] private CanvasGroup targetGroup;
    [SerializeField] private TextMeshProUGUI promptText;
    [Tooltip("Linha com a barra + 3/5 (é escondida quando o foco pede e no pneu esvaziado no chão). Vazio = esconde a barra (pai do ProgressFill) e o 3/5, cada um.")]
    [SerializeField] private GameObject progressRow;
    [Tooltip("Preenchimento da barra (o anchorMax.x vai de 0 a 1).")]
    [SerializeField] private RectTransform progressFill;
    [SerializeField] private UnityEngine.UI.Image progressFillImage;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private TextMeshProUGUI hintText;
    [SerializeField] private Color corPadraoDaBarra = new Color(0.25f, 1f, 0.8f, 1f);

    [Header("Aviso de foco eliminado")]
    [SerializeField] private CanvasGroup toastGroup;
    [SerializeField] private TextMeshProUGUI toastText;
    [Tooltip("Quanto tempo o aviso fica na tela (segundos).")]
    [SerializeField] private float duracaoAviso = 1f;

    [Header("Contador de focos")]
    [SerializeField] private TextMeshProUGUI counterLabel;
    [SerializeField] private TextMeshProUGUI counterText;
    [SerializeField] private RectTransform counterPanel;

    [Header("Mensagem final")]
    [SerializeField] private CanvasGroup completeGroup;
    [SerializeField] private TextMeshProUGUI completeText;

    [Header("Animação")]
    [Tooltip("Velocidade das transições (mira, fade, barra).")]
    [SerializeField] private float smoothSpeed = 14f;

    // Kit do agente: o pneu esvaziado no chão e as recusas (aviso com dica).
    // FerramentaErrada usa o FocoHUDInfo.textoFerramentaErrada e, vazio, o
    // padrão com o nome da ferramenta certa; SemCarga, o nome da necessária
    // (quando sai SemCarga, a selecionada já é a certa). A areia também
    // enche no monte de areia do terreno baldio; as outras, só na picape.
    // O saco esvazia nas lixeiras do bairro; o pneu vai para a caçamba da
    // picape da UPA. Os internal são conferidos pelos testes (SinaisTestes).
    internal const string TextoPneuVazio = "Pneu vazio: leve à <b>picape da UPA</b>";
    private const string RecusaFerramentaErrada = "Isso não resolve aqui. Tente: <b>{0}</b>.";
    private const string RecusaSemCarga = "Acabou: <b>{0}</b>. Reabasteça na picape da UPA.";
    private const string RecusaSemAreia = "Acabou: <b>{0}</b>. Reabasteça na picape da UPA ou no monte de areia do terreno baldio.";
    internal const string AvisoErrouSkillCheck = "A tela rasgou! Pegue outra na <b>picape da UPA</b>.";
    internal const string RecusaSacoCheio = "Saco cheio. Jogue numa <b>lixeira</b>.";
    internal const string RecusaMaosOcupadas = "Mãos ocupadas: leve o pneu à <b>picape da UPA</b> ou largue [botão direito].";

    // Atalho de esvaziar (Parte B): a
    // linha de ação no atalho e o aviso ao esvaziar.
    internal const string AcaoSkillCheck = "Clique para <b>encaixar a grelha</b>";
    internal const string AcaoEsvaziar = "Segure para <b>esvaziar</b> (a chuva enche de novo)";
    internal const string AvisoEsvaziado = "Esvaziado, por enquanto. A chuva enche de novo.";

    // A ação no lugar do "segure o clique", no mesmo estilo dele.
    private const string FormatoLinhaDeAcao = "<size=70%><color=#FFFFFFB4>{0}</color></size>";

    /* A ação num foco esvaziado: só a ferramenta certa resolve de vez. O
     * artigo concorda com a ferramenta ("com a areia", "com o saco de
     * lixo"); o negrito fica só no nome. */
    internal static string AcaoResolverDeVez(Ferramenta f)
    {
        return $"Segure com {Ferramentas.Artigo(f)} <b>{Ferramentas.Nome(f).ToLowerInvariant()}</b> para resolver de vez";
    }

    /* Recusa JaVazio: as Mãos num foco já esvaziado ("A areia", "O saco de
     * lixo" resolve de vez). */
    internal static string RecusaJaVazio(Ferramenta f)
    {
        return $"Já está vazio, mas a chuva enche de novo. {Ferramentas.Artigo(f, true)} <b>{Ferramentas.Nome(f).ToLowerInvariant()}</b> resolve de vez.";
    }

    /* O pneu com vaga na carroceria tem aviso próprio no fim do arco
     * (HandlePneuRecebido): o de eliminado não sai, para o texto não trocar
     * no meio da leitura. A vaga é reservada antes do Concluir. Sem vaga,
     * sai o de eliminado. */
    internal static bool EsperaAvisoDoPneu(Interactable foco)
    {
        return foco is PneuInteractable { VagaNaCacamba: not null };
    }

    /* Aviso ao esvaziar: no pneu, o próximo passo (levar à picape); nos
     * outros, que é por enquanto (a chuva enche de novo). */
    internal static string AvisoAoEsvaziar(Interactable foco)
    {
        return foco is PneuInteractable ? TextoPneuVazio : AvisoEsvaziado;
    }

    /* Aviso de pneu na caçamba da picape: o n-ésimo de 'total' pneus. */
    internal static string TextoPneuRecolhido(int n, int total)
    {
        return $"Pneu recolhido — {n} de {total}";
    }

    private readonly List<Interactable> focos = new List<Interactable>();
    private Cacamba[] cacambas;   // as assinadas no Start (para desassinar)
    private int eliminated;
    private float displayedFill;
    private Component lastTarget;   // foco ou ponto de uso
    private float counterPunch;   // 0..1, decai com o tempo
    private float toastTimer;
    private float toastDuracao;
    private Vector2 toastBasePos;

    void Start()
    {
        if (interactor == null)
            interactor = FindFirstObjectByType<Interactor>();

        // Recusas do kit (ferramenta errada, sem carga...): o aviso explica.
        if (interactor != null)
        {
            interactor.OnRecusado += HandleRecusado;
            interactor.OnPontoRecusado += HandlePontoRecusado;
            interactor.OnErrouSkillCheck += HandleErrouSkillCheck;
        }

        // Registra todos os focos da partida (inclusive desativados), vindos
        // do gerenciador.
        if (gerenciador == null) gerenciador = GerenciadorDeFocos.Obter();
        if (gerenciador != null)
        {
            gerenciador.Garantir();
            foreach (Interactable foco in gerenciador.Focos)
            {
                if (foco == null)
                    continue;
                focos.Add(foco);
                if (foco.IsComplete)
                    eliminated++;
            }
            gerenciador.OnFocoEliminado += HandleCompleted;
            gerenciador.OnFocoEsvaziado += HandleEsvaziado;
        }

        // Pneus na caçamba da picape: "Pneu recolhido — N de T". A Cacamba
        // não é Interactable (não entra na varredura dos focos).
        cacambas = FindObjectsByType<Cacamba>(FindObjectsInactive.Include);
        foreach (Cacamba cacamba in cacambas)
            cacamba.OnPneuRecebido += HandlePneuRecebido;

        if (targetGroup != null) targetGroup.alpha = 0f;
        if (completeGroup != null) completeGroup.alpha = 0f;
        if (toastGroup != null)
        {
            toastGroup.alpha = 0f;
            toastBasePos = ((RectTransform)toastGroup.transform).anchoredPosition;
        }

        // O aviso agora pode ter duas linhas (título + dica): sem isso o
        // texto sairia numa linha só, atravessando a tela.
        if (toastText != null)
            toastText.textWrappingMode = TextWrappingModes.Normal;

        toastDuracao = duracaoAviso;

        ApplyStaticTexts();
        UpdateCounter();
        AvisarReferenciasFaltando();
    }

    /* Referência vazia aqui é falha silenciosa: o aviso (e a dica junto
     * com ele) simplesmente não aparece, sem nenhum erro no Console.
     * Melhor gritar no Play do que o jogador achar que é bug do jogo. */
    private void AvisarReferenciasFaltando()
    {
        System.Text.StringBuilder faltando = new System.Text.StringBuilder();

        if (toastGroup == null || toastText == null)
            faltando.Append("\n  • ToastPanel/ToastText — o aviso de foco eliminado E a dica educativa não aparecem");
        if (promptText == null)
            faltando.Append("\n  • PromptText — o texto ao mirar não aparece");
        if (counterText == null)
            faltando.Append("\n  • CounterText — o contador de focos não atualiza");
        if (completeText == null)
            faltando.Append("\n  • CompleteText — a mensagem final não aparece");
        if (hintText == null && mostrarDicaAoMirar)
            faltando.Append("\n  • HintText — a dica ao mirar não aparece");

        if (faltando.Length > 0)
            Debug.LogWarning("[HUD] Faltam referências na DengueHUD:" + faltando +
                             "\nRode  Dengue > Consertar HUD (criar o que falta)  para criar o que sumiu e religar tudo.", this);
    }

    /* Atualiza os textos fixos no Editor assim que você muda no Inspector. */
    void OnValidate()
    {
        ApplyStaticTexts();
    }

    void OnDestroy()
    {
        // Só pelo campo: procurar ou criar um gerenciador aqui (fim do Play,
        // troca de cena) deixaria objeto novo para trás.
        if (gerenciador != null)
        {
            gerenciador.OnFocoEliminado -= HandleCompleted;
            gerenciador.OnFocoEsvaziado -= HandleEsvaziado;
        }

        if (cacambas != null)
        {
            foreach (Cacamba cacamba in cacambas)
            {
                if (cacamba != null)
                    cacamba.OnPneuRecebido -= HandlePneuRecebido;
            }
        }

        if (interactor != null)
        {
            interactor.OnRecusado -= HandleRecusado;
            interactor.OnPontoRecusado -= HandlePontoRecusado;
            interactor.OnErrouSkillCheck -= HandleErrouSkillCheck;
        }
    }

    void Update()
    {
        float k = 1f - Mathf.Exp(-smoothSpeed * Time.deltaTime);
        Interactable target = interactor != null ? interactor.CurrentTarget : null;
        PontoDeUso ponto = interactor != null ? interactor.PontoAlvo : null;

        // Durante o skill check, só o anel dele na tela: sem mira nem painel.
        bool emSkillCheck = interactor != null && interactor.SkillCheckAtivo != null;
        if (crosshair != null)
            Ativar(crosshair.gameObject, !emSkillCheck);

        if (emSkillCheck)
        {
            if (targetGroup != null)
                targetGroup.alpha = 0f;
        }
        else
        {
            UpdateCrosshair(target != null || ponto != null, k);
            UpdateTargetPanel(target, ponto, k);
        }
        UpdateToast();
        UpdateCounterPunch();

        if (completeGroup != null)
        {
            float goal = focos.Count > 0 && eliminated >= focos.Count ? 1f : 0f;
            completeGroup.alpha = Mathf.Lerp(completeGroup.alpha, goal, k * 0.5f);
        }
    }

    /* ---------------- Textos fixos ---------------- */

    private void ApplyStaticTexts()
    {
        if (!aplicarTextosFixos) return;
        if (counterLabel != null) counterLabel.text = tituloContador;
        if (completeText != null) completeText.text = mensagemFinal;
    }

    /* ---------------- Mira ---------------- */

    private void UpdateCrosshair(bool hasTarget, float k)
    {
        if (!animarMira)
            return;

        if (crosshair != null)
        {
            float goal = hasTarget ? crosshairTargetSize : crosshairNormalSize;
            float size = Mathf.Lerp(crosshair.sizeDelta.x, goal, k);
            crosshair.sizeDelta = new Vector2(size, size);
        }
        if (crosshairImage != null)
            crosshairImage.color = Color.Lerp(crosshairImage.color, hasTarget ? crosshairTarget : crosshairNormal, k);
    }

    /* ---------------- Painel do foco ---------------- */

    /* Painel do alvo da mira: um foco OU um ponto de uso (nunca os dois). */
    private void UpdateTargetPanel(Interactable target, PontoDeUso ponto, float k)
    {
        bool temAlvo = target != null || ponto != null;
        if (targetGroup != null)
            targetGroup.alpha = Mathf.Lerp(targetGroup.alpha, temAlvo ? 1f : 0f, k);

        // Sem alvo: mantém o último conteúdo enquanto some (fade-out suave).
        if (!temAlvo)
            return;

        Component alvo = target != null ? (Component)target : ponto;
        FocoHUDInfo info = target != null ? target.GetComponent<FocoHUDInfo>() : null;

        if (alvo != lastTarget)
        {
            lastTarget = alvo;
            displayedFill = Progress();

            if (progressFillImage != null && (controlarCorDaBarra || (info != null && info.usarCorPropria)))
                progressFillImage.color = info != null && info.usarCorPropria ? info.corDaBarra : corPadraoDaBarra;

            if (hintText != null)
            {
                // Por padrão a dica NÃO aparece aqui: ela é mostrada depois,
                // no aviso de foco eliminado, onde o jogador tem tempo de ler.
                // Ponto de uso não é criadouro: nunca tem dica.
                string dica = target != null && mostrarDicaAoMirar ? DicaDoFoco(target, info) : "";
                hintText.gameObject.SetActive(!string.IsNullOrWhiteSpace(dica));
                hintText.text = target != null ? FormatFoco(dica, target, info) : "";
            }
        }

        // Pneu esvaziado no chão: pegar de novo é na hora, a barra ficaria
        // cheia o tempo todo. Conferido a cada quadro, e não só na troca de
        // alvo: o mesmo pneu volta à mira depois de pego e largado.
        bool pneuVazio = target is PneuInteractable pneu && pneu.Esvaziado && !pneu.Carregado;
        MostrarProgresso(target == null || (!pneuVazio && !(info != null && info.esconderProgresso)));

        // Texto de mirar é refeito a cada frame porque pode usar {cliques}/{faltam}
        // (e o estágio, o esvaziado e a ferramenta selecionada mudam).
        KitDoAgente kit = interactor != null ? interactor.Kit : null;
        if (promptText != null)
            promptText.text = ComLinhaDeAcao(TextoDeMirar(target, ponto, info, kit),
                                             AcaoDoFoco(target, pneuVazio, kit));

        displayedFill = Mathf.Lerp(displayedFill, Progress(), k);
        if (progressFill != null)
            progressFill.anchorMax = new Vector2(displayedFill, 1f);
        if (progressText != null)
        {
            string contagem = "";
            if (target != null)
            {
                Contagem(target, kit, out int feitos, out int total);
                contagem = $"{feitos}/{total}";
            }
            progressText.text = contagem;
        }
    }

    /* Mostra ou esconde a linha da barra + 3/5. A HUD da cena hoje não tem
     * o ProgressRow (a barra e o 3/5 são filhos diretos do painel): sem
     * ele, esconde a barra (o pai do ProgressFill) e o 3/5, cada um. */
    private void MostrarProgresso(bool mostrar)
    {
        if (progressRow != null)
        {
            Ativar(progressRow, mostrar);
            return;
        }

        Transform barra = progressFill != null ? progressFill.parent : null;
        if (barra != null && (targetGroup == null || barra != targetGroup.transform))
            Ativar(barra.gameObject, mostrar);
        if (progressText != null)
            Ativar(progressText.gameObject, mostrar);
    }

    private static void Ativar(GameObject objeto, bool ativo)
    {
        if (objeto.activeSelf != ativo)
            objeto.SetActive(ativo);
    }

    /* Barra contínua: cliques já dados + o passo em andamento (no ponto de
     * uso, quanto do uso já foi segurado), calculada pelo Interactor. */
    private float Progress()
    {
        return interactor != null ? interactor.ProgressoContinuo : 0f;
    }

    /* Texto ao mirar: no ponto de uso, o dele para o kit do Interactor
     * (TextoPara: a picape muda com o pneu nas mãos); no foco, só o nome,
     * o mesmo da lista de focos (pedido do usuário). O estado e o que fazer
     * ficam na linha de ação, embaixo. */
    internal static string TextoDeMirar(Interactable foco, PontoDeUso ponto, FocoHUDInfo info, KitDoAgente kit)
    {
        if (foco == null)
            return ponto.TextoPara(kit);
        return FocoListaHUD.Categoria(foco, info).rotulo;
    }

    /* A ação no lugar do "segure o clique" (null = o "segure o clique"):
     * num foco esvaziado, resolver de vez com a ferramenta certa; com as
     * Mãos no atalho de esvaziar, esvaziar. O pneu esvaziado no chão diz
     * para onde levar. */
    private static string AcaoDoFoco(Interactable foco, bool pneuVazio, KitDoAgente kit)
    {
        if (foco == null)
            return null;
        if (pneuVazio)
            return TextoPneuVazio;
        if (foco.UsaSkillCheck)
            return AcaoSkillCheck;
        if (foco.Esvaziado)
            return AcaoResolverDeVez(foco.FerramentaNecessaria);
        if (foco.NoCaminhoDeEsvaziar(kit))
            return AcaoEsvaziar;
        return null;
    }

    /* Acrescenta a linha de ação embaixo do texto ao mirar: a ação do foco,
     * no estilo do "segure o clique", ou, sem ação, o próprio "segure o
     * clique". Uma linha só: o "segure" não se repete. */
    private string ComLinhaDeAcao(string texto, string acao)
    {
        string linha = acao != null ? string.Format(FormatoLinhaDeAcao, acao) : linhaSegurar;
        if (string.IsNullOrWhiteSpace(linha))
            return texto;
        return string.IsNullOrWhiteSpace(texto) ? linha : texto + "\n" + linha;
    }

    /* Os passos da barra (o 3/5 e os marcadores {cliques} {total} {faltam}):
     * no atalho de esvaziar, os passos de esvaziar; senão, os cliques. */
    private static void Contagem(Interactable foco, KitDoAgente kit, out int feitos, out int total)
    {
        if (!foco.Esvaziado && foco.NoCaminhoDeEsvaziar(kit))
        {
            feitos = foco.PassosEsvaziando;
            total = foco.PassosParaEsvaziar;
            return;
        }
        feitos = foco.CurrentClicks;
        total = foco.RequiredClicks;
    }

    /* Nome do foco: FocoHUDInfo.nome > padrão por tipo > nome do objeto. */
    private static string NomeDoFoco(Interactable foco, FocoHUDInfo info)
    {
        if (info != null && !string.IsNullOrWhiteSpace(info.nome)) return info.nome;
        if (foco is PneuInteractable) return "o pneu";
        if (foco is VasoInteractable) return "o pratinho do vaso";
        return foco.gameObject.name;
    }

    /* kit: o da mira (no atalho de esvaziar, os marcadores contam os passos
     * de esvaziar); null nos avisos, que contam os cliques. */
    private static string FormatFoco(string template, Interactable foco, FocoHUDInfo info, KitDoAgente kit = null)
    {
        if (string.IsNullOrEmpty(template))
            return "";
        Contagem(foco, kit, out int feitos, out int total);
        return template
            .Replace("{nome}", NomeDoFoco(foco, info))
            .Replace("{cliques}", feitos.ToString())
            .Replace("{total}", total.ToString())
            .Replace("{faltam}", Mathf.Max(0, total - feitos).ToString());
    }

    /* Dica do foco: FocoHUDInfo.dica > padrão por tipo > dicaPadrao.
     * O pneu e o vaso são anteriores ao FocoHUDInfo e não têm o
     * componente, por isso o padrão por tipo — assim todo foco do jogo
     * ensina alguma coisa ao ser eliminado, sem mexer nos prefabs. */
    private string DicaDoFoco(Interactable foco, FocoHUDInfo info)
    {
        if (info != null && !string.IsNullOrWhiteSpace(info.dica))
            return info.dica;
        if (foco is PneuInteractable)
            return "Pneu guarda água da chuva por meses. Fure, cubra ou guarde em lugar coberto.";
        if (foco is VasoInteractable)
            return "Pratinho de vaso: encha de areia até a borda. Sem água livre, não nasce larva.";
        return dicaPadrao;
    }

    private string FormatContagem(string template)
    {
        return template
            .Replace("{eliminados}", eliminated.ToString())
            .Replace("{total}", focos.Count.ToString())
            .Replace("{restantes}", Mathf.Max(0, focos.Count - eliminated).ToString());
    }

    /* ---------------- Aviso (toast) ---------------- */

    private void ShowToast(string message, bool comDica)
    {
        if (toastGroup == null || toastText == null || string.IsNullOrWhiteSpace(message))
            return;
        toastText.text = message;
        toastDuracao = comDica ? Mathf.Max(duracaoAviso, duracaoAvisoComDica) : duracaoAviso;
        toastTimer = toastDuracao;
    }

    private void UpdateToast()
    {
        if (toastGroup == null)
            return;

        toastTimer = Mathf.Max(0f, toastTimer - Time.deltaTime);
        float elapsed = toastDuracao - toastTimer;

        // Entra rápido, fica, e sai subindo — saída rápida (0.2s) para não
        // demorar na tela depois que o jogador já leu.
        float alpha = toastTimer <= 0f ? 0f : Mathf.Min(Mathf.Clamp01(elapsed / 0.15f), Mathf.Clamp01(toastTimer / 0.2f));
        toastGroup.alpha = alpha;

        float rise = toastTimer < 0.2f ? (1f - toastTimer / 0.2f) * 20f : 0f;
        ((RectTransform)toastGroup.transform).anchoredPosition = toastBasePos + new Vector2(0f, rise);
    }

    /* ---------------- Recusas do kit ---------------- */

    /* Recusa num foco: o aviso (com o tempo da dica) explica o porquê. O
     * Interactor avisa uma vez por aperto; a mesma recusa de novo com o
     * aviso na tela só renova o tempo dele (ShowToast), sem empilhar. */
    private void HandleRecusado(Interactable foco, ResultadoDoPasso resultado)
    {
        ShowToast(MensagemDeRecusa(foco, resultado), true);
    }

    /* Recusa num ponto de uso: o motivo vem pronto do próprio ponto. */
    private void HandlePontoRecusado(PontoDeUso ponto, string motivo)
    {
        ShowToast(motivo, false);
    }

    private void HandleErrouSkillCheck(Interactable foco)
    {
        ShowToast(AvisoErrouSkillCheck, true);
    }

    private string MensagemDeRecusa(Interactable foco, ResultadoDoPasso resultado)
    {
        switch (resultado)
        {
            case ResultadoDoPasso.FerramentaErrada:
                FocoHUDInfo info = foco.GetComponent<FocoHUDInfo>();
                if (info != null && !string.IsNullOrWhiteSpace(info.textoFerramentaErrada))
                    return FormatFoco(info.textoFerramentaErrada, foco, info);
                return string.Format(RecusaFerramentaErrada, Ferramentas.Nome(foco.FerramentaNecessaria));

            case ResultadoDoPasso.SemCarga:
                string semCarga = foco.FerramentaNecessaria == Ferramenta.Areia ? RecusaSemAreia : RecusaSemCarga;
                return string.Format(semCarga, Ferramentas.Nome(foco.FerramentaNecessaria));

            case ResultadoDoPasso.SacoCheio:
                return RecusaSacoCheio;

            case ResultadoDoPasso.MaosOcupadas:
                // Hoje não chega aqui (com o pneu, a mira atravessa os focos),
                // mas fica pronta para quando chegar.
                return RecusaMaosOcupadas;

            case ResultadoDoPasso.JaVazio:
                return RecusaJaVazio(foco.FerramentaNecessaria);

            default:
                return "";
        }
    }

    /* ---------------- Contador ---------------- */

    /* OnFocoEliminado do gerenciador (uma vez por foco): conta e dá o punch
     * do contador; o aviso sai aqui, menos no pneu que tem vaga na carroceria
     * (EsperaAvisoDoPneu). */
    internal void HandleCompleted(Interactable foco)
    {
        eliminated++;
        counterPunch = 1f;
        UpdateCounter();

        if (EsperaAvisoDoPneu(foco))
            return;

        FocoHUDInfo info = foco.GetComponent<FocoHUDInfo>();
        string template = info != null && !string.IsNullOrWhiteSpace(info.textoAoEliminar) ? info.textoAoEliminar : textoAoEliminar;
        string mensagem = FormatFoco(FormatContagem(template), foco, info);

        // A dica educativa entra aqui, numa segunda linha: é o momento em
        // que o jogador acabou de agir e está olhando para o resultado.
        ShowToast(ComDica(mensagem, foco, info, out bool comDica), comDica);
    }

    /* Acrescenta a dica do foco (se houver e mostrarDicaAoEliminar) numa
     * segunda linha do aviso. */
    private string ComDica(string mensagem, Interactable foco, FocoHUDInfo info, out bool comDica)
    {
        string dica = mostrarDicaAoEliminar ? DicaDoFoco(foco, info) : "";
        comDica = !string.IsNullOrWhiteSpace(dica);
        if (comDica)
            mensagem += "\n" + formatoDicaNoAviso.Replace("{dica}", FormatFoco(FormatContagem(dica), foco, info));
        return mensagem;
    }

    /* OnFocoEsvaziado do gerenciador: o atalho tirou a água, mas é por
     * enquanto (a chuva enche de novo). No pneu, o aviso já diz o próximo
     * passo: levar à picape da UPA. */
    private void HandleEsvaziado(Interactable foco)
    {
        ShowToast(AvisoAoEsvaziar(foco), false);
    }

    /* Um pneu chegou à caçamba da picape (no fim do arco): é o aviso da
     * entrega, com a dica do pneu na segunda linha (o de eliminado não saiu:
     * ver HandleCompleted). Sem vaga, ele troca o de eliminado. */
    internal void HandlePneuRecebido(PneuInteractable pneu, int numero)
    {
        string mensagem = TextoPneuRecolhido(numero, TotalDePneus());
        ShowToast(ComDica(mensagem, pneu, pneu.GetComponent<FocoHUDInfo>(), out bool comDica), comDica);
    }

    /* Os pneus da partida (os PneuInteractable do gerenciador; sem
     * gerenciador, 0). */
    private int TotalDePneus()
    {
        int total = 0;
        if (gerenciador == null)
            return total;

        foreach (Interactable foco in gerenciador.Focos)
        {
            if (foco is PneuInteractable)
                total++;
        }
        return total;
    }

    private void UpdateCounter()
    {
        if (counterText != null)
            counterText.text = FormatContagem(formatoContador);
    }

    /* "Pulinho" do painel quando um foco é eliminado. */
    private void UpdateCounterPunch()
    {
        if (counterPanel == null)
            return;
        counterPunch = Mathf.MoveTowards(counterPunch, 0f, Time.deltaTime * 3f);
        float s = 1f + Mathf.Sin(counterPunch * Mathf.PI) * 0.12f;
        counterPanel.localScale = new Vector3(s, s, 1f);
    }
}
