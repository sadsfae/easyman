using System.Diagnostics;

namespace Easyman;

public class MiddlemanProcess : IDisposable
{
    private const string ProcessName = "middleman";
    private Process? _process;

    public bool IsRunning =>
        (_process is not null && !_process.HasExited) || HasExistingProcess();

    public static bool HasExistingProcess()
    {
        var procs = Process.GetProcessesByName(ProcessName);
        bool found = procs.Length > 0;
        foreach (var p in procs)
            p.Dispose();
        return found;
    }

    public static void KillExisting()
    {
        foreach (var proc in Process.GetProcessesByName(ProcessName))
        {
            try
            {
                proc.Kill(entireProcessTree: true);
                proc.WaitForExit(3000);
            }
            catch (InvalidOperationException) { }
            finally
            {
                proc.Dispose();
            }
        }
    }

    public void Start(string middlemanExePath)
    {
        if (_process is not null && !_process.HasExited)
            return;

        if (_process is not null)
        {
            _process.Dispose();
            _process = null;
        }

        if (HasExistingProcess())
        {
            KillExisting();
        }

        if (!File.Exists(middlemanExePath))
            throw new FileNotFoundException(
                "middleman.exe was not found alongside Easyman.",
                middlemanExePath);

        _process = Process.Start(new ProcessStartInfo
        {
            FileName = middlemanExePath,
            WorkingDirectory = Path.GetDirectoryName(middlemanExePath),
            UseShellExecute = false,
            CreateNoWindow = true
        });
    }

    public void Stop()
    {
        if (_process is not null && !_process.HasExited)
        {
            try
            {
                _process.Kill(entireProcessTree: true);
                _process.WaitForExit(3000);
            }
            catch (InvalidOperationException) { }
            finally
            {
                _process.Dispose();
                _process = null;
            }
        }

        if (HasExistingProcess())
            KillExisting();
    }

    public void Dispose()
    {
        Stop();
        _process?.Dispose();
        _process = null;
        GC.SuppressFinalize(this);
    }
}
