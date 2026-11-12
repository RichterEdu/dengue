using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* ============================================================
 *  CaixaDaguaInteractable.cs
 *  ---------------------------------------------------------------------
 *  Foco do tipo CAIXA D'ÁGUA DESTAMPADA — herda Interactable.
 *
 *  Comportamento (GameDoc: "caixa d'água destampada -> interagir ->
 *  caixa tampada"):
 *  - A caixa começa ABERTA, com a tampa encostada na lateral e a água
 *    parada à mostra lá dentro.
 *  - A cada clique a tampa desliza um pouco em direção à boca da caixa
 *    (posição e rotação interpoladas), como se o agente estivesse
 *    empurrando ela para o lugar.
 *  - No último clique a tampa assenta, a caixa fica fechada e o foco é
 *    eliminado (evento OnCompleted é disparado).
 *  - A caixa CONTINUA na cena (não encolhe/some), a menos que
 *    removeOnComplete seja marcado.
 *
 *  Kit do agente: a tampa é CARREGÁVEL, como o pneu (não é minigame).
 *  - Com as Mãos e a tampa encostada, um passo (instantâneo) a põe nos
 *    braços (KitDoAgente.PegarTampa → PrenderTampaEm, Pegou): ela vira
 *    filha do PontoDeCarga, na pose posNosBracos/rotNosBracos, com o
 *    tamanho de mundo dela e os colliders desligados.
 *  - Com a tampa nos braços DESTE kit (AceitaComMaosOcupadas), segurar
 *    segundosParaTampar é um passo só que conclui, qualquer que seja o
 *    requiredClicks: a tampa volta a ser filha da caixa e faz o arco do
 *    MoveRoutine até a boca (fora do Play, assenta na hora).
 *  - O botão de largar (KitDoAgente.DevolverTampa → DevolverTampaALateral)
 *    a leva de volta à lateral (posAberta/rotAberta, Fechamento 0).
 *  - Nos dois casos o pai, a escala local e os colliders de antes voltam.
 *  - Com a tampa nos braços a chuva não reenche a caixa.
 *  No caminho antigo Interact() (sem kit), a tampa continua deslizando
 *  um pouco a cada clique.
 *
 *  O prefab é gerado pelo menu  Dengue > Criar Caixa d'Agua
 *  (Assets/Editor/CaixaDaguaBuilder.cs), que já preenche as poses.
 * ============================================================ */

public class CaixaDaguaInteractable : Interactable
{
    [Header("Caixa d'Água")]
    [Tooltip("Objeto filho com a tampa da caixa.")]
    [SerializeField] private Transform tampa;

    [Tooltip("Posição local da tampa com a caixa ABERTA (encostada na lateral).")]
    [SerializeField] private Vector3 posAberta = new Vector3(0.44f, 0.45f, 0f);

    [Tooltip("Rotação local (euler) da tampa com a caixa ABERTA.")]
    [SerializeField] private Vector3 rotAberta = new Vector3(0f, 0f, 62f);

    [Tooltip("Posição local da tampa com a caixa FECHADA (assentada na boca).")]
    [SerializeField] private Vector3 posFechada = new Vector3(0f, 0.762f, 0f);

    [Tooltip("Rotação local (euler) da tampa com a caixa FECHADA.")]
    [SerializeField] private Vector3 rotFechada = Vector3.zero;

    [Tooltip("Duração da animação da tampa a cada clique.")]
    [SerializeField] private float duracaoDoMovimento = 0.3f;

    [Tooltip("Altura extra do arco que a tampa descreve no caminho (0 = desliza reto).")]
    [SerializeField] private float alturaDoArco = 0.20f;

    [Tooltip("Objeto filho com a água parada (opcional). Some quando a caixa fecha.")]
    [SerializeField] private GameObject agua;

    [Tooltip("Se marcado, após fechar a caixa segue o fluxo padrão (encolhe e some).")]
    [SerializeField] private bool removeOnComplete = false;

    [Header("Tampa carregável (kit)")]
    [Tooltip("Segundos segurando o botão, com a tampa nos braços, para tampar a caixa.")]
    [SerializeField] private float segundosParaTampar = 0.8f;

    [Tooltip("Posição local da tampa nos braços (filha do PontoDeCarga do kit).")]
    [SerializeField] private Vector3 posNosBracos = Vector3.zero;

    [Tooltip("Rotação local (euler) da tampa nos braços.")]
    [SerializeField] private Vector3 rotNosBracos = Vector3.zero;

    // Aviso dos pontos de uso (lixeira, picape) com a tampa nos braços.
    internal const string RecusaComATampa = "Ponha a tampa na <b>caixa d'água</b> antes.";

    private Coroutine moveRoutine;

    // Guardados no PrenderTampaEm para devolver a tampa à caixa.
    private Transform paiDaTampa;
    private Vector3 escalaDaTampa;
    private List<Collider> desligados;   // os colliders da tampa que o PrenderTampaEm desligou

    /* 0 = tampa encostada na lateral, 1 = caixa fechada. */
    public float Fechamento { get; private set; }

    /* A tampa está nos braços de um agente (KitDoAgente.TampaCarregada). */
    public bool TampaNosBracos { get; private set; }

    /* Com a tampa já em movimento ou nos braços, a chuva não reenche: a
     * água voltaria numa caixa que está sendo fechada. */
    public override bool PodeReencher => base.PodeReencher && CurrentClicks == 0 && !TampaNosBracos;

    /* Pegar a tampa é na hora; tampar, segurando segundosParaTampar. */
    public override float SegundosPorPasso => TampaNosBracos ? segundosParaTampar : 0f;

    /* Sem kit (Interactor sem KitDoAgente), segurar continua o caminho
     * antigo, clique a clique: o tempo da base (segundosParaConcluir dividido
     * pelos cliques), e não um passo por aperto. Com kit, como acima. */
    public override float SegundosPorPassoCom(KitDoAgente kit)
    {
        if (kit == null)
            return segundosParaConcluir / Mathf.Max(1, RequiredClicks);
        return base.SegundosPorPassoCom(kit);
    }

    /* Tempo previsto da ação (BalanceamentoSimulador): pegar a tampa e tampar. */
    public override float SegundosEstimados => 1.5f;

    /* A barra é só o segurar de tampar (um passo, sem cliques). */
    public override float ProgressoCom(KitDoAgente kit, float fracao) => Mathf.Clamp01(fracao);

    /* Com a tampa desta caixa nos braços, o passo (tampar) vale. */
    protected override bool AceitaComMaosOcupadas(KitDoAgente kit) => kit.TampaCarregada == this;

    /* Com kit: a tampa encostada vai para os braços (Pegou); nos braços
     * deste kit, um passo só tampa e conclui (pelo passo normal da base, com
     * os eventos e a conta da carga). Sem kit, o passo de sempre. */
    public override ResultadoDoPasso AvancarCom(KitDoAgente kit)
    {
        if (kit == null)
            return base.AvancarCom(kit);

        ResultadoDoPasso conferido = Verificar(kit);
        if (conferido != ResultadoDoPasso.Avancou)
            return conferido;

        if (kit.TampaCarregada == this)
        {
            // As mãos ficam livres antes do OnCompleted (como no pneu).
            kit.SoltarTampaColocada();
            RecolocarTampa();
            currentClicks = Mathf.Max(0, RequiredClicks - 1);
            return base.AvancarCom(kit);
        }

        return kit.PegarTampa(this) ? ResultadoDoPasso.Pegou : ResultadoDoPasso.MaosOcupadas;
    }

    /* Vai para os braços: guarda o pai e a escala local, desliga os
     * colliders ligados (não empurram o agente nem tapam a mira) e vira
     * filha do ponto, com o tamanho de mundo, na pose dos braços. */
    internal void PrenderTampaEm(Transform ponto)
    {
        TampaNosBracos = true;
        if (tampa == null)
            return;

        PararMovimento();
        paiDaTampa = tampa.parent;
        escalaDaTampa = tampa.localScale;

        desligados = new List<Collider>();
        foreach (Collider c in tampa.GetComponentsInChildren<Collider>(true))
        {
            if (!c.enabled)
                continue;
            c.enabled = false;
            desligados.Add(c);
        }

        tampa.SetParent(ponto, true);
        tampa.localPosition = posNosBracos;
        tampa.localRotation = Quaternion.Euler(rotNosBracos);
    }

    /* De volta à lateral da caixa: aberta, como no começo. A água fica
     * como estava (a caixa nascida seca continua seca). */
    internal void DevolverTampaALateral()
    {
        TampaNosBracos = false;
        Fechamento = 0f;
        if (tampa == null)
            return;

        VoltarParaACaixa(false);
        tampa.localPosition = PosicaoEm(0f);
        tampa.localRotation = RotacaoEm(0f);
    }

    /* Tampando: a tampa volta a ser filha da caixa no lugar em que está (de
     * onde parte o arco do MoveRoutine até a boca). */
    private void RecolocarTampa()
    {
        TampaNosBracos = false;
        if (tampa != null)
            VoltarParaACaixa(true);
    }

    private void VoltarParaACaixa(bool manterNoMundo)
    {
        tampa.SetParent(paiDaTampa, manterNoMundo);
        tampa.localScale = escalaDaTampa;

        if (desligados != null)
        {
            foreach (Collider c in desligados)
            {
                if (c != null)
                    c.enabled = true;
            }
            desligados = null;
        }
    }

    protected override void OnStart()
    {
        Fechamento = 0f;
        if (agua != null)
            agua.SetActive(true);
        AplicarPose(0f);
    }

    protected override void OnInteract()
    {
        float alvo = RequiredClicks <= 0 ? 1f : (float)CurrentClicks / RequiredClicks;
        MoverTampa(alvo);
        base.OnInteract();
    }

    protected override void OnComplete()
    {
        // Garante o assentamento exato mesmo se a animação for interrompida.
        MoverTampa(1f);

        if (removeOnComplete)
            base.OnComplete();
        // Caso contrário, a caixa fica na cena, fechada.
    }

    /* Nasce seco: a caixa destampada já aparece sem água parada. */
    protected override void AoNascerVazio()
    {
        if (agua != null)
            agua.SetActive(false);
    }

    /* Reenchido (a chuva): a água volta na caixa aberta (nasceu seco). */
    protected override void AoReencher()
    {
        if (agua != null && Fechamento < 0.999f)
            agua.SetActive(true);
    }

    private void MoverTampa(float alvo)
    {
        Fechamento = Mathf.Clamp01(alvo);
        if (tampa == null)
            return;

        PararMovimento();

        // Fora do Play as corrotinas não andam: a pose vai direto ao alvo.
        if (duracaoDoMovimento <= 0.01f || !Application.isPlaying || !isActiveAndEnabled)
        {
            AplicarPose(Fechamento);
            return;
        }

        moveRoutine = StartCoroutine(MoveRoutine(Fechamento));
    }

    private void PararMovimento()
    {
        if (moveRoutine == null)
            return;

        StopCoroutine(moveRoutine);
        moveRoutine = null;
    }

    private IEnumerator MoveRoutine(float alvo)
    {
        Vector3 posInicial = tampa.localPosition;
        Quaternion rotInicial = tampa.localRotation;

        // De onde a pose atual "está" no caminho, para o arco continuar coerente.
        float inicio = EstimarProgresso(posInicial);

        for (float t = 0f; t < duracaoDoMovimento; t += Time.deltaTime)
        {
            float k = 1f - Mathf.Pow(1f - t / duracaoDoMovimento, 3f);   // ease-out
            float p = Mathf.Lerp(inicio, alvo, k);
            tampa.localPosition = Vector3.Lerp(posInicial, PosicaoEm(alvo), k) + Vector3.up * Arco(p);
            tampa.localRotation = Quaternion.Slerp(rotInicial, RotacaoEm(alvo), k);
            yield return null;
        }

        AplicarPose(alvo);
        moveRoutine = null;
    }

    private void AplicarPose(float p)
    {
        if (tampa == null)
            return;
        tampa.localPosition = PosicaoEm(p) + Vector3.up * Arco(p);
        tampa.localRotation = RotacaoEm(p);

        // Com a caixa fechada a água some (já ficaria escondida, mas evita
        // z-fighting da lâmina d'água com a tampa e economiza um draw).
        if (agua != null)
            agua.SetActive(p < 0.999f);
    }

    private Vector3 PosicaoEm(float p)
    {
        return Vector3.Lerp(posAberta, posFechada, p);
    }

    private Quaternion RotacaoEm(float p)
    {
        return Quaternion.Slerp(Quaternion.Euler(rotAberta), Quaternion.Euler(rotFechada), p);
    }

    /* Levanta a tampa no meio do caminho: ela sobe e assenta, em vez de
     * atravessar a parede da caixa. */
    private float Arco(float p)
    {
        return alturaDoArco * Mathf.Sin(Mathf.Clamp01(p) * Mathf.PI);
    }

    /* Onde a pose atual está entre aberta e fechada (usado para continuar
     * o arco de onde a tampa parou). */
    private float EstimarProgresso(Vector3 posAtual)
    {
        Vector3 caminho = posFechada - posAberta;
        float mag = caminho.sqrMagnitude;
        if (mag < 1e-6f)
            return Fechamento;
        return Mathf.Clamp01(Vector3.Dot(posAtual - posAberta, caminho) / mag);
    }
}
