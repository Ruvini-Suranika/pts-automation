using PTS.Automation.Infrastructure;
using PTS.Automation.Pages.Admin.Debits;
using PTS.Automation.Pages.Admin.Shell;

namespace PTS.Automation.Features.Admin.P0.Debits;

/// <seealso cref="Pages.Admin.Debits.AdminDebitsUnauthorisedPage"/>
[TestFixture]
[Category(Categories.EpicDebits)]
[Category(Categories.Debits)]
public sealed class AdminDebitsUnauthorisedP0Tests : AdminP0TestBase
{
    [Test]
    [Category(Categories.UI)]
    [Category(Categories.Smoke)]
    [Description("ADMIN-P0-D1 — Debits Unauthorised screen (nav + grid chrome).")]
    public async Task ADMIN_P0_D1_Debits_Unauthorised_screen_validation()
    {
        await LandOnAdminEnquiriesAsync();

        var nav = new AdminNavBar(Page, Settings.Applications.Admin);
        await nav.GoToDebitsUnauthorisedAsync();

        var pageObj = new AdminDebitsUnauthorisedPage(Page, Settings.Applications.Admin);
        await pageObj.WaitForReadyAsync();

        Assert.That(Page.Url, Does.Contain("DebitsUnAuthorised").IgnoreCase);
        Assert.That(await pageObj.IsGridVisibleAsync(), Is.True);
    }
}
