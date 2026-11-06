
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using bibliotheque_mayidi.Data.Generated;
using bibliotheque_mayidi.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace bibliotheque_mayidi.Pages.Documents;

public class CreateModel : PageModel
{
    private readonly BibliothequeScaffoldContext _context;

    public CreateModel(BibliothequeScaffoldContext context)
    {
        _context = context;
    }

    [BindProperty]
    public DocumentInput Input { get; set; } = new();

    public IList<TypesDocument> Types { get; set; } = new List<TypesDocument>();
    public IList<Editeur> Editeurs { get; set; } = new List<Editeur>();
    public IList<Collection> Collections { get; set; } = new List<Collection>();
    public IList<Rayon> Rayons { get; set; } = new List<Rayon>();
    public IList<Etagere> Etageres { get; set; } = new List<Etagere>();
    public IList<Emplacement> Emplacements { get; set; } = new List<Emplacement>();

    public class DocumentInput
    {
        [Required(ErrorMessage = "Le titre est obligatoire.")]
        [StringLength(255)]
        public string Titre { get; set; } = string.Empty;

        [StringLength(255)]
        public string? SousTitre { get; set; }

        [Required(ErrorMessage = "Sélectionnez un type de document.")]
        public string CodeTypeDocument { get; set; } = string.Empty;

        public int? CodeCollection { get; set; }
        public int? CodeEditeur { get; set; }

        [StringLength(50)]
        public string? IsbnIssn { get; set; }

        [StringLength(100)]
        public string? Edition { get; set; }

        [StringLength(150)]
        public string? LieuPublication { get; set; }

        [Range(1, 2100, ErrorMessage = "L'année doit être valide.")]
        public int? AnneePublication { get; set; }

        [StringLength(50)]
        public string? Langue { get; set; }

        [Range(1, 100000,
            ErrorMessage = "Le nombre de pages doit être positif.")]
        public int? NombrePages { get; set; }

        public string? Resume { get; set; }
        public string? MotsCles { get; set; }

        public bool EmpruntAutorise { get; set; } = true;

        [Required(ErrorMessage = "Le nom de l'auteur est obligatoire.")]
        [StringLength(150)]
        public string NomAuteur { get; set; } = string.Empty;

        [StringLength(150)]
        public string? PrenomAuteur { get; set; }

        [StringLength(100)]
        public string? DatesAuteur { get; set; }

        public string? NoticeBiographique { get; set; }

        [Required(ErrorMessage = "Sélectionnez un rayon.")]
        public string CodeRayon { get; set; } = string.Empty;

        [Range(1, int.MaxValue,
            ErrorMessage = "Sélectionnez une étagère.")]
        public int CodeEtagere { get; set; }

        [Range(1, int.MaxValue,
            ErrorMessage = "Sélectionnez une place.")]
        public int CodeEmplacement { get; set; }

        [Required(ErrorMessage = "La cote est obligatoire.")]
        [StringLength(100)]
        public string Cote { get; set; } = string.Empty;
    }

    public async Task OnGetAsync()
    {
        await ChargerListesAsync();
    }

    // Chargement des étagères du rayon choisi.
    public async Task<JsonResult> OnGetEtageresAsync(string codeRayon)
    {
        if (string.IsNullOrWhiteSpace(codeRayon))
            return new JsonResult(Array.Empty<object>());

        var etageres = await _context.Etageres
            .AsNoTracking()
            .Where(e =>
                e.Actif &&
                e.CodeRayon == codeRayon &&
                e.CodeRayonNavigation.Actif)
            .OrderBy(e => e.NumeroEtagereRoman)
            .Select(e => new
            {
                value = e.CodeEtagere,
                label = e.NumeroEtagereRoman
            })
            .ToListAsync();

        return new JsonResult(etageres);
    }

    // Chargement des places de l'étagère choisie.
    public async Task<JsonResult> OnGetEmplacementsAsync(int codeEtagere)
    {
        if (codeEtagere <= 0)
            return new JsonResult(Array.Empty<object>());

        var emplacements = await _context.Emplacements
            .AsNoTracking()
            .Where(e =>
                e.Actif &&
                e.CodeEtagere == codeEtagere &&
                e.CodeEtagereNavigation.Actif &&
                e.CodeEtagereNavigation.CodeRayonNavigation.Actif)
            .OrderBy(e => e.NumeroPlace)
            .Select(e => new
            {
                e.CodeEmplacement,
                e.NumeroPlace
            })
            .ToListAsync();

        return new JsonResult(emplacements.Select(e => new
        {
            value = e.CodeEmplacement,
            label = Convert.ToString(e.NumeroPlace)
        }));
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Recharge uniquement les listes nécessaires à l'affichage.
        await ChargerListesAsync(chargerLocalisationSelectionnee: true);

        if (!ModelState.IsValid)
            return Page();

        string? identifiantCompte =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(identifiantCompte, out int codeCompte))
            return Forbid();

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            long codeSession = await _context.Database
                .SqlQuery<long>($"""
                    SELECT code_session AS "Value"
                    FROM "SESSIONS_ENREGISTREMENT"
                    WHERE code_compte = {codeCompte}
                      AND statut_session = 'EN_COURS'
                    ORDER BY date_ouverture DESC
                    LIMIT 1
                    """)
                .SingleOrDefaultAsync();

            if (codeSession <= 0)
            {
                await transaction.RollbackAsync();

                ModelState.AddModelError(
                    string.Empty,
                    "Aucune session d'enregistrement n'est ouverte. " +
                    "Veuillez ouvrir une session avant d'ajouter un document.");

                return Page();
            }

            bool typeExiste = await _context.TypesDocuments
                .AnyAsync(t =>
                    t.CodeTypeDocument == Input.CodeTypeDocument);

            if (!typeExiste)
            {
                ModelState.AddModelError(
                    "Input.CodeTypeDocument",
                    "Le type de document sélectionné n'existe pas.");

                await transaction.RollbackAsync();
                return Page();
            }

            if (Input.CodeCollection.HasValue &&
                !await _context.Collections.AnyAsync(
                    c => c.CodeCollection == Input.CodeCollection.Value))
            {
                ModelState.AddModelError(
                    "Input.CodeCollection",
                    "La collection sélectionnée n'existe pas.");

                await transaction.RollbackAsync();
                return Page();
            }

            if (Input.CodeEditeur.HasValue &&
                !await _context.Editeurs.AnyAsync(
                    e => e.CodeEditeur == Input.CodeEditeur.Value))
            {
                ModelState.AddModelError(
                    "Input.CodeEditeur",
                    "L'éditeur sélectionné n'existe pas.");

                await transaction.RollbackAsync();
                return Page();
            }

            bool localisationValide = await _context.Emplacements
                .AnyAsync(e =>
                    e.CodeEmplacement == Input.CodeEmplacement &&
                    e.Actif &&
                    e.CodeEtagere == Input.CodeEtagere &&
                    e.CodeEtagereNavigation.CodeRayon == Input.CodeRayon &&
                    e.CodeEtagereNavigation.Actif &&
                    e.CodeEtagereNavigation.CodeRayonNavigation.Actif);

            if (!localisationValide)
            {
                ModelState.AddModelError(
                    "Input.CodeEmplacement",
                    "La localisation sélectionnée est invalide ou inactive.");

                await transaction.RollbackAsync();
                return Page();
            }

            string nomAuteur = Input.NomAuteur.Trim();
            string? prenomAuteur = Nettoyer(Input.PrenomAuteur);

            Auteur? auteur = await _context.Auteurs.FirstOrDefaultAsync(a =>
                a.NomAuteur.ToLower() == nomAuteur.ToLower() &&
                (a.PrenomAuteur ?? "").ToLower() ==
                (prenomAuteur ?? "").ToLower());

            if (auteur == null)
            {
                auteur = new Auteur
                {
                    NomAuteur = nomAuteur,
                    PrenomAuteur = prenomAuteur,
                    DatesAuteur = Nettoyer(Input.DatesAuteur),
                    NoticeBiographique =
                        Nettoyer(Input.NoticeBiographique)
                };

                _context.Auteurs.Add(auteur);
            }

            var document = new Document
            {
                Titre = Input.Titre.Trim(),
                SousTitre = Nettoyer(Input.SousTitre),
                CodeTypeDocument = Input.CodeTypeDocument,
                CodeCollection = Input.CodeCollection,
                CodeEditeur = Input.CodeEditeur,
                IsbnIssn = Nettoyer(Input.IsbnIssn),
                Edition = Nettoyer(Input.Edition),
                LieuPublication = Nettoyer(Input.LieuPublication),
                AnneePublication = Input.AnneePublication,
                Langue = Nettoyer(Input.Langue),
                NombrePages = Input.NombrePages,
                Resume = Nettoyer(Input.Resume),
                MotsCles = Nettoyer(Input.MotsCles),
                EmpruntAutorise = Input.EmpruntAutorise,
                StatutNotice = "VALIDE",
                DateEncodage = DateTime.Now
            };

            document.DocumentsAuteurs.Add(new DocumentsAuteur
            {
                CodeAuteurNavigation = auteur,
                OrdreAffichage = 1,
                RoleAuteur = "AUTEUR"
            });

            _context.Documents.Add(document);
            await _context.SaveChangesAsync();

            var exemplaire = new Exemplaire
            {
                CodeDocument = document.CodeDocument,
                CodeEmplacement = Input.CodeEmplacement,
                Cote = Input.Cote.Trim(),
                NumeroInventaire = "AUTO",
                DeviseValeur = "CDF",
                EtatDocument = "BON",
                StatutExemplaire = "DISPONIBLE",
                DateCreation = DateTime.Now,
                EncodePar = codeCompte
            };

            _context.Entry(exemplaire)
                .Property<long?>("CodeSessionEnregistrement")
                .CurrentValue = codeSession;

            _context.Exemplaires.Add(exemplaire);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            TempData["Succes"] =
                $"Le document « {document.Titre} », son auteur et son " +
                $"exemplaire ont été enregistrés. " +
                $"Cote : {exemplaire.Cote} ; " +
                $"inventaire : {exemplaire.NumeroInventaire}.";

            return RedirectToPage("./Index");
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();

            ModelState.AddModelError(
                string.Empty,
                "L'enregistrement a échoué. Aucune création partielle " +
                "n'a été conservée. Vérifiez les données et réessayez.");

            await ChargerListesAsync(chargerLocalisationSelectionnee: true);

            return Page();
        }
    }

    private async Task ChargerListesAsync(
        bool chargerLocalisationSelectionnee = false)
    {
        Types = await _context.TypesDocuments
            .AsNoTracking()
            .OrderBy(t => t.NomTypeDocument)
            .ToListAsync();

        Editeurs = await _context.Editeurs
            .AsNoTracking()
            .OrderBy(e => e.NomEditeur)
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

        Etageres = new List<Etagere>();
        Emplacements = new List<Emplacement>();

        if (!chargerLocalisationSelectionnee)
            return;

        if (!string.IsNullOrWhiteSpace(Input.CodeRayon))
        {
            Etageres = await _context.Etageres
                .AsNoTracking()
                .Where(e =>
                    e.Actif &&
                    e.CodeRayon == Input.CodeRayon &&
                    e.CodeRayonNavigation.Actif)
                .OrderBy(e => e.NumeroEtagereRoman)
                .ToListAsync();
        }

        if (Input.CodeEtagere > 0)
        {
            Emplacements = await _context.Emplacements
                .AsNoTracking()
                .Where(e =>
                    e.Actif &&
                    e.CodeEtagere == Input.CodeEtagere &&
                    e.CodeEtagereNavigation.Actif &&
                    e.CodeEtagereNavigation.CodeRayonNavigation.Actif)
                .OrderBy(e => e.NumeroPlace)
                .ToListAsync();
        }
    }

    private static string? Nettoyer(string? valeur)
    {
        return string.IsNullOrWhiteSpace(valeur)
            ? null
            : valeur.Trim();
    }
}