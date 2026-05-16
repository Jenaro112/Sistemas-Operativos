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

/*
Preguntas:
1. ¿Por qué el resultado puede ser incorrecto?
   Porque ++ no es atomico: es leer, sumar, escribir. Dos hilos pueden
   leer el mismo valor, incrementarlo y escribir, perdiendose incrementos.

2. ¿Qué es una seccion critica?
   Es la parte del codigo que accede a un recurso compartido y no debe
   ser ejecutada por mas de un hilo a la vez para evitar inconsistencias.

3. ¿Qué diferencia hay entre usar lock y operaciones atomicas?
   - lock: bloquea una seccion critica completa, permitiendo varias operaciones
     como una unidad atomica. Puede causar contention/sobrecarga.
   - atomicas (AtomicInteger, Interlocked): operaciones a nivel de hardware
     (CAS), mas rapidas y sin riesgo de deadlocks, pero solo para
     operaciones simples (incrementar, sumar, etc.).
*/
