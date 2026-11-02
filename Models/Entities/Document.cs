using System;
using System.Collections.Generic;

namespace bibliotheque_mayidi.Models.Entities;

public partial class Document
{
    public int CodeDocument { get; set; }

    public string CodeTypeDocument { get; set; } = null!;

    public int? CodeCollection { get; set; }

    public int? CodeEditeur { get; set; }

    public string Titre { get; set; } = null!;

    public string? SousTitre { get; set; }

    public string? IsbnIssn { get; set; }

    public string? Edition { get; set; }

    public string? LieuPublication { get; set; }

    public int? AnneePublication { get; set; }

    public string? Langue { get; set; }

    public int? NombrePages { get; set; }

    public string? Resume { get; set; }

    public string? MotsCles { get; set; }

    public bool EmpruntAutorise { get; set; }

    public string StatutNotice { get; set; } = null!;

    public DateTime DateEncodage { get; set; }

    public int? EncodePar { get; set; }

    public DateTime? DateModification { get; set; }

    public virtual Collection? CodeCollectionNavigation { get; set; }

    public virtual Editeur? CodeEditeurNavigation { get; set; }

    public virtual TypesDocument CodeTypeDocumentNavigation { get; set; } = null!;

    public virtual ICollection<DocumentsAuteur> DocumentsAuteurs { get; set; } = new List<DocumentsAuteur>();

    public virtual ComptesUtilisateur? EncodeParNavigation { get; set; }

    public virtual ICollection<Exemplaire> Exemplaires { get; set; } = new List<Exemplaire>();

    public virtual NumerosRevue? NumerosRevue { get; set; }

    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

    public virtual ICollection<Theme> CodeThemes { get; set; } = new List<Theme>();
}
