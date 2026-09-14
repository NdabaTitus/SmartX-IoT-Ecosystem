namespace SmartX.WPF.Models
{
    /// <summary>
    /// Mirror of the Gateway's TelemetryData model.
    /// This is what gets deserialized from the API JSON response.
    /// </summary>
    public class TelemetryData
    {
        public int Id { get; set; }
        public string DeviceId { get; set; } = string.Empty;
        public string SensorType { get; set; } = string.Empty;
        public double Value { get; set; }
        public string Unit { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public bool IsConnected { get; set; } = true;
    }

    /// <summary>
    /// Shape of the JSON returned by /api/telemetry/statistics
    /// </summary>
    public class TelemetryStatistics
    {
        public int TotalReadings { get; set; }
        public int UniqueDevices { get; set; }
        public double AverageValue { get; set; }
        public int DisconnectedCount { get; set; }
        public double RecursiveSum { get; set; }
    }
}