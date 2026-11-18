using UnityEngine;

/* ============================================================
 *  FotoDoFoco.cs
 *  ---------------------------------------------------------------------
 *  A foto de um foco, para o narrador (HistoriaRedeSocial) postar quando
 *  o jogador empaca. Fica no MESMO GameObject do Interactable: é a
 *  referência ao objeto que liga a foto àquele foco, então nomes
 *  repetidos na hierarquia não importam.
 *
 *    foto              o PNG pronto (Assets/Meus/Assets Gerados/
 *                      Fotos dos Focos/);
 *    posicaoDaCamera   a pose de onde a foto foi tirada, em espaço de
 *    rotacaoDaCamera   MUNDO (Euler) — não como filho do foco, porque o
 *    campoDeVisao      pneu é carregado em jogo e a pose é só do Editor.
 *
 *  Selecionado no Editor, desenha o frustum da câmera da foto, para ver
 *  de onde ela foi tirada.
 * ============================================================ */

[DisallowMultipleComponent]
public class FotoDoFoco : MonoBehaviour
{
    [Tooltip("A foto pronta deste foco (PNG 720×480).")]
    public Texture2D foto;

    [Tooltip("Posição da câmera da foto, em espaço de mundo.")]
    public Vector3 posicaoDaCamera;

    [Tooltip("Rotação da câmera da foto (Euler), em espaço de mundo.")]
    public Vector3 rotacaoDaCamera;

    [Tooltip("Campo de visão vertical (graus) da câmera da foto.")]
    public float campoDeVisao = 60f;

    // Só para o gizmo: a proporção da foto (720×480) e até onde o frustum vai.
    private const float AspectoDoGizmo = 1.5f;
    private const float AlcanceDoGizmo = 3f;

    void OnDrawGizmosSelected()
    {
        Matrix4x4 matrizAnterior = Gizmos.matrix;
        Color corAnterior = Gizmos.color;

        Gizmos.matrix = Matrix4x4.TRS(posicaoDaCamera, Quaternion.Euler(rotacaoDaCamera), Vector3.one);
        Gizmos.color = Color.yellow;
        Gizmos.DrawFrustum(Vector3.zero, campoDeVisao, AlcanceDoGizmo, 0f, AspectoDoGizmo);

        Gizmos.matrix = matrizAnterior;
        Gizmos.color = corAnterior;
    }
}
