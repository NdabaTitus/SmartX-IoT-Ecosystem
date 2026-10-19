using System;
using System.Collections.Generic;
using SmartX.Gateway.Models;

namespace SmartX.Gateway.Services
{
    public class SmartXDataStructures
    {
      
        public Queue<TelemetryData> TelemetryQueue { get; } = new();
        public PriorityQueue<TelemetryData, int> CriticalAlertQueue { get; } = new();
        public Stack<Command> CommandUndoStack { get; } = new();

        public Dictionary<string, Device> DeviceRegistry { get; } = new();
        public SortedDictionary<DateTime, TelemetryData> SensorTimeline { get; } = new();

     
        public HashSet<string> DisconnectedNodes { get; } = new();

        public List<UserActionLog> ActionHistory { get; } = new();
    }

    public class Device
    {
        public string MacAddress { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }

    public class Command
    {
        public string DeviceMacAddress { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public DateTime IssuedAt { get; set; } = DateTime.Now;
    }

    public class UserActionLog
    {
        public string ActionType { get; set; } = string.Empty; 
        public string Details { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}