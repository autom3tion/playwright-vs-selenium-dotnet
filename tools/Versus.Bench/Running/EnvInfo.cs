namespace Versus.Bench.Running;

public sealed record EnvInfo(
    string Machine,
    string Os,
    string Cpu,
    int Cores,
    long RamBytes,
    string DotnetRuntime,
    string DotnetSdk,
    string ChromeVersion,
    IReadOnlyDictionary<string, string> Packages,
    DateTimeOffset DateUtc,
    IReadOnlyDictionary<string, double> LatencyMs);
