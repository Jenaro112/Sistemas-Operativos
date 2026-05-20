# Práctica: Productor / Consumidor con Hilos

## Objetivo

Comprender el problema clásico **Productor / Consumidor** utilizando hilos en **Java, Python y C#**, analizando cómo varios hilos pueden compartir una estructura de datos común de forma segura mediante mecanismos de sincronización.

---

# 1. Descripción del problema

El problema **Productor / Consumidor** representa una situación donde uno o más hilos generan datos y otros hilos los consumen.

- El **productor** crea elementos y los coloca en una cola o buffer compartido.
- El **consumidor** toma elementos de esa cola y los procesa.
- Si la cola está llena, el productor debe esperar.
- Si la cola está vacía, el consumidor debe esperar.

Este problema permite estudiar conceptos fundamentales de concurrencia como:

- Hilos
- Cola compartida
- Sincronización
- Espera y notificación
- Control de acceso a recursos compartidos

---

# 2. Ejemplo en Java

## Librerías utilizadas

```java
import java.util.concurrent.BlockingQueue;
import java.util.concurrent.ArrayBlockingQueue;
```

## Código

```java
import java.util.concurrent.BlockingQueue;
import java.util.concurrent.ArrayBlockingQueue;

public class ProductorConsumidor {

    public static void main(String[] args) {
        BlockingQueue<Integer> cola = new ArrayBlockingQueue<>(5);

        Thread productor = new Thread(() -> {
            try {
                for (int i = 1; i <= 10; i++) {
                    cola.put(i);
                    System.out.println("Productor generó: " + i);
                    Thread.sleep(500);
                }
            } catch (InterruptedException e) {
                Thread.currentThread().interrupt();
            }
        });

        Thread consumidor = new Thread(() -> {
            try {
                for (int i = 1; i <= 10; i++) {
                    int valor = cola.take();
                    System.out.println("Consumidor procesó: " + valor);
                    Thread.sleep(800);
                }
            } catch (InterruptedException e) {
                Thread.currentThread().interrupt();
            }
        });

        productor.start();
        consumidor.start();
    }
}
```

## Explicación

En Java se utiliza `BlockingQueue`, una cola preparada para trabajar con hilos.  
El método `put()` agrega un elemento a la cola; si la cola está llena, el productor espera automáticamente.  
El método `take()` extrae un elemento; si la cola está vacía, el consumidor espera automáticamente.

De esta forma, Java resuelve gran parte de la sincronización internamente, evitando que el programador tenga que usar manualmente `wait()` y `notify()`.

---

# 3. Ejemplo en Python

## Librerías utilizadas

```python
import threading
import queue
import time
```

## Código

```python
import threading
import queue
import time

cola = queue.Queue(maxsize=5)

def productor():
    for i in range(1, 11):
        cola.put(i)
        print(f"Productor generó: {i}")
        time.sleep(0.5)

def consumidor():
    for i in range(1, 11):
        valor = cola.get()
        print(f"Consumidor procesó: {valor}")
        cola.task_done()
        time.sleep(0.8)

hilo_productor = threading.Thread(target=productor)
hilo_consumidor = threading.Thread(target=consumidor)

hilo_productor.start()
hilo_consumidor.start()

hilo_productor.join()
hilo_consumidor.join()

print("Proceso finalizado")
```

## Explicación

En Python se utiliza `queue.Queue`, que es una cola segura para hilos.  
El productor agrega elementos usando `put()`, mientras que el consumidor los toma usando `get()`.

Cuando la cola está llena, `put()` bloquea al productor hasta que exista espacio disponible.  
Cuando la cola está vacía, `get()` bloquea al consumidor hasta que aparezca un nuevo elemento.

La librería `threading` permite crear y ejecutar hilos, mientras que `time.sleep()` se usa para simular tiempos de producción y consumo.

---

# 4. Ejemplo en C#

## Librerías utilizadas

```csharp
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
```

## Código

```csharp
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static void Main()
    {
        BlockingCollection<int> cola = new BlockingCollection<int>(boundedCapacity: 5);

        Task productor = Task.Run(() =>
        {
            for (int i = 1; i <= 10; i++)
            {
                cola.Add(i);
                Console.WriteLine($"Productor generó: {i}");
                Thread.Sleep(500);
            }

            cola.CompleteAdding();
        });

        Task consumidor = Task.Run(() =>
        {
            foreach (int valor in cola.GetConsumingEnumerable())
            {
                Console.WriteLine($"Consumidor procesó: {valor}");
                Thread.Sleep(800);
            }
        });

        Task.WaitAll(productor, consumidor);

        Console.WriteLine("Proceso finalizado");
    }
}
```

## Explicación

En C# se utiliza `BlockingCollection<int>`, una colección concurrente que permite coordinar productores y consumidores de forma segura.

El productor agrega elementos con `Add()`. Si la colección está llena, el hilo productor queda esperando.  
El consumidor obtiene elementos mediante `GetConsumingEnumerable()`, que permite recorrer la colección a medida que llegan nuevos datos.

Cuando el productor termina, llama a `CompleteAdding()` para indicar que no se agregarán más elementos.

---

# 5. Comparación entre lenguajes

| Lenguaje | Mecanismo usado | Ventaja principal |
|---|---|---|
| Java | `BlockingQueue` | Maneja automáticamente espera y bloqueo |
| Python | `queue.Queue` | Simple y segura para hilos |
| C# | `BlockingCollection` | Muy útil para escenarios productor/consumidor |

---

# 6. Conceptos importantes

## Cola compartida

Es la estructura donde el productor deposita los datos y desde donde el consumidor los retira.

## Sincronización

Permite evitar que productor y consumidor accedan de forma incorrecta al recurso compartido.

## Bloqueo

Un hilo puede quedar esperando cuando no puede continuar:

- El productor espera si la cola está llena.
- El consumidor espera si la cola está vacía.

## Buffer limitado

La cola tiene una capacidad máxima. Esto evita que el productor genere datos ilimitadamente sin control.

---

# 7. Ejercicio para resolver

## Consigna

Modificar los ejemplos anteriores para implementar el siguiente escenario:

Una fábrica produce paquetes y varios empleados los procesan.

## Requisitos

- Crear una cola con capacidad máxima de 3 elementos.
- Crear 2 productores.
- Crear 3 consumidores.
- Cada productor debe generar 10 paquetes.
- Cada consumidor debe procesar paquetes mostrando su identificador.
- Mostrar por pantalla qué productor generó cada paquete.
- Mostrar por pantalla qué consumidor procesó cada paquete.
- Evitar errores de concurrencia.
- Finalizar correctamente todos los hilos.

## Ejemplo de salida esperada

```text
Productor 1 generó paquete 1
Productor 2 generó paquete 1
Consumidor 1 procesó paquete del Productor 1
Consumidor 2 procesó paquete del Productor 2
Productor 1 generó paquete 2
Consumidor 3 procesó paquete del Productor 1
...
Proceso finalizado
```

---

# 8. Preguntas para responder

1. ¿Por qué es necesario utilizar una cola sincronizada?
2. ¿Qué ocurre si el productor genera datos más rápido que el consumidor?
3. ¿Qué ocurre si el consumidor intenta consumir cuando la cola está vacía?
4. ¿Qué diferencia hay entre una cola común y una cola bloqueante?
5. ¿Qué ventajas tiene resolver este problema con estructuras thread-safe?

---

# 9. Conclusión

El problema Productor / Consumidor permite comprender uno de los patrones más importantes de la programación concurrente. A través de este ejercicio se observa cómo los hilos pueden colaborar entre sí compartiendo datos, pero también cómo es necesario coordinar correctamente el acceso a esos datos para evitar errores, bloqueos o inconsistencias.

