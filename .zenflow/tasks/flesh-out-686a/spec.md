# Technical Specification - The Bad Place Executor

## 1. Technical Context
- **Target Platform**: Windows (Polytoria Client - Unity/IL2CPP)
- **Programming Language**: C#
- **Backend Framework**: MelonLoader (for IL2CPP domain management and Harmony patching)
- **Frontend Framework**: WinForms (.NET 6.0 or higher)
- **Scripting Engine**: MoonSharp (Il2CppMoonSharp)
- **IPC Mechanism**: Named Pipes (System.IO.Pipes)
- **Code Editor**: Ace Editor (SynMonaco wrapper)

## 2. Implementation Approach

### 2.1. Core Backend (DLL)
The Backend will run inside the Polytoria process as a MelonLoader mod.

- **Initialization**:
  - `OnInitializeMelon` will start the IPC Bridge Server thread.
  - Apply Harmony patches to capture the `Script` engine instance.
- **Script Capture**:
  - Prefix patch on `Il2CppMoonSharp.Interpreter.Script.DoString`.
  - Store the `__instance` in a static field for later use.
  - Proactively check `ScriptService.Instance` for existing scripts if possible.
- **Execution Logic**:
  - When a script is received via IPC, it will be executed using `_capturedScript.DoString(code)`.
  - Ensure execution happens on the Unity main thread if required (using MelonLoader's `MelonCoroutines` or `OnUpdate`).

### 2.2. IPC Bridge
A lightweight communication layer using Windows Named Pipes.

- **Pipe Name**: `TheBadPlace_Executor_Pipe`
- **Backend (Server)**:
  - Runs a background thread listening for connections.
  - Reads UTF-8 encoded strings from the pipe.
  - Buffers received scripts for the main thread to execute.
- **Frontend (Client)**:
  - Connects to the pipe when the "Execute" button is clicked.
  - Sends the editor content as a raw string.

### 2.3. Frontend (UI)
A standalone WinForms application designed to mimic Synapse X.

- **Layout Structure**:
  - **Header**: Custom title bar with "The Bad Place Executor" and minimal window controls.
  - **Main Area**: 
    - **Center**: Tabbed interface hosting multiple SynMonaco editor instances.
    - **Right Sidebar**: ListBox or TreeView for a "Scripts" folder browser, allowing quick loading.
  - **Footer**: A horizontal panel containing primary action buttons: `Execute`, `Clear`, `Open File`, `Save File`, `Script Hub`, and `Options`.
- **Theming**: Dark mode (Dracula or Tomorrow Night Eighties) to match the game's aesthetic and professional executor standards.
- **SynMonaco Integration**: Uses a `WebBrowser` control (or `WebView2`) to host `EditorPolytoria.html` from the `SynMonaco` library.
- **Independence**: The UI process does not reference any Unity or Polytoria DLLs.

### 2.4. API Extension (Custom Command Registry)
A dedicated module to extend the Lua environment.

- **Functions to Register**:
  - `print_custom(message)`: Calls `MelonLogger.Msg` and potentially the game's internal `LuaPrint`.
- **Registration**:
  - Performed immediately after capturing a new `Script` instance.
  - Uses `script.Globals.Set(name, DynValue.NewCallback(delegate))`.

## 3. Source Code Structure Changes

```text
BadPlaceExecutor/ (Solution)
├── CoreBackend/ (Project - Class Library)
│   ├── Core.cs (MelonMod entry point)
│   ├── IPC/
│   │   └── PipeServer.cs (Named Pipe listener)
│   ├── Scripting/
│   │   ├── ScriptEngineCapture.cs (Harmony patches)
│   │   └── CommandRegistry.cs (Custom Lua functions)
│   └── Utils/
│       └── UnityMainThreadDispatcher.cs
└── Frontend/ (Project - WinForms)
    ├── Program.cs
    ├── MainForm.cs
    ├── IPC/
    │   └── PipeClient.cs
    └── Editor/
        └── MonacoHost.cs (Wrapper for SynMonaco)
```

## 4. Delivery Phases

### Phase 1: Core & Capture
- Implement `Core.cs` with Harmony patches.
- Verify `Script` instance capture via logs.
- Register `print_custom` and test with an in-game script.

### Phase 2: IPC & Execution
- Implement `PipeServer` in Backend and `PipeClient` in Frontend.
- Enable basic "string sending" from UI to Backend.
- Implement main-thread script execution.

### Phase 3: UI & Editor
- Build the WinForms UI layout.
- Integrate Ace Editor (SynMonaco).
- Connect the "Execute" button to the `PipeClient`.

## 5. Verification Approach
- **Logging**: Use `MelonLogger` for Backend debugging.
- **Lua Testing**: Execute `print_custom("Hello from Backend")` from an in-game script to verify API injection.
- **IPC Testing**: Send a simple `print("Hello from UI")` from the WinForms app and verify it appears in the Polytoria console.
