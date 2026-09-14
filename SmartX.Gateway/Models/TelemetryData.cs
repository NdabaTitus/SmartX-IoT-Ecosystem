namespace SmartX.Gateway.Models
{
    
    public class TelemetryData
    {
        public int Id { get; set; }
        public string DeviceId { get; set; } = string.Empty;
        public string SensorType { get; set; } = string.Empty;
        public double Value { get; set; }
        public string Unit { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public bool IsConnected { get; set; } = true;

        // For anomaly detection
        public double? MinThreshold { get; set; }
        public double? MaxThreshold { get; set; }
    }
}