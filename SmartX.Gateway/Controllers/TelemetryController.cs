using Microsoft.AspNetCore.Mvc;
using SmartX.Gateway.Generics;
using SmartX.Gateway.Models;
using SmartX.Gateway.Services;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SmartX.Gateway.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TelemetryController : ControllerBase
    {
        private readonly TelemetryService _telemetryService;
        private readonly DeploymentTreeService _deploymentService;
        private readonly SmartXDataStructures _dataStructures;
        private readonly PredictiveActionEngine _predictiveEngine;

        public TelemetryController(
            TelemetryService telemetryService,
            SmartXDataStructures dataStructures,
            PredictiveActionEngine predictiveEngine)
        {
            _telemetryService = telemetryService;
            _dataStructures = dataStructures;
            _predictiveEngine = predictiveEngine;
            _deploymentService = new DeploymentTreeService();
        }

      
        [HttpPost]
        public IActionResult ReceiveTelemetry([FromBody] TelemetryData data)
        {
            if (string.IsNullOrEmpty(data.DeviceId))
                return BadRequest(new { message = "DeviceId is required" });

       
            _dataStructures.TelemetryQueue.Enqueue(data);

         
            _dataStructures.SensorTimeline[DateTime.Now] = data;

           
            if (data.SensorType == "SoilMoisture" && data.Value < 20.0)
            {
                _dataStructures.CriticalAlertQueue.Enqueue(data, 1); 
            }

            var saved = _telemetryService.AddTelemetry(data);

            return Ok(new
            {
                message = "Telemetry received successfully",
                id = saved.Id,
                deviceId = saved.DeviceId,
                sensorType = saved.SensorType,
                value = saved.Value,
                timestamp = saved.Timestamp
            });
        }

        [HttpGet("insights")]
        public IActionResult GetInsights()
        {
            var insights = _predictiveEngine.GenerateInsights();
            return Ok(insights);
        }


        [HttpPost("command")]
        public IActionResult IssueCommand([FromBody] Command command)
        {
            if (command == null || string.IsNullOrEmpty(command.DeviceMacAddress))
                return BadRequest(new { message = "Invalid command." });

          
            _predictiveEngine.LogAction("COMMAND", command.Action);

            
            _dataStructures.CommandUndoStack.Push(command);

            return Ok(new { message = $"Command '{command.Action}' issued to {command.DeviceMacAddress}." });
        }

        [HttpPost("undo")]
        public IActionResult UndoCommand()
        {
            if (_dataStructures.CommandUndoStack.Count == 0)
                return BadRequest(new { message = "No commands to undo." });

            var undoneCommand = _dataStructures.CommandUndoStack.Pop();

            return Ok(new { message = $"Undid command '{undoneCommand.Action}' on {undoneCommand.DeviceMacAddress}." });
        }

       
        [HttpPost("upload-devices")]
        public async Task<IActionResult> UploadDeviceList(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No file uploaded." });

            using var reader = new StreamReader(file.OpenReadStream());
            int count = 0;
            string? line;

            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

               
                var parts = line.Split(',');
                if (parts.Length >= 3)
                {
                    var mac = parts[0].Trim();
                    var device = new Device
                    {
                        MacAddress = mac,
                        Location = parts[1].Trim(),
                        Category = parts[2].Trim()
                    };
                    _dataStructures.DeviceRegistry[mac] = device;
                    count++;
                }
            }

            return Ok(new { message = $"Successfully loaded {count} devices into the registry." });
        }

  
        [HttpGet("devices")]
        public IActionResult GetDevices()
        {
            return Ok(_dataStructures.DeviceRegistry.Values);
        }

      
        [HttpPost("generic")]
        public IActionResult ReceiveGenericPacket([FromBody] TelemetryPacket<double> packet)
        {
            if (packet == null || string.IsNullOrEmpty(packet.DeviceMacAddress))
                return BadRequest(new { message = "Invalid packet." });

            var data = new TelemetryData
            {
                DeviceId = packet.DeviceMacAddress,
                SensorType = packet.SensorCategory,
                Value = packet.Payload,
                Unit = packet.Unit,
                Timestamp = packet.Timestamp,
                IsConnected = true
            };

            var saved = _telemetryService.AddTelemetry(data);

            return Ok(new { message = "Generic packet received successfully", id = saved.Id, deviceId = saved.DeviceId });
        }

        [HttpGet]
        public IActionResult GetAllTelemetry() => Ok(_telemetryService.GetAllTelemetry());

        [HttpGet("device/{deviceId}")]
        public IActionResult GetTelemetryByDevice(string deviceId) => Ok(_telemetryService.GetTelemetry(deviceId));

        [HttpGet("latest")]
        public IActionResult GetLatestPerDevice() => Ok(_telemetryService.GetLatestPerDevice());

        [HttpGet("statistics")]
        public IActionResult GetStatistics() => Ok(_telemetryService.GetStatistics());

        [HttpDelete("clear")]
        public IActionResult ClearAll()
        {
            _telemetryService.ClearAll();
            return Ok(new { message = "All telemetry data cleared" });
        }

        [HttpPost("sensor/register")]
        public IActionResult RegisterSensor([FromBody] SensorRegistration registration)
        {
            if (registration == null || string.IsNullOrWhiteSpace(registration.MacAddress))
                return BadRequest(new { message = "MAC address is required." });

            return Ok(new { message = "Sensor registered successfully", registration, registeredAt = DateTime.UtcNow });
        }

        [HttpPost("upload/{deviceId}")]
        public async Task<IActionResult> UploadFile(string deviceId, IFormFile file)
        {
            if (file == null || file.Length == 0 || string.IsNullOrWhiteSpace(deviceId))
                return BadRequest(new { message = "Invalid request." });

            try
            {
                var safeDeviceId = SanitizeFolderName(deviceId);
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", safeDeviceId);
                Directory.CreateDirectory(uploadsFolder);

                var safeFileName = SanitizeFolderName(Path.GetFileName(file.FileName));
                var filePath = Path.Combine(uploadsFolder, safeFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return Ok(new { message = "File uploaded successfully", deviceId, fileName = file.FileName, size = file.Length });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "File upload failed", error = ex.Message });
            }
        }

        private static string SanitizeFolderName(string name)
        {
            if (string.IsNullOrEmpty(name)) return "unknown";
            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = new string(name.Select(c => invalidChars.Contains(c) ? '_' : c).ToArray());
            return sanitized.Length > 100 ? sanitized.Substring(0, 100) : sanitized;
        }

        [HttpPost("validate-deployment")]
        public IActionResult ValidateDeployment([FromBody] DeploymentNode root)
        {
            if (root == null) return BadRequest(new { message = "Deployment root is required." });
            var issues = _deploymentService.ValidateTree(root);
            return Ok(new { totalDevices = _deploymentService.CountDevices(root), issueCount = issues.Count, issues, isValid = issues.Count == 0 });
        }
    }

    public class SensorRegistration
    {
        public string MacAddress { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }
}