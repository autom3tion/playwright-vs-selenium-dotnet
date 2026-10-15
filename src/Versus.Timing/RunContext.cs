using System.Globalization;

namespace Versus.Timing;

/// <summary>Where and for which round a benchmark run records timings. Absent under a plain dotnet test.</summary>
public sealed record RunContext(string TimingDirectory, int Round, int Workers)
{
    public const string DirectoryVariable = "VERSUS_TIMING_DIR";
    public const string RoundVariable = "VERSUS_ROUND";
    public const string WorkersVariable = "VERSUS_WORKERS";

    public static RunContext? FromEnvironment() => From(Environment.GetEnvironmentVariable);

    public static RunContext? From(Func<string, string?> read)
    {
        var directory = read(DirectoryVariable);
        if (string.IsNullOrWhiteSpace(directory))
        {
            return null;
        }

        return new RunContext(
            directory,
            int.Parse(read(RoundVariable) ?? "0", CultureInfo.InvariantCulture),
            int.Parse(read(WorkersVariable) ?? "1", CultureInfo.InvariantCulture));
    }
}
