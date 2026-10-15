using System.Diagnostics;

namespace Versus.Bench;

public sealed record ShellResult(int ExitCode, string StdOut, string StdErr);

public static class Shell
{
    public static async Task<ShellResult> RunAsync(string fileName, IEnumerable<string> arguments)
    {
        var start = new ProcessStartInfo(fileName)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        foreach (var argument in arguments)
        {
            start.ArgumentList.Add(argument);
        }

        using var process = Process.Start(start) ?? throw new BenchException($"Could not start '{fileName}'.");
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        return new ShellResult(process.ExitCode, await stdout, await stderr);
    }
}
