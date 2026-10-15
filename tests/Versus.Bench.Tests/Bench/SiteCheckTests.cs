using System.Net;
using Precept;
using Versus.Bench.Running;

namespace Versus.Bench.Tests.Bench;

[TestSuite("SiteCheck")]
public class SiteCheckTests
{
    private sealed class StubHandler(Func<int, HttpStatusCode> status) : HttpMessageHandler
    {
        private int _calls;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(status(++_calls)));
    }

    private static readonly Uri[] Sites = [new("http://uitestingplayground.com")];

    [Test("Up at once: no waiting")]
    public async Task Up()
    {
        var delays = new List<TimeSpan>();
        var check = new SiteCheck(new HttpClient(new StubHandler(_ => HttpStatusCode.OK)), d => { delays.Add(d); return Task.CompletedTask; });

        await Assert.That(await check.IsUpAsync(Sites)).ToBeTrueAsync();
        await Assert.That(delays).ToBeEmptyAsync();
    }

    [Test("Down once, then up: one 30-second wait")]
    public async Task Recovers()
    {
        var delays = new List<TimeSpan>();
        var check = new SiteCheck(
            new HttpClient(new StubHandler(n => n == 1 ? HttpStatusCode.ServiceUnavailable : HttpStatusCode.OK)),
            d => { delays.Add(d); return Task.CompletedTask; });

        await Assert.That(await check.IsUpAsync(Sites)).ToBeTrueAsync();
        await Assert.That(string.Join(",", delays)).ToBeEqualAsync("00:00:30");
    }

    [Test("Still down after three retries: reported down")]
    public async Task Down()
    {
        var delays = new List<TimeSpan>();
        var check = new SiteCheck(
            new HttpClient(new StubHandler(_ => HttpStatusCode.BadGateway)),
            d => { delays.Add(d); return Task.CompletedTask; });

        await Assert.That(await check.IsUpAsync(Sites)).ToBeFalseAsync();
        await Assert.That(delays).ToHaveCountAsync(3);
    }
}
