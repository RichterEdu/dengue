/* ============================================================
 *  RegraDoItemNaMao.cs
 *  ---------------------------------------------------------------------
 *  A regra dos itens do kit na mão direita do agente. Classe C# pura,
 *  para dar pra testar no EditMode; quem aplica na cena é o ItensNaMao.
 *
 *    Qual              que item aparece para a ferramenta selecionada;
 *    Gesticula         se o agente está usando o item agora;
 *    EstadoDoAnimator  estado da camada "ItemNaMao" para o item;
 *    GiroDoOmbro       graus que o braço acompanha o olhar.
 *
 *  Sem carga: o balde fica vazio, o borrifador continua na mão e a tela
 *  não aparece. As Mãos e o saco de lixo não passam por aqui (o saco é
 *  do FeedbackDeCarga). Com o pneu nos braços, nenhum item aparece.
 *
 *  O enum é salvo como número: valores novos sempre no FIM.
 * ============================================================ */

public enum ItemNaMao
{
    Nenhum,
    BaldeCheio,
    BaldeVazio,
    Tela,
    Borrifador
    // Novos itens sempre no FIM: o enum é salvo como número.
}

public static class RegraDoItemNaMao
{
    /* O item na mão para a ferramenta selecionada e a carga dela. */
    public static ItemNaMao Qual(Ferramenta selecionada, int carga, bool comPneu)
    {
        if (comPneu)
            return ItemNaMao.Nenhum;

        switch (selecionada)
        {
            case Ferramenta.Areia:
                return carga > 0 ? ItemNaMao.BaldeCheio : ItemNaMao.BaldeVazio;
            case Ferramenta.Tela:
                return carga > 0 ? ItemNaMao.Tela : ItemNaMao.Nenhum;
            case Ferramenta.AguaSanitaria:
                return ItemNaMao.Borrifador;
            default:
                return ItemNaMao.Nenhum;
        }
    }

    /* O gesto toca com um item utilizável na mão, no skill check ou
     * segurando num foco que pede a ferramenta selecionada. Ponto de uso
     * (reabastecer) não tem foco: necessariaDoAlvo vem null. */
    public static bool Gesticula(ItemNaMao item, Ferramenta selecionada, bool skillCheckAtivo,
                                 bool avancando, Ferramenta? necessariaDoAlvo)
    {
        if (item == ItemNaMao.Nenhum || item == ItemNaMao.BaldeVazio)
            return false;

        return skillCheckAtivo || (avancando && necessariaDoAlvo == selecionada);
    }

    /* Nome do estado na camada "ItemNaMao" (null = sem item, sem estado). */
    public static string EstadoDoAnimator(ItemNaMao item, bool gesticulando)
    {
        string prefixo = gesticulando ? "Usando" : "Carregando";
        switch (item)
        {
            case ItemNaMao.BaldeCheio:
            case ItemNaMao.BaldeVazio:
                return prefixo + "Balde";
            case ItemNaMao.Tela:
                return prefixo + "Tela";
            case ItemNaMao.Borrifador:
                return prefixo + "Borrifador";
            default:
                return null;
        }
    }

    /* Graus que o ombro gira para o braço acompanhar o olhar. */
    public static float GiroDoOmbro(float pitch, float fator, float peso)
    {
        return pitch * fator * peso;
    }
}
