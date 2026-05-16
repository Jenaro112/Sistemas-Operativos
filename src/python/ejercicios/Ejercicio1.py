import threading
import time

def imprimir_numeros():
    for i in range(1, 11):
        print(f"Numero: {i}")
        time.sleep(0.2)

def imprimir_letras():
    for c in range(ord('A'), ord('K')):
        print(f"Letra: {chr(c)}")
        time.sleep(0.2)

hilo1 = threading.Thread(target=imprimir_numeros)
hilo2 = threading.Thread(target=imprimir_letras)

hilo1.start()
hilo2.start()

hilo1.join()
hilo2.join()

