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
    private Panel titleBar;
    private Label titleLabel;
    private Button closeButton;
    private Button minimizeButton;
    private ListBox scriptList;
    private FlowLayoutPanel buttonPanel;
    private TabControl editorTabs;
    private Label statusDot;
    private Label statusText;
    private System.Windows.Forms.Timer statusTimer;
    
    // Autocomplete support
    private ApiParser apiParser = new ApiParser();
    private string? cachedCompletionsJs;
    private string? cachedHighlightConfigJson;
    
    // Synapse X style buttons
    private Button executeBtn;
    private Button clearBtn;
    private Button openFileBtn;
    private Button saveFileBtn;
    private Button optionsBtn;
    private Button attachBtn;
    private Button scriptHubBtn;

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
        SetupStyles();
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
        statusTimer.Interval = 2000; // Check every 2 seconds
        statusTimer.Tick += (s, e) => {
            bool isConnected = false;
            try {
                // Check if pipe exists without full connect
                string pipePath = @"\\.\pipe\TheBadPlace_Executor_Pipe";
                if (System.IO.File.Exists(pipePath)) {
                    isConnected = true;
                }
            } catch { }

            if (isConnected) {
                statusDot.ForeColor = Color.Green;
                statusText.Text = "ATTACHED";
            } else {
                statusDot.ForeColor = Color.Red;
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

    private void SetupEvents()
    {
        executeBtn.Click += async (s, e) => {
            string script = await GetEditorText();
            if (!string.IsNullOrEmpty(script)) {
                // Remove quotes from ExecuteScriptAsync result if present
                if (script.StartsWith("\"") && script.EndsWith("\"")) {
                    script = JsonSerializer.Deserialize<string>(script) ?? script;
                }
                await PipeClient.SendScript(script);
            }
        };

        clearBtn.Click += (s, e) => {
            SetEditorText("");
        };

        attachBtn.Click += (s, e) => {
            MessageBox.Show("Attachment is handled automatically by MelonLoader in this prototype.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        };

        scriptList.SelectedIndexChanged += async (s, e) => {
            if (scriptList.SelectedItem != null) {
                string fileName = scriptList.SelectedItem.ToString();
                string filePath = Path.Combine(EnvironmentUtils.ScriptsPath, fileName);
                if (File.Exists(filePath)) {
                    string content = File.ReadAllText(filePath);
                    
                    // If current tab is not empty and not matching this file, maybe open new tab?
                    // For now, just load into current tab and rename tab
                    if (editorTabs.SelectedTab != null) {
                        editorTabs.SelectedTab.Text = fileName;
                        SetEditorText(content);
                    }
                }
            }
        };

        optionsBtn.Text = "+";
        optionsBtn.Size = new Size(30, 30);
        optionsBtn.Click += (s, e) => {
            CreateNewTab("Script " + (editorTabs.TabPages.Count + 1));
        };

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
                    // Clean up potential JSON quotes from Monaco return
                    if (content.StartsWith("\"") && content.EndsWith("\"")) {
                        content = JsonSerializer.Deserialize<string>(content) ?? content;
                    }
                    File.WriteAllText(sfd.FileName, content);
                }
            }
        };
    }

    private void InitializeComponent()
    {
        this.SuspendLayout();
        
        // Form properties
        this.Size = new Size(800, 450);
        this.FormBorderStyle = FormBorderStyle.None;
        this.BackColor = Color.FromArgb(45, 45, 45);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.Text = "The Bad Place Executor";

        // Title Bar
        titleBar = new Panel {
            Dock = DockStyle.Top,
            Height = 30,
            BackColor = Color.FromArgb(60, 60, 60)
        };
        titleBar.MouseDown += (s, e) => {
            if (e.Button == MouseButtons.Left) {
                ReleaseCapture();
                SendMessage(this.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        };

        titleLabel = new Label {
            Text = "THE BAD PLACE EXECUTOR",
            ForeColor = Color.White,
            Location = new Point(10, 5),
            AutoSize = true,
            Font = new Font("Segoe UI", 9, FontStyle.Bold)
        };

        closeButton = new Button {
            Text = "X",
            Size = new Size(30, 30),
            Dock = DockStyle.Right,
            FlatStyle = FlatStyle.Flat,
            ForeColor = Color.White,
            BackColor = Color.FromArgb(60, 60, 60)
        };
        closeButton.FlatAppearance.BorderSize = 0;
        closeButton.Click += (s, e) => Application.Exit();

        minimizeButton = new Button {
            Text = "_",
            Size = new Size(30, 30),
            Dock = DockStyle.Right,
            FlatStyle = FlatStyle.Flat,
            ForeColor = Color.White,
            BackColor = Color.FromArgb(60, 60, 60)
        };
        minimizeButton.FlatAppearance.BorderSize = 0;
        minimizeButton.Click += (s, e) => this.WindowState = FormWindowState.Minimized;

        titleBar.Controls.Add(titleLabel);
        titleBar.Controls.Add(minimizeButton);
        titleBar.Controls.Add(closeButton);

        // Editor and ListBox Container
        Panel mainContent = new Panel {
            Dock = DockStyle.Fill,
            Padding = new Padding(5)
        };

        editorTabs = new TabControl {
            Dock = DockStyle.Fill,
            Appearance = TabAppearance.Normal,
            Padding = new Point(10, 3)
        };
        // Add initial tab
        CreateNewTab("Script 1");

        scriptList = new ListBox {
            Dock = DockStyle.Right,
            Width = 150,
            BackColor = Color.FromArgb(30, 30, 30),
            ForeColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            Font = new Font("Segoe UI", 9)
        };

        mainContent.Controls.Add(editorTabs);
        mainContent.Controls.Add(scriptList);

        // Button Panel
        buttonPanel = new FlowLayoutPanel {
            Dock = DockStyle.Bottom,
            Height = 40,
            Padding = new Padding(5),
            BackColor = Color.FromArgb(45, 45, 45)
        };

        statusDot = new Label {
            Text = "●",
            ForeColor = Color.Red,
            AutoSize = true,
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            Margin = new Padding(0, 5, 0, 0)
        };

        statusText = new Label {
            Text = "NOT ATTACHED",
            ForeColor = Color.White,
            AutoSize = true,
            Font = new Font("Segoe UI", 8, FontStyle.Bold),
            Margin = new Padding(0, 8, 10, 0)
        };

        executeBtn = CreateStyledButton("Execute");
        clearBtn = CreateStyledButton("Clear");
        openFileBtn = CreateStyledButton("Open File");
        saveFileBtn = CreateStyledButton("Save File");
        optionsBtn = CreateStyledButton("Options");
        attachBtn = CreateStyledButton("Attach");
        scriptHubBtn = CreateStyledButton("Script Hub");

        buttonPanel.Controls.AddRange(new Control[] { 
            statusDot, statusText, executeBtn, clearBtn, openFileBtn, saveFileBtn, optionsBtn, attachBtn, scriptHubBtn 
        });

        this.Controls.Add(mainContent);
        this.Controls.Add(buttonPanel);
        this.Controls.Add(titleBar);

        this.ResumeLayout(false);
    }

    private Button CreateStyledButton(string text)
    {
        var btn = new Button {
            Text = text,
            Size = new Size(100, 30),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(60, 60, 60),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 8)
        };
        btn.FlatAppearance.BorderSize = 0;
        return btn;
    }

    private void SetupStyles()
    {
        // Add rounded corners or other styles if needed
    }

    private WebView2 CreateNewTab(string title)
    {
        TabPage page = new TabPage(title);
        WebView2 webView = new WebView2 {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(30, 30, 30)
        };
        page.Controls.Add(webView);
        editorTabs.TabPages.Add(page);
        editorTabs.SelectedTab = page;
        
        InitializeEditorForView(webView);
        return webView;
    }

    private WebView2? GetCurrentEditor()
    {
        if (editorTabs.SelectedTab != null && editorTabs.SelectedTab.Controls.Count > 0) {
            return editorTabs.SelectedTab.Controls[0] as WebView2;
        }
        return null;
    }

    private async void InitializeEditorForView(WebView2 view)
    {
        try {
            await view.EnsureCoreWebView2Async(null);

            view.NavigationCompleted += async (s, e) => {
                if (e.IsSuccess) {
                    if (!string.IsNullOrEmpty(cachedHighlightConfigJson)) {
                        string encoded = JsonSerializer.Serialize(cachedHighlightConfigJson);
                        await view.ExecuteScriptAsync($"LoadHighlighting({encoded})");
                    }
                    if (!string.IsNullOrEmpty(cachedCompletionsJs)) {
                        await view.ExecuteScriptAsync(cachedCompletionsJs);
                    }
                }
            };

            string editorPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Editor", "SynMonaco", "EditorPolytoria.html");
            if (File.Exists(editorPath)) {
                view.Source = new Uri(editorPath);
            } else {
                string devPath = Path.Combine(Directory.GetCurrentDirectory(), "Editor", "SynMonaco", "EditorPolytoria.html");
                if (File.Exists(devPath)) {
                    view.Source = new Uri(devPath);
                }
            }
        } catch (Exception ex) {
            MessageBox.Show("Failed to initialize editor tab: " + ex.Message);
        }
    }

    // Helper to get text from current editor (async)
    public async Task<string> GetEditorText()
    {
        var view = GetCurrentEditor();
        if (view != null && view.CoreWebView2 != null) {
            return await view.ExecuteScriptAsync("GetText()");
        }
        return "";
    }

    public async void SetEditorText(string text)
    {
        var view = GetCurrentEditor();
        if (view != null && view.CoreWebView2 != null) {
            string encoded = JsonSerializer.Serialize(text);
            await view.ExecuteScriptAsync($"SetText({encoded})");
        }
    }
}
