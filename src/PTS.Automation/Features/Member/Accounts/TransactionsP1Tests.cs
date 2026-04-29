using Allure.NUnit.Attributes;
using PTS.Automation.Infrastructure;
using PTS.Automation.Pages.Member.Accounts;

namespace PTS.Automation.Features.Member.Accounts;

/// <summary>P1 critical coverage: Member → Accounts → Transactions (<c>/Client/Transactions</c>).</summary>
[TestFixture]
[AllureSuite(Categories.Member)]
[AllureFeature("Transactions")]
[AllureTag(Categories.P1)]
[AllureTag(Categories.Regression)]
[AllureTag(Categories.Member)]
[AllureTag("Accounts.Transactions")]
[Category(Categories.Regression)]
[Category(Categories.Member)]
[Category("Accounts.Transactions")]
public sealed class TransactionsP1Tests : MemberTest
{
    [Test]
    [Category("P1")]
    [Category("UI")]
    [Description("DEV-TC-121 — Validate Transactions record (filters, grid chrome, key columns).")]
    public async Task DEV_TC_121_Validate_transactions_record()
    {
        var page = new TransactionsPage(Page, Settings.Applications.Member);

        await StepAsync("Open Transactions", () => page.GotoAsync());

        await StepAsync("Assert filter panel and controls", async () =>
        {
            Assert.That(await page.IsFilterPanelVisibleAsync(), Is.True);
            Assert.That(await page.IsSearchButtonVisibleAsync(), Is.True);
            Assert.That(await page.IsGridSearchVisibleAsync(), Is.True);
            Assert.That(await page.IsThreeDotsToggleVisibleAsync(), Is.True);
        });

        var headers = await StepAsync("Read column headers", () => page.GetColumnHeadersAsync());
        Logger.Information("Headers: {Headers}", string.Join(" | ", headers));

        await StepAsync("Assert expected column headers", async () =>
        {
            foreach (var expected in new[] { "Booking Ref", "Customer Ref", "Type",
                     "Currency", "Gross Credit", "Debit Amount" })
            {
                Assert.That(await page.ColumnIndexOfAsync(expected), Is.GreaterThan(0),
                    $"Expected column '{expected}'. Got: {string.Join(" | ", headers)}");
            }
        });
    }

    [Test]
    [Category("P1")]
    [Category("UI")]
    [Description("DEV-TC-496 — Validate three dots options on Transactions (copy / print / PDF / Excel).")]
    public async Task DEV_TC_496_Validate_three_dots_options()
    {
        var page = new TransactionsPage(Page, Settings.Applications.Member);

        await StepAsync("Open Transactions", () => page.GotoAsync());
        await StepAsync("Open three-dots menu", () => page.OpenThreeDotsMenuAsync());
        await Page.WaitForTimeoutAsync(200);

        await StepAsync("Assert menu actions are visible", async () =>
        {
            Assert.That(await page.IsCopyToClipboardVisibleAsync(), Is.True);
            Assert.That(await page.IsPrintVisibleAsync(), Is.True);
            Assert.That(await page.IsDownloadPdfVisibleAsync(), Is.True);
            Assert.That(await page.IsDownloadExcelVisibleAsync(), Is.True);
        });

        await StepAsync("Run search to populate export hrefs", async () =>
        {
            await page.SelectBroadPeriodAsync();
            await page.SearchAndCaptureAsync();
        });

        var pdfHref   = await StepAsync("Read PDF export href", () => page.GetDownloadPdfHrefAsync());
        var excelHref = await StepAsync("Read Excel export href", () => page.GetDownloadExcelHrefAsync());

        await StepAsync("Assert export hrefs target expected endpoints", async () =>
        {
            Assert.That(pdfHref, Does.Contain("/Common/ExportTransactionToPdf").IgnoreCase);
            Assert.That(excelHref, Does.Contain("/Common/ExportTransactionToExcel").IgnoreCase);
        });
    }

    [Test]
    [Category("P1")]
    [Category("UI")]
    [Description("DEV-TC-893 — Refine functionality on Accounts Transactions page.")]
    public async Task DEV_TC_893_Refine_search_on_transactions()
    {
        var page = new TransactionsPage(Page, Settings.Applications.Member);

        await StepAsync("Open Transactions", () => page.GotoAsync());
        await StepAsync("Populate grid via broad search", async () =>
        {
            await page.SelectBroadPeriodAsync();
            await page.SearchAndCaptureAsync();
        });

        var rowsBefore = await page.Grid.RowCountAsync();
        if (rowsBefore == 0)
            Assert.Ignore("No transactions for this member/period — refine has nothing to filter.");

        const string sentinel = "ZZZ_PTS_AUTOMATION_NO_MATCH_ZZZ";
        await StepAsync($"Apply grid refine '{sentinel}'", () => page.SetGridSearchAsync(sentinel));
        await Page.WaitForTimeoutAsync(300);

        await StepAsync("Assert no visible data rows after refine", async () =>
        {
            var visibleAfter = await Page.Locator("#transactionsresult tr:visible").CountAsync();
            Assert.That(visibleAfter, Is.EqualTo(0),
                $"All rows should hide after non-matching refine. Rows before: {rowsBefore}.");
        });
    }

    [Test]
    [Category("P1")]
    [Category("UI")]
    [Description("DEV-TC-896 — Booking Reference link on Accounts Transactions.")]
    public async Task DEV_TC_896_Booking_reference_link_on_transactions()
    {
        var page = new TransactionsPage(Page, Settings.Applications.Member);

        await StepAsync("Open Transactions", () => page.GotoAsync());
        await StepAsync("Populate grid", async () =>
        {
            await page.SelectBroadPeriodAsync();
            await page.SearchAndCaptureAsync();
        });

        if (await page.Grid.RowCountAsync() == 0)
            Assert.Ignore("No transactions — no booking-ref link to verify.");

        var link = await StepAsync("Resolve first booking-ref link", () => page.FirstRowBookingLinkAsync());
        var href    = await link.GetAttributeAsync("href");
        var onclick = await link.GetAttributeAsync("onclick");
        var combined = $"{href ?? ""} {onclick ?? ""}";

        await StepAsync("Assert booking navigation target", async () =>
        {
            Assert.That(combined.Length, Is.GreaterThan(0));
            Assert.That(combined,
                Does.Contain("BookingDetails").IgnoreCase
                   .Or.Contain("ClientBookingDetails").IgnoreCase
                   .Or.Contain("BookingOverview").IgnoreCase);
        });
    }

    [Test]
    [Category("P1")]
    [Category("UI")]
    [Description("DEV-TC-897 — Customer Reference link on Accounts Transactions.")]
    public async Task DEV_TC_897_Customer_reference_link_on_transactions()
    {
        var page = new TransactionsPage(Page, Settings.Applications.Member);

        await StepAsync("Open Transactions", () => page.GotoAsync());
        await StepAsync("Populate grid", async () =>
        {
            await page.SelectBroadPeriodAsync();
            await page.SearchAndCaptureAsync();
        });

        if (await page.Grid.RowCountAsync() == 0)
            Assert.Ignore("No transactions — no customer-ref link to verify.");

        var link = await StepAsync("Resolve first customer-ref link", () => page.FirstRowCustomerLinkAsync());
        var href    = await link.GetAttributeAsync("href");
        var onclick = await link.GetAttributeAsync("onclick");
        var combined = $"{href ?? ""} {onclick ?? ""}";

        await StepAsync("Assert client navigation target", async () =>
        {
            Assert.That(combined.Length, Is.GreaterThan(0));
            Assert.That(combined,
                Does.Contain("ClientDetails").IgnoreCase
                   .Or.Contain("ClientOverview").IgnoreCase
                   .Or.Contain("ClientBookingDetails").IgnoreCase);
        });
    }
}
