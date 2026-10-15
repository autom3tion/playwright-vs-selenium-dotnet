namespace Versus.Timing;

/// <summary>One run of one test, as its fixture saw it. Written by [AfterTest], read by Versus.Bench.</summary>
public sealed record TimingRecord(
    string Tool,
    string TestKey,
    int Round,
    int Workers,
    double SetupMs,
    double BodyMs,
    double TeardownMs,
    string Outcome,
    string? FailureType,
    string? FailureMessage,
    DateTimeOffset StartedUtc)
{
    /// <summary>Test key of the record Playwright writes for its once-per-run browser launch.</summary>
    public const string LaunchKey = "_launch";
}
