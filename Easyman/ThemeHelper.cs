using Microsoft.Win32;

namespace Easyman;

public static class ThemeHelper
{
    private static readonly Color DarkBackground = Color.FromArgb(30, 30, 30);
    private static readonly Color DarkSurface = Color.FromArgb(45, 45, 45);
    private static readonly Color DarkText = Color.FromArgb(220, 220, 220);
    private static readonly Color DarkMutedText = Color.FromArgb(140, 140, 140);

    public static bool DetectSystemDarkMode()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            var value = key?.GetValue("AppsUseLightTheme");
            if (value is int intVal)
                return intVal == 0;
        }
        catch
        {
        }
        return false;
    }

    public static void Apply(Form form, bool dark)
    {
        if (dark)
        {
            form.BackColor = DarkBackground;
            form.ForeColor = DarkText;
        }
        else
        {
            form.BackColor = SystemColors.Control;
            form.ForeColor = SystemColors.ControlText;
        }

        ApplyToControls(form.Controls, dark);
    }

    private static void ApplyToControls(Control.ControlCollection controls, bool dark)
    {
        foreach (Control ctrl in controls)
        {
            switch (ctrl)
            {
                case TextBox tb:
                    tb.BackColor = dark ? DarkSurface : SystemColors.Window;
                    tb.ForeColor = dark ? DarkText : SystemColors.WindowText;
                    break;
                case Button btn:
                    btn.BackColor = dark ? DarkSurface : SystemColors.Control;
                    btn.ForeColor = dark ? DarkText : SystemColors.ControlText;
                    btn.FlatStyle = dark ? FlatStyle.Flat : FlatStyle.Standard;
                    break;
                case LinkLabel ll:
                    ll.LinkColor = dark ? Color.FromArgb(100, 160, 255) : Color.Blue;
                    ll.ActiveLinkColor = dark ? Color.FromArgb(140, 190, 255) : Color.Red;
                    ll.ForeColor = dark ? DarkMutedText : SystemColors.GrayText;
                    break;
                case Label lbl when lbl.ForeColor == SystemColors.GrayText || lbl.ForeColor == DarkMutedText:
                    lbl.ForeColor = dark ? DarkMutedText : SystemColors.GrayText;
                    break;
                case Label lbl:
                    lbl.ForeColor = dark ? DarkText : SystemColors.ControlText;
                    break;
                case RadioButton rb:
                    rb.ForeColor = dark ? DarkText : SystemColors.ControlText;
                    break;
                case CheckBox cb:
                    cb.ForeColor = dark ? DarkText : SystemColors.ControlText;
                    break;
                case GroupBox gb:
                    gb.ForeColor = dark ? DarkText : SystemColors.ControlText;
                    break;
                case Panel pnl when pnl.Size is { Width: 16, Height: 16 }:
                    break;
            }

            if (ctrl.HasChildren)
                ApplyToControls(ctrl.Controls, dark);
        }
    }
}
