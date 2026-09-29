using System.IO;
using UnityEditor;
using UnityEngine;

/* ============================================================
 *  ImportadorDeSons.cs
 *  ---------------------------------------------------------------------
 *  Configura a importação dos sons gerados por
 *  Ferramentas/Audio/gerar_sons.py (só age na pasta deles), pelo nome:
 *
 *    *_loop e trilha_*   Compressed In Memory, Vorbis — loops longos
 *                        (exceto o zumbido, que é curto e fica em PCM)
 *    o resto             Decompress On Load, PCM — curtos, sem atraso
 *    mosquito_ foco_ passaro_ latido_ passo_   mono (são tocados em 3D)
 *
 *  Roda a cada (re)importação: mudança feita à mão no Inspector desses
 *  arquivos volta ao padrão quando o arquivo for regerado.
 * ============================================================ */

public class ImportadorDeSons : AssetPostprocessor
{
    public const string Pasta = "Assets/Meus/Assets Gerados/Sons/";

    private static readonly string[] Prefixos3D = { "mosquito_", "foco_", "passaro_", "latido_", "passo_" };

    void OnPreprocessAudio()
    {
        if (!assetPath.StartsWith(Pasta))
            return;

        AudioImporter importador = (AudioImporter)assetImporter;
        string nome = Path.GetFileNameWithoutExtension(assetPath);

        bool som3D = false;
        foreach (string prefixo in Prefixos3D)
            som3D |= nome.StartsWith(prefixo);
        bool longo = (nome.EndsWith("_loop") || nome.StartsWith("trilha_")) && !nome.StartsWith("mosquito_");

        importador.forceToMono = som3D;
        importador.loadInBackground = false;

        AudioImporterSampleSettings s = importador.defaultSampleSettings;
        s.preloadAudioData = true;   // as camadas da trilha precisam estar prontas pra começar juntas
        s.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
        if (longo)
        {
            s.loadType = AudioClipLoadType.CompressedInMemory;
            s.compressionFormat = AudioCompressionFormat.Vorbis;
            s.quality = 0.6f;
        }
        else
        {
            s.loadType = AudioClipLoadType.DecompressOnLoad;
            s.compressionFormat = AudioCompressionFormat.PCM;
        }
        importador.defaultSampleSettings = s;
    }
}
