# Foco Zero — Parte B: Focos vivos, chuva e entregas com peso

Spec da Parte B, aprovada pelo usuário em 2026-10-29 (brainstorming).
Unity 6000.5.6f1, URP, Input System 1.20. Base: Parte A pronta e mesclada na `beta-0.3-audio`
(commits `44891b9` e `5a00f5e`).

## Context

O Foco Zero é um trabalho de faculdade que precisa prender quem joga por vontade própria e ter um pouco de
rejogabilidade. O prazo total é de 2 a 4 semanas. Roteiro: **A → B → C**.
- **A** (pronta): kit do agente, segurar para agir, ferramenta certa por foco, pneu carregado, saco de garrafas.
- **B** (esta spec): o mapa ganha urgência. Os focos evoluem, a chuva cobra o atalho e as entregas ganham peso e recompensa.
- **C** (depois): focos sorteados, pontuação/estrelas, recorde, muletas mais leves.

Problema que a B resolve: hoje um foco esperando é igual a um foco recém-achado. A única pressão é a barra, e
não há escolha de "o que fazer primeiro". A ideia original, "a chuva reenche o que só foi esvaziado", não cabia
mais depois da A, em que todo foco recebe a ação definitiva. Por isso a B **cria o atalho de esvaziar**.

Cena atual: 22 focos (6 pneus, 4 ralos, 4 bromélias, 3 vasos, 3 grupos de garrafas, 2 caixas d'água; não há piscina).

### Decisões do usuário
- Papel da chuva: **atalho que cobra depois**.
- Foco no estágio mosquito: **pesa mais na barra**. Não espalha e não cria focos novos.
- Sinais do estágio: **no mundo e na mira**. A lista (Tab) não mostra o estágio, e não há aviso na HUD.
- Focos com atalho: **vaso, pneu e garrafas**.
- Esvaziado: **alivia a barra, não vence**.
- Chuva: **com previsão no celular**.
- Arquitetura: **gerenciador central de focos**.
- Feedback tem muita importância, sobretudo **carregando**.
- O agente **segura o pneu com os braços**, sem ícone flutuante.
- Pneus vão para a **caçamba da picape da UPA**, a única que existe.
- Garrafas vão para as **lixeiras de rua**. As 3 caçambas de entulho saem da cena.
- Pneus passam a ter o **tamanho real (~0,7 m)**.

## Jogabilidade

### Ciclo de vida
- Todo foco ativo (não tratado e não esvaziado) passa por **ovo → larva → mosquito**. Cada etapa leva `segundosPorEstagio` (padrão 75 s).
- O relógio só anda depois do `FluxoDeJogo.PartidaIniciada`, e o mosquito é o estágio final.
- **Início sorteado a cada partida:**
  - `focosComMosquitoNoInicio` focos (padrão 3) começam com mosquito;
  - cerca de um terço dos restantes começa em larva, e o resto em ovo;
  - o tempo já passado no estágio inicial é sorteado entre 0 e `segundosPorEstagio`, para os focos não virarem todos juntos.
- **Peso na barra:** ovo 1, larva 1,5, mosquito 2,5. O esvaziado e o tratado pesam 0. Os valores ficam no Inspector do gerenciador.

### Atalho de esvaziar (vaso, pneu e garrafas)
- **Segurar com as Mãos** esvazia. São `passosParaEsvaziar` passos (padrão 3) em `segundosParaEsvaziar` (padrão 1,2 s).
- O foco fica no lugar, sem água (`Esvaziado`), pesa 0 e não zumbe.
- O atalho não dá o `alivioPorFoco`, não dispara `OnCompleted` e não conta para a vitória.
- Só vale **antes de começar a ação definitiva** (`currentClicks == 0`). Com a areia já pela metade, Mãos no vaso continuam dando `FerramentaErrada`.
- Depois de esvaziado, **a ferramenta certa ainda resolve de vez**: Areia no vaso, Saco nas garrafas, pegar o pneu e entregá-lo.
- Mãos num foco já esvaziado dão `ResultadoDoPasso.JaVazio`, com o aviso "Já está vazio, mas a chuva enche de novo. A **areia** resolve de vez." (o nome da ferramenta muda por foco).
- **Pneu:** como as Mãos já são a ferramenta dele, o caminho normal da A vira o esvaziar. O último passo só esvazia e o pneu **fica no lugar**. Segurar de novo com as Mãos **pega** o pneu, num passo instantâneo, como na A.
- Ralo, bromélia e caixa d'água não têm atalho. Para eles, Mãos continuam dando `FerramentaErrada`; a caixa d'água continua sendo tampada com as Mãos.

### Chuva
- Estados: **Seco → Previsão (45 s) → Chovendo (20 s) → Seco**.
- O início de cada chuva é sorteado entre 180 e 240 s depois do início da anterior. A primeira conta a partir do `IniciarPartida`. A previsão começa 45 s antes de cada início.
- **Na previsão:** sai um post no celular, com a notificação, e o céu começa a escurecer.
- **No começo da chuva:** todo foco esvaziado (e não tratado, nem carregado) é **reenchido**. A água volta e ele retorna como **larva**, porque os ovos grudados na parede eclodem. O pneu largado em outro lugar também é reenchido. O pneu nos braços, não.
- A chuva não acelera o ciclo dos outros focos.
- A partida encerrada para o relógio da chuva.

### Destinos e entregas (mudam a Parte A)
- **Pneu → caçamba da picape da UPA**, a única que existe, perto da saída do hospital.
  - Mirar na carroceria com o pneu nos braços entrega. Largar com o raio caindo na carroceria também entrega.
  - Mirar na cabine (sem pneu) continua reabastecendo o kit.
- **Garrafas → lixeiras de rua.**
  - As 3 lixeiras que já existem viram pontos de uso: `Trassh Bin` (7, -6), `Trassh Bin (1)` (-10, -15) e `Trash_01` (-20, 24).
  - O builder cria mais 2 perto dos grupos de garrafas que ficam a mais de 20 m de uma lixeira: o do terreno baldio norte (cerca de (-19, 60) e (-26, 44)) e o de (-32, -18).
  - A lixeira esvazia o saco. Com pneu nos braços, ela recusa: "Pneu vai na caçamba da **picape da UPA**."
- **As 3 caçambas de entulho saem da cena**, e o `KitDoAgenteBuilder` para de criá-las.
- **Pneus em tamanho real:** os 6 pneus da cena passam de cerca de 2 m para cerca de 0,7 m de diâmetro, numa escala uniforme aplicada pelo builder. A água (filho) acompanha.
- **Velocidade carregando:** `multiplicadorCarregando` passa de 0,7 para 0,85 (valor final pelo balanceamento). Continua sem correr nem pular.

### Vitória e derrota
Não mudam. A vitória exige que todos os focos estejam tratados de vez antes de a barra chegar a 100%.

## Sinais e feedback

### No mundo
- **Mosquito:** um enxame de pontinhos escuros (partículas criadas em tempo de execução) voando em volta do foco, visível a 15–20 m.
- **Larva:** não há sinal visível de longe.
- **Esvaziado:** sem água (pratinho seco, pneu e garrafas sem água).
- **Chuva:**
  - na previsão, a intensidade da luz direcional cai e a névoa aumenta, em rampa;
  - durante a chuva, partículas caem presas à câmera;
  - em 10 s depois do fim, tudo volta ao normal.

### Carregando o pneu
- **O agente segura o pneu com os braços.**
  - Uma ação nova, "Carregando", entra no `gerar_agente.py`, com os braços à frente e as mãos nas laterais do pneu.
  - No `Agente_AnimController` ela fica numa camada com `AvatarMask` (braços e tronco; o rig é Generic) e peso 1 enquanto o agente carrega.
  - O pneu fica num `PontoDeCarga` filho do osso do tronco, entre as mãos, na altura da cintura e do peito e um pouco inclinado para não tampar a visão. Esse ponto deixa de ser filho da câmera.
  - Há um **preview no Unity aprovado pelo usuário** (skill `preview-design`) antes de fechar a pose.
- **Peso no corpo:** o head bob do `CameraJogador` fica mais amplo e mais lento, com uma leve inclinação lateral. Os passos ficam mais pesados, e há um som de esforço baixo a cada poucos segundos.
- **HUD:** um aviso fixo, "Carregando pneu. Leve à **picape da UPA**. [Botão direito] larga."
- Não há ícone flutuante nem seta.

### Carregando o saco
- Um **saco visível na mão do agente** (modelo low poly gerado por script) cresce conforme as garrafas que tem. Ao andar, as garrafas tilintam.
- **Saco cheio:** o aviso da HUD diz "Saco cheio. Jogue numa **lixeira**." Não há ícone flutuante.

### Entregas, o momento de recompensa
- **Pneu na picape:**
  - o pneu é arremessado num arco até a carroceria, cai com um baque e a suspensão da picape balança;
  - saem partículas e o aviso "Pneu recolhido — N de T" (T = total de pneus da partida);
  - **os pneus ficam empilhados na carroceria**, em 6 vagas, e não somem.
- **Garrafas na lixeira:** a tampa abre, o saco vira com som de vidro e a tampa fecha.

### Som (`SomDosFocos` e novos)
- O volume do zumbido varia por estágio: ovo baixo, larva médio, mosquito alto. O esvaziado não zumbe.
- Sons novos, gerados por `Ferramentas/Audio/gerar_sons.py`: loop de chuva, trovão, esvaziar (splash), baque do pneu na picape, rangido da suspensão, passo pesado, esforço, tilintar do saco e tampa da lixeira.

### Na mira (`DengueHUD`)
- **Larva e mosquito:** o texto do foco ganha um complemento: "— **com larvas**" ou "— **com mosquitos voando**".
- **Esvaziado:** "… vazio, por enquanto". A ação padrão passa a ser "Segure com a **ferramenta certa** para resolver de vez", com o nome da ferramenta.
- **Mira na picape com o pneu nos braços:** "Segure para jogar o pneu na caçamba".
- **Mira na lixeira:** "Segure para esvaziar o saco".

### No celular (`HistoriaRedeSocial`)
- **Previsão a cada chuva:** um post sorteado entre uns 4 textos, sem repetir o último.
- **Primeiro esvaziar:** "Esvaziar ajuda, mas os ovos ficam grudados na parede. Se chover, eles voltam."
- **Primeiro foco reenchido:** "Choveu e aquele pratinho já está cheio de larva de novo. Tem que pôr areia." O texto muda por tipo de foco.
- **Primeiro pneu na picape:** um post do Carlos, que substitui o da caçamba.
- **Primeiro saco na lixeira:** um post da Bruna, que substitui o da caçamba.
- Os posts da A que citam a "caçamba" são revistos.
- Não há post nem aviso quando um foco vira mosquito.

### Sem mudança
A lista (Tab) e o contador de focos: o esvaziado continua contando como restante.

## Arquitetura

### Arquivos novos (`Assets/Scripts/`)
- **`GerenciadorDeFocos.cs`:** um por cena, no grupo dos focos. É **a única varredura de `Interactable`** do jogo.
  - **Registro:** no `Awake`, pega todos os focos, inclusive os inativos (`FindObjectsInactive.Include`). Tem `Garantir()` preguiçoso para os testes EditMode.
  - **Propriedades:** `Focos` (`IReadOnlyList<Interactable>`), `Total`, `Eliminados` e `Restantes`.
  - **Eventos:** `OnFocoEliminado`, `OnFocoClicado`, `OnEstagioMudou(Interactable, EstagioDoFoco)`, `OnFocoEsvaziado` e `OnFocoReenchido`. Ele repassa os `OnCompleted`, `OnClicado`, `OnEsvaziado` e `OnReenchido` de cada foco, que continuam existindo.
  - **Ciclo:** um `CicloDeVida` por foco, avançado num único `Update`, só com a partida iniciada e não encerrada.
  - **Consultas e ações:** `EstagioDe(foco)`, `Pressao()` (soma dos pesos dividida por `Total`), `ReencherEsvaziados()` e o sorteio do início (`SortearInicio(System.Random)`).
  - Os campos do Inspector guardam os tempos, os pesos e `focosComMosquitoNoInicio`.
  - **Parte C:** o sorteio de quais focos existem vai morar aqui.
- **`CicloDeVida.cs`:** classe C# pura com `enum EstagioDoFoco { Ovo, Larva, Mosquito }`, `Estagio`, `TempoNoEstagio`, `bool Avancar(float dt)` (devolve se mudou), `Zerar()` (ao esvaziar), `Reencher()` (volta como larva) e `Definir(estagio, tempo)`.
- **`Chuva.cs`:** um por cena.
  - Máquina de estados Seco/Previsao/Chovendo, que avança com `Avancar(float dt)`, testável sem `Update`.
  - O intervalo é sorteado com `System.Random` injetável.
  - Eventos `OnPrevisao`, `OnComecou` e `OnTerminou`. No `OnComecou`, chama `gerenciador.ReencherEsvaziados()`.
  - Liga-se ao `FluxoDeJogo` (início e encerramento).
- **`ChuvaVisual.cs`:** escuta a `Chuva` e cuida das partículas presas à câmera, da rampa da luz direcional e da névoa, do loop de chuva e do trovão.
- **`EnxameDeMosquitos.cs`:** escuta o `OnEstagioMudou` e liga e desliga um enxame de partículas em cada foco com mosquito. Também desliga ao concluir ou esvaziar.
- **`Lixeira.cs`:** um `PontoDeUso` que esvazia o saco, recusa o pneu e anima a tampa.
- **`FeedbackDeCarga.cs`** (no Player): enquanto `KitDoAgente.PneuCarregado != null`, liga a camada "Carregando" do Animator, o bob pesado do `CameraJogador` e os sons de esforço. Também mostra o saco na mão e o faz crescer.

### Arquivos alterados
- **`Interactable.cs`:**
  - **`Esvaziado` sai do `PneuInteractable` e vai para a base**, com os campos `podeEsvaziar`, `passosParaEsvaziar` e `segundosParaEsvaziar`;
  - os eventos `OnEsvaziado` e `OnReenchido`, os ganchos virtuais `AoAvancarEsvaziando(int passo)`, `AoEsvaziar()` e `AoReencher()`, e o público `Reencher()`;
  - o desvio no `Verificar` e no `AvancarCom`: Mãos, `podeEsvaziar`, `ferramentaNecessaria != Maos` e `currentClicks == 0` levam ao caminho de esvaziar; se o foco já está esvaziado, o resultado é `JaVazio`;
  - o `SegundosPorPasso` respeita o caminho de esvaziar;
  - o `Zumbe` passa a ser falso quando esvaziado.
- **`Ferramenta.cs`:** `ResultadoDoPasso.JaVazio` entra **no fim** do enum.
- **`PneuInteractable.cs`:**
  - o último passo (`AoTerminarAcao`) só esvazia e o pneu fica no lugar;
  - com o pneu esvaziado, as Mãos dão `Pegou`;
  - `AoReencher` religa a água e zera os cliques;
  - o `PrenderEm` usa o `PontoDeCarga` do tronco;
  - a entrega na picape faz o arremesso em arco e encaixa o pneu numa vaga da carroceria, sem `CompleteAnimation` e sem `SetActive(false)`.
- **`VasoInteractable.cs`:** `AoAvancarEsvaziando` baixa a água do pratinho, `AoEsvaziar` a esconde e `AoReencher` a devolve.
- **`GarrafaInteractable.cs`:** igual ao vaso, para a água das garrafas que restam. As garrafas continuam no lugar.
- **`Cacamba.cs`:** passa a ser **só o receptor de pneus**, sem o saco. Fica num filho "Cacamba da Picape" (collider da carroceria) da picape. Tem as 6 vagas de empilhamento e o balanço da suspensão.
- **`KitDoAgente.cs`:**
  - `EsvaziarSaco` passa a ser chamado pela `Lixeira`;
  - o `multiplicadorCarregando` padrão vai para 0,85;
  - o `PontoDeCarga` passa a ser o do tronco;
  - os eventos `OnSacoEsvaziado` e `OnPneuEntregue` continuam.
- **`FluxoDeJogo.cs`:**
  - `TaxaPara(float tempo, float pressao, bool celularAberto)`, em que `pressao` é a do gerenciador e pode passar de 1;
  - `FocosTotal` e `FocosEliminados` passam a vir do gerenciador;
  - a vitória e a derrota não mudam.
- **Migração para o gerenciador**, sem mudar a lógica de cada sistema: `DengueHUD`, `FocoListaHUD`, `RedeSocial`, `HistoriaRedeSocial`, `SomDosFocos` e `FocoProximityGlow`. Cada um troca o laço `FindObjectsByType<Interactable>` e a assinatura por foco por uma referência ao gerenciador (campo serializado, com `FindAnyObjectByType` como alternativa) e pelos eventos únicos.
- **`SomDosFocos.cs`:** volume do zumbido por estágio.
- **`DengueHUD.cs`:** os complementos de estágio e de esvaziado na mira, o texto da picape e da lixeira, o aviso fixo de carregando, o aviso de saco cheio com a lixeira, a recusa `JaVazio` e o "Pneu recolhido — N de T".
- **`HistoriaRedeSocial.cs`:** os posts de previsão, de primeiro esvaziar, de primeiro reenchido, de pneu na picape e de saco na lixeira.
- **`CameraJogador.cs`:** um modo de bob pesado, ligado pelo `FeedbackDeCarga`.
- **`SomDoJogador.cs`:** passo pesado enquanto carrega.
- **`Ferramentas/Blender/gerar_agente.py`:** a ação "Carregando" e um modelo simples de saco (ou um script `gerar_saco.py` separado), reexportados.
- **`Agente_AnimController.controller`:** a camada "Carregando", com `AvatarMask` e parâmetro booleano.
- **`Ferramentas/Audio/gerar_sons.py`** e `ImportadorDeSons`: os 9 sons novos.

### Editor
- **`Assets/Editor/FocosVivosBuilder.cs`:** menu **Dengue > Configurar Focos Vivos e Chuva**, idempotente, que usa a cena do `FluxoDeJogo` achado, como o `KitDoAgenteBuilder`. Ele:
  - cria o `GerenciadorDeFocos`, a `Chuva`, o `ChuvaVisual` e o `EnxameDeMosquitos`, e liga as referências das 6 varreduras;
  - marca `podeEsvaziar` nos vasos, pneus e garrafas, nos prefabs e nas instâncias;
  - põe os 6 pneus em escala real (~0,7 m);
  - cria o filho "Cacamba da Picape" com o `Cacamba` e as 6 vagas;
  - remove as 3 caçambas de entulho;
  - transforma as 3 lixeiras em `Lixeira`, com collider se faltar, e cria as 2 novas;
  - põe o `FeedbackDeCarga` e o `PontoDeCarga` do tronco no Player.
- **`KitDoAgenteBuilder.cs`:** para de criar caçambas de entulho.
- **`BalanceamentoSimulador.cs`:** passa a simular o início sorteado (média de várias sementes), o peso por estágio, a chuva (sem reencher, porque o simulador não usa o atalho), o pneu até a picape à velocidade de carga e o saco até a lixeira mais próxima. Usa o novo `TaxaPara`.

## Verificação

### Testes EditMode (`Assets/Editor/Testes/`, `[Category("ParteB")]`, escritos antes do código)
Os 56 testes da Parte A continuam passando; os que mudam de comportamento (pneu, caçamba) são atualizados.
- **`CicloDeVida`:**
  - transições ovo → larva → mosquito nos tempos certos;
  - o mosquito é o fim;
  - `Zerar` e `Reencher` (volta como larva);
  - pesos.
- **`GerenciadorDeFocos`:**
  - registra focos ativos e inativos;
  - `Total`, `Eliminados` e `Restantes`;
  - repassa o `OnFocoEliminado`;
  - `Pressao()` com pesos, esvaziado e tratado valendo 0;
  - o ciclo não anda antes de a partida começar;
  - `ReencherEsvaziados` só mexe nos esvaziados, não no carregado;
  - o sorteio do início respeita `focosComMosquitoNoInicio`.
- **Esvaziar:**
  - Mãos esvaziam vaso, garrafas e pneu;
  - ralo, bromélia e caixa d'água continuam como antes;
  - foco já vazio dá `JaVazio`;
  - esvaziar não conclui e não dispara `OnCompleted`;
  - com a ação definitiva começada, o atalho não vale;
  - a ferramenta certa resolve um foco esvaziado.
- **Pneu:**
  - o último passo esvazia e o pneu fica no lugar;
  - o passo seguinte pega;
  - entregue na `Cacamba` da picape, o pneu conclui e ocupa uma vaga, e o próximo ocupa a seguinte;
  - largado sobre a carroceria, é entregue;
  - largado e reenchido, a água volta.
- **`Lixeira`:** esvazia o saco, recusa o pneu e fica sem efeito com o saco vazio.
- **`Chuva`:**
  - Seco → Previsão → Chovendo → Seco nos tempos certos;
  - o intervalo fica dentro do sorteio;
  - o `OnComecou` reenche;
  - parada com a partida encerrada.
- **`FluxoDeJogo.TaxaPara`** com a pressão por peso.

### Play Mode
Via `unity command eval_file`, com `runInBackground` ligado só durante o teste e `ControleAtivo = false`.
- O estágio avança e o enxame aparece no mosquito.
- Uma chuva forçada reenche um vaso esvaziado como larva, e saem a previsão e os posts.
- Os 6 pneus vão até a picape, ficam empilhados e a contagem sobe.
- O saco é esvaziado na lixeira.
- Com tudo tratado, a vitória sai.
- O Console não tem erros novos.

**Capturas:** o enxame, a chuva, o pneu nos braços na primeira pessoa, a carroceria cheia e o saco na mão.
**Preview da pose de carregar** (skill `preview-design`), aprovado pelo usuário antes de fechar.

### Balanceamento
Recalibrar `taxaComTodosOsFocos` (e, se preciso, `multiplicadorCarregando`) para os critérios da Parte A:
- 30 s/foco com custo de rota vence, com pico ≤ 90%;
- 35 s/foco com custo de rota vence;
- 40 s/foco com custo de rota perde;
- parado perde entre 2:30 e 3:30.

Os resultados vão para o `contexto do jogo.md`.

### Fica para o usuário
Uma partida real com teclado e mouse, para dizer se a chuva, o ciclo e o ritmo estão bons.

## Regras do projeto a seguir
- Registrar tudo no `contexto do jogo.md`, no Histórico de Alterações, no formato do `AGENTS.md`.
- Valores novos de enum só no fim, porque são serializados como int.
- Ao terminar, deixar aberta só a cena `MenuInicial` (`GuardaDeCenas`).
- Não fazer commit sem pedido do usuário.

## Fora do escopo
- Foco que espalha ou cria focos novos.
- Chuva que acelera o ciclo.
- Estágio na lista (Tab).
- Aviso na HUD de "virou mosquito".
- Ícone flutuante ou seta de destino.
- O simulador modelar o atalho de esvaziar.

## Próximo passo
Revisão desta spec pelo usuário e, depois, a skill `writing-plans` para o plano de implementação.
