using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

/* ============================================================
 *  JaLimposTestes.cs
 *  ---------------------------------------------------------------------
 *  Testes EditMode do sorteio dos focos que já começam limpos
 *  (GerenciadorDeFocos.SortearJaLimpos / JaLimposDe / UnidadeDe): em
 *  Play, o Garantir desliga os sorteados e registra só o resto. Aqui o
 *  sorteio é chamado direto, com System.Random de semente fixa, e o
 *  registro é feito à mão com o que sobrou (o Garantir só sorteia em
 *  Play).
 *
 *  Os focos de teste são de tipos diferentes (classes) para o mínimo por
 *  tipo: Interactable, VasoInteractable e BueiroInteractable. Nascem pelo
 *  TesteUtil e são destruídos no [TearDown].
 *
 *  Rodar:  unity command run_tests --mode EditMode --filter_type category --filter ParteB
 * ============================================================ */

[Category("ParteB")]
public class JaLimposTestes
{
    private GerenciadorDeFocos gerenciador;

    [SetUp]
    public void Preparar()
    {
        gerenciador = TesteUtil.Com<GerenciadorDeFocos>("Gerenciador (teste)");
    }

    [TearDown]
    public void Limpar()
    {
        TesteUtil.LimparTudo();
    }

    private static List<Interactable> Focos<T>(int quantos, string nome) where T : Interactable
    {
        List<Interactable> focos = new List<Interactable>();
        for (int i = 0; i < quantos; i++)
            focos.Add(TesteUtil.Com<T>(nome + " " + i + " (teste)"));
        return focos;
    }

    private static int Contar<T>(IEnumerable<Interactable> focos) where T : Interactable
    {
        int n = 0;
        foreach (Interactable foco in focos)
        {
            if (foco.GetType() == typeof(T))
                n++;
        }
        return n;
    }

    [Test]
    public void JaLimposDe_ArredondaAFracao()
    {
        TesteUtil.Setar(gerenciador, "fracaoJaLimpos", 1f / 3f);
        Assert.AreEqual(16, gerenciador.JaLimposDe(48));
        Assert.AreEqual(10, gerenciador.JaLimposDe(31));
        Assert.AreEqual(0, gerenciador.JaLimposDe(1));
        Assert.AreEqual(0, gerenciador.JaLimposDe(0));

        TesteUtil.Setar(gerenciador, "fracaoJaLimpos", 0f);
        Assert.AreEqual(0, gerenciador.JaLimposDe(48));

        TesteUtil.Setar(gerenciador, "fracaoJaLimpos", 1f);
        Assert.AreEqual(48, gerenciador.JaLimposDe(48));
    }

    [Test]
    public void Sorteio_TiraAContagemFixa_EAMesmaSementeRepete()
    {
        TesteUtil.Setar(gerenciador, "fracaoJaLimpos", 1f / 3f);
        List<Interactable> focos = Focos<Interactable>(12, "Foco");
        focos.AddRange(Focos<VasoInteractable>(6, "Vaso"));
        focos.AddRange(Focos<BueiroInteractable>(6, "Bueiro"));

        List<Interactable> a = gerenciador.SortearJaLimpos(focos, new System.Random(7));
        List<Interactable> b = gerenciador.SortearJaLimpos(focos, new System.Random(7));
        List<Interactable> c = gerenciador.SortearJaLimpos(focos, new System.Random(8));

        Assert.AreEqual(8, a.Count, "1/3 de 24");
        CollectionAssert.AreEqual(a, b, "mesma semente, mesmo sorteio");
        Assert.AreEqual(8, c.Count, "outra semente, mesma contagem");
        CollectionAssert.AreNotEqual(a, c, "outra semente, outros focos");
    }

    [Test]
    public void Sorteio_DeixaOMinimoDeCadaTipo_MesmoComTudoLimpo()
    {
        TesteUtil.Setar(gerenciador, "fracaoJaLimpos", 1f);
        TesteUtil.Setar(gerenciador, "minimoAtivosPorTipo", 1);
        List<Interactable> focos = Focos<Interactable>(5, "Foco");
        focos.AddRange(Focos<VasoInteractable>(3, "Vaso"));
        focos.AddRange(Focos<BueiroInteractable>(1, "Bueiro"));

        List<Interactable> limpos = gerenciador.SortearJaLimpos(focos, new System.Random(1));

        Assert.AreEqual(4, Contar<Interactable>(limpos), "sobra 1 dos 5");
        Assert.AreEqual(2, Contar<VasoInteractable>(limpos), "sobra 1 dos 3");
        Assert.AreEqual(0, Contar<BueiroInteractable>(limpos), "o único bueiro fica");
    }

    [Test]
    public void Sorteio_GrupoDeFocosSaiInteiroOuFicaInteiro()
    {
        TesteUtil.Setar(gerenciador, "fracaoJaLimpos", 0.5f);
        TesteUtil.Setar(gerenciador, "minimoAtivosPorTipo", 0);
        GameObject monte = TesteUtil.Novo("Monte (teste)");
        monte.AddComponent<GrupoDeFocos>();
        List<Interactable> doMonte = Focos<Interactable>(3, "Pneu do monte");
        foreach (Interactable pneu in doMonte)
            pneu.transform.SetParent(monte.transform);
        List<Interactable> focos = new List<Interactable>(doMonte);
        focos.AddRange(Focos<Interactable>(5, "Solto"));

        for (int semente = 1; semente <= 20; semente++)
        {
            List<Interactable> limpos = gerenciador.SortearJaLimpos(focos, new System.Random(semente));
            int doMonteLimpos = 0;
            foreach (Interactable pneu in doMonte)
            {
                if (limpos.Contains(pneu))
                    doMonteLimpos++;
            }
            Assert.That(doMonteLimpos == 0 || doMonteLimpos == 3, "semente " + semente + ": o monte saiu pela metade");
            Assert.LessOrEqual(limpos.Count, 4, "semente " + semente + ": passou da contagem");
        }

        Assert.AreSame(monte, GerenciadorDeFocos.UnidadeDe(doMonte[0]), "a unidade é o grupo");
        Assert.AreSame(focos[3].gameObject, GerenciadorDeFocos.UnidadeDe(focos[3]), "sem grupo, o próprio foco");
    }

    [Test]
    public void Sorteio_IgnoraFocoConcluidoOuDesligado()
    {
        TesteUtil.Setar(gerenciador, "fracaoJaLimpos", 1f);
        TesteUtil.Setar(gerenciador, "minimoAtivosPorTipo", 0);
        List<Interactable> focos = Focos<Interactable>(3, "Foco");
        focos[0].gameObject.SetActive(false);
        TesteUtil.Setar(focos[1], "requiredClicks", 1);
        TesteUtil.Setar(focos[1], "punchScale", 0f);
        TesteUtil.Setar(focos[1], "shrinkDuration", 0f);
        focos[1].Interact();
        Assert.IsTrue(focos[1].IsComplete, "pré-condição: concluído");

        List<Interactable> limpos = gerenciador.SortearJaLimpos(focos, new System.Random(3));

        CollectionAssert.AreEqual(new[] { focos[2] }, limpos);
    }

    [Test]
    public void Registro_SemOsLimpos_TotalERestantesDescontam_EOsSorteiosNaoOsVeem()
    {
        TesteUtil.Setar(gerenciador, "fracaoJaLimpos", 0.5f);
        TesteUtil.Setar(gerenciador, "minimoAtivosPorTipo", 0);
        TesteUtil.Setar(gerenciador, "fracaoNascendoComAgua", 1f);
        List<Interactable> focos = Focos<Interactable>(10, "Foco");

        List<Interactable> limpos = gerenciador.SortearJaLimpos(focos, new System.Random(5));
        List<Interactable> naPartida = focos.FindAll(f => !limpos.Contains(f));
        gerenciador.Registrar(naPartida);

        Assert.AreEqual(5, gerenciador.Total);
        Assert.AreEqual(5, gerenciador.Restantes);
        foreach (Interactable foco in limpos)
            CollectionAssert.DoesNotContain(gerenciador.Focos, foco);

        // O começo da partida só sorteia estágio entre os registrados.
        TesteUtil.Setar(gerenciador, "focosComMosquitoNoInicio", 10);
        gerenciador.SortearInicio(new System.Random(2));
        foreach (Interactable foco in naPartida)
            Assert.AreEqual(EstagioDoFoco.Mosquito, gerenciador.EstagioDe(foco));
        foreach (Interactable foco in limpos)
            Assert.AreEqual(EstagioDoFoco.Ovo, gerenciador.EstagioDe(foco), "não registrado: fica no padrão");
    }

    /* ---------------- Já limpo no mapa ---------------- */

    private static void RodarStart(Interactable foco)
    {
        typeof(Interactable).GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(foco, null);
    }

    private static GameObject Filho(Component pai, string nome, bool ativo = true)
    {
        GameObject go = TesteUtil.Novo(nome);
        go.transform.SetParent(pai.transform, false);
        go.SetActive(ativo);
        return go;
    }

    [Test]
    public void DeixarJaLimpo_QuemApareceFicaNoMapa_ConcluidoESemEvento()
    {
        VasoInteractable vaso = TesteUtil.Com<VasoInteractable>("Vaso (teste)");
        int eventos = 0;
        vaso.OnCompleted += _ => eventos++;

        GerenciadorDeFocos.DeixarJaLimpo(vaso);

        Assert.IsTrue(vaso.gameObject.activeSelf, "continua no mapa");
        Assert.IsTrue(vaso.JaLimpo);
        Assert.IsTrue(vaso.IsComplete);
        Assert.IsFalse(vaso.CanInteract, "não aceita passo");
        Assert.IsFalse(vaso.Zumbe);
        Assert.IsFalse(vaso.PodeReencher, "a chuva não mexe");
        Assert.AreEqual(0, eventos, "sem OnCompleted: não está na partida");
    }

    [Test]
    public void DeixarJaLimpo_PneuEGarrafasSomem()
    {
        PneuInteractable pneu = TesteUtil.Com<PneuInteractable>("Pneu (teste)");
        GarrafaInteractable garrafas = TesteUtil.Com<GarrafaInteractable>("Garrafas (teste)");

        GerenciadorDeFocos.DeixarJaLimpo(pneu);
        GerenciadorDeFocos.DeixarJaLimpo(garrafas);

        Assert.IsFalse(pneu.gameObject.activeSelf);
        Assert.IsFalse(garrafas.gameObject.activeSelf);
        Assert.IsFalse(pneu.JaLimpo, "some, não fica resolvido no mapa");
    }

    [Test]
    public void JaLimpo_NoStart_CadaTipoMostraOEstadoResolvido()
    {
        // Lona: dobrada, sem poça.
        LonaInteractable lona = TesteUtil.Com<LonaInteractable>("Lona (teste)");
        GameObject poca = Filho(lona, "Poca_1");
        GameObject esticada = Filho(lona, "Esticada");
        GameObject dobrada = Filho(lona, "Dobrada", false);
        typeof(LonaInteractable).GetField("pocas", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(lona, new[] { poca });
        TesteUtil.Setar(lona, "esticada", esticada);
        TesteUtil.Setar(lona, "dobrada", dobrada);

        // Pote: água limpa, sem a velha.
        PoteDeBichoInteractable pote = TesteUtil.Com<PoteDeBichoInteractable>("Pote (teste)");
        GameObject velha = Filho(pote, "Agua_Velha");
        GameObject limpa = Filho(pote, "Agua_Limpa", false);
        TesteUtil.Setar(pote, "aguaVelha", velha);
        TesteUtil.Setar(pote, "aguaLimpa", limpa);

        // Vaso: sem a água do pratinho.
        VasoInteractable vaso = TesteUtil.Com<VasoInteractable>("Vaso (teste)");
        GameObject aguaDoVaso = Filho(vaso, "Agua");
        TesteUtil.Setar(vaso, "water", aguaDoVaso);

        foreach (Interactable foco in new Interactable[] { lona, pote, vaso })
        {
            TesteUtil.Setar(foco, "shrinkDuration", 0f);
            GerenciadorDeFocos.DeixarJaLimpo(foco);
            RodarStart(foco);
        }

        Assert.IsFalse(poca.activeSelf, "lona sem poça");
        Assert.IsFalse(esticada.activeSelf, "lona: a esticada some");
        Assert.IsTrue(dobrada.activeSelf, "lona dobrada");
        Assert.IsFalse(velha.activeSelf, "pote sem a água velha");
        Assert.IsTrue(limpa.activeSelf, "pote com água limpa");
        Assert.IsFalse(aguaDoVaso.activeSelf, "vaso sem água no pratinho");
        Assert.AreEqual(1f, vaso.SandFill, "pratinho cheio de areia");
    }
}
