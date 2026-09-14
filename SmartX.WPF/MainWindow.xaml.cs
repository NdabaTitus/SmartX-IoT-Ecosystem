using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.Win32;
using SmartX.WPF.Models;

namespace SmartX.WPF
{
    public partial class MainWindow : Window
    {
        private readonly HttpClient _httpClient;
        private readonly DispatcherTimer _refreshTimer;
        private readonly Random _random = new();
        private bool _isConnected = false;

        private const string GatewayUrl = "http://localhost:5044";

        private readonly string[] _deviceIds = {
            "ESP32-FARM-01", "ESP32-FARM-02",
            "ESP32-GRID-01", "ESP32-GRID-02",
            "ESP32-BUILD-01"
        };

        private readonly string[] _sensorTypes = {
            "SoilMoisture", "PowerWattage", "ValveState",
            "Temperature", "Humidity"
        };

        private readonly string[] _units = { "%", "W", "bool", "°C", "%" };

        public MainWindow()
        {
            InitializeComponent();

            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(GatewayUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(10);

            _refreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)
            };
            _refreshTimer.Tick += async (s, e) => await RefreshDataAsync();
        }

        // ============================================================
        // Connection
        // ============================================================
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
                    StatusText.Foreground = System.Windows.Media.Brushes.LimeGreen;

                    _refreshTimer.Start();
                    await RefreshDataAsync();

                    MessageBox.Show("Successfully connected to Smart-X Gateway!",
                        "Connection Status", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    throw new Exception($"Server returned {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _isConnected = false;
                StatusIndicator.Fill = System.Windows.Media.Brushes.Red;
                StatusText.Text = "Disconnected";
                StatusText.Foreground = System.Windows.Media.Brushes.Red;

                MessageBox.Show(
                    $"Failed to connect to gateway.\n\nMake sure the Gateway is running at {GatewayUrl}\n\nError: {ex.Message}",
                    "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ============================================================
        // Generate Simulated Test Data
        // ============================================================
        private async void GenerateDataButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isConnected)
            {
                MessageBox.Show("Please connect to the gateway first!",
                    "Not Connected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                int batchSize = _random.Next(5, 15);

                for (int i = 0; i < batchSize; i++)
                {
                    int sensorIndex = _random.Next(_sensorTypes.Length);

                    var telemetry = new TelemetryData
                    {
                        DeviceId = _deviceIds[_random.Next(_deviceIds.Length)],
                        SensorType = _sensorTypes[sensorIndex],
                        Value = GenerateRealisticValue(sensorIndex),
                        Unit = _units[sensorIndex],
                        Timestamp = DateTime.UtcNow,
                        IsConnected = _random.Next(100) > 10
                    };

                    await _httpClient.PostAsJsonAsync("/api/telemetry", telemetry);
                }

                await RefreshDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending data: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private double GenerateRealisticValue(int sensorIndex)
        {
            return sensorIndex switch
            {
                0 => Math.Round(_random.NextDouble() * 100, 1),
                1 => Math.Round(_random.NextDouble() * 5000, 2),
                2 => _random.Next(2),
                3 => Math.Round(15 + _random.NextDouble() * 20, 1),
                4 => Math.Round(30 + _random.NextDouble() * 60, 1),
                _ => _random.NextDouble() * 100
            };
        }

        // ============================================================
        // Refresh Data
        // ============================================================
        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await RefreshDataAsync();
        }

        private async Task RefreshDataAsync()
        {
            if (!_isConnected) return;

            try
            {
                var telemetryResponse = await _httpClient.GetAsync("/api/telemetry");
                if (telemetryResponse.IsSuccessStatusCode)
                {
                    var json = await telemetryResponse.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var data = JsonSerializer.Deserialize<List<TelemetryData>>(json, options);

                    if (data != null)
                    {
                        TelemetryGrid.ItemsSource = data
                            .OrderByDescending(t => t.Timestamp)
                            .Take(100)
                            .ToList();
                    }
                }

                var statsResponse = await _httpClient.GetAsync("/api/telemetry/statistics");
                if (statsResponse.IsSuccessStatusCode)
                {
                    var json = await statsResponse.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var stats = JsonSerializer.Deserialize<TelemetryStatistics>(json, options);

                    if (stats != null)
                    {
                        TotalReadingsText.Text = stats.TotalReadings.ToString();
                        ActiveDevicesText.Text = stats.UniqueDevices.ToString();
                        AverageValueText.Text = stats.AverageValue.ToString("F1");
                        DisconnectedText.Text = stats.DisconnectedCount.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Refresh error: {ex.Message}");
            }
        }

        // ============================================================
        // Clear Data
        // ============================================================
        private async void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isConnected)
            {
                MessageBox.Show("Please connect to the gateway first!",
                    "Not Connected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                "Are you sure you want to clear all telemetry data?",
                "Confirm Clear", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _httpClient.DeleteAsync("/api/telemetry/clear");
                    TelemetryGrid.ItemsSource = null;
                    TotalReadingsText.Text = "0";
                    ActiveDevicesText.Text = "0";
                    AverageValueText.Text = "0.0";
                    DisconnectedText.Text = "0";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error clearing data: {ex.Message}",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // ============================================================
        // Attach File (OpenFileDialog + Multipart Upload)
        // ============================================================
        private async void AttachFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Select a configuration file, photo, or hardware log",
                Filter = "All Files (*.*)|*.*|" +
                         "Config Files (*.json;*.xml;*.yaml)|*.json;*.xml;*.yaml|" +
                         "Images (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|" +
                         "Log Files (*.log;*.txt)|*.log;*.txt"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    string deviceId = MacAddressBox.Text.Trim();
                    if (string.IsNullOrEmpty(deviceId))
                    {
                        MessageBox.Show("Please enter a MAC address first.",
                            "Missing Device ID", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    using var content = new MultipartFormDataContent();
                    var fileStream = File.OpenRead(dialog.FileName);
                    var fileContent = new StreamContent(fileStream);
                    fileContent.Headers.ContentType =
                        new MediaTypeHeaderValue("application/octet-stream");
                    content.Add(fileContent, "file", Path.GetFileName(dialog.FileName));

                    var response = await _httpClient.PostAsync(
                        $"/api/telemetry/upload/{deviceId}", content);

                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show(
                            $"File '{Path.GetFileName(dialog.FileName)}' uploaded successfully!",
                            "Upload Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show($"Upload failed: {response.StatusCode}",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Upload error: {ex.Message}",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // ============================================================
        // Register Sensor
        // ============================================================
        private async void RegisterSensor_Click(object sender, RoutedEventArgs e)
        {
            if (!_isConnected)
            {
                MessageBox.Show("Please connect to the gateway first!",
                    "Not Connected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var selectedCategory = (CategoryCombo.SelectedItem as ComboBoxItem)?.Content?.ToString()
                                       ?? "Unknown";

                var registration = new
                {
                    macAddress = MacAddressBox.Text.Trim(),
                    location = LocationBox.Text.Trim(),
                    category = selectedCategory
                };

                if (string.IsNullOrEmpty(registration.macAddress))
                {
                    MessageBox.Show("MAC Address is required.",
                        "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var response = await _httpClient.PostAsJsonAsync(
                    "/api/telemetry/sensor/register", registration);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show(
                        $"Sensor '{registration.macAddress}' registered successfully!",
                        "Registration Complete", MessageBoxButton.OK, MessageBoxImage.Information);

                    MacAddressBox.Clear();
                    LocationBox.Clear();
                }
                else
                {
                    MessageBox.Show($"Registration failed: {response.StatusCode}",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Registration error: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}