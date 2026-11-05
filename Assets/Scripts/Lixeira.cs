using System.Collections;
using UnityEngine;

/* ============================================================
 *  Lixeira.cs
 *  ---------------------------------------------------------------------
 *  Lixeira de rua (Parte B): ponto de uso onde o agente esvazia o saco
 *  de lixo com as garrafas recolhidas (KitDoAgente.EsvaziarSaco). As
 *  caçambas de entulho saíram do bairro; o pneu vai para a caçamba da
 *  picape da UPA (Cacamba.cs).
 *
 *  Na mira, o texto é sempre "Esvaziar o saco na lixeira" (TextoPara), no
 *  lugar do textoAoMirar ("Lixeira", do builder da cena): diz o que fazer
 *  ali sem repetir o "segure", porque a HUD já põe a linha "segure o
 *  clique" embaixo.
 *
 *  Recusa, nesta ordem: sem kit (sem aviso), com o pneu nas mãos ("Pneu
 *  vai na caçamba da picape da UPA.") e com o saco vazio ("O saco está
 *  vazio."). Ao usar, a tampa abre anguloAberta graus no X, o som da
 *  tampa toca e ela fecha de novo (só em Play: no EditMode as
 *  corrotinas não andam). O som de vidro do saco é do SomDoKit
 *  (OnSacoEsvaziado). Desligada no meio da animação, a tampa fecha na
 *  hora.
 *
 *  Sem tampa separada (as lixeiras do bairro são malha única), a lixeira
 *  inteira balança: uma oscilação de AnguloDoBalanco graus para cada lado
 *  em SegundosDoBalanco, em torno do eixo Z do mundo (o da malha importada
 *  pode ser o vertical), que volta ao giro original. Desligada no meio,
 *  ela endireita na hora. A raiz não pode estar em Batching Static, ou
 *  a malha não se mexe (as 5 da cena não estão).
 *
 *  Os nomes 'tampa', 'anguloAberta' e 'somTampa' são gravados por
 *  SerializedObject pelo builder da cena: não renomear.
 * ============================================================ */

public class Lixeira : PontoDeUso
{
    // Tempos da tampa: abre, fica aberta e fecha.
    private const float SegundosAbrindo = 0.15f;
    private const float SegundosAberta = 0.35f;
    private const float SegundosFechando = 0.25f;

    // Balanço da raiz, na lixeira sem tampa: graus para cada lado e duração.
    internal const float AnguloDoBalanco = 6f;
    private const float SegundosDoBalanco = 0.3f;

    // Texto da mira (sem "segure": a HUD põe a linha "segure o clique").
    private const string TextoEsvaziarSaco = "Esvaziar o saco na <b>lixeira</b>";

    [Header("Lixeira")]
    [Tooltip("Tampa da lixeira, que abre girando no X ao esvaziar o saco. Vazio = a lixeira inteira balança.")]
    [SerializeField] private Transform tampa;

    [Tooltip("Quantos graus a tampa abre (no X local).")]
    [SerializeField] private float anguloAberta = 100f;

    [Tooltip("Som da tampa abrindo e batendo ao esvaziar o saco. Vazio = sem som.")]
    [SerializeField] private AudioClip somTampa;

    // Giro fechado da tampa (guardado na primeira abertura).
    private Quaternion giroFechada;
    private bool giroGuardado;
    private Coroutine abrindo;
    private CanalDeAudio fonte;

    // Giro da raiz antes do balanço (guardado no primeiro balanço).
    private Quaternion giroDaRaiz;
    private bool raizGuardada;
    private Coroutine balancando;

    /* Animação interrompida: a tampa não pode ficar aberta nem a lixeira
     * torta. */
    void OnDisable()
    {
        if (abrindo != null)
        {
            StopCoroutine(abrindo);
            abrindo = null;
        }
        if (giroGuardado && tampa != null)
            tampa.localRotation = giroFechada;

        if (balancando != null)
        {
            StopCoroutine(balancando);
            balancando = null;
        }
        if (raizGuardada)
            transform.localRotation = giroDaRaiz;
    }

    /* O que se faz na lixeira (a HUD acrescenta o "segure o clique"). */
    public override string TextoPara(KitDoAgente kit) => TextoEsvaziarSaco;

    public override bool PodeUsar(KitDoAgente kit, out string motivo)
    {
        motivo = string.Empty;

        // Sem kit não há saco: recusa sem aviso.
        if (kit == null)
            return false;

        if (kit.PneuCarregado != null)
        {
            motivo = "Pneu vai na caçamba da <b>picape da UPA</b>.";
            return false;
        }
        if (kit.Estoque.GarrafasNoSaco <= 0)
        {
            motivo = "O saco está vazio.";
            return false;
        }
        return true;
    }

    public override void Usar(KitDoAgente kit)
    {
        if (kit == null)
            return;

        if (kit.EsvaziarSaco() <= 0)
            return;

        if (tampa != null)
            AbrirTampa();
        else
            Balancar();
        TocarSom();
    }

    /* Sem tampa, a lixeira inteira balança (só em Play). */
    private void Balancar()
    {
        if (!Application.isPlaying || !isActiveAndEnabled)
            return;

        if (balancando != null)
            StopCoroutine(balancando);
        balancando = StartCoroutine(BalancoRoutine());
    }

    private IEnumerator BalancoRoutine()
    {
        for (float t = 0f; t < SegundosDoBalanco; t += Time.deltaTime)
        {
            AplicarBalanco(t / SegundosDoBalanco);
            yield return null;
        }
        AplicarBalanco(1f);
        balancando = null;
    }

    /* Põe a raiz no ponto k (0 a 1) do balanço, em torno do Z do mundo. */
    internal void AplicarBalanco(float k)
    {
        if (!raizGuardada)
        {
            giroDaRaiz = transform.localRotation;
            raizGuardada = true;
        }

        Vector3 eixo = transform.parent != null ? transform.parent.InverseTransformDirection(Vector3.forward) : Vector3.forward;
        transform.localRotation = Quaternion.AngleAxis(AnguloDoBalancoEm(k), eixo) * giroDaRaiz;
    }

    /* O ângulo no ponto k do balanço: uma oscilação completa, de um lado ao
     * outro, começando e terminando em 0. */
    internal static float AnguloDoBalancoEm(float k)
    {
        return AnguloDoBalanco * Mathf.Sin(Mathf.Clamp01(k) * Mathf.PI * 2f);
    }

    /* A tampa abre e fecha (só em Play). */
    private void AbrirTampa()
    {
        if (!Application.isPlaying || !isActiveAndEnabled)
            return;

        if (!giroGuardado)
        {
            giroFechada = tampa.localRotation;
            giroGuardado = true;
        }
        if (abrindo != null)
            StopCoroutine(abrindo);
        abrindo = StartCoroutine(TampaRoutine());
    }

    private IEnumerator TampaRoutine()
    {
        Quaternion aberta = giroFechada * Quaternion.Euler(-anguloAberta, 0f, 0f);
        Quaternion inicio = tampa.localRotation;

        for (float t = 0f; t < SegundosAbrindo; t += Time.deltaTime)
        {
            tampa.localRotation = Quaternion.Slerp(inicio, aberta, t / SegundosAbrindo);
            yield return null;
        }
        tampa.localRotation = aberta;

        yield return new WaitForSeconds(SegundosAberta);

        // Fecha com ease-in: a tampa cai e bate.
        for (float t = 0f; t < SegundosFechando; t += Time.deltaTime)
        {
            float k = t / SegundosFechando;
            tampa.localRotation = Quaternion.Slerp(aberta, giroFechada, k * k);
            yield return null;
        }
        tampa.localRotation = giroFechada;
        abrindo = null;
    }

    /* O som da tampa, 3D, no canal de efeitos (criado no primeiro uso, só em Play). */
    private void TocarSom()
    {
        if (somTampa == null || !Application.isPlaying)
            return;

        if (fonte == null)
            fonte = CanalDeAudio.Criar(transform, "Som Lixeira", CanalSom.Efeitos, true);
        fonte.Fonte.PlayOneShot(somTampa);
    }
}
