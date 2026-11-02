using System;
using System.Collections.Generic;

namespace bibliotheque_mayidi.Models.Entities;

public partial class DossiersManquant
{
    public long CodeDossierManquant { get; set; }

    public string NumeroDossier { get; set; } = null!;

    public int CodeLecteur { get; set; }

    public long CodeLignePret { get; set; }

    public int CodeExemplaire { get; set; }

    public DateTime DateConstat { get; set; }

    public int ConstatePar { get; set; }

    /// <summary>
    /// A_VERIFIER, MANQUANT_CONFIRME, OUVRAGE_RETROUVE, REMBOURSE, CLOTURE, LITIGE
    /// </summary>
    public string StatutDossier { get; set; } = null!;

    public decimal? ValeurEstimee { get; set; }

    public decimal? MontantARembourser { get; set; }

    public decimal MontantDejaPaye { get; set; }

    public decimal SoldeRestant { get; set; }

    public string Devise { get; set; } = null!;

    public DateTime? DateConfirmation { get; set; }

    public int? ConfirmePar { get; set; }

    public DateOnly? DateLimiteReglement { get; set; }

    public string? Motif { get; set; }

    public string? Observations { get; set; }

    public DateTime? DateCloture { get; set; }

    public int? CloturePar { get; set; }

    public virtual ComptesUtilisateur? ClotureParNavigation { get; set; }

    public virtual Exemplaire CodeExemplaireNavigation { get; set; } = null!;

    public virtual Lecteur CodeLecteurNavigation { get; set; } = null!;

    public virtual LignesPret CodeLignePretNavigation { get; set; } = null!;

    public virtual ComptesUtilisateur? ConfirmeParNavigation { get; set; }

    public virtual ComptesUtilisateur ConstateParNavigation { get; set; } = null!;

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<PaiementsManquant> PaiementsManquants { get; set; } = new List<PaiementsManquant>();
}
