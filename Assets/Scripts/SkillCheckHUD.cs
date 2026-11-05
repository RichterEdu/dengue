using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/* ============================================================
 *  SkillCheckHUD.cs
 *  ---------------------------------------------------------------------
 *  Desenha o skill check do Interactor (SessaoDeSkillCheck): um disco
 *  escuro no centro da tela, com o trilho, a zona verde, o ponteiro e
 *  um pontinho por check no meio.
 *
 *  Monta o próprio Canvas em código; o Interactor adiciona este
 *  componente no Start, sem nada na cena. Todas as formas são sprites
 *  gerados uma vez, com bordas suaves. A zona é uma Image Filled
 *  (radial), então mudar de check não redesenha textura nenhuma.
 *
 *  Animações (sempre em tempo sem escala): entra com um pop, cada acerto
 *  dá um pulso e um clarão verde e, depois da sessão, o anel ainda fica
 *  um instante: cresce e some ao concluir, treme em vermelho ao errar.
 * ============================================================ */

public class SkillCheckHUD : MonoBehaviour
{
    private const float TamanhoNaTela = 120f;   // em 1920x1080
    private const int TexturaDoAnel = 256;
    private const int LarguraDoPonteiro = 32;
    private const int AlturaDoPonteiro = 128;

    private const float DuracaoEntrada = 0.14f;
    private const float DuracaoPulso = 0.22f;
    private const float DuracaoSaida = 0.34f;
    private const float EscalaDoPulso = 0.10f;
    private const float TremorDoErro = 7f;

    private static readonly Color CorDoDisco = new Color(0.04f, 0.07f, 0.10f, 0.72f);
    private static readonly Color CorDoTrilho = new Color(1f, 1f, 1f, 0.22f);
    private static readonly Color CorDaZona = new Color(0.25f, 1f, 0.8f, 1f);
    private static readonly Color CorDoBrilho = new Color(0.25f, 1f, 0.8f, 0.38f);
    private static readonly Color CorDoPonteiro = new Color(1f, 0.97f, 0.9f, 1f);
    private static readonly Color CorDoPontoApagado = new Color(1f, 1f, 1f, 0.25f);
    private static readonly Color CorDoErro = new Color(1f, 0.3f, 0.27f, 1f);

    private enum Fase { Oculto, Ativo, Saindo }

    private Interactor interactor;
    private GameObject raiz;
    private RectTransform conteudo;
    private CanvasGroup grupo;
    private Image zona;
    private Image brilho;
    private Image clarao;
    private Image ponteiro;
    private RectTransform pivo;
    private RectTransform pontos;
    private Sprite spriteDoPonto;
    private readonly List<Image> marcadores = new List<Image>();
    private readonly List<Object> gerados = new List<Object>();

    private Fase fase = Fase.Oculto;
    private SessaoDeSkillCheck sessaoVista;
    private int acertosVistos;
    private bool errou;
    private float desdeEntrada;
    private float desdePulso = DuracaoPulso;
    private float desdeSaida;

    void Start()
    {
        interactor = GetComponent<Interactor>();
        if (interactor != null)
            interactor.OnErrouSkillCheck += AoErrar;
        Montar();
        raiz.SetActive(false);
    }

    void OnDestroy()
    {
        if (interactor != null)
            interactor.OnErrouSkillCheck -= AoErrar;
        if (raiz != null)
            Destroy(raiz);
        foreach (Object gerado in gerados)
            Destroy(gerado);
    }

    private void AoErrar(Interactable foco)
    {
        errou = true;
    }

    void LateUpdate()
    {
        SessaoDeSkillCheck sessao = interactor != null ? interactor.SkillCheckAtivo : null;

        if (sessao != null)
        {
            if (!ReferenceEquals(sessao, sessaoVista))
                Entrar(sessao);
            Desenhar(sessao);
        }
        else if (fase == Fase.Ativo)
        {
            fase = Fase.Saindo;
            desdeSaida = 0f;
            sessaoVista = null;
            if (errou)
                Pintar(CorDoErro);
        }

        if (fase == Fase.Saindo)
            AnimarSaida();
    }

    private void Entrar(SessaoDeSkillCheck sessao)
    {
        sessaoVista = sessao;
        fase = Fase.Ativo;
        errou = false;
        acertosVistos = 0;
        desdeEntrada = 0f;
        desdePulso = DuracaoPulso;
        conteudo.anchoredPosition = Vector2.zero;
        Pintar(CorDaZona);
        MontarMarcadores(sessao.Total);
        raiz.SetActive(true);
    }

    private void Desenhar(SessaoDeSkillCheck sessao)
    {
        SkillCheck check = sessao.Atual;
        float dt = Time.unscaledDeltaTime;

        if (sessao.Acertos != acertosVistos)
        {
            acertosVistos = sessao.Acertos;
            desdePulso = 0f;
        }

        zona.fillAmount = check.TamanhoDaZona;
        brilho.fillAmount = check.TamanhoDaZona;
        Quaternion rotacaoDaZona = Quaternion.Euler(0f, 0f, -360f * check.InicioDaZona);
        zona.rectTransform.localRotation = rotacaoDaZona;
        brilho.rectTransform.localRotation = rotacaoDaZona;

        pivo.localEulerAngles = new Vector3(0f, 0f, -360f * check.Ponteiro);

        bool naZona = check.Ponteiro >= check.InicioDaZona
                   && check.Ponteiro <= check.InicioDaZona + check.TamanhoDaZona;
        ponteiro.color = naZona ? CorDaZona : CorDoPonteiro;

        for (int i = 0; i < marcadores.Count; i++)
            marcadores[i].color = i < sessao.Acertos ? CorDaZona : CorDoPontoApagado;

        // Entrada: cresce passando um pouco do tamanho e volta (ease-out back).
        desdeEntrada += dt;
        float e = Mathf.Clamp01(desdeEntrada / DuracaoEntrada) - 1f;
        float entrada = 1f + 2.7f * e * e * e + 1.7f * e * e;
        grupo.alpha = Mathf.Clamp01(desdeEntrada / DuracaoEntrada);

        // Pulso do acerto: rápido na ida, suave na volta.
        desdePulso += dt;
        float p = Mathf.Clamp01(desdePulso / DuracaoPulso);
        float pulso = (1f - p) * (1f - p);
        conteudo.localScale = Vector3.one * (Mathf.Lerp(0.75f, 1f, entrada) + EscalaDoPulso * pulso);
        DefinirAlpha(clarao, pulso);
    }

    /* Concluiu: cresce e some. Errou: treme em vermelho e some. */
    private void AnimarSaida()
    {
        desdeSaida += Time.unscaledDeltaTime;
        float k = Mathf.Clamp01(desdeSaida / DuracaoSaida);

        grupo.alpha = 1f - k * k;
        DefinirAlpha(clarao, 1f - k);

        if (errou)
        {
            float tremor = TremorDoErro * (1f - k) * Mathf.Sin(k * Mathf.PI * 7f);
            conteudo.anchoredPosition = new Vector2(tremor, 0f);
            conteudo.localScale = Vector3.one;
        }
        else
        {
            conteudo.localScale = Vector3.one * (1f + 0.22f * (1f - (1f - k) * (1f - k)));
        }

        if (k >= 1f)
        {
            fase = Fase.Oculto;
            raiz.SetActive(false);
        }
    }

    /* Cor do momento: o verde normal ou o vermelho do erro. */
    private void Pintar(Color cor)
    {
        clarao.color = new Color(cor.r, cor.g, cor.b, clarao.color.a);
        zona.color = cor;
        ponteiro.color = cor == CorDoErro ? cor : CorDoPonteiro;
        brilho.color = new Color(cor.r, cor.g, cor.b, CorDoBrilho.a);
        if (cor == CorDoErro)
            DefinirAlpha(clarao, 1f);
    }

    private static void DefinirAlpha(Image imagem, float alpha)
    {
        Color cor = imagem.color;
        cor.a = alpha;
        imagem.color = cor;
    }

    private void MontarMarcadores(int total)
    {
        const float tamanho = 8f;
        const float espaco = 14f;

        while (marcadores.Count < total)
        {
            Image ponto = Camada("Ponto", spriteDoPonto, CorDoPontoApagado, tamanho, pontos);
            marcadores.Add(ponto);
        }

        float inicio = -espaco * (total - 1) * 0.5f;
        for (int i = 0; i < marcadores.Count; i++)
        {
            marcadores[i].gameObject.SetActive(i < total);
            marcadores[i].rectTransform.anchoredPosition = new Vector2(inicio + espaco * i, 0f);
        }
    }

    /* ---------------- Montagem ---------------- */

    private void Montar()
    {
        raiz = new GameObject("SkillCheckHUD");
        Canvas canvas = raiz.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        CanvasScaler escala = raiz.AddComponent<CanvasScaler>();
        escala.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        escala.referenceResolution = new Vector2(1920f, 1080f);

        conteudo = new GameObject("Conteudo").AddComponent<RectTransform>();
        conteudo.SetParent(raiz.transform, false);
        conteudo.sizeDelta = Vector2.one * TamanhoNaTela;
        grupo = conteudo.gameObject.AddComponent<CanvasGroup>();
        grupo.blocksRaycasts = false;
        grupo.interactable = false;

        Sprite disco = GerarAnel(-1f, 0.97f, 0.03f);
        Sprite trilho = GerarAnel(0.745f, 0.775f, 0.008f);
        Sprite faixa = GerarAnel(0.70f, 0.82f, 0.012f);
        Sprite halo = GerarAnel(0.64f, 0.88f, 0.09f);
        spriteDoPonto = GerarAnel(-1f, 0.82f, 0.12f);

        Camada("Disco", disco, CorDoDisco, TamanhoNaTela, conteudo);
        Camada("Trilho", trilho, CorDoTrilho, TamanhoNaTela, conteudo);

        brilho = Camada("Brilho", halo, CorDoBrilho, TamanhoNaTela, conteudo);
        Preencher(brilho);

        zona = Camada("Zona", faixa, CorDaZona, TamanhoNaTela, conteudo);
        Preencher(zona);

        clarao = Camada("Clarao", halo, new Color(CorDaZona.r, CorDaZona.g, CorDaZona.b, 0f), TamanhoNaTela, conteudo);

        pontos = new GameObject("Pontos").AddComponent<RectTransform>();
        pontos.SetParent(conteudo, false);

        pivo = new GameObject("Pivo").AddComponent<RectTransform>();
        pivo.SetParent(conteudo, false);

        ponteiro = Camada("Ponteiro", GerarPonteiro(), CorDoPonteiro, 1f, pivo);
        ponteiro.rectTransform.sizeDelta = new Vector2(7f, TamanhoNaTela * 0.27f);
        ponteiro.rectTransform.anchoredPosition = new Vector2(0f, TamanhoNaTela * 0.38f);
    }

    private static Image Camada(string nome, Sprite sprite, Color cor, float tamanho, Transform pai)
    {
        Image imagem = new GameObject(nome).AddComponent<Image>();
        imagem.transform.SetParent(pai, false);
        imagem.sprite = sprite;
        imagem.color = cor;
        imagem.raycastTarget = false;
        imagem.rectTransform.sizeDelta = Vector2.one * tamanho;
        return imagem;
    }

    /* Radial horário a partir do topo: a mesma medida do ponteiro e da zona. */
    private static void Preencher(Image imagem)
    {
        imagem.type = Image.Type.Filled;
        imagem.fillMethod = Image.FillMethod.Radial360;
        imagem.fillOrigin = (int)Image.Origin360.Top;
        imagem.fillClockwise = true;
        imagem.fillAmount = 0f;
    }

    /* Anel branco com bordas suaves (raios em frações da metade da textura).
     * Raio interno negativo = disco cheio. A cor vem da Image. */
    private Sprite GerarAnel(float raioInterno, float raioExterno, float suavidade)
    {
        float centro = (TexturaDoAnel - 1) * 0.5f;
        Color32[] pixels = new Color32[TexturaDoAnel * TexturaDoAnel];

        for (int y = 0; y < TexturaDoAnel; y++)
        {
            for (int x = 0; x < TexturaDoAnel; x++)
            {
                float raio = Mathf.Sqrt((x - centro) * (x - centro) + (y - centro) * (y - centro)) / centro;
                float entrada = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(raioInterno - suavidade, raioInterno + suavidade, raio));
                float saida = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(raioExterno + suavidade, raioExterno - suavidade, raio));
                byte alpha = (byte)Mathf.RoundToInt(255f * entrada * saida);
                pixels[y * TexturaDoAnel + x] = new Color32(255, 255, 255, alpha);
            }
        }

        return CriarSprite(pixels, TexturaDoAnel, TexturaDoAnel);
    }

    /* Ponteiro afilado para o centro, com a ponta de fora mais larga. */
    private Sprite GerarPonteiro()
    {
        Color32[] pixels = new Color32[LarguraDoPonteiro * AlturaDoPonteiro];
        float centro = (LarguraDoPonteiro - 1) * 0.5f;

        for (int y = 0; y < AlturaDoPonteiro; y++)
        {
            float v = y / (AlturaDoPonteiro - 1f);
            float meiaLargura = Mathf.Lerp(2f, 13f, v);
            float pontas = Mathf.Clamp01(Mathf.Min(v, 1f - v) * 24f);
            for (int x = 0; x < LarguraDoPonteiro; x++)
            {
                float lado = Mathf.Clamp01(meiaLargura - Mathf.Abs(x - centro) + 0.5f);
                pixels[y * LarguraDoPonteiro + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(255f * lado * pontas));
            }
        }

        return CriarSprite(pixels, LarguraDoPonteiro, AlturaDoPonteiro);
    }

    private Sprite CriarSprite(Color32[] pixels, int largura, int altura)
    {
        Texture2D textura = new Texture2D(largura, altura, TextureFormat.RGBA32, true)
        {
            filterMode = FilterMode.Trilinear,
            wrapMode = TextureWrapMode.Clamp
        };
        textura.SetPixels32(pixels);
        textura.Apply(true, true);

        Sprite sprite = Sprite.Create(textura, new Rect(0f, 0f, largura, altura), new Vector2(0.5f, 0.5f),
                                      100f, 0, SpriteMeshType.FullRect);
        gerados.Add(textura);
        gerados.Add(sprite);
        return sprite;
    }
}
