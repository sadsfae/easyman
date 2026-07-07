namespace Easyman;

partial class ClosePromptForm
{
    private System.ComponentModel.IContainer components = null;

    private Label lblMessage;
    private RadioButton rbRevert;
    private RadioButton rbKeep;
    private CheckBox chkRemember;
    private Button btnOk;
    private Button btnCancel;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        lblMessage = new Label();
        rbRevert = new RadioButton();
        rbKeep = new RadioButton();
        chkRemember = new CheckBox();
        btnOk = new Button();
        btnCancel = new Button();
        SuspendLayout();

        // lblMessage
        lblMessage.Location = new Point(12, 12);
        lblMessage.Size = new Size(310, 36);
        lblMessage.Text = "Middleman is still running. What would you like to do when Easyman closes?";

        // rbRevert
        rbRevert.Location = new Point(20, 55);
        rbRevert.Size = new Size(290, 20);
        rbRevert.Text = "Revert eqhost.txt to original and stop middleman";

        // rbKeep
        rbKeep.Location = new Point(20, 80);
        rbKeep.Size = new Size(290, 20);
        rbKeep.Text = "Keep eqhost.txt and stop middleman (recommended)";
        rbKeep.Checked = true;

        // chkRemember
        chkRemember.Location = new Point(20, 115);
        chkRemember.Size = new Size(290, 20);
        chkRemember.Text = "Remember this choice (change in Settings)";

        // btnOk
        btnOk.Location = new Point(150, 150);
        btnOk.Size = new Size(80, 28);
        btnOk.Text = "OK";
        btnOk.DialogResult = DialogResult.OK;

        // btnCancel
        btnCancel.Location = new Point(236, 150);
        btnCancel.Size = new Size(80, 28);
        btnCancel.Text = "Cancel";
        btnCancel.DialogResult = DialogResult.Cancel;

        // ClosePromptForm
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(334, 190);
        Controls.Add(lblMessage);
        Controls.Add(rbRevert);
        Controls.Add(rbKeep);
        Controls.Add(chkRemember);
        Controls.Add(btnOk);
        Controls.Add(btnCancel);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "Easyman";
        AcceptButton = btnOk;
        CancelButton = btnCancel;
        ResumeLayout(false);
        PerformLayout();
    }
}
