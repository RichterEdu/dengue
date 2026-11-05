using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* ============================================================
 *  Cacamba.cs
 *  ---------------------------------------------------------------------
 *  A caçamba da picape da UPA (Parte B): ponto de uso que só recebe
 *  PNEUS. Fica num filho da picape (o collider da carroceria), e o saco
 *  de lixo agora vai para as lixeiras (Lixeira.cs).
 *
 *    - com um pneu nas mãos, entrega o pneu (KitDoAgente.EntregarPneu):
 *      ele conclui e é arremessado num arco até a próxima vaga livre da
 *      carroceria (ReservarVaga, que pula as vagas vazias da lista), onde
 *      fica empilhado, sem sumir. No fim do arco, Receber dispara o
 *      OnPneuRecebido (posts, o "Pneu recolhido — N de T" da HUD), a
 *      carroceria balança (a suspensão) e tocam o baque do pneu e o
 *      rangido da suspensão (3D, onde o pneu caiu, só em Play). Sem vaga
 *      livre, o pneu encolhe até sumir 1,2 m acima do pivô da caçamba, e
 *      o Receber sai na hora;
 *    - o número do aviso é o dos pneus recebidos (1, 2, 3...), contado na
 *      reserva e guardado no pneu: dois arremessos ainda no ar chegam
 *      como 1 e 2, e não como 2 e 2 (com ou sem vaga);
 *    - sem pneu, a caçamba é a própria picape: delega a ela (PodeUsar,
 *      Usar, o tempo de segurar e o texto da mira), para mirar na
 *      carroceria também reabastecer. Sem picape, recusa com "Só pneu vai
 *      na caçamba da picape.".
 *
 *  O texto da mira com o pneu nas mãos não diz "segure": a HUD já põe a
 *  linha "segure o clique" embaixo.
 *
 *  Largar o pneu (botão direito) em cima da caçamba também é entrega
 *  (KitDoAgente.LargarPneu), e largar contra a cabine da picape também
 *  (PneuInteractable.OndeCai). Mirar na cabine com o pneu nas mãos
 *  também entrega (PontoDeReabastecimento, que delega à Cacamba filha,
 *  com o tempo de segurar dela).
 *
 *  Desligada no meio do balanço, a carroceria volta ao giro de descanso.
 *
 *  Os nomes 'vagas', 'picape', 'somPneu' e 'somSuspensao' são gravados
 *  por SerializedObject pelo builder da cena, pelo SomBuilder e pelos
 *  testes: não renomear.
 * ============================================================ */

public class Cacamba : PontoDeUso
{
    // Balanço da carroceria ao receber um pneu.
    private const float DuracaoBalanco = 0.4f;
    private const float AnguloBalanco = 3f;

    [Header("Caçamba da picape")]
    [Tooltip("Vagas da carroceria onde os pneus entregues ficam empilhados, na ordem em que são ocupadas. Sem vaga livre, o pneu encolhe até sumir.")]
    [SerializeField] private List<Transform> vagas = new List<Transform>();

    [Tooltip("A picape da UPA (o ponto de reabastecimento). Sem pneu nas mãos, a caçamba delega a ela. Vazio = recusa sem pneu.")]
    [SerializeField] private PontoDeReabastecimento picape;

    [Tooltip("Carroceria que balança (a suspensão) quando um pneu cai nela. Vazio = sem balanço.")]
    [SerializeField] private Transform carroceria;

    [Tooltip("Altura (em unidades) do topo do arco do arremesso do pneu, acima da reta entre as mãos e a vaga.")]
    [SerializeField] private float alturaDoArco = 1.5f;

    [Tooltip("Duração (em segundos) do arremesso do pneu até a vaga.")]
    [SerializeField] private float duracaoArremesso = 0.6f;

    [Header("Sons")]
    [Tooltip("Baque de borracha + metal do pneu caindo na carroceria. Vazio = sem som.")]
    [SerializeField] private AudioClip somPneu;

    [Tooltip("Rangido da suspensão, junto com o baque do pneu. Vazio = sem som.")]
    [SerializeField] private AudioClip somSuspensao;

    [Tooltip("Volume dos sons da caçamba.")]
    [Range(0f, 1f)]
    [SerializeField] private float volumeSons = 0.85f;

    // Pneus recebidos (com ou sem vaga), contados na reserva.
    private int pneusRecebidos;

    // Índice da próxima vaga a olhar na lista (as vazias são puladas).
    private int proximaVaga;

    // Giro de descanso da carroceria (guardado no primeiro balanço).
    private Quaternion giroDaCarroceria;
    private bool giroGuardado;
    private Coroutine balanco;
    private CanalDeAudio fonte;

    /* Um pneu chegou à caçamba: o pneu e quantos pneus ela já recebeu,
     * contando este (1, 2, 3...), na ordem das entregas. */
    public event Action<PneuInteractable, int> OnPneuRecebido;

    /* Vagas já reservadas (os pneus empilhados ou a caminho). */
    public int Ocupadas { get; private set; }

    public int Capacidade => vagas != null ? vagas.Count : 0;

    public float AlturaDoArco => alturaDoArco;
    public float DuracaoArremesso => duracaoArremesso;

    /* Balanço interrompido (a picape saiu de cena): a carroceria não pode
     * ficar torta. */
    void OnDisable()
    {
        if (balanco != null)
        {
            StopCoroutine(balanco);
            balanco = null;
        }
        if (giroGuardado && carroceria != null)
            carroceria.localRotation = giroDaCarroceria;
    }

    public override bool PodeUsar(KitDoAgente kit, out string motivo)
    {
        motivo = string.Empty;

        // Sem kit não há o que jogar: recusa sem aviso.
        if (kit == null)
            return false;

        if (kit.PneuCarregado != null)
            return true;

        if (picape != null)
            return picape.PodeUsar(kit, out motivo);

        motivo = "Só pneu vai na caçamba da picape.";
        return false;
    }

    public override void Usar(KitDoAgente kit)
    {
        if (kit == null)
            return;

        if (kit.PneuCarregado != null)
            kit.EntregarPneu(this);
        else if (picape != null)
            picape.Usar(kit);
    }

    /* Sem pneu, a caçamba é a picape: o tempo de reabastecer é o dela. */
    public override float SegundosParaUsarPara(KitDoAgente kit)
    {
        bool semPneu = kit != null && kit.PneuCarregado == null;
        return semPneu && picape != null ? picape.SegundosParaUsarPara(kit) : base.SegundosParaUsarPara(kit);
    }

    /* Com o pneu nas mãos, o convite a jogar; sem pneu, o texto da picape
     * (sem picape, o TextoAoMirar daqui). */
    public override string TextoPara(KitDoAgente kit)
    {
        if (kit != null && kit.PneuCarregado != null)
            return "Jogar o pneu na <b>caçamba</b>";
        return picape != null ? picape.TextoPara(kit) : base.TextoPara(kit);
    }

    /* Uma entrega: conta o pneu ('numero', o do aviso) e reserva a próxima
     * vaga livre (Ocupadas sobe), pulando as vazias da lista. Devolve null
     * se não há vaga (lista vazia ou cheia). */
    internal Transform ReservarVaga(out int numero)
    {
        numero = ++pneusRecebidos;

        while (vagas != null && proximaVaga < vagas.Count)
        {
            Transform vaga = vagas[proximaVaga++];
            if (vaga != null)
            {
                Ocupadas++;
                return vaga;
            }
        }
        return null;
    }

    /* O pneu chegou (no fim do arco ou, sem vaga, na hora): avisa com o
     * número dele (o da reserva), balança a carroceria e toca os sons. */
    internal void Receber(PneuInteractable pneu, int numero)
    {
        OnPneuRecebido?.Invoke(pneu, numero);
        Balancar();
        TocarSons(pneu);
    }

    /* O baque e o rangido, 3D, onde o pneu caiu, no canal de efeitos (a
     * fonte é criada no primeiro uso, só em Play). */
    private void TocarSons(PneuInteractable pneu)
    {
        if ((somPneu == null && somSuspensao == null) || !Application.isPlaying || !isActiveAndEnabled)
            return;

        if (fonte == null)
        {
            fonte = CanalDeAudio.Criar(transform, "Som Caçamba", CanalSom.Efeitos, true);
            fonte.Fonte.minDistance = 1.5f;
            fonte.Fonte.maxDistance = 30f;
        }

        fonte.transform.position = pneu != null ? pneu.transform.position : transform.position;
        if (somPneu != null)
            fonte.Fonte.PlayOneShot(somPneu, volumeSons);
        if (somSuspensao != null)
            fonte.Fonte.PlayOneShot(somSuspensao, volumeSons);
    }

    /* A suspensão: a carroceria balança ±AnguloBalanco no X. Só em Play
     * (no EditMode as corrotinas não andam). */
    private void Balancar()
    {
        if (carroceria == null || !Application.isPlaying || !isActiveAndEnabled)
            return;

        if (!giroGuardado)
        {
            giroDaCarroceria = carroceria.localRotation;
            giroGuardado = true;
        }
        if (balanco != null)
            StopCoroutine(balanco);
        balanco = StartCoroutine(BalancoRoutine());
    }

    /* Uma oscilação que morre em DuracaoBalanco e volta ao giro de descanso. */
    private IEnumerator BalancoRoutine()
    {
        for (float t = 0f; t < DuracaoBalanco; t += Time.deltaTime)
        {
            float k = t / DuracaoBalanco;
            float angulo = AnguloBalanco * Mathf.Sin(k * Mathf.PI * 2f) * (1f - k);
            carroceria.localRotation = giroDaCarroceria * Quaternion.Euler(angulo, 0f, 0f);
            yield return null;
        }

        carroceria.localRotation = giroDaCarroceria;
        balanco = null;
    }
}
