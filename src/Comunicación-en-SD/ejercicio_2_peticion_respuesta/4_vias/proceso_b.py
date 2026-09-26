import socket
import time

# ! Puertos cruzados
PUERTO_A_RECV = 5001
PUERTO_B_RECV = 5002

# * Socket UDP
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
# ! PREVENCIÓN ERROR 48
sock.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
sock.bind(("0.0.0.0", PUERTO_B_RECV))

print("--- PROTOCOLO 4 VÍAS (Proceso B - Respuesta) ---")

# * 1. Recibir REQUEST
# ! Capturamos la IP de A automáticamente del primer mensaje
datos_req, direccion_origen = sock.recvfrom(1024)
IP_A = direccion_origen[0]
print(f"[1] Recibido REQUEST de A ({IP_A}): {datos_req.decode()}")

# * 2. Enviar ACK del Request
time.sleep(0.5)
# ! Aviso a A que el mensaje llegó
print(f"[2] Enviando ACK a A (Recibí tu petición)...")
sock.sendto(b"ACK:Recibi_tu_REQUEST", (IP_A, PUERTO_A_RECV))

# * 3. Enviar REPLY (La respuesta real tras procesar)
time.sleep(2) # ? Simula un procesamiento que toma su tiempo
print(f"[3] Enviando REPLY a A (Procesamiento terminado)...")
sock.sendto(b"REPLY:Resultado_Exitoso", (IP_A, PUERTO_A_RECV))

# * 4. Recibir ACK del Reply
# ! B ahora verifica que el cliente haya recibido el resultado
datos_ack_rep, _ = sock.recvfrom(1024)
print(f"[4] Recibido ACK final de A: {datos_ack_rep.decode()}")

# ! Fin de ejecución
sock.close()
print("Proceso terminado.")
