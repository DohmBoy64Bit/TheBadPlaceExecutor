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
using Frontend.Editors;

namespace Frontend;

public partial class MainWindow : Window
{
    private ApiParser apiParser = new ApiParser();
    private string? cachedCompletionsJs;
    private string? cachedHighlightConfigJson;
    private DispatcherTimer? statusTimer;
    private ScriptEditorManager? editorManager;

    public MainWindow()
    {
        InitializeComponent();
        
        try {
            EnvironmentUtils.InitializeFolders();
            InitializeAutocomplete();
            
            if (EditorTabs == null)
            {
                File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TheBadPlace", "crash_main.log"), "EditorTabs is null!");
                return;
            }
            
            editorManager = new ScriptEditorManager(EditorTabs);
            editorManager.LoadCompletions(apiParser);
            
            SetupEvents();
            LoadScripts();
            SetupFileSystemWatcher();
            SetupStatusTimer();
            
            // Initial Tab
            editorManager.CreateNewTab("Script 1");
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
        statusTimer.Interval = TimeSpan.FromSeconds(1);
        statusTimer.Tick += (s, e) => {
            bool isConnected = false;
            try {
                // More robust pipe check - try to connect briefly
                using (var client = new NamedPipeClientStream(".", "TheBadPlace_Executor_Pipe", PipeDirection.Out)) {
                    try {
                        client.Connect(10); // Very short timeout
                        isConnected = true;
                    } catch {
                        isConnected = false;
                    }
                }
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
            try {
                string script = editorManager?.GetEditorText() ?? "";
                if (!string.IsNullOrEmpty(script)) {
                    await PipeClient.SendScript(script);
                }
            } catch { }
        };

        ClearBtn.Click += (s, e) => editorManager?.SetEditorText("");

        ScriptList.SelectionChanged += (s, e) => {
            try {
                if (ScriptList.SelectedItem != null) {
                    string fileName = ScriptList.SelectedItem.ToString() ?? "";
                    string filePath = Path.Combine(EnvironmentUtils.ScriptsPath, fileName);
                    if (File.Exists(filePath)) {
                        string content = File.ReadAllText(filePath);
                        if (EditorTabs.SelectedItem is TabItem selectedTab) {
                            selectedTab.Header = fileName;
                            editorManager?.SetEditorText(content);
                        }
                    }
                }
            } catch { }
        };

        OptionsBtn.Click += (s, e) => editorManager?.CreateNewTab("Script " + (EditorTabs.Items.Count + 1));

        OpenFileBtn.Click += async (s, e) => {
            try {
                var dialog = new OpenFileDialog();
                dialog.Directory = EnvironmentUtils.ScriptsPath;
                dialog.Filters.Add(new FileDialogFilter() { Name = "Lua Files", Extensions = { "lua", "txt" } });
                
                var result = await dialog.ShowAsync(this);
                if (result != null && result.Length > 0) {
                    string content = File.ReadAllText(result[0]);
                    editorManager?.SetEditorText(content);
                }
            } catch { }
        };

        SaveFileBtn.Click += async (s, e) => {
            try {
                var dialog = new SaveFileDialog();
                dialog.Directory = EnvironmentUtils.ScriptsPath;
                dialog.Filters.Add(new FileDialogFilter() { Name = "Lua Files", Extensions = { "lua", "txt" } });
                
                var result = await dialog.ShowAsync(this);
                if (result != null) {
                    string content = editorManager?.GetEditorText() ?? "";
                    File.WriteAllText(result, content);
                }
            } catch { }
        };
    }

}
