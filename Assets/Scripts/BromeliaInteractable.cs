using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* ============================================================
 *  BromeliaInteractable.cs
 *  ---------------------------------------------------------------------
 *  Foco do tipo BROMÉLIA — herda Interactable.
 *
 *  Comportamento ("bromélia com água -> interagir -> bromélia seca"):
 *  - A planta começa com água parada no copo central e em pocinhas
 *    entre as folhas. Nasce sempre com água (SempreNasceComAgua), fora
 *    do sorteio do nascimento seco, como o ralo.
 *  - A cada clique algumas pocinhas secam (encolhem e somem), das de
 *    fora para a do centro.
 *  - No último clique não sobra água nenhuma. A planta CONTINUA na cena:
 *    o certo é esvaziar a bromélia, não arrancá-la.
 *
 *  Minigame do borrifador (vista de perto): com a água sanitária, o
 *  clique começa uma SessaoDaBromelia (CriarMinigame, uma
 *  PontosDeBorrifar) no lugar do segurar. A câmera vai para cima do copo
 *  central (a última de 'pocas'): alturaDaCamera acima dele, recuada
 *  recuoDaCamera para o lado do agente, olhando para o copo, com a base
 *  da tela do lado do agente e o FOV fovDePerto (PoseDaVistaDePerto). As
 *  pocinhas com água viram bolinhas na tela (a projeção delas nessa
 *  pose), uma por vez: as de entre as folhas embaralhadas pela semente e
 *  o copo por último, no centro do campo. Cada bolinha acertada seca a
 *  pocinha dela (AoAtualizarMinigame); os passos são os cliques que
 *  faltam. O último acerto só conclui depois do fecho (o tempo de o
 *  último borrifo aparecer); o passo que conclui seca o que sobrou.
 *  Enquanto a vista de perto durar, e até a câmera voltar aos olhos, o
 *  enxame de mosquitos da planta fica escondido (a câmera atravessa o
 *  lugar dele).
 *
 *  Sair, o bloqueio, a recusa no meio e a planta desativada largam a
 *  sessão e guardam as secas; a próxima sessão cobre só o que falta.
 *  Garantia de água: se a interrupção chega no fecho (tudo seco, mas o
 *  passo final ainda não saiu), o copo volta a ter água, para o foco
 *  nunca ficar sem pocinha e sem estar concluído.
 *
 *  Sem sessão (o caminho antigo Interact(), ou AvancarCom direto), a
 *  ordem fixa continua, pulando as que já estão secas. Fora do Play a
 *  pocinha some na hora (as corrotinas não andam).
 *
 *  O modelo vem do Blender (Ferramentas/Blender/gerar_focos.py) e o
 *  prefab é gerado pelo menu  Dengue > Criar Bromélia  (FocosBlenderBuilder.cs).
 * ============================================================ */

public class BromeliaInteractable : Interactable
{
    [Header("Bromélia")]
    [Tooltip("Poças d'água, na ordem em que secam (a última deve ser a do centro).")]
    [SerializeField] private GameObject[] pocas = new GameObject[0];

    [Tooltip("Duração da animação de cada poça secando.")]
    [SerializeField] private float duracaoSecar = 0.3f;

    [Tooltip("Se marcado, após secar segue o fluxo padrão (encolhe e some).")]
    [SerializeField] private bool removeOnComplete = false;

    [Header("Vista de perto (minigame)")]
    [Tooltip("Metros da câmera acima do copo central.")]
    [SerializeField] private float alturaDaCamera = 0.58f;

    [Tooltip("Metros que a câmera recua do copo para o lado do agente.")]
    [SerializeField] private float recuoDaCamera = 0.17f;

    [Tooltip("Campo de visão vertical da câmera de perto, em graus.")]
    [SerializeField] private float fovDePerto = 40f;

    [Tooltip("Raio da bolinha, em unidades do campo (a altura da tela é 2); o acerto usa o mesmo.")]
    [SerializeField] private float raioDaBolinha = PontosDeBorrifar.RaioPadrao;

    [Tooltip("Segundos para o anel da bolinha fechar.")]
    [SerializeField] private float segundosDoAnel = PontosDeBorrifar.AnelPadrao;

    [Tooltip("Segundos que a bolinha some depois de o anel fechar.")]
    [SerializeField] private float pausaDaBolinha = PontosDeBorrifar.PausaPadrao;

    [Tooltip("Segundos sem bolinha no começo (a câmera chegando).")]
    [SerializeField] private float esperaInicial = PontosDeBorrifar.EsperaPadrao;

    [Tooltip("Segundos entre o último acerto e a conclusão (o último borrifo aparecer).")]
    [SerializeField] private float fecho = PontosDeBorrifar.FechoPadrao;

    [Tooltip("Quanto a mira anda no campo por grau de movimento do mouse/analógico.")]
    [SerializeField] private float miraPorGrau = PontosDeBorrifar.MiraPorGrauPadrao;

    private const string DicaDeBorrifar = "Clique nas bolinhas para borrifar   |   Botão direito: sair";
    private const string ConviteDeBorrifar = "Clique para borrifar as pocinhas";

    // Tempo de mirar e acertar cada bolinha (SegundosEstimados).
    private const float SegundosPorBolinha = 0.87f;

    // Pocinhas secas (inativas ou secando agora).
    private int secas;

    // Pocinhas no meio da animação de secar (ainda ativas, mas já secas),
    // com a escala de antes (para devolver a água).
    private readonly Dictionary<GameObject, Vector3> secando = new Dictionary<GameObject, Vector3>();

    // Sessão em andamento (null = nenhuma) e quantos acertos dela já secaram.
    private SessaoDaBromelia sessaoAtual;
    private int acertosSecos;

    private GameObject[] Pocas => pocas ?? new GameObject[0];

    /* Sem secar pela metade, a chuva enche de novo (nasceu seco e o
     * jogador não começou a secar: o resto secaria por cima da água). */
    public override bool PodeReencher => base.PodeReencher && CurrentClicks == 0;

    /* A bromélia nasce sempre com água, como o ralo: fica fora do sorteio
     * do nascimento seco do começo da partida (sem água, não há minigame). */
    public override bool SempreNasceComAgua => true;

    protected override void OnStart()
    {
        secas = 0;
        DevolverAguaATodas();
    }

    void OnDisable()
    {
        AoDesativar();
    }

    /* Minigame do borrifador: a água sanitária numa bromélia com água.
     * Fábrica pura. O agente é o dono do kit; a proporção é a da câmera. */
    public override IMinigame CriarMinigame(KitDoAgente kit, int semente)
    {
        if (!TemMinigame(kit))
            return null;

        Camera camera = Camera.main;
        if (camera == null)
            return null;

        return CriarSessao(kit.transform.position, kit.transform.forward, camera.aspect, semente);
    }

    /* A pose da vista de perto: acima do copo (a última de 'pocas'; sem
     * ela, o próprio objeto), recuada para o lado do agente e olhando para
     * o copo, com o topo da tela do lado oposto ao agente. Com o agente em
     * cima da planta, vale a frenteReserva (sem o y); ainda nula, o +z. */
    internal PoseDePerto PoseDaVistaDePerto(Vector3 agente, Vector3 frenteReserva)
    {
        GameObject[] lista = Pocas;
        GameObject ultima = lista.Length > 0 ? lista[lista.Length - 1] : null;
        Vector3 copo = ultima != null ? ultima.transform.position : transform.position;

        Vector3 frente = SemAltura(copo - agente);
        if (frente.sqrMagnitude < 1e-6f)
            frente = SemAltura(frenteReserva);
        if (frente.sqrMagnitude < 1e-6f)
            frente = Vector3.forward;
        frente.Normalize();

        Vector3 posicao = copo + Vector3.up * alturaDaCamera - frente * recuoDaCamera;
        Vector3 olhar = copo - posicao;
        if (olhar.sqrMagnitude < 1e-8f)
            olhar = Vector3.down;

        return new PoseDePerto
        {
            Posicao = posicao,
            Giro = Quaternion.LookRotation(olhar, frente),
            Fov = fovDePerto
        };
    }

    private static Vector3 SemAltura(Vector3 v) => new Vector3(v.x, 0f, v.z);

    /* A sessão com as pocinhas com água: as de entre as folhas na ordem
     * embaralhada pela semente, o copo (a última de 'pocas') por último.
     * Cada ponto é a pocinha no campo da pose; os passos são os que
     * faltam; a proporção vira a meia-largura do campo. Fábrica pura. */
    internal SessaoDaBromelia CriarSessao(Vector3 agente, Vector3 frenteReserva, float proporcao, int semente)
    {
        PoseDePerto pose = PoseDaVistaDePerto(agente, frenteReserva);
        GameObject[] lista = Pocas;
        int copo = lista.Length - 1;

        List<int> indices = new List<int>();
        for (int i = 0; i < copo; i++)
            if (Ativa(i)) indices.Add(i);

        System.Random sorteio = new System.Random(semente);
        for (int i = indices.Count - 1; i > 0; i--)
        {
            int j = sorteio.Next(i + 1);
            (indices[i], indices[j]) = (indices[j], indices[i]);
        }

        if (copo >= 0 && Ativa(copo))
            indices.Add(copo);

        Vector2[] pontos = new Vector2[indices.Count];
        for (int k = 0; k < pontos.Length; k++)
            pontos[k] = pose.NoCampo(lista[indices[k]].transform.position);

        int passos = Mathf.Max(1, RequiredClicks - CurrentClicks);
        return new SessaoDaBromelia(pontos, indices.ToArray(), pose, passos, raioDaBolinha, segundosDoAnel,
                                    pausaDaBolinha, esperaInicial, fecho, proporcao, miraPorGrau);
    }

    /* Índice em 'pocas' do ponto k da sessão em andamento; −1 sem sessão ou fora do intervalo. */
    internal int PocaDoPonto(int k)
    {
        if (sessaoAtual == null || k < 0 || k >= sessaoAtual.Pocas.Length)
            return -1;
        return sessaoAtual.Pocas[k];
    }

    public override string ConviteDoMinigame(KitDoAgente kit) => TemMinigame(kit) ? ConviteDeBorrifar : null;

    public override string DicaDoMinigame => DicaDeBorrifar;

    /* A espera inicial, o fecho e o tempo de acertar cada bolinha. */
    public override float SegundosEstimados
    {
        get
        {
            int n = Pocas.Length;
            if (n == 0)
                return base.SegundosEstimados;
            return esperaInicial + fecho + SegundosPorBolinha * n;
        }
    }

    private bool TemMinigame(KitDoAgente kit)
    {
        return kit != null && AceitaFerramenta(kit.Selecionada) && !IsComplete && TemPocaAtiva();
    }

    /* Guarda a sessão nova (larga a anterior) e esconde o enxame. */
    public override void AoIniciarMinigame(IMinigame sessao)
    {
        LargarSessao();
        if (!(sessao is SessaoDaBromelia nova))
            return;

        sessaoAtual = nova;
        EsconderEnxame();
    }

    /* Todo quadro: cada acerto novo seca a pocinha do ponto dele. O enxame
     * é escondido de novo (ele pode nascer no meio da sessão). */
    public override void AoAtualizarMinigame(IMinigame sessao)
    {
        if (sessaoAtual == null || sessao != sessaoAtual)
            return;

        GameObject[] lista = Pocas;
        while (acertosSecos < sessaoAtual.Acertos && acertosSecos < sessaoAtual.Pocas.Length)
        {
            int i = sessaoAtual.Pocas[acertosSecos++];
            if (Ativa(i))
                SecarPoca(lista[i]);
        }
        EsconderEnxame();
    }

    /* Saída voluntária ou bloqueio: as secas ficam secas. */
    public override void AoSairDoMinigame()
    {
        LargarSessao();
        GarantirAgua();
    }

    /* A câmera voltou aos olhos: o enxame volta. */
    public override void AoVoltarDaVistaDePerto()
    {
        DevolverEnxame();
    }

    /* O que o OnDisable faz: larga a sessão em andamento. */
    internal void AoDesativar()
    {
        if (sessaoAtual != null)
        {
            LargarSessao();
            GarantirAgua();
        }
        DevolverEnxame();
    }

    /* Recusado no meio da sessão, o Interactor a encerra sem o
     * AoSairDoMinigame: larga a sessão aqui. (O passo que conclui já
     * largou no OnInteract.) */
    public override ResultadoDoPasso AvancarCom(KitDoAgente kit)
    {
        ResultadoDoPasso resultado = base.AvancarCom(kit);
        if (resultado != ResultadoDoPasso.Avancou && sessaoAtual != null)
        {
            LargarSessao();
            GarantirAgua();
        }
        return resultado;
    }

    protected override void OnInteract()
    {
        GameObject[] lista = Pocas;
        if (sessaoAtual != null)
        {
            // Com a sessão, quem seca são os acertos. No passo que conclui, o
            // que sobrou seca junto (a garantia).
            if (CurrentClicks >= RequiredClicks)
            {
                LargarSessao();
                foreach (GameObject p in lista)
                    if (p != null && p.activeSelf) p.SetActive(false);
            }
            secas = ContarSecas();
        }
        else
        {
            // Distribui as poças pelos cliques; no último clique seca o que
            // sobrou. Na ordem do array, pulando as que já estão secas.
            int alvo = RequiredClicks <= 0
                ? lista.Length
                : Mathf.CeilToInt((float)lista.Length * CurrentClicks / RequiredClicks);
            alvo = Mathf.Min(alvo, lista.Length);

            secas = ContarSecas();
            for (int i = 0; i < lista.Length && secas < alvo; i++)
            {
                if (!Ativa(i))
                    continue;
                SecarPoca(lista[i]);
                secas++;
            }
        }

        base.OnInteract();
    }

    protected override void OnComplete()
    {
        if (removeOnComplete)
            base.OnComplete();
        // Caso contrário, a bromélia fica na cena, seca.
    }

    /* Nasce seco: sem poça nenhuma, com o contador já no fim (o OnInteract
     * não secaria poça que não existe). */
    protected override void AoNascerVazio()
    {
        secas = Pocas.Length;
        foreach (GameObject p in Pocas)
            if (p != null) p.SetActive(false);
    }

    /* Reenchido (a chuva): as poças voltam inteiras (nasceu seco). */
    protected override void AoReencher()
    {
        secas = 0;
        DevolverAguaATodas();
    }

    /* A pocinha i tem água: existe, está ativa e não está secando. */
    private bool Ativa(int i)
    {
        GameObject[] lista = Pocas;
        return i >= 0 && i < lista.Length && lista[i] != null && lista[i].activeSelf && !secando.ContainsKey(lista[i]);
    }

    private bool TemPocaAtiva()
    {
        for (int i = 0; i < Pocas.Length; i++)
            if (Ativa(i)) return true;
        return false;
    }

    /* Secas = sem água (nula, inativa ou secando agora). */
    private int ContarSecas()
    {
        int contadas = 0;
        for (int i = 0; i < Pocas.Length; i++)
            if (!Ativa(i)) contadas++;
        return contadas;
    }

    /* Zera a sessão em andamento. O enxame continua escondido: a câmera
     * ainda vai atravessar o lugar dele na volta. */
    private void LargarSessao()
    {
        sessaoAtual = null;
        acertosSecos = 0;
    }

    /* Não concluída, ainda aceitando passos e sem pocinha nenhuma com água
     * (a interrupção no fecho): o copo volta a ter água. */
    private void GarantirAgua()
    {
        if (IsComplete || !CanInteract || TemPocaAtiva())
            return;

        GameObject[] lista = Pocas;
        GameObject copo = lista.Length > 0 ? lista[lista.Length - 1] : null;
        if (copo != null)
            DevolverAgua(copo);
    }

    /* A pocinha volta a ter água: sai de 'secando' com a escala de antes e
     * fica ativa (a corrotina dela para sem desativar). */
    private void DevolverAgua(GameObject poca)
    {
        if (poca == null)
            return;
        if (secando.TryGetValue(poca, out Vector3 escala))
        {
            secando.Remove(poca);
            poca.transform.localScale = escala;
        }
        poca.SetActive(true);
    }

    private void DevolverAguaATodas()
    {
        foreach (GameObject p in Pocas)
            DevolverAgua(p);
        secando.Clear();   // as que não estão mais em 'pocas'
    }

    private void EsconderEnxame()
    {
        Transform enxame = transform.Find(EnxameDeMosquitos.NomeDoFilho);
        if (enxame != null && enxame.gameObject.activeSelf)
            enxame.gameObject.SetActive(false);
    }

    private void DevolverEnxame()
    {
        Transform enxame = transform.Find(EnxameDeMosquitos.NomeDoFilho);
        if (enxame != null && !enxame.gameObject.activeSelf)
            enxame.gameObject.SetActive(true);
    }

    /* Seca uma pocinha: no Play, a animação; fora dele, na hora. */
    private void SecarPoca(GameObject poca)
    {
        if (poca == null || secando.ContainsKey(poca))
            return;
        if (Application.isPlaying && isActiveAndEnabled && duracaoSecar > 0f)
        {
            secando.Add(poca, poca.transform.localScale);
            StartCoroutine(Secar(poca));
            return;
        }
        poca.SetActive(false);
    }

    /* Encolhe a pocinha até sumir. Se ela sair de 'secando' no meio (a
     * água foi devolvida), para sem desativar. */
    private IEnumerator Secar(GameObject poca)
    {
        Transform t = poca.transform;
        Vector3 escala = t.localScale;
        for (float e = 0f; e < duracaoSecar; e += Time.deltaTime)
        {
            if (poca == null || !secando.ContainsKey(poca))
                yield break;
            float k = e / duracaoSecar;
            t.localScale = Vector3.Lerp(escala, new Vector3(0f, escala.y, 0f), k * k);
            yield return null;
        }
        if (poca == null || !secando.TryGetValue(poca, out Vector3 original))
            yield break;
        secando.Remove(poca);
        poca.SetActive(false);
        t.localScale = original;
    }
}

/* A sessão da bromélia: os pontos, a pose da câmera e qual pocinha é cada ponto. */
internal sealed class SessaoDaBromelia : PontosDeBorrifar, IMinigameDePerto
{
    public PoseDePerto Pose { get; }
    internal int[] Pocas { get; }   // Pocas[k] = índice em BromeliaInteractable.pocas do ponto k

    internal SessaoDaBromelia(Vector2[] pontos, int[] pocas, PoseDePerto pose, int passos,
                              float raio, float segundosDoAnel, float pausa, float esperaInicial,
                              float fecho, float meiaLargura, float miraPorGrau)
        : base(pontos, passos, raio, segundosDoAnel, pausa, esperaInicial, fecho, meiaLargura, miraPorGrau)
    {
        Pose = pose;
        Pocas = pocas ?? new int[0];
    }
}
