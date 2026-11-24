# Focos já limpos, Lona e Pote de bicho (com minigames)

## Intenção
Cada partida deve parecer nova. Uma parte dos focos da cena já começa "limpa" (resolvida pela vizinhança) e fica fora da partida, sorteada a cada vez. O jogo ganha dois tipos novos de foco, cada um com minigame próprio. O usuário posiciona os focos na cena manualmente; o trabalho entrega o código e os prefabs prontos.

## Decisões do usuário
- **"Limpo" = já resolvido:** o foco some do mapa, não aparece na lista do Tab e não conta no total. Não é o "nascer seco" que já existia.
- **Tipos novos:** Lona com poça e Pote de água de bicho, escolhidos entre quatro propostas. As prévias do Blender foram aprovadas.
- **Posições:** os pontos no mapa ficam com o usuário. Nenhuma instância foi colocada na cena.
- **Minigame do pote:** esponja e um círculo predefinido para completar com o mouse, só com as Mãos. A esponja aparece só na hora de limpar.
- **Minigame da lona:** "inclinar e escorrer", escolhido entre duas opções. As duas prévias em Play foram aprovadas.

## Abordagens consideradas
- **Já limpos:**
  - *desligar e não registrar* (escolhida), porque todo consumidor copia a lista do gerenciador no `Start`;
  - *registrar e marcar como concluído*, descartada porque contaria como eliminado na nota e na rede social.
- **Minigame da lona:**
  - *inclinar e escorrer* (escolhida);
  - *dobrar puxando as pontas*, descartada por ser mais próxima de um quick time event.

## Regras
- **Sorteio dos já limpos** (`GerenciadorDeFocos.SortearJaLimpos`, só em Play, dentro do `Garantir`, antes do registro):
  - contagem fixa `round(fracaoJaLimpos × focos ligados e não concluídos)`, com padrão 1/3;
  - sempre sobra `minimoAtivosPorTipo` (padrão 1) de cada classe;
  - um `GrupoDeFocos` sai inteiro;
  - os sorteados são desligados.
- **Lona:** com as Mãos, cada passo tira uma poça. Concluída, vira a lona dobrada e fica na cena.
  - Minigame (`InclinarLona`): o mouse inclina a lona, que volta sozinha a ficar reta. As poças escorrem, e a borda −x (verde) é a saída. Cada poça que sai dá um passo.
- **Pote:** com as Mãos, a água velha sai no primeiro passo e a água limpa aparece no fim. O pote fica na cena.
  - Minigame (`EsfregarCirculo`): segurando o clique, a esponja passa pelo círculo na parede de dentro. Os trechos limpos ficam verdes, e os passos se dividem pela volta.

## Deixado para o construtor
Cores, tamanhos, câmera de perto e a sensibilidade dos minigames, tudo ajustável no Inspector dos prefabs.
