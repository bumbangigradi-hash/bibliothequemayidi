using System;
using System.Collections.Generic;

namespace bibliotheque_mayidi.Models.Entities;

public partial class PaiementsManquant
{
    public long CodePaiementManquant { get; set; }

    public string NumeroRecu { get; set; } = null!;

    public long CodeDossierManquant { get; set; }

    public DateTime DatePaiement { get; set; }

    public decimal Montant { get; set; }

    public string Devise { get; set; } = null!;

    /// <summary>
    /// ESPECES, VIREMENT, MOBILE_MONEY, AUTRE
    /// </summary>
    public string ModePaiement { get; set; } = null!;

    public string? ReferencePaiement { get; set; }

    public int EnregistrePar { get; set; }

    public string? Observations { get; set; }

    public virtual DossiersManquant CodeDossierManquantNavigation { get; set; } = null!;

    public virtual ComptesUtilisateur EnregistreParNavigation { get; set; } = null!;
}
