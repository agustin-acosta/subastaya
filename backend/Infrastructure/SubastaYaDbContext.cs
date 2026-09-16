using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure;

public class SubastaYaDbContext : DbContext
{
    public SubastaYaDbContext(DbContextOptions<SubastaYaDbContext> options)
        : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Billetera> Billeteras => Set<Billetera>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Subasta> Subastas => Set<Subasta>();
    public DbSet<Puja> Pujas => Set<Puja>();
    public DbSet<TransaccionLedger> TransaccionesLedger => Set<TransaccionLedger>();
    public DbSet<AuditoriaLog> AuditoriaLogs => Set<AuditoriaLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SubastaYaDbContext).Assembly);

        var conversorUtc = new ValueConverter<DateTime, DateTime>(
            aGuardar => aGuardar,
            aLeer => DateTime.SpecifyKind(aLeer, DateTimeKind.Utc));

        var conversorUtcNulable = new ValueConverter<DateTime?, DateTime?>(
            aGuardar => aGuardar,
            aLeer => aLeer.HasValue ? DateTime.SpecifyKind(aLeer.Value, DateTimeKind.Utc) : aLeer);

        foreach (var entidad in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var propiedad in entidad.GetProperties())
            {
                if (propiedad.ClrType == typeof(DateTime))
                {
                    propiedad.SetValueConverter(conversorUtc);
                }
                else if (propiedad.ClrType == typeof(DateTime?))
                {
                    propiedad.SetValueConverter(conversorUtcNulable);
                }
            }
        }

        base.OnModelCreating(modelBuilder);
    }
}