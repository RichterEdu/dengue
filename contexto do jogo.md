**GameDoc \- Documento Macro do Projeto**

Autor: Eduardo Augusto Richter  
Data de criação do projeto: 

1. **Área temática:** Saúde Pública & Educação Comunitária com foco em conscientização sobre epidemias sazonais (especialmente dengue) e combate aos criadouros do Aedes aegypti. O jogo aborda a responsabilidade coletiva na prevenção, mostrando que infraestrutura hospitalar sozinha não é suficiente sem ação comunitária.   
     
2. **História:** O jogo inicia na recepção caótica de uma UBS/Hospital público superlotado com pacientes graves de dengue. A equipe médica alertou que sem eliminar os criadouros no bairro, o tratamento hospitalar é inútil — a epidemia continuará gerando novos casos.  
   O jogador assume o papel de um agente comunitário de saúde que sai a campo pelas ruas vizinhas com a missão urgente de inspecionar residências, terrenos baldios e calçadas para erradicar todos os focos do mosquito antes que o hospital entre em colapso total.  
   A rede social do bairro reflete essa urgência: comentários negativos inicialmente ("Que abandono\!", "Meu filho tem dengue 😞") que evoluem para positivos conforme o jogador completa sua missão ("Muito bom\! Graças a você\!"). 

   **A história é contada pelo celular (decisão de 2026-09-26):** tudo o que o jogador faz vira uma publicação de um personagem do bairro sobre *aquela* ação — chegar perto de um criadouro, eliminá-lo (o primeiro, os seguintes e o último de cada tipo), largar um pela metade, sair do hospital, ficar parado, demorar sem limpar nada, limpar dois seguidos, pular à toa, a ocupação do hospital subir ou cair. Com o celular fechado, cada post aparece como notificação na tela. O jogo abre com cartelas em tela preta ("Vila Nova, fevereiro. Três semanas de chuva...") e termina com um epílogo "Uma semana depois..." em que cada personagem conta como ficou — o texto muda conforme o que o jogador limpou ou deixou para trás.

   **Nome do jogo: Foco Zero (decisão de 2026-09-26).** O jogo abre numa tela de título (cena `MenuInicial`): "FOCO ZERO", com o "ZERO" em vermelho, sobre o bairro em fim de tarde, e a câmera alternando entre a rua do hospital, a praça e um sobrevoo. "Jogar" leva às cartelas da abertura.

3. **Dinâmica do jogo:** Perspectiva: Primeira Pessoa (FPS) para máxima imersão e simplicidade técnica.

Loop Principal:

* Explorar as quadras do bairro  
* Localizar criadouros pelo áudio 3D (zumbido de mosquito intensifica ao se aproximar)  
* Interagir com cada foco para eliminá-lo (mudança de estado visual simples)  
* Monitorar a barra de lotação do hospital no HUD (sobe continuamente, reduz a cada foco eliminado)  
* Acompanhar feedback da rede social (clica no ícone de celular, expande como um "tweeter" mostrando comentários da comunidade)

**Mecânica de Pressão:**

* Barra de ocupação hospitalar sobe continuamente com o tempo  
* Cada foco eliminado desacelera a taxa de ocupação  
* Se atingir 100% \= derrota (tela educativa sobre impacto da negligência)

**Interações Simples (sem animações complexas):**

* Pneu com água → interagir → pneu sem água (mudança de textura/estado)  
* Caixa d'água destampada → interagir → caixa tampada  
* Prato de vaso sem areia → interagir → prato com areia  
* Garrafa/recipiente com água → interagir → recipiente retirado/vazio  
* Vaso de planta com água parada → interagir → vaso seco  
* Tampa de lata com água → interagir → tampa removida  
* Poço/caixa aberta → interagir → caixa fechada/protegida





4. **Personagens:**  Jogador: Agente comunitário de saúde em primeira pessoa. Sem rosto visível, sem diálogos — apenas ações e presença.  
   Personagens da Rede Social (elenco fixo, cada um "dono" de um tipo de criadouro — não aparecem no mapa, só no celular):  
   * **Carlos Mendes** (@carlinhos.m) — pneus do terreno baldio; reclamava há meses. Epílogo: o terreno vira horta comunitária.  
   * **Dona Neusa** (@neusa.flores) — vasos/pratinhos; a vizinha das flores. Epílogo: vira "fiscal de vaso" da rua.  
   * **João Silva** (@joaosilva) — caixas d'água; o filho está internado com dengue. Epílogo: o filho tem alta (ou continua internado).  
   * **Ana Paula** (@anapaula.rs) — piscina de plástico do filho; também comenta quando o agente fica parado ou pula à toa. Epílogo: o filho monta a "patrulha do mosquito".  
   * **Bruna Lima** (@bru.lima) — garrafas/lixo e dicas; comenta sequências rápidas. Epílogo: a rua ganha ponto de coleta de recicláveis.  
   * **Seu Osvaldo** (@osvaldo.bar) — ralos da calçada; dono do bar da esquina, deixava a grelha de lado para lavar o chão. Epílogo: põe tela em todos os ralos e ensina os clientes.  
   * **Lúcia Andrade** (@lu.plantas) — bromélias da praça; tem medo de o agente arrancar as plantas. Epílogo: as bromélias ficam, e a criançada ajuda a esvaziar as folhas toda semana.  
   * **Enf. Rita** (@enf.rita.ubs) — a enfermeira da recepção (único NPC com corpo, na sala do hospital); posta a saída do agente e a lotação.  
   * **UBS Vila Nova** (@ubs.vilanova) — perfil oficial; marcos de ocupação e derrota.  
   Na sala do hospital: a enfermeira Rita (balão de fala com a missão) e dois pacientes que existem só como voz (balões, sem corpo).  
     
     
5. **Trilha sonora (som de fundo):**  Uma trilha urgente/tensa que reforça a pressão do tempo e a gravidade da situação hospitalar. Tom urbano, possivelmente com elementos de ambiente hospitalar (sinais de alerta leves, batidas de coração sutis) que diminuem conforme o jogador completa focos — criando feedback auditivo de progresso.  
   A música deve ser imersiva mas não excessivamente dramática, mantendo o tom educacional.  
     
     
6. **Sons da aplicação (sons dos objetos):** SONS DA APLICAÇÃO (Sons dos Objetos)  
* Zumbido de mosquito: Áudio 3D que intensifica conforme o jogador se aproxima de um criadouro ativo  
* Som de eliminação: Feedback sonoro quando um foco é neutralizado (tipo "whoosh" ou sinal satisfatório)  
* Som de notificação da rede social: Quando um novo comentário aparece no "tweeter" (tipo notification pop)  
* Som de barra de hospital: Alterações na ocupação (subindo \= alerta leve, descendo \= confirmação)  
* Ambiente urbano: Sons de cidade ao fundo (carros distantes, vozes leves) para contexto




7. **Fases do jogo (quantas e o que acontece em cada fase):** 

**Uma fase única** com progressão implícita:

**Fase 1 \- O Bairro em Crise (única fase):**

* **Cenário:** Duas quadras conectadas de um bairro urbano residencial  
* **Focos:** 15-20 criadouros espalhados predefinidos entre casas, ruas, terrenos baldios  
* **Dinâmica:** O jogador começa no hospital, sai para o bairro e tem **tempo limitado** (determinado pela barra de ocupação) para eliminar todos os focos  
* **Progressão implícita:** Conforme elimina focos, a barra desacelera; conforme avança, a rede social oferece feedback motivacional  
* **Vitória:** Todos os 15-20 focos eliminados \+ barra de ocupação estabilizada  
* **Derrota:** Barra atinge 100% antes de completar \= colapso hospitalar  
    
    
8. **Descrição de uma cena do jogo (descrição textual das ações da cena):** 

**Cena: Início no Hospital**

O jogador spawna na **recepção do hospital superlotado**. À sua frente, filas caóticas de pacientes com sintomas de dengue (febre, manchas vermelhas). Alguns sentados no chão por falta de leitos. Uma **enfermeira/médica** em primeiro plano (visual estático, sem animação) está visibilmente estressada.

No **HUD (canto superior):**

* Barra de ocupação hospitalar em **70%** (vermelha, em alerta)  
* Ícone de celular no canto inferior direito  
* Mensagem de contexto: "A equipe médica avisa: erradique os criadouros no bairro. Sem isso, o tratamento não adianta."

**Jogador clica no ícone de celular → expande a rede social:**

* 3-4 comentários negativos iniciais: "João Silva: Que abandono\! Meu filho tá com dengue 😞"  
* "Maria: Alguém faz algo?\!"  
* "Carlos: Hospital lotado, impossível atender"

**Cena: Explorando as Duas Quadras**

Saindo do hospital, o jogador entra em uma **rua urbana com casas residenciais**. À esquerda, casarões com quintais visíveis; à direita, rua com terreno baldio e comércios fechados.

**Primeira quadra:**

* 3-4 pneus empilhados em um quintal (com água visível \= focos)  
* 1-2 caixas d'água destampadas em telhados (visíveis à distância)  
* 2-3 vasos de plantas com água em calçadas  
* 1 poço aberto em terreno baldio

O jogador se aproxima de um **pneu com água** → **zumbido de mosquito intensifica** → interage (tecla de ação) → pneu muda para "sem água" (transição simples de estado visual)

**Feedback imediato:**

* Partículas de mosquitos desaparecem  
* Zumbido cessa  
* Mensagem na tela: "Foco eliminado\! Remova água parada de pneus para evitar criadouros."  
* Barra do hospital **desacelera ligeiramente**

**Ícone de celular pisca** → jogador clica → novo comentário aparece: "João Silva: Opa, alguém tá fazendo algo\! 👍"

**Segunda quadra:**

* Mais garrafa/recipientes plásticos com água  
* Tampas de latas em sarjeta  
* Mais vasos de plantas  
* Caixa d'água descoberta em outro imóvel

Mesmo padrão: buscar pelo áudio, interagir, eliminar, receber feedback visual e da comunidade.

**Cena: Vitória**

Após eliminar o 15º-20º foco, a **barra de ocupação hospitalar cai para \~30%** (controlada).

**Tela Final:**

* "MISSÃO CUMPRIDA\!"  
* Tempo total gasto: XX minutos  
* Focos eliminados: 20/20  
* Taxa de ocupação hospitalar: 35% (vermelho → amarelo → verde)  
* Mensagem educativa: "Você evitou o colapso do sistema de saúde\! Cada ação comunitária conta."  
* **Feed final da rede social:**  
  * "João Silva: Muito obrigado\! Meu filho está melhorando\! 💚"  
  * "Maria: Que trabalho incrível\! Comunidade unida\!"  
  * "Carlos: Hospital respirando melhor agora. Parabéns\!"


  


  


9. **Esboço da tela central do jogo (Destacar elementos presentes na cena e representá-los graficamente em baixo nível):**   
   **![][image1]**  
     
10. **Principais elementos da cena (liste elementos animados e não animados):** 

**Elementos Não-Animados (Estáticos/Mudança de Estado):**

* Casas residenciais  
* Ruas asfaltadas  
* Terreno baldio  
* Árvores/arbustos (cenário)  
* Pneus (com/sem água)  
* Caixas d'água (tampadas/destampadas)  
* Vasos de plantas (com/sem areia)  
* Garrafas/recipientes (cheios/vazios)  
* Tampas de latas (presentes/removidas)  
* Poços/caixas (abertos/fechados)  
* Edifícios do hospital (spawn inicial)

**Elementos Animados (Feedback Visual):**

* Partículas de mosquitos (aparecem/desaparecem ao eliminar foco)  
* Zumbido 3D de mosquito (espacializado, intensifica com proximidade)  
* Barra de ocupação hospitalar (sobe/desce dinamicamente)  
* Ícone de celular (pisca ao novo comentário)  
* Notificações da rede social (fade in/out)  
* Mensagens educativas (aparecem ao eliminar foco)  
    
    
11. **Ferramentas de desenvolvimento (mesmo que projeção):** 

    Unity 3D 

    

    

    

    

    

    

    

    

    

    

    

    

    

  


12. **Descrever como será a dinâmica da fase 1 do jogo.**

		**Estrutura Temporal:**

1. **Introdução (Hospital):** Jogador spawna no hospital, vê a situação crítica, recebe contexto via rede social  
2. **Exploração Ativa:** Sai do hospital e tem \~10-15 minutos (tempo determinado pela barra) para encontrar e eliminar os 15-20 focos nas duas quadras  
3. **Feedback Contínuo:** A cada foco eliminado:  
   * Barra de hospital desacelera  
   * Rede social oferece novo comentário (progressivamente mais positivo)  
   * Mensagem educativa aparece  
4. **Progressão:** Sem níveis de dificuldade — tudo está espalhado desde o início; desafio é **encontrar todos os focos a tempo**  
5. **Clímax:** Quando faltam poucos focos, barra diminui para mostrar sucesso  
6. **Resolução:** Eliminar os últimos focos desestabiliza a barra; Hospital volta ao normal  
7. **Encerramento:** Tela de vitória com estatísticas e feedback final da comunidade

**Mecânica Educacional:**

* Cada tipo de criadouro oferece uma **dica específica** ao ser eliminado  
* Reforça a importância de cada ação individual  
* A rede social mostra que **ações individuais têm impacto coletivo**  
* Final mostra que saúde pública é responsabilidade compartilhada

[image1]: <data:image/png;base64,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>
## Histórico de Alterações (Log)

### 2026-09-16 — Migração do Player: Rigidbody → CharacterController

**Objetivo:** Corrigir a movimentação do player que travava em pequenas elevações do terreno e ajustar o pulo para um valor mais realista.

**Problemas corrigidos:**
- Movimento "quebrado"/travado em qualquer pequena elevação do terreno (ex.: rampas de terreno).
- Pulo alto demais (era força 6f).

**Arquivos alterados:**
- `Assets/Scripts/mover.cs` — Reescrito: Rigidbody → CharacterController (cápsula), gravidade manual (-20), pulo via `TriggerJump()`.
- `Assets/Scripts/pular.cs` — Reescrito: usa `CharacterController.isGrounded` e chama `Mover.TriggerJump()`; removidos `jumpForce` e `groundCheckOffset`.
- `Assets/Scripts/MouseLook.cs` — Comentário atualizado (removida referência ao Rigidbody).
- `Assets/Scenes/SampleScene.unity` — Player: removidos `Rigidbody` e `BoxCollider`; adicionado `CharacterController`.

**Configurações no CharacterController (Inspector do Player):**
| Parâmetro | Valor | Efeito |
|---|---|---|
| Height | 1 | Altura da cápsula |
| Radius | 0.5 | Raio da cápsula |
| Slope Limit | 45° | Não sobe rampas acima de 45° |
| Step Offset | 0.3 | Sobe automaticamente degraus/elevações até 0.3 |
| Skin Width | 0.08 | Evita atravessar colisores |
| Min Move Distance | 0.001 | Evita "jitter" parado |

**Configurações do Mover (Inspector):**
- `moveSpeed`: 5
- `gravity`: -20
- `jumpForce`: 3

**Observações para futuras sessões de IA:**
- O player NÃO usa mais Rigidbody — o Mover usa CharacterController nativamente (resolve rampas e pequenas elevações sozinho).
- O Pular.cs apenas detecta a tecla Espaço e chama `Mover.TriggerJump()`; gravidade e impulso vertical ficam no Mover.
- A câmera é posicionada pelo MouseLook.cs como "olhos" do player (`eyeHeight = 1`).
### 2026-09-16 — Sistema de Interação em Primeira Pessoa (Focos de Criadouro)

**Objetivo:** Criar a base da mecânica de gameplay de eliminação de focos: o player mira em um objeto interativo (ex.: pneu) e clica para eliminá-lo após vários cliques.

**Problemas/necessidades atendidas:**
- Não existia nenhum sistema de interação no projeto.
- Mecânica do jogo (eliminar criadouros) precisa de um sistema reutilizável para pneus, caixas d'água, vasos, etc.

**Arquivos criados:**
- `Assets/Scripts/Interactable.cs` — Classe base genérica de foco:
  - `requiredClicks` (5), `interactionRange` (3), `scaleReductionPerClick` (0), `shrinkDuration` (1), `destroyOnComplete` (false).
  - Efeito padrão de conclusão: **encolhe até sumir** e desativa o objeto.
  - Hooks virtuais `OnInteract()`/`OnComplete()`/`OnStart()` para herança.
  - Propriedades públicas para HUD futura: `RequiredClicks`, `CurrentClicks`, `CanInteract`, `IsComplete`.
- `Assets/Scripts/PneuInteractable.cs` — Herda Interactable (foco do tipo PNEU):
  - `water` (referência ao filho "agua do pneu") e `clicksToDrainWater` (3): após 3 cliques a água é desativada (retirada), depois o pneu encolhe até sumir.
- `Assets/Scripts/Interactor.cs` — Script no PLAYER:
  - Raycast do centro da tela a cada frame; clique ESQUERDO do mouse interage (`Mouse.current.leftButton`).
  - Funciona com `GetComponentInParent<Interactable>()` (clicar na água também conta como clique no pneu).
  - Prompt via `OnGUI` (sem dependência de TextMeshPro): "Foco: X/Y — clique para eliminar". <sub>*(Este prompt e o campo `showPrompt` foram removidos depois — atualmente não há prompt/UI; ver "Remoção das Referências Órfãs de HUD" mais abaixo.)*</sub>

**Cena (`Assets/Scenes/SampleScene.unity`) — fiação:**
- Player: adicionado `Interactor` (fileID 1910821837) no prefab instance 246685078.
- Pneu (700591058): adicionado `PneuInteractable` (fileID 700591061) no prefab instance 700591057, com `water = {fileID: 446363895}` e `clicksToDrainWater = 3`.

**GUIDs dos scripts (usados nos .meta e na cena):**
- `Interactable.cs`: ebb64347c9a84eeebca4ad3e8760b2ff
- `Interactor.cs`: 73bdc91207b74295b3113cbc5abf7071
- `PneuInteractable.cs`: d385e67ded4a409db003692dcae14305

**Observações para futuras sessões de IA:**
- Novo foco pode ser criado apenas herdando `Interactable` e sobrescrevendo `OnInteract()`/`OnComplete()` (ou usar a base direto no GameObject).
- Requer um colisor no objeto (ou filho) para o raycast do Interactor funcionar.
- Ao clicar no filho "agua do pneu", o `GetComponentInParent` encontra o `PneuInteractable` do pneu automaticamente.
- Quando concluído e `destroyOnComplete = false` (padrão), o objeto fica `SetActive(false)` e pode ser reutilizado/respawnado.

### 2026-09-16 — Correção: Scripts Invisíveis no Unity (.meta Rejeitados)

**Objetivo:** Fazer os scripts do sistema de interação (Interactable, Interactor, PneuInteractable) aparecerem e serem importados pelo Unity — estavam invisíveis (assets ignorados).

**Problema (causa raiz):**
- Os 3 .meta foram criados À MÃO (GUIDs gerados externamente). O importador do Unity os rejeitou: "The contents of '...meta' could not be parsed"/"does not have a valid GUID and its corresponding Asset file will be ignored".
- Mesmo corrigindo o final do arquivo (faltava newline final), o Unity mantinha o estado de falha em cache (chegou a tentar "Extracting archive" nos .cs).
- Resultado: scripts invisíveis no Project e Assembly não compilava com eles.

**Correção aplicada (solução definitiva):**
- Deletados os 3 .meta à mão de Assets/Scripts/ e forçado Refresh do Unity (foco na janela + Ctrl+R).
- O Unity regerou os .meta (GUIDs novos) e recompilou Assembly-CSharp.dll sem erros (CompilerOutput: Assembly-CSharp 15:27).
- O Unity reescreveu a cena automaticamente, re-mapeando os componentes pelo m_EditorClassIdentifier e removendo um componente Interactor legado duplicado/orfão.

**GUIDs NOVOS (definitivos, gerados pelo Unity):**
- Interactable.cs: f0250d56a771d694097d720f6ac144d9
- Interactor.cs: f8b6ce2e3cd3ad4429dbdfa4d3770962
- PneuInteractable.cs: e52907f38c80ebc48aa72f8df9cf4490

**Fiação final da cena (foi reescrita pelo Unity; conferida em disco):**
- Player (prefab instance 246685078): Interactor = bloco MonoBehaviour fileID 1910821841, m_Script guid: f8b6ce2e..., interactionRange: 3, showPrompt: 1.
- Pneu "pneu" (700591058, prefab instance 700591057): PneuInteractable = fileID 700591061, m_Script guid: e52907f..., requiredClicks: 5, interactionRange: 3, scaleReductionPerClick: 0, shrinkDuration: 1, destroyOnComplete: 0, water: {fileID: 446363895}, clicksToDrainWater: 3.
- "agua do pneu" (446363895) continua como filho do pneu (446363896) e é desativada após 3 cliques.

**Observações para futuras sessões de IA (IMPORTANTE — lição):**
- NUNCA escrever/editar arquivos .meta à mão: o Unity rejeita e ignora o asset (scripts ficam invisíveis). CRIAR apenas o .cs, deixar o Unity gerar o .meta, e DEPOIS ler o GUID gerado no .meta para referenciar na cena.
- Se referências antigas quebrarem após regenerar metas, o próprio Unity re-mapeia scripts na cena pelo m_EditorClassIdentifier durante um Refresh — não tentar "consertar" na mão antes de deixar o Unity terminar.
- Para forçar o Unity a processar mudanças externas: focar a janela do Editor e pressionar Ctrl+R.

### 2026-09-16 — Remoção das Referências Órfãs de HUD (tentativa descartada)

**Objetivo:** Descartar a tentativa anterior de HUD (o usuário não aprovou e vai refazer diferente depois). Os scripts `HUDController.cs`, `GameManager.cs` e `SocialFeed.cs` citados nas antigas seções NUNCA existiram em disco — a única coisa que existia eram referências órfãs na cena. Esta entrada substitui as seções antigas de HUD, que foram apagadas.

**Arquivos alterados:**
- `Assets/Scenes/SampleScene.unity` — removidos do Player (prefab instance 246685078):
  - Bloco `MonoBehaviour &1910821845` (HUDController, guid `3b78fbbd...`) e seus campos `initialOccupancy`, `ratePerSecond`, `rateReductionPerFoco`, `occupancyDropPerFoco`, `totalFocos`, `fillLow/Mid/High`;
  - Entrada `addedObject {fileID: 1910821845}` no `m_AddedComponents` do prefab;
  - Campo fantasma `showPrompt: 1` do bloco `Interactor &1910821841` (campo não existe mais no script atual).
- Menções a `HUDController` nos comentários de `Interactor.cs`/`Interactable.cs` removidas (feitas junto com a melhoria de interação — ver próxima entrada).

**Observações:**
- A cena NÃO possui mais nenhum componente ou campo de HUD. Prompt/crosshair ficam ausentes até a nova versão da HUD.
- Menções a HUD no DESIGN do jogo (barra de lotação, celular, rede social, telas de vitória/derrota, etc.) foram MANTIDAS no corpo do documento — servem de especificação para a futura implementação.
- Nenhum `.meta` foi criado/alterado à mão (apenas edição de arquivos existentes).

### 2026-09-16 — Melhoria de Feedback Visual na Interação (highlight + punch + conclusão viva)

**Objetivo:** Deixar a interação com focos de criadouro "mais viva": destaque claro ao mirar, reação visível a cada clique e uma animação de conclusão com sinal (em vez do antigo encolher linear sem feedback).

**Achado corrigido no caminho:** o `Interact()` em disco NUNCA chamava `OnComplete()` — o foco nunca era concluído/removido. Agora, ao atingir `requiredClicks`, dispara `OnCompleted` + `OnComplete()`. (As seções antigas do log descreviam esse comportamento, mas o arquivo real não tinha a chamada.)

**Arquivos alterados:**
- `Assets/Scripts/Interactor.cs` — **highlight por emissão ao mirar:**
  - Ao mudar o alvo, aplica brilho pulsante (`_EMISSION`) em todos os `Renderer` do foco (inclusive filhos, ex.: "agua do pneu"), com pulso senoidal (`highlightColor`, `highlightIntensity`, `highlightPulseSpeed`).
  - Ao deselecionar, restaura cor/estado originais de cada material (instanciado via `renderer.material`, sem afetar o asset).
  - Comentários sem menção a HUD.
- `Assets/Scripts/Interactable.cs` — **punch + conclusão:**
  - `PlayPunch()`/`PunchRoutine()`: a cada clique (exceto o final) o objeto expande ~10% e volta com levesinho overshoot (`punchScale`, `punchDuration`).
  - `CompleteAnimation()` substitui o encolher linear: POP (expande 1.18x) → FLASH branco de emissão (restaura o material depois) + **estouro de partículas** (ParticleSystem criado em runtime, textura built-in `Default-Particle.psd`; shaders URP com fallback) → encolhe com ease-out + giro (`tiltDegrees`, 120°/s) + pulinho (`hopHeight`) → `FinishRemoval()`.
  - Configuráveis: `punchScale`, `punchDuration`, `shrinkDuration` (0.8), `hopHeight`, `tiltDegrees`, `flashColor`, `popColor`, `particleCount`.

**Verificação:**
- Cena conferida em disco: bloco `HUDController` removido, `m_AddedComponents` do Player com 5 itens (CharacterController, Mover, Pular, MouseLook, Interactor), sem `showPrompt`/guid HUD (grep limpo).
- `Interactor`/`Interactable` reescritos sem referências a HUD/showPrompt/OnGUI.
- Compilação validada via `dotnet build Assembly-CSharp.csproj`: **0 erros, 0 avisos** (LangVersion 9.0, mesmo do Editor). Ainda é preciso Confirmar com Ctrl+R/Play no Editor e checar `Editor.log`.

**Observações para futuras sessões de IA:**
- O highlight e o flash usam `_EMISSION` (shaders URP). Objeto completado e desativado → o Interactor restaura o material no frame seguinte.
- `PneuInteractable` não precisou mudar (herda os novos efeitos; remove a água no 3º clique como antes).
- `destroyOnComplete = false` (padrão) deixa o objeto desativado — para reutilizar, é só reativar; a escala volta ao normal via `Start()` (`initialScale`).
- O "pop" de partículas é destruído após 1.5s (não vaza objetos).
### 2026-09-16 — Correção da Movimentação do Player (tremedeira + arranque robótico)

**Objetivo:** Deixar a movimentação do player mais suave e natural — o usuário relatou movimento "meio estranho".

**Problemas corrigidos:**
- **Tremedeira ao andar:** `Mover` movia o CharacterController no `FixedUpdate` (50 Hz), enquanto o `MouseLook` posiciona a câmera no `LateUpdate` (a cada frame). Câmera e corpo ficavam fora de sincronia → imagem "tremendo"/serrilhada.
- **Arranque/parada instantâneos:** velocidade ia de 0 a máxima (e vice-versa) em um frame.
- **Direção dependia da câmera:** trocado para o `transform` do player (yaw já aplicado pelo MouseLook), sem influência do pitch.
- **Teto:** ao bater a cabeça, a velocidade vertical continuava positiva ("grudava" no teto).

**Arquivos alterados:**
- `Assets/Scripts/mover.cs` — reescrito:
  - Movimento no `Update` com `Time.deltaTime`.
  - Velocidade horizontal com `Vector3.MoveTowards` (novos campos: `acceleration` = 40, `deceleration` = 50, `airControl` = 0.5).
  - Usa `CollisionFlags` do `Move()`: zera subida ao bater no teto e não acumula velocidade contra paredes.
  - Mantidos `moveSpeed` (5), `gravity` (-20), `jumpForce` (3) e `TriggerJump()` (Pular.cs não mudou).

**Observações para futuras sessões de IA:**
- Os novos campos não existem na cena ainda — o Unity usa os valores padrão do script ao recompilar (nenhuma edição de cena/.meta).
- Valores atuais na cena (lidos em disco): `MouseLook.eyeHeight` = 1, `mouseSensitivity` = 1; CharacterController Height 1 / Radius 0.5 / Step Offset 0.3.

### 2026-09-16 — Novo Asset: Vaso de Flor (foco de criadouro)

**Objetivo:** Criar um novo tipo de foco de criadouro (vaso de flor com água parada), reaproveitando a arquitetura `Interactable`/`PneuInteractable` já existente. O usuário escolheu representar o vaso com **primitivas simples** do Unity (não há nenhum modelo pronto de "vaso com prato d'água" nos pacotes importados — só plantas/flores genéricas sem prato de água).

**Arquivos criados:**
- `Assets/Scripts/VasoInteractable.cs` — Herda `Interactable`, idêntico em estrutura ao `PneuInteractable`: remove o filho `water` ("agua do vaso") após `clicksToDrainWater` cliques, depois segue o fluxo padrão de conclusão (herdado).
- `Assets/Meus/vaso de flor.mat` — Material URP Lit cor terracota (`_BaseColor`/`_Color`: r 0.698, g 0.323, b 0.157), mesmo shader/estrutura do `agua do pneu.mat`.
- `Assets/Meus/Vaso de Flor.prefab` — Prefab autônomo (não é instância de outro prefab), montado só com primitivas internas do Unity:
  - Raiz "Vaso de Flor": `MeshFilter` com mesh `Cylinder` (builtin, fileID 10206), `MeshRenderer` com o material `vaso de flor.mat`, `BoxCollider` (size 1x2x1, cobrindo o cilindro), escala do objeto (0.5, 0.4, 0.5) → vaso baixo e largo. Componente `VasoInteractable` (requiredClicks: 3, clicksToDrainWater: 1, demais campos = padrão de `Interactable`).
  - Filho "agua do vaso": `MeshFilter` com mesh `Cube` (builtin, fileID 10202) achatado (escala local 0.85 x 0.05 x 0.85, mesma técnica usada em "agua do pneu"), posicionado perto do topo do cilindro (`m_LocalPosition.y = 0.85`), `MeshRenderer` reaproveitando o **mesmo material da água do pneu** (`agua do pneu.mat`, guid `55033aa5a50f0a74eb552c5daaf7ecd0`) para manter consistência visual entre focos de água parada, `BoxCollider` (size 1x1x1).

**GUIDs gerados pelo Unity (confirmados lendo os `.meta` após o Unity importar — nenhum `.meta` foi criado ou editado à mão, só os `.cs`/`.mat`/`.prefab` em si):**
- `VasoInteractable.cs`: `438dcddaa4693b34595c8ddf3ded4bd1`
- `vaso de flor.mat`: `b0c7a0e0f707689468be3ba1b515e511`
- `Vaso de Flor.prefab`: `ec7ba7d19f0de1b47b6d1304aea81fb4`

**Verificação feita:**
- `dotnet build Assembly-CSharp.csproj`: 0 erros, 0 avisos (script compila).
- Unity (Editor já aberto pelo usuário, janela focada via automação + Ctrl+R) gerou os `.meta` de todos os 3 arquivos novos sem rejeitá-los — confirma que o YAML dos assets (material e prefab) foi aceito pelo importador.
- **Ainda NÃO testado em Play Mode** — o prefab não foi arrastado para a cena. Falta o usuário (ou uma sessão futura) posicionar o prefab em `Assets/Scenes/SampleScene.unity` e testar a interação in-game.

**Observações para futuras sessões de IA:**
- O prefab foi criado como **arquivo autônomo** (não editei `SampleScene.unity` diretamente) por ser mais seguro: a cena tem ~2,9 milhões de linhas, e inserir objetos novos direto nela por edição de texto tem alto risco de erro sem o Unity aberto para validar visualmente. Um `.prefab` isolado é pequeno, fácil de revisar, e o usuário só precisa arrastá-lo da pasta `Assets/Meus` para a cena no Editor.
- Os prefabs de foco (`PneuInteractable`, `VasoInteractable`, futuros) seguem o mesmo padrão: um `GameObject` raiz com `Interactable`-derivado + um filho de água que é desativado (`SetActive(false)`) após N cliques.
- Para forçar o Unity a importar assets novos criados por fora do Editor, funcionou: focar a janela do Unity via automação (`SetForegroundWindow`/`AttachThreadInput`) + `SendKeys("^r")` (Ctrl+R). O `Logs/Editor.log` do projeto NÃO se mostrou confiável para confirmar importação em tempo real (parou de ser atualizado após o boot do Editor nesta sessão) — a confirmação real foi checar se o `.meta` correspondente apareceu no disco.
- Cores/tamanhos do vaso e da água são só um placeholder básico (primitivas) — podem ser ajustados livremente no Inspector sem qualquer necessidade de reabrir este processo.

### 2026-09-16 — Revisão do Vaso de Flor (versão mais "bonita")

**Objetivo:** O usuário não gostou do resultado anterior (um cilindro liso + um cubo achatado como água — muito cru) e **apagou o material e o prefab pelo próprio Editor** (`vaso de flor.mat` e `Vaso de Flor.prefab`; o script `VasoInteractable.cs` não foi tocado). Pediu uma versão que parecesse de fato um vaso de planta.

**O que mudou vs. a primeira versão:**
- Continua tudo primitivas internas do Unity (sem modelo de vaso pronto disponível nos pacotes do projeto — só plantas/flores soltas), mas agora com mais peças e uma planta de verdade em cima:
  - **Corpo do Vaso** (Cylinder, material terracota) + **Boca do Vaso** (Cylinder mais largo e baixo, mesmo material, dá o efeito de borda/lábio do vaso) — antes era só um cilindro liso.
  - **Terra** (Cylinder achatado, material marrom escuro novo `terra do vaso.mat`) visível dentro da boca do vaso, recuada abaixo da borda.
  - **agua do vaso** (Cylinder achatado, reaproveitando o material `agua do pneu.mat` já existente) — é o foco de fato, referenciado pelo `VasoInteractable.water`; some ao ser drenado.
  - **Planta**: reaproveita a mesh e o material já prontos de `SimpleNaturePack/Prefabs/Flowers_01.prefab` (mesh `Flowers_01_LOD0`, guid `b5a80b3e2a089b841b155d7c7f461e7c` / material guid `93caadf4359ae8143aad35b3145d31af`) posicionada saindo da terra — dá o visual reconhecível de "planta num vaso" em vez de só terracota vazia.
- Todos os filhos agora ficam direto sob a raiz "Vaso de Flor" (hierarquia plana), com posições/escalas em unidades reais (raiz com escala 1,1,1, sem distorcer os filhos) — mais fácil de ajustar no Inspector do que a v1 (que escalava a raiz e "compensava" a escala da água).

**Arquivos recriados/criados:**
- `Assets/Meus/vaso de flor.mat` — recriado (terracota, mesma cor de antes).
- `Assets/Meus/terra do vaso.mat` — novo (marrom escuro, `_BaseColor`/`_Color` r 0.197, g 0.122, b 0.075).
- `Assets/Meus/Vaso de Flor.prefab` — recriado com a hierarquia de 5 partes descrita acima (`Corpo do Vaso`, `Boca do Vaso`, `Terra`, `agua do vaso`, `Planta`), `BoxCollider` na raiz (0.6 x 0.9 x 0.6) para o raycast do `Interactor`, e `VasoInteractable` (requiredClicks: 3, clicksToDrainWater: 1) apontando para `agua do vaso`.

**GUIDs confirmados (lidos dos `.meta` gerados pelo Unity após Ctrl+R; nenhum `.meta` editado à mão):**
- `vaso de flor.mat`: `b0c7a0e0f707689468be3ba1b515e511` (Unity reaproveitou o mesmo guid da v1 ao recriar o arquivo no mesmo caminho)
- `terra do vaso.mat`: `5e8f0fd668bbdc54daf5462c90ae84ad`
- `Vaso de Flor.prefab`: `ec7ba7d19f0de1b47b6d1304aea81fb4` (mesmo guid da v1)

**Ainda não testado em Play Mode / nem colocado na cena** — segue pendente o usuário arrastar o prefab pra `SampleScene.unity` e conferir visualmente/jogando.

**Observações para futuras sessões de IA:**
- Reaproveitar mesh+material de um prefab de outro pacote (aqui, `Flowers_01`) como filho comum (não como prefab aninhado) funciona bem e é bem mais simples/seguro do que tentar aninhar um `PrefabInstance` de verdade à mão — só referenciar o guid da mesh e do material do FBX/pacote original direto no `MeshFilter`/`MeshRenderer`.
- Se o resultado visual ainda não agradar, o caminho mais barato é ajustar cores/proporções no Inspector (é tudo primitiva, edição rápida) antes de partir para outra revisão via código/arquivo.

### 2026-09-16 — Vaso de Flor com Pratinho (v3, malhas de revolução + mecânica de areia)

**Objetivo:** O usuário apagou de novo a v2 (primitivas) e pediu um asset de **vaso de flor com pratinho embaixo**, preparado para a mecânica do GameDoc "prato sem areia → interagir → prato com areia".

**Abordagem:** Em vez de escrever `.prefab`/`.mat` à mão (GUIDs/fileIDs frágeis), foi criado um **script de Editor** que gera tudo dentro do Unity. As peças são malhas de revolução (perfil 2D girado em torno de Y, 48 segmentos), então o vaso afunila e tem lábio, e o prato tem aba levantada. Winding/normais conferidos fora do Unity (0 triângulos invertidos) e prévia renderizada.

**Arquivos criados/alterados:**
- `Assets/Editor/VasoComPratoBuilder.cs` (novo) — menu **Dengue > Criar Vaso de Flor com Pratinho**. Gera em `Assets/Meus/Vaso de Flor/`:
  - `Malha Vaso/Prato/Terra/Agua/Areia.asset` (Mesh), materiais `barro do vaso`, `barro do prato`, `terra do vaso`, `areia do prato` (URP Lit; HDRP/Standard como fallback). Água reaproveita `Assets/Meus/agua do pneu.mat` (se não existir, cria `agua do prato.mat`).
  - `Vaso de Flor.prefab`: raiz com `BoxCollider` (0.54 x 0.44 x 0.54, centro y 0.22) + `VasoInteractable` (requiredClicks 3). Filhos: `Prato`, `Vaso`, `Terra`, `agua do prato`, `areia do prato` (pivô y=0.022, inativo), `Planta` (cópia desempacotada de `SimpleNaturePack/Prefabs/Flowers_01.prefab`, colliders removidos, escalada para ~35 cm).
  - Pode ser executado de novo: reaproveita os arquivos existentes (mantém GUIDs).
  - Medidas: vaso ~44 cm de altura, prato ~54 cm de diâmetro; piso do prato em y=0.022, água em y=0.048.
- `Assets/Scripts/VasoInteractable.cs` (reescrito) — campos `water` (GameObject), `sand` (Transform), `minSandFill` 0.4, `sandFillDuration` 0.25, `removeOnComplete` false. 1º clique desativa a água e mostra a areia; cada clique anima a escala Y da areia até encher; ao concluir dispara `OnCompleted` e o vaso **permanece** na cena (a menos que `removeOnComplete`). Propriedade pública `SandFill` (0–1). Campo antigo `clicksToDrainWater` removido.
- `Assets/Scripts/Interactor.cs` — `RaycastTarget()` ignora `Interactable` com `CanInteract == false` (focos concluídos que ficam na cena não recebem mais destaque).

**Pendente:** o usuário rodar o menu no Editor, arrastar o prefab para `SampleScene.unity` e testar em Play Mode. Não foi possível compilar fora do Unity nesta sessão (sem acesso ao shell do PC).

**Observações para futuras sessões de IA:**
- Para a mecânica futura de "inserir areia" (ex.: exigir que o player carregue um saco de areia), o ponto de extensão é `VasoInteractable.OnInteract()` — checar o item antes de chamar `SetSandFill`.
- Ajustes de forma: editar os arrays `*Profile` (raio, altura) no builder e rodar o menu de novo.

### 2026-09-17 — Celular com Rede Social (aparelho low poly + feed da comunidade)

**Objetivo:** Implementar a mecânica de feedback social do GameDoc — o "tweeter" do bairro com comentários que evoluem de negativos para positivos conforme o jogador elimina focos. O usuário pediu explicitamente um **design caprichado do aparelho, em estilo low poly** (nada de retângulo liso).

**Arquivos novos:**
- `Assets/Scripts/RedeSocial.cs` — modelo do feed. Cada `PostRedeSocial` tem autor, @usuario, texto, curtidas, `humor` (Negativo/Neutro/Positivo) e **`progresso` (0 a 1)**: o post é publicado quando a *fração* de focos eliminados alcança esse valor. Por ser fração, funciona igual com 2 focos de teste ou com os 20 focos do GameDoc. Os posts saem em **fila** (atraso inicial 1,2 s, intervalo 1,4 s) para não despejarem todos de uma vez. Conta não lidos (`NaoLidos`), expõe `OnPostPublicado` / `OnNaoLidosMudou`. Registra os focos igual ao `DengueHUD` (`FindObjectsByType<Interactable>` + `OnCompleted`). Traz 13 publicações padrão prontas (reclamação → dúvida → dica educativa → resultado → agradecimento), editáveis pelo Inspector.
- `Assets/Scripts/PostView.cs` — só as referências de um card (avatar, inicial, nome, usuário, texto, curtidas, faixa de humor).
- `Assets/Scripts/FeedRedeSocial.cs` — a tela do app. Clona o card modelo a cada publicação, insere **no topo** com fade + escala, cor de avatar fixa por autor (hash do nome), tempo relativo ("agora" → "3 min" → "1 h"), relógio da barra de status com a hora real, subtítulo com o contador de focos e rolagem suave (`Rolar`, `IrParaTopo`).
- `Assets/Scripts/CelularController.cs` — o aparelho na mão. **Tecla T** levanta/guarda; subida com *overshoot* (`EaseOutBack`) e descida seca (`EaseInCubic`); *sway* do mouse (o celular fica atrás do movimento), respiração lenta quando levantado e **vibração** quando chega post com o celular guardado (tela acende + ícone pulsa + `PlayOneShot` opcional). Enquanto levantado: `Interactor.Bloqueado = true`, canvas da `DengueHUD` desligado e a rodinha do mouse rola o feed.
- `Assets/Editor/CelularBuilder.cs` — menu **Dengue > Criar Celular**. Gera tudo na cena, preso na Main Camera.

**Arquivo alterado:**
- `Assets/Scripts/Interactor.cs` — nova propriedade `public bool Bloqueado { get; set; }`. Quando true, o `Update()` larga o alvo (desfaz o destaque) e sai. Foi a mudança mínima para o celular suspender a interação sem deixar o brilho preso no último foco mirado.

**Como o aparelho é modelado (não é cubo esticado):**
- O corpo é um **contorno de retângulo arredondado varrido em profundidade**: 4 cantos × 4 segmentos (facetado de propósito) e um perfil (recuo, z) com **chanfro na frente e atrás** — é o chanfro que pega a luz e dá o acabamento low poly.
- Peças: `Corpo`, `Tela` (chapa arredondada saliente, material com emissão), `Modulo Camera` em relevo nas costas, `Lente 1/2` (aro de metal + vidro), `Flash`, `Botao Power`, `Botao Volume`, `Alto-falante` e `Camera Frontal` no bisel.
- Medidas reais: **7,6 × 15,4 × 0,78 cm**. Assets em `Assets/Meus/Celular/` (`Malha Corpo/Tela/Modulo.asset` + materiais `corpo/metal/tela/lente/flash do celular`). O builder reaproveita os arquivos existentes (mantém GUIDs) e pode ser rodado de novo.

**App (Canvas World Space dentro do aparelho):** 700 × 1420 px em escala 0,0001 (= 7 × 14,2 cm), rotacionado 180° em Y para ficar legível de frente. Barra de status (hora, sinal, bateria), cabeçalho "VizinhApp" com subtítulo `Vila Nova · {eliminados}/{total} focos eliminados` e ponto "ao vivo", viewport com `RectMask2D`, conteúdo com `VerticalLayoutGroup` + `ContentSizeFitter`, card modelo (`ModeloPost`, desativado) e home indicator.

**HUD:** canvas próprio `CelularHUD` (sortingOrder 9) com o ícone do celular no canto inferior direito, desenhado com Images (corpo, borda, tela, alto-falante, home), **badge vermelho de não lidos** e a dica `[T]`. O ícone some enquanto o celular está levantado.

**Ajuste automático:** o builder reduz o `Near Clip Plane` da Main Camera para 0,03 (com o padrão 0,3 o aparelho seria cortado, já que fica a ~20 cm da câmera).

**Pendente:** rodar `Dengue > Criar Celular` no Editor e testar em Play Mode. Não foi possível compilar nem rodar nesta sessão (o shell do PC do usuário está indisponível por causa de uma atualização do Windows de 08/09).

**Observações para futuras sessões de IA:**
- Ordem dos vértices das malhas varridas: faixa lateral usa (a0, b0, a1) / (a1, b0, b1) e as tampas usam leque invertido na frente — conferido no braço, mas se alguma face aparecer invertida no Editor é só trocar a ordem em `CorpoChanfrado`/`Tampa`.
- Para acrescentar publicações, use o Inspector do componente `RedeSocial` (objeto `Celular`): basta preencher texto e `progresso`. Não precisa mexer em código.
- Ganchos prontos para a próxima etapa: `CelularController` já tem campos de áudio (`somNotificacao/Abrir/Fechar`) esperando clipes, e `RedeSocial.Progresso` serve de entrada para a futura barra de ocupação hospitalar.

### 2026-09-17 — Celular refeito: só HUD 2D (versão 3D descartada)

**Objetivo:** O usuário rodou a versão anterior (aparelho 3D preso na Main Camera), **não gostou** e pediu o celular inteiro pela HUD, "muito mais simples, sem 3D".

**O que saiu:**
- `Assets/Editor/CelularBuilder.cs` foi **reescrito do zero**: agora só monta um Canvas Screen Space Overlay. Sumiram as malhas de revolução/chanfro, os materiais, as lentes, os botões laterais, o Canvas World Space e o ajuste de near clip.
- `Assets/Scripts/CelularController.cs` foi **reescrito**: em vez de pose 3D, sway do mouse, respiração e emissão de material, agora só anima `anchoredPosition` + `alpha` + `localScale` do painel.
- O builder novo **limpa a versão 3D sozinho** ao rodar: destrói o objeto `Celular` de dentro da Main Camera, devolve o Near Clip Plane para 0.3 (se ainda estiver em 0.03) e apaga a pasta `Assets/Meus/Celular` (malhas `Malha Corpo/Tela/Modulo` e materiais `corpo/metal/tela/lente/flash do celular`).

**O que ficou igual (não precisou mudar, não depende de 3D):**
- `RedeSocial.cs` — publicações com `progresso` (fração de focos), fila, não lidos.
- `PostView.cs` e `FeedRedeSocial.cs` — cards, avatar com inicial, tempo relativo, relógio, scroll.
- `Interactor.cs` — propriedade `Bloqueado`.

**Layout novo (referência 1920x1080):** Canvas `CelularHUD` (sortingOrder 11, acima da DengueHUD).
- `Painel` 420 × 780 no canto inferior direito (margem 40), com moldura de 2 px. Fechado fica em y = -840 (fora da tela) com alpha 0.
- Dentro: `StatusBar` (44 px: hora, 3 barrinhas de sinal, bateria), `Header` (84 px: "VizinhApp", subtítulo `Vila Nova · {eliminados}/{total} focos eliminados`, ponto verde "ao vivo", divisor), `Viewport` com `RectMask2D` + `Conteudo` (`VerticalLayoutGroup` + `ContentSizeFitter`) + `ModeloPost` (card desativado com `PostView`), `Vazio` e `Rodape` ("[T] fechar · rodinha do mouse rola o feed").
- `Icone` 54 × 94 no mesmo canto, com moldura, tela, alto-falante, home, **badge vermelho** de não lidos e a dica `[T]`. Some quando o painel abre.

**Animação:** abre em 0,32 s com `EaseOutBack` (sobe do canto, com leve esticada) e fecha em 0,20 s com `EaseInCubic`. Publicação nova com o painel fechado: o ícone **sacode**, pulsa e a tela do ícone acende.

**Confirmado no `Editor.log` da sessão anterior:** os scripts compilaram sem nenhum `error CS` e o builder 3D rodou até o fim (`[Celular] Pronto.`). Ou seja, a base (RedeSocial/Feed/PostView) está válida — o que mudou foi só a apresentação.

**Observações para futuras sessões de IA:**
- O shell do PC do usuário segue indisponível (atualização do Windows de 08/09) e o Unity dele roda **como administrador**, o que bloqueia automação de teclado/mouse por UIPI. Para rodar menus do Editor por automação, seria preciso o Unity aberto sem elevação.
- Pendência anterior, sem relação com o celular: `Interactable.cs` linha 303 (`CreateParticleMaterial`) tenta carregar `Default-Particle.psd`, que não existe no URP — dá erro em runtime ao concluir um foco.

### 2026-09-18 — Rede social: incentivo a cada 1–3 focos limpos

**Objetivo:** O usuário pediu que, a cada 1, 2 ou 3 focos limpos, apareça uma publicação incentivando o jogador. Antes o gatilho era a **fração** de progresso (`progresso` de 0 a 1), que dependia do total de focos da cena; agora é por **contagem de focos**.

**`Assets/Scripts/RedeSocial.cs` (reescrito):**
- O campo `progresso` de `PostRedeSocial` **deixou de existir**. As publicações agora estão em três listas separadas, cada uma com seu momento:
  - `postsIniciais` — abertura, saem assim que a fase começa (4 posts, o bairro reclamando).
  - `postsDeIncentivo` — saem **a cada 1, 2 ou 3 focos limpos**, na ordem da lista (16 posts: elogio + dica educativa, esquentando de Neutro para Positivo).
  - `postsFinais` — saem quando `eliminados >= total` (3 posts de comemoração). Quando o último foco cai, entram no lugar do incentivo.
- Campos novos no Inspector: `minFocosPorPost` (1), `maxFocosPorPost` (3) e `incentivoNoPrimeiroFoco` (true — a primeira publicação sai já no primeiro foco, para o jogador entender na hora que limpar foco mexe com o bairro). O intervalo é sorteado com `Random.Range(min, max + 1)` a cada publicação.
- Se os incentivos acabarem antes dos focos (ex.: 20 focos e 16 posts), ele passa a **sortear** entre os existentes evitando repetir o último — nunca fica mudo.
- A fila com `atrasoInicial`/`intervaloEntrePosts` continua igual, assim como `NaoLidos`, `MarcarComoLido()`, `OnPostPublicado`, `OnNaoLidosMudou` e a propriedade `Progresso` (que segue servindo de entrada para a futura barra de ocupação hospitalar).

**`Assets/Scripts/FeedRedeSocial.cs`:** o texto do post agora passa pelo novo método `Marcadores()`, que troca `{eliminados}`, `{total}` e `{restantes}` pelos números **do momento da publicação** (o card fica congelado com esses valores, como um post de verdade). O mesmo método passou a alimentar o subtítulo do cabeçalho. Vários incentivos padrão já usam esses marcadores, ex.: "Faltam {restantes} no bairro — seguimos!".

**Nada mudou** em `CelularBuilder.cs`, `CelularController.cs` e `PostView.cs` — o builder não referencia nenhum campo mexido.

**Atenção ao atualizar:** como o formato da lista mudou, a `RedeSocial` que já estiver na cena vem com as listas **vazias** (o Unity descarta a lista antiga `posts`). Elas se preenchem sozinhas com o padrão no `Awake`, mas se você tiver editado textos pelo Inspector, eles se perdem — rode `Dengue > Criar Celular` de novo ou confira a lista no Inspector.

### 2026-09-18 — Novo foco: Caixa d'Água destampada (abre fechada com a tampa assentando)

**Objetivo:** Implementar o foco do GameDoc "caixa d'água destampada → interagir → caixa tampada". A caixa nasce **aberta**, com a tampa encostada na lateral e a lâmina d'água à mostra; o jogador clica e a tampa vai assentando até fechar.

**Arquivos novos:**
- `Assets/Scripts/CaixaDaguaInteractable.cs` — herda `Interactable`, mesmo padrão do `VasoInteractable` (`OnStart`/`OnInteract`/`OnComplete`).
  - A tampa é um **filho** com duas poses guardadas no Inspector: `posAberta` (0.44, 0.45, 0) / `rotAberta` (0,0,62) e `posFechada` (0, 0.762, 0) / `rotFechada` (0,0,0).
  - A cada clique o alvo é `CurrentClicks / RequiredClicks`, e uma corrotina interpola posição (`Vector3.Lerp`) e rotação (`Quaternion.Slerp`) com ease-out em 0,3 s. Não é um snap no final: a tampa anda um pedaço por clique.
  - `alturaDoArco` (0.20) levanta a tampa no meio do caminho (`sin(p·π)`), senão ela atravessaria o lábio da boca. Conferido em prévia: no meio do trajeto a saia passa acima de y=0.765.
  - `EstimarProgresso()` projeta a pose atual no caminho para o arco continuar coerente se o jogador clicar rápido e interromper a animação.
  - `OnComplete()` força `MoverTampa(1f)` (assentamento exato) e, com `removeOnComplete = false`, a caixa **fica na cena** fechada. A água é desativada ao fechar (evita z-fighting e economiza um draw).
  - Propriedade pública `Fechamento` (0 a 1).
- `Assets/Editor/CaixaDaguaBuilder.cs` — menu **Dengue > Criar Caixa d'Agua**. Mesma técnica de torno (`Lathe`) do `VasoComPratoBuilder`, 28 segmentos.

**Modelo:** caixa redonda de polietileno, ~94 cm de diâmetro por 77 cm de altura (uma 500 L), pivô no centro da base (é só largar no chão).
- `Malha Corpo` — fundo arredondado, parede levemente cônica, **nervura de reforço** em y 0.625–0.660, lábio na boca e parede interna descendo até o fundo interno em y=0.470 (é o que se vê olhando para dentro).
- `Malha Tampa` — domo com **saia** que encaixa por fora do lábio (raio interno 0.470 contra lábio 0.465, folga de 5 mm); pivô no plano de assentamento.
- `Malha Puxador` — pegador no topo do domo, filho da tampa.
- `Malha Agua` — lâmina d'água em y=0.478, reaproveitando `Assets/Meus/agua do pneu.mat` (mesmo material dos outros focos).
- Materiais novos: `caixa dagua.mat` (azul 0.16/0.40/0.68) e `tampa da caixa.mat` (azul mais escuro).
- Prefab `Assets/Meus/Caixa Dagua/Caixa d'Agua.prefab`: `CapsuleCollider` na raiz (raio 0.47, altura 0.80, centro y 0.40), `CaixaDaguaInteractable` com `requiredClicks = 4` e `punchScale = 0.03` (caixa grande, punch discreto) e `FocoHUDInfo` já preenchido (nome "a caixa d'água", texto ao mirar, dica educativa e mensagem de conclusão).

**Verificação feita (sem Unity, já que o shell do PC segue fora):** o torno e os quatro perfis foram portados para Python — **0 faces invertidas** em 1.904 triângulos (winding conferido contra as normais escritas) e prévia renderizada nos três estados (aberta, meio do caminho, fechada), que foi o que pegou dois erros de pose: a tampa aberta flutuando (corrigido de y 0.50 para 0.45, agora apoiada no chão) e o arco baixo demais, que fazia a tampa passar por dentro do lábio (0.12 → 0.20).

**Pendente:** rodar `Dengue > Criar Caixa d'Agua` no Editor, arrastar o prefab para a cena e testar em Play Mode.

**Observações para futuras sessões de IA:**
- A regra do torno (`Lathe`): percorra o perfil deixando o material à esquerda — a normal sai à direita do sentido de caminhada. Perfil de fora subindo, de dentro descendo. Foi assim que o `CorpoProfile` e o `TampaProfile` ficaram com as normais certas de primeira.
- O `DengueHUD.NomeDoFoco()` tem um `switch` por tipo (`PneuInteractable`, `VasoInteractable`) que **não** conhece a caixa — por isso o prefab já vem com `FocoHUDInfo.nome` preenchido, que tem prioridade. Novos focos devem fazer o mesmo em vez de editar o `DengueHUD`.

### 2026-09-18 — Terreno Baldio populado (executado o builder já existente)

**Objetivo:** O usuário pediu para popular o terreno baldio. Toda a base já existia no projeto (malhas/materiais/prefabs em `Assets/Meus/Terreno Baldio/` e o `Assets/Editor/TerrenoBaldioBuilder.cs` com o menu **Dengue > Popular Terreno Baldio**), mas nunca tinha sido rodada — não existia `"Cenario Terreno Baldio"` na cena. Não foi criado nenhum script/asset novo nesta sessão, só executado o que já estava pronto.

**Como foi executado:** o shell local não tinha o pacote `com.unity.pipeline` (Unity CLI) instalado neste projeto. Passos:
1. `unity pipeline install` (adiciona `com.unity.pipeline` ao `Packages/manifest.json`).
2. O Editor já aberto não pegou o pacote novo sozinho — foi preciso focar a janela dele (`SetForegroundWindow`) e mandar Ctrl+R (mesma técnica já documentada neste arquivo para forçar reimport externo) para o `unity status` passar a enxergá-lo.
3. Com o Editor conectado (`unity command`), rodei um script C# via `unity command eval_file` (não dá pra chamar `Popular(Terrain)` direto porque é método privado de instância do `EditorWindow`): criei uma instância de `TerrenoBaldioBuilder` com `ScriptableObject.CreateInstance`, achei o `Terrain` "Terreno Baldio" na cena e invoquei `Popular()` por reflection (`BindingFlags.NonPublic | Instance`), depois `EditorSceneManager.SaveOpenScenes()` dentro do próprio script.
4. **Achado de sintaxe do CLI:** `unity command eval_file <arquivo>` sozinho (posicional) faz o parâmetro `timeout` do comando cair no default de 5000 ms — pouco tempo pra `Popular()` (chega a instanciar centenas de objetos + pintar mato no Terrain). Só funcionou passando os parâmetros nomeados explicitamente: `unity command eval_file --file "<caminho>" --timeout 60000` (o `--timeout` nomeado aqui vai para o parâmetro do comando, não para o timeout genérico do CLI).

**Resultado (com os parâmetros padrão do builder — semente 7, densidade 1.0, margem 2 m, tudo marcado):** 214 objetos criados em `"Cenario Terreno Baldio"` dentro da cena — Monte de Entulho 4, Monte de Areia 2, Pilar Quebrado 1, Colchão Velho 1, Árvore Rala 2, Pilha de Tábuas 2, Pilha de Tijolos 2, Toco 2, Caixote 4, Laje Partida 5, Pneu Velho 3, Monte de Lixo 3, Arbusto 15, Saco de Lixo 8, Pedra 13, Tábua Solta 7, Galho 11, Pedaço de Concreto 15, Tijolo 18, Garrafa 13, Lata 13, Tufo Seco 26, Tufo de Mato 44 — mais mato pintado no Terrain (3 camadas de Detail, resolução 1024). O builder também rodou a conversão de materiais Built-in → URP (`_consertarMateriais`, marcado por padrão) antes de popular.

**Arquivos alterados:**
- `Assets/Scenes/SampleScene.unity` — só via o Editor (salvo com `EditorSceneManager.SaveOpenScenes()`), nenhuma edição manual do YAML. Confirmado em disco: `"Cenario Terreno Baldio"` aparece 1x na cena.
- `Packages/manifest.json` — ganhou a dependência `com.unity.pipeline` (necessária para o CLI controlar o Editor ao vivo). Nenhum outro arquivo do projeto foi tocado.

**Observações para futuras sessões de IA:**
- O builder respeita o player e os focos existentes (raio livre configurável, `_raioPlayer`/`_raioFocos`) e evita colisores já presentes na cena (`_respeitarCena`) — então rodar de novo com outra semente não deveria enterrar nada importante.
- Para rodar de novo com outros parâmetros (densidade, semente etc.), é só abrir a janela pelo menu `Dengue > Popular Terreno Baldio` no Editor e ajustar no Inspector da janela — ou repetir o mesmo truque de `eval_file` com reflection, setando os campos privados (`_semente`, `_densidade`, ...) antes de chamar `Popular()`.
- Botão "Limpar" da mesma janela (`Limpar(Terrain)`) remove tudo que esse builder colocou (raiz `Cenario Terreno Baldio` + mato pintado) — usar se o resultado não agradar, antes de rodar de novo com outra semente.
- Pendente: conferir visualmente no Editor/Play Mode se a densidade/distribuição ficou boa e se algum objeto gerado atrapalha a passagem do player ou a mira nos focos de criadouro.

### 2026-09-18 — Correção: assets do terreno baldio brancos/roxos (materiais quebrados)

**Objetivo:** O usuário reportou que, depois de popular o terreno baldio, alguns assets apareciam brancos ou roxos. Usei screenshots da Scene View (via `unity command screenshot`) para comparar antes/depois de cada tentativa em vez de adivinhar.

**Causa 1 — tingimento vermelho errado (efeito "roxo"):** `Assets/SimpleNaturePack/Materials/SimpleNaturePack_Texture_01.mat` (material compartilhado por praticamente todo o pacote SimpleNaturePack — árvores, pedras, arbustos, galhos, tocos, cogumelos, cada um amostrando uma cor por UV numa textura-paleta `NaturePackLite_Texture_01.png`) estava com `_BaseColor`/`_Color` = vermelho (0.96, 0, 0) em vez de branco. Isso tingia toda a paleta de vermelho/rosa. Confirmado como bug (não intencional) comparando com o material irmão `CartoonLowPolyCityLite_01.mat`, que usa a mesma técnica de paleta e já estava branco. **Corrigido**: `_BaseColor`/`_Color` → branco (1,1,1,1), direto no Editor ao vivo (`AssetDatabase`, sem editar `.mat` à mão).

**Causa 2 — GPU Instancing desligado (efeito "branco" no entulho estático):** Todos os materiais gerados por `TerrenoBaldioAssets.MaterialDe()` (tijolo, concreto, terra, areia, madeira, etc.) e os dois materiais de pacote acima nasciam com `enableInstancing = false`. Combinado com `StaticEditorFlags.BatchingStatic` (aplicado pelo `TerrenoBaldioBuilder` em todo objeto colocado), isso fazia vários objetos (concreto, monte de areia, pneu velho, tijolos...) renderizar em branco no URP. **Corrigido**: liguei `enableInstancing = true` nos 14 materiais gerados em `Assets/Meus/Assets Gerados/Terreno Baldio/Materiais/` e nos 2 materiais de pacote.

**Causa 3 — mato do Terrain (Detail) branco:** A maior parte dos "gravetos brancos" espalhados pelo chão não eram os objetos avulsos (esses já estavam com a cor certa, dava pra comparar lado a lado num screenshot aproximado), e sim a camada de **Detail** pintada no Terrain (`TerrenoBaldioMato.cs`). O `DetailPrototype` era criado com `dp.useInstancing = false` — no sistema de Detail do Terrain do Unity 6/URP isso faz o mato cair num caminho de renderização que ignora a cor real do material. **Corrigido**: `Assets/Editor/TerrenoBaldioMato.cs` → `dp.useInstancing = true`; recompilei (`unity command recompile`) e repintei o mato chamando `TerrenoBaldioMato.Pintar(terreno, 7, 1.0f, 2.0f)` de novo (mesma semente/densidade/margem da populada original) para os `DetailPrototype` serem recriados já com instancing ligado.

**Como foi feito:** tudo via `unity command eval_file` no Editor já aberto (mesma técnica da populada original) — nenhum `.mat`/`.unity`/`.asset` editado à mão. Usei `unity command screenshot --view scene` repetidamente para validar visualmente cada tentativa (o "antes"/"depois" ficou registrado nos prints, não só no relatório de texto).

**Atualização da mesma sessão — o "branco intencional" também era bug:** achei que os poucos objetos ainda brancos (2 "Árvore Rala", algumas "Pedra") fossem uma variante legítima do pacote (célula branca da paleta, tipo árvore de flor branca). O usuário pediu para corrigir mesmo assim, e ao investigar a fundo achei a causa real: o `SimpleNaturePack_Texture_01.mat` estava com **`_EMISSION` ligada e `_EmissionColor` = branco (1,1,1,1)** — o material "brilhava" branco puro por cima de qualquer textura/luz, disfarçando a cor real da paleta. Isso também veio da conversão Built-in→URP: o `TerrenoBaldioMateriais.Converter()` lê `_EmissionColor` do material antigo e liga `_EMISSION` sempre que essa cor não é preta, sem checar se a emissão estava de fato habilitada no shader original (o HDRP Lit, usado antes, guarda um `_EmissionColor` branco por padrão mesmo com emissão desligada) — carregou esse branco "órfão" para o URP e ligou emissão à toa. Confirmado comparando com o material-irmão `CartoonLowPolyCityLite_01.mat`, que sempre teve `_EMISSION` desligada.

**Correção final:** `_EMISSION` desligada + `_EmissionColor` = preto + `globalIlluminationFlags = EmissiveIsBlack` em `SimpleNaturePack_Texture_01.mat` (e também em `SimpleNaturePack_BG.mat`, mesmo bug, mesmo material-mestre do pacote, achado numa varredura do projeto inteiro — não é usado na cena atual, mas corrigido por consistência). Confirmado por screenshot: árvore virou copa verde de verdade, arbustos/pedras passaram a mostrar as cores certas da paleta (cinza, verde, etc.) em vez de branco estourado. A pista que me fez desconfiar de emissão em vez de textura: uma amostragem programática da textura-paleta pelas UVs da malha (`Texture2D.GetPixelBilinear`) não achava nenhuma célula branca nos objetos "brancos" — ou seja, a textura em si nunca foi a fonte do branco.

**Observação nova para futuras sessões de IA:** `TerrenoBaldioMateriais.Converter()` (e qualquer conversão Built-in/HDRP → URP parecida) pode ligar `_EMISSION` por engano em materiais HDRP que tragam `_EmissionColor` não-preto "órfão" no serializado, mesmo com a emissão de fato desligada no material original. Se algo aparecer "brilhando"/branco estourado depois de converter, checar `mat.IsKeywordEnabled("_EMISSION")` e `mat.GetColor("_EmissionColor")` antes de desconfiar da textura ou do tingimento.

**Arquivos alterados:**
- `Assets/Editor/TerrenoBaldioMato.cs` — `dp.useInstancing = false` → `true`.
- `Assets/SimpleNaturePack/Materials/SimpleNaturePack_Texture_01.mat` — `_BaseColor`/`_Color` vermelho → branco; `enableInstancing` → true.
- `Assets/CartoonLowPolyCityLite/Materials/CartoonLowPolyCityLite_01.mat` — `enableInstancing` → true (já estava branco).
- `Assets/Meus/Assets Gerados/Terreno Baldio/Materiais/*.mat` (14 materiais: areia, borracha, concreto, ferrugem, folha, folha seca, madeira, metal, papelao, plastico, tecido, terra, tijolo, vidro) — `enableInstancing` → true.
- `Assets/Scenes/SampleScene.unity` e a `TerrainData` de "Terreno Baldio" (`ecadc266ce5ed1b44971e5706a16a67d`) — repintura do mato salva via Editor.

**Observações para futuras sessões de IA:**
- Qualquer novo `DetailPrototype` criado com `usePrototypeMesh = true` no Terrain deste projeto (Unity 6 + URP) precisa de `useInstancing = true`, senão volta a renderizar branco.
- Qualquer material novo gerado por código para objetos `StaticEditorFlags.BatchingStatic` neste projeto deve nascer com `enableInstancing = true` (o `TerrenoBaldioAssets.MaterialDe()` e o `TerrenoBaldioMateriais.Converter()` ainda NÃO fazem isso automaticamente — só corrigi os materiais já existentes; se rodar `GerarTudo()`/`ConverterProjeto()` de novo no futuro e criar materiais novos, lembrar de ligar o instancing neles também).
- `unity command screenshot --view scene` (com `Selection.activeGameObject` + `SceneView.lastActiveSceneView.FrameSelected()`, ou setando `sv.pivot`/`sv.rotation`/`sv.size` na mão pra aproximar de um objeto pequeno) é a forma confiável de inspecionar visualmente o resultado sem depender do usuário tirar print — útil para validar próximas alterações visuais.

### 2026-09-18 — Novo asset: Escada de Mão (para subir na casa/telhado)

**Objetivo:** Criar o asset de escada de mão encostável — o agente usa para alcançar telhados e caixas d'água. O usuário pediu **só o modelo 3D**, sem script de interação.

**Arquivo novo:**
- `Assets/Editor/EscadaDeMaoBuilder.cs` — menu **Dengue > Criar Escada de Mao**. Mesma técnica dos builders anteriores (`VasoComPratoBuilder`, `CaixaDaguaBuilder`), mas com **extrusão de seção 2D** em vez de torno: cada peça é um prisma cuja seção é um retângulo com as 4 quinas cortadas (8 pontos), o que deixa as arestas chanfradas e "pegando luz" — o acabamento low poly do projeto, em vez de cubos lisos.

Gera em `Assets/Meus/Escada de Mao/`:
- `Malha Madeira.asset` — 2 montantes + 10 degraus numa malha só.
- `Malha Sapatas.asset` — os pés de borracha embaixo e as capas no topo dos montantes.
- `madeira da escada.mat` (marrom 0.520 / 0.352 / 0.192) e `sapata da escada.mat` (borracha escura 0.115 / 0.118 / 0.125), URP Lit com fallback HDRP/Standard.
- `Escada de Mao.prefab` — raiz com `BoxCollider` (0.46 × 3.045 × 0.11, centro y 1.52) e dois filhos, `Madeira` e `Sapatas`. **Sem `Interactable`** — é cenário, não foco de criadouro.

**Medidas:** 3,05 m de altura total × 0,44 m de largura. Montante 3 cm (X) × 9 cm (Z) — lado largo na profundidade, como escada de verdade. 10 degraus, o primeiro a 30 cm do chão, espaçados 28 cm; degrau de 5,5 cm de profundidade × 2,8 cm de espessura, entrando 1 cm dentro de cada montante. **Pivô no centro da base**: é só largar no chão e girar de −15° a −20° no X para encostar na parede (a base fica no lugar).

**Verificação feita (sem Unity, o shell do PC segue fora):** o gerador foi portado para Python — **448 triângulos, 0 faces invertidas** (normal de cada triângulo conferida contra o centro da peça) — e prévia renderizada em 4 vistas (3/4, frente, lado e encostada a −17°), que confirmou as proporções.

**Pendente:** copiar o `.cs` para `Assets/Editor/`, rodar `Dengue > Criar Escada de Mao` e arrastar o prefab para a cena.

**Observações para futuras sessões de IA:**
- O builder pode ser rodado de novo: ele reaproveita malhas e materiais existentes (`mesh.Clear()` + copiar os dados), mantendo os GUIDs e as referências do prefab. Nenhum `.meta` foi escrito à mão.
- Para mudar tamanho/número de degraus, editar as constantes no topo do arquivo (`AlturaMontante`, `LarguraExterna`, `Degraus`, `EspacoDegraus`…) e rodar o menu de novo.
- Convenção das matrizes: a seção é desenhada no plano XY local e extrudada em +Z local; `EixoY()` (Euler −90, 0, 0) usa a extrusão como altura e `EixoX()` (Euler 0, 90, 0) como largura. Ordem dos pontos da seção é anti-horária — se alguma face aparecer invertida, é a ordem da seção que está trocada.
- Se depois virar foco de criadouro (ex.: escada com calha/balde d'água), basta herdar `Interactable` e adicionar o componente na raiz do prefab, sem mexer na geração da malha.

### 2026-09-18 — Assets gerados reunidos em `Assets/Meus/Assets Gerados`

**Objetivo:** O usuário decidiu popular o terreno baldio à mão (o espalhamento automático não agradou) e pediu para juntar numa pasta nova, dentro de `Assets/Meus`, tudo que foi gerado pelos builders.

**Arquivo novo:**
- `Assets/Editor/MeusOrganizador.cs` — menu **Dengue > Organizar Assets Gerados**. Move, via `AssetDatabase.MoveAsset` (leva o `.meta` junto, então o **GUID não muda** e nada quebra na cena), para `Assets/Meus/Assets Gerados/`:
  - `Terreno Baldio/` (com `Malhas/`, `Materiais/`, `Prefabs/` — 35 prefabs)
  - `Vaso de Flor/`, `Caixa Dagua/`, `Escada de Mao/`
  - e os prefabs que estavam soltos na raiz de `Meus` (`Vaso de Flor.prefab`, `Caixa d'Agua.prefab`, `Escada de Mao.prefab`) vão para dentro da pasta correspondente; se já existir um com o mesmo nome, entra com `GenerateUniqueAssetPath` em vez de sobrescrever.
  - Pode rodar de novo: o que já está no lugar é ignorado. Expõe também `MeusOrganizador.GarantirPasta(caminho)`, que cria a pasta e as de cima dela.

**Ficaram onde estavam** (são do usuário, não foram tocados): `cerca.fbx`, `rubber tries.fbx` e `agua do pneu.mat` — este último ainda é referenciado por caminho pelo `VasoComPratoBuilder` e pelo `CaixaDaguaBuilder`.

**Arquivos alterados** (só as constantes de caminho, para os builders continuarem gravando no lugar certo depois da mudança):
- `TerrenoBaldioAssets.cs` — `Pasta` → `Assets/Meus/Assets Gerados/Terreno Baldio`; `GarantirPastas()` agora usa `MeusOrganizador.GarantirPasta`.
- `VasoComPratoBuilder.cs` — `Folder` → `Assets/Meus/Assets Gerados/Vaso de Flor`.
- `CaixaDaguaBuilder.cs` — `Folder` → `Assets/Meus/Assets Gerados/Caixa Dagua`.
- `EscadaDeMaoBuilder.cs` — `Pasta` → `Assets/Meus/Assets Gerados/Escada de Mao`; `GarantirPasta()` simplificada para usar o helper.

**Observações para futuras sessões de IA:**
- Nunca mover asset do Unity por fora (mv/copiar): tem que ser `AssetDatabase.MoveAsset`, senão o `.meta` fica para trás e todas as referências viram "Missing".
- Ao criar um builder novo, gravar direto em `Assets/Meus/Assets Gerados/<nome do asset>/` e usar `MeusOrganizador.GarantirPasta` para criar a árvore de pastas.

### 2026-09-18 — Grupo de garrafas vira foco (comportamento do pneu)

**Objetivo:** O usuário montou na cena um grupo de garrafas (uma raiz com mais 4 penduradas nela, todas cópias do `Garrafa.prefab` do terreno baldio) e pediu que o grupo fosse **um foco só**, com o mesmo comportamento do pneu.

**Arquivos novos:**
- `Assets/Scripts/GarrafaInteractable.cs` — cópia fiel do `PneuInteractable`, com uma diferença: a água é uma **lista** (`aguas`), porque o foco é um grupo. Ao atingir `clicksToDrainWater` desativa todas; depois segue o fluxo padrão do `Interactable` (pop, flash, partículas, encolhe e some), que é o "recipiente retirado" do GameDoc. Com a lista vazia, é o pneu sem água.
- `Assets/Editor/GarrafaFocoBuilder.cs` — menu **Dengue > Transformar Selecao em Foco (Garrafas)**. Converte o que estiver selecionado na Hierarchy: põe `BoxCollider` em cada garrafa sem collider (o Unity ajusta a caixa à malha sozinho), põe o `GarrafaInteractable` **na raiz** (um foco para o grupo inteiro), amarra como água qualquer filha cujo nome comece com "agua"/"água" e adiciona o `FocoHUDInfo` já preenchido. Aceita seleção múltipla, pode rodar de novo sem duplicar nada e passa tudo por `Undo`.

**Valores:** `requiredClicks = 4`, `clicksToDrainWater = 2` (o pneu usa 5 e 3; garrafa é menor, então menos cliques). Editáveis no Inspector.

**Por que collider nas filhas funciona:** o `Interactor` faz `hit.collider.GetComponentInParent<Interactable>()`, então o clique em qualquer garrafa do grupo encontra o foco na raiz. Foi por isso que o collider foi para cada garrafa em vez de uma caixa só na raiz — o grupo é espalhado, uma caixa englobando tudo pegaria ar.

**Sem água por enquanto:** o `vidro.mat` do terreno baldio é **opaco** (`_Surface: 0`, RenderType Opaque, cor 0.42/0.55/0.40), então lâmina d'água dentro da garrafa não apareceria. Por isso o builder não gera água; se o material virar transparente depois, basta criar as lâminas com nome começando em "agua" dentro do grupo e rodar o menu de novo — elas são amarradas sozinhas.

**Pendente:** copiar os dois `.cs`, selecionar o grupo na Hierarchy e rodar o menu.

**Observações para futuras sessões de IA:**
- A cena (`SampleScene.unity`) está com **128 MB**: não editar a cena por texto. Para mexer em objetos que o usuário montou à mão, o caminho é um menu de Editor que age sobre `Selection.gameObjects`.
- `Undo.AddComponent` + `Undo.RegisterFullObjectHierarchyUndo` deixam a conversão reversível com Ctrl+Z — vale seguir esse padrão em qualquer ferramenta que mexa na cena do usuário.
- O `Garrafa.prefab` (`Assets/Meus/Assets Gerados/Terreno Baldio/Prefabs/`) é um `GameObject` único com `MeshFilter`/`MeshRenderer`, sem filhos e **sem collider** — todo prefab do terreno baldio é assim.

### 2026-09-18 — Foco de garrafas refeito: some uma garrafa por clique

**Objetivo:** A primeira versão (grupo inteiro encolhendo de uma vez, igual ao pneu) não agradou. O usuário quer que **cada clique retire uma garrafa**, uma sumindo de cada vez.

**`Assets/Scripts/GarrafaInteractable.cs` (reescrito):**
- O foco agora guarda uma `List<Renderer> garrafas` (preenchida pelo builder; se ficar vazia, o `OnStart` busca sozinho todos os `MeshRenderer` filhos, ignorando os que começam com "agua"/"água").
- `cliquesIgualAoNumeroDeGarrafas` (padrão true) faz `requiredClicks = garrafas.Count` no `OnStart` — uma garrafa por clique, e a última fecha o foco no mesmo instante.
- Cada clique roda uma corrotina de sumiço na garrafa escolhida: **pulinho** (`sin(k·π) × 0.22`), **giro** (240°) e **encolher** (quadrático, começa devagar e acelera) em 0,32 s. No fim a pose é devolvida e o objeto é desativado (a garrafa da raiz só tem o `Renderer` desligado, porque desativar a raiz levaria o script junto).
- O collider da garrafa é desligado no começo da animação, para não dar para clicar duas vezes na mesma.
- `ordem` (`MaisPertoPrimeiro` por padrão, ou `Hierarquia` / `Aleatoria`): recolhe a garrafa mais perto da câmera, ou seja, a que o jogador está olhando.
- **A garrafa da raiz sai sempre por último**, porque animar a raiz arrasta todas as filhas penduradas nela; só quando não sobra mais nenhuma é que ela pode sumir.
- `OnComplete()` sobrescrito **sem** chamar a base: não faz sentido o encolher padrão do `Interactable` se as garrafas já sumiram uma a uma. O `OnCompleted` continua sendo disparado pela base, então HUD e rede social são avisadas normalmente.
- A lista `aguas` e o `clicksToDrainWater` da versão anterior foram removidos: água, se existir, deve ser **filha da garrafa** e some junto com ela.

**`Assets/Editor/GarrafaFocoBuilder.cs`:** passou a preencher a lista `garrafas`, a deixar `requiredClicks` igual ao número de garrafas e a **zerar o `punchScale`** — o punch da base sacode o grupo inteiro (inclusive as garrafas que ainda não foram recolhidas), e agora quem anima é cada garrafa. Pode rodar de novo no mesmo grupo: a lista é refeita.

**Observações para futuras sessões de IA:**
- Cuidado ao animar a raiz de um foco que tem filhas visíveis: escala/posição da raiz valem para o grupo todo. Por isso a regra "raiz por último" aqui.
- O mesmo padrão (lista de peças + uma some por clique) serve para qualquer foco que seja um monte de coisas (pilha de pneus, latas na sarjeta, entulho com recipientes).

### 2026-09-19 — Fundo no aviso de foco eliminado

**Objetivo:** Com o `Consertar HUD` rodado, o aviso e a dica passaram a aparecer, mas o usuário achou **ruim de ler** — texto solto em cima do cenário, sem contraste.

**O que mudou:** o `ToastPanel` ganhou fundo e passou a ter altura de conteúdo.
- `Image` com o `UISprite` fatiado em `pixelsPerUnitMultiplier = 0.35` (cantos bem arredondados), cor `0.05/0.08/0.10` com alpha **0.82** — a mesma família dos outros painéis da HUD, só que mais opaca, porque aqui o texto precisa competir com o mato e o céu.
- **Faixa verde** na lateral esquerda (6 px, accent `0.25/1/0.8`), igual à do contador, com `LayoutElement.ignoreLayout = true` para não entrar no layout como conteúdo.
- `VerticalLayoutGroup` (padding 26/22/14/14) + `ContentSizeFitter` vertical: o painel **cresce com o texto**. Título curto = painel baixo; com a dica de duas linhas ele sobe. Como o pivô do ToastPanel é (0.5, 0), ele cresce **para cima**, sem tapar a mira.
- O `ContentSizeFitter` mexe só no `sizeDelta`, então não briga com a animação de subida do aviso, que escreve `anchoredPosition`.

**Arquivos alterados:**
- `Assets/Editor/DengueHUDReparo.cs` — novo método `GarantirFundo(toast, toastText)`, chamado pelo `Dengue > Consertar HUD`. É idempotente: só cria o que não existe.
- `Assets/Editor/DengueHUDBuilder.cs` — o `Dengue > Criar HUD` chama **o mesmo** `DengueHUDReparo.GarantirFundo`, em vez de duplicar o código. O aviso nasce igual pelos dois caminhos.

**Para aplicar:** rodar `Dengue > Consertar HUD (criar o que falta)` de novo — ele acrescenta o fundo ao ToastPanel que já existe, sem mexer no resto.

**Observações para futuras sessões de IA:**
- Texto de HUD sobre cenário 3D precisa de fundo opaco o bastante (alpha ≥ 0.8) ou de outline: o jogo tem céu claro e terreno claro, e accent verde sobre isso some.
- Painel que cresce com o texto: `VerticalLayoutGroup` + `ContentSizeFitter(PreferredSize)` no painel e `textWrappingMode = Normal` no TMP. Com pivô embaixo, cresce para cima.

### 2026-09-19 — Lista de focos restantes (tecla Tab)

**Objetivo:** O usuário pediu uma lista visual dos focos que ainda faltam eliminar, mostrada ao apertar Tab (toggle), puramente informativa — sem afetar mira, clique ou movimentação do jogador.

**Arquivos novos:**
- `Assets/Scripts/FocoListaHUD.cs` — `MonoBehaviour` independente (não depende de `DengueHUD` nem de `Interactor`). Escaneia `Interactable` na cena no `Start()` (mesmo padrão do `DengueHUD`/`RedeSocial`: `FindObjectsByType<Interactable>(FindObjectsInactive.Include, ...)`), assina `OnCompleted` de cada foco ainda não concluído e mantém uma `List<Interactable> focosRestantes`, tirando o foco da lista (e atualizando o texto) assim que ele é eliminado. Tecla `Tab` (`Keyboard.current[Key.Tab].wasPressedThisFrame`) **alterna** `aberto`; o `CanvasGroup.alpha` acompanha esse estado com `Mathf.MoveTowards` (fade, `velocidadeFade = 10`). `CanvasGroup.blocksRaycasts`/`interactable` ficam sempre `false` e todo elemento gráfico com `raycastTarget = false` — não bloqueia nada de gameplay (ao contrário do celular, não mexe em `Interactor.Bloqueado` nem desliga a `DengueHUD`). Nome de cada foco: `FocoHUDInfo.nome` > tipo (`PneuInteractable`→"o pneu", `VasoInteractable`→"o pratinho do vaso") > `gameObject.name` — mesma lógica do `DengueHUD.NomeDoFoco`, duplicada de propósito (função pequena, evita editar `DengueHUD.cs`). Lista vazia (todos eliminados) mostra "Nenhum foco restante!".
- `Assets/Editor/FocoListaHUDBuilder.cs` — menu **Dengue > Criar Lista de Focos (Tab)**. Cria, dentro do Canvas `DengueHUD` já existente, o painel `FocoListaPanel` (canto superior direito, `anchorMin=anchorMax=pivot=(1,1)`, `anchoredPosition=(-32,-32)`) com fundo (`UISprite` fatiado, mesma cor `0.05/0.08/0.10/0.72` dos outros painéis), faixa lateral verde decorativa, `VerticalLayoutGroup`+`ContentSizeFitter` (o painel cresce/encolhe com a lista) e dois textos TMP (`FocoListaTitulo` = "FOCOS RESTANTES", `FocoListaTexto` = lista multilinha, um "• nome" por linha). Liga as referências do `FocoListaHUD` via `SerializedObject`, só as que estiverem vazias. Idempotente (roda de novo sem duplicar nada) e isolado — não toca em `DengueHUDBuilder.cs`/`DengueHUDReparo.cs`.

**Verificado no Editor conectado (via Unity CLI/Pipeline):** recompile sem erros nos dois arquivos; menu rodado (e re-rodado sem duplicar), criou a hierarquia inteira e ligou as duas referências (`painelGrupo`, `textoLista`) automaticamente; em Play Mode a lista escaneou os 14 focos da `SampleScene` e montou os nomes corretos (inclusive os fallbacks por tipo e por `FocoHUDInfo`); o painel nasce fechado (`CanvasGroup.alpha = 0`); `FocoListaHUD.Alternar()` inverte `Aberto` corretamente. A animação de fade e a remoção ao vivo de um foco ao ser eliminado (via `OnCompleted`) não puderam ser observadas visualmente nesta sessão porque a janela do Editor não estava em foco (o `Time.frameCount` não avançava) — o código segue o mesmo padrão já testado de `CelularController`/`DengueHUD`, mas vale conferir visualmente numa sessão normal de Play.

**Observações para futuras sessões de IA:**
- O painel entra no mesmo Canvas `DengueHUD` (não um Canvas próprio), para reaproveitar `CanvasScaler`/`sortingOrder` — segue o mesmo padrão de `CounterPanel`/`ToastPanel`.
- Ao abrir o celular (tecla T), `CelularController.Abrir()` desativa o **componente Canvas** inteiro da `DengueHUD` — a lista de focos some junto com o resto da HUD nesse momento. É efeito esperado, herdado da estrutura existente, não um bug da lista.
- Não há gerenciador central de focos no projeto: qualquer novo sistema que precise da lista de `Interactable` deve fazer sua própria varredura com `FindObjectsByType<Interactable>(FindObjectsInactive.Include, ...)` e assinar `OnCompleted`, como já fazem `DengueHUD`, `RedeSocial` e agora `FocoListaHUD`.

### 2026-09-19 — Lista de focos agrupada por tipo + dica permanente do Tab

**Objetivo:** Dois ajustes pedidos na lista de focos restantes (`FocoListaHUD`, ver entrada anterior): (1) agrupar por tipo com contagem em vez de repetir o nome de cada item ("Faltam 4 pneus" em vez de "o pneu" x4); (2) avisar o jogador, de forma permanente e discreta, que a tecla Tab existe.

**`Assets/Scripts/FocoListaHUD.cs` (reescrito):**
- `AtualizarTexto()` agora agrupa `focosRestantes` por **categoria** (tipo do foco) num `Dictionary<string, (singular, plural, quantidade)>`, mostrando uma linha por categoria com concordância: `formatoLinhaSingular` ("• Falta {n} {categoria}") quando sobra 1, `formatoLinhaPlural` ("• Faltam {n} {categoria}") quando sobra mais de 1. Os campos antigos `formatoLinha`/`NomeDoFoco` foram removidos.
- Novo método `Categoria(Interactable, FocoHUDInfo)` com a tabela singular/plural por tipo: `PneuInteractable`→pneu/pneus, `VasoInteractable`→vaso/vasos, `CaixaDaguaInteractable`→caixa d'água/caixas d'água, `PiscinaInteractable`→piscina de plástico/piscinas de plástico, `GarrafaInteractable`→grupo de garrafas/grupos de garrafas (categoriza pelo TIPO, não pelo `FocoHUDInfo.nome` daquele foco — o `nome` da garrafa descreve quantas garrafas tem *dentro* do grupo, não quantos grupos faltam). Fallback: com `FocoHUDInfo.nome` preenchido mas tipo desconhecido, remove o artigo inicial e pluraliza (+s); sem nada, usa `gameObject.name` cru (mesma imperfeição que já existia antes para objetos sem `FocoHUDInfo`, ex. os grupos de garrafa nomeados "Lixo"/"Latas" na cena — que agora, por sorte, já caem certinho em "grupos de garrafas" por serem `GarrafaInteractable`, e não mais aparecem com o nome cru do objeto).
- Helpers novos: `RemoverArtigo` (tira "a "/"o "/"as "/"os ") e `Pluralizar` (regra geral +s; não trata plurais irregulares, que hoje não ocorrem nos nomes usados).

**`Assets/Editor/FocoListaHUDBuilder.cs`:** cria (idempotente) um novo texto `FocoListaDica`, filho direto do Canvas `DengueHUD` — **fora** do `CanvasGroup` do `FocoListaPanel`, de propósito, para ficar sempre visível mesmo com a lista fechada. Texto fixo "TAB — focos restantes", canto inferior esquerdo (`anchorMin=anchorMax=pivot=(0,0)`, `anchoredPosition=(32,32)`), fonte pequena (16) e alpha baixo (0.55) — discreto, sem `CanvasGroup` próprio (não pisca nem anima, é permanente). Não precisa de referência serializada em `FocoListaHUD` (é só decoração estática).

**Verificado no Editor conectado (Unity CLI):** recompile sem erros; menu rodado de novo criou só o `FocoListaDica` que faltava (resto idempotente, nada duplicado); em Play Mode (com a janela do Editor em foco desta vez, para o `Time`/`Update` avançarem) a lista mostrou exatamente `Faltam 4 pneus / Faltam 3 vasos / Faltam 4 grupos de garrafas / Faltam 2 caixas d'água / Falta 1 piscina de plástico` (soma 14, batendo com o total de focos da cena); eliminando pneus via `Interact()` a contagem caiu ao vivo (4→3→1) e a concordância mudou sozinha para singular ("Falta 1 pneu"); eliminando o último pneu a linha inteira da categoria sumiu da lista. `FocoListaDica` confirmado sempre ativo e sem `CanvasGroup`. Sem erros novos no Console. Mudanças de Play Mode descartadas ao sair (comportamento padrão do Unity); cena salva só com a hierarquia nova (`FocoListaDica`) em modo Edição.

**Observações para futuras sessões de IA:**
- Categorizar por **tipo C#** (`foco is XInteractable`) é mais confiável do que por `FocoHUDInfo.nome`, porque o `nome` é texto livre por instância (pode variar) e, no caso de `GarrafaInteractable`, descreve uma coisa diferente do que a lista precisa contar (garrafas dentro do grupo vs. grupos restantes).
- Se um novo tipo de foco for criado, adicionar sua entrada (singular/plural) em `FocoListaHUD.Categoria()` — senão ele cai no fallback genérico (usa o nome como está, plural só com "+s", pode ficar estranho para palavras irregulares).

### 2026-09-19 — Lista de focos em frase única; aviso de foco eliminado no topo e mais rápido

**Objetivo:** Dois ajustes pedidos pelo usuário sobre features já em uso: (1) a lista de focos (ver as duas entradas anteriores) devia virar uma frase compacta tipo "Focos faltantes: Pneu (4), vaso de flor (2)." em vez de uma linha por categoria com "Falta/Faltam"; (2) o aviso "Foco eliminado!" (`ToastPanel`, em `DengueHUD.cs`) devia subir para o topo da tela, com fundo retangular translúcido (já existia, ver entrada de HUD anterior) e sumir mais rápido.

**`Assets/Scripts/FocoListaHUD.cs` (simplificado):** como o formato novo mostra a contagem entre parênteses, não precisa mais de concordância singular/plural — `Categoria()` agora devolve só **um rótulo** por tipo (`PneuInteractable`→"Pneu", `VasoInteractable`→"Vaso de flor", `CaixaDaguaInteractable`→"Caixa d'água", `PiscinaInteractable`→"Piscina de plástico", `GarrafaInteractable`→"Garrafas"; fallback com `FocoHUDInfo.nome` → artigo removido + primeira letra maiúscula; sem nada → `gameObject.name` cru). Os campos `formatoLinhaSingular`/`formatoLinhaPlural` e o helper `Pluralizar` foram removidos; entraram `prefixo` ("Focos faltantes: "), `formatoItem` ("{categoria} ({n})") e `separador` (", "). `AtualizarTexto()` monta a frase inteira com `string.Join` e um "." no final.

**`Assets/Editor/FocoListaHUDBuilder.cs`:** o `FocoListaTitulo` separado ("FOCOS RESTANTES") não faz mais sentido — o rótulo agora está embutido na frase. O builder **desativa** esse objeto se o encontrar (idempotente: só desativa, não apaga, e não recria em builds novos).

**`Assets/Scripts/DengueHUD.cs`:** `duracaoAviso` (aviso sem dica) 1.8s → **1s**; `duracaoAvisoComDica` 4.5s → **3s**; a janela de saída (fade + "subida") em `UpdateToast()` encolheu de 0.4s → **0.2s**, deixando o desaparecimento nitidamente mais rápido em qualquer um dos dois casos.

**`Assets/Editor/DengueHUDBuilder.cs` e `DengueHUDReparo.cs`:** `ToastPanel` passou de ancorado no centro (`anchor/pivot (0.5,0.5)/(0.5,0)`, 60px acima do centro) para ancorado no **topo da tela** (`anchor/pivot (0.5,1)/(0.5,1)`, `anchoredPosition (0,-60)`); com pivô no topo agora, o painel cresce **para baixo** quando a dica entra (antes crescia para cima, com pivô embaixo). O fundo retangular translúcido do aviso já existia (`GarantirFundo`, ver entrada "Fundo no aviso de foco eliminado") — não precisou de nada novo, só acompanhou o reposicionamento por já estar dentro do `ToastPanel`.

**Aplicado direto na cena já aberta (via Unity CLI/Pipeline, sem editar o `.unity` por texto):** como os builders são idempotentes e não *movem* o que já existe, o `ToastPanel` e os campos `duracaoAviso`/`duracaoAvisoComDica` do `DengueHUD` já colocado na cena foram atualizados com `eval`/`set_serialized_field` para os mesmos valores do código-fonte, e a cena foi salva. Repare: `duracaoAviso` estava salvo como `3` (diferente tanto do valor antigo do código, 1.8, quanto do novo, 1) — alguém tinha ajustado esse campo à mão no Inspector em algum momento; isso foi sobrescrito para o novo padrão (1s).

**Verificado no Editor conectado:** recompile sem erros; rodando o menu de novo, só desativou o `FocoListaTitulo` (nada duplicado); em Play Mode a lista mostrou `Focos faltantes: Garrafas (4), Piscina de plástico (1), Pneu (4), Caixa d'água (2), Vaso de flor (3).`; eliminando um vaso, `ToastText` mudou para o texto certo (aviso + dica) e o `ToastPanel` confirmado com `anchorMax=(0.5,1)`/`pivot=(0.5,1)` (topo). A suavidade exata da animação de entrada/saída não deu para observar visualmente nesta sessão (mesmo problema de sempre: o `Time`/`Update` do Editor não avança em tempo real neste ambiente) — vale conferir ao vivo numa sessão normal.

**Observações para futuras sessões de IA:**
- Um builder idempotente (`Achar<T>` + "cria só o que falta") **nunca reposiciona nem retexta** o que já existe na cena — só cria o que falta. Para mudar posição/valores de algo que o usuário já tem na cena (não só em builds novos), é preciso também aplicar a mudança na cena aberta via CLI (`eval`/`set_serialized_field`) além de atualizar o código-fonte, senão o código e a cena ficam dessincronizados.
- Um campo serializado pode estar com um valor diferente do default do código porque alguém mudou pelo Inspector antes (foi o caso de `duracaoAviso`, salvo como `3`) — ao "restaurar" um valor padrão, sempre checar o valor atual na cena em vez de assumir que é o default antigo do código.

### 2026-09-19 — Lista de focos volta a ser uma linha por item; "Foco eliminado!" padronizado

**Objetivo:** Dois ajustes pedidos pelo usuário: (1) a lista de focos (a frase única da entrada anterior) devia voltar a ter **um item por linha**; (2) o aviso colorido mostrado ao eliminar qualquer foco devia sempre dizer **"Foco eliminado!"**, igual ao do pneu — os textos customizados dos outros tipos ("Caixa tampada!", "Piscina virada!", "Garrafas recolhidas!", "latas recolhidas!", "Lixo recolhido!") deviam ser trocados por esse texto padrão.

**`Assets/Scripts/FocoListaHUD.cs`:** `separador` (usado entre os itens da lista) mudou o padrão de `", "` para `"\n"`, e `AtualizarTexto()` agora sempre coloca o `prefixo` ("Focos faltantes:") em sua própria linha, seguido de um item por linha:
```
Focos faltantes:
Pneu (4)
Garrafas (4)
Caixa d'água (2)
Vaso de flor (3)
Piscina de plástico (1)
```
(O `.` final do formato em frase única foi removido — não fazia sentido sozinho numa lista de linhas.)

**Padronização do texto de eliminação:** `FocoHUDInfo.textoAoEliminar` só deveria ser preenchido quando o foco precisa de um texto realmente diferente — não é o caso de nenhum foco hoje, porque a parte educativa já mora inteira no campo `dica` (mostrado como segunda linha do aviso). Removidas as linhas `info.textoAoEliminar = "..."` de `Assets/Editor/CaixaDaguaBuilder.cs`, `Assets/Editor/PiscinaBuilder.cs` e `Assets/Editor/GarrafaFocoBuilder.cs` — agora esses builders só preenchem `nome`, `textoAoMirar` e `dica`, deixando o aviso cair no padrão `"<b>Foco eliminado!</b>"` do `DengueHUD` (o mesmo caminho que o pneu já usava, por nunca ter tido `FocoHUDInfo`).

**Aplicado direto na cena já aberta (via Unity CLI/Pipeline):** os 7 `FocoHUDInfo` já existentes na cena (`Caixa d'Agua`, `Caixa d'Agua (1)`, `Piscina de Plastico`, e 4 dentro de `Cenario Terreno Baldio/Lixo`: `Saco de Lixo 001`, `Garrafa `, `Garrafa  (1)`, `Lata 001`) tinham `textoAoEliminar` customizado herdado de quando os builders rodaram (mais alguns editados à mão depois, ex. "latas recolhidas!"/"Lixo recolhido!" com grafia diferente do padrão do builder). Todos foram atualizados via `set_serialized_field` para `"<b>Foco eliminado!</b>"`, igual ao `DengueHUD`. Os campos `prefixo`/`separador` do `FocoListaHUD` já colocado na cena também foram atualizados pelo mesmo motivo (builders idempotentes não tocam em campos de instância já existente).

**Verificado no Editor conectado:** recompile sem erros; em Play Mode a lista mostrou exatamente o formato de uma linha por item (acima); eliminando a piscina, o `ToastText` mostrou `"<b>Foco eliminado!</b>"` na primeira linha e a dica específica da piscina ("Não basta esvaziar: guarde de boca para baixo...") na segunda — confirmando que só o título mudou, a dica educativa de cada tipo continua intacta. Sem erros novos no Console.

**Observações para futuras sessões de IA:**
- `set_serialized_field` **não aceita string vazia** como valor (`--value ""` falha ou, pior, alguns clientes/shells fazem o parser interpretar como um booleano `true` em vez de string vazia — cuidado, isso realmente aconteceu aqui e teve que ser corrigido). Para "limpar" um campo de texto num componente já em cena, usar o texto padrão desejado explicitamente, não string vazia.
- Um `GameObject` cujo nome termina com espaço (ex. `"Garrafa "`) ou tem espaços duplos antes do `(1)` (ex. `"Garrafa  (1)"`) pode falhar ao ser endereçado por `hierarchyPath` em alguns comandos do Pipeline (o espaço final se perde no caminho). Nesse caso, usar o `globalId` (string estável, veio de `find_gameobjects`) como `--target` em vez do caminho — funciona mesmo com esses nomes estranhos. Não usar o `instanceId` numérico diretamente: é grande demais e pode perder precisão ao virar número em JSON, apontando para o objeto errado.
- Todo foco novo deveria só preencher `nome`/`textoAoMirar`/`dica` em `FocoHUDInfo` — **não** `textoAoEliminar` — para o aviso de eliminação continuar padronizado como "Foco eliminado!" em qualquer tipo de foco.

### 2026-09-19 — Lista de focos já começa aberta

**Objetivo:** O usuário pediu que a lista de focos restantes (`FocoListaHUD`) já apareça aberta assim que o jogo começa, em vez de precisar apertar Tab uma primeira vez.

**`Assets/Scripts/FocoListaHUD.cs`:** novo campo `[SerializeField] private bool comecarAberto = true;`. Em `Start()`, `aberto = comecarAberto` e o `CanvasGroup.alpha` já nasce em `1f` (em vez de `0f`) quando `aberto` é true — sem esperar o fade. Tab continua alternando normalmente a partir daí (fecha na primeira pressionada, já que começa aberta). Como é um campo novo (nunca foi serializado antes), o componente já existente na cena pegou o valor padrão `true` sozinho ao recompilar — não precisou de nenhum ajuste manual via CLI desta vez.

**Verificado no Editor conectado:** recompile sem erros; `comecarAberto` confirmado `true` no componente já em cena; em Play Mode, `FocoListaHUD.Aberto` e o `CanvasGroup.alpha` já nasceram `true`/`1` sem precisar apertar Tab. Sem erros novos no Console.

**Observações para futuras sessões de IA:**
- Diferente de mudar o *valor* de um campo já existente (que exige `set_serialized_field` na cena, como nas entradas anteriores), um campo **novo** adicionado a um `MonoBehaviour` já usado na cena recebe o valor padrão do código sozinho no próximo recompile — só é preciso mexer via CLI quando o campo já existia antes com outro valor serializado.

### 2026-09-19 — História: abertura jogável no hospital + ocupação hospitalar + vitória/derrota

**Objetivo:** O jogo tinha premissa e mecânicas boas mas "parecia sem história" — o `contexto do jogo.md` já descrevia uma abertura no hospital, uma barra de ocupação hospitalar e uma tela de vitória/derrota com estatísticas (seções "História"/"Fase 1" no topo deste arquivo), mas nada disso estava implementado (só a rede social existia de verdade como narrativa). Implementado agora: (1) o jogo começa dentro de um quarto de hospital jogável, o jogador anda até a porta e só aí é liberado pro bairro; (2) uma barra de ocupação hospitalar real (sobe com o tempo, desce a cada foco eliminado); (3) telas de vitória e derrota com estatísticas (tempo, focos, % ocupação final) e opção de reiniciar.

**Descoberta importante antes de implementar:** o prédio "Hospital" já presente na cena (`Assets/Construçoes/Simple Building Generic Free/Prefabs/Hospital.prefab`) é só uma casca externa — uma malha única sem interior, igual aos outros prédios do pacote (Escola, Mercado, etc.). Não dava pra fazer uma cutscene "dentro" dele. Por isso a abertura jogável usa uma **salinha nova, escondida bem abaixo do mapa** (Y = -30), sem nenhuma relação visual com o prédio que já existia na cena (que continua só decorativo, do jeito que estava).

**Arquivos novos:**
- `Assets/Scripts/FluxoDeJogo.cs` — estado da partida (não desenha nada). `Ocupacao` (0 a 1, começa em `ocupacaoInicial = 0.7`), sobe sozinha `aumentoPorSegundo` (padrão 1%/s) **só depois** que `IniciarPartida()` é chamado (pelo `SaidaDoHospital`, ao sair do quarto — antes disso o relógio fica parado). Cada foco eliminado desconta `reducaoPorFoco` (padrão 4%) via a mesma varredura de `Interactable`/`OnCompleted` que `DengueHUD`/`RedeSocial`/`FocoListaHUD` já fazem (sem gerenciador central, cada sistema com a própria varredura — convenção já estabelecida no projeto). Dispara `OnVitoria` (todos os focos eliminados antes de bater 100%) ou `OnDerrota` (ocupação bate 100% antes de todos os focos sumirem); só um dos dois pode disparar (`PartidaEncerrada` trava o outro).
- `Assets/Scripts/FluxoDeJogoHUD.cs` — apresentação: texto de briefing (frase da equipe médica já descrita neste documento) com fade-in e fade-out sozinho (`duracaoBriefing = 6s`); barra de ocupação (cor interpola verde→vermelho conforme sobe, preenchimento suavizado com `Lerp`, mesmo padrão de `anchorMax.x` que `DengueHUD` usa na barra de progresso do foco); telas de vitória/derrota com estatísticas montadas a partir do `FluxoDeJogo` (tempo em `mm:ss`, `eliminados/total`, `% ocupação final`); ao disparar qualquer uma das duas, bloqueia interação (`Interactor.Bloqueado = true`, mesmo padrão que o `CelularController` já usa). Tecla **R** (com `PartidaEncerrada`) recarrega a cena (`SceneManager.LoadScene`) pra jogar de novo.
- `Assets/Scripts/SaidaDoHospital.cs` — fica na "porta" do quarto (trigger). Ao o `CharacterController` do player cruzar, teleporta ele de volta pra posição **original** que ele tinha no bairro antes desta feature (gravada pelo builder), com a dança seguro de `CharacterController` (`controller.enabled = false` → move → `true`) pra não brigar com a física, e chama `FluxoDeJogo.IniciarPartida()`. **Só a posição é restaurada, não a rotação** — o `MouseLook.cs` controla o giro por um `yaw` interno próprio (não lê o `Transform`), então qualquer rotação setada aqui seria sobrescrita no próximo `Update()` dele mesmo jeito; o jogador simplesmente continua olhando pra onde já estava olhando ao cruzar a porta, como atravessar um vão de verdade.
- `Assets/Editor/QuartoHospitalBuilder.cs` — menu **Dengue > Criar Sala do Hospital**. Constrói a sala (chão + 3 paredes com `GameObject.CreatePrimitive(PrimitiveType.Cube)`, que já vêm com collider sólido de graça; o lado Sul fica **aberto de propósito** — é a porta, mais simples que abrir vão numa parede) num `GameObject` raiz em `(0, -30, 0)`, bem longe de qualquer outra geometria da cena. Acha o player por `FindFirstObjectByType<CharacterController>()` (não por nome — mais resistente a renomear), **grava a posição atual dele ANTES de mexer em qualquer coisa** (essa é a posição que vai pro `SaidaDoHospital.posicaoNoBairro`), aí move o player pra dentro da sala nova. Cria também o `FluxoDeJogo` (objeto solto na raiz da cena). **Não é idempotente de propósito**: se `"SalaDoHospital"` já existir, recusa rodar de novo — rodar duas vezes gravaria a posição ATUAL do player (já deslocado) como se fosse a "original", corrompendo o teleporte de saída.
- `Assets/Editor/FluxoDeJogoHUDBuilder.cs` — menu **Dengue > Criar HUD de Fim de Jogo** (esse sim idempotente, como os outros builders de HUD). Cria `OcupacaoPanel` **dentro do Canvas `DengueHUD` já existente** (canto inferior central, `anchor/pivot (0.5,0)`, mesmas cores/padrão de barra de progresso do `DengueHUDBuilder`) e um Canvas novo `FluxoDeJogoCanvas` (`sortingOrder = 20`, acima de tudo — `DengueHUD` é 10, `CelularHUD` é 11) com `BriefingPanel`/`VitoriaPanel`/`DerrotaPanel`. Exige que `Dengue > Criar Sala do Hospital` já tenha rodado antes (precisa achar o `FluxoDeJogo` pra ligar a referência).

**Verificado no Editor conectado (Unity CLI/Pipeline):** recompile sem erros (só os warnings de API obsoleta já existentes no projeto todo, `FindFirstObjectByType`/`FindObjectsSortMode`, mesmo padrão usado em todo o resto do código); os dois menus rodados em sequência, hierarquia inteira criada e ligada; em Play Mode:
- Player nasceu dentro da sala em `(0, -28.52, -2.5)` (o cálculo bateu: centro da sala `(0,-30,0)` + o offset de spawn); `FluxoDeJogo.Ocupacao = 0.7`, `PartidaIniciada = false`.
- Simulando o `OnTriggerEnter` da porta (chamada direta via reflection, já que a física não avança sozinha neste ambiente de teste — ver observação de sessões anteriores sobre `Time`/`Update` não rodarem em tempo real aqui): o player foi teleportado exatamente para `(-6.23, 1.69, -20.04)` — a posição original gravada — e `PartidaIniciada` virou `true`.
- Eliminando os 14 focos da cena: `OnVitoria` disparou, `VitoriaTexto` mostrou `"Focos eliminados: 14/14"` e `"Ocupação final: 14%"` — bate a conta (70% inicial − 14 × 4% = 14%) —, e `Interactor.Bloqueado` virou `true`.
- Numa segunda rodada de Play, forçando `Ocupacao` a 100% via reflection e chamando `Update()` uma vez: `OnDerrota` disparou, `DerrotaTexto` mostrou `"HOSPITAL EM COLAPSO"`/`"Ocupação final: 100%"`, `Interactor.Bloqueado = true`.
- Barra de ocupação e briefing confirmados fazendo fade suave (não instantâneo) nos poucos frames que rodaram.
- Sem erros novos no Console (só os erros/warnings pré-existentes já conhecidos: `Default-Particle.psd` faltando, warnings de API obsoleta).

**Achado à parte (não mexido, fora do escopo deste pedido):** o player (`GameObject "Cube"`) tem **dois componentes `Mover` duplicados** (um com `jumpForce = 3`, outro com `jumpForce = 7`) — parece resíduo de configuração de uma sessão anterior. Não estava relacionado a esta feature, não foi tocado; vale o usuário limpar isso manualmente quando quiser (remover um dos dois `Mover` no Inspector do "Cube").

**Observações para futuras sessões de IA:**
- Balanceamento (`ocupacaoInicial`, `aumentoPorSegundo`, `reducaoPorFoco`, todos em `FluxoDeJogo.cs`) são só um ponto de partida razoável — não deu pra jogar a partida inteira em tempo real neste ambiente pra sentir o ritmo. Vale o usuário testar de verdade numa sessão normal do Editor e ajustar esses três números pelo Inspector conforme o "sentimento" de urgência que quiser.
- A câmera **não é filha do player** — o `MouseLook.cs` sincroniza a posição dela (`transform.position + up*eyeHeight`) a cada `LateUpdate()`. Por isso teleportar o player (mudar só o `Transform`) já "leva" a câmera automaticamente no próximo frame, sem precisar de nenhum código extra pra mover a câmera também.
- Pra reconstruir a sala do zero (ex.: mudar o tamanho/posição/formato dela), é preciso apagar `"SalaDoHospital"` E `"FluxoDeJogo"` na Hierarchy antes de rodar `Dengue > Criar Sala do Hospital` de novo — o builder recusa rodar se `"SalaDoHospital"` já existir, e recriar sem apagar o `FluxoDeJogo` antigo deixaria um componente órfão duplicado.
- Testar disparo de eventos (`OnVitoria`/`OnDerrota`) sem esperar o tempo real passar: dá pra usar reflection no `eval` do Unity CLI pra chamar `Update()` de um `MonoBehaviour` diretamente, ou pra setar um campo `private set` (ex. `Ocupacao`) via `GetField("<Prop>k__BackingField", NonPublic | Instance)` — jeito rápido de simular o fim de jogo sem precisar zerar o relógio manualmente.

### 2026-09-19 — Sala do hospital mobiliada (pack Office) + HUD com acabamento tipo celular

**Objetivo:** O usuário achou a sala/HUD da entrada anterior "feia" e pediu pra deixar a sala parecendo hospital de verdade e a HUD mais bonita, além de mostrar onde ajustar as variáveis principais do jogo. Como não existe nenhum asset médico (cama/maca/biombo) no projeto, a solução usou o pack **`Office`** (o único com mobília de interior) por analogia — balcão de recepção, cadeira, armário e dispensador de álcool em gel (bem temático).

**`Assets/Editor/QuartoHospitalBuilder.cs` (reescrito):** a sala deixou de ser cubos esticados e passou a ser montada com peças de verdade do pack Office (dimensões reais medidas via `eval` nos bounds dos renderers antes de escrever o código: parede 2×3m, piso em ladrilho 2×2m, moldura de porta 2×3m):
- **Piso** (`Assets/Office/Prefabs/Building/floor_tile.prefab`, grade 3×3 = sala de 6×6m) e **4 paredes** (`wall_standard.prefab`, 3 módulos por lado) com **Teto** novo (cubo simples — texture/mesh de `floor_tile` não é confiável vista de baixo).
- **Porta**: o módulo do meio da parede Sul usa `wall_standard_door_frame.prefab` (moldura com vão livre) em vez de `wall_standard` — sem folha de porta separada, o vão já é o caminho de saída. `PortaTrigger` (o mesmo `SaidaDoHospital.cs` de antes) fica logo depois do vão.
- **Mobília** (`MobiliarSala`): balcão de recepção (`counter_a_1`) encostado na parede Norte, dispensador de álcool em gel (`alcohol_dispenser_a`) em cima dele, cadeira (`standard_office_chair_1`) de frente pro balcão, armário (`file_cabinet_medium`) no canto, relógio de parede (`clock`) — todos posicionados calculando o offset de cada prefab a partir do próprio pivô (a maioria tem pivô na base; o dispensador tem pivô ~0,20 acima da base, por isso soma esse valor pra ficar exatamente em cima do balcão).
- **Cor "clínica"**: os prefabs do Office vêm com um material de atlas (cor varia por UV, não por textura tileável) — em vez de usar `renderer.material.color` (isso vaza material sem dono em modo Editor, o Unity até avisa/loga erro), cada material tingido é derivado com `new Material(original)` e **salvo como asset de verdade** via `AssetDatabase.CreateAsset` em `Assets/Meus/Assets Gerados/Sala do Hospital/Materiais/` (usa `MeusOrganizador.GarantirPasta`, o mesmo helper que os outros builders já usam), com cache por material original pra não duplicar. Também zera a `_Smoothness` desses materiais (URP/Lit vem com 0.5 por padrão — gerava um brilho especular forte).
- **Reconstruível**: a `RootName` ("SalaDoHospital") continua não-idempotente no sentido de "não recria automaticamente", mas agora, se já existir, a ferramenta **apaga e reconstrói do zero**, reaproveitando a `posicaoNoBairro` já gravada no `SaidaDoHospital` antigo (lida antes de destruir) em vez de reler a posição atual do player (que a essa altura já está dentro da sala, não no bairro) — dá pra rodar quantas vezes quiser pra iterar no layout sem corromper o teleporte de saída.

**`Assets/Editor/FluxoDeJogoHUDBuilder.cs` (reescrito):** os 4 painéis (`OcupacaoPanel`, `BriefingPanel`, `VitoriaPanel`, `DerrotaPanel`) ganharam o mesmo nível de acabamento do celular (`CelularBuilder.cs`), levantado antes numa pesquisa dedicada:
- **Paleta nomeada** (`CorFundo`, `CorBorda`, `CorLinha`, `CorTexto`, `CorTextoFraco`) reaproveitada em tudo, em vez das 2-3 cores soltas de antes.
- **Painel com borda**: helper novo `PainelBordado` — um retângulo "Borda" (cor de acento translúcida) com um "…Dentro" recuado 3px por cima (fundo escuro opaco), simulando um contorno de destaque em vez de um retângulo plano. `PainelCaixaCentral` é a mesma ideia pras telas de vitória/derrota (caixa de 760×420 centralizada sobre o véu escurecido de tela inteira).
- **Linha divisória** (`LinhaDivisoria`, 1px, `CorLinha`) separando título de conteúdo — mesma função do "Divisor" do celular.
- **Ícone simulado** (`IconeCruz`): círculo (sprite `Knob.psd`, cor de acento) com duas barras brancas cruzadas por cima — um "+" de saúde sem precisar de nenhum sprite importado (o projeto não tem nenhum ícone customizado, confirmado numa pesquisa antes de começar). Usado no título da barra de ocupação e nos painéis de briefing/vitória/derrota (verde para vitória, vermelho `AccentDerrota` para derrota).
- Como os 4 painéis **já existiam** (da sessão anterior) e o builder só cria o que falta pelo nome, tive que apagar `OcupacaoPanel` e `FluxoDeJogoCanvas` na cena (via CLI, não editando o `.unity`) antes de rodar o menu de novo — senão o builder achava que já estava tudo pronto e não aplicava o redesenho.

**Verificado no Editor conectado:** recompile sem erros (2 rodadas, uma pra corrigir o vazamento de material); os dois menus rodados, hierarquia e materiais criados; em Play Mode, capturas de tela confirmaram a mobília posicionada certinha (balcão, dispensador em cima, cadeira, armário, relógio) e os valores reais dos painéis (`corImg`, `sortingOrder`, `alpha`) batendo exatamente com o código.

**Problema visual encontrado e NÃO resolvido (fora do escopo, documentado pro usuário):** nas capturas de tela dentro da sala aparece um "estouro" de brilho branco no teto e os painéis translúcidos ficam meio lavados. Investigado a fundo: **não é a luz nova** (`Luz`, o `Point Light` da sala) — reduzi a intensidade dela de 6 para 0.35 (17x mais fraca) e o efeito não mudou nem um pouco, o que descarta ela como causa. Achei uma **`Global Volume`** já existente na cena (`SampleSceneProfile`, com `Bloom`+`Tonemapping`+`MotionBlur`+`Vignette`) — pré-existente, não criada nesta sessão — que é a suspeita mais forte (Bloom com threshold baixo reagindo à `Directional Light` da cena, que aparentemente ainda ilumina o teto da sala mesmo ela estando escondida em Y=-30 e fechada). Tentei confirmar desligando a `Directional Light` e recapturando, mas a screenshot saiu **idêntica em bytes** à anterior — sinal de que o Editor não re-renderizou o frame (o mesmo problema de sempre neste ambiente: `Time`/`Update` não avança de verdade sem a janela em foco real). Não deu pra concluir com certeza nem consertar às cegas. **Fica pro usuário conferir numa sessão normal do Editor**: se o brilho aparecer lá também, o primeiro lugar pra olhar é o Bloom do `Global Volume` (Hierarchy > "Global Volume" > Profile).

**Onde mexer nas variáveis principais do jogo (resumo dado ao usuário no chat, repetido aqui pra ficar registrado):**
- `FluxoDeJogo` (objeto solto na raiz): `Ocupacao Inicial`, `Aumento Por Segundo`, `Redução Por Foco`.
- `FluxoDeJogoCanvas` → componente `FluxoDeJogoHUD`: `Duracao Briefing`, `Velocidade Fade Briefing`.
- `DengueHUD` → componente `DengueHUD`: `Duracao Aviso`, `Duracao Aviso Com Dica`.
- `DengueHUD/FocoListaPanel` → componente `FocoListaHUD`: tecla, velocidade de fade.

**Não coberto nesta sessão (avisar o usuário, não decidir sozinho):** o mesmo acabamento "com borda" não foi aplicado no `FocoListaPanel` (lista de focos) nem nos painéis originais do `DengueHUD` (`CounterPanel`, `ToastPanel`, `TargetPanel`, `CompletePanel`) — só nos 4 painéis novos do `FluxoDeJogoHUDBuilder`. Ficou de fora por escopo/tempo, não por dificuldade técnica — o mesmo helper `PainelBordado` serviria.

**Observações para futuras sessões de IA:**
- `renderer.material` em modo Editor **sempre** vaza um material sem dono (o próprio Unity loga um erro no Console avisando) — pra tingir/derivar um material permanentemente, usar `new Material(original)` + `AssetDatabase.CreateAsset(...)` num caminho de verdade, e reaproveitar via `sharedMaterial`, nunca `renderer.material.color =` num builder de Editor.
- Materiais URP/Lit vêm com `_Smoothness = 0.5` por padrão — em superfícies grandes (parede, chão, teto) isso cria brilho especular forte perto de qualquer luz. Zerar (`SetFloat("_Smoothness", 0.05f)`) pra acabamento fosco/fresco.
- Antes de mexer em qualquer prefab de asset pack de terceiros, medir as dimensões reais via `eval` (bounds dos `Renderer`) em vez de adivinhar pelo nome — os números batem exatamente com o que sai no Inspector, e economiza todo o trabalho de tentativa-e-erro de posicionamento.
- Um builder que precisa "lembrar" de um dado do mundo real (ex.: onde o player estava antes) e depois **reconstruir** a mesma geometria não pode simplesmente reler esse dado nas próximas execuções — precisa recuperar o valor já gravado (ex.: de um componente que já existe) antes de apagar/recriar, senão cada rebuild corrompe o dado original com o estado atual (já alterado).

### 2026-09-19 — Sala e HUD refeitas de verdade (o usuário rejeitou a versão anterior: "tá parecendo vibecodado")

**Objetivo:** A sessão anterior deixou a sala e a HUD "feias"/genéricas. O usuário pediu explicitamente pra investigar a fundo e levar o tempo necessário, em vez de só ajustar números às pressas. Investigação completa ANTES de tocar em código (com o Editor conectado: screenshots reais, medição de bounds, teste de hipóteses uma a uma) achou **três causas técnicas concretas**, não só "gosto":

1. **Bug real de geometria**: os prefabs do pack `Office` (parede, moldura de porta, piso) têm o **pivô na quina da malha, não no centro** — confirmado medindo `MeshFilter.sharedMesh.bounds` de cada um (`wall_standard`: `min=(-2,0,-0.1) max=(0,3,0.1)`, ou seja, a malha inteira fica do lado negativo de X a partir do pivô). O código da sessão anterior assumiu pivô central E usou a mesma rotação (90°) nas paredes Leste E Oeste — dois erros empilhados que deixavam buracos nos cantos mostrando o bairro "flutuando" ao fundo (visto claramente numa screenshot de diagnóstico via Scene View).
2. **Luz colada no teto**: o `Point Light` ficava a só 0,4 m do teto — perto o bastante pra estourar por causa da lei do inverso do quadrado, **não importa a intensidade configurada** (provado: reduzi de 6 pra 0,35, 17× mais fraca, e o hotspot na screenshot não mudou nem um pixel — isso é que confirmou que o problema era posição, não intensidade).
3. **Bloom exagerado num `Global Volume` pré-existente da cena** (`SampleSceneProfile`, já existia antes desta feature) — confirmado como causa real porque o mesmo halo aparecia tanto no teto (luz branca) quanto nos painéis da HUD (cor de acento verde-água saturada). Usuário autorizou ajustar.

**Correções em `Assets/Editor/QuartoHospitalBuilder.cs`:**
- Matemática de posicionamento **recalculada do zero**, verificando com `Quaternion.Euler(0,ang,0) * Vector3.right/forward` (não chute) pra onde cada rotação (0/90/180/270) aponta o eixo local X — cada parede tem uma fórmula de "próximo módulo" diferente porque o sinal muda com a rotação (comentado em detalhe no cabeçalho do arquivo). Verificado com uma câmera ortográfica temporária de cima (só em Play Mode, descartada ao sair): **os 4 cantos fecham perfeitamente agora**, sem nenhum gap.
- **Duas luzes fracas em cantos opostos** (`LuzA`/`LuzB`, intensidade 3,5, na ALTURA MÉDIA da sala — não coladas em nada) em vez de uma luz forte colada no teto.
- **Piso mais escuro que a parede** (contraste de ambiente real em vez de tudo no mesmo tom pastel) + **tapete** (`rug_a.prefab`, textura de verdade) na frente do balcão + **quadro** (`big_painting.prefab`) na parede que ficava vazia.
- `LimparMateriaisAntigos()`: apaga a pasta de materiais tingidos e limpa o cache estático no início de cada rebuild — sem isso, mudar uma cor no código não tinha efeito (o cache da rodada anterior, que só reseta com recompile, continuava devolvendo o material antigo) e cada rebuild acumulava arquivos `.mat` órfãos.

**Bloom do `Global Volume`** (autorizado pelo usuário, asset pré-existente): `threshold` 1,0→1,35, `intensity` 0,25→0,12, `clamp` 65472→6.

**Redesign completo da HUD — trocado o estilo "borda neon" pelo estilo sóbrio:**
O acabamento da sessão anterior (`PainelBordado`: retângulo inteiro na cor de acento saturada + painel recuado 3px por cima) foi identificado como a peça mais "IA gerada" do visual — e, pior, é exatamente o que o Bloom pegava e espalhava. Trocado em `Assets/Editor/FluxoDeJogoHUDBuilder.cs`, `Assets/Editor/FocoListaHUDBuilder.cs` e `Assets/Editor/DengueHUDReparo.cs`/`DengueHUDBuilder.cs` (método novo e público `DengueHUDReparo.GarantirAcabamentoSobrio`, chamado pelos dois) por:
- Fundo quase opaco (`CorFundo`) sem depender de brilho pra se destacar.
- **`UnityEngine.UI.Shadow`** (sombra) e **`UnityEngine.UI.Outline`** (contorno 1px bem sutil, branco baixo alpha) como componentes no PRÓPRIO `Image` do painel — não uma segunda `Image` separada. Isso importa de verdade: uma sombra feita com `Image` irmã, fora do `CanvasGroup` do painel, **fica sempre visível mesmo com o painel em alpha 0** (bug real que apareceu nesta sessão: retângulos cinza fantasma flutuando na tela, do `TargetPanel`/`ToastPanel` escondidos). `Shadow`/`Outline` são processados pelo mesmo `CanvasRenderer` do painel, então herdam o `CanvasGroup.alpha` dele automaticamente e acompanham o tamanho se o painel crescer via `ContentSizeFitter`.
- Cor de acento reservada pra detalhes pequenos (ícone, barra de progresso, título) — nunca mais como moldura de painel inteiro.
- As antigas faixas laterais neon (`Faixa` no `ToastPanel`/`FocoListaPanel`, `Accent` no `CounterPanel`) são **desativadas** (não apagadas) se encontradas, idempotente.
- Aplicado nos 4 painéis novos (`OcupacaoPanel`, `BriefingPanel`, `VitoriaPanel`, `DerrotaPanel`), no `FocoListaPanel`, e nos 4 painéis originais (`CounterPanel`, `ToastPanel`, `TargetPanel`, `CompletePanel`) — a HUD inteira agora usa o mesmo acabamento, não só os painéis novos.

**Verificado no Editor conectado (extensivamente, com screenshots reais em cada etapa):** recompile sem erros em cada rodada; vista de cima confirmou os 4 cantos da sala fechados; vista de dentro da sala confirmou iluminação uniforme sem hotspot e mobília bem visível (armário, cadeira, balcão, dispensador); a HUD (contador, lista de focos, ocupação, briefing) confirmada sem halo/vazamento de cor, textos legíveis sobre fundo escuro sólido; o bug da sombra-fantasma foi pego, diagnosticado e corrigido na mesma sessão (screenshot antes/depois comparando).

**Observações para futuras sessões de IA:**
- **Nunca** fazer uma "sombra"/"borda" de painel UI como uma `Image` num `GameObject` separado (irmã ou solta) quando o painel tem `CanvasGroup` — ela não vai herdar o alpha e fica visível/fantasma quando o painel devia estar escondido. Usar `UnityEngine.UI.Shadow`/`Outline` como componente no MESMO `GameObject` do `Image` do painel — são processados pelo mesmo `CanvasRenderer`, herdam `CanvasGroup.alpha` de graça, e acompanham redimensionamento por `ContentSizeFitter`.
- Antes de aceitar "intensidade da luz está errada" como causa de um hotspot, **testar a hipótese isoladamente**: mudar só a intensidade (ordem de grandeza, tipo 6→0.35) e comparar a mesma cena/ângulo. Se nada mudar, a causa é outra (aqui era distância à superfície, depois confirmou-se que também tinha Bloom do Volume global somando).
- Pivô de prefab de asset pack de terceiros: **nunca assumir centralizado**. Medir `MeshFilter.sharedMesh.bounds` (local, não world) antes de escrever qualquer matemática de grade/módulo — e depois verificar pra ONDE cada rotação (`Quaternion.Euler(0,ang,0) * Vector3.right/forward`) aponta o eixo local, já que o sinal da fórmula de posicionamento muda com a rotação.
- Screenshot com `source=screen` neste ambiente **trava num frame antigo** até algo forçar um novo tick real — comparar `bytes` do resultado é um jeito rápido de detectar isso (bytes idênticos = mesmo frame, a mudança que você fez não foi capturada). Não adianta insistir tentando forçar repaint por código; é uma limitação do ambiente, não do jogo.

### 2026-09-20 — Recepção do hospital ganhou gente: médico no balcão + 2 pacientes esperando

**Objetivo:** A sala do hospital (da sessão anterior) tinha mobília mas nenhum personagem — o próprio `contexto do jogo.md` (seção "Cena: Início no Hospital", no topo deste arquivo) descreve pacientes numa fila caótica e uma enfermeira/médica estressada, nunca implementado. Não existia NENHUM modelo humano em lugar nenhum do projeto (confirmado por busca exaustiva antes de começar), então dois pacotes gratuitos novos foram importados pelo usuário via Package Manager > My Assets: `Assets/BackRock Studios/LowPoly-Doctor` (um médico rigged/animado) e `Assets/PolyOne/People Lowpoly` (8 poses estáticas genéricas, sem rig). Escopo combinado com o usuário: só personagens nesta rodada (áudio 3D/trilha fica pra depois), 1 médico + 2 pacientes, estáticos (sem animação, como o documento pede).

**Dois problemas de import corrigidos ANTES de posicionar qualquer coisa (ambos no pacote do médico, não no PolyOne):**
1. **Escala bugada do FBX**: `Lowpoly_Doctor.fbx` vinha com `fileScale=0.01` (herdado do arquivo) resultando numa altura real de só ~0,36 m — testado isolando `useFileScale=false` (deu ~36,3 "unidades cruas") e corrigido com `globalScale=0.0482` no `ModelImporter`, batendo ~1,75 m (altura real de adulto). Sem esse teste isolado (comparar com/sem `useFileScale`) não dava pra saber se o problema era o fileScale ou a malha em si.
2. **Materiais Built-in (magenta no URP)**: os 3 materiais do médico (`lambert2`, `lambert2Editable`, `nf2_aiStandardSurface1`) usavam shader `Standard`/`Autodesk Interactive` (Built-in RP) — apareciam magenta. O menu `Edit/Rendering/Materials/Convert Selected Built-in Materials to URP` **não existe** nesta versão do Editor (6000.5.6f1) — `ExecuteMenuItem` falhou silenciosamente (retornou `false`, sem exceção). Corrigido manualmente: capturar `material.color`/`material.mainTexture` ANTES de trocar `material.shader` pra `Universal Render Pipeline/Lit`, e reatribuir os dois depois — funciona porque esses dois property acessores são resolvidos pelas tags `[MainColor]`/`[MainTexture]` do shader (não por nome fixo de propriedade), então sobrevivem à troca de shader sem precisar saber o nome exato da propriedade em cada shader.

**Pose do médico:** o Animator (Avatar Humanoid, controller com idle/walk/run) foi testado com `AnimationClip.SampleAnimation(instancia, tempo)` na clip "idle" antes de decidir — a clip tem curvas de root motion que descolam o pivô da posição dos pés de um jeito difícil de compensar (pés saíam a -0,90 do pivô em vez de ~0). A pose de BIND (Animator removido, nenhuma clip amostrada) já era uma pose relaxada de braços entreabertos (nada de T-pose esticada, conferido por screenshot) com os pés a só -0,06 do pivô — usada no lugar, mais simples e com resultado melhor.

**Arquivo novo:** `Assets/Editor/PersonagensHospitalBuilder.cs` — companheiro do `QuartoHospitalBuilder.cs` (mesmo padrão de `TerrenoBaldioMato.cs`/`TerrenoBaldioAssets.cs`: método público `Popular(Transform pai, float metade)`, **sem** `[MenuItem]` próprio, chamado de dentro de `QuartoHospitalBuilder.Criar()` logo após `MobiliarSala()` — os personagens nascem/morrem junto com a sala, sem precisar de lógica de limpeza própria). `QuartoHospitalBuilder.Instanciar()` promovido de `private` pra `internal` pra ser reaproveitado. Nenhum dos dois pacotes trazia Collider — `CapsuleCollider` adicionado em cada personagem (dimensões medidas via `Renderer.bounds` antes de escrever os números, mesma disciplina de sempre) pra o `CharacterController` do player esbarrar neles.

**Posicionamento (sala 6×6 m, `metade=3`):** médico ao LADO do balcão (não atrás — medido: o balcão fica com a face de trás encostada exatamente na parede Norte, sem vão nenhum atrás dele) em `(-0.55, 0.06, -2.75)`; paciente sentado (pose `SM_People_Lowpoly_01`, já vem com pés no chão sem precisar de cadeira — conferido visualmente) em `(2.0, 0, 0.4)`; paciente em pé (`SM_People_Lowpoly_03`) em `(1.6, 0, 1.7)` — os dois do lado Leste da sala, longe do corredor até a porta (vão X entre -0.7 e 0.7 perto de Z=3.6) e do armário (canto Nordeste).

**Verificado no Editor conectado:** recompile sem erros; rodado o menu `Dengue > Criar Sala do Hospital` duas vezes seguidas — sem duplicar personagens (sempre 3 filhos em `Personagens`) e com `posicaoNoBairro` preservada. Uma screenshot inicial (câmera alta, olhando de cima-e-de-trás) deu a impressão de personagens "flutuando" — **era ilusão de perspectiva**: medido programaticamente (`Renderer.bounds.min.y` vs. Y do piso da sala) o gap real era 0,002 m pro médico e exatamente 0 pros dois pacientes. Screenshots num ângulo mais horizontal confirmaram visualmente. Console final: 0 erros, 0 warnings.

**Achado à parte (não é bug, não mexido):** por um ângulo de câmera olhando pra cima através do vão da porta, aparecia uma textura parecendo um "deck de madeira" no céu — investigado com uma varredura de todos os `Renderer` da cena numa região acima/além da porta: nada encontrado ali além das próprias peças da sala. Conclusão: é só o Skybox do projeto (uma textura HDRI que por coincidência tem esse padrão), visível through qualquer vão que aponte pra cima — não é geometria de verdade, não precisa de correção.

**Observações para futuras sessões de IA:**
- Import de FBX de asset pack de terceiros: **sempre** medir a altura real (`Renderer.bounds`, não confiar no nome/descrição do pacote) antes de posicionar. Se a escala parecer errada, testar `useFileScale=false` com `globalScale=1` primeiro pra ver o tamanho "cru" do arquivo, depois calcular o `globalScale` que bate com uma altura humana real (~1.6-1.8 m) — muito mais confiável que chutar um fator de correção direto.
- Material com shader Built-in (`Standard`, `Autodesk Interactive`, etc.) aparece magenta em projeto URP. O menu de conversão automática (`Edit/Rendering/Materials/Convert...`) pode não existir nessa versão do Editor — não assumir que `ExecuteMenuItem` funcionou só porque não lançou exceção (ele retorna `bool`, confira o retorno). Alternativa manual que sempre funciona: capturar `material.color`/`material.mainTexture` antes de trocar `material.shader`, reatribuir depois (esses dois acessores seguem as tags `[MainColor]`/`[MainTexture]` do shader ativo, não um nome fixo).
- `AnimationClip.SampleAnimation()` em clipes Humanoid com root motion pode descolar o pivô da posição real dos pés/corpo de um jeito não-óbvio — sempre medir `Renderer.bounds` depois de amostrar antes de confiar na pose. Às vezes a pose de BIND (sem nenhum Animator/clip) já é boa o bastante e evita esse problema inteiro.
- Screenshot de câmera em ângulo alto/de-cima-pra-baixo **engana muito** sobre se um objeto está "flutuando" ou não (perspectiva + chão distante recuando no quadro). Quando a dúvida for só "está encostado no chão ou não", medir `Renderer.bounds.min.y` contra o Y do piso é muito mais confiável e rápido do que ajustar ângulo de câmera repetidamente até "parecer certo".

### 2026-09-20 — Jogador com o corpo do médico, sala branca/vermelha, balões de fala nos pacientes

**Objetivo:** usuário achou os pacientes da sessão anterior "muito feios/low poly", a sala "muito morta" e pediu 4 coisas: (1) o jogador passar a SER visualmente o médico, inclusive vendo o próprio corpo ao olhar pra baixo; (2) trocar o pacote de pacientes por um menos feio (pendente — ver abaixo); (3) sala branca com acentos vermelhos, cara de hospital de verdade; (4) "contexto" — pacientes "falando" que estão doentes, decidido como texto (balão de fala 3D), sem áudio.

**Arquivo novo: `Assets/Editor/PlayerBuilder.cs`** (menu `Dengue > Configurar Player (Corpo Visível)`) — diferente dos outros builders (que apagam/reconstroem do zero), este é idempotente no sentido de **não mexer de novo** no que já está pronto, porque edita um GameObject (`"Cube"`, achado por `CharacterController`, não por nome) que já existe na cena, não algo descartável:
- **Corrigiu 2 bugs reais encontrados no player** (não só o pedido do usuário): o Cube tinha **dois componentes `Mover` ao mesmo tempo**, ambos com `Update()` ativo — `CharacterController.Move()` estava sendo chamado 2x por frame com deslocamentos calculados de forma independente (não é só "componente duplicado inofensivo"). `Pular.cs` usa `GetComponent<Mover>()`, que resolve pro primeiro (`jumpForce=7`) — esse foi o mantido, o outro (`jumpForce=3`) foi destruído.
- Removeu o `MeshFilter`/`MeshRenderer`/`BoxCollider` do Cube (o "corpo" antigo era um cubo esticado via `lossyScale=(0.70,1.32,0.36)`) — confirmado que nenhum outro script depende de o player ter Renderer/BoxCollider.
- `transform.localScale` resetado pra `(1,1,1)` — o médico já nasce na escala real certa (corrigida no import da sessão anterior).
- `CharacterController` recalculado pra `height=1.75, radius=0.28, center=(0,0.875,0)` (mesmos números do `CapsuleCollider` do NPC médico — mesmo prefab, mesma medição). Como `center.y == height/2`, o chão da cápsula virou `transform.position.y` DIRETO — mudança de convenção importante, propagada pro spawn dentro da sala (`QuartoHospitalBuilder.cs`, `Y: 1.2f → 0.05f`).
- Instancia `Lowpoly_Doctor.prefab` como filho `"Modelo"`. Pra ver o corpo sem a câmera ficar "dentro" da própria cabeça: pegou `Animator.GetBoneTransform(HumanBodyBones.Head)` **antes** de remover o Animator, mediu a altura real desse bone (não chutou) pra calcular `MouseLook.eyeHeight` (deu `1.4886`, aplicado via `SerializedObject`, substituindo o `eyeHeight=1` que estava gravado manualmente na cena), encolheu `head.localScale` pra `0.001` (some com a cabeça sem separar malha nem criar material, já que é um único `SkinnedMeshRenderer`) e só então removeu o Animator (reaproveitando `PersonagensHospitalBuilder.RemoverAnimator`, promovido a `internal`).
- Criou a User Layer `"CorpoJogador"` (primeiro slot livre do `TagManager.asset` via `SerializedObject`) e aplicou recursivamente no `Modelo` — seguro pro dia em que o corpo ganhar Collider (hoje não tem, então não afeta o raycast do `Interactor`, mas a `Interactor.cs` ganhou uma máscara de exclusão defensiva mesmo assim).
- **Pendência anotada pro usuário, não automatizável:** a posição de spawn no BAIRRO (`SaidaDoHospital.posicaoNoBairro`, gravada em runtime da primeira vez que a sala foi criada) foi salva com a convenção ANTIGA — na primeira saída da sala depois desta mudança, o jogador aparece uns 0,5-0,66 m afundado no chão do bairro. Correção manual, única vez, direto no Transform no Editor.

**Bug real encontrado e corrigido no `MaterialTingido()` do `QuartoHospitalBuilder.cs`:** o cache (`CacheTingido`) era indexado só pelo material ORIGINAL, sem a cor — inofensivo enquanto só o `Teto` usava um cubo primitivo (`CreatePrimitive`) com uma única cor. Ao introduzir o friso vermelho e a cruz de saúde (também cubos primitivos, MESMO material padrão do Unity, mas cores diferentes: vermelho e branco), o primeiro tingimento pedido pra esse material "vencia" e todo o resto saía com a cor errada. Corrigido trocando a chave do dicionário pra `(Material, Color)` (tupla).

**Sala com cara de hospital (`QuartoHospitalBuilder.cs`):** paleta trocada pra branco clínico (`CorParede=(0.95,0.95,0.94)`, `CorTeto=(0.98,0.98,0.97)`, `CorPiso=(0.80,0.83,0.85)`, antes um azul escuro `(0.30,0.36,0.42)` — principal causa do "morto") + `CorVermelho=(0.82,0.10,0.10)` novo. Cadeira do balcão tingida de vermelho (era branca). Dois elementos novos, só com `GameObject.CreatePrimitive(PrimitiveType.Cube)` (sem asset novo): **`FrisoVermelho`** (faixa baixa ~0,9m nas 4 paredes, 2 segmentos na parede Sul pra não cruzar o vão da porta) e **`CruzDeSaude`** (placa branca + 2 barras vermelhas, na parede Leste — livre, nada preso nela até então —, espelhando a posição do `quadro` já existente na parede Oeste).

**Balão de fala 3D nos pacientes — arquivo novo `Assets/Scripts/BalaoDeFala.cs`:** primeira vez que o projeto usa `TextMeshPro` "puro" (3D/world-space) em vez de `TextMeshProUGUI` (Canvas 2D, usado em toda a HUD até agora). **Dois bugs pegos e corrigidos só depois de ver renderizado**, ambos previsíveis em retrospecto mas não óbvios de antemão:
1. `fontSize` num `TextMeshPro` 3D é em UNIDADES DO MUNDO, não pixels — o valor inicial (`3.5`, "parecia razoável" por analogia com Canvas 2D) cobriu a parede inteira. Corrigido pra `0.7`.
2. O billboard (`Quaternion.LookRotation(direcao)`) apontando `transform.position - cam.position` deixava o texto de costas/espelhado pro jogador — o `+Z` de um `TextMeshPro` precisa apontar NA DIREÇÃO da câmera (texto "olha" pro leitor), não pra longe dela. Invertido pra `cam.position - transform.position`.
Fundo do balão via tag nativa `<mark=#RRGGBBAA>` (sem Quad/material novo). Billboard só no eixo Y (yaw) — não inclina olhando de cima/baixo. Frases: paciente sentado *"Já faz três dias com febre... isso é dengue, com certeza."*; paciente em pé *"Que demora! Minha filha tá com dengue e ninguém atende direito."*

**Bug de infraestrutura sério, achado e corrigido no meio da sessão:** `Interactor.cs` ganhou uma máscara de raycast excluindo a layer `CorpoJogador`, inicialmente como `private static readonly int CamadaCorpoJogador = LayerMask.NameToLayer("CorpoJogador");`. Isso **quebrou o jogo inteiro**: `LayerMask.NameToLayer` chamado num inicializador de campo estático de `MonoBehaviour` lança `UnityException` ("not allowed to be called from a MonoBehaviour constructor..."), e como é um `static readonly` a exceção fica "presa" no `TypeInitializationException` do tipo — TODA leitura futura do tipo `Interactor` (por qualquer script, ex. `DengueHUD.Update()` tentando achar o `Interactor` da cena) relança a mesma exceção. Resultado: **51 mil+ erros idênticos no console** em segundos, um a cada tentativa de acesso. Corrigido movendo a leitura pra uma variável de instância (`camadaCorpoJogador`), preenchida em `Start()` — mesmo padrão de qualquer outra referência de cena no projeto. **Lição:** nunca inicializar um campo `static` de `MonoBehaviour` chamando API do Unity (`LayerMask`, `GameObject.Find`, `Resources.Load` etc.) — só é seguro em `Awake`/`Start`/métodos de instância.

**Pendente (depende do usuário):** trocar o pacote de pacientes (`Assets/PolyOne/People Lowpoly`) por `"PolyPeople Series - Hospital [Free]"` (Simply Poly Lab) — aguardando o usuário baixar via Package Manager > My Assets. Quando importado: medir bounds reais, checar shader (Built-in→URP se precisar), adicionar `CapsuleCollider` se não vier, escolher 2 poses e atualizar `PersonagensHospitalBuilder.CaminhoPacienteSentado`/`CaminhoPacienteEmPe`.

**Verificado no Editor conectado:** recompile sem erros a cada etapa; os dois menus (`Configurar Player`, `Criar Sala do Hospital`) rodados 2x seguidas cada — sem duplicar `Modelo`/personagens, `eyeHeight`/layer preservados entre rodadas. Visão em primeira pessoa (câmera reposicionada via reflection nos campos privados `pitch`/`yaw` do `MouseLook`, já que não há input real neste ambiente) confirmou: corpo do médico visível olhando pra baixo (pernas/pés no chão, sem gap), cabeça não aparece (sem tela preta nem clipping), braços em A-pose ficam meio "flutuando" nas bordas do campo de visão quando o ângulo é bem íngreme — efeito esperado de uma pose estática sem animação de braço, não um bug. Console final: 0 erros, 0 warnings.

**Achado à parte, não corrigido (fora do escopo deste pedido, avisar o usuário):** o piso da sala aparece visivelmente mais escuro/azulado do que o valor real do material (`(0.80,0.83,0.85)`, cinza claro) sugere. Causa raiz identificada via `RenderSettings`: `ambientMode=Skybox` com `ambientSkyColor=(0.21,0.23,0.26)` (azulado) — superfícies com normal voltada pra CIMA (o piso) recebem esse tom ambiente do "céu" do Unity, enquanto as paredes (normais horizontais) usam `ambientEquatorColor` e ficam mais dominadas pela luz direta das duas `Point Light` da sala. Não mexido: a sala já teve uma rodada inteira de ajuste fino de iluminação numa sessão anterior (evitar hotspot), e essa mudança tocaria configuração de ambiente que também afeta o bairro lá fora — precisa de decisão/aprovação do usuário antes de tocar nisso.

**Pivô no meio da sessão — pacientes ficaram SEM CORPO:** o plano original (item 2) era trocar o pacote de pacientes (PolyOne, "feio") por `"PolyPeople Series - Hospital [Free]"`. O usuário baixou e importou (`Assets/PolyPeople_Hospital_Free/`), mas o pacote gratuito só traz **1 personagem de amostra — uma enfermeira de bata branca (`Female_WhiteCoat`, 544 triângulos, já com shader URP/Lit, sem Collider)**, nenhuma pose de paciente; o resto do conjunto (incluindo pacientes) só existe na versão paga. Perguntado ao usuário como proceder — decisão: **abandonar corpo visível de paciente de vez**, deixando só a "voz" (balão de fala) nos lugares onde os pacientes estariam. `PersonagensHospitalBuilder.cs` foi reescrito: removidas as constantes/instanciação dos prefabs do PolyOne e o `AdicionarColisor` deles; agora `CriarVozPaciente(pai, nome, posicaoLocal, texto, altura)` cria só um `GameObject` vazio (`VozPacienteSentado`/`VozPacienteEmPe`) na mesma posição de antes, com o `BalaoDeFala` como filho — sem malha, sem collider. O pacote `PolyPeople_Hospital_Free` **ficou importado mas sem uso** (nenhum código referencia `Female_WhiteCoat`) — disponível caso o usuário queira uma 2ª pessoa da equipe (enfermeira) no futuro; os prefabs do PolyOne (`Assets/PolyOne/People Lowpoly`) também ficaram no projeto sem uso, mesma lógica (não apaguei nenhum dos dois pacotes, só parei de referenciá-los).

**Verificado no Editor conectado após o pivô:** recompile sem erros; `Dengue > Criar Sala do Hospital` rodado de novo — `Personagens` agora com 3 filhos (`Medico`, `VozPacienteSentado`, `VozPacienteEmPe`), balões de fala nas mesmas posições de antes, sem nenhum modelo de paciente visível. Console final: 0 erros.

### 2026-09-20 — Player com animação de andar/correr + sala mais cheia de vida (monitor, telefone, planta, abajur)

**Objetivo:** usuário confirmou "sem pacientes" (já feito) e pediu mais duas coisas: (1) o corpo do jogador **andando parado** ("esquisito") — precisa de animação de verdade; (2) a sala "falta assets, você pode construir assets, qualquer coisa" — deixar menos vazia.

**Animação do jogador — `Assets/Scripts/AnimadorJogador.cs` (novo) + `Assets/Editor/PlayerBuilder.cs` reescrito:**
- Descoberta importante: o `Doctor_AnimController.controller` (Blend Tree pronto: parâmetro `Speed` Float, `Idle` no threshold 0, `Walk` em 0.5, `Run` em 1 — confirmado inspecionando o `BlendTree` via `UnityEditor.Animations.AnimatorController` no Editor conectado, não chutado) **nunca esteve atribuído a nenhum Animator do pacote** — nem no prefab, nem na FBX, nem na cena de demo (`grep` por "Doctor_AnimController" em todo `Assets/BackRock Studios` só encontra o próprio arquivo do controller). `PlayerBuilder.cs` agora atribui isso manualmente (`animModelo.runtimeAnimatorController = ...`) — sem isso, `SetFloat("Speed", ...)` não tinha efeito nenhum (`runtimeAnimatorController` ficava `null`, confirmado no Editor conectado antes de perceber o problema).
- `Mover.cs` ganhou duas propriedades públicas (`VelocidadeAtual`, `VelocidadeMaxima`) só de leitura, sem mudar nenhum comportamento existente.
- `AnimadorJogador.cs`: fica no player (Cube), lê `Mover.VelocidadeAtual/VelocidadeMaxima` a cada frame, suaviza (`MoveTowards`) e manda pro parâmetro `Speed` do Animator — como não há sprint/andar devagar no jogo (um `moveSpeed` só), na prática alterna Idle↔Run com uma transição suave pelo meio (Walk), o que já resolve o "andando parado".
- `Animator.applyRootMotion = false` — quem move o player continua sendo só o `CharacterController` via `Mover.cs`; a animação nunca desloca o personagem (evita de vez o problema de root motion descolando o pivô, já visto numa sessão anterior testando a pose "idle" do NPC).
- **Diferente do NPC da recepção** (que fica com o Animator REMOVIDO, estático): o player agora MANTÉM o Animator. Isso funciona junto com o truque de esconder a cabeça (`head.localScale = 0.001`) porque animação Humanoid só afeta ROTAÇÃO dos ossos, nunca a escala — a cabeça continua escondida com o Animator rodando.
- `PlayerBuilder.Configurar()` deixou de ser "só cria o Modelo se não existir" — agora **sempre apaga e recria o Modelo** a cada rodada (mais fácil iterar; as outras partes — Mover duplicado, CharacterController, layer — continuam idempotentes por natureza, sem precisar apagar nada).
- **Não verificado por screenshot nesta sessão** (ver observação de ambiente abaixo) — verificado por API: `runtimeAnimatorController` atribuído corretamente, `SetFloat("Speed", 1f)` aceito sem erro, valores do Blend Tree conferem exatamente com o parâmetro alimentado. Risco residual baixo (mecanismo padrão do Unity, nada exótico), mas vale o usuário confirmar visualmente jogando.

**Sala mais cheia de vida — `QuartoHospitalBuilder.cs`, `CriarDecoracaoExtra()` (novo):** tudo peças do pacote Office já no projeto (sem import novo):
- Monitor (`new_monitor.prefab`) + telefone (`phone.prefab`) no lado direito do balcão (o dispensador já ocupa o esquerdo) — a recepção parece "em uso".
- Planta (`nature_deco_plant.prefab`) perto da porta.
- Abajur de chão (`lamp.prefab`) no canto Sudoeste, o único que sobrou livre — por coincidência feliz, a cúpula dele já é vermelha na textura original do pack, combinando com a paleta sem precisar tingir.
- **Achado no meio do caminho**: a primeira tentativa de rotação do monitor (200°) deixou a tela de costas pra sala — só percebi depois de finalmente conseguir uma screenshot que funcionou (ver observação de ambiente abaixo); corrigido pra 20°, reconstruído e confirmado visualmente que a tela (com um "desktop" na textura) fica de frente pra quem entra.

**Observação de ambiente (relevante pra qualquer sessão futura que dependa de screenshot):** nesta sessão, além do já documentado "screenshot trava num frame antigo", surgiu um caso novo: depois de editar um arquivo `.cs` já compilado antes, `unity command recompile` respondeu `"up_to_date"` (achando que nada mudou) mesmo com o arquivo em disco já atualizado — o menu rodou de novo mas com o CÓDIGO ANTIGO ainda carregado (rotação do monitor continuava 200° depois de eu já ter editado pra 20°). Só resolveu com `AssetDatabase.ImportAsset(caminho, ImportAssetOptions.ForceUpdate)` chamado explicitamente sobre o arquivo — isso disparou um reload de domínio de verdade (o Pipeline server até trocou de porta, 7801→7800). **Lição: depois de editar um `.cs`, não confiar cegamente em `recompile` dizer "up_to_date" — se uma mudança de valor simples não aparecer na cena depois de reconstruir, forçar `AssetDatabase.ImportAsset(caminho, ImportAssetOptions.ForceUpdate)` no arquivo específico antes de continuar depurando por outros motivos.**

**Verificado no Editor conectado:** recompile sem erros (depois do ForceUpdate); os dois menus (`Configurar Player`, `Criar Sala do Hospital`) rodados 2x seguidas — sem duplicar nada. Screenshots (depois de finalmente conseguir contornar o travamento do ambiente, ver abaixo) confirmaram visualmente: monitor com tela de frente, planta e abajur bem posicionados e sem flutuar, os dois médicos (NPC da recepção e o jogador, agora ambos com o mesmo modelo) lado a lado na mesma cena. Console final: 0 erros, 0 warnings.

**Como contornei o travamento de screenshot nesta sessão:** `capture_game_view` (`source=screen` e `source=camera`) e até `capture_scene_view` ficaram travados num frame antigo por boa parte da sessão, mesmo depois de `editor_focus`, `SceneView.RepaintAll()` e `EditorApplication.QueuePlayerLoopUpdate()` forçados via `eval` — nada disso resolveu enquanto o Play Mode estava rodando há um tempo. O que finalmente destravou: **voltar pro modo Editor (sair do Play Mode)** — os `capture_scene_view` seguintes vieram frescos de novo. Ou seja, a limitação parece specific a sessões de Play Mode prolongadas nesse ambiente, não ao mecanismo de screenshot em si. Pra verificar animação/gameplay de verdade no futuro, o jeito mais confiável continua sendo o usuário rodar no próprio Editor.

### 2026-09-20 — Corpo do jogador "afundado"/deformado com a animação — corrigido (pés grudados no chão)

**Objetivo:** usuário reportou, jogando de verdade, que o corpo animado do player estava "muito esquisito", "parece que todos o modelo se unificou, ficou tudo feio" — e sugeriu usar a animação que o modelo "já tem" em vez de uma criada do zero (a entrada anterior já usava o Blend Tree PRONTO do pacote, `Doctor_AnimController`, não uma animação nova — o problema não era a origem da animação, e sim um bug de posicionamento, descoberto ao investigar).

**Causa raiz (medida ao vivo em Play Mode, não só amostrando o clipe fora do ar — foi isso que enganou na entrada anterior):** mesmo com `Animator.applyRootMotion = false`, a pose que o Blend Tree produz de verdade (Idle/Walk/Run) fica **~0,84 m mais baixa** do que a pose de bind usada pra calcular `OffsetPeMedico` (0,06). Ou seja: o `applyRootMotion=false` impede a TRANSLAÇÃO automática do GameObject, mas não impede que a pose interna do esqueleto reflita a altura mais baixa das curvas de root motion do clipe. Resultado: o personagem ficava enterrado até a cintura no piso — o que o usuário descreveu como "modelo unificado". É um defeito nos próprios clipes do pacote gratuito (mesma raiz do problema já visto e documentado ao testar a animação "idle" isoladamente, numa sessão anterior), não do script.

**Correção em `Assets/Scripts/AnimadorJogador.cs`:** em vez de tentar consertar o asset (ou calcular um offset fixo, que não funcionaria igual pra Idle/Walk/Run já que cada clipe descola o pivô de um jeito diferente), a solução foi **"pés grudados no chão" recalculado TODO FRAME**: um novo `LateUpdate()` (roda depois do Animator já ter aplicado a pose do frame) mede `Renderer.bounds.min.y` de todos os renderers do `Modelo` e desloca o `Modelo` inteiro (não os ossos individuais — não mexe na malha, só na raiz) o suficiente pra encostar de nascença no piso (`transform.position.y` do player, mesma convenção já estabelecida). Funciona igual não importa qual pose o Blend Tree estiver tocando.

**Descoberta lateral que quase levou a um diagnóstico errado:** logo depois de corrigir isso, uma medição em Play Mode ainda mostrava o gap antigo (-0,84) — só depois de forçar `AssetDatabase.ImportAsset(caminho, ImportAssetOptions.ForceUpdate)` no `AnimadorJogador.cs` (mesmo truque já documentado na entrada anterior pro `QuartoHospitalBuilder.cs`) e **salvar a cena antes de entrar em Play Mode** (`EditorSceneManager.SaveOpenScenes()`) que a correção passou a valer de verdade. Two descobertas separadas aqui, ambas relevantes pra qualquer sessão futura:
1. Editar um `.cs` nem sempre é pego automaticamente por `unity command recompile` (pode responder `"up_to_date"` mesmo com o arquivo mudado) — reforça a lição já registrada acima.
2. **Sempre salvar a cena (`EditorSceneManager.SaveOpenScenes()`) depois de rodar um builder e ANTES de entrar em Play Mode** — mudanças feitas em modo Editor (via `eval`) que não foram salvas correm risco de não estarem presentes no estado que o Play Mode realmente usa (confirmado: sem salvar, `Animator.applyRootMotion` voltava pra `true` ao entrar em Play, mesmo o código tendo acabado de setar `false`).

**Verificado no Editor conectado, AO VIVO em Play Mode (não só por API/asset como a entrada anterior tinha ficado, dessa vez com medição real):** `gap pés-piso` medido em `Renderer.bounds.min.y` contra `player.transform.position.y` — **0,0 exato**, parado (Idle) e reconferido depois de forçar `Speed=1` (Run) também. Screenshot (finalmente consegui uma fresca, depois do Play Mode recém-iniciado) confirmou visualmente: sem o efeito de "afundado"/deformado, corpo bem posicionado olhando pra baixo. Console final: 0 erros. Os dois menus rodados de novo em sequência (idempotência) sem duplicar nada.

### 2026-09-20 — Brilho por proximidade nos focos de dengue

**Objetivo:** o usuário pediu que os focos de criadouro ficassem mais fáceis de enxergar: eles devem **brilhar quando o jogador chegar perto**, complementando (sem substituir) o highlight de mira que já existia. Raio, prioridade e cor foram decididos com o usuário: raio médio (~5,5 m), mira sempre tem prioridade sobre o brilho de proximidade quando os dois coincidem, e cor/raio globais (sem customização por tipo de foco).

**`Assets/Scripts/EmissionHighlightUtil.cs` (novo):** helper estático que extrai a técnica de emissão já usada em `Interactor.ApplyHighlight`/`RestoreHighlight` e em `Interactable.FlashRoutine` (instanciar `renderer.material`, guardar `_EmissionColor`/estado do `_EMISSION` original, aplicar, restaurar) — evita triplicar essa lógica. `Interactor.cs` e `Interactable.cs` não foram tocados, continuam com a própria cópia da lógica (risco desnecessário mexer em código já estável só para reaproveitar).

**`Assets/Scripts/FocoProximityGlow.cs` (novo):** componente colocado no player (mesmo objeto que tem `Interactor`). No `Start()` cacheia `Camera.main` e a lista de `Interactable` da cena (`FindObjectsByType`, incluindo inativos). No `Update()`, para cada foco ainda `CanInteract` e ativo na hierarquia, mede a distância de `Camera.main.transform.position` até o foco; dentro do raio (`raioProximidade = 5.5`) aplica um brilho verde-água fixo (`corBrilho = (0.25, 1, 0.8)` — a mesma cor de "foco/sucesso" já usada em `Interactable.popColor` e `FocoHUDInfo.corDaBarra` — vezes `intensidadeBrilho = 0.5`, sem pulso); fora do raio, restaura. **Nunca mexe num foco que seja `Interactor.CurrentTarget` no frame** — assim a mira (amarela, mais forte) sempre tem prioridade e os dois sistemas nunca disputam a mesma instância de material.

**`Assets/Editor/FocoProximityGlowBuilder.cs` (novo):** menu **Dengue > Configurar Brilho de Proximidade dos Focos**, idempotente (acha o player por `CharacterController`, só adiciona o componente se ainda não existir) — mesmo padrão do `PlayerBuilder.cs`.

**Como foi aplicado na cena:** com o Editor já aberto e conectado via `unity command`, rodei `recompile` (checado por `recompile_status` até `completed`, sem erros), depois `menu --path "Dengue/Configurar Brilho de Proximidade dos Focos"` e `save_scene`. Confirmado por `find_gameobjects --type FocoProximityGlow` que o componente ficou no GameObject `/Player` (o mesmo que tem `CharacterController`, `Interactor` e `MouseLook` — **não** existe mais um "Cube": o player foi renomeado para "Player" em alguma sessão anterior, mas o comentário de cabeçalho do `PlayerBuilder.cs` ainda cita "Cube"; há também um outro GameObject chamado literalmente "Cube" na cena sem nenhum desses componentes — **cuidado ao usar `GameObject.Find("Cube")` em sessões futuras, o nome certo do player é "Player"**).

**Verificado ao vivo em Play Mode:** teleportei o player pra 3 m de um pneu (dentro do raio) via `eval` e li a cor de emissão do material do pneu — bateu exatamente com o valor esperado (`corBrilho × intensidadeBrilho` = RGBA 0,125/0,500/0,400/0,500), confirmando que a captura e a aplicação do brilho funcionam. Ao se afastar, a keyword `_EMISSION` voltou a ficar desligada (restaurada). **Achado sobre o ambiente de teste, não sobre o jogo:** neste projeto o Editor às vezes entra em Play Mode mas fica com o loop de jogo **congelado** (`Time.frameCount` parado) quando a janela está sem foco — `editor_status` reportou `"playMode":"stopped"` mesmo depois de um `editor_play` bem-sucedido anterior; foi preciso reentrar em Play (`editor_play` de novo) e confirmar `Time.frameCount` avançando antes de qualquer teste valer. Não visto testar o comportamento com múltiplos focos próximos ao mesmo tempo nem o cruzamento mira↔proximidade jogando manualmente (só validado via `eval`, não com o mouse/teclado de verdade) — vale conferir numa sessão jogável normal.

**Arquivos alterados:**
- `Assets/Scripts/EmissionHighlightUtil.cs` (novo)
- `Assets/Scripts/FocoProximityGlow.cs` (novo)
- `Assets/Editor/FocoProximityGlowBuilder.cs` (novo)
- `Assets/Scenes/SampleScene.unity` — componente `FocoProximityGlow` adicionado ao Player, salvo via `save_scene`.

**Observações para futuras sessões de IA:**
- Se for customizar cor/raio por tipo de foco no futuro, o ponto de extensão natural é `FocoHUDInfo` (mesmo padrão já usado pra textos/cor da barra) — decisão consciente de não fazer isso agora porque o usuário pediu valores globais.
- `raioProximidade`, `corBrilho` e `intensidadeBrilho` são `[SerializeField]` no Inspector do `FocoProximityGlow` (no GameObject Player) — dá pra ajustar sem mexer em código.

### 2026-09-26 — Skill de preview para mudanças visuais

**Objetivo:** a pedido do usuário, criada uma skill para que, sempre que ele pedir um plano que mude a interface gráfica ou o design do jogo, a IA mostre um preview visual (mockup antes × depois) antes de implementar.

**Arquivos alterados:**
- `.claude/skills/preview-design/SKILL.md` (novo) — instruções: ler o estado atual, capturar screenshot da Game view se possível, montar mockup com `show_widget` usando cores/tamanhos reais do código, apresentar plano e aguardar aprovação.
- **Atualização (mesmo dia):** o usuário pediu que o preview seja gerado **dentro do Unity**. A skill foi reescrita: em Play Mode, aplica a mudança temporária via `unity command eval_file` sob um GameObject `__Preview`, captura antes/depois com `unity command screenshot '{"view":"game"}'` e depois limpa. Verificado ao vivo: Canvas `ScreenSpaceOverlay` não sai na captura (nem o HUD OnGUI); `ScreenSpaceCamera` com `Camera.main` sai corretamente. Mockup HTML só como fallback se o Editor não estiver conectado.

### 2026-09-26 — Player refeito: movimentação nova + corpo de agente de saúde modelado do zero

**Objetivo:** o usuário pediu para "melhorar o player, refazer toda a movimentação já existente e colocar um asset de agente de saúde". Decisões com o usuário: **primeira pessoa** continua; recursos novos **correr com Shift**, **balanço de câmera** e **suporte a controle (gamepad)**; agachar fica de fora. O plano era usar o pacote CC0 da Quaternius ("Ultimate Animated Character Pack"), mas o Google Drive dele recusou o download (*Quota exceeded*). O usuário então pediu: "faça você mesmo um modelo foda". O agente foi **modelado, rigado e animado do zero no Blender 5.2** (instalado na máquina), por script.

**Estado encontrado na cena (diferente do que as entradas anteriores descrevem):** o player (`/Player`) não tinha mais o corpo do médico (`Modelo`). Tinha um filho `Cube` achatado (escala 0,31×1×0,08, com `BoxCollider` na layer Default), `MouseLook.eyeHeight = 2,12` e `CharacterController.skinWidth = 0,08`, que deixava o corpo flutuando 8 cm. O `Cube` foi tratado como corpo provisório e removido pelo builder.

**Modelo: `Ferramentas/Blender/gerar_agente.py` (novo, fora de `Assets/`).**
- Uso: `blender -b --factory-startup --python gerar_agente.py -- <pasta_saida> [--sem-render]`.
- Gera `AgenteSaude.fbx`, `AgenteSaude.blend` e prévias PNG.
- Low poly: 7,8 mil triângulos no corpo e 1,4 mil na cabeça. Altura ~1,78 m com o boné.
- Visual: colete azul ACS com duas faixas refletivas amarelo-limão, zíper, bolsos com lapela, "ACS" no peito direito e "AGENTE DE SAÚDE" nas costas; camiseta branca, calça jeans, tênis com sola branca; boné azul com logo (círculo branco + cruz azul); crachá no peito esquerdo (faixa, foto, linhas de nome, clipe); bolsa tiracolo bege com "SAÚDE" na lapela; relógio no pulso esquerdo (aparece em primeira pessoa).
- Esqueleto Generic com nomes compatíveis com Humanoid (`Hips`, `Spine`, `Chest`, `Neck`, `Head`, `Left/RightUpperArm`, `LowerArm`, `Hand`, `UpperLeg`, `LowerLeg`, `Foot`). Pesos por proximidade de junta, com mistura suave.
- Animações in-place a 30 fps: `Idle` (90 f), `Walk` (16 f, passo ~0,74 m, feita para 2,8 m/s), `Run` (15 f, passo ~1,45 m, feita para 5,8 m/s) e `Air` (pose no ar, 20 f).
- A **cabeça é uma malha separada** (`AgenteSaude_Cabeca`): no jogo fica em *Shadows Only*. Some da câmera em primeira pessoa mas continua fazendo sombra, sem o truque antigo de encolher o osso `Head`.

**Asset no Unity:** `Assets/Meus/Assets Gerados/Agente de Saude/` com `AgenteSaude.fbx`, `Materiais/Agente_*.mat` (19 materiais URP/Lit, cor sólida, instancing ligado, sem emissão), `Agente_AnimController.controller` e `Agente de Saude.prefab` (variante do FBX com o controller).

**Scripts novos (`Assets/Scripts/`):**
- `EntradaJogador.cs` — único lugar que lê a entrada, via `Assets/InputSystem_Actions.inputactions` (mapa `Player`: Move, Look, Jump, Sprint, Attack). Teclado, mouse e controle funcionam.
  - `sensibilidadeMouse` em graus por pixel; o builder copiou o valor que o usuário tinha ajustado no `MouseLook` (0,5). `sensibilidadeControle` em graus por segundo.
  - Buffer de pulo de 0,12 s. `Habilitado` bloqueia tudo. `DefaultExecutionOrder(-100)`.
- `ControladorJogador.cs` — substitui `Mover` + `Pular` + o giro do `MouseLook`. Um único `Update`: olhar → andar/correr → pulo → gravidade → um `CharacterController.Move`.
  - Andar 2,8 m/s; correr 5,8 m/s (Shift + andar para a frente).
  - Pulo por altura (`alturaPulo` 1,1 m) com coyote time de 0,12 s. A gravidade é integrada pela velocidade média do frame, então a altura sai exata em qualquer FPS.
  - O `yaw` inicial vem da rotação da cena; o `MouseLook` começava em 0 e dava um "estalo".
  - API pública: `ControleAtivo`, `Yaw`, `Pitch`, `VelocidadeHorizontal`, `VelocidadeVertical`, `VelocidadeAndar/Correr`, `NoChao`, `Correndo`, `TempoNoAr`, eventos `OnPulou` e `OnAterrissou(float velQueda)`, `Teleportar(Vector3)` e `DefinirOlhar(yaw, pitch)`.
- `CameraJogador.cs` (na Main Camera) — a câmera gira em torno do **pescoço** (`alturaPivo` medido no osso `Head`, ~1,49 m) com os olhos em `olhosNoPivo` (0,10 m à frente e 0,13 m acima). Olhando para baixo, os olhos vão para a frente, como uma cabeça de verdade, e aparecem colete, mãos e pontas dos pés sem a câmera entrar no peito.
  - Head bob sincronizado com a distância andada (mesmo comprimento de passo das animações).
  - Afundada com mola amortecida ao aterrissar; FOV +6° na corrida; `nearClipPlane` 0,05.
- `AnimadorJogador.cs` (reescrito, mesmo GUID) — `Speed` mapeado pela velocidade real (andar = 0,5, correr = 1), por isso os pés não patinam. `NoChao` troca para a pose no ar na hora ao pular e com 0,15 s de tolerância ao cair de degrau. **A gambiarra "pés grudados no chão" por frame foi removida**; ela era específica dos clipes do médico.

**Removidos:** `Assets/Scripts/mover.cs`, `pular.cs` e `MouseLook.cs` (+ `.meta`), via `delete_asset`, depois de salvar a cena. Não sobrou referência a eles em cena ou prefab; só nas cópias automáticas de `Assets/_Recovery/`, que não são usadas.

**Integrações:**
- `SaidaDoHospital.cs` usa `ControladorJogador.Teleportar` (com fallback para o CC puro).
- `FluxoDeJogoHUD.cs`: campo `jogador`; na vitória ou derrota faz `ControleAtivo = false` e o jogador fica parado na tela final.
- `Interactor.cs`: além do clique esquerdo, aceita a ação `Attack` do `EntradaJogador` (botão Oeste do controle).

**`Assets/Editor/PlayerBuilder.cs` (reescrito):** menu **Dengue > Configurar Player (Agente de Saúde)**. É idempotente e acha o player pelo `CharacterController`. Etapas:
- **Materiais:** cria ou atualiza os materiais da paleta.
- **Import do FBX:** `bakeAxisConversion`, Generic, sem root motion, clipes renomeados `AgenteSaude|Walk` → `Walk` e em loop, remap de material por nome.
- **Controller:** recria o controller (Blend Tree 1D `Speed` Idle 0 / Walk 0,5 / Run 1; estado `NoAr` pelo bool `NoChao`, padrão true).
- **Prefab:** recria o prefab com a pose Idle aplicada.
- **Player:** troca os componentes antigos (por nome de tipo, funciona mesmo com os scripts já apagados) e ajusta o CC (altura 1,75, raio 0,3, centro 0,875, degrau 0,35, `skinWidth` 0,03, com o modelo descendo 0,03).
- **Corpo:** substitui o corpo antigo (`Modelo` ou `Cube`) pelo prefab, com a cabeça em *Shadows Only*, `updateWhenOffscreen` e a layer `CorpoJogador`.
- **Câmera:** coloca o `CameraJogador` na Main Camera.
- **Checagem:** avisa no console se o modelo estiver olhando para -Z.

**Verificado no Editor conectado (Unity CLI):**
- Recompile sem erros.
- Builder rodado várias vezes sem duplicar nada.
- Play Mode com teclado e mouse virtuais do Input System:
  - Parado: câmera a 1,62 m.
  - W: 2,80 m/s com `Speed` 0,50. W + Shift: 5,80 m/s com `Speed` 1,00 e FOV 66. Soltar: para.
  - Pulo: 1,100 m exatos, estado `NoAr` tocado e `OnAterrissou` disparado uma vez.
  - Mouse: 60 px → 30° de yaw, com o corpo girando junto.
  - Porta do hospital: teleporta e inicia a partida.
  - Ocupação forçada a 100%: derrota, `ControleAtivo` false e W não move mais.
- Capturas em primeira pessoa (fora do Play, com a mesma conta de câmera) mostram mãos e pontas dos tênis a ~60° para baixo, e colete, alça, zíper e crachá bem para baixo.

**Observações para futuras sessões de IA:**
- **Mudar o visual do agente:** editar `Ferramentas/Blender/gerar_agente.py`, rodar no Blender, copiar o FBX para `Assets/Meus/Assets Gerados/Agente de Saude/` e rodar o menu. O script já gira o objeto do esqueleto 180° só na exportação. Com *Bake Axis Conversion*, o `axis_forward` do exportador não faz diferença: sem a rotação o agente entra no Unity olhando para -Z (de costas).
- **Play Mode com o Editor sem foco** fica parado no frame 2. `Application.runInBackground = true` destrava, mas **também grava `PlayerSettings.runInBackground = true` no projeto**: voltar para false depois. Trocar `InputSystem.settings` por uma cópia (`IgnoreFocus` + `AllDeviceInputAlwaysGoesToGameView`) faz o teclado virtual funcionar sem mexer no asset; restaurar e remover os dispositivos no fim.
- `capture_game_view` com `save_path` relativo grava em `Assets/Temp/...`: apagar a pasta depois, senão vira asset.
- Se um `.cs` for salvo durante o Play Mode (por exemplo, por outra sessão), o Unity recompila e recarrega o domínio no meio do teste e todo o estado se perde.
- Ajustes finos pelo Inspector, sem código: velocidades, pulo e aceleração no `ControladorJogador`; sensibilidade no `EntradaJogador`; balanço, afundada, FOV e posição dos olhos (`olhosNoPivo`) no `CameraJogador` da Main Camera.

### 2026-09-26 — História contada pelo celular + abertura/epílogo em cartelas + enfermeira na recepção

**Objetivo:** o usuário pediu o jogo "numa pegada mais de história". Decisões dele: (1) o celular/tweets SÃO a história — tudo o que o jogador fizer gera um tweet de alguém do bairro sobre aquilo; (2) abertura + epílogo em cartelas; (3) protagonista = agente comunitário de saúde, e o NPC da recepção deixa de ser o médico (mesmo modelo do corpo antigo do jogador) e vira a enfermeira.

**Arquivos novos:**
- `Assets/Scripts/HistoriaRedeSocial.cs` — o narrador. `PersonagemDoBairro` (autor, @, `TipoFoco`, falas `aoChegarPerto`/`aoEliminar` (lista em ordem, depois sorteio)/`aoEliminarUltimo`/`aoLargarPelaMetade`, `epilogoLimpo`/`epilogoPendente`). Elenco padrão: Carlos (pneus), Dona Neusa (vasos), João (caixas d'água, filho internado), Ana Paula (piscina), Bruna (garrafas); Enf. Rita e UBS para hospital/ocupação. Outros gatilhos: saída do hospital (`FluxoDeJogo.OnPartidaIniciada`), ~60 s sem foco, ~35 s parado (mede o deslocamento real do player, ignora com o celular aberto), 2 focos em < 20 s, 4 pulos em 6 s (`ControladorJogador.OnPulou`), marcos de ocupação 70/85/95% subindo e 40% descendo (só disparam depois de "armados" do outro lado do limite), derrota. Posts de comportamento só entram se `RedeSocial.NaFila <= 2`. Marcadores próprios: `{restantesTipo} {eliminadosTipo} {totalTipo} {ocupacao}`. Liga `RedeSocial.NarradorExterno` no Awake e enfileira os `PostsFinais` quando o último foco cai. `LinhasEpilogo(vitoria)` monta o epílogo (derrota = só tipos pendentes). Tudo editável no Inspector do objeto `Historia`.
- `Assets/Scripts/CartelasNarrativas.cs` — tela preta + frase com fade, em tempo real (`unscaledDeltaTime`); clique/Espaço/Enter avança (clique na entrada só completa o fade), Esc pula tudo; callback `antesDeClarear` e `aoTerminar`.
- `Assets/Scripts/HistoriaDoJogo.cs` — abertura (5 cartelas) com `Time.timeScale = 0`, `ControleAtivo = false`, `Interactor.Bloqueado` e `CelularController` desligado; pulada ao reiniciar com R (flag estática zerada a cada Play por `RuntimeInitializeOnLoadMethod(SubsystemRegistration)`); restaura `timeScale` no `OnDestroy`. Epílogo 1,6 s após vitória/derrota: "Uma semana depois..." + fala de cada personagem em formato de post + fechamento; segura a tela de estatísticas (`FluxoDeJogoHUD.SegurarTelaFinal`) até o fim.
- `Assets/Scripts/NotificacaoCelular.cs` — banner push acima do ícone do celular (avatar com a cor do feed, autor, @, 2 linhas com reticências); fila com mínimo de 2,6 s por aviso; some e descarta a fila com o celular aberto.
- `Assets/Editor/HistoriaBuilder.cs` — menu **Dengue > Configurar Historia** (idempotente): cria `CartelasCanvas` (sortingOrder 30), `CelularHUD/Notificacao`, objeto `Historia` com os dois componentes, liga referências e troca o texto antigo do `BriefingTexto`.

**Arquivos alterados:** `RedeSocial.cs` (`Enfileirar` público, `NaFila`, `PostsFinais`, `NarradorExterno`); `FeedRedeSocial.cs` (`Marcadores`, `CorDoAutor`, `Inicial` públicos); `FluxoDeJogo.cs` (evento `OnPartidaIniciada`, `IniciarPartida` não dispara 2x); `FluxoDeJogoHUD.cs` (`SegurarTelaFinal`); `FluxoDeJogoHUDBuilder.cs` (`BriefingPadrao` internal, agora aponta pro celular); `PersonagensHospitalBuilder.cs` (recepção = `Female_WhiteCoat` com `BaixarBracos()` — gira os ossos Humanoid da T-pose antes de remover o Animator —, collider 1,6/0,26, balão com a missão; cai no médico se o pacote sumir); `QuartoHospitalBuilder.cs` (só comentário); `BalaoDeFala.cs` (billboard invertido — ver pendências).

**Aplicado na cena (via CLI, salva):** grupo `SalaDoHospital/Personagens` refeito chamando `PersonagensHospitalBuilder.Popular(sala, 3f)` (sem reconstruir a sala inteira, para não mexer no player da sessão paralela); menu `Configurar Historia` rodado 2x (2ª: nada a criar).

**Verificado em Play Mode:** abertura com `timeScale 0`, controles travados e 4 posts de abertura retidos na fila; ao fim, tudo destrava e os posts saem; saída do hospital → posts da Enf. Rita e da UBS ("Ocupação da UBS em 70%"); pneu eliminado → Carlos com "Faltam 3"; marcos 85/95/100% + derrota postados; epílogo de derrota com as 5 falas pendentes, depois a tela de derrota; banner visível com avatar/autor. Enfermeira com pés exatamente no piso (raycast + vértices do `BakeMesh`; o `Renderer.bounds` do prefab enganava com 5 cm de folga).

**Pendente (limite de uso atingido no meio):**
1. `BalaoDeFala.cs` foi editado mas **não recompilado/testado**. Diagnóstico confirmado em Play: com a versão antiga o texto aparecia espelhado; girando o balão com a frente para longe da câmera ficou legível.
2. O balão da enfermeira (1,25 m de largura) entra na parede Norte quando visto de lado — pôr o balão num filho âncora ~0,5 m à frente dela (o `BalaoDeFala.Start` sobrescreve `localPosition`, então precisa da âncora) e recriar o grupo `Personagens`.
3. Não testados ainda: aproximação, foco largado pela metade, combo, pulos, parado, tempo sem foco, marco de 40% descendo, vitória + epílogo de vitória, reinício com R pulando a abertura.
4. Balanceamento já existente na cena: `FluxoDeJogo` começa em 70% e sobe ~1%/s → derrota em 30 s sem limpar nada (o default do código é 0,3%/s, mas a cena guarda o valor antigo).

**Observações para futuras sessões de IA:**
- Em projeto URP, TextMeshPro 3D fica legível com o +Z apontando PRA LONGE da câmera.
- `capture_game_view --save_path` relativo grava em `Assets/Temp/` — apagar os PNG + .meta depois. `Application.runInBackground = true` (necessário para o Play avançar com o Editor sem foco) grava `PlayerSettings.runInBackground` — voltar para false no fim.
- No PowerShell 5.1, `unity command eval --code '...'` perde as aspas duplas; usar `eval_file`.

### 2026-09-26 — História (parte 2): ritmo da ocupação, balões de fala legíveis e testes que faltavam

**Objetivo:** fechar as pendências da entrada anterior e dois pedidos novos do usuário: (1) "no início seja mais lenta, pra dar tempo do player pensar e ler um pouco o que tá acontecendo; quando a pessoa puxa o celular a taxa de ocupação suba mais devagar — o player não precisa saber disso"; (2) "os balões de fala mais legíveis".

**Ritmo da ocupação — `Assets/Scripts/FluxoDeJogo.cs`:** campos novos (a cena pegou os defaults sozinha; o `aumentoPorSegundo = 0.01` que o usuário configurou foi mantido): `aumentoNoInicio` 0.002 (0,2%/s), `duracaoInicioCalmo` 45 s, `duracaoRampa` 60 s (sobe linear até `aumentoPorSegundo`), `multiplicadorComCelular` 0.25 e referência `celular` (vazia = procura). `TaxaAtual()` privado junta tudo; nenhum aviso na tela. Com os valores da cena (70% inicial, −15% por foco): ~87 s até perder sem limpar nada, em vez de 30 s. Medido em Play: 0,200%/s logo após sair do hospital (Δocupação real em 7,5 s de jogo), 0,600%/s no meio da rampa, 1,000%/s depois, 0,250%/s com o celular aberto.
- `HistoriaRedeSocial.cs`: o timer de "muito tempo sem foco" também para com o celular aberto (igual ao de "parado") — ler o feed não rende cobrança. Conferido: 7 s com o celular aberto → timer em 0 e nenhum post; ao fechar, o post saiu.

**Balões de fala — preview antes de implementar (skill `preview-design`):** montados em Play Mode (descartado) e capturados no mesmo enquadramento: "antes" × variante A (balão claro de HQ) × variante B (painel escuro sóbrio igual à HUD). O usuário escolheu a **B**.
- `Assets/Scripts/BalaoDeFala.cs` reescrito: o visual é montado em RUNTIME como Canvas world-space filho — `Painel` (UISprite fatiado, `pixelsPerUnitMultiplier` 0.3, cor 0.07/0.09/0.11, `Shadow` + `Outline` 14% branco, `VerticalLayoutGroup` + `ContentSizeFitter`), `Nome` (24 px, negrito, maiúsculas, verde-água 0.25/1/0.8, espaçamento 4), `Texto` (36 px, branco, 400 px de largura via `LayoutElement`), `Rabo` (quadrado 30 px girado 45°). 400 px = 1 m; de longe cresce até 1,5× (a partir de 3,5 m). Sempre de frente total pra câmera (`rotation = cam.rotation`) — acabou o texto espelhado. Campos novos: `quemFala`, `deslocamentoFrente`, `spriteFundo`, cores/tamanhos com nomes novos (não herdam os 0,7/1,4 serializados do TMP 3D). Se o objeto ainda tiver o `TextMeshPro` 3D antigo, ele é desligado no `Awake`.
- `Assets/Editor/PersonagensHospitalBuilder.cs`: `CriarBalao(pai, quem, texto, altura, frente)` cria só o `BalaoDeFala` (sem TMP 3D) e preenche `spriteFundo` com `UI/Skin/UISprite.psd` (em runtime não dá pra carregar o sprite embutido). Enfermeira: "Enf. Rita", altura 1,95, 0,6 m pra frente (o balão entrava na parede Norte). Pacientes: "Paciente", alturas 1,2 (sentado) e 1,85 (em pé) — no preview os dois balões encostavam.
- Aplicado na cena via `eval_file` (apagar `SalaDoHospital/Personagens` + `PersonagensHospitalBuilder.Popular(sala, 3f)`, sem reconstruir a sala nem mexer no player) e `save_scene`. Conferido em Play: igual ao preview B de longe (~5 m) e de perto, de lado, no ângulo que antes cortava o balão na parede.

**Testes em Play que faltavam (todos OK, via `eval_file`, com a ocupação zerada por reflection só no teste):** aproximação de vaso → Dona Neusa; 1 clique numa caixa d'água e ir embora → João "largou aberta"; muito tempo sem foco → João/Enf. Rita/Ana Paula (com `{restantes}` certo no card); duas garrafas em < 20 s → Bruna (1ª e 2ª fala) + combo; parado → Ana Paula e depois Carlos; `OnPulou` 4× → Ana Paula; ocupação abaixo de 40% → UBS "caiu para 38%"; vitória → falas de cada tipo ("primeiro", seguintes, "último"), epílogo de vitória com as falas "limpo" e a tela de vitória só depois dele; reinício (mesma chamada do R) → abertura pulada, `timeScale` 1, controles ativos. Console: só o erro antigo do `Default-Particle.psd` (`Interactable.cs:303`).

**Observação sobre a cena atual:** ela tem **13 focos e nenhuma piscina** (nenhum `PiscinaInteractable`), então a Ana Paula nunca posta sobre piscina e o epílogo de vitória tem 4 falas. Se a piscina voltar pra cena, a fala dela entra sozinha.

**Observações para futuras sessões de IA:**
- Para o Play avançar com o Editor sem foco, `unity command set_autotick --enable true` NÃO bastou nesta sessão (avançava alguns frames por comando e travava). O que funcionou foi `Application.runInBackground = true` no Play e, no fim, `PlayerSettings.runInBackground = false` (conferido no disco: `runInBackground: 0`). Chamar `set_autotick` antes do `editor_play` não adianta: o reload de domínio do Play desliga.
- Durante testes, `ControladorJogador.ControleAtivo = false`: o Input System do Editor recebe teclado/mouse mesmo sem foco, e o jogador andava/girava sozinho entre as capturas (o usuário digitando em outra janela).
- `unity command screenshot --view game` grava em `Temp/pipeline-screenshots/` (fora de `Assets`) e captura canvases world-space (não pega Screen Space Overlay).
- No PowerShell, não nomear função como `R` (é alias do histórico).

### 2026-09-26 — Menu de pausa (Esc) com Configurações, Reiniciar do zero e Sair

**Objetivo:** o usuário pediu um menu ao apertar Esc com um botão de configurações (volume, sensibilidade, FOV "e etc."), um que reinicia o jogo do zero e um para fechar. Preview feito antes com a skill `preview-design` (menu, configurações e confirmação montados em Play Mode e capturados); o usuário aprovou com um pedido: **"os botões de dentro do menu com um padding um pouco menor do que o resto"** — interpretado como: texto dos cartões a 44 px da borda (`PaddingConteudo`), botões e linhas de configuração a 28 px (`PaddingBotoes`), então os botões ficam mais largos que a coluna de texto e o texto deles fica alinhado ao título. As duas constantes estão no topo do `MenuPausaBuilder.cs`.

**Arquivos novos:**
- `Assets/Scripts/MenuPausa.cs` — no Canvas `MenuPausaCanvas`. Esc (ou Start no controle) abre/fecha. Abrir: `Time.timeScale = 0`, `ControladorJogador.ControleAtivo = false`, `Interactor.Bloqueado = true`, `CelularController` desligado, cursor solto; guarda o estado anterior de cada um e restaura ao fechar (o cursor sempre volta travado). A restauração é feita no `LateUpdate` do mesmo frame, para o clique/Espaço que fechou o menu não virar clique num foco nem pulo. Não abre com `CartelasNarrativas.Tocando` (lá o Esc pula a história) e fecha sozinho se o epílogo começar por baixo. Com o celular levantado, o Esc primeiro guarda o celular. Telas: Menu (rótulo muda: "UBS VILA NOVA · RECEPÇÃO" antes de sair do hospital, "MISSÃO EM ANDAMENTO", "MISSÃO ENCERRADA"; status com focos eliminados/total e ocupação colorida), Configurações e Confirmação (reiniciar/sair; abre por cima do menu, com "Cancelar" selecionado). Esc / B = voltar uma tela. `GarantirSelecao()` mantém sempre um item da tela atual selecionado (clicar no vazio desmarcaria tudo e o controle ficaria perdido). `AjustarAoTamanhoDaTela()` encolhe os cartões se a janela for baixa. Reiniciar do zero: `ConfiguracoesJogo.Salvar()`, `HistoriaDoJogo.EsquecerAbertura()`, `timeScale = 1`, `LoadScene`. Sair: `Application.Quit()` (no Editor, sai do Play).
- `Assets/Scripts/ConfiguracoesJogo.cs` — classe estática + enum `CanalSom`. Valores em PlayerPrefs (`config.*`), carregados em `RuntimeInitializeOnLoadMethod(BeforeSceneLoad)` e reaplicados ao player/câmera em todo `SceneManager.sceneLoaded` (inclusive no reinício). Padrões: volume geral 100%, música 80%, efeitos 100%, sensibilidade 1,0× (0,1–3,0), FOV 60° (60–100), inverter Y desligado, balanço ligado, qualidade = a do projeto. Volume geral → `AudioListener.volume`; tela cheia → `Screen.fullScreenMode` (sem PlayerPrefs, o Unity já guarda). Evento `Mudou`. `Salvar()` é chamado ao sair das Configurações e ao fechar o menu.
- `Assets/Scripts/CanalDeAudio.cs` — põe uma `AudioSource` no canal Música ou Efeitos: volume = volume do Inspector × volume do canal. **Não foi usado AudioMixer**: o Unity não tem API pública para criar mixer/grupos por script; o componente resolve sem asset novo. Toda `AudioSource` nova deve ganhar um (trilha → Música; zumbido, notificação, sons de foco → Efeitos).
- `Assets/Scripts/ItemDeMenu.cs` — visual de cada item: passar o mouse SELECIONA o alvo (mouse e controle usam o mesmo destaque), destaque = fundo mais claro + barrinha de acento; se o alvo for `Toggle`, desenha a chave liga/desliga (bolinha, cor da pista, "Ligado/Desligado"). Funciona com `timeScale = 0`.
- `Assets/Editor/MenuPausaBuilder.cs` — menu **Dengue > Criar Menu de Pausa**. Reconstrói do zero a cada execução (apaga o `MenuPausaCanvas` antigo). Canvas Overlay, `sortingOrder 40` (acima das cartelas, 30), `CanvasScaler` 1920×1080 / 0,5, `CanvasGroup`. Sliders com `wholeNumbers` (volume de 5 em 5%, sensibilidade de 0,1 em 0,1, FOV de 5 em 5°); toggles na própria linha (clicar em qualquer lugar da linha liga/desliga); "Qualidade gráfica" = Slider sem trilho na linha (esquerda/direita no controle) + setas clicáveis; navegação explícita (cima/baixo; na confirmação, esquerda/direita). Cria `EventSystem` + `InputSystemUIInputModule` se faltar (troca `StandaloneInputModule`, que não funciona com só o Input System novo) e põe `CanalDeAudio` (Efeitos) na `AudioSource` do `CelularHUD`. Cores: botão de destaque normal um pouco mais escuro (`AccentEscuro`/`PerigoEscuro`) e cor cheia quando selecionado.

**Arquivos alterados:**
- `EntradaJogador.cs` — `MultiplicadorSensibilidade` (vale para mouse E controle) e propriedade `InverterY`.
- `CameraJogador.cs` — `FovBase` (muda na hora, inclusive pausado) e `BalancoAtivo` (zera o peso do head bob/inclinação).
- `HistoriaDoJogo.cs` — `public static EsquecerAbertura()`: o "Reiniciar do zero" toca a abertura de novo (o R do fim de jogo continua pulando).

**Aplicado na cena (via CLI, salva):** menu `Dengue > Criar Menu de Pausa` rodado; criou `EventSystem`, `MenuPausaCanvas` e o `CanalDeAudio` no `CelularHUD`. Conferido: as 39 referências do `MenuPausa` ligadas e 17 `ItemDeMenu`.

**Verificado em Play Mode:** abrir → `timeScale 0`, controle/interação/celular travados, cursor solto, "Continuar" selecionado; Configurações → FOV 75 aplicado na câmera na hora, sensibilidade 1,4× e Y invertido no `EntradaJogador`, textos "75°"/"1,4×", chaves com estado certo, linha do FOV destacada; Voltar → menu com "Configurações" selecionado; Reiniciar → confirmação com "Cancelar" selecionado; Cancelar → menu com "Reiniciar do zero" selecionado; Continuar → `timeScale 1` na hora e controles/cursor restaurados no `LateUpdate`; PlayerPrefs gravados; Reiniciar + Confirmar → cena recarregada com a abertura tocando de novo e as configurações reaplicadas ao player/câmera novos. As preferências de teste foram apagadas no fim (`config.*`). Capturas do menu, das configurações e da confirmação conferidas (canvas trocado para Screen Space Camera só no Play, para aparecer no screenshot).

**NÃO verificado:** a tecla Esc/Start de verdade. Com o Editor sem foco o Input System descartou os eventos de teclado sintéticos (nem as cartelas os receberam), mesmo com `InputSystem.settings` em `IgnoreFocus`/`AllDeviceInputAlwaysGoesToGameView` (restaurados depois). O código usa o mesmo padrão já em uso (`Keyboard.current.escapeKey.wasPressedThisFrame`). Conferir numa sessão normal: Esc abre/fecha, Esc nas Configurações volta, Esc nas cartelas só pula a história.

**Descoberta — qualidade gráfica:** fora do Play, `QualitySettings.count` = 2 (Mobile, PC); no Play/build de PC = 1, porque o nível "Mobile" está excluído do Standalone. Por isso o `MenuPausa.OcultarQualidadeSeUnica()` decide em runtime: com um nível só, a linha "Qualidade gráfica" some, as de baixo sobem, o cartão encolhe (880 → 826) e a navegação é religada. Hoje ela não aparece no jogo; para aparecer, habilitar mais de um nível para PC em Project Settings > Quality ("Mobile" vira "Baixa" e "PC" vira "Alta" no menu).

**Observações para futuras sessões de IA:**
- Neste Unity (6000.5), `FindFirstObjectByType` e `FindObjectsByType(..., FindObjectsSortMode)` estão obsoletos (CS0618): usar `FindAnyObjectByType` e `FindObjectsByType<T>()` / `FindObjectsByType<T>(FindObjectsInactive)` em código novo.
- `unity command menu` recebe o caminho em `--path "Dengue/..."` (JSON `{"path":...}` só lista os itens). Logo depois de um `recompile` o comando pode falhar com "Network error": esperar `editor_status` com `domainReloadInProgress:false`.
- Com os frames parados (Editor sem foco), `UnityEditor.EditorApplication.isPaused = true; EditorApplication.Step();` via `eval_file` avança um frame por chamada — foi o que fez o `LoadScene` do reinício acontecer. Lembrar de `isPaused = false` no fim.
- Atalho de teste que pula a abertura parando a corrotina (`StopAllCoroutines` nas cartelas) deixa `CartelasNarrativas.Tocando` preso em `true` — o menu então fecha sozinho. Zerar o backing field `<Tocando>k__BackingField` junto.
- `Camera.main.Render()` chamado via `eval` para capturar com os frames parados gera erros do URP "attachment dimensions do not match RenderPass" no console — são do teste, não do jogo.
- O canvas de UI em Screen Space Camera passa pelo HDR/tonemapping: véus translúcidos parecem bem mais claros na captura do que no Overlay real.
- Python no Git Bash deste Windows lê heredoc com a codificação errada (acentos): escrever o script com a ferramenta Write e rodar com `python -X utf8`.

---

### 2026-09-26 — Troca de "UBS" por "UPA" em todo o jogo

**Objetivo:** a pedido do usuário, todo texto do jogo que dizia "UBS" passa a dizer "UPA".

**Arquivos alterados:**
- `Assets/Scripts/HistoriaDoJogo.cs` — cartelas narrativas ("A UPA do bairro…", "A ocupação da UPA…", "A UPA Vila Nova fechou…").
- `Assets/Scripts/HistoriaRedeSocial.cs` — perfis `UPA Vila Nova` / `@upa.vilanova` / `@enf.rita.upa`, textos dos posts; constantes renomeadas `Ubs`/`UbsUsuario` → `Upa`/`UpaUsuario`.
- `Assets/Scripts/RedeSocial.cs` — posts padrão do perfil `UPA Vila Nova`.
- `Assets/Scripts/MenuPausa.cs` — rótulo "UPA VILA NOVA · RECEPÇÃO".
- `Assets/Editor/PersonagensHospitalBuilder.cs` — comentário (`@enf.rita.upa`).
- `Assets/Scenes/SampleScene.unity` — valores serializados dos mesmos textos (substituição direta no YAML, 31 ocorrências).

**Observações técnicas:** substituição por palavra inteira (`UBS`→`UPA`, `ubs`→`upa`). O gênero gramatical se mantém ("da UPA"). As menções a "UBS" em entradas antigas deste histórico foram mantidas como registro.

---

### 2026-09-26 — Hierarquia da cena organizada em grupos

**Objetivo:** o usuário pediu ajuda com a hierarquia do jogo no Unity. A `SampleScene` tinha 53 objetos soltos na raiz (prédios, focos, HUDs, sistemas, câmera e luz misturados). Escolhas do usuário: organizar em **pais vazios recolhíveis** (sem limpar sobras) e **ajustar os builders** para recriarem os objetos dentro do mesmo grupo.

**Hierarquia resultante (8 grupos na raiz, todos na origem, rotação 0, escala 1):**
```
--- SISTEMAS ---            FluxoDeJogo, Historia
--- JOGADOR ---             Main Camera, Player   (câmera continua irmã do Player, não filha)
--- ILUMINACAO ---          Directional Light, Global Volume
--- UI ---                  DengueHUD, CelularHUD, FluxoDeJogoCanvas, CartelasCanvas, EventSystem, MenuPausaCanvas
--- FOCOS ---               pneu ×4, Vaso de Flor ×3, Caixa d'Agua ×2
--- BAIRRO ---
    Terrenos                Terreno Baldio, Terrain, Cenario Terreno Baldio
    Ruas e Praca            Quadra, Praca
    Construcoes             Hospital, Coffe house, House_01 ×3, Building01-03, Market, School, Police_Station,
                            Workshop, NaturePark Hovels, Fire_Station, building, building 2
    Vegetacao               LPW_Tree_B1_9m_01, NaturePark, NatureParkAutumn
    Objetos de Rua          Car_03, Trash_01, Escada de Mao ×2
    Cercas                  cerca
    Limites do Mapa         FimDoMundo (paredes invisíveis)
--- HOSPITAL (INTERIOR) --- SalaDoHospital
--- TESTES ---              TesteDoutor (Lowpoly_Doctor do pack BackRock parado em 0,0,0; não documentado — decidir depois se fica)
```
Nomes sem acento, no padrão da cena (`Praca`, `Cenario`, `Mobilia`). Os focos que já estavam aninhados (ex.: garrafas dentro do `Cenario Terreno Baldio`) não foram mexidos: em Play continuam 13 `Interactable`.

**Arquivo novo:**
- `Assets/Editor/HierarquiaOrganizador.cs` — menu **Dengue > Organizar Hierarquia**. Só mexe nos objetos soltos na **raiz** (ignora nomes começando com `--- `). Classifica primeiro por componente: `Interactable` → FOCOS; `Canvas`/`EventSystem` → UI; `ControladorJogador`/`CameraJogador` → JOGADOR; `Light`/`Volume` → ILUMINACAO; `FluxoDeJogo`/`HistoriaDoJogo` → SISTEMAS; `SaidaDoHospital` em algum filho → HOSPITAL; `Terrain` → BAIRRO/Terrenos. Depois usa a tabela `Prefixos` (primeiro prefixo que bater; `NaturePark Hovels` antes de `NaturePark`). O que não se encaixa **fica na raiz** e sai listado no Console. `Undo.SetTransformParent(..., worldPositionStays: true)`, um passo só de Undo, relatório só por `Debug.Log` (sem `DisplayDialog`, para não travar o CLI). Helper público `RecolocarNoLugar(novo, pai, indice)` para os builders.

**Arquivos alterados (builders que apagam e recriam):** guardam `parent` + `GetSiblingIndex()` do objeto antigo antes de `Undo.DestroyObjectImmediate` e chamam `HierarquiaOrganizador.RecolocarNoLugar` no novo:
- `Assets/Editor/DengueHUDBuilder.cs`, `CelularBuilder.cs`, `MenuPausaBuilder.cs`, `QuartoHospitalBuilder.cs` (na sala, depois de `root.transform.position = CentroDaSala`).
- `Assets/Editor/TerrenoBaldioBuilder.cs` — `Popular` e `Limpar` procuravam `Cenario Terreno Baldio` só na raiz (`GetRootGameObjects`); dentro de um grupo, o `Popular` criaria um cenário **duplicado**. Agora usam `AcharCenarios(Scene)`, que procura em qualquer nível, e o `Popular` recria no mesmo grupo.
- Os builders do tipo "acha ou cria" (`HistoriaBuilder`, `FluxoDeJogoHUDBuilder`, `PersonagensHospitalBuilder`) e os de prefab de foco não precisaram de mudança: `GameObject.Find(nome)` acha objetos ativos em qualquer nível.

**Aplicado na cena (via CLI, salva):** menu rodado → 53 objetos movidos, 0 na raiz sem grupo, 15 GameObjects de grupo criados (8 grupos + 7 subgrupos do BAIRRO).

**Verificado:**
- Snapshot de pose no mundo + `activeSelf`/`activeInHierarchy` dos 1871 transforms antes e depois: nenhum sumiu, nenhum mudou de estado ativo, maior diferença de pose 1,5e-5 (arredondamento de float ao reparentar).
- Segunda execução do menu: "0 objetos movidos", sem grupo duplicado.
- `Dengue > Criar Menu de Pausa` rodado de teste: o `MenuPausaCanvas` novo nasceu dentro de `--- UI ---` no mesmo índice (5), uma cópia só, e o builder não gravou nenhum arquivo. A cena foi reaberta do disco para descartar o teste.
- `TerrenoBaldioBuilder.AcharCenarios` via reflection: acha exatamente 1 cenário, em `--- BAIRRO ---/Terrenos`. `Popular` não foi rodado (regrava assets e refaz o espalhamento).
- Play Mode: nenhum erro novo no console; `FluxoDeJogo` achado (ocupação 0,7), Player e `Camera.main` na sala do hospital, os canvases ativos, 13 focos. `PlayerSettings.runInBackground` voltou para `false`; `git status` idêntico ao de antes do teste.
- HUD, Celular e Sala **não** foram rodados de novo: o da HUD abre diálogo (trava o CLI) e o da Sala gera materiais duplicados (`Lit (clinica) 1.mat`, `2.mat`...). A validação deles foi pela compilação e pela revisão do trecho, que é o mesmo do Menu de Pausa.

**Observações para futuras sessões de IA:**
- Soltou foco/prédio/UI novo na raiz? Rodar **Dengue > Organizar Hierarquia**. Se aparecer "Ficaram na raiz" no Console, adicionar um prefixo em `HierarquiaOrganizador.Prefixos`.
- Builder novo que apaga e recria um objeto de cena: guardar pai + índice do antigo e chamar `HierarquiaOrganizador.RecolocarNoLugar`. Nunca procurar objetos só com `GetRootGameObjects()` — a raiz agora só tem os grupos.
- Não marcar os grupos com a tag `EditorOnly`: isso tiraria os filhos da build.
- No `eval` do CLI, avisos de API obsoleta viram erro de compilação: usar `GetEntityId()` em vez de `GetInstanceID()`.


### 2026-09-26 — Preview dos novos focos: Ralo e Bromélia (Blender)

**Objetivo:** Criar dois novos tipos de foco (ralo destampado e bromélia com água acumulada), modelados no Blender por script. A pedido do usuário, primeiro só o **preview** (renders); nada foi importado no Unity ainda.

**Arquivos criados:**
- `Ferramentas/Blender/gerar_focos.py` — gera os dois modelos low poly e exporta `Ralo.fbx`, `Bromelia.fbx`, `Focos.blend` e as prévias (`previa_ralo`, `previa_ralo_fechado`, `previa_bromelia`, `previa_bromelia_seca`, `previa_bromelia_cima`, `previa_juntos`). Uso: `blender -b --factory-startup --python gerar_focos.py -- <pasta_saida> [--sem-render]`.

**Estrutura dos modelos (nomes que os scripts do Unity vão usar):**
- `Ralo` → `Caixa` (moldura 0,42 m + poço de 12 cm), `Tampa` (grelha, começa tirada e encostada ao lado), `Agua` (lâmina 3 cm abaixo da boca), `Tela` (tela verde de proteção, aparece no fim).
- `Bromelia` → `Vaso` (terracota), `Folhas` (3 anéis de folhas em calha, pontas vermelhas), `Flor`, `Agua` com filhos `Agua_Centro` e `Agua_1..4` (uma poça por clique).

**Próximo passo (após aprovação):** exportar FBX para `Assets/Meus/Assets Gerados/Focos/`, criar `RaloInteractable`/`BromeliaInteractable` e o builder `Dengue > Criar Ralo / Criar Bromélia`.
### 2026-09-26 — Integração dos focos Ralo e Bromélia

**Objetivo:** Colocar no projeto os dois focos aprovados no preview (ralo destampado e bromélia), com modelo do Blender, script de interação e prefab gerado por menu.

**Arquivos criados:**
- `Assets/Scripts/RaloInteractable.cs` — herda `Interactable`. Cada clique leva a grelha (`tampa`) de `posAberta/rotAberta` para `posFechada/rotFechada`, com arco. No fim a `agua` some e a `tela` aparece; o ralo fica na cena (`removeOnComplete` opcional). **Não** herda `CaixaDaguaInteractable` de propósito: `FocoListaHUD` e `HistoriaRedeSocial` classificam o foco por `is`, e o ralo seria contado como caixa d'água.
- `Assets/Scripts/BromeliaInteractable.cs` — herda `Interactable`. Array `pocas` na ordem de secar; cada clique seca `ceil(n × cliques / total)` poças (encolhem em `duracaoSecar` e desativam). A planta fica na cena.
- `Assets/Editor/FocosBlenderBuilder.cs` — menus **Dengue > Criar Ralo** e **Dengue > Criar Bromélia**. Configura o import do FBX (`bakeAxisConversion`, `preserveHierarchy`, sem animação/collider), cria materiais URP Lit `Focos/Materiais/Foco_*.mat` com a paleta do Blender (a água usa `Assets/Meus/agua do pneu.mat`), monta o prefab com o Interactable, `FocoHUDInfo` e um `BoxCollider` na raiz cobrindo todos os renderers. A pose aberta da grelha é lida do FBX; a fechada é `(0, 0.034, 0)` sem rotação.
- `Assets/Meus/Assets Gerados/Focos/` — `Ralo.fbx`, `Bromelia.fbx`, `Ralo.prefab`, `Bromelia.prefab`, `Materiais/Foco_*.mat`.

**Arquivos alterados:**
- `Ferramentas/Blender/gerar_focos.py`:
  - Export com `bake_space_transform=True`: o Unity funde o nó raiz do FBX e perdia a correção de eixo (o modelo entrava deitado).
  - As poças da bromélia agora são filhas diretas da raiz, com pivô no centro de cada uma. Dentro de um Empty, o export as girava 90°.
  - A água do ralo subiu para 1 cm abaixo da borda (`ALT - 0.01`) e agora ocupa toda a boca: as calçadas do jogo não têm buraco, e o poço abaixo de y=0 ficava escondido.

**Configuração dos prefabs:**
- Ralo: 3 cliques, `punchScale` 0.04. HUD: "o ralo" / "Clique para tampar {nome}" / dica sobre manter fechado ou com tela e limpar toda semana.
- Bromélia: 5 cliques (1 por poça: Agua_1..4, depois Agua_Centro), `punchScale` 0.06. HUD: "a bromélia" / "Clique para esvaziar {nome}" / dica sobre esvaziar com jato d'água toda semana.

**Verificado:**
- Menus rodados pelo CLI; os prefabs foram renderizados numa *preview scene* do Unity (sem tocar na SampleScene), no estado inicial e no final.
- Play Mode com instâncias temporárias, destruídas depois; a cena não foi salva:
  - Ralo: 3/3, a grelha assenta em y=0.034, a água some e a tela aparece.
  - Bromélia: cada clique seca uma poça, o centro por último.
- `PlayerSettings.runInBackground` foi ligado só durante o teste e voltou a `false`.

**Observações para futuras sessões de IA:**
- Os prefabs ainda **não** foram colocados na cena. A lista de focos (`FocoListaHUD`) e a rede social usam `FocoHUDInfo.nome` ("Ralo", "Bromélia"). `HistoriaRedeSocial` classifica esses focos como `TipoFoco.Nenhum`, então nenhum personagem da rede social comenta sobre eles ainda. Para isso, é preciso adicionar tipos ao enum `TipoFoco` e um personagem.
- Ao testar pelo CLI: o jogo abre com `Time.timeScale = 0` (cartelas da `HistoriaDoJogo`), e o Editor em segundo plano quase não avança frames. Coroutines e o `Start()` de objetos instanciados atrasam. Para testar a lógica, zere as durações das animações por reflection.
- Regerar um modelo: rodar `gerar_focos.py`, copiar o FBX para `Assets/Meus/Assets Gerados/Focos/` e rodar o menu de novo (o GUID do prefab é mantido).

### 2026-09-26 — Personagens da rede social para Ralo e Bromélia

**Objetivo:** Os focos novos eram classificados como `TipoFoco.Nenhum`. Por isso ninguém comentava sobre eles na rede social (chegar perto, eliminar, último do tipo, largar pela metade) e eles ficavam fora do epílogo. Agora cada um tem tipo e personagem "dono".

**Arquivos alterados:**
- `Assets/Scripts/HistoriaRedeSocial.cs`:
  - Enum `TipoFoco` ganhou `Ralo` e `Bromelia` **no fim**, porque o enum é salvo como número na cena e os 5 personagens antigos precisam manter o tipo.
  - `TipoDoFoco()` reconhece `RaloInteractable` e `BromeliaInteractable`.
  - `PersonagensPadrao()` ganhou **Seu Osvaldo** (@osvaldo.bar, Ralo) e **Lúcia Andrade** (@lu.plantas, Bromelia), com todas as falas e os dois epílogos.
  - Novo `public int CompletarPersonagens()`: acrescenta o personagem padrão de cada tipo que ainda não tem dono na lista, sem mexer nos existentes. `PreencherPadroes(false)` (no `Awake`) chama esse método quando a lista não está vazia, então o jogo já funciona em Play mesmo com a cena antiga.
- `Assets/Editor/HistoriaBuilder.cs` — `Dengue > Configurar Historia` chama `CompletarPersonagens()` (com Undo e SetDirty), para os personagens novos aparecerem no Inspector do objeto "Historia".
- Seção **4. Personagens** deste arquivo: os dois personagens novos.

**Aplicado na cena (via CLI, NÃO salva):** o menu foi rodado e a lista passou de 5 para 7 personagens. A cena já tinha alterações não salvas do usuário e ficou para ele salvar.

**Verificado:**
- Compilação OK.
- Menu rodado 2×: 7 personagens nas duas vezes, sem duplicar; os textos dos 5 originais estão intactos.
- `TipoDoFoco` por reflection: `Ralo.prefab` → Ralo, `Bromelia.prefab` → Bromelia, Caixa d'Água da cena → CaixaDagua (sem regressão).
- `CompletarPersonagens()` devolve 0 com a lista completa.

**Observações para futuras sessões de IA:**
- Novo tipo de foco na rede social: acrescentar no **fim** do enum `TipoFoco`, um `if (foco is X)` em `TipoDoFoco()` e um personagem em `PersonagensPadrao()`. Depois rodar `Dengue > Configurar Historia`.
- Como o jogo abre com `timeScale = 0`, `TipoDoFoco`, que é estático, dá para testar em modo Edição por reflection, sem Play Mode.

### 2026-09-26 — Dica "Pressione [Shift] para correr"

**Objetivo:** Mostrar na lateral esquerda da tela a dica "Pressione **[Shift]** para correr" quando o jogador passa um tempo sem correr. O visual foi aprovado pelo usuário com preview no Unity: caixa preta a 60% de opacidade, barrinha branca à esquerda e texto branco com "[Shift]" em negrito.

**Arquivos alterados:**
- `Assets/Scripts/DicaCorrer.cs` (novo):
  - Se instala sozinho via `RuntimeInitializeOnLoadMethod(AfterSceneLoad)` no objeto que tem o `ControladorJogador`. Não é preciso mexer na cena nem num builder.
  - Cria o próprio Canvas (`DicaCorrer_Canvas`, Overlay, `sortingOrder` 50, referência 1920×1080). A caixa tem 380×70, fica ancorada no meio da borda esquerda e deslocada 30 px.
  - Soma o tempo sem `EntradaJogador.CorrerSegurado`. Ao passar de `tempoSemCorrer` (20 s), a dica aparece com fade (`duracaoFade` 0,4 s, com tempo *unscaled*). Ao segurar Shift, o contador zera e a dica some.
  - Fica oculta e não conta tempo com `Time.timeScale == 0` (pausa e cartelas), `ControleAtivo == false` ou `EntradaJogador.Habilitado == false`.

**Verificado:** A compilação passou. Em Play Mode, a dica fica oculta durante a introdução, com o controle desligado. Com o controle ativo e o contador forçado, `alpha` foi para 1. A cena não foi salva.

**Observações para futuras sessões de IA:** Os valores são ajustáveis no Inspector do componente em Play. Como ele é adicionado em tempo de execução, para mudar os padrões é preciso editar os campos no script. A dica sempre diz "[Shift]", inclusive quando se joga com controle.

### 2026-09-26 — Menu de pausa (Esc) no tema branco

**Objetivo:** O menu de pausa passa do tema escuro com verde-água para o tema branco do jogo. O usuário comparou prévias no Unity e escolheu a variante "B refinada" com o **véu escuro**: cartão branco opaco, texto grafite e botão principal grafite. O vermelho aparece só em "Sair do jogo" e na confirmação de reinício.

**Arquivos alterados:**
- `Assets/Editor/MenuPausaBuilder.cs`:
  - Paleta nova (l. ~47–72): `CorFundo` branco, `CorTexto`/`CorTextoFraco`/`CorTextoDica` em cinzas **opacos** (não usam mais transparência), `CorRotulo`, `Primaria`/`PrimariaHover` (grafite, substituem `Accent*`), `Perigo*` e `Neutro`/`NeutroDestaque` sólidos. Também `CorTrilho`, `CorChaveDesligada`, `CorCaixa` e `CorVeuConfirmar`.
  - Contornos e sombras escuros, sombra do cartão mais longa (-10).
  - Botões secundários com fundo cinza-claro sólido. "Sair do jogo" tem fundo vermelho a 8% e "Reiniciar" (confirmação) é vermelho sólido com texto branco.
  - Cruz grafite com traço branco; sliders, chaves e setas em grafite.
  - Grava no `MenuPausa` as cores `corOcupacaoBaixa`/`corOcupacaoAlta`/`corFocosEliminados`, e no `ItemDeMenu` das chaves as cores `pistaLigada`/`pistaDesligada`/`bolaLigada`/`bolaDesligada`.
- `Assets/Scripts/MenuPausa.cs`:
  - Ocupação vai de grafite (baixa) a vermelho (alta).
  - Novo `corFocosEliminados` no lugar do `#FFFFFFEE` fixo no texto de status.
- `Assets/Scripts/ItemDeMenu.cs` — os valores padrão das cores (destaque e chave) seguem o tema branco.

**Aplicado na cena (via CLI, NÃO salva):** `Dengue > Criar Menu de Pausa` rodado. O `MenuPausaCanvas` foi reconstruído e a cena ficou suja para o usuário salvar.

**Verificado:**
- Compilação OK.
- Em Play Mode foram capturados o menu principal, as Configurações (slider selecionado) e a confirmação. Os três conferem com a prévia aprovada e não sobrou verde-água.

**Observações para futuras sessões de IA:**
- `MenuPausa.Abrir()` é recusado enquanto as `CartelasNarrativas` tocam (`PodeAbrir`). Para capturar o menu pelo CLI, force `CanvasGroup.alpha = 1` e `interactable = true`. Sem `interactable`, os Selectables aparecem na cor *disabled*.
- A captura de tela só pega o canvas em ScreenSpaceCamera. Desligue `renderPostProcessing` da câmera para as cores da UI saírem fiéis (o Overlay real não passa pelo pós-processamento).
- A `FluxoDeJogoHUD` ainda usa verde-água na barra de ocupação (`corOcupacaoBaixa`). Ela não fez parte desta mudança.

### 2026-09-26 — Menu inicial "Foco Zero" (cena MenuInicial) + "Menu inicial" no pause

**Objetivo:** o usuário pediu um menu inicial. Decisões dele, tomadas com um preview renderizado no Unity (skill `preview-design`, Play Mode descartado, capturas em 16:9):
- **Cena separada** `Assets/Scenes/MenuInicial.unity`, índice 0 do build. A `SampleScene` passa a ser o índice 1.
- Nome do jogo **Foco Zero**. O título é "FOCO" em branco e "ZERO" em vermelho (`#E04848`).
- Estilo **cinematográfico**:
  - Título à esquerda sobre um degradê escuro, e botões só de texto.
  - A câmera **alterna 3 tomadas** (rua do hospital, praça e sobrevoo), de ~20 s cada, com fade entre elas.
  - **Luz de fim de tarde**, só no menu.
- Botões: Jogar, Como jogar, Configurações e Sair. Sem créditos.
- O pause ganhou o botão **"Menu inicial"**, com confirmação.

**Arquivos novos:**
- `Assets/Scripts/MenuInicial.cs`
  - Telas: Título, Como jogar e Configurações. Esc ou B volta uma tela.
  - Pré-carrega a `SampleScene` com `LoadSceneAsync` e `allowSceneActivation = false` enquanto o jogador está no menu.
  - "Jogar": fade para preto de 0,5 s e ativa a cena. A `SampleScene` nasce com a tela preta das cartelas, então a troca é de preto para preto. Se a carga não tiver terminado, aparece "Carregando… N%".
  - "Sair" sai direto, sem confirmação. No Editor, sai do Play.
  - `Awake` põe `timeScale = 1` e solta o cursor, porque a volta pelo pause chega com ambos travados.
  - Constantes `NomeCena` e `NomeCenaJogo`.
- `Assets/Scripts/CameraMenu.cs`
  - Lista de tomadas `{posInicio, alvoInicio, posFim, alvoFim, duracao}`.
  - Travelling com *ease* leve e parallax pelo mouse de ±1,5°, em tempo *unscaled*.
  - Na troca de tomada, o `fadeTomada` (uma tela preta **atrás** do texto) escurece só o cenário.
  - Os caminhos aparecem na Scene view com a câmera selecionada.
- `Assets/Scripts/PainelConfiguracoes.cs`
  - A tela de Configurações saiu do `MenuPausa` sem mudar o comportamento.
  - É a mesma no pause e no menu inicial.
  - `Preparar()` pode ser chamado com a tela desativada. `Mostrar()`, `PrimeiroItem` e `BotaoVoltar` ficam expostos.
- `Assets/Editor/MenuInicialBuilder.cs`: menu **Dengue > Criar Menu Inicial**. Refaz a cena do zero a cada execução e **só lê** a `SampleScene`; se ela não estiver aberta, abre por trás e fecha sem salvar.
  - Copia todo o `--- BAIRRO ---` e o `--- FOCOS ---`.
  - Remove por *whitelist* tudo que não desenha: ficam Transform, MeshFilter, Renderer, LODGroup e Terrain. Objetos vazios ou inativos também saem.
  - As malhas embutidas na cena (as **cercas de ProBuilder**, 864 malhas, e uma de `Ruas e Praca`) viram 3 malhas combinadas por material, sem tangente e com a normal em Float16. Elas ficam em `Assets/Meus/Assets Gerados/Menu Inicial/CenarioMalhas.asset`, de ~20 MB.
  - Monta a luz de fim de tarde:
    - Sol a (16°, 285°), cor (1; 0,74; 0,50), intensidade 1,9.
    - Ambiente *flat* (0,42; 0,36; 0,40).
    - Neblina linear de 40 a 170 m, que também esconde o horizonte cinza do sobrevoo.
  - Monta a câmera (FOV 55, **sem pós-processamento**, como no preview), a UI e o `EventSystem`.
  - Gera `Degrade.png` e `DegradeBaixo.png`.
  - Confere o caminho das tomadas com `Physics.CheckSphere` e avisa no Console se a câmera atravessar algo.
  - Atualiza o Build Settings.
- `Assets/Scenes/MenuInicial.unity` (gerada, ~1,8 MB) e a pasta `Assets/Meus/Assets Gerados/Menu Inicial/`.

**Arquivos alterados:**
- `Assets/Scripts/MenuPausa.cs`
  - A lógica de configurações foi para o `PainelConfiguracoes`.
  - Botão `botaoMenuInicial` com a confirmação "Voltar ao menu inicial?". Ao confirmar: `HistoriaDoJogo.EsquecerAbertura()`, `timeScale = 1` e `LoadScene(MenuInicial.NomeCena)`.
  - Se a cena do menu não estiver no build, o botão some e o cartão encolhe.
- `Assets/Editor/MenuPausaBuilder.cs`
  - As peças de UI (cores, `Novo`, `Pos`, `Img`, `Txt`, `Botao`, `BotaoCentral`, `Cartao`, `Tecla`, `Linha*` etc.) passaram a `internal`, e entrou o `PrepararRecursos()`.
  - `MontarConfiguracoes(pai, rotulo, out tela, out painel)` virou `internal`.
  - Botão "Menu inicial" entre "Reiniciar do zero" e a divisória. O cartão cresceu de 640 para 712.
- `Assets/Scripts/ItemDeMenu.cs`: campos opcionais `texto`, `corTextoNormal` e `corTextoDestaque`. O texto dos botões do título acende quando selecionado.
- `ProjectSettings/EditorBuildSettings.asset`: MenuInicial (0) e SampleScene (1).
- Seção 2 (História) deste arquivo: o nome do jogo.

**Aplicado na cena:**
- `Dengue > Criar Menu de Pausa` rodado e `SampleScene` salva. Ela estava limpa antes.
- `Dengue > Criar Menu Inicial` rodado 3×. O GUID da cena se manteve e o Build Settings ficou sem duplicata.

**Verificado:**
- Compilação OK.
- Auditoria da cena `MenuInicial`: só Transform, MeshFilter, MeshRenderer, LODGroup e Terrain no cenário, 0 malhas embutidas ou nulas, e todas as referências de `MenuInicial`, `CameraMenu` e `PainelConfiguracoes` ligadas.
- Em Play a partir do menu (capturas em 16:9 conferidas com o preview):
  - Cursor livre, "Jogar" selecionado e `SampleScene` pré-carregada (0,90).
  - As 3 tomadas corretas. O fade de troca vai de 1 → 0 em 0,6 s no começo e de 0 → 1 nos últimos 0,6 s.
  - Como jogar → Voltar ("Como jogar" volta selecionado).
  - Configurações: FOV 75, e a linha "Qualidade gráfica" oculta (um nível só no PC).
  - Jogar: a `SampleScene` ativa com as cartelas tocando e o FOV 75 aplicado no `CameraJogador`.
  - Pause (5 botões) → Menu inicial → confirmação com "Cancelar" selecionado → Confirmar: volta ao menu com `timeScale` 1, cursor livre e pré-carga pronta.
  - Jogar de novo: a abertura toca outra vez.
  - Console sem erros novos.
- Limpeza: `PlayerSettings.runInBackground = false` e as PlayerPrefs `config.*` do teste apagadas.

**NÃO verificado:** as teclas de verdade (Esc/Enter/B), pelo mesmo motivo das entradas anteriores: com o Editor sem foco, o Input System descarta o teclado sintético. Os testes acionaram os botões por código.

**Observações para futuras sessões de IA:**
- **CUIDADO com ProBuilder ao copiar objetos por script:** `Object.Instantiate` de um objeto com `ProBuilderMesh` **compartilha a mesma Mesh** com o original. Destruir o `ProBuilderMesh` da cópia apaga essa malha (`OnDestroy` → `DestroyUnityMesh`), e as 864 cercas da `SampleScene` aberta ficaram sem malha **na memória**. A primeira versão do builder fez isso. Foi desfeito reabrindo a cena do disco; ela não estava suja e tinha sido salva antes.
  - O builder agora liga `preserveMeshAssetOnDestroy = true` (por reflexão) antes de limpar, e dá erro no Console se a `SampleScene` perder malhas.
  - Se isso aparecer: **não salve a SampleScene**, reabra-a do disco.
- Mudou o bairro? Rode **Dengue > Criar Menu Inicial** de novo. O `CenarioMalhas.asset` é apagado e recriado (GUID novo) a cada execução.
- Play direto na `SampleScene` continua funcionando sem menu. Para testar o fluxo completo, abra `MenuInicial.unity` e dê Play.
- Enquadramentos e luz: constantes no topo do `MenuInicialBuilder` (`Tomadas()`, `RotacaoSol`, `CorSol`...). Para ajustar só em teste, mexa no Inspector do `CameraMenu` em Play.
- `unity command capture_game_view '{"width":1920,"height":1080}'` devolve um PNG em base64 a 1280×720 (16:9), independente do tamanho da Game view (que está ultralarga no Editor do usuário). Serve para conferir o layout real da UI; o canvas precisa estar em ScreenSpaceCamera durante o teste.
- No `eval` do CLI, `Object` é ambíguo (`UnityEngine.Object` × `object`): escrever `UnityEngine.Object`.

### 2026-09-26 — AGENTS.md com o nome Foco Zero

**Objetivo:** a pedido do usuário, o resumo do `AGENTS.md` passa a usar o nome do jogo.

**Arquivos alterados:**
- `AGENTS.md`:
  - "Nome" passou a ser "Foco Zero — jogo educativo sobre dengue / Aedes aegypti".
  - A linha "Cena" virou "Cenas", com `MenuInicial.unity` (índice 0, tela de título) antes da `SampleScene.unity` (índice 1). Ela descrevia só a `SampleScene` e estava desatualizada desde o menu inicial.

### 2026-09-26 — Sons do jogo (todos gerados por script)

**Objetivo:** o usuário pediu para colocar sons no jogo, que estava mudo (o projeto não tinha nenhum arquivo de áudio). Decisões dele:
- Todos os sons são **gerados por script**, sem baixar nada.
- Entram todos os grupos: essenciais do GameDoc, jogador, ambiente/trilha e interface.
- O ambiente é de **cidade pequena e parada**: **sem carros**, porque nenhum passa no mapa.
- **Pouquíssimos sons de interface**: ficou um só, o clique.

**Gerador — `Ferramentas/Audio/gerar_sons.py` (novo):**
- Uso: `python -X utf8 Ferramentas/Audio/gerar_sons.py "Assets/Meus/Assets Gerados/Sons" [--so nome1,nome2] [--listar]`. Leva ~16 s.
- Só usa numpy e `wave` (a máquina não tem scipy).
- Filtros e ruído colorido por FFT, reverb por convolução com ruído que decai, e síntese modal para batidas (plástico, vidro, metal, sino).
- Cada som tem semente fixa (crc32 do nome), então regerar dá o mesmo arquivo. Sobrescrever o `.wav` mantém o `.meta`/GUID.
- Loops montados em linha do tempo circular, sem crossfade: tons com número inteiro de ciclos, e eventos que passam do fim voltam para o começo. A emenda de todos ficou dentro da variação normal entre amostras vizinhas (medido).
- Efeitos em 44,1 kHz mono. Loops longos (ambiente e trilha) em 32 kHz estéreo.
- **44 arquivos (~30 MB) em `Assets/Meus/Assets Gerados/Sons/`.** O `.gitattributes` já manda `*.wav` para o Git LFS.

**Os sons:**

| Grupo | Arquivos |
|---|---|
| Focos | `mosquito_zumbido_loop` (6 s: enxame de 5 mosquitos entre 470 e 640 Hz, com harmônicos e vibrato), `foco_agua_1-3` (bolhas + respingo), `foco_areia_1-2`, `foco_tampa_1-2`, `foco_garrafa_1-3` (vidro; o 3 é PET amassando), `foco_metal_1-2` (grelha do ralo), `foco_eliminado` (whoosh + Ré-Fá#-Lá subindo) |
| Hospital | `ocupacao_alerta` (bipe duplo), `ocupacao_desce` (duas notas subindo), `vitoria`, `derrota` |
| Jogador | `passo_rua_1-4`, `passo_piso_1-4`, `pulo`, `aterrissagem` |
| Celular / UI | `celular_notificacao`, `celular_abrir`, `celular_fechar`, `ui_clique` |
| Ambiente | `ambiente_bairro_loop` (45 s: vento, folhas, cigarras crescendo e sumindo), `ambiente_hospital_loop` (30 s: lâmpada de 120 Hz, ar-condicionado, murmúrio abafado, monitor a 72 bpm), `passaro_1-3` (bem-te-vi, pardal, assobio), `latido_1-2` |
| Trilha | 80 BPM, Ré menor, Dm–B♭–Gm–A: `trilha_base` (pad), `trilha_pulso` (batimento + baixo), `trilha_tensao` (ostinato + tique-taque), `trilha_esperanca` (marimba com eco). As 4 têm 24 s e exatamente 768.000 amostras. `trilha_menu` (32 s, pad de fim de tarde) |

**Arquivos novos (`Assets/Scripts/`):** cada componente cria as próprias `AudioSource` em runtime, sempre com `CanalDeAudio`. A cena só guarda os componentes e os clipes.
- `SomDosFocos.cs` (objeto `Sons`):
  - Cria um loop 3D de zumbido por foco em `Sons/Zumbidos` (não mexe na hierarquia dos focos), posicionado no centro dos renderers do foco.
  - Curva de rolloff própria: volume cheio até 0,8 m, depois cai como 1/d × (1 − x²) e chega a 0 em 11 m.
  - Só toca com o ouvinte a menos de 11,5 m: 23 focos, e o projeto tem 32 vozes reais. Ponto de início e pitch aleatórios em cada foco.
  - Cada clique toca o som do tipo do foco (tabela `sonsPorTipo`) num pool de 4 fontes 3D.
  - Ao eliminar: o zumbido some em 0,4 s, toca `foco_eliminado` e, 0,45 s depois, `ocupacao_desce`. Esse último não toca no último foco (é a vitória).
- `SomDoJogador.cs` (Player):
  - O passo toca quando `CameraJogador.FasePasso` cruza π/2 + kπ, o ponto mais baixo do head bob. Só no chão e acima de 0,3 m/s.
  - Volume de 0,35 andando a 0,6 correndo, pitch ±8%, sem repetir a variação anterior.
  - Passo de piso antes de `PartidaIniciada`, de rua depois.
  - Pulo e aterrissagem vêm dos eventos do `ControladorJogador`. Quedas abaixo de 3 m/s, que não disparam o evento, viram um passo se o jogador ficou mais de 0,2 s no ar.
- `AmbienteSonoro.cs` (Sons):
  - Loop do hospital, com crossfade de 1,2 s para o do bairro em `OnPartidaIniciada`.
  - Fade de entrada de 2,5 s em tempo de jogo, então só começa depois das cartelas.
  - No bairro, avulsos 3D em volta do ouvinte: pássaros a cada 5–14 s (12–30 m, 4–8 m de altura) e latidos a cada 25–60 s (25–45 m).
- `TrilhaSonora.cs` (Sons):
  - As 4 camadas com `PlayScheduled` no mesmo `dspTime`, em loop, com `ignoreListenerPause`.
  - Pesos: base 0,8; pulso = `InverseLerp(0,55; 0,95; ocupação)`; tensão = (1 − progresso)^0,7 depois de sair do hospital (0,2 antes); esperança = progresso^0,8. Mudam a 0,25/s.
  - Vitória: pulso e tensão vão a 0 e toca o sting. Derrota: tudo some em 1,5 s e toca o sting.
  - Alerta ao cruzar 80, 90 e 95% subindo; o marco se rearma 3% abaixo.
  - **Pausa global:** `AudioListener.pause = (Time.timeScale == 0)`. No pause e nas cartelas da abertura, zumbido, ambiente e passos congelam; música e interface continuam. Com o `MenuPausa` aberto, a música cai para 55%. No `OnDestroy`, volta para `false`.
- `MusicaDeFundo.cs` (menu inicial): loop no canal Música, volume × (1 − `fade.alpha`) do `MenuInicial`. Entra com a imagem e some no "Jogar", sem mexer no `MenuInicial.cs`.
- `SomDeInterface.cs` (um por cena): no `Start`, assina o `onClick` dos `Button` e o `onValueChanged` dos `Toggle` de todos os `ItemDeMenu`, inclusive das telas escondidas. Toca `ui_clique`. Sem hover e sem sliders. Os toggles das Configurações usam `SetIsOnWithoutNotify`, então não tocam sozinhos.

**Arquivos novos (`Assets/Editor/`):**
- `ImportadorDeSons.cs`: `AssetPostprocessor` que só age na pasta dos sons.
  - `*_loop` e `trilha_*`: Compressed In Memory, Vorbis 0,6.
  - O resto, inclusive o zumbido: Decompress On Load, PCM.
  - `mosquito_`, `foco_`, `passaro_`, `latido_` e `passo_`: mono.
  - Todos com `preloadAudioData`.
- `SomBuilder.cs`: menu **Dengue > Configurar Sons**, idempotente.
  - Cria `--- SISTEMAS ---/Sons` com os 4 componentes e põe `SomDoJogador` no Player.
  - Religa os clipes toda vez. Na tabela `sonsPorTipo`, só acrescenta o tipo que falta, preservando ajustes feitos à mão.
  - Liga os 3 clipes do `CelularController` e garante o `CanalDeAudio` (Efeitos).
  - Abre a `MenuInicial` por trás, monta `Sons` (`MusicaDeFundo` + `SomDeInterface`), salva e fecha.
  - Sem `FluxoDeJogo` na cena ativa, só configura o menu (se for a cena do menu) ou avisa.
  - `MontarSomDoMenu(cena)` e `LigarSonsDoCelular(controller)` são públicos para os outros builders.

**Arquivos alterados:**
- `Interactable.cs`: evento `OnClicado`, disparado em `Interact()` logo depois de `OnInteract()`.
- `CanalDeAudio.cs`:
  - propriedade `Volume` (volume antes do canal). Fades mexem nela, nunca no `AudioSource.volume`, que seria desfeito no próximo `Mudou`;
  - inicialização sob demanda;
  - `Fonte`;
  - helper `CanalDeAudio.Criar(pai, nome, canal, espacial)`.
- `CameraJogador.cs`: `public float FasePasso`.
- `HistoriaRedeSocial.cs`: `TipoDoFoco` passou de `private` para `internal` (usado pelo `SomDosFocos`).
- `CelularBuilder.cs`: chama `SomBuilder.LigarSonsDoCelular`. Antes, recriar o celular perdia os clipes e o `CanalDeAudio`.
- `MenuInicialBuilder.cs`: chama `SomBuilder.MontarSomDoMenu(menu)` depois de `MontarCamera`, porque o builder refaz a cena do zero.
- `HierarquiaOrganizador.cs`: `TrilhaSonora` → SISTEMAS.

**Aplicado nas cenas (via CLI):**
- `Dengue > Configurar Sons` rodado com a `SampleScene` aberta, e as duas cenas salvas.
- Segunda execução: nada novo criado.

**Verificado em Play (via `eval_file`):**
- Abertura:
  - `listenerPause` fica ligado durante as cartelas.
  - A trilha entra em fade: pesos 0,05 → 0,80 em ~3 s.
  - O ambiente do hospital só entra depois das cartelas (0,5 com 2,5 s de fade).
- Pulo, aterrissagem e passo tocam, com piso no hospital e rua depois.
- Saída + teleporte a 2 m de um pneu:
  - 3 zumbidos tocando (2,2 / 4,4 / 4,6 m) e os outros 20 parados;
  - crossfade do ambiente para o bairro;
  - pássaro tocando a 21 m;
  - tensão = 1,0.
- Os 5 cliques no pneu tocaram no centro dele, com pitch variado. No 5º: `foco_eliminado` tocou e o zumbido parou.
- Ocupação de 0,58 para 0,899: os marcos de 80 e 90% dispararam e o de 95% continuou armado.
- `MenuPausa.Abrir()`: `listenerPause` ligado, música a 55%, e o clique em "Configurações" tocou com o jogo pausado. `Fechar()`: tudo volta.
- Vitória (todos os focos): sting tocando; pulso 0,17 e tensão 0,13 descendo; esperança 0,92.
- Derrota (ocupação 0,999): as 4 camadas a 0 e o sting tocando.
- Menu inicial:
  - música tocando (0,56 = 0,7 × 80% do canal Música com o fade claro);
  - clique em "Como jogar" e em "Jogar";
  - a `SampleScene` carregou a partir do menu com os sons funcionando.
- Console: só o erro antigo do `Default-Particle.psd` (`Interactable.cs:307`).
- Limpeza: `PlayerSettings.runInBackground = false`, e a `MenuInicial` aberta como estava.

**NÃO verificado:**
- **Ninguém ouviu os sons.** O caráter foi conferido só por medição e espectrograma: bolhas subindo na água, sílabas do pássaro, colcheias da tensão, batimentos, e o fim dos sons modais sem estalo (−62 a −72 dB).
- Passos com movimento real (andar, correr), porque as teclas não chegam com o Editor sem foco.
- O fade da música no "Jogar" (0,5 s, rápido demais para medir entre duas chamadas do CLI). A regra é a mesma da entrada, que foi medida.
- `Dengue > Criar Celular` e `Dengue > Criar Menu Inicial` **não** foram rodados de novo: um abre diálogo e o outro refaz o `CenarioMalhas.asset` com GUID novo. A chamada nova de cada um foi validada só pela compilação.

**Observações para futuras sessões de IA:**
- **NUNCA editar scripts com o jogo em Play.** O Editor recompila e continua o Play (reload de domínio): todas as inscrições em eventos são perdidas (inclusive `OnDerrota`/`OnVitoria` dos scripts antigos) e os campos `NonSerialized` zeram. Nesta sessão isso gerou um `NullReferenceException` falso no `AmbienteSonoro` e uma derrota sem sting. Saia do Play, edite, recompile e entre de novo.
- `AudioSource.ignoreListenerPause` **não é salvo na cena**. Ligue-o em runtime (os componentes de som fazem isso ao criar as fontes).
- Para mudar um som: edite a receita em `gerar_sons.py` e rode com `--so nome`. O GUID se mantém; não precisa rodar o builder de novo.
- Som novo: gere o `.wav`, ponha o campo no componente e ligue em `SomBuilder`. Fonte nova sempre com `CanalDeAudio.Criar(...)`.
- Com várias camadas de música, o `unscaledDeltaTime` do primeiro frame traz o tempo de carregamento da cena. Limite o passo (a `TrilhaSonora` usa 0,1 s), senão o fade de entrada é pulado.
- Script de teste que pula a abertura: `StopAllCoroutines` nas cartelas + zerar `<Tocando>k__BackingField` + `EsconderImediato()` + invocar o `FimAbertura` privado do `HistoriaDoJogo`.

### 2026-09-26 — Jogo mais calmo: trilha, mosquitos e notificações

**Objetivo:** depois de ouvir os sons, o usuário achou o jogo **frenético**. Pedidos:
1. "quero que a maior parte do jogo seja um ambiente calmo e tranquilo", sem a música tensa o tempo todo;
2. menos som de mosquito;
3. "tá vindo muito popup no celular ao mesmo tempo". Se dois gatilhos de mensagem dispararem num intervalo curto, só um deve aparecer, e o outro fica "guardado para outra vez" (não some).

Escolhas dele: só o foco mais perto zumbe, e a mensagem que perde a vaga espera e aparece depois.

**Notificações — `RedeSocial.cs`:**
- Novo `enum PrioridadePost { Baixa, Normal, Alta, Maxima }` e `Enfileirar(post, prioridade = Normal)`.
- A fila virou uma **reserva**:
  - Gatilhos do mesmo instante (`janelaAgrupamento` 0,5 s) disputam a vaga. Vence a maior prioridade; empate, o que chegou antes.
  - No máximo **1 aviso a cada `intervaloEntreAvisos` (8 s)**. O resto espera.
  - Passou de `validadeReserva` (60 s) ou `validadeBaixa` (20 s, para prioridade Baixa): entra no feed **sem aviso**.
  - Com o celular aberto, a reserva entra toda no feed na hora.
- Novo evento `OnAviso`, só para os posts que viram popup + som. O `OnPostPublicado` continua sendo todo post do feed.
- Nova propriedade `PodeAvisar`: reserva vazia e intervalo já passado. `NaFila` = reserva. Sai o `intervaloEntrePosts`.
- Post publicado com o celular aberto já conta como lido. Antes, o badge voltava a contar o que o jogador tinha acabado de ver no feed.
- Campo novo `celular`: vazio, procura na cena.

**`HistoriaRedeSocial.cs`:**
- Prioridades:
  - **Máxima**: derrota e finais.
  - **Alta**: fala do dono ao eliminar, marcos de ocupação, saída do hospital.
  - **Normal**: chegar perto, largar pela metade, foco sem dono. A abertura da `RedeSocial` também.
  - **Baixa**: sequência rápida, parado, pulos, demora.
- `FilaLivre` = `rede.PodeAvisar`, e sai o `maxFilaParaExtras`. As mensagens de estado (chegar perto, largar, parado, pulos, demora) só nascem quando viram aviso na hora. Senão o índice não anda e o gatilho tenta de novo depois.
- A "sequência rápida" é um evento único e chega junto com a fala do dono, que sempre ganha. Por isso ela vai para a reserva com prioridade Baixa (`extra: false`): aparece depois ou entra no feed em silêncio após 20 s. Foi um ajuste em relação ao plano, porque do outro jeito ela nunca apareceria.

**`CelularController.cs` e `NotificacaoCelular.cs`:** o som, a pulsada do ícone e o banner escutam `OnAviso` (antes, `OnPostPublicado`).

**Trilha — `TrilhaSonora.cs`:** calma na maior parte do jogo.
- Pulso = ocupação **82→97%** (antes 55→95%).
- Tensão = ocupação **88→98%** (campo novo `faixaTensao`). Antes era (1 − progresso)^0,7, cheia do começo ao fim; sai o `tensaoNoHospital`.
- Esperança (marimba) = `Lerp(esperancaInicial 0,45; 1; progresso^0,8)`, presente desde o começo.
- `volumeCamada` 0,55 → 0,4 e `velocidadeMistura` 0,25 → 0,15.
- Alerta sonoro só em 90% e 95%.

**Ambiente — `AmbienteSonoro.cs`:** `volumeBairro` 0,6 → 0,75.

**Mosquitos — `SomDosFocos.cs`:**
- `maxZumbidosJuntos` = 1: só o foco ativo mais perto, dentro do alcance, zumbe.
- Cada zumbido tem um `peso` que entra e sai em `duracaoTroca` (0,5 s).
- `distanciaMaxima` 11 → 7 m e `volumeZumbido` 0,6 → 0,35.
- `gerar_sons.py` / `s_mosquito`: 3 insetos em vez de 5, harmônicos caindo mais rápido, passa-baixas em 3,5 kHz, oscilação de volume menor. Centróide espectral em 779 Hz, 1,9% da energia acima de 2 kHz. Regerado com `--so mosquito_zumbido_loop` (o GUID se manteve).

**Aplicado na cena:** um `eval_file` com `SerializedObject` gravou os valores novos dos campos que já existiam na `SampleScene`: `volumeZumbido`, `distanciaMaxima`, `volumeCamada`, `velocidadeMistura`, `faixaPulso`, `marcos` e `volumeBairro`. Depois, `save_scene`. Os campos novos pegaram o padrão do código.

**Verificado em Play (via `eval_file`; nenhum script editado durante o Play):**
- Log dos avisos:
  - Um a cada exatos **8,0 s** (1,2 / 9,2 / 17,2 / 25,2 / 33,2...).
  - A saída do hospital e as falas dos donos passaram na frente das postagens de abertura.
  - O combo da Bruna esperou e entrou no feed sem aviso aos 20 s.
  - A fila do banner nunca passou de 0.
- Celular aberto com 3 na reserva: as 3 entraram no feed sem aviso, e os não lidos ficaram em 0.
- Trilha:
  - ocupação 70% → pulso 0, tensão 0, esperança 0,45;
  - 95% → pulso 0,87, tensão 0,70;
  - de volta a 70% → as duas zeram.
- Zumbidos: só 1 tocando. Ao teleportar para uma garrafa a 3 m da bromélia, a garrafa chegou a peso 1 e a bromélia a 0 em ~0,5 s.
- Emenda do zumbido novo: gerado com 12 s, a segunda metade repete a primeira com diferença de 2×10⁻⁹ do pico.
- Console sem erros novos. `PlayerSettings.runInBackground` voltou a `false`, e a `MenuInicial` ficou aberta.

**Descoberta:** as configurações salvas do usuário estão com **Efeitos 25%** e **Geral 40%** (Música 80%). O zumbido, o ambiente do bairro e os passos estão no canal Efeitos, então com 25% eles ficam bem abaixo da música. Não foi mexido (é preferência dele); avisado.

**NÃO verificado:** ninguém ouviu ainda a versão calma. Nos testes, o intervalo de 8 s foi visto só pelo log.

**Observações para futuras sessões de IA:**
- Mensagem nova no celular: escolha a prioridade. Se ela só faz sentido na hora (estado do jogador), use `extra: true` / `FilaLivre`, e ela tenta de novo depois. Se é um evento único, mande para a reserva.
- Nos testes com a ocupação alta, **congele** a subida (`aumentoPorSegundo` e `aumentoNoInicio` = 0 por reflection). A 1%/s a derrota chega em segundos e zera a trilha no meio da medição.

### 2026-09-26 — Mensagens na hora (a recusada só volta num novo gatilho) + passos sem chiado

**Objetivo:** o usuário testou a reserva da entrada anterior e não gostou: "parece que as mensagens estão com delay. Depois que tu faz a ação, demora". Pedido dele: se a mensagem de uma ação não é a prioridade, ela fica guardada e **só é ativada se o gatilho dela acontecer de novo**. Nunca sai sozinha depois. Também: "os passos estão parecendo muito chiado".

**`RedeSocial.cs` — sai a reserva inteira** (`reserva`, `intervaloEntreAvisos`, `janelaAgrupamento`, `validadeReserva`, `validadeBaixa`, `NaFila`, `PodeAvisar`):
- `janelaConflito` (4 s) é o "intervalo curto".
- `Aceitaria(prioridade)`: verdadeiro se já passou a janela desde o último aviso, ou se a prioridade é MAIOR que a dele.
- `Postar(post, prioridade)`: publica **no mesmo frame** (feed + `OnAviso`) ou recusa sem publicar nada, devolvendo `false`.
- `PostarNoFeed(post)`: só no feed, sem aviso.
- `PostarGrupo(posts, prioridade)`: posts do mesmo momento. O 1º vira aviso e o resto entra só no feed, junto. Usado na abertura (depois de `atrasoInicial`, que conta a partir do fim das cartelas), na saída do hospital, nos finais e na derrota.
- Continuam valendo: `OnAviso`, e o post que chega com o celular aberto já contar como lido.

**`NotificacaoCelular.cs` — sem fila.** O aviso aparece na hora. Se já houver um banner (quando chega algo de prioridade maior logo depois), o conteúdo é trocado e ele reentra do meio da animação. Sai o `duracaoMinima`.

**`HistoriaRedeSocial.cs` — consulta `Aceitaria` ANTES de gastar a mensagem. Recusada, o gatilho precisa acontecer de novo:**
- **Parado / demora sem foco** (Baixa): o timer zera de qualquer jeito.
- **Pulos** (Baixa): a contagem zera de qualquer jeito.
- **Chegar perto** (Normal): agora dispara ao **ENTRAR** no raio (`dentroDoRaio`). Antes, tentava a cada 0,25 s enquanto o jogador estava perto.
- **Largar pela metade** (Normal): dispara ao **SAIR** de perto (`pertoDoComecado`).
- **Marco de ocupação** (Alta): recusado, ele **desarma**. Precisa voltar para o outro lado do limite e cruzar de novo.
- **Fala do dono** (Alta): só é gasta se sair (`FalaAoEliminar` avança o índice e só é chamada depois de `Aceitaria`).
- `PostarDaLista` só avança o índice quando o post sai.
- **Uma mensagem por foco eliminado**, a mais importante: finais > "último do tipo" do dono > sequência rápida (agora Alta) > fala normal do dono > "sem dono". Sem essa regra, a sequência rápida perderia sempre para a fala do dono.
- O `Enfileirar` interno virou `Postar` / `Formatar` / `Formatados`.

**Passos — `gerar_sons.py`:**
- `pisada_rua`:
  - Sai o ruído de "cascalho" de 2,5–8 kHz e o ruído de banda larga até 2,2 kHz.
  - Agora: baque de 90 → 60 Hz (0,45), modos curtos em 180 / 340 / 720 Hz e um raspão baixo só entre 250 e 1200 Hz.
  - Passa-baixas de 2 kHz, sem reverb.
- `s_passo_piso`: modos em 700–1400 Hz (antes 1,7–5,2 kHz), sem o estalo de ruído agudo, reverb de 0,3 s com brilho de 3 kHz.
- `s_aterrissagem`: herda a pisada nova, baque em 0,7, sem reverb.
- Medido:

| Som | Energia acima de 2 kHz | Centróide |
|---|---|---|
| passo_rua | **61% → 0,0–0,1%** | 4239 → ~230 Hz |
| passo_piso | 15% → 0% | 1512 → ~245 Hz |
| aterrissagem | 32% → 0% | — |

- A 1ª versão ficou grave demais (96% abaixo de 300 Hz, centróide em 114 Hz, e some em alto-falante pequeno). Foi reequilibrada: ~76% abaixo de 300 Hz.
- Regerados com `--so` (os GUIDs se mantiveram).

**Verificado em Play (`eval_file` + log do `OnAviso` com o `Time.time`):**
- Abertura: 1 aviso e 3 no feed, todos em t=1,20.
- Saída do hospital: aviso da Enf. Rita e o post da UPA no feed, no mesmo t.
- Pneu eliminado 3 s depois da saída: recusado. A fala do Carlos ficou no índice 0.
- Segundo pneu 7,6 s depois: a sequência rápida saiu **no mesmo t da eliminação** (16,05). O marco de 40%, que disparou junto, foi recusado e desarmado.
- 21 s parado: nada saiu, fora o gatilho legítimo de "parado há 35 s".
- Próximo pneu: a fala guardada do Carlos ("esvaziou o primeiro pneu") saiu no mesmo t da eliminação (63,12).
- Marco: com a ocupação em 39%, não saiu sozinho. Saiu na hora só depois de voltar a 45% e cair de novo.
- Prioridade: o marco (Alta) passou por cima do "parado" (Baixa) dentro da janela.
- "Chegar perto":
  - recusado ao entrar dentro da janela;
  - 6 s parado lá: nada;
  - saiu e voltou: saiu a da bromélia, e a da garrafa, que entrou junto, foi recusada;
  - saiu e voltou de novo: saiu a da garrafa.
- Console sem erros novos. `runInBackground = false` e a `MenuInicial` aberta como estava.

**NÃO verificado:** a troca de conteúdo do banner (prioridade maior dentro da janela) foi validada só pela lógica, sem captura de tela. Os passos novos ainda não foram ouvidos.

**Observações para futuras sessões de IA:**
- O usuário deixou o Editor **em Play** depois de testar. Antes de `recompile`, confira `editor_status.playMode` e dê `editor_stop`: recompilar com o jogo rodando derruba todos os eventos.
- Para mensagem nova no celular, escolha: (a) a prioridade; (b) o que conta como "o gatilho de novo" quando ela for recusada; (c) só gastar o conteúdo depois de `rede.Aceitaria(...)`. Nunca crie uma fila ou um "tenta de novo depois": o usuário recusou atraso.
- No Git Bash deste Windows, heredoc com certos textos (acentos + aspas) quebra o parse. Escreva o script Python com a ferramenta Write e rode o arquivo.

### 2026-09-27 — Rebalanceamento da ocupação: cada foco desacelera a taxa

**Objetivo:** o usuário achou o jogo "meio desbalanceado" e pediu mais dificuldade. Escolhas dele: a alavanca **nova curva** (taxa proporcional aos focos ativos, como o GameDoc pede) e o alvo **desafio moderado** (na primeira vez dá para vencer explorando com calma; só perde quem fica muito tempo perdido). Nada de rejogabilidade por enquanto.

**Diagnóstico do modelo antigo** (valores da cena: 70% inicial, 0,2%/s no início, rampa até 2%/s, −15% por foco, 23 focos = 345% de redução):
- No começo era fácil demais: no período calmo, 5 focos zeravam a barra, e a redução que passava do 0% se perdia.
- Depois virava um paredão: a 2%/s, cada foco comprava só 7,5 s. Simulado com ritmo uniforme, o jogador perdia por volta dos 2 min mesmo limpando um foco a cada 10 s.

**Modelo novo — `Assets/Scripts/FluxoDeJogo.cs`:**
```
pressao = focosAtivos / focosTotal
rampa   = Lerp(fatorInicioCalmo, 1, Clamp01((t − duracaoInicioCalmo) / duracaoRampa))
taxa    = rampa × (taxaComTodosOsFocos × pressao − recuperacaoSemFocos)
se taxa > 0 e celular aberto: taxa × multiplicadorComCelular
Ocupacao = Clamp(Ocupacao + taxa·dt, ocupacaoMinima, 1); ao eliminar: −alivioPorFoco (respeitando o mínimo)
```
- Saíram `aumentoPorSegundo`, `reducaoPorFoco` e `aumentoNoInicio`. Os campos novos têm **nomes novos de propósito**, sem `FormerlySerializedAs`, para os valores antigos da cena não voltarem.
- Valores: `taxaComTodosOsFocos` 0,003 (0,3%/s), `recuperacaoSemFocos` 0,001, `alivioPorFoco` 0,025, `ocupacaoMinima` 0,30, `fatorInicioCalmo` 0,2. Mantidos: `ocupacaoInicial` 0,7, `duracaoRampa` 60, `multiplicadorComCelular` 0,25 (que agora só vale para a subida).
- **`duracaoInicioCalmo` ficou em 15 s**, o valor que o usuário tinha posto na cena (o código tem 45 de padrão). Não foi mexido.
- Com os 23 focos, a taxa líquida é +0,2%/s. Com **7 focos restantes** ela fica negativa e a ocupação **desce**: é o "clímax" do GameDoc.
- Novo `public float TaxaPara(float tempo, int focosAtivos, bool celularAberto, int focosTotal = -1)`, usado pelo `Update` e pelo simulador. Propriedades `OcupacaoInicial`, `OcupacaoMinima` e `AlivioPorFoco`.
- APIs obsoletas trocadas neste arquivo: `FindAnyObjectByType` e `FindObjectsByType<Interactable>(FindObjectsInactive.Include)`.
- Quem só lê `Ocupacao` não mudou: `TrilhaSonora`, `HistoriaRedeSocial`, `FluxoDeJogoHUD`, `MenuPausa` e `SomDosFocos`.

**Arquivo novo — `Assets/Editor/BalanceamentoSimulador.cs`:** menu **Dengue > Simular Balanceamento**.
- Lê o `FluxoDeJogo` da cena aberta e simula com o mesmo `TaxaPara`, a passos de 0,1 s.
- Ritmos de 15 a 50 s por foco. Os últimos focos levam até 1,6× mais, porque são os mais difíceis de achar. O celular fica fechado.
- Imprime pico, ocupação final e duração, ou o momento da derrota, e também o tempo até perder parado.
- Estima a rota mínima: vizinho mais próximo a partir de `SaidaDoHospital.posicaoNoBairro`, na velocidade de corrida, mais 0,25 s por clique.
- Saída só no Console.

Resultado na cena atual:

| Ritmo (s/foco) | Resultado |
|---|---|
| 15 | vence em 7:25, pico 71% |
| 20 | vence em 9:53, pico 72% |
| 25 | vence em 12:21, pico 79% |
| 30 | vence em 14:49, pico 87% |
| 35 | vence em 17:16, pico 95% |
| 40 | perde aos 6:28 (8/23) |
| 45 | perde aos 5:22 (6/23) |
| parado | perde aos 3:06 |

Rota mínima: 390 m ≈ 1:42 (4,4 s/foco, limite teórico de quem já conhece o mapa).

**Marco "a ocupação caiu": de 40% para 55%.** Com o mínimo de 30% e a descida só no fim, 40% quase nunca aconteceria. Com 55%, o post da UPA sai no clímax.
- `HistoriaRedeSocial.cs`: padrão em `MarcosPadrao`.
- Na cena: `Historia` → `marcosDeOcupacao[3].limite`, via `SerializedObject`. A `SampleScene` estava limpa e foi salva.

**Verificado em Play (via `eval_file`; abertura pulada; `ControleAtivo = false`):**
- `TaxaPara`: +0,200%/s com os 23 ativos depois da rampa; +0,050%/s com o celular; +0,040%/s no início calmo; −0,061%/s com 3 ativos (o celular não muda a descida).
- Subida real no `Update`: +1,95% em 9,77 s de jogo.
- 20 focos eliminados por código: −2,5% cada, parando em 30,00% (não passa do mínimo).
- Ocupação forçada a 58% com 3 ativos: desceu a −0,061%/s. Ao cruzar 55%, saiu o post "A ocupação caiu para 55%!".
- Todos eliminados: `OnVitoria`, partida encerrada, epílogo segurando a tela final, relógio parado.
- Partida nova com a ocupação a 99,5%: chegou a 100%, `OnDerrota` disparou, e a HUD e o epílogo de derrota foram ativados. Nenhum erro no Console.
- Os únicos erros foram os antigos do `Default-Particle.psd` (`Interactable.cs:307`), 2 por foco que usa a animação padrão de conclusão (6 pneus = 12).
- Limpeza: `PlayerSettings.runInBackground = false`.

**NÃO verificado:**
- Partida real com teclado e mouse (o Editor sem foco não recebe input). Falta o usuário jogar e dizer se ficou moderado.
- Ao sair do Play, a `SampleScene` voltou marcada como modificada sem nenhuma edição desta sessão depois do save. Não foi salva de novo.

**Observações para futuras sessões de IA:**
- Para reajustar a dificuldade: mexa primeiro em `taxaComTodosOsFocos` (pressão geral) e `duracaoInicioCalmo` (folga do começo) no Inspector do `FluxoDeJogo` e rode **Dengue > Simular Balanceamento**. `recuperacaoSemFocos` / `taxaComTodosOsFocos` × total = quantos focos restantes fazem a barra descer.
- ~~Com as cenas `MenuInicial` e `SampleScene` abertas juntas no Editor, o Play gera milhares de avisos "There are 2 event systems in the scene". Não é bug do jogo.~~ **Errado.** Isso fazia as duas cenas rodarem juntas no Play, e o usuário viu o jogo "iniciando direto". Ver a entrada seguinte (`GuardaDeCenas`).
- `unity command eval_file` recebe o caminho em `--file` (não `--path`).

### 2026-09-28 — MenuInicial e SampleScene rodando juntas no Play + proteção `GuardaDeCenas`

**Problema:** o usuário reclamou que o jogo estava "iniciando direto". A causa era o estado do Editor, não arquivo nenhum:
- a `SampleScene` (ativa) e a `MenuInicial` estavam abertas juntas na Hierarchy, e o Play rodava as duas;
- o console mostrava "There are 2 event systems" e "2 audio listeners";
- o `MenuInicial.Awake` forçava `timeScale = 1` e pré-carregava outra `SampleScene` por trás.

O estado já existia no começo da sessão de 2026-09-27. Aquela sessão viu os avisos e só anotou aqui, sem avisar o usuário. Conferido que nada foi gravado errado:
- a `SampleScene` salva só mudou os campos do `FluxoDeJogo` e o marco de 55%;
- o Build Settings continua com o `MenuInicial` no índice 0;
- o `SomBuilder` e o `MenuInicialBuilder` fecham direito as cenas que abrem.

**Correção no Editor (escolha do usuário):** ficou aberta **só a `MenuInicial`**. O Play começa na tela "FOCO ZERO", igual ao jogo final. Foi feito com `OpenScene(..., Single)` depois de conferir que nenhuma cena tinha mudança não salva.

**Arquivo novo — `Assets/Editor/GuardaDeCenas.cs`:**
- `[InitializeOnLoad]` + `playModeStateChanged`, age no `ExitingEditMode`.
- Se as duas cenas (`MenuInicial.NomeCena` / `NomeCenaJogo`) estiverem carregadas, fica só a cena **ativa**:
  - a outra, sem mudanças: é fechada com `CloseScene(extra, true)` e um aviso `[Cenas]` sai no Console;
  - a outra, com mudanças não salvas: o **Play é cancelado** (`EditorApplication.isPlaying = false`) com um aviso explicando o que fazer. Nada é descartado.
  - se o `CloseScene` falhar, também cancela o Play.
- Sem diálogo, para não travar o CLI.

**Verificado:**
- Compilação OK.
- Cena limpa: `SampleScene` aberta como aditiva com a `MenuInicial` ativa, depois Play.
  - Aviso "fechei SampleScene para o Play rodar só MenuInicial".
  - Nenhum "2 event systems".
  - Em Play, só a `MenuInicial`, mais a pré-carga normal da `SampleScene` (`loaded=False`).
  - Ao sair do Play, só a `MenuInicial` aberta.
- Cena marcada como suja (só `MarkSceneDirty`, sem mudança real): Play cancelado, aviso no Console e as duas cenas continuaram abertas. Depois a `SampleScene` de teste foi fechada.
- Os arquivos das duas cenas não mudaram (mesmo oid do LFS).

**NÃO verificado:** o usuário apertando Play de verdade no menu e seguindo pelo "Jogar".

**Observações para futuras sessões de IA:**
- Ao terminar uma sessão, deixe aberta **só a `MenuInicial`**. Para testar a partida direto, abra a `SampleScene` como cena única (`OpenSceneMode.Single`), nunca aditiva junto do menu.
- Se abrir a `SampleScene` como aditiva para algum builder, a proteção fecha a que não for ativa no Play. Se ela tiver mudanças, o Play é recusado.

### 2026-09-28 — Abertura obriga a abrir o celular (tecla T)

**Objetivo (pedido do usuário):** no começo do jogo, na tela preta das cartelas, o jogador precisa apertar **T** para abrir o celular antes de jogar. Antes, a última frase só sugeria "Acompanhe pelo celular [T]" e dava para passar direto.

**Comportamento:**
- A **última cartela da abertura** ("Tudo o que você fizer, alguém vai comentar. / Acompanhe pelo celular [T].") não sai sozinha. Clique, Espaço e Enter não avançam.
- **Esc** pula as frases anteriores e cai nessa última, que continua travada.
- A dica do rodapé troca de "clique para continuar · Esc pula" para **"aperte T para abrir o celular"**, em ciano `#40FFCC`, pulsando entre alpha 0,5 e 1 (1,4 pulso/s).
- Ao apertar **T**:
  - a tela preta clareia e o jogo despausa;
  - um frame depois o celular abre sozinho;
  - os posts iniciais da rede social saem na hora. Antes, eles esperavam o `atrasoInicial` de 1,2 s depois de o jogo andar, e o feed abriria vazio.
- O celular abre **antes** de publicar. Com ele aberto, os posts não viram não lidos e não geram banner nem som de notificação.
- O reinício com R continua pulando a abertura, e nesse caso não há obrigação. O "Reiniciar do zero" do pause toca a abertura de novo, com a obrigação.
- O epílogo não muda: continua avançando com clique, e o Esc pula tudo.

**Arquivos alterados:**
- `Assets/Scripts/CartelasNarrativas.cs`
  - `Tocar(..., Key? teclaParaSair = null)`: com a tecla, a última linha não vazia fica travada até ela.
  - Novo `EsperarTecla`: troca o texto e a cor da dica e pulsa; depois restaura os dois, porque o epílogo usa a mesma dica.
  - Campos novos: `textoDicaTecla`, `corDicaTecla`, `velocidadePulsoDica`.
  - O `foreach` virou `for` com índice.
- `Assets/Scripts/HistoriaDoJogo.cs`
  - `exigirCelularNaAbertura` (padrão `true`) e referência `rede`, achada no `Awake`.
  - `FimAbertura` chama a corrotina `AbrirCelularNoProximoFrame`, que faz `celular.Abrir()` e depois `rede.PublicarAberturaAgora()`.
- `Assets/Scripts/CelularController.cs`: `public Key TeclaAbrir`.
- `Assets/Scripts/RedeSocial.cs`: `PublicarAberturaAgora()` (só publica uma vez), usado também pelo `Update`.
- **Cena não alterada.** Os campos novos usam o valor padrão do código, e o texto da última cartela continua o serializado.

**Preview (skill preview-design):** antes × depois da última cartela capturados na Game view em Play Mode e mostrados ao usuário.

**Verificado (Play Mode, `SampleScene`, com `runInBackground` ligado só no teste):**
- Compilação OK. Nenhum erro no Console; os 53 avisos são os de API obsoleta que já existiam.
- Esc na 2ª cartela → pulou para a última, com a dica trocada. Clique e mais Esc, depois 12 s → continuou travada, `timeScale = 0`, celular desligado.
- T → `Tocando = false`, `timeScale = 1`, celular aberto, `aberturaPublicada = true`, `NaoLidos = 0`, interactor bloqueado (celular aberto), `ControleAtivo = true`. A dica voltou ao texto original. Screenshot: celular aberto no quarto do hospital com os posts da Dona Neusa, SOSO, João Silva e Ana Paula.
- Epílogo (chamado por reflection): dica normal, e o Esc pulou tudo.
- Limpeza: `PlayerSettings.runInBackground = false` (conferido `runInBackground: 0` no disco).

**NÃO verificado:**
- As teclas de verdade (T, Esc, clique), pelo mesmo motivo de sempre: com o Editor sem foco o Input System descarta o teclado sintético. As flags `pedirPularTudo`/`pedirProxima`/`pedirTecla` foram setadas por reflection.
- A leitura do T usa o mesmo padrão do `CelularController` (`teclado[tecla].wasPressedThisFrame`). O celular fica desligado até a tela clarear, então o mesmo T não abre e fecha na hora.

**Observações para futuras sessões de IA:**
- Para desligar a obrigação: `HistoriaDoJogo.exigirCelularNaAbertura = false` no Inspector.
- A tecla vem do `CelularController.teclaAbrir`. Se ela mudar, a trava segue a nova tecla, mas o texto `textoDicaTecla` (e a última cartela) precisam ser trocados à mão.


### 2026-09-29 — Nome do jogo na build

**Objetivo:** a build sair com o nome do jogo em vez de "My project (1)".

**Arquivo alterado:** `ProjectSettings/ProjectSettings.asset` — `productName: My project (1)` → `productName: Foco Zero`.

**Efeito:** o executável do Windows (`Foco Zero.exe`), a pasta `Foco Zero_Data` e o título da janela passam a usar o nome.

**Observações:**
- `companyName` também trocado: `DefaultCompany` → `Eduardo Richter`. Os `applicationIdentifier` continuam os do template URP (só importam para Android/iOS).
- PlayerPrefs ficam no registro em `HKCU\Software\<companyName>\<productName>`, então as configurações salvas antes (`ConfiguracoesJogo`) voltam ao padrão uma vez.
- Se o Editor estava aberto durante a edição, conferir em Project Settings > Player se o nome ficou; se não, digitar lá.


### 2026-09-29 — Ícone do jogo

**Objetivo:** a build ter ícone próprio. O usuário escolheu entre 3 prévias (mira, proibido, texto) e ficou com a **B — proibido**.

**Arte:** mosquito branco (vista de cima, asas translúcidas) atrás de um sinal de proibido vermelho `#E04848`, sobre um quadrado arredondado escuro (degradê `#1B2229` → `#0A0D10`), igual à paleta do menu. 1024×1024 RGBA, gerado por SVG.

**Arquivos:**
- `Assets/Meus/Assets Gerados/Icone/IconeFocoZero.png` (novo).
- `Assets/Editor/IconeDoJogo.cs` (novo): `[InitializeOnLoad]` + menu **Dengue > Aplicar Icone do Jogo**. Configura o import (sem compressão, sem mipmaps, alpha transparente) e põe a textura no **Default Icon** com `PlayerSettings.SetIcons(NamedBuildTarget.Unknown, ..., IconKind.Any)`. Só mexe se o ícone ainda não for esse.

**Observações:**
- Foi feito por script em vez de editar o `ProjectSettings.asset` à mão porque o Editor estava aberto (daria conflito com a cópia em memória).
- Para trocar o ícone: substituir o PNG no mesmo caminho, ou apagar o script e escolher no Inspector.


### 2026-10-25 — Roteiro "jogo interessante" (Partes A → B → C) + spec da Parte A (kit do agente)

**Objetivo (pedido do usuário):** "deixar o jogo mais interessante, transformar ele num jogo interessante mesmo", analisando tudo o que falta e fazendo por partes. É trabalho da faculdade, mas precisa prender quem joga por vontade própria e ter um pouco de rejogabilidade. Prazo: 2 a 4 semanas.

**Diagnóstico:** a casca (menu, abertura, hospital, celular, epílogo, sons, pausa, balanceamento) está pronta. O loop central é raso:
- todo foco se resolve mirando e clicando de 3 a 5 vezes;
- a busca não tem desafio (lista aberta, brilho por proximidade, zumbido);
- o mundo é parado (focos não evoluem, sem chuva);
- o bairro é vazio (moradores só no celular);
- não há recursos nem escolhas;
- não há rejogabilidade.

**Roteiro escolhido pelo usuário:** **A → B → C**.
- **A — Kit do agente + ação certa para cada foco** (spec pronta).
- **B — Focos vivos + chuva**: ovo → larva → mosquito, e a chuva reenche o que só foi esvaziado.
- **C — Rejogabilidade**: focos sorteados, pontuação/estrelas, recorde, muletas mais leves.
- Ficou para depois: D — moradores no mapa.

**Decisões da Parte A (escolhas do usuário):**
- abordagem "kit + ferramenta certa": segurar para agir, cada foco pede a ação correta, ferramenta errada ensina o porquê, pneu carregado até a caçamba, garrafas no saco;
- estoque **apertado**: areia 1, tela 1, água sanitária 2 doses, saco de 6 garrafas, tudo ajustável no Inspector;
- reabastecimento na picape da UPA, caçamba no terreno baldio e monte de areia como atalho.

**Arquivo novo:** `docs/superpowers/specs/2026-10-25-parte-a-kit-do-agente-design.md`, com a spec completa: jogabilidade, arquitetura, arquivos novos e alterados, montagem da cena e verificação.

**Nenhum script, cena ou asset foi alterado nesta entrada.** O próximo passo é o plano de implementação da Parte A.

**Observações para futuras sessões de IA:**
- A pasta `F:\Unity\Projetos\My project (1)` está vazia; o projeto é este (`Foco Zero`).
- Na Parte A, `PontoDeUso` (picape, caçamba, monte de areia) **não pode herdar `Interactable`**. `FluxoDeJogo`, `DengueHUD`, `FocoListaHUD` e `RedeSocial` varrem `Interactable` como foco.
- O projeto tem o Unity Test Framework 1.7, mas nenhum teste até aqui. A spec prevê testes EditMode em `Assets/Editor/Testes/`.


### 2026-10-25 — Reposicionamento da Barra de Ocupação Hospitalar na HUD (Topo Central)

**Objetivo (pedido do usuário):** Resolver conflito visual de sobreposição entre a barra de lotação do hospital e a barra de ferramentas do agente (Hotbar/KitHUD), movendo a barra de ocupação para a parte superior da tela.

**Arquivos alterados:**
- `Assets/Editor/FluxoDeJogoHUDBuilder.cs` — Atualizado `PainelSobrio("OcupacaoPanel", ...)` para ancorar no centro superior (`anchorMin = (0.5, 1)`, `anchorMax = (0.5, 1)`, `anchoredPosition = (0, -32)`).
- `Assets/Editor/DengueHUDBuilder.cs` e `Assets/Editor/DengueHUDReparo.cs` — Ajustada a posição de nascimento do painel de aviso temporário `ToastPanel` de `y = -60` para `y = -126`, garantindo que mensagens de eliminação de foco e dicas educativas apareçam logo abaixo da barra de ocupação sem colisão.
- `Assets/Editor/HUDLayoutAjustador.cs` (novo) — Ferramenta utilitária acessível via menu `Dengue > Ajustar Layout da HUD (Ocupação no Topo)` que atualiza as âncoras e posições dos GameObjects existentes na cena aberta sem recriar objetos ou perder bindings.
- `docs/superpowers/specs/2026-10-25-hud-ocupacao-superior-design.md` — Especificação do layout da HUD.
- `docs/superpowers/plans/2026-10-25-hud-ocupacao-superior.md` — Plano de implementação.

**Configurações aplicadas:**
- `OcupacaoPanel`: Anchors `(0.5, 1.0)`, Pivot `(0.5, 1.0)`, AnchoredPosition `(0, -32)`.
- `ToastPanel`: Anchors `(0.5, 1.0)`, Pivot `(0.5, 1.0)`, AnchoredPosition `(0, -126)`.
- Hotbar (`KitHUD`): Permanece no centro inferior (`y = 40`), agora totalmente desimpedida.

**Observações técnicas:**
- No Editor do Unity, basta rodar o menu `Dengue > Ajustar Layout da HUD (Ocupação no Topo)` para aplicar instantaneamente na cena aberta e salvar (Ctrl+S).


### 2026-10-25 — Parte A implementada: kit do agente e ação certa por foco

**Objetivo:** implementar a Parte A do roteiro "jogo interessante". O registro anterior desta mesma data ("Roteiro 'jogo interessante' (Partes A → B → C) + spec da Parte A (kit do agente)") já existia e trazia só a spec. Agora o "clicar N vezes" virou **"segurar com a ferramenta certa"**, com um kit de estoque apertado:
- cada foco pede a sua ferramenta, e a errada ensina o porquê;
- o pneu é carregado até uma caçamba;
- as garrafas vão para um saco que enche;
- as cargas se reabastecem na picape da UPA (e a areia também no monte de areia).

Spec: `docs/superpowers/specs/2026-10-25-parte-a-kit-do-agente-design.md`. Plano (14 tarefas): `docs/superpowers/plans/2026-10-25-parte-a-kit-do-agente.md`. Trabalho na branch `parte-a/kit-do-agente`, **sem commit**: tudo ficou no índice do git (staged), para o usuário commitar ou desfazer.

**Como jogar agora:**
- **Barra de ferramentas** (KitHUD), no centro de baixo da tela, com 5 espaços: 1 Mãos, 2 Areia, 3 Tela, 4 Água sanitária, 5 Saco de lixo. Cada espaço mostra a carga ("1/1", "0/6"); a carga zerada fica vermelha.
  - Troca pelas teclas **1–5** (ou numpad 1–5), pela **roda do mouse** ou por **LB/RB** no controle.
  - Carregando pneu, não troca.
- **Segurar** o clique esquerdo (no controle, o botão de ataque, Oeste) no foco mirado.
  - A barra da mira enche de forma contínua.
  - Ao soltar, os passos já dados ficam guardados; só a fração do passo em andamento se perde.
  - A carga só é gasta quando o foco conclui.
- **A mira mostra o problema**, por exemplo "Ralo destampado com água parada", com a linha "segure o clique" embaixo.
  - Ferramenta errada: nada acontece, e um aviso ensina a certa (`FocoHUDInfo.textoFerramentaErrada`).
  - Sem carga: "Acabou: **Areia**. Reabasteça na picape da UPA."
  - Saco cheio: "Saco cheio. Esvazie na **caçamba** do terreno baldio."
- **Pneu:** com as Mãos, segurar esvazia a água, e o pneu vai para os braços, no canto inferior direito da tela.
  - Carregando: velocidade × 0,7, sem correr, sem pular e sem trocar ferramenta. Só pontos de uso viram alvo, e a linha "Carregando pneu: leve à caçamba · [botão direito] larga" aparece acima da barra.
  - **Botão direito, G ou botão Norte** do controle larga o pneu na frente do agente. Largado, ele pega de novo na hora com as Mãos, e a mira mostra "Pneu vazio: leve à caçamba".
  - O pneu só conta como eliminado quando é entregue numa **caçamba** (segurar 1 s nela).
  - O pneu já esvaziado não zumbe mais.
- **Garrafas:** com o Saco, uma garrafa por passo (0,4 s cada). Com o saco cheio, é preciso esvaziar numa caçamba (segurar 1 s).
- **Pontos de uso:** um uso por aperto (para usar de novo, solte e aperte outra vez).
  - **Picape da UPA**, perto da saída do hospital no bairro: segurar 1 s enche areia, tela e água sanitária.
  - **Monte de areia** do terreno baldio: segurar 1,5 s enche só a areia.
  - Recusas: "Seu kit já está cheio.", "Largue o pneu antes." e "Nada para jogar na caçamba.".
- A **Enf. Rita** agora fala do kit e da picape: "Agente, o hospital não aguenta mais! Seu kit está pronto [1-5] e a picape da UPA tem mais material. [...]".
- A dica "Teclas **1–5** trocam a ferramenta" aparece no começo da partida e some na primeira troca.

**Valores finais por foco** (tabela `Focos` do `KitDoAgenteBuilder`, gravada nos prefabs e nas instâncias):

| Foco | Ferramenta | Segurando | Na cena |
|---|---|---|---|
| Pneu | Mãos | 1,5 s (5 passos de 0,3 s) + entrega na caçamba | 6 |
| Vaso (pratinho) | Areia | 2,0 s (10 × 0,2 s) | 3 |
| Caixa d'água | Mãos | 3,0 s (10 × 0,3 s) | 2 |
| Piscina de plástico | Mãos | 2,5 s | 0 (a cena não tem piscina; os 2 prefabs ficaram configurados) |
| Garrafas | Saco de lixo | 0,4 s por garrafa | 3 grupos (5, 5 e 12 garrafas) |
| Ralo | Tela | 2,0 s (3 × 0,667 s) | 4 |
| Bromélia | Água sanitária | 2,0 s (5 × 0,4 s) | 4 |

Total: **22 focos** (o plano falava em 23 contando uma piscina que a cena não tem).

**Outros valores:**
- **Estoque** (`KitDoAgente` no Player, ajustável no Inspector): areia 1, tela 1, água sanitária 2 doses, saco de 6 garrafas. Carregando pneu, `multiplicadorCarregando` = 0,7.
- **Picape da UPA** (`--- BAIRRO ---/Objetos de Rua/Picape da UPA`, prefab do pacote Palmov):
  - em (−2,23; 0,11; −19,19), girada −90°, estacionada rente ao meio-fio norte;
  - recarrega Areia, Tela e Água sanitária; 1 s.
- **Monte de areia** (`Monte de Areia 001`, o único da cena):
  - em (−25,32; 0,22; 58,94); recarrega só a Areia; 1,5 s;
  - fica a ~38 m do centro dos vasos.
- **3 caçambas** (5 cubos com `Cacamba.mat`, `BoxCollider` sólido na raiz e pivô no chão), 1 s cada. O pneu entregue é solto 1,2 m acima do pivô.

  | Caçamba | Posição | Onde | Pneus mais perto |
  |---|---|---|---|
  | `Cacamba` | (−23,90; 0,26; 56,40) | entrada do terreno baldio, ao lado do monte de areia | pneu, pneu (1), pneu (2) |
  | `Cacamba (2)` | (−45,30; 0,29; −2,70) | grama a oeste da delegacia | pneu (3), pneu (4) |
  | `Cacamba (3)` | (43,90; 0,29; 28,10) | fundo do beco da oficina | pneu (5) |

  O pneu mais longe da caçamba mais perto fica a 20,1 m. Com uma caçamba só, eram 60,8 m.
- **PontoDeCarga** (filho da Main Camera, onde fica o pneu carregado): posição (0,80; −1,00; 1,10) e giro (0; 10; −10).
- **SomDoKit** (no Player):
  - volume 0,5 em cada som curto e 0,25 no loop de segurar;
  - `volumeErro` = 0,25 na cena, porque o `kit_erro` é ~12 dB mais "cheio" que os outros.
- **FluxoDeJogo** (cena): `taxaComTodosOsFocos` 0,003 → **0,00277**. `duracaoInicioCalmo` continua **15**.
- **Layout da HUD**, o mesmo da entrada anterior, agora também gravado pelo builder:
  - `OcupacaoPanel` no centro de cima, em (0; −32), 560×84;
  - a `OcupacaoPanelSombra` vai junto, em (0; −27), 570×94;
  - `ToastPanel` em (0; −126).
- `DengueHUD.textoAoMirar` da cena = `"{nome}"`.

**Simulador (Dengue > Simular Balanceamento), cena final:**
```
[Balanceamento] 22 focos · ocupação inicial 70% · mínima 30% · alívio por foco 2,5%
Taxa depois da rampa: +0,18%/s com todos os focos · a ocupação começa a descer com 7 focos restantes
Ritmo + custo do kit (s/foco; os últimos levam até 1,6x; custo do kit 5,5 s/foco):
  15 s + 5,5 s → vence em 9:40 · pico 71% · final 30%
  20 s + 5,5 s → vence em 12:01 · pico 76% · final 33%
  25 s + 5,5 s → vence em 14:22 · pico 82% · final 38%
  30 s + 5,5 s → vence em 16:44 · pico 88% · final 43%
  35 s + 5,5 s → vence em 19:05 · pico 95% · final 48%
  40 s + 5,5 s → PERDE aos 7:30 com 8/22 focos
  45 s + 5,5 s → PERDE aos 6:08 com 6/22 focos
  50 s + 5,5 s → PERDE aos 5:33 com 5/22 focos
  parado → PERDE aos 3:25 com 0/22 focos
Rota mínima (vizinho mais próximo, correndo a 5,8 m/s, em linha reta): 389 m + cliques ≈ 1:40 = 4,6 s/foco.
Rota com kit (com pneu a 1,96 m/s, sem correr): 676 m correndo + 86 m com pneu + 45 s segurando + 15 s nos pontos
  ≈ 3:40 = 10,0 s/foco (5 reabastecimentos, 3 idas e voltas para esvaziar o saco).
  Custo do kit: (220 s − 100 s) / 22 = 5,5 s/foco.
```

Como a taxa foi escolhida:
- Critérios: ritmo 30 + custo vence com pico ≤ 90%; 35 + custo vence; 40 + custo perde; parado perde entre 2:30 e 3:30. Os quatro passam.
- Com 1 caçamba, o custo do kit era 9,7 s/foco e já se perdia com 30 + custo.
- Com 3 caçambas e a taxa antiga (0,003), 30 + custo dava pico de 95% e 35 + custo perdia.
- A taxa 0,00277 com calmo 15 foi a de maior folga (~1,3 s/foco de custo) numa varredura de taxa × calmo.

**Arquivos novos:**
- Scripts (`Assets/Scripts/`):
  - `Ferramenta.cs`: enums `Ferramenta` e `ResultadoDoPasso`, e os nomes;
  - `EstoqueDoKit.cs`: C# puro, cargas e saco;
  - `SessaoDeSegurar.cs`: C# puro, tempo segurado → passos;
  - `KitDoAgente.cs`: no Player; estoque, ferramenta, pneu e eventos;
  - `PontoDeUso.cs` (base abstrata), `PontoDeReabastecimento.cs` e `Cacamba.cs`;
  - `KitHUD.cs`: barra 1–5, linha do pneu e dica; monta o próprio Canvas;
  - `SomDoKit.cs`: sons do kit, em 2D.
- Editor: `Assets/Editor/KitDoAgenteBuilder.cs`, o menu **Dengue > Configurar Kit do Agente**.
- Testes EditMode (`Assets/Editor/Testes/`, todos com `[Category("ParteA")]`), 51 no total:

  | Classe | Testes |
  |---|---|
  | `EstoqueDoKitTestes` | 8 |
  | `SessaoDeSegurarTestes` | 7 |
  | `KitDoAgenteTestes` | 7 |
  | `InteractableKitTestes` | 9 |
  | `PneuCarregarTestes` | 8 |
  | `GarrafaSacoTestes` | 3 |
  | `PontosDeUsoTestes` | 7 |
  | `KitHUDTestes` | 1 |
  | `HistoriaKitTestes` | 1 |

  O helper é o `TesteUtil.cs`.
- Sons (`Assets/Meus/Assets Gerados/Sons/`, gerados por `Ferramentas/Audio/gerar_sons.py --so ...`): `kit_trocar`, `kit_erro`, `kit_reabastecer`, `cacamba_despejar`, `pneu_pegar`, `pneu_largar` e `kit_segurar_loop` (loop de 2 s, Vorbis).
- Material: `Assets/Meus/Assets Gerados/Cacamba/Cacamba.mat` (URP Lit, #E8A21F, com emissão para o destaque da mira).

**Arquivos alterados:**
- `Interactable.cs`:
  - campos `ferramentaNecessaria` e `segundosParaConcluir`;
  - `Verificar(kit)` e `AvancarCom(kit)`, mais os hooks `VerificarExtra`, `AoAvancar`, `ConcluiSozinho` e `AoTerminarAcao`;
  - `SegundosPorPasso`, `Zumbe` e `Concluir()`;
  - `IsComplete` virou uma flag;
  - o `Interact()` continua como caminho antigo, sem kit.
- `PneuInteractable.cs`:
  - `Esvaziado` e `Carregado`;
  - o último passo põe o pneu nos braços, e quem conclui é o `Entregar` na caçamba;
  - `PrenderEm`, `Largar` (com raycast no peito para não atravessar muro) e `Entregar`.
- `GarrafaInteractable.cs`: `segundosPorGarrafa`, `SacoCheio` no `VerificarExtra`, cada passo guarda uma garrafa no saco, e `ContarGarrafas()`.
- `Interactor.cs`, reescrito:
  - segurar em vez de clicar;
  - conferência no aperto, com os eventos `OnRecusado` e `OnPontoRecusado`;
  - `PontoAlvo`, `ProgressoContinuo` e `Avancando`;
  - troca de ferramenta e largar;
  - o raio atravessa foco concluído.
- `EntradaJogador.cs`: `InteragirSegurado`, `LargarPressionado`, `FerramentaPedida` (teclas 1–5) e `TrocaDeFerramenta` (roda, Next e Previous).
- `ControladorJogador.cs`: `MultiplicadorVelocidade` e `PodeCorrerEPular` (o pulo que ficou no buffer é descartado enquanto não pode pular).
- `DengueHUD.cs`:
  - mira com `{nome}` + "segure o clique";
  - barra contínua;
  - toasts de recusa;
  - painel do ponto de uso e o "Pneu vazio: leve à caçamba".
- `FocoHUDInfo.cs`: `textoFerramentaErrada`.
- `HistoriaRedeSocial.cs`:
  - `aoUsarFerramentaErrada` por personagem;
  - 3 listas novas: `aoReabastecerPrimeiraVez`, `aoCarregarPneu` e `aoEsvaziarSacoPrimeiraVez`;
  - o "largou pela metade" ignora o pneu esvaziado.
- `SomDosFocos.cs`: só zumbe foco com `Zumbe`.
- `Assets/InputSystem_Actions.inputactions`, mapa Player:
  - `<Keyboard>/1` e `/2` saíram de Previous e Next;
  - LB/RB entraram em Previous e Next;
  - ação nova `Largar`: botão direito, G e botão Norte.
- Editor:
  - `SomBuilder.cs`: `LigarKit`;
  - `PersonagensHospitalBuilder.cs`: `FalaEnfermeira` virou `public const`, com a fala nova;
  - `BalanceamentoSimulador.cs`: rota com kit, custo do kit e pneu → caçamba mais perto.
- `Ferramentas/Audio/gerar_sons.py`: 7 receitas novas. Nenhum som antigo mudou.
- Prefabs de foco (ferramenta, tempo e textos): `Assets/Meus/Vaso de Flor/Vaso de Flor.prefab`, `Assets/Meus/Caixa d'Agua.prefab`, `Assets/Meus/Assets Gerados/Focos/Ralo.prefab`, `Assets/Meus/Assets Gerados/Focos/Bromelia.prefab`, `Assets/Meus/Assets Gerados/Piscina de Plastico/Piscina de Plastico.prefab` e `Assets/Piscina de Plastico.prefab`.
- `Assets/Scenes/SampleScene.unity`, salva pelo builder:
  - Player com `KitDoAgente`, `SomDoKit` e `Main Camera/PontoDeCarga`;
  - `--- UI ---/KitHUD`;
  - os 22 focos com ferramenta, tempo e `FocoHUDInfo` (os pneus ganharam o deles);
  - Picape da UPA, as 3 caçambas e o monte de areia;
  - a fala da Rita, o layout da HUD e a taxa do `FluxoDeJogo`.
- `ProjectSettings/Packages/com.unity.probuilder/Settings.json`: ruído, chaves padrão que o próprio ProBuilder grava. Ficou no índice.
- Os arquivos da entrada anterior (layout da HUD: `FluxoDeJogoHUDBuilder.cs`, `HUDLayoutAjustador.cs`, `DengueHUDBuilder.cs`, `DengueHUDReparo.cs` e os docs `hud-ocupacao-superior`) são de **outra sessão** e não foram alterados aqui. Também entraram no índice.

**Decisões tomadas durante a execução (e por quê):**
- **Sem worktree e sem commit.** Tudo foi feito no próprio checkout, numa branch nova, porque o Editor está ligado a esta pasta e só aqui dá para compilar e testar. O índice do git serviu de base de cada tarefa, e o plano proíbe commit sem pedido do usuário.
- **Testes não dependem de corrotina.** Os testes do `Interactable` zeram `punchScale` e `shrinkDuration`, porque no EditMode as corrotinas não rodam. O teste com lista (personagens) monta o campo por `SerializedObject`, porque o `TesteUtil.Setar` não aceita listas.
- **`Interactor` robusto a "null falso" e à pausa.** Um alvo destruído vira `null` de verdade antes da `SessaoDeSegurar`, que compara por referência. Com `deltaTime <= 0` (pausa), nenhum passo é dado e nenhum aviso sai.
- **Menos redesenho e pulo travado com o pneu.** O `OnMudou` só dispara em mudança real, para a HUD não redesenhar à toa. O pulo que fica no buffer é descartado enquanto carrega pneu, porque a spec diz "não pula". A animação e os passos continuam lendo a velocidade base, e com 0,7× soam como "andar devagar", coerente com carregar peso.
- **Conferência e consumo.** O `Verificar` confere a carga da ferramenta **selecionada**, e o `AvancarCom` gasta a `FerramentaNecessaria`; hoje são sempre a mesma. A carga é gasta depois do `OnCompleted`, porque ninguém lê o estoque nele e a HUD escuta o `OnMudou`.
- **Pneu.** Ao largar, um raycast na altura do peito evita que o pneu atravesse o muro; sem isso, um pneu largado podia ficar inalcançável e a partida, impossível. No caminho antigo `Interact()` (sem kit), o pneu conclui como antes. Uma segunda entrega não dispara outro `OnCompleted`.
- **Pneu esvaziado e largado.** A HUD mostra "Pneu vazio: leve à caçamba", sem barra nem "5/5", e o post "largou pela metade" não sai: a água já saiu.
- **Pontos de uso.**
  - Motivo vazio = recusa sem aviso.
  - O raio do `Interactor` atravessa foco concluído, porque o pneu entregue encolhe em cima da caçamba.
  - Um uso por aperto, para não repetir o reabastecer e logo depois mostrar "kit já está cheio".
  - Precisam de collider sólido (o raio ignora triggers) e de `_EmissionColor` para o destaque.
- **`MaosOcupadas` ficou inalcançável pelo `Interactor`.** Com o pneu nas mãos os focos são atravessados. A linha "Carregando pneu" da KitHUD já orienta, e o mapeamento do texto ficou, inofensivo.
- **"Sai uma vez" pelo índice.** Os posts de reabastecer, pneu e saco usam o índice do `PostarDaLista` (anda só quando o post sai). Com 1 post por lista, é o mesmo que um bool. O de ferramenta errada é marcado por **tipo**, e só quando o post sai.
- **`volumeErro` 0,25 na cena**, porque o `kit_erro` soava ~12 dB acima dos outros curtos.
- **Layout da HUD.** O builder grava o MESMO layout que o usuário pediu na outra sessão e leva junto a `OcupacaoPanelSombra`, que o `HUDLayoutAjustador` esquecia. O builder não chama o ajustador, porque ele abre um diálogo modal que trava o menu rodado pela linha de comando.
- **3 caçambas em vez de 1.** Os 6 pneus estão espalhados pelas duas quadras, até 91 m um do outro. Com uma caçamba só, a viagem mais longa carregando era de ~61 m, contra a intenção da spec ("caçamba perto dos pneus"). Critério: nenhum pneu a mais de 30 m da caçamba mais perto.
- **Simulador pessimista de propósito.** O custo do kit entra antes do `FatorUltimos` (os últimos focos levam até 1,6×), então o simulador fica do lado seguro.
- **Pose do pneu nos braços** decidida pelo critério do plano (canto inferior direito, sem cobrir a mira), conferida em captura. O pneu deitado na frente da câmera tapava metade da tela.

**Verificado:**
- 51/51 testes EditMode `ParteA` passando (`Failed: 0`). Comando: `unity command run_tests --mode EditMode --filter_type category --filter ParteA`.
- Play Mode na `SampleScene`, com `eval_file`:
  - preparação: abertura pulada (cartelas paradas, `pausouJogo = false`), `timeScale = 1`, `Interactor.Bloqueado = false`, `ControleAtivo = false`, `fluxo.IniciarPartida()` e `runInBackground` ligado só no teste;
  - logs `[T14]` nos eventos do kit, do `Interactor`, da `RedeSocial` e do `FluxoDeJogo`.
- **Cada tipo de foco** (vaso, ralo, bromélia, caixa d'água, garrafas e pneu), com `AvancarCom` pelo kit da cena:
  - com uma ferramenta errada: `FerramentaErrada`, e `CurrentClicks` não mudou;
  - com a da tabela: concluiu (o pneu deu `Pegou`).
- **Recusa pelo `Interactor`:** agente teleportado e mirando a bromélia e o ralo, com as Mãos; a conferência do aperto (`Interactor.Conferir`, por reflexão):
  - `OnRecusado(FerramentaErrada)`;
  - toast com o `textoFerramentaErrada` do foco;
  - `CurrentClicks` 0 → 0.
- **Estoque:**
  - depois de 1 vaso, Areia 0, e o 2º vaso dá `SemCarga`;
  - o monte de areia volta a Areia para 1 e, de novo, recusa com "Seu kit já está cheio.";
  - a picape encheu areia, tela e água (0/0/1 → 1/1/2) e depois recusou com "Seu kit já está cheio.".
- **Pneu:**
  - pegar não mexe em `FocosEliminados` (5 → 5);
  - nos braços: `Zumbe == false`, multiplicador 0,7, sem correr e pular, `Selecionar` recusado e a linha do pneu da KitHUD ativa;
  - largar devolve o movimento, e pegar de novo é instantâneo;
  - a caçamba entrega e conta (5 → 6).
- **Saco:**
  - 'Garrafa ' (5) concluiu com o saco em 5/6;
  - a 'Lata 001' avançou 1 e deu `SacoCheio`;
  - a `Cacamba (3)` esvaziou e, vazia, recusou com "Nada para jogar na caçamba.";
  - a lata parou de novo no saco cheio, a `Cacamba (2)` esvaziou, e a lata concluiu.
- **Rede social:** os 4 posts novos saíram, **uma vez cada**:
  - ferramenta errada: uma vez **por tipo**. A Lúcia saiu na bromélia; a 2ª recusa na bromélia, 4 s depois, não repetiu; o Seu Osvaldo saiu no ralo;
  - reabastecer (UPA): saiu no 1º reabastecimento, no monte; a picape, depois, não repetiu;
  - carregando pneu (Carlos): saiu no 1º pego; pegar de novo não repetiu;
  - 1º saco na caçamba (Bruna): saiu no 1º esvaziamento; o 2º não repetiu.

  Antes de cada gatilho foi conferido `RedeSocial.Aceitaria(Normal)`, esperando o intervalo de 4 s quando preciso.
- **Fim:**
  - os 22 focos eliminados pelo kit (3 idas à picape e 6 à caçamba no resto) → `OnVitoria` (`FocosEliminados` 22/22, ocupação final 37%);
  - a tela final ficou segurada enquanto o epílogo tocava nas cartelas;
  - pulando o epílogo (o flag do Esc), a tela "MISSÃO CUMPRIDA! · Focos eliminados: 22/22" apareceu (alpha 1).
- **Console do Play:** só os 12 erros antigos de `Default-Particle.psd` (2 por pneu concluído com a animação padrão). Nenhum aviso.
- **Limpeza:**
  - `editor_stop` e `PlayerSettings.runInBackground = false` (`runInBackground: 0` no disco);
  - a `SampleScene` não ficou suja (o Play reverte; nada foi salvo depois);
  - **só a `MenuInicial` aberta** no Editor.

**NÃO verificado:**
- **Partida real com teclado, mouse e controle.** O Editor sem foco descarta a entrada, então nada disso foi testado na mão: segurar de verdade, teclas 1–5, roda, LB/RB, botão direito/G/Norte para largar.
- O **"apertado" do estoque** e o ritmo de ir e voltar à picape e às caçambas: só simulados.
- A **margem do balanceamento é estreita**. A folga é de ~1,3 s/foco de custo do kit, e o parado perde aos 3:25 para um limite de 3:30. Falta o usuário jogar e dizer se ficou moderado.
- Os sons novos não foram ouvidos (a emenda do loop depois da compressão Vorbis, inclusive). A ligação deles foi conferida em Play nas tarefas anteriores.

**Pendências encontradas na verificação (não corrigidas):**
- **Post de reabastecer com texto da picape.** (corrigido na revisão final) Ele sai no 1º reabastecimento, onde quer que seja. Se for no **monte de areia**, que fica perto dos vasos e é um caminho provável, o texto da UPA diz "passou na picape da UPA pra pegar material. Areia, tela e água sanitária...", o que não bate com o monte. Opções: ligar o post só à picape (por exemplo, um campo no `PontoDeReabastecimento`) ou trocar o texto.
- **'Lata 001' deixa uma garrafa visível** (corrigido na revisão final), um problema da cena anterior à Parte A. A instância tem `cliquesIgualAoNumeroDeGarrafas = false` e `requiredClicks` 11 para 12 garrafas:
  - ela conclui com 11 passos (11 garrafas no saco), e 1 garrafa fica na cena (`GarrafasRestantes = 1`);
  - correção provável: ligar `cliquesIgualAoNumeroDeGarrafas` nessa instância (12 passos).
- **Aviso de saco cheio desatualizado.** (corrigido na revisão final) Ele ainda diz "Esvazie na caçamba do terreno baldio" (`DengueHUD.RecusaSacoCheio`, `const`), mas agora são 3 caçambas e só uma fica no terreno baldio. Basta trocar por "numa caçamba".
- **Pequenas, conhecidas, deixadas para depois:**
  - com o clique esquerdo apertado, largar o pneu faz pegá-lo de novo na hora, e depois do pego a caçamba na mira entrega sem soltar o botão (exigir soltar depois de `Concluiu`/`Pegou`);
  - trocar de ferramenta ainda no hospital marca a dica "1–5" como vista antes de ela aparecer (corrigido na revisão final);
  - a roda contínua (touchpad) troca várias ferramentas por gesto;
  - o pneu carregado (~2 m) atravessa paredes visualmente e é maior que a caçamba na entrega;
  - o `Cacamba.mat` é regravado com ruído de float a cada execução do builder.

**Observações para futuras sessões de IA:**
- **`PontoDeUso` não herda `Interactable`, e não pode herdar.** `FluxoDeJogo`, `DengueHUD`, `FocoListaHUD`, `RedeSocial` e `HistoriaRedeSocial` varrem `Interactable` como foco, e a picape e a caçamba virariam focos.
- **A ordem dos enums `Ferramenta` e `ResultadoDoPasso` é fixa.** Enum serializado é salvo como número (cena e prefabs: Mãos 0, Areia 1, Tela 2, AguaSanitaria 3, SacoDeLixo 4), então valor novo só entra no FIM, como no `TipoFoco`. A barra 1–5 segue a ordem do enum.
- **O builder `Dengue > Configurar Kit do Agente` pode rodar de novo.** Ele acha pelo nome o que já criou, atualiza campos, textos e poses sem duplicar, regrava prefab só se algo mudou e salva a cena.
  - Para ajustar ferramenta, tempo e textos de um tipo, mude a tabela `Focos`.
  - Caçambas: tabela `Cacambas`. Pose do pneu: `PosicaoDaCarga` e `RotacaoDaCarga`.
  - Depois rode o menu e `Dengue > Simular Balanceamento`.
- **O layout da HUD também é gravado pelo builder**, com os MESMOS valores do `HUDLayoutAjustador`: ocupação em y −32, sombra em −27 e toast em −126. Se mudar um, mude o outro. O builder leva a sombra junto; o ajustador não leva.
- **A cena não tem piscina** (22 focos). Os prefabs de piscina já estão configurados (Mãos, 2,5 s) para quando ela voltar.
- **Testes EditMode** em `Assets/Editor/Testes/`, todos com `[Category("ParteA")]`.
  - Para rodar: Window > General > Test Runner > EditMode, ou `unity command run_tests --mode EditMode --filter_type category --filter ParteA`. Esperado: 51, `Failed: 0`.
  - No EditMode, `Awake`/`Start` não rodam: o `KitDoAgente` cria o estoque e acha o `ControladorJogador` no primeiro acesso, e os testes mudam campos serializados com `TesteUtil.Setar`, que usa `SerializedObject`.
  - Objetos de teste nascem `HideAndDontSave` e são destruídos no `[TearDown]` (`TesteUtil.LimparTudo()`), para não sujar a cena aberta.
- **Para testar em Play sem input:**
  - pule a abertura, como na preparação acima;
  - chame `AvancarCom(kit)` nos focos e `PodeUsar`/`Usar(kit)` nos pontos;
  - para a recusa com toast, mire de verdade (`Teleportar` + `DefinirOlhar`, um frame) e chame `Interactor.Conferir` por reflexão.
  - Posts novos precisam de `RedeSocial.Aceitaria(...)` verdadeiro na hora: espere ~4 s depois de outro aviso.
- `SegundosPorPasso` da garrafa é por garrafa: o `segundosParaConcluir` não vale nela. O simulador usa `SegundosPorPasso × passos`.

**Correções da revisão final** (2026-10-29, mesma branch, sem commit)

**Objetivo:** aplicar a rodada única de correções da revisão final da branch (lista `.superpowers/sdd/2026-10-25-parte-a-kit-do-agente/final-fixes.md`, itens A–O): três defeitos importantes confirmados (A, B, C), as pendências P1–P3 desta entrada (D, E, F), o que a triagem mandou corrigir antes do merge (G, H, I) e menores baratos nos mesmos arquivos (J–O).

**O que mudou, por item:**
- **A — "largou pela metade" com o saco cheio** (`HistoriaRedeSocial.cs`). O `VerificarLargados` agora ignora, além do pneu esvaziado, o grupo de garrafas enquanto `kit.Estoque.SacoCheio` (método novo `SaidaObrigatoria`) e o tira do `pertoDoComecado`. Sair para esvaziar o saco numa caçamba não dá mais o post da Bruna. Voltar com espaço no saco e abandonar o grupo continua dando.
- **B — o pneu chegava aos braços com 20% do tamanho** (`Interactable.cs`). Os 6 pneus da cena têm `scaleReductionPerClick` 0,2 e 5 cliques, e cada passo encolhia o pneu. Agora o encolher por clique não vale para foco que não conclui sozinho (`ConcluiSozinho = false`, o pneu). O pneu é pego, carregado, largado e entregue com a escala dele (100 na cena), e o `alturaDoPivo` do `PrenderEm` é medido com ela.
- **C — o pneu largado nascia dentro do agente** (`PneuInteractable.cs`, `KitDoAgente.cs`). O `Largar(pes, frente)` virou `OndeCai(pes, frente, raioDoAgente, out posicao, out giro, out cacamba)`, que calcula sem mexer no pneu, mais `Largar(posicao, giro)`.
  - **Giro:** o pneu cai de pé, com o giro de quando estava no chão virado só em torno do vertical, para o eixo da roda apontar para a frente do agente. O giro é sempre o menor (até 90°), e um pneu deitado fica como estava. O eixo é o `EixoDaRoda` = X local do `rubber tries.fbx`, conferido no asset: banda de 0,58 m no X e disco de 2,02 m no plano YZ, com o pivô no centro.
  - **Avanço pelo tamanho real:** `raioDoAgente` (raio do CharacterController × escala + pele: 0,3 + 0,03 m no Player) + `FolgaDoAgente` 0,15 m + a meia profundidade do pneu, medida nos cantos das caixas locais dos Renderers ligados. No pneu da cena, o pivô cai a ~0,77 m dos pés, e a borda de cá a ~0,48 m.
  - **Muro:** a proteção continua. O raio no peito vai até a borda de lá do pneu + `FolgaDoMuro`, que agora vale 0,1 m da borda de lá até o muro (antes, 0,35 m do centro). A constante `DistanciaAFrente` (0,8 m fixo) saiu.
- **D — o post da picape saía no monte de areia** (`KitDoAgente.cs`, `PontoDeReabastecimento.cs`, `HistoriaRedeSocial.cs`, `SomDoKit.cs`).
  - `KitDoAgente.OnReabasteceu` virou `Action<PontoDeReabastecimento>`, e `Reabastecer(ferramentas, origem = null)` recebe o ponto. O `PontoDeReabastecimento` passa a si mesmo.
  - Campo novo `ehPicapeDaUpa` (`EhPicapeDaUpa`), que o builder grava: `true` na picape e `false` no monte.
  - O `HandleReabasteceu` só posta quando a origem é a picape. O `SomDoKit` toca em qualquer ponto, como antes.
- **E — a 'Lata 001' terminava com 1 garrafa na cena** (`KitDoAgenteBuilder.cs`). O `AplicarFoco` grava `cliquesIgualAoNumeroDeGarrafas = true` em todo `GarrafaInteractable`. Na cena, só a Lata 001 mudou (0 → 1). No Play ela tem 12 passos, um por garrafa.
- **F — aviso de saco cheio** (`DengueHUD.cs`): agora diz "Saco cheio. Esvazie numa <b>caçamba</b>.", porque são 3 caçambas.
- **G — a dica 1–5 era apagada no hospital** (`KitHUD.cs`). A troca de ferramenta só marca a dica como vista com a barra à mostra (`BarraAMostra`: partida iniciada, sem pausa e com o controle ligado), a mesma condição que mostra a barra. Quem aperta 1–5 ouvindo a Rita ainda vê a dica no bairro.
- **H — builder com cenas aditivas** (`KitDoAgenteBuilder.cs`). A cena configurada é a do `FluxoDeJogo` achado (`cenaDoJogo`), e não a cena ativa. Essa cena vale para:
  - as buscas (`DaCena<T>`, `PrimeiroDaCena<T>` e `AcharNaCena`);
  - as criações (`NovoNaCena` e `InstantiatePrefab(prefab, cena)`);
  - o `SaveScene`.
  A `Camera.main` só vale se for dessa cena.
- **I — testes do Largar com chão:** os testes novos de B, C e M têm chão de teste em `SemChao`.
- **J — sem areia** (`DengueHUD.cs`): agora diz "Acabou: <b>Areia</b>. Reabasteça na picape da UPA ou no monte de areia do terreno baldio." As outras ferramentas continuam mandando só para a picape.
- **K — dica de correr carregando pneu** (`DicaCorrer.cs`): a dica não aparece, e o tempo não conta, enquanto `ControladorJogador.PodeCorrerEPular` é falso.
- **L — bromélia é tratada com água sanitária** (`KitDoAgenteBuilder.cs`, `HistoriaRedeSocial.cs`, prefab da bromélia, cena):
  - **Dica:** vem da coluna nova `dica` da tabela `Focos`: "Bromélia guarda água entre as folhas. Regue o copinho com água sanitária diluída (1 colher de sopa por litro) duas vezes por semana." Fica no `Bromelia.prefab`, e as 4 instâncias herdam.
  - **1ª fala da Lúcia ao eliminar:** a constante `HistoriaRedeSocial.FalaLuciaPrimeiraBromelia` diz "Ufa! O agente não arrancou nada: só pôs água sanitária diluída no copinho, e a bromélia ficou inteirinha." Ela vale no padrão do código e é gravada no personagem salvo na cena pelo passo 8 novo do builder (`ConfigurarLucia`, via `SerializedObject`).
- **M — largar em cima da caçamba** (`KitDoAgente.cs`, `PneuInteractable.cs`, `Cacamba.cs`). Se o raio do chão acerta uma `Cacamba` (ou o do peito, com o agente encostado nela), o `LargarPneu` entrega: chama `EntregarPneu(cacamba.PontoDeDescarte)`, e o foco conclui. `Cacamba.PontoDeDescarte` (pivô + `alturaDescarte`) também é usado pelo `Usar`.
- **N — escala da KitHUD** (`KitHUD.cs`): `CanvasScaler.matchWidthOrHeight = 0.5`, como os outros Canvas do jogo. Pela conta das escalas (não conferido em captura), a dica 1–5 continua sem encostar na DicaCorrer, que usa 0: sobram ~17 px em 21:9 e ~21 px em 4:3.
- **O — ralo com 8 passos** (`KitDoAgenteBuilder.cs`, coluna nova `passos`; `Ralo.prefab`): `requiredClicks` passou de 3 para 8, e o tempo total continua 2,0 s (8 × 0,25 s). Ao soltar, a barra volta no máximo 1/8. As 4 instâncias herdam do prefab.

**Arquivos alterados:**
- Scripts: `Interactable.cs`, `PneuInteractable.cs`, `KitDoAgente.cs`, `Cacamba.cs`, `PontoDeReabastecimento.cs`, `SomDoKit.cs`, `HistoriaRedeSocial.cs`, `DengueHUD.cs`, `KitHUD.cs` e `DicaCorrer.cs`.
- Editor: `KitDoAgenteBuilder.cs`.
- Testes:
  - `PneuCarregarTestes.cs`: 4 casos novos e os helpers `CriarChao`, `RodarStart`, `CorpoComoNaCena`, `DarFormaDePneu`, `DistanciaNoChao` e `AssertEscala`; o teste do muro passou a usar o `CriarChao`;
  - `PontosDeUsoTestes.cs`: 1 teste novo;
  - `KitDoAgenteTestes.cs` e `PontosDeUsoTestes.cs`: os assinantes do `OnReabasteceu` com o parâmetro novo.
- Assets, pelo builder:
  - `Assets/Meus/Assets Gerados/Focos/Ralo.prefab`: `requiredClicks` 8;
  - `Assets/Meus/Assets Gerados/Focos/Bromelia.prefab`: a dica;
  - `Assets/Scenes/SampleScene.unity`: 4 linhas, conferidas contra o índice — `ehPicapeDaUpa` 1 na picape e 0 no monte, `cliquesIgualAoNumeroDeGarrafas` 1 na Lata 001 e a fala da Lúcia.
- `Cacamba.mat`: só o ruído de float de sempre (R13-6, adiado).
- Não mexidos (outra sessão): `FluxoDeJogoHUDBuilder.cs`, `HUDLayoutAjustador.cs`, `DengueHUDBuilder.cs`, `DengueHUDReparo.cs` e os docs `hud-ocupacao-superior`.

**Testes:** 56/56 ParteA (`Failed: 0`), os 51 de antes mais 5 novos:

| Teste | Item |
|---|---|
| `PneuCarregarTestes.PegarELargar_ComEncolherPorClique_MantemOTamanho` | B, com chão |
| `PneuCarregarTestes.Largar_PneuGrande_NaoNasceDentroDoAgente` | C, com chão |
| `PneuCarregarTestes.Largar_NaCacamba_Entrega(0,8)` e `(1,2)` | M: o raio do chão acerta o topo / o do peito acerta a parede |
| `PontosDeUsoTestes.Reabastecer_AvisaOPontoDeOrigem` | D |

Os testes de B, C e M foram escritos antes das correções e falharam (RED, 55 testes e 4 falhas):
- B: "nos braços: escala (0.26, 0.26, 0.26), esperada (1.30, 1.30, 1.30)";
- C: a cápsula do agente cruzava o pneu largado;
- M: "largado na caçamba, conta como entregue: esperado True, veio False", nos 2 casos.

O teste de B chama o `Start` por reflexão, para o pneu guardar a escala inicial como no Play.

**Builder e simulador:**
- `Dengue > Configurar Kit do Agente` rodou 3 vezes: 2 com a SampleScene sozinha e 1 com a MenuInicial ativa e a SampleScene aditiva.
  - O resumo `[Kit]` saiu igual nas 3 vezes, e não há duplicata (KitHUD 1, Cacamba 3, PontoDeReabastecimento 2, FocoHUDInfo 22).
  - Na rodada aditiva, a MenuInicial continuou limpa: 686 objetos, nada do builder e não suja.
- `Dengue > Simular Balanceamento`, com a taxa mantida em 0,00277:
```
[Balanceamento] 22 focos · ocupação inicial 70% · mínima 30% · alívio por foco 2,5%
Taxa depois da rampa: +0,18%/s com todos os focos · a ocupação começa a descer com 7 focos restantes
Ritmo + custo do kit (s/foco; os últimos levam até 1,6x; custo do kit 5,2 s/foco):
  15 s + 5,2 s → vence em 9:34 · pico 71% · final 30%
  20 s + 5,2 s → vence em 11:55 · pico 75% · final 33%
  25 s + 5,2 s → vence em 14:16 · pico 81% · final 38%
  30 s + 5,2 s → vence em 16:38 · pico 88% · final 43%
  35 s + 5,2 s → vence em 19:00 · pico 95% · final 48%
  40 s + 5,2 s → PERDE aos 7:31 com 8/22 focos
  45 s + 5,2 s → PERDE aos 6:08 com 6/22 focos
  50 s + 5,2 s → PERDE aos 5:33 com 5/22 focos
  parado → PERDE aos 3:25 com 0/22 focos
Rota mínima (vizinho mais próximo, correndo a 5,8 m/s, em linha reta): 389 m + cliques ≈ 1:45 = 4,8 s/foco.
Rota com kit (com pneu a 1,96 m/s, sem correr): 676 m correndo + 86 m com pneu + 46 s segurando + 15 s nos pontos
  ≈ 3:40 = 10,0 s/foco (5 reabastecimentos, 3 idas e voltas para esvaziar o saco).
  Custo do kit: (221 s − 105 s) / 22 = 5,2 s/foco.
```
  - Os 4 critérios continuam passando: 30 + custo vence com pico de 88%; 35 + custo vence; 40 + custo perde aos 7:31; parado perde aos 3:25. A taxa não mudou.
  - O custo do kit caiu de 5,5 para 5,2 s/foco quase só pela referência. A rota mínima (o jogo antigo, 0,25 s por clique) passou a contar 8 cliques por ralo (+5 s). A rota com kit ganhou só 1 s, a 12ª garrafa da Lata 001.

**Verificado em Play** (SampleScene sozinha; `runInBackground` ligado só no teste; abertura pulada como na T14; partida iniciada por `fluxo.IniciarPartida()`):
- **G:** ainda no hospital, `Selecionar(Areia)` não marcou a dica. Na partida, a troca marcou.
- **N:** `matchWidthOrHeight` = 0,5 na KitHUD.
- **E e O:** a Lata 001 tem 12 passos; o ralo tem 8 passos de 0,25 s.
- **B e C:** testados no 'pneu' (de pé, sem giro) e no 'pneu (4)' (inclinado, giro (14, 89, 330)), com o agente no lugar do pneu e virado ao longo do disco (o pior caso de antes).
  - A escala ficou 100 nos braços e no chão.
  - O pneu caiu com +90° só em Y, a inclinação original mantida.
  - A cápsula do CharacterController não tocou o MeshCollider. A borda de cá ficou a 0,48 m do agente no 'pneu' e a 0,56 m no 'pneu (4)'.
  - 2 s depois, o agente não tinha saído do lugar (0,000 m).
- **M:** o agente encostado na `Cacamba`, com o topo dela a 0,98 m dos pés: o raio do peito passa por cima, que é o caso do achado. Largar entregou: `FocosEliminados` foi de 0 a 1, com `OnPneuEntregue` e sem `OnPneuLargado`.
- **D:** com a rede aceitando posts, o monte de areia reabasteceu sem post. A picape deu o post da UPA.
- **A:** com a 'Garrafa  (1)' começada e o saco cheio, sair para 30 m não deu post. Com espaço no saco, voltar e sair deu o post da Bruna.
- **K:** carregando pneu, o tempo da dica de correr ficou em 0 por 6 s e a dica sumiu. Sem o pneu, voltou a contar.
- **Console do Play:** só os 2 erros antigos de `Default-Particle.psd` (um pneu concluído).
- **Limpeza:**
  - `editor_stop` e `PlayerSettings.runInBackground = false` (o `ProjectSettings.asset` não mudou);
  - a SampleScene não ficou suja;
  - **só a MenuInicial aberta** no Editor.

**Fora desta rodada (adiados, continuam no registro):**
- **codigo F3 / unity U-F2:** trocar de ferramenta com o botão ainda apertado não confere o alvo de novo; é preciso soltar e apertar.
- **codigo F4:** uma recusa do kit apaga o aviso de foco eliminado, e a dica junto.
- **spec F6:** a fala longa da Rita pode entrar no teto da sala vista de longe; falta conferir em captura.
- **spec F7:** há um loop de "segurar" só, igual para todas as ferramentas.
- **unity U-F4:** o destaque de mira liga `_EMISSION` em runtime, e a variante pode faltar na build. Além disso, a frase "com emissão para o destaque da mira" sobre o `Cacamba.mat`, mais acima nesta entrada, não confere com o asset.
- **U8-2:** com o esquerdo apertado, largar faz pegar o pneu de novo, e depois do `Pegou` a caçamba na mira entrega sem soltar.
- **Os demais "minor (deferred)" do ledger** (`.superpowers/sdd/2026-10-25-parte-a-kit-do-agente/progress.md`), entre eles:
  - T5-3, o `PararPunch` não restaura a escala;
  - U4, o `Entregar` religa os colliders;
  - U2, o fallback "pes" religa os colliders sobre o CharacterController;
  - U12-5, o pneu de ~2 m nos braços;
  - R13-6, o ruído do `Cacamba.mat`;
  - a margem estreita do balanceamento;
  - as lacunas de cobertura de teste.

**Observações para futuras sessões de IA:**
- **O eixo da roda é o X local do `rubber tries.fbx`** (`PneuInteractable.EixoDaRoda`). Se o modelo do pneu mudar, confira o eixo; a folga do largar já sai do tamanho real dos Renderers.
- **Largar o pneu em cima de uma caçamba é entrega.** Quem cuida disso é o `KitDoAgente.LargarPneu` (o `OndeCai` devolve a caçamba). A entrega pela mira continua igual.
- **A dica da bromélia e a 1ª fala da Lúcia são gravadas pelo builder,** como a fala da Rita. Uma edição no Inspector volta ao texto do builder quando o menu roda de novo. Para mudar de vez, edite a tabela `Focos` ou a constante `FalaLuciaPrimeiraBromelia`.
- **Outras falas da Lúcia ainda falam em "esvaziar" a bromélia:** `aoLargarPelaMetade`, `aoEliminarUltimo` ("jato de mangueira") e o epílogo. Ficaram de fora do item L, que pedia só a dica e a 1ª fala.
- **O `OnReabasteceu` leva o ponto de origem.** Assinante novo usa `Action<PontoDeReabastecimento>`.
- **A rota mínima do simulador conta cliques** (0,25 s cada). Mudar o número de passos de um foco mexe nessa referência, e por isso no "custo do kit", mesmo sem mudar a rota real.
