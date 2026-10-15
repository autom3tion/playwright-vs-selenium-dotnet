using System.Text.Json;
using Versus.Bench.Running;
using Versus.Timing;

namespace Versus.Bench.Reporting;

public sealed record RawData(EnvInfo Env, BenchLog Log, IReadOnlyList<RunRecord> Runs, IReadOnlyList<TimingRecord> Timings);

public static class RawLoader
{
    public static RawData Load(string raw)
    {
        var env = Read<EnvInfo>(Path.Combine(raw, RawLayout.EnvFile));
        var logPath = Path.Combine(raw, RawLayout.LogFile);
        var log = File.Exists(logPath) ? Read<BenchLog>(logPath) : new BenchLog(0);

        var runs = new List<RunRecord>();
        var timings = new List<TimingRecord>();

        foreach (var runFile in Directory.EnumerateFiles(raw, RawLayout.RunFile, SearchOption.AllDirectories))
        {
            // A crashed first attempt is kept for its output.log only; the retry's run.json is the record.
            if (RawLayout.IsFirstAttemptDirectory(Path.GetDirectoryName(runFile)!))
            {
                continue;
            }

            var run = Read<RunRecord>(runFile);
            if (run.Round == RoundPlan.WarmUpRound)
            {
                continue;
            }

            runs.Add(run);
            var runDirectory = Path.GetDirectoryName(runFile)!;
            var timingDirectory = RawLayout.TimingDirectory(runDirectory);
            var recorded = Directory.Exists(timingDirectory)
                ? Directory.EnumerateFiles(timingDirectory, "*.json", SearchOption.AllDirectories).Select(TimingSink.Read).ToList()
                : [];
            timings.AddRange(recorded);

            // A test the TRX file knows about but that wrote no record still ran, and failed.
            var trx = Path.Combine(runDirectory, RawLayout.TrxFileName(run.Tool));
            if (!File.Exists(trx))
            {
                continue;
            }

            var keys = recorded.Select(r => r.TestKey).ToHashSet();
            foreach (var result in TrxReader.Read(trx))
            {
                var key = TestKey.From(result.ClassName, result.MethodName);
                if (!keys.Contains(key))
                {
                    timings.Add(new TimingRecord(run.Tool, key, run.Round, run.Workers, 0, 0, 0, "Failed",
                        FailureType.Harness, result.ErrorMessage ?? "The test wrote no timing record.", DateTimeOffset.MinValue));
                }
            }
        }

        return new RawData(env, log, runs, timings);
    }

    private static T Read<T>(string path) =>
        JsonSerializer.Deserialize<T>(File.ReadAllText(path), TimingSink.Options)
        ?? throw new BenchException($"'{path}' is empty.");
}
