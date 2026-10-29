using System.Collections.Generic;
using UnityEngine;

/* ============================================================
 *  PneuInteractable.cs
 *  ---------------------------------------------------------------------
 *  Foco do tipo PNEU — herda a base Interactable e adiciona o
 *  comportamento específico: depois de alguns cliques, a ÁGUA é
 *  retirada (o filho "agua do pneu" é desativado).
 *
 *  Kit do agente (Parte A): o pneu não conclui sozinho.
 *  - Com as Mãos, o último passo esvazia a água (Esvaziado) e o pneu
 *    vai para os braços do agente (KitDoAgente.Pegar → PrenderEm): fica
 *    no PontoDeCarga, com os colliders desligados (Carregado). Não
 *    encolhe nos passos (Interactable): é carregado do tamanho dele.
 *  - Largar: OndeCai() calcula a pose sem mexer no pneu, e Largar()
 *    assenta nela. O pneu cai de pé, com o eixo da roda apontado para a
 *    frente do agente (assim ocupa só a largura da banda na direção dele,
 *    e não o diâmetro), afastado pelo próprio tamanho: a borda de cá fica
 *    fora do corpo do agente. Com um muro ou parede na frente, fica do
 *    lado de cá; sem chão (beira de muro, borda do mapa), cai no pé dele.
 *    Largado, continua sendo foco (alcançável pela mira), e pegar de novo
 *    é instantâneo. Se ele cairia numa caçamba, o KitDoAgente entrega em
 *    vez de largar.
 *  - Entregar() (na caçamba) chama Concluir(): só aí o foco conta como
 *    eliminado, e o pneu segue o fluxo padrão de encolher até sumir.
 *  - Esvaziado, o pneu já não zumbe (SomDosFocos).
 *
 *  No caminho antigo Interact() (sem kit), o último clique conclui o
 *  pneu como antes.
 * ============================================================ */

public class PneuInteractable : Interactable
{
    [Header("Pneu")]
    [Tooltip("Objeto filho que representa a água parada dentro do pneu.")]
    [SerializeField] private GameObject water;

    [Tooltip("Após quantos cliques a água é removida.")]
    [SerializeField] private int clicksToDrainWater = 3;

    // Raycasts que procuram o chão (para baixo, até esta distância).
    private const float AlcanceDoChao = 4f;

    // Largar: o eixo da roda no modelo da cena (rubber tries.fbx) é o X
    // local: banda de ~0,6 m no X e disco de ~2 m no plano YZ, pivô no
    // centro. A borda de cá do pneu fica FolgaDoAgente além do corpo do
    // agente. O chão é procurado de cima (altura do raio) para baixo; um
    // muro na frente, na altura do peito, e a borda de lá fica a
    // FolgaDoMuro dele.
    private static readonly Vector3 EixoDaRoda = Vector3.right;
    private const float FolgaDoAgente = 0.15f;
    private const float AlturaDoRaio = 1.5f;
    private const float AlturaDoPeito = 1.0f;
    private const float FolgaDoMuro = 0.1f;

    // Guardados no PrenderEm para o Largar/Entregar.
    private Transform paiOriginal;
    private Quaternion rotacaoOriginal;
    private float alturaDoPivo;         // do pivô até o chão, para assentar ao largar
    private List<Collider> desligados;  // os colliders que o PrenderEm desligou

    /* A água já saiu (último passo dado). Daqui em diante pegar é instantâneo. */
    public bool Esvaziado { get; private set; }

    /* Está nos braços do agente (filho do PontoDeCarga). */
    public bool Carregado { get; private set; }

    /* Nos braços não é alvo. Esvaziado, continua alvo (para pegar de novo)
     * mesmo com os cliques já no total; antes disso, vale a regra da base. */
    public override bool CanInteract => !IsComplete && !Carregado && (Esvaziado || base.CanInteract);

    /* Sem água não há criadouro: o zumbido para, mesmo antes da caçamba. */
    public override bool Zumbe => !Esvaziado;

    /* Esvaziado, pegar de novo é na hora (0 = um passo por aperto). */
    public override float SegundosPorPasso => Esvaziado ? 0f : base.SegundosPorPasso;

    /* O último passo só põe o pneu nos braços; quem conclui é o Entregar. */
    protected override bool ConcluiSozinho => false;

    protected override void OnInteract()
    {
        // Ao atingir o número de cliques configurado, remove a água.
        if (water != null && CurrentClicks >= clicksToDrainWater && water.activeSelf)
            water.SetActive(false);

        base.OnInteract();
    }

    /* Já esvaziado, pegar de novo é um passo só, sem clique a mais. Senão,
     * o passo normal da base, que no último vira Pegou (pneu nos braços). */
    public override ResultadoDoPasso AvancarCom(KitDoAgente kit)
    {
        if (Esvaziado)
        {
            ResultadoDoPasso conferido = Verificar(kit);
            if (conferido != ResultadoDoPasso.Avancou)
                return conferido;

            // Sem kit não há braços para onde ir.
            return kit != null && kit.Pegar(this) ? ResultadoDoPasso.Pegou : ResultadoDoPasso.Bloqueado;
        }

        ResultadoDoPasso resultado = base.AvancarCom(kit);
        if (resultado == ResultadoDoPasso.Avancou && Carregado)
            return ResultadoDoPasso.Pegou;
        return resultado;
    }

    /* Último passo: a água sai e o pneu vai para os braços do agente. No
     * caminho antigo (kit null) conclui na hora, como antes do kit. */
    protected override void AoTerminarAcao(KitDoAgente kit)
    {
        Esvaziado = true;
        if (water != null)
            water.SetActive(false);

        if (kit == null)
        {
            Concluir();
            return;
        }
        kit.Pegar(this);
    }

    /* Vai para os braços: guarda de onde veio, desliga os colliders (não
     * empurra o agente nem bloqueia a mira) e vira filho do ponto. */
    internal void PrenderEm(Transform ponto)
    {
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

        alturaDoPivo = Physics.Raycast(transform.position, Vector3.down, out RaycastHit chao,
                                       AlcanceDoChao, ~0, QueryTriggerInteraction.Ignore)
            ? chao.distance
            : 0f;

        // A pose fina nos braços é a do PontoDeCarga.
        transform.SetParent(ponto, false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        Carregado = true;
    }

    /* Onde o pneu vai parar se for largado agora, sem mexer nele: a pose
     * (posição e giro) e a caçamba em que ele cairia (o raio do peito ou o
     * do chão acertou uma; null = nenhuma). raioDoAgente é o raio do corpo
     * do agente (CharacterController, com a pele).
     *   - De pé (o giro de quando estava no chão), virado só em torno do
     *     vertical para o eixo da roda apontar para a frente.
     *   - A borda de cá fica FolgaDoAgente além do corpo do agente: o
     *     collider religado não pode nascer dentro do CharacterController.
     *   - Com um muro (parede de casa, borda invisível) antes da borda de
     *     lá, recua para o lado de cá dele: do outro lado a mira não
     *     alcança e a partida não fecha.
     *   - Assentado no chão por raycast, com o pivô na altura medida ao
     *     pegar; sem chão embaixo (beira de muro, borda do mapa), no pé. */
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
                cacamba = muro.collider.GetComponentInParent<Cacamba>();
            }
        }

        Vector3 origem = pes + adiante * avanco + Vector3.up * AlturaDoRaio;
        if (Physics.Raycast(origem, Vector3.down, out RaycastHit chao,
                            AlcanceDoChao, mascara, QueryTriggerInteraction.Ignore))
        {
            posicao = chao.point + Vector3.up * alturaDoPivo;
            if (cacamba == null)
                cacamba = chao.collider.GetComponentInParent<Cacamba>();
        }
        else
        {
            posicao = pes;
        }
    }

    /* Sai dos braços e assenta na pose dada (a do OndeCai), com os colliders
     * de volta. */
    internal void Largar(Vector3 posicao, Quaternion giro)
    {
        Soltar();
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

    /* Entregue na caçamba: solta em 'destino' e conclui (o encolher padrão
     * roda ali, no OnComplete). */
    internal void Entregar(Vector3 destino)
    {
        Soltar();
        transform.position = destino;
        ReligarColliders();
        Concluir();
    }

    /* Sai dos braços e volta ao pai de antes (o grupo dos focos na
     * hierarquia), mantendo a posição de mundo até ser posicionado. */
    private void Soltar()
    {
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
