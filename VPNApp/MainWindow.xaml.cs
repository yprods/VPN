using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VPNApp
{
    public partial class MainWindow : Window
    {
        private VPNService _vpnService = new VPNService();
        private Process? _proxyProcess;
        private List<ServerInfo> _servers = new List<ServerInfo>();
        private bool _isConnected = false;

        public MainWindow()
        {
            try
            {
                InitializeComponent();
                LoadServers();
                UpdateUI();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error initializing application:\n\n{ex.Message}\n\n{ex.StackTrace}",
                    "Initialization Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                // Try to show window anyway
                try
                {
                    InitializeComponent();
                }
                catch
                {
                    // If we can't even show the window, close the app
                    Application.Current.Shutdown();
                }
            }
        }

        private void LoadServers()
        {
            try
            {
                // Try multiple paths to find servers.json
                string[] possiblePaths = {
                    Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "servers.json"), // From bin/Debug/net8.0-windows
                    Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "servers.json"), // From bin/Debug
                    Path.Combine(Directory.GetCurrentDirectory(), "..", "servers.json"), // From bin
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "servers.json"), // Alternative base path
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "servers.json"), // In output directory
                    "servers.json" // Current directory
                };

                string configPath = string.Empty;
                foreach (var path in possiblePaths)
                {
                    if (File.Exists(path))
                    {
                        configPath = path;
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(configPath) && File.Exists(configPath))
                {
                    string json = File.ReadAllText(configPath);
                    var config = JObject.Parse(json);
                    var serversObj = config["servers"] as JObject;

                    _servers = new List<ServerInfo>();
                    if (serversObj != null)
                    {
                        foreach (var server in serversObj)
                        {
                            var serverInfo = server.Value?.ToObject<ServerConfig>();
                            if (serverInfo != null && !string.IsNullOrEmpty(serverInfo.Host))
                            {
                                // Check if server is configured (has real IP, not placeholder)
                                bool isConfigured = !serverInfo.Host.Contains("your_") && 
                                                   !serverInfo.Host.Contains("YOUR_") &&
                                                   !serverInfo.Host.StartsWith("your") &&
                                                   serverInfo.Host != "localhost" &&
                                                   !string.IsNullOrWhiteSpace(serverInfo.Host);
                                
                                _servers.Add(new ServerInfo
                                {
                                    Id = server.Key,
                                    Country = serverInfo.Country ?? server.Key,
                                    Host = serverInfo.Host,
                                    Port = serverInfo.Port,
                                    Description = isConfigured 
                                        ? (serverInfo.Description ?? string.Empty)
                                        : $"{serverInfo.Description ?? string.Empty} (Not Configured - Please set server IP)"
                                });
                            }
                        }
                    }

                    CountryComboBox.ItemsSource = _servers;
                    AddLog($"Loaded {_servers.Count} server(s) from configuration.");
                    AddLog($"Configuration file: {configPath}");
                }
                else
                {
                    AddLog("ERROR: servers.json not found. Please create it with your server configurations.");
                    AddLog("Searched in:");
                    string[] searchPaths = {
                        Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "servers.json"),
                        Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "servers.json"),
                        Path.Combine(Directory.GetCurrentDirectory(), "..", "servers.json"),
                        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "servers.json"),
                        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "servers.json"),
                        "servers.json"
                    };
                    foreach (var path in searchPaths)
                    {
                        AddLog($"  - {Path.GetFullPath(path)}");
                    }
                    MessageBox.Show("servers.json not found!\n\nPlease create servers.json in the project directory with your server configurations.", 
                        "Configuration Missing", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                AddLog($"Error loading servers: {ex.Message}");
                MessageBox.Show($"Error loading server configuration: {ex.Message}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CountryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateUI();
            if (CountryComboBox.SelectedItem is ServerInfo server)
            {
                AddLog($"Selected country: {server.Country} ({server.Host}:{server.Port})");
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            UpdateUI();
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (CountryComboBox.SelectedItem is ServerInfo server)
            {
                ConnectToVPN(server);
            }
        }

        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            DisconnectVPN();
        }

        private async void CheckIPButton_Click(object sender, RoutedEventArgs e)
        {
            await CheckIPAddress();
        }

        private void ConfigureServersButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Find servers.json path
                string[] possiblePaths = {
                    Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "servers.json"),
                    Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "servers.json"),
                    Path.Combine(Directory.GetCurrentDirectory(), "..", "servers.json"),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "servers.json"),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "servers.json"),
                    "servers.json"
                };

                string configPath = string.Empty;
                foreach (var path in possiblePaths)
                {
                    if (File.Exists(path))
                    {
                        configPath = path;
                        break;
                    }
                }

                if (string.IsNullOrEmpty(configPath))
                {
                    // Create default servers.json if it doesn't exist
                    configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "servers.json");
                    if (!File.Exists(configPath))
                    {
                        var defaultConfig = new JObject
                        {
                            ["servers"] = new JObject(),
                            ["settings"] = new JObject
                            {
                                ["default_port"] = 8888,
                                ["proxy_port"] = 1080
                            }
                        };
                        File.WriteAllText(configPath, defaultConfig.ToString(Formatting.Indented));
                    }
                }

                var configWindow = new ServerConfigWindow(configPath)
                {
                    Owner = this
                };

                if (configWindow.ShowDialog() == true)
                {
                    // Reload servers after configuration is saved
                    AddLog("Server configuration updated. Reloading servers...");
                    LoadServers();
                    AddLog("Servers reloaded successfully.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening server configuration: {ex.Message}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                AddLog($"Error opening configuration: {ex.Message}");
            }
        }

        private void PayPalDonateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string paypalUrl = GetPayPalUrl();
                
                if (string.IsNullOrEmpty(paypalUrl) || paypalUrl.Contains("YOUR_BUTTON_ID") || paypalUrl.Contains("yourusername"))
                {
                    MessageBox.Show(
                        "PayPal donation link is not configured.\n\n" +
                        "Please edit donation-config.json or contact the developer.\n\n" +
                        "To set up:\n" +
                        "1. Visit https://www.paypal.com/buttons\n" +
                        "2. Create a 'Donate' button\n" +
                        "3. Update the URL in donation-config.json",
                        "Donation Not Configured",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    return;
                }
                
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = paypalUrl,
                    UseShellExecute = true
                });
                
                AddLog("Opening PayPal donation page...");
            }
            catch (Exception ex)
            {
                AddLog($"Error opening PayPal: {ex.Message}");
                MessageBox.Show(
                    "Could not open PayPal donation page.\n\n" +
                    "Please visit: https://www.paypal.com/donate\n\n" +
                    "Or contact the developer for donation information.",
                    "Donation",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private string GetPayPalUrl()
        {
            try
            {
                // Try to load from donation-config.json
                string[] possiblePaths = {
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "donation-config.json"),
                    Path.Combine(Directory.GetCurrentDirectory(), "donation-config.json"),
                    "donation-config.json"
                };

                foreach (var path in possiblePaths)
                {
                    if (File.Exists(path))
                    {
                        string json = File.ReadAllText(path);
                        var config = JObject.Parse(json);
                        var paypalObj = config["paypal"] as JObject;
                        
                        if (paypalObj != null)
                        {
                            bool enabled = paypalObj["enabled"]?.Value<bool>() ?? true;
                            if (!enabled) return string.Empty;
                            
                            string? url = paypalObj["url"]?.ToString();
                            if (!string.IsNullOrEmpty(url) && !url.Contains("YOUR_BUTTON_ID"))
                            {
                                return url;
                            }
                            
                            // Try alternative URL
                            string? altUrl = paypalObj["alternative_url"]?.ToString();
                            if (!string.IsNullOrEmpty(altUrl) && !altUrl.Contains("yourusername"))
                            {
                                return altUrl;
                            }
                        }
                        break;
                    }
                }
            }
            catch
            {
                // If config file doesn't exist or has errors, use default
            }
            
            // Default fallback (user should configure this)
            return "https://www.paypal.com/donate/?hosted_button_id=YOUR_BUTTON_ID";
        }

        private async void ConnectToVPN(ServerInfo server)
        {
            try
            {
                // Check if server is configured
                bool isConfigured = !server.Host.Contains("your_") && 
                                   !server.Host.Contains("YOUR_") &&
                                   !server.Host.StartsWith("your") &&
                                   server.Host != "localhost" &&
                                   !string.IsNullOrWhiteSpace(server.Host);

                if (!isConfigured)
                {
                    MessageBox.Show(
                        $"Server for {server.Country} is not configured!\n\n" +
                        $"Please edit servers.json and set a valid IP address for the '{server.Id}' server.\n\n" +
                        $"Current host: {server.Host}",
                        "Server Not Configured", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Warning);
                    return;
                }

                AddLog($"Connecting to {server.Country} VPN server...");
                StatusText.Text = "Connecting...";
                StatusText.Foreground = System.Windows.Media.Brushes.Orange;
                StatusDetails.Text = $"Connecting to {server.Host}:{server.Port}";
                ConnectButton.IsEnabled = false;

                string password = PasswordBox.Password;
                string? pythonPath = FindPythonPath();

                if (string.IsNullOrEmpty(pythonPath))
                {
                    AddLog("ERROR: Python not found! Please install Python 3.7+ and add it to PATH.");
                    MessageBox.Show("Python not found!\n\nPlease install Python 3.7 or higher and ensure it's in your system PATH.", 
                        "Python Not Found", MessageBoxButton.OK, MessageBoxImage.Error);
                    StatusText.Text = "Error";
                    StatusText.Foreground = System.Windows.Media.Brushes.Red;
                    ConnectButton.IsEnabled = true;
                    return;
                }

                // Start SOCKS5 proxy
                string scriptPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "socks5_proxy.py");
                if (!File.Exists(scriptPath))
                {
                    scriptPath = "socks5_proxy.py";
                }

                if (!File.Exists(scriptPath))
                {
                    AddLog("ERROR: socks5_proxy.py not found!");
                    MessageBox.Show("socks5_proxy.py not found!\n\nPlease ensure the Python VPN scripts are in the correct location.", 
                        "Script Not Found", MessageBoxButton.OK, MessageBoxImage.Error);
                    StatusText.Text = "Error";
                    StatusText.Foreground = System.Windows.Media.Brushes.Red;
                    ConnectButton.IsEnabled = true;
                    return;
                }

                string arguments = $@"""{scriptPath}"" --server {server.Host}:{server.Port} --port 1080";
                if (!string.IsNullOrEmpty(password))
                {
                    arguments += $" --password \"{password}\"";
                }

                _proxyProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = pythonPath,
                        Arguments = arguments,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };

                _proxyProcess.OutputDataReceived += (s, args) =>
                {
                    if (!string.IsNullOrEmpty(args.Data))
                    {
                        Dispatcher.Invoke(() => AddLog($"[PROXY] {args.Data}"));
                    }
                };

                _proxyProcess.ErrorDataReceived += (s, args) =>
                {
                    if (!string.IsNullOrEmpty(args.Data))
                    {
                        Dispatcher.Invoke(() => AddLog($"[ERROR] {args.Data}"));
                    }
                };

                _proxyProcess.Start();
                _proxyProcess.BeginOutputReadLine();
                _proxyProcess.BeginErrorReadLine();

                // Wait a bit for connection
                await Task.Delay(2000);

                if (!_proxyProcess.HasExited)
                {
                    _isConnected = true;
                    StatusText.Text = "Connected";
                    StatusText.Foreground = System.Windows.Media.Brushes.Green;
                    StatusDetails.Text = $"Connected to {server.Country} ({server.Host})";
                    AddLog($"✓ Successfully connected to {server.Country} VPN!");
                    AddLog($"Your IP will now appear as if you're in {server.Country}");
                    AddLog($"SOCKS5 Proxy: 127.0.0.1:1080");
                    AddLog($"Configure your browser/applications to use this proxy.");

                    // Check IP after connection
                    await Task.Delay(1000);
                    await CheckIPAddress();
                }
                else
                {
                    AddLog("ERROR: Proxy process exited unexpectedly");
                    StatusText.Text = "Connection Failed";
                    StatusText.Foreground = System.Windows.Media.Brushes.Red;
                }

                UpdateUI();
            }
            catch (Exception ex)
            {
                AddLog($"ERROR: {ex.Message}");
                StatusText.Text = "Error";
                StatusText.Foreground = System.Windows.Media.Brushes.Red;
                StatusDetails.Text = ex.Message;
                UpdateUI();
            }
        }

        private void DisconnectVPN()
        {
            try
            {
                if (_proxyProcess != null && !_proxyProcess.HasExited)
                {
                    try
                    {
                        _proxyProcess.Kill();
                    }
                    catch { }
                    _proxyProcess.Dispose();
                    _proxyProcess = null;
                }

                _isConnected = false;
                StatusText.Text = "Disconnected";
                StatusText.Foreground = System.Windows.Media.Brushes.Red;
                StatusDetails.Text = "Not connected to any VPN server";
                AddLog("Disconnected from VPN");
                UpdateUI();
            }
            catch (Exception ex)
            {
                AddLog($"Error disconnecting: {ex.Message}");
            }
        }

        private async Task CheckIPAddress()
        {
            try
            {
                CurrentIPText.Text = "Checking...";
                AddLog("Checking your IP address...");

                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(10);
                    var response = await client.GetStringAsync("https://api.ipify.org?format=json");
                    var ipData = JObject.Parse(response);
                    string? ip = ipData["ip"]?.ToString();

                    if (!string.IsNullOrEmpty(ip))
                    {
                        CurrentIPText.Text = ip;
                        AddLog($"Your current IP: {ip}");

                        // Try to get location
                        try
                        {
                            var locationResponse = await client.GetStringAsync($"https://ipapi.co/{ip}/json/");
                            var location = JObject.Parse(locationResponse);
                            string? city = location["city"]?.ToString();
                            string? country = location["country_name"]?.ToString();
                            
                            if (!string.IsNullOrEmpty(city) && !string.IsNullOrEmpty(country))
                            {
                                AddLog($"Location: {city}, {country}");
                                CurrentIPText.Text = $"{ip} ({city}, {country})";
                            }
                        }
                        catch
                        {
                            // Location lookup failed, but we have IP
                        }
                    }
                    else
                    {
                        CurrentIPText.Text = "Could not determine IP";
                    }
                }
            }
            catch (Exception ex)
            {
                CurrentIPText.Text = "Error checking IP";
                AddLog($"Error checking IP: {ex.Message}");
            }
        }

        private void UpdateUI()
        {
            bool hasSelection = CountryComboBox.SelectedItem != null;
            ConnectButton.IsEnabled = hasSelection && !_isConnected;
            DisconnectButton.IsEnabled = _isConnected;
            CountryComboBox.IsEnabled = !_isConnected;
            PasswordBox.IsEnabled = !_isConnected;
        }

        private void AddLog(string message)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            LogTextBlock.Text += $"\n[{timestamp}] {message}";
            
            // Auto-scroll to bottom - find parent ScrollViewer
            var scrollViewer = FindParent<ScrollViewer>(LogTextBlock);
            if (scrollViewer != null)
            {
                scrollViewer.ScrollToEnd();
            }
        }

        private T? FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject? parentObject = VisualTreeHelper.GetParent(child);
            
            if (parentObject == null) return null;
            
            if (parentObject is T parent)
                return parent;
            else
                return FindParent<T>(parentObject);
        }

        private string? FindPythonPath()
        {
            string[] possiblePaths = {
                "python",
                "python3",
                "py",
                @"C:\Python39\python.exe",
                @"C:\Python310\python.exe",
                @"C:\Python311\python.exe",
                @"C:\Python312\python.exe",
                @"C:\Program Files\Python39\python.exe",
                @"C:\Program Files\Python310\python.exe",
                @"C:\Program Files\Python311\python.exe",
                @"C:\Program Files\Python312\python.exe"
            };

            foreach (var path in possiblePaths)
            {
                try
                {
                    var process = Process.Start(new ProcessStartInfo
                    {
                        FileName = path,
                        Arguments = "--version",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    });

                    if (process != null)
                    {
                        process.WaitForExit();
                        if (process.ExitCode == 0)
                        {
                            return path;
                        }
                        process.Dispose();
                    }
                }
                catch { }
            }

            return null;
        }

        protected override void OnClosed(EventArgs e)
        {
            DisconnectVPN();
            base.OnClosed(e);
        }
    }

    public class ServerInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class ServerConfig
    {
        [JsonProperty("host")]
        public string Host { get; set; } = string.Empty;

        [JsonProperty("port")]
        public int Port { get; set; }

        [JsonProperty("country")]
        public string? Country { get; set; }

        [JsonProperty("description")]
        public string? Description { get; set; }
    }

    public class VPNService
    {
        // Service class for future VPN operations
    }
}

