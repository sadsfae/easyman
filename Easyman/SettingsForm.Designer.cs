namespace Easyman;

partial class SettingsForm
{
    private System.ComponentModel.IContainer components = null;

    private GroupBox grpCloseBehavior;
    private RadioButton rbCloseAsk;
    private RadioButton rbCloseRevert;
    private RadioButton rbCloseKeep;

    private GroupBox grpTheme;
    private RadioButton rbThemeSystem;
    private RadioButton rbThemeLight;
    private RadioButton rbThemeDark;

    private Button btnSave;
    private Button btnCancel;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        grpCloseBehavior = new GroupBox();
        rbCloseAsk = new RadioButton();
        rbCloseRevert = new RadioButton();
        rbCloseKeep = new RadioButton();

        grpTheme = new GroupBox();
        rbThemeSystem = new RadioButton();
        rbThemeLight = new RadioButton();
        rbThemeDark = new RadioButton();

        btnSave = new Button();
        btnCancel = new Button();
        SuspendLayout();

        // grpCloseBehavior
        grpCloseBehavior.Text = "On Close (while middleman is running)";
        grpCloseBehavior.Location = new Point(12, 12);
        grpCloseBehavior.Size = new Size(310, 100);

        // rbCloseAsk
        rbCloseAsk.Text = "Ask me each time";
        rbCloseAsk.Location = new Point(12, 22);
        rbCloseAsk.Size = new Size(280, 20);
        rbCloseAsk.Checked = true;

        // rbCloseRevert
        rbCloseRevert.Text = "Revert eqhost.txt and stop middleman";
        rbCloseRevert.Location = new Point(12, 46);
        rbCloseRevert.Size = new Size(280, 20);

        // rbCloseKeep
        rbCloseKeep.Text = "Keep eqhost.txt and stop middleman";
        rbCloseKeep.Location = new Point(12, 70);
        rbCloseKeep.Size = new Size(280, 20);

        grpCloseBehavior.Controls.Add(rbCloseAsk);
        grpCloseBehavior.Controls.Add(rbCloseRevert);
        grpCloseBehavior.Controls.Add(rbCloseKeep);

        // grpTheme
        grpTheme.Text = "Theme";
        grpTheme.Location = new Point(12, 120);
        grpTheme.Size = new Size(310, 100);

        // rbThemeSystem
        rbThemeSystem.Text = "Follow system theme";
        rbThemeSystem.Location = new Point(12, 22);
        rbThemeSystem.Size = new Size(280, 20);
        rbThemeSystem.Checked = true;

        // rbThemeLight
        rbThemeLight.Text = "Light";
        rbThemeLight.Location = new Point(12, 46);
        rbThemeLight.Size = new Size(280, 20);

        // rbThemeDark
        rbThemeDark.Text = "Dark";
        rbThemeDark.Location = new Point(12, 70);
        rbThemeDark.Size = new Size(280, 20);

        grpTheme.Controls.Add(rbThemeSystem);
        grpTheme.Controls.Add(rbThemeLight);
        grpTheme.Controls.Add(rbThemeDark);

        // btnSave
        btnSave.Text = "Save";
        btnSave.Location = new Point(160, 232);
        btnSave.Size = new Size(75, 28);
        btnSave.DialogResult = DialogResult.OK;
        btnSave.Click += BtnSave_Click;

        // btnCancel
        btnCancel.Text = "Cancel";
        btnCancel.Location = new Point(245, 232);
        btnCancel.Size = new Size(75, 28);
        btnCancel.DialogResult = DialogResult.Cancel;

        // SettingsForm
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(334, 272);
        Controls.Add(grpCloseBehavior);
        Controls.Add(grpTheme);
        Controls.Add(btnSave);
        Controls.Add(btnCancel);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "Easyman Settings";
        AcceptButton = btnSave;
        CancelButton = btnCancel;
        ResumeLayout(false);
        PerformLayout();
    }
}
