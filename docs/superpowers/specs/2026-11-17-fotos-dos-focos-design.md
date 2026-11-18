# Foco Zero — Fotos dos focos na rede social

Spec com o design combinado com o usuário em 2026-11-17 (brainstorming em chat).
Base: branch `bromelia-pontos-de-borrifar` em `8cbedb3`.

Material de apoio, fora do git, em `.superpowers/sdd/2026-11-17-fotos-dos-focos/`:
- `aprovadas/`: as **31 fotos aprovadas pelo usuário** (`NN - Tipo - Nome.png`, 720×480), o `poses.tsv` com a pose
  da câmera de cada uma (a mesma tabela do Apêndice) e `folhas/aprovadas 1.png` e `2.png` (folhas de contato);
- `capturas/`: o preview do celular aprovado (`1 - celular aberto, post com foto.png`,
  `2 - celular aberto, post resolvido.png`, `3 - celular fechado, notificacao com foto.png`), as folhas das rodadas
  de escolha e, em `capturas/scripts/`, os scripts de `eval_file` que geraram tudo (`fotos_v2_poses.cs`,
  `fotos_v3_refazer.cs`, `preview_capturas.cs`...).

As capturas do celular renderizam a Main Camera com os canvases trocados para Screen Space Camera; o "Ocupação 6%"
da captura 3 não é real (a abertura foi pulada no preview).

## Por quê

Pedido do usuário: "o jogador novo vai entrar no jogo e não vai saber onde tá cada foco, por isso quero que com o
passar do tempo alguém faça um post na rede social com a foto do foco, e com a foto o jogador vai conseguir
identificar onde está o foco".

Hoje o jogador acha os focos pelo zumbido (curto alcance), pelo brilho de proximidade (curtíssimo) e pela lista do
Tab (só conta por tipo, não diz onde). Nada ajuda quem está perdido longe dos focos. A cena tem **31 focos**
espalhados: 4 bromélias, 2 caixas d'água em telhados, 7 garrafas/latas, 10 pneus (6 deles num monte), 4 ralos e
4 vasos.

## O que o usuário decidiu

- **Quando:** a foto sai **quando o jogador empaca** (muito tempo sem eliminar foco), no lugar da bronca de hoje.
  Ele escolheu isso entre "ritmo fixo", "quando empacar" e "os dois".
- **Como:** **fotos prontas**, geradas no Editor (não tiradas em tempo de jogo), **cada uma ligada a um foco
  específico**.
- **Aprovação:** "preciso que me mostre as fotos antes de deixar no jogo". Já aconteceu: duas rodadas de folhas de
  contato, 13 fotos refeitas com 3 opções cada; as 31 escolhidas estão em `aprovadas/` (Apêndice). Ele aprovou
  também o visual do celular ("gostei de como ficou no celular, ótimo").
- Propostas minhas que ele leu e aprovou ("Está certo"): o relógio da bronca (60 s, ajustável, parado com o
  celular aberto); outra foto a cada novo intervalo se ele continuar empacado; o foco **mais perto** do jogador
  que ainda não saiu em foto; focos colados num que já saiu ficam de fora (o monte de pneus não sai 6 vezes);
  quem posta é o **dono do tipo**, com fala curta e **sem endereço** (a foto mostra o lugar); a foto mostra o foco
  com o entorno; o selo **Resolvido** quando o foco da foto é eliminado; a notificação com **miniatura** e
  "enviou uma foto".
- Visual aprovado no preview: a foto dentro do card, com o **círculo vermelho** "à mão" em volta do foco; o card
  resolvido com a foto escurecida e o selo verde **RESOLVIDO**; a notificação mais larga (500 px) com a miniatura
  à direita.

## Objetivos

1. Quando o jogador passa `tempoSemFoco` (60 s) sem eliminar foco, sai um post com a foto de um foco perto dele,
   feito pelo personagem dono daquele tipo de foco.
2. Cada foco da cena tem a sua foto pronta, ligada a ele pelo próprio objeto (não pelo nome), e a pose da câmera
   que a gerou, para dar para refazer.
3. As fotos do jogo são exatamente as 31 aprovadas.
4. O card mostra a foto; a notificação mostra a miniatura; o foco eliminado carimba o card com "Resolvido".
5. Dá para refazer uma foto no Editor: mover a câmera (pela vista da Scene) e gerar de novo.

## Fora do escopo

- Foto tirada em tempo de jogo, câmera ao vivo, mapa, seta ou endereço escrito.
- Ampliar a foto (clique/zoom): com o celular aberto o mouse continua girando a câmera.
- Mudar os outros gatilhos do narrador (chegar perto, combos, chuva...).
- Fotos em ritmo fixo (sem estar empacado).
- Gerar ponto de câmera automático no Editor para focos futuros (os scripts do preview ficam guardados para isso;
  ver "O que fica para quem constrói").

## O que seria um fracasso, mesmo funcionando

- Uma foto no jogo diferente da que o usuário aprovou.
- Foto ligada ao foco errado (há dois objetos chamados `pneu (5)` na cena).
- Mais poluição na tela: a foto **substitui** a bronca, não soma a ela.
- Foto de um foco que o jogador já achou (já mexeu nele) ou já eliminou.
- O card com foto quebrando o layout do feed, ou o selo aparecendo no card errado.

## Restrições do projeto

- `AGENTS.md`: toda mudança vai para o Histórico do `contexto do jogo.md`; fora ele, só se mexe em arquivo a
  pedido do usuário (este pedido cobre a feature).
- Unity 6000.5.6f1, URP, Input System novo, TextMeshPro, uGUI. Celular feito em Canvas Screen Space Overlay
  (`CelularBuilder`, `HistoriaBuilder`).
- `Dengue > Criar Celular` **apaga e recria** o `CelularHUD` inteiro, com a `RedeSocial` voltando ao padrão: na
  cena atual ele não pode ser rodado só para ganhar o card novo (a cena tem posts editados no Inspector).
- Regra das mensagens (`RedeSocial.Postar`): sai na hora ou é recusada pela janela de conflito; a recusada não é
  gasta. Exceção já existente: a previsão de chuva tenta de novo enquanto vale.
- O narrador conta como foco tudo que o `GerenciadorDeFocos` registra; a cena **não** tem piscina.
- Ao terminar a sessão, deixar aberta só a `MenuInicial` (regra do `GuardaDeCenas`).
- **Outra sessão** ("Conquista sem tela quebrada") deixou mudanças sem commit nesta branch, inclusive na
  `SampleScene` e no `contexto do jogo.md`, os mesmos arquivos que esta feature muda. Isso precisa ser resolvido
  com o usuário antes de implementar (ver "Antes de implementar").

## Abordagens consideradas

1. **Foto tirada na hora do post** (câmera escondida vai ao ponto marcado e renderiza): mostra o estado do momento
   (água, mosquitos), nada para manter. Recomendada por mim; **recusada pelo usuário**, que preferiu ver e aprovar
   cada foto antes.
2. **Fotos prontas, geradas no Editor** (escolhida): cada foto conferida uma vez e congelada. Custo aceito: a foto
   mostra o foco como estava no Editor (com água, sem mosquitos), mesmo que na partida ele tenha nascido seco.
   Por isso os textos dos posts não afirmam que há água.
3. Descartadas: câmera ao vivo dentro do celular (pesa e não é foto) e desenho por tipo com endereço escrito (não
   mostra o lugar).

## Design

### 1. Como se joga

1. O jogador sai do hospital. O relógio de "empacado" é o mesmo da bronca: `timerSemFoco` do
   `HistoriaRedeSocial`, que zera a cada foco eliminado, não anda com o celular aberto e só anda em jogo.
2. Ao chegar a `tempoSemFoco` (60 s na cena), o narrador escolhe um foco (seção 4) e o dono do tipo posta a foto.
   Com o celular fechado, sai a notificação com a miniatura e o som de sempre; o card entra no topo do feed.
3. O jogador abre o celular (T), vê a foto com o círculo vermelho e anda comparando com o que vê (dá para andar
   com o celular aberto).
4. Se continuar empacado, outra foto sai depois de mais `tempoSemFoco`, de outro foco.
5. Quando o foco de uma foto é eliminado (por qualquer caminho), o card dele fica com a foto escurecida e o selo
   **RESOLVIDO**. A fala normal do dono ao eliminar continua como hoje.
6. Sem foco elegível para foto, sai a bronca antiga (`aoDemorarSemFoco`), como hoje.

### 2. A foto ligada ao foco — `FotoDoFoco` (novo, `Assets/Scripts/FotoDoFoco.cs`)

Componente no **mesmo GameObject do `Interactable`**. É ele que liga a foto àquele foco: a referência é ao objeto,
então nomes repetidos não importam.

- `Texture2D foto`: a foto pronta (asset PNG).
- `Vector3 posicaoDaCamera`, `Vector3 rotacaoDaCamera` (Euler), `float campoDeVisao` (60): a pose, em espaço de
  mundo, de onde a foto foi tirada. Em mundo, e não como filho do foco, porque o pneu é carregado em jogo e a pose
  é só do Editor.
- `OnDrawGizmosSelected`: desenha o frustum da câmera da foto, para ver de onde ela foi tirada.
- `[DisallowMultipleComponent]`.

### 3. As fotos — arquivos e geração no Editor

**Arquivos:** `Assets/Meus/Assets Gerados/Fotos dos Focos/`, um PNG 720×480 por foco. O nome sai do caminho do
foco na hierarquia (sem o grupo do topo, `/` vira ` - `), e é único; em caso de colisão, ganha sufixo. Importação:
Texture Type Default, sem mipmaps, Clamp, sRGB, tamanho máximo 1024, compressão normal.

**Builder** (`Assets/Editor/FotosDosFocosBuilder.cs`), menus em `Dengue > Fotos dos Focos`:

- **Importar fotos aprovadas:** uma vez. Lê o `poses.tsv` aprovado (número, caminho do foco na hierarquia e
  pose), copia o PNG aprovado daquele número para a pasta do jogo **sem regerar** e põe ou atualiza o `FotoDoFoco`
  do foco com a pose e a foto. Assim a foto do jogo é, byte a byte, a que o usuário aprovou. Foco do arquivo que
  não existe na cena, ou foco da cena sem linha, sai num aviso no Console (não para).
- **Gerar fotos (todos os focos)** e **Gerar foto (focos selecionados):** para cada `FotoDoFoco`, renderiza da
  pose, desenha o círculo vermelho, grava o PNG, importa e liga em `foto`. A renderização e o círculo são os do
  preview, sem mudança:
  - câmera temporária `HideAndDontSave`, copiada da Main Camera (`CopyFrom`) e com o pós-processamento,
    antialiasing e sombras do `UniversalAdditionalCameraData` dela; FOV 60, near 0,1, far 400;
  - `RenderTexture` 720×480, 24 bits, MSAA 4×, `Camera.Render()`, `ReadPixels`, `EncodeToPNG`;
  - círculo: retângulo dos 8 cantos do `Renderer.bounds` do foco (sem `ParticleSystemRenderer`) projetado na foto;
    elipse com raios `max(meia largura × 1,3 + 14, 30)` e `max(meia altura × 1,3 + 14, 26)`, traço de raio
    ≈ 2,6 px, cor (232, 30, 30), 1,1 volta, oscilação `0,035 × sen(3t + fase)` e abertura de 7% ao longo do
    traço; `fase = (semente × 1,7) mod 6,28`, com a semente tirada do caminho do foco na hierarquia (estável
    entre execuções). O preview usou a ordem do `FindObjectsByType`, que não se reproduz; por isso as fotos
    aprovadas são copiadas, e o gerador só entra quando uma foto é refeita;
  - nada é salvo na cena: só muda o `FotoDoFoco` (pela `SerializedObject`, com a cena marcada suja).
- **Usar a vista da Scene no foco selecionado:** copia a posição, a rotação e o FOV da câmera da última Scene view
  para o `FotoDoFoco` do foco selecionado. É o jeito de corrigir uma foto: enquadrar na Scene, rodar este menu,
  rodar "Gerar foto (focos selecionados)".
- **Preparar celular para fotos:** idempotente. Na cena atual, acrescenta ao card modelo (`ModeloPost`) a moldura
  da foto e o selo, e ao banner (`Notificacao`) a miniatura, ligando as referências novas. As mesmas peças entram no
  `CelularBuilder.MontarCard` e no `HistoriaBuilder.GarantirNotificacao`, para um celular recriado já nascer com
  elas.

**A foto do jogo é a aprovada:** as 31 do jogo são cópias das de `aprovadas/` (conferido por hash). O gerador é
conferido à parte: regerar algumas fotos numa pasta temporária, a partir das poses importadas, tem de dar a mesma
imagem fora da faixa do círculo (rodar a v2 do preview duas vezes deu arquivos idênticos byte a byte).

### 4. A escolha do foco — `EscolhaDaFoto` (novo, lógica pura)

Classe estática sem `MonoBehaviour` (`Assets/Scripts/EscolhaDaFoto.cs`), para testar sem cena. Recebe uma lista
de candidatos (posição, elegível, já fotografado, "conhecido") e a posição do jogador; devolve o índice escolhido
ou −1.

- **Elegível:** não concluído, ativo na hierarquia, com `FotoDoFoco` e `foto`, e **não mexido pelo jogador**.
- **Mexido:** qualquer coisa que o jogador fez no foco: recebeu um passo (`GerenciadorDeFocos.OnFocoClicado`), foi
  esvaziado pelas Mãos (`GerenciadorDeFocos.OnFocoEsvaziado`; o atalho de esvaziar não dispara `OnClicado`), teve a
  tampa pega (`KitDoAgente.OnTampaPega`; pegar a tampa da caixa d'água não dá passo) ou foi pego
  (`KitDoAgente.OnPneuPego`). O nascimento seco não conta: ele não dispara `OnEsvaziado`.
- **Lugar conhecido:** um foco que já saiu em foto, foi mexido ou foi eliminado. A posição de cada foco é a
  guardada no `Start` do narrador (o pneu muda de lugar ao ser carregado e termina na caçamba).
- **1ª passada:** elegíveis que nunca saíram em foto e estão a mais de `raioLugarConhecido` (4 m) de qualquer
  lugar conhecido; vence o **mais perto do jogador** (distância em linha reta).
- **2ª passada** (nada na 1ª): elegíveis, mesmo os já fotografados, menos o da última foto; vence o mais perto.
- **Nada:** −1 → o narrador posta a bronca antiga.

### 5. O post — `HistoriaRedeSocial`

- No `Update`, quando `timerSemFoco >= tempoSemFoco`: tenta `PostarFotoDeFoco()`.
  - Sem foco (−1): `PostarDaLista(aoDemorarSemFoco, ...)`, como hoje, e o relógio zera.
  - Com foco e a `RedeSocial` aceitando: posta, marca o foco como fotografado, guarda-o como "última foto" e zera
    o relógio.
  - Com foco e a `RedeSocial` recusando (outro aviso acabou de sair): nada é gasto, o relógio fica no limite e a
    tentativa se repete a cada 1 s enquanto ele continuar empacado. É a mesma exceção da previsão de chuva:
    a foto é a ajuda, e perder 60 s por causa de um aviso qualquer seria ruim.
- Prioridade `Normal` (a bronca era `Baixa`), humor `Neutro`, curtidas sorteadas pelo progresso (`-1`).
- **Quem posta:** o `PersonagemDoBairro` do tipo do foco, com o texto novo `aoMandarFoto` (lista; segue a ordem e,
  quando acaba, sorteia sem repetir o último, como o `aoEliminar`). Tipo sem dono ou sem texto: o perfil oficial
  "UPA Vila Nova" (@upa.vilanova) com a lista `aoMandarFotoSemDono`.
- `CompletarPersonagens()` preenche o `aoMandarFoto` vazio dos personagens já salvos na cena com o padrão do tipo
  (mesmo tratamento do `aoUsarFerramentaErrada`). Novo campo `raioLugarConhecido` (4 m) no grupo "Gatilhos".
- O narrador assina `OnFocoClicado` e `OnFocoEsvaziado` (já tem o gerenciador; o segundo já é assinado) e
  `kit.OnPneuPego` (já assina) e `kit.OnTampaPega` para marcar "mexido".
- **Textos padrão** (sem endereço; não afirmam água, porque a foto é congelada e metade dos focos nasce seca —
  só o ralo nasce sempre com água):
  - Carlos Mendes (pneus): "Esse pneu tá largado aqui faz uma semana. Alguém tira isso, pelo amor de Deus?" ·
    "Tirei foto pra ninguém dizer que não avisei. Pneu velho jogado é casa de mosquito." · "Agente, olha o que
    tem perto de casa. Passa aqui?"
  - Dona Neusa (vasos): "Agente, olha esse vaso. Pratinho com água é convite pro mosquito." · "Tirei foto desse
    vaso hoje cedo. Dá uma passadinha aqui?"
  - João Silva (caixas d'água): "Essa caixa d'água tá destampada faz tempo. Meu filho tá internado, agente. Vem
    ver isso." · "Olha essa caixa lá em cima. Alguém consegue subir e tampar?"
  - Ana Paula (piscina): "Essa piscininha tá esquecida no quintal. Agente, passa aqui?"
  - Bruna Lima (garrafas): "Achei isso jogado. Uma tampinha de água já basta pro mosquito." · "Foto do lixo que
    ninguém recolhe. Agente, dá uma olhada?"
  - Seu Osvaldo (ralos): "Esse ralo tá sem tela faz tempo e junta água toda vez que chove. Agente, consegue ver?"
  - Lúcia Andrade (bromélias): "Agente, essa bromélia pode estar juntando água nas folhas. Dá uma olhada? Mas não
    arranca, por favor!"
  - UPA Vila Nova (sem dono): "Moradores mandaram esta foto pra gente. Agente, dá uma olhada?"

### 6. O dado do post — `PostRedeSocial`

Dois campos novos, **`[System.NonSerialized]`** (as listas de posts do Inspector não mostram nem gravam):
`Texture foto` e `Interactable foco`. Os posts do Inspector continuam sem foto.

### 7. O card — `FeedRedeSocial` e `PostView`

- O card modelo ganha, entre `Texto` e `Acoes`: `Foto` (moldura: `Image` com o `UISprite` fatiado, `Mask` sem
  mostrar o gráfico, `LayoutElement` com altura 237) → `Imagem` (`RawImage` esticada) e `Resolvido` (selo
  176×44, verde (0,20; 0,68; 0,30; 0,95), girado 8°, texto TMP "RESOLVIDO" 22 em negrito, branco, espaçamento 6).
  A largura da foto é a do card (356 px de conteúdo), o que dá 3:2 com a altura 237.
- `PostView` ganha as referências `foto` (a moldura), `fotoImagem`, `selo` e, em runtime, `foco`.
- `Adicionar(post)`: com `post.foto`, liga a moldura e põe a textura; sem foto, a moldura fica desligada (o card
  fica como hoje). Guarda `post.foco` no `PostView`.
- O feed assina o `OnFocoEliminado` do `GerenciadorDeFocos` (campo ou `Obter()`, como os outros) e carimba os
  cards daquele foco: `RawImage` com cor (0,62; 0,62; 0,62) e o selo ligado. Ao recuperar posts antigos no
  `Start`, um foco já concluído já entra carimbado.
- Carimbo com o celular aberto: o selo entra com um "pop" (escala 1,3 → 1 em 0,25 s, sem escala de tempo). Valor
  meu, ajustável.

### 8. A notificação — `NotificacaoCelular`

- O banner passa de 440 para **500 px** de largura (para todos os posts).
- Novas referências: `miniatura` (moldura 96×64, à direita, centrada na altura, margem 14, com `Mask`) e
  `miniaturaImagem` (`RawImage`).
- Post com foto: miniatura ligada com a foto; cabeçalho "**autor**  enviou uma foto" (o "enviou uma foto" com a cor
  apagada do @); a área do texto termina a 118 px da borda direita.
- Post sem foto: miniatura desligada, cabeçalho de hoje (autor + @), a área do texto volta aos 18 px de hoje.

### 9. Valores iniciais

| Valor | Onde | Inicial |
|---|---|---|
| `tempoSemFoco` | `HistoriaRedeSocial` (já existe) | 60 s (o da cena) |
| `raioLugarConhecido` | `HistoriaRedeSocial` (novo) | 4 m |
| nova tentativa da foto recusada | `HistoriaRedeSocial` | 1 s |
| tamanho da foto | builder | 720×480, FOV 60 |
| moldura no card | `CelularBuilder` | altura 237 (largura do card) |
| miniatura na notificação | `HistoriaBuilder` | 96×64 |
| largura do banner | `HistoriaBuilder` | 500 px |

## Testes

EditMode, no padrão dos testes do projeto (`Assets/Editor/Testes/`, `internal` liberado pelo `AssemblyInfo`):

- `EscolhaDaFoto`: o mais perto vence; não elegível (concluído, inativo, sem foto, mexido) fica de fora; foco a
  menos de 4 m de um lugar conhecido fica de fora na 1ª passada; a 2ª passada repete fotografado, menos o último;
  nada elegível → −1.
- `HistoriaRedeSocial`: empacado posta o post com `foto` e `foco` do escolhido e autor = dono do tipo; recusado não
  marca o foco nem zera o relógio, e sai na tentativa seguinte; sem candidato sai a bronca; mexido
  (`OnFocoClicado`, `OnPneuPego`) deixa de ser escolhido; `aoMandarFoto` em ordem e depois sorteado sem repetir;
  mexido também por `OnFocoEsvaziado` e `OnTampaPega`; tipo sem dono usa a UPA; `CompletarPersonagens` preenche
  `aoMandarFoto` vazio e não mexe no preenchido.
- `FeedRedeSocial`: post com foto liga a moldura com a textura; sem foto, desligada; `OnFocoEliminado` carimba só os
  cards daquele foco; recuperar posts no `Start` com foco concluído já carimba.
- `NotificacaoCelular`: com foto, miniatura e "enviou uma foto"; sem foto, como hoje.
- `PostRedeSocial`: `JsonUtility.ToJson` não leva `foto` nem `foco`.
- Builder: "Preparar celular" rodado duas vezes não duplica nada; "Gerar foto" num foco de teste grava um PNG
  720×480 e liga a textura.

## Verificação no jogo

- As 31 fotos do jogo com o mesmo hash das de `aprovadas/`, e cada `FotoDoFoco` apontando para a do seu foco
  (conferir os dois `pneu (5)`). Algumas regeradas numa pasta temporária iguais fora do círculo.
- Play com o Editor em foco: sair do hospital, esperar 60 s parado → notificação com miniatura; abrir o celular →
  card com a foto do foco mais perto; eliminar esse foco → selo no card. Esperar de novo → foto de outro foco.
- Capturas (Screen Space Camera, como no preview) para o registro. Console sem erros.
- Suíte EditMode inteira passando.

## Escolhas minhas

1. Pose em espaço de mundo no componente, não um filho do foco (o pneu é carregado em jogo).
2. Correção de foto pela vista da Scene, em vez de um gizmo arrastável.
3. Nome do arquivo pelo caminho na hierarquia; a ligação de verdade é a referência no componente.
4. `raioLugarConhecido` de 4 m (o monte de pneus cabe em 2 m; a mesinha da Coffe House junta 3 focos em 2 m).
5. Foto recusada pela janela de conflito tenta de novo a cada 1 s (exceção como a da chuva).
6. Prioridade `Normal` e humor `Neutro`.
7. Perfil "UPA Vila Nova" para tipo sem dono.
8. Textos padrão da seção 5.
9. Banner 500 px para todos os posts, não só os com foto.
10. "Pop" do selo quando o foco é eliminado com o celular aberto.
11. Fotos 720×480 (o dobro do que aparece no card), compressão normal.
12. "Mexido" inclui esvaziar com as Mãos e pegar a tampa (apontado na revisão do documento: sem isso podia sair a
    foto da caixa cuja tampa o jogador está carregando).
13. As fotos do jogo são cópias das aprovadas, não regeradas; a semente do círculo do gerador vem do caminho do foco.

## O que fica para quem constrói

- A semente do círculo por foco e o nome exato dos arquivos.
- Onde guardar o `poses.tsv` para o menu de importação (ele está fora do git; o Apêndice tem os mesmos valores).
- Ordem das tarefas e branch (ver "Antes de implementar").
- Um menu de ponto automático para focos novos fica fora; os scripts `fotos_v2_poses.cs` (filtro físico) e
  `fotos_v3_refazer.cs` (filtro físico + medição na imagem, 3 opções) estão em `capturas/scripts/` se ele for
  pedido depois.

## Antes de implementar

A outra sessão deixou sem commit, nesta branch: `AvaliacaoDaPartida.cs`, `PlacarDaPartida.cs`, três testes, a
`SampleScene` e o `contexto do jogo.md` (troca da meta "Sem retrabalho" por "Sem erros"). Esta feature muda a
`SampleScene` (no LFS, não dá para juntar à mão) e o `contexto do jogo.md`. O usuário decide como separar antes da
primeira tarefa (por exemplo, commitar o trabalho daquela sessão primeiro, num commit dela, e abrir a branch desta
feature a partir dali). As mudanças dela na cena são só três textos de interface, que não aparecem nas fotos.

`Assets/a.unity` (solto, de 15/11) e `LiberationSans SDF - Fallback.asset` (modificado desde antes desta sessão)
não são desta feature e ficam fora dos commits dela.

## Apêndice — poses aprovadas

Posição (x; y; z) e rotação Euler (x; y; z) da câmera de cada foto, FOV 60. As fotos 19 a 23 (monte de pneus)
usam a pose da 15, que o usuário aprovou.

| Nº | Foco (caminho na hierarquia) | Posição | Rotação |
|---|---|---|---|
| 01 | `--- FOCOS ---/Bromelia (1)` | -3.148; 1.893; 4.890 | 7.750; 195.000; 0.000 |
| 02 | `--- FOCOS ---/Bromelia (2)` | -3.409; 1.893; 4.629 | 7.750; 75.000; 0.000 |
| 03 | `--- FOCOS ---/Bromelia (3)` | -28.433; 1.895; -21.839 | 11.139; 300.000; 0.000 |
| 04 | `--- FOCOS ---/Bromelia (4)` | -26.021; 1.895; 31.196 | 0.060; 15.000; 0.000 |
| 05 | `--- FOCOS ---/Caixa d'Agua` | 37.743; 4.793; -23.495 | 6.094; 30.000; 0.000 |
| 06 | `--- FOCOS ---/Caixa d'Agua (1)` | -25.024; 7.295; 31.320 | 11.644; 15.000; 0.000 |
| 07 | `--- BAIRRO ---/Terrenos/Cenario Terreno Baldio/Lixo/Garrafa ` | -18.114; 1.783; 63.445 | 12.794; 195.000; 0.000 |
| 08 | `--- BAIRRO ---/Terrenos/Cenario Terreno Baldio/Lixo/Garrafa  (1)` | -32.460; 1.893; -14.195 | 12.340; 180.000; 0.000 |
| 09 | `--- BAIRRO ---/Terrenos/Cenario Terreno Baldio/Lixo/Garrafa  (2)` | -19.531; 1.895; 12.003 | 8.852; 60.000; 0.000 |
| 10 | `--- BAIRRO ---/Terrenos/Cenario Terreno Baldio/Lixo/Garrafa  (3)` | -38.028; 1.875; 55.526 | 9.050; 15.000; 0.000 |
| 11 | `--- BAIRRO ---/Terrenos/Cenario Terreno Baldio/Lixo/Lata 001` | -27.590; 1.893; 38.752 | 8.577; 15.000; 0.000 |
| 12 | `--- BAIRRO ---/Terrenos/Cenario Terreno Baldio/Lixo/Lata 001 (1)` | -30.988; 1.637; 76.113 | 7.971; 255.000; 0.000 |
| 13 | `--- BAIRRO ---/Terrenos/Cenario Terreno Baldio/Lixo/Lata 001 (2)` | 21.249; 1.930; 16.380 | 9.050; 285.000; 0.000 |
| 14 | `--- FOCOS ---/pneu (1)` | -36.391; 1.746; 66.033 | 10.140; 330.000; 0.000 |
| 15 | `--- FOCOS ---/PilhadePneu/pneu (10)` | 41.668; 1.893; 22.064 | 6.170; 345.000; 0.000 |
| 16 | `--- FOCOS ---/pneu (3)` | -41.719; 1.893; -16.183 | 9.430; 135.000; 0.000 |
| 17 | `--- FOCOS ---/pneu (4)` | -42.664; 1.893; 16.059 | 12.638; 135.000; 0.000 |
| 18 | `--- FOCOS ---/pneu (5)` | 15.631; 1.710; 25.430 | 5.234; 195.000; 0.000 |
| 19 | `--- FOCOS ---/PilhadePneu/pneu (5)` | 41.668; 1.893; 22.064 | 6.170; 345.000; 0.000 |
| 20 | `--- FOCOS ---/PilhadePneu/pneu (6)` | 41.668; 1.893; 22.064 | 6.170; 345.000; 0.000 |
| 21 | `--- FOCOS ---/PilhadePneu/pneu (7)` | 41.668; 1.893; 22.064 | 6.170; 345.000; 0.000 |
| 22 | `--- FOCOS ---/PilhadePneu/pneu (8)` | 41.668; 1.893; 22.064 | 6.170; 345.000; 0.000 |
| 23 | `--- FOCOS ---/PilhadePneu/pneu (9)` | 41.668; 1.893; 22.064 | 6.170; 345.000; 0.000 |
| 24 | `--- FOCOS ---/Ralo (1)` | 21.825; 1.710; -12.381 | 13.464; 180.000; 0.000 |
| 25 | `--- FOCOS ---/Ralo (2)` | 15.653; 1.710; 24.515 | 13.464; 90.000; 0.000 |
| 26 | `--- FOCOS ---/Ralo (3)` | -25.573; 1.710; 25.943 | 13.464; 120.000; 0.000 |
| 27 | `--- FOCOS ---/Ralo (4)` | -16.735; 1.710; -22.089 | 13.464; 315.000; 0.000 |
| 28 | `--- FOCOS ---/Vaso de Flor` | -12.433; 1.893; -5.354 | 358.390; 210.000; 0.000 |
| 29 | `--- FOCOS ---/Vaso de Flor (1)` | -23.463; 1.895; 31.033 | 358.605; 0.000; 0.000 |
| 30 | `--- FOCOS ---/Vaso de Flor (2)` | -29.787; 1.893; 32.995 | 356.829; 45.000; 0.000 |
| 31 | `--- FOCOS ---/Vaso de Flor (4)` | -34.892; 1.893; 19.367 | 6.354; 60.000; 0.000 |

Os nomes `Garrafa ` e `Garrafa  (n)` têm espaços como na cena (um no fim; dois antes do número). Há dois
`pneu (5)`: o 18 é `--- FOCOS ---/pneu (5)` (no quiosque da praça) e o 19 é `--- FOCOS ---/PilhadePneu/pneu (5)`.

## Registro

Entrada "2026-11-17 — Design: fotos dos focos na rede social" no `contexto do jogo.md`.
