using PTS.Automation.Infrastructure;
using PTS.Automation.Pages.Admin.Credits;
using PTS.Automation.Pages.Admin.Shell;

namespace PTS.Automation.Features.Admin.P0.Credits;

/// <seealso cref="Pages.Admin.Credits.AdminUnassignedOverviewPage"/>
[TestFixture]
[Category(Categories.EpicCreditsOverviews)]
public sealed class AdminUnassignedOverviewP0Tests : AdminP0TestBase
{
    [Test]
    [Category(Categories.UI)]
    [Category(Categories.Smoke)]
    [Description("ADMIN-P0-C1a — Unassigned overview screen access (Admin → Credits).")]
    public async Task ADMIN_P0_C1a_Unassigned_overview_screen_access()
    {
        await LandOnAdminEnquiriesAsync();

        var nav = new AdminNavBar(Page, Settings.Applications.Admin);
        await nav.GoToUnassignedOverviewAsync();

        var unassigned = new AdminUnassignedOverviewPage(Page, Settings.Applications.Admin);
        await unassigned.WaitForReadyAsync();

        Assert.That(Page.Url, Does.Contain("UnassignedCredits").IgnoreCase);
        Assert.That(await unassigned.IsRemoveButtonPresentAsync(), Is.True);
    }
}
