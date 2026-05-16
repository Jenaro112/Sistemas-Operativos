# Práctica: Hilos Simultáneos en Java, Python y C#

## Objetivo de la práctica

Comprender el uso básico de **threads** o **hilos de ejecución** mediante ejemplos prácticos en **Java**, **Python** y **C#**, observando cómo dos tareas pueden ejecutarse de forma concurrente dentro de un mismo programa.

La práctica permite analizar cómo se crean hilos, cómo se inician, cómo se ejecutan tareas simultáneas y cómo el sistema operativo alterna la ejecución de cada hilo.

---

# 1. Concepto general: Hilos simultáneos

Un **hilo** es una unidad de ejecución dentro de un programa. Un programa puede tener uno o varios hilos ejecutándose al mismo tiempo.

Cuando se utilizan hilos simultáneos, dos o más tareas pueden avanzar de manera concurrente. Esto no significa necesariamente que siempre se ejecuten exactamente en paralelo, ya que eso depende de la cantidad de núcleos del procesador y de la planificación del sistema operativo.

Por ejemplo, un programa puede ejecutar al mismo tiempo:

- Una tarea que imprime números.
- Otra tarea que imprime letras.

El resultado puede aparecer intercalado, porque el sistema operativo decide qué hilo se ejecuta en cada momento.

---

# 2. Ejemplo en Java

## Librerías utilizadas

```java
java.lang.Thread
```

En Java, la clase `Thread` forma parte del paquete `java.lang`, por lo tanto no es necesario importarla explícitamente.

## Código de ejemplo

```java
public class HilosSimultaneosJava {

    public static void main(String[] args) {

        Thread hiloNumeros = new Thread(() -> {
            for (int i = 1; i <= 10; i++) {
                System.out.println("Números: " + i);
                try {
                    Thread.sleep(500);
                } catch (InterruptedException e) {
                    e.printStackTrace();
                }
            }
        });

        Thread hiloLetras = new Thread(() -> {
            for (char letra = 'A'; letra <= 'J'; letra++) {
                System.out.println("Letras: " + letra);
                try {
                    Thread.sleep(500);
                } catch (InterruptedException e) {
                    e.printStackTrace();
                }
            }
        });

        hiloNumeros.start();
        hiloLetras.start();
    }
}
```

## Explicación del funcionamiento

En este ejemplo se crean dos objetos de tipo `Thread`.

El primer hilo ejecuta una tarea que imprime números del 1 al 10. El segundo hilo imprime letras de la A a la J.

El método `start()` inicia la ejecución concurrente de cada hilo. Es importante destacar que no se llama directamente al método `run()`, porque eso ejecutaría el código de forma secuencial en el hilo principal.

La instrucción `Thread.sleep(500)` pausa temporalmente cada hilo durante 500 milisegundos. Esto permite observar mejor cómo las salidas se intercalan en consola.

---

# 3. Ejemplo en Python

## Librerías utilizadas

```python
threading
time
```

- `threading`: permite crear y administrar hilos.
- `time`: permite pausar la ejecución usando `sleep()`.

## Código de ejemplo

```python
import threading
import time


def imprimir_numeros():
    for i in range(1, 11):
        print(f"Números: {i}")
        time.sleep(0.5)


def imprimir_letras():
    for letra in "ABCDEFGHIJ":
        print(f"Letras: {letra}")
        time.sleep(0.5)


hilo_numeros = threading.Thread(target=imprimir_numeros)
hilo_letras = threading.Thread(target=imprimir_letras)

hilo_numeros.start()
hilo_letras.start()

hilo_numeros.join()
hilo_letras.join()

print("Finalizó la ejecución de ambos hilos.")
```

## Explicación del funcionamiento

En Python se utiliza el módulo `threading` para crear hilos.

Cada hilo recibe una función mediante el parámetro `target`. En este caso, un hilo ejecuta la función `imprimir_numeros()` y el otro ejecuta `imprimir_letras()`.

El método `start()` inicia cada hilo. Luego, `join()` hace que el programa principal espere a que ambos hilos terminen antes de mostrar el mensaje final.

La función `time.sleep(0.5)` genera una pausa de medio segundo, permitiendo visualizar la ejecución alternada de los hilos.

En Python existe el GIL, conocido como **Global Interpreter Lock**, que limita la ejecución paralela real de código Python puro en CPU. Sin embargo, los hilos son muy útiles para tareas de entrada/salida, como lectura de archivos, comunicación por red o espera de respuestas externas.

---

# 4. Ejemplo en C#

## Librerías utilizadas

```csharp
System
System.Threading
```

- `System`: permite usar funcionalidades básicas como `Console`.
- `System.Threading`: permite crear y administrar hilos mediante la clase `Thread`.

## Código de ejemplo

```csharp
using System;
using System.Threading;

class Program
{
    static void Main(string[] args)
    {
        Thread hiloNumeros = new Thread(ImprimirNumeros);
        Thread hiloLetras = new Thread(ImprimirLetras);

        hiloNumeros.Start();
        hiloLetras.Start();

        hiloNumeros.Join();
        hiloLetras.Join();

        Console.WriteLine("Finalizó la ejecución de ambos hilos.");
    }

    static void ImprimirNumeros()
    {
        for (int i = 1; i <= 10; i++)
        {
            Console.WriteLine($"Números: {i}");
            Thread.Sleep(500);
        }
    }

    static void ImprimirLetras()
    {
        for (char letra = 'A'; letra <= 'J'; letra++)
        {
            Console.WriteLine($"Letras: {letra}");
            Thread.Sleep(500);
        }
    }
}
```

## Explicación del funcionamiento

En C# se utiliza la clase `Thread`, incluida en el espacio de nombres `System.Threading`.

Se crean dos hilos: uno asociado al método `ImprimirNumeros()` y otro asociado al método `ImprimirLetras()`.

El método `Start()` inicia cada hilo. Ambos comienzan su ejecución de manera concurrente.

El método `Join()` indica que el hilo principal debe esperar a que los hilos secundarios finalicen antes de continuar.

La instrucción `Thread.Sleep(500)` pausa el hilo actual durante 500 milisegundos, permitiendo observar la alternancia entre las tareas.

---

# 5. Comparación entre Java, Python y C#

| Aspecto | Java | Python | C# |
|---|---|---|---|
| Clase o módulo principal | `Thread` | `threading.Thread` | `Thread` |
| Pausa de ejecución | `Thread.sleep()` | `time.sleep()` | `Thread.Sleep()` |
| Inicio del hilo | `start()` | `start()` | `Start()` |
| Espera de finalización | `join()` | `join()` | `Join()` |
| Uso recomendado | Concurrencia general | Tareas I/O-bound | Concurrencia y tareas de sistema |

---

# 6. Observaciones importantes

- El orden de salida en consola puede cambiar en cada ejecución.
- El sistema operativo decide cuándo se ejecuta cada hilo.
- No se debe asumir que un hilo siempre terminará antes que otro.
- Los hilos permiten ejecutar tareas concurrentes, pero también introducen complejidad.
- Cuando varios hilos acceden a recursos compartidos, se necesita sincronización.

---

# 7. Ejercicio para resolver

## Consigna

Modificar los ejemplos de Java, Python y C# para que el programa ejecute tres hilos simultáneos:

1. Un hilo debe imprimir números del 1 al 10.
2. Un hilo debe imprimir letras de la A a la J.
3. Un hilo debe imprimir los nombres de 10 estudiantes.

Cada hilo debe realizar una pausa de 300 milisegundos entre cada impresión.

Al finalizar, el programa debe mostrar el mensaje:

```text
Todas las tareas finalizaron correctamente.
```

## Requisitos

- Resolver el ejercicio en Java, Python y C#.
- Utilizar hilos reales en cada lenguaje.
- Usar `sleep` para simular trabajo.
- Usar `join` para esperar la finalización de todos los hilos.
- Comparar si el orden de salida es igual o diferente en cada ejecución.

---

# 8. Preguntas para responder

1. ¿El orden de impresión fue siempre el mismo?
2. ¿Qué función cumple `start()`?
3. ¿Qué diferencia existe entre llamar a `start()` y llamar directamente a una función o método?
4. ¿Para qué se utiliza `join()`?
5. ¿Qué sucede si se elimina `sleep()`?
6. ¿Qué ventajas tiene ejecutar tareas mediante hilos?
7. ¿Qué riesgos aparecen cuando los hilos comparten datos?

---

# 9. Conclusión

Los hilos permiten que un programa ejecute múltiples tareas de manera concurrente. En Java, Python y C# el concepto general es similar, aunque cambia la sintaxis y las librerías utilizadas.

Esta práctica permite comprender la base de la programación concurrente antes de avanzar hacia problemas más complejos, como condiciones de carrera, bloqueos, sincronización y coordinación entre hilos.
