using System.Text.Json;
using System.Text.Json.Serialization;

namespace Easyman;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        // SetColorMode requires Windows 11+ (build 22000+)
        if (Environment.OSVersion.Version.Build >= 22000)
        {
            var colorMode = ResolveColorMode();
            Application.SetColorMode(colorMode);
        }

        Application.Run(new MainForm());
    }

    private static SystemColorMode ResolveColorMode()
    {
        var settingsPath = Path.Combine(AppContext.BaseDirectory, "easyman-settings.json");
        if (!File.Exists(settingsPath))
            return SystemColorMode.System;

        try
        {
            var json = File.ReadAllText(settingsPath);
            var options = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() }
            };
            var settings = JsonSerializer.Deserialize<AppSettings>(json, options);

            return settings?.UseDarkTheme switch
            {
                true => SystemColorMode.Dark,
                false => SystemColorMode.Classic,
                _ => SystemColorMode.System
            };
        }
        catch
        {
            return SystemColorMode.System;
        }
    }
}
