using System.Globalization;

namespace Versus.Bench.Running;

/// <summary>
/// results/raw/env.json, results/raw/bench.json, and one folder per run:
/// results/raw/w&lt;workers&gt;/r&lt;round&gt;/&lt;tool&gt;/{run.json, &lt;tool&gt;.trx, output.log, timing/}, and
/// &lt;tool&gt;.attempt1/ beside it when the first attempt crashed and was retried.
/// </summary>
public static class RawLayout
{
    public const string EnvFile = "env.json";
    public const string LogFile = "bench.json";
    public const string RunFile = "run.json";

    public static string RunDirectory(string raw, int workers, int round, string tool) =>
        Path.Combine(raw, $"w{workers}", "r" + round.ToString("D3", CultureInfo.InvariantCulture), tool);

    /// <summary>Where a crashed first attempt is kept, beside the run folder, so its output.log survives the retry.</summary>
    public static string FirstAttemptDirectory(string runDirectory) => runDirectory + AttemptSuffix;

    /// <summary>True for a kept first attempt; RawLoader never loads its run.json.</summary>
    public static bool IsFirstAttemptDirectory(string directory) =>
        Path.GetFileName(directory).EndsWith(AttemptSuffix, StringComparison.Ordinal);

    private const string AttemptSuffix = ".attempt1";

    public static string TimingDirectory(string runDirectory) => Path.Combine(runDirectory, "timing");

    public static string TrxFileName(string tool) => $"{tool}.trx";
}
