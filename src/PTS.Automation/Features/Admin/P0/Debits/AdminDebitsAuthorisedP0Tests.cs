using PTS.Automation.Infrastructure;
using PTS.Automation.Pages.Admin.Debits;
using PTS.Automation.Pages.Admin.Shell;

namespace PTS.Automation.Features.Admin.P0.Debits;

/// <seealso cref="Pages.Admin.Debits.AdminDebitsAuthorisedPage"/>
[TestFixture]
[Category(Categories.EpicDebits)]
[Category(Categories.Debits)]
public sealed class AdminDebitsAuthorisedP0Tests : AdminP0TestBase
{
    [Test]
    [Description("ADMIN-P0-D2 — Debits Authorised screen (nav + readiness).")]
    public async Task ADMIN_P0_D2_Debits_Authorised_screen_validation()
    {
        await LandOnAdminEnquiriesAsync();

        var nav = new AdminNavBar(Page, Settings.Applications.Admin);
        await nav.GoToDebitsAuthorisedAsync();

        var pageObj = new AdminDebitsAuthorisedPage(Page, Settings.Applications.Admin);
        await pageObj.WaitForReadyAsync();

        Assert.That(Page.Url, Does.Contain("GetDebitsAuthorised").IgnoreCase);
    }
}
