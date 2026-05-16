package ejercicios;

public class Ejercicio9 {
    private static boolean recursoOcupado = false;

    public static void main(String[] args) throws InterruptedException {
        Thread hiloA = new Thread(() -> {
            for (int i = 0; i < 10; i++) {
                if (recursoOcupado) {
                    System.out.println("Hilo A: recurso ocupado, cedo el paso...");
                    Thread.yield();
                } else {
                    recursoOcupado = true;
                    System.out.println("Hilo A: tomo el recurso");
                    recursoOcupado = false;
                }
                pausa(100);
            }
        });

        Thread hiloB = new Thread(() -> {
            for (int i = 0; i < 10; i++) {
                if (recursoOcupado) {
                    System.out.println("Hilo B: recurso ocupado, cedo el paso...");
                    Thread.yield();
                } else {
                    recursoOcupado = true;
                    System.out.println("Hilo B: tomo el recurso");
                    recursoOcupado = false;
                }
                pausa(100);
            }
        });

        hiloA.start();
        hiloB.start();
        hiloA.join();
        hiloB.join();

        System.out.println("\nAmbos hilos estuvieron activos todo el tiempo");
        System.out.println("pero rara vez lograron tomar el recurso (livelock).");
    }

    private static void pausa(int ms) {
        try { Thread.sleep(ms); } catch (InterruptedException e) {
            Thread.currentThread().interrupt();
        }
    }
}

/*
Preguntas:
1. ¿Por que livelock no es igual a deadlock?
   En deadlock los hilos estan bloqueados esperando. En livelock los
   hilos estan activos, ejecutando codigo, pero sin progreso real.

2. ¿Los hilos estan detenidos?
   No, los hilos estan activos y ejecutandose continuamente.
   Solo que ninguno logra completar su tarea.

3. ¿Como se podria resolver?
   - Introducir backoff aleatorio: cada hilo espera un tiempo
     aleatorio antes de reintentar, reduciendo colisiones.
   - Usar un mecanismo de coordinacion como un coordinator
     que asigna turnos de forma explcita.
*/
