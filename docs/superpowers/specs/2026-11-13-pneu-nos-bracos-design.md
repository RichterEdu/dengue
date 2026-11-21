# Foco Zero — Pneu esvaziado nos braços e bromélia mais fácil

Design curto, 2026-11-13. Base: `main` em `33d2930`, com os minigames da leva 1.

## Por que
O usuário jogou e não gostou de duas coisas:
- **Pneu:** balançar o pneu de pé, onde ele está, não convenceu. Ele quer pegar o pneu primeiro e tirar a água com o pneu nas mãos.
- **Bromélia:** é difícil de tratar. Na revisão dos minigames, o raio de acerto do borrifador caiu para cerca de 0,5° no prefab real, então cada pocinha pede mira com precisão de pixel.

## O que o usuário pediu
- Pegar o pneu com água nos braços, como a tampa da caixa d'água: um clique e ele vem para as mãos.
- Tirar a água **movendo a câmera**, no ritmo da água (a `OndaDoPneu` que já existe).
- **Quanto mais água, mais pesado:** o agente anda mais devagar com o pneu cheio e fica mais leve conforme esvazia.
- Bromélia fácil de tratar.

## Abordagens consideradas
1. **Pegar e depois balançar parado:** um segundo clique imobiliza o agente para balançar. Fica parecido com a tampa, mas para o jogo e não usa o peso.
2. **Pegar já balançando:** um clique pega e começa a sessão. Não dá para carregar o pneu cheio, e o botão direito fica ambíguo.
3. **Balançar andando (escolhida, com o peso do usuário):** o pneu esvazia enquanto o agente anda com ele, movendo a câmera no ritmo, e o peso muda a velocidade. É o mais fluido e a única das três em que "mais leve, mais fácil de andar" faz sentido.

## Como fica
**Pegar:** com as Mãos, um aperto num pneu com água o põe nos braços na hora (`Pegou`), como o pneu vazio hoje. Com outra ferramenta, a recusa continua a mesma.

**Esvaziar nos braços:**
- Enquanto o pneu está nos braços e tem água, a `OndaDoPneu` roda todo quadro, alimentada pelo olhar do jogador.
- Não há sessão nem clique, e o agente não para. *(Decisão minha: o usuário falou em mover a câmera, não em parar.)*
- A onda só cresce no ritmo dela, então olhar em volta normalmente quase não derrama.
- Cada passo da onda é um passo do pneu, e o último o deixa esvaziado (`Esvaziou`), como os 5 passos de hoje.

**Visual:**
- O pneu inclina nas mãos em torno do próprio centro, até `inclinacaoMaxima` (25°), e balança de lado alguns centímetros.
- Derramando, sai o respingo de partículas, como no balanço de hoje.
- A pose dos braços não muda.
- **Água escondida nos braços.** A água de cada pneu da cena é um disco plano deitado. Com o pneu de pé nos braços, o disco ficaria em pé, tapando o furo como uma película. Então, nos braços, o nível aparece pela HUD e pelo respingo, e o disco volta ao largar.

**Peso:**
- O multiplicador de velocidade vai, de forma linear com o nível, de **0,5** (cheio) a **0,85** (vazio, o valor de hoje).
- Corrida e stamina seguem as regras atuais do pneu.
- *(Decisão minha: valores iniciais a calibrar jogando.)*

**Caçamba:** recusa pneu com água, com o aviso "Esvazie o pneu antes: balance a câmera". *(Decisão minha: entregar com água tiraria o sentido de esvaziar.)*

**Largar:** o pneu cai com o nível que tinha, e pegar de novo continua desse nível. A chuva segue a regra de hoje: só reenche o pneu esvaziado e fora dos braços.

**HUD:** a linha do pneu nos braços diz "Pneu com água (60%): balance a câmera para esvaziar" e, vazio, a frase de hoje: leve à caçamba. *(Decisão minha.)*

**No chão:** o pneu perde o minigame de balançar no lugar. O convite ao mirar passa a ser "Clique para pegar o pneu".

**Bromélia:**
- O raio de acerto passa a ser cerca de 2,5° (`raioDeAcerto`), mais ou menos o tamanho de uma pocinha na tela, ainda secando uma por vez.
- De longe, um ponto de mira pode alcançar duas pocinhas vizinhas; isso é aceito.
- Conferir em Play se mirar a bromélia para começar também é fácil.

## Fora do escopo
- Caixa d'água, ralo, vaso, garrafas, piscina e itens na mão.
- Animação nova dos braços para o balanço.
- Sons novos: o derrame usa o que já existe.
- Recalibrar o balanceamento geral: rodar o simulador depois.

## Não pode acontecer
- Andar e mirar com o pneu cheio virar um sofrimento, ou olhar em volta esvaziar o pneu sem querer.
- O pneu ficar inclinado ou com a água fora do lugar ao largar, entregar ou pausar.

## O que fica com quem implementar
- Onde a onda roda (no `PneuInteractable` chamado pelo `Interactor` ou pelo `KitDoAgente`) e como o olhar chega até ela.
- Os testes, reaproveitando o `PneuBalancarTestes`.
- Os textos finais do aviso e da HUD.
