# Práctica de Hilos: Problema de Livelock

## 1. Objetivo de la práctica

Comprender el problema de **Livelock** en programación concurrente mediante ejemplos prácticos en **Java**, **Python** y **C#**, observando cómo dos o más hilos pueden permanecer activos, ejecutando instrucciones continuamente, pero sin lograr avanzar realmente en la tarea.

---

## 2. Concepto: ¿Qué es Livelock?

Un **Livelock** ocurre cuando dos o más hilos no están bloqueados, pero sus acciones provocan que ninguno pueda progresar.

A diferencia del **Deadlock**, donde los hilos quedan detenidos esperando recursos, en el **Livelock** los hilos siguen ejecutándose, reaccionando entre sí, pero sin completar su trabajo.

### Ejemplo cotidiano

Dos personas se encuentran de frente en un pasillo:

- La persona A se mueve hacia la derecha para dejar pasar.
- La persona B también se mueve hacia la derecha.
- Ambas vuelven a intentar corregirse.
- Siguen moviéndose, pero ninguna logra pasar.

En programación, esto puede ocurrir cuando los hilos intentan evitar conflictos de manera excesivamente cooperativa.

---

# 3. Ejemplo en Java

## Librerías utilizadas

```java
java.lang.Thread
```

## Código

```java
public class LivelockExample {

    static class Worker {
        private String name;
        private boolean active = true;

        public Worker(String name) {
            this.name = name;
        }

        public boolean isActive() {
            return active;
        }

        public void work(Worker otherWorker) {
            while (active) {
                if (otherWorker.isActive()) {
                    System.out.println(name + " ve que " + otherWorker.name + " está activo y le cede el paso.");
                    try {
                        Thread.sleep(500);
                    } catch (InterruptedException e) {
                        e.printStackTrace();
                    }
                    continue;
                }

                System.out.println(name + " realiza la tarea.");
                active = false;
            }
        }
    }

    public static void main(String[] args) {
        Worker worker1 = new Worker("Hilo 1");
        Worker worker2 = new Worker("Hilo 2");

        Thread t1 = new Thread(() -> worker1.work(worker2));
        Thread t2 = new Thread(() -> worker2.work(worker1));

        t1.start();
        t2.start();
    }
}
```

## Explicación

En este ejemplo, cada hilo verifica si el otro sigue activo.  
Si el otro hilo está activo, decide cederle el paso y vuelve a intentar más tarde.

El problema es que ambos hilos toman la misma decisión continuamente:

- Hilo 1 cede al Hilo 2.
- Hilo 2 cede al Hilo 1.
- Ambos siguen ejecutándose.
- Ninguno completa la tarea.

Este comportamiento representa un **Livelock**, porque los hilos no están detenidos, pero tampoco avanzan.

---

# 4. Ejemplo en Python

## Librerías utilizadas

```python
threading
time
```

## Código

```python
import threading
import time

class Worker:
    def __init__(self, name):
        self.name = name
        self.active = True

    def is_active(self):
        return self.active

    def work(self, other_worker):
        while self.active:
            if other_worker.is_active():
                print(f"{self.name} ve que {other_worker.name} está activo y le cede el paso.")
                time.sleep(0.5)
                continue

            print(f"{self.name} realiza la tarea.")
            self.active = False

worker1 = Worker("Hilo 1")
worker2 = Worker("Hilo 2")

t1 = threading.Thread(target=worker1.work, args=(worker2,))
t2 = threading.Thread(target=worker2.work, args=(worker1,))

t1.start()
t2.start()
```

## Explicación

El programa crea dos hilos que intentan trabajar, pero antes verifican si el otro hilo está activo.

Como ambos hilos encuentran activo al otro, ambos ceden el paso repetidamente.  
Esto produce una situación donde:

- El programa no se detiene.
- Los hilos siguen ejecutándose.
- No se completa la tarea principal.

Este caso es útil para diferenciar claramente **Livelock** de **Deadlock**.

---

# 5. Ejemplo en C#

## Librerías utilizadas

```csharp
System
System.Threading
```

## Código

```csharp
using System;
using System.Threading;

class Worker
{
    public string Name { get; set; }
    public bool Active { get; set; } = true;

    public Worker(string name)
    {
        Name = name;
    }

    public void Work(Worker otherWorker)
    {
        while (Active)
        {
            if (otherWorker.Active)
            {
                Console.WriteLine($"{Name} ve que {otherWorker.Name} está activo y le cede el paso.");
                Thread.Sleep(500);
                continue;
            }

            Console.WriteLine($"{Name} realiza la tarea.");
            Active = false;
        }
    }
}

class Program
{
    static void Main()
    {
        Worker worker1 = new Worker("Hilo 1");
        Worker worker2 = new Worker("Hilo 2");

        Thread t1 = new Thread(() => worker1.Work(worker2));
        Thread t2 = new Thread(() => worker2.Work(worker1));

        t1.Start();
        t2.Start();
    }
}
```

## Explicación

En C#, cada hilo ejecuta el método `Work`.  
Antes de realizar la tarea, cada hilo consulta si el otro sigue activo.

Como ambos hilos están activos al mismo tiempo, cada uno decide ceder el paso al otro.  
El resultado es que los dos hilos siguen corriendo, pero ninguno llega a completar su tarea.

Este ejemplo muestra que la cooperación entre hilos también debe estar correctamente diseñada.

---

# 6. Comparación entre Java, Python y C#

| Lenguaje | Clase o mecanismo principal | Pausa | Observación |
|---|---|---|---|
| Java | `Thread` | `Thread.sleep()` | Usa objetos y métodos para representar los trabajadores |
| Python | `threading.Thread` | `time.sleep()` | Sintaxis simple para observar el comportamiento concurrente |
| C# | `Thread` | `Thread.Sleep()` | Usa clases y propiedades para modelar el estado de cada hilo |

---

# 7. Diferencia entre Deadlock y Livelock

| Concepto | Deadlock | Livelock |
|---|---|---|
| Estado de los hilos | Bloqueados | Activos |
| Hay ejecución | No | Sí |
| Hay progreso real | No | No |
| Causa típica | Espera circular por recursos | Reacción continua entre hilos |
| Ejemplo | Dos hilos esperan locks cruzados | Dos hilos se ceden el paso infinitamente |

---

# 8. ¿Cómo evitar un Livelock?

Algunas estrategias posibles son:

- Agregar prioridades entre hilos.
- Usar tiempos de espera aleatorios.
- Limitar la cantidad de reintentos.
- Definir una regla clara para decidir qué hilo avanza primero.
- Evitar que todos los hilos reaccionen de la misma manera.
- Usar mecanismos de sincronización correctamente diseñados.

---

# 9. Ejercicio para resolver

## Consigna

Modificar los ejemplos anteriores para evitar el **Livelock**.

El programa debe permitir que uno de los dos hilos finalmente realice la tarea.

## Requisitos

Implementar alguna de estas estrategias:

1. Asignar prioridad a uno de los hilos.
2. Agregar un contador máximo de intentos.
3. Usar una pausa aleatoria antes de volver a intentar.
4. Permitir que un hilo avance si el otro ya cedió varias veces.

## Resultado esperado

El programa debe mostrar que:

- Ambos hilos comienzan activos.
- Inicialmente pueden cederse el paso.
- Luego uno de los hilos logra ejecutar la tarea.
- El programa finaliza correctamente.

---

# 10. Preguntas para responder

1. ¿Por qué este problema no es un Deadlock?
2. ¿Qué significa que los hilos estén activos pero no progresen?
3. ¿Qué solución implementó para evitar el Livelock?
4. ¿Qué pasaría si se agregara una espera aleatoria?
5. ¿En qué tipo de sistemas reales podría aparecer este problema?

---

# 11. Conclusión

El **Livelock** es un problema de concurrencia donde los hilos no están bloqueados, pero tampoco logran avanzar.  
Aparece cuando varios hilos reaccionan continuamente entre sí de forma incorrecta.

Comprender este problema permite diseñar aplicaciones concurrentes más robustas, evitando comportamientos donde el sistema parece estar funcionando, pero no produce resultados útiles.
