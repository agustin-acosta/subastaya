using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class AuditoriaLogConfiguration : IEntityTypeConfiguration<AuditoriaLog>
{
    public void Configure(EntityTypeBuilder<AuditoriaLog> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Entidad).HasMaxLength(50);
        builder.Property(a => a.Accion).HasMaxLength(50);
    }
}