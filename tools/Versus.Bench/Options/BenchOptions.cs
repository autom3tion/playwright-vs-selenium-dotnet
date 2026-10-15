using System.Globalization;

namespace Versus.Bench.Options;

public enum BenchCommand { Run, Report }

public enum Suite { All, Flakiness, Journeys }

public sealed record BenchOptions(
    BenchCommand Command,
    int Rounds,
    IReadOnlyList<int> Workers,
    Suite Suite,
    bool AllowHighLoad,
    string? PlaygroundUrl,
    string? ToolshopUrl,
    string? RawDirectory,
    string? OutDirectory)
{
    public const int DefaultRounds = 20;

    /// <summary>The most workers allowed against the free hosted sites.</summary>
    public const int PoliteWorkerLimit = 4;

    public static readonly IReadOnlyList<int> DefaultWorkers = [1, 2, 4];

    public const string Usage = """
        Usage:
          dotnet run --project tools/Versus.Bench -- run [--rounds 20] [--workers 1,2,4] [--suite all|flakiness|journeys]
                                                       [--playground-url URL] [--toolshop-url URL] [--allow-high-load]
                                                       [--raw DIR] [--out DIR]
          dotnet run --project tools/Versus.Bench -- report [--raw DIR] [--out DIR]
        """;

    public static BenchOptions Parse(IReadOnlyList<string> args)
    {
        if (args.Count == 0)
        {
            throw new BenchException(Usage);
        }

        var command = args[0] switch
        {
            "run" => BenchCommand.Run,
            "report" => BenchCommand.Report,
            var other => throw new BenchException($"Unknown command '{other}'.\n{Usage}"),
        };

        var options = new BenchOptions(command, DefaultRounds, DefaultWorkers, Suite.All, false, null, null, null, null);

        for (var i = 1; i < args.Count; i++)
        {
            var name = args[i];
            if (name == "--allow-high-load")
            {
                options = options with { AllowHighLoad = true };
                continue;
            }

            var value = i + 1 < args.Count ? args[++i] : throw new BenchException($"{name} needs a value.");
            options = name switch
            {
                "--rounds" => options with { Rounds = Positive(name, value) },
                "--workers" => options with { Workers = value.Split(',').Select(v => Positive(name, v)).Distinct().Order().ToArray() },
                "--suite" => options with
                {
                    Suite = Enum.TryParse<Suite>(value, ignoreCase: true, out var suite)
                        ? suite
                        : throw new BenchException($"--suite is all, flakiness or journeys, not '{value}'."),
                },
                "--playground-url" => options with { PlaygroundUrl = value },
                "--toolshop-url" => options with { ToolshopUrl = value },
                "--raw" => options with { RawDirectory = value },
                "--out" => options with { OutDirectory = value },
                _ => throw new BenchException($"Unknown option '{name}'.\n{Usage}"),
            };
        }

        if (!options.AllowHighLoad && options.Workers.Max() > PoliteWorkerLimit)
        {
            throw new BenchException(
                $"More than {PoliteWorkerLimit} workers puts real load on free community sites. " +
                "Pass --allow-high-load only when the URLs point at your own copies.");
        }

        return options;
    }

    private static int Positive(string name, string value) =>
        int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var n) && n > 0
            ? n
            : throw new BenchException($"{name} takes a whole number above 0, not '{value}'.");
}
