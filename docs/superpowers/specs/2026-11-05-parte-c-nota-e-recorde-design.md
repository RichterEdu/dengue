# Foco Zero — Parte C: Nota, estrelas e recorde

Spec da Parte C, com o design aprovado pelo usuário seção por seção em 2026-11-05 (brainstorming).
Unity 6000.5.6f1, URP, Input System 1.20. Base: `main` em `1b8af39` (Partes A e B mescladas, PR #2).

## Context

O Foco Zero é um trabalho de faculdade que precisa prender quem joga por vontade própria e ter um pouco de
rejogabilidade. O prazo total é de 2 a 4 semanas; a conta parte de 2026-10-25, data do roteiro, e é uma suposição.
Roteiro: **A → B → C**.
- **A** (pronta): kit do agente, segurar para agir, ferramenta certa por foco.
- **B** (pronta): focos vivos, chuva que cobra o atalho, entregas com peso.
- **C** (esta spec): uma razão para jogar a segunda partida.
- **D** (fora do prazo): moradores no mapa.

Problema que a C resolve: a partida termina com três estatísticas soltas e nada é guardado. Vencer uma vez
encerra o interesse, porque não há o que melhorar. O motor escolhido é **bater a própria nota**: a tela final diz
qual meta faltou, e a partida seguinte tem um alvo claro.

Estado atual relevante:
- 30 focos fixos na cena (11 pneus). Só o estágio inicial é sorteado.
- No simulador, vencer no ritmo de 30 s por foco leva cerca de 26 min.
- O R recomeça sem a abertura, e o relógio só anda depois da porta do hospital.
- A lista de focos começa aberta e o brilho de proximidade tem raio de 5,5 m.
- A tela final é um texto só (`FluxoDeJogoHUD.MontarEstatisticas`).
- `PlayerPrefs` já guarda as configurações (`ConfiguracoesJogo`, prefixo `config.`).

### Decisões do usuário
- Motor da repetição: **bater a própria nota**.
- Forma da nota: **pontos para o recorde e 3 metas com nome**, mostrando qual faltou.
- Metas: **Resposta rápida, Sem retrabalho e Hospital sob controle**. Todas exigem vencer.
- Focos: **continuam os 30 fixos**. Nada de sorteio de lugares nem de pontos novos. O roteiro previa sortear entre
  cerca de 40 pontos, e isso saiu.
- Muletas: **lista fechada e brilho menor**. A lista continua por tipo.
- Abordagem: **nota no fim, metas à vista**. Sem pontos ao vivo na HUD.

## Jogabilidade

### Metas

As 3 estrelas exigem vencer. Na derrota não há estrelas.

| Meta | Cumprida quando | Perdida quando |
|---|---|---|
| Resposta rápida | Vence em até 20:00 | O relógio passa de 20:00 |
| Sem retrabalho | Nenhum foco foi reenchido pela chuva | A chuva reenche o primeiro foco |
| Hospital sob controle | A ocupação nunca passa de 85% | O pico passa de 85% |

- Esvaziar não derruba "Sem retrabalho". Só o reenchimento derruba: quem esvazia e elimina antes da chuva cumpre a meta.
- As comparações usam os mesmos valores inteiros que a tela mostra, para o número exibido nunca contradizer a
  estrela. O tempo é o segundo inteiro (`FloorToInt`) e o pico é o percentual arredondado (`RoundToInt(pico × 100)`).
  Assim, 20:00,9 ainda cumpre, 20:01 não; 85,4% cumpre, 85,5% não.
- Os limites são pontos de partida, ajustáveis, e passam pelo simulador (ver Calibragem).

### Pontos

| Item | Valor | Na derrota |
|---|---|---|
| Foco eliminado | +100 | Vale |
| Vitória | +1.000 | Não vale |
| Tempo | +2 por segundo abaixo de 30:00 (nunca negativo) | Não vale |
| Hospital | +20 × (100 − pico em %) | Não vale |
| Estrela | +500 cada | Não vale |
| Foco reenchido | −100 cada | Vale |

O total nunca fica abaixo de zero.

Exemplos, que entram como testes:
- Vitória, 30 focos, 22:00, pico 84%, 0 reenchidos, 2 estrelas: 3.000 + 1.000 + 960 + 320 + 1.000 = **6.280**.
- Derrota aos 12:00 com 14 focos, 0 reenchidos: **1.400**.

### Recorde

- Guarda a maior pontuação e, em separado, o maior número de estrelas. Os dois podem vir de partidas diferentes.
- É recorde só a nota estritamente maior que a guardada. Nota 0 nunca é recorde.
- Sobrevive a fechar o jogo.
- Só a partida encerrada conta. Reiniciar ou sair pela pausa não grava nada.

### Muletas

- `FocoListaHUD.comecarAberto` passa a falso. A dica "TAB — focos restantes" já existe na HUD.
- `FocoProximityGlow.raioProximidade` cai de 5,5 m para 3,5 m.
- O zumbido e o enxame não mudam.

## Interface

Previews aprovados, gerados no Unity com a HUD real e números de exemplo. As estrelas são sprites, porque a fonte
LiberationSans SDF do jogo não tem ★ nem ☆. Elas preenchem sempre da esquerda para a direita: a fileira mostra a
quantidade, e a lista de metas mostra quais.

### Tela final (`VitoriaCaixa` e `DerrotaCaixa`)

A caixa cresce de 720×400 para 920×630. Saem o `IconeCirculo` e o `Divisor`, que colidiriam com as estrelas. De
cima para baixo:
1. Título atual ("MISSÃO CUMPRIDA!" ou "HOSPITAL EM COLAPSO").
2. 3 estrelas de 76 px.
3. Nota em 68, negrito.
4. Se a nota bateu o recorde, na vitória ou na derrota: "NOVO RECORDE" em dourado. Senão: "Recorde: 6.280". Sem recorde
   guardado e com nota 0: linha vazia.
5. Linha divisória.
6. Coluna esquerda, "METAS": estrela pequena, nome e detalhe por meta, nesta ordem fixa: Resposta rápida, Sem
   retrabalho, Hospital sob controle.
7. Coluna direita, "PONTOS": um item por linha, com o valor à direita. Item com valor zero não aparece, exceto Focos.
8. Rodapé: "Aperte R para jogar de novo".

Detalhes das metas:

| Meta | Cumprida | Perdida (detalhe em vermelho) |
|---|---|---|
| Resposta rápida | `18:30 de 20:00` | `22:00 de 20:00` |
| Sem retrabalho | `0 reenchidos` | `2 reenchidos` |
| Hospital sob controle | `pico 84%` | `pico 91% de 85%` |

Na derrota, as três mostram "exige vencer" em cor fraca, e a coluna de pontos termina com "Faltaram 16 focos".

### Pausa

Cartão novo, "METAS DA PARTIDA", à direita do cartão do menu, no mesmo estilo claro. Fica dentro da `telaMenu`, então
some nas Configurações. O cartão do menu vai 220 px para a esquerda.
- Meta viva: estrela cheia e o andamento (`09:42 de 20:00`, `0 reenchidos`, `pico 78% de 85%`).
- Meta perdida: estrela vazia e detalhe em vermelho (`perdida: 2 reenchidos`).
- Abaixo: "RECORDE", pontos e estrelas. Sem recorde, a linha não aparece.

A tela atualiza ao abrir a pausa, porque o jogo fica parado enquanto ela está aberta. Em janela baixa, o cartão novo
encolhe junto com o do menu (lista `cartoes` do `MenuPausa`).

### Aviso de meta perdida

Faixa de 420×46 centralizada logo abaixo da barra de ocupação: estrela vazia, "META PERDIDA" em vermelho e o nome da
meta. Entra com fade, fica cerca de 3 s e sai. Sem fila: se outra meta cair, o conteúdo é trocado na hora (como no
`NotificacaoCelular`). Não aparece depois da partida encerrada.

### Menu inicial

Na `TelaTitulo`, acima da assinatura: "RECORDE", pontos e estrelas, em tamanho discreto. Sem recorde salvo, não aparece.

## Arquitetura

Separação principal: a conta é pura, o placar observa a partida, o recorde é só armazenamento e a interface apenas lê.

### Arquivos novos (`Assets/Scripts/`)

- **`AvaliacaoDaPartida.cs`** — sem `MonoBehaviour`, testável sem cena.
  - `MetaDaPartida`: enum `RespostaRapida`, `SemRetrabalho`, `HospitalSobControle`.
  - `RegrasDaNota`: `[Serializable]` com `limiteDeSegundos` (1200), `limiteDePico` (85, em %), `pontosPorFoco` (100),
    `pontosPorVitoria` (1000), `tetoDeSegundos` (1800), `pontosPorSegundo` (2), `pontosPorPercentual` (20),
    `pontosPorEstrela` (500), `penalidadePorReenchido` (100).
  - `DadosDaPartida`: `Venceu`, `Segundos`, `Pico` (0 a 1), `Eliminados`, `Total`, `Reenchidos`.
  - `ResultadoDaNota`: `Pontos`, `Estrelas`, a situação de cada meta e o valor de cada item.
  - `Calcular(dados, regras)`, `MetaCumprida(meta, dados, regras)` e `Formatar(pontos)`, que dá `6.280` sem depender
    da cultura do sistema.
- **`RecordeDoJogo.cs`** — sem cena. Lê e grava em `PlayerPrefs`, com o prefixo `recorde.` por padrão e prefixo próprio
  nos testes. Expõe `Pontos`, `Estrelas` e `Registrar(pontos, estrelas)`, que devolve se houve recorde novo e chama
  `PlayerPrefs.Save`. Valor negativo lido vira zero. Não guarda estado em campo estático.
- **`PlacarDaPartida.cs`** — `MonoBehaviour` na cena.
  - Campos: `fluxo` e `gerenciador` (vazios = procura, como nos outros scripts) e `regras`.
  - Assina `OnFocoReenchido`, `OnVitoria` e `OnDerrota`. Só conta com a partida iniciada e não encerrada.
  - `OnMetaPerdida(MetaDaPartida)` dispara uma vez por meta.
  - `Fechar(bool venceu)` calcula e grava o recorde na primeira chamada. As seguintes devolvem o mesmo resultado.
    Quem ouve `OnVitoria` não tem ordem garantida, por isso a HUD chama `Fechar` direto em vez de esperar o placar.
  - Lidos pela interface: `Reenchidos`, `MetaViva(meta)`, `Regras`, `Resultado`, `RecordeNovo`.
  - `Ligar()` e `Conferir()` são `internal`, para os testes no EditMode, onde `Start` e `Update` não rodam.
- **`AvisoDeMeta.cs`** — a faixa. Ouve `OnMetaPerdida`, no `FluxoDeJogoCanvas`.
- **`TelaDaNota.cs`** — um componente em cada caixa final. `Mostrar(ResultadoDaNota, dados, regras, recordeNovo)` preenche
  estrelas, nota, metas e pontos.
- **`CartaoDeMetas.cs`** — o cartão da pausa. `Atualizar()` lê o placar e o recorde.
- **`RecordeNoMenu.cs`** — a linha do menu inicial. Lê `RecordeDoJogo` no `Start`.

### Arquivos alterados

- **`FluxoDeJogo.cs`** — ganha `OcupacaoPico`, que começa em `OcupacaoInicial` e sobe no `Update`. O pico mora aqui porque
  é aqui que a ocupação é calculada.
- **`FluxoDeJogoHUD.cs`** — campos `placar`, `telaVitoria` e `telaDerrota`. Em `HandleVitoria` e `HandleDerrota`, com placar
  e tela ligados, usa `TelaDaNota`. Sem eles, continua com `MontarEstatisticas`, como hoje.
- **`MenuPausa.cs`** — chama `CartaoDeMetas.Atualizar()` em `AtualizarStatus`.
- **`Assets/Editor/BalanceamentoSimulador.cs`** — lê as `RegrasDaNota` do placar da cena e mostra nota e estrelas por
  ritmo, usando `AvaliacaoDaPartida.Calcular`. Como hoje, o simulador não usa o atalho, então os reenchidos são 0.
- **`Assets/Editor/ConfiguracoesGlobais.cs`** — grupo "Nota" na aba Partida (`Tipo = typeof(PlacarDaPartida)`, campos
  `regras.*`). A cena segue como a única fonte desses valores.

### Editor

- **`Assets/Editor/NotaBuilder.cs`** — menu `Dengue > Configurar Nota e Recorde`. Idempotente, como os outros builders:
  - gera os sprites `Estrela Cheia` e `Estrela Vazia` em `Assets/Meus/`;
  - na `SampleScene`: cria o `PlacarDaPartida`, ajusta as caixas finais, monta `TelaDaNota`, o aviso e o cartão da pausa,
    e liga as referências;
  - aplica `comecarAberto = false` e `raioProximidade = 3,5`;
  - na `MenuInicial`: cria a linha de recorde;
  - **não grava** nenhum campo de `RegrasDaNota`, para respeitar o teste `Builders_NaoGravamOsCamposDaJanela`;
  - termina só com a cena `MenuInicial` aberta.

### Fluxo dos dados

1. `SaidaDoHospital` chama `IniciarPartida`, e o relógio começa.
2. A cada quadro, `Conferir()` compara o tempo e o `OcupacaoPico` com as regras. A meta que cai avisa `OnMetaPerdida`.
3. `OnFocoReenchido` soma 1 em `Reenchidos`, e a primeira vez derruba "Sem retrabalho".
4. Em `OnVitoria` ou `OnDerrota`, `Fechar` monta os `DadosDaPartida`, chama `Calcular` e `RecordeDoJogo.Registrar`.
5. `TelaDaNota` mostra o resultado depois do epílogo, sem mudar o `SegurarTelaFinal`.

### Casos de borda

- **Sem placar na cena:** a tela final mostra as estatísticas de hoje, sem erro.
- **Sem gerenciador:** não há reenchidos a contar, e o placar não quebra.
- **Foco reenchido várias vezes:** cada reenchimento custa −100.
- **R antes do fim do epílogo:** o recorde já foi gravado no fim da partida.
- **Duas metas caindo no mesmo quadro:** a segunda substitui a primeira no aviso, e as duas aparecem na pausa.

## Calibragem

Depois de implementar, rodar `Dengue > Simular Balanceamento` e ajustar os limites na cena. Referência: no ritmo de 30 s
por foco, ganhar a estrela do hospital e não ganhar a do tempo. Se os limites mudarem, a entrada do `contexto do jogo.md`
registra os valores finais. Os exemplos dos testes usam regras fixas, então não dependem dessa calibragem.

## Verificação

### Testes EditMode (`Assets/Editor/Testes/`, `[Category("ParteC")]`, escritos antes do código)

- **Nota:** cada item, os exemplos 6.280 e 1.400, piso de zero, derrota sem estrelas, tempo acima do teto sem bônus negativo.
- **Limites:** 20:00 e 20:00,9 cumprem, 20:01 não; 85,4% cumpre, 85,5% não.
- **Placar:** cada meta avisa uma vez; `Fechar` duas vezes grava uma; esvaziar não derruba "Sem retrabalho"; reencher
  derruba; nada conta antes da partida ou depois do fim.
- **Recorde:** só sobe com nota maior; estrelas independentes dos pontos; nota 0 não é recorde; valor inválido vira zero;
  prefixo de teste não toca nas chaves reais.
- **FluxoDeJogo:** `OcupacaoPico` sobe e não desce quando a ocupação cai.
- **Sem placar:** a HUD final mantém as estatísticas.
- **Formato:** `Formatar(6280)` dá `6.280`, inclusive com cultura diferente.
- **Regressão:** ParteA, ParteB e Config continuam passando.

### Play Mode

- Forçar vitória e derrota e ver a tela final real, com epílogo.
- Ver cair cada meta, com o aviso, e abrir a pausa.
- Conferir o recorde depois do R, depois de voltar ao menu e depois de fechar e reabrir o Play.
- Console sem erros.

### Build

Uma build exportada no fim, para conferir a tela final, o recorde gravado e a pendência da Parte B (névoa da chuva e
destaque da mira).

### Fica para o usuário

Sons e sensação de jogo, além de confirmar se as metas parecem alcançáveis jogando.

## Ordem de corte se o prazo apertar

1. Grupo "Nota" na janela Configurações Globais.
2. Recorde no menu inicial (continua na tela final e na pausa).
3. Cartão de metas na pausa.

Nota, estrelas, tela final e aviso ficam até o fim.

## Regras do projeto a seguir

- Registrar toda mudança no fim de `contexto do jogo.md`.
- Sem commit sem pedido do usuário.
- Conferir `EditorApplication.isPlaying` antes de recompilar.
- Terminar só com a cena `MenuInicial` aberta.
- Trabalhar na branch `parte-c/nota-e-recorde`, criada a partir da `main`.

## Fora do escopo

- Pontos novos no mapa e sorteio de lugares.
- Pontos ao vivo na HUD.
- Ranking online e nome do jogador.
- Níveis de dificuldade.
- Moradores no mapa (Parte D).

## Próximo passo

Depois da revisão desta spec pelo usuário, escrever o plano de implementação com a skill `writing-plans`.
