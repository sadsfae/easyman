namespace Easyman;

public partial class SettingsForm : Form
{
    public CloseAction ChosenCloseAction { get; private set; }
    public bool? ChosenTheme { get; private set; }

    public SettingsForm(AppSettings current)
    {
        InitializeComponent();

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
    }
}
