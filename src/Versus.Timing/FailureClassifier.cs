using System.Reflection;

namespace Versus.Timing;

public static class FailureType
{
    /// <summary>Waited for a state that never came: WebDriverWait, a Playwright action, or a Playwright Expect.</summary>
    public const string Timeout = "timeout";
    public const string StaleElement = "stale-element";
    public const string ClickIntercepted = "click-intercepted";
    public const string NotInteractable = "not-interactable";
    /// <summary>A value already in hand was wrong.</summary>
    public const string Assertion = "assertion";
    /// <summary>The test left no timing record (its fixture failed, or the process died).</summary>
    public const string Harness = "harness";
    public const string Other = "other";
}

/// <summary>Sorts a test's failure by exception type name, so neither tool's assembly is referenced.</summary>
public static class FailureClassifier
{
    public static string? Classify(Exception? failure)
    {
        if (failure is null)
        {
            return null;
        }

        var e = Unwrap(failure);
        var name = e.GetType().Name;

        if (name == "ElementClickInterceptedException" || e.Message.Contains("intercepts pointer events", StringComparison.Ordinal))
        {
            return FailureType.ClickIntercepted;
        }

        return name switch
        {
            "StaleElementReferenceException" => FailureType.StaleElement,
            "ElementNotInteractableException" => FailureType.NotInteractable,
            "WebDriverTimeoutException" or "TimeoutException" => FailureType.Timeout,
            "PlaywrightException" when e.Message.Contains("expected", StringComparison.OrdinalIgnoreCase) => FailureType.Timeout,
            "PreceptAssertionException" => FailureType.Assertion,
            _ => FailureType.Other,
        };
    }

    private static Exception Unwrap(Exception e)
    {
        while (e is TargetInvocationException or AggregateException && e.InnerException is not null)
        {
            e = e.InnerException;
        }

        return e;
    }
}
