namespace Versus.Bench.Running;

/// <summary>Hands out rounds; a round whose site was down goes to the back of the queue.</summary>
public sealed class RoundScheduler(IEnumerable<int> rounds)
{
    public const int MaxConsecutiveSiteDown = 3;

    private readonly Queue<int> _pending = new(rounds);
    private int _consecutiveSiteDown;

    public int SiteDownReruns { get; private set; }

    public bool TryNext(out int round) => _pending.TryDequeue(out round);

    public void Completed() => _consecutiveSiteDown = 0;

    public void SiteDown(int round)
    {
        SiteDownReruns++;
        _consecutiveSiteDown++;
        if (_consecutiveSiteDown >= MaxConsecutiveSiteDown)
        {
            throw new BenchException(
                $"A site was down for {MaxConsecutiveSiteDown} rounds in a row. Stopping; the results so far are kept.");
        }

        _pending.Enqueue(round);
    }
}
