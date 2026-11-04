using Lamport.Common;

// ============================================================
//  Ejercicio 3 - OrdersService (registra pedidos)
//  Orquesta: Inventory -> Payments, con reloj lógico de Lamport.
// ============================================================

var builder = WebApplication.CreateBuilder(args);
builder.AddLamportProcess("OrdersService", defaultOffsetSeconds: 0);

builder.Services.AddHttpClient("inventory", c =>
    c.BaseAddress = new Uri(Env.Str("INVENTORY_URL", "http://localhost:5302").TrimEnd('/') + "/"));
builder.Services.AddHttpClient("payments", c =>
    c.BaseAddress = new Uri(Env.Str("PAYMENTS_URL", "http://localhost:5303").TrimEnd('/') + "/"));

var app = builder.Build();
app.MapControllers();
app.Run();
