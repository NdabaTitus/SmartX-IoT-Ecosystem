using SmartX.Gateway.Models;

namespace SmartX.Gateway.Services
{
  
    
    
    public class TelemetryService
    {
       
        private readonly List<TelemetryData> _telemetryStore = new();
        private int _nextId = 1;

        
        private readonly object _lock = new();

        
      
        public TelemetryData AddTelemetry(TelemetryData data)
        {
            lock (_lock)
            {
                data.Id = _nextId++;
                data.Timestamp = DateTime.UtcNow;
                _telemetryStore.Add(data);
                return data;
            }
        }

       
        
        
        public TelemetryData AddTelemetry(string deviceId, string sensorType, double value, string unit)
        {
            var data = new TelemetryData
            {
                DeviceId = deviceId,
                SensorType = sensorType,
                Value = value,
                Unit = unit,
                IsConnected = true
            };
            return AddTelemetry(data);
        }

   
     
     
        public List<TelemetryData> GetAllTelemetry()
        {
            lock (_lock)
            {
                return new List<TelemetryData>(_telemetryStore);
            }
        }

     
       
      
        public List<TelemetryData> GetTelemetry(string deviceId)
        {
            lock (_lock)
            {
                return _telemetryStore
                    .Where(t => t.DeviceId.Equals(deviceId, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
        }

        
        /// Gets the latest reading for each unique device.
        /// Uses LINQ and demonstrates understanding of data aggregation.
      
        public List<TelemetryData> GetLatestPerDevice()
        {
            lock (_lock)
            {
                return _telemetryStore
                    .GroupBy(t => t.DeviceId)
                    .Select(g => g.OrderByDescending(t => t.Timestamp).First())
                    .ToList();
            }
        }

        /// RECURSION EXAMPLE: Calculates the sum of all values recursively.
        /// This demonstrates recursive thinking for educational purposes.
        /// In production, you'd use LINQ's .Sum()
      
        public double CalculateTotalRecursive(List<TelemetryData> data, int index = 0)
        {
            // Base case: we've reached the end of the list
            if (index >= data.Count)
                return 0;

            // Recursive case: current value + sum of rest
            return data[index].Value + CalculateTotalRecursive(data, index + 1);
        }

    
        /// Gets statistics for the dashboard
       
        public object GetStatistics()
        {
            lock (_lock)
            {
                if (_telemetryStore.Count == 0)
                {
                    return new
                    {
                        totalReadings = 0,
                        uniqueDevices = 0,
                        averageValue = 0.0,
                        disconnectedCount = 0
                    };
                }

                return new
                {
                    totalReadings = _telemetryStore.Count,
                    uniqueDevices = _telemetryStore.Select(t => t.DeviceId).Distinct().Count(),
                    averageValue = Math.Round(_telemetryStore.Average(t => t.Value), 2),
                    disconnectedCount = _telemetryStore.Count(t => !t.IsConnected),
                    recursiveSum = CalculateTotalRecursive(_telemetryStore)
                };
            }
        }

      
        /// Clears all data (useful for testing)
        
        public void ClearAll()
        {
            lock (_lock)
            {
                _telemetryStore.Clear();
                _nextId = 1;
            }
        }
    
        /// Uses a jagged array (double[][]) to manage sequential historical
        /// batches of raw telemetry before transferring them into an
        /// optimized List<T>.
       
        public Dictionary<string, List<double>> ProcessBatchWithJaggedArray(double[][] rawBatches)
        {
         
            var processed = new Dictionary<string, List<double>>();

            for (int i = 0; i < rawBatches.Length; i++)
            {
                string batchName = $"Batch_{i + 1}";
                processed[batchName] = new List<double>();

                for (int j = 0; j < rawBatches[i].Length; j++)
                {
                    processed[batchName].Add(rawBatches[i][j]);
                }
            }

            return processed;
        }
    }
}