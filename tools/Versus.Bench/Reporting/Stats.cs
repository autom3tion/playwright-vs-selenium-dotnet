namespace Versus.Bench.Reporting;

public static class Stats
{
    /// <summary>Two results within 5% of each other are called a tie.</summary>
    public const double TieTolerance = 0.05;

    public static double Median(IEnumerable<double> values) => Percentile(values, 0.5);

    /// <summary>Linear interpolation between the closest ranks.</summary>
    public static double Percentile(IEnumerable<double> values, double p)
    {
        var sorted = values.Order().ToArray();
        if (sorted.Length == 0)
        {
            throw new ArgumentException("A percentile of no values is undefined.", nameof(values));
        }

        var rank = p * (sorted.Length - 1);
        var lower = (int)Math.Floor(rank);
        var upper = (int)Math.Ceiling(rank);
        return sorted[lower] + (rank - lower) * (sorted[upper] - sorted[lower]);
    }

    public static bool IsTie(double a, double b, double tolerance = TieTolerance) =>
        Math.Abs(a - b) <= tolerance * Math.Max(Math.Abs(a), Math.Abs(b));
}
