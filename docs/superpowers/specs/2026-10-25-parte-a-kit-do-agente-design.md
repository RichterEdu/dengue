# Foco Zero — Parte A: Kit do agente + ação certa para cada foco

Spec da Parte A, aprovada pelo usuário em 2026-10-25 (brainstorming).
Unity 6000.5.6f1, URP, Input System 1.20.

## Context

O usuário quer que o Foco Zero vire "um jogo interessante mesmo", feito por partes. É trabalho de
faculdade, mas precisa prender quem joga por vontade própria e ter um pouco de rejogabilidade. O prazo é de 2 a 4 semanas.

A casca já está pronta: menu, abertura, hospital, história pelo celular, epílogo, sons, pausa e
balanceamento. O problema é o loop central. Hoje, os 23 focos se resolvem do mesmo jeito: mirar e clicar de 3 a 5 vezes
(`Interactable.requiredClicks`). Não há decisão, variedade nem desafio de busca.

Roteiro decidido com o usuário: **A → B → C**.
- **A — Kit do agente + ação certa para cada foco** (esta spec)
- **B — Focos vivos + chuva**: ovo → larva → mosquito; a chuva reenche o que só foi esvaziado. Brainstorm próprio depois da A.
- **C — Rejogabilidade**: focos sorteados entre ~40 pontos, pontuação/estrelas, recorde, muletas mais leves. Brainstorm próprio.
- Fica fora do prazo: **D — Moradores no mapa**.

Distribuição na cena: 6 pneus, 4 ralos, 4 bromélias, 3 vasos, 3 grupos de garrafas, 2 caixas d'água e 1 piscina.

## Jogabilidade

- **Barra de ferramentas** na parte de baixo da tela, com 5 espaços. Troca pelas teclas 1–5, pela roda do mouse ou por LB/RB:

  | # | Ferramenta | Foco | Carga (estoque "apertado", ajustável no Inspector) |
  |---|---|---|---|
  | 1 | Mãos | caixa d'água, piscina, pegar pneu | sempre |
  | 2 | Areia | pratinho do vaso | 1 |
  | 3 | Tela | ralo | 1 |
  | 4 | Água sanitária diluída | bromélia | 2 doses |
  | 5 | Saco de lixo | garrafas | 6 garrafas por saco |

- **Segurar para agir.** O tempo varia por tipo, entre 1,5 e 3 s. Ao soltar, o progresso fica guardado. A carga só é gasta quando o foco termina.
- **A mira mostra o problema, não a solução.** Por exemplo: "Ralo destampado com água parada". Com a ferramenta errada nada acontece, e a HUD mostra o porquê e a ferramenta correta. Exemplo: "Areia entope o ralo. Ralo se protege com tela."
- **Pneu.** Com as Mãos, segurar esvazia a água e o pneu vai para os braços, na frente da câmera. Enquanto carrega, o agente anda mais devagar, não corre, não pula e não usa ferramentas. O foco só é eliminado quando o pneu é entregue na **caçamba**. O botão direito larga o pneu. Largado, ele continua sendo foco e pode ser pego de novo com as Mãos, na hora.
- **Garrafas.** Segurar recolhe uma garrafa por vez para o saco. Com o saco cheio, é preciso esvaziar na caçamba. O grupo é eliminado quando a última garrafa é recolhida.
- **Reabastecimento:**
  - **picape da UPA**, perto de `SaidaDoHospital.posicaoNoBairro`: recarrega areia, tela e água sanitária;
  - **caçamba**, no terreno baldio, perto dos pneus: recebe o pneu e esvazia o saco;
  - **um monte de areia** do terreno baldio: recarrega só a areia.
- **Tutorial.** A fala da Rita menciona o kit e a picape. A dica "1–5 troca a ferramenta" segue o modelo do `DicaCorrer`.

## Arquitetura

Princípio: o "segurar" vira **passos**. A cada `segundosParaConcluir / requiredClicks` segurando, o código chama o
`Interact()` que já existe. Assim as animações de cada foco, os sons por clique (`OnClicado`), o post de "largou pela metade"
e o `FluxoDeJogo` continuam funcionando.

### Arquivos novos (`Assets/Scripts/`)
- `Ferramenta.cs`: enum com `Maos`, `Areia`, `Tela`, `AguaSanitaria` e `SacoDeLixo`.
- `EstoqueDoKit.cs`: classe C# pura (testável) com:
  - capacidade e quantidade por ferramenta;
  - `TemCarga`, `Consumir` e `Reabastecer(conjunto)`;
  - o saco: `GarrafasNoSaco`, `SacoCheio`, `GuardarGarrafa` e `EsvaziarSaco`.
- `KitDoAgente.cs`: fica no Player. Guarda o estoque, a ferramenta `Selecionada`, `Selecionar`, `Proxima/Anterior` e o `PneuCarregado`, e tem o evento `OnMudou`.
- `PontoDeUso.cs`: base abstrata (MonoBehaviour) para coisas usáveis. **Não herda `Interactable`**, porque `FluxoDeJogo`, `DengueHUD`,
  `FocoListaHUD` e `RedeSocial` varrem `Interactable` como foco. Tem `textoAoMirar`, `segundosParaUsar`, `PodeUsar(kit, out motivo)` e `Usar(kit)`. Duas filhas:
  - `PontoDeReabastecimento.cs`: lista de ferramentas que recarrega;
  - `Cacamba.cs`: entrega o pneu carregado e esvazia o saco.
- `KitHUD.cs`: a barra de ferramentas no estilo da HUD atual, com a quantidade e o destaque da ferramenta selecionada.

### Arquivos alterados
- `Interactable.cs`:
  - campos `ferramentaNecessaria` e `segundosParaConcluir`;
  - `AvancarCom(KitDoAgente kit)`, que confere a ferramenta e a carga, chama `Interact()` e consome ao concluir;
  - `virtual bool AceitaFerramenta(f)`, `virtual bool ConcluiSozinho` (padrão `true`) e `protected void Concluir()`.
  - **`IsComplete` passa a ser uma flag setada só na conclusão real.** Sem isso, um pneu carregado sumiria da lista e da contagem antes de chegar à caçamba.
- `Interactor.cs`:
  - troca o `wasPressedThisFrame` por "segurando": acumula tempo no alvo e avança os passos;
  - o raycast também acha `PontoDeUso`;
  - expõe `ProgressoContinuo` e os eventos `OnFerramentaErrada(foco)` e `OnSemCarga(ferramenta)`;
  - o botão direito ("Largar") larga o pneu. Enquanto carrega pneu, só a caçamba e o largar funcionam.
- `PneuInteractable.cs`:
  - `ConcluiSozinho = false`;
  - ao terminar a ação, a água some, o pneu vai para o `PontoDeCarga` (filho da câmera) e os colisores desligam;
  - `Largar()` assenta o pneu no chão por raycast;
  - `Entregar()` chama `Concluir()` e encolhe na caçamba.
- `GarrafaInteractable.cs`: cada passo exige saco com espaço e chama `kit.GuardarGarrafa()`.
- `ControladorJogador.cs`: `MultiplicadorVelocidade` e `PodeCorrerEPular`.
- `EntradaJogador.cs` + `Assets/InputSystem_Actions.inputactions`:
  - `InteragirSegurado` (`IsPressed`);
  - teclas 1–5;
  - `Player/TrocarFerramenta` (roda / LB-RB);
  - `Player/Largar` (botão direito / botão do controle sem conflito com T e Esc).
- `DengueHUD.cs` + `FocoHUDInfo.cs`:
  - barra contínua;
  - texto padrão "Segure para…";
  - campo novo `textoFerramentaErrada`;
  - avisos de sem carga e saco cheio.
- `HistoriaRedeSocial.cs`: 4 posts que saem uma vez cada — primeira ferramenta errada (do dono do tipo), primeiro reabastecimento (UPA), carregando pneu (Carlos) e primeiro saco na caçamba (Bruna).
- Sons em `Ferramentas/Audio/gerar_sons.py` + `ImportadorDeSons`/`SomBuilder`: trocar ferramenta, loop de ação por ferramenta, erro, reabastecer, despejar e pegar/largar pneu.
- `Assets/Editor/PersonagensHospitalBuilder.cs`: `FalaEnfermeira` menciona o kit.
- `Assets/Editor/BalanceamentoSimulador.cs`: soma, com as distâncias reais da cena, o tempo de ação por tipo, o caminho pneu → caçamba e as voltas ao carro e à caçamba conforme o estoque. Depois recalibra o `FluxoDeJogo` para o alvo "desafio moderado" de 27/09.

### Montagem da cena
`Assets/Editor/KitDoAgenteBuilder.cs`, menu **Dengue > Configurar Kit do Agente**, que pode ser rodado de novo. Ele:
- põe `KitDoAgente` e `PontoDeCarga` no Player;
- cria a `KitHUD` e a dica 1–5;
- posiciona a picape (`Palmov Island/.../Vehicles/car SUVs pickups lights red.prefab`) + `PontoDeReabastecimento`;
- gera a caçamba (malha simples) no terreno baldio + `Cacamba`;
- marca um `Monte de Areia` como reabastecimento só de areia;
- preenche `ferramentaNecessaria`, `segundosParaConcluir` e os textos de `FocoHUDInfo` nos prefabs (Vaso, Ralo, Bromélia, Caixa d'Água, Piscina) e nas instâncias da cena (pneus, garrafas).

### Reaproveitar
- as animações de progresso que já existem (`CaixaDaguaInteractable.MoverTampa`, `VasoInteractable.SetSandFill`, `RaloInteractable`, `BromeliaInteractable.Secar`, `GarrafaInteractable`);
- o destaque de mira do `Interactor`;
- o padrão de dica do `DicaCorrer`;
- `FocoHUDInfo` para os textos por foco (não editar o `switch` do `DengueHUD`);
- o pipeline de sons por script.

## Verificação
- **EditMode, test-first** (Unity Test Framework 1.7 já instalado), em `Assets/Editor/Testes/` (Assembly-CSharp-Editor):
  - estoque: capacidade, consumo, reabastecimento parcial, saco cheio/esvaziar;
  - tempo segurado → número de passos;
  - aceita/recusa ferramenta, e carga gasta só ao concluir;
  - o pneu não conclui até `Entregar()`, e `IsComplete` continua falso enquanto ele é carregado;
  - `PontoDeUso` não aparece na varredura de `Interactable`.
- **Play Mode**, como nas sessões anteriores (`unity command eval_file` + screenshot, `runInBackground` ligado só durante o teste):
  - cada tipo conclui com a ferramenta certa, e a errada não avança;
  - o estoque zera e a picape e o monte de areia reabastecem;
  - pneu → caçamba aumenta `FocosEliminados`;
  - saco cheio bloqueia e a caçamba esvazia;
  - vitória com os 23;
  - nenhum erro novo no Console.
- Simulador de balanceamento rodado e resultados registrados.
- Preview (skill `preview-design`) do pneu carregado e da barra de ferramentas antes de fechar o visual.
- **Fica para o usuário:** uma partida com teclado e mouse de verdade, porque o Editor sem foco descarta a entrada.

## Regras do projeto a seguir
- Registrar tudo no `contexto do jogo.md` → Histórico de Alterações, no formato do `agents.md`.
- Ao terminar, deixar aberta só a cena `MenuInicial` (`GuardaDeCenas`).
- Nada de commit sem pedido do usuário.

## Próximo passo
Plano de implementação passo a passo (skill `writing-plans`), com TDD.
