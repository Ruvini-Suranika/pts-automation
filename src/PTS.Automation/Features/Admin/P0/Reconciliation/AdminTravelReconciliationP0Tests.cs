using Allure.NUnit.Attributes;
using PTS.Automation.Infrastructure;
using PTS.Automation.Pages.Admin.Reconciliation;
using PTS.Automation.Pages.Admin.Shell;

namespace PTS.Automation.Features.Admin.P0.Reconciliation;

/// <seealso cref="Pages.Admin.Reconciliation.AdminTravelReconciliationPage"/>
[TestFixture]
[AllureSuite(Categories.Admin)]
[AllureFeature("Travel reconciliation")]
[AllureTag(Categories.P0)]
[AllureTag(Categories.EpicReconciliation)]
[AllureTag(Categories.Reconciliation)]
[Category(Categories.EpicReconciliation)]
[Category(Categories.Reconciliation)]
public sealed class AdminTravelReconciliationP0Tests : AdminP0TestBase
{
    [Test]
    [Category(Categories.UI)]
    [Category(Categories.Smoke)]
    [Description("ADMIN-P0-B1 — Reconciliation page loads; basic client-side search on grid.")]
    public async Task ADMIN_P0_B1_Reconciliation_screen_access_and_basic_search()
    {
        await StepAsync("Land on Admin Enquiries", () => LandOnAdminEnquiriesAsync());

        var nav = new AdminNavBar(Page, Settings.Applications.Admin);
        await StepAsync("Open Travel Reconciliation", () => nav.GoToReconciliationAsync());

        var recon = new AdminTravelReconciliationPage(Page, Settings.Applications.Admin);
        await StepAsync("Wait for Reconciliation page", () => recon.WaitForReadyAsync());

        await StepAsync("Verify Reconciliation URL", async () =>
        {
            Assert.That(Page.Url, Does.Contain("PreviousTravelReconcilation").IgnoreCase,
                "URL must use the MVC action name (including historical typo Reconcilation).");
        });

        await StepAsync("Filter grid by keyword 'a'", () => recon.FilterGridByKeywordAsync("a"));

        await StepAsync("Verify results section is visible", async () =>
        {
            await Expect(Page.Locator("#divResults")).ToBeVisibleAsync();
        });
    }
}
