using Precept;
using Versus.Bench;
using Versus.Bench.Running;

namespace Versus.Bench.Tests.Bench;

[TestSuite("RoundScheduler")]
public class RoundSchedulerTests
{
    [Test("A round whose site was down is tried again after the others")]
    public async Task RetriesLater()
    {
        var scheduler = new RoundScheduler([1, 2, 3]);
        var order = new List<int>();

        while (scheduler.TryNext(out var round))
        {
            order.Add(round);
            if (round == 1 && order.Count == 1)
            {
                scheduler.SiteDown(round);
            }
            else
            {
                scheduler.Completed();
            }
        }

        await Assert.That(string.Join(",", order)).ToBeEqualAsync("1,2,3,1");
        await Assert.That(scheduler.SiteDownReruns).ToBeEqualAsync(1);
    }

    [Test("Three outages in a row stop the benchmark")]
    public async Task StopsAfterThreeInARow()
    {
        var scheduler = new RoundScheduler([1, 2, 3]);
        scheduler.TryNext(out _);
        scheduler.SiteDown(1);
        scheduler.TryNext(out _);
        scheduler.SiteDown(2);
        scheduler.TryNext(out _);

        await Assert.That(() => { scheduler.SiteDown(3); return Task.CompletedTask; }).ToThrowAsync<BenchException>();
    }

    [Test("A completed round resets the outage count")]
    public async Task CompletedResets()
    {
        var scheduler = new RoundScheduler([1, 2, 3, 4]);
        scheduler.TryNext(out _);
        scheduler.SiteDown(1);
        scheduler.TryNext(out _);
        scheduler.SiteDown(2);
        scheduler.TryNext(out _);
        scheduler.Completed();
        scheduler.TryNext(out _);
        scheduler.SiteDown(4);
        scheduler.TryNext(out _);

        await Assert.That(() => { scheduler.SiteDown(1); return Task.CompletedTask; }).ToCompleteAsync();
    }
}
