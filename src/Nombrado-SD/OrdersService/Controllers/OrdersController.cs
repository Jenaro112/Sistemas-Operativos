using Microsoft.AspNetCore.Mvc;

namespace OrdersService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private static readonly HttpClient _client = new HttpClient();

        [HttpGet("{productId}")]
        public async Task<IActionResult> CreateOrder(string productId)
        {
            try
            {
                // El cliente utiliza el nombre lógico del servicio en lugar de una IP directa
                var response = await _client.GetAsync($"http://inventory-service:8080/api/inventory/{productId}");
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Respuesta recibida de inventario: {content}");

                return Ok(new
                {
                    Status = "Order Created",
                    InventoryResponse = content
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Status = "Error connecting to inventory",
                    Details = ex.Message
                });
            }
        }
    }
}
