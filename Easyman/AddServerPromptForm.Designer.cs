namespace Easyman;

partial class AddServerPromptForm
{
    private System.ComponentModel.IContainer components = null;

    private Label lblPrompt;
    private TextBox txtServerName;
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
        lblPrompt = new Label();
        txtServerName = new TextBox();
        btnOk = new Button();
        btnCancel = new Button();
        SuspendLayout();

        // lblPrompt
        lblPrompt.Location = new Point(12, 12);
        lblPrompt.Size = new Size(310, 20);
        lblPrompt.Text = "Server name (partial match, case-insensitive):";

        // txtServerName
        txtServerName.Location = new Point(12, 36);
        txtServerName.Size = new Size(310, 23);
        txtServerName.MaxLength = 127;

        // btnOk
        btnOk.Location = new Point(160, 72);
        btnOk.Size = new Size(75, 28);
        btnOk.Text = "OK";
        btnOk.DialogResult = DialogResult.OK;

        // btnCancel
        btnCancel.Location = new Point(245, 72);
        btnCancel.Size = new Size(75, 28);
        btnCancel.Text = "Cancel";
        btnCancel.DialogResult = DialogResult.Cancel;

        // AddServerPromptForm
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(334, 112);
        Controls.Add(lblPrompt);
        Controls.Add(txtServerName);
        Controls.Add(btnOk);
        Controls.Add(btnCancel);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "Add Allowed Server";
        AcceptButton = btnOk;
        CancelButton = btnCancel;
        ResumeLayout(false);
        PerformLayout();
    }
}
