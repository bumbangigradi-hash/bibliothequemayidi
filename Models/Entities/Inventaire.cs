using System;
using System.Collections.Generic;

namespace bibliotheque_mayidi.Models.Entities;

public partial class Inventaire
{
    public long CodeInventaire { get; set; }

    public string NumeroInventaireSession { get; set; } = null!;

    public string Libelle { get; set; } = null!;

    public DateTime DateDebut { get; set; }

    public DateTime? DateFin { get; set; }

    public string? CodeRayon { get; set; }

    public int Responsable { get; set; }

    public string StatutInventaire { get; set; } = null!;

    public string? Observations { get; set; }

    public virtual Rayon? CodeRayonNavigation { get; set; }

    public virtual ICollection<LignesInventaire> LignesInventaires { get; set; } = new List<LignesInventaire>();

    public virtual ComptesUtilisateur ResponsableNavigation { get; set; } = null!;
}
