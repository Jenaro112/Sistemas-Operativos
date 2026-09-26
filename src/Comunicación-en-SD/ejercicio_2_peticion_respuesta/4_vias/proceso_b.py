import socket
import sys
import time

# ! IPs y Puertos cruzados
IP_A = sys.argv[1] if len(sys.argv) > 1 else "127.0.0.1"
PUERTO_A_RECV = 5001
PUERTO_B_RECV = 5002

# * Socket UDP
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
sock.bind(("0.0.0.0", PUERTO_B_RECV))

print("--- PROTOCOLO 4 VÍAS (Proceso B - Respuesta) ---")

# * 1. Recibir REQUEST
datos_req, _ = sock.recvfrom(1024)
print(f"[1] Recibido REQUEST de A: {datos_req.decode()}")

# * 2. Enviar ACK del Request
time.sleep(0.5)
# ! B avisa de inmediato que la conexión se hizo, evitando un reintento del cliente
print(f"[2] Enviando ACK a A (Recibí tu petición)...")
sock.sendto(b"ACK:Recibi_tu_REQUEST", (IP_A, PUERTO_A_RECV))

# * 3. Enviar REPLY (La respuesta real tras procesar)
time.sleep(2) # ? Simula un procesamiento complejo y largo
print(f"[3] Enviando REPLY a A (Procesamiento terminado)...")
sock.sendto(b"REPLY:Resultado_Exitoso", (IP_A, PUERTO_A_RECV))

# * 4. Recibir ACK del Reply
# ! B exige una última confirmación del cliente para considerar la transacción terminada
datos_ack_rep, _ = sock.recvfrom(1024)
print(f"[4] Recibido ACK final de A: {datos_ack_rep.decode()}")

# ! Fin de ejecución
sock.close()
print("Proceso terminado.")
