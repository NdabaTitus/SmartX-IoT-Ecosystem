using Microsoft.AspNetCore.Mvc;
using SmartX.Gateway.Generics;
using SmartX.Gateway.Models;
using SmartX.Gateway.Services;
using System.IO;
using System.Linq;

namespace SmartX.Gateway.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TelemetryController : ControllerBase
    {
        private readonly TelemetryService _telemetryService;
        private readonly DeploymentTreeService _deploymentService;

        public TelemetryController(TelemetryService telemetryService)
        {
            _telemetryService = telemetryService;
            _deploymentService = new DeploymentTreeService();
        }

       
        [HttpPost]
        public IActionResult ReceiveTelemetry([FromBody] TelemetryData data)
        {
            if (string.IsNullOrEmpty(data.DeviceId))
            {
                return BadRequest(new { message = "DeviceId is required" });
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

      
        [HttpPost("generic")]
        public IActionResult ReceiveGenericPacket([FromBody] TelemetryPacket<double> packet)
        {
            if (packet == null || string.IsNullOrEmpty(packet.DeviceMacAddress))
            {
                return BadRequest(new { message = "Invalid packet." });
            }

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

            return Ok(new
            {
                message = "Generic packet received successfully",
                id = saved.Id,
                deviceId = saved.DeviceId
            });
        }

       
        [HttpGet]
        public IActionResult GetAllTelemetry()
        {
            var data = _telemetryService.GetAllTelemetry();
            return Ok(data);
        }

        
        [HttpGet("device/{deviceId}")]
        public IActionResult GetTelemetryByDevice(string deviceId)
        {
            var data = _telemetryService.GetTelemetry(deviceId);
            return Ok(data);
        }

      
        [HttpGet("latest")]
        public IActionResult GetLatestPerDevice()
        {
            var data = _telemetryService.GetLatestPerDevice();
            return Ok(data);
        }

        
        [HttpGet("statistics")]
        public IActionResult GetStatistics()
        {
            var stats = _telemetryService.GetStatistics();
            return Ok(stats);
        }

        
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
            {
                return BadRequest(new { message = "MAC address is required." });
            }

            return Ok(new
            {
                message = "Sensor registered successfully",
                registration = registration,
                registeredAt = DateTime.UtcNow
            });
        }


        // ============================================================
        // Upload a file attached to a specific sensor
        // ============================================================
        [HttpPost("upload/{deviceId}")]
        public async Task<IActionResult> UploadFile(string deviceId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No file uploaded." });

            if (string.IsNullOrWhiteSpace(deviceId))
                return BadRequest(new { message = "Device ID is required." });

            try
            {
                // Sanitize the deviceId so it can be used as a folder name.
                // MAC addresses contain ':' which Windows does not allow in paths.
                var safeDeviceId = SanitizeFolderName(deviceId);

                var uploadsFolder = Path.Combine(
                    Directory.GetCurrentDirectory(), "Uploads", safeDeviceId);
                Directory.CreateDirectory(uploadsFolder);

                // Sanitize the filename too (in case it has odd characters)
                var safeFileName = SanitizeFolderName(Path.GetFileName(file.FileName));
                var filePath = Path.Combine(uploadsFolder, safeFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return Ok(new
                {
                    message = "File uploaded successfully",
                    deviceId = deviceId,          // original (with colons) for display
                    fileName = file.FileName,     // original filename for display
                    size = file.Length,
                    storedAs = safeFileName,
                    path = filePath
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "File upload failed",
                    error = ex.Message
                });
            }
        }

        
        /// Replaces any characters that are illegal in Windows file/folder names
        /// (like ':' from MAC addresses) with underscores.
      
        private static string SanitizeFolderName(string name)
        {
            if (string.IsNullOrEmpty(name)) return "unknown";

            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = new string(name
                .Select(c => invalidChars.Contains(c) ? '_' : c)
                .ToArray());

            // Also cap the length to avoid extremely long paths
            if (sanitized.Length > 100)
                sanitized = sanitized.Substring(0, 100);

            return sanitized;
        }


        [HttpPost("validate-deployment")]
        public IActionResult ValidateDeployment([FromBody] DeploymentNode root)
        {
            if (root == null)
                return BadRequest(new { message = "Deployment root is required." });

            var issues = _deploymentService.ValidateTree(root);
            var deviceCount = _deploymentService.CountDevices(root);

            return Ok(new
            {
                totalDevices = deviceCount,
                issueCount = issues.Count,
                issues = issues,
                isValid = issues.Count == 0
            });
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