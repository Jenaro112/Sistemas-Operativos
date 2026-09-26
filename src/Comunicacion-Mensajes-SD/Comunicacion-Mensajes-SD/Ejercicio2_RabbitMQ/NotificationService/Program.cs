using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NotificationService
{
    // * [RABBITMQ CONSUMER - Notification Service]
    // * Este servicio actúa como Consumidor de mensajes.
    // * Se encarga de escuchar permanentemente una cola y procesar lo que llega.
    class Program
    {
        static void Main(string[] args)
        {
            // * Se establece la misma configuración de conexión al Broker que el Productor.
            var factory = new ConnectionFactory() { HostName = "localhost" };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            // * Volvemos a declarar la cola por precaución.
            // ? Si este servicio arranca antes que el productor, RabbitMQ creará la cola para que
            // ? el consumidor no de error al intentar escuchar de una cola inexistente.
            channel.QueueDeclare(queue: "pedidos_queue",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            // * Creamos un objeto consumidor basado en eventos (Event-driven).
            var consumer = new EventingBasicConsumer(channel);
            
            // * Nos suscribimos al evento "Received", que se disparará automáticamente
            // * cada vez que RabbitMQ envíe un nuevo mensaje a este consumidor.
            consumer.Received += (model, ea) =>
            {
                // ? Traducimos los bytes recibidos de vuelta a texto (String).
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                
                // * Procesamiento del mensaje simulando envío de mail.
                Console.WriteLine($"[NotificationService] Consumido: {message} -> Enviando correo al cliente...");
            };

            // * Iniciamos el consumo explícitamente indicando de qué cola leer.
            // ! autoAck: true indica que el mensaje se borrará de la cola apenas RabbitMQ lo entregue,
            // ! sin importar si nuestro programa falló procesándolo. En entornos reales, suele usarse false
            // ! y se hace el "Acknowledge" manual tras procesarlo exitosamente para no perder datos ante errores.
            channel.BasicConsume(queue: "pedidos_queue",
                                 autoAck: true,
                                 consumer: consumer);

            Console.WriteLine("Esperando mensajes de pedidos... Presiona Enter para salir.");
            
            // * Mantenemos la aplicación viva escuchando en segundo plano.
            Console.ReadLine();
        }
    }
}
