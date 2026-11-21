# Parte C — Nota, estrelas e recorde: plano de implementação

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** dar uma razão para jogar a segunda partida: cada partida termina com pontos, até 3 estrelas com nome e um recorde guardado, e a tela final mostra qual meta faltou.

**Architecture:** a conta é pura (`AvaliacaoDaPartida`) e o armazenamento é isolado (`RecordeDoJogo`). O `PlacarDaPartida` observa `FluxoDeJogo` e `GerenciadorDeFocos`, e a interface só lê o placar. A cena é montada pelo menu `Dengue > Configurar Nota e Recorde`.

**Tech Stack:** Unity 6000.5.6f1, URP, TextMeshPro, Unity Test Framework (NUnit), `unity` CLI conectado ao Editor aberto.

**Spec:** `docs/superpowers/specs/2026-11-05-parte-c-nota-e-recorde-design.md`

## Global Constraints

- **Mesmas regras das Partes A e B:** estilo do projeto (pt-BR, cabeçalho `/* ==== ... ==== */`, `[Tooltip]` em pt-BR), textos sem emoji e objetos de teste com `TesteUtil` (`Com<T>`, `Setar`, `LimparTudo` no `[TearDown]`).
- Toda classe de teste nova leva `[Category("ParteC")]`, em `Assets/Editor/Testes/`.
- **Ciclo:** conferir que o Editor não está em Play (`unity command editor_status`, `playMode` = `stopped`), `unity command recompile`, `unity command recompile_status`, `unity command console_status`, e `unity command run_tests --mode EditMode --filter_type category --filter ParteC`. O `run_tests` pode dar timeout de 30 s no CLI; o resultado fica em `C:/Users/eduar/AppData/LocalLow/Eduardo Richter/Foco Zero/TestResults.xml`. "PASS" = `failed="0"`. Se o Editor estiver em Play, parar e avisar o usuário: ele costuma jogar com o Editor aberto.
- Em EditMode, `Start` e `Update` não rodam. Lógica fica em método `internal` chamável no teste, e o `Update` só chama esse método. `InternalsVisibleTo` já existe em `Assets/Scripts/AssemblyInfo.cs`.
- **Valores padrão de `RegrasDaNota`:** `limiteDeSegundos` 1200, `limiteDePico` 85, `pontosPorFoco` 100, `pontosPorVitoria` 1000, `tetoDeSegundos` 1800, `pontosPorSegundo` 2, `pontosPorPercentual` 20, `pontosPorEstrela` 500, `penalidadePorReenchido` 100.
- Comparações com os valores que a tela mostra: segundos por `FloorToInt`, pico por `RoundToInt(pico × 100)`.
- Os 30 focos não mudam. Nada de sorteio de lugares nem de pontos novos.
- **Nada de commit** sem pedido do usuário. Os checkpoints só conferem `git status`.
- Toda mudança é registrada no fim de `contexto do jogo.md`. No fim, só a cena `MenuInicial` aberta.
- **Se o prazo apertar**, cortar nesta ordem e avisar o usuário: grupo "Nota" na janela (Task 6), `RecordeNoMenu` (Tasks 4 e 5), `CartaoDeMetas` (Tasks 4 e 5). Nota, estrelas, tela final e aviso ficam até o fim.

## Review Focus

1. **Sair sem terminar.** R no meio do epílogo já gravou, mas reiniciar ou sair pela pausa com a partida rolando não pode gravar nada. Teste: `PlacarTestes.Destruir_SemFechar_NaoGrava` (Task 3).
2. **`PlayerPrefs` estranho.** Pontos negativos e estrelas fora de 0 a 3 (arquivo editado à mão, versão antiga) viram valores válidos. Teste: `RecordeTestes.ValoresInvalidos_FicamNaFaixa` (Task 2).
3. **Pausa antes da porta do hospital.** O cartão abre com a partida parada e mostra `00:00 de 20:00`, `0 reenchidos`, `pico 70% de 85%`, sem nenhuma meta perdida. Teste: `PlacarTestes.AntesDaPorta_MetasVivas` (Task 3).
4. **Partida muito longa.** Passando de 60 minutos, o tempo segue legível (`61:05`), e o bônus de tempo fica em zero. Teste: `NotaTestes.PartidaLonga_TempoLegivelESemBonus` (Task 1).
5. **Cena incompleta.** Placar sem fluxo ou sem gerenciador não quebra o fim da partida. Teste: `PlacarTestes.Fechar_SemFluxo_NaoQuebra` (Task 3).

---

### Task 1: A conta da nota

**Files:**
- Create: `Assets/Scripts/AvaliacaoDaPartida.cs`
- Test: `Assets/Editor/Testes/NotaTestes.cs`

**Interfaces:**
- Produces (tudo em `AvaliacaoDaPartida.cs`):
  - `public enum MetaDaPartida { RespostaRapida, SemRetrabalho, HospitalSobControle }`
  - `[Serializable] public class RegrasDaNota` com os campos públicos `int` de Global Constraints, cada um com `[Tooltip]`
  - `public struct DadosDaPartida { bool Venceu; float Segundos; float Pico; int Eliminados; int Total; int Reenchidos; }`
  - `public struct ResultadoDaNota { int Pontos; int Estrelas; int DeFocos; int DeVitoria; int DeTempo; int DeHospital; int DeEstrelas; int Penalidade; bool Cumpriu(MetaDaPartida meta); }`
  - `public static class AvaliacaoDaPartida`:
    - `MetaDaPartida[] Metas`: a ordem fixa da spec
    - `bool DentroDoLimite(meta, DadosDaPartida d, RegrasDaNota r)`: ignora `Venceu`
    - `bool MetaCumprida(meta, d, r)`: `d.Venceu && DentroDoLimite`
    - `ResultadoDaNota Calcular(d, r)`
    - `string Formatar(int pontos)`, `string Tempo(int segundos)`, `string Nome(MetaDaPartida meta)`
    - `string DetalheFinal(meta, d, r)`, `string DetalheAoVivo(meta, d, r)`
    - `List<(string rotulo, string valor)> LinhasDePontos(ResultadoDaNota n, DadosDaPartida d, RegrasDaNota r)`
    - `string TextoDoRecorde(bool recordeNovo, int guardado)`

- [ ] **Step 1: Criar a branch**

Run: `git switch -c parte-c/nota-e-recorde main`
Expected: a branch muda sem mexer em arquivos do jogo, porque `main` e a branch atual têm o mesmo conteúdo. `contexto do jogo.md` e a spec continuam como alterações locais.

- [ ] **Step 2: Escrever os testes**

Helper: `Vitoria(float s, float pico, int reench = 0)` com 30 eliminados de 30; `Regras()` = `new RegrasDaNota()`.

```csharp
[Test] public void ExemploVitoria_6280()
{
    ResultadoDaNota n = AvaliacaoDaPartida.Calcular(Vitoria(1320, 0.84f), Regras());
    Assert.AreEqual(6280, n.Pontos);
    Assert.AreEqual(2, n.Estrelas);
    Assert.AreEqual(960, n.DeTempo);
    Assert.AreEqual(320, n.DeHospital);
    Assert.IsFalse(n.Cumpriu(MetaDaPartida.RespostaRapida));
}
[Test] public void ExemploDerrota_1400_SemEstrelas()
{
    var d = new DadosDaPartida { Segundos = 720, Pico = 1f, Eliminados = 14, Total = 30 };
    ResultadoDaNota n = AvaliacaoDaPartida.Calcular(d, Regras());
    Assert.AreEqual(1400, n.Pontos);
    Assert.AreEqual(0, n.Estrelas);
}
[Test] public void Reenchidos_Descontam_ComPisoZero()
{
    var d = new DadosDaPartida { Eliminados = 1, Total = 30, Reenchidos = 5, Pico = 1f };
    Assert.AreEqual(0, AvaliacaoDaPartida.Calcular(d, Regras()).Pontos);
}
[TestCase(1200f, true)] [TestCase(1200.9f, true)] [TestCase(1201f, false)]
public void Limite_Tempo(float s, bool cumpre) =>
    Assert.AreEqual(cumpre, AvaliacaoDaPartida.MetaCumprida(MetaDaPartida.RespostaRapida, Vitoria(s, 0.8f), Regras()));
[TestCase(0.854f, true)] [TestCase(0.855f, false)]
public void Limite_Pico(float p, bool cumpre) =>
    Assert.AreEqual(cumpre, AvaliacaoDaPartida.MetaCumprida(MetaDaPartida.HospitalSobControle, Vitoria(1000f, p), Regras()));
[Test] public void SemRetrabalho_UmReenchido_Perde() =>
    Assert.IsFalse(AvaliacaoDaPartida.MetaCumprida(MetaDaPartida.SemRetrabalho, Vitoria(1000f, 0.8f, 1), Regras()));
[Test] public void PartidaLonga_TempoLegivelESemBonus()
{
    Assert.AreEqual("61:05", AvaliacaoDaPartida.Tempo(3665));
    Assert.AreEqual(0, AvaliacaoDaPartida.Calcular(Vitoria(3665f, 0.8f), Regras()).DeTempo);
}
[Test] public void Formatar_NaoDependeDaCultura()
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
[Test] public void Detalhes_Final()
{
    var perdeu = Vitoria(1320f, 0.91f, 2);
    Assert.AreEqual("22:00 de 20:00", AvaliacaoDaPartida.DetalheFinal(MetaDaPartida.RespostaRapida, perdeu, Regras()));
    Assert.AreEqual("2 reenchidos", AvaliacaoDaPartida.DetalheFinal(MetaDaPartida.SemRetrabalho, perdeu, Regras()));
    Assert.AreEqual("pico 91% de 85%", AvaliacaoDaPartida.DetalheFinal(MetaDaPartida.HospitalSobControle, perdeu, Regras()));
    Assert.AreEqual("pico 84%", AvaliacaoDaPartida.DetalheFinal(MetaDaPartida.HospitalSobControle, Vitoria(900f, 0.84f), Regras()));
    Assert.AreEqual("1 reenchido", AvaliacaoDaPartida.DetalheFinal(MetaDaPartida.SemRetrabalho, Vitoria(900f, 0.8f, 1), Regras()));
    var derrota = new DadosDaPartida { Segundos = 700f, Pico = 1f, Eliminados = 14, Total = 30 };
    Assert.AreEqual("exige vencer", AvaliacaoDaPartida.DetalheFinal(MetaDaPartida.RespostaRapida, derrota, Regras()));
}
[Test] public void Detalhes_AoVivo()
{
    var d = new DadosDaPartida { Segundos = 582f, Pico = 0.78f, Reenchidos = 2 };
    Assert.AreEqual("09:42 de 20:00", AvaliacaoDaPartida.DetalheAoVivo(MetaDaPartida.RespostaRapida, d, Regras()));
    Assert.AreEqual("perdida: 2 reenchidos", AvaliacaoDaPartida.DetalheAoVivo(MetaDaPartida.SemRetrabalho, d, Regras()));
    Assert.AreEqual("pico 78% de 85%", AvaliacaoDaPartida.DetalheAoVivo(MetaDaPartida.HospitalSobControle, d, Regras()));
}
[Test] public void Linhas_Vitoria_SemItemZerado()
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
[Test] public void Linhas_Derrota_ComReenchidoEFaltantes()
{
    var d = new DadosDaPartida { Segundos = 700f, Pico = 1f, Eliminados = 14, Total = 30, Reenchidos = 2 };
    var linhas = AvaliacaoDaPartida.LinhasDePontos(AvaliacaoDaPartida.Calcular(d, Regras()), d, Regras());
    Assert.AreEqual(("Focos  14 × 100", "1.400"), linhas[0]);
    Assert.AreEqual(("Reenchidos  2 × 100", "−200"), linhas[1]);
    Assert.AreEqual(("Faltaram 16 focos", ""), linhas[2]);
}
[Test] public void TextoDoRecorde_TresCasos()
{
    Assert.AreEqual("NOVO RECORDE", AvaliacaoDaPartida.TextoDoRecorde(true, 6280));
    Assert.AreEqual("Recorde: 6.280", AvaliacaoDaPartida.TextoDoRecorde(false, 6280));
    Assert.AreEqual("", AvaliacaoDaPartida.TextoDoRecorde(false, 0));
}
```

- [ ] **Step 3: Rodar e confirmar que falha**

Run: o ciclo de Global Constraints.
Expected: erro de compilação, `AvaliacaoDaPartida` não existe.

- [ ] **Step 4: Implementar `AvaliacaoDaPartida.cs`**

`Formatar`: `pontos.ToString("N0", CultureInfo.InvariantCulture).Replace(',', '.')`. `Tempo`: `$"{s / 60:00}:{s % 60:00}"`. `DeTempo` = `pontosPorSegundo × Max(0, teto − FloorToInt(Segundos))`. `DeHospital` = `pontosPorPercentual × Max(0, 100 − pico%)`. Vitória, tempo, hospital e estrelas só com `Venceu`. O negativo em `LinhasDePontos` usa o sinal `−` (U+2212), e 1 reenchido fica no singular. Os nomes das metas são os da spec: "Resposta rápida", "Sem retrabalho", "Hospital sob controle".

- [ ] **Step 5: Rodar e confirmar que passa**

Run: o ciclo. Expected: `ParteC` com `failed="0"`.

- [ ] **Step 6: Checkpoint**

Run: `git status --short`. Expected: só os arquivos desta task, a spec, o plano e `contexto do jogo.md`.

---

### Task 2: O recorde guardado

**Files:**
- Create: `Assets/Scripts/RecordeDoJogo.cs`
- Test: `Assets/Editor/Testes/RecordeTestes.cs`

**Interfaces:**
- Produces: `public sealed class RecordeDoJogo` com `const string PrefixoPadrao = "recorde."`, construtor `RecordeDoJogo(string prefixo = PrefixoPadrao)`, `int Pontos`, `int Estrelas`, `bool Registrar(int pontos, int estrelas)` e `internal void Apagar()`. Lê do `PlayerPrefs` a cada acesso, sem cache estático.

- [ ] **Step 1: Escrever os testes**

Usar `new RecordeDoJogo("teste.recorde.")`, com `Apagar()` no `[SetUp]` e no `[TearDown]`.

```csharp
[Test] public void Registrar_SoSobeComNotaMaior()
{
    Assert.IsTrue(recorde.Registrar(1000, 1));
    Assert.IsFalse(recorde.Registrar(1000, 1));
    Assert.IsFalse(recorde.Registrar(900, 0));
    Assert.AreEqual(1000, recorde.Pontos);
}
[Test] public void Estrelas_IndependemDosPontos()
{
    recorde.Registrar(6000, 1);
    recorde.Registrar(5000, 3);
    Assert.AreEqual(6000, recorde.Pontos);
    Assert.AreEqual(3, recorde.Estrelas);
}
[Test] public void NotaZero_NaoERecorde() => Assert.IsFalse(recorde.Registrar(0, 0));
[Test] public void ValoresInvalidos_FicamNaFaixa()
{
    PlayerPrefs.SetInt("teste.recorde.pontos", -50);
    PlayerPrefs.SetInt("teste.recorde.estrelas", 9);
    Assert.AreEqual(0, recorde.Pontos);
    Assert.AreEqual(3, recorde.Estrelas);
}
[Test] public void PrefixoDeTeste_NaoTocaNasChavesReais()
{
    int antes = new RecordeDoJogo().Pontos;
    recorde.Registrar(antes + 1000, 3);
    Assert.AreEqual(antes, new RecordeDoJogo().Pontos);
}
```

- [ ] **Step 2: Rodar e confirmar que falha**

Run: o ciclo. Expected: erro de compilação, `RecordeDoJogo` não existe.

- [ ] **Step 3: Implementar `RecordeDoJogo.cs`**

Chaves `prefixo + "pontos"` e `prefixo + "estrelas"`. Leitura com `Max(0, ...)` e `Clamp(..., 0, 3)`. `Registrar` chama `PlayerPrefs.Save` quando grava algo.

- [ ] **Step 4: Rodar e confirmar que passa**

Run: o ciclo. Expected: `ParteC` com `failed="0"`.

---

### Task 3: O placar da partida

**Files:**
- Modify: `Assets/Scripts/FluxoDeJogo.cs`
- Create: `Assets/Scripts/PlacarDaPartida.cs`
- Test: `Assets/Editor/Testes/PlacarTestes.cs`

**Interfaces:**
- Consumes: Tasks 1 e 2; `GerenciadorDeFocos.OnFocoReenchido`; `FluxoDeJogo.OnVitoria`, `OnDerrota`, `DefinirEstadoParaTeste`, `Ligar`.
- Produces:
  - `FluxoDeJogo`: `float OcupacaoPico` (começa em `OcupacaoInicial` no `Ligar`) e `internal void AvancarOcupacao(float dt)`, com o corpo do `Update` depois da guarda: tempo, taxa, ocupação, pico e derrota.
  - `PlacarDaPartida : MonoBehaviour`, com `[SerializeField] fluxo`, `gerenciador`, `regras`:
    - `event Action<MetaDaPartida> OnMetaPerdida`
    - `int Reenchidos`, `RegrasDaNota Regras`, `DadosDaPartida Dados`, `bool Fechado`, `ResultadoDaNota Resultado`, `bool RecordeNovo`, `RecordeDoJogo Recorde`
    - `bool MetaViva(MetaDaPartida meta)`
    - `ResultadoDaNota Fechar(bool venceu)`
    - `internal void Ligar()`, `internal void Conferir()`, `internal void UsarRecorde(RecordeDoJogo recorde)`

- [ ] **Step 1: Escrever os testes**

`[SetUp]`: `FluxoDeJogo` e `GerenciadorDeFocos` pelo `TesteUtil`; `Setar(gerenciador, "fluxo", fluxo)`, `Setar(fluxo, "gerenciador", gerenciador)`; `fluxo.Ligar()`; placar com `Setar` de `fluxo` e `gerenciador`, `UsarRecorde(new RecordeDoJogo("teste.placar."))`, `placar.Ligar()`; `fluxo.DefinirEstadoParaTeste(true, false)`. `NovoVasoEsvaziado()` copiado de `ChuvaTestes.cs` (usa um `KitDoAgente` em `ControladorJogador`).

```csharp
[Test] public void Pico_SobeENaoDesce()
{
    TesteUtil.Setar(fluxo, "ocupacaoInicial", 0.9f);
    fluxo.Ligar();
    Assert.AreEqual(0.9f, fluxo.OcupacaoPico, 1e-4f);
    fluxo.AvancarOcupacao(100f);          // sem focos, a ocupação desce
    Assert.Less(fluxo.Ocupacao, 0.9f);
    Assert.AreEqual(0.9f, fluxo.OcupacaoPico, 1e-4f);
}
[Test] public void Tempo_CaiUmaVez()
{
    int avisos = 0; placar.OnMetaPerdida += m => avisos++;
    fluxo.AvancarOcupacao(1201f);
    placar.Conferir(); placar.Conferir();
    Assert.AreEqual(1, avisos);
    Assert.IsFalse(placar.MetaViva(MetaDaPartida.RespostaRapida));
}
[Test] public void Hospital_CaiAoPassarDoLimite()
{
    TesteUtil.Setar(fluxo, "ocupacaoInicial", 0.9f);
    fluxo.Ligar();
    placar.Conferir();
    Assert.IsFalse(placar.MetaViva(MetaDaPartida.HospitalSobControle));
}
[Test] public void Esvaziar_NaoDerruba_ReencherDerruba()
{
    VasoInteractable vaso = NovoVasoEsvaziado();
    placar.Conferir();
    Assert.IsTrue(placar.MetaViva(MetaDaPartida.SemRetrabalho));
    vaso.Reencher();
    Assert.AreEqual(1, placar.Reenchidos);
    Assert.IsFalse(placar.MetaViva(MetaDaPartida.SemRetrabalho));
}
[Test] public void Fechar_DuasVezes_GravaUmaEMantemOPrimeiro()
{
    ResultadoDaNota a = placar.Fechar(true);
    ResultadoDaNota b = placar.Fechar(false);
    Assert.AreEqual(a.Pontos, b.Pontos);
    Assert.IsTrue(placar.Dados.Venceu);
}
[Test] public void Fechar_NotaMaior_MarcaRecordeNovo()
{
    placar.Fechar(true);
    Assert.IsTrue(placar.RecordeNovo);
    Assert.Greater(placar.Recorde.Pontos, 0);
}
[Test] public void ForaDaPartida_NaoConta()
{
    fluxo.DefinirEstadoParaTeste(false, false);
    fluxo.AvancarOcupacao(1201f);
    placar.Conferir();
    Assert.IsTrue(placar.MetaViva(MetaDaPartida.RespostaRapida));
    fluxo.DefinirEstadoParaTeste(true, true);
    placar.Conferir();
    Assert.IsTrue(placar.MetaViva(MetaDaPartida.RespostaRapida));
}
[Test] public void AntesDaPorta_MetasVivas()
{
    fluxo.DefinirEstadoParaTeste(false, false);
    placar.Conferir();
    foreach (MetaDaPartida meta in AvaliacaoDaPartida.Metas)
        Assert.IsTrue(placar.MetaViva(meta));
    StringAssert.StartsWith("00:00 de", AvaliacaoDaPartida.DetalheAoVivo(MetaDaPartida.RespostaRapida, placar.Dados, placar.Regras));
    Assert.AreEqual("pico 70% de 85%", AvaliacaoDaPartida.DetalheAoVivo(MetaDaPartida.HospitalSobControle, placar.Dados, placar.Regras));
}
[Test] public void Destruir_SemFechar_NaoGrava()
{
    int antes = placar.Recorde.Pontos;
    fluxo.AvancarOcupacao(10f);
    UnityEngine.Object.DestroyImmediate(placar);
    Assert.AreEqual(antes, new RecordeDoJogo("teste.placar.").Pontos);
}
[Test] public void Fechar_SemFluxo_NaoQuebra()
{
    PlacarDaPartida solto = TesteUtil.Com<PlacarDaPartida>("Solto (teste)");
    solto.UsarRecorde(new RecordeDoJogo("teste.placar."));
    Assert.DoesNotThrow(() => { solto.Ligar(); solto.Conferir(); solto.Fechar(false); });
}
```

Apagar `"teste.placar."` no `[SetUp]` e no `[TearDown]`, antes de `TesteUtil.LimparTudo()`.

- [ ] **Step 2: Rodar e confirmar que falha**

Run: o ciclo. Expected: erro de compilação, `OcupacaoPico` e `PlacarDaPartida` não existem.

- [ ] **Step 3: Alterar `FluxoDeJogo.cs`**

Mover o corpo do `Update`, a partir de `TempoDecorrido += ...`, para `AvancarOcupacao(float dt)`, trocando `Time.deltaTime` por `dt`. Atualizar o pico logo depois do `Clamp`. O `Update` mantém a guarda e chama `AvancarOcupacao(Time.deltaTime)`. Sem gerenciador, o método continua saindo depois de somar o tempo.

- [ ] **Step 4: Implementar `PlacarDaPartida.cs`**

`MetaViva` lê um conjunto de metas já perdidas, preenchido só pelo `Conferir` e pelo `OnFocoReenchido`; não é recalculado dos números. `Dados`, depois de `Fechar`, devolve o retrato fechado; antes, o retrato ao vivo, com `Venceu = false`. Sem fluxo, os números são zero e `Pico` é 0. `Ligar()`: resolve o `gerenciador` por `GerenciadorDeFocos.Obter()` e o `fluxo` por `FindAnyObjectByType`, só se o campo estiver vazio, e assina sem duplicar (`-=` antes de `+=`). `Start` chama `Ligar`, `Update` chama `Conferir`. `Conferir` só age com a partida iniciada e não encerrada, e o `OnFocoReenchido` também. `Fechar` não exige `Conferir` anterior. `OnDestroy` solta as assinaturas.

- [ ] **Step 5: Rodar e confirmar que passa**

Run: o ciclo, mais `--filter ParteB`. Expected: `failed="0"` nas duas. O `Update` da ocupação mudou de lugar, então `FluxoPressaoTestes` é a regressão dele.

---

### Task 4: Telas e avisos

**Files:**
- Create: `Assets/Scripts/TelaDaNota.cs`, `Assets/Scripts/AvisoDeMeta.cs`, `Assets/Scripts/CartaoDeMetas.cs`, `Assets/Scripts/RecordeNoMenu.cs`
- Modify: `Assets/Scripts/FluxoDeJogoHUD.cs`, `Assets/Scripts/MenuPausa.cs`
- Test: `Assets/Editor/Testes/InterfaceDaNotaTestes.cs`

**Interfaces:**
- Consumes: Tasks 1 a 3.
- Produces (campos `[SerializeField]`, ligados na Task 5):
  - `TelaDaNota`: `Image[] estrelas` (3), `Sprite estrelaCheia, estrelaVazia`, `Color corCheia, corVazia, corPerdida, corTextoFraco, corRecordeNovo`, `TextMeshProUGUI textoNota, textoRecorde`, `Image[] estrelasDasMetas` (3), `TextMeshProUGUI[] nomesDasMetas, detalhesDasMetas` (3), `TextMeshProUGUI[] rotulosDePontos, valoresDePontos` (6).
    - `public void Mostrar(ResultadoDaNota n, DadosDaPartida d, RegrasDaNota r, bool recordeNovo, int guardado)`
    - `public static void PintarEstrelas(Image[] estrelas, int quantas, Sprite cheia, Sprite vazia, Color corCheia, Color corVazia)`: preenche da esquerda para a direita e ignora itens nulos.
  - `AvisoDeMeta`: `PlacarDaPartida placar`, `CanvasGroup grupo`, `TextMeshProUGUI textoMeta`, `float segundosVisivel = 3f`, `float velocidadeFade = 4f`.
    - `internal void Ligar()`, `internal void Mostrar(MetaDaPartida meta)`, `internal void Avancar(float dt)`, `internal float Restante`
  - `CartaoDeMetas`: `PlacarDaPartida placar`, os mesmos arrays de metas da tela, `Sprite estrelaCheia, estrelaVazia`, `Color corCheia, corPerdida, corTextoFraco, corRecordeVazia`, `GameObject linhaDoRecorde`, `TextMeshProUGUI textoRecorde`, `Image[] estrelasDoRecorde` (3).
    - `public void Atualizar()`
  - `RecordeNoMenu`: `GameObject linha`, `TextMeshProUGUI textoPontos`, `Image[] estrelas`, sprites e cores.
    - `internal void Atualizar(RecordeDoJogo recorde)`; `Start` chama com `new RecordeDoJogo()`.
  - `FluxoDeJogoHUD`: campos `PlacarDaPartida placar`, `TelaDaNota telaVitoria, telaDerrota` e `internal void PreencherFim(bool venceu)`, chamado por `HandleVitoria` e `HandleDerrota`.
  - `MenuPausa`: campo `CartaoDeMetas cartaoDeMetas`.

- [ ] **Step 1: Escrever os testes**

```csharp
[Test] public void PintarEstrelas_EsquerdaParaDireita()
{
    Image[] i = { Imagem(), Imagem(), Imagem() };
    TelaDaNota.PintarEstrelas(i, 2, cheia, vazia, Color.green, Color.gray);
    Assert.AreSame(cheia, i[0].sprite);
    Assert.AreSame(cheia, i[1].sprite);
    Assert.AreSame(vazia, i[2].sprite);
}
[Test] public void HUD_SemPlacar_MantemEstatisticas()
{
    // FluxoDeJogoHUD com fluxo e vitoriaTexto (TMP) por Setar, sem placar
    hud.PreencherFim(true);
    StringAssert.Contains("Focos eliminados", vitoriaTexto.text);
}
[Test] public void HUD_ComPlacar_UsaATela_ENaoOTexto()
{
    // mesmo HUD, com placar e telaVitoria por Setar; a tela tem textoNota
    hud.PreencherFim(true);
    Assert.AreEqual("", vitoriaTexto.text ?? "");
    Assert.AreEqual(AvaliacaoDaPartida.Formatar(placar.Resultado.Pontos), textoNota.text);
}
[Test] public void Aviso_MostraETroca_SemFila()
{
    aviso.Mostrar(MetaDaPartida.SemRetrabalho);
    Assert.AreEqual("Sem retrabalho", textoMeta.text);
    Assert.AreEqual(3f, aviso.Restante, 1e-4f);
    aviso.Avancar(2f);
    aviso.Mostrar(MetaDaPartida.RespostaRapida);
    Assert.AreEqual("Resposta rápida", textoMeta.text);
    Assert.AreEqual(3f, aviso.Restante, 1e-4f);
}
[Test] public void Aviso_SomeDepoisDoTempo()
{
    aviso.Mostrar(MetaDaPartida.SemRetrabalho);
    aviso.Avancar(3.1f);
    Assert.AreEqual(0f, aviso.Restante, 1e-4f);
}
[Test] public void Aviso_NaoApareceDepoisDoFim()
{
    placar.Fechar(true);
    aviso.Mostrar(MetaDaPartida.SemRetrabalho);
    Assert.AreEqual(0f, aviso.Restante, 1e-4f);
}
[Test] public void Cartao_MostraMetaEPerdida()
{
    // placar com 1 reenchido
    cartao.Atualizar();
    Assert.AreEqual("perdida: 1 reenchido", detalhes[1].text);
    Assert.AreSame(vazia, estrelasMetas[1].sprite);
    Assert.AreSame(cheia, estrelasMetas[0].sprite);
}
[Test] public void RecordeNoMenu_SemRecorde_Esconde()
{
    menu.Atualizar(new RecordeDoJogo("teste.menu."));
    Assert.IsFalse(linha.activeSelf);
}
[Test] public void RecordeNoMenu_ComRecorde_MostraPontosEEstrelas()
{
    RecordeDoJogo r = new RecordeDoJogo("teste.menu.");
    r.Registrar(6280, 2);
    menu.Atualizar(r);
    Assert.IsTrue(linha.activeSelf);
    Assert.AreEqual("6.280", textoPontos.text);
}
```

Reaproveitar o preparo do `PlacarTestes` (fluxo, gerenciador, placar com recorde `"teste.interface."`). Imagens e textos nascem com `TesteUtil.Com<Image>` e `TesteUtil.Com<TextMeshProUGUI>`, e os sprites por `Sprite.Create` sobre uma `Texture2D` 2×2. Arrays serializados são preenchidos por um helper local com `SerializedObject`, porque `TesteUtil.Setar` não aceita arrays. Apagar as chaves de teste no `[SetUp]` e no `[TearDown]`.

- [ ] **Step 2: Rodar e confirmar que falha**

Run: o ciclo. Expected: erro de compilação.

- [ ] **Step 3: Implementar os quatro scripts**

Nada de lógica de nota neles: textos vêm de `AvaliacaoDaPartida`. `TelaDaNota.Mostrar`:
- estrelas por `PintarEstrelas(estrelas, n.Estrelas, ...)`;
- nota por `Formatar`;
- `textoRecorde` por `TextoDoRecorde`, em `corRecordeNovo` se for recorde novo;
- metas na ordem de `Metas`, com estrela cheia só se `n.Cumpriu(meta)`, e detalhe em `corPerdida` se venceu e não cumpriu; senão, `corTextoFraco`;
- linhas de `LinhasDePontos`, limpando as sobras.

`AvisoDeMeta.Update` chama `Avancar(Time.deltaTime)`, que anda `Restante` e faz o fade do `grupo` para 1 (ou 0 quando `Restante` chega a zero). Com `placar.Fechado`, `Mostrar` não faz nada e `Avancar` zera `Restante`. `CartaoDeMetas` pinta a estrela da meta pelo `placar.MetaViva`, e, com partida encerrada, usa o que `placar` tem. A linha do recorde só fica ativa com `Recorde.Pontos > 0`.

- [ ] **Step 4: Ligar em `FluxoDeJogoHUD` e `MenuPausa`**

`PreencherFim(venceu)`: com `placar` e a tela correspondente ligados, `tela.Mostrar(placar.Fechar(venceu), placar.Dados, placar.Regras, placar.RecordeNovo, placar.Recorde.Pontos)`. Sem eles, o `MontarEstatisticas` de hoje no `vitoriaTexto` ou `derrotaTexto`. `Start` preenche `placar` por `FindAnyObjectByType`, se vazio. `MenuPausa.AtualizarStatus` chama `cartaoDeMetas.Atualizar()`, se ligado, antes da guarda de `fluxo == null`.

- [ ] **Step 5: Rodar e confirmar que passa**

Run: o ciclo. Expected: `ParteC` com `failed="0"`.

---

### Task 5: Montar a cena

**Files:**
- Create: `Assets/Editor/NotaBuilder.cs`
- Create (gerados pelo menu): `Assets/Meus/Assets Gerados/Interface/Estrela Cheia.png`, `Estrela Vazia.png`
- Modify (pelo menu): `Assets/Scenes/SampleScene.unity`, `Assets/Scenes/MenuInicial.unity`
- Modify: `Assets/Editor/Testes/ConfiguracoesGlobaisTestes.cs`
- Test: `Assets/Editor/Testes/NotaBuilderTestes.cs`

**Interfaces:**
- Consumes: Task 4; `MenuPausaBuilder.PrepararRecursos`, `Cartao`, `Novo`, `Pos`, `Txt`, `CorTexto`, `CorTextoFraco` (`internal`).
- Produces: `[MenuItem("Dengue/Configurar Nota e Recorde")] public static void Configurar()` e `internal static Texture2D GerarEstrela(bool cheia, int tamanho)`.

- [ ] **Step 1: Escrever os testes**

```csharp
[Test] public void GerarEstrela_CheiaTemCentro_VaziaNao()
{
    Assert.Greater(NotaBuilder.GerarEstrela(true, 96).GetPixel(48, 48).a, 0.9f);
    Assert.Less(NotaBuilder.GerarEstrela(false, 96).GetPixel(48, 48).a, 0.1f);
}
[Test] public void GerarEstrela_CantoTransparente() =>
    Assert.Less(NotaBuilder.GerarEstrela(true, 96).GetPixel(0, 0).a, 0.01f);
```

Em `ConfiguracoesGlobaisTestes`, acrescentar `[TestCase("Assets/Editor/NotaBuilder.cs")]` em `Builders_NaoGravamOsCamposDaJanela`.

- [ ] **Step 2: Rodar e confirmar que falha**

Run: o ciclo. Expected: erro de compilação, `NotaBuilder` não existe.

- [ ] **Step 3: Implementar `NotaBuilder.cs`**

`Configurar()`:
1. Em Play, ou com alguma cena suja, avisa no Console e sai, sem diálogo.
2. Gera os sprites (`GerarEstrela` em 96 px, importados como Sprite Single, sem mipmaps, com `alphaIsTransparency`).
3. `EditorSceneManager.OpenScene` da `SampleScene`, modo Single. Monta tudo, salva.
4. `OpenScene` da `MenuInicial`, modo Single. Monta a linha, salva. O Editor termina só com ela.

`GerarEstrela`: polígono de 10 pontas (raio externo 0,98, interno 0,45) com teste ponto no polígono e supersampling 3×3. A vazia tira a mesma estrela em escala 0,68, deslocada 0,02 para baixo.

Idempotente: cada peça é procurada pelo nome antes de criar. O cabeçalho do arquivo avisa que rodar `Criar Menu de Pausa` ou `Criar Menu Inicial` exige rodar este menu de novo.

Na `SampleScene`:
- `PlacarDaPartida` no objeto do `FluxoDeJogo`.
- `FocoListaHUD.comecarAberto = false`; `FocoProximityGlow.raioProximidade = 3.5f`.
- Em `VitoriaCaixa` e `DerrotaCaixa`, um `TelaDaNota` em cada caixa, com as referências ligadas por `SerializedObject`:

| Peça | Âncora / pivô | Posição | Tamanho | Texto |
|---|---|---|---|---|
| Caixa | centro | — | 920×630 | — |
| `*Titulo` | topo | 0, −30 | como está | — |
| `IconeCirculo`, `Divisor`, `*Texto` | — | `SetActive(false)` | — | — |
| 3 estrelas | topo / centro | x −92, 0, 92; y −132 | 76×76 | — |
| `textoNota` | topo / centro | 0, −222 | 400×80 | 68 negrito |
| `textoRecorde` | topo / centro | 0, −272 | 400×30 | 20 |
| Linha | topo / centro | 0, −306 | 840×2 | branco 8% |
| "METAS" | topo-esq | 40, −326 | 300×24 | 16 negrito, fraco |
| "PONTOS" | topo-esq | 530, −326 | 300×24 | 16 negrito, fraco |
| Metas, linha i | topo-esq | y = −364 − 62i; estrela x 40 (30×30, y − 6), nome x 84 (22), detalhe x 84, y − 28 (17) | 380×28 | — |
| Pontos, linha i (6) | topo-esq / topo-dir | y = −364 − 36i; rótulo x 530 (260×28); valor direita em 880 (120×28, negrito) | — | 21 |
| Rodapé | base / base | 0, 20 | 500×28 | 17, fraco, "Aperte R para jogar de novo" |

Cores da tela: `corCheia` (0,25; 1; 0,8), `corVazia` branco 28%, `corPerdida` (1; 0,4; 0,4), `corTextoFraco` branco 55%, `corRecordeNovo` (1; 0,82; 0,3), texto (0,95; 0,96; 0,97).

- Aviso em `FluxoDeJogoCanvas`, objeto `AvisoDeMeta` com `CanvasGroup` (alpha 0) e fundo (0,07; 0,09; 0,11; 0,92): âncora e pivô topo-centro, posição 0, −126, 420×46. Estrela vazia 24×24 em x 18; "META PERDIDA" 15 negrito vermelho em x 54; `textoMeta` 21 em x 194.
- Cartão da pausa: filho da `telaMenu`, criado por `MenuPausaBuilder.Cartao("CartaoMetas", ...)`, 400×420, âncora centro, pivô topo-esquerdo, posição 60, 356. O `Cartao` do menu vai para −220, 0. "METAS DA PARTIDA" 15 negrito em 32, −44. Metas em y = −92 − 76i: estrela 30×30 em x 32, nome 22 em x 76, detalhe 17 em x 76, y − 28. Linha em 32, −326 (336×2). Recorde em y −344: "RECORDE" 15 em x 32, pontos 26 negrito em x 128, 3 estrelas 24×24 a partir de x 226, passo 28. Cores: `corCheia` (0,12; 0,62; 0,5), `corPerdida` (0,8; 0,25; 0,25), `corRecordeVazia` preto 30%, textos `MenuPausaBuilder.CorTexto` e `CorTextoFraco`. Acrescentar o cartão ao array `cartoes` do `MenuPausa` sem duplicar, e ligar `cartaoDeMetas`.
- Ligar `placar`, `telaVitoria` e `telaDerrota` no `FluxoDeJogoHUD`.

Na `MenuInicial`, em `TelaTitulo`: objeto `LinhaRecorde`, âncora e pivô base-esquerda como a `Assinatura`, posição (x da `Assinatura`, 96), 420×52. "RECORDE" 15 fraco em x 0, pontos 26 negrito em x 96, estrelas 24×24 a partir de x 188, passo 28. Componente `RecordeNoMenu` em `TelaTitulo`, para o `Start` rodar mesmo com a linha inativa.

- [ ] **Step 4: Rodar e confirmar que passa**

Run: o ciclo, mais `--filter Config`. Expected: `failed="0"` nas duas.

- [ ] **Step 5: Rodar o menu**

Run: `unity command menu --path "Dengue/Configurar Nota e Recorde"`, depois `unity command list_open_scenes` e `unity command console_status`.
Expected: só `MenuInicial` aberta, sem suja, sem erro novo.

- [ ] **Step 6: Rodar o menu de novo**

Run: o mesmo menu, depois `git status --short`.
Expected: nenhuma peça duplicada, conferido com `unity command eval_file` contando `CartaoMetas`, `LinhaRecorde` e `PlacarDaPartida`, um de cada.

---

### Task 6: Simulador, janela e calibragem

**Files:**
- Modify: `Assets/Editor/BalanceamentoSimulador.cs`, `Assets/Editor/ConfiguracoesGlobais.cs`
- Modify (cena, só se a calibragem pedir): `Assets/Scenes/SampleScene.unity`
- Test: `Assets/Editor/Testes/BalanceamentoSimuladorTestes.cs`

**Interfaces:**
- Consumes: Task 1; `PlacarDaPartida.Regras`.
- Produces: `internal static string Resumir(IReadOnlyList<Partida> partidas, int total, RegrasDaNota regras = null)` e `internal static string NotaDaLinha(bool venceu, float segundos, float pico, int eliminados, int total, RegrasDaNota regras)`. Em `ConfiguracoesGlobais.Partida`, grupo `Titulo = "Nota"`, `Tipo = typeof(PlacarDaPartida)`, com os 9 campos `regras.*`.

- [ ] **Step 1: Escrever os testes**

Em `BalanceamentoSimuladorTestes` (o helper `Vitoria(semente, pico)` já existe; manter `[Category]` da classe e acrescentar a categoria `ParteC` nestes testes):

```csharp
[Test, Category("ParteC")] public void NotaDaLinha_Vitoria()
{
    string s = BalanceamentoSimulador.NotaDaLinha(true, 1320f, 0.84f, 30, 30, new RegrasDaNota());
    StringAssert.Contains("nota 6.280", s);
    StringAssert.Contains("2 estrelas", s);
}
[Test, Category("ParteC")] public void Resumir_SemRegras_NaoMostraNota() =>
    StringAssert.DoesNotContain("nota", BalanceamentoSimulador.Resumir(new[] { Vitoria(1, 0.8f) }, Total));
[Test, Category("ParteC")] public void Resumir_ComRegras_MostraNota() =>
    StringAssert.Contains("nota", BalanceamentoSimulador.Resumir(new[] { Vitoria(1, 0.8f) }, Total, new RegrasDaNota()));
```

- [ ] **Step 2: Rodar e confirmar que falha**

Run: o ciclo. Expected: erro de compilação.

- [ ] **Step 3: Implementar**

`Simular()` procura `PlacarDaPartida` na cena e passa `Regras` por `SimularRitmo` até `Resumir`; sem placar, passa `null`. `LinhaDeVitoria` acrescenta `NotaDaLinha` com a média de duração e pico, antes da semente que muda o resultado; `LinhaDeDerrota` faz o mesmo. Formato: ` · nota 6.280 · 2 estrelas`. Reenchidos são sempre 0. Acrescentar o grupo "Nota" ao array `Partida`.

- [ ] **Step 4: Rodar e confirmar que passa**

Run: o ciclo, mais `--filter ParteB` e `--filter Config`. Expected: `failed="0"` nas três. `Partida_TodoCampoExisteNoComponente` cobre os caminhos `regras.*`.

- [ ] **Step 5: Calibrar**

Run: `unity command open_scene --path Assets/Scenes/SampleScene.unity`, `unity command menu --path "Dengue/Simular Balanceamento"` e `unity command console`.
Expected: a linha de 30 s ganha a estrela do hospital e não a do tempo. Se o pico passar de `limiteDePico` em 30 s, subir o limite para o percentual inteiro acima do pico máximo da linha. Se 30 s cumprir o tempo, baixar `limiteDeSegundos` para 2 minutos abaixo da duração da linha. Mudança de valor é feita por `SerializedObject` no placar da cena, com o `SaveScene`, e registrada no `contexto do jogo.md`. Depois, `unity command open_scene --path Assets/Scenes/MenuInicial.unity`.

---

### Task 7: Verificar no jogo e na build

**Files:**
- Modify: `contexto do jogo.md`

- [ ] **Step 1: Suíte inteira**

Run: `unity command run_tests --mode EditMode`.
Expected: `failed="0"` em todas as categorias.

- [ ] **Step 2: Vitória e derrota em Play**

`PlayerSettings.runInBackground = true` só durante o teste. Com `unity command editor_play`, entrar pela `MenuInicial`, forçar `timeScale` 1 e chamar `IniciarPartida`. Por `eval_file` (via reflexão, porque `eval_file` não enxerga `internal`), concluir todos os focos para a vitória. Em outra rodada, levar a ocupação a 100% para a derrota.

Expected:
- depois do epílogo (Esc pula), a tela final mostra nota, estrelas e metas;
- `new RecordeDoJogo().Pontos` igual à nota;
- R recomeça, e o recorde continua;
- ao voltar ao menu, a linha aparece;
- ao sair do Play e entrar de novo, o recorde continua;
- Console sem erros.

Capturas em 1920×1080: trocar os canvases para `ScreenSpaceCamera`, como no preview, e comparar com as imagens aprovadas.

- [ ] **Step 3: Metas em Play**

Passar 20:00 com `AvancarOcupacao` por reflexão, chamar `ReencherEsvaziados` com um foco esvaziado e abrir a pausa.
Expected: aviso abaixo da barra, cartão com as metas perdidas em vermelho, e lista fechada ao entrar (Tab abre).

- [ ] **Step 4: Limpar o recorde de teste**

Run por `eval_file`: `new RecordeDoJogo().Apagar()` por reflexão, e `PlayerSettings.runInBackground = false`.
Expected: `Pontos` = 0, para o usuário começar do zero.

- [ ] **Step 5: Build**

Run: com o Editor fora de Play, `eval_file` com `BuildPipeline.BuildPlayer` para `Builds/Foco Zero/Foco Zero.exe`, `BuildTarget.StandaloneWindows64`, cenas de `EditorBuildSettings`. Confirmar antes que `Builds/` está no `.gitignore`.
Expected: `BuildResult.Succeeded`. Abrir o executável e, numa partida curta, conferir recorde gravado, névoa da chuva e destaque da mira (pendências da Parte B). Se o executável não puder ser conferido por automação, dizer isso ao usuário e pedir que ele confira. O `PlayerPrefs` da build é separado do Editor: um recorde criado nessa conferência fica nela, e o usuário deve ser avisado.

- [ ] **Step 6: Registrar**

Acrescentar ao fim de `contexto do jogo.md`: objetivo, arquivos, valores finais de `RegrasDaNota`, resultado de testes, Play e build, e o que não foi verificado.

- [ ] **Step 7: Conferir o estado final**

Run: `unity command list_open_scenes` e `git status --short`.
Expected: só `MenuInicial` aberta e nenhum commit feito.
