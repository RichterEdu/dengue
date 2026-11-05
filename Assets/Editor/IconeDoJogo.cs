using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

// Aplica o ícone do jogo (Project Settings > Player > Default Icon) sozinho
// quando o Editor carrega. Também dá para rodar pelo menu Dengue > Aplicar Icone do Jogo.
[InitializeOnLoad]
public static class IconeDoJogo
{
    public const string CaminhoIcone = "Assets/Meus/Assets Gerados/Icone/IconeFocoZero.png";

    static IconeDoJogo()
    {
        EditorApplication.delayCall += () => Aplicar(false);
    }

    [MenuItem("Dengue/Aplicar Icone do Jogo")]
    private static void AplicarPeloMenu() => Aplicar(true);

    private static void Aplicar(bool avisar)
    {
        var importer = AssetImporter.GetAtPath(CaminhoIcone) as TextureImporter;
        if (importer == null)
        {
            if (avisar) Debug.LogWarning($"[IconeDoJogo] Não achei {CaminhoIcone}.");
            return;
        }

        // Ícone sem compressão e sem mipmaps, para não borrar nos tamanhos pequenos.
        if (importer.textureCompression != TextureImporterCompression.Uncompressed ||
            importer.mipmapEnabled || !importer.alphaIsTransparency ||
            importer.npotScale != TextureImporterNPOTScale.None)
        {
            importer.textureType = TextureImporterType.Default;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.maxTextureSize = 1024;
            importer.SaveAndReimport();
        }

        var icone = AssetDatabase.LoadAssetAtPath<Texture2D>(CaminhoIcone);
        if (icone == null) return;

        var atuais = PlayerSettings.GetIcons(NamedBuildTarget.Unknown, IconKind.Any);
        if (atuais != null && atuais.Length > 0 && atuais[0] == icone)
        {
            if (avisar) Debug.Log("[IconeDoJogo] O ícone já estava aplicado.");
            return;
        }

        var tamanhos = PlayerSettings.GetIconSizes(NamedBuildTarget.Unknown, IconKind.Any);
        var icones = Enumerable.Repeat(icone, Mathf.Max(1, tamanhos.Length)).ToArray();
        PlayerSettings.SetIcons(NamedBuildTarget.Unknown, icones, IconKind.Any);
        AssetDatabase.SaveAssets();
        Debug.Log("[IconeDoJogo] Ícone do jogo aplicado (Player > Default Icon).");
    }
}
