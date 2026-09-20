# Regras do Projeto para Assistentes de IA

## Regras de Edição

1. **Sempre** que alterar qualquer arquivo do projeto (scripts, cena, configs, assets), registrar a mudança no final do arquivo `contexto do jogo.md` na seção **Histórico de Alterações**, seguindo o formato já estabelecido (data, objetivo, arquivos alterados, configurações aplicadas, observações técnicas).
2. `contexto do jogo.md` é o **único** arquivo que pode ser editado de forma autônoma por assistentes de IA.
3. Alterações em **outros arquivos** do projeto somente a pedido explícito do usuário.
4. O contexto completo do jogo (história, mecânicas, fases, sons, etc.) está em `contexto do jogo.md`. Leia-o ao iniciar qualquer sessão para entender o projeto.

---

## Contexto do Jogo (Resumo)

- **Nome:** Jogo educativo sobre dengue / Aedes aegypti
- **Gênero:** FPS educativo — agente comunitário de saúde eliminando criadouros
- **Engine:** Unity 3D (Input System novo)
- **Autor:** Eduardo Augusto Richter

### Premissa
O jogador assume um agente comunitário de saúde que sai de um hospital superlotado para erradicar focos do mosquito Aedes aegypti em duas quadras de um bairro urbano, antes que a ocupação hospitalar atinja 100%.

### Mecânicas Principais
- FPS com movimentação relativa à câmera (WASD + mouse)
- Barra de ocupação hospitalar sobe com o tempo; cada foco eliminado desacelera
- Rede social com comentários da comunidade como feedback (ícone de celular)
- 15-20 criadouros espalhados no mapa (pneus, caixas d'água, vasos, etc.)
- Áudio 3D de zumbido de mosquito intensifica ao se aproximar de foco
- Tempo limitado determinado pela barra de ocupação

### Estrutura Técnica Atual
- **Player:** CharacterController (não Rigidbody), movimentação horizontal + gravidade manual + pulo via `TriggerJump()`, com `Interactor` (clique esquerdo para interagir)
- **Scripts:**
  - `mover.cs` — Movimentação (CharacterController.Move), gravidade (-20), pulo (3f)
  - `pular.cs` — Detecta tecla Espaço, verifica isGrounded, chama TriggerJump()
  - `MouseLook.cs` — Rotação horizontal no cubo, vertical na câmera
  - `Interactable.cs` — Classe base genérica de focos de criadouro (requiredClicks=5, encolhe até sumir ao concluir, hooks virtuais)
  - `Interactor.cs` — Raycast do centro da tela + clique esquerdo → Interactable (com HUD simples OnGUI)
  - `PneuInteractable.cs` — Herda Interactable; remove a água do pneu após 3 cliques e então encolhe até sumir
- **Cena:** `Assets/Scenes/SampleScene.unity`
- **Câmera:** Main Camera posicionada como "olhos" do player (eyeHeight = 1)
- **Foco implementado:** "pneu" (700591058) com PneuInteractable; reutilizar criando derivados de `Interactable` ou usá-lo direto
