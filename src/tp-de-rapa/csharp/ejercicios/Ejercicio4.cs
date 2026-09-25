using System;
using System.Threading;

class Ejercicio4
{
    public static void Run()
    {
        int contadorInseguro = 0;
        int contadorSincronizado = 0;
        var lockObj = new object();

        // SIN sincronizacion
        var hilos = new Thread[5];
        for (int i = 0; i < 5; i++)
        {
            hilos[i] = new Thread(() => {
                for (int j = 0; j < 10000; j++)
                    contadorInseguro++;
            });
            hilos[i].Start();
        }
        foreach (var h in hilos) h.Join();
        Console.WriteLine($"Esperado: 50000 | Sin control: {contadorInseguro}");

        // CON lock
        for (int i = 0; i < 5; i++)
        {
            hilos[i] = new Thread(() => {
                for (int j = 0; j < 10000; j++)
                    lock (lockObj) { contadorSincronizado++; }
            });
            hilos[i].Start();
        }
        foreach (var h in hilos) h.Join();
        Console.WriteLine($"Esperado: 50000 | Con lock: {contadorSincronizado}");

        // CON Interlocked
        int contadorInterlocked = 0;
        for (int i = 0; i < 5; i++)
        {
            hilos[i] = new Thread(() => {
                for (int j = 0; j < 10000; j++)
                    Interlocked.Increment(ref contadorInterlocked);
            });
            hilos[i].Start();
        }
        foreach (var h in hilos) h.Join();
        Console.WriteLine($"Esperado: 50000 | Con Interlocked: {contadorInterlocked}");
    }
}

