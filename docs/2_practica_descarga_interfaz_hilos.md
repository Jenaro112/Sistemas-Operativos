# Práctica: Descarga + Interfaz Simulada con Hilos

## Objetivo

Comprender cómo el uso de **hilos de ejecución** permite ejecutar una tarea lenta o pesada en segundo plano, mientras otra parte del programa continúa respondiendo. Este caso simula una situación típica de una aplicación con interfaz de usuario: una descarga avanza mientras la interfaz sigue activa.

---

# 1. Descripción del problema

En muchas aplicaciones, algunas operaciones pueden tardar varios segundos, por ejemplo:

- Descargar un archivo.
- Consultar una API.
- Procesar datos.
- Leer archivos grandes.
- Ejecutar una operación de base de datos.

Si estas tareas se ejecutan en el hilo principal, la aplicación puede quedar bloqueada. Para evitarlo, se utiliza un hilo secundario que realiza la operación lenta, mientras el hilo principal continúa atendiendo la interfaz.

En esta práctica se simulará:

- Un hilo que representa una descarga de archivo.
- Otro hilo que representa una interfaz activa.

---

# 2. Ejemplo en Java

## Librerías utilizadas

```java
java.lang.Thread
```

La clase `Thread` pertenece a `java.lang`, por lo tanto no requiere una importación explícita.

## Código

```java
public class DescargaInterfaz {

    public static void main(String[] args) {

        Thread descarga = new Thread(() -> {
            for (int i = 10; i <= 100; i += 10) {
                System.out.println("Descargando archivo... " + i + "%");
                try {
                    Thread.sleep(1000);
                } catch (InterruptedException e) {
                    System.out.println("La descarga fue interrumpida.");
                }
            }
            System.out.println("Descarga finalizada.");
        });

        Thread interfaz = new Thread(() -> {
            for (int i = 1; i <= 10; i++) {
                System.out.println("Interfaz activa... el usuario puede seguir usando la aplicación.");
                try {
                    Thread.sleep(500);
                } catch (InterruptedException e) {
                    System.out.println("La interfaz fue interrumpida.");
                }
            }
        });

        descarga.start();
        interfaz.start();
    }
}
```

## Explicación

En este ejemplo se crean dos objetos `Thread`. El primer hilo simula una descarga mostrando el progreso cada un segundo. El segundo hilo representa la interfaz de usuario, mostrando mensajes cada medio segundo.

Ambos hilos se inician con el método `start()`. Esto permite que las dos tareas avancen de manera concurrente. Mientras la descarga progresa lentamente, la interfaz sigue mostrando mensajes, demostrando que la aplicación no queda bloqueada.

---

# 3. Ejemplo en Python

## Librerías utilizadas

```python
import threading
import time
```

- `threading`: permite crear y administrar hilos.
- `time`: permite simular esperas usando `sleep()`.

## Código

```python
import threading
import time


def descargar_archivo():
    for progreso in range(10, 101, 10):
        print(f"Descargando archivo... {progreso}%")
        time.sleep(1)
    print("Descarga finalizada.")


def interfaz_activa():
    for i in range(10):
        print("Interfaz activa... el usuario puede seguir usando la aplicación.")
        time.sleep(0.5)


hilo_descarga = threading.Thread(target=descargar_archivo)
hilo_interfaz = threading.Thread(target=interfaz_activa)

hilo_descarga.start()
hilo_interfaz.start()

hilo_descarga.join()
hilo_interfaz.join()

print("Programa finalizado.")
```

## Explicación

En Python se utiliza la librería `threading` para crear dos hilos. Cada hilo recibe una función mediante el parámetro `target`.

El hilo `hilo_descarga` ejecuta la función `descargar_archivo()`, que simula el avance de una descarga. El hilo `hilo_interfaz` ejecuta la función `interfaz_activa()`, que representa una interfaz que continúa respondiendo.

El método `start()` inicia cada hilo. Luego, `join()` hace que el programa principal espere a que ambos hilos terminen antes de finalizar.

Este tipo de ejemplo es especialmente útil para tareas de entrada/salida, como descargas, consultas web o lectura de archivos.

---

# 4. Ejemplo en C#

## Librerías utilizadas

```csharp
using System;
using System.Threading;
```

- `System`: permite usar funciones básicas como `Console.WriteLine()`.
- `System.Threading`: permite crear y manejar hilos mediante la clase `Thread`.

## Código

```csharp
using System;
using System.Threading;

class Program
{
    static void DescargarArchivo()
    {
        for (int i = 10; i <= 100; i += 10)
        {
            Console.WriteLine($"Descargando archivo... {i}%");
            Thread.Sleep(1000);
        }

        Console.WriteLine("Descarga finalizada.");
    }

    static void InterfazActiva()
    {
        for (int i = 1; i <= 10; i++)
        {
            Console.WriteLine("Interfaz activa... el usuario puede seguir usando la aplicación.");
            Thread.Sleep(500);
        }
    }

    static void Main(string[] args)
    {
        Thread hiloDescarga = new Thread(DescargarArchivo);
        Thread hiloInterfaz = new Thread(InterfazActiva);

        hiloDescarga.Start();
        hiloInterfaz.Start();

        hiloDescarga.Join();
        hiloInterfaz.Join();

        Console.WriteLine("Programa finalizado.");
    }
}
```

## Explicación

En C# se utiliza la clase `Thread`, disponible en el espacio de nombres `System.Threading`. Se definen dos métodos: uno para simular la descarga y otro para simular la interfaz activa.

Cada método se ejecuta en un hilo diferente. El método `Start()` inicia los hilos y `Join()` permite esperar a que terminen antes de cerrar el programa.

El resultado muestra que la descarga y la interfaz se ejecutan de forma concurrente. Esto representa el comportamiento esperado de una aplicación que no se bloquea mientras realiza una operación lenta.

---

# 5. Comparación entre los tres lenguajes

| Lenguaje | Clase o módulo principal | Método para iniciar | Método para esperar |
|---|---|---|---|
| Java | `Thread` | `start()` | `join()` |
| Python | `threading.Thread` | `start()` | `join()` |
| C# | `Thread` | `Start()` | `Join()` |

En los tres casos, la idea principal es la misma: separar una tarea lenta del flujo principal del programa para permitir que otras acciones continúen ejecutándose.

---

# 6. Ejercicio para resolver

## Consigna

Modificar los ejemplos anteriores para simular una aplicación con tres tareas concurrentes:

1. Una descarga de archivo que avance de 5% en 5% hasta llegar al 100%.
2. Una interfaz activa que muestre cada cierto tiempo el mensaje: `Interfaz respondiendo...`.
3. Un proceso de guardado automático que muestre el mensaje: `Guardando progreso...` cada 2 segundos.

## Requisitos

- Resolver el ejercicio en Java, Python y C#.
- Usar hilos para ejecutar las tres tareas al mismo tiempo.
- Mostrar mensajes claros en consola.
- Hacer que el programa principal espere la finalización de todos los hilos.

## Preguntas para responder

1. ¿Qué sucede con la interfaz mientras se ejecuta la descarga?
2. ¿Qué tarea termina primero?
3. ¿El orden de los mensajes es siempre igual?
4. ¿Qué pasaría si la descarga se ejecutara sin usar hilos?
5. ¿Este ejemplo representa mejor un problema CPU-bound o I/O-bound?

---

# 7. Conclusión

El uso de hilos permite que una aplicación continúe respondiendo mientras ejecuta tareas lentas en segundo plano. Este concepto es fundamental en aplicaciones modernas, especialmente en sistemas con interfaces gráficas, servicios web, descargas de archivos, procesamiento de datos y operaciones de entrada/salida.

La idea central es evitar que una operación prolongada bloquee completamente el programa.
