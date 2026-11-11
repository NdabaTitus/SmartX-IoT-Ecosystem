using SmartX.Gateway.Services;
using SmartX.Gateway.Models;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;

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

       

        [Fact]
        public void TelemetryBST_ShouldInsertAndQueryDateRange()
        {
            var bst = new TelemetryBST();
            var now = DateTime.Now;

            bst.Insert(now.AddHours(-2), new TelemetryData { DeviceId = "Node1", Value = 10 });
            bst.Insert(now.AddHours(-1), new TelemetryData { DeviceId = "Node1", Value = 20 });
            bst.Insert(now, new TelemetryData { DeviceId = "Node1", Value = 30 });

            var results = bst.GetRange(now.AddHours(-1.5), now);

            Assert.Equal(2, results.Count);
            Assert.Equal(20, results[0].Value);
            Assert.Equal(30, results[1].Value);
        }

        [Fact]
        public void MeshGraph_BFS_ShouldReturnAllConnectedDevices()
        {
            var graph = new MeshGraph();
            graph.AddNode("A");
            graph.AddNode("B");
            graph.AddNode("C");
            graph.AddNode("D");

            graph.AddEdge("A", "B", 1);
            graph.AddEdge("B", "C", 1);

            var connected = graph.GetConnectedDevices("A");

            Assert.Equal(3, connected.Count);
            Assert.Contains("A", connected);
            Assert.Contains("B", connected);
            Assert.Contains("C", connected);
            Assert.DoesNotContain("D", connected);
        }

        [Fact]
        public void MeshGraph_MST_ShouldReturnMinimumSpanningTree()
        {
            var graph = new MeshGraph();
            graph.AddNode("A");
            graph.AddNode("B");
            graph.AddNode("C");

            graph.AddEdge("A", "B", 1);
            graph.AddEdge("B", "C", 2);
            graph.AddEdge("A", "C", 10);

            var mst = graph.GetMinimumSpanningTree();

            Assert.Equal(2, mst.Count);
            Assert.DoesNotContain(mst, e => e.Weight == 10);
        }

        [Fact]
        public void MeshGraph_MST_ShouldIgnoreOfflineNodes()
        {
            var graph = new MeshGraph();
            graph.AddNode("A");
            graph.AddNode("B");
            graph.AddNode("C");
            graph.Nodes["C"].IsOnline = false;

            graph.AddEdge("A", "B", 1);
            graph.AddEdge("B", "C", 1);

            var mst = graph.GetMinimumSpanningTree();

            Assert.Single(mst);
            Assert.Equal("B", mst[0].TargetMac);
        }
    }
}