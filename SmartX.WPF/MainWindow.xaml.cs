using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.Win32;

namespace SmartX.WPF
{
    public partial class MainWindow : Window
    {
        private readonly HttpClient _httpClient;
        private readonly DispatcherTimer _insightsTimer;
        private bool _isConnected = false;
        private const string GatewayUrl = "http://localhost:5044";

        public MainWindow()
        {
            InitializeComponent();
            _httpClient = new HttpClient { BaseAddress = new Uri(GatewayUrl), Timeout = TimeSpan.FromSeconds(10) };

            _insightsTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
            _insightsTimer.Tick += async (s, e) => await RefreshInsightsAsync();
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/telemetry/statistics");
                if (response.IsSuccessStatusCode)
                {
                    _isConnected = true;
                    StatusIndicator.Fill = System.Windows.Media.Brushes.LimeGreen;
                    StatusText.Text = "Connected";
                    _insightsTimer.Start();
                    await RefreshDataAsync();
                    MessageBox.Show("Connected to Gateway!", "Success");
                }
            }
            catch
            {
                MessageBox.Show("Failed to connect to Gateway.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void GenerateDataButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isConnected) return;
            var random = new Random();

            for (int i = 0; i < 3; i++)
            {
                var data = new TelemetryData
                {
                    DeviceId = $"ESP32-{random.Next(1, 5)}",
                    SensorType = "SoilMoisture",
                    Value = random.Next(10, 60),
                    Unit = "%",
                    Timestamp = DateTime.UtcNow,
                    IsConnected = true
                };
                await _httpClient.PostAsJsonAsync("/api/telemetry", data);
            }
            await RefreshDataAsync();
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e) => await RefreshDataAsync();

        private async void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            await _httpClient.DeleteAsync("/api/telemetry/clear");
            TelemetryGrid.ItemsSource = null;
            InsightsListBox.ItemsSource = null;
        }

        private async Task RefreshDataAsync()
        {
            if (!_isConnected) return;
            try
            {
                var json = await _httpClient.GetStringAsync("/api/telemetry");
                var data = JsonSerializer.Deserialize<List<TelemetryData>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                TelemetryGrid.ItemsSource = data?.OrderByDescending(t => t.Timestamp).Take(100).ToList();
            }
            catch { }
        }

        private async Task RefreshInsightsAsync()
        {
            if (!_isConnected) return;
            try
            {
                var json = await _httpClient.GetStringAsync("/api/telemetry/insights");
                var insights = JsonSerializer.Deserialize<List<string>>(json);
                InsightsListBox.ItemsSource = insights;
            }
            catch { }
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isConnected) return;
            try
            {
                await _httpClient.PostAsJsonAsync("/api/telemetry/command", new
                {
                    deviceMacAddress = "SYSTEM",
                    action = $"SEARCHED: {SearchBox.Text}"
                });
                await RefreshInsightsAsync();
            }
            catch { }
        }

        private async void CommandPump_Click(object sender, RoutedEventArgs e)
        {
            if (!_isConnected) return;
            await _httpClient.PostAsJsonAsync("/api/telemetry/command", new
            {
                deviceMacAddress = "ESP32-01",
                action = "Turn On Pump"
            });
            await RefreshInsightsAsync();
            MessageBox.Show("Pump command issued!");
        }

        private async void CommandRelay_Click(object sender, RoutedEventArgs e)
        {
            if (!_isConnected) return;
            await _httpClient.PostAsJsonAsync("/api/telemetry/command", new
            {
                deviceMacAddress = "ESP32-02",
                action = "Toggle Relay"
            });
            await RefreshInsightsAsync();
            MessageBox.Show("Relay toggled!");
        }

        private async void UndoCommand_Click(object sender, RoutedEventArgs e)
        {
            if (!_isConnected) return;
            try
            {
                var response = await _httpClient.PostAsync("/api/telemetry/undo", null);
                if (response.IsSuccessStatusCode)
                    MessageBox.Show("Last command undone successfully.");
                else
                    MessageBox.Show("No commands to undo.");
            }
            catch { }
        }

        private async void UploadDevices_Click(object sender, RoutedEventArgs e)
        {
            if (!_isConnected) return;

            var dialog = new OpenFileDialog { Filter = "Text Files (*.txt)|*.txt" };
            if (dialog.ShowDialog() == true)
            {
                using var content = new MultipartFormDataContent();
                var fileStream = File.OpenRead(dialog.FileName);
                content.Add(new StreamContent(fileStream), "file", Path.GetFileName(dialog.FileName));

                var response = await _httpClient.PostAsync("/api/telemetry/upload-devices", content);
                if (response.IsSuccessStatusCode)
                {
                    var devicesJson = await _httpClient.GetStringAsync("/api/telemetry/devices");
                    var devices = JsonSerializer.Deserialize<List<Device>>(devicesJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    DeviceGrid.ItemsSource = null;
                    DeviceGrid.ItemsSource = devices;

                    MessageBox.Show("Devices loaded successfully!", "Success");
                }
                else
                {
                    MessageBox.Show("Upload failed on the server side.", "Error");
                }
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }

        

        private async void LoadTopology_Click(object sender, RoutedEventArgs e)
        {
            if (!_isConnected) return;
            try
            {
                var json = await _httpClient.GetStringAsync("/api/telemetry/topology");
                var nodes = JsonSerializer.Deserialize<List<MeshNodeDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                var displayNodes = nodes.Select(n => new {
                    n.MacAddress,
                    IsOnline = n.IsOnline ? "Online" : "Offline",
                    EdgeCount = n.Edges.Count
                }).ToList();

                TopologyGrid.ItemsSource = displayNodes;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading topology: {ex.Message}");
            }
        }

        private async void CalculateMST_Click(object sender, RoutedEventArgs e)
        {
            if (!_isConnected) return;
            try
            {
                var json = await _httpClient.GetStringAsync("/api/telemetry/mst");
                var mstEdges = JsonSerializer.Deserialize<List<MeshEdgeDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                MSTGrid.ItemsSource = mstEdges;
                MessageBox.Show($"Calculated MST with {mstEdges.Count} optimized routing paths.", "Prim's Algorithm Success");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error calculating MST: {ex.Message}");
            }
        }

        private async void QueryHistory_Click(object sender, RoutedEventArgs e)
        {
            if (!_isConnected) return;
            try
            {
                
                var start = DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-ddTHH:mm:ss");
                var end = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss");

                var json = await _httpClient.GetStringAsync($"/api/telemetry/history/query?start={start}&end={end}");
                var history = JsonSerializer.Deserialize<List<TelemetryData>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                HistoryGrid.ItemsSource = history;
                MessageBox.Show($"Retrieved {history.Count} records from the Binary Search Tree.", "BST Query Success");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error querying BST: {ex.Message}");
            }
        }
    }

    
    public class TelemetryData
    {
        public int Id { get; set; }
        public string DeviceId { get; set; } = string.Empty;
        public string SensorType { get; set; } = string.Empty;
        public double Value { get; set; }
        public string Unit { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public bool IsConnected { get; set; }
    }

    public class Device
    {
        public string MacAddress { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }


    public class MeshNodeDto
    {
        public string MacAddress { get; set; } = string.Empty;
        public bool IsOnline { get; set; }
        public List<MeshEdgeDto> Edges { get; set; } = new();
    }

    public class MeshEdgeDto
    {
        public string SourceMac { get; set; } = string.Empty;
        public string TargetMac { get; set; } = string.Empty;
        public int Weight { get; set; }
    }
}