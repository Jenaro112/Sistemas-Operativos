namespace Lamport.Common;

// ---- Mensajes de negocio (viajan dentro de LamportMessage<T>) ----

public record OrderRequest(string Product, int Quantity, decimal Amount);

public record InventoryRequest(string OrderId, string Product, int Quantity);
public record InventoryResult(string OrderId, bool Available, int RemainingStock);

public record PaymentRequest(string OrderId, decimal Amount);
public record PaymentResult(string OrderId, bool Approved, string TransactionId);

public record OrderResult(string OrderId, string Status, long FinalLamport, IReadOnlyList<EventRecord> Events);

public record InternalEventRequest(string Description);
