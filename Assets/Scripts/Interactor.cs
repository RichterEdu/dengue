using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/* ============================================================
 *  Interactor.cs
 *  ---------------------------------------------------------------------
 *  Fica no PLAYER e gerencia a interação em primeira pessoa.
 *
 *  - Dispara um raycast do CENTRO da tela (como a visão do jogador) e
 *    mira o primeiro alvo válido: um foco (Interactable ou derivado) que
 *    ainda aceita passos, ou um ponto de uso (PontoDeUso: picape, monte
 *    de areia, caçamba), achados no próprio objeto ou nos pais. Foco já
 *    concluído (ou que não pode ser alvo agora) não tapa a mira: o raio
 *    passa por ele (o pneu entregue encolhe em cima da caçamba). Parede,
 *    chão e o resto tapam.
 *  - SEGURAR para agir (Parte A): o botão de interagir (clique ESQUERDO;
 *    no controle, a ação "Attack" do EntradaJogador.cs, botão Oeste)
 *    mantido apertado acumula tempo no alvo (SessaoDeSegurar.cs). Cada
 *    SegundosPorPasso segurado vira um AvancarCom(kit) do foco (= um
 *    clique antigo); no ponto, segurar SegundosParaUsar chama Usar(kit),
 *    um uso por aperto.
 *  - No aperto (ou ao trocar de alvo segurando), o alvo é conferido uma
 *    vez (Verificar / PodeUsar). Recusado, OnRecusado / OnPontoRecusado
 *    avisa a HUD e nada acumula até soltar o botão ou trocar de alvo.
 *    Recusa de ponto com motivo vazio passa em silêncio.
 *  - Kit do agente: as teclas 1–5, a roda do mouse e LB/RB trocam a
 *    ferramenta; o botão direito (G; no controle, botão Norte) larga o
 *    pneu. Com o pneu nas mãos, só ponto de uso é alvo (a caçamba). Sem
 *    KitDoAgente no player, segurar avança o foco sem ferramenta (como o
 *    clique antigo) e os pontos de uso recusam sem aviso.
 *  - ProgressoContinuo (0..1) e Avancando alimentam a barra da HUD e o
 *    som de ação.
 *  - Destaque visual: quando o jogador mira num foco ou ponto de uso, os
 *    materiais (Renderer) do alvo ganham um brilho de emissão pulsante.
 *    Ao deselecionar, cada material é restaurado ao estado original.
 *  - Usa o Input System NOVO, pelo EntradaJogador.cs (Mouse.current só
 *    quando o player não tem EntradaJogador).
 * ============================================================ */

public class Interactor : MonoBehaviour
{
    [Header("Interação")]
    [Tooltip("Distância máxima de interação, em unidades.")]
    [SerializeField] private float interactionRange = 3f;

    [Header("Kit do agente")]
    [Tooltip("Kit do agente (ferramenta, cargas, pneu). Vazio = o deste mesmo objeto. Sem kit, segurar avança o foco sem ferramenta, como o clique antigo.")]
    [SerializeField] private KitDoAgente kit;

    [Header("Destaque ao mirar")]
    [Tooltip("Cor do brilho de emissão aplicada ao objeto mirado.")]
    [SerializeField] private Color highlightColor = new Color(1f, 0.85f, 0.2f);
    [Tooltip("Intensidade base do brilho.")]
    [SerializeField] private float highlightIntensity = 1.6f;
    [Tooltip("Velocidade do pulso do brilho (0 = brilho fixo).")]
    [SerializeField] private float highlightPulseSpeed = 4.5f;

    private Camera cam;
    private EntradaJogador entrada;
    private Interactable currentTarget;
    private PontoDeUso pontoAlvo;

    // O agente de saúde (corpo do próprio jogador, ver PlayerBuilder.cs) não tem
    // Collider hoje, então o raycast abaixo não tem como acertar o próprio
    // corpo — mas se algum dia ganhar física (roupa, ragdoll etc.), esta
    // máscara evita que o jogador "mire em si mesmo" sem querer.
    // NÃO pode ser "static readonly" com inicializador: LayerMask.NameToLayer
    // é proibido em inicializador de campo/construtor estático de
    // MonoBehaviour (Unity lança exceção toda vez que o tipo é usado) —
    // por isso é lido em Start(), como qualquer outra referência da cena.
    private int camadaCorpoJogador = -1;

    // O raio atravessa focos fora de jogo, então são vários acertos por
    // quadro. 16 sobra para 3 m de alcance (sem alocar a cada quadro).
    private const int MaxAcertos = 16;
    private readonly RaycastHit[] acertos = new RaycastHit[MaxAcertos];
    private static readonly IComparer<RaycastHit> PorDistancia =
        Comparer<RaycastHit>.Create((a, b) => a.distance.CompareTo(b.distance));

    // Segurar para agir: tempo segurado no alvo → passos.
    private readonly SessaoDeSegurar sessao = new SessaoDeSegurar();

    // Conferência do aperto: o alvo já conferido enquanto o botão está
    // apertado, e se ele liberou o segurar (false depois de uma recusa ou
    // de um uso de ponto, até soltar o botão ou trocar de alvo).
    private Component alvoConferido;
    private bool liberado;

    private readonly List<HighlightEntry> highlighted = new List<HighlightEntry>();

    /* Recusa num foco (ferramenta errada, sem carga, saco cheio, mãos
     * ocupadas), para a HUD explicar. Sai uma vez por aperto. */
    public event Action<Interactable, ResultadoDoPasso> OnRecusado;

    /* Recusa num ponto de uso, com o motivo (nunca vazio). Sai uma vez por aperto. */
    public event Action<PontoDeUso, string> OnPontoRecusado;

    /* Foco mirado (lido pela HUD, pelo brilho de proximidade e por scripts futuros). */
    public Interactable CurrentTarget => currentTarget;

    /* Ponto de uso mirado (picape, monte de areia, caçamba). Nunca junto com um foco. */
    public PontoDeUso PontoAlvo => pontoAlvo;

    /* Barra da HUD (0..1): no foco, cliques dados + o passo em andamento;
     * no ponto, quanto do uso já foi segurado. */
    public float ProgressoContinuo { get; private set; }

    /* Segurando num alvo liberado (passo válido) neste quadro: som de ação. */
    public bool Avancando { get; private set; }

    /* Quando true, o jogador não mira nem interage com nada (usado, por
     * exemplo, enquanto o celular está levantado). Ao bloquear, o destaque
     * do alvo atual é desfeito e o segurar é zerado. */
    public bool Bloqueado { get; set; }

    /* Guarda o estado ORIGINAL de um material antes de aplicar o destaque. */
    private class HighlightEntry
    {
        public Renderer renderer;
        public Material material;
        public Color originalEmissionColor;
        public bool emissionEnabled;
    }

    void Start()
    {
        cam = Camera.main;
        entrada = GetComponent<EntradaJogador>();
        if (kit == null)
            kit = GetComponent<KitDoAgente>();
        camadaCorpoJogador = LayerMask.NameToLayer("CorpoJogador");
    }

    void Update()
    {
        // 1) Bloqueado (celular levantado, menu aberto...): larga o alvo,
        //    zera o segurar e sai.
        if (Bloqueado)
        {
            TrocarAlvo(null, null);
            sessao.Zerar();
            alvoConferido = null;
            liberado = false;
            ProgressoContinuo = 0f;
            Avancando = false;
            return;
        }

        // 2) Troca de ferramenta, só com as mãos livres.
        if (kit != null && kit.PneuCarregado == null && entrada != null)
        {
            int pedida = entrada.FerramentaPedida;
            if (pedida >= 0 && pedida < Ferramentas.Todas.Length)
                kit.Selecionar(Ferramentas.Todas[pedida]);

            int troca = entrada.TrocaDeFerramenta;
            if (troca > 0)
                kit.Proxima();
            else if (troca < 0)
                kit.Anterior();
        }

        // 3) Largar o pneu no chão, na frente do agente.
        if (kit != null && kit.PneuCarregado != null && entrada != null && entrada.LargarPressionado)
            kit.LargarPneu(transform.position, transform.forward);

        // 4) Alvo da mira: se mudou, desfaz o destaque no antigo e aplica no novo.
        bool comPneu = kit != null && kit.PneuCarregado != null;
        RaycastTarget(comPneu, out Interactable foco, out PontoDeUso ponto);
        TrocarAlvo(foco, ponto);

        UpdateHighlightPulse();

        // 5) e 6) Conferir no aperto e avançar segurando.
        AtualizarSegurar();

        // 7) Barra da HUD.
        AtualizarProgresso();
    }

    /* Botão de interagir mantido apertado (clique esquerdo ou "Attack"). */
    private bool LerSegurando()
    {
        if (entrada != null)
            return entrada.InteragirSegurado;

        // Sem EntradaJogador no player: só o mouse, como antes.
        Mouse mouse = Mouse.current;
        return mouse != null && mouse.leftButton.isPressed;
    }

    /* Confere o alvo no aperto (ou ao trocar de alvo segurando) e converte o
     * tempo segurado em passos: AvancarCom no foco, Usar no ponto. */
    private void AtualizarSegurar()
    {
        // Alvo como referência C# de verdade: um objeto destruído vira null
        // aqui (o "null falso" do Unity não pode chegar à SessaoDeSegurar,
        // que compara por referência).
        Component alvo = null;
        if (currentTarget != null)
            alvo = currentTarget;
        else if (pontoAlvo != null)
            alvo = pontoAlvo;

        bool segurando = LerSegurando();
        if (!segurando)
        {
            alvoConferido = null;
            liberado = false;
        }

        // Pausado (timeScale 0): nenhum passo, nem o instantâneo do pneu
        // esvaziado, e nenhuma conferência (o aviso sairia com o jogo parado).
        if (Time.deltaTime <= 0f)
        {
            sessao.Zerar();
            Avancando = false;
            return;
        }

        if (segurando && alvo != null && !ReferenceEquals(alvo, alvoConferido))
        {
            // Aperto novo, ou outro alvo com o botão ainda apertado: confere uma vez.
            alvoConferido = alvo;
            liberado = Conferir();
        }

        float segundosPorPasso = currentTarget != null ? currentTarget.SegundosPorPasso
                               : pontoAlvo != null ? pontoAlvo.SegundosParaUsar
                               : 0f;
        int passos = sessao.Atualizar(alvo, segurando && liberado, Time.deltaTime, segundosPorPasso);

        for (int i = 0; i < passos; i++)
        {
            if (!DarPasso())
                break;
        }

        Avancando = segurando && liberado && alvo != null;
    }

    /* Conferência do aperto, sem avançar nem gastar nada. Recusado, avisa. */
    private bool Conferir()
    {
        if (currentTarget != null)
        {
            ResultadoDoPasso resultado = currentTarget.Verificar(kit);
            if (resultado == ResultadoDoPasso.Avancou)
                return true;

            AvisarRecusa(currentTarget, resultado);
            return false;
        }

        if (pontoAlvo != null)
        {
            if (pontoAlvo.PodeUsar(kit, out string motivo))
                return true;

            AvisarRecusa(pontoAlvo, motivo);
            return false;
        }

        return false;
    }

    /* Um passo no alvo atual. False = parar por aqui neste aperto ou alvo
     * (o foco concluiu ou foi pego, o ponto foi usado, ou houve recusa). */
    private bool DarPasso()
    {
        if (currentTarget != null)
        {
            ResultadoDoPasso resultado = currentTarget.AvancarCom(kit);
            switch (resultado)
            {
                case ResultadoDoPasso.Avancou:
                    return true;

                case ResultadoDoPasso.Concluiu:
                case ResultadoDoPasso.Pegou:
                    // O foco saiu da mira (concluído ou nos braços).
                    sessao.Zerar();
                    return false;

                default:
                    // Recusa no meio do segurar (ex.: o saco encheu).
                    sessao.Zerar();
                    liberado = false;
                    AvisarRecusa(currentTarget, resultado);
                    return false;
            }
        }

        if (pontoAlvo != null)
        {
            // Confere de novo: o kit pode ter mudado desde o aperto.
            if (pontoAlvo.PodeUsar(kit, out string motivo))
                pontoAlvo.Usar(kit);
            else
                AvisarRecusa(pontoAlvo, motivo);

            // Um uso por aperto: segurar mais não repete o uso (nem mostra
            // "kit já está cheio" logo depois de reabastecer).
            sessao.Zerar();
            liberado = false;
            return false;
        }

        return false;
    }

    /* Só as recusas que a HUD explica; o Bloqueado passa em silêncio. */
    private void AvisarRecusa(Interactable foco, ResultadoDoPasso resultado)
    {
        if (Ferramentas.EhRecusa(resultado))
            OnRecusado?.Invoke(foco, resultado);
    }

    /* Motivo vazio = recusa sem aviso (ex.: sem kit). */
    private void AvisarRecusa(PontoDeUso ponto, string motivo)
    {
        if (!string.IsNullOrEmpty(motivo))
            OnPontoRecusado?.Invoke(ponto, motivo);
    }

    /* Barra da HUD: cliques já dados + a fração do passo em andamento. */
    private void AtualizarProgresso()
    {
        if (currentTarget != null)
            ProgressoContinuo = Mathf.Clamp01((currentTarget.CurrentClicks + sessao.Fracao) /
                                              Mathf.Max(1, currentTarget.RequiredClicks));
        else if (pontoAlvo != null)
            ProgressoContinuo = sessao.Fracao;
        else
            ProgressoContinuo = 0f;
    }

    /* Troca o alvo mirado; mudou, desfaz o destaque no antigo e aplica no novo. */
    private void TrocarAlvo(Interactable foco, PontoDeUso ponto)
    {
        // Por referência: um alvo destruído também conta como mudança.
        if (ReferenceEquals(foco, currentTarget) && ReferenceEquals(ponto, pontoAlvo))
            return;

        RestoreHighlight();
        currentTarget = foco;
        pontoAlvo = ponto;

        if (foco != null)
            ApplyHighlight(foco);
        else if (ponto != null)
            ApplyHighlight(ponto);
    }

    /* Dispara um raio do centro da tela e acha o primeiro alvo válido, do
     * mais perto para o mais longe: um foco que ainda aceita passos (só com
     * as mãos livres) ou um ponto de uso. Foco concluído ou que não pode ser
     * alvo agora (ex.: vaso já com areia, pneu entregue encolhendo, qualquer
     * foco com o pneu nas mãos) é atravessado; qualquer outro collider
     * (parede, chão, muro) tapa a mira. */
    private void RaycastTarget(bool comPneu, out Interactable foco, out PontoDeUso ponto)
    {
        foco = null;
        ponto = null;

        if (cam == null)
            return;

        Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f));

        int mascara = camadaCorpoJogador >= 0 ? ~(1 << camadaCorpoJogador) : ~0;
        int quantos = Physics.RaycastNonAlloc(ray, acertos, interactionRange, mascara, QueryTriggerInteraction.Ignore);
        Array.Sort(acertos, 0, quantos, PorDistancia);

        for (int i = 0; i < quantos; i++)
        {
            Collider atingido = acertos[i].collider;

            Interactable achado = atingido.GetComponentInParent<Interactable>();
            if (achado != null)
            {
                if (!comPneu && achado.CanInteract)
                {
                    foco = achado;
                    return;
                }
                continue;   // foco fora de jogo: o raio passa por ele
            }

            PontoDeUso pontoAchado = atingido.GetComponentInParent<PontoDeUso>();
            if (pontoAchado != null)
                ponto = pontoAchado;
            return;   // ponto de uso, ou algo que tapa a mira
        }
    }

    /* Aplica emissão pulsante em todos os Renderer do alvo (e filhos). */
    private void ApplyHighlight(Component target)
    {
        foreach (Renderer rend in target.GetComponentsInChildren<Renderer>())
        {
            if (rend == null || rend.sharedMaterial == null)
                continue;

            // Instancia o material para não afetar outros usuários do mesmo asset.
            Material mat = rend.material;
            highlighted.Add(new HighlightEntry
            {
                renderer = rend,
                material = mat,
                originalEmissionColor = mat.GetColor("_EmissionColor"),
                emissionEnabled = mat.IsKeywordEnabled("_EMISSION")
            });

            mat.EnableKeyword("_EMISSION");
        }
    }

    /* Pulsa a cor de emissão de cada material destacado a cada frame. */
    private void UpdateHighlightPulse()
    {
        if (highlighted.Count == 0)
            return;

        float intensity = highlightIntensity * (0.75f + 0.25f * Mathf.Sin(Time.time * highlightPulseSpeed));
        Color emission = highlightColor * intensity;

        for (int i = highlighted.Count - 1; i >= 0; i--)
        {
            if (highlighted[i].renderer == null)
            {
                highlighted.RemoveAt(i);
                continue;
            }
            highlighted[i].material.SetColor("_EmissionColor", emission);
        }
    }

    /* Devolve cada material ao estado original (emissão/cor/ativado). */
    private void RestoreHighlight()
    {
        foreach (HighlightEntry entry in highlighted)
        {
            if (entry.renderer == null || entry.material == null)
                continue;

            if (entry.emissionEnabled)
                entry.material.SetColor("_EmissionColor", entry.originalEmissionColor);
            else
                entry.material.DisableKeyword("_EMISSION");
        }

        highlighted.Clear();
    }
}
