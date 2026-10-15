using Precept;
using Versus.Playwright.Tests.Fixtures;

namespace Versus.Playwright.Tests.Flakiness;

[TestSuite("overlapped: the input is partly under another element and clears itself if typed into there")]
public class OverlappedTests : PlaywrightTest
{
    [Test]
    public async Task NameFieldAcceptsTextOnceScrolledIntoView()
    {
        await Page.GotoAsync($"{Settings.PlaygroundUrl}/overlapped");
        var name = Page.Locator("#name");

        // #name is only partly clipped by its 100px scroll container, so scrolling it into view is a no-op
        // and the grey overlay (top: 67px, 50px tall) stays over its centre. Typing there makes the page's
        // input handler clear the field. Scrolling the container to reveal #subject below it moves #name clear.
        await Page.Locator("#subject").ScrollIntoViewIfNeededAsync();
        await name.FillAsync("Ada Lovelace");

        await Expect(name).ToHaveValueAsync("Ada Lovelace");
    }
}
