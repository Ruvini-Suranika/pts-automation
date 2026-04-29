using Allure.NUnit.Attributes;
using PTS.Automation.Infrastructure;
using PTS.Automation.Pages.Member.Auth;

namespace PTS.Automation.Features.Smoke;

/// <summary>
/// Diagnostic helpers kept around as <c>[Explicit]</c> tools for when login
/// behaviour changes and we need to inspect the live network / DOM. Not part
/// of any regular run.
///
/// To invoke:
/// <code>
///     dotnet test --filter "FullyQualifiedName~LoginDiagnostic"
/// </code>
/// </summary>
[TestFixture]
[AllureSuite("Diagnostic")]
[AllureFeature("Login")]
[AllureTag("Diagnostic")]
[AllureTag(Categories.Hybrid)]
[Category("Diagnostic")]
[Explicit("Debug helper — run manually when login behaviour changes")]
public class LoginDiagnostic : BaseTest
{
    [Test]
    [Category(Categories.Hybrid)]
    public async Task Diagnose_member_login_response()
    {
        var creds = Settings.Users.Member;
        Assume.That(creds.IsConfigured, "Member creds required for this diagnostic.");

        var login = new LoginPage(Page, Settings.Applications.Member);
        await StepAsync("Open Member login page", () => login.GotoAsync());

        await StepAsync("Log current login page URL", async () =>
        {
            Logger.Information("On login page, URL: {Url}", Page.Url);
        });

        // Subscribe before submit so the response is captured when the login button is clicked.
        var loginCheckTask = Page.WaitForResponseAsync(
            r => r.Url.Contains("/Account/LoginCheck", StringComparison.OrdinalIgnoreCase),
            new PageWaitForResponseOptions { Timeout = 30_000 });

        await StepAsync("Fill username and password", async () =>
        {
            await Page.Locator("#Username").FillAsync(creds.Username);
            await Page.Locator("#Password").FillAsync(creds.Password);
        });

        await StepAsync("Click login button", () => Page.Locator("#loginBtn").ClickAsync());

        IResponse response;
        try
        {
            response = await StepAsync("Wait for LoginCheck response", () => loginCheckTask);
        }
        catch (TimeoutException)
        {
            Logger.Error("No response from /Account/LoginCheck within 30s. Page URL: {Url}", Page.Url);
            await DumpDiagnosticsAsync("no-loginCheck-response");
            Assert.Fail("The /Account/LoginCheck request was never issued or never completed within 30s. "
                        + "Check DevTools network tab or the dumped HTML for JS errors.");
            return;
        }

        await StepAsync("Log LoginCheck response status and body", async () =>
        {
            var status = response.Status;
            string bodyText;
            try { bodyText = await response.TextAsync(); }
            catch (Exception ex) { bodyText = $"<could not read body: {ex.Message}>"; }

            Logger.Information("LoginCheck response status: {Status}", status);
            Logger.Information("LoginCheck response body (first 2000 chars): {Body}",
                bodyText.Length > 2000 ? bodyText[..2000] + "…(truncated)" : bodyText);
        });

        await StepAsync("Brief wait for client-side redirect", () => Page.WaitForTimeoutAsync(3000));

        await StepAsync("Log post-submit URL and title", async () =>
        {
            Logger.Information("Post-submit URL: {Url}", Page.Url);
            Logger.Information("Post-submit title: {Title}", await Page.TitleAsync());
        });

        await StepAsync("Write diagnostic screenshot and HTML", () => DumpDiagnosticsAsync("after-loginCheck"));

        await StepAsync("Emit summary to test output", async () =>
        {
            TestContext.Out.WriteLine($"LoginCheck status: {response.Status}");
            TestContext.Out.WriteLine($"Post-submit URL:   {Page.Url}");
        });
    }

    private async Task DumpDiagnosticsAsync(string tag)
    {
        var shotDir = Path.Combine(Settings.Paths.ScreenshotDir);
        Directory.CreateDirectory(shotDir);

        var safeName = TestContext.CurrentContext.Test.Name + "-" + tag;
        var shotPath = Path.Combine(shotDir, $"{safeName}.png");
        var htmlPath = Path.Combine(shotDir, $"{safeName}.html");

        try { await Page.ScreenshotAsync(new() { Path = shotPath, FullPage = true }); } catch { /* best-effort */ }

        try
        {
            var html = await Page.ContentAsync();
            await File.WriteAllTextAsync(htmlPath, html);
        }
        catch { /* best-effort */ }

        Logger.Information("Diagnostic artifacts written to {Shot} and {Html}", shotPath, htmlPath);
    }
}
