# Itens do kit na mão Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Dar modelo 3D, pose e gesto ao balde de areia, à tela com moldura e ao borrifador, e mostrar a mesma tela sob a grelha do bueiro concluído.

**Architecture:** Uma regra C# pura decide o item e o gesto. O componente `ItensNaMao`, no Player, aplica isso aos objetos filhos de `RightHand` e a uma camada nova do Animator, sem tocar no `FeedbackDeCarga`. Modelos e poses saem de scripts do Blender, e o menu `Dengue > Configurar Itens na Mão` monta tudo de forma idempotente.

**Tech Stack:** Unity 6000.5.6f1, URP, NUnit EditMode, Blender 5.2 em modo batch, `unity` CLI.

**Spec:** `docs/superpowers/specs/2026-11-06-itens-do-kit-na-mao-design.md`

## Global Constraints

- Trabalhar na branch `itens-na-mao`, nunca na `main`. Commits terminam com `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`.
- Não mudar `FeedbackDeCarga`, `KitDoAgente`, `Interactor`, `DengueHUD`, `BueiroInteractable` nem `EstoqueDoKit`.
- Não regerar o `Bueiro.fbx`. Não mudar saco, pneu, balanceamento, textos ou estoque.
- Enum salvo como número: valores novos sempre no fim, nunca reordenar `Ferramenta`.
- Scripts e comentários em pt-BR, com o cabeçalho de comentário usado no projeto.
- Tela: 29 × 29 cm, moldura de 1,7 cm, 0,8 cm de espessura; na mão, escala 0,67; no bueiro, escala local 1 e centro a 2,9 cm.
- Peso da camada em 0,15 s; troca de estado em 0,12 s; `fatorDoOlhar` padrão 1.
- Python no Git Bash deste Windows: rodar com `python -X utf8`, e escrever scripts com a ferramenta Write, não por heredoc.
- `AGENTS.md` só muda com pedido explícito do usuário. Toda alteração entra no `contexto do jogo.md`.

## Review Focus

- **Última carga acaba no meio do gesto:** o item segue a tabela no mesmo quadro, e as partículas param (Task 2).
- **Reabastecer com o balde vazio na mão:** a areia volta na hora, sem trocar de ferramenta (Task 2).
- **Animator ainda não pronto ao carregar a cena:** nenhum erro; a pose entra quando ele iniciar (Task 2).
- **Pausa no meio do gesto:** com `dt = 0`, o peso não anda, e ao voltar sem gesto a pose de segurar retorna (Task 2).
- **Menus rodados duas vezes:** nada duplica no Player nem no prefab do bueiro (Tasks 5 e 6).

---

### Task 1: Regra pura do item e do gesto

**Files:**
- Create: `Assets/Scripts/RegraDoItemNaMao.cs`
- Test: `Assets/Editor/Testes/RegraDoItemNaMaoTestes.cs`

**Interfaces:**
- Produces:
  - `enum ItemNaMao { Nenhum, BaldeCheio, BaldeVazio, Tela, Borrifador }`
  - `static ItemNaMao RegraDoItemNaMao.Qual(Ferramenta selecionada, int carga, bool comPneu)`
  - `static bool RegraDoItemNaMao.Gesticula(ItemNaMao item, Ferramenta selecionada, bool skillCheckAtivo, bool avancando, Ferramenta? necessariaDoAlvo)`
  - `static string RegraDoItemNaMao.EstadoDoAnimator(ItemNaMao item, bool gesticulando)`
  - `static float RegraDoItemNaMao.GiroDoOmbro(float pitch, float fator, float peso)`

- [ ] **Step 1: Criar a branch e guardar a spec, o plano e o registro**

```bash
git switch -c itens-na-mao
git add docs/superpowers "contexto do jogo.md"
git commit -m "Spec e plano: itens do kit na mão"
```

- [ ] **Step 2: Escrever os testes**

```csharp
[Category("ItensNaMao")]
public class RegraDoItemNaMaoTestes
{
    [TestCase(Ferramenta.Areia, 1, false, ItemNaMao.BaldeCheio)]
    [TestCase(Ferramenta.Areia, 0, false, ItemNaMao.BaldeVazio)]
    [TestCase(Ferramenta.Tela, 1, false, ItemNaMao.Tela)]
    [TestCase(Ferramenta.Tela, 0, false, ItemNaMao.Nenhum)]
    [TestCase(Ferramenta.AguaSanitaria, 2, false, ItemNaMao.Borrifador)]
    [TestCase(Ferramenta.AguaSanitaria, 0, false, ItemNaMao.Borrifador)]
    [TestCase(Ferramenta.Maos, 0, false, ItemNaMao.Nenhum)]
    [TestCase(Ferramenta.SacoDeLixo, 3, false, ItemNaMao.Nenhum)]
    [TestCase(Ferramenta.Areia, 1, true, ItemNaMao.Nenhum)]
    [TestCase(Ferramenta.AguaSanitaria, 2, true, ItemNaMao.Nenhum)]
    public void Qual_SegueATabela(Ferramenta f, int carga, bool pneu, ItemNaMao esperado) =>
        Assert.AreEqual(esperado, RegraDoItemNaMao.Qual(f, carga, pneu));

    [TestCase(ItemNaMao.BaldeCheio, Ferramenta.Areia, false, true, Ferramenta.Areia, true)]    // foco certo
    [TestCase(ItemNaMao.Borrifador, Ferramenta.AguaSanitaria, false, true, null, false)]      // ponto de uso
    [TestCase(ItemNaMao.BaldeCheio, Ferramenta.Areia, false, true, Ferramenta.Tela, false)]   // outro foco
    [TestCase(ItemNaMao.BaldeCheio, Ferramenta.Areia, false, false, Ferramenta.Areia, false)] // sem segurar
    [TestCase(ItemNaMao.BaldeVazio, Ferramenta.Areia, false, true, Ferramenta.Areia, false)]
    [TestCase(ItemNaMao.Tela, Ferramenta.Tela, true, false, Ferramenta.Tela, true)]           // skill check
    [TestCase(ItemNaMao.Nenhum, Ferramenta.Tela, true, false, Ferramenta.Tela, false)]        // errou e zerou
    public void Gesticula_SegueATabela(ItemNaMao item, Ferramenta f, bool check, bool avancando,
                                       Ferramenta? necessaria, bool esperado) =>
        Assert.AreEqual(esperado, RegraDoItemNaMao.Gesticula(item, f, check, avancando, necessaria));

    [TestCase(ItemNaMao.BaldeCheio, false, "CarregandoBalde")]
    [TestCase(ItemNaMao.BaldeVazio, false, "CarregandoBalde")]
    [TestCase(ItemNaMao.BaldeCheio, true, "UsandoBalde")]
    [TestCase(ItemNaMao.Tela, false, "CarregandoTela")]
    [TestCase(ItemNaMao.Tela, true, "UsandoTela")]
    [TestCase(ItemNaMao.Borrifador, false, "CarregandoBorrifador")]
    [TestCase(ItemNaMao.Borrifador, true, "UsandoBorrifador")]
    [TestCase(ItemNaMao.Nenhum, false, null)]
    public void EstadoDoAnimator_DoItem(ItemNaMao item, bool gesto, string esperado) =>
        Assert.AreEqual(esperado, RegraDoItemNaMao.EstadoDoAnimator(item, gesto));

    [Test]
    public void GiroDoOmbro_MultiplicaPitchFatorEPeso() =>
        Assert.AreEqual(15f, RegraDoItemNaMao.GiroDoOmbro(40f, 0.75f, 0.5f), 0.001f);
}
```

- [ ] **Step 3: Rodar e ver falhar**

Run: `unity command run_tests --mode EditMode --filter_type category --filter ItensNaMao`
Expected: erro de compilação: `RegraDoItemNaMao` não existe.

- [ ] **Step 4: Implementar `Assets/Scripts/RegraDoItemNaMao.cs`**

Classe estática pura, sem `UnityEngine`, no padrão do `SkillCheck.cs`, com as assinaturas de Interfaces.

- [ ] **Step 5: Rodar e ver passar**

Run: `unity command run_tests --mode EditMode --filter_type category --filter ItensNaMao`
Expected: todos passam.

- [ ] **Step 6: Commit**

```bash
git add Assets/Scripts/RegraDoItemNaMao.cs* Assets/Editor/Testes/RegraDoItemNaMaoTestes.cs*
git commit -m "Itens na mão: regra do item e do gesto"
```

### Task 2: Componente `ItensNaMao`

**Files:**
- Create: `Assets/Scripts/ItensNaMao.cs`
- Test: `Assets/Editor/Testes/ItensNaMaoTestes.cs`

**Interfaces:**
- Consumes: Task 1. `kit.OnMudou`, `kit.Selecionada`, `kit.PneuCarregado`, `kit.Estoque.Quantidade`, `interactor.Avancando`, `interactor.SkillCheckAtivo`, `interactor.CurrentTarget.FerramentaNecessaria`, `jogador.Pitch`.
- Produces:
  - `public const string ItensNaMao.NomeCamada = "ItemNaMao"`
  - Campos serializados da spec, com estes nomes: `kit`, `interactor`, `jogador`, `animator`, `baldeNaMao`, `areiaDoBalde`, `telaNaMao`, `borrifadorNaMao`, `areiaCaindo`, `nevoa`, `ombroDireito`, `duracaoBraco`, `fatorDoOlhar`
  - `public void Registrar()`: assina `OnMudou` uma vez e aplica o estado; chamado pelo `OnEnable`
  - `internal void Aplicar()`, `internal void AtualizarGesto(float dt, bool gesticula)`, `internal void GirarOmbro()`
  - `internal ItemNaMao ItemAtual`, `internal float Peso`, `internal bool Gesticulando`

- [ ] **Step 1: Escrever os testes**

Montagem com `TesteUtil`, como em `FeedbackDeCargaTestes`: kit e `ItensNaMao` no mesmo objeto, três `Transform` de item, `areiaDoBalde` filha do balde, dois `ParticleSystem`, referências via `TesteUtil.Setar`, `Registrar()` no `[SetUp]` e `TesteUtil.LimparTudo()` no `[TearDown]`. A carga vem de `kit.Consumir` e `kit.Reabastecer`.

| Teste | Assertiva |
|---|---|
| `Areia_MostraBaldeComAreia` | `baldeNaMao` ativo, `areiaDoBalde` ativa, tela e borrifador inativos |
| `AreiaZerada_MostraBaldeVazio` | `baldeNaMao` ativo, `areiaDoBalde` inativa |
| `TelaZerada_NaoMostraNada` | os três inativos, `ItemAtual == Nenhum` |
| `AguaZerada_MantemBorrifador` | `borrifadorNaMao` ativo |
| `TrocarDeFerramenta_TrocaOItem` | após `Selecionar`, só o novo item ativo (via `OnMudou`) |
| `Reabastecer_DevolveAreiaAoBalde` | zerada e `Reabastecer` → `areiaDoBalde` ativa, sem `Selecionar` |
| `UltimaCargaNoGesto_EscondeEParaParticulas` | com gesto ligado, `Consumir` a última tela; `telaNaMao` inativa; depois de `AtualizarGesto(dt, false)`, `Gesticulando == false` |
| `Gesto_LigaSoAParticulaDoItem` | `AtualizarGesto(0.1f, true)` no balde → emissão de `areiaCaindo` ligada e de `nevoa` desligada; com `false`, as duas desligadas |
| `Peso_SobeEmDuracaoBraco` | com item, de 0, `AtualizarGesto(0.075f, false)` → `Peso == 0.5f`; mais `0.075f` → `1f` |
| `Peso_DesceSemItem` | sem item, de 1, `AtualizarGesto(0.15f, false)` → `Peso == 0f` |
| `Pausa_NaoAndaOPeso` | `AtualizarGesto(0f, true)` não muda `Peso`; a seguir `AtualizarGesto(0f, false)` → `Gesticulando == false` |
| `Ombro_GiraPeloOlhar` | Player sem giro, `jogador.DefinirOlhar(0, 40)`, `Peso` 0,5, `fatorDoOlhar` 1: `GirarOmbro()` gira `ombroDireito` 20° em volta de `transform.right` |
| `Ombro_ParadoSemPeso` | com `Peso == 0`, a rotação não muda |
| `AnimatorNaoPronto_NaoErraNemAvisa` | `Animator` em objeto inativo, `AtualizarGesto` não gera aviso nem exceção |
| `CamadaAusente_AvisaUmaVez` | controller em memória sem a camada: `LogAssert.Expect` de um único aviso em duas chamadas |

- [ ] **Step 2: Rodar e ver falhar**

Run: `unity command run_tests --mode EditMode --filter_type category --filter ItensNaMao`
Expected: erro de compilação: `ItensNaMao` não existe.

- [ ] **Step 3: Implementar `Assets/Scripts/ItensNaMao.cs`**

- `Aplicar()`: calcula `Qual(kit.Selecionada, kit.Estoque.Quantidade(kit.Selecionada), kit.PneuCarregado != null)` e ativa os objetos.
- `Update`: chama `AtualizarGesto(Time.deltaTime, RegraDoItemNaMao.Gesticula(...))`, com `necessariaDoAlvo` vindo de `interactor.CurrentTarget` (null sem foco).
- `AtualizarGesto`: `Peso` por `MoveTowards(Peso, alvo, dt / duracaoBraco)`; liga e desliga `emission.enabled` das partículas, sem `Stop`, para os grãos no ar terminarem de cair; com a camada presente, `SetLayerWeight` e `CrossFadeInFixedTime(estado, 0.12f, camada)` só quando o estado muda.
- Animator: como o `ConferirAnimator` do `FeedbackDeCarga`: espera `isInitialized`, procura a camada uma vez e avisa uma vez, pedindo o menu `Dengue > Configurar Itens na Mão`.
- `LateUpdate`: chama `GirarOmbro()`, que usa `ombroDireito.RotateAround(ombroDireito.position, transform.right, GiroDoOmbro(jogador.Pitch, fatorDoOlhar, Peso))`.
- `OnDisable`: desassina `OnMudou`.

- [ ] **Step 4: Rodar e ver passar**

Run: `unity command run_tests --mode EditMode --filter_type category --filter ItensNaMao`
Expected: todos passam.

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/ItensNaMao.cs* Assets/Editor/Testes/ItensNaMaoTestes.cs*
git commit -m "Itens na mão: componente do Player"
```

### Task 3: Modelos e importação

**Files:**
- Create: `Ferramentas/Blender/gerar_itens.py`, `Assets/Editor/ItensNaMaoBuilder.cs`, `Assets/Meus/Assets Gerados/Itens/` (FBX, PNG e materiais)
- Test: `Assets/Editor/Testes/ItensNaMaoAssetsTestes.cs`

**Interfaces:**
- Produces:
  - `Balde.fbx` (objetos `Balde`, `Areia`), `Tela.fbx` (`Moldura`, `Malha`), `Borrifador.fbx` (`Borrifador`)
  - `Tela_Malha.png`, `Particula.png`, `Itens_Particula.mat`
  - `public static class ItensNaMaoBuilder`, com `Pasta = "Assets/Meus/Assets Gerados/Itens"` e `public static string GarantirImportacaoDosItens()`

Materiais no FBX e no Unity (sRGB):

| FBX | Materiais |
|---|---|
| `Balde` | `Zinco #9EA8B3`, `Alca #4D4F54`, `Areia #D6B87A` |
| `Tela` | `TelaMoldura #17805F`, `TelaMalha` (textura) |
| `Borrifador` | `Frasco #EDF0EB`, `Rotulo #1C54AD`, `Faixa #F5CC33`, `Cabeca #17805F`, `Gatilho #0D4D3D` |

- [ ] **Step 1: Escrever `Ferramentas/Blender/gerar_itens.py`**

Mesmo esqueleto do `gerar_saco.py` (`hexcol`, bmesh, exportação FBX e prévia), com pivôs e medidas da spec.
- Balde: Ø 22 cm, 17 cm, pivô no meio da alça, topo do balde 15 cm abaixo do pivô.
- Tela: quadro no plano XY, centro na origem. `Malha` com UV de 1 repetição a cada 5 cm.
- Borrifador: ~20 cm, pivô no gargalo, bico em -Y (fica +Z no Unity), silhueta da prévia.
- `Tela_Malha.png`: 64 × 64, fio de 2 px a cada 8 px, sem fio transparente, cor `#293635`.
- `Particula.png`: 32 × 32, bolinha branca de borda suave.

- [ ] **Step 2: Gerar e copiar**

```bash
"/c/Program Files/Blender Foundation/Blender 5.2/blender.exe" -b --factory-startup --python Ferramentas/Blender/gerar_itens.py -- Temp/itens
```

Expected: `FBX OK` para os três. Conferir `Temp/itens/previa_*.png` com Read. Copiar FBX e PNG, sem as prévias, para `Assets/Meus/Assets Gerados/Itens/`.

- [ ] **Step 3: Escrever os testes de assets**

```csharp
[TestCase("Balde", 0.21f, 0.24f)]      // Ø 22 cm, com a borda
[TestCase("Tela", 0.285f, 0.295f)]
public void Fbx_TemALarguraDaSpec(string nome, float min, float max);
// carrega o FBX, soma Renderer.bounds e confere Mathf.Max(size.x, size.z)

[Test] public void Borrifador_TemAAlturaDaSpec();   // size.y entre 0.19 e 0.21
[Test] public void Balde_TemAreiaSeparada();        // filho "Areia"
[Test] public void Tela_MalhaEhAlphaClip();         // _AlphaClip 1, _ALPHATEST_ON, _Cull 0, _BaseMap com Tela_Malha.png
[Test] public void Tela_TexturaPreservaCobertura(); // TextureImporter: Repeat, mipMapsPreserveCoverage
[Test] public void Malha_EhPlanaEDeitada();         // size.y < 0.01 na Tela.fbx, com a bakeAxisConversion ligada
```

- [ ] **Step 4: Rodar e ver falhar**

Run: `unity command run_tests --mode EditMode --filter_type category --filter ItensNaMao`
Expected: `ItensNaMaoAssetsTestes` falha por faltar o builder ou a importação.

- [ ] **Step 5: Implementar `GarantirImportacaoDosItens()` em `Assets/Editor/ItensNaMaoBuilder.cs`**

Mesma importação de `PoseDeCarga.GarantirImportacaoDoSaco` para os 3 FBX, com remap da tabela. `Tela_Malha.mat` em URP Lit, com `_AlphaClip` 1, `_Cutoff` 0,5, keyword `_ALPHATEST_ON`, `_Cull` 0 e fila 2450. Textura com `wrapMode = Repeat`, `mipMapsPreserveCoverage = true` e `alphaTestReferenceValue = 0.5f`. `Itens_Particula.mat` com `Sprites/Default` e `Particula.png`, porque a textura embutida de partícula não existe nesta versão do Unity. Rodar a função por `eval_file`.

- [ ] **Step 6: Rodar e ver passar**

Run: `unity command run_tests --mode EditMode --filter_type category --filter ItensNaMao`
Expected: todos passam.

- [ ] **Step 7: Commit**

```bash
git add Ferramentas/Blender/gerar_itens.py "Assets/Meus/Assets Gerados/Itens"* Assets/Editor/ItensNaMaoBuilder.cs* Assets/Editor/Testes/ItensNaMaoAssetsTestes.cs*
git commit -m "Itens na mão: modelos e importação"
```

### Task 4: Poses, gestos e camada do Animator

**Files:**
- Modify: `Ferramentas/Blender/gerar_agente.py`, `Assets/Meus/Assets Gerados/Agente de Saude/AgenteSaude.fbx`, `Assets/Editor/PoseDeCarga.cs`, `Assets/Editor/ItensNaMaoBuilder.cs`, `Assets/Meus/Assets Gerados/Agente de Saude/Agente_AnimController.controller`
- Test: `Assets/Editor/Testes/ItensNaMaoAssetsTestes.cs`

**Interfaces:**
- Consumes: `ItensNaMao.NomeCamada`, Task 2.
- Produces:
  - Clipes `CarregandoBalde`, `CarregandoTela`, `CarregandoBorrifador`, `UsandoBalde`, `UsandoTela`, `UsandoBorrifador`
  - `public static string ItensNaMaoBuilder.GarantirCamadaDosItens()`
  - `internal static string PoseDeCarga.GarantirClipes(IEnumerable<string> nomes)`, extraído do `GarantirClipesDeCarga`
  - `PoseDeCarga.RecriarCamada` e `PoseDeCarga.GarantirMascara` passam de `private` a `internal`

- [ ] **Step 1: Escrever os testes**

```csharp
[Test] public void Controller_TemACamadaComOs6Estados();
// camada "ItemNaMao": defaultWeight 0, máscara Agente_CarregandoSaco, os 6 estados com motion e writeDefaultValues false

[TestCase("UsandoBalde", 16, 24)] [TestCase("UsandoTela", 16, 24)] [TestCase("UsandoBorrifador", 16, 24)]
public void Gesto_DuraDe16a24Quadros(string clipe, int min, int max);

[Test] public void Clipes_MantemOsAntigos();
// Idle, Walk, Run, Air, Carregando e CarregandoSaco continuam no FBX; as camadas antigas seguem com peso 0
```

- [ ] **Step 2: Rodar e ver falhar**

Run: `unity command run_tests --mode EditMode --filter_type category --filter ItensNaMao`
Expected: `Controller_TemACamadaComOs6Estados` falha.

- [ ] **Step 3: Adicionar as ações em `gerar_agente.py`**

Depois de `CarregandoSaco`, usando `braco_ik` e o mesmo par de `pose_perna`. Valores iniciais, em metros (mão direita em x negativo, frente em -y):

| Item | Palma | Dedos | Normal da palma |
|---|---|---|---|
| Balde | `(-0.30, -0.50, 1.62)` | `(0.10, -0.85, -0.50)` | `(1, 0, 0)` |
| Tela | `(-0.30, -0.60, 1.42)` | `(0.35, -0.90, 0)` | `(0.9, -0.3, 0)` |
| Borrifador | `(-0.30, -0.60, 1.545)` | `(0.35, -0.90, 0)` | `(0.9, -0.3, 0)` |

Gestos em quadros, com `k = sin(π·f/n)`:
- `UsandoBalde` (24): dedos giram até 45° para a frente e para baixo.
- `UsandoTela` (20): palma avança 10 cm e desce 6 cm.
- `UsandoBorrifador` (16): palma recua 1,2 cm e os dedos sobem 6°.

- [ ] **Step 4: Exportar e copiar o agente**

```bash
"/c/Program Files/Blender Foundation/Blender 5.2/blender.exe" -b --factory-startup --python Ferramentas/Blender/gerar_agente.py -- Temp/agente --sem-render
```

Expected: `FBX OK`. Copiar só `AgenteSaude.fbx` para a pasta do agente.

- [ ] **Step 5: Implementar a camada**

Extrair `PoseDeCarga.GarantirClipes`, mantendo o `GarantirClipesDeCarga` com o mesmo resultado. `GarantirCamadaDosItens()` chama `GarantirClipes` com os 6 nomes, recria a camada com `RecriarCamada(controller, ItensNaMao.NomeCamada, mascaraDoSaco)` e cria os 6 estados, com `CarregandoBalde` como padrão. Rodar por `eval_file`.

- [ ] **Step 6: Rodar a suíte inteira**

Run: `unity command run_tests --mode EditMode`
Expected: nenhuma falha, inclusive `FeedbackDeCargaTestes`.

- [ ] **Step 7: Commit**

```bash
git add Ferramentas/Blender/gerar_agente.py "Assets/Meus/Assets Gerados/Agente de Saude" Assets/Editor/PoseDeCarga.cs Assets/Editor/ItensNaMaoBuilder.cs Assets/Editor/Testes/ItensNaMaoAssetsTestes.cs
git commit -m "Itens na mão: poses, gestos e camada do Animator"
```

### Task 5: Montagem no Player e menu

**Files:**
- Modify: `Assets/Editor/ItensNaMaoBuilder.cs`, `Assets/Scenes/SampleScene.unity`
- Test: `Assets/Editor/Testes/ItensNaMaoAssetsTestes.cs`

**Interfaces:**
- Consumes: Tasks 2 a 4. `PoseDeCarga.NomeOssoMao`.
- Produces:
  - `public static void ItensNaMaoBuilder.AplicarNoPlayer(GameObject player)`
  - Filhos de `RightHand`, inativos: `BaldeNaMao`, `TelaNaMao`, `BorrifadorNaMao`; `AreiaCaindo` no balde e `Nevoa` no borrifador
  - Menu `Dengue/Configurar Itens na Mão`, que nesta task chama `GarantirImportacaoDosItens`, `GarantirCamadaDosItens`, `AplicarNoPlayer` e salva a cena

- [ ] **Step 1: Escrever os testes**

Player falso com `TesteUtil`: `Modelo/RightUpperArm/RightHand`, com `KitDoAgente`.

```csharp
[Test] public void Aplicar_CriaOsTresItensInativos();   // na RightHand, com Tela em escala 0.67
[Test] public void Aplicar_LigaOComponente();           // todos os campos preenchidos, ombroDireito = RightUpperArm
[Test] public void Aplicar_CriaParticulasDesligadas();  // emissão desligada, espaço World, sem sombra
[Test] public void Aplicar_DuasVezesNaoDuplica();       // um filho de cada nome, um ItensNaMao
[Test] public void Itens_FicamNaCamadaDoCorpo();        // layer igual ao da RightHand, para a mira não pegar
```

- [ ] **Step 2: Rodar e ver falhar**

Run: `unity command run_tests --mode EditMode --filter_type category --filter ItensNaMao`
Expected: `AplicarNoPlayer` não existe.

- [ ] **Step 3: Implementar `AplicarNoPlayer` e o menu**

Seguir `PoseDeCarga.Aplicar`: acha os ossos, instancia os FBX com `PrefabUtility.InstantiatePrefab`, reaproveita filhos existentes e define pose por constantes (`PosicaoDoBalde`, `GiroDoBalde` e as equivalentes). Início: balde na posição e no giro do saco.

Partículas:
- `AreiaCaindo`: na boca do balde; 40 por segundo, máximo 60, vida 0,5 s, gravidade 1, tamanho 0,012 a 0,02, cor `#D6B87A`.
- `Nevoa`: no bico; 50 por segundo, máximo 40, vida 0,35 s, velocidade 1,2, cone de 14°, tamanho crescendo de 0,03 a 0,08, branco com alfa 0,35, gravidade 0.

As duas com `Itens_Particula.mat`, `playOnAwake`, `loop`, espaço World e sem sombra.

- [ ] **Step 4: Rodar e ver passar**

Run: `unity command run_tests --mode EditMode --filter_type category --filter ItensNaMao`
Expected: todos passam.

- [ ] **Step 5: Rodar o menu na cena**

```bash
unity command open_scene --path Assets/Scenes/SampleScene.unity
unity command menu --path "Dengue/Configurar Itens na Mão"
```

Expected: sem erro no Console; o Player tem o `ItensNaMao` com todos os campos ligados (conferir por `eval_file`).

- [ ] **Step 6: Ajustar enquadramento com capturas**

Em Play, com a abertura pulada como no `contexto do jogo.md` (`FimAbertura`), `Application.runInBackground = true` e câmera olhando reto, capturar cada item segurando e usando. Ajustar as constantes e, se preciso, as poses da Task 4 até: pega dentro da mão; item inteiro à vista no canto inferior direito (no balde, a boca com areia e a alça); mira livre. Ao fim, `PlayerSettings.runInBackground = false`.

- [ ] **Step 7: Commit**

```bash
git add Assets/Editor/ItensNaMaoBuilder.cs Assets/Editor/Testes/ItensNaMaoAssetsTestes.cs Assets/Scenes/SampleScene.unity Ferramentas/Blender/gerar_agente.py "Assets/Meus/Assets Gerados/Agente de Saude"
git commit -m "Itens na mão: montagem no Player"
```

### Task 6: Tela instalada no bueiro

**Files:**
- Modify: `Assets/Editor/ItensNaMaoBuilder.cs`, `Assets/Editor/FocosBlenderBuilder.cs`, `Assets/Meus/Assets Gerados/Focos/Bueiro.prefab`
- Test: `Assets/Editor/Testes/ItensNaMaoAssetsTestes.cs`

**Interfaces:**
- Consumes: `Tela.fbx`, Task 3.
- Produces: `public static void ItensNaMaoBuilder.AdicionarTelaAoBueiro()`. Objeto `TelaInstalada` no `Bueiro.prefab`. O menu passa a chamá-la.

- [ ] **Step 1: Escrever os testes**

```csharp
[Test] public void Bueiro_TelaApontaParaATelaInstalada();  // campo "tela" = TelaInstalada, inativa
[Test] public void Bueiro_TelaFicaAjustadaSobAGrelha();
// escala local 1, irmã da Tampa, y local 0.029; meia-largura 0.145 <= 0.15 da grelha
// e topo 0.033 abaixo de posFechada.y 0.034
[Test] public void Bueiro_ObjetoAntigoSegueInativo();      // "Tela" do FBX inativo
[Test] public void Bueiro_RodarDuasVezesNaoDuplica();
```

- [ ] **Step 2: Rodar e ver falhar**

Run: `unity command run_tests --mode EditMode --filter_type category --filter ItensNaMao`
Expected: os testes do bueiro falham.

- [ ] **Step 3: Implementar `AdicionarTelaAoBueiro()`**

Usar `PrefabUtility.LoadPrefabContents`, reaproveitar `TelaInstalada` se existir, instanciar o `Tela.fbx` ao lado da `Tampa`, ajustar o campo por `SerializedObject`, salvar com `SaveAsPrefabAsset` e descarregar em `finally`. Não recriar o prefab pelo `CriarBueiro`: isso perderia os valores gravados pelo kit. Chamar a função no menu, e no fim do `CriarBueiro`, depois de `SalvarPrefab`.

- [ ] **Step 4: Rodar o menu e os testes**

Run: `unity command menu --path "Dengue/Configurar Itens na Mão"`
Run: `unity command run_tests --mode EditMode --filter_type category --filter ItensNaMao`
Expected: todos passam.

- [ ] **Step 5: Commit**

```bash
git add Assets/Editor/ItensNaMaoBuilder.cs Assets/Editor/FocosBlenderBuilder.cs "Assets/Meus/Assets Gerados/Focos/Bueiro.prefab" Assets/Editor/Testes/ItensNaMaoAssetsTestes.cs
git commit -m "Itens na mão: tela instalada no bueiro"
```

### Task 7: Verificação final e registro

**Files:**
- Modify: `Assets/Editor/PlayerBuilder.cs`, `Assets/Editor/PoseDeCarga.cs` (só comentários), `contexto do jogo.md`

**Interfaces:**
- Consumes: tudo das tasks anteriores.

- [ ] **Step 1: Atualizar os comentários**

No cabeçalho do `PlayerBuilder.cs` e no aviso do `PoseDeCarga.cs`: depois do Configurar Player, rodar também `Dengue > Configurar Itens na Mão`.

- [ ] **Step 2: Rodar a suíte inteira**

Run: `unity command run_tests --mode EditMode`
Expected: nenhuma falha; total acima dos 336 da Parte C.

- [ ] **Step 3: Verificar em Play, com capturas**

| Situação | Esperado |
|---|---|
| Andar, correr, pegar pneu, saco com garrafas | Iguais a antes da reexportação |
| Olhar para um foco no chão com cada item | Item segue no canto; foco e mira visíveis |
| Olhar a ±80° | Braço não entra na câmera; se entrar, reduzir `fatorDoOlhar` |
| Segurar no vaso e na bromélia | Gesto e partículas; param ao soltar |
| Skill check no bueiro | Gesto da tela; no erro com a última tela, ela some |
| Balde vazio e borrifador sem carga, no foco certo | Sem gesto; aviso "Acabou: ..." |
| Reabastecer com o balde vazio | Areia volta; sem gesto |
| Os quatro bueiros concluídos | `tela` ativa, sob a grelha, ajustada à escala; malha legível entre as barras |
| Pausar no meio do gesto e voltar | Sem pose presa |

- [ ] **Step 4: Restaurar `PlayerSettings.runInBackground` e sair do Play**

Conferir `runInBackground: 0` em `ProjectSettings/ProjectSettings.asset` e `git status` sem arquivos inesperados.

- [ ] **Step 5: Registrar no `contexto do jogo.md`**

Entrada no formato do Histórico: data, objetivo, arquivos, configurações, testes, o que foi e o que não foi verificado (build e som de ouvido seguem pendentes, se não forem testados).

- [ ] **Step 6: Commit**

```bash
git add Assets/Editor/PlayerBuilder.cs Assets/Editor/PoseDeCarga.cs "contexto do jogo.md"
git commit -m "Itens na mão: verificação e registro"
```
