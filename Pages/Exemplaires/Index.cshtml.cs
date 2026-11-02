
using bibliotheque_mayidi.Data.Generated;
using bibliotheque_mayidi.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace bibliotheque_mayidi.Pages.Exemplaires;

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
    public string? RayonSelectionne { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? StatutSelectionne { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? CollectionSelectionnee { get; set; }

    public IList<Exemplaire> Exemplaires { get; set; } =
        new List<Exemplaire>();

    public IList<Rayon> Rayons { get; set; } =
        new List<Rayon>();

    public IList<Collection> Collections { get; set; } =
        new List<Collection>();

    public int TotalExemplaires { get; set; }
    public int TotalDisponibles { get; set; }
    public int TotalEmpruntes { get; set; }
    public int TotalEndommages { get; set; }

    public async Task OnGetAsync()
    {
        Rayons = await _context.Rayons
            .AsNoTracking()
            .Where(r => r.Actif)
            .OrderBy(r => r.NomRayon)
            .ToListAsync();

        Collections = await _context.Collections
            .AsNoTracking()
            .OrderBy(c => c.NomCollection)
            .ToListAsync();

        IQueryable<Exemplaire> requete = _context.Exemplaires
            .AsNoTracking()
            .Include(e => e.CodeDocumentNavigation)
                .ThenInclude(d => d.CodeCollectionNavigation)
            .Include(e => e.CodeDocumentNavigation)
                .ThenInclude(d => d.DocumentsAuteurs)
                    .ThenInclude(da => da.CodeAuteurNavigation)
            .Include(e => e.CodeEmplacementNavigation)
                .ThenInclude(em => em.CodeEtagereNavigation)
                    .ThenInclude(et => et.CodeRayonNavigation);

        // Recherche par titre, auteur, cote, inventaire ou code-barres.
        if (!string.IsNullOrWhiteSpace(Recherche))
        {
            string terme = Recherche.Trim();

            requete = requete.Where(e =>
                e.Cote.Contains(terme) ||
                e.NumeroInventaire.Contains(terme) ||
                (e.CodeBarres != null &&
                 e.CodeBarres.Contains(terme)) ||
                e.CodeDocumentNavigation.Titre.Contains(terme) ||
                e.CodeDocumentNavigation.DocumentsAuteurs.Any(da =>
                    da.CodeAuteurNavigation.NomAuteur.Contains(terme) ||
                    (da.CodeAuteurNavigation.PrenomAuteur != null &&
                     da.CodeAuteurNavigation.PrenomAuteur.Contains(terme))));
        }

        // Filtre par rayon.
        if (!string.IsNullOrWhiteSpace(RayonSelectionne))
        {
            string codeRayon = RayonSelectionne;

            requete = requete.Where(e =>
                e.CodeEmplacementNavigation
                    .CodeEtagereNavigation.CodeRayon == codeRayon);
        }

        // Filtre par statut de l'exemplaire.
        if (!string.IsNullOrWhiteSpace(StatutSelectionne))
        {
            string statut = StatutSelectionne;

            requete = requete.Where(e =>
                e.StatutExemplaire == statut);
        }

        // Filtre par collection du document.
        if (!string.IsNullOrWhiteSpace(CollectionSelectionnee) &&
            int.TryParse(CollectionSelectionnee, out int codeCollection))
        {
            requete = requete.Where(e =>
                e.CodeDocumentNavigation.CodeCollection == codeCollection);
        }

        Exemplaires = await requete
            .OrderBy(e => e.CodeDocumentNavigation.Titre)
            .ThenBy(e => e.Cote)
            .ToListAsync();

        // Statistiques générales, indépendantes des filtres.
        TotalExemplaires = await _context.Exemplaires.CountAsync();

        TotalDisponibles = await _context.Exemplaires
            .CountAsync(e => e.StatutExemplaire == "DISPONIBLE");

        TotalEmpruntes = await _context.Exemplaires
            .CountAsync(e => e.StatutExemplaire == "EMPRUNTE");

        TotalEndommages = await _context.Exemplaires
            .CountAsync(e => e.EtatDocument == "ENDOMMAGE");
    }
}
