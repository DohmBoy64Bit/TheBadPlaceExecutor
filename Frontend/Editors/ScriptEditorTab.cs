using Avalonia.Controls;
using AvaloniaEdit;

namespace Frontend.Editors;

public class ScriptEditorTab
{
    public string FileName { get; set; } = "";
    public bool IsModified { get; set; } = false;
    public TextEditor Editor { get; set; }
    public TabItem TabItem { get; set; }

    public ScriptEditorTab(TextEditor editor, TabItem tabItem)
    {
        Editor = editor;
        TabItem = tabItem;
    }
}
