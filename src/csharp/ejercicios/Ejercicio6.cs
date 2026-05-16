using System;
using System.Threading;

class Ejercicio6
{
    public static void Run()
    {
        var recurso = new object();

        for (int id = 1; id <= 3; id++)
        {
            int idCapturado = id;
            new Thread(() => {
                for (int i = 0; i < 20; i++)
                {
                    lock (recurso)
                        Console.WriteLine($"Hilo rapido {idCapturado} accedio (total: {i + 1})");
                    Thread.Yield();
                }
            }).Start();
        }

        var hiloLento = new Thread(() => {
            for (int i = 0; i < 5; i++)
            {
                lock (recurso)
                    Console.WriteLine($">>> Hilo LENTO accedio (vez {i + 1})");
                Thread.Sleep(50);
            }
        });

        hiloLento.Start();
        hiloLento.Join();

        Console.WriteLine("\nLos hilos rapidos acaparan el recurso.");
        Console.WriteLine("El lento apenas logra acceder (starvation).");
    }
}

/*
Preguntas:
1. ¿Que significa starvation? Un hilo nunca (o casi nunca) accede
   al recurso porque otros lo acaparan.
2. ¿Es lo mismo que deadlock? No. En deadlock nada avanza. En
   starvation algunos avanzan, uno queda postergado.
3. ¿Como reducirlo? Colas justas, envejecimiento (aging), prioridades.
*/
