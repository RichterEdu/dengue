using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

/* ============================================================
 *  EscolhaDaFotoTestes.cs
 *  ---------------------------------------------------------------------
 *  Testes EditMode da escolha do foco que sai em foto (EscolhaDaFoto):
 *  o mais perto do jogador vence; o não elegível fica de fora; o foco
 *  perto de um lugar conhecido fica de fora na 1ª passada; a 2ª passada
 *  repete um fotografado, menos o da última foto; sem nada, −1.
 *
 *  Lógica pura: os candidatos são montados à mão, sem cena e sem
 *  objetos. O jogador fica na origem.
 *
 *  Rodar:  Window > General > Test Runner > EditMode (categoria Fotos)
 *  ou      unity command run_tests --mode EditMode --filter_type category --filter Fotos
 * ============================================================ */

[Category("Fotos")]
public class EscolhaDaFotoTestes
{
    private const float Raio = 4f;
    private static readonly Vector3 Jogador = Vector3.zero;

    private static CandidatoDaFoto Candidato(Vector3 posicao, bool elegivel = true,
                                             bool fotografado = false, bool conhecido = false)
    {
        return new CandidatoDaFoto
        {
            posicao = posicao,
            elegivel = elegivel,
            fotografado = fotografado,
            conhecido = conhecido
        };
    }

    [Test]
    public void Escolher_MaisPertoVence()
    {
        var candidatos = new List<CandidatoDaFoto>
        {
            Candidato(new Vector3(10f, 0f, 0f)),
            Candidato(new Vector3(0f, 0f, 3f)),
            Candidato(new Vector3(-7f, 0f, 0f))
        };

        Assert.AreEqual(1, EscolhaDaFoto.Escolher(candidatos, Jogador, Raio, -1));
    }

    [Test]
    public void Escolher_NaoElegivel_FicaDeFora()
    {
        var candidatos = new List<CandidatoDaFoto>
        {
            Candidato(new Vector3(10f, 0f, 0f)),
            Candidato(new Vector3(0f, 0f, 3f), elegivel: false),
            Candidato(new Vector3(-7f, 0f, 0f))
        };

        Assert.AreEqual(2, EscolhaDaFoto.Escolher(candidatos, Jogador, Raio, -1));
    }

    [Test]
    public void Escolher_PertoDeLugarConhecido_FicaDeFora()
    {
        // 0: conhecido (eliminado), 5 m do jogador.
        // 1: A, 2 m do jogador e 3 m do conhecido.
        // 2: B, 6 m do jogador e a mais de 6 m de todo o resto.
        var candidatos = new List<CandidatoDaFoto>
        {
            Candidato(new Vector3(5f, 0f, 0f), elegivel: false, conhecido: true),
            Candidato(new Vector3(2f, 0f, 0f)),
            Candidato(new Vector3(0f, 0f, -6f))
        };

        Assert.AreEqual(2, EscolhaDaFoto.Escolher(candidatos, Jogador, Raio, -1));
        Assert.AreEqual(1, EscolhaDaFoto.Escolher(candidatos, Jogador, 2.5f, -1));
    }

    [Test]
    public void Escolher_SegundaPassada_RepeteFotografadoMenosOUltimo()
    {
        var candidatos = new List<CandidatoDaFoto>
        {
            Candidato(new Vector3(5f, 0f, 0f), fotografado: true, conhecido: true),
            Candidato(new Vector3(0f, 0f, 2f), fotografado: true, conhecido: true)
        };

        Assert.AreEqual(0, EscolhaDaFoto.Escolher(candidatos, Jogador, Raio, 1));
    }

    [Test]
    public void Escolher_SoOUltimo_DevolveMenosUm()
    {
        var candidatos = new List<CandidatoDaFoto>
        {
            Candidato(new Vector3(3f, 0f, 0f), fotografado: true, conhecido: true)
        };

        Assert.AreEqual(-1, EscolhaDaFoto.Escolher(candidatos, Jogador, Raio, 0));
    }

    [Test]
    public void Escolher_ListaVazia_DevolveMenosUm()
    {
        Assert.AreEqual(-1, EscolhaDaFoto.Escolher(new List<CandidatoDaFoto>(), Jogador, Raio, -1));
    }
}
