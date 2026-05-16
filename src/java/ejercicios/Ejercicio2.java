package ejercicios;

public class Ejercicio2 {
    public static void main(String[] args) {
        Thread descarga = new Thread(() -> {
            for (int i = 10; i <= 100; i += 10) {
                System.out.println("Descargando... " + i + "%");
                pausa(500);
            }
            System.out.println("Descarga completa.");
        });

        Thread interfaz = new Thread(() -> {
            for (int i = 0; i < 10; i++) {
                System.out.println("Interfaz activa...");
                pausa(500);
            }
        });

        descarga.start();
        interfaz.start();
    }

    private static void pausa(int ms) {
        try {
            Thread.sleep(ms);
        } catch (InterruptedException e) {
            Thread.currentThread().interrupt();
        }
    }
}

/*
Preguntas:
1. ¿Qué pasaría si la descarga se ejecutara en el hilo principal?
   La interfaz se congelaría hasta que la descarga termine.
   La aplicación no respondería a clics, entradas del usuario, etc.

2. ¿Por qué es importante esto en aplicaciones gráficas?
   Porque el hilo principal (UI thread) debe estar siempre disponible
   para procesar eventos del usuario. Si se bloquea, la app parece
   congelada o "no responsive".

3. ¿Este caso es CPU-bound o I/O-bound?
   Es I/O-bound (ligado a E/S). La descarga simula una operación de
   entrada/salida (red/disco), no un cómputo intensivo del procesador.
*/
