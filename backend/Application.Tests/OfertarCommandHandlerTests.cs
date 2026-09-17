using Application.Commands.Ofertar;
using Application.Interfaces;
using Domain;
using Domain.Exceptions;
using Moq;
using Xunit;

namespace Application.Tests;

public class OfertarCommandHandlerTests
{
    private static Subasta CrearSubastaActiva(
        int id = 1, int vendedorId = 1, decimal precioBase = 1000, decimal incrementoMinimo = 100,
        decimal? pujaActualMonto = null, DateTime? fechaFin = null)
    {
        return new Subasta
        {
            Id = id,
            VendedorId = vendedorId,
            CategoriaId = 1,
            Titulo = "Subasta de prueba",
            Descripcion = "Descripcion",
            UrlImagen = "",
            PrecioBase = precioBase,
            IncrementoMinimo = incrementoMinimo,
            FechaInicio = DateTime.UtcNow.AddHours(-1),
            FechaFin = fechaFin ?? DateTime.UtcNow.AddHours(1),
            Estado = EstadoSubasta.Activa,
            PujaActualMonto = pujaActualMonto
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

    private static (OfertarCommandHandler handler, Mock<ISubastaRepository> subastaRepo, Mock<IBilleteraRepository> billeteraRepo, Mock<INotificadorSubastas> notificadorSubastas)
        CrearHandler()
    {
        var subastaRepo = new Mock<ISubastaRepository>();
        var billeteraRepo = new Mock<IBilleteraRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        var notificadorSubastas = new Mock<INotificadorSubastas>();

        var handler = new OfertarCommandHandler(subastaRepo.Object, billeteraRepo.Object, unitOfWork.Object, notificadorSubastas.Object);
        return (handler, subastaRepo, billeteraRepo, notificadorSubastas);
    }

    [Fact]
    public async Task Ofertar_ConMontoValido_ActualizaLaSubastaYRetieneElSaldo()
    {
        var (handler, subastaRepo, billeteraRepo, notificadorSubastas) = CrearHandler();

        var subasta = CrearSubastaActiva();
        var billeteraComprador = CrearBilletera(usuarioId: 2, saldoTotal: 5000);

        subastaRepo.Setup(r => r.ObtenerPorIdAsync(subasta.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subasta);
        subastaRepo.Setup(r => r.ObtenerPujaConMayorMontoAsync(subasta.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Puja?)null);
        billeteraRepo.Setup(r => r.ObtenerPorUsuarioIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(billeteraComprador);

        var command = new OfertarCommand(subasta.Id, compradorId: 2, monto: 1200);

        await handler.Handle(command, CancellationToken.None);

        Assert.Equal(1200, subasta.PujaActualMonto);
        Assert.Equal(1200, billeteraComprador.SaldoRetenido);
        subastaRepo.Verify(r => r.AgregarPuja(It.Is<Puja>(p => p.CompradorId == 2 && p.Monto == 1200)), Times.Once);

        notificadorSubastas.Verify(n => n.NotificarNuevaPujaAsync(
            subasta.Id,
            It.Is<NuevaPujaNotificacion>(p => p.CompradorId == 2 && p.Monto == 1200),
            It.IsAny<CancellationToken>()), Times.Once);

        notificadorSubastas.Verify(n => n.NotificarExtensionAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Ofertar_ConMontoMenorAlIncrementoMinimo_LanzaMontoInsuficienteExceptionYNoNotificaNada()
    {
        var (handler, subastaRepo, _, notificadorSubastas) = CrearHandler();
        var subasta = CrearSubastaActiva(precioBase: 1000, incrementoMinimo: 100);

        subastaRepo.Setup(r => r.ObtenerPorIdAsync(subasta.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subasta);

        var command = new OfertarCommand(subasta.Id, compradorId: 2, monto: 1050);

        await Assert.ThrowsAsync<MontoInsuficienteException>(() => handler.Handle(command, CancellationToken.None));

        notificadorSubastas.Verify(n => n.NotificarNuevaPujaAsync(It.IsAny<int>(), It.IsAny<NuevaPujaNotificacion>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Ofertar_ConSaldoInsuficiente_LanzaSaldoInsuficienteExceptionYNoNotificaNada()
    {
        var (handler, subastaRepo, billeteraRepo, notificadorSubastas) = CrearHandler();
        var subasta = CrearSubastaActiva();
        var billeteraComprador = CrearBilletera(usuarioId: 2, saldoTotal: 500);

        subastaRepo.Setup(r => r.ObtenerPorIdAsync(subasta.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subasta);
        subastaRepo.Setup(r => r.ObtenerPujaConMayorMontoAsync(subasta.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Puja?)null);
        billeteraRepo.Setup(r => r.ObtenerPorUsuarioIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(billeteraComprador);

        var command = new OfertarCommand(subasta.Id, compradorId: 2, monto: 1200);

        await Assert.ThrowsAsync<SaldoInsuficienteException>(() => handler.Handle(command, CancellationToken.None));

        notificadorSubastas.Verify(n => n.NotificarNuevaPujaAsync(It.IsAny<int>(), It.IsAny<NuevaPujaNotificacion>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Ofertar_EnSubastaVencida_LanzaSubastaNoVigenteExceptionYNoNotificaNada()
    {
        var (handler, subastaRepo, _, notificadorSubastas) = CrearHandler();
        var subasta = CrearSubastaActiva(fechaFin: DateTime.UtcNow.AddMinutes(-5));

        subastaRepo.Setup(r => r.ObtenerPorIdAsync(subasta.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subasta);

        var command = new OfertarCommand(subasta.Id, compradorId: 2, monto: 1200);

        await Assert.ThrowsAsync<SubastaNoVigenteException>(() => handler.Handle(command, CancellationToken.None));

        notificadorSubastas.Verify(n => n.NotificarNuevaPujaAsync(It.IsAny<int>(), It.IsAny<NuevaPujaNotificacion>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Ofertar_SobreLaPropiaSubasta_LanzaOperacionInvalidaExceptionYNoNotificaNada()
    {
        var (handler, subastaRepo, _, notificadorSubastas) = CrearHandler();
        var subasta = CrearSubastaActiva(vendedorId: 7);

        subastaRepo.Setup(r => r.ObtenerPorIdAsync(subasta.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subasta);

        var command = new OfertarCommand(subasta.Id, compradorId: 7, monto: 1200);

        await Assert.ThrowsAsync<OperacionInvalidaException>(() => handler.Handle(command, CancellationToken.None));

        notificadorSubastas.Verify(n => n.NotificarNuevaPujaAsync(It.IsAny<int>(), It.IsAny<NuevaPujaNotificacion>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Ofertar_SuperandoAlLiderAnterior_LiberaSuRetencionYRetieneAlNuevo()
    {
        var (handler, subastaRepo, billeteraRepo, notificadorSubastas) = CrearHandler();
        var subasta = CrearSubastaActiva(pujaActualMonto: 1100);

        var pujaAnterior = new Puja { Id = 1, SubastaId = subasta.Id, CompradorId = 2, Monto = 1100, FechaPuja = DateTime.UtcNow.AddMinutes(-10) };
        var billeteraLiderAnterior = CrearBilletera(usuarioId: 2, saldoTotal: 5000, saldoRetenido: 1100);
        var billeteraNuevoComprador = CrearBilletera(usuarioId: 3, saldoTotal: 5000);

        subastaRepo.Setup(r => r.ObtenerPorIdAsync(subasta.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subasta);
        subastaRepo.Setup(r => r.ObtenerPujaConMayorMontoAsync(subasta.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pujaAnterior);
        billeteraRepo.Setup(r => r.ObtenerPorUsuarioIdAsync(3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(billeteraNuevoComprador);
        billeteraRepo.Setup(r => r.ObtenerPorUsuarioIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(billeteraLiderAnterior);

        var command = new OfertarCommand(subasta.Id, compradorId: 3, monto: 1300);

        await handler.Handle(command, CancellationToken.None);

        Assert.Equal(0, billeteraLiderAnterior.SaldoRetenido);
        Assert.Equal(1300, billeteraNuevoComprador.SaldoRetenido);
        Assert.Equal(1300, subasta.PujaActualMonto);

        notificadorSubastas.Verify(n => n.NotificarNuevaPujaAsync(
            subasta.Id,
            It.Is<NuevaPujaNotificacion>(p => p.CompradorId == 3 && p.Monto == 1300),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Ofertar_DentroDeLaVentanaAntiSniping_ExtiendeLaSubastaYRegistraAuditoriaYNotificaLaExtension()
    {
        var (handler, subastaRepo, billeteraRepo, notificadorSubastas) = CrearHandler();
        var fechaFinOriginal = DateTime.UtcNow.AddSeconds(30);
        var subasta = CrearSubastaActiva(fechaFin: fechaFinOriginal);
        var billeteraComprador = CrearBilletera(usuarioId: 2, saldoTotal: 5000);

        subastaRepo.Setup(r => r.ObtenerPorIdAsync(subasta.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subasta);
        subastaRepo.Setup(r => r.ObtenerPujaConMayorMontoAsync(subasta.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Puja?)null);
        billeteraRepo.Setup(r => r.ObtenerPorUsuarioIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(billeteraComprador);

        var command = new OfertarCommand(subasta.Id, compradorId: 2, monto: 1200);

        await handler.Handle(command, CancellationToken.None);

        Assert.True(subasta.FechaFin >= fechaFinOriginal.AddMinutes(2).AddSeconds(-1));
        subastaRepo.Verify(r => r.AgregarAuditoria(It.Is<AuditoriaLog>(a => a.Accion == "EXTENSION_TIEMPO")), Times.Once);

        notificadorSubastas.Verify(n => n.NotificarExtensionAsync(
            subasta.Id,
            It.Is<DateTime>(f => f >= fechaFinOriginal.AddMinutes(2).AddSeconds(-1)),
            It.IsAny<CancellationToken>()), Times.Once);
        notificadorSubastas.Verify(n => n.NotificarNuevaPujaAsync(subasta.Id, It.IsAny<NuevaPujaNotificacion>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}