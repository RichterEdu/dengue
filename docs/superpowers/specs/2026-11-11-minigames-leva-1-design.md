# Foco Zero — Minigames dos focos, leva 1

Spec com o design aprovado pelo usuário em 2026-11-11 (brainstorming em chat).
Base: branch `itens-na-mao` em `5b5ebb7`, mais o nascimento seco de 2026-11-10, ainda não commitado.

## Context

Só o ralo tem minigame (o skill check do anel). Os outros seis focos são "segurar o botão até a barra encher":
pneu, vaso, garrafas, bromélia, caixa d'água e piscina.

Objetivo: cada foco ganha uma ação própria, diferente da do ralo e das outras, em que o jogador faz o gesto em vez de
esperar uma barra.

Estado atual relevante:
- `Interactor` tem dois modos: segurar (`SessaoDeSegurar`) e skill check (`SessaoDeSkillCheck`), este escrito direto
  nele (`IniciarSkillCheck`, `AtualizarSkillCheck`, `EncerrarSkillCheck`).
- O foco entra no skill check por `Interactable.UsaSkillCheck`; erro e cancelamento são `ErrarSkillCheck` e
  `CancelarSkillCheck`.
- Cada acerto vira um `AvancarCom(kit)`. É esse passo que dispara som, HUD, gasto de carga, rede social e nota.
- `ControladorJogador.Imobilizado` para andar, olhar e pular, com a entrada ainda ligada.
- `KitDoAgente.PneuCarregado` é o único objeto carregável; `Interactor` e `Interactable.Verificar` perguntam por ele.
- `BalanceamentoSimulador` estima o tempo de cada foco por `SegundosPorPasso × RequiredClicks`.

### Decisões do usuário
- Formato: **depende do foco** (gesto na cena 3D ou painel 2D). Na leva 1 os três são na cena 3D.
- Entrega em **duas levas**. Esta cobre a base comum, pneu, bromélia e caixa d'água.
- Ir mal **só perde tempo**: não gasta carga nem zera o foco. O ralo continua como está, gastando a tela no erro.
- Pneu: **balançar no ritmo** até a água transbordar.
- Bromélia: **mirar o borrifador** em cada pocinha.
- Caixa d'água: **carregar a tampa e tampar**, como o pneu na picape. A tampa fica **encostada na caixa**, como hoje.

### Fora do escopo
- Vaso, garrafas e piscina (leva 2, com spec próprio). Continuam no segurar.
- Atalho de esvaziar com as Mãos (vaso, garrafas): sem mudança.
- Painel 2D de minigame: nenhum foco desta leva usa.
- Nota e recorde: nenhum critério novo.

## Design

### 1. Base comum de minigame

Arquivo novo `Assets/Scripts/Minigame.cs`, só C# puro (testável no EditMode):

```csharp
public struct EntradaDoMinigame
{
    public Vector2 Olhar;      // delta do mouse / analógico direito (EntradaJogador.Olhar)
    public bool Segurando;     // botão de interagir mantido
    public bool Apertou;       // botão de interagir apertado neste quadro
}

public enum EstadoDoMinigame { EmAndamento, Passo, Errou }

public interface IMinigame
{
    EstadoDoMinigame Atualizar(float dt, EntradaDoMinigame entrada);
    float FracaoDoPasso { get; }   // 0..1 do passo atual, para a barra da HUD
    bool PodeSair { get; }         // o botão de largar sai do minigame (ralo: false)
}
```

`Atualizar` devolve no máximo um `Passo` por quadro. A sessão não sabe quando o foco termina: quem diz é o
`AvancarCom`.

`Interactable` troca `UsaSkillCheck`, `SegundosPorVoltaDoCheck`, `ErrarSkillCheck` e `CancelarSkillCheck` por:

```csharp
public virtual IMinigame CriarMinigame(KitDoAgente kit, int semente) => null;   // null = segurar, como hoje
public virtual void AoErrarMinigame(KitDoAgente kit) { }
public virtual void AoSairDoMinigame() { }      // saída voluntária ou cancelamento
public virtual float SegundosEstimados => SegundosPorPasso * RequiredClicks;   // BalanceamentoSimulador
```

`Interactor`:
- No aperto, se `Conferir()` passa e `CriarMinigame` devolve uma sessão, ela vira `MinigameAtivo` e o jogador fica
  `Imobilizado`. Enquanto ativa, não há troca de ferramenta nem de alvo.
- A cada quadro (com `Time.deltaTime > 0`) monta a `EntradaDoMinigame` e chama `Atualizar`:
  - `Passo`: `AvancarCom(kit)`. Qualquer resultado diferente de `Avancou` encerra o minigame.
  - `Errou`: `AoErrarMinigame(kit)`, encerra e dispara `OnErrouSkillCheck`.
- **Sair:** com `PodeSair`, o botão de largar (`EntradaJogador.LargarPressionado`: botão direito, G, botão Norte) chama
  `AoSairDoMinigame` e encerra. Os passos já dados ficam; o progresso dentro do passo se perde.
- `Bloqueado` (celular, fim de jogo) encerra do mesmo jeito, sem punição.
- `ProgressoContinuo` = `(CurrentClicks + FracaoDoPasso) / RequiredClicks`.
- `Avancando` fica verdadeiro com minigame ativo que não seja o skill check (som de ação e gesto do item na mão). No
  skill check continua falso, como hoje.
- `SkillCheckAtivo` continua existindo para a `SkillCheckHUD` e para o `ItensNaMao`: devolve a sessão quando o minigame
  ativo é o do ralo.

Ralo: `MinigameDeSkillCheck` embrulha a `SessaoDeSkillCheck` (aperto → `Apertar`, senão `Atualizar`; `Acertou` →
`Passo`). `RaloInteractable.CriarMinigame` o devolve; `AoErrarMinigame` gasta a tela e volta ao começo;
`AoSairDoMinigame` só volta ao começo. `PodeSair` é false: o botão de largar não sai do ralo, e a regra do anel não muda.

### 2. Pneu: balançar no ritmo

Lógica pura em `Assets/Scripts/OndaDoPneu.cs` (`IMinigame`):
- A água é um oscilador amortecido: `a = -ω²·x − 2ζω·v + ganho·u`, com `u = clamp(Olhar.x × sensibilidade, −1, 1)`.
- `|x|` acima de `limiarDaBorda` derrama: `nivel −= taxa × (|x| − limiarDaBorda) × dt`. `nivel` vai de 1 a 0.
- Devolve `Passo` cada vez que `nivel` cruza `1 − k/passos`, com `passos = RequiredClicks` do pneu na cena.
  `FracaoDoPasso` é o quanto da fatia atual já saiu.
- `u` usa a velocidade do gesto (`Olhar.x / dt`), para não depender do FPS.
- Expõe `Inclinacao` (x) e `Nivel` para o visual.

Valores iniciais (Inspector do `PneuInteractable`, calibrados jogando): período de 0,9 s, `ζ = 0,12`, e ganho e taxa
ajustados para cerca de 5 s no ritmo certo. Parado, a onda morre em uns 3 s. `SegundosEstimados` = 6 s.

`PneuInteractable`:
- `CriarMinigame` devolve a onda quando o kit está nas Mãos e o pneu não está `Esvaziado`. Esvaziado, devolve null e
  pegar continua instantâneo.
- Durante o minigame, a cada quadro: o pneu gira em torno do ponto de apoio no chão conforme `Inclinacao` (até ±25°), e
  a água (`water`) encolhe com `Nivel` e desloca para o lado baixo.
- Ao derramar, um respingo de partículas no lado baixo (mesmo material do `EstourarParticulas`).
- `AoSairDoMinigame` devolve a pose do pneu e deixa a água no nível do último passo fechado.
- O terceiro passo esvazia o pneu pelo `AoTerminarAcao` de hoje (`Esvaziou`), que encerra o minigame.
- Reencher (chuva) zera os cliques e a água volta inteira, como hoje.

Dica na HUD enquanto ativo: "Balance o pneu ← →" e "Botão direito: sair".

### 3. Bromélia: mirar o borrifador

Lógica pura em `Assets/Scripts/MiraDoBorrifador.cs` (`IMinigame`):
- Recebe as direções das pocinhas ainda cheias (ângulos de yaw e pitch relativos ao centro da planta, vistos da câmera
  no início), o raio angular de acerto e os segundos para secar cada uma.
- Guarda a mira (yaw, pitch) somando `Olhar`, presa a um cone de `coneDaMira` graus em volta do centro.
- Com `Segurando`, a pocinha cheia mais perto da mira e dentro do raio seca: `umidade −= dt / segundosParaSecar`.
- Uma pocinha que chega a zero é marcada como seca. Devolve `Passo` cada vez que a contagem de secas alcança
  `ceil(total × k / passos)`.
- Expõe `Mira`, `PocaSecandoAgora` e `Umidade(i)`.

Valores iniciais: cone de 30°, raio de acerto de 4°, 0,5 s por pocinha e 1,2 s para o copo central (a última do array
`pocas`). `SegundosEstimados` = soma dos tempos de secar + 0,4 s de mira por pocinha.

`BromeliaInteractable`:
- `CriarMinigame` devolve a mira com a água sanitária selecionada.
- O `Interactor` aplica `Mira` na visão (`ControladorJogador.DefinirOlhar`), então o centro da tela é a mira.
- A pocinha que seca na sessão encolhe conforme `Umidade` e some em zero. O `OnInteract` deixa de secar em ordem fixa
  quando o passo vem do minigame; no último passo, seca o que tiver sobrado (garantia).
- No caminho antigo `Interact()` (sem kit) e nos testes sem minigame, a ordem fixa continua.
- `AoSairDoMinigame`: as secas ficam secas; a que estava pela metade volta a cheia.
- A dose é gasta só ao concluir, como hoje.

Jato: partículas saindo do bico do borrifador na mão (`ItensNaMao`) enquanto `Segurando`. Borrifar fora de qualquer
pocinha não faz nada.

Dica na HUD: "Segure para borrifar as pocinhas" e "Botão direito: sair".

### 4. Caixa d'água: carregar a tampa

Não usa `IMinigame`: é carregar, como o pneu.

`KitDoAgente`:
- `CaixaDaguaInteractable TampaCarregada` (a caixa dona da tampa nos braços; null = nenhuma).
- `bool MaosOcupadas => PneuCarregado != null || TampaCarregada != null`. `Selecionar`, o `Interactor` (troca de
  ferramenta, `comPneu`) e `Interactable.Verificar` passam a usar `MaosOcupadas`.
- `PegarTampa(caixa)`: põe a tampa no `PontoDeCarga`, aplica os limites de movimento do pneu (velocidade ×
  `multiplicadorCarregando`, sem pular; a stamina de corrida vale igual). Evento `OnTampaPega`.
- `DevolverTampa()`: a tampa volta para a lateral da caixa (pose aberta). Evento `OnTampaDevolvida`.
- `ColocarTampa()`: chamado pela caixa ao concluir. Evento `OnTampaColocada`.

`CaixaDaguaInteractable`:
- Com as Mãos e a tampa encostada: o aperto pega a tampa (`ResultadoDoPasso.Pegou`), sem tempo de segurar.
- Com a tampa nos braços, só a caixa dona é alvo. Segurar `segundosParaTampar` (0,8 s) nela: a tampa sai dos braços,
  faz o arco até a boca (o `MoveRoutine` de hoje, partindo da posição de mundo) e o foco conclui.
- Tampar é um passo único, que não depende do `requiredClicks` salvo na cena (nenhuma cena ou prefab é editado).
  `Fechamento` vai de 0 a 1 só nesse passo.
- Botão de largar com a tampa nos braços: `DevolverTampa`. Vale a qualquer distância; a tampa volta à caixa.
- Tampa nos braços: a chuva não reenche (`PodeReencher`), como hoje com a tampa em movimento.
- `SegundosEstimados` = 1,5 s.

`Interactor.RaycastTarget`: com `TampaCarregada`, o único foco que não é atravessado é a caixa dona.

HUD (`FocoHUDInfo`): "Pegar a tampa" com a tampa encostada, "Tampar a caixa" com ela nos braços.

### 5. Entrada e controle

- Mouse e controle usam o mesmo `EntradaJogador.Olhar`. `OndaDoPneu` e `MiraDoBorrifador` têm sensibilidade própria no
  Inspector do foco.
- Pausa (`Time.deltaTime == 0`): a sessão não anda.

## Testes

EditMode, em `Assets/Editor/Testes/`:
- `OndaDoPneuTestes`: parado não derrama; empurrar em fase derrama e fecha 3 passos; empurrar fora de fase não passa do
  limiar; um `Passo` por quadro no máximo.
- `MiraDoBorrifadorTestes`: mira presa ao cone; segurar fora das pocinhas não seca; seca a mais próxima; passos nas
  contagens certas; soltar no meio não conta.
- `MinigameNoInteractorTestes`: `Passo` vira `AvancarCom`; sair mantém os cliques; `Bloqueado` encerra sem gastar carga.
- `TampaTestes`: pegar, devolver, tampar conclui, mãos ocupadas recusam outros focos, pneu e tampa não coexistem.
- Atualizar `SkillCheckTestes`, `PneuEsvaziarTestes`, `InteractableKitTestes`, `NascimentoTestes` e
  `BalanceamentoSimuladorTestes` para a interface nova e para `SegundosEstimados`.

No jogo: jogar os três focos com mouse e com controle; chuva durante cada um; celular e fim de jogo no meio; ralo igual
a antes.

## Registro

Toda alteração entra no Histórico de Alterações de `contexto do jogo.md` (`agents.md`).
