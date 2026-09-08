using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class SubastaConfiguration : IEntityTypeConfiguration<Subasta>
{
    public void Configure(EntityTypeBuilder<Subasta> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.PrecioBase).HasColumnType("decimal(18,2)");
        builder.Property(s => s.IncrementoMinimo).HasColumnType("decimal(18,2)");
        builder.Property(s => s.PujaActualMonto).HasColumnType("decimal(18,2)");
        builder.Property(s => s.Version).IsRowVersion();

        builder.Property(s => s.Estado)
               .HasConversion<string>()
               .HasMaxLength(20);

        builder.HasOne(s => s.Vendedor)
               .WithMany()
               .HasForeignKey(s => s.VendedorId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Categoria)
               .WithMany()
               .HasForeignKey(s => s.CategoriaId);

        builder.HasMany(s => s.Pujas)
               .WithOne(p => p.Subasta)
               .HasForeignKey(p => p.SubastaId);
    }
}