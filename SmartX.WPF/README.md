# Smart-X IoT Ecosystem - Part 2

## Project Overview
Smart-X is a hybrid IoT ecosystem built in C# (.NET). It consists of a RESTful API backend (`SmartX.Gateway`) and a Windows desktop client (`SmartX.WPF`). This application fulfills the Part 2 requirements for Real-Time Processing, Advanced Data Structures, and Predictive Action Engines.

##  Architecture & Features
1. **Authentication:** Secure Login via Firebase REST API.
2. **Navigation:** 3-Tab dashboard (Part 1, Part 2, Final PoE disabled).
3. **Advanced Data Structures:**
   - `Queue<T>` for FIFO telemetry.
   - `PriorityQueue<T>` for critical alerts (<20% moisture).
   - `Stack<T>` for manual command undo.
   - `Dictionary<TKey, TValue>` for instant device registry lookup.
   - `HashSet<T>` for deduplicating disconnected node alerts.
4. **Predictive Action Engine:** Analyzes user history and live telemetry to suggest actions in the UI before the user searches.

##  How to Test the Data Structures (Rubric Requirement)

### 1. Dictionary (Device Registry)
1. Create a text file `devices.txt` formatted as: `MAC,Location,Category` (e.g., `AA:BB:CC:DD:EE:01,Zone A,Environmental`).
2. Run the API and WPF app. Login and navigate to **Tab 2**.
3. Click **Upload Devices (.txt)**. The text file is parsed and instantly populates the `Dictionary<string, Device>` and the UI grid.

### 2. Simulating a High Priority Alert (PriorityQueue)
1. Navigate to **Tab 1** and click **Generate Test Data** repeatedly.
2. The API randomly generates soil moisture values. When a value drops below `20.0`, the API pushes it into the `PriorityQueue<TelemetryData, int>` with priority `1`.
3. **Result:** Navigate to **Tab 2**. The `PredictiveActionEngine` reads this queue and displays a red **"⚠ CRITICAL"** warning in the Suggested Actions panel.

### 3. Command Undo Stack (Stack)
1. On **Tab 2**, click **Turn On Pump** and **Toggle Relay**. (These are pushed to the Stack).
2. Click **Undo Last Command**. It safely pops the Stack.
3. Click it again when the stack is empty. The API returns a graceful "No commands to undo" message without crashing.

---

##  AI Sub-Report

**Use of Generative AI in Development**

During the development of the Smart-X C# prototype, Generative AI (ChatGPT) was utilized as an advanced debugging assistant and architectural guide. It was instrumental in pivoting the project requirements into a functional .NET solution.

**1. Architectural & Data Structure Guidance:**
The AI provided the exact implementation structure for the `SmartXDataStructures` class, demonstrating how to properly combine `Queue`, `PriorityQueue`, `Stack`, `Dictionary`, and `HashSet` within a unified Singleton service in ASP.NET Core. It also provided the logic for the `PredictiveActionEngine`, specifically the LINQ queries used to analyze user command history and group frequent actions.

**2. Debugging C# and WPF:**
AI was used to resolve complex WPF threading issues (e.g., ensuring the `DispatcherTimer` correctly invoked async API calls without freezing the UI). It also assisted in troubleshooting the `HttpClient` multipart form data upload, ensuring the C# API correctly parsed the uploaded `.txt` file line-by-line into the device registry.

**3. Test-Driven Development (TDD):**
The AI generated the xUnit test cases for the `SmartX.Tests` project. It provided the correct `Assert` methods to verify FIFO behavior in Queues and LIFO behavior in Stacks, ensuring the application met the rubric's data structure requirements without crashing.

**4. UI/UX & CI/CD:**
AI helped design the dark-themed XAML layout, specifically the `TabControl` navigation and DataGrid binding for both telemetry and the device registry. Finally, it generated the YAML configuration for the GitHub Actions `.NET` workflow, automating the build and test process on every commit.

**Conclusion:** AI effectively reduced the time spent troubleshooting environment-specific NuGet package errors and XAML binding issues, allowing more focus on the core logic and rubric requirements.

---

##  Video Demonstration
*[Watch the video here](YOUR_YOUTUBE_LINK)*