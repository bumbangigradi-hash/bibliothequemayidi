using System;
using System.Collections.Generic;

namespace bibliotheque_mayidi.Models.Entities;

public partial class Role
{
    public string CodeRole { get; set; } = null!;

    public string NomRole { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<ComptesUtilisateur> ComptesUtilisateurs { get; set; } = new List<ComptesUtilisateur>();
}
