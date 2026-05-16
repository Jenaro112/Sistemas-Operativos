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

"""
Preguntas:
1. ¿El orden de ejecución es siempre el mismo?
   No, el orden varía cada vez que se ejecuta. Depende del scheduler del SO.

2. ¿Quién decide qué hilo se ejecuta primero?
   El scheduler (planificador) del sistema operativo decide qué hilo
   se ejecuta y por cuánto tiempo.

3. ¿Qué sucede si se eliminan las pausas?
   Un hilo tiende a ejecutarse completamente antes de que el otro
   comience, porque el cambio de contexto ocurre menos frecuentemente.
   Se pierde la intercalación visible entre hilos.
"""
