using Application.Interfaces;
using Domain;
using Domain.Exceptions;

namespace Application.Commands.Ofertar;

public class OfertarCommandHandler
{
    private readonly ISubastaRepository _subastaRepository;
    private readonly IBilleteraRepository _billeteraRepository;
    private readonly IUnitOfWork _unitOfWork;

    private const int SegundosVentanaAntiSniping = 60;
    private static readonly TimeSpan ExtensionAntiSniping = TimeSpan.FromMinutes(2);

    public OfertarCommandHandler(
        ISubastaRepository subastaRepository,
        IBilleteraRepository billeteraRepository,
        IUnitOfWork unitOfWork)
    {
        _subastaRepository = subastaRepository;
        _billeteraRepository = billeteraRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(OfertarCommand command, CancellationToken cancellationToken)
    {
        var subasta = await _subastaRepository.ObtenerPorIdAsync(command.SubastaId, cancellationToken);
        if (subasta is null)
        {
            throw new KeyNotFoundException("La subasta no existe.");
        }

        if (subasta.VendedorId == command.CompradorId)
        {
            throw new OperacionInvalidaException("No podés ofertar en tu propia subasta.");
        }

        var ahora = DateTime.UtcNow;

        if (subasta.Estado != EstadoSubasta.Activa || ahora < subasta.FechaInicio || ahora > subasta.FechaFin)
        {
            throw new SubastaNoVigenteException("La subasta no está vigente para recibir ofertas.");
        }

        var montoMinimoRequerido = (subasta.PujaActualMonto ?? subasta.PrecioBase) + subasta.IncrementoMinimo;
        if (command.Monto < montoMinimoRequerido)
        {
            throw new MontoInsuficienteException(
                $"El monto debe ser al menos {montoMinimoRequerido:C}.");
        }

        var billeteraComprador = await _billeteraRepository.ObtenerPorUsuarioIdAsync(command.CompradorId, cancellationToken);
        if (billeteraComprador is null)
        {
            throw new KeyNotFoundException("El usuario comprador no existe.");
        }

        if (billeteraComprador.SaldoDisponible < command.Monto)
        {
            throw new SaldoInsuficienteException("Saldo disponible insuficiente para esta oferta.");
        }

        var pujaAnterior = await _subastaRepository.ObtenerUltimaPujaAsync(subasta.Id, cancellationToken);
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
        if (tiempoRestante.TotalSeconds <= SegundosVentanaAntiSniping)
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

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return nuevaPuja.Id;
    }
}