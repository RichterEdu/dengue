using TMPro;
using UnityEngine;
using UnityEngine.UI;

/* ============================================================
 *  PostView.cs
 *  ---------------------------------------------------------------------
 *  Só guarda as referências de UM card de publicação do feed.
 *
 *  O modelo (template) é criado pelo menu  Dengue > Criar Celular
 *  já com estas referências preenchidas; o FeedRedeSocial clona esse
 *  modelo a cada post novo e preenche os textos.
 *
 *  A moldura da foto (com o selo "RESOLVIDO") entra no modelo pelo
 *  CelularBuilder.GarantirFotoNoCard; o feed a liga nos posts com foto.
 *
 *  Não tem lógica aqui de propósito: é só a "ficha" do card.
 * ============================================================ */

public class PostView : MonoBehaviour
{
    [Header("Referências do card")]
    public RectTransform rect;
    public CanvasGroup grupo;
    public Image faixa;        // barra colorida na lateral (humor do post)
    public Image avatar;       // círculo colorido
    public TextMeshProUGUI inicial;
    public TextMeshProUGUI nome;
    public TextMeshProUGUI usuario;
    public TextMeshProUGUI texto;
    public TextMeshProUGUI curtidas;
    public Image coracao;

    [Header("Foto do foco")]
    public RectTransform foto;     // moldura "Foto" (desligada nos posts sem foto)
    public RawImage fotoImagem;    // "Foto/Imagem"
    public GameObject selo;        // "Foto/Resolvido" (ligado quando o foco é eliminado)

    [HideInInspector] public string usuarioBase = "";  // "@fulano" sem o tempo
    [HideInInspector] public float instante;           // Time.time da publicação
    [HideInInspector] public float anim;               // 0..1 da animação de entrada
    [HideInInspector] public Interactable foco;        // foco da foto (runtime)
    [HideInInspector] public float animSelo = 1f;      // 0..1 do "pop" do selo
}
