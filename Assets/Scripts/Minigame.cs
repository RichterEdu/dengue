using UnityEngine;

/* ============================================================
 *  Minigame.cs
 *  ---------------------------------------------------------------------
 *  Base comum dos minigames dos focos (lógica pura, sem MonoBehaviour e
 *  sem UnityEngine.Time: o tempo entra pelo dt, testável no EditMode).
 *
 *  - O foco cria a sessão (Interactable.CriarMinigame); null = o segurar
 *    de sempre. Quem conduz é o Interactor.cs: a cada quadro monta a
 *    EntradaDoMinigame e chama Atualizar.
 *  - Atualizar devolve no máximo um Passo por quadro. Cada Passo vira um
 *    AvancarCom(kit) do foco; a sessão não sabe quando o foco termina,
 *    quem diz é o AvancarCom.
 *  - Errou chama Interactable.AoErrarMinigame (só o ralo gasta carga).
 *  - FracaoDoPasso (0..1) é o quanto do passo atual já foi feito, para a
 *    barra da HUD. PodeSair = o botão de largar sai do minigame.
 *  - IMinigameComMira: sessão que mexe no olhar do jogador. Mira é em
 *    graus (yaw, pitch), a partir do olhar do começo do minigame.
 *
 *  O skill check do ralo entra por um adaptador (MinigameDeSkillCheck,
 *  em SkillCheck.cs).
 * ============================================================ */

/* O que o jogador fez neste quadro, já lido pelo Interactor. */
public struct EntradaDoMinigame
{
    public Vector2 Olhar;      // delta do mouse / analógico direito (EntradaJogador.Olhar), em graus
    public bool Segurando;     // botão de interagir mantido
    public bool Apertou;       // botão de interagir apertado neste quadro
}

public enum EstadoDoMinigame
{
    EmAndamento,
    Passo,
    Errou
}

public interface IMinigame
{
    EstadoDoMinigame Atualizar(float dt, EntradaDoMinigame entrada);

    /* 0..1 do passo atual, para a barra da HUD. */
    float FracaoDoPasso { get; }

    /* O botão de largar sai do minigame (ralo: false). */
    bool PodeSair { get; }
}

/* Sessão que aponta o olhar do jogador enquanto está ativa. */
public interface IMinigameComMira
{
    /* Graus (yaw, pitch) a partir do olhar inicial. */
    Vector2 Mira { get; }
}
