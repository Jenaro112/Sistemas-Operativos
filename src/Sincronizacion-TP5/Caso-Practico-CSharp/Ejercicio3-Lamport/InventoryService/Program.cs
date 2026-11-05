using Lamport.Common;

// ============================================================
//  Ejercicio 3 - InventoryService (verifica y reserva productos)
// ============================================================

var builder = WebApplication.CreateBuilder(args);
// Reloj físico ATRASADO respecto de OrdersService: así la "hora de recepción"
// queda antes que la "hora de envío" y se ve por qué no sirve para ordenar.
builder.AddLamportProcess("InventoryService", defaultOffsetSeconds: -4);
var app = builder.Build();
app.MapControllers();
app.Run();
