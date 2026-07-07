using System.Text.Json;

namespace Easyman;

public partial class MainForm : Form
{
    private readonly MiddlemanProcess _middleman = new();
    private string? _eqHostPath;

    private static string SettingsPath =>
        Path.Combine(AppContext.BaseDirectory, "easyman-settings.json");

    private static string MiddlemanExePath =>
        Path.Combine(AppContext.BaseDirectory, "middleman.exe");

    public MainForm()
    {
        InitializeComponent();
        CheckForExistingProcess();
        LoadSettings();
    }

    private void CheckForExistingProcess()
    {
        if (!MiddlemanProcess.HasExistingProcess())
            return;

        var result = MessageBox.Show(
            "An existing middleman.exe process was detected.\n\n" +
            "Would you like Easyman to take over and manage it?",
            "Easyman",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (result == DialogResult.Yes)
            MiddlemanProcess.KillExisting();
    }

    private void LoadSettings()
    {
        if (!File.Exists(SettingsPath))
            return;

        try
        {
            var json = File.ReadAllText(SettingsPath);
            var settings = JsonSerializer.Deserialize<AppSettings>(json);
            if (settings?.EqHostPath is not null && File.Exists(settings.EqHostPath))
            {
                _eqHostPath = settings.EqHostPath;
                txtEqHostPath.Text = _eqHostPath;
                RefreshState();
            }
        }
        catch (JsonException)
        {
        }
    }

    private void SaveSettings()
    {
        var settings = new AppSettings { EqHostPath = _eqHostPath };
        var json = JsonSerializer.Serialize(settings);
        File.WriteAllText(SettingsPath, json);
    }

    private void RefreshState()
    {
        if (_eqHostPath is null || !File.Exists(_eqHostPath))
        {
            pnlIndicator.BackColor = Color.Gray;
            lblStatus.Text = "Not configured";
            lblStatus.ForeColor = SystemColors.GrayText;
            btnToggle.Enabled = false;
            btnToggle.Text = "Enable Middleman";
            return;
        }

        if (_middleman.IsRunning)
        {
            pnlIndicator.BackColor = Color.LimeGreen;
            lblStatus.Text = "Middleman is RUNNING";
            lblStatus.ForeColor = Color.Green;
            btnToggle.Text = "Disable Middleman";
        }
        else
        {
            pnlIndicator.BackColor = Color.Red;
            lblStatus.Text = "Middleman is OFF";
            lblStatus.ForeColor = Color.Firebrick;
            btnToggle.Text = "Enable Middleman";
        }

        btnToggle.Enabled = true;
    }

    private void BtnBrowse_Click(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Locate your eqhost.txt",
            Filter = "eqhost.txt|eqhost.txt",
            FileName = "eqhost.txt"
        };

        if (dialog.ShowDialog() != DialogResult.OK)
            return;

        _eqHostPath = dialog.FileName;
        txtEqHostPath.Text = _eqHostPath;
        SaveSettings();
        RefreshState();
    }

    private void BtnToggle_Click(object? sender, EventArgs e)
    {
        if (_eqHostPath is null)
            return;

        try
        {
            if (_middleman.IsRunning)
            {
                _middleman.Stop();
                EqHostManager.SetState(_eqHostPath, MiddlemanState.Off);
            }
            else
            {
                if (!File.Exists(MiddlemanExePath))
                {
                    MessageBox.Show(
                        "middleman.exe was not found.\n\n" +
                        "Please re-download Easyman from the releases page.\n" +
                        "middleman.exe should be included in the download.",
                        "Easyman",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                EqHostManager.SetState(_eqHostPath, MiddlemanState.On);
                _middleman.Start(MiddlemanExePath);
            }

            RefreshState();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Error: {ex.Message}",
                "Easyman",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (_middleman.IsRunning)
        {
            var result = MessageBox.Show(
                "Middleman is still running.\n\n" +
                "Click Yes to disable middleman and restore eqhost.txt.\n" +
                "Click No to close Easyman but leave middleman running.\n" +
                "Click Cancel to go back.",
                "Easyman",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (result == DialogResult.Cancel)
            {
                e.Cancel = true;
                return;
            }

            if (result == DialogResult.Yes)
            {
                _middleman.Stop();
                if (_eqHostPath is not null)
                    EqHostManager.SetState(_eqHostPath, MiddlemanState.Off);
            }
        }

        _middleman.Dispose();
        base.OnFormClosing(e);
    }
}

internal class AppSettings
{
    public string? EqHostPath { get; set; }
}
