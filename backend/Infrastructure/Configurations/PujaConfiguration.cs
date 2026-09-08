using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class PujaConfiguration : IEntityTypeConfiguration<Puja>
{
    public void Configure(EntityTypeBuilder<Puja> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Monto).HasColumnType("decimal(18,2)");

        builder.HasOne(p => p.Comprador)
               .WithMany()
               .HasForeignKey(p => p.CompradorId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}