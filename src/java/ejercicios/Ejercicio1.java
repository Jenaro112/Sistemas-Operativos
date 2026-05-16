package ejercicios;

public class Ejercicio1 {
    public static void main(String[] args) {
        Thread hiloNumeros = new Thread(() -> {
            for (int i = 1; i <= 10; i++) {
                System.out.println("Numero: " + i);
                pausa();
            }
        });

        Thread hiloLetras = new Thread(() -> {
            for (char c = 'A'; c <= 'J'; c++) {
                System.out.println("Letra: " + c);
                pausa();
            }
        });

        hiloNumeros.start();
        hiloLetras.start();
    }

    private static void pausa() {
        try {
            Thread.sleep(200);
        } catch (InterruptedException e) {
            Thread.currentThread().interrupt();
        }
    }
}

/**
Preguntas:
1. ¿El orden de ejecución es siempre el mismo?
   No, el orden varía cada vez que se ejecuta. Depende del scheduler del SO.

2. ¿Quién decide qué hilo se ejecuta primero?
   El scheduler (planificador) del sistema operativo decide qué hilo
   se ejecuta y por cuánto tiempo.

3. ¿Qué sucede si se eliminan las pausas?
   Un hilo tiende a ejecutarse completamente antes de que el otro
   comience, porque el cambio de contexto ocurre menos frecuentemente.
   Se pierde la intercalación visible entre hilos.
*/
