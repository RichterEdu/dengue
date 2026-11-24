using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

/* ============================================================
 *  AvaliacaoDaPartida.cs
 *  ---------------------------------------------------------------------
 *  A conta da nota. Tudo aqui é C# puro e estático (testável sem cena):
 *  recebe os DadosDaPartida e as RegrasDaNota e devolve pontos, estrelas
 *  e os textos que a tela final e a janela mostram.
 *
 *    - Focos, vitória, tempo, hospital e estrelas somam; reenchidos
 *      descontam (nota nunca abaixo de zero).
 *    - Sem erros: nenhum minigame errado na partida (a tela rasgada do
 *      bueiro conta como erro). O reenchido só desconta pontos.
 *    - Vitória, tempo, hospital e estrelas só contam com Venceu.
 *    - Comparações com o que a tela mostra: segundos por FloorToInt,
 *      pico por RoundToInt(pico x 100).
 * ============================================================ */

public enum MetaDaPartida
{
    RespostaRapida,
    SemErros,
    HospitalSobControle
}

[Serializable]
public class RegrasDaNota
{
    [Tooltip("Meta Resposta rápida: tempo máximo, em segundos, para vencer.")]
    public int limiteDeSegundos = 1200;

    [Tooltip("Meta Hospital sob controle: pico máximo de ocupação, em porcentagem.")]
    public int limiteDePico = 85;

    [Tooltip("Pontos por foco eliminado.")]
    public int pontosPorFoco = 100;

    [Tooltip("Pontos por vencer a partida.")]
    public int pontosPorVitoria = 1000;

    [Tooltip("Tempo (segundos) a partir do qual o bônus de tempo é zero.")]
    public int tetoDeSegundos = 1800;

    [Tooltip("Pontos por segundo que sobrou até o teto de tempo.")]
    public int pontosPorSegundo = 2;

    [Tooltip("Pontos por ponto percentual de ocupação abaixo de 100%, no pico.")]
    public int pontosPorPercentual = 20;

    [Tooltip("Pontos por estrela (meta cumprida).")]
    public int pontosPorEstrela = 500;

    [Tooltip("Pontos descontados por criadouro reenchido.")]
    public int penalidadePorReenchido = 100;
}

public struct DadosDaPartida
{
    public bool Venceu;
    public float Segundos;
    public float Pico;
    public int Eliminados;
    public int Total;
    public int Reenchidos;
    public int Erros;
}

public struct ResultadoDaNota
{
    public int Pontos;
    public int Estrelas;
    public int DeFocos;
    public int DeVitoria;
    public int DeTempo;
    public int DeHospital;
    public int DeEstrelas;
    public int Penalidade;

    internal bool CumpriuRapida;
    internal bool CumpriuSemErros;
    internal bool CumpriuHospital;

    public bool Cumpriu(MetaDaPartida meta)
    {
        switch (meta)
        {
            case MetaDaPartida.RespostaRapida: return CumpriuRapida;
            case MetaDaPartida.SemErros: return CumpriuSemErros;
            default: return CumpriuHospital;
        }
    }
}

public static class AvaliacaoDaPartida
{
    private const string Menos = "−";

    public static readonly MetaDaPartida[] Metas =
    {
        MetaDaPartida.RespostaRapida,
        MetaDaPartida.SemErros,
        MetaDaPartida.HospitalSobControle
    };

    private static int Segundos(DadosDaPartida d) => Mathf.FloorToInt(d.Segundos);
    private static int PicoPercentual(DadosDaPartida d) => Mathf.RoundToInt(d.Pico * 100f);

    /* O limite da meta, sem olhar se venceu. */
    public static bool DentroDoLimite(MetaDaPartida meta, DadosDaPartida d, RegrasDaNota r)
    {
        switch (meta)
        {
            case MetaDaPartida.RespostaRapida: return Segundos(d) <= r.limiteDeSegundos;
            case MetaDaPartida.SemErros: return d.Erros == 0;
            default: return PicoPercentual(d) <= r.limiteDePico;
        }
    }

    public static bool MetaCumprida(MetaDaPartida meta, DadosDaPartida d, RegrasDaNota r) =>
        d.Venceu && DentroDoLimite(meta, d, r);

    public static ResultadoDaNota Calcular(DadosDaPartida d, RegrasDaNota r)
    {
        ResultadoDaNota n = new ResultadoDaNota();
        n.CumpriuRapida = MetaCumprida(MetaDaPartida.RespostaRapida, d, r);
        n.CumpriuSemErros = MetaCumprida(MetaDaPartida.SemErros, d, r);
        n.CumpriuHospital = MetaCumprida(MetaDaPartida.HospitalSobControle, d, r);

        foreach (MetaDaPartida meta in Metas)
            if (n.Cumpriu(meta))
                n.Estrelas++;

        n.DeFocos = d.Eliminados * r.pontosPorFoco;
        n.Penalidade = d.Reenchidos * r.penalidadePorReenchido;

        if (d.Venceu)
        {
            n.DeVitoria = r.pontosPorVitoria;
            n.DeTempo = r.pontosPorSegundo * Mathf.Max(0, r.tetoDeSegundos - Segundos(d));
            n.DeHospital = r.pontosPorPercentual * Mathf.Max(0, 100 - PicoPercentual(d));
            n.DeEstrelas = n.Estrelas * r.pontosPorEstrela;
        }

        n.Pontos = Mathf.Max(0, n.DeFocos + n.DeVitoria + n.DeTempo + n.DeHospital + n.DeEstrelas - n.Penalidade);
        return n;
    }

    /* 6280 vira "6.280", em qualquer cultura. */
    public static string Formatar(int pontos) =>
        pontos.ToString("N0", CultureInfo.InvariantCulture).Replace(',', '.');

    public static string Tempo(int segundos) => $"{segundos / 60:00}:{segundos % 60:00}";

    public static string Nome(MetaDaPartida meta)
    {
        switch (meta)
        {
            case MetaDaPartida.RespostaRapida: return "Resposta rápida";
            case MetaDaPartida.SemErros: return "Sem erros";
            default: return "Hospital sob controle";
        }
    }

    private static string Erros(int n) => n == 1 ? "1 erro" : $"{n} erros";

    /* Detalhe de cada meta na tela final. */
    public static string DetalheFinal(MetaDaPartida meta, DadosDaPartida d, RegrasDaNota r)
    {
        if (!d.Venceu)
            return "exige vencer";

        bool dentro = DentroDoLimite(meta, d, r);
        switch (meta)
        {
            case MetaDaPartida.RespostaRapida:
                return $"{Tempo(Segundos(d))} de {Tempo(r.limiteDeSegundos)}";
            case MetaDaPartida.SemErros:
                return Erros(d.Erros);
            default:
                return dentro ? $"pico {PicoPercentual(d)}%" : $"pico {PicoPercentual(d)}% de {r.limiteDePico}%";
        }
    }

    /* Detalhe de cada meta na janela, durante a partida. */
    public static string DetalheAoVivo(MetaDaPartida meta, DadosDaPartida d, RegrasDaNota r)
    {
        switch (meta)
        {
            case MetaDaPartida.RespostaRapida:
                return $"{Tempo(Segundos(d))} de {Tempo(r.limiteDeSegundos)}";
            case MetaDaPartida.SemErros:
                return d.Erros == 0 ? Erros(0) : $"perdida: {Erros(d.Erros)}";
            default:
                return $"pico {PicoPercentual(d)}% de {r.limiteDePico}%";
        }
    }

    /* As linhas da conta, sem os itens zerados. */
    public static List<(string rotulo, string valor)> LinhasDePontos(ResultadoDaNota n, DadosDaPartida d, RegrasDaNota r)
    {
        var linhas = new List<(string rotulo, string valor)>();
        linhas.Add(($"Focos  {d.Eliminados} × {r.pontosPorFoco}", Formatar(n.DeFocos)));

        if (n.DeVitoria > 0)
            linhas.Add(("Vitória", Formatar(n.DeVitoria)));
        if (n.DeTempo > 0)
            linhas.Add(($"Tempo  {Tempo(Segundos(d))}", Formatar(n.DeTempo)));
        if (n.DeHospital > 0)
            linhas.Add(($"Hospital  pico {PicoPercentual(d)}%", Formatar(n.DeHospital)));
        if (n.DeEstrelas > 0)
            linhas.Add(($"Estrelas  {n.Estrelas} × {r.pontosPorEstrela}", Formatar(n.DeEstrelas)));
        if (d.Reenchidos > 0)
        {
            string rotulo = d.Reenchidos == 1 ? "Reenchido" : "Reenchidos";
            linhas.Add(($"{rotulo}  {d.Reenchidos} × {r.penalidadePorReenchido}", Menos + Formatar(n.Penalidade)));
        }

        int faltam = d.Total - d.Eliminados;
        if (!d.Venceu && faltam > 0)
            linhas.Add((faltam == 1 ? "Faltou 1 foco" : $"Faltaram {faltam} focos", ""));

        return linhas;
    }

    public static string TextoDoRecorde(bool recordeNovo, int guardado)
    {
        if (recordeNovo)
            return "NOVO RECORDE";
        return guardado > 0 ? $"Recorde: {Formatar(guardado)}" : "";
    }
}
