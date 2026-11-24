using System.Reflection;
using NUnit.Framework;
using UnityEngine;

/* ============================================================
 *  LonaPoteTestes.cs
 *  ---------------------------------------------------------------------
 *  Testes EditMode dos focos Lona (LonaInteractable) e Pote de bicho
 *  (PoteDeBichoInteractable), pelo caminho do segurar (AvancarCom com as
 *  Mãos): o visual de cada passo, a conclusão que deixa o objeto na cena
 *  (lona dobrada, pote com água limpa), o nascimento seco e a chuva.
 *
 *  Os focos são montados à mão com filhos vazios no lugar das malhas. O
 *  Start não roda no EditMode: os filhos já nascem no estado do começo.
 *  Os objetos nascem pelo TesteUtil e são destruídos no [TearDown].
 * ============================================================ */

[Category("ParteB")]
public class LonaPoteTestes
{
    private KitDoAgente kit;

    [SetUp]
    public void Preparar()
    {
        ControladorJogador jogador = TesteUtil.Com<ControladorJogador>("Jogador (teste)");
        kit = jogador.gameObject.AddComponent<KitDoAgente>();
    }

    [TearDown]
    public void Limpar()
    {
        TesteUtil.LimparTudo();
    }

    private static GameObject Filho(Component pai, string nome, bool ativo = true)
    {
        GameObject go = TesteUtil.Novo(nome);
        go.transform.SetParent(pai.transform, false);
        go.SetActive(ativo);
        return go;
    }

    private static void SemAnimacao(Interactable foco, int passos)
    {
        TesteUtil.Setar(foco, "requiredClicks", passos);
        TesteUtil.Setar(foco, "punchScale", 0f);
        TesteUtil.Setar(foco, "shrinkDuration", 0f);
    }

    /* ---------------- Lona ---------------- */

    private LonaInteractable NovaLona(out GameObject[] pocas, out GameObject esticada, out GameObject dobrada)
    {
        LonaInteractable lona = TesteUtil.Com<LonaInteractable>("Lona (teste)");
        SemAnimacao(lona, 4);
        pocas = new GameObject[4];
        for (int i = 0; i < 4; i++)
            pocas[i] = Filho(lona, "Poca_" + (i + 1));
        esticada = Filho(lona, "Esticada");
        dobrada = Filho(lona, "Dobrada", false);
        // Array: o TesteUtil.Setar não cobre; vai por reflexão, como na bromélia.
        typeof(LonaInteractable).GetField("pocas", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(lona, pocas);
        TesteUtil.Setar(lona, "esticada", esticada);
        TesteUtil.Setar(lona, "dobrada", dobrada);
        return lona;
    }

    [Test]
    public void Lona_CadaPassoTiraUmaPoca_EConcluiDobrada()
    {
        LonaInteractable lona = NovaLona(out GameObject[] pocas, out GameObject esticada, out GameObject dobrada);

        for (int passo = 1; passo <= 3; passo++)
        {
            Assert.AreEqual(ResultadoDoPasso.Avancou, lona.AvancarCom(kit), "passo " + passo);
            Assert.AreEqual(4 - passo, lona.PocasComAgua, "poças depois do passo " + passo);
        }

        Assert.AreEqual(ResultadoDoPasso.Concluiu, lona.AvancarCom(kit));
        Assert.IsTrue(lona.IsComplete);
        Assert.AreEqual(0, lona.PocasComAgua);
        Assert.IsFalse(esticada.activeSelf, "esticada some");
        Assert.IsTrue(dobrada.activeSelf, "dobrada aparece");
        Assert.IsTrue(lona.gameObject.activeSelf, "a lona fica na cena");
    }

    [Test]
    public void Lona_PedeAsMaos()
    {
        LonaInteractable lona = NovaLona(out _, out _, out _);
        kit.Selecionar(Ferramenta.Areia);

        Assert.AreEqual(ResultadoDoPasso.FerramentaErrada, lona.AvancarCom(kit));
        Assert.AreEqual(4, lona.PocasComAgua);
    }

    [Test]
    public void Lona_NasceSeca_EAChuvaDevolveAsPocas()
    {
        LonaInteractable lona = NovaLona(out GameObject[] pocas, out _, out _);

        lona.NascerVazio();
        Assert.AreEqual(0, lona.PocasComAgua, "nasce sem poça");
        Assert.IsTrue(lona.PodeReencher);

        lona.Reencher();
        Assert.AreEqual(4, lona.PocasComAgua, "a chuva devolve todas");
        Assert.IsFalse(lona.Esvaziado);
    }

    [Test]
    public void Lona_ComecadaADobrar_AChuvaNaoReenche()
    {
        LonaInteractable lona = NovaLona(out _, out _, out _);
        lona.NascerVazio();
        lona.AvancarCom(kit);

        Assert.IsFalse(lona.PodeReencher);
    }

    /* ---------------- Pote de bicho ---------------- */

    private PoteDeBichoInteractable NovoPote(out GameObject velha, out GameObject sujeira, out GameObject limpa)
    {
        PoteDeBichoInteractable pote = TesteUtil.Com<PoteDeBichoInteractable>("Pote (teste)");
        SemAnimacao(pote, 4);
        velha = Filho(pote, "Agua_Velha");
        sujeira = Filho(pote, "Sujeira");
        limpa = Filho(pote, "Agua_Limpa", false);
        TesteUtil.Setar(pote, "aguaVelha", velha);
        TesteUtil.Setar(pote, "sujeira", sujeira);
        TesteUtil.Setar(pote, "aguaLimpa", limpa);
        return pote;
    }

    [Test]
    public void Pote_PrimeiroPassoJogaAAguaVelhaFora_EConcluiComAguaLimpa()
    {
        PoteDeBichoInteractable pote = NovoPote(out GameObject velha, out GameObject sujeira, out GameObject limpa);

        Assert.AreEqual(ResultadoDoPasso.Avancou, pote.AvancarCom(kit));
        Assert.IsFalse(velha.activeSelf, "a água velha sai no 1º passo");
        Assert.IsFalse(sujeira.activeSelf, "a sujeira vai junto");
        Assert.IsFalse(limpa.activeSelf, "a limpa só no fim");

        pote.AvancarCom(kit);
        pote.AvancarCom(kit);
        Assert.AreEqual(ResultadoDoPasso.Concluiu, pote.AvancarCom(kit));
        Assert.IsTrue(pote.IsComplete);
        Assert.IsTrue(limpa.activeSelf, "água limpa no fim");
        Assert.IsTrue(pote.gameObject.activeSelf, "o pote fica na cena");
    }

    [Test]
    public void Pote_NasceSeco_AindaEFoco_EAChuvaDevolveAAguaVelha()
    {
        PoteDeBichoInteractable pote = NovoPote(out GameObject velha, out _, out _);

        pote.NascerVazio();
        Assert.IsFalse(pote.TemAguaVelha);
        Assert.IsTrue(pote.CanInteract, "seco, os ovos continuam na parede");

        pote.Reencher();
        Assert.IsTrue(velha.activeSelf);
    }
}
