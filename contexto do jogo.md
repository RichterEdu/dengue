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


### 2026-10-29 — Spec da Parte B (focos vivos, chuva e entregas com peso)

**Objetivo (pedido do usuário):** seguir o roteiro com a Parte B. O brainstorming terminou com a spec aprovada seção por seção.

**Escolhas do usuário:**
- **Papel da chuva:** o atalho que cobra depois. Esvaziar com as Mãos (vaso, pneu e garrafas) é rápido e não gasta carga, mas a chuva reenche e o foco volta como larva. A ferramenta certa resolve de vez.
- **Ciclo do foco:** ovo → larva → mosquito, cerca de 75 s por etapa. O foco com mosquito **pesa mais na barra** (pesos 1 / 1,5 / 2,5), mas não espalha. O estágio aparece **no mundo** (enxame) e **na mira**, nunca na lista.
- **Foco esvaziado:** alivia a barra, mas não conta para a vitória.
- **Chuva:** a cada 3 a 4 min, com previsão no celular 45 s antes.
- **Arquitetura:** um **gerenciador central de focos** (`GerenciadorDeFocos`) substitui as 7 varreduras de `Interactable`.
- **Feedback com muita importância**, sobretudo carregando. O agente **segura o pneu com os braços** (pose nova no Blender e camada no Animator), sem ícone flutuante.
- **Destinos (mudam a Parte A):**
  - pneus vão para a **caçamba da picape da UPA** e ficam empilhados lá;
  - garrafas vão para as **lixeiras de rua** (as 3 que já existem e mais 2 novas);
  - as 3 caçambas de entulho saem da cena.
- **Pneus em tamanho real:** passam de cerca de 2 m de diâmetro para cerca de 0,7 m. A medição na cena mostrou 2,02 m num agente de 1,88 m e numa picape de 2,05 m de altura.

**Arquivo novo:** `docs/superpowers/specs/2026-10-29-parte-b-focos-vivos-e-chuva-design.md`.

**Nenhum script, cena ou asset foi alterado nesta entrada.** O próximo passo é o plano de implementação (`writing-plans`).

**Medições feitas na cena (somente leitura, com a `SampleScene` aberta como aditiva e fechada sem salvar):**
- distâncias de cada pneu até a picape da UPA (-2,2; -19,2): 36 a 96 m; até a caçamba mais perto: 4 a 20 m;
- lixeiras de rua: `Trassh Bin` (7, -6), `Trassh Bin (1)` (-10, -15) e `Trash_01` (-20, 24). `Lixo` (-39, 54) é um monte do terreno baldio, não uma lixeira.

**Observações para futuras sessões de IA:**
- O rig do agente (`AgenteSaude.fbx`) é **Generic** (`animationType: 2`), sem IK Humanoid. A pose de carregar é uma ação do Blender numa camada com `AvatarMask`.
- Com os pneus até a picape, a partida fica cerca de 4 min mais longa só de caminhada carregando. A spec sobe `multiplicadorCarregando` para 0,85 e pede recalibração no simulador.


### 2026-10-29 — Plano de implementação da Parte B

**Objetivo (pedido do usuário):** com a spec da Parte B aprovada, escrever o plano de implementação.

**Arquivo novo:** `docs/superpowers/plans/2026-10-29-parte-b-focos-vivos-e-chuva.md`, com 15 tarefas:
1. ciclo de vida;
2. atalho de esvaziar na base, no vaso e nas garrafas;
3. pneu esvaziado no lugar;
4. `GerenciadorDeFocos`;
5. `FluxoDeJogo` com pressão por peso;
6. migração das outras 6 varreduras e zumbido por estágio;
7. chuva;
8. caçamba da picape com vagas e lixeiras;
9. enxame e textos da HUD;
10. posts;
11. pose de carregar, saco na mão e peso, **com preview aprovado pelo usuário antes de seguir**;
12. sons;
13. builder `Dengue > Configurar Focos Vivos e Chuva`;
14. balanceamento;
15. verificação em Play e registro.

**Nenhum script, cena ou asset foi alterado nesta entrada.**

**Observações para futuras sessões de IA:**
- `ResultadoDoPasso` ganha `JaVazio` e `Esvaziou` no fim do enum.
- Os testes novos usam a categoria `ParteB`, e o ciclo roda `ParteA` e `ParteB`.


### 2026-11-04 — Parte B implementada: focos vivos, chuva e entregas com peso

**Objetivo (pedido do usuário):** implementar a Parte B do roteiro. Os focos evoluem (ovo → larva → mosquito) e pesam na barra de ocupação. As Mãos ganham um atalho de esvaziar, que a chuva cobra reenchendo. Os pneus vão para a caçamba da picape da UPA, as garrafas vão para as lixeiras de rua, e carregar tem feedback forte.

Spec: `docs/superpowers/specs/2026-10-29-parte-b-focos-vivos-e-chuva-design.md`. Plano (15 tarefas): `docs/superpowers/plans/2026-10-29-parte-b-focos-vivos-e-chuva.md`. Branch `parte-b/focos-vivos-e-chuva`, **sem commit** (o usuário decide). Registro por tarefa: `.superpowers/sdd/2026-10-29-parte-b-focos-vivos-e-chuva/progress.md`.

**Resultado dos testes:** ParteA 57/57 e ParteB 155/155, `Failed: 0`. Console do Play: 0 erros e 0 avisos nas duas sessões (os erros antigos do `Default-Particle.psd` sumiram na Task 12).

**O que o jogador vê agora:**
- **Ciclo do foco:** ovo → larva → mosquito, com enxame em volta do foco e o estágio na mira ("com larvas", "com mosquitos voando"). A lista não mostra o estágio. Quem tem mosquito pesa mais na barra.
- **Atalho de esvaziar:** Mãos em vaso e garrafas, em 3 passos (1,2 s), só com a ação definitiva ainda não começada. O pneu não usa o atalho: esvazia nos 5 passos da ação normal, com as Mãos. O foco esvaziado pesa 0, não zumbe, não dá alívio e não conta para a vitória. A mira diz "vazio, por enquanto" e ensina a ferramenta certa.
- **Chuva:** a cada 3 a 4 min, com previsão no celular 45 s antes. Luz e névoa caem em rampa, depois vêm gotas, trovão e loop. O esvaziado volta como larva.
- **Destinos:** pneu em tamanho real (0,7 m) para as 6 vagas da carroceria da picape, arremessado em arco e com balanço. Garrafas para as 5 lixeiras de rua. As 3 caçambas de entulho saíram.
- **Peso de carregar:** pose nova de braços com o pneu, saco na mão direita que cresce com as garrafas, câmera mais pesada, passo pesado, esforço e tilintar.

**Arquivos novos:**
- **Scripts (`Assets/Scripts/`):** `AssemblyInfo.cs`, `Chuva.cs`, `ChuvaVisual.cs`, `CicloDeVida.cs`, `EnxameDeMosquitos.cs`, `FeedbackDeCarga.cs`, `GerenciadorDeFocos.cs`, `Lixeira.cs`.
- **Editor (`Assets/Editor/`):** `FocosVivosBuilder.cs` (menu **Dengue > Configurar Focos Vivos e Chuva**), `PoseDeCarga.cs`.
- **Testes (`Assets/Editor/Testes/`):** `BalanceamentoSimuladorTestes`, `ChuvaTestes`, `CicloDeVidaTestes`, `DestinosTestes`, `EsvaziarTestes`, `FeedbackDeCargaTestes`, `FluxoPressaoTestes`, `GerenciadorDeFocosTestes`, `HistoriaChuvaTestes`, `MigracaoGerenciadorTestes`, `PneuEsvaziarTestes`, `SinaisTestes`, `SonsNovosTestes`.
- **Assets gerados (`Assets/Meus/Assets Gerados/`):**
  - `Agente de Saude/Agente_Carregando.mask` e `Agente_CarregandoSaco.mask`;
  - `Chuva/Gota de Chuva.mat`;
  - `Saco/Saco.fbx` e `Saco/Saco_Plastico.mat`;
  - em `Sons/`: `chuva_loop`, `esforco_1` a `3`, `esvaziar`, `lixeira_tampa`, `passo_pesado_1` a `3`, `pneu_picape`, `saco_tilintar`, `suspensao`, `trovao` (.wav).
- **Ferramentas e docs:** `Ferramentas/Blender/gerar_saco.py`, a spec e o plano.

**Arquivos alterados:**
- **Scripts:** `Cacamba`, `CameraJogador`, `DengueHUD`, `EmissionHighlightUtil`, `Ferramenta`, `FluxoDeJogo`, `FocoListaHUD`, `FocoProximityGlow`, `GarrafaInteractable`, `HistoriaRedeSocial`, `Interactable`, `Interactor`, `KitDoAgente`, `KitHUD`, `PneuInteractable`, `PontoDeReabastecimento`, `PontoDeUso`, `RedeSocial`, `SomDoJogador`, `SomDoKit`, `SomDosFocos`, `VasoInteractable`.
- **Editor:** `BalanceamentoSimulador`, `ImportadorDeSons`, `KitDoAgenteBuilder`, `SomBuilder`.
- **Testes:** `PneuCarregarTestes`, `PontosDeUsoTestes`.
- **Assets e cena:** `AgenteSaude.fbx` (e `.meta`), `Agente_AnimController.controller`, `Vaso de Flor.prefab`, `Assets/Scenes/SampleScene.unity`.
- **Configuração e ferramentas:** `ProjectSettings/GraphicsSettings.asset` (Fog Modes em Custom, com Linear), `Ferramentas/Audio/gerar_sons.py`, `Ferramentas/Blender/gerar_agente.py`.

**Valores finais (na cena):**

| Campo | Valor |
|---|---|
| `FluxoDeJogo.taxaComTodosOsFocos` | **0,0013** (era 0,00277) |
| `FluxoDeJogo.alivioPorFoco` | **4,5%** (era 2,5%) |
| `FluxoDeJogo.recuperacaoSemFocos` | 0,1%/s (sem mudança) |
| Ciclo | 75 s por estágio; pesos ovo 1, larva 1,5, mosquito 2,5 |
| Início da partida | 3 focos com mosquito, 1/3 dos outros com larva, o resto com ovo |
| Atalho (vaso e garrafas) | 3 passos em 1,2 s; o pneu esvazia nos 5 passos da ação normal |
| Chuva | entre inícios 180–240 s, previsão 45 s, chovendo 20 s, volta do visual 10 s |
| Visual da chuva | luz × 0,45, névoa Linear de 15 a 120 m |
| `KitDoAgente.multiplicadorCarregando` | 0,85 |
| Carroceria e saco | 6 vagas; saco de 6 garrafas |

Os padrões dos campos no código continuam os antigos (0,003 e 0,025). Quem grava a calibração é o `FocosVivosBuilder`.

**Simulador (Dengue > Simular Balanceamento, Task 14):**

```
[Balanceamento] 22 focos · ocupação inicial 70% · mínima 30% · alívio por foco 4,5%
Taxa depois da rampa: +0,03%/s com todos em ovo · +0,23%/s com todos em mosquito · a ocupação começa a descer com 6 focos restantes em mosquito
Ritmo + custo do kit (s/foco; os últimos levam até 1,6x; custo do kit 11,2 s/foco; maioria de 10 sementes):
  15 s + 11,2 s → vence em 12:22 · pico 71% (70%–71%) · final 30% · 10/10 sementes vencem
  20 s + 11,2 s → vence em 14:43 · pico 71% (71%–72%) · final 30% · 10/10 sementes vencem
  25 s + 11,2 s → vence em 17:05 · pico 78% (77%–79%) · final 30% · 10/10 sementes vencem
  30 s + 11,2 s → vence em 19:26 · pico 87% (86%–88%) · final 30% · 10/10 sementes vencem
  35 s + 11,2 s → vence em 21:48 · pico 95% (94%–96%) · final 31% · 10/10 sementes vencem
  40 s + 11,2 s → PERDE aos 7:37 (7:16–8:17) com 7/22 focos · 0/10 sementes vencem
  45 s + 11,2 s → PERDE aos 6:17 (5:54–6:42) com 5/22 focos · 0/10 sementes vencem
  50 s + 11,2 s → PERDE aos 5:39 (5:17–6:01) com 4/22 focos · 0/10 sementes vencem
  parado → PERDE aos 3:21 (3:16–3:26) com 0/22 focos · 0/10 sementes vencem
```

- Os 4 critérios fecham nas 10 sementes: 30 s/foco com pico até 90%, 35 s vence, 40 s perde, parado perde entre 2:30 e 3:30.
- **Nota sobre o alívio mínimo:** o relatório da Task 14 diz "pelo menos 4%", e isso é impreciso. Com 3,75% e taxa 0,00125 também fecha, mas sem folga. A escolha de 4,5% continua de pé: é o menor valor testado com 4 s de folga no parado.
- A transição é abrupta: a primeira derrota de alguma semente sai em 37 s/foco, e a última vitória em 38 s/foco. A margem é de cerca de 2 s/foco para cada lado.

**Verificado em Play (Task 15, `SampleScene`, duas sessões, abertura pulada, `runInBackground` só no teste):**
1. **Início:** 3 focos em Mosquito, cada um com o filho "Enxame" tocando (24 mosquitos). Pressão 1,341.
2. **Ciclo:** `Avancar(75)` mudou 19 estágios, e a pressão foi de 1,341 a 1,909.
3. **Esvaziar e reencher:**
   - O vaso esvaziado com as Mãos foi de peso 2,5 a 0, e um novo passo deu `JaVazio`.
   - A chuva o reencheu como Larva (peso 1,5).
   - Saíram a previsão, o post da Lúcia (primeiro esvaziar) e o da Bruna (primeiro reenchido).
4. **Pneus:** os 6 ficaram nas Vagas 1 a 6, com a escala certa, e `FocosEliminados` subiu 6.
5. **Saco:** 5 garrafas esvaziadas na Lixeira Oeste, e a lixeira recusa com o saco vazio. Há 5 lixeiras em cena.
6. **Vitória:** com os 22 focos tratados, cada um com a ferramenta certa, saiu `OnVitoria`.
7. **Console:** 0 erros e 0 avisos nas duas sessões, conferido também por `Application.logMessageReceived`.

**Conferências extras:**
- **Enxame e mira:** mirar e chegar perto do Ralo (2) com enxame não logou `_EmissionColor`.
- **Poses (andar e correr por um controle virtual do Input System):**
  - Com pneu: 2,38 m/s, sem corrida, camada "Carregando" em 1.
  - Sem pneu: 2,80 m/s e 5,80 m/s, camada em 0. Os ossos do braço voltam ao descanso: diferença de 0,4° a 1,3°.
  - Com o saco: camada "CarregandoSaco" em 1 e volta a 0 depois da lixeira, com o braço solto de novo (0,4°).
  - O saco some com o pneu nos braços e volta ao largar.
- **Pneu largado longe (zumbido):** reenchido a cerca de 80 m de onde nasceu, zumbe no lugar novo (fonte a 0,00 m do pneu, tocando, volume 0,245).
- **Voltar ao menu pela pausa, chovendo:** a `MenuInicial` fica com a névoa dela (Linear, 40 a 170 m, cor 0,86/0,66/0,52), igual à registrada antes do Play.
- **Água na cena real:**
  - No vaso, o plano da água desce de y 1,210 a 1,162 (o fundo do prato), some no último passo e volta na altura original.
  - O pneu perde e recupera a água.
  - **As garrafas não têm água nenhuma:** os 3 grupos (22 garrafas e latas) não têm filho `agua*`, então esvaziar e reencher não muda nada no modelo (ver o item abaixo).
- **Sons com o Player completo, por dedução:** a medida foi contar as subidas de `AudioSource.isPlaying` a cada tick do Editor. Ela mostra que a fonte começou a tocar, e não qual clipe tocou: o `PlayOneShot` não expõe o clipe. O que se mediu e o que se deduziu:
  - **Passo pesado:** a fonte `Som Passos` subiu várias vezes andando com pneu, com o `PassoPesado` ligado e 3 clipes em `passosPesados` na cena. Que o clipe tocado era o pesado é dedução, juntando o flag e os clipes.
  - **Esforço:** a fonte `Som Carga` subiu 4 vezes nos primeiros 20 s com o pneu. É esforço por eliminação: essa fonte só toca esforço e tilintar, e com o pneu o tilintar não sai.
  - **Tilintar:** sem pneu e sem saco, a contagem da `Som Carga` ficou parada em 28, andando e correndo. Com o saco a 5/6, andando, foi a 30. É tilintar por eliminação: o esforço só toca com o pneu, então sem pneu só ele resta.
  - **Tilintar correndo:** inconclusivo (ver "O que NÃO foi verificado").
  - **Limiar da metade do saco:** não foi medido em Play, que só testou 5/6 e sem saco. Quem o cobre são os testes EditMode `Tilintar_ACadaPasso_ComSacoAcimaDaMetade` e `Tilintar_SacoAteAMetade_NaoToca`, que chamam `PassoDoSaco` sem fonte de áudio: a regra está testada, mas nenhum som tocou nesses casos.
- **Mira e botões pelo `Interactor` real (controle virtual):** segurar esvaziou o vaso e o pneu, soltar e apertar de novo pegou o pneu, e o botão de largar largou.
- **Capturas:** `.superpowers/sdd/2026-10-29-parte-b-focos-vivos-e-chuva/task-15-*.png` (enxame, chuva, pneu em 1ª pessoa, carroceria com 6 pneus, saco na mão, HUD "com larvas" e "vazio, por enquanto").

**Bug achado e corrigido na Task 15:**
- **O que era:** colado num muro, o pneu largado ficava flutuando a 2,1 m do chão. O pneu recua para perto do agente, e o raio do chão desce por dentro do corpo dele. O `CharacterController` fica na camada Default, a máscara não o tira, e o raio parava no corpo. Pior: a altura errada ia para o `alturaDoPivo` no pegar seguinte, e o pneu não descia mais.
- **Correção:** `PneuInteractable.AcharChao`, usado no `OndeCai` e no `PrenderEm`, pega o acerto mais perto fora do corpo do agente.
- **Teste novo:** `Largar_ColadoNoMuro_OCorpoDoAgenteNaoEChao` (`ParteA`). Antes da correção dava 2,10 m; agora dá 0,70 m. Na cena, encostado no `building 2`, o pneu assentou em y 0,657.

**O que NÃO foi verificado:**
- **Partida real com teclado e mouse.** A entrada foi um controle virtual ou chamadas diretas. Os focos foram tratados por `AvancarCom`, sem andar até cada um.
- **Sons de ouvido.** `isPlaying` diz que a fonte tocou, não qual clipe, como soa nem se o volume está bom. Em especial: esforço, tilintar, passo pesado, trovão e a repetição do alerta.
- **Identidade dos clipes de carga.** O tilintar, o esforço e o passo pesado foram deduzidos (ver "Sons com o Player completo"): nenhum clipe tocado foi lido.
- **Tilintar correndo.** Com o saco a 5/6, a contagem da `Som Carga` não subiu mais ao correr. A amostra só conta subidas, então não dá para saber se a fonte seguiu tocando sem soltar ou se não tocou. Fica inconclusivo.
- **Limiar da metade do saco em Play.** Com garrafas, só se andou com o saco a 5/6, sem nenhuma rodada de 1 garrafa até a metade: o limiar vem só dos testes EditMode.
- **Dificuldade jogada.** Só há o simulador, e ele é pessimista.
- **Build exportada:** a névoa Linear e o `_EMISSION` na mira dependem das variantes levadas para a build, e ninguém testou um executável.
- **Abertura e saída do hospital reais.** A abertura foi pulada, e `IniciarPartida` foi chamado direto.
- **Balanço da picape quadro a quadro,** nem a densidade das gotas como gosto do usuário.

**Decisões tomadas durante a execução (`Ruling:` do ledger, em resumo):**
- **Processo:** sem worktree e sem commit; o índice do git foi a base de cada tarefa. Dois revisores por tarefa, e as correções foram para agentes novos. A T11 parou até o usuário aprovar a pose, e os testes zeram `punchScale` e `shrinkDuration` quando não testam animação.
- **Gerenciador:**
  - Campo vazio chama `GerenciadorDeFocos.Obter()`: acha o da cena e, em Play, cria um se faltar. Ele também acha inativo, assina o fluxo no Awake e não cria saindo do Play.
  - Um gerenciador inativo fica com o ciclo parado, sem aviso (fica assim).
  - Os consumidores desassinam só pelo campo em cache.
  - O sorteio exclui os esvaziados.
  - O ciclo anda em velocidade cheia com o celular aberto.
- **Esvaziar:**
  - O passo não dispara `OnClicado`.
  - `Esvaziou` exige soltar o botão, e `JaVazio` conta como recusa.
  - `NoCaminhoDeEsvaziar` é público.
  - Com a ação definitiva começada, vaso e garrafa não reenchem. O pneu nos braços também não, e o reencher zera os cliques.
  - A água do pneu só sai no último passo.
  - A mira das Mãos diz "Segure para esvaziar (a chuva enche de novo)", com toast próprio.
- **Chuva:**
  - `Avancar(dt)` processa em ordem todas as transições que atravessa.
  - A névoa ficou Linear, com o builder pondo o Fog Modes em Custom.
  - O `ChuvaVisual` só restaura o `RenderSettings` se alterou algo e se a cena dele é a ativa.
  - A previsão recusada na janela de conflito fica pendente e tenta de novo, em prioridade Alta.
- **Caçamba e lixeira:**
  - Com pneu, a picape delega à `Cacamba`.
  - Fora do Play o arremesso é instantâneo.
  - A `Cacamba` conta os recebidos, e o pneu sem vaga encolhe 1,2 m acima do pivô.
  - `ReservarVaga` pula vagas nulas.
  - As vagas têm escala (1, 1, 1).
  - Os textos não repetem o "segure".
- **HUD e posts:**
  - `Ferramentas.ComArtigo` dá o artigo certo.
  - O toast do pneu diz "leve à picape".
  - O enxame sincroniza no `Ligar` e fica fora do brilho (`Destacavel`).
  - O reenchido usa `PostPorTipo`.
  - O primeiro esvaziar ignora o pneu.
- **Carga:**
  - A pose vive em `PoseDeCarga.cs`.
  - O saco pedido pelo usuário exigiu uma camada só do braço direito.
  - As camadas nascem com peso 0 no asset.
  - O `ModoPesado` é suavizado.
  - O saco some com o pneu nos braços.
- **Sons:** ficam em `Assets/Meus/Assets Gerados/Sons/`. As partículas de estouro usam material estático e textura gerada em código.
- **Cena:** pneu medido no espaço dele (0,700 nos 6). Lixeiras novas copiam a `Trassh Bin (1)`, de 1,08 m. Nenhuma lixeira tem tampa separada.
- **Balanceamento:** "média das sementes" é maioria, e a ordem de eliminação é a posição no sorteio. Alívio 4,5% e taxa 0,0013 foram a exceção prevista: só a taxa não fechava os 4 critérios.
- **Encaminhadas e resolvidas:**
  - **R3-1:** os comentários do `Interactable` que punham o pneu nos braços no último passo foram corrigidos (o último passo esvazia; pegar é o passo seguinte).
  - **T6U-3:** o volume do zumbido muda de estágio em rampa (`fator` no `SomDosFocos`), sem degrau.
  - **T6U-4:** a lista `focos` morta do `SomDosFocos` foi removida.
  - **U8-1:** aceito que a cena não fechasse a partida entre a T8 e a T13; as lixeiras da T13 fecharam.
  - **U8-6:** largar contra a cabine entrega (`CacambaDe`), e o tempo de segurar na delegação é o da caçamba (`SegundosParaUsarPara`).
  - **U8-7:** o pneu empilhado não pisca com a picape mirada (`Interactor.EntraNoDestaque`).
  - **U8-10:** o `OnDisable` da `Lixeira` fecha a tampa, e o da `Cacamba` endireita a carroceria.
  - **U1:** o zumbido e o splash do pneu movido tocam onde ele está (`SomDosFocos.CentroAtual`).
  - **T12-1:** o esforço não toca no epílogo (`FeedbackDeCarga.SonsLigados`).
  - **T12-2:** sem pneu, o tempo de segurar na carroceria é o da picape (`Cacamba.SegundosParaUsarPara`).

**Desvios do plano:**
- **Névoa Linear** no lugar de `densidadeDeNevoaNaChuva`: os campos viraram `inicioDaNevoaNaChuva` e `fimDaNevoaNaChuva`, porque a névoa Linear sobrevive ao stripping da build.
- **Pneu subiu 10 cm com a pega nova** (`PosicaoDaCarga`), depois do preview aprovado pelo usuário.
- **Saco escondido com o pneu nos braços,** contra o "ativo com GarrafasNoSaco > 0" do plano: as duas mãos estão no pneu.
- **Lixeiras sem animação de tampa.** Os 3 modelos são malha única, e só o som toca. O feedback substituto (balanço ou punch) ficou para decidir.
- **`Assets/Scripts/AssemblyInfo.cs` com `InternalsVisibleTo("Assembly-CSharp-Editor")`,** para os testes chamarem métodos `internal`.
- **O alerta sonoro pode repetir com o alívio novo:** 4,5% rearma o alerta (rearme 0,03) a cada foco, e o marco de 95% pode tocar até 4 vezes. Falta avaliar jogando.
- **`alivioPorFoco` mudou:** o plano só previa mexer na taxa e, se preciso, no multiplicador.

**Observações para futuras sessões de IA:**
- **Só o gerenciador varre `Interactable`.** Nos scripts de jogo, ninguém mais faz `FindObjectsByType<Interactable>`. Quem precisa de focos usa `GerenciadorDeFocos.Obter()` e os eventos dele.
- **O atalho fica na base (`Interactable`),** ligado por `podeEsvaziar`. Vaso e garrafas só dizem o que acontece com a água. O pneu não usa o atalho: esvazia nos 5 passos da ação normal.
- **Os pneus estão em escala real** (0,7 m, escala de mundo 34,7) e **não há mais caçambas de entulho.** A única `Cacamba` é a da picape. O `Cacamba.mat` ficou órfão.
- **Depois do `PlayerBuilder`, rode o menu Focos Vivos** (`GarantirAssets` + `Aplicar`), senão a pose de carga e o saco somem do Player.
- **Cada rodada do menu reescreve o controller:** os fileIDs do `Agente_AnimController.controller` mudam sem mudança de comportamento.
- **O simulador é pessimista:** o pneu esvaziado pesa 0 durante a viagem no jogo, mas no simulador pesa até a entrega, e o atalho fica de fora. O jogo real fica um pouco mais fácil que a tabela.
- **A margem é estreita (~2 s/foco).** Mudar a picape de lugar ou o número de pneus pode pedir recalibração.
- **O corpo do agente é collider na camada Default.** Raio que desce perto dele precisa ignorá-lo (`PneuInteractable.AcharChao`). A máscara `CorpoJogador` só tira a malha do modelo.
- **Colado num muro, largar o pneu empurra o agente uns 20 cm:** o pneu recua e nasce encostado na cápsula. Não atrapalha, mas é o único caso em que ele nasce tocando o corpo.
- **As garrafas não têm água na cena.** O `AoEsvaziar` e o `AoReencher` da `GarrafaInteractable` só agem sobre filhos `agua*`, que não existem nos 3 grupos. O feedback ali é o loop de segurar, o splash, o toast e a mira. Para ver a água mudar, é preciso modelar esses filhos.
- **A `Chuva.Avancar` de uma vez só (fora do jogo normal) junta previsão e reenchido no mesmo instante,** e o post do reenchido cai na janela de conflito. No jogo há 45 s entre os dois, e o post saiu normalmente.
- **`capture_game_view` grava em `Assets/` mesmo com caminho absoluto.** Mova a captura e apague a pasta `Assets/.superpowers`.


### 2026-11-04 — Correções da revisão final da Parte B

**Objetivo:** leva única de correção da revisão final (`.superpowers/sdd/2026-10-29-parte-b-focos-vivos-e-chuva/final-review-report.md`). Branch `parte-b/focos-vivos-e-chuva`, **sem commit**. Ficam como estão, por decisão do controlador: M6 (largar colado no muro empurra ~21 cm) e M7 (o alerta rearma a cada foco).

**Resultado dos testes:** ParteA 57/57 e ParteB 165/165, `Failed: 0` (eram 155: 10 testes novos). Com as correções de M1, M2, M3, M4 e M8 desligadas de propósito, 5 testes falharam, um por correção; o código foi restaurado depois.

**O que mudou:**
- **M1 (post do primeiro pneu):** "Primeiro pneu na caçamba..." nunca saía: a fala do dono fechava a janela de conflito no mesmo quadro. Agora sai dentro do `HandleFocoEliminado`, depois dos finais, em prioridade Alta. Saíram a assinatura do `OnPneuEntregue` na `HistoriaRedeSocial` e o `HandlePneuEntregue` (o evento do kit fica). O teste passa pela entrega real, com a janela ligada.
- **Fala do Carlos:** a 1ª do `aoEliminar` dizia "primeiro pneu" e sairia no 2º. Virou "Lá se vai mais um na picape da UPA. Faltam {restantesTipo}, e eu tô contando daqui da janela." (`FalaCarlosDepoisDoPrimeiroPneu`). O `RegravarTextosDaParteB` regrava só essa fala, e ela foi gravada na cena.
- **M2 (aviso da entrega):** com vaga na carroceria, o `HandleCompleted` conta e dá o punch, sem "Foco eliminado!"; o "Pneu recolhido — N de T" sai no fim do arco. Sem vaga, o aviso de eliminado continua.
- **M3 (lixeira):** sem tampa, a raiz balança ±6° em 0,3 s, em torno do Z do mundo (em 4 das 5, o Z local é o vertical, pelo giro de -90° do import), e volta ao giro original, também no `OnDisable`. As 5 lixeiras da cena têm `StaticEditorFlags` 0: o builder não precisou mudar.
- **M4 (garrafas):** a mira do grupo esvaziado diz "Garrafas viradas — vazias, por enquanto", sem o "juntando água". Sem modelar água.
- **M5 (enxame a distância):** com 0,002 e 24 partículas, o enxame não se lia a 15 e a 20 m: pontos de 2–3 px soltos no cenário. Subiu para `TamanhoMinimoNaTela` **0,004** e `particulas` **40** (no código e na cena). Agora se lê a 15 m, a 20 m e a 20 m na chuva.
- **M8 (som):** o `primeiroClique` só toca com `!foco.Esvaziado` (`SomDosFocos.TocaPrimeiroClique`).
- **M9 e T2-4 (comentários):** `EstoqueDoKit` e `GarrafaInteractable` mandam o saco para a lixeira, o `Interactor` diz que o pneu entregue fica na vaga, e o cabeçalho de `EsvaziarTestes` diz que o pneu esvazia pelo caminho normal.
- **R15-2 e R15-3 (este arquivo):** na entrada da Parte B, vaso e garrafas usam o atalho de 3 passos e o pneu esvazia nos 5 da ação normal, e entrou o bullet "Encaminhadas e resolvidas".

**Arquivos alterados:**
- **Scripts:** `DengueHUD`, `EnxameDeMosquitos`, `EstoqueDoKit`, `GarrafaInteractable`, `HistoriaRedeSocial`, `Interactor`, `Lixeira`, `SomDosFocos`.
- **Testes:** `DestinosTestes`, `EsvaziarTestes`, `HistoriaChuvaTestes`, `SinaisTestes`.
- **Cena:** `Assets/Scenes/SampleScene.unity`, só a 1ª fala do Carlos e `EnxameDeMosquitos.particulas`.

**Configurações aplicadas:**

| Campo | Valor |
|---|---|
| `EnxameDeMosquitos.TamanhoMinimoNaTela` | **0,004** (era 0,002) |
| `EnxameDeMosquitos.particulas` | **40** (era 24, código e cena) |
| Balanço da lixeira | ±6° em 0,3 s, uma oscilação |

**Verificado em Play (`SampleScene`, abertura pulada, `runInBackground` só no teste, chamadas diretas de `Usar`, `Pegar` e `EntregarPneu`):**
- **Entrega real:** "Pneu recolhido — 1 de 6" a 0,60 s, `Eliminados` em 1, e o post do Carlos publicado. A ausência do "Foco eliminado!" antes do arco só tem cobertura em EditMode (em Play foi lido o texto do rótulo, não o alpha).
- **Lixeira Oeste:** ângulo máximo 6,00°, ângulo final 0,000°.
- **Capturas:** `.superpowers/sdd/2026-10-29-parte-b-focos-vivos-e-chuva/task-16-enxame-15m.png`, `task-16-enxame-20m.png`, `task-16-enxame-20m-chuva.png` e as duas `-antes` (vaso em frente à delegacia, com os valores antigos).

**O que NÃO foi verificado:**
- **Partida real com teclado e mouse.** Continua sendo condição do merge.
- **Build exportada.** Névoa Linear e `_EMISSION` na mira seguem sem prova num executável.
- **Sons de ouvido.** O `primeiroClique` corrigido e os sons da Parte B seguem sem escuta.
- **Enxame em toda a cena.** Medido em um foco, no pneu do terreno baldio, contra a cerca; as capturas novas são só dele. Contra fundo movimentado, só há a captura `-antes`.
- **Balanço visto em movimento.** Medi o ângulo, mas ninguém olhou a lixeira balançando.
- **Aviso da entrega em Play.** O alpha do aviso não foi medido, e o punch do contador só tem cobertura em EditMode.

**Observações para futuras sessões de IA:**
- **O evento `KitDoAgente.OnPneuEntregue` segue existindo,** usado nos testes, mas a história não o escuta: o post sai do `HandleFocoEliminado`. A janela de conflito recusaria um segundo post no mesmo quadro.
- **`AGENTS.md` continua desatualizado:** diz que o `PneuInteractable` tira a água "após 3 cliques" e "encolhe até sumir". Só o usuário pode pedir essa mudança.
- **`particulas` é serializado:** mudar o padrão no código não muda a cena.


### 2026-11-04 — Ajustes da Parte B depois da partida do usuário

**Objetivo (pedido do usuário):** corrigir três pontos vistos jogando.

**Arquivos alterados:** `Assets/Scripts/FeedbackDeCarga.cs`, `Assets/Scripts/EnxameDeMosquitos.cs`, `Assets/Scripts/DengueHUD.cs`, `Assets/Editor/Testes/FeedbackDeCargaTestes.cs`, `Assets/Editor/Testes/SinaisTestes.cs` e `Assets/Editor/Testes/SonsNovosTestes.cs`.

**Mudanças:**
- **Saco:** só fica à mão com o saco de lixo selecionado. Trocar de ferramenta guarda o saco, baixa o braço e para o tilintar; as garrafas continuam dentro. Vale também para a tecla 5 de volta.
- **Mosquitos:** `TamanhoMinimoNaTela` foi de 0,004 para 0, então cada mosquito encolhe com a distância. Isso desfaz o ajuste M5 da revisão final: o enxame deixa de ser lido de 15 a 20 m por construção, e o requisito da spec dá lugar ao pedido do usuário.
- **Mira:** saíram "com larvas" e "com mosquitos voando" (e o `ComplementoDoEstagio`). Fica só "vazio, por enquanto". Com isso, o estágio larva não tem mais nenhum sinal para o jogador.

**Observações para futuras sessões de IA:** o foco ainda muda de estágio e pesa na barra; só o texto saiu da mira.


### 2026-11-04 — Skill check no ralo

**Objetivo:** Trocar o "segurar até concluir" do ralo por uma sequência de skill checks, a pedido do usuário.

**Arquivos alterados:**
- `Assets/Scripts/SkillCheck.cs` (novo) — lógica pura: `SkillCheck` e `SessaoDeSkillCheck`.
- `Assets/Scripts/SkillCheckHUD.cs` (novo) — anel no centro da tela, com Canvas próprio em código. O `Interactor` adiciona o componente no `Start`; nada na cena.
- `Assets/Scripts/Interactor.cs` — em foco com `UsaSkillCheck`, um aperto inicia os checks; evento `OnErrouSkillCheck`.
- `Assets/Scripts/Interactable.cs` — `UsaSkillCheck`, `SegundosPorVoltaDoCheck`, `ErrarSkillCheck`, `CancelarSkillCheck`.
- `Assets/Scripts/RaloInteractable.cs` — erro zera o progresso, devolve a grelha e gasta a tela; campo `segundosPorVolta`.
- `Assets/Scripts/ControladorJogador.cs` — `Imobilizado`: sem andar, olhar nem pular, com a entrada ligada.
- `Assets/Scripts/CelularController.cs` — o celular não abre durante o check.
- `Assets/Scripts/DengueHUD.cs` e `Assets/Scripts/SomDoKit.cs` — aviso e som de erro.
- `Assets/Editor/KitDoAgenteBuilder.cs` e `Assets/Meus/Assets Gerados/Focos/Ralo.prefab` — ralo com 3 passos (era 8).
- `Assets/Editor/Testes/SkillCheckTestes.cs` (novo).

**Configurações aplicadas:** 3 checks; 1,2 s por volta; zona de 15%, 12,5% e 10% do anel, sorteada entre 30% e 95% da volta; anel de 120 px em 1920x1080. Clicar fora da zona ou deixar a volta acabar é erro.

**Observações técnicas:**
- Testes EditMode: 232 de 232, já com o `Outline` do contador.
- Play Mode: os 4 ralos têm 3 passos. O tempo esgotado zerou o progresso, gastou a tela e soltou o jogador. O check foi iniciado por reflexão; o clique real do mouse não foi exercitado.
- O contador "1/3" não apareceu na captura sobre a parede clara, e o `Outline` foi acrescentado sem nova captura.
- `Bloqueado` no meio do check cancela sem gastar a tela. O `BalanceamentoSimulador` segue com 2,0 s por ralo e não conta os erros nem a viagem extra à picape.


### 2026-11-04 — Visual do skill check do ralo

**Objetivo:** Deixar só o skill check na tela durante a ação e melhorar o visual, a pedido do usuário.

**Arquivos alterados:**
- `Assets/Scripts/SkillCheckHUD.cs` — refeito: disco escuro, trilho fino, rastro do ponteiro, zona verde com brilho, ponteiro afilado e um pontinho por check no lugar do texto "1/3". Formas em sprites gerados com borda suave; zona e rastro usam `Image` Filled radial. Animações de entrada, acerto, conclusão e erro.
- `Assets/Scripts/DengueHUD.cs` — durante o check, esconde a mira e o painel do alvo (texto, barra e 3/5). Nova `AcaoSkillCheck`: "Clique para encaixar a grelha", no lugar do "segure o clique" no ralo.

**Configurações aplicadas:** anel segue com 120 px em 1920x1080; ponteiro fica verde dentro da zona.

**Observações técnicas:**
- Testes EditMode: 232 de 232.
- Play Mode, com check iniciado por script: mira desligada e `targetGroup` em alfa 0, conferidos por reflexão; anel e pontinhos conferidos em captura, sobre o ralo. As animações não foram vistas em movimento.
- Canvas `ScreenSpaceOverlay` não aparece no `screenshot`: para capturar, trocar o Canvas `SkillCheckHUD` para `ScreenSpaceCamera` por eval.
- Compilar com o Editor em Play Mode recarrega o domínio e deixa referências nulas (`KitHUD.cs:127`, `SkillCheckHUD`). Sair do Play Mode antes de recompilar.


### 2026-11-04 — Skill check sem o rastro do ponteiro

**Objetivo:** Tirar a faixa que acompanhava o ponteiro no anel, a pedido do usuário.

**Arquivos alterados:** `Assets/Scripts/SkillCheckHUD.cs` — saiu a camada `Rastro`.

**Configurações aplicadas:** nenhuma.

**Observações técnicas:** o trilho, a zona e o ponteiro não mudaram.


### 2026-11-04 — Pilha de pneus na cena

**Objetivo:** Organizar os pneus do objeto `PilhadePneu` como uma pilha de verdade, a pedido do usuário.

**Arquivos alterados:** `Assets/Scenes/SampleScene.unity` — 6 filhos de `--- FOCOS ---/PilhadePneu`.

**Configurações aplicadas:** pneus deitados, em coluna única sobre o chão (y 0,293 a 1,460), com giro e deslocamento de até 3 cm em cada um para não ficar perfeito. Encaixe de 1 cm entre eles.

**Observações técnicas:** altura conferida pelos `Renderer.bounds`, sem captura de tela. Os pneus não têm física: tirar um de baixo deixa os de cima parados no ar.


### 2026-11-04 — Pneus em monte, em vez de pilha

**Objetivo:** Trocar a coluna do `PilhadePneu` por um monte jogado, com cara de abandonado, a pedido do usuário.

**Arquivos alterados:** `Assets/Scenes/SampleScene.unity` — 6 filhos de `--- FOCOS ---/PilhadePneu`.

**Configurações aplicadas:** queda simulada no Editor (`Physics.Simulate`, 500 passos, semente 7). 4 pneus ficaram no chão e 2 apoiados neles. `Rigidbody` e `convex` foram só da simulação e saíram depois.

**Observações técnicas:** esta entrada substitui a anterior, da coluna. Os pneus seguem sem física: tirar um de baixo deixa os apoiados parados como estão.


### 2026-11-04 — Água dos pneus do monte

**Objetivo:** Ajustar a água dos pneus do `PilhadePneu` à nova posição deles, a pedido do usuário.

**Arquivos alterados:** `Assets/Scenes/SampleScene.unity` — filho `agua do pneu` dos 6 pneus.

**Configurações aplicadas:** água sempre nivelada. Nos 4 pneus deitados, lâmina de 0,42 x 0,42 m tapando o furo, 2 cm abaixo do centro. Nos 2 inclinados, lâmina de 0,24 x 0,40 m a 15 cm do centro, no lado mais baixo do pneu.

**Observações técnicas:** o `agua do pneu` é um cubo escalado, feito para pneu em pé: deitado, virava uma placa vertical. Os demais pneus da cena não mudaram. O `BoxCollider` da água acompanhou a nova escala.


### 2026-11-04 — Água recortada no formato do pneu

**Objetivo:** Fazer a água dos pneus do `PilhadePneu` ficar só dentro do pneu, a pedido do usuário.

**Arquivos alterados:**
- `Assets/Scenes/SampleScene.unity` — `agua do pneu` dos 6 pneus usa malha própria; `BoxCollider` ajustado a ela.
- `Assets/Meus/Assets Gerados/Focos/Agua dos Pneus/` (novo) — 6 malhas `.asset`, uma por pneu.

**Configurações aplicadas:** lâmina nivelada, recortada pelo interior do pneu (raio 0,25 m, 8,5 cm de cada lado do plano do pneu). Nos deitados, 2 cm abaixo do centro; nos inclinados, 10 cm abaixo.

**Observações técnicas:** substitui as lâminas retangulares da entrada anterior. As malhas valem para a posição atual: mover ou girar um pneu do monte exige gerar a dele de novo.


### 2026-11-04 — Água só na parte oca dos pneus do monte

**Objetivo:** Deixar o furo dos pneus do `PilhadePneu` vazio, com água só por dentro da borracha, a pedido do usuário.

**Arquivos alterados:**
- `Assets/Scenes/SampleScene.unity` — `agua do pneu` dos 6 pneus (posição, escala e `BoxCollider`).
- `Assets/Meus/Assets Gerados/Focos/Agua dos Pneus/` — as 6 malhas refeitas.

**Configurações aplicadas:** lâmina nivelada, limitada à parte oca: raio de 0,215 a 0,325 m e 7,2 cm de cada lado do plano do pneu. Deitados: anel 1,5 cm acima da lateral de baixo. Inclinados: poça em meia-lua, 5 cm acima do ponto mais baixo.

**Observações técnicas:**
- A malha do pneu é oca, com a abertura voltada para o furo (raio 0,205 m). Substitui a entrada anterior, que tapava o furo.
- A água fica sob a lateral de cima: é vista pelo furo, em ângulo, e não de cima.
- Na captura, os 4 pneus deitados têm o furo vazio. A água dos 2 inclinados não ficou visível nesse ângulo; a malha deles foi gerada (42 e 38 triângulos).
- As malhas valem para a posição atual dos pneus.


### 2026-11-04 — Água na parte oca em todos os pneus

**Objetivo:** Aplicar a água da parte oca a todos os pneus da cena, respeitando o ângulo de cada um, a pedido do usuário.

**Arquivos alterados:**
- `Assets/Scenes/SampleScene.unity` — `agua do pneu` dos 11 pneus (`--- FOCOS ---` e `PilhadePneu`).
- `Assets/Meus/Assets Gerados/Focos/Agua dos Pneus/` — 11 malhas `.asset`, uma por pneu.

**Configurações aplicadas:** mesmos limites da parte oca da entrada anterior (raio de 0,215 a 0,325 m, 7,2 cm de cada lado). Deitados: anel 1,5 cm acima da lateral de baixo. Inclinados ou em pé: poça nivelada com 6 cm de profundidade no ponto mais baixo (os do monte eram 5 cm).

**Observações técnicas:**
- O recorte passou a tratar o plano da água como uma reta no perfil do pneu, porque a conta anterior dividia pelo `a.y`, que é zero em pneu em pé.
- O pneu largado só gira em torno do vertical (`DeLadoPara`), então a inclinação não muda e a água continua nivelada quando a chuva o reenche.
- Mudar a inclinação de um pneu na cena exige gerar a malha dele de novo.
- Conferido por captura: `pneu (1)`, `pneu (3)` e parte do monte. `pneu`, `pneu (2)` e `pneu (4)` foram conferidos só pelos números da malha.


### 2026-11-05 — Mira mostra só o nome do foco

**Objetivo (pedido do usuário):** ao apontar para um foco, mostrar apenas o nome dele.

**Arquivos alterados:** `Assets/Scripts/DengueHUD.cs`, `Assets/Scripts/FocoListaHUD.cs` (`Categoria` virou `internal`), `Assets/Editor/Testes/EsvaziarTestes.cs` e `Assets/Editor/Testes/SinaisTestes.cs`.

**Mudanças:**
- O texto de cima da mira usa o mesmo nome da lista de focos ("Pneu", "Vaso de flor", "Garrafas"...). Saíram as descrições do `FocoHUDInfo.textoAoMirar` (o campo continua na cena, sem uso na mira) e o "vazio, por enquanto".
- A linha de ação, embaixo, continua: "Segure com a areia para resolver de vez", "Segure para esvaziar". O pneu vazio passa a mostrar ali "Pneu vazio: leve à picape da UPA".
- Picape, lixeira e monte de areia não mudam.


### 2026-11-05 — Picape para 11 pneus, corrida com pneu e stamina

**Objetivo (pedido do usuário):** o usuário subiu os pneus do mapa de 6 para 11 (30 focos no total). A picape devia guardar todos, e o agente devia poder correr com o pneu gastando stamina.

**Arquivos novos:** `Assets/Scripts/StaminaDeCarga.cs` e `Assets/Editor/Testes/StaminaDeCargaTestes.cs`.

**Arquivos alterados:** `ControladorJogador.cs`, `DicaCorrer.cs`, `KitDoAgente.cs`, `KitHUD.cs`, `FeedbackDeCarga.cs`, `Assets/Editor/FocosVivosBuilder.cs`, `Assets/Editor/BalanceamentoSimulador.cs`, os testes que usavam `PodeCorrerEPular`, e a `SampleScene` (pelo menu).

**Mudanças:**
- **Vagas:** `FocosVivosBuilder.TotalDeVagas` dá uma vaga por pneu, em pares, com mínimo de 6. Com 11 pneus são 12 vagas em 2 colunas e 6 alturas (topo em y 2,13). Pôs mais pneus: rode **Dengue > Configurar Focos Vivos e Chuva**.
- **Stamina:** com pneu, o Shift corre a `velocidadeCorrer × multiplicadorCarregando` (4,93 m/s), ainda sem pular. Esgota em 6 s, recupera em 5 s sem correr, e zerada só volta a correr com 30%. De mãos livres a corrida não gasta. Campos no `KitDoAgente`.
- `ControladorJogador.PodeCorrerEPular` virou `PodeCorrer` e `PodePular`.
- **HUD:** barra fina na linha "Carregando pneu", vermelha abaixo de 30%, e o aviso "[Shift] corre". Ao zerar, toca um esforço.
- **Simulador:** a viagem com pneu usa a stamina, com o Shift apertado (`SegundosCarregando`).
- **Balanceamento:** `alivioPorFoco` de 4,5% para 5,5%, com `taxaComTodosOsFocos` 0,0013. Varredura com custo do kit de 10,6 s/foco: 30 s vence com pico 83%; 35 s vence com 94%; 40 s perde; parado perde aos 3:21.

**Efeito do menu:** ele reescalou para 0,70 m e reassentou os pneus (5) a (8), que estavam com 0,80 a 0,92 m, em volta de (40; 28).

**Verificado em Play (chamadas diretas, tempo forçado a 1):** os 11 pneus em vagas distintas, stamina zerando e travando a corrida, a corrida voltando a 30%, movimento normal depois da entrega, Console sem erros.

**NÃO verificado:** corrida com teclado de verdade, a barra na tela, a pilha de 12 vagas vista (a captura falhou) e a dificuldade jogada.

**Observações para futuras sessões de IA:**
- Antes de recompilar, confira `EditorApplication.isPlaying`: o usuário costuma jogar com o Editor aberto.
- A abertura deixa o `timeScale` em 0; em teste por `eval_file`, force 1 para as corrotinas andarem.
- `eval_file` não enxerga membros `internal`: use reflexão.


### 2026-11-05 — Silêncio do zumbido depois de limpar um foco

**Objetivo (pedido do usuário):** depois de limpo, o foco ainda parecia dar zumbido de mosquito.

**Causa:** o zumbido do foco limpo já parava (`Zumbe` falso, ou `Sumir`). Quem zumbia era o vizinho: há focos colados na cena (6 pneus perto de (40; 28), a 0,5–0,9 m; 3 vasos a 0,9 m), e o `SomDosFocos` passava o som para o mais perto em 0,5 s, quase do mesmo ponto.

**Arquivos alterados:** `Assets/Scripts/SomDosFocos.cs`, `Assets/Editor/Testes/SonsNovosTestes.cs`.

**Mudança:** campo `silencioAposLimpar` (3 s): esvaziar ou eliminar um foco cala todos os zumbidos por esse tempo; depois, o foco de pé mais perto volta a zumbir. O tempo usa `Time.time`, então não corre na pausa.

**Observações para futuras sessões de IA:** um foco reenchido pela chuva volta a zumbir, de propósito. Se o usuário quiser mais ou menos silêncio, ajuste `silencioAposLimpar` no objeto `Sons`.

### 2026-11-05 — Janela Dengue > Configurações Globais

**Objetivo (pedido do usuário):** mudar de uma vez valores globais, como a quantidade de cliques de todos os focos de um tipo, sem editar foco por foco no Inspector.

**Arquivos alterados:**
- `Assets/Editor/ConfiguracoesGlobais.cs` (novo) — lógica sem interface: tabela de tipos, leitura e gravação.
- `Assets/Editor/ConfiguracoesGlobaisJanela.cs` (novo) — janela e menu `Dengue/Configurações Globais`.
- `Assets/Editor/Testes/ConfiguracoesGlobaisTestes.cs` (novo) — 23 testes, categoria `Config`.
- `Assets/Editor/KitDoAgenteBuilder.cs` — saíram as colunas `segundos` e `passos` da tabela `Focos`; não grava mais tempo nem cliques.
- `Assets/Editor/FocosVivosBuilder.cs` — saíram `GravarBalanceamento` e o `multiplicadorCarregando`.
- `Assets/Editor/Testes/BalanceamentoSimuladorTestes.cs` — saiu `Builder_GravaOsValoresCalibrados`, que testava o método removido.

**Configurações aplicadas:** nenhum valor da cena ou dos prefabs mudou. Aba Focos: passos, tempo e esvaziar por tipo, com um Aplicar por tipo. Aba Partida: ocupação, ciclo de vida, chuva e kit, gravando na hora. Limites: passos mínimo 1, tempo mínimo 0, `segundosPorVolta` mínimo 0,1.

**Observações técnicas:**
- A cena agora é a única fonte de tempo, passos e balanceamento: rodar Configurar Kit do Agente ou Configurar Focos Vivos e Chuva não desfaz mais esses valores. O teste `Builders_NaoGravamOsCamposDaJanela` falha se algum deles voltar a gravar um campo da janela.
- Foco novo criado do zero nasce com o padrão do script ou do prefab, porque a tabela do Kit não define mais o tempo. O `Ralo (1)` continua com 5 passos e o simulador segue lendo os 3 s dele: não foi unificado.
- Nos focos de segurar, passos não muda o tempo total, só divide (`SegundosPorPasso`). Garrafas: passos é só leitura (um por garrafa). Ralo: cada passo é um skill check, e o tempo é o `segundosPorVolta`; `segundosParaConcluir` não vale em Play.
- Aplicar grava primeiro no prefab de origem e depois nas instâncias: instância sem override segue herdando e as com override recebem o valor. Um único Ctrl+Z desfaz cena e prefab. A janela não chama `MarkSceneDirty`: com ele, a cena continuaria suja depois do Undo.
- Prefab antigo é reserializado no primeiro Aplicar: o da bromélia ganha `podeEsvaziar`, `passosParaEsvaziar` e `segundosParaEsvaziar` com os valores padrão, sem mudar comportamento.
- Calibração que estava no comentário removido do `FocosVivosBuilder` (taxa 0,0013 e alívio 0,055, com o custo de rota da Parte B): 30 s/foco vence com pico de 87%, 35 s/foco vence, 40 s/foco perde nas 10 sementes e o parado perde aos 3:21. Com a taxa em 0,0011, o parado leva 4:02.
- Conferido: suíte EditMode 266/266; na `SampleScene`, Aplicar e Ctrl+Z nos vasos, com cena e prefab voltando idênticos. Os testes de cena rodam só na cena temporária do Test Runner.
- Não conferido: partida em Play com valores alterados, inclusive pneu com 10 passos (pela leitura do código, o punch pode acumular escala com passo curto). Os dois menus também não foram rodados na cena real, porque salvam a cena e regravam HUD e textos; a garantia vem do teste acima.


### 2026-11-05 — Parte B na main (PR #2)

**Objetivo (pedido do usuário):** abrir uma PR e fazer o merge na `main`.

**O que entrou:** [PR #2](https://github.com/RichterEdu/dengue/pull/2), com merge commit `1b8af39`. Ela levou 4 commits: Beta 0.3, Parte A, barra de ocupação no topo e Parte B (com a janela Configurações Globais). A PR #1 (Beta 0.3) ficou como merged junto, porque o commit dela entrou na `main`.

**Nenhum script, cena ou asset foi alterado nesta entrada.** Esta entrada ficou sem commit, na branch `parte-b/focos-vivos-e-chuva`.

**NÃO verificado antes do merge:** build exportada, sons de ouvido e a dificuldade com 30 focos jogada.


### 2026-11-05 — Spec da Parte C (nota, estrelas e recorde)

**Objetivo (pedido do usuário):** seguir o roteiro com a Parte C. O brainstorming terminou com o design aprovado seção por seção, e a spec aguarda a revisão do usuário.

**Decisões do usuário:**
- motor da repetição: bater a própria nota;
- pontos para o recorde e 3 metas com nome, todas exigindo vencer: Resposta rápida, Sem retrabalho e Hospital sob controle;
- os 30 focos continuam fixos: o sorteio de lugares entre cerca de 40 pontos saiu do roteiro;
- muletas: lista fechada no início e brilho de 3,5 m;
- nota no fim, com metas na pausa e aviso curto, sem pontos ao vivo na HUD.

**Arquivo novo:** `docs/superpowers/specs/2026-11-05-parte-c-nota-e-recorde-design.md`.

**Nenhum script, cena ou asset foi alterado nesta entrada.** O preview da interface rodou só em Play, sem salvar a cena.

**Observações técnicas:**
- A fonte LiberationSans SDF não tem ★ nem ☆: as estrelas serão sprites.
- Canvas em Screen Space Overlay não aparece no `screenshot`. Para o preview, os canvases passaram a Screen Space Camera, só em Play, com captura de 1920×1080 por RenderTexture.
- Limites de 20:00 e 85% são pontos de partida, a calibrar no simulador.


### 2026-11-05 — Plano da Parte C

**Objetivo (pedido do usuário):** escrever o plano de implementação da spec da Parte C.

**Arquivo novo:** `docs/superpowers/plans/2026-11-05-parte-c-nota-e-recorde.md`, com 7 tarefas: conta da nota, recorde, placar, telas, montagem da cena, simulador e calibragem, e verificação com build.

**Nenhum script, cena ou asset foi alterado nesta entrada.** O plano aguarda a revisão do usuário e a escolha da forma de execução.

**Observações técnicas:** `FluxoDeJogo.Update` passa a chamar `AvancarOcupacao(dt)`, que fica testável no EditMode. `NotaBuilder` reaproveita as peças `internal` do `MenuPausaBuilder`, e rodar de novo `Criar Menu de Pausa` ou `Criar Menu Inicial` exigirá rodar `Configurar Nota e Recorde`.


### 2026-11-05 — Parte C: verificação no jogo e na build, com a calibragem

**Objetivo (pedido do usuário):** executar a Parte C (nota, estrelas e recorde) e fechar com a verificação da Task 7: suíte inteira, vitória e derrota em Play, metas em Play, build e registro.

**Arquivos da Parte C:**
- Novos em `Assets/Scripts/`: `AvaliacaoDaPartida.cs`, `RecordeDoJogo.cs`, `PlacarDaPartida.cs`, `AvisoDeMeta.cs`, `TelaDaNota.cs`, `CartaoDeMetas.cs`, `RecordeNoMenu.cs`.
- Alterados em `Assets/Scripts/`: `FluxoDeJogo.cs` (`OcupacaoPico`, `AvancarOcupacao`), `FluxoDeJogoHUD.cs` (`placar`, `telaVitoria`, `telaDerrota`), `MenuPausa.cs` (`cartaoDeMetas`).
- Editor: `Assets/Editor/NotaBuilder.cs` (novo, menu `Dengue > Configurar Nota e Recorde`), `BalanceamentoSimulador.cs` (nota por ritmo) e `ConfiguracoesGlobais.cs` (grupo "Nota").
- Testes em `Assets/Editor/Testes/`: `NotaTestes.cs`, `RecordeTestes.cs`, `PlacarTestes.cs`, `InterfaceDaNotaTestes.cs`, `NotaBuilderTestes.cs` (novos); `BalanceamentoSimuladorTestes.cs` e `ConfiguracoesGlobaisTestes.cs` (alterados).
- Cenas: `Assets/Scenes/SampleScene.unity` e `Assets/Scenes/MenuInicial.unity`.
- Sprites: `Assets/Meus/Assets Gerados/Interface/Estrela Cheia.png` e `Estrela Vazia.png`.
- A verificação (Task 7) só alterou este arquivo. Sem commit.

**Configurações aplicadas (valores finais de `RegrasDaNota`, os padrões):** `limiteDeSegundos` 1200, `limiteDePico` 85, `pontosPorFoco` 100, `pontosPorVitoria` 1000, `tetoDeSegundos` 1800, `pontosPorSegundo` 2, `pontosPorPercentual` 20, `pontosPorEstrela` 500, `penalidadePorReenchido` 100. Na cena: `FocoListaHUD.comecarAberto` falso e `FocoProximityGlow.raioProximidade` 3,5 m. `CartaoMetas` é filho do `Cartao` do menu de pausa, para encolher junto.

**Calibragem (`Dengue > Simular Balanceamento`, regras padrão, 30 focos, 10 sementes). Nenhum limite mudou:**

| Ritmo | Resultado | Nota |
|---|---|---|
| 15 s + 10,6 s | vence em 16:30, pico 70% (70%-71%) | 7.720, 3 estrelas |
| 20 s + 10,6 s | vence em 19:44, pico 71% (71%-71%) | 7.312, 3 estrelas |
| 25 s + 10,6 s | vence em 22:57, pico 74% (73%-74%) | 6.366, 2 estrelas |
| 30 s + 10,6 s | vence em 26:11, pico 83% (82%-84%) | 5.798, 2 estrelas |
| 35 s + 10,6 s | vence em 29:24, pico 94% (93%-94%) | 4.692, 1 estrela |
| 40 s + 10,6 s | perde aos 8:48, com 9/30 focos | 900, 0 estrelas |
| 45 s + 10,6 s | perde aos 6:46, com 6/30 focos | 600, 0 estrelas |
| 50 s + 10,6 s | perde aos 6:05, com 5/30 focos | 500, 0 estrelas |
| parado | perde aos 3:21, com 0/30 focos | 0, 0 estrelas |

A referência foi cumprida: a 30 s por foco ganha a estrela do hospital e não a do tempo. A margem é curta: o pico máximo dessa linha é 84%, contra o limite de 85%.

**Testes:** suíte EditMode inteira 324/324, `failed="0"`.

**Play (Editor, entrando pela `MenuInicial`):**
- Vitória forçada: 22:16, pico 71%, tela com 6.508, 2 estrelas e "NOVO RECORDE"; `new RecordeDoJogo().Pontos` = 6508. A conta bate: 3.000 + 1.000 + 928 + 580 + 1.000.
- R recomeçou pela tecla real (simulada no Input System), e o recorde continuou. Voltar ao menu mostrou a linha do recorde, alinhada à assinatura (x 120). Sair do Play e entrar de novo manteve o recorde.
- Derrota real: 400 pontos (500 − 100 de um reenchido), "exige vencer" nas três metas, "Faltaram 25 focos" e "Recorde: 6.508".
- Metas: aviso abaixo da barra para Resposta rápida, Sem retrabalho e Hospital sob controle; esvaziar não derrubou a meta e reencher derrubou. A pausa real mostrou "perdida: 1 reenchido" e `20:17 de 20:00` em vermelho, além do recorde.
- Lista fechada ao entrar, Tab abre e fecha. Brilho de proximidade com 3,5 m.
- Janela baixa de verdade (1900×300, canvas 572 de altura): pausa aberta pelo Esc, os dois cartões com a mesma escala (0,386) e dentro da tela.
- Console sem erros durante o Play. Os canvases passaram a Screen Space Camera só nas capturas.
- O recorde de teste foi apagado (`Pontos` = 0) e `runInBackground` voltou a falso. `ProjectSettings` ficou igual ao `HEAD`.

**Build:** `Builds/Foco Zero/Foco Zero.exe` (fora do git), `BuildResult.Succeeded`, 2 cenas, cerca de 230 MB. Conferido na tela, pela janela do executável, com teclas e mouse enviados por script:
- destaque da mira: mira amarela e painel "Vaso de flor", com a areia eliminando o foco;
- névoa da chuva: chuva visível na captura; névoa não distinguida com certeza, conferir jogando;
- derrota com 1 foco: epílogo inteiro sem Esc, 100 pontos e "NOVO RECORDE";
- recorde gravado: depois de fechar e reabrir o executável, o menu mostrou "RECORDE 100";
- R recomeçou a partida.

Esse recorde de 100 pontos ficou no `PlayerPrefs` da build, que é separado do Editor.

**Observações técnicas:**
- A build registra `Shader error in 'Universal Render Pipeline/Terrain/Lit': Lost connection with shader compiler process. Suspected crash in FXC` num programa de fragmento do passe ForwardLit (d3d11), repetido na segunda build. O resultado segue `Succeeded`, o terreno apareceu normal nas capturas e o executável rodou em Direct3D 12 sem erro no log. Repetiu nas duas builds. Não foi investigado se ocorria antes da Parte C (primeira build na branch); o terreno apareceu normal.
- Os outros erros contados no resumo da build (4 na primeira, 2 na segunda, contando o do shader) são `Failed to handle /api/exec request`, os timeouts do CLI com o Editor ocupado na build.
- `eval_file` com a build estoura o timeout de 5 s do CLI, mas a build continua. O resumo foi gravado em arquivo.
- A faixa antiga "Todos os focos foram eliminados!" (`CompletePanel`, y 140 a 230, no canvas `DengueHUD`) fica visível atrás da tela de vitória, e a caixa nova (topo em 225) cobre 5 px dela. Não aparece no preview aprovado. Corrigido depois (Correção 1): a faixa apaga quando a tela final fica visível.
- "NOVO RECORDE" sai sem negrito no jogo; no preview aprovado estava em negrito. Corrigido depois (Correção 1): agora sai em negrito com recorde novo.
- A pausa sem recorde mantém a linha divisória e o espaço vazio embaixo no cartão de metas.
- `FluxoDeJogoHUD.jogador` continua vazio na cena e é achado em runtime, como antes.
- A build grava `LiberationSans SDF - Fallback.asset` com os glifos gerados no Play. O arquivo foi restaurado com `git checkout`, e isso volta a acontecer em toda build depois de jogar.

**Não verificado:** vitória na build; "Sem retrabalho" derrubado por chuva real, tanto no Play quanto na build (no Play foi pelo `ReencherEsvaziados`, e o tempo foi avançado por `AvancarOcupacao`); controle; sons; e se as metas parecem alcançáveis jogando de verdade. O brilho de emissão no foco mirado não foi medido na build: só a mira amarela e o painel foram confirmados.

**Correção 1 da tela final (2026-11-05):** (1) a faixa antiga "Todos os focos foram eliminados!" continua acesa durante o epílogo, mas apaga quando a tela final fica visível: `FluxoDeJogoHUD.TelaFinalVisivel` (`internal`; vitória ou derrota ativa e `SegurarTelaFinal` falso) e `DengueHUD.AlvoDaFaixaFinal`, com o campo opcional `telaFinal` achado por `FindAnyObjectByType` no `Start` (sem ele, o comportamento de antes). (2) "NOVO RECORDE" em negrito em `TelaDaNota`; "Recorde: N" fica normal. Só código, cena intacta. Testes novos em `InterfaceDaNotaTestes`: ParteC 63/63 e suíte inteira 330/330. Em Play, com os 30 focos concluídos: faixa em 1,00 durante o epílogo e 0,00 com a tela final visível e a vitória em alpha 1. Na build foi jogada só uma partida curta, terminada em derrota, e o recorde foi provado pela tela depois de reabrir o executável, não pelo Registro.

- **Onda final de correções da revisão (2026-11-05):** (1) `AvisoDeMeta.Visivel` (`internal`, `Restante > 0`) e `DengueHUD.DeslocamentoDoToast(bool)` (−54): o aviso de foco e dica desce abaixo da faixa de meta perdida, que ocupava o mesmo ponto (0, −126); campo opcional `avisoDeMeta` achado por `FindAnyObjectByType` no `Start` (sem ele, como antes), só código, cena intacta. (2) Testes novos: pico que sobe com pressão, reenchido fora da partida (não iniciada e encerrada), `CartaoDeMetas` com partida vencida com meta perdida e com derrota, `Cartao_SemPlacarENulos_NaoQuebra` agora sem placar de verdade, `Toast_Desce...` e `Aviso_Visivel...`. Nenhum defeito achado no cartão. Mutações conferidas e restauradas.


### 2026-11-05 — Parte C na main local

**Objetivo (pedido do usuário):** mesclar a Parte C na `main` localmente, sem push.

**O que entrou:** o trabalho da branch `parte-c/nota-e-recorde`, num único commit, porque a execução usou o índice do git, sem commits por tarefa. A branch foi removida depois do merge.

**Nenhum script, cena ou asset foi alterado nesta entrada.**

**Observações técnicas:** a suíte de 336 testes rodou na mesma árvore antes do commit. Não rodou de novo depois do merge, porque o Editor estava fechado e o merge só avança a `main` para esse commit.

**NÃO verificado antes do merge:** vitória na build, erro de shader do terreno (repetido em duas builds), névoa da chuva e destaque do foco mirado na build, chuva real derrubando "Sem retrabalho", deslocamento do toast em Play, sons e controle.


### 2026-11-06 — Spec dos itens do kit na mão

**Objetivo (pedido do usuário):** dar modelo 3D aos itens do kit, como o saco de lixo. O brainstorming terminou com o design aprovado, e a spec foi aprovada pelo usuário.

**Decisões do usuário:**
- balde de areia, tela com moldura e borrifador na mão direita, com gesto ao usar;
- depois de concluído, o ralo mostra a mesma tela com moldura, sob a grelha fechada e ajustada ao tamanho do ralo;
- sem carga: balde vazio, borrifador na mão e tela fora da mão; ao tentar usar, só o aviso que já existe.

**Arquivos novos:** `docs/superpowers/specs/2026-11-06-itens-do-kit-na-mao-design.md` e `docs/superpowers/plans/2026-11-06-itens-do-kit-na-mao.md`.

**Nenhum script, cena ou asset foi alterado nesta entrada.** As prévias rodaram só em Play, sem salvar a cena.

**Observações técnicas:**
- A cena aberta no Editor pode ser a `MenuInicial`: para a prévia, carregar a `SampleScene` em Play antes de procurar o Player.
- Com o Animator desligado em Play, dá para posar o braço por IK via `eval_file`, sem criar clipe. Valeu para a prévia, não para o jogo.
- Sob a grelha fechada, a moldura da tela quase some, e o que aparece é a malha entre as barras. Foi escolha do usuário, depois de comparar com a tela sobre a grelha.
- Ao menos um ralo da cena tem escala 1,21 × 2,11: uma peça nova filha do prefab herda esse esticamento.
- Olhar para o chão ainda não foi testado com o braço real. A spec prevê o braço acompanhar o olhar.
- `PlayerSettings.runInBackground` foi restaurado para falso depois das prévias (conferido em `ProjectSettings.asset`).


### 2026-11-06 — Itens do kit na mão

**Objetivo (pedido do usuário):** dar modelo 3D, pose e gesto ao balde de areia, à tela com moldura e ao borrifador, como o saco de lixo, e mostrar a mesma tela sob a grelha do ralo concluído.

**Arquivos novos:**
- `Assets/Scripts/RegraDoItemNaMao.cs` — regra pura: qual item, quando há gesto, estado do Animator e giro do ombro.
- `Assets/Scripts/ItensNaMao.cs` — componente no Player.
- `Assets/Editor/ItensNaMaoBuilder.cs` — menu **Dengue > Configurar Itens na Mão**.
- `Ferramentas/Blender/gerar_itens.py` e `Assets/Meus/Assets Gerados/Itens/` — modelos, texturas e materiais.
- `Assets/Editor/Testes/RegraDoItemNaMaoTestes.cs`, `ItensNaMaoTestes.cs` e `ItensNaMaoAssetsTestes.cs`.

**Arquivos alterados:** `Ferramentas/Blender/gerar_agente.py`, `AgenteSaude.fbx`, `Agente_AnimController.controller`, `Ralo.prefab`, `Assets/Scenes/SampleScene.unity`, `Assets/Editor/FocosBlenderBuilder.cs`, `Assets/Editor/PoseDeCarga.cs` e `Assets/Editor/PlayerBuilder.cs`.

**Configurações aplicadas:**
- Tela: 29 × 29 cm, escala 0,67 na mão, escala local 1 e centro a 2,9 cm no ralo, sob a grelha.
- Sem carga: balde vazio, borrifador na mão e tela fora da mão.
- Camada `ItemNaMao` com 6 estados: peso em 0,15 s e troca de estado em 0,12 s.
- `fatorDoOlhar` 1: o ombro acompanha o olhar.
- Gesto do balde: 28° e 8 cm de subida.
- Névoa: alfa 0,6 e tamanho de 0,05 a 0,12.

**Testes:** suíte EditMode verde (detalhe no commit).

**Verificado em Play, com mouse virtual segurando o clique real (`SampleScene`):**
- vaso e bromélia: gesto, partículas e parada ao soltar, com o foco concluindo;
- ralo: gesto no skill check, e a tela some da mão quando o erro gasta a última;
- sem carga: "Acabou: Areia" e "Acabou: Água sanitária", sem gesto;
- pausa no meio do gesto: não prende a pose;
- pneu e saco: pesos das camadas certos depois da reexportação;
- os quatro ralos concluídos: `TelaInstalada` ativa, herdando a escala 1,21 × 2,11;
- olhar a +80° e -60°: o braço não entra na câmera;
- andar (2,80 m/s) e correr (5,80 m/s) com teclado virtual, depois da reexportação;
- pausa com a Tela olhando para baixo: o ombro ficou parado por milhares de quadros, porque o Animator reescreve a pose com `timeScale` 0. A revisão final levantou a dúvida e a medição a descartou.

**O que NÃO foi verificado:**
- partida inteira com teclado e mouse de verdade;
- build exportada;
- movimento dos gestos e da locomoção visto em tempo real: as capturas são quadros isolados;
- controle (gamepad).

**Observações técnicas:**
- **Partículas em espaço local:** o sistema lê o emissor antes do `LateUpdate`, onde o ombro gira. Em espaço de mundo, a névoa nascia longe do bico ao olhar para baixo.
- **Cena suja trava o Test Runner:** o Unity recalcula o layout da UI ao abrir a `SampleScene`, a cena fica suja e o Test Runner abre a janela "Scene(s) Have Been Modified". Sem foco, o Editor não avança. Antes de testar por CLI, abrir uma cena vazia.
- **`run_tests` responde `success: true` com 0 testes** quando a compilação falha. Conferir o total.
- **`Temp/` é apagada ao reabrir o Editor,** inclusive as capturas.
- **`UsandoBalde` e o alcance do braço:** a pose do balde está perto do limite do braço. Subir a mão funciona; avançar, não.
- **`Saco_Plastico.mat` e os `Item_*.mat`:** o Unity regrava `_Color` com arredondamento. Não tem efeito visual.
- Em `eval_file`, membros `internal` não são acessíveis: usar reflexão.
- Depois de **Dengue > Configurar Player**, rodar também **Dengue > Configurar Itens na Mão**.


### 2026-11-10 — Nascimento seco (a maioria dos focos já nasce esvaziada)

**Objetivo (pedido do usuário):** ~90% dos focos de criadouro nascerem JÁ esvaziados (secos), como se a comunidade tivesse cuidado da vizinhança antes do agente chegar; só uma contagem fixa sorteada nasce com água. A chuva ficou COMO ESTÁ: reenche todos os esvaziados (o pedido original de "50% na chuva" foi descartado).

**Decisões do usuário:**
- **10% com água / 90% secos**, com **contagem fixa sorteada** (não chance por foco, nunca 0 com água);
- **os ralos ficam sempre com água** (pedido posterior do usuário, na mesma rodada): ficam fora do sorteio, até com a fração em 0;
- a chuva reenche tudo, como antes (`ReencherEsvaziados()` não mudou).

**Arquivos alterados em `Assets/Scripts/`:**
- `Interactable.cs` — `Start()` chama `NascerVazio()` em Play quando o sorteio não deu água ao foco (guard `Application.isPlaying`: fora do Play ou sem gerenciador, nasce cheio, e os testes EditMode continuam determinísticos); `NasceComAgua()` privado pergunta ao gerenciador; novo `public virtual bool SempreNasceComAgua => false` (o ralo sobrescreve para true); `NascerVazio()` (`internal`) só marca `Esvaziado = true` e chama o hook `AoNascerVazio()` — **sem `OnEsvaziado`** (não houve esvaziamento: sem splash, sem post de rede social, sem zumbido, nasce parado em ovo pesando 0, as Mãos dão `JaVazio` e a ferramenta certa conclui de vez).
- `GerenciadorDeFocos.cs` — campo `[Range(0,1)] fracaoNascendoComAgua = 0,1`; `ComAguaNoNascimento(total)` = `Clamp(RoundToInt(fracao × total), 1, total)` (0 se total ou fração ≤ 0); `SortearNascimento(rng)` com Fisher-Yates sobre os focos de pé (`!IsComplete` **e `!SempreNasceComAgua`** — o ralo não entra no pool), definindo quem nasce seco (HashSet `nascidosSecos`); `NasceComAgua(foco)` com sorteio lazy na 1ª consulta (`new System.Random()` se rng null) e `true` para `SempreNasceComAgua`. Foco registrado DEPOIS do sorteio nasce com água, como sempre.
- `VasoInteractable`, `GarrafaInteractable`, `PneuInteractable`, `BromeliaInteractable`, `CaixaDaguaInteractable`, `PiscinaInteractable` — cada um com `AoNascerVazio()` escondendo o visual de água (água do pneu/vaso/caixa/piscina, filhos "agua" das garrafas, poças da bromélia com `secas` já no fim). Bromélia, caixa d'água e piscina ganharam `PodeReencher => base.PodeReencher && CurrentClicks == 0` e `AoReencher()` (chuva não enche por cima de secagem/fechamento começado pela metade) — vaso, garrafa e pneu já tinham guards equivalentes.
- `RaloInteractable.cs` — só `SempreNasceComAgua => true` (o ralo nasce sempre molhado e nunca entra no sorteio). Os overrides de água desta mesma rodada (`AoNascerVazio`, `AoReencher`, `PodeReencher`) foram removidos: com `podeEsvaziar` desligado no prefab, `Esvaziado` nunca fica verdadeiro no ralo, então eram inalcançáveis.

**Arquivo novo:** `Assets/Editor/Testes/NascimentoTestes.cs` (categoria ParteB): contagem fixa (18→2, 10→1, 1→1, 0→0; fração 0/0,5/1), sorteio determinístico por semente, foco fora do sorteio nasce com água, `NascerVazio` sem evento/zumbido/peso, água some e a chuva devolve (com escala e estágio Larva), ferramenta certa conclui, pneu seco pega na hora e nos braços a chuva não reenche, `Start` fora do Play não sorteia, ralo sempre com água e fora da conta do sorteio (fração 0,4: 1 de 3 comuns, não 2 de 4), e o visual de cada tipo (garrafas, bromélia com guard de secagem, caixa, piscina).

**Configurações aplicadas:** `fracaoNascendoComAgua = 0,1` (default; os objetos já serializados herdam o valor). O sorteio conta só os focos que podem nascer secos (a cena tem **4 ralos**, que ficam sempre molhados, fora da conta). Nenhuma cena ou prefab foi editado.

**Testes:** compilação verificada fora do Editor com o Roslyn do VS, usando refs e defines extraídos dos `.csproj` gerados pelo Unity (o `Assembly-CSharp-Editor.csproj`, regenerado às 00:18, já inclui o teste novo): Assembly-CSharp (110 fontes) e Assembly-CSharp-Editor (76 fontes) compilam sem erros. Depois, pelo **CLI oficial do Unity** (`unity command` no Editor aberto): `recompile` OK e Test Runner > EditMode **ParteB 195/195 verdes** e **suíte EditMode completa 427/427 verdes** (0 falhas, 0 pulados), sem regressões.

**Observações técnicas:**
- `SortearInicio` já filtra `Esvaziado`, então focos nascidos secos não ganham estágio no começo; o epílogo da rede social classifica por eliminação, não por `Esvaziado` (focos secos restantes ficam "pendentes", coerente).
- **O ralo (4 na cena) fica sempre molhado:** `SempreNasceComAgua` o tira do pool do sorteio — a fração de 10% vale só para os focos que podem nascer secos. No prefab do ralo, `podeEsvaziar` é 0: ele nunca fica `Esvaziado`, e por isso não precisa de visual de água no nascimento seco nem de `AoReencher`.
- Com isso ficam ~7 focos molhados no começo: os 4 ralos + 3 sorteados dos ~28 que podem nascer secos. `focosComMosquitoNoInicio = 3` sorteia os mosquitos entre esses molhados, então ralos podem zumbar desde o início (ajustável no Inspector).
- O BalanceamentoSimulador simula todos os focos molhados (pior caso), sem mudança.
- Para validar compilação sem abrir mão do Editor ocupado: `csc.exe` do VS (`MSBuild\Current\Bin\Roslyn`) + rsp montado dos `.csproj` do Unity (`/noconfig /nostdlib+ /langversion:9.0`, refs por HintPath e o `ProjectReference` trocado pelo DLL recém-compilado). O `csc` do `MonoBleedingEdge` não roda aqui (falha em `System.Text.Encoding.CodePages`).

### 2026-11-11 — Spec dos minigames dos focos (leva 1)

**Objetivo:** dar a cada foco uma ação própria, diferente do skill check do ralo. Leva 1: base comum de minigame, pneu (balançar no ritmo até a água transbordar), bromélia (mirar o borrifador nas pocinhas) e caixa d'água (carregar a tampa e tampar). Vaso, garrafas e piscina ficam para a leva 2.

**Arquivos alterados:** `docs/superpowers/specs/2026-11-11-minigames-leva-1-design.md` (novo). Nenhum script, cena ou prefab foi editado.

**Configurações aplicadas:** nenhuma.

**Observações técnicas:**
- Decisões do usuário: formato depende do foco (os três desta leva são na cena 3D); ir mal só perde tempo (sem gastar carga); a tampa da caixa d'água continua encostada na caixa.
- O skill check do ralo passa para a interface comum `IMinigame`, com a regra do anel intacta.
- Próximo passo: plano de implementação, depois da revisão do spec pelo usuário.

### 2026-11-11 — Plano dos minigames dos focos (leva 1)

**Objetivo:** plano de implementação do spec dos minigames (leva 1), em 8 tarefas (0 a 7).

**Arquivos alterados:** `docs/superpowers/plans/2026-11-11-minigames-leva-1.md` (novo); `docs/superpowers/specs/2026-11-11-minigames-leva-1-design.md` (dois ajustes: tampar não depende do `requiredClicks` da cena; os passos do pneu são o `RequiredClicks` dele). Nenhum script, cena ou prefab foi editado.

**Configurações aplicadas:** nenhuma.

**Observações técnicas:**
- A Tarefa 0 pede a decisão do usuário sobre commitar o nascimento seco, pendente nos mesmos arquivos que o plano edita.
- Testes novos na categoria `Minigames`.

### 2026-11-11 — Minigames: base IMinigame e ralo no adaptador

**Objetivo:** Tarefa 1 do plano dos minigames (leva 1). O skill check do ralo, escrito direto no `Interactor`, vira um caso da base comum `IMinigame`, para os próximos focos (pneu, bromélia, caixa d'água) ganharem minigames próprios. O ralo se comporta como antes, agora por um adaptador.

**Arquivos alterados:**
- `Assets/Scripts/Minigame.cs` (novo) — `EntradaDoMinigame` (Olhar, Segurando, Apertou), `EstadoDoMinigame` (EmAndamento, Passo, Errou), `IMinigame` (`Atualizar(dt, entrada)`, `FracaoDoPasso`, `PodeSair`) e `IMinigameComMira` (`Mira` em graus a partir do olhar inicial). C# puro, o tempo entra pelo `dt`.
- `Assets/Scripts/SkillCheck.cs` — `MinigameDeSkillCheck : IMinigame` embrulha a `SessaoDeSkillCheck` (`Sessao`): aperto → `Apertar`, senão `Atualizar(dt)`; `Acertou` → `Passo`, `Errou` → `Errou`; `PodeSair` false; `FracaoDoPasso` 0.
- `Assets/Scripts/Interactable.cs` — saem `UsaSkillCheck`, `SegundosPorVoltaDoCheck`, `ErrarSkillCheck`, `CancelarSkillCheck`; entram `CriarMinigame(kit, semente)` (null = segurar; fábrica pura, que não muda o foco: o Interactor também a chama só para perguntar), `AoIniciarMinigame(sessao)` (chamado ao aceitar a sessão, antes do primeiro `Atualizar`), `AoErrarMinigame`, `AoSairDoMinigame`, `AoAtualizarMinigame`, `DicaDoMinigame`, `SegundosEstimados` e `ProgressoCom(kit, fracao)` (as duas contas da barra que estavam no `Interactor.AtualizarProgresso`).
- `Assets/Scripts/RaloInteractable.cs` — `CriarMinigame` devolve o adaptador; `AoErrarMinigame` = o antigo `ErrarSkillCheck` (volta ao começo e gasta a tela); `AoSairDoMinigame` = o antigo `CancelarSkillCheck`; `SegundosEstimados => RequiredClicks × segundosPorVolta`.
- `Assets/Scripts/Interactor.cs` — `MinigameAtivo`; `SkillCheckAtivo` = a sessão do adaptador; `IniciarMinigame(foco)` (internal; o foco vira o alvo, imobiliza e guarda o olhar inicial), `AvancarMinigame(dt, entrada, sair)` (internal; alvo destruído/inativo encerra, `sair && PodeSair` sai, `Passo` → `AvancarCom` e resultado diferente de `Avancou` encerra, `Errou` → `AoErrarMinigame` + `OnErrouSkillCheck`, `AoAtualizarMinigame` todo quadro, mira aplicada por `DefinirOlhar`) e `AoBloquear()` (internal; o corpo do antigo bloco `Bloqueado` do `Update`, saindo do minigame sem culpa). O `Update` só monta a entrada e chama `AvancarMinigame` (pausado, nada anda). Só um aperto NOVO, conferido, num foco com minigame começa a sessão no lugar do segurar (`ConferirFoco(foco, apertoNovo)`, internal); a mira chegando ao foco com o botão já apertado não começa nada, não confere (sem aviso) e prende o segurar até soltar. Ao encerrar, segurar só volta a valer depois de soltar o botão. `Avancando` fica verdadeiro com minigame ativo que não seja o skill check. Kit e `ControladorJogador` são achados no primeiro uso (no EditMode o `Start` não roda). A barra usa `ProgressoCom(kit, fração)`, com a fração do minigame quando há um ativo.
- `Assets/Scripts/DengueHUD.cs` — a linha de ação "encaixar a grelha" passa a sair para `foco is RaloInteractable` (era `UsaSkillCheck`).
- `Assets/Editor/BalanceamentoSimulador.cs` — a rota com kit soma `foco.SegundosEstimados` (era `SegundosPorPasso × RequiredClicks`).
- `Assets/Editor/Testes/MinigameNoInteractorTestes.cs` (novo, categoria `Minigames`), `Assets/Editor/Testes/SkillCheckTestes.cs` (adaptador e chamadas novas), `Assets/Editor/Testes/ConfiguracoesGlobaisTestes.cs` (lê o campo `segundosPorVolta` do ralo).

**Configurações aplicadas:** nenhuma. Nenhuma cena ou prefab foi editado; nenhum campo serializado novo.

**Testes:** `MinigameNoInteractorTestes` (13: PassoViraAvancarCom, UltimoPassoEncerra, SairMantemOsCliques, SairIgnoradoSemPodeSair, ErroChamaOGanchoENaoGastaCarga, AlvoDestruidoEncerraSemExcecao, FocoSemMinigameNaoInicia, BloqueioEncerraSemGastarCarga, MiraAplicadaNoOlhar, ApertoNovoIniciaOMinigame, SegurarSemApertoNaoIniciaOMinigame, FocoSemMinigameSegueNoSegurar, CriarMinigameNaoMudaOFoco) e, em `SkillCheckTestes`, AdaptadorAcertoViraPasso e AdaptadorErroViraErrou. Pelo CLI do Unity em batch mode: `MinigameNoInteractorTestes` 13/13 e suíte EditMode completa **442/442** (0 falhas).

**Observações técnicas:**
- Ordem no quadro do minigame: `Atualizar` → `AoAtualizarMinigame` → mira → aplicar o resultado. Assim o `AoErrarMinigame`/`AoSairDoMinigame` vem depois do visual do quadro e pode desfazê-lo.
- Como antes, só o aperto começa o skill check: passar a mira para um ralo segurando o botão não começa nada (o ponteiro correria sem o jogador querer e o erro gastaria a tela). Ao terminar, segurar não avança o ralo: é preciso soltar e apertar de novo.
- Pendente: jogar um ralo no Editor (acerto, erro gastando a tela, celular no meio).

**Atualização:** depois desta entrada o `IMinigame` ganhou um quarto membro, `Agindo` (Tarefa 5, "bromélia de mirar o borrifador"), e o `Avancando` do Interactor passou a seguir `minigame.Agindo` (não mais "minigame ativo que não seja o skill check"). Na revisão final (2026-11-12), um clique rápido (apertar e soltar no mesmo quadro) num foco com minigame voltou a começar a sessão.

### 2026-11-11 — Minigames: onda do pneu (lógica pura)

**Objetivo:** lógica do minigame do pneu: a água é um oscilador amortecido; empurrar o pneu no ritmo faz a onda crescer até transbordar, e o transbordar esvazia o `Nivel`. Só a lógica; a ligação com o `PneuInteractable` e o visual ficam para tarefas seguintes.

**Arquivos alterados:** `Assets/Scripts/OndaDoPneu.cs` (novo, implementa `IMinigame`), `Assets/Editor/Testes/OndaDoPneuTestes.cs` (novo, categoria `Minigames`).

**Configurações aplicadas:** nenhuma. Nenhuma cena ou prefab foi editado. Padrões do construtor mantidos como no plano: `periodo` 0,9, `amortecimento` 0,12, `ganho` 15, `limiarDaBorda` 1, `taxa` 1,4, `grausPorSegundoCheio` 180.

**Testes:** `OndaDoPneuTestes` (6): Parada_NaoDerrama, EmFase_Esvazia (3 passos, esvazia em ~5,4 s simulados, faixa aceita 3 a 7 s), ContraFase_NaoPassaDaBorda, OndaMorreSozinha, QuadroLongo_NaoExplode, FracaoDoPasso_SobeDentroDoTerco. Pelo CLI do Unity em batch mode: 6/6 e suíte EditMode completa 448/448 (0 falhas).

**Observações técnicas:**
- Euler semi-implícito em subpassos de no máximo 1/120 s; `passos` < 1 vale 1; `dt <= 0` não muda nada.
- O `Nivel` nunca passa do limite do próximo passo num mesmo `Atualizar` (no máximo um `Passo` por quadro); o resto fica para o quadro seguinte.
- Ir mal só custa tempo: nada aqui gasta carga nem reinicia o foco.
- Entradas extremas: `dt` não finito ou <= 0 não muda nada; `dt` é limitado a 0,25 s (pausa/engasgo não trava o quadro); `Olhar` não finito conta como parado. Testes: DtNaoFinito_NaoMudaNada, OlharNaoFinito_ContaComoParado, DtEnorme_EhLimitado (suíte completa 451/451).

### 2026-11-11 — Minigames: pneu de balançar

**Objetivo:** Tarefa 3 do plano dos minigames (leva 1). O jogador esvazia o pneu balançando-o no ritmo da água (a `OndaDoPneu` da tarefa anterior), no lugar de segurar o botão.

**Arquivos alterados:**
- `Assets/Scripts/PneuInteractable.cs` — campos serializados `periodoDaOnda` (0,9), `inclinacaoMaxima` (25) e `deslocamentoDaAgua` (0,05 m). `CriarMinigame` (fábrica pura) devolve `new OndaDoPneu(RequiredClicks, periodoDaOnda)` só com as Mãos num pneu não esvaziado; `ConviteDoMinigame` = "Clique para balançar o pneu" na mesma condição; `DicaDoMinigame` = "Balance o pneu ← →   |   Botão direito: sair"; `SegundosEstimados => Esvaziado ? 0 : 6`. `AoIniciarMinigame` guarda pose, ponto de apoio (ponto mais baixo dos `Renderer.bounds`), eixo (horizontal da câmera ao pneu; `Vector3.forward` sem `Camera.main`) e a água cheia (lida uma vez). `AoAtualizarMinigame` gira o pneu em torno do apoio por `Inclinacao × inclinacaoMaxima` (preso a ±), dá à água escala Y `Nivel` e a desloca para o lado baixo, e derramando solta `EstourarParticulas` no lado baixo a cada 0,25 s (só no Play). A pose original volta em `AoSairDoMinigame` (água na escala `1 − CurrentClicks/RequiredClicks`), no `AoTerminarAcao` (o Interactor encerra o último passo sem chamar o `AoSairDoMinigame`) e a água cheia também no `AoReencher`. Os ganchos não lançam em qualquer ordem.
- `Assets/Scripts/Interactable.cs` — `ConviteDoMinigame(kit)` (texto da linha de ação ao mirar um foco cujo aperto novo começaria um minigame; null = "segure o clique").
- `Assets/Scripts/RaloInteractable.cs` — `ConviteDoMinigame` devolve `DengueHUD.AcaoSkillCheck`.
- `Assets/Scripts/DengueHUD.cs` — `AcaoDoFoco` usa `ConviteDoMinigame` no lugar do `foco is RaloInteractable`; com minigame ativo, a linha de ação mostra `DicaDoMinigame` do alvo (o nome do foco não muda).
- `Assets/Editor/Testes/PneuBalancarTestes.cs` (novo, categoria `Minigames`), `Assets/Editor/Testes/MinigameNoInteractorTestes.cs` (convite do ralo).

**Configurações aplicadas:** nenhuma. Nenhuma cena ou prefab foi editado; campos novos com valor padrão no código.

**Testes:** `PneuBalancarTestes` (10): MaosNoPneuCheioCriaAOnda, PneuEsvaziadoNaoTemMinigame, OutraFerramentaNaoTemMinigame, ConviteSoComMaosEPneuCheio, UltimoPassoEsvaziaEEncerra, SairDevolveAPose, BloqueioNoMeioDevolveAPose, AguaAcompanhaONivel, ReenchidoRecomecaDoZero, GanchosForaDeOrdemNaoLancam; mais ConviteDoRaloEhOTextoDoSkillCheck. Suíte EditMode completa em batch mode: **462/462**.

**Observações técnicas:**
- `SegundosEstimados` do pneu (6 s) passa a valer na rota do `BalanceamentoSimulador`; nenhum teste fixava o tempo do pneu.
- Pendente no Editor: balançar um pneu com mouse e com controle, conferir cerca de 5 s no ritmo e a onda morrendo parado, e ajustar `ganho`/`taxa` da `OndaDoPneu` se preciso (anotar os valores finais).
- Retomada: `OndaDoPneu` ganhou o parâmetro opcional `nivelInicial` (último; limitado a 0..1, não finito = 1; os passos que o nível implica contam como dados). O pneu o cria com `1 − CurrentClicks/RequiredClicks`, então, depois de sair com passos fechados, a nova sessão continua do nível guardado (a água não volta a cheia) e esvazia junto com o `CurrentClicks`. Testes novos: `ComecaDoNivelInicial`, `NivelInicialForaDaFaixaEhLimitado`, `RetomarContinuaDoNivelGuardado`; suíte completa 465/465.

**Atualização:** o `CriarMinigame` do pneu passou a `new OndaDoPneu(RequiredClicks, periodoDaOnda, amortecimentoDaOnda, ganhoDaOnda, limiarDaBorda, taxaDeDerrame, grausPorSegundoCheio, nivel)` (retomada e, na revisão final de 2026-11-12, os números da onda no Inspector), e o `PneuBalancarTestes` tem hoje 15 testes (mais `RetomarContinuaDoNivelGuardado` e os 4 da revisão final).

### 2026-11-11 — Minigames: mira do borrifador

**Objetivo:** Tarefa 4 do plano dos minigames (leva 1). Lógica pura do minigame de mirar o borrifador e secar as pocinhas da bromélia (a `BromeliaInteractable` a usa numa tarefa seguinte; aqui não há cena, prefab nem MonoBehaviour).

**Arquivos alterados:**
- `Assets/Scripts/MiraDoBorrifador.cs` (novo) — `MiraDoBorrifador : IMinigame, IMinigameComMira`. `Mira` (yaw, pitch em graus; `Mira.x += Olhar.x`, `Mira.y -= Olhar.y`) presa a um cone de raio `max(cone, maior |pocinha| + raio + 2)`. Segurando, seca a pocinha molhada mais próxima (distância euclidiana em graus) a até `raio` da mira; `Umidade(i)` (1 cheia .. 0 seca) fica guardada ao soltar; `PocaSecandoAgora` (-1 = nenhuma), `Total`, `Secas`. O k-ésimo `Passo` sai ao secar a `ceil(Total·k/passos)`-ésima pocinha, no máximo um por `Atualizar` (os devidos saem um por chamada nas seguintes; sem pocinhas todos são devidos desde o início). `FracaoDoPasso` (0..1) = secas desde o último passo mais a umidade já tirada da pocinha em andamento, sobre as que faltam para o próximo; 0 logo após um Passo.
- `Assets/Editor/Testes/MiraDoBorrifadorTestes.cs` (novo, categoria `Minigames`) e os `.meta` dos dois.

**Configurações aplicadas:** nenhuma. Padrões do construtor: `cone` 30, `raio` 4.

**Testes:** `MiraDoBorrifadorTestes` (18): MiraPresaAoCone, ConeCrescePraAlcancarTodasAsPocinhas, ConvencaoDaMira_YInvertido, SegurarForaNaoSeca, SemSegurarNaoSeca, SecaAMaisProxima, EscolheAMaisProximaDentroDoRaio, SoltarNoMeioGuardaAUmidade, PassosNasContagensCertas, MaisPassosQuePocinhas, SemPocinhasTodosOsPassosSaem, FracaoDoPasso_FicaEmZeroAUm, DtInvalidoNaoMudaNada, DtEnorme_SecaNoMaximoUmaPocinha, OlharNaoFinitoContaComoParado, ArraysDeTamanhosDiferentes, ArgumentosRuins_ViramPadrao, ArraysSaoCopiados. Pelo CLI do Unity em batch mode: 18/18 e suíte EditMode completa 483/483 (0 falhas).

**Observações técnicas:**
- Entradas ruins: `dt` <= 0 ou NaN não muda nada (só pode sair um Passo já devido); `Olhar` não finito conta como 0; `dt` enorme seca no máximo a pocinha sob a mira. Construtor: `pocas` null = vazio; arrays de tamanhos diferentes usam só as primeiras `min` entradas; tempo para secar <= 0 ou não finito vale 0,01 s; `passos` < 1 vale 1; coordenada de pocinha não finita vira 0; os arrays são copiados.


### 2026-11-11 — Minigames: bromélia de mirar o borrifador

**Objetivo:** Tarefa 5 do plano dos minigames (leva 1). A bromélia troca o "segurar enquanto a barra enche" pela `MiraDoBorrifador`: com a água sanitária, o jogador aponta o borrifador para cada pocinha entre as folhas e segura até ela secar.

**Arquivos alterados:**
- `Assets/Scripts/BromeliaInteractable.cs` — campos `segundosPorPoca` (0,5), `segundosDoCopoCentral` (1,2; a última de `pocas`) e `raioDeAcerto` (4°). `CriarMira(olho, visao)` (internal) monta a sessão com as pocinhas ATIVAS, na ordem de `pocas`: `local = Inverse(visao)·(posição − olho)`, yaw `atan2(x, z)`, pitch `atan2(−y, √(x²+z²))` (positivo = abaixo, como o `ControladorJogador.Pitch`), e `passos = RequiredClicks − CurrentClicks` (mín. 1: retomar cobre só o que falta). `CriarMinigame` (fábrica pura) usa o olho do `Camera.main` e a visão `Euler(jogador.Pitch, jogador.Yaw, 0)` do `ControladorJogador` do kit (sem ele, a rotação da câmera); null sem kit, com ferramenta não aceita, concluída, sem pocinha ativa ou sem câmera. `AoIniciarMinigame` guarda a sessão, o mapa índice da sessão → índice em `pocas` e a escala local original de cada uma; `AoAtualizarMinigame` escala X e Z por `Umidade(i)` e, em 0, desativa com a escala original de volta; `AoSairDoMinigame` devolve as meio secas à escala original (as secas ficam secas). `OnInteract`: com sessão, não seca na ordem fixa, e o passo que conclui desativa o que sobrou e encerra a sessão; sem sessão, ordem fixa pulando as já secas (`secas` conta o que está mesmo sem água; fora do Play a pocinha some na hora). `AvancarCom` recusado no meio da sessão também a encerra. `ConviteDoMinigame` = "Clique para borrifar as pocinhas"; `DicaDoMinigame` = "Segure para borrifar as pocinhas   |   Botão direito: sair"; `SegundosEstimados` = (n − 1)·0,5 + 1,2 + 0,4·n (n = 0: o da base).
- `Assets/Scripts/Minigame.cs` — `IMinigame.Agindo` (a sessão faz a sua ação neste quadro).
- `Assets/Scripts/Interactor.cs` — `Avancando = minigame != null && minigame.Agindo` no `AvancarMinigame` (no lugar do `is MinigameDeSkillCheck`): som de ação (`SomDoKit`) e gesto/partículas do item na mão (`ItensNaMao`) seguem a sessão. O `ItensNaMao.cs` não mudou.
- `Assets/Scripts/SkillCheck.cs` (`MinigameDeSkillCheck.Agindo` = false), `Assets/Scripts/OndaDoPneu.cs` (true), `Assets/Scripts/MiraDoBorrifador.cs` (`Agindo` = o `Segurando` do último `Atualizar` com `dt` válido; false antes do primeiro e depois de um `dt` inválido).
- `Assets/Editor/Testes/BromeliaBorrifarTestes.cs` (novo, categoria `Minigames`); asserts novos em `MinigameNoInteractorTestes`, `MiraDoBorrifadorTestes`, `SkillCheckTestes` e `OndaDoPneuTestes`.

**Configurações aplicadas:** nenhuma. Nenhuma cena ou prefab foi editado; campos novos com valor padrão no código. No prefab real (5 pocinhas): `SegundosEstimados` = 4·0,5 + 1,2 + 0,4·5 = 5,2 s.

**Testes:** `BromeliaBorrifarTestes` (17): CriaAMiraComAsPocinhasAtivas, AngulosDaPocinha, SemPocinhasNaoTemMinigame, SemFerramentaOuSemCameraNaoTemMinigame, NascidaSecaNaoTemMinigame, PocinhaAtingidaEAQueSeca, AvancandoSoSegurando, VisualSegueAUmidade, UltimoPassoSecaOQueSobrou, SairGuardaAsSecasEDevolveAMeia, RetomarUsaSoOQueFalta, CaminhoAntigoMantemAOrdemFixa, CaminhoAntigoPulaAsJaSecas, DoseSoNoFim, ConviteEDica, SegundosEstimadosSomaOsTempos, GanchosForaDeOrdemNaoLancam; mais AvancandoSegueOAgindoDaSessao e AgindoSoSegurando. Suíte EditMode completa em batch mode: **502/502**.

**Observações técnicas:**
- A cena aberta no modo batch tem a sua "Main Camera": os testes pelo Interactor usam o `Camera.main` que houver como olho (e criam uma pelo `TesteUtil` só se não houver nenhuma), com as pocinhas em volta dele.
- Pendente no Editor: tratar uma bromélia com mouse e com controle; conferir que toda pocinha é alcançável dentro do cone e que a névoa sai do bico só enquanto segura.

### 2026-11-11 — Minigames: caixa d'água com tampa carregável

**Objetivo:** Tarefa 6 do plano dos minigames (leva 1). A caixa d'água deixa de fechar segurando enquanto a tampa desliza: o agente pega a tampa encostada na lateral (um aperto), carrega nos braços como o pneu e, com um segundo aperto segurado na caixa, tampa. Não é `IMinigame`: a tampa é o segundo objeto carregável do `KitDoAgente`, ao lado do `PneuCarregado`.

**Arquivos alterados:**
- `Assets/Scripts/KitDoAgente.cs` — `TampaCarregada` (a caixa dona da tampa), `MaosOcupadas` (pneu ou tampa), `PegarTampa(caixa)` (false com as mãos ocupadas; mesmos limites do pneu: `multiplicadorCarregando`, sem pular, stamina de corrida), `DevolverTampa()` (volta à lateral, movimento restaurado), `SoltarTampaColocada()` (internal, a caixa chama ao tampar, antes do `OnCompleted`), eventos `OnTampaPega`, `OnTampaDevolvida`, `OnTampaColocada`; `OnMudou` em cada mudança. `Selecionar`, `Pegar(pneu)` e a stamina passam a usar `MaosOcupadas`.
- `Assets/Scripts/CaixaDaguaInteractable.cs` — campos `segundosParaTampar` (0,8), `posNosBracos` e `rotNosBracos` (zero). `TampaNosBracos`; `AceitaComMaosOcupadas(kit) => kit.TampaCarregada == this`; `AvancarCom` com kit: tampa encostada → `PegarTampa` → `Pegou`; tampa nos braços deste kit → solta do kit, a tampa volta a ser filha da caixa (no lugar de mundo) e `currentClicks = requiredClicks − 1` antes do passo da base, que conclui (`Concluiu`, eventos e conta da carga normais) e faz o arco do `MoveRoutine` até a boca. `PrenderTampaEm`/`DevolverTampaALateral` (internal) guardam e devolvem o pai e a escala local e desligam/religam só os colliders que estavam ligados. `SegundosPorPasso => TampaNosBracos ? segundosParaTampar : 0`, `ProgressoCom` = só a fração, `PodeReencher` falso com a tampa nos braços, `SegundosEstimados` = 1,5. Fora do Play (ou objeto inativo) `MoverTampa` aplica a pose na hora. O caminho antigo `Interact()` (sem kit) continua clique a clique. `RecusaComATampa` (aviso dos pontos de uso).
- `Assets/Scripts/Interactable.cs` — `protected virtual bool AceitaComMaosOcupadas(kit) => false`; `Verificar` recusa com `kit.MaosOcupadas && !AceitaComMaosOcupadas(kit)`.
- `Assets/Scripts/Interactor.cs` — troca de ferramenta só com `!kit.MaosOcupadas`; o botão de largar devolve a tampa (o pneu segue igual); `RaycastTarget(maosOcupadas, focoPermitido, ...)`: com a tampa, o único foco alvo é a caixa dona dela (os outros são atravessados; pontos de uso como com o pneu); `Pegou` em `DarPasso` também põe `liberado = false` (é preciso soltar o botão antes de tampar). `AoBloquear` não mudou: pausa/cutscene deixam a tampa nos braços.
- `Assets/Scripts/DengueHUD.cs` — linha de ação "Pegar a tampa" (encostada, Mãos) e "Tampar a caixa" (nos braços); barra escondida com a tampa encostada (pegar é na hora); contagem 0/1 com a tampa nos braços; recusa `MaosOcupadas` com a tampa tem texto próprio.
- Leitores de `PneuCarregado`: `Lixeira` e `PontoDeReabastecimento` (recusa) → `MaosOcupadas`, com aviso da tampa; `FeedbackDeCarga.SacoEmMaos` e `Aplicar` (pose de carga, câmera e passos pesados) → `MaosOcupadas`; `ItensNaMao` (esconder o item) → `MaosOcupadas`. Continuam só com o pneu: `PontoDeReabastecimento.CacambaParaOPneu`, `Cacamba`, `FeedbackDeCarga.AvancarEsforco` (som de esforço), `KitHUD` (linha "carregando pneu" com a stamina) e `SomDoKit` (sons de borracha; a tampa não ganhou som de pegar/devolver).
- `Assets/Editor/Testes/TampaTestes.cs` (novo, categoria `Minigames`, e `.meta`); `NascimentoTestes.CaixaDagua_NasceSeca_E_AChuvaDevolve` agora pega a tampa com as Mãos (Pegou) em vez de dar um passo de areia.

**Configurações aplicadas:** nenhuma. Nenhuma cena ou prefab foi editado; campos novos com valor padrão no código.

**Testes:** `TampaTestes` (13): PegarPoeATampaNosBracos, TamparConclui, DevolverVoltaALateral, OutroFocoRecusaComATampa, OutraCaixaRecusaComATampa, PneuETampaNaoCoexistem, NaoTrocaDeFerramentaComATampa, FerramentaErradaNaoPega, ChuvaNaoReencheComATampaNosBracos, BloqueioMantemATampaNosBracos, TampaColocadaReligaOsColliders, TampaDevolvidaReligaOsColliders, CaminhoAntigoSemKitContinuaCliqueAClique. Suíte EditMode completa em batch mode: **515/515**.

**Observações técnicas:**
- "Soltar antes de tampar" depende da entrada do jogador (o `DarPasso` lê o botão): coberto por leitura, sem teste.
- Com a tampa nos braços a stamina limita a corrida, mas a `KitHUD` só mostra a barra de stamina na linha do pneu.
- Pendente no Editor: pegar, andar, devolver, pegar de novo e tampar; chuva e celular com a tampa nos braços; ajustar `posNosBracos`/`rotNosBracos` para a tampa não tapar a visão.

### 2026-11-11 — Minigames (leva 1): verificação final

**Objetivo:** Tarefa 7 do plano dos minigames (leva 1). Conferir a suíte inteira, estimar o efeito no balanceamento e registrar os valores calibrados e o que falta jogar no Editor. Nenhum código, cena, prefab ou número de balanceamento foi alterado.

**Arquivos alterados:** só este `contexto do jogo.md`.

**Testes:** suíte EditMode completa em batch mode (`unity test`, sem filtro): **515/515**, `failed="0"` (no começo do ramo eram 427). Nenhum `warning CS` nem `error CS` no log do Unity dessa rodada.

**Balanceamento (estimativa, sem rodar o menu):** o `Dengue > Simular Balanceamento` precisa do Editor aberto; a tentativa headless (`unity run -- -executeMethod BalanceamentoSimulador.Simular`) não imprimiu nada e foi abandonada. A conta abaixo vem da `SampleScene` e dos prefabs: contagem pelos `m_Script` (pneus, soltos na cena) e pelos `m_SourcePrefab` (bromélia, caixa d'água, ralo), com os `propertyPath` de `requiredClicks`/`segundosParaConcluir` das instâncias. Antes, o `SegundosEstimados` de todo foco era `segundosParaConcluir`.

| Foco | Na cena | Antes (s cada) | Agora (s cada) | Diferença (s) |
|---|---|---|---|---|
| Pneu | 10 | 1,5 | 6,0 | +45,0 |
| Bromélia (5 pocinhas) | 4 | 2,0 | 5,2 | +12,8 |
| Caixa d'água | 2 | 3,0 | 1,5 | −3,0 |
| Ralo (5 voltas × 1,5 s) | 4 | 2,0 em 3 deles, 3,0 em 1 (total 9,0) | 7,5 | +21,0 |
| **Total** | 20 | | | **+75,8 s** |

- O `segundosPorVolta` do ralo vale 1,5 no prefab (o padrão do código é 1,2); a cena não sobrescreve. O ralo com 3,0 s é a instância que sobrescreve `segundosParaConcluir`. Os outros 10 focos (vaso, garrafas, piscina) não mudam.
- Referência: a última saída do simulador no registro (calibração da nota, 30 focos, 10 sementes, custo do kit 10,6 s/foco): 30 s + 10,6 s → vence em 26:11 (1.571 s); 20 s + 10,6 s → 19:44. A diferença entra no custo do kit: +75,8 s / 30 focos = +2,5 s/foco (10,6 → cerca de 13,1).
- Mudança relativa: +75,8 s sobre 1.571 s = **4,8%** (cerca de 6%, ~+98 s, se os últimos focos, que levam até 1,6x, também escalarem). **Abaixo do limite de 15%**; nenhum número de balanceamento foi mexido. Estimativa: a saída real do simulador com o código novo não foi obtida.

**Valores calibrados como estão no código:**
- `OndaDoPneu` (construtor): `periodo` 0,9; `amortecimento` 0,12; `ganho` 15; `limiarDaBorda` 1; `taxa` 1,4; `grausPorSegundoCheio` 180; `nivelInicial` 1; `DtMaximo` 0,25 s.
- `PneuInteractable`: `periodoDaOnda` 0,9; `inclinacaoMaxima` 25°; `deslocamentoDaAgua` 0,05 m; `SegundosEstimados` 6 s.
- `BromeliaInteractable`: `segundosPorPoca` 0,5; `segundosDoCopoCentral` 1,2; `raioDeAcerto` 4°.
- `MiraDoBorrifador` (padrões do construtor): `cone` 30°; `raio` 4°.
- `CaixaDaguaInteractable`: `segundosParaTampar` 0,8; `SegundosEstimados` 1,5 s; `posNosBracos`/`rotNosBracos` zero.
- `RaloInteractable`: `segundosPorVolta` 1,2 no código (1,5 no prefab do ralo).

**Pendente para o usuário (jogar no Editor, mouse e controle):**
- Ralo: acerto; erro que gasta a tela; celular aberto no meio; mirar o ralo com o botão já segurado não inicia o anel.
- Pneu: balançar com mouse e com controle; conferir o tempo no ritmo (cerca de 5,4 s na simulação) e ajustar ganho/taxa se preciso; conferir o eixo do balanço e o lado para onde a água desloca; sair no meio e voltar (a água continua de onde parou); pegar e levar à caçamba como antes.
- Bromélia: toda pocinha alcançável dentro do cone; a névoa sai do bico só enquanto segura; sair no meio e voltar; a dose só é gasta ao concluir.
- Caixa d'água: pegar a tampa, andar, devolver com o botão de largar, pegar de novo, soltar o botão e segurar para tampar; ajustar `posNosBracos`/`rotNosBracos` se a tampa tapar a visão; a tampa não deve pulsar com o destaque enquanto está nos braços; chuva com a tampa nos braços.
- Geral: vaso, garrafas e piscina continuam no segurar; nota e recorde aparecem no fim; o `TimeManager.asset` foi commitado com `m_TimeScale: 0` (conferir se o jogo abre correndo).

### 2026-11-12 — Minigames (leva 1): correções da revisão final

**Objetivo:** corrigir os nove achados da revisão do ramo `minigames-leva-1` inteiro.

**Arquivos alterados:**
1. Bromélia, raio de acerto — `Assets/Scripts/MiraDoBorrifador.cs`: `RaioParaAsPocinhas(pocas, raioMaximo)` = `max(0,3°, min(raioMaximo, 0,45 × a menor distância entre duas pocinhas))`; `Assets/Scripts/BromeliaInteractable.cs`: `raioDeAcerto` 4 → **1,5**, e cada sessão usa o raio da geometria (um só por sessão). No prefab real (pocinhas a 0,055 m do centro, escala 1,51) o raio fica em ~0,48°: mirar o centro e segurar já não seca as cinco.
2. Bromélia, ângulos — `BromeliaInteractable.CriarMira(olho, yawInicial, pitchInicial, pitchMinimo = −90, pitchMaximo = 90)`: cada pocinha é o yaw `atan2(d.x, d.z)` e o pitch `atan2(−d.y, √(x²+z²))` de MUNDO de olho → pocinha, menos o olhar do começo (`Mathf.DeltaAngle`), como o Interactor aplica a mira (`DefinirOlhar(yawInicial + Mira.x, pitchInicial + Mira.y)`). A `Mira.y` fica presa a `[pitchMinimo − pitchInicial, pitchMaximo − pitchInicial]` dentro da sessão (parâmetros `miraYMinima`/`miraYMaxima` do `MiraDoBorrifador`); os limites vêm de `ControladorJogador.PitchMinimo`/`PitchMaximo` (getters novos). Sem `ControladorJogador`, o olhar do começo é o giro da câmera.
3. Pneu, apoio e respingo — `PneuInteractable.AoIniciarMinigame` ignora os Renderers com `EmissionHighlightUtil.Destacavel` falso (o `ParticleSystemRenderer` do enxame de mosquitos) e os da água; `Apoio`/`RaioDoPneu` internal para os testes.
4. Calibração no Inspector — `PneuInteractable`: `amortecimentoDaOnda`, `ganhoDaOnda`, `limiarDaBorda`, `taxaDeDerrame`, `grausPorSegundoCheio`, passados à `OndaDoPneu`. `BromeliaInteractable`: `coneDaMira`, `sensibilidadeDaMira` (multiplica o `Olhar` no `MiraDoBorrifador`, último parâmetro opcional `sensibilidade`; não finita ou <= 0 vale 1).
5. HUD da tampa — `Assets/Scripts/KitHUD.cs`: `TextoDaLinha(kit)` (internal) escolhe a linha de carga: a do pneu (sem mudança) ou, com `kit.TampaCarregada`, "Carregando a tampa. Tampe a caixa d'água. [Botão direito] devolve." (`TextoLinhaTampa`); a faixa da stamina aparece nos dois.
6. Pneu inclinado sem gancho — `PneuInteractable.OnDisable` (internal) chama o `AoSairDoMinigame` (pose original e água no nível dos passos dados); `AoIniciarMinigame` chama o `RestaurarPose` antes de guardar a pose (sessão anterior ainda marcada).
7. Ralo, clique rápido — `Assets/Scripts/Interactor.cs` (`AtualizarSegurar`): o aperto novo (`LerApertou()`) num foco com minigame confere (`ConferirFoco`) mesmo se o botão já foi solto no mesmo quadro; o segurar dos outros focos não mudou.
8. Caixa d'água sem kit — `CaixaDaguaInteractable.SegundosPorPassoCom(kit)`: sem kit, `segundosParaConcluir / requiredClicks` (como a base); com kit, como antes.
9. Este registro: linhas **Atualização:** nas entradas da Tarefa 1 e da Tarefa 3.
- Testes: `BromeliaBorrifarTestes`, `MiraDoBorrifadorTestes`, `PneuBalancarTestes`, `KitHUDTestes`, `TampaTestes` e `TesteUtil.Primitivo` (primitiva escondida, destruída no `LimparTudo`).

**Configurações aplicadas:** nenhuma cena, prefab ou asset editado (o prefab da bromélia só foi lido). Campos novos com o padrão no código: `PneuInteractable` — `amortecimentoDaOnda` 0,12; `ganhoDaOnda` 15; `limiarDaBorda` 1; `taxaDeDerrame` 1,4; `grausPorSegundoCheio` 180 (ao lado do `periodoDaOnda` 0,9). `BromeliaInteractable` — `raioDeAcerto` **1,5** (era 4); `coneDaMira` 30; `sensibilidadeDaMira` 1.

**Testes:** novos — `BromeliaBorrifarTestes`: PrefabReal_MirarOCentroESegurarSecaNoMaximoUma, PrefabReal_MirarCadaUmaSecaTodas, PitchInicial45_AMiraApontaParaAPocinha, MiraPresaAosLimitesDoPitch, SensibilidadeDaMiraChegaASessao; `MiraDoBorrifadorTestes`: RaioParaAsPocinhas_NaoAlcancaDuas, MiraYPresaAosLimites, SensibilidadeMultiplicaOOlhar_ValorRuimVale1; `PneuBalancarTestes`: ApoioIgnoraAsParticulasDoEnxame, TaxaDeDerrameChegaASessao, DesativadoNoMeioDevolveAPose, NovaSessaoComAAnteriorDePeVoltaAPoseAntes; `KitHUDTestes`: LinhaDeCarga_MaosLivresNaoTem, LinhaDeCarga_ComOPneu, LinhaDeCarga_ComATampa; `TampaTestes`: SemKitOPassoDuraODaBase. Classes tocadas 77/77; suíte EditMode completa em batch mode **531/531** (0 falhas).

**Observações técnicas:**
- O clique rápido do ralo (achado 7) depende da entrada do jogador (`LerApertou`/`LerSegurando`): coberto por leitura, sem teste.
- No EditMode o Unity não manda o `OnDisable` a este script (sem `ExecuteAlways`): o teste chama `pneu.OnDisable()` entre o `SetActive(false)` e o `SetActive(true)`.
- A caixa de um `ParticleSystem` ainda não simulado é (0, 0, 0) com tamanho zero; o teste do apoio simula as partículas e tira o pneu do eixo da origem para o enxame mexer no apoio sem a correção.

**Pendente para o usuário (jogar no Editor):**
- Bromélia: mirar o centro e segurar sem mover (deve secar no máximo uma pocinha); secar cada pocinha olhando de cima e quase em cima da planta.
- Pneu: jogar um pneu no estágio Mosquito (enxame visível) e conferir o pivô e o respingo; testar com FPS livre.
- Ralo: cliques muito rápidos e pausa no meio do anel.
- Tampa: pegar e andar sem olhar para a caixa (a linha da HUD explica?); correr até a stamina acabar; terminar a partida com ela.
- Interrupções: celular e fim de partida no meio do pneu e da bromélia.
- Rodar `Dengue > Simular Balanceamento`.

### 2026-11-12 — Metade dos focos nasce com água

**Objetivo:** fazer os minigames da leva 1 aparecerem numa partida normal. Com 10% dos focos molhados, só 1 dos 10 pneus e 1 das 4 bromélias ofereciam minigame (conferido em Play); o resto nascia seco e se comportava como antes.

**Arquivos alterados:** `Assets/Scripts/GerenciadorDeFocos.cs` (padrão do campo); `Assets/Scenes/SampleScene.unity` (valor do objeto na cena, salvo pelo Editor).

**Configurações aplicadas:** `fracaoNascendoComAgua` de 0,1 para 0,5, a pedido do usuário.

**Observações técnicas:**
- Mais focos molhados aumentam a pressão do hospital: o balanceamento não foi recalibrado. Rodar Dengue > Simular Balanceamento.
- O salvamento da cena pelo Editor levou junto cerca de 70 linhas de outras mudanças de serialização, não inspecionadas.
- Verificado em Play antes da mudança: o minigame do pneu roda de ponta a ponta (convite, sessão, 5 passos em 5,47 s com entrada simulada no ritmo, pose devolvida).


### 2026-11-13 — Design: pneu esvaziado nos braços e bromélia mais fácil

**Objetivo (pedido do usuário):** depois de jogar, mudar duas coisas.
- **Pneu:** pegar o pneu com água nos braços e tirar a água movendo a câmera; quanto mais água, mais lento o agente anda.
- **Bromélia:** ficou difícil de tratar.

**Arquivos novos:**
- `docs/superpowers/specs/2026-11-13-pneu-nos-bracos-design.md`, aprovado pelo usuário.
- `docs/superpowers/plans/2026-11-13-pneu-nos-bracos.md`, com 5 tarefas.

**Mudança no design depois da aprovação:** a água fica escondida com o pneu nos braços. O disco de água de cada pneu é plano e, com o pneu de pé, taparia o furo.

**Nenhum script, cena ou asset foi alterado nesta entrada.**

**Observações técnicas:**
- **Causa provável da bromélia:** o raio de acerto do borrifador caiu para cerca de 0,5° no prefab real, na revisão dos minigames. A proposta é cerca de 2,5°.
- **Mudança não minha no disco:** havia uma alteração em `LiberationSans SDF - Fallback.asset`, que deixei como estava.

### 2026-11-13 — Pneu: pegar com água e esvaziar nos braços (Task 1)

**Objetivo:** o pneu deixa de ter o minigame de balançar no chão. As Mãos o pegam cheio na hora e ele esvazia nos braços, com a mesma `OndaDoPneu`.

**Arquivos alterados:**
- `Assets/Scripts/PneuInteractable.cs`: sai o minigame no chão (`CriarMinigame` e `ConviteDoMinigame` devolvem null; somem `AoIniciarMinigame`, `AoAtualizarMinigame`, `AoSairDoMinigame`, `OnDisable`, `RestaurarPose`, `Apoio`, `RaioDoPneu`, os campos de apoio e eixo e `deslocamentoDaAgua`). Entram `NivelDeAgua`, `BalancarNosBracos(dt, olharX, frenteDoAgente)` e `DarPassoNosBracos()`.
- `Assets/Editor/Testes/PneuNosBracosTestes.cs` (novo, categoria `Minigames`, 12 testes) no lugar de `PneuBalancarTestes.cs` (apagado).
- Ajustes mínimos em testes que esvaziavam o pneu no chão: `TesteUtil.cs` (novo `EsvaziarPneu`), `PneuEsvaziarTestes.cs`, `PneuCarregarTestes.cs`, `DestinosTestes.cs`, `GerenciadorDeFocosTestes.cs`, `SonsNovosTestes.cs`. Em `NascimentoTestes.cs`, o teste de contagem passa a fixar a fração em 0,1 (estava quebrado desde o padrão 0,5).

**Comportamento:**
- `AvancarCom(kit)` com kit e pneu fora dos braços: pega na hora, cheio ou vazio (`Pegou`); `SegundosPorPassoCom(kit)` é 0.
- `PrenderEm` esconde a água (o disco deitado taparia o furo com o pneu de pé) e zera a onda; `Largar` devolve a água se o pneu não esvaziou; `EntregarEm` zera a onda.
- `BalancarNosBracos` só age com o pneu nos braços e cheio: cria a onda do nível dos passos já fechados, cada `Passo` vira `DarPassoNosBracos()` (o último esvazia, esconde a água e zera a pose). Visual: inclina em torno da frente do agente (até `inclinacaoMaxima`) e desloca 0,04 m para a direita do agente; derramando, estouro de partículas no lado baixo a cada 0,25 s, só no Play.
- `NivelDeAgua`: 0 esvaziado; com a onda ativa, o `Nivel` dela; senão, `1 − cliques/total`.

**Observações técnicas:**
- Nada chama `BalancarNosBracos` ainda: o `Interactor` repassa o olhar na Task 2. Até lá, o pneu pego não esvazia no jogo.
- `OnClicado` e o punch não disparam nos passos nos braços (o `DarPasso` da base é privado); os sons dos passos do pneu ficam para as tarefas seguintes, se forem desejados.
- O ritmo do teste usa 5° por quadro (a entrada satura em ±1); com 3°, a onda levaria cerca de 12 s para esvaziar 2 passos.
- Suíte EditMode inteira: 529 testes, todos passando.

### 2026-11-13 — Pneu: peso pela água, olhar repassado e caçamba só com pneu vazio (Task 2)

**Objetivo:** ligar o olhar do jogador ao balanço do pneu, dar peso a ele pela água e fazer a caçamba recusar pneu com água.

**Arquivos alterados:**
- `Assets/Scripts/KitDoAgente.cs`: novo `multiplicadorCheio` (0,5, serializado) e `MultiplicadorComPneu(nivel)` = `Lerp(multiplicadorCarregando, multiplicadorCheio, nivel)`. `Pegar` usa o nível do pneu; `AvancarStamina` reajusta o multiplicador a cada quadro com o pneu nos braços (esvaziando, anda mais leve; a tampa da caixa continua com 0,85). `LargarPneu` sobre a caçamba com o pneu não esvaziado devolve false e o pneu fica nos braços.
- `Assets/Scripts/Interactor.cs`: `BalancarPneuNosBracos(dt, olharX)` (nada com `Bloqueado`, sem kit ou sem pneu) chamado pelo `Update` depois do passo de largar, com `entrada.Olhar.x` e `transform.forward`. Largar cheio sobre a caçamba avisa `OnPontoRecusado(null, Cacamba.RecusaPneuComAgua)`.
- `Assets/Scripts/Cacamba.cs`: `RecusaPneuComAgua` e `PodeUsar` falso com pneu não esvaziado (vale também para a cabine da picape, que delega).
- Testes: 6 novos em `PneuNosBracosTestes.cs`; `DestinosTestes.cs`, `PontosDeUsoTestes.cs` e `PneuCarregarTestes.cs` esvaziam o pneu antes de entregar ou esperam 0,5 cheio.

**Observações técnicas:**
- O handler `HandlePontoRecusado` da `DengueHUD` só mostra o aviso e o `SomDoKit` só toca o erro: aceitam ponto null.
- Suíte EditMode inteira: 535 testes, todos passando.

### 2026-11-13 — Pneu: HUD da água nos braços e convite de pegar (Task 3)

**Objetivo:** a linha do kit mostra a água restante do pneu cheio nos braços, e mirar um pneu cheio no chão com as Mãos mostra o convite de pegar.

**Arquivos alterados:**
- `Assets/Scripts/KitHUD.cs`: `TextoLinhaPneuComAgua(pct)` (internal); `TextoDaLinha` devolve esse texto com o pneu nos braços não esvaziado (percentual = `RoundToInt(NivelDeAgua·100)`) e `TextoLinhaPneu` com ele esvaziado. O `Update` compara uma chave inteira (percentual, -1 esvaziado, -2 tampa, -3 sem linha) e só reescreve o texto quando ela muda (a água cai sem `OnMudou`, e o esvaziar também não dispara).
- `Assets/Scripts/DengueHUD.cs`: `AcaoPegarPneu` ("Clique para pegar o pneu"); `AcaoDoFoco` passa a `internal` e, num `PneuInteractable` não esvaziado com as Mãos, devolve `AcaoPegarPneu`.
- Testes: `KitHUDTestes.LinhaDeCarga_PneuComAgua` e `PneuNosBracosTestes.AcaoNoPneuCheioEhPegar`.

**Observações técnicas:**
- Sem alocação de string por quadro: o texto só é montado quando o percentual muda.

### 2026-11-13 — Bromélia: raio de acerto fixo de 2,5° (Task 4)

**Objetivo:** tornar a bromélia mais fácil de tratar. O raio calculado (0,45 × a menor distância entre pocinhas) dava cerca de 0,5° no prefab real e exigia precisão de pixel.

**Arquivos alterados:**
- `Assets/Scripts/BromeliaInteractable.cs`: `raioDeAcerto` passa a 2,5 (graus, cerca do tamanho de uma pocinha na tela) e `CriarMira` o usa direto; tooltip e cabeçalho atualizados.
- `Assets/Scripts/MiraDoBorrifador.cs`: removidos `RaioParaAsPocinhas` e suas constantes; cabeçalho descreve o raio fixo.
- Testes: `BromeliaBorrifarTestes.PrefabReal_MirarPertoDeCadaPocinhaSeca` (2° ao lado de cada pocinha a 1,5 m, segurar seca todas) substitui `PrefabReal_MirarOCentroESegurarSecaNoMaximoUma`; saiu `MiraDoBorrifadorTestes.RaioParaAsPocinhas_NaoAlcancaDuas`.

**Observações técnicas:**
- Continua secando uma pocinha por vez (a mais próxima dentro do raio); de longe um ponto de mira pode alcançar duas vizinhas, o que foi aceito.
- Nem o prefab `Bromelia.prefab` nem a `SampleScene` sobrescrevem `raioDeAcerto`.
- Suíte EditMode inteira: 536 testes, todos passando.

### 2026-11-13 — Pneu nos braços: verificação em Play e registro (Task 5)

**Objetivo:** conferir no Play o pneu pego cheio e esvaziado nos braços, o peso pela água, a caçamba só com pneu vazio e a bromélia com raio fixo (Tasks 1 a 4). Nenhum script, cena, prefab ou asset foi alterado.

**Arquivos alterados:** só este `contexto do jogo.md`.

**Como foi verificado:** `SampleScene` no Editor, em Play, com mouse e teclado virtuais do Input System. Abertura pulada, celular fechado e ocupação do hospital presa em 0,6, só no teste. O olhar no ritmo foi gerado em quadros reais, por eventos do mouse virtual: 300°/s × cos(2πt/0,9), cerca de ±43° de yaw. Passa pelo caminho real (`EntradaJogador` → `Interactor.Update` → `BalancarPneuNosBracos`). Capturas em `.superpowers/sdd/2026-11-13-pneu-nos-bracos/capturas/` (pasta fora do git).

| Situação | Esperado | Observado |
|---|---|---|
| Clique num pneu cheio com as Mãos | Nos braços, água escondida, multiplicador 0,5 | Convite "Clique para pegar o pneu". Um clique: nos braços, `agua do pneu` desligada, nível 1,000, multiplicador 0,500, 1,40 m/s andando. HUD "Pneu com água (100%): …" |
| Olhar no ritmo andando para a frente | Balança, respinga, nível cai, velocidade até 0,85 | Pneu (1), de cheio a vazio em 6,49 s, andando 12,7 m. Primeiro derrame em 1,51 s; passos em 3,00, 3,93, 4,78, 5,64 e 6,49 s. Inclinação até ±24,8°, deslocamento ±0,040 m. Multiplicador de 0,500 a 0,850, velocidade de 1,40 a 2,38 m/s. Vazio: pose zero, HUD "Carregando pneu…", aviso "Pneu vazio: leve à picape da UPA" |
| Olhar em volta sem ritmo por 5 s | Nível acima de 0,9 | Nível 1,000 nos três padrões: varredura de 180°/s com período de 3 s (\|Inclinação\| máxima 0,39), cinco piscadas de 90° em 0,25 s (0,55) e ruído de ±300°/s trocado a cada 0,15 s (0,63) |
| Caçamba com água | Aviso "Esvazie o pneu antes"; vazio, entrega | Com água, segurar o clique e o botão direito dão o aviso "Esvazie o pneu antes: balance a câmera." e o pneu fica nos braços. Vazio, entrega: "Pneu recolhido — 1 de 10", pneu na vaga, multiplicador 1 |
| Largar meio cheio e pegar de novo | Continua do nível | Nível 0,405 nos braços, com 2 passos fechados. No chão, 0,600 com a água visível; pego de novo, 0,600, com multiplicador 0,640. Continua do nível dos passos fechados, não do nível exato: a HUD vai de 41% para 60% (corrigido em 6b27c6b: guarda o nível exato). O pneu caiu no pé do agente (defeito abaixo) |
| Pausa com o pneu cheio | Nada esvazia | Menu de pausa aberto com 5 s de olhar no ritmo e W: nível 1,000, inclinação 0, 0 m andados. Pausado no meio do balanço (nível 0,880): nível e pose parados por 4,3 s, depois a onda some e a pose volta a zero |
| Bromélia a 1,5 m, mirando perto de cada pocinha | Seca uma por vez; começar mirando a planta é fácil | Convite na hora; a sessão começa no primeiro aperto. Mirando 1,5° ao lado de cada pocinha, seca uma por vez, a mais perto dentro do raio: 5/5 em 3,3 s. Com 2,5°, uma segurada secou antes a vizinha mais perto (0,97° contra 1,5°) e depois a mirada. A mira pega a planta numa área de ~48° × 21° |

**Testes:** suíte EditMode inteira, 536/536, sem falhas.

**Defeito encontrado (corrigido em 6b27c6b):**
- **O que acontece:** largar um pneu com água no estágio Mosquito (com o enxame filho) o põe no pé do agente.
- **Causa:** o `PneuInteractable.MedirNaDirecao` soma o `ParticleSystemRenderer` do `Enxame`. Com a escala do pneu (34,7), deu perto de −97 a −109 m e longe de +105 a +156 m. O raio do peito do `OndeCai` então acha um "muro" a até ~100 m e o avanço vira 0.
- **Efeitos vistos:** o pneu nasce dentro do `CharacterController` e empurra o agente 0,62 m. A 7 m da picape, o botão direito foi recusado com "Esvazie o pneu antes", porque a picape virou a caçamba de destino.
- **Comparação:** vazio, sem o enxame, que sai ao esvaziar, o pneu mede −0,25/+0,25 m e cai 0,73 m à frente. Antes deste ramo, só pneus vazios iam aos braços.

**Observações técnicas:**
- **Bromélia neste Editor:** em Play, o `raioDeAcerto` das quatro bromélias era 4, não 2,5. O `Bromelia.prefab` não grava o campo. O prefab carregado na memória do Editor aberto guarda o padrão antigo; um componente novo dá 2,5. Antes de jogar, reimporte o prefab ou reabra o Editor. Se o prefab for salvo nesta sessão, ele grava 4. O teste de 2,5° pôs o valor na instância, só em Play.
- **Sentido do balanço:** com `Inclinacao` > 0, o lado direito do pneu sobe, ele vai 4 cm para a direita e respinga à esquerda. Isso acontece quando o olhar está no extremo direito. Parece natural, como um pneu pendurado nas mãos; não está invertido. Os 4 cm quase não se veem perto dos 25° de inclinação.
- **`OnClicado`:** nenhum `OnFocoClicado` nos 5 passos nos braços, e um `OnFocoEsvaziado`. O único ouvinte de `OnClicado` é o som de cada passo (`SomDosFocos.Clicado`). Assim, o derrame fica mudo até o último passo, que toca o som de esvaziar. O `Interactor.Avancando` fica falso no balanço, então também não há som de ação do kit.
- **Ouvintes de `OnEsvaziado`:** o aviso da HUD saiu e o enxame foi removido. A rede social ignora o pneu esvaziado de propósito: ela posta na entrega.
- **Respingo:** é o estouro turquesa de sempre, espalhado em todas as direções no espaço de mundo. Perto da câmera, vira uma nuvem grande, às vezes na altura dos olhos.
- **Enxame nos braços:** ele acompanha o pneu, e os mosquitos ficam em volta da câmera.
- **Mira na caçamba com o pneu cheio:** ainda diz "Jogar o pneu na caçamba / segure o clique"; a recusa só sai ao clicar.
- **Bromélia:** BoxCollider de 1,03 × 0,52 × 1,05 m. Durante a sessão, o painel do alvo cobre a parte de baixo da planta.
- **Campos do pneu e do kit em Play:** iguais aos padrões do código (onda 0,9 / 0,12 / 15 / 1 / 1,4 / 180; inclinação 25°; multiplicadores 0,85 e 0,5).
- **Limpeza:** dispositivos e callbacks removidos, Input System de volta (`ResetAndDisableNonBackgroundDevices`, `PointersAndKeyboardsRespectGameViewFocus`), `runInBackground` falso (`ProjectSettings.asset` com 0), Play parado, cena não salva.

**Pendente para o usuário (jogar no Editor, mouse e controle):**
- Partida real com mouse físico e com controle: o ritmo do balanço pela mão, olhar em volta andando com o pneu cheio, correr com ele.
- Sons: o derrame nos braços (agora com o som de tirar a água, ver 2026-11-14) e o som de esvaziar.
- Celular aberto e chuva com o pneu nos braços.
- Bromélia com 2,5° num Editor reaberto ou na build.

### 2026-11-14 — Pneu nos braços: correções da verificação e da revisão final

**Objetivo:** corrigir os defeitos achados na verificação em Play e os apontados pela revisão final da branch `pneu-nos-bracos`.

**Correções pós-verificação (commits 6b27c6b e 5c7f7b5):**
- `PneuInteractable.MedirNaDirecao` ignora Renderers não destacáveis (o enxame do estágio Mosquito media centenas de metros e o `OndeCai` achava caçamba falsa); largar o pneu no meio do esvaziar guarda o nível exato da onda (`nivelGuardado`), usado por `NivelDeAgua` e pela próxima onda ao pegar de novo; chuva e esvaziar zeram esse nível.
- `PneuCarregarTestes`: o pneu de teste ganhou material (sem ele o Renderer não é medido).

**Correções da revisão final:**
- `Assets/Scripts/SomDosFocos.cs`: ouve `KitDoAgente.OnPneuPego` e `OnPneuLargado` (campo `kit`, vazio = procura na cena) e leva a fonte do zumbido, parada no mundo, para o centro atual do pneu. Antes, pegar cheio, andar e largar longe deixava o zumbido no lugar antigo.
- `Assets/Scripts/Interactable.cs`: novo `protected void NotificarClique()`, que dispara `OnClicado`.
- `Assets/Scripts/PneuInteractable.cs`: `DarPassoNosBracos` chama `NotificarClique()` em cada passo que não é o último, e o derrame ganha o som que já existe (`SomDosFocos.TocaPrimeiroClique` escolhe o "tira a água" no 1º passo). O último passo segue só com `OnEsvaziado`. `Interactor.Avancando` não foi ligado.
- `Assets/Scripts/KitHUD.cs`: o percentual de água usa `CeilToInt` (com folga de 1e-4 contra erro de ponto flutuante), então um fio de água mostra 1%, nunca 0%. O comentário "já trocou de ferramenta uma vez" voltou ao `dicaVista`, e `ChaveSemLinha` ficou junto das outras constantes.
- Testes novos: `SonsNovosTestes.Zumbido_PneuCheioLargadoEmOutroLugar_ZumbeNoLugarNovo`, `PneuEsvaziarTestes.EsvaziarNosBracos_DisparaOnClicadoEmCadaPassoMenosOUltimo`, `KitHUDTestes.LinhaDeCarga_FiozinhoDeAgua_MostraUmPorCento`.
- Este `contexto do jogo.md`: defeitos da verificação marcados como corrigidos e duplicata removida de "Pendente para o usuário".

**Observações técnicas:**
- Suíte EditMode inteira: 541 testes, todos passando.
- Play não repetido depois das correções.
- Reabrir o Editor antes de jogar ou fazer build: o prefab da bromélia não grava `raioDeAcerto`, e o Editor aberto guarda 4 na memória.


### 2026-11-14 — Pneu: esvaziar mexendo a câmera, sem ritmo

**Objetivo (pedido do usuário):** esvaziar o pneu nos braços ficou difícil, porque o jogo pedia para balançar no ritmo da água. Agora basta mexer um pouco a câmera para a água sair.

**Arquivos alterados:**
- `Assets/Scripts/PneuInteractable.cs`:
  - `BalancarNosBracos(dt, olhar, frente)` não usa mais a `OndaDoPneu`;
  - qualquer movimento de câmera derrama: o nível cai `|olhar| / grausParaEsvaziar`;
  - cada limite de passo cruzado é um clique, com o som do derrame, e o último esvazia;
  - a inclinação do pneu segue a velocidade da câmera, suavizada;
  - o nível exato fica em `nivelGuardado`;
  - saíram os campos da onda (`periodoDaOnda`, `amortecimentoDaOnda`, `ganhoDaOnda`, `limiarDaBorda`, `taxaDeDerrame`);
  - `SegundosEstimados` foi de 6 s para 1 s.
- `Assets/Scripts/Interactor.cs`: `BalancarPneuNosBracos(dt, Vector2 olhar)` repassa o tamanho do movimento em qualquer direção, com o sinal do horizontal para o lado da inclinação.
- Removidos `Assets/Scripts/OndaDoPneu.cs` e `Assets/Editor/Testes/OndaDoPneuTestes.cs`, que ficaram sem uso.
- `Assets/Editor/Testes/PneuNosBracosTestes.cs`:
  - testes novos: `MexerACameraEsvaziaRapido`, `CameraParadaNaoDerrama`, `PoucoMovimentoSemRitmoJaDerrama` e `InteractorContaOOlharVertical`;
  - ajustados: `PassosSobemComONivel` e `LargarNoMeioGuardaONivelExato`.

**Configurações aplicadas:** `grausParaEsvaziar` = 150 (graus somados de câmera para esvaziar o pneu cheio); `grausPorSegundoCheio` 180 e `inclinacaoMaxima` 25, só para o visual.

**Testes:** suíte EditMode inteira, 532/532.

**Observações técnicas:**
- Olhar em volta andando também derrama. Isso é de propósito: o usuário pediu que pouco movimento já tirasse a água.
- O `BalanceamentoSimulador` passa a contar 1 s por pneu.
- **Não verificado em Play:** a sensação com o mouse de verdade. Se esvaziar rápido ou devagar demais, ajuste `grausParaEsvaziar` no Inspector do pneu.

### 2026-11-14 — Design: bromélia com pontos de borrifar (bolinhas)

**Objetivo (pedido do usuário):** reformular por completo o minigame da bromélia, no estilo do jogo osu!: clicar em bolinhas para concluir. O usuário não gostou de nada do minigame de mirar o borrifador e segurar.

**Decisões do usuário:**
- A câmera vai para perto, por cima da planta.
- O borrifador continua na tela, e as bolinhas são os pontos onde tem que borrifar.

**Arquivos novos:**
- `docs/superpowers/specs/2026-11-14-bromelia-pontos-de-borrifar-design.md`, aprovado pelo usuário em 2026-11-15.

**Nenhum script, cena, prefab ou asset foi alterado nesta entrada.**

**Observações técnicas:**
- **Preview:** feito no Unity em Play, com nada salvo. Capturas em `.superpowers/sdd/2026-11-14-bromelia-pontos-de-borrifar/capturas/` (pasta fora do git), junto do `preview_aprovado.cs` que gerou a imagem aprovada.
- **Pocinhas vistas de cima:** as folhas tapam as 4 de fora; só o copo aparece. O design prevê subi-las (no preview, 4,5 cm no mundo e 1,5× maiores).
- **Play com o Editor sem foco:** o jogo fica parado no quadro 1 ou 2 (`runInBackground` falso). A captura funciona mesmo assim, com a câmera e os objetos postos direto por `eval_file` e `Canvas.ForceUpdateCanvases()` no fim.
- **Texto no Canvas de preview:** com `planeDistance` de 0,06 (interface na frente das folhas), o TextMeshPro não aparece na captura; o `UnityEngine.UI.Text` aparece.
- **Mudança não minha no disco:** `LiberationSans SDF - Fallback.asset` continua modificado, como já estava.
- **Revisão do documento:** três revisores leram o design antes da entrega (um como quem vai construir, um conferindo os fatos com o código, um procurando lacunas). Entraram por causa deles: o fecho de 0,45 s depois do último acerto (o passo que conclui encerra a sessão no mesmo quadro, e o último acerto sumiria); o agente parado e sem mira até a câmera voltar aos olhos; o enxame de mosquitos escondido na vista de perto (a câmera ficaria dentro dele); o borrifador de perto sem sombra e o da mão quieto; a dica e a contagem acima da barra do kit; e a mira calculada com a sensibilidade da cena (0,2° por pixel, não o padrão 0,15 do código).


### 2026-11-15 — Plano: bromélia com pontos de borrifar (bolinhas)

**Objetivo:** plano de implementação do design aprovado pelo usuário em 2026-11-15 (a bromélia no estilo osu!, com vista de perto e borrifador na tela).

**Arquivos novos:**
- `docs/superpowers/plans/2026-11-14-bromelia-pontos-de-borrifar.md`, com 8 tarefas: a sessão `PontosDeBorrifar`; o enquadramento temporário do `CameraJogador`; o `Interactor` conduzindo a vista de perto; a bromélia criando a sessão nova e a saída do minigame de mirar; as pocinhas à vista; a interface das bolinhas; o borrifador de perto; a verificação em Play e o registro.

**Nenhum script, cena, prefab ou asset foi alterado nesta entrada.**

**Decisões do plano que o design deixava para quem constrói:**
- **Branch:** `bromelia-pontos-de-borrifar`, a partir de `pneu-nos-bracos` (`99bcbad`).
- **Pocinhas:** sobem e aumentam pelo `FocosBlenderBuilder` (valores absolutos no espaço da raiz do prefab, aplicados por um menu novo e também no `Criar Bromélia`), sem passar pelo Blender e sem refazer o prefab.
- **Mira sem o "inverter Y":** a `EntradaDoMinigame` ganha o campo `Ponteiro`, alimentado por `EntradaJogador.OlharSemInverter`.
- **Quem segura o agente na volta da câmera:** o `Interactor` (`Voltando`), liberado quando o `CameraJogador` sai do enquadramento. A ida e a volta da câmera andam em tempo sem escala, para valer na pausa.
- **Pose e pocinha de cada ponto:** viajam na própria sessão (`SessaoDaBromelia`, que deriva de `PontosDeBorrifar`), porque `CriarMinigame` é fábrica pura.
- **Regra a mais, que o design não tinha:** se a sessão for interrompida no meio segundo do fecho (botão direito, celular ou pausa logo depois do último acerto), o copo volta a ter água. Sem isso a planta ficaria seca e sem concluir, e sem minigame para terminar.

**Observações técnicas:**
- **O que há acima das bromélias:** o Editor estava desconectado ao escrever o plano, então a conferência (um `SphereCastAll` acima de cada uma das 4) virou o primeiro passo da tarefa da bromélia. Se alguma estiver tapada, o plano manda parar e avisar o usuário.
- **Mudança não minha no disco:** `LiberationSans SDF - Fallback.asset` continua modificado, como já estava.
- **Revisão do plano:** três revisores leram o plano (fatos do código, números dos testes, lacunas contra o design). Entraram por causa deles: o enxame continua escondido até a câmera voltar aos olhos, e não só até o fim da sessão (gancho novo `Interactable.AoVoltarDaVistaDePerto`); o enquadramento é solto mesmo com a câmera desligada; `Proxima` vale −1 na espera inicial e no fecho (a bolinha 1 apareceria antes de a câmera chegar); `FracaoDoPasso` vale 1 quando o passo seguinte já é devido (daria 0/0 com mais passos que pocinhas); a interface guarda o último estado para o fade de saída; e o aviso de que o `Bromelia.prefab`, ao ser salvo pelo menu das pocinhas, passa a gravar os valores padrão dos campos novos.

### 2026-11-15 — Bromélia com pontos de borrifar: implementação, verificação em Play e registro

**Objetivo (pedido do usuário):** o minigame da bromélia no estilo osu!, conforme o design aprovado em 2026-11-15 (`docs/superpowers/specs/2026-11-14-bromelia-pontos-de-borrifar-design.md`) e o plano (`docs/superpowers/plans/2026-11-14-bromelia-pontos-de-borrifar.md`): com a água sanitária, um clique numa bromélia com água leva a câmera para cima da planta; cada pocinha com água vira uma bolinha numerada (as de fora em ordem sorteada, o copo por último), e clicar nas bolinhas com a mira desenhada pelo jogo borrifa e seca a pocinha. O borrifador aparece na tela, preso à câmera. Errar só custa tempo. Branch `bromelia-pontos-de-borrifar` (a partir de `pneu-nos-bracos`, `99bcbad`).

**Commits (`git log --oneline 99bcbad..HEAD`, sem o deste registro):**
- `65c9c8c` Spec e plano: bromélia com pontos de borrifar
- `912068b` Bromélia: sessão de pontos de borrifar (lógica pura)
- `f3b64fe` Câmera: enquadramento temporário para a vista de perto
- `c927396` Interactor: vista de perto pedida pela sessão, agente parado até a câmera voltar
- `79fec95` Bromélia: sessão de pontos de borrifar na vista de perto; sai o minigame de mirar
- `8388fea` Bromélia: pocinhas de fora mais altas e maiores, à vista de cima
- `bf40de8` Bromélia: interface das bolinhas, mira, contagem e dica
- `3f65a97` Bromélia: borrifador de perto preso à câmera; o da mão fica quieto
- `c1b3ef6` Bromélia: borrifador de perto entra já apontado para a mira, sem virar (correção achada nesta verificação)

**Arquivos novos (cada um com o `.meta`):**
- `Assets/Scripts/PontosDeBorrifar.cs` — a sessão, em lógica pura (campo de −1 a +1 na altura, mira, bolinha da vez, anel, pausa, espera inicial, fecho, passos).
- `Assets/Scripts/PontosDeBorrifarHUD.cs` — a interface (véu, dica, contagem, próxima, bolinha da vez com anel, estouro, mira), Canvas próprio em código, sortingOrder 45.
- `Assets/Scripts/BorrifadorDePerto.cs` — a cópia do borrifador da mão presa à câmera, sem sombra, com a névoa herdada como borrifada.
- `Assets/Editor/Testes/PontosDeBorrifarTestes.cs`, `PontosDeBorrifarHUDTestes.cs`, `BorrifadorDePertoTestes.cs` e `CameraJogadorTestes.cs`.
- `docs/superpowers/specs/2026-11-14-bromelia-pontos-de-borrifar-design.md` e `docs/superpowers/plans/2026-11-14-bromelia-pontos-de-borrifar.md`.

**Arquivos alterados:**
- `Assets/Scripts/BromeliaInteractable.cs` — cria a `SessaoDaBromelia` (pose da vista de perto, pontos projetados, copo por último), seca a pocinha de cada acerto, esconde o enxame até a câmera voltar, garante água no copo se a sessão cair no fecho; dica nova.
- `Assets/Scripts/CameraJogador.cs` — enquadramento temporário (`Enquadrar`, `SoltarEnquadramento`, `Enquadrada`), em tempo sem escala.
- `Assets/Scripts/Interactor.cs` — pede e solta o enquadramento, segura o agente e a mira até a câmera voltar (`Voltando`, `LargarSeVoltando`, `AtualizarVolta`), passa o `Ponteiro`; adiciona a `PontosDeBorrifarHUD` e o `BorrifadorDePerto` no `Start`.
- `Assets/Scripts/Minigame.cs` — `EntradaDoMinigame.Ponteiro`, `PoseDePerto` (com `NoCampo`), `IMinigameDePerto`; sai a `IMinigameComMira`.
- `Assets/Scripts/EntradaJogador.cs` — `OlharSemInverter`.
- `Assets/Scripts/DengueHUD.cs` — esconde a mira normal e o painel do alvo na vista de perto e na volta.
- `Assets/Scripts/ItensNaMao.cs` e `RegraDoItemNaMao.cs` — a mão fica quieta (sem gesto nem névoa) na vista de perto.
- `Assets/Scripts/Interactable.cs` (gancho `AoVoltarDaVistaDePerto`), `EnxameDeMosquitos.cs` (`NomeDoFilho` interno), `ControladorJogador.cs`, `SkillCheckHUD.cs`.
- `Assets/Editor/FocosBlenderBuilder.cs` — menu **Dengue > Ajustar Pocinhas da Bromélia**, também aplicado no **Criar Bromélia**; `Ferramentas/Blender/gerar_focos.py` (comentário).
- `Assets/Meus/Assets Gerados/Focos/Bromelia.prefab` — as 4 pocinhas de fora mais altas e maiores.
- Testes: `BromeliaBorrifarTestes.cs` (reescrito), `MinigameNoInteractorTestes.cs`, `ItensNaMaoTestes.cs`, `RegraDoItemNaMaoTestes.cs`.

**Arquivos apagados (com o `.meta`):** `Assets/Scripts/MiraDoBorrifador.cs` e `Assets/Editor/Testes/MiraDoBorrifadorTestes.cs`.

**Configurações aplicadas (padrões no código; nenhuma cena muda):**
- Câmera de perto: 0,58 m acima do copo, recuo de 0,17 m para o lado do agente, FOV fixo de 40°; ida em 0,3 s e volta em 0,25 s, em tempo sem escala.
- Sessão: raio da bolinha (e do acerto) 0,14 do campo; anel de 1,5 s; pausa da bolinha de 0,35 s; espera inicial de 0,3 s; fecho de 0,45 s; mira de 0,0093 do campo por grau; `Agindo` por 0,2 s depois de cada aperto (`PontosDeBorrifar.SegundosAgindo`, sem mudança).
- Pocinhas de fora: altura 0,22 no modelo (0,19 + 0,03) e escala 1,5 (`FocosBlenderBuilder.AlturaDasPocinhas` e `EscalaDasPocinhas`).
- Borrifador de perto: na câmera em (0,172; −0,078; 0,36), bico em (0; 0,034; 0,088), 30° de mergulho, 24 gotas por borrifada (0,02 de tamanho, 0,8 m/s), entrada de 0,2 s.
- Interface: dica com o centro a 172 px da base (faixa de 880 × 44), contagem a 212 px, em 1920 × 1080.
- Textos: convite "Clique para borrifar as pocinhas" (igual); dica "Clique nas bolinhas para borrifar   |   Botão direito: sair".

**Testes:** suíte EditMode inteira, **576/576** (categoria `Minigames`: 137/137). Em `99bcbad` eram 532.

**Verificado em Play (SampleScene no Editor, nada salvo na cena):**

Como: `Application.runInBackground = true` só no teste; abertura pulada (corrotinas das cartelas paradas, `Tocando` das cartelas falso, jogador destravado); partida não iniciada (a ocupação parada); bromélias secas reenchidas e o agente teleportado ao lado de cada uma. Entrada por **mouse virtual** do Input System (`QueueStateEvent`, movimento e botões) e, numa parte, por **controle virtual**. Para a entrada chegar ao jogo com o Editor sem foco, o `InputSystem.settings` em memória (não há asset de settings no projeto) ficou com `backgroundBehavior = IgnoreFocus` e `editorInputBehaviorInPlayMode = AllDeviceInputAlwaysGoesToGameView` só durante o teste, e voltou ao padrão no fim. Um registro quadro a quadro (`Application.onBeforeRender`) gravou o estado da sessão, da câmera, da HUD, do borrifador, das pocinhas, do enxame, das doses e o `OnFocoClicado` (o som de passo). A interface em overlay não aparece na captura: foi conferida pelo estado dos objetos (posição, tamanho, `activeSelf`, alfa, textos). Capturas em `.superpowers/sdd/2026-11-14-bromelia-pontos-de-borrifar/capturas/play_*.png` (pasta fora do git).

- **Começo:** um clique real (mouse virtual → `EntradaJogador` → `Interactor`) começa a sessão; o agente fica `Imobilizado`; a câmera chega à pose em cerca de 0,27 s; o borrifador entra pelo canto de baixo, à direita, em 0,2 s; a mira nasce no centro; a mira normal e o painel do alvo somem (a mira normal ainda aparece no 1º quadro); a 1ª bolinha só aparece com a câmera parada (0,3 s).
- **As 4 bromélias, inclusive a alta (y = 1,17):** nada tapa a vista; as 5 pocinhas aparecem; a projeção de cada ponto bate com a pocinha na captura.
- **Acertos:** cada acerto seca a pocinha dele (animação de 0,3 s), acende o estouro e o ponto da contagem, e dispara o `OnFocoClicado` no mesmo quadro; a borrifada sai do bico na direção da mira (capturas congeladas logo depois do aperto); o som de ação (loop do `SomDoKit`) sobe a cada aperto e cai em seguida.
- **Último acerto:** o copo termina de secar 0,30 s depois, o estouro dura 0,3 s e o foco conclui 0,45 s depois do acerto; a dose (2 → 1) só sai na conclusão; só então a câmera volta.
- **Volta da câmera:** 0,24 s; durante ela o agente fica parado, sem alvo e sem mira normal; um clique esquerdo dado na volta não começa sessão nem acende a planta; a mira normal volta no quadro seguinte ao da chegada.
- **Anel que fecha:** a bolinha some por 0,35 s e volta no mesmo lugar, com o anel cheio (ciclo medido de 1,50 s); nada se perde.
- **Clique fora e na bolinha apagada:** só a borrifada (a contagem de borrifadas sobe, a de acertos não).
- **Botão direito no meio:** sai guardando as secas; a sessão nova traz só as que faltam (5 → 3 → 2 → 1 pontos). Sair nos 0,3 s em que uma pocinha seca: ela termina seca, com a escala original (1,5).
- **Botão direito, celular e pausa no fecho (0,1 a 0,15 s depois do último acerto):** a planta não conclui, o copo volta com água e com a escala original (1,0), e a dose não sai. Na pausa (`timeScale` 0) a câmera voltou aos olhos com o jogo parado; ao fechar o menu o agente anda.
- **Celular no meio; fim de jogo no meio** (derrota disparada pelo `FluxoDeJogo`): sessão encerrada, sem gastar dose, câmera nos olhos; com o celular fechado o agente anda (no fim de jogo ele fica parado pela tela final, como sempre).
- **"Inverter Y" ligado:** mouse para cima → mira para cima.
- **FOV do menu em 60 (mínimo) e 100 (máximo):** a mesma pose (posição, giro e FOV 40) e os mesmos pontos; ao sair, a câmera volta ao FOV do menu.
- **Estágio de mosquito** (enxame posto pelo `EnxameDeMosquitos`): escondido do começo da sessão até a câmera chegar aos olhos, e de volta depois.
- **Agente colado na planta** (Bromélia (1), agente encostado no collider dela): o corpo não entra na vista de perto.
- **HUD:** em tela, a dica e a contagem ficam entre a barra do kit e a bolinha mais baixa, sem tocar nelas; a barra do kit não estava à mostra no teste (a partida não tinha começado), mas a geometria dela foi conferida. O borrifador não fica atrás do ícone do celular nem da lista de focos (vértices projetados em 2,2:1 e 16:9).
- **Chuva durante a sessão:** a sessão segue.
- **Bromélia desativada no meio e no fecho:** nenhum erro no console ("already being activated or deactivated" não aparece); ao reativar, o enxame e o copo estão como deveriam.
- **Primeiro quadro da interface:** o Canvas já tem a altura e a escala certas no 1º quadro.
- **Controle virtual:** o botão Oeste começa a sessão e acerta, o analógico direito anda a mira (direita e para cima), o Norte sai.
- **Console:** sem erros durante os testes.

**Correção feita nesta verificação:** na primeira sessão, a cópia do borrifador nascia com o giro da mão e virava cerca de 50° em 0,1 s, no canto da tela (e, depois de sair com a mira longe do centro, a sessão seguinte também virava). Agora a cópia é ativada já apontada para a mira (`BorrifadorDePerto.Ativar`, em meios passos, que convergem), com o teste `AoAtivarJaApontaParaAMira`. Visto em Play: o giro é o final desde o 1º quadro.

**O que NÃO foi verificado:**
- **Partida real com mouse e com controle de verdade, pelo usuário.** A sensação de mirar com o mouse e com o analógico, o tamanho das bolinhas e o ritmo de 5 s por planta.
- **Som:** não dá para ouvir pelo CLI. Ficou provado que o loop de ação sobe a cada aperto (fade de 0,15 s, `Agindo` de 0,2 s) e que o `OnFocoClicado` sai a cada passo. Se a borrifada quase não se ouvir, subir `PontosDeBorrifar.SegundosAgindo`.
- **A interface vista de verdade** (bolinhas, anel, estouro, mira, dica): só pelo estado dos objetos; a captura do CLI não mostra o overlay.
- **Movimento visto em tempo real:** a ida e a volta da câmera e a entrada do borrifador foram medidas quadro a quadro, não vistas.
- **Acerto com o analógico:** o analógico andou a mira e o Oeste acertou, mas a mira foi posta na bolinha pelo mouse virtual.
- **Pausa no meio da sessão:** a pausa só foi testada no fecho (logo depois do último acerto). No meio, o que foi testado é o celular e o fim de jogo, que passam pelo mesmo `AoBloquear`.
- **Itens menores que a revisão final da branch deixou ficar:** o agente ficaria parado para sempre se a `CameraJogador` nunca saísse do enquadramento (só acontece sem `ControladorJogador` na cena); um enxame que nascer nos 0,25 s da volta da câmera aparece na frente dela; a mira normal pode aparecer no primeiro quadro da sessão; o véu de 12% da interface nova escurece de leve também a HUD de baixo.
- **Barra do kit à mostra** junto com a sessão, e a interface em 1920 × 1080 (a Game view do Editor tinha 1097 × 495).
- **Build exportada.**

**Observações técnicas:**
- **Brilho de mira, de perto:** o brilho pulsante amarelo deixa a água das pocinhas amarelo-clara em vez de azul (capturas `play_bromelia2_cheia_com_brilho_de_mira.png` e `play_bromelia2_cheia_sem_brilho_comparacao.png`). As bolinhas são desenhadas por cima, então a leitura delas não é tapada; o brilho ficou ligado, como o design pede (item 23). Fica para o usuário decidir.
- **Proporção 4:3:** o borrifador de perto encosta no canto do ícone do celular (68 vértices na região dele). Em 16:9 e mais largo, não.
- **Input System com o Editor sem foco:** além do `runInBackground`, o padrão `PointersAndKeyboardsRespectGameViewFocus` descarta o mouse e o teclado virtuais. Mudar o `InputSystem.settings` em memória e recriar o mouse virtual resolveu; nada foi gravado em disco.
- **Abertura pulada parando as corrotinas:** o `CartelasNarrativas.Tocando` fica true e o menu de pausa se fecha sozinho no quadro seguinte. No teste, `Tocando` foi posto em false.
- **Congelar para a captura:** `Time.timeScale = 0` para as gotas no ar, mas a câmera, a HUD e a entrada do borrifador andam em tempo sem escala.
- **Mudança não minha no disco:** `LiberationSans SDF - Fallback.asset` continua modificado, como já estava. `PlayerSettings.runInBackground` voltou a false e nenhum arquivo de `ProjectSettings` mudou.

### 2026-11-17 — Meta "Sem retrabalho" trocada por "Sem erros"

**Objetivo (pedido do usuário):** a estrela "Sem retrabalho" (nenhum foco reenchido pela chuva) era quase impossível, porque não dá para limpar todos os focos antes da primeira chuva. Ela passa a valer para quem não erra nenhum minigame de limpar foco (por exemplo, a tela do ralo que rasga).

**Arquivos alterados:**
- `Assets/Scripts/AvaliacaoDaPartida.cs`: `MetaDaPartida.SemRetrabalho` virou `SemErros` (nome na tela: "Sem erros"); `DadosDaPartida` ganhou `Erros`; a meta vale com `Erros == 0`; os detalhes viraram "0 erros", "1 erro" e "perdida: N erros".
- `Assets/Scripts/PlacarDaPartida.cs`: novo campo `interactor` (vazio = o `Interactor` da cena), assinatura do `Interactor.OnErrouSkillCheck` → `HandleErrouMinigame`, que conta `Erros` e derruba a meta, só durante a partida. O reenchido continua sendo contado em `Reenchidos`, mas agora só para o desconto.
- `Assets/Editor/Testes/NotaTestes.cs`, `PlacarTestes.cs` e `InterfaceDaNotaTestes.cs`: adaptados à meta nova. Testes novos: `SemErros_ReenchidoNaoDerruba` e `Reencher_SoConta_ErroDerruba`; `Reenchido_ForaDaPartida_NaoConta` agora confere também o erro fora da partida.
- `Assets/Scenes/SampleScene.unity`: os 3 textos fixos "Sem retrabalho" (tela final, cartão da pausa, aviso) viraram "Sem erros". Em jogo eles já são escritos por `AvaliacaoDaPartida.Nome`.

**O que não mudou:** o desconto de 100 pontos por reenchido continua na nota (linha "Reenchidos N × 100"). O enum mantém a mesma posição (1), então nada serializado muda de valor.

**Verificação:** `unity test` EditMode com filtro `NotaTestes|PlacarTestes|InterfaceDaNotaTestes`: 60 de 60 passaram. **Não verificado:** a suíte inteira, Play e build; o erro real do ralo derrubando a meta em Play (o evento `OnErrouSkillCheck` é o mesmo que já mostra o toast "A tela rasgou!").

**Observações técnicas:** hoje só o ralo (skill check) devolve `EstadoDoMinigame.Errou`; qualquer minigame novo que devolver `Errou` passa pelo mesmo evento e conta como erro sem mudar o placar.


### 2026-11-17 — Design: fotos dos focos na rede social

**Objetivo (pedido do usuário):** o jogador novo não sabe onde estão os focos. Com o tempo, alguém do bairro posta na rede social a foto de um foco, e pela foto o jogador reconhece o lugar.

**Decisões do usuário:**
- A foto sai **quando o jogador empaca** (o mesmo relógio da bronca, `tempoSemFoco` = 60 s), no lugar da bronca.
- **Fotos prontas**, geradas no Editor, **cada uma ligada a um foco específico** (pelo próprio objeto, não pelo nome).
- Ver e aprovar as fotos antes de irem para o jogo. Feito em duas rodadas: 13 das 31 fotos foram refeitas (3 opções cada) e ele escolheu uma por foco.
- Visual do celular aprovado: foto no card com círculo vermelho em volta do foco, selo **RESOLVIDO** quando o foco é eliminado, notificação com miniatura e "enviou uma foto".
- Propostas aceitas: o foco mais perto do jogador que ainda não saiu em foto; focos colados num que já saiu ficam de fora; quem posta é o dono do tipo; texto sem endereço.

**Arquivos novos:**
- `docs/superpowers/specs/2026-11-17-fotos-dos-focos-design.md`, com as 31 poses aprovadas no apêndice.

**Nenhum script, cena, prefab ou asset foi alterado nesta entrada.**

**Observações técnicas:**
- **Preview no Editor, sem salvar nada:** as fotos foram renderizadas em modo Edição com uma câmera `HideAndDontSave` (cópia da Main Camera, com o pós-processamento do URP) e `Camera.Render()` num `RenderTexture` 720×480; a cena ficou sem mudanças (`isDirty` falso antes e depois). O preview do celular foi feito em Play, com os canvases trocados para Screen Space Camera e o estado final das animações forçado por reflexão (o Play ficou parado no quadro 2, com o Editor sem foco).
- **Escolha automática do ponto da câmera:** o chão do ponto precisa ser o primeiro acerto descendo de perto do foco. O "acerto mais baixo" caía no piso da sala do hospital, que fica embaixo da praça. As caixas d'água em telhado só aparecem de 4,5 a 7 m de altura ("da janela do vizinho"). Colisores não enxergam folhagem: a segunda versão renderiza com e sem o foco (`Renderer.forceRenderingOff`, que não é serializado) e mede na imagem quanto dele aparece e a luz em volta.
- **Determinismo:** rodar o mesmo script duas vezes deu PNGs idênticos byte a byte.
- **Nomes repetidos na cena:** há dois `pneu (5)` (um solto no quiosque da praça, um no monte) e garrafas com espaços no nome (`Garrafa `, `Garrafa  (1)`).
- **Material fora do git:** `.superpowers/sdd/2026-11-17-fotos-dos-focos/` (fotos aprovadas, `poses.tsv`, folhas de contato, capturas do celular e os scripts de `eval_file`).
- **Mudanças de outra sessão no disco:** a entrada "Meta 'Sem retrabalho' trocada por 'Sem erros'" logo acima, com `AvaliacaoDaPartida.cs`, `PlacarDaPartida.cs`, três testes e a `SampleScene`, está sem commit. Não foi tocada. `LiberationSans SDF - Fallback.asset` continua modificado, como já estava.
- **Revisão do documento:** um revisor leu o design como quem vai construir. Entraram por causa dele: "mexido" passa a contar também o esvaziar com as Mãos (`OnFocoEsvaziado`, o atalho não dispara `OnClicado`) e pegar a tampa da caixa d'água (`KitDoAgente.OnTampaPega`, que não dá passo); as fotos do jogo são **cópias** das aprovadas, porque a semente do círculo vermelho do preview vinha da ordem do `FindObjectsByType` e não se reproduz; a posição de cada foco para "lugar conhecido" é a guardada no início (o pneu muda de lugar ao ser carregado).

### 2026-11-17 — Plano: fotos dos focos na rede social

**Objetivo:** plano de implementação do design aprovado pelo usuário em 2026-11-17 (`docs/superpowers/specs/2026-11-17-fotos-dos-focos-design.md`).

**Commits antes do plano (a pedido do usuário):**
- `6926759` na `bromelia-pontos-de-borrifar`: o trabalho que a sessão "Conquista sem tela quebrada" deixou sem commit (meta "Sem erros"), só com os arquivos dela. Para separar o `contexto do jogo.md`, a entrada do design desta feature saiu do arquivo durante o commit e voltou depois.
- Branch nova `fotos-dos-focos` a partir dali; `378022e`: o spec e a entrada do design.

**Arquivos novos:**
- `docs/superpowers/plans/2026-11-17-fotos-dos-focos.md`, com 7 tarefas: a escolha do foco (lógica pura); o narrador postando a foto; a foto e o carimbo no card; a miniatura na notificação; o builder das fotos; as 31 fotos aprovadas ligadas na cena; a verificação em Play e o registro.

**Nenhum script, cena, prefab ou asset foi alterado nesta entrada.**

**Decisões do plano que o design deixava para quem constrói:**
- **Ligação pelo caminho na hierarquia:** o `poses.tsv` aprovado diz o caminho de cada foco (com os espaços dos nomes das garrafas) e o builder copia o PNG aprovado daquele número, sem regerar.
- **Nome do arquivo:** o caminho sem o grupo do topo, com ` - ` entre as partes (ex.: `PilhadePneu - pneu (5).png`).
- **Semente do círculo do gerador:** FNV-1a do caminho (estável entre execuções).
- **Peças novas na cena atual** por um menu idempotente (`Dengue > Fotos dos Focos > Preparar celular para fotos`), porque o `Criar Celular` recria o `CelularHUD` e zera a `RedeSocial` editada.
- **Testes:** categoria NUnit `Fotos`; o relógio de "empacado" vira `AvancarEmpacado(dt, posição, lendoCelular)` para os testes chamarem sem o `Update`.

**Observações técnicas:** `Assets/a.unity` e `LiberationSans SDF - Fallback.asset` ficam fora dos commits desta feature.

### 2026-11-17 — Fotos dos focos: implementação, verificação em Play e registro

**Objetivo (pedido do usuário):** implementar o design aprovado em 2026-11-17 (`docs/superpowers/specs/2026-11-17-fotos-dos-focos-design.md`) pelo plano (`docs/superpowers/plans/2026-11-17-fotos-dos-focos.md`): quando o jogador empaca (`tempoSemFoco` = 60 s sem eliminar foco), alguém do bairro posta na rede social a foto de um foco, com um círculo vermelho em volta; a notificação mostra a miniatura e "enviou uma foto"; quando o foco é eliminado, o card ganha o selo **RESOLVIDO**. Branch `fotos-dos-focos` (a partir de `378022e`).

**Commits (`git log --oneline 378022e..HEAD`, sem o deste registro):**
- `dc02264` Plano: fotos dos focos na rede social
- `1de4589` Fotos dos focos: escolha do foco (lógica pura)
- `8bb9fe3` Fotos dos focos: narrador posta a foto quando o jogador empaca
- `7c50c67` Fotos dos focos: FalaAoEliminar usa o FalaEmOrdemDepoisSorteada
- `be06e77` Fotos dos focos: foto e selo Resolvido no card do feed
- `8938bdf` Fotos dos focos: miniatura da foto na notificação
- `da9f292` Fotos dos focos: builder (importar, gerar, vista da Scene, preparar celular)
- `d76cc25` Fotos dos focos: Gerar não regrava o PNG de outro foco que divide a foto
- `f3bfd5a` Fotos dos focos: as 31 fotos aprovadas ligadas aos focos

**Arquivos novos (cada um com o `.meta`):**
- `Assets/Scripts/EscolhaDaFoto.cs` — a escolha do foco, em lógica pura (`Escolher(candidatos, posição do jogador, raio, última foto)`): o mais perto que ainda não saiu em foto e não está a até `raioLugarConhecido` de um lugar conhecido; se não houver, uma 2ª passada repete um já fotografado, menos o da última foto; empate, o de menor índice.
- `Assets/Scripts/FotoDoFoco.cs` — componente no foco: a foto (`Texture2D`) e a pose da câmera que a gerou (posição, rotação, campo de visão).
- `Assets/Editor/FotosDosFocosBuilder.cs` — menus **Dengue > Fotos dos Focos >** "Importar fotos aprovadas", "Gerar fotos (todos os focos)", "Gerar foto (focos selecionados)", "Usar a vista da Scene no foco selecionado" e "Preparar celular para fotos". O importar lê o `poses.tsv`, liga cada linha ao foco pelo caminho na hierarquia e **copia** o PNG aprovado (não regera).
- `Assets/Meus/Assets Gerados/Fotos dos Focos/` — as 31 fotos aprovadas (720×480, sem mipmaps, `npotScale` None), com nome = caminho sem o grupo do topo, partes separadas por ` - ` (ex.: `PilhadePneu - pneu (5).png`).
- Testes (categoria NUnit `Fotos`): `EscolhaDaFotoTestes.cs` (6), `HistoriaFotoTestes.cs` (16), `FeedFotoTestes.cs` (8), `NotificacaoFotoTestes.cs` (3) e `FotosDosFocosBuilderTestes.cs` (11).
- `docs/superpowers/plans/2026-11-17-fotos-dos-focos.md`.

**Arquivos alterados:**
- `Assets/Scripts/HistoriaRedeSocial.cs` — o relógio de "empacado" virou `AvancarEmpacado(dt, posição, lendoCelular)` (para com o celular aberto, zera a cada foco eliminado); no limite, `TentarFotoDeFoco`: **Saiu** zera o relógio; **SemFoco** posta a bronca antiga (`aoDemorarSemFoco`); **Recusada** pela janela de conflito tenta de novo 1 s depois sem gastar o foco nem o texto. Quem posta é o dono do tipo (`aoMandarFoto`, novo em cada personagem); tipo sem dono ou dono sem texto, a UPA (`aoMandarFotoSemDono`). "Mexido" (passo, esvaziar com as Mãos, tampa pega, pneu pego) tira o foco da foto e o torna lugar conhecido. `UltimaFoto` (interno). O `FalaAoEliminar` passou a usar o `FalaEmOrdemDepoisSorteada` (mesma ordem e sorteio).
- `Assets/Scripts/RedeSocial.cs` — `PostRedeSocial.foto` e `PostRedeSocial.foco` (`[NonSerialized]`).
- `Assets/Scripts/FeedRedeSocial.cs` e `PostView.cs` — moldura da foto no card (ligada só com foto) e o carimbo: no `OnFocoEliminado`, a foto escurece e o selo entra com "pop"; post de foco já eliminado entra carimbado, sem pop.
- `Assets/Scripts/NotificacaoCelular.cs` — miniatura e cabeçalho "enviou uma foto"; sem a miniatura na cena, o post com foto aparece como os outros.
- `Assets/Editor/CelularBuilder.cs` — `GarantirFotoNoCard` (idempotente; cria a moldura "Foto" e o selo "Resolvido" no `ModeloPost` e liga as referências do `PostView`).
- `Assets/Editor/HistoriaBuilder.cs` — `GarantirMiniatura` (idempotente; cria "Miniatura" no banner, liga as referências e alarga o banner).
- `Assets/Scenes/SampleScene.unity` — rodados os menus "Preparar celular para fotos" e "Importar fotos aprovadas": `ModeloPost/Foto` (com o selo), `Notificacao/Miniatura` e um `FotoDoFoco` em cada um dos 31 focos.

**Configurações aplicadas (padrões no código):**
- `tempoSemFoco` 60 s (já existia; a cena tem 60); `raioLugarConhecido` 4 m; nova tentativa da foto recusada em 1 s.
- Post da foto: prioridade `Normal`, humor `Neutro`, curtidas sorteadas (`-1`).
- Foto: 720×480, FOV 60, near 0,1, far 400 (cópia da Main Camera, com o pós-processamento do URP).
- Card: moldura com altura 237 e a largura do card (356 px de conteúdo, 3:2); selo 176×44, cor (0,20; 0,68; 0,30; 0,95), giro de 8°, "RESOLVIDO" em TMP 22 negrito branco, espaçamento 6; foto carimbada com a cor (0,62; 0,62; 0,62; 1); pop do selo 1,3 → 1 em 0,25 s, em tempo sem escala.
- Notificação: banner com 500 px de largura (era 440); miniatura 96×64, âncora direita-meio, x −14, com `Mask`; área do texto termina em −118 com foto (−18 sem); cabeçalho com `sizeDelta.x` −190 com foto (−90 sem), texto `<b>{autor}</b>  <color=#FFFFFF80>enviou uma foto</color>`.

**Testes:** suíte EditMode inteira, **621/621** (categoria `Fotos`: 44/44), rodada numa cena vazia.

**Verificado em Play (SampleScene no Editor, nada salvo na cena):**

Como: `Application.runInBackground = true` só durante o teste (em runtime; o `PlayerSettings` ficou false); abertura pulada como no preview (`CartelasNarrativas` desligada, `Time.timeScale = 1`, `ControleAtivo` true, `Interactor.Bloqueado` false, `CelularController` ligado); a saída do hospital refeita como o `SaidaDoHospital` faz (teleporte para a `posicaoNoBairro` (−6,23; 1,69; −20,04) e `FluxoDeJogo.IniciarPartida()`). Os quadros andaram (cerca de 400 por segundo), então nenhuma animação foi forçada. Capturas com os canvases trocados para Screen Space Camera e a Main Camera renderizada num `RenderTexture` 1920×1080, em `.superpowers/sdd/2026-11-17-fotos-dos-focos/verificacao/` (pasta fora do git).

- **Agente parado 60 s de verdade:** no limite do relógio, Dona Neusa postou a foto do `Vaso de Flor`, o foco com foto mais perto do agente (13,5 m, a previsão feita antes pelas distâncias), com o texto "Agente, olha esse vaso. Pratinho com água é convite pro mosquito."; o card entrou no topo do feed com a foto; `UltimaFoto` = `Vaso de Flor`.
- **Foco eliminado** (`Interact()` 10 vezes, até `IsComplete`): o card daquele foco ganhou o selo, a foto escureceu para 0,62 e o pop terminou em escala 1 (lido 2 s depois); o relógio zerou; saiu o post de eliminação da Dona Neusa.
- **Limite repetido** (`timerSemFoco` posto em 59,7 por reflexão): Seu Osvaldo postou a foto do `Ralo (4)` (o 2º mais perto, 14,2 m), outro foco; notificação com a miniatura do ralo, "enviou uma foto", cabeçalho −190 e texto −118. Uma 3ª vez (para a captura, porque o banner do ralo já tinha saído em 4,5 s): Lúcia Andrade com a `Bromelia (1)` (18,8 m).
- **Capturas:** `1 - celular fechado, notificacao com a foto.png` (banner da bromélia com a miniatura), `2 - celular aberto, post com a foto.png` (card da bromélia no topo, com o círculo vermelho), `3 - celular aberto, card carimbado.png` (card do vaso, escurecido, com o selo RESOLVIDO; o feed foi rolado até ele por código).
- **Console:** nenhum erro do jogo. O único erro novo é do próprio CLI ("Failed to handle /api/exec request: Main thread operation timed out after 5000ms"), no momento da entrada no Play.
- Fora do Play: `runInBackground` false e a `SampleScene` sem mudanças (`isDirty` falso).

**O que NÃO foi verificado:**
- **Partida real pelo usuário,** com mouse e teclado: se a foto ajuda mesmo a achar o foco (o objetivo do design), o tamanho da miniatura e o ritmo de 60 s.
- **Em Play, só nos testes EditMode:** a foto recusada pela janela de conflito (nova tentativa em 1 s), a bronca quando não há foco com foto, o post da UPA (os 7 tipos da cena têm dono), o relógio parado com o celular aberto, os "mexidos" (passo, esvaziar, tampa, pneu pego) e um foco destruído ao concluir.
- **Eliminar pelo kit:** o foco foi eliminado por `Interact()` (o caminho sem kit). Um pneu não conclui assim (precisa da caçamba).
- **A tela de verdade:** as capturas usam Screen Space Camera, não o Overlay do jogo; a Game view do Editor não foi vista.
- **Build exportada.**
- **Step 3 da Task 6 (gerador × aprovadas), aceite do usuário pendente.** As 31 fotos do jogo são as aprovadas byte a byte (hash). Mas uma foto regerada a partir da pose guardada no `poses.tsv` sai deslocada por menos de um pixel, porque o `poses.tsv` (formato do spec) guarda a pose com 3 casas e em ângulos de Euler: fora da faixa do círculo, máx. Δ 145/157/169, médio 0,246/0,190/0,058 e Δ>2 em 0,22/0,24/0,20% dos pixels (focos 04 Bromélia (4), 15 pneu (10) do monte, 24 Ralo (1)). Com a pose exata que o gerador dá (o quaternion do `LookRotation`), Δ = 0: a receita do gerador reproduz as aprovadas. O critério ficou como decisão do controlador (R4′, em `.superpowers/sdd/2026-11-17-fotos-dos-focos/contexto-execucao.md`), sem aceite do usuário; a página `capturas/decisao-step3-fotos-dos-focos.html` mostra a tabela e as opções: (a) aceitar como está, ou (b) guardar a pose em quaternion com posição completa, o que muda o spec (seção 3 e apêndice), o importar e o `FotoDoFoco`. Só importa se o usuário refizer uma foto sem mudar a pose.

**Observações técnicas:**
- **Post de "parado" cobre a foto:** com o agente parado, o post de `aoFicarParado` (a cada 35 s) saiu cerca de 10 s depois da foto, empurrou o card para a 2ª posição e trocou o banner. É o comportamento de antes (o design só troca a bronca `aoDemorarSemFoco` pela foto), mas pode esconder a ajuda de quem está parado. Fica para o usuário decidir.
- **Auto save de terceiros:** o `EditorAutoSave` (`Assets/IntenseNation/Editor Auto Save/`) salva as cenas abertas por tempo. Na Task 6 foi ele que salvou a `SampleScene` (antes do `SaveScene` do plano); conferido que só entraram os 31 `FotoDoFoco` e os 6 objetos de UI. Em tarefas futuras, uma cena mexida por teste pode ser salva sem pedido.
- **Cena re-serializada** no primeiro save depois de mudanças antigas de código: 8 `PneuInteractable` perderam campos órfãos da onda (removida em `99bcbad`) e ganharam `grausParaEsvaziar: 150`; apareceram `KitDoAgente.multiplicadorCheio` 0,5, `SomDosFocos.kit`, `PlacarDaPartida.interactor` e `FeedRedeSocial.gerenciador` vazios. São os padrões do código ou têm busca em runtime; o comportamento não muda.
- **Textos das fotos no Inspector:** `aoMandarFoto` dos 7 personagens e `aoMandarFotoSemDono` estão salvos vazios na cena; o `Awake` preenche os textos do spec em runtime (o mesmo padrão dos outros textos novos). Fora do Play o Inspector mostra as listas vazias.
- **Miniatura sem mipmaps:** a foto de 720×480 cai 7,5 vezes na miniatura de 96×64; bordas finas podem serrilhar na animação do banner. Se aparecer, ligar os mipmaps custa cerca de 1,8 MB.
- **Erro do CLI no Console:** "Main thread operation timed out after 5000ms" aparece quando um comando chega durante um domain reload ou um menu demorado; não é do jogo.
- **Achados menores adiados** (testes que faltam, avisos do importar, guardas de Play nos menus etc.) estão listados em `.superpowers/sdd/2026-11-17-fotos-dos-focos/progress.md` para a revisão final da branch.
- **Material fora do git, mantido no disco:** `.superpowers/sdd/2026-11-17-fotos-dos-focos/` (fotos aprovadas, `poses.tsv`, capturas, scripts de `eval_file`).
- **Mudanças não minhas no disco:** `LiberationSans SDF - Fallback.asset` continua modificado e `Assets/a.unity` continua sem commit, como já estavam.

### 2026-11-18 — Fotos dos focos: correções da revisão final

**Objetivo (pedido do usuário):** corrigir os achados da revisão final da branch `fotos-dos-focos` (`.superpowers/sdd/2026-11-17-fotos-dos-focos/final-review.md`): os dois *Important* e os quatro *minors* de uma linha recomendados.

**Arquivos alterados:**
- `Assets/Scripts/HistoriaRedeSocial.cs` — `Candidato`: foco destruído continua **lugar conhecido** (`conhecido = true`, fora da escolha). Só o `Interactable.FinishRemoval`, depois de concluir, destrói um foco, então destruído = eliminado (spec §4). Antes, eliminar uma `Lata 001` (`destroyOnComplete`) esquecia o lugar, e a 1ª passada podia mandar a foto de um foco a menos de 4 m dali. `HandleFocoEliminado` zera também o `esperaDaFoto` junto com o `timerSemFoco` (sem o atraso de até 1 s de uma foto recusada antes).
- `Assets/Editor/FotosDosFocosBuilder.cs` — os 5 menus não rodam em Play (`EditorApplication.isPlayingOrWillChangePlaymode`: só um aviso no Console, porque a foto sairia do estado da partida). "Gerar fotos (todos os focos)" pede confirmação (`DisplayDialog`: sobrescreve as fotos aprovadas pelo usuário com renders novos, sem Desfazer) e usa `FindObjectsInactive.Exclude` (foco inativo sairia como foto sem o foco). Os dois "Gerar" param com um aviso quando não há Main Camera. No `Renderizar`, o `RenderTexture.active` volta ao anterior e a câmera e a RenderTexture temporárias são soltas no `finally`, mesmo com exceção.
- `Assets/Scripts/PostView.cs` — `foco` virou `[System.NonSerialized]` (só em runtime; antes ia como null no `ModeloPost`).
- `Assets/Editor/Testes/HistoriaFotoTestes.cs` — 2 testes novos: `Empacado_FocoDestruido_ContinuaLugarConhecido` (foco destruído a 2 m do vaso: o vaso fica fora da 1ª passada e sai o pneu, longe; falhava antes da correção) e `Empacado_EliminacaoDepoisDaRecusa_ZeraAEspera`. O `Empacado_FocoDestruido_Ignora` continua passando.

**Testes:** suíte EditMode inteira, **623/623** (eram 621; +2 da categoria `Fotos`), rodada numa cena vazia. Conferido também por `eval_file`, numa cena vazia, o aviso de "Nenhuma Main Camera" do `GerarVarias` (nenhuma foto gravada). Os menus em Play e o diálogo de confirmação não foram exercitados (o Editor não entrou em Play e o diálogo é modal).

**Observações técnicas:**
- Nada de cena foi salvo; a `SampleScene` e as 31 fotos aprovadas não mudaram.
- Continua pendente o aceite do usuário sobre a R4′ (gerador × aprovadas fora do círculo), registrado na entrada anterior.

### 2026-11-21 — Fotos dos focos sem círculo e nova foto do Vaso de Flor (4)

**Objetivo (pedidos do usuário):** aceitar o gerador de fotos como está ("aceito"), tirar o círculo vermelho das fotos ("pode tirar o círculo das fotos") e trocar só a foto do Vaso de Flor (4), escolhendo a opção 31E entre 4 ângulos novos.

**Arquivos alterados:**
- `Assets/Editor/FotosDosFocosBuilder.cs`: o gerador não desenha mais o círculo; saíram `SementeDe`, `DesenharCirculo`, `RetanguloNaFoto` e a cor `Vermelho`. Comentários atualizados.
- `Assets/Editor/Testes/FotosDosFocosBuilderTestes.cs`: saíram os 2 testes do círculo (`SementeDe_Estavel`, `DesenharCirculo_PintaABordaENaoOCentro`).
- `Assets/Meus/Assets Gerados/Fotos dos Focos/*.png`: as 31 fotos regeradas sem círculo, das poses guardadas (mesmos ângulos aprovados).
- `Assets/Scenes/SampleScene.unity`: pose do `FotoDoFoco` do Vaso de Flor (4) trocada pela 31E — posição (−30,477; 1,893; 16,720), rotação (3,54; 330; 0), FOV 60. Também saiu do card modelo o campo `foco` (agora `[NonSerialized]`, do commit anterior; o Unity regravou a cena ao fechar).
- `docs/superpowers/specs/2026-11-17-fotos-dos-focos-design.md`: bloco "Mudanças depois da implementação" no topo.

**Verificação:** suíte EditMode inteira 621/621 (eram 623, menos os 2 testes do círculo). Diff da cena conferido linha a linha: só a pose do Vaso de Flor (4) e o `foco: {fileID: 0}` do card. A foto nova do Vaso de Flor (4) conferida contra a opção 31E. **Não verificado:** Play depois da troca (a lógica não mudou, só as imagens).

**Observações técnicas:**
- `.superpowers/sdd/2026-11-17-fotos-dos-focos/aprovadas/` agora guarda as fotos limpas e o `poses.tsv` com a linha 31 nova, para o menu "Importar fotos aprovadas" não trazer de volta as fotos com círculo; as antigas ficaram em `aprovadas/com-circulo-2026-11-17/`.
- Um `eval_file` que gera as 31 fotos passa do limite de 5 s do Pipeline e devolve erro, mas o Unity termina o trabalho; conferir pelo `git status` antes de repetir.

### 2026-11-21 — Bromélia sempre nasce com água

**Objetivo (pedido do usuário):** toda bromélia do mapa deve ter o minigame. Antes, metade dos focos nascia sem água pelo sorteio do começo da partida, e uma bromélia sem água não tem minigame.

**Arquivos alterados:**
- `Assets/Scripts/BromeliaInteractable.cs`: `SempreNasceComAgua` passa a `true`, como no ralo. O cabeçalho do arquivo diz isso.
- `Assets/Editor/Testes/NascimentoTestes.cs`: teste novo `Bromelia_NasceSempreComAgua_ForaDoSorteio`.

**Configurações aplicadas:** nenhuma. A `fracaoNascendoComAgua` (0,5 na `SampleScene`) continua valendo para os outros focos.

**Testes:** suíte EditMode inteira, 622/622.

**Observações técnicas:**
- **Balanceamento:** as 4 bromélias saem do sorteio, como o ralo. A fração de 0,5 passa a valer só para os focos que sobram, então o mapa começa com mais focos com água do que antes.
- **Não verificado em Play.**
- A chuva continua podendo encher uma bromélia já esvaziada, e o `NascerVazio` continua existindo, embora a bromélia não passe mais por ele no começo da partida.

### 2026-11-21 — Design: fotos dos focos por tipo

**Objetivo (pedido do usuário):** o jogador costuma ir limpando focos antes de a foto sair (ela só saía depois de 60 s sem eliminar nenhum foco). A foto passa a sair por tipo: se o jogador não elimina nenhum foco de um tipo (ex.: vaso) em 90 s, alguém posta a foto de um foco daquele tipo.

**Decisões do usuário:** relógio por tipo de foco, 90 s para todos (ajustável por tipo no Inspector), abordagem "relógio por tipo no narrador".

**Arquivos novos:** `docs/superpowers/specs/2026-11-21-fotos-por-tipo-design.md` (aprovado pelo usuário). Branch `fotos-por-tipo`, a partir da `main` em `c1806e5`.

**Nenhum script, cena, prefab ou asset foi alterado nesta entrada.**

**Propostas minhas aceitas com o design:** começo escalonado (o 1º tipo vence aos 90 s e os seguintes a cada 15 s, na ordem do enum `TipoFoco`); o "empacado" geral sai e a bronca `aoDemorarSemFoco` fica sem uso (o texto estaria errado para quem limpa outros tipos); tipo sem foco elegível zera o relógio sem postar.

### 2026-11-21 — Plano: fotos dos focos por tipo

**Arquivos novos:** `docs/superpowers/plans/2026-11-21-fotos-por-tipo.md`, com 2 tarefas: o relógio por tipo no `HistoriaRedeSocial` (com os testes do "empacado" trocados pelos por tipo) e a verificação em Play com o registro.

**Nenhum script, cena, prefab ou asset foi alterado nesta entrada.**

**Decisões do plano:** campos novos `tempoPorTipo` (90), `intervaloEntreTipos` (15) e `temposPorTipo` (lista tipo → segundos); `AvancarEmpacado`/`TentarFotoDeFoco` viram `AvancarRelogiosDosTipos`/`TentarFotoDoTipo`; a 2ª passada da escolha evita a última foto **do tipo**; vários tipos vencidos juntos saem um por vez pela janela de conflito.

### 2026-11-21 — HUD sem contagem de focos: a informação fica só no celular

**Objetivo (pedido do usuário):** a HUD não deve mostrar quantos focos faltam; o jogador se informa só pelo celular (subtítulo do VizinhApp `{eliminados}/{total} focos eliminados` e os posts com `{restantes}`/`{restantesTipo}`).

**Arquivos alterados:**
- `Assets/Scenes/SampleScene.unity`: `m_IsActive` 1 → 0 em três GameObjects da `DengueHUD`: `FocoListaPanel` (lista "Focos faltantes" do Tab, com o componente `FocoListaHUD`), `FocoListaDica` ("TAB — focos restantes") e `CounterPanel` ("FOCOS ELIMINADOS x/y").

**Configurações aplicadas:** nenhuma outra. Nenhum script mudou.

**Observações técnicas:**
- Os objetos foram **desativados, não apagados**: a `DengueHUD` continua contando (escreve no `CounterText` inativo sem erro; a validação dela só acusa referência nula), o aviso "Foco eliminado!" continua usando `FocoListaHUD.Categoria()` (estático) e o `NotaBuilder` acha o `FocoListaHUD` inativo (`FindObjectsInactive.Include`). Para voltar, basta reativar os três objetos.
- Com o `FocoListaPanel` inativo, o Tab não faz mais nada e o `comecarAberto` deixa de importar.
- Os builders (`Dengue > Criar Lista de Focos (Tab)`, Criar HUD) não chamam `SetActive(true)`, então rodá-los de novo não religa os painéis.
- **Não verificado em Play** (edição direta do YAML da cena; ela é LFS).
