using System;
using System.Collections.Generic;

namespace bibliotheque_mayidi.Models.Entities;

public partial class Notification
{
    public long CodeNotification { get; set; }

    public int CodeCompte { get; set; }

    public long? CodeReservation { get; set; }

    public long? CodeLignePret { get; set; }

    public long? CodeDossierManquant { get; set; }

    public string Titre { get; set; } = null!;

    public string Contenu { get; set; } = null!;

    public string TypeNotification { get; set; } = null!;

    public DateTime DateCreation { get; set; }

    public DateTime? DateLecture { get; set; }

    /// <summary>
    /// INTERNE, EMAIL si configure
    /// </summary>
    public string Canal { get; set; } = null!;

    public virtual ComptesUtilisateur CodeCompteNavigation { get; set; } = null!;

    public virtual DossiersManquant? CodeDossierManquantNavigation { get; set; }

    public virtual LignesPret? CodeLignePretNavigation { get; set; }

    public virtual Reservation? CodeReservationNavigation { get; set; }
}
