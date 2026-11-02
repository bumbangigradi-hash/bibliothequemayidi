using System;
using System.Collections.Generic;

namespace bibliotheque_mayidi.Models.Entities;

/// <summary>
/// Le delai de retrait est de 48 heures apres notification de disponibilite
/// </summary>
public partial class Reservation
{
    public long CodeReservation { get; set; }

    public string NumeroReservation { get; set; } = null!;

    public int CodeLecteur { get; set; }

    public int CodeDocument { get; set; }

    public int? CodeExemplaire { get; set; }

    public DateTime DateReservation { get; set; }

    /// <summary>
    /// EN_ATTENTE, EN_VERIFICATION, PRETE, HONOREE, EXPIREE, ANNULEE, REFUSEE
    /// </summary>
    public string StatutReservation { get; set; } = null!;

    public DateTime? DateVerification { get; set; }

    public int? VerifiePar { get; set; }

    public DateTime? DateNotificationDisponibilite { get; set; }

    public DateTime? DateLimiteRetrait { get; set; }

    public DateTime? DateRetraitEffectif { get; set; }

    public string? MotifRefus { get; set; }

    public string? Observations { get; set; }

    public virtual Document CodeDocumentNavigation { get; set; } = null!;

    public virtual Exemplaire? CodeExemplaireNavigation { get; set; }

    public virtual Lecteur CodeLecteurNavigation { get; set; } = null!;

    public virtual ICollection<LignesPret> LignesPrets { get; set; } = new List<LignesPret>();

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ComptesUtilisateur? VerifieParNavigation { get; set; }
}
