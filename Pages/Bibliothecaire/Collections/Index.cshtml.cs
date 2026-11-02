
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using bibliotheque_mayidi.Data.Generated;
using bibliotheque_mayidi.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace bibliotheque_mayidi.Pages.Bibliothecaire.Collections
{
    [Authorize(Roles = "BIBLIOTHECAIRE,ADMINISTRATEUR")]
    public class IndexModel : PageModel
    {
        private readonly BibliothequeScaffoldContext _context;

        public IndexModel(BibliothequeScaffoldContext context)
        {
            _context = context;
        }

        public List<Collection> Collections { get; set; } = new();

        [TempData]
        public string? Message { get; set; }

        public async Task OnGetAsync()
        {
            Collections = await _context.Collections
                .AsNoTracking()
                .Include(c => c.Documents)
                .OrderBy(c => c.NomCollection)
                .ToListAsync();
        }
    }
}
