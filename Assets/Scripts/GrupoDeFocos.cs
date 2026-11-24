using UnityEngine;

/* ============================================================
 *  GrupoDeFocos.cs
 *  ---------------------------------------------------------------------
 *  Marcador: os focos filhos deste objeto entram juntos no sorteio dos
 *  "já limpos" do GerenciadorDeFocos (SortearJaLimpos). Ou o grupo todo
 *  começa limpo (o objeto inteiro é desligado), ou o grupo todo fica na
 *  partida. Serve para conjuntos que não fazem sentido pela metade, como
 *  um monte de pneus empilhados (tirar um de baixo deixaria os de cima
 *  flutuando).
 *
 *  Sem o marcador, cada foco é sorteado sozinho e só o objeto dele é
 *  desligado. O grupo mais próximo acima do foco é o que vale.
 * ============================================================ */

[DisallowMultipleComponent]
public class GrupoDeFocos : MonoBehaviour
{
}
