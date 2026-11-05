# Ejercicios en C++ — Sincronización

Resolución de los ejercicios del final del cuestionario:

| Carpeta | Ejercicio | Programas |
|---|---|---|
| `A-Lamport/` | A. 4 computadoras que ejecutan P1, P2, P3, P4 **en ese orden**, sincronizadas con Lamport | `lamport` (un proceso por computadora) |
| `B-Berkeley/` | B. Sincronizar computadoras con el algoritmo de Berkeley | `berkeley_master`, `berkeley_slave` |
| `C-Cristian/` | C. Método de Cristian para sincronizar relojes | `cristian_server`, `cristian_client` |
| `common/` | Código compartido | `net.hpp` (sockets TCP para Windows y Linux), `util.hpp` (reloj simulado, logs, argumentos) |
| `registros/` | Salidas de las pruebas | Logs de cada escenario |

Los programas se comunican por **sockets TCP** con mensajes de texto de una línea, así que pueden correr en
computadoras distintas (pasando la IP de cada una) o todos en la misma PC con puertos distintos.

> **Relojes simulados.** Ningún programa cambia la hora del sistema operativo. Cada uno tiene un reloj
> simulado = hora del sistema + **desfase** (`--offset-s`) + **deriva** (`--drift-ppm`), para poder ver
> relojes distintos aunque corran en la misma máquina.

---

## Cómo compilar

**Con CMake** (Windows con MinGW o Visual Studio, Linux, macOS):

```bash
cmake -S . -B build
cmake --build build
```

**Con g++ directamente** (en Windows/MinGW agregar `-lws2_32` al final):

```bash
g++ -std=c++17 -pthread -o lamport          A-Lamport/lamport.cpp
g++ -std=c++17 -pthread -o berkeley_master  B-Berkeley/berkeley_master.cpp
g++ -std=c++17 -pthread -o berkeley_slave   B-Berkeley/berkeley_slave.cpp
g++ -std=c++17 -pthread -o cristian_server  C-Cristian/cristian_server.cpp
g++ -std=c++17 -pthread -o cristian_client  C-Cristian/cristian_client.cpp
```

**Con Docker** (cada contenedor hace de computadora): en cada carpeta `A-Lamport`, `B-Berkeley` o
`C-Cristian`, correr `docker compose up --build`. Los logs quedan en la carpeta `logs/` de cada ejercicio.

---

## A. Lamport — orden P1 → P2 → P3 → P4

Los cuatro procesos forman un anillo. Cada uno **espera el mensaje del anterior** antes de ejecutar su
parte, y todo mensaje lleva la marca de Lamport del emisor. P4 le avisa a P1 que terminó la ronda.

| Evento | Regla |
|---|---|
| Interno (ejecutar su parte) | `L = L + 1` |
| Envío | `L = L + 1`, el mensaje lleva `L` |
| Recepción | `L = max(L, L_mensaje) + 1` |

Ejecutar en 4 terminales (o 4 PCs, cambiando `127.0.0.1` por la IP del siguiente):

```bash
lamport --id 1 --port 6001 --next 127.0.0.1:6002
lamport --id 2 --port 6002 --next 127.0.0.1:6003 --offset-s -3
lamport --id 3 --port 6003 --next 127.0.0.1:6004 --offset-s 5 --initial-events 6
lamport --id 4 --port 6004 --next 127.0.0.1:6001 --offset-s -2 --initial-events 1
```

Se pueden lanzar en cualquier orden: P2, P3 y P4 se quedan esperando hasta que les llega el mensaje.
`--initial-events` hace que un proceso arranque con eventos locales propios, para que su contador
empiece adelantado. Otras opciones: `--rounds 2`, `--task-ms 400`, `--n 4`, `--log-dir logs`.

**Resultado** (`registros/A-Lamport/`): las tareas se ejecutan con L = 1 (P1), 4 (P2), 8 (P3) y 11 (P4) en la
ronda 1, y 14, 17, 20 y 23 en la ronda 2: siempre crecientes en el orden pedido. Se ven los dos casos de la
recepción:

- P2 recibe L = 2 teniendo 0, así que se adelanta a `max(0,2)+1 = 3`.
- P3 ya tenía 6 por sus eventos locales y recibe L = 5, así que conserva el suyo: `max(6,5)+1 = 7`.

Los relojes físicos están desfasados (P3 va 5 s adelantado, P2 3 s atrasado), por eso las horas físicas
de los logs no respetan el orden, pero los valores de Lamport sí.

---

## B. Berkeley — maestro y esclavos

El maestro, en cada ronda:

1. Pide la hora a cada esclavo (`TIME`) y mide el RTT para compensar la demora (hora + RTT/2).
2. Calcula la diferencia de cada reloj respecto del suyo.
3. Promedia las diferencias, incluida la suya (0). Con `--max-dev-s N` descarta del promedio los relojes
   que se alejan más de N segundos de la mediana (**promedio tolerante a fallas**).
4. Calcula el ajuste de cada uno: `promedio − diferencia`.
5. Envía a cada esclavo **su ajuste** (`ADJUST <ms>`) y corrige su propio reloj.
6. Verifica los relojes y repite cada `--interval` segundos.

```bash
berkeley_slave --name pc-1 --port 7001 --offset-s 5   --drift-ppm 800
berkeley_slave --name pc-2 --port 7002 --offset-s -10 --drift-ppm -600
berkeley_slave --name pc-3 --port 7003 --offset-s 20  --drift-ppm 300
berkeley_master --slaves 127.0.0.1:7001,127.0.0.1:7002,127.0.0.1:7003 --interval 10
```

Opciones del maestro: `--rounds N` (0 = infinito), `--max-dev-s N`, `--timeout-ms 2000`, `--offset-s`.

**Resultados** (`registros/B-Berkeley/`):

| Escenario | Promedio | Dispersión antes → después |
|---|---|---|
| 01 · relojes con +5, −10 y +20 s | +3,75 s | 30 s → 1 ms |
| 02 · pc-3 con +5 minutos, promedio simple | +73,75 s (todos se adelantan) | 310 s → 1 ms |
| 03 · pc-3 con +5 minutos, promedio tolerante (30 s) | −1,67 s (pc-3 EXCLUIDO del promedio) | 310 s → 1 ms |
| 04 · pc-2 se cae en la ronda 3 | se promedia sin pc-2 (SIN RESPUESTA) | el resto sigue sincronizado |

---

## C. Cristian — servidor de tiempo y clientes

El cliente, en cada ronda:

1. Guarda **T0** (hora local) y pide la hora al servidor.
2. Recibe **Ts** (hora del servidor) y guarda **T1**.
3. Calcula **RTT = T1 − T0** y estima la hora: **Ts + RTT/2**.
4. Toma varias muestras y usa la de menor RTT; descarta las que superan `--max-rtt-ms`.
5. Ajusta un **reloj lógico de la aplicación** (hora local + corrección), sin tocar el reloj del sistema.

```bash
cristian_server --port 8001 --offset-s 2
cristian_client --name pc-A --server 127.0.0.1:8001 --offset-s 0  --drift-ppm 500
cristian_client --name pc-B --server 127.0.0.1:8001 --offset-s 8  --drift-ppm 1000
cristian_client --name pc-C --server 127.0.0.1:8001 --offset-s -5 --drift-ppm -800
```

Opciones del cliente: `--samples 3`, `--interval 5`, `--rounds N`, `--max-rtt-ms 1000`, `--timeout-ms 2000`.
Opciones del servidor para pruebas: `--req-delay-ms` (demora de ida) y `--resp-delay-ms` (demora de vuelta).

**Resultados** (`registros/C-Cristian/`):

| Escenario | Qué pasa |
|---|---|
| 01 · red normal | pc-B corrige −6001 ms (estaba 6 s adelantado respecto del servidor), precisión ±0,1 ms |
| 02 · red lenta y asimétrica (ida 400 ms, vuelta 50 ms) | RTT ≈ 450 ms, precisión ±225 ms y un error de ≈175 ms = (400 − 50)/2, porque Cristian supone que ida y vuelta tardan lo mismo |
| 03 · RTT de 1300 ms (> máximo de 1000 ms) | se descartan las muestras y no se ajusta el reloj |
| 04 · el servidor se cae | el cliente mantiene la última corrección y sigue con la deriva de su reloj |
