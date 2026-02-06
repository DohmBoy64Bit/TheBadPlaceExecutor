using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace Frontend;

public partial class MainForm : Form
{
    private Panel titleBar;
    private Label titleLabel;
    private Button closeButton;
    private Button minimizeButton;
    private WebView2 editorView;
    private ListBox scriptList;
    private FlowLayoutPanel buttonPanel;
    
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
        InitializeComponent();
        SetupStyles();
        InitializeEditor();
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

        editorView = new WebView2 {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(30, 30, 30)
        };

        scriptList = new ListBox {
            Dock = DockStyle.Right,
            Width = 150,
            BackColor = Color.FromArgb(30, 30, 30),
            ForeColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            Font = new Font("Segoe UI", 9)
        };

        mainContent.Controls.Add(editorView);
        mainContent.Controls.Add(scriptList);

        // Button Panel
        buttonPanel = new FlowLayoutPanel {
            Dock = DockStyle.Bottom,
            Height = 40,
            Padding = new Padding(5),
            BackColor = Color.FromArgb(45, 45, 45)
        };

        executeBtn = CreateStyledButton("Execute");
        clearBtn = CreateStyledButton("Clear");
        openFileBtn = CreateStyledButton("Open File");
        saveFileBtn = CreateStyledButton("Save File");
        optionsBtn = CreateStyledButton("Options");
        attachBtn = CreateStyledButton("Attach");
        scriptHubBtn = CreateStyledButton("Script Hub");

        buttonPanel.Controls.AddRange(new Control[] { 
            executeBtn, clearBtn, openFileBtn, saveFileBtn, optionsBtn, attachBtn, scriptHubBtn 
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

    private async void InitializeEditor()
    {
        try {
            await editorView.EnsureCoreWebView2Async(null);
            string editorPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Editor", "SynMonaco", "EditorPolytoria.html");
            if (File.Exists(editorPath)) {
                editorView.Source = new Uri(editorPath);
            } else {
                // Fallback for development if not in output dir yet
                string devPath = Path.Combine(Directory.GetCurrentDirectory(), "Editor", "SynMonaco", "EditorPolytoria.html");
                if (File.Exists(devPath)) {
                    editorView.Source = new Uri(devPath);
                }
            }
        } catch (Exception ex) {
            MessageBox.Show("Failed to initialize editor: " + ex.Message);
        }
    }

    // Helper to get text from editor (async)
    public async Task<string> GetEditorText()
    {
        return await editorView.ExecuteScriptAsync("GetText()");
    }

    public async void SetEditorText(string text)
    {
        string encoded = JsonSerializer.Serialize(text);
        await editorView.ExecuteScriptAsync($"SetText({encoded})");
    }
}
