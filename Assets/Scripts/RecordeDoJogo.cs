using UnityEngine;

/* ============================================================
 *  RecordeDoJogo.cs
 *  ---------------------------------------------------------------------
 *  A melhor nota do jogador, guardada em PlayerPrefs (vale depois de
 *  fechar o jogo). Pontos e estrelas são guardados em separado: o recorde
 *  de estrelas pode vir de uma partida diferente do de pontos.
 *
 *  Lê do PlayerPrefs a cada acesso (sem cache estático). O prefixo das
 *  chaves é parâmetro para os testes não tocarem nas chaves reais.
 * ============================================================ */

public sealed class RecordeDoJogo
{
    public const string PrefixoPadrao = "recorde.";
    private const int MaxEstrelas = 3;

    private readonly string chavePontos;
    private readonly string chaveEstrelas;

    public RecordeDoJogo(string prefixo = PrefixoPadrao)
    {
        chavePontos = prefixo + "pontos";
        chaveEstrelas = prefixo + "estrelas";
    }

    public int Pontos => Mathf.Max(0, PlayerPrefs.GetInt(chavePontos, 0));

    public int Estrelas => Mathf.Clamp(PlayerPrefs.GetInt(chaveEstrelas, 0), 0, MaxEstrelas);

    /* Grava o que superar o guardado. Devolve true se os PONTOS bateram o recorde. */
    public bool Registrar(int pontos, int estrelas)
    {
        bool gravou = false;
        bool novoRecorde = pontos > Pontos;

        if (novoRecorde)
        {
            PlayerPrefs.SetInt(chavePontos, pontos);
            gravou = true;
        }

        estrelas = Mathf.Clamp(estrelas, 0, MaxEstrelas);
        if (estrelas > Estrelas)
        {
            PlayerPrefs.SetInt(chaveEstrelas, estrelas);
            gravou = true;
        }

        if (gravou)
            PlayerPrefs.Save();
        return novoRecorde;
    }

    internal void Apagar()
    {
        PlayerPrefs.DeleteKey(chavePontos);
        PlayerPrefs.DeleteKey(chaveEstrelas);
    }
}
