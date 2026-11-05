# Reposicionamento da Barra de Ocupação Hospitalar na HUD Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Mover a barra de ocupação hospitalar para o centro superior da HUD (`y = -32px`) e reposicionar o painel de avisos temporários `ToastPanel` (`y = -120px`), desobstruindo a hotbar e evitando sobreposição de elementos na tela.

**Architecture:** Atualização dos geradores e reparadores de UI no Editor Unity (`FluxoDeJogoHUDBuilder.cs`, `DengueHUDBuilder.cs`, `DengueHUDReparo.cs`) e inclusão de rotina para atualizar a cena ativa diretamente via Unity Editor / Undo sem recriar objetos ou perder referências serializadas. Registro formal da alteração em `contexto do jogo.md`.

**Tech Stack:** Unity 6000.5.6f1, uGUI, TextMeshPro, C# Editor scripting.

**Spec:** `docs/superpowers/specs/2026-10-25-hud-ocupacao-superior-design.md`

## Global Constraints
- Resolução de referência do Canvas: 1920×1080 (`matchWidthOrHeight = 0.5`).
- Cores e estilo sóbrio mantidos (fundo escuro `#12171CF5`, contorno branco sutil `#FFFFFF1A`, acento `#40FFCC`).
- Apenas `contexto do jogo.md` pode ser editado autonomamente; alterações nos scripts do projeto foram aprovadas pelo usuário.
- O histórico de alterações em `contexto do jogo.md` deve ser devidamente preenchido ao concluir as alterações.

## Review Focus
1. **Âncoras e pivôs:** Garantir que o `OcupacaoPanel` utilize `anchorMin = (0.5, 1.0)`, `anchorMax = (0.5, 1.0)`, `pivot = (0.5, 1.0)` e `anchoredPosition = (0, -32)` para não se deslocar com mudanças de aspect ratio.
2. **Posição do ToastPanel:** Garantir que avisos de eliminação de foco não colidam verticalmente com o painel de ocupação (`anchoredPosition = (0, -120)`).
3. **Persistência na Cena:** A cena aberta (`SampleScene.unity`) deve ser atualizada e marcada como dirty para salvar as posições dos GameObjects existentes.

---

### Task 1: Documentar a Spec formal
**Files:**
- Create: `docs/superpowers/specs/2026-10-25-hud-ocupacao-superior-design.md`

- [ ] **Step 1: Criar o arquivo de design formal na pasta de specs**

---

### Task 2: Atualizar os scripts de Builder e Reparo da HUD
**Files:**
- Modify: `Assets/Editor/FluxoDeJogoHUDBuilder.cs`
- Modify: `Assets/Editor/DengueHUDBuilder.cs`
- Modify: `Assets/Editor/DengueHUDReparo.cs`

- [ ] **Step 1: Atualizar `FluxoDeJogoHUDBuilder.cs`**
  - Mudar `PainelSobrio("OcupacaoPanel", ...)` para ancorar no centro superior (`anchorMin = (0.5, 1)`, `anchorMax = (0.5, 1)`, `anchoredPosition = (0, -32)`).
  - Adicionar/ajustar rotina para reposicionar o painel existente na cena sem precisar recriar tudo.
- [ ] **Step 2: Atualizar `DengueHUDBuilder.cs` e `DengueHUDReparo.cs`**
  - Mudar a posição padrão do `ToastPanel` de `y = -60` para `y = -120` para não sobrepor a barra de ocupação.
- [ ] **Step 3: Criar rotina utilitária de ajuste imediato na cena**
  - Implementar método acionável pelo menu `Dengue/Ajustar Layout da HUD (Ocupação no Topo)` que busca `OcupacaoPanel` e `ToastPanel` na cena aberta e aplica as novas coordenadas com `Undo.RecordObject` e marca a cena como dirty.

---

### Task 3: Aplicar as alterações na cena e registrar no histórico
**Files:**
- Modify: `Assets/Scenes/SampleScene.unity` (via menu/script do Unity)
- Modify: `contexto do jogo.md`

- [ ] **Step 1: Executar o ajuste e salvar a cena**
- [ ] **Step 2: Atualizar o arquivo `contexto do jogo.md` na seção Histórico de Alterações**
