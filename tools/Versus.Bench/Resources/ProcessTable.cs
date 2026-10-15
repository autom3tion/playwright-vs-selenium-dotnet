using System.Globalization;

namespace Versus.Bench.Resources;

/// <summary>Every process on the machine with its parent, resident memory and CPU time so far.</summary>
public static class ProcessTable
{
    // One line per process: "<pid> <ppid> <rss in KB> <cpu seconds>", invariant culture.
    private const string WindowsQuery =
        "Get-CimInstance Win32_Process | ForEach-Object { '{0} {1} {2} {3}' -f $_.ProcessId, $_.ParentProcessId, " +
        "[int64]($_.WorkingSetSize / 1024), (($_.KernelModeTime + $_.UserModeTime) / 1e7).ToString([cultureinfo]::InvariantCulture) }";

    public static async Task<IReadOnlyList<ProcessSample>> SnapshotAsync()
    {
        var result = OperatingSystem.IsWindows()
            ? await Shell.RunAsync("powershell", ["-NoProfile", "-Command", WindowsQuery])
            : await Shell.RunAsync("ps", ["-A", "-o", "pid=,ppid=,rss=,time="]);

        if (result.ExitCode != 0)
        {
            throw new BenchException($"Could not list processes: {result.StdErr}");
        }

        return Parse(result.StdOut);
    }

    public static IReadOnlyList<ProcessSample> Parse(string text)
    {
        var rows = new List<ProcessSample>();
        foreach (var line in text.Split('\n'))
        {
            var f = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (f.Length == 4
                && int.TryParse(f[0], CultureInfo.InvariantCulture, out var pid)
                && int.TryParse(f[1], CultureInfo.InvariantCulture, out var ppid)
                && long.TryParse(f[2], CultureInfo.InvariantCulture, out var rssKb))
            {
                rows.Add(new ProcessSample(pid, ppid, rssKb * 1024, ParseCpuTime(f[3])));
            }
        }

        return rows;
    }

    /// <summary>"m:ss.cc" (macOS), "hh:mm:ss" or "d-hh:mm:ss" (Linux), or plain seconds (Windows query).</summary>
    public static TimeSpan ParseCpuTime(string value)
    {
        var days = 0;
        var dash = value.IndexOf('-');
        if (dash > 0)
        {
            days = int.Parse(value[..dash], CultureInfo.InvariantCulture);
            value = value[(dash + 1)..];
        }

        var seconds = 0d;
        foreach (var part in value.Split(':'))
        {
            seconds = seconds * 60 + double.Parse(part, CultureInfo.InvariantCulture);
        }

        return TimeSpan.FromDays(days) + TimeSpan.FromSeconds(seconds);
    }
}
