namespace Versus.Bench.Running;

/// <summary>Before each round: every site answers 2xx within 10 s, else wait 30 s and try again, up to 3 more times.</summary>
public sealed class SiteCheck(HttpClient http, Func<TimeSpan, Task> delay)
{
    public const int Attempts = 4;
    public static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(10);
    public static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(30);

    public async Task<bool> IsUpAsync(IReadOnlyList<Uri> sites)
    {
        for (var attempt = 1; attempt <= Attempts; attempt++)
        {
            if (await AllAnswerAsync(sites))
            {
                return true;
            }

            if (attempt < Attempts)
            {
                await delay(RetryDelay);
            }
        }

        return false;
    }

    private async Task<bool> AllAnswerAsync(IReadOnlyList<Uri> sites)
    {
        foreach (var site in sites)
        {
            try
            {
                using var timeout = new CancellationTokenSource(RequestTimeout);
                using var response = await http.GetAsync(site, timeout.Token);
                if (!response.IsSuccessStatusCode)
                {
                    return false;
                }
            }
            catch (Exception e) when (e is HttpRequestException or TaskCanceledException)
            {
                return false;
            }
        }

        return true;
    }
}
