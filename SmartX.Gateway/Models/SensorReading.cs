namespace SmartX.Gateway.Models
{
  
    public class SensorReading
    {
        public string DeviceId { get; set; } = string.Empty;
        public double Value { get; set; }
        public string Unit { get; set; } = string.Empty;

        public SensorReading() { }

        public SensorReading(string deviceId, double value, string unit = "")
        {
            DeviceId = deviceId;
            Value = value;
            Unit = unit;
        }


        /// Aggregate two readings: Meter3 = Meter1 + Meter2</summary>
        public static SensorReading operator +(SensorReading a, SensorReading b)
        {
            return new SensorReading(
                $"{a.DeviceId}+{b.DeviceId}",
                a.Value + b.Value,
                a.Unit
            );
        }

        ///Delta comparison: Delta = Meter2 - Meter1</summary>
        public static SensorReading operator -(SensorReading a, SensorReading b)
        {
            return new SensorReading(
                $"Delta({a.DeviceId},{b.DeviceId})",
                a.Value - b.Value,
                a.Unit
            );
        }

        /// Greater-than comparison</summary>
        public static bool operator >(SensorReading a, SensorReading b)
            => a.Value > b.Value;

        /// Less-than comparison</summary>
        public static bool operator <(SensorReading a, SensorReading b)
            => a.Value < b.Value;

        ///Equality (required when overloading ==)</summary>
        public static bool operator ==(SensorReading a, SensorReading b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a is null || b is null) return false;
            return Math.Abs(a.Value - b.Value) < 0.001;
        }

        public static bool operator !=(SensorReading a, SensorReading b)
            => !(a == b);

        public override bool Equals(object? obj) => obj is SensorReading s && this == s;
        public override int GetHashCode() => Value.GetHashCode();

        public override string ToString() => $"{DeviceId}: {Value} {Unit}";
    }
}