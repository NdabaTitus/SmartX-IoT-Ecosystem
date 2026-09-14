namespace SmartX.Gateway.Generics
{
   
    public class TelemetryPacket<T>
    {
        public string DeviceMacAddress { get; set; } = string.Empty;
        public string SensorCategory { get; set; } = string.Empty;
        public T Payload { get; set; }
        public DateTime Timestamp { get; set; }
        public string Unit { get; set; } = string.Empty;

        
        /// Parameterless constructor — required by System.Text.Json
        /// so it can deserialize incoming JSON into this class.
     
        public TelemetryPacket()
        {
            Timestamp = DateTime.UtcNow;
        }

      
        /// Convenience constructor for creating packets in code.
        /// Note: this is used when you build packets programmatically,
        /// NOT for JSON deserialization.
       
        public TelemetryPacket(string macAddress, string category, T payload, string unit = "")
        {
            DeviceMacAddress = macAddress;
            SensorCategory = category;
            Payload = payload;
            Timestamp = DateTime.UtcNow;
            Unit = unit;
        }

       
        /// Returns a display string for the packet payload.
     
        public string GetDisplayValue() => $"{Payload} {Unit}".Trim();

     
        /// Operator overloading: aggregate two packets of the same type.
        /// Example: Meter3 = Meter1 + Meter2 (for int/float payloads).
    
        public static TelemetryPacket<T> operator +(TelemetryPacket<T> a, TelemetryPacket<T> b)
        {
            if (a == null || b == null)
                throw new ArgumentNullException("Cannot aggregate null packets.");

            dynamic sum = (dynamic)a.Payload + (dynamic)b.Payload;

            return new TelemetryPacket<T>(
                $"{a.DeviceMacAddress}+{b.DeviceMacAddress}",
                a.SensorCategory,
                (T)sum,
                a.Unit
            );
        }
    }
}