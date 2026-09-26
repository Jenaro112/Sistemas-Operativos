using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace OrdersService
{
    // * [REST CLIENT - Orders Service]
    // * Este servicio actúa como Cliente.
    // * Se comunica de forma SÍNCRONA con el InventoryService usando el protocolo HTTP.
    class Program
    {
        static async Task Main(string[] args)
        {
            // * Configuramos el cliente HTTP apuntando al puerto fijo del InventoryService.
            var client = new HttpClient { BaseAddress = new Uri("http://localhost:5000/") };
            Console.WriteLine("Consultando stock para P001 (cantidad: 2)...");
            
            try 
            {
                // ? Paso 1: Consulta de disponibilidad (Acoplamiento temporal)
                // ! En la comunicación síncrona, el hilo de ejecución ESPERA (await) la respuesta.
                // ! Si InventoryService es lento, este servicio también se volverá lento.
                var checkResponse = await client.GetFromJsonAsync<InventoryCheckResponse>("api/inventory/check/P001/2");

                if (checkResponse != null && checkResponse.Available)
                {
                    Console.WriteLine("Stock disponible. Confirmando pedido...");
                    
                    // ? Paso 2: Si hay stock, confirmamos la creación del pedido vía POST.
                    var orderResponse = await client.PostAsJsonAsync("api/inventory/order", new { ProductId = "P001", Quantity = 2 });
                    
                    // * Mapeamos la respuesta JSON a un objeto C# para formatear el texto.
                    var result = await orderResponse.Content.ReadFromJsonAsync<OrderCreationResponse>();
                    
                    if (result != null && result.Success)
                    {
                        Console.WriteLine($"\n✅ EXITO: {result.Message}\n");
                    }
                    else
                    {
                        Console.WriteLine($"\n❌ RECHAZADO: {result?.Message}\n");
                    }
                }
                else
                {
                    Console.WriteLine("Stock insuficiente. No se puede confirmar el pedido.");
                }
            }
            catch(HttpRequestException)
            {
                // ! Tolerancia a fallos: Nula por defecto en arquitectura síncrona.
                // ! Si el InventoryService está apagado, la petición falla inmediatamente y se interrumpe el flujo.
                Console.WriteLine("Error conectando con InventoryService. Asegurate de que este corriendo.");
            }
        }
    }

    // * Registros auxiliares (Records) para mapear automáticamente el JSON de respuesta a objetos C#.
    record InventoryCheckResponse(string ProductId, bool Available);
    record OrderCreationResponse(bool Success, string Message);
}
