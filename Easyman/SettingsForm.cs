namespace Easyman;

public partial class SettingsForm : Form
{
    private readonly bool? _themeSetting;

    public CloseAction ChosenCloseAction { get; private set; }
    public bool? ChosenTheme { get; private set; }
    public List<string> AllowedServers { get; private set; } = new();

    public SettingsForm(AppSettings current)
    {
        InitializeComponent();
        _themeSetting = current.UseDarkTheme;

        switch (current.OnCloseAction)
        {
            case CloseAction.RevertOnClose: rbCloseRevert.Checked = true; break;
            case CloseAction.KeepOnClose: rbCloseKeep.Checked = true; break;
            default: rbCloseAsk.Checked = true; break;
        }

        switch (current.UseDarkTheme)
        {
            case true: rbThemeDark.Checked = true; break;
            case false: rbThemeLight.Checked = true; break;
            default: rbThemeSystem.Checked = true; break;
        }

        try
        {
            AllowedServers = AllowedServersManager.Load();
        }
        catch
        {
            AllowedServers = AllowedServersManager.GetDefaults();
        }

        foreach (var server in AllowedServers)
            lstServers.Items.Add(server);
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        if (rbCloseRevert.Checked)
            ChosenCloseAction = CloseAction.RevertOnClose;
        else if (rbCloseKeep.Checked)
            ChosenCloseAction = CloseAction.KeepOnClose;
        else
            ChosenCloseAction = CloseAction.Ask;

        if (rbThemeDark.Checked)
            ChosenTheme = true;
        else if (rbThemeLight.Checked)
            ChosenTheme = false;
        else
            ChosenTheme = null;

        AllowedServers = lstServers.Items.Cast<string>().ToList();
    }

    private void BtnAddServer_Click(object? sender, EventArgs e)
    {
        using var prompt = new AddServerPromptForm();
        bool dark = _themeSetting ?? ThemeHelper.DetectSystemDarkMode();
        ThemeHelper.Apply(prompt, dark);

        if (prompt.ShowDialog(this) != DialogResult.OK)
            return;

        var name = prompt.ServerName;
        if (string.IsNullOrWhiteSpace(name))
            return;

        if (name.StartsWith('#') || name.StartsWith(';'))
        {
            MessageBox.Show(
                "Server names cannot start with # or ; (reserved for comments).",
                "Easyman",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        lstServers.Items.Add(name);
    }

    private void BtnRemoveServer_Click(object? sender, EventArgs e)
    {
        if (lstServers.SelectedIndex < 0)
            return;
        lstServers.Items.RemoveAt(lstServers.SelectedIndex);
    }

    private void BtnRevertServers_Click(object? sender, EventArgs e)
    {
        lstServers.Items.Clear();
        foreach (var server in AllowedServersManager.GetDefaults())
            lstServers.Items.Add(server);
    }
}
