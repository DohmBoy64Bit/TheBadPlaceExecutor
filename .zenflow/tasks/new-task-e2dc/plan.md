# Spec and build

## Configuration
- **Artifacts Path**: {@artifacts_path} → `.zenflow/tasks/{task_id}`

---

## Agent Instructions

Ask the user questions when anything is unclear or needs their input. This includes:
- Ambiguous or incomplete requirements
- Technical decisions that affect architecture or user experience
- Trade-offs that require business context

Do not make assumptions on important decisions — get clarification first.

---

## Workflow Steps

### [x] Step: Technical Specification

Assess the task's difficulty, as underestimating it leads to poor outcomes.
- easy: Straightforward implementation, trivial bug fix or feature
- medium: Moderate complexity, some edge cases or caveats to consider
- hard: Complex logic, many caveats, architectural considerations, or high-risk changes

Create a technical specification for the task that is appropriate for the complexity level:
- Review the existing codebase architecture and identify reusable components.
- Define the implementation approach based on established patterns in the project.
- Identify all source code files that will be created or modified.
- Define any necessary data model, API, or interface changes.
- Describe verification steps using the project's test and lint commands.

Save the output to `{@artifacts_path}/spec.md` with:
- Technical context (language, dependencies)
- Implementation approach
- Source code structure changes
- Data model / API / interface changes
- Verification approach

If the task is complex enough, create a detailed implementation plan based on `{@artifacts_path}/spec.md`:
- Break down the work into concrete tasks (incrementable, testable milestones)
- Each task should reference relevant contracts and include verification steps
- Replace the Implementation step below with the planned tasks

Rule of thumb for step size: each step should represent a coherent unit of work (e.g., implement a component, add an API endpoint, write tests for a module). Avoid steps that are too granular (single function).

Important: unit tests must be part of each implementation task, not separate tasks. Each task should implement the code and its tests together, if relevant.

Save to `{@artifacts_path}/plan.md`. If the feature is trivial and doesn't warrant this breakdown, keep the Implementation step below as is.

---

### [x] Step: Setup and Dependencies
<!-- chat-id: 377fe321-506b-4140-9597-ed5fe0126330 -->

Add AvaloniaEdit package and create project structure for the new editor system.

**Tasks**:
- Add `Avalonia.AvaloniaEdit` package reference (version 11.4.1) to Frontend.csproj
- Create `Frontend/Editors/` directory structure
- Verify the package is compatible with existing Avalonia 11.0.10 packages
- Build the project to ensure no dependency conflicts

**Verification**:
- Run `dotnet restore Frontend/Frontend.csproj`
- Run `dotnet build Frontend/Frontend.csproj` - should complete without errors

---

### [x] Step: Create ScriptEditorManager Core
<!-- chat-id: eb67d327-bc47-492d-ad5e-571f071efaca -->

Implement the core ScriptEditorManager class with tab management and text operations.

**Tasks**:
- Create `Frontend/Editors/ScriptEditorManager.cs` with:
  - Constructor accepting TabControl parameter
  - `CreateNewTab(string title)` method - creates TextEditor in new TabItem
  - `GetEditorText()` method - returns text from active editor
  - `SetEditorText(string text)` method - sets text in active editor
  - `GetActiveEditor()` helper method
  - Track editor instances and associate with tabs
- Create `Frontend/Editors/ScriptEditorTab.cs` data model class with:
  - `FileName`, `IsModified`, `Editor`, `TabItem` properties
- Apply basic styling to TextEditor:
  - Dark background (#1E1E1E)
  - Light foreground (#DCDCDC)
  - Monospace font (Consolas, 14px)
  - Show line numbers
  - Line numbers foreground (#858585)

**Verification**:
- Build project successfully
- Create test instance of ScriptEditorManager
- Verify tab creation works
- Verify text get/set operations work

---

### [x] Step: Implement Lua Syntax Highlighting
<!-- chat-id: 897b738d-efe9-4cb7-9bbe-ab48cf0332e7 -->

Add Lua syntax highlighting to the TextEditor instances.

**Tasks**:
- Check if AvaloniaEdit has built-in Lua syntax highlighting
- If available, use `HighlightingManager.Instance.GetDefinition("Lua")`
- If not available, create custom XSHD file:
  - Create `Frontend/Resources/SyntaxHighlighting/Lua.xshd`
  - Define Lua keywords, operators, comments, strings
  - Load custom highlighting definition
- Apply syntax highlighting to all editors created by ScriptEditorManager
- Test with sample Lua code to verify highlighting works

**Verification**:
- Create tab with Lua code
- Verify keywords are highlighted (if, then, else, function, etc.)
- Verify strings and comments are highlighted
- Verify operators are highlighted

---

### [x] Step: Implement Autocomplete System
<!-- chat-id: 88f00d8e-138c-4378-a2e4-9ea638a57d2c -->

Create custom autocomplete functionality integrated with ApiParser.

**Tasks**:
- Create `Frontend/Editors/LuaCompletionData.cs` implementing `ICompletionData`:
  - Properties: `Text`, `Content`, `Description`, `Priority`
  - Implement `Complete()` method to insert text
- In ScriptEditorManager, add `LoadCompletions(ApiParser parser)` method
- Implement `TextArea.TextEntered` event handler:
  - Detect trigger character '.' for member access
  - Parse current line to get token before '.'
  - Lookup token type in ApiParser
  - Get members (properties, methods, events) for that type
  - Show CompletionWindow with suggestions
- Add support for global objects (game, workspace, script, etc.)
- Add support for custom executor functions (readfile, writefile, etc.)

**Verification**:
- Type "game." and verify popup shows Environment members
- Type "workspace." and verify popup shows Environment members
- Type "readfile" and verify autocomplete suggests it
- Select completion and verify it inserts correctly
- Test with multiple class types from ApiParser

---

### [x] Step: Integrate ScriptEditorManager with MainWindow
<!-- chat-id: 8a6785d2-6b2c-4850-acfb-e02fef38b5c4 -->

Replace WebView usage in MainWindow with ScriptEditorManager.

**Tasks**:
- Update `Frontend/MainWindow.axaml`:
  - Remove `xmlns:wv="clr-namespace:WebViewControl;assembly=WebViewControl.Avalonia"` namespace
  - Verify TabControl "EditorTabs" remains unchanged (no content changes needed)
- Update `Frontend/MainWindow.axaml.cs`:
  - Add `private ScriptEditorManager editorManager;` field
  - In constructor, create ScriptEditorManager: `editorManager = new ScriptEditorManager(EditorTabs);`
  - Call `editorManager.LoadCompletions(apiParser);` after InitializeAutocomplete()
  - Replace `CreateNewTab()` calls with `editorManager.CreateNewTab()`
  - Update `ExecuteBtn.Click` to use `editorManager.GetEditorText()` (remove async/await)
  - Update `ClearBtn.Click` to use `editorManager.SetEditorText("")`
  - Update `ScriptList.SelectionChanged` to use `editorManager.SetEditorText(content)`
  - Update `OpenFileBtn.Click` to use `editorManager.SetEditorText(content)`
  - Update `SaveFileBtn.Click` to use `editorManager.GetEditorText()`
  - Update `OptionsBtn.Click` to use `editorManager.CreateNewTab()`
  - Remove old `GetEditorText()`, `SetEditorText()`, `CreateNewTab()` methods

**Verification**:
- Build project successfully
- Run application
- Verify initial tab "Script 1" is created
- Click all buttons and verify functionality
- Test file open/save dialogs
- Test script list selection
- Verify no WebView references remain in code

---

### [ ] Step: Remove WebView Dependencies

Clean up WebView and Ace Editor artifacts.

**Tasks**:
- Remove `WebViewControl-Avalonia` package from Frontend.csproj
- Remove `Frontend/Editor/SynMonaco/` directory (Ace Editor files)
- Update .gitignore if it references WebView-specific paths
- Search codebase for any remaining WebView references
- Remove unused `using WebViewControl;` statement from MainWindow.axaml.cs

**Verification**:
- Run `dotnet clean Frontend/Frontend.csproj`
- Run `dotnet restore Frontend/Frontend.csproj`
- Run `dotnet build Frontend/Frontend.csproj --configuration Release`
- Grep for "WebView" in codebase - should return no results
- Grep for "SynMonaco" in codebase - should return no results (except .gitignore if present)

---

### [ ] Step: Visual Consistency and Styling

Ensure the new AvaloniaEdit implementation matches the current visual design.

**Tasks**:
- Create XAML styles for TextEditor in MainWindow.axaml or App.axaml:
  - Background: #1E1E1E
  - Foreground: #DCDCDC
  - FontFamily: Consolas or Cascadia Code
  - FontSize: 14
  - LineNumbersForeground: #858585
  - ShowLineNumbers: True
- Match tab styling (if needed)
- Test with side-by-side comparison against WebView version (if available)
- Adjust colors/fonts to match exactly

**Verification**:
- Visual inspection: compare with screenshots of old WebView version
- Verify dark theme is consistent
- Verify line numbers appear correctly
- Verify font is monospace and readable
- Verify no UI glitches or rendering issues

---

### [ ] Step: Testing and Final Verification

Comprehensive testing of all features and edge cases.

**Tasks**:
- Test all functional requirements:
  - [ ] Create multiple tabs (5+)
  - [ ] Switch between tabs - each retains content
  - [ ] Type Lua code with syntax highlighting
  - [ ] Test autocomplete with "game.", "workspace.", "Vector3.", etc.
  - [ ] Execute script - verify PipeClient receives it
  - [ ] Clear editor - verify text is cleared
  - [ ] Open file dialog - load .lua file into editor
  - [ ] Save file dialog - save editor content to .lua file
  - [ ] Click script in sidebar - verify content loads
  - [ ] Status indicator - verify it updates (if executor running)
- Test edge cases:
  - [ ] Create tab, type text, switch tab, return - verify text persists
  - [ ] Load large file (1000+ lines) - verify performance
  - [ ] Test autocomplete with many suggestions
  - [ ] Test with special characters in script
- Performance testing:
  - [ ] Measure startup time
  - [ ] Test with 10+ tabs open
  - [ ] Compare memory usage (if possible)

**Verification**:
- All functional tests pass
- No crashes or errors
- Performance is acceptable
- Visual appearance matches requirements

---

### [ ] Step: Final Report

Document the implementation and create final report.

**Tasks**:
- Create `{@artifacts_path}/report.md` with:
  - Summary of what was implemented
  - Key architectural decisions made
  - Testing approach and results
  - Known issues or limitations (if any)
  - Suggestions for future enhancements
  - Before/after comparison (WebView vs AvaloniaEdit)

**Verification**:
- Report is clear and comprehensive
- All implementation steps are documented
- Testing results are included
