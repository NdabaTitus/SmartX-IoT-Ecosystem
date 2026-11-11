using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SmartX.Gateway.Models;

namespace SmartX.Gateway.Services
{
    
    public class TelemetryProcessingService : BackgroundService
    {
        private readonly SmartXDataStructures _data;
        private readonly NetworkTopologyService _topologyService;

        public TelemetryProcessingService(SmartXDataStructures data, NetworkTopologyService topologyService)
        {
            _data = data;
            _topologyService = topologyService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
               
                if (_data.CriticalAlertQueue.TryDequeue(out var criticalData, out _))
                {
                    
                    if (_data.DisconnectedNodes.Add(criticalData.DeviceId))
                    {
                        
                        Console.WriteLine($"[BACKGROUND] PROCESSED CRITICAL ALERT for {criticalData.DeviceId}");
                        _topologyService.AddTelemetryToTree(criticalData);
                    }
                    else
                    {
                       
                        Console.WriteLine($"[BACKGROUND] Ignored duplicate alert for {criticalData.DeviceId}");
                    }
                }
              
                else if (_data.TelemetryQueue.TryDequeue(out var normalData))
                {
                    _topologyService.AddTelemetryToTree(normalData);
                }

                
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
