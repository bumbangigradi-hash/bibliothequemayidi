
using bibliotheque_mayidi.Data.Generated;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace bibliotheque_mayidi.Pages.Admin;

[Authorize(Roles = "ADMINISTRATEUR")]
public class IndexModel : PageModel
{
    private readonly BibliothequeScaffoldContext _context;

    public IndexModel(BibliothequeScaffoldContext context)
    {
        _context = context;
    }

    // Statistiques des lecteurs
    public int InscriptionsEnAttente { get; set; }

    public int TotalLecteurs { get; set; }

    public int LecteursActifs { get; set; }

    // Statistiques du personnel
    public int TotalPersonnel { get; set; }

    public int PersonnelActif { get; set; }

    public int PersonnelSuspendu { get; set; }

    public async Task OnGetAsync()
    {
        // Nombre de demandes d'inscription en attente
        InscriptionsEnAttente = await _context.ComptesUtilisateurs
            .CountAsync(c =>
                c.CodeRole == "LECTEUR" &&
                c.StatutCompte == "EN_ATTENTE");

        // Nombre total de lecteurs
        TotalLecteurs = await _context.ComptesUtilisateurs
            .CountAsync(c =>
                c.CodeRole == "LECTEUR");

        // Nombre de lecteurs actifs
        LecteursActifs = await _context.ComptesUtilisateurs
            .CountAsync(c =>
                c.CodeRole == "LECTEUR" &&
                c.StatutCompte == "ACTIF" &&
                c.Actif);

        // Nombre total de membres du personnel
        TotalPersonnel = await _context.ComptesUtilisateurs
            .CountAsync(c =>
                c.CodeRole != "LECTEUR" &&
                c.CodeRole != "ADMINISTRATEUR");

        // Personnel actif
        PersonnelActif = await _context.ComptesUtilisateurs
            .CountAsync(c =>
                c.CodeRole != "LECTEUR" &&
                c.CodeRole != "ADMINISTRATEUR" &&
                c.StatutCompte == "ACTIF" &&
                c.Actif);

        // Personnel suspendu
        PersonnelSuspendu = await _context.ComptesUtilisateurs
            .CountAsync(c =>
                c.CodeRole != "LECTEUR" &&
                c.CodeRole != "ADMINISTRATEUR" &&
                c.StatutCompte == "SUSPENDU");
    }
}
