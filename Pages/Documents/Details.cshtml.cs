
using bibliotheque_mayidi.Data.Generated;
using bibliotheque_mayidi.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace bibliotheque_mayidi.Pages.Documents;

public class DetailsModel : PageModel
{
    private readonly BibliothequeScaffoldContext _context;

    public DetailsModel(BibliothequeScaffoldContext context)
    {
        _context = context;
    }

    public Document? Document { get; set; }

    public string Auteurs { get; set; } = string.Empty;

    public int NombreExemplaires { get; set; }

    public int NombreDisponibles { get; set; }

    public int NombreEmpruntes { get; set; }

    public int NombreReserves { get; set; }

    public int NombreEndommages { get; set; }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        Document = await _context.Documents
            .AsNoTracking()
            .Include(d => d.CodeTypeDocumentNavigation)
            .Include(d => d.CodeCollectionNavigation)
            .Include(d => d.CodeEditeurNavigation)
            .Include(d => d.DocumentsAuteurs)
                .ThenInclude(da => da.CodeAuteurNavigation)
            .Include(d => d.Exemplaires)
                .ThenInclude(e => e.CodeEmplacementNavigation)
                    .ThenInclude(em => em.CodeEtagereNavigation)
                        .ThenInclude(et => et.CodeRayonNavigation)
            .FirstOrDefaultAsync(d => d.CodeDocument == id.Value);

        if (Document == null)
        {
            return NotFound();
        }

        Auteurs = string.Join(
            ", ",
            Document.DocumentsAuteurs
                .OrderBy(da => da.OrdreAffichage)
                .Select(da =>
                    string.Join(
                        " ",
                        new[]
                        {
                            da.CodeAuteurNavigation.PrenomAuteur,
                            da.CodeAuteurNavigation.NomAuteur
                        }.Where(n => !string.IsNullOrWhiteSpace(n))
                    )
                )
        );

        NombreExemplaires = Document.Exemplaires.Count;

        NombreDisponibles = Document.Exemplaires.Count(
            e => e.StatutExemplaire == "DISPONIBLE");

        NombreEmpruntes = Document.Exemplaires.Count(
            e => e.StatutExemplaire == "EMPRUNTE");

        NombreReserves = Document.Exemplaires.Count(
            e => e.StatutExemplaire == "RESERVE");

        NombreEndommages = Document.Exemplaires.Count(
            e => e.StatutExemplaire == "ENDOMMAGE");

        return Page();
    }
}
