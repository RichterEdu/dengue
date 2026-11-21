using NUnit.Framework;
using UnityEngine;

/* ============================================================
 *  CameraJogadorTestes.cs
 *  ---------------------------------------------------------------------
 *  Testes EditMode (Minigames) do enquadramento temporário da câmera
 *  (CameraJogador.Enquadrar / SoltarEnquadramento): a câmera vai dos
 *  olhos até a pose pedida em SegundosDeIda e volta em SegundosDeVolta,
 *  contando o tempo sem escala (dtReal), inclusive com o jogo pausado.
 *  Soltar no meio da ida ou enquadrar de novo no meio da volta continua
 *  de onde a câmera está, sem salto. O FOV do menu (FovBase) só aparece
 *  na câmera fora do enquadramento. dtReal inválido não anda, e sem
 *  jogador nada lança.
 *
 *  O Awake não roda no EditMode: o jogador é ligado pelo TesteUtil.Setar
 *  e a câmera é achada no primeiro uso. Parado, sem pulo e olhando reto,
 *  os olhos ficam no pivô de 1,5 m mais (0,10 à frente, 0,13 acima).
 *
 *  Rodar:  unity command run_tests --mode EditMode --filter_type category --filter Minigames
 * ============================================================ */

[Category("Minigames")]
public class CameraJogadorTestes
{
    private const float Q = 1f / 60f;

    private static readonly Vector3 Olhos = new Vector3(10f, 1.63f, 5.10f);

    private static readonly PoseDePerto P = new PoseDePerto
    {
        Posicao = new Vector3(12f, 0.9f, 6f),
        Giro = Quaternion.LookRotation(Vector3.down, Vector3.forward),
        Fov = 40f
    };

    private ControladorJogador jogador;
    private CameraJogador camera;
    private Camera cam;

    [SetUp]
    public void Preparar()
    {
        jogador = TesteUtil.Com<ControladorJogador>("Jogador (teste)");
        jogador.transform.position = new Vector3(10f, 0f, 5f);
        camera = TesteUtil.Com<CameraJogador>("Câmera (teste)");
        TesteUtil.Setar(camera, "jogador", jogador);
        camera.FovBase = 70f;
        cam = camera.GetComponent<Camera>();
    }

    [TearDown]
    public void Limpar()
    {
        TesteUtil.LimparTudo();
    }

    private void Chegar()
    {
        camera.Enquadrar(P);
        camera.Posicionar(Q, 1f);
    }

    private Vector3 Posicao => camera.transform.position;

    private static void Perto(Vector3 esperado, Vector3 real, float tolerancia, string msg)
    {
        Assert.Less(Vector3.Distance(esperado, real), tolerancia, $"{msg}: esperado {esperado:F4}, veio {real:F4}");
    }

    [Test]
    public void SemEnquadrarFicaNosOlhos()
    {
        camera.Posicionar(Q, Q);

        Perto(Olhos, Posicao, 1e-3f, "nos olhos");
        Assert.IsFalse(camera.Enquadrada);
        Assert.AreEqual(70f, cam.fieldOfView, 1e-4f);
    }

    [Test]
    public void EnquadradaChegaAPosePedida()
    {
        camera.Enquadrar(P);
        camera.Posicionar(Q, 0.15f);

        Assert.IsTrue(camera.Enquadrada);
        Assert.Greater(Vector3.Distance(Posicao, Olhos), 0.01f, "saiu dos olhos");
        Assert.Greater(Vector3.Distance(Posicao, P.Posicao), 0.01f, "ainda não chegou");

        camera.Posicionar(Q, 0.15f);

        Perto(P.Posicao, Posicao, 1e-4f, "na pose");
        Assert.Less(Quaternion.Angle(camera.transform.rotation, P.Giro), 0.01f);
        Assert.AreEqual(40f, cam.fieldOfView, 1e-4f);
    }

    [Test]
    public void SoltaVoltaAosOlhos()
    {
        Chegar();
        camera.SoltarEnquadramento();
        Assert.IsTrue(camera.Enquadrada, "voltando ainda conta como enquadrada");

        camera.Posicionar(Q, 0.125f);
        Assert.Greater(Vector3.Distance(Posicao, Olhos), 0.01f, "meio do caminho: longe dos olhos");
        Assert.Greater(Vector3.Distance(Posicao, P.Posicao), 0.01f, "meio do caminho: longe da pose");

        camera.Posicionar(Q, 0.125f);
        Perto(Olhos, Posicao, 1e-3f, "de volta aos olhos");
        Assert.AreEqual(70f, cam.fieldOfView, 1e-4f);
        Assert.IsFalse(camera.Enquadrada);
    }

    [Test]
    public void SoltaComOJogoPausadoVolta()
    {
        Chegar();
        camera.SoltarEnquadramento();
        camera.Posicionar(0f, 0.25f);

        Perto(Olhos, Posicao, 1e-3f, "de volta aos olhos com dt 0");
        Assert.IsFalse(camera.Enquadrada);
    }

    [Test]
    public void SoltarNoMeioDaIdaVoltaDeOndeEstava()
    {
        camera.Enquadrar(P);
        camera.Posicionar(Q, 0.15f);
        Vector3 a = Posicao;

        camera.SoltarEnquadramento();
        camera.Posicionar(Q, 0.0001f);
        Perto(a, Posicao, 0.01f, "sem salto ao soltar");

        camera.Posicionar(Q, 0.25f);
        Perto(Olhos, Posicao, 1e-3f, "de volta aos olhos");
    }

    [Test]
    public void EnquadrarDeNovoNaVoltaNaoSalta()
    {
        Chegar();
        camera.SoltarEnquadramento();
        camera.Posicionar(Q, 0.125f);
        Vector3 a = Posicao;

        camera.Enquadrar(P);
        camera.Posicionar(Q, 0.0001f);
        Perto(a, Posicao, 0.01f, "sem salto ao enquadrar de novo");

        camera.Posicionar(Q, 0.3f);
        Perto(P.Posicao, Posicao, 1e-4f, "na pose");
    }

    [Test]
    public void FovDoMenuNaoMexeNoEnquadramento()
    {
        Chegar();
        camera.FovBase = 90f;
        Assert.AreEqual(40f, cam.fieldOfView, 1e-4f, "o menu não mexe na vista de perto");

        camera.SoltarEnquadramento();
        camera.Posicionar(Q, 1f);
        Assert.AreEqual(90f, cam.fieldOfView, 1e-4f, "de volta com o FOV novo");
    }

    [Test]
    public void DtRealInvalidoNaoAnda()
    {
        camera.Enquadrar(P);
        camera.Posicionar(Q, 0f);
        camera.Posicionar(Q, float.NaN);
        camera.Posicionar(Q, -1f);

        Perto(Olhos, Posicao, 1e-3f, "não andou");
        Assert.IsTrue(camera.Enquadrada);
    }

    [Test]
    public void SemJogadorNaoLanca()
    {
        CameraJogador sozinha = TesteUtil.Com<CameraJogador>("Câmera sem jogador (teste)");

        Assert.DoesNotThrow(() =>
        {
            sozinha.Enquadrar(P);
            sozinha.Posicionar(Q, Q);
        });
    }
}
