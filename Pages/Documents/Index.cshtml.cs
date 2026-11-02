
using bibliotheque_mayidi.Data.Generated;
using bibliotheque_mayidi.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace bibliotheque_mayidi.Pages.Documents;

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
    public string? TypeSelectionne { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? CollectionSelectionnee { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? RayonSelectionne { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? DisponibiliteSelectionnee { get; set; }

    public IList<Document> Documents { get; set; } = new List<Document>();

    public IList<TypesDocument> Types { get; set; } = new List<TypesDocument>();

    public IList<Collection> Collections { get; set; } = new List<Collection>();

    public IList<Rayon> Rayons { get; set; } = new List<Rayon>();

    public async Task OnGetAsync()
    {
        Types = await _context.TypesDocuments
            .AsNoTracking()
            .OrderBy(t => t.NomTypeDocument)
            .ToListAsync();

        Collections = await _context.Collections
            .AsNoTracking()
            .OrderBy(c => c.NomCollection)
            .ToListAsync();

        Rayons = await _context.Rayons
            .AsNoTracking()
            .Where(r => r.Actif)
            .OrderBy(r => r.NomRayon)
            .ToListAsync();

        IQueryable<Document> requete = _context.Documents
            .AsNoTracking()
            .Include(d => d.CodeTypeDocumentNavigation)
            .Include(d => d.CodeCollectionNavigation)
            .Include(d => d.DocumentsAuteurs)
                .ThenInclude(da => da.CodeAuteurNavigation)
            .Include(d => d.Exemplaires)
                .ThenInclude(e => e.CodeEmplacementNavigation)
                    .ThenInclude(em => em.CodeEtagereNavigation)
                        .ThenInclude(et => et.CodeRayonNavigation);

        // Recherche par titre, sous-titre, ISBN/ISSN,
        // mots-clés ou auteur.
        if (!string.IsNullOrWhiteSpace(Recherche))
        {
            string terme = Recherche.Trim();

            requete = requete.Where(d =>
                d.Titre.Contains(terme) ||
                (d.SousTitre != null &&
                 d.SousTitre.Contains(terme)) ||
                (d.IsbnIssn != null &&
                 d.IsbnIssn.Contains(terme)) ||
                (d.MotsCles != null &&
                 d.MotsCles.Contains(terme)) ||
                d.DocumentsAuteurs.Any(da =>
                    da.CodeAuteurNavigation.NomAuteur.Contains(terme) ||
                    (da.CodeAuteurNavigation.PrenomAuteur != null &&
                     da.CodeAuteurNavigation.PrenomAuteur.Contains(terme))
                )
            );
        }

        // Filtre par type de document.
        if (!string.IsNullOrWhiteSpace(TypeSelectionne))
        {
            requete = requete.Where(d =>
                d.CodeTypeDocument == TypeSelectionne);
        }

        // Filtre par collection.
        if (!string.IsNullOrWhiteSpace(CollectionSelectionnee)
            && int.TryParse(CollectionSelectionnee, out int codeCollection))
        {
            requete = requete.Where(d =>
                d.CodeCollection == codeCollection);
        }

        // Filtre par rayon.
        if (!string.IsNullOrWhiteSpace(RayonSelectionne))
        {
            requete = requete.Where(d =>
                d.Exemplaires.Any(e =>
                    e.DateRetrait == null &&
                    e.CodeEmplacementNavigation.Actif &&
                    e.CodeEmplacementNavigation.CodeEtagereNavigation.Actif &&
                    e.CodeEmplacementNavigation.CodeEtagereNavigation.CodeRayon
                        == RayonSelectionne
                )
            );
        }

        // Filtre par disponibilité.
        if (DisponibiliteSelectionnee == "disponible")
        {
            requete = requete.Where(d =>
                d.Exemplaires.Any(e =>
                    e.DateRetrait == null &&
                    e.StatutExemplaire == "DISPONIBLE"));
        }
        else if (DisponibiliteSelectionnee == "emprunte")
        {
            requete = requete.Where(d =>
                d.Exemplaires.Any(e =>
                    e.DateRetrait == null &&
                    e.StatutExemplaire == "EMPRUNTE"));
        }
        else if (DisponibiliteSelectionnee == "consultation")
        {
            requete = requete.Where(d =>
                d.Exemplaires.Any(e =>
                    e.DateRetrait == null &&
                    e.StatutExemplaire == "CONSULTATION"));
        }
        else if (DisponibiliteSelectionnee == "indisponible")
        {
            requete = requete.Where(d =>
                !d.Exemplaires.Any(e =>
                    e.DateRetrait == null &&
                    e.StatutExemplaire == "DISPONIBLE"));
        }

        Documents = await requete
            .OrderBy(d => d.Titre)
            .ToListAsync();
    }
}
