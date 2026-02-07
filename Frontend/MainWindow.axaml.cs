using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.IO;
using System.IO.Pipes;
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
        try {
            InitializeComponent();
            EnvironmentUtils.InitializeFolders();
            InitializeAutocomplete();
            SetupEvents();
            LoadScripts();
            SetupFileSystemWatcher();
            SetupStatusTimer();
            
            // Initial Tab
            CreateNewTab("Script 1");
        } catch (Exception ex) {
            LogCrash(ex);
        }
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            this.BeginMoveDrag(e);
        }
    }

    private void LogDebug(string message)
    {
        try {
            string logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TheBadPlace", "debug.log");
            Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
            File.AppendAllText(logPath, $"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
        } catch { }
    }

    private void LogCrash(Exception ex)
    {
        string logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TheBadPlace", "crash_main.log");
        File.WriteAllText(logPath, ex.ToString());
    }

    private void InitializeAutocomplete()
    {
        try {
            string xmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ReflectionData.xml");
            if (File.Exists(xmlPath))
            {
                if (apiParser.LoadFromFile(xmlPath))
                {
                    cachedCompletionsJs = AutocompleteGenerator.GeneratePolytoriaCompletions(apiParser);
                    cachedHighlightConfigJson = AutocompleteGenerator.GenerateHighlightConfig(apiParser);
                }
            }
        } catch { }
    }

    private void SetupStatusTimer()
    {
        statusTimer = new DispatcherTimer();
        statusTimer.Interval = TimeSpan.FromSeconds(2); // Slightly longer interval
        statusTimer.Tick += (s, e) => {
            bool isConnected = false;
            try {
                // More robust pipe check - try to connect briefly
                using (var client = new NamedPipeClientStream(".", "TheBadPlace_Executor_Pipe", PipeDirection.Out)) {
                    try {
                        client.Connect(100); // 100ms timeout
                        isConnected = true;
                    } catch {
                        isConnected = false;
                    }
                }
            } catch { }

            if (isConnected) {
                if (StatusText.Text != "ATTACHED") LogDebug("Status: ATTACHED");
                StatusDot.Fill = Brushes.LimeGreen;
                StatusText.Text = "ATTACHED";
            } else {
                if (StatusText.Text != "NOT ATTACHED") LogDebug("Status: NOT ATTACHED");
                StatusDot.Fill = Brushes.OrangeRed;
                StatusText.Text = "NOT ATTACHED";
            }
        };
        statusTimer.Start();
        LogDebug("Status Timer Started");
    }

    private void SetupFileSystemWatcher()
    {
        try {
            FileSystemWatcher watcher = new FileSystemWatcher(EnvironmentUtils.ScriptsPath);
            watcher.Filter = "*.*";
            watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;
            watcher.EnableRaisingEvents = true;
            
            watcher.Created += (s, e) => Dispatcher.UIThread.InvokeAsync(LoadScripts);
            watcher.Deleted += (s, e) => Dispatcher.UIThread.InvokeAsync(LoadScripts);
            watcher.Renamed += (s, e) => Dispatcher.UIThread.InvokeAsync(LoadScripts);
        } catch { }
    }

    private void LoadScripts()
    {
        try {
            ScriptList.Items.Clear();
            if (Directory.Exists(EnvironmentUtils.ScriptsPath)) {
                foreach (string file in Directory.GetFiles(EnvironmentUtils.ScriptsPath)) {
                    ScriptList.Items.Add(Path.GetFileName(file));
                }
            }
        } catch { }
    }

    private void SetupEvents()
    {
        ExecuteBtn.Click += async (s, e) => {
            LogDebug("Execute Button Clicked");
            try {
                string script = await GetEditorText();
                LogDebug($"Script retrieved: {(!string.IsNullOrEmpty(script) ? script.Length + " chars" : "EMPTY")}");
                
                if (!string.IsNullOrEmpty(script)) {
                    // Remove potential JSON wrapping if necessary
                    if (script.StartsWith("\"") && script.EndsWith("\"")) {
                        LogDebug("Removing JSON quotes from script");
                        script = JsonSerializer.Deserialize<string>(script) ?? script;
                    }
                    
                    LogDebug("Sending script to IPC Bridge");
                    await PipeClient.SendScript(script);
                    LogDebug("Script sent successfully");
                } else {
                    LogDebug("Execution cancelled: Script is empty");
                }
            } catch (Exception ex) {
                LogDebug($"Execute Error: {ex.Message}");
                LogCrash(ex);
            }
        };

        ClearBtn.Click += (s, e) => {
            LogDebug("Clear Button Clicked");
            SetEditorText("");
        };

        ScriptList.SelectionChanged += async (s, e) => {
            try {
                if (ScriptList.SelectedItem != null) {
                    string fileName = ScriptList.SelectedItem.ToString() ?? "";
                    LogDebug($"Script selected: {fileName}");
                    string filePath = Path.Combine(EnvironmentUtils.ScriptsPath, fileName);
                    if (File.Exists(filePath)) {
                        string content = File.ReadAllText(filePath);
                        if (EditorTabs.SelectedItem is TabItem selectedTab) {
                            selectedTab.Header = fileName;
                            SetEditorText(content);
                            LogDebug("File content loaded into editor");
                        }
                    }
                }
            } catch (Exception ex) {
                LogDebug($"Selection Error: {ex.Message}");
            }
        };

        OptionsBtn.Click += (s, e) => {
            LogDebug("New Tab Button Clicked");
            CreateNewTab("Script " + (EditorTabs.Items.Count + 1));
        };

        OpenFileBtn.Click += async (s, e) => {
            LogDebug("Open File Clicked");
            try {
                var dialog = new OpenFileDialog();
                dialog.Directory = EnvironmentUtils.ScriptsPath;
                dialog.Filters.Add(new FileDialogFilter() { Name = "Lua Files", Extensions = { "lua", "txt" } });
                
                var result = await dialog.ShowAsync(this);
                if (result != null && result.Length > 0) {
                    LogDebug($"Opening file: {result[0]}");
                    string content = File.ReadAllText(result[0]);
                    SetEditorText(content);
                }
            } catch (Exception ex) {
                LogDebug($"Open File Error: {ex.Message}");
            }
        };

        SaveFileBtn.Click += async (s, e) => {
            LogDebug("Save File Clicked");
            try {
                var dialog = new SaveFileDialog();
                dialog.Directory = EnvironmentUtils.ScriptsPath;
                dialog.Filters.Add(new FileDialogFilter() { Name = "Lua Files", Extensions = { "lua", "txt" } });
                
                var result = await dialog.ShowAsync(this);
                if (result != null) {
                    LogDebug($"Saving to: {result}");
                    string content = await GetEditorText();
                    if (content.StartsWith("\"") && content.EndsWith("\"")) {
                        content = JsonSerializer.Deserialize<string>(content) ?? content;
                    }
                    File.WriteAllText(result, content);
                    LogDebug("File saved successfully");
                }
            } catch (Exception ex) {
                LogDebug($"Save File Error: {ex.Message}");
            }
        };
    }

    private async Task<string> GetEditorText()
    {
        try 
        {
            // Ensure we are looking at the current tab and it contains a WebView
            if (EditorTabs.SelectedItem is TabItem selectedTab && selectedTab.Content is WebView webView) 
            {
                LogDebug("Requesting text from WebView editor");
                // EvaluateScript returns a Task; execute it directly
                // Using GetText() function defined in HTML for better reliability
                string result = await webView.EvaluateScript<string>("GetText();");
                LogDebug($"WebView returned {result?.Length ?? 0} characters");
                return result ?? "";
            } else {
                LogDebug("GetEditorText failed: No active tab or WebView");
            }
        } 
        catch (Exception ex) 
        {
            LogDebug($"WebView GetText Error: {ex.Message}");
            LogCrash(ex);
        }
        return "";
    }

    private void SetEditorText(string text)
    {
        try {
            if (EditorTabs.SelectedItem is TabItem selectedTab && selectedTab.Content is WebView webView) {
                LogDebug($"Setting text in WebView ({text?.Length ?? 0} chars)");
                string escapedText = JsonSerializer.Serialize(text);
                webView.ExecuteScript($"editor.setValue({escapedText});");
                LogDebug("SetText script executed");
            } else {
                LogDebug("SetEditorText failed: No active tab or WebView");
            }
        } catch (Exception ex) {
            LogDebug($"WebView SetText Error: {ex.Message}");
        }
    }

    private void CreateNewTab(string title)
    {
        try {
            LogDebug($"Creating new tab: {title}");
            var webView = new WebView();
            var tabItem = new TabItem {
                Header = title,
                Content = webView
            };

            string htmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Editor", "SynMonaco", "EditorPolytoria.html");
            if (File.Exists(htmlPath)) {
                LogDebug($"Loading HTML: {htmlPath}");
                webView.LoadUrl("file:///" + htmlPath.Replace("\\", "/"));
            } else {
                LogDebug($"CRITICAL: HTML NOT FOUND at {htmlPath}");
            }

            webView.WebViewInitialized += () => {
                LogDebug($"WebView for {title} initialized");
                
                if (!string.IsNullOrEmpty(cachedCompletionsJs)) {
                    LogDebug("Injecting completions");
                    webView.ExecuteScript(cachedCompletionsJs);
                }
                if (!string.IsNullOrEmpty(cachedHighlightConfigJson)) {
                    LogDebug("Injecting highlight config");
                    webView.ExecuteScript($"setHighlightingConfig({cachedHighlightConfigJson});");
                }
            };

            EditorTabs.Items.Add(tabItem);
            EditorTabs.SelectedItem = tabItem;
        } catch (Exception ex) {
            LogDebug($"CreateNewTab Error: {ex.Message}");
            LogCrash(ex);
        }
    }
}
