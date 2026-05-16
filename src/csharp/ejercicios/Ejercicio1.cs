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

/*
Preguntas:
1. ¿El orden de ejecucion es siempre el mismo? No, varia cada vez.
   Depende del scheduler del SO.
2. ¿Quien decide que hilo se ejecuta primero? El scheduler del SO.
3. ¿Que sucede si se eliminan las pausas? Un hilo tiende a ejecutarse
   completamente antes del otro. Se pierde la intercalacion visible.
*/
