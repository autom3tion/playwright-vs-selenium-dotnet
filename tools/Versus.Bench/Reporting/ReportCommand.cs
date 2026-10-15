using System.Globalization;
using System.Text.Json;
using Versus.Bench.Options;
using Versus.Bench.Running;
using Versus.Timing;

namespace Versus.Bench.Reporting;

public static class ReportCommand
{
    /// <summary>
    /// Writes results/&lt;date&gt;-&lt;machine&gt;/ and the README block. With --out, writes there and leaves the
    /// README alone, so a smoke run never touches the committed page.
    /// </summary>
    public static int Execute(BenchOptions options, RepoPaths repo)
    {
        var raw = options.RawDirectory ?? repo.RawResults;
        var data = RawLoader.Load(raw);
        var summary = Aggregator.Summarize(data, LocCounter.ForRepo(repo));

        var output = options.OutDirectory ?? DefaultOutput(repo, raw, data.Env);
        Directory.CreateDirectory(output);
        File.WriteAllText(Path.Combine(output, "summary.json"), JsonSerializer.Serialize(summary, TimingSink.Options));
        File.Copy(Path.Combine(raw, RawLayout.EnvFile), Path.Combine(output, "env.json"), overwrite: true);
        ChartWriter.WriteAll(summary, Path.Combine(output, "charts"));
        File.WriteAllText(Path.Combine(output, "report.md"), MarkdownReport.Full(summary));

        if (options.OutDirectory is null)
        {
            var resultsPath = Path.GetRelativePath(repo.Root, output).Replace('\\', '/');
            File.WriteAllText(repo.Readme, ReadmeBlock.Replace(File.ReadAllText(repo.Readme), MarkdownReport.ForReadme(summary, resultsPath)));
        }

        Console.WriteLine($"Report written to {output}");
        Console.WriteLine(Headline.Build(summary));
        return 0;
    }

    /// <summary>
    /// results/&lt;date&gt;-&lt;machine&gt;. When that folder already holds a different run (its env.json differs),
    /// the new one gets "-HHmm" (its UTC start time) instead of overwriting it. The same run reported again overwrites.
    /// </summary>
    private static string DefaultOutput(RepoPaths repo, string raw, EnvInfo env)
    {
        var output = Path.Combine(repo.Results, $"{Format.Date(env.DateUtc)}-{env.Machine}");
        var existingEnv = Path.Combine(output, RawLayout.EnvFile);
        if (File.Exists(existingEnv) && File.ReadAllText(existingEnv) != File.ReadAllText(Path.Combine(raw, RawLayout.EnvFile)))
        {
            output += "-" + env.DateUtc.UtcDateTime.ToString("HHmm", CultureInfo.InvariantCulture);
        }

        return output;
    }
}
