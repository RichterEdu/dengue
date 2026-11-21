using System.Globalization;
using NUnit.Framework;

/* ============================================================
 *  NotaTestes.cs
 *  ---------------------------------------------------------------------
 *  A conta da nota da partida: pontos, estrelas, metas, limites e os
 *  textos que a tela final e a janela mostram.
 * ============================================================ */

[Category("ParteC")]
public class NotaTestes
{
    private static RegrasDaNota Regras() => new RegrasDaNota();

    private static DadosDaPartida Vitoria(float s, float pico, int reench = 0, int erros = 0) =>
        new DadosDaPartida { Venceu = true, Segundos = s, Pico = pico, Eliminados = 30, Total = 30, Reenchidos = reench, Erros = erros };

    [Test]
    public void ExemploVitoria_6280()
    {
        ResultadoDaNota n = AvaliacaoDaPartida.Calcular(Vitoria(1320, 0.84f), Regras());
        Assert.AreEqual(6280, n.Pontos);
        Assert.AreEqual(2, n.Estrelas);
        Assert.AreEqual(960, n.DeTempo);
        Assert.AreEqual(320, n.DeHospital);
        Assert.IsFalse(n.Cumpriu(MetaDaPartida.RespostaRapida));
    }

    [Test]
    public void ExemploDerrota_1400_SemEstrelas()
    {
        var d = new DadosDaPartida { Segundos = 720, Pico = 1f, Eliminados = 14, Total = 30 };
        ResultadoDaNota n = AvaliacaoDaPartida.Calcular(d, Regras());
        Assert.AreEqual(1400, n.Pontos);
        Assert.AreEqual(0, n.Estrelas);
    }

    [Test]
    public void Reenchidos_Descontam_ComPisoZero()
    {
        var d = new DadosDaPartida { Eliminados = 1, Total = 30, Reenchidos = 5, Pico = 1f };
        Assert.AreEqual(0, AvaliacaoDaPartida.Calcular(d, Regras()).Pontos);
    }

    [TestCase(1200f, true)]
    [TestCase(1200.9f, true)]
    [TestCase(1201f, false)]
    public void Limite_Tempo(float s, bool cumpre) =>
        Assert.AreEqual(cumpre, AvaliacaoDaPartida.MetaCumprida(MetaDaPartida.RespostaRapida, Vitoria(s, 0.8f), Regras()));

    [TestCase(0.854f, true)]
    [TestCase(0.855f, false)]
    public void Limite_Pico(float p, bool cumpre) =>
        Assert.AreEqual(cumpre, AvaliacaoDaPartida.MetaCumprida(MetaDaPartida.HospitalSobControle, Vitoria(1000f, p), Regras()));

    [Test]
    public void SemErros_UmErro_Perde() =>
        Assert.IsFalse(AvaliacaoDaPartida.MetaCumprida(MetaDaPartida.SemErros, Vitoria(1000f, 0.8f, 0, 1), Regras()));

    [Test]
    public void SemErros_ReenchidoNaoDerruba() =>
        Assert.IsTrue(AvaliacaoDaPartida.MetaCumprida(MetaDaPartida.SemErros, Vitoria(1000f, 0.8f, 3), Regras()));

    [Test]
    public void PartidaLonga_TempoLegivelESemBonus()
    {
        Assert.AreEqual("61:05", AvaliacaoDaPartida.Tempo(3665));
        Assert.AreEqual(0, AvaliacaoDaPartida.Calcular(Vitoria(3665f, 0.8f), Regras()).DeTempo);
    }

    [Test]
    public void Formatar_NaoDependeDaCultura()
    {
        CultureInfo antes = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
            Assert.AreEqual("6.280", AvaliacaoDaPartida.Formatar(6280));
        }
        finally { CultureInfo.CurrentCulture = antes; }
        Assert.AreEqual("0", AvaliacaoDaPartida.Formatar(0));
    }

    [Test]
    public void Detalhes_Final()
    {
        var perdeu = Vitoria(1320f, 0.91f, 0, 2);
        Assert.AreEqual("22:00 de 20:00", AvaliacaoDaPartida.DetalheFinal(MetaDaPartida.RespostaRapida, perdeu, Regras()));
        Assert.AreEqual("2 erros", AvaliacaoDaPartida.DetalheFinal(MetaDaPartida.SemErros, perdeu, Regras()));
        Assert.AreEqual("pico 91% de 85%", AvaliacaoDaPartida.DetalheFinal(MetaDaPartida.HospitalSobControle, perdeu, Regras()));
        Assert.AreEqual("pico 84%", AvaliacaoDaPartida.DetalheFinal(MetaDaPartida.HospitalSobControle, Vitoria(900f, 0.84f), Regras()));
        Assert.AreEqual("1 erro", AvaliacaoDaPartida.DetalheFinal(MetaDaPartida.SemErros, Vitoria(900f, 0.8f, 0, 1), Regras()));
        var derrota = new DadosDaPartida { Segundos = 700f, Pico = 1f, Eliminados = 14, Total = 30 };
        Assert.AreEqual("exige vencer", AvaliacaoDaPartida.DetalheFinal(MetaDaPartida.RespostaRapida, derrota, Regras()));
    }

    [Test]
    public void Detalhes_AoVivo()
    {
        var d = new DadosDaPartida { Segundos = 582f, Pico = 0.78f, Erros = 2 };
        Assert.AreEqual("09:42 de 20:00", AvaliacaoDaPartida.DetalheAoVivo(MetaDaPartida.RespostaRapida, d, Regras()));
        Assert.AreEqual("perdida: 2 erros", AvaliacaoDaPartida.DetalheAoVivo(MetaDaPartida.SemErros, d, Regras()));
        Assert.AreEqual("pico 78% de 85%", AvaliacaoDaPartida.DetalheAoVivo(MetaDaPartida.HospitalSobControle, d, Regras()));
    }

    [Test]
    public void Detalhes_MetasCumpridas()
    {
        var ok = Vitoria(1110f, 0.84f);
        Assert.AreEqual("18:30 de 20:00", AvaliacaoDaPartida.DetalheFinal(MetaDaPartida.RespostaRapida, ok, Regras()));
        Assert.AreEqual("0 erros", AvaliacaoDaPartida.DetalheFinal(MetaDaPartida.SemErros, ok, Regras()));
        Assert.AreEqual("18:30 de 20:00", AvaliacaoDaPartida.DetalheAoVivo(MetaDaPartida.RespostaRapida, ok, Regras()));
        Assert.AreEqual("0 erros", AvaliacaoDaPartida.DetalheAoVivo(MetaDaPartida.SemErros, ok, Regras()));
    }

    [Test]
    public void Linhas_Vitoria_SemItemZerado()
    {
        var d = Vitoria(1320f, 0.84f);
        var linhas = AvaliacaoDaPartida.LinhasDePontos(AvaliacaoDaPartida.Calcular(d, Regras()), d, Regras());
        Assert.AreEqual(5, linhas.Count);
        Assert.AreEqual(("Focos  30 × 100", "3.000"), linhas[0]);
        Assert.AreEqual(("Vitória", "1.000"), linhas[1]);
        Assert.AreEqual(("Tempo  22:00", "960"), linhas[2]);
        Assert.AreEqual(("Hospital  pico 84%", "320"), linhas[3]);
        Assert.AreEqual(("Estrelas  2 × 500", "1.000"), linhas[4]);
    }

    [Test]
    public void Linhas_Derrota_ComReenchidoEFaltantes()
    {
        var d = new DadosDaPartida { Segundos = 700f, Pico = 1f, Eliminados = 14, Total = 30, Reenchidos = 2 };
        var linhas = AvaliacaoDaPartida.LinhasDePontos(AvaliacaoDaPartida.Calcular(d, Regras()), d, Regras());
        Assert.AreEqual(("Focos  14 × 100", "1.400"), linhas[0]);
        Assert.AreEqual(("Reenchidos  2 × 100", "−200"), linhas[1]);
        Assert.AreEqual(("Faltaram 16 focos", ""), linhas[2]);
    }

    [Test]
    public void TextoDoRecorde_TresCasos()
    {
        Assert.AreEqual("NOVO RECORDE", AvaliacaoDaPartida.TextoDoRecorde(true, 6280));
        Assert.AreEqual("Recorde: 6.280", AvaliacaoDaPartida.TextoDoRecorde(false, 6280));
        Assert.AreEqual("", AvaliacaoDaPartida.TextoDoRecorde(false, 0));
    }
}
