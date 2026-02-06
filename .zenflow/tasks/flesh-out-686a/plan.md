# Full SDD workflow

## Configuration
- **Artifacts Path**: {@artifacts_path} → `.zenflow/tasks/{task_id}`

---

## Workflow Steps

### [x] Step: Requirements
<!-- chat-id: 924d45a3-588a-4ce3-afe6-cc1f0264a83c -->

Create a Product Requirements Document (PRD) based on the feature description.

1. Review existing codebase to understand current architecture and patterns
2. Analyze the feature definition and identify unclear aspects
3. Ask the user for clarifications on aspects that significantly impact scope or user experience
4. Make reasonable decisions for minor details based on context and conventions
5. If user can't clarify, make a decision, state the assumption, and continue

Save the PRD to `{@artifacts_path}/requirements.md`.

### [x] Step: Technical Specification
<!-- chat-id: 58a47d98-309d-4c85-b21b-7b623bfc0534 -->

Create a technical specification based on the PRD in `{@artifacts_path}/requirements.md`.

1. Review existing codebase architecture and identify reusable components
2. Define the implementation approach

Save to `{@artifacts_path}/spec.md` with:
- Technical context (language, dependencies)
- Implementation approach referencing existing code patterns
- Source code structure changes
- Data model / API / interface changes
- Delivery phases (incremental, testable milestones)
- Verification approach using project lint/test commands

### [x] Step: Planning
<!-- chat-id: 2328a8e8-ed5e-4ce7-8e10-75f115f2b5ea -->

Create a detailed implementation plan based on `{@artifacts_path}/spec.md`.

1. Break down the work into concrete tasks
2. Each task should reference relevant contracts and include verification steps
3. Replace the Implementation step below with the planned tasks

Rule of thumb for step size: each step should represent a coherent unit of work (e.g., implement a component, add an API endpoint). Avoid steps that are too granular (single function) or too broad (entire feature).

Important: unit tests must be part of each implementation task, not separate tasks. Each task should implement the code and its tests together, if relevant.

If the feature is trivial and doesn't warrant full specification, update this workflow to remove unnecessary steps and explain the reasoning to the user.

Save to `{@artifacts_path}/plan.md`.

### [x] Step: Core Backend - Setup & Capture
<!-- chat-id: fb5373ee-ce6a-4cb4-ba82-e9d7b8fcf9d8 -->
Initialize the MelonLoader mod and apply Harmony patches to capture the active `MoonSharp.Interpreter.Script` instance.

1. Create the `CoreBackend` project structure.
2. Implement the `MelonMod` entry point in `Core.cs`.
3. Implement Harmony prefix patch on `Il2CppMoonSharp.Interpreter.Script.DoString` in `ScriptEngineCapture.cs`.
4. Add logging to verify capture.

Verification: Check `MelonLoader` logs for "Captured Script Instance" message when in-game scripts run.

### [x] Step: Core Backend - IPC Server
<!-- chat-id: 4fa0bb1f-752a-4d3c-bcf5-72b3ef697ded -->
Implement a Named Pipe server to receive script strings from the external UI.

1. Implement `PipeServer.cs` using `System.IO.Pipes`.
2. Run the server on a background thread.
3. Implement a thread-safe buffer for received scripts.

Verification: Use a simple script to send data to the pipe and log the received content in the game console.

### [x] Step: Core Backend - Script Execution & API
<!-- chat-id: 12ae57ac-4681-40fd-a458-58c48962eb98 -->
Implement script execution on the Unity main thread and register custom Lua commands.

1. Implement `UnityMainThreadDispatcher.cs` (or use MelonLoader's `OnUpdate`).
2. Execute buffered scripts using `_capturedScript.DoString(code)`.
3. Implement `CommandRegistry.cs` and register `print_custom(message)`.

Verification: Execute `print_custom("Test")` via the captured script and verify output in the game console.

### [x] Step: Frontend - Migrate to Avalonia UI
Migrate the Frontend from WinForms to Avalonia UI for a more modern look and cross-platform potential.

1. Update `Frontend.csproj` with Avalonia 11 and `WebViewControl-Avalonia`.
2. Implement `App.axaml` and `MainWindow.axaml` (Synapse X style).
3. Port logic from `MainForm.cs` to `MainWindow.axaml.cs`.
4. Verify build and IPC integration.

Verification: Build succeeds and the UI launches with functional script execution.

### [x] Step: Frontend - IPC Client & Integration
<!-- chat-id: 1148ed18-4f5a-4ce8-a5e4-7898e2750f2e -->
Implement the IPC client and connect the UI "Execute" button to the Backend.

1. Implement `PipeClient.cs` to connect to `TheBadPlace_Executor_Pipe`.
2. Link the "Execute" button to send the editor's text via the IPC client.
3. Final end-to-end testing of the full execution flow.

Verification: Type `print_custom("Hello from UI")` in the editor, click Execute, and see the message in the Polytoria console.

### [x] Step: Environment - Folder Setup (AppData)
Initialize the AppData folder structure for scripts and configuration.

1. Define a shared `EnvironmentUtils` or constant class for paths.
2. Create `%AppData%\TheBadPlace\AutoExec`, `Scripts`, and `Workspace` folders on startup in both Frontend and Backend.

Verification: Folders exist in AppData after running the applications.

### [x] Step: Frontend - Script Hub & File System Integration
Implement script loading and management.

1. Populate the `scriptList` in `MainForm.cs` from the `Scripts` folder.
2. Implement file clicking to load content into the current tab.
3. Implement "Open File" and "Save File" button logic.

Verification: Files in the `Scripts` folder show up in the UI and can be loaded/saved.

### [x] Step: Frontend - UI Polish (Tabs & Status)
Enhance the UI with multi-tab support and connection status.

1. Replace single `WebView2` with a `TabControl` containing `WebView2` instances.
2. Add a status label/dot to indicate if the IPC connection to the game is active.
3. Add a "+" button to create new script tabs.

Verification: Can open multiple tabs and switch between them. Status indicator updates based on connection success.

### [x] Step: Core Backend - AutoExec Implementation
Implement automatic script execution on startup.

1. Backend should scan `%AppData%\TheBadPlace\AutoExec` on initialization.
2. Execute all `.lua` and `.txt` files found in that folder using the captured script instance.

Verification: Scripts placed in `AutoExec` run automatically when the mod initializes in-game.
