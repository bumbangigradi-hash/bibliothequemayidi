using System;
using System.Collections.Generic;

namespace bibliotheque_mayidi.Models.Entities;

public partial class LignesRetour
{
    public long CodeLigneRetour { get; set; }

    public long CodeBordereauRetour { get; set; }

    public long CodeLignePret { get; set; }

    /// <summary>
    /// BON, ABIME, INCOMPLET, AUTRE
    /// </summary>
    public string EtatRetour { get; set; } = null!;

    public string StatutApresControle { get; set; } = null!;

    public string? Observations { get; set; }

    public virtual BordereauxRetour CodeBordereauRetourNavigation { get; set; } = null!;

    public virtual LignesPret CodeLignePretNavigation { get; set; } = null!;
}
