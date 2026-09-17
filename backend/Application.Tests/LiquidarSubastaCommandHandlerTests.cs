using Application.Commands.LiquidarSubasta;
using Application.Interfaces;
using Domain;
using Moq;
using Xunit;

namespace Application.Tests;

public class LiquidarSubastaCommandHandlerTests
{
    private static Subasta CrearSubastaVencida(int id = 1, int vendedorId = 1)
    {
        return new Subasta
        {
            Id = id,
            VendedorId = vendedorId,
            CategoriaId = 1,
            Titulo = "Subasta de prueba",
            Descripcion = "Descripcion",
            UrlImagen = "",
            PrecioBase = 1000,
            IncrementoMinimo = 100,
            FechaInicio = DateTime.UtcNow.AddHours(-2),
            FechaFin = DateTime.UtcNow.AddMinutes(-1),
            Estado = EstadoSubasta.Activa,
            PujaActualMonto = null
        };
    }

    private static Billetera CrearBilletera(int usuarioId, decimal saldoTotal, decimal saldoRetenido = 0)
    {
        return new Billetera
        {
            Id = usuarioId,
            UsuarioId = usuarioId,
            SaldoTotal = saldoTotal,
            SaldoRetenido = saldoRetenido
        };
    }

    private static (LiquidarSubastaCommandHandler handler, Mock<ISubastaRepository> subastaRepo, Mock<IBilleteraRepository> billeteraRepo, Mock<INotificadorSubastas> notificadorSubastas)
        CrearHandler()
    {
        var subastaRepo = new Mock<ISubastaRepository>();
        var billeteraRepo = new Mock<IBilleteraRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        var notificadorSubastas = new Mock<INotificadorSubastas>();

        var handler = new LiquidarSubastaCommandHandler(subastaRepo.Object, billeteraRepo.Object, unitOfWork.Object, notificadorSubastas.Object);
        return (handler, subastaRepo, billeteraRepo, notificadorSubastas);
    }

    [Fact]
    public async Task Liquidar_ConGanador_TransfiereElSaldoYFinalizaLaSubastaYNotificaElCambio()
    {
        var (handler, subastaRepo, billeteraRepo, notificadorSubastas) = CrearHandler();
        var subasta = CrearSubastaVencida(vendedorId: 1);

        var pujaGanadora = new Puja { Id = 10, SubastaId = subasta.Id, CompradorId = 2, Monto = 1200, FechaPuja = DateTime.UtcNow.AddMinutes(-5) };
        var billeteraComprador = CrearBilletera(usuarioId: 2, saldoTotal: 5000, saldoRetenido: 1200);
        var billeteraVendedor = CrearBilletera(usuarioId: 1, saldoTotal: 0);

        subastaRepo.Setup(r => r.ObtenerPorIdAsync(subasta.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subasta);
        subastaRepo.Setup(r => r.ObtenerPujaConMayorMontoAsync(subasta.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pujaGanadora);
        billeteraRepo.Setup(r => r.ObtenerPorUsuarioIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(billeteraComprador);
        billeteraRepo.Setup(r => r.ObtenerPorUsuarioIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(billeteraVendedor);

        await handler.Handle(new LiquidarSubastaCommand(subasta.Id), CancellationToken.None);

        Assert.Equal(EstadoSubasta.Finalizada, subasta.Estado);
        Assert.Equal(3800, billeteraComprador.SaldoTotal);
        Assert.Equal(0, billeteraComprador.SaldoRetenido);
        Assert.Equal(1200, billeteraVendedor.SaldoTotal);
        subastaRepo.Verify(r => r.AgregarAuditoria(It.Is<AuditoriaLog>(a => a.Accion == "CIERRE_CON_GANADOR")), Times.Once);
        subastaRepo.Verify(r => r.ActualizarSubasta(subasta), Times.Once);

        notificadorSubastas.Verify(n => n.NotificarCambioEstadoAsync(subasta.Id, "Finalizada", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Liquidar_SinOfertas_MarcaLaSubastaComoDesiertaYNotificaElCambio()
    {
        var (handler, subastaRepo, billeteraRepo, notificadorSubastas) = CrearHandler();
        var subasta = CrearSubastaVencida();

        subastaRepo.Setup(r => r.ObtenerPorIdAsync(subasta.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subasta);
        subastaRepo.Setup(r => r.ObtenerPujaConMayorMontoAsync(subasta.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Puja?)null);

        await handler.Handle(new LiquidarSubastaCommand(subasta.Id), CancellationToken.None);

        Assert.Equal(EstadoSubasta.Desierta, subasta.Estado);
        subastaRepo.Verify(r => r.AgregarAuditoria(It.Is<AuditoriaLog>(a => a.Accion == "CIERRE_DESIERTA")), Times.Once);
        billeteraRepo.Verify(r => r.AgregarMovimiento(It.IsAny<TransaccionLedger>()), Times.Never);

        notificadorSubastas.Verify(n => n.NotificarCambioEstadoAsync(subasta.Id, "Desierta", It.IsAny<CancellationToken>()), Times.Once);
    }
}