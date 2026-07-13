namespace Easyman;

public static class AllowedServersManager
{
    private const string FileName = "allowed_emu.txt";
    private static readonly string[] DefaultServers = ["Ryhoz world"];

    public static string GetFilePath()
    {
        return Path.Combine(AppContext.BaseDirectory, FileName);
    }

    public static List<string> Load()
    {
        var path = GetFilePath();
        if (!File.Exists(path))
            return new List<string>(DefaultServers);

        var servers = new List<string>();
        foreach (var line in File.ReadAllLines(path))
        {
            var trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith('#') || trimmed.StartsWith(';'))
                continue;
            servers.Add(trimmed);
        }

        return servers;
    }

    public static void Save(IEnumerable<string> servers)
    {
        var path = GetFilePath();
        var lines = new List<string>
        {
            "# Allowed EMU server names (case-insensitive substring match)",
            "# One server name per line. Lines starting with # or ; are comments.",
            "# Servers whose name starts with \"Project 1999\" are always allowed."
        };
        lines.AddRange(servers);
        File.WriteAllLines(path, lines);
    }

    public static List<string> GetDefaults()
    {
        return new List<string>(DefaultServers);
    }
}
