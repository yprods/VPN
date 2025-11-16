"""
VPN Client - Connects to VPN server and routes traffic through it
"""
import socket
import threading
import select
import sys
from crypto_utils import VPNCrypto


class VPNClient:
    """VPN Client that connects to server and tunnels traffic"""
    
    def __init__(self, server_host, server_port, password=None):
        """
        Initialize VPN Client
        
        Args:
            server_host: VPN server host address
            server_port: VPN server port
            password: Encryption password (optional, must match server)
        """
        self.server_host = server_host
        self.server_port = server_port
        self.crypto = VPNCrypto(password)
        self.running = False
        
        print(f"[CLIENT] Connecting to server at {server_host}:{server_port}")
    
    def connect_to_server(self):
        """Connect to VPN server"""
        try:
            self.server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            self.server_socket.connect((self.server_host, self.server_port))
            self.running = True
            
            print("[CLIENT] Connected to VPN server")
            
            # Receive encryption key from server
            key_length = int.from_bytes(self.server_socket.recv(4), 'big')
            key = self.server_socket.recv(key_length)
            self.crypto.set_key(key)
            
            print("[CLIENT] Encryption established")
            return True
            
        except Exception as e:
            print(f"[CLIENT] Failed to connect: {e}")
            return False
    
    def tunnel_to_target(self, target_host, target_port):
        """
        Tunnel traffic to target through VPN server
        
        Args:
            target_host: Target host to connect to
            target_port: Target port to connect to
        """
        if not self.running:
            if not self.connect_to_server():
                return False
        
        try:
            # Send target info to server (encrypted)
            target_info = f"{target_host}:{target_port}"
            encrypted_info = self.crypto.encrypt(target_info.encode())
            self.server_socket.sendall(encrypted_info)
            
            print(f"[CLIENT] Requesting tunnel to {target_host}:{target_port}")
            
            # Create local socket for client application
            local_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            local_socket.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
            local_socket.bind(('127.0.0.1', 0))
            local_port = local_socket.getsockname()[1]
            local_socket.listen(1)
            
            print(f"[CLIENT] Local proxy listening on 127.0.0.1:{local_port}")
            print(f"[CLIENT] Connect your application to 127.0.0.1:{local_port} to use VPN")
            
            # Accept local connection
            client_socket, client_address = local_socket.accept()
            print(f"[CLIENT] Local client connected from {client_address}")
            
            # Start bidirectional tunneling
            self.tunnel_traffic(client_socket, self.server_socket)
            
            return True
            
        except Exception as e:
            print(f"[CLIENT] Tunneling error: {e}")
            return False
    
    def tunnel_traffic(self, client_socket, server_socket):
        """Tunnel traffic between local client and VPN server"""
        sockets = [client_socket, server_socket]
        
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
                            # Data from local client -> encrypt -> send to server
                            encrypted = self.crypto.encrypt(data)
                            server_socket.sendall(encrypted)
                        else:
                            # Data from server -> decrypt -> send to local client
                            try:
                                decrypted = self.crypto.decrypt(data)
                                client_socket.sendall(decrypted)
                            except Exception as e:
                                print(f"[CLIENT] Decryption error: {e}")
                                return
                                
                    except socket.error:
                        return
                        
        except Exception as e:
            print(f"[CLIENT] Tunneling error: {e}")
        finally:
            client_socket.close()
    
    def stop(self):
        """Stop the VPN client"""
        self.running = False
        if hasattr(self, 'server_socket'):
            self.server_socket.close()
        print("[CLIENT] Client stopped")


def main():
    """Main function to run VPN client"""
    import argparse
    
    parser = argparse.ArgumentParser(description='VPN Client')
    parser.add_argument('--server', required=True, help='VPN server address (host:port)')
    parser.add_argument('--target', required=True, help='Target address (host:port)')
    parser.add_argument('--password', help='Encryption password (optional, must match server)')
    
    args = parser.parse_args()
    
    # Parse server address
    server_parts = args.server.split(':')
    server_host = server_parts[0]
    server_port = int(server_parts[1]) if len(server_parts) > 1 else 8888
    
    # Parse target address
    target_parts = args.target.split(':')
    target_host = target_parts[0]
    target_port = int(target_parts[1]) if len(target_parts) > 1 else 80
    
    client = VPNClient(server_host, server_port, password=args.password)
    
    try:
        client.tunnel_to_target(target_host, target_port)
    except KeyboardInterrupt:
        print("\n[CLIENT] Shutting down...")
        client.stop()


if __name__ == '__main__':
    main()

