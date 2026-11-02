using bibliotheque_mayidi.Data.Generated;
using bibliotheque_mayidi.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace bibliotheque_mayidi.Pages.Documents;

public class EditModel : PageModel
{
    private readonly BibliothequeScaffoldContext _context;

    public EditModel(BibliothequeScaffoldContext context)
    {
        _context = context;
    }

    public Document Document { get; set; } = null!;

    public IList<Collection> Collections { get; set; }
        = new List<Collection>();

    [BindProperty]
    public int? CodeCollection { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Document? document = await _context.Documents
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.CodeDocument == id);

        if (document == null)
        {
            return NotFound();
        }

        Document = document;
        CodeCollection = document.CodeCollection;

        await ChargerCollectionsAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        Document? document = await _context.Documents
            .FirstOrDefaultAsync(d => d.CodeDocument == id);

        if (document == null)
        {
            return NotFound();
        }

        if (CodeCollection.HasValue)
        {
            bool collectionExiste = await _context.Collections
                .AnyAsync(c => c.CodeCollection == CodeCollection.Value);

            if (!collectionExiste)
            {
                ModelState.AddModelError(
                    nameof(CodeCollection),
                    "La collection sélectionnée n'existe pas."
                );

                Document = document;
                await ChargerCollectionsAsync();

                return Page();
            }
        }

        document.CodeCollection = CodeCollection;

        await _context.SaveChangesAsync();

        TempData["Succes"] =
            "La collection du document a été modifiée avec succès.";

        return RedirectToPage("./Index");
    }

    private async Task ChargerCollectionsAsync()
    {
        Collections = await _context.Collections
            .AsNoTracking()
            .OrderBy(c => c.NomCollection)
            .ToListAsync();
    }
}