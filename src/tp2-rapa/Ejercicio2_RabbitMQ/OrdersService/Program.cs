using System;
using System.Text;
using RabbitMQ.Client;

namespace OrdersService
{
    // * [RABBITMQ PRODUCER - Orders Service]
    // * Este servicio actúa como Productor de mensajes en un entorno de comunicación ASÍNCRONA.
    // * En vez de llamar a otro servicio, deja un mensaje en un "buzón" (cola) y sigue trabajando.
    class Program
    {
        static void Main(string[] args)
        {
            // * Configuramos la conexión hacia el Message Broker (RabbitMQ).
            // ? Si este servicio corriera dentro de Docker, el HostName sería "rabbitmq". 
            var factory = new ConnectionFactory() { HostName = "localhost" }; 

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            // * Declaración de la Cola (Queue)
            // ! Es fundamental declarar la cola antes de enviar mensajes para asegurar que exista.
            // * Si la cola ya existe, este comando no hace nada destructivo, simplemente la referencia.
            channel.QueueDeclare(queue: "pedidos_queue",
                                 durable: false, // ? Si es false, los mensajes se pierden si RabbitMQ se reinicia.
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            // * Simulamos la creación de 3 pedidos de forma consecutiva.
            for (int i = 1; i <= 3; i++)
            {
                string message = $"Pedido Creado #{i}";
                
                // ? RabbitMQ transfiere datos en formato binario (arreglo de bytes).
                var body = Encoding.UTF8.GetBytes(message);

                // * Publicación del mensaje en la cola "pedidos_queue".
                // * Al usar un "exchange" vacío (""), el routingKey funciona directamente como el nombre de la cola.
                channel.BasicPublish(exchange: "",
                                     routingKey: "pedidos_queue",
                                     basicProperties: null,
                                     body: body);
                                     
                Console.WriteLine($"[OrdersService] Publicado: {message}");
                System.Threading.Thread.Sleep(1000); // Simulamos tiempo de procesamiento
            }

            // * El productor terminó su trabajo inmediatamente, sin importar si el NotificationService 
            // * estaba encendido o apagado. ¡Esa es la principal ventaja del desacoplamiento asíncrono!
            Console.WriteLine("Presiona Enter para salir...");
            Console.ReadLine();
        }
    }
}
