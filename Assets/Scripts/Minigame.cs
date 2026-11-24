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
 *  - Errou chama Interactable.AoErrarMinigame (só o bueiro gasta carga).
 *  - FracaoDoPasso (0..1) é o quanto do passo atual já foi feito, para a
 *    barra da HUD. PodeSair = o botão de largar sai do minigame.
 *  - Agindo = a sessão faz a sua ação neste quadro (o Avancando do
 *    Interactor: som de ação e o item na mão).
 *  - EntradaDoMinigame.Ponteiro: o mesmo movimento do Olhar, sem o
 *    "inverter Y" (+x = direita, +y = para cima na tela), para sessões
 *    que mexem um ponteiro na tela em vez do olhar.
 *  - PoseDePerto: pose de câmera de uma vista de perto (posição, giro e
 *    FOV vertical). NoCampo diz onde um ponto do mundo cai no campo da
 *    tela: y de −1 (base) a +1 (topo), x na mesma unidade (de −proporção
 *    a +proporção), centro em (0, 0).
 *  - IMinigameDePerto: sessão que quer a câmera nessa pose enquanto durar.
 *
 *  O skill check do bueiro entra por um adaptador (MinigameDeSkillCheck,
 *  em SkillCheck.cs).
 * ============================================================ */

/* O que o jogador fez neste quadro, já lido pelo Interactor. */
public struct EntradaDoMinigame
{
    public Vector2 Olhar;      // delta do mouse / analógico direito (EntradaJogador.Olhar), em graus
    public Vector2 Ponteiro;   // o mesmo movimento, sem o "inverter Y": +x = direita, +y = para cima na tela
    public bool Segurando;    // botão de interagir mantido
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

    /* O botão de largar sai do minigame (bueiro: false). */
    bool PodeSair { get; }

    /* A sessão está fazendo a sua ação neste quadro: vira o Avancando do
     * Interactor (som de ação no SomDoKit, gesto e partículas do item na
     * mão no ItensNaMao). */
    bool Agindo { get; }
}

/* Pose de câmera de uma vista de perto. */
public struct PoseDePerto
{
    /* Profundidade mínima (1 cm) para um ponto contar como à frente da câmera. */
    private const float ProfundidadeMinima = 0.01f;

    public Vector3 Posicao;
    public Quaternion Giro;
    public float Fov;          // vertical, em graus

    /* Onde um ponto do mundo cai no campo desta pose. Atrás da câmera
     * (profundidade menor que 1 cm): Vector2.zero. */
    public Vector2 NoCampo(Vector3 mundo)
    {
        Vector3 l = Quaternion.Inverse(Giro) * (mundo - Posicao);
        if (!(l.z >= ProfundidadeMinima)) return Vector2.zero;
        float escala = l.z * Mathf.Tan(Fov * 0.5f * Mathf.Deg2Rad);
        return new Vector2(l.x, l.y) / escala;
    }
}

/* Sessão que quer a câmera numa pose enquanto durar. */
public interface IMinigameDePerto
{
    PoseDePerto Pose { get; }
}
