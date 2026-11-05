
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Security.Claims;
using System.Text;
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

    private readonly PasswordHasher<ComptesUtilisateur>
        _passwordHasher = new();

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

    // Nettoyage des noms pour créer un identifiant valide.
    private static string NettoyerIdentifiant(string valeur)
    {
        string normalise = valeur.Normalize(NormalizationForm.FormD);

        var resultat = new StringBuilder();

        foreach (char caractere in normalise)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(caractere)
                == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (char.IsLetterOrDigit(caractere))
            {
                resultat.Append(char.ToLowerInvariant(caractere));
            }
            else if (resultat.Length > 0 &&
                     resultat[resultat.Length - 1] != '.')
            {
                resultat.Append('.');
            }
        }

        return resultat.ToString().Trim('.');
    }

    // Génération d'un identifiant unique.
    private async Task<string> GenererIdentifiantUniqueAsync()
    {
        string prenom = NettoyerIdentifiant(Input.Prenom ?? "");
        string nom = NettoyerIdentifiant(Input.NomUtilisateur);
        string postnom = NettoyerIdentifiant(Input.Postnom ?? "");

        string baseIdentifiant = string.Join(
            ".",
            new[] { prenom, nom, postnom }
                .Where(p => !string.IsNullOrWhiteSpace(p))
        );

        if (string.IsNullOrWhiteSpace(baseIdentifiant))
        {
            throw new InvalidOperationException(
                "Impossible de générer l'identifiant. Vérifiez le nom du lecteur."
            );
        }

        // Limitation à 100 caractères.
        baseIdentifiant = baseIdentifiant.Length > 96
            ? baseIdentifiant[..96]
            : baseIdentifiant;

        string identifiant = baseIdentifiant;
        int numero = 2;

        while (await _context.ComptesUtilisateurs
            .AnyAsync(c => c.Identifiant == identifiant))
        {
            string suffixe = numero.ToString();

            int longueur = Math.Min(
                baseIdentifiant.Length,
                100 - suffixe.Length
            );

            identifiant =
                baseIdentifiant[..longueur] + suffixe;

            numero++;
        }

        return identifiant;
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
        // Nettoyage des données.
        Input.NomUtilisateur = Input.NomUtilisateur?.Trim() ?? "";
        Input.Postnom = Input.Postnom?.Trim();
        Input.Prenom = Input.Prenom?.Trim();
        Input.NumeroLecteur = Input.NumeroLecteur?.Trim() ?? "";
        Input.CategorieLecteur = Input.CategorieLecteur?.Trim() ?? "";
        Input.Telephone = Input.Telephone?.Trim();
        Input.EmailExterne = Input.EmailExterne?.Trim();
        Input.MatriculeSeminaire = Input.MatriculeSeminaire?.Trim();
        Input.InstitutionOrigine = Input.InstitutionOrigine?.Trim();
        Input.Observations = Input.Observations?.Trim();

        // L'identifiant envoyé par le navigateur n'est pas fiable.
        ModelState.Remove("Input.Identifiant");

        // Génération définitive côté serveur.
        try
        {
            Input.Identifiant = await GenererIdentifiantUniqueAsync();
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }

        // Vérification de la date.
        if (Input.DateExpiration.HasValue &&
            Input.DateExpiration.Value <
            DateOnly.FromDateTime(DateTime.Today))
        {
            ModelState.AddModelError(
                "Input.DateExpiration",
                "La date d'expiration ne peut pas être dans le passé."
            );
        }

        // Vérification des catégories.
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
                "La catégorie sélectionnée est invalide."
            );
        }

        // Vérification du numéro de lecteur.
        if (!string.IsNullOrWhiteSpace(Input.NumeroLecteur))
        {
            bool numeroExiste = await _context.Lecteurs
                .AnyAsync(l =>
                    l.NumeroLecteur == Input.NumeroLecteur);

            if (numeroExiste)
            {
                ModelState.AddModelError(
                    "Input.NumeroLecteur",
                    "Ce numéro de lecteur existe déjà."
                );
            }
        }

        // Le matricule est facultatif.
        // On vérifie son unicité uniquement lorsqu'il est renseigné.
        if (!string.IsNullOrWhiteSpace(Input.MatriculeSeminaire))
        {
            bool matriculeExiste = await _context.Lecteurs
                .AnyAsync(l =>
                    l.MatriculeSeminaire == Input.MatriculeSeminaire);

            if (matriculeExiste)
            {
                ModelState.AddModelError(
                    "Input.MatriculeSeminaire",
                    "Ce matricule est déjà utilisé par un autre lecteur."
                );
            }
        }
        else
        {
            Input.MatriculeSeminaire = null;
        }

        // Vérification du rôle.
        bool roleExiste = await _context.Roles
            .AnyAsync(r => r.CodeRole == "LECTEUR");

        if (!roleExiste)
        {
            ModelState.AddModelError(
                string.Empty,
                "Le rôle LECTEUR est absent de la base de données."
            );
        }

        if (!ModelState.IsValid)
            return Page();

        // Identification du bibliothécaire connecté.
        string? claimId = User.FindFirstValue(
            ClaimTypes.NameIdentifier
        );

        if (!int.TryParse(claimId, out int codeValidateur))
        {
            ModelState.AddModelError(
                string.Empty,
                "Impossible d'identifier le compte du bibliothécaire connecté."
            );

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
                "Le compte validateur est introuvable ou inactif."
            );

            return Page();
        }

        // Vérification finale du rôle avant la transaction.
        string identifiant = Input.Identifiant;
        string numeroLecteur = Input.NumeroLecteur;

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            DateTime maintenant = DateTime.Now;

            // 1. Création du compte utilisateur.
            var compte = new ComptesUtilisateur
            {
                CodeRole = "LECTEUR",

                NomUtilisateur = Input.NomUtilisateur,
                Postnom = Input.Postnom,
                Prenom = Input.Prenom,

                Identifiant = identifiant,

                Telephone = Input.Telephone,
                EmailExterne = Input.EmailExterne,

                StatutCompte = "ACTIF",
                Actif = true,

                DateCreation = maintenant,
                DateValidation = maintenant,

                ValidePar = validateur.CodeCompte,

                NombreEchecsConnexion = 0
            };

            // Hachage sécurisé du mot de passe.
            compte.MotDePasseHash =
                _passwordHasher.HashPassword(
                    compte,
                    Input.MotDePasse
                );

            _context.ComptesUtilisateurs.Add(compte);

            await _context.SaveChangesAsync();

            // 2. Création du dossier de lecteur.
            var lecteur = new Lecteur
            {
                CodeCompte = compte.CodeCompte,

                NumeroLecteur = numeroLecteur,

                MatriculeSeminaire =
                    Input.MatriculeSeminaire,

                CategorieLecteur =
                    Input.CategorieLecteur,

                InstitutionOrigine =
                    Input.InstitutionOrigine,

                DateInscription =
                    DateOnly.FromDateTime(DateTime.Today),

                DateExpiration =
                    Input.DateExpiration,

                StatutLecteur = "ACTIF",

                Observations = Input.Observations
            };

            _context.Lecteurs.Add(lecteur);

            await _context.SaveChangesAsync();

            // 3. Validation définitive de la transaction.
            await transaction.CommitAsync();

            TempData["Succes"] =
                $"Le lecteur {Input.NomUtilisateur} " +
                $"a été créé avec succès. " +
                $"Identifiant : {identifiant}";

            return RedirectToPage("./Index");
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync();

            ModelState.AddModelError(
                string.Empty,
                "L'enregistrement a échoué. Une information existe peut-être déjà dans la base de données. Vérifiez le numéro de lecteur, le matricule et l'identifiant."
            );

            return Page();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();

            ModelState.AddModelError(
                string.Empty,
                "Une erreur inattendue est survenue pendant la création du lecteur. Aucune création partielle n'a été conservée."
            );

            return Page();
        }
    }
}
