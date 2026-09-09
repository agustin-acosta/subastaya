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

    public async Task<int> Handle(OfertarCommand command)
    {
        var subasta = await _subastaRepository.ObtenerPorIdAsync(command.SubastaId);
        if (subasta is null)
        {
            throw new KeyNotFoundException("La subasta no existe.");
        }

        // Regla propia (PROPUESTA, no exigida por la consigna): el vendedor
        // no puede pujar en su propia subasta.
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

        var billeteraComprador = await _billeteraRepository.ObtenerPorUsuarioIdAsync(command.CompradorId);
        if (billeteraComprador is null)
        {
            throw new KeyNotFoundException("El usuario comprador no existe.");
        }

        if (billeteraComprador.SaldoDisponible < command.Monto)
        {
            throw new SaldoInsuficienteException("Saldo disponible insuficiente para esta oferta.");
        }

        // Identificar al líder anterior (si existe) para liberar su retención.
        var pujaAnterior = await _subastaRepository.ObtenerUltimaPujaAsync(subasta.Id);
        if (pujaAnterior is not null && pujaAnterior.CompradorId != command.CompradorId)
        {
            var billeteraLiderAnterior = await _billeteraRepository.ObtenerPorUsuarioIdAsync(pujaAnterior.CompradorId);
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

        // Retener el monto del nuevo líder (una única vez).
        billeteraComprador.SaldoRetenido += command.Monto;

        _billeteraRepository.AgregarMovimiento(new TransaccionLedger
        {
            BilleteraId = billeteraComprador.Id,
            Tipo = TipoMovimiento.Retencion,
            Monto = command.Monto,
            Fecha = ahora,
            SubastaId = subasta.Id
        });

        // Crear la nueva puja.
        var nuevaPuja = new Puja
        {
            SubastaId = subasta.Id,
            CompradorId = command.CompradorId,
            Monto = command.Monto,
            FechaPuja = ahora
        };
        _subastaRepository.AgregarPuja(nuevaPuja);

        // Actualizar el monto líder de la subasta.
        subasta.PujaActualMonto = command.Monto;

        // Anti-sniping: si estamos dentro de la ventana crítica, extendemos.
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

        // Este es el punto crítico: acá EF Core compara el Version de cada fila
        // trackeada contra el valor que tenía cuando la leímos al principio.
        // Si alguien más ya modificó Subasta o alguna Billetera mientras tanto,
        // esto lanza DbUpdateConcurrencyException.
        await _unitOfWork.SaveChangesAsync();

        return nuevaPuja.Id;
    }
}