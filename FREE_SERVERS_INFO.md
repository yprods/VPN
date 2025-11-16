# Free VPN Servers Information

## Important Note

**Our VPN implementation uses a custom Python protocol** that requires servers running our specific `vpn_server.py` script. Standard free VPN services (like ProtonVPN, Windscribe, etc.) use different protocols (OpenVPN, WireGuard, IKEv2) and **will NOT work** with this application.

## Why Free Public Servers Don't Work

1. **Different Protocols**: Free VPN services use standard protocols (OpenVPN, WireGuard) that our custom Python VPN doesn't support
2. **Custom Implementation**: Our VPN uses a custom encrypted tunnel protocol on port 8888
3. **Server Requirements**: You need servers that are running `vpn_server.py` specifically

## What You Need to Do

### Option 1: Deploy Your Own Servers (Recommended)

1. **Get a VPS/Server** in your target country:
   - **DigitalOcean**: https://www.digitalocean.com (from $5/month)
   - **AWS EC2**: https://aws.amazon.com/ec2 (free tier available)
   - **Linode**: https://www.linode.com (from $5/month)
   - **Vultr**: https://www.vultr.com (from $2.50/month)
   - **Hetzner**: https://www.hetzner.com (from €4/month)

2. **Deploy the VPN Server**:
   ```bash
   # SSH into your server
   ssh user@your_server_ip
   
   # Install Python and dependencies
   sudo apt update
   sudo apt install python3 python3-pip -y
   pip3 install cryptography
   
   # Upload vpn_server.py to the server
   # Then run:
   python3 vpn_server.py --host 0.0.0.0 --port 8888 --password your_password
   ```

3. **Update servers.json** with your server's IP address

### Option 2: Use VPN Gate (Different Protocol - Won't Work)

VPN Gate (by University of Tsukuba) offers free VPN servers, but they use OpenVPN/L2TP protocols, not our custom protocol. You would need to modify the application to support OpenVPN, which is a significant change.

**VPN Gate Website**: https://www.vpn gate.net/en/

### Option 3: Share Servers with Friends

If you have friends in different countries, you can:
1. Ask them to run `vpn_server.py` on their computers
2. Share the server IP addresses
3. Add them to your `servers.json`

## Free Trial Options

Some VPS providers offer free trials:
- **AWS Free Tier**: 12 months free (t2.micro instance)
- **Google Cloud**: $300 free credit
- **Azure**: $200 free credit
- **Oracle Cloud**: Always-free tier available

## Security Warning

⚠️ **Never use untrusted public VPN servers** - they could:
- Monitor your traffic
- Steal your data
- Inject malware
- Log your activities

Always use servers you trust or deploy your own!

## Example Server Configuration

Once you have a server running `vpn_server.py`, add it to `servers.json`:

```json
{
  "servers": {
    "israel": {
      "host": "123.45.67.89",
      "port": 8888,
      "country": "Israel",
      "description": "My Israel VPN server",
      "flag": "🇮🇱"
    }
  }
}
```

## Need Help?

- Check `SETUP_GUIDE.md` for detailed setup instructions
- Review `README.md` for usage information
- Ensure your server firewall allows port 8888

