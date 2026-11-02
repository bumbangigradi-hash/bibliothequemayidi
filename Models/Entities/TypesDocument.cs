using System;
using System.Collections.Generic;

namespace bibliotheque_mayidi.Models.Entities;

/// <summary>
/// Livre : 14 jours ; revue : 4 jours ; dictionnaire : consultation sur place
/// </summary>
public partial class TypesDocument
{
    public string CodeTypeDocument { get; set; } = null!;

    public string NomTypeDocument { get; set; } = null!;

    public int DureePretJours { get; set; }

    public bool EmpruntAutorise { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
}
