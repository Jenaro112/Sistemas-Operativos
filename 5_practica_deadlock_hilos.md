# Práctica de Hilos: Problema Deadlock

## Tema
**Deadlock / Bloqueo mutuo en programación concurrente**

## Objetivo
Comprender cómo se produce un **deadlock** cuando dos o más hilos quedan bloqueados esperando recursos que nunca se liberan, analizar su comportamiento en **Java, Python y C#**, e identificar estrategias para prevenirlo.

---

# 1. Concepto general

Un **deadlock** ocurre cuando dos o más hilos quedan esperando indefinidamente porque cada uno posee un recurso que el otro necesita para continuar.

En esta situación:

- Los hilos no finalizan.
- El programa queda detenido o congelado.
- Ningún hilo puede avanzar.
- El problema no siempre genera un error visible.

## Ejemplo conceptual

Dos personas quieren cruzar una puerta angosta al mismo tiempo:

- La persona A espera que B se mueva.
- La persona B espera que A se mueva.
- Ambas quedan bloqueadas.

En programación, esto sucede cuando dos hilos bloquean recursos en distinto orden.

---

# 2. Librerías utilizadas

## Java

```java
java.lang.Thread
```

En Java, la clase `Thread` permite crear hilos y la palabra clave `synchronized` permite bloquear objetos compartidos.

---

## Python

```python
import threading
import time
```

En Python, el módulo `threading` permite crear hilos y usar objetos `Lock` para bloquear recursos compartidos.

---

## C#

```csharp
using System;
using System.Threading;
```

En C#, la clase `Thread` permite crear hilos y la palabra clave `lock` permite proteger secciones críticas.

---

# 3. Ejemplo en Java

## Código

```java
public class DeadlockEjemplo {

    private static final Object recursoA = new Object();
    private static final Object recursoB = new Object();

    public static void main(String[] args) {

        Thread hilo1 = new Thread(() -> {
            synchronized (recursoA) {
                System.out.println("Hilo 1 bloqueó Recurso A");

                try {
                    Thread.sleep(1000);
                } catch (InterruptedException e) {
                    e.printStackTrace();
                }

                System.out.println("Hilo 1 espera Recurso B");
                synchronized (recursoB) {
                    System.out.println("Hilo 1 bloqueó Recurso B");
                }
            }
        });

        Thread hilo2 = new Thread(() -> {
            synchronized (recursoB) {
                System.out.println("Hilo 2 bloqueó Recurso B");

                try {
                    Thread.sleep(1000);
                } catch (InterruptedException e) {
                    e.printStackTrace();
                }

                System.out.println("Hilo 2 espera Recurso A");
                synchronized (recursoA) {
                    System.out.println("Hilo 2 bloqueó Recurso A");
                }
            }
        });

        hilo1.start();
        hilo2.start();
    }
}
```

## Explicación

En este ejemplo existen dos recursos compartidos: `recursoA` y `recursoB`.

El **Hilo 1** bloquea primero `recursoA` y luego intenta acceder a `recursoB`. Al mismo tiempo, el **Hilo 2** bloquea primero `recursoB` y luego intenta acceder a `recursoA`.

Como cada hilo tiene un recurso que el otro necesita, ambos quedan esperando indefinidamente. Esto produce un **deadlock**.

---

# 4. Ejemplo en Python

## Código

```python
import threading
import time

recurso_a = threading.Lock()
recurso_b = threading.Lock()

def tarea_1():
    with recurso_a:
        print("Hilo 1 bloqueó Recurso A")
        time.sleep(1)

        print("Hilo 1 espera Recurso B")
        with recurso_b:
            print("Hilo 1 bloqueó Recurso B")

def tarea_2():
    with recurso_b:
        print("Hilo 2 bloqueó Recurso B")
        time.sleep(1)

        print("Hilo 2 espera Recurso A")
        with recurso_a:
            print("Hilo 2 bloqueó Recurso A")

hilo1 = threading.Thread(target=tarea_1)
hilo2 = threading.Thread(target=tarea_2)

hilo1.start()
hilo2.start()

hilo1.join()
hilo2.join()
```

## Explicación

En Python se utilizan dos objetos `Lock`: `recurso_a` y `recurso_b`.

El primer hilo toma el bloqueo de `recurso_a` y luego intenta tomar `recurso_b`. El segundo hilo hace lo contrario: toma primero `recurso_b` y luego intenta tomar `recurso_a`.

El resultado es que ambos hilos quedan esperando. El programa no avanza porque ninguno libera el recurso que el otro necesita.

---

# 5. Ejemplo en C#

## Código

```csharp
using System;
using System.Threading;

class Program
{
    static readonly object recursoA = new object();
    static readonly object recursoB = new object();

    static void Main()
    {
        Thread hilo1 = new Thread(Tarea1);
        Thread hilo2 = new Thread(Tarea2);

        hilo1.Start();
        hilo2.Start();

        hilo1.Join();
        hilo2.Join();
    }

    static void Tarea1()
    {
        lock (recursoA)
        {
            Console.WriteLine("Hilo 1 bloqueó Recurso A");
            Thread.Sleep(1000);

            Console.WriteLine("Hilo 1 espera Recurso B");
            lock (recursoB)
            {
                Console.WriteLine("Hilo 1 bloqueó Recurso B");
            }
        }
    }

    static void Tarea2()
    {
        lock (recursoB)
        {
            Console.WriteLine("Hilo 2 bloqueó Recurso B");
            Thread.Sleep(1000);

            Console.WriteLine("Hilo 2 espera Recurso A");
            lock (recursoA)
            {
                Console.WriteLine("Hilo 2 bloqueó Recurso A");
            }
        }
    }
}
```

## Explicación

En C# se utilizan dos objetos como recursos compartidos y la palabra clave `lock` para bloquearlos.

El **Hilo 1** bloquea `recursoA` y luego espera `recursoB`. El **Hilo 2** bloquea `recursoB` y luego espera `recursoA`.

Como los recursos se solicitan en distinto orden, se genera un bloqueo circular. Ningún hilo puede continuar y el programa queda detenido.

---

# 6. Comparación entre lenguajes

| Lenguaje | Creación de hilos | Mecanismo de bloqueo | Riesgo de deadlock |
|---|---|---|---|
| Java | `Thread` | `synchronized` | Sí, si se bloquean recursos en distinto orden |
| Python | `threading.Thread` | `threading.Lock` | Sí, si los locks se adquieren de forma cruzada |
| C# | `Thread` | `lock` | Sí, si se bloquean objetos en orden incorrecto |

---

# 7. Cómo prevenir un deadlock

Algunas estrategias para evitar deadlocks son:

1. **Bloquear los recursos siempre en el mismo orden.**
2. **Evitar mantener bloqueos durante mucho tiempo.**
3. **No bloquear recursos innecesarios.**
4. **Usar timeouts cuando sea posible.**
5. **Diseñar secciones críticas pequeñas.**
6. **Evitar locks anidados si no son necesarios.**

---

# 8. Ejercicio para resolver

## Consigna

Modificar los ejemplos anteriores para evitar el deadlock.

El programa debe cumplir con lo siguiente:

- Mantener dos recursos compartidos: `RecursoA` y `RecursoB`.
- Crear dos hilos.
- Ambos hilos deben acceder a los dos recursos.
- El programa no debe quedar bloqueado.
- Los recursos deben bloquearse siempre en el mismo orden.

---

## Variante sugerida

Ambos hilos deben bloquear primero `RecursoA` y luego `RecursoB`.

De esta manera:

- Un hilo podrá ingresar primero.
- El otro esperará.
- Cuando el primero libere ambos recursos, el segundo continuará.
- No se producirá bloqueo circular.

---

# 9. Preguntas para responder

1. ¿Por qué se produce el deadlock en los ejemplos originales?
2. ¿Qué recurso tiene bloqueado cada hilo?
3. ¿Qué recurso espera cada hilo?
4. ¿Por qué el programa no finaliza?
5. ¿Qué cambio permite evitar el deadlock?
6. ¿Cuál es la diferencia entre esperar un recurso y quedar bloqueado indefinidamente?
7. ¿Qué recomendaciones aplicarías en un sistema real?

---

# 10. Conclusión

El **deadlock** es uno de los problemas más importantes de la programación concurrente. No ocurre por un error de sintaxis, sino por un mal diseño en la forma en que los hilos acceden a recursos compartidos.

Comprender este problema permite desarrollar aplicaciones más robustas, especialmente en sistemas donde múltiples tareas trabajan al mismo tiempo y comparten memoria, archivos, conexiones o estructuras de datos.

La clave para evitar deadlocks es diseñar correctamente el orden de acceso a los recursos y mantener las secciones críticas lo más simples y controladas posible.
