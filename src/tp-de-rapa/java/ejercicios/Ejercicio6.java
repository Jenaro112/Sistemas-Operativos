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

