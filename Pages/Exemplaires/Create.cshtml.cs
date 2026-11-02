
using System.ComponentModel.DataAnnotations;
using bibliotheque_mayidi.Data.Generated;
using bibliotheque_mayidi.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace bibliotheque_mayidi.Pages.Exemplaires;

public class CreateModel : PageModel
{
    private readonly BibliothequeScaffoldContext _context;

    public CreateModel(BibliothequeScaffoldContext context)
    {
        _context = context;
    }

    [BindProperty]
    public ExemplaireInput Input { get; set; } = new();

    public IList<Document> Documents { get; set; } = new List<Document>();

    public IList<Emplacement> Emplacements { get; set; } =
        new List<Emplacement>();

    public class ExemplaireInput
    {
        [Required(ErrorMessage = "Veuillez sélectionner un document.")]
        public int? CodeDocument { get; set; }

        [Required(ErrorMessage = "Veuillez sélectionner un emplacement.")]
        public int? CodeEmplacement { get; set; }

        [Required(ErrorMessage = "Le numéro d'inventaire est obligatoire.")]
        [StringLength(100)]
        public string NumeroInventaire { get; set; } = string.Empty;

        [StringLength(100)]
        public string? CodeBarres { get; set; }

        [Required(ErrorMessage = "Veuillez préciser l'état du document.")]
        public string EtatDocument { get; set; } = "BON";

        // DateOnly? correspond au type PostgreSQL date.
        public DateOnly? DateAcquisition { get; set; }

        [StringLength(100)]
        public string? ModeAcquisition { get; set; }

        [Range(typeof(decimal), "0", "999999999999",
            ErrorMessage = "La valeur d'acquisition ne peut pas être négative.")]
        public decimal? ValeurAcquisition { get; set; }

        [Required]
        public string DeviseValeur { get; set; } = "CDF";

        public string? Observations { get; set; }
    }

    public async Task OnGetAsync()
    {
        await ChargerListesAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Input.NumeroInventaire = Input.NumeroInventaire.Trim();

        if (!string.IsNullOrWhiteSpace(Input.CodeBarres))
        {
            Input.CodeBarres = Input.CodeBarres.Trim();
        }

        await ChargerListesAsync();

        if (Input.CodeDocument.HasValue &&
            !await _context.Documents.AnyAsync(
                d => d.CodeDocument == Input.CodeDocument.Value))
        {
            ModelState.AddModelError(
                "Input.CodeDocument",
                "Le document sélectionné n'existe pas.");
        }

        if (Input.CodeEmplacement.HasValue &&
            !await _context.Emplacements.AnyAsync(e =>
                e.CodeEmplacement == Input.CodeEmplacement.Value &&
                e.Actif))
        {
            ModelState.AddModelError(
                "Input.CodeEmplacement",
                "L'emplacement sélectionné n'existe pas ou est inactif.");
        }

        if (!string.IsNullOrWhiteSpace(Input.NumeroInventaire) &&
            await _context.Exemplaires.AnyAsync(e =>
                e.NumeroInventaire == Input.NumeroInventaire))
        {
            ModelState.AddModelError(
                "Input.NumeroInventaire",
                "Ce numéro d'inventaire existe déjà.");
        }

        if (!string.IsNullOrWhiteSpace(Input.CodeBarres) &&
            await _context.Exemplaires.AnyAsync(e =>
                e.CodeBarres == Input.CodeBarres))
        {
            ModelState.AddModelError(
                "Input.CodeBarres",
                "Ce code-barres est déjà utilisé.");
        }

        if (Input.ValeurAcquisition.HasValue &&
            Input.ValeurAcquisition.Value < 0)
        {
            ModelState.AddModelError(
                "Input.ValeurAcquisition",
                "La valeur ne peut pas être négative.");
        }

        if (Input.DateAcquisition.HasValue &&
            Input.DateAcquisition.Value > DateOnly.FromDateTime(DateTime.Today))
        {
            ModelState.AddModelError(
                "Input.DateAcquisition",
                "La date d'acquisition ne peut pas être dans le futur.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var emplacement = await _context.Emplacements
            .Include(e => e.CodeEtagereNavigation)
                .ThenInclude(et => et.CodeRayonNavigation)
            .FirstOrDefaultAsync(e =>
                e.CodeEmplacement == Input.CodeEmplacement!.Value &&
                e.Actif);

        if (emplacement == null)
        {
            ModelState.AddModelError(
                "Input.CodeEmplacement",
                "Impossible de retrouver cet emplacement.");

            return Page();
        }

        var etagere = emplacement.CodeEtagereNavigation;
        var rayon = etagere.CodeRayonNavigation;

        // Exemple : PHILOSOPHIE X/1
        string cote =
            $"{rayon.NomRayon} {etagere.NumeroEtagereRoman}/{emplacement.NumeroPlace}";

        var exemplaire = new Exemplaire
        {
            CodeDocument = Input.CodeDocument!.Value,
            CodeEmplacement = Input.CodeEmplacement!.Value,
            Cote = cote,
            NumeroInventaire = Input.NumeroInventaire,
            CodeBarres = string.IsNullOrWhiteSpace(Input.CodeBarres)
                ? null
                : Input.CodeBarres,
            DateAcquisition = Input.DateAcquisition,
            ModeAcquisition = Input.ModeAcquisition,
            ValeurAcquisition = Input.ValeurAcquisition,
            DeviseValeur = Input.DeviseValeur,
            EtatDocument = Input.EtatDocument,
            StatutExemplaire = "DISPONIBLE",
            Observations = Input.Observations,
            DateCreation = DateTime.Now
        };

        _context.Exemplaires.Add(exemplaire);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(
                string.Empty,
                "L'enregistrement a échoué. Vérifiez les contraintes de la base de données.");

            return Page();
        }

        TempData["Succes"] =
            $"Exemplaire enregistré avec succès. Cote : {cote}.";

        return RedirectToPage("/Documents/Index");
    }

    private async Task ChargerListesAsync()
    {
        Documents = await _context.Documents
            .AsNoTracking()
            .OrderBy(d => d.Titre)
            .ToListAsync();

        Emplacements = await _context.Emplacements
            .AsNoTracking()
            .Include(e => e.CodeEtagereNavigation)
                .ThenInclude(et => et.CodeRayonNavigation)
            .Where(e =>
                e.Actif &&
                e.CodeEtagereNavigation.Actif &&
                e.CodeEtagereNavigation.CodeRayonNavigation.Actif)
            .OrderBy(e => e.CodeEtagereNavigation.CodeRayonNavigation.NomRayon)
            .ThenBy(e => e.CodeEtagereNavigation.NumeroEtagereRoman)
            .ThenBy(e => e.NumeroPlace)
            .ToListAsync();
    }
}
