using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* ============================================================
 *  PneuInteractable.cs
 *  ---------------------------------------------------------------------
 *  Foco do tipo PNEU — herda a base Interactable e adiciona o
 *  comportamento específico: o pneu é pego CHEIO, com as Mãos, e esvazia
 *  NOS BRAÇOS (a água é retirada: o filho "agua do pneu" é desativado e o
 *  pneu fica Esvaziado). Antes disso a água continua lá: o pneu que parece
 *  vazio está vazio (não zumbe e a chuva o reenche).
 *
 *  Kit do agente (Parte A): o pneu não conclui sozinho.
 *  - Com as Mãos, o clique pega o pneu na hora, cheio ou vazio
 *    (KitDoAgente.Pegar → PrenderEm): fica no PontoDeCarga, com os
 *    colliders desligados (Carregado). Não encolhe nos passos
 *    (Interactable): é carregado do tamanho dele. Não há minigame no chão.
 *  - Água nos braços: cada pneu da cena tem a água como um disco plano
 *    deitado, que com o pneu de pé taparia o furo. Por isso ela fica
 *    escondida nos braços e volta ao largar, se o pneu não esvaziou.
 *  - Esvaziar nos braços (BalancarNosBracos): o jogador balança o olhar
 *    para os lados no ritmo da água (OndaDoPneu, a mesma lógica pura de
 *    antes). O Nivel cai com a água derramando (NivelDeAgua), cada passo
 *    fechado pela onda é um clique (DarPassoNosBracos) e o último esvazia:
 *    Esvaziou, a água some e o pneu fica esvaziado nos braços, de pose
 *    zerada. O visual é só o pneu inclinado em torno da frente do agente
 *    (e um pouco para o lado), com um estouro de partículas no lado baixo
 *    ao derramar (só no Play). Largar no meio guarda o nível dos passos já
 *    fechados; a onda recomeça de lá ao pegar. Os números da onda (período,
 *    amortecimento, ganho, borda, taxa, graus/s) são calibráveis no
 *    Inspector.
 *  - Largar: OndeCai() calcula a pose sem mexer no pneu, e Largar()
 *    assenta nela. O pneu cai de pé (sem a inclinação do balanço), com o
 *    eixo da roda apontado para a frente do agente (assim ocupa só a
 *    largura da banda na direção dele, e não o diâmetro), afastado pelo
 *    próprio tamanho: a borda de cá fica fora do corpo do agente. Com um
 *    muro ou parede na frente, fica do lado de cá; sem chão (beira de
 *    muro, borda do mapa), cai no pé dele. Largado, continua sendo foco
 *    (alcançável pela mira), e pegar de novo é instantâneo. Se ele cairia
 *    numa caçamba (ou contra a cabine da picape dela), o KitDoAgente
 *    entrega em vez de largar.
 *  - EntregarEm() (na caçamba da picape, Parte B) reserva a próxima vaga
 *    da carroceria (VagaNaCacamba) e chama Concluir(): só aí o foco conta
 *    como eliminado. Com vaga, o OnComplete arremessa o pneu num arco
 *    até ela (Cacamba.AlturaDoArco e DuracaoArremesso), e ele fica lá,
 *    empilhado e sem colliders, SEM encolher; no fim do arco, o estouro
 *    de partículas e o Cacamba.Receber (aviso e balanço). Fora do Play o
 *    arremesso é instantâneo (as corrotinas não andam). Sem vaga, segue
 *    o fluxo padrão de encolher até sumir, 1,2 m acima do pivô da caçamba
 *    (à vista, e não dentro da carroceria), e o Receber sai na hora. O
 *    número do aviso (o pneu recebido 1, 2, 3...) vem da reserva.
 *  - Esvaziado, o pneu já não zumbe (SomDosFocos).
 *  - Esvaziado e fora dos braços (no chão, largado), a chuva o reenche
 *    (Reencher): a água volta e os cliques zeram, e dá para pegar e
 *    esvaziar de novo. Nos braços, não reenche.
 *
 *  No caminho antigo Interact() (sem kit), o último clique conclui o
 *  pneu como antes.
 * ============================================================ */

public class PneuInteractable : Interactable
{
    [Header("Pneu")]
    [Tooltip("Objeto filho que representa a água parada dentro do pneu.")]
    [SerializeField] private GameObject water;

    [Header("Balançar nos braços")]
    [Tooltip("Segundos de uma oscilação completa da água dentro do pneu (o ritmo certo de balançar).")]
    [SerializeField] private float periodoDaOnda = 0.9f;

    [Tooltip("Amortecimento da onda: quanto ela morre sozinha quando o jogador para de balançar.")]
    [SerializeField] private float amortecimentoDaOnda = 0.12f;

    [Tooltip("Quanto cada balanço no ritmo empurra a onda (maior = transborda mais cedo).")]
    [SerializeField] private float ganhoDaOnda = 15f;

    [Tooltip("Altura da onda (Inclinacao) a partir da qual a água transborda (1 = a borda).")]
    [SerializeField] private float limiarDaBorda = 1f;

    [Tooltip("Velocidade com que o pneu esvazia enquanto a água transborda (maior = esvazia mais rápido).")]
    [SerializeField] private float taxaDeDerrame = 1.4f;

    [Tooltip("Graus por segundo de olhar para os lados que contam como balanço máximo.")]
    [SerializeField] private float grausPorSegundoCheio = 180f;

    [Tooltip("Graus que o pneu inclina nos braços quando a água está na borda (Inclinacao = 1).")]
    [SerializeField] private float inclinacaoMaxima = 25f;

    // Balançar nos braços: o quanto o pneu se desloca para o lado (na direita
    // do agente) com a onda na borda, o raio do respingo (do centro do pneu ao
    // lado baixo) e o intervalo entre os estouros de partículas.
    private const float DeslocamentoLateral = 0.04f;
    private const float RaioDoRespingo = 0.3f;
    private const float IntervaloDasParticulas = 0.25f;

    private OndaDoPneu onda;            // null = ainda não balançou desde que pegou
    private float proximaParticula;

    // Raycasts que procuram o chão (para baixo, até esta distância).
    private const float AlcanceDoChao = 4f;

    // Largar: o eixo da roda no modelo da cena (rubber tries.fbx) é o X
    // local: banda de ~0,2 m no X e disco de ~0,7 m no plano YZ, pivô no
    // centro. A borda de cá do pneu fica FolgaDoAgente além do corpo do
    // agente. O chão é procurado de cima (altura do raio) para baixo; um
    // muro na frente, na altura do peito, e a borda de lá fica a
    // FolgaDoMuro dele.
    private static readonly Vector3 EixoDaRoda = Vector3.right;
    private const float FolgaDoAgente = 0.15f;
    private const float AlturaDoRaio = 1.5f;
    private const float AlturaDoPeito = 1.0f;
    private const float FolgaDoMuro = 0.1f;

    // Guardados no PrenderEm para o Largar/EntregarEm.
    private Transform paiOriginal;
    private Quaternion rotacaoOriginal;
    private float alturaDoPivo;         // do pivô até o chão, para assentar ao largar
    private List<Collider> desligados;  // os colliders que o PrenderEm desligou

    // Sem vaga, o pneu encolhe esta altura acima do pivô da caçamba.
    private const float AlturaSemVaga = 1.2f;

    // A caçamba em que foi entregue (EntregarEm) e o número da entrega
    // (ReservarVaga), para o Receber no fim do arco.
    private Cacamba cacambaDeDestino;
    private int numeroNaCacamba;

    /* Está nos braços do agente (filho do PontoDeCarga). */
    public bool Carregado { get; private set; }

    /* A vaga da carroceria onde o pneu entregue fica (null = não entregue,
     * ou entregue sem vaga livre). */
    public Transform VagaNaCacamba { get; private set; }

    /* Quanta água ainda há (1 cheio .. 0 vazio): 0 esvaziado; com a onda
     * ativa, o Nivel dela; senão, o que os passos já fechados deixaram. */
    public float NivelDeAgua
    {
        get
        {
            if (Esvaziado)
                return 0f;
            if (onda != null)
                return onda.Nivel;
            return RequiredClicks > 0 ? Mathf.Clamp01(1f - (float)CurrentClicks / RequiredClicks) : 1f;
        }
    }

    /* Nos braços não é alvo. Esvaziado, continua alvo (para pegar de novo)
     * mesmo com os cliques já no total; antes disso, vale a regra da base. */
    public override bool CanInteract => !IsComplete && !Carregado && (Esvaziado || base.CanInteract);

    /* Sem água não há criadouro: o zumbido para, mesmo antes da caçamba. */
    public override bool Zumbe => !Esvaziado;

    /* Esvaziado, pegar de novo é na hora (0 = um passo por aperto). */
    public override float SegundosPorPasso => Esvaziado ? 0f : base.SegundosPorPasso;

    /* Com kit, o clique só pega o pneu: na hora. */
    public override float SegundosPorPassoCom(KitDoAgente kit) => kit != null ? 0f : SegundosPorPasso;

    /* Nos braços a chuva não reenche: só no chão (no lugar ou largado). */
    public override bool PodeReencher => base.PodeReencher && !Carregado;

    /* O último passo só esvazia o pneu; quem conclui é o Entregar. */
    protected override bool ConcluiSozinho => false;

    /* Com kit, o clique pega o pneu na hora (cheio ou vazio). Sem kit, o
     * passo normal da base (caminho antigo). */
    public override ResultadoDoPasso AvancarCom(KitDoAgente kit)
    {
        if (kit == null || Carregado)
            return base.AvancarCom(kit);

        ResultadoDoPasso conferido = Verificar(kit);
        if (conferido != ResultadoDoPasso.Avancou)
            return conferido;

        return kit.Pegar(this) ? ResultadoDoPasso.Pegou : ResultadoDoPasso.Bloqueado;
    }

    /* Sem minigame no chão: o pneu esvazia nos braços. */
    public override IMinigame CriarMinigame(KitDoAgente kit, int semente) => null;

    public override string ConviteDoMinigame(KitDoAgente kit) => null;

    /* Esvaziar nos braços leva uns segundos de balanço. */
    public override float SegundosEstimados => Esvaziado ? 0f : 6f;

    /* Todo quadro com o pneu nos braços: o olhar horizontal do quadro
     * (graus) empurra a onda; derramando, o Nivel cai e cada passo fechado
     * é um clique. dt ≤ 0, pneu fora dos braços ou esvaziado: nada.
     * frenteDoAgente é o eixo da inclinação (o transform.forward do Player). */
    internal void BalancarNosBracos(float dt, float olharX, Vector3 frenteDoAgente)
    {
        if (!Carregado || Esvaziado || !(dt > 0f))
            return;

        if (onda == null)
        {
            float nivel = RequiredClicks > 0 ? 1f - (float)CurrentClicks / RequiredClicks : 1f;
            onda = new OndaDoPneu(RequiredClicks, periodoDaOnda, amortecimentoDaOnda, ganhoDaOnda, limiarDaBorda,
                                  taxaDeDerrame, grausPorSegundoCheio, nivel);
            proximaParticula = 0f;
        }

        EstadoDoMinigame estado = onda.Atualizar(dt, new EntradaDoMinigame { Olhar = new Vector2(olharX, 0f) });
        if (estado == EstadoDoMinigame.Passo && DarPassoNosBracos() == ResultadoDoPasso.Esvaziou)
            return;     // já voltou à pose zerada

        AplicarBalanco(frenteDoAgente);
    }

    /* O pneu inclinado em torno da frente do agente, sobre o giro do pai
     * (o PontoDeCarga), e levado um pouco para o lado; derramando, estoura
     * partículas no lado baixo (só no Play: o EstourarParticulas destrói
     * com atraso, o que o EditMode não permite). */
    private void AplicarBalanco(Vector3 frenteDoAgente)
    {
        Transform pai = transform.parent;
        Quaternion giroDoPai = pai != null ? pai.rotation : Quaternion.identity;
        Vector3 centro = pai != null ? pai.position : transform.position;

        Vector3 frente = frenteDoAgente.sqrMagnitude > 1e-6f ? frenteDoAgente.normalized : Vector3.forward;
        float angulo = Mathf.Clamp(onda.Inclinacao * inclinacaoMaxima, -inclinacaoMaxima, inclinacaoMaxima);
        Vector3 direita = Vector3.Cross(Vector3.up, frente).normalized;
        float lado = Mathf.Clamp(onda.Inclinacao, -1f, 1f);

        transform.SetPositionAndRotation(centro + direita * (DeslocamentoLateral * lado),
                                         Quaternion.AngleAxis(angulo, frente) * giroDoPai);

        if (onda.Derramando && Application.isPlaying && Time.time >= proximaParticula)
        {
            proximaParticula = Time.time + IntervaloDasParticulas;

            // Girar o eixo para cima inclina o topo para o lado de Cross(eixo, cima):
            // esse lado desce com a inclinação positiva.
            Vector3 ladoBaixo = Vector3.Cross(frente, Vector3.up).normalized * Mathf.Sign(onda.Inclinacao);
            EstourarParticulas(transform.position + ladoBaixo * RaioDoRespingo);
        }
    }

    /* Soma um passo de esvaziar nos braços. No último, a água sai (some), o
     * pneu fica Esvaziado e a pose nos braços volta a zero. */
    internal ResultadoDoPasso DarPassoNosBracos()
    {
        currentClicks++;
        if (currentClicks < RequiredClicks)
            return ResultadoDoPasso.Avancou;

        if (water != null)
            water.SetActive(false);
        Esvaziar();
        onda = null;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        return ResultadoDoPasso.Esvaziou;
    }

    /* Último passo do caminho antigo (kit null): a água sai e conclui na
     * hora, como antes do kit. */
    protected override void AoTerminarAcao(KitDoAgente kit)
    {
        // A água sai antes do Esvaziar: quem ouve o OnEsvaziado já vê o
        // pneu sem água.
        if (water != null)
            water.SetActive(false);
        Esvaziar();

        if (kit == null)
            Concluir();
    }

    /* Reenchido (a chuva): a água volta e os cliques zeram, para dar para
     * pegar e esvaziar de novo (sem isso o CanInteract ficaria falso de vez). */
    protected override void AoReencher()
    {
        if (water != null)
            water.SetActive(true);
        currentClicks = 0;
        onda = null;
    }

    /* Nasce seco: o pneu já aparece sem água (pegável na hora, como o
     * esvaziado — a chuva o enche, e dá para esvaziá-lo de novo). */
    protected override void AoNascerVazio()
    {
        if (water != null)
            water.SetActive(false);
    }

    /* Vai para os braços: guarda de onde veio, desliga os colliders (não
     * empurra o agente nem bloqueia a mira), esconde a água (o disco deitado
     * taparia o furo com o pneu de pé; ela volta ao Largar) e vira filho do
     * ponto. A onda recomeça do nível dos passos já fechados. */
    internal void PrenderEm(Transform ponto)
    {
        onda = null;
        if (water != null)
            water.SetActive(false);
        paiOriginal = transform.parent;
        rotacaoOriginal = transform.rotation;

        // Desliga antes de medir: o raycast não pode bater no próprio pneu.
        // Só os que estavam ligados, para religar exatamente esses.
        desligados = new List<Collider>();
        foreach (Collider c in GetComponentsInChildren<Collider>(true))
        {
            if (!c.enabled)
                continue;
            c.enabled = false;
            desligados.Add(c);
        }

        alturaDoPivo = AcharChao(transform.position, ~0, out RaycastHit chao) ? chao.distance : 0f;

        // A pose fina nos braços é a do PontoDeCarga.
        transform.SetParent(ponto, false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        Carregado = true;
    }

    /* Onde o pneu vai parar se for largado agora, sem mexer nele: a pose
     * (posição e giro) e a caçamba em que ele cairia (o raio do peito ou o
     * do chão acertou uma, ou a cabine da picape dela; null = nenhuma).
     * raioDoAgente é o raio do corpo do agente (CharacterController, com a
     * pele).
     *   - De pé (o giro de quando estava no chão), virado só em torno do
     *     vertical para o eixo da roda apontar para a frente.
     *   - A borda de cá fica FolgaDoAgente além do corpo do agente: o
     *     collider religado não pode nascer dentro do CharacterController.
     *   - Com um muro (parede de casa, borda invisível) antes da borda de
     *     lá, recua para o lado de cá dele: do outro lado a mira não
     *     alcança e a partida não fecha.
     *   - Assentado no chão por raycast (AcharChao: o corpo do agente não
     *     conta), com o pivô na altura medida ao pegar; sem chão embaixo
     *     (beira de muro, borda do mapa), no pé. */
    internal void OndeCai(Vector3 pes, Vector3 frente, float raioDoAgente,
                          out Vector3 posicao, out Quaternion giro, out Cacamba cacamba)
    {
        cacamba = null;
        giro = rotacaoOriginal;

        // Nos braços os colliders do pneu estão desligados: os raios não
        // batem nele.
        int mascara = MascaraDoMundo();
        Vector3 adiante = new Vector3(frente.x, 0f, frente.z);
        float avanco = 0f;
        if (adiante.sqrMagnitude > 1e-6f)
        {
            adiante.Normalize();
            giro = DeLadoPara(adiante);
            MedirNaDirecao(giro, adiante, out float perto, out float longe);
            avanco = raioDoAgente + FolgaDoAgente - perto;

            // Algo entre o agente e a borda de lá: recua para o lado de cá,
            // com folga (o raio do chão não pode nascer dentro do muro). Se
            // for a caçamba, o agente está encostado nela.
            if (Physics.Raycast(pes + Vector3.up * AlturaDoPeito, adiante, out RaycastHit muro,
                                avanco + longe + FolgaDoMuro, mascara, QueryTriggerInteraction.Ignore))
            {
                avanco = Mathf.Max(0f, muro.distance - FolgaDoMuro - longe);
                cacamba = CacambaDe(muro.collider);
            }
        }

        Vector3 origem = pes + adiante * avanco + Vector3.up * AlturaDoRaio;
        if (AcharChao(origem, mascara, out RaycastHit chao))
        {
            posicao = chao.point + Vector3.up * alturaDoPivo;
            if (cacamba == null)
                cacamba = CacambaDe(chao.collider);
        }
        else
        {
            posicao = pes;
        }
    }

    /* O chão abaixo de 'origem': o acerto mais perto fora do corpo do agente.
     * O CharacterController fica na camada Default, e a máscara não o tira:
     * colado num muro, o pneu recua para perto do agente, e o raio desce por
     * dentro do corpo; pararia nele, e o pneu ficaria flutuando acima da
     * cabeça. */
    private static bool AcharChao(Vector3 origem, int mascara, out RaycastHit chao)
    {
        chao = default;
        bool achou = false;
        foreach (RaycastHit acerto in Physics.RaycastAll(origem, Vector3.down, AlcanceDoChao,
                                                         mascara, QueryTriggerInteraction.Ignore))
        {
            if (acerto.collider.GetComponentInParent<ControladorJogador>() != null)
                continue;
            if (achou && acerto.distance >= chao.distance)
                continue;
            chao = acerto;
            achou = true;
        }
        return achou;
    }

    /* A caçamba deste collider: a dele (a carroceria) ou, na cabine da
     * picape (o PontoDeReabastecimento), a Cacamba filha. */
    private static Cacamba CacambaDe(Collider atingido)
    {
        Cacamba cacamba = atingido.GetComponentInParent<Cacamba>();
        if (cacamba != null)
            return cacamba;

        PontoDeReabastecimento ponto = atingido.GetComponentInParent<PontoDeReabastecimento>();
        return ponto != null ? ponto.CacambaDaCarroceria : null;
    }

    /* Sai dos braços e assenta na pose dada (a do OndeCai), com os colliders
     * de volta. */
    internal void Largar(Vector3 posicao, Quaternion giro)
    {
        Soltar();
        if (water != null)
            water.SetActive(!Esvaziado);
        transform.SetPositionAndRotation(posicao, giro);
        ReligarColliders();
    }

    /* O giro de quando estava no chão (em pé), virado só em torno do
     * vertical para o eixo da roda apontar para 'adiante'. O eixo vale nos
     * dois sentidos, então o giro é sempre o menor (até 90°). Deitado (eixo
     * a mais de 60° da horizontal), girar no vertical não afina nada: fica
     * como estava. */
    private Quaternion DeLadoPara(Vector3 adiante)
    {
        Vector3 eixo = rotacaoOriginal * EixoDaRoda;
        eixo.y = 0f;
        if (eixo.sqrMagnitude < 0.25f)
            return rotacaoOriginal;

        float angulo = Vector3.SignedAngle(eixo, adiante, Vector3.up);
        if (angulo > 90f)
            angulo -= 180f;
        else if (angulo < -90f)
            angulo += 180f;
        return Quaternion.AngleAxis(angulo, Vector3.up) * rotacaoOriginal;
    }

    /* Até onde o pneu vai, a partir do pivô, para trás (perto ≤ 0) e para a
     * frente (longe ≥ 0) na direção dada, se estiver com o giro dado: pelos
     * cantos das caixas locais dos Renderers ligados (a água, já desligada,
     * não conta). A medida sai da pose de agora (nos braços), virada para o
     * giro dado. Sem Renderer, só o pivô (0 e 0). */
    private void MedirNaDirecao(Quaternion giro, Vector3 direcao, out float perto, out float longe)
    {
        perto = 0f;
        longe = 0f;
        Quaternion virar = giro * Quaternion.Inverse(transform.rotation);
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            if (!r.enabled)
                continue;

            Bounds caixa = r.localBounds;
            for (int i = 0; i < 8; i++)
            {
                Vector3 canto = caixa.center + Vector3.Scale(caixa.extents,
                    new Vector3((i & 1) == 0 ? -1f : 1f, (i & 2) == 0 ? -1f : 1f, (i & 4) == 0 ? -1f : 1f));
                float d = Vector3.Dot(virar * (r.transform.TransformPoint(canto) - transform.position), direcao);
                perto = Mathf.Min(perto, d);
                longe = Mathf.Max(longe, d);
            }
        }
    }

    /* Entregue na caçamba da picape: sai dos braços, reserva a próxima vaga
     * e conclui (o OnComplete arremessa até a vaga). Os colliders ficam
     * desligados: empilhado na carroceria, o pneu não pode tapar a mira
     * da caçamba. Sem vaga livre, o pneu encolhe AlturaSemVaga acima do
     * pivô da caçamba (no pivô ficaria escondido na carroceria), e o
     * Receber sai na hora. */
    internal void EntregarEm(Cacamba cacamba)
    {
        Soltar();
        desligados = null;
        cacambaDeDestino = cacamba;
        VagaNaCacamba = cacamba.ReservarVaga(out numeroNaCacamba);
        if (VagaNaCacamba == null)
            transform.position = cacamba.transform.position + Vector3.up * AlturaSemVaga;

        Concluir();

        if (VagaNaCacamba == null)
            cacamba.Receber(this, numeroNaCacamba);
    }

    /* Com vaga, o arremesso até ela (instantâneo fora do Play), sem
     * encolher. Sem vaga (ou no caminho antigo, sem caçamba), o encolher
     * padrão. */
    protected override void OnComplete()
    {
        if (VagaNaCacamba == null)
        {
            base.OnComplete();
            return;
        }

        if (Application.isPlaying && isActiveAndEnabled)
        {
            StartCoroutine(ArremessoRoutine(VagaNaCacamba, cacambaDeDestino));
            return;
        }

        AssentarNaVaga(VagaNaCacamba);
        cacambaDeDestino.Receber(this, numeroNaCacamba);
    }

    /* Arco das mãos até a vaga: a posição segue a reta com uma parábola de
     * alturaDoArco por cima, e o giro vai até o da vaga. No fim, assenta,
     * estoura as partículas e avisa a caçamba. */
    private IEnumerator ArremessoRoutine(Transform vaga, Cacamba cacamba)
    {
        Vector3 inicio = transform.position;
        Quaternion giroInicial = transform.rotation;
        float duracao = cacamba.DuracaoArremesso;

        for (float t = 0f; t < duracao; t += Time.deltaTime)
        {
            float k = t / duracao;
            Vector3 reta = Vector3.Lerp(inicio, vaga.position, k);
            transform.position = reta + Vector3.up * (cacamba.AlturaDoArco * 4f * k * (1f - k));
            transform.rotation = Quaternion.Slerp(giroInicial, vaga.rotation, k);
            yield return null;
        }

        AssentarNaVaga(vaga);
        EstourarParticulas(transform.position);
        cacamba.Receber(this, numeroNaCacamba);
    }

    /* Filho da vaga, com pose local zero (mantém o tamanho de mundo). */
    private void AssentarNaVaga(Transform vaga)
    {
        transform.SetParent(vaga, true);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    /* Sai dos braços e volta ao pai de antes (o grupo dos focos na
     * hierarquia), mantendo a posição de mundo até ser posicionado. */
    private void Soltar()
    {
        onda = null;
        transform.SetParent(paiOriginal, true);
        Carregado = false;
    }

    /* A mesma máscara da mira do Interactor: tudo menos o corpo do agente.
     * O NameToLayer não pode ir em inicializador de campo de MonoBehaviour
     * (ver Interactor.cs), por isso é lido aqui, na hora de largar. */
    private static int MascaraDoMundo()
    {
        int corpo = LayerMask.NameToLayer("CorpoJogador");
        return corpo >= 0 ? ~(1 << corpo) : ~0;
    }

    private void ReligarColliders()
    {
        if (desligados == null)
            return;

        foreach (Collider c in desligados)
        {
            if (c != null)
                c.enabled = true;
        }
        desligados = null;
    }
}
