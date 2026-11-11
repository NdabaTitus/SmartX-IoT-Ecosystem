using System;
using System.Collections.Generic;
using System.Linq;
using SmartX.Gateway.Models;

namespace SmartX.Gateway.Services
{
    
    public class TelemetryBSTNode
    {
        public DateTime Timestamp { get; set; }
        public TelemetryData Data { get; set; }
        public TelemetryBSTNode Left { get; set; }
        public TelemetryBSTNode Right { get; set; }

        public TelemetryBSTNode(DateTime timestamp, TelemetryData data)
        {
            Timestamp = timestamp;
            Data = data;
        }
    }

    public class TelemetryBST
    {
        public TelemetryBSTNode Root { get; private set; }

        public void Insert(DateTime timestamp, TelemetryData data)
        {
            Root = InsertRec(Root, timestamp, data);
        }

        private TelemetryBSTNode InsertRec(TelemetryBSTNode root, DateTime timestamp, TelemetryData data)
        {
            if (root == null) return new TelemetryBSTNode(timestamp, data);

            if (timestamp < root.Timestamp)
                root.Left = InsertRec(root.Left, timestamp, data);
            else if (timestamp > root.Timestamp)
                root.Right = InsertRec(root.Right, timestamp, data);

            return root;
        }

       
        public List<TelemetryData> GetRange(DateTime start, DateTime end)
        {
            var result = new List<TelemetryData>();
            GetRangeRec(Root, start, end, result);
            return result;
        }

        private void GetRangeRec(TelemetryBSTNode root, DateTime start, DateTime end, List<TelemetryData> result)
        {
            if (root == null) return;

            if (start < root.Timestamp)
                GetRangeRec(root.Left, start, end, result);

            if (start <= root.Timestamp && end >= root.Timestamp)
                result.Add(root.Data);

            if (end > root.Timestamp)
                GetRangeRec(root.Right, start, end, result);
        }
    }

   
    public class MeshNode
    {
        public string MacAddress { get; set; }
        public bool IsOnline { get; set; } = true;
        public List<MeshEdge> Edges { get; set; } = new();
    }

    public class MeshEdge
    {
        public string SourceMac { get; set; }
        public string TargetMac { get; set; }
        public int Weight { get; set; } // Lower weight = stronger signal / closer distance
    }

    public class MeshGraph
    {
        public Dictionary<string, MeshNode> Nodes { get; set; } = new();

        public void AddNode(string mac)
        {
            if (!Nodes.ContainsKey(mac))
                Nodes[mac] = new MeshNode { MacAddress = mac };
        }

        public void AddEdge(string sourceMac, string targetMac, int weight)
        {
            if (Nodes.ContainsKey(sourceMac) && Nodes.ContainsKey(targetMac))
            {
                Nodes[sourceMac].Edges.Add(new MeshEdge { SourceMac = sourceMac, TargetMac = targetMac, Weight = weight });
                Nodes[targetMac].Edges.Add(new MeshEdge { SourceMac = targetMac, TargetMac = sourceMac, Weight = weight });
            }
        }

       
        public List<string> GetConnectedDevices(string startMac)
        {
            var visited = new HashSet<string>();
            var queue = new Queue<string>();
            var result = new List<string>();

            if (!Nodes.ContainsKey(startMac) || !Nodes[startMac].IsOnline) return result;

            queue.Enqueue(startMac);
            visited.Add(startMac);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                result.Add(current);

                foreach (var edge in Nodes[current].Edges)
                {
                    if (!visited.Contains(edge.TargetMac) && Nodes[edge.TargetMac].IsOnline)
                    {
                        visited.Add(edge.TargetMac);
                        queue.Enqueue(edge.TargetMac);
                    }
                }
            }
            return result;
        }

      
        public List<MeshEdge> GetMinimumSpanningTree()
        {
            var mst = new List<MeshEdge>();
            var visited = new HashSet<string>();
            var priorityQueue = new PriorityQueue<MeshEdge, int>();

            var startNode = Nodes.Keys.FirstOrDefault(n => Nodes[n].IsOnline);
            if (startNode == null) return mst;

            visited.Add(startNode);

            foreach (var edge in Nodes[startNode].Edges.Where(e => Nodes[e.TargetMac].IsOnline))
                priorityQueue.Enqueue(edge, edge.Weight);

            while (priorityQueue.Count > 0)
            {
                var edge = priorityQueue.Dequeue();

                if (!visited.Contains(edge.TargetMac))
                {
                    visited.Add(edge.TargetMac);
                    mst.Add(edge);

                    foreach (var nextEdge in Nodes[edge.TargetMac].Edges.Where(e => Nodes[e.TargetMac].IsOnline))
                    {
                        if (!visited.Contains(nextEdge.TargetMac))
                        {
                            priorityQueue.Enqueue(nextEdge, nextEdge.Weight);
                        }
                    }
                }
            }
            return mst;
        }
    }

    
    public class NetworkTopologyService
    {
        public TelemetryBST HistoricalDataTree { get; } = new();
        public MeshGraph NetworkGraph { get; } = new();

        public NetworkTopologyService()
        {
            
            NetworkGraph.AddNode("ESP32-A");
            NetworkGraph.AddNode("ESP32-B");
            NetworkGraph.AddNode("ESP32-C");
            NetworkGraph.AddNode("ESP32-D");
            NetworkGraph.AddNode("GATEWAY");

            
            NetworkGraph.AddEdge("ESP32-A", "ESP32-B", 2);
            NetworkGraph.AddEdge("ESP32-A", "GATEWAY", 10);
            NetworkGraph.AddEdge("ESP32-B", "ESP32-C", 3);
            NetworkGraph.AddEdge("ESP32-B", "GATEWAY", 6);
            NetworkGraph.AddEdge("ESP32-C", "ESP32-D", 1);
            NetworkGraph.AddEdge("ESP32-C", "GATEWAY", 4);
            NetworkGraph.AddEdge("ESP32-D", "GATEWAY", 8);
        }

        public void AddTelemetryToTree(TelemetryData data)
        {
            HistoricalDataTree.Insert(DateTime.Now, data);
        }
    }
}