namespace Easyman;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    private Label lblTitle;
    private Label lblPath;
    private TextBox txtEqHostPath;
    private Button btnBrowse;
    private Panel pnlIndicator;
    private Label lblStatus;
    private Button btnToggle;
    private Button btnRevert;
    private Button btnSettings;
    private LinkLabel lnkCredit;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        lblTitle = new Label();
        lblPath = new Label();
        txtEqHostPath = new TextBox();
        btnBrowse = new Button();
        pnlIndicator = new Panel();
        lblStatus = new Label();
        btnToggle = new Button();
        btnRevert = new Button();
        btnSettings = new Button();
        lnkCredit = new LinkLabel();
        SuspendLayout();

        // lblTitle
        lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
        lblTitle.Location = new Point(12, 9);
        lblTitle.Size = new Size(360, 30);
        lblTitle.Text = "Easyman";
        lblTitle.TextAlign = ContentAlignment.MiddleCenter;

        // lblPath
        lblPath.Location = new Point(12, 50);
        lblPath.Size = new Size(100, 23);
        lblPath.Text = "eqhost.txt path:";
        lblPath.TextAlign = ContentAlignment.MiddleLeft;

        // txtEqHostPath
        txtEqHostPath.Location = new Point(12, 73);
        txtEqHostPath.Size = new Size(280, 23);
        txtEqHostPath.ReadOnly = true;
        txtEqHostPath.BackColor = SystemColors.Window;

        // btnBrowse
        btnBrowse.Location = new Point(298, 72);
        btnBrowse.Size = new Size(75, 25);
        btnBrowse.Text = "Browse...";
        btnBrowse.Click += BtnBrowse_Click;

        // pnlIndicator
        pnlIndicator.Location = new Point(120, 118);
        pnlIndicator.Size = new Size(16, 16);
        pnlIndicator.BackColor = Color.Gray;

        // lblStatus
        lblStatus.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblStatus.Location = new Point(140, 115);
        lblStatus.Size = new Size(230, 22);
        lblStatus.Text = "Not configured";
        lblStatus.TextAlign = ContentAlignment.MiddleLeft;

        // btnToggle
        btnToggle.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        btnToggle.Location = new Point(80, 150);
        btnToggle.Size = new Size(220, 45);
        btnToggle.Text = "Enable Middleman";
        btnToggle.Enabled = false;
        btnToggle.Click += BtnToggle_Click;

        // btnRevert
        btnRevert.Font = new Font("Segoe UI", 8F);
        btnRevert.Location = new Point(115, 200);
        btnRevert.Size = new Size(150, 25);
        btnRevert.Text = "Revert eqhost.txt";
        btnRevert.Enabled = false;
        btnRevert.Click += BtnRevert_Click;

        // btnSettings
        btnSettings.Font = new Font("Segoe UI", 8F);
        btnSettings.Location = new Point(305, 240);
        btnSettings.Size = new Size(68, 22);
        btnSettings.Text = "Settings";
        btnSettings.Click += BtnSettings_Click;

        // lnkCredit
        lnkCredit.Font = new Font("Segoe UI", 8F);
        lnkCredit.Location = new Point(12, 240);
        lnkCredit.Size = new Size(290, 20);
        lnkCredit.Text = "Built by Repo/Gretchen <Europa>";
        lnkCredit.LinkArea = new LinkArea(0, 5);
        lnkCredit.TextAlign = ContentAlignment.MiddleLeft;
        lnkCredit.LinkClicked += LnkCredit_Click;

        // MainForm
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(384, 270);
        Controls.Add(lblTitle);
        Controls.Add(lblPath);
        Controls.Add(txtEqHostPath);
        Controls.Add(btnBrowse);
        Controls.Add(pnlIndicator);
        Controls.Add(lblStatus);
        Controls.Add(btnToggle);
        Controls.Add(btnRevert);
        Controls.Add(btnSettings);
        Controls.Add(lnkCredit);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Easyman - Middleman Helper for P99";
        ResumeLayout(false);
        PerformLayout();
    }
}
