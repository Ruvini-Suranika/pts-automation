using PTS.Automation.Infrastructure;
using PTS.Automation.Pages.Admin.Reconciliation;
using PTS.Automation.Pages.Admin.Shell;

namespace PTS.Automation.Features.Admin.P0.Reconciliation;

/// <seealso cref="Pages.Admin.Reconciliation.AdminTravelReconciliationPage"/>
[TestFixture]
[Category(Categories.EpicReconciliation)]
[Category(Categories.Reconciliation)]
public sealed class AdminTravelReconciliationP0Tests : AdminP0TestBase
{
    [Test]
    [Description("ADMIN-P0-B1 — Reconciliation page loads; basic client-side search on grid.")]
    public async Task ADMIN_P0_B1_Reconciliation_screen_access_and_basic_search()
    {
        await LandOnAdminEnquiriesAsync();

        var nav = new AdminNavBar(Page, Settings.Applications.Admin);
        await nav.GoToReconciliationAsync();

        var recon = new AdminTravelReconciliationPage(Page, Settings.Applications.Admin);
        await recon.WaitForReadyAsync();

        Assert.That(Page.Url, Does.Contain("PreviousTravelReconcilation").IgnoreCase,
            "URL must use the MVC action name (including historical typo Reconcilation).");

        await recon.FilterGridByKeywordAsync("a");

        await Expect(Page.Locator("#divResults")).ToBeVisibleAsync();
    }
}
