package ejercicios;

import java.util.LinkedList;
import java.util.Queue;

public class Ejercicio3 {
    private static final int CAPACIDAD = 5;

    public static void main(String[] args) {
        Queue<Integer> cola = new LinkedList<>();
        Object lock = new Object();

        Thread productor = new Thread(() -> {
            for (int i = 1; i <= 20; i++) {
                synchronized (lock) {
                    while (cola.size() == CAPACIDAD) {
                        try { lock.wait(); } catch (InterruptedException e) { }
                    }
                    cola.add(i);
                    System.out.println("Producido: " + i);
                    lock.notifyAll();
                }
                pausa(300);
            }
        });

        Thread consumidor = new Thread(() -> {
            for (int i = 0; i < 20; i++) {
                synchronized (lock) {
                    while (cola.isEmpty()) {
                        try { lock.wait(); } catch (InterruptedException e) { }
                    }
                    int valor = cola.poll();
                    System.out.println("Consumido: " + valor);
                    lock.notifyAll();
                }
                pausa(500);
            }
        });

        productor.start();
        consumidor.start();
    }

    private static void pausa(int ms) {
        try { Thread.sleep(ms); } catch (InterruptedException e) {
            Thread.currentThread().interrupt();
        }
    }
}

/*
Preguntas:
1. ¿Por qué se necesita sincronización?
   Para evitar condiciones de carrera. Sin sincronización, productor
   y consumidor podrían acceder/modificar la cola al mismo tiempo,
   corrompiendo los datos (inconsistencia).

2. ¿Qué error aparece si productor y consumidor acceden libremente a la cola?
   Data race / condicion de carrera. Pueden perderse elementos,
   duplicarse, o la cola puede quedar en un estado inconsistente
   (ej. un poll() sobre una cola vacía da null o excepción).

3. ¿Qué ventajas tiene usar una cola thread-safe?
   - Elimina la necesidad de gestionar locks manualmente.
   - Reduce errores como deadlocks o condiciones de carrera.
   - El codigo es mas limpio y legible.
*/
