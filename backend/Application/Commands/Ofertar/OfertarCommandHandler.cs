using Application.Interfaces;
using Domain;
using Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Application.Commands.Ofertar;

public class OfertarCommandHandler
{
    private readonly ISubastaRepository _subastaRepository;
    private readonly IBilleteraRepository _billeteraRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificadorSubastas _notificadorSubastas;

    private const int SegundosVentanaAntiSniping = 60;
    private static readonly TimeSpan ExtensionAntiSniping = TimeSpan.FromMinutes(2);

    public OfertarCommandHandler(
        ISubastaRepository subastaRepository,
        IBilleteraRepository billeteraRepository,
        IUnitOfWork unitOfWork,
        INotificadorSubastas notificadorSubastas)
    {
        _subastaRepository = subastaRepository;
        _billeteraRepository = billeteraRepository;
        _unitOfWork = unitOfWork;
        _notificadorSubastas = notificadorSubastas;
    }

    public async Task<int> Handle(OfertarCommand command, CancellationToken cancellationToken)
    {
        var subasta = await _subastaRepository.ObtenerPorIdAsync(command.SubastaId, cancellationToken);
        if (subasta is null)
        {
            throw new EntidadNoEncontradaException("La subasta no existe.");
        }

        if (subasta.VendedorId == command.CompradorId)
        {
            throw new OperacionInvalidaException("No podés ofertar en tu propia subasta.");
        }

        var ahora = DateTime.UtcNow;

        if (subasta.Estado != EstadoSubasta.Activa || ahora < subasta.FechaInicio || ahora > subasta.FechaFin)
        {
            await RegistrarPujaRechazadaAsync(subasta.Id, command.CompradorId, "PUJA_RECHAZADA_SUBASTA_NO_VIGENTE",
                new { command.Monto }, cancellationToken);
            throw new SubastaNoVigenteException("La subasta no está vigente para recibir ofertas.");
        }

        var montoMinimoRequerido = (subasta.PujaActualMonto ?? subasta.PrecioBase) + subasta.IncrementoMinimo;
        if (command.Monto < montoMinimoRequerido)
        {
            await RegistrarPujaRechazadaAsync(subasta.Id, command.CompradorId, "PUJA_RECHAZADA_MONTO_INSUFICIENTE",
                new { command.Monto, montoMinimoRequerido }, cancellationToken);
            throw new MontoInsuficienteException(
                $"El monto debe ser al menos {montoMinimoRequerido:C}.");
        }

        var billeteraComprador = await _billeteraRepository.ObtenerPorUsuarioIdAsync(command.CompradorId, cancellationToken);
        if (billeteraComprador is null)
        {
            throw new EntidadNoEncontradaException("El usuario comprador no existe.");
        }

        var pujaAnterior = await _subastaRepository.ObtenerPujaConMayorMontoAsync(subasta.Id, cancellationToken);

        var montoQueSeLiberaDelMismoComprador = pujaAnterior is not null && pujaAnterior.CompradorId == command.CompradorId
            ? pujaAnterior.Monto
            : 0m;

        if (billeteraComprador.SaldoDisponible + montoQueSeLiberaDelMismoComprador < command.Monto)
        {
            await RegistrarPujaRechazadaAsync(subasta.Id, command.CompradorId, "PUJA_RECHAZADA_SALDO_INSUFICIENTE",
                new { command.Monto }, cancellationToken);
            throw new SaldoInsuficienteException("Saldo disponible insuficiente para esta oferta.");
        }

        if (pujaAnterior is not null)
        {
            var billeteraLiderAnterior = pujaAnterior.CompradorId == command.CompradorId
                ? billeteraComprador
                : await _billeteraRepository.ObtenerPorUsuarioIdAsync(pujaAnterior.CompradorId, cancellationToken);

            if (billeteraLiderAnterior is not null)
            {
                billeteraLiderAnterior.SaldoRetenido -= pujaAnterior.Monto;

                _billeteraRepository.AgregarMovimiento(new TransaccionLedger
                {
                    BilleteraId = billeteraLiderAnterior.Id,
                    Tipo = TipoMovimiento.Liberacion,
                    Monto = pujaAnterior.Monto,
                    Fecha = ahora,
                    SubastaId = subasta.Id
                });
            }
        }

        billeteraComprador.SaldoRetenido += command.Monto;

        _billeteraRepository.AgregarMovimiento(new TransaccionLedger
        {
            BilleteraId = billeteraComprador.Id,
            Tipo = TipoMovimiento.Retencion,
            Monto = command.Monto,
            Fecha = ahora,
            SubastaId = subasta.Id
        });

        var nuevaPuja = new Puja
        {
            SubastaId = subasta.Id,
            CompradorId = command.CompradorId,
            Monto = command.Monto,
            FechaPuja = ahora
        };
        _subastaRepository.AgregarPuja(nuevaPuja);

        subasta.PujaActualMonto = command.Monto;

        var tiempoRestante = subasta.FechaFin - ahora;
        var huboExtension = tiempoRestante.TotalSeconds <= SegundosVentanaAntiSniping;

        if (huboExtension)
        {
            subasta.FechaFin += ExtensionAntiSniping;

            _subastaRepository.AgregarAuditoria(new AuditoriaLog
            {
                Entidad = "Subasta",
                EntidadId = subasta.Id,
                Accion = "EXTENSION_TIEMPO",
                UsuarioId = null,
                DetalleJson = $"{{\"segundosRestantes\":{tiempoRestante.TotalSeconds}}}",
                Fecha = ahora
            });
        }

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            _unitOfWork.LimpiarSeguimiento();

            _subastaRepository.AgregarAuditoria(new AuditoriaLog
            {
                Entidad = "Subasta",
                EntidadId = subasta.Id,
                Accion = "PUJA_RECHAZADA_CONCURRENCIA",
                UsuarioId = command.CompradorId,
                DetalleJson = System.Text.Json.JsonSerializer.Serialize(new { monto = command.Monto }),
                Fecha = DateTime.UtcNow
            });

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            throw;
        }
        var notificacionPuja = new NuevaPujaNotificacion(command.CompradorId, $"Postor #{command.CompradorId}", command.Monto, ahora);
        await _notificadorSubastas.NotificarNuevaPujaAsync(subasta.Id, notificacionPuja, cancellationToken);

        if (huboExtension)
        {
            await _notificadorSubastas.NotificarExtensionAsync(subasta.Id, subasta.FechaFin, cancellationToken);
        }

        return nuevaPuja.Id;
    }

    private async Task RegistrarPujaRechazadaAsync(
        int subastaId, int usuarioId, string accion, object detalle, CancellationToken cancellationToken)
    {
        _subastaRepository.AgregarAuditoria(new AuditoriaLog
        {
            Entidad = "Subasta",
            EntidadId = subastaId,
            Accion = accion,
            UsuarioId = usuarioId,
            DetalleJson = System.Text.Json.JsonSerializer.Serialize(detalle),
            Fecha = DateTime.UtcNow
        });

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}