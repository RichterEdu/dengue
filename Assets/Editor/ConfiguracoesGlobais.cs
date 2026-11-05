using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

/* ============================================================
 *  ConfiguracoesGlobais.cs  (Editor)
 *  ---------------------------------------------------------------------
 *  A lógica da janela Dengue > Configurações Globais, sem interface: o
 *  que cada tipo de foco edita, a leitura dos valores atuais e a gravação.
 *
 *  A cena é a única fonte destes valores: nenhum menu do Dengue os regrava.
 *
 *  Passos e tempo não são a mesma coisa. Nos focos de segurar, passos só
 *  divide o mesmo tempo total (Interactable.SegundosPorPasso). As exceções
 *  ficam na tabela Tipos:
 *    - Garrafas: um passo por garrafa, sempre. O OnStart troca o
 *      requiredClicks pelo número de garrafas, então passos é só leitura
 *      e o tempo é por garrafa. Não exponha cliquesIgualAoNumeroDeGarrafas:
 *      desligado, o grupo conclui deixando garrafas na cena.
 *    - Ralo: cada passo é um skill check e o erro recomeça do zero, então
 *      aqui mais passos é mais difícil. segundosParaConcluir não vale em
 *      Play: o tempo é o segundosPorVolta.
 *
 *  Aplicar grava primeiro no prefab de origem (instância nova já nasce
 *  certa, e instância sem override segue herdando) e depois em cada foco
 *  (quem tem override, ou nem veio de prefab: pneus e garrafas têm o
 *  componente adicionado na cena).
 * ============================================================ */

public static class ConfiguracoesGlobais
{
    public const string CampoPassosEsvaziar = "passosParaEsvaziar";
    public const string CampoSegundosEsvaziar = "segundosParaEsvaziar";

    public enum ModoDoFoco { Segurar, PorGarrafa, SkillCheck }

    public sealed class TipoDeFoco
    {
        public System.Type Tipo;
        public string Rotulo;
        public ModoDoFoco Modo = ModoDoFoco.Segurar;
        public string CampoPassos = "requiredClicks";   // null = só leitura
        public string CampoTempo = "segundosParaConcluir";
        public float TempoMinimo = 0f;                  // 0 = um passo por aperto
        public bool TemEsvaziar;
        public string RotuloPassos = "Passos";
        public string RotuloTempo = "Tempo total (s)";
        public string Nota;
    }

    public sealed class GrupoDaPartida
    {
        public string Titulo;
        public System.Type Tipo;
        public string[] Campos;
    }

    /* O que gravar. Campo nulo fica como está em cada foco. */
    public sealed class Valores
    {
        public int? Passos;
        public float? Tempo;
        public int? PassosEsvaziar;
        public float? SegundosEsvaziar;
    }

    public struct Faixa
    {
        public float Min;
        public float Max;
        public bool Misto => !Mathf.Approximately(Min, Max);
    }

    public struct Leitura
    {
        public int Quantidade;
        public Faixa Passos;
        public Faixa Tempo;
        public Faixa PassosEsvaziar;
        public Faixa SegundosEsvaziar;
    }

    public static readonly IReadOnlyList<TipoDeFoco> Tipos = new[]
    {
        new TipoDeFoco
        {
            Tipo = typeof(PneuInteractable), Rotulo = "Pneu",
            Nota = "Os passos esvaziam. Pegar é um passo a mais, instantâneo.",
        },
        new TipoDeFoco { Tipo = typeof(VasoInteractable), Rotulo = "Vaso de flor", TemEsvaziar = true },
        new TipoDeFoco { Tipo = typeof(CaixaDaguaInteractable), Rotulo = "Caixa d'água" },
        new TipoDeFoco { Tipo = typeof(PiscinaInteractable), Rotulo = "Piscina" },
        new TipoDeFoco
        {
            Tipo = typeof(GarrafaInteractable), Rotulo = "Garrafas", Modo = ModoDoFoco.PorGarrafa,
            CampoPassos = null, CampoTempo = "segundosPorGarrafa", TemEsvaziar = true,
            RotuloTempo = "Segundos por garrafa",
            Nota = "Um passo por garrafa: para mudar, mude as garrafas do grupo.",
        },
        new TipoDeFoco
        {
            Tipo = typeof(RaloInteractable), Rotulo = "Ralo", Modo = ModoDoFoco.SkillCheck,
            CampoTempo = "segundosPorVolta", TempoMinimo = 0.1f,
            RotuloPassos = "Skill checks", RotuloTempo = "Segundos por volta",
            Nota = "Cada passo é um skill check, e errar recomeça: mais passos, mais difícil.",
        },
        new TipoDeFoco { Tipo = typeof(BromeliaInteractable), Rotulo = "Bromélia" },
    };

    public static readonly IReadOnlyList<GrupoDaPartida> Partida = new[]
    {
        new GrupoDaPartida
        {
            Titulo = "Ocupação", Tipo = typeof(FluxoDeJogo),
            Campos = new[]
            {
                "ocupacaoInicial", "ocupacaoMinima", "taxaComTodosOsFocos", "recuperacaoSemFocos", "alivioPorFoco",
                "fatorInicioCalmo", "duracaoInicioCalmo", "duracaoRampa", "multiplicadorComCelular",
            },
        },
        new GrupoDaPartida
        {
            Titulo = "Ciclo de vida", Tipo = typeof(GerenciadorDeFocos),
            Campos = new[]
            {
                "segundosPorEstagio", "pesoOvo", "pesoLarva", "pesoMosquito",
                "focosComMosquitoNoInicio", "fracaoLarvaNoInicio",
            },
        },
        new GrupoDaPartida
        {
            Titulo = "Chuva", Tipo = typeof(Chuva),
            Campos = new[] { "intervaloEntreInicios", "segundosDePrevisao", "segundosChovendo" },
        },
        new GrupoDaPartida
        {
            Titulo = "Kit do agente", Tipo = typeof(KitDoAgente),
            Campos = new[]
            {
                "cargaAreia", "cargaTela", "cargaAguaSanitaria", "capacidadeSaco", "multiplicadorCarregando",
                "segundosDeCorrida", "segundosParaRecuperar", "fracaoParaVoltarACorrer",
            },
        },
    };

    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    /* ================= Cena ================= */

    /* A cena do jogo é a do FluxoDeJogo, não a ativa: a MenuInicial costuma
     * ficar aberta junto (como no KitDoAgenteBuilder). */
    public static bool AcharCenaDoJogo(out Scene cena)
    {
        FluxoDeJogo fluxo = Object.FindAnyObjectByType<FluxoDeJogo>(FindObjectsInactive.Include);
        cena = fluxo != null ? fluxo.gameObject.scene : default;
        return fluxo != null;
    }

    /* Todos os focos da cena, inclusive inativos: o jogo também conta esses. */
    public static List<Interactable> FocosDaCena(Scene cena)
    {
        List<Interactable> focos = new List<Interactable>();
        foreach (Interactable foco in Object.FindObjectsByType<Interactable>(FindObjectsInactive.Include))
            if (foco.gameObject.scene == cena)
                focos.Add(foco);
        return focos;
    }

    /* O primeiro componente deste tipo na cena (null se não houver). */
    public static Component ComponenteDaCena(Scene cena, System.Type tipo)
    {
        foreach (GameObject raiz in cena.GetRootGameObjects())
        {
            Component achado = raiz.GetComponentInChildren(tipo, true);
            if (achado != null)
                return achado;
        }
        return null;
    }

    /* ================= Leitura ================= */

    public static Leitura Ler(TipoDeFoco tipo, IEnumerable<Interactable> focos)
    {
        List<Interactable> doTipo = DoTipo(tipo, focos);
        Leitura leitura = new Leitura { Quantidade = doTipo.Count };
        if (doTipo.Count == 0)
            return leitura;

        leitura.Passos = FaixaDe(doTipo, f => PassosDe(tipo, f));
        leitura.Tempo = FaixaDe(doTipo, f => new SerializedObject(f).FindProperty(tipo.CampoTempo).floatValue);
        leitura.PassosEsvaziar = FaixaDe(doTipo, f => f.PassosParaEsvaziar);
        leitura.SegundosEsvaziar = FaixaDe(doTipo, f => new SerializedObject(f).FindProperty(CampoSegundosEsvaziar).floatValue);
        return leitura;
    }

    /* A conta que o jogador sente, em uma linha. */
    public static string Resumo(TipoDeFoco tipo, IEnumerable<Interactable> focos)
    {
        List<Interactable> doTipo = DoTipo(tipo, focos);
        if (doTipo.Count == 0)
            return "Sem instâncias na cena.";

        Leitura leitura = Ler(tipo, doTipo);
        switch (tipo.Modo)
        {
            case ModoDoFoco.PorGarrafa:
                string grupos = string.Join(", ", doTipo.Select(f => ((int)PassosDe(tipo, f)).ToString(PtBr)));
                return $"garrafas por grupo: {grupos} · {Segundos(leitura.Tempo, "0.00")} por garrafa";
            case ModoDoFoco.SkillCheck:
                return $"{Contagem(leitura.Passos, "check", "checks")} · {Segundos(leitura.Tempo, "0.##")} por volta";
            default:
                string passos = Contagem(leitura.Passos, "passo", "passos");
                string total = $"{Segundos(leitura.Tempo, "0.##")} no total";
                if (leitura.Passos.Misto || leitura.Tempo.Misto)
                    return $"{passos} · {total}";
                string porPasso = (leitura.Tempo.Min / Mathf.Max(1f, leitura.Passos.Min)).ToString("0.00", PtBr);
                return $"{passos} · {total} · {porPasso} s por passo";
        }
    }

    /* Aviso para este número de passos (null = nada a dizer). Bromélia: as
     * poças secam distribuídas nos passos; com mais passos que poças, alguns
     * passos não mudam nada na tela. */
    public static string Aviso(TipoDeFoco tipo, IEnumerable<Interactable> focos, int passos)
    {
        if (tipo.Tipo != typeof(BromeliaInteractable))
            return null;
        List<Interactable> doTipo = DoTipo(tipo, focos);
        if (doTipo.Count == 0)
            return null;

        int pocas = doTipo.Min(f => new SerializedObject(f).FindProperty("pocas").arraySize);
        return pocas > 0 && passos > pocas
            ? $"Cada bromélia tem {pocas} poças: acima disso, alguns passos não secam nada na tela."
            : null;
    }

    /* ================= Gravação ================= */

    /* Grava os valores em todos os focos deste tipo e nos prefabs de origem,
     * num único Ctrl+Z. Devolve quantos focos mudaram. */
    public static int Aplicar(TipoDeFoco tipo, IEnumerable<Interactable> focos, Valores valores)
    {
        List<Interactable> doTipo = DoTipo(tipo, focos);

        // Conta antes: instância que herda muda junto com o prefab.
        int mudados = doTipo.Count(f => Escrever(new SerializedObject(f), tipo, valores));

        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Configurações Globais: " + tipo.Rotulo);
        int grupoUndo = Undo.GetCurrentGroup();

        HashSet<Interactable> origens = new HashSet<Interactable>();
        foreach (Interactable foco in doTipo)
        {
            Interactable origem = PrefabUtility.GetCorrespondingObjectFromSource(foco);
            if (origem == null || !origens.Add(origem))
                continue;
            SerializedObject so = new SerializedObject(origem);
            if (Escrever(so, tipo, valores))
            {
                so.ApplyModifiedProperties();
                PrefabUtility.SavePrefabAsset(origem.transform.root.gameObject);
            }
        }

        foreach (Interactable foco in doTipo)
        {
            SerializedObject so = new SerializedObject(foco);
            if (Escrever(so, tipo, valores))
                so.ApplyModifiedProperties();
        }

        Undo.CollapseUndoOperations(grupoUndo);
        return mudados;
    }

    /* Põe os valores no SerializedObject, sem aplicar. Devolve se algo mudou. */
    private static bool Escrever(SerializedObject so, TipoDeFoco tipo, Valores valores)
    {
        if (valores.Passos.HasValue && tipo.CampoPassos != null)
            so.FindProperty(tipo.CampoPassos).intValue = Mathf.Max(1, valores.Passos.Value);
        if (valores.Tempo.HasValue)
            so.FindProperty(tipo.CampoTempo).floatValue = Mathf.Max(tipo.TempoMinimo, valores.Tempo.Value);
        if (tipo.TemEsvaziar)
        {
            if (valores.PassosEsvaziar.HasValue)
                so.FindProperty(CampoPassosEsvaziar).intValue = Mathf.Max(1, valores.PassosEsvaziar.Value);
            if (valores.SegundosEsvaziar.HasValue)
                so.FindProperty(CampoSegundosEsvaziar).floatValue = Mathf.Max(0f, valores.SegundosEsvaziar.Value);
        }
        return so.hasModifiedProperties;
    }

    /* ================= Apoio ================= */

    private static List<Interactable> DoTipo(TipoDeFoco tipo, IEnumerable<Interactable> focos)
    {
        return focos.Where(f => f != null && f.GetType() == tipo.Tipo).ToList();
    }

    private static float PassosDe(TipoDeFoco tipo, Interactable foco)
    {
        return foco is GarrafaInteractable garrafas ? garrafas.ContarGarrafas() : foco.RequiredClicks;
    }

    private static Faixa FaixaDe(List<Interactable> focos, System.Func<Interactable, float> valor)
    {
        return new Faixa { Min = focos.Min(valor), Max = focos.Max(valor) };
    }

    private static string Contagem(Faixa faixa, string singular, string plural)
    {
        if (faixa.Misto)
            return $"{faixa.Min.ToString("0", PtBr)} a {faixa.Max.ToString("0", PtBr)} {plural}";
        return $"{faixa.Min.ToString("0", PtBr)} {(Mathf.Approximately(faixa.Min, 1f) ? singular : plural)}";
    }

    private static string Segundos(Faixa faixa, string formato)
    {
        if (faixa.Misto)
            return $"{faixa.Min.ToString(formato, PtBr)} a {faixa.Max.ToString(formato, PtBr)} s";
        return $"{faixa.Min.ToString(formato, PtBr)} s";
    }
}
