import socket
import threading

# * Configuración del servidor de chat
IP_SERVIDOR = "0.0.0.0"  # ! Escucha en todas las interfaces de red para soportar múltiples computadoras
PUERTO = 7000

# * Lista para almacenar los sockets de todos los clientes conectados
clientes = []
# * Lock para evitar condiciones de carrera (Race Conditions) al modificar la lista de clientes
lock_clientes = threading.Lock()

def broadcast(mensaje, remitente_socket=None):
    """
    * Función que retransmite (broadcast) un mensaje a todos los clientes conectados.
    ? Si se especifica remitente_socket, no le vuelve a enviar el mensaje a él mismo.
    """
    with lock_clientes:
        for cliente in clientes:
            if cliente != remitente_socket:
                try:
                    cliente.sendall(mensaje)
                except Exception:
                    # ! Si falla el envío (ej: cliente desconectado abruptamente), se ignora aquí
                    # ! y será limpiado por su propio hilo.
                    pass

def manejar_cliente(conexion, direccion):
    """
    * Hilo independiente para atender a cada cliente que se conecta.
    """
    print(f"[+] Nueva conexión desde {direccion[0]}:{direccion[1]}")
    
    apodo = "Anónimo"
    try:
        # * 1. Lo primero que esperamos del cliente es su apodo / nombre
        apodo = conexion.recv(1024).decode('utf-8').strip()
        if not apodo:
            apodo = f"Usuario-{direccion[1]}"
            
        with lock_clientes:
            clientes.append(conexion)

        # * 2. Notificar a todos los demás participantes que alguien se unió
        mensaje_bienvenida = f"\n📢 [SISTEMA]: ¡{apodo} se unió a la sala de chat!\n"
        print(f"[*] {apodo} ({direccion[0]}) ingresó al chat.")
        broadcast(mensaje_bienvenida.encode('utf-8'), remitente_socket=conexion)
        
        conexion.sendall(f"📢 [SISTEMA]: Bienvenido/a al chat, {apodo}! Escribí 'salir' para abandonar.\n".encode('utf-8'))

        # * 3. Bucle para recibir mensajes continuos de este cliente
        while True:
            datos = conexion.recv(1024)
            # ! Si recv no devuelve nada, el cliente cerró la conexión
            if not datos:
                break
            
            mensaje = datos.decode('utf-8')
            if mensaje.lower().strip() == 'salir':
                break

            # * Formatear el mensaje con el nombre del autor y enviarlo a los demás
            mensaje_formateado = f"[{apodo}]: {mensaje}\n"
            print(f"[{apodo}]: {mensaje}")
            broadcast(mensaje_formateado.encode('utf-8'), remitente_socket=conexion)

    except (ConnectionResetError, ConnectionAbortedError):
        # ! Ocurre si el cliente cierra la terminal bruscamente
        print(f"[-] Conexión perdida repentinamente con {apodo}")
    finally:
        # * 4. Limpieza al desconectarse el cliente
        with lock_clientes:
            if conexion in clientes:
                clientes.remove(conexion)
        conexion.close()
        
        # ! Avisar a los demás usuarios que este participante se fue
        despedida = f"\n📢 [SISTEMA]: {apodo} abandonó la sala.\n"
        print(f"[-] {apodo} se desconectó.")
        broadcast(despedida.encode('utf-8'))

def iniciar_servidor():
    # * 1. Crear socket TCP
    # ! SOCK_STREAM garantiza entrega confiable y ordenada de los mensajes del chat
    servidor = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    # ! SO_REUSEADDR previene el 'Errno 48: Address already in use'
    servidor.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
    
    servidor.bind((IP_SERVIDOR, PUERTO))
    servidor.listen(10) # ? Permite hasta 10 conexiones en espera
    print("==================================================")
    print(f"🚀 Servidor de Chat Grupal activo en el puerto {PUERTO}")
    print("Listo para recibir múltiples computadoras en simultáneo...")
    print("==================================================\n")

    try:
        while True:
            # ! accept() es bloqueante: espera a que una nueva compu se conecte
            conexion, direccion = servidor.accept()
            
            # * Creamos un hilo (Thread) exclusivo para este cliente para no bloquear el servidor
            hilo = threading.Thread(target=manejar_cliente, args=(conexion, direccion))
            hilo.daemon = True # ? Permite que el hilo muera si cerramos el servidor
            hilo.start()
            
    except KeyboardInterrupt:
        print("\n[*] Apagando servidor de chat...")
    finally:
        servidor.close()

if __name__ == "__main__":
    iniciar_servidor()
