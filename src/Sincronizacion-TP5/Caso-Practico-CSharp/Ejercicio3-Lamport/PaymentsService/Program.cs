using Lamport.Common;

// ============================================================
//  Ejercicio 3 - PaymentsService (procesa pagos)
// ============================================================

var builder = WebApplication.CreateBuilder(args);
// Reloj físico desfasado (atrasado 2 s respecto de OrdersService).
builder.AddLamportProcess("PaymentsService", defaultOffsetSeconds: -2);
var app = builder.Build();
app.MapControllers();
app.Run();
