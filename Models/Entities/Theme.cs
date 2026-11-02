using System;
using System.Collections.Generic;

namespace bibliotheque_mayidi.Models.Entities;

public partial class Theme
{
    public int CodeTheme { get; set; }

    public string NomTheme { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Document> CodeDocuments { get; set; } = new List<Document>();
}
