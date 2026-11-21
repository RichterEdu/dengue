using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;

/* ============================================================
 *  BorrifadorDePertoTestes.cs
 *  ---------------------------------------------------------------------
 *  Testes EditMode do BorrifadorDePerto: o giro que aponta o bico para a
 *  mira como ela aparece na tela (GiroParaAMira, sem NaN com a mira em
 *  cima do bico) e a cópia do borrifador presa à câmera (Montar): filha
 *  da câmera, inativa, na PosicaoNaCamera, sem sombra e com a névoa
 *  herdada ajustada para perto, sem mexer no original. Ao ativar
 *  (Ativar), a cópia já entra apontada para a mira, sem virar.
 *
 *  O "original" de teste imita o borrifador da mão: um objeto inativo
 *  com um filho de MeshRenderer (sombra ligada) e um filho Nevoa com
 *  ParticleSystem. A cópia é filha da câmera de teste e é destruída
 *  junto, no TearDown (e explicitamente, por garantia).
 *
 *  Rodar:  unity command run_tests --mode EditMode --filter_type category --filter Minigames
 * ============================================================ */

[Category("Minigames")]
public class BorrifadorDePertoTestes
{
    private Camera camera;
    private GameObject original;
    private MeshRenderer corpoDoOriginal;
    private BorrifadorDePerto borrifador;
    private GameObject copia;

    [SetUp]
    public void Preparar()
    {
        camera = TesteUtil.Com<Camera>("Câmera (teste)");
        borrifador = TesteUtil.Com<BorrifadorDePerto>("Player (teste)");

        original = TesteUtil.Novo("Borrifador na mão (teste)");
        corpoDoOriginal = TesteUtil.Com<MeshRenderer>("Corpo");
        corpoDoOriginal.transform.SetParent(original.transform, false);
        corpoDoOriginal.shadowCastingMode = ShadowCastingMode.On;
        ParticleSystem nevoa = TesteUtil.Com<ParticleSystem>("Nevoa");
        nevoa.transform.SetParent(original.transform, false);
        original.SetActive(false);
    }

    [TearDown]
    public void Limpar()
    {
        if (copia != null)
            Object.DestroyImmediate(copia);
        copia = null;
        TesteUtil.LimparTudo();
    }

    [Test]
    public void GiroApontaParaAMiraNaTela()
    {
        Vector3 bico = new Vector3(0.17f, -0.08f, 0.36f);

        Vector3 f = BorrifadorDePerto.GiroParaAMira(Vector2.zero, 40f, bico, 30f) * Vector3.forward;
        Assert.Less(f.x, 0f, "a mira no centro fica à esquerda do bico");
        Assert.Greater(f.y, 0f, "e acima dele");
        Assert.AreEqual(0.5f, f.z, 1e-4f, "30° para dentro da cena");

        f = BorrifadorDePerto.GiroParaAMira(new Vector2(1.7f, -0.6f), 40f, bico, 30f) * Vector3.forward;
        Assert.Greater(f.x, 0f, "a mira na borda direita fica à direita do bico");
    }

    [Test]
    public void MiraEmCimaDoBicoNaoQuebra()
    {
        Vector3 bico = new Vector3(0.17f, -0.08f, 0.36f);
        float escala = Mathf.Tan(20f * Mathf.Deg2Rad) * bico.z;
        Vector2 mira = new Vector2(bico.x, bico.y) / escala;

        Quaternion giro = BorrifadorDePerto.GiroParaAMira(mira, 40f, bico, 30f);

        foreach (float c in new[] { giro.x, giro.y, giro.z, giro.w })
            Assert.IsFalse(float.IsNaN(c) || float.IsInfinity(c), "giro finito: " + giro);
    }

    [Test]
    public void CopiaPresaACameraSemSombraEComUmaNevoaSo()
    {
        copia = borrifador.Montar(camera.transform, original.transform);

        Assert.IsNotNull(copia);
        Assert.AreSame(camera.transform, copia.transform.parent);
        Assert.IsFalse(copia.activeSelf);
        Assert.AreEqual(BorrifadorDePerto.PosicaoNaCamera, copia.transform.localPosition);
        Assert.AreEqual(Vector3.one, copia.transform.localScale);

        foreach (Renderer r in copia.GetComponentsInChildren<Renderer>(true))
            Assert.AreEqual(ShadowCastingMode.Off, r.shadowCastingMode, r.name + " sem sombra");

        ParticleSystem[] sistemas = copia.GetComponentsInChildren<ParticleSystem>(true);
        Assert.AreEqual(1, sistemas.Length, "só a névoa herdada");
        Assert.IsFalse(sistemas[0].emission.enabled);
        Assert.AreEqual(0.02f, sistemas[0].main.startSize.constant, 1e-5f);
        Assert.AreEqual(0.8f, sistemas[0].main.startSpeed.constant, 1e-5f);

        Assert.IsFalse(original.activeSelf, "o original segue inativo");
        Assert.AreEqual(ShadowCastingMode.On, corpoDoOriginal.shadowCastingMode, "e com a sombra");

        Assert.AreSame(copia, borrifador.Montar(camera.transform, original.transform), "monta uma vez só");
    }

    [Test]
    public void AoAtivarJaApontaParaAMira()
    {
        copia = borrifador.Montar(camera.transform, original.transform);
        copia.transform.localRotation = Quaternion.Euler(299f, 329f, 19f);   // o giro herdado da mão
        Vector2 mira = new Vector2(0.3f, -0.2f);

        borrifador.Ativar(mira, 40f);

        Assert.IsTrue(copia.activeSelf);
        Vector3 bico = BorrifadorDePerto.PosicaoNaCamera + copia.transform.localRotation * BorrifadorDePerto.BicoLocal;
        Quaternion alvo = BorrifadorDePerto.GiroParaAMira(mira, 40f, bico, BorrifadorDePerto.MergulhoGraus);
        Assert.Less(Quaternion.Angle(copia.transform.localRotation, alvo), 0.5f, "entra já apontado, sem virar");
    }

    [Test]
    public void SemOriginalNaoMonta()
    {
        GameObject montada = null;
        Assert.DoesNotThrow(() => montada = borrifador.Montar(camera.transform, null));
        Assert.IsNull(montada);
    }
}
