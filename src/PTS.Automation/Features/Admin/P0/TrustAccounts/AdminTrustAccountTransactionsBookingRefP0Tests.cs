using Allure.NUnit.Attributes;
using PTS.Automation.Infrastructure;
using PTS.Automation.Pages.Admin.Shell;
using PTS.Automation.Pages.Admin.TrustAccounts;

namespace PTS.Automation.Features.Admin.P0.TrustAccounts;

/// <seealso cref="Pages.Admin.TrustAccounts.AdminTrustAccountTransactionsPage"/>
[TestFixture]
[AllureSuite(Categories.Admin)]
[AllureFeature("Trust account transactions")]
[AllureTag(Categories.P0)]
[AllureTag(Categories.EpicTrustAccounts)]
[Category(Categories.EpicTrustAccounts)]
public sealed class AdminTrustAccountTransactionsBookingRefP0Tests : AdminP0TestBase
{
    [Test]
    [Category(Categories.UI)]
    [Category(Categories.Smoke)]
    [Description("ADMIN-P0-A2 — Booking Reference link on transaction reporting (opens from balance link).")]
    public async Task ADMIN_P0_A2_Trust_Accounts_Booking_Reference_link_on_transactions()
    {
        await StepAsync("Land on Admin Enquiries", () => LandOnAdminEnquiriesAsync());

        var nav = new AdminNavBar(Page, Settings.Applications.Admin);
        await StepAsync("Open Trust Accounts from Bank menu", () => nav.GoToTrustAccountsAsync());

        var trust = new AdminTrustAccountsPage(Page, Settings.Applications.Admin);
        await StepAsync("Wait for Trust Accounts page", () => trust.WaitForReadyAsync());

        var blockCount = await StepAsync("Count trust account blocks", () => trust.CountAccountBlocksAsync());
        if (blockCount == 0)
            Assert.Ignore("No trust account blocks — cannot open AdminTransactions.");

        var txTab = await StepAsync("Open first account transactions in new tab",
            () => trust.OpenFirstAccountTransactionsInNewTabAsync());
        try
        {
            var tx = new AdminTrustAccountTransactionsPage(txTab, Settings.Applications.Admin);
            await StepAsync("Wait for transactions page", () => tx.WaitForReadyAsync());

            try
            {
                await StepAsync("Submit search with first period", () => tx.SubmitSearchWithFirstPeriodAsync());
            }
            catch (TimeoutException)
            {
                Assert.Ignore("Transactions search did not complete — period options or QA data unavailable.");
            }

            var linkCount = await StepAsync("Count booking reference links in grid", () => tx.CountBookingReferenceLinksAsync());
            if (linkCount == 0)
            {
                Assert.Ignore(
                    "No booking-reference links in the grid after search. Requires historical transactions for the selected trust account on QA.");
            }

            var moneyTab = await StepAsync("Click booking reference link and wait for Money popup",
                () => txTab.RunAndWaitForPopupAsync(async () =>
                    await tx.BookingReferenceLinks.First.ClickAsync()));
            try
            {
                await StepAsync("Verify Money drill-down URL", async () =>
                {
                    Assert.That(moneyTab.Url, Does.Contain("/Admin/Money/").IgnoreCase,
                        "Booking ref cell should link to Money drill-down per _TransactionAllResultPartial.cshtml.");
                });
            }
            finally
            {
                await StepAsync("Close Money tab", () => moneyTab.CloseAsync());
            }
        }
        finally
        {
            await StepAsync("Close transactions tab", () => txTab.CloseAsync());
        }
    }
}
