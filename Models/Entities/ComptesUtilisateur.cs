using System;
using System.Collections.Generic;

namespace bibliotheque_mayidi.Models.Entities;

public partial class ComptesUtilisateur
{
    public int CodeCompte { get; set; }

    public string CodeRole { get; set; } = null!;

    public string NomUtilisateur { get; set; } = null!;

    public string? Postnom { get; set; }

    public string? Prenom { get; set; }

    public string Identifiant { get; set; } = null!;

    public string? AdresseInterne { get; set; }

    public string MotDePasseHash { get; set; } = null!;

    public string? Telephone { get; set; }

    public string? EmailExterne { get; set; }

    public string? CheminPhoto { get; set; }

    /// <summary>
    /// EN_ATTENTE, ACTIF, REJETE, SUSPENDU, BLOQUE
    /// </summary>
    public string StatutCompte { get; set; } = null!;

    public bool Actif { get; set; }

    public DateTime DateCreation { get; set; }

    public DateTime? DateValidation { get; set; }

    public int? ValidePar { get; set; }

    public DateTime? DerniereConnexion { get; set; }

    public int NombreEchecsConnexion { get; set; }

    public DateTime? DateDerniereModification { get; set; }

    public virtual ICollection<BordereauxPret> BordereauxPrets { get; set; } = new List<BordereauxPret>();

    public virtual ICollection<BordereauxRetour> BordereauxRetours { get; set; } = new List<BordereauxRetour>();

    public virtual Role CodeRoleNavigation { get; set; } = null!;

    public virtual ICollection<ConsultationsSurPlace> ConsultationsSurPlaces { get; set; } = new List<ConsultationsSurPlace>();

    public virtual ICollection<DestinatairesMessage> DestinatairesMessages { get; set; } = new List<DestinatairesMessage>();

    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();

    public virtual ICollection<DossiersManquant> DossiersManquantClotureParNavigations { get; set; } = new List<DossiersManquant>();

    public virtual ICollection<DossiersManquant> DossiersManquantConfirmeParNavigations { get; set; } = new List<DossiersManquant>();

    public virtual ICollection<DossiersManquant> DossiersManquantConstateParNavigations { get; set; } = new List<DossiersManquant>();

    public virtual ICollection<Exemplaire> Exemplaires { get; set; } = new List<Exemplaire>();

    public virtual ICollection<HistoriqueConnexion> HistoriqueConnexions { get; set; } = new List<HistoriqueConnexion>();

    public virtual ICollection<IncidentsDocument> IncidentsDocumentResoluParNavigations { get; set; } = new List<IncidentsDocument>();

    public virtual ICollection<IncidentsDocument> IncidentsDocumentSignaleParNavigations { get; set; } = new List<IncidentsDocument>();

    public virtual ICollection<Inventaire> Inventaires { get; set; } = new List<Inventaire>();

    public virtual ICollection<ComptesUtilisateur> InverseValideParNavigation { get; set; } = new List<ComptesUtilisateur>();

    public virtual ICollection<JournalAudit> JournalAudits { get; set; } = new List<JournalAudit>();

    public virtual Lecteur? Lecteur { get; set; }

    public virtual ICollection<LignesInventaire> LignesInventaires { get; set; } = new List<LignesInventaire>();

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<PaiementsManquant> PaiementsManquants { get; set; } = new List<PaiementsManquant>();

    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

    public virtual ComptesUtilisateur? ValideParNavigation { get; set; }
}
