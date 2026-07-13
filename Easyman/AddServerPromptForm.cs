namespace Easyman;

public partial class AddServerPromptForm : Form
{
    public string ServerName => txtServerName.Text.Trim();

    public AddServerPromptForm()
    {
        InitializeComponent();
    }
}
