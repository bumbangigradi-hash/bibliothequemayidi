
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
        var compte = await _context.ComptesUtilisateurs
            .FirstOrDefaultAsync(c =>
                c.CodeCompte == codeCompte &&
                c.CodeRole == "LECTEUR" &&
                c.StatutCompte == "EN_ATTENTE");

        if (compte == null)
        {
            Message = "Cette demande est introuvable ou a déjà été traitée.";
            return RedirectToPage();
        }

        compte.StatutCompte = "ACTIF";
        compte.Actif = true;
        compte.DateValidation = DateTime.Now;
        compte.NombreEchecsConnexion = 0;

        await _context.SaveChangesAsync();

        Message = "Le compte du lecteur a été validé avec succès.";

        return RedirectToPage();
    }
}
