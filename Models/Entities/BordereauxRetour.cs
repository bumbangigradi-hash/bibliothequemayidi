using System;
using System.Collections.Generic;

namespace bibliotheque_mayidi.Models.Entities;

public partial class BordereauxRetour
{
    public long CodeBordereauRetour { get; set; }

    public string NumeroBordereau { get; set; } = null!;

    public int CodeLecteur { get; set; }

    public DateTime DateRetour { get; set; }

    public int EnregistrePar { get; set; }

    public string? Observations { get; set; }

    public string? SignatureLecteur { get; set; }

    public string? SignatureAgent { get; set; }

    public virtual Lecteur CodeLecteurNavigation { get; set; } = null!;

    public virtual ComptesUtilisateur EnregistreParNavigation { get; set; } = null!;

    public virtual ICollection<LignesRetour> LignesRetours { get; set; } = new List<LignesRetour>();
}
