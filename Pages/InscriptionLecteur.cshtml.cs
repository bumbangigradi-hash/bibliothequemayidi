
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

    public InscriptionLecteurModel(
        BibliothequeScaffoldContext context,
        IPasswordHasher<ComptesUtilisateur> hasher)
    {
        _context = context;
        _hasher = hasher;
    }

    [BindProperty]
    [Required(ErrorMessage = "Le nom est obligatoire.")]
    public string Nom { get; set; } = "";

    [BindProperty]
    public string? Postnom { get; set; }

    [BindProperty]
    public string? Prenom { get; set; }

    [BindProperty]
    public string? MatriculeSeminaire { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "La catégorie est obligatoire.")]
    public string CategorieLecteur { get; set; } = "";

    [BindProperty]
    public string? InstitutionOrigine { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "L'identifiant est obligatoire.")]
    [StringLength(80, MinimumLength = 3)]
    public string Identifiant { get; set; } = "";

    [BindProperty]
    [Required(ErrorMessage = "Le mot de passe est obligatoire.")]
    [StringLength(100, MinimumLength = 12,
        ErrorMessage = "Le mot de passe doit contenir au moins 12 caractères.")]
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
        Nom = Nom.Trim();
        Identifiant = Identifiant.Trim();
        CategorieLecteur = CategorieLecteur.Trim();

        MatriculeSeminaire = string.IsNullOrWhiteSpace(MatriculeSeminaire)
            ? null
            : MatriculeSeminaire.Trim();

        if (!ModelState.IsValid)
            return Page();

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

        var roleExiste = await _context.Roles
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
            var compte = new ComptesUtilisateur
            {
                CodeRole = "LECTEUR",
                NomUtilisateur = Nom,
                Postnom = string.IsNullOrWhiteSpace(Postnom)
                    ? null : Postnom.Trim(),
                Prenom = string.IsNullOrWhiteSpace(Prenom)
                    ? null : Prenom.Trim(),
                Identifiant = Identifiant,
                MotDePasseHash = "",
                StatutCompte = "EN_ATTENTE",
                Actif = true,
                DateCreation = DateTime.UtcNow,
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
                InstitutionOrigine = string.IsNullOrWhiteSpace(InstitutionOrigine)
                    ? null : InstitutionOrigine.Trim(),
                DateInscription = DateOnly.FromDateTime(DateTime.Today),
                StatutLecteur = "EN_ATTENTE"
            };

            _context.Lecteurs.Add(lecteur);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            Message = "Votre demande d'inscription a été enregistrée. " +
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
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync();

            ModelState.AddModelError(
                string.Empty,
                "L'inscription n'a pas pu être enregistrée. " +
                "Vérifiez l'identifiant et le matricule, puis réessayez.");

            return Page();
        }
    }

    private async Task<string> GenererNumeroLecteurAsync()
    {
        for (int tentative = 0; tentative < 10; tentative++)
        {
            string numero =
                $"MAY-{DateTime.UtcNow:yyyyMMdd}-{RandomNumberGenerator.GetInt32(0, 1_000_000):D6}";

            bool existe = await _context.Lecteurs
                .AnyAsync(l => l.NumeroLecteur == numero);

            if (!existe)
                return numero;
        }

        throw new InvalidOperationException(
            "Impossible de générer un numéro de lecteur unique.");
    }
}
