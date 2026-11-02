using System;
using System.Collections.Generic;

namespace bibliotheque_mayidi.Models.Entities;

public partial class LignesInventaire
{
    public long CodeLigneInventaire { get; set; }

    public long CodeInventaire { get; set; }

    public int CodeExemplaire { get; set; }

    public bool Trouve { get; set; }

    public int? CodeEmplacementAttendu { get; set; }

    public int? CodeEmplacementConstate { get; set; }

    public string? EtatConstate { get; set; }

    public string? EcartConstate { get; set; }

    public string? Observations { get; set; }

    public int? VerifiePar { get; set; }

    public DateTime? DateVerification { get; set; }

    public virtual Emplacement? CodeEmplacementAttenduNavigation { get; set; }

    public virtual Emplacement? CodeEmplacementConstateNavigation { get; set; }

    public virtual Exemplaire CodeExemplaireNavigation { get; set; } = null!;

    public virtual Inventaire CodeInventaireNavigation { get; set; } = null!;

    public virtual ComptesUtilisateur? VerifieParNavigation { get; set; }
}
