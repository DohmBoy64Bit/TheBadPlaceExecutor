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

### [ ] Step: Core Backend - Script Execution & API
Implement script execution on the Unity main thread and register custom Lua commands.

1. Implement `UnityMainThreadDispatcher.cs` (or use MelonLoader's `OnUpdate`).
2. Execute buffered scripts using `_capturedScript.DoString(code)`.
3. Implement `CommandRegistry.cs` and register `print_custom(message)`.

Verification: Execute `print_custom("Test")` via the captured script and verify output in the game console.

### [ ] Step: Frontend - Basic UI & Monaco
Create the WinForms application layout and integrate the SynMonaco editor.

1. Create the `Frontend` project and `MainForm.cs`.
2. Implement the Synapse X-style layout (Tabs, Sidebar, Buttons).
3. Integrate SynMonaco using a `WebBrowser` or `WebView2` control.

Verification: Launch the UI and verify that the editor loads and buttons are visible.

### [ ] Step: Frontend - IPC Client & Integration
Implement the IPC client and connect the UI "Execute" button to the Backend.

1. Implement `PipeClient.cs` to connect to `TheBadPlace_Executor_Pipe`.
2. Link the "Execute" button to send the editor's text via the IPC client.
3. Final end-to-end testing of the full execution flow.

Verification: Type `print_custom("Hello from UI")` in the editor, click Execute, and see the message in the Polytoria console.
