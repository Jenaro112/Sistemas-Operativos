using System;
using System.Threading;

class Ejercicio5
{
    private static readonly object recursoA = new();
    private static readonly object recursoB = new();

    public static void Run()
    {
        Console.WriteLine("=== DEADLOCK ===");

        var hilo1 = new Thread(() => {
            lock (recursoA)
            {
                Console.WriteLine("Hilo 1: bloqueo RecursoA");
                Thread.Sleep(100);
                lock (recursoB)
                    Console.WriteLine("Hilo 1: bloqueo RecursoB");
            }
        });

        var hilo2 = new Thread(() => {
            lock (recursoB)
            {
                Console.WriteLine("Hilo 2: bloqueo RecursoB");
                Thread.Sleep(100);
                lock (recursoA)
                    Console.WriteLine("Hilo 2: bloqueo RecursoA");
            }
        });

        hilo1.Start();
        hilo2.Start();
        hilo1.Join(2000);
        hilo2.Join(2000);
        Console.WriteLine("(Si no termina, es deadlock)");

        Console.WriteLine("\n=== CORREGIDO (mismo orden) ===");

        var hilo1b = new Thread(() => {
            lock (recursoA)
            {
                Console.WriteLine("Hilo 1: bloqueo RecursoA");
                Thread.Sleep(100);
                lock (recursoB)
                    Console.WriteLine("Hilo 1: bloqueo RecursoB");
            }
        });

        var hilo2b = new Thread(() => {
            lock (recursoA)
            {
                Console.WriteLine("Hilo 2: bloqueo RecursoA");
                Thread.Sleep(100);
                lock (recursoB)
                    Console.WriteLine("Hilo 2: bloqueo RecursoB");
            }
        });

        hilo1b.Start();
        hilo2b.Start();
        hilo1b.Join();
        hilo2b.Join();
    }
}

