using Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex) when (!context.Response.HasStarted)
        {
            switch (ex)
            {
                case DbUpdateConcurrencyException:
                    context.Response.StatusCode = StatusCodes.Status409Conflict;
                    await context.Response.WriteAsJsonAsync(new { error = "La subasta fue modificada por otra oferta. Intentá de nuevo." });
                    break;
                case EntidadNoEncontradaException entidadNoEncontrada:
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    await context.Response.WriteAsJsonAsync(new { error = entidadNoEncontrada.Message });
                    break;
                case SubastaNoVigenteException subastaNoVigente:
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsJsonAsync(new { error = subastaNoVigente.Message });
                    break;
                case MontoInsuficienteException montoInsuficiente:
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsJsonAsync(new { error = montoInsuficiente.Message });
                    break;
                case SaldoInsuficienteException saldoInsuficiente:
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsJsonAsync(new { error = saldoInsuficiente.Message });
                    break;
                case OperacionInvalidaException operacionInvalida:
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsJsonAsync(new { error = operacionInvalida.Message });
                    break;
                case CredencialesInvalidasException credencialesInvalidas:
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsJsonAsync(new { error = credencialesInvalidas.Message });
                    break;
                default:
                    _logger.LogError(ex, "Error no controlado procesando {Method} {Path}", context.Request.Method, context.Request.Path);
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    await context.Response.WriteAsJsonAsync(new { error = "Ocurrió un error inesperado en el servidor." });
                    break;
            }
        }
    }
}