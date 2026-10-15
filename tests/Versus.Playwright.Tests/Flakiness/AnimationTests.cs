using System.Text.RegularExpressions;
using Precept;
using Versus.Playwright.Tests.Fixtures;

namespace Versus.Playwright.Tests.Flakiness;

[TestSuite("animation: click the target only after it stops moving")]
public class AnimationTests : PlaywrightTest
{
    [Test]
    public async Task MovingTargetIsClickedAfterAnimation()
    {
        await Page.GotoAsync($"{Settings.PlaygroundUrl}/animation");
        await Page.Locator("#animationButton").ClickAsync();

        // While it moves, the target's class includes "spin".
        await Expect(Page.Locator("#movingTarget")).Not.ToHaveClassAsync(new Regex("spin"));
        await Page.Locator("#movingTarget").ClickAsync();
        await Expect(Page.Locator("#opstatus"))
            .ToHaveTextAsync("Moving Target clicked. It's class name is 'btn btn-primary'");
    }
}
