package ejercicios;

public class Ejercicio1 {
    public static void main(String[] args) {
        Thread hiloNumeros = new Thread(() -> {
            for (int i = 1; i <= 10; i++) {
                System.out.println("Numero: " + i);
                pausa();
            }
        });

        Thread hiloLetras = new Thread(() -> {
            for (char c = 'A'; c <= 'J'; c++) {
                System.out.println("Letra: " + c);
                pausa();
            }
        });

        hiloNumeros.start();
        hiloLetras.start();
    }

    private static void pausa() {
        try {
            Thread.sleep(200);
        } catch (InterruptedException e) {
            Thread.currentThread().interrupt();
        }
    }
}

