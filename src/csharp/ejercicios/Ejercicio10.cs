using System;
using System.Collections.Generic;
using System.Threading;

class Ejercicio10
{
    public static void Run()
    {
        // 1. Mutex / Lock
        Console.WriteLine("=== 1. Mutex / Lock ===");
        var lockObj = new object();
        int contador = 0;
        var hilos = new Thread[5];
        for (int i = 0; i < 5; i++)
        {
            hilos[i] = new Thread(() => {
                for (int j = 0; j < 1000; j++)
                    lock (lockObj) { contador++; }
            });
            hilos[i].Start();
        }
        foreach (var h in hilos) h.Join();
        Console.WriteLine($"Contador con lock: {contador} (esperado: 5000)");

        // 2. Semaphore
        Console.WriteLine("\n=== 2. Semaphore (max 2 accesos) ===");
        var semaforo = new SemaphoreSlim(2, 2);
        for (int i = 0; i < 5; i++)
        {
            int id = i;
            new Thread(() => {
                semaforo.Wait();
                Console.WriteLine($"Hilo {id} accedio");
                Thread.Sleep(200);
                semaforo.Release();
            }).Start();
        }
        Thread.Sleep(600);

        // 3. Monitor / Condition
        Console.WriteLine("\n=== 3. Monitor / Condition ===");
        var cola = new Queue<int>();
        var monLock = new object();

        var prod = new Thread(() => {
            for (int i = 1; i <= 5; i++)
            {
                lock (monLock)
                {
                    cola.Enqueue(i);
                    Console.WriteLine($"Producido: {i}");
                    Monitor.Pulse(monLock);
                }
                Thread.Sleep(100);
            }
        });

        var cons = new Thread(() => {
            for (int i = 0; i < 5; i++)
            {
                lock (monLock)
                {
                    while (cola.Count == 0) Monitor.Wait(monLock);
                    Console.WriteLine($"Consumido: {cola.Dequeue()}");
                }
                Thread.Sleep(200);
            }
        });

        prod.Start();
        cons.Start();
        prod.Join();
        cons.Join();

        // 4. Atomic operations
        Console.WriteLine("\n=== 4. Atomic operations ===");
        contador = 0;
        for (int i = 0; i < 5; i++)
        {
            hilos[i] = new Thread(() => {
                for (int j = 0; j < 1000; j++)
                    Interlocked.Increment(ref contador);
            });
            hilos[i].Start();
        }
        foreach (var h in hilos) h.Join();
        Console.WriteLine($"Interlocked: {contador} (esperado: 5000)");

        // 5. Critical Section
        Console.WriteLine("\n=== 5. Critical Section ===");
        Console.WriteLine("Seccion critica: codigo dentro de lock { }");
        Console.WriteLine("Solo un hilo ejecuta esa seccion a la vez.");
    }
}
