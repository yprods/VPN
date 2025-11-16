# Python VPN - Change Your IP to Different Countries

A VPN implementation that allows you to change your IP address to appear as if you're browsing from a different country (e.g., from Italy to Israel).

## Features

- **Change Your IP Location**: Route all traffic through servers in different countries
- **SOCKS5 Proxy Support**: Easy integration with browsers and applications
- **Encrypted Communication**: All traffic encrypted using Fernet symmetric encryption
- **Country Selection**: Easy-to-use manager for connecting to different countries
- **Password Protection**: Optional password-based encryption
- **Cross-Platform**: Works on Windows, Linux, and macOS

## Installation

1. Install Python 3.7 or higher

2. Install dependencies:
```bash
pip install -r requirements.txt
```

## Quick Start - Change Your IP to Israel (or any country)

### Step 1: Deploy VPN Server in Target Country

You need a server/VPS in the country where you want your IP to appear from. For example, to appear from Israel:

1. **Get a VPS/Server in Israel** (from providers like DigitalOcean, AWS, Linode, etc.)
2. **Deploy the VPN server** on that server:
   ```bash
   python vpn_server.py --host 0.0.0.0 --port 8888 --password your_secure_password
   ```

### Step 2: Configure Your Client

1. **Edit `servers.json`** and add your server's IP address:
   ```json
   {
     "servers": {
       "israel": {
         "host": "your_israel_server_ip_address",
         "port": 8888,
         "country": "Israel",
         "description": "Israel VPN server"
       }
     }
   }
   ```

### Step 3: Connect to VPN

**Option A: Using VPN Manager (Recommended)**
```bash
# List available servers
python vpn_manager.py list

# Connect to Israel VPN
python vpn_manager.py connect --country israel --password your_secure_password
```

**Option B: Using SOCKS5 Proxy Directly**
```bash
python socks5_proxy.py --server your_israel_server_ip:8888 --password your_secure_password
```

### Step 4: Configure Your Browser/Application

Once connected, configure your browser or application to use the SOCKS5 proxy:

- **Proxy Server**: `127.0.0.1`
- **Port**: `1080` (default)
- **Type**: SOCKS5

**Chrome/Edge:**
```bash
chrome.exe --proxy-server="socks5://127.0.0.1:1080"
```

**Firefox:**
1. Settings → Network Settings → Manual proxy configuration
2. SOCKS Host: `127.0.0.1`, Port: `1080`
3. Select "SOCKS v5"

**Windows System Proxy:**
- Settings → Network & Internet → Proxy
- Manual proxy setup → Use a proxy server
- Address: `127.0.0.1`, Port: `1080`

### Step 5: Verify Your IP Changed

```bash
# Check your IP address
python vpn_manager.py check

# Or visit: https://whatismyipaddress.com
```

## Example: Change from Italy to Israel

1. **On your Israel server:**
   ```bash
   python vpn_server.py --port 8888 --password mypassword123
   ```

2. **On your local machine (Italy):**
   ```bash
   # Edit servers.json with your Israel server IP
   # Then connect:
   python vpn_manager.py connect --country israel --password mypassword123
   ```

3. **Configure browser to use proxy** (127.0.0.1:1080)

4. **Check IP** - You should now see an Israeli IP address!

## Multiple Countries Setup

You can set up servers in multiple countries and easily switch between them:

```json
{
  "servers": {
    "israel": {
      "host": "israel_server_ip",
      "port": 8888,
      "country": "Israel"
    },
    "italy": {
      "host": "italy_server_ip",
      "port": 8888,
      "country": "Italy"
    },
    "usa": {
      "host": "usa_server_ip",
      "port": 8888,
      "country": "United States"
    }
  }
}
```

Then switch between countries:
```bash
python vpn_manager.py connect --country israel
python vpn_manager.py connect --country italy
python vpn_manager.py connect --country usa
```

## Advanced Usage

### Direct Server Connection
```bash
python vpn_server.py --host 0.0.0.0 --port 8888 --password secure_password
```

### Direct Client Connection
```bash
python vpn_client.py --server server_ip:8888 --target google.com:80 --password secure_password
```

### Custom Proxy Port
```bash
python socks5_proxy.py --server server_ip:8888 --port 8080 --password secure_password
```

## How It Works

1. **VPN Server** runs on a server in your target country (e.g., Israel)
2. **VPN Client** connects to the server and creates a local SOCKS5 proxy
3. **Your Applications** connect to the local proxy (127.0.0.1:1080)
4. **All Traffic** is encrypted and routed through the server
5. **Your IP** appears as the server's IP address (Israeli IP in this case)

## Server Deployment Guide

### Option 1: Cloud VPS Providers

1. **DigitalOcean**: Create a droplet in your target country
2. **AWS EC2**: Launch instance in target region
3. **Linode**: Create a node in target location
4. **Vultr**: Deploy server in target country

### Option 2: Using SSH Tunnel

If you have SSH access to a server in the target country:

```bash
# On the server
python vpn_server.py --port 8888 --password your_password

# On your local machine
python vpn_manager.py connect --country israel --password your_password
```

## Security Notes

- Always use strong passwords for VPN connections
- Consider using firewall rules to restrict server access
- For production use, consider:
  - Using stronger encryption (AES-256)
  - Implementing certificate-based authentication
  - Using random salts for key derivation
  - Adding rate limiting and connection monitoring

## Troubleshooting

**Can't connect to server:**
- Check firewall rules (allow port 8888)
- Verify server IP address in servers.json
- Ensure server is running

**IP didn't change:**
- Make sure browser/application is using the proxy
- Verify proxy is running (check port 1080)
- Try restarting the proxy connection

**Connection drops:**
- Check server status
- Verify network connectivity
- Check server logs for errors

## Limitations

- Requires a server/VPS in the target country
- Currently supports TCP traffic only
- No automatic reconnection on connection loss
- Basic error handling

## License

This project is provided as-is for educational purposes.
