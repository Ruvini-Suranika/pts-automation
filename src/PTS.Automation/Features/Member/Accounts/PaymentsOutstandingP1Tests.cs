using Allure.NUnit.Attributes;
using PTS.Automation.Infrastructure;
using PTS.Automation.Pages.Member.Accounts;
using PTS.Automation.Pages.Member.Dashboard;

namespace PTS.Automation.Features.Member.Accounts;

/// <summary>P1 critical coverage: Member → Accounts → Payments Outstanding.</summary>
[TestFixture]
[AllureSuite(Categories.Member)]
[AllureFeature("Payments outstanding")]
[AllureTag(Categories.P1)]
[AllureTag(Categories.Regression)]
[AllureTag(Categories.Member)]
[AllureTag("Accounts.PaymentsOutstanding")]
[Category(Categories.Regression)]
[Category(Categories.Member)]
[Category("Accounts.PaymentsOutstanding")]
public sealed class PaymentsOutstandingP1Tests : MemberTest
{
    private static async Task NavigateToPaymentsOutstandingAsync(
        DashboardPage dashboard, PaymentsOutstandingPage page)
    {
        await dashboard.GotoAsync();
        await dashboard.NavBar.GoToPaymentsOutstandingAsync();
        await page.WaitForReadyAsync();
    }

    [Test]
    [Category("P1")]
    [Category("UI")]
    [Description("DEV-TC-311 — Branch Store column in Payments Outstanding screen.")]
    public async Task DEV_TC_311_Branch_store_column_in_payments_outstanding()
    {
        var dashboard = new DashboardPage(Page, Settings.Applications.Member);
        var page      = new PaymentsOutstandingPage(Page, Settings.Applications.Member);

        await StepAsync("Navigate to Payments Outstanding", () => NavigateToPaymentsOutstandingAsync(dashboard, page));

        var headers = await StepAsync("Read column headers", () => page.GetColumnHeadersAsync());
        Logger.Information("Headers: {Headers}", string.Join(" | ", headers));

        await StepAsync("Assert BRANCH STORE column", async () =>
        {
            Assert.That(headers, Does.Contain("BRANCH STORE")
                                    .Or.Contain("Branch Store")
                                    .Or.Contain("BRANCH STORE".ToLowerInvariant()));
            Assert.That(await page.ColumnIndexOfAsync("BRANCH STORE"), Is.GreaterThan(0));
        });
    }

    [Test]
    [Category("P1")]
    [Category("UI")]
    [Description("DEV-TC-385 — Send reminder function for Payments Outstanding (compose modal opens).")]
    public async Task DEV_TC_385_Send_reminder_opens_compose_modal()
    {
        var dashboard = new DashboardPage(Page, Settings.Applications.Member);
        var page      = new PaymentsOutstandingPage(Page, Settings.Applications.Member);

        await StepAsync("Navigate to Payments Outstanding", () => NavigateToPaymentsOutstandingAsync(dashboard, page));
        await StepAsync("Wait for grid load", () => page.WaitForGridLoadedAsync());

        if (await page.AssignableRowCheckboxCountAsync() == 0)
            Assert.Ignore("No outstanding payment rows — cannot exercise Send Reminder with a selection.");

        await StepAsync("Select first outstanding row checkbox", () => page.SelectFirstAssignableRowCheckboxAsync());
        await StepAsync("Click Send Reminder", () => page.ClickSendReminderAsync());

        await StepAsync("Assert email compose modal is shown", async () =>
        {
            await Page.WaitForTimeoutAsync(500);
            Assert.That(await page.IsEmailComposeModalVisibleAsync(), Is.True,
                "Email compose modal should open when at least one row is selected.");
        });

        await StepAsync("Dismiss email modal", async () =>
        {
            if (await page.IsEmailComposeModalVisibleAsync())
                await Page.Keyboard.PressAsync("Escape");
        });
    }

    [Test]
    [Category("P1")]
    [Category("UI")]
    [Description("DEV-TC-900 — Refine Search on Accounts Payments Outstanding page.")]
    public async Task DEV_TC_900_Refine_search_on_payments_outstanding()
    {
        var dashboard = new DashboardPage(Page, Settings.Applications.Member);
        var page      = new PaymentsOutstandingPage(Page, Settings.Applications.Member);

        await StepAsync("Navigate to Payments Outstanding", () => NavigateToPaymentsOutstandingAsync(dashboard, page));
        await StepAsync("Wait for grid load", () => page.WaitForGridLoadedAsync());

        var rowsBefore = await StepAsync("Count grid rows", () => page.Grid.RowCountAsync());
        if (rowsBefore == 0)
            Assert.Ignore("No outstanding payments — refine-search has nothing to filter.");

        const string sentinel = "ZZZ_PTS_AUTOMATION_NO_MATCH_ZZZ";
        await StepAsync($"Apply refine search '{sentinel}'", () => page.SetGridSearchAsync(sentinel));
        await Page.WaitForTimeoutAsync(300);

        await StepAsync("Assert all rows hidden", async () =>
        {
            var visibleAfter = await Page.Locator("#PaymentOutstanding tr:visible").CountAsync();
            Assert.That(visibleAfter, Is.EqualTo(0),
                $"After refine, no rows should remain visible. Rows before: {rowsBefore}.");
        });
    }

    [Test]
    [Category("P1")]
    [Category("Hybrid")]
    [Description("DEV-TC-901 — Payments Outstanding: Payment type search (POST body).")]
    [TestCase("Deposit",      "7")]
    [TestCase("Balance",      "8")]
    [TestCase("Part-Payment", "9")]
    public async Task DEV_TC_901_Payment_type_search(string label, string expectedTypeId)
    {
        var dashboard = new DashboardPage(Page, Settings.Applications.Member);
        var page      = new PaymentsOutstandingPage(Page, Settings.Applications.Member);

        await StepAsync("Navigate to Payments Outstanding", () => NavigateToPaymentsOutstandingAsync(dashboard, page));
        await StepAsync("Wait for grid load", () => page.WaitForGridLoadedAsync());

        await StepAsync($"Select Payment Type '{label}'", () => page.SelectPaymentTypeAsync(label));

        var (status, postData) = await StepAsync("Click Search and capture POST",
            () => page.SearchAndCaptureAsync());

        await StepAsync("Assert POST carries PaymentTypeId", async () =>
        {
            Assert.That(status, Is.EqualTo(200), $"Search endpoint should return 200. Got {status}.");
            Assert.That(postData, Does.Contain($"PaymentTypeId={expectedTypeId}").IgnoreCase
                                  .Or.Contain($"\"PaymentTypeId\":\"{expectedTypeId}\"").IgnoreCase,
                $"POST body should carry PaymentTypeId={expectedTypeId}.\nActual: {postData}");
        });
    }

    [Test]
    [Category("P1")]
    [Category("UI")]
    [Description("DEV-TC-904 — Accounts Payments Outstanding: booking link in row.")]
    public async Task DEV_TC_904_Booking_link_in_payments_outstanding_row()
    {
        var dashboard = new DashboardPage(Page, Settings.Applications.Member);
        var page      = new PaymentsOutstandingPage(Page, Settings.Applications.Member);

        await StepAsync("Navigate to Payments Outstanding", () => NavigateToPaymentsOutstandingAsync(dashboard, page));
        await StepAsync("Wait for grid load", () => page.WaitForGridLoadedAsync());

        if (await page.Grid.RowCountAsync() == 0)
            Assert.Ignore("No outstanding payments — no booking link to verify.");

        var link = page.FirstRowBookingLink();
        var href   = await StepAsync("Read first booking href", () => link.GetAttributeAsync("href"));
        var target = await StepAsync("Read link target", () => link.GetAttributeAsync("target"));

        await StepAsync("Assert booking link contract", async () =>
        {
            Assert.That(href, Is.Not.Null.And.Not.Empty);
            Assert.That(href, Does.Contain("/Client/BookingDetails").IgnoreCase);
            Assert.That(href, Does.Contain("Id=").IgnoreCase);
            Assert.That(href, Does.Contain("BookingRefId=").IgnoreCase);
            Assert.That(target, Is.EqualTo("_blank"));
        });
    }
}
