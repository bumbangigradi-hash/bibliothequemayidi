using System;
using System.Collections.Generic;

namespace bibliotheque_mayidi.Models.Entities;

public partial class Exemplaire
{
    public int CodeExemplaire { get; set; }

    public int CodeDocument { get; set; }

    public int CodeEmplacement { get; set; }

    public string Cote { get; set; } = null!;

    public string NumeroInventaire { get; set; } = null!;

    public string? CodeBarres { get; set; }

    public DateOnly? DateAcquisition { get; set; }

    public string? ModeAcquisition { get; set; }

    public decimal? ValeurAcquisition { get; set; }

    public string DeviseValeur { get; set; } = null!;

    public string EtatDocument { get; set; } = null!;

    /// <summary>
    /// DISPONIBLE, RESERVE, EMPRUNTE, CONSULTATION, ENDOMMAGE, PERDU, RETIRE
    /// </summary>
    public string StatutExemplaire { get; set; } = null!;

    public DateOnly? DateRetrait { get; set; }

    public string? MotifRetrait { get; set; }

    public string? Observations { get; set; }

    public DateTime DateCreation { get; set; }

    public int? EncodePar { get; set; }

    public virtual Document CodeDocumentNavigation { get; set; } = null!;

    public virtual Emplacement CodeEmplacementNavigation { get; set; } = null!;

    public virtual ICollection<ConsultationsSurPlace> ConsultationsSurPlaces { get; set; } = new List<ConsultationsSurPlace>();

    public virtual ICollection<DossiersManquant> DossiersManquants { get; set; } = new List<DossiersManquant>();

    public virtual ComptesUtilisateur? EncodeParNavigation { get; set; }

    public virtual ICollection<IncidentsDocument> IncidentsDocuments { get; set; } = new List<IncidentsDocument>();

    public virtual ICollection<LignesInventaire> LignesInventaires { get; set; } = new List<LignesInventaire>();

    public virtual LignesPret? LignesPret { get; set; }

    public virtual Reservation? Reservation { get; set; }
}
