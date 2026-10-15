using System.Globalization;
using System.Text.Json;
using Versus.Bench.Chrome;
using Versus.Bench.Options;
using Versus.Bench.Reporting;
using Versus.Timing;

namespace Versus.Bench.Running;

public static class RunCommand
{
    public static async Task<int> ExecuteAsync(BenchOptions options, RepoPaths repo)
    {
        var raw = options.RawDirectory ?? repo.RawResults;
        if (Directory.Exists(raw) && Directory.EnumerateFileSystemEntries(raw).Any())
        {
            throw new BenchException($"'{raw}' already holds results. Move or delete it first, so two benchmarks never mix.");
        }

        using var http = new HttpClient();
        var chrome = await new ChromeInstaller(http).EnsureAsync(repo.ChromeCache);
        Console.WriteLine($"Chrome for Testing {chrome.Version}: {chrome.Chrome}");

        Console.WriteLine("Building the test projects in Release…");
        var build = await Shell.RunAsync("dotnet", ["build", repo.Solution, "-c", "Release", "--nologo", "-v", "q"]);
        if (build.ExitCode != 0)
        {
            throw new BenchException("dotnet build failed:\n" + build.StdOut + build.StdErr);
        }

        var sites = SuiteFilter.Sites(
            options.Suite,
            new Uri(options.PlaygroundUrl ?? RunEnvironment.DefaultPlaygroundUrl),
            new Uri(options.ToolshopUrl ?? RunEnvironment.DefaultToolshopUrl));

        var check = new SiteCheck(http, d => Task.Delay(d));
        if (!await check.IsUpAsync(sites))
        {
            throw new BenchException($"A site did not answer: {string.Join(", ", sites)}");
        }

        Directory.CreateDirectory(raw);
        var env = await EnvCollector.CollectAsync(repo, http, sites);
        await File.WriteAllTextAsync(Path.Combine(raw, RawLayout.EnvFile), JsonSerializer.Serialize(env, TimingSink.Options));

        var siteDownReruns = 0;
        try
        {
            foreach (var workers in options.Workers)
            {
                var scheduler = new RoundScheduler(RoundPlan.Rounds(options.Rounds));
                try
                {
                    while (scheduler.TryNext(out var round))
                    {
                        if (!await check.IsUpAsync(sites))
                        {
                            Console.WriteLine($"w{workers} r{round}: a site is down; this round runs again later.");
                            scheduler.SiteDown(round);
                            continue;
                        }

                        foreach (var tool in RoundPlan.Order(round))
                        {
                            var directory = RawLayout.RunDirectory(raw, workers, round, tool);
                            var spec = new RunSpec(tool, round, workers, repo.TestAssembly(tool), directory,
                                RunEnvironment.For(options, chrome, RawLayout.TimingDirectory(directory), round, workers),
                                SuiteFilter.For(options.Suite));
                            var run = await TestProcessRunner.RunAsync(spec);
                            var label = round == RoundPlan.WarmUpRound ? "warm-up" : $"r{round}";
                            Console.WriteLine(string.Create(CultureInfo.InvariantCulture,
                                $"w{workers} {label} {tool}: {run.WallMs / 1000:0.0} s, exit {run.ExitCode}{(run.Crashed ? ", CRASHED" : "")}"));
                        }

                        scheduler.Completed();
                    }
                }
                finally
                {
                    siteDownReruns += scheduler.SiteDownReruns;
                }
            }
        }
        finally
        {
            await File.WriteAllTextAsync(Path.Combine(raw, RawLayout.LogFile),
                JsonSerializer.Serialize(new BenchLog(siteDownReruns), TimingSink.Options));
        }

        return ReportCommand.Execute(options with { RawDirectory = raw }, repo);
    }
}
