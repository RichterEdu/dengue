using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Simula a curva de ocupação hospitalar do FluxoDeJogo da cena aberta, para
/// reajustar o balanceamento sem jogar uma partida inteira a cada mudança.
///
/// Menu: Dengue > Simular Balanceamento
///
/// Usa o mesmo FluxoDeJogo.TaxaPara do jogo, com os valores do Inspector.
/// Para cada ritmo (segundos por foco) o jogador limpa um foco por vez; os
/// últimos demoram até (1 + FatorUltimos)x mais, porque são os mais difíceis
/// de achar. Celular fechado o tempo todo (aberto, a subida só fica mais lenta).
/// Também estima a rota mínima: vizinho mais próximo a partir da saída do
/// hospital, sempre correndo, mais ~0,25 s por clique.
/// Saída só no Console (sem diálogo, para não travar o CLI).
/// </summary>
public static class BalanceamentoSimulador
{
    private static readonly float[] Ritmos = { 15f, 20f, 25f, 30f, 35f, 40f, 45f, 50f };
    private const float FatorUltimos = 0.6f;
    private const float Passo = 0.1f;
    private const float SegundosPorClique = 0.25f;
    private const float TempoMaximo = 3600f;

    [MenuItem("Dengue/Simular Balanceamento")]
    public static void Simular()
    {
        FluxoDeJogo fluxo = Object.FindAnyObjectByType<FluxoDeJogo>();
        if (fluxo == null)
        {
            Debug.LogWarning("[Balanceamento] Nenhum FluxoDeJogo na cena aberta.");
            return;
        }

        Interactable[] focos = Object.FindObjectsByType<Interactable>(FindObjectsInactive.Include);
        int total = focos.Length;
        if (total == 0)
        {
            Debug.LogWarning("[Balanceamento] Nenhum foco (Interactable) na cena aberta.");
            return;
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"[Balanceamento] {total} focos · ocupação inicial {Pct(fluxo.OcupacaoInicial)} · mínima {Pct(fluxo.OcupacaoMinima)} · alívio por foco {Pct1(fluxo.AlivioPorFoco)}");
        sb.AppendLine($"Taxa depois da rampa: {PctS(fluxo.TaxaPara(TempoMaximo, total, false, total))} com todos os focos · a ocupação começa a descer com {FocosParaDescer(fluxo, total)} focos restantes");
        sb.AppendLine($"Ritmo (s/foco; os últimos levam até {1f + FatorUltimos:0.0}x):");
        foreach (float ritmo in Ritmos)
            sb.AppendLine($"  {ritmo,2:0} s → {SimularRitmo(fluxo, total, ritmo)}");
        sb.AppendLine($"  parado → {SimularRitmo(fluxo, total, float.PositiveInfinity)}");
        sb.AppendLine(RotaMinima(focos));

        Debug.Log(sb.ToString());
    }

    private static string SimularRitmo(FluxoDeJogo fluxo, int total, float ritmo)
    {
        float ocupacao = fluxo.OcupacaoInicial;
        float pico = ocupacao;
        float t = 0f;
        float noFoco = 0f;
        int ativos = total;

        while (ativos > 0 && t < TempoMaximo)
        {
            ocupacao = Mathf.Clamp(ocupacao + fluxo.TaxaPara(t, ativos, false, total) * Passo, fluxo.OcupacaoMinima, 1f);
            t += Passo;
            noFoco += Passo;
            pico = Mathf.Max(pico, ocupacao);

            if (ocupacao >= 1f)
                return $"PERDE aos {Tempo(t)} com {total - ativos}/{total} focos";

            float tempoDesteFoco = ritmo * (1f + FatorUltimos * (1f - (float)ativos / total));
            if (noFoco >= tempoDesteFoco)
            {
                noFoco = 0f;
                ativos--;
                ocupacao = Mathf.Max(fluxo.OcupacaoMinima, ocupacao - fluxo.AlivioPorFoco);
            }
        }

        return ativos == 0
            ? $"vence em {Tempo(t)} · pico {Pct(pico)} · final {Pct(ocupacao)}"
            : "não termina em 1 h (a ocupação estabiliza abaixo de 100%)";
    }

    /* Maior número de focos ativos com que a taxa cheia já é <= 0. */
    private static int FocosParaDescer(FluxoDeJogo fluxo, int total)
    {
        for (int ativos = total; ativos >= 0; ativos--)
            if (fluxo.TaxaPara(TempoMaximo, ativos, false, total) <= 0f)
                return ativos;
        return 0;
    }

    private static string RotaMinima(Interactable[] focos)
    {
        ControladorJogador jogador = Object.FindAnyObjectByType<ControladorJogador>();
        float velocidade = jogador != null ? jogador.VelocidadeCorrer : 5.8f;

        Vector3 atual;
        SaidaDoHospital saida = Object.FindAnyObjectByType<SaidaDoHospital>(FindObjectsInactive.Include);
        if (saida != null)
            atual = new SerializedObject(saida).FindProperty("posicaoNoBairro").vector3Value;
        else if (jogador != null)
            atual = jogador.transform.position;
        else
            return "Rota mínima: sem SaidaDoHospital nem jogador na cena.";

        List<Interactable> restantes = new List<Interactable>(focos);
        float distancia = 0f;
        float cliques = 0f;
        while (restantes.Count > 0)
        {
            int melhor = 0;
            float melhorDistancia = float.MaxValue;
            for (int i = 0; i < restantes.Count; i++)
            {
                float d = Vector3.Distance(atual, Centro(restantes[i]));
                if (d < melhorDistancia)
                {
                    melhorDistancia = d;
                    melhor = i;
                }
            }

            distancia += melhorDistancia;
            atual = Centro(restantes[melhor]);
            cliques += restantes[melhor].RequiredClicks * SegundosPorClique;
            restantes.RemoveAt(melhor);
        }

        float tempo = distancia / velocidade + cliques;
        return $"Rota mínima (vizinho mais próximo, correndo a {velocidade:0.0} m/s, em linha reta): {distancia:0} m + cliques ≈ {Tempo(tempo)} = {tempo / focos.Length:0.0} s/foco. É o limite teórico de quem já sabe onde está cada foco.";
    }

    private static Vector3 Centro(Interactable foco)
    {
        Renderer[] renderers = foco.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
            return foco.transform.position;

        Bounds b = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            b.Encapsulate(renderers[i].bounds);
        return b.center;
    }

    private static string Pct(float v) => $"{v * 100f:0}%";
    private static string Pct1(float v) => $"{v * 100f:0.#}%";
    private static string PctS(float v) => $"{v * 100f:+0.00;-0.00}%/s";
    private static string Tempo(float s) => $"{Mathf.FloorToInt(s / 60f)}:{Mathf.FloorToInt(s % 60f):00}";
}
