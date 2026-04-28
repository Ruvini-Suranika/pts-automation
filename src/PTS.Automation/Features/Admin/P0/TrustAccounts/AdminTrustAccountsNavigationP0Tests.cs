using PTS.Automation.Infrastructure;
using PTS.Automation.Pages.Admin.Shell;
using PTS.Automation.Pages.Admin.TrustAccounts;

namespace PTS.Automation.Features.Admin.P0.TrustAccounts;

/// <seealso cref="Pages.Admin.TrustAccounts.AdminTrustAccountsPage"/>
[TestFixture]
[Category(Categories.EpicTrustAccounts)]
public sealed class AdminTrustAccountsNavigationP0Tests : AdminP0TestBase
{
    [Test]
    [Category(Categories.UI)]
    [Category(Categories.Smoke)]
    [Description("ADMIN-P0-A1 — Trust Accounts: reach page via Bank menu; readiness without 404.")]
    public async Task ADMIN_P0_A1_Trust_Accounts_page_navigation_via_Bank_menu()
    {
        await LandOnAdminEnquiriesAsync();

        var nav = new AdminNavBar(Page, Settings.Applications.Admin);
        await nav.GoToTrustAccountsAsync();

        var trust = new AdminTrustAccountsPage(Page, Settings.Applications.Admin);
        await trust.WaitForReadyAsync();

        Assert.That(Page.Url, Does.Contain("TrustAccounts").IgnoreCase,
            "Trust Accounts MVC action should appear in the URL after menu navigation.");
        Assert.That(await trust.CountAccountBlocksAsync(), Is.GreaterThan(0),
            "Expected at least one trust account block from API-backed model on QA.");
    }
}
