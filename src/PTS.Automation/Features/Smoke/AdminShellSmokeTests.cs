using PTS.Automation.Infrastructure;
using PTS.Automation.Pages.Admin.Home;

namespace PTS.Automation.Features.Smoke;

/// <summary>
/// Authenticated Admin shell: uses primed <see cref="AdminTest"/> storage state,
/// opens the Sales / Enquiries home, validates layout chrome, and exercises logout.
/// </summary>
[TestFixture]
[Category(Categories.Smoke)]
[Category(Categories.Admin)]
[Category(Categories.Authentication)]
public class AdminShellSmokeTests : AdminTest
{
    [Test]
    [Category(Categories.UI)]
    [Description("Primed PtsAdmin session loads Admin Index with header and profile chrome.")]
    public async Task Authenticated_admin_sees_enquiries_shell()
    {
        var enquiries = new AdminEnquiriesPage(Page, Settings.Applications.Admin);

        await StepAsync("Open Admin Enquiries (Sales) home", async () =>
        {
            await enquiries.GotoAsync();
            await enquiries.WaitForReadyAsync();
        });

        await StepAsync("Admin header and profile are visible", async () =>
        {
            Assert.That(await enquiries.IsAdminHeaderVisibleAsync(), Is.True,
                "Admin top header should be visible on an authenticated admin page.");
            Assert.That(await enquiries.ProfileMenu.IsToggleVisibleAsync(), Is.True,
                "Profile dropdown toggle should be visible.");
            var name = await enquiries.ProfileMenu.GetDisplayNameAsync();
            Assert.That(name, Is.Not.Null.And.Not.Empty,
                "Admin profile should show a display name after login.");
        });

        Assert.That(await Page.TitleAsync(), Does.Contain("Enquiries").Or.Contain("Admin"),
            $"Unexpected browser title after loading admin home: '{await Page.TitleAsync()}'");
    }

    [Test]
    [Category(Categories.UI)]
    [Description("Logout ends the admin session; revisiting a protected admin URL requires login.")]
    public async Task Logout_ends_admin_session()
    {
        var enquiries = new AdminEnquiriesPage(Page, Settings.Applications.Admin);
        await enquiries.GotoAsync();
        await enquiries.WaitForReadyAsync();

        await StepAsync("Log out via profile menu", () => enquiries.ProfileMenu.LogoutAsync());

        Assert.That(Page.Url, Does.Not.Contain("/Admin/").IgnoreCase,
            $"After logout we must have left /Admin/. Actual URL: {Page.Url}");

        await StepAsync("Re-visiting /Admin/Index should redirect to login", async () =>
        {
            await Page.GotoAsync(AbsoluteUrl(Settings.Applications.Admin, "Admin/Index").ToString());

            var landed = Page.Url;
            var needsLogin =
                landed.Contains("/Account/Login", StringComparison.OrdinalIgnoreCase) ||
                landed.Contains("/Account/LogIn", StringComparison.OrdinalIgnoreCase) ||
                landed.Contains("AccessDenied", StringComparison.OrdinalIgnoreCase);

            Assert.That(needsLogin, Is.True,
                $"Protected admin page should redirect an unauthenticated user. Actual URL: {landed}");
        });
    }
}
