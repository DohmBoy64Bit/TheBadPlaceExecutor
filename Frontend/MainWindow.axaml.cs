using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Frontend.IPC;
using Frontend.Utils;
using Frontend.Scripting;
using WebViewControl;

namespace Frontend;

public partial class MainWindow : Window
{
    private ApiParser apiParser = new ApiParser();
    private string? cachedCompletionsJs;
    private string? cachedHighlightConfigJson;
    private DispatcherTimer? statusTimer;

    public MainWindow()
    {
        InitializeComponent();
        EnvironmentUtils.InitializeFolders();
        InitializeAutocomplete();
        SetupEvents();
        LoadScripts();
        SetupFileSystemWatcher();
        SetupStatusTimer();
        
        // Initial Tab
        CreateNewTab("Script 1");
    }

    private void InitializeAutocomplete()
    {
        string xmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ReflectionData.xml");
        if (File.Exists(xmlPath))
        {
            if (apiParser.LoadFromFile(xmlPath))
            {
                cachedCompletionsJs = AutocompleteGenerator.GeneratePolytoriaCompletions(apiParser);
                cachedHighlightConfigJson = AutocompleteGenerator.GenerateHighlightConfig(apiParser);
            }
        }
    }

    private void SetupStatusTimer()
    {
        statusTimer = new DispatcherTimer();
        statusTimer.Interval = TimeSpan.FromSeconds(1);
        statusTimer.Tick += (s, e) => {
            bool isConnected = false;
            try {
                isConnected = Directory.GetFiles(@"\\.\pipe\").Contains(@"\\.\pipe\TheBadPlace_Executor_Pipe");
            } catch { }

            if (isConnected) {
                StatusDot.Fill = Brushes.LimeGreen;
                StatusText.Text = "ATTACHED";
            } else {
                StatusDot.Fill = Brushes.OrangeRed;
                StatusText.Text = "NOT ATTACHED";
            }
        };
        statusTimer.Start();
    }

    private void SetupFileSystemWatcher()
    {
        FileSystemWatcher watcher = new FileSystemWatcher(EnvironmentUtils.ScriptsPath);
        watcher.Filter = "*.*";
        watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;
        watcher.EnableRaisingEvents = true;
        
        watcher.Created += (s, e) => Dispatcher.UIThread.InvokeAsync(LoadScripts);
        watcher.Deleted += (s, e) => Dispatcher.UIThread.InvokeAsync(LoadScripts);
        watcher.Renamed += (s, e) => Dispatcher.UIThread.InvokeAsync(LoadScripts);
    }

    private void LoadScripts()
    {
        ScriptList.Items.Clear();
        if (Directory.Exists(EnvironmentUtils.ScriptsPath)) {
            foreach (string file in Directory.GetFiles(EnvironmentUtils.ScriptsPath)) {
                ScriptList.Items.Add(Path.GetFileName(file));
            }
        }
    }

    private void SetupEvents()
    {
        ExecuteBtn.Click += async (s, e) => {
            string script = await GetEditorText();
            if (!string.IsNullOrEmpty(script)) {
                if (script.StartsWith("\"") && script.EndsWith("\"")) {
                    script = JsonSerializer.Deserialize<string>(script) ?? script;
                }
                await PipeClient.SendScript(script);
            }
        };

        ClearBtn.Click += (s, e) => SetEditorText("");

        ScriptList.SelectionChanged += async (s, e) => {
            if (ScriptList.SelectedItem != null) {
                string fileName = ScriptList.SelectedItem.ToString();
                string filePath = Path.Combine(EnvironmentUtils.ScriptsPath, fileName);
                if (File.Exists(filePath)) {
                    string content = File.ReadAllText(filePath);
                    if (EditorTabs.SelectedItem is TabItem selectedTab) {
                        selectedTab.Header = fileName;
                        SetEditorText(content);
                    }
                }
            }
        };

        OptionsBtn.Click += (s, e) => CreateNewTab("Script " + (EditorTabs.Items.Count + 1));

        OpenFileBtn.Click += async (s, e) => {
            var dialog = new OpenFileDialog();
            dialog.Directory = EnvironmentUtils.ScriptsPath;
            dialog.Filters.Add(new FileDialogFilter() { Name = "Lua Files", Extensions = { "lua", "txt" } });
            
            var result = await dialog.ShowAsync(this);
            if (result != null && result.Length > 0) {
                string content = File.ReadAllText(result[0]);
                SetEditorText(content);
            }
        };

        SaveFileBtn.Click += async (s, e) => {
            var dialog = new SaveFileDialog();
            dialog.Directory = EnvironmentUtils.ScriptsPath;
            dialog.Filters.Add(new FileDialogFilter() { Name = "Lua Files", Extensions = { "lua", "txt" } });
            
            var result = await dialog.ShowAsync(this);
            if (result != null) {
                string content = await GetEditorText();
                if (content.StartsWith("\"") && content.EndsWith("\"")) {
                    content = JsonSerializer.Deserialize<string>(content) ?? content;
                }
                File.WriteAllText(result, content);
            }
        };
    }

    private async Task<string> GetEditorText()
    {
        if (EditorTabs.SelectedItem is TabItem selectedTab && selectedTab.Content is WebView webView) {
            try {
                // WebViewControl-Avalonia often uses ExecuteScript
                return await Task.Run(() => webView.EvaluateScript<string>("editor.getValue();"));
            } catch { }
        }
        return "";
    }

    private void SetEditorText(string text)
    {
        if (EditorTabs.SelectedItem is TabItem selectedTab && selectedTab.Content is WebView webView) {
            try {
                string escapedText = JsonSerializer.Serialize(text);
                webView.ExecuteScript($"editor.setValue({escapedText});");
            } catch { }
        }
    }

    private void CreateNewTab(string title)
    {
        var webView = new WebView();
        var tabItem = new TabItem {
            Header = title,
            Content = webView
        };

        string htmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Editor", "SynMonaco", "EditorPolytoria.html");
        if (File.Exists(htmlPath)) {
            webView.LoadUrl("file:///" + htmlPath.Replace("\\", "/"));
        }

        webView.WebViewInitialized += async () => {
            if (!string.IsNullOrEmpty(cachedCompletionsJs)) {
                webView.ExecuteScript(cachedCompletionsJs);
            }
            if (!string.IsNullOrEmpty(cachedHighlightConfigJson)) {
                webView.ExecuteScript($"setHighlightingConfig({cachedHighlightConfigJson});");
            }
        };

        EditorTabs.Items.Add(tabItem);
        EditorTabs.SelectedItem = tabItem;
    }
}
