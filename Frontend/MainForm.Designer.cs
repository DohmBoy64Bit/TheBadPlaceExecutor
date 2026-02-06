namespace Frontend;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.titleBar = new System.Windows.Forms.Panel();
        this.titleLabel = new System.Windows.Forms.Label();
        this.closeButton = new System.Windows.Forms.Button();
        this.minimizeButton = new System.Windows.Forms.Button();
        this.mainContent = new System.Windows.Forms.Panel();
        this.editorBorder = new System.Windows.Forms.Panel();
        this.editorTabs = new System.Windows.Forms.TabControl();
        this.scriptListBorder = new System.Windows.Forms.Panel();
        this.scriptList = new System.Windows.Forms.ListBox();
        this.buttonPanel = new System.Windows.Forms.FlowLayoutPanel();
        this.statusDot = new System.Windows.Forms.Label();
        this.statusText = new System.Windows.Forms.Label();
        this.executeBtn = new System.Windows.Forms.Button();
        this.clearBtn = new System.Windows.Forms.Button();
        this.openFileBtn = new System.Windows.Forms.Button();
        this.saveFileBtn = new System.Windows.Forms.Button();
        this.optionsBtn = new System.Windows.Forms.Button();
        this.scriptHubBtn = new System.Windows.Forms.Button();
        this.titleBar.SuspendLayout();
        this.mainContent.SuspendLayout();
        this.editorBorder.SuspendLayout();
        this.scriptListBorder.SuspendLayout();
        this.buttonPanel.SuspendLayout();
        this.SuspendLayout();

        // 
        // titleBar
        // 
        this.titleBar.BackColor = System.Drawing.Color.FromArgb(60, 60, 60);
        this.titleBar.Controls.Add(this.titleLabel);
        this.titleBar.Controls.Add(this.minimizeButton);
        this.titleBar.Controls.Add(this.closeButton);
        this.titleBar.Dock = System.Windows.Forms.DockStyle.Top;
        this.titleBar.Height = 30;
        this.titleBar.Name = "titleBar";

        // 
        // titleLabel
        // 
        this.titleLabel.AutoSize = true;
        this.titleLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        this.titleLabel.ForeColor = System.Drawing.Color.White;
        this.titleLabel.Location = new System.Drawing.Point(10, 5);
        this.titleLabel.Name = "titleLabel";
        this.titleLabel.Text = "THE BAD PLACE EXECUTOR";

        // 
        // closeButton
        // 
        this.closeButton.BackColor = System.Drawing.Color.FromArgb(60, 60, 60);
        this.closeButton.Dock = System.Windows.Forms.DockStyle.Right;
        this.closeButton.FlatAppearance.BorderSize = 0;
        this.closeButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.closeButton.ForeColor = System.Drawing.Color.White;
        this.closeButton.Name = "closeButton";
        this.closeButton.Size = new System.Drawing.Size(30, 30);
        this.closeButton.Text = "X";

        // 
        // minimizeButton
        // 
        this.minimizeButton.BackColor = System.Drawing.Color.FromArgb(60, 60, 60);
        this.minimizeButton.Dock = System.Windows.Forms.DockStyle.Right;
        this.minimizeButton.FlatAppearance.BorderSize = 0;
        this.minimizeButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.minimizeButton.ForeColor = System.Drawing.Color.White;
        this.minimizeButton.Name = "minimizeButton";
        this.minimizeButton.Size = new System.Drawing.Size(30, 30);
        this.minimizeButton.Text = "_";

        // 
        // mainContent
        // 
        this.mainContent.Controls.Add(this.editorBorder);
        this.mainContent.Controls.Add(this.scriptListBorder);
        this.mainContent.Dock = System.Windows.Forms.DockStyle.Fill;
        this.mainContent.Name = "mainContent";
        this.mainContent.Padding = new System.Windows.Forms.Padding(5);

        // 
        // editorBorder
        // 
        this.editorBorder.BackColor = System.Drawing.Color.FromArgb(35, 35, 35);
        this.editorBorder.Controls.Add(this.editorTabs);
        this.editorBorder.Dock = System.Windows.Forms.DockStyle.Fill;
        this.editorBorder.Name = "editorBorder";
        this.editorBorder.Padding = new System.Windows.Forms.Padding(1);

        // 
        // editorTabs
        // 
        this.editorTabs.Dock = System.Windows.Forms.DockStyle.Fill;
        this.editorTabs.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
        this.editorTabs.Name = "editorTabs";
        this.editorTabs.Padding = new System.Drawing.Point(12, 3);

        // 
        // scriptListBorder
        // 
        this.scriptListBorder.BackColor = System.Drawing.Color.FromArgb(35, 35, 35);
        this.scriptListBorder.Controls.Add(this.scriptList);
        this.scriptListBorder.Dock = System.Windows.Forms.DockStyle.Right;
        this.scriptListBorder.Name = "scriptListBorder";
        this.scriptListBorder.Padding = new System.Windows.Forms.Padding(1);
        this.scriptListBorder.Width = 152;

        // 
        // scriptList
        // 
        this.scriptList.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
        this.scriptList.BorderStyle = System.Windows.Forms.BorderStyle.None;
        this.scriptList.Dock = System.Windows.Forms.DockStyle.Fill;
        this.scriptList.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.scriptList.ForeColor = System.Drawing.Color.White;
        this.scriptList.Name = "scriptList";

        // 
        // buttonPanel
        // 
        this.buttonPanel.BackColor = System.Drawing.Color.FromArgb(45, 45, 45);
        this.buttonPanel.Controls.Add(this.statusDot);
        this.buttonPanel.Controls.Add(this.statusText);
        this.buttonPanel.Controls.Add(this.executeBtn);
        this.buttonPanel.Controls.Add(this.clearBtn);
        this.buttonPanel.Controls.Add(this.openFileBtn);
        this.buttonPanel.Controls.Add(this.saveFileBtn);
        this.buttonPanel.Controls.Add(this.optionsBtn);
        this.buttonPanel.Controls.Add(this.scriptHubBtn);
        this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.buttonPanel.Height = 35;
        this.buttonPanel.Name = "buttonPanel";
        this.buttonPanel.Padding = new System.Windows.Forms.Padding(2);
        this.buttonPanel.WrapContents = false;

        // 
        // statusDot
        // 
        this.statusDot.AutoSize = true;
        this.statusDot.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
        this.statusDot.ForeColor = System.Drawing.Color.Red;
        this.statusDot.Margin = new System.Windows.Forms.Padding(0, 5, 0, 0);
        this.statusDot.Name = "statusDot";
        this.statusDot.Text = "●";

        // 
        // statusText
        // 
        this.statusText.AutoSize = true;
        this.statusText.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
        this.statusText.ForeColor = System.Drawing.Color.White;
        this.statusText.Margin = new System.Windows.Forms.Padding(0, 8, 10, 0);
        this.statusText.Name = "statusText";
        this.statusText.Text = "NOT ATTACHED";

        // 
        // executeBtn
        // 
        this.executeBtn.BackColor = System.Drawing.Color.FromArgb(60, 60, 60);
        this.executeBtn.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(80, 80, 80);
        this.executeBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.executeBtn.Font = new System.Drawing.Font("Segoe UI", 8F);
        this.executeBtn.ForeColor = System.Drawing.Color.White;
        this.executeBtn.Name = "executeBtn";
        this.executeBtn.Size = new System.Drawing.Size(90, 28);
        this.executeBtn.Text = "Execute";

        // 
        // clearBtn
        // 
        this.clearBtn.BackColor = System.Drawing.Color.FromArgb(60, 60, 60);
        this.clearBtn.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(80, 80, 80);
        this.clearBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.clearBtn.Font = new System.Drawing.Font("Segoe UI", 8F);
        this.clearBtn.ForeColor = System.Drawing.Color.White;
        this.clearBtn.Name = "clearBtn";
        this.clearBtn.Size = new System.Drawing.Size(90, 28);
        this.clearBtn.Text = "Clear";

        // 
        // openFileBtn
        // 
        this.openFileBtn.BackColor = System.Drawing.Color.FromArgb(60, 60, 60);
        this.openFileBtn.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(80, 80, 80);
        this.openFileBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.openFileBtn.Font = new System.Drawing.Font("Segoe UI", 8F);
        this.openFileBtn.ForeColor = System.Drawing.Color.White;
        this.openFileBtn.Name = "openFileBtn";
        this.openFileBtn.Size = new System.Drawing.Size(90, 28);
        this.openFileBtn.Text = "Open File";

        // 
        // saveFileBtn
        // 
        this.saveFileBtn.BackColor = System.Drawing.Color.FromArgb(60, 60, 60);
        this.saveFileBtn.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(80, 80, 80);
        this.saveFileBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.saveFileBtn.Font = new System.Drawing.Font("Segoe UI", 8F);
        this.saveFileBtn.ForeColor = System.Drawing.Color.White;
        this.saveFileBtn.Name = "saveFileBtn";
        this.saveFileBtn.Size = new System.Drawing.Size(90, 28);
        this.saveFileBtn.Text = "Save File";

        // 
        // optionsBtn
        // 
        this.optionsBtn.BackColor = System.Drawing.Color.FromArgb(60, 60, 60);
        this.optionsBtn.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(80, 80, 80);
        this.optionsBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.optionsBtn.Font = new System.Drawing.Font("Segoe UI", 8F);
        this.optionsBtn.ForeColor = System.Drawing.Color.White;
        this.optionsBtn.Name = "optionsBtn";
        this.optionsBtn.Size = new System.Drawing.Size(90, 28);
        this.optionsBtn.Text = "Options";

        // 
        // scriptHubBtn
        // 
        this.scriptHubBtn.BackColor = System.Drawing.Color.FromArgb(60, 60, 60);
        this.scriptHubBtn.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(80, 80, 80);
        this.scriptHubBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.scriptHubBtn.Font = new System.Drawing.Font("Segoe UI", 8F);
        this.scriptHubBtn.ForeColor = System.Drawing.Color.White;
        this.scriptHubBtn.Name = "scriptHubBtn";
        this.scriptHubBtn.Size = new System.Drawing.Size(90, 28);
        this.scriptHubBtn.Text = "Script Hub";

        // 
        // MainForm
        // 
        this.BackColor = System.Drawing.Color.FromArgb(45, 45, 45);
        this.ClientSize = new System.Drawing.Size(800, 450);
        this.Controls.Add(this.mainContent);
        this.Controls.Add(this.buttonPanel);
        this.Controls.Add(this.titleBar);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
        this.Name = "MainForm";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "The Bad Place Executor";
        this.titleBar.ResumeLayout(false);
        this.titleBar.PerformLayout();
        this.mainContent.ResumeLayout(false);
        this.editorBorder.ResumeLayout(false);
        this.scriptListBorder.ResumeLayout(false);
        this.buttonPanel.ResumeLayout(false);
        this.buttonPanel.PerformLayout();
        this.ResumeLayout(false);
    }

    private System.Windows.Forms.Panel titleBar;
    private System.Windows.Forms.Label titleLabel;
    private System.Windows.Forms.Button closeButton;
    private System.Windows.Forms.Button minimizeButton;
    private System.Windows.Forms.Panel mainContent;
    private System.Windows.Forms.Panel editorBorder;
    private System.Windows.Forms.TabControl editorTabs;
    private System.Windows.Forms.Panel scriptListBorder;
    private System.Windows.Forms.ListBox scriptList;
    private System.Windows.Forms.FlowLayoutPanel buttonPanel;
    private System.Windows.Forms.Label statusDot;
    private System.Windows.Forms.Label statusText;
    private System.Windows.Forms.Button executeBtn;
    private System.Windows.Forms.Button clearBtn;
    private System.Windows.Forms.Button openFileBtn;
    private System.Windows.Forms.Button saveFileBtn;
    private System.Windows.Forms.Button optionsBtn;
    private System.Windows.Forms.Button scriptHubBtn;
}
