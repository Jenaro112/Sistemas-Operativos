using System.Net.Http.Json;
using System.Text;
using Lamport.Common;
using Microsoft.AspNetCore.Mvc;

namespace OrdersService.Controllers;

/// <summary>
/// Junta los eventos de los tres servicios y los muestra:
///   a) ordenados por RELOJ FÍSICO (cada servicio tiene su propio reloj desfasado -> orden incorrecto)
///   b) ordenados por LAMPORT (desempate por nombre de proceso -> orden total consistente con la causalidad)
/// </summary>
[ApiController]
[Route("api/trace")]
public class TraceController(LamportProcess process, IHttpClientFactory httpFactory) : ControllerBase
{
    [HttpGet]
    public async Task<IReadOnlyList<EventRecord>> Get() =>
        (await Collect()).OrderBy(e => e.Lamport).ThenBy(e => e.Process).ToList();

    /// <summary>GET /api/trace/text → tabla de texto para el informe.</summary>
    [HttpGet("text")]
    public async Task<ContentResult> Text()
    {
        var all = await Collect();
        var sb = new StringBuilder();
        sb.AppendLine("ORDEN POR RELOJ FÍSICO (cada servicio usa su propio reloj, con desfase):");
        sb.AppendLine(LamportProcess.Table(all.OrderBy(e => e.PhysicalTime)));
        sb.AppendLine("ORDEN POR RELOJ DE LAMPORT (L, desempate por nombre de proceso = orden total):");
        sb.AppendLine(LamportProcess.Table(all.OrderBy(e => e.Lamport).ThenBy(e => e.Process)));

        var phys = all.OrderBy(e => e.PhysicalTime).ToList();
        var anomalies = all.Where(r => r.Kind == EventKind.Recepcion)
            .Select(r => (recv: r, send: all.FirstOrDefault(s => s.Kind == EventKind.Envio && s.Process == r.Peer && s.Lamport == r.MessageLamport)))
            .Where(x => x.send is not null && x.recv.PhysicalTime < x.send.PhysicalTime)
            .ToList();
        if (anomalies.Count > 0)
        {
            sb.AppendLine("ANOMALÍAS DEL RELOJ FÍSICO (recepción con hora ANTERIOR al envío):");
            foreach (var (recv, send) in anomalies)
                sb.AppendLine($"  - '{send!.Description}' ({send.Process}, {Fmt.Time(send.PhysicalTime)}) -> " +
                              $"'{recv.Description}' ({recv.Process}, {Fmt.Time(recv.PhysicalTime)})   " +
                              $"pero Lamport {send.Lamport} < {recv.Lamport} ✔");
        }
        return Content(sb.ToString(), "text/plain; charset=utf-8");
    }

    private async Task<List<EventRecord>> Collect()
    {
        var all = new List<EventRecord>(process.Events);
        foreach (var name in new[] { "inventory", "payments" })
        {
            try
            {
                var ev = await httpFactory.CreateClient(name).GetFromJsonAsync<List<EventRecord>>("api/events");
                if (ev is not null) all.AddRange(ev);
            }
            catch (HttpRequestException) { /* servicio caído: se muestra lo disponible */ }
        }
        return all;
    }
}
