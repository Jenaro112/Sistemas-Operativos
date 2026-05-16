import threading
import time

recurso_ocupado = False

def hilo_a():
    global recurso_ocupado
    for _ in range(10):
        if recurso_ocupado:
            print("Hilo A: recurso ocupado, cedo el paso...")
        else:
            recurso_ocupado = True
            print("Hilo A: tomo el recurso")
            recurso_ocupado = False
        time.sleep(0.1)

def hilo_b():
    global recurso_ocupado
    for _ in range(10):
        if recurso_ocupado:
            print("Hilo B: recurso ocupado, cedo el paso...")
        else:
            recurso_ocupado = True
            print("Hilo B: tomo el recurso")
            recurso_ocupado = False
        time.sleep(0.1)

a = threading.Thread(target=hilo_a)
b = threading.Thread(target=hilo_b)
a.start()
b.start()
a.join()
b.join()

print("\nAmbos hilos estuvieron activos todo el tiempo")
print("pero rara vez lograron tomar el recurso (livelock).")

"""
Preguntas:
1. ¿Por que livelock no es igual a deadlock?
   En deadlock los hilos estan bloqueados esperando. En livelock los
   hilos estan activos, ejecutando codigo, pero sin progreso real.

2. ¿Los hilos estan detenidos?
   No, los hilos estan activos y ejecutandose continuamente.
   Solo que ninguno logra completar su tarea.

3. ¿Como se podria resolver?
   - Introducir backoff aleatorio: cada hilo espera un tiempo
     aleatorio antes de reintentar, reduciendo colisiones.
   - Usar un mecanismo de coordinacion como un coordinator
     que asigna turnos de forma explcita.
"""
