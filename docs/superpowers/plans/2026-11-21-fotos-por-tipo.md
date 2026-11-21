# Fotos dos focos por tipo Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** A foto do foco sai por tipo: se o jogador passa 90 s sem eliminar nenhum foco de um tipo, o dono do tipo posta a foto de um foco daquele tipo.

**Architecture:** O relógio único de "empacado" do `HistoriaRedeSocial` vira um relógio por `TipoFoco`, com começo escalonado. A escolha continua sendo o `EscolhaDaFoto`, alimentado só com os focos do tipo vencido como elegíveis. Nada fora do narrador muda.

**Tech Stack:** Unity 6000.5.6f1, NUnit EditMode, `unity` CLI.

**Spec:** `docs/superpowers/specs/2026-11-21-fotos-por-tipo-design.md`

## Global Constraints

- Branch `fotos-por-tipo` (de `main` em `c1806e5`; spec em `3ddab10`). Commits terminam com `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`.
- Nunca commitar `Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF - Fallback.asset` nem `Assets/a.unity`/`.meta`; `git add` só com caminhos explícitos.
- Valores: `tempoPorTipo` = 90 s; `intervaloEntreTipos` = 15 s; nova tentativa da foto recusada = 1 s (a constante `EsperaDaFotoRecusada` de hoje). Prioridade `Normal`, humor `Neutro`, curtidas `-1` (como hoje).
- Ordem do escalonamento: os tipos presentes nos focos registrados, em ordem crescente do valor do enum `TipoFoco`; o i-ésimo (a partir de 0) começa com o relógio em `-i × intervaloEntreTipos`.
- A bronca `aoDemorarSemFoco` e o campo `tempoSemFoco` ficam no Inspector, sem uso. Nenhum warning novo de compilação (se surgir CS0414 por campo que ficou sem leitura, envolver só ele com `#pragma warning disable/restore 0414` e um comentário dizendo por quê).
- Convenções do projeto: comentários e nomes em português com acentos; handlers `internal` para os testes; categoria NUnit `Fotos`. Como operar o Unity pelo CLI (cena vazia antes dos testes, `Total > 0`, `recompile`/`recompile_status`, `eval_file` sem `internal`): `.superpowers/sdd/2026-11-17-fotos-dos-focos/contexto-execucao.md`, seção "Como operar o Unity".
- Toda alteração entra no Histórico do `contexto do jogo.md` (Task 2).

## Review Focus

- **Vários tipos vencendo no mesmo quadro** (ex.: o jogador abre o celular por minutos e fecha): saem um por vez, sem perder nenhum, pela janela de conflito + nova tentativa de 1 s. Task 1: `Tipo_DoisVencidosJuntos_SaemUmDepoisDoOutro`.
- **Tipo só com focos mexidos** (o jogador está no meio de um vaso): nada sai daquele tipo e o relógio dele zera, sem bronca. Task 1: `Tipo_SemElegivel_ZeraSemPostar`.
- **Foco de outro tipo mais perto do jogador:** a foto continua sendo do tipo vencido. Task 1: `Tipo_VenceAos90s_PostaFotoDoTipo`.
- **Tipo inteiro eliminado:** o relógio dele para e nunca mais posta. Task 1: `Tipo_TodoEliminado_NaoPostaMais`.
- **Celular aberto:** nenhum relógio anda. Task 1: `Tipo_CelularAberto_Pausa`.

---

### Task 1: Relógio por tipo no narrador

**Files:**
- Modify: `Assets/Scripts/HistoriaRedeSocial.cs` (campos do grupo "Gatilhos", estado das fotos, `Update`, `HandlePartidaIniciada`, `HandleFocoEliminado`, `RegistrarFocos`, `AvancarEmpacado`, `TentarFotoDeFoco`, cabeçalho)
- Test: `Assets/Editor/Testes/HistoriaFotoTestes.cs`

**Interfaces:**
- Produces:
  ```csharp
  [System.Serializable]
  public class TempoDoTipo { public TipoFoco tipo; public float segundos = 90f; }   // no HistoriaRedeSocial.cs, junto das outras classes [Serializable]

  // HistoriaRedeSocial, grupo "Gatilhos":
  [SerializeField] private float tempoPorTipo = 90f;          // tooltip: segundos sem eliminar foco de um tipo até sair a foto dele
  [SerializeField] private float intervaloEntreTipos = 15f;   // tooltip: atraso do começo de cada tipo em relação ao anterior
  [SerializeField] private List<TempoDoTipo> temposPorTipo = new List<TempoDoTipo>();   // tooltip: sobrepõe o tempoPorTipo
  // tempoSemFoco: tooltip passa a dizer que ficou sem uso (as fotos saem por tipo)

  internal void ReiniciarRelogiosDosTipos();   // escalonamento; chamado no fim do RegistrarFocos e no HandlePartidaIniciada
  internal void AvancarRelogiosDosTipos(float dt, Vector3 posicaoDoJogador, bool lendoCelular);   // substitui o AvancarEmpacado
  internal ResultadoDaFoto TentarFotoDoTipo(TipoFoco tipo, Vector3 posicaoDoJogador);              // substitui o TentarFotoDeFoco
  internal float RelogioDoTipo(TipoFoco tipo);            // para os testes
  internal Interactable UltimaFotoDoTipo(TipoFoco tipo);  // null = nenhuma ainda
  // UltimaFoto (a última de qualquer tipo) continua existindo e sendo atualizada.
  ```
- Regras de `AvancarRelogiosDosTipos` (para cada tipo, na ordem do escalonamento):
  - tipo com todos os focos eliminados: pula (relógio parado);
  - sem `lendoCelular`: relógio += dt; abaixo do limite (`temposPorTipo` do tipo, senão `tempoPorTipo`): pula;
  - espera de recusa do tipo > 0: desconta dt e só tenta quando ela zerar;
  - `TentarFotoDoTipo`: `Saiu` → relógio do tipo = 0; `SemFoco` → relógio do tipo = 0, **sem bronca**; `Recusada` → espera do tipo = 1 s.
- `TentarFotoDoTipo`: candidatos como no `Candidato(i)` de hoje, com `elegivel` falso para foco de outro tipo (mas `conhecido` igual); a 2ª passada evita `UltimaFotoDoTipo(tipo)`; resto (dono/UPA, fala gasta só se `Aceitaria`, `foto`/`foco` no post) igual ao `TentarFotoDeFoco` de hoje.
- `HandleFocoEliminado`: zera o relógio **e** a espera do tipo do foco (troca o `timerSemFoco = 0; esperaDaFoto = 0`).
- `Update`: chama `AvancarRelogiosDosTipos` no lugar de `AvancarEmpacado` (mesma condição `player != null`). O `aoDemorarSemFoco` não é mais postado.

- [ ] **Step 1: Trocar os testes do "empacado" pelos por tipo** em `HistoriaFotoTestes.cs` (mesmo `[SetUp]`, `Foco<T>`, `Registrar`; jogador na origem; reflexão de `HandleFocoEliminado` como no teste de hoje). Os testes `Empacado_*`, `Mexido_*`, `TipoSemDono_PostaComoUpa` e `AoMandarFoto_*` passam a chamar `AvancarRelogiosDosTipos` com tempo suficiente; nos `Mexido_*` e `Empacado_FocoDestruido_*`, o "outro" foco escolhido tem de ser do **mesmo tipo** do mexido/destruído. `Empacado_SemFocoComFoto_SaiABronca` sai (substituído pelo `Tipo_SemElegivel_ZeraSemPostar`). Novos:
  - `Tipo_VenceAos90s_PostaFotoDoTipo`: pneu a 20 m e vaso a 5 m (pneu é o 1º tipo). `AvancarRelogiosDosTipos(89.9f, …, false)` → 0 posts; `(0.2f)` → 1 post, `foco` = o pneu, autor "Carlos Mendes".
  - `Tipo_Escalonado_SegundoTipoVence15sDepois`: pneu e vaso. `(90.1f)` → 1 post (pneu); `(14.8f)` → ainda 1; `(0.3f)` → 2 posts, o 2º com `foco` = o vaso.
  - `Tipo_EliminarDoTipo_ZeraSoEle`: 2 pneus e 1 vaso. `(85f)`; eliminar um pneu; `(10f)` → 0 posts e `RelogioDoTipo(Pneu)` ≈ 10, `RelogioDoTipo(Vaso)` ≈ 80; `(10f)` → 1 post, do vaso.
  - `Tipo_FotoQueSaiu_ZeraORelogioDoTipo`: só 2 pneus. `(90.1f)` → 1 post, `RelogioDoTipo(Pneu)` == 0; `(89f)` → ainda 1; `(1.2f)` → 2.
  - `Tipo_CelularAberto_Pausa`: pneu e vaso. `(300f, …, true)` → 0 posts e `RelogioDoTipo(Pneu)` == 0.
  - `Tipo_Recusada_NaoGastaETentaDe1Em1s`: o de hoje (`Empacado_Recusada_NaoGastaETentaDeNovo`) com o pneu: recusada não marca `FoiFotografado`; `(0.5f)` não posta; `(0.6f)` posta com o 1º texto do Carlos.
  - `Tipo_DoisVencidosJuntos_SaemUmDepoisDoOutro`: pneu e vaso; `janelaConflito` da rede em 4. `(200f, …, true)` e depois `(105.1f, …, false)` → 1 post (pneu); "passar" a janela pondo `janelaConflito` em 0; `(1.1f)` → 2 posts, o 2º do vaso.
  - `Tipo_SemElegivel_ZeraSemPostar`: 1 pneu, mexido (`HandleFocoClicado`). `(90.1f)` → 0 posts (nenhum da bronca) e `RelogioDoTipo(Pneu)` == 0.
  - `Tipo_TodoEliminado_NaoPostaMais`: 1 pneu, eliminado (concluído e `HandleFocoEliminado`) + 1 vaso mexido. `(500f)` → 0 posts.
  - `Tipo_TemposPorTipo_Sobrepoe`: pneu e vaso; `temposPorTipo` = [{Vaso, 30}] pelo `SerializedObject`; `ReiniciarRelogiosDosTipos()`; `(45.1f)` → 1 post, do vaso.
- [ ] **Step 2: Rodar e ver falhar** (`unity command run_tests --mode EditMode --filter_type category --filter Fotos`; não compila: `AvancarRelogiosDosTipos` não existe).
- [ ] **Step 3: Implementar** conforme as Interfaces; atualizar o cabeçalho do arquivo (o parágrafo "Fotos dos focos") para descrever o relógio por tipo.
- [ ] **Step 4: Rodar** a categoria `Fotos`, depois `ParteA` e `ParteB`, depois a suíte EditMode inteira (cena vazia antes) → tudo passa, `Total > 0`; Console sem warning novo.
- [ ] **Step 5: Commit** — `Fotos dos focos: relógio por tipo (90 s, escalonado)`.

### Task 2: Verificação em Play e registro

**Files:**
- Modify: `contexto do jogo.md`

- [ ] **Step 1: Play** com a `SampleScene` como cena única (fora do Play antes; cena limpa), abertura pulada e partida iniciada como em `.superpowers/sdd/2026-11-17-fotos-dos-focos/capturas/scripts/preview_celular.cs`; `Application.runInBackground = true` só no teste. Por reflexão: eliminar todos os focos de pneu e depois `AvancarRelogiosDosTipos(105.1f, posição do agente, false)` → saiu a foto de um vaso (`UltimaFotoDoTipo(Vaso)` não nulo) e nenhuma de pneu. Console sem erros novos. Sair do Play.
- [ ] **Step 2: Registrar** no fim do `contexto do jogo.md` uma entrada "2026-11-21 — Fotos dos focos por tipo: implementação e verificação", no formato das anteriores: objetivo, commits (`git log --oneline 3ddab10..HEAD`), arquivos, valores, o que foi verificado e o que não foi.
- [ ] **Step 3: Deixar aberta só a `MenuInicial`** (cena única, sem mudanças) e commitar o `contexto do jogo.md` — `Fotos dos focos por tipo: verificação e registro`.
