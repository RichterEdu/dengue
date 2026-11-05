using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;

/* ============================================================
 *  HistoriaChuvaTestes.cs
 *  ---------------------------------------------------------------------
 *  Testes EditMode (Parte B) dos posts novos da rede social
 *  (HistoriaRedeSocial): a previsão de chuva, o primeiro esvaziar com as
 *  Mãos, o primeiro foco reenchido pela chuva (o post escolhido pelo
 *  tipo do foco) e o primeiro pneu entregue na picape da UPA. Também a
 *  previsão recusada pela janela de conflito (fica pendente e sai depois,
 *  ou é descartada se a chuva começar) e o pneu esvaziado, que não é o
 *  atalho e não posta. O post do primeiro pneu passa pela entrega real
 *  (KitDoAgente.EntregarPneu, gerenciador, HandleFocoEliminado), com a
 *  janela de conflito ligada: era ela que o recusava.
 *
 *  O Awake/Start não rodam no EditMode: a RedeSocial e o FluxoDeJogo de
 *  teste são ligados nos campos e os handlers (internal, visíveis pelo
 *  AssemblyInfo.cs) são chamados direto, como se o evento tivesse
 *  disparado. A partida fica "em jogo" (DefinirEstadoParaTeste) e a
 *  janelaConflito da RedeSocial vai a 0, para um post não recusar o
 *  seguinte — assim o teste mede só a regra da HistoriaRedeSocial. Os
 *  testes da previsão recusada sobem a janela (o Time.time não anda num
 *  teste EditMode) e "passam" a janela voltando-a a 0.
 *
 *  As listas usam os textos padrão (já nascem preenchidas).
 *
 *  Os objetos nascem pelo TesteUtil (escondidos, fora da cena) e são
 *  destruídos no [TearDown].
 *
 *  Rodar:  Window > General > Test Runner > EditMode (categoria ParteB)
 *  ou      unity command run_tests --mode EditMode --filter_type category --filter ParteB
 * ============================================================ */

[Category("ParteB")]
public class HistoriaChuvaTestes
{
    private HistoriaRedeSocial historia;
    private RedeSocial rede;

    [SetUp]
    public void Preparar()
    {
        rede = TesteUtil.Com<RedeSocial>("Rede (teste)");
        TesteUtil.Setar(rede, "janelaConflito", 0f);

        FluxoDeJogo fluxo = TesteUtil.Com<FluxoDeJogo>("Fluxo (teste)");
        fluxo.DefinirEstadoParaTeste(true, false);

        historia = TesteUtil.Com<HistoriaRedeSocial>("Historia (teste)");
        TesteUtil.Setar(historia, "rede", rede);
        TesteUtil.Setar(historia, "fluxo", fluxo);
    }

    [TearDown]
    public void Limpar()
    {
        TesteUtil.LimparTudo();
    }

    private static readonly HashSet<string> TextosPrevisao = new HashSet<string>
    {
        "Previsão de chuva forte daqui a pouco aqui no bairro! Tudo que estiver só esvaziado vai encher de novo.",
        "O céu fechou do lado da praça. Vai cair água já já.",
        "Recolhi a roupa do varal, que vem chuva aí. Quem esvaziou vaso, bota areia antes que encha.",
        "Trovoada chegando. Daqui a pouco chove forte no bairro.",
    };

    /* Uma Chuva de teste (sem fluxo nem gerenciador) andada até a previsão. */
    private static Chuva ChuvaEmPrevisao()
    {
        Chuva chuva = TesteUtil.Com<Chuva>("Chuva (teste)");
        chuva.Iniciar(new System.Random(7));
        for (int s = 0; s < 1000 && chuva.Estado != EstadoDaChuva.Previsao; s++)
            chuva.Avancar(1f);
        Assert.AreEqual(EstadoDaChuva.Previsao, chuva.Estado, "a chuva de teste tinha que chegar à previsão");
        return chuva;
    }

    [Test]
    public void Previsao_Posta_SemRepetirOAnterior()
    {
        // A regra vale em qualquer sorteio: várias previsões seguidas, cada
        // uma com um post, e nenhum igual ao da previsão anterior.
        const int previsoes = 20;
        for (int i = 0; i < previsoes; i++)
        {
            historia.HandlePrevisaoChuva();
            Assert.AreEqual(i + 1, rede.Publicados.Count, $"a previsão {i + 1} tinha que dar um post");
        }

        for (int i = 0; i < previsoes; i++)
        {
            Assert.IsTrue(TextosPrevisao.Contains(rede.Publicados[i].texto),
                $"a previsão {i + 1} não é um dos 4 textos padrão: \"{rede.Publicados[i].texto}\"");
            if (i > 0)
                Assert.AreNotEqual(rede.Publicados[i - 1].texto, rede.Publicados[i].texto,
                    $"as previsões {i} e {i + 1} repetiram o mesmo post");
        }
    }

    /* A janela de conflito recusa a previsão (um post Alta acabou de
     * sair): ela fica pendente e sai na nova tentativa, depois que a
     * janela passa, porque a chuva ainda está em previsão. */
    [Test]
    public void Previsao_Recusada_SaiDepoisDaJanela()
    {
        Chuva chuva = ChuvaEmPrevisao();
        TesteUtil.Setar(historia, "chuva", chuva);
        TesteUtil.Setar(rede, "janelaConflito", 1000f);

        Assert.IsTrue(rede.Postar(new PostRedeSocial { texto = "Aviso Alta de antes." }, PrioridadePost.Alta),
            "o post Alta de antes tinha que sair");
        historia.HandlePrevisaoChuva();
        Assert.AreEqual(1, rede.Publicados.Count, "dentro da janela, a previsão (Alta) tinha que ser recusada");

        // Ainda na janela: a nova tentativa também é recusada, sem perder a pendente.
        historia.TentarPrevisaoPendente();
        Assert.AreEqual(1, rede.Publicados.Count, "ainda na janela, a nova tentativa tinha que ser recusada");

        // A janela passa: a pendente sai, uma vez só.
        TesteUtil.Setar(rede, "janelaConflito", 0f);
        historia.TentarPrevisaoPendente();
        Assert.AreEqual(2, rede.Publicados.Count, "passada a janela, a previsão pendente tinha que sair");
        Assert.IsTrue(TextosPrevisao.Contains(rede.Publicados[1].texto),
            $"o post que saiu não é uma previsão: \"{rede.Publicados[1].texto}\"");

        historia.TentarPrevisaoPendente();
        Assert.AreEqual(2, rede.Publicados.Count, "a previsão pendente só sai uma vez");
    }

    /* Recusada e a chuva já começou: a pendente é descartada. Vale pela
     * conferência do estado na nova tentativa e pelo OnComecou. */
    [Test]
    public void Previsao_Recusada_DescartadaAoComecarAChover()
    {
        Chuva chuva = ChuvaEmPrevisao();
        TesteUtil.Setar(historia, "chuva", chuva);
        TesteUtil.Setar(rede, "janelaConflito", 1000f);

        rede.Postar(new PostRedeSocial { texto = "Aviso Alta de antes." }, PrioridadePost.Alta);
        historia.HandlePrevisaoChuva();
        Assert.AreEqual(1, rede.Publicados.Count, "dentro da janela, a previsão tinha que ser recusada");

        // Começa a chover (a previsão dura 45 s).
        chuva.Avancar(50f);
        Assert.AreEqual(EstadoDaChuva.Chovendo, chuva.Estado, "a chuva tinha que ter começado");

        TesteUtil.Setar(rede, "janelaConflito", 0f);
        historia.TentarPrevisaoPendente();
        Assert.AreEqual(1, rede.Publicados.Count, "com a chuva começada, a previsão pendente tinha que ser descartada");

        // Pelo evento: recusada de novo (o aviso Alta de antes ainda está na
        // janela, porque o Time.time não andou) e OnComecou antes da nova
        // tentativa, com uma chuva ainda em previsão.
        Chuva outra = ChuvaEmPrevisao();
        TesteUtil.Setar(historia, "chuva", outra);
        TesteUtil.Setar(rede, "janelaConflito", 1000f);
        historia.HandlePrevisaoChuva();
        Assert.AreEqual(1, rede.Publicados.Count, "a segunda previsão tinha que ser recusada");
        historia.HandleChuvaComecou();
        TesteUtil.Setar(rede, "janelaConflito", 0f);
        historia.TentarPrevisaoPendente();
        Assert.AreEqual(1, rede.Publicados.Count, "o OnComecou tinha que descartar a previsão pendente");
    }

    [Test]
    public void PrimeiroEsvaziar_Posta1x()
    {
        VasoInteractable vaso = TesteUtil.Com<VasoInteractable>("Vaso (teste)");
        GarrafaInteractable garrafas = TesteUtil.Com<GarrafaInteractable>("Garrafas (teste)");

        historia.HandleFocoEsvaziado(vaso);
        historia.HandleFocoEsvaziado(garrafas);

        Assert.AreEqual(1, rede.Publicados.Count, "dois esvaziados tinham que dar um post só");
        Assert.AreEqual("Lúcia Andrade", rede.Publicados[0].autor);
        Assert.AreEqual("@lu.plantas", rede.Publicados[0].usuario);
        Assert.AreEqual("Esvaziar ajuda, mas os ovos ficam grudados na parede. Se chover, eles voltam.",
            rede.Publicados[0].texto);
    }

    /* O pneu esvaziado é o caminho normal da ação definitiva, não o
     * atalho: não posta e não gasta o gatilho do primeiro esvaziar. */
    [Test]
    public void PneuEsvaziado_NaoPosta_NemGastaOGatilho()
    {
        PneuInteractable pneu = TesteUtil.Com<PneuInteractable>("Pneu (teste)");
        VasoInteractable vaso = TesteUtil.Com<VasoInteractable>("Vaso (teste)");

        historia.HandleFocoEsvaziado(pneu);
        Assert.AreEqual(0, rede.Publicados.Count, "o pneu esvaziado não pode postar");

        historia.HandleFocoEsvaziado(vaso);
        Assert.AreEqual(1, rede.Publicados.Count, "o vaso esvaziado depois do pneu tinha que postar");
        Assert.AreEqual("Lúcia Andrade", rede.Publicados[0].autor);
    }

    [Test]
    public void PrimeiroReenchido_EscolhePorTipo()
    {
        VasoInteractable vaso = TesteUtil.Com<VasoInteractable>("Vaso (teste)");
        PneuInteractable pneu = TesteUtil.Com<PneuInteractable>("Pneu (teste)");

        historia.HandleFocoReenchido(vaso);

        Assert.AreEqual(1, rede.Publicados.Count, "o primeiro reenchido tinha que dar um post");
        Assert.AreEqual("Dona Neusa", rede.Publicados[0].autor, "vaso reenchido = post da Dona Neusa");
        Assert.AreEqual("Choveu e aquele pratinho já está cheio de larva de novo. Tem que pôr areia.",
            rede.Publicados[0].texto);

        // Um segundo reenchido, mesmo de outro tipo (que tem post), não posta.
        historia.HandleFocoReenchido(pneu);
        Assert.AreEqual(1, rede.Publicados.Count, "só o primeiro reenchido da partida posta");
    }

    /* Pneu de teste sem punch nem animação de conclusão. */
    private static PneuInteractable NovoPneu(string nome)
    {
        PneuInteractable pneu = TesteUtil.Com<PneuInteractable>(nome);
        TesteUtil.Setar(pneu, "punchScale", 0f);
        TesteUtil.Setar(pneu, "shrinkDuration", 0f);
        return pneu;
    }

    [Test]
    public void PrimeiroPneuNaPicape_PelaEntregaReal_Posta1x()
    {
        ControladorJogador jogador = TesteUtil.Com<ControladorJogador>("Jogador (teste)");
        KitDoAgente kit = jogador.gameObject.AddComponent<KitDoAgente>();
        kit.PontoDeCarga = TesteUtil.Novo("Ponto de carga (teste)").transform;
        Cacamba cacamba = TesteUtil.Com<Cacamba>("Caçamba (teste)");

        // Três pneus: o segundo ainda não é o último do tipo.
        PneuInteractable pneu1 = NovoPneu("Pneu 1 (teste)");
        PneuInteractable pneu2 = NovoPneu("Pneu 2 (teste)");
        PneuInteractable pneu3 = NovoPneu("Pneu 3 (teste)");
        GerenciadorDeFocos gerenciador = TesteUtil.Com<GerenciadorDeFocos>("Gerenciador (teste)");
        gerenciador.Registrar(new Interactable[] { pneu1, pneu2, pneu3 });

        // O Start não roda no EditMode; é ele que junta os focos e assina o
        // gerenciador. A janela de combo negativa desliga a sequência rápida
        // (o Time.time não anda aqui, e todo foco cairia nela).
        TesteUtil.Setar(historia, "gerenciador", gerenciador);
        TesteUtil.Setar(historia, "janelaCombo", -1f);
        historia.CompletarPersonagens();
        MethodInfo start = typeof(HistoriaRedeSocial).GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(start, "HistoriaRedeSocial.Start não encontrado");
        start.Invoke(historia, null);

        // Com a janela de conflito ligada, como na cena: um post só por vez.
        TesteUtil.Setar(rede, "janelaConflito", 10f);
        Assert.IsTrue(kit.Pegar(pneu1), "pneu 1 nos braços");
        Assert.IsTrue(kit.EntregarPneu(cacamba), "pneu 1 entregue");

        Assert.AreEqual(1, rede.Publicados.Count, "a entrega dá um post só");
        Assert.AreEqual("Carlos Mendes", rede.Publicados[0].autor);
        Assert.AreEqual("Primeiro pneu na caçamba da picape da UPA! Um criadouro a menos no bairro.",
            rede.Publicados[0].texto);

        // "Passa" a janela: o segundo pneu já é a fala do dono.
        TesteUtil.Setar(rede, "janelaConflito", 0f);
        Assert.IsTrue(kit.Pegar(pneu2), "pneu 2 nos braços");
        Assert.IsTrue(kit.EntregarPneu(cacamba), "pneu 2 entregue");

        Assert.AreEqual(2, rede.Publicados.Count, "o post da picape não repete");
        Assert.AreEqual("Lá se vai mais um na picape da UPA. Faltam 1, e eu tô contando daqui da janela.",
            rede.Publicados[1].texto);
    }

    /* Nenhum texto do dono do pneu pode dizer "primeiro": o primeiro pneu é
     * sempre o post da picape. */
    [Test]
    public void FalasDoCarlos_NaoFalamDoPrimeiroPneu()
    {
        historia.CompletarPersonagens();
        SerializedObject so = new SerializedObject(historia);
        SerializedProperty personagens = so.FindProperty("personagens");
        for (int i = 0; i < personagens.arraySize; i++)
        {
            SerializedProperty p = personagens.GetArrayElementAtIndex(i);
            if (p.FindPropertyRelative("tipo").intValue != (int)TipoFoco.Pneu)
                continue;

            SerializedProperty falas = p.FindPropertyRelative("aoEliminar");
            Assert.AreEqual(HistoriaRedeSocial.FalaCarlosDepoisDoPrimeiroPneu, falas.GetArrayElementAtIndex(0).stringValue);
            for (int f = 0; f < falas.arraySize; f++)
                StringAssert.DoesNotContain("primeiro", falas.GetArrayElementAtIndex(f).stringValue.ToLowerInvariant());
            return;
        }
        Assert.Fail("o personagem do pneu não existe");
    }

    /* A cena guarda os textos antigos, e o CompletarPersonagens só preenche
     * campo vazio: o builder regrava os revistos, sem mexer nos outros tipos. */
    [Test]
    public void RegravarTextosDaParteB_TrocaOsRevistos_SemMexerNoResto()
    {
        HistoriaRedeSocial historia = TesteUtil.Com<HistoriaRedeSocial>("História antiga (teste)");
        historia.CompletarPersonagens();
        SerializedObject so = new SerializedObject(historia);
        SerializedProperty personagens = so.FindProperty("personagens");
        int pneu = -1, garrafas = -1, vaso = -1;
        for (int i = 0; i < personagens.arraySize; i++)
        {
            SerializedProperty p = personagens.GetArrayElementAtIndex(i);
            int tipo = p.FindPropertyRelative("tipo").intValue;
            if (tipo == (int)TipoFoco.Pneu) pneu = i;
            if (tipo == (int)TipoFoco.Garrafas) garrafas = i;
            if (tipo == (int)TipoFoco.Vaso) vaso = i;
            p.FindPropertyRelative("aoUsarFerramentaErrada").stringValue = "texto antigo: pra caçamba";
        }
        SerializedProperty falasPneu = personagens.GetArrayElementAtIndex(pneu).FindPropertyRelative("aoEliminar");
        falasPneu.GetArrayElementAtIndex(0).stringValue = "O agente esvaziou o primeiro pneu.";
        falasPneu.GetArrayElementAtIndex(1).stringValue = "segunda fala, editada na cena";
        so.ApplyModifiedPropertiesWithoutUndo();

        historia.RegravarTextosDaParteB();

        so.Update();
        string TextoDe(int i) => personagens.GetArrayElementAtIndex(i).FindPropertyRelative("aoUsarFerramentaErrada").stringValue;
        StringAssert.Contains("picape da UPA", TextoDe(pneu));
        StringAssert.Contains("lixeira", TextoDe(garrafas));
        Assert.AreEqual("texto antigo: pra caçamba", TextoDe(vaso), "os outros tipos ficam como estão");
        Assert.AreEqual(HistoriaRedeSocial.FalaCarlosDepoisDoPrimeiroPneu, falasPneu.GetArrayElementAtIndex(0).stringValue,
            "a 1ª fala do Carlos sai a partir do 2º pneu");
        Assert.AreEqual("segunda fala, editada na cena", falasPneu.GetArrayElementAtIndex(1).stringValue, "as outras falas ficam");

        StringAssert.Contains("picape da UPA", so.FindProperty("aoCarregarPneu").GetArrayElementAtIndex(0).FindPropertyRelative("texto").stringValue);
        StringAssert.Contains("lixeira", so.FindProperty("aoEsvaziarSacoPrimeiraVez").GetArrayElementAtIndex(0).FindPropertyRelative("texto").stringValue);
        Assert.AreEqual(1, so.FindProperty("aoEntregarPneuPrimeiraVez").arraySize);
        Assert.AreEqual(4, so.FindProperty("aoPreverChuva").arraySize);
        Assert.AreEqual(1, so.FindProperty("aoEsvaziarPrimeiraVez").arraySize);
        Assert.AreEqual(3, so.FindProperty("aoReencherPrimeiraVez").arraySize);
    }
}
