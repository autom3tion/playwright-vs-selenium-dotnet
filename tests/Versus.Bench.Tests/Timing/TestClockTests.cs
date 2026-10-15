using Microsoft.Extensions.Time.Testing;
using Precept;
using Versus.Timing;

namespace Versus.Bench.Tests.Timing;

[TestSuite("TestClock")]
public class TestClockTests
{
    [Test("Splits a test into setup, body and teardown")]
    public async Task SplitsPhases()
    {
        var time = new FakeTimeProvider();
        var clock = new TestClock(time);

        clock.Start();
        time.Advance(TimeSpan.FromMilliseconds(120));
        clock.SetupDone();
        time.Advance(TimeSpan.FromMilliseconds(900));
        clock.BodyDone();
        time.Advance(TimeSpan.FromMilliseconds(40));
        var (setup, body, teardown) = clock.Finish();

        await Assert.That(setup).ToBeEqualAsync(120d);
        await Assert.That(body).ToBeEqualAsync(900d);
        await Assert.That(teardown).ToBeEqualAsync(40d);
    }

    [Test("A setup that never finished counts as zero setup")]
    public async Task SetupNeverFinished()
    {
        var time = new FakeTimeProvider();
        var clock = new TestClock(time);

        clock.Start();
        time.Advance(TimeSpan.FromMilliseconds(50));
        clock.BodyDone();
        var (setup, body, _) = clock.Finish();

        await Assert.That(setup).ToBeEqualAsync(0d);
        await Assert.That(body).ToBeEqualAsync(50d);
    }
}
