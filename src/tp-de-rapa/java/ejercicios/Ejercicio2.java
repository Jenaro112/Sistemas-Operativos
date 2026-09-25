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

