using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using bibliotheque_mayidi.Models.Entities;

namespace bibliotheque_mayidi.Data.Generated;

public partial class BibliothequeScaffoldContext : DbContext
{
    public BibliothequeScaffoldContext(DbContextOptions<BibliothequeScaffoldContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ArticlesRevue> ArticlesRevues { get; set; }

    public virtual DbSet<Auteur> Auteurs { get; set; }

    public virtual DbSet<BordereauxPret> BordereauxPrets { get; set; }

    public virtual DbSet<BordereauxRetour> BordereauxRetours { get; set; }

    public virtual DbSet<Collection> Collections { get; set; }

    public virtual DbSet<ComptesUtilisateur> ComptesUtilisateurs { get; set; }

    public virtual DbSet<ConsultationsSurPlace> ConsultationsSurPlaces { get; set; }

    public virtual DbSet<DestinatairesMessage> DestinatairesMessages { get; set; }

    public virtual DbSet<Document> Documents { get; set; }

    public virtual DbSet<DocumentsAuteur> DocumentsAuteurs { get; set; }

    public virtual DbSet<DossiersManquant> DossiersManquants { get; set; }

    public virtual DbSet<Editeur> Editeurs { get; set; }

    public virtual DbSet<Emplacement> Emplacements { get; set; }

    public virtual DbSet<Etagere> Etageres { get; set; }

    public virtual DbSet<Exemplaire> Exemplaires { get; set; }

    public virtual DbSet<HistoriqueConnexion> HistoriqueConnexions { get; set; }

    public virtual DbSet<IncidentsDocument> IncidentsDocuments { get; set; }

    public virtual DbSet<Inventaire> Inventaires { get; set; }

    public virtual DbSet<JournalAudit> JournalAudits { get; set; }

    public virtual DbSet<Lecteur> Lecteurs { get; set; }

    public virtual DbSet<LignesInventaire> LignesInventaires { get; set; }

    public virtual DbSet<LignesPret> LignesPrets { get; set; }

    public virtual DbSet<LignesRetour> LignesRetours { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<NumerosRevue> NumerosRevues { get; set; }

    public virtual DbSet<PaiementsManquant> PaiementsManquants { get; set; }

    public virtual DbSet<Rayon> Rayons { get; set; }

    public virtual DbSet<Reservation> Reservations { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Theme> Themes { get; set; }

    public virtual DbSet<TypesDocument> TypesDocuments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ArticlesRevue>(entity =>
        {
            entity.HasKey(e => e.CodeArticle).HasName("ARTICLES_REVUES_pkey");

            entity.ToTable("ARTICLES_REVUES");

            entity.HasIndex(e => new { e.CodeNumeroRevue, e.TitreArticle }, "ARTICLES_REVUES_code_numero_revue_titre_article_idx");

            entity.Property(e => e.CodeArticle).HasColumnName("code_article");
            entity.Property(e => e.CodeAuteur).HasColumnName("code_auteur");
            entity.Property(e => e.CodeNumeroRevue).HasColumnName("code_numero_revue");
            entity.Property(e => e.MotsCles).HasColumnName("mots_cles");
            entity.Property(e => e.PageDebut).HasColumnName("page_debut");
            entity.Property(e => e.PageFin).HasColumnName("page_fin");
            entity.Property(e => e.Resume).HasColumnName("resume");
            entity.Property(e => e.TitreArticle)
                .HasMaxLength(500)
                .HasColumnName("titre_article");

            entity.HasOne(d => d.CodeAuteurNavigation).WithMany(p => p.ArticlesRevues)
                .HasForeignKey(d => d.CodeAuteur)
                .HasConstraintName("ARTICLES_REVUES_code_auteur_fkey");

            entity.HasOne(d => d.CodeNumeroRevueNavigation).WithMany(p => p.ArticlesRevues)
                .HasForeignKey(d => d.CodeNumeroRevue)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ARTICLES_REVUES_code_numero_revue_fkey");
        });

        modelBuilder.Entity<Auteur>(entity =>
        {
            entity.HasKey(e => e.CodeAuteur).HasName("AUTEURS_pkey");

            entity.ToTable("AUTEURS");

            entity.Property(e => e.CodeAuteur).HasColumnName("code_auteur");
            entity.Property(e => e.DatesAuteur)
                .HasMaxLength(60)
                .HasColumnName("dates_auteur");
            entity.Property(e => e.NomAuteur)
                .HasMaxLength(120)
                .HasColumnName("nom_auteur");
            entity.Property(e => e.NoticeBiographique).HasColumnName("notice_biographique");
            entity.Property(e => e.PrenomAuteur)
                .HasMaxLength(120)
                .HasColumnName("prenom_auteur");
        });

        modelBuilder.Entity<BordereauxPret>(entity =>
        {
            entity.HasKey(e => e.CodeBordereauPret).HasName("BORDEREAUX_PRETS_pkey");

            entity.ToTable("BORDEREAUX_PRETS");

            entity.HasIndex(e => e.NumeroBordereau, "BORDEREAUX_PRETS_numero_bordereau_key").IsUnique();

            entity.HasIndex(e => e.CodeLecteur, "IX_BORDEREAUX_PRETS_LECTEUR");

            entity.Property(e => e.CodeBordereauPret).HasColumnName("code_bordereau_pret");
            entity.Property(e => e.CodeLecteur).HasColumnName("code_lecteur");
            entity.Property(e => e.DatePret)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date_pret");
            entity.Property(e => e.EnregistrePar).HasColumnName("enregistre_par");
            entity.Property(e => e.NumeroBordereau)
                .HasMaxLength(50)
                .HasColumnName("numero_bordereau");
            entity.Property(e => e.Observations).HasColumnName("observations");
            entity.Property(e => e.SignatureAgent)
                .HasMaxLength(500)
                .HasColumnName("signature_agent");
            entity.Property(e => e.SignatureLecteur)
                .HasMaxLength(500)
                .HasColumnName("signature_lecteur");
            entity.Property(e => e.StatutBordereau)
                .HasMaxLength(30)
                .HasDefaultValueSql("'OUVERT'::character varying")
                .HasComment("OUVERT, PARTIELLEMENT_RETOURNE, CLOTURE")
                .HasColumnName("statut_bordereau");

            entity.HasOne(d => d.CodeLecteurNavigation).WithMany(p => p.BordereauxPrets)
                .HasForeignKey(d => d.CodeLecteur)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("BORDEREAUX_PRETS_code_lecteur_fkey");

            entity.HasOne(d => d.EnregistreParNavigation).WithMany(p => p.BordereauxPrets)
                .HasForeignKey(d => d.EnregistrePar)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("BORDEREAUX_PRETS_enregistre_par_fkey");
        });

        modelBuilder.Entity<BordereauxRetour>(entity =>
        {
            entity.HasKey(e => e.CodeBordereauRetour).HasName("BORDEREAUX_RETOURS_pkey");

            entity.ToTable("BORDEREAUX_RETOURS");

            entity.HasIndex(e => e.NumeroBordereau, "BORDEREAUX_RETOURS_numero_bordereau_key").IsUnique();

            entity.HasIndex(e => e.CodeLecteur, "IX_BORDEREAUX_RETOURS_LECTEUR");

            entity.Property(e => e.CodeBordereauRetour).HasColumnName("code_bordereau_retour");
            entity.Property(e => e.CodeLecteur).HasColumnName("code_lecteur");
            entity.Property(e => e.DateRetour)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date_retour");
            entity.Property(e => e.EnregistrePar).HasColumnName("enregistre_par");
            entity.Property(e => e.NumeroBordereau)
                .HasMaxLength(50)
                .HasColumnName("numero_bordereau");
            entity.Property(e => e.Observations).HasColumnName("observations");
            entity.Property(e => e.SignatureAgent)
                .HasMaxLength(500)
                .HasColumnName("signature_agent");
            entity.Property(e => e.SignatureLecteur)
                .HasMaxLength(500)
                .HasColumnName("signature_lecteur");

            entity.HasOne(d => d.CodeLecteurNavigation).WithMany(p => p.BordereauxRetours)
                .HasForeignKey(d => d.CodeLecteur)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("BORDEREAUX_RETOURS_code_lecteur_fkey");

            entity.HasOne(d => d.EnregistreParNavigation).WithMany(p => p.BordereauxRetours)
                .HasForeignKey(d => d.EnregistrePar)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("BORDEREAUX_RETOURS_enregistre_par_fkey");
        });

        modelBuilder.Entity<Collection>(entity =>
        {
            entity.HasKey(e => e.CodeCollection).HasName("COLLECTIONS_pkey");

            entity.ToTable("COLLECTIONS");

            entity.HasIndex(e => e.NomCollection, "COLLECTIONS_nom_collection_key").IsUnique();

            entity.Property(e => e.CodeCollection).HasColumnName("code_collection");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.NomCollection)
                .HasMaxLength(150)
                .HasColumnName("nom_collection");
        });

        modelBuilder.Entity<ComptesUtilisateur>(entity =>
        {
            entity.HasKey(e => e.CodeCompte).HasName("COMPTES_UTILISATEURS_pkey");

            entity.ToTable("COMPTES_UTILISATEURS");

            entity.HasIndex(e => e.AdresseInterne, "COMPTES_UTILISATEURS_adresse_interne_key").IsUnique();

            entity.HasIndex(e => e.Identifiant, "COMPTES_UTILISATEURS_identifiant_key").IsUnique();

            entity.Property(e => e.CodeCompte).HasColumnName("code_compte");
            entity.Property(e => e.Actif)
                .HasDefaultValue(true)
                .HasColumnName("actif");
            entity.Property(e => e.AdresseInterne)
                .HasMaxLength(180)
                .HasColumnName("adresse_interne");
            entity.Property(e => e.CheminPhoto)
                .HasMaxLength(500)
                .HasColumnName("chemin_photo");
            entity.Property(e => e.CodeRole)
                .HasMaxLength(30)
                .HasColumnName("code_role");
            entity.Property(e => e.DateCreation)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date_creation");
            entity.Property(e => e.DateDerniereModification)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date_derniere_modification");
            entity.Property(e => e.DateValidation)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date_validation");
            entity.Property(e => e.DerniereConnexion)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("derniere_connexion");
            entity.Property(e => e.EmailExterne)
                .HasMaxLength(180)
                .HasColumnName("email_externe");
            entity.Property(e => e.Identifiant)
                .HasMaxLength(100)
                .HasColumnName("identifiant");
            entity.Property(e => e.MotDePasseHash)
                .HasMaxLength(255)
                .HasColumnName("mot_de_passe_hash");
            entity.Property(e => e.NomUtilisateur)
                .HasMaxLength(100)
                .HasColumnName("nom_utilisateur");
            entity.Property(e => e.NombreEchecsConnexion).HasColumnName("nombre_echecs_connexion");
            entity.Property(e => e.Postnom)
                .HasMaxLength(100)
                .HasColumnName("postnom");
            entity.Property(e => e.Prenom)
                .HasMaxLength(100)
                .HasColumnName("prenom");
            entity.Property(e => e.StatutCompte)
                .HasMaxLength(30)
                .HasDefaultValueSql("'EN_ATTENTE'::character varying")
                .HasComment("EN_ATTENTE, ACTIF, REJETE, SUSPENDU, BLOQUE")
                .HasColumnName("statut_compte");
            entity.Property(e => e.Telephone)
                .HasMaxLength(30)
                .HasColumnName("telephone");
            entity.Property(e => e.ValidePar).HasColumnName("valide_par");

            entity.HasOne(d => d.CodeRoleNavigation).WithMany(p => p.ComptesUtilisateurs)
                .HasForeignKey(d => d.CodeRole)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("COMPTES_UTILISATEURS_code_role_fkey");

            entity.HasOne(d => d.ValideParNavigation).WithMany(p => p.InverseValideParNavigation)
                .HasForeignKey(d => d.ValidePar)
                .HasConstraintName("COMPTES_UTILISATEURS_valide_par_fkey");
        });

        modelBuilder.Entity<ConsultationsSurPlace>(entity =>
        {
            entity.HasKey(e => e.CodeConsultation).HasName("CONSULTATIONS_SUR_PLACE_pkey");

            entity.ToTable("CONSULTATIONS_SUR_PLACE");

            entity.HasIndex(e => e.NumeroConsultation, "CONSULTATIONS_SUR_PLACE_numero_consultation_key").IsUnique();

            entity.Property(e => e.CodeConsultation).HasColumnName("code_consultation");
            entity.Property(e => e.CodeExemplaire).HasColumnName("code_exemplaire");
            entity.Property(e => e.CodeLecteur).HasColumnName("code_lecteur");
            entity.Property(e => e.DateDebut)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date_debut");
            entity.Property(e => e.DateFin)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date_fin");
            entity.Property(e => e.EnregistrePar).HasColumnName("enregistre_par");
            entity.Property(e => e.EtatRetour)
                .HasMaxLength(30)
                .HasColumnName("etat_retour");
            entity.Property(e => e.EtatSortie)
                .HasMaxLength(30)
                .HasColumnName("etat_sortie");
            entity.Property(e => e.NumeroConsultation)
                .HasMaxLength(50)
                .HasColumnName("numero_consultation");
            entity.Property(e => e.Observations).HasColumnName("observations");

            entity.HasOne(d => d.CodeExemplaireNavigation).WithMany(p => p.ConsultationsSurPlaces)
                .HasForeignKey(d => d.CodeExemplaire)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("CONSULTATIONS_SUR_PLACE_code_exemplaire_fkey");

            entity.HasOne(d => d.CodeLecteurNavigation).WithMany(p => p.ConsultationsSurPlaces)
                .HasForeignKey(d => d.CodeLecteur)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("CONSULTATIONS_SUR_PLACE_code_lecteur_fkey");

            entity.HasOne(d => d.EnregistreParNavigation).WithMany(p => p.ConsultationsSurPlaces)
                .HasForeignKey(d => d.EnregistrePar)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("CONSULTATIONS_SUR_PLACE_enregistre_par_fkey");
        });

        modelBuilder.Entity<DestinatairesMessage>(entity =>
        {
            entity.HasKey(e => new { e.CodeMessage, e.CodeCompte }).HasName("DESTINATAIRES_MESSAGES_pkey");

            entity.ToTable("DESTINATAIRES_MESSAGES");

            entity.Property(e => e.CodeMessage).HasColumnName("code_message");
            entity.Property(e => e.CodeCompte).HasColumnName("code_compte");
            entity.Property(e => e.Archive).HasColumnName("archive");
            entity.Property(e => e.DateLecture)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date_lecture");
            entity.Property(e => e.DateReception)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date_reception");
            entity.Property(e => e.SupprimeAffichage).HasColumnName("supprime_affichage");

            entity.HasOne(d => d.CodeCompteNavigation).WithMany(p => p.DestinatairesMessages)
                .HasForeignKey(d => d.CodeCompte)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("DESTINATAIRES_MESSAGES_code_compte_fkey");

            entity.HasOne(d => d.CodeMessageNavigation).WithMany(p => p.DestinatairesMessages)
                .HasForeignKey(d => d.CodeMessage)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("DESTINATAIRES_MESSAGES_code_message_fkey");
        });

        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(e => e.CodeDocument).HasName("DOCUMENTS_pkey");

            entity.ToTable("DOCUMENTS");

            entity.Property(e => e.CodeDocument).HasColumnName("code_document");
            entity.Property(e => e.AnneePublication).HasColumnName("annee_publication");
            entity.Property(e => e.CodeCollection).HasColumnName("code_collection");
            entity.Property(e => e.CodeEditeur).HasColumnName("code_editeur");
            entity.Property(e => e.CodeTypeDocument)
                .HasMaxLength(30)
                .HasColumnName("code_type_document");
            entity.Property(e => e.DateEncodage)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date_encodage");
            entity.Property(e => e.DateModification)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date_modification");
            entity.Property(e => e.Edition)
                .HasMaxLength(120)
                .HasColumnName("edition");
            entity.Property(e => e.EmpruntAutorise)
                .HasDefaultValue(true)
                .HasColumnName("emprunt_autorise");
            entity.Property(e => e.EncodePar).HasColumnName("encode_par");
            entity.Property(e => e.IsbnIssn)
                .HasMaxLength(40)
                .HasColumnName("isbn_issn");
            entity.Property(e => e.Langue)
                .HasMaxLength(80)
                .HasColumnName("langue");
            entity.Property(e => e.LieuPublication)
                .HasMaxLength(150)
                .HasColumnName("lieu_publication");
            entity.Property(e => e.MotsCles).HasColumnName("mots_cles");
            entity.Property(e => e.NombrePages).HasColumnName("nombre_pages");
            entity.Property(e => e.Resume).HasColumnName("resume");
            entity.Property(e => e.SousTitre)
                .HasMaxLength(500)
                .HasColumnName("sous_titre");
            entity.Property(e => e.StatutNotice)
                .HasMaxLength(30)
                .HasDefaultValueSql("'ACTIF'::character varying")
                .HasColumnName("statut_notice");
            entity.Property(e => e.Titre)
                .HasMaxLength(500)
                .HasColumnName("titre");

            entity.HasOne(d => d.CodeCollectionNavigation).WithMany(p => p.Documents)
                .HasForeignKey(d => d.CodeCollection)
                .HasConstraintName("DOCUMENTS_code_collection_fkey");

            entity.HasOne(d => d.CodeEditeurNavigation).WithMany(p => p.Documents)
                .HasForeignKey(d => d.CodeEditeur)
                .HasConstraintName("DOCUMENTS_code_editeur_fkey");

            entity.HasOne(d => d.CodeTypeDocumentNavigation).WithMany(p => p.Documents)
                .HasForeignKey(d => d.CodeTypeDocument)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("DOCUMENTS_code_type_document_fkey");

            entity.HasOne(d => d.EncodeParNavigation).WithMany(p => p.Documents)
                .HasForeignKey(d => d.EncodePar)
                .HasConstraintName("DOCUMENTS_encode_par_fkey");

            entity.HasMany(d => d.CodeThemes).WithMany(p => p.CodeDocuments)
                .UsingEntity<Dictionary<string, object>>(
                    "DocumentsTheme",
                    r => r.HasOne<Theme>().WithMany()
                        .HasForeignKey("CodeTheme")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("DOCUMENTS_THEMES_code_theme_fkey"),
                    l => l.HasOne<Document>().WithMany()
                        .HasForeignKey("CodeDocument")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("DOCUMENTS_THEMES_code_document_fkey"),
                    j =>
                    {
                        j.HasKey("CodeDocument", "CodeTheme").HasName("DOCUMENTS_THEMES_pkey");
                        j.ToTable("DOCUMENTS_THEMES");
                        j.IndexerProperty<int>("CodeDocument").HasColumnName("code_document");
                        j.IndexerProperty<int>("CodeTheme").HasColumnName("code_theme");
                    });
        });

        modelBuilder.Entity<DocumentsAuteur>(entity =>
        {
            entity.HasKey(e => new { e.CodeDocument, e.CodeAuteur }).HasName("DOCUMENTS_AUTEURS_pkey");

            entity.ToTable("DOCUMENTS_AUTEURS");

            entity.Property(e => e.CodeDocument).HasColumnName("code_document");
            entity.Property(e => e.CodeAuteur).HasColumnName("code_auteur");
            entity.Property(e => e.OrdreAffichage)
                .HasDefaultValue(1)
                .HasColumnName("ordre_affichage");
            entity.Property(e => e.RoleAuteur)
                .HasMaxLength(50)
                .HasDefaultValueSql("'AUTEUR'::character varying")
                .HasColumnName("role_auteur");

            entity.HasOne(d => d.CodeAuteurNavigation).WithMany(p => p.DocumentsAuteurs)
                .HasForeignKey(d => d.CodeAuteur)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("DOCUMENTS_AUTEURS_code_auteur_fkey");

            entity.HasOne(d => d.CodeDocumentNavigation).WithMany(p => p.DocumentsAuteurs)
                .HasForeignKey(d => d.CodeDocument)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("DOCUMENTS_AUTEURS_code_document_fkey");
        });

        modelBuilder.Entity<DossiersManquant>(entity =>
        {
            entity.HasKey(e => e.CodeDossierManquant).HasName("DOSSIERS_MANQUANTS_pkey");

            entity.ToTable("DOSSIERS_MANQUANTS");

            entity.HasIndex(e => new { e.CodeLignePret, e.CodeExemplaire }, "DOSSIERS_MANQUANTS_code_ligne_pret_code_exemplaire_idx").IsUnique();

            entity.HasIndex(e => e.NumeroDossier, "DOSSIERS_MANQUANTS_numero_dossier_key").IsUnique();

            entity.HasIndex(e => e.CodeLignePret, "UX_DOSSIERS_MANQUANTS_PRET_OUVERT")
                .IsUnique()
                .HasFilter("(date_cloture IS NULL)");

            entity.Property(e => e.CodeDossierManquant).HasColumnName("code_dossier_manquant");
            entity.Property(e => e.CloturePar).HasColumnName("cloture_par");
            entity.Property(e => e.CodeExemplaire).HasColumnName("code_exemplaire");
            entity.Property(e => e.CodeLecteur).HasColumnName("code_lecteur");
            entity.Property(e => e.CodeLignePret).HasColumnName("code_ligne_pret");
            entity.Property(e => e.ConfirmePar).HasColumnName("confirme_par");
            entity.Property(e => e.ConstatePar).HasColumnName("constate_par");
            entity.Property(e => e.DateCloture)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date_cloture");
            entity.Property(e => e.DateConfirmation)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date_confirmation");
            entity.Property(e => e.DateConstat)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date_constat");
            entity.Property(e => e.DateLimiteReglement).HasColumnName("date_limite_reglement");
            entity.Property(e => e.Devise)
                .HasMaxLength(10)
                .HasDefaultValueSql("'CDF'::character varying")
                .HasColumnName("devise");
            entity.Property(e => e.MontantARembourser)
                .HasPrecision(14, 2)
                .HasColumnName("montant_a_rembourser");
            entity.Property(e => e.MontantDejaPaye)
                .HasPrecision(14, 2)
                .HasColumnName("montant_deja_paye");
            entity.Property(e => e.Motif).HasColumnName("motif");
            entity.Property(e => e.NumeroDossier)
                .HasMaxLength(50)
                .HasColumnName("numero_dossier");
            entity.Property(e => e.Observations).HasColumnName("observations");
            entity.Property(e => e.SoldeRestant)
                .HasPrecision(14, 2)
                .HasColumnName("solde_restant");
            entity.Property(e => e.StatutDossier)
                .HasMaxLength(30)
                .HasDefaultValueSql("'A_VERIFIER'::character varying")
                .HasComment("A_VERIFIER, MANQUANT_CONFIRME, OUVRAGE_RETROUVE, REMBOURSE, CLOTURE, LITIGE")
                .HasColumnName("statut_dossier");
            entity.Property(e => e.ValeurEstimee)
                .HasPrecision(14, 2)
                .HasColumnName("valeur_estimee");

            entity.HasOne(d => d.ClotureParNavigation).WithMany(p => p.DossiersManquantClotureParNavigations)
                .HasForeignKey(d => d.CloturePar)
                .HasConstraintName("DOSSIERS_MANQUANTS_cloture_par_fkey");

            entity.HasOne(d => d.CodeExemplaireNavigation).WithMany(p => p.DossiersManquants)
                .HasForeignKey(d => d.CodeExemplaire)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("DOSSIERS_MANQUANTS_code_exemplaire_fkey");

            entity.HasOne(d => d.CodeLecteurNavigation).WithMany(p => p.DossiersManquants)
                .HasForeignKey(d => d.CodeLecteur)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("DOSSIERS_MANQUANTS_code_lecteur_fkey");

            entity.HasOne(d => d.CodeLignePretNavigation).WithOne(p => p.DossiersManquant)
                .HasForeignKey<DossiersManquant>(d => d.CodeLignePret)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("DOSSIERS_MANQUANTS_code_ligne_pret_fkey");

            entity.HasOne(d => d.ConfirmeParNavigation).WithMany(p => p.DossiersManquantConfirmeParNavigations)
                .HasForeignKey(d => d.ConfirmePar)
                .HasConstraintName("DOSSIERS_MANQUANTS_confirme_par_fkey");

            entity.HasOne(d => d.ConstateParNavigation).WithMany(p => p.DossiersManquantConstateParNavigations)
                .HasForeignKey(d => d.ConstatePar)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("DOSSIERS_MANQUANTS_constate_par_fkey");
        });

        modelBuilder.Entity<Editeur>(entity =>
        {
            entity.HasKey(e => e.CodeEditeur).HasName("EDITEURS_pkey");

            entity.ToTable("EDITEURS");

            entity.HasIndex(e => e.NomEditeur, "EDITEURS_nom_editeur_key").IsUnique();

            entity.Property(e => e.CodeEditeur).HasColumnName("code_editeur");
            entity.Property(e => e.NomEditeur)
                .HasMaxLength(180)
                .HasColumnName("nom_editeur");
            entity.Property(e => e.Pays)
                .HasMaxLength(120)
                .HasColumnName("pays");
            entity.Property(e => e.Ville)
                .HasMaxLength(120)
                .HasColumnName("ville");
        });

        modelBuilder.Entity<Emplacement>(entity =>
        {
            entity.HasKey(e => e.CodeEmplacement).HasName("EMPLACEMENTS_pkey");

            entity.ToTable("EMPLACEMENTS");

            entity.HasIndex(e => new { e.CodeEtagere, e.NumeroPlace }, "EMPLACEMENTS_code_etagere_numero_place_idx").IsUnique();

            entity.HasIndex(e => new { e.CodeEtagere, e.NumeroPlace }, "uq_emplacements_etagere_place").IsUnique();

            entity.Property(e => e.CodeEmplacement).HasColumnName("code_emplacement");
            entity.Property(e => e.Actif)
                .HasDefaultValue(true)
                .HasColumnName("actif");
            entity.Property(e => e.CodeEtagere).HasColumnName("code_etagere");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.NumeroPlace)
                .HasMaxLength(20)
                .HasColumnName("numero_place");

            entity.HasOne(d => d.CodeEtagereNavigation).WithMany(p => p.Emplacements)
                .HasForeignKey(d => d.CodeEtagere)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("EMPLACEMENTS_code_etagere_fkey");
        });

        modelBuilder.Entity<Etagere>(entity =>
        {
            entity.HasKey(e => e.CodeEtagere).HasName("ETAGERES_pkey");

            entity.ToTable("ETAGERES");

            entity.HasIndex(e => new { e.CodeRayon, e.NumeroEtagereRoman }, "ETAGERES_code_rayon_numero_etagere_roman_idx").IsUnique();

            entity.HasIndex(e => new { e.CodeRayon, e.NumeroEtagereRoman }, "uq_etageres_rayon_numero").IsUnique();

            entity.Property(e => e.CodeEtagere).HasColumnName("code_etagere");
            entity.Property(e => e.Actif)
                .HasDefaultValue(true)
                .HasColumnName("actif");
            entity.Property(e => e.CodeRayon)
                .HasMaxLength(30)
                .HasColumnName("code_rayon");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.NumeroEtagereRoman)
                .HasMaxLength(20)
                .HasColumnName("numero_etagere_roman");

            entity.HasOne(d => d.CodeRayonNavigation).WithMany(p => p.Etageres)
                .HasForeignKey(d => d.CodeRayon)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ETAGERES_code_rayon_fkey");
        });

        modelBuilder.Entity<Exemplaire>(entity =>
        {
            entity.HasKey(e => e.CodeExemplaire).HasName("EXEMPLAIRES_pkey");

            entity.ToTable("EXEMPLAIRES");

            entity.HasIndex(e => e.CodeBarres, "EXEMPLAIRES_code_barres_key").IsUnique();

            entity.HasIndex(e => e.Cote, "EXEMPLAIRES_cote_key").IsUnique();

            entity.HasIndex(e => e.NumeroInventaire, "EXEMPLAIRES_numero_inventaire_key").IsUnique();

            entity.Property(e => e.CodeExemplaire).HasColumnName("code_exemplaire");
            entity.Property(e => e.CodeBarres)
                .HasMaxLength(100)
                .HasColumnName("code_barres");
            entity.Property(e => e.CodeDocument).HasColumnName("code_document");
            entity.Property(e => e.CodeEmplacement).HasColumnName("code_emplacement");
            entity.Property(e => e.Cote)
                .HasMaxLength(100)
                .HasColumnName("cote");
            entity.Property(e => e.DateAcquisition).HasColumnName("date_acquisition");
            entity.Property(e => e.DateCreation)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date_creation");
            entity.Property(e => e.DateRetrait).HasColumnName("date_retrait");
            entity.Property(e => e.DeviseValeur)
                .HasMaxLength(10)
                .HasDefaultValueSql("'CDF'::character varying")
                .HasColumnName("devise_valeur");
            entity.Property(e => e.EncodePar).HasColumnName("encode_par");
            entity.Property(e => e.EtatDocument)
                .HasMaxLength(30)
                .HasDefaultValueSql("'BON'::character varying")
                .HasColumnName("etat_document");
            entity.Property(e => e.ModeAcquisition)
                .HasMaxLength(50)
                .HasColumnName("mode_acquisition");
            entity.Property(e => e.MotifRetrait).HasColumnName("motif_retrait");
            entity.Property(e => e.NumeroInventaire)
                .HasMaxLength(80)
                .HasColumnName("numero_inventaire");
            entity.Property(e => e.Observations).HasColumnName("observations");
            entity.Property(e => e.StatutExemplaire)
                .HasMaxLength(30)
                .HasDefaultValueSql("'DISPONIBLE'::character varying")
                .HasComment("DISPONIBLE, RESERVE, EMPRUNTE, CONSULTATION, ENDOMMAGE, PERDU, RETIRE")
                .HasColumnName("statut_exemplaire");
            entity.Property(e => e.ValeurAcquisition)
                .HasPrecision(14, 2)
                .HasColumnName("valeur_acquisition");

            entity.HasOne(d => d.CodeDocumentNavigation).WithMany(p => p.Exemplaires)
                .HasForeignKey(d => d.CodeDocument)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("EXEMPLAIRES_code_document_fkey");

            entity.HasOne(d => d.CodeEmplacementNavigation).WithMany(p => p.Exemplaires)
                .HasForeignKey(d => d.CodeEmplacement)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("EXEMPLAIRES_code_emplacement_fkey");

            entity.HasOne(d => d.EncodeParNavigation).WithMany(p => p.Exemplaires)
                .HasForeignKey(d => d.EncodePar)
                .HasConstraintName("EXEMPLAIRES_encode_par_fkey");
        });

        modelBuilder.Entity<HistoriqueConnexion>(entity =>
        {
            entity.HasKey(e => e.CodeConnexion).HasName("HISTORIQUE_CONNEXIONS_pkey");

            entity.ToTable("HISTORIQUE_CONNEXIONS");

            entity.Property(e => e.CodeConnexion).HasColumnName("code_connexion");
            entity.Property(e => e.AdresseIp)
                .HasMaxLength(50)
                .HasColumnName("adresse_ip");
            entity.Property(e => e.CodeCompte).HasColumnName("code_compte");
            entity.Property(e => e.ConnexionReussie).HasColumnName("connexion_reussie");
            entity.Property(e => e.DateTentative)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date_tentative");
            entity.Property(e => e.IdentifiantSaisi)
                .HasMaxLength(100)
                .HasColumnName("identifiant_saisi");
            entity.Property(e => e.MotifEchec)
                .HasMaxLength(255)
                .HasColumnName("motif_echec");
            entity.Property(e => e.Navigateur).HasColumnName("navigateur");

            entity.HasOne(d => d.CodeCompteNavigation).WithMany(p => p.HistoriqueConnexions)
                .HasForeignKey(d => d.CodeCompte)
                .HasConstraintName("HISTORIQUE_CONNEXIONS_code_compte_fkey");
        });

        modelBuilder.Entity<IncidentsDocument>(entity =>
        {
            entity.HasKey(e => e.CodeIncident).HasName("INCIDENTS_DOCUMENTS_pkey");

            entity.ToTable("INCIDENTS_DOCUMENTS");

            entity.HasIndex(e => e.NumeroIncident, "INCIDENTS_DOCUMENTS_numero_incident_key").IsUnique();

            entity.Property(e => e.CodeIncident).HasColumnName("code_incident");
            entity.Property(e => e.CodeExemplaire).HasColumnName("code_exemplaire");
            entity.Property(e => e.CodeLecteur).HasColumnName("code_lecteur");
            entity.Property(e => e.CodeLignePret).HasColumnName("code_ligne_pret");
            entity.Property(e => e.DateIncident)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date_incident");
            entity.Property(e => e.DateResolution)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date_resolution");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.NumeroIncident)
                .HasMaxLength(50)
                .HasColumnName("numero_incident");
            entity.Property(e => e.ResoluPar).HasColumnName("resolu_par");
            entity.Property(e => e.Resolution).HasColumnName("resolution");
            entity.Property(e => e.SignalePar).HasColumnName("signale_par");
            entity.Property(e => e.StatutIncident)
                .HasMaxLength(30)
                .HasDefaultValueSql("'OUVERT'::character varying")
                .HasColumnName("statut_incident");
            entity.Property(e => e.TypeIncident)
                .HasMaxLength(40)
                .HasComment("PERDU, ENDOMMAGE, INTROUVABLE, MAUVAISE_COTE, AUTRE")
                .HasColumnName("type_incident");

            entity.HasOne(d => d.CodeExemplaireNavigation).WithMany(p => p.IncidentsDocuments)
                .HasForeignKey(d => d.CodeExemplaire)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("INCIDENTS_DOCUMENTS_code_exemplaire_fkey");

            entity.HasOne(d => d.CodeLecteurNavigation).WithMany(p => p.IncidentsDocuments)
                .HasForeignKey(d => d.CodeLecteur)
                .HasConstraintName("INCIDENTS_DOCUMENTS_code_lecteur_fkey");

            entity.HasOne(d => d.CodeLignePretNavigation).WithMany(p => p.IncidentsDocuments)
                .HasForeignKey(d => d.CodeLignePret)
                .HasConstraintName("INCIDENTS_DOCUMENTS_code_ligne_pret_fkey");

            entity.HasOne(d => d.ResoluParNavigation).WithMany(p => p.IncidentsDocumentResoluParNavigations)
                .HasForeignKey(d => d.ResoluPar)
                .HasConstraintName("INCIDENTS_DOCUMENTS_resolu_par_fkey");

            entity.HasOne(d => d.SignaleParNavigation).WithMany(p => p.IncidentsDocumentSignaleParNavigations)
                .HasForeignKey(d => d.SignalePar)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("INCIDENTS_DOCUMENTS_signale_par_fkey");
        });

        modelBuilder.Entity<Inventaire>(entity =>
        {
            entity.HasKey(e => e.CodeInventaire).HasName("INVENTAIRES_pkey");

            entity.ToTable("INVENTAIRES");

            entity.HasIndex(e => e.NumeroInventaireSession, "INVENTAIRES_numero_inventaire_session_key").IsUnique();

            entity.Property(e => e.CodeInventaire).HasColumnName("code_inventaire");
            entity.Property(e => e.CodeRayon)
                .HasMaxLength(30)
                .HasColumnName("code_rayon");
            entity.Property(e => e.DateDebut)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date_debut");
            entity.Property(e => e.DateFin)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date_fin");
            entity.Property(e => e.Libelle)
                .HasMaxLength(180)
                .HasColumnName("libelle");
            entity.Property(e => e.NumeroInventaireSession)
                .HasMaxLength(50)
                .HasColumnName("numero_inventaire_session");
            entity.Property(e => e.Observations).HasColumnName("observations");
            entity.Property(e => e.Responsable).HasColumnName("responsable");
            entity.Property(e => e.StatutInventaire)
                .HasMaxLength(30)
                .HasDefaultValueSql("'EN_COURS'::character varying")
                .HasColumnName("statut_inventaire");

            entity.HasOne(d => d.CodeRayonNavigation).WithMany(p => p.Inventaires)
                .HasForeignKey(d => d.CodeRayon)
                .HasConstraintName("INVENTAIRES_code_rayon_fkey");

            entity.HasOne(d => d.ResponsableNavigation).WithMany(p => p.Inventaires)
                .HasForeignKey(d => d.Responsable)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("INVENTAIRES_responsable_fkey");
        });

        modelBuilder.Entity<JournalAudit>(entity =>
        {
            entity.HasKey(e => e.CodeAudit).HasName("JOURNAL_AUDIT_pkey");

            entity.ToTable("JOURNAL_AUDIT");

            entity.Property(e => e.CodeAudit).HasColumnName("code_audit");
            entity.Property(e => e.Action)
                .HasMaxLength(100)
                .HasColumnName("action");
            entity.Property(e => e.AdresseIp)
                .HasMaxLength(50)
                .HasColumnName("adresse_ip");
            entity.Property(e => e.CleEnregistrement)
                .HasMaxLength(100)
                .HasColumnName("cle_enregistrement");
            entity.Property(e => e.CodeCompte).HasColumnName("code_compte");
            entity.Property(e => e.DateAction)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date_action");
            entity.Property(e => e.Details).HasColumnName("details");
            entity.Property(e => e.TableConcernee)
                .HasMaxLength(100)
                .HasColumnName("table_concernee");

            entity.HasOne(d => d.CodeCompteNavigation).WithMany(p => p.JournalAudits)
                .HasForeignKey(d => d.CodeCompte)
                .HasConstraintName("JOURNAL_AUDIT_code_compte_fkey");
        });

        modelBuilder.Entity<Lecteur>(entity =>
        {
            entity.HasKey(e => e.CodeLecteur).HasName("LECTEURS_pkey");

            entity.ToTable("LECTEURS");

            entity.HasIndex(e => e.CodeCompte, "LECTEURS_code_compte_key").IsUnique();

            entity.HasIndex(e => e.MatriculeSeminaire, "LECTEURS_matricule_seminaire_key").IsUnique();

            entity.HasIndex(e => e.NumeroLecteur, "LECTEURS_numero_lecteur_key").IsUnique();

            entity.Property(e => e.CodeLecteur).HasColumnName("code_lecteur");
            entity.Property(e => e.CategorieLecteur)
                .HasMaxLength(40)
                .HasComment("SEMINARISTE, FORMATEUR, PRETRE, PERSONNEL, AUTRE")
                .HasColumnName("categorie_lecteur");
            entity.Property(e => e.CodeCompte).HasColumnName("code_compte");
            entity.Property(e => e.DateExpiration).HasColumnName("date_expiration");
            entity.Property(e => e.DateInscription)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("date_inscription");
            entity.Property(e => e.InstitutionOrigine)
                .HasMaxLength(180)
                .HasColumnName("institution_origine");
            entity.Property(e => e.MatriculeSeminaire)
                .HasMaxLength(60)
                .HasColumnName("matricule_seminaire");
            entity.Property(e => e.NumeroLecteur)
                .HasMaxLength(40)
                .HasColumnName("numero_lecteur");
            entity.Property(e => e.Observations).HasColumnName("observations");
            entity.Property(e => e.StatutLecteur)
                .HasMaxLength(30)
                .HasDefaultValueSql("'ACTIF'::character varying")
                .HasColumnName("statut_lecteur");

            entity.HasOne(d => d.CodeCompteNavigation).WithOne(p => p.Lecteur)
                .HasForeignKey<Lecteur>(d => d.CodeCompte)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("LECTEURS_code_compte_fkey");
        });

        modelBuilder.Entity<LignesInventaire>(entity =>
        {
            entity.HasKey(e => e.CodeLigneInventaire).HasName("LIGNES_INVENTAIRES_pkey");

            entity.ToTable("LIGNES_INVENTAIRES");

            entity.HasIndex(e => new { e.CodeInventaire, e.CodeExemplaire }, "LIGNES_INVENTAIRES_code_inventaire_code_exemplaire_idx").IsUnique();

            entity.Property(e => e.CodeLigneInventaire).HasColumnName("code_ligne_inventaire");
            entity.Property(e => e.CodeEmplacementAttendu).HasColumnName("code_emplacement_attendu");
            entity.Property(e => e.CodeEmplacementConstate).HasColumnName("code_emplacement_constate");
            entity.Property(e => e.CodeExemplaire).HasColumnName("code_exemplaire");
            entity.Property(e => e.CodeInventaire).HasColumnName("code_inventaire");
            entity.Property(e => e.DateVerification)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date_verification");
            entity.Property(e => e.EcartConstate)
                .HasMaxLength(50)
                .HasColumnName("ecart_constate");
            entity.Property(e => e.EtatConstate)
                .HasMaxLength(30)
                .HasColumnName("etat_constate");
            entity.Property(e => e.Observations).HasColumnName("observations");
            entity.Property(e => e.Trouve).HasColumnName("trouve");
            entity.Property(e => e.VerifiePar).HasColumnName("verifie_par");

            entity.HasOne(d => d.CodeEmplacementAttenduNavigation).WithMany(p => p.LignesInventaireCodeEmplacementAttenduNavigations)
                .HasForeignKey(d => d.CodeEmplacementAttendu)
                .HasConstraintName("LIGNES_INVENTAIRES_code_emplacement_attendu_fkey");

            entity.HasOne(d => d.CodeEmplacementConstateNavigation).WithMany(p => p.LignesInventaireCodeEmplacementConstateNavigations)
                .HasForeignKey(d => d.CodeEmplacementConstate)
                .HasConstraintName("LIGNES_INVENTAIRES_code_emplacement_constate_fkey");

            entity.HasOne(d => d.CodeExemplaireNavigation).WithMany(p => p.LignesInventaires)
                .HasForeignKey(d => d.CodeExemplaire)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("LIGNES_INVENTAIRES_code_exemplaire_fkey");

            entity.HasOne(d => d.CodeInventaireNavigation).WithMany(p => p.LignesInventaires)
                .HasForeignKey(d => d.CodeInventaire)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("LIGNES_INVENTAIRES_code_inventaire_fkey");

            entity.HasOne(d => d.VerifieParNavigation).WithMany(p => p.LignesInventaires)
                .HasForeignKey(d => d.VerifiePar)
                .HasConstraintName("LIGNES_INVENTAIRES_verifie_par_fkey");
        });

        modelBuilder.Entity<LignesPret>(entity =>
        {
            entity.HasKey(e => e.CodeLignePret).HasName("LIGNES_PRETS_pkey");

            entity.ToTable("LIGNES_PRETS");

            entity.HasIndex(e => new { e.CodeBordereauPret, e.CodeExemplaire }, "LIGNES_PRETS_code_bordereau_pret_code_exemplaire_idx").IsUnique();

            entity.HasIndex(e => e.CodeExemplaire, "UX_LIGNES_PRETS_EXEMPLAIRE_EN_COURS")
                .IsUnique()
                .HasFilter("((statut_ligne)::text = 'EN_COURS'::text)");

            entity.Property(e => e.CodeLignePret).HasColumnName("code_ligne_pret");
            entity.Property(e => e.CodeBordereauPret).HasColumnName("code_bordereau_pret");
            entity.Property(e => e.CodeExemplaire).HasColumnName("code_exemplaire");
            entity.Property(e => e.CodeReservation).HasColumnName("code_reservation");
            entity.Property(e => e.DateEcheance).HasColumnName("date_echeance");
            entity.Property(e => e.DateRetrait)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date_retrait");
            entity.Property(e => e.Observations).HasColumnName("observations");
            entity.Property(e => e.StatutLigne)
                .HasMaxLength(30)
                .HasDefaultValueSql("'EN_COURS'::character varying")
                .HasComment("EN_COURS, RETOURNE, MANQUANT, PERDU")
                .HasColumnName("statut_ligne");

            entity.HasOne(d => d.CodeBordereauPretNavigation).WithMany(p => p.LignesPrets)
                .HasForeignKey(d => d.CodeBordereauPret)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("LIGNES_PRETS_code_bordereau_pret_fkey");

            entity.HasOne(d => d.CodeExemplaireNavigation).WithOne(p => p.LignesPret)
                .HasForeignKey<LignesPret>(d => d.CodeExemplaire)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("LIGNES_PRETS_code_exemplaire_fkey");

            entity.HasOne(d => d.CodeReservationNavigation).WithMany(p => p.LignesPrets)
                .HasForeignKey(d => d.CodeReservation)
                .HasConstraintName("LIGNES_PRETS_code_reservation_fkey");
        });

        modelBuilder.Entity<LignesRetour>(entity =>
        {
            entity.HasKey(e => e.CodeLigneRetour).HasName("LIGNES_RETOURS_pkey");

            entity.ToTable("LIGNES_RETOURS");

            entity.HasIndex(e => e.CodeLignePret, "UX_LIGNES_RETOURS_LIGNE_PRET").IsUnique();

            entity.Property(e => e.CodeLigneRetour).HasColumnName("code_ligne_retour");
            entity.Property(e => e.CodeBordereauRetour).HasColumnName("code_bordereau_retour");
            entity.Property(e => e.CodeLignePret).HasColumnName("code_ligne_pret");
            entity.Property(e => e.EtatRetour)
                .HasMaxLength(30)
                .HasComment("BON, ABIME, INCOMPLET, AUTRE")
                .HasColumnName("etat_retour");
            entity.Property(e => e.Observations).HasColumnName("observations");
            entity.Property(e => e.StatutApresControle)
                .HasMaxLength(30)
                .HasDefaultValueSql("'DISPONIBLE'::character varying")
                .HasColumnName("statut_apres_controle");

            entity.HasOne(d => d.CodeBordereauRetourNavigation).WithMany(p => p.LignesRetours)
                .HasForeignKey(d => d.CodeBordereauRetour)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("LIGNES_RETOURS_code_bordereau_retour_fkey");

            entity.HasOne(d => d.CodeLignePretNavigation).WithOne(p => p.LignesRetour)
                .HasForeignKey<LignesRetour>(d => d.CodeLignePret)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("LIGNES_RETOURS_code_ligne_pret_fkey");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.CodeMessage).HasName("MESSAGES_pkey");

            entity.ToTable("MESSAGES");

            entity.Property(e => e.CodeMessage).HasColumnName("code_message");
            entity.Property(e => e.CodeExpediteur).HasColumnName("code_expediteur");
            entity.Property(e => e.CodeLignePret).HasColumnName("code_ligne_pret");
            entity.Property(e => e.CodeReservation).HasColumnName("code_reservation");
            entity.Property(e => e.Contenu).HasColumnName("contenu");
            entity.Property(e => e.DateEnvoi)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date_envoi");
            entity.Property(e => e.Sujet)
                .HasMaxLength(250)
                .HasColumnName("sujet");
            entity.Property(e => e.TypeMessage)
                .HasMaxLength(40)
                .HasDefaultValueSql("'MESSAGE'::character varying")
                .HasColumnName("type_message");

            entity.HasOne(d => d.CodeExpediteurNavigation).WithMany(p => p.Messages)
                .HasForeignKey(d => d.CodeExpediteur)
                .HasConstraintName("MESSAGES_code_expediteur_fkey");

            entity.HasOne(d => d.CodeLignePretNavigation).WithMany(p => p.Messages)
                .HasForeignKey(d => d.CodeLignePret)
                .HasConstraintName("MESSAGES_code_ligne_pret_fkey");

            entity.HasOne(d => d.CodeReservationNavigation).WithMany(p => p.Messages)
                .HasForeignKey(d => d.CodeReservation)
                .HasConstraintName("MESSAGES_code_reservation_fkey");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.CodeNotification).HasName("NOTIFICATIONS_pkey");

            entity.ToTable("NOTIFICATIONS");

            entity.Property(e => e.CodeNotification).HasColumnName("code_notification");
            entity.Property(e => e.Canal)
                .HasMaxLength(30)
                .HasDefaultValueSql("'INTERNE'::character varying")
                .HasComment("INTERNE, EMAIL si configure")
                .HasColumnName("canal");
            entity.Property(e => e.CodeCompte).HasColumnName("code_compte");
            entity.Property(e => e.CodeDossierManquant).HasColumnName("code_dossier_manquant");
            entity.Property(e => e.CodeLignePret).HasColumnName("code_ligne_pret");
            entity.Property(e => e.CodeReservation).HasColumnName("code_reservation");
            entity.Property(e => e.Contenu).HasColumnName("contenu");
            entity.Property(e => e.DateCreation)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date_creation");
            entity.Property(e => e.DateLecture)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date_lecture");
            entity.Property(e => e.Titre)
                .HasMaxLength(250)
                .HasColumnName("titre");
            entity.Property(e => e.TypeNotification)
                .HasMaxLength(50)
                .HasColumnName("type_notification");

            entity.HasOne(d => d.CodeCompteNavigation).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.CodeCompte)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("NOTIFICATIONS_code_compte_fkey");

            entity.HasOne(d => d.CodeDossierManquantNavigation).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.CodeDossierManquant)
                .HasConstraintName("NOTIFICATIONS_code_dossier_manquant_fkey");

            entity.HasOne(d => d.CodeLignePretNavigation).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.CodeLignePret)
                .HasConstraintName("NOTIFICATIONS_code_ligne_pret_fkey");

            entity.HasOne(d => d.CodeReservationNavigation).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.CodeReservation)
                .HasConstraintName("NOTIFICATIONS_code_reservation_fkey");
        });

        modelBuilder.Entity<NumerosRevue>(entity =>
        {
            entity.HasKey(e => e.CodeNumeroRevue).HasName("NUMEROS_REVUES_pkey");

            entity.ToTable("NUMEROS_REVUES");

            entity.HasIndex(e => e.CodeDocument, "NUMEROS_REVUES_code_document_key").IsUnique();

            entity.Property(e => e.CodeNumeroRevue).HasColumnName("code_numero_revue");
            entity.Property(e => e.CodeDocument).HasColumnName("code_document");
            entity.Property(e => e.DateParution).HasColumnName("date_parution");
            entity.Property(e => e.IssnNumero)
                .HasMaxLength(40)
                .HasColumnName("issn_numero");
            entity.Property(e => e.Numero)
                .HasMaxLength(40)
                .HasColumnName("numero");
            entity.Property(e => e.Observations).HasColumnName("observations");
            entity.Property(e => e.Periode)
                .HasMaxLength(100)
                .HasColumnName("periode");
            entity.Property(e => e.Volume)
                .HasMaxLength(40)
                .HasColumnName("volume");

            entity.HasOne(d => d.CodeDocumentNavigation).WithOne(p => p.NumerosRevue)
                .HasForeignKey<NumerosRevue>(d => d.CodeDocument)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("NUMEROS_REVUES_code_document_fkey");
        });

        modelBuilder.Entity<PaiementsManquant>(entity =>
        {
            entity.HasKey(e => e.CodePaiementManquant).HasName("PAIEMENTS_MANQUANTS_pkey");

            entity.ToTable("PAIEMENTS_MANQUANTS");

            entity.HasIndex(e => e.CodeDossierManquant, "IX_PAIEMENTS_MANQUANTS_DOSSIER");

            entity.HasIndex(e => e.NumeroRecu, "PAIEMENTS_MANQUANTS_numero_recu_key").IsUnique();

            entity.Property(e => e.CodePaiementManquant).HasColumnName("code_paiement_manquant");
            entity.Property(e => e.CodeDossierManquant).HasColumnName("code_dossier_manquant");
            entity.Property(e => e.DatePaiement)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date_paiement");
            entity.Property(e => e.Devise)
                .HasMaxLength(10)
                .HasDefaultValueSql("'CDF'::character varying")
                .HasColumnName("devise");
            entity.Property(e => e.EnregistrePar).HasColumnName("enregistre_par");
            entity.Property(e => e.ModePaiement)
                .HasMaxLength(40)
                .HasComment("ESPECES, VIREMENT, MOBILE_MONEY, AUTRE")
                .HasColumnName("mode_paiement");
            entity.Property(e => e.Montant)
                .HasPrecision(14, 2)
                .HasColumnName("montant");
            entity.Property(e => e.NumeroRecu)
                .HasMaxLength(50)
                .HasColumnName("numero_recu");
            entity.Property(e => e.Observations).HasColumnName("observations");
            entity.Property(e => e.ReferencePaiement)
                .HasMaxLength(150)
                .HasColumnName("reference_paiement");

            entity.HasOne(d => d.CodeDossierManquantNavigation).WithMany(p => p.PaiementsManquants)
                .HasForeignKey(d => d.CodeDossierManquant)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("PAIEMENTS_MANQUANTS_code_dossier_manquant_fkey");

            entity.HasOne(d => d.EnregistreParNavigation).WithMany(p => p.PaiementsManquants)
                .HasForeignKey(d => d.EnregistrePar)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("PAIEMENTS_MANQUANTS_enregistre_par_fkey");
        });

        modelBuilder.Entity<Rayon>(entity =>
        {
            entity.HasKey(e => e.CodeRayon).HasName("RAYONS_pkey");

            entity.ToTable("RAYONS");

            entity.HasIndex(e => e.NomRayon, "RAYONS_nom_rayon_key").IsUnique();

            entity.Property(e => e.CodeRayon)
                .HasMaxLength(30)
                .HasColumnName("code_rayon");
            entity.Property(e => e.Actif)
                .HasDefaultValue(true)
                .HasColumnName("actif");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.NomRayon)
                .HasMaxLength(120)
                .HasColumnName("nom_rayon");
        });

        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.HasKey(e => e.CodeReservation).HasName("RESERVATIONS_pkey");

            entity.ToTable("RESERVATIONS", tb => tb.HasComment("Le delai de retrait est de 48 heures apres notification de disponibilite"));

            entity.HasIndex(e => e.NumeroReservation, "RESERVATIONS_numero_reservation_key").IsUnique();

            entity.HasIndex(e => e.CodeExemplaire, "UX_RESERVATIONS_EXEMPLAIRE_ACTIVE")
                .IsUnique()
                .HasFilter("((code_exemplaire IS NOT NULL) AND ((statut_reservation)::text = ANY ((ARRAY['EN_ATTENTE'::character varying, 'CONFIRMEE'::character varying])::text[])))");

            entity.Property(e => e.CodeReservation).HasColumnName("code_reservation");
            entity.Property(e => e.CodeDocument).HasColumnName("code_document");
            entity.Property(e => e.CodeExemplaire).HasColumnName("code_exemplaire");
            entity.Property(e => e.CodeLecteur).HasColumnName("code_lecteur");
            entity.Property(e => e.DateLimiteRetrait)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date_limite_retrait");
            entity.Property(e => e.DateNotificationDisponibilite)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date_notification_disponibilite");
            entity.Property(e => e.DateReservation)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date_reservation");
            entity.Property(e => e.DateRetraitEffectif)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date_retrait_effectif");
            entity.Property(e => e.DateVerification)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date_verification");
            entity.Property(e => e.MotifRefus).HasColumnName("motif_refus");
            entity.Property(e => e.NumeroReservation)
                .HasMaxLength(50)
                .HasColumnName("numero_reservation");
            entity.Property(e => e.Observations).HasColumnName("observations");
            entity.Property(e => e.StatutReservation)
                .HasMaxLength(30)
                .HasDefaultValueSql("'EN_ATTENTE'::character varying")
                .HasComment("EN_ATTENTE, EN_VERIFICATION, PRETE, HONOREE, EXPIREE, ANNULEE, REFUSEE")
                .HasColumnName("statut_reservation");
            entity.Property(e => e.VerifiePar).HasColumnName("verifie_par");

            entity.HasOne(d => d.CodeDocumentNavigation).WithMany(p => p.Reservations)
                .HasForeignKey(d => d.CodeDocument)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("RESERVATIONS_code_document_fkey");

            entity.HasOne(d => d.CodeExemplaireNavigation).WithOne(p => p.Reservation)
                .HasForeignKey<Reservation>(d => d.CodeExemplaire)
                .HasConstraintName("RESERVATIONS_code_exemplaire_fkey");

            entity.HasOne(d => d.CodeLecteurNavigation).WithMany(p => p.Reservations)
                .HasForeignKey(d => d.CodeLecteur)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("RESERVATIONS_code_lecteur_fkey");

            entity.HasOne(d => d.VerifieParNavigation).WithMany(p => p.Reservations)
                .HasForeignKey(d => d.VerifiePar)
                .HasConstraintName("RESERVATIONS_verifie_par_fkey");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.CodeRole).HasName("ROLES_pkey");

            entity.ToTable("ROLES");

            entity.HasIndex(e => e.NomRole, "ROLES_nom_role_key").IsUnique();

            entity.Property(e => e.CodeRole)
                .HasMaxLength(30)
                .HasColumnName("code_role");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.NomRole)
                .HasMaxLength(80)
                .HasColumnName("nom_role");
        });

        modelBuilder.Entity<Theme>(entity =>
        {
            entity.HasKey(e => e.CodeTheme).HasName("THEMES_pkey");

            entity.ToTable("THEMES");

            entity.HasIndex(e => e.NomTheme, "THEMES_nom_theme_key").IsUnique();

            entity.Property(e => e.CodeTheme).HasColumnName("code_theme");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.NomTheme)
                .HasMaxLength(150)
                .HasColumnName("nom_theme");
        });

        modelBuilder.Entity<TypesDocument>(entity =>
        {
            entity.HasKey(e => e.CodeTypeDocument).HasName("TYPES_DOCUMENTS_pkey");

            entity.ToTable("TYPES_DOCUMENTS", tb => tb.HasComment("Livre : 14 jours ; revue : 4 jours ; dictionnaire : consultation sur place"));

            entity.HasIndex(e => e.NomTypeDocument, "TYPES_DOCUMENTS_nom_type_document_key").IsUnique();

            entity.Property(e => e.CodeTypeDocument)
                .HasMaxLength(30)
                .HasColumnName("code_type_document");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DureePretJours).HasColumnName("duree_pret_jours");
            entity.Property(e => e.EmpruntAutorise)
                .HasDefaultValue(true)
                .HasColumnName("emprunt_autorise");
            entity.Property(e => e.NomTypeDocument)
                .HasMaxLength(80)
                .HasColumnName("nom_type_document");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
