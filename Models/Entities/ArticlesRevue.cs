using System;
using System.Collections.Generic;

namespace bibliotheque_mayidi.Models.Entities;

public partial class ArticlesRevue
{
    public int CodeArticle { get; set; }

    public int CodeNumeroRevue { get; set; }

    public string TitreArticle { get; set; } = null!;

    public int? CodeAuteur { get; set; }

    public int? PageDebut { get; set; }

    public int? PageFin { get; set; }

    public string? Resume { get; set; }

    public string? MotsCles { get; set; }

    public virtual Auteur? CodeAuteurNavigation { get; set; }

    public virtual NumerosRevue CodeNumeroRevueNavigation { get; set; } = null!;
}
