using System;

/* ============================================================
 *  SessaoDeSegurar.cs
 *  ---------------------------------------------------------------------
 *  Converte o tempo com o botão segurado em passos de ação (Parte A).
 *  Classe C# pura, sem MonoBehaviour, para dar pra testar no EditMode;
 *  quem chama o Atualizar a cada quadro é o KitDoAgente.
 *
 *  Cada passo vira um Interact() do foco, então o progresso de verdade
 *  fica no foco. Aqui só conta o tempo do passo atual:
 *
 *    - acumula dt enquanto segura o MESMO alvo e devolve quantos passos
 *      inteiros fechou nesse quadro, guardando o resto para o próximo;
 *    - soltar o botão, mirar no nada ou trocar de alvo zera a contagem
 *      (o tempo de um foco não passa para outro);
 *    - segundosPorPasso <= 0 é passo instantâneo: um passo por aperto,
 *      e para dar outro é preciso soltar ou trocar de alvo.
 *
 *  Fracao (0..1) é quanto do passo atual já foi segurado, para a barra
 *  de progresso da HUD.
 * ============================================================ */

public class SessaoDeSegurar
{
    private float acumulado;             // segundos segurados dentro do passo atual
    private bool passoInstantaneoDado;   // segundosPorPasso <= 0: já deu o passo deste aperto

    /* Alvo segurado agora (null quando a sessão está zerada). */
    public object Alvo { get; private set; }

    /* Quanto do passo atual já foi segurado (0..1). */
    public float Fracao { get; private set; }

    /* Chamar a cada quadro. Devolve quantos passos fechou neste quadro. */
    public int Atualizar(object alvo, bool segurando, float dt, float segundosPorPasso)
    {
        // Comparação por referência: é o mesmo foco, não um "igual" a ele.
        if (alvo == null || !segurando || !ReferenceEquals(alvo, Alvo))
            Zerar();

        if (alvo == null || !segurando)
            return 0;

        Alvo = alvo;

        if (segundosPorPasso <= 0f)
        {
            Fracao = 0f;   // passo instantâneo não tem barra para encher
            if (passoInstantaneoDado)
                return 0;

            passoInstantaneoDado = true;
            return 1;
        }

        if (dt > 0f)
            acumulado += dt;

        int passos = (int)Math.Floor(acumulado / segundosPorPasso);
        // Max: o arredondamento do float não pode deixar o resto negativo.
        acumulado = Math.Max(0f, acumulado - passos * segundosPorPasso);
        Fracao = Math.Min(1f, acumulado / segundosPorPasso);
        return passos;
    }

    /* Volta ao estado inicial: sem alvo, sem tempo acumulado. */
    public void Zerar()
    {
        Alvo = null;
        Fracao = 0f;
        acumulado = 0f;
        passoInstantaneoDado = false;
    }
}
