
using System.Security.Claims;
using bibliotheque_mayidi.Data.Generated;
using bibliotheque_mayidi.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace bibliotheque_mayidi.Pages.Reservations;

[Authorize(Roles = "BIBLIOTHECAIRE,ADMINISTRATEUR,RESPONSABLE,VICE_RESPONSABLE")]
public class IndexModel : PageModel
{
    private readonly BibliothequeScaffoldContext _context;

    public IndexModel(BibliothequeScaffoldContext context)
    {
        _context = context;
    }

    public IList<Reservation> Reservations { get; private set; }
        = new List<Reservation>();

    public int TotalReservations { get; private set; }
    public int EnAttente { get; private set; }
    public int EnVerification { get; private set; }
    public int Pretes { get; private set; }
    public int Honorees { get; private set; }

    [BindProperty(SupportsGet = true)]
    public string Filtre { get; set; } = "TOUTES";

    [BindProperty(SupportsGet = true)]
    public string Recherche { get; set; } = "";

    [BindProperty]
    public string MotifRefus { get; set; } = "";

    public async Task<IActionResult> OnGetAsync()
    {
        var acces = await ChargerReservationsAsync();

        if (acces != null)
            return acces;

        return Page();
    }

    private async Task<IActionResult?> ChargerReservationsAsync()
    {
        var baseRequete = _context.Reservations
            .AsNoTracking()
            .Include(r => r.CodeDocumentNavigation)
            .Include(r => r.CodeLecteurNavigation)
                .ThenInclude(l => l.CodeCompteNavigation)
            .Include(r => r.CodeExemplaireNavigation)
            .AsQueryable();

        TotalReservations = await baseRequete.CountAsync();

        EnAttente = await baseRequete.CountAsync(
            r => r.StatutReservation == "EN_ATTENTE");

        EnVerification = await baseRequete.CountAsync(
            r => r.StatutReservation == "EN_VERIFICATION");

        Pretes = await baseRequete.CountAsync(
            r => r.StatutReservation == "PRETE");

        Honorees = await baseRequete.CountAsync(
            r => r.StatutReservation == "HONOREE");

        var requete = baseRequete;

        if (!string.IsNullOrWhiteSpace(Recherche))
        {
            var terme = Recherche.Trim();

            requete = requete.Where(r =>
                r.NumeroReservation.Contains(terme) ||
                r.CodeDocumentNavigation.Titre.Contains(terme) ||
                r.CodeLecteurNavigation.NumeroLecteur.Contains(terme) ||
                r.CodeLecteurNavigation.CodeCompteNavigation
                    .NomUtilisateur.Contains(terme) ||
                r.CodeLecteurNavigation.CodeCompteNavigation
                    .Identifiant.Contains(terme));
        }

        switch (Filtre)
        {
            case "EN_ATTENTE":
                requete = requete.Where(
                    r => r.StatutReservation == "EN_ATTENTE");
                break;

            case "EN_VERIFICATION":
                requete = requete.Where(
                    r => r.StatutReservation == "EN_VERIFICATION");
                break;

            case "PRETE":
                requete = requete.Where(
                    r => r.StatutReservation == "PRETE");
                break;

            case "HONOREE":
                requete = requete.Where(
                    r => r.StatutReservation == "HONOREE");
                break;

            case "EXPIREE":
                requete = requete.Where(
                    r => r.StatutReservation == "EXPIREE");
                break;

            case "ANNULEE":
                requete = requete.Where(
                    r => r.StatutReservation == "ANNULEE");
                break;

            case "REFUSEE":
                requete = requete.Where(
                    r => r.StatutReservation == "REFUSEE");
                break;
        }

        Reservations = await requete
            .OrderByDescending(r => r.DateReservation)
            .ToListAsync();

        return null;
    }

    public async Task<IActionResult> OnPostValiderAsync(
        long codeReservation)
    {
        if (!TryGetCodeCompte(out int codeCompte))
            return Forbid();

        var reservation = await _context.Reservations
            .Include(r => r.CodeDocumentNavigation)
            .FirstOrDefaultAsync(
                r => r.CodeReservation == codeReservation);

        if (reservation == null)
            return NotFound();

        if (reservation.StatutReservation != "EN_ATTENTE" &&
            reservation.StatutReservation != "EN_VERIFICATION")
        {
            TempData["ErreurReservation"] =
                "Cette réservation ne peut plus être validée.";

            return RetourListe();
        }

        var exemplaire = await _context.Exemplaires
            .Where(e =>
                e.CodeDocument == reservation.CodeDocument &&
                e.StatutExemplaire == "DISPONIBLE")
            .OrderBy(e => e.CodeExemplaire)
            .FirstOrDefaultAsync();

        if (exemplaire == null)
        {
            TempData["ErreurReservation"] =
                "Aucun exemplaire disponible pour ce document. " +
                "La réservation n'a pas été validée.";

            return RetourListe();
        }

        var maintenant = DateTime.Now;

        reservation.CodeExemplaire = exemplaire.CodeExemplaire;
        reservation.StatutReservation = "PRETE";
        reservation.DateVerification = maintenant;
        reservation.VerifiePar = codeCompte;
        reservation.DateNotificationDisponibilite = maintenant;
        reservation.DateLimiteRetrait = maintenant.AddHours(48);

        exemplaire.StatutExemplaire = "RESERVE";

        await _context.SaveChangesAsync();

        TempData["ConfirmationReservation"] =
            $"Réservation {reservation.NumeroReservation} validée. " +
            "Le lecteur dispose de 48 heures pour retirer le document.";

        return RetourListe();
    }

    public async Task<IActionResult> OnPostRefuserAsync(
        long codeReservation)
    {
        if (!TryGetCodeCompte(out int codeCompte))
            return Forbid();

        if (string.IsNullOrWhiteSpace(MotifRefus))
        {
            TempData["ErreurReservation"] =
                "Veuillez préciser le motif du refus.";

            return RetourListe();
        }

        var reservation = await _context.Reservations
            .FirstOrDefaultAsync(
                r => r.CodeReservation == codeReservation);

        if (reservation == null)
            return NotFound();

        if (reservation.StatutReservation != "EN_ATTENTE" &&
            reservation.StatutReservation != "EN_VERIFICATION")
        {
            TempData["ErreurReservation"] =
                "Seules les demandes en attente ou en vérification " +
                "peuvent être refusées.";

            return RetourListe();
        }

        reservation.StatutReservation = "REFUSEE";
        reservation.DateVerification = DateTime.Now;
        reservation.VerifiePar = codeCompte;
        reservation.MotifRefus = MotifRefus.Trim();

        await _context.SaveChangesAsync();

        TempData["ConfirmationReservation"] =
            $"La réservation {reservation.NumeroReservation} a été refusée.";

        return RetourListe();
    }

    private bool TryGetCodeCompte(out int codeCompte)
    {
        var identifiant = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        return int.TryParse(identifiant, out codeCompte);
    }

    private IActionResult RetourListe()
    {
        return RedirectToPage(new
        {
            Filtre,
            Recherche
        });
    }
}
