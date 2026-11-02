
using System.ComponentModel.DataAnnotations;
using bibliotheque_mayidi.Data.Generated;
using bibliotheque_mayidi.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace bibliotheque_mayidi.Pages.Bibliothecaire.Collections;

[Authorize(Roles = "BIBLIOTHECAIRE,ADMINISTRATEUR")]
public class CreerModel : PageModel
{
    private readonly BibliothequeScaffoldContext _context;

    public CreerModel(BibliothequeScaffoldContext context)
    {
        _context = context;
    }

    [BindProperty]
    public FormulaireCollection Formulaire { get; set; } = new();

    public class FormulaireCollection
    {
        [Required(ErrorMessage = "Le nom de la collection est obligatoire.")]
        [StringLength(150, ErrorMessage = "150 caractères maximum.")]
        public string NomCollection { get; set; } = "";

        [StringLength(5000)]
        public string? Description { get; set; }
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        string nom = Formulaire.NomCollection.Trim();

        bool existe = await _context.Collections
            .AnyAsync(c => c.NomCollection.ToLower() == nom.ToLower());

        if (existe)
        {
            ModelState.AddModelError(
                "Formulaire.NomCollection",
                "Cette collection existe déjà."
            );

            return Page();
        }

        var collection = new Collection
        {
            NomCollection = nom,
            Description = string.IsNullOrWhiteSpace(Formulaire.Description)
                ? null
                : Formulaire.Description.Trim()
        };

        _context.Collections.Add(collection);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(
                "Formulaire.NomCollection",
                "Impossible d'enregistrer cette collection. Vérifiez si son nom existe déjà."
            );

            return Page();
        }

        TempData["Message"] = "La collection a été créée avec succès.";

        return RedirectToPage("./Index");
    }
}
