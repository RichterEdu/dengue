using TMPro;
using UnityEditor;
using UnityEngine;

/* ============================================================
 *  PersonagensHospitalBuilder.cs  (Editor)
 *  ---------------------------------------------------------------------
 *  Arquivo companheiro do QuartoHospitalBuilder.cs (mesmo padrão de
 *  TerrenoBaldioMato.cs/TerrenoBaldioAssets.cs): SEM [MenuItem] próprio,
 *  só um método público chamado de dentro de QuartoHospitalBuilder.Criar()
 *  logo depois de MobiliarSala(). Isso garante que os personagens nascem
 *  e morrem junto com a sala — nenhuma lógica extra de idempotência é
 *  necessária, já que Criar() já apaga e reconstrói "SalaDoHospital"
 *  inteira a cada rodada.
 *
 *  Preenche a recepção com a ENFERMEIRA RITA (visual estático, sem
 *  animação, pedido no contexto do jogo.md) ao lado do balcão, com um
 *  balão de fala que entrega a missão e aponta pro celular — é ela quem
 *  posta como @enf.rita.upa na rede social (HistoriaRedeSocial.cs).
 *
 *  Antes era o médico do pacote BackRock, mas o corpo do jogador usava o
 *  MESMO modelo (dois médicos idênticos na sala). Com o protagonista
 *  definido como agente comunitário de saúde, a recepção passou a ser da
 *  enfermeira (Female_WhiteCoat, pacote PolyPeople Hospital [Free]).
 *  Se o pacote sumir, cai de volta no médico.
 *
 *  POSE DA ENFERMEIRA: o prefab vem em T-pose (braços abertos, 1,43 m de
 *  largura) e sem AnimatorController. BaixarBracos() gira os ossos
 *  Humanoid dos braços (upper/lower arm) até apontarem pra baixo — largura
 *  cai pra ~0,50 m, pose relaxada — e só DEPOIS o Animator é removido
 *  (sem ele não dá pra achar os ossos por HumanBodyBones). Medido:
 *  ~1,55 m de altura, pés no pivô, shader já URP/Lit.
 *
 *  PACIENTES: só EXISTEM como voz (balão de fala 3D flutuando no lugar
 *  onde estariam), sem corpo visível. Motivo: os modelos low-poly
 *  disponíveis de graça (pacote PolyOne, usado numa sessão anterior)
 *  foram considerados feios demais pelo usuário, e o único pacote
 *  temático gratuito encontrado (PolyPeople Hospital [Free]) traz só UMA
 *  enfermeira de amostra, nenhuma pose de paciente — decisão explícita do
 *  usuário: sem pacote bom disponível, melhor sugerir a presença deles só
 *  pela fala ("Já faz três dias com febre...") do que usar um modelo feio.
 *  Ver Assets/PolyPeople_Hospital_Free (importado, não usado ainda — fica
 *  disponível caso o usuário queira uma 2ª pessoa da equipe no futuro).
 *
 *  Pacote de terceiros usado (gratuito, importado numa sessão anterior):
 *    - Assets/BackRock Studios/LowPoly-Doctor  (Backrock Studios)
 *
 *  AJUSTES FEITOS NESSE PACOTE (uma vez, no import, não aqui):
 *    - Lowpoly_Doctor.fbx vinha com Scale Factor bugado (altura real
 *      saía ~0,36 m em vez de ~1,75 m) — corrigido no ModelImporter
 *      (globalScale = 0.0482, useFileScale = false).
 *    - Os 3 materiais do médico (lambert2, lambert2Editable,
 *      nf2_aiStandardSurface1) vinham com shader Built-in (Standard /
 *      Autodesk Interactive) — apareciam magenta no URP. Convertidos
 *      pra Universal Render Pipeline/Lit preservando cor e textura
 *      principal (material.color / material.mainTexture, que mapeiam
 *      pra _BaseColor/_BaseMap via tag [MainColor]/[MainTexture] do
 *      shader, então sobrevivem à troca de shader).
 *
 *  POSE DO MÉDICO: testado sample() da animação "idle" (Animator com
 *  Avatar Humanoid) antes de decidir — o clipe tem curvas de root motion
 *  que descolam o pivô da posição dos pés de um jeito difícil de
 *  compensar de forma confiável. A pose de bind (sem Animator, sem
 *  nenhum clipe amostrado) já media 1,75 m de altura com os pés a
 *  -0,06 do pivô (offset compensado abaixo) e, visualmente (conferido
 *  por screenshot), é uma pose relaxada de braços entreabertos — nada
 *  de T-pose esticada. Por isso o Animator é só removido (não
 *  congelado numa pose amostrada): mais simples e com resultado melhor.
 * ============================================================ */

public static class PersonagensHospitalBuilder
{
    // internal (não private): PlayerBuilder.cs reaproveita o mesmo prefab e o
    // mesmo offset de pé pra dar ao jogador o corpo do médico.
    internal const string CaminhoMedico = "Assets/BackRock Studios/LowPoly-Doctor/Prefab/Lowpoly_Doctor.prefab";

    // Altura medida da pose de bind do médico (Renderer.bounds, escala já
    // corrigida no import): pés ficam 0,06 m ABAIXO do pivô — sem este
    // offset ele afundaria um pouco no piso.
    internal const float OffsetPeMedico = 0.06f;

    private const string CaminhoEnfermeira = "Assets/PolyPeople_Hospital_Free/Prefabs/Staff/Female_WhiteCoat.prefab";

    // Medido pelos vértices da malha (BakeMesh) com os braços já baixados:
    // os pés ficam EXATAMENTE no pivô. O Renderer.bounds do prefab sugeria
    // 0,05 m abaixo, mas é só a folga da caixa do SkinnedMeshRenderer —
    // com 0,05 aqui ela flutuava 5 cm (conferido com raycast no piso).
    private const float OffsetPeEnfermeira = 0f;

    // public: o KitDoAgenteBuilder.cs regrava esta fala no balão da Rita que
    // já está na cena (a sala não precisa ser reconstruída).
    public const string FalaEnfermeira =
        "Agente, o hospital não aguenta mais! Seu kit está pronto [1-5] e a picape da UPA tem mais material. O bairro vai te contar pelo celular [T] onde estão os focos. Vai lá!";

    public static void Popular(Transform pai, float metade)
    {
        GameObject enfermeiraPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CaminhoEnfermeira);
        GameObject medicoPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CaminhoMedico);
        if (enfermeiraPrefab == null && medicoPrefab == null)
        {
            Debug.LogWarning("[Sala do Hospital] Não encontrei nem a enfermeira (PolyPeople Hospital) nem o médico " +
                              "(BackRock Studios). Confirme se os pacotes ainda estão em Assets/.");
            return;
        }

        GameObject grupo = new GameObject("Personagens");
        Undo.RegisterCreatedObjectUndo(grupo, "Criar Sala do Hospital");
        grupo.transform.SetParent(pai, false);

        // Recepção: ao lado do balcão (não atrás — o balcão fica encostado na
        // parede, sem vão nenhum atrás dele; ver medição no histórico de
        // sessão), de frente pra sala/porta.
        GameObject recepcao;
        if (enfermeiraPrefab != null)
        {
            recepcao = QuartoHospitalBuilder.Instanciar(enfermeiraPrefab, grupo.transform,
                new Vector3(-0.55f, OffsetPeEnfermeira, -metade + 0.25f), 0f, Color.white);
            recepcao.name = "Enfermeira";
            BaixarBracos(recepcao);
            RemoverAnimator(recepcao);
            AdicionarColisor(recepcao, altura: 1.6f, raio: 0.26f);
            // 0,6 m pra frente: ela fica a 25 cm da parede Norte e o balão
            // (~1,3 m de largura) entrava na parede quando visto de lado.
            CriarBalao(recepcao.transform, "Enf. Rita", FalaEnfermeira, 1.95f, frente: 0.6f);
        }
        else
        {
            Debug.LogWarning("[Sala do Hospital] Enfermeira (PolyPeople Hospital) não encontrada — usando o médico na recepção.");
            recepcao = QuartoHospitalBuilder.Instanciar(medicoPrefab, grupo.transform,
                new Vector3(-0.55f, OffsetPeMedico, -metade + 0.25f), 0f, Color.white);
            recepcao.name = "Medico";
            RemoverAnimator(recepcao);
            AdicionarColisor(recepcao, altura: 1.75f, raio: 0.28f);
            CriarBalao(recepcao.transform, "Enf. Rita", FalaEnfermeira, 2.1f, frente: 0.6f);
        }

        // Pacientes: só a voz (ver cabeçalho do arquivo) — mesmas posições
        // que os corpos ocupariam, do lado Leste da sala, longe do corredor
        // até a porta (vão X entre -0.7 e 0.7 perto de Z=3.6) e do armário.
        // Alturas bem diferentes (1,2 / 1,85) pros dois balões não
        // encostarem um no outro vistos da porta (visto no preview).
        CriarVozPaciente(grupo.transform, "VozPacienteSentado",
            new Vector3(2.0f, 0f, 0.4f), "Já faz três dias com febre... isso é dengue, com certeza.", altura: 1.2f);
        CriarVozPaciente(grupo.transform, "VozPacienteEmPe",
            new Vector3(1.6f, 0f, 1.7f), "Que demora! Minha filha tá com dengue e ninguém atende direito.", altura: 1.85f);
    }

    /* O médico vem com Animator+Avatar Humanoid (controller com idle/walk/run),
     * mas o contexto do jogo.md pede visual estático, sem animação — sem o
     * Animator ele fica congelado na pose de bind (relaxada, já conferida por
     * screenshot). Sem Animator também não sobra nada tentando mexer nos ossos
     * depois de posicionado. internal: PlayerBuilder.cs reaproveita pro
     * corpo do jogador (a cabeça já foi tratada antes de chamar isto — ver
     * PlayerBuilder.Configurar()). */
    internal static void RemoverAnimator(GameObject instancia)
    {
        foreach (Animator anim in instancia.GetComponentsInChildren<Animator>(true))
            Object.DestroyImmediate(anim);
    }

    /* Paciente sem corpo: um GameObject vazio só pra ancorar o balão de fala
     * (ver BalaoDeFala.cs) na posição onde o paciente estaria. */
    private static void CriarVozPaciente(Transform pai, string nome, Vector3 posicaoLocal, string texto, float altura)
    {
        GameObject anchor = new GameObject(nome);
        Undo.RegisterCreatedObjectUndo(anchor, "Criar Sala do Hospital");
        anchor.transform.SetParent(pai, false);
        anchor.transform.localPosition = posicaoLocal;

        CriarBalao(anchor.transform, "Paciente", texto, altura, frente: 0f);
    }

    /* Balão de fala (BalaoDeFala.cs) preso num personagem ou âncora. O
     * visual é montado pelo próprio BalaoDeFala em runtime; aqui só vão os
     * dados e o sprite arredondado (UISprite embutido — em runtime não dá
     * pra carregá-lo, por isso a referência já vem serializada daqui). */
    private static void CriarBalao(Transform pai, string quem, string texto, float altura, float frente)
    {
        GameObject balaoGo = new GameObject("BalaoDeFala");
        Undo.RegisterCreatedObjectUndo(balaoGo, "Criar Sala do Hospital");
        balaoGo.transform.SetParent(pai, false);

        BalaoDeFala balao = Undo.AddComponent<BalaoDeFala>(balaoGo);
        balao.quemFala = quem;
        balao.texto = texto;
        balao.alturaAcimaDaCabeca = altura;
        balao.deslocamentoFrente = frente;
        balao.spriteFundo = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
    }

    /* Tira o personagem da T-pose: gira cada braço (osso Humanoid) até
     * apontar pra baixo, levemente aberto e pra frente. Calculado no espaço
     * LOCAL do personagem, então vale pra qualquer rotação dele na sala.
     * Precisa do Animator (Avatar Humanoid) ainda presente — chamar ANTES
     * de RemoverAnimator. As rotações ficam salvas nos ossos da cena. */
    private static void BaixarBracos(GameObject instancia)
    {
        Animator anim = instancia.GetComponentInChildren<Animator>();
        if (anim == null || !anim.isHuman)
        {
            Debug.LogWarning("[Sala do Hospital] " + instancia.name + " sem Avatar Humanoid — braços ficam na pose original.");
            return;
        }

        Transform raiz = instancia.transform;
        BaixarBraco(anim, raiz, HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftHand);
        BaixarBraco(anim, raiz, HumanBodyBones.RightUpperArm, HumanBodyBones.RightLowerArm, HumanBodyBones.RightHand);
    }

    private static void BaixarBraco(Animator anim, Transform raiz, HumanBodyBones ombro, HumanBodyBones cotovelo, HumanBodyBones mao)
    {
        Transform braco = anim.GetBoneTransform(ombro);
        Transform antebraco = anim.GetBoneTransform(cotovelo);
        Transform punho = anim.GetBoneTransform(mao);
        if (braco == null || antebraco == null || punho == null)
            return;

        Undo.RecordObject(braco, "Criar Sala do Hospital");
        Undo.RecordObject(antebraco, "Criar Sala do Hospital");

        // Braço: do ombro pra baixo, 0,22 pra fora (lado do próprio braço).
        Vector3 dir = raiz.InverseTransformDirection(antebraco.position - braco.position).normalized;
        Vector3 alvo = raiz.TransformDirection(new Vector3(Mathf.Sign(dir.x) * 0.22f, -1f, 0.04f).normalized);
        braco.rotation = Quaternion.FromToRotation(raiz.TransformDirection(dir), alvo) * braco.rotation;

        // Antebraço: quase reto, com a mão um pouco à frente do corpo.
        Vector3 dir2 = raiz.InverseTransformDirection(punho.position - antebraco.position).normalized;
        Vector3 alvo2 = raiz.TransformDirection(new Vector3(Mathf.Sign(dir.x) * 0.10f, -1f, 0.18f).normalized);
        antebraco.rotation = Quaternion.FromToRotation(raiz.TransformDirection(dir2), alvo2) * antebraco.rotation;
    }

    /* O médico não traz Collider — sem isso o CharacterController do player
     * atravessaria ele. CapsuleCollider simples, medido a partir dos bounds
     * reais do prefab (ver comentário de topo). */
    private static void AdicionarColisor(GameObject instancia, float altura, float raio)
    {
        CapsuleCollider colisor = instancia.AddComponent<CapsuleCollider>();
        colisor.height = altura;
        colisor.radius = raio;
        colisor.center = new Vector3(0f, altura * 0.5f, 0f);
    }
}
