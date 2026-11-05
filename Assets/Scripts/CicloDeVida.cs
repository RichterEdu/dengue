/* ============================================================
 *  CicloDeVida.cs
 *  ---------------------------------------------------------------------
 *  O relógio de vida de um foco (Parte B): ovo → larva → mosquito.
 *  Classe C# pura, sem MonoBehaviour, para dar pra testar no EditMode;
 *  quem guarda um ciclo por foco e chama o Avancar a cada quadro é o
 *  GerenciadorDeFocos.
 *
 *    - cada estágio dura segundosPorEstagio (padrão do jogo: 75 s);
 *    - Avancar(dt) soma o tempo e atravessa quantos estágios couberem
 *      nele (160 s de uma vez levam o ovo ao mosquito); a sobra vira o
 *      tempo no estágio novo (ovo + 80 s = larva com 5 s);
 *    - o mosquito é o fim: dali não sai mais, o tempo só continua
 *      contando;
 *    - dt zero ou negativo não muda nada (o tempo não volta).
 *
 *  Zerar volta para ovo (o foco foi esvaziado); Reencher volta para
 *  larva (a chuva encheu de novo e os ovos grudados na parede eclodem);
 *  Definir põe estágio e tempo direto (o início sorteado da partida).
 *
 *  O peso de cada estágio na barra de ocupação fica no Inspector do
 *  GerenciadorDeFocos, não aqui.
 * ============================================================ */

/* Estágio de um foco, na ordem do ciclo. */
public enum EstagioDoFoco
{
    Ovo,
    Larva,
    Mosquito
    // A ordem é a do ciclo: valores novos sempre no FIM, nunca reordenar.
}

public class CicloDeVida
{
    private readonly float segundosPorEstagio;

    public EstagioDoFoco Estagio { get; private set; }

    /* Segundos desde que o foco entrou no estágio atual. */
    public float TempoNoEstagio { get; private set; }

    /* Começa em ovo, com tempo 0. */
    public CicloDeVida(float segundosPorEstagio)
    {
        this.segundosPorEstagio = segundosPorEstagio;
        Zerar();
    }

    /* Soma dt ao relógio. Devolve true se o estágio mudou. */
    public bool Avancar(float dt)
    {
        if (dt <= 0f)
            return false;

        EstagioDoFoco antes = Estagio;
        TempoNoEstagio += dt;

        // Um dt grande pode atravessar mais de um estágio. O laço para no
        // mosquito, então segundosPorEstagio <= 0 não trava: só leva
        // qualquer dt positivo direto ao mosquito.
        while (Estagio != EstagioDoFoco.Mosquito && TempoNoEstagio >= segundosPorEstagio)
        {
            TempoNoEstagio -= segundosPorEstagio;
            Estagio = Estagio == EstagioDoFoco.Ovo ? EstagioDoFoco.Larva : EstagioDoFoco.Mosquito;
        }

        return Estagio != antes;
    }

    /* Volta para ovo, com tempo 0 (o foco foi esvaziado). */
    public void Zerar()
    {
        Definir(EstagioDoFoco.Ovo, 0f);
    }

    /* Volta como larva, com tempo 0 (a chuva encheu o foco de novo). */
    public void Reencher()
    {
        Definir(EstagioDoFoco.Larva, 0f);
    }

    /* Põe estágio e tempo direto. O tempo não é conferido aqui: se já
     * passou do fim do estágio, a troca acontece no próximo Avancar. */
    public void Definir(EstagioDoFoco estagio, float tempo)
    {
        Estagio = estagio;
        TempoNoEstagio = tempo;
    }
}
