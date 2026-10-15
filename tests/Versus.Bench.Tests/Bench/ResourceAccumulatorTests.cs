using Precept;
using Versus.Bench.Resources;

namespace Versus.Bench.Tests.Bench;

[TestSuite("ResourceAccumulator")]
public class ResourceAccumulatorTests
{
    [Test("Peak memory is the largest total; CPU is each process's last known time, summed")]
    public async Task Accumulates()
    {
        var usage = new ResourceAccumulator();
        usage.Add([new(1, 0, 100, TimeSpan.FromSeconds(1)), new(2, 1, 50, TimeSpan.FromSeconds(2))]);
        usage.Add([new(1, 0, 120, TimeSpan.FromSeconds(3)), new(2, 1, 70, TimeSpan.FromSeconds(4))]);
        usage.Add([new(1, 0, 90, TimeSpan.FromSeconds(5))]); // process 2 exited

        await Assert.That(usage.PeakRssBytes).ToBeEqualAsync(190L);
        await Assert.That(usage.CpuSeconds).ToBeEqualAsync(9d);
    }
}
