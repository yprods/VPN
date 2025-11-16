# Free VPS Setup Guide - Get Free VPN Servers

## ⚠️ Important Security Note

**There are NO safe free public VPN servers** that work with our custom protocol. Free VPN services (ProtonVPN, Windscribe, etc.) use different protocols and **WILL NOT WORK**.

**The ONLY safe way** is to deploy your own servers using free VPS trials.

## 🆓 Best Free Options (Ranked)

### 1. Oracle Cloud Always-Free Tier ⭐ RECOMMENDED

**Why it's the best:**
- ✅ **Truly FREE forever** (no credit card required for basic tier)
- ✅ Multiple regions available (USA, UK, Germany, Netherlands, Japan, Singapore, etc.)
- ✅ 2 AMD-based VMs or 4 ARM-based VMs
- ✅ 10TB data transfer per month
- ✅ No time limit

**Setup:**
1. Sign up: https://www.oracle.com/cloud/free
2. Create VM instance in your target country
3. Follow deployment steps below

**Limitations:**
- Limited CPU/RAM (but enough for VPN)
- May require credit card for some regions (but won't be charged on free tier)

---

### 2. AWS Free Tier

**Why it's good:**
- ✅ 12 months completely free
- ✅ t2.micro instance (1 vCPU, 1GB RAM)
- ✅ 750 hours/month
- ✅ Multiple regions

**Setup:**
1. Sign up: https://aws.amazon.com/free
2. Launch EC2 instance (choose t2.micro)
3. Follow deployment steps below

**Limitations:**
- Only 12 months free
- Requires credit card (won't be charged if you stay in free tier)

---

### 3. Google Cloud Free Tier

**Why it's good:**
- ✅ $300 free credit (valid 90 days)
- ✅ Enough for several months of small VMs
- ✅ Multiple regions

**Setup:**
1. Sign up: https://cloud.google.com/free
2. Create VM instance
3. Follow deployment steps below

**Limitations:**
- Credit expires after 90 days
- Requires credit card

---

### 4. Microsoft Azure Free Account

**Why it's good:**
- ✅ $200 free credit (30 days)
- ✅ Always-free services after credit expires
- ✅ Multiple regions

**Setup:**
1. Sign up: https://azure.microsoft.com/free
2. Create virtual machine
3. Follow deployment steps below

**Limitations:**
- Credit expires after 30 days
- Requires credit card

---

## 🚀 Quick Deployment Steps

Once you have a VPS, follow these steps:

### Step 1: Connect to Your Server

```bash
ssh username@your_server_ip
```

### Step 2: Install Python and Dependencies

```bash
# Update system
sudo apt update
sudo apt upgrade -y

# Install Python
sudo apt install python3 python3-pip -y

# Install cryptography library
pip3 install cryptography
```

### Step 3: Upload VPN Server Files

```bash
# Create directory
mkdir ~/vpn
cd ~/vpn

# Upload vpn_server.py and crypto_utils.py using SCP or SFTP
# Or clone from your repository
```

### Step 4: Configure Firewall

```bash
# Allow port 8888
sudo ufw allow 8888/tcp
sudo ufw enable
```

### Step 5: Start VPN Server

```bash
# Run in background using screen or tmux
screen -S vpn
python3 vpn_server.py --host 0.0.0.0 --port 8888 --password your_secure_password

# Press Ctrl+A then D to detach
```

### Step 6: Add to Your App

1. Open the VPN app
2. Click "⚙️ Configure" button
3. Enter your server's IP address for the country
4. Save changes

---

## 📋 Server Requirements

**Minimum:**
- 1 vCPU
- 512MB RAM
- 10GB storage
- Ubuntu 20.04+ or Debian 11+

**Recommended:**
- 1 vCPU
- 1GB RAM
- 20GB storage

---

## 🔒 Security Best Practices

1. **Use Strong Passwords**: At least 16 characters, mix of letters, numbers, symbols
2. **Keep Server Updated**: `sudo apt update && sudo apt upgrade`
3. **Use SSH Keys**: Disable password authentication, use SSH keys only
4. **Firewall Rules**: Only allow necessary ports (22 for SSH, 8888 for VPN)
5. **Monitor Logs**: Check server logs regularly for suspicious activity

---

## 💡 Tips

- **Start with Oracle Cloud**: It's the only truly free forever option
- **Test Locally First**: Run the server on your local machine to test
- **Use Screen/Tmux**: Keeps server running after SSH disconnect
- **Set Up Auto-Start**: Configure server to start on boot
- **Monitor Usage**: Free tiers have limits, monitor your usage

---

## ❓ Troubleshooting

**Server won't start:**
- Check Python version: `python3 --version` (need 3.7+)
- Check if port 8888 is available: `sudo netstat -tulpn | grep 8888`
- Check firewall: `sudo ufw status`

**Can't connect from app:**
- Verify server is running: `ps aux | grep vpn_server`
- Check firewall allows port 8888
- Verify IP address is correct
- Test connection: `telnet server_ip 8888`

**High CPU/Memory usage:**
- Free tier VMs are limited, this is normal
- Consider upgrading if you need better performance

---

## 📚 Additional Resources

- Oracle Cloud Documentation: https://docs.oracle.com/en-us/iaas/
- AWS EC2 Documentation: https://docs.aws.amazon.com/ec2/
- Ubuntu Server Guide: https://ubuntu.com/server/docs

---

**Remember**: There are no shortcuts to safe, free VPN servers. You MUST deploy your own using free VPS trials. This is the only way to ensure security and compatibility with our custom VPN protocol.

