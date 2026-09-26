import socket
import time

# ! Puertos cruzados con respecto al Proceso A
PUERTO_A_RECV = 5001  
PUERTO_B_RECV = 5002  

# * Inicialización del socket UDP
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
# ! PREVENCIÓN ERROR 48
sock.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
# ! 0.0.0.0 permite escuchar desde cualquier computadora en la red local
sock.bind(("0.0.0.0", PUERTO_B_RECV))

print("--- PROTOCOLO 2 VÍAS (Proceso B - Respuesta) ---")
print(f"Escuchando peticiones en el puerto {PUERTO_B_RECV}")

# * 1. Recibir REQUEST
# ! Capturamos la dirección (IP) real desde donde Proceso A envió el mensaje
datos, direccion_origen = sock.recvfrom(1024)
IP_A = direccion_origen[0]
print(f"[1] Recibido REQUEST de A ({IP_A}): {datos.decode()}")

# * 2. Enviar REPLY
time.sleep(1) # ? Simula tiempo de procesamiento en el servidor
print(f"[2] Enviando REPLY a Proceso A ({IP_A}:{PUERTO_A_RECV})...")
sock.sendto(b"REPLY:Aqui_tienes_la_Informacion", (IP_A, PUERTO_A_RECV))

# ! Cierre del socket
sock.close()
print("Proceso terminado.")
