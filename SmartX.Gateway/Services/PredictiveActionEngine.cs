using System;
using System.Collections.Generic;
using System.Linq;
using SmartX.Gateway.Models;

namespace SmartX.Gateway.Services
{
    
    public class PredictiveActionEngine
    {
        private readonly SmartXDataStructures _data;

        public PredictiveActionEngine(SmartXDataStructures data)
        {
            _data = data;
        }

        
        public List<string> GenerateInsights()
        {
            var suggestions = new List<string>();

            foreach (var node in _data.DisconnectedNodes)
            {
                suggestions.Add($"⚠️ ALERT: Node {node} is currently disconnected. Check network mesh.");
            }

        
            foreach (var telemetry in _data.TelemetryQueue)
            {
                if (telemetry.SensorType == "SoilMoisture" && telemetry.Value < 20.0)
                {
                    suggestions.Add($"⚠️ CRITICAL: {telemetry.DeviceId} moisture is at {telemetry.Value}%. Turn on water pump immediately.");
                }
            }

           

            bool searchedForPump = _data.ActionHistory.Any(a =>
                a.ActionType == "SEARCH" && a.Details.Contains("Pump", StringComparison.OrdinalIgnoreCase));

            if (searchedForPump)
            {
                suggestions.Add("💡 INSIGHT: Based on your recent search for 'Pump', we recommend issuing a 'Turn On' command for Pump A.");
            }

           
            var frequentCommands = _data.ActionHistory
                .Where(a => a.ActionType == "COMMAND")
                .GroupBy(a => a.Details)
                .Where(g => g.Count() >= 3)
                .Select(g => g.Key);

            foreach (var cmd in frequentCommands)
            {
                suggestions.Add($"💡 INSIGHT: You frequently issue the command '{cmd}'. You can add this to your quick actions.");
            }

            
            return suggestions.Distinct().ToList();
        }

        
        public void LogAction(string type, string details)
        {
            _data.ActionHistory.Add(new UserActionLog
            {
                ActionType = type,
                Details = details
            });
        }
    }
}