# Building and Running the VPN WPF Application

## Prerequisites

1. **Install .NET 8.0 SDK**
   - Download from: https://dotnet.microsoft.com/download/dotnet/8.0
   - Verify installation:
     ```bash
     dotnet --version
     ```
     Should show version 8.0.x or higher

2. **Install Python 3.7+**
   - Download from: https://www.python.org/downloads/
   - Make sure to check "Add Python to PATH" during installation
   - Verify installation:
     ```bash
     python --version
     ```

3. **Ensure Python VPN scripts are in place**
   - `socks5_proxy.py` should be in the project root directory
   - `servers.json` should be in the project root directory

## Building the Application

### Option 1: Using Command Line (Recommended)

1. **Navigate to the project directory:**
   ```bash
   cd VPNApp
   ```

2. **Restore NuGet packages:**
   ```bash
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

### Option 2: Using Visual Studio

1. **Open the solution:**
   - Double-click `VPNApp.sln` or open it in Visual Studio 2022

2. **Restore packages:**
   - Visual Studio will automatically restore NuGet packages
   - Or right-click solution → "Restore NuGet Packages"

3. **Build:**
   - Press `Ctrl+Shift+B` or Build → Build Solution

4. **Run:**
   - Press `F5` to run with debugging
   - Or `Ctrl+F5` to run without debugging

### Option 3: Build Release Version

1. **Build release:**
   ```bash
   dotnet build -c Release
   ```

2. **Output location:**
   - Executable will be in: `VPNApp/bin/Release/net8.0-windows/VPNApp.exe`

3. **Run the executable:**
   - Double-click `VPNApp.exe` or run from command line

## Configuration Before Running

1. **Edit `servers.json`** (in project root, same level as VPNApp folder):
   ```json
   {
     "servers": {
       "israel": {
         "host": "your_actual_israel_server_ip",
         "port": 8888,
         "country": "Israel",
         "description": "Israel VPN server"
       },
       "italy": {
         "host": "your_actual_italy_server_ip",
         "port": 8888,
         "country": "Italy",
         "description": "Italy VPN server"
       }
     }
   }
   ```

2. **Ensure Python scripts are accessible:**
   - The app looks for `socks5_proxy.py` in the parent directory
   - If scripts are elsewhere, modify the path in `MainWindow.xaml.cs` (line ~150)

## Troubleshooting Build Issues

### "Newtonsoft.Json not found"
```bash
cd VPNApp
dotnet add package Newtonsoft.Json
```

### "Target framework not found"
- Install .NET 8.0 SDK
- Or change target framework in `VPNApp.csproj` to a version you have installed

### "Python scripts not found"
- Ensure `socks5_proxy.py` is in the project root directory
- Or modify the script path in `MainWindow.xaml.cs`

### "servers.json not found"
- Create `servers.json` in the project root (same level as VPNApp folder)
- See example in main README

## Running the Application

1. **Start the app:**
   - Run `dotnet run` from VPNApp directory
   - Or double-click the built executable

2. **Select a country** from the dropdown

3. **Enter password** (if your VPN server requires one)

4. **Click "Connect"**

5. **Wait for connection** - Status will show "Connected" when ready

6. **Check your IP** - Click "Check IP" button to verify location change

7. **Configure browser** to use SOCKS5 proxy:
   - Server: `127.0.0.1`
   - Port: `1080`

## Distribution

To distribute the application:

1. **Build release version:**
   ```bash
   dotnet publish -c Release -r win-x64 --self-contained false
   ```

2. **Output folder:**
   - `VPNApp/bin/Release/net8.0-windows/publish/`
   - Contains all necessary files

3. **Requirements for end users:**
   - .NET 8.0 Runtime (downloadable from Microsoft)
   - Python 3.7+ installed
   - Python VPN scripts and servers.json

## Quick Test

After building, test the application:

1. Make sure you have a VPN server running
2. Configure `servers.json` with the server IP
3. Run the app
4. Select a country and connect
5. Check IP to verify it changed

## Notes

- The app automatically finds Python in common installation locations
- If Python is not found, you'll see an error message
- The app creates a local SOCKS5 proxy on port 1080
- Make sure port 1080 is not in use by another application

