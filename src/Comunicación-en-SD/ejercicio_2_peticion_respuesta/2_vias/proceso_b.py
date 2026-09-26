import socket
import sys
import time

# ! IP y Puertos cruzados con respecto al Proceso A
IP_A = sys.argv[1] if len(sys.argv) > 1 else "127.0.0.1"
PUERTO_A_RECV = 5001  
PUERTO_B_RECV = 5002  

# * Inicialización del socket UDP
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
sock.bind(("0.0.0.0", PUERTO_B_RECV))

print("--- PROTOCOLO 2 VÍAS (Proceso B - Respuesta) ---")
print(f"Escuchando peticiones en el puerto {PUERTO_B_RECV}")

# * 1. Recibir REQUEST
# ! Esperando la petición inicial del Proceso A
datos, _ = sock.recvfrom(1024)
print(f"[1] Recibido REQUEST de A: {datos.decode()}")

# * 2. Enviar REPLY
time.sleep(1) # ? Simula tiempo de procesamiento en el servidor
print(f"[2] Enviando REPLY a Proceso A ({IP_A}:{PUERTO_A_RECV})...")
sock.sendto(b"REPLY:Aqui_tienes_la_Informacion", (IP_A, PUERTO_A_RECV))

# ! Cierre del socket
sock.close()
print("Proceso terminado.")
