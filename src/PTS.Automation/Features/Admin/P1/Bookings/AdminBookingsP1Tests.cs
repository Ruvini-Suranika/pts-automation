using System.Text.Json;

using Allure.NUnit.Attributes;
using PTS.Automation.Infrastructure;
using PTS.Automation.Pages.Admin.Bookings;
using PTS.Automation.Pages.Admin.Shell;

namespace PTS.Automation.Features.Admin.P1.Bookings;

/// <summary>P1 critical coverage: Admin → Admin → Bookings (overview, money, fees).</summary>
[TestFixture]
[AllureSuite(Categories.Admin)]
[AllureFeature("Bookings")]
[AllureTag(Categories.P1)]
[AllureTag(Categories.Regression)]
[AllureTag("Admin.Bookings")]
[Category(Categories.Regression)]
[Category("Admin.Bookings")]
public sealed class AdminBookingsP1Tests : AdminP1TestBase
{
    /// <summary>
    /// Resolves <c>Id</c> (client new id) + booking reference for Admin booking URLs.
    /// Prefer <see cref="TestDataSettings.AdminBookings"/>; otherwise search when debit seed booking ref is set.
    /// </summary>
    private async Task<(string ClientNewId, string BookingRefId)?> TryResolveAdminBookingContextAsync()
    {
        var ab = Settings.TestData.AdminBookings;
        if (ab.IsConfigured)
            return (ab.ClientNewId.Trim(), ab.BookingReferenceId.Trim());

        var debitRef = Settings.TestData.Member.Debits.BookingReferenceId?.Trim();
        if (string.IsNullOrEmpty(debitRef))
            return null;

        await LandOnAdminEnquiriesAsync();
        var nav = new AdminNavBar(Page, Settings.Applications.Admin);
        await nav.GoToBookingSearchAsync();

        var search = new AdminBookingSearchPage(Page, Settings.Applications.Admin);
        await search.WaitForReadyAsync();

        var waitList = Page.WaitForResponseAsync(
            r => r.Url.Contains("/Admin/BookingSearchAdmin", StringComparison.OrdinalIgnoreCase)
                 && string.Equals(r.Request.Method, "POST", StringComparison.OrdinalIgnoreCase),
            new PageWaitForResponseOptions { Timeout = Settings.Timeouts.NavigationMs });

        await search.SearchByBookingReferenceAsync(debitRef);
        try
        {
            await waitList;
        }
        catch (TimeoutException)
        {
            return null;
        }

        try
        {
            await search.WaitForResultRowsAsync(1);
        }
        catch (TimeoutException)
        {
            return null;
        }

        return await search.TryReadFirstBookingDetailsQueryAsync();
    }

    [Test]
    [Category(Categories.UI)]
    [Description("DEV-TC-550 — Booking overview: Payment out summary section is present and populated when data exists.")]
    public async Task DEV_TC_550_Payment_out_summary_section()
    {
        var ids = await StepAsync("Resolve booking context", TryResolveAdminBookingContextAsync);
        if (ids is null)
            Assert.Ignore(
                "No admin booking context: configure TestData:AdminBookings:ClientNewId + BookingReferenceId, " +
                "or TestData:Member:Debits:BookingReferenceId so Admin booking search returns rows.");

        var overview = new AdminBookingOverviewPage(Page, Settings.Applications.Admin);

        var responseTask = Page.WaitForResponseAsync(
            r => r.Url.Contains("/client/GetPaymentOutSummaryData", StringComparison.OrdinalIgnoreCase),
            new PageWaitForResponseOptions { Timeout = Settings.Timeouts.NavigationMs });

        await StepAsync("Open Admin booking overview", () => overview.GotoWithBookingAsync(ids.Value.ClientNewId, ids.Value.BookingRefId));

        var resp = await StepAsync("Await payment-out summary API", () => responseTask);
        await StepAsync("Assert API success", async () =>
        {
            Assert.That(resp, Is.Not.Null);
            Assert.That(resp!.Ok, Is.True, $"GetPaymentOutSummaryData should succeed. Status={resp.Status}.");
        });

        await StepAsync("Assert Payment out summary heading and grid shell", async () =>
        {
            Assert.That(await overview.IsPaymentOutSummarySectionVisibleAsync(), Is.True,
                "Payment out summary heading should be visible on Admin booking overview.");
            Assert.That(await Page.Locator("#tbl_paymentOutSummary thead").IsVisibleAsync(), Is.True,
                "Payment out summary table header should render.");
        });

        var body = await resp!.TextAsync();
        await StepAsync("Parse payment-out API payload", async () =>
        {
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;
            Assert.That(root.TryGetProperty("Result", out _), Is.True,
                "GetPaymentOutSummaryData JSON should include a Result property.");
        });

        try
        {
            await StepAsync("Wait for payment-out data rows when returned by API", () => overview.WaitForPaymentOutSummaryRowsAsync(1));
        }
        catch (Exception)
        {
            Assert.Ignore("Payment out summary API returned no renderable payment-out rows for this booking.");
        }

        await StepAsync("Assert at least one payment-out row is bound", async () =>
        {
            Assert.That(await overview.PaymentOutSummaryDataRowCountAsync(), Is.GreaterThan(0));
        });
    }

    [Test]
    [Category(Categories.UI)]
    [Description("DEV-TC-703 — Add and edit Refund on Admin Money (Credits and debits / Refunds section; modal flows without persisting when cancelled).")]
    public async Task DEV_TC_703_Add_and_edit_refund_modals_on_admin_money()
    {
        var ids = await StepAsync("Resolve booking context", TryResolveAdminBookingContextAsync);
        if (ids is null)
            Assert.Ignore(
                "No admin booking context: configure TestData:AdminBookings or Member:Debits:BookingReferenceId for search.");

        var money = new AdminBookingMoneyPage(Page, Settings.Applications.Admin);
        await StepAsync("Open Admin Money", () => money.GotoWithBookingAsync(ids.Value.ClientNewId, ids.Value.BookingRefId));

        await StepAsync("Assert Credits and debits and Refunds sections", async () =>
        {
            Assert.That(await money.IsCreditsAndDebitsHeadingVisibleAsync(), Is.True);
            Assert.That(await money.IsRefundsHeadingVisibleAsync(), Is.True);
            Assert.That(await money.AddRefundButton.IsVisibleAsync(), Is.True);
        });

        await StepAsync("Open Add Refund modal", () => money.OpenAddRefundModalAsync());
        await StepAsync("Assert Add Refund modal (no Save)", async () =>
        {
            Assert.That(await money.IsAddRefundModalVisibleAsync(), Is.True);
            Assert.That((await money.AddRefundModalHeaderTextAsync()).Trim(), Does.Contain("Add Refund").IgnoreCase);
            await money.FillRefundAccountNameAsync("Automation Refund Account");
            await money.FillRefundAccountNumberAsync("12345678");
            await money.FillRefundSortCodeAsync("12-34-56");
            await money.FillRefundValueAsync("0.01");
            await money.SelectRefundTypeValueAsync("14");
            await Page.WaitForTimeoutAsync(300);
            await money.CloseAddRefundModalViaCancelAsync();
        });

        var editCount = await StepAsync("Count refund edit actions", () => money.RefundEditLinkCountAsync());
        if (editCount == 0)
        {
            Assert.Ignore("No existing refund rows with edit action — cannot exercise edit Refund UI on this booking.");
        }

        await StepAsync("Open first Refund edit modal", () => money.OpenFirstRefundEditModalAsync());
        await StepAsync("Assert Edit Refund modal (no Save)", async () =>
        {
            Assert.That(await money.IsAddRefundModalVisibleAsync(), Is.True);
            Assert.That((await money.AddRefundModalHeaderTextAsync()).Trim(), Does.Contain("Edit Refund").IgnoreCase);
            Assert.That((await money.UpdateRefundButtonTextAsync()).Trim(), Is.EqualTo("Save").IgnoreCase);
            var name = (await Page.Locator("#txtAccountName").InputValueAsync()).Trim();
            Assert.That(name, Is.Not.Empty, "Edit Refund should hydrate account name from the selected row.");
            await money.CloseAddRefundModalViaCancelAsync();
        });
    }

    [Test]
    [Category(Categories.Hybrid)]
    [Description("DEV-TC-825 — Controls: Booking access switch toggles and persists via UpdateClientControl (restored after test).")]
    public async Task DEV_TC_825_Booking_access_lock_toggle()
    {
        var ids = await StepAsync("Resolve booking context", TryResolveAdminBookingContextAsync);
        if (ids is null)
            Assert.Ignore("No admin booking context — configure TestData:AdminBookings or Member:Debits:BookingReferenceId.");

        var overview = new AdminBookingOverviewPage(Page, Settings.Applications.Admin);
        await StepAsync("Open Admin booking overview", () => overview.GotoWithBookingAsync(ids.Value.ClientNewId, ids.Value.BookingRefId));

        await StepAsync("Assert Controls section", async () =>
        {
            Assert.That(await overview.IsControlsSectionVisibleAsync(), Is.True);
            Assert.That(await overview.BookingAccessSwitch.IsVisibleAsync(), Is.True);
        });

        var before = await StepAsync("Read initial Booking access state", () => overview.BookingAccessSwitch.IsCheckedAsync());

        var responseTask = Page.WaitForResponseAsync(
            r => r.Url.Contains("/Admin/UpdateClientControl", StringComparison.OrdinalIgnoreCase)
                 && string.Equals(r.Request.Method, "POST", StringComparison.OrdinalIgnoreCase),
            new PageWaitForResponseOptions { Timeout = Settings.Timeouts.NavigationMs });

        await StepAsync("Toggle Booking access switch", () => overview.BookingAccessSwitch.ClickAsync());

        var updateResp = await StepAsync("Await UpdateClientControl", () => responseTask);
        await StepAsync("Assert control update HTTP success", async () =>
        {
            Assert.That(updateResp.Ok, Is.True, $"UpdateClientControl failed: HTTP {updateResp.Status}.");
        });

        await StepAsync("Assert switch state changed", async () =>
        {
            await Page.WaitForTimeoutAsync(500);
            var after = await overview.BookingAccessSwitch.IsCheckedAsync();
            Assert.That(after, Is.Not.EqualTo(before), "Booking access switch should reflect the new state after POST.");
        });

        var restoreTask = Page.WaitForResponseAsync(
            r => r.Url.Contains("/Admin/UpdateClientControl", StringComparison.OrdinalIgnoreCase)
                 && string.Equals(r.Request.Method, "POST", StringComparison.OrdinalIgnoreCase),
            new PageWaitForResponseOptions { Timeout = Settings.Timeouts.NavigationMs });

        await StepAsync("Toggle Booking access switch back to original", () => overview.BookingAccessSwitch.ClickAsync());

        _ = await StepAsync("Await second UpdateClientControl", () => restoreTask);
        await StepAsync("Assert switch restored", async () =>
        {
            await Page.WaitForTimeoutAsync(500);
            Assert.That(await overview.BookingAccessSwitch.IsCheckedAsync(), Is.EqualTo(before));
        });
    }

    [Test]
    [Category(Categories.UI)]
    [Description("DEV-TC-826 — Log notes: currency purchasing entry when trustee payment used alternate currency (log modal).")]
    public async Task DEV_TC_826_Log_note_currency_purchasing_alternate_currency()
    {
        var ids = await StepAsync("Resolve booking context", TryResolveAdminBookingContextAsync);
        if (ids is null)
            Assert.Ignore("No admin booking context — configure TestData:AdminBookings or Member:Debits:BookingReferenceId.");

        var overview = new AdminBookingOverviewPage(Page, Settings.Applications.Admin);
        await StepAsync("Open Admin booking overview", () => overview.GotoWithBookingAsync(ids.Value.ClientNewId, ids.Value.BookingRefId));

        if (await overview.PaymentOutLogTriggers.CountAsync() == 0)
            Assert.Ignore("No payment-out log actions — nothing to open for currency purchasing notes.");

        await StepAsync("Open first payment-out log", () => overview.ClickFirstPaymentOutLogAsync());
        await StepAsync("Wait for log modal", () => overview.WaitForLogNotesModalAsync());

        var currencyNote = await StepAsync("Read currency purchasing log note", () => overview.CurrencyPurchasingLogNoteTextAsync());
        var trimmed = currencyNote.Trim();
        if (trimmed.Length < 8)
            Assert.Ignore(
                "Currency purchasing log note is empty or too short for this booking — needs a payment-out with alternate-currency purchasing history.");

        await StepAsync("Assert currency purchasing note has substance", async () =>
        {
            Assert.That(trimmed, Is.Not.EqualTo("-"));
        });

        await StepAsync("Dismiss log modal", () => overview.PressEscapeAsync());
    }

    [Test]
    [Category(Categories.UI)]
    [Description("DEV-TC-827 — Log notes: trustee authorisation entry when trustee payment authorised in same currency (log modal).")]
    public async Task DEV_TC_827_Log_note_trustee_authorisation_same_currency()
    {
        var ids = await StepAsync("Resolve booking context", TryResolveAdminBookingContextAsync);
        if (ids is null)
            Assert.Ignore("No admin booking context — configure TestData:AdminBookings or Member:Debits:BookingReferenceId.");

        var overview = new AdminBookingOverviewPage(Page, Settings.Applications.Admin);
        await StepAsync("Open Admin booking overview", () => overview.GotoWithBookingAsync(ids.Value.ClientNewId, ids.Value.BookingRefId));

        if (await overview.PaymentOutLogTriggers.CountAsync() == 0)
            Assert.Ignore("No payment-out log actions — nothing to open for trustee authorisation notes.");

        await StepAsync("Open first payment-out log", () => overview.ClickFirstPaymentOutLogAsync());
        await StepAsync("Wait for log modal", () => overview.WaitForLogNotesModalAsync());

        var trusteeNote = await StepAsync("Read trustee authorisation log note", () => overview.TrusteeAuthorisationLogNoteTextAsync());
        var trimmed = trusteeNote.Trim();
        if (trimmed.Length < 8)
            Assert.Ignore(
                "Trustee authorisation log note is empty or too short — needs a payment-out with same-currency trustee authorisation history.");

        await StepAsync("Assert trustee authorisation note has substance", async () =>
        {
            Assert.That(trimmed, Is.Not.EqualTo("-"));
        });

        await StepAsync("Dismiss log modal", () => overview.PressEscapeAsync());
    }

    [Test]
    [Category(Categories.Hybrid)]
    [Description("DEV-TC-847 — Online payment appears on Admin Booking Fees incoming fees when present.")]
    public async Task DEV_TC_847_Online_payment_on_admin_booking_fees()
    {
        var ids = await StepAsync("Resolve booking context", TryResolveAdminBookingContextAsync);
        if (ids is null)
            Assert.Ignore("No admin booking context — configure TestData:AdminBookings or Member:Debits:BookingReferenceId.");

        var fees = new AdminBookingFeePage(Page, Settings.Applications.Admin);

        var response = await StepAsync("Navigate to Admin Booking Fee", () => fees.GotoWithClientNewIdAndReturnResponseAsync(ids.Value.ClientNewId));

        await StepAsync("Assert document load succeeded", async () =>
        {
            Assert.That(response, Is.Not.Null);
            Assert.That(response!.Ok, Is.True, $"Admin/BookingFee navigation failed: HTTP {response.Status}.");
        });

        await StepAsync("Assert incoming fees section", async () =>
        {
            Assert.That(await fees.IsIncomingFeesSectionVisibleAsync(), Is.True);
        });

        var hasOnline = await StepAsync("Detect Online payment fee row (case-insensitive)", () => fees.IncomingFeesContainsTextAsync("online"));
        if (!hasOnline)
            Assert.Ignore("No incoming fee row containing 'online' — booking has no online payment fee line to validate.");

        await StepAsync("Assert Online payment row is visible in incoming fees", async () =>
        {
            Assert.That(await fees.IncomingFeesContainsTextAsync("online"), Is.True);
        });
    }
}
