**GameDoc \- Documento Macro do Projeto**

Autor: Eduardo Augusto Richter  
Data de criação do projeto: 

1. **Área temática:** Saúde Pública & Educação Comunitária com foco em conscientização sobre epidemias sazonais (especialmente dengue) e combate aos criadouros do Aedes aegypti. O jogo aborda a responsabilidade coletiva na prevenção, mostrando que infraestrutura hospitalar sozinha não é suficiente sem ação comunitária.   
     
2. **História:** O jogo inicia na recepção caótica de uma UBS/Hospital público superlotado com pacientes graves de dengue. A equipe médica alertou que sem eliminar os criadouros no bairro, o tratamento hospitalar é inútil — a epidemia continuará gerando novos casos.  
   O jogador assume o papel de um agente comunitário de saúde que sai a campo pelas ruas vizinhas com a missão urgente de inspecionar residências, terrenos baldios e calçadas para erradicar todos os focos do mosquito antes que o hospital entre em colapso total.  
   A rede social do bairro reflete essa urgência: comentários negativos inicialmente ("Que abandono\!", "Meu filho tem dengue 😞") que evoluem para positivos conforme o jogador completa sua missão ("Muito bom\! Graças a você\!"). 

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
   Personagens da Rede Social: Genéricos do bairro (nomes aleatórios como "João Silva", "Maria Santos", "Carlos", etc). Não aparecem fisicamente no mapa, apenas seus comentários na rede social.  
   Sem NPCs no mapa: O foco é puramente na mecânica de busca e eliminação, criando imersão através do áudio 3D e feedback visual/social.  
     
     
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

[image1]: <data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAqgAAAFJCAYAAABXSN/mAABPG0lEQVR4Xu3dh9cURaI28PtvfOdeL0kysiBBQEARBA4sQVmWRQVRFBPsCkdFEFhMrKJyV1FEV1EMrKKIqCiKCopIEAUJSk4LEiRnAanvPMWpod96uyd110xX1fM7p887b3VPT6fqeaY6/Zcgssy5c+dk9/vvv4uzZ8+KM2fOyO706dOy++2339ixY2egU3UMHeoc6h/qoaqTRERJ+S+9gCjtggFVhdRgUA2GVXbs2CXTBeuXqnOqDjKgElHSGFDJSuoLMRhU9cDKjh27ZDu9rgXrIRFRkhhQyVrBL8eosOpCd+DAAbF169ZK5aXq5syZU6lM78o9jWEdpkkvYxe/0+scwykRmcCAStYLflGePHlSfP3112L79u2VvkRNdLt27arw/86dOysNE7dDQNTL0talcRqTmiYc0tbLCukOHjxYqSypbtGiRZXKiu1+/fXXSmXoTpw4UalMdUREpjCgklMQUGHhwoXyC/Tbb7+V/+Pw5JEjRzL/A/rPnz9fLF68WF4Asm3bNtl/+fLlYs+ePbIMgRP/z5s3L/M++PHHH+VfvBcw/u+++0788ssv8n2gpgVWrFghh1GBAp+F//F+NQ5YunSpLAd8LiBoAT4T3ebNm8UXX3whh/vyyy9lGf6q4RUMc/jwYfkXQVpZtmyZ+Oabb+R04n2A6Ub3ww8/yLK9e/eKr776KvMewHT//PPPctlivJg/tRzUNGIZYJkBPhfDYJ7xmWo6jx07lvlcBf2wHMI+d8GCBZllhfWFcX3++ecyUOEzdu/eLYdBp4YBNU34saLWD8aN+cD/GzZskD9mMN+wadMmOV9YBoBpwvaAMsB8oAUxSK0TwDLFvAHm+9ChQzLcYrz4PPRDGWA5Hj16NLMcMQwEpwWfh/ctWbJEzgOo8Qe3P/QHrH9sP6C2dwyPz8A6wXD79+8Xx48fl6ET26RaHmqc+nSuXLlSDo9yVRdyQSBv3ry5qFOnjrjyyitFx44dxf/+7//K7rrrrhOffPJJZj6IiKIwoJJTgqEQIRChREGACP4PCFv4YkYwwnvxBY/ghr8owxc6vkxViMSXOb741XhUOUIAOgQMfJEjNKlgA/iCR3DCZ6lQgnGsWrWqQkDF+/G5GEZ9iaugheCEcKY+W4UajBNBKxiiMR58hpqG4GdgPAh3Koip6Ua3fv16GWQQXDDOIBWogvMQFlARoFVLtpoOUMOeOnUqEyIVhEUVIIOfi+lULeIIS99//32FZY5+anoAn6emTU0TlplaP2rZqWH0gIr+wbCIz1LjVuVYl8HtQM0X+mMaUQ4IagiZKMfpBnif+iw1TjUdYQEVn4H5VSEZ/6vxB7c/1R/bi3o/YHvHZwbXldom9OUB6B+cTjUs/gbrQjYzZsyQQfSuu+7Se4m1a9fKHxZdu3YV1atXF40bNxY9e/YUY8eOFRs3btQHJyLPMaCSs/QvU9UyGSXskGVYGa5oDsJ49TK0UIFqcVLn74GaLoxbtWzq0xacdhW00kbNI5RyGoNhW0Eo1gWnSV8/gGWuwjBaCSFsfYNad8EfQGHjBFWuhlX/6+NW6zy4HEFvpYXg9hH2ufq2DipIhwnrpz4jWK7OOdXLoyCc1qpVSy/OCmEc9eT1118XY8aMEdWqVRPt27fXByMizzCgEpUJDkmvW7dOL64ErWjlhJboXMo9jWHymSa0SFJyEE71H1uFQkvt4MGDRY0aNcRbb72l9yYiTzCgElkAYQstguXo1PmIuZRzGsM6Kr1HHnlELyqaOm9VnQ5BRH5hQCUiotimTZumF8WC0y7q1asnW1OJyD8MqERUNgghuFBNPw+T7NO7d295YViS+vfvL1tRicg/DKhEVDK4Sh/nKOLiG3V7r3379skLivBa3VmA7IK7LZgIkrgLAm5ZRUT+YUAlopIJ3tYIARW35ELrafA2UXEvsqHSw31dTQRU3F7MxHiJKP0YUImoZMICKu67ioCKWz6hxYzsM3fuXHHxxRfrxbH99NNP8mb/ROQfBlQiIooF54oOGTJEL47tz3/+s3j++ef1YiLyAAMqEREVDTfwx5Ohxo8fr/eKBU8cw+F99fhWIvILAyoRERVty5YtMkgmfXoGHmKB8e7Zs0fvRUQeYEAlIqKioYUTQTLpW0zNnz9fVKlSJfTRr0TkPgZUIiIq2nPPPScuv/xyvTi2hx56SHTu3FkvJiJPMKASEVHRWrZsKUaMGKEXx1a7dm0xbtw4vZiIPMGASkRERcPh/alTp+rFsWG8M2fO1IuJyBMMqEREVDQESVwolTSMF08ZIyI/MaASEVFR8BSw+vXr68Wxbd26VTRp0kQvJiKPMKASEVFRtm3bJnr37q0XxzZnzhxxxRVX6MVE5BEGVCIiKsqnn34qnnnmGb04tpEjRxp5MhXZA08Rw2ke//d//6f3Ik8woBIRUVFGjRol5s6dqxfH1rNnT/HOO+/oxWShb7/9NhM20eE1QifKc8Fw6n3kHwZUIiIqGB5xWrVqVflI0iQdOnSIgcRywWCZrcNw+rBhLabZ+pG7GFCJiKhgeASpiSCJljUT46XSUK2lhbaQhgXWYCANDkd+YEAlIqKCqUecJm38+PHyEadkp7jbRDCgqtMBwgIsuY8BlYiICvbss8+Krl276sWx3XTTTbzFlKX0Vs+4gueuhnXkNgZUIiIqGMLDY489phfH1qJFC9G5c2e9mCyQRGjMFUpVl88pBGQ3BlQiIioYQsKKFSv04tgw3n/84x96MaVcmg+916pVS05bmzZtxNmzZ/XesY0ZM0YvytuGDRv0okp69OghXnnlFb04lrp168q/s2bNErfeeqvWNx0YUImIqGD4wt+5c6deHBvGu3DhQr2YUk61bKYN7ql79OhR+Rp/Fy9erA0RLd/5GTp0qF6Ut5UrV+pFFbzxxhti+vTpenFstWvX1otShwGViIgKcvz4cXHppZfqxbEdPHhQHuIn+yDMJXn+aRJwK7SwC+527NghBgwYIHr16iXvRgFoZb3zzjtFvXr15P/Dhw+X8/TAAw/I7bJPnz4yiK5evTrT/5577hENGjTIBNS+fftm3gcIn2idvO222+T/Ct6D4dq1a5cJqNWrVxczZ86Uf5VvvvlGtGrVSo7j2LFjFcb/888/i3379snXmG70w/yi/mB6XnvtNTmO66+/XraSNm7cODNeUAEVn4/hJ02aJM8px2tMV506deS0fP3113K4li1bymWGz8PyA7zG9Kg6i/eNGzdOjBgxQu4j4mJAJSKigt199916UWzr168XN998s15MFsi3tbGUEOrCWgobNmyYed2+fXv5V00/3qNCa9g83XjjjfJvMPgi1O3fv18GNDhz5ow8dI/w169fv8xwgHJVhnv+YhgE088++0yWLViwIDi4HDeG0cePecC0//rrr5lhg6caqGkPC+gQFlDRWgt4L8IuplXNL84NBkwzwiqmR302AjiGDVtecTCgEhFRwWbMmKEXxYYWoyeeeEIvJgskHU6SEjZdwTIV1LIF1AMHDsiA9vzzz8u/GKZ58+bnRyAuhMjg43nxhDUV/oJQPnHixMz/ahi0kqI1El2QGrc+fkwbziNFWFVUiAQ1XydPnsy0hgaFBVT1VDg131gOapzB+cB78T6EWFDvxSkU3bt3D13mxWBAtRyuZDTVqXvOlapDRUhLhwrGjh278O6iiy6qVGZTp9f3cnf6vtB0p+/rk+iwXNXrYsV5b5QjR47IJ54h+OHvSy+9JFsp0Vo4derUzCF7TD8EAyqC2OzZs8XatWvltL377rtyfQEOqS9btkwMHDgwE95wezS0JOJiLAgLqIDp2Lx5szx8r4IePn/r1q3ykHyQCqgQHD9aXHGoHcET4RDvQxgdPHiwnC6cews4tI/50U+dQZDEXTjyDahYFlheOAVCHfZHGaYH8wOYLkzTtGnT5LTExYBaRljx+o6T3YVO34nH6fQddK5O3/nm0xHlS9920tDpdSBbhy9K3FBfLy+mC9bTatWqyfMC9fqbRKfvX9ilv8P2kRQVOpXDhw/LMKqoC6l0aD1VfxEkgy2WapzBOwPonxN11wA13iD9vaC/P2yY4Hxg+nBualDYeyA4nGoNheCyUPOLEItpCQ4H+rgxzqQef8yAWmL4IkDFww6TiMhG2Iclbfv27UbGm6S0T185JblsEEyDITXJcVNx5s2bpxcZx4BaQqrCERHFUe4fuCb2Y99//72R8SaFQSk7U8uGIdVfDKglog4zERHFVc4va5xjZuIWU1OmTBFXXHGFXpwabGDIzuT3G5a7OvpI/mBALQFUKpOVl4j8oVqUyuWOO+4wcgU/bpmDC1jSqtzLPe1Mf8cFD/uTHxhQDWOFIqIklXt/YuLzcfEFxourj9PKxHy7pBThnd+nfmFANYyViYiSUu5WvI0bN8ppwM26k4bxBm86niY8BzI30y2oCtZDqT6LyosB1SB1CxUioiSUOyQtWrRITkMSjzEMQjDFTcfTSt2miqKV8ruO68IPDKgGsRIRUVLUfUPLaeHChXK/pm5unpTJkyeL0aNH68WpwUPLuTGgUtIYUA1iJSKipKRlf1KzZs3EpwXjS+rm3iYwoOZWyuWDzyr3jzUyjwHVoFJWWCJyV5rOgVyyZImcFjzyEK+TgGeFpxkDUW6l3D5520Y/MKAaFLfCluLxfKqip61ThzN97fTlkVSnr/+oDsNiOigdsE7Spl+/fnK6EC7HjBkjli9fLtavXx/6CMds8OjERx99VC9ODdSDNC7/tCnlMlL7SHIbA6pBxVZYFRJwY+Ig/dnZuZ6frYcTvdNDCbvkOn1ZF9rp6zJXp28X+XTZoL+al1zDknlYD2m1d+9e8fzzz4s2bdpktpnu3buLJ554Qrz55pvihx9+ECdOnNDflnHnnXfqRami5omyK+UyUvsnchsDqkHFVCAGAkoThGVuk+WlfoTYYPfu3WLVqlXi6aefFn/6058q/GgbNmyYvE1VEELs7bffXqEsbXwPqHPmzNGLQpV6GZX686j0GFANKrQCFTo8USmokErl4dqyv//++0Xbtm3FH//4R7FgwQK9d+pg+dvyAyEMHoJQrG3btsn5xz4gl2zbKU7j2LJli14cS7bPIzcwoBpUSAXCDrCQ4YlKidtmefjeepcGWP62HkE4cuSInP6DBw/K0zCGDx8uw2K+1HnG9957r95LLF68WI7vgQcekOcdZ9tO0a9FixZiw4YN8j34G1e2zyM3MKAaVEgFwi9UW3eC5L5CtmVKDpd7+dm8DtQPHL2rWrWqPmgleHgChv36668rlDdq1KjS+FQX5r333qs0nOoKCcu6qM8jdzCgGlRIBSpkWKJS4/ZZHvkcWiVzbL8YJyw4IpzmEw67detW6b24+A1laA0Nwv8oP3bsWIVyqFGjhuyH842V+fPnyzK00BZLnzZyDwOqQYVUoHyHVTsc1fXo0UMfpCTeeOMN0b59e704EqbVxPO7gz777DP5OS+//LLYt29fzh1wEN4XPM+sYcOGBZ0fh8c0Ll26VC9OFKZx3bp1enFW+W5XuSQ1Hsofl3n5qTtq2GjmzJmiSpUqerHUoEEDeWuwbLD93XHHHRXKPv7449DtEue5onzevHkVyrFPDBse2rVrF9kvH3HeS3ZgQDWokAqU77D6cAMHDhSffPJJhbJSmDRpUqVf0dlguvfs2aMXV6LPXyFw4QW8//774sorr9T6FqZ27dpi7ty5enGkQocvBs71OnPmjHyN5ZTPeVxxlmdQUuOh/PCc9HTAOrD11KtsIXTo0KFZ999jx46V8672N7mo0wH0H+nZQii+Q6L65SPOe8kODKgGFVKB8h02bLhyHAZMY0BNUqGBs9Dh48JyWrlypV5cSVLLM6nxUH6wvG1tuXOJrdv9yZMn5bTv2rVL7yVh/419VhiEUrxXbz3NBhdR4T3642pRFvU52F/GWb5x3kt2YEA1qJAKlO+w+nBDhgyRtwKBvn37yvOLZs2aJWrVqpUJrtgZ4Z6E1atXFxMmTJD/d+3aNfM/YLx4mgtuth21Q8Ewd999t/j73/8uBgwYkAmoeIIM+qGloXPnzvLwug79VUBVO0BcVYppxHQDWghRjqs8//Wvf2Xeh+maOHFihenCfKJDSwCGUVeRYvqnTp1aYTlh+jCv77zzjizHYSrdjh07ZD+8F/OA5acCJ6ZPTS/+hrUqYNqwTPH5eH/w8/H+Xr16VXo/ph/zH5xerBu0fKD8qaeekuXqvC61DLF88PrWW2+Vy0z1ww3PcVgv+BnB6YgjqfFQbmw9TQ9b10Ou1knc4gtdGJwWkO29OowHw69evVrvJcsxLWFuu+22gj5HF+e9ZAcGVIMKqUD5Dovh9C7M0aNHM/2wgwjuJHLtvNBPP9kdoTd4flHz5s0zARXD4xCPEnZuKoZRARXhK3hyPPqpkJ3vdCHEZYPTHtTnBce5ffv20M/A/IwaNSrzP4ZRARWv1fmsCH5hO3YE1GCLgwrTaMEIft7gwYMz7w+bDn3dYDmpHxrBZYjXUS2omJfgtCchqfFQbljWbD0tP5t/KKgf7mGwL0M/nLOvU1fcRzVS6DAcho+6KwD64UdzGPSLmsZ8xHkv2YEB1aBCKlC+w4YNh9ZBUC1x+B8hRQ2L0BM8vB52eF7tLNTVmPrheP00AjUODKfeq7omTZpUGBaC48TrYLjCeFSA1udPPT6xadOmmXGgC/tV3rNnTzlM48aNxSWXXCI/A4FWH2fY/OllwUP2YctKpx/iV+/RlzWmSb0fzy1Xy0yF22zDZ1uGajx4Ljr+Ri3PYiU1HsqNyzodVJ2ykTp8Hna0R90mKoya51wXmOLxtmrYL774Qu+dgf5hP+hxlA394lw/ETUP5A4GVIMKqUD5Dhs2nCrDX3WlPP6q8lwBFUEneHI73qcHuPHjx1c4nwmHZzAOdbg+bEcYFBwnAmwwYCJQqyvmg/OH6VLhW/XDOPBZuN2JLvhe/GpXAS5YrqZXP1cKnxNsUcAwYa2QOI0Bh/91UQFVv4oV04U7BOhUiwWWS3CecQqGCvzBZYjXwYCK9aEEl2/ws+NIajyUnU2PNXUdtnn9h7kt1A36R44cWaFctZ4ipOpwv1P0e/311/VeFezcuVMOhy7XhZpqOJ06RSuOsPGSWxhQDSqkAuU7bHA47GywA1I7IfTDFexwxRVXZIbNFVARPPGcbFAnu+sBFS2RCFHqlzWGUePAOZbB8eH+eTqc1/TQQw/J10uWLMlMmzqZX8Hrt956S74OHh5XwwUDGu6lB/fdd18meOKJKeqWJyrAoXVShc/WrVvLK0t1L774Yuaz1OkRwYCqph3n/IYdGosKqID3Y/rVa/V+3A5LUTtrdYg/OLy6UXZw/lu2bCnPK1aCh+QwXDCg5moNyYetX9S2UdsglR/Whc0/FnAaFOZBhUi1D0UX1qCA8qjbUsG4ceMy79c7NBiE7WfUVfzBW/ap+7AiRMfBuuI+BlSDCqlAhQybDXY86jxNXDgE+o4D/+stiChT4efw4cMV+gVhGLw3bBzop5+7GoTxBi+gwvBhn4VxBHegGA6fpw+LILpx48YKZRgm7NnTKNNDdxg1TNh8ZHu/Pry+bDBdYe9HeXC+1I+HsOnVd+gYJvhetW6w7ILrXC2/OJLaPima+rJPo99++00vcl5a10Uh9CAZNU8dOnSQ/bLtJ9TFU+hw0ers2bPlhZzNmjWrMH69dTbsyVM4RSCuqHkhdzCgGlRIBSpkWLoArZHqTgQu0Fu304Lbp3lYxmm75yYutrn88ssrhIv+/ftnLnwx/fCNcnJhm1dHg9AhUEZRw2SDH8RPP/10hQtig/7zn/+ITp06hbY647QljD/u/amDck0v2Y8B1aBCKhAOoabtyynt9BZEMqeQbZkKl8YrxjE9eBAITpsJOn36tFi7dq2YMmWKvN1cMLwOGjRInsMYdgjZNr6c1qJO67KNjdNMhWFANaiQCoSdYSHDE5USt02z0rh8cTeMsNNldAisCo4AXH311ZnAev3114uPPvooMLQdfLpYDeeE4slRtkljnaFkMaAaVEgFQutpIcMTlRK3TbPStnxxz2PcBi0u3EaoS5cu4uKLL9Z7pZovrae24velHxhQDSq0AqlWB6I04XZpFpZt2lrrcNFMknDxHubTloutuL2nGwOqHxhQDSqmAqkwkLYvrDTDzqrUnS/UqSc+zXMppfHcUzAxTfXq1bPmgkYT80/Jwf6I68h9DKgGFVuB1JeWCx0CTq5One/lSqfPXz6dvtzS0mF+yJy0LmNMV9JwBXevXr304tRh+Ek/riM/MKAa5FMF0lsYC+n0gGd7p89fPh35KY37iC1btojLLrtML47t2muvFT169NCLUwd1OI3rhS7APpPryH0MqAaxAhFRFAQhtJ6nDQ7DP/7443pxbLh/6tSpU/Xi1ElrqzZdwIDqBwZUg1iBiChKWvcPmK5FixbpxbGldX51tkynzxhQ/cCAahArEBGFSfNhZEyX/gjhJGC8STzi0rS0rhe6gAHVDwyoBrECEVGYNB9GxrSZeBJU/fr19aJU4n47/RhQ/cCAahArEBGFSeu+AS2cvXv31osT0bNnT70oldL6w4EuYED1AwOqQaxARKRTd3tIo9WrV4uJEyfqxYkYMWKEXkRUFAZUPzCgGsQKRES6NO8XnnvuORlSkzZ37lxjwTdJaf3hQBUxoPqBAdUgViAiCsI+Ic37hb/85S9iz549enFso0ePFsuXL9eLUyfN64YuYED1AwOqQaxARKSk+cp9OHLkiLHpu/rqq/WiVDI1/5QsBlQ/MKAaxApEREraW083bdpkbPoaNWqkF6WSqfmnZDGg+oEB1SBWICJS0r4/mDFjhpFbQa1atcrIeJOW5ovXqCIGVD8woBrECkREkPbD+3D33XcbucXUpEmTRPPmzfXi1MH6QfCh9GNA9QMDqkGsQEQENuwLLr30UnkxU9Jw4ZUN82/DNNJ5DKh+YEA1iBWIiGxoPd25c6ecRtwOKmktWrSw4tB52tcRXcCA6gcGVINYgYgI+4G07wtWrFghp3HHjh16r9hq1Kghtm3bphenig0/IugCBlQ/MKAaxApERDbsBx577DHRoUMHvTi2JUuWWDH/mEYbWnnpPAZUPzCgGsQKROQ3W64M79q1qxgyZIheHNurr75qxX7QhmmkCxhQ/cCAahArEJHfbNkHYDpxtX3Shg8fLjp27KgXp44t64nOY0D1AwOqQaxARP5C/bdlH4DpXL16tV4cW8uWLcX8+fP14tSxZT3ReQyofmBANYgViMhfqP82HN4/c+aMqFq1qjh79qzeK7aaNWvqRan05z//WS+iFGNA9QMDqkGsQER+sqn1dOPGjaJ///56cSJMXHiVNPyI4A367cKA6gcGVINYgYj8ZEvrKSxevFiMGzdOL47t8OHD4oYbbtCLU4etp/ZhQPUDA6pBrEBEfrKp7k+YMEHMnDlTL47tww8/FC+88IJenDo2rSs6jwHVDwyoBrECEfnHtnrfuHFjsW/fPr04tj59+ogTJ07oxalj2/oiBlRfMKAaxApE5Bfbnkh08OBBY9N71VVX6UWpw6BjJ643PzCgGsQKROQX1Hmb6r16xKkJeMRp2tm2vug8BlQ/MKAaxApE5Bfb6vybb74p71WatGXLlok2bdroxamD9WXLxWx0AQOqHxhQDWIFIvKHbYf34fbbbxf33XefXhwbLrwaNGiQXpw6WF+8xZR9GFD9wIBqECsQkT9srO+Y5qVLl+rFsXXp0kW8/vrrenHq2LjOiAHVFwyoBrECEfnBxtZTMDXNzZs3F9u3b9eLU8fU/JNZDKh+YEA1iBWIyA+o6zaey1i9enW9KBFNmzbVi1KHIcdetv4gpMIwoBrECkTkB1vr+sCBA/WiRAwbNkwvSh2EHBt/VBADqi8YUA1iBSJyn81fltdff71elIj33ntPL0odW9cZ2V3nKH8MqAahAhXS4ZnQ6NQve3Q4DMWrTInSy9YvysOHD4vLLrtML45t8uTJelEq2breiAHVFwyoBqECqYpUyi4q6BJRslSdsxWmPenHnHbq1EkvSiWb15vvGFD9wIBqkIkKpFpUg+FTBVI9qJruGITJZ+pLEn9t9f3338tW1Dlz5ui9irJy5UrRtm1bvTh1GHDsxvXnBwZUg9JcgfSgW66Qiy4s5DLoUtqp7dd2P/74o5yP3r17i/nz54sTJ06Is2fP6oPlpXXr1mLo0KF6ceq4su58xYDqBwZUg1iBKgfhcoXhqNZeBmEqFrYr1+zduzdTR9GyittQoUV01KhR8rGo2e5t+tprr4l+/frpxamk9glkJwZUPzCgGsQKlBw96DLkUjn59AWJJ00NGTJEPh2qfv36omHDhplW0s2bN4uTJ0/KYIrlgdZXG2BasQ7JTj7VP58xoBrECpQ+5Q66DLluwLr0NeBs2bJFjBs3LrNNV6lSRd73dP369fqgqcRwYz+uQz8woBrECuSvNARhhmAz+OVoN1VHyF6sg35gQDWIFYjiKlfQZUtvNLWMyE5cf/ZjQPUDA6pBrEBki3IEYVtDMKad7IX1h22N7MWA6gcGVINYgcg3pQ66esg1HXDV55K9LrrookrbUak7td2GdcEfbHqntm+98w0Dqh8YUA1iBSIqnB5yTQZdPRTk+sLHe8hu+jbAzt6O3MaAapCJCrRq1Srx1FNPiUmTJonp06fL27wcOHBAnDp1Sh+UiLIoNAircrJbth8gZAe2oPqBAdWgpCvQY489JsfZvn17cd9994nOnTuLOnXqZL5AW7RoIb9EH3nkEXnTbDwVBoEWIXb//v1WhNgnnnhCPPnkk+Ltt98WX375pXzKzaFDh/TBiMqC4Yao/BhQ/cCAalDSFahq1ario48+0ouzWr16tfj4449luL3zzjvFtddeKzp06CBatmwp6tWrJ6pVqyZvvN2tWzfxzDPP6G8vqWbNmolOnTqJmTNnijVr1ohPPvlETJ48WZbprVnoMPzAgQPFgw8+KF555RWxZMkS+SxwPO1m37594syZM/pHEBGR5RhQ/cCAalCSFej3339PdHxBOEVgxYoV4pprrhEjRozQe5fE4MGDZXguBFqGp06dKqe5Z8+eolevXvJpN61atRKNGjWS4btp06ZyvH//+99liywREdmNAdUPDKgGJV2Bkh5fGLSqItSV2h/+8AexbNkyvTi23bt3y0c14lniaIFGyyoREdmLAdUPDKgGJVmBcNg6yfFF2bt3b0k+J6hUn4nnhDdp0kQvLgk8rzwYjo8cOSI7noZARFQYBlQ/MKAalGQFwsVCSY4vytatW0WbNm30YqM2bNhQknmDSy+9VC8qCZzzi3kM6xo0aCCaN28urr76atGjRw/Zin3bbbfJnfD3339vxcVtRESlwoDqBwZUg5KsQJ9++qm8mMk0BOGuXbvqxUa9+OKL8hzSUmjcuLE4ePCgXmzU9ddfLy6//PKsQROtyL/99pt8jbsWfPjhh/KOBt27dxc1a9aUd25o27atvDAM/1988cUy9OKuDY8++qj44osvxA8//CDWrl2rjZmIyC0MqH5gQDUoyQq0cOFCGUZM++qrrxKd7nyMGzdO3HTTTXqxEbgt17Fjx/RiYxBKTSzP48ePi59++knejgtBtl+/fjK0BltmcdHYq6++qr+ViMhqDKh+YEA1KMkK9NJLL4mhQ4fqxYn797//XfJD/E8//bQYO3asXmwE1sm5c+f0YmNOnz6d6HaQL3XXB3R4oAMRkSsYUP3AgGpQkhXo66+/TnR8UXDLpocfflgvNgr3MZ04caJebAQOk5daKdZbFJw6UKtWLXlBFhGRCxhQ/cCAalDSFQjnhs6ePVveOsmUG264QcybN08vNgqHonFRUCmU6lzXoKS3g0Jdcskl8mEN5YYfWbgXLS6KCzp8+DDvZkBEeWNA9QMDqkEmKxBurP/444/Lp0PhkHyVKlVkEMHjTv/2t7/JVsmbb75Z/POf/5RPZtq4caM83JwNLh6qW7euvJK/lBC8sax++eUXvVfinn/+eb3IKDxuFueGllOfPn3EokWL9OKSwSN5cZHYW2+9JdatWyfLcNs0XPiH7RM/ULDdqVMSVIeLwdBv2LBh8klhCLe4EGzTpk3yR1q2i86IyF0MqH5gQDWo1BUIwRI3u3/vvffklfG4+Agtk/iSx833caN6PQQg1OLiKwQI/F+/fn19tMYhdOCzEVI+//xzeTX7+vXrZYjB06JwPmUSEGjmzp2rFxv12muvycfIltNll10mHx1bDnv27BEPPPBAUevw6NGjsuX3qaeekuNA6z7uahC8m0Ht2rVFx44dxT333CNPE8FFfsuXL5dBGJ9NRO5hQPUDA6pBaaxAuFk8AiFarxBmP/vsM/Htt9+K8ePHi7vuuksfvGQWLFggbr/9dnmfUj1Eo8PtoRCk//GPf4g333xTPh2qUMOHDxdTpkzRi40aOHCguPfee/XiksIPlGKWVxLGjBmTs+U+DpwegNZh3MkAd4KoUaNGpW0Ht2dDKzIuxEPQLWdrMpmH/YS+DaStcxm+T9Dp/we7IP3/fDCg+oEB1SBWoOTg1lA7d+6U5y/iXq1z5swRL7zwghg8eLBsoWzUqJG8hVTwULFqHb711lvl/9WqVdNHa1yHDh3ErFmz9OKSwtOz1KH1UktjHcAdMTBdeLoXucOW8IdAVsrpVJ+HDvtDFQj10FhIZ0IhobOQYcleDKgGsQKVFu4NiqvWlQMHDsidKVrX/vrXvxq9uCwKDkXjdIVyKud2WM7PzgbTNWHCBL2YLJbWbS0M9kuluK+1bUEO05pPALZtvqg4DKgGsQL5DfdbTcM2UM5pKOdnZ4NW7RtvvFEvJkshsOQTbNKkFHWjFJ+RtHymmQHVDwyoBrECEbaB/fv368UlVc7tsJyfnc3o0aNLfj4ymVOK1siklaJuuLpcGFD9wIBqECsQYRvAvWvLqXfv3npRSSxevFheoJVGV155Zcnv90vm2LivxTSbbvVlQCWbMaAaxApEuCUS7h5QTmgtLIdnn31WjBo1Si9OhXr16snH+pIbbNzXYpoRtEyydbnkwoDqBwZUg1iB6NVXX5XbAc5HLYeVK1fK++KWw4ABA8Qbb7yhF6cC1gke60tusHFfy4AaLp9pZkD1AwOqQaxABLjhfMuWLeVtrrBNBLvWrVvLoDR16lT5hKSk4fZbuEVXOeCetu+//75enApY9mxBdYeN+1oG1HD5TDMDqh8YUA1iBaKgsKcpbdmyRT5tCjeQx+kAeoBFh3Dbs2dPedP7l19+WZ7biZZRPL42GzxNrJyPWU3z9o9pM33+H5VOmre1KKXYBl1dLgyofmBANQgVCCepo7PxNiiUTrjBPJ4GNm3aNHHnnXfKC370UIvH2t5xxx3ySUvlgocUrF69Wi9OBSwjPPqW3GBjWCnFNJfiM5LGgEoKA6pBemiI6lSANX24h/yABxL88ssvenHJ4eIwhOi0QcDHU8bIDQgzNoaVUkxzKT4jaQyopDCgGqRXIFQ6VKxinhXNVliyzSuvvCLatWunF6fCtddeqxeRpRhQo5XiM5KGac71PceA6gcGVIOSqEAMtWSzQYMGyaCaNrgojdyRxL621EoxzaX4jKTlM80MqH5gQDWoVBVInR6gB9R8OhViiUzA4XRsZ507dxZjx44Va9asEYsWLRI//fST+Oabb8TatWvlXQ5KjU+Rckup9rVJKsU0l+IzkpbPNDOg+oEB1aC0ViDVKsuWWSqH06dPi7lz54pnnnlGXuTVoEGDStsYLvLCzfRxBwNcbIVD8nfddZcYP368+PLLL8XOnTv10eYN4zBxSy8qH2wztinFNJfiM5KWzzQzoPqBAdUgmytQMMTq4SFXxwBLxcDFXZs3bxarVq0S8+fPlw85GDZsmHxUa+3atSttZzVq1BAtWrQQ3bp1E/fff7+85yrubrBu3Tqxfft2ffQSWm3xXnKLjeu0FNNcis9IWj7TzIDqBwZUg1yvQDw/lkoJpwvs27dPtn7iPrBffPGFPFSPUwfQwtqkSZNK2xk6BFw87hXnw6JVdvbs2fqoyXJYz7YpxTSX4jOSls80M6D6gQHVIFagyhhqiZI1ffp00bdvX3HFFVeITp06iaZNm4qZM2fqgznNxn1CKb4fSvEZSco3eOY7HNmNAdUgVqDixAmxKsDa+IVFVIiDBw+K22+/XdSsWVO0b99e3pkAwRRPGLvqqqsydaJXr17iySefFHv27NFH4RSb9relujjVpmUCah+eCwOqHxhQDWIFKp24oZYts2Sb+vXrixkzZujFoXDXhKVLl4pJkyaJ6667LrPt43G4rsD8qPqfVtjHqGVfCqX8rCRg/eWDAdUPDKgGsQKliwqihYZYnlpAaYS7GxQLd1KAW265xan9lF5309qVcl+iPjOflslyKfSHBQOqHxhQDWIFskvwzgUMsZR2b731ll5UlCVLlohatWqJd999V+9lrXKc5qM+M+qzo8pLQd9fpbErZNkwoPqBAdUgViA/FHt6AUMtFWv58uV6USxoUcV5rA899JDeiyh1GFD9wIBqECsQQbEts8EAyxBLQbhtVtJWr14tH5BAlHYMqH5gQDWIFYjiKCbUolPvIzedOHHCyL7l0KFDRsZLlDQGVD8woBrECkQmBVtm9ZCarWPLrN3wlCxT+xZT4yVKEgOqHxhQDWIFonIr9vQCtsKm17Jly4ztW0yNlyhJDKh+YEA1iBWIbFRsyyxbZUujf//+4uabb9aLY3vkkUfE008/rRcTpU6ht6UiOzGgGsQKRK5SQbSQlll1agFbZuNp1qyZmDJlil4c2yWXXCJ27NihFxOlDgOqHxhQDWIFIqqo0JZZ3oqrojVr1sjl8sEHH+i9YjF5XitR0hhQ/cCAahArEFFh4rTM+hBicTN9zPOGDRv0XrH88MMP3F+RNRhQ/cCAahArEFHyCj1HNnhqQTDEop9tpxs8/PDDonbt2npxbK+++qro3LmzXkyUSgyofmBANYgViCgdsrXM2hRSGzduLG699Va9OLaWLVuKF154QS8mSiUGVD8woBrECkSUTiqc2gbTPGHCBL04Noz3u+++04uJUokB1Q8MqAaxAhGlk20tp4qpIInx7t27Vy8mB6mjCfl06tSYfDsEx2K74FGNfDtyGwOqQaxAROmDL1Jb6yam++jRo3pxLMeOHRM1a9bUi50xbNiwSmEon04PQ+zMdfqyz3cdkNsYUA1iBSJKH9RLG1tPp02bJnr37q0XxzZnzhzx4IMP6sVOUK16F110UaXWwUI7Sg8VXsltDKgGsQIRpYvNrafNmzcXo0eP1otjwzmt8+bN04udEDzsTO5gQPUDA6pBrEBE6WLzoUFMN1pRkzZo0CCxbt06vdgJqrXcxhZzisaA6gcGVINYgYjSxdbD+4BpN/Eo0jp16ohTp07pxU6w+QcJRWNA9QMDqkGsQETpgfMIba2TP/74o5FpX7p0qZHxpgUDqpsYUP3AgGoQKxBRetjcemrqC3nIkCFGxpsWppYblRfXqx8YUA1iBSJKB5svjgJMOw7FJ61Lly6ibdu2erEzbP5RQtEYUP3AgGoQKxBROth+qBfTfscdd+jFsTVr1kz0799fL3YGggxvEeUeBlQ/MKAaxApEVH6q9dTmWw1h+hcuXKgXx4bxvvjii3qxE2xvNadoDKh+YEA1iBWIqPxsbz0FTP/69ev14tgw3m+++UYvdgJDjLu4bv3AgGoQKxBR+dneegqYh9OnT+vFsRw4cEBcfPHFerEzXFjvFI4B1Q8MqAaxAhGVn+31cMGCBeKaa67Ri2P79NNPRbdu3fRiZ2C98wIpNzGg+oEB1SBWIKLycuHw/rhx44wEralTp4r77rtPL3aG7eudojGg+oEB1SBWoGThalzVqccXolNlRDoX6mCnTp30okTceuut4qOPPtKLncALpNzGgOoHBlSDWIGKh8CpdkKFdiZam8g+anuwXZMmTfSiRFx++eVi06ZNerETXFn3FI4B1Q8MqAaxAuUnLIzif9U6WgzVgsIvKn+5sO537dplZB42bNhgZLxp4cK6p2gMqH5gQDWIFSg79Wx0FUiLDaO5BMMqW1f94cL6njdvnpH9yAcffGBkvGnBgOo2BlQ/MKAaxAoUTu1cTIbSMGxV9Ycr5yAuXbrUyHyMHz9eVK9eXS92Buu42xhQ/cCAahAr0AXBcFgsdXFUEtS0JDU+ShdX1u0999wjHnnkEb04tj/+8Y9i9OjRerEzXFn/FI4B1Q8MqAaxAp2H5VDMDbPDzk3Vu7jnqqrxFPt+Sh9XWk/hyiuvFD/99JNeHBtu0D937ly92BmurH8Kx4DqBwZUg3yvQCr85UsPo4WG2rBAm+84uMNzR6HbXZphPvbs2aMXx4bxbt++XS92gks/UCgc99d+YEA1yNcKpL4g8gmHwQulCg2VuQTHmQ81LTw0aC+17bmyDuvXry82b96sF8eGZXTixAm92AkML+7jOvYDA6pBPlYgFfLyOWSuB9OkQ4U6Z7WQ81/VsPlMP6VPvuvZFpiXpFs6z5w5I3r37q0XOwPLLKkfuZRODKh+YEA1yLcKlG84CIbSUgfB4Gdnk88wlD5YZ0n/0Cmntm3bivbt2+vFsS1fvlwvckY59itUWgyofmBANcinCpTPeV/Blsxyt3DkE0DzGYbSI59t0DYzZsxIfJ4effRRvcgpSS8vSh8GVD8woBrkSwXKJ8ipYdLWsoFpytXilmveKB3yWZc2euGFF0S1atXE3Xffrfcq2Jtvvun09uzijxSqjAHVDwyoBvlQgXKFU3VOarlbTLPJNQ/cGaaf68Fk48aNYsiQIZltFd1f//pXMW3aNLFmzRqxd+9e/S2VLFiwQL5v/fr1ei9nsK76gevZDwyoBrlegdQFSFFsCg25plXdworSKdePDBfhYqdVq1aJwYMHi6uuuqpCeB02bJjYtm2bePzxx8V1110n6tatK+rVqyc2bNigj8YpPm4HPmJA9QMDqkEuV6BcgS5X/zTKNc3ol7ZTFOg8rJtsP5Z8cOzYMTFp0iTRp08fcdddd4mmTZuKOnXqiAYNGoi+ffvqgzuJAdUPDKh+YEA1yOUKhHmLalFUh/VtlO0Lzub5clmuHxbkj2z7JXIHA6ofGFANcrUC5Tq0b/t85wo82eadSo+tpwSq3vIoh/sYUP3AgGqQqxUo23xl62eTbC2pUeVUerl+TJA/GFr8wXXtBwZUg1ysQNlaq1ybX4bU9Mu2jsgv3Bb8wYDqBwZUg1yrQLlaq6KCq82i5hflPNetvNT2GLWOyC/cFvzBgOoHBlSDXKtAmJ+oEOravCpRO0JeMFV+DCQUlG3/RG6J2i+TWxhQDXKtAkXNT66WVdtFBaGwMiqdqPVCfuK24A8GVD8woBrkUgXKduW+S/MZJmpnyJv3lxfWCa/YJiWsjpKbovbJ5BYGVINcqkBR84JyH0JaVBiKWi5klgutpwcOHBC//fZbXo8ppexcP4pDFTGg+oEB1SBXKlC2nX9UuWuiloEvAT1ton4w2KJ58+aiWrVqmaCNrn///mLmzJn6oJQH1E/WQ38woPqBAdUgVyoQ5sPXw/tBYfPKHWXpqUBnq4kTJ8rpf/zxx8Xp06fF7t27xfr168WcOXPEoEGDKoRW1Ltt27bpoyBNtn0UuYf7XT8woBrkSgWKmo9s56W6CMshrJUmavmQGTYH1HXr1slp37dvn94r1EcffSRuueUW0bp1a1G/fv3MvN9+++1i06ZN+uDesnV7oOIwoPqBAdUgVypQ1Hyg3ObDrIWKCkZhZWQOlretP4zGjx8vWrVqpRfnDa2tO3fuFGPHjpXLYdGiRfogXmId9AsDqh8YUA1ypQJFhQE1f2pnERXgXBF179OwMjLD9m3syiuvFLNmzdKLi/bggw+Kr776Si/2StT54eQuBlQ/MKAa5EIFQigLayUNhjXsLPbs2SNfnzx50ulz5sLWaVSAp+Rh+du8vMO2nzhwDmvS47QNA6p/GFD9wIBqkAsVKCoMBL8UggH1zJkzMqD27dtXPPTQQ3KY4cOHy37t2rUTTZs2FSNHjpT/47w6mDRpkpg7d658reB9l1xyiWjUqJFYs2ZNpuVM3ZIH48RV0LioBMNUrVpVhuOwcXTv3l3+f9ddd4kmTZqIwYMHy8/GZ7744ouyX4sWLcSxY8dE165d5bjQhQlbp1EhnpLlQhAxMf3Vq1eXF175Su0byB8MqH5gQDXIhQoUNQ/BLwXsLIYMGSJD4+zZszNlKrRiuDFjxoiXX35Z9sew6IdQCHpAnTdvXuY11K5dO/NafSbeE/w/WKasXbtWfl6DBg1kyB06dKgsRxDFZ+Mz1XtUQD137pw8ZIppXLlyZXB0UtTyCLt4ipJlexB57733xGWXXaYXx1anTh1x9dVX68XewDbB+ucXF36sUm4MqAa5UIGi5kEPqCqMKnpAxcUhCIlBUQF1xYoVmdeAFiIl34CKVlzVoorP2bVrl+jXr5/8H6+jAmqNGjXk/ygPC6hRX4RR5ZQM9YUU1aJvg2HDholp06bpxbHh/qkvvfSSXuwN27cLKhwDqh8YUA1yoQJFzUMwoOJw/pEjRyr0x2H0/fv3y9cqYE6ePFm+RwXFcePGyf/xpb1gwYLMe6Fx48ayH65WPn78uDzkjrKzZ8/K/urQvAqU8NZbb2VeA95fr169zCH+o0ePik6dOsnTBNq3by/L2rRpIzsMc+rUqcw0vvbaa5UCNWDHGHY4nwHVrOD2ZiuckmKC7cslLt/n30cMqH5gQDXIhQoUNQ82BgacYgDPPvusmDBhgtY3Pwyo5YFtzfZWMjw9ygTb6mHSfJ9/HzGg+oEB1SAXKlDUPNgYUNESi/Nk9dbaQjCglp4rX0ZdunTRixLhwrKJw/f595Er+wTKjgHVIBcqUNQ82BhQkxAVRKPKKT4XWk9xWkmxrfbZvPvuu/JuGb7CdmH7tkGFY0D1AwOqQS5UoKh58DWgRs0zvyTNcOWLaPXq1WLZsmV6cWy9e/cWq1at0ou9gW0j7IgGuc2V/QJlx4BqkAsVKGoefL0PXdQ8M6Ca4coPoeeee04vSoQLyyYO3+ffVwyofmBANciFChQ1D1GP/XRd2DzzRv3muBJQ//KXv+hFiXBh2cTh+/z7igHVDwyoBrlQgaIuCgIX5q8QUTtFnn9qRtTyts2OHTuMzMf7779vZLy28PVHMrmzb6DsGFANcqUCRQUwzF9UeHVRVGteWBnFF7W8bfP5558bmQ/coN/nJ0gxpPiL694PDKgGuVKBouYDwTUqvLoIyyHsXNOo5UPxuLJcn3nmGSPzgnGqh174yJUfMFQ4BlQ/MKAa5EoFwnxEtZSWch7LHYbD5pU7SjNcCh/Ybv/2t7/pxbE1aNBAfPbZZ3qxN6J+MJL7uN/1AwOqQa5UIMxHVDjM1i9p5Qwt2CGGfRnyS9KMcq7rpCFI4n6lSatSpYp8PK+vsv1wJrcxoPqBAdUglypQ1LyU6nZT5d4hRX12VDkVr9zrOmmYl40bN+rFsbVu3Vov8opL2wgVxrV9BIVjQDXIpQqUbV5K0dqF8ZerpTJbCC9V67FPSrE9lRLmZefOnXpxbNdcc41e5BWXthEqDAOqHxhQDXKpAuFQWlRANB0oyn07mahwHFZG8UUtbxsdPXpUNGrUSC+ODS2yDz/8sF7slXLuE6i8GFD94GxAnTt3rhgyZIiYPXu27HAvwlJzrQJlmx+TIdXkuHOJOvcUyjVNLivnujYBjzjFfihpU6ZMEVu2bNGLvZGtXpL7GFD94HRAnTRpkl5cUq5VoGyHugH9TFy0UK4WtWwtt+WaJte5FlA//PBDcdNNN+nFsd177716kVd4ao3fGFD94HRArVOnjmjRooXsNmzYIKpWrSr7TZ06VYwdO1acPHlSfPDBB7LsP//5j5gwYYI4d+6c/L9GjRqZcRXLxQqULZiZCBfl2hGpzw2b13JNk+tMbD/lhiD1xRdf6MWxdejQQS/yimvbCRWG+2A/OB1Q9RbU8ePHy78IoXXr1hXz5s2r0L9hw4aZ1xMnTgz0KY6rFSjbfP3PpR1ll5SokGhats/NNv9UPNcC6pkzZ+T84G+Sjh8/LmrVqqUXe8Wl7YQKx4DqB68C6m233Sb/4sIFtKquWbOmwpdH8LYtSZw35moFQqtQVHjr9dyP4v8N/Vp2eB1XOZZhtp0f+kXNO8WT7UeBjXbt2hW5HcWxbds20apVK73YKyaWK9kj2z6a3OFsQMX5gy+++GKFskWLFsmNukePHpmydu3aybJNmzbJ/1UrzsKFCzPDFMvlCpQtTKiAipZU9frJOdv0wXLKdc6rCWr9h+FO0Zxsy91WixcvNjJPc+bMETfeeKNe7A3WQ+I24AdnA2oauFyBsl1ABCqYqhBbTFAtdWjJtdPL1o/iyfaDx1Y4p93ELaZGjhwpHnroIb3YG+X44UrpkmtfTW5gQDXIhwoUNo/BQ/zqdVS3cMNB/e1SUjsg/fPQhckVhrP1o3hyLXtbNW7cWHTv3l0vjuX333+Xjzg9ceKE3ssbLv6YocIk9f1A6caAapAPFSgsXKhgCgigwWAYFVpRHmxdDRtvIYKfq8YPeI3PCQbVXJ+VrR/Fl2v52+jgwYNyntDamaQ9e/Y4t6wK5fv8EwOqLxhQDfKlAoUFjGAoVP8Hw2GuDuMbNvofgTHmTx8XPhOdfgFXcJgomA4T93alC7CMXbuv5fr16+V84XzRJOHG/3pd843v808MqL5gQM2TOu+pkMDiUwUKOy9MD4EIiX8a+3alABnWYVx6mRpH2GkBekutCsJ6S2qww0Vc2NGp4YLC5oeS5+oXzZtvvikP8ScNt7+74YYb9GJvuLq9UGG4HfiBAbUAKqAGu2znQvlWgdQyCQoGSz0gRnUYx/90vrtSea5OnSagB2O9++9+L1SYTpQFAyrDaelgObvWegq4iKlv3756cWy9e/cWM2bM0Iu9gW3Fxe2FCsOA6gcG1AKpSqHCmLonaLCFVYVWHyuQ2nHoXyLBgJgrrOL9ellSXdgPCpSDujOBPu1khstfMpivpUuX6sWxubq88oX5z/cIFrnL5X0HXcCAWiAVQoOvoypKVLnrsi0XFRSjQipaTk0F1KgHB6Cfml5++ZVO1DbiAszXli1b9OLYXF1e+fJ9/uk8BlQ/MKAWQa8YUV+0YWW+UDsQdHqrZVQ4xfmp6j3B8qv/saDSsGGdOnSPruMN508RUOegZgun/93pfChmOC2d4PbhIlPzZWq8tvB9/uk8BlQ/MKAWSQ806vBwMJCxAmUPqqCfL6qHUxUu9TJ0CK5hF03pw+v9AdOi+lPpuRxOT548KQYOHKgXx7Z161Z583+fubrNUGEYUP3AgFqkqC/YYBgL6+8rtVzCgqoKir3vGptZZlGtrEl3VB5R9ccFK1euFKNGjdKLY9u/f7+R0wZsou87yE/8fvUDA2oM2SpJVBjzXbBFVV8++F8Fx7CW0TAYBp1qNQ07lB9s3eZh/PLLVm9csHDhQtGjRw+9OLbJkyfrRV7hvpQU1/chdB4DakyoKFE7zmAQo4r0oBpcTlGtp+qG+1G3kgoG2mAoZTBNF9frBA7F16tXTxw5ckTvFUunTp30Iq9E7WfJPwyofmBANUhVID2AUWV6UA3rcGN9XPyELnhbL71jGE0vX75YMI8NGjTQi4uG0wZ8WG7Z+D7/dIEv+xHfMaAaFKxAwRY9qihsZ4MydVNuPYCiU/0YRu3iSx345JNP5JOk6tatK1/DiRMnxNmzZ7Uh83Pdddd5sdyy8X3+6YKw7wxyDwOqQWEVSH1B83DVBb6EFvJvXb///vuiW7ducp4bNmwoqlSpIsPmI488Ip8ItXfvXv0tlZw6dUq+v1GjRnovr/i03VB2DKh+YEA1KKoCBc+/ZFCNXk7kHq7rC9Cy2qxZs8y+oHnz5mLYsGFixYoV4t///reYPn26aNmypTyftXr16mLXrl36KLzBQEJB3B78wIBqUK4KpL6Ycg3nsmwXmZFbfN/Woxw6dEhs2LBBvP3222LcuHEV9guXXnqpDKfbt2/X3+YV9UhpImBA9QMDqkH5ViCfg6qP8+wrrmsqFrcdCmJA9QMDqkGFVKDgYX9fcCfjD9+2bUoWtx0K4neHHxhQDSqmAqkvch8OZ/kyn8SASvFw26EgBlQ/MKAaFKcC+RBU4ywfsocP2zKZhXNQyQzcqg91U926L+zWfsF+6roBvK9ct/ljQPUDA6pBcStQcAfhGhfnicK5ug1T6fDHTTL0J+yZ7FSYNRFiGVD9wIBqUFIVKFjpXcCdi1+wrhkwqFgmAo4vogKp6RbpYKus/tl6V0yQ5XeIHxhQDUqyAqkKic72L/sklwulG79IKC7b93flosKpqoOmQ2khMG25wmu2W4sVu1+JGh+lEwOqQcVUoFyCFdhWNk87FUZ9QRIVi/uLwulBzxaq5VUPq/r84IEWxWwXwfFQ+jGgGmSyEtha0WybXipesa0cREHchgqD5eVy3UOIbdWqVYXvwGCX7+kCNn5/+oYB1aBSbPzBipl2mEabfs1TPLZsl5Ru3Ibypw6Lu77M1OkLwf+ztbxGhVb1Hh7lSScGVINKuZMIVsY04k7AP1znFBe2H/6ozU/wFlCu0wNqLrkCrDofNp9lN3ny5Mz7PvzwQ1l24403yr/ff/+9OHDgQGDo9Ktbt65elBoMqAYVUoGSEKyA+VS0Uir1sqDy8qEVh8zzJXAlQe37fVBoQI2ifgDpgTXKmTNnxJAhQzL/T5w4Uf61+UdU7dq19aLUYEA1KNuGblI+Fa2U0jIdVDpp2v7IXtyG8udTmI8TUKPuIJBPyJw0aZJeJKn3Hjt2TJw7d06+7tWrl6hTp4744YcfZKtqp06dZJlu8eLFcrgBAwbovUS/fv1kv/Xr18txjB8/XpbPmzdPLFmyRL5n9+7dmWFg586dom3bthXG98svv4g2bdqIjz/+WP6PoI3/O3TokAmoJ0+eFB07dhTNmjXLvK/cGFANKrYCJSUNLar47LBzf8hdvrWeHj9+XIwYMUI0aNBAjBw5UixbtkwfhIrk03YUh28/CIsJqMEgWux3UlSIVeV79uyRIXXMmDHaEOehvzodQFm5cmXm9bZt2zKvVRiFN954Q/5dvny5uOGGGzJBs0WLFmLXrl3y9W233VZhXHitpmPp0qWZ4TF9VapUyQynlmNw3jCuNGBANajQCmRKuYKqT7/o6QJfviz37dsnrybGzn7atGkymP7rX/8SXbp0EY0aNZKtEbfccouYP3+++P333/W3Ux582I7i8u0HIRQTUJOQqwVVBdS+fftW6N+zZ085DPYTwSCIYXHKwPDhw2WHfYqC4VQ5OgXzjRZQUIETMG1z586V+6SBAwfK/4cOHSr7qeCqhm/evPn5kYkLh/jxPvVZb7/9dqZ/OTGgGlSOCpSLCg+mw6r6pUp+UV+WPqz7WrVqyZ36pk2b9F4Zhw4dkhdSPPHEE/JLo2bNmpn6d+2118qLKiicOj+QsvPlB2FQuQIq4HNRlxHk1DTg7+zZszMBFQESw6DFE4fM8YMV04xgqG/TeO+CBQvEO++8I9+vqHFs3rxZdO7cWY63atWqsh+O1uzYsUMGTryeNWtWZlowzJo1a0Tjxo0jAyoC9ODBg+WP6urVq8t+V1xxhWy1xTSr95UbA6pB5apAuQRbVE0EVRPjJDv48mWJw2r16tXTi/Py66+/yr/4UmrSpIlswRg9erQ2FOGLnPuR3Hypc0HlDKhw6tSpTD0GnHeqX71/9uzZCmXqtWr9DEI/jFOH8QZDqzq/FTB+FTiDLa+g/scwQcHPwGt0R48ezZRh2vRxlRMDqkHlrED50INq3OlVO41iz+8h+yWxHdkAh/JHjRqlFxcFrRsIu9u3b9d7ec2H7SgJPjYIlDugpkXwEL+LGFANsq0ChQXWXDu+4BWR+qEL8otPXxpJzye+ZKpVqya+/vprvZe3kl7GrvJxOfm0r/EZA6pBtlYgVP6wsJqtY6spqW3BBybmE+PEOWd0noll7BofL5ACBlQ/MKAa5GIFYhClKC5u72FOnDghWzuThuXXtGlTvdhbPCKTG7aZXEe5XMSA6gcGVINYgcgXPrWefvrpp6J///56cWwYJ+6jSuf5GLwK5Uud0zGg+oEB1SBWIPKFTwEVN8rG/QaT5svyywfDaX583WYYUP3AgGoQKxD5wqdDjXg84M8//6wXx8b9xQU8vJ8fX7cZBlQ/MKAaxApEPvDtQg3cdDt4D8Sk+LQMc+GyyM3nkObzvPuEAdUgViDygU+tp9CyZUu9KDbcjBuPPKTzuO/MzbcfhkEMqH5gQDWIFYhc5+OX5M0336wXxbZq1Soxb948vdhbvm1TxfCx7ikMqH5gQDWIFYhch23cp+0cLZ2vvvqqXhzb008/rRd5y+fgVQicp+vruboMqH5gQDWIFYhc59vhfbRyLl68WC+OrXfv3nqRt3zbporFgMrvV9cxoBrECkQu87Gl6/nnnxdHjhzRi2M5ffq0d8sxGywLPhAkN4RTX4M8A6ofGFANYgUil/nY0tW6dWu9KLYNGzZwXxHAZZEfH+ufwoDqBwZUg1iByFU+tp5C7dq19aLY/vnPf3q5LKNwWeTH55ZmBlQ/MKAaxApErsK27dv2/cknn4guXbroxbH16tVLDBs2TC/2lq/nVRaKAdWv/Y+PGFANYgUiF6nWU98OL44cOVLcc889enFsdevWFTNnztSLveXbdlUsBlR+v7qOAdUgViBykY+tp9CtWzfx0ksv6cWxbNq0SS5L3AeVzodTX0NXoRhQ/dsH+YYB1SBWIHKRj62n8Ic//EGsW7dOL45l6tSpcnkePXpU7+Ul7jPzx4DKbcV1DKgGsQKRa3z9Yjhw4ICoWbOmXhzb/fffb+TOALbycdsqFgMqtxXXMaAaxApErsE27eNFLM8884wYOHCgXhxbq1at5L1V6TzuM/Pnc0AFbivuY0A1iBWIXOPrNo1Q/uSTT+rFsWF5fvfdd3qxl9gqVhgsKx9PtVG4rbiPAdUgViByCbZnX7fpdu3aiaVLl+rFsWF5bt26VS/2kq/31i0WAyq3FdcxoBrECkQu8TmgVq9eXfz22296cSyHDx8WNWrU0Iu95XvgKhSWl4+n2yi+7ot8woBqECsQuQTbs6/nvOEWU0n76KOPxIABA/Rib/m8fRWDAZXfr65jQDWIFYhc4XPrKUyfPl0viq1Pnz7i888/14u9lYbtC4FPbevs4ncmmR4/lR8DqkGsQOQKbMu+Hn5dtGiRWLlypV4c21VXXSXWrl2rF3up3OefBoOpLdI+vWqdmmoVT/O8UzIYUA1iBSIXlDs8lNt9992nFyWiYcOGYt++fXqxl1RALBcbf4ClPaCCyX2HqfFSejCgGsQKRC6w4YvQJBPzvmzZMiPjtVU5A2K5PjcuW+qlqcfX2jDvFA8DqkGsQGQ71QJi65d4EkzU4wkTJhgZr62wLEyEmHzYuh5sCahgYv9hy7xT8RhQDWIFItvZ9CVowu7du+Wh+KTdfffdfMRpQDm3sXJ+dhw21U0T02linJQuDKgGsQKR7bANm2j9sMVzzz0nxo4dqxfH1r17dzFixAi92Fvl3FfaeqsmBtTkx0npwoBqECsQ2azcF66kAeb/zJkzenFsGC8fcXpBObezXAG1Vq1amTB49uxZvbf49ddfxY033qgXZ5hogQefAyp+NOdab2Q/BlSDkq6URKWE7dfn1lMwVYcxXj7i9DxsY+XczrJ99siRI8XRo0cz/y9evDjQ97w9e/ZkDUu1a9fWixKBbSjb56YJpjXJc4wx39nWG7mBAdUgU19uRKaZvD2MTUwtA4z3+PHjerGXEDaSDC+FyhbyotZ/mzZtZHfy5MkKAfW6667LDNOpUyf5VwVUtMT369dPtG3bVr7Go26ffPLJom9jVu5gX4ikA2rS46N0YkA1KGrnRpR22Ha5/QrRo0cPvSi2H3/8keefBpR7O4sKqMeOHQtt/WzSpEnmdfv27SsE1BYtWmT6qfkKGwcO++N97dq103s5KelAWe5thkqDAdWgqB0fUdrhC8CW1hmT+vfvrxfFhoDK808vKHfYyPb5Yf1QNnz48EyXb0B96qmnZKvrrFmzZFmuUwNcErYc40h6fJRODKiG+bIDInfw8P4Fl19+uV4UG4IKnYdWtXJva9k+/8iRI6Jq1aoyiPbp00e89NJLYvny5WLKlCly2qtXry4P12McO3bsEPfee6+48847RceOHTPjxd+ZM2eKm266Sb4Pd3BgQC2eTac2UDwMqIYlWTGJSgHbrC9fnLkggKxevVovjuX666/Xi7yVhh9D+Xw+wmTwbg6nTp0SBw4cyPx/7tw52QFODYDgxVW40j/Y78SJE/KviTtEpFE+yzhfSY6L0o0B1TBUpiTPvSEyjV8AF7Rq1Uo0bdpULy7a6dOnuXwDsCzKvTzK/fk+SGoZp+EHDZUOA2oJMKSSLdIQGNIGV1ljmQwaNCh2a+qAAQO4fAOwLMrdWs/1YV5Syzip8ZAdGFBLgL/6yBYMqOGmTp0qunbtKpcNLoR54oknZPnGjRvl7YLygeHwftxaiM7D8ij3+YTc3s1LYhmnYVuh0mJALSFWMCL7/f7772Lbtm1i3Lhx8ip/nAKggj1C7GOPPZYZFuca7t27V8ydO1f2HzNmTGBMlERwiYv7ZLPifu+l4UI6Kg8G1BJjCxWRe3Dof/r06eLZZ5+Vh6xRx6tVqyYvssJV4C1btpS3l6KK0rIvTMt0uCZuuFRHH+MEXLIXA2oZqEqnKh4qcVLnqKpxlaNTt/8oZ4dwkKZOrWd27Nixi9vp+5diO32/WWin7/ujOjXdeJ0P/X3oyF8MqFSJvpNhx86VjigXBDA9GLIrrmOdozgYUImIiIgoVRhQiYiIiChVGFCJiIiIKFUYUImIiIgoVRhQiYiIiChVGFCJiIiIKFUYUImIiIgoVRhQiYiIiChVGFCJiIiIKFUYUImIiIgoVRhQiYiIiChV/j+ZlhGztUd9IwAAAABJRU5ErkJggg==>
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
