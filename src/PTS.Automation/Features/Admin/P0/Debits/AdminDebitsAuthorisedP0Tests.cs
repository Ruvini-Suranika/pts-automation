using Allure.NUnit.Attributes;
using PTS.Automation.Infrastructure;
using PTS.Automation.Pages.Admin.Debits;
using PTS.Automation.Pages.Admin.Shell;

namespace PTS.Automation.Features.Admin.P0.Debits;

/// <seealso cref="Pages.Admin.Debits.AdminDebitsAuthorisedPage"/>
[TestFixture]
[AllureSuite(Categories.Admin)]
[AllureFeature("Debits authorised")]
[AllureTag(Categories.P0)]
[AllureTag(Categories.EpicDebits)]
[AllureTag(Categories.Debits)]
[Category(Categories.EpicDebits)]
[Category(Categories.Debits)]
public sealed class AdminDebitsAuthorisedP0Tests : AdminP0TestBase
{
    [Test]
    [Category(Categories.UI)]
    [Category(Categories.Smoke)]
    [Description("ADMIN-P0-D2 — Debits Authorised screen (nav + readiness).")]
    public async Task ADMIN_P0_D2_Debits_Authorised_screen_validation()
    {
        await StepAsync("Land on Admin Enquiries", () => LandOnAdminEnquiriesAsync());

        var nav = new AdminNavBar(Page, Settings.Applications.Admin);
        await StepAsync("Open Debits Authorised", () => nav.GoToDebitsAuthorisedAsync());

        var pageObj = new AdminDebitsAuthorisedPage(Page, Settings.Applications.Admin);
        await StepAsync("Wait for Debits Authorised page", () => pageObj.WaitForReadyAsync());

        await StepAsync("Verify Debits Authorised URL", async () =>
        {
            Assert.That(Page.Url, Does.Contain("GetDebitsAuthorised").IgnoreCase);
        });
    }
}
