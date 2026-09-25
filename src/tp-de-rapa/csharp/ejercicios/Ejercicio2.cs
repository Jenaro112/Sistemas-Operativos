using System;
using System.Threading;

class Ejercicio2
{
    public static void Run()
    {
        var descarga = new Thread(() => {
            for (int i = 10; i <= 100; i += 10)
            {
                Console.WriteLine($"Descargando... {i}%");
                Thread.Sleep(500);
            }
            Console.WriteLine("Descarga completa.");
        });

        var interfaz = new Thread(() => {
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine("Interfaz activa...");
                Thread.Sleep(500);
            }
        });

        descarga.Start();
        interfaz.Start();

        descarga.Join();
        interfaz.Join();
    }
}

