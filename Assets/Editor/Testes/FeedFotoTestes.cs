using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

/* ============================================================
 *  FeedFotoTestes.cs
 *  ---------------------------------------------------------------------
 *  Testes EditMode da foto e do selo "RESOLVIDO" no card do feed
 *  (FeedRedeSocial + PostView + CelularBuilder.MontarCard):
 *    - o card montado pelo builder já nasce com a moldura "Foto" logo
 *      depois do "Texto", com a imagem e o selo, tudo desligado;
 *    - GarantirFotoNoCard rodado duas vezes não duplica a moldura;
 *    - post com foto liga a moldura com a textura; sem foto, desligada;
 *    - CarimbarFoco (o OnFocoEliminado do gerenciador) carimba só os
 *      cards daquele foco: foto escurecida e selo ligado;
 *    - post de um foco já concluído (posts recuperados no Start) entra
 *      carimbado.
 *
 *  O Awake/Start não rodam no EditMode: o feed de teste recebe o
 *  viewport, o conteúdo e o card modelo (montado pelo próprio
 *  CelularBuilder.MontarCard) pelo TesteUtil.Setar, e o Adicionar é
 *  chamado direto (é o handler do OnPostPublicado).
 *
 *  Os objetos nascem pelo TesteUtil (escondidos, fora da cena) e são
 *  destruídos no [TearDown], junto com as texturas de teste.
 *
 *  Rodar:  Window > General > Test Runner > EditMode (categoria Fotos)
 *  ou      unity command run_tests --mode EditMode --filter_type category --filter Fotos
 * ============================================================ */

[Category("Fotos")]
public class FeedFotoTestes
{
    private static readonly Color CorCarimbada = new Color(0.62f, 0.62f, 0.62f, 1f);

    private FeedRedeSocial feed;
    private RectTransform conteudo;
    private PostView modelo;
    private readonly List<Texture2D> texturas = new List<Texture2D>();

    [SetUp]
    public void Preparar()
    {
        feed = TesteUtil.Com<FeedRedeSocial>("Feed (teste)");
        RectTransform viewport = TesteUtil.Novo("Viewport (teste)").AddComponent<RectTransform>();
        conteudo = TesteUtil.Novo("Conteudo (teste)").AddComponent<RectTransform>();
        modelo = CelularBuilder.MontarCard(conteudo, null, null);

        TesteUtil.Setar(feed, "viewport", viewport);
        TesteUtil.Setar(feed, "conteudo", conteudo);
        TesteUtil.Setar(feed, "modelo", modelo);
    }

    [TearDown]
    public void Limpar()
    {
        TesteUtil.LimparTudo();
        foreach (Texture2D t in texturas)
            if (t != null)
                Object.DestroyImmediate(t);
        texturas.Clear();
    }

    /* ---------------- Ajudantes ---------------- */

    private Texture2D NovaTextura()
    {
        Texture2D t = new Texture2D(4, 4) { hideFlags = HideFlags.HideAndDontSave };
        texturas.Add(t);
        return t;
    }

    /* Foco de 1 clique, sem animação: o Interact() o conclui na hora. */
    private static Interactable NovoFoco(string nome)
    {
        Interactable foco = TesteUtil.Com<Interactable>(nome);
        TesteUtil.Setar(foco, "requiredClicks", 1);
        TesteUtil.Setar(foco, "punchScale", 0f);
        TesteUtil.Setar(foco, "shrinkDuration", 0f);
        return foco;
    }

    private static PostRedeSocial Post(Texture foto, Interactable foco)
    {
        return new PostRedeSocial
        {
            autor = "Carlos Mendes",
            usuario = "@carlinhos.m",
            texto = "Olha esse pneu.",
            curtidas = 3,
            foto = foto,
            foco = foco,
        };
    }

    /* O card que o Adicionar acabou de pôr no topo do feed. */
    private PostView Adicionar(PostRedeSocial post)
    {
        feed.Adicionar(post);
        PostView pv = conteudo.GetChild(0).GetComponent<PostView>();
        Assert.IsNotNull(pv, "o card novo entra no topo do conteúdo");
        Assert.AreNotSame(modelo, pv, "o card novo é um clone, não o modelo");
        return pv;
    }

    private static void AssertCor(Color esperada, Color atual, string mensagem)
    {
        Assert.AreEqual(esperada.r, atual.r, 1e-4f, mensagem + " (r)");
        Assert.AreEqual(esperada.g, atual.g, 1e-4f, mensagem + " (g)");
        Assert.AreEqual(esperada.b, atual.b, 1e-4f, mensagem + " (b)");
        Assert.AreEqual(esperada.a, atual.a, 1e-4f, mensagem + " (a)");
    }

    /* ---------------- Builder ---------------- */

    [Test]
    public void MontarCard_TemFotoESeloDesligados()
    {
        Assert.IsNotNull(modelo.foto, "PostView.foto ligado");
        Assert.IsNotNull(modelo.fotoImagem, "PostView.fotoImagem ligado");
        Assert.IsNotNull(modelo.selo, "PostView.selo ligado");

        Assert.IsFalse(modelo.foto.gameObject.activeSelf, "a moldura nasce desligada");
        Assert.IsFalse(modelo.selo.activeSelf, "o selo nasce desligado");

        Assert.AreEqual("Foto", modelo.foto.name);
        Assert.AreSame(modelo.transform, modelo.foto.parent, "a moldura é filha do card");
        Transform texto = modelo.transform.Find("Texto");
        Assert.IsNotNull(texto, "o card tem o Texto");
        Assert.AreEqual(texto.GetSiblingIndex() + 1, modelo.foto.GetSiblingIndex(), "a moldura vem logo depois do Texto");

        Assert.AreSame(modelo.foto, modelo.fotoImagem.transform.parent, "Foto/Imagem");
        Assert.AreSame(modelo.foto, modelo.selo.transform.parent, "Foto/Resolvido");
        Assert.AreEqual("Imagem", modelo.fotoImagem.name);
        Assert.AreEqual("Resolvido", modelo.selo.name);
    }

    [Test]
    public void GarantirFotoNoCard_DuasVezes_NaoDuplica()
    {
        // Card como o de uma cena antiga: sem a moldura e sem as referências.
        Object.DestroyImmediate(modelo.foto.gameObject);
        modelo.foto = null;
        modelo.fotoImagem = null;
        modelo.selo = null;

        Assert.IsTrue(CelularBuilder.GarantirFotoNoCard(modelo, null), "1ª chamada cria a moldura");
        Assert.IsFalse(CelularBuilder.GarantirFotoNoCard(modelo, null), "2ª chamada não cria nada");

        int fotos = 0;
        foreach (Transform filho in modelo.transform)
            if (filho.name == "Foto")
                fotos++;
        Assert.AreEqual(1, fotos, "um só filho Foto");

        Assert.IsNotNull(modelo.foto, "PostView.foto ligado");
        Assert.IsNotNull(modelo.fotoImagem, "PostView.fotoImagem ligado");
        Assert.IsNotNull(modelo.selo, "PostView.selo ligado");
        Assert.AreEqual(modelo.transform.Find("Texto").GetSiblingIndex() + 1, modelo.foto.GetSiblingIndex(),
            "a moldura vem logo depois do Texto");
    }

    /* ---------------- Feed ---------------- */

    [Test]
    public void Adicionar_ComFoto_MostraAFoto()
    {
        Texture2D foto = NovaTextura();
        PostView pv = Adicionar(Post(foto, NovoFoco("Pneu (teste)")));

        Assert.IsTrue(pv.foto.gameObject.activeSelf, "a moldura liga com a foto");
        Assert.AreSame(foto, pv.fotoImagem.texture, "a textura do post vai para a RawImage");
        Assert.IsFalse(pv.selo.activeSelf, "foco vivo: sem selo");
        Assert.IsFalse(FeedRedeSocial.Carimbado(pv));
        AssertCor(Color.white, pv.fotoImagem.color, "foto sem escurecer");
    }

    [Test]
    public void Adicionar_SemFoto_SemMoldura()
    {
        Adicionar(Post(NovaTextura(), NovoFoco("Pneu (teste)")));
        PostView semFoto = Adicionar(Post(null, null));

        Assert.IsFalse(semFoto.foto.gameObject.activeSelf, "post sem foto: moldura desligada");
        Assert.IsFalse(semFoto.selo.activeSelf);
    }

    [Test]
    public void CarimbarFoco_SoOsCardsDoFoco()
    {
        Interactable focoA = NovoFoco("Pneu A (teste)");
        Interactable focoB = NovoFoco("Pneu B (teste)");
        PostView cardA = Adicionar(Post(NovaTextura(), focoA));
        PostView cardB = Adicionar(Post(NovaTextura(), focoB));

        feed.CarimbarFoco(focoA);

        Assert.IsTrue(FeedRedeSocial.Carimbado(cardA), "o card do foco eliminado ganha o selo");
        AssertCor(CorCarimbada, cardA.fotoImagem.color, "a foto do foco eliminado escurece");
        Assert.IsFalse(FeedRedeSocial.Carimbado(cardB), "o card do outro foco fica como estava");
        AssertCor(Color.white, cardB.fotoImagem.color, "a foto do outro foco não escurece");
    }

    [Test]
    public void CarimbarFoco_ComPop_SeloComecaMaior()
    {
        Interactable foco = NovoFoco("Pneu (teste)");
        PostView card = Adicionar(Post(NovaTextura(), foco));

        feed.CarimbarFoco(foco);

        Assert.AreEqual(0f, card.animSelo, 1e-5f, "o pop começa do zero");
        Assert.AreEqual(1.3f, card.selo.transform.localScale.x, 1e-4f, "o selo entra a 1,3");
    }

    [Test]
    public void CarimbarFoco_SemCard_NaoQuebra()
    {
        Adicionar(Post(null, null));
        Assert.DoesNotThrow(() => feed.CarimbarFoco(NovoFoco("Outro (teste)")));
        Assert.DoesNotThrow(() => feed.CarimbarFoco(null));
    }

    [Test]
    public void Adicionar_FocoJaConcluido_EntraCarimbado()
    {
        Interactable foco = NovoFoco("Pneu (teste)");
        foco.Interact();
        Assert.IsTrue(foco.IsComplete, "o foco de teste concluiu");

        PostView card = Adicionar(Post(NovaTextura(), foco));

        Assert.IsTrue(FeedRedeSocial.Carimbado(card), "foco já eliminado: o card entra carimbado");
        AssertCor(CorCarimbada, card.fotoImagem.color, "foto escurecida");
        Assert.AreEqual(1f, card.animSelo, 1e-5f, "sem pop");
        Assert.AreEqual(1f, card.selo.transform.localScale.x, 1e-4f, "selo no tamanho final");
    }
}
