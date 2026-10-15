using System.Reflection;
using Precept;
using Versus.Timing;

namespace Versus.Bench.Tests.Timing;

public class ElementClickInterceptedException(string m) : Exception(m);
public class StaleElementReferenceException(string m) : Exception(m);
public class ElementNotInteractableException(string m) : Exception(m);
public class WebDriverTimeoutException(string m) : Exception(m);
public class PlaywrightException(string m) : Exception(m);

[TestSuite("FailureClassifier")]
public class FailureClassifierTests
{
    [Test("No failure, no type")]
    public async Task NoFailure() => await Assert.That(FailureClassifier.Classify(null)).ToBeNullAsync();

    [Test("Selenium's click-intercepted exception")]
    public async Task SeleniumIntercepted() =>
        await Assert.That(FailureClassifier.Classify(new ElementClickInterceptedException("x")))
            .ToBeEqualAsync(FailureType.ClickIntercepted);

    [Test("Playwright's timeout naming an element that intercepts pointer events")]
    public async Task PlaywrightIntercepted() =>
        await Assert.That(FailureClassifier.Classify(new TimeoutException("<div> intercepts pointer events")))
            .ToBeEqualAsync(FailureType.ClickIntercepted);

    [Test("Stale element")]
    public async Task Stale() =>
        await Assert.That(FailureClassifier.Classify(new StaleElementReferenceException("x")))
            .ToBeEqualAsync(FailureType.StaleElement);

    [Test("Not interactable")]
    public async Task NotInteractable() =>
        await Assert.That(FailureClassifier.Classify(new ElementNotInteractableException("x")))
            .ToBeEqualAsync(FailureType.NotInteractable);

    [Test("Waiting gave up: Selenium WebDriverWait, Playwright timeout, Playwright Expect")]
    public async Task Timeouts()
    {
        await Assert.That(FailureClassifier.Classify(new WebDriverTimeoutException("Timed out after 30 seconds")))
            .ToBeEqualAsync(FailureType.Timeout);
        await Assert.That(FailureClassifier.Classify(new TimeoutException("Timeout 30000ms exceeded.")))
            .ToBeEqualAsync(FailureType.Timeout);
        await Assert.That(FailureClassifier.Classify(new PlaywrightException("Locator expected to have text 'x'")))
            .ToBeEqualAsync(FailureType.Timeout);
    }

    [Test("A wrong value in hand is an assertion failure")]
    public async Task Assertion() =>
        await Assert.That(FailureClassifier.Classify(new PreceptAssertionException("Expected 1 but found 2")))
            .ToBeEqualAsync(FailureType.Assertion);

    [Test("Unwraps reflection and aggregate wrappers")]
    public async Task Unwraps() =>
        await Assert.That(FailureClassifier.Classify(
                new TargetInvocationException(new AggregateException(new StaleElementReferenceException("x")))))
            .ToBeEqualAsync(FailureType.StaleElement);

    [Test("Anything else")]
    public async Task Other() =>
        await Assert.That(FailureClassifier.Classify(new InvalidOperationException("x")))
            .ToBeEqualAsync(FailureType.Other);
}
