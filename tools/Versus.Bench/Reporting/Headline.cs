using System.Globalization;
using Versus.Timing;

namespace Versus.Bench.Reporting;

/// <summary>The one-line summary at the top of the README, generated from the numbers rather than written as an opinion.</summary>
public static class Headline
{
    /// <summary>Pass rates closer than this many percentage points are called equal.</summary>
    public const double PassRateTiePoints = 0.5;

    public static string Build(Summary s)
    {
        var pw = s.PerTool.Single(t => t.Tool == Tools.Playwright);
        var se = s.PerTool.Single(t => t.Tool == Tools.Selenium);
        var sentence = $"{Speed(pw, se)}, {Reliability(pw, se)}, and {Memory(s)}.";
        return char.ToUpperInvariant(sentence[0]) + sentence[1..];
    }

    private static string Speed(ToolStats pw, ToolStats se)
    {
        if (pw.SpeedTotalMs <= 0 || se.SpeedTotalMs <= 0)
        {
            return "total test time could not be compared";
        }

        if (Stats.IsTie(pw.SpeedTotalMs, se.SpeedTotalMs))
        {
            return "total test time was a tie";
        }

        var (fast, slow) = pw.SpeedTotalMs < se.SpeedTotalMs ? (pw, se) : (se, pw);
        return string.Create(CultureInfo.InvariantCulture,
            $"{fast.Tool} needed {slow.SpeedTotalMs / fast.SpeedTotalMs:0.0}× less total test time across {Format.Plural(fast.SpeedTests, "test")}");
    }

    private static string Reliability(ToolStats pw, ToolStats se)
    {
        var points = (pw.PassRate - se.PassRate) * 100;
        if (Math.Abs(points) < PassRateTiePoints)
        {
            return "both passed equally often";
        }

        var better = points > 0 ? pw : se;
        return string.Create(CultureInfo.InvariantCulture, $"{better.Tool} passed {Math.Abs(points):0.0} pp more often");
    }

    private static string Workers(int n) => n == 1 ? "1 worker" : string.Create(CultureInfo.InvariantCulture, $"{n} workers");

    private static string Memory(Summary s)
    {
        var workers = s.Workers.Count > 0 ? s.Workers.Min() : 1;
        var pw = s.Scaling.SingleOrDefault(p => p.Tool == Tools.Playwright && p.Workers == workers);
        var se = s.Scaling.SingleOrDefault(p => p.Tool == Tools.Selenium && p.Workers == workers);
        if (pw is null || se is null)
        {
            return "memory could not be compared";
        }

        if (Stats.IsTie(pw.MedianPeakRssMb, se.MedianPeakRssMb))
        {
            return $"memory use was a tie at {Workers(workers)}";
        }

        var (low, high) = pw.MedianPeakRssMb < se.MedianPeakRssMb ? (pw, se) : (se, pw);
        return string.Create(CultureInfo.InvariantCulture, $"{low.Tool} used {(1 - low.MedianPeakRssMb / high.MedianPeakRssMb) * 100:0}% less memory at {Workers(workers)}");
    }
}
