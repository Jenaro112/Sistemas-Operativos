# Guía práctica: Comunicación con Kafka en C# con Docker

## Objetivo
El objetivo de esta práctica es implementar un ejercicio de comunicación en sistemas distribuidos mediante **Apache Kafka**, utilizando **C#**. Los participantes desarrollarán un **Productor (Producer)** y un **Consumidor (Consumer)** y desplegarán ambos servicios junto con Kafka en **Docker**.

---

## 1. Crear la solución y proyectos

```bash
mkdir KafkaExercise && cd KafkaExercise
dotnet new sln -n KafkaExercise
dotnet new console -n Producer
dotnet new console -n Consumer
dotnet sln add Producer/Producer.csproj
dotnet sln add Consumer/Consumer.csproj
```

---

## 2. Configuración de Kafka y Zookeeper con Docker

Archivo `docker-compose.yml` completo:

```yaml
version: '3.8'
services:
  zookeeper:
    image: wurstmeister/zookeeper
    container_name: zookeeper
    ports:
      - "2181:2181"

  kafka:
    image: wurstmeister/kafka
    container_name: kafka
    ports:
      - "9092:9092"
    environment:
      KAFKA_BROKER_ID: 1
      KAFKA_ZOOKEEPER_CONNECT: zookeeper:2181
      KAFKA_ADVERTISED_LISTENERS: PLAINTEXT://kafka:9092
      KAFKA_OFFSETS_TOPIC_REPLICATION_FACTOR: 1
    depends_on:
      - zookeeper

  producer:
    build: ./Producer
    depends_on:
      - kafka

  consumer:
    build: ./Consumer
    depends_on:
      - kafka
```

Levantar servicios:

```bash
docker-compose up -d
```

---

## 3. Implementación del Producer (Productor)

Instalar paquete NuGet:

```bash
cd Producer
dotnet add package Confluent.Kafka
```

Archivo `Program.cs`:

```csharp
using System;
using System.Threading.Tasks;
using Confluent.Kafka;

var config = new ProducerConfig { BootstrapServers = "kafka:9092" };
using var producer = new ProducerBuilder<Null, string>(config).Build();

for (int i = 1; i <= 10; i++)
{
    var message = $"Mensaje {i}";
    var result = await producer.ProduceAsync("test_topic", new Message<Null, string> { Value = message });
    Console.WriteLine($"Enviado: {message} a {result.TopicPartitionOffset}");
    await Task.Delay(500);
}

Console.WriteLine("Presiona Enter para salir...");
Console.ReadLine();
```

---

## 4. Implementación del Consumer (Consumidor)

Instalar paquete NuGet:

```bash
cd ../Consumer
dotnet add package Confluent.Kafka
```

Archivo `Program.cs`:

```csharp
using System;
using System.Threading;
using Confluent.Kafka;

var config = new ConsumerConfig
{
    GroupId = "test-group",
    BootstrapServers = "kafka:9092",
    AutoOffsetReset = AutoOffsetReset.Earliest
};

using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
consumer.Subscribe("test_topic");

CancellationTokenSource cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

try
{
    while (true)
    {
        var cr = consumer.Consume(cts.Token);
        Console.WriteLine($"Recibido: {cr.Message.Value}");
    }
}
catch (OperationCanceledException)
{
    consumer.Close();
}
```

---

## 5. Dockerizar Producer y Consumer

**Dockerfile del Producer:**

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Producer.dll"]
```

**Dockerfile del Consumer:**

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Consumer.dll"]
```

---

## 6. Levantar todo con Docker Compose

```bash
docker-compose up --build
```

- La salida de Producer mostrará los mensajes enviados.
- La salida de Consumer mostrará los mensajes recibidos.
- Docker Compose levanta Zookeeper, Kafka, Producer y Consumer.

---

## 7. Extensiones y mejoras

- Configurar **particiones y replicación** en Kafka.
- Manejar **offsets y confirmaciones manuales**.
- Escalar múltiples consumidores para paralelismo.
- Monitorear con **Kafka Manager** o **Confluent Control Center**.

---


