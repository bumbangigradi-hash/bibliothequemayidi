
using bibliotheque_mayidi.Data.Generated;
using bibliotheque_mayidi.Models.Entities;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configuration des pages Razor.
builder.Services.AddRazorPages(options =>
{
    // Connexion obligatoire par défaut.
    options.Conventions.AuthorizeFolder("/");

    // Pages accessibles sans connexion.
    options.Conventions.AllowAnonymousToPage("/Connexion");
    options.Conventions.AllowAnonymousToPage("/Error");
    options.Conventions.AllowAnonymousToPage("/DiagnosticBase");
    options.Conventions.AllowAnonymousToPage("/InscriptionLecteur");
});

// Connexion à PostgreSQL.
var connectionString =
    builder.Configuration.GetConnectionString("BibliothequeDb");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "La chaîne de connexion BibliothequeDb est absente.");
}

builder.Services.AddDbContext<BibliothequeScaffoldContext>(options =>
    options.UseNpgsql(connectionString));

// Services applicatifs.
builder.Services.AddScoped<DiagnosticBaseService>();

builder.Services.AddScoped<
    IPasswordHasher<ComptesUtilisateur>,
    PasswordHasher<ComptesUtilisateur>>();

// Authentification par cookie.
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Connexion";
        options.AccessDeniedPath = "/Connexion";

        options.Cookie.Name = "Mayidi.Bibliotheque.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;

        // Adapté au profil HTTP ou HTTPS utilisé en développement.
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;

        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Gestion des erreurs.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Laisser Razor Pages gérer les routes.
app.MapRazorPages();

app.Run();
