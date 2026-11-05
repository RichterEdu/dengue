using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;

/* ============================================================
 *  MigracaoGerenciadorTestes.cs
 *  ---------------------------------------------------------------------
 *  Testes EditMode (Parte B) da migração para o GerenciadorDeFocos:
 *
 *    - SoOGerenciadorVarreFocos: nenhum script de jogo (Assets/Scripts)
 *      varre a cena atrás dos Interactable, a não ser o próprio
 *      GerenciadorDeFocos. Os outros (HUD, rede social, história, som,
 *      brilho) pegam a lista e os eventos dele. As linhas de comentário
 *      (as que, sem os espaços do começo, começam com //, /* ou *) não
 *      contam;
 *    - FatorDoEstagio_Tabela: o zumbido do SomDosFocos fica mais alto a
 *      cada estágio (ovo 0,45, larva 0,7, mosquito 1,0).
 *
 *  Rodar:  Window > General > Test Runner > EditMode (categoria ParteB)
 *  ou      unity command run_tests --mode EditMode --filter_type category --filter ParteB
 * ============================================================ */

[Category("ParteB")]
public class MigracaoGerenciadorTestes
{
    private const string Varredura = "FindObjectsByType<Interactable>";

    [Test]
    public void SoOGerenciadorVarreFocos()
    {
        string pasta = Path.Combine(Application.dataPath, "Scripts");
        List<string> culpados = new List<string>();

        foreach (string arquivo in Directory.GetFiles(pasta, "*.cs"))
        {
            if (Path.GetFileName(arquivo) == "GerenciadorDeFocos.cs")
                continue;

            string[] linhas = File.ReadAllText(arquivo).Split('\n');
            for (int i = 0; i < linhas.Length; i++)
            {
                string linha = linhas[i].TrimStart();
                if (linha.StartsWith("//") || linha.StartsWith("/*") || linha.StartsWith("*"))
                    continue;

                if (linha.Contains(Varredura))
                    culpados.Add(Path.GetFileName(arquivo) + ":" + (i + 1));
            }
        }

        Assert.IsEmpty(culpados,
            "Só o GerenciadorDeFocos pode varrer a cena atrás dos focos; estes ainda varrem: " + string.Join(", ", culpados));
    }

    [Test]
    public void FatorDoEstagio_Tabela()
    {
        Vector3 tabela = new Vector3(0.45f, 0.7f, 1.0f);

        Assert.AreEqual(0.45f, SomDosFocos.FatorDoEstagio(EstagioDoFoco.Ovo, tabela), 1e-5f);
        Assert.AreEqual(0.7f, SomDosFocos.FatorDoEstagio(EstagioDoFoco.Larva, tabela), 1e-5f);
        Assert.AreEqual(1.0f, SomDosFocos.FatorDoEstagio(EstagioDoFoco.Mosquito, tabela), 1e-5f);
    }
}
