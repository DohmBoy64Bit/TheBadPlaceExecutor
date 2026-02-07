# Technical Specification: AvaloniaEdit Integration for TheBadPlace

## Task Complexity: HARD

**Rationale**: This is a complex architectural change that involves:
- Complete replacement of a core UI component (WebView → AvaloniaEdit)
- Custom autocomplete implementation for Lua syntax
- Maintaining visual consistency with the current design
- Integration with multiple existing systems
- Cross-platform compatibility requirements

---

## Technical Context

### Current Stack
- **Framework**: Avalonia UI 11.0.10 on .NET 9.0 (Windows x64)
- **Current Editor**: WebView with Ace Editor (Chromium-based)
- **Language**: C# with Lua scripting support
- **Target Platform**: Windows (cross-platform capable)

### Current Dependencies
```xml
<PackageReference Include="Avalonia" Version="11.0.10" />
<PackageReference Include="Avalonia.Desktop" Version="11.0.10" />
<PackageReference Include="Avalonia.Themes.Fluent" Version="11.0.10" />
<PackageReference Include="WebViewControl-Avalonia" Version="3.120.11" />
```

### Files to Modify
1. **[./Frontend/Frontend.csproj](./Frontend/Frontend.csproj)** - Add AvaloniaEdit package, remove WebView
2. **[./Frontend/MainWindow.axaml](./Frontend/MainWindow.axaml)** - Update XAML to remove WebView namespace
3. **[./Frontend/MainWindow.axaml.cs](./Frontend/MainWindow.axaml.cs)** - Simplify to delegate to ScriptEditorManager

### Files to Create
1. **[./Frontend/Editors/ScriptEditorManager.cs](./Frontend/Editors/ScriptEditorManager.cs)** - Core editor management
2. **[./Frontend/Editors/ScriptEditorTab.cs](./Frontend/Editors/ScriptEditorTab.cs)** - Tab wrapper class (optional but recommended)
3. **[./Frontend/Editors/LuaCompletionData.cs](./Frontend/Editors/LuaCompletionData.cs)** - Custom completion data for autocomplete

### Files to Remove (Post-Migration)
- `Frontend/Editor/SynMonaco/` directory (Ace Editor HTML/JS files) - can be removed after verification

---

## Implementation Approach

### 1. Architecture Overview

```
MainWindow (UI Controller)
    ├─ Handles UI events (buttons, script list)
    ├─ Delegates editor operations to ScriptEditorManager
    └─ Maintains existing IPC, FileSystemWatcher logic

ScriptEditorManager (Editor Orchestrator)
    ├─ Manages TabControl and TextEditor instances
    ├─ Creates/destroys editor tabs
    ├─ Applies syntax highlighting (Lua)
    ├─ Implements autocomplete integration
    └─ Provides GetEditorText/SetEditorText APIs

ScriptEditorTab (Data Model)
    ├─ Wraps TextEditor instance
    ├─ Tracks tab metadata (filename, dirty state)
    └─ Associated TabItem reference

ApiParser (Existing, Unchanged)
    └─ Supplies autocomplete data from ReflectionData.xml
```

### 2. Component Responsibilities

| Component | Responsibility |
|-----------|---------------|
| **MainWindow** | Layout, button handling, script list, file dialogs, pipe status checking |
| **ScriptEditorManager** | Tab creation, editor lifecycle, syntax highlighting, autocomplete setup, text get/set |
| **ScriptEditorTab** | Encapsulates editor instance + metadata (filename, dirty flag, TabItem) |
| **LuaCompletionData** | Implements `ICompletionData` for AvaloniaEdit autocomplete popup |
| **ApiParser** | (No changes) Parses ReflectionData.xml, provides API data |
| **PipeClient** | (No changes) Sends scripts to executor |
| **EnvironmentUtils** | (No changes) Manages folder paths |

### 3. Key Features & Implementation Details

#### 3.1 Package Update
**Remove**: `WebViewControl-Avalonia` (3.120.11)  
**Add**: `Avalonia.AvaloniaEdit` (11.4.1 - latest stable compatible with Avalonia 11.0.10)

```xml
<PackageReference Include="Avalonia.AvaloniaEdit" Version="11.4.1" />
```

#### 3.2 Tabbed Editor System
- Each tab contains a unique `TextEditor` instance
- Tab header shows filename or default title ("Script 1", "Script 2", etc.)
- Switching tabs updates the active editor
- ScriptEditorManager tracks active editor via `TabControl.SelectedItem`

#### 3.3 Syntax Highlighting
AvaloniaEdit uses XSHD (XML Syntax Highlighting Definition) files for syntax highlighting.

**Options**:
1. **Built-in Lua** (if available) - Use `HighlightingManager.Instance.GetDefinition("Lua")`
2. **Custom XSHD** - Create `Resources/SyntaxHighlighting/Lua.xshd` based on Lua syntax

**Implementation**:
```csharp
var highlighting = HighlightingManager.Instance.GetDefinition("Lua");
if (highlighting != null)
{
    editor.SyntaxHighlighting = highlighting;
}
```

**Fallback**: If built-in Lua highlighting is not available, create a custom XSHD file with Lua keywords, operators, and comments.

#### 3.4 Autocomplete System

**Challenge**: AvaloniaEdit doesn't have Monaco's autocomplete system. We need to implement it manually.

**Approach**:
1. Hook into `TextArea.TextEntered` event
2. Detect trigger characters (`.` for member access, letters for global objects)
3. Parse current line/context to determine what to suggest
4. Show `CompletionWindow` with filtered suggestions
5. Use existing `ApiParser` to get class members, methods, properties, events

**Components**:
- `LuaCompletionData` class implementing `AvaloniaEdit.CodeCompletion.ICompletionData`
- Completion logic in `ScriptEditorManager`
- Parsing current editor context to determine appropriate suggestions

**Example Flow**:
```
User types: "game."
    → Detect '.' trigger
    → Parse token before '.': "game"
    → Lookup "game" type in ApiParser (type: "Environment")
    → Get all properties/methods for "Environment" class
    → Create CompletionWindow with filtered suggestions
    → User selects "Workspace" → insert "Workspace"
```

#### 3.5 Text Get/Set API

**ScriptEditorManager API**:
```csharp
public class ScriptEditorManager
{
    private TabControl tabControl;
    private ApiParser? apiParser;

    public ScriptEditorManager(TabControl tabControl)
    {
        this.tabControl = tabControl;
    }

    // Tab Management
    public void CreateNewTab(string title) { }
    public void CloseTab(TabItem tab) { }

    // Text Operations
    public string GetEditorText() { } // Returns text of active editor
    public void SetEditorText(string text) { } // Sets text of active editor
    public TextEditor? GetActiveEditor() { } // Returns active TextEditor instance

    // Autocomplete
    public void LoadCompletions(ApiParser parser) { } // Sets up autocomplete
}
```

**Comparison with Current WebView Implementation**:

| Operation | Current (WebView) | New (AvaloniaEdit) |
|-----------|-------------------|-------------------|
| Get Text | `webView.EvaluateScript<string>("editor.getValue();")` | `editor.Document.Text` |
| Set Text | `webView.ExecuteScript($"editor.setValue({json});")` | `editor.Document.Text = text;` |
| Async | Yes (JS bridge) | No (direct property access) |

#### 3.6 Visual Consistency

**Current Design**:
- Dark theme (`#1E1E1E` background)
- Tab control with transparent background
- Border with `#44FFFFFF` color
- Corner radius of 4px

**AvaloniaEdit Styling**:
```xaml
<Style Selector="avaloniaEdit|TextEditor">
    <Setter Property="Background" Value="#1E1E1E"/>
    <Setter Property="Foreground" Value="#DCDCDC"/>
    <Setter Property="FontFamily" Value="Consolas"/>
    <Setter Property="FontSize" Value="14"/>
    <Setter Property="ShowLineNumbers" Value="True"/>
    <Setter Property="LineNumbersForeground" Value="#858585"/>
</Style>
```

**Key Visual Elements to Maintain**:
- Dark background (`#1E1E1E`)
- Light foreground text (`#DCDCDC`)
- Line numbers with muted color (`#858585`)
- Monospace font (Consolas or Cascadia Code)
- Same border/corner radius as current design

#### 3.7 Script Execution Flow

**No changes required** - execution flow remains identical:
```
ExecuteBtn.Click → GetEditorText() → PipeClient.SendScript(script)
```

The only difference is the internal implementation of `GetEditorText()`:
- **Before**: Async JS bridge call
- **After**: Direct property access (synchronous)

---

## Data Model Changes

### New Classes

#### ScriptEditorTab
```csharp
public class ScriptEditorTab
{
    public string FileName { get; set; } = string.Empty;
    public bool IsModified { get; set; } = false;
    public TextEditor Editor { get; set; }
    public TabItem TabItem { get; set; }
}
```

#### LuaCompletionData
```csharp
public class LuaCompletionData : ICompletionData
{
    public string Text { get; set; } // "Workspace"
    public object Content { get; set; } // Display text
    public object Description { get; set; } // Documentation
    public double Priority { get; set; } // Sort priority
    public IBitmap Image { get; set; } // Icon (optional)

    public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
    {
        // Insert completion text
    }
}
```

### Modified Classes

**MainWindow.axaml.cs**:
- Remove `GetEditorText()`, `SetEditorText()`, `CreateNewTab()` methods
- Add `ScriptEditorManager editorManager;` field
- Delegate all editor operations to `editorManager`

---

## API/Interface Changes

### Public API of ScriptEditorManager

```csharp
public class ScriptEditorManager
{
    // Constructor
    public ScriptEditorManager(TabControl tabControl);

    // Tab Management
    public void CreateNewTab(string title);
    public void CloseCurrentTab();
    public int TabCount { get; }

    // Text Operations (operate on active tab)
    public string GetEditorText();
    public void SetEditorText(string text);
    public TextEditor? GetActiveEditor();

    // Autocomplete
    public void LoadCompletions(ApiParser parser);

    // Events
    public event EventHandler<TextChangedEventArgs>? TextChanged;
    public event EventHandler<EventArgs>? TabChanged;
}
```

### Changes to MainWindow

**Constructor**:
```csharp
public MainWindow()
{
    InitializeComponent();
    EnvironmentUtils.InitializeFolders();
    
    // NEW: Create editor manager
    editorManager = new ScriptEditorManager(EditorTabs);
    
    InitializeAutocomplete();
    
    // NEW: Pass autocomplete to manager
    if (apiParser != null)
    {
        editorManager.LoadCompletions(apiParser);
    }
    
    SetupEvents();
    LoadScripts();
    SetupFileSystemWatcher();
    SetupStatusTimer();
    
    // NEW: Use manager to create tab
    editorManager.CreateNewTab("Script 1");
}
```

**Button Events**:
```csharp
ExecuteBtn.Click += async (s, e) => {
    string script = editorManager.GetEditorText(); // Changed from await GetEditorText()
    if (!string.IsNullOrEmpty(script)) {
        await PipeClient.SendScript(script);
    }
};

ClearBtn.Click += (s, e) => editorManager.SetEditorText("");

OptionsBtn.Click += (s, e) => editorManager.CreateNewTab($"Script {editorManager.TabCount + 1}");
```

---

## Verification Approach

### 1. Build Verification
```bash
dotnet restore Frontend/Frontend.csproj
dotnet build Frontend/Frontend.csproj --configuration Release
```

### 2. Functional Testing

| Feature | Test Case | Expected Result |
|---------|-----------|-----------------|
| **Tab Creation** | Click "+" button | New tab created with default title |
| **Syntax Highlighting** | Type Lua code | Keywords highlighted in appropriate colors |
| **Autocomplete** | Type `game.` | Popup shows Environment members |
| **Script Execution** | Write script, click Execute | Script sent to pipe, no errors |
| **File Open** | Open File dialog, select .lua | Content loads in active tab |
| **File Save** | Save File dialog, enter name | File saved with editor content |
| **Script List** | Click script in sidebar | Content loads in active tab |
| **Tab Switching** | Create 2 tabs, switch between | Each tab retains independent content |
| **Status Indicator** | Start/stop executor | Status dot changes (green/red) |

### 3. Visual Testing
- Compare side-by-side with current WebView version
- Verify dark theme consistency
- Check font rendering (Consolas monospace)
- Verify line numbers appearance
- Test tab header styling

### 4. Performance Testing
- Open 10+ tabs, verify responsiveness
- Load large script files (1000+ lines)
- Test autocomplete response time
- Monitor memory usage vs WebView version

### 5. Cross-Platform Testing (Future)
- Test on Linux (after removing Windows-specific runtime identifier)
- Test on macOS
- Verify syntax highlighting works on all platforms

---

## Risk Assessment & Mitigation

### Risks

| Risk | Impact | Likelihood | Mitigation |
|------|--------|------------|------------|
| **Autocomplete complexity** | High | Medium | Start with basic autocomplete, iterate |
| **Visual inconsistency** | Medium | Low | Use XAML styling to match current design |
| **Performance regression** | Medium | Low | AvaloniaEdit is native, should be faster |
| **Missing Lua highlighting** | Low | Medium | Create custom XSHD if built-in unavailable |
| **Breaking existing workflows** | High | Low | Maintain exact same API surface |

### Migration Strategy

1. **Phase 1**: Add AvaloniaEdit alongside WebView (both packages present)
2. **Phase 2**: Implement ScriptEditorManager with basic functionality
3. **Phase 3**: Replace WebView in MainWindow with ScriptEditorManager calls
4. **Phase 4**: Test all features, fix issues
5. **Phase 5**: Remove WebView package and Ace Editor files

---

## Optional Enhancements (Out of Scope for Initial Implementation)

1. **Dirty State Tracking**: Track unsaved changes per tab (asterisk in tab header)
2. **Undo/Redo**: Built-in AvaloniaEdit support (verify it works)
3. **Syntax Error Markers**: Underline/highlight Lua syntax errors
4. **Breakpoint Support**: Visual markers in gutter
5. **Code Folding**: Collapse/expand code blocks
6. **Search/Replace**: Built-in AvaloniaEdit feature (verify UI)
7. **Multi-cursor Editing**: AvaloniaEdit may support this
8. **Themes**: Support light/dark theme switching
9. **Tab Close Buttons**: Add 'X' button to each tab

---

## Dependencies & Compatibility

### Package Compatibility
- **Avalonia.AvaloniaEdit 11.4.1** is compatible with **Avalonia 11.0.10**
- Both target .NET 6.0+, current project uses .NET 9.0 ✅
- No conflicts with existing packages

### Platform Compatibility
- Current: Windows x64 only (`<RuntimeIdentifier>win-x64</RuntimeIdentifier>`)
- AvaloniaEdit: Cross-platform (Windows, Linux, macOS)
- Future: Remove RuntimeIdentifier to enable cross-platform builds

---

## Implementation Checklist

### Prerequisites
- [ ] Review AvaloniaEdit documentation
- [ ] Test AvaloniaEdit sample project
- [ ] Confirm Lua syntax highlighting availability

### Phase 1: Setup
- [ ] Add `Avalonia.AvaloniaEdit` package reference
- [ ] Create `Frontend/Editors/` directory
- [ ] Create basic `ScriptEditorManager.cs` skeleton
- [ ] Create `LuaCompletionData.cs` class

### Phase 2: Core Implementation
- [ ] Implement tab creation in `ScriptEditorManager`
- [ ] Implement text get/set operations
- [ ] Add syntax highlighting for Lua
- [ ] Create XAML styles for TextEditor

### Phase 3: Autocomplete
- [ ] Implement `TextEntered` event handler
- [ ] Parse context and determine suggestions
- [ ] Create `CompletionWindow` with suggestions
- [ ] Test autocomplete with ApiParser data

### Phase 4: Integration
- [ ] Update `MainWindow.axaml` (remove WebView namespace)
- [ ] Update `MainWindow.axaml.cs` (use ScriptEditorManager)
- [ ] Test all button events
- [ ] Test file open/save
- [ ] Test script list integration

### Phase 5: Testing & Cleanup
- [ ] Run build and verify no errors
- [ ] Test all functional requirements
- [ ] Compare visual appearance with original
- [ ] Remove WebView package
- [ ] Remove `Editor/SynMonaco/` directory
- [ ] Update .gitignore if needed

---

## Success Criteria

1. ✅ All existing features work identically to WebView version
2. ✅ Visual appearance matches current design (dark theme, styling)
3. ✅ Autocomplete provides suggestions based on ApiParser
4. ✅ No WebView or Chromium dependencies remain
5. ✅ Application builds and runs without errors
6. ✅ Performance is equal or better than WebView version
7. ✅ Code is well-organized with clear separation of concerns

---

## References

- [AvaloniaEdit GitHub](https://github.com/AvaloniaUI/AvaloniaEdit)
- [AvaloniaEdit NuGet Package](https://www.nuget.org/packages/Avalonia.AvaloniaEdit)
- [Avalonia UI Documentation](https://docs.avaloniaui.net/)
- [XSHD Syntax Highlighting](https://github.com/icsharpcode/AvalonEdit/wiki/Syntax-Highlighting)
- Current Implementation: [./Frontend/MainWindow.axaml.cs:194-242](./Frontend/MainWindow.axaml.cs:194-242)
