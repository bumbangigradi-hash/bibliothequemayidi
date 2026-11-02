using System;
using System.Collections.Generic;

namespace bibliotheque_mayidi.Models.Entities;

public partial class JournalAudit
{
    public long CodeAudit { get; set; }

    public int? CodeCompte { get; set; }

    public string Action { get; set; } = null!;

    public string? TableConcernee { get; set; }

    public string? CleEnregistrement { get; set; }

    public string? Details { get; set; }

    public DateTime DateAction { get; set; }

    public string? AdresseIp { get; set; }

    public virtual ComptesUtilisateur? CodeCompteNavigation { get; set; }
}
