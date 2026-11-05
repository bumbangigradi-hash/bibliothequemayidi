
using bibliotheque_mayidi.Data.Generated;
using bibliotheque_mayidi.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace bibliotheque_mayidi.Pages.Emprunts;

[Authorize(Roles = "BIBLIOTHECAIRE,ADMINISTRATEUR,RESPONSABLE,VICE_RESPONSABLE")]
public class ImprimerBordereauModel : PageModel
{
    private readonly BibliothequeScaffoldContext _context;

    public ImprimerBordereauModel(BibliothequeScaffoldContext context)
    {
        _context = context;
    }

    public BordereauxPret Bordereau { get; private set; } = null!;

    public List<LignesPret> Lignes { get; private set; } = new();

    public string NomBibliothecaire { get; private set; } = "";

    public async Task<IActionResult> OnGetAsync(int id)
    {
        // Récupération du bordereau et du lecteur.
        var bordereau = await _context.BordereauxPrets
            .AsNoTracking()
            .Include(b => b.CodeLecteurNavigation)
                .ThenInclude(l => l.CodeCompteNavigation)
            .FirstOrDefaultAsync(b => b.CodeBordereauPret == id);

        if (bordereau == null)
        {
            return NotFound("Bordereau de prêt introuvable.");
        }

        Bordereau = bordereau;

        // Récupération des ouvrages empruntés.
        Lignes = await _context.LignesPrets
            .AsNoTracking()
            .Include(l => l.CodeExemplaireNavigation)
                .ThenInclude(e => e.CodeDocumentNavigation)
            .Where(l => l.CodeBordereauPret == id)
            .OrderBy(l => l.CodeExemplaireNavigation.Cote)
            .ToListAsync();

        // Récupération du bibliothécaire ayant enregistré le prêt.
        var compte = await _context.ComptesUtilisateurs
            .AsNoTracking()
            .FirstOrDefaultAsync(c =>
                c.CodeCompte == bordereau.EnregistrePar);

        if (compte != null)
        {
            NomBibliothecaire = string.Join(" ",
                new[]
                {
                    compte.NomUtilisateur,
                    compte.Postnom,
                    compte.Prenom
                }.Where(s => !string.IsNullOrWhiteSpace(s)));
        }

        if (string.IsNullOrWhiteSpace(NomBibliothecaire))
        {
            NomBibliothecaire = "Nom non renseigné";
        }

        return Page();
    }
}
