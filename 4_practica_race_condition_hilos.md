# Práctica: Race Condition con Hilos en Java, Python y C#

## 1. Objetivo de la práctica

Comprender qué es una **Race Condition** en programación concurrente, observar cómo se produce cuando varios hilos acceden al mismo recurso compartido sin sincronización, y aplicar mecanismos de protección para evitar resultados incorrectos.

Una **Race Condition** ocurre cuando el resultado de un programa depende del orden en que se ejecutan los hilos. Como ese orden no está totalmente controlado por el programador, el resultado puede variar entre una ejecución y otra.

---

# 2. Problema a resolver

Se trabajará con un contador compartido.

La idea es crear varios hilos que incrementen una misma variable. Si no se protege correctamente el acceso a esa variable, el resultado final puede ser incorrecto.

## Resultado esperado

Si se crean 5 hilos y cada uno incrementa el contador 10000 veces, el resultado esperado es:

```text
5 x 10000 = 50000
```

Sin embargo, si los hilos acceden al contador sin sincronización, puede obtenerse un valor menor.

---

# 3. Concepto clave: sección crítica

Una **sección crítica** es una parte del código donde se accede a un recurso compartido.

En este caso, la sección crítica es:

```text
contador = contador + 1
```

Aunque parezca una sola operación, internamente puede dividirse en varios pasos:

```text
1. Leer el valor actual del contador
2. Sumar 1
3. Guardar el nuevo valor
```

Si dos hilos hacen esto al mismo tiempo, pueden sobrescribir el resultado del otro.

---

# 4. Ejemplo en Java

## Librerías utilizadas

```java
java.lang.Thread
java.util.concurrent.atomic.AtomicInteger
```

La clase `Thread` permite crear y ejecutar hilos.  
La clase `AtomicInteger` permite realizar operaciones atómicas sobre enteros.

---

## 4.1. Ejemplo con Race Condition

```java
public class RaceConditionJava {

    static int contador = 0;

    public static void main(String[] args) throws InterruptedException {
        int cantidadHilos = 5;
        int incrementosPorHilo = 10000;

        Thread[] hilos = new Thread[cantidadHilos];

        for (int i = 0; i < cantidadHilos; i++) {
            hilos[i] = new Thread(() -> {
                for (int j = 0; j < incrementosPorHilo; j++) {
                    contador++;
                }
            });
        }

        for (Thread hilo : hilos) {
            hilo.start();
        }

        for (Thread hilo : hilos) {
            hilo.join();
        }

        System.out.println("Resultado esperado: " + (cantidadHilos * incrementosPorHilo));
        System.out.println("Resultado obtenido: " + contador);
    }
}
```

---

## 4.2. Corrección usando `synchronized`

```java
public class RaceConditionJavaSolucion {

    static int contador = 0;

    public static synchronized void incrementar() {
        contador++;
    }

    public static void main(String[] args) throws InterruptedException {
        int cantidadHilos = 5;
        int incrementosPorHilo = 10000;

        Thread[] hilos = new Thread[cantidadHilos];

        for (int i = 0; i < cantidadHilos; i++) {
            hilos[i] = new Thread(() -> {
                for (int j = 0; j < incrementosPorHilo; j++) {
                    incrementar();
                }
            });
        }

        for (Thread hilo : hilos) {
            hilo.start();
        }

        for (Thread hilo : hilos) {
            hilo.join();
        }

        System.out.println("Resultado esperado: " + (cantidadHilos * incrementosPorHilo));
        System.out.println("Resultado obtenido: " + contador);
    }
}
```

---

## 4.3. Corrección usando `AtomicInteger`

```java
import java.util.concurrent.atomic.AtomicInteger;

public class RaceConditionJavaAtomic {

    static AtomicInteger contador = new AtomicInteger(0);

    public static void main(String[] args) throws InterruptedException {
        int cantidadHilos = 5;
        int incrementosPorHilo = 10000;

        Thread[] hilos = new Thread[cantidadHilos];

        for (int i = 0; i < cantidadHilos; i++) {
            hilos[i] = new Thread(() -> {
                for (int j = 0; j < incrementosPorHilo; j++) {
                    contador.incrementAndGet();
                }
            });
        }

        for (Thread hilo : hilos) {
            hilo.start();
        }

        for (Thread hilo : hilos) {
            hilo.join();
        }

        System.out.println("Resultado esperado: " + (cantidadHilos * incrementosPorHilo));
        System.out.println("Resultado obtenido: " + contador.get());
    }
}
```

---

## Explicación en Java

En Java, cuando varios hilos ejecutan `contador++`, pueden leer el mismo valor al mismo tiempo y luego guardar resultados superpuestos. Por eso el contador puede terminar con un valor menor al esperado.

La palabra clave `synchronized` obliga a que solo un hilo por vez ejecute el método `incrementar()`.  
`AtomicInteger`, por su parte, ofrece operaciones atómicas ya preparadas para escenarios concurrentes.

---

# 5. Ejemplo en Python

## Librerías utilizadas

```python
threading
```

La librería `threading` permite crear hilos y utilizar mecanismos de sincronización como `Lock`.

---

## 5.1. Ejemplo con Race Condition

```python
import threading

contador = 0
cantidad_hilos = 5
incrementos_por_hilo = 10000


def incrementar():
    global contador
    for _ in range(incrementos_por_hilo):
        contador += 1


hilos = []

for _ in range(cantidad_hilos):
    hilo = threading.Thread(target=incrementar)
    hilos.append(hilo)
    hilo.start()

for hilo in hilos:
    hilo.join()

print("Resultado esperado:", cantidad_hilos * incrementos_por_hilo)
print("Resultado obtenido:", contador)
```

---

## 5.2. Corrección usando `Lock`

```python
import threading

contador = 0
cantidad_hilos = 5
incrementos_por_hilo = 10000
lock = threading.Lock()


def incrementar():
    global contador
    for _ in range(incrementos_por_hilo):
        with lock:
            contador += 1


hilos = []

for _ in range(cantidad_hilos):
    hilo = threading.Thread(target=incrementar)
    hilos.append(hilo)
    hilo.start()

for hilo in hilos:
    hilo.join()

print("Resultado esperado:", cantidad_hilos * incrementos_por_hilo)
print("Resultado obtenido:", contador)
```

---

## Explicación en Python

En Python, varios hilos pueden intentar modificar la variable `contador` al mismo tiempo. La operación `contador += 1` no debe considerarse segura para este tipo de práctica, ya que implica leer, modificar y guardar.

El objeto `Lock` permite proteger la sección crítica. Con `with lock`, solo un hilo puede ejecutar el incremento en un momento determinado.

Aunque Python tiene el GIL, esto no elimina la necesidad de aprender sincronización, especialmente cuando se trabaja con recursos compartidos o con operaciones más complejas.

---

# 6. Ejemplo en C#

## Librerías utilizadas

```csharp
System
System.Threading
System.Threading.Tasks
```

`System.Threading` permite trabajar con hilos, bloqueos y operaciones atómicas.  
`System.Threading.Tasks` permite crear tareas concurrentes mediante `Task`.

---

## 6.1. Ejemplo con Race Condition

```csharp
using System;
using System.Threading.Tasks;

class Program
{
    static int contador = 0;

    static void Main()
    {
        int cantidadHilos = 5;
        int incrementosPorHilo = 10000;

        Task[] tareas = new Task[cantidadHilos];

        for (int i = 0; i < cantidadHilos; i++)
        {
            tareas[i] = Task.Run(() =>
            {
                for (int j = 0; j < incrementosPorHilo; j++)
                {
                    contador++;
                }
            });
        }

        Task.WaitAll(tareas);

        Console.WriteLine($"Resultado esperado: {cantidadHilos * incrementosPorHilo}");
        Console.WriteLine($"Resultado obtenido: {contador}");
    }
}
```

---

## 6.2. Corrección usando `lock`

```csharp
using System;
using System.Threading.Tasks;

class Program
{
    static int contador = 0;
    static readonly object bloqueo = new object();

    static void Main()
    {
        int cantidadHilos = 5;
        int incrementosPorHilo = 10000;

        Task[] tareas = new Task[cantidadHilos];

        for (int i = 0; i < cantidadHilos; i++)
        {
            tareas[i] = Task.Run(() =>
            {
                for (int j = 0; j < incrementosPorHilo; j++)
                {
                    lock (bloqueo)
                    {
                        contador++;
                    }
                }
            });
        }

        Task.WaitAll(tareas);

        Console.WriteLine($"Resultado esperado: {cantidadHilos * incrementosPorHilo}");
        Console.WriteLine($"Resultado obtenido: {contador}");
    }
}
```

---

## 6.3. Corrección usando `Interlocked`

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static int contador = 0;

    static void Main()
    {
        int cantidadHilos = 5;
        int incrementosPorHilo = 10000;

        Task[] tareas = new Task[cantidadHilos];

        for (int i = 0; i < cantidadHilos; i++)
        {
            tareas[i] = Task.Run(() =>
            {
                for (int j = 0; j < incrementosPorHilo; j++)
                {
                    Interlocked.Increment(ref contador);
                }
            });
        }

        Task.WaitAll(tareas);

        Console.WriteLine($"Resultado esperado: {cantidadHilos * incrementosPorHilo}");
        Console.WriteLine($"Resultado obtenido: {contador}");
    }
}
```

---

## Explicación en C#

En C#, el uso de `contador++` en varios hilos puede generar resultados incorrectos porque no es una operación atómica.

El bloque `lock` protege la sección crítica, permitiendo que solo una tarea acceda al contador por vez.

`Interlocked.Increment` es una alternativa más eficiente para operaciones simples, ya que realiza el incremento de manera atómica sin escribir manualmente una sección crítica con `lock`.

---

# 7. Comparación entre lenguajes

| Lenguaje | Problema sin sincronización | Mecanismo de solución | Alternativa atómica |
|---|---|---|---|
| Java | `contador++` no seguro | `synchronized` | `AtomicInteger` |
| Python | Variable compartida modificada por varios hilos | `threading.Lock` | No tan directo como Java/C# |
| C# | `contador++` no seguro | `lock` | `Interlocked.Increment` |

---

# 8. Ejercicio para resolver

## Consigna

Modificar los ejemplos anteriores para simular una cuenta bancaria compartida.

La cuenta inicia con un saldo de:

```text
100000
```

Deben crearse varios hilos que realicen operaciones de depósito y extracción.

## Requisitos

- Crear al menos 4 hilos.
- Dos hilos deben realizar depósitos.
- Dos hilos deben realizar extracciones.
- Cada hilo debe realizar 5000 operaciones.
- Primero implementar la versión sin sincronización.
- Luego implementar la versión corregida.
- Mostrar el saldo inicial, las operaciones realizadas y el saldo final.

---

# 9. Preguntas para responder

1. ¿Por qué el saldo final puede ser incorrecto en la versión sin sincronización?
2. ¿Cuál es la sección crítica del programa?
3. ¿Qué diferencia existe entre usar `lock`, `synchronized` y `Lock`?
4. ¿Cuándo conviene usar una operación atómica?
5. ¿Por qué una Race Condition puede ser difícil de detectar?

---

# 10. Conclusión esperada

La Race Condition demuestra que la programación con hilos no solo implica ejecutar tareas al mismo tiempo, sino también coordinar correctamente el acceso a los recursos compartidos.

Un programa concurrente puede parecer correcto, compilar sin errores y aun así producir resultados incorrectos si no se protege adecuadamente la sección crítica.

