using ScottPlot;
using Versus.Timing;

namespace Versus.Bench.Reporting;

public static class ChartWriter
{
    public static readonly IReadOnlyList<(string File, string Alt)> Charts =
    [
        ("body-time", "Median test time per test at 1 worker, Playwright and Selenium"),
        ("setup-time", "Median setup and teardown time per test"),
        ("suite-time", "Median suite time at each worker count"),
        ("memory", "Median peak memory at each worker count"),
    ];

    private sealed record Theme(string Name, Color Foreground, Color Grid, Color Playwright, Color Selenium);

    // Checked with the dataviz skill's validator (Task 13 Step 1).
    private static readonly Theme[] Themes =
    [
        new("light", Color.FromHex("#1F2937"), Color.FromHex("#E5E7EB"), Color.FromHex("#2563EB"), Color.FromHex("#D97706")),
        new("dark", Color.FromHex("#E5E7EB"), Color.FromHex("#374151"), Color.FromHex("#3B82F6"), Color.FromHex("#D97706")),
    ];

    public static IReadOnlyList<string> WriteAll(Summary s, string directory)
    {
        Directory.CreateDirectory(directory);
        var written = new List<string>();

        var timed = s.PerTest
            .GroupBy(t => t.TestKey)
            .Where(g => g.Count(t => t.MedianBodyMs is not null) == Tools.All.Count)
            .Select(g => (
                Label: g.Key.Split('.')[1].Replace("Tests", "", StringComparison.Ordinal),
                Playwright: g.Single(t => t.Tool == Tools.Playwright).MedianBodyMs!.Value / 1000,
                Selenium: g.Single(t => t.Tool == Tools.Selenium).MedianBodyMs!.Value / 1000))
            .ToList();
        var pw = s.PerTool.Single(t => t.Tool == Tools.Playwright);
        var se = s.PerTool.Single(t => t.Tool == Tools.Selenium);

        foreach (var theme in Themes)
        {
            written.Add(Save(
                PairedBars(theme, timed.Select(t => t.Label).ToList(), timed.Select(t => t.Playwright).ToList(), timed.Select(t => t.Selenium).ToList(), "Seconds (median, 1 worker)"),
                directory, "body-time", theme, 900, Math.Max(240, 60 + 36 * timed.Count)));
            written.Add(Save(
                PairedBars(theme, ["Setup", "Teardown"], [pw.MedianSetupMs ?? 0, pw.MedianTeardownMs ?? 0], [se.MedianSetupMs ?? 0, se.MedianTeardownMs ?? 0], "Milliseconds (median)"),
                directory, "setup-time", theme, 900, 240));
            written.Add(Save(
                Lines(theme, s, p => p.MedianWallMs / 1000, "Suite time, seconds (median)"),
                directory, "suite-time", theme, 900, 420));
            written.Add(Save(
                Lines(theme, s, p => p.MedianPeakRssMb, "Peak memory, MB (median)"),
                directory, "memory", theme, 900, 420));
        }

        return written;
    }

    private static Plot PairedBars(Theme theme, IReadOnlyList<string> labels, IReadOnlyList<double> playwright, IReadOnlyList<double> selenium, string axis)
    {
        var plot = new Plot();
        var bars = new List<Bar>();
        for (var i = 0; i < labels.Count; i++)
        {
            bars.Add(new Bar { Position = i * 3 + 1, Value = playwright[i], FillColor = theme.Playwright, LineWidth = 0 });
            bars.Add(new Bar { Position = i * 3, Value = selenium[i], FillColor = theme.Selenium, LineWidth = 0 });
        }

        plot.Add.Bars(bars).Horizontal = true;
        plot.Axes.Left.TickGenerator = new ScottPlot.TickGenerators.NumericManual(
            labels.Select((_, i) => i * 3 + 0.5).ToArray(), labels.ToArray());
        plot.Axes.Bottom.Label.Text = axis;
        plot.Axes.Margins(left: 0);
        AddLegend(plot, theme);
        return plot;
    }

    private static Plot Lines(Theme theme, Summary s, Func<ScalingPoint, double> value, string axis)
    {
        var plot = new Plot();
        foreach (var tool in Tools.All)
        {
            var points = s.Scaling.Where(p => p.Tool == tool).OrderBy(p => p.Workers).ToList();
            if (points.Count == 0)
            {
                continue;
            }

            var line = plot.Add.Scatter(points.Select(p => (double)p.Workers).ToArray(), points.Select(value).ToArray());
            line.Color = tool == Tools.Playwright ? theme.Playwright : theme.Selenium;
            line.LineWidth = 2;
            line.MarkerSize = 8;
            line.LegendText = tool;
        }

        plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(
            s.Workers.Select(w => (double)w).ToArray(), s.Workers.Select(w => $"{w} worker{(w == 1 ? "" : "s")}").ToArray());
        plot.Axes.Left.Label.Text = axis;
        return plot;
    }

    private static void AddLegend(Plot plot, Theme theme)
    {
        plot.Legend.ManualItems.Add(new LegendItem { LabelText = Tools.Playwright, FillColor = theme.Playwright });
        plot.Legend.ManualItems.Add(new LegendItem { LabelText = Tools.Selenium, FillColor = theme.Selenium });
    }

    private static string Save(Plot plot, string directory, string name, Theme theme, int width, int height)
    {
        plot.FigureBackground.Color = Colors.Transparent;
        plot.DataBackground.Color = Colors.Transparent;
        plot.Axes.Color(theme.Foreground);
        plot.Grid.MajorLineColor = theme.Grid;
        plot.Legend.BackgroundColor = Colors.Transparent;
        plot.Legend.FontColor = theme.Foreground;
        plot.Legend.OutlineColor = theme.Grid;
        plot.ShowLegend(Alignment.LowerRight);

        var path = Path.Combine(directory, $"{name}.{theme.Name}.svg");
        plot.SaveSvg(path, width, height);
        return path;
    }
}
