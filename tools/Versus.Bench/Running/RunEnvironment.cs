using System.Globalization;
using Versus.Bench.Chrome;
using Versus.Bench.Options;
using Versus.Timing;

namespace Versus.Bench.Running;

public static class RunEnvironment
{
    public const string DefaultPlaygroundUrl = "http://uitestingplayground.com";
    public const string DefaultToolshopUrl = "https://practicesoftwaretesting.com";

    public static IReadOnlyDictionary<string, string> For(
        BenchOptions options, ChromePaths chrome, string timingDirectory, int round, int workers)
    {
        var env = new Dictionary<string, string>
        {
            ["PRECEPT_MAXPARALLELISM"] = workers.ToString(CultureInfo.InvariantCulture),
            ["PRECEPT_VERSUS__CHROMEPATH"] = chrome.Chrome,
            ["PRECEPT_VERSUS__CHROMEDRIVERPATH"] = chrome.ChromeDriver,
            [RunContext.DirectoryVariable] = timingDirectory,
            [RunContext.RoundVariable] = round.ToString(CultureInfo.InvariantCulture),
            [RunContext.WorkersVariable] = workers.ToString(CultureInfo.InvariantCulture),
        };

        if (options.PlaygroundUrl is { } playground)
        {
            env["PRECEPT_VERSUS__PLAYGROUNDURL"] = playground;
        }

        if (options.ToolshopUrl is { } toolshop)
        {
            env["PRECEPT_VERSUS__TOOLSHOPURL"] = toolshop;
        }

        return env;
    }
}
