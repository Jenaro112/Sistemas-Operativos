package ejercicios;

import java.util.concurrent.atomic.AtomicInteger;

public class Ejercicio4 {
    static int contadorInseguro = 0;
    static AtomicInteger contadorSeguro = new AtomicInteger(0);
    static final Object lock = new Object();
    static int contadorSincronizado = 0;

    public static void main(String[] args) throws InterruptedException {
        // SIN sincronizacion
        Thread[] hilos = new Thread[5];
        for (int i = 0; i < 5; i++) {
            hilos[i] = new Thread(() -> {
                for (int j = 0; j < 10000; j++) {
                    contadorInseguro++;
                }
            });
            hilos[i].start();
        }
        for (Thread h : hilos) h.join();
        System.out.println("Esperado: 50000 | Sin control: " + contadorInseguro);

        // CON sincronizacion (synchronized)
        contadorSincronizado = 0;
        for (int i = 0; i < 5; i++) {
            hilos[i] = new Thread(() -> {
                for (int j = 0; j < 10000; j++) {
                    synchronized (lock) { contadorSincronizado++; }
                }
            });
            hilos[i].start();
        }
        for (Thread h : hilos) h.join();
        System.out.println("Esperado: 50000 | Con synchronized: " + contadorSincronizado);

        // CON AtomicInteger
        contadorSeguro.set(0);
        for (int i = 0; i < 5; i++) {
            hilos[i] = new Thread(() -> {
                for (int j = 0; j < 10000; j++) {
                    contadorSeguro.incrementAndGet();
                }
            });
            hilos[i].start();
        }
        for (Thread h : hilos) h.join();
        System.out.println("Esperado: 50000 | Con AtomicInteger: " + contadorSeguro.get());
    }
}

