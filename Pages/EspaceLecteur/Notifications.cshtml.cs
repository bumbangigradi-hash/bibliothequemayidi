
using System.Security.Claims;
using bibliotheque_mayidi.Data.Generated;
using bibliotheque_mayidi.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace bibliotheque_mayidi.Pages.EspaceLecteur;

[Authorize]
public class NotificationsModel : PageModel
{
    private readonly BibliothequeScaffoldContext _context;

    public NotificationsModel(BibliothequeScaffoldContext context)
    {
        _context = context;
    }

    public IList<DestinatairesMessage> Messages { get; private set; }
        = new List<DestinatairesMessage>();

    public int TotalNonLus { get; private set; }

    public int TotalMessages { get; private set; }

    public string NomLecteur { get; private set; } = "";

    [BindProperty(SupportsGet = true)]
    public string Filtre { get; set; } = "TOUS";

    public async Task<IActionResult> OnGetAsync()
    {
        var identifiantCompte = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(identifiantCompte, out int codeCompte))
        {
            return Forbid();
        }

        var compte = await _context.ComptesUtilisateurs
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CodeCompte == codeCompte);

        if (compte == null)
        {
            return Forbid();
        }

        NomLecteur = string.Join(" ",
            new[]
            {
                compte.Prenom,
                compte.NomUtilisateur,
                compte.Postnom
            }.Where(n => !string.IsNullOrWhiteSpace(n)));

        var requete = _context.DestinatairesMessages
            .AsNoTracking()
            .Include(d => d.CodeMessageNavigation)
            .Where(d =>
                d.CodeCompte == codeCompte &&
                !d.Archive &&
                !d.SupprimeAffichage);

        TotalMessages = await requete.CountAsync();

        TotalNonLus = await requete.CountAsync(d =>
            d.DateLecture == null);

        if (Filtre == "NON_LUS")
        {
            requete = requete.Where(d => d.DateLecture == null);
        }
        else if (Filtre == "LUS")
        {
            requete = requete.Where(d => d.DateLecture != null);
        }

        Messages = await requete
            .OrderByDescending(d => d.CodeMessageNavigation.DateEnvoi)
            .ToListAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostLireAsync(long codeMessage)
    {
        var identifiantCompte = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(identifiantCompte, out int codeCompte))
        {
            return Forbid();
        }

        var destinataire = await _context.DestinatairesMessages
            .FirstOrDefaultAsync(d =>
                d.CodeCompte == codeCompte &&
                d.CodeMessage == codeMessage &&
                !d.Archive &&
                !d.SupprimeAffichage);

        if (destinataire == null)
        {
            return NotFound();
        }

        if (destinataire.DateLecture == null)
        {
            destinataire.DateLecture = DateTime.Now;
            await _context.SaveChangesAsync();
        }

        return RedirectToPage(new { Filtre });
    }
}
