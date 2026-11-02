using System;
using System.Collections.Generic;

namespace bibliotheque_mayidi.Models.Entities;

public partial class Editeur
{
    public int CodeEditeur { get; set; }

    public string NomEditeur { get; set; } = null!;

    public string? Ville { get; set; }

    public string? Pays { get; set; }

    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
}
