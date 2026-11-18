# Fotos dos focos na rede social Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Quando o jogador passa 60 s sem eliminar foco, o dono do tipo posta a foto pronta (aprovada pelo usuário) de um foco perto dele; o card mostra a foto, a notificação mostra a miniatura e o foco eliminado carimba o card com "RESOLVIDO".

**Architecture:** Cada foco ganha um `FotoDoFoco` (foto + pose da câmera). A escolha do foco é uma classe pura (`EscolhaDaFoto`); o `HistoriaRedeSocial` usa o relógio da bronca para postar, e o `PostRedeSocial` leva a foto e o foco em campos não serializados até o `FeedRedeSocial` e a `NotificacaoCelular`. Um builder de Editor copia as 31 fotos aprovadas para o projeto, liga cada uma ao seu foco pelo caminho na hierarquia e sabe regerar uma foto.

**Tech Stack:** Unity 6000.5.6f1, URP, uGUI + TextMeshPro, NUnit EditMode, `unity` CLI (Pipeline).

**Spec:** `docs/superpowers/specs/2026-11-17-fotos-dos-focos-design.md`

## Global Constraints

- Branch `fotos-dos-focos`, aberta de `bromelia-pontos-de-borrifar` depois do commit `6926759` (trabalho da outra sessão); o spec está em `378022e`. Commits terminam com `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`.
- Nunca commitar `Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF - Fallback.asset` nem `Assets/a.unity`/`.meta` (não são deste trabalho).
- **Não rodar `Dengue > Criar Celular`** na cena: ele recria o `CelularHUD` e zera a `RedeSocial` editada. As peças novas entram na cena pelo menu idempotente da Task 5.
- As fotos do jogo são **cópias** das 31 em `.superpowers/sdd/2026-11-17-fotos-dos-focos/aprovadas/` (`NN - Tipo - Nome.png` + `poses.tsv`, colunas: nº, caminho na hierarquia, px, py, pz, rx, ry, rz, fov, cultura invariante, separador TAB). O caminho tem espaços significativos (`Garrafa `, `Garrafa  (1)`): nunca `Trim` na coluna do caminho.
- Valores iniciais (padrão no código):

  | Valor | Inicial |
  |---|---|
  | `tempoSemFoco` (já existe; a cena tem 60) | 60 s |
  | `raioLugarConhecido` | 4 m |
  | nova tentativa da foto recusada | 1 s |
  | foto | 720×480, FOV 60, near 0,1, far 400 |
  | moldura no card | altura 237, largura do card |
  | selo | 176×44, cor (0,20; 0,68; 0,30; 0,95), giro 8°, TMP 22 negrito branco, espaçamento 6 |
  | foto carimbada | cor da `RawImage` (0,62; 0,62; 0,62; 1) |
  | pop do selo | escala 1,3 → 1 em 0,25 s, tempo sem escala |
  | banner | 500 px de largura (era 440) |
  | miniatura | 96×64, âncora direita-meio, x −14, com `Mask` |
  | área do texto do banner | direita −118 com foto, −18 sem |
  | cabeçalho do banner | `sizeDelta.x` −190 com foto, −90 sem |

- Textos exatos: selo `"RESOLVIDO"`; cabeçalho com foto `"<b>{autor}</b>  <color=#FFFFFF80>enviou uma foto</color>"`; os textos de `aoMandarFoto` e `aoMandarFotoSemDono` da seção 5 do spec, sem mudar uma vírgula.
- Post da foto: prioridade `PrioridadePost.Normal`, humor `HumorPost.Neutro`, curtidas `-1` (sorteadas pelo `Formatar`).
- Convenções: cabeçalho de comentário em cada arquivo, nomes e comentários em português, lógica pura sem `MonoBehaviour` quando der, testes EditMode em `Assets/Editor/Testes/` com `TesteUtil` (objetos escondidos, `TesteUtil.LimparTudo()` no `[TearDown]`), handlers `internal` para os testes chamarem (o `AssemblyInfo` libera). Categoria NUnit dos testes novos: `[Category("Fotos")]`.
- Testes pelo CLI, no Editor aberto: antes, conferir que não está em Play e abrir uma cena vazia (`EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single)` num `.cs` rodado por `unity command eval_file --file <arquivo>`; a `SampleScene` precisa estar sem mudanças, senão sai uma janela modal). Rodar `unity command run_tests --mode EditMode --filter_type category --filter Fotos` e conferir `Total > 0` (com erro de compilação ele responde sucesso com 0 testes). `eval_file` não enxerga `internal`: usar reflexão.
- Toda alteração entra no Histórico do `contexto do jogo.md` (Task 7). Ao terminar, deixar aberta só a `MenuInicial`.

## Review Focus

- **Dois focos com o mesmo nome** (`--- FOCOS ---/pneu (5)` e `--- FOCOS ---/PilhadePneu/pneu (5)`): cada um recebe a sua foto. Task 6: `Importar_DoisComMesmoNome_LigaPeloCaminho`; Task 6 confere na cena.
- **Celular aberto quando o relógio chegaria ao limite:** o relógio não anda e nenhuma foto sai enquanto ele lê. Task 2: `Empacado_CelularAberto_NaoPosta`.
- **Feed recuperando posts antigos** (o feed nasce depois do post, `Start`) com o foco já eliminado: o card já entra carimbado. Task 3: `Adicionar_FocoJaConcluido_EntraCarimbado`.
- **Post sem foto logo depois de um com foto** (o card é clonado do modelo; o banner é reaproveitado): o card nasce sem moldura e o banner volta sem miniatura, com o cabeçalho e as margens de hoje. Task 3: `Adicionar_SemFoto_SemMoldura`; Task 4: `Banner_PostSemFotoDepoisDeFoto_VoltaAoNormal`.
- **Foco desativado ou destruído durante a partida:** não é escolhido e nada quebra. Task 1: `Escolher_NaoElegivel_FicaDeFora`; Task 2: `Empacado_FocoDestruido_Ignora`.

---

## Mapa de arquivos

| Arquivo | Papel | Task |
|---|---|---|
| `Assets/Scripts/EscolhaDaFoto.cs` (novo) | escolha do foco, lógica pura | 1 |
| `Assets/Scripts/FotoDoFoco.cs` (novo) | foto + pose ligadas ao foco | 2 |
| `Assets/Scripts/RedeSocial.cs` | `PostRedeSocial.foto` e `.foco` (`[NonSerialized]`) | 2 |
| `Assets/Scripts/HistoriaRedeSocial.cs` | relógio → foto, mexidos, textos | 2 |
| `Assets/Scripts/PostView.cs`, `FeedRedeSocial.cs` | foto no card, carimbo | 3 |
| `Assets/Editor/CelularBuilder.cs` | peças da foto no card modelo | 3 |
| `Assets/Scripts/NotificacaoCelular.cs`, `Assets/Editor/HistoriaBuilder.cs` | miniatura e banner de 500 px | 4 |
| `Assets/Editor/FotosDosFocosBuilder.cs` (novo) | importar, gerar, vista da Scene, preparar celular | 5 |
| `Assets/Meus/Assets Gerados/Fotos dos Focos/*.png` (novos), `Assets/Scenes/SampleScene.unity` | as 31 fotos e a cena ligada | 6 |
| `contexto do jogo.md` | registro | 7 |
| testes | `EscolhaDaFotoTestes`, `HistoriaFotoTestes`, `FeedFotoTestes`, `NotificacaoFotoTestes`, `FotosDosFocosBuilderTestes` em `Assets/Editor/Testes/` | 1–5 |

---

### Task 1: Escolha do foco (lógica pura)

**Files:**
- Create: `Assets/Scripts/EscolhaDaFoto.cs`
- Test: `Assets/Editor/Testes/EscolhaDaFotoTestes.cs`

**Interfaces:**
- Produces:
  ```csharp
  public struct CandidatoDaFoto
  {
      public Vector3 posicao;   // posição do foco guardada no início da partida
      public bool elegivel;     // não concluído, ativo, com foto, não mexido
      public bool fotografado;  // já saiu em foto
      public bool conhecido;    // lugar conhecido: fotografado, mexido ou concluído (vale mesmo sem ser elegível)
  }
  public static class EscolhaDaFoto
  {
      // Índice escolhido ou -1.
      public static int Escolher(IReadOnlyList<CandidatoDaFoto> candidatos, Vector3 jogador,
                                 float raioLugarConhecido, int ultimaFoto);
  }
  ```
  Regra: 1ª passada = elegível, não fotografado e com `Vector3.Distance` **maior** que `raioLugarConhecido` até todo outro candidato `conhecido`; vence o menor `Vector3.Distance` ao jogador (empate: menor índice). 2ª passada (nada na 1ª) = elegível e índice ≠ `ultimaFoto`; mesmo critério. Nada: −1.

- [ ] **Step 1: Escrever os testes** (`[Category("Fotos")]`), com candidatos montados à mão:
  - `Escolher_MaisPertoVence`: três elegíveis a 10, 3 e 7 m do jogador → índice do de 3 m.
  - `Escolher_NaoElegivel_FicaDeFora`: o mais perto com `elegivel = false` → o seguinte.
  - `Escolher_PertoDeLugarConhecido_FicaDeFora`: elegível A a 2 m do jogador e a 3 m de um conhecido; elegível B a 6 m do jogador e longe de tudo → B. Com `raioLugarConhecido` 2,5 → A.
  - `Escolher_SegundaPassada_RepeteFotografadoMenosOUltimo`: dois elegíveis, ambos fotografados, `ultimaFoto` = o mais perto → o outro.
  - `Escolher_SoOUltimo_DevolveMenosUm`: um elegível, fotografado, e é o `ultimaFoto` → −1.
  - `Escolher_ListaVazia_DevolveMenosUm`.
- [ ] **Step 2: Rodar e ver falhar** (não compila: `EscolhaDaFoto` não existe).
- [ ] **Step 3: Implementar `EscolhaDaFoto.Escolher`** conforme a regra acima.
- [ ] **Step 4: Rodar a categoria `Fotos`** → 6 passam, `Total > 0`.
- [ ] **Step 5: Commit** `git add Assets/Scripts/EscolhaDaFoto.cs* Assets/Editor/Testes/EscolhaDaFotoTestes.cs*` (com os `.meta` gerados) — mensagem `Fotos dos focos: escolha do foco (lógica pura)`.

---

### Task 2: O narrador posta a foto

**Files:**
- Create: `Assets/Scripts/FotoDoFoco.cs`
- Modify: `Assets/Scripts/RedeSocial.cs` (classe `PostRedeSocial`), `Assets/Scripts/HistoriaRedeSocial.cs` (`PersonagemDoBairro`, `Start`, `Update`, `HandleFocoEsvaziado`, `HandlePneuPego`, `OnDestroy`, `PreencherPadroes`, `CompletarPersonagens`, `PersonagensPadrao`)
- Test: `Assets/Editor/Testes/HistoriaFotoTestes.cs`

**Interfaces:**
- Consumes: `EscolhaDaFoto.Escolher`, `CandidatoDaFoto` (Task 1).
- Produces:
  ```csharp
  [DisallowMultipleComponent]
  public class FotoDoFoco : MonoBehaviour
  {
      public Texture2D foto;
      public Vector3 posicaoDaCamera;
      public Vector3 rotacaoDaCamera;      // Euler
      public float campoDeVisao = 60f;
      // OnDrawGizmosSelected: frustum da câmera da foto (Gizmos.matrix na pose + DrawFrustum, aspecto 1,5, alcance 3 m)
  }
  // em PostRedeSocial:
  [System.NonSerialized] public Texture foto;
  [System.NonSerialized] public Interactable foco;
  // em PersonagemDoBairro:
  [Tooltip("Post com a foto de um foco deste tipo quando o jogador empaca. Segue a ordem e, quando acaba, sorteia sem repetir.")]
  [TextArea(2, 4)] public List<string> aoMandarFoto = new List<string>();
  // em HistoriaRedeSocial:
  [SerializeField] private float raioLugarConhecido = 4f;                 // grupo "Gatilhos"
  [SerializeField] private List<string> aoMandarFotoSemDono;              // autor Upa / UpaUsuario
  internal enum ResultadoDaFoto { Saiu, Recusada, SemFoco }
  internal void RegistrarFocos();                       // a parte do Start que lê o gerenciador; guarda posição de cada foco e o FotoDoFoco
  internal void AvancarEmpacado(float dt, Vector3 posicaoDoJogador, bool lendoCelular);
  internal ResultadoDaFoto TentarFotoDeFoco(Vector3 posicaoDoJogador);
  internal void HandleFocoClicado(Interactable foco);   // marca "mexido"
  internal void HandleTampaPega(CaixaDaguaInteractable caixa); // marca "mexido"
  // HandleFocoEsvaziado (já internal) e HandlePneuPego (vira internal) também marcam "mexido", ANTES dos returns que já existem
  internal Interactable UltimaFoto { get; }
  internal bool FoiFotografado(Interactable foco);
  ```
- `AvancarEmpacado` substitui o bloco "Muito tempo sem eliminar foco" do `Update` (que passa `player.position` e `celular != null && celular.Aberto`; sem `player`, não chama): soma `dt` ao `timerSemFoco` só sem `lendoCelular`; abaixo de `tempoSemFoco`, nada. No limite: se houver espera de nova tentativa em curso, desconta `dt` e só tenta quando ela zerar. `TentarFotoDeFoco`: `Saiu` → zera o relógio; `SemFoco` → `PostarDaLista(aoDemorarSemFoco, ref idxDemora, PrioridadePost.Baixa)` e zera o relógio (como hoje); `Recusada` → relógio fica, espera de 1 s.
- `TentarFotoDeFoco`: monta um `CandidatoDaFoto` por foco registrado (nulo/destruído = `elegivel` falso, `conhecido` falso), chama `Escolher` com `raioLugarConhecido` e o índice da `UltimaFoto`. Autor e texto: dono do tipo com `aoMandarFoto` não vazio; senão `Upa`/`UpaUsuario` com `aoMandarFotoSemDono`. Só gasta o índice da fala se `rede.Aceitaria(PrioridadePost.Normal)`. Post = `Formatar(autor, usuario, texto, HumorPost.Neutro, tipo, -1)` com `foto` e `foco` preenchidos; `rede.Postar(post, PrioridadePost.Normal)`. Saiu: marca fotografado e guarda como `UltimaFoto`.
- `Start` passa a chamar `RegistrarFocos()` e assina `gerenciador.OnFocoClicado` e `kit.OnTampaPega` (soltos no `OnDestroy`).

- [ ] **Step 1: Escrever os testes** (`[Category("Fotos")]`). `[SetUp]` no padrão do `HistoriaChuvaTestes` (rede com `janelaConflito` 0, fluxo em jogo, historia ligada) mais um `GerenciadorDeFocos` de teste no campo `gerenciador`; focos criados com `TesteUtil.Com<VasoInteractable>` / `<PneuInteractable>` / `<CaixaDaguaInteractable>` em posições dadas, cada um com `FotoDoFoco` e uma `Texture2D` 4×4; `gerenciador.Registrar(...)` e `historia.RegistrarFocos()`; concluir um foco como no `GerenciadorDeFocosTestes`. Salvo quando o teste diz outra coisa, os focos ficam a pelo menos 10 m uns dos outros e o jogador na origem.
  - `Empacado_PostaFotoDoMaisPerto`: `AvancarEmpacado(60f, origem, false)` → 1 publicado, `foto` = a textura do foco mais perto, `foco` = ele, `autor` = dono do tipo (Dona Neusa para vaso), `texto` = 1º de `aoMandarFoto` do vaso, `humor` Neutro.
  - `Empacado_AntesDoLimite_NaoPosta`: `AvancarEmpacado(59.9f, …)` → 0 publicados.
  - `Empacado_CelularAberto_NaoPosta`: `AvancarEmpacado(120f, …, true)` → 0 publicados.
  - `Empacado_SegueEmpacado_OutroFoco`: dois `AvancarEmpacado(60f)` → 2 posts, focos diferentes.
  - `Empacado_Recusada_NaoGastaETentaDeNovo`: `janelaConflito` 100 e um `rede.Postar` de prioridade `Alta` antes → `AvancarEmpacado(60f)` não publica, `FoiFotografado` falso; `janelaConflito` 0, `AvancarEmpacado(0.5f)` não publica, `AvancarEmpacado(0.6f)` publica a foto com o **1º** texto.
  - `Empacado_SemFocoComFoto_SaiABronca`: nenhum foco com `FotoDoFoco` → o post é o 1º de `aoDemorarSemFoco` e `foto` nulo.
  - `Mexido_NaoSaiEmFoto`: `HandleFocoClicado` no mais perto → a foto é do outro. Idem com `HandleFocoEsvaziado` (vaso), `HandlePneuPego` (pneu) e `HandleTampaPega` (caixa) — um teste cada.
  - `Empacado_FocoDestruido_Ignora`: destruir (`Object.DestroyImmediate`) o mais perto depois do `RegistrarFocos` → a foto é do outro, sem exceção.
  - `TipoSemDono_PostaComoUpa`: foco `Interactable` puro (tipo `Nenhum`) → autor "UPA Vila Nova", texto de `aoMandarFotoSemDono`.
  - `AoMandarFoto_SegueOrdemDepoisSorteiaSemRepetir`: 6 posts de pneus (Carlos tem 3 textos) → os 3 primeiros em ordem e nenhum par seguido igual.
  - `CompletarPersonagens_PreencheAoMandarFotoVazio`: no padrão do `HistoriaKitTestes`, personagem Ralo salvo com `aoMandarFoto` vazio → preenchido com o texto do Seu Osvaldo; um com texto próprio não muda.
  - `PostRedeSocial_FotoEFocoNaoSaoSerializados`: `JsonUtility.ToJson` de um post com `foto` e `foco` não contém `"foto"` nem `"foco"`.
- [ ] **Step 2: Rodar e ver falhar** (não compila).
- [ ] **Step 3: Implementar** `FotoDoFoco`, os campos de `PostRedeSocial` e de `PersonagemDoBairro`, os textos padrão em `PersonagensPadrao` (os 7 personagens) e `AoMandarFotoSemDonoPadrao()`, `PreencherPadroes` (lista vazia → padrão), `CompletarPersonagens` (`aoMandarFoto` vazio → padrão do tipo), e a lógica do narrador descrita acima.
- [ ] **Step 4: Rodar a categoria `Fotos`** → todos passam; rodar também `ParteA` e `ParteB` (os testes do narrador que já existem) → passam.
- [ ] **Step 5: Commit** dos 3 scripts, do `FotoDoFoco.cs.meta` e do teste — `Fotos dos focos: narrador posta a foto quando o jogador empaca`.

---

### Task 3: A foto e o carimbo no card

**Files:**
- Modify: `Assets/Scripts/PostView.cs`, `Assets/Scripts/FeedRedeSocial.cs`, `Assets/Editor/CelularBuilder.cs` (`MontarCard`)
- Test: `Assets/Editor/Testes/FeedFotoTestes.cs`

**Interfaces:**
- Consumes: `PostRedeSocial.foto`, `.foco` (Task 2).
- Produces:
  ```csharp
  // PostView
  public RectTransform foto;        // moldura "Foto"
  public RawImage fotoImagem;       // "Foto/Imagem"
  public GameObject selo;           // "Foto/Resolvido"
  [HideInInspector] public Interactable foco;
  [HideInInspector] public float animSelo = 1f;   // 0..1 do pop
  // FeedRedeSocial
  [SerializeField] private GerenciadorDeFocos gerenciador;   // vazio = GerenciadorDeFocos.Obter() no Start
  internal void Adicionar(PostRedeSocial post);              // era private
  internal void CarimbarFoco(Interactable foco);             // handler do OnFocoEliminado
  internal static bool Carimbado(PostView pv);               // selo ativo
  // CelularBuilder
  internal static PostView MontarCard(RectTransform pai, Sprite ui, Sprite knob);   // era private
  internal static bool GarantirFotoNoCard(PostView modelo, Sprite ui);              // idempotente; true se criou
  ```
- Hierarquia criada por `GarantirFotoNoCard`, logo depois de `Texto`: `Foto` (`Image` com `ui` fatiado, `pixelsPerUnitMultiplier` 0,5; `Mask` com `showMaskGraphic` falso; `LayoutElement` min/preferred 237; desativado) → `Imagem` (`RawImage` esticada) e `Resolvido` (valores do selo das Global Constraints, desativado) → `Texto` (TMP `"RESOLVIDO"`). `MontarCard` chama `GarantirFotoNoCard` depois de criar o `PostView` (a moldura vai para o índice logo depois de `Texto`, antes de `Acoes`), e o `PostView` recebe as três referências. Fonte do TMP: a mesma dos outros textos do card.
- `Adicionar`: guarda `post.foco`; moldura ativa só com `post.foto`; textura na `RawImage`; cor branca; selo desligado; se `post.foco` já concluído, carimba sem pop. `CarimbarFoco`: para cada card com aquele `foco` e ainda sem selo, cor (0,62…) e selo ligado com `animSelo = 0`. O `Update` anda o `animSelo` com `Time.unscaledDeltaTime / 0.25f` e aplica a escala `Mathf.Lerp(1.3f, 1f, ease out)` no selo. `Start` assina `gerenciador.OnFocoEliminado += CarimbarFoco` e o `OnDestroy` solta (só pelo campo, como na `RedeSocial`).

- [ ] **Step 1: Escrever os testes** (`[Category("Fotos")]`): feed de teste com `viewport`, `conteudo` e `modelo = CelularBuilder.MontarCard(conteudo, null, null)` ligados por `TesteUtil.Setar`; `Adicionar` chamado direto.
  - `MontarCard_TemFotoESeloDesligados`: `modelo.foto`, `fotoImagem`, `selo` não nulos; `foto` e `selo` inativos; `foto` é irmão logo depois de `Texto`.
  - `GarantirFotoNoCard_DuasVezes_NaoDuplica`: num card montado sem foto (apagar `Foto` e zerar as referências), 1ª chamada → true, 2ª → false, um só filho `Foto`.
  - `Adicionar_ComFoto_MostraAFoto`: post com textura → moldura ativa, `fotoImagem.texture` é ela, selo inativo.
  - `Adicionar_SemFoto_SemMoldura`: um post com foto e depois um sem → o card novo tem a moldura inativa.
  - `CarimbarFoco_SoOsCardsDoFoco`: dois posts de focos diferentes → `CarimbarFoco(focoA)` → só o card de A `Carimbado` e com a cor (0,62…).
  - `Adicionar_FocoJaConcluido_EntraCarimbado`.
- [ ] **Step 2: Rodar e ver falhar.**
- [ ] **Step 3: Implementar** conforme as Interfaces.
- [ ] **Step 4: Rodar a categoria `Fotos`** → passa.
- [ ] **Step 5: Commit** — `Fotos dos focos: foto e selo Resolvido no card do feed`.

---

### Task 4: A miniatura na notificação

**Files:**
- Modify: `Assets/Scripts/NotificacaoCelular.cs`, `Assets/Editor/HistoriaBuilder.cs` (`GarantirNotificacao`)
- Test: `Assets/Editor/Testes/NotificacaoFotoTestes.cs`

**Interfaces:**
- Consumes: `PostRedeSocial.foto` (Task 2).
- Produces:
  ```csharp
  // NotificacaoCelular
  [SerializeField] private GameObject miniatura;
  [SerializeField] private RawImage miniaturaImagem;
  internal void HandlePost(PostRedeSocial post);   // era private
  // HistoriaBuilder
  internal static bool GarantirMiniatura(NotificacaoCelular notificacao, Sprite ui);  // idempotente: banner 500 px + "Miniatura"/"Imagem"; true se criou
  ```
- `GarantirNotificacao` cria o banner já com 500 px e chama `GarantirMiniatura`. A miniatura segue os valores das Global Constraints (`Image` fatiado com `pixelsPerUnitMultiplier` 0,8 + `Mask` sem gráfico; `RawImage` esticada; inativa).
- `Mostrar` recebe a foto: com foto, miniatura ativa com a textura, cabeçalho com "enviou uma foto", `texto.rectTransform.offsetMax.x = -118`, `cabecalho.rectTransform.sizeDelta.x = -190`; sem foto, miniatura inativa, cabeçalho de hoje, −18 e −90.

- [ ] **Step 1: Escrever os testes** (`[Category("Fotos")]`): banner de teste montado à mão com os nomes do `HistoriaBuilder` (`Notificacao` 440×104 com `NotificacaoCelular`, filhos TMP `Cabecalho` e `Texto` com as âncoras e offsets de hoje), ligados por `TesteUtil.Setar` (`painel`, `cabecalho`, `texto`), e depois `GarantirMiniatura(notificacao, null)`.
  - `GarantirMiniatura_DuasVezes_NaoDuplica`: true, depois false; um só filho `Miniatura`; largura do banner 500.
  - `Banner_ComFoto_MostraMiniaturaECabecalho`: `HandlePost` de um post com foto → miniatura ativa com a textura; cabeçalho igual a `"<b>Lúcia Andrade</b>  <color=#FFFFFF80>enviou uma foto</color>"`; `offsetMax.x` −118.
  - `Banner_PostSemFotoDepoisDeFoto_VoltaAoNormal`: um com foto, depois um sem → miniatura inativa, cabeçalho `"<b>{autor}</b>  <color=#FFFFFF80>{usuario}</color>"`, −18 e −90.
- [ ] **Step 2: Rodar e ver falhar.**
- [ ] **Step 3: Implementar.**
- [ ] **Step 4: Rodar a categoria `Fotos`** → passa.
- [ ] **Step 5: Commit** — `Fotos dos focos: miniatura da foto na notificação`.

---

### Task 5: O builder das fotos

**Files:**
- Create: `Assets/Editor/FotosDosFocosBuilder.cs`
- Test: `Assets/Editor/Testes/FotosDosFocosBuilderTestes.cs`

**Interfaces:**
- Consumes: `FotoDoFoco` (Task 2), `CelularBuilder.GarantirFotoNoCard` (Task 3), `HistoriaBuilder.GarantirMiniatura` (Task 4).
- Produces:
  ```csharp
  public static class FotosDosFocosBuilder
  {
      internal const string PastaDasFotos = "Assets/Meus/Assets Gerados/Fotos dos Focos";
      internal const string PastaAprovadasPadrao = ".superpowers/sdd/2026-11-17-fotos-dos-focos/aprovadas"; // relativa à raiz do projeto
      internal struct Pose { public int numero; public string caminho; public Vector3 posicao; public Vector3 rotacao; public float fov; }
      internal static string CaminhoNaHierarquia(Transform t);           // "--- FOCOS ---/PilhadePneu/pneu (5)"
      internal static string NomeDoArquivo(string caminho);              // sem o 1º segmento; "/" → " - "; espaços repetidos viram um; trim por segmento; caracteres inválidos de arquivo removidos; + ".png"
      internal static List<Pose> LerPoses(string textoTsv);              // InvariantCulture; ignora linha vazia; caminho sem Trim
      internal static int SementeDe(string caminho);                     // FNV-1a 32 bits sobre os chars, como int não negativo
      internal static void DesenharCirculo(Color32[] px, int largura, int altura, Rect retangulo, int semente); // traço do spec, seção 3
      internal static int Importar(string pastaAprovadas, IEnumerable<Interactable> focos, StringBuilder avisos); // devolve quantos ligou
      internal static Texture2D Renderizar(FotoDoFoco f);                // 720×480, com o círculo; quem chama destrói
      // Menus em "Dengue/Fotos dos Focos/": "Importar fotos aprovadas", "Gerar fotos (todos os focos)",
      // "Gerar foto (focos selecionados)", "Usar a vista da Scene no foco selecionado", "Preparar celular para fotos"
  }
  ```
- `Importar`: para cada `Pose`, acha o foco com `CaminhoNaHierarquia` igual; acha em `pastaAprovadas` o PNG que começa com `numero:00` + `" - "`; copia **os bytes** para `PastaDasFotos/NomeDoArquivo(caminho)` (colisão → `" (2)"` antes de `.png`); importa com Default, sem mipmaps, Clamp, sRGB, máx 1024, compressão normal; põe ou atualiza o `FotoDoFoco` (via `Undo.AddComponent`/`SerializedObject`) com pose, FOV e a textura; marca a cena suja. Foco sem linha ou linha sem foco → linha em `avisos`, segue.
- `Renderizar` e os menus de gerar usam o código de câmera e de círculo do spec, seção 3 (o mesmo de `.superpowers/sdd/2026-11-17-fotos-dos-focos/capturas/scripts/fotos_v2_poses.cs`), com `SementeDe(caminho)`. Gerar grava em `PastaDasFotos` com o `NomeDoArquivo` e liga a textura.
- "Usar a vista da Scene…": `SceneView.lastActiveSceneView.camera` → posição, `eulerAngles`, `fieldOfView` no `FotoDoFoco` do selecionado (cria se faltar).
- "Preparar celular para fotos": acha o `ModeloPost` (pelo `FeedRedeSocial.modelo`) e a `NotificacaoCelular` na cena aberta, chama `GarantirFotoNoCard` e `GarantirMiniatura` com o `UISprite` embutido, liga as referências e marca a cena suja. Loga o que criou.

- [ ] **Step 1: Escrever os testes** (`[Category("Fotos")]`):
  - `NomeDoArquivo_TiraOGrupoEJuntaComTraco`: `"--- FOCOS ---/PilhadePneu/pneu (5)"` → `"PilhadePneu - pneu (5).png"`; `"--- BAIRRO ---/Terrenos/Cenario Terreno Baldio/Lixo/Garrafa  (1)"` → `"Terrenos - Cenario Terreno Baldio - Lixo - Garrafa (1).png"`; `"--- FOCOS ---/Caixa d'Agua (1)"` → `"Caixa d'Agua (1).png"`.
  - `LerPoses_LeTsvComEspacosNoCaminho`: a linha `"07\t--- BAIRRO ---/Terrenos/Cenario Terreno Baldio/Lixo/Garrafa \t-18.114\t1.783\t63.445\t12.794\t195.000\t0.000\t60.0"` → `numero` 7, caminho terminando em `"Garrafa "`, posição (−18,114; 1,783; 63,445), rotação y 195, fov 60.
  - `SementeDe_Estavel`: duas chamadas iguais, caminhos diferentes → sementes diferentes, ambas ≥ 0.
  - `DesenharCirculo_PintaABordaENaoOCentro`: imagem 200×100 cinza, retângulo (90, 40, 20, 20) → o pixel do centro continua cinza; existe pixel (232, 30, 30) a ≥ 25 px do centro.
  - `Importar_DoisComMesmoNome_LigaPeloCaminho`: pasta temporária com `01 - Pneu - pneu (5).png` e `02 - Pneu - pneu (5).png` (PNGs 4×4 de cores diferentes) e `poses.tsv` com `"--- FOCOS ---/pneu (5)"` e `"--- FOCOS ---/PilhadePneu/pneu (5)"`; dois `PneuInteractable` de teste com esses caminhos → cada `FotoDoFoco.foto` tem a cor do seu PNG; 2 ligados. Usar uma `PastaDasFotos` de teste (parâmetro interno ou campo `internal static` trocado no teste) e apagar os assets no `[TearDown]`.
- [ ] **Step 2: Rodar e ver falhar.**
- [ ] **Step 3: Implementar.**
- [ ] **Step 4: Rodar a categoria `Fotos`** → passa.
- [ ] **Step 5: Commit** — `Fotos dos focos: builder (importar, gerar, vista da Scene, preparar celular)`.

---

### Task 6: Ligar as 31 fotos aprovadas na cena

**Files:**
- Create: `Assets/Meus/Assets Gerados/Fotos dos Focos/*.png` (+ `.meta`)
- Modify: `Assets/Scenes/SampleScene.unity`

- [ ] **Step 1: Abrir a `SampleScene`** (Editor fora de Play; a cena aberta sem mudanças) e rodar, por `eval_file`, `EditorApplication.ExecuteMenuItem("Dengue/Fotos dos Focos/Preparar celular para fotos")` e depois `"Dengue/Fotos dos Focos/Importar fotos aprovadas"`. Esperado no Console: card e miniatura criados; 31 focos ligados, nenhum aviso.
- [ ] **Step 2: Conferir as fotos.** Hash de cada PNG em `PastaDasFotos` igual ao do aprovado do mesmo número (`sha256sum` nos dois); 31 `FotoDoFoco` na cena, todos com `foto`; o de `--- FOCOS ---/pneu (5)` aponta para a cópia da 18 e o de `--- FOCOS ---/PilhadePneu/pneu (5)` para a da 19 (por `eval_file` com reflexão).
- [ ] **Step 3: Conferir o gerador.** Por `eval_file`, `Renderizar` nos focos 04, 15 e 24 e comparar com os aprovados pixel a pixel, fora de uma faixa de 12 px em volta do círculo (o círculo do gerador usa outra semente): diferença máxima ≤ 2 por canal. Se diferir, parar e mostrar ao usuário.
- [ ] **Step 4: Salvar a cena** (`EditorSceneManager.SaveScene`) e conferir que o `git diff --stat` só mostra a `SampleScene` e os PNGs novos.
- [ ] **Step 5: Commit** dos PNGs, `.meta`, da pasta `.meta` e da cena — `Fotos dos focos: as 31 fotos aprovadas ligadas aos focos`.

---

### Task 7: Verificação no jogo e registro

**Files:**
- Modify: `contexto do jogo.md`

- [ ] **Step 1: Suíte EditMode inteira** (cena vazia antes): todos passam, `Total > 0`. Anotar o total.
- [ ] **Step 2: Play com a `SampleScene` como cena única**, `Application.runInBackground = true` só durante o teste, abertura pulada como no preview. Sair do hospital (ou `FluxoDeJogo` em jogo), deixar o agente parado 60 s (ou `timerSemFoco` posto no limite por reflexão) → notificação com a miniatura e o card no topo do feed com a foto do foco mais perto; `UltimaFoto` é esse foco. Eliminar esse foco (`Interact()` até concluir) → card carimbado. Repetir o limite → foto de outro foco. Console sem erros novos.
- [ ] **Step 3: Capturas** como no preview (canvases em Screen Space Camera, `Camera.Render` num RT 1920×1080), salvas em `.superpowers/sdd/2026-11-17-fotos-dos-focos/verificacao/`: celular fechado com a notificação, celular aberto com a foto, card carimbado. Sair do Play; `runInBackground` volta a false.
- [ ] **Step 4: Registrar** no fim do `contexto do jogo.md` uma entrada "2026-11-17 — Fotos dos focos: implementação, verificação em Play e registro", no formato das anteriores: objetivo, commits (`git log --oneline 378022e..HEAD`), arquivos novos e alterados, valores, o que foi verificado e o que **não** foi.
- [ ] **Step 5: Deixar aberta só a `MenuInicial`** (`OpenScene(..., Single)`, cena sem mudanças) e commitar o `contexto do jogo.md` — `Fotos dos focos: verificação e registro`.
