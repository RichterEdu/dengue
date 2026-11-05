using UnityEngine;

/* ============================================================
 *  StaminaDeCarga.cs
 *  ---------------------------------------------------------------------
 *  O fôlego para correr com o pneu nos braços. Classe C# pura, como o
 *  EstoqueDoKit (testável sem cena); quem a guarda é o KitDoAgente.
 *
 *    - Fracao vai de 1 (cheia) a 0. Correndo, esgota em segundosParaEsgotar;
 *      sem correr, recupera em segundosParaRecuperar, andando com o pneu ou
 *      de mãos livres.
 *    - Ao zerar, fica Esgotada: PodeCorrer é falso até a fração voltar a
 *      fracaoParaVoltar. Sem isso, segurar o Shift daria um pisca-pisca de
 *      correr e andar a cada quadro.
 * ============================================================ */

public class StaminaDeCarga
{
    private readonly float segundosParaEsgotar;
    private readonly float segundosParaRecuperar;
    private readonly float fracaoParaVoltar;

    public float Fracao { get; private set; } = 1f;
    public bool Esgotada { get; private set; }
    public bool PodeCorrer => !Esgotada;

    public StaminaDeCarga(float segundosParaEsgotar, float segundosParaRecuperar, float fracaoParaVoltar)
    {
        this.segundosParaEsgotar = Mathf.Max(0.01f, segundosParaEsgotar);
        this.segundosParaRecuperar = Mathf.Max(0.01f, segundosParaRecuperar);
        this.fracaoParaVoltar = Mathf.Clamp01(fracaoParaVoltar);
    }

    /* Anda dt segundos. Devolve true só no quadro em que a stamina zerou. */
    public bool Avancar(float dt, bool correndo)
    {
        if (dt <= 0f)
            return false;

        if (correndo && !Esgotada)
        {
            Fracao = Mathf.Max(0f, Fracao - dt / segundosParaEsgotar);
            if (Fracao > 0f)
                return false;
            Esgotada = true;
            return true;
        }

        Fracao = Mathf.Min(1f, Fracao + dt / segundosParaRecuperar);
        if (Esgotada && Fracao >= fracaoParaVoltar)
            Esgotada = false;
        return false;
    }
}
