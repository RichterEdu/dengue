# Foco Zero — Fotos dos focos por tipo (design curto)

Design combinado com o usuário em 2026-11-21. Base: `main` em `c1806e5` (fotos dos focos já juntadas pelo PR #3).
Muda só **quando** e **de que tipo** sai a foto; o resto da feature "fotos dos focos"
(`docs/superpowers/specs/2026-11-17-fotos-dos-focos-design.md`) continua valendo.

## Por quê

Pedido do usuário: "o jogador normalmente fica limpando os focos antes de vir o post, então quero que os posts do
pessoal sejam de acordo com cada tipo de foco; se o player não limpar nenhum vaso de flor dentro de x tempo, venha o
post de algum vaso de flor".

Hoje a foto só sai depois de `tempoSemFoco` (60 s) sem eliminar **nenhum** foco. Quem limpa um atrás do outro nunca
empaca, mas pode estar esquecendo um tipo inteiro (nunca achou os vasos) — e não recebe ajuda.

## O que o usuário decidiu

- Um relógio **por tipo de foco**; vencido, sai a foto de um foco **daquele tipo**.
- **90 s**, igual para todos os tipos (aceitou a proposta de dar para ajustar por tipo no Inspector).
- Abordagem 1 (relógio por tipo no narrador), entre: 1) por tipo no narrador; 2) por foco (mais posts, perde o
  "você esqueceu os vasos"); 3) manter o "empacado" geral e somar o por tipo (dois posts pelo mesmo motivo).

## Design

1. **Relógio por tipo** (`HistoriaRedeSocial`): um para cada tipo que tem foco na cena (hoje: pneu, vaso, caixa
   d'água, garrafas, bueiro, bromélia). Anda só em jogo e com o celular fechado, como o relógio de hoje.
2. **Zera** quando um foco daquele tipo é eliminado e quando sai uma foto daquele tipo. Eliminar foco de outro tipo
   não mexe nele.
3. **Vence aos 90 s** (`tempoPorTipo`, Inspector) ou no valor da lista opcional `temposPorTipo` (tipo → segundos)
   para os tipos que o usuário quiser diferentes.
4. **Começo escalonado**: os tipos presentes, na ordem do enum `TipoFoco` (pneu, vaso, caixa, garrafas, bueiro,
   bromélia), começam com o relógio atrasado `i × intervaloEntreTipos` (15 s): na saída do hospital, o 1º vence aos
   90 s, o 2º aos 105 s... o 6º aos 165 s. Assim os posts não se amontoam.
5. **A foto**: a mesma escolha de hoje (`EscolhaDaFoto`), só que entre os focos daquele tipo — os de outros tipos
   entram como não elegíveis, mas continuam contando como "lugar conhecido". A 2ª passada (repetir foto) evita a
   última foto **daquele tipo**. Quem posta é o dono do tipo, com o `aoMandarFoto` de hoje.
6. **Recusada** pela janela de conflito (outro aviso acabou de sair): não gasta nada e tenta de novo a cada 1 s,
   como hoje. Dois tipos vencidos juntos saem um depois do outro.
7. **Sem foco elegível no tipo** (todos mexidos ou já eliminados, ou só sobrou o da última foto): não posta, e o
   relógio zera. **Tipo todo eliminado**: o relógio para.
8. **Sai o "empacado" geral**: o relógio por tipo já cobre quem para tudo. A bronca `aoDemorarSemFoco` ("faz tempo
   que ninguém vê o agente limpar foco") fica sem uso — continua no Inspector, mas não é postada, porque o texto
   ficaria errado para quem está limpando outros tipos. O campo `tempoSemFoco` também fica sem uso.

## Fora do escopo

Mudar fotos, card, selo, notificação, textos, a escolha "mais perto" ou o menu do Editor.

## Escolhas minhas

- Ordem do escalonamento pela ordem do enum (previsível), 15 s entre tipos (90 ÷ 6).
- A bronca e o `tempoSemFoco` ficam sem uso em vez de apagados (nada some do Inspector nem da cena).
- Tipo sem foco elegível zera o relógio em vez de tentar a cada segundo (evita post repetido do mesmo foco).

## Testes (EditMode, categoria `Fotos`)

Relógio de um tipo vence aos 90 s e posta foto daquele tipo; eliminar foco do tipo zera só ele; eliminar de outro
tipo não zera; foto que saiu zera o relógio do tipo; escalonamento (2º tipo só vence 15 s depois do 1º); celular
aberto pausa; recusada não gasta e sai 1 s depois; tipo todo eliminado não posta mais; tipo sem elegível zera sem
postar; `temposPorTipo` sobrepõe os 90 s; a foto não é de outro tipo mesmo quando ele está mais perto. Os testes do
"empacado" geral que deixam de valer são trocados pelos equivalentes por tipo.

## Verificação

Suíte EditMode inteira; em Play, com o agente limpando só pneus, sai a foto de um vaso (ou do 1º tipo esquecido)
perto dos 90 s; Console sem erros.
