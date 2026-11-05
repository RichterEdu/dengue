using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

/* ============================================================
 *  PontosDeUsoTestes.cs
 *  ---------------------------------------------------------------------
 *  Testes EditMode (Parte A) dos pontos de uso: a picape da UPA e o
 *  monte de areia (PontoDeReabastecimento), a caçamba (Cacamba) e, na
 *  Parte B, a lixeira (Lixeira).
 *
 *    - PontoDeUso não é foco: não herda Interactable, que o FluxoDeJogo,
 *      a HUD e a rede social varrem como criadouro;
 *    - o reabastecimento enche só o que a lista 'recarrega' oferece e
 *      recusa, nesta ordem, com pneu nas mãos e com o kit já cheio (sem
 *      disparar o OnReabasteceu, que dá o post);
 *    - o OnReabasteceu leva o ponto usado: o post da picape da UPA só
 *      sai nela (ehPicapeDaUpa), e não no monte de areia;
 *    - a caçamba recebe o pneu carregado (numa vaga da carroceria) e, sem
 *      pneu e sem picape, recusa; o saco esvazia na lixeira, e não mais
 *      na caçamba (Parte B). Os testes completos da Parte B estão no
 *      DestinosTestes.
 *
 *  A lista 'recarrega' é trocada pelo SerializedObject (o TesteUtil.Setar
 *  não aceita listas), do mesmo jeito que o simulador a lê.
 *
 *  O kit fica num objeto com ControladorJogador e um PontoDeCarga de
 *  teste, como no PneuCarregarTestes. O pneu de teste não tem punch nem
 *  animação de conclusão (shrinkDuration 0: o OnComplete só desativa o
 *  objeto), porque as corrotinas não andam no EditMode; ele vai para os
 *  braços direto pelo kit.Pegar.
 *
 *  Os objetos nascem pelo TesteUtil (escondidos, fora da cena) e são
 *  destruídos no [TearDown].
 *
 *  Rodar:  Window > General > Test Runner > EditMode (categoria ParteA)
 *  ou      unity command run_tests --mode EditMode --filter_type category --filter ParteA
 * ============================================================ */

[Category("ParteA")]
public class PontosDeUsoTestes
{
    private KitDoAgente kit;

    [SetUp]
    public void Preparar()
    {
        ControladorJogador jogador = TesteUtil.Com<ControladorJogador>("Jogador (teste)");
        kit = jogador.gameObject.AddComponent<KitDoAgente>();
        kit.PontoDeCarga = TesteUtil.Novo("Ponto de carga (teste)").transform;
    }

    [TearDown]
    public void Limpar()
    {
        TesteUtil.LimparTudo();
    }

    /* Troca a lista 'recarrega' pelo SerializedObject, como o Inspector. */
    private static void DefinirRecarrega(PontoDeReabastecimento ponto, params Ferramenta[] ferramentas)
    {
        SerializedObject so = new SerializedObject(ponto);
        SerializedProperty lista = so.FindProperty("recarrega");
        Assert.IsNotNull(lista, "o campo serializado 'recarrega' existe");

        lista.arraySize = ferramentas.Length;
        for (int i = 0; i < ferramentas.Length; i++)
            lista.GetArrayElementAtIndex(i).intValue = (int)ferramentas[i];   // enum salvo como número
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    /* Pneu de teste já nos braços do agente. */
    private PneuInteractable PneuNasMaos()
    {
        PneuInteractable pneu = TesteUtil.Com<PneuInteractable>("Pneu (teste)");
        TesteUtil.Setar(pneu, "requiredClicks", 2);
        TesteUtil.Setar(pneu, "punchScale", 0f);
        TesteUtil.Setar(pneu, "shrinkDuration", 0f);

        Assert.IsTrue(kit.Pegar(pneu), "pneu nos braços");
        return pneu;
    }

    [Test]
    public void PontoDeUso_NaoEhFoco()
    {
        Assert.IsFalse(typeof(Interactable).IsAssignableFrom(typeof(PontoDeUso)),
                       "PontoDeUso não pode entrar na varredura de focos");
    }

    /* Monte de areia: só a areia volta; a tela gasta continua gasta. */
    [Test]
    public void Picape_RecarregaSoOQueOferece()
    {
        PontoDeReabastecimento ponto = TesteUtil.Com<PontoDeReabastecimento>("Monte de areia (teste)");
        DefinirRecarrega(ponto, Ferramenta.Areia);
        Assert.IsTrue(kit.Consumir(Ferramenta.Areia));
        Assert.IsTrue(kit.Consumir(Ferramenta.Tela));

        Assert.IsTrue(ponto.PodeUsar(kit, out string motivo), $"falta areia (motivo: {motivo})");
        ponto.Usar(kit);

        Assert.AreEqual(1, kit.Estoque.Quantidade(Ferramenta.Areia), "areia recarregada");
        Assert.AreEqual(0, kit.Estoque.Quantidade(Ferramenta.Tela), "tela não é oferecida aqui");
    }

    /* Review Focus 5: com o kit cheio, recusa e não dá o post de reabastecimento. */
    [Test]
    public void Picape_KitCheio_Recusa()
    {
        PontoDeReabastecimento picape = TesteUtil.Com<PontoDeReabastecimento>("Picape (teste)");
        int reabastecidas = 0;
        kit.OnReabasteceu += origem => reabastecidas++;

        Assert.IsFalse(picape.PodeUsar(kit, out string motivo));
        Assert.AreEqual("Seu kit já está cheio.", motivo);

        // Mesmo usada assim, nada enche: o aviso (e o post) não sai.
        picape.Usar(kit);
        Assert.AreEqual(0, reabastecidas, "OnReabasteceu");
    }

    /* Revisão final (D): o OnReabasteceu diz de que ponto veio o material. A
     * HistoriaRedeSocial só posta "passou na picape da UPA" com a picape. */
    [Test]
    public void Reabastecer_AvisaOPontoDeOrigem()
    {
        PontoDeReabastecimento monte = TesteUtil.Com<PontoDeReabastecimento>("Monte de areia (teste)");
        DefinirRecarrega(monte, Ferramenta.Areia);
        PontoDeReabastecimento picape = TesteUtil.Com<PontoDeReabastecimento>("Picape (teste)");
        TesteUtil.Setar(picape, "ehPicapeDaUpa", true);
        List<PontoDeReabastecimento> origens = new List<PontoDeReabastecimento>();
        kit.OnReabasteceu += origens.Add;

        Assert.IsTrue(kit.Consumir(Ferramenta.Areia));
        monte.Usar(kit);
        Assert.IsTrue(kit.Consumir(Ferramenta.Tela));
        picape.Usar(kit);

        CollectionAssert.AreEqual(new[] { monte, picape }, origens, "um aviso por uso, com o ponto de origem");
        Assert.IsFalse(monte.EhPicapeDaUpa, "o monte de areia não é a picape");
        Assert.IsTrue(picape.EhPicapeDaUpa);
    }

    /* O kit também está cheio: o pneu nas mãos vem antes na ordem das recusas. */
    [Test]
    public void Picape_ComPneu_Recusa()
    {
        PontoDeReabastecimento picape = TesteUtil.Com<PontoDeReabastecimento>("Picape (teste)");
        PneuNasMaos();

        Assert.IsFalse(picape.PodeUsar(kit, out string motivo));
        Assert.AreEqual("Largue o pneu antes.", motivo);
    }

    /* Parte B: o saco não esvazia mais na caçamba (ela só recebe pneus);
     * vai para a lixeira. */
    [Test]
    public void Saco_EsvaziaNaLixeira_ENaoNaCacamba()
    {
        Cacamba cacamba = TesteUtil.Com<Cacamba>("Caçamba (teste)");
        Lixeira lixeira = TesteUtil.Com<Lixeira>("Lixeira (teste)");
        for (int i = 0; i < 3; i++)
            Assert.IsTrue(kit.GuardarGarrafa(), $"garrafa {i + 1}");

        Assert.IsFalse(cacamba.PodeUsar(kit, out _), "a caçamba só recebe pneus");
        cacamba.Usar(kit);
        Assert.AreEqual(3, kit.Estoque.GarrafasNoSaco, "nada saiu na caçamba");

        Assert.IsTrue(lixeira.PodeUsar(kit, out string motivo), $"saco com garrafas (motivo: {motivo})");
        lixeira.Usar(kit);

        Assert.AreEqual(0, kit.Estoque.GarrafasNoSaco);
    }

    /* O pneu entregue conclui e vai para a vaga da carroceria (Parte B:
     * fora do Play, o arremesso é instantâneo). */
    [Test]
    public void Cacamba_EntregaPneu()
    {
        Cacamba cacamba = TesteUtil.Com<Cacamba>("Caçamba (teste)");
        cacamba.transform.position = new Vector3(3f, 0f, 4f);
        Transform vaga = TesteUtil.Novo("Vaga (teste)").transform;
        vaga.SetParent(cacamba.transform, false);
        vaga.localPosition = new Vector3(0f, 1.2f, 0f);
        SerializedObject so = new SerializedObject(cacamba);
        SerializedProperty vagas = so.FindProperty("vagas");
        vagas.arraySize = 1;
        vagas.GetArrayElementAtIndex(0).objectReferenceValue = vaga;
        so.ApplyModifiedPropertiesWithoutUndo();
        PneuInteractable pneu = PneuNasMaos();

        Assert.IsTrue(cacamba.PodeUsar(kit, out string motivo), $"pneu nas mãos (motivo: {motivo})");
        cacamba.Usar(kit);

        Assert.IsTrue(pneu.IsComplete, "só a entrega conclui o pneu");
        Assert.IsNull(kit.PneuCarregado, "mãos livres");
        Assert.That(Vector3.Distance(new Vector3(3f, 1.2f, 4f), pneu.transform.position), Is.LessThan(1e-4f),
                    "na vaga da caçamba");
    }

    /* Parte B: sem pneu e sem picape, a caçamba não tem o que fazer. */
    [Test]
    public void Cacamba_Vazia_Recusa()
    {
        Cacamba cacamba = TesteUtil.Com<Cacamba>("Caçamba (teste)");

        Assert.IsFalse(cacamba.PodeUsar(kit, out string motivo));
        Assert.AreEqual("Só pneu vai na caçamba da picape.", motivo);
    }
}
