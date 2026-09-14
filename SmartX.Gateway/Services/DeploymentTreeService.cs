namespace SmartX.Gateway.Services
{
   
    /// Represents a node in the device deployment tree.
    /// Each node can have child nodes (sub-zones, devices, etc.)
    
    public class DeploymentNode
    {
        public string Name { get; set; } = string.Empty;
        public string NodeType { get; set; } = string.Empty; // "Facility", "Zone", "SubZone", "Device"
        public string MacAddress { get; set; } = string.Empty;
        public bool IsConfigured { get; set; }
        public List<DeploymentNode> Children { get; set; } = new();
    }

  
    /// Recursive validation service that walks the deployment tree
    /// and validates each node, returning a list of issues found.
    /// 
    /// Example: Validates that a node is safely configured within
    /// Sub-Zone B -> Zone 1 -> Facility A.
  
    public class DeploymentTreeService
    {
       
        /// Recursively validates the entire deployment tree.
        /// Returns a list of validation issues.
     
        public List<string> ValidateTree(DeploymentNode node, string path = "")
        {
            var issues = new List<string>();
            var currentPath = string.IsNullOrEmpty(path) ? node.Name : $"{path} -> {node.Name}";

            // Validate the current node
            if (!node.IsConfigured)
            {
                issues.Add($"UNCONFIGURED: '{currentPath}' is not properly configured.");
            }

            if (string.IsNullOrWhiteSpace(node.MacAddress) && node.NodeType == "Device")
            {
                issues.Add($"MISSING MAC: '{currentPath}' is a device with no MAC address.");
            }

            // Recursive step: validate all children
            foreach (var child in node.Children)
            {
                issues.AddRange(ValidateTree(child, currentPath));
            }

            return issues;
        }

      
        /// Recursively counts all devices in the tree.
       
        public int CountDevices(DeploymentNode node)
        {
            int count = node.NodeType == "Device" ? 1 : 0;
            foreach (var child in node.Children)
            {
                count += CountDevices(child);
            }
            return count;
        }
    }
}