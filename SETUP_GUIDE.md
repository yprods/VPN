# Quick Setup Guide - Change IP from Italy to Israel

Follow these steps to set up your VPN and change your IP address to appear from Israel (or any other country).

## Prerequisites

1. **A VPS/Server in Israel** (or your target country)
   - Recommended providers: DigitalOcean, AWS, Linode, Vultr
   - Minimum: 1GB RAM, 1 CPU core
   - Cost: ~$5-10/month

2. **Python 3.7+** installed on both server and your local machine

## Step-by-Step Setup

### Part 1: Server Setup (On Your Israel Server)

1. **SSH into your Israel server:**
   ```bash
   ssh user@your_israel_server_ip
   ```

2. **Install Python and dependencies:**
   ```bash
   # Install Python (if not already installed)
   sudo apt update
   sudo apt install python3 python3-pip -y
   
   # Upload VPN files to server (use scp, ftp, or git)
   # Then install dependencies:
   pip3 install -r requirements.txt
   ```

3. **Start the VPN server:**
   ```bash
   python3 vpn_server.py --host 0.0.0.0 --port 8888 --password YourSecurePassword123
   ```

4. **Keep server running:**
   - Use `screen` or `tmux` to keep it running after SSH disconnect:
   ```bash
   sudo apt install screen -y
   screen -S vpn
   python3 vpn_server.py --host 0.0.0.0 --port 8888 --password YourSecurePassword123
   # Press Ctrl+A then D to detach
   ```

5. **Configure firewall (if needed):**
   ```bash
   sudo ufw allow 8888/tcp
   ```

### Part 2: Client Setup (On Your Local Machine - Italy)

1. **Install dependencies:**
   ```bash
   pip install -r requirements.txt
   ```

2. **Edit `servers.json`:**
   ```json
   {
     "servers": {
       "israel": {
         "host": "YOUR_ISRAEL_SERVER_IP_HERE",
         "port": 8888,
         "country": "Israel",
         "description": "Israel VPN server - Change your IP to appear from Israel"
       }
     }
   }
   ```
   Replace `YOUR_ISRAEL_SERVER_IP_HERE` with your actual server IP.

3. **Connect to VPN:**
   ```bash
   python vpn_manager.py connect --country israel --password YourSecurePassword123
   ```

   You should see:
   ```
   [MANAGER] ✓ Connected to Israel VPN!
   [MANAGER] Your IP will now appear as if you're in Israel
   [MANAGER] SOCKS5 Proxy: 127.0.0.1:1080
   ```

### Part 3: Configure Your Browser

**Chrome/Edge:**
```bash
# Windows
chrome.exe --proxy-server="socks5://127.0.0.1:1080"

# Or create a shortcut with this target:
"C:\Program Files\Google\Chrome\Application\chrome.exe" --proxy-server="socks5://127.0.0.1:1080"
```

**Firefox:**
1. Open Firefox
2. Settings → General → Network Settings → Settings
3. Select "Manual proxy configuration"
4. SOCKS Host: `127.0.0.1`
5. Port: `1080`
6. Select "SOCKS v5"
7. Check "Proxy DNS when using SOCKS v5"
8. Click OK

**Windows System Proxy:**
1. Settings → Network & Internet → Proxy
2. Under "Manual proxy setup", toggle "Use a proxy server"
3. Address: `127.0.0.1`
4. Port: `1080`
5. Save

### Part 4: Verify It Works

1. **Check your IP:**
   ```bash
   python vpn_manager.py check
   ```

2. **Or visit in browser:**
   - https://whatismyipaddress.com
   - https://ipinfo.io
   - https://www.iplocation.net

   You should see an Israeli IP address!

## Switching Between Countries

If you have servers in multiple countries:

1. **Edit `servers.json`** to add more servers
2. **List available servers:**
   ```bash
   python vpn_manager.py list
   ```

3. **Connect to different country:**
   ```bash
   python vpn_manager.py connect --country italy
   python vpn_manager.py connect --country usa
   ```

## Troubleshooting

**"Server not found" error:**
- Check that `servers.json` has the correct server IP
- Verify the server is running on the server side

**"Connection refused" error:**
- Check firewall on server (allow port 8888)
- Verify server is running: `ps aux | grep vpn_server`
- Check server IP address is correct

**"IP didn't change":**
- Make sure browser is using the proxy (127.0.0.1:1080)
- Check that proxy is running (should see "[PROXY] SOCKS5 proxy listening...")
- Try restarting the VPN connection

**Connection drops:**
- Check server is still running
- Verify network connectivity
- Check server logs for errors

## Security Tips

1. **Use strong passwords** (at least 16 characters, mix of letters, numbers, symbols)
2. **Restrict server access** using firewall rules
3. **Keep server updated:** `sudo apt update && sudo apt upgrade`
4. **Monitor server logs** for suspicious activity

## Cost Estimate

- **VPS in Israel:** $5-10/month (DigitalOcean, Linode, etc.)
- **Total setup time:** 15-30 minutes
- **Ongoing maintenance:** Minimal

## Next Steps

- Set up servers in multiple countries
- Automate server startup on boot
- Set up monitoring and alerts
- Configure automatic reconnection

Enjoy your new VPN! Your IP will now appear as if you're browsing from Israel (or whatever country your server is in).

