using Allure.NUnit.Attributes;
using PTS.Automation.Infrastructure;
using PTS.Automation.Pages.Admin.Debits;
using PTS.Automation.Pages.Admin.Shell;

namespace PTS.Automation.Features.Admin.P0.Debits;

/// <seealso cref="Pages.Admin.Debits.AdminDebitsGroupingPage"/>
[TestFixture]
[AllureSuite(Categories.Admin)]
[AllureFeature("Debits grouping")]
[AllureTag(Categories.P0)]
[AllureTag(Categories.EpicDebits)]
[AllureTag(Categories.Debits)]
[Category(Categories.EpicDebits)]
[Category(Categories.Debits)]
public sealed class AdminDebitsGroupingP0Tests : AdminP0TestBase
{
    [Test]
    [Category(Categories.UI)]
    [Category(Categories.Smoke)]
    [Description("ADMIN-P0-D3 — Debits Grouping: table + Group + Pay/Currency affordances when rows exist.")]
    public async Task ADMIN_P0_D3_Debits_Grouping_screen_validation()
    {
        await StepAsync("Land on Admin Enquiries", () => LandOnAdminEnquiriesAsync());

        var nav = new AdminNavBar(Page, Settings.Applications.Admin);
        await StepAsync("Open Debits Grouping", () => nav.GoToDebitsGroupingAsync());

        var pageObj = new AdminDebitsGroupingPage(Page, Settings.Applications.Admin);
        await StepAsync("Wait for Debits Grouping page", () => pageObj.WaitForReadyAsync());

        await StepAsync("Verify Debits Grouping URL", async () =>
        {
            Assert.That(Page.Url, Does.Contain("DebitsGrouping").IgnoreCase);
        });

        await StepAsync("Verify grouping table is visible", async () =>
        {
            Assert.That(await pageObj.IsGroupingTableVisibleAsync(), Is.True);
        });

        var dataRowCount = await StepAsync("Count grouping table data rows",
            () => Page.Locator("#debitgrouping tbody tr").CountAsync());
        if (dataRowCount > 0)
        {
            await StepAsync("Verify Pay and/or Currency affordances when rows exist", async () =>
            {
                var payCount = await pageObj.PayLinks.CountAsync();
                var curCount = await pageObj.CurrencyLinks.CountAsync();
                Assert.That(payCount + curCount, Is.GreaterThan(0),
                    "With at least one grouping row, expect Pay and/or Currency affordances in the grid.");
            });
        }
    }
}
