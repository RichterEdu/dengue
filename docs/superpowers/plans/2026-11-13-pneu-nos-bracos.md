# Pneu nos braços e bromélia mais fácil Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** O agente pega o pneu com água nos braços e esvazia movendo a câmera no ritmo, mais lento quanto mais água; a bromélia fica fácil de tratar.

**Architecture:** O pneu perde o minigame de balançar no chão. Nos braços, o `PneuInteractable` roda a `OndaDoPneu` todo quadro, alimentada pelo olhar que o `Interactor` repassa. O `KitDoAgente` lê o nível da água para a velocidade, e a caçamba recusa pneu com água. Na bromélia, o raio de acerto passa a ser fixo.

**Tech Stack:** Unity 6000.5.6f1, URP, NUnit EditMode, `unity` CLI.

**Spec:** `docs/superpowers/specs/2026-11-13-pneu-nos-bracos-design.md`

## Global Constraints

- Trabalhar na branch `pneu-nos-bracos`, criada a partir da `main`. Commits terminam com `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`.
- Não commitar `Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF - Fallback.asset`, que já estava modificado e não é deste trabalho.
- Não mudar caixa d'água, ralo, vaso, garrafas, piscina, itens na mão, animações dos braços nem sons.
- Valores: velocidade de **0,5** com o pneu cheio a **0,85** vazio, linear com o nível. Raio de acerto da bromélia: **2,5°**.
- Textos exatos:
  - `Cacamba.RecusaPneuComAgua` = `"Esvazie o pneu antes: balance a câmera."`
  - `DengueHUD.AcaoPegarPneu` = `"Clique para pegar o pneu"`
  - Linha do kit com água: `$"Pneu com água ({pct}%): balance a câmera para esvaziar. [Botão direito] larga."`
- Testes pelo CLI:
  - abrir uma cena vazia antes de rodar (cena suja abre uma janela modal e trava o Editor);
  - conferir `Total > 0`, porque o `run_tests` responde sucesso com 0 testes quando a compilação falha;
  - a categoria é `Minigames`.
- Toda alteração entra no `contexto do jogo.md`. O `AGENTS.md` só muda com pedido explícito do usuário.

## Review Focus

- **Largar o pneu com água em cima da caçamba:** continua nos braços, com o aviso de esvaziar, sem entregar (Task 2).
- **Pausa, celular ou cutscene com o pneu cheio nos braços:** a onda não anda e nada esvazia sozinho (Task 2).
- **Largar no meio do esvaziamento e pegar de novo:** o nível continua de onde estava, e a água aparece no chão de novo (Task 1).
- **Olhar em volta sem ritmo:** quase nada derrama; olhar no ritmo esvazia em segundos (Task 1).
- **Chuva com o pneu nos braços ou largado meio cheio:** nos braços não reenche; no chão segue a regra de hoje (Task 1).

---

### Task 1: Pneu com água nos braços

**Files:**
- Modify: `Assets/Scripts/PneuInteractable.cs`
- Create: `Assets/Editor/Testes/PneuNosBracosTestes.cs`
- Delete: `Assets/Editor/Testes/PneuBalancarTestes.cs` (e o `.meta`)

**Interfaces:**
- Produces:
  - `public float NivelDeAgua { get; }`: 0 se `Esvaziado`; com a onda ativa, `onda.Nivel`; senão, `1 − CurrentClicks/RequiredClicks`.
  - `internal void BalancarNosBracos(float dt, float olharX, Vector3 frenteDoAgente)`: só age com `Carregado && !Esvaziado && dt > 0`.
  - `internal ResultadoDoPasso DarPassoNosBracos()`: soma um passo; no último, esconde a água, chama `Esvaziar()` e devolve `Esvaziou`; antes disso, devolve `Avancou`.

- [ ] **Step 1: Criar a branch**

```bash
git switch -c pneu-nos-bracos
git add docs/superpowers/specs/2026-11-13-pneu-nos-bracos-design.md docs/superpowers/plans/2026-11-13-pneu-nos-bracos.md "contexto do jogo.md"
git commit -m "Spec e plano: pneu nos braços"
```

- [ ] **Step 2: Escrever os testes**

Em `PneuNosBracosTestes.cs`, categoria `Minigames`, com a mesma montagem do `PneuBalancarTestes`: pneu com água filha, kit e `TesteUtil`.

| Teste | Assertiva |
|---|---|
| `MaosNoPneuCheioPegaNaHora` | `AvancarCom(kit)` com as Mãos → `Pegou`; `kit.PneuCarregado == pneu`; `Esvaziado == false` |
| `PneuNoChaoNaoTemMinigame` | `CriarMinigame(kit, 0)` e `ConviteDoMinigame(kit)` são null, cheio ou vazio |
| `OutraFerramentaNaoPega` | com a Areia → `FerramentaErrada` e o kit sem pneu |
| `AguaSomeNosBracosEVoltaAoLargar` | depois de pegar, a água inativa; ao largar cheio, ativa de novo |
| `BalancarNoRitmoEsvazia` | `BalancarNosBracos(1/60, 3·sen(2π·t/0,9), Vector3.forward)` por até 12 s simulados → `Esvaziado` antes do fim, e a água inativa |
| `OlharSemRitmoQuaseNaoDerrama` | olharX constante 2 por 5 s → `NivelDeAgua > 0,9` |
| `PassosSobemComAOnda` | depois de `BalancarNoRitmoEsvazia` parcial, `CurrentClicks` = passos fechados pela onda |
| `LargarNoMeioDoBalancoAssentaDePe` | balançar alguns quadros e largar → giro do pneu igual ao do `OndeCai`, sem a inclinação do balanço |
| `UltimoPassoEsvazia` | `DarPassoNosBracos()` `RequiredClicks` vezes → o último devolve `Esvaziou` e os anteriores `Avancou` |
| `LargarNoMeioGuardaONivel` | dois passos; largar; pegar → `NivelDeAgua` igual a `1 − 2/RequiredClicks` |
| `ChuvaNaoReencheNosBracos` | `PodeReencher == false` com o pneu nos braços |
| `NivelDeAguaVazioEhZero` | esvaziado → `NivelDeAgua == 0` |
| `ForaDosBracosBalancarNaoFazNada` | no chão, `BalancarNosBracos` não muda `NivelDeAgua` nem a pose |

- [ ] **Step 3: Rodar e ver falhar**

Run: `unity command run_tests --mode EditMode --filter_type category --filter Minigames`
Expected: erro de compilação, porque `BalancarNosBracos` não existe.

- [ ] **Step 4: Implementar no `PneuInteractable`**

**Sai:** o minigame no chão. `CriarMinigame` e `ConviteDoMinigame` passam a devolver null (ou saem do override), e somem `AoIniciarMinigame`, `AoAtualizarMinigame`, `AoSairDoMinigame`, `OnDisable`, `RestaurarPose`, `Apoio`, `RaioDoPneu` e os campos de apoio e eixo. Os campos da onda no Inspector ficam.

**`AvancarCom(kit)`:**
- Com kit e `!Carregado`, pegar na hora, cheio ou vazio: `Verificar` e depois `kit.Pegar(this)`, devolvendo `Pegou`.
- Sem kit, o caminho antigo continua igual.
- `SegundosPorPasso` passa a 0 com kit.

**`PrenderEm`:** guarda se a água estava ativa e a esconde. Zera a onda.

**`Largar` e `EntregarEm`:** zeram a onda. O `Largar` volta a água a `!Esvaziado`.

**`BalancarNosBracos`:**
- Cria a onda na primeira chamada depois de pegar, com o `nivelInicial` dos passos e os campos do Inspector.
- Chama `Atualizar(dt, new EntradaDoMinigame { Olhar = new Vector2(olharX, 0) })`, e cada `Passo` vira `DarPassoNosBracos()`.
- **Visual:** gira o pneu em torno de `frenteDoAgente` por `Clamp(Inclinacao·inclinacaoMaxima, ±inclinacaoMaxima)` sobre o giro do pai (`PontoDeCarga`) e o desloca de lado por `0,04 m × Clamp(Inclinacao, −1, 1)` na direita do agente.
- **Respingo:** derramando, `EstourarParticulas` no lado baixo, a cada 0,25 s e só no Play.
- **Ao esvaziar:** pose local zerada.

- [ ] **Step 5: Rodar e ver passar**

Run: `unity command run_tests --mode EditMode --filter_type category --filter Minigames`
Expected: todos passam e `Total > 0`.

- [ ] **Step 6: Commit**

```bash
git add Assets/Scripts/PneuInteractable.cs Assets/Editor/Testes/PneuNosBracosTestes.cs* Assets/Editor/Testes/PneuBalancarTestes.cs*
git commit -m "Pneu: pegar com água e esvaziar nos braços"
```

### Task 2: Peso, olhar e caçamba

**Files:**
- Modify: `Assets/Scripts/KitDoAgente.cs`, `Assets/Scripts/Interactor.cs`, `Assets/Scripts/Cacamba.cs`
- Test: `Assets/Editor/Testes/PneuNosBracosTestes.cs`, mais os testes existentes que entregam pneu

**Interfaces:**
- Consumes: Task 1.
- Produces:
  - `KitDoAgente.multiplicadorCheio` (serializado, 0,5)
  - `internal float KitDoAgente.MultiplicadorComPneu(float nivel)` = `Mathf.Lerp(multiplicadorCarregando, multiplicadorCheio, Mathf.Clamp01(nivel))`
  - `public const string Cacamba.RecusaPneuComAgua`
  - `internal void Interactor.BalancarPneuNosBracos(float dt, float olharX)`: sai sem fazer nada com `Bloqueado`, sem kit ou sem pneu nos braços; chamado pelo `Update`

- [ ] **Step 1: Escrever os testes**

| Teste | Assertiva |
|---|---|
| `PesoSegueONivel` | `MultiplicadorComPneu(1) == 0,5`, `(0) == 0,85`, `(0,5) == 0,675` |
| `PegarCheioDeixaLento` | depois de `kit.Pegar(pneu)` cheio, `jogador.MultiplicadorVelocidade == 0,5` |
| `EsvaziandoFicaMaisLeve` | dois passos e `kit.AvancarStamina(0,016, false)` → multiplicador `≈ Lerp(0,85, 0,5, 1 − 2/RequiredClicks)` |
| `CacambaRecusaPneuComAgua` | `cacamba.PodeUsar(kit, out m)` falso, com `m == RecusaPneuComAgua`; esvaziado → verdadeiro |
| `LargarCheioSobreACacambaNaoEntrega` | montagem do `DestinosTestes.Largar_SobreCacamba_Entrega`, com o pneu cheio: `LargarPneu` falso, pneu nos braços e não concluído |
| `InteractorRepassaOOlhar` | com o pneu nos braços, `BalancarPneuNosBracos(1/60, olharNoRitmo)` por alguns segundos faz o `NivelDeAgua` cair; com `Bloqueado = true`, não muda |

- [ ] **Step 2: Rodar e ver falhar**

Run: `unity command run_tests --mode EditMode --filter_type category --filter Minigames`
Expected: erro de compilação, porque `MultiplicadorComPneu` não existe.

- [ ] **Step 3: Implementar**

**`KitDoAgente`:**
- `Pegar` limita com `MultiplicadorComPneu(pneu.NivelDeAgua)`.
- `AvancarStamina`, com o pneu, também ajusta `MultiplicadorVelocidade` a cada quadro.
- `LargarPneu`: se `OndeCai` achar caçamba e o pneu não estiver `Esvaziado`, devolve false e o pneu fica nos braços.
- `EntregarPneu` não muda.

**`Interactor.Update`:**
- Depois do passo 3 (largar), chama `BalancarPneuNosBracos(Time.deltaTime, entrada.Olhar.x)`, que repassa para `kit.PneuCarregado.BalancarNosBracos(dt, olharX, transform.forward)`.
- No largar, se `LargarPneu` devolver false com o pneu ainda nos braços, chama `AvisarRecusa((PontoDeUso)null, Cacamba.RecusaPneuComAgua)`. Conferir que o handler do `OnPontoRecusado` na `DengueHUD` aceita ponto null; se não aceitar, passar a caçamba achada.

**`Cacamba.PodeUsar`:** com o pneu não esvaziado, devolve false com `RecusaPneuComAgua`.

- [ ] **Step 4: Ajustar os testes existentes que entregam pneu cheio**

Rodar a suíte inteira. Os testes que falharem por entregar ou largar sobre a caçamba um pneu cheio passam a esvaziá-lo antes, com `DarPassoNosBracos()` até `Esvaziado`. Nenhum outro assert muda. Candidatos: `DestinosTestes.Largar_ContraACabine_Entrega` e `Largar_SobreCacamba_Entrega`, e o `PontosDeUsoTestes`.

Run: `unity command run_tests --mode EditMode`
Expected: nenhuma falha e `Total > 0`.

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/KitDoAgente.cs Assets/Scripts/Interactor.cs Assets/Scripts/Cacamba.cs Assets/Editor/Testes
git commit -m "Pneu: peso pela água, olhar repassado e caçamba só com pneu vazio"
```

### Task 3: HUD do pneu

**Files:**
- Modify: `Assets/Scripts/KitHUD.cs`, `Assets/Scripts/DengueHUD.cs`
- Test: `Assets/Editor/Testes/KitHUDTestes.cs`, `Assets/Editor/Testes/PneuNosBracosTestes.cs`

**Interfaces:**
- Consumes: `PneuInteractable.NivelDeAgua`, Task 1.
- Produces:
  - `internal static string KitHUD.TextoLinhaPneuComAgua(int pct)`
  - `internal const string DengueHUD.AcaoPegarPneu`
  - `internal static string DengueHUD.AcaoDoFoco` passa a internal, para o teste

- [ ] **Step 1: Escrever os testes**

| Teste | Assertiva |
|---|---|
| `KitHUDTestes.LinhaDeCarga_PneuComAgua` | pneu cheio nos braços → `TextoDaLinha(kit) == TextoLinhaPneuComAgua(100)`, que contém "100%" |
| `KitHUDTestes.LinhaDeCarga_ComOPneu` (existente) | esvaziado nos braços → continua `TextoLinhaPneu` |
| `PneuNosBracosTestes.AcaoNoPneuCheioEhPegar` | `AcaoDoFoco(pneuCheio, false, kitComMaos)` == `AcaoPegarPneu` |

- [ ] **Step 2: Rodar e ver falhar**

Run: `unity command run_tests --mode EditMode --filter_type category --filter Minigames`
Expected: erro de compilação, porque `TextoLinhaPneuComAgua` não existe.

- [ ] **Step 3: Implementar**

- **`KitHUD.TextoDaLinha`:** com o pneu nos braços e `!Esvaziado`, devolve `TextoLinhaPneuComAgua(RoundToInt(NivelDeAgua·100))`.
- **`KitHUD.Update`:** com a linha à mostra, atualiza o texto quando ele mudar.
- **`DengueHUD.AcaoDoFoco`:** num `PneuInteractable` não esvaziado, com as Mãos, devolve `AcaoPegarPneu`.

- [ ] **Step 4: Rodar e ver passar**

Run: `unity command run_tests --mode EditMode --filter_type category --filter Minigames`
Expected: todos passam.

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/KitHUD.cs Assets/Scripts/DengueHUD.cs Assets/Editor/Testes/KitHUDTestes.cs Assets/Editor/Testes/PneuNosBracosTestes.cs
git commit -m "Pneu: HUD da água nos braços e convite de pegar"
```

### Task 4: Bromélia mais fácil

**Files:**
- Modify: `Assets/Scripts/BromeliaInteractable.cs`, `Assets/Scripts/MiraDoBorrifador.cs`
- Test: `Assets/Editor/Testes/BromeliaBorrifarTestes.cs`, `Assets/Editor/Testes/MiraDoBorrifadorTestes.cs`

**Interfaces:**
- Produces: `BromeliaInteractable.raioDeAcerto` com padrão 2,5, usado direto como raio da sessão. Sai `MiraDoBorrifador.RaioParaAsPocinhas`.

- [ ] **Step 1: Escrever o teste**

`BromeliaBorrifarTestes.PrefabReal_MirarPertoDeCadaPocinhaSeca`: no prefab real, a 1,5 m, mirar 2° ao lado de cada pocinha e segurar seca todas. Ele substitui o `PrefabReal_MirarOCentroESegurarSecaNoMaximoUma`, que sai. Sai também o `MiraDoBorrifadorTestes.RaioParaAsPocinhas_NaoAlcancaDuas`.

- [ ] **Step 2: Rodar e ver falhar**

Run: `unity command run_tests --mode EditMode --filter_type category --filter Minigames`
Expected: `PrefabReal_MirarPertoDeCadaPocinhaSeca` falha, porque o raio está em cerca de 0,5°.

- [ ] **Step 3: Implementar**

`CriarMira` passa `raioDeAcerto` direto (2,5). Remover `RaioParaAsPocinhas` e atualizar o comentário do cabeçalho. Conferir que o prefab e a cena não sobrescrevem `raioDeAcerto`.

- [ ] **Step 4: Rodar e ver passar**

Run: `unity command run_tests --mode EditMode`
Expected: nenhuma falha.

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/BromeliaInteractable.cs Assets/Scripts/MiraDoBorrifador.cs Assets/Editor/Testes/BromeliaBorrifarTestes.cs Assets/Editor/Testes/MiraDoBorrifadorTestes.cs
git commit -m "Bromélia: raio de acerto de 2,5 graus"
```

### Task 5: Verificação em Play e registro

**Files:**
- Modify: `contexto do jogo.md`

- [ ] **Step 1: Verificar em Play, com mouse e teclado virtuais**

Mesma técnica do registro de 2026-11-06: `Application.runInBackground` só no teste, abertura pulada, desbloqueio só no teste.

| Situação | Esperado |
|---|---|
| Clique num pneu cheio com as Mãos | Pneu nos braços, água escondida, multiplicador 0,5 |
| Olhar no ritmo andando para a frente | Pneu balança e respinga, o nível cai, e a velocidade sobe até 0,85 |
| Olhar em volta sem ritmo por 5 s | Nível acima de 0,9 |
| Caçamba com água | Aviso "Esvazie o pneu antes"; vazio, entrega |
| Largar meio cheio e pegar de novo | Continua do nível |
| Pausa com o pneu cheio | Nada esvazia |
| Bromélia a 1,5 m, mirando perto de cada pocinha | Seca uma por vez; começar a sessão mirando a planta é fácil |

Capturar o balanço, a HUD da linha e a bromélia. Ao fim, `PlayerSettings.runInBackground = false`.

- [ ] **Step 2: Rodar a suíte inteira**

Run: `unity command run_tests --mode EditMode`
Expected: nenhuma falha e `Total > 0`.

- [ ] **Step 3: Registrar e fazer commit**

Entrada no `contexto do jogo.md` com objetivo, arquivos, valores, testes, o que foi e o que não foi verificado (partida real com mouse e controle).

```bash
git add "contexto do jogo.md"
git commit -m "Pneu nos braços: verificação e registro"
```
