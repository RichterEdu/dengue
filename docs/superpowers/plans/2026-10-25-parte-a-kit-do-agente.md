# Parte A — Kit do agente: plano de implementação

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** trocar o "clicar N vezes" por "segurar com a ferramenta certa", com kit de estoque apertado, pneu carregado até a caçamba, garrafas no saco e reabastecimento na picape da UPA.

**Architecture:** o "segurar" vira passos (`SessaoDeSegurar`). Cada passo chama o `Interact()` que já existe, então as animações, os sons e a rede social atuais continuam funcionando. As regras ficam em classes C# puras (`EstoqueDoKit`, `SessaoDeSegurar`) e nos métodos `Verificar`/`AvancarCom` do `Interactable`, todos testados em EditMode. A cena é montada por um builder de menu (`Dengue > Configurar Kit do Agente`).

**Tech Stack:** Unity 6000.5.6f1, URP, Input System 1.20, TextMeshPro, Unity Test Framework 1.7 (NUnit), `unity` CLI 1.0 conectado ao Editor aberto, Python 3 + numpy (sons).

**Spec:** `docs/superpowers/specs/2026-10-25-parte-a-kit-do-agente-design.md`

## Global Constraints

- Scripts de jogo em `Assets/Scripts/` (Assembly-CSharp, sem asmdef). Editor em `Assets/Editor/`. Testes em `Assets/Editor/Testes/`: o Assembly-CSharp-Editor já referencia `nunit.framework`, `UnityEditor.TestRunner` e `Assembly-CSharp`. Toda classe de teste leva `[Category("ParteA")]`.
- Estilo do projeto:
  - nomes em pt-BR;
  - cabeçalho `/* ==== Arquivo.cs ---- explicação ==== */` em todo script novo;
  - `[Tooltip]` em pt-BR em todo campo serializado;
  - a mesma densidade de comentários dos arquivos vizinhos.
- Enums serializados são salvos como número: **novos valores só no fim**, nunca reordenar (mesma regra do `TipoFoco`).
- Textos de post e HUD **sem emoji**, porque a fonte padrão do TextMeshPro não tem.
- Estoque padrão (ajustável no Inspector): **areia 1, tela 1, água sanitária 2 doses, saco de 6 garrafas**. Ao carregar pneu: **velocidade × 0,7**, sem correr nem pular.
- Tempos de segurar, todos entre 1,5 e 3 s: pneu 1,5; vaso 2,0; ralo 2,0; bromélia 2,0; piscina 2,5; caixa d'água 3,0; garrafas 0,4 s por garrafa.
- Nos testes EditMode, `Awake`/`Start` **não rodam**. Por isso:
  - o `KitDoAgente` cria o estoque e acha o `ControladorJogador` de forma preguiçosa, no primeiro acesso;
  - os testes ajustam campos serializados com `SerializedObject`.
- Objetos de teste nascem com `EditorUtility.CreateGameObjectWithHideFlags(nome, HideFlags.HideAndDontSave)` e são destruídos com `Object.DestroyImmediate` no `[TearDown]`, para não sujar a cena aberta. O helper fica em `Assets/Editor/Testes/TesteUtil.cs`, criado na Task 1:
  - `GameObject Novo(string nome)`;
  - `T Com<T>(string nome) where T : Component`;
  - `void Setar(Object alvo, string campo, object valor)`, que aceita int, float, bool, enum (como int), string e Object;
  - `void LimparTudo()`.
- Ciclo de compilação e teste (o Editor fica aberto e conectado):
  1. `unity command recompile`;
  2. repetir `unity command recompile_status` até `completed` ou `up_to_date`;
  3. `unity command console_status`, sem flag de falha de compilação;
  4. `unity command run_tests --mode EditMode --filter_type category --filter ParteA`.
  - "PASS" = `Failed: 0`, com o total esperado.
- **Nada de commit** sem pedido do usuário. Os passos de "checkpoint" só conferem `git status`.
- Ao final, tudo registrado no `contexto do jogo.md` (Task 14) e só a cena `MenuInicial` aberta no Editor (regra do `GuardaDeCenas`).
- Capturas: Canvas `ScreenSpaceOverlay` **não aparece** no `unity command screenshot`. Para conferir a HUD, troque temporariamente para `ScreenSpaceCamera` via `eval_file`, como na skill `preview-design`.

## Review Focus

1. **Varrer a mira de um foco para outro segurando o botão.** O tempo acumulado não pode passar para o foco novo. Teste: `SessaoDeSegurarTestes.TrocarDeAlvo_ZeraTempo` (Task 2).
2. **Abrir o celular ou a pausa no meio do segurar** (Interactor bloqueado ou `timeScale` 0). Nada avança, e ao voltar o passo recomeça do zero. Testes: `SessaoDeSegurarTestes.Soltar_ZeraFracao` e `DtZero_NaoAvanca` (Task 2).
3. **Ferramenta errada e sem carga ao mesmo tempo.** Deve aparecer a mensagem de ferramenta errada, que é a que ensina. Teste: `InteractableKitTestes.FerramentaErrada_TemPrioridadeSobreSemCarga` (Task 4).
4. **Largar o pneu sem chão embaixo** (beira de muro, borda do mapa). O pneu cai no pé do jogador, e não no vazio. Teste: `PneuCarregarTestes.Largar_SemChao_CaiNoPe` (Task 5).
5. **Usar a picape com o kit já cheio.** Recusa com "Seu kit já está cheio." e não dispara o post de reabastecimento. Teste: `PontosDeUsoTestes.Picape_KitCheio_Recusa` (Task 7).

---

### Task 1: Ferramentas e estoque do kit

**Files:**
- Create: `Assets/Scripts/Ferramenta.cs`
- Create: `Assets/Scripts/EstoqueDoKit.cs`
- Create: `Assets/Editor/Testes/TesteUtil.cs`
- Test: `Assets/Editor/Testes/EstoqueDoKitTestes.cs`

**Interfaces:**
- Produces:
  - `public enum Ferramenta { Maos, Areia, Tela, AguaSanitaria, SacoDeLixo }`. A ordem é a dos espaços 1–5.
  - `public enum ResultadoDoPasso { Avancou, Concluiu, Pegou, FerramentaErrada, SemCarga, SacoCheio, MaosOcupadas, Bloqueado }`
  - `public static class Ferramentas`:
    - `Ferramenta[] Todas`, na ordem do enum;
    - `string Nome(Ferramenta)`: "Mãos", "Areia", "Tela", "Água sanitária", "Saco de lixo";
    - `bool Consumivel(Ferramenta)`: true só para Areia, Tela e AguaSanitaria;
    - `bool EhRecusa(ResultadoDoPasso)`: true para FerramentaErrada, SemCarga, SacoCheio e MaosOcupadas.
  - `public class EstoqueDoKit`:
    - construtor `EstoqueDoKit(int areia, int tela, int aguaSanitaria, int capacidadeSaco)`; começa cheio, com o saco vazio;
    - `int Quantidade(Ferramenta)` e `int Capacidade(Ferramenta)`, que devolvem 0 para ferramenta não consumível;
    - `bool TemCarga(Ferramenta)` e `bool Consumir(Ferramenta)`: para não consumível, sempre true e nada muda;
    - `bool Reabastecer(IEnumerable<Ferramenta>)`: true se algo mudou;
    - `bool Cheio(IEnumerable<Ferramenta>)`;
    - o saco: `int GarrafasNoSaco`, `int CapacidadeDoSaco`, `bool SacoCheio`, `bool GuardarGarrafa()` (false se cheio) e `int EsvaziarSaco()` (devolve quantas tirou).

- [ ] **Step 1: Write the failing tests** — `EstoqueDoKitTestes`, usando `new EstoqueDoKit(1, 1, 2, 6)`:
  - `NovoEstoque_ComecaCheio`: Areia 1, Tela 1, AguaSanitaria 2, `GarrafasNoSaco` 0, `CapacidadeDoSaco` 6.
  - `Consumir_GastaUma_E_FalhaQuandoVazio`: `Consumir(Areia)` dá true, depois `Quantidade(Areia)` 0, `TemCarga(Areia)` false e `Consumir(Areia)` false.
  - `NaoConsumivel_SempreTemCarga`: para Maos e SacoDeLixo, `TemCarga` true, `Consumir` true e `Quantidade` 0.
  - `Reabastecer_SoOQueEstaNaLista`: consome Areia e Tela; `Reabastecer(new[]{Areia})` dá true; Areia 1 e Tela 0.
  - `Reabastecer_JaCheio_DevolveFalse`.
  - `Cheio_FicaFalsoDepoisDeConsumir`: `Cheio({Areia, Tela})` true; depois de `Consumir(Areia)`, false.
  - `Saco_EncheEsvazia`: 6× `GuardarGarrafa()` true, a 7ª false, `SacoCheio` true; `EsvaziarSaco()` devolve 6 e `GarrafasNoSaco` volta a 0.
  - `Ferramentas_NomesEConsumiveis`: `Nome(AguaSanitaria)` == "Água sanitária"; `Consumivel(Tela)` true; `Consumivel(SacoDeLixo)` false; `Todas.Length` 5.
- [ ] **Step 2:** Criar o `TesteUtil.cs` (Global Constraints) e rodar o ciclo. Esperado: falha de compilação (`EstoqueDoKit` não existe).
- [ ] **Step 3:** Implementar `Ferramenta.cs` e `EstoqueDoKit.cs`. O estoque guarda os valores em `Dictionary<Ferramenta, int>`.
- [ ] **Step 4:** Rodar o ciclo. Esperado: PASS, com 8 testes.
- [ ] **Step 5: Checkpoint** — `git status` mostra só os 4 arquivos novos e os `.meta`.

### Task 2: Sessão de segurar (tempo segurado → passos)

**Files:**
- Create: `Assets/Scripts/SessaoDeSegurar.cs`
- Test: `Assets/Editor/Testes/SessaoDeSegurarTestes.cs`

**Interfaces:**
- Produces: `public class SessaoDeSegurar` com:
  - `object Alvo { get; }`;
  - `float Fracao { get; }` (0..1 do passo atual);
  - `int Atualizar(object alvo, bool segurando, float dt, float segundosPorPasso)`;
  - `void Zerar()`.
- Regras do `Atualizar`:
  - alvo diferente do anterior, `alvo == null` ou `!segurando` zeram a sessão (sem segurar devolve 0);
  - `segundosPorPasso <= 0` devolve 1 uma vez por aperto e depois 0, até soltar ou trocar de alvo;
  - caso contrário, acumula `dt` e devolve `floor(acumulado / segundosPorPasso)`, guardando o resto.

- [ ] **Step 1: Write the failing tests:**
  - `MeioPasso_NaoAvanca`: `Atualizar(a, true, 0.25f, 0.5f)` devolve 0 e `Fracao` ≈ 0,5.
  - `PassoInteiro_AvancaUm`: dois `Atualizar(a, true, 0.25f, 0.5f)` somam 1.
  - `DtGrande_AvancaVarios`: `Atualizar(a, true, 1.1f, 0.5f)` devolve 2 e `Fracao` ≈ 0,2.
  - `TrocarDeAlvo_ZeraTempo` (Review Focus 1): 0,4 s em `a` e depois `Atualizar(b, true, 0.2f, 0.5f)` devolve 0, com `Fracao` ≈ 0,4.
  - `Soltar_ZeraFracao` (Review Focus 2): 0,4 s em `a`; `Atualizar(a, false, 0.1f, 0.5f)` deixa `Fracao` em 0; depois `Atualizar(a, true, 0.2f, 0.5f)` devolve 0.
  - `DtZero_NaoAvanca` (Review Focus 2): `Atualizar(a, true, 0f, 0.5f)` devolve 0.
  - `SegundosZero_UmPassoPorAperto`: o primeiro `Atualizar(a, true, 0.016f, 0f)` devolve 1, o segundo 0; depois de soltar, apertar de novo devolve 1.
- [ ] **Step 2:** Rodar o ciclo. Esperado: falha de compilação.
- [ ] **Step 3:** Implementar `SessaoDeSegurar`.
- [ ] **Step 4:** Rodar o ciclo. Esperado: PASS, com 15 testes.
- [ ] **Step 5: Checkpoint.**

### Task 3: KitDoAgente (estado do jogador) + limites de movimento

**Files:**
- Create: `Assets/Scripts/KitDoAgente.cs`
- Modify: `Assets/Scripts/ControladorJogador.cs` (bloco de andar/correr e pulo, linhas ~122–145)
- Test: `Assets/Editor/Testes/KitDoAgenteTestes.cs`

**Interfaces:**
- Consumes: `EstoqueDoKit`, `Ferramenta`, `Ferramentas` (Task 1).
- Produces: `public class KitDoAgente : MonoBehaviour`
  - Campos serializados:
    - `int cargaAreia = 1`, `int cargaTela = 1`, `int cargaAguaSanitaria = 2`, `int capacidadeSaco = 6`;
    - `float multiplicadorCarregando = 0.7f`;
    - `Transform pontoDeCarga`;
    - `ControladorJogador jogador` (vazio = `GetComponent`, preguiçoso).
  - Propriedades:
    - `EstoqueDoKit Estoque` (preguiçoso, criado com os campos);
    - `Ferramenta Selecionada` (padrão `Maos`);
    - `PneuInteractable PneuCarregado`;
    - `Transform PontoDeCarga { get; set; }`.
  - Eventos:
    - `Action OnMudou`, para qualquer mudança;
    - `Action<Ferramenta> OnFerramentaTrocada`;
    - `Action OnReabasteceu` e `Action OnSacoEsvaziado`;
    - `Action<PneuInteractable> OnPneuPego`, `OnPneuLargado` e `OnPneuEntregue`.
  - Métodos:
    - `bool Selecionar(Ferramenta)`: false se carregando pneu ou se já era a selecionada;
    - `void Proxima()` e `void Anterior()`: dão a volta na lista;
    - `bool Consumir(Ferramenta)`, `bool Reabastecer(IReadOnlyList<Ferramenta>)`, `bool GuardarGarrafa()` e `int EsvaziarSaco()`: delegam ao estoque e disparam `OnMudou` (e `OnReabasteceu` / `OnSacoEsvaziado` só quando algo mudou ou saiu).
  - Os métodos de pneu entram na Task 5.
- Produces (`ControladorJogador`):
  - `public float MultiplicadorVelocidade { get; set; } = 1f`, que multiplica `velMax`;
  - `public bool PodeCorrerEPular { get; set; } = true`: com false, `Correndo` fica false e o pulo é ignorado.

- [ ] **Step 1: Write the failing tests:**
  - `Estoque_SemAwake_UsaCargasPadrao`: Areia 1, AguaSanitaria 2, `CapacidadeDoSaco` 6.
  - `Estoque_UsaCampoDoInspector`: `Setar(kit, "cargaTela", 3)` antes do primeiro acesso faz `Capacidade(Tela)` dar 3.
  - `Selecionar_TrocaEAvisa`: `Selecionar(Areia)` dá true, `OnFerramentaTrocada` recebe Areia 1×, `OnMudou` ≥ 1; selecionar Areia de novo dá false.
  - `Proxima_DaVolta`: a partir de SacoDeLixo, `Proxima()` vai para Maos; a partir de Maos, `Anterior()` vai para SacoDeLixo.
  - `Reabastecer_SoAvisaQuandoMuda`: com o kit cheio, `Reabastecer({Areia})` dá false e `OnReabasteceu` 0×; depois de `Consumir(Areia)`, dá true e 1×.
  - `EsvaziarSaco_SoAvisaComGarrafa`: saco vazio dá 0 e o evento 0×; depois de 2 garrafas, dá 2 e o evento 1×.
  - `Controlador_PadraoSemLimite`: um `ControladorJogador` novo tem `MultiplicadorVelocidade` 1 e `PodeCorrerEPular` true.
- [ ] **Step 2:** Rodar o ciclo. Esperado: falha de compilação.
- [ ] **Step 3:** Implementar `KitDoAgente` e as duas propriedades do `ControladorJogador`.
- [ ] **Step 4:** Rodar o ciclo. Esperado: PASS, com 22 testes.
- [ ] **Step 5: Checkpoint.**

### Task 4: Interactable avança com o kit

**Files:**
- Modify: `Assets/Scripts/Interactable.cs` (campos ~30–79, `Interact()` 88–119; o cabeçalho ganha a explicação do novo fluxo)
- Test: `Assets/Editor/Testes/InteractableKitTestes.cs`

**Interfaces:**
- Consumes: `KitDoAgente` (Task 3), `Ferramenta`, `ResultadoDoPasso` e `Ferramentas` (Task 1).
- Produces (em `Interactable`):
  - Campos serializados `protected Ferramenta ferramentaNecessaria = Ferramenta.Maos` e `protected float segundosParaConcluir = 2f`.
  - `public Ferramenta FerramentaNecessaria`.
  - `public virtual float SegundosPorPasso`, que vale `segundosParaConcluir / max(1, requiredClicks)`.
  - `public virtual bool CanInteract`, que vale `!concluido && currentClicks < requiredClicks`.
  - `public bool IsComplete`, que vale `concluido`. **É a flag**, não mais a conta de cliques.
  - `public virtual bool Zumbe`, que vale `CanInteract`.
  - `public virtual bool AceitaFerramenta(Ferramenta f)`, que vale `f == ferramentaNecessaria`.
  - `public virtual ResultadoDoPasso Verificar(KitDoAgente kit)`. Confere sem avançar, nesta ordem:
    1. `!CanInteract` → Bloqueado;
    2. pneu nas mãos → MaosOcupadas;
    3. ferramenta não aceita → FerramentaErrada;
    4. `!kit.Estoque.TemCarga` → SemCarga;
    5. `VerificarExtra(kit)`.
    - Com `kit == null`, a resposta é sempre Avancou ou Bloqueado.
  - `protected virtual ResultadoDoPasso VerificarExtra(KitDoAgente kit)`, que por padrão devolve Avancou.
  - `public virtual ResultadoDoPasso AvancarCom(KitDoAgente kit)`: chama `Verificar`; se der Avancou, dá um passo. Se o foco concluiu, chama `kit.Consumir(FerramentaNecessaria)` e devolve Concluiu; senão, devolve Avancou.
  - `public void Interact()`: o caminho antigo, um passo sem kit. Continua público.
  - `protected virtual void AoAvancar(KitDoAgente kit)`: chamado em cada passo, logo depois do `OnInteract()`.
  - `protected virtual bool ConcluiSozinho`, padrão true.
  - `protected virtual void AoTerminarAcao(KitDoAgente kit)`: chamado no último passo quando `ConcluiSozinho` é false.
  - `protected void Concluir()`: só age uma vez. Seta a flag, para o punch e dispara `OnCompleted` e depois `OnComplete()`.
- O passo interno recebe `KitDoAgente` (pode ser null) e faz, em ordem:
  1. `currentClicks++` e a escala por clique;
  2. `OnInteract()`, `AoAvancar(kit)` e `OnClicado`;
  3. no último clique, `Concluir()` ou `AoTerminarAcao(kit)`, conforme `ConcluiSozinho`; nos outros, o punch.

- [ ] **Step 1: Write the failing tests.** O helper do teste cria um `Interactable` e ajusta, via `Setar`:
  - `requiredClicks` 2, `segundosParaConcluir` 1, `shrinkDuration` 0;
  - `ferramentaNecessaria`, conforme o caso.

  Testes:
  - `FerramentaCerta_AvancaEConclui`: foco de Areia, `kit.Selecionar(Areia)`. O primeiro `AvancarCom` dá Avancou, com `CurrentClicks` 1 e `IsComplete` false. O segundo dá Concluiu, com `IsComplete` true e `Quantidade(Areia)` 0.
  - `CargaSoEGastaAoConcluir`: depois do primeiro passo, `Quantidade(Areia)` continua 1.
  - `FerramentaErrada_NaoAvanca`: com Maos num foco de Areia, dá FerramentaErrada e `CurrentClicks` 0.
  - `SemCarga_NaoAvanca`: `kit.Consumir(Areia)` e depois `AvancarCom` dá SemCarga.
  - `FerramentaErrada_TemPrioridadeSobreSemCarga` (Review Focus 3): foco de Tela; Areia consumida e selecionada. Dá FerramentaErrada.
  - `Verificar_NaoMexeNoFoco`: `Verificar` dá Avancou e `CurrentClicks` continua 0.
  - `OnCompleted_DisparaUmaVez`: conta 1 depois de concluir; um terceiro `AvancarCom` dá Bloqueado.
  - `Interact_SemKit_ContinuaFuncionando`: dois `Interact()` deixam `IsComplete` true.
  - `SegundosPorPasso_DivideEntreOsCliques`: `segundosParaConcluir` 2 e `requiredClicks` 4 dão 0,5.
- [ ] **Step 2:** Rodar o ciclo. Esperado: falhas.
- [ ] **Step 3:** Implementar no `Interactable.cs`. Os derivados não mudam nesta task.
- [ ] **Step 4:** Rodar o ciclo. Esperado: PASS, com 31 testes.
- [ ] **Step 5:** `grep -n "IsComplete\|CanInteract" Assets/Scripts/*.cs` e conferir que nenhum uso antigo dependia de "cliques ≥ total". Os usos são os de `FluxoDeJogo`, `DengueHUD`, `FocoListaHUD`, `RedeSocial`, `HistoriaRedeSocial`, `SomDosFocos`, `FocoProximityGlow` e `Interactor`. Checkpoint.

### Task 5: Pneu carregado até a caçamba

**Files:**
- Modify: `Assets/Scripts/PneuInteractable.cs` (arquivo inteiro, 28 linhas)
- Modify: `Assets/Scripts/KitDoAgente.cs` (métodos de pneu)
- Modify: `Assets/Scripts/SomDosFocos.cs:151` (pular foco com `!Zumbe`)
- Test: `Assets/Editor/Testes/PneuCarregarTestes.cs`

**Interfaces:**
- Consumes: `Interactable.AoTerminarAcao`, `ConcluiSozinho`, `Concluir` e `Zumbe` (Task 4); `KitDoAgente` (Task 3).
- Produces (`PneuInteractable`):
  - `bool Esvaziado` (true depois do último passo) e `bool Carregado`;
  - `CanInteract` = `!IsComplete && !Carregado`, e, se ainda não esvaziado, também a regra da base;
  - `Zumbe` = `!Esvaziado`;
  - `SegundosPorPasso` = 0 quando já esvaziado: pegar de novo é instantâneo;
  - `ConcluiSozinho` = false;
  - `AoTerminarAcao(kit)`: esconde a água e chama `kit?.Pegar(this)`;
  - `AvancarCom(kit)`:
    - já esvaziado: confere `Verificar`; se der Avancou, chama `kit.Pegar(this)` e devolve Pegou;
    - senão: chama a base e devolve Pegou quando terminou carregado;
  - `internal void PrenderEm(Transform ponto)`: guarda pai, posição e rotação originais e a altura do pivô ao chão (raycast para baixo; sem chão, 0). Desliga os colliders e vira filho de `ponto` com pose local zero;
  - `internal void Largar(Vector3 pes, Vector3 frente)`:
    - solta do pai e religa os colliders;
    - raycast de `pes + frente*0.8 + up*1.5` para baixo, até 4 m;
    - com chão: fica em `hit.point + up*alturaDoPivo`; sem chão: fica em `pes`;
    - volta a rotação original;
  - `internal void Entregar(Vector3 destino)`: solta, vai para `destino` e chama `Concluir()` (o encolher padrão roda ali).
- Produces (`KitDoAgente`):
  - `bool Pegar(PneuInteractable p)`: false se já carrega um. Aplica `MultiplicadorVelocidade = multiplicadorCarregando` e `PodeCorrerEPular = false`, e dispara `OnPneuPego` e `OnMudou`;
  - `bool LargarPneu(Vector3 pes, Vector3 frente)`;
  - `bool EntregarPneu(Vector3 destino)`.
  - Os dois últimos devolvem false sem pneu. Ambos restauram 1 e true no movimento e disparam `OnPneuLargado` / `OnPneuEntregue` e `OnMudou`.

- [ ] **Step 1: Write the failing tests.** O pneu tem `requiredClicks` 2 e `shrinkDuration` 0. O kit fica num objeto com `ControladorJogador`, com `PontoDeCarga` = um Transform de teste.
  - `TerminarAcao_PegaENaoConclui`: Maos. O primeiro `AvancarCom` dá Avancou; o segundo dá Pegou. Fica `kit.PneuCarregado == pneu`, `IsComplete` false, `CanInteract` false, `transform.parent == ponto`, `MultiplicadorVelocidade` 0,7 e `PodeCorrerEPular` false.
  - `Largar_SemChao_CaiNoPe` (Review Focus 4): sem colliders na cena de teste, `kit.LargarPneu(new Vector3(5,0,5), Vector3.forward)` deixa a posição ≈ (5,0,5), `parent` null, `CanInteract` true, `PneuCarregado` null e o movimento restaurado.
  - `PegarDeNovo_EInstantaneo`: depois de largar, `SegundosPorPasso` 0 e `AvancarCom` dá Pegou.
  - `Entregar_Conclui`: `kit.EntregarPneu(Vector3.zero)` deixa `IsComplete` true, `OnCompleted` 1× e `PneuCarregado` null.
  - `ComPneu_OutroFoco_MaosOcupadas`: um `Interactable` de Maos dá MaosOcupadas.
  - `ComPneu_NaoTrocaFerramenta`: `Selecionar(Areia)` dá false.
  - `Esvaziado_NaoZumbe`: depois de pegar, `Zumbe` false.
- [ ] **Step 2:** Rodar o ciclo. Esperado: falhas.
- [ ] **Step 3:** Implementar o pneu e os métodos do kit. Em `SomDosFocos.Update`, acrescentar `|| !z.foco.Zumbe` ao `continue` da primeira varredura.
- [ ] **Step 4:** Rodar o ciclo. Esperado: PASS, com 38 testes.
- [ ] **Step 5: Checkpoint.**

### Task 6: Garrafas no saco

**Files:**
- Modify: `Assets/Scripts/GarrafaInteractable.cs` (campos ~41–58; `OnStart`; novo `VerificarExtra`/`AoAvancar`)
- Test: `Assets/Editor/Testes/GarrafaSacoTestes.cs`

**Interfaces:**
- Consumes: `VerificarExtra`, `AoAvancar` (Task 4); `KitDoAgente.GuardarGarrafa`, `Estoque.SacoCheio` (Tasks 1/3).
- Produces (`GarrafaInteractable`):
  - campo `float segundosPorGarrafa = 0.4f`, com `SegundosPorPasso` = `segundosPorGarrafa`;
  - `VerificarExtra` devolve SacoCheio quando `kit != null && kit.Estoque.SacoCheio`;
  - `AoAvancar(kit)` chama `kit?.GuardarGarrafa()`;
  - `public int ContarGarrafas()`: o tamanho da lista ou `BuscarGarrafas().Length` (para o simulador da Task 13).

- [ ] **Step 1: Write the failing tests:**
  - `SacoCheio_Bloqueia_E_CacambaLibera`: kit com `capacidadeSaco` 2; garrafa com `requiredClicks` 3 e `ferramentaNecessaria` SacoDeLixo; `Selecionar(SacoDeLixo)`. Dois `AvancarCom` dão Avancou; o terceiro dá SacoCheio, com `GarrafasNoSaco` 2. Depois de `kit.EsvaziarSaco()`, `AvancarCom` dá Concluiu.
  - `SegundosPorPasso_EhPorGarrafa`: dá 0,4.
  - `SemSaco_FerramentaErrada`: com Maos, dá FerramentaErrada.
- [ ] **Step 2:** Rodar o ciclo. Esperado: falhas.
- [ ] **Step 3:** Implementar.
- [ ] **Step 4:** Rodar o ciclo. Esperado: PASS, com 41 testes.
- [ ] **Step 5: Checkpoint.**

### Task 7: Pontos de uso — picape, monte de areia e caçamba

**Files:**
- Create: `Assets/Scripts/PontoDeUso.cs`
- Create: `Assets/Scripts/PontoDeReabastecimento.cs`
- Create: `Assets/Scripts/Cacamba.cs`
- Test: `Assets/Editor/Testes/PontosDeUsoTestes.cs`

**Interfaces:**
- Consumes: `KitDoAgente` (Tasks 3 e 5).
- Produces:
  - `public abstract class PontoDeUso : MonoBehaviour`:
    - campos serializados `[TextArea] protected string textoAoMirar` e `protected float segundosParaUsar = 1f`;
    - `string TextoAoMirar` e `float SegundosParaUsar`;
    - `abstract bool PodeUsar(KitDoAgente kit, out string motivo)` e `abstract void Usar(KitDoAgente kit)`.
  - `public class PontoDeReabastecimento : PontoDeUso`:
    - `List<Ferramenta> recarrega`, padrão `{ Areia, Tela, AguaSanitaria }`;
    - motivos de recusa, nesta ordem: com pneu, "Largue o pneu antes."; com tudo cheio, "Seu kit já está cheio.";
    - `Usar` chama `kit.Reabastecer(recarrega)`.
  - `public class Cacamba : PontoDeUso`:
    - campo `float alturaDescarte = 1.2f`;
    - pode usar com pneu nas mãos ou com garrafa no saco; senão, recusa com "Nada para jogar na caçamba.";
    - `Usar`: com pneu, `kit.EntregarPneu(transform.position + Vector3.up * alturaDescarte)`; senão, `kit.EsvaziarSaco()`.

- [ ] **Step 1: Write the failing tests:**
  - `PontoDeUso_NaoEhFoco`: `typeof(Interactable).IsAssignableFrom(typeof(PontoDeUso))` dá false.
  - `Picape_RecarregaSoOQueOferece`: `recarrega = {Areia}`; consome Areia e Tela; `PodeUsar` dá true; depois de `Usar`, Areia 1 e Tela 0.
  - `Picape_KitCheio_Recusa` (Review Focus 5): `PodeUsar` dá false com motivo "Seu kit já está cheio." e `OnReabasteceu` 0×.
  - `Picape_ComPneu_Recusa`: motivo "Largue o pneu antes."
  - `Cacamba_EsvaziaSaco`: com 3 garrafas, `Usar` deixa `GarrafasNoSaco` 0.
  - `Cacamba_EntregaPneu`: pneu nas mãos; `Usar` deixa `pneu.IsComplete` true.
  - `Cacamba_Vazia_Recusa`: motivo "Nada para jogar na caçamba."
- [ ] **Step 2:** Rodar o ciclo. Esperado: falha de compilação.
- [ ] **Step 3:** Implementar os três arquivos.
- [ ] **Step 4:** Rodar o ciclo. Esperado: PASS, com 48 testes.
- [ ] **Step 5: Checkpoint.**

### Task 8: Entrada e Interactor — segurar, trocar ferramenta e largar

**Files:**
- Modify: `Assets/InputSystem_Actions.inputactions` (mapa Player)
- Modify: `Assets/Scripts/EntradaJogador.cs` (propriedades novas + `OnEnable`)
- Modify: `Assets/Scripts/Interactor.cs` (cabeçalho, `Update`, `RaycastTarget`, `ApplyHighlight`)

**Interfaces:**
- Consumes: `SessaoDeSegurar` (Task 2); `Interactable.Verificar`, `AvancarCom` e `SegundosPorPasso` (Tasks 4–6); `PontoDeUso` (Task 7); `KitDoAgente` (Tasks 3 e 5).
- Mudanças no `.inputactions` (via script Python com `json`, `encoding="utf-8-sig"`, `indent=4`; ids novos com `uuid4`):
  - tirar `<Keyboard>/1` de `Previous` e `<Keyboard>/2` de `Next`, porque as teclas 1–5 passam a escolher a ferramenta;
  - pôr `<Gamepad>/leftShoulder` em `Previous` e `<Gamepad>/rightShoulder` em `Next`, com grupo `;Gamepad`;
  - criar a ação `Largar` (Button), ligada a `<Mouse>/rightButton` e `<Keyboard>/g` (grupo `;Keyboard&Mouse`) e a `<Gamepad>/buttonNorth` (grupo `;Gamepad`).
  - Botões que já têm dono e não podem ser usados: buttonEast é o voltar da pausa, Start é a pausa, T é o celular.
- Produces (`EntradaJogador`), todas respeitando `Habilitado`:
  - `bool InteragirSegurado` (`acaoInteragir.IsPressed()`);
  - `bool LargarPressionado` (`Player/Largar`);
  - `int FerramentaPedida`: 0..4 para teclas 1–5 (ou numpad 1–5) apertadas neste frame; senão −1;
  - `int TrocaDeFerramenta`: +1 com `Player/Next` ou roda do mouse para baixo; −1 com `Player/Previous` ou roda para cima; senão 0.
- Produces (`Interactor`):
  - campo `[SerializeField] KitDoAgente kit` (vazio = `GetComponent`);
  - `PontoDeUso PontoAlvo`, `float ProgressoContinuo` (0..1) e `bool Avancando` (segurando com passo válido neste frame);
  - eventos `Action<Interactable, ResultadoDoPasso> OnRecusado` e `Action<PontoDeUso, string> OnPontoRecusado`. Cada um sai **uma vez por aperto**.
- Regras do `Update`, em ordem:
  1. **Bloqueado:** desfaz o destaque, `sessao.Zerar()`, retorna.
  2. **Sem pneu nas mãos:** `FerramentaPedida ≥ 0` chama `kit.Selecionar(Ferramentas.Todas[i])`; `TrocaDeFerramenta` chama `Proxima`/`Anterior`.
  3. **Largar:** `LargarPressionado` com pneu chama `kit.LargarPneu(transform.position, transform.forward)`.
  4. **Raycast:** acha `Interactable` (com `CanInteract` e sem pneu nas mãos) ou `PontoDeUso`, via `GetComponentInParent`. O destaque vale para os dois; `ApplyHighlight` passa a receber `Component`.
  5. **No aperto (ou ao trocar de alvo segurando):** chama `Verificar` / `PodeUsar` uma vez. Se for recusa, dispara o evento e não acumula até soltar.
  6. **Segurando:** `passos = sessao.Atualizar(alvo, segurando && liberado, Time.deltaTime, alvo.SegundosPorPasso | ponto.SegundosParaUsar)`. Para cada passo, chama `AvancarCom` / `Usar`; recusa vira evento; Concluiu ou Pegou zera a sessão.
  7. **Progresso:** `ProgressoContinuo` = `(CurrentClicks + sessao.Fracao) / RequiredClicks` no foco, e `sessao.Fracao` no ponto.

- [ ] **Step 1:** Editar o `.inputactions` com o script e conferir com o mesmo `python -I -c` usado na exploração. A listagem tem que mostrar `Largar` com as 3 ligações, os ombros e nenhuma `<Keyboard>/1` ou `/2`.
- [ ] **Step 2:** Implementar `EntradaJogador` e `Interactor`.
- [ ] **Step 3:** Rodar o ciclo. Esperado: compila, sem avisos novos de `[EntradaJogador]`, e PASS com 48 testes.
- [ ] **Step 4:** Verificação em Play Mode (`eval_file` no scratchpad; `Application.runInBackground = true` só no teste; `ControleAtivo = false`): `ControladorJogador.Teleportar` + `DefinirOlhar` para mirar um vaso a 1,5 m. Esperado: `interactor.CurrentTarget` é o vaso e `PontoAlvo` é null.
  - O aperto real do botão não dá para simular, porque o Editor sem foco descarta a entrada. A lógica de segurar já está coberta por `SessaoDeSegurarTestes` e `InteractableKitTestes`, e o resto fica para a partida do usuário (Task 14).
- [ ] **Step 5: Checkpoint.**

### Task 9: HUD — painel do foco, recusas e barra de ferramentas

**Files:**
- Modify: `Assets/Scripts/FocoHUDInfo.cs` (campo novo)
- Modify: `Assets/Scripts/DengueHUD.cs` (`UpdateTargetPanel`, `Progress`, `Start`/`OnDestroy` para assinar o Interactor, `textoAoMirar` padrão)
- Create: `Assets/Scripts/KitHUD.cs`
- Test: `Assets/Editor/Testes/KitHUDTestes.cs`

**Interfaces:**
- Consumes: `Interactor.PontoAlvo`, `ProgressoContinuo`, `OnRecusado` e `OnPontoRecusado` (Task 8); eventos do `KitDoAgente` (Tasks 3 e 5); `FluxoDeJogo.PartidaIniciada`.
- Produces:
  - `FocoHUDInfo.textoFerramentaErrada` (`[TextArea(1,3)] public string`).
  - `DengueHUD`:
    - campo `linhaSegurar = "<size=70%><color=#FFFFFFB4>segure o clique</color></size>"`, acrescentado embaixo do texto de mirar;
    - `textoAoMirar` padrão = "{nome}";
    - a barra usa `interactor.ProgressoContinuo`;
    - com `PontoAlvo`, o painel mostra `TextoAoMirar` e a barra, sem "3/5".
  - Mensagens de recusa (toast, `comDica: true`):
    - FerramentaErrada: `info.textoFerramentaErrada`; se estiver vazio, `"Isso não resolve aqui. Tente: <b>{Nome(ferramentaNecessaria)}</b>."`;
    - SemCarga: `"Acabou: <b>{Nome(selecionada)}</b>. Reabasteça na picape da UPA."`;
    - SacoCheio: `"Saco cheio. Esvazie na <b>caçamba</b> do terreno baldio."`;
    - MaosOcupadas: `"Mãos ocupadas: leve o pneu à caçamba ou largue [botão direito]."`;
    - `OnPontoRecusado`: o próprio motivo, num toast sem dica.
  - `public class KitHUD : MonoBehaviour`. Monta o próprio Canvas no `Awake` (`ScreenSpaceOverlay`, `sortingOrder` 40, `CanvasScaler` 1920×1080, TextMeshProUGUI):
    - **barra:** 5 espaços de 96×96, 10 px de distância, centralizados a 40 px da borda de baixo, fundo `#12171CF5` com contorno `#FFFFFF1A`;
    - **cada espaço:** número no canto, nome e quantidade de `TextoQuantidade`;
    - **selecionado:** contorno `#40FFCC` e escala 1,1;
    - **consumível zerado:** quantidade em `#E04848`;
    - **linha de pneu:** com pneu, acima da barra, "Carregando pneu: leve à caçamba  ·  [botão direito] larga";
    - **dica:** "Teclas <b>1–5</b> trocam a ferramenta", com o visual do `DicaCorrer`, aparece quando `PartidaIniciada` e some com fade na primeira `OnFerramentaTrocada`;
    - só aparece com `PartidaIniciada`.
  - `public static string TextoQuantidade(EstoqueDoKit e, Ferramenta f)`: consumível dá `"{q}/{cap}"`; SacoDeLixo dá `"{garrafas}/{capSaco}"`; Maos dá `""`.

- [ ] **Step 1: Write the failing test** — `KitHUDTestes.TextoQuantidade_PorFerramenta`. Com `new EstoqueDoKit(1,1,2,6)` e uma garrafa guardada:
  - Areia dá "1/1";
  - AguaSanitaria dá "2/2";
  - SacoDeLixo dá "1/6";
  - Maos dá "".
- [ ] **Step 2:** Rodar o ciclo. Esperado: falha de compilação.
- [ ] **Step 3:** Implementar `FocoHUDInfo`, `DengueHUD` e `KitHUD`.
- [ ] **Step 4:** Rodar o ciclo. Esperado: PASS, com 49 testes.
- [ ] **Step 5: Checkpoint.** O visual é conferido na Task 12, junto com a montagem.

### Task 10: Rede social — 4 posts novos

**Files:**
- Modify: `Assets/Scripts/HistoriaRedeSocial.cs`:
  - `PersonagemDoBairro` ganha um campo;
  - campos de "Momentos" e referências;
  - `Awake`/`Start`/`OnDestroy`;
  - handlers novos;
  - `CompletarPersonagens`;
  - padrões.
- Test: `Assets/Editor/Testes/HistoriaKitTestes.cs`

**Interfaces:**
- Consumes: `Interactor.OnRecusado` (Task 8); `KitDoAgente.OnReabasteceu`, `OnPneuPego` e `OnSacoEsvaziado`.
- Produces:
  - `PersonagemDoBairro.aoUsarFerramentaErrada` (`[TextArea(2,4)] string`).
  - Listas `aoReabastecerPrimeiraVez`, `aoCarregarPneu` e `aoEsvaziarSacoPrimeiraVez` (`List<PostRedeSocial>`, preenchidas em `PreencherPadroes`).
  - Referências `[SerializeField] Interactor interactor` e `KitDoAgente kit`, achadas no `Awake`.
  - `CompletarPersonagens()` também preenche `aoUsarFerramentaErrada` vazio com o padrão do mesmo `tipo`. A cena já tem personagens salvos sem esse campo.
  - Cada post sai **uma vez**: a flag só marca quando `Postar` devolve true, seguindo a regra do projeto (recusado volta no próximo gatilho). Só com `EmJogo`. Prioridade Normal.
  - Ferramenta errada: um post por tipo (`HashSet<TipoFoco>`), do dono de `TipoDoFoco(foco)`, com humor Neutro.
- Textos padrão (exatos, sem emoji):
  - `aoUsarFerramentaErrada` por tipo:
    - Pneu: "Pneu não adianta só esvaziar: tem que levar embora. Pega com as mãos e leva pra caçamba!"
    - Vaso: "Pratinho de vaso é com areia, viu? Até a borda. O agente tá aprendendo!"
    - CaixaDagua: "Caixa d'água se resolve com a tampa, e a tampa tá ali do lado! É só encaixar."
    - Piscina: "A piscininha é só esvaziar e virar de boca pra baixo. Com as mãos mesmo!"
    - Garrafas: "Garrafa vai pro saco de lixo e depois pra caçamba. Nada de deixar pelo chão!"
    - Ralo: "Vi o agente querendo resolver o ralo com outra coisa. Ralo é com tela, moço!"
    - Bromelia: "Calma, agente! Bromélia não precisa arrancar nem tampar: água sanitária diluída no copinho e pronto."
  - `aoReabastecerPrimeiraVez`: UPA Vila Nova, @upa.vilanova, Positivo — "Nosso agente passou na picape da UPA pra pegar material. Areia, tela e água sanitária a caminho dos quintais!"
  - `aoCarregarPneu`: Carlos Mendes, @carlinhos.m, Positivo — "Olha o agente carregando pneu pela rua! Finalmente alguém tirando esse entulho do terreno."
  - `aoEsvaziarSacoPrimeiraVez`: Bruna Lima, @bru.lima, Positivo — "Primeiro saco de garrafas na caçamba! Lixo no lugar certo não junta água."

- [ ] **Step 1: Write the failing test** — `CompletarPersonagens_PreencheFerramentaErradaVazia`. Uma `HistoriaRedeSocial` com um personagem tipo Ralo (via `Setar` na lista `personagens`) e `aoUsarFerramentaErrada` vazio; depois de `CompletarPersonagens()`, o texto começa com "Vi o agente querendo resolver o ralo".
- [ ] **Step 2:** Rodar o ciclo. Esperado: falha.
- [ ] **Step 3:** Implementar.
- [ ] **Step 4:** Rodar o ciclo. Esperado: PASS, com 50 testes.
- [ ] **Step 5: Checkpoint.** Os posts são conferidos em Play na Task 14.

### Task 11: Sons do kit

**Files:**
- Modify: `Ferramentas/Audio/gerar_sons.py` (receitas `s_kit_*` + entradas no `SONS`)
- Create: `Assets/Scripts/SomDoKit.cs`
- Modify: `Assets/Editor/SomBuilder.cs` (novo `public static void LigarKit(SomDoKit c)`)
- Create: `Assets/Meus/Assets Gerados/Sons/` com 7 WAVs gerados

**Interfaces:**
- Consumes: eventos do `KitDoAgente`; `Interactor.OnRecusado`, `OnPontoRecusado` e `Avancando`; `CanalDeAudio.Criar(Transform, string, CanalSom, bool)`.
- Produces:
  - sons, todos 2D, 44,1 kHz mono, com o pico do padrão `efeito()` do script:
    - `kit_trocar`: clique de plástico, ~80 ms;
    - `kit_erro`: dois bipes graves descendentes, ~250 ms;
    - `kit_reabastecer`: zíper + 3 batidas de plástico, ~600 ms;
    - `cacamba_despejar`: metal oco + baque, ~700 ms;
    - `pneu_pegar`: borracha abafada, ~250 ms;
    - `pneu_largar`: baque de borracha + quique, ~400 ms;
    - `kit_segurar_loop`: farfalhar suave, loop circular de 2 s.
  - `SomDoKit : MonoBehaviour`:
    - campos `AudioClip trocar, erro, reabastecer, despejar, pneuPegar, pneuLargar, segurarLoop`;
    - volumes `[Range(0,1)]`: 0,5 nos curtos e 0,25 no loop;
    - dois `CanalDeAudio` 2D, um curto e um loop;
    - o loop entra e sai com fade de 0,15 s conforme `interactor.Avancando`;
    - `despejar` toca em `OnSacoEsvaziado` e `OnPneuEntregue`.
  - `SomBuilder.LigarKit(SomDoKit c)` liga os clips por nome, com os helpers `Clip`/`Clips` que já existem.

- [ ] **Step 1:** Escrever as 7 receitas e gerar:
  ```bash
  python -X utf8 Ferramentas/Audio/gerar_sons.py "Assets/Meus/Assets Gerados/Sons" --so kit_trocar,kit_erro,kit_reabastecer,cacamba_despejar,pneu_pegar,pneu_largar,kit_segurar_loop
  ```
  Esperado: 7 `.wav` novos. Os existentes não mudam: `git status` só mostra arquivos novos em `Sons/`.
- [ ] **Step 2:** Implementar `SomDoKit` e `SomBuilder.LigarKit`. Rodar o ciclo. Esperado: compila e PASS com 50 testes.
- [ ] **Step 3:** No Editor, `unity command recompile` e `AssetDatabase.Refresh` via `eval_file`. Conferir no import que `kit_segurar_loop` saiu como Vorbis/Compressed In Memory e os outros como PCM, conforme a regra do `ImportadorDeSons`.
- [ ] **Step 4: Checkpoint.**

### Task 12: Montagem da cena — `Dengue > Configurar Kit do Agente`

**Files:**
- Create: `Assets/Editor/KitDoAgenteBuilder.cs`
- Modify: `Assets/Editor/PersonagensHospitalBuilder.cs:90-91` (`FalaEnfermeira` vira `public const` com o texto novo)
- Create: `Assets/Meus/Assets Gerados/Cacamba/Cacamba.mat`
- Modify: `Assets/Scenes/SampleScene.unity` (rodando o builder), os prefabs de foco listados abaixo e `Assets/Meus/Vaso de Flor/Vaso de Flor.prefab`

**Interfaces:**
- Consumes: tudo das Tasks 1–11.
- Produces: `[MenuItem("Dengue/Configurar Kit do Agente")] public static void Configurar()`. Pode rodar de novo: acha pelo nome o que já criou e só atualiza. Loga um resumo `[Kit]` no Console. O que ele faz:
  1. **Player** (o objeto com `ControladorJogador`): garante `KitDoAgente` e `SomDoKit` (com `SomBuilder.LigarKit`). Cria `PontoDeCarga` como filho da `Main Camera`, com pose local inicial (0,30; −0,55; 0,85) e rotação (0, 0, 75), e liga `kit.pontoDeCarga`.
  2. **HUD:** garante um objeto `KitHUD`, irmão do objeto da `DengueHUD`.
  3. **Focos:** uma tabela por tipo C#, aplicada **em cada prefab** e **em cada instância da cena** (`SerializedObject`; cria `FocoHUDInfo` se faltar). Os prefabs são `Assets/Meus/Vaso de Flor/Vaso de Flor.prefab`, `Assets/Meus/Caixa d'Agua.prefab`, `Assets/Meus/Assets Gerados/Focos/Ralo.prefab`, `.../Focos/Bromelia.prefab` e `Assets/Meus/Assets Gerados/Piscina de Plastico/Piscina de Plastico.prefab`. Pneus e garrafas não têm prefab.

     | Tipo | ferramentaNecessaria | segundos | textoAoMirar | textoFerramentaErrada |
     |---|---|---|---|---|
     | Pneu | Maos | 1,5 | "Pneu velho com água parada" | "Pneu não se resolve com isso. Esvazie e leve para a caçamba: use as <b>mãos</b>." |
     | Vaso | Areia | 2,0 | "Pratinho do vaso com água parada" | "Isso não deixa o pratinho seco. Encha de <b>areia</b> até a borda." |
     | CaixaDagua | Maos | 3,0 | "Caixa d'água destampada" | "Caixa d'água se protege com a tampa. Use as <b>mãos</b> para encaixar." |
     | Piscina | Maos | 2,5 | "Piscina de plástico com água da chuva" | "Esvazie e vire de boca para baixo. Use as <b>mãos</b>." |
     | Garrafas | SacoDeLixo | 0,4 por garrafa (`segundosPorGarrafa`) | "Garrafas largadas juntando água" | "Garrafa vai para o <b>saco de lixo</b>, e o saco para a caçamba." |
     | Ralo | Tela | 2,0 | "Ralo destampado com água parada" | "Isso não resolve o ralo. Ralo se protege com <b>tela</b> e grelha fechada." |
     | Bromelia | AguaSanitaria | 2,0 | "Bromélia com água acumulada nas folhas" | "Não precisa arrancar nem cobrir. Bromélia se trata com <b>água sanitária</b> diluída." |

  4. **Picape da UPA** (`Assets/Palmov Island/Low Poly Atmospheric Locations Pack/Prefabs/Vehicles/car SUVs pickups lights red.prefab`), com o nome "Picape da UPA":
     - posição: `SaidaDoHospital.posicaoNoBairro` + (3,5; 0; 2,0), assentada no chão por raycast;
     - `BoxCollider` nos bounds, se não tiver collider;
     - `PontoDeReabastecimento` com `{Areia, Tela, AguaSanitaria}`, `textoAoMirar` "Picape da UPA: material para o kit" e 1 s.
  5. **Caçamba**, com o nome "Cacamba":
     - 5 cubos (fundo de 2,4×0,1×1,4 m e paredes de 1,0 m de altura e 0,08 de espessura), com material URP Lit `#E8A21F`;
     - posição: centroide dos `PneuInteractable` + (−3; 0; 0), no chão;
     - `BoxCollider` (2,4×1,0×1,4) na raiz;
     - `Cacamba` com `textoAoMirar` "Caçamba de entulho" e 1 s.
  6. **Monte de areia:** o objeto "Monte de Areia 1" mais perto do centroide dos vasos ganha `PontoDeReabastecimento` (`{Areia}`, "Monte de areia: encher o balde de areia", 1,5 s) e um `MeshCollider` se não tiver collider.
  7. **Rita:** o `BalaoDeFala` com `quemFala == "Enf. Rita"` recebe `texto = PersonagensHospitalBuilder.FalaEnfermeira`, que passa a ser: "Agente, o hospital não aguenta mais! Seu kit está pronto [1-5] e a picape da UPA tem mais material. O bairro vai te contar pelo celular [T] onde estão os focos. Vai lá!"
  8. `EditorSceneManager.MarkSceneDirty` + `SaveScene` da cena ativa e `AssetDatabase.SaveAssets()`.

- [ ] **Step 1:** Implementar o builder e a mudança no `PersonagensHospitalBuilder`. Rodar o ciclo. Esperado: compila e PASS com 50 testes.
- [ ] **Step 2:** `unity command open_scene --path Assets/Scenes/SampleScene.unity` (antes, `console_status` e conferir que nenhuma cena está suja) e `unity command menu --path "Dengue/Configurar Kit do Agente"`. Esperado no `unity command console`: o resumo `[Kit]` com 23 focos configurados (6 pneus, 4 ralos, 4 bromélias, 3 vasos, 3 garrafas, 2 caixas, 1 piscina), "Picape da UPA", "Cacamba" e "Monte de areia" OK, e nenhum erro.
- [ ] **Step 3:** Rodar de novo o mesmo menu. Esperado: nenhum objeto duplicado (`GameObject.FindObjectsByType<Cacamba>().Length == 1`, conferido via `eval_file`).
- [ ] **Step 4:** Conferir o posicionamento com `unity command screenshot --view scene` de cima, sobre a picape e a caçamba. Nada pode atravessar casa ou muro, e a caçamba precisa ficar a menos de 10 m dos pneus. Se precisar, ajustar os dois offsets do builder, rodar de novo e recapturar.
- [ ] **Step 5: Preview** (skill `preview-design` do projeto) em Play Mode:
  - **antes**: a HUD atual;
  - **depois**: a barra de ferramentas, com o Canvas do `KitHUD` trocado para `ScreenSpaceCamera` só na captura, e o pneu nas mãos, chamando `kit.Pegar` num pneu.
  - Mostrar ao usuário com `SendUserFile`. Ajustar a pose do `PontoDeCarga` até o pneu aparecer no canto inferior direito, sem cobrir a mira. Gravar a pose final no builder.
- [ ] **Step 6: Checkpoint:** `git status` mostra a cena, os prefabs, o material e os scripts.

### Task 13: Balanceamento com o kit

**Files:**
- Modify: `Assets/Editor/BalanceamentoSimulador.cs`
- Modify: `Assets/Scenes/SampleScene.unity` (valores do `FluxoDeJogo`, via `SerializedObject`)

**Interfaces:**
- Consumes:
  - `Interactable.FerramentaNecessaria` e `SegundosPorPasso`;
  - `GarrafaInteractable.ContarGarrafas()`;
  - os campos de carga, saco e `multiplicadorCarregando` do `KitDoAgente`, lidos com `SerializedObject`;
  - `PontoDeReabastecimento` (lista `recarrega` via `SerializedObject`) e `Cacamba` da cena;
  - `ControladorJogador.VelocidadeAndar` e `VelocidadeCorrer`.
- Produces: `RotaComKit(...)`, que imprime a distância, o tempo e o **custo do kit por foco**, isto é, (tempo com kit − tempo da rota mínima atual) / total. A rota é o vizinho mais próximo a partir de `posicaoNoBairro`, correndo, e:
  - **ação de cada foco:** `SegundosPorPasso × passos`; para garrafas, `ContarGarrafas()` passos;
  - **consumível zerado:** antes de ir ao foco, desvia para o ponto de reabastecimento mais perto que tenha a ferramenta;
  - **pneu:** depois de pegar, vai até a caçamba mais perto andando a `VelocidadeAndar × multiplicadorCarregando` e segue de lá;
  - **saco cheio no meio de um grupo:** ida e volta à caçamba.
- A tabela de ritmos passa a somar o custo do kit a cada ritmo, com a legenda "ritmo + custo do kit".

- [ ] **Step 1:** Implementar e rodar `unity command menu --path "Dengue/Simular Balanceamento"`. Copiar a saída do `unity command console`.
- [ ] **Step 2:** Recalibrar mexendo primeiro em `taxaComTodosOsFocos` e depois em `duracaoInicioCalmo` no `FluxoDeJogo` da cena (`SerializedObject` via `eval_file`, depois `save_scene`). Alvo, o mesmo "desafio moderado" de 27/09:
  - **ritmo 30 + custo:** vence com pico ≤ 90%;
  - **ritmo 35 + custo:** vence;
  - **ritmo 40 + custo:** perde;
  - **parado:** perde entre 2:30 e 3:30.
- [ ] **Step 3:** Rodar o simulador de novo e confirmar os 4 critérios na saída.
- [ ] **Step 4: Checkpoint.**

### Task 14: Verificação final em Play Mode + registro

**Files:**
- Modify: `contexto do jogo.md` (nova entrada no fim do Histórico)

- [ ] **Step 1:** Rodar o ciclo completo de testes. Esperado: PASS, 50 testes, `Failed: 0`.
- [ ] **Step 2:** Play Mode na `SampleScene`, com um `eval_file` por grupo. Preparação: `Application.runInBackground = true`, `ControleAtivo = false`, abertura pulada e `fluxo.IniciarPartida()`. Conferir:
  - **cada tipo de foco:** conclui com a ferramenta da tabela, chamando `AvancarCom` pelo kit da cena; com uma ferramenta errada, `CurrentClicks` não muda;
  - **estoque:** depois de 1 vaso, Areia 0; o 2º vaso dá SemCarga; o monte de areia (`Usar`) volta para 1; a picape enche tudo;
  - **pneu:** pegar → `FocosEliminados` não muda → caçamba → `FocosEliminados` +1;
  - **saco:** com capacidade 6, um grupo maior que o espaço livre dá SacoCheio; a caçamba esvazia;
  - **rede social:** os 4 posts novos aparecem no feed (`RedeSocial`), cada um uma vez só;
  - **zumbido:** o pneu carregado não zumbe (`Zumbe == false`);
  - **fim:** vitória com os 23 → `OnVitoria`, epílogo e tela final;
  - **Console:** só os erros antigos de `Default-Particle.psd`.
- [ ] **Step 3:** Limpeza: `editor_stop`, `PlayerSettings.runInBackground = false` (conferir `runInBackground: 0` no disco) e deixar aberta só a `MenuInicial` (`open_scene --path Assets/Scenes/MenuInicial.unity`).
- [ ] **Step 4:** Escrever a entrada "2026-MM-DD — Parte A implementada: kit do agente" no `contexto do jogo.md`, no formato das anteriores:
  - objetivo e arquivos novos e alterados;
  - valores finais do balanceamento, com a tabela do simulador;
  - o que foi verificado e o que **não** foi: a partida real com teclado, mouse e controle;
  - observações para futuras sessões: o `PontoDeUso` não herda `Interactable`, a ordem do enum `Ferramenta` não pode mudar e o builder pode ser rodado de novo.
- [ ] **Step 5:** Pedir ao usuário uma partida real e o retorno sobre o ritmo e o "apertado".
