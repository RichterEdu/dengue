using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Object = UnityEngine.Object;

/* ============================================================
 *  FotosDosFocosBuilder.cs  (Editor)
 *  ---------------------------------------------------------------------
 *  As FOTOS DOS FOCOS que o narrador posta quando o jogador empaca
 *  (ver FotoDoFoco.cs e HistoriaRedeSocial.cs).
 *
 *  Menus em  Dengue > Fotos dos Focos:
 *    - Importar fotos aprovadas: lê o poses.tsv aprovado (nº, caminho do
 *      foco na hierarquia e pose), COPIA o PNG daquele número (sem regerar:
 *      a foto do jogo é, byte a byte, a que o usuário aprovou) para a pasta
 *      do jogo e põe ou atualiza o FotoDoFoco do foco com a pose e a foto.
 *      Linha sem foco na cena, ou foco da cena sem linha, sai num aviso.
 *    - Gerar fotos (todos os focos) / Gerar foto (focos selecionados):
 *      renderiza da pose do FotoDoFoco, desenha o círculo vermelho, grava o
 *      PNG, importa e liga em 'foto'. Câmera e círculo são os do preview
 *      aprovado, com a semente do círculo tirada do caminho do foco.
 *    - Usar a vista da Scene no foco selecionado: copia a câmera da última
 *      Scene view para o FotoDoFoco do foco (cria se faltar). Para corrigir
 *      uma foto: enquadrar na Scene, rodar este menu e o "Gerar foto".
 *    - Preparar celular para fotos: acrescenta ao card modelo a moldura da
 *      foto e o selo, e ao banner a miniatura, ligando as referências
 *      (CelularBuilder.GarantirFotoNoCard e HistoriaBuilder.GarantirMiniatura).
 *
 *  Arquivos: Assets/Meus/Assets Gerados/Fotos dos Focos/, um PNG 720×480
 *  por foco, com o nome tirado do caminho na hierarquia (sem o grupo do
 *  topo, "/" vira " - "). Importação: Default, sem mipmaps, Clamp, sRGB,
 *  máximo 1024, compressão normal, sem redimensionar para potência de 2.
 *
 *  Nada é salvo: só muda o FotoDoFoco (pelo SerializedObject, que entra no
 *  Undo) e a cena fica marcada suja. Câmera e texturas temporárias são
 *  HideAndDontSave e são destruídas no fim.
 * ============================================================ */

public static class FotosDosFocosBuilder
{
    internal const string PastaDasFotos = "Assets/Meus/Assets Gerados/Fotos dos Focos";
    internal const string PastaAprovadasPadrao = ".superpowers/sdd/2026-11-17-fotos-dos-focos/aprovadas"; // relativa à raiz do projeto

    private const string ArquivoDasPoses = "poses.tsv";
    private const string Menu = "Dengue/Fotos dos Focos/";
    private const string Log = "[Fotos dos Focos] ";

    /* ---- A foto (a mesma do preview aprovado) ---- */
    private const int Largura = 720;
    private const int Altura = 480;
    private const float Perto = 0.1f;
    private const float Longe = 400f;
    private static readonly Color32 Vermelho = new Color32(232, 30, 30, 255);

    internal struct Pose { public int numero; public string caminho; public Vector3 posicao; public Vector3 rotacao; public float fov; }

    private static string RaizDoProjeto => Path.GetDirectoryName(Application.dataPath);

    /* ================= Menus ================= */

    [MenuItem(Menu + "Importar fotos aprovadas")]
    private static void MenuImportar()
    {
        Interactable[] focos = Object.FindObjectsByType<Interactable>(FindObjectsInactive.Exclude);
        StringBuilder avisos = new StringBuilder();
        int ligados = Importar(Path.Combine(RaizDoProjeto, PastaAprovadasPadrao), PastaDasFotos, focos, avisos);

        Debug.Log(Log + ligados + " foco(s) ligado(s) às fotos aprovadas, copiadas para " + PastaDasFotos + ".");
        if (avisos.Length > 0)
            Debug.LogWarning(Log + "Avisos da importação:\n" + avisos);
    }

    [MenuItem(Menu + "Gerar fotos (todos os focos)")]
    private static void MenuGerarTodas()
    {
        GerarVarias(Object.FindObjectsByType<FotoDoFoco>(FindObjectsInactive.Include));
    }

    [MenuItem(Menu + "Gerar foto (focos selecionados)")]
    private static void MenuGerarSelecionados()
    {
        List<FotoDoFoco> fotos = new List<FotoDoFoco>();
        foreach (GameObject go in Selection.gameObjects)
        {
            FotoDoFoco f = go.GetComponent<FotoDoFoco>();
            if (f != null)
                fotos.Add(f);
        }

        if (fotos.Count == 0)
        {
            Debug.LogWarning(Log + "Selecione um ou mais focos com FotoDoFoco.");
            return;
        }
        GerarVarias(fotos);
    }

    [MenuItem(Menu + "Usar a vista da Scene no foco selecionado")]
    private static void MenuUsarVistaDaScene()
    {
        GameObject go = Selection.activeGameObject;
        if (go == null || go.GetComponent<Interactable>() == null)
        {
            Debug.LogWarning(Log + "Selecione um foco (um objeto com Interactable).");
            return;
        }

        SceneView vista = SceneView.lastActiveSceneView;
        if (vista == null || vista.camera == null)
        {
            Debug.LogWarning(Log + "Nenhuma Scene view aberta.");
            return;
        }

        Camera camera = vista.camera;
        FotoDoFoco f = GarantirFotoDoFoco(go);
        GravarPose(f, camera.transform.position, camera.transform.eulerAngles, camera.fieldOfView);
        Debug.Log(Log + "Pose da Scene view copiada para " + CaminhoNaHierarquia(go.transform)
            + ". Rode \"Gerar foto (focos selecionados)\" para refazer a foto.", go);
    }

    [MenuItem(Menu + "Preparar celular para fotos")]
    private static void MenuPrepararCelular()
    {
        FeedRedeSocial feed = Object.FindAnyObjectByType<FeedRedeSocial>(FindObjectsInactive.Include);
        NotificacaoCelular notificacao = Object.FindAnyObjectByType<NotificacaoCelular>(FindObjectsInactive.Include);
        Sprite ui = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        StringBuilder log = new StringBuilder();
        PrepararCelular(feed, notificacao, ui, log);
        Debug.Log(Log + "Preparar celular para fotos:" + log);
    }

    /* ================= Nomes, poses e semente ================= */

    /* "--- FOCOS ---/PilhadePneu/pneu (5)": os nomes da raiz até t, com "/". */
    internal static string CaminhoNaHierarquia(Transform t)
    {
        string caminho = t.name;
        for (Transform pai = t.parent; pai != null; pai = pai.parent)
            caminho = pai.name + "/" + caminho;
        return caminho;
    }

    /* Nome do PNG de um foco: sem o 1º segmento (o grupo do topo), cada
     * segmento sem caracteres inválidos de arquivo, com os espaços repetidos
     * virando um e sem espaço nas pontas, juntos com " - ". */
    internal static string NomeDoArquivo(string caminho)
    {
        string[] segmentos = caminho.Split('/');
        int primeiro = segmentos.Length > 1 ? 1 : 0;   // um foco solto na raiz usa o próprio nome
        char[] invalidos = Path.GetInvalidFileNameChars();

        List<string> partes = new List<string>();
        for (int i = primeiro; i < segmentos.Length; i++)
        {
            StringBuilder limpo = new StringBuilder();
            foreach (char c in segmentos[i])
                if (Array.IndexOf(invalidos, c) < 0)
                    limpo.Append(c);

            string parte = Regex.Replace(limpo.ToString(), " {2,}", " ").Trim();
            if (parte.Length > 0)
                partes.Add(parte);
        }

        return (partes.Count > 0 ? string.Join(" - ", partes) : "Foco") + ".png";
    }

    /* Lê o poses.tsv: nº, caminho, px, py, pz, rx, ry, rz, fov, separados por
     * TAB, em cultura invariante. Ignora linha vazia; o caminho fica como está
     * (os espaços de "Garrafa " e "Garrafa  (1)" fazem parte do nome). */
    internal static List<Pose> LerPoses(string textoTsv)
    {
        List<Pose> poses = new List<Pose>();
        string[] linhas = textoTsv.Split('\n');
        for (int i = 0; i < linhas.Length; i++)
        {
            string linha = linhas[i].TrimEnd('\r');
            if (string.IsNullOrWhiteSpace(linha))
                continue;

            string[] c = linha.Split('\t');
            if (c.Length < 9)
                throw new FormatException(string.Format("{0}, linha {1}: esperava 9 colunas separadas por TAB, achou {2}.",
                    ArquivoDasPoses, i + 1, c.Length));

            poses.Add(new Pose
            {
                numero = int.Parse(c[0], NumberStyles.Integer, CultureInfo.InvariantCulture),
                caminho = c[1],
                posicao = new Vector3(Numero(c[2]), Numero(c[3]), Numero(c[4])),
                rotacao = new Vector3(Numero(c[5]), Numero(c[6]), Numero(c[7])),
                fov = Numero(c[8]),
            });
        }
        return poses;
    }

    private static float Numero(string texto)
    {
        return float.Parse(texto, NumberStyles.Float, CultureInfo.InvariantCulture);
    }

    /* FNV-1a de 32 bits sobre os chars do caminho, como int não negativo:
     * a mesma semente para o mesmo foco em toda execução. */
    internal static int SementeDe(string caminho)
    {
        uint hash = 2166136261u;
        foreach (char c in caminho)
        {
            hash ^= c;
            hash = unchecked(hash * 16777619u);
        }
        return (int)(hash & 0x7FFFFFFFu);
    }

    /* ================= Círculo ================= */

    /* O círculo vermelho "à mão" em volta do retângulo do foco (em pixels da
     * foto, origem embaixo à esquerda): elipse com raios
     * max(meia largura × 1,3 + 14, 30) e max(meia altura × 1,3 + 14, 26),
     * traço de raio ≈ 2,6 px, 1,1 volta, oscilação 0,035 × sen(3t + fase) e
     * abertura de 7% ao longo do traço; fase = (semente × 1,7) mod 6,28. */
    internal static void DesenharCirculo(Color32[] px, int largura, int altura, Rect retangulo, int semente)
    {
        float cx = retangulo.center.x, cy = retangulo.center.y;
        float rx = Mathf.Max(retangulo.width * 0.5f * 1.3f + 14f, 30f);
        float ry = Mathf.Max(retangulo.height * 0.5f * 1.3f + 14f, 26f);
        float fase = (float)((semente * 1.7) % 6.28);

        const int passos = 900;
        for (int k = 0; k <= passos; k++)
        {
            float u = k / (float)passos;
            float t = fase + u * Mathf.PI * 2f * 1.1f;
            float escala = 1f + 0.035f * Mathf.Sin(3f * t + fase) + 0.07f * u;
            float x = cx + Mathf.Cos(t) * rx * escala;
            float y = cy + Mathf.Sin(t) * ry * escala;
            for (int dx = -3; dx <= 3; dx++)
                for (int dy = -3; dy <= 3; dy++)
                {
                    if (dx * dx + dy * dy > 7)
                        continue;
                    int ix = Mathf.RoundToInt(x) + dx, iy = Mathf.RoundToInt(y) + dy;
                    if (ix < 0 || iy < 0 || ix >= largura || iy >= altura)
                        continue;
                    px[iy * largura + ix] = Vermelho;
                }
        }
    }

    /* ================= Importar ================= */

    /* Liga cada linha do poses.tsv de 'pastaAprovadas' ao foco com o mesmo
     * caminho na hierarquia: copia os bytes do PNG "NN - ..." para
     * 'pastaDestino' (pasta de assets), importa e grava pose, FOV e foto no
     * FotoDoFoco. Problemas vão para 'avisos' e a importação segue.
     * Devolve quantos focos ligou. */
    internal static int Importar(string pastaAprovadas, string pastaDestino, IEnumerable<Interactable> focos, StringBuilder avisos)
    {
        string arquivoPoses = Path.Combine(pastaAprovadas, ArquivoDasPoses);
        if (!File.Exists(arquivoPoses))
        {
            avisos.AppendLine("Não achei o " + ArquivoDasPoses + " em " + pastaAprovadas + ".");
            return 0;
        }

        List<Pose> poses;
        try
        {
            poses = LerPoses(File.ReadAllText(arquivoPoses));
        }
        catch (FormatException e)
        {
            avisos.AppendLine(e.Message);
            return 0;
        }

        Dictionary<string, Interactable> porCaminho = new Dictionary<string, Interactable>(StringComparer.Ordinal);
        foreach (Interactable foco in focos)
        {
            if (foco == null)
                continue;
            string caminho = CaminhoNaHierarquia(foco.transform);
            if (porCaminho.ContainsKey(caminho))
                avisos.AppendLine("Dois focos com o caminho \"" + caminho + "\": só o primeiro é ligado.");
            else
                porCaminho.Add(caminho, foco);
        }

        string[] pngs = Directory.GetFiles(pastaAprovadas, "*.png");
        Array.Sort(pngs, StringComparer.Ordinal);
        MeusOrganizador.GarantirPasta(pastaDestino);

        HashSet<string> comLinha = new HashSet<string>(StringComparer.Ordinal);
        HashSet<string> usados = new HashSet<string>(StringComparer.OrdinalIgnoreCase);   // o disco do Windows não diferencia maiúsculas
        int ligados = 0;
        foreach (Pose pose in poses)
        {
            comLinha.Add(pose.caminho);
            if (!porCaminho.TryGetValue(pose.caminho, out Interactable foco))
            {
                avisos.AppendLine(string.Format("Linha {0:00}: nenhum foco na cena com o caminho \"{1}\".", pose.numero, pose.caminho));
                continue;
            }

            string png = AcharPng(pngs, pose.numero);
            if (png == null)
            {
                avisos.AppendLine(string.Format("Linha {0:00}: não achei o PNG \"{0:00} - ...\" em {1}.", pose.numero, pastaAprovadas));
                continue;
            }

            // Colisão de nomes só entre os focos desta importação: rodar de novo dá os mesmos arquivos.
            string destino = CaminhoLivre(pastaDestino, NomeDoArquivo(pose.caminho), usados.Contains);
            usados.Add(destino);
            File.Copy(png, NoDisco(destino), true);

            FotoDoFoco f = GarantirFotoDoFoco(foco.gameObject);
            GravarPose(f, pose.posicao, pose.rotacao, pose.fov);
            GravarFoto(f, ImportarFoto(destino));
            ligados++;
        }

        foreach (KeyValuePair<string, Interactable> par in porCaminho)
            if (!comLinha.Contains(par.Key))
                avisos.AppendLine("Foco sem linha no " + ArquivoDasPoses + ": \"" + par.Key + "\".");

        return ligados;
    }

    /* O PNG aprovado do número: o arquivo que começa com "NN - ". */
    private static string AcharPng(string[] pngs, int numero)
    {
        string prefixo = numero.ToString("00", CultureInfo.InvariantCulture) + " - ";
        foreach (string png in pngs)
            if (Path.GetFileName(png).StartsWith(prefixo, StringComparison.Ordinal))
                return png;
        return null;
    }

    /* ================= Gerar ================= */

    /* A foto do foco, renderizada da pose do FotoDoFoco: 720×480, com o
     * círculo vermelho em volta dos Renderers do foco. Quem chama destrói. */
    internal static Texture2D Renderizar(FotoDoFoco f)
    {
        // Câmera temporária copiada da Main Camera, com o pós-processamento,
        // o antialiasing e as sombras dela (como no preview aprovado).
        GameObject go = new GameObject("__FotoCam") { hideFlags = HideFlags.HideAndDontSave };
        RenderTexture rt = new RenderTexture(Largura, Altura, 24, RenderTextureFormat.ARGB32)
        {
            antiAliasing = 4,
            hideFlags = HideFlags.HideAndDontSave,
        };
        Texture2D tex = new Texture2D(Largura, Altura, TextureFormat.RGB24, false) { hideFlags = HideFlags.HideAndDontSave };
        try
        {
            Camera camera = go.AddComponent<Camera>();
            Camera principal = Camera.main;
            if (principal != null)
                camera.CopyFrom(principal);
            camera.enabled = false;
            camera.fieldOfView = f.campoDeVisao;
            camera.nearClipPlane = Perto;
            camera.farClipPlane = Longe;

            UniversalAdditionalCameraData dadosPrincipal = principal != null
                ? principal.GetComponent<UniversalAdditionalCameraData>()
                : null;
            UniversalAdditionalCameraData dados = go.GetComponent<UniversalAdditionalCameraData>();
            if (dados == null)
                dados = go.AddComponent<UniversalAdditionalCameraData>();
            if (dadosPrincipal != null)
            {
                dados.renderPostProcessing = dadosPrincipal.renderPostProcessing;
                dados.antialiasing = dadosPrincipal.antialiasing;
                dados.renderShadows = true;
            }

            camera.targetTexture = rt;
            camera.aspect = (float)Largura / Altura;
            go.transform.SetPositionAndRotation(f.posicaoDaCamera, Quaternion.Euler(f.rotacaoDaCamera));
            camera.Render();

            RenderTexture anterior = RenderTexture.active;
            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, Largura, Altura), 0, 0);
            tex.Apply();
            RenderTexture.active = anterior;

            Color32[] px = tex.GetPixels32();
            DesenharCirculo(px, Largura, Altura, RetanguloNaFoto(camera, f.gameObject),
                SementeDe(CaminhoNaHierarquia(f.transform)));
            tex.SetPixels32(px);
            tex.Apply();

            camera.targetTexture = null;
            return tex;
        }
        catch
        {
            Object.DestroyImmediate(tex);
            throw;
        }
        finally
        {
            Object.DestroyImmediate(rt);
            Object.DestroyImmediate(go);
        }
    }

    /* Retângulo, em pixels da foto, dos 8 cantos do Renderer.bounds do foco
     * (sem ParticleSystemRenderer); sem Renderer, o ponto do próprio foco. */
    private static Rect RetanguloNaFoto(Camera camera, GameObject foco)
    {
        Bounds b = new Bounds(foco.transform.position, Vector3.zero);
        bool tem = false;
        foreach (Renderer r in foco.GetComponentsInChildren<Renderer>())
        {
            if (r is ParticleSystemRenderer)
                continue;
            if (!tem) { b = r.bounds; tem = true; }
            else b.Encapsulate(r.bounds);
        }

        float xmin = float.MaxValue, xmax = float.MinValue, ymin = float.MaxValue, ymax = float.MinValue;
        for (int i = 0; i < 8; i++)
        {
            Vector3 canto = new Vector3((i & 1) == 0 ? b.min.x : b.max.x,
                (i & 2) == 0 ? b.min.y : b.max.y,
                (i & 4) == 0 ? b.min.z : b.max.z);
            Vector3 sp = camera.WorldToScreenPoint(canto);
            xmin = Mathf.Min(xmin, sp.x); xmax = Mathf.Max(xmax, sp.x);
            ymin = Mathf.Min(ymin, sp.y); ymax = Mathf.Max(ymax, sp.y);
        }
        return Rect.MinMaxRect(xmin, ymin, xmax, ymax);
    }

    /* Renderiza, grava o PNG em 'pastaDestino', importa e liga em f.foto.
     * Se a foto atual já está nessa pasta e é só deste foco (PodeRegravar),
     * regrava o mesmo arquivo; senão usa o NomeDoArquivo (com sufixo se já
     * houver outro arquivo com ele). Devolve o caminho do asset. */
    internal static string Gerar(FotoDoFoco f, string pastaDestino)
    {
        Texture2D tex = Renderizar(f);
        byte[] png;
        try
        {
            png = tex.EncodeToPNG();
        }
        finally
        {
            Object.DestroyImmediate(tex);
        }

        MeusOrganizador.GarantirPasta(pastaDestino);
        string nome = NomeDoArquivo(CaminhoNaHierarquia(f.transform));
        string atual = f.foto != null ? AssetDatabase.GetAssetPath(f.foto) : null;
        string destino = !string.IsNullOrEmpty(atual) && atual.StartsWith(pastaDestino + "/", StringComparison.Ordinal)
                && atual.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                && PodeRegravar(f, atual, nome)
            ? atual
            : CaminhoLivre(pastaDestino, nome, c => File.Exists(NoDisco(c)));

        File.WriteAllBytes(NoDisco(destino), png);
        GravarFoto(f, ImportarFoto(destino));
        return destino;
    }

    /* O Gerar pode regravar o arquivo 'atual' da foto de 'f'? Um Ctrl+D (ou
     * Paste Component Values) copia a referência 'foto', e aí dois focos
     * apontam para o mesmo PNG: regravar trocaria, calado, a foto do outro.
     * Por isso: sim se nenhum outro FotoDoFoco carregado (inclusive os
     * escondidos e os inativos; prefabs em disco não contam) usa a mesma
     * textura; se outros usam, só o dono do nome — o único entre eles cujo
     * NomeDoArquivo é o nome do arquivo — regrava; os outros ganham arquivo
     * próprio. */
    private static bool PodeRegravar(FotoDoFoco f, string atual, string nome)
    {
        string arquivo = Path.GetFileName(atual);
        bool dividida = false;
        bool outroDono = false;
        foreach (FotoDoFoco outro in Resources.FindObjectsOfTypeAll<FotoDoFoco>())
        {
            if (outro == f || outro.foto != f.foto || EditorUtility.IsPersistent(outro))
                continue;
            dividida = true;
            if (string.Equals(NomeDoArquivo(CaminhoNaHierarquia(outro.transform)), arquivo, StringComparison.OrdinalIgnoreCase))
                outroDono = true;
        }
        return !dividida || (!outroDono && string.Equals(nome, arquivo, StringComparison.OrdinalIgnoreCase));
    }

    private static void GerarVarias(IEnumerable<FotoDoFoco> fotos)
    {
        int geradas = 0;
        foreach (FotoDoFoco f in fotos)
        {
            string caminho = Gerar(f, PastaDasFotos);
            Debug.Log(Log + CaminhoNaHierarquia(f.transform) + " → " + caminho, f);
            geradas++;
        }
        Debug.Log(Log + geradas + " foto(s) gerada(s) em " + PastaDasFotos + ".");
    }

    /* ================= Preparar celular ================= */

    /* Acrescenta ao card modelo do feed (FeedRedeSocial.modelo) a moldura da
     * foto e o selo, e ao banner a miniatura, ligando as referências; marca a
     * cena suja e escreve em 'log' o que criou. Idempotente. */
    internal static void PrepararCelular(FeedRedeSocial feed, NotificacaoCelular notificacao, Sprite ui, StringBuilder log)
    {
        PostView modelo = feed != null
            ? new SerializedObject(feed).FindProperty("modelo").objectReferenceValue as PostView
            : null;
        if (feed == null)
            log.Append("\n  • Sem FeedRedeSocial na cena (rode Dengue > Criar Celular).");
        else if (modelo == null)
            log.Append("\n  • O FeedRedeSocial não tem o card modelo (ModeloPost).");
        else
        {
            log.Append(CelularBuilder.GarantirFotoNoCard(modelo, ui)
                ? "\n  • Criado: ModeloPost/Foto (moldura da foto e selo RESOLVIDO)"
                : "\n  • ModeloPost já tinha a Foto (referências conferidas)");
            MarcarSuja(modelo.gameObject);
        }

        if (notificacao == null)
            log.Append("\n  • Sem NotificacaoCelular na cena (rode Dengue > Configurar Historia).");
        else
        {
            log.Append(HistoriaBuilder.GarantirMiniatura(notificacao, ui)
                ? "\n  • Criado: Notificacao/Miniatura (miniatura da foto)"
                : "\n  • Notificacao já tinha a Miniatura (referências conferidas)");
            MarcarSuja(notificacao.gameObject);
        }
    }

    /* ================= Helpers ================= */

    private static FotoDoFoco GarantirFotoDoFoco(GameObject foco)
    {
        FotoDoFoco f = foco.GetComponent<FotoDoFoco>();
        return f != null ? f : Undo.AddComponent<FotoDoFoco>(foco);
    }

    private static void GravarPose(FotoDoFoco f, Vector3 posicao, Vector3 rotacao, float fov)
    {
        SerializedObject so = new SerializedObject(f);
        so.FindProperty("posicaoDaCamera").vector3Value = posicao;
        so.FindProperty("rotacaoDaCamera").vector3Value = rotacao;
        so.FindProperty("campoDeVisao").floatValue = fov;
        so.ApplyModifiedProperties();
        MarcarSuja(f.gameObject);
    }

    private static void GravarFoto(FotoDoFoco f, Texture2D foto)
    {
        SerializedObject so = new SerializedObject(f);
        so.FindProperty("foto").objectReferenceValue = foto;
        so.ApplyModifiedProperties();
        MarcarSuja(f.gameObject);
    }

    /* Marca suja a cena do objeto (os objetos escondidos dos testes ficam de fora). */
    private static void MarcarSuja(GameObject go)
    {
        if ((go.hideFlags & HideFlags.DontSaveInEditor) == 0 && go.scene.IsValid())
            EditorSceneManager.MarkSceneDirty(go.scene);
    }

    /* Importa o PNG com as configurações das fotos e devolve a textura. */
    private static Texture2D ImportarFoto(string caminhoAsset)
    {
        AssetDatabase.ImportAsset(caminhoAsset, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
        TextureImporter ti = AssetImporter.GetAtPath(caminhoAsset) as TextureImporter;
        if (ti != null && (ti.textureType != TextureImporterType.Default || ti.mipmapEnabled
            || ti.wrapMode != TextureWrapMode.Clamp || !ti.sRGBTexture || ti.maxTextureSize != 1024
            || ti.textureCompression != TextureImporterCompression.Compressed
            || ti.npotScale != TextureImporterNPOTScale.None))
        {
            ti.textureType = TextureImporterType.Default;
            ti.mipmapEnabled = false;
            ti.wrapMode = TextureWrapMode.Clamp;
            ti.sRGBTexture = true;
            ti.maxTextureSize = 1024;
            ti.textureCompression = TextureImporterCompression.Compressed;
            ti.npotScale = TextureImporterNPOTScale.None;   // 720×480 fica 720×480
            ti.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Texture2D>(caminhoAsset);
    }

    /* "pasta/nome.png", ou "pasta/nome (2).png", "(3)"... enquanto 'ocupado'. */
    private static string CaminhoLivre(string pasta, string nome, Func<string, bool> ocupado)
    {
        string baseDoNome = Path.GetFileNameWithoutExtension(nome);
        string extensao = Path.GetExtension(nome);
        string caminho = pasta + "/" + nome;
        for (int n = 2; ocupado(caminho); n++)
            caminho = pasta + "/" + baseDoNome + " (" + n + ")" + extensao;
        return caminho;
    }

    /* Caminho no disco de um caminho de asset ("Assets/..."). */
    private static string NoDisco(string caminhoAsset)
    {
        return Path.Combine(RaizDoProjeto, caminhoAsset);
    }
}
