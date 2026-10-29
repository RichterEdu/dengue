using System;
using System.Collections.Generic;

/* ============================================================
 *  EstoqueDoKit.cs
 *  ---------------------------------------------------------------------
 *  Quanto o agente leva de cada ferramenta (Parte A). Classe C# pura,
 *  sem MonoBehaviour, para dar pra testar no EditMode; quem guarda uma
 *  instância dela no Player é o KitDoAgente.
 *
 *  Consumíveis (Areia, Tela, Água sanitária): cada um tem capacidade e
 *  quantidade. Começa cheio; Consumir gasta uma carga (o Interactable
 *  chama só quando o foco termina) e Reabastecer enche de novo as que
 *  estiverem na lista do ponto de reabastecimento (picape da UPA: as
 *  três; monte de areia: só a areia).
 *
 *  Mãos e Saco de lixo não gastam: TemCarga/Consumir dão sempre true e
 *  Quantidade/Capacidade dão 0. O saco tem a conta própria dele: cada
 *  garrafa recolhida ocupa um lugar e, cheio, só volta a aceitar depois
 *  de esvaziado na caçamba.
 *
 *  Os números padrão (areia 1, tela 1, água sanitária 2 doses, saco de
 *  6 garrafas) ficam no Inspector do KitDoAgente.
 * ============================================================ */

public class EstoqueDoKit
{
    // Só os consumíveis entram nos dicionários.
    private readonly Dictionary<Ferramenta, int> capacidade = new Dictionary<Ferramenta, int>();
    private readonly Dictionary<Ferramenta, int> quantidade = new Dictionary<Ferramenta, int>();

    public int GarrafasNoSaco { get; private set; }
    public int CapacidadeDoSaco { get; }
    public bool SacoCheio => GarrafasNoSaco >= CapacidadeDoSaco;

    /* Começa com tudo cheio e o saco vazio. Valores negativos viram 0. */
    public EstoqueDoKit(int areia, int tela, int aguaSanitaria, int capacidadeSaco)
    {
        DefinirCapacidade(Ferramenta.Areia, areia);
        DefinirCapacidade(Ferramenta.Tela, tela);
        DefinirCapacidade(Ferramenta.AguaSanitaria, aguaSanitaria);
        CapacidadeDoSaco = Math.Max(0, capacidadeSaco);
    }

    private void DefinirCapacidade(Ferramenta ferramenta, int valor)
    {
        int cap = Math.Max(0, valor);
        capacidade[ferramenta] = cap;
        quantidade[ferramenta] = cap;
    }

    /* Cargas que restam (0 para ferramenta não consumível). */
    public int Quantidade(Ferramenta ferramenta)
    {
        return quantidade.TryGetValue(ferramenta, out int q) ? q : 0;
    }

    /* Cargas quando cheio (0 para ferramenta não consumível). */
    public int Capacidade(Ferramenta ferramenta)
    {
        return capacidade.TryGetValue(ferramenta, out int c) ? c : 0;
    }

    public bool TemCarga(Ferramenta ferramenta)
    {
        return !Ferramentas.Consumivel(ferramenta) || Quantidade(ferramenta) > 0;
    }

    /* Gasta uma carga. False se já estava vazio (aí nada muda). */
    public bool Consumir(Ferramenta ferramenta)
    {
        if (!Ferramentas.Consumivel(ferramenta))
            return true;

        int q = Quantidade(ferramenta);
        if (q <= 0)
            return false;

        quantidade[ferramenta] = q - 1;
        return true;
    }

    /* Enche até a capacidade as ferramentas da lista. True se alguma mudou. */
    public bool Reabastecer(IEnumerable<Ferramenta> ferramentas)
    {
        if (ferramentas == null)
            return false;

        bool mudou = false;
        foreach (Ferramenta f in ferramentas)
        {
            if (!Ferramentas.Consumivel(f) || Quantidade(f) >= Capacidade(f))
                continue;

            quantidade[f] = Capacidade(f);
            mudou = true;
        }
        return mudou;
    }

    /* True se todas as da lista estão no máximo (não consumível conta como cheia). */
    public bool Cheio(IEnumerable<Ferramenta> ferramentas)
    {
        if (ferramentas == null)
            return true;

        foreach (Ferramenta f in ferramentas)
        {
            if (Quantidade(f) < Capacidade(f))
                return false;
        }
        return true;
    }

    /* Põe uma garrafa no saco. False se o saco já está cheio. */
    public bool GuardarGarrafa()
    {
        if (SacoCheio)
            return false;

        GarrafasNoSaco++;
        return true;
    }

    /* Esvazia o saco (na caçamba) e devolve quantas garrafas tirou. */
    public int EsvaziarSaco()
    {
        int tiradas = GarrafasNoSaco;
        GarrafasNoSaco = 0;
        return tiradas;
    }
}
