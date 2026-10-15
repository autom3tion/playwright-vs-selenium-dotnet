namespace Versus.Playwright.Tests;

/// <summary>The "Versus" section of precept.json. Versus.Bench overrides it with PRECEPT_VERSUS__* variables.</summary>
public sealed class VersusSettings
{
    public string PlaygroundUrl { get; set; } = "http://uitestingplayground.com";
    public string ToolshopUrl { get; set; } = "https://practicesoftwaretesting.com";
    public string ChromePath { get; set; } = "";
    public string ChromeDriverPath { get; set; } = "";
    public int TimeoutSeconds { get; set; } = 30;
    public string ToolshopEmail { get; set; } = "";
    public string ToolshopPassword { get; set; } = "";
}
