# Foco Zero — Bromélia: pontos de borrifar (bolinhas)

Spec com o design aprovado pelo usuário em 2026-11-14 (brainstorming em chat).
Base: branch `pneu-nos-bracos` em `99bcbad` (12 commits à frente da `main`).

Material de apoio, fora do git, em `.superpowers/sdd/2026-11-14-bromelia-pontos-de-borrifar/`:
- `capturas/aprovado_de_perto_com_borrifador.png`: a referência visual, capturada no Unity;
- `preview_aprovado.cs`: o código que a gerou (pose da câmera, posição do borrifador, ajuste das pocinhas).

A captura mostra só o que a câmera renderiza. A HUD normal do jogo (barra do kit, barra do hospital, celular,
lista de focos) é desenhada por cima e não aparece nela.

## Por quê

O usuário não gostou de nada do minigame atual da bromélia (mirar o borrifador e segurar o clique em cada pocinha)
e pediu para reformular por completo: "um minigame como se fosse o jogo osu, clicar em bolinhas pra concluir".

Histórico que pesa no desenho:
- 2026-11-13: a bromélia "ficou difícil de tratar". O raio de acerto subiu de cerca de 0,5° para 2,5°.
- 2026-11-14: o ritmo saiu do pneu, porque balançar no tempo certo ficou difícil.
- Regra da leva 1 dos minigames: ir mal só perde tempo, não gasta carga nem zera o foco.

## O que o usuário decidiu

Com as palavras dele:
- O minigame é no estilo osu!: bolinhas aparecem e o jogador clica nelas; clicar em todas conclui.
- A câmera vai para perto, por cima da planta ("opção 2" do preview).
- O borrifador continua na tela, e as bolinhas são os pontos onde tem que borrifar.

Propostas minhas que ele leu em resumo e aprovou ("Pode escrever"): os itens 1 a 11 de "Escolhas minhas".

O que ele ainda não viu são os itens 12 a 24 da mesma lista. Eles ficam aprovados se ele aprovar este documento.

## Objetivos

- Tratar uma bromélia cheia é clicar em 5 bolinhas, em cerca de 5 s.
- Funciona com mouse e com controle, pela mesma entrada.
- Ir mal só custa tempo.
- O jogador vê onde a água se acumula: entre as folhas e no copo do meio.

## Fora do escopo

- Música, batida ou qualquer acerto "no tempo".
- Pontuação, combo, "perfeito", sliders ou spinners do osu!.
- Critério novo de nota ou recorde.
- Outros focos, e o painel de minigame para eles.
- Textos educativos, rede social e balanceamento. O tempo previsto da bromélia fica o mesmo.
- Áudio novo. Usa os sons que já existem.

## O que seria um fracasso, mesmo funcionando

- Exigir precisão de pixel ou tempo exato. Foi o que tornou a bromélia e o pneu difíceis.
- Punir o erro: gastar dose, zerar progresso ou encerrar a sessão.
- As bolinhas ficarem longe da água, ou alguma coisa tapar a planta na vista de perto.
- Só dar para jogar com mouse.

## Restrições do projeto

- O mouse fica preso (`Cursor.lockState = Locked`). Toda entrada vem do `EntradaJogador`: `Olhar` (graus por quadro,
  mouse ou analógico direito), interagir (`Attack`) e largar (botão direito, G, botão Norte).
- Minigame de foco segue `Minigame.cs`: `IMinigame.Atualizar(dt, EntradaDoMinigame)`; cada `Passo` vira um
  `AvancarCom(kit)`. `CriarMinigame` é fábrica pura: o `Interactor` também a chama só para perguntar se há minigame
  (com semente 0) e descarta a sessão.
- O passo que conclui encerra a sessão no mesmo quadro (`Interactor.AvancarMinigame` → `EncerrarMinigame`).
- Lógica em classe C# pura, com o tempo entrando por `dt`, testada no EditMode em `Assets/Editor/Testes/`.
- A interface de minigame é montada em código, como a `SkillCheckHUD`. Nenhuma cena é editada.
- O FOV do jogador é configurável no menu de pausa. O enquadramento de perto não pode depender dele.
- `Interactor.Bloqueado` (celular, pausa, cutscene, fim de jogo) encerra a sessão sem punição (`AoBloquear`).
- `agents.md`: toda alteração entra no Histórico de Alterações de `contexto do jogo.md`.

## Abordagens consideradas

1. **Bolinhas espalhadas pela tela, câmera parada.** A tela inteira vira o campo, como no osu!. É a menor e não
   depende de onde a planta está. A planta fica de fundo e as bolinhas não ficam sobre a água.
2. **De perto, por cima da planta, com as bolinhas sobre as pocinhas.** O jogador clica na própria água. As bolinhas
   ficam juntas no miolo e o ponteiro anda pouco. É mais trabalho: câmera que vai e volta, planta ajustada.
3. **Bolinhas no mundo, miradas pelo centro da tela.** Descartada: é o minigame de hoje com clique no lugar de
   segurar. As pocinhas ficam a cerca de 3° uma da outra, e a precisão voltaria.
4. **Bolinhas no ritmo da música.** Descartada: o ritmo já saiu do pneu por ser difícil.

**Escolhida pelo usuário: a 2**, com um acréscimo dele: o borrifador fica na tela e as bolinhas são os pontos onde
borrifar. Eu tinha recomendado a 1 por ser menor; ele preferiu clicar na água.

## Design

### 1. Como se joga

Com a água sanitária selecionada, mirar uma bromélia com água mostra o convite "Clique para borrifar as pocinhas".
Um aperto novo começa a sessão:

1. O agente para. A câmera sai dos olhos e desce até ficar debruçada sobre a planta, em cerca de 0,3 s.
2. O borrifador aparece no canto de baixo, à direita, e uma mira (o ponteiro do jogador) aparece no centro.
3. Surge a primeira bolinha sobre uma pocinha, numerada, com um anel fechando em volta. A bolinha seguinte já
   aparece apagada, para o jogador saber aonde ir depois.
4. O jogador leva a mira até a bolinha da vez e clica. O borrifador, que acompanha a mira, solta uma borrifada;
   a bolinha estoura, a pocinha seca e a próxima acende.
5. Depois do último acerto (o copo do meio), a vista de perto ainda dura cerca de meio segundo, para a última
   borrifada, o estouro e a pocinha secando aparecerem. Só então o foco conclui, como hoje, e a câmera volta.
6. O agente volta a andar, olhar e mirar quando a câmera chega aos olhos.

```
+--------------------------------------------------------------+
|                 (barra do hospital, no topo)                 |
|                      (folhas, vistas de cima)                |
|                 ( 2 )<- anel fechando     * <- estouro da 1  |
|                   o                    .:névoa:.             |
|               ( 3 )  [copo]                  \\              |
|             apagada                           \\  borrifador |
|                        o o o o o               \\___         |
|        Clique nas bolinhas para borrifar | Botão direito: sair|
|                     [ barra do kit ]                         |
+--------------------------------------------------------------+
```

Regras:
- **Uma bolinha por pocinha com água.** Na planta cheia são 5: as 4 de fora, em ordem sorteada, e o copo por último.
- **Acerto:** aperto de interagir com a mira dentro da bolinha da vez. Não existe "cedo demais".
- **Anel:** é só o tempo da bolinha. Se fechar sem acerto, a bolinha some por um instante e volta no mesmo lugar,
  com o anel cheio. Nada se perde além do tempo.
- **Clique fora** (inclusive na bolinha apagada): sai a borrifada, nada mais acontece.
- **Mira:** anda com o mouse ou com o analógico direito e fica presa à tela.
- **Sair:** o botão de largar sai. O que secou fica seco; a próxima sessão traz só o que falta.
- **Bloqueio** (celular, pausa, cutscene, fim de jogo): encerra sem punição, como hoje.
- **Dose:** gasta só ao concluir, como hoje.
- **Nunca erra:** a sessão não devolve `Errou`.
- **Mosquitos:** o enxame desta bromélia fica escondido enquanto durar a vista de perto.

### 2. A sessão (lógica pura)

Arquivo novo, sugestão `Assets/Scripts/PontosDeBorrifar.cs`: classe C# pura que implementa `IMinigame`.

Trabalha num **campo** 2D, que é a tela da vista de perto: `y` vai de −1 (base da tela) a +1 (topo), `x` de
−proporção a +proporção (16:9 dá ±1,78). O centro é (0, 0).

Recebe, na criação:
- os pontos, já na ordem de jogo (posição de cada pocinha no campo);
- `passos` (quantos `Passo` a sessão deve dar);
- o raio de acerto, o tempo do anel, a pausa antes de uma bolinha voltar, a espera inicial (o tempo de a câmera
  chegar), o fecho (o tempo depois do último acerto), a meia-largura do campo e quanto a mira anda por grau.

Comportamento de `Atualizar(dt, entrada)`:
- `dt` inválido (≤ 0 ou NaN): nada muda; só pode sair um `Passo` já devido.
- A mira soma o movimento do jogador × sensibilidade e fica presa ao campo. Valor não finito conta como zero.
- Durante a espera inicial não há bolinha da vez. A mira já anda.
- `Apertou` com a mira a até `raio` da bolinha da vez: acerto. A próxima vira a da vez, com o anel cheio.
- `Apertou` em qualquer outro caso: só registra a borrifada.
- Sem acerto, o anel anda. Em zero, a bolinha fica fora pela pausa e depois volta, com o anel cheio.
- O k-ésimo `Passo` fica devido quando os acertos chegam a `ceil(Total × k / passos)`. No máximo um `Passo` por
  chamada; os devidos saem nas seguintes. Sem pontos, todos os passos já são devidos. (Mesma regra da
  `MiraDoBorrifador`.)
- **Fecho:** os passos devidos pelo **último** acerto só saem depois do tempo de fecho. Nesse intervalo a sessão
  continua ativa, sem bolinha da vez. É o que dá tempo de o último acerto aparecer, já que o passo que conclui
  encerra a sessão no mesmo quadro.

Expõe, para a interface, o borrifador e a bromélia: a mira, o total, os acertos, o índice da vez (ou nenhum), o da
próxima, quanto falta do anel (1 → 0), se cada ponto já foi acertado, se está no fecho, e a última borrifada (onde
foi e se acertou).

`FracaoDoPasso`: acertos desde o último passo sobre os que faltam para o próximo. `PodeSair`: sempre verdadeiro.
`Agindo`: verdadeiro por um instante depois de cada aperto (cerca de 0,2 s), para o som de ação do kit tocar a
borrifada.

### 3. A bromélia

`BromeliaInteractable`:
- `CriarMinigame(kit, semente)` continua fábrica pura e com as mesmas condições de hoje: kit com a ferramenta aceita,
  foco não concluído, alguma pocinha com água e câmera principal. Monta a pose da câmera de perto (item 4), projeta
  cada pocinha com água nessa pose para achar o ponto dela no campo, sorteia a ordem das de fora pela `semente` e
  põe o copo por último. `passos = RequiredClicks − CurrentClicks` (mínimo 1), como hoje.
- `AoIniciarMinigame` guarda a sessão e qual pocinha é cada ponto, e esconde o enxame de mosquitos deste foco.
- `AoAtualizarMinigame` seca a pocinha de cada ponto recém-acertado, com a animação de secar que já existe
  (`duracaoSecar`, 0,3 s). O fecho é mais longo que ela, então o copo termina de secar antes de o foco concluir.
- `OnInteract` com sessão: não seca em ordem fixa; o passo que conclui seca o que tiver sobrado (a garantia de hoje).
- `AoSairDoMinigame`, a recusa no meio (`AvancarCom` diferente de `Avancou`) e o passo que conclui largam a sessão
  e devolvem o enxame. Não há mais pocinha "pela metade" para devolver.
- O caminho antigo (`Interact()` sem kit, ou `AvancarCom` sem sessão) continua secando em ordem fixa.
- Nascer seca, reencher com a chuva e `PodeReencher` não mudam.
- `ConviteDoMinigame`: "Clique para borrifar as pocinhas" (igual).
  `DicaDoMinigame`: "Clique nas bolinhas para borrifar   |   Botão direito: sair".
- `SegundosEstimados` = 0,75 + 0,87 × pocinhas. Com 5 dá 5,1 s (hoje 5,2 s): o balanceamento não muda.
- Saem os campos `segundosPorPoca`, `segundosDoCopoCentral`, `raioDeAcerto`, `coneDaMira` e `sensibilidadeDaMira`.
  Entram os de "Valores iniciais".

`requiredClicks` continua ajustável em Dengue > Configurações Globais. Com passos diferentes do número de pocinhas,
a regra do `ceil` distribui, como hoje. O som de passo da bromélia continua saindo a cada `Passo` (`OnClicado`):
na planta padrão, de 5 passos e 5 pocinhas, isso é a cada acerto.

### 4. A câmera de perto

A câmera hoje é posta todo quadro pelo `CameraJogador`, a partir do pescoço do agente. Ela ganha um
**enquadramento temporário**: alguém pede uma pose (posição, giro e FOV) e ela vai até lá suavemente; ao soltar,
volta aos olhos.

Pose pedida pela bromélia, com `copo` = centro do copo (a última de `pocas`) e `frente` = direção horizontal do
agente para a planta:
- posição = `copo` + para cima × altura − `frente` × recuo;
- giro = olhar para o `copo`, com o "para cima" da tela apontando para `frente`. A base da tela fica do lado do
  agente, então o movimento de ida parece um debruçar;
- FOV fixo, só enquanto durar.

Quem pede e solta é o `Interactor`. A sessão ou o foco diz que quer a vista de perto e qual é a pose; sugestão: uma
interface `IMinigameDePerto` na sessão, no padrão da `IMinigameComMira` de hoje.

Requisitos:
- Ida em cerca de 0,3 s e volta em cerca de 0,25 s, suaves.
- **Toda saída solta o enquadramento:** concluir, sair, bloqueio, recusa no meio da sessão e alvo destruído ou
  desativado.
- **Enquanto a câmera volta, o agente continua parado e o `Interactor` não mira nem interage.** Hoje o
  `EncerrarMinigame` libera o jogador no mesmo quadro e o raio da mira sai da `Camera.main`, que ainda estaria
  sobre a planta, olhando para baixo.
- Bloqueio com o jogo pausado (`timeScale` 0): ao fechar o menu, a câmera já está nos olhos e o agente, livre.
- Durante a vista de perto não há balanço de passos nem FOV de corrida.
- A câmera não pode ficar dentro de um teto ou objeto acima da planta.

### 5. A interface das bolinhas

Componente novo no Player, sugestão `PontosDeBorrifarHUD`, no molde da `SkillCheckHUD`: monta o próprio Canvas em
código (overlay, referência 1920×1080), gera os sprites de disco e anel uma vez, e é adicionado pelo `Interactor`
no `Start` se não existir. Lê a sessão por `Interactor.MinigameAtivo`.

Desenha, por cima da cena:
- a **bolinha da vez**: disco verde-escuro, borda branca, número branco, brilho verde em volta e o anel branco, que
  vai de cerca de 2,2× o tamanho da bolinha até encostar nela;
- a **próxima**: igual, a meia transparência, sem anel fechando;
- o **estouro** do acerto: anel verde que cresce e some, com gotinhas;
- a **mira**: anel branco pequeno com miolo amarelo;
- a **contagem**: um ponto por bolinha, aceso ao acertar, como no anel do ralo;
- a **dica**, numa faixa escura.

O campo vira tela assim: `y = ±1` é a borda de cima e a de baixo. As bolinhas só aparecem quando a câmera chega
(a espera inicial da sessão). Animações em tempo sem escala, como na `SkillCheckHUD`.

**Onde ficam a contagem e a dica:** logo acima da barra do kit, sem cobrir bolinha. A barra do kit continua na
tela: fica no centro de baixo, a 40 px da borda, com 96 px de altura (`KitHUD`). Na captura a dica está mais
embaixo, porque a barra do kit não aparece nela.

Cores: o verde `(0,25; 1; 0,8)` e o escuro `(0,04; 0,07; 0,10; 0,72)` da `SkillCheckHUD`.

Com a sessão ativa, a `DengueHUD` esconde a mira normal e o painel do alvo, como já faz no skill check do ralo.
O resto da HUD (barra do hospital, barra do kit, celular, lista de focos, avisos) fica como está.

### 6. O borrifador de perto

O borrifador de verdade fica na mão direita do corpo do agente, que não aparece na vista de perto. Para ele
continuar na tela, um componente novo (sugestão `BorrifadorDePerto`) mostra uma cópia do mesmo modelo presa à
câmera, só durante a vista de perto:
- entra pelo canto de baixo, à direita: aparecem a cabeça com o gatilho e o gargalo; o resto sai da tela, então a
  falta da mão não aparece;
- vira para a mira, como ela aparece na tela, com cerca de 30° de mergulho para dentro. De perfil ele é
  reconhecível; apontado de verdade para a planta, seria visto por trás;
- a cada aperto solta uma borrifada curta de névoa na direção da mira, com gotas menores que as da névoa da mão;
- fica entre a câmera e as folhas (até cerca de 0,36 m da câmera), para não entrar na planta;
- **não projeta sombra**: a sombra de um frasco flutuando sobre a planta denunciaria a falta da mão.

A cópia pode ser criada em tempo de execução a partir do `BorrifadorNaMao` que o `ItensNaMao` já referencia, sem
editar cena nem prefab. Ela herda o filho `Nevoa` do original; a borrifada de perto usa partículas próprias ou
esse filho reconfigurado, nunca as duas.

**O borrifador da mão fica quieto** durante a vista de perto: sem o gesto de usar e sem a névoa do bico. Sem isso,
como o `Avancando` segue o `Agindo`, o `ItensNaMao` soltaria uma segunda névoa, vinda do corpo, a cada aperto.

### 7. A planta

Vistas de cima, as 4 pocinhas de fora ficam hoje tapadas pelas folhas; só o copo aparece. Elas sobem e crescem um
pouco para aparecerem na vista de perto. No preview: 4,5 cm mais altas no mundo (0,03 no modelo) e 1,5× maiores.

Onde elas são definidas: `Ferramentas/Blender/gerar_focos.py` (`poca('Agua_%d', ..., BASE_Z + 0.05, 0.018)`, quatro
de cinco posições a 72° uma da outra, a 0,055 do centro), que gera o `Bromelia.fbx`; o menu Dengue > Criar Bromélia
(`FocosBlenderBuilder.CriarBromelia`) monta o `Bromelia.prefab`. As 4 bromélias da `SampleScene` são instâncias
desse prefab, com escala 1,51.

### 8. O que mais muda e o que sai

- `Interactor`: pede e solta o enquadramento; segura o agente e a mira até a câmera voltar; adiciona a interface
  nova e o borrifador de perto no `Start`.
- `EntradaDoMinigame`: com "inverter Y" ligado nas configurações, a mira não pode inverter. `Olhar` já chega
  invertido, então a mira precisa do movimento sem a inversão.
- `DengueHUD`: esconde a mira normal e o painel do alvo durante esta sessão.
- `ItensNaMao`: sem gesto e sem névoa da mão durante a vista de perto.
- **Sai**, por ficar sem uso (como a `OndaDoPneu` saiu): `MiraDoBorrifador.cs`, `MiraDoBorrifadorTestes.cs`, a
  interface `IMinigameComMira`, o trecho do `Interactor` que aplica a mira no olhar (`yawInicial`, `pitchInicial`,
  `DefinirOlhar` no minigame) e o `MinigameComMiraFalso` dos testes.
- Cabeçalhos de `Minigame.cs`, `Interactor.cs` e `BromeliaInteractable.cs` passam a descrever o que existe.

### 9. Valores iniciais

No Inspector da bromélia, com padrão no código (nenhum prefab ou cena grava o valor):

| Valor | Inicial | O que é |
|---|---|---|
| altura da câmera | 0,58 m | acima do copo, no mundo |
| recuo da câmera | 0,17 m | para o lado do agente |
| FOV de perto | 40° | fixo durante a sessão |
| raio da bolinha | 0,14 do campo | 150 px de diâmetro em 1080p; o acerto usa o mesmo raio |
| tempo do anel | 1,5 s | para o anel fechar |
| pausa da bolinha | 0,35 s | fora da tela depois de o anel fechar |
| espera inicial | 0,3 s | igual à ida da câmera |
| fecho | 0,45 s | depois do último acerto, antes de concluir |
| mira por grau | 0,0093 do campo | 1 px de mouse ≈ 1 px em 1080p, com a sensibilidade da cena |

A sensibilidade do mouse na `SampleScene` é 0,2° por pixel (o padrão do código é 0,15). O multiplicador do menu de
pausa entra por cima, como no olhar.

Com esses valores, as pocinhas de fora ficam a cerca de 0,37 do campo do centro (uns 200 px em 1080p) e as vizinhas
a cerca de 0,43 uma da outra (uns 230 px). As bolinhas de 150 px não se sobrepõem.

## Testes

EditMode, categoria `Minigames`, em `Assets/Editor/Testes/`:

- `PontosDeBorrifarTestes` (novo): a mira anda com o movimento e fica presa ao campo; clique dentro acerta e passa
  para a próxima; clique fora não conta; clique na bolinha apagada não conta; nada acerta durante a espera inicial;
  o anel que fecha tira a bolinha e ela volta, sem perder acertos e sem `Errou`; passos nas contagens certas; no
  máximo um `Passo` por chamada; mais passos que pontos; sem pontos, todos os passos saem; o passo do último acerto
  só sai depois do fecho, e os anteriores saem na hora; `dt` inválido não muda nada; movimento não finito conta
  como parado; `Agindo` só logo depois de um aperto; argumentos ruins viram padrão.
- `BromeliaBorrifarTestes` (reescrito): cria a sessão com as pocinhas com água, com o copo por último; a mesma
  semente dá a mesma ordem; o copo cai no centro do campo; a pose fica acima do copo, com a base da tela do lado do
  agente; o acerto seca a pocinha certa; o último passo seca o que sobrou; sair guarda as secas; retomar usa só o
  que falta; a dose só sai no fim; convite e dica; `SegundosEstimados`; sem ferramenta, sem câmera ou nascida seca
  não há minigame; o caminho antigo mantém a ordem fixa; o enxame some ao começar e volta em cada saída.
- `MinigameNoInteractorTestes`: sai o teste da mira; entram "sessão de perto pede o enquadramento ao começar",
  "solta ao concluir, ao sair, ao bloquear, na recusa no meio e com o alvo desativado" e "o agente fica parado e
  sem mira até a câmera voltar".
- Câmera: enquadrada, chega à pose pedida; solta, volta aos olhos; solta com `timeScale` 0, volta.
- `ItensNaMaoTestes`: sem gesto e sem névoa da mão durante a vista de perto.
- Sai `MiraDoBorrifadorTestes`.
- A suíte inteira continua passando (532 testes em `99bcbad`, antes desta mudança).

## Verificação no jogo

Jogar no Editor, com mouse e com controle:
- as 4 bromélias da cena, uma delas mais alta que as outras (y = 1,17, perto da "Coffe house"): nada tapa a vista
  de perto;
- a dica e a contagem não cobrem bolinha nem a barra do kit; o borrifador não some atrás do ícone do celular nem
  da lista de focos;
- o último acerto aparece inteiro antes de a câmera voltar;
- sair no meio e retomar; celular e pausa no meio; fim de jogo no meio;
- chuva durante a sessão;
- bromélia no estágio de mosquito: sem mosquitos na frente da câmera, e eles voltam depois;
- "inverter Y" ligado; FOV do menu no mínimo e no máximo;
- o som de cada borrifada e o de cada passo.

## Escolhas minhas

**Lidas e aprovadas pelo usuário, em resumo:**

1. A câmera leva cerca de 0,3 s para chegar e volta ao terminar ou sair.
2. O borrifador entra pelo canto de baixo, à direita, e vira para a mira. Cada clique solta uma borrifada.
3. Uma bolinha por pocinha com água; de fora em ordem sorteada, o copo por último; a próxima aparece apagada.
4. Não existe "cedo demais"; o anel é só o tempo, e a bolinha volta se ele fechar.
5. Errar ou clicar fora só custa tempo.
6. A mira é desenhada pelo jogo e anda com o mouse ou com o analógico direito; o cursor do sistema continua preso.
7. Botão direito sai e guarda o que secou. Celular e fim de jogo encerram sem punição.
8. As 4 pocinhas de fora sobem para aparecerem de cima.
9. Cerca de 5 s por bromélia.
10. O minigame atual de mirar sai do código.
11. A aparência das bolinhas, da mira e do borrifador é a da captura aprovada.

**Ainda não vistas por ele:**

12. Depois do último acerto, a vista de perto dura mais 0,45 s antes de concluir.
13. O agente só volta a andar e mirar quando a câmera chega aos olhos.
14. O enxame de mosquitos da bromélia fica escondido durante a vista de perto. Ele é uma esfera de 0,9 m centrada
    1 m acima do pivô da planta: a câmera de perto ficaria dentro dele.
15. O FOV de perto é fixo (40°), para o enquadramento não depender do FOV do menu.
16. O borrifador de perto é uma cópia presa à câmera, sem mão e sem sombra, e aponta "como na tela", não para o
    ponto real.
17. O borrifador da mão fica quieto durante a vista de perto.
18. A contagem e a dica ficam acima da barra do kit, que continua na tela.
19. A interface `IMinigameComMira` e o trecho do `Interactor` que a usa saem junto com o minigame antigo.
20. Os sons são os que já existem: o som de ação do kit na borrifada e o som de passo da bromélia a cada passo.
21. O convite ao mirar fica igual; só a dica da sessão muda.
22. Com "inverter Y" ligado, a mira não inverte.
23. O brilho pulsante do alvo mirado continua durante a sessão, como hoje.
24. Os valores da tabela.

## O que fica para quem constrói

- Nomes de classes, arquivos e campos (os daqui são sugestões).
- Como a câmera faz a transição (curva, tempo com ou sem escala) e a forma da API do enquadramento.
- Onde mora o "voltando": quem segura o agente e a mira até a câmera chegar.
- O que fazer se algo estiver acima da planta: conferir as 4 da cena primeiro. Se precisar, baixar a câmera e
  abrir o FOV na mesma proporção mantém o enquadramento.
- Onde subir as pocinhas: no `gerar_focos.py` (precisa do Blender), no `FocosBlenderBuilder` ou direto no prefab.
  Cuidado: rodar Dengue > Criar Bromélia de novo sobrescreve o prefab, e outros builders já mexeram nele (a
  ferramenta necessária, por exemplo). O que for escolhido precisa valer também numa regeneração futura.
- Como a mira recebe o movimento sem o "inverter Y".
- Como esconder e devolver o enxame (ele é criado pelo `EnxameDeMosquitos` como filho "Enxame" do foco).
- Como o `ItensNaMao` fica sabendo da vista de perto.
- Como o som de ação do kit soa em pulsos curtos (é um loop com fade de 0,15 s). Se soar errado, usar outro som
  que já exista; não criar áudio.
- Se o brilho pulsante atrapalhar de perto, desligar só na sessão.
- A posição exata da contagem, da dica e do borrifador na tela.
- A ordem e o tamanho das tarefas do plano.

## Registro

Toda alteração entra no Histórico de Alterações de `contexto do jogo.md` (`agents.md`). Esse arquivo não tem outra
seção que descreva o minigame da bromélia: basta a entrada no Histórico.
