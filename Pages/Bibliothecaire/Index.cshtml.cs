
using System.Threading.Tasks;
using bibliotheque_mayidi.Data.Generated;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace bibliotheque_mayidi.Pages.Bibliothecaire
{
    [Authorize(Roles = "BIBLIOTHECAIRE,ADMINISTRATEUR,RESPONSABLE,VICE_RESPONSABLE")]
    public class IndexModel : PageModel
    {
        private readonly BibliothequeScaffoldContext _context;

        public IndexModel(BibliothequeScaffoldContext context)
        {
            _context = context;
        }

        public int TotalReservations { get; private set; }

        public int ReservationsEnAttente { get; private set; }

        public int ReservationsEnVerification { get; private set; }

        public int ReservationsPretes { get; private set; }

        public int TotalLecteurs { get; private set; }

        public int TotalEmpruntsEnCours { get; private set; }

        public async Task OnGetAsync()
        {
            TotalReservations = await _context.Reservations
                .AsNoTracking()
                .CountAsync();

            ReservationsEnAttente = await _context.Reservations
                .AsNoTracking()
                .CountAsync(r => r.StatutReservation == "EN_ATTENTE");

            ReservationsEnVerification = await _context.Reservations
                .AsNoTracking()
                .CountAsync(r => r.StatutReservation == "EN_VERIFICATION");

            ReservationsPretes = await _context.Reservations
                .AsNoTracking()
                .CountAsync(r => r.StatutReservation == "PRETE");

            TotalLecteurs = await _context.Lecteurs
                .AsNoTracking()
                .CountAsync();

            TotalEmpruntsEnCours = await _context.LignesPrets
                .AsNoTracking()
                .CountAsync(l => l.StatutLigne == "EN_COURS");
        }
    }
}
