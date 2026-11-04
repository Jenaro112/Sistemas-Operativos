using Microsoft.AspNetCore.Mvc;

namespace BerkeleyNode.Controllers;

public record ClockReading(string Node, DateTime Time);
public record AdjustRequest(int Round, double AdjustmentMs);
public record AdjustResponse(string Node, DateTime Before, DateTime After, double AdjustmentMs);

[ApiController]
[Route("api/clock")]
public class ClockController(SimulatedClock clock, NodeState state) : ControllerBase
{
    /// <summary>GET /api/clock → el coordinador pide la hora actual del nodo.</summary>
    [HttpGet]
    public async Task<ActionResult<ClockReading>> Get(CancellationToken ct)
    {
        await WaitIfDisconnected(ct);
        var now = clock.Now;
        ExecLog.Write($"Coordinador solicita la hora -> {Fmt.Time(now)}");
        return new ClockReading(state.Name, now);
    }

    /// <summary>POST /api/clock/adjust → el nodo corrige su reloj simulado con el ajuste recibido.</summary>
    [HttpPost("adjust")]
    public async Task<ActionResult<AdjustResponse>> Adjust(AdjustRequest req, CancellationToken ct)
    {
        await WaitIfDisconnected(ct);
        var before = clock.Now;
        clock.Adjust(TimeSpan.FromMilliseconds(req.AdjustmentMs));
        state.LastAdjustmentUtc = DateTime.UtcNow;
        var after = clock.Now;
        ExecLog.Write($"Ronda {req.Round}: ajuste recibido {Fmt.Ms(req.AdjustmentMs)} | " +
                      $"antes {Fmt.Time(before)} -> después {Fmt.Time(after)} | " +
                      $"desfase acumulado vs SO {Fmt.Ms(clock.Offset)}");
        return new AdjustResponse(state.Name, before, after, req.AdjustmentMs);
    }

    private async Task WaitIfDisconnected(CancellationToken ct)
    {
        // Simula un nodo caído: no responde y el coordinador agota su timeout.
        if (state.Disconnected) await Task.Delay(Timeout.Infinite, ct);
    }
}
