# Práctica: Nombrado en Sistemas Distribuidos (SO - SD)

Este repositorio contiene la resolución práctica paso a paso del caso práctico: **Plataforma distribuida de comercio electrónico**.

---

## Ejercicio 1 - Crear una red Docker

### Comandos ejecutados:
```bash
docker network create ecommerce-network
docker network ls
docker network inspect ecommerce-network
```

### Resultados obtenidos:
- **ID de la red:** `ecommerce-network` creada exitosamente con driver `bridge`.
- **Subred asignada:** `172.19.0.0/16` (Gateway: `172.19.0.1`).
- **Scope:** `local`.

---

### Análisis y respuestas:

1. **¿Qué función cumple una red Docker?**
   - Actúa como un switch virtual aislado (driver `bridge` por defecto en redes de usuario) que interconecta contenedores en una misma máquina host. Proporciona aislamiento de tráfico frente a otros contenedores o redes, asignación dinámica de direcciones IP privadas dentro de una subred dedicada y enrutamiento interno.

2. **¿Qué servicios pueden comunicarse en ella?**
   - Exclusivamente aquellos contenedores que hayan sido conectados explícitamente a `ecommerce-network` (mediante `--network ecommerce-network` o mediante la sección `networks` en Docker Compose). Contenedores que se encuentren en otras redes o en la red por defecto `bridge` no tienen visibilidad directa.

3. **¿Qué relación tiene con la resolución de nombres?**
   - En las redes creadas por el usuario (*user-defined networks*), Docker activa automáticamente su **servidor DNS interno integrado** (que corre en la dirección loopback de Docker `127.0.0.11`).
   - Gracias a este DNS, los contenedores pueden comunicarse utilizando el **nombre del contenedor** (por ejemplo, `inventory-service` o `orders-service`) como nombre de host (hostname), sin necesidad de conocer de antemano ni fijar direcciones IP rígidas.

---

## Ejercicio 2 - Crear el servicio de inventario

### Comandos ejecutados:
```bash
docker run -d --name inventory-service --network ecommerce-network nginx
docker ps
docker inspect inventory-service
```

### Datos registrados:
- **Nombre:** `/inventory-service`
- **IP:** `172.19.0.2` (asignada dinámicamente)
- **Red:** `ecommerce-network`
- **Estado:** `running`

### Análisis: Diferencia entre Nombre y Dirección
- **Nombre (Lógico):** Identifica **qué** es el servicio de manera unívoca y comprensible para humanos y aplicaciones (`inventory-service`). Permanece estable ante reinicios y despliegues.
- **Dirección (Red/Física):** Indica **dónde** está ubicado el servicio en la topología de red en un momento específico (`172.19.0.2`). Es volátil y puede cambiar en cualquier recreación del contenedor.

---

## Ejercicio 3 - Crear un cliente y resolver nombres

### Comandos ejecutados:
```bash
docker run -it --name orders-service --network ecommerce-network alpine
```

Dentro del contenedor Alpine:
```sh
apk add --no-cache bind-tools curl
ping -c 3 inventory-service
nslookup inventory-service
getent hosts inventory-service
```

### Análisis y respuestas:
1. **¿Quién resolvió el nombre?**
   - El servidor DNS interno integrado de Docker, ubicado en la IP `127.0.0.11` (puerto 53), configurado automáticamente en `/etc/resolv.conf` del contenedor.
2. **¿Fue necesario conocer la IP de antemano?**
   - No, el cliente solo necesitó conocer el identificador lógico (`inventory-service`).
3. **¿Qué ventaja presenta?**
   - **Transparencia de ubicación:** Permite desacoplar el cliente de la ubicación física de red del proveedor del servicio. Si `inventory-service` cambia de IP o de nodo, `orders-service` no requiere modificaciones.

---

## Ejercicio 4 - Cambio de dirección IP y transparencia de ubicación

### Comandos ejecutados:
```bash
docker stop inventory-service
docker rm inventory-service
docker run -d --name inventory-service --network ecommerce-network nginx
docker inspect inventory-service
```

Consulta desde el cliente `orders-service`:
```bash
docker exec -it orders-service nslookup inventory-service
```

### Respuestas al análisis:
1. **¿Cambió la IP?**
   - Sí (o está sujeta a asignación dinámica por el IPAM de Docker según el orden de creación/reutilización de la subred).
2. **¿Cambió el nombre?**
   - No. El nombre lógico sigue siendo `inventory-service`.
3. **¿Fue necesario modificar el cliente?**
   - No. El cliente `orders-service` no requirió ningún cambio de código ni reconfiguración.
4. **¿Qué relación tiene con transparencia de ubicación?**
   - Representa la esencia de la **transparencia de ubicación**: los clientes acceden a un recurso mediante un identificador lógico estable, mientras que el mecanismo de resolución (DNS interno) mapea dicho nombre a la dirección de red actual sin que el cliente perciba si el servicio fue destruido, recreado o reubicado.

---

## Ejercicio 5 - DNS tradicional vs Resolución interna de Docker

### Comandos ejecutados:
```bash
nslookup www.google.com
dig www.google.com
```

### Comparación técnica:

| Criterio | DNS Tradicional (Internet) | DNS Interno de Docker |
| :--- | :--- | :--- |
| **Espacio de nombres** | Jerárquico y global (`host.subdominio.dominio.tld.`) | Generalmente plano y de alcance local a la red virtual (`nombre-contenedor`). |
| **Arquitectura** | Distribuida y jerárquica a escala global (Servidores raíz, TLD, autoritativos, recursivos). | Centralizada en el daemon de Docker (`127.0.0.11`) para el host local. |
| **Dinamismo / Actualización** | Basado en TTL (Time-To-Live) y caché; propagación lenta (minutos u horas). | Inmediato y en tiempo real; los registros se crean o eliminan al instante con el ciclo de vida del contenedor. |
| **Propósito principal** | Mapear dominios públicos a IPs de servidores en Internet. | Descubrimiento de servicios (*Service Discovery*) básico entre microservicios de una misma red privada. |

---

## Ejercicio 6 - InventoryService en C# / ASP.NET Core

Se implementó el microservicio `InventoryService` con un endpoint en `api/inventory/{productId}` que retorna el ID del producto, el stock disponible y el nombre de la máquina/servidor (`Environment.MachineName`) que atendió la solicitud.

---

## Ejercicio 7 - OrdersService (Consumidor)

### Código del cliente HTTP:
```csharp
var client = new HttpClient();
var response = await client.GetAsync("http://inventory-service:8080/api/inventory/P001");
var content = await response.Content.ReadAsStringAsync();
Console.WriteLine(content);
```

### Análisis: IP directa vs Nombre lógico

| Criterio | Uso de IP directa (`http://172.18.0.5:8080`) | Uso de Nombre lógico (`http://inventory-service:8080`) |
| :--- | :--- | :--- |
| **Acoplamiento** | Alto acoplamiento a la topología física y de red. | Desacoplamiento total; comunicación a nivel semántico. |
| **Tolerancia a fallos / Reinicios** | Muy baja: si el contenedor se recrea o migra, cambia de IP y el cliente falla. | Alta: Docker DNS resuelve dinámicamente la IP vigente sin afectar al cliente. |
| **Mantenibilidad** | Requiere cambios de configuración o recompilación ante cualquier cambio de infraestructura. | Cero modificaciones en el cliente: el nombre permanece estable. |
| **Escalabilidad y Balanceo** | No permite escalar réplicas fácilmente sin un balanceador con IP fija previa. | Permite balanceo automático por DNS (Round-Robin) entre múltiples instancias. |

---

## Ejercicio 8 - Dockerizar los servicios

Se construyeron los Dockerfiles multi-etapa (*multi-stage build*) para empaquetar ambos servicios en contenedores ligeros con el runtime de ASP.NET Core 8.

- **InventoryService:** [`InventoryService/Dockerfile`](./InventoryService/Dockerfile)
- **OrdersService:** [`OrdersService/Dockerfile`](./OrdersService/Dockerfile)

Estructura de construcción:
1. **Etapa build:** Utiliza la imagen `mcr.microsoft.com/dotnet/sdk:8.0` para restaurar dependencias y compilar el binario optimizado en modo `Release`.
2. **Etapa runtime:** Utiliza la imagen reducida `mcr.microsoft.com/dotnet/aspnet:8.0` copiando únicamente el output publicado (`/app/publish`), minimizando el tamaño final y la superficie de ataque.

---

## Ejercicio 9 - Docker Compose

### Archivo `docker-compose.yml`:
```yaml
services:
  inventory-service:
    build: ./InventoryService
    networks:
      - ecommerce-network

  orders-service:
    build: ./OrdersService
    depends_on:
      - inventory-service
    networks:
      - ecommerce-network

networks:
  ecommerce-network:
    driver: bridge
```

### Comandos de ejecución:
```bash
docker compose up --build
docker compose ps
```

### Análisis: Nombres, Direcciones y Resolución

1. **Nombres:**
   - En Docker Compose, la clave del servicio (`inventory-service` y `orders-service`) se registra automáticamente como un alias DNS de red para ese contenedor.
   - Aunque el nombre del contenedor generado tenga sufijos (como `nombrado-sd-inventory-service-1`), cualquier servicio de la misma red puede resolverlo usando el alias lógico `inventory-service`.

2. **Direcciones:**
   - El motor de Docker asigna dinámicamente una dirección IP de la subred `ecommerce-network` a cada contenedor al momento de levantarlo.
   - Estas direcciones son efímeras y cambian entre despliegues o reinicios.

3. **Resolución:**
   - Cuando `OrdersService` invoca `http://inventory-service:8080/api/inventory/...`, la petición DNS viaja a `127.0.0.11:53` (servidor DNS interno de Docker).
   - Docker traduce instantáneamente el nombre `inventory-service` a la IP privada actual del contenedor.
   - Se logra **transparencia de ubicación total**: los servicios se comunican sin conocer de antemano las IPs de sus contrapartes.

---

## Ejercicio 10 - Recrear la infraestructura

### Comandos ejecutados:
```bash
# Consultar IPs antes de destruir
docker inspect -f '{{.Name}} => IP: {{range $k, $v := .NetworkSettings.Networks}}{{$v.IPAddress}}{{end}}' nombrado-sd-inventory-service-1 nombrado-sd-orders-service-1

# Destruir y volver a levantar
docker compose down
docker compose up -d

# Consultar IPs después
docker inspect -f '{{.Name}} => IP: {{range $k, $v := .NetworkSettings.Networks}}{{$v.IPAddress}}{{end}}' nombrado-sd-inventory-service-1 nombrado-sd-orders-service-1
```

### Determinación y análisis:
- **¿El cliente necesita modificaciones cuando cambia la IP?**
  - **No**, en absoluto. El cliente `OrdersService` no requiere ningún cambio de código, recompilación ni actualización de variables de entorno.
  - Al destruirse y crearse nuevamente los contenedores, el IPAM de Docker asigna nuevas direcciones IP dinámicas y el DNS interno actualiza inmediatamente sus tablas en memoria.
  - El cliente continúa utilizando la URI lógica `http://inventory-service:8080/api/inventory/...`, preservando la **transparencia de ubicación** y la resiliencia de la arquitectura orientada a servicios.

---

## Ejercicio 11 - Múltiples instancias y Balanceo de Carga

### Comprobación práctica opcional con Docker Compose:
```bash
docker compose up -d --scale inventory-service=3
docker compose ps
```

### Respuestas al análisis:

1. **¿Debería OrdersService conocer las tres IP?**
   - **No.** El cliente únicamente debe conocer el nombre lógico del servicio (`inventory-service`). No debe tener conocimiento de la topología ni de cuántas réplicas existen detrás.

2. **¿Qué problemas generaría que el cliente conozca las IPs?**
   - **Acoplamiento rígido:** Obliga al cliente a gestionar listas estáticas de direcciones.
   - **Pérdida de elasticidad:** Si se escala o desescala el servicio, habría que reconfigurar y reiniciar a todos los clientes.
   - **Sobrecarga de lógica:** El cliente debería programar algoritmos de balanceo, monitoreo de salud (*health checks*) y gestión de timeouts.
   - **Puntos de fallo:** Si una IP deja de existir o cambia, el cliente experimentará errores de conexión (*connection refused*).

3. **¿Qué mecanismo debería localizar las instancias?**
   - Un mecanismo de **Resolución de Nombres con Service Discovery** y/o un **Reverse Proxy / Load Balancer** (por ejemplo, el DNS interno de Docker con Round-Robin, un Service de Kubernetes, Consul, Envoy o Nginx).

4. **¿Cómo se relaciona con el balanceo de carga?**
   - Al asociar un único identificador lógico a múltiples direcciones IP, el componente de resolución puede repartir las peticiones entrantes entre las distintas instancias (mediante algoritmos como **Round-Robin** o menor número de conexiones), distribuyendo la carga de trabajo y optimizando el rendimiento global del sistema.

5. **¿Qué ocurre si una instancia falla?**
   - En un sistema distribuido con descubrimiento de servicios y chequeos de salud (*liveness/readiness probes*), la instancia caída se desasocia inmediatamente del nombre lógico.
   - Las resoluciones de nombres futuras solo devolverán las direcciones de las réplicas sanas, logrando **alta disponibilidad** y **transparencia ante fallos**.

---

## Ejercicio 12 - Service Discovery

### Análisis y respuestas:

1. **¿Qué información mantiene Service Discovery?**
   - Un registro dinámico o catálogo (*Service Registry*) que almacena:
     - Nombre lógico del servicio (ej. `inventory-service`).
     - Lista de direcciones IP y puertos de cada una de las instancias activas.
     - Estado de salud actual (*healthy* / *unhealthy*).
     - Metadatos adicionales (versión, región, entorno).

2. **¿Cómo se registran nuevas instancias?**
   - **Auto-registro (*Self-Registration*):** Al iniciar, el propio servicio se comunica con el registro mediante API/gRPC enviando sus datos de red.
   - **Registro por terceros (*Third-Party Registration* / Orquestador):** Un agente externo o el orquestador (como Docker o el Control Plane de Kubernetes) detecta la creación del contenedor y lo inscribe automáticamente.

3. **¿Cómo se detectan instancias no disponibles?**
   - **Health Checks activos:** El sistema de descubrimiento sondea periódicamente un endpoint de salud (ej. `GET /health` o comprobación TCP).
   - **Heartbeats (latidos / TTL):** La instancia debe enviar una señal periódica de vida; si expira el tiempo sin recibirla, el registro la da de baja.

4. **¿Qué relación existe con el nombrado?**
   - Service Discovery es la implementación moderna del concepto de **resolución de nombres** en sistemas distribuidos efímeros y dinámicos. Vincula un nombre lógico constante con una topología de direcciones físicas que muta continuamente en tiempo real.

5. **¿Por qué es importante en microservicios?**
   - En microservicios, las instancias son efímeras: se crean, escalan, fallan y se mueven de nodo en cuestión de segundos.
   - Service Discovery elimina la necesidad de configurar IPs de forma estática, garantizando **transparencia de ubicación**, elasticidad y alta disponibilidad sin intervención manual.

---

## Ejercicio 13 - Kubernetes (Service y Abstracción de Pods)

### Análisis y respuestas:

1. **¿Qué problema de nombrado resuelve Kubernetes Service?**
   - Resuelve la **volatilidad y efimeridad de los Pods**. 
   - Los Pods se destruyen, escalan o migran constantemente, adquiriendo IPs nuevas e impredecibles.
   - Un `Kubernetes Service` define un **punto de entrada estable con nombre DNS persistente** (ej. `inventory-service.default.svc.cluster.local`) y una IP virtual fija (*ClusterIP*), aislando a los clientes de los cambios en los Pods.

2. **¿Por qué no conviene referenciar directamente las IP de los Pods?**
   - Porque las IPs de los Pods no son estables: si un Pod muere o se actualiza una versión (rolling update), su IP desaparece para siempre.
   - Además, referenciar una IP individual impediría realizar balanceo de carga automático entre múltiples réplicas.

3. **¿Qué ocurre si un Pod es reemplazado?**
   - El plano de control de Kubernetes (*Endpoints/EndpointSlice Controller*) detecta el nuevo Pod y su nueva IP, actualizando automáticamente la lista de destinos del Servicio.
   - El componente de red (`kube-proxy` o eBPF) enruta el tráfico hacia el nuevo Pod sin perder peticiones.

4. **¿El cliente necesita conocer el cambio?**
   - **No.** El cliente continúa comunicándose mediante el nombre lógico `inventory-service` y la misma *ClusterIP* virtual.

5. **¿Cómo se relaciona con la transparencia de ubicación?**
   - Proporciona **transparencia de ubicación absoluta**: el cliente no sabe (ni necesita saber) en qué servidor físico/nodo del clúster corre el Pod, cuántas réplicas existen ni qué IPs tienen. El cliente solo consume un nombre y Kubernetes gestiona el enrutamiento.

---

## Ejercicio 14 - Comparación de Mecanismos de Nombrado

### Tabla comparativa general:

| Mecanismo | Nombre utilizado | Quién resuelve | Destino | ¿Puede cambiar la IP? |
| :--- | :--- | :--- | :--- | :--- |
| **DNS Internet** | `www.ejemplo.com` (FQDN jerárquico) | Servidores DNS públicos y autoritativos | Servidor Web / Host en Internet | **Sí** (con propagación según TTL). |
| **Docker** | `inventory-service` (alias de servicio) | DNS interno embebido de Docker (`127.0.0.11`) | Contenedor en red virtual (*bridge*) | **Sí** (en tiempo real ante reinicios). |
| **Kubernetes** | `inventory-service` (o `service.ns.svc.cluster.local`) | CoreDNS + kube-proxy / eBPF (ClusterIP) | Pods efímeros en el clúster | **Sí** (las IPs de los Pods cambian continuamente de forma transparente). |
| **IP directa** | `172.18.0.5` (sin nombre) | *No existe resolución* | Host o Contenedor específico | **No**: la referencia queda obsoleta y la conexión falla (*tight coupling*). |

---

## Ejercicio 15 - Conclusión Técnica Final

### Cadena de Nombrado y Resolución:

$$\text{NOMBRE} \longrightarrow \text{RESOLUCIÓN} \longrightarrow \text{DIRECCIÓN} \longrightarrow \text{ENTIDAD / SERVICIO}$$

1. **NOMBRE (Identificador Lógico):**
   - Representa **qué** es el recurso (ej. `inventory-service`).
   - Es una abstracción semántica, estable, humanamente legible e invariante en el tiempo.

2. **RESOLUCIÓN (Mecanismo de Indirección):**
   - Es el proceso dinámico que mapea el nombre lógico a una dirección de red física actual (implementado mediante DNS de Docker, Service Discovery de Kubernetes, Consul, etc.).
   - Introduce el nivel de indirección necesario para absorber la volatilidad de la infraestructura.

3. **DIRECCIÓN (Ubicación en Red):**
   - Representa **dónde** se encuentra el recurso en un momento determinado (ej. `172.19.0.4:8080`).
   - Es transitoria, efímera y dependiente de la topología del clúster o red virtual.

4. **ENTIDAD / SERVICIO (Instancia de Ejecución):**
   - Es el contenedor, Pod o proceso que efectivamente procesa la solicitud (ej. el proceso ASP.NET Core de inventario).
   - Puede ser una o múltiples réplicas que se crean, destruyen o reubican continuamente.

---

### Importancia fundamental de los nombres en Sistemas Distribuidos:

1. **Transparencia de Ubicación (*Location Transparency*):**
   - Permite que los clientes (como `OrdersService`) consuman recursos distribuidos a través de abstracciones estables sin acoplarse a direcciones físicas, nodos de cómputo o topologías cambiantes.

2. **Elasticidad y Escalabilidad Horizontal:**
   - Facilita asociar un único nombre lógico a múltiples instancias paralelas. El mecanismo de resolución distribuye el tráfico equitativamente (balanceo de carga) sin que el cliente deba gestionar listas de réplicas.

3. **Alta Disponibilidad y Tolerancia a Fallos:**
   - Si un nodo o contenedor falla, el orquestador lo destruye y crea uno nuevo con otra IP. El sistema de descubrimiento actualiza los registros en tiempo real; para el cliente la caída es transparente y el servicio sigue disponible.

4. **Desacoplamiento del Ciclo de Vida y Despliegues Continuos:**
   - Permite aplicar actualizaciones continuas (*rolling updates*, *canary*, *blue-green deployments*) y migrar cargas entre servidores o nubes sin requerir recompilar, reconfigurar ni reiniciar los sistemas clientes.












