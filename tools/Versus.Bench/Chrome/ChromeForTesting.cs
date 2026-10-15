using System.Runtime.InteropServices;

namespace Versus.Bench.Chrome;

/// <summary>The one Chrome build both tools drive. Bump Version deliberately; it is printed in every report.</summary>
public static class ChromeForTesting
{
    public const string Version = "155.0.8059.79";

    public static string CurrentPlatform()
    {
        var os = OperatingSystem.IsMacOS() ? OSPlatform.OSX
            : OperatingSystem.IsLinux() ? OSPlatform.Linux
            : OperatingSystem.IsWindows() ? OSPlatform.Windows
            : throw new BenchException("Chrome for Testing is published for macOS, Linux and Windows only.");
        return Platform(os, RuntimeInformation.OSArchitecture);
    }

    public static string Platform(OSPlatform os, Architecture arch)
    {
        if (os == OSPlatform.OSX)
        {
            return arch == Architecture.Arm64 ? "mac-arm64" : "mac-x64";
        }

        if (os == OSPlatform.Linux && arch == Architecture.X64)
        {
            return "linux64";
        }

        if (os == OSPlatform.Windows)
        {
            return arch == Architecture.X86 ? "win32" : "win64";
        }

        throw new BenchException($"Chrome for Testing has no build for {os} on {arch}.");
    }

    /// <param name="product">"chrome" or "chromedriver".</param>
    public static Uri DownloadUrl(string product, string platform) =>
        new($"https://storage.googleapis.com/chrome-for-testing-public/{Version}/{platform}/{product}-{platform}.zip");

    public static string ChromeRelativePath(string platform) => platform switch
    {
        "mac-arm64" or "mac-x64" => Path.Combine($"chrome-{platform}", "Google Chrome for Testing.app", "Contents", "MacOS", "Google Chrome for Testing"),
        "linux64" => Path.Combine("chrome-linux64", "chrome"),
        _ => Path.Combine($"chrome-{platform}", "chrome.exe"),
    };

    public static string DriverRelativePath(string platform) =>
        Path.Combine($"chromedriver-{platform}", platform.StartsWith("win", StringComparison.Ordinal) ? "chromedriver.exe" : "chromedriver");
}
