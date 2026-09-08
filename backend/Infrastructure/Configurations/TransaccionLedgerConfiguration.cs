using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class TransaccionLedgerConfiguration : IEntityTypeConfiguration<TransaccionLedger>
{
    public void Configure(EntityTypeBuilder<TransaccionLedger> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Monto).HasColumnType("decimal(18,2)");
        builder.Property(t => t.Tipo).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(t => t.Billetera)
               .WithMany()
               .HasForeignKey(t => t.BilleteraId);
    }
}