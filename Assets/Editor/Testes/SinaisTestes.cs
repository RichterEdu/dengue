using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;

/* ============================================================
 *  SinaisTestes.cs
 *  ---------------------------------------------------------------------
 *  Testes EditMode (Parte B) dos sinais do estágio dos focos:
 *
 *    - o enxame de mosquitos (EnxameDeMosquitos): no Mosquito, o foco
 *      ganha o filho "Enxame" com um ParticleSystem em loop (espaço
 *      local, esfera do raio, ruído ligado, maxParticles = particulas);
 *      em outro estágio, ao esvaziar e ao eliminar, o filho sai; o
 *      Ligar já põe o enxame nos focos que estão no Mosquito; o brilho
 *      de emissão (mira, proximidade) pula o ParticleSystemRenderer e o
 *      material continua um só para todos os enxames;
 *    - os textos da HUD (DengueHUD): o complemento do estágio e do
 *      esvaziado na mira, a ação de resolver de vez, as recusas
 *      (já vazio, saco cheio, mãos ocupadas), o pneu vazio, o aviso de
 *      esvaziado (no pneu, o do pneu vazio) e o "Pneu recolhido — N de
 *      T"; o artigo de cada ferramenta (Ferramentas.Artigo/ComArtigo); e
 *      a linha do pneu da barra do kit (KitHUD);
 *    - o aviso na entrega do pneu: com vaga na carroceria, o contador
 *      conta e o de eliminado não sai antes do "Pneu recolhido".
 *
 *  O Start não roda no EditMode: o enxame é chamado direto pelo
 *  Atualizar(foco, estagio) ou, nos testes com o gerenciador, ligado a
 *  ele pelo Ligar(). Fora do Play o filho sai na hora (DestroyImmediate).
 *  Os focos de teste não têm punch (punchScale 0) nem animação de
 *  conclusão (shrinkDuration 0: o OnComplete só desativa o objeto). O
 *  kit fica num objeto com ControladorJogador e começa com as Mãos.
 *
 *  Os objetos nascem pelo TesteUtil (escondidos, fora da cena) e são
 *  destruídos no [TearDown].
 *
 *  Rodar:  Window > General > Test Runner > EditMode (categoria ParteB)
 *  ou      unity command run_tests --mode EditMode --filter_type category --filter ParteB
 * ============================================================ */

[Category("ParteB")]
public class SinaisTestes
{
    private EnxameDeMosquitos enxame;
    private GerenciadorDeFocos gerenciador;
    private KitDoAgente kit;

    [SetUp]
    public void Preparar()
    {
        gerenciador = TesteUtil.Com<GerenciadorDeFocos>("Gerenciador (teste)");
        TesteUtil.Setar(gerenciador, "fluxo", TesteUtil.Com<FluxoDeJogo>("Fluxo (teste)"));

        enxame = TesteUtil.Com<EnxameDeMosquitos>("Enxame de mosquitos (teste)");
        TesteUtil.Setar(enxame, "gerenciador", gerenciador);

        ControladorJogador jogador = TesteUtil.Com<ControladorJogador>("Jogador (teste)");
        kit = jogador.gameObject.AddComponent<KitDoAgente>();
    }

    [TearDown]
    public void Limpar()
    {
        TesteUtil.LimparTudo();
    }

    /* Foco de teste sem punch nem animação de conclusão. */
    private static T NovoFoco<T>(string nome, Ferramenta ferramenta) where T : Interactable
    {
        T foco = TesteUtil.Com<T>(nome);
        TesteUtil.Setar(foco, "ferramentaNecessaria", ferramenta);
        TesteUtil.Setar(foco, "requiredClicks", 1);
        TesteUtil.Setar(foco, "punchScale", 0f);
        TesteUtil.Setar(foco, "shrinkDuration", 0f);
        return foco;
    }

    private static Transform FilhoEnxame(Interactable foco)
    {
        return foco.transform.Find("Enxame");
    }

    /* ---------------- Enxame de mosquitos ---------------- */

    [Test]
    public void Enxame_Mosquito_CriaFilho_Larva_Remove()
    {
        Interactable foco = NovoFoco<Interactable>("Foco (teste)", Ferramenta.Maos);

        enxame.Atualizar(foco, EstagioDoFoco.Mosquito);

        Transform filho = FilhoEnxame(foco);
        Assert.IsNotNull(filho, "no Mosquito, o filho \"Enxame\" existe");
        ParticleSystem ps = filho.GetComponent<ParticleSystem>();
        Assert.IsNotNull(ps, "com ParticleSystem");
        Assert.IsTrue(ps.main.loop, "em loop");
        Assert.AreEqual(ParticleSystemSimulationSpace.Local, ps.main.simulationSpace, "espaço local");
        Assert.AreEqual(40, ps.main.maxParticles, "maxParticles = particulas");
        Assert.AreEqual(0f, filho.GetComponent<ParticleSystemRenderer>().minParticleSize, "sem tamanho mínimo: encolhe com a distância");
        Assert.AreEqual(ParticleSystemShapeType.Sphere, ps.shape.shapeType, "esfera");
        Assert.AreEqual(0.9f, ps.shape.radius, 1e-4f, "raio");
        Assert.IsTrue(ps.noise.enabled, "ruído ligado");
        Assert.AreEqual(0.03f, ps.main.startSize.constantMin, 1e-4f, "partícula de 0,03 m");
        Assert.AreEqual(0.05f, ps.main.startSize.constantMax, 1e-4f, "a 0,05 m");

        enxame.Atualizar(foco, EstagioDoFoco.Mosquito);
        Assert.AreEqual(1, foco.transform.childCount, "de novo no Mosquito: continua um enxame só");

        enxame.Atualizar(foco, EstagioDoFoco.Larva);
        Assert.IsNull(FilhoEnxame(foco), "na Larva, o enxame sai");
    }

    /* Pelo gerenciador: o relógio leva o vaso ao Mosquito (OnEstagioMudou)
     * e o atalho das Mãos o esvazia (Ovo e OnFocoEsvaziado). */
    [Test]
    public void Enxame_Esvaziado_Remove()
    {
        VasoInteractable vaso = NovoFoco<VasoInteractable>("Vaso (teste)", Ferramenta.Areia);
        TesteUtil.Setar(vaso, "podeEsvaziar", true);
        gerenciador.Registrar(new Interactable[] { vaso });
        enxame.Ligar();

        gerenciador.Avancar(160f);
        Assert.AreEqual(EstagioDoFoco.Mosquito, gerenciador.EstagioDe(vaso));
        Assert.IsNotNull(FilhoEnxame(vaso), "no Mosquito, o enxame aparece");

        for (int i = 0; i < 3; i++)
            vaso.AvancarCom(kit);

        Assert.IsTrue(vaso.Esvaziado);
        Assert.IsNull(FilhoEnxame(vaso), "esvaziado, o enxame sai");
    }

    [Test]
    public void Enxame_Eliminado_Remove()
    {
        Interactable foco = NovoFoco<Interactable>("Foco (teste)", Ferramenta.Maos);
        gerenciador.Registrar(new Interactable[] { foco });
        enxame.Ligar();

        gerenciador.Avancar(160f);
        Assert.IsNotNull(FilhoEnxame(foco), "no Mosquito, o enxame aparece");

        Assert.AreEqual(ResultadoDoPasso.Concluiu, foco.AvancarCom(kit));
        Assert.IsNull(FilhoEnxame(foco), "eliminado, o enxame sai");
    }

    /* Focos que já estavam no Mosquito antes do Ligar (o sorteio do
     * início, por exemplo) ganham o enxame na hora; o eliminado não. */
    [Test]
    public void Enxame_Ligar_FocoJaNoMosquito_CriaEnxame()
    {
        Interactable vivo = NovoFoco<Interactable>("Foco vivo (teste)", Ferramenta.Maos);
        Interactable eliminado = NovoFoco<Interactable>("Foco eliminado (teste)", Ferramenta.Maos);
        gerenciador.Registrar(new Interactable[] { vivo, eliminado });

        gerenciador.Avancar(160f);
        Assert.AreEqual(EstagioDoFoco.Mosquito, gerenciador.EstagioDe(vivo));
        Assert.AreEqual(ResultadoDoPasso.Concluiu, eliminado.AvancarCom(kit));
        Assert.IsNull(FilhoEnxame(vivo), "antes do Ligar, nada");

        enxame.Ligar();

        Assert.IsNotNull(FilhoEnxame(vivo), "o Ligar põe o enxame no foco que já está no Mosquito");
        Assert.IsNull(FilhoEnxame(eliminado), "no eliminado, não");
    }

    /* O brilho de emissão (FocoProximityGlow e, pela mesma regra, o
     * destaque do Interactor e o flash do Interactable) pula o
     * ParticleSystemRenderer do enxame: sem GetColor("_EmissionColor") no
     * material de partícula (erro no Console) e sem cópia do material. */
    [Test]
    public void Enxame_ForaDoBrilhoDeEmissao_MaterialCompartilhado()
    {
        Interactable a = NovoFoco<Interactable>("Foco A (teste)", Ferramenta.Maos);
        Interactable b = NovoFoco<Interactable>("Foco B (teste)", Ferramenta.Maos);
        enxame.Atualizar(a, EstagioDoFoco.Mosquito);
        enxame.Atualizar(b, EstagioDoFoco.Mosquito);

        ParticleSystemRenderer psrA = FilhoEnxame(a).GetComponent<ParticleSystemRenderer>();
        ParticleSystemRenderer psrB = FilhoEnxame(b).GetComponent<ParticleSystemRenderer>();
        Material compartilhado = psrA.sharedMaterial;
        Assert.IsNotNull(compartilhado);
        Assert.AreSame(compartilhado, psrB.sharedMaterial, "um material só para todos os enxames");

        Assert.IsFalse(EmissionHighlightUtil.Destacavel(psrA), "partícula não entra no brilho");
        var capturados = EmissionHighlightUtil.Capture(a.GetComponentsInChildren<Renderer>());
        Assert.AreEqual(0, capturados.Count, "o Capture do foco com enxame não pega o ParticleSystemRenderer");
        Assert.AreSame(compartilhado, psrA.sharedMaterial, "o material do enxame continua o compartilhado");
    }

    /* ---------------- Textos da HUD ---------------- */

    [Test]
    public void Complementos_Textos()
    {

        Assert.AreEqual("Segure com a <b>areia</b> para resolver de vez", DengueHUD.AcaoResolverDeVez(Ferramenta.Areia));
        Assert.AreEqual("Pneu vazio: leve à <b>picape da UPA</b>", DengueHUD.TextoPneuVazio);
        Assert.AreEqual("Saco cheio. Jogue numa <b>lixeira</b>.", DengueHUD.RecusaSacoCheio);
        Assert.AreEqual("Mãos ocupadas: leve o pneu à <b>picape da UPA</b> ou largue [botão direito].", DengueHUD.RecusaMaosOcupadas);
        Assert.AreEqual("Carregando pneu. Leve à <b>picape da UPA</b>. [Shift] corre. [Botão direito] larga.", KitHUD.TextoLinhaPneu);

        // Decisões do controlador: a ação do atalho (sem repetir o "segure")
        // e o aviso ao esvaziar.
        Assert.AreEqual("Segure para <b>esvaziar</b> (a chuva enche de novo)", DengueHUD.AcaoEsvaziar);
        Assert.AreEqual("Esvaziado, por enquanto. A chuva enche de novo.", DengueHUD.AvisoEsvaziado);
    }

    [Test]
    public void RecusaJaVazio_Areia()
    {
        Assert.AreEqual("Já está vazio, mas a chuva enche de novo. A <b>areia</b> resolve de vez.",
                        DengueHUD.RecusaJaVazio(Ferramenta.Areia));
    }

    /* As garrafas (saco de lixo) também têm o atalho: o artigo concorda. */
    [Test]
    public void RecusaJaVazio_E_ResolverDeVez_SacoDeLixo()
    {
        Assert.AreEqual("Já está vazio, mas a chuva enche de novo. O <b>saco de lixo</b> resolve de vez.",
                        DengueHUD.RecusaJaVazio(Ferramenta.SacoDeLixo));
        Assert.AreEqual("Segure com o <b>saco de lixo</b> para resolver de vez",
                        DengueHUD.AcaoResolverDeVez(Ferramenta.SacoDeLixo));
    }

    [Test]
    public void Ferramentas_ComArtigo()
    {
        Assert.AreEqual("as mãos", Ferramentas.ComArtigo(Ferramenta.Maos));
        Assert.AreEqual("a areia", Ferramentas.ComArtigo(Ferramenta.Areia));
        Assert.AreEqual("a tela", Ferramentas.ComArtigo(Ferramenta.Tela));
        Assert.AreEqual("a água sanitária", Ferramentas.ComArtigo(Ferramenta.AguaSanitaria));
        Assert.AreEqual("o saco de lixo", Ferramentas.ComArtigo(Ferramenta.SacoDeLixo));
        Assert.AreEqual("O saco de lixo", Ferramentas.ComArtigo(Ferramenta.SacoDeLixo, true));
        Assert.AreEqual("As mãos", Ferramentas.ComArtigo(Ferramenta.Maos, true));
        Assert.AreEqual("A", Ferramentas.Artigo(Ferramenta.AguaSanitaria, true));
    }

    /* No pneu, o aviso do esvaziar já diz o próximo passo (levar à picape). */
    [Test]
    public void AvisoAoEsvaziar_Pneu_E_OutrosFocos()
    {
        PneuInteractable pneu = TesteUtil.Com<PneuInteractable>("Pneu (teste)");
        VasoInteractable vaso = TesteUtil.Com<VasoInteractable>("Vaso (teste)");

        Assert.AreEqual("Pneu vazio: leve à <b>picape da UPA</b>", DengueHUD.AvisoAoEsvaziar(pneu));
        Assert.AreEqual("Esvaziado, por enquanto. A chuva enche de novo.", DengueHUD.AvisoAoEsvaziar(vaso));
    }

    [Test]
    public void TextoPneuRecolhido_3de6()
    {
        Assert.AreEqual("Pneu recolhido — 3 de 6", DengueHUD.TextoPneuRecolhido(3, 6));
    }

    /* ---------------- Aviso na entrega do pneu ---------------- */

    /* HUD de teste com o aviso e o contador ligados. */
    private DengueHUD NovaHud(out TextMeshProUGUI aviso, out TextMeshProUGUI contador)
    {
        DengueHUD hud = TesteUtil.Com<DengueHUD>("HUD (teste)");
        aviso = TesteUtil.Com<TextMeshProUGUI>("Aviso (teste)");
        contador = TesteUtil.Com<TextMeshProUGUI>("Contador (teste)");
        TesteUtil.Setar(hud, "toastGroup", TesteUtil.Com<CanvasGroup>("Painel do aviso (teste)"));
        TesteUtil.Setar(hud, "toastText", aviso);
        TesteUtil.Setar(hud, "counterText", contador);
        TesteUtil.Setar(hud, "gerenciador", gerenciador);
        return hud;
    }

    /* Pneu nos braços entregue numa caçamba com 'vagas' vagas. */
    private PneuInteractable PneuEntregue(int vagas)
    {
        Cacamba cacamba = TesteUtil.Com<Cacamba>("Caçamba (teste)");
        SerializedObject so = new SerializedObject(cacamba);
        SerializedProperty lista = so.FindProperty("vagas");
        lista.arraySize = vagas;
        for (int i = 0; i < vagas; i++)
        {
            Transform vaga = TesteUtil.Novo("Vaga (teste)").transform;
            vaga.SetParent(cacamba.transform, false);
            lista.GetArrayElementAtIndex(i).objectReferenceValue = vaga;
        }
        so.ApplyModifiedPropertiesWithoutUndo();

        kit.PontoDeCarga = TesteUtil.Novo("Ponto de carga (teste)").transform;
        PneuInteractable pneu = NovoFoco<PneuInteractable>("Pneu (teste)", Ferramenta.Maos);
        Assert.IsTrue(kit.Pegar(pneu), "pneu nos braços");
        Assert.IsTrue(kit.EntregarPneu(cacamba), "pneu entregue");
        return pneu;
    }

    [Test]
    public void Entrega_ComVaga_ContaSemAvisoDeEliminado()
    {
        DengueHUD hud = NovaHud(out TextMeshProUGUI aviso, out TextMeshProUGUI contador);
        PneuInteractable pneu = PneuEntregue(1);
        Assert.IsNotNull(pneu.VagaNaCacamba, "a vaga é reservada na entrega");
        string contadorAntes = contador.text;

        hud.HandleCompleted(pneu);

        Assert.IsTrue(string.IsNullOrEmpty(aviso.text), "o aviso de eliminado não sai: '" + aviso.text + "'");
        Assert.AreNotEqual(contadorAntes, contador.text, "o contador conta na entrega");

        hud.HandlePneuRecebido(pneu, 1);
        StringAssert.StartsWith("Pneu recolhido — 1 de", aviso.text);
    }

    [Test]
    public void Entrega_SemVaga_MostraOAvisoDeEliminado()
    {
        DengueHUD hud = NovaHud(out TextMeshProUGUI aviso, out _);
        PneuInteractable pneu = PneuEntregue(0);
        Assert.IsNull(pneu.VagaNaCacamba);

        hud.HandleCompleted(pneu);

        Assert.IsFalse(string.IsNullOrEmpty(aviso.text), "sem vaga, o aviso de eliminado continua");
    }

    [Test]
    public void OutroFoco_MostraOAvisoDeEliminado()
    {
        DengueHUD hud = NovaHud(out TextMeshProUGUI aviso, out _);

        hud.HandleCompleted(NovoFoco<VasoInteractable>("Vaso (teste)", Ferramenta.Areia));

        Assert.IsFalse(string.IsNullOrEmpty(aviso.text));
    }
}
