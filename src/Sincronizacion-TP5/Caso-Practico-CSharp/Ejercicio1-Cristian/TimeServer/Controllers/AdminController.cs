using Microsoft.AspNetCore.Mvc;

namespace TimeServer.Controllers;

public record NetworkConfigRequest(int? RequestDelayMs, int? ResponseDelayMs, int? JitterMs);

/// <summary>
/// Endpoints para las PRUEBAS: cambiar la latencia y simular la caída del servidor
/// sin reiniciar los contenedores.
/// </summary>
[ApiController]
[Route("api/admin")]
public class AdminController(NetworkSimulation network) : ControllerBase
{
    [HttpGet("network")]
    public NetworkSimulation GetNetwork() => network;

    /// <summary>POST /api/admin/network  {"requestDelayMs":800,"responseDelayMs":800,"jitterMs":300}</summary>
    [HttpPost("network")]
    public NetworkSimulation SetNetwork(NetworkConfigRequest req)
    {
        if (req.RequestDelayMs is { } a) network.RequestDelayMs = a;
        if (req.ResponseDelayMs is { } b) network.ResponseDelayMs = b;
        if (req.JitterMs is { } c) network.JitterMs = c;
        ExecLog.Write($"[ADMIN] Red simulada: ida={network.RequestDelayMs} ms, vuelta={network.ResponseDelayMs} ms, jitter={network.JitterMs} ms");
        return network;
    }

    /// <summary>POST /api/admin/down → el servidor deja de responder.</summary>
    [HttpPost("down")]
    public IActionResult Down()
    {
        network.Down = true;
        ExecLog.Write("[ADMIN] *** Servidor de tiempo FUERA DE SERVICIO (no responde) ***");
        return Ok(new { down = true });
    }

    /// <summary>POST /api/admin/up → el servidor vuelve a responder.</summary>
    [HttpPost("up")]
    public IActionResult Up()
    {
        network.Down = false;
        ExecLog.Write("[ADMIN] *** Servidor de tiempo RESTABLECIDO ***");
        return Ok(new { down = false });
    }
}
