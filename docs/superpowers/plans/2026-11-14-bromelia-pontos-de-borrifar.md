# Bromélia: pontos de borrifar (bolinhas) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Tratar a bromélia passa a ser clicar em bolinhas sobre as pocinhas, numa vista de perto por cima da planta, com o borrifador na tela.

**Architecture:** A sessão é uma classe pura (`PontosDeBorrifar`) que trabalha num campo 2D. A bromélia calcula uma pose de câmera, projeta nela as pocinhas com água e entrega a sessão junto com a pose (`IMinigameDePerto`). O `Interactor` pede essa pose ao `CameraJogador` e segura o agente até a câmera voltar. Dois componentes novos no Player desenham a sessão: a `PontosDeBorrifarHUD` (bolinhas, mira, contagem e dica) e o `BorrifadorDePerto` (cópia do borrifador presa à câmera). O minigame antigo (`MiraDoBorrifador`, `IMinigameComMira`) sai.

**Tech Stack:** Unity 6000.5.6f1, URP, uGUI + TextMeshPro, NUnit EditMode, `unity` CLI.

**Spec:** `docs/superpowers/specs/2026-11-14-bromelia-pontos-de-borrifar-design.md`

## Global Constraints

- Trabalhar na branch `bromelia-pontos-de-borrifar`, criada a partir de `pneu-nos-bracos` (`99bcbad`). Commits terminam com `Co-Authored-By: Claude Fable 5.2 <noreply@anthropic.com>`: os blocos `git commit` abaixo mostram só o título, e a linha entra com um segundo `-m`. Arquivo novo vai com o `.meta`; arquivo apagado leva o `.meta` junto.
- Não commitar `Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF - Fallback.asset`, que já estava modificado e não é deste trabalho.
- Nenhuma cena é editada. O único asset alterado é `Assets/Meus/Assets Gerados/Focos/Bromelia.prefab` (Task 5). O `AGENTS.md` não muda.
- Não mudar: ralo, pneu, caixa d'água, vaso, garrafas, áudio (nenhum arquivo de som novo), textos educativos, rede social, `requiredClicks` e o caminho antigo `Interact()`.
- A sessão nunca devolve `EstadoDoMinigame.Errou`. A dose de água sanitária só sai quando o foco conclui.
- Valores iniciais, com padrão no código:

  | Valor | Inicial |
  |---|---|
  | altura da câmera acima do copo | 0,58 m |
  | recuo da câmera para o lado do agente | 0,17 m |
  | FOV de perto | 40° |
  | raio da bolinha (o acerto usa o mesmo) | 0,14 do campo |
  | tempo do anel | 1,5 s |
  | pausa da bolinha depois de o anel fechar | 0,35 s |
  | espera inicial | 0,3 s |
  | fecho depois do último acerto | 0,45 s |
  | mira por grau | 0,0093 do campo |
  | ida e volta da câmera | 0,3 s e 0,25 s, em tempo sem escala |

- Textos exatos:
  - convite ao mirar: `"Clique para borrifar as pocinhas"` (não muda);
  - dica da sessão: `"Clique nas bolinhas para borrifar   |   Botão direito: sair"`.
- Campo: `y` de −1 (base da tela) a +1 (topo); `x` na mesma unidade, de −proporção a +proporção; centro em (0, 0).
- Convenções do projeto: cabeçalho de comentário em cada arquivo, nomes e comentários em português, lógica em classe C# pura com o tempo entrando por `dt`, testes EditMode em `Assets/Editor/Testes/` com o `TesteUtil`. A categoria é `Minigames`, salvo onde a tarefa disser outra.
- Testes pelo CLI:
  - abrir uma cena vazia antes de rodar (cena suja abre uma janela modal e trava o Editor);
  - conferir `Total > 0`, porque o `run_tests` responde sucesso com 0 testes quando a compilação falha.
- Trocar de cena pelo CLI: um arquivo `.cs` de uma linha rodado com `unity command eval_file --file <arquivo>`. Cena vazia: `UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene); return "ok";`. Cena do jogo: `UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity"); return "ok";`. Arquivos de apoio assim ficam em `.superpowers/sdd/2026-11-14-bromelia-pontos-de-borrifar/` (fora do git).
- Toda alteração entra no Histórico de Alterações do `contexto do jogo.md` (Task 8).

## Review Focus

- **Interrupção no meio segundo do fecho** (botão direito, celular ou pausa logo depois do último acerto): a planta não pode ficar seca e sem concluir. O copo volta a ter água e a sessão seguinte traz só ele (Task 4: `SairNoFechoDevolveAAguaDoCopo`, `BloqueioNoFechoDevolveAAguaDoCopo`).
- **Bloqueio com a câmera em trânsito** (antes de ela chegar ou durante a volta, inclusive com `timeScale` 0): a câmera volta de onde estava, sem salto, e o agente é liberado (Task 2: `SoltarNoMeioDaIdaVoltaDeOndeEstava`, `SoltaComOJogoPausadoVolta`; Task 3: `BloquearSolta`).
- **Bromélia desativada ou destruída no meio da sessão:** a câmera é solta, o agente é liberado e o enxame não fica escondido (Task 3: `AlvoDesativadoSolta`, `AlvoDestruidoSolta`; Task 4: `DesativadaNoMeioLargaASessao`).
- **"Inverter Y" ligado nas configurações:** a mira sobe quando o mouse sobe (Task 1: `MiraIgnoraOOlhar`; Task 3 liga o `Ponteiro`; Task 8 confere em Play).
- **Botão segurado e tela fora de 16:9:** segurar não acerta nada, só o aperto conta; a mira fica presa à tela real (Task 1: `SegurarNaoAcerta`, `MiraPresaAoCampo`; Task 4: `ProporcaoDaTelaChegaASessao`).

## Mapa de arquivos

| Arquivo | Papel | Task |
|---|---|---|
| `Assets/Scripts/Minigame.cs` | ganha `Ponteiro`, `PoseDePerto` e `IMinigameDePerto`; perde `IMinigameComMira` | 1, 4 |
| `Assets/Scripts/PontosDeBorrifar.cs` (novo) | a sessão, lógica pura | 1 |
| `Assets/Scripts/CameraJogador.cs` | enquadramento temporário | 2 |
| `Assets/Scripts/EntradaJogador.cs` | `OlharSemInverter` | 3 |
| `Assets/Scripts/Interactor.cs` | pede e solta o enquadramento; segura o agente na volta | 3, 4, 6, 7 |
| `Assets/Scripts/BromeliaInteractable.cs` | cria a sessão, seca as pocinhas, esconde o enxame | 4 |
| `Assets/Scripts/EnxameDeMosquitos.cs` | `NomeDoFilho` passa a `internal` | 4 |
| `Assets/Scripts/ControladorJogador.cs` | saem `PitchMinimo` e `PitchMaximo` | 4 |
| `Assets/Scripts/MiraDoBorrifador.cs` | apagado | 4 |
| `Assets/Editor/FocosBlenderBuilder.cs` | sobe e aumenta as 4 pocinhas de fora | 5 |
| `Assets/Scripts/PontosDeBorrifarHUD.cs` (novo) | bolinhas, mira, contagem e dica | 6 |
| `Assets/Scripts/SkillCheckHUD.cs` | `GerarAnel` vira estático, para as duas HUDs | 6 |
| `Assets/Scripts/DengueHUD.cs` | sem mira nem painel na vista de perto | 6 |
| `Assets/Scripts/BorrifadorDePerto.cs` (novo) | a cópia do borrifador presa à câmera | 7 |
| `Assets/Scripts/ItensNaMao.cs`, `RegraDoItemNaMao.cs` | mão quieta na vista de perto | 7 |

---

### Task 1: A sessão `PontosDeBorrifar`

**Files:**
- Modify: `Assets/Scripts/Minigame.cs`
- Create: `Assets/Scripts/PontosDeBorrifar.cs`
- Test: `Assets/Editor/Testes/PontosDeBorrifarTestes.cs` (novo)

**Interfaces:**
- Produces, em `Minigame.cs`:

```csharp
public struct EntradaDoMinigame
{
    public Vector2 Olhar;
    public Vector2 Ponteiro;   // o mesmo movimento, sem o "inverter Y": +x = direita, +y = para cima na tela
    public bool Segurando;
    public bool Apertou;
}

/* Pose de câmera de uma vista de perto. */
public struct PoseDePerto
{
    public Vector3 Posicao;
    public Quaternion Giro;
    public float Fov;          // vertical, em graus

    /* Onde um ponto do mundo cai no campo desta pose. Atrás da câmera
     * (profundidade menor que 1 cm): Vector2.zero. */
    public Vector2 NoCampo(Vector3 mundo);
}

/* Sessão que quer a câmera numa pose enquanto durar. */
public interface IMinigameDePerto
{
    PoseDePerto Pose { get; }
}
```

- Produces, em `PontosDeBorrifar.cs`:

```csharp
public class PontosDeBorrifar : IMinigame
{
    public const float RaioPadrao = 0.14f, AnelPadrao = 1.5f, PausaPadrao = 0.35f, EsperaPadrao = 0.3f,
                       FechoPadrao = 0.45f, MeiaLarguraPadrao = 16f / 9f, MiraPorGrauPadrao = 0.0093f;
    public const float SegundosAgindo = 0.2f;

    public PontosDeBorrifar(Vector2[] pontos, int passos,
        float raio = RaioPadrao, float segundosDoAnel = AnelPadrao, float pausa = PausaPadrao,
        float esperaInicial = EsperaPadrao, float fecho = FechoPadrao,
        float meiaLargura = MeiaLarguraPadrao, float miraPorGrau = MiraPorGrauPadrao);

    public Vector2 Mira { get; }
    public int Total { get; }
    public int Acertos { get; }
    public float Raio { get; }
    public int DaVez { get; }            // índice da bolinha da vez; −1 na espera inicial e depois do último acerto
    public int Proxima { get; }          // DaVez + 1; −1 se DaVez for −1 ou a última
    public bool BolinhaNaTela { get; }   // false na espera inicial, na pausa e no fecho
    public float Anel { get; }           // 1 = cheio, 0 = fechado; 1 sem bolinha na tela
    public bool NoFecho { get; }         // tudo acertado e ainda há passo a dar
    public int Borrifadas { get; }       // apertos contados
    public Vector2 UltimaBorrifada { get; }   // onde a mira estava no último aperto
    public bool UltimaAcertou { get; }
    public Vector2 Ponto(int k);         // fora do intervalo: Vector2.zero

    // IMinigame: Atualizar, FracaoDoPasso, PodeSair (sempre true), Agindo
}
```

- [ ] **Step 1: Criar a branch e registrar spec e plano**

```bash
git switch -c bromelia-pontos-de-borrifar
git add docs/superpowers/specs/2026-11-14-bromelia-pontos-de-borrifar-design.md docs/superpowers/plans/2026-11-14-bromelia-pontos-de-borrifar.md "contexto do jogo.md"
git commit -m "Spec e plano: bromélia com pontos de borrifar"
```

- [ ] **Step 2: Escrever os testes**

Ajudantes de `PontosDeBorrifarTestes`:

```csharp
private const float Q = 1f / 60f;
private const float Mpg = 0.01f;   // mira por grau nos testes: Ponteiro (10, 0) anda 0,1 do campo
private static readonly Vector2[] Tres = { new Vector2(0.4f, 0f), new Vector2(-0.4f, 0.2f), Vector2.zero };

private static PontosDeBorrifar Nova(Vector2[] pontos, int passos, float espera = 0f, float fecho = 0f)
    => new PontosDeBorrifar(pontos, passos, 0.14f, 1.5f, 0.35f, espera, fecho, 1.5f, Mpg);

private static EstadoDoMinigame Quadro(PontosDeBorrifar s, Vector2 ponteiro = default, bool apertou = false, float dt = Q)
    => s.Atualizar(dt, new EntradaDoMinigame { Ponteiro = ponteiro, Apertou = apertou });

/* Um quadro que leva a mira até 'alvo' (e aperta, se pedido). */
private static EstadoDoMinigame Ir(PontosDeBorrifar s, Vector2 alvo, bool apertou = false)
    => Quadro(s, (alvo - s.Mira) / Mpg, apertou);

/* Quadros parados (de Q em Q) por 'segundos'; devolve quantos deram Passo. Falha se algum der Errou. */
private static int Esperar(PontosDeBorrifar s, float segundos) { /* a escrever */ }
```

`Cinco` = cinco pontos quaisquer a mais de 0,3 um do outro. "Acertar" = `Ir(s, s.Ponto(s.DaVez), true)`. Com espera inicial 0, a primeira bolinha já está na tela antes do primeiro `Atualizar`. As tolerâncias de tempo abaixo já contam um ou dois quadros de folga.

| Teste | Montagem e assertivas |
|---|---|
| `MiraAndaComOPonteiro` | `Nova(Tres, 3)`; `Quadro(s, (10, −5))` → `Mira == (0.1, −0.05)` |
| `MiraIgnoraOOlhar` | `Atualizar(Q, { Olhar = (10, 10) })` → `Mira == (0, 0)` |
| `MiraPresaAoCampo` | `Quadro(s, (1000, 1000))` → `Mira == (1.5, 1)`; `Quadro(s, (−5000, −5000))` → `(−1.5, −1)` |
| `MovimentoNaoFinitoContaComoParado` | `Quadro(s, (NaN, +∞))` → `Mira == (0, 0)`; depois `Quadro(s, (10, 0))` → `(0.1, 0)` |
| `CliqueDentroAcertaEPassaParaAProxima` | `Ir(s, Tres[0], true)` devolve `Passo`; `Acertos == 1`, `DaVez == 1`, `Proxima == 2`, `Anel == 1`, `UltimaAcertou`, `Borrifadas == 1`, `UltimaBorrifada == Tres[0]` |
| `CliqueNoRaioAcerta` | `Ir(s, Tres[0] + (0.139, 0), true)` → `Acertos == 1`. Em outra sessão, `+ (0.15, 0)` → `Acertos == 0` |
| `CliqueForaNaoConta` | `Ir(s, (0.9, 0.9), true)` devolve `EmAndamento`; `Acertos == 0`, `Borrifadas == 1`, `!UltimaAcertou`, `DaVez == 0` |
| `CliqueNaApagadaNaoConta` | `Ir(s, Tres[1], true)` → `Acertos == 0` |
| `SegurarNaoAcerta` | `Ir(s, Tres[0])`; 60 quadros com `Segurando = true` e `Apertou = false` → `Acertos == 0`, `Borrifadas == 0` |
| `NadaAcertaNaEsperaInicial` | `Nova(Tres, 3, espera: 0.3f)`; `Ir(s, Tres[0], true)` → `Acertos == 0`, `DaVez == −1`, `Proxima == −1`, `!BolinhaNaTela`, `Borrifadas == 1`; `Esperar(s, 0.3f)` → `DaVez == 0`, `BolinhaNaTela`; `Quadro(s, apertou: true)` → `Acertos == 1` |
| `AnelFechaEABolinhaVolta` | `Ir(s, Tres[0])`; `Esperar(0.75f)` → `Anel` em 0.5 ± 0.04; `Esperar(0.8f)` → `!BolinhaNaTela`, `DaVez == 0`; aperto → `Acertos == 0`; `Esperar(0.4f)` → `BolinhaNaTela`, `Anel > 0.8`; aperto → `Acertos == 1` |
| `PassosNasContagensCertas` | `Nova(Cinco, 3)`: acertando os cinco, o retorno é `Passo` só no 2º, no 4º e no 5º acerto |
| `MaisPassosQuePontos` | dois pontos, 5 passos: 1º acerto → `Passo` e `FracaoDoPasso == 1` (o passo seguinte já é devido); quadro parado → `Passo`; outro → `EmAndamento`; 2º acerto → `Passo`; dois quadros parados → `Passo`, `Passo`; outro → `EmAndamento` |
| `SemPontosTodosOsPassosSaem` | `pontos` vazio e `pontos` null, 3 passos: `Total == 0`, `DaVez == −1`; quatro quadros → `Passo`, `Passo`, `Passo`, `EmAndamento` |
| `PassoDoUltimoAcertoSoDepoisDoFecho` | `Nova(Tres, 3, fecho: 0.45f)`: 1º e 2º acertos → `Passo`; 3º → `EmAndamento`, `NoFecho`, `DaVez == −1`, `Proxima == −1`, `FracaoDoPasso == 1`; `Esperar(0.4f) == 0`; `Esperar(0.1f) == 1`; `!NoFecho` |
| `FechoSeguraTodosOsPassosDoUltimoAcerto` | dois pontos, 5 passos, fecho 0.45: depois do 2º acerto, `Esperar(0.4f) == 0`; `Esperar(0.2f) == 3` |
| `DtInvalidoNaoMudaNada` | para `dt` 0, −1 e NaN: `Quadro(s, (10, 0), true, dt)` → `Mira == (0, 0)`, `Borrifadas == 0`, `Anel == 1`, `!Agindo`. Com dois pontos e 5 passos, depois do 1º acerto, `Quadro(dt: 0)` ainda devolve o `Passo` devido |
| `DtInvalidoNaoAndaOFecho` | depois do último acerto com fecho 0.45, cem `Quadro(dt: 0)` → sempre `EmAndamento` |
| `AgindoSoLogoDepoisDeUmAperto` | antes: `!Agindo`; `Ir(s, (0.9, 0.9), true)` → `Agindo`; `Esperar(0.1f)` → `Agindo`; `Esperar(0.15f)` → `!Agindo` |
| `FracaoDoPassoContaOsAcertos` | `Nova(Cinco, 2)`: 0 acertos → 0; 1 → 1/3; 2 → 2/3; 3 (deu `Passo`) → 0; 4 → 0.5 |
| `PodeSairSempre` | `PodeSair` na espera inicial, com bolinha na tela e no fecho |
| `ArgumentosRuinsViramPadrao` | `new PontosDeBorrifar(new[] { new Vector2(float.NaN, 0.2f) }, 0, −1, NaN, −1, NaN, −1, 0, +∞)`: `Raio == 0.14`; `Ponto(0) == (0, 0.2)`; `DaVez == −1` no 1º quadro (a espera virou 0,3); `Quadro(s, (100, 0))` → `Mira.x == 0.93`; `Quadro(s, (1e6, 0))` → `Mira.x == 16/9`; `Esperar(0.35f)`, um aperto com `Ponteiro = (Ponto(0) − Mira) / MiraPorGrauPadrao` e `Esperar(1f)`: ao todo, um único `Passo` |
| `PoseDePerto_NoCampo` | pose em (0, 1, 0), `Giro = LookRotation(Vector3.down, Vector3.forward)`, `Fov = 90`: `NoCampo((0,0,0)) == (0, 0)`; `NoCampo((0,0,1)) == (0, 1)`; `NoCampo((1,0,0)) == (1, 0)`; `NoCampo((0,2,0)) == (0, 0)` (atrás) |

- [ ] **Step 3: Rodar e ver falhar**

Run: `unity command run_tests --mode EditMode --filter_type category --filter Minigames`
Expected: erro de compilação, porque `PontosDeBorrifar` não existe.

- [ ] **Step 4: Implementar**

**`Minigame.cs`:** acrescentar `Ponteiro`, `PoseDePerto` e `IMinigameDePerto` e descrevê-los no cabeçalho. `IMinigameComMira` fica até a Task 4. `NoCampo`: com `l = Inverse(Giro) * (mundo − Posicao)`, devolve `(l.x, l.y) / (l.z * tan(Fov / 2))`.

**`PontosDeBorrifar`:** a ordem dentro de `Atualizar(dt, entrada)` decide os testes, então é esta:

1. `dt` inválido (`!(dt > 0)` ou não finito): pular para o passo 5.
2. Mira: `Mira += Ponteiro × miraPorGrau`, cada eixo não finito valendo 0; prender `x` em ±`meiaLargura` e `y` em ±1.
3. Tempos, nesta ordem: o resto do `Agindo`; a espera inicial (ao zerar, a primeira bolinha entra com o anel cheio); a pausa (ao zerar, a bolinha volta com o anel cheio); o fecho.
4. Aperto (`entrada.Apertou`): `Borrifadas++`, `UltimaBorrifada = Mira`, `Agindo` por `SegundosAgindo`. É acerto se `BolinhaNaTela` e a distância da mira ao ponto da vez for ≤ `raio`: `Acertos++`, o anel volta a cheio para a seguinte e, se foi o último, o fecho começa. Sem acerto neste quadro, e só se a bolinha já estava na tela no começo da chamada, o anel anda `dt / segundosDoAnel`; em zero, a bolinha sai pela pausa.
5. Passo: o k-ésimo fica devido com `Acertos ≥ ceil(Total × k / passos)`. Um passo que precisa de todos os acertos só sai com o fecho zerado. Sem pontos, todos saem, sem fecho. No máximo um `Passo` por chamada.

O resto:
- `FracaoDoPasso` = (acertos desde o último passo dado) ÷ (acertos entre ele e o próximo), entre 0 e 1; 0 com todos os passos dados; 1 quando o próximo passo já é devido (o divisor seria 0).
- Argumentos: `passos < 1` vira 1. `raio`, `segundosDoAnel`, `meiaLargura` e `miraPorGrau` não finitos ou ≤ 0 viram o padrão. `pausa`, `esperaInicial` e `fecho` não finitos ou < 0 viram o padrão (0 é válido). Coordenada de ponto não finita vira 0.
- A classe não é `sealed` (a bromélia deriva dela na Task 4).

- [ ] **Step 5: Rodar e ver passar**

Run: `unity command run_tests --mode EditMode --filter_type category --filter Minigames`
Expected: todos passam e `Total > 0`.

- [ ] **Step 6: Commit**

```bash
git add Assets/Scripts/Minigame.cs Assets/Scripts/PontosDeBorrifar.cs* Assets/Editor/Testes/PontosDeBorrifarTestes.cs*
git commit -m "Bromélia: sessão de pontos de borrifar (lógica pura)"
```

### Task 2: Enquadramento temporário da câmera

**Files:**
- Modify: `Assets/Scripts/CameraJogador.cs`
- Test: `Assets/Editor/Testes/CameraJogadorTestes.cs` (novo)

**Interfaces:**
- Consumes: `PoseDePerto` (Task 1).
- Produces, em `CameraJogador`:

```csharp
public const float SegundosDeIda = 0.3f;
public const float SegundosDeVolta = 0.25f;

/* Leva a câmera até a pose e a mantém lá. Chamado no meio da volta, continua de onde está. */
public void Enquadrar(PoseDePerto pose);

/* Devolve a câmera aos olhos. Chamado no meio da ida, volta de onde está. */
public void SoltarEnquadramento();

/* Indo, parada na pose ou voltando. Falso só com a câmera de volta aos olhos. */
public bool Enquadrada { get; }

/* O corpo do LateUpdate. dt move o balanço e a mola; dtReal (sem escala) move o enquadramento. */
internal void Posicionar(float dt, float dtReal);
```

- [ ] **Step 1: Escrever os testes**

Montagem: `jogador = TesteUtil.Com<ControladorJogador>(...)` em (10, 0, 5); `camera = TesteUtil.Com<CameraJogador>(...)`; `TesteUtil.Setar(camera, "jogador", jogador)`; `camera.FovBase = 70f`. `Olhos` = (10, 1.63, 5.10), que é o pivô de 1,5 m mais os olhos (0,10 à frente, 0,13 acima) olhando reto. `P` = pose em (12, 0.9, 6), `Giro = LookRotation(Vector3.down, Vector3.forward)`, `Fov = 40`. `Chegar()` = `Enquadrar(P)` e `Posicionar(Q, 1f)`.

| Teste | Assertivas |
|---|---|
| `SemEnquadrarFicaNosOlhos` | `Posicionar(Q, Q)` → posição `Olhos` (1e-3), `!Enquadrada`, `fieldOfView == 70` |
| `EnquadradaChegaAPosePedida` | `Enquadrar(P)`; `Posicionar(Q, 0.15f)` → `Enquadrada`, posição a mais de 0,01 de `Olhos` e de `P.Posicao`; `Posicionar(Q, 0.15f)` → posição `P.Posicao` (1e-4), `Quaternion.Angle(rotation, P.Giro) < 0.01`, `fieldOfView == 40` |
| `SoltaVoltaAosOlhos` | `Chegar()`; `SoltarEnquadramento()` → `Enquadrada` ainda; `Posicionar(Q, 0.125f)` → no meio do caminho; `Posicionar(Q, 0.125f)` → `Olhos`, `fieldOfView == 70`, `!Enquadrada` |
| `SoltaComOJogoPausadoVolta` | `Chegar()`; soltar; `Posicionar(0f, 0.25f)` → `Olhos`, `!Enquadrada` |
| `SoltarNoMeioDaIdaVoltaDeOndeEstava` | `Enquadrar(P)`; `Posicionar(Q, 0.15f)`, guardar a posição `A`; soltar; `Posicionar(Q, 0.0001f)` → a menos de 0,01 de `A`; `Posicionar(Q, 0.25f)` → `Olhos` |
| `EnquadrarDeNovoNaVoltaNaoSalta` | `Chegar()`; soltar; `Posicionar(Q, 0.125f)`, guardar `A`; `Enquadrar(P)`; `Posicionar(Q, 0.0001f)` → a menos de 0,01 de `A`; `Posicionar(Q, 0.3f)` → `P.Posicao` |
| `FovDoMenuNaoMexeNoEnquadramento` | `Chegar()`; `FovBase = 90f` → `fieldOfView == 40`; soltar; `Posicionar(Q, 1f)` → `fieldOfView == 90` |
| `DtRealInvalidoNaoAnda` | `Enquadrar(P)`; `Posicionar(Q, 0f)`, `(Q, NaN)`, `(Q, −1f)` → posição `Olhos`, `Enquadrada` |
| `SemJogadorNaoLanca` | câmera sem `jogador`: `Enquadrar(P)` e `Posicionar(Q, Q)` não lançam |

- [ ] **Step 2: Rodar e ver falhar**

Run: `unity command run_tests --mode EditMode --filter_type category --filter Minigames`
Expected: erro de compilação, porque `Enquadrar` não existe.

- [ ] **Step 3: Implementar**

- `LateUpdate` passa a ser `Posicionar(Time.deltaTime, Time.unscaledDeltaTime)`. O cálculo de hoje (balanço, afundada, pose dos olhos, FOV da corrida) fica como está, mas guarda o resultado em variáveis locais em vez de escrever no `transform`.
- A câmera é achada no primeiro uso (`GetComponent<Camera>()`), porque o `Awake` não roda no EditMode.
- O FOV dos olhos ganha um campo próprio (`fovDosOlhos`), suavizado como hoje. O setter de `FovBase` atualiza esse campo e só escreve em `cam.fieldOfView` com a câmera fora do enquadramento.
- Enquadramento: um peso de 0 a 1 que anda com `dtReal` para 1 em `SegundosDeIda` (pedido ativo) ou para 0 em `SegundosDeVolta` (solto). `dtReal` não finito ou ≤ 0 não anda. Com `s = SmoothStep(0, 1, peso)`: posição = `Lerp(olhos, pose.Posicao, s)`, giro = `Slerp(giroDosOlhos, pose.Giro, s)`, FOV = `Lerp(fovDosOlhos, pose.Fov, s)`.
- `Enquadrada` = pedido ativo ou peso > 0.
- Atualizar o cabeçalho do arquivo.

- [ ] **Step 4: Rodar e ver passar**

Run: `unity command run_tests --mode EditMode`
Expected: nenhuma falha e `Total > 0` (o `FeedbackDeCargaTestes` também usa esta câmera).

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/CameraJogador.cs Assets/Editor/Testes/CameraJogadorTestes.cs*
git commit -m "Câmera: enquadramento temporário para a vista de perto"
```

### Task 3: `Interactor` conduz a vista de perto

**Files:**
- Modify: `Assets/Scripts/Interactor.cs`, `Assets/Scripts/EntradaJogador.cs`, `Assets/Scripts/Interactable.cs`
- Test: `Assets/Editor/Testes/MinigameNoInteractorTestes.cs`

**Interfaces:**
- Consumes: `IMinigameDePerto`, `PoseDePerto`, `PontosDeBorrifar` (Task 1); `CameraJogador.Enquadrar`, `SoltarEnquadramento`, `Enquadrada` (Task 2).
- Produces:

```csharp
// EntradaJogador
/* O Olhar deste quadro sem o "inverter Y" (o ponteiro dos minigames). Zero com Habilitado falso. */
public Vector2 OlharSemInverter { get; private set; }

// Interactor
public PontosDeBorrifar PontosAtivos { get; }   // minigame as PontosDeBorrifar
/* Vista de perto: sessão IMinigameDePerto em andamento, ou a câmera ainda voltando dela. */
public bool DePerto { get; }
internal bool Voltando { get; }
/* A câmera que enquadra. Vazia = a da Camera.main, achada no primeiro uso. */
internal CameraJogador CameraDePerto { get; set; }
/* Libera o agente quando a câmera chega aos olhos. Chamado no começo do Update. */
internal void AtualizarVolta();
/* Com a câmera voltando: larga o alvo, zera o segurar e devolve true (o Update sai aí). */
internal bool LargarSeVoltando();

// Interactable
/* A vista de perto deste foco acabou e a câmera já voltou aos olhos (ou não há câmera para voltar). */
public virtual void AoVoltarDaVistaDePerto() { }
```

- [ ] **Step 1: Escrever os testes**

Em `MinigameNoInteractorTestes`, acrescentar:

```csharp
/* Sessão falsa que pede a vista de perto. */
private class MinigameDePertoFalso : MinigameFalso, IMinigameDePerto
{
    public PoseDePerto Pose => new PoseDePerto
    {
        Posicao = new Vector3(2f, 0.9f, 1f),
        Giro = Quaternion.LookRotation(Vector3.down, Vector3.forward),
        Fov = 40f
    };
}

/* Câmera de teste ligada ao Interactor; começa a sessão de perto no foco, com 'fila' na sessão nova. */
private CameraJogador IniciarDePerto(params EstadoDoMinigame[] fila)
{
    CameraJogador camera = TesteUtil.Com<CameraJogador>("Câmera (teste)");
    TesteUtil.Setar(camera, "jogador", jogador);
    interactor.CameraDePerto = camera;
    foco.Sessao = new MinigameDePertoFalso();
    foreach (EstadoDoMinigame estado in fila) foco.Sessao.Fila.Enqueue(estado);
    Assert.IsTrue(interactor.IniciarMinigame(foco));
    return camera;
}
```

"A câmera volta" = `camera.Posicionar(1f / 60f, 1f)` seguido de `interactor.AtualizarVolta()`. O `FocoDeTeste` ganha o contador `Voltas`, somado no `AoVoltarDaVistaDePerto`.

| Teste | Assertivas |
|---|---|
| `SessaoDePertoPedeOEnquadramento` | `IniciarDePerto()` → `camera.Enquadrada`, `interactor.DePerto`, `jogador.Imobilizado` |
| `SessaoComumNaoEnquadra` | com a câmera de teste ligada e o `MinigameFalso` de sempre → `!camera.Enquadrada`, `!interactor.DePerto` |
| `ConcluirSoltaESeguraOAgenteAteACameraVoltar` | `requiredClicks` 1; `IniciarDePerto(EstadoDoMinigame.Passo)`; `camera.Posicionar(Q, 1f)`; `Quadro()` → `MinigameAtivo == null`, `Voltando`, `DePerto`, `jogador.Imobilizado`, `foco.Voltas == 0`; `AtualizarVolta()` sem mover a câmera → nada muda; a câmera volta → `!Voltando`, `!DePerto`, `!jogador.Imobilizado`, `foco.Voltas == 1` |
| `SemMiraDuranteAVolta` | depois de concluir (`Voltando`): `LargarSeVoltando()` devolve true, `CurrentTarget == null`, `ProgressoContinuo == 0`, `!Avancando`; a câmera volta → `LargarSeVoltando()` devolve false |
| `SairSolta` | `Quadro(sair: true)` → `Voltando`, `foco.Saidas == 1`; a câmera volta → agente livre |
| `BloquearSolta` | `Bloqueado = true`; `AoBloquear()` → `MinigameAtivo == null`, `Voltando`, `foco.Saidas == 1`; a câmera volta → agente livre |
| `RecusaNoMeioSolta` | `IniciarDePerto(EstadoDoMinigame.Passo)`; `kit.Selecionar(Ferramenta.Tela)`; `Quadro()` → `MinigameAtivo == null`, `Voltando` |
| `AlvoDesativadoSolta` | `foco.gameObject.SetActive(false)`; `Quadro()` → `MinigameAtivo == null`, `Voltando` |
| `AlvoDestruidoSolta` | `Object.DestroyImmediate(foco.gameObject)`; `Quadro()` não lança; `Voltando`; a câmera volta → agente livre |
| `CameraDesligadaNaoPrendeOAgente` | `IniciarDePerto()`; `camera.enabled = false`; `Quadro(sair: true)` → `!Voltando`, `!jogador.Imobilizado`, `foco.Voltas == 1`; `camera.enabled = true`; `camera.Posicionar(Q, 1f)` → `!camera.Enquadrada` |
| `NovaSessaoNaVoltaEnquadraDeNovo` | sair (`Voltando`); `IniciarMinigame(foco)` de novo → `!Voltando`, `camera.Enquadrada`, `jogador.Imobilizado` |

Os testes que existem continuam como estão, inclusive `MiraAplicadaNoOlhar` (sai na Task 4).

- [ ] **Step 2: Rodar e ver falhar**

Run: `unity command run_tests --mode EditMode --filter_type category --filter Minigames`
Expected: erro de compilação, porque `CameraDePerto` não existe.

- [ ] **Step 3: Implementar**

**`EntradaJogador`:** `LerOlhar` calcula os graus como hoje, guarda-os em `OlharSemInverter` e só então aplica o `inverterY` para o `Olhar`. Com `Habilitado` falso ou sem a ação de olhar, os dois ficam zero.

**`Interactor`:**
- `LerEntradaDoMinigame` preenche `Ponteiro` com `entrada.OlharSemInverter` (zero sem `EntradaJogador`).
- "Câmera ativa" = `CameraDePerto` não nula, `enabled` e com o objeto ativo na hierarquia.
- `IniciarMinigame`: zera o `voltando`; se a sessão criada é `IMinigameDePerto` e a câmera está ativa, `CameraDePerto.Enquadrar(pose)`.
- `EncerrarMinigame`: se a sessão que acabou era `IMinigameDePerto`, chama `SoltarEnquadramento()` sempre que `CameraDePerto` não for nula, mesmo desligada. Com a câmera ativa, `voltando = true`, o foco da sessão fica guardado e o `Imobilizado` **continua true**. Sem câmera ativa, libera o agente e chama `AoVoltarDaVistaDePerto()` do foco na hora (se ele ainda existir). Sessão comum: libera o agente como hoje.
- `AtualizarVolta`: com `voltando` e a câmera inativa ou já fora do enquadramento, `voltando = false`, `Imobilizado = false` e `AoVoltarDaVistaDePerto()` no foco guardado (se ele ainda existir).
- `Update`: a primeira linha é `AtualizarVolta()`. Depois do bloco do `Bloqueado` e antes do bloco do minigame: `if (LargarSeVoltando()) return;`. O `LargarSeVoltando` larga o alvo e zera o segurar, o progresso e o `Avancando` (a mesma limpeza do `AoBloquear`, sem o minigame). É isso que impede o raio da mira de sair da câmera ainda sobre a planta.
- Atualizar o cabeçalho do arquivo: a vista de perto e a volta.

- [ ] **Step 4: Rodar e ver passar**

Run: `unity command run_tests --mode EditMode --filter_type category --filter Minigames`
Expected: todos passam e `Total > 0`.

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/Interactor.cs Assets/Scripts/EntradaJogador.cs Assets/Scripts/Interactable.cs Assets/Editor/Testes/MinigameNoInteractorTestes.cs
git commit -m "Interactor: vista de perto pedida pela sessão, agente parado até a câmera voltar"
```

### Task 4: A bromélia cria a sessão nova; o minigame antigo sai

**Files:**
- Modify: `Assets/Scripts/BromeliaInteractable.cs`, `Assets/Scripts/EnxameDeMosquitos.cs`, `Assets/Scripts/Interactor.cs`, `Assets/Scripts/Minigame.cs`, `Assets/Scripts/ControladorJogador.cs`
- Delete: `Assets/Scripts/MiraDoBorrifador.cs`, `Assets/Editor/Testes/MiraDoBorrifadorTestes.cs` (e os `.meta`)
- Test: `Assets/Editor/Testes/BromeliaBorrifarTestes.cs` (reescrito), `Assets/Editor/Testes/MinigameNoInteractorTestes.cs`

**Interfaces:**
- Consumes: Tasks 1 e 3.
- Produces, em `BromeliaInteractable.cs`:

```csharp
/* A sessão da bromélia: os pontos, a pose da câmera e qual pocinha é cada ponto. */
internal sealed class SessaoDaBromelia : PontosDeBorrifar, IMinigameDePerto
{
    public PoseDePerto Pose { get; }
    internal int[] Pocas { get; }   // Pocas[k] = índice em BromeliaInteractable.pocas do ponto k
}

// BromeliaInteractable
internal PoseDePerto PoseDaVistaDePerto(Vector3 agente, Vector3 frenteReserva);
internal SessaoDaBromelia CriarSessao(Vector3 agente, Vector3 frenteReserva, float proporcao, int semente);
/* Índice em 'pocas' do ponto k da sessão em andamento; −1 sem sessão ou fora do intervalo. */
internal int PocaDoPonto(int k);
/* O que o OnDisable faz: larga a sessão em andamento. */
internal void AoDesativar();
```

- `EnxameDeMosquitos.NomeDoFilho` passa de `private const` a `internal const`.

- [ ] **Step 1: Conferir o que há acima das 4 bromélias da cena**

Com a `SampleScene` aberta no Editor, fora do Play, rodar por `eval_file`:

```csharp
var sb = new System.Text.StringBuilder();
Physics.SyncTransforms();
foreach (var b in UnityEngine.Object.FindObjectsByType<BromeliaInteractable>(FindObjectsSortMode.None))
{
    Vector3 copo = b.transform.position;
    foreach (Transform t in b.GetComponentsInChildren<Transform>(true))
        if (t.name == "Agua_Centro") copo = t.position;
    string achou = "livre";
    foreach (var h in Physics.SphereCastAll(copo + Vector3.up * 0.3f, 0.2f, Vector3.up, 0.5f, ~0, QueryTriggerInteraction.Ignore))
        if (h.collider.GetComponentInParent<BromeliaInteractable>() != b)
            achou = h.collider.name + " a " + h.distance.ToString("F2") + " m";
    sb.AppendLine(b.name + " (y=" + b.transform.position.y.ToString("F2") + "): " + achou);
}
return sb.ToString();
```

Expected: quatro linhas, todas `livre`. Se alguma não for, **parar e mostrar a saída ao usuário**: o design fixa a altura em 0,58 m e o FOV em 40°, e a saída que ele prevê (baixar a câmera e abrir o FOV na mesma proporção) mexe também no borrifador de perto. Depois, abrir a cena vazia para os testes.

- [ ] **Step 2: Reescrever os testes**

Montagem do `BromeliaBorrifarTestes`: jogador com `KitDoAgente` (água sanitária selecionada) e `Interactor`, como hoje, mais `interactor.CameraDePerto` = uma `CameraJogador` de teste. A `Camera.main` segue a regra de hoje (a da cena ou uma de teste com a tag). Bromélia em `Origem` = (20, 0, 20) e jogador em `Origem + (0, 0, −1.5)`.

```csharp
private const float Q = 1f / 60f;
/* Deslocamentos a partir da bromélia; a última é o copo. */
private static readonly Vector3[] TresPocinhas =
    { new Vector3(-0.06f, 0.3f, 0.05f), new Vector3(0.06f, 0.3f, 0.05f), new Vector3(0f, 0.3f, 0f) };
private static readonly Vector3[] CincoPocinhas =
    { new Vector3(-0.08f, 0.3f, 0.03f), new Vector3(0.08f, 0.3f, 0.03f), new Vector3(-0.05f, 0.3f, -0.07f),
      new Vector3(0.05f, 0.3f, -0.07f), new Vector3(0f, 0.3f, 0f) };

/* Começa pelo Interactor e passa da espera inicial. */
private SessaoDaBromelia Iniciar()
{
    Assert.IsTrue(interactor.IniciarMinigame(bromelia), "a bromélia devia ter minigame");
    Esperar(0.35f);
    return (SessaoDaBromelia)interactor.MinigameAtivo;
}

private void Esperar(float segundos)
{
    for (float t = 0f; t < segundos && interactor.MinigameAtivo != null; t += Q)
        interactor.AvancarMinigame(Q, new EntradaDoMinigame(), false);
}

/* Leva a mira à bolinha da vez e aperta, num quadro só. */
private void Acertar(SessaoDaBromelia s)
{
    Vector2 ponteiro = (s.Ponto(s.DaVez) - s.Mira) / PontosDeBorrifar.MiraPorGrauPadrao;
    interactor.AvancarMinigame(Q, new EntradaDoMinigame { Ponteiro = ponteiro, Apertou = true }, false);
}

private void Sair() => interactor.AvancarMinigame(Q, new EntradaDoMinigame(), true);
```

`Montar`, `DefinirPocas` e a limpeza ficam como hoje. "Enxame" = um filho da bromélia chamado `EnxameDeMosquitos.NomeDoFilho`, criado pelo teste. "A câmera volta" = `Posicionar(Q, 1f)` na câmera de teste e `interactor.AtualizarVolta()`. Cada caso de um teste que repete situações usa uma bromélia nova (`Montar` de novo).

| Teste | Assertivas |
|---|---|
| `CriaASessaoComAsPocinhasComAgua` | `CriarMinigame(kit, 1)` é `SessaoDaBromelia` com `Total == 3`; com `pocas[0]` inativa, `Total == 2` |
| `CopoPorUltimoENoCentroDoCampo` | para as sementes 0 a 9: `Pocas[Total − 1] == 2` e `Ponto(Total − 1) == (0, 0)` (1e-4) |
| `MesmaSementeMesmaOrdem` | com `CincoPocinhas`: `CriarSessao(..., 7)` duas vezes dá o mesmo `Pocas`; entre as sementes 0 a 19 há pelo menos duas ordens diferentes |
| `PontosSaoAProjecaoDasPocinhas` | para todo k: `Ponto(k) == Pose.NoCampo(pocas[Pocas[k]].transform.position)` |
| `PoseAcimaDoCopoComABaseDaTelaDoLadoDoAgente` | `PoseDaVistaDePerto(Origem + (0, 0, −1.5), Vector3.forward)`: `Posicao == copo + (0, 0.58, −0.17)`; `Fov == 40`; `NoCampo(copo) == (0, 0)`; `NoCampo(copo + (0, 0, −0.05)).y < 0`; `NoCampo(copo + (0.05, 0, 0)).x > 0` |
| `AgenteEmCimaDaPlantaUsaAFrenteReserva` | `PoseDaVistaDePerto(copo sem a altura, Vector3.right)`: `Posicao == copo + (−0.17, 0.58, 0)`, giro finito |
| `ProporcaoDaTelaChegaASessao` | `CriarSessao(agente, frente, 4f / 3f, 0)`; um `Atualizar` com `Ponteiro = (1e6, 0)` → `Mira.x == 4/3` |
| `AcertoSecaAPocinhaCerta` | `Iniciar()`; `i = PocaDoPonto(0)`; `Acertar(s)` → `pocas[i]` inativa, as outras duas ativas, `CurrentClicks == 1`, `MinigameAtivo != null` |
| `CliqueForaNaoSecaNada` | aperto com a mira em (1.2, 0.9) → as três ativas, `CurrentClicks == 0` |
| `UltimoAcertoSoConcluiDepoisDoFecho` | três `Acertar` → as três inativas, `!IsComplete`, `MinigameAtivo != null`; `Esperar(0.5f)` → `IsComplete`, `MinigameAtivo == null` |
| `UltimoPassoSecaOQueSobrou` | `AoIniciarMinigame(CriarSessao(...))`; dois `AvancarCom(kit)` → `Avancou`, todas ativas; o terceiro → `Concluiu`, todas inativas |
| `SairGuardaAsSecas` | um acerto; `Sair()` → `MinigameAtivo == null`; a pocinha acertada inativa e com a escala original; as outras ativas; `CurrentClicks == 1` |
| `RetomarUsaSoOQueFalta` | `CincoPocinhas`, 5 cliques: dois acertos e `Sair()`; nova sessão com `Total == 3` e o copo por último; três acertos e `Esperar(0.5f)` → `IsComplete`, `CurrentClicks == 5` |
| `SairNoFechoDevolveAAguaDoCopo` | três acertos; `Sair()` antes do fecho → `!IsComplete`, `CurrentClicks == 2`, o copo (`pocas[2]`) ativo e com a escala original, as outras inativas; `CriarMinigame(kit, 0)` tem `Total == 1` |
| `BloqueioNoFechoDevolveAAguaDoCopo` | o mesmo, com `interactor.AoBloquear()` no lugar de `Sair()` |
| `DesativadaNoMeioLargaASessao` | com "Enxame"; `Iniciar()`; `bromelia.AoDesativar()` → o "Enxame" ativo na hora e `PocaDoPonto(0) == −1`; `AoDesativar()` sem sessão não muda pocinha nenhuma |
| `EnxameSomeAoComecarEVoltaComACamera` | com "Enxame": inativo depois de `Iniciar()`; ainda inativo depois de `Sair()` (a câmera está voltando); ativo depois de a câmera voltar. Repetir para: `AoBloquear()`; concluir (três acertos e `Esperar(0.5f)`); recusa no meio (`kit.Selecionar(Ferramenta.Areia)` e um acerto) |
| `EnxameQueNasceNoMeioTambemSome` | `Iniciar()`; criar o "Enxame"; `Esperar(Q)` → inativo |
| `DoseSoNoFim` | a quantidade de água sanitária não muda nos dois primeiros acertos nem no terceiro; depois de `Esperar(0.5f)`, uma a menos |
| `ConviteEDica` | o convite de hoje; `DicaDoMinigame == "Clique nas bolinhas para borrifar   |   Botão direito: sair"`; convite null com outra ferramenta e sem kit |
| `SegundosEstimadosPelasPocinhas` | três pocinhas → 3.36 (0,75 + 0,87 × 3); cinco → 5.1; sem pocinhas → o da base |
| `SemPocinhasNaoTemMinigame`, `SemFerramentaOuSemCameraNaoTemMinigame`, `NascidaSecaNaoTemMinigame` | como hoje, trocando `CriarMira(...)` por `CriarMinigame(kit, 0)` |
| `CaminhoAntigoMantemAOrdemFixa` | como hoje |
| `CaminhoAntigoPulaAsJaSecas` | como hoje, secando com `Acertar` as que a sessão trouxer primeiro e conferindo que o `AvancarCom` sem sessão seca a próxima ativa do array |
| `GanchosForaDeOrdemNaoLancam` | como hoje, com `CriarSessao` no lugar de `CriarMira`; `AoAtualizarMinigame` com a sessão de outra bromélia não seca nada |

Saem os testes de ângulo, de umidade e do prefab real de hoje (`AngulosDaPocinha`, `AvancandoSoSegurando`, `VisualSegueAUmidade`, `SairGuardaAsSecasEDevolveAMeia`, os três `PrefabReal_*`/`PitchInicial45_*`, `MiraPresaAosLimitesDoPitch`, `SensibilidadeDaMiraChegaASessao`). O prefab real volta na Task 5.

Em `MinigameNoInteractorTestes`: saem `MinigameComMiraFalso` e `MiraAplicadaNoOlhar`.

- [ ] **Step 3: Rodar e ver falhar**

Run: `unity command run_tests --mode EditMode --filter_type category --filter Minigames`
Expected: erro de compilação, porque `SessaoDaBromelia` não existe.

- [ ] **Step 4: Implementar a bromélia**

**Campos.** Saem `segundosPorPoca`, `segundosDoCopoCentral`, `raioDeAcerto`, `coneDaMira`, `sensibilidadeDaMira` e a constante `SegundosDeMirarPorPoca`. Entram, sob `[Header("Vista de perto (minigame)")]`, com `Tooltip` e os padrões da tabela de valores: `alturaDaCamera`, `recuoDaCamera`, `fovDePerto`, `raioDaBolinha`, `segundosDoAnel`, `pausaDaBolinha`, `esperaInicial`, `fecho`, `miraPorGrau`. Constante nova: `SegundosPorBolinha = 0.87f`.

**`PoseDaVistaDePerto`.** `copo` = posição da última de `pocas` (sem ela, a do próprio objeto). `frente` = `copo − agente` sem o `y`, normalizada; quase nula, a `frenteReserva` sem o `y`; ainda nula, `Vector3.forward`. Posição = `copo + up × alturaDaCamera − frente × recuoDaCamera`. Giro = `LookRotation(copo − posição, frente)`. FOV = `fovDePerto`.

**`CriarSessao`.** Fábrica pura. Junta os índices das pocinhas com água na ordem de `pocas`; embaralha os que não são o copo com `new System.Random(semente)`; o copo (o último índice de `pocas`), se tiver água, vai por último. Cada ponto é `pose.NoCampo` da pocinha. `passos = Max(1, RequiredClicks − CurrentClicks)`. A `proporcao` vira a meia-largura do campo.

**`CriarMinigame`.** As condições de hoje (`TemMinigame(kit)` e `Camera.main` não nula) e então `CriarSessao(kit.transform.position, kit.transform.forward, Camera.main.aspect, semente)`.

**Sessão em andamento.** Um campo para a `SessaoDaBromelia` e um contador dos acertos já secos.
- `AoIniciarMinigame`: larga a anterior; guarda a nova se for `SessaoDaBromelia`; esconde o enxame.
- `AoAtualizarMinigame` (só para a sessão guardada): para cada acerto ainda não visto, `SecarPoca` da pocinha daquele ponto; esconde o enxame de novo (ele pode nascer no meio).
- `OnInteract` com sessão: como hoje. O passo que conclui larga a sessão e desativa o que sobrou.
- `AoSairDoMinigame`, a recusa no meio (no `AvancarCom`, resultado diferente de `Avancou` com sessão em andamento) e o `AoDesativar` (com sessão): largam a sessão e chamam a garantia de água.
- Largar a sessão: zera os dois campos. O enxame **continua escondido**: a câmera ainda vai atravessar o lugar dele na volta.
- `AoVoltarDaVistaDePerto` (Task 3) e o `AoDesativar` devolvem o enxame.
- `void OnDisable()` chama `AoDesativar()`.

**Garantia de água.** Se o foco não concluiu, ainda aceita passos e nenhuma pocinha tem água, o copo volta a ter. É o caso da interrupção durante o fecho.

**Secar com volta.** `secando` vira `Dictionary<GameObject, Vector3>` (a pocinha e a escala de antes). A corrotina `Secar` para sem desativar se a pocinha saiu de `secando`. "Devolver a água" a uma pocinha = tirá-la de `secando` restaurando a escala guardada, e ativá-la. `OnStart` e `AoReencher` devolvem a água assim a todas, no lugar do `secando.Clear()`.

**Enxame.** Esconder ou devolver = `SetActive` no filho `EnxameDeMosquitos.NomeDoFilho`, se existir.

**Textos e tempo.** `DicaDeBorrifar` com o texto novo. `SegundosEstimados` = `esperaInicial + fecho + SegundosPorBolinha × pocinhas` (sem pocinhas, o da base).

**Cabeçalho do arquivo:** descrever a vista de perto, as bolinhas, o fecho e a garantia de água.

- [ ] **Step 5: Remover o minigame antigo**

- Apagar `MiraDoBorrifador.cs` e `MiraDoBorrifadorTestes.cs`, com os `.meta`.
- `Minigame.cs`: sai `IMinigameComMira` e a linha dela no cabeçalho.
- `Interactor.cs`: saem `yawInicial`, `pitchInicial`, a leitura deles no `IniciarMinigame`, o `DefinirOlhar` do `AvancarMinigame` e a menção à mira no cabeçalho.
- `ControladorJogador.cs`: saem as propriedades `PitchMinimo` e `PitchMaximo` (só a bromélia usava). Os campos privados ficam.
- Conferir com uma busca por `MiraDoBorrifador`, `IMinigameComMira` e `CriarMira` em `Assets/`: nenhum resultado.

- [ ] **Step 6: Rodar e ver passar**

Run: `unity command run_tests --mode EditMode`
Expected: nenhuma falha e `Total > 0`.

- [ ] **Step 7: Commit**

```bash
git add -A Assets/Scripts Assets/Editor/Testes
git commit -m "Bromélia: sessão de pontos de borrifar na vista de perto; sai o minigame de mirar"
```

Antes do commit, conferir com `git status` que só entraram os arquivos desta tarefa.

### Task 5: Pocinhas à vista

**Files:**
- Modify: `Assets/Editor/FocosBlenderBuilder.cs`, `Assets/Meus/Assets Gerados/Focos/Bromelia.prefab` (pelo menu), `Ferramentas/Blender/gerar_focos.py` (só um comentário)
- Test: `Assets/Editor/Testes/BromeliaBorrifarTestes.cs`

**Interfaces:**
- Consumes: `BromeliaInteractable.CriarSessao` (Task 4).
- Produces, em `FocosBlenderBuilder`:

```csharp
internal const float AlturaDasPocinhas = 0.22f;   // 0,19 do modelo + 0,03
internal const float EscalaDasPocinhas = 1.5f;

/* Sobe e aumenta Agua_1..Agua_4 sob 'raiz' (valores absolutos: rodar duas vezes dá no mesmo).
 * Devolve quantas ajustou. O copo (Agua_Centro) não muda. */
internal static int AjustarPocinhas(Transform raiz);

[MenuItem("Dengue/Ajustar Pocinhas da Bromélia")]
public static void AjustarPocinhasDoPrefab();   // abre o Bromelia.prefab, ajusta e salva, sem refazer o prefab
```

- [ ] **Step 1: Escrever os testes**

No fim de `BromeliaBorrifarTestes`, com o prefab carregado por `AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Meus/Assets Gerados/Focos/Bromelia.prefab")`:

| Teste | Assertivas |
|---|---|
| `PrefabReal_PocinhasAVista` | no espaço da raiz do prefab, `Agua_1` a `Agua_4` têm `y == 0.22` (1e-3) e escala local 1,5; `Agua_Centro` tem `y == 0.215` e escala 1 |
| `PrefabReal_BolinhasNaoSeSobrepoem` | uma instância do prefab (escondida, destruída no fim) com escala 1,51; para o agente a 1,5 m em 8 direções (de 45° em 45°): `CriarSessao(agente, frente, 16f / 9f, 0)` tem `Total == 5`; o copo em (0, 0) (1e-3); cada ponto de fora entre 0,30 e 0,42 do centro; todo par de pontos a mais de 0,30 um do outro |
| `AjustarPocinhas_Idempotente` | numa raiz de teste com filhos `Agua_1`…`Agua_4` e `Agua_Centro`: `AjustarPocinhas` devolve 4; chamar de novo não muda posição nem escala; `Agua_Centro` intacto |

- [ ] **Step 2: Rodar e ver falhar**

Run: `unity command run_tests --mode EditMode --filter_type category --filter Minigames`
Expected: erro de compilação, porque `AjustarPocinhas` não existe.

- [ ] **Step 3: Implementar e aplicar no prefab**

- `AjustarPocinhas`: para cada `Agua_1`…`Agua_4` achada, põe a posição com `y = AlturaDasPocinhas` **no espaço da raiz** (o `x` e o `z` ficam) e `localScale = Vector3.one * EscalaDasPocinhas`.
- `CriarBromelia` chama `AjustarPocinhas(root.transform)` antes de salvar. Assim uma regeneração futura do prefab já sai certa.
- `AjustarPocinhasDoPrefab`: `PrefabUtility.LoadPrefabContents`, `AjustarPocinhas`, `SaveAsPrefabAsset`, `UnloadPrefabContents`. Não passa pelo `CriarBromelia`, que refaria o prefab e perderia o que outros builders puseram nele.
- `gerar_focos.py`: um comentário na linha das pocinhas dizendo que o Unity as sobe e aumenta (`FocosBlenderBuilder.AjustarPocinhas`). Os números do script não mudam.
- Aplicar, por `eval_file`: `UnityEditor.EditorApplication.ExecuteMenuItem("Dengue/Ajustar Pocinhas da Bromélia"); return "ok";`

- [ ] **Step 4: Rodar e ver passar**

Run: `unity command run_tests --mode EditMode --filter_type category --filter Minigames`
Expected: todos passam e `Total > 0`. Conferir no `git diff` do prefab que mudaram as quatro pocinhas (posição e escala) e, no bloco do `BromeliaInteractable`, só entraram campos que o YAML ainda não tinha, com os valores padrão (os de esvaziar do `Interactable` e os 9 da vista de perto). O prefab passa a gravar esses padrões: mudar um padrão no código depois disso pede mudar também no prefab.

- [ ] **Step 5: Commit**

```bash
git add Assets/Editor/FocosBlenderBuilder.cs "Assets/Meus/Assets Gerados/Focos/Bromelia.prefab" Ferramentas/Blender/gerar_focos.py Assets/Editor/Testes/BromeliaBorrifarTestes.cs
git commit -m "Bromélia: pocinhas de fora mais altas e maiores, à vista de cima"
```

### Task 6: A interface das bolinhas

**Files:**
- Create: `Assets/Scripts/PontosDeBorrifarHUD.cs`
- Modify: `Assets/Scripts/SkillCheckHUD.cs`, `Assets/Scripts/DengueHUD.cs`, `Assets/Scripts/Interactor.cs`
- Test: `Assets/Editor/Testes/PontosDeBorrifarHUDTestes.cs` (novo)

**Interfaces:**
- Consumes: `Interactor.PontosAtivos`, `Interactor.DePerto`, `Interactor.CurrentTarget.DicaDoMinigame`; os membros de leitura de `PontosDeBorrifar` (Task 1).
- Produces:

```csharp
// PontosDeBorrifarHUD
internal const float AlturaDaDica = 172f;       // centro da faixa, a partir da base da tela (1920x1080)
internal const float AlturaDaFaixa = 44f;
internal const float AlturaDaContagem = 212f;   // centro dos pontos
internal const float TamanhoDoPonto = 16f;
/* Do campo para o Canvas: y = ±1 é a borda de cima e a de baixo. */
internal static Vector2 NaTela(Vector2 campo, float alturaDoCanvas);
/* Tamanho do anel em relação à bolinha: 2,2 com o anel cheio, 1 fechado. */
internal static float EscalaDoAnel(float anel);

// SkillCheckHUD: o gerador de sprites que as duas HUDs usam
internal static Sprite GerarAnel(float raioInterno, float raioExterno, float suavidade, List<Object> gerados);

// DengueHUD
internal static bool EscondeMiraEPainel(bool emSkillCheck, bool dePerto);
```

- [ ] **Step 1: Escrever os testes**

| Teste | Assertivas |
|---|---|
| `NaTela_BordasDoCampo` | `NaTela((1, −1), 1080) == (540, −540)`; `NaTela((0, 0), 1080) == (0, 0)`; `NaTela((0.5, 0), 720) == (180, 0)` |
| `EscalaDoAnel_DeCheioAFechado` | `EscalaDoAnel(1) == 2.2`; `(0) == 1`; `(0.5) == 1.6`; `(2) == 2.2`; `(−1) == 1` |
| `DicaAcimaDaBarraDoKit` | `AlturaDaDica − AlturaDaFaixa / 2 > 40 + 96 × 1.1` (a barra do kit com o espaço selecionado) |
| `ContagemAcimaDaDica` | `AlturaDaContagem − TamanhoDoPonto / 2 > AlturaDaDica + AlturaDaFaixa / 2` |
| `BolinhaMaisBaixaNaoEncostaNaContagem` | `540 − (0.42 + PontosDeBorrifar.RaioPadrao) × 540 > AlturaDaContagem + TamanhoDoPonto / 2` (0,42 é o teto da Task 5) |
| `EscondeMiraEPainel` | `(false, false)` → false; `(true, false)`, `(false, true)` e `(true, true)` → true |

- [ ] **Step 2: Rodar e ver falhar**

Run: `unity command run_tests --mode EditMode --filter_type category --filter Minigames`
Expected: erro de compilação, porque `PontosDeBorrifarHUD` não existe.

- [ ] **Step 3: Implementar**

**`SkillCheckHUD.GerarAnel`:** vira `internal static`, recebendo a lista `gerados` onde registra a textura e o sprite (o `CriarSprite` que ele usa vira estático junto, com a mesma lista). O comportamento da `SkillCheckHUD` não muda.

**`PontosDeBorrifarHUD`**, no molde da `SkillCheckHUD`: monta tudo em código no `Start`, destrói no `OnDestroy`, desenha no `LateUpdate` e anima em tempo sem escala.
- Canvas próprio: `ScreenSpaceOverlay`, `sortingOrder` 45 (acima da barra do kit, abaixo do skill check), `CanvasScaler` 1920×1080 com `matchWidthOrHeight` 0,5, `CanvasGroup` sem raycast.
- A sessão é `interactor.PontosAtivos`. Com sessão, a raiz aparece com fade de 0,15 s; sem sessão, some com fade de 0,15 s, mostrando o último estado desenhado, sem ler o `Interactor` (o alvo já é null nessa hora). O texto da dica é guardado enquanto há sessão.
- Ao mudar a referência da sessão (inclusive para null), zeram os acertos já vistos: `Acertos` recomeça do 0 a cada sessão.
- Posições e tamanhos saem do campo com `NaTela(ponto, alturaDoCanvas)`; o diâmetro da bolinha é `2 × sessao.Raio × alturaDoCanvas / 2` (151 px em 1080p).
- Sprites, gerados uma vez com `SkillCheckHUD.GerarAnel`: disco `(−1, 0.97, 0.02)`, borda `(0.82, 0.97, 0.02)`, anel fino `(0.935, 0.985, 0.012)`, halo `(0.30, 0.62, 0.30)`.
- Cores: verde `(0.25, 1, 0.8)`; escuro `(0.04, 0.07, 0.10, 0.72)`; corpo da bolinha `(0.05, 0.40, 0.38, 0.93)`; miolo da mira `(1, 0.93, 0.35)`; ponto apagado branco a 0,30; gotas `(0.75, 1, 0.95, 0.85)`.
- Camadas, de trás para a frente:
  1. véu preto a 0,12 na tela toda;
  2. dica: faixa escura de 880 × `AlturaDaFaixa` com o centro a `AlturaDaDica` da base, texto TextMeshPro branco, negrito, 24 px, com `CurrentTarget.DicaDoMinigame`;
  3. contagem: um ponto de `TamanhoDoPonto` por bolinha, a 34 px um do outro, centrados a `AlturaDaContagem` da base; verde para `k < Acertos`;
  4. a próxima (`Proxima ≥ 0`, inclusive na pausa da bolinha da vez): halo, corpo, borda e número, a 0,5 de transparência, sem anel;
  5. a da vez (`BolinhaNaTela`): halo verde a 0,30 com 2× o diâmetro, corpo, borda branca, número (`DaVez + 1`, branco, negrito, 0,44 do diâmetro) e o anel fino branco com `EscalaDoAnel(Anel)` × o diâmetro;
  6. estouro, a cada acerto novo, no ponto acertado: anel verde que vai de 1× a 1,6× o diâmetro e some em 0,3 s, com 10 gotinhas de 9 a 14 px se afastando;
  7. mira, desde o começo da sessão: sombra preta a 0,35 de 50 px, anel branco de 46 px e miolo amarelo de 18 px, em `NaTela(Mira)`.
- Clique fora não desenha nada aqui (a borrifada é do `BorrifadorDePerto`).

**`Interactor.Start`:** adiciona `PontosDeBorrifarHUD` se o Player não tiver, como já faz com a `SkillCheckHUD`.

**`DengueHUD.Update`:** a condição que hoje é só `emSkillCheck` passa a ser `EscondeMiraEPainel(emSkillCheck, interactor.DePerto)`: sem mira e com o painel do alvo a zero durante a sessão e a volta da câmera.

- [ ] **Step 4: Rodar e ver passar**

Run: `unity command run_tests --mode EditMode`
Expected: nenhuma falha e `Total > 0`.

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/PontosDeBorrifarHUD.cs* Assets/Scripts/SkillCheckHUD.cs Assets/Scripts/DengueHUD.cs Assets/Scripts/Interactor.cs Assets/Editor/Testes/PontosDeBorrifarHUDTestes.cs*
git commit -m "Bromélia: interface das bolinhas, mira, contagem e dica"
```

### Task 7: O borrifador de perto e a mão quieta

**Files:**
- Create: `Assets/Scripts/BorrifadorDePerto.cs`
- Modify: `Assets/Scripts/ItensNaMao.cs`, `Assets/Scripts/RegraDoItemNaMao.cs`, `Assets/Scripts/Interactor.cs`
- Test: `Assets/Editor/Testes/BorrifadorDePertoTestes.cs` (novo, categoria `Minigames`), `Assets/Editor/Testes/RegraDoItemNaMaoTestes.cs`, `Assets/Editor/Testes/ItensNaMaoTestes.cs` (categoria `ItensNaMao`)

**Interfaces:**
- Consumes: `Interactor.PontosAtivos`, `Interactor.DePerto`; `PontosDeBorrifar.Mira` e `Borrifadas`.
- Produces:

```csharp
// BorrifadorDePerto
internal static readonly Vector3 PosicaoNaCamera = new Vector3(0.172f, -0.078f, 0.36f);
internal static readonly Vector3 BicoLocal = new Vector3(0f, 0.034f, 0.088f);
internal const float MergulhoGraus = 30f;
internal const int GotasPorBorrifada = 24;
/* Giro, no espaço da câmera, que aponta o bico para a mira como ela aparece na tela,
 * com 'mergulhoGraus' para dentro da cena. */
internal static Quaternion GiroParaAMira(Vector2 mira, float fov, Vector3 bicoNaCamera, float mergulhoGraus);
/* Cria (uma vez) a cópia do borrifador presa à câmera, inativa. Sem original: null. */
internal GameObject Montar(Transform camera, Transform original);

// ItensNaMao
internal Transform BorrifadorNaMao { get; }
internal bool QuerGesticular();   // hoje é private

// RegraDoItemNaMao
public static bool Gesticula(ItemNaMao item, Ferramenta selecionada, bool skillCheckAtivo,
                             bool avancando, Ferramenta? necessariaDoAlvo, bool dePerto = false);
```

- [ ] **Step 1: Escrever os testes**

`BorrifadorDePertoTestes`. O "original" de teste é um objeto inativo com um filho de `MeshRenderer` (sombra ligada) e um filho `Nevoa` com `ParticleSystem`.

| Teste | Assertivas |
|---|---|
| `GiroApontaParaAMiraNaTela` | `f = GiroParaAMira((0, 0), 40, (0.17, −0.08, 0.36), 30) * Vector3.forward`: `f.x < 0`, `f.y > 0`, `f.z == 0.5` (1e-4). Com a mira em `(1.7, −0.6)`: `f.x > 0` |
| `MiraEmCimaDoBicoNaoQuebra` | com a mira exatamente sobre o bico na tela (`mira = (bico.x, bico.y) / (tan(20°) × bico.z)`), o giro é finito (nenhum componente NaN) |
| `CopiaPresaACameraSemSombraEComUmaNevoaSo` | `Montar(camera.transform, original)`: a cópia é filha da câmera, inativa, em `PosicaoNaCamera`, com escala 1; todo `Renderer` dela com `shadowCastingMode == Off`; exatamente um `ParticleSystem`, com a emissão desligada, `startSize` 0,02 e `startSpeed` 0,8; o original segue inativo e com a sombra ligada; `Montar` de novo devolve a mesma cópia |
| `SemOriginalNaoMonta` | `Montar(camera.transform, null) == null`, sem exceção |

`RegraDoItemNaMaoTestes`:

| Teste | Assertivas |
|---|---|
| `DePertoNaoGesticula` | `Gesticula(Borrifador, AguaSanitaria, false, true, AguaSanitaria, dePerto: true) == false`; o mesmo com `dePerto: false` → true; com `skillCheckAtivo` true e `dePerto` true → false |

`ItensNaMaoTestes`, com um `Interactor` no objeto do jogador ligado ao campo `interactor` do `ItensNaMao`, e um foco de teste que pede água sanitária e devolve uma sessão falsa com `Agindo` true:

| Teste | Assertivas |
|---|---|
| `VistaDePerto_SemGestoNemNevoa` | sessão falsa `IMinigameDePerto`: depois de `IniciarMinigame` e um `AvancarMinigame`, `interactor.Avancando` e `!itens.QuerGesticular()`; `AtualizarGesto(0.1f, itens.QuerGesticular())` → `!nevoa.emission.enabled` |
| `SessaoComum_ContinuaGesticulando` | a mesma montagem com uma sessão falsa sem `IMinigameDePerto` → `itens.QuerGesticular()` |

- [ ] **Step 2: Rodar e ver falhar**

Run: `unity command run_tests --mode EditMode`
Expected: erro de compilação, porque `BorrifadorDePerto` não existe.

- [ ] **Step 3: Implementar**

**`GiroParaAMira`:**

```csharp
float t = Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad);
Vector2 naTela = new Vector2(mira.x * t * bicoNaCamera.z - bicoNaCamera.x,
                             mira.y * t * bicoNaCamera.z - bicoNaCamera.y);
Vector2 lado = naTela.sqrMagnitude > 1e-8f ? naTela.normalized : Vector2.up;
float m = mergulhoGraus * Mathf.Deg2Rad;
Vector3 frente = new Vector3(lado.x * Mathf.Cos(m), lado.y * Mathf.Cos(m), Mathf.Sin(m));
return Quaternion.LookRotation(frente, Vector3.up);
```

**`Montar`:** `Instantiate` do original como filho da câmera, com o nome `BorrifadorDePerto`, `localPosition = PosicaoNaCamera`, `localScale = Vector3.one` e inativo. Todo `Renderer` da cópia sem projetar sombra. O filho `Nevoa` herdado vira a névoa de perto: emissão desligada, `startSize` 0,02, `startSpeed` 0,8. Nenhum outro sistema de partículas é criado.

**O componente** (no Player, `[DefaultExecutionOrder(110)]`, depois da câmera):
- Com `interactor.PontosAtivos` não nulo: monta a cópia na `Camera.main` a partir de `ItensNaMao.BorrifadorNaMao` (se ainda não montou), ativa e faz a entrada, deslizando de `PosicaoNaCamera + (0.05, −0.12, 0)` até `PosicaoNaCamera` em 0,2 s.
- A cada `LateUpdate`: o giro local vai para `GiroParaAMira(sessao.Mira, camera.fieldOfView, PosicaoNaCamera + giroLocalAtual * BicoLocal, MergulhoGraus)` com `Slerp` de fator `1 − exp(−18 × dt sem escala)`.
- A cada `Borrifadas` novo: `Emit(GotasPorBorrifada)` na névoa da cópia. Ao mudar a referência da sessão, a contagem já vista volta a 0.
- Sem sessão: limpa a névoa e desativa a cópia.
- Sem câmera, sem `ItensNaMao` ou sem borrifador na mão: não faz nada.

**`Interactor.Start`:** adiciona `BorrifadorDePerto` se o Player não tiver.

**`RegraDoItemNaMao.Gesticula`:** com `dePerto`, devolve false antes de qualquer outra regra.

**`ItensNaMao`:** `QuerGesticular` passa a `internal` e entrega `interactor.DePerto` à regra. `BorrifadorNaMao` expõe o campo. Atualizar os cabeçalhos dos dois arquivos.

- [ ] **Step 4: Rodar e ver passar**

Run: `unity command run_tests --mode EditMode`
Expected: nenhuma falha e `Total > 0`.

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/BorrifadorDePerto.cs* Assets/Scripts/ItensNaMao.cs Assets/Scripts/RegraDoItemNaMao.cs Assets/Scripts/Interactor.cs Assets/Editor/Testes/BorrifadorDePertoTestes.cs* Assets/Editor/Testes/RegraDoItemNaMaoTestes.cs Assets/Editor/Testes/ItensNaMaoTestes.cs
git commit -m "Bromélia: borrifador de perto preso à câmera; o da mão fica quieto"
```

### Task 8: Verificação em Play e registro

**Files:**
- Modify: `contexto do jogo.md`

- [ ] **Step 1: Verificar em Play, com mouse e teclado virtuais**

Com a `SampleScene` aberta. Mesma técnica do registro de 2026-11-06: `Application.runInBackground` só no teste, abertura pulada, desbloqueio só no teste. A captura do CLI só mostra o que a câmera renderiza: a interface em overlay não aparece nela, então conferir a HUD pelo estado dos objetos (posição dos `RectTransform`, textos, `activeSelf`) e a cena pela captura.

| Situação | Esperado |
|---|---|
| Clique numa bromélia com água, com a água sanitária | O agente para; a câmera desce em cerca de 0,3 s; o borrifador entra pelo canto de baixo, à direita; a mira aparece no centro |
| As 4 bromélias, inclusive a mais alta (y = 1,17) | Nada tapa a vista; as 5 pocinhas aparecem; cada bolinha fica sobre uma pocinha |
| Acertar as 5 | Cada pocinha seca no acerto; a borrifada sai do bico na direção da mira; o som de ação toca a cada aperto e o de passo a cada passo |
| Último acerto | O copo termina de secar e o estouro aparece antes de a câmera voltar; a dose só sai aí |
| Volta da câmera | O agente não anda, a mira normal e o painel do alvo não aparecem até a câmera chegar aos olhos |
| Deixar o anel fechar | A bolinha some e volta no mesmo lugar; nada se perde |
| Clique fora e clique na bolinha apagada | Só a borrifada |
| Botão direito no meio; clicar de novo | Sai guardando as secas; a sessão nova traz só o que falta |
| Botão direito, celular e pausa logo depois do último acerto | A planta não conclui e o copo volta com água |
| Celular e pausa no meio; fim de jogo no meio | Sessão encerrada sem gastar dose; ao fechar, a câmera está nos olhos e o agente anda |
| "Inverter Y" ligado | A mira sobe quando o mouse sobe |
| FOV do menu no mínimo e no máximo | A vista de perto é a mesma |
| Bromélia no estágio de mosquito | Sem mosquitos na frente da câmera; eles voltam depois |
| Agente colado na planta ao clicar | O corpo não entra na vista de perto. Se entrar, parar e mostrar a captura ao usuário: nenhuma tarefa esconde o corpo |
| HUD | A dica e a contagem não cobrem bolinha nem a barra do kit; o borrifador não fica atrás do ícone do celular nem da lista de focos |
| Chuva durante a sessão | A sessão segue |
| Brilho pulsante do alvo e brilho de proximidade, de perto | Não atrapalham a leitura das bolinhas. Se atrapalharem, desligar só durante a sessão e registrar |

Se o som de ação quase não se ouvir nos pulsos curtos, subir `PontosDeBorrifar.SegundosAgindo` (0,2 s) até soar como uma borrifada, e registrar o valor. Nenhum áudio novo. Ao fim, `PlayerSettings.runInBackground = false` e nada salvo na cena.

- [ ] **Step 2: Rodar a suíte inteira**

Run: `unity command run_tests --mode EditMode`
Expected: nenhuma falha e `Total > 0`.

- [ ] **Step 3: Registrar e fazer commit**

Entrada no fim do `contexto do jogo.md`, no formato do Histórico: objetivo, arquivos novos, alterados e apagados, valores aplicados, testes (total da suíte), o que foi verificado em Play e o que não foi (partida real com mouse e com controle, pelo usuário).

```bash
git add "contexto do jogo.md"
git commit -m "Bromélia com pontos de borrifar: verificação e registro"
```
