"""
VPN Server - Accepts client connections and tunnels traffic
"""
import socket
import threading
import select
import sys
from crypto_utils import VPNCrypto


class VPNServer:
    """VPN Server that handles client connections and traffic tunneling"""
    
    def __init__(self, host='0.0.0.0', port=8888, password=None):
        """
        Initialize VPN Server
        
        Args:
            host: Server host address
            port: Server port
            password: Encryption password (optional)
        """
        self.host = host
        self.port = port
        self.crypto = VPNCrypto(password)
        self.clients = {}
        self.running = False
        
        print(f"[SERVER] Initialized on {host}:{port}")
        if password:
            print(f"[SERVER] Using password-based encryption")
        else:
            print(f"[SERVER] Encryption key: {self.crypto.get_key().decode()[:20]}...")
    
    def start(self):
        """Start the VPN server"""
        self.server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self.server_socket.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
        
        try:
            self.server_socket.bind((self.host, self.port))
            self.server_socket.listen(5)
            self.running = True
            
            print(f"[SERVER] Listening on {self.host}:{self.port}")
            print("[SERVER] Waiting for client connections...")
            
            while self.running:
                try:
                    client_socket, client_address = self.server_socket.accept()
                    print(f"[SERVER] New client connected from {client_address}")
                    
                    # Handle client in separate thread
                    client_thread = threading.Thread(
                        target=self.handle_client,
                        args=(client_socket, client_address),
                        daemon=True
                    )
                    client_thread.start()
                    
                except OSError:
                    if self.running:
                        print("[SERVER] Server socket closed")
                    break
                    
        except Exception as e:
            print(f"[SERVER] Error: {e}")
        finally:
            self.stop()
    
    def handle_client(self, client_socket, client_address):
        """Handle a client connection"""
        try:
            # Send encryption key to client
            key = self.crypto.get_key()
            key_length = len(key).to_bytes(4, 'big')
            client_socket.sendall(key_length + key)
            
            # Receive target connection info from client
            encrypted_data = client_socket.recv(4096)
            if not encrypted_data:
                return
            
            try:
                decrypted_data = self.crypto.decrypt(encrypted_data)
                target_info = decrypted_data.decode().split(':')
                target_host = target_info[0]
                target_port = int(target_info[1])
                
                print(f"[SERVER] Client wants to connect to {target_host}:{target_port}")
                
                # Create connection to target
                target_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
                target_socket.settimeout(10)
                
                try:
                    target_socket.connect((target_host, target_port))
                    print(f"[SERVER] Connected to target {target_host}:{target_port}")
                    
                    # Start bidirectional tunneling
                    self.tunnel_traffic(client_socket, target_socket, client_address)
                    
                except Exception as e:
                    print(f"[SERVER] Failed to connect to target: {e}")
                    client_socket.close()
                    
            except Exception as e:
                print(f"[SERVER] Error decrypting client data: {e}")
                client_socket.close()
                
        except Exception as e:
            print(f"[SERVER] Error handling client: {e}")
        finally:
            if client_socket in self.clients:
                del self.clients[client_socket]
            print(f"[SERVER] Client {client_address} disconnected")
    
    def tunnel_traffic(self, client_socket, target_socket, client_address):
        """Tunnel traffic between client and target"""
        sockets = [client_socket, target_socket]
        
        try:
            while True:
                readable, _, exceptional = select.select(sockets, [], sockets, 1)
                
                if exceptional:
                    break
                
                for sock in readable:
                    try:
                        data = sock.recv(4096)
                        if not data:
                            return
                        
                        if sock is client_socket:
                            # Data from client -> decrypt -> send to target
                            try:
                                decrypted = self.crypto.decrypt(data)
                                target_socket.sendall(decrypted)
                            except Exception as e:
                                print(f"[SERVER] Decryption error: {e}")
                                return
                        else:
                            # Data from target -> encrypt -> send to client
                            encrypted = self.crypto.encrypt(data)
                            client_socket.sendall(encrypted)
                            
                    except socket.error:
                        return
                        
        except Exception as e:
            print(f"[SERVER] Tunneling error: {e}")
        finally:
            client_socket.close()
            target_socket.close()
    
    def stop(self):
        """Stop the VPN server"""
        self.running = False
        if hasattr(self, 'server_socket'):
            self.server_socket.close()
        print("[SERVER] Server stopped")


def main():
    """Main function to run VPN server"""
    import argparse
    
    parser = argparse.ArgumentParser(description='VPN Server')
    parser.add_argument('--host', default='0.0.0.0', help='Server host (default: 0.0.0.0)')
    parser.add_argument('--port', type=int, default=8888, help='Server port (default: 8888)')
    parser.add_argument('--password', help='Encryption password (optional)')
    
    args = parser.parse_args()
    
    server = VPNServer(host=args.host, port=args.port, password=args.password)
    
    try:
        server.start()
    except KeyboardInterrupt:
        print("\n[SERVER] Shutting down...")
        server.stop()


if __name__ == '__main__':
    main()

