using System.Collections.Generic;
using UnityEngine;

/* ============================================================
 *  EmissionHighlightUtil.cs
 *  ---------------------------------------------------------------------
 *  Helper estático para aplicar/restaurar brilho de emissão (_EMISSION /
 *  _EmissionColor) em um grupo de Renderer, sem afetar o material
 *  compartilhado (asset) — mesma técnica já usada em Interactor.cs e
 *  Interactable.cs, extraída aqui para não triplicar a lógica no
 *  FocoProximityGlow.cs.
 *
 *  Destacavel diz quais Renderer entram no brilho (aqui, no Interactor e
 *  no flash do Interactable): o ParticleSystemRenderer fica de fora (o
 *  enxame de mosquitos e os estouros). O material de partícula não tem
 *  _EmissionColor (o GetColor logaria erro no Console) e o rend.material
 *  criaria uma cópia do material compartilhado do enxame que nunca é
 *  destruída.
 * ============================================================ */

public static class EmissionHighlightUtil
{
    public class HighlightEntry
    {
        public Renderer renderer;
        public Material material;
        public Color originalEmissionColor;
        public bool emissionEnabled;
    }

    /* Entra no brilho de emissão: tem material e não é partícula (o
     * enxame e os estouros não têm emissão nem devem ganhar cópia do
     * material). */
    public static bool Destacavel(Renderer rend)
    {
        return rend != null && rend.sharedMaterial != null && !(rend is ParticleSystemRenderer);
    }

    /* Instancia o material de cada Renderer destacável e guarda o estado
     * original da emissão. */
    public static List<HighlightEntry> Capture(IEnumerable<Renderer> renderers)
    {
        List<HighlightEntry> entries = new List<HighlightEntry>();

        foreach (Renderer rend in renderers)
        {
            if (!Destacavel(rend))
                continue;

            Material mat = rend.material;
            entries.Add(new HighlightEntry
            {
                renderer = rend,
                material = mat,
                originalEmissionColor = mat.GetColor("_EmissionColor"),
                emissionEnabled = mat.IsKeywordEnabled("_EMISSION")
            });

            mat.EnableKeyword("_EMISSION");
        }

        return entries;
    }

    /* Aplica a mesma cor de emissão em todas as entradas capturadas. */
    public static void ApplyColor(List<HighlightEntry> entries, Color emission)
    {
        for (int i = entries.Count - 1; i >= 0; i--)
        {
            if (entries[i].renderer == null)
            {
                entries.RemoveAt(i);
                continue;
            }
            entries[i].material.SetColor("_EmissionColor", emission);
        }
    }

    /* Devolve cada material ao estado original (emissão/cor/ativado). */
    public static void Restore(List<HighlightEntry> entries)
    {
        foreach (HighlightEntry entry in entries)
        {
            if (entry.renderer == null || entry.material == null)
                continue;

            if (entry.emissionEnabled)
                entry.material.SetColor("_EmissionColor", entry.originalEmissionColor);
            else
                entry.material.DisableKeyword("_EMISSION");
        }

        entries.Clear();
    }
}
