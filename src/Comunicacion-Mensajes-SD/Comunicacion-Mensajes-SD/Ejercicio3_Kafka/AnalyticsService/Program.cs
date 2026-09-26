using System;
using System.Threading;
using Confluent.Kafka;

namespace AnalyticsService
{
    // * [KAFKA CONSUMER 2 - Analytics Service]
    // * Reacciona a los MISMOS eventos que NotificationService, pero con otro propósito (Métricas).
    class Program
    {
        static void Main(string[] args)
        {
            var config = new ConsumerConfig
            {
                // * GroupId = "analytics-group"
                // ! ESTA ES LA CLAVE DE KAFKA (Publish-Subscribe real):
                // * Al usar un grupo distinto al de Notificaciones, este servicio lleva 
                // * su propio "puntero" (Offset) de lectura. Ambos pueden leer la misma historia 
                // * a velocidades distintas sin molestarse mutuamente.
                GroupId = "analytics-group",
                BootstrapServers = "localhost:9092",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
            consumer.Subscribe("pedidos_topic");

            CancellationTokenSource cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

            Console.WriteLine("[AnalyticsService] Recopilando estadisticas de eventos...");
            try
            {
                while (true)
                {
                    var cr = consumer.Consume(cts.Token);
                    
                    // * Aquí se simularía el guardado en una base de datos para armar gráficos, reportes, etc.
                    Console.WriteLine($"[AnalyticsService] Registrado para métricas: {cr.Message.Value}");
                }
            }
            catch (OperationCanceledException)
            {
                consumer.Close();
            }
        }
    }
}
