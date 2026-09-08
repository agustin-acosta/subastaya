using Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public static class DbSeeder
{
    public static void Seed(SubastaYaDbContext context)
    {
        Console.WriteLine(">>> DbSeeder.Seed() fue llamado.");
        if (context.Usuarios.Any())
        {
            Console.WriteLine(">>> Ya hay usuarios cargados, no se siembra nada.");
            return;
        }
        Console.WriteLine(">>> Sembrando datos...");

        // usuarios
        var vendedor = new Usuario
        {
            Email = "vendedor@test.com",
            Nombre = "Creador de Publicaciones",
            PasswordHash = "temporal",
            FechaRegistro = DateTime.UtcNow
        };
        var comprador1 = new Usuario
        {
            Email = "comprador1@test.com",
            Nombre = "Postor Líder",
            PasswordHash = "temporal",
            FechaRegistro = DateTime.UtcNow
        };
        var comprador2 = new Usuario
        {
            Email = "comprador2@test.com",
            Nombre = "Postor Habilitado",
            PasswordHash = "temporal",
            FechaRegistro = DateTime.UtcNow
        };
        var sinFondos = new Usuario
        {
            Email = "sinfondos@test.com",
            Nombre = "Usuario Sin Fondos",
            PasswordHash = "temporal",
            FechaRegistro = DateTime.UtcNow
        };

        context.Usuarios.AddRange(vendedor, comprador1, comprador2, sinFondos);
        context.SaveChanges();

        // billeteras
        var billeteraVendedor = new Billetera
        {
            UsuarioId = vendedor.Id,
            SaldoTotal = 0m,
            SaldoRetenido = 0m
        };
        var billeteraComprador1 = new Billetera
        {
            UsuarioId = comprador1.Id,
            SaldoTotal = 150000m,
            SaldoRetenido = 45000m
        };
        var billeteraComprador2 = new Billetera
        {
            UsuarioId = comprador2.Id,
            SaldoTotal = 200000m,
            SaldoRetenido = 0m
        };
        var billeteraSinFondos = new Billetera
        {
            UsuarioId = sinFondos.Id,
            SaldoTotal = 500m,
            SaldoRetenido = 0m
        };

        context.Billeteras.AddRange(billeteraVendedor, billeteraComprador1, billeteraComprador2, billeteraSinFondos);

        // categorias
        var tecnologia = new Categoria { Nombre = "Tecnología" };
        var coleccionables = new Categoria { Nombre = "Coleccionables" };
        var indumentaria = new Categoria { Nombre = "Indumentaria" };
        var vehiculos = new Categoria { Nombre = "Vehículos" };

        context.Categorias.AddRange(tecnologia, coleccionables, indumentaria, vehiculos);
        context.SaveChanges();

        var ahora = DateTime.UtcNow;

        // subastas
        var activaEstandar = new Subasta
        {
            VendedorId = vendedor.Id,
            CategoriaId = tecnologia.Id,
            Titulo = "Notebook Gamer",
            Descripcion = "Notebook usada, buen estado.",
            UrlImagen = "",
            PrecioBase = 40000m,
            IncrementoMinimo = 1000m,
            FechaInicio = ahora.AddHours(-2),
            FechaFin = ahora.AddMinutes(25), 
            Estado = EstadoSubasta.Activa,
            PujaActualMonto = 45000m 
        };

        var activaCritica = new Subasta
        {
            VendedorId = vendedor.Id,
            CategoriaId = coleccionables.Id,
            Titulo = "Figura de colección edición limitada",
            Descripcion = "Sellada, nunca abierta.",
            UrlImagen = "",
            PrecioBase = 50000m,
            IncrementoMinimo = 2000m,
            FechaInicio = ahora.AddHours(-2),
            FechaFin = ahora.AddSeconds(45), // anti sniping
            Estado = EstadoSubasta.Activa,
            PujaActualMonto = 52000m
        };

        var proxima = new Subasta
        {
            VendedorId = vendedor.Id,
            CategoriaId = indumentaria.Id,
            Titulo = "Campera de cuero vintage",
            Descripcion = "Talle M, poco uso.",
            UrlImagen = "",
            PrecioBase = 30000m,
            IncrementoMinimo = 1000m,
            FechaInicio = ahora.AddHours(24), 
            FechaFin = ahora.AddDays(5),
            Estado = EstadoSubasta.Programada,
            PujaActualMonto = null
        };

        var vencidaConGanador = new Subasta
        {
            VendedorId = vendedor.Id,
            CategoriaId = vehiculos.Id,
            Titulo = "Bicicleta rodado 29",
            Descripcion = "Poco uso, service al día.",
            UrlImagen = "",
            PrecioBase = 80000m,
            IncrementoMinimo = 3000m,
            FechaInicio = ahora.AddDays(-5),
            FechaFin = ahora.AddDays(-1), // vencida
            Estado = EstadoSubasta.Finalizada,
            PujaActualMonto = 95000m
        };

        var vencidaDesierta = new Subasta
        {
            VendedorId = vendedor.Id,
            CategoriaId = tecnologia.Id,
            Titulo = "Teclado mecánico usado",
            Descripcion = "Sin uso reciente.",
            UrlImagen = "",
            PrecioBase = 20000m,
            IncrementoMinimo = 1000m,
            FechaInicio = ahora.AddDays(-6),
            FechaFin = ahora.AddDays(-2), // vencida, sin pujas
            Estado = EstadoSubasta.Desierta,
            PujaActualMonto = null
        };

        context.Subastas.AddRange(activaEstandar, activaCritica, proxima, vencidaConGanador, vencidaDesierta);
        context.SaveChanges();

        var puja1 = new Puja
        {
            SubastaId = activaEstandar.Id,
            CompradorId = comprador2.Id,
            Monto = 41000m, // primera oferta, superada
            FechaPuja = ahora.AddHours(-2)
        };
        var puja2 = new Puja
        {
            SubastaId = activaEstandar.Id,
            CompradorId = comprador1.Id,
            Monto = 45000m, // oferta lider actual, coincide con PujaActualMonto y la retencion
            FechaPuja = ahora.AddHours(-1)
        };

        var pujaGanadora = new Puja
        {
            SubastaId = vencidaConGanador.Id,
            CompradorId = comprador1.Id,
            Monto = 95000m,
            FechaPuja = ahora.AddDays(-2)
        };

        context.Pujas.AddRange(puja1, puja2, pujaGanadora);

        var depositoComprador1 = new TransaccionLedger
        {
            BilleteraId = billeteraComprador1.Id,
            Tipo = TipoMovimiento.Deposito,
            Monto = 150000m,
            Fecha = ahora.AddDays(-3),
            SubastaId = null
        };
        var retencionComprador1 = new TransaccionLedger
        {
            BilleteraId = billeteraComprador1.Id,
            Tipo = TipoMovimiento.Retencion,
            Monto = 45000m,
            Fecha = ahora.AddHours(-1),
            SubastaId = activaEstandar.Id
        };
        var depositoComprador2 = new TransaccionLedger
        {
            BilleteraId = billeteraComprador2.Id,
            Tipo = TipoMovimiento.Deposito,
            Monto = 200000m,
            Fecha = ahora.AddDays(-3),
            SubastaId = null
        };

        var depositoSinFondos = new TransaccionLedger
        {
            BilleteraId = billeteraSinFondos.Id,
            Tipo = TipoMovimiento.Deposito,
            Monto = 500m,
            Fecha = ahora.AddDays(-3),
            SubastaId = null
        };

        context.TransaccionesLedger.AddRange(
            depositoComprador1, retencionComprador1, depositoComprador2, depositoSinFondos);

        context.SaveChanges();
        Console.WriteLine(">>> Seed completado.");
    }
}