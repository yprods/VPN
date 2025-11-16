# VPN Location Changer - WPF Application

A beautiful Windows desktop application to easily change your IP address to appear from different countries.

## Features

- 🎨 **Modern WPF UI** - Clean and intuitive interface
- 🌍 **Country Selection** - Choose from configured VPN servers
- 🔒 **Secure Connection** - Password-protected VPN connections
- 📍 **IP Checking** - See your current IP and location
- 📊 **Real-time Status** - Connection status and logs
- 🔄 **Easy Switching** - Connect/disconnect with one click

## Requirements

- **.NET 8.0** or higher
- **Python 3.7+** installed and in PATH
- **Python VPN scripts** (socks5_proxy.py, servers.json) in the project directory

## Building the Application

1. **Install .NET SDK 8.0:**
   - Download from: https://dotnet.microsoft.com/download

2. **Restore NuGet packages:**
   ```bash
   cd VPNApp
   dotnet restore
   ```

3. **Build the application:**
   ```bash
   dotnet build
   ```

4. **Run the application:**
   ```bash
   dotnet run
   ```

   Or build a release:
   ```bash
   dotnet build -c Release
   ```

## Configuration

1. **Ensure `servers.json` exists** in the parent directory (same level as VPNApp folder)
2. **Configure your servers** in `servers.json`:
   ```json
   {
     "servers": {
       "israel": {
         "host": "your_israel_server_ip",
         "port": 8888,
         "country": "Israel",
         "description": "Israel VPN server"
       }
     }
   }
   ```

3. **Ensure Python scripts are accessible:**
   - `socks5_proxy.py` should be in the parent directory
   - Or modify the script path in `MainWindow.xaml.cs`

## Usage

1. **Launch the application**
2. **Select a country** from the dropdown
3. **Enter password** (if required by your VPN server)
4. **Click "Connect"** to establish VPN connection
5. **Check your IP** to verify the location change
6. **Configure your browser** to use SOCKS5 proxy:
   - Server: `127.0.0.1`
   - Port: `1080`

## Browser Configuration

### Chrome/Edge:
```bash
chrome.exe --proxy-server="socks5://127.0.0.1:1080"
```

### Firefox:
1. Settings → General → Network Settings → Settings
2. Manual proxy configuration
3. SOCKS Host: `127.0.0.1`, Port: `1080`
4. Select "SOCKS v5"

## Troubleshooting

**"Python not found" error:**
- Install Python 3.7+ from python.org
- Add Python to system PATH
- Or specify Python path in code

**"servers.json not found" error:**
- Create `servers.json` in the project root
- Ensure it contains valid server configurations

**Connection fails:**
- Verify VPN server is running
- Check firewall settings
- Verify server IP and port in configuration
- Check password is correct

**IP didn't change:**
- Ensure browser is using the proxy (127.0.0.1:1080)
- Check connection status in the app
- Verify proxy process is running

## Project Structure

```
VPNApp/
├── App.xaml              # Application resources
├── App.xaml.cs           # Application entry point
├── MainWindow.xaml       # Main UI layout
├── MainWindow.xaml.cs    # Main logic and event handlers
├── VPNApp.csproj        # Project file
└── README.md            # This file
```

## Dependencies

- **Newtonsoft.Json** - For JSON configuration parsing
- **WPF** - Windows Presentation Foundation (included in .NET)

## License

This project is provided as-is for educational purposes.

