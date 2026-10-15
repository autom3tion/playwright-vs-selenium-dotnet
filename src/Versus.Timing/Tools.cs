namespace Versus.Timing;

public static class Tools
{
    public const string Playwright = "Playwright";
    public const string Selenium = "Selenium";

    public static readonly IReadOnlyList<string> All = [Playwright, Selenium];
}
