import socket
import time

# ! Puertos cruzados según consigna
PUERTO_A_RECV = 5001
PUERTO_B_RECV = 5002

# * Socket UDP
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
# ! PREVENCIÓN ERROR 48
sock.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
sock.bind(("0.0.0.0", PUERTO_B_RECV))

print("--- PROTOCOLO 3 VÍAS (Proceso B - Respuesta) ---")

# * 1. Recibir REQUEST
# ! Obtenemos la IP automáticamente para funcionar en 2 PCs
datos, direccion_origen = sock.recvfrom(1024)
IP_A = direccion_origen[0]
print(f"[1] Recibido REQUEST de A ({IP_A}): {datos.decode()}")

# * 2. Enviar REPLY
time.sleep(1)
print(f"[2] Enviando REPLY a A ({IP_A}:{PUERTO_A_RECV})...")
sock.sendto(b"REPLY:Aqui_tienes_DatoX", (IP_A, PUERTO_A_RECV))

# * 3. Recibir ACK de la Respuesta
# ! El servidor ahora debe esperar la confirmación de llegada
datos_ack, _ = sock.recvfrom(1024)
print(f"[3] Recibido ACK de A: {datos_ack.decode()}")

# ! Fin de ejecución
sock.close()
print("Proceso terminado.")
