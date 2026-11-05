using System.Collections.Concurrent;
using Lamport.Common;
using Microsoft.AspNetCore.Mvc;

namespace InventoryService.Controllers;

[ApiController]
[Route("api/inventory")]
public class InventoryController(LamportProcess process) : ControllerBase
{
    private static readonly ConcurrentDictionary<string, int> Stock = new(StringComparer.OrdinalIgnoreCase)
    {
        ["notebook"] = 5,
        ["mouse"] = 20,
        ["monitor"] = 2
    };

    /// <summary>GET /api/inventory → stock actual.</summary>
    [HttpGet]
    public IDictionary<string, int> Get() => Stock;

    /// <summary>POST /api/inventory/reserve (mensaje de OrdersService con marca de Lamport)</summary>
    [HttpPost("reserve")]
    public async Task<LamportMessage<InventoryResult>> Reserve(LamportMessage<InventoryRequest> msg)
    {
        var req = msg.Payload;

        // 3. InventoryService registra la recepción: L = max(L, T) + 1
        process.Receive(msg, $"Mensaje recibido de Orders ({req.OrderId})", req.OrderId);

        // 4. InventoryService verifica el inventario (evento interno)
        bool available;
        int remaining;
        lock (Stock)
        {
            var current = Stock.GetValueOrDefault(req.Product, 0);
            available = current >= req.Quantity;
            remaining = available ? current - req.Quantity : current;
            if (available) Stock[req.Product] = remaining;
        }
        process.Internal($"Inventario verificado: {req.Product} {(available ? "reservado" : "SIN STOCK")} (quedan {remaining})", req.OrderId);

        // 5. InventoryService envía una respuesta (envío)
        await ServiceSetup.NetworkDelay();
        return process.Send("OrdersService", $"Respuesta enviada a Orders ({req.OrderId})",
            new InventoryResult(req.OrderId, available, remaining), req.OrderId);
    }
}
