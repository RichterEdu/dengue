# Minigames dos focos (leva 1) — Plano de implementação

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Trocar o "segurar até a barra encher" por uma ação própria em três focos: pneu (balançar no ritmo), bromélia (mirar o borrifador) e caixa d'água (carregar a tampa).

**Architecture:** Uma interface `IMinigame` (C# puro) substitui o skill check escrito direto no `Interactor`; o foco cria a sessão e cada `Passo` dela vira o `AvancarCom(kit)` de sempre. O ralo passa a usar a mesma interface por um adaptador. A tampa da caixa d'água não é minigame: é um segundo objeto carregável no `KitDoAgente`.

**Tech Stack:** Unity 6000.5.6f1, URP, Input System 1.20, C# 9, NUnit (testes EditMode em `Assets/Editor/Testes/`).

**Spec:** `docs/superpowers/specs/2026-11-11-minigames-leva-1-design.md`

## Global Constraints

- Ir mal só perde tempo: nenhum minigame novo gasta carga nem zera o foco. Só o ralo gasta a tela no erro.
- Nenhuma cena ou prefab é editado: todo campo serializado novo tem valor padrão no código.
- `requiredClicks` vem da cena (5 na maioria dos focos) e não muda: as sessões recebem `RequiredClicks` como número de passos.
- Enums `Ferramenta` e `ResultadoDoPasso` são salvos como número: valores novos só no fim. Este plano não cria nenhum.
- Lógica de sessão é C# puro, sem `UnityEngine.Time` nem `MonoBehaviour`; o tempo entra por `dt`.
- `EntradaJogador.Olhar` já vem em graus por quadro (o `ControladorJogador` soma direto no `Yaw`).
- Testes novos: `[Category("Minigames")]`, objetos pelo `TesteUtil`, `TesteUtil.LimparTudo()` no `[TearDown]`.
- Rodar testes: abrir uma cena vazia no Editor antes (cena suja trava o Test Runner), depois
  `unity command run_tests --mode EditMode --filter_type category --filter Minigames`. Suíte completa: o mesmo comando sem os filtros.
- Cada tarefa termina com uma entrada no Histórico de Alterações de `contexto do jogo.md` (formato das entradas existentes) e um commit.
- Comentários e nomes em português, no estilo dos arquivos vizinhos (cabeçalho em bloco no topo de cada script).

## Review Focus

1. **Quadro longo (travada de 0,5 s) no pneu:** a onda não pode explodir nem derramar tudo de uma vez. Esperado: integração em subpassos e no máximo um `Passo` por `Atualizar`. Teste na Tarefa 2.
2. **Celular ou fim de jogo no meio do balanço:** o pneu está inclinado e a água deslocada. Esperado: o minigame encerra sem punição, o pneu volta à pose e os passos fechados ficam. Teste na Tarefa 3. (A chuva não é risco aqui: só reenche foco `Esvaziado`, e o pneu em balanço ainda não está.)
3. **Bromélia sem pocinhas (array vazio ou só nulos) ou nascida seca e reenchida:** esperado: `CriarMinigame` devolve null e o foco cai no segurar, sem divisão por zero. Teste na Tarefa 5.
4. **Fim de jogo ou celular com a tampa nos braços:** esperado: a tampa volta para a lateral da caixa e o movimento do jogador volta ao normal. Teste na Tarefa 6.
5. **Alvo destruído ou desativado com minigame ativo:** esperado: o `Interactor` encerra e libera o `Imobilizado`, sem `NullReferenceException`. Teste na Tarefa 1.

---

### Task 0: Limpar a base (decisão do usuário)

O branch `itens-na-mao` tem o nascimento seco (2026-11-10) não commitado nos mesmos arquivos que este plano edita (`Interactable.cs`, `PneuInteractable.cs`, `BromeliaInteractable.cs`, `CaixaDaguaInteractable.cs`, `RaloInteractable.cs`, `GerenciadorDeFocos.cs`, `FocoHUDInfo.cs`, `GarrafaInteractable.cs`, `PiscinaInteractable.cs`, `NascimentoTestes.cs`, cena e materiais).

- [ ] **Step 1:** Perguntar ao usuário se o nascimento seco pode ser commitado como está. Não commitar sem a resposta.
- [ ] **Step 2:** Com o sim, commitar esse trabalho num commit próprio ("Nascimento seco") e criar o branch `minigames-leva-1` a partir dele.
- [ ] **Step 3:** Rodar a suíte EditMode completa. Esperado: 427/427, `Failed: 0`. Esse é o número de partida.

### Task 1: Base `IMinigame`, ralo no adaptador e `Interactor`

**Files:**
- Create: `Assets/Scripts/Minigame.cs`, `Assets/Editor/Testes/MinigameNoInteractorTestes.cs`
- Modify: `Assets/Scripts/Interactable.cs` (bloco "Skill check", linhas 185-190), `Assets/Scripts/RaloInteractable.cs`, `Assets/Scripts/Interactor.cs`, `Assets/Scripts/SkillCheck.cs`, `Assets/Editor/BalanceamentoSimulador.cs:321-415`
- Modify (chamadas): todos os usos de `UsaSkillCheck`, `ErrarSkillCheck`, `CancelarSkillCheck`, `SegundosPorVoltaDoCheck` (grep em `Assets/`), incluindo `SkillCheckTestes.cs`

**Interfaces:**
- Produces (`Minigame.cs`):
  ```csharp
  public struct EntradaDoMinigame { public Vector2 Olhar; public bool Segurando; public bool Apertou; }
  public enum EstadoDoMinigame { EmAndamento, Passo, Errou }
  public interface IMinigame {
      EstadoDoMinigame Atualizar(float dt, EntradaDoMinigame entrada);
      float FracaoDoPasso { get; }
      bool PodeSair { get; }
  }
  public interface IMinigameComMira { Vector2 Mira { get; } }   // graus (yaw, pitch) a partir do olhar inicial
  ```
- Produces (`Interactable`):
  ```csharp
  public virtual IMinigame CriarMinigame(KitDoAgente kit, int semente) => null;
  public virtual void AoErrarMinigame(KitDoAgente kit) { }
  public virtual void AoSairDoMinigame() { }
  public virtual void AoAtualizarMinigame(IMinigame sessao) { }      // visual por quadro
  public virtual string DicaDoMinigame => null;                       // texto da HUD com a sessão ativa
  public virtual float SegundosEstimados => SegundosPorPasso * RequiredClicks;
  public virtual float ProgressoCom(KitDoAgente kit, float fracao);   // a conta que hoje está no Interactor.AtualizarProgresso
  ```
- Produces (`SkillCheck.cs`): `public class MinigameDeSkillCheck : IMinigame` com `public SessaoDeSkillCheck Sessao { get; }`; `Apertou` → `Sessao.Apertar()`, senão `Sessao.Atualizar(dt)`; `Acertou` → `Passo`, `Errou` → `Errou`; `PodeSair == false`; `FracaoDoPasso == 0`.
- Produces (`Interactor`):
  ```csharp
  public IMinigame MinigameAtivo { get; }
  public SessaoDeSkillCheck SkillCheckAtivo { get; }   // (MinigameAtivo as MinigameDeSkillCheck)?.Sessao
  internal bool IniciarMinigame(Interactable foco);                       // false se CriarMinigame devolve null
  internal void AvancarMinigame(float dt, EntradaDoMinigame entrada, bool sair);
  ```
  `Update` só monta a entrada e chama `AvancarMinigame`. Regras de `AvancarMinigame`: alvo nulo, destruído ou inativo → encerra; `sair && PodeSair` → `AoSairDoMinigame` e encerra; `Passo` → `AvancarCom(kit)`, e qualquer resultado diferente de `Avancou` encerra; `Errou` → `AoErrarMinigame(kit)`, encerra, `OnErrouSkillCheck`; sempre chama `AoAtualizarMinigame`; se a sessão é `IMinigameComMira`, aplica `jogador.DefinirOlhar(yawInicial + Mira.x, pitchInicial + Mira.y)`. `Bloqueado` chama `AoSairDoMinigame` e encerra. `Avancando` é verdadeiro com sessão ativa que não seja `MinigameDeSkillCheck`.

- [ ] **Step 1: Testes que falham** em `MinigameNoInteractorTestes.cs`, com um `FocoDeTeste : Interactable` que devolve um `MinigameFalso` (fila de `EstadoDoMinigame` a devolver, `PodeSair` configurável) e conta as chamadas dos ganchos:
  - `PassoViraAvancarCom`: fila `[Passo]` → `foco.CurrentClicks == 1`, `MinigameAtivo != null`.
  - `UltimoPassoEncerra`: `requiredClicks = 1`, fila `[Passo]` → `foco.IsComplete`, `MinigameAtivo == null`.
  - `SairMantemOsCliques`: um `Passo`, depois `sair = true` → `CurrentClicks == 1`, `saidas == 1`, `MinigameAtivo == null`.
  - `SairIgnoradoSemPodeSair`: `PodeSair = false`, `sair = true` → `MinigameAtivo != null`.
  - `ErroChamaOGanchoENaoGastaCarga`: fila `[Errou]` → `erros == 1`; estoque de areia do kit igual ao de antes.
  - `AlvoDestruidoEncerraSemExcecao`: `Object.DestroyImmediate(foco.gameObject)` com sessão ativa, depois `AvancarMinigame` → `Assert.DoesNotThrow`, `MinigameAtivo == null`.
  - `FocoSemMinigameNaoInicia`: `Interactable` puro → `IniciarMinigame == false`.
  - Em `SkillCheckTestes.cs`: `AdaptadorAcertoViraPasso` e `AdaptadorErroViraErrou` para `MinigameDeSkillCheck`.
- [ ] **Step 2:** Rodar a categoria `Minigames`. Esperado: não compila (`IMinigame` não existe).
- [ ] **Step 3:** Implementar as interfaces acima; remover `UsaSkillCheck`, `SegundosPorVoltaDoCheck`, `ErrarSkillCheck`, `CancelarSkillCheck` da base e migrar o ralo (`CriarMinigame` devolve o adaptador; `AoErrarMinigame` = o `ErrarSkillCheck` de hoje; `AoSairDoMinigame` = o `CancelarSkillCheck` de hoje). `BalanceamentoSimulador` passa a somar `foco.SegundosEstimados` onde hoje soma `SegundosPorPasso * RequiredClicks`; o ralo declara `SegundosEstimados => RequiredClicks * segundosPorVolta`.
- [ ] **Step 4:** Rodar `Minigames` e `SkillCheck`: tudo verde. Rodar a suíte completa: `Failed: 0`.
- [ ] **Step 5:** No Editor, jogar um ralo: acerto, erro (gasta a tela) e celular no meio se comportam como antes.
- [ ] **Step 6:** Registrar no `contexto do jogo.md` e commitar ("Minigames: base IMinigame e ralo no adaptador").

### Task 2: `OndaDoPneu` (lógica pura)

**Files:**
- Create: `Assets/Scripts/OndaDoPneu.cs`, `Assets/Editor/Testes/OndaDoPneuTestes.cs`

**Interfaces:**
- Consumes: `IMinigame`, `EntradaDoMinigame`, `EstadoDoMinigame` (Tarefa 1).
- Produces:
  ```csharp
  public class OndaDoPneu : IMinigame {
      public OndaDoPneu(int passos, float periodo = 0.9f, float amortecimento = 0.12f, float ganho = 15f,
                        float limiarDaBorda = 1f, float taxa = 1.4f, float grausPorSegundoCheio = 180f);
      public float Inclinacao { get; }   // x da onda; ±1 = água na borda
      public float Velocidade { get; }
      public float Nivel { get; }        // 1 cheio .. 0 vazio
      public bool Derramando { get; }    // |Inclinacao| > limiarDaBorda neste quadro
      public bool PodeSair => true;
  }
  ```

- [ ] **Step 1: Testes que falham.** Ajudante `Simular(onda, segundos, Func<OndaDoPneu, float> u)` que anda em quadros de 1/60 s com `Olhar.x = u * 180f / 60f` e devolve quantos `Passo` saíram.
  - `ParadaNaoDerrama`: `u = 0` por 10 s → 0 passos, `Nivel == 1`.
  - `EmFaseEsvazia`: `u = Velocidade >= 0 ? 1 : -1` por 10 s, `passos = 3` → 3 passos, `Nivel == 0`.
  - `ContraFaseNaoPassaDaBorda`: `u = Velocidade >= 0 ? -1 : 1` por 10 s → `|Inclinacao| < 1` em todo quadro, 0 passos.
  - `OndaMorreSozinha`: 2 s em fase, depois `u = 0` por 3 s → `|Inclinacao| < 0.3`.
  - `QuadroLongoNaoExplode`: 2 s em fase, depois um `Atualizar(0.5f, u = 1)` → `|Inclinacao| < 3`, resultado é `EmAndamento` ou `Passo`, e `Nivel` caiu no máximo `1f / passos` nesse quadro.
  - `FracaoDoPassoSobeDentroDoTerco`: `FracaoDoPasso` em [0, 1] e volta perto de 0 logo depois de cada `Passo`.
- [ ] **Step 2:** Rodar: não compila.
- [ ] **Step 3:** Implementar. O que o teste não fixa:
  ```
  ω = 2π / periodo;  u = clamp(Olhar.x / dt / grausPorSegundoCheio, −1, 1)
  subpassos de no máximo 1/120 s (Euler semi-implícito):
      v += (−ω²·x − 2·amortecimento·ω·v + ganho·u) · h;   x += v · h
      se |x| > limiarDaBorda:  nivel −= taxa · (|x| − limiarDaBorda) · h
  nivel nunca desce abaixo do próximo limite de passo num mesmo Atualizar (o resto fica para o quadro seguinte)
  ```
  `dt <= 0` devolve `EmAndamento` sem mudar nada.
- [ ] **Step 4:** Rodar `OndaDoPneuTestes`: verde. Se `EmFaseEsvazia` levar menos de 3 s ou mais de 7 s simulados, ajustar só `ganho` e `taxa` (os padrões do construtor) até cair nessa faixa.
- [ ] **Step 5:** Registrar e commitar ("Minigames: onda do pneu").

### Task 3: Pneu no minigame

**Files:**
- Modify: `Assets/Scripts/PneuInteractable.cs`, `Assets/Scripts/DengueHUD.cs` (texto da mira), `Assets/Editor/Testes/PneuEsvaziarTestes.cs`
- Create: `Assets/Editor/Testes/PneuBalancarTestes.cs`

**Interfaces:**
- Consumes: `OndaDoPneu` (Tarefa 2); ganchos da Tarefa 1.
- Produces em `PneuInteractable`: campos serializados `periodoDaOnda = 0.9f`, `inclinacaoMaxima = 25f`; `CriarMinigame` devolve `new OndaDoPneu(RequiredClicks, periodoDaOnda)` só com `kit.Selecionada == Ferramenta.Maos` e `!Esvaziado`; `SegundosEstimados => Esvaziado ? 0f : 6f`; `DicaDoMinigame => "Balance o pneu ← →   |   Botão direito: sair"`.

- [ ] **Step 1: Testes que falham** (`PneuBalancarTestes`, mesmo `SetUp` do `PneuEsvaziarTestes`):
  - `MaosNoPneuCheioCriaAOnda`: `CriarMinigame(kit, 0) is OndaDoPneu`.
  - `PneuEsvaziadoNaoTemMinigame`: depois de esvaziar → `null` (pegar continua instantâneo).
  - `OutraFerramentaNaoTemMinigame`: areia selecionada → `null`.
  - `UltimoPassoEsvaziaEEncerra`: pelo `Interactor` (`IniciarMinigame` + `AvancarMinigame` em fase até acabar) → `pneu.Esvaziado`, água desativada, `MinigameAtivo == null`, pneu ainda no lugar (não `Carregado`).
  - `SairDevolveAPose`: alguns quadros em fase, depois sair → `transform.rotation` e `position` iguais aos de antes do minigame.
  - `BloqueioNoMeioDevolveAPose`: um passo fechado e o pneu inclinado, `interactor.Bloqueado = true` e o tratamento do bloqueio → `MinigameAtivo == null`, pose original, `CurrentClicks == 1`, `!pneu.Esvaziado`.
  - `ReenchidoRecomecaDoZero`: esvaziar pelo minigame, `Reencher()`, iniciar de novo → `CriarMinigame` devolve uma onda nova com `Nivel == 1` e `CurrentClicks == 0`.
- [ ] **Step 2:** Rodar: falham.
- [ ] **Step 3:** Implementar. `AoAtualizarMinigame`: gira o pneu em torno do ponto de apoio no chão (ponto mais baixo dos `Renderer.bounds` no início) por `Inclinacao × inclinacaoMaxima` graus no eixo da frente da câmera, preso a ±`inclinacaoMaxima`; a água (`water`) ganha escala Y `Nivel` e desloca para o lado baixo; com `Derramando`, `EstourarParticulas` no lado baixo, no máximo uma vez a cada 0,25 s. `AoSairDoMinigame`: pose original e água na escala `1 − CurrentClicks / RequiredClicks`. O tratamento do bloqueio no `Interactor` fica num método `internal void AoBloquear()` (o corpo do `if (Bloqueado)` de hoje), para o teste chamar.
- [ ] **Step 4:** `DengueHUD`: com `interactor.MinigameAtivo != null` e `CurrentTarget.DicaDoMinigame` não vazio, o texto da mira é a dica.
- [ ] **Step 5:** Atualizar `PneuEsvaziarTestes` (os que usam `AvancarCom` direto continuam valendo; nada no caminho `Interact()` muda). Rodar `Minigames` e `ParteB`: verdes.
- [ ] **Step 6:** No Editor: balançar um pneu com mouse e com controle; conferir que leva cerca de 5 s no ritmo e que parado a onda morre. Ajustar `ganho`/`taxa` da Tarefa 2 se preciso e anotar os valores finais no registro.
- [ ] **Step 7:** Registrar e commitar ("Minigames: pneu de balançar").

### Task 4: `MiraDoBorrifador` (lógica pura)

**Files:**
- Create: `Assets/Scripts/MiraDoBorrifador.cs`, `Assets/Editor/Testes/MiraDoBorrifadorTestes.cs`

**Interfaces:**
- Consumes: `IMinigame`, `IMinigameComMira` (Tarefa 1).
- Produces:
  ```csharp
  public class MiraDoBorrifador : IMinigame, IMinigameComMira {
      // pocas[i] = (yaw, pitch) em graus a partir do olhar inicial; null em segundosParaSecar[i] não existe: arrays do mesmo tamanho
      public MiraDoBorrifador(Vector2[] pocas, float[] segundosParaSecar, int passos, float cone = 30f, float raio = 4f);
      public Vector2 Mira { get; }
      public int Total { get; }
      public int Secas { get; }
      public float Umidade(int i);          // 1 cheia .. 0 seca
      public int PocaSecandoAgora { get; }  // -1 = nenhuma
      public bool PodeSair => true;
  }
  ```
  Convenção da mira: `Mira.x += Olhar.x`, `Mira.y -= Olhar.y` (a do `ControladorJogador`), com `|Mira| <= cone`.

- [ ] **Step 1: Testes que falham**, com 3 pocinhas em `(0,0)`, `(10,0)`, `(0,10)`, 0,5 s cada, `passos = 3`:
  - `MiraPresaAoCone`: `Olhar = (100, 0)` → `Mira.magnitude <= 30`.
  - `SegurarForaNaoSeca`: mira em `(20, 20)`, segurar 2 s → `Secas == 0`, `PocaSecandoAgora == -1`.
  - `SemSegurarNaoSeca`: mira em `(0,0)`, 2 s sem `Segurando` → `Umidade(0) == 1`.
  - `SecaAMaisProxima`: mira em `(1, 0)`, segurar 0,6 s → `Umidade(0) == 0`, `Secas == 1`, saiu 1 `Passo`.
  - `SoltarNoMeioGuardaAUmidade`: 0,25 s segurando → `Umidade(0)` entre 0,4 e 0,6; mais 0,3 s → seca.
  - `PassosNasContagensCertas`: 5 pocinhas e `passos = 2` → `Passo` ao secar a 3ª e a 5ª (`ceil(5·k/2)`).
  - `MaisPassosQuePocinhas`: 2 pocinhas e `passos = 5` → saem 5 passos ao todo, um por `Atualizar`, os que faltam logo após a última secar.
  - `FracaoDoPasso` entre 0 e 1.
- [ ] **Step 2:** Rodar: não compila.
- [ ] **Step 3:** Implementar. `cone` efetivo = `max(cone, maior |pocas[i]| + raio + 2)`, para toda pocinha ser alcançável.
- [ ] **Step 4:** Rodar: verde.
- [ ] **Step 5:** Registrar e commitar ("Minigames: mira do borrifador").

### Task 5: Bromélia no minigame

**Files:**
- Modify: `Assets/Scripts/BromeliaInteractable.cs`, `Assets/Scripts/ItensNaMao.cs`, `Assets/Scripts/Interactor.cs` (liga o jato)
- Create: `Assets/Editor/Testes/BromeliaBorrifarTestes.cs`

**Interfaces:**
- Consumes: `MiraDoBorrifador` (Tarefa 4); ganchos da Tarefa 1.
- Produces em `BromeliaInteractable`: campos `segundosPorPoca = 0.5f`, `segundosDoCopoCentral = 1.2f`, `raioDeAcerto = 4f`; `internal MiraDoBorrifador CriarMira(Vector3 olho, Quaternion visao)` (ângulos de cada pocinha ativa vistos do olho; a última do array `pocas` é o copo central); `CriarMinigame` chama `CriarMira` com `Camera.main` e devolve null sem câmera, sem pocinha ativa ou sem a água sanitária selecionada; `SegundosEstimados` = soma dos tempos + 0,4 s por pocinha; `DicaDoMinigame => "Segure para borrifar as pocinhas   |   Botão direito: sair"`.
- Produces em `ItensNaMao`: `public void Borrifar(bool ligado)` (partículas no bico do borrifador; sem o borrifador na mão, não faz nada).

- [ ] **Step 1: Testes que falham** (bromélia de teste com 3 filhos como pocinhas em posições conhecidas, olho a 1,5 m):
  - `CriaAMiraComAsPocinhasAtivas`: `CriarMira(...).Total == 3`.
  - `SemPocinhasNaoTemMinigame`: `pocas` vazio → `CriarMinigame == null`; `pocas` só com nulos → `null`; nenhum lança exceção.
  - `NascidaSecaNaoTemMinigame`: `NascerVazio()` → `null`; depois de `Reencher()` → 3 pocinhas de novo.
  - `PocinhaAtingidaEAQueSeca`: secar pela sessão a pocinha de índice 1 e fechar um passo → só `pocas[1]` inativa (não a de índice 0, como na ordem fixa).
  - `UltimoPassoSecaOQueSobrou`: forçar o último `AvancarCom` com pocinhas ainda ativas → todas inativas, `IsComplete`.
  - `SairGuardaAsSecasEDevolveAMeia`: uma seca, outra a meio caminho, sair → a primeira inativa, a segunda ativa e com a escala original.
  - `CaminhoAntigoMantemAOrdemFixa`: `Interact()` sem kit → seca na ordem do array, como hoje.
  - `DoseSoNoFim`: estoque de água sanitária igual até o passo que conclui, e um a menos depois.
- [ ] **Step 2:** Rodar: falham.
- [ ] **Step 3:** Implementar. `AoAtualizarMinigame` aplica `Umidade(i)` na escala XZ da pocinha `i` e a desativa em zero; o `OnInteract` pula a ordem fixa quando há sessão ativa, menos no último passo. No `Interactor.AvancarMinigame`, chamar `itensNaMao.Borrifar(entrada.Segurando)` quando o alvo é bromélia com sessão ativa, e `Borrifar(false)` ao encerrar.
- [ ] **Step 4:** Rodar `Minigames` e a suíte completa: `Failed: 0`.
- [ ] **Step 5:** No Editor: tratar uma bromélia com mouse e controle; conferir que toda pocinha é alcançável dentro do cone e que o jato aparece no bico.
- [ ] **Step 6:** Registrar e commitar ("Minigames: bromélia de mirar o borrifador").

### Task 6: Caixa d'água com tampa carregável

**Files:**
- Modify: `Assets/Scripts/KitDoAgente.cs`, `Assets/Scripts/CaixaDaguaInteractable.cs`, `Assets/Scripts/Interactable.cs` (`Verificar`), `Assets/Scripts/Interactor.cs`, `Assets/Scripts/FocoHUDInfo.cs`, e os leitores de `PneuCarregado` que na verdade perguntam "mãos ocupadas" (grep: `DengueHUD`, `KitHUD`, `FeedbackDeCarga`, `SomDoKit`, `Lixeira`, `PontoDeReabastecimento`, `CelularController`, `ItensNaMao`; cada um decide caso a caso, `Cacamba` continua só com pneu)
- Create: `Assets/Editor/Testes/TampaTestes.cs`

**Interfaces:**
- Produces em `KitDoAgente`:
  ```csharp
  public CaixaDaguaInteractable TampaCarregada { get; private set; }
  public bool MaosOcupadas => PneuCarregado != null || TampaCarregada != null;
  public bool PegarTampa(CaixaDaguaInteractable caixa);   // false com as mãos ocupadas
  public bool DevolverTampa();                             // volta à lateral da caixa
  internal void SoltarTampaColocada();                     // a caixa chama ao tampar
  public event Action<CaixaDaguaInteractable> OnTampaPega, OnTampaDevolvida, OnTampaColocada;
  ```
  Limites de movimento com a tampa: os mesmos do pneu (`multiplicadorCarregando`, sem pular, stamina de corrida).
- Produces em `Interactable`: `protected virtual bool AceitaComMaosOcupadas(KitDoAgente kit) => false;` e o `Verificar` troca `kit.PneuCarregado != null` por `kit.MaosOcupadas && !AceitaComMaosOcupadas(kit)`.
- Produces em `CaixaDaguaInteractable`: campo `segundosParaTampar = 0.8f`; `public bool TampaNosBracos { get; }`; `AvancarCom`: tampa encostada + Mãos → `kit.PegarTampa(this)` → `Pegou`; tampa nos braços deste kit → passo único que conclui (`Concluiu`), a tampa volta a ser filha da caixa e faz o arco do `MoveRoutine` até a boca; `SegundosPorPasso => TampaNosBracos ? segundosParaTampar : 0f`; `ProgressoCom` devolve só a fração; `PodeReencher` falso com a tampa nos braços; `SegundosEstimados => 1.5f`. Não depende de `requiredClicks`.

- [ ] **Step 1: Testes que falham** (kit com `ControladorJogador` e `PontoDeCarga`, caixa com filho `tampa`):
  - `PegarPoeATampaNosBracos`: `AvancarCom(kit) == Pegou`, `kit.TampaCarregada == caixa`, tampa filha do `PontoDeCarga`, `jogador.PodePular == false`.
  - `TamparConclui`: segundo `AvancarCom` → `Concluiu`, `caixa.IsComplete`, `kit.TampaCarregada == null`, tampa filha da caixa em `posFechada`, `jogador.PodePular == true`, `Fechamento == 1`.
  - `DevolverVoltaALateral`: `DevolverTampa()` → tampa filha da caixa em `posAberta`/`rotAberta`, `!caixa.IsComplete`, `caixa.CanInteract`.
  - `OutroFocoRecusaComATampa`: vaso → `Verificar == MaosOcupadas`.
  - `OutraCaixaRecusaComATampa`: segunda caixa → `MaosOcupadas`.
  - `PneuETampaNaoCoexistem`: com a tampa, `kit.Pegar(pneu) == false`; com o pneu, `PegarTampa == false`.
  - `NaoTrocaDeFerramentaComATampa`: `kit.Selecionar(Ferramenta.Areia) == false`.
  - `FerramentaErradaNaoPega`: areia selecionada na caixa → `FerramentaErrada`.
  - `ChuvaNaoReencheComATampaNosBracos`: caixa nascida seca, tampa nos braços → `!PodeReencher`.
  - `BloqueioDevolveATampa` (Review Focus 4): `interactor.Bloqueado = true` e `AoBloquear()` (Tarefa 3), que passa a chamar `kit.DevolverTampa()` → `kit.TampaCarregada == null`, tampa na lateral, `jogador.MultiplicadorVelocidade == 1`.
- [ ] **Step 2:** Rodar: não compila.
- [ ] **Step 3:** Implementar. No `Interactor`: troca de ferramenta só com `!kit.MaosOcupadas`; o botão de largar chama `DevolverTampa()` quando há tampa; em `RaycastTarget`, com tampa, o único foco não atravessado é `kit.TampaCarregada`; `AtualizarProgresso` usa `ProgressoCom`. `FluxoDeJogo` no fim de jogo já desliga o controle: conferir que passa pelo `Bloqueado` ou chamar `DevolverTampa()` ali.
- [ ] **Step 4:** `FocoHUDInfo`/`DengueHUD`: na caixa, "Pegar a tampa" com ela encostada e "Tampar a caixa" com ela nos braços.
- [ ] **Step 5:** Rodar `Minigames`, `ParteA`, `ParteB` e a suíte completa: `Failed: 0`. Atualizar os testes antigos da caixa que contavam cliques.
- [ ] **Step 6:** No Editor: pegar, andar, devolver, pegar de novo, tampar; chuva e celular com a tampa nos braços.
- [ ] **Step 7:** Registrar e commitar ("Minigames: caixa d'água com tampa carregável").

### Task 7: Verificação final

- [ ] **Step 1:** Suíte EditMode completa: `Failed: 0`, e o total subiu em relação ao número da Tarefa 0.
- [ ] **Step 2:** `Dengue > Simular Balanceamento` (ou o menu equivalente do `BalanceamentoSimulador`): comparar o tempo total estimado com o de antes e anotar a diferença no registro. Se passar de 15%, avisar o usuário antes de mexer em qualquer número de balanceamento.
- [ ] **Step 3:** Partida inteira no Editor com mouse; meia partida com controle. Conferir: ralo igual a antes; pneu, bromélia e caixa novos; vaso, garrafas e piscina ainda no segurar; nota e recorde aparecem no fim.
- [ ] **Step 4:** Registro final no `contexto do jogo.md` com os valores calibrados, e commit.
