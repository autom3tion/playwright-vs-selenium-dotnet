namespace Versus.Bench.Resources;

public sealed record ProcessSample(int Pid, int ParentPid, long RssBytes, TimeSpan CpuTime);
