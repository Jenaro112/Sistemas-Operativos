using Microsoft.AspNetCore.Mvc;

namespace BerkeleyNode.Controllers;

/// <summary>Endpoints para las pruebas (inyectar desfase, simular desconexión).</summary>
[ApiController]
[Route("api/admin")]
public class AdminController(SimulatedClock clock, NodeState state) : ControllerBase
{
    /// <summary>GET /api/admin/status → estado del nodo (también responde estando "desconectado").</summary>
    [HttpGet("status")]
    public object Status() => new
    {
        node = state.Name,
        time = clock.Now,
        offsetMs = clock.Offset.TotalMilliseconds,
        disconnected = state.Disconnected
    };

    /// <summary>POST /api/admin/offset?seconds=300 → fija un desfase (ej. nodo con 5 minutos de diferencia).</summary>
    [HttpPost("offset")]
    public object SetOffset([FromQuery] double seconds)
    {
        clock.SetOffset(TimeSpan.FromSeconds(seconds));
        ExecLog.Write($"[ADMIN] Desfase forzado a {seconds:+0.###;-0.###;0} s -> hora {Fmt.Time(clock.Now)}");
        return Status();
    }

    /// <summary>POST /api/admin/disconnect → el nodo deja de responder al coordinador.</summary>
    [HttpPost("disconnect")]
    public object Disconnect()
    {
        state.Disconnected = true;
        ExecLog.Write("[ADMIN] *** Nodo DESCONECTADO (no responde al coordinador) ***");
        return Status();
    }

    /// <summary>POST /api/admin/reconnect → el nodo vuelve a responder.</summary>
    [HttpPost("reconnect")]
    public object Reconnect()
    {
        state.Disconnected = false;
        ExecLog.Write($"[ADMIN] *** Nodo RECONECTADO *** hora {Fmt.Time(clock.Now)}");
        return Status();
    }
}
