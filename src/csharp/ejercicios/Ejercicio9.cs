using System;
using System.Threading;

class Ejercicio9
{
    public static void Run()
    {
        bool recursoOcupado = false;

        var hiloA = new Thread(() => {
            for (int i = 0; i < 10; i++)
            {
                if (recursoOcupado)
                {
                    Console.WriteLine("Hilo A: recurso ocupado, cedo el paso...");
                    Thread.Yield();
                }
                else
                {
                    recursoOcupado = true;
                    Console.WriteLine("Hilo A: tomo el recurso");
                    recursoOcupado = false;
                }
                Thread.Sleep(100);
            }
        });

        var hiloB = new Thread(() => {
            for (int i = 0; i < 10; i++)
            {
                if (recursoOcupado)
                {
                    Console.WriteLine("Hilo B: recurso ocupado, cedo el paso...");
                    Thread.Yield();
                }
                else
                {
                    recursoOcupado = true;
                    Console.WriteLine("Hilo B: tomo el recurso");
                    recursoOcupado = false;
                }
                Thread.Sleep(100);
            }
        });

        hiloA.Start();
        hiloB.Start();
        hiloA.Join();
        hiloB.Join();

        Console.WriteLine("\nAmbos activos, pero sin progreso real (livelock).");
    }
}

