using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Versus.Bench.Chrome;
using Versus.Bench.Reporting;

namespace Versus.Bench.Running;

public static partial class EnvCollector
{
    public static async Task<EnvInfo> CollectAsync(RepoPaths repo, HttpClient http, IReadOnlyList<Uri> sites)
    {
        var sdk = await Shell.RunAsync("dotnet", ["--version"]);
        var latency = new Dictionary<string, double>();
        foreach (var site in sites)
        {
            latency[site.ToString()] = await MedianLatencyAsync(http, site);
        }

        return new EnvInfo(
            MachineSlug(Environment.MachineName),
            RuntimeInformation.OSDescription,
            await CpuModelAsync(),
            Environment.ProcessorCount,
            GC.GetGCMemoryInfo().TotalAvailableMemoryBytes,
            RuntimeInformation.FrameworkDescription,
            sdk.StdOut.Trim(),
            ChromeForTesting.Version,
            ReadPackages(File.ReadAllText(repo.PackagesProps)),
            DateTimeOffset.UtcNow,
            latency);
    }

    public static IReadOnlyDictionary<string, string> ReadPackages(string propsXml) =>
        XDocument.Parse(propsXml).Descendants("PackageVersion")
            .ToDictionary(p => (string)p.Attribute("Include")!, p => (string)p.Attribute("Version")!);

    public static string MachineSlug(string machineName) =>
        NonAlphanumeric().Replace(machineName.ToLowerInvariant(), "-").Trim('-');

    private static async Task<double> MedianLatencyAsync(HttpClient http, Uri site)
    {
        var samples = new List<double>();
        for (var i = 0; i < 5; i++)
        {
            var watch = Stopwatch.StartNew();
            try
            {
                using var timeout = new CancellationTokenSource(SiteCheck.RequestTimeout);
                using var response = await http.GetAsync(site, timeout.Token);
                if (response.IsSuccessStatusCode)
                {
                    samples.Add(watch.Elapsed.TotalMilliseconds);
                }
            }
            catch (Exception e) when (e is HttpRequestException or TaskCanceledException)
            {
                // Failed requests are not latency samples.
            }
        }

        return samples.Count == 0 ? -1 : Stats.Median(samples);
    }

    private static async Task<string> CpuModelAsync()
    {
        if (OperatingSystem.IsMacOS())
        {
            return (await Shell.RunAsync("sysctl", ["-n", "machdep.cpu.brand_string"])).StdOut.Trim();
        }

        if (OperatingSystem.IsLinux())
        {
            var line = File.ReadLines("/proc/cpuinfo").FirstOrDefault(l => l.StartsWith("model name", StringComparison.Ordinal));
            return line?.Split(':', 2)[1].Trim() ?? "unknown";
        }

        return Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER") ?? "unknown";
    }

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex NonAlphanumeric();
}
