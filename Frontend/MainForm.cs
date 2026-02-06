using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System.Runtime.InteropServices;
using System.Text.Json;
using Frontend.IPC;
using Frontend.Utils;
using Frontend.Scripting;

namespace Frontend;

public partial class MainForm : Form
{
    // Theme Colors
    private readonly Color BorderColor = Color.FromArgb(35, 35, 35);
    private readonly Color ActiveTabColor = Color.FromArgb(45, 45, 45);
    private readonly Color InactiveTabColor = Color.FromArgb(30, 30, 30);
    private readonly Color TextColor = Color.White;
    
    // Autocomplete support
    private ApiParser apiParser = new ApiParser();
    private string? cachedCompletionsJs;
    private string? cachedHighlightConfigJson;
    
    private System.Windows.Forms.Timer statusTimer;

    // Draggable window support
    [DllImport("user32.dll")]
    public static extern bool ReleaseCapture();
    [DllImport("user32.dll")]
    public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
    private const int WM_NCLBUTTONDOWN = 0xA1;
    private const int HT_CAPTION = 0x2;

    public MainForm()
    {
        EnvironmentUtils.InitializeFolders();
        InitializeAutocomplete();
        InitializeComponent();
        SetupCustomUI();
        SetupEvents();
        LoadScripts();
        SetupFileSystemWatcher();
        SetupStatusTimer();
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
        statusTimer = new System.Windows.Forms.Timer();
        statusTimer.Interval = 1000; // Check every second
        statusTimer.Tick += (s, e) => {
            bool isConnected = false;
            try {
                isConnected = System.IO.Directory.GetFiles(@"\\.\pipe\").Contains(@"\\.\pipe\TheBadPlace_Executor_Pipe");
            } catch { }

            if (isConnected) {
                statusDot.ForeColor = Color.LimeGreen;
                statusText.Text = "ATTACHED";
            } else {
                statusDot.ForeColor = Color.OrangeRed;
                statusText.Text = "NOT ATTACHED";
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
        
        watcher.Created += (s, e) => this.Invoke(new Action(LoadScripts));
        watcher.Deleted += (s, e) => this.Invoke(new Action(LoadScripts));
        watcher.Renamed += (s, e) => this.Invoke(new Action(LoadScripts));
    }

    private void LoadScripts()
    {
        scriptList.Items.Clear();
        if (Directory.Exists(EnvironmentUtils.ScriptsPath)) {
            foreach (string file in Directory.GetFiles(EnvironmentUtils.ScriptsPath)) {
                scriptList.Items.Add(Path.GetFileName(file));
            }
        }
    }

    private void SetupCustomUI()
    {
        // Custom Drag Support for Title Bar
        titleBar.MouseDown += (s, e) => {
            if (e.Button == MouseButtons.Left) {
                ReleaseCapture();
                SendMessage(this.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        };

        // Custom Tab Rendering
        editorTabs.Paint += (s, e) => {
            e.Graphics.Clear(InactiveTabColor);
            using (var pen = new Pen(BorderColor, 1)) {
                e.Graphics.DrawRectangle(pen, 0, 0, editorTabs.Width - 1, editorTabs.Height - 1);
            }
        };

        editorTabs.DrawItem += (s, e) => {
            var tabRect = editorTabs.GetTabRect(e.Index);
            bool isSelected = editorTabs.SelectedIndex == e.Index;
            
            using (var brush = new SolidBrush(isSelected ? ActiveTabColor : InactiveTabColor)) {
                e.Graphics.FillRectangle(brush, tabRect);
            }

            using (var pen = new Pen(BorderColor)) {
                e.Graphics.DrawRectangle(pen, tabRect);
            }

            TextRenderer.DrawText(e.Graphics, editorTabs.TabPages[e.Index].Text, editorTabs.Font, 
                new Point(tabRect.X + 8, tabRect.Y + 6), TextColor);

            if (editorTabs.TabPages.Count > 1) {
                TextRenderer.DrawText(e.Graphics, "x", editorTabs.Font, 
                    new Point(tabRect.Right - 15, tabRect.Y + 5), Color.Gray);
            }
        };

        editorTabs.MouseDown += (s, e) => {
            for (int i = 0; i < editorTabs.TabPages.Count; i++) {
                var tabRect = editorTabs.GetTabRect(i);
                var closeRect = new Rectangle(tabRect.Right - 20, tabRect.Y, 20, tabRect.Height);
                if (closeRect.Contains(e.Location)) {
                    if (editorTabs.TabPages.Count > 1) {
                        editorTabs.TabPages.RemoveAt(i);
                    }
                    break;
                }
            }
        };

        // Initial Tab
        CreateNewTab("Script 1");
    }

    private void SetupEvents()
    {
        closeButton.Click += (s, e) => Application.Exit();
        minimizeButton.Click += (s, e) => this.WindowState = FormWindowState.Minimized;

        executeBtn.Click += async (s, e) => {
            string script = await GetEditorText();
            if (!string.IsNullOrEmpty(script)) {
                if (script.StartsWith("\"") && script.EndsWith("\"")) {
                    script = JsonSerializer.Deserialize<string>(script) ?? script;
                }
                await PipeClient.SendScript(script);
            }
        };

        clearBtn.Click += (s, e) => SetEditorText("");

        scriptList.SelectedIndexChanged += async (s, e) => {
            if (scriptList.SelectedItem != null) {
                string fileName = scriptList.SelectedItem.ToString();
                string filePath = Path.Combine(EnvironmentUtils.ScriptsPath, fileName);
                if (File.Exists(filePath)) {
                    string content = File.ReadAllText(filePath);
                    if (editorTabs.SelectedTab != null) {
                        editorTabs.SelectedTab.Text = fileName;
                        SetEditorText(content);
                    }
                }
            }
        };

        optionsBtn.Click += (s, e) => CreateNewTab("Script " + (editorTabs.TabPages.Count + 1));

        openFileBtn.Click += (s, e) => {
            using (OpenFileDialog ofd = new OpenFileDialog()) {
                ofd.InitialDirectory = EnvironmentUtils.ScriptsPath;
                ofd.Filter = "Lua files (*.lua)|*.lua|Text files (*.txt)|*.txt|All files (*.*)|*.*";
                if (ofd.ShowDialog() == DialogResult.OK) {
                    string content = File.ReadAllText(ofd.FileName);
                    SetEditorText(content);
                }
            }
        };

        saveFileBtn.Click += async (s, e) => {
            using (SaveFileDialog sfd = new SaveFileDialog()) {
                sfd.InitialDirectory = EnvironmentUtils.ScriptsPath;
                sfd.Filter = "Lua files (*.lua)|*.lua|Text files (*.txt)|*.txt|All files (*.*)|*.*";
                if (sfd.ShowDialog() == DialogResult.OK) {
                    string content = await GetEditorText();
                    if (content.StartsWith("\"") && content.EndsWith("\"")) {
                        content = JsonSerializer.Deserialize<string>(content) ?? content;
                    }
                    File.WriteAllText(sfd.FileName, content);
                }
            }
        };
    }

    private async Task<string> GetEditorText()
    {
        if (editorTabs.SelectedTab != null && editorTabs.SelectedTab.Controls.Count > 0) {
            if (editorTabs.SelectedTab.Controls[0] is WebView2 webView) {
                try {
                    return await webView.ExecuteScriptAsync("editor.getValue();");
                } catch { }
            }
        }
        return "";
    }

    private async void SetEditorText(string text)
    {
        if (editorTabs.SelectedTab != null && editorTabs.SelectedTab.Controls.Count > 0) {
            if (editorTabs.SelectedTab.Controls[0] is WebView2 webView) {
                try {
                    string escapedText = JsonSerializer.Serialize(text);
                    await webView.ExecuteScriptAsync($"editor.setValue({escapedText});");
                } catch { }
            }
        }
    }

    private WebView2 CreateNewTab(string title)
    {
        TabPage page = new TabPage(title);
        page.BackColor = InactiveTabColor;
        WebView2 webView = new WebView2 {
            Dock = DockStyle.Fill,
            BackColor = InactiveTabColor
        };
        
        webView.CoreWebView2InitializationCompleted += async (s, e) => {
            if (e.IsSuccess) {
                string htmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Editor", "SynMonaco", "EditorPolytoria.html");
                if (File.Exists(htmlPath)) {
                    webView.CoreWebView2.Navigate("file:///" + htmlPath.Replace("\\", "/"));
                    
                    // Inject completions and highlighting after navigation
                    webView.CoreWebView2.NavigationCompleted += async (sender, args) => {
                        if (args.IsSuccess) {
                            if (!string.IsNullOrEmpty(cachedCompletionsJs)) {
                                await webView.CoreWebView2.ExecuteScriptAsync(cachedCompletionsJs);
                            }
                            if (!string.IsNullOrEmpty(cachedHighlightConfigJson)) {
                                await webView.CoreWebView2.ExecuteScriptAsync($"setHighlightingConfig({cachedHighlightConfigJson});");
                            }
                        }
                    };
                }
            }
        };

        webView.EnsureCoreWebView2Async();
        page.Controls.Add(webView);
        editorTabs.TabPages.Add(page);
        editorTabs.SelectedTab = page;
        return webView;
    }
}
