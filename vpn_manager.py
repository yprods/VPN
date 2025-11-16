"""
VPN Manager - Easy interface to connect to VPN servers in different countries
"""
import json
import sys
import subprocess
import os
from socks5_proxy import SOCKS5Proxy
import threading


class VPNManager:
    """Manages VPN connections to different countries"""
    
    def __init__(self, config_file='servers.json'):
        """Initialize VPN Manager with server configurations"""
        self.config_file = config_file
        self.servers = {}
        self.current_proxy = None
        self.load_servers()
    
    def load_servers(self):
        """Load server configurations from JSON file"""
        try:
            with open(self.config_file, 'r') as f:
                config = json.load(f)
                self.servers = config.get('servers', {})
            print(f"[MANAGER] Loaded {len(self.servers)} server configurations")
        except FileNotFoundError:
            print(f"[MANAGER] Configuration file {self.config_file} not found")
            print("[MANAGER] Please create servers.json with your server configurations")
        except Exception as e:
            print(f"[MANAGER] Error loading configuration: {e}")
    
    def list_servers(self):
        """List all available VPN servers"""
        print("\n=== Available VPN Servers ===")
        for server_id, server_info in self.servers.items():
            print(f"\n{server_id.upper()}:")
            print(f"  Country: {server_info.get('country', 'Unknown')}")
            print(f"  Host: {server_info.get('host', 'Not configured')}")
            print(f"  Port: {server_info.get('port', 8888)}")
            print(f"  Description: {server_info.get('description', '')}")
        print()
    
    def connect(self, country, password=None, proxy_port=1080):
        """
        Connect to a VPN server in a specific country
        
        Args:
            country: Country name (e.g., 'israel', 'italy')
            password: VPN encryption password (optional)
            proxy_port: Local SOCKS5 proxy port (default: 1080)
        """
        country_lower = country.lower()
        
        if country_lower not in self.servers:
            print(f"[MANAGER] Server '{country}' not found in configuration")
            print("[MANAGER] Available servers:")
            for server_id in self.servers.keys():
                print(f"  - {server_id}")
            return False
        
        server_info = self.servers[country_lower]
        server_host = server_info.get('host')
        server_port = server_info.get('port', 8888)
        
        if server_host == f"your_{country_lower}_server_ip" or not server_host:
            print(f"[MANAGER] Server for {country} is not configured!")
            print(f"[MANAGER] Please edit {self.config_file} and set the 'host' field")
            print(f"[MANAGER] You need to deploy a VPN server in {server_info.get('country')} first")
            return False
        
        print(f"[MANAGER] Connecting to {server_info.get('country')} VPN server...")
        print(f"[MANAGER] Server: {server_host}:{server_port}")
        
        # Stop existing proxy if running
        if self.current_proxy:
            self.disconnect()
        
        # Start SOCKS5 proxy
        self.current_proxy = SOCKS5Proxy(
            server_host,
            server_port,
            proxy_port,
            password
        )
        
        # Run proxy in a thread
        proxy_thread = threading.Thread(
            target=self.current_proxy.start,
            daemon=True
        )
        proxy_thread.start()
        
        print(f"\n[MANAGER] ✓ Connected to {server_info.get('country')} VPN!")
        print(f"[MANAGER] Your IP will now appear as if you're in {server_info.get('country')}")
        print(f"[MANAGER] SOCKS5 Proxy: 127.0.0.1:{proxy_port}")
        print(f"[MANAGER] Configure your browser/applications to use this proxy")
        print(f"\n[MANAGER] Press Ctrl+C to disconnect")
        
        try:
            # Keep running
            while True:
                import time
                time.sleep(1)
        except KeyboardInterrupt:
            self.disconnect()
        
        return True
    
    def disconnect(self):
        """Disconnect from current VPN"""
        if self.current_proxy:
            self.current_proxy.stop()
            self.current_proxy = None
            print("[MANAGER] Disconnected from VPN")
    
    def check_ip(self):
        """Check current IP address"""
        try:
            import urllib.request
            print("[MANAGER] Checking your IP address...")
            response = urllib.request.urlopen('https://api.ipify.org?format=json', timeout=5)
            data = json.loads(response.read().decode())
            print(f"[MANAGER] Your current IP: {data.get('ip')}")
            
            # Try to get location info
            try:
                response = urllib.request.urlopen(f"https://ipapi.co/{data.get('ip')}/json/", timeout=5)
                location = json.loads(response.read().decode())
                print(f"[MANAGER] Location: {location.get('city', 'Unknown')}, {location.get('country_name', 'Unknown')}")
            except:
                pass
        except Exception as e:
            print(f"[MANAGER] Could not check IP: {e}")


def main():
    """Main function for VPN Manager"""
    import argparse
    
    parser = argparse.ArgumentParser(description='VPN Manager - Connect to VPN servers in different countries')
    parser.add_argument('command', choices=['list', 'connect', 'check'], help='Command to execute')
    parser.add_argument('--country', help='Country to connect to (for connect command)')
    parser.add_argument('--password', help='VPN encryption password')
    parser.add_argument('--port', type=int, default=1080, help='Local proxy port (default: 1080)')
    
    args = parser.parse_args()
    
    manager = VPNManager()
    
    if args.command == 'list':
        manager.list_servers()
    elif args.command == 'connect':
        if not args.country:
            print("[MANAGER] Error: --country is required for connect command")
            print("[MANAGER] Use 'list' command to see available countries")
            sys.exit(1)
        manager.connect(args.country, args.password, args.port)
    elif args.command == 'check':
        manager.check_ip()


if __name__ == '__main__':
    main()

