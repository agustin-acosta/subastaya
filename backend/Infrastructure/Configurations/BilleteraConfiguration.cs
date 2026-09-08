using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class BilleteraConfiguration : IEntityTypeConfiguration<Billetera>
{
    public void Configure(EntityTypeBuilder<Billetera> builder)
    {
        builder.HasKey(b => b.Id);
        builder.Property(b => b.SaldoTotal).HasColumnType("decimal(18,2)");
        builder.Property(b => b.SaldoRetenido).HasColumnType("decimal(18,2)");
        builder.Ignore(b => b.SaldoDisponible);
        builder.Property(b => b.Version).IsRowVersion();

        builder.HasOne(b => b.Usuario)
               .WithOne(u => u.Billetera)
               .HasForeignKey<Billetera>(b => b.UsuarioId);
    }
}