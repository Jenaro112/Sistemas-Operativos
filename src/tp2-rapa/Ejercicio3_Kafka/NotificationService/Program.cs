using System;
using System.Threading;
using Confluent.Kafka;

namespace NotificationService
{
    // * [KAFKA CONSUMER 1 - Notification Service]
    // * Reacciona a los eventos del sistema (ej: enviar mail al cliente cuando se envía el pedido)
    class Program
    {
        static void Main(string[] args)
        {
            // * Configuración crítica del consumidor Kafka
            var config = new ConsumerConfig
            {
                // * GroupId identifica al grupo de consumidores. 
                // ! Cada mensaje en un topic se entrega UNA SOLA VEZ a cada GroupId.
                // * Al darle un GroupId único a este servicio, garantizamos que reciba su propia copia de los mensajes
                // * independientemente de si AnalyticsService ya los leyó o no.
                GroupId = "notification-group", 
                BootstrapServers = "localhost:9092",
                
                // ? AutoOffsetReset.Earliest: 
                // * Si este consumidor se conecta por primera vez y no hay registro de por dónde iba leyendo,
                // * Kafka le enviará los mensajes DESDE EL PRINCIPIO de la historia. ¡Evita pérdida de datos!
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
            
            // * Nos suscribimos al topic deseado
            consumer.Subscribe("pedidos_topic");

            // * Token para poder detener el bucle infinito limpiamente al apretar Ctrl+C
            CancellationTokenSource cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

            Console.WriteLine("[NotificationService] Esperando eventos de pedidos...");
            try
            {
                // * Bucle continuo tradicional para consumo en Kafka ("Pull model")
                // ? A diferencia de RabbitMQ (donde el broker "empuja" los mensajes), 
                // ? en Kafka el cliente está continuamente pidiendo nuevos mensajes.
                while (true)
                {
                    var cr = consumer.Consume(cts.Token); // Se bloquea hasta que haya un mensaje
                    Console.WriteLine($"[NotificationService] Procesando: {cr.Message.Value} -> Enviando notificación");
                }
            }
            catch (OperationCanceledException)
            {
                // * Cierre ordenado para indicarle a Kafka que este consumidor se desconectó.
                consumer.Close();
            }
        }
    }
}
