using Versus.Timing;

namespace Versus.Bench.Running;

public static class RoundPlan
{
    /// <summary>Run first and thrown away, so JIT, DNS and CDN caches are warm for both tools.</summary>
    public const int WarmUpRound = 0;

    public static IReadOnlyList<int> Rounds(int measured) => [WarmUpRound, .. Enumerable.Range(1, measured)];

    /// <summary>Alternates which tool goes first, so drift in the network or the site hits both equally.</summary>
    public static IReadOnlyList<string> Order(int round) =>
        round % 2 == 1 ? [Tools.Playwright, Tools.Selenium] : [Tools.Selenium, Tools.Playwright];
}
