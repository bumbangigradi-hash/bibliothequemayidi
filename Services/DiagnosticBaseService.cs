
using bibliotheque_mayidi.Data.Generated;
using Microsoft.EntityFrameworkCore;

public class DiagnosticBaseService
{
    private readonly BibliothequeScaffoldContext _context;

    public DiagnosticBaseService(BibliothequeScaffoldContext context)
    {
        _context = context;
    }

    public async Task<(bool Succes, string Message)> TesterConnexionAsync()
    {
        try
        {
            bool connecte = await _context.Database.CanConnectAsync();

            return connecte
                ? (true, "Connexion à PostgreSQL réussie.")
                : (false, "PostgreSQL est accessible, mais la connexion à la base a échoué.");
        }
        catch (Exception)
        {
            return (false, "Connexion impossible. Vérifiez la configuration et les accès PostgreSQL.");
        }
    }
}
