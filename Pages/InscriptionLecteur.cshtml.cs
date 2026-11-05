
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using bibliotheque_mayidi.Data.Generated;
using bibliotheque_mayidi.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace bibliotheque_mayidi.Pages;

[AllowAnonymous]
public class InscriptionLecteurModel : PageModel
{
    private readonly BibliothequeScaffoldContext _context;
    private readonly IPasswordHasher<ComptesUtilisateur> _hasher;
    private readonly ILogger<InscriptionLecteurModel> _logger;

    public InscriptionLecteurModel(
        BibliothequeScaffoldContext context,
        IPasswordHasher<ComptesUtilisateur> hasher,
        ILogger<InscriptionLecteurModel> logger)
    {
        _context = context;
        _hasher = hasher;
        _logger = logger;
    }

    [BindProperty]
    [Required(ErrorMessage = "Le nom est obligatoire.")]
    [StringLength(100)]
    public string Nom { get; set; } = "";

    [BindProperty]
    [StringLength(100)]
    public string? Postnom { get; set; }

    [BindProperty]
    [StringLength(100)]
    public string? Prenom { get; set; }

    [BindProperty]
    [StringLength(60)]
    public string? MatriculeSeminaire { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "La catégorie est obligatoire.")]
    [StringLength(40)]
    public string CategorieLecteur { get; set; } = "";

    [BindProperty]
    [StringLength(180)]
    public string? InstitutionOrigine { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "L'identifiant est obligatoire.")]
    [StringLength(100, MinimumLength = 3,
        ErrorMessage = "L'identifiant doit contenir entre 3 et 100 caractères.")]
    public string Identifiant { get; set; } = "";

    [BindProperty]
    [Required(ErrorMessage = "Le mot de passe est obligatoire.")]
    [StringLength(100, MinimumLength = 8,
        ErrorMessage = "Le mot de passe doit contenir au moins 8 caractères.")]
    public string MotDePasse { get; set; } = "";

    [BindProperty]
    [Required(ErrorMessage = "Confirmez le mot de passe.")]
    [Compare(nameof(MotDePasse),
        ErrorMessage = "Les mots de passe ne correspondent pas.")]
    public string ConfirmationMotDePasse { get; set; } = "";

    public string? Message { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Nom = Nom?.Trim() ?? "";
        Postnom = Nettoyer(Postnom);
        Prenom = Nettoyer(Prenom);
        Identifiant = Identifiant?.Trim() ?? "";
        CategorieLecteur = CategorieLecteur?.Trim() ?? "";
        MatriculeSeminaire = Nettoyer(MatriculeSeminaire);
        InstitutionOrigine = Nettoyer(InstitutionOrigine);

        if (!ModelState.IsValid)
            return Page();

        string[] categoriesAutorisees =
        {
            "SEMINARISTE",
            "FORMATEUR",
            "PERSONNEL",
            "EXTERNE"
        };

        if (!categoriesAutorisees.Contains(
                CategorieLecteur,
                StringComparer.Ordinal))
        {
            ModelState.AddModelError(
                nameof(CategorieLecteur),
                "Veuillez sélectionner une catégorie valide.");

            return Page();
        }

        if (await _context.ComptesUtilisateurs
            .AnyAsync(c => c.Identifiant == Identifiant))
        {
            ModelState.AddModelError(
                nameof(Identifiant),
                "Cet identifiant est déjà utilisé.");

            return Page();
        }

        if (MatriculeSeminaire != null &&
            await _context.Lecteurs.AnyAsync(
                l => l.MatriculeSeminaire == MatriculeSeminaire))
        {
            ModelState.AddModelError(
                nameof(MatriculeSeminaire),
                "Ce matricule est déjà enregistré.");

            return Page();
        }

        bool roleExiste = await _context.Roles
            .AnyAsync(r => r.CodeRole == "LECTEUR");

        if (!roleExiste)
        {
            ModelState.AddModelError(
                string.Empty,
                "Le rôle LECTEUR n'existe pas dans la base de données.");

            return Page();
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            // Date compatible avec PostgreSQL timestamp without time zone.
            DateTime maintenant = DateTime.SpecifyKind(
                DateTime.Now,
                DateTimeKind.Unspecified);

            var compte = new ComptesUtilisateur
            {
                CodeRole = "LECTEUR",
                NomUtilisateur = Nom,
                Postnom = Postnom,
                Prenom = Prenom,
                Identifiant = Identifiant,
                MotDePasseHash = "",
                StatutCompte = "EN_ATTENTE",
                Actif = false,
                DateCreation = maintenant,
                NombreEchecsConnexion = 0
            };

            compte.MotDePasseHash =
                _hasher.HashPassword(compte, MotDePasse);

            _context.ComptesUtilisateurs.Add(compte);

            await _context.SaveChangesAsync();

            var lecteur = new Lecteur
            {
                CodeCompte = compte.CodeCompte,
                NumeroLecteur = await GenererNumeroLecteurAsync(),
                MatriculeSeminaire = MatriculeSeminaire,
                CategorieLecteur = CategorieLecteur,
                InstitutionOrigine = InstitutionOrigine,
                DateInscription = DateOnly.FromDateTime(maintenant),
                StatutLecteur = "EN_ATTENTE"
            };

            _context.Lecteurs.Add(lecteur);

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            Message =
                "Votre demande d'inscription a été enregistrée. " +
                "Vous pourrez vous connecter après validation par l'administration.";

            ModelState.Clear();

            Nom = "";
            Postnom = "";
            Prenom = "";
            MatriculeSeminaire = "";
            CategorieLecteur = "";
            InstitutionOrigine = "";
            Identifiant = "";
            MotDePasse = "";
            ConfirmationMotDePasse = "";

            return Page();
        }
        catch (DbUpdateException ex)
        {
            await transaction.RollbackAsync();

            _logger.LogError(
                ex,
                "Erreur PostgreSQL lors de l'inscription du lecteur. " +
                "Détail : {Detail}",
                ex.InnerException?.Message ?? ex.Message);

            ModelState.AddModelError(
                string.Empty,
                "L'inscription n'a pas pu être enregistrée. " +
                "Une erreur de base de données est survenue. Réessayez.");

            return Page();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            _logger.LogError(
                ex,
                "Erreur inattendue lors de l'inscription publique du lecteur.");

            ModelState.AddModelError(
                string.Empty,
                "Une erreur inattendue est survenue. Veuillez réessayer.");

            return Page();
        }
    }

    private static string? Nettoyer(string? valeur)
    {
        return string.IsNullOrWhiteSpace(valeur)
            ? null
            : valeur.Trim();
    }

    private async Task<string> GenererNumeroLecteurAsync()
    {
        for (int tentative = 0; tentative < 20; tentative++)
        {
            string numero =
                $"MAY-{DateTime.Now:yyyyMMdd}-" +
                $"{RandomNumberGenerator.GetInt32(0, 1_000_000):D6}";

            bool existe = await _context.Lecteurs
                .AnyAsync(l => l.NumeroLecteur == numero);

            if (!existe)
                return numero;
        }

        throw new InvalidOperationException(
            "Impossible de générer un numéro de lecteur unique.");
    }
}
