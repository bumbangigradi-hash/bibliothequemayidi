
using bibliotheque_mayidi.Data.Generated;
using bibliotheque_mayidi.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace bibliotheque_mayidi.Pages.Exemplaires;

public class DetailsModel : PageModel
{
    private readonly BibliothequeScaffoldContext _context;

    public DetailsModel(BibliothequeScaffoldContext context)
    {
        _context = context;
    }

    public Exemplaire Exemplaire { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var exemplaire = await _context.Exemplaires
            .AsNoTracking()
            .Include(e => e.CodeDocumentNavigation)
                .ThenInclude(d => d.CodeTypeDocumentNavigation)
            .Include(e => e.CodeDocumentNavigation)
                .ThenInclude(d => d.DocumentsAuteurs)
                    .ThenInclude(da => da.CodeAuteurNavigation)
            .Include(e => e.CodeEmplacementNavigation)
                .ThenInclude(em => em.CodeEtagereNavigation)
                    .ThenInclude(et => et.CodeRayonNavigation)
            .FirstOrDefaultAsync(e => e.CodeExemplaire == id);

        if (exemplaire == null)
        {
            return NotFound();
        }

        Exemplaire = exemplaire;

        return Page();
    }
}
