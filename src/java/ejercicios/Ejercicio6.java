package ejercicios;

public class Ejercicio6 {
    private static final Object recurso = new Object();
    private static int turno = 0;

    public static void main(String[] args) throws InterruptedException {
        // Hilos agresivos: acceden constantemente
        for (int id = 1; id <= 3; id++) {
            int finalId = id;
            new Thread(() -> {
                for (int i = 0; i < 20; i++) {
                    synchronized (recurso) {
                        System.out.println("Hilo rapido " + finalId + " accedio (total: " + (i + 1) + ")");
                    }
                    Thread.yield();
                }
            }).start();
        }

        // Hilo lento: intenta acceder pero siempre queda postergado
        Thread hiloLento = new Thread(() -> {
            for (int i = 0; i < 5; i++) {
                synchronized (recurso) {
                    System.out.println(">>> Hilo LENTO accedio (vez " + (i + 1) + ")");
                }
                try { Thread.sleep(50); } catch (InterruptedException e) { }
            }
        });
        hiloLento.start();
        hiloLento.join();

        System.out.println("\nLos hilos rapidos acaparan el recurso y el lento");
        System.out.println("apenas logra acceder (starvation).");
    }
}

/*
Preguntas:
1. ¿Qué significa que un hilo sufra starvation?
   Significa que nunca (o muy raramente) obtiene acceso al recurso
   porque otros hilos lo acaparan constantemente.

2. ¿Es lo mismo que deadlock?
   No. En deadlock los hilos se bloquean mutuamente y ninguno avanza.
   En starvation hay hilos que avanzan mientras uno queda postergado
   indefinidamente, pero el sistema sigue funcionando.

3. ¿Cómo se puede reducir este problema?
   - Usar colas justas (fair queues) como ReentrantLock(true) en Java.
   - Aumentar la prioridad del hilo postergado.
   - Implementar envejecimiento (aging): aumentar prioridad cuanto
     mas tiempo espera el hilo.
*/
