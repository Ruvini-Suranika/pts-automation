using PTS.Automation.Infrastructure;
using PTS.Automation.Pages.Member.Accounts;

namespace PTS.Automation.Features.Member.Accounts;

/// <summary>P1 critical coverage: Member → Accounts → Payments Due.</summary>
[TestFixture]
[Category(Categories.Regression)]
[Category(Categories.Member)]
[Category("Accounts.PaymentDue")]
public sealed class PaymentDueP1Tests : MemberTest
{
    [Test]
    [Category("P1")]
    [Category("UI")]
    [Description("DEV-TC-177 — User is able to select the tick boxes (Payment Due).")]
    public async Task DEV_TC_177_User_can_select_tick_boxes()
    {
        var page = new PaymentDuePage(Page, Settings.Applications.Member);

        await StepAsync("Open Payment Due", () => page.GotoAsync());
        await StepAsync("Wait for initial grid load", () => page.WaitForGridLoadedAsync());

        var rowBoxes = await StepAsync("Count assignable row checkboxes",
            () => page.AssignableRowCheckboxCountAsync());
        if (rowBoxes == 0)
            Assert.Ignore("No payment-due rows for this test member — no tickboxes to select.");

        await StepAsync("Click header select-all", () => page.ClickCheckAllAsync());
        await Page.WaitForTimeoutAsync(200);

        await StepAsync("Assert all row checkboxes are selected", async () =>
        {
            var checkedAfterSelect = await page.CheckedRowCountAsync();
            Assert.That(checkedAfterSelect, Is.EqualTo(rowBoxes),
                "After select-all, every visible row checkbox should be ticked.");
        });

        await StepAsync("Click header select-all again to clear", () => page.ClickCheckAllAsync());
        await Page.WaitForTimeoutAsync(200);

        await StepAsync("Assert all row checkboxes are cleared", async () =>
        {
            Assert.That(await page.CheckedRowCountAsync(), Is.EqualTo(0),
                "After second select-all click, all row checkboxes should be cleared.");
        });
    }

    [Test]
    [Category("P1")]
    [Category("UI")]
    [Description("DEV-TC-179 — Accounts Payments Due: user is able to send reminder (compose modal opens).")]
    public async Task DEV_TC_179_User_can_send_reminder_with_row_selected()
    {
        var page = new PaymentDuePage(Page, Settings.Applications.Member);

        await StepAsync("Open Payment Due", () => page.GotoAsync());
        await StepAsync("Wait for grid load", () => page.WaitForGridLoadedAsync());

        if (await page.AssignableRowCheckboxCountAsync() == 0)
            Assert.Ignore("No payment-due rows — cannot exercise Send Reminder with a selection.");

        await StepAsync("Select first payment-due row checkbox", () => page.SelectFirstAssignableRowCheckboxAsync());

        await StepAsync("Click Send Reminder", () => page.ClickSendReminderAsync());

        await StepAsync("Assert email compose modal is shown", async () =>
        {
            await Page.WaitForTimeoutAsync(500);
            Assert.That(await page.IsEmailComposeModalVisibleAsync(), Is.True,
                "Email compose modal (#addemailtemplate) should open when at least one row is selected.");
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
    [Description("DEV-TC-312 — Validate the Branch Store column in Payments Due screen.")]
    public async Task DEV_TC_312_Branch_store_column_in_payments_due()
    {
        var page = new PaymentDuePage(Page, Settings.Applications.Member);

        await StepAsync("Open Payment Due", () => page.GotoAsync());

        var headers = await StepAsync("Read column headers", () => page.GetColumnHeadersAsync());
        Logger.Information("Headers: {Headers}", string.Join(" | ", headers));

        await StepAsync("Assert BRANCH STORE column exists", async () =>
        {
            Assert.That(await page.ColumnIndexOfAsync("BRANCH STORE"), Is.GreaterThan(0),
                $"Expected a BRANCH STORE column. Got: {string.Join(" | ", headers)}");
        });
    }

    [Test]
    [Category("P1")]
    [Category("UI")]
    [Description("DEV-TC-899 — Refine Search function on Accounts Payments Due page.")]
    public async Task DEV_TC_899_Refine_search_on_payment_due()
    {
        var page = new PaymentDuePage(Page, Settings.Applications.Member);

        await StepAsync("Open Payment Due", () => page.GotoAsync());
        await StepAsync("Wait for grid load", () => page.WaitForGridLoadedAsync());

        var rowsBefore = await StepAsync("Count grid rows", () => page.Grid.RowCountAsync());
        if (rowsBefore == 0)
            Assert.Ignore("No payment-due rows — refine-search has nothing to filter.");

        const string sentinel = "ZZZ_PTS_AUTOMATION_NO_MATCH_ZZZ";
        await StepAsync($"Apply refine search '{sentinel}'", () => page.SetGridSearchAsync(sentinel));
        await Page.WaitForTimeoutAsync(300);

        await StepAsync("Assert all rows hidden after refine", async () =>
        {
            var visibleAfter = await Page.Locator("#PaymentDue tr:visible").CountAsync();
            Assert.That(visibleAfter, Is.EqualTo(0),
                $"After a non-matching refine filter, no data rows should remain visible. Rows before: {rowsBefore}.");
        });
    }

    [Test]
    [Category("P1")]
    [Category("Hybrid")]
    [Description("DEV-TC-902 — Accounts Payments Due: refine search by Payment type (POST body).")]
    [TestCase("Deposit",      "7")]
    [TestCase("Balance",      "8")]
    [TestCase("Part-Payment", "9")]
    public async Task DEV_TC_902_Payment_type_round_trip(string label, string expectedTypeId)
    {
        var page = new PaymentDuePage(Page, Settings.Applications.Member);

        await StepAsync("Open Payment Due", () => page.GotoAsync());
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
    [Description("DEV-TC-903 — Accounts Payments Due: booking link in row.")]
    public async Task DEV_TC_903_Booking_link_in_payment_due_row()
    {
        var page = new PaymentDuePage(Page, Settings.Applications.Member);

        await StepAsync("Open Payment Due", () => page.GotoAsync());
        await StepAsync("Wait for grid load", () => page.WaitForGridLoadedAsync());

        if (await page.Grid.RowCountAsync() == 0)
            Assert.Ignore("No payment-due rows — no booking link to verify.");

        var href = await StepAsync("Read first row booking href",
            () => page.FirstRowBookingLink().GetAttributeAsync("href"));

        await StepAsync("Assert booking link targets Client BookingDetails", async () =>
        {
            Assert.That(href, Is.Not.Null.And.Not.Empty);
            Assert.That(href, Does.Contain("/Client/BookingDetails").IgnoreCase);
            Assert.That(href, Does.Contain("Id=").IgnoreCase);
            Assert.That(href, Does.Contain("BookingRefId=").IgnoreCase);
        });
    }
}
