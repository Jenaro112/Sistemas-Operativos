# TP5 — Sincronización en Sistemas Distribuidos

**Licenciatura en Sistemas de Información · SO - Sistemas Distribuidos**

Implementación en **C# / .NET 8 / ASP.NET Core Web API / Docker Compose** de:

| Carpeta | Algoritmo | Aplicaciones |
|---|---|---|
| `Ejercicio1-Cristian/` | Cristian (reloj físico) | `TimeServer` (Web API) + `CristianClient` ×3 (servidores A, B, C) |
| `Ejercicio2-Berkeley/` | Berkeley (reloj físico) | `BerkeleyCoordinator` + `BerkeleyNode` ×3 |
| `Ejercicio3-Lamport/` | Lamport (reloj lógico) | `OrdersService`, `InventoryService`, `PaymentsService` (Web API) + `Lamport.Common` |
| `registros/` | — | Registros de ejecución de todas las pruebas |
| `RESPUESTAS.md` | — | Respuestas teóricas y cuadro comparativo (Ejercicio 4) |

> **Relojes simulados.** Ningún programa modifica el reloj del sistema operativo. Cada proceso tiene un
> reloj simulado = hora del SO + **desfase** (offset) + **deriva** (drift, en ppm). Así se pueden reproducir
> las diferencias del enunciado dentro de contenedores que comparten el mismo reloj del host.
> Las horas se muestran en UTC-3 (`DISPLAY_UTC_OFFSET_HOURS`).

Requisitos: Docker Desktop. Para correr sin Docker: .NET 8 SDK (`Sincronizacion-TP5.sln` abre todo en Visual Studio / Rider / VS Code).

---

## Ejercicio 1 — Algoritmo de Cristian

```bash
cd Ejercicio1-Cristian
docker compose up --build
```

| Contenedor | Rol | Desfase inicial | Deriva |
|---|---|---|---|
| `time-server` (puerto 5100) | Servidor de tiempo | +2 s (10:00:02) | 0 |
| `servidor-a` | Cliente | 0 s (10:00:00) | +500 ppm |
| `servidor-b` | Cliente | +8 s (10:00:08) | +1000 ppm |
| `servidor-c` | Cliente | −5 s (09:59:55) | −800 ppm |

Cada cliente, cada `SYNC_INTERVAL_SECONDS` (10 s):

1. Consulta `GET /api/time` del servidor de tiempo.
2. Registra **T0** (envío) y 3. **T1** (recepción) con su reloj local.
4. Calcula **RTT = T1 − T0** (toma `SAMPLES` muestras y se queda con la de menor RTT).
5. Estima la hora: **Ts + (RTT − I) / 2** (I = tiempo de procesamiento que informa el servidor).
6. Ajusta su **reloj lógico de aplicación** (`ApplicationClock` = reloj local + corrección), sin tocar el reloj del SO.
7. Muestra la hora antes, después y la diferencia.

Pruebas (con el sistema corriendo, ver `requests.http`):

```bash
# Latencia alta y asimétrica (ida 1200 ms, vuelta 200 ms)
curl -X POST http://localhost:5100/api/admin/network -H "Content-Type: application/json" -d "{\"requestDelayMs\":1200,\"responseDelayMs\":200,\"jitterMs\":300}"
# Servidor de tiempo que deja de responder / vuelve
curl -X POST http://localhost:5100/api/admin/down
curl -X POST http://localhost:5100/api/admin/up
# (o directamente: docker compose stop time-server)
```

Variables útiles de los clientes: `SAMPLES`, `MAX_RTT_MS` (descarta muestras con RTT mayor), `REQUEST_TIMEOUT_MS`, `SYNC_INTERVAL_SECONDS`.

---

## Ejercicio 2 — Algoritmo de Berkeley

```bash
cd Ejercicio2-Berkeley
docker compose --env-file escenarios/01-base.env up --build              # diferencias de 5, 10 y 20 s
docker compose --env-file escenarios/02-nodo-5-minutos.env up --build    # un nodo con 5 min (promedio simple)
docker compose --env-file escenarios/03-tolerante-a-fallas.env up --build # 5 min + promedio tolerante a fallas
```

| Contenedor | Rol | Desfase (escenario base) |
|---|---|---|
| `coordinador` | Coordinador | 0 s |
| `nodo-1` (5201) | Nodo | +5 s |
| `nodo-2` (5202) | Nodo | −10 s |
| `nodo-3` (5203) | Nodo | +20 s (300 s en escenarios 02/03) |

En cada ronda el coordinador: pide la hora a cada nodo (compensando RTT/2), calcula las diferencias,
promedia incluyendo su propio reloj, calcula el ajuste de cada uno (**promedio − diferencia**), envía
el ajuste con `POST /api/clock/adjust`, corrige su reloj y verifica la dispersión final. Se repite cada
`SYNC_INTERVAL_SECONDS`.

- `MAX_DEVIATION_SECONDS = 0` → promedio simple (Berkeley clásico).
- `MAX_DEVIATION_SECONDS > 0` → **promedio tolerante a fallas**: excluye del promedio los relojes que
  se alejan más de ese valor de la mediana (pero igual los corrige).

Pruebas con el sistema corriendo:

```bash
curl -X POST "http://localhost:5203/api/admin/offset?seconds=300"   # introducir un nodo con 5 minutos
curl -X POST http://localhost:5202/api/admin/disconnect              # desconectar un participante
curl -X POST http://localhost:5202/api/admin/reconnect
docker compose stop nodo-2                                           # desconexión "real"
docker compose stop coordinador                                      # falla del coordinador
```

Si el coordinador se cae, cada nodo lo detecta (`COORDINATOR_TIMEOUT_SECONDS`) y lo informa en su log.

---

## Ejercicio 3 — Relojes lógicos de Lamport

```bash
cd Ejercicio3-Lamport
docker compose up --build
```

| Servicio | Puerto | Reloj físico |
|---|---|---|
| `OrdersService` | 5301 | 0 s |
| `InventoryService` | 5302 | −4 s (atrasado) |
| `PaymentsService` | 5303 | −2 s (atrasado) |

```bash
# Crear un pedido: ejecuta la secuencia 1..9 del enunciado
curl -X POST http://localhost:5301/api/orders -H "Content-Type: application/json" -d "{\"product\":\"notebook\",\"quantity\":1,\"amount\":850000}"

# Traza de los 3 servicios: orden por reloj físico vs orden por Lamport
curl http://localhost:5301/api/trace/text

# Evento interno independiente (para analizar concurrencia)
curl -X POST http://localhost:5303/api/events/internal -H "Content-Type: application/json" -d "{\"description\":\"Conciliación bancaria\"}"
```

Reglas implementadas en `Lamport.Common/LamportClock.cs`:

| Evento | Regla |
|---|---|
| Interno | `L = L + 1` |
| Envío | `L = L + 1`, el mensaje lleva `L` (`LamportMessage<T>`) |
| Recepción | `L = max(L, T_mensaje) + 1` |

Cada servicio registra `Proceso | Evento | Reloj físico | Reloj lógico` en consola y en `logs/<Servicio>.log`.
Como InventoryService y PaymentsService tienen el reloj físico atrasado, en la traza aparecen
**recepciones con hora física anterior al envío**, mientras que Lamport siempre respeta el orden causal.

---

## Registros de ejecución

Cada `docker compose up` escribe los logs en `EjercicioN/logs/`. En `registros/` están los registros ya
generados de todas las pruebas:

```
registros/
├── ejercicio1-cristian/        rondas normales, RTT alto/asimétrico, RTT > máximo, servidor caído y recuperación
├── ejercicio2-berkeley/
│   ├── 01-base-5-10-20s/
│   ├── 02-nodo-5-minutos/
│   ├── 03-nodo-5-minutos-tolerante/
│   ├── 04-desconexion-nodo/
│   └── 05-falla-coordinador/
└── ejercicio3-lamport/         logs de los 3 servicios + trazas (orden físico vs Lamport)
```
