namespace Versus.Bench;

public sealed record RepoPaths(string Root)
{
    public static RepoPaths Find(string start)
    {
        for (var dir = new DirectoryInfo(start); dir is not null; dir = dir.Parent)
        {
            if (File.Exists(Path.Combine(dir.FullName, "Versus.slnx")))
            {
                return new RepoPaths(dir.FullName);
            }
        }

        throw new BenchException($"No Versus.slnx in '{start}' or above it. Run this from inside the repository.");
    }

    public string Solution => Path.Combine(Root, "Versus.slnx");
    public string PlaywrightTests => Path.Combine(Root, "tests", "Versus.Playwright.Tests");
    public string SeleniumTests => Path.Combine(Root, "tests", "Versus.Selenium.Tests");
    public string Results => Path.Combine(Root, "results");
    public string RawResults => Path.Combine(Results, "raw");
    public string ChromeCache => Path.Combine(Root, ".cache", "chrome");
    public string Readme => Path.Combine(Root, "README.md");
    public string PackagesProps => Path.Combine(Root, "Directory.Packages.props");

    public string TestAssembly(string tool) =>
        Path.Combine(Root, "tests", $"Versus.{tool}.Tests", "bin", "Release", "net10.0", $"Versus.{tool}.Tests.dll");
}
