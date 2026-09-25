using System;
using System.Threading.Tasks;
using Confluent.Kafka;

namespace OrdersService
{
    // * [KAFKA PRODUCER - Orders Service]
    // * Este servicio publica Eventos (cambios de estado) en un "Topic" de Kafka.
    class Program
    {
        static async Task Main(string[] args)
        {
            // * Configuramos el productor indicando dónde se encuentra el clúster (Broker) de Kafka.
            var config = new ProducerConfig { BootstrapServers = "localhost:9092" }; 
            
            using var producer = new ProducerBuilder<Null, string>(config).Build();

            // * Simulamos el ciclo de vida de un pedido (Event Sourcing / Event Streaming)
            string[] estados = { "creado", "confirmado", "enviado" };

            for (int i = 1; i <= 3; i++)
            {
                foreach (var estado in estados) 
                {
                    var message = $"Pedido #{i} - {estado}";
                    
                    // * Publicamos el mensaje de forma asíncrona en el topic "pedidos_topic".
                    // ? A diferencia de las colas de RabbitMQ, un Topic actúa como un registro histórico (Log).
                    // ! Si es el primer mensaje, Kafka creará el topic automáticamente gracias a su configuración por defecto.
                    var result = await producer.ProduceAsync("pedidos_topic", new Message<Null, string> { Value = message });
                    
                    Console.WriteLine($"[OrdersService] Evento publicado: {message} en offset {result.TopicPartitionOffset}");
                    await Task.Delay(1000); // Pausa de 1 segundo para simular tiempo
                }
            }

            Console.WriteLine("Presiona Enter para salir...");
            Console.ReadLine();
        }
    }
}
