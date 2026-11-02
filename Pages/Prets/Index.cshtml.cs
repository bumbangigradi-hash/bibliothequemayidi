
using bibliotheque_mayidi.Data.Generated;
using bibliotheque_mayidi.Models.Entities;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace bibliotheque_mayidi.Pages.Prets;

public class IndexModel : PageModel
{
    private readonly BibliothequeScaffoldContext _context;

    public IndexModel(BibliothequeScaffoldContext context)
    {
        _context = context;
    }

    public IList<BordereauxPret> Bordereaux { get; set; }
        = new List<BordereauxPret>();

    public int TotalBordereaux { get; set; }
    public int TotalLignesEnCours { get; set; }
    public int TotalRetards { get; set; }

    public string? Recherche { get; set; }
    public string? StatutSelectionne { get; set; }

    public async Task OnGetAsync(string? recherche, string? statut)
    {
        Recherche = recherche?.Trim();
        StatutSelectionne = statut;

        var aujourdHui = DateOnly.FromDateTime(DateTime.Today);

        var requete = _context.BordereauxPrets
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(Recherche))
        {
            var terme = Recherche;

            requete = requete.Where(b =>
                b.NumeroBordereau.Contains(terme) ||
                b.CodeLecteurNavigation.NumeroLecteur.Contains(terme) ||
                b.CodeLecteurNavigation.CodeCompteNavigation.NomUtilisateur
                    .Contains(terme) ||
                (b.CodeLecteurNavigation.CodeCompteNavigation.Prenom != null &&
                 b.CodeLecteurNavigation.CodeCompteNavigation.Prenom
                    .Contains(terme)) ||
                b.LignesPrets.Any(l =>
                    l.CodeExemplaireNavigation.CodeDocumentNavigation.Titre
                        .Contains(terme)));
        }

        if (!string.IsNullOrWhiteSpace(StatutSelectionne))
        {
            requete = requete.Where(b =>
                b.StatutBordereau == StatutSelectionne);
        }

        TotalBordereaux = await _context.BordereauxPrets
            .CountAsync();

        TotalLignesEnCours = await _context.LignesPrets
            .CountAsync(l => l.StatutLigne == "EN_COURS");

        TotalRetards = await _context.LignesPrets
            .CountAsync(l =>
                l.StatutLigne == "EN_COURS" &&
                l.DateEcheance < aujourdHui);

        Bordereaux = await requete
            .Include(b => b.CodeLecteurNavigation)
                .ThenInclude(l => l.CodeCompteNavigation)
            .Include(b => b.LignesPrets)
                .ThenInclude(l => l.CodeExemplaireNavigation)
                    .ThenInclude(e => e.CodeDocumentNavigation)
            .OrderByDescending(b => b.DatePret)
            .ToListAsync();
    }
}
