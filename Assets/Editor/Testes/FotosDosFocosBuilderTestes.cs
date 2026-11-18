using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

/* ============================================================
 *  FotosDosFocosBuilderTestes.cs
 *  ---------------------------------------------------------------------
 *  Testes EditMode do builder das fotos dos focos
 *  (FotosDosFocosBuilder):
 *    - NomeDoArquivo: sem o grupo do topo, "/" vira " - ", espaços
 *      repetidos viram um;
 *    - LerPoses: lê o TSV aprovado sem tirar os espaços do caminho;
 *    - SementeDe: estável e diferente para caminhos diferentes;
 *    - DesenharCirculo: pinta a borda em volta do retângulo, não o centro;
 *    - Importar: dois focos com o mesmo nome ("pneu (5)") são ligados
 *      pelo caminho na hierarquia, cada um ao PNG do seu número; foco sem
 *      linha e linha sem foco saem nos avisos;
 *    - Gerar: grava um PNG 720×480 e liga a textura no FotoDoFoco;
 *    - PrepararCelular: rodado duas vezes não duplica nada.
 *
 *  As fotos aprovadas de teste ficam numa pasta temporária fora do
 *  projeto; a pasta de destino é PastaDeTeste, dentro de Assets/, e é
 *  apagada (com os .meta) no [TearDown], junto com a temporária. Os
 *  objetos nascem pelo TesteUtil (escondidos, fora da cena) e são
 *  destruídos no [TearDown].
 *
 *  Rodar:  Window > General > Test Runner > EditMode (categoria Fotos)
 *  ou      unity command run_tests --mode EditMode --filter_type category --filter Fotos
 * ============================================================ */

[Category("Fotos")]
public class FotosDosFocosBuilderTestes
{
    private const string PastaDeTeste = "Assets/FotosDosFocosTeste";

    private string pastaAprovadas;
    private readonly List<Texture2D> texturas = new List<Texture2D>();

    [SetUp]
    public void Preparar()
    {
        pastaAprovadas = Path.Combine(Path.GetTempPath(), "FotosDosFocosTeste-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(pastaAprovadas);
    }

    [TearDown]
    public void Limpar()
    {
        TesteUtil.LimparTudo();
        foreach (Texture2D t in texturas)
            if (t != null)
                Object.DestroyImmediate(t);
        texturas.Clear();

        if (AssetDatabase.IsValidFolder(PastaDeTeste))
            AssetDatabase.DeleteAsset(PastaDeTeste);
        if (Directory.Exists(pastaAprovadas))
            Directory.Delete(pastaAprovadas, true);
    }

    /* ---------------- Ajudantes ---------------- */

    /* GameObject de teste com o nome dado, filho de 'pai' (ou na raiz). */
    private static Transform Filho(string nome, Transform pai)
    {
        Transform t = TesteUtil.Novo(nome).transform;
        t.SetParent(pai, false);
        return t;
    }

    private static PneuInteractable Pneu(string nome, Transform pai)
    {
        PneuInteractable pneu = TesteUtil.Com<PneuInteractable>(nome);
        pneu.transform.SetParent(pai, false);
        return pneu;
    }

    /* Grava em pastaAprovadas um PNG 4×4 de uma cor só; devolve os bytes. */
    private byte[] GravarPng(string nome, Color32 cor)
    {
        Texture2D t = new Texture2D(4, 4, TextureFormat.RGBA32, false) { hideFlags = HideFlags.HideAndDontSave };
        texturas.Add(t);
        Color32[] px = new Color32[16];
        for (int i = 0; i < px.Length; i++)
            px[i] = cor;
        t.SetPixels32(px);
        t.Apply();
        byte[] bytes = t.EncodeToPNG();
        File.WriteAllBytes(Path.Combine(pastaAprovadas, nome), bytes);
        return bytes;
    }

    private void GravarPoses(params string[] linhas)
    {
        File.WriteAllText(Path.Combine(pastaAprovadas, "poses.tsv"), string.Join("\n", linhas) + "\n");
    }

    /* A cor do 1º pixel do arquivo da textura (o asset não é legível). */
    private Color32 CorDoArquivo(Texture2D foto)
    {
        string caminho = AssetDatabase.GetAssetPath(foto);
        Assert.IsNotEmpty(caminho, "a foto é um asset");
        Texture2D lida = new Texture2D(2, 2) { hideFlags = HideFlags.HideAndDontSave };
        texturas.Add(lida);
        Assert.IsTrue(lida.LoadImage(File.ReadAllBytes(caminho)), "o arquivo da foto é um PNG");
        return lida.GetPixels32()[0];
    }

    private static Color32 Cinza => new Color32(128, 128, 128, 255);

    /* ---------------- Nome, poses e semente ---------------- */

    [Test]
    public void NomeDoArquivo_TiraOGrupoEJuntaComTraco()
    {
        Assert.AreEqual("PilhadePneu - pneu (5).png",
            FotosDosFocosBuilder.NomeDoArquivo("--- FOCOS ---/PilhadePneu/pneu (5)"));
        Assert.AreEqual("Terrenos - Cenario Terreno Baldio - Lixo - Garrafa (1).png",
            FotosDosFocosBuilder.NomeDoArquivo("--- BAIRRO ---/Terrenos/Cenario Terreno Baldio/Lixo/Garrafa  (1)"));
        Assert.AreEqual("Caixa d'Agua (1).png",
            FotosDosFocosBuilder.NomeDoArquivo("--- FOCOS ---/Caixa d'Agua (1)"));
    }

    [Test]
    public void CaminhoNaHierarquia_JuntaOsPaisComBarra()
    {
        Transform raiz = Filho("--- FOCOS ---", null);
        Transform pilha = Filho("PilhadePneu", raiz);
        Transform pneu = Filho("pneu (5)", pilha);

        Assert.AreEqual("--- FOCOS ---/PilhadePneu/pneu (5)", FotosDosFocosBuilder.CaminhoNaHierarquia(pneu));
        Assert.AreEqual("--- FOCOS ---", FotosDosFocosBuilder.CaminhoNaHierarquia(raiz));
    }

    [Test]
    public void LerPoses_LeTsvComEspacosNoCaminho()
    {
        List<FotosDosFocosBuilder.Pose> poses = FotosDosFocosBuilder.LerPoses(
            "\r\n07\t--- BAIRRO ---/Terrenos/Cenario Terreno Baldio/Lixo/Garrafa \t-18.114\t1.783\t63.445\t12.794\t195.000\t0.000\t60.0\r\n\r\n");

        Assert.AreEqual(1, poses.Count, "a linha vazia é ignorada");
        FotosDosFocosBuilder.Pose p = poses[0];
        Assert.AreEqual(7, p.numero);
        Assert.AreEqual("--- BAIRRO ---/Terrenos/Cenario Terreno Baldio/Lixo/Garrafa ", p.caminho, "o espaço do fim fica");
        Assert.AreEqual(-18.114f, p.posicao.x, 1e-4f);
        Assert.AreEqual(1.783f, p.posicao.y, 1e-4f);
        Assert.AreEqual(63.445f, p.posicao.z, 1e-4f);
        Assert.AreEqual(12.794f, p.rotacao.x, 1e-4f);
        Assert.AreEqual(195f, p.rotacao.y, 1e-4f);
        Assert.AreEqual(0f, p.rotacao.z, 1e-4f);
        Assert.AreEqual(60f, p.fov, 1e-4f);
    }

    [Test]
    public void SementeDe_Estavel()
    {
        int a1 = FotosDosFocosBuilder.SementeDe("--- FOCOS ---/pneu (5)");
        int a2 = FotosDosFocosBuilder.SementeDe("--- FOCOS ---/pneu (5)");
        int b = FotosDosFocosBuilder.SementeDe("--- FOCOS ---/PilhadePneu/pneu (5)");

        Assert.AreEqual(a1, a2, "o mesmo caminho dá a mesma semente");
        Assert.AreNotEqual(a1, b, "caminhos diferentes dão sementes diferentes");
        Assert.GreaterOrEqual(a1, 0);
        Assert.GreaterOrEqual(b, 0);
    }

    /* ---------------- Círculo ---------------- */

    [Test]
    public void DesenharCirculo_PintaABordaENaoOCentro()
    {
        const int largura = 200, altura = 100;
        Color32[] px = new Color32[largura * altura];
        for (int i = 0; i < px.Length; i++)
            px[i] = Cinza;

        FotosDosFocosBuilder.DesenharCirculo(px, largura, altura, new Rect(90f, 40f, 20f, 20f), 12345);

        Assert.AreEqual(Cinza, px[50 * largura + 100], "o centro continua cinza");

        bool achou = false;
        for (int y = 0; y < altura && !achou; y++)
            for (int x = 0; x < largura && !achou; x++)
            {
                Color32 c = px[y * largura + x];
                float dx = x - 100f, dy = y - 50f;
                if (c.r == 232 && c.g == 30 && c.b == 30 && dx * dx + dy * dy >= 25f * 25f)
                    achou = true;
            }
        Assert.IsTrue(achou, "há traço vermelho (232, 30, 30) a 25 px ou mais do centro");
    }

    /* ---------------- Importar ---------------- */

    [Test]
    public void Importar_DoisComMesmoNome_LigaPeloCaminho()
    {
        byte[] vermelho = GravarPng("01 - Pneu - pneu (5).png", new Color32(255, 0, 0, 255));
        byte[] azul = GravarPng("02 - Pneu - pneu (5).png", new Color32(0, 0, 255, 255));
        GravarPoses(
            "01\t--- FOCOS ---/pneu (5)\t1.000\t2.000\t3.000\t10.000\t20.000\t0.000\t60.0",
            "02\t--- FOCOS ---/PilhadePneu/pneu (5)\t4.000\t5.000\t6.000\t11.000\t21.000\t0.000\t55.0");

        Transform raiz = Filho("--- FOCOS ---", null);
        PneuInteractable solto = Pneu("pneu (5)", raiz);
        PneuInteractable naPilha = Pneu("pneu (5)", Filho("PilhadePneu", raiz));

        StringBuilder avisos = new StringBuilder();
        int ligados = FotosDosFocosBuilder.Importar(pastaAprovadas, PastaDeTeste,
            new Interactable[] { solto, naPilha }, avisos);

        Assert.AreEqual(2, ligados, "os dois focos ligados");
        Assert.AreEqual("", avisos.ToString(), "sem avisos");

        FotoDoFoco fSolto = solto.GetComponent<FotoDoFoco>();
        FotoDoFoco fPilha = naPilha.GetComponent<FotoDoFoco>();
        Assert.IsNotNull(fSolto, "o pneu solto ganhou o FotoDoFoco");
        Assert.IsNotNull(fPilha, "o pneu da pilha ganhou o FotoDoFoco");
        Assert.IsNotNull(fSolto.foto);
        Assert.IsNotNull(fPilha.foto);

        Assert.AreEqual(new Color32(255, 0, 0, 255), CorDoArquivo(fSolto.foto), "o solto tem a foto 01 (vermelha)");
        Assert.AreEqual(new Color32(0, 0, 255, 255), CorDoArquivo(fPilha.foto), "o da pilha tem a foto 02 (azul)");
        CollectionAssert.AreEqual(vermelho, File.ReadAllBytes(AssetDatabase.GetAssetPath(fSolto.foto)), "cópia byte a byte");
        CollectionAssert.AreEqual(azul, File.ReadAllBytes(AssetDatabase.GetAssetPath(fPilha.foto)), "cópia byte a byte");
        Assert.AreEqual(PastaDeTeste + "/pneu (5).png", AssetDatabase.GetAssetPath(fSolto.foto));
        Assert.AreEqual(PastaDeTeste + "/PilhadePneu - pneu (5).png", AssetDatabase.GetAssetPath(fPilha.foto));

        Assert.AreEqual(new Vector3(1f, 2f, 3f), fSolto.posicaoDaCamera);
        Assert.AreEqual(new Vector3(10f, 20f, 0f), fSolto.rotacaoDaCamera);
        Assert.AreEqual(60f, fSolto.campoDeVisao, 1e-4f);
        Assert.AreEqual(new Vector3(4f, 5f, 6f), fPilha.posicaoDaCamera);
        Assert.AreEqual(new Vector3(11f, 21f, 0f), fPilha.rotacaoDaCamera);
        Assert.AreEqual(55f, fPilha.campoDeVisao, 1e-4f);

        TextureImporter ti = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(fSolto.foto));
        Assert.AreEqual(TextureImporterType.Default, ti.textureType);
        Assert.IsFalse(ti.mipmapEnabled, "sem mipmaps");
        Assert.AreEqual(TextureWrapMode.Clamp, ti.wrapMode);
        Assert.IsTrue(ti.sRGBTexture, "sRGB");
        Assert.AreEqual(1024, ti.maxTextureSize);
        Assert.AreEqual(TextureImporterCompression.Compressed, ti.textureCompression);
    }

    [Test]
    public void Importar_FocoSemLinhaELinhaSemFoco_AvisaESegue()
    {
        GravarPng("01 - Pneu - pneu (1).png", new Color32(255, 0, 0, 255));
        GravarPng("02 - Pneu - pneu (2).png", new Color32(0, 255, 0, 255));
        GravarPoses(
            "01\t--- FOCOS ---/pneu (1)\t1\t2\t3\t0\t0\t0\t60",
            "02\t--- FOCOS ---/pneu (2)\t1\t2\t3\t0\t0\t0\t60");

        Transform raiz = Filho("--- FOCOS ---", null);
        PneuInteractable comLinha = Pneu("pneu (1)", raiz);
        PneuInteractable semLinha = Pneu("pneu (9)", raiz);

        StringBuilder avisos = new StringBuilder();
        int ligados = FotosDosFocosBuilder.Importar(pastaAprovadas, PastaDeTeste,
            new Interactable[] { comLinha, semLinha }, avisos);

        Assert.AreEqual(1, ligados, "o foco com linha é ligado mesmo com os avisos");
        Assert.IsNotNull(comLinha.GetComponent<FotoDoFoco>());
        Assert.IsNull(semLinha.GetComponent<FotoDoFoco>(), "o foco sem linha fica como estava");
        StringAssert.Contains("--- FOCOS ---/pneu (2)", avisos.ToString(), "a linha sem foco sai no aviso");
        StringAssert.Contains("--- FOCOS ---/pneu (9)", avisos.ToString(), "o foco sem linha sai no aviso");
    }

    /* ---------------- Gerar ---------------- */

    [Test]
    public void Gerar_GravaPng720x480ELigaATextura()
    {
        GameObject cubo = TesteUtil.Primitivo(PrimitiveType.Cube, "Foco (teste)");
        cubo.AddComponent<Interactable>();
        FotoDoFoco f = cubo.AddComponent<FotoDoFoco>();
        f.posicaoDaCamera = new Vector3(0f, 0f, -3f);
        f.rotacaoDaCamera = Vector3.zero;
        f.campoDeVisao = 60f;

        string caminho = FotosDosFocosBuilder.Gerar(f, PastaDeTeste);

        Assert.AreEqual(PastaDeTeste + "/Foco (teste).png", caminho);
        Assert.IsTrue(File.Exists(caminho), "o PNG foi gravado");
        Texture2D lida = new Texture2D(2, 2) { hideFlags = HideFlags.HideAndDontSave };
        texturas.Add(lida);
        Assert.IsTrue(lida.LoadImage(File.ReadAllBytes(caminho)));
        Assert.AreEqual(720, lida.width);
        Assert.AreEqual(480, lida.height);

        Assert.IsNotNull(f.foto, "a textura foi ligada");
        Assert.AreEqual(caminho, AssetDatabase.GetAssetPath(f.foto));
        Assert.AreEqual(720, f.foto.width, "importada sem mudar o tamanho");
        Assert.AreEqual(480, f.foto.height);
    }

    /* ---------------- Preparar celular ---------------- */

    [Test]
    public void PrepararCelular_DuasVezes_NaoDuplica()
    {
        GameObject card = TesteUtil.Novo("ModeloPost");
        card.AddComponent<RectTransform>();
        PostView modelo = card.AddComponent<PostView>();
        FeedRedeSocial feed = TesteUtil.Com<FeedRedeSocial>("Feed (teste)");
        TesteUtil.Setar(feed, "modelo", modelo);

        GameObject banner = TesteUtil.Novo("Notificacao");
        banner.AddComponent<RectTransform>().sizeDelta = new Vector2(440f, 104f);
        NotificacaoCelular notificacao = banner.AddComponent<NotificacaoCelular>();

        StringBuilder log = new StringBuilder();
        FotosDosFocosBuilder.PrepararCelular(feed, notificacao, null, log);
        FotosDosFocosBuilder.PrepararCelular(feed, notificacao, null, log);

        int fotos = 0, miniaturas = 0;
        foreach (Transform t in card.transform)
            if (t.name == "Foto") fotos++;
        foreach (Transform t in banner.transform)
            if (t.name == "Miniatura") miniaturas++;
        Assert.AreEqual(1, fotos, "uma moldura no card");
        Assert.AreEqual(1, miniaturas, "uma miniatura no banner");
        Assert.IsNotNull(modelo.foto, "as referências do card ligadas");
        Assert.IsNotNull(modelo.fotoImagem);
        Assert.IsNotNull(modelo.selo);
    }
}
