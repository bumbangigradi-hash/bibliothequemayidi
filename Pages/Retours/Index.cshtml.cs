
using System.Security.Claims;
using bibliotheque_mayidi.Data.Generated;
using bibliotheque_mayidi.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace bibliotheque_mayidi.Pages.Retours;

[Authorize(Roles = "BIBLIOTHECAIRE,ADMINISTRATEUR,RESPONSABLE,VICE_RESPONSABLE")]
public class IndexModel : PageModel
{
    private readonly BibliothequeScaffoldContext _context;

    public IndexModel(BibliothequeScaffoldContext context)
    {
        _context = context;
    }

    public List<LignesPret> EmpruntsEnCours { get; private set; } = new();

    public List<BordereauxRetour> RetoursRecents { get; private set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? Recherche { get; set; }

    [BindProperty]
    public long CodeLignePret { get; set; }

    [BindProperty]
    public string EtatRetour { get; set; } = "BON";

    [BindProperty]
    public string StatutApresControle { get; set; } = "DISPONIBLE";

    [BindProperty]
    public string? Observations { get; set; }

    [BindProperty]
    public string? ObservationsBordereau { get; set; }

    public string? MessageSucces { get; private set; }

    public async Task OnGetAsync(string? message = null)
    {
        MessageSucces = message;
        await ChargerDonneesAsync();
    }

    public async Task<IActionResult> OnPostEnregistrerAsync()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(claim, out var codeCompte))
            return Forbid();

        var compteExiste = await _context.ComptesUtilisateurs
            .AnyAsync(c => c.CodeCompte == codeCompte);

        if (!compteExiste)
            return Forbid();

        var etatsAutorises = new[]
        {
            "BON", "ABIME", "DEGRADE", "INCOMPLET"
        };

        var statutsAutorises = new[]
        {
            "DISPONIBLE", "EN_REPARATION", "RETIRE", "A_VERIFIER"
        };

        if (!etatsAutorises.Contains(EtatRetour) ||
            !statutsAutorises.Contains(StatutApresControle))
        {
            ModelState.AddModelError(
                string.Empty,
                "L'état ou le statut choisi n'est pas autorisé.");

            await ChargerDonneesAsync();
            return Page();
        }

        if (CodeLignePret <= 0)
        {
            ModelState.AddModelError(
                string.Empty,
                "La ligne de prêt sélectionnée est invalide.");

            await ChargerDonneesAsync();
            return Page();
        }

        // Règles de contrôle : un livre abîmé ou incomplet
        // ne doit pas être remis directement à disposition.
        if (EtatRetour is "ABIME" or "DEGRADE" &&
            StatutApresControle == "DISPONIBLE")
        {
            ModelState.AddModelError(
                string.Empty,
                "Un livre abîmé ou dégradé doit être mis en réparation, " +
                "retiré ou placé en vérification.");

            await ChargerDonneesAsync();
            return Page();
        }

        if (EtatRetour == "INCOMPLET" &&
            StatutApresControle == "DISPONIBLE")
        {
            ModelState.AddModelError(
                string.Empty,
                "Un livre incomplet doit être placé en vérification, " +
                "en réparation ou retiré.");

            await ChargerDonneesAsync();
            return Page();
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            // Lecture sans suivi : la ligne sera ensuite mise à jour
            // uniquement si elle est toujours EN_COURS.
            var ligne = await _context.LignesPrets
                .AsNoTracking()
                .Include(l => l.CodeBordereauPretNavigation)
                    .ThenInclude(b => b.CodeLecteurNavigation)
                .Include(l => l.CodeExemplaireNavigation)
                    .ThenInclude(e => e.CodeDocumentNavigation)
                .FirstOrDefaultAsync(l =>
                    l.CodeLignePret == CodeLignePret);

            if (ligne == null)
            {
                await transaction.RollbackAsync();

                ModelState.AddModelError(
                    string.Empty,
                    "La ligne de prêt est introuvable.");

                await ChargerDonneesAsync();
                return Page();
            }

            if (ligne.StatutLigne != "EN_COURS")
            {
                await transaction.RollbackAsync();

                ModelState.AddModelError(
                    string.Empty,
                    "Ce livre a déjà été retourné ou son prêt n'est plus actif.");

                await ChargerDonneesAsync();
                return Page();
            }

            var bordereauPret = ligne.CodeBordereauPretNavigation;

            var maintenant = DateTime.Now;

            // Mise à jour conditionnelle pour empêcher le double retour.
            var lignesModifiees = await _context.LignesPrets
                .Where(l =>
                    l.CodeLignePret == CodeLignePret &&
                    l.StatutLigne == "EN_COURS")
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(l => l.StatutLigne, "RETOURNE"));

            if (lignesModifiees != 1)
            {
                await transaction.RollbackAsync();

                ModelState.AddModelError(
                    string.Empty,
                    "Ce prêt vient d'être modifié. Recharge la page et réessaie.");

                await ChargerDonneesAsync();
                return Page();
            }

            // Enregistrement du bordereau de retour.
            var bordereauRetour = new BordereauxRetour
            {
                NumeroBordereau = $"RET-{maintenant:yyyyMMddHHmmssfff}",
                CodeLecteur = bordereauPret.CodeLecteur,
                DateRetour = maintenant,
                EnregistrePar = codeCompte,
                Observations = string.IsNullOrWhiteSpace(ObservationsBordereau)
                    ? null
                    : ObservationsBordereau.Trim()
            };

            _context.BordereauxRetours.Add(bordereauRetour);
            await _context.SaveChangesAsync();

            // Détail du retour.
            var ligneRetour = new LignesRetour
            {
                CodeBordereauRetour = bordereauRetour.CodeBordereauRetour,
                CodeLignePret = ligne.CodeLignePret,
                EtatRetour = EtatRetour,
                StatutApresControle = StatutApresControle,
                Observations = string.IsNullOrWhiteSpace(Observations)
                    ? null
                    : Observations.Trim()
            };

            _context.LignesRetours.Add(ligneRetour);
            await _context.SaveChangesAsync();

            // Mise à jour du statut de l'exemplaire.
            var exemplairesModifies = await _context.Exemplaires
                .Where(e =>
                    e.CodeExemplaire == ligne.CodeExemplaire &&
                    e.StatutExemplaire == "EMPRUNTE")
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(
                        e => e.StatutExemplaire,
                        StatutApresControle));

            if (exemplairesModifies != 1)
            {
                throw new InvalidOperationException(
                    "Le statut de l'exemplaire a changé ou n'est plus EMPRUNTE.");
            }

            // Recalcul du statut du bordereau de prêt.
            var resteEnCours = await _context.LignesPrets
                .AnyAsync(l =>
                    l.CodeBordereauPret == bordereauPret.CodeBordereauPret &&
                    l.StatutLigne == "EN_COURS");

            var nouveauStatut = resteEnCours
                ? "PARTIELLEMENT_RETOURNE"
                : "CLOTURE";

            await _context.BordereauxPrets
                .Where(b =>
                    b.CodeBordereauPret == bordereauPret.CodeBordereauPret)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(b => b.StatutBordereau, nouveauStatut));

            await transaction.CommitAsync();

            return RedirectToPage("/Retours/Index", new
            {
                message = $"Retour enregistré : {bordereauRetour.NumeroBordereau}."
            });
        }
        catch
        {
            await transaction.RollbackAsync();

            ModelState.AddModelError(
                string.Empty,
                "Le retour n'a pas pu être enregistré. " +
                "Aucune modification de cette opération n'a été conservée. " +
                "Vérifie les journaux de l'application.");

            await ChargerDonneesAsync();
            return Page();
        }
    }

    private async Task ChargerDonneesAsync()
    {
        var requete = _context.LignesPrets
            .AsNoTracking()
            .Include(l => l.CodeBordereauPretNavigation)
                .ThenInclude(b => b.CodeLecteurNavigation)
            .Include(l => l.CodeExemplaireNavigation)
                .ThenInclude(e => e.CodeDocumentNavigation)
            .Where(l => l.StatutLigne == "EN_COURS");

        if (!string.IsNullOrWhiteSpace(Recherche))
        {
            var terme = Recherche.Trim();

            requete = requete.Where(l =>
                l.CodeBordereauPretNavigation.NumeroBordereau.Contains(terme) ||
                l.CodeBordereauPretNavigation.CodeLecteurNavigation.NumeroLecteur
                    .Contains(terme) ||
                l.CodeExemplaireNavigation.CodeDocumentNavigation.Titre
                    .Contains(terme) ||
                l.CodeExemplaireNavigation.Cote.Contains(terme) ||
                l.CodeExemplaireNavigation.NumeroInventaire.Contains(terme));
        }

        EmpruntsEnCours = await requete
            .OrderBy(l => l.DateEcheance)
            .ThenBy(l => l.CodeBordereauPret)
            .ToListAsync();

        RetoursRecents = await _context.BordereauxRetours
            .AsNoTracking()
            .Include(r => r.CodeLecteurNavigation)
            .Include(r => r.LignesRetours)
            .OrderByDescending(r => r.DateRetour)
            .Take(20)
            .ToListAsync();
    }
}
