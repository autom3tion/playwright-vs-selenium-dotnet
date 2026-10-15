namespace Versus.Bench;

/// <summary>A problem to tell the person running the benchmark about. Program prints the message and exits with 1.</summary>
public sealed class BenchException(string message) : Exception(message);
