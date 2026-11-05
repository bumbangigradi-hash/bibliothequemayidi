
using bibliotheque_mayidi.Data.Generated;
using bibliotheque_mayidi.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace bibliotheque_mayidi.Pages.Admin;

[Authorize(Roles = "ADMINISTRATEUR")]
public class InscriptionsModel : PageModel
{
    private readonly BibliothequeScaffoldContext _context;

    public InscriptionsModel(BibliothequeScaffoldContext context)
    {
        _context = context;
    }

    public List<ComptesUtilisateur> Inscriptions { get; set; } = new();

    [TempData]
    public string? Message { get; set; }

    public async Task OnGetAsync()
    {
        Inscriptions = await _context.ComptesUtilisateurs
            .AsNoTracking()
            .Where(c =>
                c.CodeRole == "LECTEUR" &&
                c.StatutCompte == "EN_ATTENTE")
            .OrderBy(c => c.DateCreation)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostValiderAsync(int codeCompte)
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            var compte = await _context.ComptesUtilisateurs
                .FirstOrDefaultAsync(c =>
                    c.CodeCompte == codeCompte &&
                    c.CodeRole == "LECTEUR");

            if (compte == null)
            {
                Message = "Compte lecteur introuvable.";
                return RedirectToPage();
            }

            var lecteur = await _context.Lecteurs
                .FirstOrDefaultAsync(l => l.CodeCompte == codeCompte);

            if (lecteur == null)
            {
                Message = "La fiche du lecteur est introuvable. Validation impossible.";
                return RedirectToPage();
            }

            if (compte.StatutCompte != "EN_ATTENTE" &&
                lecteur.StatutLecteur == "ACTIF")
            {
                Message = "Cette inscription a déjà été validée.";
                return RedirectToPage();
            }

            var maintenant = DateTime.SpecifyKind(
                DateTime.Now,
                DateTimeKind.Unspecified);

            compte.StatutCompte = "ACTIF";
            compte.Actif = true;
            compte.DateValidation = maintenant;
            compte.NombreEchecsConnexion = 0;

            lecteur.StatutLecteur = "ACTIF";

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            Message = "Le compte et l'inscription du lecteur ont été validés avec succès.";

            return RedirectToPage();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();

            Message = "Une erreur est survenue pendant la validation. Aucune modification n'a été enregistrée.";

            return RedirectToPage();
        }
    }
}
