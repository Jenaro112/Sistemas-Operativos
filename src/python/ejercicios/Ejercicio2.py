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

"""
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
"""
