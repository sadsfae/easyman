using Microsoft.Win32;

namespace Easyman;

public static class ThemeHelper
{
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
}
