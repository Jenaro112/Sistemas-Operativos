using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

// * [REST API - Inventory Service]
// * Este servicio actúa como Servidor (Proveedor) en un entorno de comunicación SÍNCRONA.
// * Expone endpoints HTTP para consultar y descontar stock en tiempo real.

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// ? Base de datos simulada en memoria
// * En un entorno productivo real, esto sería reemplazado por SQL Server, MongoDB, etc.
var stock = new Dictionary<string, int>
{
    ["P001"] = 100,
    ["P002"] = 0
};

// * ENDPOINT: GET /api/inventory/check/{productId}/{quantity}
// * Verifica si hay suficiente stock disponible para un producto.
app.MapGet("/api/inventory/check/{productId}/{quantity}", (string productId, int quantity) =>
{
    var available = stock.TryGetValue(productId, out var stockQty) && stockQty >= quantity;
    return Results.Ok(new { ProductId = productId, Available = available });
});

// * ENDPOINT: POST /api/inventory/order
// * Intenta confirmar un pedido descontando el stock correspondiente.
// ! Si no hay stock suficiente, devuelve un estado HTTP 400 (Bad Request).
app.MapPost("/api/inventory/order", (OrderRequest req) => 
{
    if (stock.TryGetValue(req.ProductId, out var stockQty) && stockQty >= req.Quantity)
    {
        stock[req.ProductId] -= req.Quantity; // ? Descuenta el stock
        return Results.Ok(new { Success = true, Message = "Pedido confirmado." });
    }
    return Results.BadRequest(new { Success = false, Message = "Stock insuficiente." });
});

// ! Forzamos el puerto HTTP a 5000 para evitar asignaciones dinámicas del framework.
// * Esto garantiza que el OrdersService siempre sepa a qué puerto conectarse.
app.Run("http://localhost:5000");

// * DTO (Data Transfer Object) para estructurar el cuerpo (Body) de la petición POST.
public record OrderRequest(string ProductId, int Quantity);
