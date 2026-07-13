using System.Text.Json;
using System.Text.Json.Serialization;

namespace Easyman;

public partial class MainForm : Form
{
    private readonly MiddlemanProcess _middleman = new();
    private AppSettings _settings = new();
    private string? _eqHostPath;

    private static string SettingsPath =>
        Path.Combine(AppContext.BaseDirectory, "easyman-settings.json");

    private static string MiddlemanExePath =>
        Path.Combine(AppContext.BaseDirectory, "middleman.exe");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public MainForm()
    {
        InitializeComponent();
        CheckForExistingProcess();
        LoadSettings();
        ApplyTheme();
    }

    private void ApplyTheme()
    {
        bool dark = _settings.UseDarkTheme ?? ThemeHelper.DetectSystemDarkMode();
        ThemeHelper.Apply(this, dark);
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
            var loaded = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);
            if (loaded is not null)
            {
                _settings = loaded;
                if (_settings.EqHostPath is not null && File.Exists(_settings.EqHostPath))
                {
                    _eqHostPath = _settings.EqHostPath;
                    txtEqHostPath.Text = _eqHostPath;
                    RefreshState();
                }
            }
        }
        catch (JsonException)
        {
        }
    }

    private void SaveSettings()
    {
        _settings.EqHostPath = _eqHostPath;
        var json = JsonSerializer.Serialize(_settings, JsonOptions);
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
            btnRevert.Enabled = false;
            return;
        }

        var fileState = EqHostManager.GetState(_eqHostPath);

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
        btnRevert.Enabled = fileState == MiddlemanState.On;
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

    private void LnkCredit_Click(object? sender, LinkLabelLinkClickedEventArgs e)
    {
        using var proc = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = "https://github.com/sadsfae/easyman",
            UseShellExecute = true
        });
    }

    private void BtnRevert_Click(object? sender, EventArgs e)
    {
        if (_eqHostPath is null)
            return;

        if (_middleman.IsRunning)
        {
            var result = MessageBox.Show(
                "Middleman is still running. Stop it too?",
                "Easyman",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
                _middleman.Stop();
        }

        EqHostManager.SetState(_eqHostPath, MiddlemanState.Off);
        RefreshState();

        MessageBox.Show(
            "eqhost.txt has been reverted to the original login server.",
            "Easyman",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private void BtnSettings_Click(object? sender, EventArgs e)
    {
        using var form = new SettingsForm(_settings);
        bool dark = _settings.UseDarkTheme ?? ThemeHelper.DetectSystemDarkMode();
        ThemeHelper.Apply(form, dark);

        if (form.ShowDialog(this) != DialogResult.OK)
            return;

        _settings.OnCloseAction = form.ChosenCloseAction;
        _settings.UseDarkTheme = form.ChosenTheme;
        SaveSettings();

        try
        {
            AllowedServersManager.Save(form.AllowedServers);

            if (_middleman.IsRunning)
            {
                MessageBox.Show(
                    "Middleman must be restarted for server list changes to take effect.",
                    "Easyman",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Could not save allowed servers: {ex.Message}",
                "Easyman",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        ApplyTheme();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (_middleman.IsRunning)
        {
            switch (_settings.OnCloseAction)
            {
                case CloseAction.RevertOnClose:
                    _middleman.Stop();
                    if (_eqHostPath is not null)
                        EqHostManager.SetState(_eqHostPath, MiddlemanState.Off);
                    break;

                case CloseAction.KeepOnClose:
                    _middleman.Stop();
                    break;

                case CloseAction.Ask:
                default:
                    using (var prompt = new ClosePromptForm())
                    {
                        bool dark = _settings.UseDarkTheme ?? ThemeHelper.DetectSystemDarkMode();
                        ThemeHelper.Apply(prompt, dark);

                        if (prompt.ShowDialog(this) != DialogResult.OK)
                        {
                            e.Cancel = true;
                            return;
                        }

                        if (prompt.RememberChoice)
                        {
                            _settings.OnCloseAction = prompt.ChosenAction;
                            SaveSettings();
                        }

                        _middleman.Stop();
                        if (prompt.ChosenAction == CloseAction.RevertOnClose && _eqHostPath is not null)
                            EqHostManager.SetState(_eqHostPath, MiddlemanState.Off);
                    }
                    break;
            }
        }

        _middleman.Dispose();
        base.OnFormClosing(e);
    }
}

public enum CloseAction
{
    Ask,
    RevertOnClose,
    KeepOnClose
}

public class AppSettings
{
    public string? EqHostPath { get; set; }
    public CloseAction OnCloseAction { get; set; } = CloseAction.Ask;
    public bool? UseDarkTheme { get; set; }
}
