import socket

# * Configuración del servidor
# ! 0.0.0.0 permite escuchar en cualquier interfaz de red (ideal para 2 computadoras)
IP_SERVIDOR = "0.0.0.0"
PUERTO = 5000

# * 1. Crear socket con interfaz de datagramas (UDP)
# * AF_INET = IPv4
# ! SOCK_DGRAM = Datagramas (UDP - No orientado a conexión)
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

# * 2. Asociar el socket al puerto e IP
sock.bind((IP_SERVIDOR, PUERTO))

print(f"[*] Servidor UDP escuchando en el puerto {PUERTO}...")
print("[*] Listo para recibir mensajes. Presiona Ctrl+C para salir.\n")

try:
    while True:
        # ! 3. Recibir mensajes (bloqueante hasta que llegue algo)
        # ? 1024 es el tamaño máximo del buffer en bytes
        datos, direccion_cliente = sock.recvfrom(1024)
        
        # * Decodificar los bytes a texto
        mensaje = datos.decode('utf-8')
        
        # * Mostrar el mensaje y la IP de la computadora que lo envió
        ip_cliente = direccion_cliente[0]
        puerto_cliente = direccion_cliente[1]
        print(f"[{ip_cliente}:{puerto_cliente}] dice: {mensaje}")

except KeyboardInterrupt:
    print("\n[*] Apagando servidor...")
finally:
    # ! 4. Cerrar el socket al terminar para liberar el puerto
    sock.close()
