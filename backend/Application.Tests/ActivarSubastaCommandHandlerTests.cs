using Application.Commands.ActivarSubasta;
using Application.Interfaces;
using Domain;
using Moq;
using Xunit;

namespace Application.Tests;

public class ActivarSubastaCommandHandlerTests
{
    private static Subasta CrearSubastaProgramada(int id = 1)
    {
        return new Subasta
        {
            Id = id,
            VendedorId = 1,
            CategoriaId = 1,
            Titulo = "Subasta de prueba",
            Descripcion = "Descripcion",
            UrlImagen = "",
            PrecioBase = 1000,
            IncrementoMinimo = 100,
            FechaInicio = DateTime.UtcNow.AddMinutes(-1),
            FechaFin = DateTime.UtcNow.AddHours(1),
            Estado = EstadoSubasta.Programada,
            PujaActualMonto = null
        };
    }

    private static (ActivarSubastaCommandHandler handler, Mock<ISubastaRepository> subastaRepo, Mock<INotificadorSubastas> notificadorSubastas) CrearHandler()
    {
        var subastaRepo = new Mock<ISubastaRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        var notificadorSubastas = new Mock<INotificadorSubastas>();

        var handler = new ActivarSubastaCommandHandler(subastaRepo.Object, unitOfWork.Object, notificadorSubastas.Object);
        return (handler, subastaRepo, notificadorSubastas);
    }

    [Fact]
    public async Task Activar_UnaSubastaProgramada_LaPasaAActivaYRegistraAuditoriaYNotificaElCambio()
    {
        var (handler, subastaRepo, notificadorSubastas) = CrearHandler();
        var subasta = CrearSubastaProgramada();

        subastaRepo.Setup(r => r.ObtenerPorIdAsync(subasta.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subasta);

        await handler.Handle(new ActivarSubastaCommand(subasta.Id), CancellationToken.None);

        Assert.Equal(EstadoSubasta.Activa, subasta.Estado);
        subastaRepo.Verify(r => r.AgregarAuditoria(It.Is<AuditoriaLog>(a => a.Accion == "ACTIVACION")), Times.Once);
        subastaRepo.Verify(r => r.ActualizarSubasta(subasta), Times.Once);

        notificadorSubastas.Verify(n => n.NotificarCambioEstadoAsync(subasta.Id, "Activa", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Activar_UnaSubastaQueYaNoEstaProgramada_NoHaceNadaYNoNotificaNada()
    {
        var (handler, subastaRepo, notificadorSubastas) = CrearHandler();
        var subasta = CrearSubastaProgramada();
        subasta.Estado = EstadoSubasta.Activa;

        subastaRepo.Setup(r => r.ObtenerPorIdAsync(subasta.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subasta);

        await handler.Handle(new ActivarSubastaCommand(subasta.Id), CancellationToken.None);

        subastaRepo.Verify(r => r.AgregarAuditoria(It.IsAny<AuditoriaLog>()), Times.Never);
        subastaRepo.Verify(r => r.ActualizarSubasta(It.IsAny<Subasta>()), Times.Never);
        notificadorSubastas.Verify(n => n.NotificarCambioEstadoAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}