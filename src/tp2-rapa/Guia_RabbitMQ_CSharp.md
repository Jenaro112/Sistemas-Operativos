# Guía práctica: Comunicación con RabbitMQ en C# con Docker

## Objetivo
El objetivo de esta práctica es implementar un ejercicio de comunicación en sistemas distribuidos mediante **RabbitMQ**, un broker de mensajería, utilizando **C#**. Los participantes desarrollarán un **Productor (Publisher)**, un **Consumidor (Consumer)** y desplegarán ambos servicios junto con RabbitMQ en **Docker**.

---

## 1. Crear la solución y proyectos

```bash
mkdir RabbitMQExercise && cd RabbitMQExercise

# Crear solución
dotnet new sln -n RabbitMQExercise

# Crear proyecto Publisher
dotnet new console -n Publisher

# Crear proyecto Consumer
dotnet new console -n Consumer

# Agregar proyectos a la solución
dotnet sln add Publisher/Publisher.csproj
dotnet sln add Consumer/Consumer.csproj
```

---

## 2. Configuración de RabbitMQ

### 2.1 Docker Compose

Creamos `docker-compose.yml` en la raíz:

```yaml
version: "3.9"
services:
  rabbitmq:
    image: rabbitmq:3-management
    container_name: rabbitmq
    ports:
      - "5672:5672"   # Puerto AMQP
      - "15672:15672" # Panel de administración
```

Levantar RabbitMQ:
```bash
docker-compose up -d rabbitmq
```
- Panel de administración disponible en `http://localhost:15672` (user: guest, pass: guest)

---

## 3. Implementación del Publisher (Productor)

Instalar paquete NuGet `RabbitMQ.Client`:
```bash
cd Publisher
dotnet add package RabbitMQ.Client
```

Crear `Program.cs`:

```csharp
using System.Text;
using RabbitMQ.Client;

var factory = new ConnectionFactory() { HostName = "rabbitmq" }; // nombre del servicio en docker-compose
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

channel.QueueDeclare(queue: "test_queue",
                     durable: false,
                     exclusive: false,
                     autoDelete: false,
                     arguments: null);

for (int i = 1; i <= 10; i++)
{
    string message = $"Mensaje {i}";
    var body = Encoding.UTF8.GetBytes(message);

    channel.BasicPublish(exchange: "",
                         routingKey: "test_queue",
                         basicProperties: null,
                         body: body);
    Console.WriteLine($"[x] Enviado: {message}");
    System.Threading.Thread.Sleep(500);
}

Console.WriteLine("Presiona Enter para salir...");
Console.ReadLine();
```

---

## 4. Implementación del Consumer (Consumidor)

Instalar paquete NuGet `RabbitMQ.Client`:
```bash
cd ../Consumer
dotnet add package RabbitMQ.Client
```

Crear `Program.cs`:

```csharp
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

var factory = new ConnectionFactory() { HostName = "rabbitmq" };
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

channel.QueueDeclare(queue: "test_queue",
                     durable: false,
                     exclusive: false,
                     autoDelete: false,
                     arguments: null);

var consumer = new EventingBasicConsumer(channel);
consumer.Received += (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    Console.WriteLine($"[x] Recibido: {message}");
};

channel.BasicConsume(queue: "test_queue",
                     autoAck: true,
                     consumer: consumer);

Console.WriteLine("Esperando mensajes. Presiona Enter para salir...");
Console.ReadLine();
```

---

## 5. Dockerizar Publisher y Consumer

### 5.1 Dockerfile Publisher

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Publisher.dll"]
```

### 5.2 Dockerfile Consumer

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

## 6. Extender Docker Compose para los servicios

```yaml
version: "3.9"
services:
  rabbitmq:
    image: rabbitmq:3-management
    container_name: rabbitmq
    ports:
      - "5672:5672"
      - "15672:15672"

  publisher:
    build: ./Publisher
    depends_on:
      - rabbitmq

  consumer:
    build: ./Consumer
    depends_on:
      - rabbitmq
```

Levantar todo:
```bash
docker-compose up --build
```
- La salida de Publisher mostrará los mensajes enviados.
- La salida de Consumer mostrará los mensajes recibidos.

---

## 7. Extensiones y mejoras
- Implementar **durable queues** y **persistent messages**.
- Manejo de **acknowledgments** manual.
- Consumidores múltiples (load balancing).
- Integrar un **panel de monitoreo** de RabbitMQ.
- Implementar **exchanges** de tipo fanout, direct, topic.

---
