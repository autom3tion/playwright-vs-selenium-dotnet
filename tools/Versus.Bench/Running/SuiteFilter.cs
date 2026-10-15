using Versus.Bench.Options;

namespace Versus.Bench.Running;

public static class SuiteFilter
{
    /// <summary>A Precept --precept-filter expression, or null for every test.</summary>
    public static string? For(Suite suite) => suite switch
    {
        Suite.Flakiness => "class:*.Flakiness.*",
        Suite.Journeys => "class:*.Journeys.*",
        _ => null,
    };

    public static IReadOnlyList<Uri> Sites(Suite suite, Uri playground, Uri toolshop) => suite switch
    {
        Suite.Flakiness => [playground],
        Suite.Journeys => [toolshop],
        _ => [playground, toolshop],
    };
}
