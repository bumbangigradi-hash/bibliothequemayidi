
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using bibliotheque_mayidi.Data.Generated;
using bibliotheque_mayidi.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace bibliotheque_mayidi.Pages.Lecteurs;

[Authorize(Roles = "ADMINISTRATEUR,BIBLIOTHECAIRE")]
public class CreateModel : PageModel
{
    private readonly BibliothequeScaffoldContext _context;
    private readonly PasswordHasher<ComptesUtilisateur> _passwordHasher = new();

    public CreateModel(BibliothequeScaffoldContext context)
    {
        _context = context;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required(ErrorMessage = "Le nom est obligatoire.")]
        [StringLength(150)]
        public string NomUtilisateur { get; set; } = string.Empty;

        [StringLength(150)]
        public string? Postnom { get; set; }

        [StringLength(150)]
        public string? Prenom { get; set; }

        [Required(ErrorMessage = "L'identifiant est obligatoire.")]
        [StringLength(100)]
        public string Identifiant { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le mot de passe est obligatoire.")]
        [MinLength(8, ErrorMessage = "Le mot de passe doit contenir au moins 8 caractères.")]
        public string MotDePasse { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "L'adresse e-mail n'est pas valide.")]
        [StringLength(255)]
        public string? EmailExterne { get; set; }

        [StringLength(50)]
        public string? Telephone { get; set; }

        [Required(ErrorMessage = "Le numéro de lecteur est obligatoire.")]
        [StringLength(50)]
        public string NumeroLecteur { get; set; } = string.Empty;

        [StringLength(100)]
        public string? MatriculeSeminaire { get; set; }

        [Required(ErrorMessage = "Choisissez une catégorie.")]
        public string CategorieLecteur { get; set; } = "SEMINARISTE";

        [StringLength(200)]
        public string? InstitutionOrigine { get; set; }

        [DataType(DataType.Date)]
        public DateOnly? DateExpiration { get; set; }

        public string? Observations { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        bool roleExiste = await _context.Roles
            .AnyAsync(r => r.CodeRole == "LECTEUR");

        if (!roleExiste)
        {
            TempData["Erreur"] =
                "Le rôle LECTEUR n'existe pas dans la table ROLES.";
            return RedirectToPage("./Index");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Input.NomUtilisateur = Input.NomUtilisateur.Trim();
        Input.Identifiant = Input.Identifiant.Trim();
        Input.NumeroLecteur = Input.NumeroLecteur.Trim();
        Input.CategorieLecteur = Input.CategorieLecteur.Trim();

        if (Input.DateExpiration.HasValue &&
            Input.DateExpiration.Value < DateOnly.FromDateTime(DateTime.Today))
        {
            ModelState.AddModelError(
                "Input.DateExpiration",
                "La date d'expiration ne peut pas être dans le passé.");
        }

        string[] categories =
        {
            "SEMINARISTE",
            "FORMATEUR",
            "PRETRE",
            "PERSONNEL",
            "AUTRE"
        };

        if (!categories.Contains(Input.CategorieLecteur))
        {
            ModelState.AddModelError(
                "Input.CategorieLecteur",
                "La catégorie sélectionnée est invalide.");
        }

        if (!ModelState.IsValid)
            return Page();

        string identifiant = Input.Identifiant;
        string numeroLecteur = Input.NumeroLecteur;

        if (await _context.ComptesUtilisateurs
            .AnyAsync(c => c.Identifiant == identifiant))
        {
            ModelState.AddModelError(
                "Input.Identifiant",
                "Cet identifiant existe déjà.");
        }

        if (await _context.Lecteurs
            .AnyAsync(l => l.NumeroLecteur == numeroLecteur))
        {
            ModelState.AddModelError(
                "Input.NumeroLecteur",
                "Ce numéro de lecteur existe déjà.");
        }

        bool roleExiste = await _context.Roles
            .AnyAsync(r => r.CodeRole == "LECTEUR");

        if (!roleExiste)
        {
            ModelState.AddModelError(
                string.Empty,
                "Le rôle LECTEUR est absent de la base de données.");
        }

        if (!ModelState.IsValid)
            return Page();

        string? claimId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(claimId, out int codeValidateur))
        {
            ModelState.AddModelError(
                string.Empty,
                "Impossible d'identifier le compte du bibliothécaire connecté.");
            return Page();
        }

        var validateur = await _context.ComptesUtilisateurs
            .FirstOrDefaultAsync(c =>
                c.CodeCompte == codeValidateur &&
                c.Actif &&
                c.StatutCompte == "ACTIF");

        if (validateur == null)
        {
            ModelState.AddModelError(
                string.Empty,
                "Le compte validateur est introuvable ou inactif.");
            return Page();
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            DateTime maintenant = DateTime.Now;

            var compte = new ComptesUtilisateur
            {
                CodeRole = "LECTEUR",
                NomUtilisateur = Input.NomUtilisateur,
                Postnom = Input.Postnom?.Trim(),
                Prenom = Input.Prenom?.Trim(),
                Identifiant = identifiant,
                Telephone = Input.Telephone?.Trim(),
                EmailExterne = Input.EmailExterne?.Trim(),
                StatutCompte = "ACTIF",
                Actif = true,
                DateCreation = maintenant,
                DateValidation = maintenant,
                ValidePar = validateur.CodeCompte,
                NombreEchecsConnexion = 0
            };

            compte.MotDePasseHash =
                _passwordHasher.HashPassword(
                    compte,
                    Input.MotDePasse);

            _context.ComptesUtilisateurs.Add(compte);
            await _context.SaveChangesAsync();

            var lecteur = new Lecteur
            {
                CodeCompte = compte.CodeCompte,
                NumeroLecteur = numeroLecteur,
                MatriculeSeminaire =
                    string.IsNullOrWhiteSpace(Input.MatriculeSeminaire)
                        ? null
                        : Input.MatriculeSeminaire.Trim(),
                CategorieLecteur = Input.CategorieLecteur,
                InstitutionOrigine =
                    string.IsNullOrWhiteSpace(Input.InstitutionOrigine)
                        ? null
                        : Input.InstitutionOrigine.Trim(),
                DateInscription = DateOnly.FromDateTime(DateTime.Today),
                DateExpiration = Input.DateExpiration,
                StatutLecteur = "ACTIF",
                Observations = Input.Observations?.Trim()
            };

            _context.Lecteurs.Add(lecteur);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            TempData["Succes"] =
                $"Le lecteur {lecteur.NumeroLecteur} a été créé avec succès.";

            return RedirectToPage("./Index");
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync();

            ModelState.AddModelError(
                string.Empty,
                "Enregistrement impossible. Vérifiez les contraintes de la base de données et l'unicité des informations.");
            return Page();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
