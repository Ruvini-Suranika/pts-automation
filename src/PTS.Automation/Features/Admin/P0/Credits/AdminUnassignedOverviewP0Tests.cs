using Allure.NUnit.Attributes;
using PTS.Automation.Infrastructure;
using PTS.Automation.Pages.Admin.Credits;
using PTS.Automation.Pages.Admin.Shell;

namespace PTS.Automation.Features.Admin.P0.Credits;

/// <seealso cref="Pages.Admin.Credits.AdminUnassignedOverviewPage"/>
[TestFixture]
[AllureSuite(Categories.Admin)]
[AllureFeature("Unassigned credits overview")]
[AllureTag(Categories.P0)]
[AllureTag(Categories.EpicCreditsOverviews)]
[Category(Categories.EpicCreditsOverviews)]
public sealed class AdminUnassignedOverviewP0Tests : AdminP0TestBase
{
    [Test]
    [Category(Categories.UI)]
    [Category(Categories.Smoke)]
    [Description("ADMIN-P0-C1a — Unassigned overview screen access (Admin → Credits).")]
    public async Task ADMIN_P0_C1a_Unassigned_overview_screen_access()
    {
        await StepAsync("Land on Admin Enquiries", () => LandOnAdminEnquiriesAsync());

        var nav = new AdminNavBar(Page, Settings.Applications.Admin);
        await StepAsync("Open Unassigned credits overview", () => nav.GoToUnassignedOverviewAsync());

        var unassigned = new AdminUnassignedOverviewPage(Page, Settings.Applications.Admin);
        await StepAsync("Wait for Unassigned overview page", () => unassigned.WaitForReadyAsync());

        await StepAsync("Verify Unassigned credits URL", async () =>
        {
            Assert.That(Page.Url, Does.Contain("UnassignedCredits").IgnoreCase);
        });

        await StepAsync("Verify Remove button is present", async () =>
        {
            Assert.That(await unassigned.IsRemoveButtonPresentAsync(), Is.True);
        });
    }
}
