# Foco Zero — Itens do kit na mão

Spec com o design aprovado pelo usuário em 2026-11-06 (brainstorming com prévias no Unity).
Unity 6000.5.6f1, URP, Input System 1.20. Base: `main` em `f8a4e76` (Partes A, B e C).

## Context

O agente usa 5 ferramentas para tratar os focos, mas só o saco de lixo existe em 3D: Areia, Tela e Água sanitária são
apenas nomes na barra. Tratar um foco com elas é ver uma barra encher, sem nada na mão.

Objetivo: cada um desses três itens passa a ser um modelo 3D na mão direita, como o saco, com um gesto ao ser usado.
O jogador deve reconhecer o item e sentir que o está usando.

Estado atual relevante:
- `Ferramenta`: Mãos, Areia, Tela, Água sanitária e Saco de lixo. As Mãos não ganham modelo.
- Saco: gerado por `Ferramentas/Blender/gerar_saco.py`, filho do osso `RightHand` (`SacoNaMao`), montado pelo
  `PoseDeCarga` e controlado pelo `FeedbackDeCarga`. A camada `CarregandoSaco` do Animator ergue o braço direito.
- Resultado nos focos: a areia sobe no pratinho do vaso, as poças da bromélia secam e a grelha volta ao ralo.
- Ralo: ao concluir, `RaloInteractable.OnComplete` ativa o campo `tela`, hoje o objeto `Tela` do `Ralo.fbx`, uma placa
  verde sob a grelha. Nenhuma instância da cena tem override desse campo.
- Sem carga: `Interactable.Verificar` devolve `SemCarga`, o `Interactor` recusa o aperto, a HUD mostra "Acabou: ..." e o
  `SomDoKit` toca o erro. Com isso, `Avancando` não liga.
- Ralo com skill check: `Interactor.Avancando` fica falso e o estado ativo está em `Interactor.SkillCheckAtivo`.

### Decisões do usuário
- Alcance: **item na mão + gesto + marca no foco**. Vaso e bromélia já têm a marca; o ralo recebe a tela nova.
- Itens: **balde de areia, tela com moldura e borrifador**.
- Balde: fica como na prévia aprovada, com pose própria.
- Tela: **peça quadrada de malha vazada com moldura verde**. A versão em rolo foi descartada.
- Depois de concluído, o ralo mostra **essa mesma tela, sob a grelha fechada e ajustada ao tamanho do ralo**.
- Sem carga: o **balde fica vazio**, o **borrifador continua na mão** e a **tela não aparece**.
- Tentar usar sem carga mostra só o aviso que já existe.

### Decisões tomadas no design
- **Poses e gestos animados no Blender**, no padrão do saco. Gesto feito por código brigaria com o Animator, e item
  preso à câmera se separaria da mão visível.
- **Componente novo, separado do `FeedbackDeCarga`**, que continua cuidando do saco e do pneu.
- **O braço acompanha o olhar.** O ralo e o vaso ficam no chão: com a pose presa ao corpo, olhar para baixo subiria o
  item para o meio da tela, em cima do foco.

## Jogabilidade

### O que aparece na mão

| Selecionada | Carga | Pneu nos braços | Na mão |
|---|---|---|---|
| Areia | > 0 | não | Balde com areia |
| Areia | 0 | não | Balde vazio |
| Tela | > 0 | não | Tela com moldura |
| Tela | 0 | não | Nada, braço baixado |
| Água sanitária | qualquer | não | Borrifador |
| Mãos ou Saco de lixo | qualquer | não | Nada novo (o saco segue a regra atual) |
| Qualquer uma | qualquer | sim | Nada |

### Quando o gesto toca

O gesto toca com um item na mão e uma destas condições:
- o `Interactor` está em `Avancando`, a mira está num foco (`CurrentTarget`) e a `FerramentaNecessaria` dele é a
  selecionada;
- há skill check ativo (`SkillCheckAtivo != null`).

| Item | Gesto | Efeito no item |
|---|---|---|
| Balde | Inclina e despeja para a frente | Partículas de areia caindo |
| Tela | Empurra a peça para a frente e para baixo | Nenhum |
| Borrifador | Recuo curto da mão, como apertar o gatilho | Névoa no bico |

Usar um ponto de uso, como reabastecer na picape, não toca gesto. Soltar o botão, errar o skill check ou pausar volta à
pose de segurar. Selecionar outra ferramenta troca o item em 0,15 s.

### Sem carga e ferramenta errada

Não há comportamento novo. A recusa já impede o gesto e mostra o aviso e o som atuais. Isso vale para o balde vazio, para
o borrifador sem carga e para a tela, que não aparece mas continua selecionada.

Quando a última carga acaba, o item segue a tabela no mesmo quadro: ao concluir com a última tela, ela some da mão e
aparece no ralo. Errar o skill check gasta a tela, e ela some da mão se a carga zerar.

### Marca no ralo

Ao concluir, o ralo mostra a tela com moldura **sob a grelha fechada**, no lugar da placa antiga e na mesma altura dela.
Ela cobre a boca do ralo e se apoia na moldura de concreto, com 29 × 29 cm no espaço local do ralo. Por ser filha do
prefab, acompanha a escala de cada instância e fica ajustada também nos ralos esticados.

A moldura verde fica quase toda sob o quadro da grelha, e o que aparece é a malha entre as barras. O restante do fluxo do
ralo não muda.

## Modelos

Novo `Ferramentas/Blender/gerar_itens.py`, no padrão do `gerar_saco.py`: metros, Z para cima, faces chapadas, exporta os
FBX e as prévias. Saída em `Assets/Meus/Assets Gerados/Itens/`.

| Modelo | Medidas | Objetos | Pivô |
|---|---|---|---|
| `Balde.fbx` | Ø 22 cm, 17 cm de altura, com alça | `Balde`, `Areia` | Meio da alça |
| `Tela.fbx` | 29 × 29 cm, moldura de 1,7 cm, 0,8 cm de espessura | `Moldura`, `Malha` | Centro, deitada no plano XY |
| `Borrifador.fbx` | ~20 cm de altura | `Borrifador` | Gargalo |

Cores (sRGB):
- Balde: zinco `#9EA8B3`, alça `#4D4F54`, areia `#D6B87A`.
- Tela: moldura `#17805F`, fios `#293635`.
- Borrifador: frasco `#EDF0EB`, rótulo `#1C54AD`, faixa `#F5CC33`, cabeça `#17805F`, gatilho e bico `#0D4D3D`.

A silhueta do borrifador segue a prévia aprovada: frasco oval com ombro alto, gargalo com rosca, cabeça com topo descendo
até o bico e gatilho curvo à frente do gargalo.

A malha da tela é uma textura em grade com furos reais, gerada pelo `gerar_itens.py` (`Tela_Malha.png`, 64 × 64, repetida
a cada 5 cm). Material URP Lit com Alpha Clip e as duas faces visíveis; importação com repetição e mip maps preservando
a cobertura do alfa, para a malha não sumir de longe.

Na mão, a tela usa escala 0,67 (19,4 cm). No ralo, usa escala local 1, cabendo sob a grelha. Um só modelo atende aos dois
casos.

## Animação

Em `gerar_agente.py`, só o braço direito, com `braco_ik`, como a `CarregandoSaco`:

| Ação | Tipo |
|---|---|
| `CarregandoBalde`, `CarregandoTela`, `CarregandoBorrifador` | Pose estática, 1 frame |
| `UsandoBalde`, `UsandoTela`, `UsandoBorrifador` | Loop de 16 a 24 frames, a 30 fps |

No Animator, uma camada nova `ItemNaMao` usa a máscara já existente `Agente_CarregandoSaco.mask` e tem 6 estados com os
mesmos nomes das ações, sem parâmetros nem transições. O script troca de estado com `CrossFadeInFixedTime`, em 0,12 s. A
camada nasce com peso 0, sem Write Defaults, como a do saco.

Enquadramento exigido: **o item inteiro à vista no canto inferior direito**, com a pega dentro da mão e a mira livre. No
balde, basta ver a boca com a areia e a alça. Os valores da prévia são só ponto de partida: a posição e o giro finais de
cada item ficam em constantes do `ItensNaMaoBuilder`, como os do saco no `PoseDeCarga`.

## Código

### `Assets/Scripts/RegraDoItemNaMao.cs` (novo, C# puro)

```csharp
public enum ItemNaMao { Nenhum, BaldeCheio, BaldeVazio, Tela, Borrifador }

public static class RegraDoItemNaMao
{
    public static ItemNaMao Qual(Ferramenta selecionada, int carga, bool comPneu);
    public static bool Gesticula(ItemNaMao item, Ferramenta selecionada, bool skillCheckAtivo,
                                 bool avancando, Ferramenta? necessariaDoAlvo);
}
```

Implementa as duas tabelas da seção de jogabilidade, sem depender da cena.

### `Assets/Scripts/ItensNaMao.cs` (novo, MonoBehaviour no Player)

Campos: `KitDoAgente kit`, `Interactor interactor`, `ControladorJogador jogador`, `Animator animator`,
`Transform baldeNaMao`, `GameObject areiaDoBalde`, `Transform telaNaMao`, `Transform borrifadorNaMao`,
`ParticleSystem areiaCaindo`, `ParticleSystem nevoa`, `Transform ombroDireito`, `float duracaoBraco = 0.15f`,
`float fatorDoOlhar = 1f`. Vazios, são achados no primeiro uso, como nos demais scripts do kit.

- **Estado:** assina `kit.OnMudou`, calcula `Qual(...)` e ativa o objeto certo. `areiaDoBalde` só fica ativa no
  `BaldeCheio`. `Aplicar()` é `internal`, para os testes.
- **Update:** calcula `Gesticula(...)`, troca o estado da camada quando muda e leva o peso a 1 com item e a 0 sem item,
  em `duracaoBraco`. Liga e desliga a emissão das partículas com o gesto.
- **LateUpdate:** com peso acima de 0, gira `ombroDireito` em volta do próprio ponto e do eixo direito do Player por
  `jogador.Pitch * fatorDoOlhar * peso`. Roda depois do Animator, sobre a pose do quadro.
- **Falhas:** camada ausente gera um único `Debug.LogWarning` pedindo para rodar o menu; o item aparece sem pose. Campo
  vazio é ignorado sem erro.

### `Assets/Editor/ItensNaMaoBuilder.cs` (novo)

Menu **Dengue > Configurar Itens na Mão**, idempotente:
1. `GarantirAssets()`: importação dos 3 FBX, textura e materiais URP, clipes no `AgenteSaude.fbx` (sem root motion) e
   camada `ItemNaMao`, recriada a cada vez.
2. `AplicarNoPlayer(player)`: cria `BaldeNaMao`, `TelaNaMao` e `BorrifadorNaMao` como filhos de `RightHand`, inativos,
   com posição, giro e escala em constantes; cria as partículas; adiciona e liga o `ItensNaMao`.
3. `AdicionarTelaAoRalo()`: abre o `Ralo.prefab` com `PrefabUtility.LoadPrefabContents`, cria `TelaInstalada` (instância
   do `Tela.fbx`, inativa) ao lado da `Tampa`, com escala local 1 e centro a 2,9 cm de altura local, logo abaixo da
   grelha fechada (3,4 cm), e aponta o campo `tela` para ela.
4. Salva a cena.

O objeto `Tela` do `Ralo.fbx` fica no prefab, inativo e sem referência: o `Ralo.fbx` não é regerado.

Mudanças em arquivos existentes:
- `FocosBlenderBuilder.CriarRalo`: chama `AdicionarTelaAoRalo()` no fim, para o prefab recriado não perder a tela nova.
- `PlayerBuilder.cs` e `PoseDeCarga.cs`: comentários avisando que, depois do Configurar Player, é preciso rodar também
  este menu, porque ele destrói o `Modelo` e recria o controller.
- `gerar_agente.py`: ações novas. O `AgenteSaude.fbx` é reexportado.

Nada muda em `FeedbackDeCarga`, `KitDoAgente`, `Interactor`, `DengueHUD`, `RaloInteractable` ou `EstoqueDoKit`.

## Testes

EditMode, em `Assets/Editor/Testes/ItensNaMaoTestes.cs`:
- `Qual`: cada linha da tabela, inclusive borrifador sem carga e pneu nos braços.
- `Gesticula`: foco certo, ponto de uso, ferramenta diferente da necessária, skill check, sem avançar e sem item.
- `ItensNaMao.Aplicar`: ativa só o objeto certo, esconde a areia no balde vazio e reage ao `OnMudou`.
- Ausência da camada: um único aviso, sem exceção.
- Assets: a camada `ItemNaMao` tem os 6 estados; o `Ralo.prefab` aponta `tela` para `TelaInstalada`, inativa; os três
  filhos de `RightHand` existem no Player da cena.
- Ralo: a tela nova fica inativa no início e ativa depois de concluído.
- Ajuste ao ralo: no prefab, a `TelaInstalada` tem escala local 1, cabe na área da grelha e fica abaixo dela.

A suíte existente continua passando.

Em Play Mode, com capturas:
- os três itens, nas poses de segurar e usar;
- enquadramento olhando para a frente e para um foco no chão;
- balde vazio e borrifador sem carga, com o aviso ao tentar usar;
- os quatro ralos concluídos, com a tela sob a grelha e ajustada à escala de cada um;
- troca de ferramenta com o botão apertado e pegar um pneu com a tela selecionada antes.

## Fora do escopo

- Mudar o saco, o pneu, o balanceamento, os textos ou o estoque.
- Sons novos: o loop de segurar continua igual, e o skill check segue sem ele.
- Dedos animados: o agente não tem ossos de dedos.
- Modelos para as Mãos e ícones novos na barra.
- Item sobre o corpo do agente quando não está selecionado.

## Riscos

- **Reexportar o agente** pode afetar clipes existentes. Conferir locomoção, pneu e saco depois da troca.
- **Giro do braço com o olhar** é o ponto menos provado: nas prévias o braço foi posto à mão, relativo à câmera. Se ficar
  ruim, o `fatorDoOlhar` reduz ou desliga o efeito.
- **Ralos esticados na cena:** ao menos um tem escala 1,21 × 2,11. Herdar a escala é o que ajusta a tela a esse ralo, mas
  os furos da malha esticam junto. A captura dos quatro ralos decide se algum precisa de correção.
- **Tela sob a grelha:** a calçada não tem buraco, então a malha fica sobre o piso da cena. A captura com a grelha fechada
  confirma se os furos continuam legíveis entre as barras.
