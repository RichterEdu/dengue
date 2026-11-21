using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/* ============================================================
 *  PontosDeBorrifarHUD.cs
 *  ---------------------------------------------------------------------
 *  Desenha o minigame da bromélia (PontosDeBorrifar) na vista de perto:
 *  as bolinhas a borrifar, a mira, a contagem e a dica.
 *
 *  Camadas, de trás para a frente:
 *  1. véu preto leve na tela toda;
 *  2. dica: faixa escura embaixo, acima da barra do kit, com a
 *     DicaDoMinigame do foco mirado;
 *  3. contagem: um ponto por bolinha, verde para as já acertadas;
 *  4. a próxima bolinha, meio transparente e sem anel;
 *  5. a bolinha da vez: halo verde, corpo, borda, número e o anel fino
 *     que fecha (EscalaDoAnel);
 *  6. estouro a cada acerto: anel verde que cresce e some, com gotinhas;
 *  7. a mira, desde o começo da sessão.
 *
 *  Monta o próprio Canvas em código (sortingOrder 45: acima da barra do
 *  kit, abaixo do skill check); o Interactor adiciona este componente no
 *  Start, sem nada na cena. As formas são sprites gerados uma vez pelo
 *  SkillCheckHUD.GerarAnel. Com sessão, aparece com fade; sem sessão,
 *  some com fade mostrando o último estado desenhado, sem ler o Interactor
 *  (o alvo já é null nessa hora). Tudo em tempo sem escala.
 *
 *  O campo da sessão (y de −1 na base a +1 no topo) vira posição no
 *  Canvas por NaTela. Clique fora não desenha nada aqui: a borrifada é do
 *  BorrifadorDePerto.
 * ============================================================ */

public class PontosDeBorrifarHUD : MonoBehaviour
{
    // Alturas a partir da base da tela, em 1920x1080 (acima da barra do kit).
    internal const float AlturaDaDica = 172f;       // centro da faixa
    internal const float AlturaDaFaixa = 44f;
    internal const float AlturaDaContagem = 212f;   // centro dos pontos
    internal const float TamanhoDoPonto = 16f;

    private const float LarguraDaFaixa = 880f;
    private const float TamanhoDoTexto = 24f;
    private const float EspacoEntrePontos = 34f;
    private const float DuracaoFade = 0.15f;
    private const float DuracaoEstouro = 0.3f;
    private const float EscalaFinalDoEstouro = 1.6f;
    private const int Gotas = 10;
    private const float AlturaPadrao = 1080f;

    private const float TamanhoDaSombra = 50f;
    private const float TamanhoDoAnelDaMira = 46f;
    private const float TamanhoDoMiolo = 18f;

    private static readonly Color CorVerde = new Color(0.25f, 1f, 0.8f, 1f);
    private static readonly Color CorEscura = new Color(0.04f, 0.07f, 0.10f, 0.72f);
    private static readonly Color CorDoCorpo = new Color(0.05f, 0.40f, 0.38f, 0.93f);
    private static readonly Color CorDoMiolo = new Color(1f, 0.93f, 0.35f, 1f);
    private static readonly Color CorDoPontoApagado = new Color(1f, 1f, 1f, 0.30f);
    private static readonly Color CorDasGotas = new Color(0.75f, 1f, 0.95f, 0.85f);
    private static readonly Color CorDoHalo = new Color(0.25f, 1f, 0.8f, 0.30f);
    private static readonly Color CorDoVeu = new Color(0f, 0f, 0f, 0.12f);
    private static readonly Color CorDaSombra = new Color(0f, 0f, 0f, 0.35f);

    /* Do campo para o Canvas: y = ±1 é a borda de cima e a de baixo. */
    internal static Vector2 NaTela(Vector2 campo, float alturaDoCanvas) => campo * (alturaDoCanvas * 0.5f);

    /* Tamanho do anel em relação à bolinha: 2,2 com o anel cheio, 1 fechado. */
    internal static float EscalaDoAnel(float anel) => Mathf.Lerp(1f, 2.2f, Mathf.Clamp01(anel));

    /* Peças de uma bolinha (a da vez ou a próxima). */
    private class Bolinha
    {
        public RectTransform raiz;
        public Image halo;
        public Image corpo;
        public Image borda;
        public TextMeshProUGUI numero;
        public Image anel;   // só a da vez
        public int numeroMostrado = -1;
    }

    private Interactor interactor;
    private GameObject raiz;
    private RectTransform rectDaRaiz;
    private CanvasGroup grupo;
    private TextMeshProUGUI textoDaDica;
    private RectTransform contagem;
    private Bolinha proxima;
    private Bolinha daVez;
    private RectTransform estouro;
    private Image anelDoEstouro;
    private readonly Image[] gotas = new Image[Gotas];
    private RectTransform mira;
    private Sprite spriteDoDisco;
    private readonly List<Image> pontos = new List<Image>();
    private readonly List<Object> gerados = new List<Object>();

    private PontosDeBorrifar sessaoVista;
    private int acertosVistos;
    private string dicaMostrada;
    private float desdeEstouro = DuracaoEstouro;
    private float diametroDoEstouro;

    void Start()
    {
        interactor = GetComponent<Interactor>();
        Montar();
        grupo.alpha = 0f;
        raiz.SetActive(false);
    }

    void OnDestroy()
    {
        if (raiz != null)
            Destroy(raiz);
        foreach (Object gerado in gerados)
            Destroy(gerado);
    }

    void LateUpdate()
    {
        if (raiz == null)
            return;

        PontosDeBorrifar sessao = interactor != null ? interactor.PontosAtivos : null;

        // Sessão nova (ou nenhuma): os acertos recomeçam do 0.
        if (!ReferenceEquals(sessao, sessaoVista))
        {
            sessaoVista = sessao;
            acertosVistos = 0;
            if (sessao != null)
                MontarPontos(sessao.Total);
        }

        float dt = Time.unscaledDeltaTime;
        if (sessao != null)
        {
            if (!raiz.activeSelf)
                raiz.SetActive(true);
            Desenhar(sessao);
        }

        AnimarEstouro(dt);

        grupo.alpha = Mathf.MoveTowards(grupo.alpha, sessao != null ? 1f : 0f, dt / DuracaoFade);
        if (sessao == null && grupo.alpha <= 0f && raiz.activeSelf)
            raiz.SetActive(false);
    }

    private float AlturaDoCanvas()
    {
        float altura = rectDaRaiz.rect.height;
        return altura > 0f ? altura : AlturaPadrao;
    }

    private void Desenhar(PontosDeBorrifar sessao)
    {
        float altura = AlturaDoCanvas();
        float diametro = 2f * sessao.Raio * altura * 0.5f;

        // Dica: guardada enquanto há sessão (sem sessão, o alvo já é null).
        Interactable alvo = interactor.CurrentTarget;
        if (alvo != null)
        {
            string dica = alvo.DicaDoMinigame ?? "";
            if (dica != dicaMostrada)
            {
                dicaMostrada = dica;
                textoDaDica.text = dica;
            }
        }

        for (int k = 0; k < pontos.Count; k++)
            pontos[k].color = k < sessao.Acertos ? CorVerde : CorDoPontoApagado;

        // Acerto novo: estouro no ponto acertado.
        if (sessao.Acertos > acertosVistos)
        {
            int acertado = sessao.Acertos - 1;
            estouro.anchoredPosition = NaTela(sessao.Ponto(acertado), altura);
            diametroDoEstouro = diametro;
            desdeEstouro = 0f;
            estouro.gameObject.SetActive(true);
        }
        acertosVistos = sessao.Acertos;

        // A próxima aparece inclusive na pausa da bolinha da vez.
        bool temProxima = sessao.Proxima >= 0;
        Ativar(proxima.raiz.gameObject, temProxima);
        if (temProxima)
            Posicionar(proxima, sessao.Proxima, NaTela(sessao.Ponto(sessao.Proxima), altura), diametro);

        bool naTela = sessao.BolinhaNaTela;
        Ativar(daVez.raiz.gameObject, naTela);
        if (naTela)
        {
            Posicionar(daVez, sessao.DaVez, NaTela(sessao.Ponto(sessao.DaVez), altura), diametro);
            daVez.anel.rectTransform.sizeDelta = Vector2.one * (EscalaDoAnel(sessao.Anel) * diametro);
        }

        mira.anchoredPosition = NaTela(sessao.Mira, altura);
    }

    private static void Posicionar(Bolinha bolinha, int indice, Vector2 posicao, float diametro)
    {
        bolinha.raiz.anchoredPosition = posicao;
        bolinha.halo.rectTransform.sizeDelta = Vector2.one * (2f * diametro);
        bolinha.corpo.rectTransform.sizeDelta = Vector2.one * diametro;
        bolinha.borda.rectTransform.sizeDelta = Vector2.one * diametro;
        bolinha.numero.rectTransform.sizeDelta = Vector2.one * diametro;
        bolinha.numero.fontSize = 0.44f * diametro;
        if (bolinha.numeroMostrado != indice)
        {
            bolinha.numeroMostrado = indice;
            bolinha.numero.text = (indice + 1).ToString();
        }
    }

    /* Anel verde de 1× a 1,6× o diâmetro, sumindo; as gotinhas se afastam. */
    private void AnimarEstouro(float dt)
    {
        if (!estouro.gameObject.activeSelf)
            return;

        desdeEstouro += dt;
        float k = Mathf.Clamp01(desdeEstouro / DuracaoEstouro);
        float saida = 1f - (1f - k) * (1f - k);   // rápido no começo, suave no fim

        anelDoEstouro.rectTransform.sizeDelta = Vector2.one * (Mathf.Lerp(1f, EscalaFinalDoEstouro, saida) * diametroDoEstouro);
        DefinirAlpha(anelDoEstouro, 1f - k);

        float distancia = diametroDoEstouro * (0.5f + 0.55f * saida);
        for (int i = 0; i < gotas.Length; i++)
        {
            float angulo = (i * 36f + (i % 2) * 17f) * Mathf.Deg2Rad;
            gotas[i].rectTransform.anchoredPosition = new Vector2(Mathf.Cos(angulo), Mathf.Sin(angulo)) * distancia;
            DefinirAlpha(gotas[i], CorDasGotas.a * (1f - k));
        }

        if (k >= 1f)
            estouro.gameObject.SetActive(false);
    }

    private static void DefinirAlpha(Image imagem, float alpha)
    {
        Color cor = imagem.color;
        cor.a = alpha;
        imagem.color = cor;
    }

    private static void Ativar(GameObject objeto, bool ativo)
    {
        if (objeto.activeSelf != ativo)
            objeto.SetActive(ativo);
    }

    /* Um ponto por bolinha, centrados, a EspacoEntrePontos um do outro. */
    private void MontarPontos(int total)
    {
        while (pontos.Count < total)
            pontos.Add(Camada("Ponto", spriteDoDisco, CorDoPontoApagado, TamanhoDoPonto, contagem));

        float inicio = -EspacoEntrePontos * (total - 1) * 0.5f;
        for (int i = 0; i < pontos.Count; i++)
        {
            pontos[i].gameObject.SetActive(i < total);
            pontos[i].color = CorDoPontoApagado;
            pontos[i].rectTransform.anchoredPosition = new Vector2(inicio + EspacoEntrePontos * i, 0f);
        }
    }

    /* ---------------- Montagem ---------------- */

    private void Montar()
    {
        raiz = new GameObject("PontosDeBorrifarHUD");
        Canvas canvas = raiz.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 45;
        CanvasScaler escala = raiz.AddComponent<CanvasScaler>();
        escala.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        escala.referenceResolution = new Vector2(1920f, 1080f);
        escala.matchWidthOrHeight = 0.5f;
        grupo = raiz.AddComponent<CanvasGroup>();
        grupo.blocksRaycasts = false;
        grupo.interactable = false;
        rectDaRaiz = (RectTransform)raiz.transform;

        spriteDoDisco = SkillCheckHUD.GerarAnel(-1f, 0.97f, 0.02f, gerados);
        Sprite borda = SkillCheckHUD.GerarAnel(0.82f, 0.97f, 0.02f, gerados);
        Sprite anelFino = SkillCheckHUD.GerarAnel(0.935f, 0.985f, 0.012f, gerados);
        Sprite halo = SkillCheckHUD.GerarAnel(0.30f, 0.62f, 0.30f, gerados);

        // 1. Véu na tela toda.
        Image veu = Camada("Veu", null, CorDoVeu, 0f, rectDaRaiz);
        Esticar(veu.rectTransform);

        // 2. Dica: faixa escura com o centro a AlturaDaDica da base.
        Image faixa = Camada("Dica", null, CorEscura, 0f, rectDaRaiz);
        RectTransform rf = faixa.rectTransform;
        rf.anchorMin = rf.anchorMax = new Vector2(0.5f, 0f);
        rf.pivot = new Vector2(0.5f, 0.5f);
        rf.anchoredPosition = new Vector2(0f, AlturaDaDica);
        rf.sizeDelta = new Vector2(LarguraDaFaixa, AlturaDaFaixa);

        textoDaDica = Texto("Texto", rf, TamanhoDoTexto);
        Esticar(textoDaDica.rectTransform);

        // 3. Contagem: os pontos centrados a AlturaDaContagem da base.
        contagem = NovoRect("Contagem", rectDaRaiz);
        contagem.anchorMin = contagem.anchorMax = new Vector2(0.5f, 0f);
        contagem.pivot = new Vector2(0.5f, 0.5f);
        contagem.anchoredPosition = new Vector2(0f, AlturaDaContagem);
        contagem.sizeDelta = Vector2.zero;

        // 4 a 7: no campo, com a origem no centro da tela.
        RectTransform campo = NovoRect("Campo", rectDaRaiz);
        campo.sizeDelta = Vector2.zero;

        // 4. A próxima: a 0,5 de transparência, sem anel.
        proxima = MontarBolinha("Proxima", campo, spriteDoDisco, borda, halo, null);
        proxima.raiz.gameObject.AddComponent<CanvasGroup>().alpha = 0.5f;
        proxima.raiz.gameObject.SetActive(false);

        // 5. A da vez, com o anel fino que fecha.
        daVez = MontarBolinha("DaVez", campo, spriteDoDisco, borda, halo, anelFino);
        daVez.raiz.gameObject.SetActive(false);

        // 6. Estouro: anel verde e as gotinhas (9 a 14 px).
        estouro = NovoRect("Estouro", campo);
        estouro.sizeDelta = Vector2.zero;
        anelDoEstouro = Camada("Anel", borda, CorVerde, 0f, estouro);
        for (int i = 0; i < gotas.Length; i++)
        {
            float tamanho = Mathf.Lerp(9f, 14f, (i * 7 % 10) / 9f);
            gotas[i] = Camada("Gota", spriteDoDisco, CorDasGotas, tamanho, estouro);
        }
        estouro.gameObject.SetActive(false);

        // 7. Mira: sombra, anel branco e miolo amarelo.
        mira = NovoRect("Mira", campo);
        mira.sizeDelta = Vector2.zero;
        Camada("Sombra", spriteDoDisco, CorDaSombra, TamanhoDaSombra, mira);
        Camada("Anel", borda, Color.white, TamanhoDoAnelDaMira, mira);
        Camada("Miolo", spriteDoDisco, CorDoMiolo, TamanhoDoMiolo, mira);
    }

    /* Halo, corpo, borda, número e (só na da vez) o anel, nessa ordem. */
    private static Bolinha MontarBolinha(string nome, RectTransform pai, Sprite disco, Sprite borda, Sprite halo, Sprite anel)
    {
        Bolinha b = new Bolinha();
        b.raiz = NovoRect(nome, pai);
        b.raiz.sizeDelta = Vector2.zero;
        b.halo = Camada("Halo", halo, CorDoHalo, 0f, b.raiz);
        b.corpo = Camada("Corpo", disco, CorDoCorpo, 0f, b.raiz);
        b.borda = Camada("Borda", borda, Color.white, 0f, b.raiz);
        b.numero = Texto("Numero", b.raiz, TamanhoDoTexto);
        if (anel != null)
            b.anel = Camada("Anel", anel, Color.white, 0f, b.raiz);
        return b;
    }

    private static RectTransform NovoRect(string nome, Transform pai)
    {
        RectTransform rt = (RectTransform)new GameObject(nome, typeof(RectTransform)).transform;
        rt.SetParent(pai, false);
        return rt;
    }

    private static void Esticar(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    /* Image sem raycast, centrada no pai. Sem sprite = retângulo cheio. */
    private static Image Camada(string nome, Sprite sprite, Color cor, float tamanho, Transform pai)
    {
        Image imagem = NovoRect(nome, pai).gameObject.AddComponent<Image>();
        imagem.sprite = sprite;
        imagem.color = cor;
        imagem.raycastTarget = false;
        imagem.rectTransform.sizeDelta = Vector2.one * tamanho;
        return imagem;
    }

    /* Texto branco, negrito e centrado, sem raycast e numa linha só. */
    private static TextMeshProUGUI Texto(string nome, Transform pai, float tamanho)
    {
        TextMeshProUGUI tmp = NovoRect(nome, pai).gameObject.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = tamanho;
        tmp.color = Color.white;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.raycastTarget = false;
        return tmp;
    }
}
