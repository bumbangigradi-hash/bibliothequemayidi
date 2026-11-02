
using System.Security.Claims;
using bibliotheque_mayidi.Data.Generated;
using bibliotheque_mayidi.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace bibliotheque_mayidi.Pages.EspaceLecteur;

[Authorize]
public class MesReservationsModel : PageModel
{
    private readonly BibliothequeScaffoldContext _context;

    public MesReservationsModel(BibliothequeScaffoldContext context)
    {
        _context = context;
    }

    public IList<Reservation> Reservations { get; private set; }
        = new List<Reservation>();

    public int TotalReservations { get; private set; }

    public int TotalEnAttente { get; private set; }

    public int TotalPretes { get; private set; }

    public int TotalRetraitsEffectues { get; private set; }

    public string NomLecteur { get; private set; } = "";

    [BindProperty(SupportsGet = true)]
    public string Filtre { get; set; } = "TOUTES";

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

        var requeteBase = _context.Reservations
            .AsNoTracking()
            .Where(r => r.CodeLecteur == lecteur.CodeLecteur);

        TotalReservations = await requeteBase.CountAsync();

        TotalEnAttente = await requeteBase.CountAsync(r =>
            r.StatutReservation == "EN_ATTENTE" ||
            r.StatutReservation == "EN_VERIFICATION");

        TotalPretes = await requeteBase.CountAsync(r =>
            r.StatutReservation == "PRETE");

        TotalRetraitsEffectues = await requeteBase.CountAsync(r =>
            r.StatutReservation == "HONOREE");

        IQueryable<Reservation> requete = requeteBase
            .Include(r => r.CodeDocumentNavigation)
            .Include(r => r.CodeExemplaireNavigation);

        if (Filtre == "EN_ATTENTE")
        {
            requete = requete.Where(r =>
                r.StatutReservation == "EN_ATTENTE" ||
                r.StatutReservation == "EN_VERIFICATION");
        }
        else if (Filtre == "PRETES")
        {
            requete = requete.Where(r =>
                r.StatutReservation == "PRETE");
        }
        else if (Filtre == "HONOREES")
        {
            requete = requete.Where(r =>
                r.StatutReservation == "HONOREE");
        }
        else if (Filtre == "EXPIREES")
        {
            requete = requete.Where(r =>
                r.StatutReservation == "EXPIREE");
        }
        else if (Filtre == "ANNULEES")
        {
            requete = requete.Where(r =>
                r.StatutReservation == "ANNULEE" ||
                r.StatutReservation == "REFUSEE");
        }

        Reservations = await requete
            .OrderByDescending(r => r.DateReservation)
            .ToListAsync();

        return Page();
    }
}
