# Spec: Reposicionamento da Barra de Ocupação Hospitalar na HUD

## 1. Objetivo
Reposicionar a barra de Ocupação Hospitalar (`OcupacaoPanel`) do centro inferior da tela para o centro superior, desobstruindo a Hotbar (`KitHUD`) e evitando sobreposição de elementos visuais na interface.

## 2. Layout & Coordenadas
- **Resolução de referência do Canvas:** 1920×1080.
- **OcupacaoPanel (DengueHUD):**
  - Anchor Min: `(0.5, 1.0)`
  - Anchor Max: `(0.5, 1.0)`
  - Pivot: `(0.5, 1.0)`
  - Anchored Position: `(0, -32)`
  - Size Delta: `(560, 84)`
- **ToastPanel (Aviso de eliminação de foco e dicas):**
  - Anchor Min: `(0.5, 1.0)`
  - Anchor Max: `(0.5, 1.0)`
  - Pivot: `(0.5, 1.0)`
  - Anchored Position: `(0, -126)`
  - Size Delta: `(700, 72)`

## 3. Comportamento e Compatibilidade
- Mantém o estilo sóbrio (fundo escuro, contorno sutil, sem moldura neon).
- Mantém todas as conexões de eventos e bindings de dados existentes com `FluxoDeJogoHUD` e `DengueHUD`.
- Atualiza os builders do Editor (`FluxoDeJogoHUDBuilder`, `DengueHUDBuilder`, `DengueHUDReparo`) para que novas gerações ou reparações mantenham esse layout padronizado.
