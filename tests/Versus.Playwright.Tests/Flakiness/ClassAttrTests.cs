using Precept;
using Versus.Playwright.Tests.Fixtures;

namespace Versus.Playwright.Tests.Flakiness;

[TestSuite("classattr: find the primary button by one of several classes")]
public class ClassAttrTests : PlaywrightTest
{
    [Test]
    public async Task PrimaryButtonRaisesAlert()
    {
        await Page.GotoAsync($"{Settings.PlaygroundUrl}/classattr");

        // Playwright dismisses dialogs unless a handler is registered first.
        var message = new TaskCompletionSource<string>();
        Page.Dialog += async (_, dialog) =>
        {
            message.TrySetResult(dialog.Message);
            await dialog.AcceptAsync();
        };

        await Page.Locator("button.btn-primary").ClickAsync();

        var text = await message.Task.WaitAsync(TimeSpan.FromSeconds(Settings.TimeoutSeconds));
        await Assert.That(text).ToBeEqualAsync("Primary button pressed");
    }
}
