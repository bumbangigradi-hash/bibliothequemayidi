using System;
using System.Collections.Generic;

namespace bibliotheque_mayidi.Models.Entities;

public partial class Emplacement
{
    public int CodeEmplacement { get; set; }

    public int CodeEtagere { get; set; }

    public string NumeroPlace { get; set; } = null!;

    public string? Description { get; set; }

    public bool Actif { get; set; }

    public virtual Etagere CodeEtagereNavigation { get; set; } = null!;

    public virtual ICollection<Exemplaire> Exemplaires { get; set; } = new List<Exemplaire>();

    public virtual ICollection<LignesInventaire> LignesInventaireCodeEmplacementAttenduNavigations { get; set; } = new List<LignesInventaire>();

    public virtual ICollection<LignesInventaire> LignesInventaireCodeEmplacementConstateNavigations { get; set; } = new List<LignesInventaire>();
}
