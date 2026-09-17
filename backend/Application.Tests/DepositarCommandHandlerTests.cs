using Application.Commands.Depositar;
using Application.Interfaces;
using Domain;
using Domain.Exceptions;
using Moq;
using Xunit;

namespace Application.Tests;

public class DepositarCommandHandlerTests
{
    private static Billetera CrearBilletera(int usuarioId, decimal saldoTotal)
    {
        return new Billetera
        {
            Id = usuarioId,
            UsuarioId = usuarioId,
            SaldoTotal = saldoTotal,
            SaldoRetenido = 0
        };
    }

    private static (DepositarCommandHandler handler, Mock<IBilleteraRepository> billeteraRepo) CrearHandler()
    {
        var billeteraRepo = new Mock<IBilleteraRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new DepositarCommandHandler(billeteraRepo.Object, unitOfWork.Object);
        return (handler, billeteraRepo);
    }

    [Fact]
    public async Task Depositar_ConMontoValido_AcreditaElSaldoYRegistraAuditoria()
    {
        var (handler, billeteraRepo) = CrearHandler();
        var billetera = CrearBilletera(usuarioId: 1, saldoTotal: 1000);

        billeteraRepo.Setup(r => r.ObtenerPorUsuarioIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(billetera);

        var command = new DepositarCommand(usuarioId: 1, monto: 500);

        await handler.Handle(command, CancellationToken.None);

        Assert.Equal(1500, billetera.SaldoTotal);
        billeteraRepo.Verify(r => r.AgregarMovimiento(It.Is<TransaccionLedger>(
            m => m.Tipo == TipoMovimiento.Deposito && m.Monto == 500)), Times.Once);
        billeteraRepo.Verify(r => r.AgregarAuditoria(It.Is<AuditoriaLog>(
            a => a.Accion == "ACREDITACION_MANUAL" && a.UsuarioId == 1)), Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public async Task Depositar_ConMontoCeroONegativo_LanzaOperacionInvalidaException(decimal monto)
    {
        var (handler, _) = CrearHandler();
        var command = new DepositarCommand(usuarioId: 1, monto: monto);

        await Assert.ThrowsAsync<OperacionInvalidaException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Depositar_ConUsuarioInexistente_LanzaEntidadNoEncontradaException()
    {
        var (handler, billeteraRepo) = CrearHandler();

        billeteraRepo.Setup(r => r.ObtenerPorUsuarioIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Billetera?)null);

        var command = new DepositarCommand(usuarioId: 99, monto: 500);

        await Assert.ThrowsAsync<EntidadNoEncontradaException>(() => handler.Handle(command, CancellationToken.None));
    }
}