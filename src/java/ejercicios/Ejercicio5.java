package ejercicios;

public class Ejercicio5 {
    private static final Object recursoA = new Object();
    private static final Object recursoB = new Object();

    public static void main(String[] args) throws InterruptedException {
        System.out.println("=== DEADLOCK ===");
        Thread hilo1 = new Thread(() -> {
            synchronized (recursoA) {
                System.out.println("Hilo 1: bloqueo RecursoA");
                pausa(100);
                synchronized (recursoB) {
                    System.out.println("Hilo 1: bloqueo RecursoB");
                }
            }
        });

        Thread hilo2 = new Thread(() -> {
            synchronized (recursoB) {
                System.out.println("Hilo 2: bloqueo RecursoB");
                pausa(100);
                synchronized (recursoA) {
                    System.out.println("Hilo 2: bloqueo RecursoA");
                }
            }
        });

        hilo1.start();
        hilo2.start();
        hilo1.join(2000);
        hilo2.join(2000);
        System.out.println("(Si no termina, es deadlock)");

        System.out.println("\n=== CORREGIDO (mismo orden) ===");
        Thread hilo1b = new Thread(() -> {
            synchronized (recursoA) {
                System.out.println("Hilo 1: bloqueo RecursoA");
                pausa(100);
                synchronized (recursoB) {
                    System.out.println("Hilo 1: bloqueo RecursoB");
                }
            }
        });

        Thread hilo2b = new Thread(() -> {
            synchronized (recursoA) {
                System.out.println("Hilo 2: bloqueo RecursoA");
                pausa(100);
                synchronized (recursoB) {
                    System.out.println("Hilo 2: bloqueo RecursoB");
                }
            }
        });

        hilo1b.start();
        hilo2b.start();
    }

    private static void pausa(int ms) {
        try { Thread.sleep(ms); } catch (InterruptedException e) {
            Thread.currentThread().interrupt();
        }
    }
}

/*
Preguntas:
1. ¿Por qué se produce el deadlock?
   Porque cada hilo tiene un recurso que el otro necesita y ninguno
   libera el suyo. Quedan esperandose mutuamente para siempre.

2. ¿Qué condiciones deben cumplirse para que exista deadlock?
   Las 4 condiciones de Coffman:
   - Exclusion mutua: los recursos no son compartibles.
   - Retencion y espera: un hilo retiene un recurso mientras espera otro.
   - No desalojo: un recurso no puede ser quitado forzosamente.
   - Espera circular: existe un ciclo de hilos esperando recursos.

3. ¿Cómo se puede prevenir?
   - Bloquear recursos siempre en el mismo orden (rompe espera circular).
   - Usar timeout con tryLock (rompe retencion y espera).
   - Minimizar el tiempo dentro de secciones criticas.
*/
