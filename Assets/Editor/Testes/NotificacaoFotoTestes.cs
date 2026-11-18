using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using UnityEngine;

/* ============================================================
 *  NotificacaoFotoTestes.cs
 *  ---------------------------------------------------------------------
 *  Testes EditMode da miniatura da foto no banner push
 *  (NotificacaoCelular + HistoriaBuilder.GarantirMiniatura):
 *    - GarantirMiniatura rodado duas vezes não duplica a "Miniatura" e
 *      deixa o banner com 500 px;
 *    - post com foto: miniatura ligada com a textura, cabeçalho
 *      "enviou uma foto" e a área do texto mais curta (−118);
 *    - post sem foto depois de um com foto: tudo volta ao de hoje
 *      (miniatura desligada, autor + @, −18 e −90).
 *
 *  O banner de teste é montado à mão com os nomes e medidas do
 *  HistoriaBuilder.GarantirNotificacao (Notificacao 440×104, Cabecalho e
 *  Texto), ligado pelo TesteUtil.Setar; depois o GarantirMiniatura
 *  acrescenta a miniatura. O Awake não roda no EditMode e o HandlePost
 *  (o handler do OnAviso) é chamado direto — sem celular, conta como
 *  celular fechado.
 *
 *  Os objetos nascem pelo TesteUtil (escondidos, fora da cena) e são
 *  destruídos no [TearDown], junto com as texturas de teste.
 *
 *  Rodar:  Window > General > Test Runner > EditMode (categoria Fotos)
 *  ou      unity command run_tests --mode EditMode --filter_type category --filter Fotos
 * ============================================================ */

[Category("Fotos")]
public class NotificacaoFotoTestes
{
    private NotificacaoCelular notificacao;
    private RectTransform painel;
    private TextMeshProUGUI cabecalho;
    private TextMeshProUGUI texto;
    private readonly List<Texture2D> texturas = new List<Texture2D>();

    [SetUp]
    public void Preparar()
    {
        // Banner como o do HistoriaBuilder (antes das fotos): 440×104 no canto inferior direito.
        GameObject banner = TesteUtil.Novo("Notificacao");
        painel = banner.AddComponent<RectTransform>();
        painel.anchorMin = new Vector2(1f, 0f);
        painel.anchorMax = new Vector2(1f, 0f);
        painel.pivot = new Vector2(1f, 0f);
        painel.anchoredPosition = new Vector2(-40f, 150f);
        painel.sizeDelta = new Vector2(440f, 104f);
        notificacao = banner.AddComponent<NotificacaoCelular>();

        cabecalho = NovoTexto("Cabecalho");
        RectTransform cabRt = cabecalho.rectTransform;
        cabRt.anchorMin = new Vector2(0f, 1f);
        cabRt.anchorMax = new Vector2(1f, 1f);
        cabRt.pivot = new Vector2(0f, 1f);
        cabRt.anchoredPosition = new Vector2(74f, -14f);
        cabRt.sizeDelta = new Vector2(-90f, 24f);

        texto = NovoTexto("Texto");
        RectTransform textoRt = texto.rectTransform;
        textoRt.anchorMin = Vector2.zero;
        textoRt.anchorMax = Vector2.one;
        textoRt.pivot = new Vector2(0.5f, 0.5f);
        textoRt.offsetMin = new Vector2(74f, 12f);
        textoRt.offsetMax = new Vector2(-18f, -42f);

        TesteUtil.Setar(notificacao, "painel", painel);
        TesteUtil.Setar(notificacao, "cabecalho", cabecalho);
        TesteUtil.Setar(notificacao, "texto", texto);
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

    private TextMeshProUGUI NovoTexto(string nome)
    {
        TextMeshProUGUI tmp = TesteUtil.Com<TextMeshProUGUI>(nome);
        tmp.rectTransform.SetParent(painel, false);
        return tmp;
    }

    private Texture2D NovaTextura()
    {
        Texture2D t = new Texture2D(4, 4) { hideFlags = HideFlags.HideAndDontSave };
        texturas.Add(t);
        return t;
    }

    private static PostRedeSocial Post(string autor, string usuario, Texture foto)
    {
        return new PostRedeSocial
        {
            autor = autor,
            usuario = usuario,
            texto = "Agente, essa bromélia tá juntando água nas folhas. Dá uma olhada?",
            foto = foto,
        };
    }

    /* O que o menu faz no banner: acrescenta a miniatura e liga as referências. */
    private void MontarMiniatura()
    {
        Assert.IsTrue(HistoriaBuilder.GarantirMiniatura(notificacao, null), "o banner de teste ganha a miniatura");
    }

    private GameObject Miniatura()
    {
        Transform t = painel.Find("Miniatura");
        Assert.IsNotNull(t, "o banner tem a Miniatura");
        return t.gameObject;
    }

    private UnityEngine.UI.RawImage Imagem()
    {
        UnityEngine.UI.RawImage imagem = Miniatura().GetComponentInChildren<UnityEngine.UI.RawImage>(true);
        Assert.IsNotNull(imagem, "a Miniatura tem a RawImage");
        return imagem;
    }

    /* ---------------- Builder ---------------- */

    [Test]
    public void GarantirMiniatura_DuasVezes_NaoDuplica()
    {
        Assert.IsTrue(HistoriaBuilder.GarantirMiniatura(notificacao, null), "1ª chamada cria a miniatura");
        Assert.IsFalse(HistoriaBuilder.GarantirMiniatura(notificacao, null), "2ª chamada não cria nada");

        int miniaturas = 0;
        foreach (Transform filho in painel)
            if (filho.name == "Miniatura")
                miniaturas++;
        Assert.AreEqual(1, miniaturas, "um só filho Miniatura");
        Assert.AreEqual(500f, painel.rect.width, 1e-3f, "banner com 500 px");

        GameObject miniatura = Miniatura();
        Assert.IsFalse(miniatura.activeSelf, "a miniatura nasce desligada");
        RectTransform rt = (RectTransform)miniatura.transform;
        Assert.AreEqual(new Vector2(96f, 64f), rt.sizeDelta, "96×64");
        Assert.AreEqual(new Vector2(1f, 0.5f), rt.anchorMin, "âncora direita-meio");
        Assert.AreEqual(new Vector2(1f, 0.5f), rt.anchorMax, "âncora direita-meio");
        Assert.AreEqual(-14f, rt.anchoredPosition.x, 1e-4f, "margem 14 da direita");
        Assert.IsNotNull(miniatura.GetComponent<UnityEngine.UI.Mask>(), "com Mask");
        Assert.IsFalse(miniatura.GetComponent<UnityEngine.UI.Mask>().showMaskGraphic, "Mask sem gráfico");
        Assert.AreEqual("Imagem", Imagem().name, "Miniatura/Imagem");

        UnityEditor.SerializedObject so = new UnityEditor.SerializedObject(notificacao);
        Assert.AreSame(miniatura, so.FindProperty("miniatura").objectReferenceValue, "NotificacaoCelular.miniatura ligado");
        Assert.AreSame(Imagem(), so.FindProperty("miniaturaImagem").objectReferenceValue, "NotificacaoCelular.miniaturaImagem ligado");
    }

    /* ---------------- Banner ---------------- */

    [Test]
    public void Banner_ComFoto_MostraMiniaturaECabecalho()
    {
        MontarMiniatura();
        Texture2D foto = NovaTextura();
        notificacao.HandlePost(Post("Lúcia Andrade", "@lu.plantas", foto));

        Assert.IsTrue(Miniatura().activeSelf, "post com foto: miniatura ligada");
        Assert.AreSame(foto, Imagem().texture, "a textura do post vai para a miniatura");
        Assert.AreEqual("<b>Lúcia Andrade</b>  <color=#FFFFFF80>enviou uma foto</color>", cabecalho.text);
        Assert.AreEqual(-118f, texto.rectTransform.offsetMax.x, 1e-4f, "o texto termina antes da miniatura");
        Assert.AreEqual(-190f, cabecalho.rectTransform.sizeDelta.x, 1e-4f, "o cabeçalho também encurta");
    }

    [Test]
    public void Banner_PostSemFotoDepoisDeFoto_VoltaAoNormal()
    {
        MontarMiniatura();
        notificacao.HandlePost(Post("Lúcia Andrade", "@lu.plantas", NovaTextura()));
        notificacao.HandlePost(Post("Carlos Mendes", "@carlinhos.m", null));

        Assert.IsFalse(Miniatura().activeSelf, "post sem foto: miniatura desligada");
        Assert.AreEqual("<b>Carlos Mendes</b>  <color=#FFFFFF80>@carlinhos.m</color>", cabecalho.text);
        Assert.AreEqual(-18f, texto.rectTransform.offsetMax.x, 1e-4f, "o texto volta aos 18 px de hoje");
        Assert.AreEqual(-90f, cabecalho.rectTransform.sizeDelta.x, 1e-4f, "o cabeçalho volta ao de hoje");
    }
}
