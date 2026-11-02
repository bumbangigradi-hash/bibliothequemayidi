using System;
using System.Collections.Generic;

namespace bibliotheque_mayidi.Models.Entities;

public partial class DestinatairesMessage
{
    public long CodeMessage { get; set; }

    public int CodeCompte { get; set; }

    public DateTime DateReception { get; set; }

    public DateTime? DateLecture { get; set; }

    public bool Archive { get; set; }

    public bool SupprimeAffichage { get; set; }

    public virtual ComptesUtilisateur CodeCompteNavigation { get; set; } = null!;

    public virtual Message CodeMessageNavigation { get; set; } = null!;
}
