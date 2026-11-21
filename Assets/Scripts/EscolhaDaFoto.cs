using System.Collections.Generic;
using UnityEngine;

/* ============================================================
 *  EscolhaDaFoto.cs
 *  ---------------------------------------------------------------------
 *  Qual foco sai na foto quando o jogador empaca. Classe C# pura e
 *  estática (testável sem cena); quem monta os candidatos e posta a
 *  foto é o HistoriaRedeSocial.
 *
 *    1ª passada  elegíveis que nunca saíram em foto e estão a mais de
 *                raioLugarConhecido de todo outro lugar conhecido;
 *    2ª passada  (nada na 1ª) elegíveis, mesmo os já fotografados,
 *                menos o da última foto;
 *    nada        −1 (o narrador posta a bronca antiga).
 *
 *  Nas duas passadas vence o mais perto do jogador (distância em linha
 *  reta); no empate, o de menor índice.
 * ============================================================ */

public struct CandidatoDaFoto
{
    public Vector3 posicao;   // posição do foco guardada no início da partida
    public bool elegivel;     // não concluído, ativo, com foto, não mexido
    public bool fotografado;  // já saiu em foto
    public bool conhecido;    // lugar conhecido: fotografado, mexido ou concluído (vale mesmo sem ser elegível)
}

public static class EscolhaDaFoto
{
    // Índice escolhido ou -1.
    public static int Escolher(IReadOnlyList<CandidatoDaFoto> candidatos, Vector3 jogador,
                               float raioLugarConhecido, int ultimaFoto)
    {
        int escolhido = MaisPerto(candidatos, jogador,
                                  i => !candidatos[i].fotografado &&
                                       LongeDosConhecidos(candidatos, i, raioLugarConhecido));
        if (escolhido >= 0) return escolhido;

        return MaisPerto(candidatos, jogador, i => i != ultimaFoto);
    }

    // O elegível mais perto do jogador que passa no filtro (empate: menor índice), ou -1.
    private static int MaisPerto(IReadOnlyList<CandidatoDaFoto> candidatos, Vector3 jogador,
                                 System.Func<int, bool> passa)
    {
        int melhor = -1;
        float menorDistancia = float.MaxValue;

        for (int i = 0; i < candidatos.Count; i++)
        {
            if (!candidatos[i].elegivel || !passa(i)) continue;

            float distancia = Vector3.Distance(candidatos[i].posicao, jogador);
            if (distancia < menorDistancia)
            {
                menorDistancia = distancia;
                melhor = i;
            }
        }

        return melhor;
    }

    // Se o candidato está a mais de raio de todo outro candidato conhecido.
    private static bool LongeDosConhecidos(IReadOnlyList<CandidatoDaFoto> candidatos, int indice, float raio)
    {
        for (int j = 0; j < candidatos.Count; j++)
        {
            if (j == indice || !candidatos[j].conhecido) continue;
            if (Vector3.Distance(candidatos[indice].posicao, candidatos[j].posicao) <= raio) return false;
        }

        return true;
    }
}
