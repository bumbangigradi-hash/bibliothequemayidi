using System;
using System.Collections.Generic;

namespace bibliotheque_mayidi.Models.Entities;

public partial class Collection
{
    public int CodeCollection { get; set; }

    public string NomCollection { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
}
