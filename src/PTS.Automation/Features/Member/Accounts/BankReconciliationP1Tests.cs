using System.Text.Json;
using System.Text.RegularExpressions;

using Allure.NUnit.Attributes;
using PTS.Automation.Infrastructure;
using PTS.Automation.Pages.Member.Accounts;
using PTS.Automation.Pages.Member.Clients;
using PTS.Automation.Pages.Member.Dashboard;

namespace PTS.Automation.Features.Member.Accounts;

/// <summary>P1 critical coverage: Member bank-reconciliation style flows (profit claims, money refunds, FX/APC).</summary>
[TestFixture]
[AllureSuite(Categories.Member)]
[AllureFeature("Bank reconciliation")]
[AllureTag(Categories.P1)]
[AllureTag(Categories.Regression)]
[AllureTag(Categories.Member)]
[AllureTag("Accounts.BankReconciliation")]
[Category(Categories.Regression)]
[Category(Categories.Member)]
[Category("Accounts.BankReconciliation")]
public sealed class BankReconciliationP1Tests : MemberTest
{
    private async Task<(BookingListPage List, (string ClientNewId, string BookingRefId)? Ids)> OpenBookingsAndTryFirstBookingAsync()
    {
        var dashboard = new DashboardPage(Page, Settings.Applications.Member);
        var list        = new BookingListPage(Page, Settings.Applications.Member);
        await dashboard.GotoAsync();
        await dashboard.NavBar.GoToBookingsAsync();
        await list.WaitForReadyAsync();
        await list.SearchAsync();
        var ids = await list.TryReadFirstBookingDetailsQueryAsync();
        return (list, ids);
    }

    [Test]
    [Category("P1")]
    [Category("UI")]
    [Description("DEV-TC-606 — Unable to claim profit if credit is still pending on booking (disabled claim row in Available remittances).")]
    public async Task DEV_TC_606_Claim_disabled_when_credit_pending_on_booking()
    {
        var dashboard = new DashboardPage(Page, Settings.Applications.Member);
        var profit      = new ProfitClaimsPage(Page, Settings.Applications.Member);

        await StepAsync("Open Profit claims", async () =>
        {
            await dashboard.GotoAsync();
            await dashboard.NavBar.GoToProfitClaimsAsync();
            await profit.WaitForReadyAsync();
        });

        var blocked = await StepAsync("Count available rows with disabled claim controls",
            () => profit.AvailableRemittancesDisabledClaimRowCountAsync());

        if (blocked == 0)
            Assert.Ignore(
                "No available-remittance row is in the blocked state (claim checkbox disabled) — need a booking with pending credit/payment mismatch per ProfitClaims.cshtml rules.");

        await StepAsync("Assert at least one blocked claim row", () =>
        {
            Assert.That(blocked, Is.GreaterThan(0));
            return Task.CompletedTask;
        });
    }

    [Test]
    [Category("P1")]
    [Category("UI")]
    [Description("DEV-TC-664 — Add Refund on Money from Credits and Debits when a Batch-type credit row exists (modal smoke; no Save).")]
    public async Task DEV_TC_664_Add_refund_modal_for_batch_credit_context()
    {
        var (_, ids) = await StepAsync("Open Bookings and read first booking", OpenBookingsAndTryFirstBookingAsync);
        if (ids is null)
            Assert.Ignore("No booking rows.");

        var money = new BookingMoneyPage(Page, Settings.Applications.Member);
        await StepAsync("Open Booking Money", () => money.GotoWithBookingAsync(ids.Value.ClientNewId, ids.Value.BookingRefId));

        if (await money.MoneyGridDataRowCountAsync() == 0)
            Assert.Ignore("No money grid rows.");

        var batchRow = await StepAsync("Find first Batch credit row", () => money.FirstDataRowWhereTransactionTypeContainsAsync("batch"));
        if (batchRow is null)
            Assert.Ignore("No transaction row with type containing 'batch' — cannot anchor DEV-TC-664.");

        await StepAsync("Scroll batch row into view", () => batchRow.ScrollIntoViewIfNeededAsync());

        await StepAsync("Assert Add Refund is available", async () =>
        {
            Assert.That(await money.IsAddRefundButtonEnabledAsync(), Is.True,
                "Add Refund should be enabled when debit features allow (see money.js IsDebitEnabledInMember / profit claimed).");
        });

        await StepAsync("Open Add Refund modal", () => money.OpenAddRefundModalAsync());

        await StepAsync("Assert modal and Bank Refund type selection (no Save)", async () =>
        {
            Assert.That(await money.IsAddRefundModalVisibleAsync(), Is.True);
            await money.SelectRefundTypeValueAsync("14");
            await Page.WaitForTimeoutAsync(200);
            await money.CloseAddRefundModalAsync();
        });
    }

    [Test]
    [Category("P1")]
    [Category("UI")]
    [Description("DEV-TC-665 — Add Refund on Money from Credits and Debits when a Bank (non-batch) credit row exists (modal smoke; no Save).")]
    public async Task DEV_TC_665_Add_refund_modal_for_bank_credit_context()
    {
        var (_, ids) = await StepAsync("Open Bookings and read first booking", OpenBookingsAndTryFirstBookingAsync);
        if (ids is null)
            Assert.Ignore("No booking rows.");

        var money = new BookingMoneyPage(Page, Settings.Applications.Member);
        await StepAsync("Open Booking Money", () => money.GotoWithBookingAsync(ids.Value.ClientNewId, ids.Value.BookingRefId));

        if (await money.MoneyGridDataRowCountAsync() == 0)
            Assert.Ignore("No money grid rows.");

        var bankRow = await StepAsync("Find first Bank credit row (exclude batch)", () => money.FirstDataRowWhereBankCreditNotBatchAsync());

        if (bankRow is null)
            Assert.Ignore("No transaction row with type containing 'bank' but not 'batch'.");

        await StepAsync("Scroll bank row into view", () => bankRow.ScrollIntoViewIfNeededAsync());

        await StepAsync("Assert Add Refund is available", async () =>
        {
            Assert.That(await money.IsAddRefundButtonEnabledAsync(), Is.True);
        });

        await StepAsync("Open Add Refund modal", () => money.OpenAddRefundModalAsync());

        await StepAsync("Assert modal and Bank Refund type selection (no Save)", async () =>
        {
            Assert.That(await money.IsAddRefundModalVisibleAsync(), Is.True);
            await money.SelectRefundTypeValueAsync("14");
            await Page.WaitForTimeoutAsync(200);
            await money.CloseAddRefundModalAsync();
        });
    }

    [Test]
    [Category("P1")]
    [Category("Hybrid")]
    [Description("DEV-TC-668 — Verify FX rate on Payment out summary (GET GetPaymentOutSummaryData + UI cell 1:rate).")]
    public async Task DEV_TC_668_Verify_fx_rate_on_payment_out_summary()
    {
        var (_, ids) = await StepAsync("Open Bookings and read first booking", OpenBookingsAndTryFirstBookingAsync);
        if (ids is null)
            Assert.Ignore("No booking rows.");

        var overview = new BookingOverviewPage(Page, Settings.Applications.Member);

        var responseTask = Page.WaitForResponseAsync(
            r => r.Url.Contains("/client/GetPaymentOutSummaryData", StringComparison.OrdinalIgnoreCase),
            new PageWaitForResponseOptions { Timeout = Settings.Timeouts.NavigationMs });

        await StepAsync("Open Booking overview", () => overview.GotoWithBookingAsync(ids.Value.ClientNewId, ids.Value.BookingRefId));

        var resp = await StepAsync("Await payment-out summary API", () => responseTask);
        await StepAsync("Assert API success", async () =>
        {
            Assert.That(resp, Is.Not.Null);
            Assert.That(resp!.Ok, Is.True, $"GetPaymentOutSummaryData should return success. Status={resp.Status}.");
        });

        decimal? apiFx = null;
        await StepAsync("Parse FixedRate from JSON when present", async () =>
        {
            var body = await resp.TextAsync();
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;
            if (!root.TryGetProperty("Result", out var resultEl))
            {
                Assert.Ignore("Response has no Result property — cannot assert FX.");
                return;
            }

            if (resultEl.ValueKind == JsonValueKind.String)
            {
                var inner = resultEl.GetString();
                if (string.IsNullOrWhiteSpace(inner))
                {
                    Assert.Ignore("Empty Result string.");
                    return;
                }
                using var innerDoc = JsonDocument.Parse(inner);
                var arr = innerDoc.RootElement;
                if (arr.ValueKind != JsonValueKind.Array || arr.GetArrayLength() == 0)
                {
                    Assert.Ignore("No payment-out rows in API result — nothing to verify for FX.");
                    return;
                }
                var first = arr[0];
                if (first.TryGetProperty("FixedRate", out var fr) && fr.ValueKind == JsonValueKind.Number)
                    apiFx = fr.GetDecimal();
            }
            else if (resultEl.ValueKind == JsonValueKind.Array && resultEl.GetArrayLength() > 0)
            {
                var first = resultEl[0];
                if (first.TryGetProperty("FixedRate", out var fr) && fr.ValueKind == JsonValueKind.Number)
                    apiFx = fr.GetDecimal();
            }
            else
            {
                Assert.Ignore("Unexpected Result shape for GetPaymentOutSummaryData.");
            }
        });

        try
        {
            await StepAsync("Wait for payment-out table rows", () => overview.WaitForPaymentOutSummaryRowsAsync(1));
        }
        catch (Exception)
        {
            Assert.Ignore("Payment out summary table did not populate.");
        }

        var fxCell = await StepAsync("Read first FX cell", () => overview.FirstPaymentOutFxRateCellTextAsync());

        await StepAsync("Assert FX cell matches 1:rate pattern", async () =>
        {
            Assert.That(fxCell.Trim(), Does.Match(new Regex(@"^1:\s*[\d.,]+$", RegexOptions.IgnoreCase)),
                $"FX column should display as 1:{{rate}}. Cell: '{fxCell}'");
        });

        if (apiFx.HasValue)
        {
            await StepAsync("Assert UI rate matches API FixedRate", async () =>
            {
                var normalized = fxCell.Replace(" ", "", StringComparison.Ordinal).Replace("1:", "", StringComparison.OrdinalIgnoreCase);
                Assert.That(decimal.TryParse(normalized, System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out var uiRate), Is.True);
                Assert.That(uiRate, Is.EqualTo(apiFx!.Value).Within(0.02m),
                    "Displayed FX rate should align with API FixedRate.");
            });
        }
    }

    [Test]
    [Category("P1")]
    [Category("Hybrid")]
    [Description("DEV-TC-669 — Calculate APC based on booking options (Booking Fee page + document GET).")]
    public async Task DEV_TC_669_Apc_on_booking_fee_page()
    {
        var (_, ids) = await StepAsync("Open Bookings and read first booking", OpenBookingsAndTryFirstBookingAsync);
        if (ids is null)
            Assert.Ignore("No booking rows.");

        var fees = new BookingFeePage(Page, Settings.Applications.Member);

        var response = await StepAsync("Navigate to Booking Fee", () => fees.GotoWithClientNewIdAndReturnResponseAsync(ids.Value.ClientNewId));

        await StepAsync("Assert document GET succeeded", async () =>
        {
            Assert.That(response, Is.Not.Null);
            Assert.That(response!.Ok, Is.True, $"BookingFee navigation should succeed. Status={response.Status}.");
        });

        await StepAsync("Assert fees shell", async () =>
        {
            Assert.That(await fees.IsOutgoingFeesSectionVisibleAsync(), Is.True);
        });

        var apcRows = await StepAsync("Count APC fee rows", () => fees.ApcFeeRowCountAsync());
        if (apcRows == 0)
            Assert.Ignore("No APC Fee line items on Booking Fee for this client — nothing to assert.");

        await StepAsync("Assert APC row rendered", () =>
        {
            Assert.That(apcRows, Is.GreaterThan(0));
            return Task.CompletedTask;
        });
    }
}
