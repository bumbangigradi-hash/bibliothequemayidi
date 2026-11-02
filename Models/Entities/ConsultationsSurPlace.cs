using System;
using System.Collections.Generic;

namespace bibliotheque_mayidi.Models.Entities;

public partial class ConsultationsSurPlace
{
    public long CodeConsultation { get; set; }

    public string NumeroConsultation { get; set; } = null!;

    public int CodeLecteur { get; set; }

    public int CodeExemplaire { get; set; }

    public DateTime DateDebut { get; set; }

    public DateTime? DateFin { get; set; }

    public int EnregistrePar { get; set; }

    public string? EtatSortie { get; set; }

    public string? EtatRetour { get; set; }

    public string? Observations { get; set; }

    public virtual Exemplaire CodeExemplaireNavigation { get; set; } = null!;

    public virtual Lecteur CodeLecteurNavigation { get; set; } = null!;

    public virtual ComptesUtilisateur EnregistreParNavigation { get; set; } = null!;
}
