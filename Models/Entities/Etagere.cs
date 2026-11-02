using System;
using System.Collections.Generic;

namespace bibliotheque_mayidi.Models.Entities;

public partial class Etagere
{
    public int CodeEtagere { get; set; }

    public string CodeRayon { get; set; } = null!;

    public string NumeroEtagereRoman { get; set; } = null!;

    public string? Description { get; set; }

    public bool Actif { get; set; }

    public virtual Rayon CodeRayonNavigation { get; set; } = null!;

    public virtual ICollection<Emplacement> Emplacements { get; set; } = new List<Emplacement>();
}
