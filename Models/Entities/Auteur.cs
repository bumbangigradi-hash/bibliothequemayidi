using System;
using System.Collections.Generic;

namespace bibliotheque_mayidi.Models.Entities;

public partial class Auteur
{
    public int CodeAuteur { get; set; }

    public string NomAuteur { get; set; } = null!;

    public string? PrenomAuteur { get; set; }

    public string? DatesAuteur { get; set; }

    public string? NoticeBiographique { get; set; }

    public virtual ICollection<ArticlesRevue> ArticlesRevues { get; set; } = new List<ArticlesRevue>();

    public virtual ICollection<DocumentsAuteur> DocumentsAuteurs { get; set; } = new List<DocumentsAuteur>();
}
