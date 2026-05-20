# Práctica: Sincronización de Hilos

## Objetivo

Comprender y aplicar diferentes técnicas de sincronización de hilos en **Java**, **Python** y **C#**, analizando cómo permiten proteger recursos compartidos, coordinar la ejecución concurrente y evitar errores como condiciones de carrera, accesos inconsistentes o bloqueos indebidos.

---

# 1. Conceptos principales

## Mutex

Un **Mutex** permite que solo un hilo acceda a un recurso compartido a la vez. Se usa cuando se necesita exclusión mutua estricta sobre un recurso crítico.

## Lock / Sección crítica

Un **Lock** protege una porción de código para que sea ejecutada por un solo hilo a la vez. Esa zona protegida se denomina **sección crítica**.

## Semaphore

Un **Semaphore** permite controlar cuántos hilos pueden acceder simultáneamente a un recurso. A diferencia del lock, puede permitir más de un hilo al mismo tiempo.

## Monitor

Un **Monitor** combina exclusión mutua con mecanismos de espera y notificación. Sirve para coordinar hilos cuando uno debe esperar hasta que otro produzca una condición.

## Atomic Operations

Las **operaciones atómicas** son instrucciones indivisibles. Se ejecutan completamente sin que otro hilo pueda interrumpirlas en medio de la operación.

## Critical Section

Una **sección crítica** es cualquier bloque de código que accede a datos compartidos y debe protegerse para evitar inconsistencias.

---

# 2. Ejemplo en Java

## Librerías utilizadas

```java
import java.util.concurrent.Semaphore;
import java.util.concurrent.atomic.AtomicInteger;
```

## Código

```java
import java.util.concurrent.Semaphore;
import java.util.concurrent.atomic.AtomicInteger;

public class SincronizacionJava {

    private static int contadorLock = 0;
    private static final Object lock = new Object();

    private static final Semaphore semaforo = new Semaphore(2);
    private static final AtomicInteger contadorAtomico = new AtomicInteger(0);

    public static void main(String[] args) throws InterruptedException {

        // LOCK / SECCIÓN CRÍTICA
        Thread hilo1 = new Thread(() -> incrementarConLock("Hilo 1"));
        Thread hilo2 = new Thread(() -> incrementarConLock("Hilo 2"));

        hilo1.start();
        hilo2.start();

        hilo1.join();
        hilo2.join();

        System.out.println("Contador con lock: " + contadorLock);

        // SEMAPHORE
        for (int i = 1; i <= 5; i++) {
            int id = i;
            new Thread(() -> usarRecursoLimitado("Hilo " + id)).start();
        }

        Thread.sleep(3000);

        // ATOMIC OPERATION
        Thread a = new Thread(() -> incrementarAtomico());
        Thread b = new Thread(() -> incrementarAtomico());

        a.start();
        b.start();

        a.join();
        b.join();

        System.out.println("Contador atómico: " + contadorAtomico.get());
    }

    public static void incrementarConLock(String nombre) {
        for (int i = 0; i < 1000; i++) {
            synchronized (lock) {
                // Sección crítica
                contadorLock++;
            }
        }
        System.out.println(nombre + " terminó incremento con lock.");
    }

    public static void usarRecursoLimitado(String nombre) {
        try {
            semaforo.acquire();
            System.out.println(nombre + " accede al recurso limitado.");
            Thread.sleep(1000);
            System.out.println(nombre + " libera el recurso.");
        } catch (InterruptedException e) {
            e.printStackTrace();
        } finally {
            semaforo.release();
        }
    }

    public static void incrementarAtomico() {
        for (int i = 0; i < 1000; i++) {
            contadorAtomico.incrementAndGet();
        }
    }
}
```

## Explicación

En Java, la palabra clave `synchronized` funciona como un mecanismo de exclusión mutua. Permite proteger una sección crítica para que solo un hilo acceda al contador compartido por vez.

El `Semaphore` permite limitar el acceso a un recurso. En el ejemplo, solo dos hilos pueden ingresar simultáneamente.

`AtomicInteger` permite incrementar un contador sin usar un lock explícito, porque la operación `incrementAndGet()` es atómica.

---

# 3. Ejemplo en Python

## Librerías utilizadas

```python
import threading
import time
```

## Código

```python
import threading
import time

contador_lock = 0
lock = threading.Lock()
semaforo = threading.Semaphore(2)

contador_atomico = 0
lock_atomico = threading.Lock()


def incrementar_con_lock(nombre):
    global contador_lock
    for _ in range(1000):
        with lock:
            # Sección crítica
            contador_lock += 1
    print(f"{nombre} terminó incremento con lock")


def usar_recurso_limitado(nombre):
    with semaforo:
        print(f"{nombre} accede al recurso limitado")
        time.sleep(1)
        print(f"{nombre} libera el recurso")


def incrementar_atomico_simulado():
    global contador_atomico
    for _ in range(1000):
        with lock_atomico:
            contador_atomico += 1


# LOCK / SECCIÓN CRÍTICA
hilo1 = threading.Thread(target=incrementar_con_lock, args=("Hilo 1",))
hilo2 = threading.Thread(target=incrementar_con_lock, args=("Hilo 2",))

hilo1.start()
hilo2.start()

hilo1.join()
hilo2.join()

print("Contador con lock:", contador_lock)

# SEMAPHORE
hilos = []
for i in range(5):
    h = threading.Thread(target=usar_recurso_limitado, args=(f"Hilo {i+1}",))
    hilos.append(h)
    h.start()

for h in hilos:
    h.join()

# ATOMIC OPERATION SIMULADA
hilo3 = threading.Thread(target=incrementar_atomico_simulado)
hilo4 = threading.Thread(target=incrementar_atomico_simulado)

hilo3.start()
hilo4.start()

hilo3.join()
hilo4.join()

print("Contador atómico simulado:", contador_atomico)
```

## Explicación

En Python, `threading.Lock()` permite proteger una sección crítica. La estructura `with lock:` asegura que el recurso se libere automáticamente al salir del bloque.

`threading.Semaphore(2)` permite que como máximo dos hilos accedan al recurso al mismo tiempo.

Python no tiene una clase estándar equivalente directa a `AtomicInteger` en `threading`, por lo que se simula una operación atómica protegiendo el incremento con un lock.

---

# 4. Ejemplo en C#

## Librerías utilizadas

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
```

## Código

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static int contadorLock = 0;
    static object lockObj = new object();

    static SemaphoreSlim semaforo = new SemaphoreSlim(2);
    static int contadorAtomico = 0;

    static async Task Main(string[] args)
    {
        // LOCK / SECCIÓN CRÍTICA
        Thread hilo1 = new Thread(() => IncrementarConLock("Hilo 1"));
        Thread hilo2 = new Thread(() => IncrementarConLock("Hilo 2"));

        hilo1.Start();
        hilo2.Start();

        hilo1.Join();
        hilo2.Join();

        Console.WriteLine($"Contador con lock: {contadorLock}");

        // SEMAPHORE
        Task[] tareas = new Task[5];

        for (int i = 0; i < 5; i++)
        {
            int id = i + 1;
            tareas[i] = Task.Run(() => UsarRecursoLimitado($"Hilo {id}"));
        }

        await Task.WhenAll(tareas);

        // ATOMIC OPERATION
        Thread hilo3 = new Thread(IncrementarAtomico);
        Thread hilo4 = new Thread(IncrementarAtomico);

        hilo3.Start();
        hilo4.Start();

        hilo3.Join();
        hilo4.Join();

        Console.WriteLine($"Contador atómico: {contadorAtomico}");
    }

    static void IncrementarConLock(string nombre)
    {
        for (int i = 0; i < 1000; i++)
        {
            lock (lockObj)
            {
                // Sección crítica
                contadorLock++;
            }
        }

        Console.WriteLine($"{nombre} terminó incremento con lock.");
    }

    static async Task UsarRecursoLimitado(string nombre)
    {
        await semaforo.WaitAsync();

        try
        {
            Console.WriteLine($"{nombre} accede al recurso limitado.");
            await Task.Delay(1000);
            Console.WriteLine($"{nombre} libera el recurso.");
        }
        finally
        {
            semaforo.Release();
        }
    }

    static void IncrementarAtomico()
    {
        for (int i = 0; i < 1000; i++)
        {
            Interlocked.Increment(ref contadorAtomico);
        }
    }
}
```

## Explicación

En C#, `lock` permite proteger una sección crítica. Solo un hilo puede ejecutar el bloque protegido a la vez.

`SemaphoreSlim` limita la cantidad de hilos o tareas que pueden acceder simultáneamente a un recurso. En este caso, se permiten dos accesos concurrentes.

`Interlocked.Increment()` realiza un incremento atómico sobre una variable compartida, evitando condiciones de carrera sin necesidad de escribir un lock manual.

---

# 5. Comparación entre Java, Python y C#

| Técnica | Java | Python | C# |
|---|---|---|---|
| Lock / Mutex | `synchronized`, `Lock` | `threading.Lock()` | `lock`, `Mutex` |
| Semaphore | `Semaphore` | `threading.Semaphore` | `SemaphoreSlim`, `Semaphore` |
| Monitor | `wait()`, `notify()` | `Condition` | `Monitor.Wait`, `Monitor.Pulse` |
| Atomic Operations | `AtomicInteger` | Se simula con `Lock` | `Interlocked` |
| Critical Section | Bloque `synchronized` | Bloque `with lock:` | Bloque `lock { }` |

---

# 6. Ejercicio para resolver

## Consigna

Construir un programa en **Java**, **Python** y **C#** que simule el acceso concurrente a una impresora compartida.

## Requisitos

- Crear 6 hilos que representen usuarios enviando documentos a imprimir.
- La impresora solo puede imprimir un documento a la vez.
- Cada hilo debe mostrar:
  - usuario esperando impresora;
  - usuario usando impresora;
  - usuario liberando impresora.
- Luego modificar el programa para simular 2 impresoras disponibles usando un semáforo.
- Agregar un contador de documentos impresos usando una operación segura.

## Preguntas para responder

1. ¿Qué parte del código representa la sección crítica?
2. ¿Qué diferencia existe entre usar `lock` y `semaphore`?
3. ¿Qué ocurre si se elimina la sincronización?
4. ¿Por qué las operaciones atómicas son útiles para contadores compartidos?
5. ¿En qué casos conviene usar Monitor o Condition?

---

# 7. Resultado esperado

Al finalizar la práctica, el estudiante debe poder:

- Identificar una sección crítica.
- Aplicar locks para proteger recursos compartidos.
- Usar semáforos para limitar accesos concurrentes.
- Comprender el rol de monitores y condiciones.
- Diferenciar exclusión mutua de coordinación entre hilos.
- Implementar incrementos seguros usando operaciones atómicas.
