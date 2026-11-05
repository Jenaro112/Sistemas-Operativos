using Lamport.Common;
using Microsoft.AspNetCore.Mvc;

namespace PaymentsService.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentsController(LamportProcess process) : ControllerBase
{
    private static int _tx;
    private static readonly decimal MaxAmount = (decimal)Env.Num("MAX_APPROVED_AMOUNT", 5_000_000);

    /// <summary>POST /api/payments (mensaje de OrdersService con marca de Lamport)</summary>
    [HttpPost]
    public async Task<LamportMessage<PaymentResult>> Process(LamportMessage<PaymentRequest> msg)
    {
        var req = msg.Payload;

        // 7. PaymentsService registra la solicitud (recepción) ...
        process.Receive(msg, $"Solicitud de pago recibida ({req.OrderId})", req.OrderId);

        // ... y la procesa (evento interno)
        var approved = req.Amount <= MaxAmount;
        var txId = $"TX-{Interlocked.Increment(ref _tx):0000}";
        process.Internal($"Pago procesado {txId}: ${req.Amount:0.##} {(approved ? "APROBADO" : "RECHAZADO")}", req.OrderId);

        // 8. PaymentsService envía una confirmación (envío)
        await ServiceSetup.NetworkDelay();
        return process.Send("OrdersService", $"Confirmación de pago enviada ({req.OrderId})",
            new PaymentResult(req.OrderId, approved, txId), req.OrderId);
    }
}
