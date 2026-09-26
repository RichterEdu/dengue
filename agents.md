# Regras do Projeto para Assistentes de IA

## Regras de Edição

1. **Sempre** que alterar qualquer arquivo do projeto (scripts, cena, configs, assets), registrar a mudança no final do arquivo `contexto do jogo.md` na seção **Histórico de Alterações**, seguindo o formato já estabelecido (data, objetivo, arquivos alterados, configurações aplicadas, observações técnicas).
2. `contexto do jogo.md` é o **único** arquivo que pode ser editado de forma autônoma por assistentes de IA.
3. Alterações em **outros arquivos** do projeto somente a pedido explícito do usuário.
4. O contexto completo do jogo (história, mecânicas, fases, sons, etc.) está em `contexto do jogo.md`. Leia-o ao iniciar qualquer sessão para entender o projeto.

---

## Contexto do Jogo (Resumo)

- **Nome:** Foco Zero — jogo educativo sobre dengue / Aedes aegypti
- **Gênero:** FPS educativo — agente comunitário de saúde eliminando criadouros
- **Engine:** Unity 3D (Input System novo)
- **Autor:** Eduardo Augusto Richter

### Premissa
O jogador assume um agente comunitário de saúde que sai de um hospital superlotado para erradicar focos do mosquito Aedes aegypti em duas quadras de um bairro urbano, antes que a ocupação hospitalar atinja 100%.

### Mecânicas Principais
- FPS com movimentação relativa à câmera (WASD + mouse, Shift corre, Espaço pula; controle também funciona)
- Barra de ocupação hospitalar sobe com o tempo; cada foco eliminado desacelera
- Rede social com comentários da comunidade como feedback (ícone de celular)
- 15-20 criadouros espalhados no mapa (pneus, caixas d'água, vasos, etc.)
- Áudio 3D de zumbido de mosquito intensifica ao se aproximar de foco
- Tempo limitado determinado pela barra de ocupação

### Estrutura Técnica Atual
- **Player (`/Player`):** CharacterController (não Rigidbody) com corpo visível de **agente de saúde** (filho `Modelo`, prefab `Assets/Meus/Assets Gerados/Agente de Saude/Agente de Saude.prefab`, modelado no Blender por `Ferramentas/Blender/gerar_agente.py`). Montado pelo menu **Dengue > Configurar Player (Agente de Saúde)** (`Assets/Editor/PlayerBuilder.cs`).
- **Scripts do player:**
  - `EntradaJogador.cs` — Única leitura de entrada, via `InputSystem_Actions.inputactions` (teclado, mouse e controle): Move, Look, Jump, Sprint, Attack
  - `ControladorJogador.cs` — Olhar + andar (2,8 m/s) + correr com Shift (5,8 m/s) + pulo por altura (1,1 m, coyote time) + gravidade (-20), num único `CharacterController.Move`. API: `ControleAtivo`, `Teleportar`, `DefinirOlhar`, `OnPulou`, `OnAterrissou`
  - `CameraJogador.cs` (na Main Camera) — Câmera em primeira pessoa com pivô no pescoço, head bob, afundada ao aterrissar e FOV na corrida
  - `AnimadorJogador.cs` — Velocidade real → Blend Tree Idle/Walk/Run + estado NoAr (`Agente_AnimController.controller`)
  - `Interactable.cs` — Classe base genérica de focos de criadouro (requiredClicks=5, encolhe até sumir ao concluir, hooks virtuais)
  - `Interactor.cs` — Raycast do centro da tela + clique esquerdo (ou Attack no controle) → Interactable
  - `PneuInteractable.cs` — Herda Interactable; remove a água do pneu após 3 cliques e então encolhe até sumir
- **Cenas:** `Assets/Scenes/MenuInicial.unity` (índice 0, tela de título gerada por **Dengue > Criar Menu Inicial**) → `Assets/Scenes/SampleScene.unity` (índice 1, o jogo)
- **Câmera:** Main Camera livre (não é filha do player), posicionada todo frame pelo `CameraJogador`; a cabeça do modelo fica em *Shadows Only*
- **Foco implementado:** "pneu" (700591058) com PneuInteractable; reutilizar criando derivados de `Interactable` ou usá-lo direto
