"""
SOCKS5 Proxy Server - Routes all traffic through VPN
This creates a local SOCKS5 proxy that applications can use
"""
import socket
import struct
import threading
from vpn_client import VPNClient


class SOCKS5Proxy:
    """SOCKS5 proxy server that routes traffic through VPN"""
    
    def __init__(self, vpn_server_host, vpn_server_port, local_port=1080, password=None):
        """
        Initialize SOCKS5 Proxy
        
        Args:
            vpn_server_host: VPN server host
            vpn_server_port: VPN server port
            local_port: Local SOCKS5 proxy port (default: 1080)
            password: VPN encryption password
        """
        self.vpn_server_host = vpn_server_host
        self.vpn_server_port = vpn_server_port
        self.local_port = local_port
        self.password = password
        self.running = False
        
    def start(self):
        """Start the SOCKS5 proxy server"""
        self.proxy_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self.proxy_socket.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
        
        try:
            self.proxy_socket.bind(('127.0.0.1', self.local_port))
            self.proxy_socket.listen(10)
            self.running = True
            
            print(f"[PROXY] SOCKS5 proxy listening on 127.0.0.1:{self.local_port}")
            print(f"[PROXY] Configure your applications to use this proxy")
            print(f"[PROXY] Server: 127.0.0.1, Port: {self.local_port}")
            print(f"[PROXY] Routing all traffic through VPN server: {self.vpn_server_host}:{self.vpn_server_port}")
            
            while self.running:
                try:
                    client_socket, client_address = self.proxy_socket.accept()
                    print(f"[PROXY] New client connection from {client_address}")
                    
                    # Handle client in separate thread
                    client_thread = threading.Thread(
                        target=self.handle_client,
                        args=(client_socket,),
                        daemon=True
                    )
                    client_thread.start()
                    
                except OSError:
                    if self.running:
                        break
                        
        except Exception as e:
            print(f"[PROXY] Error: {e}")
        finally:
            self.stop()
    
    def handle_client(self, client_socket):
        """Handle SOCKS5 client connection"""
        try:
            # SOCKS5 handshake
            # Read authentication methods
            version = client_socket.recv(1)
            if version != b'\x05':
                client_socket.close()
                return
            
            nmethods = client_socket.recv(1)[0]
            methods = client_socket.recv(nmethods)
            
            # Send no authentication required
            client_socket.sendall(b'\x05\x00')
            
            # Read connection request
            request = client_socket.recv(4)
            if len(request) < 4 or request[0] != 5:
                client_socket.close()
                return
            
            cmd = request[1]
            if cmd != 1:  # Only support CONNECT
                client_socket.sendall(b'\x05\x07\x00\x01\x00\x00\x00\x00\x00\x00')
                client_socket.close()
                return
            
            addr_type = request[3]
            
            # Read address
            if addr_type == 1:  # IPv4
                addr = socket.inet_ntoa(client_socket.recv(4))
            elif addr_type == 3:  # Domain name
                addr_len = client_socket.recv(1)[0]
                addr = client_socket.recv(addr_len).decode()
            elif addr_type == 4:  # IPv6
                addr = socket.inet_ntop(socket.AF_INET6, client_socket.recv(16))
            else:
                client_socket.close()
                return
            
            # Read port
            port = struct.unpack('>H', client_socket.recv(2))[0]
            
            print(f"[PROXY] Client wants to connect to {addr}:{port}")
            
            # Create VPN client for this connection
            vpn_client = VPNClient(self.vpn_server_host, self.vpn_server_port, self.password)
            
            if not vpn_client.connect_to_server():
                client_socket.sendall(b'\x05\x01\x00\x01\x00\x00\x00\x00\x00\x00')
                client_socket.close()
                return
            
            # Send target info to VPN server
            target_info = f"{addr}:{port}"
            encrypted_info = vpn_client.crypto.encrypt(target_info.encode())
            vpn_client.server_socket.sendall(encrypted_info)
            
            # Send success response
            client_socket.sendall(b'\x05\x00\x00\x01\x00\x00\x00\x00\x00\x00')
            
            # Tunnel traffic
            self.tunnel_traffic(client_socket, vpn_client.server_socket, vpn_client.crypto)
            
        except Exception as e:
            print(f"[PROXY] Error handling client: {e}")
            try:
                client_socket.close()
            except:
                pass
    
    def tunnel_traffic(self, client_socket, server_socket, crypto):
        """Tunnel traffic between client and VPN server"""
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
                            # Data from client -> encrypt -> send to VPN server
                            encrypted = crypto.encrypt(data)
                            server_socket.sendall(encrypted)
                        else:
                            # Data from VPN server -> decrypt -> send to client
                            try:
                                decrypted = crypto.decrypt(data)
                                client_socket.sendall(decrypted)
                            except Exception as e:
                                print(f"[PROXY] Decryption error: {e}")
                                return
                                
                    except socket.error:
                        return
                        
        except Exception as e:
            print(f"[PROXY] Tunneling error: {e}")
        finally:
            client_socket.close()
            server_socket.close()
    
    def stop(self):
        """Stop the proxy server"""
        self.running = False
        if hasattr(self, 'proxy_socket'):
            self.proxy_socket.close()
        print("[PROXY] Proxy stopped")


def main():
    """Main function to run SOCKS5 proxy"""
    import argparse
    import select
    
    parser = argparse.ArgumentParser(description='SOCKS5 Proxy through VPN')
    parser.add_argument('--server', required=True, help='VPN server address (host:port)')
    parser.add_argument('--port', type=int, default=1080, help='Local proxy port (default: 1080)')
    parser.add_argument('--password', help='VPN encryption password')
    
    args = parser.parse_args()
    
    # Parse server address
    server_parts = args.server.split(':')
    server_host = server_parts[0]
    server_port = int(server_parts[1]) if len(server_parts) > 1 else 8888
    
    proxy = SOCKS5Proxy(server_host, server_port, args.port, args.password)
    
    try:
        proxy.start()
    except KeyboardInterrupt:
        print("\n[PROXY] Shutting down...")
        proxy.stop()


if __name__ == '__main__':
    main()

