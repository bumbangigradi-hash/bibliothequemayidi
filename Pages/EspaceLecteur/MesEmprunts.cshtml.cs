
using System.Security.Claims;
using bibliotheque_mayidi.Data.Generated;
using bibliotheque_mayidi.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace bibliotheque_mayidi.Pages.EspaceLecteur;

[Authorize]
public class MesEmpruntsModel : PageModel
{
    private readonly BibliothequeScaffoldContext _context;

    public MesEmpruntsModel(BibliothequeScaffoldContext context)
    {
        _context = context;
    }

    public IList<LignesPret> Emprunts { get; private set; }
        = new List<LignesPret>();

    public int TotalEnCours { get; private set; }

    public int TotalEnRetard { get; private set; }

    public int TotalRetournes { get; private set; }

    public string NomLecteur { get; private set; } = "";

    [BindProperty(SupportsGet = true)]
    public string Filtre { get; set; } = "EN_COURS";

    public async Task<IActionResult> OnGetAsync()
    {
        var identifiantCompte = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(identifiantCompte, out int codeCompte))
        {
            return Forbid();
        }

        var lecteur = await _context.Lecteurs
            .AsNoTracking()
            .Include(l => l.CodeCompteNavigation)
            .FirstOrDefaultAsync(l => l.CodeCompte == codeCompte);

        if (lecteur == null)
        {
            return Page();
        }

        NomLecteur = string.Join(" ",
            new[]
            {
                lecteur.CodeCompteNavigation.Prenom,
                lecteur.CodeCompteNavigation.NomUtilisateur,
                lecteur.CodeCompteNavigation.Postnom
            }.Where(n => !string.IsNullOrWhiteSpace(n)));

        var lignes = _context.LignesPrets
            .AsNoTracking()
            .Where(l => l.CodeBordereauPretNavigation.CodeLecteur == lecteur.CodeLecteur);

        TotalEnCours = await lignes.CountAsync(l =>
            l.StatutLigne == "EN_COURS");

        var aujourdHui = DateOnly.FromDateTime(DateTime.Today);

        TotalEnRetard = await lignes.CountAsync(l =>
            l.StatutLigne == "EN_COURS" &&
            l.DateEcheance < aujourdHui);

        TotalRetournes = await lignes.CountAsync(l =>
            l.StatutLigne == "RETOURNE");

        IQueryable<LignesPret> requete = lignes
            .Include(l => l.CodeBordereauPretNavigation)
            .Include(l => l.CodeExemplaireNavigation)
                .ThenInclude(e => e.CodeDocumentNavigation);

        if (Filtre == "EN_COURS")
        {
            requete = requete.Where(l => l.StatutLigne == "EN_COURS");
        }
        else if (Filtre == "EN_RETARD")
        {
            requete = requete.Where(l =>
                l.StatutLigne == "EN_COURS" &&
                l.DateEcheance < aujourdHui);
        }
        else if (Filtre == "RETOURNES")
        {
            requete = requete.Where(l => l.StatutLigne == "RETOURNE");
        }

        Emprunts = await requete
            .OrderByDescending(l => l.DateRetrait)
            .ToListAsync();

        return Page();
    }
}
