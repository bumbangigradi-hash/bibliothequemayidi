
using Microsoft.AspNetCore.Mvc.RazorPages;

public class DiagnosticBaseModel : PageModel
{
    private readonly DiagnosticBaseService _diagnostic;

    public DiagnosticBaseModel(DiagnosticBaseService diagnostic)
    {
        _diagnostic = diagnostic;
    }

    public bool TestEffectue { get; private set; }
    public bool ConnexionReussie { get; private set; }
    public string Message { get; private set; } = "";

    public async Task OnGetAsync()
    {
        var resultat = await _diagnostic.TesterConnexionAsync();

        TestEffectue = true;
        ConnexionReussie = resultat.Succes;
        Message = resultat.Message;
    }
}
