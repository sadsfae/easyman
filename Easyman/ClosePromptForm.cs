namespace Easyman;

public partial class ClosePromptForm : Form
{
    public CloseAction ChosenAction =>
        rbRevert.Checked ? CloseAction.RevertOnClose : CloseAction.KeepOnClose;

    public bool RememberChoice => chkRemember.Checked;

    public ClosePromptForm()
    {
        InitializeComponent();
    }
}
