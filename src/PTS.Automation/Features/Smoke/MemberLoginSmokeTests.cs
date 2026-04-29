using Allure.NUnit.Attributes;
using PTS.Automation.Infrastructure;
using PTS.Automation.Pages.Member.Auth;

namespace PTS.Automation.Features.Smoke;

/// <summary>
/// End-to-end smoke proving the whole framework plumbing works:
///   config → Playwright → fixture → POM → assertions → artifacts.
///
/// The first two tests require ZERO credentials — they validate the login
/// page itself renders correctly on the target environment. The third test
/// actually logs in and requires Member credentials to be configured via
/// user-secrets or the PTS_Users__Member__* environment variables.
/// </summary>
[TestFixture]
[AllureSuite(Categories.Smoke)]
[AllureFeature("Member login")]
[AllureTag(Categories.Smoke)]
[AllureTag(Categories.Member)]
[AllureTag(Categories.Authentication)]
[Category(Categories.Smoke)]
[Category(Categories.Member)]
[Category(Categories.Authentication)]
public class MemberLoginSmokeTests : BaseTest
{
    [Test]
    [Category(Categories.UI)]
    [Description("Member login page loads at the configured base URL and the login form is interactive.")]
    public async Task Login_page_loads_with_expected_form()
    {
        var login = new LoginPage(Page, Settings.Applications.Member);
        await StepAsync("Open Member login page", () => login.GotoAsync());

        await StepAsync("Verify login URL and form controls", async () =>
        {
            await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(@"/Account/Login", System.Text.RegularExpressions.RegexOptions.IgnoreCase));
            await Expect(Page.Locator("#Username")).ToBeVisibleAsync();
            await Expect(Page.Locator("#Password")).ToBeVisibleAsync();
            await Expect(Page.Locator("#loginBtn")).ToBeEnabledAsync();
        });

        await StepAsync("Verify welcome or sign-in heading", async () =>
        {
            var heading = await login.GetHeadingTextAsync();
            Assert.That(heading, Does.Match("Welcome|Sign in"),
                $"Unexpected login heading '{heading}'");
        });
    }

    [Test]
    [Category(Categories.UI)]
    [Description("Forgot-password link is present (indicates layout integrity even when creds unavailable).")]
    public async Task Login_page_shows_forgot_password_link()
    {
        var login = new LoginPage(Page, Settings.Applications.Member);
        await StepAsync("Open Member login page", () => login.GotoAsync());

        await StepAsync("Verify forgot password link is visible", async () =>
        {
            Assert.That(await login.IsForgotPasswordLinkVisibleAsync(), Is.True,
                "Forgot password link must be visible on the login page.");
        });
    }

    [Test]
    [Category(Categories.UI)]
    [Description("Valid Member credentials log in successfully and land outside the login endpoint.")]
    public async Task Member_logs_in_with_valid_credentials()
    {
        var creds = Settings.Users.Member;
        if (!creds.IsConfigured)
        {
            Assert.Ignore(
                "Member credentials are not configured. Configure via:\n" +
                "  dotnet user-secrets set \"Users:Member:Username\" \"<user>\"\n" +
                "  dotnet user-secrets set \"Users:Member:Password\" \"<pwd>\"\n" +
                "or environment variables PTS_Users__Member__Username / PTS_Users__Member__Password.");
        }

        var login = new LoginPage(Page, Settings.Applications.Member);
        await StepAsync("Open Member login page", () => login.GotoAsync());

        Logger.Information("Attempting Member login as '{User}' against {Base}",
            creds.Username, Settings.Applications.Member.BaseUrl);

        await StepAsync("Submit Member credentials", async () =>
        {
            await login.LoginAsync(creds.Username, creds.Password);
            await login.WaitForPostLoginAsync();
        });

        await StepAsync("Log post-login URL and title", async () =>
        {
            Logger.Information("Post-login URL: {Url}", Page.Url);
            Logger.Information("Post-login title: {Title}", await Page.TitleAsync());
            TestContext.Out.WriteLine($"[LOGIN OK] Landed at: {Page.Url}");
        });

        await StepAsync("Verify navigation away from login page", async () =>
        {
            Assert.That(Page.Url, Does.Not.Contain("/Account/Login").IgnoreCase,
                "After a successful login we should no longer be on /Account/Login.");
        });

        await StepAsync("Verify no login error message after submit", async () =>
        {
            var err = await login.GetErrorMessageAsync();
            Assert.That(err, Is.Null.Or.Empty,
                $"Login page is showing an error message after submit: '{err}'");
        });
    }
}
