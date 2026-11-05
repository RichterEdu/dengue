using UnityEngine;

/* ============================================================
 *  AmbienteSonoro.cs
 *  ---------------------------------------------------------------------
 *  Som de fundo do lugar onde o jogador está (no objeto "Sons"):
 *
 *  - Dentro do hospital: loop da recepção (lâmpada, ar-condicionado,
 *    murmúrio na sala ao lado, monitor longe).
 *  - Ao sair (FluxoDeJogo.OnPartidaIniciada): crossfade para o loop do
 *    bairro — cidade pequena e PARADA, tarde quente: vento, folhas,
 *    cigarras. Sem carros (nenhum passa no mapa).
 *  - No bairro, pássaros e latidos avulsos em pontos aleatórios em volta
 *    do jogador (3D), pra rua não soar como um loop.
 *
 *  O loop entra devagar no começo (a cena abre com as cartelas pretas).
 *  Tudo no canal Efeitos. Montado por Dengue > Configurar Sons.
 * ============================================================ */

public class AmbienteSonoro : MonoBehaviour
{
    [System.Serializable]
    public class SomAvulso
    {
        public AudioClip[] clips;
        [Tooltip("Segundos entre um e outro (mín, máx).")]
        public Vector2 intervalo = new Vector2(5f, 14f);
        [Tooltip("Distância do jogador (m).")]
        public Vector2 distancia = new Vector2(12f, 30f);
        [Tooltip("Altura em relação ao jogador (m).")]
        public Vector2 altura = new Vector2(4f, 8f);
        [Range(0f, 1f)] public float volume = 0.5f;
    }

    [Header("Loops")]
    [SerializeField] private AudioClip loopHospital;
    [Range(0f, 1f)]
    [SerializeField] private float volumeHospital = 0.5f;
    [SerializeField] private AudioClip loopBairro;
    [Range(0f, 1f)]
    [SerializeField] private float volumeBairro = 0.75f;
    [Tooltip("Segundos do fade inicial.")]
    [SerializeField] private float duracaoEntrada = 2.5f;
    [Tooltip("Segundos do crossfade hospital → bairro.")]
    [SerializeField] private float duracaoTroca = 1.2f;

    [Header("Avulsos no bairro (3D)")]
    [SerializeField] private SomAvulso passaros = new SomAvulso();
    [SerializeField] private SomAvulso latidos = new SomAvulso
    {
        intervalo = new Vector2(25f, 60f),
        distancia = new Vector2(25f, 45f),
        altura = new Vector2(0f, 1f),
        volume = 0.45f,
    };

    [Tooltip("Vazio = procura na cena.")]
    [SerializeField] private FluxoDeJogo fluxo;

    private CanalDeAudio hospital;
    private CanalDeAudio bairro;
    // Estado dos avulsos (o SomAvulso é só a configuração do Inspector).
    private CanalDeAudio fontePassaros;
    private CanalDeAudio fonteLatidos;
    private float proximoPassaro;
    private float proximoLatido;
    private int ultimoPassaro = -1;
    private int ultimoLatido = -1;
    private Transform ouvinte;
    private bool naRua;
    private bool entrou;

    void Start()
    {
        if (fluxo == null) fluxo = FindAnyObjectByType<FluxoDeJogo>();
        AudioListener listener = FindAnyObjectByType<AudioListener>();
        ouvinte = listener != null ? listener.transform : (Camera.main != null ? Camera.main.transform : null);

        hospital = CriarLoop("Ambiente Hospital", loopHospital);
        bairro = CriarLoop("Ambiente Bairro", loopBairro);
        fontePassaros = CriarFonteAvulso("Passaros");
        fonteLatidos = CriarFonteAvulso("Latidos");

        naRua = fluxo != null && fluxo.PartidaIniciada;
        if (fluxo != null)
            fluxo.OnPartidaIniciada += SaiuDoHospital;
    }

    void OnDestroy()
    {
        if (fluxo != null)
            fluxo.OnPartidaIniciada -= SaiuDoHospital;
    }

    private void SaiuDoHospital()
    {
        naRua = true;
        proximoPassaro = Time.time + Random.Range(1f, 4f);
        proximoLatido = Time.time + Random.Range(latidos.intervalo.x, latidos.intervalo.y);
    }

    void Update()
    {
        float duracao = entrou ? duracaoTroca : duracaoEntrada;
        bool hospitalPronto = Aproximar(hospital, naRua ? 0f : volumeHospital, volumeHospital / duracao);
        bool bairroPronto = Aproximar(bairro, naRua ? volumeBairro : 0f, volumeBairro / duracao);
        if (hospitalPronto && bairroPronto)
            entrou = true;

        if (naRua && ouvinte != null)
        {
            TentarAvulso(passaros, fontePassaros, ref proximoPassaro, ref ultimoPassaro);
            TentarAvulso(latidos, fonteLatidos, ref proximoLatido, ref ultimoLatido);
        }
    }

    /* Leva o volume do loop até o alvo; liga/desliga a fonte. Devolve true se chegou. */
    private static bool Aproximar(CanalDeAudio loop, float alvo, float velocidade)
    {
        if (loop == null)
            return true;
        loop.Volume = Mathf.MoveTowards(loop.Volume, alvo, velocidade * Time.deltaTime);
        AudioSource f = loop.Fonte;
        if (loop.Volume > 0f && !f.isPlaying)
            f.Play();
        else if (loop.Volume <= 0f && f.isPlaying)
            f.Stop();
        return Mathf.Approximately(loop.Volume, alvo);
    }

    private CanalDeAudio CriarLoop(string nome, AudioClip clip)
    {
        if (clip == null)
            return null;
        CanalDeAudio c = CanalDeAudio.Criar(transform, nome, CanalSom.Efeitos, false);
        c.Fonte.clip = clip;
        c.Fonte.loop = true;
        c.Fonte.priority = 20;
        c.Volume = 0f;
        return c;
    }

    private CanalDeAudio CriarFonteAvulso(string nome)
    {
        CanalDeAudio c = CanalDeAudio.Criar(transform, nome, CanalSom.Efeitos, true);
        c.Fonte.minDistance = 10f;   // longe, mas audível
        c.Fonte.maxDistance = 120f;
        c.Fonte.priority = 150;
        return c;
    }

    private void TentarAvulso(SomAvulso avulso, CanalDeAudio fonte, ref float proximo, ref int ultimo)
    {
        if (fonte == null || avulso.clips == null || avulso.clips.Length == 0 || Time.time < proximo)
            return;
        proximo = Time.time + Random.Range(avulso.intervalo.x, avulso.intervalo.y);

        int i = Random.Range(0, avulso.clips.Length);
        if (avulso.clips.Length > 1 && i == ultimo)
            i = (i + 1) % avulso.clips.Length;
        ultimo = i;

        Vector2 dir = Random.insideUnitCircle.normalized;
        float d = Random.Range(avulso.distancia.x, avulso.distancia.y);
        fonte.transform.position = ouvinte.position
                                   + new Vector3(dir.x, 0f, dir.y) * d
                                   + Vector3.up * Random.Range(avulso.altura.x, avulso.altura.y);
        fonte.Fonte.pitch = Random.Range(0.94f, 1.06f);
        fonte.Fonte.PlayOneShot(avulso.clips[i], avulso.volume);
    }
}
