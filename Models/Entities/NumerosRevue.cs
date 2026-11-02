using System;
using System.Collections.Generic;

namespace bibliotheque_mayidi.Models.Entities;

public partial class NumerosRevue
{
    public int CodeNumeroRevue { get; set; }

    public int CodeDocument { get; set; }

    public string? Volume { get; set; }

    public string? Numero { get; set; }

    public DateOnly? DateParution { get; set; }

    public string? Periode { get; set; }

    public string? IssnNumero { get; set; }

    public string? Observations { get; set; }

    public virtual ICollection<ArticlesRevue> ArticlesRevues { get; set; } = new List<ArticlesRevue>();

    public virtual Document CodeDocumentNavigation { get; set; } = null!;
}
