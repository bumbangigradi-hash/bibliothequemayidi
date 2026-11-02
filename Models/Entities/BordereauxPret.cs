using System;
using System.Collections.Generic;

namespace bibliotheque_mayidi.Models.Entities;

public partial class BordereauxPret
{
    public long CodeBordereauPret { get; set; }

    public string NumeroBordereau { get; set; } = null!;

    public int CodeLecteur { get; set; }

    public DateTime DatePret { get; set; }

    public int EnregistrePar { get; set; }

    /// <summary>
    /// OUVERT, PARTIELLEMENT_RETOURNE, CLOTURE
    /// </summary>
    public string StatutBordereau { get; set; } = null!;

    public string? Observations { get; set; }

    public string? SignatureLecteur { get; set; }

    public string? SignatureAgent { get; set; }

    public virtual Lecteur CodeLecteurNavigation { get; set; } = null!;

    public virtual ComptesUtilisateur EnregistreParNavigation { get; set; } = null!;

    public virtual ICollection<LignesPret> LignesPrets { get; set; } = new List<LignesPret>();
}
