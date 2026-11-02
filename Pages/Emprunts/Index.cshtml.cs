using System.Security.Claims;
using bibliotheque_mayidi.Data.Generated;
using bibliotheque_mayidi.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace bibliotheque_mayidi.Pages.Emprunts;

[Authorize(Roles = "BIBLIOTHECAIRE,ADMINISTRATEUR,RESPONSABLE,VICE_RESPONSABLE")]
public class IndexModel : PageModel
{
    private readonly BibliothequeScaffoldContext _context;

    public IndexModel(BibliothequeScaffoldContext context)
    {
        _context = context;
    }

    public List<LignesPret> Emprunts { get; private set; } = new();
    public List<Lecteur> Lecteurs { get; private set; } = new();
    public List<Exemplaire> ExemplairesDisponibles { get; private set; } = new();
    public List<Reservation> ReservationsPretes { get; private set; } = new();

    [BindProperty]
    public int CodeLecteur { get; set; }

    [BindProperty]
    public int CodeExemplaire { get; set; }

    [BindProperty]
    public long? CodeReservation { get; set; }

    [BindProperty]
    public string? Observations { get; set; }

    public string? MessageErreur { get; private set; }
    public string? MessageSucces { get; private set; }

    public int TotalEmpruntsEnCours { get; private set; }
    public int TotalEmpruntsRetournes { get; private set; }
    public int TotalEmpruntsEnRetard { get; private set; }

    public async Task OnGetAsync(string? message = null)
    {
        MessageSucces = message;
        await ChargerDonneesAsync();
    }

    public async Task<IActionResult> OnPostEnregistrerAsync()
    {
        var compteClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(compteClaim, out var codeCompte))
            return Forbid();

        var compteExiste = await _context.ComptesUtilisateurs
            .AnyAsync(c =>
                c.CodeCompte == codeCompte &&
                c.Actif &&
                c.StatutCompte == "ACTIF");

        if (!compteExiste)
            return Forbid();

        Reservation? reservation = null;
        Lecteur? lecteur;
        Exemplaire? exemplaire;

        // ---------------------------------------------------------
        // 1. Retrait à partir d'une réservation
        // ---------------------------------------------------------
        if (CodeReservation.HasValue)
        {
            reservation = await _context.Reservations
                .Include(r => r.CodeExemplaireNavigation)
                .FirstOrDefaultAsync(r =>
                    r.CodeReservation == CodeReservation.Value);

            if (reservation == null)
                return await ErreurAsync("Réservation introuvable.");

            if (reservation.StatutReservation != "PRETE")
                return await ErreurAsync(
                    "Cette réservation n'est pas prête au retrait.");

            if (reservation.DateLimiteRetrait.HasValue &&
                reservation.DateLimiteRetrait.Value < DateTime.Now)
            {
                return await ErreurAsync(
                    "Le délai de retrait de cette réservation est dépassé.");
            }

            if (!reservation.CodeExemplaire.HasValue ||
                reservation.CodeExemplaireNavigation == null)
            {
                return await ErreurAsync(
                    "Aucun exemplaire n'est attribué à cette réservation.");
            }

            // Le lecteur est déterminé par la réservation.
            lecteur = await _context.Lecteurs
                .FirstOrDefaultAsync(l =>
                    l.CodeLecteur == reservation.CodeLecteur);

            if (lecteur == null)
                return await ErreurAsync(
                    "Le lecteur associé à la réservation est introuvable.");

            // Recharger l'exemplaire avec son document et le type du document.
            exemplaire = await _context.Exemplaires
                .Include(e => e.CodeDocumentNavigation)
                    .ThenInclude(d => d.CodeTypeDocumentNavigation)
                .FirstOrDefaultAsync(e =>
                    e.CodeExemplaire == reservation.CodeExemplaire.Value);

            if (exemplaire == null)
                return await ErreurAsync(
                    "L'exemplaire réservé est introuvable.");

            if (reservation.CodeDocument != exemplaire.CodeDocument)
                return await ErreurAsync(
                    "L'exemplaire attribué ne correspond pas au document réservé.");

            if (exemplaire.StatutExemplaire != "RESERVE")
                return await ErreurAsync(
                    "L'exemplaire réservé n'a plus le statut RESERVE.");

            // Les valeurs transmises par le formulaire ne font pas foi.
            CodeLecteur = lecteur.CodeLecteur;
            CodeExemplaire = exemplaire.CodeExemplaire;
        }
        else
        {
            // -----------------------------------------------------
            // 2. Prêt direct sans réservation
            // -----------------------------------------------------
            lecteur = await _context.Lecteurs
                .FirstOrDefaultAsync(l =>
                    l.CodeLecteur == CodeLecteur);

            if (lecteur == null)
                return await ErreurAsync(
                    "Le lecteur sélectionné est introuvable.");

            exemplaire = await _context.Exemplaires
                .Include(e => e.CodeDocumentNavigation)
                    .ThenInclude(d => d.CodeTypeDocumentNavigation)
                .FirstOrDefaultAsync(e =>
                    e.CodeExemplaire == CodeExemplaire);

            if (exemplaire == null)
                return await ErreurAsync(
                    "L'exemplaire sélectionné est introuvable.");

            if (exemplaire.StatutExemplaire != "DISPONIBLE")
                return await ErreurAsync(
                    "Un prêt direct nécessite un exemplaire disponible.");
        }

        // ---------------------------------------------------------
        // 3. Vérifications communes
        // ---------------------------------------------------------
        if (!string.Equals(
                lecteur.StatutLecteur,
                "ACTIF",
                StringComparison.OrdinalIgnoreCase))
        {
            return await ErreurAsync("Ce lecteur n'est pas actif.");
        }

        var aujourdHui = DateOnly.FromDateTime(DateTime.Today);

        if (lecteur.DateExpiration.HasValue &&
            lecteur.DateExpiration.Value < aujourdHui)
        {
            return await ErreurAsync(
                "L'inscription de ce lecteur a expiré.");
        }

        var document = exemplaire.CodeDocumentNavigation;
        var typeDocument = document.CodeTypeDocumentNavigation;

        if (!document.EmpruntAutorise || !typeDocument.EmpruntAutorise)
        {
            return await ErreurAsync(
                "Ce document ou son type n'est pas autorisé au prêt.");
        }

        if (typeDocument.DureePretJours <= 0)
        {
            return await ErreurAsync(
                "La durée de prêt de ce type de document est invalide.");
        }

        // Éviter de retirer une réservation déjà liée à un prêt.
        if (reservation != null)
        {
            var reservationDejaRetiree = await _context.LignesPrets
                .AnyAsync(l =>
                    l.CodeReservation == reservation.CodeReservation);

            if (reservationDejaRetiree)
            {
                return await ErreurAsync(
                    "Cette réservation est déjà associée à un prêt.");
            }
        }

        // ---------------------------------------------------------
        // 4. Enregistrement transactionnel
        // ---------------------------------------------------------
        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            var maintenant = DateTime.Now;

            var bordereau = new BordereauxPret
            {
                NumeroBordereau =
                    $"PRET-{maintenant:yyyyMMddHHmmssfff}",
                CodeLecteur = lecteur.CodeLecteur,
                DatePret = maintenant,
                EnregistrePar = codeCompte,
                StatutBordereau = "OUVERT",
                Observations = string.IsNullOrWhiteSpace(Observations)
                    ? null
                    : Observations.Trim()
            };

            _context.BordereauxPrets.Add(bordereau);
            await _context.SaveChangesAsync();

            var ligne = new LignesPret
            {
                CodeBordereauPret = bordereau.CodeBordereauPret,
                CodeExemplaire = exemplaire.CodeExemplaire,
                CodeReservation = reservation?.CodeReservation,
                DateRetrait = maintenant,
                DateEcheance = DateOnly
                    .FromDateTime(maintenant)
                    .AddDays(typeDocument.DureePretJours),
                StatutLigne = "EN_COURS",
                Observations = string.IsNullOrWhiteSpace(Observations)
                    ? null
                    : Observations.Trim()
            };

            _context.LignesPrets.Add(ligne);

            exemplaire.StatutExemplaire = "EMPRUNTE";

            if (reservation != null)
            {
                reservation.StatutReservation = "HONOREE";
                reservation.DateRetraitEffectif = maintenant;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            var message = reservation != null
                ? $"Retrait de la réservation {reservation.NumeroReservation} enregistré. Bordereau : {bordereau.NumeroBordereau}."
                : $"Prêt {bordereau.NumeroBordereau} enregistré.";

            return RedirectToPage(
                "/Emprunts/Index",
                new { message });
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();

            ModelState.AddModelError(
                string.Empty,
                "Erreur pendant l'enregistrement du prêt. Vérifiez les journaux de l'application.");

            await ChargerDonneesAsync();
            return Page();
        }
    }

    private async Task<IActionResult> ErreurAsync(string message)
    {
        ModelState.AddModelError(string.Empty, message);
        await ChargerDonneesAsync();
        return Page();
    }

    private async Task ChargerDonneesAsync()
    {
        var aujourdHui = DateOnly.FromDateTime(DateTime.Today);

        TotalEmpruntsEnCours = await _context.LignesPrets
            .AsNoTracking()
            .CountAsync(l => l.StatutLigne == "EN_COURS");

        TotalEmpruntsRetournes = await _context.LignesPrets
            .AsNoTracking()
            .CountAsync(l => l.StatutLigne == "RETOURNE");

        TotalEmpruntsEnRetard = await _context.LignesPrets
            .AsNoTracking()
            .CountAsync(l =>
                l.StatutLigne == "EN_COURS" &&
                l.DateEcheance < aujourdHui);

        Lecteurs = await _context.Lecteurs
            .AsNoTracking()
            .Include(l => l.CodeCompteNavigation)
            .Where(l => l.StatutLecteur == "ACTIF")
            .OrderBy(l => l.NumeroLecteur)
            .ToListAsync();

        ExemplairesDisponibles = await _context.Exemplaires
            .AsNoTracking()
            .Include(e => e.CodeDocumentNavigation)
                .ThenInclude(d => d.CodeTypeDocumentNavigation)
            .Where(e =>
                e.StatutExemplaire == "DISPONIBLE" &&
                e.CodeDocumentNavigation.EmpruntAutorise &&
                e.CodeDocumentNavigation
                    .CodeTypeDocumentNavigation.EmpruntAutorise)
            .OrderBy(e => e.CodeDocumentNavigation.Titre)
            .ThenBy(e => e.Cote)
            .ToListAsync();

        ReservationsPretes = await _context.Reservations
            .AsNoTracking()
            .Include(r => r.CodeLecteurNavigation)
            .Include(r => r.CodeDocumentNavigation)
            .Include(r => r.CodeExemplaireNavigation)
            .Where(r =>
                r.StatutReservation == "PRETE" &&
                r.CodeExemplaire != null &&
                (r.DateLimiteRetrait == null ||
                 r.DateLimiteRetrait >= DateTime.Now))
            .OrderBy(r => r.DateLimiteRetrait)
            .ToListAsync();

        Emprunts = await _context.LignesPrets
            .AsNoTracking()
            .Include(l => l.CodeBordereauPretNavigation)
                .ThenInclude(b => b.CodeLecteurNavigation)
            .Include(l => l.CodeExemplaireNavigation)
                .ThenInclude(e => e.CodeDocumentNavigation)
            .OrderByDescending(l => l.DateRetrait)
            .ToListAsync();
    }
}