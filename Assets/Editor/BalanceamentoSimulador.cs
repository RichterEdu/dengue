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
/// A cada ritmo soma-se o custo do kit (Parte A): quanto a rota com o kit
/// demora a mais, por foco, que a rota mínima sem ele.
/// Rota mínima: vizinho mais próximo a partir da saída do hospital, sempre
/// correndo, mais ~0,25 s por clique (o "clicar N vezes" de antes do kit).
/// Rota com kit: o mesmo vizinho mais próximo, segurando com a ferramenta
/// certa, com o estoque do KitDoAgente, os pontos de reabastecimento e as
/// caçambas da cena (ver RotaComKit).
/// Por fim, a caçamba mais perto de cada pneu.
/// Saída só no Console (sem diálogo, para não travar o CLI).
/// </summary>
public static class BalanceamentoSimulador
{
    private static readonly float[] Ritmos = { 15f, 20f, 25f, 30f, 35f, 40f, 45f, 50f };
    private const float FatorUltimos = 0.6f;
    private const float Passo = 0.1f;
    private const float SegundosPorClique = 0.25f;
    private const float TempoMaximo = 3600f;

    // Sem ControladorJogador na cena, os padrões dele.
    private const float VelocidadeCorrerPadrao = 5.8f;
    private const float VelocidadeAndarPadrao = 2.8f;

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

        // As duas rotas primeiro: o custo do kit entra na tabela de ritmos.
        ControladorJogador jogador = Object.FindAnyObjectByType<ControladorJogador>();
        float custoDoKit = 0f;
        string rotaMinima, rotaComKit;
        if (PontoDePartida(jogador, out Vector3 partida))
        {
            rotaMinima = RotaMinima(focos, partida, jogador, out float tempoMinimo);
            rotaComKit = RotaComKit(focos, partida, jogador, tempoMinimo, out custoDoKit);
        }
        else
        {
            rotaMinima = "Rota mínima: sem SaidaDoHospital nem jogador na cena.";
            rotaComKit = "Rota com kit: sem ponto de partida (custo do kit = 0).";
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"[Balanceamento] {total} focos · ocupação inicial {Pct(fluxo.OcupacaoInicial)} · mínima {Pct(fluxo.OcupacaoMinima)} · alívio por foco {Pct1(fluxo.AlivioPorFoco)}");
        sb.AppendLine($"Taxa depois da rampa: {PctS(fluxo.TaxaPara(TempoMaximo, total, false, total))} com todos os focos · a ocupação começa a descer com {FocosParaDescer(fluxo, total)} focos restantes");
        sb.AppendLine($"Ritmo + custo do kit (s/foco; os últimos levam até {1f + FatorUltimos:0.0}x; custo do kit {custoDoKit:0.0} s/foco):");
        foreach (float ritmo in Ritmos)
            sb.AppendLine($"  {ritmo,2:0} s + {custoDoKit:0.0} s → {SimularRitmo(fluxo, total, ritmo + custoDoKit)}");
        sb.AppendLine($"  parado → {SimularRitmo(fluxo, total, float.PositiveInfinity)}");
        sb.AppendLine(rotaMinima);
        sb.AppendLine(rotaComKit);
        sb.Append(PneusECacambas());

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

    /* De onde as duas rotas saem: a saída do hospital no bairro ou, sem ela,
     * o jogador. False se a cena não tem nenhum dos dois. */
    private static bool PontoDePartida(ControladorJogador jogador, out Vector3 partida)
    {
        SaidaDoHospital saida = Object.FindAnyObjectByType<SaidaDoHospital>(FindObjectsInactive.Include);
        if (saida != null)
            partida = new SerializedObject(saida).FindProperty("posicaoNoBairro").vector3Value;
        else if (jogador != null)
            partida = jogador.transform.position;
        else
        {
            partida = Vector3.zero;
            return false;
        }
        return true;
    }

    private static string RotaMinima(Interactable[] focos, Vector3 partida, ControladorJogador jogador, out float tempo)
    {
        float velocidade = jogador != null ? jogador.VelocidadeCorrer : VelocidadeCorrerPadrao;

        Vector3 atual = partida;
        List<Interactable> restantes = new List<Interactable>(focos);
        float distancia = 0f;
        float cliques = 0f;
        while (restantes.Count > 0)
        {
            Interactable foco = MaisPerto(restantes, atual, out float ate);
            distancia += ate;
            atual = Centro(foco);
            cliques += foco.RequiredClicks * SegundosPorClique;
            restantes.Remove(foco);
        }

        tempo = distancia / velocidade + cliques;
        return $"Rota mínima (vizinho mais próximo, correndo a {velocidade:0.0} m/s, em linha reta): {distancia:0} m + cliques ≈ {Tempo(tempo)} = {tempo / focos.Length:0.0} s/foco. É o limite teórico de quem já sabe onde está cada foco.";
    }

    /* A rota com o kit da Parte A, no mesmo vizinho mais próximo a partir da
     * partida, correndo, com o estoque do KitDoAgente da cena (começa cheio,
     * saco vazio):
     *   - ação de cada foco: SegundosPorPasso × passos (garrafas: um passo
     *     por garrafa; pneu: o tempo de esvaziar, com água);
     *   - consumível zerado: antes de ir ao foco, desvia para o ponto de
     *     reabastecimento mais perto que tenha a ferramenta;
     *   - pneu: depois de pegar, anda até a caçamba mais perto, a
     *     VelocidadeAndar × multiplicadorCarregando (sem correr), e segue de lá;
     *   - saco cheio no meio de um grupo de garrafas: ida e volta à caçamba
     *     mais perto.
     * Cada uso de ponto (reabastecer, entregar o pneu, esvaziar o saco) soma o
     * SegundosParaUsar dele. Devolve em custoPorFoco o custo do kit:
     * (tempo com kit − tempo da rota mínima) / total de focos. */
    private static string RotaComKit(Interactable[] focos, Vector3 partida, ControladorJogador jogador,
                                     float tempoMinimo, out float custoPorFoco)
    {
        custoPorFoco = 0f;
        KitDoAgente kit = Object.FindAnyObjectByType<KitDoAgente>(FindObjectsInactive.Include);
        if (kit == null)
            return "Rota com kit: sem KitDoAgente na cena (custo do kit = 0).";

        Cacamba[] cacambas = Object.FindObjectsByType<Cacamba>(FindObjectsInactive.Include);
        PontoDeReabastecimento[] pontos = Object.FindObjectsByType<PontoDeReabastecimento>(FindObjectsInactive.Include);

        // Campos privados do kit: o estoque só nasce no Play (Awake).
        SerializedObject so = new SerializedObject(kit);
        EstoqueDoKit estoque = new EstoqueDoKit(so.FindProperty("cargaAreia").intValue,
                                                so.FindProperty("cargaTela").intValue,
                                                so.FindProperty("cargaAguaSanitaria").intValue,
                                                so.FindProperty("capacidadeSaco").intValue);
        float correndo = jogador != null ? jogador.VelocidadeCorrer : VelocidadeCorrerPadrao;
        float carregando = (jogador != null ? jogador.VelocidadeAndar : VelocidadeAndarPadrao)
                         * so.FindProperty("multiplicadorCarregando").floatValue;

        Vector3 atual = partida;
        float metrosCorrendo = 0f, metrosComPneu = 0f, segundosSegurando = 0f, segundosNosPontos = 0f;
        int reabastecimentos = 0, idasDoSaco = 0;

        List<Interactable> restantes = new List<Interactable>(focos);
        while (restantes.Count > 0)
        {
            Interactable foco = MaisPerto(restantes, atual, out _);
            restantes.Remove(foco);
            Ferramenta ferramenta = foco.FerramentaNecessaria;

            if (!estoque.TemCarga(ferramenta))
            {
                PontoDeReabastecimento ponto = PontoQueRecarrega(pontos, ferramenta, atual,
                                                                 out List<Ferramenta> recarrega, out float ate);
                if (ponto == null)
                    return $"Rota com kit: nenhum PontoDeReabastecimento recarrega {Ferramentas.Nome(ferramenta)} (custo do kit = 0).";
                metrosCorrendo += ate;
                atual = Centro(ponto);
                segundosNosPontos += ponto.SegundosParaUsar;
                estoque.Reabastecer(recarrega);
                reabastecimentos++;
            }

            Vector3 centro = Centro(foco);
            metrosCorrendo += Vector3.Distance(atual, centro);
            atual = centro;

            if (foco is GarrafaInteractable garrafas)
            {
                int passos = PassosPrevistos(garrafas);
                for (int i = 0; i < passos; i++)
                {
                    if (estoque.SacoCheio)
                    {
                        Cacamba cacamba = MaisPerto(cacambas, atual, out float ida);
                        if (cacamba == null)
                            return "Rota com kit: nenhuma Cacamba na cena para esvaziar o saco (custo do kit = 0).";
                        metrosCorrendo += 2f * ida;
                        segundosNosPontos += cacamba.SegundosParaUsar;
                        estoque.EsvaziarSaco();
                        idasDoSaco++;
                    }
                    estoque.GuardarGarrafa();
                    segundosSegurando += garrafas.SegundosPorPasso;
                }
                continue;
            }

            segundosSegurando += foco.SegundosPorPasso * foco.RequiredClicks;
            estoque.Consumir(ferramenta);

            if (foco is PneuInteractable)
            {
                Cacamba cacamba = MaisPerto(cacambas, atual, out float ate);
                if (cacamba == null)
                    return "Rota com kit: nenhuma Cacamba na cena para entregar o pneu (custo do kit = 0).";
                metrosComPneu += ate;
                atual = Centro(cacamba);
                segundosNosPontos += cacamba.SegundosParaUsar;
            }
        }

        int total = focos.Length;
        float tempo = metrosCorrendo / correndo + metrosComPneu / carregando + segundosSegurando + segundosNosPontos;
        custoPorFoco = (tempo - tempoMinimo) / total;
        return $"Rota com kit (mesmo vizinho mais próximo; com pneu a {carregando:0.00} m/s, sem correr): " +
               $"{metrosCorrendo:0} m correndo + {metrosComPneu:0} m com pneu + {segundosSegurando:0} s segurando + {segundosNosPontos:0} s nos pontos " +
               $"≈ {Tempo(tempo)} = {tempo / total:0.0} s/foco ({reabastecimentos} reabastecimento(s), {idasDoSaco} ida(s) e volta(s) para esvaziar o saco). " +
               $"Custo do kit: ({tempo:0} s − {tempoMinimo:0} s) / {total} = {custoPorFoco:0.0} s/foco.";
    }

    /* Passos previstos de um grupo de garrafas. Em EditMode o OnStart não
     * rodou, então o RequiredClicks salvo pode não bater com as garrafas. */
    private static int PassosPrevistos(GarrafaInteractable garrafas)
    {
        bool umPorGarrafa = new SerializedObject(garrafas).FindProperty("cliquesIgualAoNumeroDeGarrafas").boolValue;
        int contadas = garrafas.ContarGarrafas();
        return umPorGarrafa && contadas > 0 ? contadas : garrafas.RequiredClicks;
    }

    /* O ponto de reabastecimento mais perto de 'de' que recarrega a
     * ferramenta, com a lista do que ele recarrega (null se nenhum serve). */
    private static PontoDeReabastecimento PontoQueRecarrega(PontoDeReabastecimento[] pontos, Ferramenta ferramenta,
                                                            Vector3 de, out List<Ferramenta> recarrega, out float distancia)
    {
        List<PontoDeReabastecimento> servem = new List<PontoDeReabastecimento>();
        foreach (PontoDeReabastecimento ponto in pontos)
            if (Recarrega(ponto).Contains(ferramenta))
                servem.Add(ponto);

        PontoDeReabastecimento maisPerto = MaisPerto(servem, de, out distancia);
        recarrega = maisPerto != null ? Recarrega(maisPerto) : null;
        return maisPerto;
    }

    /* A lista privada 'recarrega' do ponto (por SerializedObject). */
    private static List<Ferramenta> Recarrega(PontoDeReabastecimento ponto)
    {
        SerializedProperty lista = new SerializedObject(ponto).FindProperty("recarrega");
        List<Ferramenta> ferramentas = new List<Ferramenta>(lista.arraySize);
        for (int i = 0; i < lista.arraySize; i++)
            ferramentas.Add((Ferramenta)lista.GetArrayElementAtIndex(i).intValue);
        return ferramentas;
    }

    /* Cada pneu e a caçamba mais perto dele (todas as Cacamba da cena), na
     * mesma medida em linha reta das rotas. */
    private static string PneusECacambas()
    {
        PneuInteractable[] pneus = Object.FindObjectsByType<PneuInteractable>(FindObjectsInactive.Include);
        Cacamba[] cacambas = Object.FindObjectsByType<Cacamba>(FindObjectsInactive.Include);
        if (pneus.Length == 0)
            return "Pneus: nenhum na cena.";
        if (cacambas.Length == 0)
            return "Pneus: nenhuma Cacamba na cena.";

        System.Array.Sort(pneus, (a, b) => string.CompareOrdinal(a.name, b.name));
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"Pneu → caçamba mais perto ({cacambas.Length} caçamba(s) na cena, em linha reta):");
        float maisLonge = 0f;
        foreach (PneuInteractable pneu in pneus)
        {
            Cacamba cacamba = MaisPerto(cacambas, Centro(pneu), out float distancia);
            sb.AppendLine($"  {pneu.name} → {cacamba.name} a {distancia:0.0} m");
            maisLonge = Mathf.Max(maisLonge, distancia);
        }
        sb.Append($"  o pneu mais longe da caçamba mais perto fica a {maisLonge:0.0} m");
        return sb.ToString();
    }

    /* O candidato mais perto de 'de' (pelo Centro); null se não houver. */
    private static T MaisPerto<T>(IReadOnlyList<T> candidatos, Vector3 de, out float distancia) where T : Component
    {
        T melhor = null;
        distancia = float.MaxValue;
        foreach (T candidato in candidatos)
        {
            float d = Vector3.Distance(de, Centro(candidato));
            if (d < distancia)
            {
                distancia = d;
                melhor = candidato;
            }
        }
        return melhor;
    }

    private static Vector3 Centro(Component objeto)
    {
        Renderer[] renderers = objeto.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
            return objeto.transform.position;

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
