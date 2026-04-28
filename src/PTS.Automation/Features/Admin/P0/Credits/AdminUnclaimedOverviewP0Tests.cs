using PTS.Automation.Infrastructure;
using PTS.Automation.Pages.Admin.Credits;
using PTS.Automation.Pages.Admin.Shell;

namespace PTS.Automation.Features.Admin.P0.Credits;

/// <seealso cref="Pages.Admin.Credits.AdminUnclaimedOverviewPage"/>
[TestFixture]
[Category(Categories.EpicCreditsOverviews)]
public sealed class AdminUnclaimedOverviewP0Tests : AdminP0TestBase
{
    [Test]
    [Category(Categories.UI)]
    [Category(Categories.Smoke)]
    [Description("ADMIN-P0-C1b — Unclaimed overview screen access (Admin → Credits).")]
    public async Task ADMIN_P0_C1b_Unclaimed_overview_screen_access()
    {
        await LandOnAdminEnquiriesAsync();

        var nav = new AdminNavBar(Page, Settings.Applications.Admin);
        await nav.GoToUnclaimedOverviewAsync();

        var unclaimed = new AdminUnclaimedOverviewPage(Page, Settings.Applications.Admin);
        await unclaimed.WaitForReadyAsync();

        Assert.That(Page.Url, Does.Contain("UnclaimedCredits").IgnoreCase);
        var tab = (await unclaimed.ActiveTabTextAsync()).Trim();
        Assert.That(tab, Does.Contain("Unclaimed").IgnoreCase);
    }
}
