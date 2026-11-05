using System;
using System.Collections.Generic;
using UnityEngine;

/* ============================================================
 *  KitDoAgente.cs
 *  ---------------------------------------------------------------------
 *  O que o agente de saúde tem nas mãos (Parte A). Fica no Player, junto
 *  do ControladorJogador, e guarda:
 *
 *    - o EstoqueDoKit (cargas de areia, tela e água sanitária + o saco
 *      de lixo), criado com os números do Inspector;
 *    - a ferramenta Selecionada (começa nas Mãos), trocada pelas teclas
 *      1–5 (Selecionar) ou pela roda do mouse / LB-RB (Proxima/Anterior,
 *      dando a volta na lista);
 *    - o pneu que está sendo carregado (PneuCarregado). Pegar o põe no
 *      PontoDeCarga e limita o jogador (velocidade × multiplicadorCarregando,
 *      sem pular); enquanto carrega, não troca de ferramenta. O Shift corre
 *      gastando a Stamina (StaminaDeCarga): zerada, só anda até recuperar.
 *      De mãos livres a corrida não gasta nada, e a stamina volta sozinha.
 *      LargarPneu (no chão, na frente do agente e fora do corpo dele) e
 *      EntregarPneu (na caçamba da picape, onde o foco conclui e o pneu
 *      vai para uma vaga da carroceria) devolvem o movimento normal.
 *      Largar em cima da caçamba é entregar.
 *
 *  Quem mexe no estoque passa por aqui (Consumir, Reabastecer,
 *  GuardarGarrafa, EsvaziarSaco — este, na lixeira) para a HUD ficar sabendo: OnMudou
 *  avisa qualquer mudança; OnReabasteceu e OnSacoEsvaziado só quando
 *  algo encheu ou saiu de verdade (dão o som e o aviso na tela). O
 *  OnReabasteceu diz de que ponto veio o material (o post da picape da
 *  UPA só sai nela, e não no monte de areia).
 *
 *  No EditMode o Awake não roda: o estoque e o ControladorJogador são
 *  achados de forma preguiçosa, no primeiro acesso.
 * ============================================================ */

public class KitDoAgente : MonoBehaviour
{
    [Header("Estoque (cheio ao começar)")]
    [Tooltip("Cargas de areia (cada uma cobre o pratinho de um vaso).")]
    [SerializeField] private int cargaAreia = 1;

    [Tooltip("Telas (cada uma tampa um ralo).")]
    [SerializeField] private int cargaTela = 1;

    [Tooltip("Doses de água sanitária (cada uma trata uma bromélia).")]
    [SerializeField] private int cargaAguaSanitaria = 2;

    [Tooltip("Quantas garrafas cabem no saco de lixo antes de esvaziar numa lixeira.")]
    [SerializeField] private int capacidadeSaco = 6;

    [Header("Carregando pneu")]
    [Tooltip("Multiplica a velocidade do jogador enquanto carrega um pneu, andando ou correndo (sem pular).")]
    [SerializeField] private float multiplicadorCarregando = 0.85f;

    [Tooltip("Segundos correndo com o pneu até a stamina zerar.")]
    [SerializeField] private float segundosDeCorrida = 6f;

    [Tooltip("Segundos sem correr para a stamina encher de zero, com ou sem o pneu.")]
    [SerializeField] private float segundosParaRecuperar = 5f;

    [Tooltip("Zerada, a stamina precisa voltar a esta fração para correr de novo (0.3 = 30%).")]
    [Range(0f, 1f)]
    [SerializeField] private float fracaoParaVoltarACorrer = 0.3f;

    [Tooltip("Onde o pneu fica enquanto é carregado (filho do osso Chest, entre as mãos da pose de carga).")]
    [SerializeField] private Transform pontoDeCarga;

    [Header("Referências")]
    [Tooltip("Controlador do jogador que recebe os limites de movimento. Vazio = o deste mesmo objeto.")]
    [SerializeField] private ControladorJogador jogador;

    // Sem CharacterController (cena montada à mão): o raio do corpo do
    // Player da cena, com a pele (0,3 + 0,03 m, PlayerBuilder).
    private const float RaioPadraoDoAgente = 0.33f;

    private EstoqueDoKit estoque;
    private StaminaDeCarga stamina;

    /* Qualquer mudança no kit (ferramenta, cargas, saco, pneu): a HUD se redesenha. */
    public event Action OnMudou;
    public event Action OnStaminaEsgotada;
    public event Action<Ferramenta> OnFerramentaTrocada;

    /* Alguma carga encheu de verdade; o ponto usado vai junto (null = sem ponto). */
    public event Action<PontoDeReabastecimento> OnReabasteceu;
    public event Action OnSacoEsvaziado;

    /* Pneu nos braços, no chão de novo e entregue na caçamba (sons, HUD, posts). */
    public event Action<PneuInteractable> OnPneuPego;
    public event Action<PneuInteractable> OnPneuLargado;
    public event Action<PneuInteractable> OnPneuEntregue;

    /* Criado no primeiro acesso, com os números do Inspector. */
    public EstoqueDoKit Estoque
    {
        get
        {
            if (estoque == null)
                estoque = new EstoqueDoKit(cargaAreia, cargaTela, cargaAguaSanitaria, capacidadeSaco);
            return estoque;
        }
    }

    public Ferramenta Selecionada { get; private set; } = Ferramenta.Maos;

    /* Pneu nos braços do agente (null = mãos livres). */
    public PneuInteractable PneuCarregado { get; private set; }

    public Transform PontoDeCarga
    {
        get => pontoDeCarga;
        set => pontoDeCarga = value;
    }

    /* A stamina da corrida com o pneu (criada no primeiro uso, como o estoque). */
    public StaminaDeCarga Stamina
    {
        get
        {
            if (stamina == null)
                stamina = new StaminaDeCarga(segundosDeCorrida, segundosParaRecuperar, fracaoParaVoltarACorrer);
            return stamina;
        }
    }

    void Update()
    {
        ControladorJogador controlador = Jogador;
        AvancarStamina(Time.deltaTime, controlador != null && controlador.Correndo);
    }

    /* Só gasta correndo com o pneu. Com ele, a corrida depende da stamina;
     * sem ele, o movimento é o normal (LimitarMovimento já devolveu). */
    internal void AvancarStamina(float dt, bool correndo)
    {
        bool comPneu = PneuCarregado != null;
        if (Stamina.Avancar(dt, correndo && comPneu))
            OnStaminaEsgotada?.Invoke();

        ControladorJogador controlador = Jogador;
        if (comPneu && controlador != null)
            controlador.PodeCorrer = Stamina.PodeCorrer;
    }

    /* O ControladorJogador do campo ou, vazio, o deste objeto (achado no primeiro uso). */
    private ControladorJogador Jogador
    {
        get
        {
            if (jogador == null)
                jogador = GetComponent<ControladorJogador>();
            return jogador;
        }
    }

    /* Raio do corpo do agente (o CharacterController deste objeto, com a
     * pele): o pneu largado fica fora dele. */
    private float RaioDoAgente
    {
        get
        {
            CharacterController corpo = GetComponent<CharacterController>();
            if (corpo == null)
                return RaioPadraoDoAgente;

            Vector3 escala = transform.lossyScale;
            return corpo.radius * Mathf.Max(Mathf.Abs(escala.x), Mathf.Abs(escala.z)) + corpo.skinWidth;
        }
    }

    /* Troca de ferramenta. False se está carregando pneu ou se ela já era a selecionada. */
    public bool Selecionar(Ferramenta ferramenta)
    {
        if (PneuCarregado != null || ferramenta == Selecionada)
            return false;

        Selecionada = ferramenta;
        OnFerramentaTrocada?.Invoke(ferramenta);
        OnMudou?.Invoke();
        return true;
    }

    /* Próxima da barra; depois da última volta para a primeira. */
    public void Proxima()
    {
        AndarNaBarra(+1);
    }

    /* Anterior da barra; antes da primeira vai para a última. */
    public void Anterior()
    {
        AndarNaBarra(-1);
    }

    private void AndarNaBarra(int passo)
    {
        Ferramenta[] todas = Ferramentas.Todas;
        int atual = Array.IndexOf(todas, Selecionada);
        int nova = (atual + passo + todas.Length) % todas.Length;
        Selecionar(todas[nova]);
    }

    /* Gasta uma carga (o Interactable chama ao concluir o foco). False se estava vazio. */
    public bool Consumir(Ferramenta ferramenta)
    {
        if (!Estoque.Consumir(ferramenta))
            return false;

        // Mãos e saco não gastam nada: nada mudou para a HUD.
        if (Ferramentas.Consumivel(ferramenta))
            OnMudou?.Invoke();
        return true;
    }

    /* Enche as ferramentas da lista. True (e os avisos) só se alguma estava
     * faltando. 'origem' é o ponto usado, que vai no OnReabasteceu. */
    public bool Reabastecer(IReadOnlyList<Ferramenta> ferramentas, PontoDeReabastecimento origem = null)
    {
        if (!Estoque.Reabastecer(ferramentas))
            return false;

        OnReabasteceu?.Invoke(origem);
        OnMudou?.Invoke();
        return true;
    }

    /* Põe uma garrafa no saco. False se o saco já está cheio. */
    public bool GuardarGarrafa()
    {
        if (!Estoque.GuardarGarrafa())
            return false;

        OnMudou?.Invoke();
        return true;
    }

    /* Esvazia o saco (na lixeira) e devolve quantas garrafas saíram (avisa só se saiu alguma). */
    public int EsvaziarSaco()
    {
        int tiradas = Estoque.EsvaziarSaco();
        if (tiradas > 0)
        {
            OnSacoEsvaziado?.Invoke();
            OnMudou?.Invoke();
        }
        return tiradas;
    }

    /* Põe o pneu nos braços (o pneu chama no fim do segurar). False se já
     * carrega um. Sem PontoDeCarga, o pneu fica preso a este objeto. */
    public bool Pegar(PneuInteractable pneu)
    {
        if (pneu == null || PneuCarregado != null)
            return false;

        PneuCarregado = pneu;
        pneu.PrenderEm(pontoDeCarga != null ? pontoDeCarga : transform);
        LimitarMovimento(multiplicadorCarregando, Stamina.PodeCorrer, false);

        OnPneuPego?.Invoke(pneu);
        OnMudou?.Invoke();
        return true;
    }

    /* Larga o pneu no chão, na frente dos pés e fora do corpo do agente
     * (PneuInteractable.OndeCai). Se ele cairia na caçamba (o agente
     * encostado nela, achando que joga dentro), é entrega (EntregarPneu),
     * e o foco conclui. False se não carrega nenhum. */
    public bool LargarPneu(Vector3 pes, Vector3 frente)
    {
        PneuInteractable pneu = PneuCarregado;
        if (pneu == null)
            return false;

        pneu.OndeCai(pes, frente, RaioDoAgente, out Vector3 posicao, out Quaternion giro, out Cacamba cacamba);
        if (cacamba != null)
            return EntregarPneu(cacamba);

        PneuCarregado = null;
        LimitarMovimento(1f, true, true);
        pneu.Largar(posicao, giro);

        OnPneuLargado?.Invoke(pneu);
        OnMudou?.Invoke();
        return true;
    }

    /* Entrega o pneu na caçamba da picape, onde o foco conclui e o pneu
     * vai para a próxima vaga da carroceria (PneuInteractable.EntregarEm).
     * False se não carrega nenhum (ou sem caçamba). As mãos ficam livres
     * antes do OnCompleted do pneu, para quem o escuta já ver o kit sem
     * pneu. */
    public bool EntregarPneu(Cacamba cacamba)
    {
        PneuInteractable pneu = PneuCarregado;
        if (pneu == null || cacamba == null)
            return false;

        PneuCarregado = null;
        LimitarMovimento(1f, true, true);
        pneu.EntregarEm(cacamba);

        OnPneuEntregue?.Invoke(pneu);
        OnMudou?.Invoke();
        return true;
    }

    /* Limites de movimento do jogador (sem ControladorJogador, nada a fazer). */
    private void LimitarMovimento(float multiplicador, bool podeCorrer, bool podePular)
    {
        ControladorJogador controlador = Jogador;
        if (controlador == null)
            return;

        controlador.MultiplicadorVelocidade = multiplicador;
        controlador.PodeCorrer = podeCorrer;
        controlador.PodePular = podePular;
    }
}
