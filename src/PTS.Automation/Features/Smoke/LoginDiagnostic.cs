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
        await login.GotoAsync();
        Logger.Information("On login page, URL: {Url}", Page.Url);

        // Subscribe to the LoginCheck response BEFORE clicking submit.
        var loginCheckTask = Page.WaitForResponseAsync(
            r => r.Url.Contains("/Account/LoginCheck", StringComparison.OrdinalIgnoreCase),
            new PageWaitForResponseOptions { Timeout = 30_000 });

        // Fill and submit.
        await Page.Locator("#Username").FillAsync(creds.Username);
        await Page.Locator("#Password").FillAsync(creds.Password);
        await Page.Locator("#loginBtn").ClickAsync();

        IResponse response;
        try
        {
            response = await loginCheckTask;
        }
        catch (TimeoutException)
        {
            Logger.Error("No response from /Account/LoginCheck within 30s. Page URL: {Url}", Page.Url);
            await DumpDiagnosticsAsync("no-loginCheck-response");
            Assert.Fail("The /Account/LoginCheck request was never issued or never completed within 30s. "
                        + "Check DevTools network tab or the dumped HTML for JS errors.");
            return;
        }

        // Dump everything useful.
        var status = response.Status;
        string bodyText;
        try { bodyText = await response.TextAsync(); }
        catch (Exception ex) { bodyText = $"<could not read body: {ex.Message}>"; }

        Logger.Information("LoginCheck response status: {Status}", status);
        Logger.Information("LoginCheck response body (first 2000 chars): {Body}",
            bodyText.Length > 2000 ? bodyText[..2000] + "…(truncated)" : bodyText);

        // Give the client-side redirect a chance to fire.
        await Page.WaitForTimeoutAsync(3000);

        Logger.Information("Post-submit URL: {Url}",   Page.Url);
        Logger.Information("Post-submit title: {Title}", await Page.TitleAsync());

        await DumpDiagnosticsAsync("after-loginCheck");

        // Do NOT assert pass/fail here — this test is purely diagnostic.
        TestContext.Out.WriteLine($"LoginCheck status: {status}");
        TestContext.Out.WriteLine($"Post-submit URL:   {Page.Url}");
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
