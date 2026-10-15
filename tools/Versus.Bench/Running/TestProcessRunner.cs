using System.Diagnostics;
using System.Text.Json;
using Versus.Bench.Resources;
using Versus.Timing;

namespace Versus.Bench.Running;

public sealed record RunSpec(
    string Tool,
    int Round,
    int Workers,
    string AssemblyPath,
    string RunDirectory,
    IReadOnlyDictionary<string, string> Environment,
    string? Filter);

public static class TestProcessRunner
{
    public static readonly TimeSpan SampleInterval = TimeSpan.FromMilliseconds(500);

    /// <summary>A run still going after this long is killed, with its whole process tree, and recorded as a crash.</summary>
    public static readonly TimeSpan RunTimeout = TimeSpan.FromMinutes(60);

    /// <summary>The built assembly is started directly, the same way for both tools, so no build step is timed.</summary>
    public static IReadOnlyList<string> Arguments(RunSpec spec) =>
    [
        spec.AssemblyPath,
        "--report-trx",
        "--report-trx-filename", RawLayout.TrxFileName(spec.Tool),
        "--results-directory", spec.RunDirectory,
        .. spec.Filter is null ? Array.Empty<string>() : ["--precept-filter", spec.Filter],
    ];

    /// <summary>0: all passed. 2: some tests failed, which is data. Anything else, or no TRX file: a crash.</summary>
    public static bool IsCrash(int exitCode, bool trxExists) => !trxExists || exitCode is not (0 or 2);

    /// <summary>Runs once; a crash is run once more, and a second crash is recorded as such.</summary>
    public static async Task<RunRecord> RunAsync(RunSpec spec)
    {
        var first = await RunOnceAsync(spec, attempt: 1);
        return first.Crashed ? await RunOnceAsync(spec, attempt: 2) : first;
    }

    private static async Task<RunRecord> RunOnceAsync(RunSpec spec, int attempt)
    {
        var firstAttempt = RawLayout.FirstAttemptDirectory(spec.RunDirectory);
        if (Directory.Exists(firstAttempt))
        {
            // Left over from an earlier bench into the same raw folder, or replaced by this run's own first attempt.
            Directory.Delete(firstAttempt, recursive: true);
        }

        if (Directory.Exists(spec.RunDirectory))
        {
            if (attempt == 1)
            {
                // Left over from an earlier bench into the same raw folder.
                Directory.Delete(spec.RunDirectory, recursive: true);
            }
            else
            {
                // This run's own crashed first attempt: keep it, with its output.log, beside the retry.
                Directory.Move(spec.RunDirectory, firstAttempt);
            }
        }

        Directory.CreateDirectory(spec.RunDirectory);

        var start = new ProcessStartInfo("dotnet")
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        foreach (var argument in Arguments(spec))
        {
            start.ArgumentList.Add(argument);
        }

        foreach (var (name, value) in spec.Environment)
        {
            start.Environment[name] = value;
        }

        var watch = Stopwatch.StartNew();
        using var process = Process.Start(start) ?? throw new BenchException("Could not start dotnet.");
        var sampler = ResourceSampler.Start(process.Id, SampleInterval);
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        ResourceAccumulator usage;
        var timedOut = false;
        try
        {
            using var timeout = new CancellationTokenSource(RunTimeout);
            try
            {
                await process.WaitForExitAsync(timeout.Token);
            }
            catch (OperationCanceledException) when (timeout.IsCancellationRequested)
            {
                timedOut = true;
                process.Kill(entireProcessTree: true);
                await process.WaitForExitAsync();
            }
        }
        finally
        {
            watch.Stop();
            usage = await sampler.StopAsync();
        }

        var log = await stdout + await stderr;
        if (timedOut)
        {
            log += $"{Environment.NewLine}[Versus.Bench] Killed with its process tree after {RunTimeout.TotalMinutes:0} minutes.{Environment.NewLine}";
        }

        await File.WriteAllTextAsync(Path.Combine(spec.RunDirectory, "output.log"), log);

        var trxExists = File.Exists(Path.Combine(spec.RunDirectory, RawLayout.TrxFileName(spec.Tool)));
        var record = new RunRecord(spec.Tool, spec.Round, spec.Workers, watch.Elapsed.TotalMilliseconds,
            usage.PeakRssBytes, usage.CpuSeconds, process.ExitCode, timedOut || IsCrash(process.ExitCode, trxExists), attempt);
        await File.WriteAllTextAsync(Path.Combine(spec.RunDirectory, RawLayout.RunFile), JsonSerializer.Serialize(record, TimingSink.Options));
        return record;
    }
}
