using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace TimeServer.Controllers;

/// <summary>Respuesta del servidor de tiempo.</summary>
/// <param name="ServerTime">Hora del servidor en el instante en que atendió la solicitud (UTC).</param>
/// <param name="ProcessingMs">Tiempo que tardó el servidor en procesar (I en la fórmula de Cristian).</param>
public record TimeResponse(string Server, DateTime ServerTime, double ProcessingMs);

[ApiController]
[Route("api/time")]
public class TimeController(SimulatedClock clock, NetworkSimulation network) : ControllerBase
{
    private static long _requests;

    /// <summary>GET /api/time → devuelve la hora del servidor de tiempo.</summary>
    [HttpGet]
    public async Task<ActionResult<TimeResponse>> Get([FromQuery] string? client, CancellationToken ct)
    {
        // Falla simulada: el servidor "no responde" (la solicitud queda colgada hasta el timeout del cliente).
        if (network.Down)
        {
            await Task.Delay(Timeout.Infinite, ct);
        }

        // Viaje de ida simulado.
        await Task.Delay(network.NextRequestDelay(), ct);

        var start = Stopwatch.GetTimestamp();
        var serverTime = clock.Now;
        var processing = Stopwatch.GetElapsedTime(start).TotalMilliseconds;

        var n = Interlocked.Increment(ref _requests);
        ExecLog.Write($"#{n} solicitud de '{client ?? HttpContext.Connection.RemoteIpAddress?.ToString()}' " +
                      $"-> hora del servidor {Fmt.Time(serverTime)}");

        // Viaje de vuelta simulado.
        await Task.Delay(network.NextResponseDelay(), ct);

        return new TimeResponse("servidor-tiempo", serverTime, processing);
    }
}
