
using System.Data;
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

    [BindProperty]
    public int CodeLecteur { get; set; }

    [BindProperty]
    public List<int> CodesExemplaires { get; set; } = new();

    [BindProperty]
    public string? NumeroReservationSaisie { get; set; }

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

    // =========================================================
    // RECHERCHE DYNAMIQUE DES LECTEURS
    // =========================================================
    public async Task<IActionResult> OnGetRechercherLecteursAsync(string? q)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2)
            return new JsonResult(new { resultats = Array.Empty<object>() });

        q = q.Trim();

        var lecteurs = await _context.Lecteurs
            .AsNoTracking()
            .Include(l => l.CodeCompteNavigation)
            .Where(l =>
                l.StatutLecteur == "ACTIF" &&
                l.CodeCompteNavigation.Actif &&
                l.CodeCompteNavigation.StatutCompte == "ACTIF" &&
                (
                    l.NumeroLecteur.Contains(q) ||
                    (l.MatriculeSeminaire != null &&
                     l.MatriculeSeminaire.Contains(q)) ||
                    l.CodeCompteNavigation.NomUtilisateur.Contains(q) ||
                    (l.CodeCompteNavigation.Postnom != null &&
                     l.CodeCompteNavigation.Postnom.Contains(q)) ||
                    (l.CodeCompteNavigation.Prenom != null &&
                     l.CodeCompteNavigation.Prenom.Contains(q)) ||
                    l.CodeCompteNavigation.Identifiant.Contains(q)
                ))
            .OrderBy(l => l.NumeroLecteur)
            .Take(15)
            .Select(l => new
            {
                code = l.CodeLecteur,
                numero = l.NumeroLecteur,
                nom = l.CodeCompteNavigation.NomUtilisateur,
                postnom = l.CodeCompteNavigation.Postnom,
                prenom = l.CodeCompteNavigation.Prenom,
                identifiant = l.CodeCompteNavigation.Identifiant
            })
            .ToListAsync();

        return new JsonResult(new { resultats = lecteurs });
    }

    // =========================================================
    // RECHERCHE DYNAMIQUE DES OUVRAGES ET EXEMPLAIRES
    // =========================================================
    public async Task<IActionResult> OnGetRechercherOuvragesAsync(string? q)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2)
            return new JsonResult(new { resultats = Array.Empty<object>() });

        q = q.Trim();

        int.TryParse(q, out int codeDocument);

        var documents = await _context.Documents
            .AsNoTracking()
            .Include(d => d.CodeTypeDocumentNavigation)
            .Where(d =>
                d.EmpruntAutorise &&
                d.CodeTypeDocumentNavigation.EmpruntAutorise &&
                (
                    d.Titre.Contains(q) ||
                    (codeDocument > 0 && d.CodeDocument == codeDocument)
                ))
            .OrderBy(d => d.Titre)
            .Take(20)
            .Select(d => new
            {
                code = d.CodeDocument,
                titre = d.Titre,
                type = d.CodeTypeDocumentNavigation.NomTypeDocument,
                duree = d.CodeTypeDocumentNavigation.DureePretJours
            })
            .ToListAsync();

        var codesDocuments = documents.Select(d => d.code).ToList();

        var exemplaires = await _context.Exemplaires
            .AsNoTracking()
            .Where(e =>
                codesDocuments.Contains(e.CodeDocument) &&
                e.StatutExemplaire == "DISPONIBLE")
            .OrderBy(e => e.Cote)
            .Select(e => new
            {
                codeDocument = e.CodeDocument,
                code = e.CodeExemplaire,
                cote = e.Cote,
                inventaire = e.NumeroInventaire
            })
            .ToListAsync();

        var resultats = documents.Select(d =>
        {
            var copies = exemplaires
                .Where(e => e.codeDocument == d.code)
                .Select(e => new
                {
                    e.code,
                    e.cote,
                    e.inventaire
                })
                .ToList();

            return new
            {
                d.code,
                d.titre,
                d.type,
                d.duree,
                nombreDisponible = copies.Count,
                exemplaires = copies
            };
        }).ToList();

        return new JsonResult(new { resultats });
    }

    // =========================================================
    // CHARGEMENT AUTOMATIQUE D'UNE RESERVATION
    // =========================================================
    public async Task<IActionResult> OnGetChargerReservationAsync(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return new JsonResult(new
            {
                succes = false,
                message = "Saisissez un code de réservation."
            });
        }

        code = code.Trim();
        long.TryParse(code, out long numeroNumerique);

        var reservation = await _context.Reservations
            .AsNoTracking()
            .Include(r => r.CodeLecteurNavigation)
                .ThenInclude(l => l.CodeCompteNavigation)
            .FirstOrDefaultAsync(r =>
                r.NumeroReservation == code ||
                (numeroNumerique > 0 &&
                 r.CodeReservation == numeroNumerique));

        if (reservation == null)
        {
            return new JsonResult(new
            {
                succes = false,
                message = "Réservation introuvable."
            });
        }

        var maintenant = DateTime.SpecifyKind(
            DateTime.Now, DateTimeKind.Unspecified);

        if (reservation.StatutReservation != "PRETE")
        {
            return new JsonResult(new
            {
                succes = false,
                message = "Cette réservation n'est pas prête au retrait."
            });
        }

        if (reservation.DateLimiteRetrait.HasValue &&
            reservation.DateLimiteRetrait.Value < maintenant)
        {
            return new JsonResult(new
            {
                succes = false,
                message = "Le délai de retrait est dépassé."
            });
        }

        if (!reservation.CodeExemplaire.HasValue)
        {
            return new JsonResult(new
            {
                succes = false,
                message = "Aucun exemplaire n'est attribué à cette réservation."
            });
        }

        var exemplaire = await _context.Exemplaires
            .AsNoTracking()
            .Include(e => e.CodeDocumentNavigation)
                .ThenInclude(d => d.CodeTypeDocumentNavigation)
            .FirstOrDefaultAsync(e =>
                e.CodeExemplaire == reservation.CodeExemplaire.Value);

        if (exemplaire == null ||
            exemplaire.StatutExemplaire != "RESERVE" ||
            exemplaire.CodeDocument != reservation.CodeDocument)
        {
            return new JsonResult(new
            {
                succes = false,
                message = "L'exemplaire réservé n'est plus disponible."
            });
        }

        var document = exemplaire.CodeDocumentNavigation;
        var type = document.CodeTypeDocumentNavigation;
        var lecteur = reservation.CodeLecteurNavigation;

        if (!lecteur.CodeCompteNavigation.Actif ||
            lecteur.CodeCompteNavigation.StatutCompte != "ACTIF" ||
            lecteur.StatutLecteur != "ACTIF")
        {
            return new JsonResult(new
            {
                succes = false,
                message = "Le compte du lecteur n'est pas actif."
            });
        }

        if (!document.EmpruntAutorise ||
            !type.EmpruntAutorise ||
            type.DureePretJours <= 0)
        {
            return new JsonResult(new
            {
                succes = false,
                message = "Ce document ne peut pas être emprunté."
            });
        }

        var dejaRetiree = await _context.LignesPrets
            .AsNoTracking()
            .AnyAsync(l => l.CodeReservation == reservation.CodeReservation);

        if (dejaRetiree)
        {
            return new JsonResult(new
            {
                succes = false,
                message = "Cette réservation est déjà associée à un prêt."
            });
        }

        return new JsonResult(new
        {
            succes = true,
            reservation = reservation.NumeroReservation,
            codeReservation = reservation.CodeReservation,

            lecteur = new
            {
                code = lecteur.CodeLecteur,
                numero = lecteur.NumeroLecteur,
                nom = lecteur.CodeCompteNavigation.NomUtilisateur,
                postnom = lecteur.CodeCompteNavigation.Postnom,
                prenom = lecteur.CodeCompteNavigation.Prenom
            },

            ouvrage = new
            {
                code = document.CodeDocument,
                titre = document.Titre,
                type = type.NomTypeDocument,
                duree = type.DureePretJours
            },

            exemplaire = new
            {
                code = exemplaire.CodeExemplaire,
                cote = exemplaire.Cote,
                inventaire = exemplaire.NumeroInventaire
            }
        });
    }

    // =========================================================
    // ENREGISTREMENT DU BORDEREAU MULTI-LIGNES
    // =========================================================
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

        await using var transaction =
            await _context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);

        try
        {
            var maintenant = DateTime.SpecifyKind(
                DateTime.Now, DateTimeKind.Unspecified);

            Reservation? reservation = null;
            Lecteur? lecteur = null;
            List<int> codesAEmprunter;

            // -------------------------------------------------
            // CAS 1 : RETRAIT SUR RESERVATION
            // -------------------------------------------------
            if (!string.IsNullOrWhiteSpace(NumeroReservationSaisie))
            {
                var code = NumeroReservationSaisie.Trim();
                long.TryParse(code, out long numeroNumerique);

                reservation = await _context.Reservations
                    .FirstOrDefaultAsync(r =>
                        r.NumeroReservation == code ||
                        (numeroNumerique > 0 &&
                         r.CodeReservation == numeroNumerique));

                if (reservation == null)
                    return await EchecTransactionAsync(
                        transaction, "Réservation introuvable.");

                if (reservation.StatutReservation != "PRETE")
                    return await EchecTransactionAsync(
                        transaction, "Cette réservation n'est pas prête au retrait.");

                if (reservation.DateLimiteRetrait.HasValue &&
                    reservation.DateLimiteRetrait.Value < maintenant)
                    return await EchecTransactionAsync(
                        transaction, "Le délai de retrait est dépassé.");

                if (!reservation.CodeExemplaire.HasValue)
                    return await EchecTransactionAsync(
                        transaction, "Aucun exemplaire n'est attribué à cette réservation.");

                lecteur = await _context.Lecteurs
                    .Include(l => l.CodeCompteNavigation)
                    .FirstOrDefaultAsync(l =>
                        l.CodeLecteur == reservation.CodeLecteur);

                if (lecteur == null)
                    return await EchecTransactionAsync(
                        transaction, "Lecteur de la réservation introuvable.");

                codesAEmprunter = new List<int>
                {
                    reservation.CodeExemplaire.Value
                };

                CodeLecteur = lecteur.CodeLecteur;
            }
            // -------------------------------------------------
            // CAS 2 : PRET DIRECT MULTI-LIGNES
            // -------------------------------------------------
            else
            {
                if (CodesExemplaires == null ||
                    CodesExemplaires.Count == 0)
                    return await EchecTransactionAsync(
                        transaction, "Ajoutez au moins un ouvrage au bordereau.");

                if (CodesExemplaires.Distinct().Count() != CodesExemplaires.Count)
                    return await EchecTransactionAsync(
                        transaction, "Un même exemplaire ne peut pas apparaître deux fois.");

                lecteur = await _context.Lecteurs
                    .Include(l => l.CodeCompteNavigation)
                    .FirstOrDefaultAsync(l =>
                        l.CodeLecteur == CodeLecteur);

                codesAEmprunter = CodesExemplaires.Distinct().ToList();
            }

            // -------------------------------------------------
            // VERIFICATION DU LECTEUR
            // -------------------------------------------------
            if (lecteur == null ||
                lecteur.StatutLecteur != "ACTIF" ||
                !lecteur.CodeCompteNavigation.Actif ||
                lecteur.CodeCompteNavigation.StatutCompte != "ACTIF")
            {
                return await EchecTransactionAsync(
                    transaction, "Le compte du lecteur n'est pas actif.");
            }

            var aujourdHui = DateOnly.FromDateTime(maintenant);

            if (lecteur.DateExpiration.HasValue &&
                lecteur.DateExpiration.Value < aujourdHui)
            {
                return await EchecTransactionAsync(
                    transaction, "L'inscription du lecteur a expiré.");
            }

            // -------------------------------------------------
            // RECUPERATION DES EXEMPLAIRES
            // -------------------------------------------------
            var exemplaires = await _context.Exemplaires
                .Include(e => e.CodeDocumentNavigation)
                    .ThenInclude(d => d.CodeTypeDocumentNavigation)
                .Where(e => codesAEmprunter.Contains(e.CodeExemplaire))
                .ToListAsync();

            if (exemplaires.Count != codesAEmprunter.Count)
            {
                return await EchecTransactionAsync(
                    transaction, "Un ou plusieurs exemplaires sont introuvables.");
            }

            foreach (var exemplaire in exemplaires)
            {
                var statutAttendu = reservation != null
                    ? "RESERVE"
                    : "DISPONIBLE";

                if (exemplaire.StatutExemplaire != statutAttendu)
                {
                    return await EchecTransactionAsync(
                        transaction,
                        $"L'exemplaire {exemplaire.Cote} n'est plus disponible.");
                }

                var document = exemplaire.CodeDocumentNavigation;
                var type = document.CodeTypeDocumentNavigation;

                if (!document.EmpruntAutorise || !type.EmpruntAutorise)
                {
                    return await EchecTransactionAsync(
                        transaction,
                        $"Le document « {document.Titre} » n'est pas autorisé au prêt.");
                }

                if (type.DureePretJours <= 0)
                {
                    return await EchecTransactionAsync(
                        transaction,
                        $"La durée de prêt de « {document.Titre} » est invalide.");
                }
            }

            // -------------------------------------------------
            // VERIFICATION DE LA RESERVATION
            // -------------------------------------------------
            if (reservation != null)
            {
                var dejaRetiree = await _context.LignesPrets
                    .AnyAsync(l =>
                        l.CodeReservation == reservation.CodeReservation);

                if (dejaRetiree)
                {
                    return await EchecTransactionAsync(
                        transaction, "Cette réservation est déjà associée à un prêt.");
                }
            }

            // -------------------------------------------------
            // CREATION DU BORDEREAU
            // -------------------------------------------------
            var bordereau = new BordereauxPret
            {
                NumeroBordereau = $"PRET-{maintenant:yyyyMMddHHmmssfff}",
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

            // -------------------------------------------------
            // CREATION DES LIGNES DE PRET
            // -------------------------------------------------
            foreach (var exemplaire in exemplaires)
            {
                var duree = exemplaire.CodeDocumentNavigation
                    .CodeTypeDocumentNavigation.DureePretJours;

                var ligne = new LignesPret
                {
                    CodeBordereauPret = bordereau.CodeBordereauPret,
                    CodeExemplaire = exemplaire.CodeExemplaire,
                    CodeReservation = reservation?.CodeReservation,
                    DateRetrait = maintenant,
                    DateEcheance = aujourdHui.AddDays(duree),
                    StatutLigne = "EN_COURS",
                    Observations = string.IsNullOrWhiteSpace(Observations)
                        ? null
                        : Observations.Trim()
                };

                _context.LignesPrets.Add(ligne);
                exemplaire.StatutExemplaire = "EMPRUNTE";
            }

            if (reservation != null)
            {
                reservation.StatutReservation = "HONOREE";
                reservation.DateRetraitEffectif = maintenant;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return RedirectToPage("/Emprunts/Index", new
            {
                message = $"Bordereau {bordereau.NumeroBordereau} enregistré avec {exemplaires.Count} ouvrage(s)."
            });
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();

            ModelState.AddModelError(
                string.Empty,
                "Erreur pendant l'enregistrement. Consultez les journaux de l'application.");

            await ChargerDonneesAsync();
            return Page();
        }
    }

    // =========================================================
    // GESTION DES ERREURS DE TRANSACTION
    // =========================================================
    private async Task<IActionResult> EchecTransactionAsync(
        Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction,
        string message)
    {
        await transaction.RollbackAsync();

        ModelState.AddModelError(string.Empty, message);

        await ChargerDonneesAsync();

        return Page();
    }

    // =========================================================
    // CHARGEMENT DES STATISTIQUES ET DE L'HISTORIQUE
    // =========================================================
    private async Task ChargerDonneesAsync()
    {
        var aujourdHui = DateOnly.FromDateTime(DateTime.Today);

        TotalEmpruntsEnCours = await _context.LignesPrets
            .CountAsync(l => l.StatutLigne == "EN_COURS");

        TotalEmpruntsRetournes = await _context.LignesPrets
            .CountAsync(l => l.StatutLigne == "RETOURNE");

        TotalEmpruntsEnRetard = await _context.LignesPrets
            .CountAsync(l =>
                l.StatutLigne == "EN_COURS" &&
                l.DateEcheance < aujourdHui);

        Emprunts = await _context.LignesPrets
            .AsNoTracking()
            .Include(l => l.CodeBordereauPretNavigation)
                .ThenInclude(b => b.CodeLecteurNavigation)
            .Include(l => l.CodeExemplaireNavigation)
                .ThenInclude(e => e.CodeDocumentNavigation)
            .OrderByDescending(l => l.DateRetrait)
            .Take(300)
            .ToListAsync();
    }
}
