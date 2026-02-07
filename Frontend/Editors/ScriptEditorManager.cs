using Avalonia.Controls;
using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Highlighting.Xshd;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace Frontend.Editors;

public class ScriptEditorManager
{
    public TabControl TabControl { get; }
    private List<ScriptEditorTab> editorTabs = new List<ScriptEditorTab>();
    private IHighlightingDefinition? luaHighlighting;

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
            FontFamily = new FontFamily("Consolas"),
            FontSize = 14,
            ShowLineNumbers = true,
            LineNumbersForeground = new SolidColorBrush(Color.Parse("#858585")),
            Document = new TextDocument(),
            SyntaxHighlighting = luaHighlighting
        };

        var tabItem = new TabItem
        {
            Header = title,
            Content = editor
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
}
