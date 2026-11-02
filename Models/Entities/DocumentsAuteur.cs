using System;
using System.Collections.Generic;

namespace bibliotheque_mayidi.Models.Entities;

public partial class DocumentsAuteur
{
    public int CodeDocument { get; set; }

    public int CodeAuteur { get; set; }

    public int OrdreAffichage { get; set; }

    public string? RoleAuteur { get; set; }

    public virtual Auteur CodeAuteurNavigation { get; set; } = null!;

    public virtual Document CodeDocumentNavigation { get; set; } = null!;
}
