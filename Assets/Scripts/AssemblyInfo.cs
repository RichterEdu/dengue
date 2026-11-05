using System.Runtime.CompilerServices;

/* ============================================================
 *  AssemblyInfo.cs
 *  ---------------------------------------------------------------------
 *  Deixa o Assembly-CSharp-Editor (Editor e testes EditMode, em
 *  Assets/Editor/) ver os membros internal dos scripts do jogo: os
 *  ganchos só para teste (FluxoDeJogo.Ligar, DefinirEstadoParaTeste...)
 *  ficam internal, fora da API pública, e os testes ainda chamam.
 * ============================================================ */

[assembly: InternalsVisibleTo("Assembly-CSharp-Editor")]
