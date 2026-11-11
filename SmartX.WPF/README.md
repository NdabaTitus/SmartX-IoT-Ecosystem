# Smart-X IoT Ecosystem - Final Portfolio of Evidence (PoE)

## Project Overview
Smart-X is an advanced, full-stack IoT ecosystem built in C# (.NET). It models a mesh network of ESP32 microcontrollers, managing real-time telemetry ingestion, command routing, historical data querying, and network topology optimization. The system consists of an ASP.NET Core Web API backend and a WPF desktop frontend.

##  Architecture & Data Structures
This project implements several complex data structures to handle high-throughput telemetry and routing:

1. **Binary Search Tree (BST):** Used in `NetworkTopologyService.cs` to store historical telemetry. It allows O(log n) time complexity for querying specific time ranges (e.g., retrieving all data between 14:00 and 16:00) without iterating through a massive list.
2. **Graph & Minimum Spanning Tree (MST):** Used to model the mesh network. Vertices are nodes, edges are connections. **Prim's Algorithm** is implemented to calculate the most power-efficient routing path for data packets. **BFS** is used for network traversal and self-healing.
3. **Queues, Stacks, Dictionaries (Part 2):** Used to handle FIFO telemetry, Undo commands (LIFO), O(1) device lookups, and preventing duplicate alerts.

##  How to Run the Application

### Prerequisites
- Visual Studio 2022 (with .NET Desktop and ASP.NET workloads).
- .NET 8.0 SDK or higher.
- Firebase Account (for Authentication).

### Steps
1. Open the `SmartX.sln` solution in Visual Studio.
2. Right-click the Solution in Solution Explorer and select **Configure Startup Projects**.
3. Set both `SmartX.Gateway` (API) and `SmartX.WPF` (Client) to **Start**.
4. Press **F5** to run both applications.
5. In the WPF Login window, authenticate using your Firebase credentials.

##  How to Test the Features (PoE Requirements)

### 1. Testing the Binary Search Tree (Historical Queries)
1. Navigate to **Tab 1** and click **Generate Test Data** a few times.
2. Wait 2 seconds (the background service pushes the data into the BST).
3. Navigate to **Tab 3** and click **Query Last 24 Hours**.
4. The third column will populate with data from the BST, proving efficient time-range querying.

### 2. Testing the Graph and Minimum Spanning Tree (Mesh Routing)
1. Navigate to **Tab 3**.
2. Click **Load Network Topology** to see the Graph nodes and their connections.
3. Click **Calculate Minimum Spanning Tree** to run Prim's Algorithm and view the most power-efficient routing paths.

### 3. Testing the Command Stream (Part 2 Fixes)
1. Navigate to **Tab 2**.
2. Click **Turn On Pump** and **Undo Last Command** to test the Stack.
3. To simulate a high-priority alert (Priority Queue), go to Tab 1, generate data until a value drops below 20, and view the red alert in Tab 2's "Suggested Actions" panel.

### 4. Dictionary (Device Registry)
1. Create a text file `devices.txt` formatted as: `MAC,Location,Category` (e.g., `AA:BB:CC:DD:EE:01,Zone A,Environmental`).
2. Navigate to **Tab 2** and click **Upload Devices (.txt)**. The text file is parsed and instantly populates the Dictionary and the UI grid.

---

##  AI Sub-Report

**Use of Generative AI in Development**

During the development of the Smart-X C# prototype, Generative AI (ChatGPT) was utilized as an advanced debugging assistant and architectural guide. It was instrumental in pivoting the project requirements into a functional .NET solution.

**1. Architectural & Data Structure Guidance:**
The AI provided the exact implementation structure for the `SmartXDataStructures` class (Part 2) and the `NetworkTopologyService` (Part 3). It demonstrated how to combine `Queue`, `PriorityQueue`, `Stack`, `Dictionary`, `HashSet`, `Binary Search Tree`, and `Graph` structures within a unified ASP.NET Core service. It also provided the logic for Prim's Algorithm for the Minimum Spanning Tree.

**2. Debugging C# and WPF:**
AI was used to resolve complex WPF threading issues (e.g., ensuring the `DispatcherTimer` correctly invoked async API calls without freezing the UI). It also assisted in troubleshooting the `HttpClient` multipart form data upload and resolving JSON circular reference errors when passing Graph objects from the API to the WPF client.

**3. Test-Driven Development (TDD):**
The AI generated the xUnit test cases for the `SmartX.Tests` project. It provided the correct `Assert` methods to verify FIFO behavior in Queues and LIFO behavior in Stacks, ensuring the application met the rubric's data structure requirements without crashing.

**4. UI/UX & CI/CD:**
AI helped design the dark-themed XAML layout, specifically the `TabControl` navigation and DataGrid binding for both telemetry and the device registry. Finally, it generated the YAML configuration for the GitHub Actions `.NET` workflow, automating the build and test process on every commit.

**Conclusion:** AI effectively reduced the time spent troubleshooting environment-specific NuGet package errors and XAML binding issues, allowing more focus on the core logic and rubric requirements.

---

##  Video Demonstration
*[Watch the video here](https://youtu.be/jBxjDMbt_fg)*