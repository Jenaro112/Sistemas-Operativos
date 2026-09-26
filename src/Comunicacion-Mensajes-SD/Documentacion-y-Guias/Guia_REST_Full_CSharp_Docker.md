# Guía práctica: Comunicación por REST Full en C# con Docker

## Objetivo
El objetivo de esta práctica es implementar un ejercicio de comunicación en sistemas distribuidos mediante paso de mensajes utilizando **REST Full API** en C#. Los participantes desarrollarán un **servicio API** que expone endpoints REST y un **cliente console** que los consume, desplegando ambos en Docker.

---

## 1. Crear la solución y proyectos

```bash
mkdir RestFullExercise && cd RestFullExercise

# Crear solución
dotnet new sln -n RestFullExercise

# Crear proyecto API REST Full
 dotnet new webapi -n RestApiService

# Crear proyecto cliente consola
 dotnet new console -n RestApiClient

# Agregar proyectos a la solución
 dotnet sln add RestApiService/RestApiService.csproj
 dotnet sln add RestApiClient/RestApiClient.csproj
```

---

## 2. Implementación del servicio REST Full

### 2.1 Controlador
Creamos un controlador `Controllers/InventoryController.cs` dentro de `RestApiService`:

```csharp
using Microsoft.AspNetCore.Mvc;

namespace RestApiService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private static readonly Dictionary<string, int> Stock = new()
        {
            ["P001"] = 100,
            ["P002"] = 50
        };

        [HttpGet("check/{productId}/{quantity}")]
        public IActionResult CheckStock(string productId, int quantity)
        {
            var available = Stock.TryGetValue(productId, out var stockQty) && stockQty >= quantity;
            return Ok(new { ProductId = productId, Available = available });
        }

        [HttpPost("order")]
        public IActionResult CreateOrder([FromBody] OrderRequest request)
        {
            if (Stock.TryGetValue(request.ProductId, out var stockQty) && stockQty >= request.Quantity)
            {
                Stock[request.ProductId] -= request.Quantity;
                return Ok(new { Success = true, Message = "Order created." });
            }
            return BadRequest(new { Success = false, Message = "Insufficient stock." });
        }
    }

    public class OrderRequest
    {
        public string ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
```

---

## 3. Cliente REST Full en C#

Editamos `RestApiClient/Program.cs`:

```csharp
using System.Net.Http.Json;

var client = new HttpClient();
client.BaseAddress = new Uri("http://restapi:80/");

// Check stock
var checkResponse = await client.GetFromJsonAsync<dynamic>("api/inventory/check/P001/3");
Console.WriteLine($"Check Stock: {checkResponse}");

// Crear pedido
var orderResponse = await client.PostAsJsonAsync("api/inventory/order", new { ProductId = "P001", Quantity = 3 });
var result = await orderResponse.Content.ReadFromJsonAsync<dynamic>();
Console.WriteLine($"Create Order: {result}");
```

---

## 4. Dockerizar la API y el Cliente

### 4.1 Dockerfile del API (`RestApiService/Dockerfile`)

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "RestApiService.dll"]
```

### 4.2 Dockerfile del cliente (`RestApiClient/Dockerfile`)

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "RestApiClient.dll"]
```

---

## 5. Docker Compose

```yaml
version: "3.9"
services:
  restapi:
    build: ./RestApiService
    container_name: restapi
    ports:
      - "5000:80"
  client:
    build: ./RestApiClient
    container_name: restapi-client
    depends_on:
      - restapi
```

---

## 6. Ejecutar la aplicación

```bash
docker-compose up --build
```
- La API estará disponible en `http://localhost:5000/api/inventory/...`.
- El cliente ejecutará llamadas al API y mostrará resultados en consola.

---

## 7. Extensiones sugeridas
- Agregar endpoints PUT/DELETE para gestión de inventario.
- Integrar Swagger/OpenAPI para documentación del API.
- Implementar persistencia con SQLite o SQL Server en Docker.
- Manejar errores y códigos HTTP apropiados.
- Probar con Postman o curl fuera de Docker.

---
