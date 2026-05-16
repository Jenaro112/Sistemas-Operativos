# Sistemas Operativos

## Integrantes

- Chatelain Agustin
- Galdini Jenaro
- Hartmann Kevin
- Rodriguez Lucas
- Sandillu Axel

## Como ejecutar C#

### Instalar .NET SDK

**Mac (con Homebrew):**
```bash
brew install dotnet
```

**Mac (sin Homebrew):** Descargar desde https://dotnet.microsoft.com/download

**Windows:** Descargar e instalar desde https://dotnet.microsoft.com/download

### Ejecutar

Desde la raiz del proyecto:

```bash
dotnet run --project src/csharp
```

Luego ingresar el numero de ejercicio (1-6, 9, 10) cuando lo solicite.

---

## Preguntas y Respuestas

### Ejercicio 1 - Concurrencia basica

**¿El orden de ejecucion es siempre el mismo?**
No, el orden varía cada vez que se ejecuta. Depende del scheduler del SO.

**¿Quien decide que hilo se ejecuta primero?**
El scheduler (planificador) del sistema operativo decide qué hilo se ejecuta y por cuánto tiempo.

**¿Que sucede si se eliminan las pausas?**
Un hilo tiende a ejecutarse completamente antes de que el otro comience, porque el cambio de contexto ocurre menos frecuentemente. Se pierde la intercalación visible entre hilos.

---

### Ejercicio 2 - Descarga + interfaz simulada

**¿Que pasaria si la descarga se ejecutara en el hilo principal?**
La interfaz se congelaría hasta que la descarga termine. La aplicación no respondería a clics, entradas del usuario, etc.

**¿Por que es importante esto en aplicaciones graficas?**
Porque el hilo principal (UI thread) debe estar siempre disponible para procesar eventos del usuario. Si se bloquea, la app parece congelada o "no responsive".

**¿Este caso es CPU-bound o I/O-bound?**
Es I/O-bound (ligado a E/S). La descarga simula una operación de entrada/salida (red/disco), no un cómputo intensivo del procesador.

---

### Ejercicio 3 - Productor / Consumidor

**¿Por que se necesita sincronizacion?**
Para evitar condiciones de carrera. Sin sincronización, productor y consumidor podrían acceder/modificar la cola al mismo tiempo, corrompiendo los datos (inconsistencia).

**¿Que error aparece si productor y consumidor acceden libremente a la cola?**
Data race / condición de carrera. Pueden perderse elementos, duplicarse, o la cola puede quedar en un estado inconsistente (ej. un poll() sobre una cola vacía da null o excepción).

**¿Que ventajas tiene usar una cola thread-safe?**
- Elimina la necesidad de gestionar locks manualmente.
- Reduce errores como deadlocks o condiciones de carrera.
- El código es más limpio y legible.

---

### Ejercicio 4 - Race Condition

**¿Por que el resultado puede ser incorrecto?**
Porque ++ no es atómico: es leer, sumar, escribir. Dos hilos pueden leer el mismo valor, incrementarlo y escribir, perdiéndose incrementos.

**¿Que es una seccion critica?**
Es la parte del código que accede a un recurso compartido y no debe ser ejecutada por más de un hilo a la vez para evitar inconsistencias.

**¿Que diferencia hay entre usar lock y operaciones atomicas?**
- **lock:** bloquea una sección crítica completa, permitiendo varias operaciones como una unidad atómica. Puede causar contención/sobrecarga.
- **atómicas** (AtomicInteger, Interlocked): operaciones a nivel de hardware (CAS), más rápidas y sin riesgo de deadlocks, pero solo para operaciones simples (incrementar, sumar, etc.).

---

### Ejercicio 5 - Deadlock

**¿Por que se produce el deadlock?**
Porque cada hilo tiene un recurso que el otro necesita y ninguno libera el suyo. Quedan esperándose mutuamente para siempre.

**¿Que condiciones deben cumplirse para que exista deadlock?**
Las 4 condiciones de Coffman:
1. **Exclusión mutua:** los recursos no son compartibles.
2. **Retención y espera:** un hilo retiene un recurso mientras espera otro.
3. **No desalojo:** un recurso no puede ser quitado forzosamente.
4. **Espera circular:** existe un ciclo de hilos esperando recursos.

**¿Como se puede prevenir?**
- Bloquear recursos siempre en el mismo orden (rompe espera circular).
- Usar timeout con tryLock (rompe retención y espera).
- Minimizar el tiempo dentro de secciones críticas.

---

### Ejercicio 6 - Starvation

**¿Que significa que un hilo sufra starvation?**
Significa que nunca (o muy raramente) obtiene acceso al recurso porque otros hilos lo acaparan constantemente.

**¿Es lo mismo que deadlock?**
No. En deadlock los hilos se bloquean mutuamente y ninguno avanza. En starvation hay hilos que avanzan mientras uno queda postergado indefinidamente, pero el sistema sigue funcionando.

**¿Como se puede reducir este problema?**
- Usar colas justas (fair queues) como ReentrantLock(true) en Java.
- Aumentar la prioridad del hilo postergado.
- Implementar envejecimiento (aging): aumentar prioridad cuanto más tiempo espera el hilo.

---

### Ejercicio 9 - Livelock

**¿Por que livelock no es igual a deadlock?**
En deadlock los hilos están bloqueados esperando. En livelock los hilos están activos, ejecutando código, pero sin progreso real.

**¿Los hilos estan detenidos?**
No, los hilos están activos y ejecutándose continuamente. Solo que ninguno logra completar su tarea.

**¿Como se podria resolver?**
- Introducir backoff aleatorio: cada hilo espera un tiempo aleatorio antes de reintentar, reduciendo colisiones.
- Usar un mecanismo de coordinación como un coordinator que asigna turnos de forma explícita.

---

### Ejercicio 10 - Sincronizacion

(Ejercicio demostrativo de los 5 mecanismos de sincronización: Mutex/Lock, Semaphore, Monitor/Condition, Atomic Operations, Critical Section. No incluye preguntas teóricas.)
