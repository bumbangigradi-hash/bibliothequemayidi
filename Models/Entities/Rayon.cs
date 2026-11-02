using System;
using System.Collections.Generic;

namespace bibliotheque_mayidi.Models.Entities;

public partial class Rayon
{
    public string CodeRayon { get; set; } = null!;

    public string NomRayon { get; set; } = null!;

    public string? Description { get; set; }

    public bool Actif { get; set; }

    public virtual ICollection<Etagere> Etageres { get; set; } = new List<Etagere>();

    public virtual ICollection<Inventaire> Inventaires { get; set; } = new List<Inventaire>();
}
