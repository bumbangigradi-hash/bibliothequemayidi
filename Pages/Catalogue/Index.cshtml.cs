
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using bibliotheque_mayidi.Data.Generated;
using bibliotheque_mayidi.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace bibliotheque_mayidi.Pages.Catalogue;

public class IndexModel : PageModel
{
    private readonly BibliothequeScaffoldContext _context;

    public IndexModel(BibliothequeScaffoldContext context)
    {
        _context = context;
    }

    [BindProperty(SupportsGet = true)]
    public string? Recherche { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? TypeSelectionne { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? CollectionSelectionnee { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? RayonSelectionne { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? DisponibiliteSelectionnee { get; set; }

    public IList<Document> Documents { get; private set; }
        = new List<Document>();

    public IList<TypesDocument> Types { get; private set; }
        = new List<TypesDocument>();

    public IList<Collection> Collections { get; private set; }
        = new List<Collection>();

    public IList<Rayon> Rayons { get; private set; }
        = new List<Rayon>();

    public async Task OnGetAsync()
    {
        Types = await _context.TypesDocuments
            .AsNoTracking()
            .OrderBy(t => t.NomTypeDocument)
            .ToListAsync();

        Collections = await _context.Collections
            .AsNoTracking()
            .OrderBy(c => c.NomCollection)
            .ToListAsync();

        Rayons = await _context.Rayons
            .AsNoTracking()
            .Where(r => r.Actif)
            .OrderBy(r => r.NomRayon)
            .ToListAsync();

        // Le catalogue accepte les notices ACTIF et PUBLIE.
        IQueryable<Document> requete = _context.Documents
            .AsNoTracking()
            .Where(d =>
                d.StatutNotice == "ACTIF" ||
                d.StatutNotice == "PUBLIE")
            .Include(d => d.CodeTypeDocumentNavigation)
            .Include(d => d.CodeCollectionNavigation)
            .Include(d => d.DocumentsAuteurs)
                .ThenInclude(da => da.CodeAuteurNavigation)
            .Include(d => d.Exemplaires)
                .ThenInclude(e => e.CodeEmplacementNavigation)
                    .ThenInclude(em => em.CodeEtagereNavigation)
                        .ThenInclude(et => et.CodeRayonNavigation);

        // Recherche par titre, sous-titre, ISBN, mots-clés ou auteur.
        if (!string.IsNullOrWhiteSpace(Recherche))
        {
            string terme = Recherche.Trim();

            requete = requete.Where(d =>
                d.Titre.Contains(terme) ||
                (d.SousTitre != null &&
                 d.SousTitre.Contains(terme)) ||
                (d.IsbnIssn != null &&
                 d.IsbnIssn.Contains(terme)) ||
                (d.MotsCles != null &&
                 d.MotsCles.Contains(terme)) ||
                d.DocumentsAuteurs.Any(da =>
                    da.CodeAuteurNavigation.NomAuteur.Contains(terme) ||
                    (da.CodeAuteurNavigation.PrenomAuteur != null &&
                     da.CodeAuteurNavigation.PrenomAuteur.Contains(terme))
                )
            );
        }

        // Filtre par type de document.
        if (!string.IsNullOrWhiteSpace(TypeSelectionne))
        {
            requete = requete.Where(d =>
                d.CodeTypeDocument == TypeSelectionne);
        }

        // Filtre par collection.
        if (!string.IsNullOrWhiteSpace(CollectionSelectionnee) &&
            int.TryParse(CollectionSelectionnee, out int codeCollection))
        {
            requete = requete.Where(d =>
                d.CodeCollection == codeCollection);
        }

        // Filtre par rayon.
        if (!string.IsNullOrWhiteSpace(RayonSelectionne))
        {
            requete = requete.Where(d =>
                d.Exemplaires.Any(e =>
                    e.DateRetrait == null &&
                    e.CodeEmplacementNavigation.Actif &&
                    e.CodeEmplacementNavigation.CodeEtagereNavigation.Actif &&
                    e.CodeEmplacementNavigation.CodeEtagereNavigation.CodeRayon
                        == RayonSelectionne
                )
            );
        }

        // Filtre par disponibilité.
        if (DisponibiliteSelectionnee == "disponible")
        {
            requete = requete.Where(d =>
                d.EmpruntAutorise &&
                d.CodeTypeDocumentNavigation.EmpruntAutorise &&
                d.Exemplaires.Any(e =>
                    e.DateRetrait == null &&
                    e.StatutExemplaire == "DISPONIBLE" &&
                    e.CodeEmplacementNavigation.Actif &&
                    e.CodeEmplacementNavigation.CodeEtagereNavigation.Actif &&
                    e.CodeEmplacementNavigation.CodeEtagereNavigation
                        .CodeRayonNavigation.Actif
                )
            );
        }
        else if (DisponibiliteSelectionnee == "emprunte")
        {
            requete = requete.Where(d =>
                d.Exemplaires.Any(e =>
                    e.DateRetrait == null &&
                    e.StatutExemplaire == "EMPRUNTE"
                )
            );
        }
        else if (DisponibiliteSelectionnee == "consultation")
        {
            requete = requete.Where(d =>
                d.Exemplaires.Any(e =>
                    e.DateRetrait == null &&
                    e.StatutExemplaire == "EN_CONSULTATION"
                )
            );
        }
        else if (DisponibiliteSelectionnee == "indisponible")
        {
            requete = requete.Where(d =>
                !d.EmpruntAutorise ||
                !d.CodeTypeDocumentNavigation.EmpruntAutorise ||
                !d.Exemplaires.Any(e =>
                    e.DateRetrait == null &&
                    e.StatutExemplaire == "DISPONIBLE" &&
                    e.CodeEmplacementNavigation.Actif &&
                    e.CodeEmplacementNavigation.CodeEtagereNavigation.Actif &&
                    e.CodeEmplacementNavigation.CodeEtagereNavigation
                        .CodeRayonNavigation.Actif
                )
            );
        }

        Documents = await requete
            .OrderBy(d => d.Titre)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostReserverAsync(int codeDocument)
    {
        // Une réservation nécessite une connexion.
        if (User.Identity?.IsAuthenticated != true)
        {
            return RedirectToPage("/Connexion", new
            {
                returnUrl = "/Catalogue/Index"
            });
        }

        // Récupération du compte depuis la session authentifiée.
        var identifiantCompte = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(identifiantCompte, out int codeCompte))
        {
            return Forbid();
        }

        var compte = await _context.ComptesUtilisateurs
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CodeCompte == codeCompte);

        if (compte == null ||
            !compte.Actif ||
            compte.StatutCompte != "ACTIF")
        {
            TempData["ErreurReservation"] =
                "Votre compte n'est pas actif. Contactez la bibliothèque.";

            return RetourCatalogue();
        }

        var lecteur = await _context.Lecteurs
            .AsNoTracking()
            .FirstOrDefaultAsync(l =>
                l.CodeCompte == codeCompte &&
                l.StatutLecteur == "ACTIF");

        if (lecteur == null)
        {
            TempData["ErreurReservation"] =
                "Votre compte n'est pas associé à un lecteur actif. " +
                "Contactez la bibliothèque.";

            return RetourCatalogue();
        }

        // Accepte les notices ACTIF et PUBLIE.
        var document = await _context.Documents
            .AsNoTracking()
            .FirstOrDefaultAsync(d =>
                d.CodeDocument == codeDocument &&
                (d.StatutNotice == "ACTIF" ||
                 d.StatutNotice == "PUBLIE"));

        if (document == null)
        {
            TempData["ErreurReservation"] =
                "Ce document n'existe pas ou n'est plus accessible.";

            return RetourCatalogue();
        }

        // Vérifie la disponibilité d'au moins un exemplaire
        // dans un emplacement actif.
        bool exemplaireDisponible = await _context.Exemplaires
            .AnyAsync(e =>
                e.CodeDocument == codeDocument &&
                e.DateRetrait == null &&
                e.StatutExemplaire == "DISPONIBLE" &&
                e.CodeEmplacementNavigation.Actif &&
                e.CodeEmplacementNavigation.CodeEtagereNavigation.Actif &&
                e.CodeEmplacementNavigation.CodeEtagereNavigation
                    .CodeRayonNavigation.Actif);

        if (!document.EmpruntAutorise || !exemplaireDisponible)
        {
            TempData["ErreurReservation"] =
                "Aucun exemplaire empruntable n'est actuellement disponible.";

            return RetourCatalogue();
        }

        var typeDocument = await _context.TypesDocuments
            .AsNoTracking()
            .FirstOrDefaultAsync(t =>
                t.CodeTypeDocument == document.CodeTypeDocument);

        if (typeDocument == null || !typeDocument.EmpruntAutorise)
        {
            TempData["ErreurReservation"] =
                "Ce type de document est réservé à la consultation sur place.";

            return RetourCatalogue();
        }

        // Empêche de réserver deux fois le même ouvrage
        // tant qu'une réservation précédente est active.
        bool reservationExistante = await _context.Reservations
            .AsNoTracking()
            .AnyAsync(r =>
                r.CodeLecteur == lecteur.CodeLecteur &&
                r.CodeDocument == codeDocument &&
                (r.StatutReservation == "EN_ATTENTE" ||
                 r.StatutReservation == "EN_VERIFICATION" ||
                 r.StatutReservation == "PRETE"));

        if (reservationExistante)
        {
            TempData["ErreurReservation"] =
                "Vous avez déjà une réservation active pour ce document. " +
                "Consultez Mes réservations.";

            return RetourCatalogue();
        }

        // Réutilise le numéro du dossier actif le plus récent
        // de ce lecteur, même si plusieurs ouvrages y figurent.
        string? numeroReservationCommun = await _context.Reservations
            .AsNoTracking()
            .Where(r =>
                r.CodeLecteur == lecteur.CodeLecteur &&
                (r.StatutReservation == "EN_ATTENTE" ||
                 r.StatutReservation == "EN_VERIFICATION" ||
                 r.StatutReservation == "PRETE"))
            .OrderByDescending(r => r.DateReservation)
            .Select(r => r.NumeroReservation)
            .FirstOrDefaultAsync();

        // Si le lecteur n'a aucune réservation active,
        // on crée un nouveau numéro.
        if (string.IsNullOrWhiteSpace(numeroReservationCommun))
        {
            numeroReservationCommun = GenererNumeroReservation();
        }

        var reservation = new Reservation
        {
            NumeroReservation = numeroReservationCommun,
            CodeLecteur = lecteur.CodeLecteur,
            CodeDocument = codeDocument,
            CodeExemplaire = null,
            DateReservation = DateTime.Now,
            StatutReservation = "EN_ATTENTE"
        };

        _context.Reservations.Add(reservation);

        try
        {
            await _context.SaveChangesAsync();

            TempData["ConfirmationReservation"] =
                $"Votre ouvrage a été ajouté à la réservation " +
                $"{numeroReservationCommun}. " +
                "Vos ouvrages réservés actifs utilisent ce même numéro.";
        }
        catch (DbUpdateException)
        {
            TempData["ErreurReservation"] =
                "La réservation n'a pas pu être enregistrée. Réessayez. " +
                "Si le problème persiste, contactez la bibliothèque.";
        }

        return RetourCatalogue();
    }

    private string GenererNumeroReservation()
    {
        return $"RES-{DateTime.Now:yyyyMMdd}-" +
               $"{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
    }

    private IActionResult RetourCatalogue()
    {
        return RedirectToPage("/Catalogue/Index", new
        {
            Recherche,
            TypeSelectionne,
            CollectionSelectionnee,
            RayonSelectionne,
            DisponibiliteSelectionnee
        });
    }
}
