using Precept;

namespace Versus.Timing;

/// <summary>Writes the record of the Precept test now in its [AfterTest] hook. Does nothing outside a benchmark run.</summary>
public static class TestTimings
{
    public static void Write(string tool, TestClock clock)
    {
        if (RunContext.FromEnvironment() is not { } run)
        {
            return;
        }

        var test = PreceptTestContext.Current ?? throw new InvalidOperationException("No Precept test is running.");
        var (setupMs, bodyMs, teardownMs) = clock.Finish();

        TimingSink.Write(run, new TimingRecord(
            tool,
            TestKey.From(test.ClassName, test.MethodName),
            run.Round,
            run.Workers,
            setupMs,
            bodyMs,
            teardownMs,
            test.Outcome.ToString(),
            FailureClassifier.Classify(test.Failure),
            test.Failure?.Message,
            clock.StartedUtc));
    }
}
