using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.CodeCompletion;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Highlighting.Xshd;
using Frontend.Scripting;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;

namespace Frontend.Editors;

public class ScriptEditorManager
{
    public TabControl TabControl { get; }
    private List<ScriptEditorTab> editorTabs = new List<ScriptEditorTab>();
    private IHighlightingDefinition? luaHighlighting;
    private ApiParser? apiParser;
    private CompletionWindow? completionWindow;

    public ScriptEditorManager(TabControl tabControl)
    {
        TabControl = tabControl;
        LoadLuaSyntaxHighlighting();
    }

    private void LoadLuaSyntaxHighlighting()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "Frontend.Resources.SyntaxHighlighting.Lua.xshd";
            
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    using (var reader = new XmlTextReader(stream))
                    {
                        luaHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                    }
                }
            }
        }
        catch
        {
        }
    }

    public void CreateNewTab(string title)
    {
        var editor = new TextEditor
        {
            Background = new SolidColorBrush(Color.Parse("#1E1E1E")),
            Foreground = new SolidColorBrush(Color.Parse("#DCDCDC")),
            FontFamily = new FontFamily("Consolas,Cascadia Code,Courier New"),
            FontSize = 14,
            ShowLineNumbers = true,
            LineNumbersForeground = new SolidColorBrush(Color.Parse("#858585")),
            Document = new TextDocument(),
            SyntaxHighlighting = luaHighlighting,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch
        };

        editor.TextArea.SelectionBrush = new SolidColorBrush(Color.Parse("#264F78"));
        editor.TextArea.SelectionForeground = null;
        editor.TextArea.Caret.CaretBrush = new SolidColorBrush(Color.Parse("#AEAFAD"));

        editor.TextArea.TextEntered += OnTextEntered;

        var tabItem = new TabItem
        {
            Header = title,
            Content = editor,
            FontSize = 12,
            FontWeight = Avalonia.Media.FontWeight.SemiBold,
            MinHeight = 30,
            Padding = new Avalonia.Thickness(10, 0)
        };

        var editorTab = new ScriptEditorTab(editor, tabItem);
        editorTabs.Add(editorTab);

        TabControl.Items.Add(tabItem);
        TabControl.SelectedItem = tabItem;
    }

    public string GetEditorText()
    {
        var activeEditor = GetActiveEditor();
        return activeEditor?.Document.Text ?? "";
    }

    public void SetEditorText(string text)
    {
        var activeEditor = GetActiveEditor();
        if (activeEditor != null)
        {
            activeEditor.Document.Text = text;
        }
    }

    public TextEditor? GetActiveEditor()
    {
        if (TabControl.SelectedItem is TabItem selectedTab)
        {
            var editorTab = editorTabs.FirstOrDefault(et => et.TabItem == selectedTab);
            return editorTab?.Editor;
        }
        return null;
    }

    public void LoadCompletions(ApiParser parser)
    {
        apiParser = parser;
    }

    private void OnTextEntered(object? sender, TextInputEventArgs e)
    {
        if (apiParser == null || sender is not TextArea textArea)
            return;

        if (e.Text == ".")
        {
            ShowMemberCompletions(textArea);
        }
        else if (!string.IsNullOrWhiteSpace(e.Text) && char.IsLetter(e.Text[0]))
        {
            ShowGlobalCompletions(textArea);
        }
    }

    private void ShowMemberCompletions(TextArea textArea)
    {
        if (apiParser == null)
            return;

        var line = textArea.Document.GetLineByNumber(textArea.Caret.Line);
        var lineText = textArea.Document.GetText(line.Offset, textArea.Caret.Offset - line.Offset);

        var match = Regex.Match(lineText, @"(\w+)\.$");
        if (!match.Success)
            return;

        string tokenBeforeDot = match.Groups[1].Value;
        string? className = GetClassNameForToken(tokenBeforeDot);

        if (className == null)
            return;

        var completions = new List<ICompletionData>();

        var properties = apiParser.GetAllProperties(className);
        foreach (var prop in properties)
        {
            string rw = (prop.CanRead && prop.CanWrite) ? "read/write" : (prop.CanRead ? "read-only" : "write-only");
            completions.Add(new LuaCompletionData(
                prop.Name,
                $"{prop.Name} : {prop.Type}",
                $"{rw} property of type {prop.Type}",
                1.0
            ));
        }

        var methods = apiParser.GetAllMethods(className);
        foreach (var method in methods)
        {
            string paramsStr = string.Join(", ", method.Parameters.Select(p => $"{p.Name}: {p.Type}"));
            completions.Add(new LuaCompletionData(
                method.Name,
                $"{method.Name}({paramsStr})",
                $"Method returns {method.ReturnType}",
                2.0
            ));
        }

        var events = apiParser.GetAllEvents(className);
        foreach (var ev in events)
        {
            completions.Add(new LuaCompletionData(
                ev.Name,
                $"{ev.Name} (event)",
                $"Event - use {ev.Name}:Connect(function() end)",
                3.0
            ));
        }

        if (completions.Count > 0)
        {
            ShowCompletionWindow(textArea, completions);
        }
    }

    private void ShowGlobalCompletions(TextArea textArea)
    {
        if (apiParser == null)
            return;

        var line = textArea.Document.GetLineByNumber(textArea.Caret.Line);
        var lineText = textArea.Document.GetText(line.Offset, textArea.Caret.Offset - line.Offset);

        var match = Regex.Match(lineText, @"(\w+)$");
        if (!match.Success || match.Groups[1].Value.Length < 2)
            return;

        string currentWord = match.Groups[1].Value;
        var completions = new List<ICompletionData>();

        var globalObjects = new Dictionary<string, string>
        {
            { "game", "The game environment (equivalent to workspace)" },
            { "workspace", "The game environment (alias for game)" },
            { "script", "Reference to the current script" },
            { "Vector3", "Vector3 class for 3D vectors" },
            { "Vector2", "Vector2 class for 2D vectors" },
            { "Vector4", "Vector4 class for 4D vectors" },
            { "Color3", "Color3 class for RGB colors" },
            { "Quaternion", "Quaternion class for rotations" },
            { "Bounds", "Bounds class for bounding boxes" },
            { "JSON", "JSON encoding/decoding" }
        };

        var executorFunctions = new Dictionary<string, string>
        {
            { "readfile", "Read file contents from workspace folder" },
            { "writefile", "Write content to a file in workspace folder" },
            { "appendfile", "Append content to a file" },
            { "isfile", "Check if path is a file" },
            { "isfolder", "Check if path is a folder" },
            { "makefolder", "Create a folder" },
            { "delfile", "Delete a file" },
            { "delfolder", "Delete a folder" },
            { "listfiles", "List files in a folder" },
            { "getclipboard", "Get clipboard contents" },
            { "setclipboard", "Set clipboard contents" },
            { "toclipboard", "Alias for setclipboard" }
        };

        foreach (var global in globalObjects)
        {
            if (global.Key.StartsWith(currentWord, System.StringComparison.OrdinalIgnoreCase))
            {
                completions.Add(new LuaCompletionData(
                    global.Key,
                    global.Key,
                    global.Value,
                    0.5
                ));
            }
        }

        foreach (var func in executorFunctions)
        {
            if (func.Key.StartsWith(currentWord, System.StringComparison.OrdinalIgnoreCase))
            {
                completions.Add(new LuaCompletionData(
                    func.Key,
                    func.Key,
                    func.Value,
                    1.5
                ));
            }
        }

        if (completions.Count > 0)
        {
            int wordStartOffset = textArea.Caret.Offset - currentWord.Length;
            ShowCompletionWindow(textArea, completions, wordStartOffset, currentWord.Length);
        }
    }

    private string? GetClassNameForToken(string token)
    {
        var tokenClassMap = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase)
        {
            { "game", "Environment" },
            { "workspace", "Environment" },
            { "script", "BaseScript" },
            { "Vector3", "Vector3" },
            { "Vector2", "Vector2" },
            { "Vector4", "Vector4" },
            { "Color3", "Color3" },
            { "Quaternion", "Quaternion" },
            { "Bounds", "Bounds" },
            { "JSON", "JSON" }
        };

        if (tokenClassMap.TryGetValue(token, out var className))
        {
            return className;
        }

        if (apiParser != null && apiParser.Classes.ContainsKey(token))
        {
            return token;
        }

        return null;
    }

    private void ShowCompletionWindow(TextArea textArea, List<ICompletionData> completions, int? startOffset = null, int? length = null)
    {
        if (completionWindow != null)
        {
            completionWindow.Close();
        }

        completionWindow = new CompletionWindow(textArea);
        
        if (startOffset.HasValue && length.HasValue)
        {
            completionWindow.StartOffset = startOffset.Value;
            completionWindow.EndOffset = startOffset.Value + length.Value;
        }

        foreach (var completion in completions.OrderBy(c => c.Priority).ThenBy(c => c.Text))
        {
            completionWindow.CompletionList.CompletionData.Add(completion);
        }

        completionWindow.Closed += (o, args) => completionWindow = null;
        completionWindow.Show();
    }
}
