using System;

/* ============================================================
 *  SkillCheck.cs
 *  ---------------------------------------------------------------------
 *  Lógica pura (sem Unity) do skill check do bueiro: um ponteiro dá uma
 *  volta no anel e o jogador aperta com ele dentro da zona. Ponteiro e
 *  zona são frações da volta (0..1, horário, a partir do topo).
 *
 *  - SkillCheck: um check. Apertar fora da zona, ou a volta terminar sem
 *    aperto, é erro.
 *  - SessaoDeSkillCheck: a sequência de checks de uma ação. Cada acerto
 *    passa para o próximo, com a zona menor.
 *  - MinigameDeSkillCheck: o adaptador para a base comum (IMinigame, em
 *    Minigame.cs). O aperto é um Apertar, senão o ponteiro anda; acerto
 *    vira Passo e erro vira Errou. Não dá para sair (PodeSair false) e a
 *    barra não enche dentro do check (FracaoDoPasso 0), nem há som de
 *    ação (Agindo false). Quem aplica o resultado no foco é o
 *    Interactor.cs.
 * ============================================================ */

public enum ResultadoDoCheck
{
    EmAndamento,
    Acertou,
    Errou
}

public class SkillCheck
{
    public float Ponteiro { get; private set; }
    public float InicioDaZona { get; }
    public float TamanhoDaZona { get; }

    private readonly float segundosPorVolta;

    public SkillCheck(float inicioDaZona, float tamanhoDaZona, float segundosPorVolta)
    {
        InicioDaZona = inicioDaZona;
        TamanhoDaZona = tamanhoDaZona;
        this.segundosPorVolta = Math.Max(0.01f, segundosPorVolta);
    }

    /* Anda o ponteiro. A volta acabou sem aperto: Errou. */
    public ResultadoDoCheck Atualizar(float deltaTime)
    {
        Ponteiro += deltaTime / segundosPorVolta;
        return Ponteiro >= 1f ? ResultadoDoCheck.Errou : ResultadoDoCheck.EmAndamento;
    }

    public ResultadoDoCheck Apertar()
    {
        bool naZona = Ponteiro >= InicioDaZona && Ponteiro <= InicioDaZona + TamanhoDaZona;
        return naZona ? ResultadoDoCheck.Acertou : ResultadoDoCheck.Errou;
    }
}

public class SessaoDeSkillCheck
{
    // Zona do 1º check, quanto encolhe a cada acerto e o menor tamanho.
    internal const float ZonaInicial = 0.15f;
    internal const float EncolhePorCheck = 0.025f;
    internal const float ZonaMinima = 0.08f;

    // Até aqui o anel fica sem zona: dá tempo de ver o ponteiro sair do topo.
    internal const float InicioMinimoDaZona = 0.3f;
    internal const float FimMaximoDaZona = 0.95f;

    private readonly float segundosPorVolta;
    private readonly Random sorteio;

    public int Total { get; }

    /* Acertos até agora (= índice do check atual). */
    public int Acertos { get; private set; }

    public bool Terminou => Acertos >= Total;

    public SkillCheck Atual { get; private set; }

    public SessaoDeSkillCheck(int total, float segundosPorVolta, int semente)
    {
        Total = Math.Max(1, total);
        this.segundosPorVolta = segundosPorVolta;
        sorteio = new Random(semente);
        Atual = Sortear();
    }

    public ResultadoDoCheck Atualizar(float deltaTime)
    {
        return Atual.Atualizar(deltaTime);
    }

    /* Acertou: conta e já prepara o próximo check (se houver). */
    public ResultadoDoCheck Apertar()
    {
        ResultadoDoCheck resultado = Atual.Apertar();
        if (resultado == ResultadoDoCheck.Acertou)
        {
            Acertos++;
            if (!Terminou)
                Atual = Sortear();
        }
        return resultado;
    }

    private SkillCheck Sortear()
    {
        float tamanho = Math.Max(ZonaMinima, ZonaInicial - EncolhePorCheck * Acertos);
        float folga = FimMaximoDaZona - tamanho - InicioMinimoDaZona;
        float inicio = InicioMinimoDaZona + (float)sorteio.NextDouble() * folga;
        return new SkillCheck(inicio, tamanho, segundosPorVolta);
    }
}

/* O skill check do bueiro como minigame (criado pelo BueiroInteractable). */
public class MinigameDeSkillCheck : IMinigame
{
    public SessaoDeSkillCheck Sessao { get; }

    public MinigameDeSkillCheck(SessaoDeSkillCheck sessao)
    {
        Sessao = sessao;
    }

    public EstadoDoMinigame Atualizar(float dt, EntradaDoMinigame entrada)
    {
        ResultadoDoCheck resultado = entrada.Apertou ? Sessao.Apertar() : Sessao.Atualizar(dt);
        switch (resultado)
        {
            case ResultadoDoCheck.Acertou: return EstadoDoMinigame.Passo;
            case ResultadoDoCheck.Errou: return EstadoDoMinigame.Errou;
            default: return EstadoDoMinigame.EmAndamento;
        }
    }

    public float FracaoDoPasso => 0f;

    /* O botão de largar não sai do bueiro: a regra do anel não muda. */
    public bool PodeSair => false;

    /* Dentro do check não há som de ação nem gesto do item na mão. */
    public bool Agindo => false;
}
