using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

/* ============================================================
 *  FocoListaHUD.cs
 *  ---------------------------------------------------------------------
 *  Painel puramente informativo: lista os focos que ainda faltam
 *  eliminar. Tab ALTERNA (aperta abre, aperta de novo fecha).
 *
 *  - Não bloqueia o Interactor nem desliga a DengueHUD: é só um
 *    CanvasGroup com fade, igual a qualquer outro painel da HUD.
 *  - Pega os focos do GerenciadorDeFocos no Start() (do campo ou, vazio,
 *    GerenciadorDeFocos.Obter()) e assina o OnFocoEliminado dele para
 *    tirar cada foco da lista assim que é eliminado, esteja o painel
 *    aberto ou não. Sem gerenciador, a lista fica vazia.
 *
 *  A hierarquia é gerada pelo menu  Dengue > Criar Lista de Focos (Tab)
 *  (Assets/Editor/FocoListaHUDBuilder.cs), que já liga as referências.
 * ============================================================ */

public class FocoListaHUD : MonoBehaviour
{
    [Header("Entrada")]
    [Tooltip("Tecla que abre e fecha a lista.")]
    [SerializeField] private Key teclaAlternar = Key.Tab;

    [Header("Painel")]
    [SerializeField] private CanvasGroup painelGrupo;
    [SerializeField] private float velocidadeFade = 10f;

    [Header("Textos")]
    [SerializeField] private TextMeshProUGUI textoLista;
    [Tooltip("Mostrado quando não sobra nenhum foco.")]
    [SerializeField] private string textoListaVazia = "Nenhum foco restante!";
    [Tooltip("Frase fixa no topo da lista.")]
    [SerializeField] private string prefixo = "Focos faltantes:";
    [Tooltip("Formato de cada item da lista. Marcadores: {categoria} {n}")]
    [SerializeField] private string formatoItem = "{categoria} ({n})";
    [Tooltip("Texto entre um item e o próximo (padrão: uma quebra de linha, um item por linha).")]
    [SerializeField] private string separador = "\n";

    [Tooltip("Se marcado, a lista já começa aberta no início do jogo.")]
    [SerializeField] private bool comecarAberto = true;

    [Header("Referências")]
    [Tooltip("Focos da partida. Vazio = GerenciadorDeFocos.Obter().")]
    [SerializeField] private GerenciadorDeFocos gerenciador;

    private bool aberto;
    private readonly List<Interactable> focosRestantes = new List<Interactable>();

    public bool Aberto => aberto;

    void Start()
    {
        // Registra os focos ainda não concluídos, vindos do gerenciador.
        if (gerenciador == null) gerenciador = GerenciadorDeFocos.Obter();
        if (gerenciador != null)
        {
            gerenciador.Garantir();
            foreach (Interactable foco in gerenciador.Focos)
            {
                if (foco == null || foco.IsComplete)
                    continue;
                focosRestantes.Add(foco);
            }
            gerenciador.OnFocoEliminado += HandleFocoCompletado;
        }

        aberto = comecarAberto;

        if (painelGrupo != null)
        {
            painelGrupo.alpha = aberto ? 1f : 0f;
            painelGrupo.blocksRaycasts = false;
            painelGrupo.interactable = false;
        }

        AtualizarTexto();
    }

    /* Só pelo campo: procurar ou criar um gerenciador aqui (fim do Play,
     * troca de cena) deixaria objeto novo para trás. */
    void OnDestroy()
    {
        if (gerenciador != null)
            gerenciador.OnFocoEliminado -= HandleFocoCompletado;
    }

    void Update()
    {
        Keyboard teclado = Keyboard.current;
        if (teclado != null && teclado[teclaAlternar].wasPressedThisFrame)
            Alternar();

        if (painelGrupo != null)
        {
            float alvo = aberto ? 1f : 0f;
            painelGrupo.alpha = Mathf.MoveTowards(painelGrupo.alpha, alvo, Time.deltaTime * velocidadeFade);
        }
    }

    public void Alternar() => aberto = !aberto;

    /* OnFocoEliminado do gerenciador (uma vez por foco). */
    private void HandleFocoCompletado(Interactable foco)
    {
        focosRestantes.Remove(foco);
        AtualizarTexto();
    }

    /* Agrupa os focos restantes por categoria (tipo) e monta uma frase só,
     * tipo "Focos faltantes: Pneu (4), Vaso de flor (2)." — em vez de uma
     * linha por item individual. */
    private void AtualizarTexto()
    {
        if (textoLista == null)
            return;

        if (focosRestantes.Count == 0)
        {
            textoLista.text = textoListaVazia;
            return;
        }

        Dictionary<string, (string rotulo, int quantidade)> grupos = new Dictionary<string, (string, int)>();
        List<string> ordem = new List<string>();

        foreach (Interactable foco in focosRestantes)
        {
            if (foco == null)
                continue;

            FocoHUDInfo info = foco.GetComponent<FocoHUDInfo>();
            (string chave, string rotulo) categoria = Categoria(foco, info);

            if (grupos.TryGetValue(categoria.chave, out var grupo))
                grupos[categoria.chave] = (grupo.rotulo, grupo.quantidade + 1);
            else
            {
                grupos[categoria.chave] = (categoria.rotulo, 1);
                ordem.Add(categoria.chave);
            }
        }

        List<string> itens = new List<string>();
        foreach (string chave in ordem)
        {
            var grupo = grupos[chave];
            itens.Add(formatoItem
                .Replace("{categoria}", grupo.rotulo)
                .Replace("{n}", grupo.quantidade.ToString()));
        }

        textoLista.text = prefixo + "\n" + string.Join(separador, itens);
    }

    /* Categoria (tipo) do foco, com o rótulo já pronto para exibir.
     * Tabela levantada dos textos já usados no jogo: DengueHUD.NomeDoFoco
     * (Pneu/Vaso) e os FocoHUDInfo.nome preenchidos pelos builders
     * (CaixaDaguaBuilder, PiscinaBuilder, GarrafaFocoBuilder). GarrafaInteractable
     * usa "Garrafas" em vez do FocoHUDInfo.nome daquele tipo, porque ali o
     * nome descreve a quantidade de garrafas DENTRO do grupo, não a
     * quantidade de grupos restantes (que é o que esta lista conta). */
    internal static (string chave, string rotulo) Categoria(Interactable foco, FocoHUDInfo info)
    {
        if (foco is PneuInteractable) return ("pneu", "Pneu");
        if (foco is VasoInteractable) return ("vaso", "Vaso de flor");
        if (foco is CaixaDaguaInteractable) return ("caixaDagua", "Caixa d'água");
        if (foco is PiscinaInteractable) return ("piscina", "Piscina de plástico");
        if (foco is GarrafaInteractable) return ("garrafas", "Garrafas");

        if (info != null && !string.IsNullOrWhiteSpace(info.nome))
        {
            string rotulo = Capitalizar(RemoverArtigo(info.nome));
            return (rotulo, rotulo);
        }

        return (foco.gameObject.name, foco.gameObject.name);
    }

    private static readonly string[] Artigos = { "as ", "os ", "a ", "o " };

    private static string RemoverArtigo(string texto)
    {
        foreach (string artigo in Artigos)
            if (texto.StartsWith(artigo, StringComparison.OrdinalIgnoreCase))
                return texto.Substring(artigo.Length);
        return texto;
    }

    private static string Capitalizar(string texto)
    {
        if (string.IsNullOrEmpty(texto))
            return texto;
        return char.ToUpperInvariant(texto[0]) + texto.Substring(1);
    }
}
