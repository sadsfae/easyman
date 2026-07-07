namespace Easyman;

public enum MiddlemanState
{
    Off,
    On,
    Unknown
}

public static class EqHostManager
{
    private const string LoginHeader = "[LoginServer]";
    private const string MiddlemanHost = "Host=localhost:5998";
    private const string OriginalHost = "Host=login.eqemulator.net:5998";

    public static MiddlemanState GetState(string filePath)
    {
        if (!File.Exists(filePath))
            return MiddlemanState.Unknown;

        var lines = File.ReadAllLines(filePath);
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (trimmed.Equals(MiddlemanHost, StringComparison.OrdinalIgnoreCase))
                return MiddlemanState.On;
            if (trimmed.Equals(OriginalHost, StringComparison.OrdinalIgnoreCase))
                return MiddlemanState.Off;
        }

        return MiddlemanState.Unknown;
    }

    public static void SetState(string filePath, MiddlemanState state)
    {
        string content = state switch
        {
            MiddlemanState.On =>
                $"{LoginHeader}\r\n{MiddlemanHost}\r\n#{OriginalHost}\r\n",
            MiddlemanState.Off =>
                $"{LoginHeader}\r\n{OriginalHost}\r\n",
            _ => throw new ArgumentException("Cannot set state to Unknown")
        };

        File.WriteAllText(filePath, content);
    }
}
