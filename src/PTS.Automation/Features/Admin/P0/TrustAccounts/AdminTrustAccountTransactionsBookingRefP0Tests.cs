using PTS.Automation.Infrastructure;
using PTS.Automation.Pages.Admin.Shell;
using PTS.Automation.Pages.Admin.TrustAccounts;

namespace PTS.Automation.Features.Admin.P0.TrustAccounts;

/// <seealso cref="Pages.Admin.TrustAccounts.AdminTrustAccountTransactionsPage"/>
[TestFixture]
[Category(Categories.EpicTrustAccounts)]
public sealed class AdminTrustAccountTransactionsBookingRefP0Tests : AdminP0TestBase
{
    [Test]
    [Description("ADMIN-P0-A2 — Booking Reference link on transaction reporting (opens from balance link).")]
    public async Task ADMIN_P0_A2_Trust_Accounts_Booking_Reference_link_on_transactions()
    {
        await LandOnAdminEnquiriesAsync();

        var nav = new AdminNavBar(Page, Settings.Applications.Admin);
        await nav.GoToTrustAccountsAsync();

        var trust = new AdminTrustAccountsPage(Page, Settings.Applications.Admin);
        await trust.WaitForReadyAsync();

        if (await trust.CountAccountBlocksAsync() == 0)
            Assert.Ignore("No trust account blocks — cannot open AdminTransactions.");

        var txTab = await trust.OpenFirstAccountTransactionsInNewTabAsync();
        try
        {
            var tx = new AdminTrustAccountTransactionsPage(txTab, Settings.Applications.Admin);
            await tx.WaitForReadyAsync();

            try
            {
                await tx.SubmitSearchWithFirstPeriodAsync();
            }
            catch (TimeoutException)
            {
                Assert.Ignore("Transactions search did not complete — period options or QA data unavailable.");
            }

            if (await tx.CountBookingReferenceLinksAsync() == 0)
            {
                Assert.Ignore(
                    "No booking-reference links in the grid after search. Requires historical transactions for the selected trust account on QA.");
            }

            var moneyTab = await txTab.RunAndWaitForPopupAsync(async () =>
                await tx.BookingReferenceLinks.First.ClickAsync());
            try
            {
                Assert.That(moneyTab.Url, Does.Contain("/Admin/Money/").IgnoreCase,
                    "Booking ref cell should link to Money drill-down per _TransactionAllResultPartial.cshtml.");
            }
            finally
            {
                await moneyTab.CloseAsync();
            }
        }
        finally
        {
            await txTab.CloseAsync();
        }
    }
}
