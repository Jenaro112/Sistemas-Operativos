# Práctica de Hilos: Starvation

## 1. Objetivo de la práctica

Comprender el problema de **Starvation** en programación concurrente mediante ejemplos prácticos en **Java**, **Python** y **C#**, analizando cómo un hilo puede quedar postergado durante mucho tiempo porque otros hilos acceden repetidamente a un recurso compartido.

---

# 2. Concepto: ¿Qué es Starvation?

La **Starvation**, o inanición de hilos, ocurre cuando uno o más hilos no logran acceder a un recurso necesario para continuar su ejecución, porque otros hilos lo ocupan constantemente o tienen mayor prioridad.

A diferencia del **Deadlock**, el sistema no queda completamente bloqueado. Algunos hilos siguen ejecutándose, pero uno de ellos queda esperando demasiado tiempo o indefinidamente.

## Ejemplo conceptual

Imaginemos una impresora compartida:

- Varios usuarios envían trabajos pequeños constantemente.
- Un usuario con un trabajo largo nunca logra usar la impresora.
- La impresora funciona, pero ese usuario queda siempre postergado.

Eso representa un caso de **Starvation**.

---

# 3. Librerías utilizadas

## Java

```java
java.lang.Thread
java.util.concurrent.locks.ReentrantLock
```

Se utiliza `Thread` para crear hilos y `ReentrantLock` para controlar el acceso a una sección crítica.

## Python

```python
import threading
import time
```

Se utiliza `threading` para crear y sincronizar hilos, y `time` para simular tiempos de espera.

## C#

```csharp
using System;
using System.Threading;
```

Se utiliza `Thread` para crear hilos y `lock` para proteger el acceso al recurso compartido.

---

# 4. Ejemplo en Java

## Código

```java
import java.util.concurrent.locks.ReentrantLock;

public class StarvationExample {

    private static final ReentrantLock lock = new ReentrantLock();

    public static void main(String[] args) {

        Thread hiloPostergado = new Thread(() -> {
            while (true) {
                lock.lock();
                try {
                    System.out.println("Hilo postergado finalmente accedió al recurso");
                    break;
                } finally {
                    lock.unlock();
                }
            }
        });

        for (int i = 1; i <= 5; i++) {
            Thread hiloPrioritario = new Thread(() -> {
                for (int j = 0; j < 10; j++) {
                    lock.lock();
                    try {
                        System.out.println(Thread.currentThread().getName() + " usando el recurso");
                        try {
                            Thread.sleep(100);
                        } catch (InterruptedException e) {
                            e.printStackTrace();
                        }
                    } finally {
                        lock.unlock();
                    }
                }
            }, "Hilo prioritario " + i);

            hiloPrioritario.setPriority(Thread.MAX_PRIORITY);
            hiloPrioritario.start();
        }

        hiloPostergado.setPriority(Thread.MIN_PRIORITY);
        hiloPostergado.start();
    }
}
```

## Explicación

En este ejemplo se crean varios hilos que acceden repetidamente a un recurso protegido por un `ReentrantLock`. Estos hilos tienen prioridad alta, mientras que el hilo postergado tiene prioridad baja.

Aunque el hilo postergado también intenta acceder al recurso, puede tardar mucho más en lograrlo porque los otros hilos compiten constantemente por el mismo bloqueo.

Este comportamiento representa **Starvation**, ya que el programa sigue funcionando, pero un hilo queda relegado frente a otros.

---

# 5. Ejemplo en Python

## Código

```python
import threading
import time

lock = threading.Lock()

def hilo_prioritario(nombre):
    for i in range(10):
        with lock:
            print(f"{nombre} usando el recurso")
            time.sleep(0.1)


def hilo_postergado():
    while True:
        adquirido = lock.acquire(timeout=0.05)
        if adquirido:
            try:
                print("Hilo postergado finalmente accedió al recurso")
                break
            finally:
                lock.release()
        else:
            print("Hilo postergado sigue esperando...")


hilos = []

for i in range(5):
    t = threading.Thread(target=hilo_prioritario, args=(f"Hilo prioritario {i + 1}",))
    hilos.append(t)
    t.start()

postergado = threading.Thread(target=hilo_postergado)
postergado.start()

for t in hilos:
    t.join()

postergado.join()
```

## Explicación

En Python se utiliza un `Lock` para proteger el acceso al recurso compartido. Varios hilos prioritarios entran repetidamente a la sección crítica.

El hilo postergado intenta acceder al recurso usando `lock.acquire(timeout=0.05)`. Si no logra tomar el bloqueo dentro de ese tiempo, informa que sigue esperando.

Este ejemplo permite observar que el hilo no está bloqueado permanentemente, pero puede quedar postergado repetidamente por la actividad de otros hilos.

---

# 6. Ejemplo en C#

## Código

```csharp
using System;
using System.Threading;

class Program
{
    static readonly object recurso = new object();

    static void Main()
    {
        Thread hiloPostergado = new Thread(HiloPostergado);
        hiloPostergado.Priority = ThreadPriority.Lowest;

        for (int i = 1; i <= 5; i++)
        {
            int numero = i;
            Thread hiloPrioritario = new Thread(() => HiloPrioritario(numero));
            hiloPrioritario.Priority = ThreadPriority.Highest;
            hiloPrioritario.Start();
        }

        hiloPostergado.Start();
    }

    static void HiloPrioritario(int numero)
    {
        for (int i = 0; i < 10; i++)
        {
            lock (recurso)
            {
                Console.WriteLine($"Hilo prioritario {numero} usando el recurso");
                Thread.Sleep(100);
            }
        }
    }

    static void HiloPostergado()
    {
        while (true)
        {
            bool entro = Monitor.TryEnter(recurso, 50);

            if (entro)
            {
                try
                {
                    Console.WriteLine("Hilo postergado finalmente accedió al recurso");
                    break;
                }
                finally
                {
                    Monitor.Exit(recurso);
                }
            }
            else
            {
                Console.WriteLine("Hilo postergado sigue esperando...");
            }
        }
    }
}
```

## Explicación

En C# se utilizan varios hilos con prioridad alta que acceden repetidamente a un recurso protegido con `lock`.

El hilo postergado tiene prioridad baja e intenta entrar al recurso mediante `Monitor.TryEnter`, que permite intentar tomar el bloqueo durante un tiempo limitado.

Si no logra entrar, muestra un mensaje indicando que sigue esperando. Esto permite visualizar cómo un hilo puede quedar desplazado por otros hilos que acceden con mayor frecuencia o prioridad.

---

# 7. Comparación entre Java, Python y C#

| Lenguaje | Mecanismo usado | Observación |
|---|---|---|
| Java | `ReentrantLock`, `Thread.setPriority()` | Permite simular competencia por prioridad |
| Python | `threading.Lock`, `acquire(timeout)` | No maneja prioridades reales de hilos de forma directa |
| C# | `lock`, `Monitor.TryEnter`, `Thread.Priority` | Permite intentar acceso con timeout y modificar prioridad |

---

# 8. Diferencia entre Starvation y Deadlock

| Concepto | Descripción |
|---|---|
| Deadlock | Los hilos quedan bloqueados esperando recursos entre sí |
| Starvation | Un hilo no accede al recurso porque otros lo ocupan constantemente |
| Deadlock | El sistema queda detenido parcial o totalmente |
| Starvation | El sistema sigue funcionando, pero no de forma justa |

---

# 9. Estrategias para evitar Starvation

Algunas formas de reducir o evitar Starvation son:

- Usar colas justas de espera.
- Evitar prioridades extremas entre hilos.
- Limitar el tiempo de uso de un recurso.
- Usar locks con política de equidad cuando el lenguaje lo permita.
- Diseñar mecanismos donde todos los hilos tengan oportunidad de ejecutarse.

## Ejemplo en Java con lock justo

```java
ReentrantLock lock = new ReentrantLock(true);
```

El parámetro `true` indica que el lock debe intentar respetar el orden de llegada de los hilos.

---

# 10. Ejercicio para resolver

## Consigna

Modificar los ejemplos anteriores para construir un sistema donde:

- Existan 5 hilos llamados `Trabajador 1`, `Trabajador 2`, ..., `Trabajador 5`.
- Todos compitan por acceder a un recurso compartido.
- Cada vez que un hilo acceda al recurso, se debe incrementar un contador propio.
- El programa debe ejecutarse durante 10 segundos.
- Al finalizar, se debe mostrar cuántas veces accedió cada hilo.

## Objetivo del ejercicio

Detectar si algún hilo accedió muchas menos veces que los demás y analizar si existe una situación de **Starvation**.

---

# 11. Preguntas para responder

1. ¿Todos los hilos accedieron la misma cantidad de veces al recurso?
2. ¿Algún hilo quedó claramente postergado?
3. ¿Qué diferencia hay entre Starvation y Deadlock?
4. ¿Cómo se podría hacer más justo el acceso al recurso?
5. ¿Qué mecanismo de sincronización resultó más claro en cada lenguaje?

---

# 12. Conclusión

La **Starvation** demuestra que en programación concurrente no alcanza con evitar errores como el acceso simultáneo inseguro. También es necesario diseñar mecanismos de coordinación justos, donde todos los hilos tengan oportunidad de acceder a los recursos compartidos.

Una aplicación multihilo correcta no solo debe ser segura, sino también equilibrada y predecible.
