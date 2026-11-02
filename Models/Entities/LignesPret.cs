using System;
using System.Collections.Generic;

namespace bibliotheque_mayidi.Models.Entities;

public partial class LignesPret
{
    public long CodeLignePret { get; set; }

    public long CodeBordereauPret { get; set; }

    public int CodeExemplaire { get; set; }

    public long? CodeReservation { get; set; }

    public DateTime DateRetrait { get; set; }

    public DateOnly DateEcheance { get; set; }

    /// <summary>
    /// EN_COURS, RETOURNE, MANQUANT, PERDU
    /// </summary>
    public string StatutLigne { get; set; } = null!;

    public string? Observations { get; set; }

    public virtual BordereauxPret CodeBordereauPretNavigation { get; set; } = null!;

    public virtual Exemplaire CodeExemplaireNavigation { get; set; } = null!;

    public virtual Reservation? CodeReservationNavigation { get; set; }

    public virtual DossiersManquant? DossiersManquant { get; set; }

    public virtual ICollection<IncidentsDocument> IncidentsDocuments { get; set; } = new List<IncidentsDocument>();

    public virtual LignesRetour? LignesRetour { get; set; }

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
