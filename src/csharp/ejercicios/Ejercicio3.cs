using System;
using System.Collections.Generic;
using System.Threading;

class Ejercicio3
{
    public static void Run()
    {
        const int Capacidad = 5;
        var cola = new Queue<int>();
        var lockObj = new object();

        var productor = new Thread(() => {
            for (int i = 1; i <= 20; i++)
            {
                lock (lockObj)
                {
                    while (cola.Count == Capacidad)
                        Monitor.Wait(lockObj);
                    cola.Enqueue(i);
                    Console.WriteLine($"Producido: {i}");
                    Monitor.PulseAll(lockObj);
                }
                Thread.Sleep(300);
            }
        });

        var consumidor = new Thread(() => {
            for (int i = 0; i < 20; i++)
            {
                lock (lockObj)
                {
                    while (cola.Count == 0)
                        Monitor.Wait(lockObj);
                    int valor = cola.Dequeue();
                    Console.WriteLine($"Consumido: {valor}");
                    Monitor.PulseAll(lockObj);
                }
                Thread.Sleep(500);
            }
        });

        productor.Start();
        consumidor.Start();

        productor.Join();
        consumidor.Join();
    }
}

/*
Preguntas:
1. ¿Por que se necesita sincronizacion? Para evitar condiciones de carrera.
2. ¿Que error aparece si acceden libremente? Data race: datos corruptos,
   excepciones por cola vacia, elementos perdidos.
3. ¿Ventajas de cola thread-safe? Codigo mas limpio, menos errores
   (deadlocks, race conditions), sin gestion manual de locks.
*/
