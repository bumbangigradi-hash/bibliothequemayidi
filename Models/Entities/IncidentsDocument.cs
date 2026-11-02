using System;
using System.Collections.Generic;

namespace bibliotheque_mayidi.Models.Entities;

public partial class IncidentsDocument
{
    public long CodeIncident { get; set; }

    public string NumeroIncident { get; set; } = null!;

    public int CodeExemplaire { get; set; }

    public int? CodeLecteur { get; set; }

    public long? CodeLignePret { get; set; }

    /// <summary>
    /// PERDU, ENDOMMAGE, INTROUVABLE, MAUVAISE_COTE, AUTRE
    /// </summary>
    public string TypeIncident { get; set; } = null!;

    public DateTime DateIncident { get; set; }

    public int SignalePar { get; set; }

    public string Description { get; set; } = null!;

    public string StatutIncident { get; set; } = null!;

    public string? Resolution { get; set; }

    public int? ResoluPar { get; set; }

    public DateTime? DateResolution { get; set; }

    public virtual Exemplaire CodeExemplaireNavigation { get; set; } = null!;

    public virtual Lecteur? CodeLecteurNavigation { get; set; }

    public virtual LignesPret? CodeLignePretNavigation { get; set; }

    public virtual ComptesUtilisateur? ResoluParNavigation { get; set; }

    public virtual ComptesUtilisateur SignaleParNavigation { get; set; } = null!;
}
