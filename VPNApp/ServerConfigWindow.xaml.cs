using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VPNApp
{
    public partial class ServerConfigWindow : Window
    {
        private ObservableCollection<ServerConfigItem> _servers;
        private string _configFilePath;

        public ServerConfigWindow(string configFilePath)
        {
            InitializeComponent();
            _configFilePath = configFilePath;
            _servers = new ObservableCollection<ServerConfigItem>();
            ServersItemsControl.ItemsSource = _servers;
            LoadServers();
        }

        private void LoadServers()
        {
            try
            {
                if (File.Exists(_configFilePath))
                {
                    string json = File.ReadAllText(_configFilePath);
                    var config = JObject.Parse(json);
                    var serversObj = config["servers"] as JObject;

                    if (serversObj != null)
                    {
                        foreach (var server in serversObj)
                        {
                            var serverInfo = server.Value?.ToObject<ServerConfig>();
                            if (serverInfo != null)
                            {
                                _servers.Add(new ServerConfigItem
                                {
                                    Id = server.Key,
                                    Country = serverInfo.Country ?? server.Key,
                                    Host = serverInfo.Host ?? string.Empty,
                                    Port = serverInfo.Port > 0 ? serverInfo.Port : 8888,
                                    Description = serverInfo.Description ?? string.Empty,
                                    Flag = GetFlagForCountry(server.Key)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading servers: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetFlagForCountry(string countryId)
        {
            var flags = new Dictionary<string, string>
            {
                { "israel", "🇮🇱" }, { "italy", "🇮🇹" }, { "usa", "🇺🇸" }, { "uk", "🇬🇧" },
                { "germany", "🇩🇪" }, { "france", "🇫🇷" }, { "spain", "🇪🇸" }, { "netherlands", "🇳🇱" },
                { "switzerland", "🇨🇭" }, { "japan", "🇯🇵" }, { "singapore", "🇸🇬" }, { "canada", "🇨🇦" },
                { "australia", "🇦🇺" }, { "brazil", "🇧🇷" }, { "mexico", "🇲🇽" }
            };
            return flags.TryGetValue(countryId.ToLower(), out var flag) ? flag : "🌍";
        }

        private async void TestConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedServer = _servers.FirstOrDefault(s => !string.IsNullOrEmpty(s.Host) && 
                                                               !s.Host.Contains("your_") && 
                                                               !s.Host.Contains("YOUR_"));
            
            if (selectedServer == null)
            {
                MessageBox.Show("Please configure at least one server with a valid IP address to test.", 
                    "No Server to Test", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            TestConnectionButton.IsEnabled = false;
            TestConnectionButton.Content = "Testing...";

            try
            {
                bool isReachable = await TestServerConnection(selectedServer.Host, selectedServer.Port);
                
                if (isReachable)
                {
                    MessageBox.Show($"✓ Connection successful!\n\nServer: {selectedServer.Host}:{selectedServer.Port}\nCountry: {selectedServer.Country}", 
                        "Connection Test", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show($"✗ Connection failed!\n\nServer: {selectedServer.Host}:{selectedServer.Port}\n\nPossible reasons:\n• Server is not running\n• Firewall blocking port {selectedServer.Port}\n• Incorrect IP address", 
                        "Connection Test", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error testing connection: {ex.Message}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                TestConnectionButton.IsEnabled = true;
                TestConnectionButton.Content = "🔍 Test Selected";
            }
        }

        private async Task<bool> TestServerConnection(string host, int port)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (var ping = new Ping())
                    {
                        var reply = ping.Send(host, 3000);
                        if (reply?.Status == IPStatus.Success)
                        {
                            // Try to connect to the port
                            using (var client = new System.Net.Sockets.TcpClient())
                            {
                                var result = client.BeginConnect(host, port, null, null);
                                var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(3));
                                if (success)
                                {
                                    client.EndConnect(result);
                                    return true;
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // Connection failed
                }
                return false;
            });
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Read existing config to preserve structure
                JObject config;
                if (File.Exists(_configFilePath))
                {
                    string json = File.ReadAllText(_configFilePath);
                    config = JObject.Parse(json);
                }
                else
                {
                    config = new JObject();
                }

                // Update servers
                var serversObj = new JObject();
                foreach (var server in _servers)
                {
                    var serverObj = new JObject
                    {
                        ["host"] = server.Host,
                        ["port"] = server.Port,
                        ["country"] = server.Country,
                        ["description"] = server.Description
                    };
                    serversObj[server.Id] = serverObj;
                }

                config["servers"] = serversObj;

                // Save to file
                File.WriteAllText(_configFilePath, config.ToString(Formatting.Indented));

                MessageBox.Show($"✓ Server configuration saved successfully!\n\nFile: {_configFilePath}\n\nPlease restart the application or reload servers to see changes.", 
                    "Configuration Saved", MessageBoxButton.OK, MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving configuration: {ex.Message}\n\n{ex.StackTrace}", 
                    "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }

    public class ServerConfigItem : INotifyPropertyChanged
    {
        private string _host = string.Empty;
        private int _port = 8888;

        public string Id { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Flag { get; set; } = "🌍";
        public string Description { get; set; } = string.Empty;

        public string Host
        {
            get => _host;
            set
            {
                _host = value;
                OnPropertyChanged(nameof(Host));
                OnPropertyChanged(nameof(StatusText));
                OnPropertyChanged(nameof(StatusColor));
            }
        }

        public int Port
        {
            get => _port;
            set
            {
                _port = value;
                OnPropertyChanged(nameof(Port));
            }
        }

        public string StatusText
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Host) || Host.Contains("your_") || Host.Contains("YOUR_"))
                    return "Not Configured";
                return "Configured";
            }
        }

        public string StatusColor
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Host) || Host.Contains("your_") || Host.Contains("YOUR_"))
                    return "#E74C3C"; // Red
                return "#27AE60"; // Green
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

