# Parte B — Focos vivos, chuva e entregas com peso: plano de implementação

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** os focos evoluem (ovo → larva → mosquito) e pesam na barra conforme o estágio. Esvaziar com as Mãos vira um atalho que a chuva cobra. Os pneus vão para a caçamba da picape da UPA, nos braços do agente. As garrafas vão para as lixeiras de rua.

**Architecture:**
- **Gerenciador central:** um `GerenciadorDeFocos` vira a única varredura de `Interactable`. Ele repassa os eventos dos focos e guarda um `CicloDeVida` (classe C# pura) por foco.
- **Chuva:** a `Chuva` é uma máquina de estados com `Avancar(dt)` testável; ela chama `ReencherEsvaziados()`.
- **Atalho:** fica na base `Interactable` (estado `Esvaziado` e o desvio no `Verificar`/`AvancarCom`).
- **Visual e cena:** o visual (enxame, chuva, carga) só escuta eventos. A cena é montada pelo builder `Dengue > Configurar Focos Vivos e Chuva`.

**Tech Stack:** Unity 6000.5.6f1, URP, Input System 1.20, TextMeshPro, Unity Test Framework 1.7 (NUnit), `unity` CLI conectado ao Editor aberto, Python 3 + numpy (sons), Blender em linha de comando (pose de carregar).

**Spec:** `docs/superpowers/specs/2026-10-29-parte-b-focos-vivos-e-chuva-design.md`

## Global Constraints

- **Mesmas regras da Parte A** (ver `docs/superpowers/plans/2026-10-25-parte-a-kit-do-agente.md`, Global Constraints):
  - pastas, estilo do projeto (pt-BR, cabeçalho `/* ==== ... ==== */`, `[Tooltip]` em pt-BR), textos sem emoji;
  - objetos de teste com `TesteUtil` (`Novo`, `Com<T>`, `Setar`, `LimparTudo`) e `HideAndDontSave`;
  - o ciclo de compilação e teste pelo `unity` CLI;
  - capturas com o Canvas trocado temporariamente para `ScreenSpaceCamera`.
- Toda classe de teste nova leva `[Category("ParteB")]`. O ciclo roda **as duas categorias**: `--filter ParteA` e `--filter ParteB`. "PASS" = `Failed: 0` nas duas.
- Enums serializados como número: **valores novos só no fim**. `ResultadoDoPasso` ganha `JaVazio` e `Esvaziou`, nessa ordem, depois de `Bloqueado`.
- **Valores da spec** (padrões dos campos, ajustáveis no Inspector):
  - ciclo: `segundosPorEstagio` 75; pesos ovo 1, larva 1,5, mosquito 2,5; `focosComMosquitoNoInicio` 3; `fracaoLarvaNoInicio` 1/3;
  - atalho: `passosParaEsvaziar` 3, `segundosParaEsvaziar` 1,2;
  - chuva: intervalo entre inícios 180–240 s, previsão 45 s, chovendo 20 s, volta do visual 10 s;
  - carga: `multiplicadorCarregando` 0,85 (o valor final sai da Task 14);
  - pneus: diâmetro ~0,7 m; 6 vagas na carroceria.
- Esvaziado: **pesa 0, não zumbe, não dá `alivioPorFoco`, não dispara `OnCompleted`, não conta para a vitória**. Reenchido volta como **larva**.
- O atalho só existe em vaso, pneu e garrafas (`podeEsvaziar`), e só com `currentClicks == 0`.
- Em EditMode, `Awake`/`Start`/`Update` não rodam. Tudo que é lógica tem um método público chamável no teste (`Garantir`, `Registrar`, `Avancar`, `Iniciar`), e o `Update` só chama esses métodos.
- **Nada de commit** sem pedido do usuário. Os passos de "checkpoint" só conferem `git status`.
- No fim: tudo registrado no `contexto do jogo.md` (Task 15) e só a cena `MenuInicial` aberta.

## Review Focus

1. **Chuva com o pneu nos braços.** O pneu carregado não pode ser reenchido (não está no chão), e ao ser largado depois continua vazio. Teste: `GerenciadorDeFocosTestes.Reencher_PneuCarregado_NaoReenche` (Task 4).
2. **Foco esvaziado e depois tratado de vez.** Ele sai do ciclo, pesa 0 para sempre e a chuva seguinte não o "ressuscita". Teste: `GerenciadorDeFocosTestes.Reencher_IgnoraConcluidos` (Task 4).
3. **Partida encerrada (vitória ou derrota) com chuva agendada ou em andamento.** O relógio da chuva para, não há reenchimento nem post. Teste: `ChuvaTestes.PartidaEncerrada_NaoAvanca` (Task 7).
4. **Garrafas com o recolhimento começado.** Com 1 garrafa já no saco, as Mãos não esvaziam (dão `FerramentaErrada`), para o grupo não ficar meio recolhido e meio "vazio". Teste: `EsvaziarTestes.AcaoDefinitivaComecada_SemAtalho` (Task 2).
5. **Mais pneus entregues do que vagas** (a Parte C pode sortear mais de 6). O pneu sem vaga ainda conclui e encolhe como antes, sem erro. Teste: `DestinosTestes.Cacamba_SemVaga_ConcluiEEncolhe` (Task 8).

---

### Task 1: Ciclo de vida do foco

**Files:**
- Create: `Assets/Scripts/CicloDeVida.cs`
- Test: `Assets/Editor/Testes/CicloDeVidaTestes.cs`

**Interfaces:**
- Produces:
  - `public enum EstagioDoFoco { Ovo, Larva, Mosquito }`;
  - `public class CicloDeVida`:
    - `CicloDeVida(float segundosPorEstagio)`;
    - `EstagioDoFoco Estagio { get; }`, `float TempoNoEstagio { get; }`;
    - `bool Avancar(float dt)`: soma `dt` e atravessa quantos estágios o tempo permitir (um `dt` de 160 s com 75 s por estágio vai de Ovo a Mosquito). O Mosquito é o fim: o tempo para de importar. Devolve true se o estágio mudou;
    - `void Zerar()`: Ovo, tempo 0;
    - `void Reencher()`: Larva, tempo 0;
    - `void Definir(EstagioDoFoco estagio, float tempo)`.

- [ ] **Step 1: Write the failing tests:**
  - `Comeca_OvoZero`;
  - `Avancar_74s_ContinuaOvo_DevolveFalse`;
  - `Avancar_75s_ViraLarva_DevolveTrue`;
  - `Avancar_160s_DeUmaVez_ViraMosquito`;
  - `Mosquito_EhOFim`: `Definir(Mosquito, 0)` e `Avancar(1000)` dão false, e o estágio continua Mosquito;
  - `Zerar_VoltaOvo`: de Mosquito para Ovo, com tempo 0;
  - `Reencher_VoltaLarva`: de Ovo para Larva, com tempo 0;
  - `Definir_GuardaTempo`: `Definir(Larva, 30)`, depois `Avancar(45)`, vira Mosquito.
- [ ] **Step 2:** Rodar o ciclo. Esperado: falha de compilação.
- [ ] **Step 3:** Implementar.
- [ ] **Step 4:** Rodar o ciclo. Esperado: PASS (ParteA inteira e os 8 novos).
- [ ] **Step 5: Checkpoint.**

### Task 2: Atalho de esvaziar na base, no vaso e nas garrafas

**Files:**
- Modify: `Assets/Scripts/Ferramenta.cs` (enum `ResultadoDoPasso`)
- Modify: `Assets/Scripts/Interactable.cs` (campos, `Verificar`, `AvancarCom`, `Zumbe`, ganchos)
- Modify: `Assets/Scripts/VasoInteractable.cs`, `Assets/Scripts/GarrafaInteractable.cs`
- Modify: `Assets/Scripts/Interactor.cs` (usar `SegundosPorPassoCom`; `Esvaziou` encerra a sessão como `Concluiu`; `JaVazio` é recusa, como `FerramentaErrada`)
- Modify: `Assets/Scripts/PneuInteractable.cs` (só remover a propriedade `Esvaziado` própria, que passa para a base; o comportamento do pneu fica para a Task 3)
- Test: `Assets/Editor/Testes/EsvaziarTestes.cs`

**Interfaces:**
- Produces (`ResultadoDoPasso`): `JaVazio` e `Esvaziou`, no fim.
- Produces (`Interactable`):
  - campos `[SerializeField] protected bool podeEsvaziar = false`, `protected int passosParaEsvaziar = 3` e `protected float segundosParaEsvaziar = 1.2f`;
  - `public bool PodeEsvaziar`, `public bool Esvaziado { get; private set; }` e `public int PassosEsvaziando`;
  - `public event Action<Interactable> OnEsvaziado, OnReenchido`;
  - `protected bool NoCaminhoDeEsvaziar(KitDoAgente kit)` = `kit != null && kit.Selecionada == Maos && podeEsvaziar && ferramentaNecessaria != Maos && currentClicks == 0`;
  - `public virtual float SegundosPorPassoCom(KitDoAgente kit)`: no caminho de esvaziar, `segundosParaEsvaziar / passosParaEsvaziar`; senão, `SegundosPorPasso`;
  - `Verificar`: depois de `MaosOcupadas` e antes de `AceitaFerramenta`, no caminho de esvaziar, devolve `JaVazio` se `Esvaziado` e `Avancou` se não;
  - `AvancarCom`: no caminho de esvaziar, soma um passo de esvaziar, chama `AoAvancarEsvaziando(int passo)` e dispara `OnClicado`. No último passo chama `Esvaziar()` e devolve `Esvaziou`. Não mexe em `currentClicks`;
  - `protected void Esvaziar()`: `Esvaziado = true`, zera os passos de esvaziar, chama `AoEsvaziar()` e dispara `OnEsvaziado`;
  - `public virtual bool PodeReencher => Esvaziado && !IsComplete`;
  - `public void Reencher()`: se `PodeReencher`, `Esvaziado = false`, chama `AoReencher()` e dispara `OnReenchido`;
  - `Zumbe` = `CanInteract && !Esvaziado`;
  - ganchos `protected virtual void AoAvancarEsvaziando(int passo)`, `AoEsvaziar()` e `AoReencher()`, vazios.
- Produces (`VasoInteractable`): `AoAvancarEsvaziando` baixa a água (`water.transform.localScale.y`) na fração `1 - passo/passosParaEsvaziar`; `AoEsvaziar` desliga `water`; `AoReencher` religa com a escala original.
- Produces (`GarrafaInteractable`): os filhos `agua*` das garrafas restantes somem no `AoEsvaziar` e voltam no `AoReencher`. Sem filho de água, nada visual (o som da Task 12 cobre).
- Produces (`Interactor`): `foco.SegundosPorPassoCom(kit)` no lugar de `foco.SegundosPorPasso`.

- [ ] **Step 1: Write the failing tests.** O vaso tem `ferramentaNecessaria` Areia, `podeEsvaziar` true e `requiredClicks` 3; o kit fica num objeto com `ControladorJogador`.
  - `Maos_EsvaziaEmTresPassos`: com Maos, dois `AvancarCom` dão Avancou e o terceiro dá Esvaziou; `Esvaziado` true, `OnEsvaziado` 1×, `OnCompleted` 0×, `IsComplete` false e `CurrentClicks` 0;
  - `Esvaziado_Maos_JaVazio`;
  - `Esvaziado_NaoZumbe` e `Reenchido_VoltaAZumbir`;
  - `Esvaziado_AreiaResolveDeVez`: com Areia, 3 passos dão Concluiu; `IsComplete` true e a areia é consumida;
  - `AcaoDefinitivaComecada_SemAtalho` (Review Focus 4): 1 passo de Areia e, depois, Maos dá FerramentaErrada;
  - `SemPodeEsvaziar_MaosDaFerramentaErrada`: ralo de Tela com `podeEsvaziar` false;
  - `Reencher_SoQuandoEsvaziado`: `Reencher()` num foco não esvaziado não dispara `OnReenchido`;
  - `SegundosPorPassoCom_Esvaziar`: com Maos dá 0,4 (1,2 / 3); com Areia dá `SegundosPorPasso`;
  - `Vaso_Esvaziado_AguaDesligada_Reenchido_AguaLigada`;
  - `Garrafas_EsvaziarNaoTiraGarrafa`: `GarrafasRestantes` não muda.
- [ ] **Step 2:** Rodar o ciclo. Esperado: falhas.
- [ ] **Step 3:** Implementar `Ferramenta`, `Interactable`, `Vaso`, `Garrafa` e `Interactor`, e tirar o `Esvaziado` do pneu (o pneu passa a usar o da base, setado no `AoTerminarAcao` com `Esvaziar()`).
- [ ] **Step 4:** Rodar o ciclo. Esperado: PASS (ParteA e ParteB).
- [ ] **Step 5: Checkpoint.**

### Task 3: Pneu — esvaziar no lugar, pegar de novo e reencher

**Files:**
- Modify: `Assets/Scripts/PneuInteractable.cs` (`AoTerminarAcao`, `AvancarCom`, `PodeReencher`, `AoReencher`)
- Modify: `Assets/Editor/Testes/PneuCarregarTestes.cs` (os testes que esperavam pegar no último passo)
- Test: `Assets/Editor/Testes/PneuEsvaziarTestes.cs`

**Interfaces:**
- Consumes: `Esvaziar`, `Esvaziado`, `PodeReencher`, `AoReencher` e `ResultadoDoPasso.Esvaziou` (Task 2).
- Produces (`PneuInteractable`):
  - `AoTerminarAcao(kit)`: desliga a água e chama `Esvaziar()`. Com `kit == null` (caminho antigo), conclui como antes. **Não pega**;
  - `AvancarCom(kit)`: se `Esvaziado`, confere `Verificar` e devolve Pegou com `kit.Pegar(this)`, como na A. Senão, chama a base e devolve `Esvaziou` quando o pneu terminou esvaziado;
  - `PodeReencher` = `base.PodeReencher && !Carregado`;
  - `AoReencher()`: liga a água e faz `currentClicks = 0`.

- [ ] **Step 1: Write the failing tests:**
  - `UltimoPasso_EsvaziaEFicaNoLugar`: com Maos e `requiredClicks` 2, os resultados são Avancou e depois Esvaziou; `PneuCarregado` null, `transform.parent` sem mudança, `Esvaziado` true e `OnEsvaziado` 1×;
  - `Esvaziado_ProximoPasso_Pega`: dá Pegou;
  - `Reencher_NoChao_AguaVoltaECliquesZeram`: depois do `Reencher()`, `Esvaziado` false, a água ativa, `CurrentClicks` 0 e `Zumbe` true;
  - `Carregado_NaoReenche`: `PodeReencher` false.
- [ ] **Step 2:** Atualizar o `PneuCarregarTestes`: onde ele esperava Pegou no último passo, passa a esperar Esvaziou e mais um `AvancarCom` para pegar.
- [ ] **Step 3:** Rodar o ciclo. Esperado: falhas só nos testes novos.
- [ ] **Step 4:** Implementar.
- [ ] **Step 5:** Rodar o ciclo. Esperado: PASS (ParteA e ParteB).
- [ ] **Step 6: Checkpoint.**

### Task 4: GerenciadorDeFocos

**Files:**
- Create: `Assets/Scripts/GerenciadorDeFocos.cs`
- Test: `Assets/Editor/Testes/GerenciadorDeFocosTestes.cs`

**Interfaces:**
- Consumes: `CicloDeVida` e `EstagioDoFoco` (Task 1); `OnEsvaziado`, `OnReenchido`, `Esvaziado`, `Reencher` e `PodeReencher` (Tasks 2 e 3); `Interactable.OnCompleted` e `OnClicado`; `FluxoDeJogo.PartidaIniciada`, `PartidaEncerrada` e `OnPartidaIniciada`.
- Produces (`public class GerenciadorDeFocos : MonoBehaviour`):
  - campos: `FluxoDeJogo fluxo`, `segundosPorEstagio` 75, `pesoOvo` 1, `pesoLarva` 1,5, `pesoMosquito` 2,5, `focosComMosquitoNoInicio` 3 e `fracaoLarvaNoInicio` 1/3;
  - `IReadOnlyList<Interactable> Focos`, `int Total`, `int Eliminados` e `int Restantes`;
  - eventos `Action<Interactable> OnFocoEliminado, OnFocoClicado, OnFocoEsvaziado, OnFocoReenchido` e `Action<Interactable, EstagioDoFoco> OnEstagioMudou`;
  - `void Garantir()`: se ainda não registrou, chama `Registrar(FindObjectsByType<Interactable>(FindObjectsInactive.Include, FindObjectsSortMode.None))`. O `Awake` chama `Garantir()`;
  - `void Registrar(IEnumerable<Interactable> focos)`: cria um `CicloDeVida` por foco, assina os eventos dos focos, repassa-os e conta os já concluídos;
  - `EstagioDoFoco EstagioDe(Interactable foco)` e `float PesoDe(Interactable foco)`: 0 se concluído ou esvaziado; senão, o peso do estágio;
  - `float Pressao()`: soma dos `PesoDe` dividida por `Total` (0 se `Total` == 0);
  - `bool CicloAtivo`: `fluxo != null && fluxo.PartidaIniciada && !fluxo.PartidaEncerrada`;
  - `void Avancar(float dt)`: avança o ciclo de cada foco não concluído e não esvaziado e dispara `OnEstagioMudou` quando muda. O `Update` só chama com `CicloAtivo`;
  - ao esvaziar: `Zerar()` no ciclo e `OnEstagioMudou(foco, Ovo)`. Ao reencher: `Reencher()` no ciclo e `OnEstagioMudou(foco, Larva)`;
  - `int ReencherEsvaziados()`: chama `Reencher()` em todo foco com `PodeReencher` e devolve quantos;
  - `void SortearInicio(System.Random rng)`:
    - escolhe `min(focosComMosquitoNoInicio, Restantes)` focos para Mosquito;
    - dos outros, `round(fracaoLarvaNoInicio × n)` para Larva e o resto Ovo;
    - o tempo inicial de cada um é sorteado em `[0, segundosPorEstagio)`;
    - dispara `OnEstagioMudou` para cada um que não é Ovo;
    - é chamado no `OnPartidaIniciada` com `new System.Random()`.

- [ ] **Step 1: Write the failing tests.** Os focos de teste são passados por `Registrar`; o `FluxoDeJogo` é de teste.
  - `Registrar_ContaTotalEliminados`: 3 focos, 1 já concluído, dão `Total` 3, `Eliminados` 1 e `Restantes` 2;
  - `RepassaEliminado`: concluir um foco dispara `OnFocoEliminado` 1× e `Eliminados` sobe;
  - `Pressao_Pesos`: 4 focos (Ovo, Larva, Mosquito e um esvaziado) dão (1 + 1,5 + 2,5 + 0) / 4 = 1,25;
  - `Pressao_ConcluidoPesaZero`;
  - `Avancar_MudaEstagioEDispara`: `Avancar(75)` dispara `OnEstagioMudou(foco, Larva)`;
  - `Avancar_NaoMexeEmEsvaziado`;
  - `CicloInativo_AntesDaPartida`: com o fluxo novo, `CicloAtivo` false;
  - `Esvaziar_ZeraCiclo`: um foco em Mosquito, esvaziado, fica em Ovo e com `PesoDe` 0;
  - `Reencher_VoltaLarva`: `ReencherEsvaziados()` devolve 1 e o estágio fica Larva;
  - `Reencher_PneuCarregado_NaoReenche` (Review Focus 1): devolve 0 e o pneu continua `Esvaziado`;
  - `Reencher_IgnoraConcluidos` (Review Focus 2): esvaziado e depois concluído com Areia, `ReencherEsvaziados()` devolve 0;
  - `Sortear_RespeitaMosquitos`: 10 focos e `new System.Random(1)` dão exatamente 3 em Mosquito, `round(7/3)` = 2 em Larva e 5 em Ovo;
  - `Sortear_PoucosFocos_NaoPassaDoTotal`: 2 focos dão 2 em Mosquito, sem exceção.
- [ ] **Step 2:** Rodar o ciclo. Esperado: falha de compilação.
- [ ] **Step 3:** Implementar.
- [ ] **Step 4:** Rodar o ciclo. Esperado: PASS (ParteA e ParteB).
- [ ] **Step 5: Checkpoint.**

### Task 5: FluxoDeJogo usa o gerenciador e a pressão por peso

**Files:**
- Modify: `Assets/Scripts/FluxoDeJogo.cs` (`Start`, `OnDestroy`, `Update`, `TaxaPara`, `HandleFocoCompletado`)
- Modify: `Assets/Editor/BalanceamentoSimulador.cs` (só o que for preciso para compilar: `SimularRitmo` passa `pressao = (float)ativos / total`; o modelo completo é a Task 14)
- Test: `Assets/Editor/Testes/FluxoPressaoTestes.cs`

**Interfaces:**
- Consumes: `GerenciadorDeFocos` (Task 4).
- Produces (`FluxoDeJogo`):
  - campo `[SerializeField] GerenciadorDeFocos gerenciador` (vazio = `FindAnyObjectByType`);
  - `public float TaxaPara(float tempo, float pressao, bool celularAberto)`: a mesma fórmula, com `pressao` no lugar de `focosAtivos/focosTotal`, **sem `Clamp01`** (pode passar de 1);
  - `FocosTotal => gerenciador.Total` e `FocosEliminados => gerenciador.Eliminados`;
  - o alívio e a vitória vêm de `gerenciador.OnFocoEliminado`;
  - `internal void Ligar()`: o corpo atual do `Start` (achar o gerenciador e assinar), chamável nos testes;
  - `internal void DefinirEstadoParaTeste(bool iniciada, bool encerrada)`, usado pelos testes da Task 7;
  - o `Update` usa `gerenciador.Pressao()`.

- [ ] **Step 1: Write the failing tests.** Um `FluxoDeJogo` de teste com `taxaComTodosOsFocos` 0,003, `recuperacaoSemFocos` 0,001, `duracaoInicioCalmo` 0 e `duracaoRampa` 0.
  - `TaxaPara_PressaoUm`: `TaxaPara(100, 1, false)` ≈ 0,002;
  - `TaxaPara_PressaoDoisEMeio`: `TaxaPara(100, 2.5f, false)` ≈ 0,0065;
  - `TaxaPara_PressaoZero_Desce`: ≈ -0,001;
  - `Vitoria_QuandoGerenciadorEliminaTodos`: com o gerenciador ligado por `Setar` e 2 focos registrados, concluir os 2 dispara `OnVitoria` 1×. O fluxo é ligado com `Ligar()`.
- [ ] **Step 2:** Rodar o ciclo. Esperado: falha de compilação.
- [ ] **Step 3:** Implementar o `FluxoDeJogo` e o ajuste mínimo do simulador.
- [ ] **Step 4:** Rodar o ciclo. Esperado: PASS (ParteA e ParteB).
- [ ] **Step 5: Checkpoint.**

### Task 6: As outras varreduras migram para o gerenciador e o zumbido varia com o estágio

**Files:**
- Modify: `Assets/Scripts/DengueHUD.cs`, `FocoListaHUD.cs`, `RedeSocial.cs`, `HistoriaRedeSocial.cs`, `SomDosFocos.cs` e `FocoProximityGlow.cs` (cada laço `FindObjectsByType<Interactable>` e as assinaturas por foco)
- Test: `Assets/Editor/Testes/MigracaoGerenciadorTestes.cs`

**Interfaces:**
- Consumes: `GerenciadorDeFocos` (Task 4).
- Produces:
  - em cada um dos 6 scripts, um campo `[SerializeField] GerenciadorDeFocos gerenciador` (vazio = `FindAnyObjectByType`), com `Garantir()` antes de ler `Focos`. As listas próprias passam a vir de `gerenciador.Focos`, e as assinaturas, de `OnFocoEliminado`/`OnFocoClicado`. A lógica de cada sistema não muda;
  - `SomDosFocos`: campo `Vector3 volumePorEstagio = (0.45, 0.7, 1.0)` (ovo, larva, mosquito) e `internal static float FatorDoEstagio(EstagioDoFoco e, Vector3 tabela)`. O volume de cada zumbido é multiplicado pelo fator de `gerenciador.EstagioDe(foco)`.

- [ ] **Step 1: Write the failing tests:**
  - `SoOGerenciadorVarreFocos`: lê todo `Assets/Scripts/*.cs` com `File.ReadAllText`. Só `GerenciadorDeFocos.cs` pode conter `FindObjectsByType<Interactable>`, ignorando as linhas de comentário (as que, sem espaços no começo, começam com `//`, `/*` ou `*`);
  - `FatorDoEstagio_Tabela`: Ovo 0,45, Larva 0,7, Mosquito 1,0.
- [ ] **Step 2:** Rodar o ciclo. Esperado: o primeiro falha, apontando os 6 arquivos.
- [ ] **Step 3:** Migrar os 6 scripts, um por vez, compilando entre eles.
- [ ] **Step 4:** Rodar o ciclo. Esperado: PASS (ParteA e ParteB). O `HistoriaKitTestes` e o `KitHUDTestes` continuam passando.
- [ ] **Step 5: Smoke em Play** (`eval_file`, com a abertura pulada e `ControleAtivo = false`): concluir 1 foco por código atualiza o contador do `DengueHUD`, a lista do `FocoListaHUD` e o `FluxoDeJogo.FocosEliminados`; o Console fica sem erros novos.
- [ ] **Step 6: Checkpoint.**

### Task 7: Chuva (relógio) e ChuvaVisual

**Files:**
- Create: `Assets/Scripts/Chuva.cs`
- Create: `Assets/Scripts/ChuvaVisual.cs`
- Test: `Assets/Editor/Testes/ChuvaTestes.cs`

**Interfaces:**
- Consumes: `GerenciadorDeFocos.ReencherEsvaziados()` (Task 4); `FluxoDeJogo.OnPartidaIniciada`, `PartidaIniciada` e `PartidaEncerrada`.
- Produces:
  - `public enum EstadoDaChuva { Seco, Previsao, Chovendo }`;
  - `public class Chuva : MonoBehaviour`:
    - campos `GerenciadorDeFocos gerenciador`, `FluxoDeJogo fluxo`, `Vector2 intervaloEntreInicios = (180, 240)`, `float segundosDePrevisao = 45` e `float segundosChovendo = 20`;
    - `EstadoDaChuva Estado`, `float Tempo` (desde o `Iniciar`), `float ProximoInicio` e `int Chuvas`;
    - eventos `Action OnPrevisao, OnComecou, OnTerminou`;
    - `void Iniciar(System.Random rng)`: `Tempo` 0, `Estado` Seco e `ProximoInicio` sorteado no intervalo. É chamado no `OnPartidaIniciada`;
    - `void Avancar(float dt)`: nada se não iniciou ou se `fluxo != null && fluxo.PartidaEncerrada`. Faz as transições:
      - Seco → Previsão em `ProximoInicio - segundosDePrevisao`, com `OnPrevisao`;
      - Previsão → Chovendo em `ProximoInicio`: `Chuvas++`, `gerenciador?.ReencherEsvaziados()`, `OnComecou` e sorteio do próximo início (`ProximoInicio +=` um valor uniforme em `[intervaloEntreInicios.x, intervaloEntreInicios.y]`, com `rng.NextDouble()`);
      - Chovendo → Seco em `inicioDaChuvaAtual + segundosChovendo`, com `OnTerminou`;
    - o `Update` chama `Avancar(Time.deltaTime)`;
  - `public class ChuvaVisual : MonoBehaviour`:
    - escuta a `Chuva`, com campos `Light sol`, `float fatorDeLuzNaChuva = 0.45f`, `float densidadeDeNevoaNaChuva`, `ParticleSystem gotas` (preso à câmera) e `float segundosDeVolta = 10`;
    - na Previsão, faz a rampa de luz e névoa por 45 s; no `OnComecou`, liga as gotas; no `OnTerminou`, desliga as gotas e volta a luz e a névoa em 10 s;
    - no `OnVitoria`/`OnDerrota`, volta tudo em 10 s;
    - guarda a intensidade e a névoa originais no `Start`;
    - o som fica para a Task 12.

- [ ] **Step 1: Write the failing tests.** Chuva com `intervaloEntreInicios` (200, 200), para o sorteio ser determinístico.
  - `Iniciar_Seco_ProximoInicio200`;
  - `Previsao_Aos155`: `Avancar(155)` deixa o estado em Previsao, com `OnPrevisao` 1×;
  - `Chove_Aos200_Reenche`: um vaso esvaziado registrado no gerenciador fica reenchido depois do `Avancar(200)`, com `OnComecou` 1× e `Chuvas` 1;
  - `Termina_Aos220`: Seco e `OnTerminou` 1×;
  - `ProximaChuva_Aos400`: `ProximoInicio` 400 e a segunda previsão aos 355;
  - `Intervalo_DentroDoSorteio`: com (180, 240) e 20 sementes, todo `ProximoInicio` cai em [180, 240];
  - `PartidaEncerrada_NaoAvanca` (Review Focus 3): fluxo com `DefinirEstadoParaTeste(true, true)`. `Avancar(500)` deixa o estado em Seco, sem eventos e com 0 reenchidos;
  - `NaoIniciada_NaoAvanca`.
- [ ] **Step 2:** Rodar o ciclo. Esperado: falha de compilação.
- [ ] **Step 3:** Implementar a `Chuva` e o `ChuvaVisual`.
- [ ] **Step 4:** Rodar o ciclo. Esperado: PASS (ParteA e ParteB).
- [ ] **Step 5: Checkpoint.**

### Task 8: Destinos — caçamba da picape (com vagas) e lixeiras

**Files:**
- Modify: `Assets/Scripts/PontoDeUso.cs` (texto por kit)
- Modify: `Assets/Scripts/Cacamba.cs` (só pneus, vagas, delegar à picape sem pneu, balanço)
- Create: `Assets/Scripts/Lixeira.cs`
- Modify: `Assets/Scripts/KitDoAgente.cs` (`EntregarPneu(Cacamba)`, `LargarPneu`, `multiplicadorCarregando` 0,85)
- Modify: `Assets/Scripts/PneuInteractable.cs` (`Entregar` com vaga e arremesso)
- Modify: `Assets/Scripts/DengueHUD.cs` (mira em ponto usa `TextoPara(kit)`)
- Modify: `Assets/Editor/Testes/PontosDeUsoTestes.cs` e `PneuCarregarTestes.cs` (testes de caçamba e saco)
- Test: `Assets/Editor/Testes/DestinosTestes.cs`

**Interfaces:**
- Produces (`PontoDeUso`): `public virtual string TextoPara(KitDoAgente kit) => textoAoMirar`. O `DengueHUD` passa a usá-lo.
- Produces (`Cacamba : PontoDeUso`):
  - campos `List<Transform> vagas`, `PontoDeReabastecimento picape`, `Transform carroceria` (balança), `float alturaDoArco = 1.5f` e `float duracaoArremesso = 0.6f`;
  - `int Ocupadas`, `int Capacidade => vagas.Count` e `event Action<PneuInteractable, int> OnPneuRecebido` (o pneu e quantas vagas ocupadas);
  - `PodeUsar`: com pneu, true. Sem pneu, delega a `picape.PodeUsar`; sem picape, recusa com "Só pneu vai na caçamba da picape.";
  - `Usar`: com pneu, `kit.EntregarPneu(this)`; sem pneu, `picape.Usar(kit)`;
  - `TextoPara(kit)`: com pneu, "Segure para jogar o pneu na caçamba"; sem pneu, o texto da picape;
  - `internal Transform ReservarVaga()`: a próxima vaga livre (soma `Ocupadas`) ou null;
  - `internal void Receber(PneuInteractable pneu)`: dispara `OnPneuRecebido` e o balanço da carroceria (corrotina de 0,4 s, ±3° no X).
- Produces (`Lixeira : PontoDeUso`):
  - campos `Transform tampa`, `float anguloAberta = 100f` e `AudioClip somTampa`;
  - `PodeUsar`, nesta ordem:
    - kit null: false;
    - com pneu: recusa com "Pneu vai na caçamba da <b>picape da UPA</b>.";
    - saco vazio: recusa com "O saco está vazio.";
    - senão: true;
  - `Usar`: `kit.EsvaziarSaco()`, mais a corrotina de abrir e fechar a tampa e o som.
- Produces (`KitDoAgente`):
  - `public bool EntregarPneu(Cacamba cacamba)`, que substitui a versão com `Vector3`;
  - o `LargarPneu` que cai numa `Cacamba` chama `EntregarPneu(cacamba)`;
  - `multiplicadorCarregando` = 0,85.
- Produces (`PneuInteractable`):
  - `internal void EntregarEm(Cacamba cacamba)`: solta, pega `cacamba.ReservarVaga()` e guarda em `public Transform VagaNaCacamba`. Chama `Concluir()`. Sem vaga, chama `cacamba.Receber(this)` na hora; com vaga, o `Receber` sai no fim do arco;
  - `OnComplete`: com vaga, faz a corrotina de arremesso em arco até a vaga (`alturaDoArco`, `duracaoArremesso`), desliga os colliders e vira filho da vaga com pose zero, **sem encolher**. No fim do arco, chama `EstourarParticulas(posição)` e `cacamba.Receber(this)`. Sem vaga, usa o `base.OnComplete()` (encolhe);
- Produces (`Interactable`): o `SpawnPop` privado passa a ser `protected void EstourarParticulas(Vector3 pos)`, com o mesmo corpo, e a `CompleteAnimation` o chama.

- [ ] **Step 1: Write the failing tests:**
  - `Cacamba_EntregaPneuNaPrimeiraVaga`: com 2 vagas, depois de `Usar` com pneu, `IsComplete` true, `VagaNaCacamba == vagas[0]`, `Ocupadas` 1 (o `OnPneuRecebido` com vaga sai no fim do arco, uma corrotina, e por isso fica para a verificação em Play da Task 15);
  - `Cacamba_SegundoPneu_SegundaVaga`;
  - `Cacamba_SemVaga_ConcluiEEncolhe` (Review Focus 5): com 0 vagas, `IsComplete` true, `VagaNaCacamba` null, `OnPneuRecebido` 1× e sem exceção;
  - `Cacamba_SemPneu_DelegaAPicape`: kit com Areia 0; `PodeUsar` true e, depois de `Usar`, Areia 1;
  - `Cacamba_SemPneuSemPicape_Recusa`: motivo "Só pneu vai na caçamba da picape.";
  - `Cacamba_TextoPara`: com pneu e sem pneu (o texto da picape);
  - `Lixeira_EsvaziaSaco`: com 3 garrafas, `GarrafasNoSaco` fica 0 e `OnSacoEsvaziado` 1×;
  - `Lixeira_ComPneu_Recusa` e `Lixeira_SacoVazio_Recusa` (com os motivos exatos);
  - `Largar_SobreCacamba_Entrega`: pneu largado sobre um `BoxCollider` com `Cacamba` (como o teste M da Parte A) fica `IsComplete` true;
  - `Kit_MultiplicadorCarregando_085`.
- [ ] **Step 2:** Atualizar o `PontosDeUsoTestes` e o `PneuCarregarTestes`: os testes `Cacamba_EsvaziaSaco` e `Cacamba_Vazia_Recusa` passam para o comportamento novo (o saco vai para a `Lixeira`; sem pneu e sem picape, recusa com o motivo novo). As chamadas `EntregarPneu(Vector3)` passam a `EntregarPneu(cacamba)`.
- [ ] **Step 3:** Rodar o ciclo. Esperado: falhas.
- [ ] **Step 4:** Implementar.
- [ ] **Step 5:** Rodar o ciclo. Esperado: PASS (ParteA e ParteB).
- [ ] **Step 6: Checkpoint.**

### Task 9: Enxame de mosquitos e textos da HUD

**Files:**
- Create: `Assets/Scripts/EnxameDeMosquitos.cs`
- Modify: `Assets/Scripts/DengueHUD.cs` (mira, recusas e aviso de recolhido)
- Modify: `Assets/Scripts/KitHUD.cs` (`TextoLinhaPneu`)
- Test: `Assets/Editor/Testes/SinaisTestes.cs`

**Interfaces:**
- Consumes: `GerenciadorDeFocos.OnEstagioMudou`, `OnFocoEsvaziado` e `OnFocoEliminado` (Task 4); `Cacamba.OnPneuRecebido` e `Capacidade` (Task 8); `ResultadoDoPasso.JaVazio` (Task 2).
- Produces (`EnxameDeMosquitos : MonoBehaviour`):
  - campos `GerenciadorDeFocos gerenciador`, `int particulas = 24`, `float raio = 0.9f`, `float altura = 1.0f` e `Color cor = (0.08, 0.08, 0.08)`;
  - `internal void Atualizar(Interactable foco, EstagioDoFoco estagio)`: no Mosquito, cria (se não existe) um filho `"Enxame"` com `ParticleSystem` em loop: espaço local, forma de esfera com `raio`, módulo de ruído forte, partículas de 0,03–0,05 m e `maxParticles` = `particulas`. Em outro estágio, destrói o filho;
  - também destrói o filho no esvaziado e no eliminado.
- Produces (`DengueHUD`), como constantes e funções `internal static` testáveis:
  - `ComplementoDoEstagio(EstagioDoFoco e)`: Ovo → `""`; Larva → `" — <b>com larvas</b>"`; Mosquito → `" — <b>com mosquitos voando</b>"`;
  - `ComplementoEsvaziado` = `" — <b>vazio, por enquanto</b>"`. O complemento de esvaziado substitui o de estágio;
  - `AcaoResolverDeVez(Ferramenta f)` = `$"Segure com a <b>{Ferramentas.Nome(f).ToLowerInvariant()}</b> para resolver de vez"`, a ação mostrada em foco esvaziado;
  - `RecusaJaVazio(Ferramenta f)` = `$"Já está vazio, mas a chuva enche de novo. A <b>{nome minúsculo}</b> resolve de vez."`;
  - `TextoPneuVazio` = `"Pneu vazio: leve à <b>picape da UPA</b>"`;
  - `RecusaSacoCheio` = `"Saco cheio. Jogue numa <b>lixeira</b>."`;
  - `RecusaMaosOcupadas` = `"Mãos ocupadas: leve o pneu à <b>picape da UPA</b> ou largue [botão direito]."`;
  - `TextoPneuRecolhido(int n, int total)` = `$"Pneu recolhido — {n} de {total}"`, mostrado no toast ao `OnPneuRecebido`, com `total` = os `PneuInteractable` do gerenciador;
- Produces (`KitHUD`): `TextoLinhaPneu` = `"Carregando pneu. Leve à <b>picape da UPA</b>. [Botão direito] larga."`.

- [ ] **Step 1: Write the failing tests:**
  - `Enxame_Mosquito_CriaFilho_Larva_Remove`;
  - `Enxame_Esvaziado_Remove`;
  - `Complementos_Textos`, com as strings exatas acima;
  - `RecusaJaVazio_Areia`: dá "Já está vazio, mas a chuva enche de novo. A <b>areia</b> resolve de vez.";
  - `TextoPneuRecolhido_3de6`.
- [ ] **Step 2:** Rodar o ciclo. Esperado: falhas.
- [ ] **Step 3:** Implementar e ligar à mira do `DengueHUD`:
  - o texto do foco recebe o complemento;
  - num foco esvaziado, a ação passa a ser `AcaoResolverDeVez(foco.FerramentaNecessaria)`;
  - a recusa `JaVazio` vai para o `MensagemDeRecusa`.
- [ ] **Step 4:** Rodar o ciclo. Esperado: PASS (ParteA e ParteB).
- [ ] **Step 5: Checkpoint.**

### Task 10: Rede social — chuva, atalho e destinos

**Files:**
- Modify: `Assets/Scripts/HistoriaRedeSocial.cs` (campos, assinaturas e padrões)
- Test: `Assets/Editor/Testes/HistoriaChuvaTestes.cs`

**Interfaces:**
- Consumes: `Chuva.OnPrevisao` (Task 7); `GerenciadorDeFocos.OnFocoEsvaziado` e `OnFocoReenchido` (Task 4); `KitDoAgente.OnPneuEntregue` e `OnSacoEsvaziado`; `TipoDoFoco`.
- Produces (`HistoriaRedeSocial`):
  - campos `[SerializeField] Chuva chuva` (vazio = `FindAnyObjectByType`);
  - `List<PostRedeSocial> aoPreverChuva`: um post por previsão, sorteado sem repetir o anterior;
  - `List<PostRedeSocial> aoEsvaziarPrimeiraVez` (1×);
  - `List<PostRedeSocial> aoReencherPrimeiraVez`: o primeiro reenchido, escolhendo o post cujo `TipoFoco` bate com o do foco. Para isso, `PostRedeSocial` ganha o campo `TipoFoco tipo = TipoFoco.Nenhum` no fim da classe;
  - `List<PostRedeSocial> aoEntregarPneuPrimeiraVez` (1×);
  - o `aoEsvaziarSacoPrimeiraVez` continua, com o texto novo.
- **Textos padrão** (cópia exata, sem emoji):
  - **Previsão** (4 posts, autores existentes):
    - Ana Paula, @anapaula.rs: "Previsão de chuva forte daqui a pouco aqui no bairro! Tudo que estiver só esvaziado vai encher de novo.";
    - João Silva, @joaosilva: "O céu fechou do lado da praça. Vai cair água já já.";
    - Dona Neusa, @neusa.flores: "Recolhi a roupa do varal, que vem chuva aí. Quem esvaziou vaso, bota areia antes que encha.";
    - Seu Osvaldo, @osvaldo.bar: "Trovoada chegando. Daqui a pouco chove forte no bairro.";
  - **Primeiro esvaziar:** Lúcia Andrade, @lu.plantas: "Esvaziar ajuda, mas os ovos ficam grudados na parede. Se chover, eles voltam.";
  - **Primeiro reenchido:**
    - Vaso — Dona Neusa: "Choveu e aquele pratinho já está cheio de larva de novo. Tem que pôr areia.";
    - Pneu — Carlos Mendes: "Choveu e o pneu que só esvaziaram encheu de novo. Pneu tem que ir embora na picape.";
    - Garrafas — Bruna Lima: "Choveu e as garrafas largadas encheram de água de novo. Garrafa vai pro saco e pra lixeira.";
  - **Primeiro pneu na picape:** Carlos Mendes: "Primeiro pneu na caçamba da picape da UPA! Um criadouro a menos no bairro.";
  - **Primeiro saco na lixeira:** Bruna Lima: "Primeiro saco de garrafas na lixeira! Lixo no lugar certo não junta água.";
  - **Revisões:**
    - o `aoCarregarPneu` (Carlos): "Olha o agente carregando pneu pela rua até a picape da UPA! Finalmente alguém tirando esse entulho do terreno.";
    - o `aoUsarFerramentaErrada` do Pneu: "Pneu não adianta só esvaziar: tem que levar embora. Pega com as mãos e leva pra picape da UPA!";
    - o das Garrafas: "Garrafa vai pro saco de lixo e depois pra lixeira. Nada de deixar pelo chão!";
    - os comentários do `VerificarLargados` que citam a caçamba passam a citar a picape e a lixeira.
- Os valores serializados na cena são regravados pelo builder da Task 13, como a Parte A fez com as falas.

- [ ] **Step 1: Write the failing tests**, no padrão do `HistoriaKitTestes`:
  - `Previsao_Posta_SemRepetirOAnterior`: duas previsões seguidas dão textos diferentes;
  - `PrimeiroEsvaziar_Posta1x`: dois esvaziados, 1 post;
  - `PrimeiroReenchido_EscolhePorTipo`: um vaso reenchido dá o texto da Dona Neusa; um segundo reenchido não posta;
  - `PrimeiroPneuNaPicape_Posta1x`.
- [ ] **Step 2:** Rodar o ciclo. Esperado: falhas.
- [ ] **Step 3:** Implementar.
- [ ] **Step 4:** Rodar o ciclo. Esperado: PASS (ParteA e ParteB).
- [ ] **Step 5: Checkpoint.**

### Task 11: Carregando com o corpo — pose, saco na mão e peso

**Files:**
- Modify: `Ferramentas/Blender/gerar_agente.py` (ação `Carregando`)
- Create: `Ferramentas/Blender/gerar_saco.py` (saco low poly, exporta `Saco.fbx`)
- Modify (reexportados): `Assets/Meus/Assets Gerados/Agente de Saude/AgenteSaude.fbx`; Create: `Assets/Meus/Assets Gerados/Saco/Saco.fbx`
- Modify: `Assets/Meus/Assets Gerados/Agente de Saude/Agente_AnimController.controller` (camada nova) + Create: `Agente_Carregando.mask`
- Create: `Assets/Scripts/FeedbackDeCarga.cs`
- Modify: `Assets/Scripts/CameraJogador.cs` (modo pesado), `Assets/Scripts/SomDoJogador.cs` (passo pesado)
- Test: `Assets/Editor/Testes/FeedbackDeCargaTestes.cs`

**Interfaces:**
- Consumes: `KitDoAgente.OnPneuPego`, `OnPneuLargado`, `OnPneuEntregue`, `OnMudou` e `Estoque.GarrafasNoSaco`/`CapacidadeSaco`.
- Produces:
  - **Blender:** uma ação `Carregando` de 1 frame (pose estática). Os `UpperArm` vão à frente e um pouco abertos, os `LowerArm` dobrados, e as `Hand` ficam nas laterais de um disco de 0,7 m centrado cerca de 0,45 m à frente do `Chest` e na altura da cintura. A ação é exportada junto com as outras. Rodar com `blender -b --factory-startup --python gerar_agente.py -- <pasta> --sem-render`;
  - **Animator:**
    - parâmetro `bool Carregando`;
    - camada `Carregando` com peso 1, `AvatarMask` `Agente_Carregando.mask` (transforms `Chest`, os 6 ossos de braço e mão, e `Spine`) e estados `Vazio` ↔ `SegurandoPneu` (clip `Carregando`), com transições de 0,15 s;
  - **`CameraJogador`:** `public bool ModoPesado { get; set; }`. Com ele, a amplitude do bob × 1,6, o comprimento do passo × 1,25 e a rolagem lateral +1,5°;
  - **`SomDoJogador`:** `public bool PassoPesado { get; set; }`, com volume × 1,3 e pitch × 0,85 nos passos (clips próprios na Task 12);
  - **`FeedbackDeCarga : MonoBehaviour`** (no Player):
    - campos `KitDoAgente kit`, `Animator animator`, `CameraJogador cameraJogador`, `SomDoJogador somDoJogador`, `AudioClip[] esforco`, `Vector2 intervaloEsforco = (3, 6)`, `Transform sacoNaMao`, `Vector2 escalaSaco = (0.5, 1.0)`;
    - `internal void Aplicar()`: com pneu, `animator.SetBool("Carregando", true)` (se houver animator), `cameraJogador.ModoPesado = true` e `somDoJogador.PassoPesado = true`; sem pneu, tudo false;
    - o saco na mão fica ativo quando `GarrafasNoSaco > 0`, com escala `Lerp(escalaSaco.x, escalaSaco.y, garrafas/capacidade)`;
    - escuta o `OnMudou`;
  - **`PontoDeCarga`:** passa a ser filho do osso `Chest` (posição final ajustada no preview), e o saco fica preso ao osso `R_Hand`/`RightHand` (o nome real do rig, conferido no FBX).

- [ ] **Step 1: Write the failing tests** (kit, câmera e som em objetos de teste; animator null):
  - `Pegar_LigaModoPesado`: depois de `kit.Pegar(pneu)`, `ModoPesado` true e `PassoPesado` true;
  - `Largar_DesligaModoPesado`;
  - `Saco_EscalaPelasGarrafas`: com capacidade 6 e 3 garrafas, o saco fica ativo com escala 0,75; com 0, inativo.
- [ ] **Step 2:** Rodar o ciclo. Esperado: falha de compilação.
- [ ] **Step 3:** Implementar o `FeedbackDeCarga`, a `CameraJogador` e o `SomDoJogador`. Rodar o ciclo. Esperado: PASS.
- [ ] **Step 4:** Gerar a pose e o saco no Blender e reimportar. Criar a máscara e a camada do Animator por script de Editor (`eval_file`). Conferir que o Animator compila sem aviso no Console.
- [ ] **Step 5: Preview para o usuário (gate).**
  - Usar a skill `preview-design` e a escala de pneu da Task 13 (~0,7 m) aplicada temporariamente num pneu.
  - Tirar 3 capturas: a Game view em primeira pessoa carregando parado, a mesma andando (bob pesado) e uma de fora, do agente segurando.
  - **Parar e mostrar ao usuário.** Só seguir com a aprovação dele, ajustando a pose e o `PontoDeCarga` conforme o retorno.
- [ ] **Step 6: Checkpoint.**

### Task 12: Sons novos

**Files:**
- Modify: `Ferramentas/Audio/gerar_sons.py`
- Modify: `Assets/Editor/ImportadorDeSons.cs` (prefixos novos)
- Create (gerados): `Assets/Meus/Sons/` com `chuva_loop`, `trovao`, `esvaziar`, `pneu_picape`, `suspensao`, `passo_pesado_1..3`, `esforco_1..3`, `saco_tilintar` e `lixeira_tampa` (na mesma pasta dos 7 WAVs da Parte A)
- Modify: `Assets/Scripts/ChuvaVisual.cs`, `SomDoKit.cs`, `SomDosFocos.cs`, `Cacamba.cs`, `Lixeira.cs`, `FeedbackDeCarga.cs` e `SomDoJogador.cs` (campos `AudioClip` e onde tocam)

**Interfaces:**
- Consumes: os eventos das Tasks 2, 7, 8 e 11.
- Produces, nesta ordem de evento → som:
  - `Chuva.OnComecou` → `trovao` e o início do `chuva_loop` (canal 2D, com fade de 2 s); `OnTerminou` → fade-out de 4 s;
  - `GerenciadorDeFocos.OnFocoEsvaziado` → `esvaziar` (3D, no foco), no `SomDosFocos`;
  - `Cacamba.OnPneuRecebido` → `pneu_picape` e `suspensao`, na hora em que o arremesso termina;
  - `Lixeira.Usar` → `lixeira_tampa`, e o `SomDoKit` continua tocando o despejar no `OnSacoEsvaziado`;
  - `FeedbackDeCarga`: com pneu, um `esforco_*` a cada `intervaloEsforco`; com o saco a mais da metade e o agente andando, `saco_tilintar` a cada passo (ligado ao `CameraJogador.FasePasso`);
  - `SomDoJogador.PassoPesado` → os `passo_pesado_*` no lugar dos passos normais.
- O `ImportadorDeSons` define o import por prefixo, como já faz: loop em `chuva_loop`, mono e 3D nos sons de mundo.

- [ ] **Step 1:** Acrescentar os geradores ao `gerar_sons.py`, todos por síntese com numpy, como os existentes:
  - chuva: ruído rosa filtrado com gotas aleatórias, 8 s, loop sem emenda;
  - trovão: ruído grave com envelope longo, 3 s;
  - esvaziar: splash curto, 0,5 s;
  - pneu na picape: baque de borracha mais metal, 0,6 s;
  - suspensão: rangido curto;
  - passos pesados;
  - esforço: expiração filtrada, sem voz;
  - tilintar: 2–3 batidas de vidro;
  - tampa: plástico, 0,4 s.
  Rodar com `python Ferramentas/Audio/gerar_sons.py` (sem `-I`).
- [ ] **Step 2:** Reimportar e conferir as configurações por `eval_file`: o `chuva_loop` em loop e os demais sem loop.
- [ ] **Step 3:** Ligar os sons. Rodar o ciclo. Esperado: PASS (ParteA e ParteB) e o Console sem erros.
- [ ] **Step 4: Checkpoint.**

### Task 13: Montagem da cena — `Dengue > Configurar Focos Vivos e Chuva`

**Files:**
- Create: `Assets/Editor/FocosVivosBuilder.cs`
- Modify: `Assets/Editor/KitDoAgenteBuilder.cs` (tirar a criação das caçambas de entulho e da tabela `Cacambas`; o texto da recusa das garrafas e do pneu da tabela `Focos` passa a citar a lixeira e a picape)
- Modify: `Assets/Scenes/SampleScene.unity` (pelo builder)

**Interfaces:**
- Consumes: tudo das Tasks 2 a 12.
- Produces: o menu `Dengue > Configurar Focos Vivos e Chuva`, idempotente. Ele usa a cena do `FluxoDeJogo` achado (`fluxo.gameObject.scene`), como o `KitDoAgenteBuilder`, e faz, nesta ordem:
  1. **Gerenciador, chuva e enxame:**
     - acha ou cria `GerenciadorDeFocos` no grupo dos focos, `Chuva` + `ChuvaVisual` num objeto "Clima" (`sol` = a luz direcional da cena, `gotas` = um `ParticleSystem` filho da Main Camera, gerado), e `EnxameDeMosquitos` no gerenciador;
     - liga as referências `gerenciador`/`chuva` no `FluxoDeJogo`, nas 6 varreduras da Task 6 e no `HistoriaRedeSocial`.
  2. **Atalho:** `podeEsvaziar` = true nos prefabs Vaso e nas instâncias de vaso, pneu e garrafas da cena (via `SerializedObject`).
  3. **Pneus em tamanho real:** cada `PneuInteractable` recebe a escala uniforme que leva a maior dimensão dos bounds dos renderers a 0,7 m. O builder assenta o pneu no chão por raycast e registra a escala antes e depois no resumo. Se o pneu já estiver com cerca de 0,7 m, não mexe.
  4. **Caçamba da picape:**
     - na "Picape da UPA", cria o filho "Cacamba da Picape" com um `BoxCollider` sobre a carroceria (medida pelos bounds da metade traseira da picape), o `Cacamba` (`picape` = o `PontoDeReabastecimento`, `carroceria` = a raiz visual da picape) e 6 filhos `Vaga 1..6`;
     - as vagas são 2 colunas × 3 alturas, com os pneus deitados (eixo da roda na vertical) e 0,22 m entre alturas;
     - o `textoAoMirar` da picape continua o da Parte A.
  5. **Remove** "Cacamba", "Cacamba (2)" e "Cacamba (3)" da cena.
  6. **Lixeiras:**
     - `Lixeira` em `Trassh Bin`, `Trassh Bin (1)` e `Trash_01`, com `BoxCollider` se não tiver e `tampa` = o filho de nome com "lid"/"tampa", se houver;
     - cria 2 novas, cópias do prefab da `Trassh Bin`: "Lixeira Terreno Norte" perto de (-22, 52) e "Lixeira Oeste" perto de (-30, -14). O lugar livre é confirmado com `OverlapBox` e uma folga de 0,6 m, como o `KitDoAgenteBuilder` fez com as caçambas;
     - o `textoAoMirar` é "Lixeira";
     - o resumo lista a distância de cada grupo de garrafas à lixeira mais perto.
  7. **Player:** `FeedbackDeCarga` com referências; `PontoDeCarga` filho do osso `Chest`, na pose aprovada na Task 11; saco filho da mão direita; `KitDoAgente.multiplicadorCarregando` 0,85.
  8. **Textos:** regrava os padrões novos da Task 10 nos campos serializados do `HistoriaRedeSocial` da cena (só os campos novos e os 3 revistos).
  9. Salva a cena e imprime o resumo no Console.

- [ ] **Step 1:** Implementar o builder e o ajuste no `KitDoAgenteBuilder`.
- [ ] **Step 2:** Abrir a `SampleScene`, rodar o menu **duas vezes** e conferir por `eval_file`:
  - 1 `GerenciadorDeFocos`, 1 `Chuva`, 1 `Cacamba` (com 6 vagas) e 5 `Lixeira`;
  - 0 objetos "Cacamba (2)"/"(3)";
  - os 6 pneus com a maior dimensão em [0,65; 0,75];
  - 22 focos no gerenciador;
  - a segunda rodada não cria nada novo.
- [ ] **Step 3:** Rodar o ciclo. Esperado: PASS (ParteA e ParteB).
- [ ] **Step 4:** Capturas de cima (scene view) da picape com as vagas e de cada lixeira nova. Conferir que nada atravessa muro ou casa.
- [ ] **Step 5:** Fechar a `SampleScene` e deixar só a `MenuInicial`. **Checkpoint.**

### Task 14: Balanceamento com estágios, chuva e destinos

**Files:**
- Modify: `Assets/Editor/BalanceamentoSimulador.cs` (`SimularRitmo`, `RotaComKit`)
- Modify: `Assets/Scenes/SampleScene.unity` (`FluxoDeJogo.taxaComTodosOsFocos` e, se preciso, `KitDoAgente.multiplicadorCarregando`)

**Interfaces:**
- Consumes: `GerenciadorDeFocos` (pesos, tempos e mosquitos iniciais), `FluxoDeJogo.TaxaPara(tempo, pressao, celular)`, `Cacamba` e `Lixeira` da cena.
- Produces:
  - `SimularRitmo` com o ciclo de cada foco ativo (o início sorteado por semente, com a média de 10 sementes; a ordem de eliminação é a da rota) e `pressao` = soma dos pesos / total;
  - `RotaComKit` com o pneu levado à `Cacamba` da picape à velocidade de carga e o saco esvaziado na `Lixeira` mais perto;
  - o atalho e a chuva não entram: quem joga "do jeito certo" não é afetado pela chuva.

- [ ] **Step 1:** Implementar e rodar o **Dengue > Simular Balanceamento** com os valores atuais. Registrar a tabela.
- [ ] **Step 2:** Ajustar `taxaComTodosOsFocos` (e `multiplicadorCarregando` entre 0,8 e 0,9, se só a taxa não bastar) até cumprir os critérios da Parte A, com o custo de rota novo:
  - 30 s/foco vence com pico ≤ 90%;
  - 35 s/foco vence;
  - 40 s/foco perde;
  - parado perde entre 2:30 e 3:30.
  Gravar os valores na cena via `SerializedObject` e salvar.
- [ ] **Step 3:** Rodar o ciclo. Esperado: PASS. **Checkpoint.**

### Task 15: Verificação final em Play Mode e registro

**Files:**
- Modify: `contexto do jogo.md` (entrada da Parte B implementada)

- [ ] **Step 1: Play via `eval_file`**, com a abertura pulada, `ControleAtivo = false` e `Application.runInBackground = true` só durante o teste. Conferir:
  1. `SortearInicio` deixou 3 focos em Mosquito, cada um com o filho "Enxame";
  2. `gerenciador.Avancar(75)` muda pelo menos um estágio e a `Pressao()` sobe;
  3. um vaso esvaziado com o kit simulado (Maos) zera o peso; `chuva.Avancar` até o início reenche o vaso como Larva; saem a previsão e os posts de primeiro esvaziar e primeiro reenchido;
  4. os 6 pneus esvaziados, pegos e entregues na `Cacamba` da picape ficam em 6 vagas diferentes, e `FocosEliminados` sobe 6;
  5. o saco com garrafas é esvaziado numa `Lixeira`;
  6. com todos os focos tratados, sai `OnVitoria`;
  7. o Console fica sem erros novos (os antigos do `Default-Particle.psd` não contam).
- [ ] **Step 2: Capturas:**
  - o enxame num foco;
  - a chuva (Game view);
  - o pneu nos braços em primeira pessoa;
  - a carroceria com 6 pneus;
  - o saco na mão;
  - a HUD com "com larvas" e com "vazio, por enquanto" (Canvas temporariamente em `ScreenSpaceCamera`).
- [ ] **Step 3:** Limpeza: `PlayerSettings.runInBackground = false`, Play encerrado e só a `MenuInicial` aberta.
- [ ] **Step 4:** Registrar no `contexto do jogo.md`, no formato do `AGENTS.md`:
  - objetivo, arquivos novos e alterados, valores, a tabela do simulador e o que foi verificado;
  - o que **não** foi verificado (a partida real com teclado e mouse);
  - observações para futuras sessões: só o gerenciador varre `Interactable`; o atalho fica na base; os pneus estão em escala real; não há mais caçambas de entulho.
- [ ] **Step 5: Checkpoint final:** `git status`, com a lista dos arquivos para o usuário decidir o commit.
