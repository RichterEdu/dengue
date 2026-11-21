using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

/* ============================================================
 *  HistoriaFotoTestes.cs
 *  ---------------------------------------------------------------------
 *  Testes EditMode das fotos dos focos na rede social (HistoriaRedeSocial):
 *  quando o jogador empaca (tempoSemFoco sem eliminar foco), o dono do
 *  tipo posta a foto do foco escolhido pelo EscolhaDaFoto, com o foco
 *  ligado ao post. Cobre o relógio (antes do limite, celular aberto),
 *  a foto recusada pela janela de conflito (não gasta e tenta de novo
 *  em 1 s, e a eliminação que zera essa espera), a bronca antiga sem
 *  foco com foto, o foco "mexido" pelo jogador (passo, esvaziar, pneu
 *  pego, tampa pega), o foco destruído (fora da foto, mas lugar conhecido),
 *  o tipo sem dono (perfil da UPA), a ordem dos textos, o
 *  CompletarPersonagens e o PostRedeSocial sem foto/foco no JSON.
 *
 *  O Awake/Start não rodam no EditMode: a RedeSocial, o FluxoDeJogo e o
 *  GerenciadorDeFocos de teste são ligados nos campos, os textos padrão
 *  entram pelo PreencherPadroes (o que o Awake faz) e os focos entram
 *  pelo RegistrarFocos (a parte do Start que lê o gerenciador). A
 *  partida fica "em jogo" e a janelaConflito da RedeSocial vai a 0 (o
 *  Time.time não anda num teste EditMode); o teste da foto recusada sobe
 *  a janela e "passa" a janela voltando-a a 0.
 *
 *  Salvo quando o teste diz outra coisa, os focos ficam a pelo menos
 *  10 m uns dos outros (longe do raioLugarConhecido de 4 m) e o jogador
 *  na origem.
 *
 *  Os objetos nascem pelo TesteUtil (escondidos, fora da cena) e são
 *  destruídos no [TearDown], junto com as texturas de teste.
 *
 *  Rodar:  Window > General > Test Runner > EditMode (categoria Fotos)
 *  ou      unity command run_tests --mode EditMode --filter_type category --filter Fotos
 * ============================================================ */

[Category("Fotos")]
public class HistoriaFotoTestes
{
    private const string FotoVaso1 = "Agente, olha esse vaso. Pratinho com água é convite pro mosquito.";
    private const string FotoSemDono = "Moradores mandaram esta foto pra gente. Agente, dá uma olhada?";

    private static readonly string[] FotosDoPneu =
    {
        "Esse pneu tá largado aqui faz uma semana. Alguém tira isso, pelo amor de Deus?",
        "Tirei foto pra ninguém dizer que não avisei. Pneu velho jogado é casa de mosquito.",
        "Agente, olha o que tem perto de casa. Passa aqui?",
    };

    private HistoriaRedeSocial historia;
    private RedeSocial rede;
    private GerenciadorDeFocos gerenciador;
    private readonly List<Texture2D> texturas = new List<Texture2D>();

    [SetUp]
    public void Preparar()
    {
        rede = TesteUtil.Com<RedeSocial>("Rede (teste)");
        TesteUtil.Setar(rede, "janelaConflito", 0f);

        FluxoDeJogo fluxo = TesteUtil.Com<FluxoDeJogo>("Fluxo (teste)");
        fluxo.DefinirEstadoParaTeste(true, false);

        gerenciador = TesteUtil.Com<GerenciadorDeFocos>("Gerenciador (teste)");

        historia = TesteUtil.Com<HistoriaRedeSocial>("Historia (teste)");
        TesteUtil.Setar(historia, "rede", rede);
        TesteUtil.Setar(historia, "fluxo", fluxo);
        TesteUtil.Setar(historia, "gerenciador", gerenciador);

        // O que o Awake faz: listas vazias recebem os textos padrão.
        MethodInfo preencher = typeof(HistoriaRedeSocial).GetMethod("PreencherPadroes", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(preencher, "HistoriaRedeSocial.PreencherPadroes não encontrado");
        preencher.Invoke(historia, new object[] { false });
    }

    [TearDown]
    public void Limpar()
    {
        TesteUtil.LimparTudo();
        foreach (Texture2D t in texturas)
            if (t != null)
                Object.DestroyImmediate(t);
        texturas.Clear();
    }

    /* ---------------- Ajudantes ---------------- */

    /* Foco de teste do tipo T na posição dada, com FotoDoFoco e uma textura 4×4. */
    private T Foco<T>(string nome, Vector3 posicao, bool comFoto = true) where T : Interactable
    {
        T foco = TesteUtil.Com<T>(nome);
        foco.transform.position = posicao;
        TesteUtil.Setar(foco, "punchScale", 0f);
        TesteUtil.Setar(foco, "shrinkDuration", 0f);
        if (comFoto)
        {
            Texture2D textura = new Texture2D(4, 4) { hideFlags = HideFlags.HideAndDontSave };
            texturas.Add(textura);
            foco.gameObject.AddComponent<FotoDoFoco>().foto = textura;
        }
        return foco;
    }

    private static Texture FotoDe(Interactable foco)
    {
        return foco.GetComponent<FotoDoFoco>().foto;
    }

    /* Registra no gerenciador e no narrador (a parte do Start que lê os focos). */
    private void Registrar(params Interactable[] focos)
    {
        gerenciador.Registrar(focos);
        historia.RegistrarFocos();
    }

    private PostRedeSocial Ultimo => rede.Publicados[rede.Publicados.Count - 1];

    /* ---------------- O relógio ---------------- */

    [Test]
    public void Empacado_PostaFotoDoMaisPerto()
    {
        VasoInteractable vaso = Foco<VasoInteractable>("Vaso (teste)", new Vector3(10f, 0f, 0f));
        PneuInteractable pneu = Foco<PneuInteractable>("Pneu (teste)", new Vector3(0f, 0f, 25f));
        Registrar(vaso, pneu);

        historia.AvancarEmpacado(60f, Vector3.zero, false);

        Assert.AreEqual(1, rede.Publicados.Count, "empacado 60 s tinha que dar um post");
        PostRedeSocial post = rede.Publicados[0];
        Assert.AreSame(FotoDe(vaso), post.foto, "a foto tinha que ser a do foco mais perto");
        Assert.AreSame(vaso, post.foco, "o post tinha que levar o foco");
        Assert.AreEqual("Dona Neusa", post.autor, "vaso = Dona Neusa");
        Assert.AreEqual("@neusa.flores", post.usuario);
        Assert.AreEqual(FotoVaso1, post.texto, "o 1º texto de aoMandarFoto do vaso");
        Assert.AreEqual(HumorPost.Neutro, post.humor);
        Assert.IsTrue(historia.FoiFotografado(vaso), "o vaso tinha que ficar marcado como fotografado");
        Assert.AreSame(vaso, historia.UltimaFoto);
    }

    [Test]
    public void Empacado_AntesDoLimite_NaoPosta()
    {
        Registrar(Foco<VasoInteractable>("Vaso (teste)", new Vector3(10f, 0f, 0f)));

        historia.AvancarEmpacado(59.9f, Vector3.zero, false);

        Assert.AreEqual(0, rede.Publicados.Count, "antes de tempoSemFoco não sai nada");
    }

    [Test]
    public void Empacado_CelularAberto_NaoPosta()
    {
        Registrar(Foco<VasoInteractable>("Vaso (teste)", new Vector3(10f, 0f, 0f)));

        historia.AvancarEmpacado(120f, Vector3.zero, true);

        Assert.AreEqual(0, rede.Publicados.Count, "com o celular aberto o relógio não anda");
    }

    [Test]
    public void Empacado_SegueEmpacado_OutroFoco()
    {
        VasoInteractable vaso = Foco<VasoInteractable>("Vaso (teste)", new Vector3(10f, 0f, 0f));
        PneuInteractable pneu = Foco<PneuInteractable>("Pneu (teste)", new Vector3(0f, 0f, 25f));
        Registrar(vaso, pneu);

        historia.AvancarEmpacado(60f, Vector3.zero, false);
        historia.AvancarEmpacado(60f, Vector3.zero, false);

        Assert.AreEqual(2, rede.Publicados.Count, "cada tempoSemFoco empacado dá uma foto");
        Assert.AreSame(vaso, rede.Publicados[0].foco);
        Assert.AreSame(pneu, rede.Publicados[1].foco, "a segunda foto tinha que ser de outro foco");
    }

    /* Recusada pela janela de conflito: nada é gasto (nem o foco nem o
     * texto), o relógio fica no limite e a nova tentativa vem em 1 s. */
    [Test]
    public void Empacado_Recusada_NaoGastaETentaDeNovo()
    {
        VasoInteractable vaso = Foco<VasoInteractable>("Vaso (teste)", new Vector3(10f, 0f, 0f));
        Registrar(vaso, Foco<PneuInteractable>("Pneu (teste)", new Vector3(0f, 0f, 25f)));

        TesteUtil.Setar(rede, "janelaConflito", 100f);
        Assert.IsTrue(rede.Postar(new PostRedeSocial { texto = "Aviso Alta de antes." }, PrioridadePost.Alta),
            "o post Alta de antes tinha que sair");

        historia.AvancarEmpacado(60f, Vector3.zero, false);
        Assert.AreEqual(1, rede.Publicados.Count, "dentro da janela, a foto (Normal) tinha que ser recusada");
        Assert.IsFalse(historia.FoiFotografado(vaso), "a foto recusada não marca o foco");

        TesteUtil.Setar(rede, "janelaConflito", 0f);
        historia.AvancarEmpacado(0.5f, Vector3.zero, false);
        Assert.AreEqual(1, rede.Publicados.Count, "a nova tentativa só vem 1 s depois da recusa");

        historia.AvancarEmpacado(0.6f, Vector3.zero, false);
        Assert.AreEqual(2, rede.Publicados.Count, "passado 1 s, a foto tinha que sair");
        Assert.AreSame(FotoDe(vaso), Ultimo.foto);
        Assert.AreEqual(FotoVaso1, Ultimo.texto, "a recusa não gasta o texto: sai o 1º");
        Assert.IsTrue(historia.FoiFotografado(vaso));
    }

    /* Foco eliminado depois de uma recusa: o relógio zera e a espera de 1 s
     * da foto recusada também (a foto sai assim que o relógio chega ao limite). */
    [Test]
    public void Empacado_EliminacaoDepoisDaRecusa_ZeraAEspera()
    {
        VasoInteractable vaso = Foco<VasoInteractable>("Vaso (teste)", new Vector3(10f, 0f, 0f));
        Interactable lata = Foco<Interactable>("Lata (teste)", new Vector3(-20f, 0f, 0f));
        Registrar(vaso, lata, Foco<PneuInteractable>("Pneu (teste)", new Vector3(0f, 0f, 25f)));

        TesteUtil.Setar(rede, "janelaConflito", 100f);
        Assert.IsTrue(rede.Postar(new PostRedeSocial { texto = "Aviso Alta de antes." }, PrioridadePost.Alta));
        historia.AvancarEmpacado(60f, Vector3.zero, false);
        Assert.IsFalse(historia.FoiFotografado(vaso), "dentro da janela, a foto tinha que ser recusada");

        TesteUtil.Setar(rede, "janelaConflito", 0f);
        MethodInfo eliminado = typeof(HistoriaRedeSocial).GetMethod("HandleFocoEliminado", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(eliminado, "HistoriaRedeSocial.HandleFocoEliminado não encontrado");
        eliminado.Invoke(historia, new object[] { lata });

        historia.AvancarEmpacado(59.5f, Vector3.zero, false);
        Assert.IsFalse(historia.FoiFotografado(vaso), "a eliminação zera o relógio");
        historia.AvancarEmpacado(0.6f, Vector3.zero, false);
        Assert.IsTrue(historia.FoiFotografado(vaso), "no limite, a foto sai sem a espera da recusa de antes");
        Assert.AreSame(vaso, Ultimo.foco);
    }

    [Test]
    public void Empacado_SemFocoComFoto_SaiABronca()
    {
        Registrar(Foco<VasoInteractable>("Vaso (teste)", new Vector3(10f, 0f, 0f), comFoto: false),
                  Foco<PneuInteractable>("Pneu (teste)", new Vector3(0f, 0f, 25f), comFoto: false));

        historia.AvancarEmpacado(60f, Vector3.zero, false);

        Assert.AreEqual(1, rede.Publicados.Count, "sem foco com foto, sai a bronca antiga");
        Assert.AreEqual("Faz tempo que ninguém vê o agente limpar foco nenhum. Meu filho piorou de noite. Cadê ele?",
            rede.Publicados[0].texto, "o 1º de aoDemorarSemFoco");
        Assert.IsNull(rede.Publicados[0].foto);
        Assert.IsNull(rede.Publicados[0].foco);

        // A bronca zera o relógio, como antes.
        historia.AvancarEmpacado(59.9f, Vector3.zero, false);
        Assert.AreEqual(1, rede.Publicados.Count, "depois da bronca o relógio recomeça do zero");
    }

    /* ---------------- Mexido pelo jogador ---------------- */

    [Test]
    public void Mexido_NaoSaiEmFoto()
    {
        VasoInteractable vaso = Foco<VasoInteractable>("Vaso (teste)", new Vector3(10f, 0f, 0f));
        PneuInteractable pneu = Foco<PneuInteractable>("Pneu (teste)", new Vector3(0f, 0f, 25f));
        Registrar(vaso, pneu);

        historia.HandleFocoClicado(vaso);
        historia.AvancarEmpacado(60f, Vector3.zero, false);

        Assert.AreSame(pneu, Ultimo.foco, "o foco que recebeu um passo não sai em foto");
    }

    [Test]
    public void Mexido_PorEsvaziar_NaoSaiEmFoto()
    {
        VasoInteractable vaso = Foco<VasoInteractable>("Vaso (teste)", new Vector3(10f, 0f, 0f));
        PneuInteractable pneu = Foco<PneuInteractable>("Pneu (teste)", new Vector3(0f, 0f, 25f));
        Registrar(vaso, pneu);

        historia.HandleFocoEsvaziado(vaso);
        historia.AvancarEmpacado(60f, Vector3.zero, false);

        Assert.IsNotNull(Ultimo.foco, "tinha que sair uma foto");
        Assert.AreSame(pneu, Ultimo.foco, "o foco esvaziado com as Mãos não sai em foto");
    }

    [Test]
    public void Mexido_PorPneuPego_NaoSaiEmFoto()
    {
        PneuInteractable pneu = Foco<PneuInteractable>("Pneu (teste)", new Vector3(10f, 0f, 0f));
        VasoInteractable vaso = Foco<VasoInteractable>("Vaso (teste)", new Vector3(0f, 0f, 25f));
        Registrar(pneu, vaso);

        historia.HandlePneuPego(pneu);
        historia.AvancarEmpacado(60f, Vector3.zero, false);

        Assert.IsNotNull(Ultimo.foco, "tinha que sair uma foto");
        Assert.AreSame(vaso, Ultimo.foco, "o pneu pego não sai em foto");
    }

    [Test]
    public void Mexido_PorTampaPega_NaoSaiEmFoto()
    {
        CaixaDaguaInteractable caixa = Foco<CaixaDaguaInteractable>("Caixa (teste)", new Vector3(10f, 0f, 0f));
        VasoInteractable vaso = Foco<VasoInteractable>("Vaso (teste)", new Vector3(0f, 0f, 25f));
        Registrar(caixa, vaso);

        historia.HandleTampaPega(caixa);
        historia.AvancarEmpacado(60f, Vector3.zero, false);

        Assert.AreSame(vaso, Ultimo.foco, "a caixa com a tampa pega não sai em foto");
    }

    /* ---------------- Focos fora da foto ---------------- */

    [Test]
    public void Empacado_FocoDestruido_Ignora()
    {
        VasoInteractable vaso = Foco<VasoInteractable>("Vaso (teste)", new Vector3(10f, 0f, 0f));
        PneuInteractable pneu = Foco<PneuInteractable>("Pneu (teste)", new Vector3(0f, 0f, 25f));
        Registrar(vaso, pneu);

        Object.DestroyImmediate(vaso.gameObject);
        Assert.DoesNotThrow(() => historia.AvancarEmpacado(60f, Vector3.zero, false));

        Assert.AreEqual(1, rede.Publicados.Count);
        Assert.AreSame(pneu, rede.Publicados[0].foco, "o foco destruído fica de fora");
    }

    /* Só o Interactable.FinishRemoval (depois de concluir) destrói um foco:
     * destruído = eliminado, e o lugar dele continua conhecido (spec §4). */
    [Test]
    public void Empacado_FocoDestruido_ContinuaLugarConhecido()
    {
        VasoInteractable vaso = Foco<VasoInteractable>("Vaso (teste)", new Vector3(10f, 0f, 0f));
        Interactable lata = Foco<Interactable>("Lata (teste)", new Vector3(12f, 0f, 0f));
        PneuInteractable pneu = Foco<PneuInteractable>("Pneu (teste)", new Vector3(0f, 0f, 25f));
        Registrar(vaso, lata, pneu);

        Object.DestroyImmediate(lata.gameObject);
        historia.AvancarEmpacado(60f, Vector3.zero, false);

        Assert.AreEqual(1, rede.Publicados.Count);
        Assert.AreSame(pneu, rede.Publicados[0].foco,
            "o vaso a 2 m do foco destruído fica fora da 1ª passada; sai o pneu, longe");
    }

    [Test]
    public void Empacado_FocoConcluido_NaoSaiEmFoto()
    {
        Interactable concluido = Foco<Interactable>("Concluído (teste)", new Vector3(10f, 0f, 0f));
        TesteUtil.Setar(concluido, "requiredClicks", 1);
        PneuInteractable pneu = Foco<PneuInteractable>("Pneu (teste)", new Vector3(0f, 0f, 25f));
        Registrar(concluido, pneu);

        concluido.Interact();
        Assert.IsTrue(concluido.IsComplete, "o foco de teste tinha que concluir");
        historia.AvancarEmpacado(60f, Vector3.zero, false);

        Assert.AreSame(pneu, Ultimo.foco, "o foco concluído não sai em foto");
    }

    /* ---------------- Quem posta ---------------- */

    [Test]
    public void TipoSemDono_PostaComoUpa()
    {
        Interactable foco = Foco<Interactable>("Foco sem tipo (teste)", new Vector3(10f, 0f, 0f));
        Registrar(foco);

        historia.AvancarEmpacado(60f, Vector3.zero, false);

        Assert.AreEqual(1, rede.Publicados.Count);
        Assert.AreEqual("UPA Vila Nova", rede.Publicados[0].autor, "tipo sem dono = perfil da UPA");
        Assert.AreEqual("@upa.vilanova", rede.Publicados[0].usuario);
        Assert.AreEqual(FotoSemDono, rede.Publicados[0].texto);
        Assert.AreSame(foco, rede.Publicados[0].foco);
    }

    [Test]
    public void AoMandarFoto_SegueOrdemDepoisSorteiaSemRepetir()
    {
        List<Interactable> pneus = new List<Interactable>();
        for (int i = 0; i < 6; i++)
            pneus.Add(Foco<PneuInteractable>("Pneu " + i + " (teste)", new Vector3(10f * (i + 1), 0f, 0f)));
        Registrar(pneus.ToArray());

        for (int i = 0; i < 6; i++)
            historia.AvancarEmpacado(60f, Vector3.zero, false);

        Assert.AreEqual(6, rede.Publicados.Count, "seis empacados, seis fotos");
        for (int i = 0; i < 3; i++)
            Assert.AreEqual(FotosDoPneu[i], rede.Publicados[i].texto, $"o post {i + 1} tinha que seguir a ordem");
        for (int i = 3; i < 6; i++)
        {
            CollectionAssert.Contains(FotosDoPneu, rede.Publicados[i].texto, $"o post {i + 1} não é um texto do Carlos");
            Assert.AreNotEqual(rede.Publicados[i - 1].texto, rede.Publicados[i].texto,
                $"os posts {i} e {i + 1} repetiram o mesmo texto");
        }
    }

    [Test]
    public void CompletarPersonagens_PreencheAoMandarFotoVazio()
    {
        HistoriaRedeSocial antiga = TesteUtil.Com<HistoriaRedeSocial>("História antiga (teste)");

        // Dois personagens salvos: o do Ralo sem aoMandarFoto (salvo antes do
        // campo novo) e o do Vaso com um texto próprio.
        SerializedObject so = new SerializedObject(antiga);
        SerializedProperty lista = so.FindProperty("personagens");
        lista.arraySize = 2;
        SerializedProperty ralo = lista.GetArrayElementAtIndex(0);
        ralo.FindPropertyRelative("autor").stringValue = "Vizinho do Ralo";
        ralo.FindPropertyRelative("tipo").intValue = (int)TipoFoco.Ralo;
        SerializedProperty fotoRalo = ralo.FindPropertyRelative("aoMandarFoto");
        Assert.IsNotNull(fotoRalo, "PersonagemDoBairro não tem o campo serializado aoMandarFoto.");
        fotoRalo.arraySize = 0;
        SerializedProperty vaso = lista.GetArrayElementAtIndex(1);
        vaso.FindPropertyRelative("autor").stringValue = "Vizinha do Vaso";
        vaso.FindPropertyRelative("tipo").intValue = (int)TipoFoco.Vaso;
        SerializedProperty fotoVaso = vaso.FindPropertyRelative("aoMandarFoto");
        fotoVaso.arraySize = 1;
        fotoVaso.GetArrayElementAtIndex(0).stringValue = "texto próprio do vaso";
        so.ApplyModifiedPropertiesWithoutUndo();

        antiga.CompletarPersonagens();

        so.Update();
        lista = so.FindProperty("personagens");
        SerializedProperty raloDepois = lista.GetArrayElementAtIndex(0);
        Assert.AreEqual("Vizinho do Ralo", raloDepois.FindPropertyRelative("autor").stringValue,
            "o personagem que já estava na lista não pode ser trocado");
        SerializedProperty falasRalo = raloDepois.FindPropertyRelative("aoMandarFoto");
        Assert.AreEqual(1, falasRalo.arraySize, "o aoMandarFoto vazio recebe o padrão do tipo");
        Assert.AreEqual("Esse ralo tá sem tela faz tempo e junta água toda vez que chove. Agente, consegue ver?",
            falasRalo.GetArrayElementAtIndex(0).stringValue);

        SerializedProperty falasVaso = lista.GetArrayElementAtIndex(1).FindPropertyRelative("aoMandarFoto");
        Assert.AreEqual(1, falasVaso.arraySize, "o aoMandarFoto preenchido não muda");
        Assert.AreEqual("texto próprio do vaso", falasVaso.GetArrayElementAtIndex(0).stringValue);
    }

    /* ---------------- O dado do post ---------------- */

    [Test]
    public void PostRedeSocial_FotoEFocoNaoSaoSerializados()
    {
        Texture2D textura = new Texture2D(4, 4) { hideFlags = HideFlags.HideAndDontSave };
        texturas.Add(textura);
        PostRedeSocial post = new PostRedeSocial
        {
            texto = "post com foto",
            foto = textura,
            foco = TesteUtil.Com<Interactable>("Foco (teste)"),
        };

        string json = JsonUtility.ToJson(post);

        StringAssert.DoesNotContain("\"foto\"", json);
        StringAssert.DoesNotContain("\"foco\"", json);
        StringAssert.Contains("post com foto", json, "o resto do post continua no JSON");
    }
}
