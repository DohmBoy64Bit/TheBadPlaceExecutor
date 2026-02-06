# Product Requirements Document (PRD) - The Bad Place Executor

## 1. Introduction
**The Bad Place Executor** is a cheat/utility mod for the Polytoria game client (Unity/IL2CPP). It aims to provide a scripting environment similar to professional Roblox executors, allowing users to run custom Lua scripts within the game's internal MoonSharp engine.

## 2. Project Goals
- Provide a robust external UI for script execution.
- Maintain strict separation of concerns between game logic and UI logic.
- Support custom Lua commands to extend the game's default API.
- Ensure compatibility with Polytoria's IL2CPP architecture.

## 3. Architecture
The project is divided into four main modules:

### 3.1. Core Backend (DLL)
- **Responsibility**: Injected into the Polytoria process. Handles IL2CPP hooking and MoonSharp script interception.
- **Framework**: C# (MelonLoader for prototyping).
- **Key Tasks**:
    - Locate the active `MoonSharp.Interpreter.Script` instance.
    - Provide a method to execute raw string scripts within that instance.

### 3.2. IPC Bridge
- **Responsibility**: Facilitate communication between the Core Backend and the Frontend UI.
- **Mechanism**: Named Pipes (recommended for Windows-based IPC).
- **Communication Protocol**: Simple string-based or JSON-based messaging to send script content from UI to DLL.

### 3.3. Frontend (UI)
- **Responsibility**: User-facing application for script management and execution.
- **Framework**: Windows Forms (WinForms).
- **Editor**: SynMonaco integration for syntax highlighting and code editing.
- **Functionality**:
    - Script editor with tab support.
    - "Execute" button to send code to the game.
    - "Clear" button to wipe the editor.
    - File operations (Open/Save).

### 3.4. API Extension
- **Responsibility**: Bridging C# methods to the MoonSharp Lua environment.
- **Initial Feature**: Register a `print_custom("msg")` function into the Lua global table.

## 4. Functional Requirements
- **R1: Script Execution**: The user must be able to type Lua code into the UI and have it executed in the game.
- **R2: Custom API**: Custom functions must be available in the game's Lua environment.
- **R3: External UI**: The UI must run as a standalone process, not overlapping with the game window unless requested.
- **R4: Stealth/Stability**: The mod should be stable and avoid crashing the game during script injection or execution.

## 5. Non-Functional Requirements
- **Modularity**: Code must follow strict Separation of Concerns.
- **Portability**: Core Backend should be structured to allow switching from MelonLoader to a Pure C# injector if needed.

## 6. Assumptions & Dependencies
- **Assumption**: Polytoria uses a accessible instance of `MoonSharp.Interpreter.Script`.
- **Dependency**: MelonLoader for initial development and domain handling.
- **Dependency**: SynMonaco for the code editor component.
- **Dependency**: MoonSharp library (expected to be present in game files).
