using System;
using System.Collections.Generic;

namespace bibliotheque_mayidi.Models.Entities;

public partial class Lecteur
{
    public int CodeLecteur { get; set; }

    public int CodeCompte { get; set; }

    public string NumeroLecteur { get; set; } = null!;

    public string? MatriculeSeminaire { get; set; }

    /// <summary>
    /// SEMINARISTE, FORMATEUR, PRETRE, PERSONNEL, AUTRE
    /// </summary>
    public string CategorieLecteur { get; set; } = null!;

    public string? InstitutionOrigine { get; set; }

    public DateOnly DateInscription { get; set; }

    public DateOnly? DateExpiration { get; set; }

    public string StatutLecteur { get; set; } = null!;

    public string? Observations { get; set; }

    public virtual ICollection<BordereauxPret> BordereauxPrets { get; set; } = new List<BordereauxPret>();

    public virtual ICollection<BordereauxRetour> BordereauxRetours { get; set; } = new List<BordereauxRetour>();

    public virtual ComptesUtilisateur CodeCompteNavigation { get; set; } = null!;

    public virtual ICollection<ConsultationsSurPlace> ConsultationsSurPlaces { get; set; } = new List<ConsultationsSurPlace>();

    public virtual ICollection<DossiersManquant> DossiersManquants { get; set; } = new List<DossiersManquant>();

    public virtual ICollection<IncidentsDocument> IncidentsDocuments { get; set; } = new List<IncidentsDocument>();

    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
