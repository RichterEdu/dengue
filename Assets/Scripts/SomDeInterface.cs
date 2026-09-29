using UnityEngine;
using UnityEngine.UI;

/* ============================================================
 *  SomDeInterface.cs
 *  ---------------------------------------------------------------------
 *  O ÚNICO som de interface do jogo (pedido do usuário: "bem poucos"):
 *  um clique suave ao acionar um botão ou uma chave dos menus.
 *
 *  No Start, acha todos os ItemDeMenu da cena (inclusive das telas
 *  escondidas) e assina o onClick dos Button e o onValueChanged dos
 *  Toggle. Sem som de passar o mouse e sem som nos sliders. Os menus não
 *  precisam saber deste componente.
 *
 *  Toca com o jogo pausado (ignoreListenerPause). Um por cena: montado por
 *  Dengue > Configurar Sons (e pelo Dengue > Criar Menu Inicial).
 * ============================================================ */

public class SomDeInterface : MonoBehaviour
{
    [SerializeField] private AudioClip clique;
    [Range(0f, 1f)]
    [SerializeField] private float volume = 0.5f;

    private CanalDeAudio fonte;

    void Awake()
    {
        fonte = CanalDeAudio.Criar(transform, "Fonte Interface", CanalSom.Efeitos, false);
        fonte.Fonte.ignoreListenerPause = true;
        fonte.Fonte.priority = 16;
    }

    void Start()
    {
        foreach (ItemDeMenu item in FindObjectsByType<ItemDeMenu>(FindObjectsInactive.Include))
        {
            // O Awake do ItemDeMenu das telas escondidas ainda não rodou: Alvo pode estar vazio.
            Selectable alvo = item.Alvo != null ? item.Alvo : item.GetComponent<Selectable>();
            if (alvo is Button botao)
                botao.onClick.AddListener(Tocar);
            else if (alvo is Toggle chave)
                chave.onValueChanged.AddListener(_ => Tocar());
        }
    }

    public void Tocar()
    {
        if (clique != null && fonte != null)
            fonte.Fonte.PlayOneShot(clique, volume);
    }
}
