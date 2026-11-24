using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

/* ============================================================
 *  HistoriaFotoTestes.cs
 *  ---------------------------------------------------------------------
 *  Testes EditMode das fotos dos focos na rede social (HistoriaRedeSocial):
 *  um relógio por tipo de foco (90 s sem eliminar foco daquele tipo, com
 *  começo escalonado de 15 s entre os tipos); vencido, o dono do tipo
 *  posta a foto de um foco daquele tipo escolhido pelo EscolhaDaFoto, com
 *  o foco ligado ao post. Cobre o relógio (limite, escalonamento,
 *  eliminação que zera só o tipo, foto que zera o tipo, celular aberto,
 *  temposPorTipo), a foto recusada pela janela de conflito (não gasta e
 *  tenta de novo em 1 s; dois tipos vencidos juntos saem um por vez), o
 *  tipo sem foco elegível (zera sem postar) e o tipo todo eliminado, o
 *  foco "mexido" pelo jogador (passo, esvaziar, pneu
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

    /* Só os posts com foto (eliminar um foco também posta a fala do dono, sem foto). */
    private List<PostRedeSocial> Fotos => new List<PostRedeSocial>(rede.Publicados).FindAll(p => p.foto != null);

    private void Avancar(float segundos, bool celularAberto = false)
    {
        historia.AvancarRelogiosDosTipos(segundos, Vector3.zero, celularAberto);
    }

    /* O que o OnFocoEliminado do gerenciador faria (o Start não roda aqui). */
    private void Eliminar(Interactable foco)
    {
        MethodInfo eliminado = typeof(HistoriaRedeSocial).GetMethod("HandleFocoEliminado", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(eliminado, "HistoriaRedeSocial.HandleFocoEliminado não encontrado");
        eliminado.Invoke(historia, new object[] { foco });
    }

    /* ---------------- O relógio por tipo ----------------
     * Tipos na ordem do enum TipoFoco (Nenhum, Pneu, Vaso, Caixa...): o
     * i-ésimo tipo presente começa atrasado i × 15 s; cada um vence aos 90 s. */

    [Test]
    public void Tipo_VenceAos90s_PostaFotoDoTipo()
    {
        PneuInteractable pneu = Foco<PneuInteractable>("Pneu (teste)", new Vector3(0f, 0f, 20f));
        VasoInteractable vaso = Foco<VasoInteractable>("Vaso (teste)", new Vector3(5f, 0f, 0f));
        Registrar(pneu, vaso);

        Avancar(89.9f);
        Assert.AreEqual(0, rede.Publicados.Count, "antes dos 90 s não sai nada");

        Avancar(0.2f);
        Assert.AreEqual(1, rede.Publicados.Count, "aos 90 s sai a foto do 1º tipo");
        PostRedeSocial post = rede.Publicados[0];
        Assert.AreSame(pneu, post.foco, "a foto é do tipo vencido (pneu), mesmo com o vaso mais perto");
        Assert.AreSame(FotoDe(pneu), post.foto);
        Assert.AreEqual("Carlos Mendes", post.autor, "pneu = Carlos Mendes");
        Assert.AreEqual("@carlinhos.m", post.usuario);
        Assert.AreEqual(FotosDoPneu[0], post.texto, "o 1º texto de aoMandarFoto do pneu");
        Assert.AreEqual(HumorPost.Neutro, post.humor);
        Assert.IsTrue(historia.FoiFotografado(pneu));
        Assert.AreSame(pneu, historia.UltimaFoto);
        Assert.AreSame(pneu, historia.UltimaFotoDoTipo(TipoFoco.Pneu));
        Assert.IsNull(historia.UltimaFotoDoTipo(TipoFoco.Vaso));
    }

    [Test]
    public void Tipo_Escalonado_SegundoTipoVence15sDepois()
    {
        PneuInteractable pneu = Foco<PneuInteractable>("Pneu (teste)", new Vector3(0f, 0f, 20f));
        VasoInteractable vaso = Foco<VasoInteractable>("Vaso (teste)", new Vector3(10f, 0f, 0f));
        Registrar(pneu, vaso);

        Avancar(90.1f);
        Assert.AreEqual(1, rede.Publicados.Count, "aos 90 s só o 1º tipo (pneu)");
        Assert.AreSame(pneu, rede.Publicados[0].foco);

        Avancar(14.8f);
        Assert.AreEqual(1, rede.Publicados.Count, "o 2º tipo começa 15 s atrasado");

        Avancar(0.3f);
        Assert.AreEqual(2, rede.Publicados.Count, "aos 105 s sai o 2º tipo (vaso)");
        Assert.AreSame(vaso, rede.Publicados[1].foco);
        Assert.AreEqual("Dona Neusa", rede.Publicados[1].autor);
        Assert.AreEqual(FotoVaso1, rede.Publicados[1].texto);
    }

    [Test]
    public void Tipo_EliminarDoTipo_ZeraSoEle()
    {
        PneuInteractable pneu1 = Foco<PneuInteractable>("Pneu 1 (teste)", new Vector3(0f, 0f, 20f));
        PneuInteractable pneu2 = Foco<PneuInteractable>("Pneu 2 (teste)", new Vector3(0f, 0f, 40f));
        VasoInteractable vaso = Foco<VasoInteractable>("Vaso (teste)", new Vector3(10f, 0f, 0f));
        Registrar(pneu1, pneu2, vaso);

        Avancar(85f);
        Eliminar(pneu2);
        Avancar(10f);

        Assert.AreEqual(0, Fotos.Count, "eliminar um pneu zerou o relógio dos pneus (o post que sai é a fala do dono ao eliminar, sem foto)");
        Assert.AreEqual(10f, historia.RelogioDoTipo(TipoFoco.Pneu), 1e-3f, "pneu: 10 s desde a eliminação");
        Assert.AreEqual(80f, historia.RelogioDoTipo(TipoFoco.Vaso), 1e-3f, "vaso: −15 + 95, não zerou");

        Avancar(10f);
        Assert.AreEqual(1, Fotos.Count, "o vaso vence aos 90 s dele");
        Assert.AreSame(vaso, Fotos[0].foco);
    }

    /* Eliminar um foco de um tipo que ainda está no atraso do começo não
     * adianta a foto dele: o relógio negativo fica como está. */
    [Test]
    public void Tipo_EliminarNoAtraso_NaoAdiantaAFoto()
    {
        PneuInteractable pneu = Foco<PneuInteractable>("Pneu (teste)", new Vector3(0f, 0f, 20f));
        VasoInteractable vaso1 = Foco<VasoInteractable>("Vaso 1 (teste)", new Vector3(10f, 0f, 0f));
        VasoInteractable vaso2 = Foco<VasoInteractable>("Vaso 2 (teste)", new Vector3(30f, 0f, 0f));
        Registrar(pneu, vaso1, vaso2);

        Avancar(5f);
        Assert.AreEqual(-10f, historia.RelogioDoTipo(TipoFoco.Vaso), 1e-3f, "vaso: −15 + 5");
        Eliminar(vaso1);

        Assert.AreEqual(-10f, historia.RelogioDoTipo(TipoFoco.Vaso), 1e-3f,
            "eliminar no atraso não leva o relógio a 0 (a foto não chega antes)");
    }

    [Test]
    public void Tipo_FotoQueSaiu_ZeraORelogioDoTipo()
    {
        PneuInteractable pneu1 = Foco<PneuInteractable>("Pneu 1 (teste)", new Vector3(0f, 0f, 20f));
        PneuInteractable pneu2 = Foco<PneuInteractable>("Pneu 2 (teste)", new Vector3(0f, 0f, 40f));
        Registrar(pneu1, pneu2);

        Avancar(90.1f);
        Assert.AreEqual(1, rede.Publicados.Count);
        Assert.AreEqual(0f, historia.RelogioDoTipo(TipoFoco.Pneu), 1e-3f, "a foto que saiu zera o relógio do tipo");

        Avancar(89f);
        Assert.AreEqual(1, rede.Publicados.Count, "a próxima só depois de mais 90 s");

        Avancar(1.2f);
        Assert.AreEqual(2, rede.Publicados.Count);
        Assert.AreSame(pneu2, rede.Publicados[1].foco, "a segunda foto é de outro pneu");
    }

    [Test]
    public void Tipo_CelularAberto_Pausa()
    {
        Registrar(Foco<PneuInteractable>("Pneu (teste)", new Vector3(0f, 0f, 20f)),
                  Foco<VasoInteractable>("Vaso (teste)", new Vector3(10f, 0f, 0f)));

        Avancar(300f, celularAberto: true);

        Assert.AreEqual(0, rede.Publicados.Count, "com o celular aberto os relógios não andam");
        Assert.AreEqual(0f, historia.RelogioDoTipo(TipoFoco.Pneu), 1e-3f);
        Assert.AreEqual(-15f, historia.RelogioDoTipo(TipoFoco.Vaso), 1e-3f, "o vaso continua com o atraso do começo");
    }

    /* Recusada pela janela de conflito: nada é gasto (nem o foco nem o
     * texto), o relógio fica no limite e a nova tentativa vem em 1 s. */
    [Test]
    public void Tipo_Recusada_NaoGastaETentaDe1Em1s()
    {
        PneuInteractable pneu = Foco<PneuInteractable>("Pneu (teste)", new Vector3(0f, 0f, 20f));
        Registrar(pneu);

        TesteUtil.Setar(rede, "janelaConflito", 100f);
        Assert.IsTrue(rede.Postar(new PostRedeSocial { texto = "Aviso Alta de antes." }, PrioridadePost.Alta),
            "o post Alta de antes tinha que sair");

        Avancar(90.1f);
        Assert.AreEqual(1, rede.Publicados.Count, "dentro da janela, a foto (Normal) tinha que ser recusada");
        Assert.IsFalse(historia.FoiFotografado(pneu), "a foto recusada não marca o foco");

        TesteUtil.Setar(rede, "janelaConflito", 0f);
        Avancar(0.5f);
        Assert.AreEqual(1, rede.Publicados.Count, "a nova tentativa só vem 1 s depois da recusa");

        Avancar(0.6f);
        Assert.AreEqual(2, rede.Publicados.Count, "passado 1 s, a foto tinha que sair");
        Assert.AreSame(pneu, Ultimo.foco);
        Assert.AreEqual(FotosDoPneu[0], Ultimo.texto, "a recusa não gasta o texto: sai o 1º");
    }

    /* Foco do tipo eliminado depois de uma recusa: o relógio do tipo zera e
     * a espera de 1 s da foto recusada também. */
    [Test]
    public void Tipo_EliminacaoDepoisDaRecusa_ZeraAEspera()
    {
        PneuInteractable pneu1 = Foco<PneuInteractable>("Pneu 1 (teste)", new Vector3(0f, 0f, 20f));
        PneuInteractable pneu2 = Foco<PneuInteractable>("Pneu 2 (teste)", new Vector3(0f, 0f, 40f));
        Registrar(pneu1, pneu2);

        TesteUtil.Setar(rede, "janelaConflito", 100f);
        Assert.IsTrue(rede.Postar(new PostRedeSocial { texto = "Aviso Alta de antes." }, PrioridadePost.Alta));
        Avancar(90.1f);
        Assert.IsFalse(historia.FoiFotografado(pneu1), "dentro da janela, a foto tinha que ser recusada");

        TesteUtil.Setar(rede, "janelaConflito", 0f);
        Eliminar(pneu2);

        Avancar(89.5f);
        Assert.IsFalse(historia.FoiFotografado(pneu1), "a eliminação zera o relógio do tipo");
        Avancar(0.6f);
        Assert.IsTrue(historia.FoiFotografado(pneu1), "no limite, a foto sai sem a espera da recusa de antes");
    }

    /* Os dois tipos vencem no mesmo quadro (o celular ficou aberto e depois
     * passaram 105 s): saem um por vez, pela janela de conflito. */
    [Test]
    public void Tipo_DoisVencidosJuntos_SaemUmDepoisDoOutro()
    {
        PneuInteractable pneu = Foco<PneuInteractable>("Pneu (teste)", new Vector3(0f, 0f, 20f));
        VasoInteractable vaso = Foco<VasoInteractable>("Vaso (teste)", new Vector3(10f, 0f, 0f));
        Registrar(pneu, vaso);

        Avancar(200f, celularAberto: true);
        TesteUtil.Setar(rede, "janelaConflito", 4f);
        Avancar(105.1f);
        Assert.AreEqual(1, rede.Publicados.Count, "só um aviso passa pela janela de conflito");
        Assert.AreSame(pneu, rede.Publicados[0].foco);

        TesteUtil.Setar(rede, "janelaConflito", 0f);
        Avancar(1.1f);
        Assert.AreEqual(2, rede.Publicados.Count, "o vaso, recusado, sai na nova tentativa");
        Assert.AreSame(vaso, rede.Publicados[1].foco);
    }

    /* Tipo sem foco que possa sair em foto: o relógio zera, sem post (e sem
     * a bronca antiga, que ficaria errada para quem limpa outros tipos). */
    [Test]
    public void Tipo_SemElegivel_ZeraSemPostar()
    {
        PneuInteractable pneu = Foco<PneuInteractable>("Pneu (teste)", new Vector3(0f, 0f, 20f));
        Registrar(pneu);

        historia.HandleFocoClicado(pneu);
        Avancar(90.1f);

        Assert.AreEqual(0, rede.Publicados.Count, "sem foco elegível não sai foto nem bronca");
        Assert.AreEqual(0f, historia.RelogioDoTipo(TipoFoco.Pneu), 1e-3f, "o relógio do tipo zerou");
    }

    [Test]
    public void Tipo_TodoEliminado_NaoPostaMais()
    {
        PneuInteractable pneu = Foco<PneuInteractable>("Pneu (teste)", new Vector3(0f, 0f, 20f));
        VasoInteractable vaso = Foco<VasoInteractable>("Vaso (teste)", new Vector3(10f, 0f, 0f));
        Registrar(pneu, vaso);

        Eliminar(pneu);
        historia.HandleFocoClicado(vaso);
        Avancar(500f);

        Assert.AreEqual(0, Fotos.Count, "tipo todo eliminado não posta foto; o vaso mexido também não (a fala de eliminar o pneu não tem foto)");
    }

    [Test]
    public void Tipo_TemposPorTipo_Sobrepoe()
    {
        PneuInteractable pneu = Foco<PneuInteractable>("Pneu (teste)", new Vector3(0f, 0f, 20f));
        VasoInteractable vaso = Foco<VasoInteractable>("Vaso (teste)", new Vector3(10f, 0f, 0f));
        Registrar(pneu, vaso);

        SerializedObject so = new SerializedObject(historia);
        SerializedProperty lista = so.FindProperty("temposPorTipo");
        Assert.IsNotNull(lista, "HistoriaRedeSocial não tem o campo serializado temposPorTipo.");
        lista.arraySize = 1;
        lista.GetArrayElementAtIndex(0).FindPropertyRelative("tipo").intValue = (int)TipoFoco.Vaso;
        lista.GetArrayElementAtIndex(0).FindPropertyRelative("segundos").floatValue = 30f;
        so.ApplyModifiedPropertiesWithoutUndo();
        historia.ReiniciarRelogiosDosTipos();

        Avancar(45.1f);

        Assert.AreEqual(1, rede.Publicados.Count, "o vaso (−15 s, limite 30 s) vence aos 45 s");
        Assert.AreSame(vaso, rede.Publicados[0].foco);
    }

    /* ---------------- Mexido pelo jogador ---------------- */

    [Test]
    public void Mexido_NaoSaiEmFoto()
    {
        VasoInteractable vaso1 = Foco<VasoInteractable>("Vaso 1 (teste)", new Vector3(10f, 0f, 0f));
        VasoInteractable vaso2 = Foco<VasoInteractable>("Vaso 2 (teste)", new Vector3(0f, 0f, 25f));
        Registrar(vaso1, vaso2);

        historia.HandleFocoClicado(vaso1);
        Avancar(90.1f);

        Assert.AreSame(vaso2, Ultimo.foco, "o foco que recebeu um passo não sai em foto");
    }

    [Test]
    public void Mexido_PorEsvaziar_NaoSaiEmFoto()
    {
        VasoInteractable vaso1 = Foco<VasoInteractable>("Vaso 1 (teste)", new Vector3(10f, 0f, 0f));
        VasoInteractable vaso2 = Foco<VasoInteractable>("Vaso 2 (teste)", new Vector3(0f, 0f, 25f));
        Registrar(vaso1, vaso2);

        historia.HandleFocoEsvaziado(vaso1);
        Avancar(90.1f);

        Assert.IsNotNull(Ultimo.foco, "tinha que sair uma foto");
        Assert.AreSame(vaso2, Ultimo.foco, "o foco esvaziado com as Mãos não sai em foto");
    }

    [Test]
    public void Mexido_PorPneuPego_NaoSaiEmFoto()
    {
        PneuInteractable pneu1 = Foco<PneuInteractable>("Pneu 1 (teste)", new Vector3(10f, 0f, 0f));
        PneuInteractable pneu2 = Foco<PneuInteractable>("Pneu 2 (teste)", new Vector3(0f, 0f, 25f));
        Registrar(pneu1, pneu2);

        historia.HandlePneuPego(pneu1);
        Avancar(90.1f);

        Assert.IsNotNull(Ultimo.foco, "tinha que sair uma foto");
        Assert.AreSame(pneu2, Ultimo.foco, "o pneu pego não sai em foto");
    }

    [Test]
    public void Mexido_PorTampaPega_NaoSaiEmFoto()
    {
        CaixaDaguaInteractable caixa1 = Foco<CaixaDaguaInteractable>("Caixa 1 (teste)", new Vector3(10f, 0f, 0f));
        CaixaDaguaInteractable caixa2 = Foco<CaixaDaguaInteractable>("Caixa 2 (teste)", new Vector3(0f, 0f, 25f));
        Registrar(caixa1, caixa2);

        historia.HandleTampaPega(caixa1);
        Avancar(90.1f);

        Assert.AreSame(caixa2, Ultimo.foco, "a caixa com a tampa pega não sai em foto");
    }

    /* ---------------- Focos fora da foto ---------------- */

    [Test]
    public void Tipo_FocoDestruido_Ignora()
    {
        VasoInteractable vaso1 = Foco<VasoInteractable>("Vaso 1 (teste)", new Vector3(10f, 0f, 0f));
        VasoInteractable vaso2 = Foco<VasoInteractable>("Vaso 2 (teste)", new Vector3(0f, 0f, 25f));
        Registrar(vaso1, vaso2);

        Object.DestroyImmediate(vaso1.gameObject);
        Assert.DoesNotThrow(() => Avancar(90.1f));

        Assert.AreEqual(1, rede.Publicados.Count);
        Assert.AreSame(vaso2, rede.Publicados[0].foco, "o foco destruído fica de fora");
    }

    /* Só o Interactable.FinishRemoval (depois de concluir) destrói um foco:
     * destruído = eliminado, e o lugar dele continua conhecido (spec §4),
     * mesmo sendo de outro tipo. A lata é do tipo Nenhum (o 1º na ordem),
     * então o vaso é o 2º tipo e vence aos 105 s. */
    [Test]
    public void Tipo_FocoDestruido_ContinuaLugarConhecido()
    {
        VasoInteractable vaso1 = Foco<VasoInteractable>("Vaso 1 (teste)", new Vector3(10f, 0f, 0f));
        Interactable lata = Foco<Interactable>("Lata (teste)", new Vector3(12f, 0f, 0f));
        VasoInteractable vaso2 = Foco<VasoInteractable>("Vaso 2 (teste)", new Vector3(0f, 0f, 25f));
        Registrar(vaso1, lata, vaso2);

        Object.DestroyImmediate(lata.gameObject);
        Avancar(105.1f);

        Assert.AreEqual(1, rede.Publicados.Count);
        Assert.AreSame(vaso2, rede.Publicados[0].foco,
            "o vaso a 2 m do foco destruído fica fora da 1ª passada; sai o outro vaso, longe");
    }

    [Test]
    public void Tipo_FocoConcluido_NaoSaiEmFoto()
    {
        Interactable concluido = Foco<Interactable>("Concluído (teste)", new Vector3(10f, 0f, 0f));
        TesteUtil.Setar(concluido, "requiredClicks", 1);
        Interactable outro = Foco<Interactable>("Outro (teste)", new Vector3(0f, 0f, 25f));
        Registrar(concluido, outro);

        concluido.Interact();
        Assert.IsTrue(concluido.IsComplete, "o foco de teste tinha que concluir");
        Avancar(90.1f);

        Assert.AreSame(outro, Ultimo.foco, "o foco concluído não sai em foto");
    }

    /* ---------------- Quem posta ---------------- */

    [Test]
    public void TipoSemDono_PostaComoUpa()
    {
        Interactable foco = Foco<Interactable>("Foco sem tipo (teste)", new Vector3(10f, 0f, 0f));
        Registrar(foco);

        Avancar(90.1f);

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
            Avancar(90.1f);

        Assert.AreEqual(6, rede.Publicados.Count, "seis vezes 90 s, seis fotos");
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

        // Dois personagens salvos: o do Bueiro sem aoMandarFoto (salvo antes do
        // campo novo) e o do Vaso com um texto próprio.
        SerializedObject so = new SerializedObject(antiga);
        SerializedProperty lista = so.FindProperty("personagens");
        lista.arraySize = 2;
        SerializedProperty bueiro = lista.GetArrayElementAtIndex(0);
        bueiro.FindPropertyRelative("autor").stringValue = "Vizinho do Bueiro";
        bueiro.FindPropertyRelative("tipo").intValue = (int)TipoFoco.Bueiro;
        SerializedProperty fotoBueiro = bueiro.FindPropertyRelative("aoMandarFoto");
        Assert.IsNotNull(fotoBueiro, "PersonagemDoBairro não tem o campo serializado aoMandarFoto.");
        fotoBueiro.arraySize = 0;
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
        SerializedProperty bueiroDepois = lista.GetArrayElementAtIndex(0);
        Assert.AreEqual("Vizinho do Bueiro", bueiroDepois.FindPropertyRelative("autor").stringValue,
            "o personagem que já estava na lista não pode ser trocado");
        SerializedProperty falasBueiro = bueiroDepois.FindPropertyRelative("aoMandarFoto");
        Assert.AreEqual(1, falasBueiro.arraySize, "o aoMandarFoto vazio recebe o padrão do tipo");
        Assert.AreEqual("Esse bueiro tá sem tela faz tempo e junta água toda vez que chove. Agente, consegue ver?",
            falasBueiro.GetArrayElementAtIndex(0).stringValue);

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
