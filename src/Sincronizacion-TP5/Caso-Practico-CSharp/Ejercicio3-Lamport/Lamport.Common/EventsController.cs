using Microsoft.AspNetCore.Mvc;

namespace Lamport.Common;

/// <summary>Endpoints comunes a los tres servicios.</summary>
[ApiController]
[Route("api/events")]
public class EventsController(LamportProcess process) : ControllerBase
{
    /// <summary>GET /api/events → eventos registrados por este servicio (JSON).</summary>
    [HttpGet]
    public IReadOnlyList<EventRecord> Get() => process.Events;

    /// <summary>GET /api/events/text → los mismos eventos como tabla de texto.</summary>
    [HttpGet("text")]
    public ContentResult GetText() => Content(LamportProcess.Table(process.Events), "text/plain; charset=utf-8");

    /// <summary>GET /api/events/clock → valor actual del reloj de Lamport.</summary>
    [HttpGet("clock")]
    public object Clock() => new { process = process.Name, lamport = process.Lamport, physical = process.PhysicalNow };

    /// <summary>
    /// POST /api/events/internal  {"description":"..."}
    /// Genera un evento interno independiente (sirve para mostrar eventos CONCURRENTES).
    /// </summary>
    [HttpPost("internal")]
    public EventRecord Internal(InternalEventRequest req) => process.Internal(req.Description);
}
