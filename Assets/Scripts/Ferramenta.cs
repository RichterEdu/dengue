using System;

/* ============================================================
 *  Ferramenta.cs
 *  ---------------------------------------------------------------------
 *  As ferramentas do kit do agente (Parte A) e o que pode acontecer a
 *  cada passo de "segurar para agir" num foco.
 *
 *  A ordem do enum Ferramenta é a dos espaços da barra de ferramentas
 *  (teclas 1–5):
 *
 *    1 Mãos            caixa d'água, piscina, pegar pneu   sempre
 *    2 Areia           pratinho do vaso                     consumível
 *    3 Tela            ralo                                 consumível
 *    4 Água sanitária  bromélia                             consumível (doses)
 *    5 Saco de lixo    garrafas                             não gasta: enche
 *
 *  Quanto de cada uma o agente leva fica no EstoqueDoKit.cs. Ferramentas
 *  junta os dados fixos (nome na HUD, se é consumível).
 *
 *  Os dois enums são salvos como número na cena e nos prefabs: valores
 *  novos sempre no FIM, nunca reordenar (mesma regra do TipoFoco).
 * ============================================================ */

public enum Ferramenta
{
    Maos,
    Areia,
    Tela,
    AguaSanitaria,
    SacoDeLixo
    // Novas ferramentas sempre no FIM: o enum é salvo como número.
}

/* Resposta de um passo de ação num foco (ou ponto de uso). */
public enum ResultadoDoPasso
{
    Avancou,            // o passo contou; o foco ainda não terminou
    Concluiu,           // o foco foi eliminado neste passo
    Pegou,              // o pneu foi para os braços do agente
    FerramentaErrada,   // a ferramenta selecionada não serve para este foco
    SemCarga,           // a ferramenta certa, mas acabou o estoque dela
    SacoCheio,          // garrafas: o saco não tem mais espaço
    MaosOcupadas,       // carregando pneu: só a caçamba e o largar funcionam
    Bloqueado           // nada a fazer aqui (já concluído, desligado...), sem aviso
    // Novos resultados sempre no FIM: o enum é salvo como número.
}

public static class Ferramentas
{
    /* Todas as ferramentas, na ordem do enum (= ordem da barra). */
    public static readonly Ferramenta[] Todas = (Ferramenta[])Enum.GetValues(typeof(Ferramenta));

    /* Nome mostrado na HUD e nos avisos. */
    public static string Nome(Ferramenta ferramenta)
    {
        switch (ferramenta)
        {
            case Ferramenta.Maos: return "Mãos";
            case Ferramenta.Areia: return "Areia";
            case Ferramenta.Tela: return "Tela";
            case Ferramenta.AguaSanitaria: return "Água sanitária";
            case Ferramenta.SacoDeLixo: return "Saco de lixo";
            default: return ferramenta.ToString();
        }
    }

    /* Gasta uma carga ao concluir um foco (e precisa de reabastecimento). */
    public static bool Consumivel(Ferramenta ferramenta)
    {
        return ferramenta == Ferramenta.Areia
            || ferramenta == Ferramenta.Tela
            || ferramenta == Ferramenta.AguaSanitaria;
    }

    /* Recusa que a HUD explica ao jogador (o Bloqueado passa em silêncio). */
    public static bool EhRecusa(ResultadoDoPasso resultado)
    {
        return resultado == ResultadoDoPasso.FerramentaErrada
            || resultado == ResultadoDoPasso.SemCarga
            || resultado == ResultadoDoPasso.SacoCheio
            || resultado == ResultadoDoPasso.MaosOcupadas;
    }
}
