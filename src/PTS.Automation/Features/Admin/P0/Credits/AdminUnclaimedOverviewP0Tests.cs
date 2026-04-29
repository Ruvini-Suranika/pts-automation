using Allure.NUnit.Attributes;
using PTS.Automation.Infrastructure;
using PTS.Automation.Pages.Admin.Credits;
using PTS.Automation.Pages.Admin.Shell;

namespace PTS.Automation.Features.Admin.P0.Credits;

/// <seealso cref="Pages.Admin.Credits.AdminUnclaimedOverviewPage"/>
[TestFixture]
[AllureSuite(Categories.Admin)]
[AllureFeature("Unclaimed credits overview")]
[AllureTag(Categories.P0)]
[AllureTag(Categories.EpicCreditsOverviews)]
[Category(Categories.EpicCreditsOverviews)]
public sealed class AdminUnclaimedOverviewP0Tests : AdminP0TestBase
{
    [Test]
    [Category(Categories.UI)]
    [Category(Categories.Smoke)]
    [Description("ADMIN-P0-C1b — Unclaimed overview screen access (Admin → Credits).")]
    public async Task ADMIN_P0_C1b_Unclaimed_overview_screen_access()
    {
        await StepAsync("Land on Admin Enquiries", () => LandOnAdminEnquiriesAsync());

        var nav = new AdminNavBar(Page, Settings.Applications.Admin);
        await StepAsync("Open Unclaimed credits overview", () => nav.GoToUnclaimedOverviewAsync());

        var unclaimed = new AdminUnclaimedOverviewPage(Page, Settings.Applications.Admin);
        await StepAsync("Wait for Unclaimed overview page", () => unclaimed.WaitForReadyAsync());

        await StepAsync("Verify Unclaimed credits URL", async () =>
        {
            Assert.That(Page.Url, Does.Contain("UnclaimedCredits").IgnoreCase);
        });

        await StepAsync("Verify active tab shows Unclaimed", async () =>
        {
            var tab = (await unclaimed.ActiveTabTextAsync()).Trim();
            Assert.That(tab, Does.Contain("Unclaimed").IgnoreCase);
        });
    }
}
