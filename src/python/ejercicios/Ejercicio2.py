import threading
import time

def descarga():
    for i in range(10, 101, 10):
        print(f"Descargando... {i}%")
        time.sleep(0.5)
    print("Descarga completa.")

def interfaz():
    for _ in range(10):
        print("Interfaz activa...")
        time.sleep(0.5)

hilo1 = threading.Thread(target=descarga)
hilo2 = threading.Thread(target=interfaz)

hilo1.start()
hilo2.start()

hilo1.join()
hilo2.join()

