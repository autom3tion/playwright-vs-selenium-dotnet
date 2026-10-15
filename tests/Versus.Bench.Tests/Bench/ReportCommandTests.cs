using System.Text.Json;
using Precept;
using Versus.Bench;
using Versus.Bench.Options;
using Versus.Bench.Reporting;
using Versus.Bench.Running;
using Versus.Timing;

namespace Versus.Bench.Tests.Bench;

[TestSuite("ReportCommand")]
public class ReportCommandTests
{
    [Test("Turns a raw folder into summary, report, charts and the README block")]
    public async Task EndToEnd()
    {
        var root = Directory.CreateTempSubdirectory("versus-report-").FullName;
        var repo = new RepoPaths(root);
        File.WriteAllText(repo.Solution, "<Solution />");
        File.WriteAllText(repo.Readme, "# Versus\n<!-- results:start -->\n<!-- results:end -->\n");
        foreach (var tool in Tools.All)
        {
            Directory.CreateDirectory(Path.Combine(root, "tests", $"Versus.{tool}.Tests"));
        }

        Directory.CreateDirectory(repo.RawResults);
        File.WriteAllText(Path.Combine(repo.RawResults, RawLayout.EnvFile), JsonSerializer.Serialize(Samples.Env, TimingSink.Options));
        foreach (var tool in Tools.All)
        {
            var dir = RawLayout.RunDirectory(repo.RawResults, 1, 1, tool);
            Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir, RawLayout.RunFile), JsonSerializer.Serialize(Samples.Run(tool, 1, 60_000, 500), TimingSink.Options));
            TimingSink.Write(new RunContext(RawLayout.TimingDirectory(dir), 1, 1),
                Samples.Timing(tool, "Flakiness.AjaxTests.LabelAppearsAfterAjaxRequest", 1, true, 15_000));
        }

        var exit = ReportCommand.Execute(BenchOptions.Parse(["report"]), repo);

        var output = Path.Combine(repo.Results, "2026-10-20-test-machine");
        await Assert.That(exit).ToBeEqualAsync(0);
        await Assert.That(File.Exists(Path.Combine(output, "summary.json"))).ToBeTrueAsync();
        await Assert.That(File.Exists(Path.Combine(output, "env.json"))).ToBeTrueAsync();
        await Assert.That(File.Exists(Path.Combine(output, "report.md"))).ToBeTrueAsync();
        await Assert.That(Directory.GetFiles(Path.Combine(output, "charts"), "*.svg")).ToHaveCountAsync(ChartWriter.Charts.Count * 2);
        await Assert.That(File.ReadAllText(repo.Readme)).ToContainAsync("results/2026-10-20-test-machine/report.md");
    }

    [Test("With --out the README is left alone")]
    public async Task OutLeavesReadme()
    {
        var root = Directory.CreateTempSubdirectory("versus-report-out-").FullName;
        var repo = new RepoPaths(root);
        File.WriteAllText(repo.Readme, "# Versus\n<!-- results:start -->\n<!-- results:end -->\n");
        foreach (var tool in Tools.All)
        {
            Directory.CreateDirectory(Path.Combine(root, "tests", $"Versus.{tool}.Tests"));
        }

        Directory.CreateDirectory(repo.RawResults);
        File.WriteAllText(Path.Combine(repo.RawResults, RawLayout.EnvFile), JsonSerializer.Serialize(Samples.Env, TimingSink.Options));

        ReportCommand.Execute(BenchOptions.Parse(["report", "--out", Path.Combine(root, "elsewhere")]), repo);

        await Assert.That(File.Exists(Path.Combine(root, "elsewhere", "report.md"))).ToBeTrueAsync();
        await Assert.That(File.ReadAllText(repo.Readme)).ToBeEqualAsync("# Versus\n<!-- results:start -->\n<!-- results:end -->\n");
    }

    private static RepoPaths NewRepo(EnvInfo env)
    {
        var root = Directory.CreateTempSubdirectory("versus-report-again-").FullName;
        var repo = new RepoPaths(root);
        File.WriteAllText(repo.Readme, "# Versus\n<!-- results:start -->\n<!-- results:end -->\n");
        foreach (var tool in Tools.All)
        {
            Directory.CreateDirectory(Path.Combine(root, "tests", $"Versus.{tool}.Tests"));
        }

        Directory.CreateDirectory(repo.RawResults);
        File.WriteAllText(Path.Combine(repo.RawResults, RawLayout.EnvFile), JsonSerializer.Serialize(env, TimingSink.Options));
        return repo;
    }

    [Test("A second run on the same day gets its own folder, suffixed with its UTC start time")]
    public async Task SecondRunSameDay()
    {
        var repo = NewRepo(Samples.Env);
        ReportCommand.Execute(BenchOptions.Parse(["report"]), repo);
        var first = Path.Combine(repo.Results, "2026-10-20-test-machine");
        var firstEnv = File.ReadAllText(Path.Combine(first, "env.json"));

        var later = Samples.Env with { DateUtc = DateTimeOffset.Parse("2026-10-20T14:35:00Z") };
        File.WriteAllText(Path.Combine(repo.RawResults, RawLayout.EnvFile), JsonSerializer.Serialize(later, TimingSink.Options));
        ReportCommand.Execute(BenchOptions.Parse(["report"]), repo);

        await Assert.That(File.ReadAllText(Path.Combine(first, "env.json"))).ToBeEqualAsync(firstEnv);
        await Assert.That(File.Exists(Path.Combine(repo.Results, "2026-10-20-test-machine-1435", "report.md"))).ToBeTrueAsync();
        await Assert.That(File.ReadAllText(repo.Readme)).ToContainAsync("results/2026-10-20-test-machine-1435/report.md");
    }

    [Test("Reporting the same raw run again overwrites its own folder")]
    public async Task SameRunAgain()
    {
        var repo = NewRepo(Samples.Env);
        ReportCommand.Execute(BenchOptions.Parse(["report"]), repo);
        ReportCommand.Execute(BenchOptions.Parse(["report"]), repo);

        await Assert.That(Directory.GetDirectories(repo.Results, "2026-10-20-test-machine*")).ToHaveCountAsync(1);
    }
}
