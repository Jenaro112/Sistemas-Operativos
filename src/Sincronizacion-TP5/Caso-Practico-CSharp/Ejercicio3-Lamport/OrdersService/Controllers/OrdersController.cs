using System.Net.Http.Json;
using Lamport.Common;
using Microsoft.AspNetCore.Mvc;

namespace OrdersService.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController(LamportProcess process, IHttpClientFactory httpFactory) : ControllerBase
{
    private static int _sequence;

    /// <summary>
    /// POST /api/orders  {"product":"notebook","quantity":1,"amount":850000}
    /// Ejecuta la secuencia completa del enunciado (pasos 1 a 9).
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<OrderResult>> Create(OrderRequest req)
    {
        var orderId = $"P-{Interlocked.Increment(ref _sequence):000}";

        // 1. OrdersService recibe un pedido (evento interno)
        process.Internal($"Pedido recibido {orderId} ({req.Quantity} x {req.Product})", orderId);

        // 2. OrdersService envía una solicitud a InventoryService (envío)
        var invMsg = process.Send("InventoryService", $"Mensaje enviado a Inventory ({orderId})",
            new InventoryRequest(orderId, req.Product, req.Quantity), orderId);
        await ServiceSetup.NetworkDelay();
        var invResp = await httpFactory.CreateClient("inventory")
            .PostAsJsonAsync("api/inventory/reserve", invMsg);
        invResp.EnsureSuccessStatusCode();
        var invResult = (await invResp.Content.ReadFromJsonAsync<LamportMessage<InventoryResult>>())!;

        // (5) recepción de la respuesta de InventoryService
        process.Receive(invResult, $"Respuesta de Inventory recibida (stock {(invResult.Payload.Available ? "OK" : "insuficiente")})", orderId);

        if (!invResult.Payload.Available)
        {
            process.Internal($"Pedido rechazado {orderId} (sin stock)", orderId);
            return Result(orderId, "RECHAZADO_SIN_STOCK");
        }

        // 6. OrdersService solicita el procesamiento del pago (envío)
        var payMsg = process.Send("PaymentsService", $"Solicitud de pago enviada ({orderId})",
            new PaymentRequest(orderId, req.Amount), orderId);
        await ServiceSetup.NetworkDelay();
        var payResp = await httpFactory.CreateClient("payments")
            .PostAsJsonAsync("api/payments", payMsg);
        payResp.EnsureSuccessStatusCode();
        var payResult = (await payResp.Content.ReadFromJsonAsync<LamportMessage<PaymentResult>>())!;

        // (8) recepción de la confirmación de PaymentsService
        process.Receive(payResult, $"Confirmación de pago recibida ({payResult.Payload.TransactionId})", orderId);

        // 9. OrdersService confirma el pedido (evento interno)
        if (!payResult.Payload.Approved)
        {
            process.Internal($"Pedido rechazado {orderId} (pago rechazado)", orderId);
            return Result(orderId, "RECHAZADO_PAGO");
        }
        process.Internal($"Pedido confirmado {orderId}", orderId);
        return Result(orderId, "CONFIRMADO");
    }

    private OrderResult Result(string orderId, string status) =>
        new(orderId, status, process.Lamport, process.Events.Where(e => e.OrderId == orderId).ToList());
}
