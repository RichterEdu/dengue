using NUnit.Framework;
using UnityEditor;

/* ============================================================
 *  HistoriaKitTestes.cs
 *  ---------------------------------------------------------------------
 *  Testes EditMode (Parte A) dos posts do kit na rede social
 *  (HistoriaRedeSocial): o campo novo aoUsarFerramentaErrada.
 *
 *  A cena já tem personagens salvos de antes desse campo, que chegam
 *  com ele vazio. CompletarPersonagens() (chamado no Awake e pelo menu
 *  Dengue > Configurar Historia) preenche o vazio com o texto padrão do
 *  mesmo tipo, sem trocar o personagem que já estava lá.
 *
 *  O Awake não roda no EditMode: o teste monta a lista 'personagens'
 *  pelo SerializedObject (o TesteUtil.Setar não aceita listas) e chama
 *  CompletarPersonagens() direto.
 *
 *  Os objetos nascem pelo TesteUtil (escondidos, fora da cena) e são
 *  destruídos no [TearDown].
 *
 *  Rodar:  Window > General > Test Runner > EditMode (categoria ParteA)
 *  ou      unity command run_tests --mode EditMode --filter_type category --filter ParteA
 * ============================================================ */

[Category("ParteA")]
public class HistoriaKitTestes
{
    [TearDown]
    public void Limpar()
    {
        TesteUtil.LimparTudo();
    }

    [Test]
    public void CompletarPersonagens_PreencheFerramentaErradaVazia()
    {
        HistoriaRedeSocial historia = TesteUtil.Com<HistoriaRedeSocial>("Historia (teste)");

        // Um personagem só, do tipo Ralo, salvo "antes" do campo novo (vazio).
        SerializedObject so = new SerializedObject(historia);
        SerializedProperty lista = so.FindProperty("personagens");
        lista.arraySize = 1;
        SerializedProperty ralo = lista.GetArrayElementAtIndex(0);
        ralo.FindPropertyRelative("autor").stringValue = "Vizinho do Ralo";
        ralo.FindPropertyRelative("usuario").stringValue = "@vizinho.ralo";
        ralo.FindPropertyRelative("tipo").intValue = (int)TipoFoco.Ralo;
        SerializedProperty campo = ralo.FindPropertyRelative("aoUsarFerramentaErrada");
        Assert.IsNotNull(campo, "PersonagemDoBairro não tem o campo serializado aoUsarFerramentaErrada.");
        campo.stringValue = "";
        so.ApplyModifiedPropertiesWithoutUndo();

        historia.CompletarPersonagens();

        so.Update();
        SerializedProperty depois = so.FindProperty("personagens").GetArrayElementAtIndex(0);
        Assert.AreEqual("Vizinho do Ralo", depois.FindPropertyRelative("autor").stringValue,
            "O personagem que já estava na lista não pode ser trocado.");
        StringAssert.StartsWith("Vi o agente querendo resolver o ralo",
            depois.FindPropertyRelative("aoUsarFerramentaErrada").stringValue);
    }
}
