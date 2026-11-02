
using bibliotheque_mayidi.Data.Generated;
using bibliotheque_mayidi.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace bibliotheque_mayidi.Pages.Lecteurs;

public class IndexModel : PageModel
{
    private readonly BibliothequeScaffoldContext _context;

    public IndexModel(BibliothequeScaffoldContext context)
    {
        _context = context;
    }

    [BindProperty(SupportsGet = true)]
    public string? Recherche { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? CategorieSelectionnee { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? StatutSelectionne { get; set; }

    public IList<Lecteur> Lecteurs { get; set; } = new List<Lecteur>();

    public int TotalLecteurs { get; set; }

    public int TotalActifs { get; set; }

    public int TotalSuspendus { get; set; }

    public async Task OnGetAsync()
    {
        IQueryable<Lecteur> requete = _context.Lecteurs
            .AsNoTracking()
            .Include(l => l.CodeCompteNavigation);

        if (!string.IsNullOrWhiteSpace(Recherche))
        {
            string terme = Recherche.Trim();

            requete = requete.Where(l =>
                l.NumeroLecteur.Contains(terme) ||
                (l.MatriculeSeminaire != null &&
                 l.MatriculeSeminaire.Contains(terme)) ||
                (l.InstitutionOrigine != null &&
                 l.InstitutionOrigine.Contains(terme)) ||
                l.CodeCompteNavigation.NomUtilisateur.Contains(terme) ||
                (l.CodeCompteNavigation.Prenom != null &&
                 l.CodeCompteNavigation.Prenom.Contains(terme)) ||
                (l.CodeCompteNavigation.Postnom != null &&
                 l.CodeCompteNavigation.Postnom.Contains(terme)) ||
                l.CodeCompteNavigation.Identifiant.Contains(terme));
        }

        if (!string.IsNullOrWhiteSpace(CategorieSelectionnee))
        {
            string categorie = CategorieSelectionnee;
            requete = requete.Where(l => l.CategorieLecteur == categorie);
        }

        if (!string.IsNullOrWhiteSpace(StatutSelectionne))
        {
            string statut = StatutSelectionne;
            requete = requete.Where(l => l.StatutLecteur == statut);
        }

        Lecteurs = await requete
            .OrderBy(l => l.CodeCompteNavigation.NomUtilisateur)
            .ThenBy(l => l.CodeCompteNavigation.Prenom)
            .ToListAsync();

        TotalLecteurs = await _context.Lecteurs.CountAsync();

        TotalActifs = await _context.Lecteurs
            .CountAsync(l => l.StatutLecteur == "ACTIF");

        TotalSuspendus = await _context.Lecteurs
            .CountAsync(l => l.StatutLecteur == "SUSPENDU");
    }
}
