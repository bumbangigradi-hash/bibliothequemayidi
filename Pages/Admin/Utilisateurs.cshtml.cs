
using bibliotheque_mayidi.Data.Generated;
using bibliotheque_mayidi.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace bibliotheque_mayidi.Pages.Admin;

[Authorize(Roles = "ADMINISTRATEUR")]
public class UtilisateursModel : PageModel
{
    private readonly BibliothequeScaffoldContext _context;

    public UtilisateursModel(BibliothequeScaffoldContext context)
    {
        _context = context;
    }

    public List<ComptesUtilisateur> Utilisateurs { get; set; } = new();

    [TempData]
    public string? Message { get; set; }

    public async Task OnGetAsync()
    {
        await ChargerUtilisateursAsync();
    }

    private async Task ChargerUtilisateursAsync()
    {
        Utilisateurs = await _context.ComptesUtilisateurs
            .AsNoTracking()
            .Include(c => c.CodeRoleNavigation)
            .Where(c =>
                c.CodeRole != "LECTEUR" &&
                c.CodeRole != "ADMINISTRATEUR")
            .OrderBy(c => c.NomUtilisateur)
            .ThenBy(c => c.Prenom)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostChangerStatutAsync(int codeCompte)
    {
        var compte = await _context.ComptesUtilisateurs
            .FirstOrDefaultAsync(c =>
                c.CodeCompte == codeCompte &&
                c.CodeRole != "LECTEUR" &&
                c.CodeRole != "ADMINISTRATEUR");

        if (compte == null)
        {
            Message = "Utilisateur introuvable.";
            return RedirectToPage();
        }

        if (compte.StatutCompte == "ACTIF")
        {
            compte.StatutCompte = "SUSPENDU";
            compte.Actif = false;

            Message = "Le compte a été suspendu.";
        }
        else if (compte.StatutCompte == "SUSPENDU")
        {
            compte.StatutCompte = "ACTIF";
            compte.Actif = true;
            compte.NombreEchecsConnexion = 0;

            Message = "Le compte a été réactivé.";
        }
        else
        {
            Message = "Ce compte ne peut pas être réactivé depuis cette interface.";
            return RedirectToPage();
        }

        compte.DateDerniereModification = DateTime.Now;

        await _context.SaveChangesAsync();

        return RedirectToPage();
    }
}
