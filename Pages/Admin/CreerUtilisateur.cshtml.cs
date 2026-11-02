
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Globalization;
using bibliotheque_mayidi.Data.Generated;
using bibliotheque_mayidi.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace bibliotheque_mayidi.Pages.Admin;

[Authorize(Roles = "ADMINISTRATEUR")]
public class CreerUtilisateurModel : PageModel
{
    private readonly BibliothequeScaffoldContext _context;
    private readonly IWebHostEnvironment _environment;

    public CreerUtilisateurModel(
        BibliothequeScaffoldContext context,
        IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    [BindProperty]
    public FormulaireUtilisateur Input { get; set; } = new();

    public List<SelectListItem> RolesDisponibles { get; set; } = new();

    [TempData]
    public string? Message { get; set; }

    public class FormulaireUtilisateur
    {
        [Required(ErrorMessage = "Le nom est obligatoire.")]
        public string NomUtilisateur { get; set; } = "";

        public string? Postnom { get; set; }

        public string? Prenom { get; set; }

        [Required(ErrorMessage = "Le rôle est obligatoire.")]
        public string CodeRole { get; set; } = "";

        [Required(ErrorMessage = "Le numéro de téléphone est obligatoire.")]
        public string Telephone { get; set; } = "";

        [EmailAddress(ErrorMessage = "Adresse e-mail invalide.")]
        public string? EmailExterne { get; set; }

        [Required(ErrorMessage = "Le mot de passe est obligatoire.")]
        [MinLength(8, ErrorMessage = "Le mot de passe doit contenir au moins 8 caractères.")]
        public string MotDePasse { get; set; } = "";

        [Required(ErrorMessage = "Veuillez confirmer le mot de passe.")]
        [Compare(nameof(MotDePasse), ErrorMessage = "Les mots de passe ne correspondent pas.")]
        public string ConfirmationMotDePasse { get; set; } = "";

        public IFormFile? Photo { get; set; }
    }

    public async Task OnGetAsync()
    {
        await ChargerRolesAsync();
    }

    private async Task ChargerRolesAsync()
    {
        RolesDisponibles = await _context.Roles
            .AsNoTracking()
            .Where(r =>
                r.CodeRole != "LECTEUR" &&
                r.CodeRole != "ADMINISTRATEUR")
            .OrderBy(r => r.NomRole)
            .Select(r => new SelectListItem
            {
                Value = r.CodeRole,
                Text = r.NomRole
            })
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await ChargerRolesAsync();

        if (!ModelState.IsValid)
            return Page();

        string codeRole = Input.CodeRole.Trim();

        bool roleExiste = await _context.Roles.AnyAsync(r =>
            r.CodeRole == codeRole &&
            r.CodeRole != "LECTEUR" &&
            r.CodeRole != "ADMINISTRATEUR");

        if (!roleExiste)
        {
            ModelState.AddModelError("Input.CodeRole",
                "Le rôle sélectionné n'est pas autorisé.");
            return Page();
        }

        string nom = Input.NomUtilisateur.Trim();

        string postnom = Input.Postnom?.Trim() ?? "";
        string prenom = Input.Prenom?.Trim() ?? "";

        // Génération automatique de l'identifiant.
        string identifiantBase = GenererIdentifiant(
            prenom, nom, postnom);

        string identifiant = identifiantBase;
        int compteur = 1;

        while (await _context.ComptesUtilisateurs.AnyAsync(c =>
            c.Identifiant.ToLower() == identifiant.ToLower()))
        {
            identifiant = $"{identifiantBase}{compteur}";
            compteur++;
        }

        string adresseInterne =
            $"{identifiant}@bibliotheque-mayidi.org";

        // Vérification de la photo.
        string? cheminPhoto = null;

        if (Input.Photo != null && Input.Photo.Length > 0)
        {
            string[] extensionsAutorisees =
                { ".jpg", ".jpeg", ".png", ".webp" };

            string extension = Path.GetExtension(
                Input.Photo.FileName).ToLowerInvariant();

            if (!extensionsAutorisees.Contains(extension))
            {
                ModelState.AddModelError("Input.Photo",
                    "Format accepté : JPG, JPEG, PNG ou WEBP.");

                return Page();
            }

            if (Input.Photo.Length > 3 * 1024 * 1024)
            {
                ModelState.AddModelError("Input.Photo",
                    "La photo ne doit pas dépasser 3 Mo.");

                return Page();
            }

            string dossier = Path.Combine(
                _environment.WebRootPath ?? Path.Combine(
                    _environment.ContentRootPath, "wwwroot"),
                "uploads",
                "personnel");

            Directory.CreateDirectory(dossier);

            string nomFichier =
                $"{Guid.NewGuid()}{extension}";

            string cheminComplet = Path.Combine(
                dossier, nomFichier);

            using (var stream = new FileStream(
                cheminComplet, FileMode.Create))
            {
                await Input.Photo.CopyToAsync(stream);
            }

            cheminPhoto = $"/uploads/personnel/{nomFichier}";
        }

        var compte = new ComptesUtilisateur
        {
            CodeRole = codeRole,
            NomUtilisateur = nom,
            Postnom = string.IsNullOrWhiteSpace(postnom)
                ? null : postnom,
            Prenom = string.IsNullOrWhiteSpace(prenom)
                ? null : prenom,

            Identifiant = identifiant,
            AdresseInterne = adresseInterne,

            Telephone = Input.Telephone.Trim(),
            EmailExterne = string.IsNullOrWhiteSpace(
                Input.EmailExterne)
                ? null : Input.EmailExterne.Trim(),

            CheminPhoto = cheminPhoto,

            StatutCompte = "ACTIF",
            Actif = true,

            DateCreation = DateTime.Now,
            DateValidation = DateTime.Now,
            DateDerniereModification = DateTime.Now,

            NombreEchecsConnexion = 0
        };

        // Hachage sécurisé du mot de passe.
        var hasher = new PasswordHasher<ComptesUtilisateur>();

        compte.MotDePasseHash = hasher.HashPassword(
            compte, Input.MotDePasse);

        _context.ComptesUtilisateurs.Add(compte);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch
        {
            ModelState.AddModelError(string.Empty,
                "Une erreur est survenue lors de la création du compte. Vérifiez les données et réessayez.");

            return Page();
        }

        Message = $"Compte créé avec succès. Identifiant : {identifiant}";

        return RedirectToPage("/Admin/Utilisateurs");
    }

    private static string GenererIdentifiant(
        string prenom, string nom, string postnom)
    {
        string valeur = string.IsNullOrWhiteSpace(prenom)
            ? nom
            : $"{prenom}.{nom}";

        if (!string.IsNullOrWhiteSpace(postnom))
            valeur += "." + postnom;

        string normalise = valeur.Normalize(
            NormalizationForm.FormD);

        var resultat = new StringBuilder();

        foreach (char caractere in normalise)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(caractere)
                == UnicodeCategory.NonSpacingMark)
                continue;

            if (char.IsLetterOrDigit(caractere))
                resultat.Append(char.ToLowerInvariant(caractere));
            else if (caractere == '.' && resultat.Length > 0)
                resultat.Append('.');
        }

        return resultat.ToString().Trim('.');
    }
}
