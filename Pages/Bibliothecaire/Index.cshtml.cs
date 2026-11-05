
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using bibliotheque_mayidi.Data.Generated;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace bibliotheque_mayidi.Pages.Bibliothecaire
{
    [Authorize(Roles = "BIBLIOTHECAIRE,ADMINISTRATEUR,RESPONSABLE,VICE_RESPONSABLE")]
    public class IndexModel : PageModel
    {
        private readonly BibliothequeScaffoldContext _context;

        public IndexModel(BibliothequeScaffoldContext context)
        {
            _context = context;
        }

        // Statistiques générales
        public int TotalReservations { get; private set; }
        public int ReservationsEnAttente { get; private set; }
        public int ReservationsEnVerification { get; private set; }
        public int ReservationsPretes { get; private set; }
        public int TotalLecteurs { get; private set; }
        public int TotalEmpruntsEnCours { get; private set; }

        // Informations sur la session d'enregistrement
        public long? CodeSessionActive { get; private set; }
        public DateTime? DateOuvertureSession { get; private set; }
        public long ExemplairesSession { get; private set; }

        public bool SessionOuverte => CodeSessionActive.HasValue;

        // ---------------------------------------------------------
        // CHARGEMENT DU TABLEAU DE BORD
        // ---------------------------------------------------------
        public async Task OnGetAsync()
        {
            // Statistiques des réservations
            TotalReservations = await _context.Reservations
                .AsNoTracking()
                .CountAsync();

            ReservationsEnAttente = await _context.Reservations
                .AsNoTracking()
                .CountAsync(r => r.StatutReservation == "EN_ATTENTE");

            ReservationsEnVerification = await _context.Reservations
                .AsNoTracking()
                .CountAsync(r => r.StatutReservation == "EN_VERIFICATION");

            ReservationsPretes = await _context.Reservations
                .AsNoTracking()
                .CountAsync(r => r.StatutReservation == "PRETE");

            // Statistiques des lecteurs et emprunts
            TotalLecteurs = await _context.Lecteurs
                .AsNoTracking()
                .CountAsync();

            TotalEmpruntsEnCours = await _context.LignesPrets
                .AsNoTracking()
                .CountAsync(l => l.StatutLigne == "EN_COURS");

            // Récupération du compte connecté
            if (!EssayerObtenirCodeCompte(out int codeCompte))
                return;

            // Recherche de la session active
            long codeSession = await _context.Database
                .SqlQuery<long>($"""
                    SELECT code_session AS "Value"
                    FROM "SESSIONS_ENREGISTREMENT"
                    WHERE code_compte = {codeCompte}
                      AND statut_session = 'EN_COURS'
                    ORDER BY date_ouverture DESC
                    LIMIT 1
                    """)
                .SingleOrDefaultAsync();

            if (codeSession <= 0)
                return;

            CodeSessionActive = codeSession;

            // Date d'ouverture de la session
            DateOuvertureSession = await _context.Database
                .SqlQuery<DateTime>($"""
                    SELECT date_ouverture AS "Value"
                    FROM "SESSIONS_ENREGISTREMENT"
                    WHERE code_session = {codeSession}
                    """)
                .SingleAsync();

            // Nombre d'exemplaires enregistrés pendant cette session
            ExemplairesSession = await _context.Database
                .SqlQuery<long>($"""
                    SELECT COUNT(*)::bigint AS "Value"
                    FROM "EXEMPLAIRES"
                    WHERE code_session_enregistrement = {codeSession}
                    """)
                .SingleAsync();
        }

        // ---------------------------------------------------------
        // OUVERTURE D'UNE SESSION D'ENREGISTREMENT
        // ---------------------------------------------------------
        public async Task<IActionResult> OnPostOuvrirSessionAsync()
        {
            if (!EssayerObtenirCodeCompte(out int codeCompte))
                return Forbid();

            try
            {
                // Vérifier si une session est déjà ouverte
                long sessionExistante = await _context.Database
                    .SqlQuery<long>($"""
                        SELECT code_session AS "Value"
                        FROM "SESSIONS_ENREGISTREMENT"
                        WHERE code_compte = {codeCompte}
                          AND statut_session = 'EN_COURS'
                        LIMIT 1
                        """)
                    .SingleOrDefaultAsync();

                if (sessionExistante > 0)
                {
                    TempData["ErreurSession"] =
                        "Vous possédez déjà une session d'enregistrement ouverte.";

                    return RedirectToPage();
                }

                // Créer une nouvelle session
                await _context.Database.ExecuteSqlInterpolatedAsync($"""
                    INSERT INTO "SESSIONS_ENREGISTREMENT"
                        (code_compte, date_ouverture, statut_session)
                    VALUES
                        ({codeCompte}, CURRENT_TIMESTAMP, 'EN_COURS')
                    """);

                // Récupérer le numéro de la session créée
                long nouvelleSession = await _context.Database
                    .SqlQuery<long>($"""
                        SELECT code_session AS "Value"
                        FROM "SESSIONS_ENREGISTREMENT"
                        WHERE code_compte = {codeCompte}
                          AND statut_session = 'EN_COURS'
                        LIMIT 1
                        """)
                    .SingleAsync();

                TempData["SuccesSession"] =
                    $"Votre session d'enregistrement n° {nouvelleSession} est ouverte.";

                return RedirectToPage();
            }
            catch (Exception)
            {
                TempData["ErreurSession"] =
                    "Impossible d'ouvrir la session. Vérifiez qu'aucune autre session n'est déjà ouverte.";

                return RedirectToPage();
            }
        }

        // ---------------------------------------------------------
        // CLÔTURE MANUELLE D'UNE SESSION
        // ---------------------------------------------------------
        public async Task<IActionResult> OnPostCloturerSessionAsync()
        {
            if (!EssayerObtenirCodeCompte(out int codeCompte))
                return Forbid();

            try
            {
                int lignesModifiees = await _context.Database
                    .ExecuteSqlInterpolatedAsync($"""
                        UPDATE "SESSIONS_ENREGISTREMENT"
                        SET date_cloture = CURRENT_TIMESTAMP,
                            statut_session = 'CLOTUREE',
                            mode_cloture = 'MANUELLE'
                        WHERE code_compte = {codeCompte}
                          AND statut_session = 'EN_COURS'
                        """);

                if (lignesModifiees == 0)
                {
                    TempData["ErreurSession"] =
                        "Aucune session ouverte n'a été trouvée.";
                }
                else
                {
                    TempData["SuccesSession"] =
                        "Votre session d'enregistrement a été clôturée avec succès.";
                }

                return RedirectToPage();
            }
            catch (Exception)
            {
                TempData["ErreurSession"] =
                    "Une erreur est survenue pendant la clôture de votre session.";

                return RedirectToPage();
            }
        }

        // ---------------------------------------------------------
        // RÉCUPÉRATION DU CODE DU COMPTE CONNECTÉ
        // ---------------------------------------------------------
        private bool EssayerObtenirCodeCompte(out int codeCompte)
        {
            codeCompte = 0;

            string? identifiant = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

            return int.TryParse(identifiant, out codeCompte)
                   && codeCompte > 0;
        }
    }
}
