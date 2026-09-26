import socket

# * Configuración
IP_SERVIDOR = "0.0.0.0"
PUERTO = 6000

# * 1. Crear socket TCP
# ! SOCK_STREAM es lo que define a TCP (Confiable y orientado a conexión)
sock_server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

# ! Permite reutilizar el puerto rápidamente si reiniciamos el script
sock_server.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)

# * 2. Asociar a puerto e IP (Bind)
sock_server.bind((IP_SERVIDOR, PUERTO))

# * 3. Escuchar conexiones entrantes
# ? El '5' es el backlog: el número máximo de conexiones en espera (cola)
sock_server.listen(5)
print(f"[*] Servidor TCP confiable escuchando en el puerto {PUERTO}...")

try:
    while True:
        # * 4. Aceptar conexión (Se realiza el 3-way handshake por detrás de escena)
        # ! .accept() bloquea hasta que un cliente intente conectarse
        conexion_cliente, direccion = sock_server.accept()
        print(f"[+] Nueva conexión desde {direccion[0]}:{direccion[1]}")

        while True:
            # * 5. Recibir datos del cliente
            datos = conexion_cliente.recv(1024)
            
            # ! Si datos viene vacío, significa que el cliente cortó la conexión (FIN)
            if not datos:
                print(f"[-] El cliente {direccion[0]} se ha desconectado.")
                break
            
            mensaje = datos.decode('utf-8')
            print(f"[Cliente TCP] -> {mensaje}")

            # * 6. Enviar respuesta usando sendall (Garantiza entrega confiable)
            respuesta = f"Servidor TCP acusa recibo del mensaje: '{mensaje}'"
            conexion_cliente.sendall(respuesta.encode('utf-8'))
        
        # ! Cerrar la conexión específica solo con ese cliente
        conexion_cliente.close()

except KeyboardInterrupt:
    print("\n[*] Apagando servidor...")
finally:
    # ! Cerrar el socket maestro
    sock_server.close()
