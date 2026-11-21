using UnityEngine;

/* ============================================================
 *  BorrifadorDePerto.cs
 *  ---------------------------------------------------------------------
 *  O borrifador na tela durante a vista de perto da bromélia
 *  (PontosDeBorrifar): uma cópia do borrifador da mão (ItensNaMao
 *  .BorrifadorNaMao) presa à câmera, no canto de baixo, à direita, de
 *  perfil, apontando o bico para a mira. A mão do agente fica quieta
 *  nessa hora (RegraDoItemNaMao.Gesticula com dePerto).
 *
 *    - Montar cria a cópia uma vez, filha da câmera, inativa, na
 *      PosicaoNaCamera e com escala 1, sem projetar sombra. A névoa
 *      herdada (filho "Nevoa") vira a névoa de perto: emissão desligada,
 *      gotas menores e mais lentas. Nenhum outro sistema de partículas é
 *      criado, e o original não muda.
 *    - Com sessão (Interactor.PontosAtivos): ativa a cópia (Ativar), já
 *      apontada para a mira, que entra deslizando de baixo e da direita
 *      em DuracaoDaEntrada.
 *    - A cada quadro, o giro vai para GiroParaAMira (a mira como ela
 *      aparece na tela, com MergulhoGraus para dentro da cena), suavizado.
 *    - A cada borrifada nova da sessão, Emit(GotasPorBorrifada). Sessão
 *      nova recomeça a contagem vista.
 *    - Sem sessão: limpa a névoa e desativa a cópia.
 *    - Sem câmera, sem ItensNaMao ou sem borrifador na mão: nada.
 *
 *  Fica no Player; o Interactor adiciona este componente no Start, sem
 *  nada na cena. Roda depois da câmera (ordem 110), em tempo sem escala.
 * ============================================================ */

[DefaultExecutionOrder(110)]
public class BorrifadorDePerto : MonoBehaviour
{
    /* Onde a cópia fica no espaço da câmera (canto de baixo, à direita). */
    internal static readonly Vector3 PosicaoNaCamera = new Vector3(0.172f, -0.078f, 0.36f);

    /* O bico, no espaço da cópia. */
    internal static readonly Vector3 BicoLocal = new Vector3(0f, 0.034f, 0.088f);

    /* Graus que o bico aponta para dentro da cena (o resto é de lado, na tela). */
    internal const float MergulhoGraus = 30f;

    internal const int GotasPorBorrifada = 24;

    private const string NomeDaCopia = "BorrifadorDePerto";
    private const string NomeDaNevoa = "Nevoa";
    private const float TamanhoDaGota = 0.02f;
    private const float VelocidadeDaGota = 0.8f;
    private const float DuracaoDaEntrada = 0.2f;
    private const float RapidezDoGiro = 18f;
    private static readonly Vector3 DesvioDaEntrada = new Vector3(0.05f, -0.12f, 0f);

    private Interactor interactor;
    private ItensNaMao itens;

    private GameObject copia;
    private ParticleSystem nevoaDePerto;

    private PontosDeBorrifar sessaoVista;
    private int borrifadasVistas;
    private float tempoDeEntrada;

    /* Giro, no espaço da câmera, que aponta o bico para a mira como ela aparece na tela,
     * com 'mergulhoGraus' para dentro da cena. */
    internal static Quaternion GiroParaAMira(Vector2 mira, float fov, Vector3 bicoNaCamera, float mergulhoGraus)
    {
        float t = Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad);
        Vector2 naTela = new Vector2(mira.x * t * bicoNaCamera.z - bicoNaCamera.x,
                                     mira.y * t * bicoNaCamera.z - bicoNaCamera.y);
        Vector2 lado = naTela.sqrMagnitude > 1e-8f ? naTela.normalized : Vector2.up;
        float m = mergulhoGraus * Mathf.Deg2Rad;
        Vector3 frente = new Vector3(lado.x * Mathf.Cos(m), lado.y * Mathf.Cos(m), Mathf.Sin(m));
        return Quaternion.LookRotation(frente, Vector3.up);
    }

    /* Cria (uma vez) a cópia do borrifador presa à câmera, inativa. Sem original: null. */
    internal GameObject Montar(Transform camera, Transform original)
    {
        if (copia != null)
            return copia;
        if (camera == null || original == null)
            return null;

        copia = Instantiate(original.gameObject, camera, false);
        copia.SetActive(false);
        copia.name = NomeDaCopia;
        copia.transform.localPosition = PosicaoNaCamera;
        copia.transform.localScale = Vector3.one;

        foreach (Renderer r in copia.GetComponentsInChildren<Renderer>(true))
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        nevoaDePerto = null;
        foreach (ParticleSystem ps in copia.GetComponentsInChildren<ParticleSystem>(true))
        {
            if (ps.name == NomeDaNevoa)
            {
                nevoaDePerto = ps;
                break;
            }
        }

        if (nevoaDePerto != null)
        {
            ParticleSystem.EmissionModule emissao = nevoaDePerto.emission;
            emissao.enabled = false;
            ParticleSystem.MainModule principal = nevoaDePerto.main;
            principal.startSize = TamanhoDaGota;
            principal.startSpeed = VelocidadeDaGota;
        }

        return copia;
    }

    void OnDisable()
    {
        Esconder();
    }

    void LateUpdate()
    {
        if (interactor == null) interactor = GetComponent<Interactor>();
        if (itens == null) itens = GetComponent<ItensNaMao>();

        Camera camera = Camera.main;
        if (camera == null || itens == null || itens.BorrifadorNaMao == null)
            return;

        PontosDeBorrifar sessao = interactor != null ? interactor.PontosAtivos : null;
        if (sessao == null)
        {
            Esconder();
            return;
        }

        if (Montar(camera.transform, itens.BorrifadorNaMao) == null)
            return;

        float dt = Time.unscaledDeltaTime;

        if (!ReferenceEquals(sessao, sessaoVista))
        {
            // Sessão nova: a contagem vista recomeça e a cópia entra de novo.
            sessaoVista = sessao;
            borrifadasVistas = 0;
            tempoDeEntrada = 0f;
        }

        if (!copia.activeSelf)
            Ativar(sessao.Mira, camera.fieldOfView);

        // Entrada: desliza de baixo e da direita até a posição na câmera.
        tempoDeEntrada += dt;
        float entrada = Mathf.Clamp01(tempoDeEntrada / DuracaoDaEntrada);
        copia.transform.localPosition = Vector3.Lerp(PosicaoNaCamera + DesvioDaEntrada, PosicaoNaCamera, entrada);

        // Giro: o bico segue a mira.
        Quaternion giroAtual = copia.transform.localRotation;
        Quaternion alvo = GiroParaAMira(sessao.Mira, camera.fieldOfView,
                                        PosicaoNaCamera + giroAtual * BicoLocal, MergulhoGraus);
        copia.transform.localRotation = Quaternion.Slerp(giroAtual, alvo, 1f - Mathf.Exp(-RapidezDoGiro * dt));

        // Borrifadas novas viram gotas.
        if (sessao.Borrifadas > borrifadasVistas)
        {
            if (nevoaDePerto != null)
                nevoaDePerto.Emit(GotasPorBorrifada * (sessao.Borrifadas - borrifadasVistas));
            borrifadasVistas = sessao.Borrifadas;
        }
    }

    /* Ativa a cópia, que entra deslizando de novo, já com o bico apontado
     * para a mira: sem a virada do giro herdado da mão (ou da sessão
     * anterior). Sem cópia: nada. */
    internal void Ativar(Vector2 mira, float fov)
    {
        if (copia == null)
            return;

        copia.SetActive(true);
        tempoDeEntrada = 0f;
        // O bico depende do giro: aplicar o alvo inteiro oscila; meio passo
        // por volta converge em menos de 12 (o mesmo ponto a que o Slerp do
        // LateUpdate chegaria).
        for (int i = 0; i < 12; i++)
        {
            Quaternion giro = copia.transform.localRotation;
            Quaternion alvo = GiroParaAMira(mira, fov, PosicaoNaCamera + giro * BicoLocal, MergulhoGraus);
            copia.transform.localRotation = Quaternion.Slerp(giro, alvo, 0.5f);
        }
        if (nevoaDePerto != null && !nevoaDePerto.isPlaying)
            nevoaDePerto.Play();
    }

    /* Sem sessão: limpa a névoa e desativa a cópia. */
    private void Esconder()
    {
        sessaoVista = null;
        borrifadasVistas = 0;
        if (copia == null || !copia.activeSelf)
            return;

        if (nevoaDePerto != null)
            nevoaDePerto.Clear();
        copia.SetActive(false);
    }
}
