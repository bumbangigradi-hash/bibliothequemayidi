using System;
using System.Collections.Generic;

namespace bibliotheque_mayidi.Models.Entities;

public partial class HistoriqueConnexion
{
    public long CodeConnexion { get; set; }

    public int? CodeCompte { get; set; }

    public string? IdentifiantSaisi { get; set; }

    public DateTime DateTentative { get; set; }

    public bool ConnexionReussie { get; set; }

    public string? AdresseIp { get; set; }

    public string? Navigateur { get; set; }

    public string? MotifEchec { get; set; }

    public virtual ComptesUtilisateur? CodeCompteNavigation { get; set; }
}
