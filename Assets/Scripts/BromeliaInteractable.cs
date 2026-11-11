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
 *    entre as folhas.
 *  - A cada clique algumas pocinhas secam (encolhem e somem), das de
 *    fora para a do centro.
 *  - No último clique não sobra água nenhuma. A planta CONTINUA na cena:
 *    o certo é esvaziar a bromélia, não arrancá-la.
 *
 *  Minigame do borrifador: com a água sanitária, o clique começa uma
 *  MiraDoBorrifador (CriarMinigame) no lugar do segurar. O jogador aponta
 *  o borrifador para cada pocinha ATIVA e segura até ela secar; a sessão
 *  vê as pocinhas pelos ângulos (yaw, pitch) a partir do olho da câmera,
 *  no olhar do começo (CriarMira). A última de 'pocas' é o copo central,
 *  que demora mais. A cada quadro a pocinha encolhe no X e no Z com a
 *  Umidade e, seca, some (AoAtualizarMinigame). Com a sessão, o passo não
 *  seca na ordem fixa; o passo que conclui seca o que sobrou. Sair guarda
 *  as secas e devolve as que estavam a meio caminho à escala original; a
 *  próxima sessão cobre só o que falta. A névoa do bico e o som seguem o
 *  Agindo da sessão (Interactor.Avancando).
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

    [Header("Borrifar (minigame)")]
    [Tooltip("Segundos borrifando para secar cada pocinha entre as folhas.")]
    [SerializeField] private float segundosPorPoca = 0.5f;

    [Tooltip("Segundos borrifando para secar o copo central (a última de 'pocas').")]
    [SerializeField] private float segundosDoCopoCentral = 1.2f;

    [Tooltip("Graus entre a mira e a pocinha para o jato acertar.")]
    [SerializeField] private float raioDeAcerto = 4f;

    private const string DicaDeBorrifar = "Segure para borrifar as pocinhas   |   Botão direito: sair";
    private const string ConviteDeBorrifar = "Clique para borrifar as pocinhas";

    // Tempo de levar a mira de uma pocinha à outra (SegundosEstimados).
    private const float SegundosDeMirarPorPoca = 0.4f;

    // Pocinhas secas (inativas ou secando agora).
    private int secas;

    // Pocinhas no meio da animação de secar (ainda ativas, mas já secas).
    private readonly List<GameObject> secando = new List<GameObject>();

    // Sessão do borrifador (null = nenhuma). O índice k da sessão é a
    // pocinha pocas[indiceDaPoca[k]], que tinha a escala escalaOriginal[k].
    private MiraDoBorrifador mira;
    private readonly List<int> indiceDaPoca = new List<int>();
    private readonly List<Vector3> escalaOriginal = new List<Vector3>();

    private GameObject[] Pocas => pocas ?? new GameObject[0];

    /* Sem secar pela metade, a chuva enche de novo (nasceu seco e o
     * jogador não começou a secar: o resto secaria por cima da água). */
    public override bool PodeReencher => base.PodeReencher && CurrentClicks == 0;

    protected override void OnStart()
    {
        secas = 0;
        secando.Clear();
        foreach (GameObject p in Pocas)
            if (p != null) p.SetActive(true);
    }

    /* Minigame do borrifador: a água sanitária numa bromélia com água.
     * Fábrica pura. O olho é a câmera; o olhar é o do ControladorJogador
     * (o Interactor aplica a mira por cima dele), ou o da câmera sem ele. */
    public override IMinigame CriarMinigame(KitDoAgente kit, int semente)
    {
        if (!TemMinigame(kit))
            return null;

        Camera camera = Camera.main;
        if (camera == null)
            return null;

        ControladorJogador jogador = kit.GetComponent<ControladorJogador>();
        Quaternion visao = jogador != null
            ? Quaternion.Euler(jogador.Pitch, jogador.Yaw, 0f)
            : camera.transform.rotation;
        return CriarMira(camera.transform.position, visao);
    }

    /* A sessão com as pocinhas ativas, em graus (yaw, pitch) vistas do olho
     * com esta visão; pitch positivo = abaixo do centro (como o
     * ControladorJogador.Pitch). Os passos são os que faltam. */
    internal MiraDoBorrifador CriarMira(Vector3 olho, Quaternion visao)
    {
        GameObject[] lista = Pocas;
        Quaternion inversa = Quaternion.Inverse(visao);
        List<Vector2> angulos = new List<Vector2>();
        List<float> segundos = new List<float>();

        for (int i = 0; i < lista.Length; i++)
        {
            if (!Ativa(i))
                continue;

            Vector3 local = inversa * (lista[i].transform.position - olho);
            float yaw = Mathf.Atan2(local.x, local.z) * Mathf.Rad2Deg;
            float pitch = Mathf.Atan2(-local.y, Mathf.Sqrt(local.x * local.x + local.z * local.z)) * Mathf.Rad2Deg;
            angulos.Add(new Vector2(yaw, pitch));
            segundos.Add(i == lista.Length - 1 ? segundosDoCopoCentral : segundosPorPoca);
        }

        int passos = Mathf.Max(1, RequiredClicks - CurrentClicks);
        return new MiraDoBorrifador(angulos.ToArray(), segundos.ToArray(), passos, raio: raioDeAcerto);
    }

    public override string ConviteDoMinigame(KitDoAgente kit) => TemMinigame(kit) ? ConviteDeBorrifar : null;

    public override string DicaDoMinigame => DicaDeBorrifar;

    /* Borrifar cada pocinha (o copo central demora mais) e levar a mira de
     * uma à outra. */
    public override float SegundosEstimados
    {
        get
        {
            int n = Pocas.Length;
            if (n == 0)
                return base.SegundosEstimados;
            return (n - 1) * segundosPorPoca + segundosDoCopoCentral + SegundosDeMirarPorPoca * n;
        }
    }

    private bool TemMinigame(KitDoAgente kit)
    {
        return kit != null && AceitaFerramenta(kit.Selecionada) && !IsComplete && TemPocaAtiva();
    }

    /* Guarda o começo: quais pocinhas a sessão tem (as ativas, na ordem de
     * 'pocas', como no CriarMira) e a escala original de cada uma. */
    public override void AoIniciarMinigame(IMinigame sessao)
    {
        EncerrarSessao();
        if (!(sessao is MiraDoBorrifador nova))
            return;

        mira = nova;
        GameObject[] lista = Pocas;
        for (int i = 0; i < lista.Length; i++)
        {
            if (!Ativa(i))
                continue;
            indiceDaPoca.Add(i);
            escalaOriginal.Add(lista[i].transform.localScale);
        }
    }

    /* Todo quadro: cada pocinha encolhe no X e no Z com a Umidade; seca,
     * some, já com a escala original (a chuva a devolve inteira). */
    public override void AoAtualizarMinigame(IMinigame sessao)
    {
        if (mira == null || sessao != mira)
            return;

        int n = Mathf.Min(indiceDaPoca.Count, mira.Total);
        for (int k = 0; k < n; k++)
        {
            GameObject p = PocaDaSessao(k);
            if (p == null || !p.activeSelf)
                continue;

            Vector3 original = escalaOriginal[k];
            float umidade = mira.Umidade(k);
            if (umidade <= 0f)
            {
                p.SetActive(false);
                p.transform.localScale = original;
            }
            else
            {
                p.transform.localScale = new Vector3(original.x * umidade, original.y, original.z * umidade);
            }
        }
    }

    /* Saída voluntária ou bloqueio: as secas ficam secas; as que estavam a
     * meio caminho voltam inteiras. */
    public override void AoSairDoMinigame()
    {
        EncerrarSessao();
    }

    /* Recusado no meio da sessão, o Interactor a encerra sem o
     * AoSairDoMinigame: as pocinhas a meio caminho voltam já. (O passo que
     * conclui já encerrou no OnInteract.) */
    public override ResultadoDoPasso AvancarCom(KitDoAgente kit)
    {
        ResultadoDoPasso resultado = base.AvancarCom(kit);
        if (resultado != ResultadoDoPasso.Avancou)
            EncerrarSessao();
        return resultado;
    }

    protected override void OnInteract()
    {
        GameObject[] lista = Pocas;
        if (mira != null)
        {
            // Com a sessão, quem seca é a mira. No passo que conclui, o que
            // sobrou seca junto (a garantia).
            if (CurrentClicks >= RequiredClicks)
            {
                EncerrarSessao();
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
        secando.Clear();
        foreach (GameObject p in Pocas)
            if (p != null) p.SetActive(true);
    }

    /* A pocinha i tem água: existe, está ativa e não está secando. */
    private bool Ativa(int i)
    {
        GameObject[] lista = Pocas;
        return i >= 0 && i < lista.Length && lista[i] != null && lista[i].activeSelf && !secando.Contains(lista[i]);
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

    /* A pocinha do índice k da sessão (null se a lista mudou). */
    private GameObject PocaDaSessao(int k)
    {
        if (k < 0 || k >= indiceDaPoca.Count)
            return null;
        int i = indiceDaPoca[k];
        GameObject[] lista = Pocas;
        return i < lista.Length ? lista[i] : null;
    }

    /* Fim da sessão: as pocinhas ainda ativas voltam à escala original (as
     * secas já voltaram ao sumir). Pode ser chamado sem sessão. */
    private void EncerrarSessao()
    {
        for (int k = 0; k < indiceDaPoca.Count && k < escalaOriginal.Count; k++)
        {
            GameObject p = PocaDaSessao(k);
            if (p != null && p.activeSelf)
                p.transform.localScale = escalaOriginal[k];
        }
        mira = null;
        indiceDaPoca.Clear();
        escalaOriginal.Clear();
    }

    /* Seca uma pocinha na ordem fixa: no Play, a animação; fora dele, na hora. */
    private void SecarPoca(GameObject poca)
    {
        if (Application.isPlaying && isActiveAndEnabled && duracaoSecar > 0f)
        {
            secando.Add(poca);
            StartCoroutine(Secar(poca));
            return;
        }
        poca.SetActive(false);
    }

    private IEnumerator Secar(GameObject poca)
    {
        Transform t = poca.transform;
        Vector3 escala = t.localScale;
        for (float e = 0f; e < duracaoSecar; e += Time.deltaTime)
        {
            if (poca == null)
                yield break;
            float k = e / duracaoSecar;
            t.localScale = Vector3.Lerp(escala, new Vector3(0f, escala.y, 0f), k * k);
            yield return null;
        }
        secando.Remove(poca);
        if (poca == null)
            yield break;
        poca.SetActive(false);
        t.localScale = escala;
    }
}
