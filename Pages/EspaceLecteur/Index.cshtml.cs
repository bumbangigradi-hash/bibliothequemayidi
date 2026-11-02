
using System.Security.Claims;
using bibliotheque_mayidi.Data.Generated;
using bibliotheque_mayidi.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace bibliotheque_mayidi.Pages.EspaceLecteur;

[Authorize]
public class IndexModel : PageModel
{
    private readonly BibliothequeScaffoldContext _context;

    public IndexModel(BibliothequeScaffoldContext context)
    {
        _context = context;
    }

    public string NomLecteur { get; private set; } = "Lecteur";

    public string NumeroLecteur { get; private set; } = "";

    public string StatutLecteur { get; private set; } = "";

    public int EmpruntsEnCours { get; private set; }

    public int EmpruntsEnRetard { get; private set; }

    public int ReservationsEnAttente { get; private set; }

    public int DocumentsAPrendre { get; private set; }

    public int NotificationsNonLues { get; private set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var identifiantCompte = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(identifiantCompte, out int codeCompte))
        {
            return Forbid();
        }

        var compte = await _context.ComptesUtilisateurs
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CodeCompte == codeCompte);

        if (compte == null)
        {
            return Forbid();
        }

        NomLecteur = string.Join(" ",
            new[]
            {
                compte.Prenom,
                compte.NomUtilisateur,
                compte.Postnom
            }.Where(n => !string.IsNullOrWhiteSpace(n)));

        if (string.IsNullOrWhiteSpace(NomLecteur))
        {
            NomLecteur = compte.Identifiant;
        }

        NotificationsNonLues = await _context.DestinatairesMessages
            .AsNoTracking()
            .CountAsync(d =>
                d.CodeCompte == codeCompte &&
                d.DateLecture == null &&
                !d.Archive &&
                !d.SupprimeAffichage);

        var lecteur = await _context.Lecteurs
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.CodeCompte == codeCompte);

        if (lecteur == null)
        {
            return Page();
        }

        NumeroLecteur = lecteur.NumeroLecteur;
        StatutLecteur = lecteur.StatutLecteur;

        var lignes = _context.LignesPrets
            .AsNoTracking()
            .Where(l =>
                l.CodeBordereauPretNavigation.CodeLecteur ==
                lecteur.CodeLecteur);

        var aujourdHui = DateOnly.FromDateTime(DateTime.Today);

        EmpruntsEnCours = await lignes.CountAsync(l =>
            l.StatutLigne == "EN_COURS");

        EmpruntsEnRetard = await lignes.CountAsync(l =>
            l.StatutLigne == "EN_COURS" &&
            l.DateEcheance < aujourdHui);

        var reservations = _context.Reservations
            .AsNoTracking()
            .Where(r => r.CodeLecteur == lecteur.CodeLecteur);

        ReservationsEnAttente = await reservations.CountAsync(r =>
            r.StatutReservation == "EN_ATTENTE" ||
            r.StatutReservation == "EN_VERIFICATION");

        DocumentsAPrendre = await reservations.CountAsync(r =>
            r.StatutReservation == "PRETE");

        return Page();
    }
}
