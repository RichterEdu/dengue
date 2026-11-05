using UnityEngine;

/* ============================================================
 *  EnxameDeMosquitos.cs
 *  ---------------------------------------------------------------------
 *  O sinal do estágio Mosquito no mundo (Parte B): um enxame de
 *  pontinhos escuros voando em volta do foco, visível de longe (15–20 m).
 *
 *    - escuta o OnEstagioMudou do GerenciadorDeFocos: no Mosquito, o foco
 *      ganha um filho "Enxame" com um ParticleSystem em loop, criado em
 *      tempo de execução (sem prefab); em outro estágio (o esvaziar volta
 *      a ovo, a chuva reenche como larva), o filho sai;
 *    - também tira o enxame ao esvaziar (OnFocoEsvaziado) e ao eliminar
 *      (OnFocoEliminado);
 *    - o enxame fica 'altura' acima do pivô do foco, numa esfera de
 *      'raio', em espaço local (acompanha o foco) e com ruído forte (voo
 *      errático). A escala do foco não conta (ScalingMode Local): o
 *      tamanho é o de mundo, em qualquer foco;
 *    - todos os enxames dividem um material só (Sprites/Default, com
 *      transparência e sempre incluído na build), criado no primeiro uso;
 *      a cor vem da cor de início das partículas. O brilho da mira e da
 *      proximidade pula o ParticleSystemRenderer
 *      (EmissionHighlightUtil.Destacavel): o material segue compartilhado.
 *
 *  O gerenciador do campo ou, vazio, GerenciadorDeFocos.Obter() é ligado
 *  no Start (Ligar), que também põe o enxame nos focos que já estiverem
 *  no Mosquito naquela hora. No EditMode o Start não roda: os testes chamam o
 *  Ligar ou o Atualizar(foco, estagio) direto, e fora do Play o filho
 *  sai na hora (DestroyImmediate).
 * ============================================================ */

public class EnxameDeMosquitos : MonoBehaviour
{
    private const string NomeDoFilho = "Enxame";

    // Voo dos mosquitos: quanto vive cada pontinho, a velocidade de saída
    // e o ruído (forte: o enxame se mexe o tempo todo, sem rumo).
    private const float VidaDaParticula = 3f;
    private const float VelocidadeMinima = 0.05f;
    private const float VelocidadeMaxima = 0.25f;
    private const float ForcaDoRuido = 1.5f;
    private const float FrequenciaDoRuido = 1.2f;

    // Sem tamanho mínimo na tela: cada mosquito encolhe com a distância
    // até sumir (com um mínimo, viravam bolas vistas do outro lado do mapa).
    private const float TamanhoMinimoNaTela = 0f;

    // Lado (px) da textura da bolinha de cada mosquito.
    private const int LadoDaBolinha = 16;

    [Tooltip("Focos da partida (estágio, esvaziado, eliminado). Vazio = GerenciadorDeFocos.Obter().")]
    [SerializeField] private GerenciadorDeFocos gerenciador;

    [Tooltip("Quantos mosquitos (partículas) no máximo em cada enxame.")]
    [SerializeField] private int particulas = 40;

    [Tooltip("Raio (em metros) da esfera onde os mosquitos voam.")]
    [SerializeField] private float raio = 0.9f;

    [Tooltip("Altura (em metros) do centro do enxame acima do pivô do foco.")]
    [SerializeField] private float altura = 1.0f;

    [Tooltip("Cor dos mosquitos.")]
    [SerializeField] private Color cor = new Color(0.08f, 0.08f, 0.08f);

    // Um material para todos os enxames (criado no primeiro uso).
    private static Material materialCompartilhado;

    // Gerenciador cujos eventos estão assinados (para desassinar no OnDestroy).
    private GerenciadorDeFocos assinado;

    void Start()
    {
        Ligar();
    }

    void OnDestroy()
    {
        // Só pelo cache: procurar ou criar um gerenciador aqui (fim do Play,
        // troca de cena) deixaria objeto novo para trás.
        if (assinado == null)
            return;

        assinado.OnEstagioMudou -= Atualizar;
        assinado.OnFocoEsvaziado -= Remover;
        assinado.OnFocoEliminado -= Remover;
        assinado = null;
    }

    /* Assina os eventos do gerenciador (o do campo ou, vazio, o Obter()),
     * uma vez só, e já põe o enxame nos focos que estão no Mosquito (os
     * eventos só contam as mudanças daqui para a frente). */
    internal void Ligar()
    {
        if (assinado != null)
            return;

        if (gerenciador == null)
            gerenciador = GerenciadorDeFocos.Obter();
        if (gerenciador == null)
            return;

        assinado = gerenciador;
        assinado.OnEstagioMudou += Atualizar;
        assinado.OnFocoEsvaziado += Remover;
        assinado.OnFocoEliminado += Remover;

        Sincronizar();
    }

    /* O estado de agora: enxame em cada foco vivo (não eliminado nem
     * esvaziado) que já está no Mosquito. */
    private void Sincronizar()
    {
        foreach (Interactable foco in assinado.Focos)
        {
            if (foco == null || foco.IsComplete || foco.Esvaziado)
                continue;
            if (assinado.EstagioDe(foco) == EstagioDoFoco.Mosquito)
                Atualizar(foco, EstagioDoFoco.Mosquito);
        }
    }

    /* No Mosquito, cria o enxame no foco (se ainda não tem); em outro
     * estágio, tira. */
    internal void Atualizar(Interactable foco, EstagioDoFoco estagio)
    {
        if (foco == null)
            return;

        if (estagio == EstagioDoFoco.Mosquito)
        {
            if (foco.transform.Find(NomeDoFilho) == null)
                Criar(foco);
        }
        else
        {
            Remover(foco);
        }
    }

    /* Tira o enxame do foco, se houver. */
    private void Remover(Interactable foco)
    {
        if (foco == null)
            return;

        Transform filho = foco.transform.Find(NomeDoFilho);
        if (filho == null)
            return;

        if (Application.isPlaying)
            Destroy(filho.gameObject);
        else
            DestroyImmediate(filho.gameObject);
    }

    /* O filho "Enxame": ParticleSystem em loop, espaço local, esfera de
     * 'raio', ruído forte, partículas de 0,03–0,05 m e no máximo
     * 'particulas'. */
    private void Criar(Interactable foco)
    {
        GameObject go = new GameObject(NomeDoFilho);
        go.transform.SetParent(foco.transform, false);
        // Altura em metros de mundo (a escala do foco não entra).
        go.transform.position = foco.transform.position + Vector3.up * altura;

        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        // Para antes de configurar: com o sistema tocando, algumas
        // propriedades não podem mudar.
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        ParticleSystem.MainModule main = ps.main;
        main.loop = true;
        main.prewarm = true;   // já nasce cheio de mosquitos
        main.startLifetime = VidaDaParticula;
        main.startSpeed = new ParticleSystem.MinMaxCurve(VelocidadeMinima, VelocidadeMaxima);
        main.startSize = new ParticleSystem.MinMaxCurve(0.03f, 0.05f);
        main.startColor = cor;
        main.maxParticles = Mathf.Max(1, particulas);
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.scalingMode = ParticleSystemScalingMode.Local;
        main.gravityModifier = 0f;

        // Repõe os que morrem: a conta fica perto de 'particulas'.
        ParticleSystem.EmissionModule emission = ps.emission;
        emission.rateOverTime = Mathf.Max(1, particulas) / VidaDaParticula;

        ParticleSystem.ShapeModule shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = raio;

        ParticleSystem.NoiseModule noise = ps.noise;
        noise.enabled = true;
        noise.strength = ForcaDoRuido;
        noise.frequency = FrequenciaDoRuido;
        noise.scrollSpeed = 0.5f;
        noise.damping = true;

        ParticleSystemRenderer psr = go.GetComponent<ParticleSystemRenderer>();
        psr.renderMode = ParticleSystemRenderMode.Billboard;
        psr.minParticleSize = TamanhoMinimoNaTela;
        psr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        psr.receiveShadows = false;
        psr.sharedMaterial = MaterialDoEnxame();

        if (Application.isPlaying)
            ps.Play();
    }

    /* O material de todos os enxames: Sprites/Default (transparente e
     * sempre incluído na build), com uma bolinha gerada aqui (a textura
     * embutida "Default-Particle.psd" não é achada nesta versão do Unity). */
    private static Material MaterialDoEnxame()
    {
        if (materialCompartilhado != null)
            return materialCompartilhado;

        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null)
            shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");

        materialCompartilhado = new Material(shader)
        {
            name = "Enxame (runtime)",
            hideFlags = HideFlags.DontSave,
            mainTexture = Bolinha()
        };
        return materialCompartilhado;
    }

    /* Bolinha branca de LadoDaBolinha px, opaca no meio e sumindo na
     * borda (a cor vem das partículas). Também é a textura dos estouros
     * (Interactable.MaterialDoEstouro). */
    internal static Texture2D Bolinha()
    {
        Texture2D tex = new Texture2D(LadoDaBolinha, LadoDaBolinha, TextureFormat.RGBA32, false)
        {
            name = "Bolinha do enxame (runtime)",
            hideFlags = HideFlags.DontSave,
            wrapMode = TextureWrapMode.Clamp
        };

        float centro = (LadoDaBolinha - 1) * 0.5f;
        Color[] pixels = new Color[LadoDaBolinha * LadoDaBolinha];
        for (int y = 0; y < LadoDaBolinha; y++)
        {
            for (int x = 0; x < LadoDaBolinha; x++)
            {
                float d = new Vector2(x - centro, y - centro).magnitude / (LadoDaBolinha * 0.5f);
                pixels[y * LadoDaBolinha + x] = new Color(1f, 1f, 1f, Mathf.Clamp01((1f - d) * 2f));
            }
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }
}
