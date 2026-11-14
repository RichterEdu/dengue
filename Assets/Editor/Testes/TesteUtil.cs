using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

/* ============================================================
 *  TesteUtil.cs
 *  ---------------------------------------------------------------------
 *  Ajudantes dos testes EditMode (Parte A).
 *
 *  Nos testes EditMode o Awake/Start não rodam e a cena aberta no Editor
 *  não pode ficar suja. Por isso:
 *    - Novo / Com<T>  criam o GameObject com HideFlags.HideAndDontSave
 *                     (não aparece na Hierarchy nem é salvo na cena) e
 *                     guardam numa lista;
 *    - Primitivo      o mesmo para uma primitiva (cubo...) com malha e
 *                     material padrão;
 *    - LimparTudo     destrói (DestroyImmediate) tudo o que foi criado —
 *                     chamar no [TearDown] de cada classe de teste;
 *    - Setar          ajusta um campo serializado (inclusive private com
 *                     [SerializeField]) pelo SerializedObject, do mesmo
 *                     jeito que o Inspector faz, sem entrar no Undo.
 *
 *  Setar aceita: int, float, bool, enum (salvo como número, igual à cena),
 *  string e referência a Object (inclusive null).
 * ============================================================ */

public static class TesteUtil
{
    private static readonly List<GameObject> criados = new List<GameObject>();

    /* GameObject escondido e fora da cena, destruído no LimparTudo. */
    public static GameObject Novo(string nome)
    {
        GameObject go = EditorUtility.CreateGameObjectWithHideFlags(nome, HideFlags.HideAndDontSave);
        criados.Add(go);
        return go;
    }

    /* Atalho: GameObject novo já com o componente T. */
    public static T Com<T>(string nome) where T : Component
    {
        return Novo(nome).AddComponent<T>();
    }

    /* Primitiva do Unity (cubo, esfera...) com malha e material padrão,
     * escondida como as outras e destruída no LimparTudo. */
    public static GameObject Primitivo(PrimitiveType tipo, string nome)
    {
        GameObject go = GameObject.CreatePrimitive(tipo);
        go.name = nome;
        go.hideFlags = HideFlags.HideAndDontSave;
        criados.Add(go);
        return go;
    }

    /* Ajusta o campo serializado 'campo' de 'alvo' (nome do campo no código, como no YAML). */
    public static void Setar(Object alvo, string campo, object valor)
    {
        if (alvo == null)
            throw new ArgumentNullException(nameof(alvo), $"Setar(\"{campo}\"): alvo nulo.");

        SerializedObject so = new SerializedObject(alvo);
        SerializedProperty prop = so.FindProperty(campo);
        if (prop == null)
            throw new ArgumentException($"Setar: o campo serializado \"{campo}\" não existe em {alvo.GetType().Name}.", nameof(campo));

        switch (prop.propertyType)
        {
            case SerializedPropertyType.Integer:
                prop.intValue = Convert.ToInt32(valor);   // aceita int e enum
                break;
            case SerializedPropertyType.Enum:
                // intValue grava o NÚMERO do enum (o que vai pra cena); enumValueIndex
                // seria a posição na lista de nomes, que só coincide se o enum começa
                // em 0 sem buracos.
                prop.intValue = Convert.ToInt32(valor);
                break;
            case SerializedPropertyType.Float:
                prop.floatValue = Convert.ToSingle(valor);
                break;
            case SerializedPropertyType.Boolean:
                prop.boolValue = Convert.ToBoolean(valor);
                break;
            case SerializedPropertyType.String:
                prop.stringValue = (string)valor;
                break;
            case SerializedPropertyType.ObjectReference:
                if (valor != null && !(valor is Object))
                    throw new ArgumentException($"Setar(\"{campo}\"): esperava um UnityEngine.Object, veio {valor.GetType().Name}.", nameof(valor));
                prop.objectReferenceValue = (Object)valor;
                break;
            default:
                throw new ArgumentException($"Setar(\"{campo}\"): tipo {prop.propertyType} não suportado pelo TesteUtil.", nameof(campo));
        }

        so.ApplyModifiedPropertiesWithoutUndo();
    }

    /* O pneu pego cheio pelas Mãos e esvaziado nos braços (todos os passos de
     * DarPassoNosBracos). Com 'largar', ele volta ao chão onde estava, esvaziado
     * e fora dos braços (sem chão embaixo, o Largar o põe no mesmo lugar). */
    public static void EsvaziarPneu(PneuInteractable pneu, KitDoAgente kit, bool largar = true)
    {
        Vector3 pes = pneu.transform.position;
        if (kit.PneuCarregado != pneu)
            Assert.AreEqual(ResultadoDoPasso.Pegou, pneu.AvancarCom(kit), "as Mãos pegam o pneu cheio");

        for (int i = 0; i < 100 && !pneu.Esvaziado; i++)
            pneu.DarPassoNosBracos();
        Assert.IsTrue(pneu.Esvaziado, "o pneu esvaziou nos braços");

        if (largar)
            Assert.IsTrue(kit.LargarPneu(pes, Vector3.forward), "o pneu esvaziado volta ao chão");
    }

    /* Destrói tudo o que Novo/Com criaram (os já destruídos são ignorados). */
    public static void LimparTudo()
    {
        foreach (GameObject go in criados)
        {
            if (go != null)
                Object.DestroyImmediate(go);
        }
        criados.Clear();
    }
}
