
using System.ComponentModel.DataAnnotations;
using bibliotheque_mayidi.Data.Generated;
using bibliotheque_mayidi.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace bibliotheque_mayidi.Pages.Exemplaires;

public class EditModel : PageModel
{
    private readonly BibliothequeScaffoldContext _context;

    public EditModel(BibliothequeScaffoldContext context)
    {
        _context = context;
    }

    [BindProperty]
    public ExemplaireInput Input { get; set; } = new();

    public string TitreDocument { get; set; } = string.Empty;
    public string CoteActuelle { get; set; } = string.Empty;

    public IList<Emplacement> Emplacements { get; set; }
        = new List<Emplacement>();

    public class ExemplaireInput
    {
        public int CodeExemplaire { get; set; }

        [Required(ErrorMessage = "Le numéro d'inventaire est obligatoire.")]
        [StringLength(100)]
        public string NumeroInventaire { get; set; } = string.Empty;

        [StringLength(100)]
        public string? CodeBarres { get; set; }

        [Required(ErrorMessage = "Veuillez sélectionner un emplacement.")]
        public int? CodeEmplacement { get; set; }

        [Required(ErrorMessage = "Veuillez préciser l'état matériel.")]
        public string EtatDocument { get; set; } = "BON";

        public DateOnly? DateAcquisition { get; set; }

        [StringLength(100)]
        public string? ModeAcquisition { get; set; }

        [Range(typeof(decimal), "0", "999999999999",
            ErrorMessage = "La valeur ne peut pas être négative.")]
        public decimal? ValeurAcquisition { get; set; }

        [Required(ErrorMessage = "Veuillez sélectionner la devise.")]
        public string DeviseValeur { get; set; } = "CDF";

        public string? Observations { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var exemplaire = await _context.Exemplaires
            .Include(e => e.CodeDocumentNavigation)
            .FirstOrDefaultAsync(e => e.CodeExemplaire == id);

        if (exemplaire == null)
            return NotFound();

        ChargerInput(exemplaire);
        await ChargerEmplacementsAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        if (id != Input.CodeExemplaire)
            return BadRequest();

        var exemplaire = await _context.Exemplaires
            .Include(e => e.CodeDocumentNavigation)
            .FirstOrDefaultAsync(e => e.CodeExemplaire == id);

        if (exemplaire == null)
            return NotFound();

        Input.NumeroInventaire = (Input.NumeroInventaire ?? string.Empty).Trim();

        Input.CodeBarres = string.IsNullOrWhiteSpace(Input.CodeBarres)
            ? null
            : Input.CodeBarres.Trim();

        await ChargerEmplacementsAsync();

        if (!string.IsNullOrWhiteSpace(Input.NumeroInventaire) &&
            await _context.Exemplaires.AnyAsync(e =>
                e.CodeExemplaire != id &&
                e.NumeroInventaire == Input.NumeroInventaire))
        {
            ModelState.AddModelError(
                "Input.NumeroInventaire",
                "Ce numéro d'inventaire est déjà utilisé.");
        }

        if (!string.IsNullOrWhiteSpace(Input.CodeBarres) &&
            await _context.Exemplaires.AnyAsync(e =>
                e.CodeExemplaire != id &&
                e.CodeBarres == Input.CodeBarres))
        {
            ModelState.AddModelError(
                "Input.CodeBarres",
                "Ce code-barres est déjà utilisé.");
        }

        if (Input.CodeEmplacement.HasValue &&
            !await _context.Emplacements.AnyAsync(e =>
                e.CodeEmplacement == Input.CodeEmplacement.Value &&
                e.Actif &&
                e.CodeEtagereNavigation.Actif &&
                e.CodeEtagereNavigation.CodeRayonNavigation.Actif))
        {
            ModelState.AddModelError(
                "Input.CodeEmplacement",
                "L'emplacement sélectionné est inexistant ou inactif.");
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
            TitreDocument = exemplaire.CodeDocumentNavigation.Titre;
            CoteActuelle = exemplaire.Cote;
            return Page();
        }

        var emplacement = await _context.Emplacements
            .Include(e => e.CodeEtagereNavigation)
                .ThenInclude(et => et.CodeRayonNavigation)
            .FirstOrDefaultAsync(e =>
                e.CodeEmplacement == Input.CodeEmplacement!.Value &&
                e.Actif &&
                e.CodeEtagereNavigation.Actif &&
                e.CodeEtagereNavigation.CodeRayonNavigation.Actif);

        if (emplacement == null)
        {
            ModelState.AddModelError(
                "Input.CodeEmplacement",
                "Impossible de retrouver cet emplacement actif.");

            TitreDocument = exemplaire.CodeDocumentNavigation.Titre;
            CoteActuelle = exemplaire.Cote;
            return Page();
        }

        var rayon = emplacement.CodeEtagereNavigation.CodeRayonNavigation;
        var etagere = emplacement.CodeEtagereNavigation;

        exemplaire.NumeroInventaire = Input.NumeroInventaire;
        exemplaire.CodeBarres = Input.CodeBarres;
        exemplaire.CodeEmplacement = emplacement.CodeEmplacement;
        exemplaire.Cote =
            $"{rayon.NomRayon} {etagere.NumeroEtagereRoman}/{emplacement.NumeroPlace}";
        exemplaire.EtatDocument = Input.EtatDocument;
        exemplaire.DateAcquisition = Input.DateAcquisition;
        exemplaire.ModeAcquisition = string.IsNullOrWhiteSpace(Input.ModeAcquisition)
            ? null
            : Input.ModeAcquisition.Trim();
        exemplaire.ValeurAcquisition = Input.ValeurAcquisition;
        exemplaire.DeviseValeur = Input.DeviseValeur;
        exemplaire.Observations = string.IsNullOrWhiteSpace(Input.Observations)
            ? null
            : Input.Observations.Trim();

        // Le statut de prêt n'est pas modifié ici.
        // Il sera géré par les fonctionnalités de prêt et de retour.

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(
                string.Empty,
                "La modification a échoué. Vérifiez les contraintes de la base de données.");

            TitreDocument = exemplaire.CodeDocumentNavigation.Titre;
            CoteActuelle = exemplaire.Cote;

            return Page();
        }

        TempData["Succes"] =
            $"L'exemplaire {exemplaire.NumeroInventaire} a été modifié avec succès.";

        return RedirectToPage("./Index");
    }

    private void ChargerInput(Exemplaire exemplaire)
    {
        Input = new ExemplaireInput
        {
            CodeExemplaire = exemplaire.CodeExemplaire,
            NumeroInventaire = exemplaire.NumeroInventaire,
            CodeBarres = exemplaire.CodeBarres,
            CodeEmplacement = exemplaire.CodeEmplacement,
            EtatDocument = exemplaire.EtatDocument,
            DateAcquisition = exemplaire.DateAcquisition,
            ModeAcquisition = exemplaire.ModeAcquisition,
            ValeurAcquisition = exemplaire.ValeurAcquisition,
            DeviseValeur = exemplaire.DeviseValeur,
            Observations = exemplaire.Observations
        };

        TitreDocument = exemplaire.CodeDocumentNavigation.Titre;
        CoteActuelle = exemplaire.Cote;
    }

    private async Task ChargerEmplacementsAsync()
    {
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
