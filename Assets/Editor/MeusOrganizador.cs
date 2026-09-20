using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Junta numa pasta so tudo que foi gerado pelos builders (vaso, caixa d'agua,
/// escada e terreno baldio), deixando em Assets/Meus apenas o que e seu
/// (cerca.fbx, rubber tries.fbx, agua do pneu.mat).
///
/// Menu: Dengue > Organizar Assets Gerados
///
/// Resultado:
///   Assets/Meus/Assets Gerados/Terreno Baldio/{Malhas, Materiais, Prefabs}
///   Assets/Meus/Assets Gerados/Vaso de Flor/
///   Assets/Meus/Assets Gerados/Caixa Dagua/
///   Assets/Meus/Assets Gerados/Escada de Mao/
///
/// Usa AssetDatabase.MoveAsset, que move o arquivo junto com o .meta: o GUID nao
/// muda, entao nada quebra -- prefabs ja colocados na cena continuam apontando
/// para os mesmos assets. Pode rodar de novo: o que ja esta no lugar e ignorado.
/// </summary>
public static class MeusOrganizador
{
    public const string Meus = "Assets/Meus";
    public const string Destino = "Assets/Meus/Assets Gerados";

    [MenuItem("Dengue/Organizar Assets Gerados")]
    public static void Organizar()
    {
        GarantirPasta(Destino);

        List<string> feito = new List<string>();
        List<string> pulado = new List<string>();

        MoverPasta("Terreno Baldio", feito, pulado);
        MoverPasta("Vaso de Flor", feito, pulado);
        MoverPasta("Caixa Dagua", feito, pulado);
        MoverPasta("Escada de Mao", feito, pulado);

        MoverSolto("Vaso de Flor.prefab", "Vaso de Flor", feito, pulado);
        MoverSolto("Caixa d'Agua.prefab", "Caixa Dagua", feito, pulado);
        MoverSolto("Escada de Mao.prefab", "Escada de Mao", feito, pulado);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine(feito.Count + " itens movidos para " + Destino + ":");
        for (int i = 0; i < feito.Count; i++) sb.AppendLine("  " + feito[i]);
        if (pulado.Count > 0)
        {
            sb.AppendLine("Pulados:");
            for (int i = 0; i < pulado.Count; i++) sb.AppendLine("  " + pulado[i]);
        }
        sb.Append("Ficaram em Assets/Meus (sao seus, nao mexi): cerca.fbx, rubber tries.fbx, agua do pneu.mat.");

        Debug.Log("[Organizar] " + sb.ToString());

        Object pasta = AssetDatabase.LoadAssetAtPath<Object>(Destino);
        if (pasta != null)
        {
            Selection.activeObject = pasta;
            EditorGUIUtility.PingObject(pasta);
        }

        EditorUtility.DisplayDialog("Assets Gerados",
            feito.Count + " itens movidos para \"Assets/Meus/Assets Gerados\".\n\n" +
            "Como foi pelo AssetDatabase, os GUIDs nao mudaram: nada quebrou na cena.\n\n" +
            "Detalhes no Console.", "Ok");
    }

    // ------------------------------------------------------------------ mover
    static void MoverPasta(string nome, List<string> feito, List<string> pulado)
    {
        string origem = Meus + "/" + nome;
        string destino = Destino + "/" + nome;

        if (!AssetDatabase.IsValidFolder(origem))
        {
            if (!AssetDatabase.IsValidFolder(destino)) pulado.Add(nome + " (nao existe)");
            return;
        }
        if (AssetDatabase.IsValidFolder(destino))
        {
            pulado.Add(nome + " (ja existe uma pasta com esse nome no destino)");
            return;
        }

        string erro = AssetDatabase.ValidateMoveAsset(origem, destino);
        if (!string.IsNullOrEmpty(erro))
        {
            pulado.Add(nome + " (" + erro + ")");
            return;
        }

        erro = AssetDatabase.MoveAsset(origem, destino);
        if (string.IsNullOrEmpty(erro)) feito.Add(nome + "/");
        else pulado.Add(nome + " (" + erro + ")");
    }

    static void MoverSolto(string arquivo, string subpasta, List<string> feito, List<string> pulado)
    {
        string origem = Meus + "/" + arquivo;
        if (AssetDatabase.LoadAssetAtPath<Object>(origem) == null) return;   // nao existe: nada a fazer

        string pastaDestino = Destino + "/" + subpasta;
        GarantirPasta(pastaDestino);

        string destino = pastaDestino + "/" + arquivo;
        if (AssetDatabase.LoadAssetAtPath<Object>(destino) != null)
            destino = AssetDatabase.GenerateUniqueAssetPath(destino);   // ja tem um com esse nome

        string erro = AssetDatabase.ValidateMoveAsset(origem, destino);
        if (!string.IsNullOrEmpty(erro))
        {
            pulado.Add(arquivo + " (" + erro + ")");
            return;
        }

        erro = AssetDatabase.MoveAsset(origem, destino);
        if (string.IsNullOrEmpty(erro)) feito.Add(arquivo + " -> " + subpasta + "/");
        else pulado.Add(arquivo + " (" + erro + ")");
    }

    /// <summary>Cria a pasta e as pastas acima dela, se faltarem.</summary>
    public static void GarantirPasta(string caminho)
    {
        if (string.IsNullOrEmpty(caminho) || AssetDatabase.IsValidFolder(caminho)) return;

        int corte = caminho.LastIndexOf('/');
        if (corte <= 0) return;

        string pai = caminho.Substring(0, corte);
        string nome = caminho.Substring(corte + 1);
        GarantirPasta(pai);
        if (!AssetDatabase.IsValidFolder(caminho)) AssetDatabase.CreateFolder(pai, nome);
    }
}
