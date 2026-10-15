namespace Versus.Bench.Reporting;

public static class ReadmeBlock
{
    public const string Start = "<!-- results:start -->";
    public const string End = "<!-- results:end -->";

    public static string Replace(string readme, string block)
    {
        var start = readme.IndexOf(Start, StringComparison.Ordinal);
        var end = start < 0 ? -1 : readme.IndexOf(End, start + Start.Length, StringComparison.Ordinal);
        if (start < 0 || end < 0)
        {
            throw new BenchException($"README.md has no {Start} … {End} block to write the results into.");
        }

        return readme[..(start + Start.Length)] + "\n" + block.Trim() + "\n" + readme[end..];
    }
}
