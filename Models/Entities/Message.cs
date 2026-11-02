using System;
using System.Collections.Generic;

namespace bibliotheque_mayidi.Models.Entities;

public partial class Message
{
    public long CodeMessage { get; set; }

    public int? CodeExpediteur { get; set; }

    public string Sujet { get; set; } = null!;

    public string Contenu { get; set; } = null!;

    public DateTime DateEnvoi { get; set; }

    public string TypeMessage { get; set; } = null!;

    public long? CodeReservation { get; set; }

    public long? CodeLignePret { get; set; }

    public virtual ComptesUtilisateur? CodeExpediteurNavigation { get; set; }

    public virtual LignesPret? CodeLignePretNavigation { get; set; }

    public virtual Reservation? CodeReservationNavigation { get; set; }

    public virtual ICollection<DestinatairesMessage> DestinatairesMessages { get; set; } = new List<DestinatairesMessage>();
}
