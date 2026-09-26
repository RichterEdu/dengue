---
name: preview-design
description: Use SEMPRE que o usuário pedir um plano, proposta ou ideia que mude a interface gráfica (HUD, menus, celular/rede social, balões de fala, barras, textos na tela) ou o visual/design do jogo (cores, materiais, iluminação, layout de cena, personagens, efeitos). Gera um preview renderizado DENTRO DO UNITY (screenshot da Game view) antes de implementar.
---

# Preview de mudanças visuais / de UI (renderizado no Unity)

Quando o usuário pedir um **plano** que altere UI ou design, o plano só está completo com
um **screenshot real da Game view do Unity** mostrando como vai ficar. Não implemente de
forma definitiva antes da aprovação. Caminhos relativos à raiz do projeto.

## Fluxo (agent path)

1. **Checar o Editor:** `unity command editor_status` → precisa estar conectado.
   Se `playMode` não for `playing`, rode `unity command editor_play` (em Play Mode tudo que
   for criado/alterado é **descartado automaticamente ao sair** — é o que torna o preview seguro).
2. **Capturar o "antes":**
   ```bash
   unity command screenshot '{"view":"game"}'
   ```
   Retorna JSON com `path` (fica em `Temp/pipeline-screenshots/screenshot_game_*.png`).
3. **Aplicar a mudança TEMPORÁRIA** escrevendo C# num arquivo no scratchpad e rodando:
   ```bash
   unity command eval_file --file "<scratchpad>/preview.cs"
   ```
   Agrupe tudo sob um GameObject raiz `__Preview` para limpar fácil. Exemplo de UI:
   ```csharp
   var go = new GameObject("__Preview");
   var c = go.AddComponent<Canvas>();
   c.renderMode = RenderMode.ScreenSpaceCamera; c.worldCamera = Camera.main; c.planeDistance = 0.5f;
   var img = new GameObject("box").AddComponent<UnityEngine.UI.Image>();
   img.transform.SetParent(go.transform, false);
   img.color = new Color(0.25f, 1f, 0.8f, 0.8f);
   img.rectTransform.sizeDelta = new Vector2(300, 80);
   return "ok";
   ```
   Para design de cena: altere materiais/luzes/posições dos objetos reais direto (Play Mode reverte).
   Se o plano cria scripts novos, prefira simular o resultado visual via eval em vez de compilar.
4. **Capturar o "depois"** (mesmo comando do passo 2) e **abrir a imagem com Read** para
   conferir que a mudança aparece. Se houver variações, repita 3–4 por variação.
5. **Mostrar ao usuário** os PNGs (antes × depois) com `SendUserFile` (`display: "render"`),
   seguido do plano em texto (arquivos a alterar, passos) e pedir aprovação.
6. **Limpar:** `Object.Destroy(GameObject.Find("__Preview")); return "ok";` via eval_file,
   ou simplesmente `unity command editor_stop`. Nunca rode `save_scene` durante o preview.
7. Após aprovação: implemente de verdade e registre em `contexto do jogo.md` (regra do AGENTS.md).

## Gotchas (verificados)
- **Canvas `ScreenSpaceOverlay` NÃO aparece no `screenshot`** (a captura renderiza só a câmera).
  Use sempre `ScreenSpaceCamera` com `worldCamera = Camera.main`. Pelo mesmo motivo, o HUD
  atual feito com `OnGUI` (`Interactor`, `FluxoDeJogoHUD`) não aparece na captura — para
  comparar com ele, recrie os elementos no Canvas de preview ou avise o usuário.
- `unity command eval '{"code":"..."}'` quebra com aspas/escapes; use **`eval_file`**.
- A limpeza com `Object.Destroy(...)` imprime o aviso "Unreachable code detected", mas funciona (confirmado com `GameObject.Find("__Preview") == null`).
- A câmera segue o player, então o enquadramento muda entre capturas se o jogo andar;
  se precisar de ângulo fixo, posicione o Player/câmera via eval antes das duas capturas.
- Play Mode pode "congelar" com a janela sem foco (`Time.frameCount` parado) — veja
  o histórico em `contexto do jogo.md`; reentre em Play se necessário.
- **Fallback** (Unity fechado/desconectado): avise e faça um mockup com `show_widget`,
  deixando claro que é aproximado.
