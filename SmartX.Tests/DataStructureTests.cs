using SmartX.Gateway.Services;
using SmartX.Gateway.Models;
using Xunit;
using System.Collections.Generic;

namespace SmartX.Tests
{
    public class DataStructureTests
    {
        [Fact]
        public void TelemetryQueue_ShouldProcessFirstInFirstOut()
        {
            var data = new SmartXDataStructures();
            var t1 = new TelemetryData { DeviceId = "ESP32-01" };
            var t2 = new TelemetryData { DeviceId = "ESP32-02" };

            data.TelemetryQueue.Enqueue(t1);
            data.TelemetryQueue.Enqueue(t2);

            Assert.Equal(2, data.TelemetryQueue.Count);
            Assert.Equal("ESP32-01", data.TelemetryQueue.Dequeue().DeviceId);
            Assert.Equal("ESP32-02", data.TelemetryQueue.Dequeue().DeviceId);
        }

        [Fact]
        public void CommandUndoStack_ShouldPopLastCommand()
        {
            var data = new SmartXDataStructures();
            data.CommandUndoStack.Push(new Command { Action = "Turn On Pump" });
            data.CommandUndoStack.Push(new Command { Action = "Toggle Relay" });

            var lastCommand = data.CommandUndoStack.Pop();
            Assert.Equal("Toggle Relay", lastCommand.Action);
            Assert.Single(data.CommandUndoStack);
        }

        [Fact]
        public void UndoStack_ShouldNotCrashWhenEmpty()
        {
            var data = new SmartXDataStructures();
            // Assert that popping an empty stack safely returns false
            Assert.False(data.CommandUndoStack.TryPop(out _));
        }

        [Fact]
        public void DeviceRegistry_Dictionary_ShouldAddAndRetrieveInstantly()
        {
            var data = new SmartXDataStructures();
            var device = new Device { MacAddress = "AA:BB:CC", Location = "Zone A" };

            data.DeviceRegistry["AA:BB:CC"] = device;

            Assert.True(data.DeviceRegistry.ContainsKey("AA:BB:CC"));
            Assert.Equal("Zone A", data.DeviceRegistry["AA:BB:CC"].Location);
        }
    }
}