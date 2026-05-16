package ejercicios;

import java.util.LinkedList;
import java.util.Queue;
import java.util.concurrent.Semaphore;
import java.util.concurrent.atomic.AtomicInteger;

public class Ejercicio10 {
    public static void main(String[] args) throws InterruptedException {
        // 1. Mutex / Lock - proteger variable compartida
        System.out.println("=== 1. Mutex / Lock ===");
        Object lock = new Object();
        int[] contador = {0};
        Thread[] hilos = new Thread[5];
        for (int i = 0; i < 5; i++) {
            hilos[i] = new Thread(() -> {
                for (int j = 0; j < 1000; j++) {
                    synchronized (lock) { contador[0]++; }
                }
            });
            hilos[i].start();
        }
        for (Thread h : hilos) h.join();
        System.out.println("Contador con lock: " + contador[0] + " (esperado: 5000)");

        // 2. Semaphore - solo 2 hilos a la vez
        System.out.println("\n=== 2. Semaphore (max 2 accesos simultaneos) ===");
        Semaphore semaforo = new Semaphore(2);
        for (int i = 0; i < 5; i++) {
            int id = i;
            new Thread(() -> {
                try {
                    semaforo.acquire();
                    System.out.println("Hilo " + id + " accedio al recurso");
                    Thread.sleep(200);
                    semaforo.release();
                } catch (InterruptedException e) { }
            }).start();
        }
        pausa(600);

        // 3. Monitor / Condition - productor-consumidor
        System.out.println("\n=== 3. Monitor / Condition ===");
        Queue<Integer> cola = new LinkedList<>();
        Object monLock = new Object();
        Thread productor = new Thread(() -> {
            for (int i = 1; i <= 5; i++) {
                synchronized (monLock) {
                    cola.add(i);
                    System.out.println("Producido: " + i);
                    monLock.notify();
                }
                pausa(100);
            }
        });
        Thread consumidor = new Thread(() -> {
            for (int i = 0; i < 5; i++) {
                synchronized (monLock) {
                    while (cola.isEmpty()) {
                        try { monLock.wait(); } catch (InterruptedException e) { }
                    }
                    System.out.println("Consumido: " + cola.poll());
                }
                pausa(200);
            }
        });
        productor.start();
        consumidor.start();
        productor.join();
        consumidor.join();

        // 4. Atomic operations
        System.out.println("\n=== 4. Atomic operations ===");
        AtomicInteger atomic = new AtomicInteger(0);
        for (int i = 0; i < 5; i++) {
            hilos[i] = new Thread(() -> {
                for (int j = 0; j < 1000; j++) atomic.incrementAndGet();
            });
            hilos[i].start();
        }
        for (Thread h : hilos) h.join();
        System.out.println("AtomicInteger: " + atomic.get() + " (esperado: 5000)");

        // 5. Critical Section
        System.out.println("\n=== 5. Critical Section ===");
        System.out.println("Seccion critica: codigo dentro de synchronized { }");
        System.out.println("Solo un hilo ejecuta esa seccion a la vez.");
    }

    private static void pausa(int ms) {
        try { Thread.sleep(ms); } catch (InterruptedException e) {
            Thread.currentThread().interrupt();
        }
    }
}
