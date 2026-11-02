
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using bibliotheque_mayidi.Data.Generated;
using bibliotheque_mayidi.Models.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace bibliotheque_mayidi.Pages;

[AllowAnonymous]
public class ConnexionModel : PageModel
{
    private readonly BibliothequeScaffoldContext _context;
    private readonly IPasswordHasher<ComptesUtilisateur> _hasher;

    public ConnexionModel(
        BibliothequeScaffoldContext context,
        IPasswordHasher<ComptesUtilisateur> hasher)
    {
        _context = context;
        _hasher = hasher;
    }

    [BindProperty]
    [Required(ErrorMessage = "Saisissez votre identifiant.")]
    public string Identifiant { get; set; } = "";

    [BindProperty]
    [Required(ErrorMessage = "Saisissez votre mot de passe.")]
    public string MotDePasse { get; set; } = "";

    public string? Erreur { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        if (!ModelState.IsValid)
            return Page();

        var identifiant = Identifiant.Trim();

        var compte = await _context.ComptesUtilisateurs
            .Include(c => c.CodeRoleNavigation)
            .FirstOrDefaultAsync(c => c.Identifiant == identifiant);

        if (compte == null ||
            !compte.Actif ||
            compte.StatutCompte != "ACTIF")
        {
            Erreur = "Identifiant incorrect ou compte inactif.";
            return Page();
        }

        var resultat = _hasher.VerifyHashedPassword(
            compte,
            compte.MotDePasseHash,
            MotDePasse);

        if (resultat == PasswordVerificationResult.Failed)
        {
            Erreur = "Identifiant ou mot de passe incorrect.";
            return Page();
        }

        compte.DerniereConnexion = DateTime.Now;
        compte.NombreEchecsConnexion = 0;

        await _context.SaveChangesAsync();

        var nomAffiche = string.Join(" ",
            new[] { compte.Prenom, compte.NomUtilisateur, compte.Postnom }
                .Where(n => !string.IsNullOrWhiteSpace(n)));

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, compte.CodeCompte.ToString()),
            new(
                ClaimTypes.Name,
                string.IsNullOrWhiteSpace(nomAffiche)
                    ? compte.Identifiant
                    : nomAffiche),
            new(ClaimTypes.Role, compte.CodeRole)
        };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties
            {
                IsPersistent = false,
                AllowRefresh = true
            });

        if (!string.IsNullOrWhiteSpace(returnUrl) &&
            Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        var role = compte.CodeRole.Trim().ToUpperInvariant();

        return role switch
        {
            "ADMINISTRATEUR" => RedirectToPage("/Admin/Index"),
            "BIBLIOTHECAIRE" => RedirectToPage("/Bibliothecaire/Index"),
            "GESTIONNAIRE" => RedirectToPage("/Gestionnaire/Index"),
            "LECTEUR" => RedirectToPage("/Lecteur/Index"),
            "RESPONSABLE" => RedirectToPage("/Responsable/Index"),
            "VICE_RESPONSABLE" => RedirectToPage("/ViceResponsable/Index"),
            _ => RedirectToPage("/Index")
        };
    }
}
