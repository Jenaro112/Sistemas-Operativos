using System;
using System.Threading;

class Ejercicio1
{
    public static void Run()
    {
        var hiloNumeros = new Thread(() => {
            for (int i = 1; i <= 10; i++)
            {
                Console.WriteLine($"Numero: {i}");
                Thread.Sleep(200);
            }
        });

        var hiloLetras = new Thread(() => {
            for (char c = 'A'; c <= 'J'; c++)
            {
                Console.WriteLine($"Letra: {c}");
                Thread.Sleep(200);
            }
        });

        hiloNumeros.Start();
        hiloLetras.Start();

        hiloNumeros.Join();
        hiloLetras.Join();
    }
}

