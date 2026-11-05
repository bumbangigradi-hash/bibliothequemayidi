using bibliotheque_mayidi.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace bibliotheque_mayidi.Data.Generated;

public partial class BibliothequeScaffoldContext
{
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Exemplaire>(entity =>
        {
            entity.Property<long?>("CodeSessionEnregistrement")
                .HasColumnName("code_session_enregistrement")
                .HasColumnType("bigint");
        });
    }
}